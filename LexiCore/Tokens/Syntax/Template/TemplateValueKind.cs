namespace LexiCore.Tokens.Syntax;

/// <summary>
/// Describes how the value segment of a <see cref="TemplateToken"/>
/// should be structurally interpreted.
/// </summary>
/// <remarks>
/// This value is produced by the <see cref="Lexing.Lexers.TemplateLexer"/>
/// as a lightweight structural hint and does not represent semantic meaning.
/// </remarks>
public enum TemplateValueKind : byte
{
    None,
    Value,
    Format,
}
