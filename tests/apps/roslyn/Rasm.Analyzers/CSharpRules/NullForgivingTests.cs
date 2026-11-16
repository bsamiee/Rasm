using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class NullForgivingTests : CSharpCodeFixVerifier<NullForgivingAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ForgivenCallbackResult() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public sealed record Owned<T>(T Value, EnvIO Scope);

            public static class Products {
                public static IO<T> Of<T>(Func<T?> call) where T : class => IO.lift(() => Optional(call()).ToFin(Error.New("missing")));
                public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to) where T : class, IDisposable => IO.lift(() => {|#0:make()!|}).Bind(to);
                public static IO<Unit> Released(Action release) => IO.lift(release);
                public static IO<T> Replace<T>(Ref<Option<Owned<T>>> slot, IO<T> body) => body;
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("make()!"));
}
