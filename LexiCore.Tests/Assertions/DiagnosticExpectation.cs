using LexiCore.Diagnostics;

namespace LexiCore.Tests.Assertions;

internal readonly record struct DiagnosticExpectation(
    DiagnosticId Id,
    DiagnosticSeverity Severity
);
