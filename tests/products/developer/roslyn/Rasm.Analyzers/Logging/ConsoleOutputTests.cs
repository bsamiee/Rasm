using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Logging;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ConsoleOutputTests : CSharpCodeFixVerifier<StaticSinkAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsConsoleOutputInLibrary() => VerifyAnalyzerAsync(
        """
        using System;
        using System.IO;

        internal static class ConsoleInLibrary {
            internal static void Written(string message) => {|#0:Console.WriteLine(message)|};

            internal static void Failed(string message) => {|#1:Console.Error|}.WriteLine(message);

            internal static TextWriter Output => {|#2:Console.Out|};

            internal static Action<string> Grouped => {|#3:Console.Write|};

            internal static void Redirected(TextWriter output) => {|#4:Console.SetOut(output)|};

            internal static Stream Raw() => {|#5:Console.OpenStandardOutput()|};

            internal static void Main() => {|#6:Console.WriteLine()|};
        }
        """,
        Diagnostic("RASM1206").WithLocation(0).WithArguments("Console.WriteLine(message)"),
        Diagnostic("RASM1206").WithLocation(1).WithArguments("Console.Error"),
        Diagnostic("RASM1206").WithLocation(2).WithArguments("Console.Out"),
        Diagnostic("RASM1206").WithLocation(3).WithArguments("Console.Write"),
        Diagnostic("RASM1206").WithLocation(4).WithArguments("Console.SetOut(output)"),
        Diagnostic("RASM1206").WithLocation(5).WithArguments("Console.OpenStandardOutput()"),
        Diagnostic("RASM1206").WithLocation(6).WithArguments("Console.WriteLine()"));

    [Theory]
    [InlineData(OutputKind.ConsoleApplication)]
    [InlineData(OutputKind.WindowsApplication)]
    [InlineData(OutputKind.WindowsRuntimeApplication)]
    public Task IgnoresConsoleOutputInExecutable(OutputKind kind) => new CSharpCodeFixTest<StaticSinkAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = kind },
        TestCode = """
            using System;

            Console.WriteLine("started");
            Console.Error.WriteLine("failed");

            internal static class Report {
                internal static void Written(string line) => Console.Out.WriteLine(line);
            }
            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task IgnoresCallerWriterConsoleInputAndSameNamedType() => VerifyAnalyzerAsync(
        """
        using System;
        using System.IO;

        internal static class CallerSuppliedWriter {
            internal static void Written(TextWriter output, string message) => output.WriteLine(message);

            internal static string? Line() => Console.ReadLine();

            internal static TextReader Input => Console.In;

            internal static bool Pending => Console.KeyAvailable;

            internal static void Interrupted(ConsoleCancelEventHandler handler) => Console.CancelKeyPress += handler;

            internal static string Named => nameof(Console.WriteLine);

            internal static void Local(string message) => Terminal.Console.WriteLine(message);
        }

        namespace Terminal {
            internal static class Console {
                internal static void WriteLine(string message) => _ = message;
            }
        }
        """);
}
