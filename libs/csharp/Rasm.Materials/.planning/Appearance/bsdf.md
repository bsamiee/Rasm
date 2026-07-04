# [MATERIALS_BSDF]

THE FRAME-LOCAL SHADING KERNEL. The closed BSDF lobe family and its frame-local microfacet kernel: ONE `BsdfLobe` `[Union]` of seven physical lobes (diffuse · conductor · dielectric · sheen · clearcoat · subsurface · thin-film) under ONE `Evaluate`/`Sample`/`Pdf` contract, ONE `Microfacet` GGX/Smith/Fresnel kernel, ONE `MultiScatter` Kulla-Conty energy compensation, and ONE `LayeredBsdf` weighted-composition fold every material drives by parameter row. The page owns the `ShadingFrame`, `LocalVector`, `SpectralBand`, `BsdfLobe`, and `LayeredBsdf` shading surfaces, the folder-wide `ComparerAccessors.StringOrdinal` ordinal-key pin (the Thinktecture accessor selected once here as policy, never a local re-mint), the validated `RgbSpectrum`/`ComplexIor` reflectance carriers, and the `MaterialFault` union (band `FaultBand.Material` — allocation and disjointness are the `Rasm.Element` `FaultBand` registry, type-enforced at type initialization, never prose). The color-science lowering/grounding half — `SpectralUpsample` RGB→SPD, `ToneMap` ACES, `ConductorMetal` measured complex-IOR rows, and the `SlabStack` OpenPBR Surface 1.1 stack-of-slabs — is the sibling `surface#SPECTRAL_UPSAMPLE`/`#TONE_MAP`/`#CONDUCTOR_IOR`/`#OPENPBR_SLAB` page, split out under the per-page depth budget, composing this page's `MaterialFault` band, `ComparerAccessors.StringOrdinal`, `SpectralBand`, lobe carriers, and `LayeredBsdf.Of` fold. A material is NEVER a lobe subtype: `LayeredBsdf` carries the lobe weights and per-lobe parameters a `MaterialParameters` row supplies, so metal, glass, plastic, skin, fabric, car paint, and wax are weightings of this one closed set, never new lobe types or per-material BSDF classes. The lobe composition is the OpenPBR Surface 1.1 stack-of-slabs (`fuzz` · `coat` · `thin-film` modifier · base substrate mixing a conductor slab against a dielectric base) realized as the `surface#OPENPBR_SLAB` `SlabStack` algebra whose `ToLayered` collapse lowers to this page's `LayeredBsdf` fold; the renderer (`graph#MATERIAL_GRAPH` sink, shaded by the CPU path-trace integrator at the `Rasm.AppUi/Render/pathtrace#PATH_TRACE` seam and the GPU shading pass at the `Rasm.AppUi/Render/shading#SURFACE_SHADE` seam — one BSDF, two evaluators) shades FROM `LayeredBsdf.Sample`/`Evaluate`/`Pdf` and never re-derives lobe math.

## [01]-[INDEX]

- [01]-[SHADING_FRAME]: the `ShadingFrame` local-frame transform, the `LocalVector` z-up triple, the `SpectralBand` band-centre vocabulary, the `MaterialFault` union on the `FaultBand.Material` registry row, and the folder's `ComparerAccessors.StringOrdinal` ordinal-key pin.
- [02]-[MICROFACET_KERNEL]: Fresnel (Schlick plus exact dielectric/conductor), the GGX/Trowbridge-Reitz NDF, and Smith height-correlated masking.
- [03]-[LOBE_FAMILY]: the `BsdfLobe` `[Union]`, the validated `RgbSpectrum`/`ComplexIor` carriers, the per-lobe `Evaluate`/`Sample`/`Pdf` contract, and the Kulla-Conty multi-scatter compensation.
- [04]-[LAYERED_COMPOSITION]: the `LayeredBsdf` weighted-lobe fold, the MIS-balanced sample/pdf, and the material-is-a-row seam.
- [05]-[KERNEL_SEAMS]: the `surface#SPECTRAL_UPSAMPLE`/`#TONE_MAP`/`#CONDUCTOR_IOR`/`#OPENPBR_SLAB` lowering page that composes this kernel's `MaterialFault`/`ComparerAccessors.StringOrdinal`/`SpectralBand`/carriers/`LayeredBsdf.Of`.

## [02]-[SHADING_FRAME]

- Owner: `ShadingFrame` over the composed `Rasm.Numerics.VectorFrame`; `MaterialFault` `[Union]` on the `FaultBand.Material` registry row; the `ComparerAccessors.StringOrdinal` ordinal-key pin (Thinktecture's accessor, selected once as the folder's key-comparison policy).
- Entry: `public static Fin<ShadingFrame> Of(VectorFrame frame, Context context, Direction outgoing, Op key)` — `Fin<T>` aborts when the outgoing direction is degenerate in the local frame; `ToLocal`/`ToWorld` are the only world↔tangent transforms and `CosTheta`/`Sin2Theta`/`TanTheta`/`CosPhi`/`SinPhi` read the local z-up convention every lobe kernel shares. The frame carries the integrator's `Context` so `ToWorld` rails the unitized world direction through the PUBLIC `Direction.Of(Vector3d, Context, Op?)` overload (the `(Vector3d, double, Op?)` overload is `internal` to `Rasm` and cannot bind cross-assembly).
- Packages: Rasm (project — `Rasm.Numerics` `VectorFrame`/`Direction`/`Dimension`/`UnitInterval`), Rasm.Element (project — the `FaultBand` band-allocation registry the `Code` override reads), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new fault is one `MaterialFault` case; a new trig accessor is one expression-bodied member on the frame; zero new surface. The frame NEVER re-mints `VectorFrame` — it wraps the composed `Plane` and reads `Vector3d` projections through it.
- Boundary: `ShadingFrame` is the page's declared boundary capsule over `Rasm.Numerics` — host `Vector3d`/`Plane` access stays inside it and lobe kernels see only local-frame `LocalVector` triples (z is the surface normal, the half-vector and incident/outgoing live in this basis); the z-up tangent convention is stated here once for every lobe so no lobe re-derives `cosθ = w.Z`; `MaterialFault` is the package's one appearance-banded fault, an `Expected`-derived `Error` (`IValidationError<MaterialFault>`) whose `Code` reads the `Rasm.Element` `FaultBand.Material` registry row (band allocation and cross-federation disjointness are the registry's type-enforced law, so a telemetry reader banding by code attributes a shading fault to this folder from the integer alone), so a bare typed case lifts directly into `Fin<T>`/`Validation<Error,T>` and `Fin.Fail` accepts it without a wrapper; every fault constructs the typed case directly — `Gamut` for an out-of-gamut/non-finite shade, `Parameter` for a degenerate input, `Graph` for a degenerate frame/unmatched arm — so a lobe never throws and never returns a NaN outward, and a degenerate local direction rails `MaterialFault.Graph`; `ComparerAccessors.StringOrdinal` is the ordinal comparer the `MaterialLibrary` and `ToneOperator` tables key through.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                       // Fin, Seq, Option
using Rasm.Domain;
using Rasm.Element;                      // FaultBand — the cross-federation band-allocation registry
using Rhino;                             // RhinoMath.ZeroTolerance — the one degeneracy epsilon
using Rhino.Geometry;
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
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

// The 3-band band-centre vocabulary the thin-film lobe (here) and the surface#SPECTRAL_UPSAMPLE curve read; the
// fast-path band centres the spectral curve reduces to, declared once on the kernel so no second color register.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralBand {
    public static readonly SpectralBand Red = new("red", centerNm: 610.0);
    public static readonly SpectralBand Green = new("green", centerNm: 550.0);
    public static readonly SpectralBand Blue = new("blue", centerNm: 465.0);
    public double CenterNm { get; }
    private SpectralBand(string key, double centerNm) : this(key) => CenterNm = centerNm;
}

// --- [ERRORS] ------------------------------------------------------------------------------
// The package appearance-banded fault: Expected-derived over the kernel Rasm.Domain.Expected so the FaultBand.Material
// registry row IS the Expected Code (the generated implicit SmartEnum-to-int conversion; allocation and disjointness
// type-enforced in Rasm.Element) and a typed case lifts BARE onto Fin<T>/Validation<Error,T> with no .ToError() hop —
// Fin.Fail accepts an Expected-derived Error directly. The kernel Expected base ctor is PARAMETERLESS (Expected() {});
// Code is a virtual Error member (the kernel Fault.Unsupported proves it overridable), Message abstract, Category a
// virtual Expected member defaulting to "Fault" and read by FaultExtensions.Category(error). So the band is the one-line
// `Code => FaultBand.Material` registry read, Message and Category are ONE total generated Switch each (the canonical
// production UiFault shape — three near-identical per-case `override Category` bodies are the collapse trigger this
// Switch closes). No [GenerateUnionOps]: the kernel union-ops source-gen is strictly opt-in and emits only per-case
// SelfOp keys for marked unions — fault cases are carriers already keyed by an explicit Op, so the union stays
// unmarked. [Union] generates Switch/Map, never factories, so the band declares its OWN — a nested `…Case`
// record carries the data and an unsuffixed static factory MaterialFault.Parameter(key, detail) returns the base, the
// `…Case` suffix freeing the factory name (a same-named nested type + method is CS0102). Create routes the unspecific
// case under a boundary-admission Op so the IValidationError<MaterialFault>.Create(string) floor is total.
[Union]
public abstract partial record MaterialFault : Expected, IValidationError<MaterialFault> {
    private MaterialFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => FaultBand.Material;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record GamutCase(Op Key, string Detail) : MaterialFault(Key, Detail);
    public sealed record ParameterCase(Op Key, string Detail) : MaterialFault(Key, Detail);
    public sealed record GraphCase(Op Key, string Detail) : MaterialFault(Key, Detail);

    public override string Category => Switch(
        gamutCase:     static _ => "Gamut",
        parameterCase: static _ => "Parameter",
        graphCase:     static _ => "Graph");
    public override string Message => Switch(
        state: Detail,
        gamutCase:     static (detail, c) => $"Shade out of gamut under '{c.Key}': {detail}.",
        parameterCase: static (detail, c) => $"Degenerate appearance parameter under '{c.Key}': {detail}.",
        graphCase:     static (detail, c) => $"Degenerate shading frame under '{c.Key}': {detail}.");

    public static MaterialFault Gamut(Op key, string detail) => new GamutCase(key, detail);
    public static MaterialFault Parameter(Op key, string detail) => new ParameterCase(key, detail);
    public static MaterialFault Graph(Op key, string detail) => new GraphCase(key, detail);
    public static MaterialFault Create(string message) => Graph(Admission, message);
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

## [03]-[MICROFACET_KERNEL]

- Owner: the static `Microfacet` kernel — GGX NDF · Smith height-correlated masking · the Schlick/dielectric/conductor Fresnel family · the Heitz VNDF sampler.
- Entry: `public static double Ndf(LocalVector h, double alphaX, double alphaY)` and the sibling `MaskingShadowing`/`FresnelDielectric`/`FresnelConductor`/`FresnelSchlick` — pure values; the NDF takes only the half-vector and anisotropic roughness `(alphaX, alphaY)`, and the isotropic case is `alphaX == alphaY`, never a second isotropic kernel.
- Packages: Thinktecture.Runtime.Extensions, BCL inbox (the kernel is frame-local and host-free; the public `Rasm.Numerics.Direction.Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key)` composes at the world boundary — see LOBE_FAMILY).
- Growth: a new distribution (Beckmann) is one `Microfacet` static the owning lobe case routes; a new Fresnel term is one `Microfacet` static a lobe's fresnel policy names — the lobe case IS the Fresnel discriminant (conductor/dielectric/thin-film run exact, clearcoat runs Schlick), so a parallel mode enum re-describing that closed choice is the deleted form; zero new surface. The roughness→alpha remap (`alpha = roughness²`) is the one Disney-convention remap every lobe reads.
- Boundary: the NDF is GGX/Trowbridge-Reitz in anisotropic form `D = 1 / (π·αx·αy·((hx/αx)² + (hy/αy)² + hz²)²)`, reducing to the isotropic `D = α² / (π·(cos²θh·(α²−1)+1)²)` when `αx==αy` — one body, both modes; the masking-shadowing is the Smith height-correlated `G2 = 1 / (1 + Λ(wo) + Λ(wi))` with the GGX `Λ`, NEVER the separable `G1(wo)·G1(wi)` (separable overestimates correlated occlusion and breaks the white-furnace test); the Fresnel family is lobe-selected — the exact unpolarized dielectric term is SIDE-AWARE (`cosI < 0` flips `η` to `1/η`, so an interior ray reads its true reflectance and interior TIR reads 1 — `|cosI|` under the exterior `η` is the deleted form that missed interior TIR), the complex-IOR conductor term grounds metals, and `F0 + (1−F0)(1−cosθ)⁵` Schlick carries the fast coated path; the GGX visible-normal-distribution sample (Heitz 2018) is the one importance-sampling routine the conductor/dielectric/clearcoat lobes share.

```csharp signature
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
        double ci = Math.Clamp(cosI, -1.0, 1.0);
        if (ci < 0.0) { eta = 1.0 / eta; ci = -ci; }   // interior incidence flips the ratio — |cosI| under the exterior η misses interior TIR
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

    public static RgbSpectrum FresnelConductor(double cosI, ComplexIor ior) =>
        ior.Eta.Zip(ior.K, (e, kk) => FresnelConductor(cosI, e, kk));

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

## [04]-[LOBE_FAMILY]

- Owner: `BsdfLobe` `[Union]` closed lobe family; `LobeSample` the typed sample receipt.
- Entry: `public RgbSpectrum Evaluate(LocalVector wo, LocalVector wi)` · `public Fin<LobeSample> Sample(LocalVector wo, double uc, double u0, double u1, Op key)` · `public double Pdf(LocalVector wo, LocalVector wi)` — the three-method contract every lobe case implements through one total `Switch`; the lobe is frame-local, so `Evaluate`/`Pdf` read the local-frame `LocalVector` triples the integrator transforms once, `Sample` carries the `Op key` for its `MaterialFault` rail, and `uc` is the lobe-local CHOICE variable (the dielectric reflect/transmit lottery) decorrelated from the `(u0, u1)` pair that shapes the half-vector; `RgbSpectrum` is the validated three-band `[ComplexValueObject]` reflectance carrier gating non-finite/negative channels once at `Create`, NEVER a host color type at an interior signature and NEVER an unvalidated raw triple a downstream `IsFinite` re-checks.
- Packages: Rasm (project — `Direction.Reflect`/`Refract`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new physical lobe is one `BsdfLobe` case admitted ONLY when no parameterization of the existing seven reproduces the measured physics — and then it serves ALL materials, never one material; a new material is NEVER a lobe. The lobe set is closed at seven: diffuse, conductor, dielectric, sheen, clearcoat, subsurface, thin-film. The `[9]-[OPENPBR_SLAB]` `SlabStack` is the realized formal layering construction over these lobes — the fuzz slab lowers to the `Sheen` lobe and the albedo-scaling operators compose the stack the `LayeredBsdf` weighted fold the integrator shades collapses from.
- Boundary: every lobe `Evaluate` returns the BSDF value times nothing — the cosine-weight and division by pdf live in the integrator (`LayeredBsdf.Sample`), so a lobe is the pure `f(wo, wi)` and never folds in the geometry term twice; the `Conductor` and `ThinFilm` lobes carry one `ComplexIor` `[ComplexValueObject]` band (its `Eta`/`K` two validated `RgbSpectrum` carriers) the `Microfacet.FresnelConductor(cosI, ComplexIor)` overload reads, never a parallel `(Eta, K)`/`(BaseEta, BaseK)` triple pair; the four glossy-reflect lobes (conductor · dielectric-reflect · clearcoat · thin-film) evaluate through ONE `MicrofacetReflect` Cook-Torrance skeleton — the half-vector canonicalized to the geometric upper hemisphere (an interior both-below pair otherwise zeroes the NDF and shades compensation-only), `fresnel(state, wo·h)·D·G₂/(4|cosθo||cosθi|)` over the SIGNED half-vector cosine so the side-aware dielectric term reads interior reflectance, plus a per-lobe compensation policy, hemisphere and grazing degeneracy guarded once — each lobe supplying ONLY its Fresnel term (`FresnelConductor` over `ComplexIor`, the white-scaled exact dielectric, the weight-scaled `Schlick` `F0` 0.04 coat, the interference-modulated base-conductor film) and its Kulla-Conty add (`Black` for clearcoat/thin-film), so a per-lobe `D·G₂` re-inline is the deleted form; the conductor and dielectric lobes drive `Microfacet` with their `(alphaX, alphaY)` and the exact conductor/dielectric Fresnel kernels, the clearcoat is a fixed-`Schlick`-`F0`-0.04 dielectric GGX layer over the base reusing the conductor reflect arm at `Clearcoat.Alpha`, the sheen is the Estevez-Kulla inverted-Gaussian retroreflective fabric lobe, and the thin-film is the Belcour-Barla spectral interference term modulating the base Fresnel; the diffuse lobe carries the Oren-Nayar roughness term (Lambert is `roughness == 0`, the `a→1/b→0` limit), so one diffuse case spans matte-to-rough, never a separate Lambert and Oren-Nayar type; the `Subsurface` lobe is the in-pixel-footprint diffusion limit Burley refactors the dielectric SSS BRDF into — a Lambert base modulated by the two rough-surface Fresnel transmission factors `(1−0.5·F_L)(1−0.5·F_V)`, reciprocal and energy-bounded, the albedo carrying the scatter colour directly (the normalized-diffusion no-inversion guarantee), and the wide-radius transport is the separable BSSRDF the integrator samples through `MultiScatter.SeparableProfile` (Burley's `Rd(r)` normalized over the disk `∫Rd(r)·2πr dr = 1`) by SURFACE distance — the spatial profile is NEVER multiplied as a directional BRDF, since `Rd(r)` integrates to one over area not solid angle; the multi-scatter energy compensation is the Kulla-Conty term added to the conductor/dielectric single-scatter, recovering the energy the single-scatter Smith model loses at high roughness — without it the white-furnace test fails above α≈0.5, and its Fresnel response is the diffused-bounce geometric series `F_avg·E_avg/(1−F_avg(1−E_avg))`, never `F_avg²·(1−E_avg)/(…)` which destroys energy; the lobe sampler is frame-local and host-free — `LobeSample.Direction` is a `LocalVector`, the dielectric transmission runs the same exact Snell formula `Rasm.Numerics.Direction.Refract` owns (eta·d + (eta·cosI−√k)·n, TIR-rejected) so the math is single-sourced; the WORLD reflected/refracted ray the path tracer needs for the next bounce is the integrator's `ShadingFrame.ToWorld` composition, and when the renderer prefers the host `Direction` it COMPOSES the instance `Direction.Reflect(Direction normal)` and the static `Direction.Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key)` at that world seam (the 5-arg Snell — `etaIncident`/`etaTransmitted` are the two media IORs, not an `(eta, cosI, n)` shorthand) — Snell and the mirror are NEVER re-minted as a parallel kernel; the lobe dispatch threads `(wo, wi)`/`(owner, wo, uc, u0, u1)` through the state-passing `Switch` overload with `static` arms so the per-sample integrator loop allocates no closure; `DielectricPdf` keys its reflect/transmit split on the half-vector cosine `wo.Dot(h)` exactly as `DielectricSample` does, never the geometric `wo.CosTheta`; `DielectricSample`'s transmit arm carries the refraction Jacobian `η²·|wi·h|/(wo·h + η·wi·h)²` mirroring `DielectricPdf` (a reflect-form `1/(4|wo·h|)` on the transmit sample de-syncs the sample-local pdf from the balance-heuristic average) and draws its reflect/transmit lottery from the DEDICATED `uc` choice variable, never the consumed `u0` that already fixed the sampled half-vector (reusing it correlates the lottery with the VNDF radial coordinate and biases the estimator), so the balance-heuristic pdf stays unbiased and the white-furnace harness closes for rough glass on both sides of the interface.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct RgbSpectrum {
    public double R { get; }
    public double G { get; }
    public double B { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double r, ref double g, ref double b) {
        if (!double.IsFinite(r) || !double.IsFinite(g) || !double.IsFinite(b) || r < 0.0 || g < 0.0 || b < 0.0)
            validationError = new ValidationError($"<rgb-spectrum-out-of-band:{r:R},{g:R},{b:R}>");
    }

    public static readonly RgbSpectrum Black = Create(0.0, 0.0, 0.0);
    public static readonly RgbSpectrum White = Create(1.0, 1.0, 1.0);
    public RgbSpectrum Map(Func<double, double> f) => Create(f(R), f(G), f(B));
    public RgbSpectrum Zip(RgbSpectrum o, Func<double, double, double> f) => Create(f(R, o.R), f(G, o.G), f(B, o.B));
    public RgbSpectrum Scale(double s) => Create(R * s, G * s, B * s);
    public RgbSpectrum Mul(RgbSpectrum o) => Create(R * o.R, G * o.G, B * o.B);
    public RgbSpectrum Add(RgbSpectrum o) => Create(R + o.R, G + o.G, B + o.B);
    public RgbSpectrum Lerp(RgbSpectrum o, double t) => Create(R + (o.R - R) * t, G + (o.G - G) * t, B + (o.B - B) * t);
    public double Luminance => 0.2722287 * R + 0.6740818 * G + 0.0536895 * B;
}

[ComplexValueObject]
public readonly partial struct ComplexIor {
    public RgbSpectrum Eta { get; }
    public RgbSpectrum K { get; }

    public RgbSpectrum FresnelNormal =>
        Eta.Zip(K, static (e, k) => ((e - 1.0) * (e - 1.0) + k * k) / ((e + 1.0) * (e + 1.0) + k * k));
}

public readonly record struct LobeSample(LocalVector Direction, RgbSpectrum Value, double Pdf, bool Transmission) {
    public bool IsValid => Pdf > 0.0;
}

// --- [TYPES] -------------------------------------------------------------------------------
[Union]
public abstract partial record BsdfLobe {
    private BsdfLobe() { }

    public sealed record Diffuse(RgbSpectrum Albedo, double Roughness) : BsdfLobe;
    public sealed record Conductor(ComplexIor Ior, double AlphaX, double AlphaY) : BsdfLobe;
    public sealed record Dielectric(double Ior, double AlphaX, double AlphaY, RgbSpectrum Transmittance) : BsdfLobe;
    public sealed record Sheen(RgbSpectrum Tint, double Roughness) : BsdfLobe;
    public sealed record Clearcoat(double Weight, double Roughness) : BsdfLobe {
        public static double Alpha(Clearcoat c) => Microfacet.AlphaOf(c.Roughness);
    }
    public sealed record Subsurface(RgbSpectrum Albedo, double MeanFreePath) : BsdfLobe;
    // Thickness is NANOMETRES — the graph#MATERIAL_LIBRARY ThinFilm.ThicknessNm carrier feeds it, and FilmInterference
    // divides the OPD by a SpectralBand wavelength in nm; a metre-scaled thickness silently kills the interference.
    public sealed record ThinFilm(double Thickness, double FilmIor, double Roughness, ComplexIor BaseIor) : BsdfLobe {
        public static double AlphaX(ThinFilm f) => Microfacet.AlphaOf(f.Roughness);
        public static double AlphaY(ThinFilm f) => Microfacet.AlphaOf(f.Roughness);
    }

    // The three-method contract dispatches through the state-threading Switch overload — (wo, wi) ride the state tuple
    // and every arm is `static`, so the per-sample path-trace hot loop allocates NO closure per lobe evaluation (the
    // capturing-lambda Switch is the rejected form on the inner integrator loop). Clearcoat folds onto the conductor's
    // reflect arm at its fixed alpha — Microfacet.AlphaOf returns a total double, so a `is double` guard is dead branch.
    public RgbSpectrum Evaluate(LocalVector wo, LocalVector wi) => Switch(
        state: (Wo: wo, Wi: wi),
        diffuse:    static (s, d) => EvalDiffuse(d, s.Wo, s.Wi),
        conductor:  static (s, c) => EvalConductor(c, s.Wo, s.Wi),
        dielectric: static (s, g) => s.Wo.SameHemisphere(s.Wi) ? EvalDielectricReflect(g, s.Wo, s.Wi) : EvalDielectricTransmit(g, s.Wo, s.Wi),
        sheen:      static (s, h) => EvalSheen(h, s.Wo, s.Wi),
        clearcoat:  static (s, c) => EvalClearcoat(c, s.Wo, s.Wi),
        subsurface: static (s, b) => EvalSubsurface(b, s.Wo, s.Wi),
        thinFilm:   static (s, f) => EvalThinFilm(f, s.Wo, s.Wi));

    public double Pdf(LocalVector wo, LocalVector wi) => Switch(
        state: (Wo: wo, Wi: wi),
        diffuse:    static (s, _) => DiffusePdf(s.Wo, s.Wi),
        conductor:  static (s, c) => MicrofacetReflectPdf(s.Wo, s.Wi, c.AlphaX, c.AlphaY),
        dielectric: static (s, g) => DielectricPdf(g, s.Wo, s.Wi),
        sheen:      static (s, _) => DiffusePdf(s.Wo, s.Wi),
        clearcoat:  static (s, c) => MicrofacetReflectPdf(s.Wo, s.Wi, Clearcoat.Alpha(c), Clearcoat.Alpha(c)),
        subsurface: static (s, _) => DiffusePdf(s.Wo, s.Wi),
        thinFilm:   static (s, f) => MicrofacetReflectPdf(s.Wo, s.Wi, ThinFilm.AlphaX(f), ThinFilm.AlphaY(f)));

    // uc is the lobe-local CHOICE draw (only the dielectric lottery reads it) — decorrelated by contract from the
    // (u0, u1) pair that shapes the half-vector; LayeredBsdf.Sample supplies it as the rescaled lobe-pick remainder.
    public Fin<LobeSample> Sample(LocalVector wo, double uc, double u0, double u1, Op key) => Switch(
        state: (Owner: this, Wo: wo, Uc: uc, U0: u0, U1: u1, Key: key),
        diffuse:    static (s, _) => CosineSample(s.Wo, s.U0, s.U1, s.Owner, s.Key),
        conductor:  static (s, c) => MicrofacetReflectSample(s.Wo, c.AlphaX, c.AlphaY, s.U0, s.U1, s.Owner, s.Key),
        dielectric: static (s, g) => DielectricSample(g, s.Wo, s.Uc, s.U0, s.U1, s.Key),
        sheen:      static (s, _) => CosineSample(s.Wo, s.U0, s.U1, s.Owner, s.Key),
        clearcoat:  static (s, c) => MicrofacetReflectSample(s.Wo, Clearcoat.Alpha(c), Clearcoat.Alpha(c), s.U0, s.U1, s.Owner, s.Key),
        subsurface: static (s, _) => CosineSample(s.Wo, s.U0, s.U1, s.Owner, s.Key),
        thinFilm:   static (s, f) => MicrofacetReflectSample(s.Wo, ThinFilm.AlphaX(f), ThinFilm.AlphaY(f), s.U0, s.U1, s.Owner, s.Key));

    private static double DiffusePdf(LocalVector wo, LocalVector wi) => wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0;

    // --- [DIFFUSE]
    // Qualitative Oren-Nayar: matte-to-rough in ONE case (Lambert is Roughness==0, the a→1/b→0 limit), never a parallel
    // Lambert type. The BSDF value is the bracket/π scaled by albedo — the cosine weight and pdf division live in the
    // integrator (LayeredBsdf.Sample), so this is the pure f(wo,wi) with the geometry term folded in nowhere twice.
    private static RgbSpectrum EvalDiffuse(Diffuse d, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        double s2 = d.Roughness * d.Roughness;
        double a = 1.0 - 0.5 * s2 / (s2 + 0.33), b = 0.45 * s2 / (s2 + 0.09);
        double sinO = wo.SinTheta, sinI = wi.SinTheta;
        double maxCos = sinO > 1e-4 && sinI > 1e-4 ? Math.Max(0.0, wi.CosPhi * wo.CosPhi + wi.SinPhi * wo.SinPhi) : 0.0;
        double aco = Math.Abs(wo.CosTheta), aci = Math.Abs(wi.CosTheta);
        (double sinAlpha, double tanBeta) = aci > aco ? (sinO, sinI / aci) : (sinI, sinO / aco);
        return d.Albedo.Scale((a + b * maxCos * sinAlpha * tanBeta) / Math.PI);
    }

    // The LOCAL subsurface lobe is the energy-bounded diffuse-transmission limit Burley refactors the dielectric SSS
    // BRDF into for the in-pixel-footprint case (§2.3): a Lambert base modulated by the two rough-surface Fresnel
    // transmission factors (1-0.5*F_L)(1-0.5*F_V), reciprocal and never exceeding the Lambert energy bound — NOT the
    // spatial Rd(r) profile multiplied as a BRDF (Rd integrates to 1 over SURFACE AREA, not solid angle, so multiplying
    // it onto a directional lobe is dimensionally incoherent and unbounded as r→0). The BRDF limit is radius-independent
    // (the in-footprint diffuse is the same shape for any scatter distance), so MeanFreePath is NOT read here — it is the
    // wide-radius spatial-transport parameter the integrator hands MultiScatter.SeparableProfile to sample the true
    // BSSRDF by surface distance. Albedo carries the scatter colour directly so no albedo inversion is needed (the
    // normalized-diffusion guarantee). Subsurface is closed at this one lobe.
    private static RgbSpectrum EvalSubsurface(Subsurface b, LocalVector wo, LocalVector wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        double fL = SchlickWeight(Math.Abs(wi.CosTheta)), fV = SchlickWeight(Math.Abs(wo.CosTheta));
        double diffuse = (1.0 - 0.5 * fL) * (1.0 - 0.5 * fV) / Math.PI;
        return b.Albedo.Scale(diffuse);
    }
    private static double SchlickWeight(double cosTheta) { double m = Math.Clamp(1.0 - cosTheta, 0.0, 1.0); double m2 = m * m; return m2 * m2 * m; }

    // --- [MICROFACET_REFLECT]
    // The ONE Cook-Torrance single-scatter skeleton the four glossy-reflect lobes (conductor · dielectric-reflect ·
    // clearcoat · thin-film) drive by POLICY: fresnel(state, wo·h)·D·G2/(4|cosθo||cosθi|) — h canonicalized up, the
    // half-vector cosine SIGNED so the side-aware dielectric policy reads interior reflectance — plus the per-lobe
    // compensation term at the geometric-mean alpha — Kulla-Conty for the energy-compensated lobes, Black for the
    // uncompensated pair. Hemisphere and grazing degeneracy guard ONCE here (a zero cosine would divide the skeleton
    // to a non-finite shade the RgbSpectrum gate throws on). static lambdas keep the per-sample loop closure-free.
    private static RgbSpectrum MicrofacetReflect<TState>(TState state, LocalVector wo, LocalVector wi, double alphaX, double alphaY,
        Func<TState, double, RgbSpectrum> fresnel, Func<TState, double, LocalVector, LocalVector, RgbSpectrum> compensation) {
        if (!wo.SameHemisphere(wi) || wo.CosTheta == 0.0 || wi.CosTheta == 0.0) { return RgbSpectrum.Black; }
        LocalVector h = wo.Add(wi).Normalize();
        h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;   // canonicalize to the geometric upper hemisphere — an interior (both-below) pair otherwise zeroes Ndf and shades compensation-only
        double single = Microfacet.Ndf(h, alphaX, alphaY) * Microfacet.MaskingShadowing(wo, wi, alphaX, alphaY)
            / (4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta));
        return fresnel(state, wo.Dot(h)).Scale(single).Add(compensation(state, Math.Sqrt(alphaX * alphaY), wo, wi));   // SIGNED wo·h — the side-aware dielectric policy reads interior reflectance; conductor/film take |·| internally
    }

    // --- [CONDUCTOR]
    // Fresnel policy: the measured complex-IOR conductor term. The compensation lobe is azimuthally invariant and its
    // directional-albedo fit is isotropic, so it reads the geometric-mean alpha the skeleton hands it, and the
    // diffused-bounce Fresnel is the cosine-weighted AVERAGE — proxied by the measured normal-incidence FresnelNormal
    // the ComplexIor carries, NOT the per-sample half-vector fr (view-dependent, biasing the multi-bounce term), so a
    // tinted metal keeps its hue across the rough-surface multi-scatter recovery.
    private static RgbSpectrum EvalConductor(Conductor c, LocalVector wo, LocalVector wi) =>
        MicrofacetReflect(c, wo, wi, c.AlphaX, c.AlphaY,
            fresnel: static (own, cosH) => Microfacet.FresnelConductor(cosH, own.Ior),
            compensation: static (own, alpha, o, i) => MultiScatter.KullaConty(alpha, own.Ior.FresnelNormal, o, i));

    // --- [DIELECTRIC]
    // Reflect arm: the SAME Kulla-Conty matte multi-scatter lobe the conductor uses (Turquin §5.6 supports rough
    // dielectric reflection), the diffused-bounce Fresnel proxied by the achromatic normal-incidence reflectance
    // F0 = ((η−1)/(η+1))² as a white average so a rough glass interface conserves energy under the furnace; the
    // transmit arm carries no compensation lobe (energy lost to reflection is recovered on the reflect arm and the
    // transmit lobe is the refracted single path).
    private static RgbSpectrum EvalDielectricReflect(Dielectric g, LocalVector wo, LocalVector wi) =>
        MicrofacetReflect(g, wo, wi, g.AlphaX, g.AlphaY,
            fresnel: static (own, cosH) => RgbSpectrum.White.Scale(Microfacet.FresnelDielectric(cosH, own.Ior)),
            compensation: static (own, alpha, o, i) => MultiScatter.KullaConty(alpha, RgbSpectrum.White.Scale(DielectricF0(own.Ior)), o, i));
    private static double DielectricF0(double ior) { double r = (ior - 1.0) / (ior + 1.0); return r * r; }
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
        double sin2 = h.Sin2Theta;
        double dSheen = (2.0 + inv) * Math.Pow(sin2, inv * 0.5) / (2.0 * Math.PI);
        double g = 1.0 / (4.0 * (Math.Abs(wo.CosTheta) + Math.Abs(wi.CosTheta) - Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta)));
        return s.Tint.Scale(dSheen * g);
    }

    // --- [CLEARCOAT]
    // Fresnel policy: the weight-scaled fixed-Schlick F0 0.04 coat term; no compensation lobe (the thin smooth coat's
    // multi-scatter loss is negligible at Clearcoat.Alpha and the base substrate recovers its own energy).
    private static RgbSpectrum EvalClearcoat(Clearcoat c, LocalVector wo, LocalVector wi) =>
        MicrofacetReflect(c, wo, wi, Clearcoat.Alpha(c), Clearcoat.Alpha(c),
            fresnel: static (own, cosH) => RgbSpectrum.White.Scale(own.Weight * Microfacet.FresnelSchlick(Math.Abs(cosH), 0.04)),
            compensation: static (_, _, _, _) => RgbSpectrum.Black);

    // --- [THIN_FILM]
    // Fresnel policy: the Belcour-Barla interference term IS a Fresnel modifier — the 3-band OPD cosine modulates the
    // base conductor Fresnel at the half-vector cosine, so the film rides the same skeleton; no compensation lobe.
    private static RgbSpectrum EvalThinFilm(ThinFilm f, LocalVector wo, LocalVector wi) =>
        MicrofacetReflect(f, wo, wi, ThinFilm.AlphaX(f), ThinFilm.AlphaY(f),
            fresnel: static (own, cosH) => Microfacet.FresnelConductor(cosH, own.BaseIor).Mul(FilmInterference(own, cosH)),
            compensation: static (_, _, _, _) => RgbSpectrum.Black);
    private static RgbSpectrum FilmInterference(ThinFilm f, double cosI) {
        double sinT2 = (1.0 - cosI * cosI) / (f.FilmIor * f.FilmIor);
        double opd = 2.0 * f.FilmIor * f.Thickness * Math.Sqrt(Math.Max(0.0, 1.0 - sinT2));   // 2·n·t·cosθt — the film optical path difference
        return RgbSpectrum.Create(Interference(opd, SpectralBand.Red.CenterNm), Interference(opd, SpectralBand.Green.CenterNm), Interference(opd, SpectralBand.Blue.CenterNm));
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
        h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;   // matches the sampler's upper-hemisphere h — an interior pair otherwise pdf-zeroes against the nonzero sample
        return Microfacet.VisibleNormalPdf(wo, h, ax, ay) / (4.0 * Math.Abs(wo.Dot(h)));
    }
    // The reflect/transmit lottery draws uc — NEVER the consumed u0 that already fixed h (reuse correlates the split
    // with the VNDF radial coordinate and biases the estimator the balance heuristic assumes independent).
    private static Fin<LobeSample> DielectricSample(Dielectric g, LocalVector wo, double uc, double u0, double u1, Op key) {
        LocalVector h = Microfacet.SampleVisibleNormal(wo.CosTheta < 0.0 ? wo.Scale(-1.0) : wo, g.AlphaX, g.AlphaY, u0, u1);
        double f = Microfacet.FresnelDielectric(wo.Dot(h), g.Ior);
        if (uc < f) {
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
        // The refraction Jacobian η²·|wi·h|/(wo·h + η·wi·h)² mirroring DielectricPdf's transmit arm (η = 1/eta, the
        // half-vector ratio) — the reflect-form 1/(4|wo·h|) here de-syncs the sample-local pdf from the balance average.
        double etaH = 1.0 / eta;
        double sqrtDenom = wo.Dot(h) + etaH * wi.Dot(h);
        double pdf = (1.0 - f) * Microfacet.VisibleNormalPdf(wo, h, g.AlphaX, g.AlphaY) * Math.Abs(etaH * etaH * wi.Dot(h) / (sqrtDenom * sqrtDenom));
        return pdf > 0.0 ? Fin.Succ(new LobeSample(wi, EvalDielectricTransmit(g, wo, wi), pdf, Transmission: true)) : Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<dielectric-refract-degenerate>"));
    }
    private static double DielectricPdf(Dielectric g, LocalVector wo, LocalVector wi) {
        if (wo.SameHemisphere(wi)) {
            LocalVector h = wo.Add(wi).Normalize();
            h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;
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
    // 8-node midpoint quadrature over Eavg = 2∫E(μ)μ dμ — the page's named [EXPRESSION_SPINE] statement-kernel
    // exemption (beside the distinct LayeredBsdf.Of statically-unreachable-throw exemption [05] names):
    // a fixed-trip accumulation over the closed-form Karis fit, allocation-free on the per-shade hot path.
    private static double HemisphericalAlbedo(double alpha) {
        double acc = 0.0;
        const int n = 8;
        for (int i = 0; i < n; i++) { double mu = (i + 0.5) / n; acc += DirectionalAlbedo(alpha, mu) * mu; }
        return Math.Clamp(2.0 * acc / n, 0.0, 1.0);
    }
    // The Kulla-Conty multi-scatter lobe (Imageworks 2017; Turquin Eq.6-7; Fdez-Aguera): the matte compensation lobe
    // fms = (1-E(μo))(1-E(μi)) / (π(1-Eavg)) whose directional albedo EXACTLY complements 1-E(μo), times the diffused
    // Fresnel response Favg·Eavg / (1 - Favg(1-Eavg)) — the geometric series ∑ Favg^k(1-Eavg)^k of successive microfacet
    // bounces. The energy-bound numerator is Favg·Eavg (the per-bounce escaped fraction), NOT Favg²·(1-Eavg): the
    // latter destroys energy and fails the white-furnace test the page's [07] harness asserts. Reciprocal by
    // construction (μo↔μi symmetric). Favg uses the per-band FresnelNormal as the diffused-average proxy the conductor
    // lobe already carries, so a tinted metal (gold/copper) keeps its hue through the multi-bounce term.
    public static RgbSpectrum KullaConty(double alpha, RgbSpectrum fresnelAvg, LocalVector wo, LocalVector wi) {
        double eo = DirectionalAlbedo(alpha, Math.Abs(wo.CosTheta));
        double ei = DirectionalAlbedo(alpha, Math.Abs(wi.CosTheta));
        double eavg = HemisphericalAlbedo(alpha);
        double fms = (1.0 - eo) * (1.0 - ei) / (Math.PI * Math.Max(1e-4, 1.0 - eavg));
        return fresnelAvg.Map(f => fms * f * eavg / Math.Max(1e-4, 1.0 - f * (1.0 - eavg)));
    }

    // Burley's normalized diffusion profile Rd(r) = (e^{-r/d}+e^{-r/3d})/(8πdr), intrinsically normalized over the disk
    // (∫Rd(r)·2πr dr = 1), so the integrated result times albedo IS the diffuse colour with no albedo inversion. This is
    // the SPATIAL BSSRDF the separable subsurface integrator samples by surface distance r, never a per-sample BRDF
    // multiplier; d is the per-channel mean-free-path the SubsurfaceRadius carrier supplies (Disney scatterDistance).
    public static double SeparableProfile(double meanFreePath, double r) {
        double d = Math.Max(1e-4, meanFreePath), rr = Math.Max(1e-4, r);
        return (Math.Exp(-rr / d) + Math.Exp(-rr / (3.0 * d))) / (8.0 * Math.PI * d * rr);
    }
}
```

## [05]-[LAYERED_COMPOSITION]

- Owner: `LayeredBsdf` — the weighted-lobe fold; `LobeWeight` the per-lobe weight row.
- Entry: `public RgbSpectrum Evaluate(ShadingFrame frame, Direction wo, Direction wi)` · `public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1, Op key)` · `public double Pdf(ShadingFrame frame, Direction wo, Direction wi)` — the renderer's sole shading entry; the integrator transforms to local once, folds the weighted lobes, and transforms back; `Sample` carries the `Op key` for the `MaterialFault` rail and `Of` admits the weighted lobe list under the same key.
- Packages: Rasm (project — `Rasm.Numerics` `Direction`/`UnitInterval`), LanguageExt.Core, BCL inbox.
- Growth: a new MATERIAL is a new `Seq<LobeWeight>` value — a row of weights and lobe parameters a `MaterialParameters` row supplies — NEVER a new type; this is THE polymorphic-mandate seam: `LayeredBsdf.Of` takes the weighted lobe list a library row produces, so gold is `[Conductor 1.0]`, glass is `[Dielectric 1.0]`, plastic is `[Diffuse 0.9, Dielectric-coat 0.1]`, car paint is `[Conductor-flake 0.7, Clearcoat 0.3]`, skin is `[Subsurface 0.8, Dielectric 0.2]`, velvet is `[Diffuse 0.6, Sheen 0.4]`, wax is `[Subsurface 0.5, Diffuse 0.5]` — all the SAME `LayeredBsdf`, differing only by row data. The `[9]-[OPENPBR_SLAB]` `SlabStack` is the realized formal layering construction (fuzz over coat over base, albedo-scaling operators) whose `ToLayered` energy-preserving collapse lowers to this weighted-lobe fold the integrator shades; the slab algebra builds the stack a row drives through, this fold the one BSDF the renderer reads.
- Boundary: `Evaluate` is the weighted SUM of lobe `Evaluate` (each lobe is linear, so the layered BSDF is the convex combination of lobe values by weight); `Sample` is the one-sample MIS — pick a lobe proportionally to weight via `uLobe`, hand it the rescaled remainder `(uLobe − cdfBefore)/weight` as its `uc` choice variable (stratification survives both choices), sample it, then the returned pdf is the WEIGHT-AVERAGED pdf across ALL lobes (the balance heuristic) so the estimator is unbiased and low-variance, and the value re-evaluates the FULL layered BSDF against the sampled direction; `Pdf` mirrors that weighted average exactly; weights are `UnitInterval` and sum-normalized at `Of` (a row whose weights miss [0,1] or sum to zero rails `MaterialFault.Parameter` `<lobe-weights-degenerate>`); the `Of` normalization is the page's one admitted total-construction site — every input `Weight` is already a `UnitInterval` and `total` is their sum, so `weight/total` is in `[0,1]` by construction and the `UnitInterval.Create` throw is statically unreachable, named here as the `[EXPRESSION_SPINE]` exemption rather than exception-style control flow in a fallible path; the boundary projects the final shade to in-gamut at the renderer edge, never inside the fold — a non-finite throughput rails `MaterialFault.Gamut` `<non-finite-shade>` through `Option`/`Fin`, never propagating NaN; this fold is the ONLY place lobe weights live, so the masonry-assignment consumer and every `MaterialLibrary` row drive appearance purely by producing a `Seq<LobeWeight>`.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct LobeWeight(BsdfLobe Lobe, UnitInterval Weight);

public sealed record LayeredBsdf {
    private LayeredBsdf(Seq<LobeWeight> lobes) => Lobes = lobes;
    public Seq<LobeWeight> Lobes { get; }

    public static Fin<LayeredBsdf> Of(Seq<LobeWeight> lobes, Op key) {
        double total = lobes.Sum(static l => l.Weight.Value);
        return lobes.IsEmpty || total <= RhinoMath.ZeroTolerance
            ? Fin.Fail<LayeredBsdf>(MaterialFault.Parameter(key, "<lobe-weights-degenerate>"))
            : Fin.Succ(new LayeredBsdf(lobes.Map(l => l with { Weight = UnitInterval.Create(l.Weight.Value / total) })));
    }

    // State-threaded static folds — the per-bounce entries allocate no closure (the same closure-free hot-loop law the
    // lobe Switch names; a lambda capturing lo/li here re-introduces the allocation the static arms deleted).
    public RgbSpectrum Evaluate(ShadingFrame frame, Direction wo, Direction wi) {
        LocalVector lo = frame.ToLocal(wo), li = frame.ToLocal(wi);
        return Lobes.Fold((Acc: RgbSpectrum.Black, Lo: lo, Li: li),
            static (s, lw) => (s.Acc.Add(lw.Lobe.Evaluate(s.Lo, s.Li).Scale(lw.Weight.Value)), s.Lo, s.Li)).Acc;
    }

    public double Pdf(ShadingFrame frame, Direction wo, Direction wi) {
        LocalVector lo = frame.ToLocal(wo), li = frame.ToLocal(wi);
        return Lobes.Fold((Acc: 0.0, Lo: lo, Li: li),
            static (s, lw) => (s.Acc + lw.Weight.Value * lw.Lobe.Pdf(s.Lo, s.Li), s.Lo, s.Li)).Acc;
    }

    // One-sample MIS over the weight CDF: pick a lobe proportionally to weight via uLobe (a state-threaded fold — never
    // a mutable accumulate-and-break; the interval is HALF-OPEN [cdfBefore, cdfBefore+weight) so uLobe = 0.0 — a value
    // every stratified/QMC sampler emits — picks the FIRST lobe and a zero-weight lobe is never picked; the fp tail past
    // the last cumulative defaults to the last lobe), hand the chosen lobe the RESCALED remainder (uLobe − cdfBefore)/weight
    // as its uc choice variable so stratification survives both choices, then re-weight the returned pdf/value as the
    // WEIGHT-AVERAGED pdf and the FULL layered value across ALL lobes (the balance heuristic) — unbiased, low-variance.
    public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1, Op key) {
        LocalVector lo = frame.ToLocal(wo);
        (double U, double Cumulative, double Before, LobeWeight Pick) pick = Lobes.Fold(
            (U: uLobe, Cumulative: 0.0, Before: 0.0, Pick: Lobes.Last),
            static (s, lw) => s.Cumulative > s.U || s.Cumulative + lw.Weight.Value <= s.U
                ? (s.U, s.Cumulative + (s.Cumulative > s.U ? 0.0 : lw.Weight.Value), s.Before, s.Pick)
                : (s.U, s.Cumulative + lw.Weight.Value, s.Cumulative, lw));
        double uc = Math.Clamp((uLobe - pick.Before) / Math.Max(RhinoMath.ZeroTolerance, pick.Pick.Weight.Value), 0.0, 1.0);
        return pick.Pick.Lobe.Sample(lo, uc, u0, u1, key).Bind(sample => {
            double mixedPdf = Lobes.Fold((Acc: 0.0, Lo: lo, Wi: sample.Direction),
                static (s, lw) => (s.Acc + lw.Weight.Value * lw.Lobe.Pdf(s.Lo, s.Wi), s.Lo, s.Wi)).Acc;
            RgbSpectrum mixedValue = Lobes.Fold((Acc: RgbSpectrum.Black, Lo: lo, Wi: sample.Direction),
                static (s, lw) => (s.Acc.Add(lw.Lobe.Evaluate(s.Lo, s.Wi).Scale(lw.Weight.Value)), s.Lo, s.Wi)).Acc;
            return mixedPdf > 0.0
                ? Fin.Succ(sample with { Pdf = mixedPdf, Value = mixedValue })
                : Fin.Fail<LobeSample>(MaterialFault.Graph(key, "<degenerate-mixed-pdf>"));
        });
    }
}
```

## [06]-[KERNEL_SEAMS]

The lowering/grounding half — `SpectralUpsample` (RGB→SPD + measured-illuminant reduction), `ToneMap` (ACES RRT/ODT + scene-referred operators), `ConductorMetal` (the measured complex-IOR rows), and `SlabStack` (the OpenPBR Surface 1.1 stack-of-slabs) — is the `surface#SPECTRAL_UPSAMPLE`/`#TONE_MAP`/`#CONDUCTOR_IOR`/`#OPENPBR_SLAB` page, split out under the per-page depth budget so the kernel page owns frame-local shading and the surface page owns the OpenPBR construction. The two pages share the `[01]-[SHADING_FRAME]` `MaterialFault` band (`FaultBand.Material`) and the `SpectralBand` band-centre vocabulary declared once here and composed by the surface page; the `[04]-[LOBE_FAMILY]` `RgbSpectrum`/`ComplexIor` validated carriers and `BsdfLobe` closed set and the `[05]-[LAYERED_COMPOSITION]` `LayeredBsdf.Of` fold are read by `surface#OPENPBR_SLAB` `SlabStack.ToLayered`, the `[03]-[MICROFACET_KERNEL]` `Microfacet.FresnelConductor(cosI, ComplexIor)` overload by `surface#CONDUCTOR_IOR`, and the `[04]-[LOBE_FAMILY]` `MultiScatter.DirectionalAlbedo` by the `SlabStack` albedo-scaling. A `MaterialParameters` row lowers through `surface#OPENPBR_SLAB` `SlabStack.Lower` to the formal stack and `ToLayered` collapses it to the one `LayeredBsdf` weighted fold the integrator shades here — the slab algebra the construction, the lobe math single-sourced on this page.

## [07]-[RESEARCH]

- [WHITE_FURNACE_HARNESS] (tier-2): the energy-conservation harness that integrates each lobe's directional albedo over the hemisphere against a uniform unit-radiance environment and asserts the reflected energy never exceeds 1 (no energy created) and, for a lossless conductor (F≡1), equals 1 to a Monte-Carlo tolerance (no energy destroyed). The harness is the numeric proof that `MultiScatter.KullaConty` recovers exactly the energy the single-scatter Smith model loses at high roughness — the analytic `DirectionalAlbedo` Karis fit supplies `E(μ)` and the `HemisphericalAlbedo` 8-node quadrature closes `Eavg = 2∫E(μ)μ dμ` to quadrature accuracy, so this is a probe over a closed-form result, not an open gate. The harness samples `BsdfLobe.Sample`, accumulates `value·cosθ/pdf` over N directions, and bounds the furnace residual at the `Conductor`/`Dielectric` roughness sweep (α ∈ {0.1, 0.3, 0.5, 0.7, 0.9}); the residual to retire is whether the `surface#SPECTRAL_UPSAMPLE` 10-control-point Smits basis round-trips every `graph#MATERIAL_LIBRARY` row to an in-gamut `SurfaceShade` after the `MapToRgbGamut(GamutMap.OklchChromaReduction)` chroma reduction, since a saturated primary can push the upsampled reflectance against the [0,1] energy bound and the harness measures the perceptual ΔE the chroma-reduction costs. Reciprocity (`f(wo,wi) == f(wi,wo)`) and GGX NDF normalization (`∫ D(h)·cosθh dω == 1`) ride the same harness as exact-equality and unit-integral assertions over the `Microfacet` kernel.
- [POSITION_FREE_MULTI_SCATTER]: the multi-bounce energy is the closed-form Kulla-Conty term today; the unbiased route is the position-free Monte Carlo / atomic-decomposition methods (Belcour; Guo-Hasan-Zhao; d'Eon-Bitterli-Zeltner, SIGGRAPH Asia 2024) that evaluate the layered stack's multi-bounce transport without the closed-form approximation. The probe is whether the path tracer's per-bounce cost admits the position-free random walk over the OpenPBR slab stack as the high-fidelity path, with the Kulla-Conty term the fast path; the seam is `Rasm.AppUi/Render/pathtrace#PATH_TRACE`.
