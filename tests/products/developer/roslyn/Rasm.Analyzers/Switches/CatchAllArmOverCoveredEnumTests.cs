using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Switches;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CatchAllArmOverCoveredEnumTests : CSharpCodeFixVerifier<CatchAllArmAnalyzer, LanguageCodeFix> {
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
    public Task IgnoresCatchAllArmOverFlagsEnum() => VerifyAnalyzerAsync(
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

    [Fact]
    public Task DeletesNegatedAndDiscardArmsOverCoveredEnum() => new CSharpCodeFixTest<CatchAllArmAnalyzer, LanguageCodeFix> {
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
