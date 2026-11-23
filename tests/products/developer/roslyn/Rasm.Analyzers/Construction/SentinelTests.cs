using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Construction;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SentinelTests : CSharpCodeFixVerifier<SentinelAnalyzer, EmptyCodeFixProvider> {

    // --- [RECORDS]

    [Fact]
    public Task ReportsRecordValueThatCanBeASentinelButNotAPresentOrOverwrittenValue() => new CSharpCodeFixTest<SentinelAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using System;
            using Rhino;
            using Rhino.Geometry;

            internal sealed record Entry(Guid Key);

            internal sealed record NumericEntry(double Number);

            internal sealed record CountEntry(int Count);

            internal readonly record struct PresentKey(Guid Value) {
                public static implicit operator PresentKey(Guid value) => new(Guid.NewGuid());
            }

            internal sealed record ConvertedEntry(PresentKey Key);

            internal static class Entries {
                private const double Unsettled = 3d;
                internal static Entry Empty() => {|#0:new(Guid.Empty)|};
                internal static Entry Defaulted() => {|#1:new(default)|};
                internal static Entry Replaced(Entry entry) => {|#2:entry with { Key = Guid.Empty }|};
                internal static Entry Conditional(bool present, Guid key) => {|#3:new(present ? key : Guid.Empty)|};
                internal static NumericEntry Unset() => {|#4:new(RhinoMath.UnsetValue)|};
                internal static NumericEntry UnsetCoordinate() => {|#5:new(Point3d.Unset.X)|};
                internal static Entry StoredBefore() {
                    Guid held = Guid.Empty;
                    Entry first = {|#6:new(held)|};
                    held = Guid.NewGuid();
                    return first;
                }
                internal static Entry Present(Guid key) => new(key);
                internal static CountEntry Zero() => new(default);
                internal static NumericEntry Ordinary() => new(Unsettled);
                internal static ConvertedEntry Converted() => new(Guid.Empty);
                internal static Entry Reassigned() {
                    Guid held = Guid.Empty;
                    held = Guid.NewGuid();
                    return new(held);
                }
            }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            RASM3102.sentinels = RhinoCommon:Unset
            """), }, },
        ExpectedDiagnostics = {
            Diagnostic().WithLocation(0).WithArguments("Entry", "Guid.Empty"),
            Diagnostic().WithLocation(1).WithArguments("Entry", "default"),
            Diagnostic().WithLocation(2).WithArguments("Entry", "Guid.Empty"),
            Diagnostic().WithLocation(3).WithArguments("Entry", "Guid.Empty"),
            Diagnostic().WithLocation(4).WithArguments("NumericEntry", "RhinoMath.UnsetValue"),
            Diagnostic().WithLocation(5).WithArguments("NumericEntry", "Point3d.Unset"),
            Diagnostic().WithLocation(6).WithArguments("Entry", "Guid.Empty"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task ReportsEmptyTextOnlyForARecordMemberAnotherFileTestsForEmptiness() => new CSharpCodeFixTest<SentinelAnalyzer, EmptyCodeFixProvider> {
        TestState = { Sources = {
            ("/Models.cs", """
            using System;

            internal sealed partial record Entry(Guid Key, string Text);

            internal sealed record Label(string Text);

            internal sealed record Shown(string {|CS8907:Text|}) {
                public string Text { get; init; } = "display";
                internal bool Missing => Text.Length == 0;
            }

            internal static class Flow {
                internal static Entry Blank(Guid key) => {|#0:new(key, "")|};
                internal static Label Literal() => new("");
                internal static Shown Unstored() => new("");
            }
            """),
            ("/Checks.cs", """
            internal sealed partial record Entry {
                internal bool Missing => string.IsNullOrEmpty(Text);
            }
            """),
        }, },
        ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("Entry", "\"\"") },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [FALLBACKS]

    [Fact]
    public Task ReportsEmptyStringFallbackButNotOneSplicedIntoTextConvertedForAHostOrFilledByTheCaller() => VerifyAnalyzerAsync(
        """
        using System.Runtime.CompilerServices;
        using LanguageExt;

        internal static class Fallbacks {
            internal static int Coalesced(string? text) => ({|#0:text ?? ""|}).Length;
            internal static int Defaulted(Option<string> text) => {|#1:text.IfNone("")|}.Length;
            internal static int Matched(Option<string> text) => {|#2:text.Match(None: () => "", Some: value => value.Trim())|}.Length;
            internal static string Read(string key) => Lookup(key, {|#3:defaultValue: ""|});
            internal static void Log(string {|#4:label|} = "") { }
            internal static string Spliced(string? text) => $"[{text ?? ""}]";
            internal static string Host(Option<string> text) => text.IfNone("");
            internal static int Named(string? text) => (text ?? "none").Length;
            internal static void Trace([CallerMemberName] string member = "") { }
            private static string Lookup(string key, string defaultValue) => key.Length > 0 ? key : defaultValue;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("text ?? \"\"", "\"\""),
        Diagnostic().WithLocation(1).WithArguments("""text.IfNone("")""", "\"\""),
        Diagnostic().WithLocation(2).WithArguments("""text.Match(None: () => "", Some: value => value.Trim())""", "() => \"\""),
        Diagnostic().WithLocation(3).WithArguments("Lookup", "\"\""),
        Diagnostic().WithLocation(4).WithArguments("Log", "\"\""));
}
