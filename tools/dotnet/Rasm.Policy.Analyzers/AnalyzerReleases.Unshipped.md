; Unreleased analyzer rules
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RASM0006 | Usage | Error | Executable or plugin host references an interop facade but never invokes its initialization
