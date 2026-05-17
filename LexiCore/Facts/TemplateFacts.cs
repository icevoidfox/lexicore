using LexiCore.Syntax;
using LexiCore.Tokens.Syntax;

namespace LexiCore.Facts;

internal static class TemplateFacts
{
    public static bool IsValidIdentifierChar(char c, bool isFirstChar = false)
    {
        return isFirstChar ? TemplateSyntax.IdentifierStart.Contains(c) : TemplateSyntax.IdentifierBody.Contains(c);
    }

    public static bool IsValidValueKind(TemplateValueKind valueKind) => valueKind switch
    {
        TemplateValueKind.None => false,
        TemplateValueKind.Invalid => false,
        _ => true
    };

    public static TemplateValueKind GetValueKind(char c) => c switch
    {
        'v' => TemplateValueKind.Value,
        'f' => TemplateValueKind.Format,
        _ => TemplateValueKind.Invalid
    };
}
