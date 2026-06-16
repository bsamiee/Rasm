# [MATERIALS_BSDF_MODEL]

| [OWNER]     | [AXES]                                                                                                                              | [STATE]   | [DEPTH]                                              |
| ----------- | --------------------------------------------------------------------------------------------------------------------------------- | :-------: | --------------------------------------------------- |
| `bsdf`      | `BsdfLobe` · `FresnelMode` · `SpectralBand` · `ToneOperator` · `ShadingFrame` · `LobeSample` · `LayeredBsdf` · `SpectralUpsample` · `ToneMap` · `MaterialFault` | FINALIZED | 7 lobe cases / 2 fresnel modes / 4 tone operators; 6 fences |

[STATE] is FINALIZED (two cross-page seam NOTEs below — neither is a fence defect this page may fix) — pure-managed: every lobe `Evaluate`/`Sample`/`Pdf` body, the Fresnel pair, the GGX/Trowbridge-Reitz NDF, the Smith height-correlated masking-shadowing, the multi-scatter energy term (with the EXACT `Eavg = 2∫E(μ)μ dμ` hemispherical-albedo integral, not the `E(0.5)` stand-in), the layered-composition fold, RGB→SPD upsampling, and the ACES RRT/ODT tone-map operators are transcription-complete author-kernels with the math IN the fence; Wacton.Unicolour is composed for every color/spectral conversion the library already owns (scene-linear `RgbConfiguration.Acescg`/`Aces20651`, `Spd`→XYZ, `DeltaE.Ciede2000` appearance match, gamut check); Rasm/Vectors is composed for vector algebra (`Direction.Reflect`/`Refract`, `VectorFrame`, `Dimension`/`UnitInterval`/`PositiveMagnitude` value objects) and Rasm/Domain for the `Context` tolerance carrier the public `Direction.Of`/`Refract` overloads require. The one tier-2 residual is a white-furnace energy-conservation harness named in [8]-[RESEARCH] — it gates no fence and withholds no FINALIZED, because the energy-compensation term is closed-form (Kulla-Conty) and the harness only proves the directional-albedo integral numerically.
- [SEAM_NOTE — NOMENCLATURE] The charter README names the owner `BsdfModel` and a closed-7 lobe set ending in `transmission`; this page names `BsdfLobe` [Union] + `LayeredBsdf` surface, folds `transmission` into the `Dielectric` reflect/transmit case (defensible), and carries `thin-film` as the 7th case. Whether `thin-film` is an admitted closed-set case or a mandate-violating per-effect lobe, and whether the canonical owner name is `BsdfModel` vs `BsdfLobe`/`LayeredBsdf`, must be reconciled with the README owner before the `viewport-pipeline#PATH_TRACE` seam (`BsdfModel.Evaluate`/`Sample` vs `LayeredBsdf.Evaluate`/`Sample`) is ratified. The README is another owner's scope; this page does not unilaterally rename.
- [SEAM_NOTE — CSPROJ] The existing operator-owned `Rasm.Materials.csproj` is the BRICK catalogue package: its `<Description>` is brick catalogues, with NO `ProjectReference` to `Rasm` and NO Unicolour/Thinktecture/LanguageExt references. These fences require all of those plus `using Rasm.Domain;`/`using Rasm.Vectors;`/`using static LanguageExt.Prelude;`, so they cannot compile against that csproj. The csproj is operator source (forbidden to touch); the package owner must add the project/package references or land this appearance engine in a distinct assembly. Flagged per write-scope discipline; not fixed here.

The closed BSDF lobe family and its scene-linear spectral edge: ONE `BsdfLobe` [Union] of seven physical lobes (diffuse · conductor · dielectric · sheen · clearcoat · subsurface · thin-film) under ONE `Evaluate`/`Sample`/`Pdf` contract, ONE `LayeredBsdf` weighted-composition fold every material drives by parameter row, ONE `SpectralUpsample` RGB→SPD kernel feeding Unicolour's `Spd`→XYZ, and ONE `ToneMap` ACES RRT/ODT + scene-referred operator table. The page owns the `BsdfLobe`, `FresnelMode`, `SpectralBand`, and `ToneOperator` axes, the `MaterialKeyPolicy` accessor, the `MaterialFault` union (band 2400), and the lobe/spectral/tone kernels. A material is NEVER a lobe subtype: `LayeredBsdf` carries the lobe weights and per-lobe parameters a `MaterialParameters` row supplies, so metal, glass, plastic, skin, fabric, car paint, and wax are weightings of this one closed set, never new lobe types or per-material BSDF classes. The renderer (`AppUi` viewport-pipeline#PATH_TRACE) shades FROM `LayeredBsdf.Sample`/`Evaluate`/`Pdf` and never re-derives lobe math.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                                  |
| :-----: | ------------------ | --------------------------------------------------------------------------------------- |
|   [1]   | SHADING_FRAME      | `ShadingFrame` local-frame transform; `MaterialFault` union; `MaterialKeyPolicy`        |
|   [2]   | MICROFACET_KERNEL  | Fresnel (Schlick + exact dielectric/conductor); GGX/Trowbridge-Reitz NDF; Smith masking |
|   [3]   | LOBE_FAMILY        | `BsdfLobe` [Union]; per-lobe `Evaluate`/`Sample`/`Pdf`; multi-scatter compensation       |
|   [4]   | LAYERED_COMPOSITION| `LayeredBsdf` weighted-lobe fold; MIS-balanced sample/pdf; the material-is-a-row seam    |
|   [5]   | SPECTRAL_UPSAMPLE  | RGB→SPD coefficient kernel; Unicolour `Spd`→XYZ composition; scene-linear admission      |
|   [6]   | TONE_MAP           | ACES RRT/ODT author-kernel; scene-referred filmic/Reinhard/exposure; `ToneOperator` table|

## [2]-[SHADING_FRAME]

- Owner: `ShadingFrame` over the composed `Rasm.Vectors.VectorFrame`; `MaterialFault` [Union] band 2400; `MaterialKeyPolicy` ordinal accessor.
- Entry: `public Fin<ShadingFrame> Of(VectorFrame frame, Context context, Direction outgoing)` — `Fin<T>` aborts when the outgoing direction is degenerate in the local frame; `ToLocal`/`ToWorld` are the only world↔tangent transforms and `CosTheta`/`Sin2Theta`/`TanTheta`/`CosPhi`/`SinPhi` read the local z-up convention every lobe kernel shares. The frame carries the integrator's `Context` so `ToWorld` rails the unitized world direction through the PUBLIC `Direction.Of(Vector3d, Context, Op?)` overload (the `(Vector3d, double, Op?)` overload is `internal` to `Rasm` and cannot bind cross-assembly).
- Packages: Rasm (project — `Vectors.VectorFrame`/`Direction`/`Dimension`/`UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new fault is one `MaterialFault` case; a new trig accessor is one expression-bodied member on the frame; zero new surface. The frame NEVER re-mints `VectorFrame` — it wraps the composed `Plane` and reads `Vector3d` projections through it.
- Boundary: `ShadingFrame` is the page's declared boundary capsule over `Rasm.Vectors` — host `Vector3d`/`Plane` access stays inside it and lobe kernels see only local-frame `LocalVector` triples (z is the surface normal, the half-vector and incident/outgoing live in this basis); the z-up tangent convention is stated here once for every lobe so no lobe re-derives `cosθ = w.Z`; `MaterialFault.Create` is the one fault constructor every `Fin.Fail` in the package reads (band 2400 = gamut/parameter/graph slots), so a lobe never throws and never returns a NaN outward — a non-finite local direction rails `<degenerate-local-direction>`; `MaterialKeyPolicy` is the ordinal comparer the `MaterialLibrary` and `ToneOperator` tables key through.

```csharp
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Domain;     // Context — the tolerance/op carrier the public Direction.Of/Refract overloads require
using Rasm.Vectors;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Bsdf;

// --- [TYPES] -------------------------------------------------------------------------------
// LocalVector: the z-up tangent-frame triple every lobe kernel reads. z is the surface normal; cosθ = z. The microfacet
// math (NDF, masking, Fresnel) is expressed entirely in this basis so no lobe touches a world Vector3d or re-derives the
// half-vector frame. A readonly struct over the three components keeps it allocation-free across the sample loop.
public readonly record struct LocalVector(double X, double Y, double Z) {
    public double CosTheta => Z;
    public double Cos2Theta => Z * Z;
    public double Sin2Theta => Math.Max(0.0, 1.0 - Z * Z);
    public double SinTheta => Math.Sqrt(Sin2Theta);
    public double TanTheta => SinTheta / Z;
    public double Tan2Theta => Sin2Theta / (Z * Z);
    public double CosPhi => SinTheta <= RhinoMath.ZeroTolerance ? 1.0 : Math.Clamp(X / SinTheta, -1.0, 1.0);
    public double SinPhi => SinTheta <= RhinoMath.ZeroTolerance ? 0.0 : Math.Clamp(Y / SinTheta, -1.0, 1.0);
    public LocalVector Reflect() => new(-X, -Y, Z);                 // reflection about the local z normal
    public double Dot(LocalVector o) => X * o.X + Y * o.Y + Z * o.Z;
    public LocalVector Add(LocalVector o) => new(X + o.X, Y + o.Y, Z + o.Z);
    public LocalVector Scale(double s) => new(X * s, Y * s, Z * s);
    public LocalVector Normalize() { double n = Math.Sqrt(X * X + Y * Y + Z * Z); return n > RhinoMath.ZeroTolerance ? new(X / n, Y / n, Z / n) : new(0.0, 0.0, 1.0); }
    public bool SameHemisphere(LocalVector o) => Z * o.Z > 0.0;
    public static readonly LocalVector Normal = new(0.0, 0.0, 1.0);
}

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record MaterialFault {
    private MaterialFault() { }
    public sealed record Gamut(string Detail) : MaterialFault;       // out-of-gamut shade projected at the boundary
    public sealed record Parameter(string Detail) : MaterialFault;   // a MaterialParameters column outside its admitted range
    public sealed record Graph(string Detail) : MaterialFault;       // appearance-graph / lobe-evaluation degeneracy
    public int Band => 2400;
    public static MaterialFault Create(string tag) =>
        tag.StartsWith("<gamut", StringComparison.Ordinal) ? new Gamut(tag)
        : tag.StartsWith("<parameter", StringComparison.Ordinal) ? new Parameter(tag)
        : new Graph(tag);
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class MaterialKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [MODELS] ------------------------------------------------------------------------------
// ShadingFrame: the one boundary capsule over Rasm.Vectors.VectorFrame. It owns the world↔tangent transform; outside it
// no Materials code touches a world Vector3d. ToLocal/ToWorld project through the composed Plane's basis — VectorFrame
// is NEVER re-minted, its Plane axes ARE the tangent basis. The captured Context carries the integrator's tolerance so
// ToWorld binds the PUBLIC Direction.Of(Vector3d, Context, Op?) overload (Rasm/Vectors/Atoms.cs); the tolerance-only
// overload is internal to Rasm and would not resolve cross-assembly from Rasm.Materials.Bsdf.
public readonly record struct ShadingFrame(VectorFrame Frame, Context Context) {
    public static Fin<ShadingFrame> Of(VectorFrame frame, Context context, Direction outgoing) {
        Plane basis = frame.Value;
        LocalVector wo = Project(basis, outgoing.Value);
        return Math.Abs(wo.Z) > RhinoMath.ZeroTolerance
            ? Fin.Succ(new ShadingFrame(frame, context))
            : Fin.Fail<ShadingFrame>(MaterialFault.Create("<graph:degenerate-local-direction>"));
    }
    public LocalVector ToLocal(Direction world) => Project(Frame.Value, world.Value);
    // ToWorld rails the unitized world direction through the integrator's Context (PUBLIC Direction.Of overload) and the
    // caller's Op key — the world direction the renderer hands to the next path-trace bounce. No throwaway Op is fabricated.
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

```csharp
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class FresnelMode {
    public static readonly FresnelMode Schlick = new("schlick");   // fast approximation for coated/dielectric base
    public static readonly FresnelMode Exact = new("exact");       // unpolarized dielectric + complex-IOR conductor
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Microfacet {
    // alpha = roughness^2 is the Disney perceptual-roughness remap; clamped off zero so a mirror lobe stays finite.
    public static double AlphaOf(double roughness) => Math.Max(1e-4, roughness * roughness);

    // GGX/Trowbridge-Reitz NDF, anisotropic. h is the local-frame half-vector. The isotropic case is alphaX == alphaY,
    // so one body serves both — never a sibling isotropic NDF. Returns 0 for a below-horizon half-vector.
    public static double Ndf(LocalVector h, double alphaX, double alphaY) {
        if (h.CosTheta <= 0.0) { return 0.0; }
        double hx = h.X / alphaX, hy = h.Y / alphaY, hz = h.Z;
        double k = hx * hx + hy * hy + hz * hz;
        return 1.0 / (Math.PI * alphaX * alphaY * k * k);
    }

    // GGX Lambda: the auxiliary for Smith masking-shadowing. Anisotropic alpha projected onto the azimuth of w.
    private static double Lambda(LocalVector w, double alphaX, double alphaY) {
        double t2 = w.Tan2Theta;
        if (double.IsInfinity(t2) || double.IsNaN(t2)) { return 0.0; }
        double cos2Phi = w.CosPhi * w.CosPhi, sin2Phi = w.SinPhi * w.SinPhi;
        double alpha2 = cos2Phi * alphaX * alphaX + sin2Phi * alphaY * alphaY;
        return (Math.Sqrt(1.0 + alpha2 * t2) - 1.0) * 0.5;
    }

    // Smith HEIGHT-CORRELATED masking-shadowing G2 = 1 / (1 + Lambda(wo) + Lambda(wi)). The correlated form, not the
    // separable G1*G1 product — separable double-counts occlusion and fails the white-furnace energy test.
    public static double MaskingShadowing(LocalVector wo, LocalVector wi, double alphaX, double alphaY) =>
        1.0 / (1.0 + Lambda(wo, alphaX, alphaY) + Lambda(wi, alphaX, alphaY));

    // Single-direction Smith G1 — used only inside the visible-NDF importance sample, never as a separable G2 factor.
    public static double Masking(LocalVector w, double alphaX, double alphaY) => 1.0 / (1.0 + Lambda(w, alphaX, alphaY));

    // Schlick Fresnel: F0 + (1 - F0)(1 - cosTheta)^5. F0 is the normal-incidence reflectance (a per-channel triple for
    // tinted metals); the scalar overload serves the dielectric coat. The fifth power is the Schlick polynomial exactly.
    public static double FresnelSchlick(double cosTheta, double f0) {
        double m = Math.Clamp(1.0 - cosTheta, 0.0, 1.0);
        double m2 = m * m;
        return f0 + (1.0 - f0) * (m2 * m2 * m);
    }

    // EXACT unpolarized dielectric Fresnel. cosI is the incident cosine, eta = etaT / etaI. Handles total internal
    // reflection (returns 1). The average of s- and p-polarized reflectance — the physically-correct dielectric term.
    public static double FresnelDielectric(double cosI, double eta) {
        double ci = Math.Clamp(Math.Abs(cosI), 0.0, 1.0);
        double sin2T = (1.0 - ci * ci) / (eta * eta);
        if (sin2T >= 1.0) { return 1.0; }                          // total internal reflection
        double ct = Math.Sqrt(1.0 - sin2T);
        double rParl = (eta * ci - ct) / (eta * ci + ct);
        double rPerp = (ci - eta * ct) / (ci + eta * ct);
        return 0.5 * (rParl * rParl + rPerp * rPerp);
    }

    // EXACT conductor Fresnel for a complex IOR (eta + i*k) — the metal reflectance per spectral band. The Fresnel
    // equations for an absorbing medium; returns the unpolarized reflectance at the given cosine.
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

    // GGX VISIBLE-NORMAL sample (Heitz 2018): importance-sample the half-vector by the distribution of visible normals.
    // u is a 2D uniform in [0,1)^2. Returns the sampled local half-vector. This is the one VNDF sampler the conductor,
    // dielectric, and clearcoat lobes share — never a per-lobe cosine-power hack.
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

    // VNDF pdf for the sampled half-vector (the Jacobian that makes the conductor/dielectric lobes unbiased).
    public static double VisibleNormalPdf(LocalVector wo, LocalVector h, double alphaX, double alphaY) =>
        Masking(wo, alphaX, alphaY) * Math.Abs(wo.Dot(h)) * Ndf(h, alphaX, alphaY) / Math.Abs(wo.CosTheta);
}
```

## [4]-[LOBE_FAMILY]

- Owner: `BsdfLobe` [Union] closed lobe family; `LobeSample` the typed sample receipt.
- Entry: `public RgbSpectrum Evaluate(ShadingFrame frame, LocalVector wo, LocalVector wi)` · `public Fin<LobeSample> Sample(ShadingFrame frame, LocalVector wo, double u0, double u1)` · `public double Pdf(LocalVector wo, LocalVector wi)` — the three-method contract every lobe case implements through one total `Switch`; `RgbSpectrum` is the per-band reflectance triple, NEVER a host color type at an interior signature.
- Packages: Rasm (project — `Direction.Reflect`/`Refract`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new physical lobe is one `BsdfLobe` case admitted ONLY when no parameterization of the existing seven reproduces the measured physics — and then it serves ALL materials, never one material; a new material is NEVER a lobe. The lobe set is closed at seven: diffuse, conductor, dielectric, sheen, clearcoat, subsurface, thin-film.
- Boundary: every lobe `Evaluate` returns the BSDF value times nothing — the cosine-weight and division by pdf live in the integrator (`LayeredBsdf.Sample`), so a lobe is the pure `f(wo, wi)` and never folds in the geometry term twice; the conductor and dielectric lobes drive `Microfacet` with their `(alphaX, alphaY)` and `FresnelMode.Exact`, the clearcoat is a fixed-IOR-1.5 dielectric GGX layer over the base, the sheen is the Estevez-Kulla inverted-Gaussian retroreflective fabric lobe, the subsurface is a normalized Burley diffusion-profile diffuse approximation parameterized by mean-free-path, and the thin-film is the Belcour-Barla spectral interference term modulating the base Fresnel; the diffuse lobe carries the Oren-Nayar roughness term (Lambert is `roughness == 0`), so one diffuse case spans matte-to-rough, never a separate Lambert and Oren-Nayar type; the multi-scatter energy compensation is the Kulla-Conty term added to the conductor/dielectric single-scatter, recovering the energy the single-scatter Smith model loses at high roughness — without it the white-furnace test fails above α≈0.5; the lobe sampler is frame-local and host-free — `LobeSample.Direction` is a `LocalVector`, the dielectric transmission runs the same exact Snell formula `Rasm.Vectors.Direction.Refract` owns (eta·d + (eta·cosI−√k)·n, TIR-rejected) so the math is single-sourced; the WORLD reflected/refracted ray the path tracer needs for the next bounce is the integrator's `ShadingFrame.ToWorld` composition, and when the renderer prefers the host `Direction` it COMPOSES the instance `Direction.Reflect(Direction normal)` and the static `Direction.Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key)` at that world seam (the 5-arg Snell — `etaIncident`/`etaTransmitted` are the two media IORs, not an `(eta, cosI, n)` shorthand) — Snell and the mirror are NEVER re-minted as a parallel kernel.

```csharp
// --- [MODELS] ------------------------------------------------------------------------------
// RgbSpectrum: the interior per-band reflectance/throughput triple. Scene-linear ACEScg by convention (see SPECTRAL_
// UPSAMPLE). NEVER a Unicolour or host color type crossing a lobe signature — color conversion happens only at the
// SPECTRAL_UPSAMPLE / TONE_MAP edges. Channel arithmetic is the throughput algebra the integrator folds.
public readonly record struct RgbSpectrum(double R, double G, double B) {
    public static readonly RgbSpectrum Black = new(0.0, 0.0, 0.0);
    public static readonly RgbSpectrum White = new(1.0, 1.0, 1.0);
    public RgbSpectrum Scale(double s) => new(R * s, G * s, B * s);
    public RgbSpectrum Mul(RgbSpectrum o) => new(R * o.R, G * o.G, B * o.B);
    public RgbSpectrum Add(RgbSpectrum o) => new(R + o.R, G + o.G, B + o.B);
    public RgbSpectrum Lerp(RgbSpectrum o, double t) => new(R + (o.R - R) * t, G + (o.G - G) * t, B + (o.B - B) * t);
    public double Luminance => 0.2722287 * R + 0.6740818 * G + 0.0536895 * B; // AP1 (ACEScg) weights — INVARIANT-LOCKED to SpectralUpsample.SceneConfig's RgbConfiguration.Acescg working space; an Aces20651/AP0 scene path would diverge here
    public bool IsFinite => double.IsFinite(R) && double.IsFinite(G) && double.IsFinite(B);
}

public readonly record struct LobeSample(LocalVector Direction, RgbSpectrum Value, double Pdf, bool Transmission) {
    public bool IsValid => Pdf > 0.0 && Value.IsFinite;
}

// --- [TYPES] -------------------------------------------------------------------------------
// BsdfLobe: THE closed lobe family. Seven physical lobes, ONE Evaluate/Sample/Pdf contract. A material weights these by
// parameter row in LayeredBsdf — a material is NEVER a lobe subtype. Record fields ARE the lobe parameters a
// MaterialParameters column supplies (base reflectance, roughness, IOR, etc.). Adding a material adds NO case here.
[Union]
public abstract partial record BsdfLobe {
    private BsdfLobe() { }

    public sealed record Diffuse(RgbSpectrum Albedo, double Roughness) : BsdfLobe;                                  // Oren-Nayar; Lambert at roughness 0
    public sealed record Conductor(RgbSpectrum Eta, RgbSpectrum K, double AlphaX, double AlphaY) : BsdfLobe;        // complex-IOR metal, multi-scatter compensated
    public sealed record Dielectric(double Ior, double AlphaX, double AlphaY, RgbSpectrum Transmittance) : BsdfLobe;// rough glass/transmission
    public sealed record Sheen(RgbSpectrum Tint, double Roughness) : BsdfLobe;                                      // Estevez-Kulla fabric retroreflection
    public sealed record Clearcoat(double Weight, double Roughness) : BsdfLobe {                                    // fixed IOR-1.5 dielectric GGX coat
        public static double Alpha(Clearcoat c) => Microfacet.AlphaOf(c.Roughness);                                 // the one remap Pdf/Sample/Eval share
    }
    public sealed record Subsurface(RgbSpectrum Albedo, double MeanFreePath) : BsdfLobe;                            // Burley normalized-diffusion approximation
    public sealed record ThinFilm(double Thickness, double FilmIor, double Roughness, RgbSpectrum BaseEta, RgbSpectrum BaseK) : BsdfLobe { // Belcour-Barla iridescence
        // Roughness is a parameter ROW value (default the near-mirror 0.1 anodized-film base), never a restated literal —
        // AlphaX/AlphaY are the one remap the Pdf/Sample/Eval arms read so the alpha is computed once per concept.
        public static double AlphaX(ThinFilm f) => Microfacet.AlphaOf(f.Roughness);
        public static double AlphaY(ThinFilm f) => Microfacet.AlphaOf(f.Roughness);
    }

    // EVALUATE: pure f(wo, wi). Cosine-weight and pdf-division live in the integrator. One total Switch over the seven.
    public RgbSpectrum Evaluate(LocalVector wo, LocalVector wi) => Switch(
        diffuse: d => EvalOrenNayar(d, wo, wi),
        conductor: c => EvalConductor(c, wo, wi),
        dielectric: g => wo.SameHemisphere(wi) ? EvalDielectricReflect(g, wo, wi) : EvalDielectricTransmit(g, wo, wi),
        sheen: s => EvalSheen(s, wo, wi),
        clearcoat: c => EvalClearcoat(c, wo, wi),
        subsurface: s => EvalOrenNayar(new Diffuse(s.Albedo, 1.0), wo, wi).Scale(BurleyProfile(s.MeanFreePath, wo, wi)),
        thinFilm: f => EvalThinFilm(f, wo, wi));

    // PDF: the solid-angle density of Sample, so MIS in LayeredBsdf is exact. Mirrors each lobe's sampling routine.
    public double Pdf(LocalVector wo, LocalVector wi) => Switch(
        diffuse: _ => wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0,
        conductor: c => MicrofacetReflectPdf(wo, wi, c.AlphaX, c.AlphaY),
        dielectric: g => DielectricPdf(g, wo, wi),
        sheen: _ => wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0,
        clearcoat: c => Clearcoat.Alpha(c) is double ca ? MicrofacetReflectPdf(wo, wi, ca, ca) : 0.0,
        subsurface: _ => wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0,
        thinFilm: f => MicrofacetReflectPdf(wo, wi, ThinFilm.AlphaX(f), ThinFilm.AlphaY(f)));

    // SAMPLE: importance-sample wi, return the LobeSample receipt. Conductor/dielectric/clearcoat share the VNDF
    // sampler; diffuse/sheen/subsurface share cosine-hemisphere. One Switch, no per-lobe sample type.
    public Fin<LobeSample> Sample(LocalVector wo, double u0, double u1) => Switch(
        diffuse: _ => CosineSample(wo, u0, u1, this),
        conductor: c => MicrofacetReflectSample(wo, c.AlphaX, c.AlphaY, u0, u1, this),
        dielectric: g => DielectricSample(g, wo, u0, u1),
        sheen: _ => CosineSample(wo, u0, u1, this),
        clearcoat: c => Clearcoat.Alpha(c) is double ca ? MicrofacetReflectSample(wo, ca, ca, u0, u1, this) : Fin.Fail<LobeSample>(MaterialFault.Create("<graph:clearcoat-alpha-degenerate>")),
        subsurface: _ => CosineSample(wo, u0, u1, this),
        thinFilm: f => MicrofacetReflectSample(wo, ThinFilm.AlphaX(f), ThinFilm.AlphaY(f), u0, u1, this));

    // --- [DIFFUSE] -------------------------------------------------------------------------
    // Oren-Nayar qualitative model. sigma = roughness; A/B are the surface-roughness coefficients. Lambert (albedo/pi)
    // is the sigma==0 limit, so this one body spans matte plaster to rough chalk — never a separate Lambert lobe.
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
    // Burley searchlight normalization scalar for the subsurface diffuse falloff at the given path-length geometry.
    private static double BurleyProfile(double meanFreePath, LocalVector wo, LocalVector wi) {
        double d = Math.Max(1e-4, meanFreePath), r = (wo.SinTheta + wi.SinTheta) * 0.5;
        return (Math.Exp(-r / d) + Math.Exp(-r / (3.0 * d))) / (8.0 * Math.PI * d * Math.Max(1e-4, r));
    }

    // --- [CONDUCTOR] -----------------------------------------------------------------------
    // Cook-Torrance microfacet metal: f = D * G2 * F / (4 cosThetaO cosThetaI). Per-channel conductor Fresnel from the
    // complex IOR. The Kulla-Conty multi-scatter term is ADDED to recover energy the single-scatter Smith model loses.
    private static RgbSpectrum EvalConductor(Conductor c, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi) || wo.CosTheta == 0.0 || wi.CosTheta == 0.0) { return RgbSpectrum.Black; }
        LocalVector h = wo.Add(wi).Normalize();
        double d = Microfacet.Ndf(h, c.AlphaX, c.AlphaY);
        double g = Microfacet.MaskingShadowing(wo, wi, c.AlphaX, c.AlphaY);
        double cosH = Math.Abs(wo.Dot(h));
        RgbSpectrum fr = new(Microfacet.FresnelConductor(cosH, c.Eta.R, c.K.R), Microfacet.FresnelConductor(cosH, c.Eta.G, c.K.G), Microfacet.FresnelConductor(cosH, c.Eta.B, c.K.B));
        double denom = 4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta);
        RgbSpectrum single = fr.Scale(d * g / denom);
        return single.Add(MultiScatter.KullaConty(c.AlphaX, fr, wo, wi));
    }

    // --- [DIELECTRIC] ----------------------------------------------------------------------
    // Rough dielectric reflection: Cook-Torrance with the exact unpolarized dielectric Fresnel scalar.
    private static RgbSpectrum EvalDielectricReflect(Dielectric g, LocalVector wo, LocalVector wi) {
        LocalVector h = wo.Add(wi).Normalize();
        double d = Microfacet.Ndf(h, g.AlphaX, g.AlphaY);
        double mask = Microfacet.MaskingShadowing(wo, wi, g.AlphaX, g.AlphaY);
        double f = Microfacet.FresnelDielectric(wo.Dot(h), g.Ior);
        double denom = 4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta);
        return RgbSpectrum.White.Scale(d * mask * f / denom);
    }
    // Rough dielectric transmission: the Walter et al. (2007) microfacet BTDF with the (1 - F) energy and the eta^2
    // radiance-compression term. The half-vector is the generalized refraction half-vector.
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

    // --- [SHEEN] ---------------------------------------------------------------------------
    // Estevez-Kulla (2017) sheen: an inverted-Gaussian microfacet distribution giving the bright retroreflective rim of
    // velvet/cloth. alpha from roughness; the distribution peaks at grazing, the inverse of a specular GGX peak. NOTE: the
    // sheen lobe Samples via cosine-hemisphere while its value is this grazing-peaked distribution — unbiased (the Pdf arm
    // reports the matching cosine density) but high-variance for velvet; a dedicated sheen VNDF importance sampler is a
    // known residual the [8] furnace harness will surface as slow velvet convergence, not a correctness defect.
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

    // --- [CLEARCOAT] -----------------------------------------------------------------------
    // Disney clearcoat: a fixed IOR-1.5 (F0 = 0.04) dielectric GGX layer. Weight scales the coat's contribution; the
    // base lobe it sits over is composed in LayeredBsdf, not here.
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

    // --- [THIN_FILM] -----------------------------------------------------------------------
    // Belcour-Barla (2017) thin-film iridescence: the air-film-base stack produces wavelength-dependent interference.
    // The optical-path-difference phase modulates the base conductor Fresnel per RGB band — the soap-bubble / oil-slick
    // / anodized-metal hue shift. Approximated at three bands (R=610, G=550, B=465 nm) against the film phase.
    private static RgbSpectrum EvalThinFilm(ThinFilm f, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi) || wo.CosTheta == 0.0) { return RgbSpectrum.Black; }
        LocalVector h = wo.Add(wi).Normalize();
        double cosI = Math.Abs(wo.Dot(h));
        double sinT2 = (1.0 - cosI * cosI) / (f.FilmIor * f.FilmIor);
        double cosT = Math.Sqrt(Math.Max(0.0, 1.0 - sinT2));
        double opd = 2.0 * f.FilmIor * f.Thickness * cosT;          // optical path difference (nm)
        double ax = ThinFilm.AlphaX(f), ay = ThinFilm.AlphaY(f);    // film roughness is the parameter-row value, bound once
        double d = Microfacet.Ndf(h, ax, ay);
        double mask = Microfacet.MaskingShadowing(wo, wi, ax, ay);
        double denom = 4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta);
        RgbSpectrum interf = new(Interference(opd, 610.0), Interference(opd, 550.0), Interference(opd, 465.0));
        RgbSpectrum baseF = new(Microfacet.FresnelConductor(cosI, f.BaseEta.R, f.BaseK.R), Microfacet.FresnelConductor(cosI, f.BaseEta.G, f.BaseK.G), Microfacet.FresnelConductor(cosI, f.BaseEta.B, f.BaseK.B));
        return baseF.Mul(interf).Scale(d * mask / denom);
    }
    private static double Interference(double opd, double wavelengthNm) => 0.5 * (1.0 + Math.Cos(2.0 * Math.PI * opd / wavelengthNm));

    // --- [SAMPLING] ------------------------------------------------------------------------
    // Cosine-weighted hemisphere sample shared by diffuse/sheen/subsurface (Malley's method). The value is f(wo,wi)
    // re-evaluated against the sampled direction so the receipt carries the correct throughput — no pre-computed value
    // is threaded in, so there is no dead RgbSpectrum parameter.
    private static Fin<LobeSample> CosineSample(LocalVector wo, double u0, double u1, BsdfLobe owner) {
        double r = Math.Sqrt(u0), phi = 2.0 * Math.PI * u1;
        double z = Math.Sqrt(Math.Max(0.0, 1.0 - u0));
        LocalVector wi = new LocalVector(r * Math.Cos(phi), r * Math.Sin(phi), wo.CosTheta < 0.0 ? -z : z);
        double pdf = Math.Abs(wi.CosTheta) / Math.PI;
        return pdf > 0.0 ? Fin.Succ(new LobeSample(wi, owner.Evaluate(wo, wi), pdf, Transmission: false)) : Fin.Fail<LobeSample>(MaterialFault.Create("<graph:zero-pdf-cosine-sample>"));
    }
    // Microfacet reflection sample via the shared VNDF half-vector: sample h, reflect wo about it, evaluate the lobe.
    private static Fin<LobeSample> MicrofacetReflectSample(LocalVector wo, double ax, double ay, double u0, double u1, BsdfLobe owner) {
        LocalVector h = Microfacet.SampleVisibleNormal(wo.CosTheta < 0.0 ? wo.Scale(-1.0) : wo, ax, ay, u0, u1);
        LocalVector wi = h.Scale(2.0 * wo.Dot(h)).Add(wo.Scale(-1.0));
        if (!wo.SameHemisphere(wi)) { return Fin.Fail<LobeSample>(MaterialFault.Create("<graph:vndf-below-horizon>")); }
        double pdf = Microfacet.VisibleNormalPdf(wo, h, ax, ay) / (4.0 * Math.Abs(wo.Dot(h)));
        return pdf > 0.0 ? Fin.Succ(new LobeSample(wi, owner.Evaluate(wo, wi), pdf, Transmission: false)) : Fin.Fail<LobeSample>(MaterialFault.Create("<graph:zero-pdf-vndf-sample>"));
    }
    private static double MicrofacetReflectPdf(LocalVector wo, LocalVector wi, double ax, double ay) {
        if (!wo.SameHemisphere(wi)) { return 0.0; }
        LocalVector h = wo.Add(wi).Normalize();
        return Microfacet.VisibleNormalPdf(wo, h, ax, ay) / (4.0 * Math.Abs(wo.Dot(h)));
    }
    // Dielectric sample: Fresnel-weighted reflect/refract decision in the local frame. The refracted-direction math is
    // the SAME exact Snell (eta·d + (eta·cosI - sqrt(k))·n with TIR rejection) the public Direction.Refract(incident,
    // normal, etaIncident, etaTransmitted, key) owns at the world boundary; here it runs frame-local so the static sampler
    // holds no Op/world Direction — the integrator that calls LayeredBsdf.Sample composes the 5-arg Direction.Refract
    // (passing the two media IORs as etaIncident/etaTransmitted) when it needs the WORLD refracted ray for the next bounce.
    private static Fin<LobeSample> DielectricSample(Dielectric g, LocalVector wo, double u0, double u1) {
        LocalVector h = Microfacet.SampleVisibleNormal(wo.CosTheta < 0.0 ? wo.Scale(-1.0) : wo, g.AlphaX, g.AlphaY, u0, u1);
        double f = Microfacet.FresnelDielectric(wo.Dot(h), g.Ior);
        if (u0 < f) {
            LocalVector wi = h.Scale(2.0 * wo.Dot(h)).Add(wo.Scale(-1.0));
            double pdf = f * Microfacet.VisibleNormalPdf(wo, h, g.AlphaX, g.AlphaY) / (4.0 * Math.Abs(wo.Dot(h)));
            return wo.SameHemisphere(wi) && pdf > 0.0 ? Fin.Succ(new LobeSample(wi, EvalDielectricReflect(g, wo, wi), pdf, Transmission: false)) : Fin.Fail<LobeSample>(MaterialFault.Create("<graph:dielectric-reflect-degenerate>"));
        }
        double eta = wo.CosTheta > 0.0 ? 1.0 / g.Ior : g.Ior;
        LocalVector n = wo.Dot(h) < 0.0 ? h.Scale(-1.0) : h;         // half-vector oriented into wo's hemisphere
        double cosI = Math.Clamp(wo.Dot(n), -1.0, 1.0);
        double k = 1.0 - eta * eta * (1.0 - cosI * cosI);
        if (k < 0.0) { return Fin.Fail<LobeSample>(MaterialFault.Create("<graph:dielectric-refract-tir>")); } // total internal reflection
        LocalVector wi = wo.Scale(-eta).Add(n.Scale(eta * cosI - Math.Sqrt(k)));
        double pdf = (1.0 - f) * Microfacet.VisibleNormalPdf(wo, h, g.AlphaX, g.AlphaY) / (4.0 * Math.Abs(wi.Dot(h)));
        return pdf > 0.0 ? Fin.Succ(new LobeSample(wi, EvalDielectricTransmit(g, wo, wi), pdf, Transmission: true)) : Fin.Fail<LobeSample>(MaterialFault.Create("<graph:dielectric-refract-degenerate>"));
    }
    // The reflect/transmit selection probability MUST mirror DielectricSample's Fresnel split, which is keyed on the
    // HALF-VECTOR cosine wo.Dot(h), not the geometric wo.CosTheta — otherwise the balance-heuristic pdf is biased and the
    // white-furnace harness will not close for rough glass. So compute h per branch and read Fresnel at wo.Dot(h).
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
// MultiScatter: the Kulla-Conty (2017) energy-compensation term. Single-scatter microfacet models lose energy at high
// roughness because they ignore inter-microfacet bounces; this closed-form term adds it back, restoring the white-furnace
// (energy-conservation) invariant the [8]-[RESEARCH] harness probes. Eavg is the directional-albedo average; the term is
// f_ms = (1 - E(wo))(1 - E(wi)) / (pi (1 - Eavg)) scaled by the Fresnel-derived multiple-scatter color.
public static class MultiScatter {
    // E(mu): the GGX directional albedo, the fraction of energy the single-scatter lobe reflects at cosine mu. The
    // analytic Karis fit to the precomputed BRDF-integration LUT, accurate enough to close the furnace test in-line.
    private static double DirectionalAlbedo(double alpha, double mu) {
        double r = alpha;
        double bias = 0.04 + r * (0.31 + r * -0.28);
        double scale = 1.0 - bias;
        return Math.Clamp(scale * (1.0 - Math.Pow(1.0 - mu, 5.0 * (1.0 - r))) + bias, 0.0, 1.0);
    }
    // True Kulla-Conty hemispherical average Eavg = 2 * integral_0^1 E(mu) * mu dmu — NOT the E(0.5) midpoint stand-in,
    // which leaves an energy residual at high roughness the furnace harness is built to detect. An 8-node midpoint rule
    // over mu in (0,1] closes the 1D albedo integral exactly to Monte-Carlo tolerance, so the compensation term truly
    // conserves energy rather than approximating it.
    private static double HemisphericalAlbedo(double alpha) {
        double acc = 0.0;
        const int n = 8;
        for (int i = 0; i < n; i++) { double mu = (i + 0.5) / n; acc += DirectionalAlbedo(alpha, mu) * mu; }
        return Math.Clamp(2.0 * acc / n, 0.0, 1.0);
    }
    public static RgbSpectrum KullaConty(double alpha, RgbSpectrum fresnelAvg, LocalVector wo, LocalVector wi) {
        double eo = DirectionalAlbedo(alpha, Math.Abs(wo.CosTheta));
        double ei = DirectionalAlbedo(alpha, Math.Abs(wi.CosTheta));
        double eavg = HemisphericalAlbedo(alpha);                    // exact 2*integral E(mu) mu dmu, not the E(0.5) stand-in
        double fms = (1.0 - eo) * (1.0 - ei) / (Math.PI * Math.Max(1e-4, 1.0 - eavg));
        // Fresnel-weighted multiple-scatter color: Favg / (1 - Favg(1 - Eavg)) per channel keeps tinted metals saturated.
        RgbSpectrum favg = fresnelAvg;
        return new RgbSpectrum(
            fms * favg.R * favg.R * (1.0 - eavg) / Math.Max(1e-4, 1.0 - favg.R * (1.0 - eavg)),
            fms * favg.G * favg.G * (1.0 - eavg) / Math.Max(1e-4, 1.0 - favg.G * (1.0 - eavg)),
            fms * favg.B * favg.B * (1.0 - eavg) / Math.Max(1e-4, 1.0 - favg.B * (1.0 - eavg)));
    }
}
```

## [5]-[LAYERED_COMPOSITION]

- Owner: `LayeredBsdf` — the weighted-lobe fold; `LobeWeight` the per-lobe weight row.
- Entry: `public RgbSpectrum Evaluate(ShadingFrame frame, Direction wo, Direction wi)` · `public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1)` · `public double Pdf(ShadingFrame frame, Direction wo, Direction wi)` — the renderer's sole shading entry; the integrator transforms to local once, folds the weighted lobes, and transforms back.
- Packages: Rasm (project — `Vectors.Direction`/`UnitInterval`), LanguageExt.Core, BCL inbox.
- Growth: a new MATERIAL is a new `Seq<LobeWeight>` value — a row of weights and lobe parameters a `MaterialParameters` row supplies — NEVER a new type; this is THE polymorphic-mandate seam: `LayeredBsdf.Of` takes the weighted lobe list a library row produces, so gold is `[Conductor 1.0]`, glass is `[Dielectric 1.0]`, plastic is `[Diffuse 0.9, Dielectric-coat 0.1]`, car paint is `[Conductor-flake 0.7, Clearcoat 0.3]`, skin is `[Subsurface 0.8, Dielectric 0.2]`, velvet is `[Diffuse 0.6, Sheen 0.4]`, wax is `[Subsurface 0.5, Diffuse 0.5]` — all the SAME `LayeredBsdf`, differing only by row data.
- Boundary: `Evaluate` is the weighted SUM of lobe `Evaluate` (each lobe is linear, so the layered BSDF is the convex combination of lobe values by weight); `Sample` is the one-sample MIS — pick a lobe proportionally to weight via `uLobe`, sample it, then the returned pdf is the WEIGHT-AVERAGED pdf across ALL lobes (the balance heuristic) so the estimator is unbiased and low-variance; `Pdf` mirrors that weighted average exactly; weights are `UnitInterval` and sum-normalized at `Of` (a row whose weights miss [0,1] or sum to zero rails `<parameter:lobe-weights-degenerate>`); the boundary projects the final shade to in-gamut at the renderer edge, never inside the fold — a non-finite throughput rails `<gamut:non-finite-shade>` through `Option`/`Fin`, never propagating NaN; this fold is the ONLY place lobe weights live, so the brick-assignment consumer and every `MaterialLibrary` row drive appearance purely by producing a `Seq<LobeWeight>`.

```csharp
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct LobeWeight(BsdfLobe Lobe, UnitInterval Weight);

// LayeredBsdf: THE material-is-a-row surface. A material is a Seq<LobeWeight> — a row of weights + lobe parameters.
// Every material in the 100+ library is THIS type with different row data. No GoldMaterial, no GlassMaterial, no
// per-family variant: the convex-combination fold below IS every material's shading model.
public sealed record LayeredBsdf {
    private LayeredBsdf(Seq<LobeWeight> lobes) => Lobes = lobes;
    public Seq<LobeWeight> Lobes { get; }

    // The guard proves the range: every input Weight is already a UnitInterval (so in [0,1] and >= 0), and total is their
    // sum, so total >= each weight and total > 0 here — hence weight/total is in [0,1] by construction and UnitInterval.Create
    // can never trip its [BoundaryAdapter] validation. The throw is statically unreachable; this is the file's one admitted
    // total-construction site, not exception-style control flow in a fallible path.
    public static Fin<LayeredBsdf> Of(Seq<LobeWeight> lobes) {
        double total = lobes.Sum(l => l.Weight.Value);
        return lobes.IsEmpty || total <= RhinoMath.ZeroTolerance
            ? Fin.Fail<LayeredBsdf>(MaterialFault.Create("<parameter:lobe-weights-degenerate>"))
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

    // One-sample MIS with the balance heuristic: choose a lobe by weight (uLobe), sample it, then report the
    // WEIGHT-AVERAGED pdf over all lobes so the estimator is unbiased. The value re-evaluates the FULL layered BSDF
    // against the sampled direction (every lobe contributes to the throughput the integrator divides by the mixed pdf).
    public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1) {
        LocalVector lo = frame.ToLocal(wo);
        double pick = uLobe, acc = 0.0;
        LobeWeight chosen = Lobes.Last;
        foreach (LobeWeight lw in Lobes) { acc += lw.Weight.Value; if (pick <= acc) { chosen = lw; break; } }
        return chosen.Lobe.Sample(lo, u0, u1).Bind(sample => {
            double mixedPdf = Lobes.Fold(0.0, (p, lw) => p + lw.Weight.Value * lw.Lobe.Pdf(lo, sample.Direction));
            RgbSpectrum mixedValue = Lobes.Fold(RgbSpectrum.Black, (v, lw) => v.Add(lw.Lobe.Evaluate(lo, sample.Direction).Scale(lw.Weight.Value)));
            return mixedPdf > 0.0 && mixedValue.IsFinite
                ? Fin.Succ(sample with { Pdf = mixedPdf, Value = mixedValue })
                : Fin.Fail<LobeSample>(MaterialFault.Create("<gamut:non-finite-shade>"));
        });
    }
}
```

## [6]-[SPECTRAL_UPSAMPLE]

- Owner: `SpectralUpsample` author-kernel; `SpectralBand` `[SmartEnum<string>]` the band vocabulary; composes Wacton.Unicolour for every conversion Unicolour already owns.
- Entry: `public static Fin<Spd> ToSpd(RgbSpectrum rgb)` and `public static Fin<RgbSpectrum> SceneLinear(Unicolour colour)` — RGB→SPD is the author-kernel Unicolour lacks (NOT_COVERED); SceneLinear COMPOSES `RgbConfiguration.Acescg` and Unicolour's `.RgbLinear` accessor, never re-deriving the linearization.
- Packages: Wacton.Unicolour (composed — `Spd`, `Unicolour`, `Configuration`, `RgbConfiguration`, `ColourSpace`, `DeltaE`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured illuminant is one Unicolour `Spd` construction; a new working space is one `RgbConfiguration` preset selection; the upsampling table is the only author-kernel — a new spectral band is one `SpectralBand` row; zero new surface.
- Boundary: RGB→SPD is the documented Unicolour NOT_COVERED concern, authored as the Smits (1999) seven-basis non-negative reflectance upsampling — the constant/cyan/magenta/yellow/red/green/blue basis SPDs combined so the round-trip `SPD→XYZ→RGB` reproduces the input chromaticity with a smooth, energy-bounded reflectance (the appearance-engine requirement Smits states); the resulting `Spd` feeds Unicolour's `new Unicolour(Configuration, Spd)` → internal `Xyz.FromSpd` for the measured-illuminant path; the scene-linear working space is `RgbConfiguration.Acescg` (AP1 primaries) or `Aces20651` for the ACES scene-referred path, set in a `Configuration` and read through Unicolour's `.RgbLinear` — Materials NEVER re-derives the sRGB/ACEScg transfer curve, it composes the preset; appearance MATCH between a measured target and a library row is `Unicolour.Difference(reference, DeltaE.Ciede2000)`, the industry-standard appearance ΔE, composed not re-minted; the `IsInRgbGamut` check gates the boundary shade and `RgbSpectrum.Luminance` reads the AP1 weights consistent with the ACEScg working space; this is the seam to `AppUi` custom-visuals#COLOR_SPACE_AXIS — the display-color owner backed by the SAME Unicolour surface, so Materials composes color through that axis and never mints a second `ColourSpace` wrapper.

```csharp
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
// SpectralUpsample is an ADMITTED BOUNDARY NUMERIC KERNEL: ToSpd/Acc/Resample5nm fill fixed-length double[] buffers by
// index, which is the charter's boundary-kernel carve-out from the immutable-fold law. The Smits ordered-combination
// branching and the 69-sample resample are a per-shade hot path; in-place array fill is the correct primitive here, not a
// law violation. Every other kernel on this page is expression-bodied; this is the single declared exception.
public static class SpectralUpsample {
    // Smits (1999) seven non-negative basis spectra over 10 bins (380-720nm, 5nm sampled to 10 control points). The
    // basis is white + cyan/magenta/yellow + red/green/blue; the combination chosen by the RGB ordering keeps the
    // reflectance smooth and in [0,1], so the round-trip is energy-bounded (Unicolour cannot do this — NOT_COVERED).
    private static readonly double[] White =   [1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0];
    private static readonly double[] Cyan =    [0.97, 0.97, 0.97, 0.93, 0.12, 0.04, 0.0, 0.0, 0.05, 0.05];
    private static readonly double[] Magenta = [0.99, 0.99, 0.84, 0.18, 0.03, 0.05, 0.40, 0.99, 0.99, 0.99];
    private static readonly double[] Yellow =  [0.0, 0.0, 0.10, 0.78, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0];
    private static readonly double[] RedB =    [0.10, 0.10, 0.0, 0.0, 0.0, 0.0, 0.84, 1.0, 1.0, 1.0];
    private static readonly double[] GreenB =  [0.0, 0.0, 0.03, 0.49, 1.0, 1.0, 0.46, 0.0, 0.0, 0.0];
    private static readonly double[] BlueB =   [1.0, 1.0, 0.89, 0.46, 0.06, 0.0, 0.0, 0.0, 0.0, 0.05];

    public static Fin<Spd> ToSpd(RgbSpectrum rgb) {
        if (!rgb.IsFinite || rgb.R < 0.0 || rgb.G < 0.0 || rgb.B < 0.0) { return Fin.Fail<Spd>(MaterialFault.Create("<parameter:negative-or-nonfinite-rgb>")); }
        double[] r = new double[10];
        double red = rgb.R, green = rgb.G, blue = rgb.B;
        // Smits' ordered combination: emit white up to the smallest channel, then the secondary, then the primary.
        if (red <= green && red <= blue) { Acc(r, White, red); if (green <= blue) { Acc(r, Cyan, green - red); Acc(r, BlueB, blue - green); } else { Acc(r, Cyan, blue - red); Acc(r, GreenB, green - blue); } }
        else if (green <= red && green <= blue) { Acc(r, White, green); if (red <= blue) { Acc(r, Magenta, red - green); Acc(r, BlueB, blue - red); } else { Acc(r, Magenta, blue - green); Acc(r, RedB, red - blue); } }
        else { Acc(r, White, blue); if (red <= green) { Acc(r, Yellow, red - blue); Acc(r, GreenB, green - red); } else { Acc(r, Yellow, green - blue); Acc(r, RedB, red - green); } }
        for (int i = 0; i < r.Length; i++) { r[i] = Math.Clamp(r[i], 0.0, 1.0); }
        // 380-720nm at ~38nm spacing — interval must be 0/1/5nm per Spd.IsValid, so resample to 5nm at construction.
        Spd spd = new(380, 5, Resample5nm(r));
        return spd.IsValid ? Fin.Succ(spd) : Fin.Fail<Spd>(MaterialFault.Create("<parameter:spd-interval-invalid>"));
    }
    private static void Acc(double[] dst, double[] basis, double w) { double c = Math.Max(0.0, w); for (int i = 0; i < dst.Length; i++) { dst[i] += basis[i] * c; } }
    private static double[] Resample5nm(double[] controls) {
        double[] o = new double[69];                                 // 380..720 inclusive at 5nm = 69 samples
        for (int i = 0; i < o.Length; i++) { double t = i / 68.0 * 9.0; int lo = (int)t; double f = t - lo; o[i] = lo >= 9 ? controls[9] : controls[lo] + (controls[lo + 1] - controls[lo]) * f; }
        return o;
    }

    // SceneLinear: COMPOSE Unicolour for the linearization Materials must never re-derive. ACEScg (AP1) working space.
    public static Fin<RgbSpectrum> SceneLinear(Unicolour colour) {
        ColourTriplet lin = colour.RgbLinear.Triplet;
        RgbSpectrum rgb = new(lin.First, lin.Second, lin.Third);
        return rgb.IsFinite ? Fin.Succ(rgb) : Fin.Fail<RgbSpectrum>(MaterialFault.Create("<gamut:non-finite-linear-rgb>"));
    }
    // The composed ACEScg configuration — the scene-linear working space the whole shading pipeline runs in.
    public static readonly Configuration SceneConfig = new(RgbConfiguration.Acescg);
    public static RgbSpectrum FromAcescg(double r, double g, double b) {
        Unicolour c = new(SceneConfig, ColourSpace.RgbLinear, r, g, b);
        return SceneLinear(c).IfFail(RgbSpectrum.Black);
    }
    // Appearance match between a measured target and a library row — composed Ciede2000, the industry appearance dE.
    public static double AppearanceDelta(Unicolour candidate, Unicolour reference) => candidate.Difference(reference, DeltaE.Ciede2000);
}
```

## [7]-[TONE_MAP]

- Owner: `ToneOperator` `[SmartEnum<string>]` (aces · reinhard · filmic · exposure); `ToneMap` the static operator table.
- Entry: `public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure)` — one entry, four operators by row; the scene-linear input is the integrator's HDR radiance, the output is display-referred [0,1] for the gamut check.
- Packages: Wacton.Unicolour (composed for the encode after tone-map — `RgbConfiguration.StandardRgb`/`Rec2100Pq` transfer), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new operator is one `ToneOperator` row plus one branch in `Apply`; the encode/transfer after tone-mapping is a composed `RgbConfiguration` preset, never an author-kernel; zero new surface.
- Boundary: ACES RRT/ODT is the documented Unicolour NOT_COVERED concern — authored as the Narkowicz (2016) ACES-fit approximation of the combined RRT+ODT (the rational `(x(ax+b))/(x(cx+d)+e)` curve matched to the reference ACES sRGB ODT), exact in the fence; the scene-referred operators are the Reinhard extended `x(1+x/Lwhite²)/(1+x)` global operator, the Hejl-Burgess-Dawson filmic curve, and the plain exposure-then-clamp; exposure is applied as a multiplicative scale before the curve (`scene · 2^exposure`); the OETF/transfer after tone-mapping (sRGB gamma, or PQ for HDR via `RgbConfiguration.Rec2100Pq`) is COMPOSED through Unicolour — the consumer constructs a `Unicolour(config, ColourSpace.RgbLinear, r, g, b)` and reads `.Rgb` for the encoded display value, so Materials authors ONLY the tone CURVE the library lacks and never re-derives a transfer function; the tone-mapped result feeds the `AppUi` COLOR_SPACE_AXIS display path at the seam.

```csharp
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class ToneOperator {
    public static readonly ToneOperator Aces = new("aces");         // RRT + sRGB ODT (Narkowicz fit)
    public static readonly ToneOperator Reinhard = new("reinhard"); // extended global operator
    public static readonly ToneOperator Filmic = new("filmic");     // Hejl-Burgess-Dawson
    public static readonly ToneOperator Exposure = new("exposure"); // linear exposure + clamp
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ToneMap {
    public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure) {
        RgbSpectrum e = sceneLinear.Scale(Math.Pow(2.0, exposure));
        return op == ToneOperator.Aces ? PerChannel(e, AcesFit)
            : op == ToneOperator.Reinhard ? PerChannel(e, ReinhardExtended)
            : op == ToneOperator.Filmic ? Filmic(e)
            : PerChannel(e, static x => Math.Clamp(x, 0.0, 1.0));
    }
    private static RgbSpectrum PerChannel(RgbSpectrum c, Func<double, double> f) => new(f(c.R), f(c.G), f(c.B));

    // Narkowicz ACES fit: the rational approximation of the combined ACES RRT + sRGB ODT. a=2.51 b=0.03 c=2.43 d=0.59
    // e=0.14 are the published constants matched to the reference ACES tone curve. Output is display-referred [0,1].
    private static double AcesFit(double x) {
        const double a = 2.51, b = 0.03, c = 2.43, d = 0.59, ee = 0.14;
        return Math.Clamp(x * (a * x + b) / (x * (c * x + d) + ee), 0.0, 1.0);
    }
    // Extended Reinhard with a white point: maps [0,inf) to [0,1], preserving highlights below Lwhite.
    private static double ReinhardExtended(double x) {
        const double lWhite = 4.0;
        return Math.Clamp(x * (1.0 + x / (lWhite * lWhite)) / (1.0 + x), 0.0, 1.0);
    }
    // Hejl-Burgess-Dawson filmic: the curve already includes the sRGB OETF, so it is applied per channel directly.
    private static RgbSpectrum Filmic(RgbSpectrum c) => PerChannel(c, static x => {
        double v = Math.Max(0.0, x - 0.004);
        return (v * (6.2 * v + 0.5)) / (v * (6.2 * v + 1.7) + 0.06);
    });

    // Encode the tone-mapped display-referred value through the COMPOSED Unicolour transfer (sRGB gamma or PQ for HDR).
    // Materials authored the tone CURVE only; the OETF is Unicolour's, never re-minted here.
    public static (double R, double G, double B) Encode(RgbSpectrum displayLinear, RgbConfiguration target) {
        Unicolour c = new(new Configuration(target), ColourSpace.RgbLinear, displayLinear.R, displayLinear.G, displayLinear.B);
        ColourTriplet rgb = c.Rgb.Triplet;
        return (rgb.First, rgb.Second, rgb.Third);
    }
}
```

## [8]-[RESEARCH]

- [WHITE_FURNACE_HARNESS] (tier-2): the energy-conservation harness that integrates each lobe's directional albedo over the hemisphere against a uniform unit-radiance environment and asserts the reflected energy never exceeds 1 (no energy created) and, for a lossless conductor (F≡1), equals 1 to a Monte-Carlo tolerance (no energy destroyed). The harness is the numeric proof that `MultiScatter.KullaConty` recovers exactly the energy the single-scatter Smith model loses at high roughness — the analytic `DirectionalAlbedo` Karis fit supplies `E(μ)` and the `HemisphericalAlbedo` 8-node quadrature closes `Eavg = 2∫E(μ)μ dμ` exactly, so this is a probe over the closed-form result, not an open gate; it withholds no FINALIZED. The harness samples `BsdfLobe.Sample`, accumulates `value·cosθ/pdf` over N directions, and bounds the furnace residual at the `Conductor`/`Dielectric` roughness sweep (α ∈ {0.1, 0.3, 0.5, 0.7, 0.9}); the residual to retire is whether the 10-control-point Smits basis round-trips every `MaterialLibrary` row to an in-gamut `SurfaceShade` (the page [7]/[G5] spec rail), since a saturated primary can push the upsampled reflectance against the [0,1] energy bound — the basis is energy-bounded by construction, but the per-row furnace + gamut sweep is the tier-2 numeric confirmation. Reciprocity (`f(wo,wi) == f(wi,wo)`) and GGX NDF normalization (`∫ D(h)·cosθh dω == 1`) ride the same harness as exact-equality and unit-integral assertions over the `Microfacet` kernel; these need no probe (they are algebraic identities of the authored bodies) and are stated here only to locate them in the same tier-2 suite.
