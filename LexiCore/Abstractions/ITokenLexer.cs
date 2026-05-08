using LexiCore.Lexing;

namespace LexiCore.Abstractions;

/// <summary>
/// Represents a lexer capable of recognizing a token at a specific position in the input.
/// </summary>
/// <remarks>
/// Lexers are dispatched by their <see cref="StartChar"/> and are invoked only
/// when the input begins with a matching character.
/// </remarks>
public interface ITokenLexer
{
    /// <summary>
    /// Gets the initial character that this lexer can handle.
    /// Used as a fast pre-filter before invoking <see cref="LexToken"/>.
    /// </summary>
    char StartChar { get; }

    /// <summary>
    /// Attempts to lex a token from the given source at the specified position.
    /// </summary>
    /// <param name="source">Full source buffer.</param>
    /// <param name="sliced">Slice of the source starting at <paramref name="tokenStart"/>.</param>
    /// <param name="tokenStart">Absolute start index of the token in the source.</param>
    /// <param name="context">The current lexing context containing runtime flags.</param>
    /// <returns>
    /// A nullable result describing the lexing attempt:
    /// <list type="bullet">
    ///     <item>A tuple where <c>token</c> <see langword="is not null"/>
    ///     <description>— a token was successfully recognized;</description>
    ///     </item>
    ///     <item>A tuple where <c>token</c> <see langword="is null"/>
    ///     <description>— input was consumed but no token is emitted (used by skip/ignore lexers);</description>
    ///     </item>
    ///     <item><see langword="null"/>
    ///     <description>— the lexer does not recognize input at this position.</description>
    ///     </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Implementations must ensure forward progress by either:
    /// <list type="bullet">
    ///     <item>returning a tuple with <c>nextPosition</c> &gt; <paramref name="tokenStart"/>;</item>
    ///     <item>returning <see langword="null"/> to indicate no match, in which case the caller advances the position.</item>
    /// </list>
    /// <c>nextPosition</c> must never be less than or equal to tokenStart when a token is returned.
    /// </remarks>
    (ILexicalToken? token, int nextPosition)? LexToken(
        ReadOnlyMemory<char> source,
        ReadOnlySpan<char> sliced,
        int tokenStart,
        LexerContext context
    );
}
