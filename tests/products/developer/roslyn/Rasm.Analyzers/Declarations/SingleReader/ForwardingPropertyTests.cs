using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations.SingleReader;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ForwardingPropertyTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, EmptyCodeFixProvider> {

    // --- [VISIBILITY]

    [Fact]
    public Task ReportsForwardOfAMemberEveryReaderCanBindButNotOneAReaderCannotReach() => VerifyAnalyzerAsync(
        """
        using System.IO;

        public static class GenericReads<T> {
            public static int Count => typeof(T).GetFields().Length;
        }

        internal sealed class HiddenType;

        internal class Reads(FileInfo file) {
            private readonly FileInfo guarded = file;

            public FileInfo File { get; } = file;

            internal FileInfo Shared { get; } = file;

            protected FileInfo Protected { get; } = file;

            public FileInfo Written { private get; set; } = file;

            internal string {|#0:Name|} => File.Name;

            internal string {|#1:SharedName|} => Shared.Name;

            internal string GuardedName => guarded.Name;

            internal string ProtectedName => Protected.Name;

            internal string WrittenName => Written.Name;

            internal static int {|#2:Fields|} => GenericReads<string>.Count;

            internal static int HiddenFields => GenericReads<HiddenType>.Count;
        }

        internal static class Reader {
            internal static string Read(Reads reads) => reads.SharedName + reads.GuardedName + reads.ProtectedName;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Name", "File.Name"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("SharedName", "Shared.Name"),
        Diagnostic("RASM3012").WithLocation(2).WithArguments("Fields", "GenericReads<string>.Count"));

    // --- [BINDING]

    [Fact]
    public Task ReportsPrivateAliasButNotWhereAParameterShadowsItsTargetOrNameofNamesIt() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal sealed class PrivateAliases(FileInfo file) {
            private readonly FileInfo Source = file;

            private string {|#0:Name|} => Source.Name;

            private string ShadowedName => Source.Name;

            private string NamedName => Source.Name;

            internal string ReadName() => Name;

            internal string ReadShadowed(FileInfo Source) => ShadowedName + Source.Name;

            internal string ReadNamed() => NamedName + nameof(NamedName);
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Name", "Source.Name"));

    // --- [CONTRACTS]

    [Fact]
    public Task ReportsUnchangedReturnOfAnotherMemberButNotATransformOrContract() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.IO;

        internal sealed class Transforms(FileInfo file, List<int> values) {
            public FileInfo File { get; } = file;

            public List<int> Values { get; } = values;

            public Lazy<List<int>> Delayed { get; } = new(() => values);

            public UserDefined.Lazy<List<int>> Held { get; } = new(values);

            internal static DateTime {|#0:Utc|} => DateTime.UtcNow;

            internal static Version Placed => new(1, 0);

            internal string Upper => File.Name.ToUpperInvariant();

            internal int DelayedCount => Delayed.Value.Count;

            internal int {|#1:HeldCount|} => Held.Value.Count;

            internal Action {|#2:Clear|} => Values.Clear;
        }

        internal abstract class LengthContract {
            internal abstract long Length { get; }
        }

        internal sealed class Overridden(FileInfo file) : LengthContract {
            public FileInfo File { get; } = file;

            internal override long Length => File.Length;
        }

        internal class Extensible(List<int> values) {
            public List<int> Values { get; } = values;

            internal virtual int Count => Values.Count;

            internal int Capacity => 1;
        }

        internal sealed class Hiding(List<int> values) : Extensible(values) {
            internal new int Capacity => Values.Capacity;
        }

        internal sealed class CollectionView(List<int> values) : IReadOnlyCollection<int> {
            public List<int> Values { get; } = values;

            public int Count => Values.Count;

            public IEnumerator<int> GetEnumerator() => Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        namespace UserDefined {
            public sealed class Lazy<T>(T value) {
                public T Value { get; } = value;
            }
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Utc", "DateTime.UtcNow"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("HeldCount", "Held.Value.Count"),
        Diagnostic("RASM3012").WithLocation(2).WithArguments("Clear", "Values.Clear"));

    // --- [INDEXERS]

    [Fact]
    public Task ReportsIndexerPassingItsParametersInOrderButNotPermutedShiftedOrWritable() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal sealed class Listed(List<int> values) {
            public List<int> Values { get; } = values;

            internal int {|#0:this|}[int index] => Values[index];
        }

        internal sealed class Matrix(int[,] grid) {
            public int[,] Grid { get; } = grid;

            internal int {|#1:this|}[int row, int column] => Grid[row, column];
        }

        internal sealed class Transposed(int[,] grid) {
            public int[,] Grid { get; } = grid;

            internal int this[int row, int column] => Grid[column, row];
        }

        internal sealed class Shifted(List<int> values) {
            public List<int> Values { get; } = values;

            internal int this[int index] => Values[index + 1];
        }

        internal sealed class Writable(List<int> values) {
            public List<int> Values { get; } = values;

            internal int this[int index] {
                get => Values[index];
                set => Values[index] = value;
            }
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("this[]", "Values[index]"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("this[]", "Grid[row, column]"));
}
