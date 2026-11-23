using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Expressions;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CapturedWriteTests : CSharpCodeFixVerifier<CapturedWriteAnalyzer, EmptyCodeFixProvider> {

    // --- [READS]

    [Fact]
    public Task ReportsCallbackWriteReadAfterTheCallbackOrThroughLiftedEffectAndIgnoresReadInsideTheSameCallback() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;
        using LanguageExt;

        internal static class Reads {
            internal static List<int> After(Seq<int> values) {
                List<int> {|#0:cell|} = new();
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static void InReadingCallback(Seq<int> values) {
                List<int> {|#1:cell|} = new();
                values.Iter(value => cell.Add(value));
                values.Iter(value => Consume(cell.Count));
            }

            internal static IO<List<int>> Lifted(Seq<int> values) => IO.lift(() => {
                List<int> {|#2:cell|} = new();
                values.Iter(value => cell.Add(value));
                return cell;
            });

            internal static void SameCallback(Seq<int> values) {
                List<int> cell = new();
                values.Iter(value => { cell.Add(value); Consume(cell.Count); });
            }

            internal static void Consume(int value) { }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("cell"),
        Diagnostic().WithLocation(1).WithArguments("cell"),
        Diagnostic().WithLocation(2).WithArguments("cell"));

    // --- [ALIASES]

    [Fact]
    public Task ReportsWriteThroughAliasOrReferenceCastAndIgnoresCopyReassignedAliasAndPersistentAdd() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using LanguageExt;

        internal static class Aliases {
            internal static int Aliased(Seq<int> values) {
                List<int> {|#0:cell|} = new();
                List<int> {|#1:alias|} = cell;
                values.Iter(value => alias.Add(value));
                return cell.Count;
            }

            internal static List<int> Cast(Seq<int> values) {
                List<int> {|#2:cell|} = new();
                values.Iter(value => ((ICollection<int>)cell).Add(value));
                return cell;
            }

            internal static List<int> Copied(Seq<int> values) {
                CopyOnConversion cell = new(new List<int>());
                values.Iter(value => ((List<int>)cell).Add(value));
                return cell.Values;
            }

            internal static int Reassigned(Seq<int> values) {
                List<int> cell = new();
                List<int> alias = cell;
                alias = new();
                values.Iter(value => alias.Add(value));
                return cell.Count;
            }

            internal static ImmutableList<int> Persistent(Seq<int> values) {
                ImmutableList<int> cell = ImmutableList<int>.Empty;
                values.Iter(value => cell.Add(value));
                return cell;
            }
        }

        internal sealed class CopyOnConversion(List<int> values) {
            internal List<int> Values { get; } = values;

            public static implicit operator List<int>(CopyOnConversion source) => new(source.Values);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("cell"),
        Diagnostic().WithLocation(1).WithArguments("alias"),
        Diagnostic().WithLocation(2).WithArguments("cell"));

    // --- [PATHS]

    [Fact]
    public Task ReportsCapturedAssignmentInsideACombinatorAndIgnoresDisjointPathsAndOtherCallbacks() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using LanguageExt;

        internal static class Paths {
            internal static int Assigned(Seq<int> values) {
                int {|#0:result|} = 0;
                values.Iter(value => result = value);
                return result;
            }

            internal static int Disjoint(Seq<int> values, bool changed) {
                List<int> cell = new();
                if (changed) {
                    values.Iter(value => cell.Add(value));
                    return 0;
                }
                return cell.Count;
            }

            internal static async Task<List<int>> Scheduled() {
                List<int> cell = new();
                await Task.Run(() => cell.Add(1));
                return cell;
            }

            internal static List<int> Stored() {
                List<int> cell = new();
                Action write = () => cell.Add(1);
                write();
                return cell;
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("result"));

    // --- [QUERIES]

    [Fact]
    public Task ReportsRangeVariableMutatedInLetAndIgnoresLetThatOnlyReads() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class Queries {
            internal static Seq<int> Direct(Seq<Atom<int>> cells) =>
                from Atom<int> {|#0:cell|} in cells
                let changed = cell.Swap(value => value + 1)
                select cell.Value;

            internal static Seq<int> Aliased(Seq<Atom<int>> cells) =>
                from Atom<int> {|#1:cell|} in cells
                let {|#2:alias|} = cell
                let changed = alias.Swap(value => value + 1)
                select cell.Value;

            internal static Seq<int> Read(Seq<Atom<int>> cells) =>
                from Atom<int> cell in cells
                let current = cell.Value
                select current + 1;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("cell"),
        Diagnostic().WithLocation(1).WithArguments("cell"),
        Diagnostic().WithLocation(2).WithArguments("alias"));

    // --- [MUTATIONS]

    [Fact]
    public Task ReportsIndexerAndArrayElementWritesAsMutationsAndIgnoresIndexerRead() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;
        using LanguageExt;

        internal static class Mutations {
            internal static IList<int> Indexed(Seq<int> values) {
                IList<int> {|#0:cell|} = new List<int> { 0 };
                values.Iter(value => cell[0] = value);
                return cell;
            }

            internal static int[] Element(Seq<int> values) {
                int[] {|#1:cell|} = new int[1];
                values.Iter(value => cell[0] = value);
                return cell;
            }

            internal static Seq<int> Read(Seq<int> values) {
                IList<int> cell = new List<int> { 0 };
                Seq<int> shifted = values.Map(value => cell[0] + value);
                return cell.Count > 0 ? shifted : values;
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("cell"),
        Diagnostic().WithLocation(1).WithArguments("cell"));
}
