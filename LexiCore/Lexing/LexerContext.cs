namespace LexiCore.Lexing;

/// <summary>
/// Provides contextual information and shared state for a single lexing session.
/// </summary>
/// <remarks>
/// This is a <see langword="ref struct"/> to ensure it is only ever allocated on the stack,
/// maintaining zero-allocation performance during the tokenization process.
/// </remarks>
public readonly ref struct LexerContext
{
    private readonly TokenBufferConfig? _bufferConfig;
    private readonly bool _diagnosticsDisabled;

    public LexerContext(
        TokenBufferConfig? bufferConfig = null,
        bool enableDiagnostics = true
    ) : this()
    {
        _bufferConfig = bufferConfig;
        _diagnosticsDisabled = !enableDiagnostics;
    }

    /// <summary>
    /// Configuration for estimating the initial capacity of a list to minimize its size change during token collection.
    /// </summary>
    public readonly TokenBufferConfig BufferConfig => _bufferConfig ?? TokenBufferConfig.Default;

    /// <summary>
    /// Indicates whether diagnostics should be collected during lexing.
    /// If <see langword="false"/>, lexers should skip diagnostic production for better performance.
    /// </summary>
    public readonly bool EnableDiagnostics => !_diagnosticsDisabled;
}
