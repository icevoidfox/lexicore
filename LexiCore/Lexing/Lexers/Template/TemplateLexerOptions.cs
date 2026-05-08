namespace LexiCore.Lexing.Lexers;

public sealed record TemplateLexerOptions(
    int MaxTokenLength = 512,
    int MaxIdentifierLength = 64,
    int MaxValueLength = 256,
    TemplateTolerance Tolerance = TemplateTolerance.AllowLongTokens
)
{
    public static TemplateLexerOptions Default { get; } = new();

    /// <summary>
    /// Checks whether the specified tolerance flag is enabled.
    /// Uses bitwise comparison for performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Allows(TemplateTolerance flag) => (Tolerance & flag) != 0;
}
