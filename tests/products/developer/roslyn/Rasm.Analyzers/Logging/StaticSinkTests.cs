using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Logging;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class StaticSinkTests : CSharpCodeFixVerifier<StaticSinkAnalyzer, EmptyCodeFixProvider> {

    // --- [OWNERSHIP]

    [Fact]
    public Task ReportsConsoleAndProcessWideSinksInLibrary() => new CSharpCodeFixTest<StaticSinkAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = OutputKind.DynamicallyLinkedLibrary },
        TestCode = Output,
        ExpectedDiagnostics = {
            Diagnostic("RASM1206").WithLocation(0).WithArguments("""Console.WriteLine("started")"""),
            Diagnostic("RASM1206").WithLocation(1).WithArguments("Console.Error"),
            Diagnostic("RASM1206").WithLocation(2).WithArguments("Console.Write"),
            Diagnostic("RASM1204").WithLocation(3).WithArguments("Output.WriteLine(message)", "Debug"),
            Diagnostic("RASM1204").WithLocation(4).WithArguments("""new TraceSource("sample")""", "TraceSource"),
            Diagnostic("RASM1204").WithLocation(5).WithArguments("Debugger.Log(0, null, message)", "Debugger"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task ReportsProcessWideSinksAloneInExecutable() => new CSharpCodeFixTest<StaticSinkAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = OutputKind.ConsoleApplication },
        TestCode = Output,
        ExpectedDiagnostics = {
            Diagnostic("RASM1204").WithLocation(3).WithArguments("Output.WriteLine(message)", "Debug"),
            Diagnostic("RASM1204").WithLocation(4).WithArguments("""new TraceSource("sample")""", "TraceSource"),
            Diagnostic("RASM1204").WithLocation(5).WithArguments("Debugger.Log(0, null, message)", "Debugger"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [SOURCES]

    private const string Output = """
        using System;
        using System.Diagnostics;
        using System.IO;
        using Output = System.Diagnostics.Debug;

        internal static class Program {
            private static void Main() => {|#0:Console.WriteLine("started")|};

            internal static TextWriter Failure => {|#1:Console.Error|};

            internal static Action<string> Grouped => {|#2:Console.Write|};

            internal static void Debugged(string message) => {|#3:Output.WriteLine(message)|};

            internal static TraceSource Sourced() => {|#4:new TraceSource("sample")|};

            internal static void Attached(string message) => {|#5:Debugger.Log(0, null, message)|};

            internal static void Written(TextWriter output, string message) => output.WriteLine(message);

            internal static string? Line() => Console.ReadLine();

            internal static bool Debugging => Debugger.IsAttached;

            internal static string Named => nameof(Console.Error);

            internal static void Local(string message) => Terminal.Console.WriteLine(message);
        }

        namespace Terminal {
            internal static class Console {
                internal static void WriteLine(string message) => _ = message;
            }
        }
        """;
}
