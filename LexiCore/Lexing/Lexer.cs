using System.Collections.Frozen;

using LexiCore.Abstractions;
using LexiCore.Facts;
using LexiCore.Lexing.Lexers;
using LexiCore.Tokens.Syntax;

namespace LexiCore.Lexing;

/// <summary>
/// High-performance lexer that tokenizes input using a dispatch-based strategy.
/// </summary>
/// <remarks>
/// The lexer scans the input for known token start characters
/// and delegates token recognition to registered <see cref="ITokenLexer"/> implementations.
///
/// <para>
/// Non-token content is emitted as <see cref="TextToken"/> segments.
/// </para>
///
/// <para>
/// The implementation is optimized to minimize unnecessary allocations
/// and avoid per-character dispatch.
/// </para>
/// </remarks>
public sealed class Lexer
{
    public static readonly ITokenLexer[] DefaultLexers = [
        //new StyleLexer(),
        new TemplateLexer(),
    ];
    public static readonly ITokenLexer[] DefaultSkippers = [

    ];
    
    private readonly SearchValues<char> _startChars;
    private readonly FrozenDictionary<char, ITokenLexer[]> _dispatch;

    /// <summary>
    /// Initializes a lexer with custom token lexers and configuration.
    /// </summary>
    /// <param name="lexers">Token lexers responsible for recognizing structured tokens.</param>
    /// <param name="skippers">Skippers are lexers that may consume input without producing tokens.
    /// 
    /// <para>
    /// They participate in dispatch only if their <see cref="ITokenLexer.StartChar"/> overlaps with token lexers,
    /// ensuring they don't introduce additional start characters and therefore do not increase the scanning cost.
    /// </para>
    /// 
    /// <para>
    /// Although the fragment they skip is not perceived as a token,
    /// it does not disappear from the stream, but becomes part of the TextToken.
    /// </para>
    /// </param>
    public Lexer(ITokenLexer[]? lexers = null, ITokenLexer[]? skippers = null)
    {
        lexers ??= DefaultLexers;
        skippers ??= DefaultSkippers;

        _startChars = SearchValues.Create(lexers.Select(l => l.StartChar).Distinct().ToArray());
        // Groups lexers by their starting character and orders them by priority.
        //
        // This allows:
        // - O(1) selection of candidate lexers;
        // - deterministic resolution when multiple lexers share the same prefix.
        //
        // Only lexers whose ITokenLexer.StartChar matches are considered at runtime.
        // Higher-priority lexers are evaluated first when multiple candidates share the same start character.
        _dispatch = lexers
            .Concat(skippers.Where(s => _startChars.Contains(s.StartChar)))
            .GroupBy(l => l.StartChar)
            .ToFrozenDictionary(
                g => g.Key,
                g => g.OrderByDescending(l => l is IOrderedLexer ol ? ol.Priority : 0).ToArray()
            );
    }

    public IReadOnlyList<ILexicalToken> Tokenize(ReadOnlyMemory<char> source) => Tokenize(source, source.Span);

    /// <summary>
    /// Tokenizes a portion of the source text.
    /// </summary>
    /// <param name="source">The full source buffer.</param>
    /// <param name="span">A slice of <paramref name="source"/> to tokenize.</param>
    /// <param name="spanOffset">The starting offset of <paramref name="span"/> within <paramref name="source"/>.</param>
    /// <param name="context">The lexing context for this session.</param>
    /// <remarks>
    /// The <paramref name="span"/> is expected to represent a contiguous region of <paramref name="source"/>,
    /// starting at <paramref name="spanOffset"/>.
    /// 
    /// <para>
    /// This allows tokenizing subranges of the source while preserving absolute positioning for tokens and diagnostics.
    /// </para>
    /// 
    /// <para>
    /// In other words, for any index <c>i</c> in <paramref name="span"/>,
    /// the corresponding index in <paramref name="source"/> is <c>i + spanOffset</c>.
    /// </para>
    /// </remarks>
    public IReadOnlyList<ILexicalToken> Tokenize(
        ReadOnlyMemory<char> source,
        ReadOnlySpan<char> span,
        int spanOffset = 0,
        LexerContext context = default
    )
    {
        if (context.IsEmpty)
        {
            context = LexerContext.Default;
        }
        List<ILexicalToken> tokens = new(context.BufferConfig.GetInitialCapacity(span.Length));

        // Positions (carriages):
        // - `position`  — absolute index in the original source
        // - `relative`  — index relative to the current span
        // - `textStart` - start of the current plain text segment (between tokens)
        // 
        // This separation allows working with fragments without losing absolute positioning data
        // and avoids unnecessary slicing additional span slices where possible.
        int position = spanOffset;
        int textStart = spanOffset;

        int absoluteSpanEnd = spanOffset + span.Length;
        while (position < absoluteSpanEnd)
        {
            int relative = position - spanOffset;

            // Fast-forward scan to the next possible token start.
            //
            // This is the core optimization of the lexer:
            // scanning is driven by token start characters, not by full parsing logic,
            // which significantly reduces branching and improves performance on large inputs.
            int found = span[relative..].IndexOfAny(_startChars);
            if (found < 0)
            {
                break;
            }

            position += found;
            relative += found;

            if (LexToken(source, span, position, relative, context) is (var token, int nextPosition))
            {
                if (token is not null)
                {
                    if (textStart < position)
                    {
                        tokens.Add(new TextToken(source[textStart..position]));
                    }
                    tokens.Add(token);

                    // Advance `textStart` boundary to the end of the consumed segment.
                    // This ensures seamless interleaving of tokens and raw text without buffering gaps.
                    //
                    // Always true for any successful token match, even when no TextToken is emitted.
                    textStart = nextPosition;
                }
                position = nextPosition;
            }
            else // No lexer matched — advance by one to ensure forward progress.
            {   
                position++;
            }
        }

        
        if (textStart < absoluteSpanEnd)
        {
            tokens.Add(new TextToken(source[textStart..absoluteSpanEnd]));
        }

        return tokens;
    }

    // Attempts to lex a token using dispatch.
    //
    // Escaped characters are ignored to prevent accidental token recognition.
    // Lexers are tried in priority order until one matches.
    private (ILexicalToken? token, int nextPosition)? LexToken(
        ReadOnlyMemory<char> source,
        ReadOnlySpan<char> span,
        int position,
        int relative,
        LexerContext context
    )
    {
        if (_dispatch.TryGetValue(span[relative], out var lexers) && !SyntaxFacts.IsEscaped(span, relative))
        {
            var sliced = span[relative..];
            foreach (var lexer in lexers)
            {
                var result = lexer.LexToken(source, sliced, position, context);
                if (result is not null)
                {
                    return result;
                }
            }
        }
        return null;
    }
}
