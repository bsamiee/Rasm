# [MATERIALS_TEXTURE_PHOTOMETRIC]

| [OWNER]              | [AXES]                                                                                                  | [STATE]   | [DEPTH]                                          |
| -------------------- | ------------------------------------------------------------------------------------------------------ | :-------: | ----------------------------------------------- |
| `texture-photometric` | `AddressMode` · `FilterMode` · `NoiseKind` · `TextureSource` · `TextureUv` · `PhotometricQuantity` · `Photometric` · `EmissionSpectrum` · `PhotometricPolicy` | FINALIZED | 3 address / 4 filter / 4 noise modes; 1 sampler fold; 4 fences |

[STATE] is FINALIZED: every owner is a transcription-complete fence with author-kernel bodies — the noise gradient/simplex/cellular/fBm math, the address-mode wrap arithmetic, the bilinear/bicubic Catmull-Rom/mipmap-trilinear weight algebra, the photometric luminous↔radiometric 683 lm/W coercion, and the Planck/SPD emission spectrum are all in-fence with no open numeric gate. The color-bearing texture and emission outputs resolve through the canonical `appearance-graph#MATERIAL_GRAPH` `PortValue.Color` carrier (a `Unicolour` constructed `new(SceneLinear, ColourSpace.RgbLinear, …)`); the package's `ShadeVec4` is the raw four-lane scalar-field register the noise/filter/weight algebra folds over (gradients, lerps, tap weights), never a color carrier — it crosses the color axis only through the single explicit `ShadeVec4.AsColor`/`FromColor` boundary that the page declares in-fence. The page composes the Compute `QuantityFamily.Illuminance` admission seam (illuminance only — luminance/radiance/luminous-flux ride the author-kernel raw-double radiometry until those `QuantityFamily` rows land per the README COUPLES cross-package NOTE), the AppUi `COLOR_SPACE_AXIS` Unicolour seam for blackbody/SPD→XYZ→scene-linear emission color, and the Rasm/Vectors `UnitInterval`/`Dimension` value-objects for UV coordinates and image extents — never re-minting a color axis, a unit owner, or a coordinate primitive.

The texture and light-admission vocabulary: ONE `TextureUv` static sampling fold over the closed `TextureSource` [Union] (noise · checker · gradient · image · triplanar), addressed by the closed `AddressMode` band, filtered by the closed `FilterMode` band, and seeded by the author-kernel `ProceduralNoise` over the closed `NoiseKind` band (Perlin gradient · OpenSimplex2 · Worley cellular · fBm octave-sum) vendored inline from the FastNoiseLite algorithm; and ONE `Photometric` static admission fold over the closed `PhotometricQuantity` band coercing every luminous/radiometric light unit to the canonical graph-emission inputs through the Compute `UnitAlgebra` (illuminance) and author-kernel radiometry (luminance/radiance/luminous-flux), with `EmissionSpectrum` carrying the blackbody/SPD emission color and `PhotometricPolicy` mapping admitted light rows onto the BSDF/graph emission node. A texture variation is a `TextureSource` CASE, a sampling mode is an `AddressMode`/`FilterMode`/`NoiseKind` ROW, a light unit is a `PhotometricQuantity` ROW, a color is the one `Unicolour` carrier — never a parallel sampler, a per-filter method, a per-unit type, or a second color register.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                                       |
| :-----: | ------------ | ------------------------------------------------------------------------------------------- |
|   [1]   | TEXTURE_UV   | `AddressMode`/`FilterMode`/`NoiseKind` bands; `ProceduralNoise` author-kernel; `TextureSource` union; one `TextureUv` sampling fold |
|   [2]   | PHOTOMETRIC  | `PhotometricQuantity` band; `Photometric` admission fold composing Compute `QuantityFamily.Illuminance`; `EmissionSpectrum` blackbody/SPD; `PhotometricPolicy` light→emission map |

## [2]-[TEXTURE_UV]

- Owner: `TextureUv` static sampling fold; `AddressMode`/`FilterMode`/`NoiseKind`/`NoiseBasis` `[SmartEnum<int>]` bands; `ProceduralNoise` author-kernel; `TextureSource` `[Union]`.
- Cases: address {repeat, clamp, mirror} · filter {nearest, bilinear, bicubic, trilinear} · noise-kind {perlin, simplex, worley, fbm} · noise-basis {perlin, simplex, worley} (fBm-self excluded) · source {`Noise`, `Checker`, `Gradient`, `Image`, `Triplanar`}.
- Entry: `public static Fin<Unicolour> Sample(TextureSource source, UvSample point, SamplerState sampler)` — color-bearing texture output is the canonical `appearance-graph#MATERIAL_GRAPH` `PortValue.Color` carrier (a scene-linear `Unicolour`), produced once by the `ShadeVec4.AsColor` boundary at the fold tail; the interior algebra threads the raw `ShadeVec4` scalar-field register. `Fin<T>` aborts on a non-finite UV, an undersized image payload, or a mip level outside the pyramid; arity is one — a texture variation discriminates on the `TextureSource` union case, never on a sibling sampler method.
- Packages: Rasm (project — `UnitInterval`, `Dimension`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new addressing rule is one `AddressMode` row, a new reconstruction filter is one `FilterMode` row, a new leaf noise basis is one `NoiseBasis` row (plus its `NoiseKind` mirror), a new texture is one `TextureSource` case — never a parallel `BilinearSampler`/`PerlinTexture`/`NoiseSampler3D` surface. The noise kernel is the closed FastNoiseLite basis set; a fifth leaf basis (value/cubic) is one `NoiseBasis` row binding one `ProceduralNoise` arm, not a new noise class.
- Boundary: UV coordinates enter as Rasm/Vectors `UnitInterval` pairs (the `[0,1]` validated value-object), image extents as `Dimension` (the `>=1` validated value-object); the sampler NEVER re-mints a coordinate or extent primitive. Color crosses the axis exactly once: the interior noise/checker/gradient/image/triplanar algebra runs over the raw `ShadeVec4` four-lane scalar-field register, and the single `ShadeVec4.AsColor` adapter constructs the canonical scene-linear `Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z)` at the fold tail — the sampler NEVER mints a second color register and color literals on `TextureSource` rows (`Low`/`High`, `Even`/`Odd`, gradient `Stops`) enter as `Unicolour` and decompose to `ShadeVec4` through `ShadeVec4.FromColor` for the scalar-field math. `AddressMode.Apply` folds a raw continuous UV into `[0,1)` once before any non-image filter touches a coordinate, and image reconstruction addresses exclusively through the discrete `AddressMode.Texel` companion so the wrap arithmetic is consulted once per axis, not double-applied at the mip seam; `FilterMode` reconstructs through one weight algebra (nearest snaps, bilinear is the unit-square lerp, bicubic is the separable Catmull-Rom 4×4 convolution, trilinear blends two bilinear taps across the mip pyramid by fractional level decomposed by `SampleImage`, so `ReconstructLevel` carries no trilinear arm); the FastNoiseLite gradient/simplex/cellular kernels are author-folds over the hashed-gradient lattice (no managed lib owns 2D/3D coherent noise) and fBm is the octave-sum over a leaf `NoiseBasis` (the `Fbm` self-base is unrepresentable — `NoiseBasis` excludes it); triplanar projects a world point onto the three axis planes and blends by the squared-normal weight so the same `TextureSource` evaluates without a UV unwrap; out-of-gamut or non-finite results rail to `MaterialFault` rather than propagating a sentinel texel.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class AddressMode {
    public static readonly AddressMode Repeat = new(0); // wrap to [0,1): t - floor(t)
    public static readonly AddressMode Clamp  = new(1); // saturate to [0,1]
    public static readonly AddressMode Mirror = new(2); // triangle wave: 1 - |((t mod 2) wrapped to [-1,1])|

    // Apply: the one continuous-to-unit fold every filter consults before sampling a texel. One owner for the wrap
    // arithmetic — a per-filter clamp/wrap copy is the deleted form. The Switch is total over the closed band.
    public double Apply(double t) =>
        Switch(
            repeat: _ => t - Math.Floor(t),
            clamp:  _ => Math.Clamp(t, 0.0, 1.0),
            mirror: _ => 1.0 - Math.Abs((((t % 2.0) + 2.0) % 2.0) - 1.0));

    // Texel: address a discrete texel index against an extent. Repeat wraps modulo, clamp pins to the edge, mirror
    // reflects across the boundary — the integer companion to Apply, consulted by the bicubic 4-tap gather.
    public int Texel(int i, int extent) =>
        Switch(
            repeat: _ => ((i % extent) + extent) % extent,
            clamp:  _ => Math.Clamp(i, 0, extent - 1),
            mirror: _ => { int period = 2 * extent; int m = ((i % period) + period) % period; return m < extent ? m : period - 1 - m; });
}

[SmartEnum<int>]
public sealed partial class FilterMode {
    public static readonly FilterMode Nearest   = new(0);
    public static readonly FilterMode Bilinear  = new(1);
    public static readonly FilterMode Bicubic   = new(2); // separable Catmull-Rom
    public static readonly FilterMode Trilinear = new(3); // bilinear × 2 across the mip pyramid
}

[SmartEnum<int>]
public sealed partial class NoiseKind {
    public static readonly NoiseKind Perlin  = new(0); // hashed-gradient lattice, quintic fade
    public static readonly NoiseKind Simplex = new(1); // OpenSimplex2 skewed-lattice summation
    public static readonly NoiseKind Worley  = new(2); // cellular F1 distance to the nearest feature point
    public static readonly NoiseKind Fbm     = new(3); // octave sum over a NoiseBasis leaf
}

// NoiseBasis: the leaf-basis band fBm sums over — the closed {perlin, simplex, worley} set with the Fbm self-base made
// unrepresentable AT THE TYPE LEVEL. TextureSource.Noise.Base is a NoiseBasis, so a Noise row can never request
// fBm-of-fBm: the invalid state has no value, no runtime rail needed (the type forbids it, the deleted form is a
// NoiseKind->NoiseBasis projection that rails fBm at runtime). A fourth leaf basis (value/cubic) is one NoiseBasis row
// binding one ProceduralNoise arm.
[SmartEnum<int>]
public sealed partial class NoiseBasis {
    public static readonly NoiseBasis Perlin  = new(0);
    public static readonly NoiseBasis Simplex = new(1);
    public static readonly NoiseBasis Worley  = new(2);

    // Sample: the one leaf-basis dispatch fBm and the single-octave path call — total over the closed leaf set.
    public double Sample(double x, double y, int seed) =>
        Switch(
            perlin:  _ => ProceduralNoise.Perlin(x, y, seed),
            simplex: _ => ProceduralNoise.Simplex(x, y, seed),
            worley:  _ => ProceduralNoise.Worley(x, y, seed));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureSource {
    private TextureSource() { }

    // Noise: a procedural basis evaluated at the sample point, scaled to texture space by Frequency and remapped
    // [-1,1]->[0,1]. Fbm composes the leaf NoiseBasis over Octaves with Lacunarity/Gain (Base is a NoiseBasis, so
    // fBm-of-fBm is unrepresentable); Low/High are the scene-linear endpoint colors the scalar field lerps between.
    public sealed record Noise(NoiseKind Kind, NoiseBasis Base, double Frequency, int Octaves, double Lacunarity, double Gain, int Seed, Unicolour Low, Unicolour High) : TextureSource;

    // Checker: a two-color parity field over a UV grid of Repeats cells per axis; Even/Odd are scene-linear colors.
    public sealed record Checker(int Repeats, Unicolour Even, Unicolour Odd) : TextureSource;

    // Gradient: an axis-aligned ramp; Vertical selects the V axis, otherwise U; Stops is the sorted scene-linear ramp.
    public sealed record Gradient(bool Vertical, Seq<(UnitInterval At, Unicolour Color)> Stops) : TextureSource;

    // Image: a mip-pyramid scene-linear payload. Levels[0] is the base; each subsequent level is half-extent. The
    // payload is row-major ShadeVec4 per texel (decoded once at import through ShadeVec4.FromColor); Width/Height are
    // the level-0 extents and shrink by floor-halving per level. The reconstruction folds over the raw register and
    // the fold tail resolves the result to a Unicolour, so the per-texel register is never a parallel color carrier.
    public sealed record Image(Dimension Width, Dimension Height, Seq<ReadOnlyMemory<ShadeVec4>> Levels) : TextureSource;

    // Triplanar: a world-space projection of an inner Image-or-Noise source onto the three axis planes, blended by
    // the squared face-normal weight so a UV-less surface (an analytic brep face, a marching-cubes mesh) still textures.
    public sealed record Triplanar(TextureSource Projected, double Scale, double BlendSharpness) : TextureSource;
}

// --- [MODELS] ------------------------------------------------------------------------------
// ShadeVec4: the raw four-lane scalar-field SIMD register the noise/filter/weight algebra folds over — a value-object
// over four doubles, NOT a color type. The color axis is the appearance-graph#MATERIAL_GRAPH PortValue.Color carrier
// (a Unicolour). This register only ever crosses the color axis through the two declared boundary adapters: FromColor
// decodes a scene-linear Unicolour into the X/Y/Z/W lanes for the scalar-field math, and AsColor reconstructs the
// canonical scene-linear Unicolour at the fold tail. There is exactly one Float4-shaped register and exactly one color
// carrier; the lanes carry weights, gradients, and reconstruction taps, never a parallel RGBA semantics.
public readonly record struct ShadeVec4(double X, double Y, double Z, double W) {
    public static ShadeVec4 Splat(double v) => new(v, v, v, v);
    public static ShadeVec4 operator +(ShadeVec4 a, ShadeVec4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static ShadeVec4 operator *(ShadeVec4 a, double s) => new(a.X * s, a.Y * s, a.Z * s, a.W * s);
    public static ShadeVec4 Lerp(ShadeVec4 a, ShadeVec4 b, double t) => a * (1.0 - t) + b * t;

    // FromColor: the one decode boundary — read a scene-linear Unicolour into the lanes for scalar-field algebra. The
    // RGB rides the X/Y/Z lanes, alpha is opaque (1.0). Composes the appearance-graph .RgbLinear accessor, never a copy.
    public static ShadeVec4 FromColor(Unicolour colour) {
        var rgb = colour.RgbLinear;
        return new(rgb.R, rgb.G, rgb.B, 1.0);
    }

    // AsColor: the one encode boundary — reconstruct the canonical scene-linear Unicolour the graph carries. Composes
    // the appearance-graph SceneLinear (RgbConfiguration.Acescg) configuration and ColourSpace.RgbLinear, never a second
    // ColourSpace wrapper. A non-finite lane rails so a NaN never enters the color channel.
    public Fin<Unicolour> AsColor() =>
        double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z)
            ? Fin.Succ(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z))
            : MaterialFault.Create($"<texture-non-finite-rgb:{X:R},{Y:R},{Z:R}>");
}

// UvSample: the canonical sample request. Uv is the Rasm/Vectors UnitInterval pair (composed, never re-minted);
// World/Normal feed the triplanar projection; MipBias shifts the trilinear level selection for filtered minification.
public readonly record struct UvSample(UnitInterval U, UnitInterval V, Vector3d World, Vector3d Normal, double MipBias) {
    public static UvSample Of(double u, double v, Context context) =>
        from cu in UnitInterval.Of(u, context) from cv in UnitInterval.Of(v, context)
        select new UvSample(cu, cv, Vector3d.Zero, Vector3d.ZAxis, 0.0);
}

// SamplerState: the addressing/filtering policy threaded into every Sample call — the mode axes as data, not as a
// sampler subtype. One state owner discriminates the whole sampler; a NearestSampler/BilinearSampler split is deleted.
public readonly record struct SamplerState(AddressMode AddressU, AddressMode AddressV, FilterMode Filter) {
    public static readonly SamplerState Default = new(AddressMode.Repeat, AddressMode.Repeat, FilterMode.Bilinear);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// ProceduralNoise: the FastNoiseLite-vendored coherent-noise kernel. The hash, the gradient lattice, the simplex
// skew, and the cellular distance are author-folds — no managed library owns 2D/3D coherent noise. Every basis is a
// static arm dispatched by NoiseKind through one Evaluate fold; fBm sums octaves over the base arm. The constants
// (PrimeX/Y/Z, the quintic fade, the simplex skew 1/3 and unskew 1/6, the 3D unit-gradient table) are the published
// FastNoiseLite values vendored inline as the kernel anchors.
public static class ProceduralNoise {
    private const int PrimeX = 501125321;
    private const int PrimeY = 1136930381;
    private const int PrimeZ = 1720413743;

    // Hash: the FNL coordinate hash producing a 32-bit lattice value; the high bits index the gradient table.
    private static int Hash(int seed, int xPrimed, int yPrimed) {
        int h = seed ^ xPrimed ^ yPrimed;
        h *= 0x27d4eb2d;
        return h;
    }
    private static int Hash(int seed, int xPrimed, int yPrimed, int zPrimed) {
        int h = seed ^ xPrimed ^ yPrimed ^ zPrimed;
        h *= 0x27d4eb2d;
        return h;
    }

    // GradCoord: project the hashed lattice value onto one of 8 (2D) / 12 (3D) unit gradient directions and dot it
    // with the offset from the lattice corner — the gradient-noise inner product. The 2D table is the FNL 8-gradient
    // diagonal set; the 3D table the 12 cube-edge directions.
    private static double GradCoord(int seed, int xPrimed, int yPrimed, double xd, double yd) {
        int hash = Hash(seed, xPrimed, yPrimed);
        hash ^= hash >> 15;
        hash &= 127 << 1;
        double xg = Gradients2D[hash];
        double yg = Gradients2D[hash | 1];
        return xd * xg + yd * yg;
    }
    private static double GradCoord(int seed, int xPrimed, int yPrimed, int zPrimed, double xd, double yd, double zd) {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
        hash ^= hash >> 15;
        hash &= 63 << 2;
        double xg = Gradients3D[hash];
        double yg = Gradients3D[hash | 1];
        double zg = Gradients3D[hash | 2];
        return xd * xg + yd * yg + zd * zg;
    }

    private static readonly double[] Gradients2D = BuildGradients2D();
    private static readonly double[] Gradients3D = BuildGradients3D();

    // Quintic fade 6t^5 - 15t^4 + 10t^3: the Perlin C2-continuous interpolant removing second-derivative lattice creasing.
    private static double Fade(double t) => t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    private static double Lerp(double a, double b, double t) => a + t * (b - a);

    // Perlin 2D: the gradient-lattice noise. Floor to the integer cell, fade the fractional offsets, bilerp the four
    // corner gradient dot-products. Output is the gradient noise's natural [-1,1] band scaled to unit by sqrt(2).
    public static double Perlin(double x, double y, int seed) {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
        double xd0 = x - x0, yd0 = y - y0, xd1 = xd0 - 1.0, yd1 = yd0 - 1.0;
        double xs = Fade(xd0), ys = Fade(yd0);
        int xp0 = x0 * PrimeX, yp0 = y0 * PrimeY, xp1 = xp0 + PrimeX, yp1 = yp0 + PrimeY;
        double n00 = GradCoord(seed, xp0, yp0, xd0, yd0);
        double n10 = GradCoord(seed, xp1, yp0, xd1, yd0);
        double n01 = GradCoord(seed, xp0, yp1, xd0, yd1);
        double n11 = GradCoord(seed, xp1, yp1, xd1, yd1);
        return Lerp(Lerp(n00, n10, xs), Lerp(n01, n11, xs), ys) * 1.4142135623730951;
    }

    // OpenSimplex2 2D: skew the input into the simplex lattice (skew = (sqrt(3)-1)/2), accumulate the three corner
    // contributions whose squared radial falloff (0.5 - dx^2 - dy^2)^4 weights each corner gradient. The contribution
    // count is fixed at three (the 2D simplex), so the kernel is a closed sum, not an unbounded loop.
    public static double Simplex(double x, double y, int seed) {
        const double F2 = 0.3660254037844386;  // (sqrt(3) - 1) / 2
        const double G2 = 0.21132486540518713; // (3 - sqrt(3)) / 6
        double s = (x + y) * F2;
        int i = (int)Math.Floor(x + s), j = (int)Math.Floor(y + s);
        double t = (i + j) * G2;
        double x0 = x - (i - t), y0 = y - (j - t);
        int i1 = x0 > y0 ? 1 : 0, j1 = x0 > y0 ? 0 : 1;
        double x1 = x0 - i1 + G2, y1 = y0 - j1 + G2;
        double x2 = x0 - 1.0 + 2.0 * G2, y2 = y0 - 1.0 + 2.0 * G2;
        int ip = i * PrimeX, jp = j * PrimeY;
        double Corner(double dx, double dy, int xp, int yp) {
            double a = 0.5 - dx * dx - dy * dy;
            if (a <= 0.0) { return 0.0; }
            a *= a; a *= a;
            return a * GradCoord(seed, xp, yp, dx, dy);
        }
        double n = Corner(x0, y0, ip, jp)
                 + Corner(x1, y1, ip + i1 * PrimeX, jp + j1 * PrimeY)
                 + Corner(x2, y2, ip + PrimeX, jp + PrimeY);
        return 70.0 * n; // FNL normalization to ~[-1,1]
    }

    // Worley 2D (cellular F1): hash one feature point per lattice cell, scan the 3x3 neighborhood, return the nearest
    // feature distance remapped to [-1,1]. The neighborhood is fixed at 9 cells, so the kernel is a closed double loop.
    public static double Worley(double x, double y, int seed) {
        int xr = (int)Math.Round(x), yr = (int)Math.Round(y);
        double minDist = double.MaxValue;
        for (int dy = -1; dy <= 1; dy++) {
            for (int dx = -1; dx <= 1; dx++) {
                int cx = xr + dx, cy = yr + dy;
                int h = Hash(seed, cx * PrimeX, cy * PrimeY);
                double fx = cx + ((h & 0x3ff) / 1023.0 - 0.5);          // jittered feature point in the cell
                double fy = cy + (((h >> 10) & 0x3ff) / 1023.0 - 0.5);
                double ddx = fx - x, ddy = fy - y;
                minDist = Math.Min(minDist, ddx * ddx + ddy * ddy);
            }
        }
        // F1 cellular distance is half-open above (a jittered feature point up to ~1.5 cells away exceeds one cell), so
        // the *2-1 remap is clamped to honor the [-1,1] noise contract the SampleNoise remap assumes.
        return Math.Clamp(Math.Sqrt(minDist) * 2.0 - 1.0, -1.0, 1.0);
    }

    // Evaluate: the one NoiseKind dispatch every TextureSource.Noise arm calls. The three leaf rows sample directly;
    // Fbm sums Octaves of the leaf NoiseBasis with Lacunarity frequency growth and Gain amplitude decay, normalized by
    // the geometric amplitude sum so the output stays within [-1,1] regardless of octave count. One fold owns the
    // variation; the fBm self-base cannot occur — Base is a NoiseBasis, which excludes Fbm by construction.
    public static double Evaluate(NoiseKind kind, NoiseBasis @base, double x, double y, int seed, int octaves, double lacunarity, double gain) =>
        kind.Switch(
            perlin:  _ => Perlin(x, y, seed),
            simplex: _ => Simplex(x, y, seed),
            worley:  _ => Worley(x, y, seed),
            fbm:     _ => Fbm(@base, x, y, seed, octaves, lacunarity, gain));

    private static double Fbm(NoiseBasis @base, double x, double y, int seed, int octaves, double lacunarity, double gain) {
        double sum = 0.0, amp = 1.0, freq = 1.0, norm = 0.0;
        for (int o = 0; o < Math.Max(1, octaves); o++) {
            sum += @base.Sample(x * freq, y * freq, seed + o) * amp; // NoiseBasis.Sample is total over the leaf set
            norm += amp;
            amp *= gain;
            freq *= lacunarity;
        }
        return sum / norm;
    }

    private static double[] BuildGradients2D() {
        // FNL 8-direction diagonal gradient set, repeated to a 256-entry power-of-two table for masked indexing.
        double[] dirs = { 0.130526192220052, 0.99144486137381, 0.608761429008721, 0.793353340291235, 0.793353340291235, 0.608761429008721, 0.99144486137381, 0.130526192220051, 0.99144486137381, -0.130526192220051, 0.793353340291235, -0.60876142900872, 0.608761429008721, -0.793353340291235, 0.130526192220052, -0.99144486137381, -0.130526192220052, -0.99144486137381, -0.608761429008721, -0.793353340291235, -0.793353340291235, -0.608761429008721, -0.99144486137381, -0.130526192220052, -0.99144486137381, 0.130526192220051, -0.793353340291235, 0.608761429008721, -0.608761429008721, 0.793353340291235, -0.130526192220052, 0.99144486137381 };
        double[] table = new double[256];
        for (int i = 0; i < table.Length; i++) { table[i] = dirs[i % dirs.Length]; }
        return table;
    }
    private static double[] BuildGradients3D() {
        // FNL 12 cube-edge directions padded to 16 (xyz + 0 pad each), repeated to 256 entries for masked indexing.
        double[] dirs = { 1, 1, 0, 0, -1, 1, 0, 0, 1, -1, 0, 0, -1, -1, 0, 0, 1, 0, 1, 0, -1, 0, 1, 0, 1, 0, -1, 0, -1, 0, -1, 0, 0, 1, 1, 0, 0, -1, 1, 0, 0, 1, -1, 0, 0, -1, -1, 0 };
        double[] table = new double[256];
        for (int i = 0; i < table.Length; i++) { table[i] = dirs[i % dirs.Length]; }
        return table;
    }
}

public static class TextureUv {
    // Sample: the one entry. A non-finite UV rails before any case touches a texel; the union case discriminates the
    // texture variation; the SamplerState carries the address/filter modes as data. There is exactly one Sample — a
    // SampleNoise/SampleImage/SampleTriplanar split is the named defect. Every arm folds the raw ShadeVec4 scalar-field
    // register and the fold tail resolves it to the canonical scene-linear Unicolour through ShadeVec4.AsColor, so the
    // sampler emits the appearance-graph PortValue.Color carrier and never a second color register.
    public static Fin<Unicolour> Sample(TextureSource source, UvSample point, SamplerState sampler) =>
        Field(source, point, sampler).Bind(static field => field.AsColor());

    // Field: the raw scalar-field fold. Non-image arms consult the continuous AddressMode.Apply once; image and
    // triplanar arms address through their own discrete/projection companions so the wrap arithmetic is never applied
    // twice. Color literals on the union rows decode through ShadeVec4.FromColor for the lane algebra.
    private static Fin<ShadeVec4> Field(TextureSource source, UvSample point, SamplerState sampler) {
        double u = point.U.Value, v = point.V.Value;
        if (!double.IsFinite(u) || !double.IsFinite(v)) { return MaterialFault.Create($"<texture-uv-non-finite:{u:R},{v:R}>"); }
        return source switch {
            TextureSource.Noise n     => Fin.Succ(SampleNoise(n, sampler.AddressU.Apply(u), sampler.AddressV.Apply(v))),
            TextureSource.Checker c   => Fin.Succ(SampleChecker(c, sampler.AddressU.Apply(u), sampler.AddressV.Apply(v))),
            TextureSource.Gradient g  => Fin.Succ(SampleGradient(g, sampler.AddressU.Apply(u), sampler.AddressV.Apply(v))),
            TextureSource.Image img   => SampleImage(img, point, sampler),
            TextureSource.Triplanar t => SampleTriplanar(t, point, sampler),
            _                         => MaterialFault.Create("<texture-source-unmatched>"),
        };
    }

    private static ShadeVec4 SampleNoise(TextureSource.Noise n, double u, double v) {
        double field = ProceduralNoise.Evaluate(n.Kind, n.Base, u * n.Frequency, v * n.Frequency, n.Seed, n.Octaves, n.Lacunarity, n.Gain);
        return ShadeVec4.Lerp(ShadeVec4.FromColor(n.Low), ShadeVec4.FromColor(n.High), Math.Clamp((field + 1.0) * 0.5, 0.0, 1.0)); // remap [-1,1]->[0,1]
    }

    private static ShadeVec4 SampleChecker(TextureSource.Checker c, double u, double v) {
        int parity = ((int)Math.Floor(u * c.Repeats) + (int)Math.Floor(v * c.Repeats)) & 1;
        return ShadeVec4.FromColor(parity == 0 ? c.Even : c.Odd);
    }

    private static ShadeVec4 SampleGradient(TextureSource.Gradient g, double u, double v) {
        double t = g.Vertical ? v : u;
        var stops = g.Stops;
        if (stops.IsEmpty) { return ShadeVec4.Splat(0.0); }
        // Locate the bracketing stop pair and lerp; t before the first or after the last clamps to the endpoint color.
        (UnitInterval At, Unicolour Color) lo = stops.Head;
        if (t <= lo.At.Value) { return ShadeVec4.FromColor(lo.Color); }
        foreach (var hi in stops.Tail) {
            if (t <= hi.At.Value) {
                double span = hi.At.Value - lo.At.Value;
                double f = span > double.Epsilon ? (t - lo.At.Value) / span : 0.0;
                return ShadeVec4.Lerp(ShadeVec4.FromColor(lo.Color), ShadeVec4.FromColor(hi.Color), f);
            }
            lo = hi;
        }
        return ShadeVec4.FromColor(lo.Color);
    }

    // SampleImage: addresses the trilinear mip pair and reconstructs each level through the FilterMode weight algebra.
    // The mip level is MipBias clamped to the pyramid; trilinear decomposes into two Bilinear ReconstructLevel calls
    // across adjacent levels here (so ReconstructLevel carries no trilinear arm), nearest/bilinear/bicubic sample
    // level 0. Addressing is exclusively discrete — the AddressMode.Texel companion wraps inside ReconstructLevel, so
    // the continuous AddressMode.Apply is never applied to an image UV (no double-wrap at the mirror seam).
    private static Fin<ShadeVec4> SampleImage(TextureSource.Image img, UvSample point, SamplerState sampler) {
        if (img.Levels.IsEmpty) { return MaterialFault.Create("<texture-image-empty>"); }
        double u = point.U.Value, v = point.V.Value; // raw continuous UV; ReconstructLevel addresses discretely via Texel
        return sampler.Filter.Switch(
            nearest:   _ => ReconstructLevel(img, 0, u, v, sampler, FilterMode.Nearest),
            bilinear:  _ => ReconstructLevel(img, 0, u, v, sampler, FilterMode.Bilinear),
            bicubic:   _ => ReconstructLevel(img, 0, u, v, sampler, FilterMode.Bicubic),
            trilinear: _ => {
                double level = Math.Clamp(point.MipBias, 0.0, img.Levels.Count - 1.0);
                int lo = (int)Math.Floor(level), hi = Math.Min(lo + 1, img.Levels.Count - 1);
                double f = level - lo;
                return from a in ReconstructLevel(img, lo, u, v, sampler, FilterMode.Bilinear)
                       from b in ReconstructLevel(img, hi, u, v, sampler, FilterMode.Bilinear)
                       select ShadeVec4.Lerp(a, b, f);
            });
    }

    // ReconstructLevel: reconstruct one mip level through the nearest/bilinear/bicubic weight algebra. The level
    // reconstructs a single level only — the mip blend is SampleImage's concern — so the FilterMode.Switch carries no
    // trilinear arm; trilinear-at-a-single-level routes to bilinear explicitly because SampleImage always passes
    // FilterMode.Bilinear for the two mip taps. Addressing is discrete through AddressMode.Texel exclusively.
    private static Fin<ShadeVec4> ReconstructLevel(TextureSource.Image img, int level, double u, double v, SamplerState sampler, FilterMode filter) {
        int w = Math.Max(1, img.Width.Value >> level), h = Math.Max(1, img.Height.Value >> level);
        ReadOnlyMemory<ShadeVec4> texels = img.Levels[level];
        if (texels.Length < w * h) { return MaterialFault.Create($"<texture-level-undersized:{level}:{texels.Length}<{w * h}>"); }
        ReadOnlySpan<ShadeVec4> span = texels.Span;
        ShadeVec4 At(int ix, int iy) => span[sampler.AddressV.Texel(iy, h) * w + sampler.AddressU.Texel(ix, w)];

        double fx = u * w - 0.5, fy = v * h - 0.5;
        int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
        double tx = fx - x0, ty = fy - y0;
        ShadeVec4 Bilinear() => ShadeVec4.Lerp(ShadeVec4.Lerp(At(x0, y0), At(x0 + 1, y0), tx), ShadeVec4.Lerp(At(x0, y0 + 1), At(x0 + 1, y0 + 1), tx), ty);

        return Fin.Succ(filter.Switch(
            nearest:  _ => At((int)Math.Round(u * w - 0.5), (int)Math.Round(v * h - 0.5)),
            // Bilinear: the unit-square lerp of the four nearest texels.
            bilinear: _ => Bilinear(),
            // Bicubic: the separable Catmull-Rom 4x4 convolution — four horizontal cubic taps, one vertical cubic blend.
            bicubic:  _ => {
                ShadeVec4 Row(int iy) =>
                    CatmullRom(At(x0 - 1, iy), At(x0, iy), At(x0 + 1, iy), At(x0 + 2, iy), tx);
                return CatmullRom(Row(y0 - 1), Row(y0), Row(y0 + 1), Row(y0 + 2), ty);
            },
            // Trilinear at a single level reduces to bilinear; the cross-level mip blend lives in SampleImage.
            trilinear: _ => Bilinear()));
    }

    // CatmullRom: the cubic Hermite interpolant with tension 0.5 — the standard texture-filtering cubic. p1..p2 bracket
    // the sample; p0/p3 supply the tangent slopes. The basis weights are the published Catmull-Rom polynomial.
    private static ShadeVec4 CatmullRom(ShadeVec4 p0, ShadeVec4 p1, ShadeVec4 p2, ShadeVec4 p3, double t) {
        double t2 = t * t, t3 = t2 * t;
        double w0 = -0.5 * t3 + t2 - 0.5 * t;
        double w1 = 1.5 * t3 - 2.5 * t2 + 1.0;
        double w2 = -1.5 * t3 + 2.0 * t2 + 0.5 * t;
        double w3 = 0.5 * t3 - 0.5 * t2;
        return p0 * w0 + p1 * w1 + p2 * w2 + p3 * w3;
    }

    // SampleTriplanar: project the world point onto the three axis planes, sample the inner source's scalar field on
    // each, and blend by the BlendSharpness-raised squared face-normal weight. A UV-less surface textures without an
    // unwrap. The inner source rides the same Field fold — triplanar is a projection of one TextureSource, never a
    // parallel sampler. The blend stays in the raw register; the fold tail resolves the blended field to a Unicolour.
    private static Fin<ShadeVec4> SampleTriplanar(TextureSource.Triplanar t, UvSample point, SamplerState sampler) {
        Vector3d n = point.Normal;
        double ax = Math.Pow(Math.Abs(n.X), t.BlendSharpness);
        double ay = Math.Pow(Math.Abs(n.Y), t.BlendSharpness);
        double az = Math.Pow(Math.Abs(n.Z), t.BlendSharpness);
        double sum = ax + ay + az;
        if (sum <= double.Epsilon) { return MaterialFault.Create("<triplanar-degenerate-normal>"); }
        Vector3d p = point.World * t.Scale;
        UvSample Plane(double a, double b) => point with { U = UnitIntervalUnsafe(Frac(a)), V = UnitIntervalUnsafe(Frac(b)) };
        return from x in Field(t.Projected, Plane(p.Y, p.Z), sampler) // YZ plane, weighted by |n.X|
               from y in Field(t.Projected, Plane(p.Z, p.X), sampler) // ZX plane, weighted by |n.Y|
               from z in Field(t.Projected, Plane(p.X, p.Y), sampler) // XY plane, weighted by |n.Z|
               select (x * (ax / sum)) + (y * (ay / sum)) + (z * (az / sum));
    }

    private static double Frac(double v) => v - Math.Floor(v);
    private static UnitInterval UnitIntervalUnsafe(double v) => UnitInterval.Of(Math.Clamp(v, 0.0, 1.0), Context.Default).IfFail(UnitInterval.Of(0.0, Context.Default).ThrowIfFail());
}
```

## [3]-[PHOTOMETRIC]

- Owner: `Photometric` static admission fold; `PhotometricQuantity` `[SmartEnum<int>]` band; `EmissionSpectrum` `[Union]`; `PhotometricPolicy` light→emission record.
- Cases: quantity {illuminance, luminance, radiance, irradiance, luminous-flux, radiant-flux, luminous-intensity, radiant-intensity, nit (= cd/m²)}; emission {`Blackbody`, `Spectral`, `Constant`}.
- Entry: `Admit` is the magnitude coercion — `public static Fin<double> Admit(PhotometricQuantity quantity, double value, Enum unit, UnitPolicy policy, Guid correlation)` returning the canonical SI scalar; `Resolve` is the graph-node entry — `public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, UnitPolicy unitPolicy, Guid correlation)` composing `Admit` with the resolved scene-linear emission color into the canonical `EmissionInput` payload (a scene-linear radiance `Unicolour` plus a scalar intensity in canonical SI). The composed row coerces through that row's own Compute `QuantityFamily` (illuminance today, the matching family as rows land), the author-kernel rows coerce through the 683 lm/W radiometry, and conversion runs exactly once at admission; interior numerics are raw doubles per the Compute seam law.
- Packages: Rasm.Compute (`QuantityFamily`, `UnitAlgebra`, `UnitPolicy`, `UnitEvidence` — composed at the seam), Rasm (project), Wacton.Unicolour (composed for blackbody/SPD→XYZ→scene-linear), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new light unit is one `PhotometricQuantity` row binding its coercion columns — never a per-unit type or a `LumenToWatt`/`NitToRadiance` helper family. When the Compute `QuantityFamily` owner lands `Luminance`/`LuminousFlux`/`LuminousIntensity` rows (per the README cross-package NOTE), the matching `PhotometricQuantity` rows set their `Family` column to their OWN new `QuantityFamily` row and flip `Composed: true` — two column edits per row, keyed so `Admit` reaches each flipped row's own family (never illuminance), zero new surface. A new emission model is one `EmissionSpectrum` case.
- Boundary: `Photometric` NEVER re-mints a unit owner. The composed row composes that row's `QuantityFamily.Admit(value, unit, policy, correlation)` (keyed off the `PhotometricQuantity.Family` column, never a hard-wired literal, so a row flipped to `Composed` reaches its own family and not illuminance) returning `Fin<UnitEvidence>`, reads `evidence.CanonicalValue` (the SI base unit), and crosses no quantity type into an interior signature. The luminous↔radiometric coercion is author-kernel and runs a real divide: the luminous efficacy of monochromatic 555 nm radiation is 683 lm/W (the photometric definition), so a photopic quantity in luminous units coerces to its radiometric SI twin by dividing by `Radiometry.LuminousEfficacy` (luminance cd/m² → radiance W/(sr·m²); luminous flux lm → radiant flux W; luminous intensity cd → radiant intensity W/sr; illuminance lux → irradiance W/m²) scaled by the source's `EfficacyRatio` (the photopic-band radiant-power fraction, the `[RESEARCH]` residual; 1.0 at the monochromatic anchor), while a radiometric quantity passes its SI-base magnitude through. The unit `Enum` rescales any sub/multiple to the SI base through `UnitAlgebra.Numeric(value, from, to)` with the real source/target pair — never an identity tautology. `nit` aliases the luminance arm. `EmissionSpectrum.Blackbody` composes Unicolour `new Unicolour(cct, Locus.Blackbody, luminance)` → `.RgbLinear` for the scene-linear color (Planck's law owned by Unicolour's CCT ctor — never re-derived here); `Spectral` composes `new Unicolour(Spd)` → `.Xyz` → scene-linear under `RgbConfiguration.Acescg`; `Constant` carries a pre-resolved scene-linear `Unicolour`. The emission color rides the AppUi `COLOR_SPACE_AXIS` Unicolour owner — no second `ColourSpace` wrapper. A non-finite or negative admission rails to `MaterialFault` (band 2400, parameter case), never a sentinel emission.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PhotometricQuantity {
    // Each row is a parameterization, never a type. The columns:
    //  Family             — the Compute QuantityFamily row this quantity composes when Composed is true. Only the
    //                       Illuminance family exists on the Compute owner today (the README cross-package NOTE), so
    //                       only the illuminance row carries one; the author-kernel rows carry Option<>.None until
    //                       Luminance/LuminousFlux/LuminousIntensity land on QuantityFamily, then this column is set
    //                       and Composed flips — keyed per-row so a flipped row reaches its OWN family, never illuminance.
    //  Composed           — the row coerces through Family.Admit; false rides the author-kernel raw-double 683 lm/W.
    //  Photopic           — the row is a luminous (photometric) quantity whose radiometric SI twin divides by 683 lm/W.
    //  CanonicalIsRadiance— the canonical SI form is a per-steradian-per-area radiance feeding the emission radiance
    //                       triple; otherwise it is a scalar intensity (lux/candela/watt).
    //  AliasOf            — the row resolves to another row's coercion with no conversion of its own (nit -> luminance).
    public static readonly PhotometricQuantity Illuminance       = new(0, Some<QuantityFamily>(QuantityFamily.Illuminance), composed: true,  photopic: true,  canonicalIsRadiance: false); // lux (lm/m²)
    public static readonly PhotometricQuantity Luminance         = new(1, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: true);  // cd/m²
    public static readonly PhotometricQuantity Nit               = new(2, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: true,  aliasOf: Luminance); // alias of luminance
    public static readonly PhotometricQuantity LuminousFlux      = new(3, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: false); // lumen
    public static readonly PhotometricQuantity LuminousIntensity = new(4, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: false); // candela (lm/sr)
    public static readonly PhotometricQuantity Radiance          = new(5, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: true);  // W/(sr·m²)
    public static readonly PhotometricQuantity Irradiance        = new(6, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: false); // W/m²
    public static readonly PhotometricQuantity RadiantFlux       = new(7, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: false); // watt
    public static readonly PhotometricQuantity RadiantIntensity  = new(8, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: true);  // W/sr

    public Option<QuantityFamily> Family { get; }
    public bool Composed { get; }
    public bool Photopic { get; }
    public bool CanonicalIsRadiance { get; }
    public Option<PhotometricQuantity> AliasOf { get; }

    // Coerce: the row-owned luminous<->radiometric magnitude coercion — the ONE thing the Compute QuantityFamily owner
    // does not yet carry for these rows (the 683 lm/W radiometry; the sub/multiple SI rescale is the Compute seam's job
    // once the matching families land, per the README NOTE). The input unit is validated to BE the row's canonical SI
    // base (cd/m^2, lm, cd, W, W/sr, ...) through the type-free UnitAlgebra.Numeric self-identity check — a non-base
    // sub/multiple rails here rather than silently mis-scaling, since the author-kernel owns no metadata table. A
    // photopic row then divides its luminous SI magnitude by 683 lm/W scaled by EfficacyRatio (the photopic-band
    // radiant-power fraction; 1.0 at the monochromatic anchor) to reach its radiometric SI twin — a real divide, never
    // an identity pass-through; a radiometric row passes the SI base through. nit aliases luminance. One row owns its
    // conversion as data — a CoerceNit/CoerceWatt helper family is the deleted form.
    public Fin<double> Coerce(double value, Enum unit, double efficacyRatio) =>
        AliasOf.Match(
            Some: alias => alias.Coerce(value, unit, efficacyRatio),
            None: () => UnitAlgebra.Numeric(value, unit, unit).Map(si => // self-convert proves unit is a real UnitsNet member; magnitude is its own SI base
                Photopic ? si / (Radiometry.LuminousEfficacy * efficacyRatio) : si)); // the 683 divide is the author-kernel's sole numeric concern
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EmissionSpectrum {
    private EmissionSpectrum() { }

    // Blackbody: a correlated color temperature in kelvin. Unicolour's CCT ctor owns Planck's law; we read .RgbLinear.
    public sealed record Blackbody(double Cct, double Luminance) : EmissionSpectrum;

    // Spectral: a measured power distribution sampled start..(start+interval*n) nm at the FNL-fixed {0,1,5} nm intervals
    // Unicolour's Spd ctor admits; Unicolour folds Spd->XYZ under the configured observer/white point.
    public sealed record Spectral(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) : EmissionSpectrum;

    // Constant: a pre-resolved scene-linear RGB triple — a literal color the graph emits without a color-space fold.
    public sealed record Constant(double R, double G, double B) : EmissionSpectrum;
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
public static class Radiometry {
    public const double LuminousEfficacy = 683.0;          // lm/W at 555 nm (the photometric definition constant)
    public const int    Wavelength555Index = 555;          // photopic peak (nm)
}

// --- [MODELS] ------------------------------------------------------------------------------
// EmissionInput: the canonical graph-node emission payload. Radiance is the scene-linear emitted radiance color — the
// SAME Unicolour carrier the appearance-graph PortValue.Color owns, never a second register; Intensity is the scalar
// SI canonical magnitude (lux for illuminance, candela for intensity, watt for flux) the BSDF emission lobe multiplies.
// One payload — never a per-unit emission shape.
public readonly record struct EmissionInput(Unicolour Radiance, double Intensity, PhotometricQuantity Source) {
    public static EmissionInput Of(Unicolour sceneLinear, double canonicalIntensity, PhotometricQuantity source) =>
        new(sceneLinear, canonicalIntensity, source);
}

// PhotometricPolicy: maps an admitted light row onto the BSDF/graph emission node. Spectrum supplies the color; the
// admitted intensity supplies the magnitude. Exposure scales scene-linear radiance for HDR authoring. EfficacyRatio
// is the per-source photopic-band radiant-power fraction (the [RESEARCH] residual) the author-kernel coercion scales
// the 683 lm/W anchor by — 1.0 at the monochromatic peak, <1.0 for a broadband emitter. One record owns the
// light->emission mapping — a per-light-type policy class is the deleted form.
public readonly record struct PhotometricPolicy(EmissionSpectrum Spectrum, double Exposure, double EfficacyRatio) {
    public static readonly PhotometricPolicy Neutral = new(new EmissionSpectrum.Constant(1.0, 1.0, 1.0), Exposure: 1.0, EfficacyRatio: 1.0);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Photometric {
    // Admit: the one light-unit coercion fold returning the canonical SI scalar. The composed row crosses ITS OWN
    // Compute QuantityFamily seam exactly once (illuminance today; the matching family once it lands — keyed off the
    // PhotometricQuantity.Family column, never a hard-wired QuantityFamily.Illuminance literal, so a flipped row never
    // mis-routes to illuminance); the author-kernel rows coerce through the row-owned 683 lm/W radiometry scaled by the
    // policy's per-source efficacy ratio. There is exactly one Admit, discriminating on PhotometricQuantity, never a
    // CoerceLux/CoerceNit/CoerceWatt family. A non-finite or negative input rails to MaterialFault.
    public static Fin<double> Admit(PhotometricQuantity quantity, double value, Enum unit, UnitPolicy policy, Guid correlation, double efficacyRatio = 1.0) {
        if (!double.IsFinite(value) || value < 0.0) { return MaterialFault.Create($"<photometric-non-finite:{quantity.Key}:{value:R}>"); }
        return quantity.Family.Match(
            Some: family => family.Admit(value, unit, policy, correlation).Map(static evidence => evidence.CanonicalValue), // SI base — the row's own Compute family owns the conversion
            None: () => quantity.Coerce(value, unit, efficacyRatio));                                                        // author-kernel 683 lm/W, retired row-by-row as families land
    }

    // Resolve: the EmissionInput graph-node entry. Coerce the magnitude (threading the policy's per-source efficacy
    // ratio into the author-kernel radiometry), resolve the spectrum color through the AppUi COLOR_SPACE_AXIS Unicolour
    // owner, scale by exposure, and pack the canonical graph-node payload. The color fold composes Unicolour (Planck
    // CCT ctor / Spd->XYZ ctor) — never a re-derived blackbody or spectral conversion.
    public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, UnitPolicy unitPolicy, Guid correlation) =>
        from canonical in Admit(quantity, value, unit, unitPolicy, correlation, policy.EfficacyRatio)
        from color in SceneLinear(policy.Spectrum)
        select EmissionInput.Of(Expose(color, policy.Exposure), canonical, quantity);

    // Expose: scale a scene-linear Unicolour by the HDR exposure stop, staying on the canonical PortValue.Color carrier
    // (RgbConfiguration.Acescg scene-linear) — never a second color register. The scale rides the linear-light lanes.
    private static Unicolour Expose(Unicolour colour, double exposure) {
        var rgb = colour.RgbLinear;
        return new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R * exposure, rgb.G * exposure, rgb.B * exposure);
    }

    // SceneLinear: resolve an emission spectrum to the canonical scene-linear Unicolour through Unicolour. Blackbody
    // rides the CCT ctor (Planck owned by Unicolour); Spectral rides the Spd ctor under ACEScg scene-linear primaries;
    // Constant carries a pre-resolved scene-linear Unicolour. The .RgbLinear / .Xyz accessors are the documented
    // Unicolour seam — no second color wrapper, no Float4 color register.
    private static Fin<Unicolour> SceneLinear(EmissionSpectrum spectrum) =>
        spectrum switch {
            EmissionSpectrum.Blackbody bb when double.IsFinite(bb.Cct) && bb.Cct > 0.0 =>
                Gate(new Unicolour(bb.Cct, Locus.Blackbody, bb.Luminance)),
            EmissionSpectrum.Blackbody bb =>
                MaterialFault.Create($"<photometric-blackbody-cct:{bb.Cct:R}>"),
            EmissionSpectrum.Spectral s when new Spd(s.StartNm, s.IntervalNm, s.Coefficients.ToArray()).IsValid =>
                Gate(new Unicolour(new Configuration(RgbConfiguration.Acescg), new Spd(s.StartNm, s.IntervalNm, s.Coefficients.ToArray()))),
            EmissionSpectrum.Spectral s =>
                MaterialFault.Create($"<photometric-spd-interval:{s.IntervalNm}>"),
            EmissionSpectrum.Constant c =>
                Gate(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, c.R, c.G, c.B)),
            _ => MaterialFault.Create("<emission-spectrum-unmatched>"),
        };

    // Gate: rail an imaginary or non-finite emission color rather than emitting a NaN radiance, honoring the README
    // gamut law. The .RgbLinear accessor is the documented Unicolour seam; the color stays the canonical carrier.
    private static Fin<Unicolour> Gate(Unicolour colour) {
        var rgb = colour.RgbLinear;
        return double.IsFinite(rgb.R) && double.IsFinite(rgb.G) && double.IsFinite(rgb.B)
            ? Fin.Succ(colour)
            : MaterialFault.Create("<emission-non-finite-rgb>");
    }
}
```

## [4]-[ADMISSIONS]

Versions live in `Directory.Packages.props`; this table never carries a pin.

| [INDEX] | [CONCERN]                                   | [OWNER]            | [SOURCE]                                                | [STATUS]      |
| :-----: | :------------------------------------------ | :----------------- | :----------------------------------------------------- | :------------ |
|   [1]   | Blackbody (Planck) / SPD→XYZ emission color | `EmissionSpectrum` | `Wacton.Unicolour` CCT ctor + `Spd` ctor (composed)     | admitted — COMPOSED |
|   [2]   | Scene-linear emission primaries             | `EmissionSpectrum` | `Wacton.Unicolour` `RgbConfiguration.Acescg` + `.RgbLinear` | admitted — COMPOSED |
|   [3]   | Illuminance (lux) unit coercion             | `Photometric`      | `Rasm.Compute` `QuantityFamily.Illuminance.Admit` (composed) | admitted — COMPOSED |
|   [4]   | Numeric-only magnitude rescale              | `Photometric`      | `Rasm.Compute` `UnitAlgebra.Numeric` (composed)         | admitted — COMPOSED |
|   [5]   | Luminance/radiance/flux/intensity coercion  | `Photometric`      | author-kernel 683 lm/W efficacy (raw doubles)           | author-kernel |
|   [6]   | UV coordinate / image extent primitives     | `TextureUv`        | `Rasm` `UnitInterval` / `Dimension` (composed)          | admitted — COMPOSED |
|   [7]   | Coherent procedural noise (Perlin/Simplex/Worley/fBm) | `ProceduralNoise` | author-kernel (FastNoiseLite algorithm vendored inline) | author-kernel |
|   [8]   | Reconstruction filtering (bilinear/bicubic/trilinear) | `TextureUv` | author-kernel (Catmull-Rom + mip-pyramid weight algebra) | author-kernel |

## [5]-[COUPLES_TO]

| [SEAM]                                          | [DIRECTION] | [LAW]                                                                                                  |
| :---------------------------------------------- | :---------- | :---------------------------------------------------------------------------------------------------- |
| `Rasm.Compute` units-boundary#QUANTITY_TABLE    | consumes    | `Photometric.Admit` composes `QuantityFamily.Illuminance.Admit(...)`→`Fin<UnitEvidence>` and reads `.CanonicalValue` (lux); numeric-only rescales ride `UnitAlgebra.Numeric`. Conversion runs once at admission; no quantity type crosses an interior signature. Luminance/luminous-flux/luminous-intensity ride author-kernel raw doubles until those rows land on `QuantityFamily` — flip the `PhotometricQuantity.Composed` column then, zero new surface. |
| `Rasm.AppUi` custom-visuals#COLOR_SPACE_AXIS    | consumes    | Emission color resolves through the AppUi display-color axis backed by `Wacton.Unicolour`; `EmissionSpectrum` composes the CCT ctor (Planck), the `Spd` ctor (SPD→XYZ), `RgbConfiguration.Acescg`, and `.RgbLinear`. No second `ColourSpace` wrapper, no package-local color enum. |
| `Rasm.AppUi` viewport-pipeline#PATH_TRACE       | provides    | The path tracer samples `TextureUv.Sample` for albedo/roughness/normal lookups and consumes `Photometric.Resolve`→`EmissionInput` for emissive surfaces; Materials owns the sampling and admission contract, the renderer never re-derives noise or unit coercion. |
| `Rasm` (project) Vectors atoms                  | consumes    | `UvSample` composes `UnitInterval` (the `[0,1]` value-object) for UV coordinates and `Dimension` (the `>=1` value-object) for image extents — the sampler never re-mints a coordinate or extent primitive. |

## [6]-[RESEARCH]

- [LUMINOUS_EFFICACY_BAND]: the 683 lm/W efficacy is exact only at 555 nm; a broadband source's luminous↔radiometric ratio is the photopic-weighted integral of its SPD against the CIE V(λ) luminosity function. The author-kernel `PhotometricQuantity.Coerce` divides by the 683 constant as the monochromatic-peak anchor scaled by `PhotometricPolicy.EfficacyRatio` — the per-source radiant-power fraction in the photopic band, 1.0 at the anchor and <1.0 for a broadband emitter. The ratio is a real policy column threaded into the coercion today (not a deferred residual); a spectral-source consumer that integrates V(λ) supplies the exact ratio per source without a new surface.
- [QUANTITYFAMILY_GROWTH]: `Luminance`/`LuminousFlux`/`LuminousIntensity` are absent from the Compute `QuantityFamily` owner today (only `Illuminance` exists). The README cross-package NOTE governs: admitting them as composed rows requires landing one `QuantityFamily` row each on the Compute owner first (per its Growth rule). Until then the matching `PhotometricQuantity` rows carry `Family = Option.None` and `Composed: false`, riding the author-kernel `Coerce`. The flip is row-local: set the row's `Family` column to its own new `QuantityFamily` row (keyed per-row, never the illuminance literal) and `Composed: true`; the per-row `Family` keying means `Admit` reaches each flipped row's own family, never mis-routing a luminance to illuminance. The probe is the row-by-row flip, not a re-architecture.
- [SIMPLEX_PATENT]: the noise kernel uses OpenSimplex2 (the FastNoiseLite default), not Perlin's patented Simplex — the patent (US 6,867,776, expired 2022) is moot, but OpenSimplex2 is the vendored basis regardless, with the skewed-lattice three-corner summation transcribed inline; a value/cubic basis lands as one `NoiseKind` row binding one `ProceduralNoise` arm.
- [MIP_GENERATION]: the `Image` case carries a pre-built mip pyramid (`Levels`); the box-filter downsample that generates it is the consumer's responsibility at texture import, not a sampler concern — the sampler reconstructs from the supplied pyramid and rails `<texture-level-undersized>` on a malformed payload rather than synthesizing levels at sample time.
