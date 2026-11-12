using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ValueFactoryOverloadTests : CSharpCodeFixVerifier<ValueFactoryOverloadAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "An overload set taking one method type parameter as a value and as a factory is reported at the value overload; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/MethodOverridesShouldNotChangeParameterDefaultsAnalyzerTests.cs Override_DifferentValue")]
    public Task ValueAndFactory() => VerifyAnalyzerAsync(
        """
        using System;

        internal static class ValueAndFactory {
            internal static T {|#0:Recovered|}<T>(T fallback, Func<T, T> mapped) => mapped(fallback);

            internal static T {|#1:Recovered|}<T>(Func<T> fallback, Func<T, T> mapped) => mapped(fallback());

            internal static S {|#2:Folded|}<S, T>(T item, S seed, Func<S, T, S> step) => step(seed, item);

            internal static S {|#3:Folded|}<S, T>(T item, Func<S> made, Func<S, T, S> step) => step(made(), item);

            internal static S Folded<S, T>(T item, Func<S, T, S> step) where S : new() => step(new S(), item);
        }

        internal sealed class Instance {
            internal T {|#4:Chosen|}<T>(Func<T, T> kept, T otherwise) => kept(otherwise);

            internal T {|#5:Chosen|}<T>(Func<T, T> kept, Func<T> otherwise) => kept(otherwise());
        }
        """,
        Diagnostic().WithLocation(0).WithLocation(1).WithArguments("ValueAndFactory.Recovered", "fallback", "fallback"),
        Diagnostic().WithLocation(2).WithLocation(3).WithArguments("ValueAndFactory.Folded", "seed", "made"),
        Diagnostic().WithLocation(4).WithLocation(5).WithArguments("Instance.Chosen", "otherwise", "otherwise"));

    [Fact(DisplayName = "The declaring interface or base is reported, its implementations and overrides are clean; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/MethodOverridesShouldNotChangeParameterDefaultsAnalyzerTests.cs Override_DifferentValue and Interface_ExplicitImplementation")]
    public Task DeclaredOnce() => VerifyAnalyzerAsync(
        """
        using System;

        internal interface IRecovery {
            T {|#0:Recovered|}<T>(T fallback, Func<T, T> mapped);

            T {|#1:Recovered|}<T>(Func<T> fallback, Func<T, T> mapped);
        }

        internal sealed class Implicit : IRecovery {
            public T Recovered<T>(T fallback, Func<T, T> mapped) => mapped(fallback);

            public T Recovered<T>(Func<T> fallback, Func<T, T> mapped) => mapped(fallback());
        }

        internal sealed class Explicit : IRecovery {
            T IRecovery.Recovered<T>(T fallback, Func<T, T> mapped) => mapped(fallback);

            T IRecovery.Recovered<T>(Func<T> fallback, Func<T, T> mapped) => mapped(fallback());
        }

        internal abstract class Base {
            internal abstract T {|#2:Recovered|}<T>(T fallback, Func<T, T> mapped);

            internal abstract T {|#3:Recovered|}<T>(Func<T> fallback, Func<T, T> mapped);
        }

        internal sealed class Derived : Base {
            internal override T Recovered<T>(T fallback, Func<T, T> mapped) => mapped(fallback);

            internal override T Recovered<T>(Func<T> fallback, Func<T, T> mapped) => mapped(fallback());
        }
        """,
        Diagnostic().WithLocation(0).WithLocation(1).WithArguments("IRecovery.Recovered", "fallback", "fallback"),
        Diagnostic().WithLocation(2).WithLocation(3).WithArguments("Base.Recovered", "fallback", "fallback"));

    [Fact(DisplayName = "Sets with a second delegate in neither or one overload, no method type parameter, another position, or a factory taking arguments are clean; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/MethodOverridesShouldNotChangeParameterDefaultsAnalyzerTests.cs Override_SameValue")]
    public Task SingleForm() => VerifyAnalyzerAsync(
        """
        using System;

        internal static class SingleForm {
            internal static T Defaulted<T>(T fallback) => fallback;

            internal static T Defaulted<T>(Func<T> fallback) => fallback();

            internal static T Lone<T>(T fallback, Func<T, T> mapped) => mapped(fallback);

            internal static T Lone<T>(Func<T> fallback) => fallback();

            internal static int Counted(int fallback, Func<int, int> mapped) => mapped(fallback);

            internal static int Counted(Func<int> fallback, Func<int, int> mapped) => mapped(fallback());

            internal static T Shifted<T>(T fallback, Func<T, T> mapped) => mapped(fallback);

            internal static T Shifted<T>(Func<T, T> mapped, Func<T> fallback) => mapped(fallback());

            internal static T Keyed<T>(T fallback, Func<T, T> mapped) => mapped(fallback);

            internal static T Keyed<T>(Func<int, T> fallback, Func<T, T> mapped) => mapped(fallback(0));
        }

        internal sealed class Box<T> {
            internal T Recovered(T fallback, Func<T, T> mapped) => mapped(fallback);

            internal T Recovered(Func<T> fallback, Func<T, T> mapped) => mapped(fallback());
        }
        """);
}
