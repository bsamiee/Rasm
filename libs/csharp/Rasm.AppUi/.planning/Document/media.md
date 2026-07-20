# [APPUI_RICH_CONTENT_MEDIA]

A rich-content-and-media owner renders markdown to live Avalonia inlines and plays image/svg/video/audio through one `MediaSurface` over codec rows, so documentation cells, help, and embedded media become first-class content surfaces beside the code editor. `MarkdownInlineRenderer` walks the `Theme/typography` `MarkdownRow`/`InlineRun` projection into theme-token-styled `Avalonia.Controls.Documents` inlines (the retained materialization the typography projection produces rows for but does not itself mount) with a link-hit table for pointer resolution, and `MediaSurface` is the `[Union]` over image/svg/video/audio codec rows whose materialized control crosses to its host through the one `Shell/hosts.md` `Surfaces.Mount` rail — `HanumanInstitute.LibMpv.Avalonia` drives video/audio, the admitted `AsyncImageLoader` the image row, and `Avalonia.Svg.Skia` the vector row. The page owns the markdown retained-materialization, the media codec-row union, and the playback transport; it mints no second markdown model (the typography owner holds the AST projection), no second image cache, and no per-surface codec — one content vocabulary serves every rich surface and a new codec is one row (the `[05]-[PROHIBITIONS]` per-surface-AsyncImageLoader and SKSurface-outside-Offscreen clauses hold). The spine is `Theme/typography` `MarkdownProjection`, `Avalonia.Controls.Documents`, `AsyncImageLoader.Avalonia`, `Svg.Controls.Skia.Avalonia`, `HanumanInstitute.LibMpv`/`HanumanInstitute.LibMpv.Avalonia` (`.api/api-libmpv.md`), the `Shell/hosts.md` mount rail, Thinktecture.Runtime.Extensions, and LanguageExt rails.

## [01]-[INDEX]

- [02]-[MARKDOWN_INLINES]: The `MarkdownRow`/`InlineRun` retained materialization into theme-token Avalonia inlines; the link-hit table.
- [03]-[MEDIA_SURFACE]: The `MediaSurface` `[Union]` codec rows materialized for the one `Surfaces.Mount` crossing.
- [04]-[PLAYBACK_TRANSPORT]: One playback transport rail over the libmpv `MpvContext` — transport, track-selection, and loop verbs.

## [02]-[MARKDOWN_INLINES]

- Owner: `MarkdownInlineRenderer` the `MarkdownRow`/`InlineRun`-to-Avalonia-inline materialization; `MarkdownRendered` the inline collection plus the span-keyed link-hit table; `ContentFault` the typed fault family on the `AppUiFaultBand.Content` registry row (6410).
- Cases: `ContentFault` = Text | UnresolvedRole | CodecAbsent | DecodeFailed.
- Entry: `public static MarkdownRendered Render(MarkdownDocumentRows rows, FontChain chain)` — materializes the inline-bearing `Theme/typography` rows into one `InlineCollection` plus the span-keyed `LinkHit` table. Block rows retain their typed payload for the code, mathematics, and retained-control consumers; this owner does not advertise a block entrypoint it does not implement.
- Auto: the markdown AST projection is owned by `Theme/typography` (`MarkdownProjection`, the closed eleven-arm fold to `MarkdownRow`/`InlineRun`) — this renderer consumes those rows and never re-parses. Each `InlineRun` materializes the landed content vocabulary: `InlineContent` = Text | Code | Math | Break | Task | Opaque dispatches through the generated total `Switch`, the `FrozenSet<InlineStyle>` rows (`Strong`, `Emphasis`, `Strike`) fold to decorations and wrappers, and `LinkTarget` discriminates the hit-table hyperlink from the inline image — an image link materializes through the SAME shared `ImageLoader.AsyncImageLoader` cache the `[03]` image codec row rides, never a second loader. Code and inline mathematics resolve through the mono typography role until the math-layout consumer replaces the retained run; the round-trip `SourceSpan` maps each retained run to its source range.
- Packages: Markdig, Avalonia, AsyncImageLoader.Avalonia, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new `InlineContent` case is one content arm the generated dispatch breaks at compile time; a new `InlineStyle` row is one decoration fold arm; a new `MarkdownRow` case breaks the row dispatch and requires an explicit routing verdict.
- Boundary: the renderer dispatches all eleven current `MarkdownRow` cases — `Heading`, `Paragraph`, `Quote`, `Callout`, `ListRows`, `Definitions`, `Grid`, `CodeFence`, `Math`, `Rule`, and `Opaque`. Inline-bearing rows materialize here; block rows return an explicit empty inline projection and retain their typed payload for their owning consumer. A `Markdig` re-parse, silent catch-all, a retired flat-column `InlineRun` read, or a claim that an empty inline projection rendered a block is rejected.

```csharp signature
[Union]
public abstract partial record ContentFault : Expected, IValidationError<ContentFault> {
    private ContentFault(string detail, int code) : base(detail, code, None) { }

    public static ContentFault Create(string message) => new Text(message);

    public sealed record Text : ContentFault { public Text(string detail) : base(detail, AppUiFaultBand.Content.Code(0)) { } }
    public sealed record UnresolvedRole : ContentFault { public UnresolvedRole(string detail) : base(detail, AppUiFaultBand.Content.Code(1)) { } }
    public sealed record CodecAbsent : ContentFault { public CodecAbsent(string detail) : base(detail, AppUiFaultBand.Content.Code(2)) { } }
    public sealed record DecodeFailed : ContentFault { public DecodeFailed(string detail) : base(detail, AppUiFaultBand.Content.Code(3)) { } }
}

public readonly record struct LinkHit(SourceSpan Span, string Url);

public sealed record MarkdownRendered(InlineCollection Inlines, Seq<LinkHit> Links);

public static class MarkdownInlineRenderer {
    public static MarkdownRendered Render(MarkdownDocumentRows rows, FontChain chain) {
        InlineCollection collection = [];
        Seq<LinkHit> links = rows.Body.Fold(Seq<LinkHit>(), (acc, row) => {
            (Seq<Inline> inlines, Seq<LinkHit> hits) = Inlines(row, chain);
            inlines.Iter(collection.Add);
            return acc + hits;
        });
        return new MarkdownRendered(collection, links);
    }

    // Inline-bearing arms project runs; block rows retain their typed payload. The generated switch is
    // exhaustive over the closed eleven-arm family, so a new row case breaks this dispatch.
    static (Seq<Inline> Inlines, Seq<LinkHit> Links) Inlines(MarkdownRow row, FontChain chain) => row.Switch(
        state: chain,
        heading: static (c, heading) => Styled(heading.Runs, heading.Role, c),
        paragraph: static (c, paragraph) => Styled(paragraph.Runs, TypographyRole.Body, c),
        quote: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        callout: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        listRows: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        definitions: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        grid: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        codeFence: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        math: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        rule: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()),
        opaque: static (_, _) => (Seq<Inline>(), Seq<LinkHit>()));

    // Content dispatch is the generated total Switch over the landed six-case InlineContent family;
    // styles fold from the FrozenSet rows, and the link target discriminates hit-table hyperlink from
    // inline image — the image rides the ONE shared AsyncImageLoader cache the media codec row uses.
    static (Seq<Inline>, Seq<LinkHit>) Styled(Seq<InlineRun> runs, TypographyRole role, FontChain chain) =>
        runs.Fold((Inlines: Seq<Inline>(), Links: Seq<LinkHit>()), (acc, run) => {
            TextStyleRow style = TextStyleRow.Resolve(run.Content is InlineContent.Code or InlineContent.Math ? TypographyRole.Code : role, chain);
            bool strike = run.Styles.Contains(InlineStyle.Strike);
            bool linked = run.Link.Exists(static target => target is LinkTarget.Hyperlink);
            Inline inline = run.Content.Switch<(TextStyleRow Style, bool Strike, bool Linked), Inline>(
                state: (style, strike, linked),
                text: static (s, t) => Dressed(t.Value, s.Style, s.Strike, s.Linked),
                code: static (s, c) => Dressed(c.Value, s.Style, s.Strike, s.Linked),
                math: static (s, m) => Dressed(m.Value, s.Style, s.Strike, s.Linked),
                @break: static (_, _) => new LineBreak(),
                task: static (s, t) => Dressed(t.Checked ? "☑ " : "☐ ", s.Style, s.Strike, s.Linked),
                opaque: static (s, _) => Dressed(string.Empty, s.Style, s.Strike, s.Linked));
            inline = run.Styles.Contains(InlineStyle.Strong) ? new Bold { Inlines = { inline } } : inline;
            inline = run.Styles.Contains(InlineStyle.Emphasis) ? new Italic { Inlines = { inline } } : inline;
            return run.Link.Match(
                Some: target => target.Switch(
                    state: (Acc: acc, Inline: inline, run.Span),
                    hyperlink: static (s, link) => (s.Acc.Inlines.Add(s.Inline), s.Acc.Links.Add(new LinkHit(s.Span, link.Destination))),
                    image: static (s, image) => (s.Acc.Inlines.Add(InlineImage(image.Destination)), s.Acc.Links)),
                None: () => (acc.Inlines.Add(inline), acc.Links));
        });

    static Inline Dressed(string text, TextStyleRow style, bool strike, bool linked) =>
        new Run(text) {
            FontFamily = new FontFamily(style.Family), FontSize = style.Size, FontWeight = (FontWeight)style.Weight,
            TextDecorations = (strike, linked) switch {
                (true, true) => [.. TextDecorations.Strikethrough, .. TextDecorations.Underline],
                (true, false) => TextDecorations.Strikethrough,
                (false, true) => TextDecorations.Underline,
                (false, false) => null,
            },
        };

    static Inline InlineImage(string destination) =>
        new InlineUIContainer(new AdvancedImage(new Uri(destination)) { Source = destination, Loader = ImageLoader.AsyncImageLoader });
}
```

## [03]-[MEDIA_SURFACE]

- Owner: `MediaSurface` the `[Union]` codec-row family; `PlaybackPolicy` the admitted playback envelope; `MediaLease` the control-plus-native lifetime capsule; `MediaReceipt` the materialization evidence.
- Cases: `MediaSurface` = Image | Svg | Video | Audio under the locked kind literals — image rides the admitted `AsyncImageLoader`, vector rides the `Avalonia.Svg.Skia` `Svg` control, video and audio ride `HanumanInstitute.LibMpv.Avalonia`.
- Entry: `public static IO<Fin<MediaLease>> Materialize(MediaSurface surface, Func<MediaReceipt, IO<Unit>> sink, ClockPolicy clocks)` — the ONE codec dispatch: projects each row onto an owned lease; video/audio intake completes on the rail before the lease returns, and every native context releases on failed intake or lease disposal.
- Auto: the `Image` case assigns `AdvancedImage.Source` and the global `ImageLoader.AsyncImageLoader`, so intake uses the one shared cache; `FallbackImage`, `IsLoading`, and `CurrentImage` remain host-bindable control projections rather than fabricated receipt fields. The `Svg` case assigns the catalogued `Path`, and video/audio compose `MpvView` on `VideoRenderer.OpenGl`.
- Receipt: `MediaReceipt` — surface key, codec kind, source identity, mount outcome, `Instant`; the mounted and failed instruments contribute inward through `MediaSurfaces.TelemetryRow`, and the receipt seals through its `Diagnostics/evidence.md#RECEIPT_UNION` `EvidenceReceipt.Media` case.
- Packages: AsyncImageLoader.Avalonia, Svg.Controls.Skia.Avalonia, HanumanInstitute.LibMpv, HanumanInstitute.LibMpv.Avalonia, Avalonia, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new codec is one `MediaSurface` case with one `Materialize` arm; one media instrument is one `InstrumentRow` on `MediaSurfaces.TelemetryRow`; zero new surface.
- Boundary: the media vocabulary is the one `MediaSurface` union — a per-surface codec, a second image cache, and a parallel video player are the rejected forms; the materialized control crosses to its host through the ONE `Shell/hosts.md` `Surfaces.Mount(SurfaceHost, SurfaceSeam, Control, ClockPolicy, CorrelationId)` rail composed at the shell edge — `SurfaceSeam` carries mount delegate COLUMNS, not a mount method, so a media-local `seam.Mount(view)` spelling is a phantom and the host crossing is never re-derived here; source intake runs on the IO rail BEFORE the control returns — a mid-pipeline `.Run()` whose `Fin` is discarded is the deleted form, so a load failure reaches the caller as `ContentFault` and a mounted control never represents a failed intake; the video/audio row is `HanumanInstitute.LibMpv.Avalonia` on the OpenGL render path so a bundled libmpv native binary and a `NativeControlHost` airspace embedding are the rejected forms (`.api/api-libmpv.md` reject law), the libmpv native provisioning at the app-host distribution layer; the media surface never owns an `SKSurface` — its render rides the libmpv GL path, the `Svg` control's engine, and the image cache, so an `SKSurface` outside the `Offscreen` capsule is the `[05]-[PROHIBITIONS]` rejected form; playback control flows through the `MpvContext` the bound `IVideoView` exposes, never a hand-rolled mpv command marshaller (`.api/api-libmpv.md` reject); every `MpvContext`/view/overlay disposes through `IVideoView.Dispose` at teardown so the render context releases.

```csharp signature
// Key and Source are BASE positional columns threaded through the case constructors — a computed base
// projection sharing a case parameter name suppresses positional-property synthesis, silently discards
// the constructor argument (CS8907), and recurses at first read.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MediaSurface(string Key, string Source) {
    public sealed record Image(string Key, string Source, Stretch Stretch) : MediaSurface(Key, Source);
    public sealed record Svg(string Key, string Source) : MediaSurface(Key, Source);
    public sealed record Video(string Key, string Source, PlaybackPolicy Playback) : MediaSurface(Key, Source);
    public sealed record Audio(string Key, string Source, PlaybackPolicy Playback) : MediaSurface(Key, Source);

    public string Kind => Switch(image: static _ => "image", svg: static _ => "svg", video: static _ => "video", audio: static _ => "audio");
}

[SmartEnum<string>]
public sealed partial class LoopMode {
    public static readonly LoopMode None = new("none");
    public static readonly LoopMode File = new("file");
}

[ComplexValueObject]
public sealed partial class PlaybackPolicy {
    public bool AutoPlay { get; }
    public LoopMode Loop { get; }
    public bool Muted { get; }
    public double Rate { get; }
    public Option<double> Start { get; }
    public Option<double> Stop { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool autoPlay,
        ref LoopMode loop,
        ref bool muted,
        ref double rate,
        ref Option<double> start,
        ref Option<double> stop) =>
        validationError = !double.IsFinite(rate) || rate <= 0d
            || start.Exists(static value => !double.IsFinite(value) || value < 0d)
            || stop.Exists(static value => !double.IsFinite(value) || value < 0d)
            || (start, stop).Apply(static (from, to) => from >= to).IfNone(false)
                ? new ValidationError("playback policy carries an invalid rate or section interval")
                : validationError;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MediaOutcome {
    private MediaOutcome() { }
    public sealed record Ready : MediaOutcome;
    public sealed record Failed(ContentFault Fault) : MediaOutcome;
}

public sealed class MediaLease : IDisposable {
    private readonly Action release;
    private int disposed;

    public MediaLease(Control control, Action release) { Control = control; this.release = release; }
    public Control Control { get; }
    public void Dispose() { if (Interlocked.Exchange(ref disposed, 1) == 0) release(); }
}

public sealed record MediaReceipt(string Key, string Codec, string Source, MediaOutcome Outcome, Instant At) {
    public const string Kind = "media";
}

public static class MediaSurfaces {
    public const string MountedInstrument = "rasm.appui.media.mounted";
    public const string FailedInstrument = "rasm.appui.media.failed";

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version,
            new(MountedInstrument, InstrumentKind.Count, "{mount}", "media surfaces mounted by codec"),
            new(FailedInstrument, InstrumentKind.Count, "{mount}", "media mounts failed by codec"));

    // The one codec dispatch: intake runs ON the rail — a video/audio load failure folds before the
    // control exists, and the receipt seals mounted and failed alike through the composition-bound sink.
    public static IO<Fin<MediaLease>> Materialize(MediaSurface surface, Func<MediaReceipt, IO<Unit>> sink, ClockPolicy clocks) =>
        surface.Switch<(Func<MediaReceipt, IO<Unit>> Sink, ClockPolicy Clocks), IO<Fin<MediaLease>>>(
            state: (sink, clocks),
            image: static (ctx, i) => Sealed(ctx, i, IO.lift(() =>
                Fin<MediaLease>.Succ(new MediaLease(
                    new AdvancedImage(new Uri(i.Source)) { Source = i.Source, Stretch = i.Stretch, Loader = ImageLoader.AsyncImageLoader },
                    static () => { })))),
            svg: static (ctx, s) => Sealed(ctx, s, IO.lift(() =>
                Fin<MediaLease>.Succ(new MediaLease(new Avalonia.Svg.Skia.Svg { Path = s.Source }, static () => { })))),
            video: static (ctx, v) => Sealed(ctx, v, Wire(v.Source, v.Playback)),
            audio: static (ctx, a) => Sealed(ctx, a, Wire(a.Source, a.Playback)));

    static IO<Fin<MediaLease>> Sealed((Func<MediaReceipt, IO<Unit>> Sink, ClockPolicy Clocks) ctx, MediaSurface surface, IO<Fin<MediaLease>> mount) =>
        mount.Bind(outcome => ctx.Sink(new MediaReceipt(
                surface.Key,
                surface.Kind,
                surface.Source,
                outcome.Match<MediaOutcome>(
                    Succ: static _ => new MediaOutcome.Ready(),
                    Fail: static error => new MediaOutcome.Failed(error is ContentFault fault ? fault : new ContentFault.DecodeFailed(error.Message))),
                ctx.Clocks.Now))
            .Map(_ => outcome));

    // The wired mount: MpvContext binds onto MpvContextProperty, AutoPlay/Loop land as Pause/LoopFile
    // options, and the ONE PlaybackTransport.Load rail completes BEFORE the view returns — a load
    // failure folds ContentFault.DecodeFailed on the rail, never a mounted control over a dead source.
    static IO<Fin<MediaLease>> Wire(string source, PlaybackPolicy policy) =>
        IO.lift(() => {
            MpvContext context = new();
            MpvView view = new() { Renderer = VideoRenderer.OpenGl };
            view.SetValue(MpvView.MpvContextProperty, context);
            context.ObserveProperty(1, "time-pos", MpvFormat.Double);
            context.ObserveProperty(2, "duration", MpvFormat.Double);
            context.ObserveProperty(3, "time-remaining", MpvFormat.Double);
            context.ObserveProperty(4, "pause", MpvFormat.Flag);
            context.ObserveProperty(5, "seeking", MpvFormat.Flag);
            context.ObserveProperty(6, "eof-reached", MpvFormat.Flag);
            context.Pause.Set(!policy.AutoPlay);
            context.Mute.Set(policy.Muted);
            context.Speed.Set(policy.Rate);
            policy.Start.Iter(context.TimePos.Set);
            policy.Stop.Iter(stop => context.AbLoopB.Set(stop.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)));
            policy.Start.Filter(_ => policy.Stop.IsSome).Iter(start => context.AbLoopA.Set(start.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)));
            (policy.Loop == LoopMode.File ? Some(unit) : Option<Unit>.None).Iter(_ => context.LoopFile.Set("inf"));
            return (Context: context, View: view);
        })
        .Bind(wired => (PlaybackTransport.Load(wired.Context, source)
            .Map(_ => Fin<MediaLease>.Succ(new MediaLease(wired.View, wired.View.Dispose)))
            | @catch<IO, Fin<MediaLease>>(static _ => true, error => {
                wired.View.Dispose();
                return IO.pure(Fin.Fail<MediaLease>(new ContentFault.DecodeFailed(error.Message)));
            })).As());
}
```

## [04]-[PLAYBACK_TRANSPORT]

- Owner: `PlaybackTransport` the one playback transport over the libmpv `MpvContext`; `TransportVerb` the payload-bearing verb `[Union]`; `TrackKind` `[SmartEnum<string>]` the track-selection axis; `TransportState` the observed position-and-state snapshot.
- Entry: `public IO<Unit> Load(MpvContext context, string source)` — opens media through `LoadFile`; `public IO<Unit> Command(MpvContext context, TransportVerb verb)` — the ONE total dispatch folding every verb case onto its `MpvContext` member, never a per-control playback handler; `public IObservable<TransportState> Observe(MpvContext context)` — the observed state projection off `ObserveProperty` registrations and the `PropertyChanged` event.
- Auto: the verb family is a `[Union]` because seek, speed, volume, mute, track selection, and section loops carry per-occurrence payloads — play/pause fold onto the `Pause` option, seek onto the `TimePos` property write, speed/volume/mute onto their options, frame step onto `FrameStep`/`FrameBackStep`, stop onto `Stop`, the scrub revert onto `RevertSeek`, active-track selection onto the `AudioId`/`SubId`/`VideoId` option rows keyed by the `TrackKind` axis, an external subtitle onto the `SubAdd` command, and the A-B section loop onto the `AbLoopA`/`AbLoopB` option pair (`.api/api-libmpv.md` transport commands, track members, and properties); a new verb is one case that breaks the total `Switch` at compile time; position and state surface through the observed `MpvPropertyRead` members (`TimePos`, `Duration`, `TimeRemaining`, `Pause`, `Seeking`, `EofReached`) registered once through `ObserveProperty` and folded from `PropertyChanged`, so the surface never polls libmpv on a timer; each verb derives its `Intent` key symbolically from its case so the media-control toolbar rows derive from the one command table with zero literal drift.
- Packages: HanumanInstitute.LibMpv, System.Reactive, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new transport verb is one `TransportVerb` case folding onto its `MpvContext` member; a new selectable track lane is one `TrackKind` row; zero new surface.
- Boundary: playback transport is the one rail over the typed `MpvContext` — a hand-rolled mpv command/property marshaller is the rejected form (`.api/api-libmpv.md` reject), so transport verbs fold onto the named `MpvContext` members and command intake rides the catalogued `MpvCommand` `InvokeAsync` deferred invocation; position surfaces through observed `MpvPropertyRead`/`PropertyChanged`, never a polling timer; transport verbs derive as `CommandIntent` rows executed through the command deck, so playback evidence rides the deck's `CommandReceipt` stream and a transport-local receipt or command registry is the deleted form; a transient scrub seeks the live position and `RevertSeek` returns to the pre-scrub mark, so scrub-and-revert rides the libmpv transport rather than a snapshot; the `AudioId`/`SubId`/`VideoId` rows are `MpvOptionWithAutoNo<int>` sentinel wrappers — the typed id write rides the option base and the `auto`/`no` sentinel members ride the catalogued wrapper, never a raw property string.

```csharp signature
[SmartEnum<string>]
public sealed partial class TrackKind {
    public static readonly TrackKind Audio = new("audio");
    public static readonly TrackKind Subtitle = new("subtitle");
    public static readonly TrackKind Video = new("video");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransportVerb {
    private TransportVerb() { }
    public sealed record Play : TransportVerb;
    public sealed record Pause : TransportVerb;
    public sealed record Seek(double Seconds) : TransportVerb;
    public sealed record Speed(double Rate) : TransportVerb;
    public sealed record Volume(double Level) : TransportVerb;
    public sealed record Mute(bool Muted) : TransportVerb;
    public sealed record Step(bool Back) : TransportVerb;
    public sealed record Revert : TransportVerb;
    public sealed record Track(TrackKind Lane, int Id) : TransportVerb;
    public sealed record Subtitle(string Path) : TransportVerb;
    public sealed record SectionLoop(double From, double To) : TransportVerb;
    public sealed record Stop : TransportVerb;

    public string Kind => Switch(
        play: static _ => "play", pause: static _ => "pause", seek: static _ => "seek",
        speed: static _ => "speed", volume: static _ => "volume", mute: static _ => "mute",
        step: static _ => "step", revert: static _ => "revert", track: static _ => "track",
        subtitle: static _ => "subtitle", sectionLoop: static _ => "section-loop", stop: static _ => "stop");

    public string Intent => $"media.{Kind}";
}

public readonly record struct TransportState(double Position, double Duration, double Remaining, bool Playing, bool Seeking, bool Ended);

public static class PlaybackTransport {
    public static IO<Unit> Load(MpvContext context, string source) =>
        IO.liftAsync(async () => { await context.LoadFile(source).InvokeAsync().ConfigureAwait(false); return unit; });

    // The one verb dispatch: options and property writes ride their typed SetAsync, command verbs ride
    // the MpvCommand InvokeAsync dual — no raw mpv command strings anywhere.
    public static IO<Unit> Command(MpvContext context, TransportVerb verb) => verb.Switch(
        state: context,
        play:        static (mpv, _) => Write(mpv.Pause, false),
        pause:       static (mpv, _) => Write(mpv.Pause, true),
        seek:        static (mpv, s) => IO.liftAsync(async () => { await mpv.TimePos.SetAsync(s.Seconds).ConfigureAwait(false); return unit; }),
        speed:       static (mpv, s) => Write(mpv.Speed, s.Rate),
        volume:      static (mpv, v) => Write(mpv.Volume, v.Level),
        mute:        static (mpv, m) => Write(mpv.Mute, m.Muted),
        step:        static (mpv, s) => Invoke(s.Back ? mpv.FrameBackStep() : mpv.FrameStep()),
        revert:      static (mpv, _) => Invoke(mpv.RevertSeek(default)),
        track:       static (mpv, t) => t.Lane.Switch(
            state: (Mpv: mpv, t.Id),
            audio:    static (s, _) => IO.liftAsync(async () => { await s.Mpv.AudioId.SetAsync(s.Id).ConfigureAwait(false); return unit; }),
            subtitle: static (s, _) => IO.liftAsync(async () => { await s.Mpv.SubId.SetAsync(s.Id).ConfigureAwait(false); return unit; }),
            video:    static (s, _) => IO.liftAsync(async () => { await s.Mpv.VideoId.SetAsync(s.Id).ConfigureAwait(false); return unit; })),
        subtitle:    static (mpv, s) => Invoke(mpv.SubAdd(s.Path)),
        sectionLoop: static (mpv, l) => IO.liftAsync(async () => {
            await mpv.AbLoopA.SetAsync(l.From.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)).ConfigureAwait(false);
            await mpv.AbLoopB.SetAsync(l.To.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)).ConfigureAwait(false);
            return unit;
        }),
        stop:        static (mpv, _) => Invoke(mpv.Stop()));

    // ObserveProperty registers the position/duration/remaining/pause/seeking/eof feeds once; every
    // PropertyChanged tick re-projects one immutable snapshot — the polling timer is the deleted form.
    public static IObservable<TransportState> Observe(MpvContext context) =>
        Observable.FromEventPattern<MpvPropertyEventArgs>(
                handler => context.PropertyChanged += handler,
                handler => context.PropertyChanged -= handler)
            .Select(_ => new TransportState(
                context.TimePos.Get() ?? 0d,
                context.Duration.Get() ?? 0d,
                context.TimeRemaining.Get() ?? 0d,
                !(context.Pause.Get() ?? true),
                context.Seeking.Get() ?? false,
                context.EofReached.Get() ?? false));

    static IO<Unit> Write<T>(MpvOption<T> option, T value) where T : struct =>
        IO.liftAsync(async () => { await option.SetAsync(value).ConfigureAwait(false); return unit; });

    static IO<Unit> Invoke(MpvCommand command) =>
        IO.liftAsync(async () => { await command.InvokeAsync().ConfigureAwait(false); return unit; });
}
```

```mermaid
flowchart LR
    MarkdownDocumentRows --> MarkdownInlineRenderer
    MarkdownInlineRenderer --> MarkdownRendered
    MarkdownRendered --> TextStyleRow
    MediaSurface -->|Materialize| Control
    Control -->|Surfaces.Mount| SurfaceSession
    MediaSurface -->|Image| AsyncImageLoader
    MediaSurface -->|Svg| SvgControl["Avalonia.Svg.Skia Svg"]
    MediaSurface -->|Video/Audio| MpvView
    MediaSurface --> MediaReceipt
    MediaReceipt --> ReceiptSinkPort
```

## [05]-[RESEARCH]

(none)
