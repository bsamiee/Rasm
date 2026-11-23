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
