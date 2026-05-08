namespace LexiCore.Abstractions;

/// <summary>
/// Represents a lexer participating in priority-based resolution
/// when multiple lexers can handle the same input character.
/// </summary>
/// <remarks>
/// When multiple lexers share the same <see cref="ITokenLexer.StartChar"/>,
/// the lexer with the highest <see cref="Priority"/> is selected first.
/// </remarks>
public interface IOrderedLexer
{
    /// <summary>
    /// Gets the priority of this lexer in the resolution pipeline.
    /// Higher values indicate earlier execution order.
    /// </summary>
    int Priority { get; }
}
