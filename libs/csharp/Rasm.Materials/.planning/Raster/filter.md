# [MATERIALS_FILTER]

THE DECODED-PLANE TRANSFORM ALGEBRA. One `PlaneOp` `[Union]` closes the per-plane transform family — resample, convolve, height correspondence, height derivative, coverage dilation, tonal remap, and lane swizzle — under ONE `Apply` entry that PLANS every shape before it rents an output, SCHEDULES the sequence into stages by each op's own dependency class, and folds each stage over the plane's decoded row rails. `PlaneOp` holds a transform as a case, `ConvolveKernel` a convolution as a case, `RemapCurve` a tonal curve as a case, `HeightDerivative` a height derivative as a case, `HeightPolicy` a solver stop as a row, and `SwizzleLane` a lane projection as a row — never a per-transform entrypoint, a per-curve method, a constant inside a kernel, or a boolean selecting between two bodies.

Scheduling is the page's load-bearing decision, and it exists because the ops genuinely differ in what they can see. `Levels` remaps one texel; a Gaussian reads a neighbourhood; a histogram remap must see the WHOLE plane before it can map its first texel; a normal-to-height integration is a global spectral or sparse-linear solve; a resample changes the grid itself. Fusing that mixture into one per-row pass is a fiction — a row action cannot supply a neighbour it has not written, a plane statistic it has not gathered, or a grid it has not resized — so each case declares its `StageKind` and the scheduler fuses only what genuinely fuses: consecutive pointwise ops collapse into ONE row pass, a neighbourhood op takes a bordered two-buffer pass under its own `EdgeMode`, and a global op takes a whole-plane pass. Adding an op therefore cannot corrupt its neighbours, and adding a dependency class is one row on the scheduler. `PlaneOp` composes the `plane#TEXTURE_PLANE` typed arena with its decoded `Read`/`Write` row rails, its `CellLattice` grid, its `RunMm` spatial grain, and its `PlaneFormat.For` retyping, the `codec#RASTER_FAULT` band-2460 rail for nothing at all (every refusal here is a SHAPE refusal on band 2450), TinyEXR.NET `ImageProcessing`/`Lut3D` for every separable resample, transfer, and LUT fold, the kernel `Nabla` lattice-stencil arm for every grid derivative, the kernel `WeightKernelFamily` for every Gaussian weight, MathNet.Numerics `Fourier` for the spectral integration, the kernel `SparseMatrix`/`CholeskySparse` for the bounded Poisson solve, the kernel `Deterministic` coordinate-keyed draw for the occlusion cast's per-texel rotation, and `CommunityToolkit.HighPerformance` `ParallelHelper` over `struct IAction` partitions — re-minting no resampler, no transform, no stencil, no weight profile, no factorization, and no random source.

## [01]-[INDEX]

- [02]-[PLANE_OP]: `StageKind` axes the dependency, `ConvolveKernel`/`RemapCurve`/`HeightDerivative` family the cases, `SwizzleLane` rosters the projections, and the seven-case `PlaneOp` union projects every shape totally.
- [03]-[PLANE_STAGE]: `PlaneOp.Apply` plans, schedules, and runs — fusing the pointwise run, bordering the neighbourhood pass, and publishing `PlaneReceipt` evidence.
- [04]-[HEIGHT_FIELD]: `HeightEvidence` carries the correspondence, `HeightPolicy` carries the stop, `HeightSolver` routes the spectral and bounded integrations, and the occlusion and curvature derivatives read the height field.

## [02]-[PLANE_OP]

- Owner: `PlaneOp` the transform family; `StageKind` the dependency axis each case declares; `ConvolveKernel` the neighbourhood-kernel family; `RemapCurve` the tonal-curve family; `HeightDerivative` the height-derived-field family; `SwizzleLane` the lane-projection roster; `PlaneShape` the projected shape carrier.
- Cases: op {`Resize`, `Convolve`, `HeightNormal`, `FromHeight`, `Dilate`, `Remap`, `Swizzle`} · stage {`pointwise`, `neighbourhood`, `global`} · kernel {`Gaussian`, `UnsharpMask`, `Bilateral`, `Median`} · curve {`Levels`, `Histogram`, `Lut`} · derivative {`Occlusion`, `Curvature`} · lane {`r`, `g`, `b`, `a`, `zero`, `one`, `rInverse`, `gInverse`, `bInverse`, `aInverse`}.
- Law: `HeightNormal` carries BOTH directions of one correspondence on one case. `Inverse` is the column carrying direction, because a height field and a tangent-space normal field are the forward and inverse of a single relation — never a `NormalFromHeight`/`HeightFromNormal` sibling pair. `HeightEvidence` crosses from the forward to the inverse — millimetre amplitude, field mean, and the convention the forward recorded — because integration recovers a gradient field's shape and never its absolute offset or amplitude, and an inverse whose ingress is raw samples re-shaped into the forward's input domain fabricates exactly what the forward destroyed. `HeightPolicy` rides beside it because a Krylov stop MOVES the produced bytes, so both enter the `Digest` preimage whole.
- Law: `SwizzleLane` is DATA — a source index, a scale, and a bias — so lane reordering, lane inversion, constant fill, and the `dx`→`gl` green flip are all one kernel over a row of rows. `Swizzle(R, GInverse, B, A)` is exactly the `plane#PLANE_VOCABULARY` `NormalConvention` conversion and mints no second operation, so the corpus has one green-flip site rather than a conversion pair beside a swizzle.
- Law: `Remap` closes the tonal family on one case. `RemapCurve.Levels.Invert` is a ROW of the levels case — black at one, white at zero — so the `roughness = 1 − gloss` ingest inversion, a contrast stretch, and a gamma lift are one curve family rather than an `Invert` op beside a `Levels` op. Every curve evaluates in the LINEAR domain over decoded lanes, which is what makes an `srgb`-authored gloss plane invert correctly rather than forking the roughness silently.
- Law: `Dilate` fills a plane's UNWRITTEN texels from their nearest written neighbours, ring by ring, which is what makes a chart-packed or atlased plane survive its own mip chain: a bilinear tap straddling a chart boundary otherwise reads the neutral, and every level halves that bleed further into the shaded surface. Coverage IS the alpha lane, so the op needs no second carrier and no unwritten-texel sentinel — a plane carrying no coverage REFUSES at `Project`, because inferring emptiness from a zero texel would dilate every legitimately black region into its neighbours.
- Law: `Project` is TOTAL and runs before any rental. It folds the whole sequence into a final `PlaneShape`, so a shape refusal anywhere in the chain leaves the source untouched and costs nothing — a mid-chain refusal after three rentals is the failure mode the plan-first order forecloses. Retyping resolves through `PlaneFormat.For`, so a lane-count change lands on the storage row the semantic count rounds up to and never on a fabricated format.
- Law: shape refusals rail `MaterialFault.Parameter` on band 2450. This page reaches band 2460 nowhere: a filter has no container, no device, and no synthesizer, so a `RasterFault` here would be a shape refusal wearing a mechanical code.
- Entry: `PlaneOp.Apply(TexturePlane source, Seq<PlaneOp> ops, Op key, TimeProvider? clock = null)` is the ONE entry over every arity — an empty sequence returns the source with an empty receipt, a single op and a chain take the identical path, and no `ApplyOne`/`ApplyMany` pair exists; the clock rides so the receipt's elapsed is measured, and `press#TEXTURE_PRESS` threads its own. `PlaneOp.Digest` is the ONE canonical per-op spelling a content-key preimage folds — `press#PRESS_PLAN` pieces its post chains through it, and a consumer spelling an op through `ToString` re-keys on the next case rename.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`Layer`/`Grid`/`RunMm`, `PlaneFormat.For`, `PlaneTransfer`, `AlphaMode`, `PlaneRange`, `NormalConvention`), TinyEXR.NET (composed — `ResizeFilter`/`EdgeMode` the resample vocabulary, `Lut3D.TryParseCube`/`Apply` the `.cube` curve), `Rasm.Numerics` (composed — `WeightKernelFamily.Gaussian.Weight` the ONE Gaussian profile, `Dimension`, `UnitInterval`), `Rasm.Domain` (`Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transform is one `PlaneOp` case declaring its `StageKind` with one `Project` arm, one `Digest` arm, and one kernel arm — the scheduler, the receipt, and every consumer are untouched. Every new curve is one `RemapCurve` case, a new derived field one `HeightDerivative` case, a new lane projection one `SwizzleLane` row. Every new convolution is one `ConvolveKernel` case AND a separability declaration: `Gaussian` and `UnsharpMask` are separable and take the axis-pass pair, `Bilateral` and `Median` are not — a range weight and an order statistic each break the product — so both take a square-window body under the SAME `EdgeMode` addressing rather than a second edge law.
- Boundary: this page transforms DECODED planes and decides nothing about what a plane MEANS. Channel semantics, neutrals, packing, and mip law are `set#TEXTURE_CHANNEL`'s; containers are `codec#RASTER_CODEC`'s; the mip chain is `plane#TEXTURE_PYRAMID`'s and `Resize` is deliberately NOT its alias — a level is the grid's own `Coarsen` step under a declared policy, so a resize can never produce a level a sampler then trilinearly blends against a different filter's neighbours.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                       // CultureInfo — the invariant Digest spelling
using LanguageExt;                                // Fin, Option, Seq
using Rasm.Domain;                                // Op
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault — the band-2450 shape rail
using Rasm.Numerics;                              // Dimension, UnitInterval
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

// ConvolveKernel families the neighbourhood kernels, split by SEPARABILITY. Gaussian and UnsharpMask are one separable profile — a sharpen
// is the source minus its own blur, scaled and thresholded — so ONE axis-pass pair serves both. Bilateral and
// Median are not separable: a range weight and an order statistic each break the product, so each takes a square
// window under the same edge addressing. Separable is the column the kernel dispatch reads, never a type test.
[Union]
public abstract partial record ConvolveKernel {
    private ConvolveKernel() { }

    public sealed record Gaussian(double Sigma) : ConvolveKernel;
    public sealed record UnsharpMask(double Sigma, double Amount, double Threshold) : ConvolveKernel;
    // Bilateral is the EDGE-PRESERVING blur: a spatial Gaussian weight times a range Gaussian over the colour
    // distance, so a de-lit photograph loses its sensor noise and keeps its mortar joints. RangeSigma is stated
    // in DECODED units, so it means the same thing whatever depth stores the plane.
    public sealed record Bilateral(double Sigma, double RangeSigma) : ConvolveKernel;
    // Median is the ORDER-STATISTIC despeckle: the one kernel that removes an impulse without smearing it, which
    // is exactly what a neural stage's per-tile seam artefact is.
    public sealed record Median(int Radius) : ConvolveKernel;

    public bool Separable => Switch(
        gaussian:    static _ => true,
        unsharpMask: static _ => true,
        bilateral:   static _ => false,
        median:      static _ => false);

    public double Sigma => Switch(
        gaussian:    static k => k.Sigma,
        unsharpMask: static k => k.Sigma,
        bilateral:   static k => k.Sigma,
        median:      static _ => 0.0);

    // Three standard deviations carry over 99.7% of a Gaussian's mass, so a weighted halo truncates there rather
    // than at a caller-supplied radius that silently clips the tail into a visible ring at high sigma; the order
    // statistic carries its own window instead, because a median has no tail to truncate.
    public int Radius => Switch(
        gaussian:    static k => Math.Max(1, (int)Math.Ceiling(3.0 * k.Sigma)),
        unsharpMask: static k => Math.Max(1, (int)Math.Ceiling(3.0 * k.Sigma)),
        bilateral:   static k => Math.Max(1, (int)Math.Ceiling(3.0 * k.Sigma)),
        median:      static k => Math.Max(1, k.Radius));

    public string Digest => Switch(
        gaussian:    static k => string.Create(CultureInfo.InvariantCulture, $"gaussian|{k.Sigma:R}"),
        unsharpMask: static k => string.Create(CultureInfo.InvariantCulture, $"unsharp|{k.Sigma:R}|{k.Amount:R}|{k.Threshold:R}"),
        bilateral:   static k => string.Create(CultureInfo.InvariantCulture, $"bilateral|{k.Sigma:R}|{k.RangeSigma:R}"),
        median:      static k => string.Create(CultureInfo.InvariantCulture, $"median|{k.Radius}"));
}

// RemapCurve families the tonal curves. Levels is affine-plus-gamma, Histogram matches an empirical CDF, Lut applies a parsed .cube —
// three curves, one case each, all evaluated in the LINEAR domain over decoded lanes.
[Union]
public abstract partial record RemapCurve {
    private RemapCurve() { }

    public sealed record Levels(double Black, double White, double Gamma) : RemapCurve {
        // Black above White is the INVERSION, so `roughness = 1 - gloss` is a row of this case rather than an
        // Invert op beside it — and it inverts after the decode, which keeps an srgb-authored gloss plane honest.
        public static readonly Levels Invert = new(Black: 1.0, White: 0.0, Gamma: 1.0);
        public static readonly Levels Identity = new(Black: 0.0, White: 1.0, Gamma: 1.0);
    }

    public sealed record Histogram(Seq<double> TargetCdf, int Bins) : RemapCurve;

    // TableKey is the parsed table's content identity, minted from the .cube SOURCE TEXT at the one TryParseCube
    // call site (ContentHash.Of over its UTF-8 bytes) — Lut3D exposes no lattice read-back, so the source bytes
    // are the only digestible truth and a Lut case without them is a curve no plan key can name.
    public sealed record Lut(Lut3D Table, LutInterpolation Interpolation, UInt128 TableKey) : RemapCurve;

    // Only the histogram match needs the whole plane before it maps a texel; the other two are per-texel functions.
    public StageKind Stage => Switch(
        levels:    static _ => StageKind.Pointwise,
        histogram: static _ => StageKind.Global,
        lut:       static _ => StageKind.Pointwise);

    public string Digest => Switch(
        levels:    static c => string.Create(CultureInfo.InvariantCulture, $"levels|{c.Black:R}|{c.White:R}|{c.Gamma:R}"),
        // Every empirical CDF enters WHOLE: a bin count alone admits two different target distributions under one
        // key, and a cached plane matched to a different histogram is cache poisoning wearing a key.
        histogram: static c => string.Create(CultureInfo.InvariantCulture,
            $"histogram|{c.Bins}|{string.Join(',', c.TargetCdf.Map(static v => v.ToString("R", CultureInfo.InvariantCulture)))}"),
        lut:       static c => string.Create(CultureInfo.InvariantCulture, $"lut|{c.TableKey:x32}|{(int)c.Interpolation}"));
}

// HeightDerivative families the fields a height plane derives. Occlusion carries its own cast policy with compile-time defaults so a channel
// row spells `new HeightDerivative.Occlusion()` and takes the estate's cast rather than restating three numbers.
[Union]
public abstract partial record HeightDerivative {
    private HeightDerivative() { }

    public sealed record Occlusion(int Rays = 64, double Distance = 0.05, ulong Seed = 0UL) : HeightDerivative;
    public sealed record Curvature(CurvatureMeasure Measure = CurvatureMeasure.Mean) : HeightDerivative;

    public string Digest => Switch(
        occlusion: static d => string.Create(CultureInfo.InvariantCulture, $"occlusion|{d.Rays}|{d.Distance:R}|{d.Seed:x16}"),
        curvature: static d => string.Create(CultureInfo.InvariantCulture, $"curvature|{(int)d.Measure}"));
}

public enum CurvatureMeasure { Mean, Gaussian, PrincipalMaximum, PrincipalMinimum }

// SwizzleLane carries a lane projection as DATA. Source is the input lane or -1 for a constant; Scale and Bias carry the inversion,
// so lane reorder, lane inversion, constant fill, and the dx->gl green flip are one kernel over one row table.
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

    // FlipGreen spells the plane#PLANE_VOCABULARY dx->gl conversion ONCE for the corpus.
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
    public sealed record HeightNormal(bool Inverse, HeightEvidence Evidence, HeightSolver Solver, HeightPolicy Policy) : PlaneOp;
    public sealed record FromHeight(HeightDerivative Derivative, HeightEvidence Evidence) : PlaneOp;
    public sealed record Dilate(int Rings) : PlaneOp;
    public sealed record Remap(RemapCurve Curve) : PlaneOp;
    public sealed record Swizzle(Seq<SwizzleLane> Lanes) : PlaneOp;

    // Stage answers the dependency class the scheduler reads. It is a PROJECTION of the case, never a column a caller supplies,
    // so an op cannot declare itself cheaper than it is and get fused into a row pass it cannot survive.
    public StageKind Stage => Switch(
        resize:       static _ => StageKind.Global,
        convolve:     static _ => StageKind.Neighbourhood,
        heightNormal: static op => op.Inverse ? StageKind.Global : StageKind.Neighbourhood,
        fromHeight:   static _ => StageKind.Neighbourhood,
        dilate:       static _ => StageKind.Neighbourhood,
        remap:        static op => op.Curve.Stage,
        swizzle:      static _ => StageKind.Pointwise);

    public int Radius => Switch(
        resize:       static _ => 0,
        convolve:     static op => op.Kernel.Radius,
        heightNormal: static op => op.Inverse ? 0 : 1,
        fromHeight:   static _ => 1,
        dilate:       static op => Math.Max(1, op.Rings),
        remap:        static _ => 0,
        swizzle:      static _ => 0);

    // THE canonical per-op spelling every content-key preimage folds — press#PRESS_PLAN's post-chain pieces read
    // this member and nothing else. Rename-stable by construction: case tokens are frozen lowercase literals,
    // owned rows spell their SmartEnum Key, external enums spell their invariant integer — never a type name a
    // refactor re-keys — and every numeric formats under InvariantCulture at "R" round-trip precision. Evidence
    // columns that only REPORT (HeightEvidence.Residual) stay out of the preimage; every column that moves the
    // produced bytes enters whole, the solver stop policy included.
    public string Digest => Switch(
        resize:       static op => string.Create(CultureInfo.InvariantCulture,
            $"resize|{op.Width.Value}x{op.Height.Value}|{(int)op.Filter}|{(int)op.Edge}"),
        convolve:     static op => string.Create(CultureInfo.InvariantCulture, $"convolve|{op.Kernel.Digest}|{(int)op.Edge}"),
        heightNormal: static op => string.Create(CultureInfo.InvariantCulture,
            $"height-normal|{(op.Inverse ? "inverse" : "forward")}|{op.Solver.Key}|{op.Policy.Digest}|{op.Evidence.Digest}"),
        fromHeight:   static op => string.Create(CultureInfo.InvariantCulture, $"from-height|{op.Derivative.Digest}|{op.Evidence.Digest}"),
        dilate:       static op => string.Create(CultureInfo.InvariantCulture, $"dilate|{op.Rings}"),
        remap:        static op => $"remap|{op.Curve.Digest}",
        swizzle:      static op => $"swizzle|{string.Join(',', op.Lanes.Map(static lane => lane.Key))}");

    // Kind is the case KEY the receipt publishes — one projection over the union's own case names, so a benchmark row, a
    // wire field, and a receipt entry all read the same token and none of them reflects a runtime type name.
    public string Kind => Switch(
        resize:       static _ => "resize",
        convolve:     static _ => "convolve",
        heightNormal: static _ => "heightNormal",
        fromHeight:   static _ => "fromHeight",
        dilate:       static _ => "dilate",
        remap:        static _ => "remap",
        swizzle:      static _ => "swizzle");

    // TOTAL shape projection, folded across the whole sequence before the first rental — so a chain that cannot
    // type costs nothing and leaves the source untouched.
    public Fin<PlaneShape> Project(PlaneShape input, Op key) => Switch(
        resize: op => input.Layers.Value is 1
            ? Fin.Succ(input with { Width = op.Width, Height = op.Height })
            : MaterialFault.Parameter(key, $"<resize-layered:{input.Layers.Value}>"),
        convolve: _ => Fin.Succ(input),
        heightNormal: op => (op.Inverse, input.Format.Components) switch {
            (false, 1) => input.Retyped(3, AlphaMode.None, PlaneRange.Signed, key),
            // Inverse solves ONE bounded or periodic grid: the Laplacian and the spectral kernel are w x h
            // operators, so a layered plane refuses outright — per-layer integration is the caller's Layer fold,
            // exactly as Resize rules it.
            (true, >= 3) when input.Layers.Value is not 1 =>
                MaterialFault.Parameter(key, $"<height-inverse-layered:{input.Layers.Value}>"),
            (true, >= 3) => input.Retyped(1, AlphaMode.None, PlaneRange.Unit, key),
            (false, int n) => MaterialFault.Parameter(key, $"<height-normal-scalar:{n}>"),
            (true, int n) => MaterialFault.Parameter(key, $"<height-normal-vector:{n}>"),
        },
        fromHeight: op => input.Format.Components is 1
            ? input.Retyped(1, AlphaMode.None, op.Derivative is HeightDerivative.Curvature ? PlaneRange.Signed : PlaneRange.Unit, key)
            : MaterialFault.Parameter(key, $"<from-height-scalar:{input.Format.Components}>"),
        // Coverage IS the discriminant between written and unwritten, so a plane carrying none refuses rather than
        // reading a zero texel as emptiness — that inference dilates every legitimately black region outward.
        dilate: op => (input.Alpha.Carries, op.Rings) switch {
            (false, _) => MaterialFault.Parameter(key, $"<dilate-no-coverage:{input.Alpha.Key}>"),
            (_, <= 0) => MaterialFault.Parameter(key, $"<dilate-rings:{op.Rings}>"),
            _ => Fin.Succ(input),
        },
        remap: _ => Fin.Succ(input),
        swizzle: op => op.Lanes.IsEmpty
            ? MaterialFault.Parameter(key, "<swizzle-lanes-empty>")
            : input.Retyped(op.Lanes.Count, input.Alpha, input.Range, key));
}
```

## [03]-[PLANE_STAGE]

- Owner: `PlaneOp.Apply` the plan-schedule-run entry; `PlaneStage` the scheduled group; `PlaneReceipt` the evidence.
- Entry: `Apply(source, ops, key)` returns the transformed plane paired with its receipt. Its source is never mutated and never disposed — the caller owns it, because a chain that consumed its input would make a receipt useless as evidence.
- Law: SCHEDULING is what makes the algebra honest. Consecutive `pointwise` ops fuse into ONE row pass over one intermediate; a `neighbourhood` op takes its own pass against a materialized previous stage under its `EdgeMode` addressing; a `global` op takes a whole-plane pass. One fused row action across the whole sequence is the deleted form — it cannot supply a neighbour it has not written, a plane statistic it has not gathered, or a grid it has not resized, so the ops whose correctness depends on any of those would silently read the wrong texels.
- Law: fusion is a run-length fold over the sequence, not a special case: a chain of one op schedules identically to a chain of twenty, so the receipt reports the same stage structure at every arity and a benchmark reading it compares like with like.
- Law: each stage rents ONE output and disposes the previous intermediate at the stage boundary, so a twenty-op chain holds at most two planes and the source. Its final stage's output is the returned plane; the source is untouched.
- Law: EVERY Gaussian weight on this page is the kernel `Numerics/calculus#WEIGHT_PROFILES` `WeightKernelFamily.Gaussian` profile, and the bandwidth correspondence is READ off that row rather than copied from it. That row evaluates `exp(-B·(d/s)²)` under a frozen `B` it keeps private, so one probe at a known ratio recovers `B` at type initialization and the support that makes `Weight(d, support)` equal `exp(-d²/(2σ²))` is `σ·√(2B)` — a bandwidth change on the kernel row therefore moves every tap table here with no edit. Its three-sigma halo sits strictly inside that support, so no tap is zeroed by the row's own support cut and the truncation stays the page's declared one.
- Law: row work rides `ParallelHelper.For<TAction>` over a `struct IAction` whose fields are the two planes, the stage's ops, and the key. That action is `default`-constructed per partition or copied from the `in` seed, so the partition allocates nothing, inlines, and captures nothing — a `Parallel.For` over a closure would allocate one delegate and one display class per stage and defeat exactly the partition this shape exists for.
- Law: the receipt carries the op KEYS from the union's own `Kind` projection, never a runtime type name. Reflected type names are stale by construction against a rename and allocate on every op; the case key is the same string the wire and the benchmark row read.
- Law: layer work is per layer inside the row action: a layered plane's rows are one arena band per layer, so a stage walks `height × layers` rows and a resize refuses a layered plane outright rather than resampling across a face boundary that has no spatial meaning.
- Packages: CommunityToolkit.HighPerformance (composed — `ParallelHelper.For<TAction>(int, int)` the row partition, `IAction` the allocation-free slot, `SpanOwner<T>.Allocate` the per-row lane scratch, `MemoryOwner<T>.Allocate` the whole-plane statistic staging), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`RunMm`/`Grid`, the decoded row rails every kernel reads), `Rasm.Numerics` (composed — `WeightKernelFamily.Gaussian.Weight(double, double)` the ONE Gaussian profile every tap table and every range weight reads), TinyEXR.NET (composed — `ImageProcessing.Resize(ReadOnlySpan<float>, int, int, Span<float>, int, int, int, ResizeFilter, EdgeMode, int)` with the extent groups bracketing the two spans and the channel count following BOTH, `Lut3D.Apply(ReadOnlySpan<float>, Span<float>, int, LutInterpolation)`), `Rasm.Domain` (`Op`), LanguageExt.Core.
- Growth: a new dependency class is one `StageKind` row with one arm in the runner; a new op reaching an existing class adds nothing here at all.
- Boundary: the runner is the page's `[EXPRESSION_SPINE]` kernel exemption — fixed-extent index walks filling caller-owned buffers — while every admission, plan, schedule, and receipt surface is expression-bodied. Statements stop at the row kernel.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Numerics;                            // INumberBase — the staging-scalar constraint
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;   // ParallelHelper, IAction
using TinyEXR.V3;                                 // ImageProcessing — the composed separable resample
using LanguageExt;
using Rasm.Domain;                                // Op, Deterministic — the ONE replayable coordinate-keyed draw
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using Rasm.Numerics;                              // WeightKernelFamily — the ONE Gaussian profile
using Rhino.Geometry;                             // Point3d — the coordinate key the occlusion rotation draws on
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] ------------------------------------------------------------------------------
// One scheduled group. Ops is a fused pointwise run or exactly one non-fusing op, EACH op paired with the shape it
// ENDS at — a fused run may change lane count mid-chain (a swizzle before a remap), so the runner threads each
// op's own output shape rather than assuming the stage-terminal one, and Shape is the terminal the rental reads.
public readonly record struct PlaneStage(StageKind Kind, Seq<(PlaneOp Op, PlaneShape Shape)> Ops, PlaneShape Shape, int Radius);

public readonly record struct PlaneReceipt(Seq<string> Operations, Seq<string> Stages, long Texels, Option<HeightEvidence> Height, double ElapsedMs) {
    public static readonly PlaneReceipt Empty = new(Seq<string>.Empty, Seq<string>.Empty, 0L, None, 0.0);

    // Residual is the one signal that survives preconditioning and cancellation — press#PRESS_RECEIPT projects it
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
    // ends at — a fused run may change lane count mid-chain, and a runner holding only the terminal shape would hand a
    // mid-run op the wrong stride.
    private static Fin<Seq<PlaneStage>> Schedule(PlaneShape input, Seq<PlaneOp> ops, Op key) =>
        ops.Fold(Fin.Succ((Shape: input, Stages: Seq<PlaneStage>.Empty)), (state, op) => state.Bind(carry =>
            op.Project(carry.Shape, key).Map(shape => (
                Shape: shape,
                Stages: !carry.Stages.IsEmpty && carry.Stages.Last.Kind.Fuses && op.Stage.Fuses
                    ? carry.Stages.Init.Add(carry.Stages.Last with { Ops = carry.Stages.Last.Ops.Add((op, shape)), Shape = shape })
                    : carry.Stages.Add(new PlaneStage(op.Stage, Seq1((op, shape)), shape, op.Radius))))))
        .Map(static carry => carry.Stages);

    // RUN: one rental per stage, the previous intermediate disposed at the boundary, the SOURCE never touched — so a
    // twenty-op chain holds at most two planes and the caller's input survives to be re-used or re-keyed. Each rental
    // adopts the SOURCE GRID where the extent is unchanged and the resized stage seats a fresh one, so a
    // physically-pitched plane keeps its spatial grain through a whole chain. Execute is Fin-valued: a solver refusal
    // PROPAGATES and the failed stage's rental disposes, because an integration that could not factor swallowed into an
    // empty Option ships a plane of zeros wearing a success.
    private static Fin<(TexturePlane, PlaneReceipt)> Run(
        TexturePlane source, Seq<PlaneStage> stages, Seq<PlaneOp> ops, Op key, TimeProvider ticks, long opened) =>
        stages.Fold(Fin.Succ((Plane: source, Evidence: Option<HeightEvidence>.None)), (state, stage) => state.Bind(carry =>
            Rent(carry.Plane, stage, key)
                .Bind(destination => PlaneKernel.Execute(carry.Plane, destination, stage, key)
                    .Map(evidence => {
                        if (!ReferenceEquals(carry.Plane, source)) { carry.Plane.Dispose(); }
                        return (Plane: destination, Evidence: evidence.IfNone(() => carry.Evidence));
                    })
                    .MapFail(fault => { destination.Dispose(); return fault; }))))
        .Map(carry => (carry.Plane, new PlaneReceipt(
            ops.Map(static op => op.Kind),
            stages.Map(static stage => stage.Kind.Key),
            carry.Plane.Texels,
            carry.Evidence,
            ticks.GetElapsedTime(opened).TotalMilliseconds)));

    // Rent discriminates on EXTENT, not on the op: a stage holding the source extent adopts the source's own
    // lattice through the plane page's grid modality, so the affine and its spatial grain survive; a stage that
    // resized seats a fresh identity lattice at the new census, because a resample changes what one texel spans
    // and carrying the old cell would report a grain the plane no longer has.
    private static Fin<TexturePlane> Rent(TexturePlane source, PlaneStage stage, Op key) =>
        stage.Shape.Width == source.Width && stage.Shape.Height == source.Height
            ? TexturePlane.Of(stage.Shape.Format, source.Grid, stage.Shape.Layers, stage.Shape.Transfer,
                stage.Shape.Alpha, stage.Shape.Range, source.Primaries, key, AllocationMode.Default)
            : TexturePlane.Of(stage.Shape.Format, stage.Shape.Width, stage.Shape.Height, stage.Shape.Transfer,
                stage.Shape.Alpha, key, Some(stage.Shape.Layers), Some(stage.Shape.Range), Some(source.Primaries),
                mode: AllocationMode.Default);
}

// PlaneKernel runs the stages. Pointwise fuses into ONE row pass; neighbourhood takes a bordered pass; global takes the whole
// plane. Every body is a fixed-extent index walk over caller-owned buffers — the page's named kernel exemption.
// Execute is Fin-valued and dispatches through the vocabulary's own generated Switch, so a new StageKind row breaks
// here at compile time and a solver refusal reaches the rail rather than an empty Option.
internal static class PlaneKernel {
    // SupportPerSigma READS the bandwidth correspondence off the kernel row rather than copying it. That row evaluates
    // exp(-B(d/s)^2) under a private B, so one probe at a known ratio recovers B and the support making
    // Weight(d, support) equal exp(-d^2/(2 sigma^2)) is sigma*sqrt(2B). Nothing here restates a bandwidth, so a
    // kernel-side change moves every tap table and every range weight on this page with no edit.
    private static readonly double SupportPerSigma =
        Math.Sqrt(2.0 * -Math.Log(WeightKernelFamily.Gaussian.Weight(distance: 1.0, support: 2.0)) * 4.0);

    internal static double Support(double sigma) => Math.Max(1e-6, sigma) * SupportPerSigma;

    internal static Fin<Option<HeightEvidence>> Execute(TexturePlane source, TexturePlane destination, PlaneStage stage, Op key) =>
        stage.Kind.Switch(
            state: (Source: source, Destination: destination, Stage: stage, Key: key),
            pointwise:     static s => Pointwise(s.Source, s.Destination, s.Stage),
            neighbourhood: static s => Neighbourhood(s.Source, s.Destination, s.Stage, s.Key),
            global:        static s => Global(s.Source, s.Destination, s.Stage, s.Key));

    // Pointwise fuses the row pass: one partition over height x layers, each op in the run threaded through a PING-PONG pair
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
            int widest = Math.Max(source.Width.Value, destination.Width.Value) * PlaneFormat.MaxComponents;
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
        // this body is the page's named statement exemption — the non-fusing cases are unreachable by scheduling,
        // and the tail arm still copies the row so a reached arm can never publish pool residue as a result.
        private static void Thread(PlaneOp op, ReadOnlySpan<double> input, Span<double> output, int width, int inLanes, int outLanes) {
            switch (op) {
                case PlaneOp.Remap remap:
                    input[..(width * inLanes)].CopyTo(output);
                    Remap(remap.Curve, output[..(width * inLanes)], inLanes);
                    break;
                case PlaneOp.Swizzle swizzle:
                    Project(swizzle.Lanes, input, output, width, inLanes, outLanes);
                    break;
                default:
                    input[..(width * inLanes)].CopyTo(output);
                    break;
            }
        }

        // Every OUTPUT lane writes: a lane past the swizzle roster fills zero, because the retype rounds a
        // semantic count up through the storage roster and an unwritten trailing lane would carry the ping-pong
        // buffer's previous contents into the result.
        private static void Project(Seq<SwizzleLane> lanes, ReadOnlySpan<double> input, Span<double> output, int width, int inLanes, int outLanes) {
            for (int x = 0; x < width; x++) {
                ReadOnlySpan<double> texel = input.Slice(x * inLanes, inLanes);
                for (int lane = 0; lane < outLanes; lane++) {
                    output[(x * outLanes) + lane] = lane < lanes.Count ? lanes[lane].Project(texel) : 0.0;
                }
            }
        }

        // Levels is affine-plus-gamma in place; Lut stages the row's colour triple through the composed .cube
        // fold. Both leave the alpha lane untouched — a tonal curve over coverage darkens every edge.
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
                    // Argument three is the CHANNEL COUNT of the interleaved run — the span length carries the
                    // texel count — so a texel count here reads a three-channel row as an N-channel one.
                    lut.Table.Apply(triple.Span, triple.Span, 3, lut.Interpolation);
                    for (int x = 0; x < texels; x++) {
                        for (int c = 0; c < Math.Min(3, lanes); c++) { row[(x * lanes) + c] = triple.Span[(x * 3) + c]; }
                    }
                    break;
                }
                default: break; // Histogram is StageKind.Global by its own row and never reaches the fused pass.
            }
        }
    }

    // Neighbourhood borders the pass: the whole SOURCE materializes into one interleaved staging run so the kernel can address a
    // neighbour the row rail alone cannot reach, and the stage's own EdgeMode addresses every out-of-extent tap —
    // clamping, reflecting, or wrapping, the last being what makes a tiled plane convolve without a seam.
    private static Fin<Option<HeightEvidence>> Neighbourhood(TexturePlane source, TexturePlane destination, PlaneStage stage, Op key) {
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(
            source.Width.Value * source.Height.Value * source.Layers.Value * source.Lanes);
        Materialize(source, staging.Span);
        return stage.Ops.Head.Op.Switch(
            convolve: op => { Convolve(staging.Span, source, destination, op); return Fin.Succ(Option<HeightEvidence>.None); },
            heightNormal: op => Fin.Succ(Some(HeightField.ToNormal(staging.Span, source, destination, op.Evidence, key))),
            fromHeight: op => Fin.Succ(Some(Derive(staging.Span, source, destination, op, key))),
            dilate: op => { Dilate(staging.Span, source, destination, op, stage); return Fin.Succ(Option<HeightEvidence>.None); },
            resize: static _ => Fin.Succ(Option<HeightEvidence>.None),
            remap: static _ => Fin.Succ(Option<HeightEvidence>.None),
            swizzle: static _ => Fin.Succ(Option<HeightEvidence>.None));
    }

    // Global takes the whole plane: a resize is separable and delegates to the composed resampler; a histogram match
    // gathers the source distribution BEFORE it maps a texel; a height integration solves once over the whole grid
    // and its refusal PROPAGATES — an unfactorable Laplacian swallowed to absence ships a zero plane wearing a
    // success.
    private static Fin<Option<HeightEvidence>> Global(TexturePlane source, TexturePlane destination, PlaneStage stage, Op key) =>
        stage.Ops.Head.Op.Switch(
            resize: op => { Resample(source, destination, op); return Fin.Succ(Option<HeightEvidence>.None); },
            remap: op => { Match(source, destination, op.Curve); return Fin.Succ(Option<HeightEvidence>.None); },
            heightNormal: op => HeightField.ToHeight(source, destination, op.Solver, op.Evidence, op.Policy, key).Map(Some),
            convolve: static _ => Fin.Succ(Option<HeightEvidence>.None),
            fromHeight: static _ => Fin.Succ(Option<HeightEvidence>.None),
            dilate: static _ => Fin.Succ(Option<HeightEvidence>.None),
            swizzle: static _ => Fin.Succ(Option<HeightEvidence>.None));

    // Resample delegates to the composed separable resampler over one interleaved staging run: extent groups bracket the two spans
    // and the channel count follows BOTH, so a source-extent-then-channel-count spelling transposes the
    // destination; the alpha lane index is passed where the plane carries coverage, so the resampler
    // premultiplies across the fold.
    private static void Resample(TexturePlane source, TexturePlane destination, PlaneOp.Resize op) {
        using MemoryOwner<float> input = MemoryOwner<float>.Allocate(source.Width.Value * source.Height.Value * source.Lanes);
        using MemoryOwner<float> output = MemoryOwner<float>.Allocate(destination.Width.Value * destination.Height.Value * destination.Lanes);
        Materialize(source, input.Span);
        ImageProcessing.Resize(input.Span, source.Width.Value, source.Height.Value, output.Span,
            destination.Width.Value, destination.Height.Value, source.Lanes, op.Filter, op.Edge,
            source.Alpha.Carries ? source.Lanes - 1 : -1);
        Deposit(destination, output.Span);
    }

    // Four staging primitives serve every non-fusing pass. Materialize and Deposit are the ONE plane-to-run
    // bridge, generic over the staging scalar so the float legs the composed folds demand and the double legs the
    // local kernels prefer are one body rather than two transcriptions.
    internal static void Materialize<T>(TexturePlane plane, Span<T> staging) where T : unmanaged, INumberBase<T> {
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

    // Convolve dispatches on the kernel's own SEPARABILITY column, never a type test: the separable pair
    // takes the axis-pass form and the non-separable pair takes the square window, both under the stage's one
    // EdgeMode addressing.
    private static void Convolve(ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, PlaneOp.Convolve op) {
        if (op.Kernel.Separable) { Separable(staging, source, destination, op); return; }
        Square(staging, source, destination, op);
    }

    // Separable takes the axis-pass pair: a Gaussian is TWO passes over one intermediate, which is O(2r) per texel
    // where a square kernel is O(r²) — at the three-sigma radius a sigma-8 blur costing 2401 taps square costs 98
    // separable. The unsharp arm reuses that same blur rather than carrying a second kernel: the halo it subtracts
    // IS the blur, and the threshold suppresses amplification of the noise floor a flat region carries. Every tap
    // reads the kernel row's own Gaussian profile at the support the bandwidth probe resolved, so no exponential
    // is spelled here; weights then normalize over the taps the edge mode actually admitted, which keeps a Clamp
    // border from darkening or brightening the rim the way a fixed divisor does.
    private static void Separable(ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, PlaneOp.Convolve op) {
        int width = source.Width.Value, height = source.Height.Value, lanes = source.Lanes, radius = op.Kernel.Radius;
        int taps = (radius * 2) + 1;
        using SpanOwner<double> weights = SpanOwner<double>.Allocate(taps);
        double support = Support(op.Kernel.Sigma), total = 0.0;
        for (int tap = -radius; tap <= radius; tap++) {
            double weight = WeightKernelFamily.Gaussian.Weight(Math.Abs(tap), support);
            weights.Span[tap + radius] = weight;
            total += weight;
        }
        for (int tap = 0; tap < taps; tap++) { weights.Span[tap] /= total; }

        // Coverage premultiplies BEFORE the fold and divides back out after — the same law the mip fold
        // freezes — so a transparent texel never bleeds its colour across a coverage edge; the alpha lane itself
        // blurs as plain coverage.
        using MemoryOwner<double> working = MemoryOwner<double>.Allocate(staging.Length);
        staging.CopyTo(working.Span);
        int alphaLane = source.Alpha.Carries ? lanes - 1 : -1;
        if (alphaLane >= 0) {
            for (int at = 0; at < working.Length; at += lanes) {
                double coverage = working.Span[at + alphaLane];
                for (int c = 0; c < alphaLane; c++) { working.Span[at + c] *= coverage; }
            }
        }
        using MemoryOwner<double> horizontal = MemoryOwner<double>.Allocate(staging.Length);
        using MemoryOwner<double> blurred = MemoryOwner<double>.Allocate(staging.Length);
        for (int layer = 0, plane = 0; layer < source.Layers.Value; layer++, plane += width * height * lanes) {
            Axis(working.Span, horizontal.Span, plane, width, height, lanes, radius, weights.Span, op.Edge, horizontalPass: true);
            Axis(horizontal.Span, blurred.Span, plane, width, height, lanes, radius, weights.Span, op.Edge, horizontalPass: false);
        }
        if (alphaLane >= 0) {
            for (int at = 0; at < blurred.Length; at += lanes) {
                double coverage = blurred.Span[at + alphaLane];
                for (int c = 0; c < alphaLane; c++) { blurred.Span[at + c] = coverage > 0.0 ? blurred.Span[at + c] / coverage : 0.0; }
            }
        }
        if (op.Kernel is not ConvolveKernel.UnsharpMask mask) { Deposit(destination, blurred.Span); return; }
        for (int at = 0; at < blurred.Length; at++) {
            double difference = staging[at] - blurred.Span[at];
            blurred.Span[at] = Math.Abs(difference) > mask.Threshold ? staging[at] + (mask.Amount * difference) : staging[at];
        }
        Deposit(destination, blurred.Span);
    }

    // ONE axis body serves both passes: the pass flag selects which coordinate the tap walks, so the vertical
    // pass is the horizontal one transposed rather than a second transcription that drifts.
    private static void Axis(
        ReadOnlySpan<double> input, Span<double> output, int plane, int width, int height, int lanes,
        int radius, ReadOnlySpan<double> weights, EdgeMode edge, bool horizontalPass) {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                for (int lane = 0; lane < lanes; lane++) {
                    double sum = 0.0, admitted = 0.0;
                    for (int tap = -radius; tap <= radius; tap++) {
                        int sx = horizontalPass ? Address(x + tap, width, edge) : x;
                        int sy = horizontalPass ? y : Address(y + tap, height, edge);
                        if (sx < 0 || sy < 0) { continue; }
                        double weight = weights[tap + radius];
                        sum += weight * input[plane + (((sy * width) + sx) * lanes) + lane];
                        admitted += weight;
                    }
                    output[plane + (((y * width) + x) * lanes) + lane] = admitted > 0.0 ? sum / admitted : 0.0;
                }
            }
        }
    }

    // Square serves the non-separable pair: a bilateral weight is the spatial Gaussian times a RANGE Gaussian over the whole
    // colour distance, so every lane of one texel shares one range weight and an edge stays an edge in all lanes
    // rather than decorrelating per channel; a median selects the window's middle order statistic per lane, which
    // is what removes an impulse without smearing it. Both read the same Address fold as the separable pair, so
    // EdgeMode.Wrap keeps a tiled plane seamless under either.
    private static void Square(ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, PlaneOp.Convolve op) {
        int width = source.Width.Value, height = source.Height.Value, lanes = source.Lanes, radius = op.Kernel.Radius;
        int colour = source.Alpha.Carries ? lanes - 1 : lanes, window = ((radius * 2) + 1) * ((radius * 2) + 1);
        double spatial = Support(op.Kernel.Sigma);
        double range = op.Kernel is ConvolveKernel.Bilateral bilateral ? Support(bilateral.RangeSigma) : 0.0;
        bool ordered = op.Kernel is ConvolveKernel.Median;
        using MemoryOwner<double> output = MemoryOwner<double>.Allocate(staging.Length);
        using SpanOwner<double> sample = SpanOwner<double>.Allocate(window);
        for (int layer = 0, plane = 0; layer < source.Layers.Value; layer++, plane += width * height * lanes) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int centre = plane + (((y * width) + x) * lanes);
                    for (int lane = 0; lane < lanes; lane++) {
                        int gathered = 0;
                        double sum = 0.0, admitted = 0.0;
                        for (int dy = -radius; dy <= radius; dy++) {
                            for (int dx = -radius; dx <= radius; dx++) {
                                int sx = Address(x + dx, width, op.Edge), sy = Address(y + dy, height, op.Edge);
                                if (sx < 0 || sy < 0) { continue; }
                                int at = plane + (((sy * width) + sx) * lanes);
                                if (ordered) { sample.Span[gathered++] = staging[at + lane]; continue; }
                                double weight = WeightKernelFamily.Gaussian.Weight(Math.Sqrt((dx * dx) + (dy * dy)), spatial)
                                              * WeightKernelFamily.Gaussian.Weight(Distance(staging, at, centre, colour), range);
                                sum += weight * staging[at + lane];
                                admitted += weight;
                            }
                        }
                        output.Span[centre + lane] = ordered
                            ? Middle(sample.Span[..gathered])
                            : admitted > 0.0 ? sum / admitted : staging[centre + lane];
                    }
                }
            }
        }
        Deposit(destination, output.Span);
    }

    // Distance measures RANGE as Euclidean over the colour lanes alone: coverage is an area, not a colour, so folding
    // it into the range term would make a transparent neighbour read as a distant one.
    private static double Distance(ReadOnlySpan<double> staging, int at, int centre, int colour) {
        double sum = 0.0;
        for (int c = 0; c < colour; c++) {
            double delta = staging[at + c] - staging[centre + c];
            sum += delta * delta;
        }
        return Math.Sqrt(sum);
    }

    // Middle selects the window's middle order statistic by insertion. Window size is (2r+1)^2 and the page's radius law
    // caps a median at its own declared rings, so the selection is bounded by the kernel row rather than by the plane
    // — a full sort would pay log factors this window size never earns.
    private static double Middle(Span<double> sample) {
        for (int i = 1; i < sample.Length; i++) {
            double value = sample[i];
            int j = i - 1;
            while (j >= 0 && sample[j] > value) { sample[j + 1] = sample[j]; j--; }
            sample[j + 1] = value;
        }
        return sample.Length is 0 ? 0.0 : sample[sample.Length / 2];
    }

    // Dilate advances coverage. Each ring pushes written colour one texel outward: an uncovered texel whose window holds
    // covered neighbours takes their coverage-weighted mean and becomes covered itself, so the front advances exactly
    // one texel per ring and a chart gutter fills from its own chart rather than from whichever neighbour a single
    // wide pass reached first. Coverage is the alpha lane by the op's own admission gate, and the pass writes it back
    // at one so a later mip fold sees a written texel.
    private static void Dilate(ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, PlaneOp.Dilate op, PlaneStage stage) {
        int width = source.Width.Value, height = source.Height.Value, lanes = source.Lanes, colour = lanes - 1;
        EdgeMode edge = EdgeMode.Clamp;
        using MemoryOwner<double> current = MemoryOwner<double>.Allocate(staging.Length);
        using MemoryOwner<double> next = MemoryOwner<double>.Allocate(staging.Length);
        staging.CopyTo(current.Span);
        for (int ring = 0; ring < op.Rings; ring++) {
            current.Span.CopyTo(next.Span);
            for (int layer = 0, plane = 0; layer < source.Layers.Value; layer++, plane += width * height * lanes) {
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        int centre = plane + (((y * width) + x) * lanes);
                        if (current.Span[centre + colour] > 0.0) { continue; }
                        double weight = 0.0;
                        for (int c = 0; c < colour; c++) { next.Span[centre + c] = 0.0; }
                        for (int dy = -1; dy <= 1; dy++) {
                            for (int dx = -1; dx <= 1; dx++) {
                                int sx = Address(x + dx, width, edge), sy = Address(y + dy, height, edge);
                                if (sx < 0 || sy < 0) { continue; }
                                int at = plane + (((sy * width) + sx) * lanes);
                                double coverage = current.Span[at + colour];
                                if (coverage <= 0.0) { continue; }
                                for (int c = 0; c < colour; c++) { next.Span[centre + c] += coverage * current.Span[at + c]; }
                                weight += coverage;
                            }
                        }
                        if (weight <= 0.0) { continue; }
                        for (int c = 0; c < colour; c++) { next.Span[centre + c] /= weight; }
                        next.Span[centre + colour] = 1.0;
                    }
                }
            }
            next.Span.CopyTo(current.Span);
        }
        Deposit(destination, current.Span);
    }

    // Address is the ONE out-of-extent fold every neighbourhood kernel on this page reads. Wrap is what makes a
    // tiled plane convolve without a seam, Reflect mirrors about the border texel rather than repeating it, and
    // Clamp is stated as a NEGATIVE index the caller drops from its weight sum rather than as a duplicated edge
    // texel — a clamped tap contributing its own value at full weight is a rim the blur brightens.
    private static int Address(int index, int extent, EdgeMode edge) =>
        index >= 0 && index < extent
            ? index
            : edge switch {
                EdgeMode.Wrap => ((index % extent) + extent) % extent,
                EdgeMode.Reflect => Reflect(index, extent),
                _ => -1,
            };

    private static int Reflect(int index, int extent) {
        int period = Math.Max(1, (extent - 1) * 2);
        int folded = ((index % period) + period) % period;
        return folded < extent ? folded : period - folded;
    }

    // Derive routes the height-derived fields. Occlusion sweeps its ray budget as a low-discrepancy azimuth fan rotated per
    // texel by the Deterministic coordinate draw; curvature projects the kernel Hessian's eigenvalue pair onto the
    // requested measure. Both read the SAME staged height run, so a set deriving both pays one materialization,
    // both walk LAYERS at a plane offset so a cube face never reads its neighbour's relief across the seam, and
    // both return the evidence carrying the field mean the inverse consumes.
    private static HeightEvidence Derive(ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, PlaneOp.FromHeight op, Op key) =>
        op.Derivative switch {
            HeightDerivative.Occlusion cast => Occlude(staging, source, destination, cast, op.Evidence),
            _ => Curve(staging, source, destination, (HeightDerivative.Curvature)op.Derivative, op.Evidence),
        };

    // Occlude sweeps the horizon. Each ray marches the height field along one azimuth out to the derivative's own reach in
    // texel units and records the greatest elevation angle it saw; the texel's visibility is the fraction of the
    // hemisphere no horizon occluded. The azimuth fan is a REGULAR set rotated per texel — the rotated regular
    // construction whose discrepancy beats an independent uniform draw at every ray count, and whose values are
    // exactly computable, so a re-derived plane is byte-identical. The rotation is COORDINATE-keyed rather than
    // stream-sequential, so a band partition cannot reorder a draw and the seed genuinely replays.
    private static HeightEvidence Occlude(
        ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, HeightDerivative.Occlusion cast, HeightEvidence evidence) {
        int width = source.Width.Value, height = source.Height.Value, layers = source.Layers.Value, lanes = source.Lanes;
        int reach = Math.Max(1, (int)Math.Round(cast.Distance * Math.Max(width, height)));
        int rays = Math.Max(1, cast.Rays);
        using MemoryOwner<double> visibility = MemoryOwner<double>.Allocate(width * height * layers);
        double mean = 0.0;
        for (int layer = 0; layer < layers; layer++) {
            int plane = layer * width * height * lanes;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    double centre = Tap(staging, plane, x, y, width, height, lanes);
                    mean += centre;
                    // Two int parameters carry the FULL 64-bit seed — low half as salt, high half as seed —
                    // so the replay key loses nothing; Point3d is the kernel draw's own coordinate parameter.
                    double rotation = Deterministic.UnitInterval(new Point3d(x, y, layer),
                        salt: unchecked((int)cast.Seed), seed: unchecked((int)(cast.Seed >> 32)));
                    double open = 0.0;
                    for (int ray = 0; ray < rays; ray++) {
                        double azimuth = ((ray / (double)rays) + rotation) * 2.0 * Math.PI;
                        double dx = Math.Cos(azimuth), dy = Math.Sin(azimuth);
                        double horizon = 0.0;
                        for (int step = 1; step <= reach; step++) {
                            int sx = x + (int)Math.Round(dx * step), sy = y + (int)Math.Round(dy * step);
                            // Marches leaving the extent STOP rather than clamping: a clamped tap re-reads the
                            // border texel at every remaining step and manufactures a horizon the relief has not.
                            if (sx < 0 || sx >= width || sy < 0 || sy >= height) { break; }
                            // Tangent is a MILLIMETRE rise over the PLANE's own run — its lattice cell size,
                            // millimetres per texel where a pitched affine seats one and texel units otherwise —
                            // so the same relief at two resolutions casts one horizon.
                            double rise = (Tap(staging, plane, sx, sy, width, height, lanes) - centre) * evidence.ScaleMm;
                            horizon = Math.Max(horizon, rise / source.RunMm(step));
                        }
                        // sin²θ of the horizon angle is the cosine-weighted fraction that azimuth's slice
                        // occludes — ∫₀^θ sin·cos over the slice — so the visible fraction is 1/(1+tan²θ) and no
                        // arctangent is evaluated per step.
                        open += 1.0 / (1.0 + (horizon * horizon));
                    }
                    visibility.Span[(((layer * height) + y) * width) + x] = open / rays;
                }
            }
        }
        Deposit(destination, visibility.Span);
        return evidence with { Mean = mean / (width * (double)height * layers) };
    }

    // Curvature is ONE eigenvalue projection over the kernel's own lattice Hessian, so the four measures are four
    // reads of one pair rather than four kernels and no second-difference stencil is spelled here. The kernel arm is
    // total, non-Fin, allocation-free, border-REFLECTED, and CellSize-scaled — reflection is the exact
    // zero-normal-derivative mirror, which is the same Neumann boundary the bounded height solver assembles, so the
    // two kernels agree at the border rather than by one texel of relief. A height plane is single-lane by its own
    // Project gate, so the layer band is a slice offset and the grid addresses the slice directly. Eigenvalue
    // projection, the evidence.ScaleMm physical scaling, and the PlaneRange.Signed packing stay this page's.
    private static HeightEvidence Curve(
        ReadOnlySpan<double> staging, TexturePlane source, TexturePlane destination, HeightDerivative.Curvature curvature, HeightEvidence evidence) {
        int width = source.Width.Value, height = source.Height.Value, layers = source.Layers.Value;
        using MemoryOwner<double> field = MemoryOwner<double>.Allocate(width * height * layers);
        double mean = 0.0;
        for (int layer = 0; layer < layers; layer++) {
            ReadOnlySpan<double> slice = staging.Slice(layer * width * height, width * height);
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    mean += slice[(y * width) + x];
                    (double xx, double xy, _, double yy, _, _) =
                        Nabla.LatticeHessianAt(values: slice, grid: source.Grid, column: x, row: y);
                    // Eigenvalues of a 2x2 Hessian are the half-trace with and without the root of the
                    // half-trace squared less the determinant, so mean, Gaussian, and both principal extrema read
                    // off ONE decomposition rather than three further stencil passes.
                    double half = (xx + yy) * 0.5;
                    double gap = Math.Sqrt(Math.Max(0.0, (half * half) - ((xx * yy) - (xy * xy))));
                    double signed = curvature.Measure switch {
                        CurvatureMeasure.Mean => half,
                        CurvatureMeasure.Gaussian => (xx * yy) - (xy * xy),
                        CurvatureMeasure.PrincipalMaximum => half + gap,
                        _ => half - gap,
                    };
                    // Kernel stencils already divide by the lattice cell squared, so a declared pitch makes the
                    // measure physical through the plane's own affine and the millimetre amplitude is all this page
                    // still applies; an identity affine leaves the measure texel-relative under the bound stated at the
                    // evidence.
                    field.Span[(((layer * height) + y) * width) + x] = Math.Clamp(signed * evidence.ScaleMm, -1.0, 1.0);
                }
            }
        }
        Deposit(destination, field.Span);
        return evidence with { Mean = mean / (width * (double)height * layers) };
    }

    // Tap is the ONE bordered read the occlusion march shares with the height forward direction: a tap outside the extent
    // CLAMPS, which the march uses only to stay in bounds before its own extent test stops it. The plane offset is the
    // layer's own base, so a layered plane's faces never fold into one another.
    internal static double Tap(ReadOnlySpan<double> staging, int plane, int x, int y, int width, int height, int lanes) =>
        staging[plane + ((((Math.Clamp(y, 0, height - 1) * width) + Math.Clamp(x, 0, width - 1)) * lanes))];

    // Match runs the histogram. Source distribution gathers over the WHOLE plane into op.Bins buckets before the
    // first texel maps, which is exactly why this curve is a global stage and cannot fuse into a row pass. The map
    // is the classical CDF composition: a texel's own quantile under the source distribution is looked up in the
    // TARGET's inverse, and the target CDF is read by LINEAR interpolation between its bracketing entries so a
    // coarse target does not quantize the result into visible steps. Alpha is untouched — a tonal match over
    // coverage moves every edge.
    private static void Match(TexturePlane source, TexturePlane destination, RemapCurve curve) {
        if (curve is not RemapCurve.Histogram histogram || histogram.TargetCdf.IsEmpty) { return; }
        int width = source.Width.Value, height = source.Height.Value, lanes = source.Lanes;
        int colour = source.Alpha.Carries ? Math.Max(1, lanes - 1) : lanes;
        int bins = Math.Max(2, histogram.Bins);
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(width * height * source.Layers.Value * lanes);
        Materialize(source, staging.Span);
        using SpanOwner<double> cdf = SpanOwner<double>.Allocate(bins);
        cdf.Span.Clear();
        long counted = 0;
        for (int at = 0; at < staging.Length; at++) {
            if (lanes > 1 && (at % lanes) >= colour) { continue; }
            cdf.Span[Math.Clamp((int)(staging.Span[at] * (bins - 1)), 0, bins - 1)] += 1.0;
            counted++;
        }
        for (int bin = 1; bin < bins; bin++) { cdf.Span[bin] += cdf.Span[bin - 1]; }
        for (int bin = 0; bin < bins; bin++) { cdf.Span[bin] = counted > 0 ? cdf.Span[bin] / counted : 0.0; }
        double[] target = histogram.TargetCdf.ToArray();
        for (int at = 0; at < staging.Length; at++) {
            if (lanes > 1 && (at % lanes) >= colour) { continue; }
            staging.Span[at] = Invert(target, cdf.Span[Math.Clamp((int)(staging.Span[at] * (bins - 1)), 0, bins - 1)]);
        }
        Deposit(destination, staging.Span);
    }

    // Invert reads the target's INVERSE CDF at a quantile, linearly interpolated between the bracketing entries so a coarse
    // target distribution does not stair-step the matched plane.
    private static double Invert(ReadOnlySpan<double> target, double quantile) {
        for (int bin = 0; bin < target.Length; bin++) {
            if (target[bin] < quantile) { continue; }
            double lower = bin > 0 ? target[bin - 1] : 0.0;
            double span = target[bin] - lower;
            double within = span > 0.0 ? (quantile - lower) / span : 0.0;
            return Math.Clamp((bin + within) / Math.Max(1, target.Length - 1), 0.0, 1.0);
        }
        return 1.0;
    }
}
```

## [04]-[HEIGHT_FIELD]

- Owner: `HeightEvidence` the correspondence carrier; `HeightPolicy` the bounded arm's extent policy; `HeightSolver` the integration routes; the occlusion and curvature derivative kernels.
- Law: `HeightEvidence` is what makes the `HeightNormal` inverse honest: a gradient field determines a height field only up to an additive constant and, at a normalizing depth, only up to the scale the forward normalization consumed — so the forward RECORDS the millimetre amplitude, the field mean, and the convention it read, and the inverse consumes them. Invoked with no evidence, that inverse rails rather than reconstructing a plausible field, because a fabricated amplitude is a displacement that renders confidently and wrongly. SPATIAL grain is NOT an evidence column: it is the plane's own `CellLattice` affine, read through `TexturePlane.RunMm`, so a horizon angle, a curvature magnitude, and a gradient slope all divide a millimetre rise by the run the plane they differentiate declares — an identity affine leaves that run in texel units and states the resolution-relative bound honestly, while a pitched one makes every derivative physical without a second carrier to keep in step.
- Law: `HeightPolicy` is the bounded arm's own extent policy as DATA, for the reason `tile#TILE_SYNTH` makes every threshold a column: a constant inside a kernel is a knob no caller turns and no key records, and a Krylov run stopping at one thousand iterations lands a different field than one stopping at four thousand. Every column enters `PlaneOp.Digest` whole, so two presses of one plan under two ceilings key distinctly.
- Law: `HeightSolver` is chosen by PERIODICITY, not by preference. `Spectral` is the Frankot-Chellappa least-squares integration in the frequency domain and it assumes a periodic domain — which is exactly true of a tiled plane and exactly false of a bounded one, where it wraps the opposite edge's gradient into the solution. `Poisson` assembles the five-point Laplacian with Neumann boundaries, which is correct on a bounded plane and needlessly expensive on a tiled one. Its `Periodic` column carries the choice, so a caller states the plane's own nature rather than a solver name; the direct-versus-iterative split INSIDE the bounded arm is the arm's own `HeightPolicy` — the kernel `CholeskySparse` exact factor to the policy's unknown ceiling, the MathNet `MILU0Preconditioner`-under-`BiCgStab` Krylov lane above it, one operator assembly serving both — never a third row a caller could mis-pick.
- Law: the spectral route runs entirely on the composed transform: forward the gradient divergence, divide by the frequency-squared kernel with the zero bin held at zero (the additive constant the evidence's mean then restores), and inverse. Every buffer is caller-owned and the transform mutates it in place under the symmetric scaling that makes the forward-inverse round trip an identity.
- Law: the bounded route assembles by TRIPLET ACCUMULATION and factors through the kernel's own SPD cache. Duplicate triplets sum and zeros drop at admission, so the Laplacian assembles by accumulation and never by hand-built compressed storage, and the factor carries the pivot-loss refusal onto the typed rail rather than as a bare exception.
- Law: occlusion casts a LOW-DISCREPANCY azimuth set decorrelated per texel. That set is the exactly-computable uniform fan, and the per-texel rotation comes from the kernel `Deterministic.UnitInterval` COORDINATE draw keyed by the derivative's own seed — never a sequential stream — so a band partition cannot reorder a draw, a re-derived plane is byte-identical, and the low-frequency banding a shared direction set leaves on a flat wall breaks. Kernel `SampleKind` spectrum owns SET draws — its `ExtractionDomain` lattice case reaches a texel grid — yet this cast stays coordinate-keyed by design: a drawn set is partition-orderable and a re-derived plane must be byte-identical, which only the stateless coordinate draw guarantees.
- Law: the gradient and the Hessian are both the kernel `Numerics/calculus#NABLA` lattice-stencil arm over the plane's own `CellLattice`, so this page spells no finite difference of its own. That arm is total, non-`Fin`, allocation-free, `CellSize`-scaled, and border-REFLECTED — reflection is the exact zero-normal-derivative mirror, which is the Neumann boundary the bounded solver assembles, so the forward gradient, the curvature Hessian, and the assembled Laplacian all state one boundary condition. What stays this page's is the semantics: the surface-normal composition and its convention flip, the eigenvalue projection, the `evidence.ScaleMm` millimetre amplitude, and the `PlaneRange.Signed` packing.
- Law: the bounded route assembles the NEGATED Laplacian — positive diagonal, negative couplings, right-hand side negated to match — because Cholesky demands positive definiteness and the raw ∇² orientation is negative-semidefinite. Its last row PINS to the identity with its couplings eliminated SYMMETRICALLY (a one-sided pin leaves an asymmetric matrix no factor admits): a pure Neumann Laplacian carries the constant vector in its null space, so no factor exists for it unpinned, and the pinned gauge is exactly what the reconstruction's mean restores afterward.
- Packages: MathNet.Numerics (composed — `IntegralTransforms.Fourier.Forward(Complex[], FourierOptions)` and `Inverse(Complex[], FourierOptions)` as the ROW-COLUMN 2D fold over the row-major gradient staging — the `Forward2D`/`Inverse2D` multidim rows route to the provider seam whose MANAGED realization throws `NotSupportedException`, so the 1D pair is the platform-total form — `FourierOptions.Default` the symmetric scaling that makes the per-axis fold the 2D transform and the round trip an identity), `Rasm.Numerics` (composed — `Nabla.LatticeGradientAt`/`LatticeHessianAt` the ONE grid stencil, `CellLattice` the plane's own grid, `SparseMatrix.FromTriplets` the accumulating assembly, `SparsePreconditioner.Milu0` and `SolveIterativeDetailed` the no-fallback Krylov rail, `CholeskySparse.Of`/`Solve` the SPD factor cache carrying the pivot-loss refusal, `Dimension`), `plane#TEXTURE_PLANE` (composed — `TexturePlane.RunMm`/`Grid`/`Read`/`Write`), `Rasm.Domain` (composed — `Deterministic.UnitInterval(Point3d, int, int)` the ONE replayable coordinate-keyed draw, `Op`), CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate` the solver staging, `SpanOwner<T>.Allocate` the row rental), BCL inbox (`System.Numerics.Complex`, `INumberBase<T>` the one divergence body over two stagings).
- Growth: a new integration route is one `HeightSolver` row with one solve arm; a new derived field is one `HeightDerivative` case; a new curvature measure is one enum row the eigenvalue projection reads; a new stop axis is one `HeightPolicy` column that enters the digest by construction.
- Boundary: this section derives fields from a height plane and never SOURCES one. Height planes arrive from an ingest classification, a press bake, or the `HeightNormal` inverse over an acquired normal plane under a depth prior — and no inference stage emits height, because integration under a prior is pure mathematics the estate owns rather than a model it would have to license.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                       // CultureInfo — the invariant Digest spelling
using System.Numerics;                            // Complex, INumberBase — the one divergence body over two stagings
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using MathNet.Numerics.IntegralTransforms;        // Fourier, FourierOptions
using Rasm.Domain;                                // Op, Deterministic
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using Rasm.Numerics;                              // Nabla, CellLattice, SparseMatrix, CholeskySparse, Dimension
using Rhino.Geometry;                             // Vector3d — the lattice gradient's own carrier
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// HeightSolver rows the integration route, chosen by the PLANE's periodicity rather than by a solver preference. Spectral
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
// HeightPolicy carries the bounded arm's own extent policy as DATA. Constants inside a kernel are knobs no caller turns and no key
// records, and a Krylov stop moves the produced bytes. DirectCeiling is the unknown count at which the exact
// factor's 2D nested-dissection fill makes the factor the memory bound; the Standard row seats a 2048-square plane
// on the factor and a 4k plane on the Krylov lane.
public readonly record struct HeightPolicy(int DirectCeiling, int KrylovIterations, double KrylovTolerance) {
    public static readonly HeightPolicy Standard = new(DirectCeiling: 1 << 22, KrylovIterations: 1000, KrylovTolerance: 1e-9);
    public string Digest =>
        string.Create(CultureInfo.InvariantCulture, $"{DirectCeiling}|{KrylovIterations}|{KrylovTolerance:R}");
}

// What the forward direction DESTROYED and the inverse must be handed back: the millimetre amplitude the normalized
// [0,1] field was measured against, the mean the integration's free constant restores, the convention the gradient
// read, and the reconstruction-fit residual the inverse reports. The SPATIAL grain is deliberately absent —
// millimetres per texel is the plane's own CellLattice affine, read through TexturePlane.RunMm, so a derivative
// divides its rise by the run of the plane it differentiates and no second carrier can drift out of step with it.
// Unit is the identity evidence a channel row carries where the set supplies the real amplitude at bind time.
public readonly record struct HeightEvidence(double ScaleMm, double Mean, NormalConvention Convention, double Residual) {
    public static readonly HeightEvidence Unit = new(ScaleMm: 1.0, Mean: 0.5, NormalConvention.Gl, Residual: 0.0);
    public HeightEvidence With(double residual) => this with { Residual = residual };

    // Residual is REPORTED evidence, never a preimage column: keying on a prior run's fit residual re-keys an
    // identical operation by its own measurement noise.
    public string Digest =>
        string.Create(CultureInfo.InvariantCulture, $"{ScaleMm:R}|{Mean:R}|{Convention.Key}");
}

// --- [OPERATIONS] --------------------------------------------------------------------------
internal static class HeightField {
    // ToNormal runs the FORWARD direction: the kernel lattice gradient of the height field, scaled by its own millimetre
    // amplitude, composed into a unit tangent-space normal under the declared convention. It reads the STAGED
    // height run its caller already materialized — the bordered pass stages once and every kernel in the pass
    // reads that one run — and records the evidence the inverse consumes, so the round trip is a correspondence
    // rather than two unrelated transforms. A height plane is single-lane by the op's own Project gate, so the
    // layer band is a slice offset and the plane's grid addresses that slice directly.
    internal static HeightEvidence ToNormal(ReadOnlySpan<double> field, TexturePlane height, TexturePlane normal, HeightEvidence evidence, Op key) {
        int width = height.Width.Value, extent = height.Height.Value;
        using SpanOwner<double> row = SpanOwner<double>.Allocate(normal.RowScalars);
        double mean = 0.0;
        for (int layer = 0; layer < height.Layers.Value; layer++) {
            ReadOnlySpan<double> slice = field.Slice(layer * width * extent, width * extent);
            for (int y = 0; y < extent; y++) {
                for (int x = 0; x < width; x++) {
                    mean += slice[(y * width) + x];
                    // Kernel stencils already divide by the lattice cell, so the slope IS the height gradient
                    // once the millimetre amplitude applies; the surface normal is then the cross product of the
                    // two tangents (1,0,dx) and (0,1,dy), which is exactly (-dx, -dy, 1) before normalization.
                    Vector3d slope = Nabla.LatticeGradientAt(values: slice, grid: height.Grid, column: x, row: y);
                    double dx = slope.X * evidence.ScaleMm, dy = slope.Y * evidence.ScaleMm;
                    double length = Math.Sqrt((dx * dx) + (dy * dy) + 1.0);
                    int at = x * normal.Lanes;
                    row.Span[at] = -dx / length;
                    row.Span[at + 1] = -dy * evidence.Convention.GreenSign / length;
                    row.Span[at + 2] = 1.0 / length;
                    for (int lane = 3; lane < normal.Lanes; lane++) { row.Span[at + lane] = 1.0; }
                }
                normal.Write(y, layer, row.Span);
            }
        }
        // Forward direction is what DESTROYS the constant; recording the measured mean here is what lets the
        // inverse restore it rather than centring on whatever offset a solve happened to land.
        return evidence with { Mean = mean / (width * (double)extent * height.Layers.Value) };
    }

    // ToHeight runs the INVERSE: least-squares integration of the gradient field. Spectral divides the divergence by the
    // frequency-squared kernel with the zero bin HELD at zero — that bin is exactly the additive constant integration
    // cannot recover, so zeroing it and restoring the evidence's mean is the honest reconstruction rather than a
    // fabricated offset. The transform mutates the caller-owned buffer under symmetric scaling.
    internal static Fin<HeightEvidence> ToHeight(
        TexturePlane normal, TexturePlane height, HeightSolver solver, HeightEvidence evidence, HeightPolicy policy, Op key) =>
        solver.Periodic ? Spectral(normal, height, evidence, key) : Bounded(normal, height, evidence, policy, key);

    // Spectral rides the route's shared Fin shape — its own body has no failure arm, and the rail exists because
    // ToHeight is ONE entry whose bounded sibling genuinely refuses; a per-route return-type fork would push the
    // solver choice back onto every caller.
    private static Fin<HeightEvidence> Spectral(TexturePlane normal, TexturePlane height, HeightEvidence evidence, Op key) {
        int w = height.Width.Value, h = height.Height.Value;
        using MemoryOwner<Complex> field = MemoryOwner<Complex>.Allocate(w * h);
        Divergence(normal, field.Span, w, h);
        Fourier2(field.Span, w, h, forward: true);
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                double u = 2.0 * Math.PI * (x <= w / 2 ? x : x - w) / w;
                double v = 2.0 * Math.PI * (y <= h / 2 ? y : y - h) / h;
                double denominator = (u * u) + (v * v);
                field.Span[(y * w) + x] = denominator > 0.0 ? field.Span[(y * w) + x] / -denominator : Complex.Zero;
            }
        }
        Fourier2(field.Span, w, h, forward: false);
        using MemoryOwner<double> real = MemoryOwner<double>.Allocate(w * h);
        for (int i = 0; i < real.Length; i++) { real.Span[i] = field.Span[i].Real; }
        return Fin.Succ(Restore(height, real.Span, evidence));
    }

    // Fourier2 folds the 2D transform ROW-COLUMN over the managed 1D kernel — the MEASURED provider truth: the
    // composed Forward2D/Inverse2D pair routes every multidimensional call to
    // FourierTransformControl.Provider.ForwardMultidim, and the MANAGED provider throws NotSupportedException
    // there — the multidim rows run only under a native FFT provider, and no native provider row serves this
    // platform — while the 1D Forward/Inverse pair is managed-complete (Radix-2 at a power of two, Bluestein
    // otherwise). Symmetric scaling composes per axis (1/√w · 1/√h = 1/√(w·h)), so the fold IS the 2D transform
    // under FourierOptions.Default and the round trip stays an identity. Exact-length line stagings feed the
    // whole-array 1D entry; tile#TILE_GATE reads this same fold for its periodicity spectrum.
    internal static void Fourier2(Span<Complex> field, int width, int height, bool forward) {
        Complex[] row = new Complex[width];
        for (int y = 0; y < height; y++) {
            field.Slice(y * width, width).CopyTo(row);
            FourierLine(row, forward);
            row.CopyTo(field.Slice(y * width, width));
        }
        Complex[] column = new Complex[height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) { column[y] = field[(y * width) + x]; }
            FourierLine(column, forward);
            for (int y = 0; y < height; y++) { field[(y * width) + x] = column[y]; }
        }
    }

    private static void FourierLine(Complex[] line, bool forward) {
        if (forward) { Fourier.Forward(line, FourierOptions.Default); }
        else { Fourier.Inverse(line, FourierOptions.Default); }
    }

    // Bounded assembles the five-point Neumann Laplacian by triplet ACCUMULATION — duplicates sum and
    // zeros drop at admission — factored once through the kernel's SPD cache below the policy's ceiling, which
    // lowers the composed solver's bare pivot-loss exception onto the typed rail rather than letting it escape.
    // Direct-vs-iterative is the arm's OWN policy driven by the system's unknown count — never a caller knob and
    // never a third HeightSolver row, because the plane's PERIODICITY is the only nature a caller can state: at or
    // under the ceiling the exact factor is cache-amortized; above it the 2D nested-dissection fill makes the
    // factor the memory bound and the Krylov lane takes the SAME negated pinned operator.
    private static Fin<HeightEvidence> Bounded(TexturePlane normal, TexturePlane height, HeightEvidence evidence, HeightPolicy policy, Op key) {
        Dimension order = Dimension.Create(value: checked((int)height.Texels));
        using MemoryOwner<double> rhs = MemoryOwner<double>.Allocate(order.Value);
        Divergence(normal, rhs.Span, height.Width.Value, height.Height.Value);
        // Assembled operator is the NEGATED Laplacian — the positive-definite form Cholesky demands — so the
        // system is -∇²h = -div and the right-hand side negates to match; a raw ∇² assembly is
        // negative-semidefinite and admits no factor however the gauge is pinned.
        for (int at = 0; at < order.Value; at++) { rhs.Span[at] = -rhs.Span[at]; }
        // Pinned rows carry a ZERO right-hand side, matching the identity row that fixes the gauge: leaving the
        // divergence there would set the pinned texel to an arbitrary curvature value and drag the mean the
        // reconstruction re-centres on with it.
        rhs.Span[order.Value - 1] = 0.0;
        if (order.Value > policy.DirectCeiling) { return Krylov(rhs.Span, height, evidence, policy, key); }
        return from matrix in SparseMatrix.FromTriplets(order, order, Laplacian(height.Width.Value, height.Height.Value), key)
               from factor in CholeskySparse.Of(matrix, key)
               from solved in factor.Solve(new Arr<double>(rhs.Span), key)
               select Restore(height, solved.AsSpan(), evidence);
    }

    // Krylov is the LARGE-EXTENT arm and rides the kernel's OWN preconditioned rail: the SAME FromTriplets assembly the
    // direct arm uses (duplicates sum at admission — one operator, two solve routes), the
    // SparsePreconditioner.Milu0 row (modified row-sum-preserving incomplete LU, exactly the elliptic
    // preconditioner the Neumann Laplacian wants), and SolveIterativeDetailed's no-fallback contract — a
    // non-converged run reports IterativeExhausted with the true residual, and this page rails it by name rather
    // than letting the kernel's dense-fallback route densify millions of unknowns. A second CSR assembly on the
    // MathNet plane beside the kernel's is the deleted form.
    private static Fin<HeightEvidence> Krylov(ReadOnlySpan<double> rhs, TexturePlane height, HeightEvidence evidence, HeightPolicy policy, Op key) {
        Dimension order = Dimension.Create(rhs.Length);
        Arr<double> source = new(rhs);
        return SparseMatrix.FromTriplets(order, order, Laplacian(height.Width.Value, height.Height.Value), key)
            .Bind(matrix => matrix.SolveIterativeDetailed(source, SparsePreconditioner.Milu0,
                policy.KrylovTolerance, policy.KrylovIterations, key))
            .Bind(receipt => receipt.Stop.IsUsable
                ? Fin.Succ(Restore(height, receipt.Solution.AsSpan(), evidence))
                : MaterialFault.Parameter(key, $"<height-krylov:{receipt.Stop.Key}:{receipt.Residual:R}>"));
    }

    // Divergence is what the integration inverts, over the staged normal field: each texel's (-nx/nz, -ny/nz)
    // slope pair differentiated once more, so the right-hand side is the Laplacian of the height field being
    // sought. ONE slope-pair accumulation, written into whichever staging the route rents. The generic constraint
    // is the point: the spectral route needs Complex cells and the bounded route needs double cells, and the
    // divergence they invert is one body — a second transcription is where a sign flips in exactly one of them and
    // only the periodic plane shows it. Border slopes clamp, which is the one-sided difference the assembled
    // Laplacian's own omitted-neighbour boundary row expects.
    private static void Divergence<T>(TexturePlane normal, Span<T> field, int width, int height) where T : INumberBase<T> {
        int lanes = normal.Lanes;
        using MemoryOwner<double> staged = MemoryOwner<double>.Allocate(width * height * lanes);
        PlaneKernel.Materialize(normal, staged.Span);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                // p = -nx/nz and q = -ny/nz are the height gradients the unit normal encodes; the divergence of
                // that pair IS the Laplacian of the height field the solve seeks. A degenerate nz would divide by
                // zero on a normal the plane admitted, so the floor keeps the whole fold total.
                double px = Slope(staged.Span, x + 1, y, width, height, lanes).P;
                double mx = Slope(staged.Span, x - 1, y, width, height, lanes).P;
                double qy = Slope(staged.Span, x, y + 1, width, height, lanes).Q;
                double ry = Slope(staged.Span, x, y - 1, width, height, lanes).Q;
                field[(y * width) + x] = T.CreateChecked(((px - mx) + (qy - ry)) * 0.5);
            }
        }
    }

    private static (double P, double Q) Slope(ReadOnlySpan<double> staged, int x, int y, int width, int height, int lanes) {
        int at = ((Math.Clamp(y, 0, height - 1) * width) + Math.Clamp(x, 0, width - 1)) * lanes;
        double nz = staged[at + 2];
        double floor = Math.Abs(nz) < 1e-6 ? Math.CopySign(1e-6, nz == 0.0 ? 1.0 : nz) : nz;
        return (-staged[at] / floor, -staged[at + 1] / floor);
    }

    // Laplacian streams the NEGATED five-point Neumann operator as TRIPLETS: positive count on the diagonal, minus one per
    // kept neighbour — the positive-definite orientation a Cholesky factor demands, with the right-hand side
    // negated to match at the one assembly site. Duplicates sum and zeros drop at admission, so the boundary rows
    // assemble by accumulation and a hand-built compressed storage never appears; a boundary row simply omits its
    // absent neighbours and carries the count of the ones it kept, which IS the zero normal derivative the kernel
    // stencil's own reflection states on the forward side. The last row is PINNED to the identity so the operator
    // is non-singular: a pure Neumann Laplacian has the constant vector in its null space and no factor exists for
    // it, and the constant is exactly what the evidence's mean restores afterward.
    private static IEnumerable<(int Row, int Col, double Value)> Laplacian(int width, int height) {
        int order = width * height, pinned = order - 1;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int row = (y * width) + x;
                if (row == pinned) { yield return (row, row, 1.0); continue; }
                int kept = 0;
                // Pinning eliminates SYMMETRICALLY: a neighbour of the pinned texel keeps its diagonal count but
                // emits no coupling, because a one-sided pin leaves an asymmetric matrix no Cholesky factor admits.
                foreach (int neighbour in Neighbours(x, y, width, height)) {
                    if (neighbour != pinned) { yield return (row, neighbour, -1.0); }
                    kept++;
                }
                yield return (row, row, kept);
            }
        }
    }

    private static IEnumerable<int> Neighbours(int x, int y, int width, int height) {
        int row = (y * width) + x;
        if (x > 0) { yield return row - 1; }
        if (x < width - 1) { yield return row + 1; }
        if (y > 0) { yield return row - width; }
        if (y < height - 1) { yield return row + width; }
    }

    // Integration recovers the field up to an additive constant, so the reconstruction re-centres on the
    // evidence's recorded mean rather than on whatever offset the solve happened to land. The residual it reports
    // is the RECONSTRUCTION FIT — the largest excursion the re-centred field made outside the normalized span,
    // relative to the field's own spread — which is the one signal both routes can measure at one owner: the
    // spectral route consumes its right-hand side in place inside the transform, so no linear residual survives to
    // this seam, and a column only one route could fill would read as zero on the other.
    private static HeightEvidence Restore(TexturePlane height, ReadOnlySpan<double> solved, HeightEvidence evidence) {
        int width = height.Width.Value;
        double low = double.PositiveInfinity, high = double.NegativeInfinity, sum = 0.0;
        for (int at = 0; at < solved.Length; at++) {
            low = Math.Min(low, solved[at]);
            high = Math.Max(high, solved[at]);
            sum += solved[at];
        }
        double offset = evidence.Mean - (sum / Math.Max(1, solved.Length));
        double spread = Math.Max(1e-12, high - low);
        using SpanOwner<double> row = SpanOwner<double>.Allocate(height.RowScalars);
        double residual = 0.0;
        for (int layer = 0, at = 0; layer < height.Layers.Value; layer++) {
            for (int y = 0; y < height.Height.Value; y++) {
                for (int x = 0; x < width; x++, at++) {
                    double value = solved[at] + offset;
                    double clamped = Math.Clamp(value, 0.0, 1.0);
                    residual = Math.Max(residual, Math.Abs(value - clamped) / spread);
                    for (int lane = 0; lane < height.Lanes; lane++) { row.Span[(x * height.Lanes) + lane] = clamped; }
                }
                height.Write(y, layer, row.Span);
            }
        }
        return evidence.With(residual);
    }
}
```

## [05]-[RESEARCH]

(none)
