using LexiCore.Abstractions;
using LexiCore.Facts;
using LexiCore.Syntax;

namespace LexiCore.Lexing.Lexers;

/// <summary>
/// Base implementation for lexers that recognize delimited tokens.
/// </summary>
/// <remarks>
/// This lexer pattern is used for token types that are defined by
/// opening and closing delimiters (e.g. "{...}", "[...]", custom markers).
///
/// <para>
/// The base class handles fast pre-filtering via <see cref="StartChar"/>
/// and basic delimiter detection before delegating parsing logic to <see cref="Lex"/>.
/// </para>
///
/// <para>
/// Derived implementations are responsible only for token construction,
/// while delimiter validation and dispatch logic are handled by the base class.
/// </para>
/// </remarks>
public abstract class DelimitedTokenLexerBase<T> : ITokenLexer, IOrderedLexer
    where T : ILexicalToken
{
    /// <summary>
    /// Gets the opening terminal symbol that initiates this token.
    /// </summary>
    protected abstract SyntaxTerminal TokenOpen { get; }

    /// <summary>
    /// Gets the closing terminal symbol that terminates this token.
    /// </summary>
    protected abstract SyntaxTerminal TokenClose { get; }

    /// <inheritdoc/>
    public char StartChar => TokenOpen.FirstChar;
    /// <inheritdoc/>
    int IOrderedLexer.Priority => TokenOpen.Length;

    // Contract split:
    // - LexToken may return null (no match);
    // - Lex assumes a valid prefix and performs actual token parsing.

    /// <inheritdoc/>
    public virtual (ILexicalToken? token, int nextPosition)? LexToken(
        ReadOnlyMemory<char> source,
        ReadOnlySpan<char> sliced,
        int tokenStart,
        LexerContext context
    )
    {
        if (!IsTokenStart(sliced))
        {
            return null;
        }
        return Lex(source, sliced, tokenStart, context);
    }

    /// <summary>
    /// Performs token parsing after the initial dispatch in <see cref="LexToken"/>.
    /// </summary>
    /// <param name="source">Full source buffer.</param>
    /// <param name="sliced">Slice of the source starting at <paramref name="tokenStart"/>.</param>
    /// <param name="tokenStart">Absolute start index of the token in the source.</param>
    /// <param name="context">The current lexing context containing runtime flags.</param>
    /// <remarks>
    /// Implementations must assume that the input starts with a valid token prefix for this lexer
    /// and should focus solely on full token construction.
    /// </remarks>
    protected abstract (T? token, int nextPosition) Lex(
        ReadOnlyMemory<char> source,
        ReadOnlySpan<char> sliced,
        int tokenStart,
        LexerContext context
    );

    /// <summary>
    /// Determines if the provided span begins with the <see cref="TokenOpen"/> symbol.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsTokenStart(ReadOnlySpan<char> span, int startTokenOpen = 0)
        => SyntaxFacts.IsTokenSymbol(span, startTokenOpen, TokenOpen);

    /// <summary>
    /// Determines if the provided span begins with the <see cref="TokenClose"/> symbol.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsTokenEnd(ReadOnlySpan<char> span, int startTokenClose = 0)
        => SyntaxFacts.IsTokenSymbol(span, startTokenClose, TokenClose);
}
