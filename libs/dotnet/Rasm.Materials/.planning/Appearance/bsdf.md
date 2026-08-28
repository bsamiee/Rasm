# [MATERIALS_BSDF]

THE FRAME-LOCAL SHADING KERNEL. The closed BSDF lobe family and its frame-local microfacet kernel: ONE `BsdfLobe` `[Union]` of seven physical lobes (diffuse · conductor · dielectric · sheen · clearcoat · subsurface · thin-film) under ONE `Evaluate`/`Sample`/`Pdf` contract, ONE generic `Microfacet<T>` GGX/Smith/Fresnel kernel over the `System.Numerics` generic-math floor, ONE `MultiScatter` owner carrying both the Kulla-Conty closed-form energy compensation and its unbiased position-free walk, and ONE `LayeredBsdf` weighted-composition fold every material drives by parameter row. The page owns the `ShadingFrame`, `Scalar<T>`, `LocalVector<T>`, `SpectralBand`, `BsdfLobe`, and `LayeredBsdf` shading surfaces, the folder-wide `ComparerAccessors.StringOrdinal` ordinal-key pin (the Thinktecture accessor selected once here as policy, never a local re-mint), the validated `RgbSpectrum`/`ComplexIor` reflectance carriers, and the `MaterialFault` union (band `FaultBand.Appearance` — allocation and disjointness are type-enforced from the kernel `Rasm/Domain/results#[04]-[FAULT_BAND]` ledger). The color-science lowering/grounding half — `SpectralUpsample` RGB→SPD, `ToneMap` ACES, `ConductorMetal` measured complex-IOR rows, and the `SlabStack` OpenPBR Surface 1.1 stack-of-slabs — is the sibling `surface#SPECTRAL_UPSAMPLE`/`#TONE_MAP`/`#CONDUCTOR_IOR`/`#OPENPBR_SLAB` page, split out under the per-page depth budget, composing this page's `MaterialFault` band, `ComparerAccessors.StringOrdinal`, `SpectralBand`, lobe carriers, and `LayeredBsdf.Of` fold. A material is NEVER a lobe subtype: `LayeredBsdf` carries the lobe weights and per-lobe parameters a `MaterialParameters` row supplies, so metal, glass, plastic, skin, fabric, car paint, and wax are weightings of this one closed set, never new lobe types or per-material BSDF classes. The lobe composition is the OpenPBR Surface 1.1 stack-of-slabs (`fuzz` · `coat` · `thin-film` modifier · base substrate mixing a conductor slab against a dielectric base) realized as the `surface#OPENPBR_SLAB` `SlabStack` algebra whose `ToLayered` collapse lowers to this page's `LayeredBsdf` fold; the renderer (`graph#MATERIAL_GRAPH` sink, shaded by the CPU path-trace integrator at the `Rasm.AppUi/Render/pathtrace#PATH_TRACE` boundary and the GPU shading pass at the `Rasm.AppUi/Render/shading#SURFACE_SHADE` boundary — one BSDF, two evaluators) shades FROM `LayeredBsdf.Sample`/`Evaluate`/`Pdf` and never re-derives lobe math.

## [01]-[INDEX]

- [02]-[SHADING_FRAME]: the `ShadingFrame` local-frame transform, the generic `LocalVector<T>` z-up triple over its `Scalar<T>` anchor owner, the `SpectralBand` band-centre vocabulary, the `MaterialFault` union on the `FaultBand.Appearance` ledger row, and the folder's `ComparerAccessors.StringOrdinal` ordinal-key pin.
- [03]-[MICROFACET_KERNEL]: the generic-math `Microfacet<T>` kernel — Fresnel (Schlick plus exact dielectric/conductor), the GGX/Trowbridge-Reitz NDF, Smith height-correlated masking, and the VNDF sampler with its paired reflect density.
- [04]-[LOBE_FAMILY]: the `BsdfLobe` `[Union]`, the validated `RgbSpectrum`/`ComplexIor` carriers, the per-lobe `Evaluate`/`Sample`/`Pdf` contract, the Kulla-Conty multi-scatter compensation over its derived energy table, and the unbiased position-free walk beside it.
- [05]-[LAYERED_COMPOSITION]: the `LayeredBsdf` weighted-lobe fold, the MIS-balanced sample/pdf, and the material-is-a-row boundary.
- [06]-[KERNEL_BOUNDARIES]: the `surface#SPECTRAL_UPSAMPLE`/`#TONE_MAP`/`#CONDUCTOR_IOR`/`#OPENPBR_SLAB` lowering page that composes this kernel's `MaterialFault`/`ComparerAccessors.StringOrdinal`/`SpectralBand`/carriers/`LayeredBsdf.Of`.
- [07]-[BSDF_ORACLE]: the `BsdfProbe` energy-proof roster and the derived `BsdfOracle` fixture table asserting the white-furnace balance, reciprocity, and NDF normalization against the kernels' own closed forms.

## [02]-[SHADING_FRAME]

- Owner: `ShadingFrame` over the composed `Rasm.Numerics.VectorFrame`; `MaterialFault` `[Union]` on the `FaultBand.Appearance` ledger row via the kernel `[FaultCase]`/`Fault` floor; the `ComparerAccessors.StringOrdinal` ordinal-key pin (Thinktecture's accessor, selected once as the folder's key-comparison policy).
- Entry: `public static Fin<ShadingFrame> Of(VectorFrame frame, Context context, Direction outgoing)` — `Fin<T>` aborts when the outgoing direction is degenerate in the local frame; `ToLocal`/`ToWorld` are the only world↔tangent transforms and `CosTheta`/`Sin2Theta`/`TanTheta`/`CosPhi`/`SinPhi` read the local z-up convention every lobe kernel shares. The frame carries the integrator's `Context` so `ToWorld` routes the unitized world direction through the PUBLIC `Direction.Of(Vector3d, Context)` overload (the `(Vector3d, double)` overload is `internal` to `Rasm` and cannot bind cross-assembly).
- Packages: Rasm (project — `Rasm.Numerics` `VectorFrame`/`Direction`/`Dimension`/`UnitInterval`), Rasm.Element (project — the `FaultBand` allocation ledger generated identity reads), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new fault is one `MaterialFault` case; a new trig accessor is one expression-bodied member on the frame; zero new surface. The frame NEVER re-mints `VectorFrame` — it wraps the composed `Plane` and reads `Vector3d` projections through it.
- Law: this page declares NO raw validity predicate. `LobeSample` implements the kernel `Domain/results#…` `IValidityEvidence` and folds `ValidityClaim.All` exactly as the folder's other four `IsValid` sites already do, so the corpus acceptance oracle probes a drawn sample through its own arm rather than through a member only this page knows to call. The prior `Pdf > 0.0 && double.IsFinite(Pdf)` was the kernel `ValidityClaim.Positive` row re-derived by hand — that row carries BOTH conjuncts, screening the host unset sentinel a bare finiteness test admits — and the claim's implicit `bool` conversion keeps every reading site spelled as one predicate.
- Law: the microfacet, multi-scatter, and hemispherical-albedo loops take the page's NAMED `[EXPRESSION_SPINE]` kernel exemption and are not folded. Each is a fixed-extent numerical walk carrying loop-carried state a fold cannot express without materializing an intermediate per iteration — the VNDF draw, the Kulla-Conty energy walk, and the type-init albedo table — and every one runs per SAMPLE on the integrator's hottest path, where a `Seq` allocation per bounce is the cost the frozen kernels exist to avoid. Every admission, dispatch, and egress surface on the page stays expression-bodied, so the exemption names the four kernels and nothing else.
- Boundary: `ShadingFrame` is the page's declared boundary capsule over `Rasm.Numerics` — host `Vector3d`/`Plane` access stays inside it and lobe kernels see only local-frame `LocalVector<T>` triples at the page's own `double` instantiation (z is the surface normal, the half-vector and incident/outgoing live in this basis); the z-up tangent convention is stated here once for every lobe so no lobe re-derives `cosθ = w.Z`; `MaterialFault` closes the appearance recovery vocabulary on `FaultBand.Appearance`, and each direct case lifts onto `Fin<T>` or `Validation<Error,T>` without a wrapper; every refusal constructs `Gamut`, `Parameter`, or `Graph` directly, so a lobe never returns a non-finite shade outward; `ComparerAccessors.StringOrdinal` is the ordinal comparer the `MaterialLibrary` and `ToneOperator` tables key through.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Linq;
using System.Numerics;
using LanguageExt;
using MathNet.Numerics;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using Rasm.Numerics;
using TinyEXR.V3;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Bsdf;

// --- [TYPES] ---------------------------------------------------------------------------
internal static class Scalar<T> where T : INumber<T> {
    public static readonly T Half = T.CreateChecked(0.5);
    public static readonly T Two = T.CreateChecked(2.0);
    public static readonly T Four = T.CreateChecked(4.0);
    public static readonly T Tolerance = T.CreateChecked(RhinoMath.ZeroTolerance);
    public static readonly T AlphaFloor = T.CreateChecked(1e-4);
}

public readonly record struct LocalVector<T>(T X, T Y, T Z)
    where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T> {
    public T CosTheta => Z;
    public T Cos2Theta => Z * Z;
    public T Sin2Theta => T.Max(T.Zero, T.One - (Z * Z));
    public T SinTheta => T.Sqrt(Sin2Theta);
    public T Tan2Theta => Sin2Theta / T.Max(Scalar<T>.Tolerance, Cos2Theta);
    public T TanTheta => T.CopySign(T.Sqrt(Tan2Theta), Z);
    public T CosPhi => SinTheta <= Scalar<T>.Tolerance ? T.One : T.Clamp(X / SinTheta, -T.One, T.One);
    public T SinPhi => SinTheta <= Scalar<T>.Tolerance ? T.Zero : T.Clamp(Y / SinTheta, -T.One, T.One);
    public LocalVector<T> Reflect() => new(-X, -Y, Z);
    public T Dot(LocalVector<T> o) => (X * o.X) + (Y * o.Y) + (Z * o.Z);
    public LocalVector<T> Add(LocalVector<T> o) => new(X + o.X, Y + o.Y, Z + o.Z);
    public LocalVector<T> Scale(T s) => new(X * s, Y * s, Z * s);
    public LocalVector<T> Normalize() =>
        T.Sqrt((X * X) + (Y * Y) + (Z * Z)) switch { var n => n > Scalar<T>.Tolerance ? new LocalVector<T>(X / n, Y / n, Z / n) : Normal };
    public LocalVector<T> RotateZ(T radians) =>
        T.IsZero(radians) ? this : T.SinCos(radians) switch { var (s, c) => new LocalVector<T>((X * c) - (Y * s), (X * s) + (Y * c), Z) };
    public bool SameHemisphere(LocalVector<T> o) => Z * o.Z > T.Zero;
    public static readonly LocalVector<T> Normal = new(T.Zero, T.Zero, T.One);
}

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

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Appearance;
    private MaterialFault(string detail) { Detail = detail; }
    public string Detail { get; }
    [FaultCase(0)] public sealed partial record Gamut(string Detail) : MaterialFault(Detail);
    [FaultCase(1)] public sealed partial record Parameter(string Detail) : MaterialFault(Detail);
    [FaultCase(2)] public sealed partial record Graph(string Detail) : MaterialFault(Detail);

    public override string Message => Switch(
        state: Detail,
        gamut: static (detail, c) => $"Shade out of gamut under '{c.Key}': {detail}.",
        parameter: static (detail, c) => $"Degenerate appearance parameter under '{c.Key}': {detail}.",
        graph: static (detail, c) => $"Degenerate shading frame under '{c.Key}': {detail}.");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ShadingFrame(VectorFrame Frame, Context Context) {
    public static Fin<ShadingFrame> Of(VectorFrame frame, Context context, Direction outgoing) {
        Plane basis = frame.Value;
        LocalVector<double> wo = Project(basis, outgoing.Value);
        return Math.Abs(wo.Z) > RhinoMath.ZeroTolerance
            ? Fin.Succ(new ShadingFrame(frame, context))
            : Fin.Fail<ShadingFrame>(new MaterialFault.Graph("<degenerate-local-direction>"));
    }
    public LocalVector<double> ToLocal(Direction world) => Project(Frame.Value, world.Value);
    public Fin<Direction> ToWorld(LocalVector<double> local) {
        Plane b = Frame.Value;
        Vector3d w = (local.X * b.XAxis) + (local.Y * b.YAxis) + (local.Z * b.ZAxis);
        return Direction.Of(w, Context);
    }
    private static LocalVector<double> Project(Plane basis, Vector3d w) => new(w * basis.XAxis, w * basis.YAxis, w * basis.ZAxis);
}
```

## [03]-[MICROFACET_KERNEL]

- Owner: the generic `Microfacet<T>` kernel — GGX NDF · Smith height-correlated masking · the Schlick/dielectric/conductor Fresnel family · the Heitz VNDF sampler and its reflect density; `Scalar<T>` the per-instantiation numeric anchor owner.
- Entry: `public static T Ndf(LocalVector<T> h, T alphaX, T alphaY)` and the sibling `MaskingShadowing`/`Masking`/`FresnelDielectric`/`FresnelConductor`/`FresnelSchlick`/`SampleVisibleNormal`/`VisibleNormalPdf`/`ReflectPdf` — pure values; the NDF takes only the half-vector and anisotropic roughness `(alphaX, alphaY)`, and the isotropic case is `alphaX == alphaY`, never a second isotropic kernel.
- Packages: Thinktecture.Runtime.Extensions, BCL inbox generic math (`System.Numerics` — the kernel is frame-local and host-free; the public `Rasm.Numerics.Direction.Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted)` composes at the world boundary — see LOBE_FAMILY).
- Growth: a new distribution (Beckmann) is one `Microfacet<T>` static the owning lobe case routes; a new Fresnel term is one `Microfacet<T>` static a lobe's fresnel policy names — the lobe case IS the Fresnel discriminant (conductor/dielectric/thin-film run exact, clearcoat runs Schlick), so a parallel mode enum re-describing that closed choice is the deleted form; a new SCALAR CARRIER is one instantiation and zero new bodies — the landed three are `double` (every lobe), the `float` GPU-parity peer, and `Dual<ddouble>`, the differentiation carrier the acquisition residual composes so the synthetic capture and the residual row evaluate the SAME body; zero new surface. The roughness→alpha remap (`alpha = roughness²`) is the one Disney-convention remap every lobe reads.
- Law: the kernel is GENERIC over `T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T>` and the constraint set is declared ONCE on the type rather than re-spelled per member. `double` is the instantiation every lobe here binds and the one the integrator shades through; the generic form is what lets a `float` parity run against the `Raster/gpu#WGSL_KERNEL` twin read the SAME kernel body instead of a second dialect a fixture would have to reconcile — a half-converted kernel carrying two scalar dialects over one geometry is worse than either whole. `Scalar<T>` owns every numeric anchor per instantiation: the anchors are DOMAIN floors — the frame's degeneracy tolerance, the mirror-roughness floor, the fabric floor — so they state one physical bound at every `T` rather than tracking each mantissa. Spectral quantities are NOT generic: `RgbSpectrum`, `ComplexIor`, and the MathNet quadrature the multi-scatter term folds through are `double`-anchored by their own carriers, so the generic boundary is exactly the scalar geometry-and-Fresnel layer and the spectral layer composes it at `double`.
- Law: every kernel DIVISION is floored at its own domain anchor and the floor lives at the kernel, never at a call site that must remember it — `Ndf` floors its alpha pair, `Tan2Theta` its cosine, `VisibleNormalPdf` its view cosine, `ReflectPdf` its half-vector cosine, and `FresnelConductor` its two polarization denominators. A grazing pair or a zero-roughness consumer lobe therefore shades the physical limit instead of minting the non-finite value the validated carrier throws on inside a partitioned sweep no error channel covers, and a `pdf > 0` gate downstream no longer stands in for a finiteness test it never performed.
- Boundary: the NDF is GGX/Trowbridge-Reitz in anisotropic form `D = 1 / (π·αx·αy·((hx/αx)² + (hy/αy)² + hz²)²)`, reducing to the isotropic `D = α² / (π·(cos²θh·(α²−1)+1)²)` when `αx==αy` — one body, both modes; the masking-shadowing is the Smith height-correlated `G2 = 1 / (1 + Λ(wo) + Λ(wi))` with the GGX `Λ`, NEVER the separable `G1(wo)·G1(wi)` (separable overestimates correlated occlusion and breaks the white-furnace test); the Fresnel family is lobe-selected — the exact unpolarized dielectric term is SIDE-AWARE (`cosI < 0` flips `η` to `1/η`, so an interior ray reads its true reflectance and interior TIR reads 1 — `|cosI|` under the exterior `η` is the deleted form that missed interior TIR), the per-band conductor term grounds metals and rides the `ComplexIor` carrier that owns the measured pair, and `F0 + (1−F0)(1−cosθ)⁵` Schlick carries the fast coated path; the GGX visible-normal-distribution sample (Heitz 2018) is the one importance-sampling routine the conductor/dielectric/clearcoat/thin-film lobes share and `ReflectPdf` its one paired density, so the four samplers and the four pdf reads cannot drift by a Jacobian.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Microfacet<T>
    where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T> {
    public static T AlphaOf(T roughness) => T.Max(Scalar<T>.AlphaFloor, roughness * roughness);

    public static T Ndf(LocalVector<T> h, T alphaX, T alphaY) {
        if (h.CosTheta <= T.Zero) { return T.Zero; }
        (T ax, T ay) = (T.Max(Scalar<T>.AlphaFloor, alphaX), T.Max(Scalar<T>.AlphaFloor, alphaY));
        T hx = h.X / ax, hy = h.Y / ay, hz = h.Z;
        T k = (hx * hx) + (hy * hy) + (hz * hz);
        return T.One / (T.Pi * ax * ay * k * k);
    }

    private static T Lambda(LocalVector<T> w, T alphaX, T alphaY) =>
        ((w.CosPhi * w.CosPhi * alphaX * alphaX) + (w.SinPhi * w.SinPhi * alphaY * alphaY)) switch {
            var alpha2 => (T.Sqrt(T.One + (alpha2 * w.Tan2Theta)) - T.One) * Scalar<T>.Half,
        };

    public static T MaskingShadowing(LocalVector<T> wo, LocalVector<T> wi, T alphaX, T alphaY) =>
        T.One / (T.One + Lambda(wo, alphaX, alphaY) + Lambda(wi, alphaX, alphaY));

    public static T Masking(LocalVector<T> w, T alphaX, T alphaY) => T.One / (T.One + Lambda(w, alphaX, alphaY));

    public static T FresnelSchlick(T cosTheta, T f0) =>
        T.Clamp(T.One - cosTheta, T.Zero, T.One) switch {
            var m => f0 + ((T.One - f0) * (m * m * m * m * m)),
        };

    public static T FresnelDielectric(T cosI, T eta) {
        T ci = T.Clamp(cosI, -T.One, T.One);
        if (ci < T.Zero) { eta = T.One / eta; ci = -ci; }
        T sin2T = (T.One - (ci * ci)) / (eta * eta);
        if (sin2T >= T.One) { return T.One; }
        T ct = T.Sqrt(T.One - sin2T);
        T rParl = ((eta * ci) - ct) / ((eta * ci) + ct);
        T rPerp = (ci - (eta * ct)) / (ci + (eta * ct));
        return Scalar<T>.Half * ((rParl * rParl) + (rPerp * rPerp));
    }

    public static T FresnelConductor(T cosI, T eta, T k) {
        T ci = T.Clamp(T.Abs(cosI), T.Zero, T.One), ci2 = ci * ci;
        T sin2 = T.One - ci2;
        T eta2 = eta * eta, k2 = k * k;
        T t0 = eta2 - k2 - sin2;
        T a2b2 = T.Sqrt(T.Max(T.Zero, (t0 * t0) + (Scalar<T>.Four * eta2 * k2)));
        T t1 = a2b2 + ci2;
        T a = T.Sqrt(T.Max(T.Zero, Scalar<T>.Half * (a2b2 + t0)));
        T t2 = Scalar<T>.Two * a * ci;
        T rs = (t1 - t2) / T.Max(Scalar<T>.Tolerance, t1 + t2);
        T t3 = (ci2 * a2b2) + (sin2 * sin2);
        T t4 = t2 * sin2;
        T rp = rs * (t3 - t4) / T.Max(Scalar<T>.Tolerance, t3 + t4);
        return Scalar<T>.Half * (rp + rs);
    }

    public static LocalVector<T> SampleVisibleNormal(LocalVector<T> wo, T alphaX, T alphaY, T u0, T u1) {
        LocalVector<T> vh = new LocalVector<T>(alphaX * wo.X, alphaY * wo.Y, wo.Z).Normalize();
        T lensq = (vh.X * vh.X) + (vh.Y * vh.Y);
        LocalVector<T> t1 = lensq > T.Zero
            ? new LocalVector<T>(-vh.Y, vh.X, T.Zero).Scale(T.One / T.Sqrt(lensq))
            : new LocalVector<T>(T.One, T.Zero, T.Zero);
        LocalVector<T> t2v = new((vh.Y * t1.Z) - (vh.Z * t1.Y), (vh.Z * t1.X) - (vh.X * t1.Z), (vh.X * t1.Y) - (vh.Y * t1.X));
        T r = T.Sqrt(u0), phi = Scalar<T>.Two * T.Pi * u1;
        T p1 = r * T.Cos(phi), p2 = r * T.Sin(phi);
        T s = Scalar<T>.Half * (T.One + vh.Z);
        p2 = ((T.One - s) * T.Sqrt(T.Max(T.Zero, T.One - (p1 * p1)))) + (s * p2);
        T pz = T.Sqrt(T.Max(T.Zero, T.One - (p1 * p1) - (p2 * p2)));
        LocalVector<T> nh = t1.Scale(p1).Add(t2v.Scale(p2)).Add(vh.Scale(pz));
        return new LocalVector<T>(alphaX * nh.X, alphaY * nh.Y, T.Max(Scalar<T>.Tolerance, nh.Z)).Normalize();
    }

    public static T VisibleNormalPdf(LocalVector<T> wo, LocalVector<T> h, T alphaX, T alphaY) =>
        Masking(wo, alphaX, alphaY) * T.Abs(wo.Dot(h)) * Ndf(h, alphaX, alphaY) / T.Max(Scalar<T>.Tolerance, T.Abs(wo.CosTheta));

    public static T ReflectPdf(LocalVector<T> wo, LocalVector<T> h, T alphaX, T alphaY) =>
        VisibleNormalPdf(wo, h, alphaX, alphaY) / (Scalar<T>.Four * T.Max(Scalar<T>.Tolerance, T.Abs(wo.Dot(h))));
}
```

## [04]-[LOBE_FAMILY]

- Owner: `BsdfLobe` `[Union]` closed lobe family; `LobeSample` the typed sample and its one `Of` mint; `MultiScatter` the energy-recovery owner over both its arms.
- Entry: `public RgbSpectrum Evaluate(LocalVector<double> wo, LocalVector<double> wi)` · `public Fin<LobeSample> Sample(LocalVector<double> wo, double uc, double u0, double u1)` · `public double Pdf(LocalVector<double> wo, LocalVector<double> wi)` — the three-method contract every lobe case implements through one total `Switch`; the lobe is frame-local, so `Evaluate`/`Pdf` read the local-frame `LocalVector<double>` triples the integrator transforms once, `Sample` carries its `MaterialFault` channel, and `uc` is the lobe-local CHOICE variable (the dielectric reflect/transmit lottery) decorrelated from the `(u0, u1)` pair that shapes the half-vector; `RgbSpectrum` is the validated three-band `[ComplexValueObject]` reflectance carrier gating non-finite/negative channels once at `Create`, NEVER a host color type at an interior signature and NEVER an unvalidated raw triple a downstream `IsFinite` re-checks.
- Packages: Rasm (project — `Direction.Reflect`/`Refract`, and the `RgbProfile.Acescg.Geometry` AP1 chromaticity column the type-init luminance derivation reads), MathNet.Numerics (composed for ONE quadrature — `Integrate.GaussLegendre` closes `MultiScatter.HemisphericalAlbedo` at the `AlbedoNodes` order), TinyEXR.NET (composed for ONE type-init read — `ImageProcessing.GetLuminanceWeights(Chromaticities?)` derives the AP1 luminance triple from that geometry; the package is unreachable from every per-shade member and mints no container here), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Law: energy recovery carries TWO arms on one owner and they are not interchangeable spellings of one thing. `MultiScatter.KullaConty` is the closed-form fast path every lobe folds by default, its `E(μ)` the analytic directional-albedo fit and its `Eavg` a table DERIVED at type init by the MathNet Legendre rule over that same fit — one owner, so the `[07]-[BSDF_ORACLE]` furnace expectation and the shading path read one number instead of an exact fold and an interpolated twin drifting inside the tolerance, and the per-sample quadrature the compensation term used to pay for a value the roughness already fixed is gone. `MultiScatter.PositionFree` is the UNBIASED arm the integrator selects when its per-bounce budget affords one: a random walk between microfacets of the same GGX interface whose state is `(direction, depth)` under the position-free solid-angle × line measure, so it runs with no surface position and lands on the shading point the single-scatter term already evaluates at. Its density is a STOCHASTIC ESTIMATE — a walk cannot evaluate the density of the path it drew — and multiple importance sampling against an approximate pdf is provably unbiased, so the sample composes with the `[05]` balance heuristic rather than forking the estimator. Its budget is a RUSSIAN-ROULETTE continuation probability and the surviving throughput divides by it, so the expectation is identical at every budget and a cheap walk is noisier rather than dimmer; a hard max-bounce cap is the REJECTED form, truncating the Neumann series and biasing exactly the high-roughness band the arm exists to resolve.
- Growth: a new physical lobe is one `BsdfLobe` case admitted ONLY when no parameterization of the existing seven reproduces the measured physics — and then it serves ALL materials, never one material; a new material is NEVER a lobe. The lobe set is closed at seven: diffuse, conductor, dielectric, sheen, clearcoat, subsurface, thin-film. The `[9]-[OPENPBR_SLAB]` `SlabStack` is the realized formal layering construction over these lobes — the fuzz slab lowers to the `Sheen` lobe and the albedo-scaling operators compose the stack the `LayeredBsdf` weighted fold the integrator shades collapses from.
- Boundary: every lobe `Evaluate` returns the BSDF value times nothing — the cosine-weight and division by pdf live in the integrator (`LayeredBsdf.Sample`), so a lobe is the pure `f(wo, wi)` and never folds in the geometry term twice; the `Conductor` and `ThinFilm` lobes carry one `ComplexIor` `[ComplexValueObject]` band (its `Eta`/`K` two validated `RgbSpectrum` carriers) the carrier's own `ComplexIor.Fresnel(cosI)` per-band read answers from (`FresnelNormal` is that same term at `cosθ = 1`, so no second closed form stands beside it), never a parallel `(Eta, K)`/`(BaseEta, BaseK)` triple pair; the four glossy-reflect lobes (conductor · dielectric-reflect · clearcoat · thin-film) evaluate through ONE `MicrofacetReflect` Cook-Torrance skeleton — the half-vector canonicalized to the geometric upper hemisphere (an interior both-below pair otherwise zeroes the NDF and shades compensation-only), `fresnel(state, wo·h)·D·G₂/(4|cosθo||cosθi|)` over the SIGNED half-vector cosine so the side-aware dielectric term reads interior reflectance, plus a per-lobe compensation policy, hemisphere and grazing degeneracy guarded once — each lobe supplying ONLY its Fresnel term (`ComplexIor.Fresnel`, the white-scaled exact dielectric, the weight-scaled `Schlick` `F0` 0.04 coat, the interference-modulated base-conductor film) and its Kulla-Conty add (`Black` for clearcoat/thin-film), so a per-lobe `D·G₂` re-inline is the deleted form; the conductor and dielectric lobes drive `Microfacet<double>` with their `(alphaX, alphaY)` and the exact conductor/dielectric Fresnel kernels, the clearcoat is a fixed-`Schlick`-`F0`-0.04 dielectric GGX layer over the base reusing that same reflect arm at its own alpha pair, the sheen is the Estevez-Kulla inverted-Gaussian retroreflective fabric lobe, and the thin-film is the Belcour-Barla spectral interference term modulating the base Fresnel; all four glossy-reflect lobes carry their anisotropy REFERENCE as a `Rotation` radian column beside an `(AlphaX, AlphaY)` pair — the coat and its thin-film mix included, so a brushed lacquer keeps its grain at every `thin_film_weight` — and the skeleton rotates `wo`/`wi` by `−Rotation` about local Z at every entry — evaluation, pdf, and the VNDF draw (whose sampled direction rotates back before it leaves) — so the half-vector, the Smith `Λ` azimuth, and the sample share one basis while `ShadingFrame` stays the geometric basis every isotropic sibling reads unrotated, and a per-lobe rotation is a lobe fact rather than a frame the whole stack turns with; the `Dielectric` `SpecularTint` is the OpenPBR `specular_color` and multiplies BOTH the single-scatter Fresnel and its Kulla-Conty compensation (tinting one alone moves energy between the two and the furnace residual then reads as a tint error), while the `Clearcoat` `Tint` is a BODY absorption that never enters the achromatic dielectric reflection and instead rides the `Transmitted` throughput onto every layer beneath — absorption is opt-in on the case that carries a body tint, so the root's `Transmitted` default is `White` rather than six identical `Switch` arms; the AP1 luminance triple is DERIVED once at type init from the kernel `RgbProfile.Acescg.Geometry` chromaticity column through `ImageProcessing.GetLuminanceWeights`, so `RgbSpectrum.LuminanceWeights` is the ONE owner the `texture#TEXTURE_UV` `ShadeVec4` and `graph#MATERIAL_GRAPH` `PortValue` luminance projections read and a hand-typed decimal triple beside it is the deleted form; the diffuse lobe carries the Oren-Nayar roughness term (Lambert is `roughness == 0`, the `a→1/b→0` limit), so one diffuse case spans matte-to-rough, never a separate Lambert and Oren-Nayar type; the `Subsurface` lobe is the in-pixel-footprint diffusion limit Burley refactors the dielectric SSS BRDF into — a Lambert base modulated by the two rough-surface Fresnel transmission factors `(1−0.5·F_L)(1−0.5·F_V)`, reciprocal and energy-bounded, the albedo carrying the scatter colour directly (the normalized-diffusion no-inversion guarantee), and the wide-radius transport is the separable BSSRDF the integrator samples through `MultiScatter.SeparableProfile` (Burley's `Rd(r)` normalized over the disk `∫Rd(r)·2πr dr = 1`) by SURFACE distance — the spatial profile is NEVER multiplied as a directional BRDF, since `Rd(r)` integrates to one over area not solid angle; the multi-scatter energy compensation is the Kulla-Conty term added to the conductor/dielectric single-scatter, recovering the energy the single-scatter Smith model loses at high roughness — without it the white-furnace test fails above α≈0.5, and its Fresnel response is the diffused-bounce geometric series `F_avg·E_avg/(1−F_avg(1−E_avg))`, never `F_avg²·(1−E_avg)/(…)` which destroys energy; the lobe sampler is frame-local and host-free — `LobeSample.Direction` is a `LocalVector<double>`, the dielectric transmission runs the same exact Snell formula `Rasm.Numerics.Direction.Refract` owns (eta·d + (eta·cosI−√k)·n, TIR-rejected) so the math is single-sourced; the WORLD reflected/refracted ray the path tracer needs for the next bounce is the integrator's `ShadingFrame.ToWorld` composition, and when the renderer prefers the host `Direction` it COMPOSES the instance `Direction.Reflect(Direction normal)` and the static `Direction.Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted)` at that world boundary (the 5-arg Snell — `etaIncident`/`etaTransmitted` are the two media IORs, not an `(eta, cosI, n)` shorthand) — Snell and the mirror are NEVER re-minted as a parallel kernel; the lobe dispatch threads `(wo, wi)`/`(owner, wo, uc, u0, u1)` through the state-passing `Switch` overload with `static` arms so the per-sample integrator loop allocates no closure; `DielectricPdf` keys its reflect/transmit split on the half-vector cosine `wo.Dot(h)` exactly as `DielectricSample` does, never the geometric `wo.CosTheta`; `DielectricSample`'s transmit arm carries the refraction Jacobian `η²·|wi·h|/(wo·h + η·wi·h)²` mirroring `DielectricPdf` (a reflect-form `1/(4|wo·h|)` on the transmit sample de-syncs the sample-local pdf from the balance-heuristic average) and draws its reflect/transmit lottery from the DEDICATED `uc` choice variable, never the consumed `u0` that already fixed the sampled half-vector (reusing it correlates the lottery with the VNDF radial coordinate and biases the estimator), so the balance-heuristic pdf stays unbiased and the white-furnace harness closes for rough glass on both sides of the interface.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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
    public RgbSpectrum Map<TState>(TState state, Func<TState, double, double> f) => Create(f(state, R), f(state, G), f(state, B));
    public RgbSpectrum Zip(RgbSpectrum o, Func<double, double, double> f) => Create(f(R, o.R), f(G, o.G), f(B, o.B));
    public RgbSpectrum Zip<TState>(RgbSpectrum o, TState state, Func<TState, double, double, double> f) =>
        Create(f(state, R, o.R), f(state, G, o.G), f(state, B, o.B));
    public RgbSpectrum Scale(double s) => Create(R * s, G * s, B * s);
    public RgbSpectrum Mul(RgbSpectrum o) => Create(R * o.R, G * o.G, B * o.B);
    public RgbSpectrum Add(RgbSpectrum o) => Create(R + o.R, G + o.G, B + o.B);
    public RgbSpectrum Lerp(RgbSpectrum o, double t) => Create(R + (o.R - R) * t, G + (o.G - G) * t, B + (o.B - B) * t);

    internal static readonly Chromaticities Ap1 = RgbProfile.Acescg.Geometry switch {
        var g => new Chromaticities(
            (float)g.Red.X, (float)g.Red.Y, (float)g.Green.X, (float)g.Green.Y,
            (float)g.Blue.X, (float)g.Blue.Y, (float)g.White.X, (float)g.White.Y),
    };

    internal static readonly (double R, double G, double B) LuminanceWeights =
        ImageProcessing.GetLuminanceWeights(Ap1) switch { var w => ((double)w.X, (double)w.Y, (double)w.Z) };

    public double Luminance => (LuminanceWeights.R * R) + (LuminanceWeights.G * G) + (LuminanceWeights.B * B);
}

[ComplexValueObject]
public readonly partial struct ComplexIor {
    public RgbSpectrum Eta { get; }
    public RgbSpectrum K { get; }

    public RgbSpectrum Fresnel(double cosI) =>
        Eta.Zip(K, cosI, static (c, e, k) => Microfacet<double>.FresnelConductor(c, e, k));
    public RgbSpectrum FresnelNormal => Fresnel(1.0);
}

public readonly record struct LobeSample(LocalVector<double> Direction, RgbSpectrum Value, double Pdf, bool Transmission)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Positive(value: Pdf));

    public static Fin<LobeSample> Of(LocalVector<double> direction, RgbSpectrum value, double pdf, bool transmission, string reason) =>
        new LobeSample(direction, value, pdf, transmission) switch {
            var drawn when drawn.IsValid => Fin.Succ(drawn),
            _ => Fin.Fail<LobeSample>(new MaterialFault.Graph(reason)),
        };
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record BsdfLobe {
    private BsdfLobe() { }

    public sealed record Diffuse(RgbSpectrum Albedo, double Roughness) : BsdfLobe;
    public sealed record Conductor(ComplexIor Ior, double AlphaX, double AlphaY, double Rotation) : BsdfLobe;
    public sealed record Dielectric(double Ior, double AlphaX, double AlphaY, double Rotation, RgbSpectrum SpecularTint, RgbSpectrum Transmittance) : BsdfLobe;
    public sealed record Sheen(RgbSpectrum Tint, double Roughness) : BsdfLobe;
    public sealed record Clearcoat(double Weight, double AlphaX, double AlphaY, double Rotation, RgbSpectrum Tint) : BsdfLobe {
        public override RgbSpectrum Transmitted => RgbSpectrum.White.Lerp(Tint, Math.Clamp(Weight, 0.0, 1.0));
    }
    public sealed record Subsurface(RgbSpectrum Albedo, double MeanFreePath) : BsdfLobe;
    public sealed record ThinFilm(double Thickness, double FilmIor, double AlphaX, double AlphaY, double Rotation, ComplexIor BaseIor) : BsdfLobe;

    public virtual RgbSpectrum Transmitted => RgbSpectrum.White;

    public RgbSpectrum Evaluate(LocalVector<double> wo, LocalVector<double> wi) => Switch(
        state: (Wo: wo, Wi: wi),
        diffuse:    static (s, d) => EvalDiffuse(d, s.Wo, s.Wi),
        conductor:  static (s, c) => EvalConductor(c, s.Wo, s.Wi),
        dielectric: static (s, g) => s.Wo.SameHemisphere(s.Wi) ? EvalDielectricReflect(g, s.Wo, s.Wi) : EvalDielectricTransmit(g, s.Wo, s.Wi),
        sheen:      static (s, h) => EvalSheen(h, s.Wo, s.Wi),
        clearcoat:  static (s, c) => EvalClearcoat(c, s.Wo, s.Wi),
        subsurface: static (s, b) => EvalSubsurface(b, s.Wo, s.Wi),
        thinFilm:   static (s, f) => EvalThinFilm(f, s.Wo, s.Wi));

    public double Pdf(LocalVector<double> wo, LocalVector<double> wi) => Switch(
        state: (Wo: wo, Wi: wi),
        diffuse:    static (s, _) => DiffusePdf(s.Wo, s.Wi),
        conductor:  static (s, c) => ReflectPdf(s.Wo, s.Wi, c.Rotation, c.AlphaX, c.AlphaY),
        dielectric: static (s, g) => DielectricPdf(g, s.Wo, s.Wi),
        sheen:      static (s, _) => DiffusePdf(s.Wo, s.Wi),
        clearcoat:  static (s, c) => ReflectPdf(s.Wo, s.Wi, c.Rotation, c.AlphaX, c.AlphaY),
        subsurface: static (s, _) => DiffusePdf(s.Wo, s.Wi),
        thinFilm:   static (s, f) => ReflectPdf(s.Wo, s.Wi, f.Rotation, f.AlphaX, f.AlphaY));

    public Fin<LobeSample> Sample(LocalVector<double> wo, double uc, double u0, double u1) => Switch(
        state: (Owner: this, Wo: wo, Uc: uc, U0: u0, U1: u1),
        diffuse:    static (s, _) => CosineSample(s.Wo, s.U0, s.U1, s.Owner, s.Key),
        conductor:  static (s, c) => ReflectSample(s.Wo, c.Rotation, c.AlphaX, c.AlphaY, s.U0, s.U1, s.Owner, s.Key),
        dielectric: static (s, g) => DielectricSample(g, s.Wo, s.Uc, s.U0, s.U1, s.Key),
        sheen:      static (s, _) => CosineSample(s.Wo, s.U0, s.U1, s.Owner, s.Key),
        clearcoat:  static (s, c) => ReflectSample(s.Wo, c.Rotation, c.AlphaX, c.AlphaY, s.U0, s.U1, s.Owner, s.Key),
        subsurface: static (s, _) => CosineSample(s.Wo, s.U0, s.U1, s.Owner, s.Key),
        thinFilm:   static (s, f) => ReflectSample(s.Wo, f.Rotation, f.AlphaX, f.AlphaY, s.U0, s.U1, s.Owner, s.Key));

    private static double DiffusePdf(LocalVector<double> wo, LocalVector<double> wi) =>
        wo.SameHemisphere(wi) ? Math.Abs(wi.CosTheta) / Math.PI : 0.0;

    // --- [DIFFUSE]
    private static RgbSpectrum EvalDiffuse(Diffuse d, LocalVector<double> wo, LocalVector<double> wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        double s2 = d.Roughness * d.Roughness;
        double a = 1.0 - (0.5 * s2 / (s2 + 0.33)), b = 0.45 * s2 / (s2 + 0.09);
        double sinO = wo.SinTheta, sinI = wi.SinTheta;
        double maxCos = sinO > 1e-4 && sinI > 1e-4 ? Math.Max(0.0, (wi.CosPhi * wo.CosPhi) + (wi.SinPhi * wo.SinPhi)) : 0.0;
        double aco = Math.Max(RhinoMath.ZeroTolerance, Math.Abs(wo.CosTheta)), aci = Math.Max(RhinoMath.ZeroTolerance, Math.Abs(wi.CosTheta));
        (double sinAlpha, double tanBeta) = aci > aco ? (sinO, sinI / aci) : (sinI, sinO / aco);
        return d.Albedo.Scale((a + (b * maxCos * sinAlpha * tanBeta)) / Math.PI);
    }

    private static RgbSpectrum EvalSubsurface(Subsurface b, LocalVector<double> wo, LocalVector<double> wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        double fL = SchlickWeight(Math.Abs(wi.CosTheta)), fV = SchlickWeight(Math.Abs(wo.CosTheta));
        double diffuse = (1.0 - 0.5 * fL) * (1.0 - 0.5 * fV) / Math.PI;
        return b.Albedo.Scale(diffuse);
    }
    private static double SchlickWeight(double cosTheta) { double m = Math.Clamp(1.0 - cosTheta, 0.0, 1.0); double m2 = m * m; return m2 * m2 * m; }

    // --- [MICROFACET_REFLECT]
    private static RgbSpectrum MicrofacetReflect<TState>(
        TState state, LocalVector<double> outgoing, LocalVector<double> incident, double rotation, double alphaX, double alphaY,
        Func<TState, double, RgbSpectrum> fresnel, Func<TState, double, LocalVector<double>, LocalVector<double>, RgbSpectrum> compensation) {
        (LocalVector<double> wo, LocalVector<double> wi) = (outgoing.RotateZ(-rotation), incident.RotateZ(-rotation));
        if (!wo.SameHemisphere(wi)
            || Math.Abs(wo.CosTheta) <= RhinoMath.ZeroTolerance || Math.Abs(wi.CosTheta) <= RhinoMath.ZeroTolerance) { return RgbSpectrum.Black; }
        LocalVector<double> h = wo.Add(wi).Normalize();
        h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;
        double single = Microfacet<double>.Ndf(h, alphaX, alphaY) * Microfacet<double>.MaskingShadowing(wo, wi, alphaX, alphaY)
            / (4.0 * Math.Abs(wo.CosTheta) * Math.Abs(wi.CosTheta));
        return fresnel(state, wo.Dot(h)).Scale(single).Add(compensation(state, Math.Sqrt(alphaX * alphaY), wo, wi));
    }

    // --- [CONDUCTOR]
    private static RgbSpectrum EvalConductor(Conductor c, LocalVector<double> wo, LocalVector<double> wi) =>
        MicrofacetReflect(c, wo, wi, c.Rotation, c.AlphaX, c.AlphaY,
            fresnel: static (own, cosH) => own.Ior.Fresnel(cosH),
            compensation: static (own, alpha, o, i) => MultiScatter.KullaConty(alpha, own.Ior.FresnelNormal, o, i));

    // --- [DIELECTRIC]
    private static RgbSpectrum EvalDielectricReflect(Dielectric g, LocalVector<double> wo, LocalVector<double> wi) =>
        MicrofacetReflect(g, wo, wi, g.Rotation, g.AlphaX, g.AlphaY,
            fresnel: static (own, cosH) => own.SpecularTint.Scale(Microfacet<double>.FresnelDielectric(cosH, own.Ior)),
            compensation: static (own, alpha, o, i) => MultiScatter.KullaConty(alpha, own.SpecularTint.Scale(DielectricF0(own.Ior)), o, i));
    private static double DielectricF0(double ior) => (ior - 1.0) / (ior + 1.0) switch { var r => r * r };
    private static RgbSpectrum EvalDielectricTransmit(Dielectric g, LocalVector<double> outgoing, LocalVector<double> incident) {
        (LocalVector<double> wo, LocalVector<double> wi) = (outgoing.RotateZ(-g.Rotation), incident.RotateZ(-g.Rotation));
        double eta = wo.CosTheta > 0.0 ? g.Ior : 1.0 / g.Ior;
        LocalVector<double> h = wo.Add(wi.Scale(eta)).Normalize();
        h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;
        double d = Microfacet<double>.Ndf(h, g.AlphaX, g.AlphaY);
        double mask = Microfacet<double>.MaskingShadowing(wo, wi, g.AlphaX, g.AlphaY);
        double f = Microfacet<double>.FresnelDielectric(wo.Dot(h), g.Ior);
        double sqrtDenom = wo.Dot(h) + (eta * wi.Dot(h));
        double geometry = Math.Max(RhinoMath.ZeroTolerance, Math.Abs(wo.CosTheta * wi.CosTheta));
        double factor = Math.Abs(wo.Dot(h) * wi.Dot(h)) / geometry
            * (eta * eta) / Math.Max(RhinoMath.ZeroTolerance, sqrtDenom * sqrtDenom);
        return g.Transmittance.Scale(d * mask * (1.0 - f) * factor);
    }

    // --- [SHEEN]
    private static RgbSpectrum EvalSheen(Sheen s, LocalVector<double> wo, LocalVector<double> wi) {
        if (!wo.SameHemisphere(wi)) { return RgbSpectrum.Black; }
        LocalVector<double> h = wo.Add(wi).Normalize();
        double inv = 1.0 / Math.Max(1e-3, s.Roughness);
        double dSheen = (2.0 + inv) * Math.Pow(h.Sin2Theta, inv * 0.5) / (2.0 * Math.PI);
        double aco = Math.Abs(wo.CosTheta), aci = Math.Abs(wi.CosTheta);
        double g = 1.0 / Math.Max(RhinoMath.ZeroTolerance, 4.0 * (aco + aci - (aco * aci)));
        return s.Tint.Scale(dSheen * g);
    }

    // --- [CLEARCOAT]
    private static RgbSpectrum EvalClearcoat(Clearcoat c, LocalVector<double> wo, LocalVector<double> wi) =>
        MicrofacetReflect(c, wo, wi, c.Rotation, c.AlphaX, c.AlphaY,
            fresnel: static (own, cosH) => RgbSpectrum.White.Scale(own.Weight * Microfacet<double>.FresnelSchlick(Math.Abs(cosH), 0.04)),
            compensation: static (_, _, _, _) => RgbSpectrum.Black);

    // --- [THIN_FILM]
    private static RgbSpectrum EvalThinFilm(ThinFilm f, LocalVector<double> wo, LocalVector<double> wi) =>
        MicrofacetReflect(f, wo, wi, f.Rotation, f.AlphaX, f.AlphaY,
            fresnel: static (own, cosH) => own.BaseIor.Fresnel(cosH).Mul(FilmInterference(own, cosH)),
            compensation: static (_, _, _, _) => RgbSpectrum.Black);
    private static RgbSpectrum FilmInterference(ThinFilm f, double cosI) {
        double sinT2 = (1.0 - (cosI * cosI)) / (f.FilmIor * f.FilmIor);
        double opd = 2.0 * f.FilmIor * f.Thickness * Math.Sqrt(Math.Max(0.0, 1.0 - sinT2));
        return RgbSpectrum.Create(Interference(opd, SpectralBand.Red.CenterNm), Interference(opd, SpectralBand.Green.CenterNm), Interference(opd, SpectralBand.Blue.CenterNm));
    }
    private static double Interference(double opd, double wavelengthNm) => 0.5 * (1.0 + Math.Cos(2.0 * Math.PI * opd / wavelengthNm));

    // --- [SAMPLING]
    private static Fin<LobeSample> CosineSample(LocalVector<double> wo, double u0, double u1, BsdfLobe owner) {
        double r = Math.Sqrt(u0), phi = 2.0 * Math.PI * u1;
        double z = Math.Sqrt(Math.Max(0.0, 1.0 - u0));
        LocalVector<double> wi = new(r * Math.Cos(phi), r * Math.Sin(phi), wo.CosTheta < 0.0 ? -z : z);
        return LobeSample.Of(wi, owner.Evaluate(wo, wi), Math.Abs(wi.CosTheta) / Math.PI, transmission: false, "<zero-pdf-cosine-sample>");
    }
    private static Fin<LobeSample> ReflectSample(
        LocalVector<double> wo, double rotation, double ax, double ay, double u0, double u1, BsdfLobe owner) {
        LocalVector<double> local = wo.RotateZ(-rotation);
        LocalVector<double> h = Microfacet<double>.SampleVisibleNormal(local.CosTheta < 0.0 ? local.Scale(-1.0) : local, ax, ay, u0, u1);
        LocalVector<double> reflected = h.Scale(2.0 * local.Dot(h)).Add(local.Scale(-1.0));
        if (!local.SameHemisphere(reflected)) { return Fin.Fail<LobeSample>(new MaterialFault.Graph("<vndf-below-horizon>")); }
        LocalVector<double> wi = reflected.RotateZ(rotation);
        return LobeSample.Of(wi, owner.Evaluate(wo, wi), Microfacet<double>.ReflectPdf(local, h, ax, ay),
            transmission: false, "<zero-pdf-vndf-sample>");
    }
    private static double ReflectPdf(LocalVector<double> outgoing, LocalVector<double> incident, double rotation, double ax, double ay) {
        (LocalVector<double> wo, LocalVector<double> wi) = (outgoing.RotateZ(-rotation), incident.RotateZ(-rotation));
        if (!wo.SameHemisphere(wi)) { return 0.0; }
        LocalVector<double> h = wo.Add(wi).Normalize();
        h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;
        return Microfacet<double>.ReflectPdf(wo, h, ax, ay);
    }
    private static Fin<LobeSample> DielectricSample(Dielectric g, LocalVector<double> outgoing, double uc, double u0, double u1) {
        LocalVector<double> wo = outgoing.RotateZ(-g.Rotation);
        LocalVector<double> h = Microfacet<double>.SampleVisibleNormal(wo.CosTheta < 0.0 ? wo.Scale(-1.0) : wo, g.AlphaX, g.AlphaY, u0, u1);
        double f = Microfacet<double>.FresnelDielectric(wo.Dot(h), g.Ior);
        if (uc < f) {
            LocalVector<double> reflected = h.Scale(2.0 * wo.Dot(h)).Add(wo.Scale(-1.0));
            LocalVector<double> wiReflect = reflected.RotateZ(g.Rotation);
            return wo.SameHemisphere(reflected)
                ? LobeSample.Of(wiReflect, EvalDielectricReflect(g, outgoing, wiReflect),
                      f * Microfacet<double>.ReflectPdf(wo, h, g.AlphaX, g.AlphaY), transmission: false, "<dielectric-reflect-degenerate>")
                : Fin.Fail<LobeSample>(new MaterialFault.Graph("<dielectric-reflect-degenerate>"));
        }
        double eta = wo.CosTheta > 0.0 ? 1.0 / g.Ior : g.Ior;
        LocalVector<double> n = wo.Dot(h) < 0.0 ? h.Scale(-1.0) : h;
        double cosI = Math.Clamp(wo.Dot(n), -1.0, 1.0);
        double k = 1.0 - (eta * eta * (1.0 - (cosI * cosI)));
        if (k < 0.0) { return Fin.Fail<LobeSample>(new MaterialFault.Graph("<dielectric-refract-tir>")); }
        LocalVector<double> refracted = wo.Scale(-eta).Add(n.Scale((eta * cosI) - Math.Sqrt(k)));
        double etaH = 1.0 / eta;
        double sqrtDenom = wo.Dot(h) + (etaH * refracted.Dot(h));
        double pdf = (1.0 - f) * Microfacet<double>.VisibleNormalPdf(wo, h, g.AlphaX, g.AlphaY)
            * Math.Abs(etaH * etaH * refracted.Dot(h)) / Math.Max(RhinoMath.ZeroTolerance, sqrtDenom * sqrtDenom);
        LocalVector<double> wi = refracted.RotateZ(g.Rotation);
        return LobeSample.Of(wi, EvalDielectricTransmit(g, outgoing, wi), pdf, transmission: true, "<dielectric-refract-degenerate>");
    }
    private static double DielectricPdf(Dielectric g, LocalVector<double> outgoing, LocalVector<double> incident) {
        (LocalVector<double> wo, LocalVector<double> wi) = (outgoing.RotateZ(-g.Rotation), incident.RotateZ(-g.Rotation));
        if (wo.SameHemisphere(wi)) {
            LocalVector<double> h = wo.Add(wi).Normalize();
            h = h.CosTheta < 0.0 ? h.Scale(-1.0) : h;
            return Microfacet<double>.FresnelDielectric(wo.Dot(h), g.Ior) * Microfacet<double>.ReflectPdf(wo, h, g.AlphaX, g.AlphaY);
        }
        double eta = wo.CosTheta > 0.0 ? g.Ior : 1.0 / g.Ior;
        LocalVector<double> ht = wo.Add(wi.Scale(eta)).Normalize();
        ht = ht.CosTheta < 0.0 ? ht.Scale(-1.0) : ht;
        double ft = Microfacet<double>.FresnelDielectric(wo.Dot(ht), g.Ior);
        double sqrtDenom = wo.Dot(ht) + (eta * wi.Dot(ht));
        return (1.0 - ft) * Microfacet<double>.VisibleNormalPdf(wo, ht, g.AlphaX, g.AlphaY)
            * Math.Abs(eta * eta * wi.Dot(ht)) / Math.Max(RhinoMath.ZeroTolerance, sqrtDenom * sqrtDenom);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MultiScatter {
    public static double DirectionalAlbedo(double alpha, double mu) =>
        0.04 + (alpha * (0.31 - (0.28 * alpha))) switch {
            var bias => Math.Clamp(((1.0 - bias) * (1.0 - Math.Pow(1.0 - mu, 5.0 * (1.0 - alpha)))) + bias, 0.0, 1.0),
        };

    internal const int AlbedoNodes = 16;
    internal const int AlbedoGrid = 65;

    private static readonly double[] AlbedoCurve =
        [.. Enumerable.Range(0, AlbedoGrid).Select(static node => node / (AlbedoGrid - 1.0)).Select(static alpha =>
            Math.Clamp(2.0 * Integrate.GaussLegendre(mu => DirectionalAlbedo(alpha, mu) * mu, 0.0, 1.0, AlbedoNodes), 0.0, 1.0))];

    internal static double HemisphericalAlbedo(double alpha) =>
        Math.Clamp(alpha, 0.0, 1.0) * (AlbedoGrid - 1) switch {
            var scaled => Math.Min((int)scaled, AlbedoGrid - 2) switch {
                var node => AlbedoCurve[node] + ((AlbedoCurve[node + 1] - AlbedoCurve[node]) * (scaled - node)),
            },
        };

    public static RgbSpectrum KullaConty(double alpha, RgbSpectrum fresnelAvg, LocalVector<double> wo, LocalVector<double> wi) {
        double eo = DirectionalAlbedo(alpha, Math.Abs(wo.CosTheta));
        double ei = DirectionalAlbedo(alpha, Math.Abs(wi.CosTheta));
        double eavg = HemisphericalAlbedo(alpha);
        double fms = (1.0 - eo) * (1.0 - ei) / (Math.PI * Math.Max(1e-4, 1.0 - eavg));
        return fresnelAvg.Map((Fms: fms, Eavg: eavg),
            static (s, f) => s.Fms * f * s.Eavg / Math.Max(1e-4, 1.0 - (f * (1.0 - s.Eavg))));
    }

    // --- [POSITION_FREE_WALK]
    public static Fin<LobeSample> PositionFree(
        double alphaX, double alphaY, ComplexIor ior, LocalVector<double> wo, UnitInterval survival, int seed, int tap) {
        double live = Math.Clamp(survival.Value, 1e-3, 1.0);
        (RgbSpectrum weight, double density, LocalVector<double> w) = (RgbSpectrum.White, 1.0, wo);
        for (int depth = 0; ; depth++) {
            LocalVector<double> upper = w.CosTheta < 0.0 ? w.Scale(-1.0) : w;
            LocalVector<double> h = Microfacet<double>.SampleVisibleNormal(upper, alphaX, alphaY, Draw(seed, tap, depth, axis: 0), Draw(seed, tap, depth, axis: 1));
            LocalVector<double> next = h.Scale(2.0 * upper.Dot(h)).Add(upper.Scale(-1.0)).Normalize();
            weight = weight.Mul(ior.Fresnel(Math.Abs(upper.Dot(h))));
            density *= Microfacet<double>.ReflectPdf(upper, h, alphaX, alphaY);
            if (next.CosTheta > 0.0) { return LobeSample.Of(next, weight, density, transmission: false, "<multi-scatter-walk-degenerate>"); }
            if (Draw(seed, tap, depth, axis: 2) >= live) { return Fin.Fail<LobeSample>(new MaterialFault.Graph("<multi-scatter-walk-absorbed>")); }
            (w, weight, density) = (next, weight.Scale(1.0 / live), density * live);
        }
    }

    private static double Draw(int seed, int tap, int depth, int axis) => Deterministic.Unit(lanes: [tap, depth, axis], seed: seed);

    public static double SeparableProfile(double meanFreePath, double r) {
        double d = Math.Max(1e-4, meanFreePath), rr = Math.Max(1e-4, r);
        return (Math.Exp(-rr / d) + Math.Exp(-rr / (3.0 * d))) / (8.0 * Math.PI * d * rr);
    }
}
```

## [05]-[LAYERED_COMPOSITION]

- Owner: `LayeredBsdf` — the weighted-lobe fold; `LobeWeight` the per-lobe weight row.
- Entry: `public RgbSpectrum Evaluate(ShadingFrame frame, Direction wo, Direction wi)` · `public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1)` · `public double Pdf(ShadingFrame frame, Direction wo, Direction wi)` — the renderer's sole shading entry; the integrator transforms to local once, folds the weighted lobes, and transforms back; `Sample` carries its `MaterialFault` channel and `Of` admits the weighted lobe list under the same key.
- Packages: Rasm (project — `Rasm.Numerics` `Direction`/`UnitInterval`), LanguageExt.Core, BCL inbox.
- Growth: a new MATERIAL is a new `Seq<LobeWeight>` value — a row of weights and lobe parameters a `MaterialParameters` row supplies — NEVER a new type; this is THE polymorphic-mandate boundary: `LayeredBsdf.Of` takes the weighted lobe list a library row produces, so gold is `[Conductor 1.0]`, glass is `[Dielectric 1.0]`, plastic is `[Diffuse 0.9, Dielectric-coat 0.1]`, car paint is `[Conductor-flake 0.7, Clearcoat 0.3]`, skin is `[Subsurface 0.8, Dielectric 0.2]`, velvet is `[Diffuse 0.6, Sheen 0.4]`, wax is `[Subsurface 0.5, Diffuse 0.5]` — all the SAME `LayeredBsdf`, differing only by row data. The `[9]-[OPENPBR_SLAB]` `SlabStack` is the realized formal layering construction (fuzz over coat over base, albedo-scaling operators) whose `ToLayered` energy-preserving collapse lowers to this weighted-lobe fold the integrator shades; the slab algebra builds the stack a row drives through, this fold the one BSDF the renderer reads.
- Boundary: `Evaluate` is the weighted SUM of each lobe's own value under its `Throughput` (each lobe is linear, so the layered BSDF is the convex combination of lobe values by weight, the spectral absorption of the layers above riding the per-row column rather than the scalar weight the normalization owns); the fold carries REFLECTANCE alone — the `surface#OPENPBR_SLAB` collapse returns its accumulated emission BESIDE the `LayeredBsdf` because self-emission is energy the surface ADDS rather than energy it redistributes, so an emission lobe inside this normalized convex sum would be divided by a pdf and multiplied by a cosine that describe scattering, and the integrator adds the emission term exactly ONCE per shading point outside the BSDF estimator; `Sample` is the one-sample MIS — pick a lobe proportionally to weight via `uLobe`, hand it the rescaled remainder `(uLobe − cdfBefore)/weight` as its `uc` choice variable (stratification survives both choices), sample it, then the returned pdf is the WEIGHT-AVERAGED pdf across ALL lobes (the balance heuristic) so the estimator is unbiased and low-variance, and the value re-evaluates the FULL layered BSDF against the sampled direction; `Pdf` mirrors that weighted average exactly; weights are `UnitInterval` and sum-normalized at `Of` (a row whose weights miss [0,1] or sum to zero fails `MaterialFault.Parameter` `<lobe-weights-degenerate>`); the `Of` normalization is the page's one admitted total-construction site — every input `Weight` is already a `UnitInterval` and `total` is their sum, so `weight/total` is in `[0,1]` by construction and the `UnitInterval.Create` throw is statically unreachable, named here as the `[EXPRESSION_SPINE]` exemption rather than exception-style control flow in a fallible path; the boundary projects the final shade to in-gamut at the renderer edge, never inside the fold — a non-finite throughput fails `MaterialFault.Gamut` `<non-finite-shade>` through `Option`/`Fin`, never propagating NaN; this fold is the ONLY place lobe weights live, so the masonry-assignment consumer and every `MaterialLibrary` row drive appearance purely by producing a `Seq<LobeWeight>`.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LobeWeight(BsdfLobe Lobe, UnitInterval Weight, RgbSpectrum Throughput);

public sealed record LayeredBsdf {
    private LayeredBsdf(Seq<LobeWeight> lobes) => Lobes = lobes;
    public Seq<LobeWeight> Lobes { get; }

    public static Fin<LayeredBsdf> Of(Seq<LobeWeight> lobes) {
        double total = lobes.Sum(static l => l.Weight.Value);
        return lobes.IsEmpty || total <= RhinoMath.ZeroTolerance
            ? Fin.Fail<LayeredBsdf>(new MaterialFault.Parameter("<lobe-weights-degenerate>"))
            : Fin.Succ(new LayeredBsdf(lobes.Map(l => l with { Weight = UnitInterval.Create(l.Weight.Value / total) })));
    }

    public RgbSpectrum Evaluate(ShadingFrame frame, Direction wo, Direction wi) {
        LocalVector<double> lo = frame.ToLocal(wo), li = frame.ToLocal(wi);
        return Lobes.Fold((Acc: RgbSpectrum.Black, Lo: lo, Li: li),
            static (s, lw) => (s.Acc.Add(lw.Lobe.Evaluate(s.Lo, s.Li).Mul(lw.Throughput).Scale(lw.Weight.Value)), s.Lo, s.Li)).Acc;
    }

    public double Pdf(ShadingFrame frame, Direction wo, Direction wi) {
        LocalVector<double> lo = frame.ToLocal(wo), li = frame.ToLocal(wi);
        return Lobes.Fold((Acc: 0.0, Lo: lo, Li: li),
            static (s, lw) => (s.Acc + lw.Weight.Value * lw.Lobe.Pdf(s.Lo, s.Li), s.Lo, s.Li)).Acc;
    }

    public Fin<LobeSample> Sample(ShadingFrame frame, Direction wo, double uLobe, double u0, double u1) {
        LocalVector<double> lo = frame.ToLocal(wo);
        (double U, double Cumulative, double Before, LobeWeight Pick) pick = Lobes.Fold(
            (U: uLobe, Cumulative: 0.0, Before: 0.0, Pick: Lobes[^1]),
            static (s, lw) => s.Cumulative > s.U || s.Cumulative + lw.Weight.Value <= s.U
                ? (s.U, s.Cumulative + (s.Cumulative > s.U ? 0.0 : lw.Weight.Value), s.Before, s.Pick)
                : (s.U, s.Cumulative + lw.Weight.Value, s.Cumulative, lw));
        double uc = Math.Clamp((uLobe - pick.Before) / Math.Max(RhinoMath.ZeroTolerance, pick.Pick.Weight.Value), 0.0, 1.0);
        return pick.Pick.Lobe.Sample(lo, uc, u0, u1, key).Bind(sample => {
            double mixedPdf = Lobes.Fold((Acc: 0.0, Lo: lo, Wi: sample.Direction),
                static (s, lw) => (s.Acc + lw.Weight.Value * lw.Lobe.Pdf(s.Lo, s.Wi), s.Lo, s.Wi)).Acc;
            RgbSpectrum mixedValue = Lobes.Fold((Acc: RgbSpectrum.Black, Lo: lo, Wi: sample.Direction),
                static (s, lw) => (s.Acc.Add(lw.Lobe.Evaluate(s.Lo, s.Wi).Mul(lw.Throughput).Scale(lw.Weight.Value)), s.Lo, s.Wi)).Acc;
            return mixedPdf > 0.0
                ? Fin.Succ(sample with { Pdf = mixedPdf, Value = mixedValue })
                : Fin.Fail<LobeSample>(new MaterialFault.Graph(key, "<degenerate-mixed-pdf>"));
        });
    }
}
```

## [06]-[KERNEL_BOUNDARIES]

The lowering/grounding half — `SpectralUpsample` (RGB→SPD + measured-illuminant reduction), `ToneMap` (ACES RRT/ODT + scene-referred operators), `ConductorMetal` (the measured complex-IOR rows), and `SlabStack` (the OpenPBR Surface 1.1 stack-of-slabs) — is the `surface#SPECTRAL_UPSAMPLE`/`#TONE_MAP`/`#CONDUCTOR_IOR`/`#OPENPBR_SLAB` page, split out under the per-page depth budget so the kernel page owns frame-local shading and the surface page owns the OpenPBR construction. The two pages share the `[01]-[SHADING_FRAME]` `MaterialFault` band (`FaultBand.Appearance`) and the `SpectralBand` band-centre vocabulary declared once here and composed by the surface page; the `[04]-[LOBE_FAMILY]` `RgbSpectrum`/`ComplexIor` validated carriers and `BsdfLobe` closed set and the `[05]-[LAYERED_COMPOSITION]` `LayeredBsdf.Of` fold are read by `surface#OPENPBR_SLAB` `SlabStack.ToLayered`, the `[04]-[LOBE_FAMILY]` `ComplexIor.Fresnel` per-band read by `surface#CONDUCTOR_IOR`, and the `[04]-[LOBE_FAMILY]` `MultiScatter.DirectionalAlbedo` by the `SlabStack` albedo-scaling. A `MaterialParameters` row lowers through `surface#OPENPBR_SLAB` `SlabStack.Lower` to the formal stack and `ToLayered` collapses it to the PAIR the integrator reads — the one `LayeredBsdf` weighted fold this page shades and the accumulated `RgbSpectrum` emission the integrator adds as radiance outside that fold, the emission already attenuated by the throughput of every slab covering it — so the slab algebra is the construction, the lobe math single-sourced on this page, and the two terminal quantities travel together rather than one of them dying at the collapse.

## [07]-[BSDF_ORACLE]

- Owner: `BsdfProbe` `[SmartEnum<string>]` the closed energy-proof roster carrying its measurement fold and its fixture mint as delegate columns; `BsdfOracle` the per-(probe, roughness) fixture row; `BsdfOracle.All` the derived table.
- Entry: `public static Fin<Unit> Prove(BsdfOracle fixture)` runs the row's own `Measure` fold and gates the deviation against `Tolerance + Residue`; `All` DERIVES from `BsdfProbe.Items` crossed with the roughness sweep, so a sixth roughness or a fourth probe moves the whole table and no hand-maintained second roster exists to drift.
- Packages: Rasm (project — `Deterministic.Hammersley`/`RadicalInverse`, the kernel's equidistributed draw family), MathNet.Numerics (composed transitively through `MultiScatter.HemisphericalAlbedo`, whose value every furnace row pins to), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new energy law is one `BsdfProbe` row carrying its fold and its fixture mint; a new roughness is one `Sweep` element; zero new surface.
- Law: every expectation is EXACTLY COMPUTABLE from the kernel's own definition INCLUDING its own quadrature. The furnace expectation is `MultiScatter.HemisphericalAlbedo`'s Gauss-Legendre value at `MultiScatter.AlbedoNodes` and the row's alpha, RE-PINNED off the midpoint sum that preceded it, never the analytic 1 — an analytic limit asserted against a discrete estimator fails a CORRECT kernel and passes only after someone loosens the tolerance — and `Residue` records what the tap count leaves so the pair is read together; reciprocity and NDF normalization carry exact expectations because neither is quadrature-bounded at the assertion, the first an equality between two evaluations and the second the closed-form unit integral. A transcribed decimal nobody can re-derive is the deleted form.
- Boundary: the furnace row is the numeric proof that `MultiScatter.KullaConty` recovers exactly the energy the single-scatter Smith model loses at high roughness — the analytic `DirectionalAlbedo` Karis fit supplies `E(μ)` and the `HemisphericalAlbedo` quadrature closes `Eavg = 2∫E(μ)μ dμ`, so this is a probe over a closed-form result and never an open gate; the lossless conductor (`η→0`, `k→∞`, so `FresnelConductor ≡ 1` at every incidence) is the ONE lobe whose furnace expectation is a full unit, because a tinted metal folds its own absorption into the residual and proves nothing about the compensation term. Draws are `Deterministic.Hammersley` over `RadicalInverse` — the kernel's equidistributed family, since a splitmix stream clusters visibly at this tap budget and the clustering would read as an energy defect — and a refused sample contributes zero, the physically correct below-horizon draw. Oracle divergence fails `MaterialFault.Parameter` naming the fixture and the deviation, and it is a HARD failure: a lobe disagreeing with its own closed-form answer is a broken lobe. The `Raster/gpu#ORACLE_VECTOR` table is the GPU peer of this shape over WGSL kernels, and the two never share a row — one proves a device, this one proves a lobe.
- Exemption: the three `Measure` folds are fixed-trip accumulations over the closed-form kernels, and `[04]-[LOBE_FAMILY]` `MultiScatter.PositionFree` is the page's one UNBOUNDED walk kernel — roulette and escape are its only exits, so it carries no trip count to make expression-shaped. `MultiScatter.HemisphericalAlbedo` names no carve: its table folds through LINQ at type init and its read is a switch expression.

```csharp

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BsdfOracle(string Name, BsdfProbe Probe, BsdfLobe Lobe, int Taps, double Expected, double Residue, double Tolerance);

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BsdfProbe {
    public static readonly BsdfProbe Furnace     = new("furnace",     Energy.Furnace,     Energy.FurnaceRow);
    public static readonly BsdfProbe Reciprocity = new("reciprocity", Energy.Reciprocity, Energy.ReciprocityRow);
    public static readonly BsdfProbe NdfNormal   = new("ndf-normal",  Energy.NdfNormal,   Energy.NdfNormalRow);

    [UseDelegateFromConstructor] public partial double Measure(BsdfLobe lobe, int taps);
    [UseDelegateFromConstructor] public partial BsdfOracle Fixture(double roughness);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Energy {
    internal const int Taps = 4096;
    internal static readonly Seq<double> Sweep = Seq(0.1, 0.3, 0.5, 0.7, 0.9);
    private static readonly ComplexIor Lossless = ComplexIor.Create(RgbSpectrum.Black, RgbSpectrum.White.Scale(1e6));

    public static Seq<BsdfOracle> All =>
        toSeq(BsdfProbe.Items).Bind(probe => Sweep.Map(probe.Fixture));

    public static Fin<Unit> Prove(BsdfOracle fixture) =>
        fixture.Probe.Measure(fixture.Lobe, fixture.Taps) switch {
            var measured when double.IsFinite(measured) && Math.Abs(measured - fixture.Expected) <= fixture.Tolerance + fixture.Residue => Fin.Succ(unit),
            var measured => Fin.Fail<Unit>(new MaterialFault.Parameter($"<bsdf-oracle:{fixture.Name}:{measured:R}!={fixture.Expected:R}+-{fixture.Tolerance + fixture.Residue:R}>")),
        };

    internal static BsdfOracle FurnaceRow(double roughness) =>
        Microfacet<double>.AlphaOf(roughness) switch {
            var alpha => new BsdfOracle($"furnace-a{roughness:F1}", BsdfProbe.Furnace,
                new BsdfLobe.Conductor(Lossless, alpha, alpha, Rotation: 0.0), Taps,
                Expected: MultiScatter.HemisphericalAlbedo(alpha), Residue: 1.0 - MultiScatter.HemisphericalAlbedo(alpha), Tolerance: 4e-3),
        };
    internal static BsdfOracle ReciprocityRow(double roughness) =>
        Microfacet<double>.AlphaOf(roughness) switch {
            var alpha => new BsdfOracle($"reciprocity-a{roughness:F1}", BsdfProbe.Reciprocity,
                new BsdfLobe.Conductor(Lossless, alpha, alpha * 0.5, Rotation: 0.3), Taps, Expected: 0.0, Residue: 0.0, Tolerance: 1e-9),
        };
    internal static BsdfOracle NdfNormalRow(double roughness) =>
        Microfacet<double>.AlphaOf(roughness) switch {
            var alpha => new BsdfOracle($"ndf-normal-a{roughness:F1}", BsdfProbe.NdfNormal,
                new BsdfLobe.Conductor(Lossless, alpha, alpha * 0.5, Rotation: 0.0), Taps, Expected: 1.0, Residue: 0.0, Tolerance: 2e-3),
        };

    internal static double Furnace(BsdfLobe lobe, int taps) {
        double acc = 0.0;
        for (int i = 0; i < taps; i++) {
            (double u0, double u1) = Deterministic.Hammersley(index: i, count: taps);
            acc += lobe.Sample(LocalVector<double>.Normal, Deterministic.RadicalInverse(index: (uint)i, radix: 3), u0, u1, Probe)
                .Match(Succ: static s => s.Value.Luminance * Math.Abs(s.Direction.CosTheta) / s.Pdf, Fail: static _ => 0.0);
        }
        return acc / taps;
    }

    internal static double Reciprocity(BsdfLobe lobe, int taps) {
        double worst = 0.0;
        for (int i = 0; i < taps; i++) {
            (double u0, double u1) = Deterministic.Hammersley(index: i, count: taps);
            LocalVector<double> wo = Hemisphere(u0, u1), wi = Hemisphere(u1, Deterministic.RadicalInverse(index: (uint)i, radix: 5));
            worst = Math.Max(worst, Math.Abs(lobe.Evaluate(wo, wi).Luminance - lobe.Evaluate(wi, wo).Luminance));
        }
        return worst;
    }

    internal static double NdfNormal(BsdfLobe lobe, int taps) {
        double acc = 0.0, ax = lobe is BsdfLobe.Conductor c ? c.AlphaX : 1.0, ay = lobe is BsdfLobe.Conductor d ? d.AlphaY : 1.0;
        for (int i = 0; i < taps; i++) {
            (double u0, double u1) = Deterministic.Hammersley(index: i, count: taps);
            LocalVector<double> h = Hemisphere(u0, u1);
            acc += Microfacet<double>.Ndf(h, ax, ay) * h.CosTheta;
        }
        return acc * (2.0 * Math.PI) / taps;
    }

    private static LocalVector<double> Hemisphere(double u0, double u1) {
        double z = u0, r = Math.Sqrt(Math.Max(0.0, 1.0 - (z * z))), phi = 2.0 * Math.PI * u1;
        return new LocalVector<double>(r * Math.Cos(phi), r * Math.Sin(phi), z);
    }
}
```

## [08]-[RESEARCH]

(none)
