using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Telemetry;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class ErrorDiagnostics {

    // --- [CAUSES]

    extension(Error error) {
        public bool IsCancellation => error.Count is 1 && error.Is(Errors.Cancelled);

        public string Messages() => string.Join(Environment.NewLine, Causes(error, static _ => true).Map(static cause => cause.Message));
    }

    private static string ErrorType(Error error) =>
        error.Head.Exception.Map(static thrown => thrown.GetType().ToString())
            .IfNone(() => error.Head.Code is 0 ? error.Head.GetType().Name : error.Head.Code.ToString(CultureInfo.InvariantCulture));

    private static Seq<Error> Causes(Error error, Func<Error, bool> descends) =>
        toSeq(LanguageExt.List.unfold(error.AsIterable().ToSeq(), pending => pending.Head.Map(cause =>
            (cause, (descends(cause) ? cause.Inner.ToSeq().Bind(static inner => inner.AsIterable().ToSeq()) : []) + pending.Tail))));

    private static (Option<Exception> Thrown, string Stated) Parts(Error error) =>
        Causes(error, static cause => !cause.IsExceptional).Partition(static cause => cause.IsExceptional) switch {
            var (thrown, stated) => (
                thrown.Choose(static cause => cause.Exception) switch { [] => None, [var single] => Some(single), var many => Some<Exception>(new AggregateException(many)) },
                string.Join(Environment.NewLine, stated.Map(static cause => cause.Message))),
        };

    // --- [SPANS]

    extension(ActivitySource source) {
        public IO<T> Span<T>(string operation, IO<T> effect, ActivityKind kind = ActivityKind.Internal, ActivityContext parent = default) =>
            IO.liftVAsync(async environment => {
                using Activity? activity = source.StartActivity(operation, kind, parent);
                return await effect.Catch(error => IO.lift(() => Marked(activity, error)) >> IO.fail<T>(error)).RunAsync(environment).ConfigureAwait(false);
            });
    }

    private static Unit Marked(Activity? activity, Error error) =>
        ignore(activity is { IsAllDataRequested: true } && !error.IsCancellation
            ? Parts(error).Thrown.Fold(activity.SetStatus(ActivityStatusCode.Error, error.Messages()).SetTag("error.type", ErrorType(error)), static (failed, thrown) => failed.AddException(thrown))
            : activity);

    // --- [RECORDS]

    extension(ILogger logger) {
        public Unit Failed(Error error, [CallerMemberName] string member = "") {
            if (error.IsCancellation) {
                Canceled(logger, member);
                return unit;
            }
            (Option<Exception> thrown, string stated) = Parts(error);
            _ = Marked(Activity.Current, error);
            if (stated.Length is 0) Threw(logger, member, ErrorType(error), thrown.ValueUnsafe());
            else Failure(logger, member, ErrorType(error), stated, thrown.ValueUnsafe());
            return unit;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "{CodeFunctionName} failed with {ErrorType}: {ErrorMessage}")]
    private static partial void Failure(ILogger logger, string codeFunctionName, string errorType, string errorMessage, Exception? exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "{CodeFunctionName} failed with {ErrorType}")]
    private static partial void Threw(ILogger logger, string codeFunctionName, string errorType, Exception? exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{CodeFunctionName} canceled")]
    private static partial void Canceled(ILogger logger, string codeFunctionName);
}
