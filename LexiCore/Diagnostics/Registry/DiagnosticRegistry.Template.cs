using LexiCore.Syntax;
using LexiCore.Tokens.Syntax;

namespace LexiCore.Diagnostics;

internal static partial class DiagnosticRegistry
{
    private static partial void Register(Dictionary<DiagnosticId, DiagnosticDescriptor> map)
    {
        // Token
        map[DiagnosticId.TemplateTokenEmpty] = Create(
            DiagnosticCode.TemplateTokenEmpty,
            $"{nameof(TemplateToken)} is empty."
        );
        map[DiagnosticId.TemplateTokenTooLong] = Create<TooLongArgs>(
            DiagnosticCode.TemplateTokenTooLong,
            (args) => $"{nameof(TemplateToken)} exceeds maximum length of {args.Max} (over by {args.Overflow} characters)."
        );
        map[DiagnosticId.TemplateTokenUnclosed] = Create(
            DiagnosticCode.TemplateTokenUnclosed,
            $"{nameof(TemplateToken)} was not closed properly (missing '{TemplateSyntax.TokenClose}')."
        );
        map[DiagnosticId.TemplateTokenTrailingEscape] = Create(
            DiagnosticCode.TemplateTokenUnexpectedEOF,
            $"Incomplete escape sequence at the end of the template (trailing '{SharedSyntax.EscapeChar}')."
        );
        map[DiagnosticId.TemplateTokenUnexpectedEOF] = Create(
            DiagnosticCode.TemplateTokenUnexpectedEOF,
            "Unexpected end of file while parsing template token."
        );

        // Identifier
        map[DiagnosticId.TemplateIdentifierMissing] = Create(
            DiagnosticCode.TemplateMissingIdentifier,
            "Identifier missing."
        );
        map[DiagnosticId.TemplateIdentifierInvalidStart] = Create<CharArgs>(
            DiagnosticCode.TemplateInvalidIdentifier,
            (args) => $"Identifier must start with a letter [A-Za-z] or a special character [{SharedSyntax.ExtraIdentifierStart}], but found '{args.Character}'."
        );
        map[DiagnosticId.TemplateIdentifierInvalidChars] = Create(
            DiagnosticCode.TemplateInvalidIdentifier,
            $"Identifier must contain only letters [A-Za-z], numbers [0-9], or special characters [{SharedSyntax.ExtraIdentifierBody}]."
        );
        map[DiagnosticId.TemplateIdentifierTooLong] = Create<TooLongArgs>(
            DiagnosticCode.TemplateIdentifierTooLong,
            (args) => $"{nameof(TemplateToken.Identifier)} exceeds maximum length of {args.Max} (over by {args.Overflow} characters)."
        );
        map[DiagnosticId.TemplateIdentifierUnexpectedContent] = Create(
            DiagnosticCode.TemplateUnexpectedContentAfterIdentifier,
            "Unexpected characters after identifier."
        );

        // Separator
        map[DiagnosticId.TemplateSeparatorMissing] = Create(
            DiagnosticCode.TemplateMissingSeparator,
            $"Missing identifier-value separator '{TemplateSyntax.ContentSeparator}'."
        );
        map[DiagnosticId.TemplateSeparatorInvalid] = Create(
            DiagnosticCode.TemplateInvalidSeparator,
            $"Invalid separator. Expected '{TemplateSyntax.ContentSeparator}' between identifier and value kind/value."
        );
        map[DiagnosticId.TemplateSeparatorIncomplete] = Create(
            DiagnosticCode.TemplateInvalidSeparator,
            $"Separator is incomplete. Expected '{TemplateSyntax.ContentSeparator}'."
        );

        // ValueKind
        map[DiagnosticId.TemplateValueKindInvalid] = Create(
            DiagnosticCode.TemplateInvalidValueKind,
            "Invalid value kind character. Expected one of the allowed template value kinds (e.g., 'v', 'f')."
        );
        map[DiagnosticId.TemplateValueKindUnexpectedContent] = Create(
            DiagnosticCode.TemplateUnexpectedContentAfterValueKind,
            "Unexpected characters after value kind."
        );

        // Value
        map[DiagnosticId.TemplateValueMissing] = Create(
            DiagnosticCode.TemplateMissingValue,
            "Missing value section after separator and/or value kind."
        );
        map[DiagnosticId.TemplateValueInvalidOpen] = Create(
            DiagnosticCode.TemplateInvalidValueOpen,
            $"Invalid value open symbol. Expected '{TemplateSyntax.ValueOpen}'."
        );
        map[DiagnosticId.TemplateValueIncompleteOpen] = Create(
            DiagnosticCode.TemplateInvalidValueOpen,
            $"Value open symbol is incomplete. Expected '{TemplateSyntax.ValueOpen}'."
        );
        map[DiagnosticId.TemplateValueInvalidClose] = Create(
            DiagnosticCode.TemplateInvalidValueClose,
            $"Invalid value close symbol. Expected '{TemplateSyntax.ValueClose}'."
        );
        map[DiagnosticId.TemplateValueIncompleteClose] = Create(
            DiagnosticCode.TemplateInvalidValueClose,
            $"Value close symbol is incomplete. Expected '{TemplateSyntax.ValueClose}'."
        );
        map[DiagnosticId.TemplateValueTooLong] = Create<TooLongArgs>(
            DiagnosticCode.TemplateValueTooLong,
            (args) => $"{nameof(TemplateToken.Value)} exceeds maximum length of {args.Max} (over by {args.Overflow} characters)."
        );
        map[DiagnosticId.TemplateValueUnclosed] = Create(
            DiagnosticCode.TemplateValueUnclosed,
            $"{nameof(TemplateToken.Value)} was not closed properly (missing '{TemplateSyntax.ValueClose}')."
        );
        map[DiagnosticId.TemplateValueUnexpectedContent] = Create(
            DiagnosticCode.TemplateUnexpectedContentAfterValue,
            "Unexpected characters after value."
        );
    }
}
