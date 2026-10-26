using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.Rhino;
using Rasm.Rhino.UI;
using Rasm.UI;
using Rhino.Runtime.Notifications;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HttpServer {
    public Uri Address => new(_value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = Uri.TryCreate(value, UriKind.Absolute, out Uri? address)
            && (string.Equals(address.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) || string.Equals(address.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal))
            && address.Query.Length is 0 && address.Fragment.Length is 0
                ? null
                : new ValidationError($"{value} is no absolute http or https address without query or fragment");
}

[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoticeTopic {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Length is >= 1 and <= 64 && value.All(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
            ? null
            : new ValidationError($"topic {value} needs 1 to 64 ASCII letters, digits, hyphens, or underscores");
}

public sealed record NoticeSettings(HttpServer Server, Option<NoticeTopic> Topic) : IStateRecord<NoticeSettings, NoticeParameter> {
    public static NoticeSettings Default { get; } = new(HttpServer.Create("https://ntfy.sh"), None);
}

[SmartEnum<string>]
public sealed partial class NoticeParameter : IStateParameter<NoticeSettings> {
    public static readonly NoticeParameter Server = new("server", new StateParameter<NoticeSettings>.Keyed<HttpServer, string>(
            Lens<NoticeSettings, HttpServer>.New(static notices => notices.Server, static server => notices => notices with { Server = server })),
        new("Server", "ntfy server queue notices post to"));
    public static readonly NoticeParameter Topic = new("topic", new StateParameter<NoticeSettings>.OptionalKeyed<NoticeTopic, string>(
            Lens<NoticeSettings, Option<NoticeTopic>>.New(static notices => notices.Topic, static topic => notices => notices with { Topic = topic })),
        new("Topic", "ntfy topic queue notices post to, empty posts none"));

    public StateParameter<NoticeSettings> Kind { get; }

    public ParameterText Text { get; }
}

[SmartEnum<int>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class NoticePriority {
    public static readonly NoticePriority Default = new(3, Notification.Severity.Info);
    public static readonly NoticePriority High = new(4, Notification.Severity.Warning);
    public static readonly NoticePriority Max = new(5, Notification.Severity.Serious);

    public Notification.Severity Severity { get; }
}

[Union]
public abstract partial record QueueEvent {
    private QueueEvent(NoticePriority priority, Option<string> tag, bool fresh) => (Priority, Tag, Fresh) = (priority, tag, fresh);

    public NoticePriority Priority { get; }
    public Option<string> Tag { get; }
    public bool Fresh { get; }

    public string Title => Captions.Of($"{nameof(RenderQueue)}{GetType().Name}");

    public abstract string Description(DateTimeZone zone, IPlugInSink sink);

    public sealed record Started(int Entries, long Frames) : QueueEvent(NoticePriority.Default, "racehorse", fresh: true) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => RowText.Localize("{0} frames across {1} entries", sink, arguments: [Frames, Entries]).Local;
    }

    public sealed record Submitted(Uri Job) : QueueEvent(NoticePriority.Default, None, fresh: true) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => RowText.Localize("Flamenco job {0}", sink, arguments: [Job]).Local;
    }

    public sealed record Estimate(QueueEstimate Reading) : QueueEvent(NoticePriority.Default, "hourglass", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => Reading.Description(zone, sink);
    }

    public sealed record Complete(long Frames, Duration Elapsed) : QueueEvent(NoticePriority.Default, "white_check_mark", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => RowText.Localize("{0} frames in {1:-H:mm:ss}", sink, arguments: [Frames, Elapsed]).Local;
    }

    public sealed record Stopped(long Frames) : QueueEvent(NoticePriority.High, "warning", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => RowText.Localize("{0} frames rendered", sink, arguments: [Frames]).Local;
    }

    public sealed record Failed(Error Error) : QueueEvent(NoticePriority.Max, "x", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => ErrorOps.Localize(Error);
    }

    public sealed record RetriesExhausted(RetryCount Retries) : QueueEvent(NoticePriority.Max, "x", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => RowText.Localize("Queue crashed after {0} restarts", sink, arguments: [(int)Retries]).Local;
    }

    public sealed record Test() : QueueEvent(NoticePriority.Default, None, fresh: true) {
        public override string Description(DateTimeZone zone, IPlugInSink sink) => RowText.Localize(Title, sink).Local;
    }
}

internal sealed record NtfyMessage(string Topic, string Title, string Message, int Priority, string[] Tags);

[JsonSerializable(typeof(NtfyMessage))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
internal sealed partial class NtfyContext : JsonSerializerContext;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class NoticeSender : IDisposable {
    private static readonly GlyphSource Mark = new GlyphSource.Paired(typeof(NoticeSender).Assembly, "mark-16.svg", "mark-16_darkmode.svg");

    private readonly HttpClient client = new() { Timeout = Duration.FromSeconds(10).ToTimeSpan() };
    private readonly Retained<Disposal<Notification>> raised = new(DisposalOps.Release);
    private readonly IO<NoticeSettings> settings;
    private readonly IPlugInViews views;

    private NoticeSender(IO<NoticeSettings> settings, IPlugInViews views) => (this.settings, this.views) = (settings, views);

    // --- [ACQUISITION]
    public static IO<NoticeSender> Acquire(IO<NoticeSettings> settings, IPlugInViews views, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => new NoticeSender(settings, views)), retain);

    // --- [DELIVERY]
    public IO<Unit> Notify(QueueEvent queued) =>
        IO.lift(() => new Notice(RowText.Localize(queued.Title, views).Local, queued.Description(views.Clock.ToZonedClock().Zone, views), None, queued.Priority.Severity, Some(Mark), None))
            .OnUiThread()
            .Bind(notice => Callbacks.Each(Seq(Noticed(queued.Fresh, notice).OnUiThread(), Notifications.Show(notice).OnUiThread(), Posted(queued, notice))))
            .Map(unit)
            .Catch(static error => !error.Is(Errors.Cancelled), error => IO.lift(() => views.Report(error, typeof(NoticeSender), nameof(Notify))));

    private IO<Unit> Noticed(bool fresh, Notice notice) =>
        IO.lift(() => raised.Held).Bind(held => held.Head.Filter(_ => !fresh).Match(
            Some: kept => Status.Revise(kept, notice),
            None: () => DisposalOps.OnFailure(Status.Show(notice, None, views, cell => raised.Replace(cell).Run()).Map(static _ => unit), raised.Drain)));

    private IO<Unit> Posted(QueueEvent queued, Notice notice) =>
        settings.OnUiThread().Bind(held => held.Topic.Match(
            Some: topic => Sent(held.Server, topic, queued, notice),
            None: static () => IO.pure(unit)));

    private IO<Unit> Sent(HttpServer server, NoticeTopic topic, QueueEvent queued, Notice notice) =>
        IO.liftAsync(async environment => {
            using JsonContent content = JsonContent.Create(new NtfyMessage(topic, notice.Title, notice.Description, queued.Priority.Key, [.. queued.Tag.ToSeq()]), NtfyContext.Default.NtfyMessage);
            using HttpResponseMessage response = await client.PostAsync(server.Address, content, environment.Token).ConfigureAwait(false);
            return response.StatusCode is HttpStatusCode.OK ? Fin.Succ(unit) : Error.New($"{server} answered notice with {response.StatusCode}");
        }).Bind(IO.lift)
        .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Unit>(Error.New($"notice post to {server} failed", error)))
        .Catch(static error => error.Is(Errors.Cancelled), static error => cancelToken.Bind(token => IO.fail<Unit>(token.IsCancellationRequested ? error : Errors.TimedOut)));

    // --- [RELEASE]
    public void Dispose() {
        try {
            raised.Dispose();
        } finally {
            client.Dispose();
        }
    }
}
