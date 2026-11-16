using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CatchAllArmTests : CSharpCodeFixVerifier<CatchAllArmAnalyzer, LanguageCodeFix> {

    // --- [RASM1101]

    [Fact]
    public Task DiscardArm() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [Union]
        internal abstract partial record Shape {
            internal sealed record Circle(double Radius) : Shape;
            internal sealed record Square(double Side) : Shape;
        }

        internal static class DiscardArm {
            internal static double Area(Shape shape) => shape switch { Shape.Circle circle => circle.Radius, {|#0:_ => 0d|} };
        }
        """,
        Diagnostic("RASM1101").WithLocation(0).WithArguments("Shape"));

    [Fact]
    public Task DefaultSection() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [Union]
        internal abstract partial record Shape {
            internal sealed record Circle(double Radius) : Shape;
            internal sealed record Square(double Side) : Shape;
        }

        internal static class DefaultSection {
            internal static double Area(Shape shape) {
                switch (shape) {
                    case Shape.Circle circle:
                        return circle.Radius;
                    {|#0:default:|}
                        return 0d;
                }
            }
        }
        """,
        Diagnostic("RASM1101").WithLocation(0).WithArguments("Shape"));

    [Fact]
    public Task NullArm() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [Union]
        internal abstract partial record Shape {
            internal sealed record Circle(double Radius) : Shape;
            internal sealed record Square(double Side) : Shape;
        }

        internal static class NullArm {
            internal static double Plain(object value) => value switch { int count => count, _ => 0d };
            internal static bool Present(Shape? shape) => shape switch { null => false, _ => true };
        }
        """);

    // --- [RASM3002]

    [Fact]
    public Task CoveredEnum() => VerifyCodeFixAsync(
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
    public Task FlagsEnum() => VerifyAnalyzerAsync(
        """
        using System;

        [Flags]
        internal enum Marks {
            None = 0,
            Left = 1,
        }

        internal static class FlagsEnum {
            internal static int Flagged(Marks marks) => marks switch {
                Marks.None => 0,
                Marks.Left => 1,
                _ => throw new InvalidOperationException(),
            };
        }
        """);

    // --- [RASM3004]

    [Fact]
    public Task UnnamedEnum() => VerifyAnalyzerAsync(
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

        internal static class UnnamedEnum {
            internal static int Flagged(Marks marks) => marks switch {
                Marks.None => 0,
                Marks.Left => 1,
                _ => throw new InvalidOperationException(),
            };

            internal static int Guarded(Mode mode, bool held) => mode switch { Mode.First when held => 1, _ => 0 };

            internal static int GuardedNegation(Mode mode, bool held) => mode {|CS8846:switch|} {
                Mode.First => 1,
                not Mode.First when held => 2,
                Mode.Second => 3,
                Mode.Third => 4,
            };

            internal static int Named(Mode mode) => mode {|CS8524:switch|} {
                Mode.First => 1,
                Mode.Second or Mode.Third => 2,
            };
        }
        """);

    [Fact]
    public Task PartialEnum() => VerifyAnalyzerAsync(
        """
        using System;

        internal enum Mode {
            First,
            Second,
            Third,
        }

        internal static class PartialEnum {
            internal static int Thrown(Mode mode) => mode switch {
                Mode.First => 1,
                {|#0:_ => throw new InvalidOperationException()|},
            };

            internal static int Valued(Mode mode) => mode switch { Mode.First or Mode.Second => 1, {|#1:_ => 0|} };

            internal static int Negated(Mode mode) => mode switch { Mode.First => 1, {|#2:not Mode.First => 2|} };

            internal static int Excluded(Mode mode) => mode switch { Mode.First => 1, Mode.Second => 2, {|#3:not (Mode.First or Mode.Second) => 3|} };
        }
        """,
        Diagnostic("RASM3004").WithLocation(0).WithArguments("Mode", "`Mode.Second`, `Mode.Third`"),
        Diagnostic("RASM3004").WithLocation(1).WithArguments("Mode", "`Mode.Third`"),
        Diagnostic("RASM3004").WithLocation(2).WithArguments("Mode", "`Mode.Second`, `Mode.Third`"),
        Diagnostic("RASM3004").WithLocation(3).WithArguments("Mode", "`Mode.Third`"));

    // --- [COMPILER]

    [Fact]
    public Task CoveredArms() => new CSharpCodeFixTest<CatchAllArmAnalyzer, LanguageCodeFix> {
        ExpectedDiagnostics = { DiagnosticResult.CompilerError("CS8510").WithLocation(0) },
        FixedState = { ExpectedDiagnostics = { DiagnosticResult.CompilerWarning("CS8524").WithLocation(1) } },
        TestCode = """
            internal enum Mode { First, Second }

            internal static class Sample {
                internal static int Of(Mode value) => value switch {
                    Mode.First => 1,
                    Mode.Second => 2,
                    {|RASM3002:not Mode.First => 3|},
                    {|RASM3002:{|#0:_|} => 4|},
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
}
