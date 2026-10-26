using System.Globalization;
using Rhino;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInSink {
    public void Report(Error error, Type owner, string member);
}

// --- [SERVICES] ------------------------------------------------------------------------
[ProviderAlias("CommandWindow")]
public sealed class CommandWindowLoggerProvider : ILoggerProvider {
    public ILogger CreateLogger(string categoryName) => new CommandWindowLogger(categoryName);

    public void Dispose() => Callbacks.Settled(Callbacks.Drained, CancellationToken.None).IfFail(static fault => HostUtils.ExceptionReport(nameof(CommandWindowLoggerProvider), fault.ToException()));
}

internal sealed class CommandWindowLogger(string category) : ILogger {
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel is not LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (!IsEnabled(logLevel)) return;
        Callbacks.Started(
            (from written in IO.lift(() => RhinoApp.WriteLine($"{category}: {formatter(state, exception)}"))
             from reported in IO.lift(() => Optional(exception).Iter(held => HostUtils.ExceptionReport(category, held)))
             select unit)
            .Catch(fault => IO.lift(() => HostUtils.ExceptionReport(category, fault.ToException()))).As()
            .OnUiThread());
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RowText {
    public static CultureInfo Culture => CultureInfo.GetCultureInfo(Localization.CurrentLanguageId);

    public static LocalizeStringPair Localize(string english, params ReadOnlySpan<object?> arguments) =>
        Localize(english, typeof(RowText).Assembly, arguments);

    public static LocalizeStringPair Localize(string english, object table, params ReadOnlySpan<object?> arguments) {
        LocalizeStringPair pair = Localization.LocalizeCommandOptionName(english, table, 0);
        return arguments.IsEmpty ? pair : new(string.Format(Culture, pair.English, arguments), string.Format(Culture, pair.Local, arguments));
    }
}

public static partial class ErrorOps {
    public static string Localize(Error error) => string.Join(Environment.NewLine, Causes(error).Map(static cause => cause.Message));

    public static bool Cancellation(Error error) => error.Count is 1 && error.Is(Errors.Cancelled);

    public static void Log(ILogger logger, Error error, string member) {
        if (Cancellation(error)) Cancelled(logger, member);
        else if (logger.IsEnabled(LogLevel.Error)) Failed(logger, member, Localize(error), Causes(error).Choose(static cause => cause.Exception) switch { [] => null, [var single] => single, var many => new AggregateException(many) });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "{Member} failed with {Message}", SkipEnabledCheck = true)]
    private static partial void Failed(ILogger logger, string member, string message, Exception? exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Member} canceled")]
    private static partial void Cancelled(ILogger logger, string member);

    private static Seq<Error> Causes(Error error) =>
        error.FoldM(static leaf => leaf.Cons(leaf.Inner.ToSeq().Bind(Causes))).As();
}
