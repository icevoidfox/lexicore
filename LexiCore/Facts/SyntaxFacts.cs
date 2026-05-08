using LexiCore.Syntax;

namespace LexiCore.Facts;

internal static class SyntaxFacts
{
    public static bool IsEscaped(ReadOnlySpan<char> span, int index, char escapeChar = SharedSyntax.EscapeChar)
    {
        int count = 0;
        for (int i = index - 1; i >= 0 && span[i] == escapeChar; i--)
        {
            count++;
        }
        return (count & 1) != 0;
    }

    public static bool IsTokenSymbol(ReadOnlySpan<char> span, int startTokenSymbol, SyntaxTerminal tokenSymbol)
    {
        if (tokenSymbol.IsSentinel || startTokenSymbol + tokenSymbol.Length > span.Length)
        {
            return false;
        }

        if (tokenSymbol.IsChar)
        {
            return span[startTokenSymbol] == tokenSymbol.FirstChar;
        }
        return span.Slice(startTokenSymbol, tokenSymbol.Length).SequenceEqual(tokenSymbol);
    }
}
