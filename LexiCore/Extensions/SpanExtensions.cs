using LexiCore.Syntax;

namespace LexiCore.Extensions;

/// <summary>
/// Provides low-level utilities for processing and transforming text spans.
/// </summary>
/// <remarks>
/// Includes allocation-aware helpers for lightweight parsing operations.
/// </remarks>
public static class SpanExtensions
{
    /// <summary>
    /// Removes escape characters from the input span.
    /// </summary>
    /// <remarks>
    /// Escape characters are not included in the resulting string.
    /// </remarks>
    public static string Unescape(ReadOnlySpan<char> span, char escapeChar)
    {
        if (span.IsEmpty)
        {
            return string.Empty;
        }

        if (span.IndexOf(escapeChar) == -1)
        {
            return span.ToString();
        }

        char[] arrayFromPool = ArrayPool<char>.Shared.Rent(span.Length);
        try
        {
            int written = 0;
            bool escaping = false;

            foreach (char c in span)
            {
                if (!escaping && c == escapeChar)
                {
                    escaping = true;
                }
                else
                {
                    arrayFromPool[written++] = c;
                    escaping = false;
                }
            }
            return new(arrayFromPool, 0, written);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arrayFromPool);
        }
    }

    public static string Unescape(this ReadOnlySpan<char> span) => Unescape(span, SharedSyntax.EscapeChar);
}
