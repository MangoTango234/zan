using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Zan.Transform;

/// <summary>
/// Anthropic Messages text engine. The action prompt is the system parameter;
/// the selected text is the single user message. Anthropic has no speech-to-text,
/// so it is a text engine only.
/// </summary>
internal sealed class AnthropicTextTransformer : ITextTransformer
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

    private readonly string _apiKey;
    private readonly string _model;

    public AnthropicTextTransformer(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> TransformAsync(string instruction, string input, CancellationToken ct)
    {
        var payload = new
        {
            model = _model,
            max_tokens = 2048,
            system = instruction,
            messages = new object[]
            {
                new { role = "user", content = input },
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = JsonContent.Create(payload);

        using var response = await Http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Anthropic error {(int)response.StatusCode}: {Truncate(body)}");

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();
        return content?.Trim() ?? string.Empty;
    }

    private static string Truncate(string s) => s.Length > 300 ? s[..300] : s;
}
