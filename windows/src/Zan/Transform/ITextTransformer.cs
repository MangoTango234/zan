namespace Zan.Transform;

/// <summary>
/// Runs an instruction (the action's prompt) over some input text and returns
/// the model's result. Implemented per provider. Mirrors the macOS TextTransformer.
/// </summary>
internal interface ITextTransformer
{
    Task<string> TransformAsync(string instruction, string input, CancellationToken ct);
}
