using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Chrome;
using Rasm.Rhino.UI.Views;
using Rhino.PlugIns;
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

public sealed record NoticeOptions(IO<NoticeSettings> Settings, Option<IGlyph> Glyph, Option<View.Panel> Reveal);

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

    public Notice Notice(DateTimeZone zone, Option<IGlyph> glyph, Option<View.Panel> reveal) =>
        new(RowText.Localize(Title), Description(zone), None, Priority.Severity, glyph, reveal);

    public sealed record Started(int Entries, int Frames) : QueueEvent(NoticePriority.Default, "racehorse", "Render queue started", fresh: true) {
        public override string Description(DateTimeZone zone) =>
            RowText.Localize("{0} frames across {1} entries", arguments: [Frames, Entries]);
    }

    public sealed record Estimate(QueueEstimate Reading) : QueueEvent(NoticePriority.Default, "hourglass", "Render queue estimate", fresh: false) {
        public override string Description(DateTimeZone zone) => Reading.Description(zone);
    }

    public sealed record Complete(int Frames, Duration Elapsed) : QueueEvent(NoticePriority.Default, "white_check_mark", "Render queue complete", fresh: false) {
        public override string Description(DateTimeZone zone) =>
            RowText.Localize("{0} frames in {1:-H:mm:ss}", arguments: [Frames, Elapsed]);
    }

    public sealed record Stopped(int Frames) : QueueEvent(NoticePriority.High, "warning", "Render queue stopped", fresh: false) {
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
    private readonly Atom<(Option<Disposal<Notification>> Held, Option<Disposal<Notification>> Withdrawn)> raised =
        Atom<(Option<Disposal<Notification>> Held, Option<Disposal<Notification>> Withdrawn)>((None, None));

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(NoticeOptions options, TimeProvider clock) =>
        (_, sink) =>
            from sender in IO.lift(() => new NoticeSender(options, clock, sink))
            let cell = ((IPlugInRendering)sink).Notices
            from published in cell.SwapIO(_ => Some(sender))
            select DisposalOps.Composite(
                Seq<IDisposable>(sender, new Disposal<Atom<Option<NoticeSender>>>(cell, static filled => filled.Swap(static _ => None))),
                new CallbackSite(sink, typeof(NoticeSender), nameof(Register)));

    // --- [DELIVERY]
    public IO<Unit> Notify(QueueEvent queued) =>
        IO.lift(() => queued.Notice(clock.ToZonedClock().Zone, options.Glyph, options.Reveal))
            .Post()
            .Bind(notice => Callbacks.Each(Seq(Noticed(queued.Fresh, notice).Post(), Presence.Show(notice, sink).Post(), Posted(queued, notice))))
            .Map(static _ => unit)
            .Catch(static error => !error.Is(Errors.Cancelled), error => IO.lift(() => sink.Report(error, typeof(NoticeSender), nameof(Notify))));

    private IO<Unit> Noticed(bool fresh, Notice notice) =>
        from turn in raised.SwapIO(state => fresh ? (None, state.Held) : (state.Held, None))
        from withdrawn in DisposalOps.Release(turn.Withdrawn.ToSeq())
        from cell in turn.Held.Match(
            Some: kept => Status.Revise(kept, notice).Map(_ => kept),
            None: () => Status.Show(notice, None, sink))
        from swapped in raised.SwapIO(_ => (Some(cell), None))
        select unit;

    private IO<Unit> Posted(QueueEvent queued, Notice notice) =>
        from settings in options.Settings.Post()
        from posted in settings.Topic.Match(Some: topic => Sent(settings.Server, topic, queued, notice), None: static () => IO.pure(unit))
        select posted;

    private IO<Unit> Sent(NoticeServer server, NoticeTopic topic, QueueEvent queued, Notice notice) =>
        (from token in cancelToken
         let message = new NtfyMessage(topic, notice.Title, notice.Description, queued.Priority.Key, [.. queued.Tag.ToSeq()])
         from sent in use(() => JsonContent.Create(message, NtfyContext.Default.NtfyMessage))
             .Bind(content => use(IO.liftAsync(() => client.PostAsync(server.Address, content, token)))
                 .Bind(static response => unless(response.StatusCode == HttpStatusCode.OK, IO.fail<Unit>(new NoticeRefused(response.StatusCode))).As())
                 .Bracket())
             .Bracket()
         select sent)
        .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Unit>(new NoticeUnreached(server, error)))
        .Catch(static error => error.Is(Errors.Cancelled), static error => cancelToken.Bind(token => IO.fail<Unit>(token.IsCancellationRequested ? error : Errors.TimedOut)));

    // --- [RELEASE]
    public void Dispose() {
        _ = Callbacks.Answer(
            DisposalOps.Release(raised.Value.Held.ToSeq()),
            static () => unit,
            new CallbackSite(sink, typeof(NoticeSender), nameof(Dispose)));
        client.Dispose();
    }
}
