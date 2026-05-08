using LexiCore.Abstractions;
using LexiCore.Diagnostics;
using LexiCore.Extensions;
using System.Text;

using LexiCore.Lexing;
using LexiCore.Lexing.Lexers;
using LexiCore.Primitives;
using LexiCore.Tokens.Syntax;

using LexiCore.Sandbox.Utils;

// I understand that this is difficult to read and that there are better
// and more understandable ways to implement it,
// but for now, this will serve as a rigorous testing ground.
// I needed quick results.

// Data
string[] texts = [
    "Meow, {User}! { :    } Any {text::   g[Woof! weg903jng032'] {for you ааааа ааа:v meow[Фыр} my [<sweet friend. { \\\\{{\\",
    "Any {0,10:C}",
    "{Correct::[Token]}  {[Test]} {  meow   :: f   [] afterValue}",
    "{ m : ["
];

// Configuration
const int TextIndex = 0;
const int ReadableAreaStart = 6, ReadableAreaLength = 108;

var lexer = new Lexer(
    lexers: [
        new TemplateLexer(TemplateLexerOptions.Default with {  } )
    ]
);
var context = new LexerContext(TokenBufferConfig.Default with { }, enableDiagnostics: true);

// Layout
var text = texts[Math.Min(TextIndex, texts.Length - 1)];
var readableArea = new SourceSegment(ReadableAreaStart, length: Math.Min(ReadableAreaLength, text.Length - ReadableAreaStart));

var source = text.AsMemory();
var span = readableArea.Slice(source);

// Step 1: Lexing
var tokens = lexer.Tokenize(source, span, spanOffset: readableArea.Start, context);


// Render
StringBuilder sb = new();

sb.AppendLine($"""
    {Styles.Label}Original Text:{Styles.Default}
    {Styles.Inactive}'{source[..readableArea.Start]}{Styles.Active}{span}{Styles.Inactive}{source[readableArea.End..]}'{Styles.Default}
     {new(' ', readableArea.Start)}{Styles.Caret.Apply(new('-', readableArea.Length))}
    {Styles.Label}Readble Length: {Styles.Value}{readableArea.Length}{Styles.Default}
    {Styles.Label}Readble Positions: {Styles.Range}[{readableArea.Start}..{readableArea.Start + readableArea.Length}]{Styles.Default}

    ---

    """);

int position = readableArea.Start;
foreach (var token in tokens)
{
    sb.AppendLine($"""
        {Styles.Label}Token Source: {Styles.String}'{token.Source}'{Styles.Default}
        {Styles.Label}Type: {Styles.Type}{GetTokenType(token)}{Styles.Default}
        {Styles.Label}Length: {Styles.Value}{token.Length}{Styles.Default}
        {Styles.Label}Positions: {Styles.Range}[{position}..{position + token.Length}]{Styles.Default}
        """);
    PushTokenMetadata(token, sb);

    if (token is SyntaxTokenBase syntaxToken && syntaxToken.HasDiagnostics)
    {
        int tokenStart = position - readableArea.Start;
        int tokenEnd = tokenStart + syntaxToken.Length;
        var diagnostics = syntaxToken.Diagnostics.Span;

        sb.AppendLine(Styles.Label.Apply("Diagnostics:"));
        foreach (var d in diagnostics)
        {
            int diagnosticStart = d.AbsolutePosition - readableArea.Start;

            sb.AppendLine(Styles.ListMark.Apply($"- {GetDiagnosticStyle(d.Severity)}{GetDiagnosticFormattedMessage(d)}"));

            sb.Append($"  {Styles.Inactive}'{span[..tokenStart]}");
            if (d.Length > 0)
            {
                int diagnosticEnd = diagnosticStart + d.Length;

                sb.Append($"{Styles.Active}{span[tokenStart..diagnosticStart]}");
                sb.Append($"{Styles.DiagnosticRange}{span[diagnosticStart..diagnosticEnd]}");
                sb.Append($"{Styles.Active}{span[diagnosticEnd..tokenEnd]}");
                sb.AppendLine($"{Styles.Inactive}{span[tokenEnd..]}'{Styles.Default}");

                sb.AppendLine($"   {new(' ', diagnosticStart)}{Styles.Caret.Apply(new('^', d.Length))}");
            }
            else if (d.AbsoluteEndPosition == span.Length + readableArea.Start)
            {
                sb.Append($"{Styles.Active}{span[tokenStart..^1]}");
                sb.AppendLine($"{Styles.DiagnosticInsertion}{span[^1]}{Styles.Inactive}'{Styles.Default}");

                sb.AppendLine($"   {new(' ', diagnosticStart - 1)}{Styles.Caret.Apply("~")}");
            }
            else
            {
                sb.Append($"{Styles.Active}{span[tokenStart..diagnosticStart]}");
                if (diagnosticStart == tokenEnd)
                {
                    sb.Append($"{Styles.DiagnosticExternalInsertion}{span[diagnosticStart]}");
                    sb.AppendLine($"{Styles.Inactive}{span[(diagnosticStart + 1)..]}'{Styles.Default}");
                }
                else
                {
                    sb.Append($"{Styles.DiagnosticInsertion}{span[diagnosticStart]}");
                    sb.Append($"{Styles.Active}{span[(diagnosticStart + 1)..tokenEnd]}");
                    sb.AppendLine($"{Styles.Inactive}{span[tokenEnd..]}'{Styles.Default}");
                }

                sb.AppendLine($"   {new(' ', diagnosticStart)}{Styles.Caret.Apply("~")}");
            }
        }
    }
    else sb.AppendLine();

    position += token.Length;
}

Console.WriteLine(sb);

// Helpers
void PushTokenMetadata(ILexicalToken token, StringBuilder sb)
{
    switch (token)
    {
        case TemplateToken template:
            if (template.HasIdentifier || template.ValueKind != TemplateValueKind.None || template.HasValue)
            {
                sb.AppendLine(Styles.Label.Apply("Metadata:"));
                if (template.HasIdentifier)
                {
                    sb.AppendLine(Styles.ListMark.Apply($">{Styles.Default} Identifier — {Styles.Identifier}{template.RawIdentifier}"));
                }
                if (template.ValueKind != TemplateValueKind.None)
                {
                    sb.AppendLine(Styles.ListMark.Apply($">{Styles.Default} ValueKind — {Styles.Type}{template.ValueKind}"));
                }
                if (template.HasValue)
                {
                    sb.AppendLine(Styles.ListMark.Apply($">{Styles.Default} Value — {Styles.String}'{template.RawValue.Unescape()}'"));
                }
            }
            break;
        default:
            break;
    }
}

string GetDiagnosticFormattedMessage(Diagnostic d)
{
    if (d.Code == DiagnosticCode.TemplateTokenUnexpectedEOF)
    { // The message with this code already contains words indicating the end of the file.
        return $"[{d.FormattedCode}] {d.PlainMessage}";
    }

    string location;
    if (d.AbsoluteEndPosition == source.Length)
    {
        location = "at end of file";
    }
    else if (d.Length == 1 || d.Code == DiagnosticCode.TemplateTokenEmpty)
    {
        location = $"at position {d.AbsolutePosition}";
    }
    else if (d.Length == 0)
    {
        location = $"at insertion point {d.AbsolutePosition}";
    }
    else
    {
        location = $"in positions [{d.AbsolutePosition}..{d.AbsoluteEndPosition}]";
    }

    return $"[{d.FormattedCode}] {d.PlainMessage} {location}";
}

string GetTokenType(ILexicalToken token) => token switch
{
    TextToken => "Text",
    StyleToken => "Style",
    TemplateToken => "Template",
    _ => "Unknown"
};

AnsiStyle GetDiagnosticStyle(DiagnosticSeverity severity) => severity switch
{
    DiagnosticSeverity.Information => Styles.DiagnosticInfo,
    DiagnosticSeverity.Warning => Styles.DiagnosticWarn,
    DiagnosticSeverity.Error => Styles.DiagnosticError,
    DiagnosticSeverity.Hidden or _ => Styles.DiagnosticHidden
};
