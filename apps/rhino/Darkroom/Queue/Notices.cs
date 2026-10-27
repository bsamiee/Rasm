using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.UI;
using Rasm.UI;
using Rhino.Runtime.Notifications;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NoticeSettings(Uri Server, string? Topic) {
    public static NoticeSettings Default { get; } = new(new Uri("https://ntfy.sh"), Topic: null);
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

    public string Title => $"{nameof(QueueState)} {GetType().Name}";

    public abstract string Description(DateTimeZone zone, IPlugInViews views);

    public sealed record Started(int Entries, long Frames) : QueueEvent(NoticePriority.Default, "racehorse", fresh: true) {
        public override string Description(DateTimeZone zone, IPlugInViews views) => RowText.Localize("{0} frames across {1} entries", views, arguments: [Frames, Entries]).Local;
    }

    public sealed record Submitted(Uri Job) : QueueEvent(NoticePriority.Default, None, fresh: true) {
        public override string Description(DateTimeZone zone, IPlugInViews views) => RowText.Localize("Flamenco job {0}", views, arguments: [Job]).Local;
    }

    public sealed record Estimate(QueueEstimate Reading) : QueueEvent(NoticePriority.Default, "hourglass", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInViews views) => Reading.Description(zone, views);
    }

    public sealed record Complete(long Frames, Duration Elapsed) : QueueEvent(NoticePriority.Default, "white_check_mark", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInViews views) => RowText.Localize("{0} frames in {1:-H:mm:ss}", views, arguments: [Frames, Elapsed]).Local;
    }

    public sealed record Stopped(long Frames) : QueueEvent(NoticePriority.High, "warning", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInViews views) => RowText.Localize("{0} frames rendered", views, arguments: [Frames]).Local;
    }

    public sealed record Failed(Error Error) : QueueEvent(NoticePriority.Max, "x", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInViews views) => ErrorOps.Messages(Error);
    }

    public sealed record RetriesExhausted(int Retries) : QueueEvent(NoticePriority.Max, "x", fresh: false) {
        public override string Description(DateTimeZone zone, IPlugInViews views) => RowText.Localize("Queue crashed after {0} restarts", views, arguments: [Retries]).Local;
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
    private readonly Ref<Option<Lease<Notification>>> raised = Ref(Option<Lease<Notification>>.None);
    private readonly IO<NoticeSettings> settings;
    private readonly IPlugInViews views;

    private NoticeSender(IO<NoticeSettings> settings, IPlugInViews views) => (this.settings, this.views) = (settings, views);

    // --- [ACQUISITION]
    public static IO<NoticeSender> Acquire(IO<NoticeSettings> settings, IPlugInViews views) => use(() => new NoticeSender(settings, views));

    // --- [DELIVERY]
    public IO<Unit> Notify(QueueEvent queued) =>
        IO.lift(() => new Notice(RowText.Localize(queued.Title, views).Local, queued.Description(views.Clock.ToZonedClock().Zone, views), None, queued.Priority.Severity, Some(Mark), None))
            .OnUiThread()
            .Bind(notice => Callbacks.Each(Seq(Noticed(queued.Fresh, notice).OnUiThread(), Notifications.Show(notice).OnUiThread(), Posted(queued, notice))))
            .Map(unit)
            .Catch(static error => !error.Is(Errors.Cancelled), error => Reported(error));

    private IO<Unit> Noticed(bool fresh, Notice notice) =>
        IO.lift(() => raised.Value).Bind(held => held.Filter(_ => !fresh).Match(
            Some: kept => Status.Revise(kept.Value, notice),
            None: () => views.Session.Held(Products.Swapped(raised, Status.Show(notice, None, views))).Map(static _ => unit)));

    private IO<Unit> Posted(QueueEvent queued, Notice notice) =>
        settings.OnUiThread().Bind(held => Optional(held.Topic).Match(
            Some: topic => Sent(held.Server, topic, queued, notice),
            None: static () => IO.pure(unit)));

    private IO<Unit> Sent(Uri server, string topic, QueueEvent queued, Notice notice) =>
        IO.liftAsync(async environment => {
            using JsonContent content = JsonContent.Create(new NtfyMessage(topic, notice.Title, notice.Description, queued.Priority.Key, [.. queued.Tag.ToSeq()]), NtfyContext.Default.NtfyMessage);
            using HttpResponseMessage response = await client.PostAsync(server, content, environment.Token).ConfigureAwait(false);
            return response.StatusCode is HttpStatusCode.OK ? Fin.Succ(unit) : Error.New($"{server} answered notice with {response.StatusCode}");
        }).Bind(IO.lift)
        .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Unit>(Error.New($"notice post to {server} failed", error)))
        .Catch(static error => error.Is(Errors.Cancelled), static error => cancelToken.Bind(token => IO.fail<Unit>(token.IsCancellationRequested ? error : Errors.TimedOut)));

    private IO<Unit> Reported(Error error, [CallerMemberName] string member = "") => IO.lift(() => PlugInLog.Of(views).Failed(error, typeof(NoticeSender), member));

    // --- [RELEASE]
    public void Dispose() => client.Dispose();
}
