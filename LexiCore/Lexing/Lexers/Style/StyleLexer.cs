using LexiCore.Syntax;
using LexiCore.Tokens.Syntax;

namespace LexiCore.Lexing.Lexers;

public sealed class StyleLexer : DelimitedTokenLexerBase<StyleToken>
{
    /// <inheritdoc/>
    protected override SyntaxTerminal TokenOpen => StyleSyntax.TokenOpen;
    /// <inheritdoc/>
    protected override SyntaxTerminal TokenClose => StyleSyntax.TokenClose;

    /// <inheritdoc/>
    protected override (StyleToken token, int nextPosition) Lex(
        ReadOnlyMemory<char> source,
        ReadOnlySpan<char> sliced,
        int tokenStart,
        LexerContext context
    )
    {
        throw new NotImplementedException();
    }
}
