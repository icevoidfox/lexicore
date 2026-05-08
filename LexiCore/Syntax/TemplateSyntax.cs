using LexiCore.Tokens.Syntax;

namespace LexiCore.Syntax;

/// <summary>
/// Defines syntax symbols used for parsing <see cref="TemplateToken"/>.
/// </summary>
public static class TemplateSyntax
{
    public static readonly SearchValues<char> IdentifierStart = SharedSyntax.IdentifierStart;
    public static readonly SearchValues<char> IdentifierBody = SharedSyntax.IdentifierBody;

    public const TemplateValueKind DefaultValueKind = TemplateValueKind.Value;

    public static readonly SyntaxTerminal TokenOpen = new('{');
    public static readonly SyntaxTerminal TokenClose = new('}');
    public static readonly SyntaxTerminal ContentSeparator = new("::");
    public static readonly SyntaxTerminal ValueOpen = new('[');
    public static readonly SyntaxTerminal ValueClose = new(']');
}
