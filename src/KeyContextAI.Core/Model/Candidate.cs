namespace KeyContextAI.Core.Model;

/// <summary>
/// One interpretation of a run of scan codes: the text they would produce under a particular layout.
/// </summary>
/// <param name="Layout">The layout this interpretation assumes.</param>
/// <param name="Text">The text the scan codes produce under that layout.</param>
/// <param name="IsComplete">False when at least one scan code has no mapping in this layout, which
/// makes the candidate unusable for a correction.</param>
/// <param name="IsAsTyped">True for the candidate representing what the user actually saw, so the
/// detection engine can compare against it rather than assuming the first entry.</param>
public sealed record Candidate(
    LayoutId Layout,
    string Text,
    bool IsComplete,
    bool IsAsTyped);
