using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Logging;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DoubleReportTests : CSharpCodeFixVerifier<DoubleReportAnalyzer, EmptyCodeFixProvider> {

    // --- [RECEIVERS]

    [Fact]
    public Task ReportsFallbackOrAlternativeReportingAFailureItsReceiverLoggedAndIgnoresUnloggedReceiverSilentHandlerOrLookalikeFallback() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using Microsoft.Extensions.Logging;
        using Rhino.Runtime;
        using static LanguageExt.Prelude;

        internal sealed class Sink(ILogger logger) {
            internal Fin<Unit> Report(Error error) {
                logger.LogError("{Failure}", error.Message);
                return unit;
            }
        }

        internal sealed class Outcome {
            internal static Outcome Of(Fin<Unit> result) => new();

            internal Unit IfFail(Action<Error> handle) => unit;
        }

        internal static class Receivers {
            internal static Unit Logged(Sink sink, Error error) => sink.Report(error).{|#0:IfFail(fault => HostUtils.ExceptionReport(fault.ToException()))|};

            internal static Fin<Unit> Alternative(Sink sink, Error error) => {|#1:sink.Report(error) | Crash(error)|};

            internal static Unit Handled(Sink sink, Error error) => sink.Report(error).IfFail(static _ => unit);

            internal static Unit Lookalike(Sink sink, Error error) => Outcome.Of(sink.Report(error)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> Crash(Error error) {
                HostUtils.ExceptionReport(error.ToException());
                return unit;
            }

            internal static Unit Unlogged(ILogger logger) => Checked(logger).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> Checked(ILogger logger) => logger.IsEnabled(LogLevel.Error) ? unit : Error.New("disabled");
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("HostUtils.ExceptionReport", "Sink.Report"),
        Diagnostic().WithLocation(1).WithArguments("Receivers.Crash", "Sink.Report"));

    // --- [CALLBACKS]

    [Fact]
    public Task ReportsLoggingReachedThroughCallsAndInvokedOrRelayedCallbacksAndIgnoresStoredOrCyclicCallbacks() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using LanguageExt;
        using Microsoft.Extensions.Logging;
        using Rhino.Runtime;
        using static LanguageExt.Prelude;

        internal static class Callbacks {
            private static readonly List<Action> stored = [];

            internal static Unit Chained(ILogger logger) => Forward(logger).{|#0:IfFail(fault => HostUtils.ExceptionReport(fault.ToException()))|};

            internal static Unit Invoked(ILogger logger) => Invoke(() => Failed(logger, "failure")).{|#1:IfFail(fault => HostUtils.ExceptionReport(fault.ToException()))|};

            internal static Unit Lifted(ILogger logger) => IO.lift(() => {
                Failed(logger, "failure");
                return unit;
            }).RunSafe().{|#2:IfFail(fault => HostUtils.ExceptionReport(fault.ToException()))|};

            internal static Unit Relayed(ILogger logger) => Relay(() => Failed(logger, "failure")).{|#3:IfFail(fault => HostUtils.ExceptionReport(fault.ToException()))|};

            internal static Unit Kept(ILogger logger) => Store(() => Failed(logger, "failure")).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Unit Cyclic(ILogger logger) => First(() => Failed(logger, "failure")).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            private static Fin<Unit> Forward(ILogger logger) => Invoke(() => Failed(logger, "failure"));

            private static Fin<Unit> Invoke(Action callback) {
                callback();
                return unit;
            }

            private static Fin<Unit> Relay(Action callback) => Invoke(callback);

            private static Fin<Unit> Store(Action callback) {
                stored.Add(callback);
                return unit;
            }

            private static Fin<Unit> First(Action callback) => Second(callback);

            private static Fin<Unit> Second(Action callback) => First(callback);

            [LoggerMessage(Level = LogLevel.Error, Message = "{Failure}")]
            private static void Failed(ILogger logger, string failure) { }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("HostUtils.ExceptionReport", "Callbacks.Forward"),
        Diagnostic().WithLocation(1).WithArguments("HostUtils.ExceptionReport", "Callbacks.Invoke"),
        Diagnostic().WithLocation(2).WithArguments("HostUtils.ExceptionReport", "IO.lift"),
        Diagnostic().WithLocation(3).WithArguments("HostUtils.ExceptionReport", "Callbacks.Relay"));
}
