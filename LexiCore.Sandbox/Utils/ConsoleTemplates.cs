using LexiCore.Abstractions;
using LexiCore.Diagnostics;
using LexiCore.Extensions;
using LexiCore.Primitives;
using LexiCore.Tokens.Syntax;

namespace LexiCore.Sandbox.Utils;

public static class ConsoleTemplates
{
    [ThreadStatic]
    private static TemplateBuilder? _shared;
    private static TemplateBuilder Shared => _shared ??= new();

    public static string TextInfo(string text, SourceSegment readableArea) => Shared
        .Label("Original Text:").NL()

        .Inactive(text[..readableArea.Start])
        .Active(text[readableArea.Start..readableArea.End])
        .Inactive(text[readableArea.End..]).NL()
        .Caret('-', readableArea.Start, readableArea.Length)

        .Label("Readble Length: ").Value(readableArea.Length).NL()
        .Label("Readble Positions: ").Range(readableArea.Start, readableArea.End).NL()

        .NL().Default("---").NL(2).ToString();

    public static string TokenInfo(ILexicalToken token, int position) => Shared
        .Label("Token [").Literal(GetTokenStatus(token)).Label("]: ").String(token.Source).NL()
        .Label("Type: ").Type(GetTokenType(token)).NL()
        .Label("Length: ").Value(token.Length).NL()
        .Label("Positions: ").Range(start: position, end: position + token.Length).NL()
        .ToString();

    public static string TemplateTokenMetadata(TemplateToken token)
    {
        if (!token.HasIdentifier && token.ValueKind == TemplateValueKind.None && !token.HasValue)
        {
            return string.Empty;
        }

        Shared.Label("Metadata:").NL();
        if (token.HasIdentifier)
        {
            Shared.Mark('>').Default(" Identifier — ").Identifier(token.RawIdentifier.Unescape()).NL();
        }
        if (token.ValueKind != TemplateValueKind.None || token.HasValue)
        {
            Shared.Mark('>').Default(" Value::").Type(token.ValueKind).Default(" - ");
            if (token.HasValue)
            {
                Shared.String(token.RawValue.ToString());
            }
            else
            {
                Shared.Literal("<...>");
            }
            Shared.NL();
        }
        return Shared.ToString();
    }

    public static string DiagnosticsInfo(
        string readable,
        SourceSegment readableArea,
        SourceSegment tokenArea,
        ReadOnlySpan<Diagnostic> diagnostics,
        Func<Diagnostic, string> messageFormatter
    )
    {
        Shared.Label("Diagnostics:").NL();
        foreach (var d in diagnostics)
        {
            Shared.Mark('-').Indent().Fragment(messageFormatter(d), GetDiagnosticStyle(d.Severity)).NL();

            int diagnosticStart = d.AbsolutePosition - readableArea.Start;

            Shared.Indent(2).Inactive('\'').Fragment(readable[..tokenArea.Start]);
            if (d.Length > 0)
            {
                int diagnosticEnd = diagnosticStart + d.Length;

                Shared.Active(readable[tokenArea.Start..diagnosticStart]);
                Shared.DiagnosticRange(readable[diagnosticStart..diagnosticEnd]);
                Shared.Active(readable[diagnosticEnd..tokenArea.End]);
                Shared.Inactive(readable[tokenArea.End..]).NL();

                Shared.Indent(3).Caret('^', diagnosticStart, d.Length);
            }
            else if (d.AbsoluteEndPosition == readableArea.End) // EOF
            {
                Shared.Active(readable[tokenArea.Start..^1]).DiagnosticInsertion(readable[^1]).Inactive('\'').NL();

                Shared.Indent(3).Caret('~', diagnosticStart - 1);
            }
            else if(diagnosticStart == tokenArea.End)
                {
                Shared.Active(readable[tokenArea.Start..diagnosticStart]);
                Shared.DiagnosticExternalInsertion(readable[diagnosticStart]);
                Shared.Inactive(readable[(diagnosticStart + 1)..]).NL();

                Shared.Indent(3).Caret('~', diagnosticStart);
            }
            else
            {
                Shared.Active(readable[tokenArea.Start..diagnosticStart]);
                Shared.DiagnosticInsertion(readable[diagnosticStart]);
                Shared.Active(readable[(diagnosticStart + 1)..tokenArea.End]);
                Shared.Inactive(readable[tokenArea.End..]).NL();

                Shared.Indent(3).Caret('~', diagnosticStart);
            }
        }
        return Shared.ToString();
    }

    // Helpers
    public static string TokenMetadata(ILexicalToken token) => token switch
    {
        TemplateToken templateToken => TemplateTokenMetadata(templateToken),
        _ => string.Empty,
    };

    public static string GetTokenStatus(ILexicalToken token) => token switch
    {
        SyntaxTokenBase syntaxToken => syntaxToken.IsValid ? "valid" : "invalid",
        _ => "standard",
    };

    public static string GetTokenType(ILexicalToken token) => token switch
    {
        TextToken => "Text",
        StyleToken => "Style",
        TemplateToken => "Template",
        _ => "Unknown",
    };
    
    public static AnsiStyle GetDiagnosticStyle(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Information => Styles.Info,
        DiagnosticSeverity.Warning => Styles.Warn,
        DiagnosticSeverity.Error => Styles.Error,
        _ => Styles.Hidden,
    };
}
