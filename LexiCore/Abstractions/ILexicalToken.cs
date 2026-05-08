namespace LexiCore.Abstractions;

/// <summary>
/// Represents a lexical token produced by the <see cref="Lexing.Lexer"/>.
/// </summary>
/// <remarks>
/// A lexical token is a minimal unit of the lexing stage.
/// It represents a contiguous segment of the original input source.
/// 
/// <para>
/// The interface does not define semantic meaning or token classification.
/// It only provides structural access to the underlying source slice.
/// </para>
/// </remarks>
public interface ILexicalToken
{
    /// <summary>
    /// Gets the original source slice represented by this token.
    /// </summary>
    ReadOnlyMemory<char> Source { get; }

    /// <summary>
    /// Gets the length of the token source slice.
    /// </summary>
    int Length { get; }
}
