using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LoggingRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class StaticSinkTests : CSharpCodeFixVerifier<StaticSinkAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM1204]

    [Fact(DisplayName = "Debug, Trace, TraceSource, and Debugger.Log members are reported; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs WarnOnThreadSleepInAsyncMethod")]
    public Task ProcessWideSinks() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Diagnostics;
        using Output = System.Diagnostics.Debug;

        internal static class ProcessWideSinks {
            internal static void Debugged(string message) => {|#0:Debug.WriteLine(message)|};

            internal static void Traced(string message) => {|#1:Trace.TraceInformation(message)|};

            internal static TraceSource Sourced() => {|#2:new TraceSource("sample")|};

            internal static void Evented(TraceSource source) => {|#3:source.TraceEvent(TraceEventType.Information, 1)|};

            internal static void Attached(string message) => {|#4:Debugger.Log(0, null, message)|};

            internal static void Aliased(string message) => {|#5:Output.WriteLine(message)|};

            internal static Action<int, string?, string?> Grouped => {|#6:Debugger.Log|};
        }
        """,
        Diagnostic("RASM1204").WithLocation(0).WithArguments("Debug.WriteLine(message)", "Debug"),
        Diagnostic("RASM1204").WithLocation(1).WithArguments("Trace.TraceInformation(message)", "Trace"),
        Diagnostic("RASM1204").WithLocation(2).WithArguments("""new TraceSource("sample")""", "TraceSource"),
        Diagnostic("RASM1204").WithLocation(3).WithArguments("source.TraceEvent(TraceEventType.Information, 1)", "TraceSource"),
        Diagnostic("RASM1204").WithLocation(4).WithArguments("Debugger.Log(0, null, message)", "Debugger"),
        Diagnostic("RASM1204").WithLocation(5).WithArguments("Output.WriteLine(message)", "Debug"),
        Diagnostic("RASM1204").WithLocation(6).WithArguments("Debugger.Log", "Debugger"));

    [Fact(DisplayName = "Executables hold no exemption for process-wide sinks; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseRegexSourceGeneratorAnalyzerTests.cs TopLevelStatement_NewRegex_PartialMethod")]
    public Task ProcessWideSinkInExecutable() => new CSharpCodeFixTest<StaticSinkAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = OutputKind.ConsoleApplication },
        TestCode = """
            using System.Diagnostics;

            {|#0:Trace.WriteLine("started")|};
            """,
        ExpectedDiagnostics = { Diagnostic("RASM1204").WithLocation(0).WithArguments("""Trace.WriteLine("started")""", "Trace") },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact(DisplayName = "Debugger state, nameof, an ILogger, and a same-named type of another namespace are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnOtherMethodsWithSleepName")]
    public Task CallerConfiguredOutput() => VerifyAnalyzerAsync(
        """
        using System.Diagnostics;
        using Microsoft.Extensions.Logging;

        internal static class CallerConfiguredOutput {
            internal static bool Attached => Debugger.IsAttached;

            internal static void Paused() => Debugger.Break();

            internal static string Named => nameof(Debug.WriteLine);

            internal static bool Enabled(ILogger logger) => logger.IsEnabled(LogLevel.Information);

            internal static void Local(string message) => Sinks.Debug.WriteLine(message);
        }

        namespace Sinks {
            internal static class Debug {
                internal static void WriteLine(string message) => _ = message;
            }
        }
        """);

    // --- [RASM1206]

    [Fact(DisplayName = "Library members writing to Console are reported; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs WarnOnThreadSleepInAsyncMethod")]
    public Task ConsoleInLibrary() => VerifyAnalyzerAsync(
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
        }
        """,
        Diagnostic("RASM1206").WithLocation(0).WithArguments("Console.WriteLine(message)"),
        Diagnostic("RASM1206").WithLocation(1).WithArguments("Console.Error"),
        Diagnostic("RASM1206").WithLocation(2).WithArguments("Console.Out"),
        Diagnostic("RASM1206").WithLocation(3).WithArguments("Console.Write"),
        Diagnostic("RASM1206").WithLocation(4).WithArguments("Console.SetOut(output)"),
        Diagnostic("RASM1206").WithLocation(5).WithArguments("Console.OpenStandardOutput()"));

    [Theory(DisplayName = "Executables are composition roots and write to Console; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/MakeClassStaticAnalyzerTests.cs TopLevelStatement_9")]
    [InlineData(OutputKind.ConsoleApplication)]
    [InlineData(OutputKind.WindowsApplication)]
    [InlineData(OutputKind.WindowsRuntimeApplication)]
    public Task ConsoleInExecutable(OutputKind kind) => new CSharpCodeFixTest<StaticSinkAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = kind },
        TestCode = """
            using System;

            Console.WriteLine("started");
            Console.Error.WriteLine("failed");
            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact(DisplayName = "A caller-supplied writer, Console input, nameof, and a same-named type of another namespace are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnOtherMethodsWithSleepName")]
    public Task CallerSuppliedWriter() => VerifyAnalyzerAsync(
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
