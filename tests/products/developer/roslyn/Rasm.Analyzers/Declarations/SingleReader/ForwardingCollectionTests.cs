using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations.SingleReader;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ForwardingCollectionTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {
    [Fact]
    public Task IgnoresIndexerOverPrimaryConstructorArray() => VerifyAnalyzerAsync(
        """
        internal sealed class ArrayReads(int[] values) {
            internal int this[int index] => values[index];
        }
        """);

    [Fact]
    public Task IgnoresIndexerOverPrimaryConstructorMatrix() => VerifyAnalyzerAsync(
        """
        internal sealed class MatrixReads(int[,] values) {
            internal int this[int row, int column] {
                get { return values[row, column]; }
            }
        }
        """);

    [Fact]
    public Task IgnoresIndexerAndMethodGroupOverPrimaryConstructorList() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;

        internal sealed class ListReads(List<int> values) {
            internal int this[int index] => values[index];

            internal Action Clear => values.Clear;
        }
        """);

    [Fact]
    public Task ReportsIndexerForwardingPublicArrayProperty() => VerifyAnalyzerAsync(
        """
        internal sealed class PublicArrayReads(int[] values) {
            public int[] Values { get; } = values;

            internal int {|#0:this|}[int index] => Values[index];
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("this[]", "Values[index]"));

    [Fact]
    public Task ReportsIndexerForwardingPublicMatrixProperty() => VerifyAnalyzerAsync(
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
    public Task ReportsIndexerAndMethodGroupForwardingPublicListProperty() => VerifyAnalyzerAsync(
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
    public Task IgnoresInterfaceMembersOverPrimaryConstructorList() => VerifyAnalyzerAsync(
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
    public Task ReportsPublicPropertyForwardingPublicListProperty() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal sealed class PublicCollectionReads(List<int> values) {
            public List<int> Values { get; } = values;

            public int {|#0:Capacity|} => Values.Capacity;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Capacity", "Values.Capacity"));

    [Fact]
    public Task ReportsUserLazyAndPrivateParameterForwardButNotLibraryLazyOrAtom() => VerifyAnalyzerAsync(
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
    public Task IgnoresReadsWithTransformedOrConstantIndex() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal sealed class TransformedReads(List<int> values) {
            internal int this[int index] => values[index + 1];

            internal int First => values[0];
        }
        """);

    [Fact]
    public Task IgnoresIndexerWithPermutedArguments() => VerifyAnalyzerAsync(
        """
        internal sealed class PermutedReads(int[,] values) {
            internal int this[int row, int column] => values[column, row];
        }
        """);

    [Fact]
    public Task IgnoresIndexerWithSetter() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal sealed class WritableReads(List<int> values) {
            internal int this[int index] {
                get => values[index];
                set => values[index] = value;
            }
        }
        """);
}
