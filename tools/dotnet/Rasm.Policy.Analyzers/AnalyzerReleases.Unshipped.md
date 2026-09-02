; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

| [INDEX] | [RULE_ID] | [CATEGORY] | [SEVERITY] | [NOTES]                                                                                     |
| :-----: | :-------- | :--------- | :--------- | :------------------------------------------------------------------------------------------ |
|  [01]   | RASM0006  | Usage      | Error      | Executable or plugin host references an interop facade but never invokes its initialization |
