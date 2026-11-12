using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LoggingRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class HandWrittenLogTests : CSharpCodeFixVerifier<HandWrittenLogAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task Sample() => VerifyAnalyzerAsync(
        """
        using System;
        using Microsoft.Extensions.Logging;

        internal sealed class Sample(ILogger<Sample> typed) {
            private readonly ILogger<Sample> logger = typed;

            internal static void Started(ILogger source) => source.{|#0:Log(LogLevel.Information, new EventId(1), "started", null, static (state, _) => state)|};

            internal void Stopped() => logger.{|#1:Log(LogLevel.Information, new EventId(2), "stopped", null, static (state, _) => state)|};

            internal IDisposable? Scoped() => logger.{|#2:BeginScope("scoped")|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("ILogger.Log", "a `partial void` method under `[LoggerMessage]`"),
        Diagnostic().WithLocation(1).WithArguments("ILogger.Log", "a `partial void` method under `[LoggerMessage]`"),
        Diagnostic().WithLocation(2).WithArguments("ILogger.BeginScope", "a `LoggerMessage.DefineScope` delegate in a static field"));

    [Fact]
    public Task Forwarding() => VerifyAnalyzerAsync(
        """
        using System;
        using Microsoft.Extensions.Logging;

        internal sealed class Forwarding(ILogger inner) : ILogger {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => inner.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => inner.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => inner.Log(logLevel, eventId, state, exception, formatter);
        }
        """);

    [Fact]
    public Task Generated() => VerifyAnalyzerAsync(
        """
        using Microsoft.Extensions.Logging;

        [System.CodeDom.Compiler.GeneratedCode("Microsoft.Extensions.Logging.Generators", "10.0.0")]
        internal static class Generated {
            internal static void Started(ILogger source) => source.Log(LogLevel.Information, new EventId(3), "started", null, static (state, _) => state);
        }
        """);
}
