namespace LexiCore.Diagnostics;

/// <summary>
/// Defines numeric codes for diagnostics produced by the lexer and parser.
/// </summary>
/// <remarks>
/// Diagnostic codes are grouped into logical ranges that represent subsystems.
/// Each range reserves space for future expansion to maintain backward compatibility.
/// 
/// <para>
/// Codes are stable and intended for external diagnostics consumption.
/// </para>
/// </remarks>
public enum DiagnosticCode : ushort
{
    #region Core Lexer/Parser Errors (0–99)
    None = 0,
    UnknownError = 1,
    #endregion Core Lexer/Parser Errors

    #region Style (100–199)
    StyleGeneral = 100, // Reserve
    #endregion Style

    #region Template (200–299)
    TemplateGeneral = 200, // Reserve

    // Token
    TemplateTokenEmpty = 201,
    TemplateTokenTooLong = 202,
    TemplateTokenUnclosed = 203,
    TemplateTokenUnexpectedEOF = 204,

    // Identifier
    TemplateMissingIdentifier = 205,
    TemplateInvalidIdentifier = 206,
    TemplateIdentifierTooLong = 207,
    TemplateUnexpectedContentAfterIdentifier = 208,

    // Separator
    TemplateMissingSeparator = 209,
    TemplateInvalidSeparator = 210,

    // ValueKind
    TemplateInvalidValueKind = 211,
    TemplateUnexpectedContentAfterValueKind = 212,
    
    // Value
    TemplateMissingValue = 213,
    TemplateInvalidValueOpen = 214,
    TemplateInvalidValueClose = 215,
    TemplateValueTooLong = 216,
    TemplateValueUnclosed = 217,
    TemplateUnexpectedContentAfterValue = 218,
    #endregion Template
}
