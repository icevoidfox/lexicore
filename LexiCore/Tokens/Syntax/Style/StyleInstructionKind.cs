namespace LexiCore.Tokens.Syntax;

/// <summary>
/// Describes how the <see cref="StyleInstruction"/> of a <see cref="StyleToken"/>
/// should be structurally interpreted.
/// </summary>
/// <remarks>
/// This value is produced by the <see cref="Lexing.Lexers.StyleLexer"/>
/// as a lightweight structural hint and does not represent semantic meaning.
/// </remarks>
public enum StyleInstructionKind
{
    Push,
    Pop
}
