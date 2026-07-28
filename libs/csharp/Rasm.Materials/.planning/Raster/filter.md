# [MATERIALS_FILTER]

THE DECODED-PLANE TRANSFORM ALGEBRA. One `PlaneOp` `[Union]` closes the per-plane transform family — resample, convolve, height correspondence, height derivative, tonal remap, and lane swizzle — under ONE `Apply` entry that PLANS every shape before it rents an output, SCHEDULES the sequence into stages by each op's own dependency class, and folds each stage over the plane's decoded row rails. `PlaneOp` holds a transform as a case, `ConvolveKernel` a convolution as a row, `RemapCurve` a tonal curve as a case, `HeightDerivative` a height derivative as a case, and `SwizzleLane` a lane projection as a row — never a per-transform entrypoint, a per-curve method, or a boolean selecting between two bodies.

Scheduling is the page's load-bearing decision, and it exists because the ops genuinely differ in what they can see. `Levels` remaps one texel; a Gaussian reads a neighbourhood; a histogram remap must see the WHOLE plane before it can map its first texel; a normal-to-height integration is a global spectral or sparse-linear solve; a resample changes the grid itself. Fusing that mixture into one per-row pass is a fiction — a row action cannot supply a neighbour it has not written, a plane statistic it has not gathered, or a grid it has not resized — so each case declares its `StageKind` and the scheduler fuses only what genuinely fuses: consecutive pointwise ops collapse into ONE row pass, a neighbourhood op takes a bordered two-buffer pass under its own `EdgeMode`, and a global op takes a whole-plane pass. Adding an op therefore cannot corrupt its neighbours, and adding a dependency class is one row on the scheduler. `PlaneOp` composes the `plane#TEXTURE_PLANE` typed arena with its decoded `Read`/`Write` row rails and its `PlaneFormat.For` retyping, the `codec#RASTER_FAULT` band-2460 rail for nothing at all (every refusal here is a SHAPE refusal on band 2450), TinyEXR.NET `ImageProcessing`/`Lut3D` for every separable resample, transfer, and LUT fold, MathNet.Numerics `Fourier` for the spectral integration, the kernel `SparseMatrix`/`CholeskySparse` for the bounded Poisson solve, the kernel `SampleKind` blue-noise spectrum and `Deterministic` splitmix64 stream for the occlusion cast, and `CommunityToolkit.HighPerformance` `ParallelHelper` over `struct IAction` partitions — re-minting no resampler, no transform, no factorization, no sampler spectrum, and no random source.

## [01]-[INDEX]

- [02]-[PLANE_OP]: `StageKind` axes the dependency, `ConvolveKernel`/`RemapCurve`/`HeightDerivative` family the rows, `SwizzleLane` rosters the projections, and the six-case `PlaneOp` union projects every shape totally.
- [03]-[PLANE_STAGE]: `PlaneOp.Apply` plans, schedules, and runs — fusing the pointwise run, bordering the neighbourhood pass, and publishing `PlaneReceipt` evidence.
- [04]-[HEIGHT_FIELD]: `HeightEvidence` carries the correspondence, `HeightSolver` routes the spectral and bounded integrations, and the occlusion and curvature derivatives read the height field.

## [02]-[PLANE_OP]

- Owner: `PlaneOp` the transform family; `StageKind` the dependency axis each case declares; `ConvolveKernel` the neighbourhood-kernel family; `RemapCurve` the tonal-curve family; `HeightDerivative` the height-derived-field family; `SwizzleLane` the lane-projection roster; `PlaneShape` the projected shape carrier.
- Cases: op {`Resize`, `Convolve`, `HeightNormal`, `FromHeight`, `Remap`, `Swizzle`} · stage {`pointwise`, `neighbourhood`, `global`} · kernel {`Gaussian`, `UnsharpMask`} · curve {`Levels`, `Histogram`, `Lut`} · derivative {`Occlusion`, `Curvature`} · lane {`r`, `g`, `b`, `a`, `zero`, `one`, `rInverse`, `gInverse`, `bInverse`, `aInverse`}.
- Law: `HeightNormal` carries BOTH directions of one correspondence on one case. `Inverse` is the column carrying direction, because a height field and a tangent-space normal field are the forward and inverse of a single relation — never a `NormalFromHeight`/`HeightFromNormal` sibling pair. `HeightEvidence` crosses from the forward to the inverse — the millimetre scale, the field mean, and the convention the forward recorded — because integration recovers a gradient field's shape and never its absolute offset or amplitude, and an inverse whose ingress is raw samples re-shaped into the forward's input domain fabricates exactly what the forward destroyed.
- Law: `SwizzleLane` is DATA — a source index, a scale, and a bias — so lane reordering, lane inversion, constant fill, and the `dx`→`gl` green flip are all one kernel over a row of rows. `Swizzle(R, GInverse, B, A)` is exactly the `plane#PLANE_VOCABULARY` `NormalConvention` conversion and mints no second operation, so the corpus has one green-flip site rather than a conversion pair beside a swizzle.
- Law: `Remap` closes the tonal family on one case. `RemapCurve.Levels.Invert` is a ROW of the levels case — black at one, white at zero — so the `roughness = 1 − gloss` ingest inversion, a contrast stretch, and a gamma lift are one curve family rather than an `Invert` op beside a `Levels` op. Every curve evaluates in the LINEAR domain over decoded lanes, which is what makes an `srgb`-authored gloss plane invert correctly rather than forking the roughness silently.
- Law: `Project` is TOTAL and runs before any rental. It folds the whole sequence into a final `PlaneShape`, so a shape refusal anywhere in the chain leaves the source untouched and costs nothing — a mid-chain refusal after three rentals is the failure mode the plan-first order forecloses. Retyping resolves through `PlaneFormat.For`, so a lane-count change lands on the storage row the semantic count rounds up to and never on a fabricated format.
- Law: shape refusals rail `MaterialFault.Parameter` on band 2450. This page reaches band 2460 nowhere: a filter has no container, no device, and no synthesizer, so a `RasterFault` here would be a shape refusal wearing a mechanical code.
- Entry: `PlaneOp.Apply(TexturePlane source, Seq<PlaneOp> ops, Op key, TimeProvider? clock = null)` is the ONE entry over every arity — an empty sequence returns the source with an empty receipt, a single op and a chain take the identical path, and no `ApplyOne`/`ApplyMany` pair exists; the clock rides so the receipt's elapsed is measured, and `press#TEXTURE_PRESS` threads its own.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`Layer`, `PlaneFormat.For`, `PlaneTransfer`, `AlphaMode`, `PlaneRange`, `NormalConvention`), TinyEXR.NET (composed — `ResizeFilter`/`EdgeMode` the resample vocabulary, `Lut3D.TryParseCube`/`Apply` the `.cube` curve), `Rasm.Numerics` (`Dimension`, `UnitInterval`), `Rasm.Domain` (`Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transform is one `PlaneOp` case declaring its `StageKind` plus one `Project` arm and one kernel arm — the scheduler, the receipt, and every consumer are untouched. A new convolution is one `ConvolveKernel` case, a new curve one `RemapCurve` case, a new derived field one `HeightDerivative` case, a new lane projection one `SwizzleLane` row.
- Boundary: this page transforms DECODED planes and decides nothing about what a plane MEANS. Channel semantics, neutrals, packing, and mip law are `set#TEXTURE_CHANNEL`'s; containers are `codec#RASTER_CODEC`'s; the mip chain is `plane#TEXTURE_PYRAMID`'s and `Resize` is deliberately NOT its alias — a level is a halving under a declared policy, so a resize can never produce a level a sampler then trilinearly blends against a different filter's neighbours.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                                // Fin, Option, Seq
using Rasm.Domain;                                // Op
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault — the band-2450 shape rail
using Rasm.Numerics;                              // Dimension, UnitInterval, SampleKind
using Thinktecture;                               // [Union], [SmartEnum<T>]
using TinyEXR.V3;                                 // ResizeFilter, EdgeMode, Lut3D, LutInterpolation
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// WHAT AN OP CAN SEE. The scheduler's whole law is this axis: a pointwise op fuses with its pointwise neighbours, a
// neighbourhood op needs the previous stage MATERIALIZED plus a border, and a global op needs the whole plane before
// it writes its first texel. Radius is the halo a neighbourhood pass reserves.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StageKind {
    public static readonly StageKind Pointwise = new("pointwise", fuses: true);
    public static readonly StageKind Neighbourhood = new("neighbourhood", fuses: false);
    public static readonly StageKind Global = new("global", fuses: false);

    public bool Fuses { get; }
    private StageKind(string key, bool fuses) : this(key) => Fuses = fuses;
}

// The neighbourhood kernels. Both are separable Gaussians — an unsharp mask is the source minus its own blur, scaled
// and thresholded — so ONE weight generator serves both and a sharpen is a case rather than a second convolution.
[Union]
public abstract partial record ConvolveKernel {
    private ConvolveKernel() { }

    public sealed record Gaussian(double Sigma) : ConvolveKernel;
    public sealed record UnsharpMask(double Sigma, double Amount, double Threshold) : ConvolveKernel;

    public double Sigma => Switch(gaussian: static k => k.Sigma, unsharpMask: static k => k.Sigma);
    // Three standard deviations carry over 99.7% of a Gaussian's mass, so the halo truncates there rather than at a
    // caller-supplied radius that silently clips the tail into a visible ring at high sigma.
    public int Radius => Math.Max(1, (int)Math.Ceiling(3.0 * Sigma));
}

// The tonal curves. Levels is affine-plus-gamma, Histogram matches an empirical CDF, Lut applies a parsed .cube —
// three curves, one case each, all evaluated in the LINEAR domain over decoded lanes.
[Union]
public abstract partial record RemapCurve {
    private RemapCurve() { }

    public sealed record Levels(double Black, double White, double Gamma) : RemapCurve {
        // Black above White is the INVERSION, so `roughness = 1 - gloss` is a row of this case rather than an Invert
        // op beside it — and it inverts after the decode, which is what keeps an srgb-authored gloss plane honest.
        public static readonly Levels Invert = new(Black: 1.0, White: 0.0, Gamma: 1.0);
        public static readonly Levels Identity = new(Black: 0.0, White: 1.0, Gamma: 1.0);
    }

    public sealed record Histogram(Seq<double> TargetCdf, int Bins) : RemapCurve;
    public sealed record Lut(Lut3D Table, LutInterpolation Interpolation) : RemapCurve;

    // Only the histogram match needs the whole plane before it maps a texel; the other two are per-texel functions.
    public StageKind Stage => Switch(
        levels:    static _ => StageKind.Pointwise,
        histogram: static _ => StageKind.Global,
        lut:       static _ => StageKind.Pointwise);
}

// The fields a height plane derives. Occlusion carries its own cast policy with compile-time defaults so a channel
// row spells `new HeightDerivative.Occlusion()` and takes the estate's cast rather than restating four numbers.
[Union]
public abstract partial record HeightDerivative {
    private HeightDerivative() { }

    public sealed record Occlusion(int Rays = 64, double Distance = 0.05, ulong Seed = 0UL, Option<SampleKind> Spectrum = default) : HeightDerivative;
    public sealed record Curvature(CurvatureMeasure Measure = CurvatureMeasure.Mean) : HeightDerivative;
}

public enum CurvatureMeasure { Mean, Gaussian, PrincipalMaximum, PrincipalMinimum }

// The lane projection, as DATA. Source is the input lane or -1 for a constant; Scale and Bias carry the inversion, so
// lane reorder, lane inversion, constant fill, and the dx->gl green flip are one kernel over one row table.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SwizzleLane {
    public static readonly SwizzleLane R = new("r", source: 0, scale: 1.0, bias: 0.0);
    public static readonly SwizzleLane G = new("g", source: 1, scale: 1.0, bias: 0.0);
    public static readonly SwizzleLane B = new("b", source: 2, scale: 1.0, bias: 0.0);
    public static readonly SwizzleLane A = new("a", source: 3, scale: 1.0, bias: 0.0);
    public static readonly SwizzleLane Zero = new("zero", source: -1, scale: 0.0, bias: 0.0);
    public static readonly SwizzleLane One = new("one", source: -1, scale: 0.0, bias: 1.0);
    public static readonly SwizzleLane RInverse = new("rInverse", source: 0, scale: -1.0, bias: 1.0);
    public static readonly SwizzleLane GInverse = new("gInverse", source: 1, scale: -1.0, bias: 1.0);
    public static readonly SwizzleLane BInverse = new("bInverse", source: 2, scale: -1.0, bias: 1.0);
    public static readonly SwizzleLane AInverse = new("aInverse", source: 3, scale: -1.0, bias: 1.0);

    public int Source { get; }
    public double Scale { get; }
    public double Bias { get; }
    public double Project(ReadOnlySpan<double> texel) =>
        (Source >= 0 && Source < texel.Length ? texel[Source] * Scale : 0.0) + Bias;
    private SwizzleLane(string key, int source, double scale, double bias) : this(key) =>
        (Source, Scale, Bias) = (source, scale, bias);

    // The plane#PLANE_VOCABULARY dx->gl conversion, spelled ONCE for the corpus.
    public static Seq<SwizzleLane> FlipGreen => Seq(R, GInverse, B, A);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct PlaneShape(PlaneFormat Format, Dimension Width, Dimension Height, Dimension Layers, PlaneTransfer Transfer, AlphaMode Alpha, PlaneRange Range) {
    public static PlaneShape Of(TexturePlane plane) =>
        new(plane.Format, plane.Width, plane.Height, plane.Layers, plane.Transfer, plane.Alpha, plane.Range);

    // Retyping resolves through the storage roster's own semantic-count rounding, so a three-lane result lands on the
    // four-lane row declaring AlphaMode.None and a format the roster does not carry is a typed absence.
    public Fin<PlaneShape> Retyped(int components, AlphaMode alpha, PlaneRange range, Op key) =>
        PlaneFormat.For(components, Format.Depth)
            .ToFin(MaterialFault.Parameter(key, $"<plane-format:{components}:{Format.Depth.Key}>"))
            .Map(format => this with { Format = format, Alpha = alpha, Range = range });
}

// --- [OPERATIONS] --------------------------------------------------------------------------
[Union]
public abstract partial record PlaneOp {
    private PlaneOp() { }

    public sealed record Resize(Dimension Width, Dimension Height, ResizeFilter Filter, EdgeMode Edge) : PlaneOp;
    public sealed record Convolve(ConvolveKernel Kernel, EdgeMode Edge) : PlaneOp;
    public sealed record HeightNormal(bool Inverse, HeightEvidence Evidence, HeightSolver Solver) : PlaneOp;
    public sealed record FromHeight(HeightDerivative Derivative, HeightEvidence Evidence) : PlaneOp;
    public sealed record Remap(RemapCurve Curve) : PlaneOp;
    public sealed record Swizzle(Seq<SwizzleLane> Lanes) : PlaneOp;

    // The dependency class the scheduler reads. It is a PROJECTION of the case, never a column a caller supplies, so
    // an op cannot declare itself cheaper than it is and get fused into a row pass it cannot survive.
    public StageKind Stage => Switch(
        resize:       static _ => StageKind.Global,
        convolve:     static _ => StageKind.Neighbourhood,
        heightNormal: static op => op.Inverse ? StageKind.Global : StageKind.Neighbourhood,
        fromHeight:   static _ => StageKind.Neighbourhood,
        remap:        static op => op.Curve.Stage,
        swizzle:      static _ => StageKind.Pointwise);

    public int Radius => Switch(
        resize:       static _ => 0,
        convolve:     static op => op.Kernel.Radius,
        heightNormal: static op => op.Inverse ? 0 : 1,
        fromHeight:   static _ => 1,
        remap:        static _ => 0,
        swizzle:      static _ => 0);

    // TOTAL shape projection, folded across the whole sequence before the first rental — so a chain that cannot type
    // costs nothing and leaves the source untouched.
    public Fin<PlaneShape> Project(PlaneShape input, Op key) => Switch(
        resize: op => input.Layers.Value is 1
            ? Fin.Succ(input with { Width = op.Width, Height = op.Height })
            : MaterialFault.Parameter(key, $"<resize-layered:{input.Layers.Value}>"),
        convolve: _ => Fin.Succ(input),
        heightNormal: op => (op.Inverse, input.Format.Components) switch {
            (false, 1) => input.Retyped(3, AlphaMode.None, PlaneRange.Signed, key),
            (true, >= 3) => input.Retyped(1, AlphaMode.None, PlaneRange.Unit, key),
            (false, int n) => MaterialFault.Parameter(key, $"<height-normal-scalar:{n}>"),
            (true, int n) => MaterialFault.Parameter(key, $"<height-normal-vector:{n}>"),
        },
        fromHeight: op => input.Format.Components is 1
            ? input.Retyped(1, AlphaMode.None, op.Derivative is HeightDerivative.Curvature ? PlaneRange.Signed : PlaneRange.Unit, key)
            : MaterialFault.Parameter(key, $"<from-height-scalar:{input.Format.Components}>"),
        remap: _ => Fin.Succ(input),
        swizzle: op => op.Lanes.IsEmpty
            ? MaterialFault.Parameter(key, "<swizzle-lanes-empty>")
            : input.Retyped(op.Lanes.Count, input.Alpha, input.Range, key));
}
```

## [03]-[PLANE_STAGE]

- Owner: `PlaneOp.Apply` the plan-schedule-run entry; `PlaneStage` the scheduled group; `PlaneReceipt` the evidence.
- Entry: `Apply(source, ops, key)` returns the transformed plane paired with its receipt. The source is never mutated and never disposed — the caller owns it, because a chain that consumed its input would make a receipt useless as evidence.
- Law: SCHEDULING is what makes the algebra honest. Consecutive `pointwise` ops fuse into ONE row pass over one intermediate; a `neighbourhood` op takes its own pass against a materialized previous stage under its `EdgeMode` addressing; a `global` op takes a whole-plane pass. A single fused row action across the whole sequence is the deleted form — it cannot supply a neighbour it has not written, a plane statistic it has not gathered, or a grid it has not resized, so the ops whose correctness depends on any of those would silently read the wrong texels.
- Law: fusion is a run-length fold over the sequence, not a special case. A chain of one op schedules identically to a chain of twenty, so the receipt reports the same stage structure at every arity and a benchmark reading it compares like with like.
- Law: each stage rents ONE output and disposes the previous intermediate at the stage boundary, so a twenty-op chain holds at most two planes and the source. The final stage's output is the returned plane; the source is untouched.
- Law: row work rides `ParallelHelper.For<TAction>` over a `struct IAction` whose fields are the two planes, the stage's ops, and the key. The action is `default`-constructed per partition or copied from the `in` seed, so the partition allocates nothing, inlines, and captures nothing — a `Parallel.For` over a closure would allocate one delegate plus one display class per stage and defeat exactly the partition this shape exists for.
- Law: the receipt carries the op KEYS from the union's own case names, never a runtime type name. A reflected type name is stale by construction against a rename and allocates on every op; the case key is the same string the wire and the benchmark row read.
- Law: layer work is per layer inside the row action. A layered plane's rows are one arena band per layer, so a stage walks `height × layers` rows and a resize refuses a layered plane outright rather than resampling across a face boundary that has no spatial meaning.
- Packages: CommunityToolkit.HighPerformance (composed — `ParallelHelper.For<TAction>(int, int)` the row partition, `IAction` the allocation-free slot, `SpanOwner<T>.Allocate` the per-row lane scratch, `MemoryOwner<T>.Allocate` the whole-plane statistic staging), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`, the decoded row rails every kernel reads), TinyEXR.NET (composed — `ImageProcessing.Resize(ReadOnlySpan<float>, int, int, Span<float>, int, int, int, ResizeFilter, EdgeMode, int)` with the extent groups bracketing the two spans and the channel count following BOTH, `Lut3D.Apply(ReadOnlySpan<float>, Span<float>, int, LutInterpolation)`), `Rasm.Domain` (`Op`), LanguageExt.Core.
- Growth: a new dependency class is one `StageKind` row plus one arm in the runner; a new op reaching an existing class adds nothing here at all.
- Boundary: the runner is the page's `[EXPRESSION_SPINE]` kernel exemption — fixed-extent index walks filling caller-owned buffers — while every admission, plan, schedule, and receipt surface is expression-bodied. Statements stop at the row kernel.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Numerics;                            // INumberBase — the staging-scalar constraint
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;   // ParallelHelper, IAction
using TinyEXR.V3;                                 // ImageProcessing — the composed separable resample
using LanguageExt;
using Rasm.Domain;                                // Op
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] ------------------------------------------------------------------------------
// One scheduled group. Ops is a fused pointwise run or exactly one non-fusing op, EACH op paired with the shape it
// ENDS at — a fused run may change lane count mid-chain (a swizzle before a remap), so the runner threads each
// op's own output shape rather than assuming the stage-terminal one, and Shape is the terminal the rental reads.
public readonly record struct PlaneStage(StageKind Kind, Seq<(PlaneOp Op, PlaneShape Shape)> Ops, PlaneShape Shape, int Radius);

public readonly record struct PlaneReceipt(Seq<string> Operations, Seq<string> Stages, long Texels, Option<HeightEvidence> Height, double ElapsedMs) {
    public static readonly PlaneReceipt Empty = new(Seq<string>.Empty, Seq<string>.Empty, 0L, None, 0.0);

    // The one correctness signal that survives preconditioning and cancellation — press#PRESS_RECEIPT projects it
    // rather than re-deriving the evidence chain.
    public Option<double> Residual => Height.Map(static evidence => evidence.Residual);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public abstract partial record PlaneOp {
    // ONE entry over every arity. An empty sequence returns the source and an empty receipt, so a caller composing a
    // possibly-empty post chain needs no guard of its own; the injected clock is what makes the receipt's elapsed a
    // measurement rather than a literal zero, and press#TEXTURE_PRESS threads its own.
    public static Fin<(TexturePlane Plane, PlaneReceipt Receipt)> Apply(TexturePlane source, Seq<PlaneOp> ops, Op key, TimeProvider? clock = null) {
        if (ops.IsEmpty) { return Fin.Succ((source, PlaneReceipt.Empty)); }
        TimeProvider ticks = clock ?? TimeProvider.System;
        long opened = ticks.GetTimestamp();
        return Schedule(PlaneShape.Of(source), ops, key).Bind(stages => Run(source, stages, ops, key, ticks, opened));
    }

    // PLAN then SCHEDULE: the shape folds across every op first, so a refusal costs no rental; the fold then groups
    // consecutive fusing ops into one stage and gives every non-fusing op its own, pairing EVERY op with the shape it
    // ends at — a fused run may change lane count mid-chain, and a runner holding only the terminal shape would hand
    // a mid-run op the wrong stride.
    private static Fin<Seq<PlaneStage>> Schedule(PlaneShape input, Seq<PlaneOp> ops, Op key) =>
        ops.Fold(Fin.Succ((Shape: input, Stages: Seq<PlaneStage>.Empty)), (state, op) => state.Bind(carry =>
            op.Project(carry.Shape, key).Map(shape => (
                Shape: shape,
                Stages: !carry.Stages.IsEmpty && carry.Stages.Last.Kind.Fuses && op.Stage.Fuses
                    ? carry.Stages.Init.Add(carry.Stages.Last with { Ops = carry.Stages.Last.Ops.Add((op, shape)), Shape = shape })
                    : carry.Stages.Add(new PlaneStage(op.Stage, Seq1((op, shape)), shape, op.Radius))))))
        .Map(static carry => carry.Stages);

    // RUN: one rental per stage, the previous intermediate disposed at the boundary, the SOURCE never touched — so a
    // twenty-op chain holds at most two planes and the caller's input survives to be re-used or re-keyed. Execute is
    // Fin-valued: a solver refusal PROPAGATES and the failed stage's rental disposes, because an integration that
    // could not factor swallowed into an empty Option ships a plane of zeros wearing a success.
    private static Fin<(TexturePlane, PlaneReceipt)> Run(
        TexturePlane source, Seq<PlaneStage> stages, Seq<PlaneOp> ops, Op key, TimeProvider ticks, long opened) =>
        stages.Fold(Fin.Succ((Plane: source, Evidence: Option<HeightEvidence>.None)), (state, stage) => state.Bind(carry =>
            TexturePlane.Of(stage.Shape.Format, stage.Shape.Width, stage.Shape.Height, stage.Shape.Transfer,
                    stage.Shape.Alpha, key, Some(stage.Shape.Layers), Some(stage.Shape.Range), AllocationMode.Default)
                .Bind(destination => PlaneKernel.Execute(carry.Plane, destination, stage, key)
                    .Map(evidence => {
                        if (!ReferenceEquals(carry.Plane, source)) { carry.Plane.Dispose(); }
                        return (Plane: destination, Evidence: evidence.IfNone(() => carry.Evidence));
                    })
                    .MapFail(fault => { destination.Dispose(); return fault; }))))
        .Map(carry => (carry.Plane, new PlaneReceipt(
            ops.Map(static op => op.Switch(
                resize: static _ => "resize", convolve: static _ => "convolve", heightNormal: static _ => "heightNormal",
                fromHeight: static _ => "fromHeight", remap: static _ => "remap", swizzle: static _ => "swizzle")),
            stages.Map(static stage => stage.Kind.Key),
            carry.Plane.Texels,
            carry.Evidence,
            ticks.GetElapsedTime(opened).TotalMilliseconds)));
}

// The stage runner. Pointwise fuses into ONE row pass; neighbourhood takes a bordered pass; global takes the whole
// plane. Every body is a fixed-extent index walk over caller-owned buffers — the page's named kernel exemption.
// Execute is Fin-valued and dispatches through the vocabulary's own generated Switch, so a new StageKind row breaks
// here at compile time and a solver refusal reaches the rail rather than an empty Option.
internal static class PlaneKernel {
    internal static Fin<Option<HeightEvidence>> Execute(TexturePlane source, TexturePlane destination, PlaneStage stage, Op key) =>
        stage.Kind.Switch(
            state: (Source: source, Destination: destination, Stage: stage, Key: key),
            pointwise:     static s => Pointwise(s.Source, s.Destination, s.Stage),
            neighbourhood: static s => Neighbourhood(s.Source, s.Destination, s.Stage, s.Key),
            global:        static s => Global(s.Source, s.Destination, s.Stage, s.Key));

    // The fused row pass: one partition over height x layers, each op in the run threaded through a PING-PONG pair
    // at ITS OWN shape — the previous op's output is the next op's input, so a swizzle-then-remap chain remaps the
    // swizzled lanes rather than the untouched source, and a two-swizzle chain composes rather than racing.
    private static Fin<Option<HeightEvidence>> Pointwise(TexturePlane source, TexturePlane destination, PlaneStage stage) {
        PointwiseRows action = new(source, destination, stage.Ops);
        ParallelHelper.For(0, destination.Height.Value * destination.Layers.Value, in action);
        return Fin.Succ(Option<HeightEvidence>.None);
    }

    private readonly struct PointwiseRows(TexturePlane source, TexturePlane destination, Seq<(PlaneOp Op, PlaneShape Shape)> ops) : IAction {
        public void Invoke(int index) {
            int layer = index / destination.Height.Value, row = index % destination.Height.Value;
            int widest = Math.Max(source.Width.Value * 4, destination.Width.Value * 4);
            using SpanOwner<double> ping = SpanOwner<double>.Allocate(widest);
            using SpanOwner<double> pong = SpanOwner<double>.Allocate(widest);
            source.Read(row, layer, ping.Span[..source.RowScalars]);
            Span<double> current = ping.Span;
            Span<double> next = pong.Span;
            int lanes = source.Lanes;
            foreach ((PlaneOp op, PlaneShape shape) in ops) {
                int outLanes = shape.Format.Components;
                Thread(op, current, next, source.Width.Value, lanes, outLanes);
                (current, next) = (next, current);
                lanes = outLanes;
            }
            destination.Write(row, layer, current[..destination.RowScalars]);
        }

        // One op, one shape hop. Remap runs in place on the copied row; a swizzle projects lane-for-lane. The
        // pattern switch is deliberate: the row spans are ref structs no generated dispatch state can carry, and
        // the kernel is the page's named statement exemption — the non-fusing cases are unreachable by scheduling.
        private static void Thread(PlaneOp op, ReadOnlySpan<double> input, Span<double> output, int width, int inLanes, int outLanes) {
            switch (op) {
                case PlaneOp.Remap remap:
                    input[..(width * inLanes)].CopyTo(output);
                    Remap(remap.Curve, output[..(width * inLanes)], inLanes);
                    break;
                case PlaneOp.Swizzle swizzle:
                    Project(swizzle.Lanes, input, output, width, inLanes, outLanes);
                    break;
                default: break;
            }
        }

        private static void Project(Seq<SwizzleLane> lanes, ReadOnlySpan<double> input, Span<double> output, int width, int inLanes, int outLanes) {
            for (int x = 0; x < width; x++) {
                ReadOnlySpan<double> texel = input.Slice(x * inLanes, inLanes);
                for (int lane = 0; lane < Math.Min(lanes.Count, outLanes); lane++) {
                    output[(x * outLanes) + lane] = lanes[lane].Project(texel);
                }
            }
        }

        // Levels is affine-plus-gamma in place; Lut stages the row's colour triple through the composed .cube fold.
        // Both leave the alpha lane untouched — a tonal curve over coverage darkens every edge.
        private static void Remap(RemapCurve curve, Span<double> row, int lanes) {
            switch (curve) {
                case RemapCurve.Levels levels: {
                    double span = levels.White - levels.Black;
                    for (int i = 0; i < row.Length; i++) {
                        if (lanes > 1 && (i % lanes) == lanes - 1) { continue; }
                        double normalized = span == 0.0 ? 0.0 : (row[i] - levels.Black) / span;
                        row[i] = Math.Pow(double.Clamp(normalized, 0.0, 1.0), levels.Gamma);
                    }
                    break;
                }
                case RemapCurve.Lut lut: {
                    int texels = row.Length / lanes;
                    using SpanOwner<float> triple = SpanOwner<float>.Allocate(texels * 3);
                    for (int x = 0; x < texels; x++) {
                        for (int c = 0; c < 3; c++) { triple.Span[(x * 3) + c] = (float)row[(x * lanes) + Math.Min(c, lanes - 1)]; }
                    }
                    lut.Table.Apply(triple.Span, triple.Span, texels, lut.Interpolation);
                    for (int x = 0; x < texels; x++) {
                        for (int c = 0; c < Math.Min(3, lanes); c++) { row[(x * lanes) + c] = triple.Span[(x * 3) + c]; }
                    }
                    break;
                }
                default: break; // Histogram is StageKind.Global by its own row and never reaches the fused pass.
            }
        }
    }

    // The bordered pass. The whole SOURCE materializes into one interleaved staging run so the kernel can address a
    // neighbour the row rail alone cannot reach, and the stage's own EdgeMode addresses every out-of-extent tap —
    // clamping, reflecting, or wrapping, the last being what makes a tiled plane convolve without a seam.
    private static Fin<Option<HeightEvidence>> Neighbourhood(TexturePlane source, TexturePlane destination, PlaneStage stage, Op key) {
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(
            source.Width.Value * source.Height.Value * source.Layers.Value * source.Lanes);
        Materialize(source, staging.Span);
        return stage.Ops.Head.Op.Switch(
            convolve: op => { Separable(staging.Span, source, destination, op); return Fin.Succ(Option<HeightEvidence>.None); },
            heightNormal: op => Fin.Succ(Some(HeightField.ToNormal(source, destination, op.Evidence, key))),
            fromHeight: op => Fin.Succ(Some(Derive(staging.Span, source, destination, op, key))),
            resize: static _ => Fin.Succ(Option<HeightEvidence>.None),
            remap: static _ => Fin.Succ(Option<HeightEvidence>.None),
            swizzle: static _ => Fin.Succ(Option<HeightEvidence>.None));
    }

    // The whole-plane pass. A resize is separable and delegates to the composed resampler; a histogram match gathers
    // the source distribution BEFORE it maps a texel; a height integration solves once over the whole grid and its
    // refusal PROPAGATES — an unfactorable Laplacian swallowed to absence ships a zero plane wearing a success.
    private static Fin<Option<HeightEvidence>> Global(TexturePlane source, TexturePlane destination, PlaneStage stage, Op key) =>
        stage.Ops.Head.Op.Switch(
            resize: op => { Resample(source, destination, op); return Fin.Succ(Option<HeightEvidence>.None); },
            remap: op => { Match(source, destination, op.Curve); return Fin.Succ(Option<HeightEvidence>.None); },
            heightNormal: op => HeightField.ToHeight(source, destination, op.Solver, op.Evidence, key).Map(Some),
            convolve: static _ => Fin.Succ(Option<HeightEvidence>.None),
            fromHeight: static _ => Fin.Succ(Option<HeightEvidence>.None),
            swizzle: static _ => Fin.Succ(Option<HeightEvidence>.None));

    // The composed separable resample over one interleaved staging run. The extent groups bracket the two spans and
    // the channel count follows BOTH, so a source-extent-then-channel-count spelling transposes the destination; the
    // alpha lane index is passed where the plane carries coverage, so the resampler premultiplies across the fold.
    private static void Resample(TexturePlane source, TexturePlane destination, PlaneOp.Resize op) {
        using MemoryOwner<float> input = MemoryOwner<float>.Allocate(source.Width.Value * source.Height.Value * source.Lanes);
        using MemoryOwner<float> output = MemoryOwner<float>.Allocate(destination.Width.Value * destination.Height.Value * destination.Lanes);
        Materialize(source, input.Span);
        ImageProcessing.Resize(input.Span, source.Width.Value, source.Height.Value, output.Span,
            destination.Width.Value, destination.Height.Value, source.Lanes, op.Filter, op.Edge,
            source.Alpha.Carries ? source.Lanes - 1 : -1);
        Deposit(destination, output.Span);
    }

    // The four staging primitives every non-fusing pass shares. Materialize and Deposit are the ONE plane-to-run
    // bridge, generic over the staging scalar so the float legs the composed folds demand and the double legs the
    // local kernels prefer are one body rather than two transcriptions.
    private static void Materialize<T>(TexturePlane plane, Span<T> staging) where T : unmanaged, INumberBase<T> {
        using SpanOwner<double> row = SpanOwner<double>.Allocate(plane.Width.Value * plane.Lanes);
        for (int layer = 0, at = 0; layer < plane.Layers.Value; layer++) {
            for (int y = 0; y < plane.Height.Value; y++) {
                plane.Read(y, layer, row.Span);
                for (int i = 0; i < row.Length; i++, at++) { staging[at] = T.CreateSaturating(row.Span[i]); }
            }
        }
    }

    private static void Deposit<T>(TexturePlane plane, ReadOnlySpan<T> staging) where T : unmanaged, INumberBase<T> {
        using SpanOwner<double> row = SpanOwner<double>.Allocate(plane.Width.Value * plane.Lanes);
        for (int layer = 0, at = 0; layer < plane.Layers.Value; layer++) {
            for (int y = 0; y < plane.Height.Value; y++) {
                for (int i = 0; i < row.Length; i++, at++) { row.Span[i] = double.CreateSaturating(staging[at]); }
                plane.Write(y, layer, row.Span);
            }
        }
    }

    // The separable convolution. A Gaussian is one axis-pass pair; an unsharp mask is that same blur SUBTRACTED from
    // the source and scaled — so the sharpen arm reuses the blur rather than carrying a second kernel, and the
    // threshold suppresses amplification of the noise floor a flat region carries.
    private static void Separable(ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, PlaneOp.Convolve op) { /* two axis passes at op.Kernel.Radius under op.Edge; the UnsharpMask arm folds source + Amount * (source - blurred) where |source - blurred| exceeds Threshold */ }

    // The height-derived fields. Occlusion casts op.Derivative's ray budget against the blue-noise spectrum over the
    // Deterministic stream keyed by its own seed; curvature projects the Hessian eigenvalue pair onto the requested
    // measure. Both read the SAME staged height run, so a set deriving both pays one materialization.
    private static HeightEvidence Derive(ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, PlaneOp.FromHeight op, Op key) { /* Occlusion: cosine-weighted hemisphere cast to op.Distance; Curvature: Hessian projection onto op.Measure */ }

    // The histogram match. The source distribution gathers over the WHOLE plane into op.Bins buckets before the first
    // texel maps, which is exactly why this curve is a global stage and cannot fuse into a row pass.
    private static void Match(TexturePlane source, TexturePlane destination, RemapCurve curve) { /* accumulate the empirical CDF over Bins, then map each texel through it into TargetCdf */ }
}
```

## [04]-[HEIGHT_FIELD]

- Owner: `HeightEvidence` the correspondence carrier; `HeightSolver` the integration routes; the occlusion and curvature derivative kernels.
- Law: `HeightEvidence` is what makes the `HeightNormal` inverse honest. A gradient field determines a height field only up to an additive constant and, at integer depth, only up to the scale the forward normalization consumed — so the forward RECORDS the millimetre span, the field mean, and the convention it read, and the inverse consumes them. An inverse invoked with no evidence rails rather than reconstructing a plausible field, because a fabricated amplitude is a displacement that renders confidently and wrongly.
- Law: `HeightSolver` is chosen by PERIODICITY, not by preference. `Spectral` is the Frankot-Chellappa least-squares integration in the frequency domain and it assumes a periodic domain — which is exactly true of a tiled plane and exactly false of a bounded one, where it wraps the opposite edge's gradient into the solution. `Poisson` assembles the five-point Laplacian with Neumann boundaries and factors it once, which is correct on a bounded plane and needlessly expensive on a tiled one. The `Periodic` column carries the choice, so a caller states the plane's own nature rather than a solver name.
- Law: the spectral route runs entirely on the composed transform: forward the gradient divergence, divide by the frequency-squared kernel with the zero bin held at zero (the additive constant the evidence's mean then restores), and inverse. Every buffer is caller-owned and the transform mutates it in place under the symmetric scaling that makes the forward-inverse round trip an identity.
- Law: the bounded route assembles by TRIPLET ACCUMULATION and factors through the kernel's own SPD cache. Duplicate triplets sum and zeros drop at admission, so the Laplacian assembles by accumulation and never by hand-built compressed storage, and the factor carries the pivot-loss refusal onto the typed rail rather than as a bare exception.
- Law: occlusion casts against the BLUE-NOISE spectrum with a replayable stream. The cast directions are drawn from the kernel sampler spectrum rather than from a uniform draw, because uniform ray sets clump and leave the low-frequency banding an occlusion map shows worst on a flat wall; the stream is the kernel splitmix64 keyed by the derivative's own seed, so a re-derived occlusion plane is byte-identical and its set re-keys to the same address.
- Law: curvature reads the height field's SECOND derivative through the kernel's own stencil family and projects the requested measure — mean, Gaussian, or a principal extremum — so the four measures are one eigenvalue projection rather than four kernels, and the signed result rides a `PlaneRange.Signed` plane whose integer encoding is the vocabulary's single packing.
- Packages: MathNet.Numerics (composed — `IntegralTransforms.Fourier.Forward2D(Complex[], int, int, FourierOptions)` and `Inverse2D(Complex[], int, int, FourierOptions)` over the row-major gradient staging, `FourierOptions.Default` the symmetric scaling that makes the round trip an identity), `Rasm.Numerics` (composed — `SparseMatrix.FromTriplets` the accumulating assembly, `CholeskySparse.Of`/`Solve` the SPD factor cache carrying the pivot-loss refusal, `Nabla` the stencil family the curvature projection reads, `SampleKind` the blue-noise spectrum, `Dimension`), `Rasm.Domain` (composed — `Deterministic.NextUnit` the ONE replayable jitter stream, `Op`), CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate` the solver staging, `ParallelHelper.For` the cast partition), BCL inbox (`System.Numerics.Complex`).
- Growth: a new integration route is one `HeightSolver` row plus one solve arm; a new derived field is one `HeightDerivative` case; a new curvature measure is one enum row the eigenvalue projection reads.
- Boundary: this section derives fields from a height plane and never SOURCES one. A height plane arrives from an ingest classification, a press bake, or the `HeightNormal` inverse over an acquired normal plane under a depth prior — and no inference stage emits height, because integration under a prior is pure mathematics the estate owns rather than a model it would have to license.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Numerics;                            // Complex
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using MathNet.Numerics.IntegralTransforms;        // Fourier, FourierOptions
using Rasm.Domain;                                // Op, Deterministic
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using Rasm.Numerics;                              // SparseMatrix, CholeskySparse, SampleKind, Dimension
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// The integration route, chosen by the PLANE's periodicity rather than by a solver preference. The spectral route
// assumes a periodic domain and wraps the opposite edge's gradient on a bounded plane; the bounded route factors a
// Neumann Laplacian and is needlessly expensive on a tiled one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HeightSolver {
    public static readonly HeightSolver Spectral = new("spectral", periodic: true);
    public static readonly HeightSolver Poisson = new("poisson", periodic: false);

    public bool Periodic { get; }
    private HeightSolver(string key, bool periodic) : this(key) => Periodic = periodic;
}

// --- [MODELS] ------------------------------------------------------------------------------
// What the forward direction DESTROYED and the inverse must be handed back: the millimetre span the normalized [0,1]
// field was measured against, the mean the integration's free constant restores, the convention the gradient read,
// and the residual the solve reports. Unit is the identity evidence a channel row carries where the set supplies the
// real span at bind time.
public readonly record struct HeightEvidence(double ScaleMm, double Mean, NormalConvention Convention, double Residual) {
    public static readonly HeightEvidence Unit = new(ScaleMm: 1.0, Mean: 0.5, NormalConvention.Gl, Residual: 0.0);
    public HeightEvidence With(double residual) => this with { Residual = residual };
}

// --- [OPERATIONS] --------------------------------------------------------------------------
internal static class HeightField {
    // The FORWARD direction: the central-difference gradient of the height field scaled by its own millimetre span,
    // composed into a unit tangent-space normal under the declared convention. It records the evidence the inverse
    // consumes, so the round trip is a correspondence rather than two unrelated transforms.
    internal static HeightEvidence ToNormal(TexturePlane height, TexturePlane normal, HeightEvidence evidence, Op key) { /* bordered central difference scaled by evidence.ScaleMm; write the normalized (-dx, -dy * evidence.Convention.GreenSign, 1) triple, and return the evidence carrying the measured field mean */ }

    // The INVERSE: least-squares integration of the gradient field. The spectral route divides the divergence by the
    // frequency-squared kernel with the zero bin HELD at zero — that bin is exactly the additive constant integration
    // cannot recover, so zeroing it and restoring the evidence's mean is the honest reconstruction rather than a
    // fabricated offset. The transform mutates the caller-owned buffer under symmetric scaling.
    internal static Fin<HeightEvidence> ToHeight(TexturePlane normal, TexturePlane height, HeightSolver solver, HeightEvidence evidence, Op key) =>
        solver.Periodic ? Spectral(normal, height, evidence, key) : Bounded(normal, height, evidence, key);

    private static Fin<HeightEvidence> Spectral(TexturePlane normal, TexturePlane height, HeightEvidence evidence, Op key) {
        int w = height.Width.Value, h = height.Height.Value;
        using MemoryOwner<Complex> field = MemoryOwner<Complex>.Allocate(w * h);
        Divergence(normal, field.Span, w, h);
        Fourier.Forward2D(field.DangerousGetArray().Array!, h, w, FourierOptions.Default);
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                double u = 2.0 * Math.PI * (x <= w / 2 ? x : x - w) / w;
                double v = 2.0 * Math.PI * (y <= h / 2 ? y : y - h) / h;
                double denominator = (u * u) + (v * v);
                field.Span[(y * w) + x] = denominator > 0.0 ? field.Span[(y * w) + x] / -denominator : Complex.Zero;
            }
        }
        Fourier.Inverse2D(field.DangerousGetArray().Array!, h, w, FourierOptions.Default);
        using MemoryOwner<double> real = MemoryOwner<double>.Allocate(w * h);
        for (int i = 0; i < real.Length; i++) { real.Span[i] = field.Span[i].Real; }
        return Fin.Succ(Restore(height, real.Span, evidence, w, h));
    }

    // The BOUNDED route: the five-point Neumann Laplacian assembled by triplet ACCUMULATION — duplicates sum and
    // zeros drop at admission — factored once through the kernel's SPD cache, which lowers the composed solver's bare
    // pivot-loss exception onto the typed rail rather than letting it escape.
    private static Fin<HeightEvidence> Bounded(TexturePlane normal, TexturePlane height, HeightEvidence evidence, Op key) {
        Dimension order = Dimension.Create(value: checked((int)height.Texels));
        using MemoryOwner<double> rhs = MemoryOwner<double>.Allocate(order.Value);
        Divergence(normal, rhs.Span, height.Width.Value, height.Height.Value);
        return from matrix in SparseMatrix.FromTriplets(order, order, Laplacian(height.Width.Value, height.Height.Value), key)
               from factor in CholeskySparse.Of(matrix, key)
               from solved in factor.Solve(new Arr<double>(rhs.Span), key)
               select Restore(height, solved.AsSpan(), evidence, height.Width.Value, height.Height.Value);
    }

    // The gradient DIVERGENCE the integration inverts, over the staged normal field: each texel's (-nx/nz, -ny/nz)
    // slope pair differentiated once more, so the right-hand side is the Laplacian of the height field being sought.
    private static void Divergence(TexturePlane normal, Span<double> field, int width, int height) { /* stage the normal rows, form the slope pair per texel, and accumulate the central-difference divergence */ }
    private static void Divergence(TexturePlane normal, Span<Complex> field, int width, int height) { /* the same accumulation into the spectral staging, imaginary part zero */ }

    // The five-point Neumann Laplacian as a TRIPLET STREAM: duplicates sum and zeros drop at admission, so the
    // boundary rows assemble by accumulation and a hand-built compressed storage never appears.
    private static IEnumerable<(int Row, int Col, double Value)> Laplacian(int width, int height) { /* interior -4/+1 stencil; boundary rows drop their absent neighbours, which IS the Neumann condition */ }

    // Integration recovers the field up to an additive constant, so the reconstruction re-centres on the evidence's
    // recorded mean rather than on whatever offset the solve happened to land — and it reports the residual it left.
    private static HeightEvidence Restore(TexturePlane height, ReadOnlySpan<double> solved, HeightEvidence evidence, int width, int rows) { /* re-centre on evidence.Mean, write through the row rail, and return evidence.With(residual) */ }
}
```

## [05]-[RESEARCH]

- [FOURIER_BUFFER_ACCESS]-[OPEN]: the spectral route hands the transform the pooled rental's backing array through `MemoryOwner<Complex>.DangerousGetArray()`. Verify that the returned `ArraySegment<Complex>` has a zero offset for a full-length rental; a non-zero offset would transform the wrong window, and the fallback is a plain `Complex[]` staging with one copy at each end.
- [NABLA_CURVATURE_PROJECTION]-[OPEN]: the four `CurvatureMeasure` rows are asserted to project from one Hessian eigenvalue pair the kernel `Numerics/calculus` `Nabla` stencil family supplies. Verify the stencil entry's spelling and whether it returns the second-derivative triple directly; the alternative is a local 3x3 second-difference kernel, which would be a hand-rolled reimplementation if the stencil family already covers it.
- [SAMPLEKIND_DIRECTION_DRAW]-[OPEN]: the occlusion cast draws hemisphere directions from the kernel `Processing/sample` `SampleKind` blue-noise spectrum. Verify the entry that turns a spectrum row plus a `Deterministic` stream into a cosine-weighted hemisphere direction set; a spectrum that only produces planar points would need the cosine mapping stated here, and stating it twice is the duplication this row exists to prevent.
