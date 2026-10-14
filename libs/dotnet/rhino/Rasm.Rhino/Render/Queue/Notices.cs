using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Assets;
using Rhino.Runtime.Notifications;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoticeServer {
    public static NoticeServer Default { get; } = new("https://ntfy.sh");

    public Uri Address => new(_value);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = Uri.TryCreate(value, UriKind.Absolute, out Uri? address)
            && (string.Equals(address.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) || string.Equals(address.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal))
            && address.Query.Length == 0 && address.Fragment.Length == 0
                ? null
                : new InvalidRhinoValue();
}

[ValueObject<string>(SkipIParsable = true)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoticeTopic {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value.Length is >= 1 and <= 64 && value.All(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
            ? null
            : new InvalidRhinoValue();
}

public sealed record NoticeSettings(NoticeServer Server, Option<NoticeTopic> Topic) {
    public static NoticeSettings Default { get; } = new(NoticeServer.Default, None);
}

public sealed record NoticeOptions(IO<NoticeSettings> Settings, Option<(IGlyph Glyph, int Master)> Glyph, Option<View.Panel> Reveal);

[SmartEnum<int>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public sealed partial class NoticePriority {
    public static readonly NoticePriority Default = new(3, Notification.Severity.Info);
    public static readonly NoticePriority High = new(4, Notification.Severity.Warning);
    public static readonly NoticePriority Max = new(5, Notification.Severity.Serious);

    public Notification.Severity Severity { get; }
}

[Union]
public abstract partial record QueueEvent {
    private QueueEvent(NoticePriority priority, Option<string> tag, string title, bool fresh) => (Priority, Tag, Title, Fresh) = (priority, tag, title, fresh);

    public NoticePriority Priority { get; }
    public Option<string> Tag { get; }
    public string Title { get; }
    public bool Fresh { get; }

    public abstract string Description(DateTimeZone zone);

    public Notice Notice(DateTimeZone zone, Option<(IGlyph Glyph, int Master)> glyph, Option<View.Panel> reveal) =>
        new(RowText.Localize(Title), Description(zone), None, Priority.Severity, glyph, reveal);

    public sealed record Started(int Entries, long Frames) : QueueEvent(NoticePriority.Default, "racehorse", "Render queue started", fresh: true) {
        public override string Description(DateTimeZone zone) =>
            RowText.Localize("{0} frames across {1} entries", arguments: [Frames, Entries]);
    }

    public sealed record Submitted(Uri Job) : QueueEvent(NoticePriority.Default, None, "Render queue submitted", fresh: true) {
        public override string Description(DateTimeZone zone) =>
            RowText.Localize("Job submitted to {0}", arguments: [Job]);
    }

    public sealed record Estimate(QueueEstimate Reading) : QueueEvent(NoticePriority.Default, "hourglass", "Render queue estimate", fresh: false) {
        public override string Description(DateTimeZone zone) => Reading.Description(zone);
    }

    public sealed record Complete(long Frames, Duration Elapsed) : QueueEvent(NoticePriority.Default, "white_check_mark", "Render queue complete", fresh: false) {
        public override string Description(DateTimeZone zone) =>
            RowText.Localize("{0} frames in {1:-H:mm:ss}", arguments: [Frames, Elapsed]);
    }

    public sealed record Stopped(long Frames) : QueueEvent(NoticePriority.High, "warning", "Render queue stopped", fresh: false) {
        public override string Description(DateTimeZone zone) =>
            RowText.Localize("Stopped after {0} frames", arguments: [Frames]);
    }

    public sealed record Failed(Error Error) : QueueEvent(NoticePriority.Max, "x", "Render queue failed", fresh: false) {
        public override string Description(DateTimeZone zone) => ErrorOps.Localize(Error);
    }

    public sealed record RetriesExhausted(RetryCount Retries) : QueueEvent(NoticePriority.Max, "x", "Render queue retries exhausted", fresh: false) {
        public override string Description(DateTimeZone zone) =>
            RowText.Localize("The queue ended after {0} relaunches", arguments: [Retries]);
    }

    public sealed record Test() : QueueEvent(NoticePriority.Default, None, "Render queue test", fresh: true) {
        public override string Description(DateTimeZone zone) => RowText.Localize("A test notice from the render queue");
    }
}

internal sealed record NtfyMessage(string Topic, string Title, string Message, int Priority, string[] Tags);

[JsonSerializable(typeof(NtfyMessage))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class NtfyContext : JsonSerializerContext;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class NoticeSender(NoticeOptions options, TimeProvider clock, IPlugInSink sink) : IDisposable {
    // --- [ACQUISITION]
    private readonly HttpClient client = new() { Timeout = Duration.FromSeconds(10).ToTimeSpan() };
    private readonly Ref<Option<Option<Disposal<Notification>>>> raised = Ref(Some(Option<Disposal<Notification>>.None));

    public static IO<NoticeSender> Acquire(NoticeOptions options, TimeProvider clock, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => new NoticeSender(options, clock, sink)), static held => IO.lift(held.Dispose), retain);

    // --- [DELIVERY]
    public IO<Unit> Notify(QueueEvent queued) =>
        IO.lift(() => queued.Notice(clock.ToZonedClock().Zone, options.Glyph, options.Reveal))
            .OnUiThread()
            .Bind(notice => Callbacks.Each(Seq(Noticed(queued.Fresh, notice).OnUiThread(), Notifications.Show(notice).OnUiThread(), Configured(queued, notice))))
            .Map(static _ => unit)
            .Catch(static error => !error.Is(Errors.Cancelled), error => IO.lift(() => sink.Report(error, typeof(NoticeSender), nameof(Notify))));

    private IO<Unit> Noticed(bool fresh, Notice notice) =>
        atomicIO(() => {
            Option<Option<Disposal<Notification>>> prior = raised.Value;
            raised.Value = prior.Map(held => held.Filter(_ => !fresh));
            return prior;
        }).Bind(static prior => IO.lift(prior.ToFin(Errors.Cancelled)))
            .Bind(prior => Status.Show(notice, None, sink, cell => atomic(() => {
                Option<Option<Disposal<Notification>>> current = raised.Value;
                raised.Value = current.Map(_ => Some(cell));
                return current.ToFin(Errors.Cancelled).Map(static _ => unit);
            }).ThrowIfFail()).Map(static _ => unit) switch {
                var show => fresh
                    ? DisposalOps.Within(() => prior, _ => show, static withdrawn => DisposalOps.Release(withdrawn.ToSeq()))
                    : prior.Match(Some: kept => Status.Revise(kept, notice), None: () => show),
            });

    private IO<Unit> Configured(QueueEvent queued, Notice notice) =>
        options.Settings.OnUiThread().Bind(settings => settings.Topic.Match(
            Some: topic => Sent(settings.Server, topic, queued, notice),
            None: static () => IO.pure(unit)));

    private IO<Unit> Sent(NoticeServer server, NoticeTopic topic, QueueEvent queued, Notice notice) =>
        IO.liftAsync(async environment => {
            NtfyMessage message = new(topic, notice.Title, notice.Description, queued.Priority.Key, [.. queued.Tag.ToSeq()]);
            using JsonContent content = JsonContent.Create(message, NtfyContext.Default.NtfyMessage);
            using HttpResponseMessage response = await client.PostAsync(server.Address, content, environment.Token).ConfigureAwait(false);
            return NoticeRefused.Unless(response.StatusCode);
        }).Bind(IO.lift)
        .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Unit>(new NoticeUnreached(server, error)))
        .Catch(static error => error.Is(Errors.Cancelled), static error => cancelToken.Bind(token => IO.fail<Unit>(token.IsCancellationRequested ? error : Errors.TimedOut)));

    // --- [RELEASE]
    public void Dispose() {
        try {
            _ = atomic(() => {
                Option<Option<Disposal<Notification>>> prior = raised.Value;
                raised.Value = None;
                return prior;
            }).Iter(static prior => DisposalOps.Release(prior.ToSeq()).Run());
        } finally {
            client.Dispose();
        }
    }
}
