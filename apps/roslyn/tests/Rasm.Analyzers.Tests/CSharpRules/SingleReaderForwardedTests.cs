using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SingleReaderForwardedTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {
    [Fact]
    public Task Palette() => VerifyAnalyzerAsync(
        """
        using Rhino.Geometry;

        internal static class Palette {
            internal static System.Collections.Immutable.ImmutableArray<int> Empty => [];

            internal static System.Drawing.Bitmap Blank => new(1, 1);

            internal static System.Drawing.Point Origin => default;

            internal static Point Placed => new(Point3d.Origin);

            internal static Plane {|#0:Base|} => Plane.WorldXY;

            internal static System.Numerics.Vector2 Offset => default;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Base", "Plane.WorldXY"));

    [Fact]
    public Task FileReads() => VerifyAnalyzerAsync(
        """
        using System.ComponentModel;
        using System.IO;
        using System.Runtime.CompilerServices;

        internal sealed class FileReads(FileInfo file, FileInfo? optional) {
            internal string Name => file.Name;

            internal long Length {
                get { return file.Length; }
            }

            internal string? Optional => optional?.Name;

            internal string Upper => file.Name.ToUpperInvariant();

            internal double Converted => file.Length;

            [DisplayName("File name")]
            internal string Displayed => file.Name;

            internal long Measured {
                [MethodImpl(MethodImplOptions.NoInlining)]
                get => file.Length;
            }
        }
        """);

    [Fact]
    public Task PublicFileReads() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal sealed class PublicFileReads(FileInfo file, FileInfo? optional) {
            public FileInfo File { get; } = file;

            public FileInfo? OptionalFile { get; } = optional;

            internal string {|#0:Name|} => File.Name;

            internal long {|#1:Length|} {
                get { return File.Length; }
            }

            internal string? {|#2:Optional|} => OptionalFile?.Name;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Name", "File.Name"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Length", "File.Length"),
        Diagnostic("RASM3012").WithLocation(2).WithArguments("Optional", "OptionalFile?.Name"));

    [Fact]
    public Task OwnedReads() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal sealed class OwnedReads(FileInfo file) {
            private readonly FileInfo Source = file;

            public FileInfo File { get; } = file;

            internal string {|#0:Owned|} => File.Name;

            internal string Uppercase => Source.Name;

            internal string Explicit => this.Source.Name;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Owned", "File.Name"));

    [Fact]
    public Task StaticReads() => VerifyAnalyzerAsync(
        """
        using System;
        using Calendar = System.DateTime;

        internal static class StaticReads {
            internal static DateTime {|#0:Utc|} => Calendar.UtcNow;

            internal static int {|#1:Maximum|} => int.MaxValue;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Utc", "Calendar.UtcNow"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Maximum", "int.MaxValue"));

    [Fact]
    public Task ArrayReads() => VerifyAnalyzerAsync(
        """
        internal sealed class ArrayReads(int[] values) {
            internal int this[int index] => values[index];
        }
        """);

    [Fact]
    public Task MatrixReads() => VerifyAnalyzerAsync(
        """
        internal sealed class MatrixReads(int[,] values) {
            internal int this[int row, int column] {
                get { return values[row, column]; }
            }
        }
        """);

    [Fact]
    public Task ListReads() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;

        internal sealed class ListReads(List<int> values) {
            internal int this[int index] => values[index];

            internal Action Clear => values.Clear;
        }
        """);

    [Fact]
    public Task PublicArrayReads() => VerifyAnalyzerAsync(
        """
        internal sealed class PublicArrayReads(int[] values) {
            public int[] Values { get; } = values;

            internal int {|#0:this|}[int index] => Values[index];
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("this[]", "Values[index]"));

    [Fact]
    public Task PublicMatrixReads() => VerifyAnalyzerAsync(
        """
        internal sealed class PublicMatrixReads(int[,] values) {
            public int[,] Values { get; } = values;

            internal int {|#0:this|}[int row, int column] {
                get { return Values[row, column]; }
            }
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("this[]", "Values[row, column]"));

    [Fact]
    public Task PublicListReads() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;

        internal sealed class PublicListReads(List<int> values) {
            public List<int> Values { get; } = values;

            internal int {|#0:this|}[int index] => Values[index];

            internal Action {|#1:Clear|} => Values.Clear;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("this[]", "Values[index]"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Clear", "Values.Clear"));

    [Fact]
    public Task CollectionView() => VerifyAnalyzerAsync(
        """
        using System.Collections;
        using System.Collections.Generic;

        internal sealed class CollectionView(List<int> values) : IReadOnlyCollection<int> {
            public int Count => values.Count;

            public int Capacity => values.Capacity;

            public IEnumerator<int> GetEnumerator() => values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        """);

    [Fact]
    public Task PublicCollectionReads() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal sealed class PublicCollectionReads(List<int> values) {
            public List<int> Values { get; } = values;

            public int {|#0:Capacity|} => Values.Capacity;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Capacity", "Values.Capacity"));

    [Fact]
    public Task PrivateParameterAlias() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using System.IO;
        using LanguageExt;

        internal sealed class HeldReads(Lazy<List<int>> delayed, Atom<List<int>> current) {
            internal int Delayed => delayed.Value.Count;

            internal int Current => current.Value.Count;
        }

        namespace UserDefined {
            public sealed class Lazy<T>(T value) {
                public T Value { get; } = value;
            }

            internal sealed class Read(Lazy<List<int>> value) {
                public Lazy<List<int>> Source { get; } = value;

                internal int {|#0:Count|} => Source.Value.Count;
            }
        }

        internal sealed class PrivateParameterAlias(FileInfo file) {
            private string {|#1:Name|} => file.Name;

            internal string Read() => Name;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Count", "Source.Value.Count"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Name", "file.Name"));

    [Fact]
    public Task TransformedReads() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal sealed class TransformedReads(List<int> values) {
            internal int this[int index] => values[index + 1];

            internal int First => values[0];
        }
        """);

    [Fact]
    public Task PermutedReads() => VerifyAnalyzerAsync(
        """
        internal sealed class PermutedReads(int[,] values) {
            internal int this[int row, int column] => values[column, row];
        }
        """);

    [Fact]
    public Task WritableReads() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal sealed class WritableReads(List<int> values) {
            internal int this[int index] {
                get => values[index];
                set => values[index] = value;
            }
        }
        """);

    [Fact]
    public Task OverriddenRead() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal abstract class LengthContract {
            internal abstract long Length { get; }
        }

        internal sealed class OverriddenRead(FileInfo file) : LengthContract {
            internal override long Length => file.Length;
        }
        """);

    [Fact]
    public Task ExtensibleRead() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal class ExtensibleRead(List<int> values) {
            internal virtual int Count => values.Count;
        }
        """);

    [Fact]
    public Task HiddenRead() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal class CountContract {
            internal int Count => 1;
        }

        internal sealed class HiddenRead(List<int> values) : CountContract {
            internal new int Count => values.Count;
        }
        """);

    [Fact]
    public Task PrivateAliases() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal sealed class PrivateAliases(FileInfo file) {
            private readonly FileInfo Source = file;

            private string {|#0:Name|} => Source.Name;

            private long {|#1:Length|} => this.Source.Length;

            private string ShadowedName => Source.Name;

            private string NamedName => Source.Name;

            internal string ReadName() => Name;

            internal long ReadLength() => Length;

            internal string ReadShadowed(FileInfo Source) => ShadowedName + Source.Name;

            internal string ReadNamed() => NamedName + nameof(NamedName);
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Name", "Source.Name"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Length", "this.Source.Length"));

    [Fact]
    public Task RestrictedReads() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal class RestrictedReads(FileInfo file) {
            internal FileInfo Shared { get; } = file;

            protected FileInfo Protected { get; } = file;

            public FileInfo Written { private get; set; } = file;

            internal string SharedName => Shared.Name;

            internal string ProtectedName => Protected.Name;

            internal string WrittenName => Written.Name;
        }
        """);

    [Fact]
    public Task ConstructedReads() => VerifyAnalyzerAsync(
        """
        public static class GenericReads<T> {
            public static int Count => typeof(T).GetFields().Length;
        }

        internal sealed class HiddenType;

        internal static class ConstructedReads {
            internal static int {|#0:Public|} => GenericReads<string>.Count;

            internal static int Hidden => GenericReads<HiddenType>.Count;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Public", "GenericReads<string>.Count"));

    [Fact]
    public Task Entries() => VerifyAnalyzerAsync(
        """
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
            private static double {|#0:MissingNumber|} => RhinoMath.UnsetValue;
            internal static Entry Empty() => new(Id.Empty);
            internal static Entry Default() => new(default);
            internal static Entry Constructed() => new(new Id());
            internal static Entry Aliased() => new(Missing);
            internal static DateEntry Minimum() => new(DateTime.MinValue);
            internal static DateEntry DefaultDate() => new(new DateTime());
            internal static NumericEntry Unknown() => new(double.NaN);
            internal static WideEntry Positional() => new(1, 2, 3, 4, 5, 6, 7, 8, default);
            internal static WideEntry Named() => new(Key: default, First: 1, Second: 2, Third: 3, Fourth: 4, Fifth: 5, Sixth: 6, Seventh: 7, Eighth: 8);
            internal static TextEntry EmptyText() => new("");
            internal static TextEntry MissingText(string? text) => new(text ?? "");
            internal static TextEntry OptionalText(Option<string> text) => new(text.IfNone(""));
            internal static Entry Replaced(Entry entry) => entry with { Key = Id.Empty };
            internal static Entry Initialized(Id key) => new(key) { Key = Id.Empty };
            internal static bool MissingValue(TextEntry entry) => string.IsNullOrEmpty(entry.Text);
            internal static LiteralEntry Literal() => new("");
            internal static LiteralEntry DefaultedDisplay(string? text) => new(text ?? "");
            internal static LiteralEntry OptionalDisplay(Option<string> text) => new(text.IfNone(""));
            internal static LiteralEntry MatchedDisplay(Option<string> text) => new(text.Match(None: () => "", Some: value => value.Trim()));
            internal static CountEntry Zero() => new(0);
            internal static Callback<double> DeferredNumber() => new(() => double.NaN);
            internal static Callback<Id> DeferredKey() => new(() => Id.Empty);
            internal static Entry Present(Id key) => new(key);
            internal static OffsetEntry Offset() => new(DateTimeOffset.MinValue);
            internal static NumericEntry SingleUnknown() => new(float.NaN);
            internal static TextEntry Matched(Option<string> text) => new(text.Match(None: () => "", Some: value => value.Trim()));
            internal static Entry Conditional(bool present, Id key) => new(present ? key : Id.Empty);
            internal static NumericEntry Unset() => new(Point3d.Unset.X);
            internal static NumericEntry Ordinary() => new(Unsettled);
            internal static Entry Reassigned() {
                Id held = Id.Empty;
                held = Id.NewGuid();
                return new(held);
            }
            internal static Entry StoredBefore() {
                Id held = Id.Empty;
                Entry first = new(held);
                held = Id.NewGuid();
                return first;
            }
            internal static ReplacedText Unstored() => new("");
            internal static StoredText Stored() => new("");
            internal static NumericEntry UnsetNumber() => new(RhinoMath.UnsetValue);
            internal static NumericEntry NativeAlias() => new(MissingNumber);
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
        Diagnostic("RASM3012").WithLocation(0).WithArguments("MissingNumber", "RhinoMath.UnsetValue"));
}
