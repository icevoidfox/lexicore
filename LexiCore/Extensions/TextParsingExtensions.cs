using LexiCore.Facts;
using LexiCore.Syntax;

namespace LexiCore.Extensions;

/// <summary>
/// Provides low-level string parsing utilities for extracting delimited content.
/// </summary>
/// <remarks>
/// This helper originated in lexer-related code but is now used as a general-purpose
/// utility for parsing text without requiring a full lexer.
/// 
/// <para>
/// Supports nested delimiters and escape-aware parsing.
/// </para>
/// 
/// <para>
/// This API is intended to remain stable and is not expected to change frequently.
/// </para>
/// </remarks>
public static class TextParsingExtensions
{
    public static (string content, int closeDelimiterIndex)? ExtractDelimitedContent(
        this string text,
        char openDelimiter,
        char closeDelimiter,
        int startIndex = 0,
        char? escapeChar = SharedSyntax.EscapeChar
    )
    {
        if (text.AsSpan(startIndex).TryExtractDelimitedContent(
            openDelimiter,
            closeDelimiter,
            out var content,
            out int endIndex,
            escapeChar
        ))
        {
            return (content.ToString(), startIndex + endIndex);
        }
        return null;
    }

    public static (string content, int closeDelimiterIndex)? ExtractDelimitedContent(
        this string text,
        string openDelimiter,
        string closeDelimiter,
        int startIndex = 0,
        char? escapeChar = SharedSyntax.EscapeChar
    )
    {
        if (text.AsSpan(startIndex).TryExtractDelimitedContent(
            openDelimiter,
            closeDelimiter,
            out var content,
            out int endIndex,
            escapeChar
        ))
        {
            return (content.ToString(), startIndex + endIndex);
        }
        return null;
    }

    public static bool TryExtractDelimitedContent(
        this ReadOnlySpan<char> text,
        char openDelimiter,
        char closeDelimiter,
        out ReadOnlySpan<char> content,
        out int closeDelimiterIndex,
        char? escapeChar = SharedSyntax.EscapeChar
    )
    {
        content = default;
        closeDelimiterIndex = -1;

        int openIndex = text.IndexOf(openDelimiter);
        if (openIndex < 0)
        {
            return false;
        }

        closeDelimiterIndex = IndexOfClosingDelimiter(text, openDelimiter, closeDelimiter, openIndex, escapeChar);
        if (closeDelimiterIndex < 0)
        {
            return false;
        }

        content = text.Slice(openIndex + 1, closeDelimiterIndex - openIndex - 1);
        return true;
    }

    public static bool TryExtractDelimitedContent(
        this ReadOnlySpan<char> text,
        ReadOnlySpan<char> openDelimiter,
        ReadOnlySpan<char> closeDelimiter,
        out ReadOnlySpan<char> content,
        out int closeDelimiterIndex,
        char? escapeChar = SharedSyntax.EscapeChar
    )
    {
        ValidateSpanDelimiter(openDelimiter, nameof(openDelimiter));
        ValidateSpanDelimiter(closeDelimiter, nameof(closeDelimiter));

        content = default;
        closeDelimiterIndex = -1;

        int openIndex = text.IndexOf(openDelimiter);
        if (openIndex < 0)
        {
            return false;
        }

        closeDelimiterIndex = IndexOfClosingDelimiter(text, openDelimiter, closeDelimiter, openIndex, escapeChar);
        if (closeDelimiterIndex < 0)
        {
            return false;
        }

        content = text.Slice(openIndex + openDelimiter.Length, closeDelimiterIndex - openIndex - openDelimiter.Length);
        return true;
    }

    private static int IndexOfClosingDelimiter(
        ReadOnlySpan<char> text,
        char openDelimiter,
        char closeDelimiter,
        int startIndex,
        char? escapeChar
    )
    {
        int balance = 1;

        for (int position = startIndex + 1; position < text.Length; position++)
        {
            bool isEscaped = IsEscaped(text, position, escapeChar);
            if (!isEscaped && text[position] == closeDelimiter)
            {
                balance--;
                if (balance == 0)
                {
                    return position;
                }
            }
            else if (!isEscaped && text[position] == openDelimiter)
            {
                balance++;
            }
        }

        return -1;
    }

    private static int IndexOfClosingDelimiter(
        ReadOnlySpan<char> text,
        ReadOnlySpan<char> openDelimiter,
        ReadOnlySpan<char> closeDelimiter,
        int startIndex,
        char? escapeChar
    )
    {
        int balance = 1;
        int position = startIndex + openDelimiter.Length;

        while (position < text.Length)
        {
            ReadOnlySpan<char> sliced = text[position..];

            int nextClose = sliced.IndexOf(closeDelimiter);
            if (nextClose < 0)
            {
                return -1;
            }
            nextClose += position;

            int nextOpen = sliced.IndexOf(openDelimiter);
            nextOpen = nextOpen >= 0 ? nextOpen + position : int.MaxValue;
            
            bool isOpenFirst = nextOpen < nextClose;

            int nextIndex = isOpenFirst ? nextOpen : nextClose;
            if (IsEscaped(text, nextIndex, escapeChar))
            {
                position = nextIndex + 1;
                continue;
            }

            if (isOpenFirst)
            {
                balance++;
                position = nextOpen + openDelimiter.Length;
            }
            else
            {
                balance--;
                if (balance == 0)
                {
                    return nextClose;
                }
                position = nextClose + closeDelimiter.Length;
            }
        }

        return -1;
    }
    
    private static void ValidateSpanDelimiter(ReadOnlySpan<char> delimiter, string delimiterName)
    {
        if (delimiter.Length == 0)
        {
            throw new ArgumentException("String delimiter cannot be empty.", delimiterName);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEscaped(
        ReadOnlySpan<char> text,
        int index,
        char? escapeChar
    ) => escapeChar.HasValue && SyntaxFacts.IsEscaped(text, index, escapeChar.Value);
}
