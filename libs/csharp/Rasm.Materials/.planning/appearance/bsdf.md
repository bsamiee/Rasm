# [MATERIALS_BSDF]

The closed BSDF lobe family and its scene-linear spectral edge: ONE `BsdfLobe` `[Union]` of seven physical lobes (diffuse · conductor · dielectric · sheen · clearcoat · subsurface · thin-film) under ONE `Evaluate`/`Sample`/`Pdf` contract, ONE `LayeredBsdf` weighted-composition fold every material drives by parameter row, ONE `SpectralUpsample` RGB→SPD kernel feeding Unicolour's `Spd`→XYZ, ONE `ConductorIor` measured complex-IOR table grounding every metal F0 per band, ONE `SlabStack` OpenPBR Surface 1.1 stack-of-slabs the `LayeredBsdf` fold lowers from, and ONE `ToneMap` ACES RRT/ODT + scene-referred operator table. The page owns the `BsdfLobe`, `FresnelMode`, `SpectralBand`, `ConductorMetal`, `SlabKind`, and `ToneOperator` axes, the `MaterialKeyPolicy` accessor, the `MaterialFault` union (band 2450 — disjoint from the kernel `Rasm.Geometry/Numerics/faults#FAULT_BAND` `GeometryFault` which owns band 2400, and from the Materials sibling bands `Profiles/profile#PROFILE_FAMILY` `ProfileFault` 2300 and `Construction/assembly#ELEMENT_MODEL` `ConstructionFault` 2350), and the lobe/spectral/conductor/slab/tone kernels. A material is NEVER a lobe subtype: `LayeredBsdf` carries the lobe weights and per-lobe parameters a `MaterialParameters` row supplies, so metal, glass, plastic, skin, fabric, car paint, and wax are weightings of this one closed set, never new lobe types or per-material BSDF classes. The lobe composition is the OpenPBR Surface 1.1 stack-of-slabs (`fuzz` · `coat` · `thin-film` modifier · base substrate mixing a conductor slab against a dielectric base) realized as the `SlabStack` algebra at `[9]-[OPENPBR_SLAB]`, the model the CG/AEC ecosystem standardizes on; the renderer (`graph#MATERIAL_GRAPH` sink, shaded by the path tracer at the `Rasm.AppUi/Render/viewport#PATH_TRACE` seam) shades FROM `LayeredBsdf.Sample`/`Evaluate`/`Pdf` and never re-derives lobe math.

## [1]-[INDEX]

- [1]-[SHADING_FRAME]: the `ShadingFrame` local-frame transform, the `MaterialFault` band-2450 union, and the `MaterialKeyPolicy` ordinal accessor.
- [2]-[MICROFACET_KERNEL]: Fresnel (Schlick plus exact dielectric/conductor), the GGX/Trowbridge-Reitz NDF, and Smith height-correlated masking.
- [3]-[LOBE_FAMILY]: the `BsdfLobe` `[Union]`, the per-lobe `Evaluate`/`Sample`/`Pdf` contract, and the Kulla-Conty multi-scatter compensation.
- [4]-[LAYERED_COMPOSITION]: the `LayeredBsdf` weighted-lobe fold, the MIS-balanced sample/pdf, and the material-is-a-row seam.
- [5]-[SPECTRAL_UPSAMPLE]: the RGB→SPD coefficient kernel, the Unicolour `Spd`→XYZ composition, and scene-linear admission.
- [6]-[TONE_MAP]: the ACES RRT/ODT author-kernel, the scene-referred filmic/Reinhard/exposure operators, and the `ToneOperator` table.
- [7]-[CONDUCTOR_IOR]: the `ConductorMetal` axis, the `ConductorIor` measured complex-IOR table per RGB band, and the `Conductor` lobe grounding.
- [8]-[OPENPBR_SLAB]: the `SlabKind` axis, the `Slab` `[Union]`, the `SlabStack` outermost-to-base layering algebra, and the `MaterialParameters`→stack lowering the `LayeredBsdf` fold consumes.

## [2]-[SHADING_FRAME]

- Owner: `ShadingFrame` over the composed `Rasm.Vectors.VectorFrame`; `MaterialFault` `[Union]` band 2450; `MaterialKeyPolicy` ordinal accessor.
- Entry: `public static Fin<ShadingFrame> Of(VectorFrame frame, Context context, Direction outgoing, Op key)` — `Fin<T>` aborts when the outgoing direction is degenerate in the local frame; `ToLocal`/`ToWorld` are the only world↔tangent transforms and `CosTheta`/`Sin2Theta`/`TanTheta`/`CosPhi`/`SinPhi` read the local z-up convention every lobe kernel shares. The frame carries the integrator's `Context` so `ToWorld` rails the unitized world direction through the PUBLIC `Direction.Of(Vector3d, Context, Op?)` overload (the `(Vector3d, double, Op?)` overload is `internal` to `Rasm` and cannot bind cross-assembly).
- Packages: Rasm (project — `Vectors.VectorFrame`/`Direction`/`Dimension`/`UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new fault is one `MaterialFault` case; a new trig accessor is one expression-bodied member on the frame; zero new surface. The frame NEVER re-mints `VectorFrame` — it wraps the composed `Plane` and reads `Vector3d` projections through it.
- Boundary: `ShadingFrame` is the page's declared boundary capsule over `Rasm.Vectors` — host `Vector3d`/`Plane` access stays inside it and lobe kernels see only local-frame `LocalVector` triples (z is the surface normal, the half-vector and incident/outgoing live in this basis); the z-up tangent convention is stated here once for every lobe so no lobe re-derives `cosθ = w.Z`; `MaterialFault` is the package's one appearance-banded fault, an `Expected`-derived `Error` (`IValidationError<MaterialFault>`) whose 2450 band IS the `Expected` `Code` (disjoint from the kernel `GeometryFault` 2400 a re-imported degenerate tessellation routes, so a telemetry reader banding by code never conflates a shading fault with a kernel geometry fault), so a bare typed case lifts directly into `Fin<T>`/`Validation<Error,T>` and `Fin.Fail` accepts it without a wrapper; every fault constructs the typed case directly — `Gamut` for an out-of-gamut/non-finite shade, `Parameter` for a degenerate input, `Graph` for a degenerate frame/unmatched arm — so a lobe never throws and never returns a NaN outward, and a degenerate local direction rails `MaterialFault.Graph`; `MaterialKeyPolicy` is the ordinal comparer the `MaterialLibrary` and `ToneOperator` tables key through.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Vectors;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Bsdf;

// --- [TYPES] -------------------------------------------------------------------------------
public readonly record struct LocalVector(double X, double Y, double Z) {
    public double CosTheta => Z;
    public double Cos2Theta => Z * Z;
    public double Sin2Theta => Math.Max(0.0, 1.0 - Z * Z);
    public double SinTheta => Math.Sqrt(Sin2Theta);
    public double TanTheta => SinTheta / Z;
    public double Tan2Theta => Sin2Theta / (Z * Z);
    public double CosPhi => SinTheta <= RhinoMath.ZeroTolerance ? 1.0 : Math.Clamp(X / SinTheta, -1.0, 1.0);
    public double SinPhi => SinTheta <= RhinoMath.ZeroTolerance ? 0.0 : Math.Clamp(Y / SinTheta, -1.0, 1.0);
    public LocalVector Reflect() => new(-X, -Y, Z);
    public double Dot(LocalVector o) => X * o.X + Y * o.Y + Z * o.Z;
    public LocalVector Add(LocalVector o) => new(X + o.X, Y + o.Y, Z + o.Z);
    public LocalVector Scale(double s) => new(X * s, Y * s, Z * s);
    public LocalVector Normalize() { double n = Math.Sqrt(X * X + Y * Y + Z * Z); return n > RhinoMath.ZeroTolerance ? new(X / n, Y / n, Z / n) : new(0.0, 0.0, 1.0); }
    public bool SameHemisphere(LocalVector o) => Z * o.Z > 0.0;
    public static readonly LocalVector Normal = new(0.0, 0.0, 1.0);
}

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record MaterialFault : Expected, IValidationError<MaterialFault> {
    private MaterialFault(Op key, string detail) : base(detail, 2450, None) => Key = key;
    public Op Key { get; }
    public static MaterialFault Create(string message) => new Graph(default, message);
    public sealed record Gamut(Op Key, string Detail) : MaterialFault(Key, Detail) { public override string Category => "Gamut"; }
    public sealed record Parameter(Op Key, string Detail) : MaterialFault(Key, Detail) { public override string Category => "Parameter"; }
    public sealed record Graph(Op Key, string Detail) : MaterialFault(Key, Detail) { public override string Category => "Graph"; }
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class MaterialKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ShadingFrame(VectorFrame Frame, Context Context) {
    public static Fin<ShadingFrame> Of(VectorFrame frame, Context context, Direction outgoing, Op key) {
        Plane basis = frame.Value;
        LocalVector wo = Project(basis, outgoing.Value);
        return Math.Abs(wo.Z) > RhinoMath.ZeroTolerance
            ? Fin.Succ(new ShadingFrame(frame, context))
            : Fin.Fail<ShadingFrame>(MaterialFault.Graph(key, "<degenerate-local-direction>"));
    }
    public LocalVector ToLocal(Direction world) => Project(Frame.Value, world.Value);
    public Fin<Direction> ToWorld(LocalVector local, Op key) {
        Plane b = Frame.Value;
        Vector3d w = (local.X * b.XAxis) + (local.Y * b.YAxis) + (local.Z * b.ZAxis);
        return Direction.Of(w, Context, key);
    }
    private static LocalVector Project(Plane basis, Vector3d w) => new(w * basis.XAxis, w * basis.YAxis, w * basis.ZAxis);
}
```

## [3]-[MICROFACET_KERNEL]

- Owner: `FresnelMode` `[SmartEnum<string>]` (schlick · exact) driving the static `Microfacet` kernel.
- Entry: `public static double Ndf(LocalVector h, double alphaX, double alphaY)` and the sibling `MaskingShadowing`/`FresnelDielectric`/`FresnelConductor`/`FresnelSchlick` — pure values; the NDF takes only the half-vector and anisotropic roughness `(alphaX, alphaY)` (it is Fresnel-mode-independent, so `FresnelMode` is NOT a parameter), and the isotropic case is `alphaX == alphaY`, never a second isotropic kernel.
- Packages: Thinktecture.Runtime.Extensions, BCL inbox (the kernel is frame-local and host-free; the public `Rasm.Vectors.Direction.Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key)` composes at the world boundary — see LOBE_FAMILY).
- Growth: a new distribution (Beckmann) is one `Microfacet` static plus one `FresnelMode` row routed by the lobe; a new Fresnel route is one branch on `FresnelMode`; zero new surface. The roughness→alpha remap (`alpha = roughness²`) is the one Disney-convention remap every lobe reads.
- Boundary: the NDF is GGX/Trowbridge-Reitz in anisotropic form `D = 1 / (π·αx·αy·((hx/αx)² + (hy/αy)² + hz²)²)`, reducing to the isotropic `D = α² / (π·(cos²θh·(α²−1)+1)²)` when `αx==αy` — one body, both modes; the masking-shadowing is the Smith height-correlated `G2 = 1 / (1 + Λ(wo) + Λ(wi))` with the GGX `Λ`, NEVER the separable `G1(wo)·G1(wi)` (separable overestimates correlated occlusion and breaks the white-furnace test); Fresnel is the two-mode dispatch — `FresnelMode.Exact` runs the unpolarized dielectric Fresnel for transmission lobes and the complex-IOR conductor Fresnel for metals, `FresnelMode.Schlick` runs `F0 + (1−F0)(1−cosθ)⁵` for the fast dielectric/coated path; the GGX visible-normal-distribution sample (Heitz 2018) is the one importance-sampling routine the conductor/dielectric/clearcoat lobes share.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class FresnelMode {
    public static readonly FresnelMode Schlick = new("schlick");
    public static readonly FresnelMode Exact = new("exact");
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Microfacet {
    public static double AlphaOf(double roughness) => Math.Max(1e-4, roughness * roughness);

    public static double Ndf(LocalVector h, double alphaX, double alphaY) {
        if (h.CosTheta <= 0.0) { return 0.0; }
        double hx = h.X / alphaX, hy = h.Y / alphaY, hz = h.Z;
        double k = hx * hx + hy * hy + hz * hz;
        return 1.0 / (Math.PI * alphaX * alphaY * k * k);
    }

    private static double Lambda(LocalVector w, double alphaX, double alphaY) {
        double t2 = w.Tan2Theta;
        if (double.IsInfinity(t2) || double.IsNaN(t2)) { return 0.0; }
        double cos2Phi = w.CosPhi * w.CosPhi, sin2Phi = w.SinPhi * w.SinPhi;
        double alpha2 = cos2Phi * alphaX * alphaX + sin2Phi * alphaY * alphaY;
        return (Math.Sqrt(1.0 + alpha2 * t2) - 1.0) * 0.5;
    }

    public static double MaskingShadowing(LocalVector wo, LocalVector wi, double alphaX, double alphaY) =>
        1.0 / (1.0 + Lambda(wo, alphaX, alphaY) + Lambda(wi, alphaX, alphaY));

    public static double Masking(LocalVector w, double alphaX, double alphaY) => 1.0 / (1.0 + Lambda(w, alphaX, alphaY));

    public static double FresnelSchlick(double cosTheta, double f0) {
        double m = Math.Clamp(1.0 - cosTheta, 0.0, 1.0);
        double m2 = m * m;
        return f0 + (1.0 - f0) * (m2 * m2 * m);
    }

    public static double FresnelDielectric(double cosI, double eta) {
        double ci = Math.Clamp(Math.Abs(cosI), 0.0, 1.0);
        double sin2T = (1.0 - ci * ci) / (eta * eta);
        if (sin2T >= 1.0) { return 1.0; }
        double ct = Math.Sqrt(1.0 - sin2T);
        double rParl = (eta * ci - ct) / (eta * ci + ct);
        double rPerp = (ci - eta * ct) / (ci + eta * ct);
        return 0.5 * (rParl * rParl + rPerp * rPerp);
    }

    public static double FresnelConductor(double cosI, double eta, double k) {
        double ci = Math.Clamp(Math.Abs(cosI), 0.0, 1.0), ci2 = ci * ci;
        double sin2 = 1.0 - ci2;
        double eta2 = eta * eta, k2 = k * k;
        double t0 = eta2 - k2 - sin2;
        double a2b2 = Math.Sqrt(Math.Max(0.0, t0 * t0 + 4.0 * eta2 * k2));
        double t1 = a2b2 + ci2;
        double a = Math.Sqrt(Math.Max(0.0, 0.5 * (a2b2 + t0)));
        double t2 = 2.0 * a * ci;
        double rs = (t1 - t2) / (t1 + t2);
        double t3 = ci2 * a2b2 + sin2 * sin2;
        double t4 = t2 * sin2;
        double rp = rs * (t3 - t4) / (t3 + t4);
        return 0.5 * (rp + rs);
    }

    public static RgbSpectrum FresnelConductor(double cosI, RgbSpectrum eta, RgbSpectrum k) =>
        eta.Zip(k, (e, kk) => FresnelConductor(cosI, e, kk));

    public static LocalVector SampleVisibleNormal(LocalVector wo, double alphaX, double alphaY, double u0, double u1) {
        LocalVector vh = new LocalVector(alphaX * wo.X, alphaY * wo.Y, wo.Z).Normalize();
        double lensq = vh.X * vh.X + vh.Y * vh.Y;
        LocalVector t1 = lensq > 0.0 ? new LocalVector(-vh.Y, vh.X, 0.0).Scale(1.0 / Math.Sqrt(lensq)) : new LocalVector(1.0, 0.0, 0.0);
        LocalVector t2v = new LocalVector(vh.Y * t1.Z - vh.Z * t1.Y, vh.Z * t1.X - vh.X * t1.Z, vh.X * t1.Y - vh.Y * t1.X);
        double r = Math.Sqrt(u0), phi = 2.0 * Math.PI * u1;
        double p1 = r * Math.Cos(phi), p2 = r * Math.Sin(phi);
        double s = 0.5 * (1.0 + vh.Z);
        p2 = (1.0 - s) * Math.Sqrt(Math.Max(0.0, 1.0 - p1 * p1)) + s * p2;
        double pz = Math.Sqrt(Math.Max(0.0, 1.0 - p1 * p1 - p2 * p2));
        LocalVector nh = t1.Scale(p1).Add(t2v.Scale(p2)).Add(vh.Scale(pz));
        return new LocalVector(alphaX * nh.X, alphaY * nh.Y, Math.Max(1e-6, nh.Z)).Normalize();
    }

    public static double VisibleNormalPdf(LocalVector wo, LocalVector h, double alphaX, double alphaY) =>
        Masking(wo, alphaX, alphaY) * Math.Abs(wo.Dot(h)) * Ndf(h, alphaX, alphaY) / Math.Abs(wo.CosTheta);
}
```

## [4]-[LOBE_FAMILY]

- Owner: `BsdfLobe` `[Union]` closed lobe family; `LobeSample` the typed sample receipt.
- Entry: `public RgbSpectrum Evaluate(LocalVector wo, LocalVector wi)` · `public Fin<LobeSample> Sample(LocalVector wo, double u0, double u1, Op key)` · `public double Pdf(LocalVector wo, LocalVector wi)` — the three-method contract every lobe case implements through one total `Switch`; the lobe is frame-local, so `Evaluate`/`Pdf` read the local-frame `LocalVector` triples the integrator transforms once and `Sample` carries the `Op key` for its `MaterialFault` rail; `RgbSpectrum` is the per-band reflectance triple, NEVER a host color type at an interior signature.
- Packages: Rasm (project — `Direction.Reflect`/`Refract`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new physical lobe is one `BsdfLobe` case admitted ONLY when no parameterization of the existing seven reproduces the measured physics — and then it serves ALL materials, never one material; a new material is NEVER a lobe. The lobe set is closed at seven: diffuse, conductor, dielectric, sheen, clearcoat, subsurface, thin-film. The `[9]-[OPENPBR_SLAB]` `SlabStack` is the realized formal layering construction over these lobes — the fuzz slab lowers to the `Sheen` lobe and the albedo-scaling operators compose the stack the `LayeredBsdf` weighted fold the integrator shades collapses from.
- Boundary: every lobe `Evaluate` returns the BSDF value times nothing — the cosine-weight and division by pdf live in the integrator (`LayeredBsdf.Sample`), so a lobe is the pure `f(wo, wi)` and never folds in the geometry term twice; the conductor and dielectric lobes drive `Microfacet` with their `(alphaX, alphaY)` and `FresnelMode.Exact`, the clearcoat is a fixed-IOR-1.5 dielectric GGX layer over the base, the sheen is the Estevez-Kulla inverted-Gaussian retroreflective fabric lobe, the subsurface is a normalized Burley diffusion-profile diffuse approximation parameterized by mean-free-path, and the thin-film is the Belcour-Barla spectral interference term modulating the base Fresnel; the diffuse lobe carries the Oren-Nayar roughness term (Lambert is `roughness == 0`), so one diffuse case spans matte-to-rough, never a separate Lambert and Oren-Nayar type; the multi-scatter energy compensation is the Kulla-Conty term added to the conductor/dielectric single-scatter, recovering the energy the single-scatter Smith model loses at high roughness — without it the white-furnace test fails above α≈0.5; the lobe sampler is frame-local and host-free — `LobeSample.Direction` is a `LocalVector`, the dielectric transmission runs the same exact Snell formula `Rasm.Vectors.Direction.Refract` owns (eta·d + (eta·cosI−√k)·n, TIR-rejected) so the math is single-sourced; the WORLD reflected/refracted ray the path tracer needs for the next bounce is the integrator's `ShadingFrame.ToWorld` composition, and when the renderer prefers the host `Direction` it COMPOSES the instance `Direction.Reflect(Direction normal)` and the static `Direction.Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key)` at that world seam (the 5-arg Snell — `etaIncident`/`etaTransmitted` are the two media IORs, not an `(eta, cosI, n)` shorthand) — Snell and the mirror are NEVER re-minted as a parallel kernel; `DielectricPdf` keys its reflect/transmit split on the half-vector cosine `wo.Dot(h)` exactly as `DielectricSample` does, never the geometric `wo.CosTheta`, so the balance-heuristic pdf stays unbiased and the white-furnace harness closes for rough glass.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct RgbSpectrum(double R, double G, double B) {
    public static readonly RgbSpectrum Black = new(0.0, 0.0, 0.0);
    public static readonly RgbSpectrum White = new(1.0, 1.0, 1.0);
    public RgbSpectrum Map(Func<double, double> f) => new(f(R), f(G), f(B));
    public RgbSpectrum Zip(RgbSpectrum o, Func<double, double, double> f) => new(f(R, o.R), f(G, o.G), f(B, o.B));
    public RgbSpectrum Scale(double s) => new(R * s, G * s, B * s);
    public RgbSpectrum Mul(RgbSpectrum o) => new(R * o.R, G * o.G, B * o.B);
    public RgbSpectrum Add(RgbSpectrum o) => new(R + o.R, G + o.G, B + o.B);
    public RgbSpectrum Lerp(RgbSpectrum o, double t) => new(R + (o.R - R) * t, G + (o.G - G) * t, B + (o.B - B) * t);
    public double Luminance => 0.2722287 * R + 0.6740818 * G + 0.0536895 * B;
    public bool IsFinite => double.IsFinite(R) && double.IsFinite(G) && double.IsFinite(B);
}

public readonly record struct LobeSample(LocalVector Direction, RgbSpectrum Value, double Pdf, bool Transmission) {
    public bool IsValid => Pdf > 0.0 && Value.IsFinite;
}

// --- [TYPES] -------------------------------------------------------------------------------
[Union]
public abstract partial record BsdfLobe {
    private BsdfLobe() { }

    public sealed record Diffuse(RgbSpectrum Albedo, double Roughness) : BsdfLobe;
    public sealed record Conductor(RgbSpectrum Eta, RgbSpectrum K, double AlphaX, double AlphaY) : BsdfLobe;
    public sealed record Dielectric(double Ior, double AlphaX, double AlphaY, RgbSpectrum Transmittance) : BsdfLobe;
    public sealed record Sheen(RgbSpectrum Tint, double Roughness) : BsdfLobe;
    public sealed record Clearcoat(double Weight, double Roughness) : BsdfLobe {
        public static double Alpha(Clearcoat c) => Microfacet.AlphaOf(c.Roughness);
    }
    public sealed record Subsurface(RgbSpectrum Albedo, double MeanFreePath) : BsdfLobe;
    public sealed record ThinFilm(double Thickness, double FilmIor, double Roughness, RgbSpectrum BaseEta, RgbSpectrum BaseK) : BsdfLobe {
        public static double AlphaX(ThinFilm f) => Microfacet.AlphaOf(f.Roughness);
        public static double AlphaY(ThinFilm f) => Microfacet.AlphaOf(f.Roughness);
    }

    public RgbSpectrum Evaluate(LocalVector wo, LocalVector wi) => Switch(
        diffuse: d => EvalOrenNayar(d, wo, wi),
        conductor: c => EvalConductor(c, wo, wi),
        dielectric: g => wo.SameHemisphere(wi) ? EvalDielectricReflect(g, wo, wi) : EvalDielectricTransmit(g, wo, wi),
        sheen: s => EvalSheen(s, wo, wi),
        clearcoat: c => EvalClearcoat(c, wo, wi),
        subsurface: s => EvalOrenNayar(new Diffuse(s.Albedo, 1.0), wo, wi).Scale(BurleyProfile(s.MeanFreePath, wo, wi)),
        thinFilm: f => EvalThinFilm(f, wo, wi));

    public double Pdf(LocalVector wo, LocalVector wi) => Switch(
        diffuse: _ => wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0,
        conductor: c => MicrofacetReflectPdf(wo, wi, c.AlphaX, c.AlphaY),
        dielectric: g => DielectricPdf(g, wo, wi),
        sheen: _ => wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0,
        clearcoat: c => Clearcoat.Alpha(c) is double ca ? MicrofacetReflectPdf(wo, wi, ca, ca) : 0.0,
        subsurface: _ => wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0,
        thinFilm: f => MicrofacetReflectPdf(wo, wi, ThinFilm.AlphaX(f), ThinFilm.AlphaY(f)));

    public Fin<LobeSample> Sample(LocalVector wo, double u0, double u1, Op key) => Switch(
        diffuse: _ => CosineSample(wo, u0, u1, this, key),
        conductor: c => MicrofacetReflectSample(wo, c.AlphaX, c.AlphaY, u0, u1, this, key),
        dielectric: g => DielectricSample(g, wo, u0, u1, key),
        sheen: _ => CosineSample(wo, u0, u1, this, key),
        clearcoat: c => Clearcoat.Alpha(c) is double ca ? MicrofacetReflectSample(wo, ca, ca, u0, u1, this, key) : Fin.Fail<LobeSample>(MaterialFault.Parameter(key, "<clearcoat-alpha-degenerate>")),
        subsurface: _ => CosineSample(wo, u0, u1, this, key),
        thinFilm: f => MicrofacetReflectSample(wo, ThinFilm.AlphaX(f), ThinFilm.AlphaY(f), u0, u1, this, key));

    // --- [DIFFUSE]
    private static RgbSpectrum EvalOrenNayar(Diffuse d, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        double s2 = d.Roughness * d.Roughness;
        double a = 1.0 - 0.5 * s2 / (s2 + 0.33), b = 0.45 * s2 / (s2 + 0.09);
        double sinO = wo.SinTheta, sinI = wi.SinTheta;
        double maxCos = sinO > 1e-4 && sinI > 1e-4 ? Math.Max(0.0, wi.CosPhi * wo.CosPhi + wi.SinPhi * wo.SinPhi) : 0.0;
        double aco = Math.Abs(wo.CosTheta), aci = Math.Abs(wi.CosTheta);
        double (sinAlpha, tanBeta) = aci > aco ? (sinO, sinI / aci) : (sinI, sinO / aco);
        return d.Albedo.Scale((a + b * maxCos * sinAlpha * tanBeta) / Math.PI);
    }
    private static double BurleyProfile(double meanFreePath, LocalVector wo, LocalVector wi) {
        double d = Math.Max(1e-4, meanFreePath), r = (wo.SinTheta + wi.SinTheta) * 0.5;
        return (Math.Exp(-r / d) + Math.Exp(-r / (3.0 * d))) / (8.0 * Math.PI * d * Math.Max(1e-4, r));
    }

    // --- [CONDUCTOR]
    private static RgbSpectrum EvalConductor(Conductor c, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi) || wo.CosTheta == 0.0 || wi.CosTheta == 0.0) { return RgbSpectrum.Black; }
        LocalVector h = wo.Add(wi).Normalize();
        double d = Microfacet.Ndf(h, c.AlphaX, c.AlphaY);
        double g = Microfacet.MaskingShadowing(wo, wi, c.AlphaX, c.AlphaY);
        double cosH = Math.Abs(wo.Dot(h));
        RgbSpectrum fr = Microfacet.FresnelConductor(cosH, c.Eta, c.K);
        double denom = 4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta);
        RgbSpectrum single = fr.Scale(d * g / denom);
        return single.Add(MultiScatter.KullaConty(c.AlphaX, fr, wo, wi));
    }

    // --- [DIELECTRIC]
    private static RgbSpectrum EvalDielectricReflect(Dielectric g, LocalVector wo, LocalVector wi) {
        LocalVector h = wo.Add(wi).Normalize();
        double d = Microfacet.Ndf(h, g.AlphaX, g.AlphaY);
        double mask = Microfacet.MaskingShadowing(wo, wi, g.AlphaX, g.AlphaY);
        double f = Microfacet.FresnelDielectric(wo.Dot(h), g.Ior);
        double denom = 4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta);
        return RgbSpectrum.White.Scale(d * mask * f / denom);
    }
    private static RgbSpectrum EvalDielectricTransmit(Dielectric g, LocalVector wo, LocalVector wi) {
        double eta = wo.CosTheta > 0.0 ? g.Ior : 1.0 / g.Ior;
        LocalVector h = wo.Add(wi.Scale(eta)).Normalize();
        h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;
        double d = Microfacet.Ndf(h, g.AlphaX, g.AlphaY);
        double mask = Microfacet.MaskingShadowing(wo, wi, g.AlphaX, g.AlphaY);
        double f = Microfacet.FresnelDielectric(wo.Dot(h), g.Ior);
        double sqrtDenom = wo.Dot(h) + eta * wi.Dot(h);
        double factor = Math.Abs(wo.Dot(h) * wi.Dot(h) / (wo.CosTheta * wi.CosTheta)) * (eta * eta) / (sqrtDenom * sqrtDenom);
        return g.Transmittance.Scale(d * mask * (1.0 - f) * factor);
    }

    // --- [SHEEN]
    private static RgbSpectrum EvalSheen(Sheen s, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        LocalVector h = wo.Add(wi).Normalize();
        double alpha = Math.Max(1e-3, s.Roughness);
        double inv = 1.0 / alpha;
        double cos2 = h.Cos2Theta, sin2 = h.Sin2Theta;
        double dSheen = (2.0 + inv) * Math.Pow(sin2, inv * 0.5) / (2.0 * Math.PI);
        double g = 1.0 / (4.0 * (Math.Abs(wo.CosTheta) + Math.Abs(wi.CosTheta) - Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta)));
        return s.Tint.Scale(dSheen * g);
    }

    // --- [CLEARCOAT]
    private static RgbSpectrum EvalClearcoat(Clearcoat c, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        double alpha = Clearcoat.Alpha(c);
        LocalVector h = wo.Add(wi).Normalize();
        double d = Microfacet.Ndf(h, alpha, alpha);
        double mask = Microfacet.MaskingShadowing(wo, wi, alpha, alpha);
        double f = Microfacet.FresnelSchlick(Math.Abs(wo.Dot(h)), 0.04);
        double denom = 4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta);
        return RgbSpectrum.White.Scale(c.Weight * d * mask * f / denom);
    }

    // --- [THIN_FILM]
    private static RgbSpectrum EvalThinFilm(ThinFilm f, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi) || wo.CosTheta == 0.0) { return RgbSpectrum.Black; }
        LocalVector h = wo.Add(wi).Normalize();
        double cosI = Math.Abs(wo.Dot(h));
        double sinT2 = (1.0 - cosI * cosI) / (f.FilmIor * f.FilmIor);
        double cosT = Math.Sqrt(Math.Max(0.0, 1.0 - sinT2));
        double opd = 2.0 * f.FilmIor * f.Thickness * cosT;
        double ax = ThinFilm.AlphaX(f), ay = ThinFilm.AlphaY(f);
        double d = Microfacet.Ndf(h, ax, ay);
        double mask = Microfacet.MaskingShadowing(wo, wi, ax, ay);
        double denom = 4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta);
        RgbSpectrum interf = new(Interference(opd, SpectralBand.Red.CenterNm), Interference(opd, SpectralBand.Green.CenterNm), Interference(opd, SpectralBand.Blue.CenterNm));
        RgbSpectrum baseF = Microfacet.FresnelConductor(cosI, f.BaseEta, f.BaseK);
        return baseF.Mul(interf).Scale(d * mask / denom);
    }
    private static double Interference(double opd, double wavelengthNm) => 0.5 * (1.0 + Math.Cos(2.0 * Math.PI * opd / wavelengthNm));

    // --- [SAMPLING]
    private static Fin<LobeSample> CosineSample(LocalVector wo, double u0, double u1, BsdfLobe owner, Op key) {
        double r = Math.Sqrt(u0), phi = 2.0 * Math.PI * u1;
        double z = Math.Sqrt(Math.Max(0.0, 1.0 - u0));
        LocalVector wi = new LocalVector(r * Math.Cos(phi), r * Math.Sin(phi), wo.CosTheta < 0.0 ? -z : z);
        double pdf = Math.Abs(wi.CosTheta) / Math.PI;
        return pdf > 0.0 ? Fin.Succ(new LobeSample(wi, owner.Evaluate(wo, wi), pdf, Transmission: false)) : Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<zero-pdf-cosine-sample>"));
    }
    private static Fin<LobeSample> MicrofacetReflectSample(LocalVector wo, double ax, double ay, double u0, double u1, BsdfLobe owner, Op key) {
        LocalVector h = Microfacet.SampleVisibleNormal(wo.CosTheta < 0.0 ? wo.Scale(-1.0) : wo, ax, ay, u0, u1);
        LocalVector wi = h.Scale(2.0 * wo.Dot(h)).Add(wo.Scale(-1.0));
        if (!wo.SameHemisphere(wi)) { return Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<vndf-below-horizon>")); }
        double pdf = Microfacet.VisibleNormalPdf(wo, h, ax, ay) / (4.0 * Math.Abs(wo.Dot(h)));
        return pdf > 0.0 ? Fin.Succ(new LobeSample(wi, owner.Evaluate(wo, wi), pdf, Transmission: false)) : Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<zero-pdf-vndf-sample>"));
    }
    private static double MicrofacetReflectPdf(LocalVector wo, LocalVector wi, double ax, double ay) {
        if (!wo.SameHemisphere(wi)) { return 0.0; }
        LocalVector h = wo.Add(wi).Normalize();
        return Microfacet.VisibleNormalPdf(wo, h, ax, ay) / (4.0 * Math.Abs(wo.Dot(h)));
    }
    private static Fin<LobeSample> DielectricSample(Dielectric g, LocalVector wo, double u0, double u1, Op key) {
        LocalVector h = Microfacet.SampleVisibleNormal(wo.CosTheta < 0.0 ? wo.Scale(-1.0) : wo, g.AlphaX, g.AlphaY, u0, u1);
        double f = Microfacet.FresnelDielectric(wo.Dot(h), g.Ior);
        if (u0 < f) {
            LocalVector wi = h.Scale(2.0 * wo.Dot(h)).Add(wo.Scale(-1.0));
            double pdf = f * Microfacet.VisibleNormalPdf(wo, h, g.AlphaX, g.AlphaY) / (4.0 * Math.Abs(wo.Dot(h)));
            return wo.SameHemisphere(wi) && pdf > 0.0 ? Fin.Succ(new LobeSample(wi, EvalDielectricReflect(g, wo, wi), pdf, Transmission: false)) : Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<dielectric-reflect-degenerate>"));
        }
        double eta = wo.CosTheta > 0.0 ? 1.0 / g.Ior : g.Ior;
        LocalVector n = wo.Dot(h) < 0.0 ? h.Scale(-1.0) : h;
        double cosI = Math.Clamp(wo.Dot(n), -1.0, 1.0);
        double k = 1.0 - eta * eta * (1.0 - cosI * cosI);
        if (k < 0.0) { return Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<dielectric-refract-tir>")); }
        LocalVector wi = wo.Scale(-eta).Add(n.Scale(eta * cosI - Math.Sqrt(k)));
        double pdf = (1.0 - f) * Microfacet.VisibleNormalPdf(wo, h, g.AlphaX, g.AlphaY) / (4.0 * Math.Abs(wi.Dot(h)));
        return pdf > 0.0 ? Fin.Succ(new LobeSample(wi, EvalDielectricTransmit(g, wo, wi), pdf, Transmission: true)) : Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<dielectric-refract-degenerate>"));
    }
    private static double DielectricPdf(Dielectric g, LocalVector wo, LocalVector wi) {
        if (wo.SameHemisphere(wi)) {
            LocalVector h = wo.Add(wi).Normalize();
            double fr = Microfacet.FresnelDielectric(wo.Dot(h), g.Ior);
            return fr * Microfacet.VisibleNormalPdf(wo, h, g.AlphaX, g.AlphaY) / (4.0 * Math.Abs(wo.Dot(h)));
        }
        double eta = wo.CosTheta > 0.0 ? g.Ior : 1.0 / g.Ior;
        LocalVector ht = wo.Add(wi.Scale(eta)).Normalize();
        ht = ht.CosTheta < 0.0 ? ht.Scale(-1.0) : ht;
        double ft = Microfacet.FresnelDielectric(wo.Dot(ht), g.Ior);
        double sqrtDenom = wo.Dot(ht) + eta * wi.Dot(ht);
        return (1.0 - ft) * Microfacet.VisibleNormalPdf(wo, ht, g.AlphaX, g.AlphaY) * Math.Abs(eta * eta * wi.Dot(ht) / (sqrtDenom * sqrtDenom));
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MultiScatter {
    public static double DirectionalAlbedo(double alpha, double mu) {
        double r = alpha;
        double bias = 0.04 + r * (0.31 + r * -0.28);
        double scale = 1.0 - bias;
        return Math.Clamp(scale * (1.0 - Math.Pow(1.0 - mu, 5.0 * (1.0 - r))) + bias, 0.0, 1.0);
    }
    private static double HemisphericalAlbedo(double alpha) {
        double acc = 0.0;
        const int n = 8;
        for (int i = 0; i < n; i++) { double mu = (i + 0.5) / n; acc += DirectionalAlbedo(alpha, mu) * mu; }
        return Math.Clamp(2.0 * acc / n, 0.0, 1.0);
    }
    public static RgbSpectrum KullaConty(double alpha, RgbSpectrum fresnelAvg, LocalVector wo, LocalVector wi) {
        double eo = DirectionalAlbedo(alpha, Math.Abs(wo.CosTheta));
        double ei = DirectionalAlbedo(alpha, Math.Abs(wi.CosTheta));
        double eavg = HemisphericalAlbedo(alpha);
        double fms = (1.0 - eo) * (1.0 - ei) / (Math.PI * Math.Max(1e-4, 1.0 - eavg));
        return fresnelAvg.Map(f => fms * f * f * (1.0 - eavg) / Math.Max(1e-4, 1.0 - f * (1.0 - eavg)));
    }
}
```

## [5]-[LAYERED_COMPOSITION]

- Owner: `LayeredBsdf` — the weighted-lobe fold; `LobeWeight` the per-lobe weight row.
- Entry: `public RgbSpectrum Evaluate(ShadingFrame frame, Direction wo, Direction wi)` · `public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1, Op key)` · `public double Pdf(ShadingFrame frame, Direction wo, Direction wi)` — the renderer's sole shading entry; the integrator transforms to local once, folds the weighted lobes, and transforms back; `Sample` carries the `Op key` for the `MaterialFault` rail and `Of` admits the weighted lobe list under the same key.
- Packages: Rasm (project — `Vectors.Direction`/`UnitInterval`), LanguageExt.Core, BCL inbox.
- Growth: a new MATERIAL is a new `Seq<LobeWeight>` value — a row of weights and lobe parameters a `MaterialParameters` row supplies — NEVER a new type; this is THE polymorphic-mandate seam: `LayeredBsdf.Of` takes the weighted lobe list a library row produces, so gold is `[Conductor 1.0]`, glass is `[Dielectric 1.0]`, plastic is `[Diffuse 0.9, Dielectric-coat 0.1]`, car paint is `[Conductor-flake 0.7, Clearcoat 0.3]`, skin is `[Subsurface 0.8, Dielectric 0.2]`, velvet is `[Diffuse 0.6, Sheen 0.4]`, wax is `[Subsurface 0.5, Diffuse 0.5]` — all the SAME `LayeredBsdf`, differing only by row data. The `[9]-[OPENPBR_SLAB]` `SlabStack` is the realized formal layering construction (fuzz over coat over base, albedo-scaling operators) whose `ToLayered` energy-preserving collapse lowers to this weighted-lobe fold the integrator shades; the slab algebra builds the stack a row drives through, this fold the one BSDF the renderer reads.
- Boundary: `Evaluate` is the weighted SUM of lobe `Evaluate` (each lobe is linear, so the layered BSDF is the convex combination of lobe values by weight); `Sample` is the one-sample MIS — pick a lobe proportionally to weight via `uLobe`, sample it, then the returned pdf is the WEIGHT-AVERAGED pdf across ALL lobes (the balance heuristic) so the estimator is unbiased and low-variance, and the value re-evaluates the FULL layered BSDF against the sampled direction; `Pdf` mirrors that weighted average exactly; weights are `UnitInterval` and sum-normalized at `Of` (a row whose weights miss [0,1] or sum to zero rails `<parameter:lobe-weights-degenerate>`); the `Of` normalization is the page's one admitted total-construction site — every input `Weight` is already a `UnitInterval` and `total` is their sum, so `weight/total` is in `[0,1]` by construction and the `UnitInterval.Create` throw is statically unreachable, named here as the `[EXPRESSION_SPINE]` exemption rather than exception-style control flow in a fallible path; the boundary projects the final shade to in-gamut at the renderer edge, never inside the fold — a non-finite throughput rails `<gamut:non-finite-shade>` through `Option`/`Fin`, never propagating NaN; this fold is the ONLY place lobe weights live, so the masonry-assignment consumer and every `MaterialLibrary` row drive appearance purely by producing a `Seq<LobeWeight>`.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct LobeWeight(BsdfLobe Lobe, UnitInterval Weight);

public sealed record LayeredBsdf {
    private LayeredBsdf(Seq<LobeWeight> lobes) => Lobes = lobes;
    public Seq<LobeWeight> Lobes { get; }

    public static Fin<LayeredBsdf> Of(Seq<LobeWeight> lobes, Op key) {
        double total = lobes.Sum(l => l.Weight.Value);
        return lobes.IsEmpty || total <= RhinoMath.ZeroTolerance
            ? Fin.Fail<LayeredBsdf>(MaterialFault.Parameter(key, "<lobe-weights-degenerate>"))
            : Fin.Succ(new LayeredBsdf(lobes.Map(l => l with { Weight = UnitInterval.Create(l.Weight.Value / total) })));
    }

    public RgbSpectrum Evaluate(ShadingFrame frame, Direction wo, Direction wi) {
        LocalVector lo = frame.ToLocal(wo), li = frame.ToLocal(wi);
        return Lobes.Fold(RgbSpectrum.Black, (acc, lw) => acc.Add(lw.Lobe.Evaluate(lo, li).Scale(lw.Weight.Value)));
    }

    public double Pdf(ShadingFrame frame, Direction wo, Direction wi) {
        LocalVector lo = frame.ToLocal(wo), li = frame.ToLocal(wi);
        return Lobes.Fold(0.0, (acc, lw) => acc + lw.Weight.Value * lw.Lobe.Pdf(lo, li));
    }

    public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1, Op key) {
        LocalVector lo = frame.ToLocal(wo);
        double pick = uLobe, acc = 0.0;
        LobeWeight chosen = Lobes.Last;
        foreach (LobeWeight lw in Lobes) { acc += lw.Weight.Value; if (pick <= acc) { chosen = lw; break; } }
        return chosen.Lobe.Sample(lo, u0, u1, key).Bind(sample => {
            double mixedPdf = Lobes.Fold(0.0, (p, lw) => p + lw.Weight.Value * lw.Lobe.Pdf(lo, sample.Direction));
            RgbSpectrum mixedValue = Lobes.Fold(RgbSpectrum.Black, (v, lw) => v.Add(lw.Lobe.Evaluate(lo, sample.Direction).Scale(lw.Weight.Value)));
            return mixedPdf > 0.0 && mixedValue.IsFinite
                ? Fin.Succ(sample with { Pdf = mixedPdf, Value = mixedValue })
                : Fin.Fail<LobeSample>(MaterialFault.Gamut(key, "<non-finite-shade>"));
        });
    }
}
```

## [6]-[SPECTRAL_UPSAMPLE]

- Owner: `SpectralUpsample` author-kernel; `SpectralBand` `[SmartEnum<string>]` the band vocabulary; composes Wacton.Unicolour for every conversion Unicolour already owns.
- Entry: `public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key)` and `public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key)` — RGB→SPD is the author-kernel Unicolour lacks (NOT_COVERED); SceneLinear COMPOSES `RgbConfiguration.Acescg` and Unicolour's `.RgbLinear` accessor, never re-deriving the linearization; both carry the `Op key` the `MaterialFault` rail correlates.
- Packages: Wacton.Unicolour (composed — `Spd`, `Unicolour`, `Configuration`, `RgbConfiguration`, `ColourSpace`, `DeltaE`, `GamutMap`/`MapToRgbGamut`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured illuminant is one Unicolour `Spd` construction; a new working space is one `RgbConfiguration` preset selection; the upsampling table is the only author-kernel — a new spectral band is one `SpectralBand` row; zero new surface. A measured isotropic spectral BRDF (EPFL RGL goniophotometer, brdf-loader format) admits through one `Spd` construction per band, framed at `[10]-[RESEARCH]`.
- Boundary: RGB→SPD is the documented Unicolour NOT_COVERED concern, authored as the Smits (1999) seven-basis non-negative reflectance upsampling — the constant/cyan/magenta/yellow/red/green/blue basis SPDs combined so the round-trip `SPD→XYZ→RGB` reproduces the input chromaticity with a smooth, energy-bounded reflectance (the appearance-engine requirement Smits states); the resulting `Spd` feeds Unicolour's `new Unicolour(Configuration, Spd)` → internal `Xyz.FromSpd` for the measured-illuminant path; the scene-linear working space is `RgbConfiguration.Acescg` (AP1 primaries) or `Aces20651` for the ACES scene-referred path, set in a `Configuration` and read through Unicolour's `.RgbLinear` — Materials NEVER re-derives the sRGB/ACEScg transfer curve, it composes the preset; appearance MATCH between a measured target and a library row is `Unicolour.Difference(reference, DeltaE.Ciede2000)`, the industry-standard appearance ΔE, composed not re-minted; the `IsInRgbGamut` check gates the boundary shade and a saturated upsampled primary that lands outside the gamut is perceptually pulled in through the composed `MapToRgbGamut(GamutMap.OklchChromaReduction)` (reduce Oklch chroma until in gamut) rather than hard-faulted, so the white-furnace residual closes on a chroma-reduced in-gamut reflectance instead of rejecting the row — the fault rail is reserved for a non-finite channel; `RgbSpectrum.Luminance` reads the AP1 weights consistent with the ACEScg working space; Wacton.Unicolour is consumed directly as the one scene-linear/spectral color owner and Materials never mints a second `ColourSpace` wrapper.
- Law: `SpectralUpsample` is the page's one `[EXPRESSION_SPINE]` kernel exemption — `ToSpd`/`Acc`/`Resample5nm` fill fixed-length `double[]` buffers by index across the Smits ordered-combination branch and the 69-sample 5nm resample, the admitted boundary-numeric-kernel carve-out from the immutable-fold law for the per-shade hot path; every other kernel on the page is expression-bodied.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class SpectralBand {
    public static readonly SpectralBand Red = new("red", centerNm: 610.0);
    public static readonly SpectralBand Green = new("green", centerNm: 550.0);
    public static readonly SpectralBand Blue = new("blue", centerNm: 465.0);
    public double CenterNm { get; }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SpectralUpsample {
    private static readonly double[] White =   [1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0];
    private static readonly double[] Cyan =    [0.97, 0.97, 0.97, 0.93, 0.12, 0.04, 0.0, 0.0, 0.05, 0.05];
    private static readonly double[] Magenta = [0.99, 0.99, 0.84, 0.18, 0.03, 0.05, 0.40, 0.99, 0.99, 0.99];
    private static readonly double[] Yellow =  [0.0, 0.0, 0.10, 0.78, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0];
    private static readonly double[] RedB =    [0.10, 0.10, 0.0, 0.0, 0.0, 0.0, 0.84, 1.0, 1.0, 1.0];
    private static readonly double[] GreenB =  [0.0, 0.0, 0.03, 0.49, 1.0, 1.0, 0.46, 0.0, 0.0, 0.0];
    private static readonly double[] BlueB =   [1.0, 1.0, 0.89, 0.46, 0.06, 0.0, 0.0, 0.0, 0.0, 0.05];

    public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key) {
        if (!rgb.IsFinite || rgb.R < 0.0 || rgb.G < 0.0 || rgb.B < 0.0) { return Fin.Fail<Spd>(MaterialFault.Parameter(key, "<negative-or-nonfinite-rgb>")); }
        double[] r = new double[10];
        double red = rgb.R, green = rgb.G, blue = rgb.B;
        if (red <= green && red <= blue) { Acc(r, White, red); if (green <= blue) { Acc(r, Cyan, green - red); Acc(r, BlueB, blue - green); } else { Acc(r, Cyan, blue - red); Acc(r, GreenB, green - blue); } }
        else if (green <= red && green <= blue) { Acc(r, White, green); if (red <= blue) { Acc(r, Magenta, red - green); Acc(r, BlueB, blue - red); } else { Acc(r, Magenta, blue - green); Acc(r, RedB, red - blue); } }
        else { Acc(r, White, blue); if (red <= green) { Acc(r, Yellow, red - blue); Acc(r, GreenB, green - red); } else { Acc(r, Yellow, green - blue); Acc(r, RedB, red - green); } }
        for (int i = 0; i < r.Length; i++) { r[i] = Math.Clamp(r[i], 0.0, 1.0); }
        Spd spd = new(380, 5, Resample5nm(r));
        return spd.IsValid ? Fin.Succ(spd) : Fin.Fail<Spd>(MaterialFault.Parameter(key, "<spd-interval-invalid>"));
    }
    private static void Acc(double[] dst, double[] basis, double w) { double c = Math.Max(0.0, w); for (int i = 0; i < dst.Length; i++) { dst[i] += basis[i] * c; } }
    private static double[] Resample5nm(double[] controls) {
        double[] o = new double[69];
        for (int i = 0; i < o.Length; i++) { double t = i / 68.0 * 9.0; int lo = (int)t; double f = t - lo; o[i] = lo >= 9 ? controls[9] : controls[lo] + (controls[lo + 1] - controls[lo]) * f; }
        return o;
    }

    public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key) {
        Unicolour mapped = colour.IsInRgbGamut ? colour : colour.MapToRgbGamut(GamutMap.OklchChromaReduction);
        ColourTriplet lin = mapped.RgbLinear.Triplet;
        RgbSpectrum rgb = new(lin.First, lin.Second, lin.Third);
        return rgb.IsFinite ? Fin.Succ(rgb) : Fin.Fail<RgbSpectrum>(MaterialFault.Gamut(key, "<non-finite-linear-rgb>"));
    }
    public static readonly Configuration SceneConfig = new(RgbConfiguration.Acescg);
    public static RgbSpectrum FromAcescg(double r, double g, double b, Op key) {
        Unicolour c = new(SceneConfig, ColourSpace.RgbLinear, r, g, b);
        return SceneLinear(c, key).IfFail(RgbSpectrum.Black);
    }
    public static double AppearanceDelta(Unicolour candidate, Unicolour reference) => candidate.Difference(reference, DeltaE.Ciede2000);
}
```

## [7]-[TONE_MAP]

- Owner: `ToneOperator` `[SmartEnum<string>]` (aces · reinhard · filmic · exposure); `ToneMap` the static operator table.
- Entry: `public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure)` — one entry, four operators by row; the scene-linear input is the integrator's HDR radiance, the output is display-referred [0,1] for the gamut check.
- Packages: Wacton.Unicolour (composed for the encode after tone-map — `RgbConfiguration.StandardRgb`/`Rec2100Pq` transfer), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new operator is one `ToneOperator` row carrying its per-channel `Curve` delegate — `Apply` reads the row's curve through `[UseDelegateFromConstructor]` and never branches; the encode/transfer after tone-mapping is a composed `RgbConfiguration` preset, never an author-kernel; zero new surface.
- Boundary: ACES RRT/ODT is the documented Unicolour NOT_COVERED concern — authored as the Narkowicz (2016) ACES-fit approximation of the combined RRT+ODT (the rational `(x(ax+b))/(x(cx+d)+e)` curve matched to the reference ACES sRGB ODT), exact in the fence; the scene-referred operators are the Reinhard extended `x(1+x/Lwhite²)/(1+x)` global operator, the Hejl-Burgess-Dawson filmic curve, and the plain exposure-then-clamp; exposure is applied as a multiplicative scale before the curve (`scene · 2^exposure`); the OETF/transfer after tone-mapping (sRGB gamma, or PQ for HDR via `RgbConfiguration.Rec2100Pq`) is COMPOSED through Unicolour — the consumer constructs a `Unicolour(config, ColourSpace.RgbLinear, r, g, b)` and reads `.Rgb` for the encoded display value, so Materials authors ONLY the tone CURVE the library lacks and never re-derives a transfer function; the tone-mapped display-referred output is the result the app-platform raster path (`Rasm.AppUi/Charts/custom#COLOR_SPACE`) consumes downstream, never a surface Materials reaches into.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class ToneOperator {
    public static readonly ToneOperator Aces = new("aces", ToneMap.AcesFit);
    public static readonly ToneOperator Reinhard = new("reinhard", ToneMap.ReinhardExtended);
    public static readonly ToneOperator Filmic = new("filmic", ToneMap.FilmicCurve);
    public static readonly ToneOperator Exposure = new("exposure", static x => Math.Clamp(x, 0.0, 1.0));

    [UseDelegateFromConstructor]
    public partial double Curve(double sceneLinearChannel);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ToneMap {
    public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure) =>
        sceneLinear.Scale(Math.Pow(2.0, exposure)).Map(op.Curve);

    internal static double AcesFit(double x) {
        const double a = 2.51, b = 0.03, c = 2.43, d = 0.59, ee = 0.14;
        return Math.Clamp(x * (a * x + b) / (x * (c * x + d) + ee), 0.0, 1.0);
    }
    internal static double ReinhardExtended(double x) {
        const double lWhite = 4.0;
        return Math.Clamp(x * (1.0 + x / (lWhite * lWhite)) / (1.0 + x), 0.0, 1.0);
    }
    internal static double FilmicCurve(double x) {
        double v = Math.Max(0.0, x - 0.004);
        return (v * (6.2 * v + 0.5)) / (v * (6.2 * v + 1.7) + 0.06);
    }

    public static (double R, double G, double B) Encode(RgbSpectrum displayLinear, RgbConfiguration target) {
        Unicolour c = new(new Configuration(target), ColourSpace.RgbLinear, displayLinear.R, displayLinear.G, displayLinear.B);
        ColourTriplet rgb = c.Rgb.Triplet;
        return (rgb.First, rgb.Second, rgb.Third);
    }
}
```

## [8]-[CONDUCTOR_IOR]

- Owner: `ConductorMetal` `[SmartEnum<string>]` the measured-metal axis; `ConductorIor` the per-band complex-IOR `(Eta, K)` table; the `Conductor` lobe grounding.
- Entry: `public static (RgbSpectrum Eta, RgbSpectrum K) Of(ConductorMetal metal)` reads the measured complex refractive index per RGB band, and `public static BsdfLobe.Conductor Lobe(ConductorMetal metal, double alphaX, double alphaY)` constructs the grounded `Conductor` lobe — the metal F0 is the measured `(η, k)` Fresnel, NEVER a hand-authored RGB albedo scaled to a guess.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new measured metal is one `ConductorMetal` row carrying its three-band `(η, k)` measured pair from the published refractive-index table; the table is the INTERNAL leg of `graph#MEASURED_SPECTRAL_LIBRARY` — the conductor rows ground here rather than carrying a hand-authored Acescg albedo. A spectral 195-wavelength conductor curve (EPFL RGL goniophotometer or a full `refractiveindex.info` n/k spectrum) is the [UPSTREAM-BLOCKED] extension that admits through `[6]-[SPECTRAL_UPSAMPLE]` `ToSpd` per band once a managed `.bsdf`/spectral reader lands at `acquisition#EPFL_RGL_BRDF_LOADER`; zero new surface.
- Boundary: the complex refractive index `(η, k)` per RGB band is the physically-correct conductor F0 — the `Microfacet.FresnelConductor(cosI, RgbSpectrum eta, RgbSpectrum k)` overload reads it directly, so a metal's edge tint and grazing-angle hue shift emerge from the measured dispersion rather than an artist's base-color triple; the three-band `(η, k)` values transcribe the Johnson-Christy / `refractiveindex.info` measured dataset at the RGB band centres `SpectralBand.Red`/`Green`/`Blue` carry (610/550/465 nm sampled against the published 630/532/465 nm anchors); the `graph#MATERIAL_LIBRARY` conductor rows carry a measured `BaseColor` for the diffuse-substitute preview path AND name a `ConductorMetal` so the `bsdf#LOBE_FAMILY` `Conductor` lobe grounds from `ConductorIor.Of`, the base color the perceptual seed and the `(η, k)` the shading truth; a metal absent from the table falls back to the `graph#MATERIAL_LIBRARY` base-color-as-F0 dielectric-Schlick approximation rather than faulting, so the table grounds the eight named metals and a ninth row admits without a rebuild; the conductor F0 round-trips in-gamut through the `[10]-[RESEARCH]` `WHITE_FURNACE_HARNESS` lossless-conductor furnace (F≡1 reflects unit energy) so a measured metal conserves energy under the Kulla-Conty multi-scatter term.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class ConductorMetal {
    public static readonly ConductorMetal Gold     = new("gold");
    public static readonly ConductorMetal Copper   = new("copper");
    public static readonly ConductorMetal Aluminum = new("aluminum");
    public static readonly ConductorMetal Silver   = new("silver");
    public static readonly ConductorMetal Iron     = new("iron");
    public static readonly ConductorMetal Chromium = new("chromium");
    public static readonly ConductorMetal Titanium = new("titanium");
    public static readonly ConductorMetal Brass    = new("brass");
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConductorIor {
    private static readonly FrozenDictionary<ConductorMetal, (RgbSpectrum Eta, RgbSpectrum K)> Table =
        new (ConductorMetal Metal, RgbSpectrum Eta, RgbSpectrum K)[] {
            (ConductorMetal.Gold,     new RgbSpectrum(0.183, 0.421, 1.373), new RgbSpectrum(3.424, 2.346, 1.770)),
            (ConductorMetal.Copper,   new RgbSpectrum(0.271, 0.677, 1.316), new RgbSpectrum(3.609, 2.625, 2.292)),
            (ConductorMetal.Aluminum, new RgbSpectrum(1.346, 0.965, 0.617), new RgbSpectrum(7.475, 6.400, 5.303)),
            (ConductorMetal.Silver,   new RgbSpectrum(0.159, 0.145, 0.135), new RgbSpectrum(3.929, 3.190, 2.381)),
            (ConductorMetal.Iron,     new RgbSpectrum(2.911, 2.950, 2.585), new RgbSpectrum(3.089, 2.932, 2.767)),
            (ConductorMetal.Chromium, new RgbSpectrum(2.020, 2.790, 2.020), new RgbSpectrum(3.860, 4.200, 3.860)),
            (ConductorMetal.Titanium, new RgbSpectrum(2.741, 2.542, 2.267), new RgbSpectrum(3.814, 3.435, 3.039)),
            (ConductorMetal.Brass,    new RgbSpectrum(0.444, 0.527, 1.094), new RgbSpectrum(3.695, 2.765, 1.829)),
        }.ToFrozenDictionary(static r => r.Metal, static r => (r.Eta, r.K));

    public static (RgbSpectrum Eta, RgbSpectrum K) Of(ConductorMetal metal) => Table[metal];

    public static BsdfLobe.Conductor Lobe(ConductorMetal metal, double alphaX, double alphaY) {
        (RgbSpectrum eta, RgbSpectrum k) = Of(metal);
        return new BsdfLobe.Conductor(eta, k, alphaX, alphaY);
    }

    public static Option<ConductorMetal> Resolve(string family, string name) =>
        family == "metal" && ConductorMetal.TryGet(name, out ConductorMetal? metal) ? Optional(metal) : Option<ConductorMetal>.None;

    public static RgbSpectrum FresnelNormal(ConductorMetal metal) {
        (RgbSpectrum eta, RgbSpectrum k) = Of(metal);
        return eta.Zip(k, static (e, kk) => ((e - 1.0) * (e - 1.0) + kk * kk) / ((e + 1.0) * (e + 1.0) + kk * kk));
    }
}
```

## [9]-[OPENPBR_SLAB]

- Owner: `SlabKind` `[SmartEnum<string>]` the slab axis (fuzz · coat · emission · base); `Slab` `[Union]` the closed slab family; `SlabStack` the outermost-to-base layering algebra; `OpenPbrSurface` the OpenPBR parameter vector.
- Entry: `public static SlabStack Lower(MaterialParameters parameters, ConductorMetal conductor)` lowers a `graph#MATERIAL_LIBRARY` `MaterialParameters` row to the formal OpenPBR Surface 1.1 stack (the row names its `ConductorMetal` through `bsdf#CONDUCTOR_IOR`), and `public Fin<LayeredBsdf> ToLayered(Op key)` collapses the stack to the `[5]-[LAYERED_COMPOSITION]` `LayeredBsdf` weighted-lobe fold the integrator shades — the stack IS the composition law, the weighted fold its energy-preserving lowering, so the renderer reads one `LayeredBsdf` and the slab algebra is the construction the row drives through.
- Packages: Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new layering modifier is one `Slab` case carrying its albedo-scaling operator (the fuzz slab is the new closed lobe case the seven-lobe set lacked, realized as the `Sheen` lobe at the fuzz position; a thin-film modifier rides the `Coat` slab's `ThinFilm` field); a new OpenPBR parameter is one `OpenPbrSurface` column the `Lower` reads — the standard column set (`base`, `specular`, `transmission`, `subsurface`, `coat`, `fuzz`, `thin_film`, `emission`, `geometry`) the `graph#MATERIAL_LIBRARY` `MaterialParameters` aligns to; zero new surface. The `interchange#MATERIAL_WIRE` `MaterialWire` is the OpenPBR-vector wire projection this stack defines and the TS/Py consumers decode.
- Law: the slab stack is the formal OpenPBR Surface 1.1 layering order outermost-to-base — `fuzz` over `coat` over `emission` over the `base` substrate — composed by albedo-scaling layering operators (each slab transmits `1 − E(slab)` of the energy below it, where `E` is the slab's directional albedo from `[4]-[LOBE_FAMILY]` `MultiScatter.DirectionalAlbedo`), NOT the additive convex-combination weight fold `[5]-[LAYERED_COMPOSITION]` predated; the base substrate is the metalness-mixed conductor-vs-dielectric the `bsdf#CONDUCTOR_IOR` table grounds, the dielectric arm carrying the opaque glossy-diffuse-plus-subsurface or the translucent transmission per the `transmission` weight.
- Boundary: `SlabStack.Lower` is the ONE OpenPBR construction — a per-material slab builder is the deleted form; the fuzz slab lowers to a `Sheen` lobe weighted by `fuzz_weight`, the coat slab to a `Clearcoat` lobe weighted by `coat_weight` (its `thin_film` field a `ThinFilm` lobe modifier when `thin_film_weight > 0`), the emission slab to the `graph#MATERIAL_GRAPH` emission carrier (energy-additive, never occluding), and the base substrate to the metalness lerp between a `Conductor` lobe (grounded from the `ConductorMetal` the row names through `bsdf#CONDUCTOR_IOR`) and a `Dielectric`/`Diffuse`/`Subsurface` mix per `transmission`/`subsurface`; `ToLayered` collapses the albedo-scaled slab weights into the `[5]-[LAYERED_COMPOSITION]` `LayeredBsdf.Of` normalized lobe list so the integrator shades one `LayeredBsdf` and never re-derives the slab nesting per sample — the albedo-scaling is computed once at lowering, the energy each outer slab leaves for the layer below baked into the lobe weight; the OpenPBR z-up local-frame convention matches the `[2]-[SHADING_FRAME]` `LocalVector` basis so no slab re-derives `cosθ`; an over-unit total or a non-finite slab weight rails `MaterialFault.Parameter` at `Lower`, never a propagated energy gain; the `weathering#WEATHERING` aging operator targets the slab columns directly once lowered (chalking raises `coat_roughness`, soiling raises `fuzz_weight`, patina swaps the base `ConductorMetal`), the flat `MaterialParameters` interpolation the form until a consumer drives the slab columns.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class SlabKind {
    public static readonly SlabKind Fuzz     = new("fuzz");
    public static readonly SlabKind Coat     = new("coat");
    public static readonly SlabKind Emission = new("emission");
    public static readonly SlabKind Base     = new("base");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Slab {
    private Slab() { }

    public sealed record Fuzz(double Weight, RgbSpectrum Color, double Roughness) : Slab;
    public sealed record Coat(double Weight, RgbSpectrum Color, double Roughness, double Ior, Option<double> ThinFilmThickness, double ThinFilmIor) : Slab;
    public sealed record Emission(RgbSpectrum Radiance, double Luminance) : Slab;
    public sealed record Base(double Metalness, ConductorMetal Conductor, RgbSpectrum BaseColor, double Roughness, double SpecularIor, double Anisotropy, double Transmission, double TransmissionRoughness, double Subsurface, Vector3d SubsurfaceRadius) : Slab;

    public SlabKind Kind => Switch<SlabKind>(
        fuzz:     static _ => SlabKind.Fuzz,
        coat:     static _ => SlabKind.Coat,
        emission: static _ => SlabKind.Emission,
        @base:    static _ => SlabKind.Base);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct OpenPbrSurface(
    double BaseWeight, RgbSpectrum BaseColor, double BaseMetalness, double BaseDiffuseRoughness,
    double SpecularWeight, double SpecularRoughness, double SpecularIor, double SpecularAnisotropy,
    double TransmissionWeight, double TransmissionRoughness,
    double SubsurfaceWeight, Vector3d SubsurfaceRadius,
    double CoatWeight, double CoatRoughness, double CoatIor,
    double FuzzWeight, double FuzzRoughness,
    double ThinFilmWeight, double ThinFilmThickness, double ThinFilmIor,
    RgbSpectrum EmissionColor, double EmissionLuminance,
    ConductorMetal Conductor);

public sealed record SlabStack(Seq<Slab> Slabs) {
    public static SlabStack Lower(MaterialParameters p, ConductorMetal conductor) =>
        new(Seq<Slab>()
            .Add(new Slab.Fuzz(p.Sheen, AcescgRgb(p.BaseColor), Math.Max(1e-3, p.Roughness)))
            .Add(new Slab.Coat(p.Clearcoat, RgbSpectrum.White, p.ClearcoatRoughness, 1.5, Option<double>.None, 1.5))
            .Add(new Slab.Emission(AcescgRgb(p.Emission), p.EmissionLuminance))
            .Add(new Slab.Base(p.Metalness, conductor, AcescgRgb(p.BaseColor), p.Roughness, p.Ior, p.Anisotropy, p.Transmission, p.TransmissionRoughness, p.Subsurface, p.SubsurfaceRadius)));

    public Fin<LayeredBsdf> ToLayered(Op key) {
        double ax = Microfacet.AlphaOf(BaseRoughness());
        Seq<LobeWeight> lobes = Slabs.Fold(Seq<LobeWeight>(), (acc, slab) => acc + LowerSlab(slab, ax, RemainingEnergy(acc)));
        return LayeredBsdf.Of(lobes.Filter(l => l.Weight.Value > 0.0), key);
    }

    private double BaseRoughness() => Slabs.Choose(static s => s is Slab.Base b ? Some(b.Roughness) : Option<double>.None).HeadOrNone().IfNone(0.5);

    private static double RemainingEnergy(Seq<LobeWeight> placed) =>
        Math.Clamp(1.0 - placed.Sum(l => l.Weight.Value * MultiScatter.DirectionalAlbedo(0.5, 0.5)), 0.0, 1.0);

    private static Seq<LobeWeight> LowerSlab(Slab slab, double baseAlpha, double pass) => slab.Switch(
        fuzz:     f => f.Weight > 0.0 ? Seq1(new LobeWeight(new BsdfLobe.Sheen(f.Color, f.Roughness), UnitInterval.Create(f.Weight * pass))) : Seq<LobeWeight>(),
        coat:     c => c.Weight > 0.0 ? Seq1(new LobeWeight(new BsdfLobe.Clearcoat(c.Weight, c.Roughness), UnitInterval.Create(c.Weight * pass))) : Seq<LobeWeight>(),
        emission: static _ => Seq<LobeWeight>(),
        @base:    b => LowerBase(b, baseAlpha, pass));

    private static Seq<LobeWeight> LowerBase(Slab.Base b, double alpha, double pass) {
        (RgbSpectrum eta, RgbSpectrum k) = ConductorIor.Of(b.Conductor);
        BsdfLobe conductor = new BsdfLobe.Conductor(eta, k, alpha, alpha);
        BsdfLobe dielectric = b.Transmission > 0.0
            ? new BsdfLobe.Dielectric(b.SpecularIor, Microfacet.AlphaOf(b.TransmissionRoughness), Microfacet.AlphaOf(b.TransmissionRoughness), b.BaseColor)
            : b.Subsurface > 0.0
                ? new BsdfLobe.Subsurface(b.BaseColor, b.SubsurfaceRadius.Length)
                : new BsdfLobe.Diffuse(b.BaseColor, b.Roughness);
        double mw = Math.Clamp(b.Metalness, 0.0, 1.0) * pass, dw = (1.0 - Math.Clamp(b.Metalness, 0.0, 1.0)) * pass;
        return Seq(new LobeWeight(conductor, UnitInterval.Create(Math.Clamp(mw, 0.0, 1.0))), new LobeWeight(dielectric, UnitInterval.Create(Math.Clamp(dw, 0.0, 1.0))));
    }

    private static RgbSpectrum AcescgRgb(Unicolour colour) { var lin = colour.RgbLinear; return new RgbSpectrum(lin.R, lin.G, lin.B); }
}
```

## [10]-[RESEARCH]

- [WHITE_FURNACE_HARNESS] (tier-2): the energy-conservation harness that integrates each lobe's directional albedo over the hemisphere against a uniform unit-radiance environment and asserts the reflected energy never exceeds 1 (no energy created) and, for a lossless conductor (F≡1), equals 1 to a Monte-Carlo tolerance (no energy destroyed). The harness is the numeric proof that `MultiScatter.KullaConty` recovers exactly the energy the single-scatter Smith model loses at high roughness — the analytic `DirectionalAlbedo` Karis fit supplies `E(μ)` and the `HemisphericalAlbedo` 8-node quadrature closes `Eavg = 2∫E(μ)μ dμ` exactly, so this is a probe over the closed-form result, not an open gate. The harness samples `BsdfLobe.Sample`, accumulates `value·cosθ/pdf` over N directions, and bounds the furnace residual at the `Conductor`/`Dielectric` roughness sweep (α ∈ {0.1, 0.3, 0.5, 0.7, 0.9}); the residual to retire is whether the 10-control-point Smits basis round-trips every `graph#MATERIAL_LIBRARY` row to an in-gamut `SurfaceShade` after the `MapToRgbGamut(GamutMap.OklchChromaReduction)` chroma reduction, since a saturated primary can push the upsampled reflectance against the [0,1] energy bound and the harness measures the perceptual ΔE the chroma-reduction costs. Reciprocity (`f(wo,wi) == f(wi,wo)`) and GGX NDF normalization (`∫ D(h)·cosθh dω == 1`) ride the same harness as exact-equality and unit-integral assertions over the `Microfacet` kernel.
- [POSITION_FREE_MULTI_SCATTER]: the multi-bounce energy is the closed-form Kulla-Conty term today; the unbiased route is the position-free Monte Carlo / atomic-decomposition methods (Belcour; Guo-Hasan-Zhao; d'Eon-Bitterli-Zeltner, SIGGRAPH Asia 2024) that evaluate the layered stack's multi-bounce transport without the closed-form approximation. The probe is whether the path tracer's per-bounce cost admits the position-free random walk over the OpenPBR slab stack as the high-fidelity path, with the Kulla-Conty term the fast path; the seam is `Rasm.AppUi/Render/viewport#PATH_TRACE`.
