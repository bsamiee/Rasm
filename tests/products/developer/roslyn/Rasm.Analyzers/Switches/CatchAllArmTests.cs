using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Switches;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CatchAllArmTests : CSharpCodeFixVerifier<CatchAllArmAnalyzer, LanguageCodeFix> {
    [Fact]
    public Task DeletesCatchAllArmOverEnumWithEveryMemberNamed() => VerifyCodeFixAsync(
        """
        using System;

        internal enum Mode {
            First,
            Second,
        }

        internal static class CoveredEnum {
            internal static int Of(Mode mode) => mode switch {
                Mode.First => 1,
                Mode.Second => 2,
                {|#0:_ => throw new InvalidOperationException()|},
            };

            internal static int Combined(Mode mode) => mode switch { Mode.First or Mode.Second => 1, {|#1:_ => throw new InvalidOperationException()|} };

            internal static int Negated(Mode mode) => mode switch { Mode.First => 1, Mode.Second => 2, {|#2:not Mode.First => 3|} };
        }
        """,
        [
            Diagnostic("RASM3002").WithLocation(0).WithArguments("Mode"),
            Diagnostic("RASM3002").WithLocation(1).WithArguments("Mode"),
            Diagnostic("RASM3002").WithLocation(2).WithArguments("Mode"),
        ],
        """
        using System;

        internal enum Mode {
            First,
            Second,
        }

        internal static class CoveredEnum {
            internal static int Of(Mode mode) => mode {|CS8524:switch|} {
                Mode.First => 1,
                Mode.Second => 2,
            };

            internal static int Combined(Mode mode) => mode {|CS8524:switch|} { Mode.First or Mode.Second => 1 };

            internal static int Negated(Mode mode) => mode {|CS8524:switch|} { Mode.First => 1, Mode.Second => 2 };
        }
        """);

    [Fact]
    public Task DeletesBothCatchAllArmsOfOneSwitchInOneFix() => new CSharpCodeFixTest<CatchAllArmAnalyzer, LanguageCodeFix> {
        ExpectedDiagnostics = {
            DiagnosticResult.CompilerError("CS8510").WithLocation(0),
            Diagnostic("RASM3002").WithLocation(2).WithArguments("Mode"),
            Diagnostic("RASM3002").WithLocation(3).WithArguments("Mode"),
        },
        FixedState = { ExpectedDiagnostics = { DiagnosticResult.CompilerWarning("CS8524").WithLocation(1) } },
        TestCode = """
            internal enum Mode { First, Second }

            internal static class Sample {
                internal static int Of(Mode value) => value switch {
                    Mode.First => 1,
                    Mode.Second => 2,
                    {|#2:not Mode.First => 3|},
                    {|#3:{|#0:_|} => 4|},
                };
            }
            """,
        FixedCode = """
            internal enum Mode { First, Second }

            internal static class Sample {
                internal static int Of(Mode value) => value {|#1:switch|} {
                    Mode.First => 1,
                    Mode.Second => 2,
                };
            }
            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task ReportsCatchAllArmListingUnnamedMembersAndIgnoresGuardedFlagsAndExternalEnums() => VerifyAnalyzerAsync(
        """
        using System;

        internal enum Mode {
            First,
            Second,
            Third,
        }

        [Flags]
        internal enum Marks {
            None = 0,
            Left = 1,
        }

        internal static class PartialEnum {
            internal static int Thrown(Mode mode) => mode switch {
                Mode.First => 1,
                {|#0:_ => throw new InvalidOperationException()|},
            };

            internal static int Excluded(Mode mode) => mode switch { Mode.First => 1, Mode.Second => 2, {|#1:not (Mode.First or Mode.Second) => 3|} };

            internal static int Guarded(Mode mode, bool held) => mode switch { Mode.First when held => 1, _ => 0 };

            internal static int Flagged(Marks marks) => marks switch { Marks.None => 0, Marks.Left => 1, _ => throw new InvalidOperationException() };

            internal static int External(DayOfWeek day) => day switch { DayOfWeek.Monday => 1, _ => 0 };
        }
        """,
        Diagnostic("RASM3004").WithLocation(0).WithArguments("Mode", "`Mode.Second`, `Mode.Third`"),
        Diagnostic("RASM3004").WithLocation(1).WithArguments("Mode", "`Mode.Third`"));

    [Fact]
    public Task ReportsFallbackBranchOfConditionalChainOverEnumAndIgnoresSingleTestOrMixedSubjects() => VerifyAnalyzerAsync(
        """
        using System;

        internal enum Mode {
            First,
            Second,
            Third,
        }

        internal enum Side {
            Left,
            Right,
        }

        [Flags]
        internal enum Marks {
            None = 0,
            Left = 1,
            Right = 2,
        }

        internal static class Chains {
            internal static int Ternary(Mode mode) => mode == Mode.First ? 1 : mode == Mode.Second ? 2 : {|#0:3|};

            internal static int Patterned(Mode mode) => mode is Mode.First or Mode.Second ? 1 : {|#1:0|};

            internal static int Paired(Side side) => side == Side.Left ? 1 : {|#2:2|};

            internal static int Branched(Mode mode) {
                if (mode == Mode.First) {
                    return 1;
                } else if (mode == Mode.Second) {
                    return 2;
                } else {|#3:{
                    return 3;
                }|}
            }

            internal static int Returned(Mode mode) {
                if (mode == Mode.First) return 1;
                if (mode == Mode.Second) return 2;
                {|#4:return 3;|}
            }

            internal static int Single(Mode mode) => mode == Mode.First ? 1 : 0;

            internal static int Mixed(Mode mode, Mode other) => mode == Mode.First ? 1 : other == Mode.Second ? 2 : 3;

            internal static int Unreturned(Mode mode, Func<int> act) {
                if (mode == Mode.First) act();
                if (mode == Mode.Second) act();
                return act();
            }

            internal static int Flagged(Marks marks) => marks == Marks.Left ? 1 : marks == Marks.Right ? 2 : 0;

            internal static int External(DayOfWeek day) => day == DayOfWeek.Monday ? 1 : day == DayOfWeek.Friday ? 5 : 0;
        }
        """,
        Diagnostic("RASM3004").WithLocation(0).WithArguments("Mode", "`Mode.Third`"),
        Diagnostic("RASM3004").WithLocation(1).WithArguments("Mode", "`Mode.Third`"),
        Diagnostic("RASM3004").WithLocation(2).WithArguments("Side", "`Side.Right`"),
        Diagnostic("RASM3004").WithLocation(3).WithArguments("Mode", "`Mode.Third`"),
        Diagnostic("RASM3004").WithLocation(4).WithArguments("Mode", "`Mode.Third`"));

    [Fact]
    public Task ReportsCatchAllOverUnionInSwitchExpressionAndStatementAndIgnoresNullArm() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [Union]
        internal abstract partial record Shape {
            internal sealed record Circle(double Radius) : Shape;
            internal sealed record Square(double Side) : Shape;
        }

        internal static class UnionArms {
            internal static double Discarded(Shape shape) => shape switch { Shape.Circle circle => circle.Radius, {|#0:_ => 0d|} };

            internal static double Defaulted(Shape shape) {
                switch (shape) {
                    case Shape.Circle circle:
                        return circle.Radius;
                    {|#1:default:|}
                        return 0d;
                }
            }

            internal static bool Present(Shape? shape) => shape switch { null => false, _ => true };
        }
        """,
        Diagnostic("RASM1101").WithLocation(0).WithArguments("Shape"),
        Diagnostic("RASM1101").WithLocation(1).WithArguments("Shape"));
}
