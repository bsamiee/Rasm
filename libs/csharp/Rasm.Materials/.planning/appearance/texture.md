# [MATERIALS_TEXTURE]

THE UV-AND-SOLID SAMPLING ENGINE. One `TextureUv` static sampling fold over the closed `TextureSource` `[Union]` (noise · checker · gradient · image · triplanar), addressed by the closed `AddressMode` band, reconstructed by the closed `FilterMode` band, and seeded by the author-kernel `ProceduralNoise` over the closed `NoiseBasis` band (`Perlin` gradient · `Simplex` OpenSimplex2 · `Value` lattice · `Worley` cellular) — each basis carrying its 2D AND 3D arm so triplanar and solid texturing sample the same lattice — with the fractal trajectory carried by the closed `FractalMode` axis (`FBm` · `Ridged` · `PingPong`) and the cellular feature algebra by the orthogonal `CellularDistance` × `CellularReturn` bands, all vendored inline from the FastNoiseLite algorithm. A texture variation is a `TextureSource` CASE, a sampling mode is an `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` ROW, a color is the one `Unicolour` carrier and a sampled field the one `ShadeVec4` register — never a parallel sampler, a per-filter method, a parallel fractal-vs-basis enum, a `NoiseSampler3D` surface, or a second color register. The page composes the kernel `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail, the `graph#MATERIAL_GRAPH` `PortValue`/`PortId`/`ShadePoint` carriers the node DAG threads, the Rasm/Vectors `UnitInterval`/`Dimension`/`Vector3d` value-objects for UV coordinates, image extents, and world position, and Wacton.Unicolour directly as the scene-linear color owner for every color literal — never re-minting a color space, a coordinate primitive, or a fault. The terminal seam is the `graph#MATERIAL_GRAPH` `AppearanceNode.Texture(Func<double,double,PortValue> Sample)` node: `TextureUv.Port` mints that total `(u,v)→PortValue` closure from a `TextureSource`, so a sampled texture drives a node DAG without a second sampler API; the deep `Sample` rail stays for the wire and the masked-aging consumer that wants the `Fin`.

## [01]-[INDEX]

- [01]-[TEXTURE_UV]: the `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` bands, the `ShadeVec4` four-lane field register, the `TextureSource` union, the `ProceduralNoise` author-kernel, the one `TextureUv.Sample` fold, and the `TextureUv.Port` graph-node bridge.

## [02]-[TEXTURE_UV]

- Owner: `TextureUv` static sampling fold; `AddressMode`/`FilterMode`/`NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` `[SmartEnum<int>]` bands; `ShadeVec4` field register; `ProceduralNoise` author-kernel; `TextureSource` `[Union]`.
- Cases: address {`Repeat`, `Clamp`, `Mirror`} · filter {`Nearest`, `Bilinear`, `Bicubic`, `Trilinear`} · noise-basis {`Perlin`, `Simplex`, `Value`, `Worley`} (fBm is octave-summation over a basis, `Octaves > 1`, never a fifth basis) · fractal {`FBm`, `Ridged`, `PingPong`} · cellular-distance {`EuclideanSq`, `Euclidean`, `Manhattan`, `Hybrid`} · cellular-return {`CellValue`, `Distance`, `Distance2`, `Distance2Sub`, `Distance2Add`, `Distance2Mul`} · source {`Noise`, `Checker`, `Gradient`, `Image`, `Triplanar`}.
- Entry: `public static Fin<ShadeVec4> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key)` is the deep field rail and `public static Func<double, double, PortValue> Port(TextureSource source, UvSample anchor, SamplerState sampler, Channel channel, Op key)` is the `graph#MATERIAL_GRAPH` `AppearanceNode.Texture` bridge — `Port` captures the source/sampler/key and returns the TOTAL `(u,v)→PortValue` closure the node fold reads (`Channel` projects the field to `PortValue.Color`, `.Scalar`, or `.Vector`), a non-finite/undersized/degenerate sample folding to the channel's neutral `PortValue` so the graph arm stays total while the deep `Sample` rail carries the `Op key`-correlated `MaterialFault`; arity is one — a texture variation discriminates on the `TextureSource` union case and a sample modality on `Channel`, never on a sibling sampler method.
- Packages: Rasm.Materials.Appearance.Bsdf (`MaterialFault` band-2450, `ComparerAccessors.StringOrdinal`), Rasm (project — `UnitInterval`/`Dimension`/`Vector3d`/`Point3d`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<int>]` at the deepest surface — generated total `Switch`, `[UseDelegateFromConstructor]` behavior columns), LanguageExt.Core (`Fin`/`Seq`/`Bind`/`Fold`), Wacton.Unicolour (scene-linear color owner), BCL inbox (`FrozenDictionary`/`ReadOnlyMemory<ShadeVec4>`).
- Growth: a new addressing rule is one `AddressMode` row, a new reconstruction filter one `FilterMode` row, a new leaf noise basis one `NoiseBasis` row binding one `ProceduralNoise.Sample2D`/`Sample3D` arm pair, a new fractal trajectory one `FractalMode` row, a new cellular feature one `CellularReturn`/`CellularDistance` row, a new texture one `TextureSource` case carrying its `MtlxCategory`, a new sampled-channel modality one `Channel` row — never a parallel `BilinearSampler`/`PerlinTexture`/`NoiseSampler3D` surface and never a parallel fractal-kind enum since the fractal trajectory is a `FractalMode` row over the basis octave-sum. The `NoiseBasis` set is the FastNoiseLite leaf-basis family (the `Perlin`/`Simplex`/`Value`/`Worley` gradient·simplex·lattice·cellular quartet) projecting onto the MaterialX 1.39 `noise2d`/`fractal2d`/`cellnoise2d`/`worleynoise2d` categories; `ValueCubic` is one `NoiseBasis` row binding one `ProceduralNoise` arm and one `MtlxNode`, not a new noise class. The MaterialX-1.39 node-category parity is REALIZED at `[MATERIALX_NODE_PARITY]` — the `TextureSource.MtlxCategory`/`NoiseBasis.MtlxNode`/`AddressMode.MtlxAddress`/`FilterMode.MtlxFilter` projections the `interchange#MATERIALX_DOCUMENT` `Mtlx.CategoryOf` resolves against the closed `NodeCategory` set.
- Boundary: UV coordinates enter as Rasm/Vectors `UnitInterval` pairs (the `[0,1]` validated value-object), image extents as `Dimension` (the `>=1` int-backed value-object), world position and normal as host `Vector3d`; the sampler NEVER re-mints a coordinate or extent primitive. The interior noise/checker/gradient/image/triplanar algebra runs over the one `ShadeVec4` four-lane field register (`X`/`Y`/`Z` the scalar-field/color lanes, `W` the texel alpha the `Image` reconstruction premultiplies and `Channel.Scalar`/`Channel.Mask` reads) — `ShadeVec4` is the texture field carrier distinct from the `bsdf#LOBE_FAMILY` `RgbSpectrum` validated reflectance: a noise field is signed, a normal-map decode is `[-1,1]`, and a texel carries alpha, none of which the non-negative-validated `RgbSpectrum` admits, so the field stays the raw register and crosses to the validated reflectance only through `ShadeVec4.AsColor`. Color crosses the axis exactly once: color literals on `TextureSource` rows (`Low`/`High`, `Even`/`Odd`, gradient `Stops`) enter as `Unicolour` and decompose to `ShadeVec4` through `ShadeVec4.FromColor` for the field math, and the single `ShadeVec4.AsColor` adapter constructs the canonical scene-linear `Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z)` at the projection tail — the sampler NEVER mints a second color register. `AddressMode.Apply` folds a raw continuous UV into `[0,1)` once before any non-image filter touches a coordinate, and image reconstruction addresses exclusively through the discrete `AddressMode.Texel` companion so the wrap arithmetic is consulted once per axis, not double-applied at the mip seam; `FilterMode` reconstructs through one weight algebra (`Nearest` snaps, `Bilinear` is the unit-square lerp, `Bicubic` is the separable Catmull-Rom 4×4 convolution, `Trilinear` blends two `ReconstructLevel` taps across the mip pyramid by the fractional level `SampleImage` decomposes — `ReconstructLevel` itself dispatches only the spatial `Nearest`/`Bilinear`/`Bicubic` kernels, the mip blend the `Trilinear` arm's own concern); the FastNoiseLite gradient/simplex/value/cellular kernels are author-folds over the hashed lattice (no managed lib owns 2D/3D coherent noise, the `LIBRARY_DEPTH` NOT_COVERED carve-out) with the published FNL anchors — `PrimeX`/`PrimeY`/`PrimeZ` lattice primes, the quintic fade `6t⁵−15t⁴+10t³`, the 2D simplex skew `(√3−1)/2` and unskew `(3−√3)/6`, the 3D OpenSimplex2 skew/unskew `1/3`/`1/6`, the hash-indexed 8-direction 2D and 12-cube-edge 3D unit-gradient tables and the `ValueCoord` hash→`[-1,1]` lattice projection — vendored inline as kernel literals; the fractal trajectory is the `FractalMode` octave-fold (`FBm` the signed amplitude sum, `Ridged` the `1−|n|` folded sum, `PingPong` the triangle-wave fold) with the `WeightedStrength` octave-damping FNL carries, never a hardcoded linear sum, and the `Fbm` self-base is unrepresentable — `NoiseBasis` excludes it; the cellular kernel folds the `CellularDistance` metric over the 3×3 (2D) or 3×3×3 (3D) feature neighbourhood and projects the `CellularReturn` feature (`Distance` the F1 nearest, `Distance2` the F2, `Distance2Sub` the F2−F1 vein, `CellValue` the per-cell hash) with the `CellularParams.Jitter` feature-point displacement, so the `Worley` arm spans the full cellular family rather than the single F1 distance; the `ProceduralNoise` hash-lattice fills and the fixed neighbourhood / closed three-corner simplex loops are the page's `[EXPRESSION_SPINE]` kernel exemption, in-place by index over the per-shade hot path; triplanar projects a world point onto the three axis planes, wraps each through the sampler's `AddressMode.Apply` (never a parallel `Frac`), and blends by the squared-normal weight so the same `TextureSource` evaluates without a UV unwrap, and a `Noise` source under triplanar samples the 3D basis arm directly so solid noise needs no plane projection; out-of-gamut or non-finite results rail to `MaterialFault` through the deep `Sample` rail, and the `Port` closure folds that fault to the `Channel` neutral so the graph arm never sees a sentinel texel.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Domain;                       // Op (the boundary-admission key)
using Rasm.Vectors;                      // UnitInterval, Dimension (the [0,1] / >=1 kernel value-objects)
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault (band 2450), ComparerAccessors
using Rasm.Materials.Appearance.Graph;   // PortValue (the scene-linear Configuration owner — PortValue.SceneLinear), PortId, ShadePoint
using Rhino.Geometry;                    // Vector3d, Point3d
using Wacton.Unicolour;
using Thinktecture;
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
// gradient (Perlin→noise2d), simplex (Simplex→fractal2d — MaterialX folds simplex under the fractal node, the FNL OpenSimplex2
// kernel the documented LIBRARY_DEPTH carve-out the implementation differs by), lattice/value (Value→cellnoise2d, the value-noise
// analogue interchange#MATERIALX_DOCUMENT enumerates), and cellular (Worley→worleynoise2d) families; ValueCubic is one more row.
[SmartEnum<int>]
public sealed partial class NoiseBasis {
    public static readonly NoiseBasis Perlin  = new(0, mtlxNode: "noise2d",        sample2D: ProceduralNoise.Perlin2D,  sample3D: ProceduralNoise.Perlin3D);
    public static readonly NoiseBasis Simplex = new(1, mtlxNode: "fractal2d",      sample2D: ProceduralNoise.Simplex2D, sample3D: ProceduralNoise.Simplex3D);
    public static readonly NoiseBasis Value   = new(2, mtlxNode: "cellnoise2d",    sample2D: ProceduralNoise.Value2D,   sample3D: ProceduralNoise.Value3D);
    public static readonly NoiseBasis Worley  = new(3, mtlxNode: "worleynoise2d",  sample2D: ProceduralNoise.Worley2D,  sample3D: ProceduralNoise.Worley3D);
    public string MtlxNode { get; }

    [UseDelegateFromConstructor]
    public partial double Sample2D(double x, double y, int seed, CellularParams cellular);
    [UseDelegateFromConstructor]
    public partial double Sample3D(double x, double y, double z, int seed, CellularParams cellular);
}

// The fractal octave-accumulation trajectory (FastNoiseLite FractalType): FBm sums signed amplitudes, Ridged sums 1−|n| folds for
// sharp creases, PingPong folds through a triangle wave for marbled bands. WeightedStrength damps higher octaves; the per-octave
// fold takes the running sum, the basis sample, the octave amplitude, and the weight so one octave loop drives all three trajectories.
[SmartEnum<int>]
public sealed partial class FractalMode {
    public static readonly FractalMode FBm     = new(0, mtlxFold: "fbm",       fold: static (n, amp) => n * amp,                              norm: static n => n);
    public static readonly FractalMode Ridged  = new(1, mtlxFold: "ridged",    fold: static (n, amp) => (1.0 - Math.Abs(n)) * amp,            norm: static n => n);
    public static readonly FractalMode PingPong = new(2, mtlxFold: "pingpong", fold: static (n, amp) => PingPongWave((n + 1.0) * 2.0) * amp,  norm: static n => n);
    public string MtlxFold { get; }

    // The per-octave contribution: maps the raw basis sample at this octave to its trajectory contribution scaled by amplitude.
    [UseDelegateFromConstructor]
    public partial double Fold(double sample, double amplitude);
    // The post-sum normalization (identity for the amplitude-normalized accumulation; Ridged/PingPong already land in range).
    [UseDelegateFromConstructor]
    public partial double Norm(double summed);

    private static double PingPongWave(double t) { t -= (int)(t * 0.5) * 2.0; return t < 1.0 ? t : 2.0 - t; }
}

// The cellular distance metric (FastNoiseLite CellularDistanceFunction): EuclideanSq the squared default, Euclidean the rooted, Manhattan
// the L1 taxicab, Hybrid the L1+L2 combination. The delegate accumulates one neighbour's distance into the running F1/F2 minima.
[SmartEnum<int>]
public sealed partial class CellularDistance {
    public static readonly CellularDistance EuclideanSq = new(0, metric: static (dx, dy, dz) => dx * dx + dy * dy + dz * dz);
    public static readonly CellularDistance Euclidean   = new(1, metric: static (dx, dy, dz) => Math.Sqrt(dx * dx + dy * dy + dz * dz));
    public static readonly CellularDistance Manhattan   = new(2, metric: static (dx, dy, dz) => Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz));
    public static readonly CellularDistance Hybrid      = new(3, metric: static (dx, dy, dz) => Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) + (dx * dx + dy * dy + dz * dz));

    [UseDelegateFromConstructor]
    public partial double Metric(double dx, double dy, double dz);
}

// The cellular feature projection (FastNoiseLite CellularReturnType): CellValue the per-cell hash, Distance the F1, Distance2 the F2,
// Distance2Sub/Add/Mul the F2∓/×F1 vein-and-blob combinations. The delegate maps the two nearest feature distances (and the F1 cell hash)
// to the returned scalar, centred to [-1,1] for the field; the F1-only single distance is one row of this closed return family.
[SmartEnum<int>]
public sealed partial class CellularReturn {
    public static readonly CellularReturn CellValue    = new(0, project: static (f1, f2, cell) => cell);
    public static readonly CellularReturn Distance     = new(1, project: static (f1, f2, cell) => f1 - 1.0);
    public static readonly CellularReturn Distance2    = new(2, project: static (f1, f2, cell) => f2 - 1.0);
    public static readonly CellularReturn Distance2Sub = new(3, project: static (f1, f2, cell) => f2 - f1 - 1.0);
    public static readonly CellularReturn Distance2Add = new(4, project: static (f1, f2, cell) => (f2 + f1) * 0.5 - 1.0);
    public static readonly CellularReturn Distance2Mul = new(5, project: static (f1, f2, cell) => f2 * f1 - 1.0);

    [UseDelegateFromConstructor]
    public partial double Project(double f1, double f2, double cellHash);
}

// The graph-node sample modality: a Texture node reads ONE channel of the field, so Port projects the ShadeVec4 to a PortValue by
// this row — Color the RGB triple, Scalar the AP1-primary luminance mask, Mask the alpha lane (the texel coverage), Vector the signed
// normal-map decode. The neutral fallback the total graph closure folds a fault to lives on the row, so a degenerate sample is a
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

    // A noise source carries its leaf basis, fractal trajectory, and cellular feature algebra as rows — frequency/octaves/lacunarity/gain
    // the fBm spectrum, FractalMode the octave fold, CellularParams the Worley feature family, WeightedStrength the octave damp, Warp the
    // pre-sample domain-warp displacement, Solid the 3D-vs-2D arm. A new procedural variation is a parameter change, never a second source.
    public sealed record Noise(
        NoiseBasis Base, double Frequency, int Octaves, double Lacunarity, double Gain, int Seed,
        FractalMode Fractal, double WeightedStrength, CellularParams Cellular, DomainWarp Warp, bool Solid,
        Unicolour Low, Unicolour High) : TextureSource;
    public sealed record Checker(int Repeats, Unicolour Even, Unicolour Odd) : TextureSource;
    public sealed record Gradient(bool Vertical, Seq<(UnitInterval At, Unicolour Color)> Stops) : TextureSource;
    public sealed record Image(Dimension Width, Dimension Height, Seq<ReadOnlyMemory<ShadeVec4>> Levels) : TextureSource;
    public sealed record Triplanar(TextureSource Projected, double Scale, double BlendSharpness) : TextureSource;

    // The MaterialX 1.39 standard-library node category each TextureSource round-trips to (interchange#MATERIALX_DOCUMENT consumes via
    // Mtlx.CategoryOf): a fractal noise (Octaves>1 over a gradient/value basis) is the fractal2d node, a single-octave noise its
    // NoiseBasis.MtlxNode, a checker→checkerboard, a gradient→ramplr/ramptb by axis, an image→tiledimage, a triplanar→triplanarprojection
    // — the case IS the category, and every returned string is a closed NodeCategory key the document resolves.
    public string MtlxCategory => Switch(
        noise:     static n => n.Octaves > 1 && n.Base != NoiseBasis.Worley ? "fractal2d" : n.Base.MtlxNode,
        checker:   static _ => "checkerboard",
        gradient:  static g => g.Vertical ? "ramptb" : "ramplr",
        image:     static _ => "tiledimage",
        triplanar: static _ => "triplanarprojection");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The cellular feature algebra a Noise source threads into the Worley basis arm: the distance metric, the returned feature, and the
// feature-point jitter. A non-Worley basis ignores it (the Perlin/Simplex/Value arms discard the parameter), so one parameter shape rides
// every basis without a Worley-only field, and CellularParams.Default is the EuclideanSq F1 a non-cellular Noise source carries unread.
public readonly record struct CellularParams(CellularDistance Distance, CellularReturn Return, double Jitter) {
    public static readonly CellularParams Default = new(CellularDistance.EuclideanSq, CellularReturn.Distance, 1.0);
}

// The domain-warp pre-displacement (FastNoiseLite DomainWarp): a non-zero Amp offsets the sample coordinate by a Simplex-warp vector
// before the basis samples, so a procedural pattern flows and swirls rather than reads on an axis grid. Amp 0 is the identity (no warp).
public readonly record struct DomainWarp(double Amplitude, double Frequency, int Seed) {
    public static readonly DomainWarp None = new(0.0, 1.0, 0);
}

// The one texture field register: X/Y/Z the scalar-field/color/vector lanes, W the texel alpha (coverage) the Image reconstruction
// premultiplies and Channel.Mask reads. Distinct from the bsdf#LOBE_FAMILY RgbSpectrum validated reflectance — a field is signed
// (normal-map decode), alpha-bearing (texel coverage), and unvalidated, so it crosses to the validated scene-linear Unicolour only at AsColor.
public readonly record struct ShadeVec4(double X, double Y, double Z, double W) {
    public static ShadeVec4 Splat(double v) => new(v, v, v, v);
    public static ShadeVec4 operator +(ShadeVec4 a, ShadeVec4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static ShadeVec4 operator *(ShadeVec4 a, double s) => new(a.X * s, a.Y * s, a.Z * s, a.W * s);
    public static ShadeVec4 Lerp(ShadeVec4 a, ShadeVec4 b, double t) => a * (1.0 - t) + b * t;
    public double Luminance => 0.2722287 * X + 0.6740818 * Y + 0.0536895 * Z;   // AP1-primary scene-linear Y, the graph#MATERIAL_GRAPH Color→Scalar projection — X/Y/Z are the Acescg-linear triple, so a Rec709 weight here mis-weights green

    public static ShadeVec4 FromColor(Unicolour colour) {
        ColourTriplet lin = colour.RgbLinear.Triplet;
        return new(lin.First, lin.Second, lin.Third, 1.0);
    }

    // The validated egress: a non-finite RGB lane rails MaterialFault.Gamut; the unchecked form is the total Channel projection where
    // the Port closure has already folded a fault to a neutral, so the graph arm never re-checks.
    public Fin<Unicolour> AsColor(Op key) =>
        double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z)
            ? Fin.Succ(AsColorUnchecked())
            : MaterialFault.Gamut(key, $"<texture-non-finite-rgb:{X:R},{Y:R},{Z:R}>");
    public Unicolour AsColorUnchecked() => new(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z);
}

public readonly record struct UvSample(UnitInterval U, UnitInterval V, Vector3d World, Vector3d Normal, double MipBias) {
    public static Fin<UvSample> Of(double u, double v, Op key) =>
        from cu in key.AcceptValidated<UnitInterval>(candidate: u)
        from cv in key.AcceptValidated<UnitInterval>(candidate: v)
        select new UvSample(cu, cv, Vector3d.Zero, Vector3d.ZAxis, 0.0);

    // The total re-anchor: u/v arrive already address-wrapped into [0,1] (the AddressMode fold idempotent on a wrapped value), so the
    // clamp is the structural totality guarantee that keeps UnitInterval.Create unreachable-throwing — the Port closure and triplanar feed
    // wrapped coordinates, the clamp the proof no raw node UV reaches the throwing factory. Of (the AcceptValidated Fin rail) is the deep entry.
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

    private static int Hash(int seed, int xPrimed, int yPrimed) { int h = seed ^ xPrimed ^ yPrimed; h *= 0x27d4eb2d; return h; }
    private static int Hash(int seed, int xPrimed, int yPrimed, int zPrimed) { int h = seed ^ xPrimed ^ yPrimed ^ zPrimed; h *= 0x27d4eb2d; return h; }
    private static double ValueCoord(int hash) { hash *= 0x27d4eb2d; return (hash & 0x7fffffff) / 1073741823.5 - 1.0; }   // hash → [-1,1] lattice value

    private static double GradCoord(int seed, int xPrimed, int yPrimed, double xd, double yd) {
        int hash = Hash(seed, xPrimed, yPrimed); hash ^= hash >> 15; hash &= 127 << 1;
        return xd * Gradients2D[hash] + yd * Gradients2D[hash | 1];
    }
    private static double GradCoord(int seed, int xPrimed, int yPrimed, int zPrimed, double xd, double yd, double zd) {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed); hash ^= hash >> 15; hash &= 63 << 2;
        return xd * Gradients3D[hash] + yd * Gradients3D[hash | 1] + zd * Gradients3D[hash | 2];
    }

    private static readonly double[] Gradients2D = BuildGradients2D();
    private static readonly double[] Gradients3D = BuildGradients3D();

    private static double Fade(double t) => t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    private static double Lerp(double a, double b, double t) => a + t * (b - a);

    // --- [PERLIN]
    public static double Perlin2D(double x, double y, int seed, CellularParams _) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
        double xd0 = x - x0, yd0 = y - y0, xd1 = xd0 - 1.0, yd1 = yd0 - 1.0;
        double xs = Fade(xd0), ys = Fade(yd0);
        int xp0 = x0 * PrimeX, yp0 = y0 * PrimeY, xp1 = xp0 + PrimeX, yp1 = yp0 + PrimeY;
        double n00 = GradCoord(seed, xp0, yp0, xd0, yd0), n10 = GradCoord(seed, xp1, yp0, xd1, yd0);
        double n01 = GradCoord(seed, xp0, yp1, xd0, yd1), n11 = GradCoord(seed, xp1, yp1, xd1, yd1);
        return Lerp(Lerp(n00, n10, xs), Lerp(n01, n11, xs), ys) * 1.4142135623730951;
    }
    public static double Perlin3D(double x, double y, double z, int seed, CellularParams _) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y), z0 = (int)Math.Floor(z);
        double xd0 = x - x0, yd0 = y - y0, zd0 = z - z0, xd1 = xd0 - 1.0, yd1 = yd0 - 1.0, zd1 = zd0 - 1.0;
        double xs = Fade(xd0), ys = Fade(yd0), zs = Fade(zd0);
        int xp0 = x0 * PrimeX, yp0 = y0 * PrimeY, zp0 = z0 * PrimeZ, xp1 = xp0 + PrimeX, yp1 = yp0 + PrimeY, zp1 = zp0 + PrimeZ;
        double n000 = GradCoord(seed, xp0, yp0, zp0, xd0, yd0, zd0), n100 = GradCoord(seed, xp1, yp0, zp0, xd1, yd0, zd0);
        double n010 = GradCoord(seed, xp0, yp1, zp0, xd0, yd1, zd0), n110 = GradCoord(seed, xp1, yp1, zp0, xd1, yd1, zd0);
        double n001 = GradCoord(seed, xp0, yp0, zp1, xd0, yd0, zd1), n101 = GradCoord(seed, xp1, yp0, zp1, xd1, yd0, zd1);
        double n011 = GradCoord(seed, xp0, yp1, zp1, xd0, yd1, zd1), n111 = GradCoord(seed, xp1, yp1, zp1, xd1, yd1, zd1);
        double xf0 = Lerp(Lerp(n000, n100, xs), Lerp(n010, n110, xs), ys), xf1 = Lerp(Lerp(n001, n101, xs), Lerp(n011, n111, xs), ys);
        return Lerp(xf0, xf1, zs) * 0.9648642450997855;
    }

    // --- [SIMPLEX]
    public static double Simplex2D(double x, double y, int seed, CellularParams _) {
        const double F2 = 0.3660254037844386, G2 = 0.21132486540518713;
        double s = (x + y) * F2;
        int i = (int)Math.Floor(x + s), j = (int)Math.Floor(y + s);
        double t = (i + j) * G2, x0 = x - (i - t), y0 = y - (j - t);
        int i1 = x0 > y0 ? 1 : 0, j1 = x0 > y0 ? 0 : 1;
        double x1 = x0 - i1 + G2, y1 = y0 - j1 + G2, x2 = x0 - 1.0 + 2.0 * G2, y2 = y0 - 1.0 + 2.0 * G2;
        int ip = i * PrimeX, jp = j * PrimeY;
        double Corner(double dx, double dy, int xp, int yp) { double a = 0.5 - dx * dx - dy * dy; if (a <= 0.0) { return 0.0; } a *= a; a *= a; return a * GradCoord(seed, xp, yp, dx, dy); }
        return 70.0 * (Corner(x0, y0, ip, jp) + Corner(x1, y1, ip + i1 * PrimeX, jp + j1 * PrimeY) + Corner(x2, y2, ip + PrimeX, jp + PrimeY));
    }
    public static double Simplex3D(double x, double y, double z, int seed, CellularParams _) {
        const double F3 = 1.0 / 3.0, G3 = 1.0 / 6.0;
        double s = (x + y + z) * F3;
        int i = (int)Math.Floor(x + s), j = (int)Math.Floor(y + s), k = (int)Math.Floor(z + s);
        double t = (i + j + k) * G3, x0 = x - (i - t), y0 = y - (j - t), z0 = z - (k - t);
        int i1, j1, k1, i2, j2, k2;
        if (x0 >= y0) {
            if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
            else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
        } else {
            if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
            else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
            else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
        }
        double x1 = x0 - i1 + G3, y1 = y0 - j1 + G3, z1 = z0 - k1 + G3;
        double x2 = x0 - i2 + 2.0 * G3, y2 = y0 - j2 + 2.0 * G3, z2 = z0 - k2 + 2.0 * G3;
        double x3 = x0 - 1.0 + 3.0 * G3, y3 = y0 - 1.0 + 3.0 * G3, z3 = z0 - 1.0 + 3.0 * G3;
        int ip = i * PrimeX, jp = j * PrimeY, kp = k * PrimeZ;
        double Corner(double dx, double dy, double dz, int xp, int yp, int zp) { double a = 0.6 - dx * dx - dy * dy - dz * dz; if (a <= 0.0) { return 0.0; } a *= a; a *= a; return a * GradCoord(seed, xp, yp, zp, dx, dy, dz); }
        return 32.0 * (Corner(x0, y0, z0, ip, jp, kp)
                     + Corner(x1, y1, z1, ip + i1 * PrimeX, jp + j1 * PrimeY, kp + k1 * PrimeZ)
                     + Corner(x2, y2, z2, ip + i2 * PrimeX, jp + j2 * PrimeY, kp + k2 * PrimeZ)
                     + Corner(x3, y3, z3, ip + PrimeX, jp + PrimeY, kp + PrimeZ));
    }

    // --- [VALUE]
    public static double Value2D(double x, double y, int seed, CellularParams _) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
        double xs = Fade(x - x0), ys = Fade(y - y0);
        int xp0 = x0 * PrimeX, yp0 = y0 * PrimeY, xp1 = xp0 + PrimeX, yp1 = yp0 + PrimeY;
        double v00 = ValueCoord(Hash(seed, xp0, yp0)), v10 = ValueCoord(Hash(seed, xp1, yp0));
        double v01 = ValueCoord(Hash(seed, xp0, yp1)), v11 = ValueCoord(Hash(seed, xp1, yp1));
        return Lerp(Lerp(v00, v10, xs), Lerp(v01, v11, xs), ys);
    }
    public static double Value3D(double x, double y, double z, int seed, CellularParams _) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y), z0 = (int)Math.Floor(z);
        double xs = Fade(x - x0), ys = Fade(y - y0), zs = Fade(z - z0);
        int xp0 = x0 * PrimeX, yp0 = y0 * PrimeY, zp0 = z0 * PrimeZ, xp1 = xp0 + PrimeX, yp1 = yp0 + PrimeY, zp1 = zp0 + PrimeZ;
        double v000 = ValueCoord(Hash(seed, xp0, yp0, zp0)), v100 = ValueCoord(Hash(seed, xp1, yp0, zp0));
        double v010 = ValueCoord(Hash(seed, xp0, yp1, zp0)), v110 = ValueCoord(Hash(seed, xp1, yp1, zp0));
        double v001 = ValueCoord(Hash(seed, xp0, yp0, zp1)), v101 = ValueCoord(Hash(seed, xp1, yp0, zp1));
        double v011 = ValueCoord(Hash(seed, xp0, yp1, zp1)), v111 = ValueCoord(Hash(seed, xp1, yp1, zp1));
        return Lerp(Lerp(Lerp(v000, v100, xs), Lerp(v010, v110, xs), ys), Lerp(Lerp(v001, v101, xs), Lerp(v011, v111, xs), ys), zs);
    }

    // --- [WORLEY]
    // The cellular fold: accumulate the CellularDistance metric over the 3×3 (2D) / 3×3×3 (3D) jittered-feature neighbourhood into the
    // two nearest distances F1/F2 and the F1 cell hash, then project the CellularReturn feature. F1-only is the Distance row; F2−F1, F2,
    // and the cell-value are sibling rows of the SAME fold, so the closed cellular family is one neighbourhood loop, never a per-return kernel.
    public static double Worley2D(double x, double y, int seed, CellularParams cellular) {
        int xr = (int)Math.Round(x), yr = (int)Math.Round(y);
        double f1 = double.MaxValue, f2 = double.MaxValue, cell = 0.0;
        for (int dy = -1; dy <= 1; dy++) {
            for (int dx = -1; dx <= 1; dx++) {
                int cx = xr + dx, cy = yr + dy, h = Hash(seed, cx * PrimeX, cy * PrimeY);
                double fx = cx + (((h & 0x3ff) / 1023.0 - 0.5) * cellular.Jitter), fy = cy + ((((h >> 10) & 0x3ff) / 1023.0 - 0.5) * cellular.Jitter);
                double d = cellular.Distance.Metric(fx - x, fy - y, 0.0);
                if (d < f1) { f2 = f1; f1 = d; cell = ValueCoord(h); } else if (d < f2) { f2 = d; }
            }
        }
        return Math.Clamp(cellular.Return.Project(f1, f2, cell), -1.0, 1.0);
    }
    public static double Worley3D(double x, double y, double z, int seed, CellularParams cellular) {
        int xr = (int)Math.Round(x), yr = (int)Math.Round(y), zr = (int)Math.Round(z);
        double f1 = double.MaxValue, f2 = double.MaxValue, cell = 0.0;
        for (int dz = -1; dz <= 1; dz++) {
            for (int dy = -1; dy <= 1; dy++) {
                for (int dx = -1; dx <= 1; dx++) {
                    int cx = xr + dx, cy = yr + dy, cz = zr + dz, h = Hash(seed, cx * PrimeX, cy * PrimeY, cz * PrimeZ);
                    double fx = cx + (((h & 0x3ff) / 1023.0 - 0.5) * cellular.Jitter), fy = cy + ((((h >> 10) & 0x3ff) / 1023.0 - 0.5) * cellular.Jitter), fz = cz + ((((h >> 20) & 0x3ff) / 1023.0 - 0.5) * cellular.Jitter);
                    double d = cellular.Distance.Metric(fx - x, fy - y, fz - z);
                    if (d < f1) { f2 = f1; f1 = d; cell = ValueCoord(h); } else if (d < f2) { f2 = d; }
                }
            }
        }
        return Math.Clamp(cellular.Return.Project(f1, f2, cell), -1.0, 1.0);
    }

    // --- [FRACTAL]
    // The octave-summation over a leaf basis: the FractalMode owns the per-octave contribution and the WeightedStrength damps each octave
    // by 1 − strength·(1 − |n|) so a row authored with octaves>1 reads its fractal spectrum, the single-octave noise the base sample. Domain
    // warp pre-displaces the coordinate by a Simplex-warp vector when Amp>0, so the pattern flows. 2D and 3D share the fold by an arm selector.
    public static double Evaluate(NoiseBasis basis, double x, double y, int seed, int octaves, double lacunarity, double gain, FractalMode fractal, double weightedStrength, CellularParams cellular, DomainWarp warp) {
        (double wx, double wy) = warp.Amplitude > 0.0 ? Warp2D(x, y, seed, warp) : (x, y);
        double sum = 0.0, amp = 1.0, freq = 1.0, norm = 0.0;
        for (int o = 0; o < Math.Max(1, octaves); o++) {
            double n = basis.Sample2D(wx * freq, wy * freq, seed + o, cellular);
            sum += fractal.Fold(n, amp);
            amp *= gain * (1.0 - weightedStrength * (1.0 - Math.Abs(n)));
            norm += amp; freq *= lacunarity;
        }
        return fractal.Norm(norm > 0.0 ? sum / Math.Max(1.0, norm) : sum);
    }
    public static double Evaluate(NoiseBasis basis, double x, double y, double z, int seed, int octaves, double lacunarity, double gain, FractalMode fractal, double weightedStrength, CellularParams cellular, DomainWarp warp) {
        (double wx, double wy, double wz) = warp.Amplitude > 0.0 ? Warp3D(x, y, z, seed, warp) : (x, y, z);
        double sum = 0.0, amp = 1.0, freq = 1.0, norm = 0.0;
        for (int o = 0; o < Math.Max(1, octaves); o++) {
            double n = basis.Sample3D(wx * freq, wy * freq, wz * freq, seed + o, cellular);
            sum += fractal.Fold(n, amp);
            amp *= gain * (1.0 - weightedStrength * (1.0 - Math.Abs(n)));
            norm += amp; freq *= lacunarity;
        }
        return fractal.Norm(norm > 0.0 ? sum / Math.Max(1.0, norm) : sum);
    }

    private static (double, double) Warp2D(double x, double y, int seed, in DomainWarp w) =>
        (x + w.Amplitude * Simplex2D(x * w.Frequency, y * w.Frequency, w.Seed, EmptyCellular), y + w.Amplitude * Simplex2D(x * w.Frequency + 1000.0, y * w.Frequency, w.Seed, EmptyCellular));
    private static (double, double, double) Warp3D(double x, double y, double z, int seed, in DomainWarp w) =>
        (x + w.Amplitude * Simplex3D(x * w.Frequency, y * w.Frequency, z * w.Frequency, w.Seed, EmptyCellular),
         y + w.Amplitude * Simplex3D(x * w.Frequency + 1000.0, y * w.Frequency, z * w.Frequency, w.Seed, EmptyCellular),
         z + w.Amplitude * Simplex3D(x * w.Frequency, y * w.Frequency + 1000.0, z * w.Frequency, w.Seed, EmptyCellular));
    private static readonly CellularParams EmptyCellular = CellularParams.Default;

    private static double[] BuildGradients2D() {
        double[] dirs = { 0.130526192220052, 0.99144486137381, 0.608761429008721, 0.793353340291235, 0.793353340291235, 0.608761429008721, 0.99144486137381, 0.130526192220051, 0.99144486137381, -0.130526192220051, 0.793353340291235, -0.60876142900872, 0.608761429008721, -0.793353340291235, 0.130526192220052, -0.99144486137381, -0.130526192220052, -0.99144486137381, -0.608761429008721, -0.793353340291235, -0.793353340291235, -0.608761429008721, -0.99144486137381, -0.130526192220052, -0.99144486137381, 0.130526192220051, -0.793353340291235, 0.608761429008721, -0.608761429008721, 0.793353340291235, -0.130526192220052, 0.99144486137381 };
        double[] table = new double[256];
        for (int i = 0; i < table.Length; i++) { table[i] = dirs[i % dirs.Length]; }
        return table;
    }
    private static double[] BuildGradients3D() {
        double[] dirs = { 1, 1, 0, 0, -1, 1, 0, 0, 1, -1, 0, 0, -1, -1, 0, 0, 1, 0, 1, 0, -1, 0, 1, 0, 1, 0, -1, 0, -1, 0, -1, 0, 0, 1, 1, 0, 0, -1, 1, 0, 0, 1, -1, 0, 0, -1, -1, 0 };
        double[] table = new double[256];
        for (int i = 0; i < table.Length; i++) { table[i] = dirs[i % dirs.Length]; }
        return table;
    }
}

public static class TextureUv {
    // The deep field rail: a TextureSource sampled at a UvSample under a SamplerState to one ShadeVec4, the Op key correlating the
    // MaterialFault on a non-finite UV, an undersized image payload, or a degenerate triplanar normal. Color callers project AsColor; the
    // graph node binds through Port. The arity-one entry — the union case discriminates the texture, never a sibling sampler method.
    public static Fin<ShadeVec4> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key) {
        double u = point.U.Value, v = point.V.Value;
        if (!double.IsFinite(u) || !double.IsFinite(v)) { return MaterialFault.Parameter(key, $"<texture-uv-non-finite:{u:R},{v:R}>"); }
        return source.Switch(
            state:     (point, sampler, key),
            noise:     static (s, n) => Fin.Succ(SampleNoise(n, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value), s.point)),
            checker:   static (s, c) => Fin.Succ(SampleChecker(c, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value))),
            gradient:  static (s, g) => SampleGradient(g, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value), s.key),
            image:     static (s, img) => SampleImage(img, s.point, s.sampler, s.key),
            triplanar: static (s, t) => SampleTriplanar(t, s.point, s.sampler, s.key));
    }

    // The graph#MATERIAL_GRAPH AppearanceNode.Texture bridge: capture the source/sampler/key and return the TOTAL (u,v)→PortValue closure
    // the node fold reads. The closure re-anchors the UvSample at (u,v), samples the deep rail, and projects the field through the Channel;
    // a fault folds to the Channel neutral so the graph arm stays total — the deep Sample rail owns the MaterialFault, the closure owns totality.
    public static Func<double, double, PortValue> Port(TextureSource source, UvSample anchor, SamplerState sampler, Channel channel, Op key) =>
        (u, v) => Sample(source, anchor.At(sampler.AddressU.Apply(u), sampler.AddressV.Apply(v)), sampler, key).Match(Succ: channel.Project, Fail: _ => channel.Neutral());

    private static ShadeVec4 SampleNoise(TextureSource.Noise n, double u, double v, UvSample point) {
        double field = n.Solid
            ? ProceduralNoise.Evaluate(n.Base, point.World.X * n.Frequency, point.World.Y * n.Frequency, point.World.Z * n.Frequency, n.Seed, n.Octaves, n.Lacunarity, n.Gain, n.Fractal, n.WeightedStrength, n.Cellular, n.Warp)
            : ProceduralNoise.Evaluate(n.Base, u * n.Frequency, v * n.Frequency, n.Seed, n.Octaves, n.Lacunarity, n.Gain, n.Fractal, n.WeightedStrength, n.Cellular, n.Warp);
        return ShadeVec4.Lerp(ShadeVec4.FromColor(n.Low), ShadeVec4.FromColor(n.High), Math.Clamp((field + 1.0) * 0.5, 0.0, 1.0));
    }

    private static ShadeVec4 SampleChecker(TextureSource.Checker c, double u, double v) {
        int parity = ((int)Math.Floor(u * c.Repeats) + (int)Math.Floor(v * c.Repeats)) & 1;
        return ShadeVec4.FromColor(parity == 0 ? c.Even : c.Odd);
    }

    private static Fin<ShadeVec4> SampleGradient(TextureSource.Gradient g, double u, double v, Op key) {
        Seq<(UnitInterval At, Unicolour Color)> stops = g.Stops;
        if (stops.IsEmpty) { return MaterialFault.Parameter(key, "<texture-gradient-no-stops>"); }
        double t = g.Vertical ? v : u;
        // LanguageExt v5 `Seq.Head` is `Option<A>`; the IsEmpty guard above makes the indexer total, so the mutable
        // lower-stop seed reads `stops[0]` (the `A` indexer) rather than the Option `Head`.
        (UnitInterval At, Unicolour Color) lo = stops[0];
        if (t <= lo.At.Value) { return Fin.Succ(ShadeVec4.FromColor(lo.Color)); }
        foreach ((UnitInterval At, Unicolour Color) hi in stops.Tail) {
            if (t <= hi.At.Value) {
                double span = hi.At.Value - lo.At.Value;
                double f = span > double.Epsilon ? (t - lo.At.Value) / span : 0.0;
                return Fin.Succ(ShadeVec4.Lerp(ShadeVec4.FromColor(lo.Color), ShadeVec4.FromColor(hi.Color), f));
            }
            lo = hi;
        }
        return Fin.Succ(ShadeVec4.FromColor(lo.Color));
    }

    private static Fin<ShadeVec4> SampleImage(TextureSource.Image img, UvSample point, SamplerState sampler, Op key) {
        if (img.Levels.IsEmpty) { return MaterialFault.Parameter(key, "<texture-image-empty>"); }
        double u = point.U.Value, v = point.V.Value;
        if (sampler.Filter != FilterMode.Trilinear) { return ReconstructLevel(img, 0, u, v, sampler, sampler.Filter, key); }
        double level = Math.Clamp(point.MipBias, 0.0, img.Levels.Count - 1.0);
        int lo = (int)Math.Floor(level), hi = Math.Min(lo + 1, img.Levels.Count - 1);
        double f = level - lo;
        return from a in ReconstructLevel(img, lo, u, v, sampler, FilterMode.Bilinear, key)
               from b in ReconstructLevel(img, hi, u, v, sampler, FilterMode.Bilinear, key)
               select ShadeVec4.Lerp(a, b, f);
    }

    // One mip level reconstructed by a spatial FilterMode: SampleImage routes the non-Trilinear filters here directly and the Trilinear mip
    // blend feeds Bilinear per tap, so the total Switch's trilinear arm is unreachable (kept only for exhaustiveness). Texels carry alpha in
    // the W lane; Bilinear/Bicubic premultiply by alpha before the weighted sum and un-premultiply after, so a transparent texel never bleeds
    // opaque color across a coverage edge — the alpha lane the prior page carried but never read is now load-bearing.
    private static Fin<ShadeVec4> ReconstructLevel(TextureSource.Image img, int level, double u, double v, SamplerState sampler, FilterMode filter, Op key) {
        int w = Math.Max(1, img.Width.Value >> level), h = Math.Max(1, img.Height.Value >> level);
        ReadOnlyMemory<ShadeVec4> texels = img.Levels[level];
        if (texels.Length < w * h) { return MaterialFault.Parameter(key, $"<texture-level-undersized:{level}:{texels.Length}<{w * h}>"); }
        ReadOnlySpan<ShadeVec4> span = texels.Span;
        ShadeVec4 At(int ix, int iy) => span[sampler.AddressV.Texel(iy, h) * w + sampler.AddressU.Texel(ix, w)];
        static ShadeVec4 Pre(ShadeVec4 c) => new(c.X * c.W, c.Y * c.W, c.Z * c.W, c.W);
        static ShadeVec4 UnPre(ShadeVec4 c) => c.W > 1e-6 ? new(c.X / c.W, c.Y / c.W, c.Z / c.W, c.W) : c;

        double fx = u * w - 0.5, fy = v * h - 0.5;
        int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
        double tx = fx - x0, ty = fy - y0;
        ShadeVec4 Bilinear() => UnPre(ShadeVec4.Lerp(ShadeVec4.Lerp(Pre(At(x0, y0)), Pre(At(x0 + 1, y0)), tx), ShadeVec4.Lerp(Pre(At(x0, y0 + 1)), Pre(At(x0 + 1, y0 + 1)), tx), ty));

        return Fin.Succ(filter.Switch(
            nearest:   _ => At((int)Math.Round(u * w - 0.5), (int)Math.Round(v * h - 0.5)),
            bilinear:  _ => Bilinear(),
            bicubic:   _ => {
                ShadeVec4 Row(int iy) => CatmullRom(Pre(At(x0 - 1, iy)), Pre(At(x0, iy)), Pre(At(x0 + 1, iy)), Pre(At(x0 + 2, iy)), tx);
                return UnPre(CatmullRom(Row(y0 - 1), Row(y0), Row(y0 + 1), Row(y0 + 2), ty));
            },
            trilinear: _ => Bilinear()));   // unreachable — SampleImage owns the Trilinear mip blend and feeds Bilinear per tap; kept for total-Switch exhaustiveness
    }

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

## [03]-[RESEARCH]

- [SIMPLEX_PATENT]: the noise kernel uses OpenSimplex2 (the FastNoiseLite default), not Perlin's patented Simplex — the patent (US 6,867,776, expired 2022) is moot, but OpenSimplex2 is the vendored basis regardless, with the skewed-lattice three-corner (2D) and four-corner (3D) summation transcribed inline (the 3D `OpenSimplex2` simplex-cell branch the `Simplex3D` arm carries); `ValueCubic` lands as one `NoiseBasis` row binding one `ProceduralNoise.Sample2D`/`Sample3D` arm pair.
- [NOISE_DIMENSIONALITY]: REALIZED — every `NoiseBasis` carries both a `Sample2D` and a `Sample3D` arm, so the 3D gradient/value/simplex/cellular lattice is live rather than a built-unused table. A `Noise` source with `Solid = true` samples the 3D arm from the `UvSample.World` position (solid wood/marble/stone that flows through a carved surface rather than smearing at a UV seam), and triplanar samples the 2D arm on each axis plane; the 3D `Perlin3D`/`Simplex3D` normalization constants (`0.9648642450997855` Perlin, `32.0` simplex) and the 12-cube-edge gradient table are the published FNL/OpenSimplex2 anchors. The MaterialX `noise3d` (`NodeCategory.Perlin3D`) category is the solid-noise wire target the `interchange#MATERIALX_DOCUMENT` resolves.
- [CELLULAR_FEATURE_FAMILY]: REALIZED — the `Worley` arm is the full FastNoiseLite cellular family rather than the single F1 distance: `CellularDistance` (`EuclideanSq`/`Euclidean`/`Manhattan`/`Hybrid`) is the metric the neighbourhood fold accumulates, `CellularReturn` (`CellValue`/`Distance`/`Distance2`/`Distance2Sub`/`Distance2Add`/`Distance2Mul`) the projected feature (F1, F2, the F2−F1 vein/crack, the per-cell hash), and `CellularParams.Jitter` the feature-point displacement (the FastNoiseLite `CellularJitterMod`), the three threaded through `CellularParams` on the `Noise` source. The classic Worley "crack" pattern is `Distance2Sub` over `Euclidean`, a cobblestone is `CellValue`, a Voronoi-cell border is `Distance` — all rows of one neighbourhood loop, never a per-return kernel. The two-axis `CellularDistance × CellularReturn` set is the FastNoiseLite cellular matrix verified against `auburn/fastnoiselite` `CellularDistanceFunction`/`CellularReturnType`.
- [FRACTAL_TRAJECTORY]: REALIZED — the fractal accumulation is the closed `FractalMode` axis (`FBm` the signed amplitude sum, `Ridged` the `1−|n|` folded sum for sharp ridgelines, `PingPong` the triangle-wave fold for marbled bands) over the basis octave-sum, with the FastNoiseLite `WeightedStrength` octave-damping (`amp *= gain·(1 − strength·(1 − |n|))`) so a row reads its fractal spectrum rather than the prior hardcoded linear `(field+1)·0.5`; the `MtlxCategory` projects a fractal noise (`Octaves > 1` over a gradient/value basis) to the MaterialX `fractal2d` node for every basis (not the prior Perlin-only special-case), `Worley` staying `worleynoise2d`. The `FractalType` (FBm/Ridged/PingPong) and `FractalWeightedStrength`/`FractalPingPongStrength` are the FastNoiseLite fractal anchors verified against `auburn/fastnoiselite`.
- [DOMAIN_WARP]: REALIZED — the `DomainWarp` field on the `Noise` source pre-displaces the sample coordinate by a Simplex-warp vector (`x + amp·simplex(x·freq, y·freq)`) when `Amplitude > 0`, so a procedural pattern flows and swirls rather than reading on an axis grid; `Amplitude = 0` (`DomainWarp.None`) is the identity. The warp is the FastNoiseLite `DomainWarp`/`DomainWarpAmp` capability verified against `auburn/fastnoiselite`, vendored as the one inline `Warp2D`/`Warp3D` displacement the `Evaluate` fold consumes — never a second sampler instance.
- [MIP_GENERATION]: the `Image` case carries a pre-built mip pyramid (`Levels`); the box-filter downsample that generates it is the consumer's responsibility at texture import, not a sampler concern — the sampler reconstructs from the supplied pyramid and rails `<texture-level-undersized>` on a malformed payload rather than synthesizing levels at sample time. The texel `W` lane is the coverage alpha the `Bilinear`/`Bicubic` reconstruction premultiplies and un-premultiplies so a transparent texel never bleeds opaque color across a coverage edge, and `Channel.Mask` reads it as the per-texel coverage a `graph#MATERIAL_GRAPH` `Mix` factor or a `weathering#WEATHERING` cavity mask consumes.
- [GRAPH_NODE_BRIDGE]: REALIZED — the `graph#MATERIAL_GRAPH` `AppearanceNode.Texture(Func<double,double,PortValue> Sample)` node binds the `TextureUv.Port` closure: `Port` mints the TOTAL `(u,v)→PortValue` closure from a `TextureSource` + `SamplerState` + `Channel` + `Op key`, the deep `TextureUv.Sample` rail carrying the `MaterialFault` and the closure folding a fault to the `Channel` neutral so the graph arm never propagates a sentinel. `Channel` (`Color`/`Scalar`/`Mask`/`Vector`) is the sample modality — a color texture drives `Color`, a roughness/AO mask `Scalar` or `Mask`, a normal map `Vector` — so one bridge serves every `Texture` node without a per-channel `Port` overload; the `weathering#WEATHERING` `ApplySlab` cavity-occlusion `UnitInterval` is the `Channel.Mask`/`Channel.Scalar` read of a cavity/AO `Texture` node. The remaining calibration is the per-input port-name alignment per MaterialX node, the `System.Xml.Linq` `.mtlx` serialize at the host boundary the `interchange#MATERIALX_DOCUMENT` owns, not a re-architecture of the sampling fold.
- [MATERIALX_NODE_PARITY]: REALIZED against the MaterialX 1.39 standard-node library — the `TextureSource.MtlxCategory`/`NoiseBasis.MtlxNode`/`AddressMode.MtlxAddress`/`FilterMode.MtlxFilter` projections each resolve to a closed `interchange#MATERIALX_DOCUMENT` `NodeCategory` row: `NoiseBasis` maps `Perlin→noise2d`/`Simplex→fractal2d`/`Value→cellnoise2d`/`Worley→worleynoise2d`, a fractal `Noise` source to `fractal2d`, `Checker→checkerboard`, `Gradient→ramplr`/`ramptb` by axis, `Image→tiledimage`, `Triplanar→triplanarprojection`, `AddressMode` to the `uaddressmode`/`vaddressmode` enum (`periodic`/`clamp`/`mirror`), and `FilterMode` to the `filtertype` enum (`closest`/`linear`/`cubic`, `Trilinear→linear` the mip blend being the sampler's pyramid concern). The leaf-basis quartet covers the gradient/simplex/value/cellular families against the standard library's `noise2d`/`noise3d`/`fractal2d`/`cellnoise2d`/`worleynoise2d`/`unifiednoise2d` — the `Value` basis the `cellnoise2d` value-noise analogue the prior three-basis set omitted, `unifiednoise2d` a parameterized selector neither basis maps cleanly onto. The FastNoiseLite OpenSimplex2 kernel stays the documented `LIBRARY_DEPTH` NOT_COVERED vendored carve-out the implementation differs by. Verified against the AcademySoftwareFoundation/MaterialX 1.39 `MaterialX.StandardNodes.md` and `auburn/fastnoiselite`.
