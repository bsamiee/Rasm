using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class MapFlattenTests : CSharpCodeFixVerifier<MapFlattenAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "Map over a one-parameter function followed by Flatten is reported on Fin, Option, IO, and Seq; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldReportError")]
    public Task MapThenFlatten() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class MapThenFlatten {
            internal static Fin<int> Parsed(Fin<string> text) => {|#0:text.Map(Parse).Flatten()|};

            internal static Option<int> Found(Option<string> key) => {|#1:key.Map(static held => Find(held)).Flatten()|};

            internal static IO<int> Loaded(IO<string> path) => {|#2:path.Map(Load).Flatten()|};

            internal static Seq<int> Expanded(Seq<int> counts) => {|#3:counts.Map(static count => Seq(count, count)).Flatten()|};

            internal static Fin<int> Qualified(Fin<string> text) => {|#4:FinExtensions.Flatten(text.Map(Parse))|};

            private static Fin<int> Parse(string text) => text.Length;

            private static Option<int> Find(string key) => key.Length;

            private static IO<int> Load(string path) => IO.pure(path.Length);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("text.Map(Parse).Flatten()"),
        Diagnostic().WithLocation(1).WithArguments("key.Map(static held => Find(held)).Flatten()"),
        Diagnostic().WithLocation(2).WithArguments("path.Map(Load).Flatten()"),
        Diagnostic().WithLocation(3).WithArguments("counts.Map(static count => Seq(count, count)).Flatten()"),
        Diagnostic().WithLocation(4).WithArguments("FinExtensions.Flatten(text.Map(Parse))"));

    [Fact(DisplayName = "Bind, a lone Map or Flatten, an indexed Map, and Map and Flatten of another assembly are clean; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldNotReportError")]
    public Task BoundOnce() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using Local;
        using static LanguageExt.Prelude;

        internal static class BoundOnce {
            internal static Fin<int> Parsed(Fin<string> text) => text.Bind(Fin<int> (held) => Parse(held));

            internal static Fin<int> Joined(Fin<Fin<int>> nested) => nested.Flatten();

            internal static Fin<Fin<int>> Mapped(Fin<string> text) => text.Map(Parse);

            internal static Seq<int> Indexed(Seq<int> counts) => counts.Map(static (count, index) => Seq(count, index)).Flatten();

            internal static IO<Seq<int>> Chunked(IO<Seq<Seq<int>>> chunks) => chunks.Map(static held => held.Flatten());

            internal static Box<int> Custom(Box<int> box) => box.Map(static held => new Box<int>(held)).Flatten();

            private static Fin<int> Parse(string text) => text.Length;
        }

        namespace Local {
            internal sealed record Box<T>(T Value) {
                internal Box<R> Map<R>(Func<T, R> step) => new(step(Value));
            }

            internal static class Boxes {
                internal static Box<T> Flatten<T>(this Box<Box<T>> nested) => nested.Value;
            }
        }
        """);
}
