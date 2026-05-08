namespace LexiCore.Lexing.Lexers;

/// <summary>
/// Defines tolerance flags that relax certain lexer constraints.
/// </summary>
/// <remarks>
/// These flags control whether specific validation rules
/// (such as length limits) are enforced or ignored during lexing.
/// </remarks>
[Flags]
public enum TemplateTolerance
{
    None = 0,

    AllowLongTokens = 1 << 0,
    AllowLongIdentifiers = 1 << 1,
    AllowLongValues = 1 << 2,

    All = AllowLongTokens | AllowLongIdentifiers | AllowLongValues
}
