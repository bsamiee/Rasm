using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Render.PostEffects;
using UUIDNext;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderOutput(Fin<(PixelFrame Frame, ColorEncoding Encoding)> Frame, Option<int> Prefix) {
    internal static RenderOutput Tapped(Option<RenderOutput> previous, Fin<PixelFrame> input, Transfer signal, PixelFrame formed, int prefix, bool dither) =>
        previous.Filter(static held => held.Prefix.IsSome).IfNone(() => new RenderOutput(
            dither ? input.Map(frame => (frame, new ColorEncoding(EffectPipeline.Working, signal, Nits.ReferenceWhite)))
                : input.Map(_ => (formed, EffectPipeline.Display.Encoding)),
            Callbacks.Found(dither, prefix)));
}

public sealed record RenderRun(
    Option<Camera> Camera, Option<TimeSpan> Time, Option<SceneLights> Lights, Option<float> Metres, Option<LensDistance> FocalLength, Option<PositiveMagnitude> Aperture,
    Option<Func<PassContext, Option<PixelPass>>> Formation, Option<PixelFrame> Scene, Seq<Func<PassContext, Option<PixelPass>>> Late, Seq<Guid> NativeLate, Option<RenderOutput> Output, Option<PixelFrame> Formed) {
    public static RenderRun Empty { get; } = new(None, None, None, None, None, None, None, None, [], [], None, None);

    public Option<HashMap<RenderWindow.StandardChannels, PixelFrame>> Channels { get; init; }

    public RenderRun Tapped(PostEffectType stage, PixelFrame input, Transfer signal, PixelFrame formed, Option<Func<PassContext, Option<PixelPass>>> pass, bool dither, Seq<Guid> native, Fin<PixelFrame> output) =>
        stage switch {
            PostEffectType.ToneMapping => this with { Scene = Some(input), Late = [], NativeLate = [], Output = None, Formed = None },
            PostEffectType.Late => this with {
                Formed = Some(formed),
                Late = Late + pass.ToSeq(),
                NativeLate = native,
                Output = Some(RenderOutput.Tapped(Output, output, signal, formed, Late.Count, dither)),
            },
            PostEffectType.Early => this,
        };
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class RenderRuns {
    // --- [STATE]
    private static readonly Atom<Seq<(Guid Rendering, RenderRun Run)>> runs = Atom(Seq<(Guid Rendering, RenderRun Run)>());

    private static readonly Guid captureChannel = Guid.NewGuid();

    internal static RenderRun Fill(Guid rendering, Func<RenderRun, RenderRun> change) =>
        runs.Swap(entries =>
            (Rendering: rendering, Run: change(entries.Find(entry => entry.Rendering == rendering).Map(static entry => entry.Run).IfNone(RenderRun.Empty)))
                .Cons(entries.Filter(entry => entry.Rendering != rendering).Take(SupportOptions.AutoSaveKeepAmount() - 1)))[0].Run;

    internal static IO<Unit> Started(PostEffectPipeline pipeline) =>
        DisposalOps.Within(() => Optional(pipeline.GetChannelForRead(captureChannel)),
            found => found.Match(
                Some: static _ => IO.pure(unit),
                None: () =>
                    from reset in IO.lift(() => Fill(pipeline.RenderingId, static run => run with {
                        Formation = None,
                        Scene = None,
                        Channels = None,
                        Late = [],
                        NativeLate = [],
                        Output = None,
                        Formed = None,
                    }))
                    from committed in DisposalOps.Found(() => pipeline.GetChannelForWrite(captureChannel), nameof(PostEffectPipeline.GetChannelForWrite), static channel => IO.lift(channel.Commit))
                    select unit),
            static found => DisposalOps.Release(found.ToSeq()));

    // --- [READS]
    public static Option<RenderRun> Find(Guid rendering) => runs.Value.Find(entry => entry.Rendering == rendering).Map(static entry => entry.Run);

    internal static Error NativeOutput(Guid rendering, Seq<Guid> effects) =>
        Error.New($"Rendering {rendering} cannot reconstruct the requested output around native effects {string.Join(", ", effects)}");

    public static Option<(Guid Rendering, RenderRun Run)> Rendered =>
        runs.Value.Head.Filter(static latest => latest.Run.Formed.IsSome);

    public static FrameSide Formed { get; } = new(
        new FrameFeed(EffectPipeline.Display.Encoding.Transfer, Some(EffectPipeline.Display.Peak), Eto.Drawing.ImageInterpolation.High),
        RowText.Localize("After").Local);

    // --- [FRAMING]
    private const float ApertureMinimum = 0.00001f;

    public static IO<RenderRun> Framing(RhinoDoc doc, ViewInfo view, PixelExtent extent, Option<TimeSpan> time, Func<Error, IO<Unit>> report) =>
        from optics in IO.lift(() => (view.Viewport.IsParallelProjection, (float)view.FocalBlurAperture) switch {
            var (parallel, aperture) => (
                    parallel ? Fin.Succ(Option<LensDistance>.None)
                        : LensDistance.Validated((float)(view.Viewport.Camera35mmLensLength * LengthUnit.Scale(LengthUnit.Millimeters, LengthUnit.Meters))).Map(Some),
                    parallel || view.FocalBlurMode is not ViewInfoFocalBlurModes.Manual || aperture < ApertureMinimum
                        ? Fin.Succ(Option<PositiveMagnitude>.None)
                        : PositiveMagnitude.Validated(aperture).Map(Some))
                .Apply(static (focal, number) => (Focal: focal, Aperture: number)).As(),
        })
        from lens in IO.lift(() => Cameras.Focus(view, doc.ModelUnits, optics.Focal, optics.Aperture))
            .Catch(static error => error.HasCode(ErrorCodes.ValidationFailed), error => report(error).Map(Option<LensFocus>.None))
        from camera in IO.lift(() => Cameras.ToCamera(view, doc.ModelUnits, extent, lens))
        from lights in Cameras.ToLights(doc)
        select RenderRun.Empty with { Camera = camera, Time = time, Lights = Some(lights), Metres = (float)LengthUnit.Scale(doc.ModelUnits, LengthUnit.Meters), FocalLength = optics.Focal, Aperture = optics.Aperture };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectPipeline {
    // --- [OUTPUT]
    public static readonly Gamut Working = Gamut.StandardRgb;
    public static readonly Imaging.Tone.Display Display = Imaging.Tone.Display.Srgb;
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
        PixelExtent.Validated(size.Width, size.Height);

    internal static IO<Unit> Put(RenderWindow.Channel cpu, PixelFrame frame) =>
        IO.lift(() => {
            Rectangle rect = new(frame.Origin, new Size(frame.Size.Width, frame.Size.Height));
            cpu.SetValues(rect, rect.Size, new PixelBuffer(frame.Address));
        });

    internal static IO<Option<T>> Probed<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        DisposalOps.Within(() => Optional(open(channel)),
            found => found.Traverse(opened => IO.lift(() => Refused.UnlessFound(opened.CPU(), nameof(PostEffectChannel.CPU))).Bind(cpu => body(opened, cpu))).As(),
            static found => DisposalOps.Release(found.ToSeq()));

    internal static IO<T> Opened<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        Probed(open, channel, body).Bind(found => IO.lift(found.ToFin(new Refused(open.Method.Name, Refused.Absent))));

    private static Rectangle Whole(PixelExtent extent) => new(0, 0, extent.Width, extent.Height);

    internal static PixelFrame Metric(PixelFrame frame, float metres) =>
        new(frame.Origin, frame.Size, frame.Extent, block => {
            const float background = 1e10f;
            ReadOnlySpan<Vector4> depths = MemoryMarshal.Cast<float, Vector4>(frame.Block);
            Span<Vector4> metric = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int index = 0; index < depths.Length; index++)
                metric[index] = new Vector4(new Vector3(depths[index].X == background ? float.PositiveInfinity : depths[index].X * metres), depths[index].W);
        });

    // --- [GAMMA]
    private const string GammaKey = "gamma";

    public static IO<Option<PositiveMagnitude>> OutputGamma(PostEffectPipeline pipeline) =>
        IO.lift(() => Optional(((IPostEffects)pipeline).PostEffectFromId(PostEffectUuids.Gamma))
            .Filter(gamma => gamma.CanExecute(pipeline))
            .Traverse(static gamma => {
                object? value = null;
                return Refused.Unless(gamma.GetParam(GammaKey, ref value), nameof(PostEffect.GetParam))
                    .Bind(Fin<PositiveMagnitude> (Unit _) => value is float exponent
                        ? PositiveMagnitude.Validated(exponent)
                        : new Refused(nameof(PostEffect.GetParam), Refused.Invalid));
            })
            .As());

    private static PixelPass.Color Compensated(PositiveMagnitude exponent, bool encode = false) =>
        new Transfer.Gamma(exponent) switch {
            var gamma => new PixelPass.Color(encode ? row => gamma.Encode(row, Nits.ReferenceWhite) : row => gamma.Decode(row, Nits.ReferenceWhite)),
        };

    // --- [EXPOSURE]
    private static (Guid Scale, Guid Amount) ExposureChannels(Guid effect) =>
        (Uuid.NewNameBased(effect, nameof(Exposure.Scale)), Uuid.NewNameBased(effect, nameof(EffectValue<>.Amount)));

    private static float Scalar(RenderWindow.Channel channel) {
        float[] value = [0f];
        channel.GetValue(0, 0, ComponentOrders.Irrelevant, ref value);
        return value[0];
    }

    private static IO<double> Applied(PostEffectPipeline pipeline) =>
        toSeq(pipeline.ExecutionOrder()).TraverseM(effect => Exposed(pipeline, ExposureChannels(effect))).As().Map(static values => values.Fold(0d, static (held, next) => held + next));

    private static IO<double> Exposed(PostEffectPipeline pipeline, (Guid Scale, Guid Amount) identifiers) =>
        Probed(pipeline.GetChannelForRead, identifiers.Scale, (_, cpu) =>
            from first in IO.lift(() => Scalar(cpu))
            from amount in Opened(pipeline.GetChannelForRead, identifiers.Amount, static (_, second) => IO.lift(() => Scalar(second)))
            select Exposure.Applied(first, amount)).Map(static value => value.IfNone(0d));

    // --- [RUNS]
    public static IO<Unit> Run<TState>(
        PostEffectPipeline pipeline, Guid id, Seq<EffectKind> kinds, Rectangle rect, PostEffectType stage, EffectValue<TState> value, Atom<StageFrame> input, Derivations derivations)
        where TState : IPixelStage<TState> =>
        from started in RenderRuns.Started(pipeline)
        from prepared in Context(pipeline, stage, value, derivations)
        let context = prepared.Pass
        let whole = Whole(context.Extent)
        let pass = Staged(value, context)
        let area = pass.Map(held => held.Map(color: rect, pointwise: rect, frame: whole)).IfNone(whole)
        let completed = rect == whole && !pipeline.IsRendering
        let tapped = completed && stage is not PostEffectType.Early
        let demanded = completed && input.Value.Demanded
        let order = prepared.Order
        let after = toSeq(order.SkipWhile(effect => effect.Id != id)).Skip(1)
        let gamma = stage is PostEffectType.ToneMapping && pass.IsNone ? None : prepared.Gamma
        let normalized = gamma.Filter(_ => stage is PostEffectType.Late && after.Exists(static effect => effect.Id == PostEffectUuids.Gamma))
        let passes = pass.ToSeq() + (stage is PostEffectType.ToneMapping ? gamma.Map<PixelPass>(static held => Compensated(held)).ToSeq() : [])
        let unknown = order.Filter(effect => effect.Id != PostEffectUuids.Gamma && effect.Id != PostEffectUuids.Dithering
            && !kinds.Exists(kind => kind.Id == effect.Id && kind.Pass.IsSome) && effect.CanExecute(pipeline)).Map(static effect => effect.Id)
        let native = order.Filter(effect => effect.PostEffectType is PostEffectType.Late && unknown.Exists(identifier => identifier == effect.Id)).Map(static effect => effect.Id)
        let unavailable = after.Filter(effect => native.Exists(identifier => identifier == effect.Id)).Map(static effect => effect.Id)
            + toSeq(order.TakeWhile(effect => effect.Id != id)).Filter(effect => effect.Id == PostEffectUuids.Dithering && effect.CanExecute(pipeline)).Map(static effect => effect.Id)
        let replay = value.Record is Dither ? None : pass.Map(_ => (Func<PassContext, Option<PixelPass>>)(target =>
            Staged(value, context with {
                Display = target.Display,
                Depth = target.Depth,
                Signal = stage is PostEffectType.Late ? target.Display.Encoding.Transfer : context.Signal,
                Derivations = target.Derivations,
            })))
        let exposure = value.Record is ExposureState exposed && pass.IsSome
            ? Some((exposed.Exposure.Scale, Amount: (float)value.Amount.IfNone(AxisFraction.MaxValue))) : None
        let display = stage is PostEffectType.Late
            ? new ColorEncoding(Working, context.Signal, Nits.ReferenceWhite).To(Display.Encoding).Map<PixelPass>(static step => new PixelPass.Color(step))
            : Seq<PixelPass>()
        from _ in when(pass.IsSome || tapped || demanded,
            DisposalOps.Within<PostEffectChannel, Unit>(retain =>
            from channels in Writable(pipeline, id, exposure, retain)
            from stored in IO.lift(() => channels.Iter(static held => held.Cpu.SetValue(0, 0, held.Value)))
            from frame in Read(pipeline.GetChannelForRead, context.Extent, RenderWindow.StandardChannels.RGBA, area)
            from ___ in normalized.Traverse(held => IO.lift(() => Compensated(held, encode: true).Run(frame, pipeline))).As()
            let copy = tapped || demanded ? Some(passes.IsEmpty && normalized.IsNone ? frame : frame.Copy()) : None
            from ____ in passes.TraverseM(held => IO.lift(() => held.Run(frame, pipeline))).As()
            let formed = tapped ? passes.IsEmpty && display.IsEmpty ? copy : Some(normalized.IsSome || !display.IsEmpty ? frame.Copy() : frame) : None
            from ____b in formed.Traverse(observed => display.TraverseM(held => IO.lift(() => held.Run(observed, pipeline))).As()).As()
            from ____a in normalized.Traverse(held => IO.lift(() => Compensated(held).Run(frame, pipeline))).As()
            from _____ in when(pass.IsSome, Write(pipeline, frame)).As()
            from committed in IO.lift(() => channels.Iter(static held => held.Channel.Commit()))
            from auxiliary in tapped && prepared.Run.Channels.IsNone
                ? Auxiliary(pipeline, whole, context.Extent)
                : IO.pure(Option<HashMap<RenderWindow.StandardChannels, PixelFrame>>.None)
            from ______ in IO.lift(() => RenderRuns.Fill(pipeline.RenderingId, run =>
                (copy.Filter(_ => tapped), formed).Apply((snapshot, observed) => run.Tapped(stage, snapshot, context.Signal, observed,
                    replay, value.Record is Dither && pass.IsSome, native,
                    unavailable.IsEmpty ? Fin.Succ(snapshot) : RenderRuns.NativeOutput(pipeline.RenderingId, unavailable))).As().IfNone(run) with {
                    Formation = stage is PostEffectType.ToneMapping ? replay.Filter(_ => value.Record is ToneMapping) : run.Formation,
                    Channels = run.Channels | auxiliary,
                }))
            from _______ in IO.lift(() => copy.Filter(_ => demanded).Iter(held => StageFrame.Fill(input, held, context)))
            select unit, DisposalOps.Release)).As()
        select unit;

    public static IO<bool> Runs<TState>(
        PostEffectPipeline pipeline, PostEffectType stage, EffectValue<TState> value, Atom<StageFrame> input, Derivations derivations)
        where TState : IPixelStage<TState> =>
        Context(pipeline, stage, value, derivations).Map(held =>
            Staged(value, held.Pass).IsSome || (!pipeline.IsRendering && (stage is not PostEffectType.Early || input.Value.Demanded)));

    public static Option<PixelPass> Staged<TState>(EffectValue<TState> value, PassContext context) where TState : IPixelStage<TState> =>
        value.Amount.Contains(AxisFraction.MinValue)
            ? None
            : TState.Pass(value.Record, context).Bind(pass => Blend.Mixed(pass, BlendingMode.Mix, value.Amount.IfNone(AxisFraction.MaxValue), []));

    private static IO<(PassContext Pass, Seq<PostEffect> Order, Option<PositiveMagnitude> Gamma, RenderRun Run)> Context<TState>(PostEffectPipeline pipeline, PostEffectType stage, EffectValue<TState> value, Derivations derivations)
        where TState : IPixelStage<TState> =>
        from extent in IO.lift(() => Extent(pipeline.Dimensions()))
        from run in IO.lift(() => RenderRuns.Find(pipeline.RenderingId).IfNone(RenderRun.Empty))
        from order in toSeq(pipeline.ExecutionOrder()).TraverseM(id => IO.lift(() =>
            Refused.UnlessFound(((IPostEffects)pipeline).PostEffectFromId(id), nameof(IPostEffects.PostEffectFromId)))).As()
        from gamma in stage is PostEffectType.Early ? IO.pure(Option<PositiveMagnitude>.None) : OutputGamma(pipeline)
        from exposure in Applied(pipeline)
        let signal = stage is not PostEffectType.Late ? TransferCurve.Linear : run.Formation.IsSome
            ? Display.Encoding.Transfer : gamma.Map<Transfer>(static exponent => new Transfer.Gamma(exponent)).IfNone(TransferCurve.Linear)
        from guides in Guides(pipeline.GetChannelForRead, extent, Whole(extent), TState.Channels(value.Record), run.Metres)
        select (new PassContext(
            Extent: extent, Working: Working, Display: Display, Depth: Depth, Signal: signal,
            Exposure: exposure,
            Camera: run.Camera, Guides: guides, Time: run.Time, Lights: run.Lights, Derivations: derivations), order, gamma, run);

    private static IO<Seq<(PostEffectChannel Channel, RenderWindow.Channel Cpu, float Value)>> Writable(
        PostEffectPipeline pipeline, Guid id, Option<(float Scale, float Amount)> exposure, Action<PostEffectChannel> retain) =>
        exposure.Map(held => ExposureChannels(id) switch {
            var identifiers => Seq((Id: identifiers.Scale, Value: held.Scale), (Id: identifiers.Amount, Value: held.Amount)),
        }).IfNone([]).TraverseM(held =>
            from channel in IO.lift(() => {
                PostEffectChannel acquired = Refused.UnlessFound(pipeline.GetChannelForWrite(held.Id), nameof(PostEffectPipeline.GetChannelForWrite)).ThrowIfFail();
                retain(acquired);
                return acquired;
            })
            from cpu in IO.lift(() => Refused.UnlessFound(channel.CPU(), nameof(PostEffectChannel.CPU)))
            select (Channel: channel, Cpu: cpu, held.Value)).As();

    private static IO<Option<HashMap<RenderWindow.StandardChannels, PixelFrame>>> Auxiliary(PostEffectPipeline pipeline, Rectangle whole, PixelExtent extent) =>
        toSeq(SavedChannel.Items).TraverseM(channel =>
            Probed(pipeline.GetChannelForRead, RenderWindow.ChannelId(channel.Key), (_, cpu) => IO.lift(() => RenderWindows.Frame(cpu, whole, extent).Map(pixels => (channel.Key, Pixels: pixels)))))
            .As().Map(static frames => Some(toHashMap(frames.Somes())));

    // --- [CHAINS]
    public static Seq<EffectKind> Ordered(Seq<EffectEntry> entries, Seq<EffectKind> kinds) =>
        entries.Filter(static entry => entry.Runs).Choose(entry => kinds.Find(kind => kind.Id == entry.Id));

    public static IO<Seq<(PostEffectType Stage, PixelPass Pass)>> Chain(ValueSet set, Seq<EffectKind> chain, PassContext context) =>
        chain.Choose(static kind => kind.Pass.Map(pass => (kind.Stage, Pass: pass)))
            .Fold(IO.pure((Context: context, Passes: Seq<(PostEffectType Stage, PixelPass Pass)>())), (previous, kind) =>
            from held in previous
            from staged in kind.Pass(set, held.Context)
            select (staged.Context, Passes: held.Passes + staged.Pass.Map(pass => (kind.Stage, Pass: pass)).ToSeq()))
            .Map(static held => held.Passes);

    public static PassContext Detached(PixelExtent extent, Derivations derivations) =>
        new(
                Extent: extent, Working: Working, Display: Display, Depth: Depth, Signal: TransferCurve.Linear,
                Exposure: 0d,
                Camera: None, Guides: HashMap<GuideChannel, PixelFrame>(), Time: None, Lights: None, Derivations: derivations);

    public static IO<PixelFrame> Form(PixelFrame frame, ValueSet set, Seq<EffectKind> chain, Derivations derivations) =>
        from passes in Chain(set, chain, Detached(frame.Extent, derivations))
        let formed = passes.IsEmpty ? frame : frame.Copy()
        from _ in passes.TraverseM(stage => IO.lift(() => stage.Pass.Run(formed, new Progress<int>()))).As()
        select formed;

    public static Seq<PixelPass.Color> Baked(Seq<(PostEffectType Stage, PixelPass Pass)> chain) =>
        chain.Exists(static stage => stage.Stage is PostEffectType.ToneMapping) switch {
            var formed => chain.Filter(stage => formed || stage.Stage is PostEffectType.Early)
                .Choose(static stage => stage.Pass.Switch<Option<PixelPass.Color>>(color: static color => color, pointwise: static _ => None, frame: static _ => None)),
        };
}
