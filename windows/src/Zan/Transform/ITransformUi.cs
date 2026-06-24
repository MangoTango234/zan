namespace Zan.Transform;

/// <summary>
/// UI surface the controller drives: a transient "working" HUD during the LLM
/// call, a result popup, and brief notifications. Implemented by the app so the
/// controller stays free of WPF specifics.
/// </summary>
internal interface ITransformUi
{
    void ShowWorking(string title);
    void HideWorking();
    void ShowResult(string title, string body);
    void Notify(string message);
}
