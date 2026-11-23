using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RestatedInstanceTests : CSharpCodeFixVerifier<RestatedInstanceAnalyzer, EmptyCodeFixProvider> {

    // --- [VALUES]

    [Fact]
    public Task ReportsConstructionEqualToAStaticInstanceEvenOnARecordTheSmartEnumGeneratorSkipsButNotOneDifferingInValueTypeOrOverload() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        internal sealed record Factor(float Value) {
            public static Factor Unit { get; } = new(1f);

            public static Factor Create(float value) => new(value);

            public static Factor Increased(float value) => new(value + 1f);
        }

        internal sealed record Grow(Factor Factor) {
            public static readonly Grow Fill = new(Factor.Unit);
        }

        internal sealed record Dimensions(int Width, int Height) {
            internal static Dimensions Unit { get; } = new(1, 2);
        }

        internal sealed record FloatFactor(float Value) {
            internal static FloatFactor Zero { get; } = new(0f);
        }

        internal sealed record DecimalFactor(decimal Value) {
            internal static DecimalFactor One { get; } = new(1m);
        }

        internal sealed record Overloaded {
            internal Overloaded(int value) => Value = value;

            internal Overloaded(object value) => Value = value;

            internal object Value { get; }

            internal static Overloaded One { get; } = new(1);
        }

        internal sealed record Pair(object First, object Second) {
            internal static Pair Held { get; } = new(Left.Item.Zero, Left.Item.Zero);
        }

        internal sealed record TextPair(string First, string Second) {
            internal static TextPair Held { get; } = new("a,string=b", "c");
        }

        [SmartEnum]
        internal sealed partial record Skipped(int Value) {
            public static readonly Skipped First = {|#5:new(1)|};

            public static readonly Skipped Second = {|#6:new(1)|};
        }

        internal sealed record Cycled(int Value) {
            internal static Cycled First { get; } = {|CS8601:Second|};

            internal static Cycled Second { get; } = {|CS8601:First|};
        }

        internal static class Sites {
            internal static Factor One => {|#0:Factor.Create(1f)|};

            internal static Factor Converted => {|#1:new Factor(1)|};

            internal static Grow Whole => {|#2:new Grow({|#3:Factor.Create(1f)|})|};

            internal static Dimensions Reordered => {|#4:new Dimensions(Height: 2, Width: 1)|};

            internal static Factor Half => Factor.Create(0.5f);

            internal static Factor Increased => Factor.Increased(1f);

            internal static FloatFactor NegativeZero => new(-0f);

            internal static DecimalFactor ScaledOne => new(1.0m);

            internal static Overloaded Boxed => new((object)1);

            internal static Pair Changed => new(Left.Item.Zero, new Right.Item(0));

            internal static TextPair Delimited => new("a", "b,string=c");

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
        """,
        Diagnostic().WithLocation(0).WithArguments("Factor.Create(1f)", "Factor.Unit"),
        Diagnostic().WithLocation(1).WithArguments("new Factor(1)", "Factor.Unit"),
        Diagnostic().WithLocation(2).WithArguments("new Grow(Factor.Create(1f))", "Grow.Fill"),
        Diagnostic().WithLocation(3).WithArguments("Factor.Create(1f)", "Factor.Unit"),
        Diagnostic().WithLocation(4).WithArguments("new Dimensions(Height: 2, Width: 1)", "Dimensions.Unit"),
        Diagnostic().WithLocation(5).WithArguments("new(1)", "Skipped.Second"),
        Diagnostic().WithLocation(6).WithArguments("new(1)", "Skipped.First"));

    // --- [EFFECTS]

    [Fact]
    public Task ReportsReadonlyConstructionButNotOneWhoseConstructionHasAnEffect() => VerifyAnalyzerAsync(
        """
        internal sealed class PlainFactor(int value) {
            internal static readonly PlainFactor Unit = new(1);

            public int Value { get; } = value;
        }

        internal sealed record MutableFactor(int Value) {
            internal static readonly MutableFactor Unit = new(1);

            public int Value { get; set; } = Value;
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

        internal record BaseFactor(int Value);

        internal sealed record DerivedFactor(int Input) : BaseFactor(Count(Input)) {
            private static int calls;

            internal static readonly DerivedFactor Unit = new(1);

            private static int Count(int value) {
                ++calls;
                return value;
            }
        }

        internal readonly struct ReadonlyFactor {
            internal static readonly ReadonlyFactor Unit = new(1);

            internal ReadonlyFactor(int value) => Value = value;

            internal int Value { get; }
        }

        internal static class ConstructionEffects {
            internal static PlainFactor Plain => new(1);

            internal static MutableFactor Mutable => new(1);

            internal static CountedFactor Counted => new(1);

            internal static InitializedFactor Initialized => new(1);

            internal static DerivedFactor Derived => new(1);

            internal static ReadonlyFactor Readonly => {|#0:new(1)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("new(1)", "ReadonlyFactor.Unit"));
}
