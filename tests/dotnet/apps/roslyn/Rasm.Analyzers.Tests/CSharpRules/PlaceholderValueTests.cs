using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlaceholderValueTests : CSharpCodeFixVerifier<PlaceholderValueAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "Packaged no-op delegates and disposables are reported; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseEventArgsEmptyAnalyzerTests.cs ShouldReport")]
    public Task PackagedNoOps() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Reactive.Disposables;
        using Thinktecture;

        internal static class PackagedNoOps {
            internal static Action Ignored => {|#0:Empty.Action|};

            internal static Action<int, string> Typed => {|#1:Empty.Action|};

            internal static IDisposable Released => {|#2:Empty.Disposable()|};

            internal static IAsyncDisposable ReleasedLater => {|#3:Empty.AsyncDisposable()|};

            internal static IDisposable Reactive => {|#4:Disposable.Empty|};

            internal static void Called() => {|#5:Thinktecture.Empty.Action()|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Empty.Action"),
        Diagnostic().WithLocation(1).WithArguments("Empty.Action"),
        Diagnostic().WithLocation(2).WithArguments("Empty.Disposable()"),
        Diagnostic().WithLocation(3).WithArguments("Empty.AsyncDisposable()"),
        Diagnostic().WithLocation(4).WithArguments("Disposable.Empty"),
        Diagnostic().WithLocation(5).WithArguments("Thinktecture.Empty.Action()"));

    [Fact(DisplayName = "An init member holding its default, empty collections, a made disposable, nameof, and a same-named type of another namespace are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnOtherMethodsWithSleepName")]
    public Task DeclaredDefaults() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using System.Reactive.Disposables;
        using Thinktecture;

        internal sealed record DeclaredDefaults {
            public Action<int> Changed { get; init; } = static _ => { };

            internal static IReadOnlyList<int> None => Empty.Collection<int>();

            internal static IEnumerable Untyped => Empty.Collection();

            internal static ILookup<string, int> Grouped => Empty.Lookup<string, int>();

            internal static IReadOnlyDictionary<string, int> Unkeyed => Empty.Dictionary<string, int>();

            internal static IReadOnlySet<int> Unique => Empty.Set<int>();

            internal static IDisposable Released(Action release) => Disposable.Create(release);

            internal static string Named => nameof(Empty.Action);

            internal static Action Local => Placeholders.Empty.Action;
        }

        namespace Placeholders {
            internal static class Empty {
                internal static void Action() { }
            }
        }
        """);
}
