using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Logging;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class StaticSinkTests : CSharpCodeFixVerifier<StaticSinkAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsDebugTraceTraceSourceAndDebuggerLogWrites() => VerifyAnalyzerAsync(
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

    [Fact]
    public Task ReportsProcessWideSinkInExecutable() => new CSharpCodeFixTest<StaticSinkAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = OutputKind.ConsoleApplication },
        TestCode = """
            using System.Diagnostics;

            {|#0:Trace.WriteLine("started")|};
            """,
        ExpectedDiagnostics = { Diagnostic("RASM1204").WithLocation(0).WithArguments("""Trace.WriteLine("started")""", "Trace") },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task IgnoresDebuggerStateILoggerAndSameNamedType() => VerifyAnalyzerAsync(
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
}
