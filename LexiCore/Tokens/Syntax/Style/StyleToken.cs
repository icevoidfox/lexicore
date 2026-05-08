using LexiCore.Diagnostics;

namespace LexiCore.Tokens.Syntax;

/// <summary>
/// Represents a style token containing a sequence of structural style instructions.
/// </summary>
/// <remarks>
/// A style token is produced by <see cref="Lexing.Lexers.StyleLexer"/>
/// and represents a slice of the input source together with an ordered sequence of
/// <see cref="StyleInstruction"/> instances.
///
/// <para>
/// The <see cref="Instructions"/> collection is a low-level, ordered sequence of structural
/// operations that describe style-related directives extracted from the source.
/// </para>
///
/// <para>
/// The token itself does not interpret or apply styles. It only carries structural data.
/// Interpretation is delegated to a downstream consumer.
/// </para>
/// </remarks>
/// <param name="source">Slice of the lexer input source buffer representing this token.</param>
/// <param name="instructions">Ordered sequence of style instructions extracted from the token.</param>
/// <param name="isValid">Whether the token is syntactically correct and can be processed.</param>
/// <param name="diagnostics">Diagnostic information associated with this token.</param>
public sealed class StyleToken(
    ReadOnlyMemory<char> source,
    ReadOnlyMemory<StyleInstruction> instructions,
    bool isValid,
    ReadOnlyMemory<Diagnostic> diagnostics = default
) : SyntaxTokenBase(source, isValid, diagnostics)
{
    /// <summary>
    /// Gets the ordered sequence of style instructions associated with this token.
    /// </summary>
    /// <remarks>
    /// The instructions represent structural operations on the style state
    /// and are intended to be processed sequentially.
    /// </remarks>
    public ReadOnlyMemory<StyleInstruction> Instructions { get; } = instructions;

    /// <summary>
    /// Gets a value indicating whether this token contains any style instructions.
    /// </summary>
    public bool HasInstruction => !Instructions.IsEmpty;
}
