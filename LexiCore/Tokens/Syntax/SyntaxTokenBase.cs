using LexiCore.Abstractions;
using LexiCore.Diagnostics;

namespace LexiCore.Tokens.Syntax;

/// <summary>
/// Base lexical token representing a contiguous slice of the input source buffer.
/// </summary>
/// <remarks>
/// A token is a structural result of the lexer and represents a single
/// syntactic unit extracted from the input stream.
///
/// It stores only a slice of the original input corresponding to the token
/// (e.g. from opening to closing delimiter), not the entire source buffer.
///
/// Diagnostics are attached to the token and use relative positions inside
/// the token slice. They can be mapped back to the global source using
/// <see cref="Diagnostic.SourceOffset"/> and <see cref="Diagnostic.RelativePosition"/>.
/// </remarks>
/// <param name="source">Slice of the lexer input source buffer.</param>
/// <param name="diagnostics"></param>
public abstract class SyntaxTokenBase(
    ReadOnlyMemory<char> source,
    bool isValid = true,
    ReadOnlyMemory<Diagnostic> diagnostics = default
) : ILexicalToken
{
    /// <inheritdoc/>
    public ReadOnlyMemory<char> Source => source;
    /// <inheritdoc/>
    public int Length => Source.Length;

    /// <summary>
    /// Gets a value indicating whether the token is syntactically valid
    /// and safe to process by the compiler or interpreter.
    /// </summary>
    /// <remarks>
    /// This flag is independent of <see cref="HasDiagnostics"/>.
    /// A token might have diagnostics but still be considered valid.
    /// </remarks>
    public bool IsValid => isValid;

    /// <summary>
    /// Gets the diagnostics attached to this token (errors, warnings, info messages).
    /// </summary>
    public ReadOnlyMemory<Diagnostic> Diagnostics => diagnostics;

    /// <summary>
    /// Gets a value indicating whether this token contains any diagnostics.
    /// </summary>
    public bool HasDiagnostics => !Diagnostics.IsEmpty;
}
