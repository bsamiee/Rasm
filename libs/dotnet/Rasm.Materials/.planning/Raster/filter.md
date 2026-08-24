# [MATERIALS_FILTER]

THE DECODED-PLANE TRANSFORM ALGEBRA. One `PlaneOp` `[Union]` closes the per-plane transform family — resample, convolve, height correspondence, height derivative, coverage dilation, tonal remap, and lane swizzle — under ONE `Apply` entry that PLANS every shape before it rents an output, SCHEDULES the sequence into stages by each op's own dependency class, and folds each stage over the plane's decoded row rails. `PlaneOp` holds a transform as a case, `ConvolveKernel` a convolution as a case, `RemapCurve` a tonal curve as a case, `HeightDerivative` a height derivative as a case, `HeightPolicy` a solver stop as a row, and `SwizzleLane` a lane projection as a row — never a per-transform entrypoint, a per-curve method, a constant inside a kernel, or a boolean selecting between two bodies.

Scheduling is the page's load-bearing decision, and it exists because the ops genuinely differ in what they can see. `Levels` remaps one texel; a Gaussian reads a neighbourhood; a histogram remap must see the WHOLE plane before it can map its first texel; a normal-to-height integration is a global spectral or sparse-linear solve; a resample changes the grid itself. Fusing that mixture into one per-row pass is a fiction — a row action cannot supply a neighbour it has not written, a plane statistic it has not gathered, or a grid it has not resized — so each case declares its `StageKind` and the scheduler fuses only what genuinely fuses: consecutive pointwise ops collapse into ONE row pass, a neighbourhood op takes a bordered two-buffer pass under its own `EdgeMode`, and a global op takes a whole-plane pass. Adding an op therefore cannot corrupt its neighbours, and adding a dependency class is one row on the scheduler. `PlaneOp` composes the `plane#TEXTURE_PLANE` typed arena with its decoded `Read`/`Write` row rails, its `CellLattice` grid, its `Run` spatial grain, and its `PlaneFormat.For` retyping, the `codec#RASTER_FAULT` band-2460 rail for nothing at all (every refusal here is a SHAPE refusal on band 2450), TinyEXR.NET `ImageProcessing`/`Lut3D` for every separable resample, transfer, and LUT fold, the kernel `Nabla` lattice-stencil arm for every grid derivative, the kernel `WeightKernelFamily` for every Gaussian weight, the kernel `TapSeries` tap fold for every separable axis pass, the kernel `SpectralArena` transform band for the spectral integration, the kernel `SparseMatrix`/`CholeskySparse` for the bounded Poisson solve, the kernel `Deterministic` coordinate-keyed draw for the occlusion cast's per-texel rotation, and `CommunityToolkit.HighPerformance` `ParallelHelper` over `struct IAction` partitions — re-minting no resampler, no transform, no tap fold, no stencil, no weight profile, no factorization, and no random source.

## [01]-[INDEX]

- [02]-[PLANE_OP]: `StageKind` axes the dependency, `ConvolveKernel`/`RemapCurve`/`HeightDerivative` family the cases, `SwizzleLane` rosters the projections, and the seven-case `PlaneOp` union projects every shape totally.
- [03]-[PLANE_STAGE]: `PlaneOp.Apply` plans, schedules, and runs — fusing the pointwise run, bordering the neighbourhood pass, and publishing `PlaneReceipt` evidence.
- [04]-[HEIGHT_FIELD]: `HeightEvidence` carries the correspondence, `HeightPolicy` carries the stop, `HeightSolver` routes the spectral and bounded integrations, and the occlusion and curvature derivatives read the height field.

## [02]-[PLANE_OP]

- Owner: `PlaneOp` the transform family; `StageKind` the dependency axis each case declares; `ConvolveKernel` the neighbourhood-kernel family carrying its own bandwidth correspondence and the two admitted `PositiveMagnitude` supports it publishes; `RemapCurve` the tonal-curve family; `HeightDerivative` the height-derived-field family; `SwizzleLane` the lane-projection roster; `PlaneShape` the projected shape carrier.
- Cases: op {`Resize`, `Convolve`, `HeightNormal`, `FromHeight`, `Dilate`, `Remap`, `Swizzle`} · stage {`pointwise`, `neighbourhood`, `global`} · kernel {`Gaussian`, `UnsharpMask`, `Bilateral`, `Median`} · curve {`Levels`, `Histogram`, `Lut`} · derivative {`Occlusion`, `Curvature`, each declaring its produced `PlaneRange`} · lane {`r`, `g`, `b`, `a`, `zero`, `one`, `rInverse`, `gInverse`, `bInverse`, `aInverse`, `gNegate`}.
- Law: `HeightNormal` carries BOTH directions of one correspondence on one case. `Inverse` is the column carrying direction, because a height field and a tangent-space normal field are the forward and inverse of a single relation — never a `NormalFromHeight`/`HeightFromNormal` sibling pair. `HeightEvidence` crosses from the forward to the inverse — millimetre amplitude, field mean, and the convention the forward recorded — because integration recovers a gradient field's shape and never its absolute offset or amplitude, and an inverse whose ingress is raw samples re-shaped into the forward's input domain fabricates exactly what the forward destroyed. `HeightPolicy` rides beside it because a Krylov stop MOVES the produced bytes, so both enter the `Digest` preimage whole.
- Law: `SwizzleLane` is DATA — a source index, a scale, and a bias — so lane reordering, lane inversion, constant fill, and the `dx`→`gl` green flip are all one kernel over a row of rows. `SwizzleLane.FlipGreen` is exactly the `plane#PLANE_VOCABULARY` `NormalConvention` conversion and mints no second operation, so the corpus has one green-flip site rather than a conversion pair beside a swizzle. It spells `GNegate` and NOT `GInverse`: the lanes it folds are DECODED and signed, so the flip is a negation, where `1 − g` is the unit-range complement that maps `+1` to `0` and tilts every texel of the plane it was meant to correct. The two rows coexist because both arithmetics are real — a mask inverts, a normal negates — and naming them apart is what keeps a caller from reaching for the wrong one.
- Law: `Remap` closes the tonal family on one case. `RemapCurve.Levels.Invert` is a ROW of the levels case — black at one, white at zero — so the `roughness = 1 − gloss` ingest inversion, a contrast stretch, and a gamma lift are one curve family rather than an `Invert` op beside a `Levels` op. Every curve evaluates in the LINEAR domain over decoded lanes, which is what makes an `srgb`-authored gloss plane invert correctly rather than forking the roughness silently. The VALUE AXIS IS UNBOUNDED at both ends on every curve, the same law the order statistic holds: no curve clamps to `[0,1]`, so an HDR plane keeps its headroom and a `Signed` plane its negative half, and the gamma takes the ODD extension — magnitude raised, sign restored — because the exponential answers `NaN` on a negative and one `NaN` texel poisons every fold below it. A display clamp belongs at `surface#TONE_MAP`, the one owner that knows a reference white.
- Law: `Dilate` fills a plane's UNWRITTEN texels from their nearest written neighbours, ring by ring, which is what makes a chart-packed or atlased plane survive its own mip chain: a bilinear tap straddling a chart boundary otherwise reads the neutral, and every level halves that bleed further into the shaded surface. Coverage IS the alpha lane, so the op needs no second carrier and no unwritten-texel sentinel — a plane carrying no coverage REFUSES at `Project`, because inferring emptiness from a zero texel would dilate every legitimately black region into its neighbours.
- Law: BANDWIDTH IS A ROW COLUMN and it publishes as an ADMITTED `PositiveMagnitude`. The kernel weight profile now refuses a non-positive support outright, so the support resolves ONCE at the row — from the probe that recovers the profile's own private bandwidth constant — and every tap table and range weight reads an admitted value rather than flooring a raw sigma at a call site. The ordered row publishes ABSENCE on both support columns, where the deleted `Sigma` column answered a FORGED ZERO for a kernel that carries no bandwidth at all and let a caller read that zero as one.
- Law: `Project` is TOTAL and runs before any rental. It folds the whole sequence into a final `PlaneShape` AND it is the page's one parameter admission: the convolve arm gates its kernel's scalar fields through `ConvolveKernel.Admitted` and the two declared-edge cases cross the `EdgeMode` defined-value gate, so a shape or parameter refusal anywhere in the chain leaves the source untouched and costs nothing — a mid-chain refusal after three rentals is the failure mode the plan-first order forecloses, and a kernel body below never sees an unadmitted sigma or an undefined edge integral. Retyping resolves through `PlaneFormat.For`, so a lane-count change lands on the storage row the semantic count rounds up to and never on a fabricated format.
- Law: shape refusals rail `MaterialFault.Parameter` on band 2450. This page reaches band 2460 nowhere: a filter has no container, no device, and no synthesizer, so a `RasterFault` here would be a shape refusal wearing a mechanical code.
- Entry: `PlaneOp.Apply(TexturePlane source, Seq<PlaneOp> ops, Op key, Option<TimeProvider> clock = default, BakeGovernance governance = default)` is the ONE entry over every arity — an empty sequence returns the source with an empty receipt, a single op and a chain take the identical path, and no `ApplyOne`/`ApplyMany` pair exists; the clock rides so the receipt's elapsed is measured, the governance carrier rides so a chain over a sixteen-million-texel plane is abortable and watchable, and `press#TEXTURE_PRESS` threads both of its own. `PlaneOp.Digest` is the ONE canonical per-op spelling a content-key preimage folds — `press#PRESS_PLAN` pieces its post chains through it, and a consumer spelling an op through `ToString` re-keys on the next case rename.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`Layer`/`Grid`/`Run`, `PlaneFormat.For`, `PlaneTransfer`, `AlphaMode`, `PlaneRange`, `NormalConvention`), TinyEXR.NET (composed — `ResizeFilter`/`EdgeMode` the resample vocabulary, `Lut3D.TryParseCube`/`Apply` the `.cube` curve), `Rasm.Numerics` (composed — `WeightKernelFamily.Gaussian.Weight` the ONE Gaussian profile, `Dimension`, `UnitInterval`), `Rasm.Domain` (`Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Law: `OrderStatistics` is this folder's ONE empirical-quantile owner and it holds a distribution as SORTED SAMPLES: the histogram remap here and the `tile#TILE_SYNTH` Gaussian blend read the same forward quantile and the same interpolated inverse, and a second transcription drifts on the plotting position, the value-axis range, and the tail. The value axis is unbounded at both ends, so an HDR plane keeps its headroom and a `Signed` plane its negative half, where the unit-range binned form quantized the float substrate to its bin count and collapsed both tails. The plotting position is HAZEN, `(i + 0.5)/n`, because the Gaussian-space composition needs the symmetric position that keeps the extremes finite under `Normal.InvCDF`; the kernel `Distribution.Of` R-7 convention is a DIFFERENT owner answering a reported-percentile question and deliberately pinning the extremes at 0 and 1, so the divergence is two correct answers to two questions rather than one drift to reconcile.
- Law: an arm the scheduler makes UNREACHABLE copies the source through. Every non-fusing stage rents its destination from the pool, so an arm returning without writing publishes the previous tenant's bytes as a plane; renting `AllocationMode.Clear` everywhere is the alternative and pays a full clear on every stage to cover arms that never run. The fused pass's tail arm holds the same discipline by writing every lane it read.
- Growth: a new transform is one `PlaneOp` case declaring its `StageKind` with one `Project` arm, one `Digest` arm, and one kernel arm — the scheduler, the receipt, and every consumer are untouched. Every new curve is one `RemapCurve` case, a new derived field one `HeightDerivative` case, a new lane projection one `SwizzleLane` row. Every new convolution is one `ConvolveKernel` case AND its dispatch COLUMNS — `Separable`, `Sharpen`, `Ordered`, `Support`, `RangeSupport`, its `Admitted` arm — so every kernel body reads the row and no site re-tests a case: `Gaussian` and `UnsharpMask` are separable and take the axis-pass pair, `Bilateral` and `Median` are not — a range weight and an order statistic each break the product — so both take a square-window body under the SAME `EdgeMode` addressing rather than a second edge law.
- Boundary: this page transforms DECODED planes and decides nothing about what a plane MEANS. Channel semantics, neutrals, packing, and mip law are `set#TEXTURE_CHANNEL`'s; containers are `codec#RASTER_CODEC`'s; the mip chain is `plane#TEXTURE_PYRAMID`'s and `Resize` is deliberately NOT its alias — a level is the grid's own `Coarsen` step under a declared policy, so a resize can never produce a level a sampler then trilinearly blends against a different filter's neighbours.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Numerics;
using Thinktecture;
using TinyEXR.V3;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// WHAT AN OP CAN SEE. The scheduler's whole law is this axis: a pointwise op fuses with its pointwise neighbours, a
// neighbourhood op needs the previous stage MATERIALIZED plus a border, and a global op needs the whole plane before
// it writes its first texel. The halo a banded neighbourhood pass reserves rides the op, not this axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StageKind {
    public static readonly StageKind Pointwise = new("pointwise", fuses: true);
    public static readonly StageKind Neighbourhood = new("neighbourhood", fuses: false);
    public static readonly StageKind Global = new("global", fuses: false);

    public bool Fuses { get; }
    private StageKind(string key, bool fuses) : this(key) => Fuses = fuses;
}

// ConvolveKernel families the neighbourhood kernels, split by SEPARABILITY. Gaussian and UnsharpMask share one
// separable profile — a sharpen is the source minus its own blur — so ONE axis-pass pair serves both; a range
// weight and an order statistic each break the product, so Bilateral and Median take a square window under the same
// edge addressing. Every dispatch column is a ROW read, never a type test.
[Union]
public abstract partial record ConvolveKernel {
    private ConvolveKernel() { }

    public sealed record Gaussian(double Sigma) : ConvolveKernel;
    public sealed record UnsharpMask(double Sigma, double Amount, double Threshold) : ConvolveKernel;
    // RangeSigma is stated in DECODED units, so it means the same thing whatever depth stores the plane.
    public sealed record Bilateral(double Sigma, double RangeSigma) : ConvolveKernel;
    public sealed record Median(int Radius) : ConvolveKernel;

    public bool Separable => Switch(
        gaussian:    static _ => true,
        unsharpMask: static _ => true,
        bilateral:   static _ => false,
        median:      static _ => false);

    // Sharpen carries the unsharp tail's two numbers as a TYPED ABSENCE, so the separable body applies a halo only
    // where one is declared; Ordered names the order statistic, the one square body that ignores weights entirely.
    public Option<(double Amount, double Threshold)> Sharpen => Switch(
        gaussian:    static _ => Option<(double Amount, double Threshold)>.None,
        unsharpMask: static k => Some((k.Amount, k.Threshold)),
        bilateral:   static _ => Option<(double Amount, double Threshold)>.None,
        median:      static _ => Option<(double Amount, double Threshold)>.None);

    public bool Ordered => Switch(
        gaussian:    static _ => false,
        unsharpMask: static _ => false,
        bilateral:   static _ => false,
        median:      static _ => true);

    // THE BANDWIDTH CORRESPONDENCE, READ off the kernel weight row rather than copied from it. That row evaluates
    // exp(-B(d/s)^2) under a private B, so one probe at a known ratio recovers B and the support making
    // Weight(d, support) equal exp(-d^2/(2 sigma^2)) is sigma*sqrt(2B) — a bandwidth change on the profile moves
    // every tap table and every range weight with no edit here. The support publishes as the POSITIVE MAGNITUDE the
    // profile now demands, so a zero support is unrepresentable rather than a NaN the fold carries downstream; the
    // ordered row publishes ABSENCE on both columns, where the deleted `Sigma` column answered a FORGED ZERO for a
    // kernel that has no bandwidth at all and let a caller read it as one. Create's generated throw is the section's
    // named admission exemption: `Admitted` gates every sigma strictly positive at `Project`, before any read here.
    private static readonly double SupportPerSigma = Math.Sqrt(
        2.0 * -Math.Log(WeightKernelFamily.Gaussian.Weight(distance: 1.0, support: PositiveMagnitude.Create(value: 2.0))) * 4.0);

    private static PositiveMagnitude Supporting(double sigma) => PositiveMagnitude.Create(value: sigma * SupportPerSigma);

    public Option<PositiveMagnitude> Support => Switch(
        gaussian:    static k => Some(Supporting(k.Sigma)),
        unsharpMask: static k => Some(Supporting(k.Sigma)),
        bilateral:   static k => Some(Supporting(k.Sigma)),
        median:      static _ => Option<PositiveMagnitude>.None);

    public Option<PositiveMagnitude> RangeSupport => Switch(
        gaussian:    static _ => Option<PositiveMagnitude>.None,
        unsharpMask: static _ => Option<PositiveMagnitude>.None,
        bilateral:   static k => Some(Supporting(k.RangeSigma)),
        median:      static _ => Option<PositiveMagnitude>.None);

    // Three standard deviations carry over 99.7% of a Gaussian's mass, so a weighted halo truncates there rather
    // than at a caller-supplied radius that silently clips the tail into a visible ring at high sigma; the order
    // statistic carries its own window instead, because a median has no tail to truncate.
    public int Radius => Switch(
        gaussian:    static k => Math.Max(1, (int)Math.Ceiling(3.0 * k.Sigma)),
        unsharpMask: static k => Math.Max(1, (int)Math.Ceiling(3.0 * k.Sigma)),
        bilateral:   static k => Math.Max(1, (int)Math.Ceiling(3.0 * k.Sigma)),
        median:      static k => Math.Max(1, k.Radius));

    // Admission for the kernel's own scalar fields, run once at `Project` so a refused kernel costs no rental and
    // every `Support` read below runs off admitted material. Every sigma is strictly positive because the weight
    // profile is undefined at zero support; Amount and Threshold admit at zero, both lawful unsharp rows.
    public Fin<Unit> Admitted(Op key) => Switch(
        gaussian: k => double.IsFinite(k.Sigma) && k.Sigma > 0.0
            ? Fin.Succ(unit)
            : new MaterialFault.Parameter(key, $"<convolve-sigma:{k.Sigma}>"),
        unsharpMask: k => double.IsFinite(k.Sigma) && k.Sigma > 0.0
                       && double.IsFinite(k.Amount) && k.Amount >= 0.0
                       && double.IsFinite(k.Threshold) && k.Threshold >= 0.0
            ? Fin.Succ(unit)
            : new MaterialFault.Parameter(key, $"<unsharp-parameters:{k.Sigma}|{k.Amount}|{k.Threshold}>"),
        bilateral: k => double.IsFinite(k.Sigma) && k.Sigma > 0.0 && double.IsFinite(k.RangeSigma) && k.RangeSigma > 0.0
            ? Fin.Succ(unit)
            : new MaterialFault.Parameter(key, $"<bilateral-parameters:{k.Sigma}|{k.RangeSigma}>"),
        median: k => k.Radius >= 1
            ? Fin.Succ(unit)
            : new MaterialFault.Parameter(key, $"<median-radius:{k.Radius}>"));

    public string Digest => Switch(
        gaussian:    static k => string.Create(CultureInfo.InvariantCulture, $"gaussian|{k.Sigma:R}"),
        unsharpMask: static k => string.Create(CultureInfo.InvariantCulture, $"unsharp|{k.Sigma:R}|{k.Amount:R}|{k.Threshold:R}"),
        bilateral:   static k => string.Create(CultureInfo.InvariantCulture, $"bilateral|{k.Sigma:R}|{k.RangeSigma:R}"),
        median:      static k => string.Create(CultureInfo.InvariantCulture, $"median|{k.Radius}"));
}

// RemapCurve families the tonal curves — affine-plus-gamma, empirical-CDF match, parsed .cube — one case each, all
// evaluated in the LINEAR domain over decoded lanes.
[Union]
public abstract partial record RemapCurve {
    private RemapCurve() { }

    public sealed record Levels(double Black, double White, double Gamma) : RemapCurve {
        // Black above White is the INVERSION, so `roughness = 1 - gloss` is a row of this case rather than an
        // Invert op beside it — and it inverts after the decode, which keeps an srgb-authored gloss plane honest.
        public static readonly Levels Invert = new(Black: 1.0, White: 0.0, Gamma: 1.0);
        public static readonly Levels Identity = new(Black: 0.0, White: 1.0, Gamma: 1.0);
    }

    // The target enters as its own SORTED SAMPLE LADDER rather than as a binned CDF over [0,1]: a bin count is not
    // a resolution knob but a quantization of the float substrate, and a unit-range grid collapses every
    // scene-linear value above one into the last bin and a Signed plane's whole negative half into the first.
    public sealed record Histogram(Seq<double> TargetSamples) : RemapCurve;

    // TableKey is the parsed table's content identity, minted from the .cube SOURCE TEXT at the one TryParseCube
    // call site (ContentHash.Of over its UTF-8 bytes) — keying the SOURCE rather than the parsed Lut3D.Data lattice
    // keeps the preimage one string append instead of a size-cubed float run re-folded at every plan mint.
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
            $"histogram|{string.Join(',', c.TargetSamples.Map(static v => v.ToString("R", CultureInfo.InvariantCulture)))}"),
        lut:       static c => string.Create(CultureInfo.InvariantCulture, $"lut|{c.TableKey:x32}|{(int)c.Interpolation}"));
}

// HeightDerivative families the fields a height plane derives. Occlusion carries its cast policy with compile-time
// defaults, so a channel row spells `new HeightDerivative.Occlusion()` rather than restating three numbers.
[Union]
public abstract partial record HeightDerivative {
    private HeightDerivative() { }

    public sealed record Occlusion(int Rays = 64, double Distance = 0.05, ulong Seed = 0UL) : HeightDerivative;
    public sealed record Curvature(CurvatureMeasure Measure = CurvatureMeasure.Mean) : HeightDerivative;

    // The produced field's own value range, as a ROW COLUMN the shape projection reads: a visibility fraction is
    // unit-bounded while a curvature is SIGNED at both ends, and a new derived field states its range here rather
    // than adding a second case test to the projection.
    public PlaneRange Range => Switch(
        occlusion: static _ => PlaneRange.Unit,
        curvature: static _ => PlaneRange.Signed);

    // Halo answers the row reach the banded pass reserves, in the plane's OWN extent: occlusion states its reach as
    // a FRACTION of the longer axis, so the halo grows with the plane and the axis takes an extent rather than
    // answering a constant. Curvature reads one stencil ring whatever the extent.
    public int Halo(int extent) => Switch(
        state:     extent,
        occlusion: static (span, d) => Math.Max(1, (int)Math.Round(d.Distance * span)),
        curvature: static (_, _) => 1);

    public string Digest => Switch(
        occlusion: static d => string.Create(CultureInfo.InvariantCulture, $"occlusion|{d.Rays}|{d.Distance:R}|{d.Seed:x16}"),
        curvature: static d => string.Create(CultureInfo.InvariantCulture, $"curvature|{(int)d.Measure}"));
}

public enum CurvatureMeasure { Mean, Gaussian, PrincipalMaximum, PrincipalMinimum }

// SwizzleLane carries a lane projection as DATA — a source lane or -1 for a constant, a scale, and a bias — so lane
// reorder, lane inversion, constant fill, and the dx->gl green flip are one kernel over one row table.
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
    // GNegate is the SIGNED green flip and it is a different row from GInverse, not a spelling of it. A DECODED
    // normal lane lives in `[-1,1]`, so the `dx`→`gl` conversion NEGATES; `1 − g` is the unit-range complement,
    // correct for a mask and wrong for a normal — it maps `+1` to `0` instead of to `−1` and tilts every texel.
    public static readonly SwizzleLane GNegate = new("gNegate", source: 1, scale: -1.0, bias: 0.0);

    public int Source { get; }
    public double Scale { get; }
    public double Bias { get; }
    public double Project(ReadOnlySpan<double> texel) =>
        (Source >= 0 && Source < texel.Length ? texel[Source] * Scale : 0.0) + Bias;
    private SwizzleLane(string key, int source, double scale, double bias) : this(key) =>
        (Source, Scale, Bias) = (source, scale, bias);

    // FlipGreen spells the plane#PLANE_VOCABULARY dx->gl conversion ONCE for the corpus, over the SIGNED lane the
    // decode ladder already produced — the same green-sign multiply `NormalConvention.ToGl` states.
    public static Seq<SwizzleLane> FlipGreen => Seq(R, GNegate, B, A);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct PlaneShape(PlaneFormat Format, Dimension Width, Dimension Height, Dimension Layers, PlaneTransfer Transfer, AlphaMode Alpha, PlaneRange Range) {
    public static PlaneShape Of(TexturePlane plane) =>
        new(plane.Format, plane.Width, plane.Height, plane.Layers, plane.Transfer, plane.Alpha, plane.Range);

    // Retyping resolves through the storage roster's own semantic-count rounding, so a three-lane result lands on
    // the four-lane row declaring AlphaMode.None and an unrostered format is a typed absence.
    public Fin<PlaneShape> Retyped(int components, AlphaMode alpha, PlaneRange range, Op key) =>
        PlaneFormat.For(components, Format.Depth)
            .ToFin(new MaterialFault.Parameter(key, $"<plane-format:{components}:{Format.Depth.Key}>"))
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

    // Stage is a PROJECTION of the case, never a column a caller supplies, so an op cannot declare itself cheaper
    // than it is and get fused into a row pass it cannot survive.
    public StageKind Stage => Switch(
        resize:       static _ => StageKind.Global,
        convolve:     static _ => StageKind.Neighbourhood,
        heightNormal: static op => op.Inverse ? StageKind.Global : StageKind.Neighbourhood,
        fromHeight:   static _ => StageKind.Neighbourhood,
        dilate:       static _ => StageKind.Neighbourhood,
        remap:        static op => op.Curve.Stage,
        swizzle:      static _ => StageKind.Pointwise);

    // Halo is the row HHALO a banded neighbourhood pass reserves, and it takes the SHAPE because a halo is not
    // always a constant: an occlusion march reaches a FRACTION of the plane's own extent, so its halo at 512 and at
    // 16k are different numbers and a bare radius property could only answer one of them. Every other case answers
    // the same value at every shape, which is what makes the parameter free rather than a knob.
    public int Halo(PlaneShape at) => Switch(
        state:        at,
        resize:       static (_, _) => 0,
        convolve:     static (_, op) => op.Kernel.Radius,
        heightNormal: static (_, op) => op.Inverse ? 0 : 1,
        fromHeight:   static (shape, op) => op.Derivative.Halo(Math.Max(shape.Width.Value, shape.Height.Value)),
        // A ring advances the coverage front exactly one texel, so N rings reach N rows and a band carrying that
        // halo runs every ring inside itself — which is what lets a multi-ring dilation band at all.
        dilate:       static (_, op) => Math.Max(1, op.Rings),
        remap:        static (_, _) => 0,
        swizzle:      static (_, _) => 0);

    // THE canonical per-op spelling every content-key preimage folds. Rename-stable by construction: case tokens
    // are frozen lowercase literals, owned rows spell their SmartEnum Key, external enums spell their invariant
    // integer, and every numeric formats under InvariantCulture at "R". Columns that only REPORT
    // (HeightEvidence.Residual) stay OUT of the preimage; every column that moves produced bytes enters whole.
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

    // Edge is the out-of-extent law the band fill applies ONCE, so no kernel re-derives an addressing rule per tap.
    // Resample and convolution carry the caller's declared mode; every other neighbourhood op states the mode its
    // own mathematics demands — the height stencils and the curvature Hessian are border-REFLECTED because
    // reflection IS the zero-normal-derivative mirror the bounded solver assembles, and dilation clamps because a
    // coverage front has no periodicity to honour.
    public EdgeMode Edge => Switch(
        resize:       static op => op.Edge,
        convolve:     static op => op.Edge,
        heightNormal: static _ => EdgeMode.Reflect,
        fromHeight:   static _ => EdgeMode.Reflect,
        dilate:       static _ => EdgeMode.Clamp,
        remap:        static _ => EdgeMode.Clamp,
        swizzle:      static _ => EdgeMode.Clamp);

    public string Kind => Switch(
        resize:       static _ => "resize",
        convolve:     static _ => "convolve",
        heightNormal: static _ => "heightNormal",
        fromHeight:   static _ => "fromHeight",
        dilate:       static _ => "dilate",
        remap:        static _ => "remap",
        swizzle:      static _ => "swizzle");

    // The defined-value gate over the foreign edge enum: `EdgeMode` is a wire enum whose undefined integrals are
    // representable, so a raw mode admits ONCE here and every interior switch is total over admitted values.
    private static Fin<EdgeMode> AdmittedEdge(EdgeMode edge, Op key) =>
        edge is EdgeMode.Clamp or EdgeMode.Wrap or EdgeMode.Reflect
            ? Fin.Succ(edge)
            : new MaterialFault.Parameter(key, $"<edge-mode:{(int)edge}>");

    // TOTAL shape-and-parameter projection, folded across the whole sequence before the first rental — so a chain
    // that cannot type costs nothing and leaves the source untouched, and every kernel body below is total over
    // admitted values.
    public Fin<PlaneShape> Project(PlaneShape input, Op key) => Switch(
        resize: op => input.Layers.Value is 1
            ? AdmittedEdge(op.Edge, key).Map(_ => input with { Width = op.Width, Height = op.Height })
            : new MaterialFault.Parameter(key, $"<resize-layered:{input.Layers.Value}>"),
        convolve: op => op.Kernel.Admitted(key).Bind(_ => AdmittedEdge(op.Edge, key)).Map(_ => input),
        heightNormal: op => (op.Inverse, input.Format.Components) switch {
            (false, 1) => input.Retyped(3, AlphaMode.None, PlaneRange.Signed, key),
            // Inverse solves ONE bounded or periodic grid: the Laplacian and the spectral kernel are w x h
            // operators, so a layered plane refuses outright — per-layer integration is the caller's Layer fold,
            // exactly as Resize rules it.
            (true, >= 3) when input.Layers.Value is not 1 =>
                new MaterialFault.Parameter(key, $"<height-inverse-layered:{input.Layers.Value}>"),
            (true, >= 3) => input.Retyped(1, AlphaMode.None, PlaneRange.Unit, key),
            (false, int n) => new MaterialFault.Parameter(key, $"<height-normal-scalar:{n}>"),
            (true, int n) => new MaterialFault.Parameter(key, $"<height-normal-vector:{n}>"),
        },
        fromHeight: op => input.Format.Components is 1
            ? input.Retyped(1, AlphaMode.None, op.Derivative.Range, key)
            : new MaterialFault.Parameter(key, $"<from-height-scalar:{input.Format.Components}>"),
        // Coverage IS the discriminant between written and unwritten, so a plane carrying none refuses rather than
        // reading a zero texel as emptiness — that inference dilates every legitimately black region outward.
        dilate: op => (input.Alpha.Traits.Admits(PlaneTrait.Coverage), op.Rings) switch {
            (false, _) => new MaterialFault.Parameter(key, $"<dilate-no-coverage:{input.Alpha.Key}>"),
            (_, <= 0) => new MaterialFault.Parameter(key, $"<dilate-rings:{op.Rings}>"),
            _ => Fin.Succ(input),
        },
        remap: _ => Fin.Succ(input),
        swizzle: op => op.Lanes.IsEmpty
            ? new MaterialFault.Parameter(key, "<swizzle-lanes-empty>")
            : input.Retyped(op.Lanes.Count, input.Alpha, input.Range, key));
}
```

## [03]-[PLANE_STAGE]

- Owner: `PlaneOp.Apply` the plan-schedule-run entry; `PlaneStage` the scheduled group; `BakeGovernance` the folder's ONE long-operation token-and-sink carrier; `PlaneReceipt` the evidence.
- Entry: `Apply(source, ops, key)` returns the transformed plane paired with its receipt. Its source is never mutated and never disposed — the caller owns it, because a chain that consumed its input would make a receipt useless as evidence.
- Law: GOVERNANCE is ONE carrier, never two tails. `BakeGovernance` pairs the cancellation token with an OPTIONAL `IProgress<double>` sink and its `Opened(done)` seam publishes the fraction and answers the token in one call, so no fold spells the two separately and no arm publishes progress it then cancels past. It is DEFAULT-INERT: a caller wanting neither passes nothing and an unwatched chain pays one struct copy. `Within(from, span)` NARROWS the carrier onto a sub-range, so one seam serves every depth: a stage hands its bands a governance reporting into the stage's own slice, and a band's `Opened(0..1)` reaches the caller as a true global fraction. That narrowing is what makes cancellation REAL — the token is answered per BAND inside a long pass rather than only between passes, so a cancelled sixteen-million-texel neighbourhood pass stops instead of running to completion, while the sampling stays coarse enough that a sink with one number to show is never flooded. The completed fraction is COUNT-DERIVED over the schedule rather than declared per row, because a chain's stage roster is the caller's own op sequence and there is no fixed vocabulary to declare fractions on; `press#TEXTURE_PRESS` and `environment#IBL_PREFILTER` compose this same carrier, so the corpus' three long operations report on one shape.
- Law: A NEIGHBOURHOOD PASS WALKS BANDS, because a whole-plane staging is not affordable at the extents this estate bakes: one interleaved double run over a 16k four-lane plane is 8 GiB and the separable body once held four at once, which defeats the arena law the typed store exists to hold. `StagingCeiling` is the ONE declared budget every band computation reads, and each band stages its own rows plus the op's halo. The band fills BY ADDRESS rather than by contiguity — slot `i` carries whatever row the op's `EdgeMode` names for `origin − halo + i`, so `Wrap` genuinely reaches the opposite edge, `Reflect` mirrors, and a `Clamp`-dropped tap is an ABSENT slot every kernel excludes — the square and ring walks per tap through `Slot`, the composed separable fold by narrowing its staged window onto the present run. The edge law is therefore resolved exactly once, at the fill, and `PlaneOp.Edge` states each op's own mode: a convolution carries the caller's, the height stencils and the curvature Hessian are reflected because reflection IS the Neumann mirror the bounded solver assembles, and dilation clamps. Rings iterate INSIDE the band, since a ring advances the coverage front one texel and a `Rings`-deep halo holds every neighbour those rings read.
- Law: A HALO IS A FUNCTION OF THE SHAPE, so `PlaneOp.Halo(shape)` takes the plane it runs at: an occlusion march reaches a FRACTION of the longer axis, which is 819 rows at 16k and 26 at 512, and a constant radius could only ever answer one of them. Where a halo alone exceeds the ceiling the walk COLLAPSES to one band over the whole plane — the arithmetic's own answer rather than a special case — and that degenerate band is exactly the extent-proportional op's declared cost: a 16k occlusion sweep stages its single-lane height field whole at 2 GiB. Every bounded-halo op bands genuinely.
- Law: THE EXPENSIVE KERNELS PARTITION. `Square` is O(r²) per texel and `Occlude` is rays × reach per texel, so both take the same `ParallelHelper` row partition the fused pointwise stage takes, over the band's own rows. Determinism is structural rather than promised: every row reads the READ-ONLY band and writes only its own texels, and the occlusion rotation is a COORDINATE-keyed draw with no sequence to reorder — so a re-run is byte-identical at every core count, which is what the content key requires. The fused pass partitions by BAND rather than by row, so its ping-pong rental and its widest-arity read amortize across the rows a partition walks instead of repeating per row.
- Law: SCHEDULING is what makes the algebra honest. Consecutive `pointwise` ops fuse into ONE row pass over one intermediate; a `neighbourhood` op takes its own pass against a materialized previous stage under its `EdgeMode` addressing; a `global` op takes a whole-plane pass. One fused row action across the whole sequence is the deleted form — it cannot supply a neighbour it has not written, a plane statistic it has not gathered, or a grid it has not resized, so the ops whose correctness depends on any of those would silently read the wrong texels.
- Law: fusion is a run-length fold over the sequence, not a special case: a chain of one op schedules identically to a chain of twenty, so the receipt reports the same stage structure at every arity and a benchmark reading it compares like with like.
- Law: each stage rents ONE output and disposes the previous intermediate at the stage boundary, so a twenty-op chain holds at most two planes and the source. Its final stage's output is the returned plane; the source is untouched.
- Law: EVERY Gaussian weight on this page is the kernel `Numerics/calculus#WEIGHT_PROFILES` `WeightKernelFamily.Gaussian` profile, reached through `ConvolveKernel.Support`/`RangeSupport` alone — the bandwidth correspondence is READ off that row at `[02]` and no kernel body here restates one. The three-sigma halo sits strictly inside the resolved support, so no tap is zeroed by the row's own support cut and the truncation stays the page's declared one.
- Law: the SEPARABLE FOLD IS COMPOSED, never spelled. The kernel transform band owns BOTH routes of the one convolution correspondence — the spectral product between its transform legs and the sample-domain `TapSeries.Convolve` tap fold — so this page's two axis passes are two calls on that owner and the `[04]` height integration is the same owner's spectral leg. Tap GENERATION stays here, because which weights fill the series is raster policy: the Gaussian table reads the kernel weight row at the support the bandwidth probe resolved, and it stages UNNORMALIZED, since the fold's resolved-weight divisor is the ONE partition-of-unity site — the renormalization that keeps a dropped border tap from darkening the rim lives at the owner, not in a page-local loop. The vertical pass hands the band's present run as a `TapWindow` under `TapBorder.Zero` — the fill already resolved the plane's edge law BY ADDRESS, so absence is the only law left, and under `Clamp` the absent slots sit at the staged run's own ends so the window simply narrows onto it; the horizontal pass folds whole rows under the page's one `EdgeMode`→`TapBorder` mapping, where `Clamp` maps to `Zero` because this page's clamp DROPS a tap from the weight sum rather than repeating the border texel. The band walk, the lane premultiply, the unsharp tail, and the stage scheduling stay this page's plane orchestration.
- Law: row work rides `ParallelHelper.For<TAction>` over a `struct IAction` whose fields are the two planes, the stage's ops, and the key. That action is `default`-constructed per partition or copied from the `in` seed, so the partition allocates nothing, inlines, and captures nothing — a `Parallel.For` over a closure would allocate one delegate and one display class per stage and defeat exactly the partition this shape exists for.
- Law: the receipt carries the op KEYS from the union's own `Kind` projection, never a runtime type name. Reflected type names are stale by construction against a rename and allocate on every op; the case key is the same string the wire and the benchmark row read.
- Law: layer work is per layer inside the row action: a layered plane's rows are one arena band per layer, so a stage walks `height × layers` rows and a resize refuses a layered plane outright rather than resampling across a face boundary that has no spatial meaning.
- Packages: CommunityToolkit.HighPerformance (composed — `ParallelHelper.For<TAction>(int, int)` the row partition, `IAction` the allocation-free slot, `SpanOwner<T>.Allocate` the per-row lane scratch, `MemoryOwner<T>.Allocate` the whole-plane statistic staging), System.Numerics.Tensors (`TensorPrimitives.ConvertSaturating<TFrom,TTo>` the ONE plane-to-run element crossing both staging primitives take, `TensorPrimitives.Sum` the banded height accumulation), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`Run`/`Grid`, `AlphaMode.ColourLanes`/`AlphaLane` the association's own lane arithmetic every kernel reads), `Rasm.Numerics` (composed — `WeightKernelFamily.Gaussian.Weight(double, PositiveMagnitude)` the ONE Gaussian profile every tap table and every range weight reads, `PositiveMagnitude` the admitted bandwidth carrier, `TapSeries.Of`/`Convolve` the sample-domain separable fold both axis passes call, `TapBorder` the closed border vocabulary the page's one edge mapping reaches, `TapWindow.Whole(Dimension, Dimension)` the staged-window geometry the banded horizontal pass states), TinyEXR.NET (composed — `ImageProcessing.Resize(ReadOnlySpan<float>, int, int, Span<float>, int, int, int, ResizeFilter, EdgeMode, int)` with the extent groups bracketing the two spans and the channel count following BOTH, `Lut3D.Apply(ReadOnlySpan<float>, Span<float>, int, LutInterpolation)`), `Rasm.Domain` (`Op`), LanguageExt.Core.
- Law: the MEDIAN IS A SELECTION, not a sort. Only the middle rank is wanted, so the window folds through a median-of-three partition that recurses into the side holding that rank — linear expected work in the window — and insertion survives only strictly below `SelectionCrossover`, where a contiguous compare-and-shift over a cache-line-sized run beats a partition's branching. Insertion above it is quadratic in the WINDOW rather than in the radius: a radius-8 median holds 289 samples and pays some forty thousand comparisons per lane per texel. The median-of-three pivot is what keeps an already-sorted window — a flat region, which is most of any plane a despeckle runs over — off the quadratic path.
- Growth: a new dependency class is one `StageKind` row with one arm in the runner; a new op reaching an existing class adds nothing here at all. A new halo law is one `Halo` arm and a new edge law one `Edge` arm, both read by the band fill with no kernel edit.
- Boundary: every loop-bearing member states its own KERNEL-EXEMPTION at the loop and each names the shape no span operator reaches — a strided gather, a clamped 2-D window, a data-dependent selection, a generator, or a side-effecting row rail — while every whole-run elementwise crossing folds onto `TensorPrimitives` instead. Every admission, plan, schedule, and receipt surface is expression-bodied; statements stop at the row kernel.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Numerics;
using System.Numerics.Tensors;
using System.Threading;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using TinyEXR.V3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Texture;
using Rasm.Numerics;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] ------------------------------------------------------------------------------
// THE FOLDER'S ONE LONG-OPERATION GOVERNANCE CARRIER: a token and an OPTIONAL progress sink travelling as ONE value,
// never a token tail plus a sink tail widening every signature twice. Silent is the inert default, so an unwatched
// bake pays one struct copy. Governance arrives as an explicit parameter, never as an ambient read inside a fold.
public readonly record struct BakeGovernance(CancellationToken Cancel = default, Option<IProgress<double>> Progress = default) {
    public static readonly BakeGovernance Silent = new();
    public BakeGovernance Governed(Option<IProgress<double>> progress, CancellationToken cancel) =>
        this with { Progress = progress, Cancel = cancel };
    // ONE publish-and-check seam: a stage OPENS by reporting its completed fraction and the same call answers the
    // token, so no fold spells the two separately and no arm can publish progress it then cancels past.
    public Option<Error> Opened(double done) {
        Progress.Iter(sink => sink.Report(done));
        return Cancel.IsCancellationRequested ? Some((Error)Errors.Cancelled) : None;
    }

    // Within NARROWS the carrier onto a sub-span, so one publish-and-check seam serves every depth: a band reports a
    // real GLOBAL fraction rather than a stage-local one no caller can interpret. That is what makes cancellation
    // genuine INSIDE a whole-plane pass — a token read only at stage boundaries leaves a cancelled
    // sixteen-million-texel neighbourhood pass running to completion.
    public BakeGovernance Within(double from, double span) =>
        this with { Progress = Progress.Map(sink => (IProgress<double>)new Slice(sink, from, span)) };

    private sealed record Slice(IProgress<double> Sink, double From, double Span) : IProgress<double> {
        public void Report(double value) => Sink.Report(From + (Span * Math.Clamp(value, 0.0, 1.0)));
    }
}

// One scheduled group. Ops is a fused pointwise run or exactly one non-fusing op, EACH op paired with the shape it
// ENDS at — a fused run may change lane count mid-chain (a swizzle before a remap), so the runner threads each
// op's own output shape rather than assuming the stage-terminal one, and Shape is the terminal the rental reads.
public readonly record struct PlaneStage(StageKind Kind, Seq<(PlaneOp Op, PlaneShape Shape)> Ops, PlaneShape Shape, int Halo);

// --- [ORDER_STATISTIC]
// A distribution as its SORTED SAMPLES, never a binned histogram — the ONE owner of the empirical-quantile transform
// for this folder, because two transcriptions drift on the plotting position, the value-axis range, and the tail.
// Sampling is STRIDED, CAPPED, and index-driven rather than drawn, so a 16k plane costs a bounded sort and a re-run
// reproduces the same ladder. The value axis is UNBOUNDED at both ends: an HDR plane keeps its headroom and a Signed
// plane its negative half.
public sealed class OrderStatistics {
    // Sixteen bits of quantile resolution — finer than any transfer this estate stores.
    public const int SampleCap = 65536;

    readonly double[][] ladders;

    OrderStatistics(double[][] ladders) => this.ladders = ladders;

    // ONE ladder per COLOUR lane, never one over luminance: a single table applied to three lanes shifts hue
    // wherever the lanes' distributions differ, which is every photograph carrying a colour cast.
    public static OrderStatistics Of(ReadOnlySpan<double> staging, int lanes, int colour) {
        long texels = staging.Length / lanes;
        int stride = (int)Math.Max(1L, texels / SampleCap);
        int count = (int)((texels + stride - 1) / stride);
        double[][] built = new double[colour][];
        for (int lane = 0; lane < colour; lane++) { built[lane] = new double[count]; }
        for (long texel = 0, at = 0; texel < texels && at < count; texel += stride, at++) {
            int seat = (int)(texel * lanes);
            for (int lane = 0; lane < colour; lane++) { built[lane][at] = staging[seat + lane]; }
        }
        foreach (double[] ladder in built) { Array.Sort(ladder); }
        return new OrderStatistics(built);
    }

    // The REGISTER-PLANE ingress `tile#TILE_SYNTH`'s Gaussian blend reads. The stride walks the TEXEL INDEX and the
    // row derives from it, so every read is bounded by the plane's own Height by construction — a stride loop over
    // ROWS gets that bound wrong exactly once, at the last partial row. Coverage is excluded: it is not a tonal
    // quantity and matching it re-weights every edge.
    public static OrderStatistics Of(ReadOnlySpan2D<ShadeVec4> plane) {
        long texels = (long)plane.Width * plane.Height;
        int stride = (int)Math.Max(1L, texels / SampleCap);
        int count = (int)((texels + stride - 1) / stride);
        double[][] built = [new double[count], new double[count], new double[count]];
        for (long texel = 0, at = 0; texel < texels && at < count; texel += stride, at++) {
            ShadeVec4 sample = plane[(int)(texel / plane.Width), (int)(texel % plane.Width)];
            (built[0][at], built[1][at], built[2][at]) = (sample.X, sample.Y, sample.Z);
        }
        foreach (double[] ladder in built) { Array.Sort(ladder); }
        return new OrderStatistics(built);
    }

    // A caller-supplied ladder enters SORTED whatever order it arrived in, because every read below is a binary
    // search and an unsorted target silently answers the wrong value rather than refusing.
    public static OrderStatistics Of(Seq<double> samples) {
        double[] ladder = samples.ToArray();
        Array.Sort(ladder);
        return new OrderStatistics([ladder]);
    }

    // HAZEN plotting position, (i + 0.5)/n — the SYMMETRIC convention a Gaussian-space transform requires, mapping
    // the extreme samples to finite quantiles so `Normal.InvCDF` never receives 0 or 1. The kernel `Distribution.Of`
    // R-7 convention is a DIFFERENT owner answering a reported-percentile question and deliberately pinning the
    // extremes at 0 and 1 — correct there, fatal under an inverse-normal composition.
    public double Quantile(int lane, double value) {
        double[] ladder = ladders[Math.Min(lane, ladders.Length - 1)];
        int found = Array.BinarySearch(ladder, value);
        // The HALF-SAMPLE OFFSET rides BOTH paths — a hit reports its index, a miss its insertion point, and each is
        // a rank in one ladder, so offsetting the hit alone splits two values one epsilon apart.
        double rank = (found >= 0 ? found : ~found) + 0.5;
        return Math.Clamp(rank / ladder.Length, 0.5 / ladder.Length, 1.0 - (0.5 / ladder.Length));
    }

    // The interpolated inverse in the ladder's OWN units. The quantile axis clamps because it is a probability;
    // the VALUE axis does not, which is the whole difference from the unit-range form this owner replaces.
    public double Value(int lane, double quantile) {
        double[] ladder = ladders[Math.Min(lane, ladders.Length - 1)];
        double position = (quantile * ladder.Length) - 0.5;
        int lo = Math.Clamp((int)Math.Floor(position), 0, ladder.Length - 1);
        int hi = Math.Min(lo + 1, ladder.Length - 1);
        double within = Math.Clamp(position - lo, 0.0, 1.0);
        return ladder[lo] + ((ladder[hi] - ladder[lo]) * within);
    }
}

public readonly record struct PlaneReceipt(Seq<string> Operations, Seq<string> Stages, long Texels, Option<HeightEvidence> Height, double ElapsedMs) {
    public static readonly PlaneReceipt Empty = new(Seq<string>.Empty, Seq<string>.Empty, 0L, None, 0.0);

    // Residual is the one signal that survives preconditioning and cancellation — press#PRESS_RECEIPT projects it
    // rather than re-deriving the evidence chain.
    public Option<double> Residual => Height.Map(static evidence => evidence.Residual);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public abstract partial record PlaneOp {
    // ONE entry over every arity. An empty sequence returns the source and an empty receipt, so a caller composing a
    // possibly-empty post chain needs no guard of its own; the OPTION-shaped clock is what makes the receipt's
    // elapsed a measurement rather than a literal zero, and press#TEXTURE_PRESS threads its own.
    public static Fin<(TexturePlane Plane, PlaneReceipt Receipt)> Apply(
        TexturePlane source, Seq<PlaneOp> ops, Op key, Option<TimeProvider> clock = default, BakeGovernance governance = default) {
        if (ops.IsEmpty) { return Fin.Succ((source, PlaneReceipt.Empty)); }
        TimeProvider ticks = clock.IfNone(TimeProvider.System);
        long opened = ticks.GetTimestamp();
        return Schedule(PlaneShape.Of(source), ops, key).Bind(stages => Run(source, stages, ops, key, ticks, opened, governance));
    }

    // PLAN then SCHEDULE: the shape folds across every op first, so a refusal costs no rental; the fold groups
    // consecutive fusing ops into one stage and pairs EVERY op with the shape it ENDS at — a fused run may change
    // lane count mid-chain, and a runner holding only the terminal shape hands a mid-run op the wrong stride.
    private static Fin<Seq<PlaneStage>> Schedule(PlaneShape input, Seq<PlaneOp> ops, Op key) =>
        ops.Fold(Fin.Succ((Shape: input, Stages: Seq<PlaneStage>.Empty)), (state, op) => state.Bind(carry =>
            op.Project(carry.Shape, key).Map(shape => (
                Shape: shape,
                // Head/Last are Option properties on Seq, so the tail read is the indexer — total under the
                // IsEmpty guard that already precedes it.
                Stages: !carry.Stages.IsEmpty && carry.Stages[^1].Kind.Fuses && op.Stage.Fuses
                    ? carry.Stages.Init.Add(carry.Stages[^1] with { Ops = carry.Stages[^1].Ops.Add((op, shape)), Shape = shape })
                    // The halo is read at the op's INPUT shape, never its output: a resize earlier in the chain
                    // moves the extent an occlusion march measures its reach against, and reading the terminal
                    // shape would size the band against a plane the op never sees.
                    : carry.Stages.Add(new PlaneStage(op.Stage, Seq((op, shape)), shape, op.Halo(carry.Shape)))))))
        .Map(static carry => carry.Stages);

    // RUN: one rental per stage, the previous intermediate disposed at the boundary, the SOURCE never touched, so a
    // twenty-op chain holds at most two planes. Execute is Fin-valued: a solver refusal PROPAGATES and the failed
    // stage's rental disposes, because an integration swallowed into an empty Option ships a plane of zeros.
    private static Fin<(TexturePlane, PlaneReceipt)> Run(
        TexturePlane source, Seq<PlaneStage> stages, Seq<PlaneOp> ops, Op key, TimeProvider ticks, long opened, BakeGovernance governance) =>
        stages.Fold(Fin.Succ((Plane: source, Evidence: Option<HeightEvidence>.None, Done: 0)), (state, stage) => state.Bind(carry =>
            // The stage boundary is the publish-and-check seam, its fraction COUNT-DERIVED because a chain's stage
            // roster is the caller's own op sequence with no fixed vocabulary to declare fractions on. A cancelled
            // chain disposes its intermediate on the same arm every other failure takes.
            governance.Opened(carry.Done / (double)stages.Count).Match(
                Some: abandoned => ReferenceEquals(carry.Plane, source)
                    ? Fin.Fail<(TexturePlane Plane, Option<HeightEvidence> Evidence, int Done)>(abandoned)
                    : Fin.Fail<(TexturePlane Plane, Option<HeightEvidence> Evidence, int Done)>(abandoned).Rollback(carry.Plane),
                // The stage receives a governance NARROWED to its own slice, so bands inside a long pass report a
                // real global fraction and answer the token at their own granularity.
                None: () => Rent(carry.Plane, stage, key)
                    .Bind(destination => PlaneKernel.Execute(carry.Plane, destination, stage, key,
                            governance.Within(carry.Done / (double)stages.Count, 1.0 / stages.Count))
                        .Bind(evidence => ReferenceEquals(carry.Plane, source)
                            ? Fin.Succ((Plane: destination, Evidence: evidence.IfNone(() => carry.Evidence), Done: carry.Done + 1))
                            : Custody.Bracket(
                                () => Fin.Succ((Plane: destination, Evidence: evidence.IfNone(() => carry.Evidence), Done: carry.Done + 1)),
                                carry.Plane))
                        .Rollback(destination)))))
        .Map(carry => {
            // The terminal publish lands after the last stage rather than as an off-by-one on the opening fraction.
            governance.Progress.Iter(static sink => sink.Report(1.0));
            return (carry.Plane, new PlaneReceipt(
                ops.Map(static op => op.Kind),
                stages.Map(static stage => stage.Kind.Key),
                carry.Plane.Texels,
                carry.Evidence,
                ticks.GetElapsedTime(opened).TotalMilliseconds));
        });

    // Rent discriminates on EXTENT, not on the op: a stage holding the source extent adopts the source's own lattice
    // so the affine and its spatial grain survive, and a resized stage seats a fresh one because a resample changes
    // what one texel spans.
    private static Fin<TexturePlane> Rent(TexturePlane source, PlaneStage stage, Op key) =>
        stage.Shape.Width == source.Width && stage.Shape.Height == source.Height
            ? TexturePlane.Of(stage.Shape.Format, source.Grid, stage.Shape.Layers, stage.Shape.Transfer,
                stage.Shape.Alpha, stage.Shape.Range, source.Primaries, key, AllocationMode.Default)
            : TexturePlane.Of(stage.Shape.Format, stage.Shape.Width, stage.Shape.Height, stage.Shape.Transfer,
                stage.Shape.Alpha, key, Some(stage.Shape.Layers), Some(stage.Shape.Range), Some(source.Primaries),
                mode: AllocationMode.Default);
}

// PlaneBand is the staged window ONE neighbourhood band works over — its own rows plus the halo on both sides,
// filled BY ADDRESS so the edge law resolves exactly once. Row coordinates are ABSOLUTE, so every kernel body keeps
// the shape it had over a whole-plane staging while residency stays inside the declared ceiling. A ref struct
// because it borrows its caller's rentals and must never outlive the band walk that owns them.
internal readonly ref struct PlaneBand(
    Span<double> staging, Span<bool> present, int origin, int own, int halo, int width, int height, int lanes, EdgeMode edge) {
    public Span<double> Staging => staging;
    public int Origin => origin;
    public int Own => own;
    public int Halo => halo;
    public int Width => width;
    public int Height => height;
    public int Lanes => lanes;
    public EdgeMode Edge => edge;

    // Slot maps an ABSOLUTE row onto its staged slot and answers -1 both outside this band's reach and where the
    // fill recorded an edge-dropped absence — ONE predicate, so no kernel reads a slot the fill never wrote.
    public int Slot(int row) {
        int at = row - (origin - halo);
        return at >= 0 && at < present.Length && present[at] ? at : -1;
    }

    // Every own row is present by construction: the fill addresses it inside the extent and no edge law drops one.
    public Span<double> Row(int row) => staging.Slice(Slot(row) * width * lanes, width * lanes);

    // Tap reads one lane at an absolute coordinate with the HORIZONTAL edge law inline — the vertical law already
    // rode the fill — and answers the caller's own fallback wherever either axis drops.
    public double Tap(int x, int y, int lane, double absent) {
        int slot = Slot(y), sx = PlaneKernel.Address(x, width, edge);
        return slot < 0 || sx < 0 ? absent : staging[((((slot * width) + sx) * lanes) + lane)];
    }
}

// PlaneKernel runs the stages. Pointwise fuses into ONE row pass; neighbourhood walks bordered BANDS; global takes
// the whole plane. Execute is Fin-valued and dispatches through the vocabulary's own generated Switch, so a new
// StageKind row breaks here at compile time and a solver refusal reaches the rail rather than an empty Option.
internal static class PlaneKernel {
    // --- [BAND_BUDGET]
    // THE ONE STAGING CEILING, declared so every band computation reads one number rather than a per-kernel guess:
    // one interleaved double run over a 16k four-lane plane is 8 GiB, so a pass walks ROW BANDS sized against this
    // 128 MiB order — the accelerator lane's own binding floor. A halo that ALONE exceeds it collapses the walk to
    // one band over the whole plane, the arithmetic's own answer rather than a refusal, and the ops that reach it
    // are exactly the extent-proportional ones: a 16k occlusion march at a five-percent reach needs 819 halo rows
    // and stages its single-lane field whole at 2 GiB, its declared cost at that extent.
    private const long StagingCeiling = 1L << 24;

    // A band always carries at least one own row, so the walk terminates at every extent.
    internal static int BandRows(int width, int lanes, int halo, int height) {
        long perRow = Math.Max(1L, (long)width * lanes);
        long affordable = (StagingCeiling / perRow) - (2L * halo);
        return (int)Math.Clamp(affordable, 1L, height);
    }

    internal static Fin<Option<HeightEvidence>> Execute(
        TexturePlane source, TexturePlane destination, PlaneStage stage, Op key, BakeGovernance governance) =>
        key.Catch(() => stage.Kind.Switch(
            state: (Source: source, Destination: destination, Stage: stage, Key: key, Governance: governance),
            pointwise:     static s => Pointwise(s.Source, s.Destination, s.Stage),
            neighbourhood: static s => Neighbourhood(s.Source, s.Destination, s.Stage, s.Key, s.Governance),
            global:        static s => Global(s.Source, s.Destination, s.Stage, s.Key, s.Governance)));

    // Pointwise fuses the row pass: each op threaded through a PING-PONG pair at ITS OWN shape, so a
    // swizzle-then-remap chain remaps the swizzled lanes. The partition is by ROW BAND rather than by row, because
    // the ping-pong scratch is a per-partition cost a row grain paid sixteen thousand times over a 16k plane; every
    // write still lands at its own texel, so the partition stays order-independent and byte-identical.
    private static Fin<Option<HeightEvidence>> Pointwise(TexturePlane source, TexturePlane destination, PlaneStage stage) {
        int rows = Math.Max(1, destination.Height.Value / Math.Max(1, Environment.ProcessorCount * PartitionsPerCore));
        int bands = ((destination.Height.Value + rows) - 1) / rows;
        PointwiseBands action = new(source, destination, stage.Ops, rows);
        ParallelHelper.For(0, bands * destination.Layers.Value, in action);
        return Fin.Succ(Option<HeightEvidence>.None);
    }

    // Enough bands per core that an uneven row cost still balances, few enough that the per-band rental amortizes.
    private const int PartitionsPerCore = 4;

    private readonly struct PointwiseBands(
        TexturePlane source, TexturePlane destination, Seq<(PlaneOp Op, PlaneShape Shape)> ops, int rows) : IAction {
        public void Invoke(int index) {
            int bands = ((destination.Height.Value + rows) - 1) / rows;
            int layer = index / bands, band = index % bands;
            int origin = band * rows, own = Math.Min(rows, destination.Height.Value - origin);
            // The widest-case lane scratch sizes ONCE per band off the roster's frozen widest arity, so no rental
            // crosses a row boundary inside the partition.
            int widest = Math.Max(source.Width.Value, destination.Width.Value) * PlaneFormat.MaxComponents;
            using SpanOwner<double> ping = SpanOwner<double>.Allocate(widest);
            using SpanOwner<double> pong = SpanOwner<double>.Allocate(widest);
            // The coverage lane is READ off the plane's own association row, never inferred from lane count: a
            // two-lane AlphaMode.None normal store has no alpha to skip and a four-lane one has a real padding lane.
            int alphaLane = source.Alpha.AlphaLane(source.Lanes);
            // KERNEL-EXEMPTION: the row walk drives two side-effecting rails around a ping-pong swap whose stride
            // changes per op — a sequencing statement no span operator expresses.
            for (int at = 0; at < own; at++) {
                int row = origin + at;
                source.Read(row, layer, ping.Span[..source.RowScalars]);
                Span<double> current = ping.Span;
                Span<double> next = pong.Span;
                int lanes = source.Lanes;
                foreach ((PlaneOp op, PlaneShape shape) in ops) {
                    int outLanes = shape.Format.Components;
                    Thread(op, current, next, source.Width.Value, lanes, outLanes, alphaLane);
                    (current, next) = (next, current);
                    lanes = outLanes;
                }
                destination.Write(row, layer, current[..destination.RowScalars]);
            }
        }

        // One op, one shape hop. The pattern switch is deliberate: the row spans are ref structs no generated
        // dispatch state can carry. The tail arm still COPIES the row, so a scheduler-unreachable arm can never
        // publish pool residue as a result.
        private static void Thread(PlaneOp op, ReadOnlySpan<double> input, Span<double> output, int width, int inLanes, int outLanes, int alphaLane) {
            switch (op) {
                case PlaneOp.Remap remap:
                    input[..(width * inLanes)].CopyTo(output);
                    Remap(remap.Curve, output[..(width * inLanes)], inLanes, alphaLane);
                    break;
                case PlaneOp.Swizzle swizzle:
                    Project(swizzle.Lanes, input, output, width, inLanes, outLanes);
                    break;
                default:
                    input[..(width * inLanes)].CopyTo(output);
                    break;
            }
        }

        // Every OUTPUT lane writes: a lane past the swizzle roster fills zero, because the retype rounds a semantic
        // count up and an unwritten trailing lane carries the ping-pong buffer's previous contents into the result.
        // KERNEL-EXEMPTION: a per-texel lane GATHER through a row table, not an elementwise map over one pair.
        private static void Project(Seq<SwizzleLane> lanes, ReadOnlySpan<double> input, Span<double> output, int width, int inLanes, int outLanes) {
            for (int x = 0; x < width; x++) {
                ReadOnlySpan<double> texel = input.Slice(x * inLanes, inLanes);
                for (int lane = 0; lane < outLanes; lane++) {
                    output[(x * outLanes) + lane] = lane < lanes.Count ? lanes[lane].Project(texel) : 0.0;
                }
            }
        }

        // Both leave the alpha lane untouched — a tonal curve over coverage darkens every edge. KERNEL-EXEMPTION on
        // both arms: each skips the coverage lane by stride residue and the Lut arm gathers a three-lane triple out
        // of an N-lane run, neither of which an elementwise operator over one contiguous pair addresses.
        private static void Remap(RemapCurve curve, Span<double> row, int lanes, int alphaLane) {
            switch (curve) {
                case RemapCurve.Levels levels: {
                    double span = levels.White - levels.Black;
                    for (int i = 0; i < row.Length; i++) {
                        if (alphaLane >= 0 && (i % lanes) == alphaLane) { continue; }
                        double normalized = span == 0.0 ? 0.0 : (row[i] - levels.Black) / span;
                        // The VALUE AXIS IS UNBOUNDED, the same law the order statistic holds — a display clamp
                        // belongs at the display encode, the one owner that knows a reference white. Gamma over a
                        // negative value is the ODD extension, because Math.Pow answers NaN there and one NaN texel
                        // poisons every fold below it; a unit gamma short-circuits, so Identity and Invert are exact.
                        row[i] = levels.Gamma == 1.0
                            ? normalized
                            : Math.CopySign(Math.Pow(Math.Abs(normalized), levels.Gamma), normalized);
                    }
                    break;
                }
                case RemapCurve.Lut lut: {
                    int texels = row.Length / lanes;
                    using SpanOwner<float> triple = SpanOwner<float>.Allocate(texels * 3);
                    for (int x = 0; x < texels; x++) {
                        for (int c = 0; c < 3; c++) { triple.Span[(x * 3) + c] = (float)row[(x * lanes) + Math.Min(c, lanes - 1)]; }
                    }
                    // Argument three is the CHANNEL COUNT of the interleaved run, not the texel count: the span
                    // length carries that, and a texel count here reads a three-channel row as an N-channel one.
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

    // Neighbourhood borders the pass BAND BY BAND, each band filled BY ADDRESS rather than by contiguity: slot i
    // carries whatever row the stage's `EdgeMode` names for `origin - halo + i`, so the edge law applies exactly
    // ONCE, at the fill, and every kernel below reads present slots alone. Each band OPENS the governance seam, so
    // the token is answered per BAND rather than per stage and a cancelled pass stops inside itself.
    // KERNEL-EXEMPTION on the band walk: it drives a fill, a dispatch, and an evidence accumulator in sequence.
    private static Fin<Option<HeightEvidence>> Neighbourhood(
        TexturePlane source, TexturePlane destination, PlaneStage stage, Op key, BakeGovernance governance) {
        int width = source.Width.Value, height = source.Height.Value, lanes = source.Lanes, halo = stage.Halo;
        EdgeMode edge = stage.Ops[0].Op.Edge;
        int rows = BandRows(width, lanes, halo, height);
        int bands = ((height + rows) - 1) / rows;
        long total = (long)bands * source.Layers.Value;
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate((rows + (2 * halo)) * width * lanes);
        using MemoryOwner<bool> present = MemoryOwner<bool>.Allocate(rows + (2 * halo));
        // Accumulators outlive the band walk because a derived field's evidence is a WHOLE-PLANE mean, so the bands
        // sum into one carrier and the divide lands once at the end rather than per band, where it would average
        // averages over unequal band heights.
        double gathered = 0.0;
        for (long slot = 0; slot < total; slot++) {
            int layer = (int)(slot / bands), band = (int)(slot % bands);
            Option<Error> abandoned = governance.Opened(slot / (double)total);
            if (abandoned.IsSome) { return Fin.Fail<Option<HeightEvidence>>(abandoned.IfNone(() => (Error)Errors.Cancelled)); }
            int origin = band * rows, own = Math.Min(rows, height - origin);
            PlaneBand window = Fill(source, layer, origin, own, halo, edge, staging.Span, present.Span);
            // Pattern dispatch takes the ref-struct seam Thread names: the band is a ref struct no generated
            // dispatch state can carry, and the family's own generated members keep the compile break a new case
            // owes. A refused fold PROPAGATES; the discard arm holds the CopyBand discipline.
            Fin<double> banded = stage.Ops[0].Op switch {
                PlaneOp.Convolve op => Convolve(window, source, destination, layer, op, key),
                PlaneOp.HeightNormal op => Fin.Succ(HeightField.ToNormalBand(window, source, destination, layer, op.Evidence)),
                PlaneOp.FromHeight op => Fin.Succ(Derive(window, source, destination, layer, op)),
                PlaneOp.Dilate op => Fin.Succ(Dilate(window, source, destination, layer, op)),
                _ => Fin.Succ(CopyBand(window, destination, layer)),
            };
            if (banded.Case is not double contribution) { return banded.Map(static _ => Option<HeightEvidence>.None); }
            gathered += contribution;
        }
        // Only the two evidence-bearing cases publish a mean, so the carrier is one number rather than a roster.
        double texels = width * (double)height * source.Layers.Value;
        return Fin.Succ(stage.Ops[0].Op.Switch(
            heightNormal: op => Some(op.Evidence with { Mean = gathered / texels }),
            fromHeight:   op => Some(op.Evidence with { Mean = gathered / texels }),
            convolve:     static _ => Option<HeightEvidence>.None,
            dilate:       static _ => Option<HeightEvidence>.None,
            resize:       static _ => Option<HeightEvidence>.None,
            remap:        static _ => Option<HeightEvidence>.None,
            swizzle:      static _ => Option<HeightEvidence>.None));
    }

    // Fill stages one band BY ADDRESS: a clamped halo holds an ABSENCE the kernels skip, never a duplicated border
    // texel, which is the rim a blur brightens. KERNEL-EXEMPTION: the walk drives a per-slot row rail.
    private static PlaneBand Fill(
        TexturePlane source, int layer, int origin, int own, int halo, EdgeMode edge, Span<double> staging, Span<bool> present) {
        int width = source.Width.Value, height = source.Height.Value, lanes = source.Lanes, span = own + (2 * halo);
        for (int slot = 0; slot < span; slot++) {
            int addressed = Address((origin - halo) + slot, height, edge);
            present[slot] = addressed >= 0;
            if (addressed >= 0) { source.Read(addressed, layer, staging.Slice(slot * width * lanes, width * lanes)); }
        }
        return new PlaneBand(staging[..(span * width * lanes)], present[..span], origin, own, halo, width, height, lanes, edge);
    }

    // The BAND-WISE unreachable-arm discipline: every non-fusing stage rents from the pool, so a silent return
    // publishes the last tenant's bytes as a plane. KERNEL-EXEMPTION: a side-effecting row rail.
    private static double CopyBand(PlaneBand window, TexturePlane destination, int layer) {
        for (int row = 0; row < window.Own; row++) {
            destination.Write(window.Origin + row, layer, window.Row(window.Origin + row));
        }
        return 0.0;
    }

    // Global takes the whole plane, and its refusal PROPAGATES — an unfactorable Laplacian swallowed to absence
    // ships a zero plane wearing a success.
    private static Fin<Option<HeightEvidence>> Global(
        TexturePlane source, TexturePlane destination, PlaneStage stage, Op key, BakeGovernance governance) =>
        stage.Ops[0].Op.Switch(
            resize: op => { Resample(source, destination, op); return Fin.Succ(Option<HeightEvidence>.None); },
            remap: op => { Match(source, destination, op.Curve); return Fin.Succ(Option<HeightEvidence>.None); },
            // ONE indivisible solve, so the token is answered at its ENTRY and the solve runs to its own stop:
            // cancelling inside a factorization abandons a partially built factor another owner holds.
            heightNormal: op => governance.Opened(0.0).Match(
                Some: abandoned => Fin.Fail<Option<HeightEvidence>>(abandoned),
                None: () => HeightField.ToHeight(source, destination, op.Solver, op.Evidence, op.Policy, key).Map(Some)),
            convolve: static _ => Fin.Succ(Option<HeightEvidence>.None),
            fromHeight: static _ => Fin.Succ(Option<HeightEvidence>.None),
            dilate: static _ => Fin.Succ(Option<HeightEvidence>.None),
            swizzle: static _ => Fin.Succ(Option<HeightEvidence>.None));

    // Resample delegates to the composed separable resampler: extent groups bracket the two spans and the channel
    // count follows BOTH, so a source-extent-then-channel-count spelling transposes the destination.
    private static void Resample(TexturePlane source, TexturePlane destination, PlaneOp.Resize op) {
        using MemoryOwner<float> input = MemoryOwner<float>.Allocate(source.Width.Value * source.Height.Value * source.Lanes);
        using MemoryOwner<float> output = MemoryOwner<float>.Allocate(destination.Width.Value * destination.Height.Value * destination.Lanes);
        Materialize(source, input.Span);
        ImageProcessing.Resize(input.Span, source.Width.Value, source.Height.Value, output.Span,
            destination.Width.Value, destination.Height.Value, source.Lanes, op.Filter, op.Edge,
            source.Alpha.AlphaLane(source.Lanes));
        Deposit(destination, output.Span);
    }

    // Materialize and Deposit are the ONE plane-to-run bridge, generic over the staging scalar so the float legs the
    // composed folds demand and the double legs the local kernels prefer are one body. Each row's element crossing is
    // a WHOLE-RUN elementwise convert over a contiguous pair, so it folds onto the strided conversion operator and
    // the saturating semantics stay the constraint's own; KERNEL-EXEMPTION on the band walk alone, which drives a
    // side-effecting row rail.
    internal static void Materialize<T>(TexturePlane plane, Span<T> staging) where T : unmanaged, INumberBase<T> {
        using SpanOwner<double> row = SpanOwner<double>.Allocate(plane.Width.Value * plane.Lanes);
        for (int layer = 0, at = 0; layer < plane.Layers.Value; layer++) {
            for (int y = 0; y < plane.Height.Value; y++, at += row.Length) {
                plane.Read(y, layer, row.Span);
                TensorPrimitives.ConvertSaturating(row.Span, staging.Slice(at, row.Length));
            }
        }
    }

    private static void Deposit<T>(TexturePlane plane, ReadOnlySpan<T> staging) where T : unmanaged, INumberBase<T> {
        using SpanOwner<double> row = SpanOwner<double>.Allocate(plane.Width.Value * plane.Lanes);
        for (int layer = 0, at = 0; layer < plane.Layers.Value; layer++) {
            for (int y = 0; y < plane.Height.Value; y++, at += row.Length) {
                TensorPrimitives.ConvertSaturating(staging.Slice(at, row.Length), row.Span);
                plane.Write(y, layer, row.Span);
            }
        }
    }

    // Convolve dispatches on the kernel's own SEPARABILITY column, never a type test, both routes over the same band
    // and the one edge law the fill already resolved. Neither contributes to the evidence mean, so both answer zero —
    // on the rail, because the separable route composes the kernel tap fold and its refusal PROPAGATES.
    private static Fin<double> Convolve(PlaneBand window, TexturePlane source, TexturePlane destination, int layer, PlaneOp.Convolve op, Op key) {
        int colour = source.Alpha.ColourLanes(source.Lanes);
        if (op.Kernel.Separable) { return Separable(window, source, destination, layer, op, key); }
        Square(window, destination, layer, colour, op);
        return Fin.Succ(0.0);
    }

    // Separable takes the axis-pass pair: TWO passes over one intermediate is O(2r) per texel where a square kernel
    // is O(r²), and each pass is ONE call on the kernel tap fold, so no fold body is spelled here. The unsharp arm
    // reuses that same blur — the halo it subtracts IS the blur. Tap GENERATION stays this page's: every tap reads
    // the kernel row's own Gaussian profile at the row's admitted support, and the table stages UNNORMALIZED because
    // the fold's resolved-weight divisor is the one partition-of-unity site — a border-dropped tap renormalizes at
    // the owner, never against a fixed divisor that darkens the rim. A mint or fold refusal PROPAGATES: the
    // copy-through this route once took shipped an unfiltered plane wearing a success.
    // The support arrives ADMITTED off the row, so a separable kernel with no bandwidth is unrepresentable rather
    // than floored into one; the absence arm is unreachable behind the `Separable` column and rails rather than
    // fabricating a support. KERNEL-EXEMPTION on the tap walk: a per-offset weight GENERATOR, not a map.
    private static Fin<double> Separable(PlaneBand window, TexturePlane source, TexturePlane destination, int layer, PlaneOp.Convolve op, Op key) {
        int width = window.Width, lanes = window.Lanes, radius = op.Kernel.Radius;
        if (op.Kernel.Support.Case is not PositiveMagnitude support) {
            return new MaterialFault.Parameter(key, $"<convolve-bandwidth-absent:{op.Kernel.Digest}>");
        }
        double[] taps = new double[(radius * 2) + 1];
        for (int tap = -radius; tap <= radius; tap++) { taps[tap + radius] = WeightKernelFamily.Gaussian.Weight(Math.Abs(tap), support); }
        Fin<TapSeries> mint = TapSeries.Of(new Arr<double>(taps), key);
        if (mint.Case is not TapSeries series) { return mint.Map(static _ => 0.0); }

        // Coverage premultiplies BEFORE the fold and divides back out after, so a transparent texel never bleeds its
        // colour across a coverage edge. The premultiply runs over the WHOLE band INCLUDING its halo, because a halo
        // row feeding the vertical pass must arrive in the same domain the own rows do. KERNEL-EXEMPTION on both
        // walks: each is a strided per-texel coverage weighting over an interleaved run.
        int alphaLane = source.Alpha.AlphaLane(lanes);
        if (alphaLane >= 0) {
            for (int at = 0; at < window.Staging.Length; at += lanes) {
                double coverage = window.Staging[at + alphaLane];
                for (int c = 0; c < alphaLane; c++) { window.Staging[at + c] *= coverage; }
            }
        }
        // The VERTICAL pass runs first and consumes the halo, collapsing the band to its own rows, so the
        // intermediate is OWN-ROW sized rather than band sized. The fill already resolved the plane's edge law BY
        // ADDRESS, so the staged band rides TapBorder.Zero — absence is the only law left — and under Clamp the
        // absent slots sit at the staged run's own ends, so the window simply narrows onto the present run. The
        // horizontal window's extent and stride cross as DIMENSIONS: both are census values the plane's own
        // admission already proved positive, so the generated throw is unreachable and this is the section's named
        // admission exemption rather than a rail.
        int span = window.Own + (2 * window.Halo), bottom = window.Origin - window.Halo;
        (int lead, int trail) = window.Edge == EdgeMode.Clamp
            ? (Math.Max(0, -bottom), Math.Max(0, (bottom + span) - window.Height))
            : (0, 0);
        int present = span - lead - trail;
        using MemoryOwner<double> vertical = MemoryOwner<double>.Allocate(window.Own * width * lanes);
        using MemoryOwner<double> blurred = MemoryOwner<double>.Allocate(window.Own * width * lanes);
        TapWindow columns = new(Extent: present, Origin: 0, From: window.Halo - lead, Run: window.Own, Stride: width * lanes);
        Fin<Unit> folded = series.Convolve(window.Staging.Slice(lead * width * lanes, present * width * lanes),
            vertical.Span, columns, TapBorder.Zero, key);
        for (int row = 0; folded.IsSucc && row < window.Own; row++) {
            folded = series.Convolve(vertical.Span.Slice(row * width * lanes, width * lanes),
                blurred.Span.Slice(row * width * lanes, width * lanes),
                TapWindow.Whole(extent: Dimension.Create(value: width), stride: Dimension.Create(value: lanes)),
                Border(window.Edge), key);
        }
        if (folded.Case is Error fault) { return Fin.Fail<double>(fault); }
        if (alphaLane >= 0) {
            for (int at = 0; at < blurred.Length; at += lanes) {
                double coverage = blurred.Span[at + alphaLane];
                for (int c = 0; c < alphaLane; c++) { blurred.Span[at + c] = coverage > 0.0 ? blurred.Span[at + c] / coverage : 0.0; }
            }
        }
        // The halo the unsharp tail subtracts IS this blur, read off the row's own Sharpen column; the subtrahend is
        // the band's own PREMULTIPLIED row, the same domain the blur landed in. KERNEL-EXEMPTION: the threshold
        // gates each element on its own difference, a masked arm no elementwise pair expresses.
        if (op.Kernel.Sharpen is { IsSome: true, Case: (double Amount, double Threshold) mask }) {
            for (int row = 0; row < window.Own; row++) {
                ReadOnlySpan<double> original = window.Row(window.Origin + row);
                Span<double> result = blurred.Span.Slice(row * width * lanes, width * lanes);
                for (int at = 0; at < result.Length; at++) {
                    double difference = original[at] - result[at];
                    result[at] = Math.Abs(difference) > mask.Threshold ? original[at] + (mask.Amount * difference) : original[at];
                }
            }
        }
        for (int row = 0; row < window.Own; row++) {
            destination.Write(window.Origin + row, layer, blurred.Span.Slice(row * width * lanes, width * lanes));
        }
        return Fin.Succ(0.0);
    }

    // Square serves the non-separable pair: a bilateral weight is the spatial Gaussian times a RANGE Gaussian over
    // the whole colour distance, so every lane of one texel shares one range weight and an edge stays an edge in all
    // lanes; a median selects the window's middle order statistic per lane. Square is the EXPENSIVE kernel here —
    // O(r²) against the separable pair's O(r) — so it partitions by ROW, and determinism survives because every row
    // reads the READ-ONLY band and writes only its own texels: a re-run is byte-identical at every core count.
    private static void Square(PlaneBand window, TexturePlane destination, int layer, int colour, PlaneOp.Convolve op) {
        int width = window.Width, lanes = window.Lanes;
        using MemoryOwner<double> output = MemoryOwner<double>.Allocate(window.Own * width * lanes);
        SquareRows action = new(window.Staging, output.Span, window, op, colour);
        ParallelHelper.For(0, window.Own, in action);
        for (int row = 0; row < window.Own; row++) {
            destination.Write(window.Origin + row, layer, output.Span.Slice(row * width * lanes, width * lanes));
        }
    }

    // The partition action holds SPANS, so it is a ref struct riding ParallelHelper's ref-struct action overload,
    // and the window scratch rents PER ROW so no rental crosses a partition. The two supports arrive ADMITTED off
    // the row: an ordered kernel carries ABSENCE on both, which is why the weighted arm reads them only where the
    // gather arm did not already `continue`. KERNEL-EXEMPTION on the window walk: a clamped 2-D GATHER whose taps
    // are addressed per (dy, dx) under the band's own edge law, which no elementwise operator reaches.
    private readonly ref struct SquareRows(
        Span<double> staging, Span<double> output, PlaneBand window, PlaneOp.Convolve op, int colour) : IAction {
        public void Invoke(int row) {
            int width = window.Width, lanes = window.Lanes, radius = op.Kernel.Radius;
            int span = (radius * 2) + 1;
            // The two supports UNWRAP ONCE per row: an Option probe inside the window walk is a boxed read per tap.
            // A row carrying no spatial bandwidth is ORDERED BY ABSENCE — a median publishes neither support and
            // gathers before either value is reached — so the unweighted seat is a slot the weighted path never
            // reads rather than a number standing in for a bandwidth.
            bool ordered = op.Kernel.Ordered;
            PositiveMagnitude spatial = default, range = default;
            if (op.Kernel.Support.Case is PositiveMagnitude reach) {
                (spatial, range) = (reach, op.Kernel.RangeSupport.IfNone(reach));
            } else {
                ordered = true;
            }
            int y = window.Origin + row;
            using SpanOwner<double> sample = SpanOwner<double>.Allocate(span * span);
            for (int x = 0; x < width; x++) {
                int centre = ((window.Slot(y) * width) + x) * lanes;
                for (int lane = 0; lane < lanes; lane++) {
                    int gathered = 0;
                    double sum = 0.0, admitted = 0.0;
                    for (int dy = -radius; dy <= radius; dy++) {
                        int slot = window.Slot(y + dy);
                        if (slot < 0) { continue; }
                        for (int dx = -radius; dx <= radius; dx++) {
                            int sx = PlaneKernel.Address(x + dx, width, window.Edge);
                            if (sx < 0) { continue; }
                            int at = (((slot * width) + sx) * lanes);
                            if (ordered) { sample.Span[gathered++] = staging[at + lane]; continue; }
                            double weight = WeightKernelFamily.Gaussian.Weight(Math.Sqrt((dx * dx) + (dy * dy)), spatial)
                                          * WeightKernelFamily.Gaussian.Weight(Distance(staging, at, centre, colour), range);
                            sum += weight * staging[at + lane];
                            admitted += weight;
                        }
                    }
                    output[((((row * width) + x) * lanes) + lane)] = ordered
                        ? Middle(sample.Span[..gathered])
                        : admitted > 0.0 ? sum / admitted : staging[centre + lane];
                }
            }
        }
    }

    // RANGE is Euclidean over the COLOUR lanes alone: coverage is an area, not a colour, so folding it into the
    // range term makes a transparent neighbour read as a distant one. KERNEL-EXEMPTION: a strided two-point
    // reduction over at most three interleaved lanes, where an operator call costs more than the three multiplies.
    private static double Distance(ReadOnlySpan<double> staging, int at, int centre, int colour) {
        double sum = 0.0;
        for (int c = 0; c < colour; c++) {
            double delta = staging[at + c] - staging[centre + c];
            sum += delta * delta;
        }
        return Math.Sqrt(sum);
    }

    // --- [ORDER_SELECT]
    // The middle order statistic is a SELECTION, not a sort: only the median RANK is wanted, so the window recurses
    // into the side holding that rank — linear expected work. Insertion survives strictly BELOW the crossover, where
    // a contiguous compare-and-shift inside one cache line beats a partition's branching; above it insertion is
    // quadratic in the WINDOW, and a radius-8 median holds 289 samples. KERNEL-EXEMPTION on all three bodies: an
    // in-place selection with a data-dependent recursion, which no reduction operator expresses.
    private const int SelectionCrossover = 32;

    private static double Middle(Span<double> sample) => sample.Length is 0 ? 0.0 : Select(sample, sample.Length / 2);

    private static double Select(Span<double> sample, int rank) {
        while (sample.Length > SelectionCrossover) {
            int split = Partition(sample);
            if (rank == split) { return sample[split]; }
            if (rank < split) { sample = sample[..split]; } else { sample = sample[(split + 1)..]; rank -= split + 1; }
        }
        Insertion(sample);
        return sample[rank];
    }

    // Median-of-three seats the pivot at the tail before the scan, which keeps an ALREADY SORTED window — a flat
    // region, most of any plane a despeckle runs over — off the quadratic path a first-element pivot walks into.
    private static int Partition(Span<double> sample) {
        int last = sample.Length - 1, mid = sample.Length / 2;
        if (sample[mid] < sample[0]) { (sample[0], sample[mid]) = (sample[mid], sample[0]); }
        if (sample[last] < sample[0]) { (sample[0], sample[last]) = (sample[last], sample[0]); }
        if (sample[mid] < sample[last]) { (sample[mid], sample[last]) = (sample[last], sample[mid]); }
        double pivot = sample[last];
        int split = 0;
        for (int at = 0; at < last; at++) {
            if (sample[at] > pivot) { continue; }
            (sample[split], sample[at]) = (sample[at], sample[split]);
            split++;
        }
        (sample[split], sample[last]) = (sample[last], sample[split]);
        return split;
    }

    private static void Insertion(Span<double> sample) {
        for (int i = 1; i < sample.Length; i++) {
            double value = sample[i];
            int j = i - 1;
            while (j >= 0 && sample[j] > value) { sample[j + 1] = sample[j]; j--; }
            sample[j + 1] = value;
        }
    }

    // Dilate advances coverage one texel per ring, so a chart gutter fills from its OWN chart rather than from
    // whichever neighbour a single wide pass reached first, and the pass writes coverage back at one so a later mip
    // fold sees a written texel. Rings iterate INSIDE the band: a band carrying `Rings` rows of halo holds every
    // neighbour every ring of its own rows will read, and the halo's rows going stale from ring two is harmless
    // because they are never written back. KERNEL-EXEMPTION: a ring-iterated coverage FRONT whose every texel reads
    // a 3x3 gather under the band's edge law and writes conditionally on its own coverage.
    private static double Dilate(PlaneBand window, TexturePlane source, TexturePlane destination, int layer, PlaneOp.Dilate op) {
        // The op's own Project gate refused a plane carrying no coverage, so the last lane IS the coverage lane here.
        int width = window.Width, lanes = window.Lanes, colour = lanes - 1, rows = window.Own + (2 * window.Halo);
        using MemoryOwner<double> next = MemoryOwner<double>.Allocate(window.Staging.Length);
        Span<double> current = window.Staging;
        for (int ring = 0; ring < op.Rings; ring++) {
            current.CopyTo(next.Span);
            for (int slot = 0; slot < rows; slot++) {
                for (int x = 0; x < width; x++) {
                    int centre = (((slot * width) + x) * lanes);
                    if (current[centre + colour] > 0.0) { continue; }
                    double weight = 0.0;
                    for (int c = 0; c < colour; c++) { next.Span[centre + c] = 0.0; }
                    for (int dy = -1; dy <= 1; dy++) {
                        int neighbour = slot + dy;
                        if (neighbour < 0 || neighbour >= rows) { continue; }
                        for (int dx = -1; dx <= 1; dx++) {
                            int sx = Address(x + dx, width, window.Edge);
                            if (sx < 0) { continue; }
                            int at = (((neighbour * width) + sx) * lanes);
                            double coverage = current[at + colour];
                            if (coverage <= 0.0) { continue; }
                            for (int c = 0; c < colour; c++) { next.Span[centre + c] += coverage * current[at + c]; }
                            weight += coverage;
                        }
                    }
                    if (weight <= 0.0) { continue; }
                    for (int c = 0; c < colour; c++) { next.Span[centre + c] /= weight; }
                    next.Span[centre + colour] = 1.0;
                }
            }
            next.Span.CopyTo(current);
        }
        for (int row = 0; row < window.Own; row++) {
            destination.Write(window.Origin + row, layer, window.Row(window.Origin + row));
        }
        return 0.0;
    }

    // Border is the page's ONE EdgeMode mapping onto the kernel fold's closed border rows. Clamp maps EXPLICITLY to
    // the DROPPING row, because this page's clamp DROPS a tap from the weight sum rather than repeating the border
    // texel: a clamped tap contributing its own value at full weight is a rim the blur brightens. The discard arm is
    // the compiler's totality floor over a foreign enum, unreachable behind Project's defined-value gate.
    internal static TapBorder Border(EdgeMode edge) => edge switch {
        EdgeMode.Wrap => TapBorder.Wrap,
        EdgeMode.Reflect => TapBorder.Mirror,
        EdgeMode.Clamp => TapBorder.Zero,
        _ => TapBorder.Zero,
    };

    // The ONE out-of-extent fold every neighbourhood kernel reads, resolved through the kernel border rows' own
    // address columns — no wrap or reflection arithmetic is spelled beside the owner's.
    internal static int Address(int index, int extent, EdgeMode edge) =>
        index >= 0 && index < extent ? index : Border(edge).Resolve(index, extent);

    // Derive routes the band through the derivative's own kernel and answers the band's SUMMED height, which the
    // caller divides once over the whole plane — a per-band mean averages averages over unequal band heights. The
    // ref-struct pattern seam is the same one Thread names (a state tuple holding a ref struct is CS0306), the
    // discard arm holds the CopyBand write-through discipline, and the compile break a new derivative case owes
    // rides the family's own generated Range, Halo, and Digest members.
    private static double Derive(PlaneBand window, TexturePlane source, TexturePlane destination, int layer, PlaneOp.FromHeight op) =>
        op.Derivative switch {
            HeightDerivative.Occlusion cast => Occlude(window, source, destination, layer, cast, op.Evidence),
            HeightDerivative.Curvature measure => Curve(window, source, destination, layer, measure, op.Evidence),
            _ => CopyBand(window, destination, layer),
        };

    // --- [UNREACHABLE_ARM]
    // The ONE discipline for an arm scheduling makes unreachable: copy the source through, because every non-fusing
    // stage rents from the pool and a silent return publishes the last tenant's bytes as a plane. Renting
    // AllocationMode.Clear everywhere is the alternative and pays a full clear to cover arms that never run.
    // KERNEL-EXEMPTION: a side-effecting row rail.
    private static void CopyThrough(TexturePlane source, TexturePlane destination) {
        using SpanOwner<double> row = SpanOwner<double>.Allocate(source.Width.Value * source.Lanes);
        for (int layer = 0; layer < source.Layers.Value; layer++) {
            for (int y = 0; y < source.Height.Value; y++) {
                source.Read(y, layer, row.Span);
                destination.Write(y, layer, row.Span);
            }
        }
    }

    // Occlude sweeps the horizon: each ray marches the height field along one azimuth to the derivative's own reach
    // and records the greatest elevation angle, so visibility is the hemisphere fraction no horizon occluded. The
    // azimuth fan is a REGULAR set rotated per texel — exactly computable, so a re-derived plane is byte-identical —
    // and the rotation is COORDINATE-keyed rather than stream-sequential, so a band partition cannot reorder a draw.
    private static double Occlude(
        PlaneBand window, TexturePlane source, TexturePlane destination, int layer, HeightDerivative.Occlusion cast, HeightEvidence evidence) {
        int width = window.Width, lanes = window.Lanes;
        using MemoryOwner<double> visibility = MemoryOwner<double>.Allocate(window.Own * width * lanes);
        using MemoryOwner<double> gathered = MemoryOwner<double>.Allocate(window.Own);
        // Occlusion is the page's other EXPENSIVE kernel — rays × reach taps per texel — so it partitions by row and
        // each row's height sum lands in its OWN slot, keeping the fold that follows order-independent.
        OccludeRows action = new(window.Staging, visibility.Span, gathered.Span, window, cast, evidence, layer, source);
        ParallelHelper.For(0, window.Own, in action);
        for (int row = 0; row < window.Own; row++) {
            destination.Write(window.Origin + row, layer, visibility.Span.Slice(row * width * lanes, width * lanes));
        }
        return TensorPrimitives.Sum(gathered.Span[..window.Own]);
    }

    private readonly ref struct OccludeRows(
        Span<double> staging, Span<double> visibility, Span<double> gathered, PlaneBand window,
        HeightDerivative.Occlusion cast, HeightEvidence evidence, int layer, TexturePlane source) : IAction {
        public void Invoke(int row) {
            int width = window.Width, lanes = window.Lanes, y = window.Origin + row;
            int reach = Math.Max(1, (int)Math.Round(cast.Distance * Math.Max(width, window.Height)));
            int rays = Math.Max(1, cast.Rays);
            double rowSum = 0.0;
            for (int x = 0; x < width; x++) {
                double centre = window.Tap(x, y, lane: 0, absent: 0.0);
                rowSum += centre;
                // Two int parameters carry the FULL 64-bit seed — low half as salt, high half as seed — so the
                // replay key loses nothing, and the coordinate keying is what lets the row partition above exist.
                double rotation = Deterministic.UnitInterval(new Point3d(x, y, layer),
                    salt: unchecked((int)cast.Seed), seed: unchecked((int)(cast.Seed >> 32)));
                double open = 0.0;
                for (int ray = 0; ray < rays; ray++) {
                    double azimuth = ((ray / (double)rays) + rotation) * 2.0 * Math.PI;
                    double dx = Math.Cos(azimuth), dy = Math.Sin(azimuth);
                    double horizon = 0.0;
                    for (int step = 1; step <= reach; step++) {
                        int sx = x + (int)Math.Round(dx * step), sy = y + (int)Math.Round(dy * step);
                        // Marches leaving the extent STOP rather than clamping: a clamped tap re-reads the border
                        // texel at every step and manufactures a horizon the relief has not.
                        if (sx < 0 || sx >= width || sy < 0 || sy >= window.Height) { break; }
                        // Rise is a MILLIMETRE difference and the run is the plane's own per-axis cell over the
                        // march it walked, so an anisotropic seat casts a true horizon at every resolution.
                        double rise = (window.Tap(sx, sy, lane: 0, absent: centre) - centre) * evidence.ScaleMm;
                        horizon = Math.Max(horizon, rise / source.Run(sx - x, sy - y));
                    }
                    // sin²θ is the cosine-weighted fraction that azimuth's slice occludes, so the visible fraction
                    // is 1/(1+tan²θ) and no arctangent is evaluated per step.
                    open += 1.0 / (1.0 + (horizon * horizon));
                }
                for (int lane = 0; lane < lanes; lane++) { visibility[((((row * width) + x) * lanes) + lane)] = open / rays; }
            }
            gathered[row] = rowSum;
        }
    }

    // Curvature is ONE eigenvalue projection over the kernel's own lattice Hessian, so the four measures are four
    // reads of one pair and no second-difference stencil is spelled here. That arm is border-REFLECTED — the exact
    // zero-normal-derivative mirror the bounded solver assembles — so the two kernels agree at the border rather
    // than by one texel of relief. KERNEL-EXEMPTION: a per-texel THREE-ROW stencil feeding a decomposition.
    private static double Curve(
        PlaneBand window, TexturePlane source, TexturePlane destination, int layer, HeightDerivative.Curvature curvature, HeightEvidence evidence) {
        int width = window.Width;
        using MemoryOwner<double> field = MemoryOwner<double>.Allocate(window.Own * width);
        double mean = 0.0;
        {
            // The band's halo is one row, so the Hessian addresses the staged slice at band-relative coordinates.
            ReadOnlySpan<double> slice = window.Staging;
            for (int row = 0; row < window.Own; row++) {
                int y = row + window.Halo;
                for (int x = 0; x < width; x++) {
                    mean += slice[(y * width) + x];
                    (double xx, double xy, _, double yy, _, _) =
                        Nabla.LatticeHessianAt(values: slice, grid: source.Grid, column: x, row: y);
                    // Mean, Gaussian, and both principal extrema read off ONE 2x2 decomposition rather than three
                    // further stencil passes.
                    double half = (xx + yy) * 0.5;
                    double gap = Math.Sqrt(Math.Max(0.0, (half * half) - ((xx * yy) - (xy * xy))));
                    double signed = curvature.Measure switch {
                        CurvatureMeasure.Mean => half,
                        CurvatureMeasure.Gaussian => (xx * yy) - (xy * xy),
                        CurvatureMeasure.PrincipalMaximum => half + gap,
                        _ => half - gap,
                    };
                    // Kernel stencils already divide by the lattice cell squared, so the millimetre amplitude is all
                    // this page applies and an identity affine leaves the measure texel-relative.
                    field.Span[(row * width) + x] = Math.Clamp(signed * evidence.ScaleMm, -1.0, 1.0);
                }
            }
        }
        for (int row = 0; row < window.Own; row++) {
            destination.Write(window.Origin + row, layer, field.Span.Slice(row * width, width));
        }
        return mean;
    }

    // The source distribution gathers over the WHOLE plane before the first texel maps, which is why this curve is
    // a global stage. Every arm WRITES the destination, including the two the scheduler makes unreachable, so the
    // dispatch carries the unreachable-arm discipline rather than a case test that silently returns.
    private static void Match(TexturePlane source, TexturePlane destination, RemapCurve curve) =>
        curve.Switch(
            levels:    _ => CopyThrough(source, destination),
            lut:       _ => CopyThrough(source, destination),
            histogram: c => MatchHistogram(source, destination, c));

    // The match runs over ORDER STATISTICS at both ends, so a texel maps value -> source quantile -> target value
    // entirely in REAL units: the unit-range binned form it replaces quantized the float substrate to its bin count,
    // folded every value above one into the last bin, and mapped a Signed plane's negative half onto bin zero. An
    // EMPTY target ladder copies through rather than mapping every texel onto nothing.
    // KERNEL-EXEMPTION on the map: each element's transform depends on its own LANE residue and two binary searches.
    private static void MatchHistogram(TexturePlane source, TexturePlane destination, RemapCurve.Histogram histogram) {
        if (histogram.TargetSamples.IsEmpty) { CopyThrough(source, destination); return; }
        int lanes = source.Lanes;
        int colour = Math.Max(1, source.Alpha.ColourLanes(lanes));
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(
            source.Width.Value * source.Height.Value * source.Layers.Value * lanes);
        Materialize(source, staging.Span);
        OrderStatistics observed = OrderStatistics.Of(staging.Span, lanes, colour);
        OrderStatistics target = OrderStatistics.Of(histogram.TargetSamples);
        for (int at = 0; at < staging.Length; at++) {
            int lane = at % lanes;
            // Coverage passes through untouched: it is not a tonal quantity, and matching it re-weights every edge.
            if (lane >= colour) { continue; }
            staging.Span[at] = target.Value(lane: 0, observed.Quantile(lane, staging.Span[at]));
        }
        Deposit(destination, staging.Span);
    }

}
```

## [04]-[HEIGHT_FIELD]

- Owner: `HeightEvidence` the correspondence carrier; `HeightPolicy` the bounded arm's extent policy; `HeightSolver` the integration routes; the occlusion and curvature derivative kernels.
- Law: `HeightEvidence` is what makes the `HeightNormal` inverse honest: a gradient field determines a height field only up to an additive constant and, at a normalizing depth, only up to the scale the forward normalization consumed — so the forward RECORDS the millimetre amplitude, the field mean, and the convention it read, and the inverse consumes them. Invoked with no evidence, that inverse rails rather than reconstructing a plausible field, because a fabricated amplitude is a displacement that renders confidently and wrongly. SPATIAL grain is NOT an evidence column: it is the plane's own `CellLattice` affine, read through `TexturePlane.Run`, so a horizon angle, a curvature magnitude, and a gradient slope all divide a millimetre rise by the run the plane they differentiate declares — an identity affine leaves that run in texel units and states the resolution-relative bound honestly, while a pitched one makes every derivative physical without a second carrier to keep in step.
- Law: `HeightPolicy` is the bounded arm's own extent policy as DATA, for the reason `tile#TILE_SYNTH` makes every threshold a column: a constant inside a kernel is a knob no caller turns and no key records, and a Krylov run stopping at one thousand iterations lands a different field than one stopping at four thousand. Every column enters `PlaneOp.Digest` whole, so two presses of one plan under two ceilings key distinctly.
- Law: `HeightSolver` is chosen by PERIODICITY, not by preference. `Spectral` is the Frankot-Chellappa least-squares integration in the frequency domain and it assumes a periodic domain — which is exactly true of a tiled plane and exactly false of a bounded one, where it wraps the opposite edge's gradient into the solution. `Poisson` assembles the five-point Laplacian with Neumann boundaries, which is correct on a bounded plane and needlessly expensive on a tiled one. Its `Periodic` column carries the choice, so a caller states the plane's own nature rather than a solver name; the direct-versus-iterative split INSIDE the bounded arm is the arm's own `HeightPolicy` — the kernel `CholeskySparse` exact factor to the policy's unknown ceiling, the kernel `SparsePreconditioner.Milu0` Krylov row above it, one operator assembly serving both — never a third row a caller could mis-pick.
- Law: the spectral route runs entirely on the kernel transform band and mints no transform of its own — the divergence stages into a `SpectralArena.Interleaved` over the plane's OWN `CellLattice`, `Transform(SpectralSense.Forward, SpectralScaling.Symmetric)` folds it, `SpectralReceipt.Modulate` applies the Frankot-Chellappa symbol as the pointwise spectral product the band declares its whole convolution surface, and the inverse transform closes the round trip. What survives here is the Frankot-Chellappa DIVERGENCE LAW alone: the inverse-Laplacian symbol with its zero bin held at zero, because that bin IS the additive constant integration cannot recover and the evidence's mean restores it. The arena is ARRAY-backed because every entrypoint the band composes is, so this route allocates its field where the bounded route rents one; the lattice rides at ONE layer, so the band's own rank-2 row-column fold IS the 2D transform and the multidim refusal the kernel page states as law never reaches a spelling here.
- Law: the bounded route assembles by TRIPLET ACCUMULATION and factors through the kernel's own SPD cache. Duplicate triplets sum and zeros drop at admission, so the Laplacian assembles by accumulation and never by hand-built compressed storage, and the factor carries the pivot-loss refusal onto the typed rail rather than as a bare exception.
- Law: every INEXACT floor in this section derives from a NAMED kernel `Numerics/atoms#EPSILON_POLICY` row and states which one on site. The normal-z divisor guard takes the near-unit row, because `nz` is a component of a unit vector and that row gates every near-unit comparison in the branch; the reconstruction's spread guard takes the seam-ulp row, the convergence floor no double iterate reaches below. A bare epsilon at a call site is the deleted form, and a floor whose magnitude no row explains is a tuning constant wearing a guard.
- Law: occlusion casts a LOW-DISCREPANCY azimuth set decorrelated per texel. That set is the exactly-computable uniform fan, and the per-texel rotation comes from the kernel `Deterministic.UnitInterval` COORDINATE draw keyed by the derivative's own seed — never a sequential stream — so a band partition cannot reorder a draw, a re-derived plane is byte-identical, and the low-frequency banding a shared direction set leaves on a flat wall breaks. Kernel `SampleKind` spectrum owns SET draws — its `ExtractionDomain` lattice case reaches a texel grid — yet this cast stays coordinate-keyed by design: a drawn set is partition-orderable and a re-derived plane must be byte-identical, which only the stateless coordinate draw guarantees.
- Law: the gradient and the Hessian are both the kernel `Numerics/calculus#NABLA` lattice-stencil arm over the plane's own `CellLattice`, so this page spells no finite difference of its own. That arm is total, non-`Fin`, allocation-free, `CellSize`-scaled, and border-REFLECTED — reflection is the exact zero-normal-derivative mirror, which is the Neumann boundary the bounded solver assembles, so the forward gradient, the curvature Hessian, and the assembled Laplacian all state one boundary condition. What stays this page's is the semantics: the surface-normal composition and its convention flip, the eigenvalue projection, the `evidence.ScaleMm` millimetre amplitude, and the `PlaneRange.Signed` packing.
- Law: the bounded route assembles the NEGATED Laplacian — positive diagonal, negative couplings, right-hand side negated to match — because Cholesky demands positive definiteness and the raw ∇² orientation is negative-semidefinite. Its last row PINS to the identity with its couplings eliminated SYMMETRICALLY (a one-sided pin leaves an asymmetric matrix no factor admits): a pure Neumann Laplacian carries the constant vector in its null space, so no factor exists for it unpinned, and the pinned gauge is exactly what the reconstruction's mean restores afterward.
- Packages: `Rasm.Numerics` (composed — `SpectralArena.Interleaved`/`Transform`/`SpectralSense`/`SpectralScaling` and `SpectralReceipt.Modulate` the kernel transform band this route's whole spectral leg, `Nabla.LatticeGradientAt`/`LatticeHessianAt` the ONE grid stencil, `CellLattice` the plane's own grid, `EpsilonPolicy.SqrtEpsilon`/`SeamUlp` the two named floors this section's divisor and spread guards derive from, `SparseMatrix.FromTriplets` the accumulating assembly, `SparsePreconditioner.Milu0` and `SolveIterativeDetailed` the no-fallback Krylov rail, `CholeskySparse.Of`/`Solve` the SPD factor cache carrying the pivot-loss refusal, `Dimension`), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Run`/`Grid`/`Read`/`Write`), `Rasm.Domain` (composed — `Deterministic.UnitInterval(Point3d, long, int)` the ONE replayable coordinate-keyed draw, `Op`), CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate` the solver staging, `SpanOwner<T>.Allocate` the row rental), System.Numerics.Tensors (`TensorPrimitives.Negate` the right-hand-side orientation, `TensorPrimitives.Min`/`Max`/`Sum` the reconstruction's three reductions), BCL inbox (`System.Numerics.Complex`, `INumberBase<T>` the one divergence body over two stagings).
- Growth: a new integration route is one `HeightSolver` row with one solve arm; a new derived field is one `HeightDerivative` case; a new curvature measure is one enum row the eigenvalue projection reads; a new stop axis is one `HeightPolicy` column that enters the digest by construction.
- Boundary: this section derives fields from a height plane and never SOURCES one. Height planes arrive from an ingest classification, a press bake, or the `HeightNormal` inverse over an acquired normal plane under a depth prior — and no inference stage emits height, because integration under a prior is pure mathematics the estate owns rather than a model it would have to license.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// HeightSolver rows the integration route, chosen by the PLANE's periodicity rather than by a solver preference:
// spectral wraps the opposite edge's gradient on a bounded plane, and the bounded route is needlessly expensive on
// a tiled one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HeightSolver {
    public static readonly HeightSolver Spectral = new("spectral", periodic: true);
    public static readonly HeightSolver Poisson = new("poisson", periodic: false);

    public bool Periodic { get; }
    private HeightSolver(string key, bool periodic) : this(key) => Periodic = periodic;
}

// --- [MODELS] ------------------------------------------------------------------------------
// HeightPolicy carries the bounded arm's extent policy as DATA, because a constant inside a kernel is a knob no
// caller turns and no key records while a Krylov stop MOVES the produced bytes. DirectCeiling is the unknown count
// at which the exact factor's 2D nested-dissection fill makes the factor the memory bound.
public readonly record struct HeightPolicy(int DirectCeiling, int KrylovIterations, Tolerance KrylovStop) {
    // The 1e-9 stop is the declared policy value on the kernel Krylov lane (Residual band); type-init refusal
    // is the registry-proof idiom, so an off-band edit fails at first touch rather than moving bytes silently.
    public static readonly Lazy<HeightPolicy> Standard = new(static () => new HeightPolicy(
        DirectCeiling: 1 << 22, KrylovIterations: 1000,
        KrylovStop: Tolerance.Of(lane: ToleranceLane.Krylov, value: 1e-9, key: Op.Of(name: nameof(HeightPolicy)))
            .IfFail(static e => throw e.ToException())));
    public string Digest =>
        string.Create(CultureInfo.InvariantCulture, $"{DirectCeiling}|{KrylovIterations}|{KrylovStop.Value:R}");
}

// What the forward direction DESTROYED and the inverse must be handed back: the millimetre amplitude, the mean the
// integration's free constant restores, the convention the gradient read, and the reconstruction-fit residual. The
// SPATIAL grain is deliberately ABSENT — it is the plane's own CellLattice affine read through TexturePlane.Run, so
// no second carrier can drift out of step with the plane a derivative differentiates.
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
    // The FORWARD direction: the kernel lattice gradient scaled by its own millimetre amplitude, composed into a
    // unit tangent-space normal under the declared convention, over the STAGED run the bordered pass filled once.
    // KERNEL-EXEMPTION: a per-texel THREE-ROW stencil feeding a normalized cross product into a row rail.
    internal static double ToNormalBand(PlaneBand window, TexturePlane height, TexturePlane normal, int layer, HeightEvidence evidence) {
        int width = window.Width;
        using SpanOwner<double> row = SpanOwner<double>.Allocate(normal.RowScalars);
        double mean = 0.0;
        {
            // The lattice gradient reads a THREE-ROW window and the band's halo is one row, so the stencil
            // addresses the staged slice at band-relative coordinates; the fill already applied the reflected
            // border, which is the same Neumann mirror the bounded inverse assembles.
            ReadOnlySpan<double> slice = window.Staging;
            for (int band = 0; band < window.Own; band++) {
                int y = band + window.Halo;
                for (int x = 0; x < width; x++) {
                    mean += slice[(y * width) + x];
                    // The surface normal is the cross product of the tangents (1,0,dx) and (0,1,dy), exactly
                    // (-dx, -dy, 1) before normalization; the stencil already divided by the lattice cell.
                    Vector3d slope = Nabla.LatticeGradientAt(values: slice, grid: height.Grid, column: x, row: y);
                    double dx = slope.X * evidence.ScaleMm, dy = slope.Y * evidence.ScaleMm;
                    double length = Math.Sqrt((dx * dx) + (dy * dy) + 1.0);
                    int at = x * normal.Lanes;
                    row.Span[at] = -dx / length;
                    row.Span[at + 1] = -dy * evidence.Convention.GreenSign / length;
                    row.Span[at + 2] = 1.0 / length;
                    // The retype lands a four-lane row declaring AlphaMode.None, so lane three is the structural
                    // pad and writes opaque.
                    for (int lane = 3; lane < normal.Lanes; lane++) { row.Span[at + lane] = 1.0; }
                }
                normal.Write(window.Origin + band, layer, row.Span);
            }
        }
        // The band's SUM rides back and the caller divides once, so the mean the inverse restores is a true
        // whole-plane mean rather than an average of per-band averages over unequal band heights.
        return mean;
    }

    // The INVERSE: least-squares integration of the gradient field, its zero bin HELD at zero because that bin IS
    // the additive constant integration cannot recover.
    internal static Fin<HeightEvidence> ToHeight(
        TexturePlane normal, TexturePlane height, HeightSolver solver, HeightEvidence evidence, HeightPolicy policy, Op key) =>
        solver.Periodic ? Spectral(normal, height, evidence, key) : Bounded(normal, height, evidence, policy, key);

    // Spectral is a THREE-CALL composition of the kernel transform band — stage, transform, modulate, transform back
    // — and states none of the band's row-column fold, symmetric scaling, or provider refusal. The arena is
    // ARRAY-backed because every entrypoint the band composes is, and the plane's grid seats at ONE layer so the
    // arena's census matches its buffer exactly.
    private static Fin<HeightEvidence> Spectral(TexturePlane normal, TexturePlane height, HeightEvidence evidence, Op key) {
        int w = height.Width.Value, h = height.Height.Value;
        Complex[] field = new Complex[w * h];
        Divergence(normal, field.AsSpan(), w, h);
        // The reconstruction reads the RESTORED handle's own arena rather than the seat buffer: the band mutates in
        // place today, and reading the handle states the data flow a future out-of-place band would otherwise break.
        return from spectrum in new SpectralArena.Interleaved(Values: field, Lattice: height.Grid)
                   .Transform(SpectralSense.Forward, SpectralScaling.Symmetric, key)
               from filtered in spectrum.Modulate(InverseLaplacian(w, h).AsSpan(), key)
               from restored in filtered.Arena.Transform(SpectralSense.Inverse, SpectralScaling.Symmetric, key)
               select Restore(height, RealPart(restored.Arena.Values), evidence);
    }

    // THE FRANKOT-CHELLAPPA SYMBOL — the one piece of this route the kernel does not own: least-squares integration
    // IS a pointwise spectral product with the inverse Laplacian −1/(u²+v²). Angular frequency is TEXEL-RELATIVE and
    // deliberately stays so, because Divergence differences at unit texel spacing and the receipt's lattice-scaled
    // Axis would scale one side of −∇²h = −div alone. KERNEL-EXEMPTION: a per-bin symbol GENERATOR.
    private static Complex[] InverseLaplacian(int width, int height) {
        Complex[] symbol = new Complex[width * height];
        for (int y = 0, at = 0; y < height; y++) {
            double v = 2.0 * Math.PI * (y <= height / 2 ? y : y - height) / height;
            for (int x = 0; x < width; x++, at++) {
                double u = 2.0 * Math.PI * (x <= width / 2 ? x : x - width) / width;
                double denominator = (u * u) + (v * v);
                symbol[at] = denominator > 0.0 ? new Complex(-1.0 / denominator, 0.0) : Complex.Zero;
            }
        }
        return symbol;
    }

    // The reconstructed field is real by construction, so the real part reads once into its own buffer rather than
    // making Restore index a Complex staging it has no other reason to know. KERNEL-EXEMPTION: a component
    // extraction off a two-component element, which the elementwise plane carries no operator for.
    private static double[] RealPart(ReadOnlySpan<Complex> field) {
        double[] real = new double[field.Length];
        for (int at = 0; at < real.Length; at++) { real[at] = field[at].Real; }
        return real;
    }

    // Bounded assembles the five-point Neumann Laplacian by triplet ACCUMULATION and factors through the kernel's
    // SPD cache below the policy's ceiling. Direct-vs-iterative is the arm's OWN policy driven by the unknown count,
    // never a caller knob and never a third HeightSolver row, because PERIODICITY is the only nature a caller states.
    private static Fin<HeightEvidence> Bounded(TexturePlane normal, TexturePlane height, HeightEvidence evidence, HeightPolicy policy, Op key) {
        // The order is the 2D GRID, never the plane's texel count: a layered plane's texel total would size a
        // system whose five-point stencil addresses rows that do not exist.
        Dimension order = Dimension.Create(checked(height.Width.Value * height.Height.Value));
        using MemoryOwner<double> rhs = MemoryOwner<double>.Allocate(order.Value);
        Divergence(normal, rhs.Span, height.Width.Value, height.Height.Value);
        // The assembled operator is the NEGATED Laplacian — the positive-definite form Cholesky demands — so the
        // right-hand side negates to match through the strided operator rather than an index walk.
        TensorPrimitives.Negate(rhs.Span, rhs.Span);
        // Pinned rows carry a ZERO right-hand side, matching the identity row that fixes the gauge: leaving the
        // divergence there sets the pinned texel to an arbitrary curvature and drags the restored mean with it.
        rhs.Span[order.Value - 1] = 0.0;
        if (order.Value > policy.DirectCeiling) { return Krylov(rhs.Span, height, evidence, policy, key); }
        return from matrix in SparseMatrix.FromTriplets(order, order, Laplacian(height.Width.Value, height.Height.Value), key)
               from factor in CholeskySparse.Of(matrix, key)
               from solved in factor.Solve(new Arr<double>(rhs.Span), key)
               select Restore(height, solved.AsSpan(), evidence);
    }

    // Krylov is the LARGE-EXTENT arm on the kernel's OWN preconditioned rail: the SAME FromTriplets assembly (one
    // operator, two solve routes), the Milu0 row the Neumann Laplacian wants, and SolveIterativeDetailed's
    // no-fallback contract — a non-converged run rails by NAME rather than letting a dense fallback densify
    // millions of unknowns.
    private static Fin<HeightEvidence> Krylov(ReadOnlySpan<double> rhs, TexturePlane height, HeightEvidence evidence, HeightPolicy policy, Op key) {
        Dimension order = Dimension.Create(rhs.Length);
        Arr<double> source = new(rhs);
        return SparseMatrix.FromTriplets(order, order, Laplacian(height.Width.Value, height.Height.Value), key)
            .Bind(matrix => matrix.SolveIterativeDetailed(source, SparsePreconditioner.Milu0,
                policy.KrylovStop.Value, policy.KrylovIterations, key: key))
            .Bind(receipt => receipt.Stop.IsUsable
                ? Fin.Succ(Restore(height, receipt.Solution.AsSpan(), evidence))
                : new MaterialFault.Parameter(key, $"<height-krylov:{receipt.Stop.Key}:{receipt.Residual:R}>"));
    }

    // Divergence is what the integration inverts: each texel's (-nx/nz, -ny/nz) slope pair differentiated once more,
    // so the right-hand side IS the Laplacian of the height field sought. The generic constraint is the point — the
    // spectral route needs Complex cells and the bounded route double ones, and a second transcription is where a
    // sign flips in exactly one of them and only the periodic plane shows it. Border slopes clamp, the one-sided
    // difference the assembled Laplacian's omitted-neighbour boundary row expects.
    // KERNEL-EXEMPTION: a four-tap centred stencil per texel over a strided normal run.
    private static void Divergence<T>(TexturePlane normal, Span<T> field, int width, int height) where T : INumberBase<T> {
        int lanes = normal.Lanes;
        using MemoryOwner<double> staged = MemoryOwner<double>.Allocate(width * height * lanes);
        PlaneKernel.Materialize(normal, staged.Span);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                // p = -nx/nz and q = -ny/nz are the height gradients the unit normal encodes.
                double px = Slope(staged.Span, x + 1, y, width, height, lanes).P;
                double mx = Slope(staged.Span, x - 1, y, width, height, lanes).P;
                double qy = Slope(staged.Span, x, y + 1, width, height, lanes).Q;
                double ry = Slope(staged.Span, x, y - 1, width, height, lanes).Q;
                field[(y * width) + x] = T.CreateChecked(((px - mx) + (qy - ry)) * 0.5);
            }
        }
    }

    // The DEGENERACY FLOOR on the divisor is the kernel `Numerics/atoms#EPSILON_POLICY` near-unit row, not a bare
    // literal: nz is a component of a unit vector, so the row that gates every near-unit and residual comparison in
    // the branch is the one that decides when it has collapsed. A grazing normal's true slope is unbounded, and the
    // floor exists to keep the fold TOTAL rather than to bound the answer — the sign rides through `CopySign`, so a
    // negative-facing degenerate normal keeps its orientation instead of flipping the whole divergence beneath it.
    private static (double P, double Q) Slope(ReadOnlySpan<double> staged, int x, int y, int width, int height, int lanes) {
        int at = ((Math.Clamp(y, 0, height - 1) * width) + Math.Clamp(x, 0, width - 1)) * lanes;
        double nz = staged[at + 2];
        double floor = Math.Abs(nz) < EpsilonPolicy.SqrtEpsilon
            ? Math.CopySign(EpsilonPolicy.SqrtEpsilon, nz == 0.0 ? 1.0 : nz)
            : nz;
        return (-staged[at] / floor, -staged[at + 1] / floor);
    }

    // The NEGATED five-point Neumann operator as TRIPLETS: positive count on the diagonal, minus one per kept
    // neighbour, duplicates summing at admission so a hand-built compressed storage never appears. A boundary row
    // omits its absent neighbours and carries the count it kept, which IS the zero normal derivative the forward
    // stencil's reflection states. The last row PINS to the identity because a pure Neumann Laplacian carries the
    // constant vector in its null space and no factor exists for it unpinned.
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

    // Integration recovers the field up to an additive constant, so the reconstruction re-centres on the evidence's
    // recorded mean. The residual it reports is the RECONSTRUCTION FIT — the largest excursion outside the
    // normalized span relative to the field's own spread — the one signal BOTH routes can measure at one owner,
    // since the spectral route consumes its right-hand side inside the transform and leaves no linear residual.
    // The three reductions fold onto the strided operators; KERNEL-EXEMPTION on the write walk alone, which drives
    // a side-effecting row rail while accumulating a running maximum.
    private static HeightEvidence Restore(TexturePlane height, ReadOnlySpan<double> solved, HeightEvidence evidence) {
        int width = height.Width.Value;
        double low = TensorPrimitives.Min(solved), high = TensorPrimitives.Max(solved);
        double offset = evidence.Mean - (TensorPrimitives.Sum(solved) / Math.Max(1, solved.Length));
        // The spread floor is the kernel's own SEAM-ULP row — the convergence floor no double iterate reaches below
        // — so a constant field reports a finite residual rather than dividing by an exact zero.
        double spread = Math.Max(EpsilonPolicy.SeamUlp, high - low);
        using SpanOwner<double> row = SpanOwner<double>.Allocate(height.RowScalars);
        double residual = 0.0;
        for (int layer = 0, at = 0; layer < height.Layers.Value; layer++) {
            for (int y = 0; y < height.Height.Value; y++) {
                for (int x = 0; x < width; x++, at++) {
                    double value = solved[at] + offset;
                    double clamped = Math.Clamp(value, 0.0, 1.0);
                    residual = Math.Max(residual, Math.Abs(value - clamped) / spread);
                    // The inverse's own Project gate retypes to ONE component, so the write is one scalar per texel.
                    row.Span[x] = clamped;
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
