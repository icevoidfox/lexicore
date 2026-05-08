using LexiCore.Tokens.Syntax;

namespace LexiCore.Syntax;

/// <summary>
/// Defines syntax symbols used for parsing <see cref="StyleToken"/>.
/// </summary>
public static class StyleSyntax
{
    public static readonly SearchValues<char> IdentifierStart = SharedSyntax.IdentifierStart;
    public static readonly SearchValues<char> IdentifierBody = SharedSyntax.IdentifierBody;

    public static readonly SyntaxTerminal TokenOpen = new("[<");
    public static readonly SyntaxTerminal TokenClose = new(">]");
    public static readonly SyntaxTerminal ClosePrefix = new('/');
    public static readonly SyntaxTerminal PresetPrefix = new('$');
    public static readonly SyntaxTerminal AssignmentChar = new('=');
    public static readonly SyntaxTerminal ValueSeparator = new(',');
}
