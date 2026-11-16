using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RestatedInstanceTests : CSharpCodeFixVerifier<RestatedInstanceAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task Sizes() => VerifyAnalyzerAsync(
        """
        internal sealed record Factor(float Value) {
            public static Factor Unit { get; } = new(1f);

            public static Factor Create(float value) => new(value);

            public static Factor Increased(float value) => new(value + 1f);
        }

        internal sealed record Grow(Factor Factor) {
            public static readonly Grow Fill = new(Factor.Unit);
        }

        internal static class Sizes {
            internal static Grow Whole => {|#0:new Grow({|#1:Factor.Create(1f)|})|};

            internal static Factor One => {|#2:Factor.Create(1f)|};

            internal static Factor Half => Factor.Create(0.5f);

            internal static Grow Held => Grow.Fill;

            internal static Factor Increased => Factor.Increased(1f);

            internal static Factor Converted => {|#3:new Factor(1)|};
        }

        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("new Grow(Factor.Create(1f))", "Grow.Fill"),
        Diagnostic().WithLocation(1).WithArguments("Factor.Create(1f)", "Factor.Unit"),
        Diagnostic().WithLocation(2).WithArguments("Factor.Create(1f)", "Factor.Unit"),
        Diagnostic().WithLocation(3).WithArguments("new Factor(1)", "Factor.Unit"));

    [Fact]
    public Task Reordered() => VerifyAnalyzerAsync(
        """
        internal sealed record Dimensions(int Width, int Height) {
            internal static Dimensions Unit { get; } = new(1, 2);
        }

        internal static class Reordered {
            internal static Dimensions Unit => {|#0:new Dimensions(Height: 2, Width: 1)|};
        }

        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("new Dimensions(Height: 2, Width: 1)", "Dimensions.Unit"));

    [Fact]
    public Task DistinctOverloads() => VerifyAnalyzerAsync(
        """
        internal sealed record Overloaded {
            internal Overloaded(int value) => Value = value;

            internal Overloaded(object value) => Value = value;

            internal object Value { get; }

            internal static Overloaded One { get; } = new(1);

            internal static Overloaded Create(int value) => new((object)value);
        }

        internal static class DistinctOverloads {
            internal static Overloaded Boxed => new((object)1);

            internal static Overloaded Created => Overloaded.Create(1);
        }

        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }
        """);

    [Fact]
    public Task UnnamedCycle() => VerifyAnalyzerAsync(
        """
        internal sealed record Cycled(int Value) {
            internal static Cycled First => Second;

            internal static Cycled Second => First;
        }

        internal static class UnnamedCycle {
            internal static Cycled Zero => new(0);
        }

        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }
        """);

    [Fact]
    public Task DistinctTypes() => VerifyAnalyzerAsync(
        """
        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }

        internal sealed record Pair(object First, object Second) {
            internal static Pair Held { get; } = new(Left.Item.Zero, Left.Item.Zero);
        }

        internal static class DistinctTypes {
            internal static Pair Changed => new(Left.Item.Zero, new Right.Item(0));
        }
        """);

    [Fact]
    public Task DelimitedText() => VerifyAnalyzerAsync(
        """
        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }

        internal sealed record TextPair(string First, string Second) {
            internal static TextPair Held { get; } = new("a,string=b", "c");
        }

        internal static class DelimitedText {
            internal static TextPair Changed => new("a", "b,string=c");
        }
        """);

    [Fact]
    public Task DistinctNumbers() => VerifyAnalyzerAsync(
        """
        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }

        internal sealed record FloatFactor(float Value) {
            internal static FloatFactor Zero { get; } = new(0f);
        }

        internal sealed record DecimalFactor(decimal Value) {
            internal static DecimalFactor One { get; } = new(1m);
        }

        internal static class DistinctNumbers {
            internal static FloatFactor NegativeZero => new(-0f);

            internal static DecimalFactor ScaledOne => new(1.0m);
        }
        """);

    [Fact]
    public Task Rank() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }

        [SmartEnum]
        internal sealed partial class Rank {
            public static readonly Rank Current = new(shown: true);
            public static readonly Rank Visible = new(shown: true);

            public bool Shown { get; }
        }
        """);

    [Fact]
    public Task ConstructionEffects() => VerifyAnalyzerAsync(
        """
        namespace Left {
            internal sealed record Item(int Value) {
                internal static Item Zero { get; } = new(0);
            }
        }

        namespace Right {
            internal sealed record Item(int Value);
        }

        internal sealed class MutableFactor(int value) {
            internal static readonly MutableFactor Unit = new(1);

            public int Value { get; set; } = value;
        }

        internal sealed record CountedFactor {
            private static int count;

            internal static readonly CountedFactor Unit = new(1);

            internal CountedFactor(int value) {
                Value = value;
                ++count;
            }

            internal int Value { get; }
        }

        internal sealed record InitializedFactor(int Value) {
            private static int count;

            internal static readonly InitializedFactor Unit = new(1);

            internal int Ordinal { get; } = Next();

            internal static int Next() => ++count;
        }

        internal readonly struct ReadonlyFactor {
            internal static readonly ReadonlyFactor Unit = new(1);

            internal ReadonlyFactor(int value) => Value = value;

            internal int Value { get; }

            internal static ReadonlyFactor Create(int value) => new(value);
        }

        internal record BaseFactor(int Value);

        internal sealed record DerivedFactor(int Input) : BaseFactor(Count(Input)) {
            private static int calls;

            internal static readonly DerivedFactor Unit = new(1);

            private static int Count(int value) {
                ++calls;
                return value;
            }
        }

        internal static class ConstructionEffects {
            internal static MutableFactor Mutable => new(1);

            internal static CountedFactor Counted => new(1);

            internal static InitializedFactor Initialized => new(1);

            internal static DerivedFactor Derived => new(1);

            internal static ReadonlyFactor Readonly => {|#0:new(1)|};

            internal static ReadonlyFactor Factory => {|#1:ReadonlyFactor.Create(1)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("new(1)", "ReadonlyFactor.Unit"),
        Diagnostic().WithLocation(1).WithArguments("ReadonlyFactor.Create(1)", "ReadonlyFactor.Unit"));
}
