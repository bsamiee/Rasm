using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CapturedWriteTests : CSharpCodeFixVerifier<CapturedWriteAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task CallbackValues() => VerifyAnalyzerAsync(
        """
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using LanguageExt;
        using HashSet = System.Collections.Generic.HashSet<int>;

        internal static class CallbackValues {
            internal static List<int> Escaped(Seq<int> values) {
                List<int> {|#0:cell|} = new();
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static List<int> Nonwriting(Seq<int> values) {
                List<int> {|#1:cell|} = new();
                values.Iter(value => cell.Add(value));
                values.Iter(value => Consume(cell.Count));
                return new();
            }

            internal static void OtherCell(Seq<int> values) {
                List<int> {|#2:cell|} = new();
                List<int> other = new();
                values.Iter(value => cell.Add(value));
                values.Iter(value => { other.Add(value); Consume(cell.Count); });
            }

            internal static void NestedNonwriting(Seq<int> values) {
                List<int> {|#3:cell|} = new();
                List<int> other = new();
                values.Iter(value => cell.Add(value));
                values.Iter(value => { other.Add(value); values.Iter(inner => Consume(cell.Count)); });
            }

            internal static void Enclosing(Seq<int> values) => values.Iter(value => {
                List<int> {|#4:cell|} = new();
                values.Iter(inner => cell.Add(inner));
                Consume(cell.Count);
            });

            internal static List<int> Helper(Seq<int> values) {
                List<int> {|#5:cell|} = new();
                void Write(int value) => cell.Add(value);
                values.Iter(Write);
                return cell;
            }

            internal static Seq<int> Query(Seq<Atom<int>> cells) =>
                from Atom<int> {|#6:cell|} in cells
                let changed = cell.Swap(value => value + 1)
                select cell.Value;

            internal static void NamedValue(List<int> items) {
                List<int> {|#7:value|} = new();
                items.ForEach(item => value.Add(item));
                Consume(value: value.Count);
            }

            internal static int Aliased(Seq<int> values) {
                List<int> {|#8:cell|} = new();
                List<int> {|#9:alias|} = cell;
                values.Iter(value => alias.Add(value));
                return cell.Count;
            }

            internal static int[] Indexed(Seq<int> values) {
                int[] {|#10:cell|} = new int[1];
                values.Iter(value => cell[0] = value);
                return cell;
            }

            internal static int Assigned(Seq<int> values) {
                int {|#11:result|} = 0;
                values.Iter(value => result = value);
                return result;
            }

            internal static int Out(Seq<int> values) {
                int {|#12:result|};
                result = 0;
                values.Iter(value => Set(value, out result));
                return result;
            }

            internal static (int, string) Tuple(Seq<int> values) {
                int {|#13:count|} = 0;
                string {|#14:text|} = "";
                values.Iter(value => { (count, text) = (value, value.ToString()); });
                return (count, text);
            }

            internal static IO<List<int>> Lifted(Seq<int> values) => IO.lift(() => {
                List<int> {|#15:cell|} = new();
                values.Iter(value => cell.Add(value));
                return cell;
            });

            internal static Seq<int> QueryAlias(Seq<Atom<int>> cells) =>
                from Atom<int> {|#16:cell|} in cells
                let {|#17:alias|} = cell
                let changed = alias.Swap(value => value + 1)
                select cell.Value;

            internal static ICollection<int> Collected(Seq<int> values) {
                ICollection<int> {|#18:cell|} = new List<int>();
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static IList<int> Listed(Seq<int> values) {
                IList<int> {|#19:cell|} = new List<int> { 0 };
                values.Iter(value => cell[0] = value);
                return cell;
            }

            internal static IDictionary<int, int> Added(Seq<int> values) {
                IDictionary<int, int> {|#20:cell|} = new Dictionary<int, int>();
                values.Iter(value => cell.Add(value, value));
                return cell;
            }

            internal static IDictionary<int, int> DictionaryItems(Seq<int> values) {
                IDictionary<int, int> {|#21:cell|} = new Dictionary<int, int>();
                values.Iter(value => cell[value] = value);
                return cell;
            }

            internal static ISet<int> Members(Seq<int> values) {
                ISet<int> {|#22:cell|} = new HashSet();
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static IList UntypedList(Seq<int> values) {
                IList {|#23:cell|} = new ArrayList();
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static IDictionary UntypedDictionary(Seq<int> values) {
                IDictionary {|#24:cell|} = new Hashtable();
                values.Iter(value => cell.Add(value, value));
                return cell;
            }

            internal static ImmutableArray<int>.Builder ArrayBuilder(Seq<int> values) {
                ImmutableArray<int>.Builder {|#25:cell|} = ImmutableArray.CreateBuilder<int>();
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static ImmutableList<int>.Builder ListBuilder(Seq<int> values) {
                ImmutableList<int>.Builder {|#26:cell|} = ImmutableList.CreateBuilder<int>();
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static List<int> Casted(Seq<int> values) {
                List<int> {|#27:cell|} = new();
                values.Iter(value => ((ICollection<int>)cell).Add(value));
                return cell;
            }

            internal static List<int>? Conditional(Seq<int> values) {
                List<int>? {|#28:cell|} = new();
                values.Iter(value => cell?.Add(value));
                return cell;
            }

            internal static void Consume(int value) { }

            internal static void Set(int value, out int result) => result = value;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("cell"),
        Diagnostic().WithLocation(1).WithArguments("cell"),
        Diagnostic().WithLocation(2).WithArguments("cell"),
        Diagnostic().WithLocation(3).WithArguments("cell"),
        Diagnostic().WithLocation(4).WithArguments("cell"),
        Diagnostic().WithLocation(5).WithArguments("cell"),
        Diagnostic().WithLocation(6).WithArguments("cell"),
        Diagnostic().WithLocation(7).WithArguments("value"),
        Diagnostic().WithLocation(8).WithArguments("cell"),
        Diagnostic().WithLocation(9).WithArguments("alias"),
        Diagnostic().WithLocation(10).WithArguments("cell"),
        Diagnostic().WithLocation(11).WithArguments("result"),
        Diagnostic().WithLocation(12).WithArguments("result"),
        Diagnostic().WithLocation(13).WithArguments("count"),
        Diagnostic().WithLocation(14).WithArguments("text"),
        Diagnostic().WithLocation(15).WithArguments("cell"),
        Diagnostic().WithLocation(16).WithArguments("cell"),
        Diagnostic().WithLocation(17).WithArguments("alias"),
        Diagnostic().WithLocation(18).WithArguments("cell"),
        Diagnostic().WithLocation(19).WithArguments("cell"),
        Diagnostic().WithLocation(20).WithArguments("cell"),
        Diagnostic().WithLocation(21).WithArguments("cell"),
        Diagnostic().WithLocation(22).WithArguments("cell"),
        Diagnostic().WithLocation(23).WithArguments("cell"),
        Diagnostic().WithLocation(24).WithArguments("cell"),
        Diagnostic().WithLocation(25).WithArguments("cell"),
        Diagnostic().WithLocation(26).WithArguments("cell"),
        Diagnostic().WithLocation(27).WithArguments("cell"),
        Diagnostic().WithLocation(28).WithArguments("cell"));

    [Fact]
    public Task CallbackResults() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        using LanguageExt;

        internal static class CallbackResults {
            internal static ImmutableList<int> Persistent(Seq<int> values) {
                ImmutableList<int> cell = ImmutableList<int>.Empty;
                values.Iter(value => cell.Add(value));
                return cell;
            }

            internal static List<int> Converted(Seq<int> values) {
                CopyOnConversion cell = new(new List<int>());
                values.Iter(value => ((List<int>)cell).Add(value));
                return cell.Values;
            }

            internal static void Writing(Seq<int> values) {
                List<int> cell = new();
                values.Iter(value => cell.Add(value));
                values.Iter(value => { cell.Add(value); Consume(cell.Count); });
            }

            internal static void NestedWriting(Seq<int> values) {
                List<int> cell = new();
                values.Iter(value => cell.Add(value));
                values.Iter(value => { values.Iter(inner => cell.Add(inner)); Consume(cell.Count); });
            }

            internal static void NamedArgument(List<int> items) {
                List<int> action = new();
                items.ForEach(value => action.Add(value));
                items.ForEach(action: value => Consume(value));
            }

            internal static async Task<List<int>> Opaque(Seq<int> values) {
                List<int> cell = new();
                await Task.Run(() => cell.Add(1));
                return cell;
            }

            internal static List<int> Stored(Seq<int> values) {
                List<int> cell = new();
                Action write = () => cell.Add(1);
                write();
                return cell;
            }

            internal static int Transition(Atom<int> cell) {
                int result = 0;
                cell.Swap(value => { result = value; return value + 1; });
                return result;
            }

            internal static int Reassigned(Seq<int> values) {
                List<int> cell = new();
                List<int> alias = cell;
                alias = new();
                values.Iter(value => alias.Add(value));
                return cell.Count;
            }

            internal static int Disjoint(Seq<int> values, bool changed) {
                List<int> cell = new();
                if (changed) {
                    values.Iter(value => cell.Add(value));
                    return 0;
                }
                return cell.Count;
            }

            internal static void Consume(int value) { }
        }

        internal sealed class CopyOnConversion(List<int> values) {
            internal List<int> Values { get; } = values;

            public static implicit operator List<int>(CopyOnConversion source) => new(source.Values);
        }
        """);
}
