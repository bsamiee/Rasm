using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class MapFlattenTests : CSharpCodeFixVerifier<MapFlattenAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsOneArgumentMapThenFlattenAndIgnoresIndexedMap() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class Flattened {
            internal static Fin<int> Qualified(Fin<string> text) => {|RASM1012:FinExtensions.Flatten(text.Map(Parse))|};
            internal static Seq<int> Expanded(Seq<int> counts) => {|RASM1012:counts.Map(static count => Seq(count, count)).Flatten()|};
            internal static Seq<int> Indexed(Seq<int> counts) => counts.Map(static (count, index) => Seq(count, index)).Flatten();

            private static Fin<int> Parse(string text) => text.Length;
        }
        """);

    [Fact]
    public Task ReportsLanguageExtMapThenFlattenAndIgnoresFlattenOverAnotherLibrarysMap() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using Local;

        internal static class Owned {
            internal static Option<int> Found(Option<string> key) => {|RASM1012:key.Map(static held => Find(held)).Flatten()|};
            internal static Option<int> Boxed(Box<string> box) => box.Map(static held => Find(held)).Flatten();

            private static Option<int> Find(string key) => key.Length;
        }

        namespace Local {
            internal sealed record Box<T>(T Value) {
                internal Option<R> Map<R>(Func<T, R> step) => step(Value);
            }
        }
        """);
}
