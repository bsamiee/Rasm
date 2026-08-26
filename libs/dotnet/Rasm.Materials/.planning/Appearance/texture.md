# [MATERIALS_TEXTURE]

THE UV-AND-SOLID SAMPLING ENGINE. One `TextureUv` static sampling fold over the closed `TextureSource` `[Union]` (noise · checker · gradient · image · triplanar), addressed by the closed `AddressMode` band, reconstructed by the closed `FilterMode` band, and seeded by the author-kernel `ProceduralNoise` over the closed `NoiseBasis` band (`Perlin` gradient · `Simplex` OpenSimplex2 · `Value` lattice · `Worley` cellular) — each basis carrying its 2D AND 3D arm so triplanar and solid texturing sample the same lattice — with the fractal trajectory carried by the closed `FractalMode` axis (`FBm` · `Ridged` · `PingPong`) and the cellular feature algebra by the orthogonal `CellularDistance` × `CellularReturn` bands, all vendored inline from the FastNoiseLite algorithm and made exactly periodic by the `NoisePeriod` lattice wrap every wrappable basis carries — a SEALED `Noise.Of` mint proving basis, lacunarity, frequency, and warp frequency integral against the declared period before the value exists, so a tiling procedural is periodic by construction rather than healed after the fact and a seaming source is unconstructable rather than merely unadvised. `TextureSource` cases carry every texture variation, the `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` rows carry every sampling mode, `Unicolour` carries every color and `ShadeVec4` every sampled field — never a parallel sampler, a per-filter method, a parallel fractal-vs-basis enum, a `NoiseSampler3D` surface, or a second color register. `TextureUv` composes the kernel `bsdf#SHADING_FRAME` `MaterialFault` band-2450 channel, the `graph#MATERIAL_GRAPH` `PortValue`/`PortId`/`ShadePoint` carriers the node DAG threads, the Rasm.Numerics `UnitInterval`/`Dimension` value-objects for UV coordinates and image extents, the host `Vector3d` for world position and normal at the shading edge, and Wacton.Unicolour directly as the scene-linear color owner for every color literal — never re-minting a color space, a coordinate primitive, or a fault. `graph#MATERIAL_GRAPH` `AppearanceNode.Texture(Option<PortId> Parameter, Func<double,double,Option<double>,PortValue> Sample)` closes the terminal boundary: `TextureUv.Port` mints that total `(u,v,parameter)→PortValue` closure from a `TextureSource`, so a sampled texture drives a node DAG without a second sampler API, and the deep `Sample` result serves the wire and the masked-aging consumer reading the `Fin`.

## [01]-[INDEX]

- [02]-[TEXTURE_UV]: `TextureUv` folds the `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` bands, the `ShadeVec4` four-lane field register, the `NoisePeriod`/`NoiseLattice` periodic-lattice carriers, the `UvFrame` per-bind transform on `SamplerState`, the `TextureSource` union with its `Noise.Of` admission, the `ProceduralNoise` author-kernel, the one `TextureUv.Sample` fold, and the `TextureUv.Port` graph-node bridge.
- [03]-[PERIOD_ORACLE]: `PeriodOracle` rows and the `PeriodProof` roster prove each periodic source repeats under its own shift and each unwrappable source refuses at admission.

## [02]-[TEXTURE_UV]

- Owner: `TextureUv` static sampling fold; `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` `[SmartEnum<int>]` bands; `ShadeVec4` field register; `SamplerState` with its `UvFrame` per-bind transform; `ProceduralNoise` author-kernel; `TextureSource` `[Union]`.
- Cases: address {`Repeat`, `Clamp`, `Mirror`} · filter {`Nearest`, `Bilinear`, `Bicubic`, `Trilinear`} · noise-basis {`Perlin`, `Simplex`, `Value`, `Worley`} (fBm is octave-summation over a basis, `Octaves > 1`, never a fifth basis; `Wrappable` is the per-row periodic-lattice column) · fractal {`FBm`, `Ridged`, `PingPong`} · cellular-distance {`EuclideanSq`, `Euclidean`, `Manhattan`, `Hybrid`} · cellular-return {`CellValue`, `Distance`, `Distance2`, `Distance2Sub`, `Distance2Add`, `Distance2Mul`, `Distance2Div`} · source {`Noise`, `Checker`, `Gradient`, `Image`, `Triplanar`}.
- Entry: `public static Fin<Noise> Noise.Of(NoiseBasis basis, double frequency, Op key, …)` is the noise-source MINT the lattice period exists for and the only way a `Noise` value comes into being — the constructor is private and every column get-only, so the sampler dispatches on a union case carrying its own proof instead of screening a value a public constructor could always have forged. An aperiodic draft passes on one predicate; a periodic one proves its basis wrappable, its lacunarity and frequency integral against the period, and its warp frequency integral, so a source seaming at the tile edge is unrepresentable rather than rendered — and `Checker.Of`, `Gradient.Of`, and `Triplanar.Of` seal their cases the same way (repeats, stops, scale, and blend sharpness prove at the mint), so no union case admits a bare `new`; `public static Fin<ShadeVec4> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key)` is the deep field result and `public static Func<double, double, Option<double>, PortValue> Port(TextureSource source, UvSample anchor, SamplerState sampler, Channel channel, Op key)` is the `graph#MATERIAL_GRAPH` `AppearanceNode.Texture` bridge — `Port` captures the source/sampler/key and returns the TOTAL `(u,v,parameter)→PortValue` closure the node fold reads, its third lane the DRIVEN `UvSample.Parameter` a node's own `Parameter` port resolves so a field-driven ramp needs no second bridge (`Channel` projects the field to `PortValue.Color`, `.Scalar` (luminance), `.Scalar` mask (`W`), or `.Vector`), an empty-pyramid/degenerate-normal/non-finite-field sample folding to the channel's neutral `PortValue` so the graph arm stays total while the deep `Sample` result carries the `Op key`-correlated `MaterialFault` — the UV lanes themselves are `UnitInterval` value-objects, finite-in-[0,1] by construction, so no interior re-validation exists on the coordinate path; arity is one — a texture variation discriminates on the `TextureSource` union case and a sample modality on `Channel`, never on a sibling sampler method.
- Packages: Rasm.Materials.Appearance.Bsdf (`MaterialFault` band-2450), Rasm (project — `UnitInterval`/`Dimension`, and the `Numerics/atoms#SCALAR_FLOOR` `PerceptualColor`/`BlendPath.Oklch()`/`RgbProfile.Acescg`/`GamutPolicy.Perceptual` perceptual owner the gradient resolve composes), Rhino.Geometry (`Vector3d`/`Point3d` at the shading edge, the graph-page host-geometry convention), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<int>]` at the deepest surface — generated total `Switch`, `[UseDelegateFromConstructor]` behavior columns), LanguageExt.Core (`Fin`/`Seq`/`Bind`/`Fold`/`Traverse`), Wacton.Unicolour (scene-linear color owner — the authored-stop and literal carrier), CommunityToolkit.HighPerformance (`ReadOnlyMemory2D<ShadeVec4>`/`ReadOnlySpan2D` — the mip-plane owner, admitted once per level through `AsMemory2D(height, width)`), BCL inbox.
- Law: `ProceduralNoise` holds EVERY published FastNoiseLite anchor as a named `internal const` and no kernel on this page spells one inline — the lattice primes, the hash multiplier, the `ValCoord` projection scale, the 2D skew/unskew pair, both Perlin normalizers, both simplex bounds, the 3D rotation, both cellular jitter radii, the warp decorrelation offset, and the two defining gradient angles. INTERNAL is the load-bearing part: `Raster/gpu#WGSL_KERNEL` `Wgsl.NoiseConsts` INTERPOLATES these members into its shader prelude, so one declaration serves two emissions and a re-typed literal on either side is the deleted form the branch ruling names. A private anchor is a value the GPU page must transcribe, which is exactly the fork the interpolation closes; a new anchor lands one member here and one interpolated row there, never a value that page decides.
- Law: `Directions2D` stays a VERBATIM transcription while `GradientAngle0`/`GradientAngleStep` state its DEFINITION, and the two are not redundant. `Math.Cos` over those angles agrees with the published decimals only to within an ulp, so the CPU cannot generate the table without forfeiting the byte-parity claim this family exists for; the shader carries no table blob at all and must generate. Declaring the angles beside the transcription is what lets both sides share one definition of the cycle where their bytes cannot agree, and the `PressGpuParity` workload grades that divergence rather than asserting texel equality.
- Law: every `bool` on this page is a LONE column on its owner — `NoiseBasis.Wrappable` and `Noise.Solid` — so the kernel `Domain/validation#CAPABILITY` `CapabilitySet` law leaves each a bool and the owner says so. That law deletes ADJACENT bool columns whose boolean product has illegal corners; no owner here carries a second boolean axis, and a one-member capability roster would publish a set algebra no gate reads over a fact one predicate already answers. `Wrappable` is read at `Noise.Of`'s periodic gate and by the `refuse.simplex` refusal row; `Solid` selects the 3D arm at `Field`. Neither reconstructs the other.
- Law: absence never crosses as `null` past this page's boundary. `Noise.Of`'s optional columns are ROSTER VALUES — `FractalMode`, `CellularParams`, `DomainWarp`, `NoisePeriod`, and the two gradient `Unicolour` anchors — none of which has a compile-time constant form, so the language admits them only as nullable optionals and each resolves to its own roster default INSIDE the admission expression, at the ONE place a default noise posture is spelled. No interior signature, no return, and no stored column on this page carries a nullable.
- Growth: a new addressing rule is one `AddressMode` row, a new reconstruction filter one `FilterMode` row, a new leaf noise basis one `NoiseBasis` row binding one `ProceduralNoise.Sample2D`/`Sample3D` arm pair and answering the `Wrappable` column, a new per-octave lattice policy one `NoiseLattice` column rather than a widened arm signature, a new fractal trajectory one `FractalMode` row, a new cellular feature one `CellularReturn`/`CellularDistance` row, a new texture one `TextureSource` case carrying its `MtlxCategory`, a new sampled-channel modality one `Channel` row — never a parallel `BilinearSampler`/`PerlinTexture`/`NoiseSampler3D` surface and never a parallel fractal-kind enum since the fractal trajectory is a `FractalMode` row over the basis octave-sum. `NoiseBasis` closes on the FastNoiseLite leaf-basis family (the `Perlin`/`Simplex`/`Value`/`Worley` gradient·simplex·lattice·cellular quartet) projecting onto the MaterialX 1.39 `noise2d`/`noise3d`/`fractal2d`/`cellnoise2d`/`worleynoise2d` categories — `Value` the `cellnoise2d` value-noise analogue, `noise3d` (`NodeCategory.Perlin3D`) the solid-noise wire target, `unifiednoise2d` a parameterized selector no single basis maps cleanly onto; `ValueCubic` is one `NoiseBasis` row binding one `ProceduralNoise` arm and one `MtlxNode`, not a new noise class. `TextureSource.MtlxCategory`, `NoiseBasis.MtlxNode`, `AddressMode.MtlxAddress`, and `FilterMode.MtlxFilter` project the MaterialX 1.39 node-category parity the `interchange#MATERIALX_DOCUMENT` `Mtlx.CategoryOf` resolves against the closed `NodeCategory` set — MaterialX names node categories and never the lattice math, so the FNL kernels stay the shading truth and the categories the wire projection. MaterialX's fourth `uaddressmode` member `constant` has no `AddressMode` row by design — under it an out-of-range coordinate returns the `image` node's OWN `default` input rather than folding, so the value is sampler-level payload and not a coordinate law; admitting it is one `AddressMode` row and one border-value column on `SamplerState`, and the egress side never produces it, the named import edge. A new bind-time coordinate axis is one `UvFrame` column, never a second transform carrier and never a per-source arm.
- Boundary: UV coordinates enter as Rasm.Numerics `UnitInterval` pairs (the `[0,1]` validated value-object), image extents as `Dimension` (the `>=1` int-backed value-object), world position and normal as host `Vector3d`; the sampler NEVER re-mints a coordinate or extent primitive. `ShadeVec4` carries the interior noise/checker/gradient/image/triplanar algebra on one four-lane field register (`X`/`Y`/`Z` the scalar-field/color lanes, `W` the texel alpha the `Image` reconstruction premultiplies and `Channel.Mask` reads) — `ShadeVec4` is the texture field carrier distinct from the `bsdf#LOBE_FAMILY` `RgbSpectrum` validated reflectance: a noise field is signed, a normal-map decode is `[-1,1]`, and a texel carries alpha, none of which the non-negative-validated `RgbSpectrum` admits, so the field stays the raw register and crosses to the validated reflectance only through `ShadeVec4.AsColor`. Color crosses the axis exactly once: color literals on `TextureSource` rows (`Low`/`High`, `Even`/`Odd`, gradient `Stops`) enter as `Unicolour` and decompose to `ShadeVec4` through `ShadeVec4.FromColor` for the field math — the gradient canonicalizes its authored stops sorted-by-position, admits each through the kernel `PerceptualColor.OfRgb(…, RgbProfile.Acescg)` ingress, and pre-resolves the `Lut` texel run through `PerceptualColor.Mix` on the `BlendPath.Oklch()` authored-hue path with every texel landing through `ToRgb(RgbProfile.Acescg, GamutPolicy.Perceptual)` at `Gradient.Of` construction — the corpus' ONE perceptual-blend spelling, so the perceptual hue path (never the linear-RGB lerp that bends hue through the grey dead zone, never a host colour-blend standing in for the kernel owner) is priced ONCE off the hot path with the gamut map a raw host `Mix` omits, an unsorted authored list cannot mangle the bracketing walk, and the per-sample read is an index-lerp between adjacent resolved texels — and the single `ShadeVec4.AsColor` adapter constructs the canonical scene-linear `Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z)` at the projection tail — the sampler NEVER mints a second color register. the per-bind UV transform is `SamplerState.Frame`, a `UvFrame` of five doubles applied at exactly ONE site inside `TextureUv.Sample` BEFORE the source dispatch — scale, then rotation about the UV origin, then offset, the KHR_texture_transform composition a `Rasm.Bim` `UvTransform` lowers onto at the boundary — so `AddressMode.Apply` sees the TRANSFORMED coordinate and every source case (noise, checker, gradient, image, triplanar) inherits tiling with no per-case edit, while the transform stays OFF the set: `Raster/set#SET_INGEST`'s content-addressed atlas has N sets sharing one blob, and a per-tiling column inside a set forks that key per consumer; `UvFrame.Digest` enters the `press#PRESS_PLAN` key preimage beside the post-chain ops because a re-tiled bake is different bytes, and the identity frame digests EMPTY so landing the axis re-keys nothing already pressed. `AddressMode.Apply` folds the framed continuous UV into `[0,1)` once before any non-image filter touches a coordinate, and image reconstruction addresses exclusively through the discrete `AddressMode.Texel` companion so the wrap arithmetic is consulted once per axis, not double-applied at the mip seam; `FilterMode` reconstructs through one weight algebra (`Nearest` snaps by `Floor(u·w)`, `Bilinear` is the unit-square lerp, `Bicubic` is the separable Catmull-Rom 4×4 convolution, `Trilinear` blends two `ReconstructLevel` taps across the mip pyramid by the fractional level `SampleImage` decomposes — `ReconstructLevel` itself dispatches only the spatial `Nearest`/`Bilinear`/`Bicubic` kernels, each snapped to the `MipLevel`-nearest plane so the supplied level is dead for no filter row, the cross-level blend the `Trilinear` arm's own concern); a `NoisePeriod` on the `Noise` row makes the lattice exactly periodic by wrapping the integer cell index modulo the period before the prime multiply — the fractional position inside the cell is untouched, so the interpolant stays continuous across the wrap where a coordinate modulo cuts it — and the period rides the octave ladder through `NoisePeriod.Scaled`, multiplying by the same integral lacunarity the frequency does, so octave three wraps where octave three's own lattice repeats; the `NoiseLattice` carrier threads `(seed, period, cellular)` into every basis arm as ONE column set, the cellular fold hashes the WRAPPED neighbour while displacing the feature point at its UNWRAPPED coordinate (a wrapped displacement collapses every period boundary onto one point), and a warped periodic source displaces through the wrappable `Perlin` arm at the warp's own scaled period so the displacement field shares the tile; `Raster/tile#TILE_SYNTH` is the ORTHOGONAL owner — it heals an already-authored plane whose source admits no period, where this period makes the source periodic BY CONSTRUCTION, and a procedurally periodic field needs no heal, while `Raster/gpu#WGSL_KERNEL` `noiseField` lowers the same WRAP LAW at `f32` so the preview tiles where the bake tiles — the period algebra is shared while the gradient tables generate independently at two precisions, so texel equality is never asserted and the `PressGpuParity` workload grades the divergence; the FastNoiseLite gradient/simplex/value/cellular kernels are author-folds over the hashed lattice — a DELIBERATE second family beside the kernel `Rasm/Numerics/calculus#NOISE_LATTICES` `FieldNoise`, split on parity-vs-differentiability: this family holds FNL byte-exactness for the `NoiseBasis.MtlxNode` MaterialX 1.39 category round-trip and the `Raster/gpu#WGSL_KERNEL` `noiseField` `f32` wrap-law parity, with 2D arms, periodic-by-construction cell-index lattices, and the seven-row cellular return set, while the kernel keeps the canonical published Perlin permutation feeding `NoiseKind.ContinuouslyDifferentiable` and the Lipschitz fold — collapsing either end breaks the other's gating [branch RULINGS `[03]-[COLLAPSE]`] — with the published FNL anchors — `PrimeX`/`PrimeY`/`PrimeZ` lattice primes and the `0x27d4eb2d` hash multiplier, the quintic fade `6t⁵−15t⁴+10t³` (Perlin) and the Hermite `t²(3−2t)` (Value), the 2D simplex skew `(√3−1)/2` / unskew `(3−√3)/6` with the `99.83685446303647` bound, the 3D OpenSimplex2 rotation `r=(x+y+z)·2/3` two-cell fold with the `32.69428253173828125` bound, the Perlin normalizers `1.4247691104677813` (2D) and `0.964921414852142333984375` (3D), the 24-direction 2D and 12-edge-plus-published-tail 3D gradient cycles, and `ValCoord`'s square-then-`^ << 19` hash→`[-1,1]` projection — each table taking the ONE parity mechanism its definition admits: a sequence-defined table lands exact (`Gradients3D` generates from its integer edge set, `Gradients2D` transcribes the twenty-four published directions verbatim because trig over the defining angles agrees only to within an ulp), while an AUTHORED table with no defining sequence rides as a digest-pinned content-keyed asset through `LatticeTables.Of`, so the byte-exactness claim is structural at both and a regenerated stand-in is the deleted form; the fractal trajectory is the `FractalMode` per-octave `Step` row (`FBm` the signed `n·amp` sum damping by `(n+1)/2`, `Ridged` the `(1−2|n|)·amp` fold damping by `1−|n|`, `PingPong` the centred `(p−0.5)·2·amp` triangle fold at the source's `PingPongStrength` damping by `p`) under FNL's `Lerp(1, damp, WeightedStrength)·Gain` amplitude cascade opening at the fractal bounding `1/Σ Gainⁱ` — never a hardcoded linear sum, never a post-hoc normalize — and the `Fbm` self-base is unrepresentable, `NoiseBasis` excludes it; the cellular kernel folds the `CellularDistance` metric over the 3×3 (2D) or 3×3×3 (3D) feature neighbourhood displaced by unit offset vectors at the FNL jitter radii `0.43701595` (2D) / `0.39614353` (3D) scaled by `CellularParams.Jitter`, and projects the `CellularReturn` feature (`Distance` the F1 nearest, `Distance2` the F2, `Distance2Sub` the F2−F1 vein, `Distance2Add`/`Distance2Mul`/`Distance2Div` the FNL blend trio, `CellValue` the per-cell hash), so the `Worley` arm spans the full cellular family rather than the single F1 distance; the `ProceduralNoise` hash-lattice fills, the fixed neighbourhood / three-corner (2D) and rotated two-cell (3D) simplex loops, the span tap kernels (`NearestTap`/`BilinearTap`/`BicubicTap`), and the per-source sampling folds (`SampleImage`/`SampleTriplanar`/`Gradient.Resolve`) are the page's `[EXPRESSION_SPINE]` kernel exemption, in-place by index over the per-shade hot path; triplanar projects a world point onto the three axis planes, wraps each through the sampler's `AddressMode.Apply` (never a parallel `Frac`), and blends by the squared-normal weight so the same `TextureSource` evaluates without a UV unwrap, and a `Noise` source under triplanar samples the 3D basis arm directly so solid noise needs no plane projection; the image pyramid admits ONCE at `Image.Of` — each flat level lifts through `AsMemory2D(height, width)` into a `ReadOnlyMemory2D<ShadeVec4>` plane whose `Height`/`Width` are structural facts, a payload/extent mismatch faulting at admission so the per-tap reconstruction carries no re-check and the prior per-sample undersized fault is unrepresentable; an empty pyramid or a degenerate triplanar normal fails to `MaterialFault` through the deep `Sample` result — a stopless gradient, a zero-repeat checker, and a non-positive triplanar sharpness refuse at their own `Of` mints and never reach a sample, a non-finite field lane fails at `ShadeVec4.AsColor` (`IsFinite` spans all four lanes — a corrupt coverage lane is as degenerate as a corrupt color lane), and the `Port` closure folds any fault OR non-finite field to the `Channel` neutral so the graph arm never sees a sentinel or NaN texel — the raw host `World`/`Normal` doubles are the one lane the `UnitInterval` coordinate admission cannot gate, so the finite fold at the projection tail is the boundary that keeps the graph closure total over them. The `LatticeAsset` pin columns are DECLARED-EMPTY by law rather than pending: a digest is a measurement OF a payload, so `Vecs2D`/`Vecs3D` ship `Option<ContentAddress>.None` until the FastNoiseLite offset tables vendor as content-keyed bytes and `ContentAddress.Of` reads them, and `Admit` refuses an unpinned asset in the meantime — the gate stands against the absence, never around it, and a digest authored ahead of its payload would pin nothing while admitting whatever bytes arrived, which is the same coincidence-not-construction the regenerated table was; the fitted sky-coefficient assets carry the identical declared-empty-then-vendored pin shape. [SPIKE]: each pin's VALUE converges on the vendoring's own measurement of the landed bytes alone; the deterministic floor is the `Entries`/`Lanes` extent contract and the unpinned refusal, both total without it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Numerics;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Graph;
using Rhino.Geometry;
using Thinktecture;
using Wacton.Unicolour;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Texture;

// --- [TYPES] ---------------------------------------------------------------------------
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
            state:  (Index: i, Extent: extent),
            repeat: static (s, _) => ((s.Index % s.Extent) + s.Extent) % s.Extent,
            clamp:  static (s, _) => Math.Clamp(s.Index, 0, s.Extent - 1),
            mirror: static (s, _) => (2 * s.Extent) switch {
                var period => (((s.Index % period) + period) % period) switch {
                    var m => m < s.Extent ? m : period - 1 - m,
                },
            });
}

[SmartEnum<int>]
public sealed partial class FilterMode {
    public static readonly FilterMode Nearest   = new(0, mtlxFilter: "closest");
    public static readonly FilterMode Bilinear  = new(1, mtlxFilter: "linear");
    public static readonly FilterMode Bicubic   = new(2, mtlxFilter: "cubic");
    public static readonly FilterMode Trilinear = new(3, mtlxFilter: "linear");
    public string MtlxFilter { get; }
}

[SmartEnum<int>]
public sealed partial class NoiseBasis {
    public static readonly NoiseBasis Perlin  = new(0, mtlxNode: "noise2d",        wrappable: true,  sample2D: ProceduralNoise.Perlin2D,  sample3D: ProceduralNoise.Perlin3D);
    public static readonly NoiseBasis Simplex = new(1, mtlxNode: "fractal2d",      wrappable: false, sample2D: ProceduralNoise.Simplex2D, sample3D: ProceduralNoise.Simplex3D, lossy: "simplex-as-fractal2d");
    public static readonly NoiseBasis Value   = new(2, mtlxNode: "cellnoise2d",    wrappable: true,  sample2D: ProceduralNoise.Value2D,   sample3D: ProceduralNoise.Value3D);
    public static readonly NoiseBasis Worley  = new(3, mtlxNode: "worleynoise2d",  wrappable: true,  sample2D: ProceduralNoise.Worley2D,  sample3D: ProceduralNoise.Worley3D);
    public string MtlxNode { get; }
    public bool Wrappable { get; }

    public Option<string> Lossy { get; }

    public static Option<(NoiseBasis Basis, FractalMode Fractal, bool Solid)> Unified(int selector) =>
        selector switch {
            0 => Some((Perlin, FractalMode.FBm, false)),
            1 => Some((Value, FractalMode.FBm, false)),
            2 => Some((Worley, FractalMode.FBm, false)),
            3 => Some((Perlin, FractalMode.FBm, true)),
            _ => None,
        };

    [UseDelegateFromConstructor]
    public partial double Sample2D(double x, double y, NoiseLattice lattice);
    [UseDelegateFromConstructor]
    public partial double Sample3D(double x, double y, double z, NoiseLattice lattice);
}

[SmartEnum<int>]
public sealed partial class FractalMode {
    public static readonly FractalMode FBm      = new(0, step: static (n, amp, _) => (n * amp, Math.Min(n + 1.0, 2.0) * 0.5));
    public static readonly FractalMode Ridged   = new(1, step: static (n, amp, _) => { double f = Math.Abs(n); return ((f * -2.0 + 1.0) * amp, 1.0 - f); });
    public static readonly FractalMode PingPong = new(2, step: static (n, amp, strength) => { double p = PingPongWave((n + 1.0) * strength); return ((p - 0.5) * 2.0 * amp, p); });

    [UseDelegateFromConstructor]
    public partial (double Contribution, double Damp) Step(double sample, double amplitude, double pingPongStrength);

    private static double PingPongWave(double t) { t -= (int)(t * 0.5) * 2.0; return t < 1.0 ? t : 2.0 - t; }
}

[SmartEnum<int>]
public sealed partial class CellularDistance {
    public static readonly CellularDistance EuclideanSq = new(0, metric: static (dx, dy, dz) => dx * dx + dy * dy + dz * dz);
    public static readonly CellularDistance Euclidean   = new(1, metric: static (dx, dy, dz) => Math.Sqrt(dx * dx + dy * dy + dz * dz));
    public static readonly CellularDistance Manhattan   = new(2, metric: static (dx, dy, dz) => Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz));
    public static readonly CellularDistance Hybrid      = new(3, metric: static (dx, dy, dz) => Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) + (dx * dx + dy * dy + dz * dz));

    [UseDelegateFromConstructor]
    public partial double Metric(double dx, double dy, double dz);
}

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

    public sealed record Noise : TextureSource {
        private Noise(
            NoiseBasis basis, double frequency, int octaves, double lacunarity, double gain, int seed,
            FractalMode fractal, double weightedStrength, double pingPongStrength, CellularParams cellular,
            DomainWarp warp, bool solid, NoisePeriod period, Unicolour low, Unicolour high) =>
            (Base, Frequency, Octaves, Lacunarity, Gain, Seed, Fractal, WeightedStrength, PingPongStrength,
                Cellular, Warp, Solid, Period, Low, High) =
            (basis, frequency, octaves, lacunarity, gain, seed, fractal, weightedStrength, pingPongStrength,
                cellular, warp, solid, period, low, high);

        public NoiseBasis Base { get; }
        public double Frequency { get; }
        public int Octaves { get; }
        public double Lacunarity { get; }
        public double Gain { get; }
        public int Seed { get; }
        public FractalMode Fractal { get; }
        public double WeightedStrength { get; }
        public double PingPongStrength { get; }
        public CellularParams Cellular { get; }
        public DomainWarp Warp { get; }
        public bool Solid { get; }
        public NoisePeriod Period { get; }
        public Unicolour Low { get; }
        public Unicolour High { get; }

        public static Fin<Noise> Of(
            NoiseBasis basis, double frequency, Op key,
            int octaves = 1, double lacunarity = 2.0, double gain = 0.5, int seed = 1337,
            FractalMode? fractal = null, double weightedStrength = 0.0, double pingPongStrength = 2.0,
            CellularParams? cellular = null, DomainWarp? warp = null, bool solid = false,
            NoisePeriod? period = null, Unicolour? low = null, Unicolour? high = null) {
            Noise candidate = new(basis, frequency, octaves, lacunarity, gain, seed,
                fractal ?? FractalMode.FBm, weightedStrength, pingPongStrength,
                cellular ?? CellularParams.Default, warp ?? DomainWarp.None, solid,
                period ?? NoisePeriod.Aperiodic, low ?? Grey(0.0), high ?? Grey(1.0));
            return from _ in guard(
                       double.IsFinite(candidate.Frequency) && candidate.Frequency > 0.0
                       && candidate.Octaves >= 1
                       && double.IsFinite(candidate.Lacunarity) && candidate.Lacunarity > 0.0
                       && double.IsFinite(candidate.Gain)
                       && double.IsFinite(candidate.WeightedStrength) && candidate.WeightedStrength is >= 0.0 and <= 1.0
                       && double.IsFinite(candidate.PingPongStrength) && candidate.PingPongStrength > 0.0
                       && double.IsFinite(candidate.Cellular.Jitter) && candidate.Cellular.Jitter is >= 0.0 and <= 1.0
                       && double.IsFinite(candidate.Warp.Amplitude) && candidate.Warp.Amplitude >= 0.0
                       && double.IsFinite(candidate.Warp.Frequency),
                       new MaterialFault.Parameter(key, $"<noise-column-out-of-domain:{candidate.Base.MtlxNode}>"))
                   from admitted in candidate.Period.Periodic ? Periodic(candidate, key) : Fin.Succ(candidate)
                   select admitted;
        }

        static Fin<Noise> Periodic(Noise candidate, Op key) =>
            from _ in guard(candidate.Base.Wrappable, new MaterialFault.Parameter(key, $"<noise-period-unwrappable-basis:{candidate.Base.MtlxNode}>"))
            from __ in guard(Integral(candidate.Lacunarity), new MaterialFault.Parameter(key, $"<noise-period-fractional-lacunarity:{candidate.Lacunarity:R}>"))
            from ___ in guard(Integral(candidate.Frequency) && (int)candidate.Frequency % candidate.Period.Value is 0,
                    new MaterialFault.Parameter(key, $"<noise-period-frequency-not-multiple:{candidate.Frequency:R}:{candidate.Period.Value}>"))
            from ____ in guard(candidate.Warp.Amplitude <= 0.0 || Integral(candidate.Warp.Frequency),
                    new MaterialFault.Parameter(key, $"<noise-period-fractional-warp-frequency:{candidate.Warp.Frequency:R}>"))
            select candidate;

        static bool Integral(double v) => double.IsInteger(v) && v is > 0.0 and <= int.MaxValue;

        static Unicolour Grey(double v) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, v, v, v);
    }
    public sealed record Checker : TextureSource {
        private Checker(int repeats, Unicolour even, Unicolour odd) => (Repeats, Even, Odd) = (repeats, even, odd);
        public int Repeats { get; }
        public Unicolour Even { get; }
        public Unicolour Odd { get; }

        public static Fin<Checker> Of(int repeats, Unicolour even, Unicolour odd, Op key) =>
            repeats >= 1
                ? Fin.Succ(new Checker(repeats, even, odd))
                : new MaterialFault.Parameter(key, $"<checker-repeats-out-of-domain:{repeats}>");
    }

    public sealed record Gradient : TextureSource {
        const int LutTexels = 64;
        static readonly BlendPath Route = BlendPath.Oklch();

        private Gradient(bool vertical, Seq<(UnitInterval At, Unicolour Color)> stops, Seq<ShadeVec4> lut) =>
            (Vertical, Stops, Lut) = (vertical, stops, lut);
        public bool Vertical { get; }
        public Seq<(UnitInterval At, Unicolour Color)> Stops { get; }
        public Seq<ShadeVec4> Lut { get; }

        public static Fin<Gradient> Of(bool vertical, Seq<(UnitInterval At, Unicolour Color)> stops, Op key) {
            if (stops.IsEmpty) { return new MaterialFault.Parameter(key, "<gradient-no-stops>"); }
            Seq<(UnitInterval At, Unicolour Color)> sorted = toSeq(stops.OrderBy(static s => s.At.Value));
            return sorted
                .TraverseM(stop => Admit(stop.Color, key).Map(colour => (stop.At, Colour: colour))).As()
                .Map(admitted => new Gradient(vertical, sorted,
                    toSeq(Enumerable.Range(0, LutTexels)).Map(i => Resolve(admitted, i / (LutTexels - 1.0)))));
        }

        static Fin<PerceptualColor> Admit(Unicolour colour, Op key) =>
            colour.RgbLinear.Triplet switch {
                { } lin => PerceptualColor.OfRgb(lin.First, lin.Second, lin.Third, RgbProfile.Acescg, key: key),
            };

        static ShadeVec4 Resolve(Seq<(UnitInterval At, PerceptualColor Colour)> stops, double t) {
            (UnitInterval At, PerceptualColor Colour) lo = stops[0];
            if (t <= lo.At.Value) { return Texel(lo.Colour); }
            foreach ((UnitInterval At, PerceptualColor Colour) hi in stops.Tail) {
                if (t <= hi.At.Value) {
                    double span = hi.At.Value - lo.At.Value;
                    return Texel(span > double.Epsilon
                        ? lo.Colour.Mix(hi.Colour, UnitInterval.Create((t - lo.At.Value) / span), Route)
                        : hi.Colour);
                }
                lo = hi;
            }
            return Texel(lo.Colour);
        }

        static ShadeVec4 Texel(PerceptualColor colour) =>
            colour.ToRgb(RgbProfile.Acescg, GamutPolicy.Perceptual) switch {
                { } mapped => new ShadeVec4(mapped.Red, mapped.Green, mapped.Blue, 1.0),
            };
    }

    public sealed record Image(Seq<ReadOnlyMemory2D<ShadeVec4>> Levels) : TextureSource {
        public static Fin<Image> Of(Dimension width, Dimension height, Seq<ReadOnlyMemory<ShadeVec4>> levels, Op key) =>
            levels.IsEmpty
                ? new MaterialFault.Parameter(key, "<texture-image-empty>")
                : levels.Map((flat, index) => {
                      int w = Math.Max(1, width.Value >> index), h = Math.Max(1, height.Value >> index);
                      return flat.Length == w * h
                          ? Fin.Succ(flat.AsMemory2D(h, w))
                          : Fin.Fail<ReadOnlyMemory2D<ShadeVec4>>(new MaterialFault.Parameter(key, $"<texture-level-extent:{index}:{flat.Length}!={w * h}>"));
                  }).Traverse(identity).As().Map(static planes => new Image(planes));
    }

    public sealed record Triplanar : TextureSource {
        private Triplanar(TextureSource projected, double scale, double blendSharpness) =>
            (Projected, Scale, BlendSharpness) = (projected, scale, blendSharpness);
        public TextureSource Projected { get; }
        public double Scale { get; }
        public double BlendSharpness { get; }

        public static Fin<Triplanar> Of(TextureSource projected, double scale, double blendSharpness, Op key) =>
            double.IsFinite(scale) && scale > 0.0 && double.IsFinite(blendSharpness) && blendSharpness > 0.0
                ? Fin.Succ(new Triplanar(projected, scale, blendSharpness))
                : new MaterialFault.Parameter(key, $"<triplanar-out-of-domain:{scale:R},{blendSharpness:R}>");
    }

    public string MtlxCategory => Switch(
        noise:     static n => n.Octaves > 1 && n.Base != NoiseBasis.Worley ? "fractal2d" : n.Base.MtlxNode,
        checker:   static _ => "checkerboard",
        gradient:  static g => g.Vertical ? "ramptb" : "ramplr",
        image:     static _ => "tiledimage",
        triplanar: static _ => "triplanarprojection");

    public Option<string> MtlxLossy => Switch(
        noise:     static n => n.Base.Lossy,
        checker:   static _ => Option<string>.None,
        gradient:  static _ => Option<string>.None,
        image:     static _ => Option<string>.None,
        triplanar: static _ => Option<string>.None);

    public Seq<(string Name, string Type, string Value)> MtlxParameters => Switch(
        noise:     static n => n.Base == NoiseBasis.Worley
            ? Seq(("jitter", "float", Num(n.Cellular.Jitter)),
                  ("style", "integer", n.Cellular.Return == CellularReturn.CellValue ? "1" : "0"))
            : n.Octaves > 1
                ? Seq(("octaves", "integer", n.Octaves.ToString(CultureInfo.InvariantCulture)),
                      ("lacunarity", "float", Num(n.Lacunarity)),
                      ("diminish", "float", Num(n.Gain)))
                : Seq<(string, string, string)>(),
        checker:   static c => Seq(("color1", "color3", Rgb(c.Even)), ("color2", "color3", Rgb(c.Odd)),
                                   ("uvtiling", "vector2", $"{Num(c.Repeats)}, {Num(c.Repeats)}")),
        gradient:  static g => g.Stops.IsEmpty
            ? Seq<(string, string, string)>()
            : g.Vertical
                ? Seq(("valueb", "color3", Rgb(g.Stops[0].Color)), ("valuet", "color3", Rgb(g.Stops[g.Stops.Count - 1].Color)))
                : Seq(("valuel", "color3", Rgb(g.Stops[0].Color)), ("valuer", "color3", Rgb(g.Stops[g.Stops.Count - 1].Color))),
        image:     static _ => Seq<(string, string, string)>(),
        triplanar: static _ => Seq(("upaxis", "integer", "2")));

    static string Num(double v) => v.ToString("R", CultureInfo.InvariantCulture);

    static string Rgb(Unicolour c) {
        var t = c.ConvertToConfiguration(PortValue.SceneLinear).RgbLinear.Triplet;
        return $"{Num(t.First)}, {Num(t.Second)}, {Num(t.Third)}";
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct CellularParams(CellularDistance Distance, CellularReturn Return, double Jitter) {
    public static readonly CellularParams Default = new(CellularDistance.EuclideanSq, CellularReturn.Distance, 1.0);
}

public readonly record struct DomainWarp(double Amplitude, double Frequency, int Seed) {
    public static readonly DomainWarp None = new(0.0, 1.0, 0);
}

[ValueObject<int>]
public readonly partial struct NoisePeriod {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 0) { validationError = new ValidationError($"<noise-period-negative:{value}>"); }
    }

    public static readonly NoisePeriod Aperiodic = Create(0);
    public static NoisePeriod Of(int value) => Create(value);
    public bool Periodic => Value > 0;
    public NoisePeriod Scaled(int lacunarity) =>
        Value > 0 ? Create((int)Math.Min((long)Value * Math.Max(1, lacunarity), int.MaxValue)) : this;
    public int Wrap(int cell) => Value > 0 ? ((cell % Value) + Value) % Value : cell;
}

public readonly record struct NoiseLattice(int Seed, NoisePeriod Period, CellularParams Cellular) {
    public static NoiseLattice Of(int seed) => new(seed, NoisePeriod.Aperiodic, CellularParams.Default);
}

public readonly record struct ShadeVec4(double X, double Y, double Z, double W) {
    public static ShadeVec4 Splat(double v) => new(v, v, v, v);
    public static ShadeVec4 operator +(ShadeVec4 a, ShadeVec4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static ShadeVec4 operator *(ShadeVec4 a, double s) => new(a.X * s, a.Y * s, a.Z * s, a.W * s);
    public static ShadeVec4 Lerp(ShadeVec4 a, ShadeVec4 b, double t) => a * (1.0 - t) + b * t;
    public double Luminance =>
        (RgbSpectrum.LuminanceWeights.R * X) + (RgbSpectrum.LuminanceWeights.G * Y) + (RgbSpectrum.LuminanceWeights.B * Z);

    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z) && double.IsFinite(W);

    public static ShadeVec4 FromColor(Unicolour colour) {
        ColourTriplet lin = colour.RgbLinear.Triplet;
        return new(lin.First, lin.Second, lin.Third, 1.0);
    }

    public Fin<Unicolour> AsColor(Op key) =>
        IsFinite
            ? Fin.Succ(AsColorUnchecked())
            : new MaterialFault.Gamut(key, $"<texture-non-finite-field:{X:R},{Y:R},{Z:R},{W:R}>");
    public Unicolour AsColorUnchecked() => new(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z);
}

public readonly record struct UvSample(UnitInterval U, UnitInterval V, Vector3d World, Vector3d Normal, double MipLevel) {
    public Option<double> Parameter { get; init; }

    public static Fin<UvSample> Of(double u, double v, Op key) =>
        from cu in key.AcceptValidated<UnitInterval>(candidate: u)
        from cv in key.AcceptValidated<UnitInterval>(candidate: v)
        select new UvSample(cu, cv, Vector3d.Zero, Vector3d.ZAxis, 0.0);

    public UvSample At(double u, double v) => this with { U = UnitInterval.Create(Math.Clamp(u, 0.0, 1.0)), V = UnitInterval.Create(Math.Clamp(v, 0.0, 1.0)) };
}

public readonly record struct UvFrame(double OffsetU, double OffsetV, double ScaleU, double ScaleV, double Rotation) {
    public static readonly UvFrame Identity = new(0.0, 0.0, 1.0, 1.0, 0.0);

    public (double U, double V) Apply(double u, double v) {
        (double su, double sv) = (u * ScaleU, v * ScaleV);
        (double sin, double cos) = Math.SinCos(Rotation);
        return (OffsetU + (su * cos) + (sv * sin), OffsetV - (su * sin) + (sv * cos));
    }

    public string Digest =>
        this == Identity
            ? string.Empty
            : string.Create(CultureInfo.InvariantCulture, $"{OffsetU:R}:{OffsetV:R}:{ScaleU:R}:{ScaleV:R}:{Rotation:R}");
}

public readonly record struct SamplerState(AddressMode AddressU, AddressMode AddressV, FilterMode Filter, UvFrame Frame) {
    public static readonly SamplerState Default = new(AddressMode.Repeat, AddressMode.Repeat, FilterMode.Bilinear, UvFrame.Identity);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProceduralNoise {
    // --- [FNL_ANCHORS]
    internal const int PrimeX = 501125321;
    internal const int PrimeY = 1136930381;
    internal const int PrimeZ = 1720413743;
    internal const int HashMultiplier = 0x27d4eb2d;
    internal const double ValCoordScale = 1.0 / 2147483648.0;
    internal const double Skew2D = 0.3660254037844386;
    internal const double Unskew2D = 0.21132486540518713;
    internal const double PerlinNorm2D = 1.4247691104677813;
    internal const double PerlinNorm3D = 0.964921414852142333984375;
    internal const double SimplexBound2D = 99.83685446303647;
    internal const double SimplexBound3D = 32.69428253173828125;
    internal const double SimplexRotate3D = 2.0 / 3.0;
    internal const double CellJitter2D = 0.43701595;
    internal const double CellJitter3D = 0.39614353;
    internal const double WarpDecorrelation = 1000.0;
    internal const double GradientAngle0 = Math.PI / 24.0;
    internal const double GradientAngleStep = Math.PI / 12.0;

    private static int Hash(int seed, int xPrimed, int yPrimed) { int h = seed ^ xPrimed ^ yPrimed; h *= HashMultiplier; return h; }
    private static int Hash(int seed, int xPrimed, int yPrimed, int zPrimed) { int h = seed ^ xPrimed ^ yPrimed ^ zPrimed; h *= HashMultiplier; return h; }
    private static double ValCoord(int hash) { hash *= hash; hash ^= hash << 19; return hash * ValCoordScale; }
    private static int Round(double v) => v >= 0.0 ? (int)(v + 0.5) : (int)(v - 0.5);
    private static int Cell(int index, NoisePeriod period) => period.Wrap(index);

    private static double GradCoord(int seed, int xPrimed, int yPrimed, double xd, double yd) {
        int hash = Hash(seed, xPrimed, yPrimed); hash ^= hash >> 15; hash &= 127 << 1;
        return xd * Gradients2D[hash] + yd * Gradients2D[hash | 1];
    }
    private static double GradCoord(int seed, int xPrimed, int yPrimed, int zPrimed, double xd, double yd, double zd) {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed); hash ^= hash >> 15; hash &= 63 << 2;
        return xd * Gradients3D[hash] + yd * Gradients3D[hash | 1] + zd * Gradients3D[hash | 2];
    }

    private static double Fade(double t) => t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    private static double Hermite(double t) => t * t * (3.0 - 2.0 * t);
    private static double Lerp(double a, double b, double t) => a + t * (b - a);

    // --- [PERLIN]
    public static double Perlin2D(double x, double y, NoiseLattice lattice) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
        double xd0 = x - x0, yd0 = y - y0, xd1 = xd0 - 1.0, yd1 = yd0 - 1.0;
        double xs = Fade(xd0), ys = Fade(yd0);
        int xp0 = Cell(x0, lattice.Period) * PrimeX, yp0 = Cell(y0, lattice.Period) * PrimeY;
        int xp1 = Cell(x0 + 1, lattice.Period) * PrimeX, yp1 = Cell(y0 + 1, lattice.Period) * PrimeY;
        int seed = lattice.Seed;
        double n00 = GradCoord(seed, xp0, yp0, xd0, yd0), n10 = GradCoord(seed, xp1, yp0, xd1, yd0);
        double n01 = GradCoord(seed, xp0, yp1, xd0, yd1), n11 = GradCoord(seed, xp1, yp1, xd1, yd1);
        return Lerp(Lerp(n00, n10, xs), Lerp(n01, n11, xs), ys) * PerlinNorm2D;
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
        return Lerp(xf0, xf1, zs) * PerlinNorm3D;
    }

    // --- [SIMPLEX]
    public static double Simplex2D(double x, double y, NoiseLattice lattice) {
        const double F2 = Skew2D, G2 = Unskew2D;
        int seed = lattice.Seed;
        double s = (x + y) * F2;
        int i = (int)Math.Floor(x + s), j = (int)Math.Floor(y + s);
        double t = (i + j) * G2, x0 = x - (i - t), y0 = y - (j - t);
        int i1 = x0 >= y0 ? 1 : 0, j1 = x0 >= y0 ? 0 : 1;
        double x1 = x0 - i1 + G2, y1 = y0 - j1 + G2, x2 = x0 - 1.0 + 2.0 * G2, y2 = y0 - 1.0 + 2.0 * G2;
        int ip = i * PrimeX, jp = j * PrimeY;
        double Corner(double dx, double dy, int xp, int yp) { double a = 0.5 - dx * dx - dy * dy; if (a <= 0.0) { return 0.0; } a *= a; a *= a; return a * GradCoord(seed, xp, yp, dx, dy); }
        return SimplexBound2D * (Corner(x0, y0, ip, jp) + Corner(x1, y1, ip + i1 * PrimeX, jp + j1 * PrimeY) + Corner(x2, y2, ip + PrimeX, jp + PrimeY));
    }
    public static double Simplex3D(double x, double y, double z, NoiseLattice lattice) {
        int seed = lattice.Seed;
        double r = (x + y + z) * SimplexRotate3D;
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
        return value * SimplexBound3D;
    }

    // --- [VALUE]
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
    private static readonly (double X, double Y, double Z)[] Edges3D = [
        (0, 1, 1), (0, -1, 1), (0, 1, -1), (0, -1, -1), (1, 0, 1), (-1, 0, 1), (1, 0, -1), (-1, 0, -1), (1, 1, 0), (-1, 1, 0), (1, -1, 0), (-1, -1, 0)];
    private static readonly (double X, double Y, double Z)[] Tail3D = [(1, 1, 0), (0, -1, 1), (-1, 1, 0), (0, -1, -1)];

    private static readonly double[] Directions2D = [
         0.130526192220052,  0.99144486137381,    0.38268343236509,   0.923879532511287,
         0.608761429008721,  0.793353340291235,   0.793353340291235,  0.608761429008721,
         0.923879532511287,  0.38268343236509,    0.99144486137381,   0.130526192220051,
         0.99144486137381,  -0.130526192220051,   0.923879532511287, -0.38268343236509,
         0.793353340291235, -0.60876142900872,    0.608761429008721, -0.793353340291235,
         0.38268343236509,  -0.923879532511287,   0.130526192220052, -0.99144486137381,
        -0.130526192220052, -0.99144486137381,   -0.38268343236509,  -0.923879532511287,
        -0.608761429008721, -0.793353340291235,  -0.793353340291235, -0.608761429008721,
        -0.923879532511287, -0.38268343236509,   -0.99144486137381,  -0.130526192220052,
        -0.99144486137381,   0.130526192220051,  -0.923879532511287,  0.38268343236509,
        -0.793353340291235,  0.608761429008721,  -0.608761429008721,  0.793353340291235,
        -0.38268343236509,   0.923879532511287,  -0.130526192220052,  0.99144486137381];

    private static readonly double[] Gradients2D = Cycle(Directions2D, pairs: 128, lanes: 2);
    private static readonly double[] Gradients3D = BuildGradients3D();

    private static LatticeTables authored = LatticeTables.Unbound;
    private static double[] RandVecs2D => authored.RandVecs2D;
    private static double[] RandVecs3D => authored.RandVecs3D;

    public static void Bind(LatticeTables tables) => authored = tables;

    private static double[] Cycle(double[] source, int pairs, int lanes) {
        double[] table = new double[pairs * lanes];
        int stride = source.Length / lanes;
        for (int p = 0; p < pairs; p++) {
            for (int lane = 0; lane < lanes; lane++) { table[(p * lanes) + lane] = source[((p % stride) * lanes) + lane]; }
        }
        return table;
    }

    private static double[] BuildGradients3D() {
        double[] table = new double[256];
        for (int q = 0; q < 64; q++) { (double x, double y, double z) = q < 60 ? Edges3D[q % 12] : Tail3D[q - 60]; (table[4 * q], table[4 * q + 1], table[4 * q + 2]) = (x, y, z); }
        return table;
    }
}

public readonly record struct LatticeAsset(string Name, int Entries, int Lanes, Option<ContentAddress> Pin);

public readonly record struct LatticeTables(double[] RandVecs2D, double[] RandVecs3D) {
    public static readonly LatticeTables Unbound = new([], []);

    public static readonly LatticeAsset Vecs2D = new("fnl-randvecs-2d", Entries: 256, Lanes: 2, Pin: Option<ContentAddress>.None);
    public static readonly LatticeAsset Vecs3D = new("fnl-randvecs-3d", Entries: 256, Lanes: 4, Pin: Option<ContentAddress>.None);

    public static Fin<LatticeTables> Of(ReadOnlyMemory<byte> vecs2D, ReadOnlyMemory<byte> vecs3D, Op key) =>
        from a in Admit(Vecs2D, vecs2D, key)
        from b in Admit(Vecs3D, vecs3D, key)
        select new LatticeTables(a, b);

    static Fin<double[]> Admit(LatticeAsset asset, ReadOnlyMemory<byte> payload, Op key) =>
        from pin in asset.Pin.ToFin(new MaterialFault.Parameter(key, $"<lattice-asset-unpinned:{asset.Name}>"))
        from _ in guard(payload.Length == asset.Entries * asset.Lanes * sizeof(double),
                new MaterialFault.Parameter(key, $"<lattice-asset-extent:{asset.Name}:{payload.Length}>"))
        from __ in guard(ContentAddress.Of(payload.Span) == pin,
                new MaterialFault.Parameter(key, $"<lattice-asset-digest:{asset.Name}>"))
        select MemoryMarshal.Cast<byte, double>(payload.Span).ToArray();
}

public static class TextureUv {
    public static Fin<ShadeVec4> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key) =>
        Sampled(source, point, point.U.Value, point.V.Value, sampler, key);

    static Fin<ShadeVec4> Sampled(TextureSource source, UvSample point, double u, double v, SamplerState sampler, Op key) =>
        source.Switch(
            state:     (Point: Anchored(point, u, v, sampler), sampler, key),
            noise:     static (s, n) => Fin.Succ(SampleNoise(n, s.Point.U.Value, s.Point.V.Value, s.Point)),
            checker:   static (s, c) => Fin.Succ(SampleChecker(c, s.Point.U.Value, s.Point.V.Value)),
            gradient:  static (s, g) => Fin.Succ(SampleGradient(g, s.Point)),
            image:     static (s, img) => SampleImage(img, s.Point, s.sampler, s.key),
            triplanar: static (s, t) => SampleTriplanar(t, s.Point, s.sampler, s.key));

    static UvSample Anchored(UvSample point, double u, double v, SamplerState sampler) =>
        sampler.Frame.Apply(u, v) switch {
            var (framedU, framedV) => point.At(sampler.AddressU.Apply(framedU), sampler.AddressV.Apply(framedV)),
        };

    public static Func<double, double, Option<double>, PortValue> Port(
        TextureSource source, UvSample anchor, SamplerState sampler, Channel channel, Op key) =>
        (u, v, parameter) => Sampled(source, anchor with { Parameter = parameter }, u, v, sampler, key)
            .Match(Succ: field => field.IsFinite ? channel.Project(field) : channel.Neutral(), Fail: _ => channel.Neutral());

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

    private static ShadeVec4 SampleGradient(TextureSource.Gradient g, UvSample point) {
        double drive = point.Parameter.IfNone(g.Vertical ? point.V.Value : point.U.Value);
        double t = Math.Clamp(drive, 0.0, 1.0) * (g.Lut.Count - 1);
        int lo = (int)t;
        return lo >= g.Lut.Count - 1 ? g.Lut[g.Lut.Count - 1] : ShadeVec4.Lerp(g.Lut[lo], g.Lut[lo + 1], t - lo);
    }

    private static Fin<ShadeVec4> SampleImage(TextureSource.Image img, UvSample point, SamplerState sampler, Op key) {
        if (img.Levels.IsEmpty) { return new MaterialFault.Parameter(key, "<texture-image-empty>"); }
        double u = point.U.Value, v = point.V.Value;
        double level = Math.Clamp(double.IsFinite(point.MipLevel) ? point.MipLevel : 0.0, 0.0, img.Levels.Count - 1.0);
        if (sampler.Filter != FilterMode.Trilinear) { return Fin.Succ(ReconstructLevel(img.Levels[(int)Math.Floor(level + 0.5)], u, v, sampler, sampler.Filter)); }
        int lo = (int)Math.Floor(level), hi = Math.Min(lo + 1, img.Levels.Count - 1);
        return Fin.Succ(ShadeVec4.Lerp(
            ReconstructLevel(img.Levels[lo], u, v, sampler, FilterMode.Bilinear),
            ReconstructLevel(img.Levels[hi], u, v, sampler, FilterMode.Bilinear), level - lo));
    }

    private static ShadeVec4 ReconstructLevel(ReadOnlyMemory2D<ShadeVec4> plane, double u, double v, SamplerState sampler, FilterMode filter) =>
        filter.Switch(
            state:     (plane, u, v, sampler),
            nearest:   static (s, _) => NearestTap(s.plane.Span, s.u, s.v, s.sampler),
            bilinear:  static (s, _) => BilinearTap(s.plane.Span, s.u, s.v, s.sampler),
            bicubic:   static (s, _) => BicubicTap(s.plane.Span, s.u, s.v, s.sampler),
            trilinear: static (s, _) => BilinearTap(s.plane.Span, s.u, s.v, s.sampler));

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

    private static Fin<ShadeVec4> SampleTriplanar(TextureSource.Triplanar t, UvSample point, SamplerState sampler, Op key) {
        Vector3d n = point.Normal;
        double ax = Math.Pow(Math.Abs(n.X), t.BlendSharpness), ay = Math.Pow(Math.Abs(n.Y), t.BlendSharpness), az = Math.Pow(Math.Abs(n.Z), t.BlendSharpness);
        double sum = ax + ay + az;
        if (sum <= double.Epsilon) { return new MaterialFault.Parameter(key, "<triplanar-degenerate-normal>"); }
        Vector3d p = point.World * t.Scale;
        Fin<ShadeVec4> Plane(double a, double b) => Sampled(t.Projected, point with { World = p }, a, b, sampler, key);
        return from x in Plane(p.Y, p.Z)
               from y in Plane(p.Z, p.X)
               from z in Plane(p.X, p.Y)
               select (x * (ax / sum)) + (y * (ay / sum)) + (z * (az / sum));
    }
}
```

## [03]-[PERIOD_ORACLE]

- Owner: `PeriodOracle` the per-source fixture row; `PeriodProof` the fixture table and its one verdict fold.
- Cases: `Admitted` carries the shift at which its field must repeat; `Refused` carries the reason token `Noise.Of` must produce. One table, two CASES, so a refusal fixture cannot drift into a second roster nobody iterates and cannot carry a measurement column it never fills.
- Entry: `public static Fin<Unit> Prove(PeriodOracle row, Op key)` runs the row's own verdict — admit-then-shift or refuse-with-reason — and `PeriodProof.All` is the roster the proof suite and the `Projection/benchmarks#WORKLOAD_ROWS` parity read both iterate, so a new periodic capability reaches both with no further edit.
- Law: `PeriodOracle` asserts the SHIFT EQUALITY itself, never a transcribed constant — `f(u + Δ, v) = f(u, v)` is exactly what periodicity means, so a regenerated gradient table, a re-tuned normalizer, or a widened basis roster cannot invalidate the fixture while a real seam always breaks it. `Tolerance` rides as an `Admitted` column reading the one `PeriodProof.SeamTolerance` floor, which is arithmetic rather than algorithmic: the shift adds a whole number of lattice units to a coordinate whose fractional part then re-derives at a larger exponent, so the interpolant's own input loses low mantissa bits even where the wrapped cell index is bit-identical. Exact equality asserts a property IEEE arithmetic does not have.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new periodic capability is one admitted row; a new admission gate is one refused row carrying its reason token — never a second proof entry and never a fixture file, since every row's source is authored inline from its own columns.
- Boundary: `Shifted` sweeps a DETERMINISTIC stratified lattice — sample `i` reads `u = (i + ½)/n` against a coprime-strided `v`, so the sweep needs no RNG, carries no state outside `(row, n)`, and covers both axes of every cell in the period. `PeriodProof` reads the `ProceduralNoise.Evaluate` fold rather than a basis arm directly, so it grades the WHOLE evaluation chain — octave ladder, amplitude cascade, domain warp, cellular projection — exactly as a bake drives it; a fixture over one basis call passes while the ladder seams at octave two.

```csharp

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PeriodOracle {
    private PeriodOracle(string name, Func<Op, Fin<TextureSource.Noise>> mint) => (Name, Mint) = (name, mint);

    public string Name { get; }
    public Func<Op, Fin<TextureSource.Noise>> Mint { get; }

    public sealed record Admitted(
        string Name, Func<Op, Fin<TextureSource.Noise>> Mint, double ShiftU, double ShiftV, int Samples, double Tolerance)
        : PeriodOracle(Name, Mint);
    public sealed record Refused(string Name, Func<Op, Fin<TextureSource.Noise>> Mint, string Reason)
        : PeriodOracle(Name, Mint);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PeriodProof {
    static Func<Op, Fin<TextureSource.Noise>> Row(NoiseBasis basis, double frequency, int octaves, double lacunarity, int period,
        FractalMode? fractal = null, CellularParams? cellular = null, DomainWarp? warp = null, bool solid = false) =>
        key => TextureSource.Noise.Of(basis, frequency, key,
            octaves: octaves, lacunarity: lacunarity, fractal: fractal, cellular: cellular, warp: warp, solid: solid,
            period: NoisePeriod.Of(period));

    public const double SeamTolerance = 1e-12;
    public const int SweepSamples = 512;

    public static readonly Seq<PeriodOracle> All = Seq<PeriodOracle>(
        new PeriodOracle.Admitted("perlin.single",   Row(NoiseBasis.Perlin, frequency: 4.0,  octaves: 1, lacunarity: 2.0, period: 4),  ShiftU: 1.0, ShiftV: 0.0, SweepSamples, SeamTolerance),
        new PeriodOracle.Admitted("perlin.fbm4",     Row(NoiseBasis.Perlin, frequency: 4.0,  octaves: 4, lacunarity: 2.0, period: 4),  ShiftU: 1.0, ShiftV: 0.0, SweepSamples, SeamTolerance),
        new PeriodOracle.Admitted("perlin.ridged",   Row(NoiseBasis.Perlin, frequency: 8.0,  octaves: 3, lacunarity: 2.0, period: 4, fractal: FractalMode.Ridged),   ShiftU: 0.5, ShiftV: 0.5, SweepSamples, SeamTolerance),
        new PeriodOracle.Admitted("value.pingpong",  Row(NoiseBasis.Value,  frequency: 6.0,  octaves: 2, lacunarity: 3.0, period: 3, fractal: FractalMode.PingPong), ShiftU: 0.0, ShiftV: 1.0, SweepSamples, SeamTolerance),
        new PeriodOracle.Admitted("worley.vein",     Row(NoiseBasis.Worley, frequency: 4.0,  octaves: 1, lacunarity: 2.0, period: 4, cellular: new CellularParams(CellularDistance.Euclidean, CellularReturn.Distance2Sub, 1.0)), ShiftU: 1.0, ShiftV: 0.0, SweepSamples, SeamTolerance),
        new PeriodOracle.Admitted("perlin.warped",   Row(NoiseBasis.Perlin, frequency: 4.0,  octaves: 2, lacunarity: 2.0, period: 4, warp: new DomainWarp(0.35, 2.0, 99)), ShiftU: 1.0, ShiftV: 1.0, SweepSamples, SeamTolerance),
        new PeriodOracle.Admitted("value.solid",     Row(NoiseBasis.Value,  frequency: 4.0,  octaves: 1, lacunarity: 2.0, period: 4, solid: true), ShiftU: 1.0, ShiftV: 0.0, SweepSamples, SeamTolerance),
        new PeriodOracle.Refused("refuse.simplex",     Row(NoiseBasis.Simplex, frequency: 4.0, octaves: 1, lacunarity: 2.0, period: 4), "<noise-period-unwrappable-basis"),
        new PeriodOracle.Refused("refuse.lacunarity",  Row(NoiseBasis.Perlin, frequency: 4.0, octaves: 3, lacunarity: 2.5, period: 4), "<noise-period-fractional-lacunarity"),
        new PeriodOracle.Refused("refuse.frequency",   Row(NoiseBasis.Perlin, frequency: 6.0, octaves: 1, lacunarity: 2.0, period: 4), "<noise-period-frequency-not-multiple"),
        new PeriodOracle.Refused("refuse.warpfreq",    Row(NoiseBasis.Perlin, frequency: 4.0, octaves: 1, lacunarity: 2.0, period: 4, warp: new DomainWarp(0.35, 1.5, 99)), "<noise-period-fractional-warp-frequency"));

    public static Fin<Unit> Prove(PeriodOracle row, Op key) =>
        row.Switch(
            state:    key,
            admitted: static (k, a) => a.Mint(k).Bind(source => Shifted(source, a, k)),
            refused:  static (k, r) => r.Mint(k).Match(
                Succ: _ => Fin.Fail<Unit>(new MaterialFault.Parameter(k, $"<period-oracle-admitted-refusable:{r.Name}:{r.Reason}>")),
                Fail: error => error is MaterialFault.Parameter parameter
                    && parameter.Detail.StartsWith(r.Reason[..^1], StringComparison.Ordinal)
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new MaterialFault.Parameter(k, $"<period-oracle-wrong-refusal:{r.Name}:{error.Code}>"))));

    static Fin<Unit> Shifted(TextureSource.Noise source, PeriodOracle.Admitted row, Op key) =>
        from seam in Tolerance.Of(ToleranceLane.Residual, row.Tolerance, key)
        from _ in toSeq(Enumerable.Range(0, row.Samples))
            .Map(i => (U: (i + 0.5) / row.Samples, V: ((i * 7919) % row.Samples + 0.5) / row.Samples))
            .Map(at => (At: at, Delta: Math.Abs(Field(source, at.U, at.V) - Field(source, at.U + row.ShiftU, at.V + row.ShiftV))))
            .Filter(probe => !(probe.Delta <= seam.Value))
            .Head
            .Map(probe => Fin.Fail<Unit>(new MaterialFault.Parameter(key,
                $"<period-oracle-seam:{row.Name}:{seam.Lane.Key}:u={probe.At.U:R}:v={probe.At.V:R}:delta={probe.Delta:R}>")))
            .IfNone(Fin.Succ(unit))
        select unit;

    static double Field(TextureSource.Noise source, double u, double v) =>
        source.Solid
            ? ProceduralNoise.Evaluate(source, u, v, u + v)
            : ProceduralNoise.Evaluate(source, u, v);
}
```

## [04]-[RESEARCH]

(none)
