using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations.SingleReader;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class InlinedConstantTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {

    // --- [CONSTANTS]

    [Fact]
    public Task InlinesConstantReadOnceWhereParameterOrLocalKeepsItsName() => VerifyCodeFixAsync(
        """
        using System;

        internal static class NamedConstants {
            private const int {|#0:Retries|} = 3;
            private const string {|#1:Prefix|} = "tag:";
            private const double Tolerance = -0.5;
            private const int DefaultWidth = 80;
            private const double Break = 0.5;
            private const int Limit = 3;

            internal static int Run() => Attempt(Retries) + Attempt(retries: 1);

            internal static string Tagged(string name) {
                string prefix = Prefix;
                return prefix + name;
            }

            internal static bool Near(double value) => Close(value, Tolerance);

            internal static int Wide() => Attempt(DefaultWidth);

            internal static bool High(double value) => value > Break;

            internal static int Clamped(int value) => value > Limit ? Limit : value;

            private static int Attempt(int retries) => retries;

            private static bool Close(double value, double tolerance) => Math.Abs(value) < tolerance;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Retries", "Run").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic("RASM3001").WithLocation(1).WithArguments("Prefix", "Tagged").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        using System;

        internal static class NamedConstants {
            private const double Tolerance = -0.5;
            private const int DefaultWidth = 80;
            private const double Break = 0.5;
            private const int Limit = 3;

            internal static int Run() => Attempt(3) + Attempt(retries: 1);

            internal static string Tagged(string name) {
                string prefix = "tag:";
                return prefix + name;
            }

            internal static bool Near(double value) => Close(value, Tolerance);

            internal static int Wide() => Attempt(DefaultWidth);

            internal static bool High(double value) => value > Break;

            internal static int Clamped(int value) => value > Limit ? Limit : value;

            private static int Attempt(int retries) => retries;

            private static bool Close(double value, double tolerance) => Math.Abs(value) < tolerance;
        }
        """);

    [Fact]
    public Task InlinesConstantWithoutTouchingSameNamedConstantOfOtherType() => VerifyCodeFixAsync(
        """
        public static class ScopedNames {
            private const int {|#0:Count|} = 2;

            internal static int Changed() => Take(Count) + Other.Count;

            public static int Take(int count) => count;
        }

        public static class Other {
            public const int Count = 1;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Count", "Changed").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        public static class ScopedNames {
            internal static int Changed() => Take(2) + Other.Count;

            public static int Take(int count) => count;
        }

        public static class Other {
            public const int Count = 1;
        }
        """);

    // --- [SETS]

    [Fact]
    public Task RewritesSingleReadContainsSetToPatternButNotComparerOrCustomSet() => VerifyCodeFixAsync(
        """
        using System;
        using System.Collections.Frozen;
        using System.Collections.Generic;
        using System.Linq;

        internal static class ContainedSets {
            private static readonly string[] {|#0:Known|} = ["png", "jpg"];
            private static readonly FrozenSet<char> {|#1:Separators|} = new[] { ',', ';' }.ToFrozenSet();
            private static readonly int[] Weights = [1, 2, 4];
            private static readonly HashSet<string> Folded = new(StringComparer.OrdinalIgnoreCase) { "a" };

            internal static bool Unsupported(string extension) => !Known.Contains(extension);

            internal static bool Split(char letter) => Separators.Contains(letter);

            internal static int Weighted(int index) => Weights[index] * Weights.Length;

            internal static bool Seen(string text) => Folded.Contains(text);
        }

        internal static class StaticContains {
            private static readonly string[] {|#2:Extensions|} = ["png", "jpg"];

            internal static bool Found(string value) => Enumerable.Contains(value: value, source: Extensions);
        }

        internal static class ComparedSets {
            private static readonly FrozenSet<string> Frozen = new[] { "a" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            private static readonly HashSet<string> Hashed = new[] { "a" }.ToHashSet(StringComparer.OrdinalIgnoreCase);

            internal static bool FrozenContains(string value) => Frozen.Contains(value);

            internal static bool HashedContains(string value) => Hashed.Contains(value);
        }

        internal sealed class CustomSet : HashSet<string> {
            internal new bool Contains(string value) => false;
        }

        internal static class CustomSets {
            private static readonly CustomSet Values = new() { "a" };

            internal static bool Found(string value) => Values.Contains(value);
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

        internal static class WidenedMembership {
            private static readonly long[] {|#3:Counts|} = [1L, 2L];

            internal static bool Found(int value) => Counts.Contains(value);
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Known", "Unsupported").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic("RASM3001").WithLocation(1).WithArguments("Separators", "Split").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic("RASM3001").WithLocation(2).WithArguments("Extensions", "Found").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic("RASM3001").WithLocation(3).WithArguments("Counts", "Found").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        using System;
        using System.Collections.Frozen;
        using System.Collections.Generic;
        using System.Linq;

        internal static class ContainedSets {
            private static readonly int[] Weights = [1, 2, 4];
            private static readonly HashSet<string> Folded = new(StringComparer.OrdinalIgnoreCase) { "a" };

            internal static bool Unsupported(string extension) => extension is not ("png" or "jpg");

            internal static bool Split(char letter) => letter is ',' or ';';

            internal static int Weighted(int index) => Weights[index] * Weights.Length;

            internal static bool Seen(string text) => Folded.Contains(text);
        }

        internal static class StaticContains {
            internal static bool Found(string value) => value is "png" or "jpg";
        }

        internal static class ComparedSets {
            private static readonly FrozenSet<string> Frozen = new[] { "a" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            private static readonly HashSet<string> Hashed = new[] { "a" }.ToHashSet(StringComparer.OrdinalIgnoreCase);

            internal static bool FrozenContains(string value) => Frozen.Contains(value);

            internal static bool HashedContains(string value) => Hashed.Contains(value);
        }

        internal sealed class CustomSet : HashSet<string> {
            internal new bool Contains(string value) => false;
        }

        internal static class CustomSets {
            private static readonly CustomSet Values = new() { "a" };

            internal static bool Found(string value) => Values.Contains(value);
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

        internal static class WidenedMembership {
            internal static bool Found(int value) => (long)value is 1L or 2L;
        }
        """);

    // --- [LOCATIONS]

    [Fact]
    public Task ReportsConstantWithItsReadAsAdditionalLocation() => VerifyAnalyzerAsync(
        """
        internal static class Sample {
            private const int {|#0:Read|} = 1;

            internal static readonly int ReadValue = {|#1:Read|};
        }
        """,
        Diagnostic("RASM3001").WithLocation(0).WithLocation(1).WithArguments("Read", "ReadValue"));
}
