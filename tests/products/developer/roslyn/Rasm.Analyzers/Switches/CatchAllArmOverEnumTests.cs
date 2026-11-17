using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Switches;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CatchAllArmOverEnumTests : CSharpCodeFixVerifier<CatchAllArmAnalyzer, LanguageCodeFix> {
    [Fact]
    public Task StaysSilentWhenEnumIsFlagsGuardedOrFullyNamed() => VerifyAnalyzerAsync(
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
    public Task ReportsCatchAllArmOverEnumWithUnnamedMembers() => VerifyAnalyzerAsync(
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
}
