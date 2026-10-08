using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderRun(
    Option<Camera> Camera, Option<TimeSpan> Time, SceneLights Lights, Option<float> Metres,
    Option<ToneMapping> Formation, Option<PixelFrame> Scene, Seq<PixelPass> Late, Option<PixelPass> Pending, Option<PixelFrame> Formed) {
    public static RenderRun Empty { get; } = new(None, None, SceneLights.Empty, None, None, None, [], None, None);

    public RenderRun Tapped(PostEffectType stage, PixelFrame input, Option<PixelPass> pass) =>
        stage switch {
            PostEffectType.ToneMapping => this with { Scene = Some(input), Late = [], Pending = None, Formed = None },
            PostEffectType.Late => this with { Formed = Some(input), Late = Late + Pending.ToSeq(), Pending = pass },
            PostEffectType.Early => this,
        };
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class RenderRuns {
    // --- [STATE]
    private static readonly Atom<Seq<(Guid Rendering, RenderRun Run)>> runs = Atom(Seq<(Guid Rendering, RenderRun Run)>());

    private static readonly Atom<Option<RenderRun>> seed = Atom(Option<RenderRun>.None);

    internal static RenderRun Fill(Guid rendering, Func<RenderRun, RenderRun> change) =>
        (Seeded: seed.Value.IfNone(RenderRun.Empty), Kept: SupportOptions.AutoSaveKeepAmount() - 1) switch {
            var held => runs.Swap(entries =>
                (Rendering: rendering, Run: change(entries.Find(entry => entry.Rendering == rendering).Map(static entry => entry.Run).IfNone(held.Seeded)))
                    .Cons(entries.Filter(entry => entry.Rendering != rendering).Take(held.Kept)))[0].Run,
        };

    // --- [READS]
    public static Option<RenderRun> Find(Guid rendering) => runs.Value.Find(entry => entry.Rendering == rendering).Map(static entry => entry.Run);

    public static Option<Guid> Newest => runs.Value.Find(static entry => entry.Run.Formed.IsSome).Map(static entry => entry.Rendering);

    public static Option<(PixelFrame Formed, PixelFrame Scene)> Rendered =>
        runs.Value.Head.Bind(static latest => (latest.Run.Formed, latest.Run.Scene).Apply(static (formed, scene) => (formed, scene)).As());

    public static FrameSide Formed { get; } = new(
        new FrameFeed(IO.lift(static () => Rendered.Map(static held => held.Formed)), EffectPipeline.Display.Encoding.Transfer, Some(EffectPipeline.Display.Peak), Eto.Drawing.ImageInterpolation.High),
        RowText.Localize("After").Local);

    public static IO<Option<ScopeFrames>> Scoped =>
        IO.lift(static () => Rendered.Map(static held => new ScopeFrames(
            held.Formed, EffectPipeline.Display.Encoding.Gamut, EffectPipeline.Display.Encoding.Transfer, EffectPipeline.Depth.Levels, Some((held.Scene, EffectPipeline.Working)))));

    // --- [FRAMING]
    public static IO<RenderRun> Framing(RhinoDoc doc, ViewInfo view, PixelExtent extent, Option<TimeSpan> time, CallbackSite site) =>
        from lens in IO.lift(() => Cameras.Focus(view, doc.ModelUnits))
            .Catch(static error => error.IsType<InvalidPixelValue>(), error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Map(static _ => Option<LensFocus>.None))
        from camera in IO.lift(() => Cameras.ToCamera(view, doc.ModelUnits, extent, lens))
        from lights in Cameras.ToLights(doc)
        select RenderRun.Empty with { Camera = camera, Time = time, Lights = lights, Metres = (float)LengthUnit.Scale(doc.ModelUnits, LengthUnit.Meters) };

    public static IO<Unit> Framed(Guid rendering, RenderRun framing) =>
        IO.lift(() => Fill(rendering, run => run with { Camera = framing.Camera, Time = framing.Time, Lights = framing.Lights, Metres = framing.Metres })).Map(static _ => unit);

    public static IO<T> Launched<T>(RenderRun framing, IO<T> launch) =>
        seed.SwapIO(_ => Some(framing)).Bind(_ => launch).Finally(seed.SwapIO(static _ => None));

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register { get; } = static (_, sink) => EventKind.ImageFileSaved.Inline(args => Hosted(args, sink), sink);

    private static IO<Unit> Hosted(ImageFileEventArgs args, IPlugInSink sink) =>
        from unframed in IO.lift(() => Find(args.SessionId).ForAll(static run => run.Camera.IsNone))
        from framed in when(unframed, Sourced(args.SessionId, new CallbackSite(sink, typeof(RenderRuns), nameof(Hosted)))).As()
        select framed;

    private static IO<Unit> Sourced(Guid rendering, CallbackSite site) =>
        from doc in IO.lift(static () => Missing.Unless(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)))
        from extent in IO.lift(() => EffectPipeline.Extent(RenderPipeline.RenderSize(doc, fromRenderSources: true)))
        from framing in (from source in use(() => new RenderSourceView(doc))
                         from view in IO.lift(() => Missing.Unless(source.GetViewInfo(), nameof(RenderSourceView.GetViewInfo)))
                         from built in Framing(doc, view, extent, None, site)
                         select built).Bracket()
        from framed in Framed(rendering, framing)
        select framed;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectPipeline {
    // --- [OUTPUT]
    public static readonly Gamut Working = Gamut.StandardRgb;
    public static readonly Imaging.Tone.Formations.Display Display = Imaging.Tone.Formations.Display.Srgb;
    public static readonly OutputDepth Depth = OutputDepth.UInt8;

    // --- [CHANNELS]
    public static RenderWindow.StandardChannels Channel(GuideChannel channel) =>
        channel.Map(
            depth: RenderWindow.StandardChannels.DistanceFromCamera,
            normal: RenderWindow.StandardChannels.NormalXYZ,
            albedo: RenderWindow.StandardChannels.AlbedoRGB,
            objectId: RenderWindow.StandardChannels.ObjectIds,
            materialId: RenderWindow.StandardChannels.MaterialIds);

    public static Guid[] Ids(Seq<GuideChannel> channels) =>
        [RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), .. channels.Map(static channel => RenderWindow.ChannelId(Channel(channel)))];

    public static IO<PixelFrame> Read(Func<Guid, PostEffectChannel?> open, PixelExtent extent, RenderWindow.StandardChannels channel, Rectangle rect) =>
        Opened(open, RenderWindow.ChannelId(channel), (_, cpu) => IO.lift(() => RenderWindows.Frame(cpu, rect, extent)));

    public static IO<HashMap<GuideChannel, PixelFrame>> Guides(
        Func<Guid, PostEffectChannel?> open, PixelExtent extent, Rectangle rect, Seq<GuideChannel> channels, Option<float> metres) =>
        (Depth: metres.Map<Func<PixelFrame, PixelFrame>>(static held => frame => Metric(frame, held)), Kept: Some<Func<PixelFrame, PixelFrame>>(static frame => frame)) switch {
            var forms => channels
                .Traverse(channel => channel.Map(depth: forms.Depth, normal: forms.Kept, albedo: forms.Kept, objectId: forms.Kept, materialId: forms.Kept)
                    .Traverse(form => Read(open, extent, Channel(channel), rect).Map(frame => (Channel: channel, Frame: form(frame))))
                    .As())
                .As()
                .Map(static reads => toHashMap(reads.Somes())),
        };

    public static IO<Unit> Write(PostEffectPipeline pipeline, PixelFrame frame) =>
        Opened(pipeline.GetChannelForWrite, RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), (opened, cpu) =>
            from written in Put(cpu, frame)
            from committed in IO.lift(opened.Commit)
            select committed);

    public static Fin<PixelExtent> Extent(Size size) =>
        PixelExtent.Validate(size.Width, size.Height, out PixelExtent extent) is { } error ? error : extent;

    internal static IO<Unit> Put(RenderWindow.Channel cpu, PixelFrame frame) =>
        IO.lift(() => cpu.SetValues(frame.Window, frame.Window.Size, new PixelBuffer(frame.Address)));

    internal static IO<T> Opened<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        (from opened in use(IO.lift(() => Missing.Unless(open(channel), open.Method.Name)))
         from cpu in IO.lift(() => Missing.Unless(opened.CPU(), nameof(PostEffectChannel.CPU)))
         from answer in body(opened, cpu)
         select answer).Bracket();

    private static Rectangle Whole(PixelExtent extent) => new(0, 0, extent.Width, extent.Height);

    private static PixelFrame Metric(PixelFrame frame, float metres) =>
        new(frame.Origin, frame.Size, frame.Extent, block => {
            const float background = 1e10f;
            ReadOnlySpan<Vector4> depths = MemoryMarshal.Cast<float, Vector4>(frame.Block);
            Span<Vector4> metric = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int index = 0; index < depths.Length; index++)
                metric[index] = new Vector4(new Vector3(depths[index].X == background ? float.PositiveInfinity : depths[index].X * metres), depths[index].W);
        });

    // --- [GAMMA]
    public static IO<Option<GammaExponent>> OutputGamma(PostEffectPipeline pipeline) =>
        IO.lift(() => Optional(((IPostEffects)pipeline).PostEffectFromId(BuiltinParameter.Gamma.Effect.Id))
            .Filter(gamma => gamma.CanExecute(pipeline))
            .Traverse(static gamma => {
                object? value = null;
                return Refused.Unless(gamma.GetParam(BuiltinParameter.Gamma.Key, ref value), nameof(PostEffect.GetParam))
                    .Bind(Fin<GammaExponent> (Unit _) => value is float exponent
                        ? Conversions.Validated<GammaExponent, float, InvalidColor>(exponent)
                        : new InvalidAnswer(nameof(PostEffect.GetParam)));
            })
            .As());

    private static PixelPass Compensated(GammaExponent exponent) =>
        new Transfer.Gamma(exponent) switch {
            var gamma => new PixelPass.Color(row => gamma.Decode(row, Nits.ReferenceWhite)),
        };

    // --- [RUNS]
    public static IO<Unit> Run<TState>(
        PostEffectPipeline pipeline, Seq<EffectKind> kinds, Rectangle rect, PostEffectType stage, EffectValue<TState> value, Atom<StageFrame> input, Derivations derivations)
        where TState : IPixelStage<TState> =>
        from context in Context(pipeline, kinds, stage, value.Record, derivations)
        let whole = Whole(context.Extent)
        let pass = Staged(value, context)
        let area = pass.Map(held => held.Map(color: rect, pointwise: rect, frame: whole)).IfNone(whole)
        let completed = area == whole && !pipeline.IsRendering
        let tapped = completed && stage != PostEffectType.Early
        let demanded = completed && input.Value.Demanded
        from gamma in stage == PostEffectType.ToneMapping && pass.IsSome ? OutputGamma(pipeline) : IO.pure(Option<GammaExponent>.None)
        let passes = pass.ToSeq() + gamma.Map(Compensated).ToSeq()
        from _ in when(pass.IsSome || tapped || demanded,
            from frame in Read(pipeline.GetChannelForRead, context.Extent, RenderWindow.StandardChannels.RGBA, area)
            let copy = tapped || demanded ? Some(passes.IsEmpty ? frame : Copied(frame)) : None
            from __ in IO.lift(() => copy.Filter(_ => tapped).Iter(held => RenderRuns.Fill(pipeline.RenderingId, run => run.Tapped(stage, held, pass))))
            from ___ in IO.lift(() => copy.Filter(_ => demanded).Iter(held => StageFrame.Fill(input, held)))
            from ____ in passes.TraverseM(held => IO.lift(() => held.Run(frame, pipeline))).As()
            from _____ in when(pass.IsSome, Write(pipeline, frame)).As()
            select unit).As()
        select unit;

    public static IO<bool> Runs<TState>(
        PostEffectPipeline pipeline, Seq<EffectKind> kinds, PostEffectType stage, EffectValue<TState> value, Atom<StageFrame> input, Derivations derivations)
        where TState : IPixelStage<TState> =>
        Context(pipeline, kinds, stage, value.Record, derivations).Map(context =>
            Staged(value, context).IsSome || (!pipeline.IsRendering && (stage != PostEffectType.Early || input.Value.Demanded)));

    public static Option<PixelPass> Staged<TState>(EffectValue<TState> value, PassContext context) where TState : IPixelStage<TState> =>
        TState.Pass(value.Record, context).Map(pass => value.Amount.Map(mix => Blend.Mixed(pass, BlendingMode.Mix, mix, [], context.Extent)).IfNone(pass));

    private static IO<PassContext> Context<TState>(PostEffectPipeline pipeline, Seq<EffectKind> kinds, PostEffectType stage, TState state, Derivations derivations)
        where TState : IPixelStage<TState> =>
        from extent in IO.lift(() => Extent(pipeline.Dimensions()))
        from formation in state is ToneMapping formed ? IO.pure(Some(formed)) : Held<ToneMapping>(pipeline, kinds, effect => effect.CanExecute(pipeline))
        from exposure in state is ExposureState exposed ? IO.pure(Some(exposed)) : Held<ExposureState>(pipeline, kinds, static effect => effect.On && effect.Shown)
        from run in IO.lift(() => RenderRuns.Fill(pipeline.RenderingId, held => stage == PostEffectType.ToneMapping ? held with { Formation = formation } : held))
        from signal in stage == PostEffectType.Late ? Signal(pipeline, formation) : IO.pure<Transfer>(TransferCurve.Linear)
        from guides in Guides(pipeline.GetChannelForRead, extent, Whole(extent), TState.Channels(state), run.Metres)
        select new PassContext(
            Extent: extent, Working: Working, Display: Display, Depth: Depth, Signal: signal,
            Exposure: exposure.Map(static held => held.Exposure).IfNone(Exposure.Neutral),
            Camera: run.Camera, Guides: guides, Time: run.Time, Lights: run.Lights, Derivations: derivations);

    private static IO<Option<TRecord>> Held<TRecord>(PostEffectPipeline pipeline, Seq<EffectKind> kinds, Func<PostEffect, bool> runs) where TRecord : IStateRecord<TRecord> =>
        IO.lift(() => kinds.Find(static kind => kind.State == typeof(TRecord))
                .Bind(kind => toSeq(((IPostEffects)pipeline).GetPostEffects(kind.Stage)).Find(effect => effect.Id == kind.Id && runs(effect))))
            .Bind(static found => found
                .Traverse(static effect => FieldTexts.Recalled<TRecord>(EntryKey.TypeOwner(effect.Id), path => Parameter(effect, EntryKey.Name(path))))
                .As());

    private static IO<Transfer> Signal(PostEffectPipeline pipeline, Option<ToneMapping> formation) =>
        formation.IsSome
            ? IO.pure(Display.Encoding.Transfer)
            : OutputGamma(pipeline).Map(static gamma => gamma.Map<Transfer>(static exponent => new Transfer.Gamma(exponent)).IfNone(TransferCurve.Linear));

    private static Option<string> Parameter(PostEffect effect, string key) {
        object? value = null;
        return effect.GetParam(key, ref value) && value is string text ? Some(text) : None;
    }

    private static PixelFrame Copied(PixelFrame frame) => new(frame.Origin, frame.Size, frame.Extent, block => frame.Block.CopyTo(block));

    // --- [CHAINS]
    public static Seq<EffectKind> Ordered(Seq<EffectEntry> entries, Seq<EffectKind> kinds) =>
        entries.Filter(static entry => entry.Runs).Choose(entry => kinds.Find(kind => kind.Id == entry.Id));

    public static IO<Seq<(PostEffectType Stage, PixelPass Pass)>> Chain(ValueSet set, Seq<EffectKind> chain, PassContext context) =>
        (context with { Signal = chain.Exists(static kind => kind.Stage == PostEffectType.ToneMapping) ? Display.Encoding.Transfer : context.Signal }) switch {
            var late => chain.TraverseM(kind => kind.Pass(set, kind.Stage == PostEffectType.Late ? late : context).Map(pass => pass.Map(held => (kind.Stage, Pass: held))))
                .As()
                .Map(static passes => passes.Somes()),
        };

    public static IO<PassContext> Detached(PixelExtent extent, ValueSet set, Seq<EffectKind> chain, Derivations derivations) =>
        chain.Find(static kind => kind.State == typeof(ExposureState))
            .Traverse(kind => EffectStates.Recalled<ExposureState>(EntryKey.TypeOwner(kind.Id), set, None))
            .As()
            .Map(exposure => new PassContext(
                Extent: extent, Working: Working, Display: Display, Depth: Depth, Signal: TransferCurve.Linear,
                Exposure: exposure.Map(static held => held.Record.Exposure).IfNone(Exposure.Neutral),
                Camera: None, Guides: HashMap<GuideChannel, PixelFrame>(), Time: None, Lights: SceneLights.Empty, Derivations: derivations));

    public static IO<PixelFrame> Form(PixelFrame frame, ValueSet set, Seq<EffectKind> chain, Derivations derivations) =>
        from context in Detached(frame.Extent, set, chain, derivations)
        from passes in Chain(set, chain, context)
        let formed = passes.IsEmpty ? frame : Copied(frame)
        from _ in passes.TraverseM(stage => IO.lift(() => stage.Pass.Run(formed, new Progress<int>()))).As()
        select formed;

    public static Seq<PixelPass.Color> Baked(Seq<(PostEffectType Stage, PixelPass Pass)> chain) =>
        chain.Exists(static stage => stage.Stage == PostEffectType.ToneMapping) switch {
            var formed => chain.Filter(stage => formed || stage.Stage == PostEffectType.Early)
                .Choose(static stage => stage.Pass.Switch<Option<PixelPass.Color>>(color: static color => color, pointwise: static _ => None, frame: static _ => None)),
        };
}
