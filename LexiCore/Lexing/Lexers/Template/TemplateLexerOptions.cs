namespace LexiCore.Lexing.Lexers;

/// <summary>
/// Configuration for <see cref="TemplateLexer"/> controlling parsing limits and validation policy.
/// </summary>
/// <remarks>
/// This options object separates three independent concerns:
/// <list type="number">
///     <item><paramref name="MaxTokenLength"/> is a hard parsing limit and always enforced by the lexer.</item>
///     <item>
///     <paramref name="MaxIdentifierLength"/> and <paramref name="MaxValueLength"/> define semantic constraints
///     and may be relaxed via <paramref name="Tolerance"/>.
///     </item>
///     <item>
///     <paramref name="Tolerance"/> controls whether violations are treated as errors or warnings.
///     It does not override physical parsing limits.
///     </item>
/// </list>
/// The lexer always enforces deterministic parsing regardless of tolerance settings.
/// </remarks>
public sealed record TemplateLexerOptions(
    int MaxTokenLength = 512,
    int MaxIdentifierLength = 64,
    int MaxValueLength = 256,
    TemplateTolerance Tolerance = TemplateTolerance.AllowLongValues
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
