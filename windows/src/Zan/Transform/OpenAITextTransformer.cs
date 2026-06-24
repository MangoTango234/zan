using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Zan.Transform;

/// <summary>
/// OpenAI Chat Completions text engine. The action prompt is the system message;
/// the selected text is the user message.
/// </summary>
internal sealed class OpenAITextTransformer : ITextTransformer
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

    private readonly string _apiKey;
    private readonly string _model;

    public OpenAITextTransformer(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> TransformAsync(string instruction, string input, CancellationToken ct)
    {
        var payload = new
        {
            model = _model,
            messages = new object[]
            {
                new { role = "system", content = instruction },
                new { role = "user", content = input },
            },
            temperature = 0.2,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await Http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"OpenAI error {(int)response.StatusCode}: {Truncate(body)}");

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
        return content?.Trim() ?? string.Empty;
    }

    private static string Truncate(string s) => s.Length > 300 ? s[..300] : s;
}
