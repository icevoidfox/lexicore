using LexiCore.Abstractions;

namespace LexiCore.Tokens.Syntax;

/// <summary>
/// Represents a text segment produced by the lexer when no specialized token matches.
/// </summary>
public sealed class TextToken(ReadOnlyMemory<char> source) : ILexicalToken
{
    /// <inheritdoc/>
    public ReadOnlyMemory<char> Source { get; } = source;
    /// <inheritdoc/>
    public int Length => Source.Length;

    /// <summary>
    /// Gets the token content as a span.
    /// </summary>
    public ReadOnlySpan<char> Text => Source.Span;
}
