namespace LexiCore.Lexing;

/// <summary>
/// Provides contextual information and shared state for a single lexing session.
/// </summary>
/// <remarks>
/// This is a <see langword="ref struct"/> to ensure it is only ever allocated on the stack,
/// maintaining zero-allocation performance during the tokenization process.
/// </remarks>
public readonly ref struct LexerContext(
    TokenBufferConfig? bufferConfig = null,
    bool enableDiagnostics = true
)
{
    /// <summary>
    /// Gets a default context with standard buffer configuration and diagnostics enabled.
    /// </summary>
    public static LexerContext Default => new();

    /// <summary>
    /// Configuration for estimating the initial capacity of a list to minimize its size change during token collection.
    /// </summary>
    public readonly TokenBufferConfig BufferConfig = bufferConfig ?? TokenBufferConfig.Default;

    /// <summary>
    /// Indicates whether diagnostics should be collected during lexing.
    /// If <see langword="false"/>, lexers should skip diagnostic production for better performance.
    /// </summary>
    public readonly bool EnableDiagnostics = enableDiagnostics;

    /// <summary>
    /// Gets a value indicating whether the context is uninitialized (default).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the structure was created using <see langword="default"/>
    /// without calling a constructor; otherwise, <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(BufferConfig))]
    public bool IsEmpty => BufferConfig is null;
}
