using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Rhino;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NoticeSettings(Uri Server, Option<string> Topic);

[SmartEnum<int>]
public sealed partial class NoticePriority {
    public static readonly NoticePriority Default = new(3);
    public static readonly NoticePriority High = new(4);
    public static readonly NoticePriority Max = new(5);
}

[Union]
public abstract partial record QueueEvent {
    private QueueEvent(NoticePriority priority, Option<string> tag) => (Priority, Tag) = (priority, tag);

    public NoticePriority Priority { get; }
    public Option<string> Tag { get; }

    public string Title => $"{Session.PlugIn.Name} {GetType().Name}";

    public abstract string Description(DateTimeZone zone);

    public sealed record Started(int Entries, long Frames) : QueueEvent(NoticePriority.Default, "racehorse") {
        public override string Description(DateTimeZone zone) => LocalText.Of("{0} frames across {1} entries", Frames, Entries).Local;
    }

    public sealed record Submitted(Uri Job) : QueueEvent(NoticePriority.Default, None) {
        public override string Description(DateTimeZone zone) => LocalText.Of("Flamenco job {0}", Job).Local;
    }

    public sealed record Estimate(QueueEstimate Reading) : QueueEvent(NoticePriority.Default, "hourglass") {
        public override string Description(DateTimeZone zone) => Reading.Description(zone);
    }

    public sealed record Complete(long Frames, Duration Elapsed) : QueueEvent(NoticePriority.Default, "white_check_mark") {
        public override string Description(DateTimeZone zone) => LocalText.Of("{0} frames in {1:-H:mm:ss}", Frames, Elapsed).Local;
    }

    public sealed record Stopped(long Frames) : QueueEvent(NoticePriority.High, "warning") {
        public override string Description(DateTimeZone zone) => LocalText.Of("{0} frames rendered", Frames).Local;
    }

    public sealed record Failed(Error Error) : QueueEvent(NoticePriority.Max, "x") {
        public override string Description(DateTimeZone zone) => Session.Messages(Error);
    }

    public sealed record RetriesExhausted(int Retries) : QueueEvent(NoticePriority.Max, "x") {
        public override string Description(DateTimeZone zone) => LocalText.Of("Queue crashed after {0} restarts", Retries).Local;
    }
}

internal sealed record NtfyMessage(string Topic, string Title, string Message, int Priority, string[] Tags);

[JsonSerializable(typeof(NtfyMessage))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
internal sealed partial class NtfyContext : JsonSerializerContext;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed partial class NoticeSender : IDisposable {
    private readonly HttpClient client = new() { Timeout = Duration.FromSeconds(10).ToTimeSpan() };
    private readonly IO<NoticeSettings> settings;
    private readonly TimeProvider clock;

    private NoticeSender(IO<NoticeSettings> settings, TimeProvider clock) => (this.settings, this.clock) = (settings, clock);

    // --- [ACQUISITION]
    public static IO<NoticeSender> Acquire(IO<NoticeSettings> settings, TimeProvider clock) => use(() => new NoticeSender(settings, clock));

    // --- [DELIVERY]
    public IO<Unit> Notify(QueueEvent queued) =>
        (from description in IO.lift(() => queued.Description(clock.ToZonedClock().Zone))
         from logged in IO.lift(() => Noticed(Session.Logger, queued.Title, description))
         from held in settings
         from sent in held.Topic.Traverse(topic => Sent(held.Server, topic, queued, description)).As()
         select unit)
            .Catch(static error => !Session.Cancellation(error), static error => IO.lift(() => Session.Failed(error)));

    private IO<Unit> Sent(Uri server, string topic, QueueEvent queued, string description) =>
        IO.liftAsync(async environment => {
            using JsonContent content = JsonContent.Create(new NtfyMessage(topic, queued.Title, description, queued.Priority.Key, [.. queued.Tag.ToSeq()]), NtfyContext.Default.NtfyMessage);
            using HttpResponseMessage response = await client.PostAsync(server, content, environment.Token).ConfigureAwait(false);
            return response.StatusCode is HttpStatusCode.OK ? Fin.Succ(unit) : new DarkroomFault($"notice post to {server} answering {response.StatusCode}", DarkroomReason.Refused);
        }).Bind(IO.lift)
        .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Unit>(new DarkroomFault($"notice post to {server}", DarkroomReason.Failed, Inner: error)))
        .Catch(static error => error.Is(Errors.Cancelled), static error => cancelToken.Bind(token => IO.fail<Unit>(token.IsCancellationRequested ? error : Errors.TimedOut)));

    [LoggerMessage(Level = LogLevel.Information, Message = "{Title}: {Description}")]
    private static partial void Noticed(ILogger logger, string title, string description);

    // --- [RELEASE]
    public void Dispose() => client.Dispose();
}
