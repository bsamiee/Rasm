using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations.SingleReader;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class InlinedMemberTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {

    // --- [INLINING]

    [Fact]
    public Task InlinesMethodIntoLocalOfItsNameButNotWhereSubstitutionDiffers() => VerifyCodeFixAsync(
        """
        internal static class Substituted {
            internal static int Head(int value) => Middle(value) + 1;

            internal static int Run(int value) => Combined(value, 2) + 1;

            internal static int Twofold(int value) {
                int doubled = Substituted.Doubled(value);
                return doubled + 1;
            }

            internal static int Squared(int value) => Square(value + 1);

            internal static double Halved(int count) => Half(count);

            internal static int Started(int value) => Ping(value) + Pong(value);

            internal static int Blocked(int value) => Block(value);

            internal static T First<T>(T[] values) => Head(values);

            private static int Middle(int value) => Tail(value) * 2;

            private static int Tail(int value) => value - 1;

            private static int Combined(int value, int factor) =>
                (value * factor)
                + value;

            private static int {|#0:Doubled|}(int value) => value * 2;

            private static int Square(int value) => value * value;

            private static double Half(double value) => value / 2;

            private static int Ping(int value) => value > 0 ? Pong(value - 1) : value;

            private static int Pong(int value) => value > 0 ? Ping(value - 1) : value;

            private static int Block(int value) {
                int doubled = value * 2;
                return doubled + 1;
            }

            private static T Head<T>(T[] values) => values[0];
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Doubled", "Twofold").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        internal static class Substituted {
            internal static int Head(int value) => Middle(value) + 1;

            internal static int Run(int value) => Combined(value, 2) + 1;

            internal static int Twofold(int value) {
                int doubled = value * 2;
                return doubled + 1;
            }

            internal static int Squared(int value) => Square(value + 1);

            internal static double Halved(int count) => Half(count);

            internal static int Started(int value) => Ping(value) + Pong(value);

            internal static int Blocked(int value) => Block(value);

            internal static T First<T>(T[] values) => Head(values);

            private static int Middle(int value) => Tail(value) * 2;

            private static int Tail(int value) => value - 1;

            private static int Combined(int value, int factor) =>
                (value * factor)
                + value;

            private static int Square(int value) => value * value;

            private static double Half(double value) => value / 2;

            private static int Ping(int value) => value > 0 ? Pong(value - 1) : value;

            private static int Pong(int value) => value > 0 ? Ping(value - 1) : value;

            private static int Block(int value) {
                int doubled = value * 2;
                return doubled + 1;
            }

            private static T Head<T>(T[] values) => values[0];
        }
        """);

    [Fact]
    public Task InlinesPropertyAndMethodGroupIntoLocalsOfTheirName() => VerifyCodeFixAsync(
        """
        using System;
        using System.Linq;

        internal sealed class Grouped(int offset, int width, int height) {
            private static readonly Func<int, int>[] Steps = [Doubled, Halved];

            internal int[] Shifted(int[] values) {
                Func<int, int> moved = Moved;
                return [.. values.Select(moved)];
            }

            internal int[] Raised(int[] values) => [.. values.Select(Lifted)];

            internal int Size() {
                int area = Area;
                return area + offset;
            }

            internal int Volume() => Depth * offset;

            internal static int Applied(int value) => Steps[0](value);

            private int {|#0:Area|} => width * height;

            private int Depth => width - height;

            private int {|#1:Moved|}(int value) => value + offset;

            private int Lifted(int value) => value - offset;

            private static int Doubled(int value) => value * 2;

            private static int Halved(int value) => value / 2;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Area", "Size").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic("RASM3001").WithLocation(1).WithArguments("Moved", "Shifted").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        using System;
        using System.Linq;

        internal sealed class Grouped(int offset, int width, int height) {
            private static readonly Func<int, int>[] Steps = [Doubled, Halved];

            internal int[] Shifted(int[] values) {
                Func<int, int> moved = value => value + offset;
                return [.. values.Select(moved)];
            }

            internal int[] Raised(int[] values) => [.. values.Select(Lifted)];

            internal int Size() {
                int area = width * height;
                return area + offset;
            }

            internal int Volume() => Depth * offset;

            internal static int Applied(int value) => Steps[0](value);

            private int Depth => width - height;

            private int Lifted(int value) => value - offset;

            private static int Doubled(int value) => value * 2;

            private static int Halved(int value) => value / 2;
        }
        """);

    [Fact]
    public Task InlinesMethodWithUnderscoreParameterButNotAttributeOrShadowedReads() => VerifyCodeFixAsync(
        """
        using System;
        using System.Text.RegularExpressions;

        internal sealed class Kept {
            private const string Marked = "held";
            private static readonly Regex Digits = new("[0-9]+");

            [Obsolete(Marked)]
            internal static string Note() => "note";

            internal static bool Numeric(string text) => Digits.IsMatch(text);

            private static readonly int factor = 4;

            internal static int Shadowed(int factor) => Scaled(2) + factor;

            private static int Scaled(int value) => value * factor;

            private static int Outer(int value) => value + 1;

            internal static class Nested {
                internal static int Read(int value) => Outer(value);
            }
        }

        internal static class UnderscoreParameter {
            internal static int Changed(int value) {
                int kept = Kept(value);
                return kept;
            }

            private static int {|#0:Kept|}(int _) => _ + 1;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Kept", "Changed").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        using System;
        using System.Text.RegularExpressions;

        internal sealed class Kept {
            private const string Marked = "held";
            private static readonly Regex Digits = new("[0-9]+");

            [Obsolete(Marked)]
            internal static string Note() => "note";

            internal static bool Numeric(string text) => Digits.IsMatch(text);

            private static readonly int factor = 4;

            internal static int Shadowed(int factor) => Scaled(2) + factor;

            private static int Scaled(int value) => value * factor;

            private static int Outer(int value) => value + 1;

            internal static class Nested {
                internal static int Read(int value) => Outer(value);
            }
        }

        internal static class UnderscoreParameter {
            internal static int Changed(int value) {
                int kept = value + 1;
                return kept;
            }
        }
        """);

    [Fact]
    public Task InlinesMethodCalledWithReorderedNamedArguments() => VerifyCodeFixAsync(
        """
        internal static class NamedArguments {
            internal static int Changed(int left, int right) {
                int difference = Difference(right: right, left: left);
                return difference;
            }

            private static int {|#0:Difference|}(int left, int right) => left - right;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Difference", "Changed").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        internal static class NamedArguments {
            internal static int Changed(int left, int right) {
                int difference = left - right;
                return difference;
            }
        }
        """);

    // --- [EXCLUSIONS]

    [Fact]
    public Task IgnoresInstanceMethodOfStruct() => VerifyAnalyzerAsync(
        """
        internal readonly struct Grid(int width) {
            internal int Shifted(int row) => Offset(row) + 1;

            private int Offset(int row) => row * width;
        }
        """);

    [Fact]
    public Task IgnoresUnsafeMethodWithFixedStatement() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Frozen;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text.RegularExpressions;

        internal static class Pointers {
            internal static int First(int[] values) => Read(values);

            private static unsafe int Read(int[] values) {
                fixed (int* start = values) return *start;
            }
        }
        """);

    [Fact]
    public Task IgnoresMethodNamedInNameof() => VerifyAnalyzerAsync(
        """
        internal static class NamedMethod {
            internal static int Changed(int value) {
                int twice = Twice(value);
                return twice;
            }

            internal static string Name => nameof(Twice);

            private static int Twice(int value) => value * 2;
        }
        """);

    [Fact]
    public Task IgnoresMethodWhoseInliningReordersSideEffects() => VerifyAnalyzerAsync(
        """
        public static class EvaluationOrder {
            private static int count;

            internal static int Run(int divisor) {
                int readRead = ReadRead(10 / divisor);
                return readRead;
            }

            public static int Log() => ++count;

            private static int ReadRead(int value) => Log() + value;
        }
        """);
}
