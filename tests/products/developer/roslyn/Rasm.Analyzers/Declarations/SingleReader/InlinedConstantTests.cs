using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations.SingleReader;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class InlinedConstantTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {

    // --- [CONSTANTS]

    [Fact]
    public Task InlinesConstantReadWhereTheParameterOrLocalKeepsItsNameButNotUnderAnotherName() => VerifyCodeFixAsync(
        """
        internal static class NamedConstants {
            private const int {|#0:Retries|} = 3;
            private const string {|#1:Prefix|} = "tag:";
            private const int DefaultWidth = 80;

            internal static int Run() => Attempt({|#2:Retries|}) + Attempt(DefaultWidth);

            internal static string Tagged(string name) {
                string prefix = {|#3:Prefix|};
                return prefix + name;
            }

            internal static int Attempt(int retries) => retries;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithLocation(2).WithArguments("Retries", "Run"),
            Diagnostic("RASM3001").WithLocation(1).WithLocation(3).WithArguments("Prefix", "Tagged"),
        ],
        """
        internal static class NamedConstants {
            private const int DefaultWidth = 80;

            internal static int Run() => Attempt(3) + Attempt(DefaultWidth);

            internal static string Tagged(string name) {
                string prefix = "tag:";
                return prefix + name;
            }

            internal static int Attempt(int retries) => retries;
        }
        """);

    [Fact]
    public Task InlinesConstantAtItsReadAndLeavesTheSameNamedConstantOfAnotherType() => VerifyCodeFixAsync(
        """
        public static class ScopedNames {
            private const int {|#0:Count|} = 2;

            public static int Changed() => Take({|#1:Count|}) + Other.Count;

            public static int Take(int count) => count;
        }

        public static class Other {
            public const int Count = 1;
        }
        """,
        [Diagnostic("RASM3001").WithLocation(0).WithLocation(1).WithArguments("Count", "Changed")],
        """
        public static class ScopedNames {
            public static int Changed() => Take(2) + Other.Count;

            public static int Take(int count) => count;
        }

        public static class Other {
            public const int Count = 1;
        }
        """);

    // --- [SETS]

    [Fact]
    public Task RewritesSetReadOnceToAPatternWithTheSameMembershipButNotASetWithAComparerOrOwnContains() => VerifyCodeFixAsync(
        """
        using System;
        using System.Collections.Frozen;
        using System.Collections.Generic;
        using System.Linq;

        public static class ContainedSets {
            private static readonly string[] {|#0:Known|} = ["png", "jpg"];
            private static readonly FrozenSet<char> {|#1:Separators|} = new[] { ',', ';' }.ToFrozenSet();
            private static readonly long[] {|#2:Counts|} = [1L, 2L];
            private static readonly HashSet<string> {|#3:Kinds|} = new() { "a", "b" };
            private static readonly FrozenSet<string> Frozen = new[] { "a" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            private static readonly HashSet<string> Folded = new(StringComparer.OrdinalIgnoreCase) { "a" };
            private static readonly CustomSet Custom = new() { "a" };

            public static bool Unsupported(string extension) => !{|#4:Known|}.Contains(extension);

            public static bool Split(char letter) => {|#5:Separators|}.Contains(letter);

            public static bool Counted(int value) => {|#6:Counts|}.Contains(value);

            public static bool Kind(string text) => {|#7:Kinds|}.Contains(text);

            public static bool FrozenContains(string value) => Frozen.Contains(value);

            public static bool Seen(string text) => Folded.Contains(text);

            public static bool Found(string value) => Custom.Contains(value);
        }

        public sealed class CustomSet : HashSet<string> {
            internal new bool Contains(string value) => false;
        }

        namespace Custom {
            internal static class Membership {
                internal static bool Contains(this string[] values, string value) => false;
            }

            internal static class Values {
                private static readonly string[] Names = ["a"];

                internal static bool Found(string value) => Names.Contains(value);
            }
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithLocation(4).WithArguments("Known", "Unsupported"),
            Diagnostic("RASM3001").WithLocation(1).WithLocation(5).WithArguments("Separators", "Split"),
            Diagnostic("RASM3001").WithLocation(2).WithLocation(6).WithArguments("Counts", "Counted"),
            Diagnostic("RASM3001").WithLocation(3).WithLocation(7).WithArguments("Kinds", "Kind"),
        ],
        """
        using System;
        using System.Collections.Frozen;
        using System.Collections.Generic;
        using System.Linq;

        public static class ContainedSets {
            private static readonly FrozenSet<string> Frozen = new[] { "a" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            private static readonly HashSet<string> Folded = new(StringComparer.OrdinalIgnoreCase) { "a" };
            private static readonly CustomSet Custom = new() { "a" };

            public static bool Unsupported(string extension) => extension is not ("png" or "jpg");

            public static bool Split(char letter) => letter is ',' or ';';

            public static bool Counted(int value) => (long)value is 1L or 2L;

            public static bool Kind(string text) => text is "a" or "b";

            public static bool FrozenContains(string value) => Frozen.Contains(value);

            public static bool Seen(string text) => Folded.Contains(text);

            public static bool Found(string value) => Custom.Contains(value);
        }

        public sealed class CustomSet : HashSet<string> {
            internal new bool Contains(string value) => false;
        }

        namespace Custom {
            internal static class Membership {
                internal static bool Contains(this string[] values, string value) => false;
            }

            internal static class Values {
                private static readonly string[] Names = ["a"];

                internal static bool Found(string value) => Names.Contains(value);
            }
        }
        """);
}
