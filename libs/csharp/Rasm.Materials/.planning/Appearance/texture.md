# [MATERIALS_TEXTURE]

THE UV-AND-SOLID SAMPLING ENGINE. One `TextureUv` static sampling fold over the closed `TextureSource` `[Union]` (noise · checker · gradient · image · triplanar), addressed by the closed `AddressMode` band, reconstructed by the closed `FilterMode` band, and seeded by the author-kernel `ProceduralNoise` over the closed `NoiseBasis` band (`Perlin` gradient · `Simplex` OpenSimplex2 · `Value` lattice · `Worley` cellular) — each basis carrying its 2D AND 3D arm so triplanar and solid texturing sample the same lattice — with the fractal trajectory carried by the closed `FractalMode` axis (`FBm` · `Ridged` · `PingPong`) and the cellular feature algebra by the orthogonal `CellularDistance` × `CellularReturn` bands, all vendored inline from the FastNoiseLite algorithm and made exactly periodic by the `NoisePeriod` lattice wrap every wrappable basis carries — a `Noise.Of` admission proving basis, lacunarity, frequency, and warp frequency integral against the declared period, so a tiling procedural is periodic by construction rather than healed after the fact. `TextureSource` cases carry every texture variation, the `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` rows carry every sampling mode, `Unicolour` carries every color and `ShadeVec4` every sampled field — never a parallel sampler, a per-filter method, a parallel fractal-vs-basis enum, a `NoiseSampler3D` surface, or a second color register. `TextureUv` composes the kernel `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail, the `graph#MATERIAL_GRAPH` `PortValue`/`PortId`/`ShadePoint` carriers the node DAG threads, the Rasm.Numerics `UnitInterval`/`Dimension` value-objects for UV coordinates and image extents, the host `Vector3d` for world position and normal at the shading edge, and Wacton.Unicolour directly as the scene-linear color owner for every color literal — never re-minting a color space, a coordinate primitive, or a fault. `graph#MATERIAL_GRAPH` `AppearanceNode.Texture(Func<double,double,PortValue> Sample)` closes the terminal seam: `TextureUv.Port` mints that total `(u,v)→PortValue` closure from a `TextureSource`, so a sampled texture drives a node DAG without a second sampler API, and the deep `Sample` rail serves the wire and the masked-aging consumer reading the `Fin`.

## [01]-[INDEX]

- [02]-[TEXTURE_UV]: `TextureUv` folds the `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` bands, the `ShadeVec4` four-lane field register, the `NoisePeriod`/`NoiseLattice` periodic-lattice carriers, the `TextureSource` union with its `Noise.Of` admission, the `ProceduralNoise` author-kernel, the one `TextureUv.Sample` fold, and the `TextureUv.Port` graph-node bridge.
- [03]-[PERIOD_GOLDEN]: `PeriodGolden` rows and the `PeriodProof` roster prove each periodic source repeats under its own shift and each unwrappable source refuses at admission.

## [02]-[TEXTURE_UV]

- Owner: `TextureUv` static sampling fold; `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` `[SmartEnum<int>]` bands; `ShadeVec4` field register; `ProceduralNoise` author-kernel; `TextureSource` `[Union]`.
- Cases: address {`Repeat`, `Clamp`, `Mirror`} · filter {`Nearest`, `Bilinear`, `Bicubic`, `Trilinear`} · noise-basis {`Perlin`, `Simplex`, `Value`, `Worley`} (fBm is octave-summation over a basis, `Octaves > 1`, never a fifth basis; `Wrappable` is the per-row periodic-lattice column) · fractal {`FBm`, `Ridged`, `PingPong`} · cellular-distance {`EuclideanSq`, `Euclidean`, `Manhattan`, `Hybrid`} · cellular-return {`CellValue`, `Distance`, `Distance2`, `Distance2Sub`, `Distance2Add`, `Distance2Mul`, `Distance2Div`} · source {`Noise`, `Checker`, `Gradient`, `Image`, `Triplanar`}.
- Entry: `public static Fin<Noise> Noise.Of(Noise candidate, Op key)` is the noise-source admission the lattice period exists for — an aperiodic draft passes on one predicate, a periodic one proves its basis wrappable, its lacunarity and frequency integral against the period, and its warp frequency integral, so a source seaming at the tile edge is unrepresentable rather than rendered; `public static Fin<ShadeVec4> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key)` is the deep field rail and `public static Func<double, double, PortValue> Port(TextureSource source, UvSample anchor, SamplerState sampler, Channel channel, Op key)` is the `graph#MATERIAL_GRAPH` `AppearanceNode.Texture` bridge — `Port` captures the source/sampler/key and returns the TOTAL `(u,v)→PortValue` closure the node fold reads (`Channel` projects the field to `PortValue.Color`, `.Scalar`, or `.Vector`), a stopless-gradient/empty-pyramid/degenerate-normal/non-finite-field sample folding to the channel's neutral `PortValue` so the graph arm stays total while the deep `Sample` rail carries the `Op key`-correlated `MaterialFault` — the UV lanes themselves are `UnitInterval` value-objects, finite-in-[0,1] by construction, so no interior re-validation exists on the coordinate path; arity is one — a texture variation discriminates on the `TextureSource` union case and a sample modality on `Channel`, never on a sibling sampler method.
- Packages: Rasm.Materials.Appearance.Bsdf (`MaterialFault` band-2450), Rasm (project — `UnitInterval`/`Dimension`), Rhino.Geometry (`Vector3d`/`Point3d` at the shading edge, the graph-page host-geometry convention), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<int>]` at the deepest surface — generated total `Switch`, `[UseDelegateFromConstructor]` behavior columns), LanguageExt.Core (`Fin`/`Seq`/`Bind`/`Fold`/`Traverse`), Wacton.Unicolour (scene-linear color owner; `Mix` in `ColourSpace.Oklch` the perceptual gradient resolve), CommunityToolkit.HighPerformance (`ReadOnlyMemory2D<ShadeVec4>`/`ReadOnlySpan2D` — the mip-plane owner, admitted once per level through `AsMemory2D(height, width)`), BCL inbox.
- Growth: a new addressing rule is one `AddressMode` row, a new reconstruction filter one `FilterMode` row, a new leaf noise basis one `NoiseBasis` row binding one `ProceduralNoise.Sample2D`/`Sample3D` arm pair and answering the `Wrappable` column, a new per-octave lattice policy one `NoiseLattice` column rather than a widened arm signature, a new fractal trajectory one `FractalMode` row, a new cellular feature one `CellularReturn`/`CellularDistance` row, a new texture one `TextureSource` case carrying its `MtlxCategory`, a new sampled-channel modality one `Channel` row — never a parallel `BilinearSampler`/`PerlinTexture`/`NoiseSampler3D` surface and never a parallel fractal-kind enum since the fractal trajectory is a `FractalMode` row over the basis octave-sum. `NoiseBasis` closes on the FastNoiseLite leaf-basis family (the `Perlin`/`Simplex`/`Value`/`Worley` gradient·simplex·lattice·cellular quartet) projecting onto the MaterialX 1.39 `noise2d`/`noise3d`/`fractal2d`/`cellnoise2d`/`worleynoise2d` categories — `Value` the `cellnoise2d` value-noise analogue, `noise3d` (`NodeCategory.Perlin3D`) the solid-noise wire target, `unifiednoise2d` a parameterized selector no single basis maps cleanly onto; `ValueCubic` is one `NoiseBasis` row binding one `ProceduralNoise` arm and one `MtlxNode`, not a new noise class. `TextureSource.MtlxCategory`, `NoiseBasis.MtlxNode`, `AddressMode.MtlxAddress`, and `FilterMode.MtlxFilter` project the MaterialX 1.39 node-category parity the `interchange#MATERIALX_DOCUMENT` `Mtlx.CategoryOf` resolves against the closed `NodeCategory` set — MaterialX names node categories and never the lattice math, so the FNL kernels stay the shading truth and the categories the wire projection. MaterialX's fourth `uaddressmode` member `constant` (a border colour outside `[0,1]`) has no `AddressMode` row by design — a border colour is sampler-level payload rather than a coordinate fold, so admitting it is one `AddressMode` row with a border-colour column on `SamplerState`, and the egress side never produces it, the named import edge.
- Boundary: UV coordinates enter as Rasm.Numerics `UnitInterval` pairs (the `[0,1]` validated value-object), image extents as `Dimension` (the `>=1` int-backed value-object), world position and normal as host `Vector3d`; the sampler NEVER re-mints a coordinate or extent primitive. `ShadeVec4` carries the interior noise/checker/gradient/image/triplanar algebra on one four-lane field register (`X`/`Y`/`Z` the scalar-field/color lanes, `W` the texel alpha the `Image` reconstruction premultiplies and `Channel.Scalar`/`Channel.Mask` reads) — `ShadeVec4` is the texture field carrier distinct from the `bsdf#LOBE_FAMILY` `RgbSpectrum` validated reflectance: a noise field is signed, a normal-map decode is `[-1,1]`, and a texel carries alpha, none of which the non-negative-validated `RgbSpectrum` admits, so the field stays the raw register and crosses to the validated reflectance only through `ShadeVec4.AsColor`. Color crosses the axis exactly once: color literals on `TextureSource` rows (`Low`/`High`, `Even`/`Odd`, gradient `Stops`) enter as `Unicolour` and decompose to `ShadeVec4` through `ShadeVec4.FromColor` for the field math — the gradient canonicalizes its authored stops sorted-by-position and pre-resolves them through `Unicolour.Mix(other, ColourSpace.Oklch, amount, HueSpan.Shorter)` into the `Lut` texel run at `Gradient.Of` construction, so the perceptual hue path (never the linear-RGB lerp that bends hue through the grey dead zone) is priced ONCE off the hot path, an unsorted authored list cannot mangle the bracketing walk, and the per-sample read is an index-lerp between adjacent resolved texels — and the single `ShadeVec4.AsColor` adapter constructs the canonical scene-linear `Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z)` at the projection tail — the sampler NEVER mints a second color register. `AddressMode.Apply` folds a raw continuous UV into `[0,1)` once before any non-image filter touches a coordinate, and image reconstruction addresses exclusively through the discrete `AddressMode.Texel` companion so the wrap arithmetic is consulted once per axis, not double-applied at the mip seam; `FilterMode` reconstructs through one weight algebra (`Nearest` snaps by `Floor(u·w)`, `Bilinear` is the unit-square lerp, `Bicubic` is the separable Catmull-Rom 4×4 convolution, `Trilinear` blends two `ReconstructLevel` taps across the mip pyramid by the fractional level `SampleImage` decomposes — `ReconstructLevel` itself dispatches only the spatial `Nearest`/`Bilinear`/`Bicubic` kernels, each snapped to the `MipLevel`-nearest plane so the supplied level is dead for no filter row, the cross-level blend the `Trilinear` arm's own concern); a `NoisePeriod` on the `Noise` row makes the lattice exactly periodic by wrapping the integer cell index modulo the period before the prime multiply — the fractional position inside the cell is untouched, so the interpolant stays continuous across the wrap where a coordinate modulo cuts it — and the period rides the octave ladder through `NoisePeriod.Scaled`, multiplying by the same integral lacunarity the frequency does, so octave three wraps where octave three's own lattice repeats; the `NoiseLattice` carrier threads `(seed, period, cellular)` into every basis arm as ONE column set, the cellular fold hashes the WRAPPED neighbour while displacing the feature point at its UNWRAPPED coordinate (a wrapped displacement collapses every period boundary onto one point), and a warped periodic source displaces through the wrappable `Perlin` arm at the warp's own scaled period so the displacement field shares the tile; `Raster/tile#TILE_SYNTH` is the ORTHOGONAL owner — it heals an already-authored plane whose source admits no period, where this period makes the source periodic BY CONSTRUCTION, and a procedurally periodic field needs no heal, while `Raster/gpu#WGSL_KERNEL` `noiseField` lowers the same wrap at `f32` so the preview tiles where the bake tiles; the FastNoiseLite gradient/simplex/value/cellular kernels are author-folds over the hashed lattice (no managed lib owns 2D/3D coherent noise, the `LIBRARY_DEPTH` NOT_COVERED carve-out) with the published FNL anchors — `PrimeX`/`PrimeY`/`PrimeZ` lattice primes and the `0x27d4eb2d` hash multiplier, the quintic fade `6t⁵−15t⁴+10t³` (Perlin) and the Hermite `t²(3−2t)` (Value), the 2D simplex skew `(√3−1)/2` / unskew `(3−√3)/6` with the `99.83685446303647` bound, the 3D OpenSimplex2 rotation `r=(x+y+z)·2/3` two-cell fold with the `32.69428253173828125` bound, the Perlin normalizers `1.4247691104677813` (2D) and `0.964921414852142333984375` (3D), the 24-direction 2D and 12-edge-plus-published-tail 3D gradient cycles, and `ValCoord`'s square-then-`^ << 19` hash→`[-1,1]` projection — every table GENERATED from its defining sequence, never a transcribed literal blob; the fractal trajectory is the `FractalMode` per-octave `Step` row (`FBm` the signed `n·amp` sum damping by `(n+1)/2`, `Ridged` the `(1−2|n|)·amp` fold damping by `1−|n|`, `PingPong` the centred `(p−0.5)·2·amp` triangle fold at the source's `PingPongStrength` damping by `p`) under FNL's `Lerp(1, damp, WeightedStrength)·Gain` amplitude cascade opening at the fractal bounding `1/Σ Gainⁱ` — never a hardcoded linear sum, never a post-hoc normalize — and the `Fbm` self-base is unrepresentable, `NoiseBasis` excludes it; the cellular kernel folds the `CellularDistance` metric over the 3×3 (2D) or 3×3×3 (3D) feature neighbourhood displaced by unit offset vectors at the FNL jitter radii `0.43701595` (2D) / `0.39614353` (3D) scaled by `CellularParams.Jitter`, and projects the `CellularReturn` feature (`Distance` the F1 nearest, `Distance2` the F2, `Distance2Sub` the F2−F1 vein, `Distance2Add`/`Distance2Mul`/`Distance2Div` the FNL blend trio, `CellValue` the per-cell hash), so the `Worley` arm spans the full cellular family rather than the single F1 distance; the `ProceduralNoise` hash-lattice fills, the fixed neighbourhood / three-corner (2D) and rotated two-cell (3D) simplex loops, the span tap kernels (`NearestTap`/`BilinearTap`/`BicubicTap`), and the per-source sampling folds (`SampleImage`/`SampleTriplanar`/`Gradient.Resolve`) are the page's `[EXPRESSION_SPINE]` kernel exemption, in-place by index over the per-shade hot path; triplanar projects a world point onto the three axis planes, wraps each through the sampler's `AddressMode.Apply` (never a parallel `Frac`), and blends by the squared-normal weight so the same `TextureSource` evaluates without a UV unwrap, and a `Noise` source under triplanar samples the 3D basis arm directly so solid noise needs no plane projection; the image pyramid admits ONCE at `Image.Of` — each flat level lifts through `AsMemory2D(height, width)` into a `ReadOnlyMemory2D<ShadeVec4>` plane whose `Height`/`Width` are structural facts, a payload/extent mismatch faulting at admission so the per-tap reconstruction carries no re-check and the prior per-sample undersized fault is unrepresentable; a stopless gradient, an empty pyramid, or a degenerate triplanar normal rails to `MaterialFault` through the deep `Sample` rail, a non-finite field lane rails at `ShadeVec4.AsColor` (`IsFinite` spans all four lanes — a corrupt coverage lane is as degenerate as a corrupt color lane), and the `Port` closure folds any fault OR non-finite field to the `Channel` neutral so the graph arm never sees a sentinel or NaN texel — the raw host `World`/`Normal` doubles are the one lane the `UnitInterval` coordinate admission cannot gate, so the finite fold at the projection tail is the boundary that keeps the graph closure total over them.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;              // CultureInfo (the MaterialX invariant-culture attribute text)
using CommunityToolkit.HighPerformance;  // ReadOnlyMemory2D/ReadOnlySpan2D (the mip-plane owner), AsMemory2D
using LanguageExt;                       // Seq, Option, Fin
using Rasm.Domain;                       // Op (the boundary-admission key)
using Rasm.Numerics;                      // UnitInterval, Dimension (the [0,1] / >=1 kernel value-objects)
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault (band 2450)
using Rasm.Materials.Appearance.Graph;   // PortValue (the scene-linear Configuration owner — PortValue.SceneLinear)
using Rhino.Geometry;                    // Vector3d, Point3d
using Thinktecture;
using Wacton.Unicolour;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Texture;

// --- [TYPES] -------------------------------------------------------------------------------
// MtlxAddress is the MaterialX 1.39 image/tiledimage uaddressmode/vaddressmode enum string each mode maps to.
[SmartEnum<int>]
public sealed partial class AddressMode {
    public static readonly AddressMode Repeat = new(0, mtlxAddress: "periodic", apply: static t => t - Math.Floor(t));
    public static readonly AddressMode Clamp  = new(1, mtlxAddress: "clamp",    apply: static t => Math.Clamp(t, 0.0, 1.0));
    public static readonly AddressMode Mirror = new(2, mtlxAddress: "mirror",   apply: static t => 1.0 - Math.Abs((((t % 2.0) + 2.0) % 2.0) - 1.0));
    public string MtlxAddress { get; }

    [UseDelegateFromConstructor]
    public partial double Apply(double t);

    public int Texel(int i, int extent) =>
        Switch(
            repeat: _ => ((i % extent) + extent) % extent,
            clamp:  _ => Math.Clamp(i, 0, extent - 1),
            mirror: _ => { int period = 2 * extent; int m = ((i % period) + period) % period; return m < extent ? m : period - 1 - m; });
}

// MtlxFilter is the MaterialX 1.39 image/tiledimage filtertype enum string; MaterialX carries closest/linear/cubic, so Trilinear maps to
// linear (the mip blend is the sampler's mip-pyramid concern, not a MaterialX filtertype). ReconstructLevel dispatches the spatial
// Nearest/Bilinear/Bicubic kernel per level; Trilinear is the cross-level blend SampleImage owns over two Bilinear taps, never a per-level arm.
[SmartEnum<int>]
public sealed partial class FilterMode {
    public static readonly FilterMode Nearest   = new(0, mtlxFilter: "closest");
    public static readonly FilterMode Bilinear  = new(1, mtlxFilter: "linear");
    public static readonly FilterMode Bicubic   = new(2, mtlxFilter: "cubic");
    public static readonly FilterMode Trilinear = new(3, mtlxFilter: "linear");
    public string MtlxFilter { get; }
}

// MtlxNode is the MaterialX 1.39 standard-library node category each basis round-trips to. The leaf-basis quartet covers the
// gradient (Perlin→noise2d), simplex (Simplex→fractal2d — MaterialX folds simplex under the fractal node; the vendored kernel
// is FNL OpenSimplex2, the LIBRARY_DEPTH NOT_COVERED carve-out), lattice/value (Value→cellnoise2d, the value-noise
// analogue interchange#MATERIALX_DOCUMENT enumerates), and cellular (Worley→worleynoise2d) families; ValueCubic is one more row.
// Wrappable is the SEAMLESS-TILING column: a lattice basis (Perlin gradient, Value hash, Worley cellular) evaluates
// on the integer grid, so wrapping the CELL INDEX modulo a period makes the field exactly periodic; OpenSimplex2
// evaluates on a SKEWED simplex grid whose cells do not align with any integer wrap, so the same modulo would seam
// along the skew diagonal — a periodic Simplex source is refused at admission rather than sold as approximately
// seamless. The column is data, so a future lattice basis answers it rather than growing a branch in the gate.
[SmartEnum<int>]
public sealed partial class NoiseBasis {
    public static readonly NoiseBasis Perlin  = new(0, mtlxNode: "noise2d",        wrappable: true,  sample2D: ProceduralNoise.Perlin2D,  sample3D: ProceduralNoise.Perlin3D);
    public static readonly NoiseBasis Simplex = new(1, mtlxNode: "fractal2d",      wrappable: false, sample2D: ProceduralNoise.Simplex2D, sample3D: ProceduralNoise.Simplex3D);
    public static readonly NoiseBasis Value   = new(2, mtlxNode: "cellnoise2d",    wrappable: true,  sample2D: ProceduralNoise.Value2D,   sample3D: ProceduralNoise.Value3D);
    public static readonly NoiseBasis Worley  = new(3, mtlxNode: "worleynoise2d",  wrappable: true,  sample2D: ProceduralNoise.Worley2D,  sample3D: ProceduralNoise.Worley3D);
    public string MtlxNode { get; }
    public bool Wrappable { get; }

    [UseDelegateFromConstructor]
    public partial double Sample2D(double x, double y, NoiseLattice lattice);
    [UseDelegateFromConstructor]
    public partial double Sample3D(double x, double y, double z, NoiseLattice lattice);
}

// FractalMode closes the octave trajectory (FastNoiseLite FractalType), FNL-exact: FBm sums signed n·amp damping by (n+1)/2
// (min-clamped at 2), Ridged sums (1−2|n|)·amp damping by 1−|n|, PingPong sums the centred (p−0.5)·2·amp triangle fold at the
// source's PingPongStrength (FNL SetFractalPingPongStrength, default 2) damping by p. Step returns BOTH per-octave outputs — the
// contribution and the damp base the amp *= Lerp(1, damp, WeightedStrength)·Gain cascade consumes — so one octave loop drives every
// trajectory, FNL's normalization being the fractal-bounding amp opening rather than a post-sum normalize column.
[SmartEnum<int>]
public sealed partial class FractalMode {
    public static readonly FractalMode FBm      = new(0, step: static (n, amp, _) => (n * amp, Math.Min(n + 1.0, 2.0) * 0.5));
    public static readonly FractalMode Ridged   = new(1, step: static (n, amp, _) => { double f = Math.Abs(n); return ((f * -2.0 + 1.0) * amp, 1.0 - f); });
    public static readonly FractalMode PingPong = new(2, step: static (n, amp, strength) => { double p = PingPongWave((n + 1.0) * strength); return ((p - 0.5) * 2.0 * amp, p); });

    [UseDelegateFromConstructor]
    public partial (double Contribution, double Damp) Step(double sample, double amplitude, double pingPongStrength);

    private static double PingPongWave(double t) { t -= (int)(t * 0.5) * 2.0; return t < 1.0 ? t : 2.0 - t; }
}

// CellularDistance closes the metric (FastNoiseLite CellularDistanceFunction): EuclideanSq the squared default, Euclidean the rooted,
// Manhattan the L1 taxicab, Hybrid the L1+L2 combination. Metric accumulates one neighbour's distance into the running F1/F2 minima.
[SmartEnum<int>]
public sealed partial class CellularDistance {
    public static readonly CellularDistance EuclideanSq = new(0, metric: static (dx, dy, dz) => dx * dx + dy * dy + dz * dz);
    public static readonly CellularDistance Euclidean   = new(1, metric: static (dx, dy, dz) => Math.Sqrt(dx * dx + dy * dy + dz * dz));
    public static readonly CellularDistance Manhattan   = new(2, metric: static (dx, dy, dz) => Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz));
    public static readonly CellularDistance Hybrid      = new(3, metric: static (dx, dy, dz) => Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) + (dx * dx + dy * dy + dz * dz));

    [UseDelegateFromConstructor]
    public partial double Metric(double dx, double dy, double dz);
}

// CellularReturn closes the feature projection (FastNoiseLite CellularReturnType) over the full seven-row FNL set: CellValue the
// per-cell hash, Distance the F1, Distance2 the F2, Distance2Sub/Add/Mul/Div the F2∓/×/÷F1 vein-blob-ratio family — each row
// FNL-exact (Mul carries the 0.5 product scale, Div the F1/F2 ratio with a zero-denominator floor so the projection stays total),
// centred to [-1,1] for the field.
[SmartEnum<int>]
public sealed partial class CellularReturn {
    public static readonly CellularReturn CellValue    = new(0, project: static (f1, f2, cell) => cell);
    public static readonly CellularReturn Distance     = new(1, project: static (f1, f2, cell) => f1 - 1.0);
    public static readonly CellularReturn Distance2    = new(2, project: static (f1, f2, cell) => f2 - 1.0);
    public static readonly CellularReturn Distance2Add = new(3, project: static (f1, f2, cell) => (f2 + f1) * 0.5 - 1.0);
    public static readonly CellularReturn Distance2Sub = new(4, project: static (f1, f2, cell) => f2 - f1 - 1.0);
    public static readonly CellularReturn Distance2Mul = new(5, project: static (f1, f2, cell) => f2 * f1 * 0.5 - 1.0);
    public static readonly CellularReturn Distance2Div = new(6, project: static (f1, f2, cell) => f2 > 0.0 ? f1 / f2 - 1.0 : 0.0);

    [UseDelegateFromConstructor]
    public partial double Project(double f1, double f2, double cellHash);
}

// Channel closes the graph-node sample modality: a Texture node reads ONE channel of the field, so Port projects the ShadeVec4 to a
// PortValue by this row — Color the RGB triple, Scalar the AP1-primary luminance mask, Mask the alpha lane (the texel coverage a
// graph#MATERIAL_GRAPH Mix factor or a weathering#WEATHERING ApplySlab cavity-occlusion UnitInterval reads), Vector the signed
// normal-map decode. Each row carries the neutral fallback the total graph closure folds a fault to, so a degenerate sample is a
// defined zero-mask / mid-grey rather than a propagated NaN; a new modality is one row, never a second Port overload.
[SmartEnum<int>]
public sealed partial class Channel {
    public static readonly Channel Color  = new(0, neutral: static () => new PortValue.Color(ShadeVec4.Splat(0.0).AsColorUnchecked()), project: static v => new PortValue.Color(v.AsColorUnchecked()));
    public static readonly Channel Scalar = new(1, neutral: static () => new PortValue.Scalar(0.0),                                    project: static v => new PortValue.Scalar(v.Luminance));
    public static readonly Channel Mask   = new(2, neutral: static () => new PortValue.Scalar(0.0),                                    project: static v => new PortValue.Scalar(Math.Clamp(v.W, 0.0, 1.0)));
    public static readonly Channel Vector = new(3, neutral: static () => new PortValue.Vector(new Vector3d(0.0, 0.0, 1.0)),            project: static v => new PortValue.Vector(new Vector3d(v.X, v.Y, v.Z)));

    [UseDelegateFromConstructor]
    public partial PortValue Project(ShadeVec4 field);
    [UseDelegateFromConstructor]
    public partial PortValue Neutral();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureSource {
    private TextureSource() { }

    // Noise carries its leaf basis, fractal trajectory, and cellular feature algebra as rows — frequency/octaves/lacunarity/gain span
    // its fBm spectrum, FractalMode the octave fold, WeightedStrength the octave damp, PingPongStrength the FNL triangle-fold scale
    // (FBm/Ridged rows carry it unread, the CellularParams precedent), CellularParams the Worley feature family, Warp the pre-sample
    // domain-warp displacement, Solid the 3D-vs-2D arm, Period the periodic lattice wrap. A new procedural variation is a parameter
    // change, never a second source. Of is the ONE admission and it exists FOR the period: every other column is total on its own
    // domain, while a period couples the basis, the lacunarity, the frequency, and the warp frequency into one integrality contract
    // no single column can hold — an unadmitted periodic source renders a plausible field that seams at the tile edge.
    public sealed record Noise(
        NoiseBasis Base, double Frequency, int Octaves, double Lacunarity, double Gain, int Seed,
        FractalMode Fractal, double WeightedStrength, double PingPongStrength, CellularParams Cellular, DomainWarp Warp, bool Solid,
        NoisePeriod Period, Unicolour Low, Unicolour High) : TextureSource {

        // Aperiodic sources admit unconditionally — the gates below all read Period.Periodic, so the FNL default
        // path pays one predicate. Every reason token is the exact string the [PERIOD_GOLDEN] refusal rows assert.
        public static Fin<Noise> Of(Noise candidate, Op key) =>
            !candidate.Period.Periodic
                ? Fin.Succ(candidate)
                : from _ in guard(candidate.Base.Wrappable, MaterialFault.Parameter(key, $"<noise-period-unwrappable-basis:{candidate.Base.MtlxNode}>"))
                  // Lacunarity multiplies the lattice period at every octave, so a fractional value wraps octave 1
                  // onward at a cell boundary the grid does not have and the sum seams.
                  from __ in guard(Integral(candidate.Lacunarity), MaterialFault.Parameter(key, $"<noise-period-fractional-lacunarity:{candidate.Lacunarity:R}>"))
                  // Frequency sets the lattice units the UV unit square spans and the field repeats every Period
                  // units, so the tile closes only when that span is a whole number of periods. 4.5 periods per tile is a seam.
                  from ___ in guard(Integral(candidate.Frequency) && (int)candidate.Frequency % candidate.Period.Value is 0,
                          MaterialFault.Parameter(key, $"<noise-period-frequency-not-multiple:{candidate.Frequency:R}:{candidate.Period.Value}>"))
                  // Warp displaces a periodic source by a field sharing its tile: an integral warp frequency keeps
                  // that displacement periodic on the same domain, and the warp then rides the periodic Perlin arm
                  // rather than the unwrappable Simplex one.
                  from ____ in guard(candidate.Warp.Amplitude <= 0.0 || Integral(candidate.Warp.Frequency),
                          MaterialFault.Parameter(key, $"<noise-period-fractional-warp-frequency:{candidate.Warp.Frequency:R}>"))
                  select candidate;

        static bool Integral(double v) => double.IsInteger(v) && v is > 0.0 and <= int.MaxValue;
    }
    public sealed record Checker(int Repeats, Unicolour Even, Unicolour Odd) : TextureSource;

    // Gradient pre-resolves its stops ONCE at Of: stops canonicalize SORTED by position (Resolve walks bracketing pairs,
    // so an unsorted authored list silently mangles the LUT), then each Lut texel finds its bracketing stops and mixes
    // through Unicolour.Mix in Oklch (HueSpan.Shorter) — perceptual hue path priced at admission, sample an index-lerp.
    // Lut is a Seq so the case keeps structural record equality.
    public sealed record Gradient(bool Vertical, Seq<(UnitInterval At, Unicolour Color)> Stops, Seq<ShadeVec4> Lut) : TextureSource {
        const int LutTexels = 64;

        public static Gradient Of(bool vertical, Seq<(UnitInterval At, Unicolour Color)> stops) =>
            toSeq(stops.OrderBy(static s => s.At.Value)) switch {
                var sorted => new(vertical, sorted, sorted.IsEmpty
                    ? Seq<ShadeVec4>()
                    : toSeq(Enumerable.Range(0, LutTexels)).Map(i => Resolve(sorted, i / (LutTexels - 1.0)))),
            };

        static ShadeVec4 Resolve(Seq<(UnitInterval At, Unicolour Color)> stops, double t) {
            (UnitInterval At, Unicolour Color) lo = stops[0];
            if (t <= lo.At.Value) { return ShadeVec4.FromColor(lo.Color); }
            foreach ((UnitInterval At, Unicolour Color) hi in stops.Tail) {
                if (t <= hi.At.Value) {
                    double span = hi.At.Value - lo.At.Value;
                    return ShadeVec4.FromColor(span > double.Epsilon
                        ? lo.Color.Mix(hi.Color, ColourSpace.Oklch, (t - lo.At.Value) / span, HueSpan.Shorter)
                        : hi.Color);
                }
                lo = hi;
            }
            return ShadeVec4.FromColor(lo.Color);
        }
    }

    // Image admits its pyramid ONCE: each flat level lifts through AsMemory2D(height, width) into a plane whose Height/Width
    // are structural facts — a payload/extent mismatch faults HERE, never per sample. Texture import owns the box-filter
    // downsample generating the pyramid, never the sampler.
    public sealed record Image(Seq<ReadOnlyMemory2D<ShadeVec4>> Levels) : TextureSource {
        public static Fin<Image> Of(Dimension width, Dimension height, Seq<ReadOnlyMemory<ShadeVec4>> levels, Op key) =>
            levels.IsEmpty
                ? MaterialFault.Parameter(key, "<texture-image-empty>")
                : levels.Map((flat, index) => {
                      int w = Math.Max(1, width.Value >> index), h = Math.Max(1, height.Value >> index);
                      return flat.Length == w * h
                          ? Fin.Succ(flat.AsMemory2D(h, w))
                          : Fin.Fail<ReadOnlyMemory2D<ShadeVec4>>(MaterialFault.Parameter(key, $"<texture-level-extent:{index}:{flat.Length}!={w * h}>"));
                  }).Traverse(identity).As().Map(static planes => new Image(planes));
    }

    public sealed record Triplanar(TextureSource Projected, double Scale, double BlendSharpness) : TextureSource;

    // MtlxCategory names the MaterialX 1.39 standard-library node category each TextureSource round-trips to
    // (interchange#MATERIALX_DOCUMENT consumes via Mtlx.CategoryOf): a fractal noise (Octaves>1 over a gradient/value basis) is the
    // fractal2d node, a single-octave noise its NoiseBasis.MtlxNode, a checker→checkerboard, a gradient→ramplr/ramptb by axis, an
    // image→tiledimage, a triplanar→triplanarprojection — the case IS the category, and every returned string is a closed
    // NodeCategory key the document resolves. FractalMode carries no MaterialX enum, so a Ridged/PingPong row renders host-side and
    // crosses the wire as fractal2d, the named lossy edge.
    public string MtlxCategory => Switch(
        noise:     static n => n.Octaves > 1 && n.Base != NoiseBasis.Worley ? "fractal2d" : n.Base.MtlxNode,
        checker:   static _ => "checkerboard",
        gradient:  static g => g.Vertical ? "ramptb" : "ramplr",
        image:     static _ => "tiledimage",
        triplanar: static _ => "triplanarprojection");

    // MtlxParameters projects the per-case MaterialX 1.39 inputs beside MtlxCategory — one (name, MaterialX-type, attribute-text)
    // row per procedural parameter the interchange#MATERIALX_DOCUMENT texture arm folds into MtlxInputs (image/triplanar carry
    // filename slots there, not parameters). fractal2d reads octaves/lacunarity/diminish (Gain the FNL per-octave amplitude damp),
    // worleynoise2d the cellular jitter, checkerboard the two colours, ramplr/ramptb the endpoint stops (At=0 left/bottom, At=1
    // right/top) — the SAME fractal-vs-worley dispatch MtlxCategory keys on, so category and parameters never disagree, and a
    // single-octave non-worley noise floors to the stdlib node defaults. Colours ground to the PortValue.SceneLinear working space
    // (the wire's color3 convention) BEFORE rendering, never a raw-config relabel.
    public Seq<(string Name, string Type, string Value)> MtlxParameters => Switch(
        noise:     static n => n.Base == NoiseBasis.Worley
            ? Seq(("jitter", "float", Num(n.Cellular.Jitter)))
            : n.Octaves > 1
                ? Seq(("octaves", "integer", n.Octaves.ToString(CultureInfo.InvariantCulture)),
                      ("lacunarity", "float", Num(n.Lacunarity)),
                      ("diminish", "float", Num(n.Gain)))
                : Seq<(string, string, string)>(),
        checker:   static c => Seq(("color1", "color3", Rgb(c.Even)), ("color2", "color3", Rgb(c.Odd))),
        gradient:  static g => g.Stops.IsEmpty
            ? Seq<(string, string, string)>()
            : g.Vertical
                ? Seq(("valueb", "color3", Rgb(g.Stops[0].Color)), ("valuet", "color3", Rgb(g.Stops[g.Stops.Count - 1].Color)))
                : Seq(("valuel", "color3", Rgb(g.Stops[0].Color)), ("valuer", "color3", Rgb(g.Stops[g.Stops.Count - 1].Color))),
        image:     static _ => Seq<(string, string, string)>(),
        triplanar: static _ => Seq<(string, string, string)>());

    static string Num(double v) => v.ToString("R", CultureInfo.InvariantCulture);

    // Scene-linear color3 render: ground the authored colour to PortValue.SceneLinear (the wire color3 convention) THEN
    // read RgbLinear — an sRGB-config RgbLinear relabel would hue-shift the emitted attribute (the surface#TONE_MAP law).
    static string Rgb(Unicolour c) =>
        c.ConvertToConfiguration(PortValue.SceneLinear).RgbLinear.Triplet switch { var t => $"{Num(t.First)}, {Num(t.Second)}, {Num(t.Third)}" };
}

// --- [MODELS] ------------------------------------------------------------------------------
// CellularParams carries the feature algebra a Noise source threads into the Worley basis arm: distance metric, returned feature, and
// feature-point jitter. A non-Worley basis ignores it (the Perlin/Simplex/Value arms discard the parameter), so one parameter shape rides
// every basis without a Worley-only field, and CellularParams.Default is the EuclideanSq F1 a non-cellular Noise source carries unread.
public readonly record struct CellularParams(CellularDistance Distance, CellularReturn Return, double Jitter) {
    public static readonly CellularParams Default = new(CellularDistance.EuclideanSq, CellularReturn.Distance, 1.0);
}

// DomainWarp pre-displaces the sample coordinate (FastNoiseLite DomainWarp/DomainWarpAmp): a non-zero Amp offsets it by a warp vector
// x + amp·simplex(x·freq, y·freq) before the basis samples, so a procedural pattern flows and swirls rather than reads on an axis
// grid. Amp 0 (DomainWarp.None) is the identity.
public readonly record struct DomainWarp(double Amplitude, double Frequency, int Seed) {
    public static readonly DomainWarp None = new(0.0, 1.0, 0);
}

// NoisePeriod counts the tiling lattice period in LATTICE CELLS, a capability FastNoiseLite itself ships no form of — its lattice is
// unbounded. Zero is aperiodic (the FNL default) and a positive value wraps every integer cell index modulo itself before the hash
// reads it, so the hashed corner of cell P is the hashed corner of cell 0 and the field is exactly periodic rather than blended
// toward seamlessness. Wrapping the INDEX (never the coordinate) keeps the interpolant continuous across the wrap: the fractional
// position inside the cell is untouched, so no gradient discontinuity appears at the seam a coordinate modulo introduces. Scaled
// carries the period up the octave ladder, where lacunarity multiplies the coordinate and therefore multiplies the period the same
// whole number of times.
[ValueObject<int>]
public readonly partial struct NoisePeriod {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 0) { validationError = new ValidationError($"<noise-period-negative:{value}>"); }
    }

    public static readonly NoisePeriod Aperiodic = Create(0);
    public static NoisePeriod Of(int value) => Create(value);
    public bool Periodic => Value > 0;
    public NoisePeriod Scaled(int lacunarity) => Value > 0 ? Create(Value * Math.Max(1, lacunarity)) : this;
    public int Wrap(int cell) => Value > 0 ? ((cell % Value) + Value) % Value : cell;
}

// NoiseLattice threads the per-octave lattice policy into every basis arm as ONE carrier: octave seed, octave period, and cellular
// feature algebra. One carrier makes the next lattice knob a COLUMN rather than another parameter on eight kernel signatures, and
// every arm reads exactly the columns its own lattice needs.
public readonly record struct NoiseLattice(int Seed, NoisePeriod Period, CellularParams Cellular) {
    public static NoiseLattice Of(int seed) => new(seed, NoisePeriod.Aperiodic, CellularParams.Default);
}

// ShadeVec4 is the one texture field register: X/Y/Z the scalar-field/color/vector lanes, W the texel alpha (coverage) the Image
// reconstruction premultiplies and Channel.Mask reads. Distinct from the bsdf#LOBE_FAMILY RgbSpectrum validated reflectance — a field
// is signed (normal-map decode), alpha-bearing (texel coverage), and unvalidated, so it crosses to the validated scene-linear
// Unicolour only at AsColor.
public readonly record struct ShadeVec4(double X, double Y, double Z, double W) {
    public static ShadeVec4 Splat(double v) => new(v, v, v, v);
    public static ShadeVec4 operator +(ShadeVec4 a, ShadeVec4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static ShadeVec4 operator *(ShadeVec4 a, double s) => new(a.X * s, a.Y * s, a.Z * s, a.W * s);
    public static ShadeVec4 Lerp(ShadeVec4 a, ShadeVec4 b, double t) => a * (1.0 - t) + b * t;
    public double Luminance => 0.2722287 * X + 0.6740818 * Y + 0.0536895 * Z;   // AP1-primary scene-linear Y, the graph#MATERIAL_GRAPH Color→Scalar projection — X/Y/Z are the Acescg-linear triple, so a Rec709 weight here mis-weights green

    // IsFinite is the one finiteness predicate over all four lanes — a corrupt coverage lane is as degenerate as a corrupt
    // color lane (premultiply divides by W; Channel.Mask reads it) — read by AsColor and the Port neutral fold.
    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z) && double.IsFinite(W);

    public static ShadeVec4 FromColor(Unicolour colour) {
        ColourTriplet lin = colour.RgbLinear.Triplet;
        return new(lin.First, lin.Second, lin.Third, 1.0);
    }

    // AsColor is the validated egress: a non-finite lane rails MaterialFault.Gamut; AsColorUnchecked serves the total Channel
    // projection where the Port closure has already folded a fault or non-finite field to a neutral, so the graph arm never re-checks.
    public Fin<Unicolour> AsColor(Op key) =>
        IsFinite
            ? Fin.Succ(AsColorUnchecked())
            : MaterialFault.Gamut(key, $"<texture-non-finite-field:{X:R},{Y:R},{Z:R},{W:R}>");
    public Unicolour AsColorUnchecked() => new(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z);
}

// MipLevel is the ABSOLUTE fractional pyramid level the consumer supplies (no derivative machinery exists to bias) —
// SampleImage clamps it, snaps the spatial filter rows to the nearest plane, and decomposes it into the two Trilinear
// taps; naming it a bias implied a computed base level.
public readonly record struct UvSample(UnitInterval U, UnitInterval V, Vector3d World, Vector3d Normal, double MipLevel) {
    public static Fin<UvSample> Of(double u, double v, Op key) =>
        from cu in key.AcceptValidated<UnitInterval>(candidate: u)
        from cv in key.AcceptValidated<UnitInterval>(candidate: v)
        select new UvSample(cu, cv, Vector3d.Zero, Vector3d.ZAxis, 0.0);

    // At is the total re-anchor: u/v arrive already address-wrapped into [0,1] (the AddressMode fold idempotent on a wrapped value),
    // so its clamp is the structural totality guarantee that keeps UnitInterval.Create unreachable-throwing — the Port closure and
    // triplanar feed wrapped coordinates, the clamp the proof no raw node UV reaches the throwing factory, and Of (the
    // AcceptValidated Fin rail) is the deep entry.
    public UvSample At(double u, double v) => this with { U = UnitInterval.Create(Math.Clamp(u, 0.0, 1.0)), V = UnitInterval.Create(Math.Clamp(v, 0.0, 1.0)) };
}

public readonly record struct SamplerState(AddressMode AddressU, AddressMode AddressV, FilterMode Filter) {
    public static readonly SamplerState Default = new(AddressMode.Repeat, AddressMode.Repeat, FilterMode.Bilinear);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ProceduralNoise {
    private const int PrimeX = 501125321;
    private const int PrimeY = 1136930381;
    private const int PrimeZ = 1720413743;
    private const double CellJitter2D = 0.43701595;   // FNL cellular jitter radius over the unit offset vector, 2D
    private const double CellJitter3D = 0.39614353;   // FNL cellular jitter radius, 3D

    private static int Hash(int seed, int xPrimed, int yPrimed) { int h = seed ^ xPrimed ^ yPrimed; h *= 0x27d4eb2d; return h; }
    private static int Hash(int seed, int xPrimed, int yPrimed, int zPrimed) { int h = seed ^ xPrimed ^ yPrimed ^ zPrimed; h *= 0x27d4eb2d; return h; }
    private static double ValCoord(int hash) { hash *= hash; hash ^= hash << 19; return hash * (1.0 / 2147483648.0); }   // FNL square-xor hash → [-1,1]
    private static int Round(double v) => v >= 0.0 ? (int)(v + 0.5) : (int)(v - 0.5);   // FNL FastRound — half-away-from-zero, never banker's
    private static int Cell(int index, NoisePeriod period) => period.Wrap(index);       // the ONE wrap site every lattice kernel primes through
    private const double WarpDecorrelation = 1000.0;                                     // the constant offset decorrelating the warp's second and third reads; a constant shift preserves periodicity

    private static double GradCoord(int seed, int xPrimed, int yPrimed, double xd, double yd) {
        int hash = Hash(seed, xPrimed, yPrimed); hash ^= hash >> 15; hash &= 127 << 1;
        return xd * Gradients2D[hash] + yd * Gradients2D[hash | 1];
    }
    private static double GradCoord(int seed, int xPrimed, int yPrimed, int zPrimed, double xd, double yd, double zd) {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed); hash ^= hash >> 15; hash &= 63 << 2;
        return xd * Gradients3D[hash] + yd * Gradients3D[hash | 1] + zd * Gradients3D[hash | 2];
    }

    private static double Fade(double t) => t * t * t * (t * (t * 6.0 - 15.0) + 10.0);   // Perlin quintic
    private static double Hermite(double t) => t * t * (3.0 - 2.0 * t);                  // FNL Value-noise interpolant
    private static double Lerp(double a, double b, double t) => a + t * (b - a);

    // --- [PERLIN]
    // Every lattice kernel primes the WRAPPED cell index: Cell(i, period) folds the index into [0, period) before the
    // prime multiply, so the hash of cell P is the hash of cell 0 and the field repeats exactly. The wrap is applied
    // to i and i+1 INDEPENDENTLY (never to a pre-multiplied prime), which is what makes the last cell of a period
    // interpolate into the first rather than off the end of the lattice.
    public static double Perlin2D(double x, double y, NoiseLattice lattice) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
        double xd0 = x - x0, yd0 = y - y0, xd1 = xd0 - 1.0, yd1 = yd0 - 1.0;
        double xs = Fade(xd0), ys = Fade(yd0);
        int xp0 = Cell(x0, lattice.Period) * PrimeX, yp0 = Cell(y0, lattice.Period) * PrimeY;
        int xp1 = Cell(x0 + 1, lattice.Period) * PrimeX, yp1 = Cell(y0 + 1, lattice.Period) * PrimeY;
        int seed = lattice.Seed;
        double n00 = GradCoord(seed, xp0, yp0, xd0, yd0), n10 = GradCoord(seed, xp1, yp0, xd1, yd0);
        double n01 = GradCoord(seed, xp0, yp1, xd0, yd1), n11 = GradCoord(seed, xp1, yp1, xd1, yd1);
        return Lerp(Lerp(n00, n10, xs), Lerp(n01, n11, xs), ys) * 1.4247691104677813;
    }
    public static double Perlin3D(double x, double y, double z, NoiseLattice lattice) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y), z0 = (int)Math.Floor(z);
        double xd0 = x - x0, yd0 = y - y0, zd0 = z - z0, xd1 = xd0 - 1.0, yd1 = yd0 - 1.0, zd1 = zd0 - 1.0;
        double xs = Fade(xd0), ys = Fade(yd0), zs = Fade(zd0);
        int xp0 = Cell(x0, lattice.Period) * PrimeX, yp0 = Cell(y0, lattice.Period) * PrimeY, zp0 = Cell(z0, lattice.Period) * PrimeZ;
        int xp1 = Cell(x0 + 1, lattice.Period) * PrimeX, yp1 = Cell(y0 + 1, lattice.Period) * PrimeY, zp1 = Cell(z0 + 1, lattice.Period) * PrimeZ;
        int seed = lattice.Seed;
        double n000 = GradCoord(seed, xp0, yp0, zp0, xd0, yd0, zd0), n100 = GradCoord(seed, xp1, yp0, zp0, xd1, yd0, zd0);
        double n010 = GradCoord(seed, xp0, yp1, zp0, xd0, yd1, zd0), n110 = GradCoord(seed, xp1, yp1, zp0, xd1, yd1, zd0);
        double n001 = GradCoord(seed, xp0, yp0, zp1, xd0, yd0, zd1), n101 = GradCoord(seed, xp1, yp0, zp1, xd1, yd0, zd1);
        double n011 = GradCoord(seed, xp0, yp1, zp1, xd0, yd1, zd1), n111 = GradCoord(seed, xp1, yp1, zp1, xd1, yd1, zd1);
        double xf0 = Lerp(Lerp(n000, n100, xs), Lerp(n010, n110, xs), ys), xf1 = Lerp(Lerp(n001, n101, xs), Lerp(n011, n111, xs), ys);
        return Lerp(xf0, xf1, zs) * 0.964921414852142333984375;
    }

    // --- [SIMPLEX]
    // FNL OpenSimplex2 vendors this lattice, never Perlin's patented Simplex (US 6,867,776, expired 2022, moot regardless): Simplex2D
    // is FNL's SingleSimplex three-corner skewed simplex — skew (√3−1)/2, unskew (3−√3)/6, three a⁴·grad corners at the FNL
    // 99.83685446303647 bound tuned to its 24-direction unit-gradient table (the skew is linear, so the in-kernel spelling equals
    // FNL's hoisted TransformNoiseCoordinate under per-octave lacunarity); Simplex3D is FNL's rotated two-cell fold —
    // r = (x+y+z)·2/3, FastRound cell anchors, sign-flip second cell, the 32.69428253173828125 bound, never the classic four-corner
    // skew. Both arms read the octave SEED alone: the period column is unreachable here because Noise.Of refuses a periodic Simplex
    // source outright, and wrapping the unskewed cell index seams along the skew diagonal.
    public static double Simplex2D(double x, double y, NoiseLattice lattice) {
        const double F2 = 0.3660254037844386, G2 = 0.21132486540518713;
        int seed = lattice.Seed;
        double s = (x + y) * F2;
        int i = (int)Math.Floor(x + s), j = (int)Math.Floor(y + s);
        double t = (i + j) * G2, x0 = x - (i - t), y0 = y - (j - t);
        int i1 = x0 >= y0 ? 1 : 0, j1 = x0 >= y0 ? 0 : 1;   // FNL tie-break: the equal-diagonal picks the x-corner
        double x1 = x0 - i1 + G2, y1 = y0 - j1 + G2, x2 = x0 - 1.0 + 2.0 * G2, y2 = y0 - 1.0 + 2.0 * G2;
        int ip = i * PrimeX, jp = j * PrimeY;
        double Corner(double dx, double dy, int xp, int yp) { double a = 0.5 - dx * dx - dy * dy; if (a <= 0.0) { return 0.0; } a *= a; a *= a; return a * GradCoord(seed, xp, yp, dx, dy); }
        return 99.83685446303647 * (Corner(x0, y0, ip, jp) + Corner(x1, y1, ip + i1 * PrimeX, jp + j1 * PrimeY) + Corner(x2, y2, ip + PrimeX, jp + PrimeY));
    }
    public static double Simplex3D(double x, double y, double z, NoiseLattice lattice) {
        int seed = lattice.Seed;
        double r = (x + y + z) * (2.0 / 3.0);
        double xr = r - x, yr = r - y, zr = r - z;
        int i = Round(xr), j = Round(yr), k = Round(zr);
        double x0 = xr - i, y0 = yr - j, z0 = zr - k;
        int xSign = (int)(-1.0 - x0) | 1, ySign = (int)(-1.0 - y0) | 1, zSign = (int)(-1.0 - z0) | 1;
        double ax0 = xSign * -x0, ay0 = ySign * -y0, az0 = zSign * -z0;
        int ip = i * PrimeX, jp = j * PrimeY, kp = k * PrimeZ;
        double value = 0.0, a = (0.6 - x0 * x0) - (y0 * y0 + z0 * z0);
        for (int l = 0; ; l++) {
            if (a > 0.0) { double a2 = a * a; value += a2 * a2 * GradCoord(seed, ip, jp, kp, x0, y0, z0); }
            if (ax0 >= ay0 && ax0 >= az0) { double b = a + ax0 + ax0; if (b > 1.0) { b -= 1.0; double b2 = b * b; value += b2 * b2 * GradCoord(seed, ip - xSign * PrimeX, jp, kp, x0 + xSign, y0, z0); } }
            else if (ay0 > ax0 && ay0 >= az0) { double b = a + ay0 + ay0; if (b > 1.0) { b -= 1.0; double b2 = b * b; value += b2 * b2 * GradCoord(seed, ip, jp - ySign * PrimeY, kp, x0, y0 + ySign, z0); } }
            else { double b = a + az0 + az0; if (b > 1.0) { b -= 1.0; double b2 = b * b; value += b2 * b2 * GradCoord(seed, ip, jp, kp - zSign * PrimeZ, x0, y0, z0 + zSign); } }
            if (l == 1) { break; }
            ax0 = 0.5 - ax0; ay0 = 0.5 - ay0; az0 = 0.5 - az0;
            x0 = xSign * ax0; y0 = ySign * ay0; z0 = zSign * az0;
            a += 0.75 - ax0 - ay0 - az0;
            ip += (xSign >> 1) & PrimeX; jp += (ySign >> 1) & PrimeY; kp += (zSign >> 1) & PrimeZ;
            xSign = -xSign; ySign = -ySign; zSign = -zSign;
            seed = ~seed;
        }
        return value * 32.69428253173828125;
    }

    // --- [VALUE]
    // FNL Value noise interpolates with the Hermite t²(3−2t), NOT the Perlin quintic — the softer basis is the anchor.
    public static double Value2D(double x, double y, NoiseLattice lattice) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y), seed = lattice.Seed;
        double xs = Hermite(x - x0), ys = Hermite(y - y0);
        int xp0 = Cell(x0, lattice.Period) * PrimeX, yp0 = Cell(y0, lattice.Period) * PrimeY;
        int xp1 = Cell(x0 + 1, lattice.Period) * PrimeX, yp1 = Cell(y0 + 1, lattice.Period) * PrimeY;
        double v00 = ValCoord(Hash(seed, xp0, yp0)), v10 = ValCoord(Hash(seed, xp1, yp0));
        double v01 = ValCoord(Hash(seed, xp0, yp1)), v11 = ValCoord(Hash(seed, xp1, yp1));
        return Lerp(Lerp(v00, v10, xs), Lerp(v01, v11, xs), ys);
    }
    public static double Value3D(double x, double y, double z, NoiseLattice lattice) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y), z0 = (int)Math.Floor(z), seed = lattice.Seed;
        double xs = Hermite(x - x0), ys = Hermite(y - y0), zs = Hermite(z - z0);
        int xp0 = Cell(x0, lattice.Period) * PrimeX, yp0 = Cell(y0, lattice.Period) * PrimeY, zp0 = Cell(z0, lattice.Period) * PrimeZ;
        int xp1 = Cell(x0 + 1, lattice.Period) * PrimeX, yp1 = Cell(y0 + 1, lattice.Period) * PrimeY, zp1 = Cell(z0 + 1, lattice.Period) * PrimeZ;
        double v000 = ValCoord(Hash(seed, xp0, yp0, zp0)), v100 = ValCoord(Hash(seed, xp1, yp0, zp0));
        double v010 = ValCoord(Hash(seed, xp0, yp1, zp0)), v110 = ValCoord(Hash(seed, xp1, yp1, zp0));
        double v001 = ValCoord(Hash(seed, xp0, yp0, zp1)), v101 = ValCoord(Hash(seed, xp1, yp0, zp1));
        double v011 = ValCoord(Hash(seed, xp0, yp1, zp1)), v111 = ValCoord(Hash(seed, xp1, yp1, zp1));
        return Lerp(Lerp(Lerp(v000, v100, xs), Lerp(v010, v110, xs), ys), Lerp(Lerp(v001, v101, xs), Lerp(v011, v111, xs), ys), zs);
    }

    // --- [WORLEY]
    // Worley2D/Worley3D accumulate the CellularDistance metric over the 3×3 (2D) / 3×3×3 (3D) feature neighbourhood — each feature
    // point displaced by a hash-indexed UNIT offset vector at the FNL jitter radius × CellularParams.Jitter (radius < 0.5, so F1/F2
    // correctness inside the fixed neighbourhood holds) — into the two nearest distances F1/F2 and the F1 cell hash (FNL CellValue:
    // closestHash·2⁻³¹), then project the CellularReturn feature. One neighbourhood loop spans the closed cellular family, never a
    // per-return kernel: Distance2Sub over Euclidean is the classic crack pattern, CellValue the cobblestone, Distance the
    // Voronoi-cell border. Each fold hashes the WRAPPED neighbour but displaces the feature point at its UNWRAPPED coordinate, so the
    // distance metric still measures against the neighbour actually beside this texel while the jitter offset it reads is the one
    // cell 0 reads — a wrapped displacement collapses every period boundary onto one point.
    public static double Worley2D(double x, double y, NoiseLattice lattice) {
        int xr = Round(x), yr = Round(y), seed = lattice.Seed;
        CellularParams cellular = lattice.Cellular;
        double f1 = double.MaxValue, f2 = double.MaxValue, cell = 0.0, jitter = CellJitter2D * cellular.Jitter;
        for (int dy = -1; dy <= 1; dy++) {
            for (int dx = -1; dx <= 1; dx++) {
                int cx = xr + dx, cy = yr + dy;
                int h = Hash(seed, Cell(cx, lattice.Period) * PrimeX, Cell(cy, lattice.Period) * PrimeY), idx = h & (255 << 1);
                double fx = cx + RandVecs2D[idx] * jitter, fy = cy + RandVecs2D[idx | 1] * jitter;
                double d = cellular.Distance.Metric(fx - x, fy - y, 0.0);
                if (d < f1) { f2 = f1; f1 = d; cell = h * (1.0 / 2147483648.0); } else if (d < f2) { f2 = d; }
            }
        }
        return Math.Clamp(cellular.Return.Project(f1, f2, cell), -1.0, 1.0);
    }
    public static double Worley3D(double x, double y, double z, NoiseLattice lattice) {
        int xr = Round(x), yr = Round(y), zr = Round(z), seed = lattice.Seed;
        CellularParams cellular = lattice.Cellular;
        double f1 = double.MaxValue, f2 = double.MaxValue, cell = 0.0, jitter = CellJitter3D * cellular.Jitter;
        for (int dz = -1; dz <= 1; dz++) {
            for (int dy = -1; dy <= 1; dy++) {
                for (int dx = -1; dx <= 1; dx++) {
                    int cx = xr + dx, cy = yr + dy, cz = zr + dz;
                    int h = Hash(seed, Cell(cx, lattice.Period) * PrimeX, Cell(cy, lattice.Period) * PrimeY, Cell(cz, lattice.Period) * PrimeZ), idx = h & (255 << 2);
                    double fx = cx + RandVecs3D[idx] * jitter, fy = cy + RandVecs3D[idx | 1] * jitter, fz = cz + RandVecs3D[idx | 2] * jitter;
                    double d = cellular.Distance.Metric(fx - x, fy - y, fz - z);
                    if (d < f1) { f2 = f1; f1 = d; cell = h * (1.0 / 2147483648.0); } else if (d < f2) { f2 = d; }
                }
            }
        }
        return Math.Clamp(cellular.Return.Project(f1, f2, cell), -1.0, 1.0);
    }

    // --- [FRACTAL]
    // Evaluate folds FNL's fractal over the Noise row: amp opens at the fractal bounding 1/Σ Gainⁱ (the octave sum lands in [-1,1]
    // with NO post-normalize), the FractalMode row yields the per-octave contribution and damp base, and the cascade is
    // amp *= Lerp(1, damp, WeightedStrength) · Gain — FNL's loop verbatim. Domain warp pre-displaces the frequency-scaled coordinate
    // when Amplitude > 0 so the pattern flows; octave o samples at seed + o (FNL seed++). Its octave ladder carries the PERIOD
    // alongside the frequency: octave o samples at freq·lacunarityᵒ, so its lattice repeats every period·lacunarityᵒ cells and the
    // two scale in lockstep. Noise.Of proves the lacunarity integral for a periodic source, so the cast is exact there and dead on the
    // aperiodic path where Scaled is the identity — one ladder serves both, with no periodic-vs-aperiodic fold and no second Evaluate.
    public static double Evaluate(TextureSource.Noise n, double u, double v) {
        (double x, double y) = (u * n.Frequency, v * n.Frequency);
        if (n.Warp.Amplitude > 0.0) { (x, y) = Warp2D(x, y, n.Warp, n.Period); }
        int octaves = Math.Max(1, n.Octaves), step = (int)n.Lacunarity;
        double sum = 0.0, amp = FractalBounding(n.Gain, octaves), freq = 1.0;
        NoisePeriod period = n.Period;
        for (int o = 0; o < octaves; o++) {
            (double c, double damp) = n.Fractal.Step(n.Base.Sample2D(x * freq, y * freq, new NoiseLattice(n.Seed + o, period, n.Cellular)), amp, n.PingPongStrength);
            sum += c;
            amp *= (1.0 + (damp - 1.0) * n.WeightedStrength) * n.Gain;
            freq *= n.Lacunarity;
            period = period.Scaled(step);
        }
        return sum;
    }
    public static double Evaluate(TextureSource.Noise n, double px, double py, double pz) {
        (double x, double y, double z) = (px * n.Frequency, py * n.Frequency, pz * n.Frequency);
        if (n.Warp.Amplitude > 0.0) { (x, y, z) = Warp3D(x, y, z, n.Warp, n.Period); }
        int octaves = Math.Max(1, n.Octaves), step = (int)n.Lacunarity;
        double sum = 0.0, amp = FractalBounding(n.Gain, octaves), freq = 1.0;
        NoisePeriod period = n.Period;
        for (int o = 0; o < octaves; o++) {
            (double c, double damp) = n.Fractal.Step(n.Base.Sample3D(x * freq, y * freq, z * freq, new NoiseLattice(n.Seed + o, period, n.Cellular)), amp, n.PingPongStrength);
            sum += c;
            amp *= (1.0 + (damp - 1.0) * n.WeightedStrength) * n.Gain;
            freq *= n.Lacunarity;
            period = period.Scaled(step);
        }
        return sum;
    }

    private static double FractalBounding(double gain, int octaves) {
        double g = Math.Abs(gain), amp = g, total = 1.0;
        for (int i = 1; i < octaves; i++) { total += amp; amp *= g; }
        return 1.0 / total;
    }

    // Warp2D/Warp3D are the one named non-FNL kernel: FNL warps through a gradient dual-lookup (GradCoordDual over RandVecs), while
    // these vendor the offset-sample form — decorrelated reads of ONE basis displace the coordinate — same capability, one basis
    // reused, no second gradient table pathway. Under a PERIOD the displacement rides the wrappable Perlin arm at the warp's own
    // scaled period (Noise.Of proves the warp frequency integral), so the displacement field shares the tile and the warped source
    // stays exactly periodic; an unwarped-period source and an aperiodic warp both reach their FNL-default behaviour through the
    // same two expressions, never a periodic-only sibling kernel.
    private static (double, double) Warp2D(double x, double y, in DomainWarp w, NoisePeriod period) {
        NoiseLattice lattice = new(w.Seed, period.Scaled((int)w.Frequency), CellularParams.Default);
        (double wx, double wy) = (x * w.Frequency, y * w.Frequency);
        return period.Periodic
            ? (x + w.Amplitude * Perlin2D(wx, wy, lattice), y + w.Amplitude * Perlin2D(wx + WarpDecorrelation, wy, lattice))
            : (x + w.Amplitude * Simplex2D(wx, wy, lattice), y + w.Amplitude * Simplex2D(wx + WarpDecorrelation, wy, lattice));
    }
    private static (double, double, double) Warp3D(double x, double y, double z, in DomainWarp w, NoisePeriod period) {
        NoiseLattice lattice = new(w.Seed, period.Scaled((int)w.Frequency), CellularParams.Default);
        (double wx, double wy, double wz) = (x * w.Frequency, y * w.Frequency, z * w.Frequency);
        return period.Periodic
            ? (x + w.Amplitude * Perlin3D(wx, wy, wz, lattice),
               y + w.Amplitude * Perlin3D(wx + WarpDecorrelation, wy, wz, lattice),
               z + w.Amplitude * Perlin3D(wx, wy + WarpDecorrelation, wz, lattice))
            : (x + w.Amplitude * Simplex3D(wx, wy, wz, lattice),
               y + w.Amplitude * Simplex3D(wx + WarpDecorrelation, wy, wz, lattice),
               z + w.Amplitude * Simplex3D(wx, wy + WarpDecorrelation, wz, lattice));
    }

    // --- [TABLES]
    // Every lattice table GENERATES from its defining sequence — never a transcribed literal blob. Gradients2D is the
    // FNL 128-pair cycle of the 24 unit directions at 82.5° − 15°·k (odd multiples of 7.5°); Gradients3D is the FNL
    // 64-quad table — the 12 (±1,±1,0)-family edge vectors cycled ×5 plus FNL's published 4-entry tail; RandVecs2D/3D
    // regenerate FNL's 256 hardcoded random unit offsets as golden-angle / spherical-Fibonacci unit sets — the same
    // table shape and lookup math with a deterministic spread, the one generated-table deviation from the FNL source.
    // Declaration order is load-bearing: the seed rows precede the table fields their builders read at type-init.
    private const double GoldenAngle = 2.399963229728653;
    private static readonly (double X, double Y, double Z)[] Edges3D = [
        (0, 1, 1), (0, -1, 1), (0, 1, -1), (0, -1, -1), (1, 0, 1), (-1, 0, 1), (1, 0, -1), (-1, 0, -1), (1, 1, 0), (-1, 1, 0), (1, -1, 0), (-1, -1, 0)];
    private static readonly (double X, double Y, double Z)[] Tail3D = [(1, 1, 0), (0, -1, 1), (-1, 1, 0), (0, -1, -1)];
    private static readonly double[] Gradients2D = BuildGradients2D();
    private static readonly double[] Gradients3D = BuildGradients3D();
    private static readonly double[] RandVecs2D = BuildRandVecs2D();
    private static readonly double[] RandVecs3D = BuildRandVecs3D();

    private static double[] BuildGradients2D() {
        double[] table = new double[256];
        for (int p = 0; p < 128; p++) { double a = (82.5 - 15.0 * (p % 24)) * Math.PI / 180.0; table[2 * p] = Math.Cos(a); table[2 * p + 1] = Math.Sin(a); }
        return table;
    }
    private static double[] BuildGradients3D() {
        double[] table = new double[256];
        for (int q = 0; q < 64; q++) { (double x, double y, double z) = q < 60 ? Edges3D[q % 12] : Tail3D[q - 60]; (table[4 * q], table[4 * q + 1], table[4 * q + 2]) = (x, y, z); }
        return table;
    }
    private static double[] BuildRandVecs2D() {
        double[] table = new double[512];
        for (int i = 0; i < 256; i++) { double a = i * GoldenAngle; table[2 * i] = Math.Cos(a); table[2 * i + 1] = Math.Sin(a); }
        return table;
    }
    private static double[] BuildRandVecs3D() {
        double[] table = new double[1024];
        for (int i = 0; i < 256; i++) {
            double z = 1.0 - (2.0 * i + 1.0) / 256.0, r = Math.Sqrt(1.0 - z * z), a = i * GoldenAngle;
            (table[4 * i], table[4 * i + 1], table[4 * i + 2]) = (r * Math.Cos(a), r * Math.Sin(a), z);
        }
        return table;
    }
}

public static class TextureUv {
    // Sample is the deep field rail: a TextureSource sampled at a UvSample under a SamplerState to one ShadeVec4, the Op key
    // correlating the MaterialFault on a stopless gradient, an empty pyramid, or a degenerate triplanar normal — the UV lanes are
    // UnitInterval value-objects, finite-in-[0,1] by construction, so no coordinate re-validation exists. Color callers project
    // AsColor; the graph node binds through Port. Arity stays one — the union case discriminates the texture, never a sibling
    // sampler method.
    public static Fin<ShadeVec4> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key) =>
        source.Switch(
            state:     (point, sampler, key),
            noise:     static (s, n) => Fin.Succ(SampleNoise(n, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value), s.point)),
            checker:   static (s, c) => Fin.Succ(SampleChecker(c, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value))),
            gradient:  static (s, g) => SampleGradient(g, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value), s.key),
            image:     static (s, img) => SampleImage(img, s.point, s.sampler, s.key),
            triplanar: static (s, t) => SampleTriplanar(t, s.point, s.sampler, s.key));

    // Port bridges graph#MATERIAL_GRAPH AppearanceNode.Texture: capture the source/sampler/key and return the TOTAL (u,v)→PortValue
    // closure the node fold reads. That closure re-anchors the UvSample at (u,v), samples the deep rail, and projects the field
    // through the Channel; a fault OR a non-finite field folds to the Channel neutral (a raw World/Normal is host material — NaN
    // world coordinates yield a NaN lattice sample the projection must not smuggle into the graph) — the deep Sample rail owns the
    // MaterialFault, the closure owns totality.
    public static Func<double, double, PortValue> Port(TextureSource source, UvSample anchor, SamplerState sampler, Channel channel, Op key) =>
        (u, v) => Sample(source, anchor.At(sampler.AddressU.Apply(u), sampler.AddressV.Apply(v)), sampler, key)
            .Match(Succ: field => field.IsFinite ? channel.Project(field) : channel.Neutral(), Fail: _ => channel.Neutral());

    // Noise ROW carries the parameters: Evaluate reads its fractal/cellular/warp columns directly, so the fold takes one row argument
    // instead of a re-flattened twelve-knob signature. Solid routes to the 3D arm off UvSample.World, so wood, marble, and stone flow
    // through a carved surface rather than smearing at a UV seam.
    private static ShadeVec4 SampleNoise(TextureSource.Noise n, double u, double v, UvSample point) {
        double field = n.Solid
            ? ProceduralNoise.Evaluate(n, point.World.X, point.World.Y, point.World.Z)
            : ProceduralNoise.Evaluate(n, u, v);
        return ShadeVec4.Lerp(ShadeVec4.FromColor(n.Low), ShadeVec4.FromColor(n.High), Math.Clamp((field + 1.0) * 0.5, 0.0, 1.0));
    }

    private static ShadeVec4 SampleChecker(TextureSource.Checker c, double u, double v) {
        int parity = ((int)Math.Floor(u * c.Repeats) + (int)Math.Floor(v * c.Repeats)) & 1;
        return ShadeVec4.FromColor(parity == 0 ? c.Even : c.Odd);
    }

    // Gradient.Of already priced the perceptual work (Oklch Mix into the Lut), so SampleGradient index-lerps over resolved texels —
    // raw-linear only BETWEEN adjacent Lut entries, sub-texel error, never the hue-through-grey stop lerp.
    private static Fin<ShadeVec4> SampleGradient(TextureSource.Gradient g, double u, double v, Op key) {
        if (g.Lut.IsEmpty) { return MaterialFault.Parameter(key, "<texture-gradient-no-stops>"); }
        double t = Math.Clamp(g.Vertical ? v : u, 0.0, 1.0) * (g.Lut.Count - 1);
        int lo = (int)t;
        return Fin.Succ(lo >= g.Lut.Count - 1 ? g.Lut[g.Lut.Count - 1] : ShadeVec4.Lerp(g.Lut[lo], g.Lut[lo + 1], t - lo));
    }

    // Every filter row honours MipLevel: the spatial rows snap it to the NEAREST plane (a Bilinear minification at
    // level 3 must not alias off the full-res plane — the level is the anti-aliasing control, dead for no row), and
    // Trilinear decomposes the fractional level into its two Bilinear taps.
    private static Fin<ShadeVec4> SampleImage(TextureSource.Image img, UvSample point, SamplerState sampler, Op key) {
        if (img.Levels.IsEmpty) { return MaterialFault.Parameter(key, "<texture-image-empty>"); }
        double u = point.U.Value, v = point.V.Value;
        double level = Math.Clamp(double.IsFinite(point.MipLevel) ? point.MipLevel : 0.0, 0.0, img.Levels.Count - 1.0);
        if (sampler.Filter != FilterMode.Trilinear) { return Fin.Succ(ReconstructLevel(img.Levels[(int)Math.Floor(level + 0.5)], u, v, sampler, sampler.Filter)); }
        int lo = (int)Math.Floor(level), hi = Math.Min(lo + 1, img.Levels.Count - 1);
        return Fin.Succ(ShadeVec4.Lerp(
            ReconstructLevel(img.Levels[lo], u, v, sampler, FilterMode.Bilinear),
            ReconstructLevel(img.Levels[hi], u, v, sampler, FilterMode.Bilinear), level - lo));
    }

    // ReconstructLevel reconstructs one mip level by a spatial FilterMode over the level's OWN ReadOnlySpan2D plane — extent truth is
    // structural (Image.Of admitted the plane once, so no per-sample payload recheck exists and the function is TOTAL). Its
    // state-passing Switch keeps every arm STATIC (the bsdf closure-free per-sample law — a capturing lambda here allocates a closure
    // per texel reconstruction on the shade hot path) and each tap kernel materializes the plane's Span exactly once. SampleImage
    // routes the non-Trilinear filters here directly and the Trilinear mip blend feeds Bilinear per tap, so the trilinear arm is
    // unreachable (kept only for total-Switch exhaustiveness).
    private static ShadeVec4 ReconstructLevel(ReadOnlyMemory2D<ShadeVec4> plane, double u, double v, SamplerState sampler, FilterMode filter) =>
        filter.Switch(
            state:     (plane, u, v, sampler),
            nearest:   static (s, _) => NearestTap(s.plane.Span, s.u, s.v, s.sampler),
            bilinear:  static (s, _) => BilinearTap(s.plane.Span, s.u, s.v, s.sampler),
            bicubic:   static (s, _) => BicubicTap(s.plane.Span, s.u, s.v, s.sampler),
            trilinear: static (s, _) => BilinearTap(s.plane.Span, s.u, s.v, s.sampler));   // unreachable — SampleImage owns the Trilinear mip blend and feeds Bilinear per tap

    // Texels carry alpha in the W lane; Bilinear/Bicubic premultiply before the weighted sum and un-premultiply after,
    // so a transparent texel never bleeds opaque color across a coverage edge. A ref struct cannot be CAPTURED by a
    // local function, so the At/Row taps take the span as a parameter — the [EXPRESSION_SPINE] tap kernels.
    // Nearest is Floor(u·w) — texel i owns [i/w, (i+1)/w). Math.Round(u·w − 0.5) is NOT equivalent: banker's rounding
    // snaps exact texel edges toward even indices, an even-bias seam the floor form does not have.
    private static ShadeVec4 NearestTap(ReadOnlySpan2D<ShadeVec4> plane, double u, double v, SamplerState sampler) {
        int w = plane.Width, h = plane.Height;
        return plane[sampler.AddressV.Texel((int)Math.Floor(v * h), h), sampler.AddressU.Texel((int)Math.Floor(u * w), w)];
    }
    private static ShadeVec4 BilinearTap(ReadOnlySpan2D<ShadeVec4> plane, double u, double v, SamplerState sampler) {
        int w = plane.Width, h = plane.Height;
        double fx = u * w - 0.5, fy = v * h - 0.5;
        int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
        double tx = fx - x0, ty = fy - y0;
        ShadeVec4 At(ReadOnlySpan2D<ShadeVec4> p, int ix, int iy) => Pre(p[sampler.AddressV.Texel(iy, h), sampler.AddressU.Texel(ix, w)]);
        return UnPre(ShadeVec4.Lerp(
            ShadeVec4.Lerp(At(plane, x0, y0), At(plane, x0 + 1, y0), tx),
            ShadeVec4.Lerp(At(plane, x0, y0 + 1), At(plane, x0 + 1, y0 + 1), tx), ty));
    }
    private static ShadeVec4 BicubicTap(ReadOnlySpan2D<ShadeVec4> plane, double u, double v, SamplerState sampler) {
        int w = plane.Width, h = plane.Height;
        double fx = u * w - 0.5, fy = v * h - 0.5;
        int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
        double tx = fx - x0, ty = fy - y0;
        ShadeVec4 At(ReadOnlySpan2D<ShadeVec4> p, int ix, int iy) => Pre(p[sampler.AddressV.Texel(iy, h), sampler.AddressU.Texel(ix, w)]);
        ShadeVec4 Row(ReadOnlySpan2D<ShadeVec4> p, int iy) => CatmullRom(At(p, x0 - 1, iy), At(p, x0, iy), At(p, x0 + 1, iy), At(p, x0 + 2, iy), tx);
        return UnPre(CatmullRom(Row(plane, y0 - 1), Row(plane, y0), Row(plane, y0 + 1), Row(plane, y0 + 2), ty));
    }
    private static ShadeVec4 Pre(ShadeVec4 c) => new(c.X * c.W, c.Y * c.W, c.Z * c.W, c.W);
    private static ShadeVec4 UnPre(ShadeVec4 c) => c.W > 1e-6 ? new(c.X / c.W, c.Y / c.W, c.Z / c.W, c.W) : c;

    private static ShadeVec4 CatmullRom(ShadeVec4 p0, ShadeVec4 p1, ShadeVec4 p2, ShadeVec4 p3, double t) {
        double t2 = t * t, t3 = t2 * t;
        double w0 = -0.5 * t3 + t2 - 0.5 * t, w1 = 1.5 * t3 - 2.5 * t2 + 1.0, w2 = -1.5 * t3 + 2.0 * t2 + 0.5 * t, w3 = 0.5 * t3 - 0.5 * t2;
        return p0 * w0 + p1 * w1 + p2 * w2 + p3 * w3;
    }

    // Triplanar projects the world point onto the three axis planes, wraps each plane coordinate through the sampler's AddressMode (never a
    // parallel Frac), and blends by the squared-normal weight; a Noise projected source samples its 3D basis arm directly through the Solid
    // path so solid noise needs no plane projection. A degenerate (zero-length) normal rails MaterialFault rather than dividing by zero.
    private static Fin<ShadeVec4> SampleTriplanar(TextureSource.Triplanar t, UvSample point, SamplerState sampler, Op key) {
        Vector3d n = point.Normal;
        double ax = Math.Pow(Math.Abs(n.X), t.BlendSharpness), ay = Math.Pow(Math.Abs(n.Y), t.BlendSharpness), az = Math.Pow(Math.Abs(n.Z), t.BlendSharpness);
        double sum = ax + ay + az;
        if (sum <= double.Epsilon) { return MaterialFault.Parameter(key, "<triplanar-degenerate-normal>"); }
        Vector3d p = point.World * t.Scale;
        Fin<ShadeVec4> Plane(double a, double b) => Sample(t.Projected, point.At(sampler.AddressU.Apply(a), sampler.AddressV.Apply(b)) with { World = p }, sampler, key);
        return from x in Plane(p.Y, p.Z)
               from y in Plane(p.Z, p.X)
               from z in Plane(p.X, p.Y)
               select (x * (ax / sum)) + (y * (ay / sum)) + (z * (az / sum));
    }
}
```

## [03]-[PERIOD_GOLDEN]

- Owner: `PeriodGolden` the per-source fixture row; `PeriodProof` the fixture table and its one verdict fold.
- Cases: an ADMITTED row carries the shift at which its field must repeat; a REFUSED row carries the reason token `Noise.Of` must produce. One table, two verdicts on one column, so a refusal fixture cannot drift into a second roster nobody iterates.
- Entry: `public static Fin<Unit> Prove(PeriodGolden row, Op key)` runs the row's own verdict — admit-then-shift or refuse-with-reason — and `PeriodProof.All` is the roster the proof estate and the `Projection/benchmarks#WORKLOAD_ROWS` parity read both iterate, so a new periodic capability reaches both with no further edit.
- Law: `PeriodGolden` asserts the SHIFT EQUALITY itself, never a transcribed constant — `f(u + Δ, v) = f(u, v)` is exactly what periodicity means, so a regenerated gradient table, a re-tuned normalizer, or a widened basis roster cannot invalidate the fixture while a real seam always breaks it. `Tolerance` rides as a ROW column and its floor is arithmetic rather than algorithmic: the shift adds a whole number of lattice units to a coordinate whose fractional part then re-derives at a larger exponent, so the interpolant's own input loses low mantissa bits even where the wrapped cell index is bit-identical. Exact equality asserts a property IEEE arithmetic does not have.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new periodic capability is one admitted row; a new admission gate is one refused row carrying its reason token — never a second proof entry and never a fixture file, since every row's source is authored inline from its own columns.
- Boundary: `Shifted` sweeps a DETERMINISTIC stratified lattice — sample `i` reads `u = (i + ½)/n` against a coprime-strided `v`, so the sweep needs no RNG, carries no state outside `(row, n)`, and covers both axes of every cell in the period. `PeriodProof` reads the `ProceduralNoise.Evaluate` fold rather than a basis arm directly, so it grades the WHOLE evaluation chain — octave ladder, amplitude cascade, domain warp, cellular projection — exactly as a bake drives it; a fixture over one basis call passes while the ladder seams at octave two.

```csharp signature
// (Continues the Rasm.Materials.Appearance.Texture compilation unit — the [02] prelude is in scope.)

// --- [MODELS] ------------------------------------------------------------------------------
// One row states EITHER the shift a periodic source must repeat under OR the reason token its admission must
// refuse with. Refusal is Option-typed rather than a boolean beside a nullable string, so a row cannot declare a
// reason it does not assert or assert a shift it never admits to measure.
public readonly record struct PeriodGolden(
    string Name, TextureSource.Noise Draft, double ShiftU, double ShiftV, int Samples, double Tolerance, Option<string> Refusal);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class PeriodProof {
    // Reference-typed rows carry no constant default, so the optional columns arrive null and resolve to their own
    // roster defaults here — one authoring helper rather than eleven positional arguments per fixture row.
    static TextureSource.Noise Row(NoiseBasis basis, double frequency, int octaves, double lacunarity, int period,
        FractalMode? fractal = null, CellularParams? cellular = null, DomainWarp? warp = null, bool solid = false) =>
        new(basis, frequency, octaves, lacunarity, Gain: 0.5, Seed: 1337,
            Fractal: fractal ?? FractalMode.FBm, WeightedStrength: 0.0, PingPongStrength: 2.0,
            Cellular: cellular ?? CellularParams.Default, Warp: warp ?? DomainWarp.None, Solid: solid,
            Period: NoisePeriod.Of(period), Low: Grey(0.0), High: Grey(1.0));

    static Unicolour Grey(double v) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, v, v, v);

    // Every admitted row shifts by ONE UV tile: the source's frequency is a whole multiple of its period, so one
    // unit of u advances the lattice by that whole number of periods and the field must return. The 4-period rows
    // additionally prove the ladder — a lacunarity-2 fourth octave wraps at 32 cells, which a per-octave period
    // that failed to scale would miss while octave zero still matched.
    public static readonly Seq<PeriodGolden> All = Seq(
        new PeriodGolden("perlin.single",   Row(NoiseBasis.Perlin, frequency: 4.0,  octaves: 1, lacunarity: 2.0, period: 4),  ShiftU: 1.0, ShiftV: 0.0, Samples: 512, Tolerance: 1e-12, Refusal: None),
        new PeriodGolden("perlin.fbm4",     Row(NoiseBasis.Perlin, frequency: 4.0,  octaves: 4, lacunarity: 2.0, period: 4),  ShiftU: 1.0, ShiftV: 0.0, Samples: 512, Tolerance: 1e-12, Refusal: None),
        new PeriodGolden("perlin.ridged",   Row(NoiseBasis.Perlin, frequency: 8.0,  octaves: 3, lacunarity: 2.0, period: 4, fractal: FractalMode.Ridged),   ShiftU: 0.5, ShiftV: 0.5, Samples: 512, Tolerance: 1e-12, Refusal: None),
        new PeriodGolden("value.pingpong",  Row(NoiseBasis.Value,  frequency: 6.0,  octaves: 2, lacunarity: 3.0, period: 3, fractal: FractalMode.PingPong), ShiftU: 0.0, ShiftV: 1.0, Samples: 512, Tolerance: 1e-12, Refusal: None),
        // Row worley.vein proves the wrapped-hash / unwrapped-displacement split: a wrapped feature point collapses
        // the period boundary onto one cell and this shift then reads a different F1 there.
        new PeriodGolden("worley.vein",     Row(NoiseBasis.Worley, frequency: 4.0,  octaves: 1, lacunarity: 2.0, period: 4, cellular: new CellularParams(CellularDistance.Euclidean, CellularReturn.Distance2Sub, 1.0)), ShiftU: 1.0, ShiftV: 0.0, Samples: 512, Tolerance: 1e-12, Refusal: None),
        // Row perlin.warped proves the displacement rides the periodic Perlin arm at its own scaled period.
        new PeriodGolden("perlin.warped",   Row(NoiseBasis.Perlin, frequency: 4.0,  octaves: 2, lacunarity: 2.0, period: 4, warp: new DomainWarp(0.35, 2.0, 99)), ShiftU: 1.0, ShiftV: 1.0, Samples: 512, Tolerance: 1e-12, Refusal: None),
        // Row value.solid proves the 3D arm wraps every axis: a Z-unwrapped lattice tiles in UV and seams in depth.
        new PeriodGolden("value.solid",     Row(NoiseBasis.Value,  frequency: 4.0,  octaves: 1, lacunarity: 2.0, period: 4, solid: true), ShiftU: 1.0, ShiftV: 0.0, Samples: 512, Tolerance: 1e-12, Refusal: None),
        new PeriodGolden("refuse.simplex",  Row(NoiseBasis.Simplex, frequency: 4.0, octaves: 1, lacunarity: 2.0, period: 4),  ShiftU: 0.0, ShiftV: 0.0, Samples: 0, Tolerance: 0.0, Refusal: Some("<noise-period-unwrappable-basis")),
        new PeriodGolden("refuse.lacunarity", Row(NoiseBasis.Perlin, frequency: 4.0, octaves: 3, lacunarity: 2.5, period: 4), ShiftU: 0.0, ShiftV: 0.0, Samples: 0, Tolerance: 0.0, Refusal: Some("<noise-period-fractional-lacunarity")),
        new PeriodGolden("refuse.frequency",  Row(NoiseBasis.Perlin, frequency: 6.0, octaves: 1, lacunarity: 2.0, period: 4), ShiftU: 0.0, ShiftV: 0.0, Samples: 0, Tolerance: 0.0, Refusal: Some("<noise-period-frequency-not-multiple")),
        new PeriodGolden("refuse.warpfreq",   Row(NoiseBasis.Perlin, frequency: 4.0, octaves: 1, lacunarity: 2.0, period: 4, warp: new DomainWarp(0.35, 1.5, 99)), ShiftU: 0.0, ShiftV: 0.0, Samples: 0, Tolerance: 0.0, Refusal: Some("<noise-period-fractional-warp-frequency")));

    // Prove is the ONE verdict: the row's own Refusal column decides which proof runs, so an admitted row that ought to refuse
    // and a refused row that admits are both failures with the row named, never a silent pass.
    public static Fin<Unit> Prove(PeriodGolden row, Op key) =>
        TextureSource.Noise.Of(row.Draft, key).Match(
            Succ: source => row.Refusal.Match(
                Some: reason => Fin.Fail<Unit>(MaterialFault.Parameter(key, $"<period-golden-admitted-refusable:{row.Name}:{reason}>")),
                None: () => Shifted(source, row, key)),
            Fail: error => row.Refusal.Match(
                Some: reason => error.Message.Contains(reason, StringComparison.Ordinal)
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(MaterialFault.Parameter(key, $"<period-golden-wrong-refusal:{row.Name}:{error.Message}>")),
                None: () => Fin.Fail<Unit>(error)));

    // Stratified sweep with a coprime v-stride: no RNG, no fixture file, both axes of every cell covered. A solid
    // source reads the 3D fold at the same coordinates lifted to a plane so the Z axis carries the shift too.
    static Fin<Unit> Shifted(TextureSource.Noise source, PeriodGolden row, Op key) =>
        toSeq(Enumerable.Range(0, row.Samples))
            .Map(i => (U: (i + 0.5) / row.Samples, V: ((i * 7919) % row.Samples + 0.5) / row.Samples))
            .Map(at => (At: at, Delta: Math.Abs(Field(source, at.U, at.V) - Field(source, at.U + row.ShiftU, at.V + row.ShiftV))))
            .Filter(probe => !(probe.Delta <= row.Tolerance))
            .HeadOrNone()
            .Match(
                Some: probe => Fin.Fail<Unit>(MaterialFault.Parameter(key,
                    $"<period-golden-seam:{row.Name}:u={probe.At.U:R}:v={probe.At.V:R}:delta={probe.Delta:R}>")),
                None: static () => Fin.Succ(unit));

    static double Field(TextureSource.Noise source, double u, double v) =>
        source.Solid
            ? ProceduralNoise.Evaluate(source, u, v, u + v)
            : ProceduralNoise.Evaluate(source, u, v);
}
```

## [04]-[RESEARCH]

- [SIMPLEX_PATENT]: the noise kernel is FastNoiseLite OpenSimplex2, not Perlin's patented Simplex — the patent (US 6,867,776, expired 2022) is moot regardless. The 2D arm is FNL's `SingleSimplex` (FNL's own note: the 2D OpenSimplex2 case IS the ordinary-simplex algorithm) — skew `(√3−1)/2`, unskew `(3−√3)/6`, three `a⁴·grad` corners, the `99.83685446303647` bound tuned to FNL's 24-direction unit-gradient table; the 3D arm is FNL's rotated two-cell fold — `r=(x+y+z)·2/3` coordinate rotation (never the classic four-corner skew), `FastRound` cell anchors, the sign-flip second cell, the `32.69428253173828125` bound. Both in-kernel coordinate transforms are linear, so per-octave application under lacunarity equals FNL's hoisted `TransformNoiseCoordinate`. `ValueCubic` and `OpenSimplex2S` stay the two named deferred rows — each one `NoiseBasis` row binding one `ProceduralNoise.Sample2D`/`Sample3D` arm pair, admitted when a consumer contract demands the smoother lattice.
- [NOISE_DIMENSIONALITY]: REALIZED — every `NoiseBasis` carries both a `Sample2D` and a `Sample3D` arm, so the 3D gradient/value/simplex/cellular lattice is live rather than a built-unused table. A `Noise` source with `Solid = true` samples the 3D arm from the `UvSample.World` position (solid wood/marble/stone that flows through a carved surface rather than smearing at a UV seam), and triplanar samples the 2D arm on each axis plane; the normalizers are the FNL source constants — `1.4247691104677813` (Perlin 2D), `0.964921414852142333984375` (Perlin 3D) — and the gradient tables generate the FNL sequences exactly: the 24-direction 2D cycle at `82.5° − 15°·k`, the 12-edge-vector 3D cycle plus FNL's published 4-entry tail. The MaterialX `noise3d` (`NodeCategory.Perlin3D`) category is the solid-noise wire target the `interchange#MATERIALX_DOCUMENT` resolves.
- [CELLULAR_FEATURE_FAMILY]: REALIZED — the `Worley` arm is the full FastNoiseLite cellular family rather than the single F1 distance: `CellularDistance` (`EuclideanSq`/`Euclidean`/`Manhattan`/`Hybrid`) is the metric the neighbourhood fold accumulates, `CellularReturn` the complete seven-row `CellularReturnType` set (`CellValue`/`Distance`/`Distance2`/`Distance2Add`/`Distance2Sub`/`Distance2Mul`/`Distance2Div` — `Mul` at FNL's `·0.5` product scale, `Div` the F1/F2 ratio), and `CellularParams.Jitter` the feature-point displacement scaling FNL's unit-offset radii `0.43701595` (2D) / `0.39614353` (3D) — radii below 0.5, so F1/F2 correctness inside the fixed 3×3/3×3×3 neighbourhood holds by construction. The classic Worley crack pattern is `Distance2Sub` over `Euclidean`, a cobblestone `CellValue` (FNL's `closestHash·2⁻³¹`), a Voronoi-cell border `Distance` — all rows of one neighbourhood loop, never a per-return kernel. The offset vectors regenerate FNL's 256 hardcoded random unit vectors as golden-angle (2D) / spherical-Fibonacci (3D) sets — same table shape, same `hash & (255<<1|2)` lookup, deterministic spread; the one generated-table deviation.
- [FRACTAL_TRAJECTORY]: REALIZED FNL-exact — the fractal accumulation opens at the fractal bounding `1/Σ Gainⁱ` (the octave sum lands in [-1,1] with no post-hoc normalize; the prior running-norm divide was dead for every `Gain < 1`) and folds the closed `FractalMode` axis per octave: `FBm` sums `n·amp` damping by `min(n+1, 2)·0.5`, `Ridged` sums `(1−2|n|)·amp` damping by `1−|n|` for sharp ridgelines, `PingPong` sums the centred `(p−0.5)·2·amp` triangle fold at the source's `PingPongStrength` (FNL `SetFractalPingPongStrength`, default 2) damping by `p` — each damp entering as `amp *= Lerp(1, damp, WeightedStrength)·Gain`, the two per-octave outputs one `Step` row delegate. The `MtlxCategory` projects a fractal noise (`Octaves > 1` over a gradient/value basis) to the MaterialX `fractal2d` node for every basis, `Worley` staying `worleynoise2d`; the trajectory itself has no MaterialX enum — a `Ridged`/`PingPong` row renders host-side and crosses the wire as `fractal2d`, the named lossy edge.
- [SEAMLESS_PERIOD]: REALIZED — `NoisePeriod` is the FNL capability FastNoiseLite itself never shipped: the upstream lattice is unbounded, so an authored procedural could only be tiled by healing the rendered plane afterwards. Wrapping the integer CELL INDEX modulo a period before the prime multiply makes the hashed corner of cell `P` the hashed corner of cell `0` while the fractional in-cell position stays untouched, so the field is exactly periodic AND the interpolant stays continuous — a coordinate-space modulo would achieve the first and destroy the second. Periodicity is a chain property, not a basis property, which is what `Noise.Of` exists to prove: the octave ladder multiplies the period by the lacunarity, so a fractional lacunarity wraps octave one at a cell boundary the grid does not have; the UV unit square spans `Frequency` lattice units, so the tile closes only where that span is a whole number of periods; and a domain warp displaces by a second field that must share the same tile, which routes the displacement onto the wrappable `Perlin` arm at the warp's own scaled period. OpenSimplex2 is the one refused basis — its cells are a SKEWED triangulation whose boundaries meet no integer wrap, so the same modulo seams along the skew diagonal and a periodic `Simplex` source refuses rather than rendering approximate seamlessness under an exact name. `Raster/tile#TILE_SYNTH` stays the orthogonal owner for planes whose source cannot be made periodic — an ingested photograph, an image source, a Simplex field — and a procedurally periodic source needs no heal at all; `Raster/gpu#WGSL_KERNEL` `noiseField` lowers the same wrap at `f32`, so the preview tiles where the bake tiles. `[03]-[PERIOD_GOLDEN]` grades the whole evaluation chain rather than a basis call.
- [DOMAIN_WARP]: the `DomainWarp` field on the `Noise` source pre-displaces the frequency-scaled coordinate by a Simplex-warp vector (`x + amp·simplex(x·freq, y·freq)`) when `Amplitude > 0`, so a procedural pattern flows and swirls rather than reading on an axis grid; `Amplitude = 0` (`DomainWarp.None`) is the identity. This is the ONE named non-FNL kernel: FNL warps through the `GradCoordDual` gradient dual-lookup, this page through three decorrelated offset Simplex reads — the same `DomainWarp`/`DomainWarpAmp` capability with one basis reused and no second gradient pathway, vendored as the inline `Warp2D`/`Warp3D` displacement the `Evaluate` fold consumes, never a second sampler instance.
- [MIP_GENERATION]: the `Image` case carries a pre-built mip pyramid admitted ONCE at `Image.Of` — each flat level lifts through `AsMemory2D(height, width)` into a `ReadOnlyMemory2D<ShadeVec4>` plane, a payload/extent mismatch railing `<texture-level-extent>` at admission, so `ReconstructLevel` is total over structurally-true planes and the prior per-sample `<texture-level-undersized>` recheck is unrepresentable; the box-filter downsample that generates the pyramid is the consumer's responsibility at texture import, not a sampler concern. The texel `W` lane is the coverage alpha the `Bilinear`/`Bicubic` reconstruction premultiplies and un-premultiplies so a transparent texel never bleeds opaque color across a coverage edge, and `Channel.Mask` reads it as the per-texel coverage a `graph#MATERIAL_GRAPH` `Mix` factor or a `weathering#WEATHERING` cavity mask consumes.
- [GRAPH_NODE_BRIDGE]: REALIZED — the `graph#MATERIAL_GRAPH` `AppearanceNode.Texture(Func<double,double,PortValue> Sample)` node binds the `TextureUv.Port` closure: `Port` mints the TOTAL `(u,v)→PortValue` closure from a `TextureSource` + `SamplerState` + `Channel` + `Op key`, the deep `TextureUv.Sample` rail carrying the `MaterialFault` and the closure folding a fault or a non-finite field (`ShadeVec4.IsFinite`, all four lanes) to the `Channel` neutral so the graph arm never propagates a sentinel or a NaN. `Channel` (`Color`/`Scalar`/`Mask`/`Vector`) is the sample modality — a color texture drives `Color`, a roughness/AO mask `Scalar` or `Mask`, a normal map `Vector` — so one bridge serves every `Texture` node without a per-channel `Port` overload; the `weathering#WEATHERING` `ApplySlab` cavity-occlusion `UnitInterval` is the `Channel.Mask`/`Channel.Scalar` read of a cavity/AO `Texture` node. The remaining calibration is the per-input port-name alignment per MaterialX node, the `System.Xml.Linq` `.mtlx` serialize at the host boundary the `interchange#MATERIALX_DOCUMENT` owns, not a re-architecture of the sampling fold.
- [MATERIALX_NODE_PARITY]: REALIZED against the MaterialX 1.39 standard-node library — the `TextureSource.MtlxCategory`/`NoiseBasis.MtlxNode`/`AddressMode.MtlxAddress`/`FilterMode.MtlxFilter` projections each resolve to a closed `interchange#MATERIALX_DOCUMENT` `NodeCategory` row: `NoiseBasis` maps `Perlin→noise2d`/`Simplex→fractal2d`/`Value→cellnoise2d`/`Worley→worleynoise2d`, a fractal `Noise` source to `fractal2d`, `Checker→checkerboard`, `Gradient→ramplr`/`ramptb` by axis, `Image→tiledimage`, `Triplanar→triplanarprojection`, `AddressMode` to the `uaddressmode`/`vaddressmode` enum (`periodic`/`clamp`/`mirror`), and `FilterMode` to the `filtertype` enum (`closest`/`linear`/`cubic`, `Trilinear→linear` the mip blend being the sampler's pyramid concern). MaterialX's fourth `uaddressmode` member `constant` (a border colour outside `[0,1]`) has NO `AddressMode` row by design — a border colour is sampler-level payload, not a coordinate fold, so admitting it is one `AddressMode` row plus a border-colour column on `SamplerState` when an ingest contract demands it; the egress side never produces it, the named import edge. The leaf-basis quartet covers the gradient/simplex/value/cellular families against the standard library's `noise2d`/`noise3d`/`fractal2d`/`cellnoise2d`/`worleynoise2d`/`unifiednoise2d` — the `Value` basis the `cellnoise2d` value-noise analogue the prior three-basis set omitted, `unifiednoise2d` a parameterized selector neither basis maps cleanly onto. The FastNoiseLite OpenSimplex2 kernel stays the documented `LIBRARY_DEPTH` NOT_COVERED vendored carve-out — MaterialX names node categories, never the lattice math, so the FNL kernels are the shading truth and the categories the wire projection.
