using System.Text;

namespace LexiCore.Sandbox.Utils;

public class TemplateBuilder
{
    private readonly StringBuilder _sb = new();
    private bool _isStyleDirty = false;

    public TemplateBuilder Style(AnsiStyle? style = null)
    {
        style ??= Styles.Default;

        if (style == Styles.Default && !_isStyleDirty)
        {
            return this;
        }
        _isStyleDirty = style != Styles.Default;

        _sb.Append(style);
        return this;
    }

    public TemplateBuilder Fragment(object? value, AnsiStyle? style = null)
    {
        if (style is not null)
        {
            Style(style);
        }
        _sb.Append(value);
        return this;
    }
    
    public TemplateBuilder NL(int count = 1)
    {
        Style();
        for (int i = 0; i < count; i++)
        {
            _sb.AppendLine();
        }
        return this;
    }

    public TemplateBuilder Repeat(char c, int count)
    {
        _sb.Append(c, count);
        return this;
    }
    
    public TemplateBuilder Char(char c) => Repeat(c, count: 1);
    public TemplateBuilder Indent(int count = 1) => Repeat(' ', count);

    // General
    public TemplateBuilder Default(object value) => Fragment(value, _isStyleDirty ? Styles.Default : null);
    public TemplateBuilder Active(object value) => Fragment(value, Styles.Active);
    public TemplateBuilder Inactive(object value) => Fragment(value, Styles.Inactive);
    
    // Structures
    public TemplateBuilder Label(string text) => Fragment(text, Styles.Label);
    public TemplateBuilder Mark(char c) => Style(Styles.Mark).Char(c);
    public TemplateBuilder Caret(char c, int offset, int length = 1)
        => Indent(offset).Style(Styles.Caret).Repeat(c, length).NL();

    // Value Types
    public TemplateBuilder Type(object value) => Fragment(value, Styles.Type);
    public TemplateBuilder Literal(object value) => Fragment(value, Styles.Literal);
    public TemplateBuilder Range(int start, int end) => Fragment($"[{start}..{end}]", Styles.Range);
    public TemplateBuilder String(object value) => Fragment($"'{value}'", Styles.String);
    public TemplateBuilder Identifier(object value) => Fragment(value, Styles.Identifier);
    public TemplateBuilder Value(object? value) => Fragment(value, Styles.Value);
    
    // Token Diagnostic
    public TemplateBuilder DiagnosticRange(string text) => Fragment(text, Styles.DiagnosticRange);
    public TemplateBuilder DiagnosticInsertion(char c) => Style(Styles.DiagnosticInsertion).Char(c);
    public TemplateBuilder DiagnosticExternalInsertion(char c) => Style(Styles.DiagnosticExternalInsertion).Char(c);

    // Diagnostic Severity
    public TemplateBuilder Hidden(string text) => Fragment(text, Styles.Hidden);
    public TemplateBuilder Info(string text) => Fragment(text, Styles.Info);
    public TemplateBuilder Warn(string text) => Fragment(text, Styles.Warn);
    public TemplateBuilder Error(string text) => Fragment(text, Styles.Error);

    public override string ToString()
    {
        Style();
        var result = _sb.ToString();
        Clear();
        return result;
    }

    public void Clear()
    {
        _sb.Clear();
        _isStyleDirty = false;
    }
}
