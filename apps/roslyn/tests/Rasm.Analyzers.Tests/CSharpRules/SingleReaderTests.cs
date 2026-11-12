using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SingleReaderTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {

    // --- [RASM3001]

    [Fact]
    public Task NamedConstants() => VerifyCodeFixAsync(
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
    public Task ContainedSets() => VerifyCodeFixAsync(
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

    [Fact]
    public Task Substituted() => VerifyCodeFixAsync(
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
    public Task Grouped() => VerifyCodeFixAsync(
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
    public Task UnderscoreParameter() => VerifyCodeFixAsync(
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
    public Task Grid() => VerifyAnalyzerAsync(
        """
        internal readonly struct Grid(int width) {
            internal int Shifted(int row) => Offset(row) + 1;

            private int Offset(int row) => row * width;
        }
        """);

    [Fact]
    public Task Pointers() => VerifyAnalyzerAsync(
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
    public Task ScopedNames() => VerifyCodeFixAsync(
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

    [Fact]
    public Task NamedArguments() => VerifyCodeFixAsync(
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

    [Fact]
    public Task RecursiveMembers() => VerifyAnalyzerAsync(
        """
        internal static class RecursiveMembers {
            private static int Read(int value) => ReadRead(value);

            private static int ReadRead(int value) => Read(value);
        }
        """);

    [Fact]
    public Task RecursiveProperties() => VerifyAnalyzerAsync(
        """
        internal static class RecursiveProperties {
            private static int Read => ReadRead;

            private static int ReadRead => Read;
        }
        """);

    [Fact]
    public Task ReadEnteredProperties() => VerifyCodeFixAsync(
        """
        internal static class ReadEnteredProperties {
            internal static int Value => Read;

            private static int Read => ReadRead;

            private static int {|#0:ReadRead|} => Read;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("ReadRead", "Read").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        internal static class ReadEnteredProperties {
            internal static int Value => Read;

            private static int Read => Read;
        }
        """);

    [Fact]
    public Task ReadReadEnteredProperties() => VerifyCodeFixAsync(
        """
        internal static class ReadReadEnteredProperties {
            internal static int Value => ReadRead;

            private static int {|#0:Read|} => ReadRead;

            private static int ReadRead => Read;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Read", "ReadRead").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        internal static class ReadReadEnteredProperties {
            internal static int Value => ReadRead;

            private static int ReadRead => ReadRead;
        }
        """);

    [Fact]
    public Task NamedMethod() => VerifyAnalyzerAsync(
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
    public Task EvaluationOrder() => VerifyAnalyzerAsync(
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

    // --- [FIX_ALL]

    [Fact]
    public Task DependentMembers() {
        string[] names = ["ReadRead", "Read", "ReadReadRead"];
        return new CSharpCodeFixTest<SingleReaderAnalyzer, SingleReaderCodeFix> {
            TestCode = $$"""
                internal static class Sample {
                    private const int ReadValue = 1;

                    internal static int Value => {{names[0]}}();

                {{string.Join("\n\n", names.Select((name, index) => $"    private static int {{|RASM3001:{name}|}}() => {(index + 1 == names.Length ? "ReadValue" : $"{names[index + 1]}()")};"))}}
                }
                """,
            FixedCode = """
                internal static class Sample {
                    private const int ReadValue = 1;

                    internal static int Value => ReadValue;
                }
                """,
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public Task FixAllProjects() {
        CSharpCodeFixTest<SingleReaderAnalyzer, SingleReaderCodeFix> test = new();
        ProjectState[] states = [test.TestState, test.TestState.AdditionalProjects["Other"]];
        foreach (ProjectState state in states) {
            ProjectState fixedState = state == test.TestState ? test.FixedState : test.FixedState.AdditionalProjects[state.Name];
            foreach (string name in new[] { "First", "Second" }) {
                state.Sources.Add(($"/{state.Name}/{name}.cs", $$"""
                    internal static class {{name}} {
                        internal static int Value => Read(1);

                        private static int {|RASM3001:Read|}(int read) => read;
                    }
                    """));
                fixedState.Sources.Add(($"/{state.Name}/{name}.cs", $$"""
                    internal static class {{name}} {
                        internal static int Value => 1;
                    }
                    """));
            }
        }
        test.NumberOfFixAllInDocumentIterations = states.Sum(static state => state.Sources.Count);
        test.NumberOfFixAllInProjectIterations = states.Length;
        return test.RunAsync(TestContext.Current.CancellationToken);
    }

    // --- [LOCATIONS]

    [Fact]
    public Task RelatedLocations() => VerifyAnalyzerAsync(
        """
        internal static class Sample {
            private const int {|#0:Read|} = 1;

            internal static readonly int ReadValue = {|#1:Read|};
        }
        """,
        Diagnostic("RASM3001").WithLocation(0).WithLocation(1).WithArguments("Read", "ReadValue"));
}
