using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SentinelTests : CSharpCodeFixVerifier<SentinelAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3102]

    [Fact]
    public Task Entries() => new CSharpCodeFixTest<SentinelAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using System;
            using LanguageExt;
            using Rhino;
            using Rhino.Geometry;
            using Id = System.Guid;

            internal sealed record Entry(Id Key);

            internal sealed record DateEntry(DateTime Date);

            internal sealed record OffsetEntry(DateTimeOffset Date);

            internal sealed record NumericEntry(double Number);

            internal sealed record TextEntry(string Text);

            internal sealed record LiteralEntry(string Text);

            internal sealed record CountEntry(int Count);

            internal sealed record Callback<T>(Func<T> Value);

            internal sealed record WideEntry(int First, int Second, int Third, int Fourth, int Fifth, int Sixth, int Seventh, int Eighth, Id Key);

            internal sealed record ReplacedText(string {|CS8907:Text|}) {
                public string Text { get; init; } = "display";
                internal bool Missing => Text == "";
            }

            internal sealed record StoredText(string Text) {
                public string Text { get; init; } = Text;
                internal bool Missing => Text == "";
            }

            internal readonly record struct PresentKey(Id Value) {
                public static implicit operator PresentKey(Id value) => new(Id.NewGuid());
            }

            internal sealed record ConvertedEntry(PresentKey Key);

            internal static class Entries {
                private static readonly Id Missing = Id.Empty;
                private const double Unsettled = 3d;
                private static double MissingNumber => RhinoMath.UnsetValue;
                internal static Entry Empty() => {|#0:new(Id.Empty)|};
                internal static Entry Default() => {|#1:new(default)|};
                internal static Entry Constructed() => {|#2:new(new Id())|};
                internal static Entry Aliased() => {|#3:new(Missing)|};
                internal static DateEntry Minimum() => {|#4:new(DateTime.MinValue)|};
                internal static DateEntry DefaultDate() => {|#5:new(new DateTime())|};
                internal static NumericEntry Unknown() => {|#6:new(double.NaN)|};
                internal static WideEntry Positional() => {|#7:new(1, 2, 3, 4, 5, 6, 7, 8, default)|};
                internal static WideEntry Named() => {|#8:new(Key: default, First: 1, Second: 2, Third: 3, Fourth: 4, Fifth: 5, Sixth: 6, Seventh: 7, Eighth: 8)|};
                internal static TextEntry EmptyText() => {|#9:new("")|};
                internal static TextEntry MissingText(string? text) => {|#10:new({|#11:text ?? ""|})|};
                internal static TextEntry OptionalText(Option<string> text) => {|#12:new({|#13:text.IfNone("")|})|};
                internal static Entry Replaced(Entry entry) => {|#14:entry with { Key = Id.Empty }|};
                internal static Entry Initialized(Id key) => {|#15:new(key) { Key = Id.Empty }|};
                internal static bool MissingValue(TextEntry entry) => string.IsNullOrEmpty(entry.Text);
                internal static LiteralEntry Literal() => new("");
                internal static LiteralEntry DefaultedDisplay(string? text) => new({|#16:text ?? ""|});
                internal static LiteralEntry OptionalDisplay(Option<string> text) => new({|#17:text.IfNone("")|});
                internal static LiteralEntry MatchedDisplay(Option<string> text) => new({|#18:text.Match(None: () => "", Some: value => value.Trim())|});
                internal static CountEntry Zero() => new(0);
                internal static Callback<double> DeferredNumber() => new(() => double.NaN);
                internal static Callback<Id> DeferredKey() => new(() => Id.Empty);
                internal static Entry Present(Id key) => new(key);
                internal static OffsetEntry Offset() => {|#19:new(DateTimeOffset.MinValue)|};
                internal static NumericEntry SingleUnknown() => {|#20:new(float.NaN)|};
                internal static TextEntry Matched(Option<string> text) => {|#21:new({|#22:text.Match(None: () => "", Some: value => value.Trim())|})|};
                internal static Entry Conditional(bool present, Id key) => {|#23:new(present ? key : Id.Empty)|};
                internal static NumericEntry Unset() => {|#24:new(Point3d.Unset.X)|};
                internal static NumericEntry Ordinary() => new(Unsettled);
                internal static Entry Reassigned() {
                    Id held = Id.Empty;
                    held = Id.NewGuid();
                    return new(held);
                }
                internal static Entry StoredBefore() {
                    Id held = Id.Empty;
                    Entry first = {|#25:new(held)|};
                    held = Id.NewGuid();
                    return first;
                }
                internal static ReplacedText Unstored() => new("");
                internal static StoredText Stored() => {|#26:new("")|};
                internal static NumericEntry UnsetNumber() => {|#27:new(RhinoMath.UnsetValue)|};
                internal static NumericEntry NativeAlias() => {|#28:new(MissingNumber)|};
                internal static ConvertedEntry Converted() => new(Id.Empty);
                internal static Entry Finalized(bool returned) {
                    Id held = Id.NewGuid();
                    Id selected;
                    try {
                        if (returned) {
                            held = Id.Empty;
                            return new(Id.NewGuid());
                        }
                    }
                    finally {
                        selected = held;
                    }
                    return new(selected);
                }
            }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            RASM3102.sentinels = RhinoCommon:Unset
            """), }, },
        ExpectedDiagnostics = {
            Diagnostic().WithLocation(0).WithArguments("Entry", "Id.Empty"),
            Diagnostic().WithLocation(1).WithArguments("Entry", "default"),
            Diagnostic().WithLocation(2).WithArguments("Entry", "new Id()"),
            Diagnostic().WithLocation(3).WithArguments("Entry", "Id.Empty"),
            Diagnostic().WithLocation(4).WithArguments("DateEntry", "DateTime.MinValue"),
            Diagnostic().WithLocation(5).WithArguments("DateEntry", "new DateTime()"),
            Diagnostic().WithLocation(6).WithArguments("NumericEntry", "double.NaN"),
            Diagnostic().WithLocation(7).WithArguments("WideEntry", "default"),
            Diagnostic().WithLocation(8).WithArguments("WideEntry", "default"),
            Diagnostic().WithLocation(9).WithArguments("TextEntry", "\"\""),
            Diagnostic().WithLocation(10).WithArguments("TextEntry", "\"\""),
            Diagnostic().WithLocation(11).WithArguments("text ?? \"\"", "\"\""),
            Diagnostic().WithLocation(12).WithArguments("TextEntry", "\"\""),
            Diagnostic().WithLocation(13).WithArguments("""text.IfNone("")""", "\"\""),
            Diagnostic().WithLocation(14).WithArguments("Entry", "Id.Empty"),
            Diagnostic().WithLocation(15).WithArguments("Entry", "Id.Empty"),
            Diagnostic().WithLocation(16).WithArguments("text ?? \"\"", "\"\""),
            Diagnostic().WithLocation(17).WithArguments("""text.IfNone("")""", "\"\""),
            Diagnostic().WithLocation(18).WithArguments("""text.Match(None: () => "", Some: value => value.Trim())""", "() => \"\""),
            Diagnostic().WithLocation(19).WithArguments("OffsetEntry", "DateTimeOffset.MinValue"),
            Diagnostic().WithLocation(20).WithArguments("NumericEntry", "float.NaN"),
            Diagnostic().WithLocation(21).WithArguments("TextEntry", "\"\""),
            Diagnostic().WithLocation(22).WithArguments("""text.Match(None: () => "", Some: value => value.Trim())""", "() => \"\""),
            Diagnostic().WithLocation(23).WithArguments("Entry", "Id.Empty"),
            Diagnostic().WithLocation(24).WithArguments("NumericEntry", "Point3d.Unset"),
            Diagnostic().WithLocation(25).WithArguments("Entry", "Id.Empty"),
            Diagnostic().WithLocation(26).WithArguments("StoredText", "\"\""),
            Diagnostic().WithLocation(27).WithArguments("NumericEntry", "RhinoMath.UnsetValue"),
            Diagnostic().WithLocation(28).WithArguments("NumericEntry", "RhinoMath.UnsetValue"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [FILES]

    [Fact]
    public Task CrossFileStorage() => new CSharpCodeFixTest<SentinelAnalyzer, EmptyCodeFixProvider> {
        TestState = { Sources = {
            ("/Models.cs", """
            using System;

            internal sealed partial record Entry(Guid Key, string Text);

            internal static partial class Flow {
                internal static Entry Build(string text, Guid key) => new(key, text);
            }
            """),
            ("/Calls.cs", """
            using System;

            internal sealed partial record Entry {
                internal bool Missing => Key == Guid.Empty;
            }

            internal static partial class Flow {
                internal static Entry Typed(Type type, Guid key) => Build(key: key, text: type.Name);
                internal static Entry Empty() => {|RASM3102:new Entry(default, "display")|};
            }
            """),
        }, },
    }.RunAsync(TestContext.Current.CancellationToken);
}
