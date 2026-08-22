namespace KeyContextAI.Platform.Input;

/// <summary>Native extra-info tags shared by the input accessors.</summary>
internal static class NativeInputTags
{
    /// <summary>
    /// The extra-info marker used for self-injected input so the hook can ignore it.
    /// </summary>
    internal const nuint SelfInjectionTag = 0x4B435458u;
}
