# [MATERIALS_TEXTURE]

ONE `TextureUv` static sampling fold over the closed `TextureSource` `[Union]` (noise · checker · gradient · image · triplanar), addressed by the closed `AddressMode` band, filtered by the closed `FilterMode` band, and seeded by the author-kernel `ProceduralNoise` over the closed `NoiseBasis` band (Perlin gradient · OpenSimplex2 · Worley cellular) vendored inline from the FastNoiseLite algorithm, with fBm expressed as octave-summation of a basis (`Octaves > 1`) rather than a fourth basis. A texture variation is a `TextureSource` CASE, a sampling mode is an `AddressMode`/`FilterMode`/`NoiseBasis` ROW, a color is the one `Unicolour` carrier — never a parallel sampler, a per-filter method, a parallel fractal-vs-basis enum, or a second color register. The page consumes Wacton.Unicolour directly as the scene-linear color owner for every color output and composes the Rasm/Vectors `UnitInterval`/`Dimension` value-objects for UV coordinates and image extents — never re-minting a color space or a coordinate primitive. The color-bearing texture output resolves through the canonical `graph#MATERIAL_GRAPH` `PortValue.Color` carrier; the interior algebra threads the raw `ShadeVec4` four-lane scalar-field register, crossing the color axis only through the single explicit `ShadeVec4.AsColor`/`FromColor` boundary.

## [1]-[INDEX]

One cluster: `[2]-[TEXTURE_UV]` owns the `AddressMode`/`FilterMode`/`NoiseBasis` bands, the `ProceduralNoise` author-kernel, the `TextureSource` union, the one `TextureUv` sampling fold, and the `ShadeVec4` register.

## [2]-[TEXTURE_UV]

- Owner: `TextureUv` static sampling fold; `AddressMode`/`FilterMode`/`NoiseBasis` `[SmartEnum<int>]` bands; `ProceduralNoise` author-kernel; `TextureSource` `[Union]`.
- Cases: address {repeat, clamp, mirror} · filter {nearest, bilinear, bicubic, trilinear} · noise-basis {perlin, simplex, worley} (fBm is octave-summation over a basis, `Octaves > 1`, not a fourth basis) · source {`Noise`, `Checker`, `Gradient`, `Image`, `Triplanar`}.
- Entry: `public static Fin<Unicolour> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key)` — color-bearing texture output is the canonical `graph#MATERIAL_GRAPH` `PortValue.Color` carrier (a scene-linear `Unicolour`), produced once by the `ShadeVec4.AsColor` boundary at the fold tail; the interior algebra threads the raw `ShadeVec4` scalar-field register. `Fin<T>` aborts on a non-finite UV, an undersized image payload, or a mip level outside the pyramid through the `Op key`-correlated `MaterialFault` rail; arity is one — a texture variation discriminates on the `TextureSource` union case, never on a sibling sampler method.
- Packages: Rasm (project — `UnitInterval`, `Dimension`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new addressing rule is one `AddressMode` row, a new reconstruction filter is one `FilterMode` row, a new leaf noise basis is one `NoiseBasis` row binding one `ProceduralNoise.Sample` arm, a new texture is one `TextureSource` case — never a parallel `BilinearSampler`/`PerlinTexture`/`NoiseSampler3D` surface, and never a parallel fractal-kind enum since fBm rides the basis octave-sum. The noise kernel is the closed FastNoiseLite basis set; a fifth leaf basis (value/cubic) is one `NoiseBasis` row binding one `ProceduralNoise` arm, not a new noise class. The MaterialX-1.39 Worley/color-ramp node parity is the interchange-alignment target framed at `graph#MATERIALX_GRAPH_INTERCHANGE`.
- Boundary: UV coordinates enter as Rasm/Vectors `UnitInterval` pairs (the `[0,1]` validated value-object), image extents as `Dimension` (the `>=1` validated value-object); the sampler NEVER re-mints a coordinate or extent primitive. Color crosses the axis exactly once: the interior noise/checker/gradient/image/triplanar algebra runs over the raw `ShadeVec4` four-lane scalar-field register, and the single `ShadeVec4.AsColor` adapter constructs the canonical scene-linear `Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z)` at the fold tail — the sampler NEVER mints a second color register and color literals on `TextureSource` rows (`Low`/`High`, `Even`/`Odd`, gradient `Stops`) enter as `Unicolour` and decompose to `ShadeVec4` through `ShadeVec4.FromColor` for the scalar-field math. `AddressMode.Apply` folds a raw continuous UV into `[0,1)` once before any non-image filter touches a coordinate, and image reconstruction addresses exclusively through the discrete `AddressMode.Texel` companion so the wrap arithmetic is consulted once per axis, not double-applied at the mip seam; `FilterMode` reconstructs through one weight algebra (nearest snaps, bilinear is the unit-square lerp, bicubic is the separable Catmull-Rom 4×4 convolution, trilinear blends two bilinear taps across the mip pyramid by fractional level decomposed by `SampleImage`, so `ReconstructLevel` carries no trilinear arm); the FastNoiseLite gradient/simplex/cellular kernels are author-folds over the hashed-gradient lattice (no managed lib owns 2D/3D coherent noise, the `LIBRARY_DEPTH` NOT_COVERED carve-out) with the published FNL anchors — `PrimeX`/`PrimeY`/`PrimeZ`, the quintic fade `6t⁵−15t⁴+10t³`, the simplex skew `(√3−1)/2` and unskew `(3−√3)/6`, the 8-direction 2D and 12-cube-edge 3D unit-gradient tables — vendored inline as kernel literals, and fBm is the octave-sum over a leaf `NoiseBasis` (the `Fbm` self-base is unrepresentable — `NoiseBasis` excludes it); the `ProceduralNoise` hash-lattice fills and the fixed `3×3` Worley / closed three-corner simplex loops are the page's `[EXPRESSION_SPINE]` kernel exemption, in-place by index over the per-shade hot path; triplanar projects a world point onto the three axis planes and blends by the squared-normal weight so the same `TextureSource` evaluates without a UV unwrap; out-of-gamut or non-finite results rail to `MaterialFault` rather than propagating a sentinel texel.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class AddressMode {
    public static readonly AddressMode Repeat = new(0);
    public static readonly AddressMode Clamp  = new(1);
    public static readonly AddressMode Mirror = new(2);

    public double Apply(double t) =>
        Switch(
            repeat: _ => t - Math.Floor(t),
            clamp:  _ => Math.Clamp(t, 0.0, 1.0),
            mirror: _ => 1.0 - Math.Abs((((t % 2.0) + 2.0) % 2.0) - 1.0));

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
    public static readonly FilterMode Bicubic   = new(2);
    public static readonly FilterMode Trilinear = new(3);
}

[SmartEnum<int>]
public sealed partial class NoiseBasis {
    public static readonly NoiseBasis Perlin  = new(0);
    public static readonly NoiseBasis Simplex = new(1);
    public static readonly NoiseBasis Worley  = new(2);

    public double Sample(double x, double y, int seed) =>
        Switch(
            perlin:  _ => ProceduralNoise.Perlin(x, y, seed),
            simplex: _ => ProceduralNoise.Simplex(x, y, seed),
            worley:  _ => ProceduralNoise.Worley(x, y, seed));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureSource {
    private TextureSource() { }

    public sealed record Noise(NoiseBasis Base, double Frequency, int Octaves, double Lacunarity, double Gain, int Seed, Unicolour Low, Unicolour High) : TextureSource;
    public sealed record Checker(int Repeats, Unicolour Even, Unicolour Odd) : TextureSource;
    public sealed record Gradient(bool Vertical, Seq<(UnitInterval At, Unicolour Color)> Stops) : TextureSource;
    public sealed record Image(Dimension Width, Dimension Height, Seq<ReadOnlyMemory<ShadeVec4>> Levels) : TextureSource;
    public sealed record Triplanar(TextureSource Projected, double Scale, double BlendSharpness) : TextureSource;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ShadeVec4(double X, double Y, double Z, double W) {
    public static ShadeVec4 Splat(double v) => new(v, v, v, v);
    public static ShadeVec4 operator +(ShadeVec4 a, ShadeVec4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static ShadeVec4 operator *(ShadeVec4 a, double s) => new(a.X * s, a.Y * s, a.Z * s, a.W * s);
    public static ShadeVec4 Lerp(ShadeVec4 a, ShadeVec4 b, double t) => a * (1.0 - t) + b * t;

    public static ShadeVec4 FromColor(Unicolour colour) {
        var rgb = colour.RgbLinear;
        return new(rgb.R, rgb.G, rgb.B, 1.0);
    }

    public Fin<Unicolour> AsColor(Op key) =>
        double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z)
            ? Fin.Succ(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, X, Y, Z))
            : MaterialFault.Gamut(key, $"<texture-non-finite-rgb:{X:R},{Y:R},{Z:R}>");
}

public readonly record struct UvSample(UnitInterval U, UnitInterval V, Vector3d World, Vector3d Normal, double MipBias) {
    public static Fin<UvSample> Of(double u, double v, Op key) =>
        from cu in key.AcceptValidated<UnitInterval>(candidate: u)
        from cv in key.AcceptValidated<UnitInterval>(candidate: v)
        select new UvSample(cu, cv, Vector3d.Zero, Vector3d.ZAxis, 0.0);
}

public readonly record struct SamplerState(AddressMode AddressU, AddressMode AddressV, FilterMode Filter) {
    public static readonly SamplerState Default = new(AddressMode.Repeat, AddressMode.Repeat, FilterMode.Bilinear);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ProceduralNoise {
    private const int PrimeX = 501125321;
    private const int PrimeY = 1136930381;
    private const int PrimeZ = 1720413743;

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

    private static double Fade(double t) => t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    private static double Lerp(double a, double b, double t) => a + t * (b - a);

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

    public static double Simplex(double x, double y, int seed) {
        const double F2 = 0.3660254037844386;
        const double G2 = 0.21132486540518713;
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
        return 70.0 * n;
    }

    public static double Worley(double x, double y, int seed) {
        int xr = (int)Math.Round(x), yr = (int)Math.Round(y);
        double minDist = double.MaxValue;
        for (int dy = -1; dy <= 1; dy++) {
            for (int dx = -1; dx <= 1; dx++) {
                int cx = xr + dx, cy = yr + dy;
                int h = Hash(seed, cx * PrimeX, cy * PrimeY);
                double fx = cx + ((h & 0x3ff) / 1023.0 - 0.5);
                double fy = cy + (((h >> 10) & 0x3ff) / 1023.0 - 0.5);
                double ddx = fx - x, ddy = fy - y;
                minDist = Math.Min(minDist, ddx * ddx + ddy * ddy);
            }
        }
        return Math.Clamp(Math.Sqrt(minDist) * 2.0 - 1.0, -1.0, 1.0);
    }

    public static double Evaluate(NoiseBasis @base, double x, double y, int seed, int octaves, double lacunarity, double gain) {
        double sum = 0.0, amp = 1.0, freq = 1.0, norm = 0.0;
        for (int o = 0; o < Math.Max(1, octaves); o++) {
            sum += @base.Sample(x * freq, y * freq, seed + o) * amp;
            norm += amp;
            amp *= gain;
            freq *= lacunarity;
        }
        return sum / norm;
    }

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
    public static Fin<Unicolour> Sample(TextureSource source, UvSample point, SamplerState sampler, Op key) =>
        Field(source, point, sampler, key).Bind(field => field.AsColor(key));

    private static Fin<ShadeVec4> Field(TextureSource source, UvSample point, SamplerState sampler, Op key) {
        double u = point.U.Value, v = point.V.Value;
        if (!double.IsFinite(u) || !double.IsFinite(v)) { return MaterialFault.Parameter(key, $"<texture-uv-non-finite:{u:R},{v:R}>"); }
        return source.Switch(
            state:     (point, sampler, key),
            noise:     static (s, n) => Fin.Succ(SampleNoise(n, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value))),
            checker:   static (s, c) => Fin.Succ(SampleChecker(c, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value))),
            gradient:  static (s, g) => Fin.Succ(SampleGradient(g, s.sampler.AddressU.Apply(s.point.U.Value), s.sampler.AddressV.Apply(s.point.V.Value))),
            image:     static (s, img) => SampleImage(img, s.point, s.sampler, s.key),
            triplanar: static (s, t) => SampleTriplanar(t, s.point, s.sampler, s.key));
    }

    private static ShadeVec4 SampleNoise(TextureSource.Noise n, double u, double v) {
        double field = ProceduralNoise.Evaluate(n.Base, u * n.Frequency, v * n.Frequency, n.Seed, n.Octaves, n.Lacunarity, n.Gain);
        return ShadeVec4.Lerp(ShadeVec4.FromColor(n.Low), ShadeVec4.FromColor(n.High), Math.Clamp((field + 1.0) * 0.5, 0.0, 1.0));
    }

    private static ShadeVec4 SampleChecker(TextureSource.Checker c, double u, double v) {
        int parity = ((int)Math.Floor(u * c.Repeats) + (int)Math.Floor(v * c.Repeats)) & 1;
        return ShadeVec4.FromColor(parity == 0 ? c.Even : c.Odd);
    }

    private static ShadeVec4 SampleGradient(TextureSource.Gradient g, double u, double v) {
        double t = g.Vertical ? v : u;
        var stops = g.Stops;
        if (stops.IsEmpty) { return ShadeVec4.Splat(0.0); }
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

    private static Fin<ShadeVec4> SampleImage(TextureSource.Image img, UvSample point, SamplerState sampler, Op key) {
        if (img.Levels.IsEmpty) { return MaterialFault.Parameter(key, "<texture-image-empty>"); }
        double u = point.U.Value, v = point.V.Value;
        return sampler.Filter.Switch(
            nearest:   _ => ReconstructLevel(img, 0, u, v, sampler, FilterMode.Nearest, key),
            bilinear:  _ => ReconstructLevel(img, 0, u, v, sampler, FilterMode.Bilinear, key),
            bicubic:   _ => ReconstructLevel(img, 0, u, v, sampler, FilterMode.Bicubic, key),
            trilinear: _ => {
                double level = Math.Clamp(point.MipBias, 0.0, img.Levels.Count - 1.0);
                int lo = (int)Math.Floor(level), hi = Math.Min(lo + 1, img.Levels.Count - 1);
                double f = level - lo;
                return from a in ReconstructLevel(img, lo, u, v, sampler, FilterMode.Bilinear, key)
                       from b in ReconstructLevel(img, hi, u, v, sampler, FilterMode.Bilinear, key)
                       select ShadeVec4.Lerp(a, b, f);
            });
    }

    private static Fin<ShadeVec4> ReconstructLevel(TextureSource.Image img, int level, double u, double v, SamplerState sampler, FilterMode filter, Op key) {
        int w = Math.Max(1, img.Width.Value >> level), h = Math.Max(1, img.Height.Value >> level);
        ReadOnlyMemory<ShadeVec4> texels = img.Levels[level];
        if (texels.Length < w * h) { return MaterialFault.Parameter(key, $"<texture-level-undersized:{level}:{texels.Length}<{w * h}>"); }
        ReadOnlySpan<ShadeVec4> span = texels.Span;
        ShadeVec4 At(int ix, int iy) => span[sampler.AddressV.Texel(iy, h) * w + sampler.AddressU.Texel(ix, w)];

        double fx = u * w - 0.5, fy = v * h - 0.5;
        int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
        double tx = fx - x0, ty = fy - y0;
        ShadeVec4 Bilinear() => ShadeVec4.Lerp(ShadeVec4.Lerp(At(x0, y0), At(x0 + 1, y0), tx), ShadeVec4.Lerp(At(x0, y0 + 1), At(x0 + 1, y0 + 1), tx), ty);

        return Fin.Succ(filter.Switch(
            nearest:  _ => At((int)Math.Round(u * w - 0.5), (int)Math.Round(v * h - 0.5)),
            bilinear: _ => Bilinear(),
            bicubic:  _ => {
                ShadeVec4 Row(int iy) =>
                    CatmullRom(At(x0 - 1, iy), At(x0, iy), At(x0 + 1, iy), At(x0 + 2, iy), tx);
                return CatmullRom(Row(y0 - 1), Row(y0), Row(y0 + 1), Row(y0 + 2), ty);
            },
            trilinear: _ => Bilinear()));
    }

    private static ShadeVec4 CatmullRom(ShadeVec4 p0, ShadeVec4 p1, ShadeVec4 p2, ShadeVec4 p3, double t) {
        double t2 = t * t, t3 = t2 * t;
        double w0 = -0.5 * t3 + t2 - 0.5 * t;
        double w1 = 1.5 * t3 - 2.5 * t2 + 1.0;
        double w2 = -1.5 * t3 + 2.0 * t2 + 0.5 * t;
        double w3 = 0.5 * t3 - 0.5 * t2;
        return p0 * w0 + p1 * w1 + p2 * w2 + p3 * w3;
    }

    private static Fin<ShadeVec4> SampleTriplanar(TextureSource.Triplanar t, UvSample point, SamplerState sampler, Op key) {
        Vector3d n = point.Normal;
        double ax = Math.Pow(Math.Abs(n.X), t.BlendSharpness);
        double ay = Math.Pow(Math.Abs(n.Y), t.BlendSharpness);
        double az = Math.Pow(Math.Abs(n.Z), t.BlendSharpness);
        double sum = ax + ay + az;
        if (sum <= double.Epsilon) { return MaterialFault.Parameter(key, "<triplanar-degenerate-normal>"); }
        Vector3d p = point.World * t.Scale;
        UvSample Plane(double a, double b) => point with { U = UnitInterval.Create(Frac(a)), V = UnitInterval.Create(Frac(b)) };
        return from x in Field(t.Projected, Plane(p.Y, p.Z), sampler, key)
               from y in Field(t.Projected, Plane(p.Z, p.X), sampler, key)
               from z in Field(t.Projected, Plane(p.X, p.Y), sampler, key)
               select (x * (ax / sum)) + (y * (ay / sum)) + (z * (az / sum));
    }

    private static double Frac(double v) => Math.Clamp(v - Math.Floor(v), 0.0, 1.0);
}
```

## [3]-[RESEARCH]

- [SIMPLEX_PATENT]: the noise kernel uses OpenSimplex2 (the FastNoiseLite default), not Perlin's patented Simplex — the patent (US 6,867,776, expired 2022) is moot, but OpenSimplex2 is the vendored basis regardless, with the skewed-lattice three-corner summation transcribed inline; a value/cubic basis lands as one `NoiseBasis` row binding one `ProceduralNoise.Sample` arm.
- [MIP_GENERATION]: the `Image` case carries a pre-built mip pyramid (`Levels`); the box-filter downsample that generates it is the consumer's responsibility at texture import, not a sampler concern — the sampler reconstructs from the supplied pyramid and rails `<texture-level-undersized>` on a malformed payload rather than synthesizing levels at sample time.
- [MATERIALX_NODE_PARITY]: the MaterialX 1.39.4 standard-node library adds improved Worley noise and color-ramp nodes; aligning the `TextureSource`/`AddressMode`/`FilterMode` vocabulary onto the MaterialX node categories lets a `Texture` graph node round-trip through `.mtlx` (the `graph#MATERIALX_GRAPH_INTERCHANGE` target). The probe is the node-category mapping, not a second sampler.
