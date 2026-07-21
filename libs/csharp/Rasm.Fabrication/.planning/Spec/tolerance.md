# [RASM_FABRICATION_TOLERANCE]

`Tolerance` owns every production-specification value from raw quantity admission through geometric control, ISO 286 fit, general tolerance, surface texture, method-parameterized stackup, typed derivation, and parameterized wire projection. `FeatureControl`, `FitClass`, `SurfaceTexture`, and `ToleranceChain` admit once, while `Tolerance.Apply` dispatches every ingress, operation, and egress modality on payload-complete `ToleranceRequest` cases.

`Tolerance` preserves the cross-runtime wire name consumed by the artifacts plane without exporting its C# shape, keys structural refusals through `SpecOp`, carries `ContentKey` into `ToleranceUnsatisfiable`, consumes measured cutter geometry through admitted `CutterForm`, and accepts capability evidence only as input-carried achievable width. `Rasm.Solving` owns the name `FitReceipt`, so the ISO 286 pairing result is `FitLimits`.

## [01]-[INDEX]

- [01]-[VOCABULARY]: `FeatureCharacteristic`, `FeatureScope`, and symbol-bearing modifier and material rows; payload-shaped `ToleranceZone`; datum, texture, and stackup families; generated `ItGradeName`; validated fit and general-tolerance seed laws; the `Tolerance.Apply` fold over `ToleranceRequest` into `ToleranceReceipt`.

## [02]-[VOCABULARY]

- Owner: `Tolerance` is the canonical `[Union]`; each raw case enters through one generated invariant owner and leaves through `ToleranceReceipt`.
- Cases: `FeatureScope` distinguishes surface, axis, median-line, median-plane, and center-point controls before material-condition policy resolves; `ToleranceZone` carries only evidence its zone arm consumes; `SurfaceLimit` closes exact, maximum, minimum, and ranged acceptance; `SurfaceRequirement` separates amplitude, spacing, ratio, level-ratio, material-difference, and shape payloads; `Tolerance` closes geometric, fit, texture, general, and chain specifications and projects `Source` and `Qif` over all five.
- Auto: `ItGradeName` generates `IT01` through `IT18`, including normative standard-series multipliers; `FitStandard` and `GeneralStandard` admit irreducibly tabular standard data as parameterized seeds instead of named calculators; `ZoneModifier.Admits`, `FeatureCharacteristic.AdmitsScope`, and `MaterialCondition.Boundaries` carry the ISO 1101 legality and virtual-condition law as row behavior, so `FeatureControl` never re-derives it.
- Rails: generated frame validation is the single admission authority; `Tolerance.Mint` is the one bridge from a generated factory outcome onto `Fin`, preserving the generator's message as the refusal requirement.
- Packages: `Thinktecture.Runtime.Extensions` owns admission and dispatch; `LanguageExt.Core` owns accumulating admission, closed-fault sequencing, and immutable folds; `Rasm.Domain` owns `Op`, `Fault.InvalidValue`, and `Fault.OutOfRange`; `UnitsNet` owns runtime-selected quantity parsing and `IQuantity.As` unit projection; `CutterForm` carries MTConnect-derived ISO-13399 geometry and its `CutterFamily` decides whether a cusp exists at all.
- Growth: a geometric characteristic, fit letter, zone modifier, surface parameter, stackup method, or general-tolerance class is one vocabulary row; a payload distinction is one union case; a tabular standard revision is seed data under the existing admission proof.
- Boundary: `IToleranceEncoder` is the open egress strategy; format and culture state close inside its implementation, so `ToleranceRequest.Project` carries one policy value instead of delegate and provider knobs. Roughness correspondence is not a strategy — `SurfaceParameter.RaRatio` is the declared datum, and a parameter without one refuses.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Spec;

// --- [VOCABULARIES] -------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FeatureClass {
    public static readonly FeatureClass Form = new("form");
    public static readonly FeatureClass Orientation = new("orientation");
    public static readonly FeatureClass Location = new("location");
    public static readonly FeatureClass Runout = new("runout");
}

[SmartEnum<string>]
public sealed partial class FeatureScope {
    public static readonly FeatureScope Surface = new("surface");
    public static readonly FeatureScope Axis = new("axis");
    public static readonly FeatureScope MedianLine = new("median-line");
    public static readonly FeatureScope MedianPlane = new("median-plane");
    public static readonly FeatureScope CenterPoint = new("center-point");
}

[SmartEnum<string>]
public sealed partial class DatumUse {
    public static readonly DatumUse None = new("none", static count => count == 0);
    public static readonly DatumUse Optional = new("optional", static _ => true);
    public static readonly DatumUse Required = new("required", static count => count > 0);

    [UseDelegateFromConstructor]
    public partial bool Admits(int count);
}

[SmartEnum<string>]
public sealed partial class ToleranceZoneKind {
    public static readonly ToleranceZoneKind Bilateral = new("bilateral", string.Empty);
    public static readonly ToleranceZoneKind Unilateral = new("unilateral", string.Empty);
    public static readonly ToleranceZoneKind Diameter = new("diameter", "⌀");
    public static readonly ToleranceZoneKind Spherical = new("spherical", "S⌀");
    public static readonly ToleranceZoneKind Profile = new("profile", string.Empty);
    public static readonly ToleranceZoneKind Projected = new("projected", "Ⓟ");
    public static readonly ToleranceZoneKind UnequallyDisposed = new("unequally-disposed", "UZ");

    public string Prefix { get; }
}

[SmartEnum<string>]
public sealed partial class FeatureCharacteristic {
    public static readonly FeatureCharacteristic Straightness = Row("straightness", "—", FeatureClass.Form, DatumUse.None,
        static scope => scope == FeatureScope.Surface || scope == FeatureScope.Axis || scope == FeatureScope.MedianLine,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianLine,
        static zone => zone == ToleranceZoneKind.Bilateral || zone == ToleranceZoneKind.Diameter);
    public static readonly FeatureCharacteristic Flatness = Row("flatness", "⏥", FeatureClass.Form, DatumUse.None,
        static scope => scope == FeatureScope.Surface || scope == FeatureScope.MedianPlane,
        static scope => scope == FeatureScope.MedianPlane,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic Circularity = Surface("circularity", "○", FeatureClass.Form, DatumUse.None,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic Cylindricity = Surface("cylindricity", "⌭", FeatureClass.Form, DatumUse.None,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic ProfileLine = Profile("profile-line", "⌒");
    public static readonly FeatureCharacteristic ProfileSurface = Profile("profile-surface", "⌓");
    public static readonly FeatureCharacteristic Parallelism = Orientation("parallelism", "∥");
    public static readonly FeatureCharacteristic Perpendicularity = Orientation("perpendicularity", "⊥");
    public static readonly FeatureCharacteristic Angularity = Orientation("angularity", "∠");
    public static readonly FeatureCharacteristic Position = Row("position", "⌖", FeatureClass.Location, DatumUse.Required,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane || scope == FeatureScope.CenterPoint,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane || scope == FeatureScope.CenterPoint,
        static zone => zone == ToleranceZoneKind.Diameter || zone == ToleranceZoneKind.Spherical || zone == ToleranceZoneKind.Projected);
    public static readonly FeatureCharacteristic Concentricity = Row("concentricity", "◎", FeatureClass.Location, DatumUse.Required,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.CenterPoint, static _ => false,
        static zone => zone == ToleranceZoneKind.Diameter);
    public static readonly FeatureCharacteristic Symmetry = Row("symmetry", "⌯", FeatureClass.Location, DatumUse.Required,
        static scope => scope == FeatureScope.MedianPlane, static _ => false,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic CircularRunout = Runout("circular-runout", "↗");
    public static readonly FeatureCharacteristic TotalRunout = Runout("total-runout", "⌰");

    private static FeatureCharacteristic Row(string key, string symbol, FeatureClass @class, DatumUse datums,
        Func<FeatureScope, bool> admitsScope, Func<FeatureScope, bool> admitsMaterial,
        Func<ToleranceZoneKind, bool> admitsZone) =>
        new(key, symbol, @class, datums, false, admitsScope, admitsMaterial, admitsZone);
    private static FeatureCharacteristic Surface(string key, string symbol, FeatureClass @class, DatumUse datums,
        Func<ToleranceZoneKind, bool> admitsZone) => Row(key, symbol, @class, datums,
            static scope => scope == FeatureScope.Surface, static _ => false, admitsZone);
    private static FeatureCharacteristic Profile(string key, string symbol) =>
        new(key, symbol, FeatureClass.Orientation, DatumUse.Optional, true,
            static scope => scope == FeatureScope.Surface, static _ => false,
            static zone => zone == ToleranceZoneKind.Profile || zone == ToleranceZoneKind.UnequallyDisposed);
    private static FeatureCharacteristic Orientation(string key, string symbol) =>
        Row(key, symbol, FeatureClass.Orientation, DatumUse.Required,
            static scope => scope == FeatureScope.Surface || scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane,
            static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane,
            static zone => zone == ToleranceZoneKind.Bilateral || zone == ToleranceZoneKind.Diameter);
    private static FeatureCharacteristic Runout(string key, string symbol) =>
        Surface(key, symbol, FeatureClass.Runout, DatumUse.Required,
            static zone => zone == ToleranceZoneKind.Bilateral);

    public string Symbol { get; }
    public FeatureClass Class { get; }
    public DatumUse Datums { get; }
    public bool ProfileContextual { get; }

    [UseDelegateFromConstructor]
    public partial bool AdmitsScope(FeatureScope scope);
    [UseDelegateFromConstructor]
    public partial bool AdmitsMaterial(FeatureScope scope);
    [UseDelegateFromConstructor]
    public partial bool AdmitsZone(ToleranceZoneKind zone);

    public FeatureClass EffectiveClass(int datumCount) => ProfileContextual && datumCount == 0 ? FeatureClass.Form : Class;
}

[SmartEnum<string>]
public sealed partial class FeatureGeometry {
    public static readonly FeatureGeometry Internal = new("internal",
        static (lowerMm, upperMm) => (lowerMm, upperMm), static (materialMm, widthMm) => materialMm - widthMm);
    public static readonly FeatureGeometry External = new("external",
        static (lowerMm, upperMm) => (upperMm, lowerMm), static (materialMm, widthMm) => materialMm + widthMm);

    [UseDelegateFromConstructor]
    public partial (double MaximumMm, double LeastMm) Material(double lowerMm, double upperMm);
    [UseDelegateFromConstructor]
    public partial double Boundary(double materialMm, double widthMm);
}

[SmartEnum<string>]
public sealed partial class MaterialCondition {
    public static readonly MaterialCondition Regardless = new("rfs", "Ⓢ",
        static (widthMm, _) => widthMm, static (_, _) => Option<(double VirtualMm, double ResultantMm)>.None);
    public static readonly MaterialCondition Maximum = new("mmc", "Ⓜ",
        static (widthMm, departureMm) => widthMm + departureMm,
        static (size, widthMm) => Some((
            size.Geometry.Boundary(size.MaximumMaterialMm, widthMm),
            size.Geometry.Boundary(size.LeastMaterialMm, -(widthMm + size.RangeMm)))));
    public static readonly MaterialCondition Least = new("lmc", "Ⓛ",
        static (widthMm, departureMm) => widthMm + departureMm,
        static (size, widthMm) => Some((
            size.Geometry.Boundary(size.LeastMaterialMm, -widthMm),
            size.Geometry.Boundary(size.MaximumMaterialMm, widthMm + size.RangeMm))));

    public string Symbol { get; }

    [UseDelegateFromConstructor]
    public partial double Effective(double widthMm, double departureMm);
    [UseDelegateFromConstructor]
    public partial Option<(double VirtualMm, double ResultantMm)> Boundaries(FeatureSize size, double widthMm);
}

[SmartEnum<string>]
public sealed partial class ZoneModifier {
    public static readonly ZoneModifier TangentPlane = new("tangent-plane", "Ⓣ",
        static (characteristic, scope) => scope == FeatureScope.Surface
            && (characteristic.Class == FeatureClass.Orientation || characteristic.Class == FeatureClass.Form));
    public static readonly ZoneModifier FreeState = Anywhere("free-state", "Ⓕ");
    public static readonly ZoneModifier Statistical = Anywhere("statistical", "〈ST〉");
    public static readonly ZoneModifier CommonZone = Associated("common-zone", "CZ");
    public static readonly ZoneModifier ContinuousFeature = new("continuous-feature", "〈CF〉",
        static (_, scope) => scope == FeatureScope.Surface);
    public static readonly ZoneModifier AllAround = Profiled("all-around", "○");
    public static readonly ZoneModifier AllOver = Profiled("all-over", "◎");
    public static readonly ZoneModifier Envelope = Sized("envelope", "Ⓔ");
    public static readonly ZoneModifier Independency = Sized("independency", "Ⓘ");
    public static readonly ZoneModifier Reciprocity = Sized("reciprocity", "Ⓡ");
    public static readonly ZoneModifier MinimumCircumscribed = Associated("minimum-circumscribed", "Ⓒ");
    public static readonly ZoneModifier MaximumInscribed = Associated("maximum-inscribed", "Ⓧ");
    public static readonly ZoneModifier LeastSquares = Associated("least-squares", "Ⓖ");
    public static readonly ZoneModifier MinimaxTangent = Associated("minimax-tangent", "Ⓝ");

    private static ZoneModifier Anywhere(string key, string symbol) => new(key, symbol, static (_, _) => true);
    private static ZoneModifier Associated(string key, string symbol) => new(key, symbol,
        static (characteristic, _) => characteristic.Class != FeatureClass.Runout);
    private static ZoneModifier Profiled(string key, string symbol) => new(key, symbol,
        static (characteristic, _) => characteristic.ProfileContextual);
    private static ZoneModifier Sized(string key, string symbol) => new(key, symbol,
        static (_, scope) => scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane
            || scope == FeatureScope.CenterPoint);

    public string Symbol { get; }

    [UseDelegateFromConstructor]
    public partial bool Admits(FeatureCharacteristic characteristic, FeatureScope scope);
}

[SmartEnum<string>]
public sealed partial class DatumPrecedence {
    public static readonly DatumPrecedence Primary = new("primary", 1);
    public static readonly DatumPrecedence Secondary = new("secondary", 2);
    public static readonly DatumPrecedence Tertiary = new("tertiary", 3);

    public int Order { get; }
}

[SmartEnum<string>]
public sealed partial class QifKind {
    public static readonly QifKind FeatureControlFrame = new("feature-control-frame");
    public static readonly QifKind DimensionalTolerance = new("dimensional-tolerance");
    public static readonly QifKind SurfaceTexture = new("surface-texture");
    public static readonly QifKind DatumSystem = new("datum-system");
    public static readonly QifKind GeneralTolerance = new("general-tolerance");
}

[SmartEnum<string>]
public sealed partial class FitMember {
    public static readonly FitMember Hole = new("hole");
    public static readonly FitMember Shaft = new("shaft");
}

[SmartEnum<string>]
public sealed partial class FitBound {
    public static readonly FitBound Lower = new("lower",
        static (fundamentalUm, gradeUm) => (fundamentalUm, fundamentalUm + gradeUm));
    public static readonly FitBound Upper = new("upper",
        static (fundamentalUm, gradeUm) => (fundamentalUm - gradeUm, fundamentalUm));
    public static readonly FitBound Symmetric = new("symmetric",
        static (_, gradeUm) => (-0.5 * gradeUm, 0.5 * gradeUm));

    [UseDelegateFromConstructor]
    public partial (double LowerUm, double UpperUm) Deviations(double fundamentalUm, double gradeUm);
}

[SmartEnum<string>]
public sealed partial class FitCharacter {
    public static readonly FitCharacter Clearance = new("clearance");
    public static readonly FitCharacter Transition = new("transition");
    public static readonly FitCharacter Interference = new("interference");
}

[SmartEnum<string>]
public sealed partial class FitLetter {
    public static readonly FitLetter A = new("a");
    public static readonly FitLetter B = new("b");
    public static readonly FitLetter C = new("c");
    public static readonly FitLetter Cd = new("cd");
    public static readonly FitLetter D = new("d");
    public static readonly FitLetter E = new("e");
    public static readonly FitLetter Ef = new("ef");
    public static readonly FitLetter F = new("f");
    public static readonly FitLetter Fg = new("fg");
    public static readonly FitLetter G = new("g");
    public static readonly FitLetter H = new("h");
    public static readonly FitLetter Js = new("js");
    public static readonly FitLetter J = new("j");
    public static readonly FitLetter K = new("k");
    public static readonly FitLetter M = new("m");
    public static readonly FitLetter N = new("n");
    public static readonly FitLetter P = new("p");
    public static readonly FitLetter R = new("r");
    public static readonly FitLetter S = new("s");
    public static readonly FitLetter T = new("t");
    public static readonly FitLetter U = new("u");
    public static readonly FitLetter V = new("v");
    public static readonly FitLetter X = new("x");
    public static readonly FitLetter Y = new("y");
    public static readonly FitLetter Z = new("z");
    public static readonly FitLetter Za = new("za");
    public static readonly FitLetter Zb = new("zb");
    public static readonly FitLetter Zc = new("zc");
}

[SmartEnum<string>]
public sealed partial class ItGradeName {
    public static readonly ItGradeName It01 = new("IT01", -1, static d => Rounded(0.3 + (0.008 * d)));
    public static readonly ItGradeName It0 = new("IT0", 0, static d => Rounded(0.5 + (0.012 * d)));
    public static readonly ItGradeName It1 = new("IT1", 1, static d => Rounded(0.8 + (0.020 * d)));
    public static readonly ItGradeName It2 = Interpolated("IT2", 2);
    public static readonly ItGradeName It3 = Interpolated("IT3", 3);
    public static readonly ItGradeName It4 = Interpolated("IT4", 4);
    public static readonly ItGradeName It5 = Multiple("IT5", 5, 7.0);
    public static readonly ItGradeName It6 = Multiple("IT6", 6, 10.0);
    public static readonly ItGradeName It7 = Multiple("IT7", 7, 16.0);
    public static readonly ItGradeName It8 = Multiple("IT8", 8, 25.0);
    public static readonly ItGradeName It9 = Multiple("IT9", 9, 40.0);
    public static readonly ItGradeName It10 = Multiple("IT10", 10, 64.0);
    public static readonly ItGradeName It11 = Multiple("IT11", 11, 100.0);
    public static readonly ItGradeName It12 = Multiple("IT12", 12, 160.0);
    public static readonly ItGradeName It13 = Multiple("IT13", 13, 250.0);
    public static readonly ItGradeName It14 = Multiple("IT14", 14, 400.0);
    public static readonly ItGradeName It15 = Multiple("IT15", 15, 640.0);
    public static readonly ItGradeName It16 = Multiple("IT16", 16, 1000.0);
    public static readonly ItGradeName It17 = Multiple("IT17", 17, 1600.0);
    public static readonly ItGradeName It18 = Multiple("IT18", 18, 2500.0);

    private static ItGradeName Interpolated(string key, int grade) => new(key, grade,
        diameterMm => InterpolatedMicrometers(grade, diameterMm));
    private static ItGradeName Multiple(string key, int grade, double factor) => new(key, grade, d => Rounded(factor * Unit(d)));
    private static double InterpolatedMicrometers(int grade, double diameterMm) =>
        Rounded((0.8 + (0.020 * diameterMm))
            * Math.Pow(7.0 * Unit(diameterMm) / (0.8 + (0.020 * diameterMm)), (grade - 1) / 4.0));
    private static double Unit(double diameterMm) => (0.45 * Math.Cbrt(diameterMm)) + (0.001 * diameterMm);
    private static double Rounded(double micrometers) => Math.Round(micrometers, micrometers < 2.0 ? 1 : 0,
        MidpointRounding.AwayFromZero);

    public int Number { get; }

    [UseDelegateFromConstructor]
    public partial double Micrometers(double geometricMeanMm);
}

[SmartEnum<string>]
public sealed partial class GeneralToleranceClass {
    public static readonly GeneralToleranceClass Fine = new("f");
    public static readonly GeneralToleranceClass Medium = new("m");
    public static readonly GeneralToleranceClass Coarse = new("c");
    public static readonly GeneralToleranceClass VeryCoarse = new("v");
}

[SmartEnum<string>]
public sealed partial class GeneralToleranceKind {
    public static readonly GeneralToleranceKind Linear = Measured("linear");
    public static readonly GeneralToleranceKind ExternalRadius = Measured("external-radius");
    public static readonly GeneralToleranceKind Chamfer = Measured("chamfer");
    public static readonly GeneralToleranceKind Angular = new("angular",
        static limit => limit is GeneralLimit.Angular);
    public static readonly GeneralToleranceKind Straightness = Measured("straightness");
    public static readonly GeneralToleranceKind Flatness = Measured("flatness");
    public static readonly GeneralToleranceKind Perpendicularity = Measured("perpendicularity");
    public static readonly GeneralToleranceKind Symmetry = Measured("symmetry");
    public static readonly GeneralToleranceKind Runout = Measured("runout");

    private static GeneralToleranceKind Measured(string key) => new(key, static limit => limit is GeneralLimit.Linear);

    [UseDelegateFromConstructor]
    public partial bool Admits(GeneralLimit limit);
}

[SmartEnum<string>]
public sealed partial class SurfaceProfile {
    public static readonly SurfaceProfile Roughness = new("roughness");
    public static readonly SurfaceProfile Waviness = new("waviness");
    public static readonly SurfaceProfile Primary = new("primary");
}

[SmartEnum<string>]
public sealed partial class SurfaceMeasure {
    public static readonly SurfaceMeasure Amplitude = Positive("amplitude");
    public static readonly SurfaceMeasure Spacing = Positive("spacing");
    public static readonly SurfaceMeasure Ratio = Percent("ratio");
    public static readonly SurfaceMeasure LevelRatio = Percent("level-ratio");
    public static readonly SurfaceMeasure Difference = Positive("difference");
    public static readonly SurfaceMeasure Shape = new("shape",
        static limit => limit is not null && limit.Valid(static value => double.IsFinite(value)));

    private static SurfaceMeasure Positive(string key) => new(key,
        static limit => limit is not null && limit.Valid(static value => double.IsFinite(value) && value > 0.0));
    private static SurfaceMeasure Percent(string key) => new(key,
        static limit => limit is not null && limit.Valid(static value => double.IsFinite(value) && value is >= 0.0 and <= 100.0));

    [UseDelegateFromConstructor]
    public partial bool Admits(SurfaceLimit limit);
}

[SmartEnum<string>]
public sealed partial class SurfaceParameter {
    public static readonly SurfaceParameter Ra = Converted("Ra", 1.0);
    public static readonly SurfaceParameter Rq = Converted("Rq", double.Sqrt(2.0 / Math.PI));
    public static readonly SurfaceParameter Rz = Converted("Rz", 0.25);
    public static readonly SurfaceParameter Rt = Converted("Rt", 1.0 / 5.5);
    public static readonly SurfaceParameter Rp = Converted("Rp", 0.5);
    public static readonly SurfaceParameter Rv = Converted("Rv", 0.5);
    public static readonly SurfaceParameter Rc = Converted("Rc", 0.25);
    public static readonly SurfaceParameter Rk = Row("Rk", SurfaceProfile.Roughness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Rpk = Row("Rpk", SurfaceProfile.Roughness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Rvk = Row("Rvk", SurfaceProfile.Roughness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Rsk = Row("Rsk", SurfaceProfile.Roughness, SurfaceMeasure.Shape);
    public static readonly SurfaceParameter Rku = Row("Rku", SurfaceProfile.Roughness, SurfaceMeasure.Shape);
    public static readonly SurfaceParameter Rdq = Row("Rdq", SurfaceProfile.Roughness, SurfaceMeasure.Shape);
    public static readonly SurfaceParameter Rsm = Row("RSm", SurfaceProfile.Roughness, SurfaceMeasure.Spacing);
    public static readonly SurfaceParameter Mr1 = Row("Mr1", SurfaceProfile.Roughness, SurfaceMeasure.Ratio);
    public static readonly SurfaceParameter Mr2 = Row("Mr2", SurfaceProfile.Roughness, SurfaceMeasure.Ratio);
    public static readonly SurfaceParameter Rmr = Row("Rmr", SurfaceProfile.Roughness, SurfaceMeasure.LevelRatio);
    public static readonly SurfaceParameter Rdc = Row("Rdc", SurfaceProfile.Roughness, SurfaceMeasure.Difference);
    public static readonly SurfaceParameter Wa = Row("Wa", SurfaceProfile.Waviness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Wq = Row("Wq", SurfaceProfile.Waviness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Wz = Row("Wz", SurfaceProfile.Waviness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Wt = Row("Wt", SurfaceProfile.Waviness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Wp = Row("Wp", SurfaceProfile.Waviness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Wv = Row("Wv", SurfaceProfile.Waviness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Wc = Row("Wc", SurfaceProfile.Waviness, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Wsk = Row("Wsk", SurfaceProfile.Waviness, SurfaceMeasure.Shape);
    public static readonly SurfaceParameter Wku = Row("Wku", SurfaceProfile.Waviness, SurfaceMeasure.Shape);
    public static readonly SurfaceParameter Wsm = Row("WSm", SurfaceProfile.Waviness, SurfaceMeasure.Spacing);
    public static readonly SurfaceParameter Pa = Row("Pa", SurfaceProfile.Primary, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Pq = Row("Pq", SurfaceProfile.Primary, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Pz = Row("Pz", SurfaceProfile.Primary, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Pt = Row("Pt", SurfaceProfile.Primary, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Pp = Row("Pp", SurfaceProfile.Primary, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Pv = Row("Pv", SurfaceProfile.Primary, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Pc = Row("Pc", SurfaceProfile.Primary, SurfaceMeasure.Amplitude);
    public static readonly SurfaceParameter Psk = Row("Psk", SurfaceProfile.Primary, SurfaceMeasure.Shape);
    public static readonly SurfaceParameter Pku = Row("Pku", SurfaceProfile.Primary, SurfaceMeasure.Shape);
    public static readonly SurfaceParameter Psm = Row("PSm", SurfaceProfile.Primary, SurfaceMeasure.Spacing);

    private static SurfaceParameter Row(string key, SurfaceProfile profile, SurfaceMeasure measure) =>
        new(key, profile, measure, None);
    private static SurfaceParameter Converted(string key, double raRatio) =>
        new(key, SurfaceProfile.Roughness, SurfaceMeasure.Amplitude, Some(raRatio));

    public SurfaceProfile Profile { get; }
    public SurfaceMeasure Measure { get; }
    public Option<double> RaRatio { get; }
}

[Union]
public abstract partial record SurfaceLimit {
    private SurfaceLimit() { }
    public sealed record Exact(double Value) : SurfaceLimit;
    public sealed record Maximum(double Value) : SurfaceLimit;
    public sealed record Minimum(double Value) : SurfaceLimit;
    public sealed record Range(double Lower, double Upper) : SurfaceLimit;

    internal bool Valid(Func<double, bool> admits) => admits is not null && Switch(
        exact: row => admits(row.Value),
        maximum: row => admits(row.Value),
        minimum: row => admits(row.Value),
        range: row => admits(row.Lower) && admits(row.Upper) && row.Lower <= row.Upper);

    public Option<double> Upper() => Switch(
        exact: static row => Some(row.Value),
        maximum: static row => Some(row.Value),
        minimum: static _ => None,
        range: static row => Some(row.Upper));
}

[SmartEnum<string>]
public sealed partial class SurfaceLay {
    public static readonly SurfaceLay Parallel = new("parallel");
    public static readonly SurfaceLay Perpendicular = new("perpendicular");
    public static readonly SurfaceLay Crossed = new("crossed");
    public static readonly SurfaceLay Multidirectional = new("multidirectional");
    public static readonly SurfaceLay Circular = new("circular");
    public static readonly SurfaceLay Radial = new("radial");
    public static readonly SurfaceLay Particulate = new("particulate");
}

[SmartEnum<string>]
public sealed partial class ProcessMark {
    public static readonly ProcessMark Any = new("any");
    public static readonly ProcessMark RemovalRequired = new("removal-required");
    public static readonly ProcessMark RemovalProhibited = new("removal-prohibited");
}

[SmartEnum<string>]
public sealed partial class SpecAxis {
    public static readonly SpecAxis Length = new("length", typeof(UnitsNet.Length), LengthUnit.Millimeter);
    public static readonly SpecAxis Angle = new("angle", typeof(UnitsNet.Angle), AngleUnit.Degree);
    public static readonly SpecAxis Roughness = new("roughness", typeof(UnitsNet.Length), LengthUnit.Micrometer);
    public static readonly SpecAxis Reference = new("reference", typeof(UnitsNet.Temperature), TemperatureUnit.DegreeCelsius);
    public static readonly SpecAxis Restraint = new("restraint", typeof(UnitsNet.Force), ForceUnit.Newton);

    public Type QuantityType { get; }
    public Enum CanonicalUnit { get; }

    public double Canonical(IQuantity value) => value.As(CanonicalUnit);
}

// --- [ADMITTED_MODELS] ----------------------------------------------------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct ZoneWidth {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0.0 ? null : new ValidationError(message: "tolerance-zone-width");
}

[ValueObject<double>]
public readonly partial struct FinishingAllowanceFactor {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value >= 0.0
            ? null : new ValidationError(message: "finishing-allowance-factor");
}

[ValueObject<double>]
public readonly partial struct ScallopFactor {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0.0 ? null : new ValidationError(message: "scallop-factor");
}

[ComplexValueObject]
public sealed partial class DiameterBand {
    public double LowerMm { get; }
    public double UpperMm { get; }
    public double ReferenceMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double lowerMm, ref double upperMm,
        ref double referenceMm) =>
        validationError = double.IsFinite(lowerMm) && lowerMm >= 0.0 && double.IsFinite(upperMm) && upperMm > lowerMm
            && double.IsFinite(referenceMm) && referenceMm > lowerMm && referenceMm <= upperMm
            ? null : new ValidationError(message: "diameter-band");

    public bool Contains(double diameterMm) => diameterMm > LowerMm && diameterMm <= UpperMm;
}

[Union]
public abstract partial record ToleranceZone(ZoneWidth Width, Set<ZoneModifier> Modifiers) {
    public sealed record Bilateral(ZoneWidth Width, Set<ZoneModifier> Modifiers) : ToleranceZone(Width, Modifiers);
    public sealed record Unilateral(ZoneWidth Width, double OffsetMm, Set<ZoneModifier> Modifiers) : ToleranceZone(Width, Modifiers);
    public sealed record Diameter(ZoneWidth Width, Set<ZoneModifier> Modifiers) : ToleranceZone(Width, Modifiers);
    public sealed record Spherical(ZoneWidth Width, Set<ZoneModifier> Modifiers) : ToleranceZone(Width, Modifiers);
    public sealed record Profile(ZoneWidth Width, Set<ZoneModifier> Modifiers) : ToleranceZone(Width, Modifiers);
    public sealed record Projected(ZoneWidth Width, double HeightMm, Set<ZoneModifier> Modifiers) : ToleranceZone(Width, Modifiers);
    public sealed record Unequal(ZoneWidth Width, double OffsetMm, Set<ZoneModifier> Modifiers) : ToleranceZone(Width, Modifiers);

    public ToleranceZoneKind Kind() => Shape().Kind;

    public (Option<double> ProjectedHeightMm, Option<double> UnequalOffsetMm) Dimensions() {
        var shape = Shape();
        return (shape.ProjectedHeightMm, shape.UnequalOffsetMm);
    }

    public bool Valid() => double.IsFinite(Width.ToValue()) && Width.ToValue() > 0.0 && Shape().CaseValid;

    private (ToleranceZoneKind Kind, Option<double> ProjectedHeightMm, Option<double> UnequalOffsetMm, bool CaseValid) Shape() => Switch(
        bilateral: static _ => (ToleranceZoneKind.Bilateral, None, None, true),
        unilateral: static zone => (ToleranceZoneKind.Unilateral, None, Some(zone.OffsetMm), double.IsFinite(zone.OffsetMm)),
        diameter: static _ => (ToleranceZoneKind.Diameter, None, None, true),
        spherical: static _ => (ToleranceZoneKind.Spherical, None, None, true),
        profile: static _ => (ToleranceZoneKind.Profile, None, None, true),
        projected: static zone => (ToleranceZoneKind.Projected, Some(zone.HeightMm), None,
            double.IsFinite(zone.HeightMm) && zone.HeightMm > 0.0),
        unequal: static zone => (ToleranceZoneKind.UnequallyDisposed, None, Some(zone.OffsetMm),
            double.IsFinite(zone.OffsetMm) && zone.OffsetMm >= 0.0 && zone.OffsetMm <= zone.Width.ToValue()));
}

[ComplexValueObject]
public sealed partial class DatumReference {
    public string Label { get; }
    public DatumPrecedence Precedence { get; }
    public MaterialCondition Material { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string label,
        ref DatumPrecedence precedence, ref MaterialCondition material) {
        label = label?.Trim().ToUpperInvariant() ?? string.Empty;
        validationError = label.Length > 0 && precedence is not null && material is not null
            ? null : new ValidationError(message: "datum-reference");
    }
}

[ComplexValueObject]
public sealed partial class DatumPoint {
    public double XMm { get; }
    public double YMm { get; }
    public double ZMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double xMm, ref double yMm, ref double zMm) =>
        validationError = double.IsFinite(xMm) && double.IsFinite(yMm) && double.IsFinite(zMm)
            ? null : new ValidationError(message: "datum-point");
}

[Union]
public abstract partial record DatumTarget(string Label, DatumPoint At) {
    public sealed record Point(string Label, DatumPoint At) : DatumTarget(Label, At);
    public sealed record Line(string Label, DatumPoint At, double LengthMm) : DatumTarget(Label, At);
    public sealed record Area(string Label, DatumPoint At, double LengthMm, double WidthMm) : DatumTarget(Label, At);
}

[ComplexValueObject]
public sealed partial class DatumSystem {
    public Arr<DatumReference> References { get; }
    public QifKind Qif => QifKind.DatumSystem;
    public string Annotation => string.Concat(References.Map(static row =>
        string.Concat("|", row.Label, row.Material.Symbol)));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<DatumReference> references) {
        if (references.ForAll(static row => row is not null))
            references = references.OrderBy(static row => row.Precedence.Order).ToArr();
        validationError = references.Count <= 3 && references.ForAll(static row => row is not null)
            && references.Map(static row => row.Label).Distinct().Count == references.Count
            && references.Map(static row => row.Precedence).Distinct().Count == references.Count
            && references.ForAll(row => row.Precedence.Order <= references.Count)
            ? null : new ValidationError(message: "datum-system");
    }
}

[ComplexValueObject]
public sealed partial class BasicDimension {
    public string Label { get; }
    public double NominalMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string label, ref double nominalMm) {
        label = label?.Trim() ?? string.Empty;
        validationError = label.Length > 0 && double.IsFinite(nominalMm) ? null : new ValidationError(message: "basic-dimension");
    }
}

[ComplexValueObject]
public sealed partial class CompositeSegment {
    public ZoneWidth Width { get; }
    public Set<ZoneModifier> Modifiers { get; }
    public DatumSystem Datums { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ZoneWidth width,
        ref Set<ZoneModifier> modifiers, ref DatumSystem datums) =>
        validationError = double.IsFinite(width.ToValue()) && width.ToValue() > 0.0
            && datums is not null && datums.References.Count > 0 ? null : new ValidationError(message: "composite-segment");
}

[ComplexValueObject]
public sealed partial class FrameExtension {
    public Arr<BasicDimension> Basics { get; }
    public Arr<DatumTarget> Targets { get; }
    public Option<CompositeSegment> Composite { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<BasicDimension> basics,
        ref Arr<DatumTarget> targets, ref Option<CompositeSegment> composite) =>
        validationError = basics.ForAll(static row => row is not null) && targets.ForAll(static row => row is not null)
            && basics.Map(static row => row.Label).Distinct().Count == basics.Count
            && targets.Map(static row => row.Label).Distinct().Count == targets.Count
            && targets.ForAll(ValidTarget)
                ? null : new ValidationError(message: "frame-extension");

    private static bool ValidTarget(DatumTarget target) => target.Switch(
        point: static row => !string.IsNullOrWhiteSpace(row.Label) && row.At is not null,
        line: static row => !string.IsNullOrWhiteSpace(row.Label) && row.At is not null
            && double.IsFinite(row.LengthMm) && row.LengthMm > 0.0,
        area: static row => !string.IsNullOrWhiteSpace(row.Label) && row.At is not null
            && double.IsFinite(row.LengthMm) && row.LengthMm > 0.0
            && double.IsFinite(row.WidthMm) && row.WidthMm > 0.0);

    // A target label carries its datum letter as a prefix, and a composite lower segment refines the upper datums.
    public bool Anchored(DatumSystem datums) => datums is not null
        && Targets.ForAll(target => datums.References.Exists(row =>
            target.Label.StartsWith(row.Label, StringComparison.Ordinal)))
        && Composite.ForAll(segment => segment.Datums.References.ForAll(row =>
            datums.References.Exists(upper => upper.Label == row.Label)));
}

[ComplexValueObject]
public sealed partial class FeatureSize {
    public FeatureGeometry Geometry { get; }
    public double LowerMm { get; }
    public double UpperMm { get; }
    public double MaximumMaterialMm => Geometry.Material(LowerMm, UpperMm).MaximumMm;
    public double LeastMaterialMm => Geometry.Material(LowerMm, UpperMm).LeastMm;
    public double RangeMm => UpperMm - LowerMm;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FeatureGeometry geometry,
        ref double lowerMm, ref double upperMm) =>
        validationError = geometry is not null && double.IsFinite(lowerMm) && lowerMm > 0.0
            && double.IsFinite(upperMm) && upperMm >= lowerMm ? null : new ValidationError(message: "feature-size");

    public bool Contains(double actualMm) => actualMm >= LowerMm && actualMm <= UpperMm;
}

[ComplexValueObject]
public sealed partial class FeatureControl {
    public CharacteristicId Id { get; }
    public ContentKey Source { get; }
    public FeatureCharacteristic Characteristic { get; }
    public FeatureScope Scope { get; }
    public ToleranceZone Zone { get; }
    public DatumSystem Datums { get; }
    public MaterialCondition Material { get; }
    public FrameExtension Extension { get; }
    public Option<FeatureSize> Size { get; }
    public Option<double> AchievableMm { get; }
    public FeatureClass Class => Characteristic.EffectiveClass(Datums.References.Count);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref CharacteristicId id, ref ContentKey source,
        ref FeatureCharacteristic characteristic, ref FeatureScope scope, ref ToleranceZone zone, ref DatumSystem datums,
        ref MaterialCondition material, ref FrameExtension extension, ref Option<FeatureSize> size,
        ref Option<double> achievableMm) =>
        validationError = id != default && source is not null && characteristic is not null && scope is not null && zone is not null
            && datums is not null && material is not null && extension is not null && zone.Valid()
            && characteristic.Datums.Admits(datums.References.Count) && characteristic.AdmitsScope(scope)
            && characteristic.AdmitsZone(zone.Kind())
            && zone.Modifiers.ForAll(modifier => modifier.Admits(characteristic, scope))
            && (material == MaterialCondition.Regardless || (characteristic.AdmitsMaterial(scope) && size.IsSome))
            && extension.Anchored(datums)
            && achievableMm.ForAll(static value => double.IsFinite(value) && value > 0.0)
                ? null : new ValidationError(message: "feature-control");

    public static Fin<FeatureControl> Admit(ToleranceRequest.Feature raw) =>
        from input in Optional(raw).ToFin(Tolerance.Invalid("feature-control:raw"))
        from admitted in Tolerance.Mint(
            Validate(input.Id, input.Source, input.Characteristic, input.Scope, input.Zone, input.Datums, input.Material,
                input.Extension, input.Size, input.AchievableMm, out FeatureControl? value), value, "feature-control")
        select admitted;
}

[ComplexValueObject]
public sealed partial class FeatureFrameReceipt {
    public CharacteristicId Id => Control.Id;
    public FeatureControl Control { get; }
    public Tolerance.Geometric Specification => new(Control);
    public QifKind Qif => Specification.Qif();
    public FeatureCharacteristic Characteristic => Control.Characteristic;
    public FeatureScope Scope => Control.Scope;
    public ToleranceZoneKind Kind => Control.Zone.Kind();
    public double WidthMm => Control.Zone.Width.ToValue();
    public Arr<ZoneModifier> Modifiers => toSeq(Control.Zone.Modifiers)
        .OrderBy(static modifier => modifier.ToValue()).ToArr();
    public Arr<DatumReference> Datums => Control.Datums.References;
    public MaterialCondition Material => Control.Material;
    public Option<FeatureSize> Size => Control.Size;
    public Option<double> ProjectedHeightMm => Control.Zone.Dimensions().ProjectedHeightMm;
    public Option<double> UnequalOffsetMm => Control.Zone.Dimensions().UnequalOffsetMm;
    public FrameExtension Extension => Control.Extension;
    public Option<double> AchievableMm => Control.AchievableMm;
    public string Annotation => string.Concat(Control.Characteristic.Symbol, Control.Zone.Kind().Prefix,
        WidthMm.ToString("0.###", CultureInfo.InvariantCulture), Control.Material.Symbol,
        string.Concat(Modifiers.Map(static modifier => modifier.Symbol)), Control.Datums.Annotation);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FeatureControl control) =>
        validationError = control is null ? new ValidationError(message: "feature-frame-receipt") : null;
}

[ComplexValueObject]
public sealed partial class ItGrade {
    public ItGradeName Name { get; }
    public DiameterBand Diameter { get; }
    public FinishingAllowanceFactor AllowanceFactor { get; }
    public int Number => Name.Number;
    public double ToleranceMicrometers => Name.Micrometers(Diameter.ReferenceMm);
    public double ToleranceMillimeters => ToleranceMicrometers / 1000.0;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ItGradeName name,
        ref DiameterBand diameter, ref FinishingAllowanceFactor allowanceFactor) =>
        validationError = name is not null && diameter is not null && double.IsFinite(allowanceFactor.ToValue())
            && allowanceFactor.ToValue() >= 0.0 && double.IsFinite(name.Micrometers(diameter.ReferenceMm))
            && name.Micrometers(diameter.ReferenceMm) > 0.0 ? null : new ValidationError(message: "it-grade");
}

public readonly record struct DeviationSeed(FitMember Member, FitLetter Letter, DiameterBand Diameter,
    Option<ItGradeName> Grade, FitBound FundamentalBound, double FundamentalMicrometers);

[ComplexValueObject]
public sealed partial class FitStandard {
    public Arr<DiameterBand> Diameters { get; }
    public Arr<DeviationSeed> Deviations { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<DiameterBand> diameters,
        ref Arr<DeviationSeed> deviations) =>
        validationError = diameters.Count > 0 && deviations.Count > 0
            && deviations.ForAll(static row => row.Member is not null && row.Letter is not null && row.Diameter is not null
                && row.FundamentalBound is not null && double.IsFinite(row.FundamentalMicrometers))
            && deviations.ForAll(row => diameters.Exists(candidate => candidate == row.Diameter))
            && deviations.Map(static row => (row.Member, row.Letter, row.Diameter, row.Grade)).Distinct().Count == deviations.Count
                ? null : new ValidationError(message: "fit-standard");

    // A grade-specific seed outranks the grade-agnostic row of the same member, letter, and band.
    public Fin<DeviationSeed> Resolve(FitMember member, FitLetter letter, ItGradeName grade, DiameterBand diameter) =>
        toSeq(Deviations)
            .Filter(row => row.Member == member && row.Letter == letter && row.Diameter == diameter)
            .Fold(Option<DeviationSeed>.None, (held, row) =>
                row.Grade.Exists(candidate => candidate == grade) ? Some(row)
                    : held.IsSome || row.Grade.IsSome ? held : Some(row))
            .ToFin(Tolerance.Invalid("fit-standard",
                $"deviation seed for {member.ToValue()}{letter.ToValue()}{grade.ToValue()}"));
}

[ComplexValueObject]
public sealed partial class FitClass {
    public FitMember Member { get; }
    public FitLetter Letter { get; }
    public ItGrade Grade { get; }
    public FitBound FundamentalBound { get; }
    public double FundamentalMicrometers { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FitMember member,
        ref FitLetter letter, ref ItGrade grade, ref FitBound fundamentalBound, ref double fundamentalMicrometers) =>
        validationError = member is not null && letter is not null && grade is not null && fundamentalBound is not null
            && double.IsFinite(fundamentalMicrometers) ? null : new ValidationError(message: "fit-class");

    public (double LowerUm, double UpperUm) Limits =>
        FundamentalBound.Deviations(FundamentalMicrometers, Grade.ToleranceMicrometers);

    public string Designation => Member == FitMember.Hole
        ? string.Concat(Letter.ToValue().ToUpperInvariant(), Grade.Number.ToString(CultureInfo.InvariantCulture))
        : string.Concat(Letter.ToValue(), Grade.Number.ToString(CultureInfo.InvariantCulture));

    public (double LowerMm, double UpperMm) Sizes(double nominalMm) =>
        (nominalMm + (Limits.LowerUm / 1000.0), nominalMm + (Limits.UpperUm / 1000.0));

    public static Fin<FitClass> Admit(FitMember member, FitLetter letter, ItGrade grade, FitStandard standard) =>
        from admittedGrade in Optional(grade).ToFin(Tolerance.Invalid("fit-class:grade"))
        from admittedStandard in Optional(standard).ToFin(Tolerance.Invalid("fit-class:standard"))
        from _ in guard(member is not null && letter is not null, Tolerance.Invalid("fit-class:shape")).ToFin()
        from seed in admittedStandard.Resolve(member, letter, admittedGrade.Name, admittedGrade.Diameter)
        select Create(member, letter, admittedGrade, seed.FundamentalBound, seed.FundamentalMicrometers);
}

[ComplexValueObject]
public sealed partial class FitLimits {
    public ContentKey Source { get; }
    public double NominalMm { get; }
    public FitClass Hole { get; }
    public FitClass Shaft { get; }
    public FitCharacter Character { get; }
    public (double LowerMm, double UpperMm) HoleSizes => Hole.Sizes(NominalMm);
    public (double LowerMm, double UpperMm) ShaftSizes => Shaft.Sizes(NominalMm);
    public double MaxClearanceMm => HoleSizes.UpperMm - ShaftSizes.LowerMm;
    public double MinClearanceMm => HoleSizes.LowerMm - ShaftSizes.UpperMm;
    public string Designation => string.Concat("⌀", NominalMm.ToString("0.###", CultureInfo.InvariantCulture),
        " ", Hole.Designation, "/", Shaft.Designation);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ContentKey source,
        ref double nominalMm, ref FitClass hole, ref FitClass shaft, ref FitCharacter character) {
        if (source is null || hole is null || shaft is null || character is null) {
            validationError = new ValidationError(message: "fit-limits");
            return;
        }
        (double LowerMm, double UpperMm) holeSizes = hole.Sizes(nominalMm);
        (double LowerMm, double UpperMm) shaftSizes = shaft.Sizes(nominalMm);
        double maximum = holeSizes.UpperMm - shaftSizes.LowerMm;
        double minimum = holeSizes.LowerMm - shaftSizes.UpperMm;
        FitCharacter derived = minimum >= 0.0 ? FitCharacter.Clearance
            : maximum <= 0.0 ? FitCharacter.Interference : FitCharacter.Transition;
        validationError = hole.Member == FitMember.Hole && shaft.Member == FitMember.Shaft
            && hole.Grade.Diameter == shaft.Grade.Diameter
            && double.IsFinite(nominalMm) && hole.Grade.Diameter.Contains(nominalMm)
            && double.IsFinite(maximum) && double.IsFinite(minimum) && maximum >= minimum
            && character == derived ? null : new ValidationError(message: "fit-limits");
    }
}

[Union]
public abstract partial record GeneralLimit {
    private GeneralLimit() { }
    public sealed record Linear(double Millimeters) : GeneralLimit;
    public sealed record Angular(double Degrees) : GeneralLimit;

    public bool Valid() => Switch(
        linear: static row => double.IsFinite(row.Millimeters) && row.Millimeters > 0.0,
        angular: static row => double.IsFinite(row.Degrees) && row.Degrees > 0.0);
}

public readonly record struct GeneralSeed(GeneralToleranceClass Class, GeneralToleranceKind Kind,
    DiameterBand Band, GeneralLimit Limit) {
    public bool Overlaps(GeneralSeed other) => Class == other.Class && Kind == other.Kind
        && Band.LowerMm < other.Band.UpperMm && other.Band.LowerMm < Band.UpperMm;
}

[ComplexValueObject]
public sealed partial class GeneralStandard {
    public Arr<GeneralSeed> Seeds { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<GeneralSeed> seeds) =>
        validationError = seeds.Count > 0 && seeds.ForAll(static row => row.Class is not null && row.Kind is not null
            && row.Band is not null && row.Limit is not null && row.Limit.Valid()
            && row.Kind.Admits(row.Limit))
            && seeds.Map(static row => (row.Class, row.Kind, row.Band)).Distinct().Count == seeds.Count
            && !seeds.Exists(left => seeds.Exists(right => left != right && left.Overlaps(right)))
                ? null : new ValidationError(message: "general-standard");

    public Fin<GeneralLimit> Resolve(GeneralToleranceClass @class, GeneralToleranceKind kind, double nominalMm) =>
        Seeds.Filter(row => row.Class == @class && row.Kind == kind && row.Band.Contains(nominalMm))
            .Map(static row => row.Limit).Head.ToFin(Tolerance.Invalid("general-standard:band"));
}

[ComplexValueObject]
public sealed partial class GeneralTolerance {
    public ContentKey Source { get; }
    public GeneralToleranceClass Class { get; }
    public GeneralToleranceKind Kind { get; }
    public double NominalMm { get; }
    public GeneralLimit Limit { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ContentKey source,
        ref GeneralToleranceClass @class, ref GeneralToleranceKind kind, ref double nominalMm, ref GeneralLimit limit) =>
        validationError = source is not null && @class is not null && kind is not null && double.IsFinite(nominalMm)
            && nominalMm > 0.0 && limit is not null && limit.Valid()
            && kind.Admits(limit)
                ? null : new ValidationError(message: "general-tolerance");
}

[Union]
public abstract partial record SurfaceRequirement {
    private SurfaceRequirement() { }
    public sealed record Amplitude(SurfaceParameter Parameter, SurfaceLimit Micrometers) : SurfaceRequirement;
    public sealed record Spacing(SurfaceParameter Parameter, SurfaceLimit Millimeters) : SurfaceRequirement;
    public sealed record Ratio(SurfaceParameter Parameter, SurfaceLimit Percent) : SurfaceRequirement;
    public sealed record LevelRatio(SurfaceParameter Parameter, SurfaceLimit Percent, double LevelMicrometers) : SurfaceRequirement;
    public sealed record Difference(SurfaceParameter Parameter, double FromPercent, double ToPercent,
        SurfaceLimit Micrometers) : SurfaceRequirement;
    public sealed record Shape(SurfaceParameter Parameter, SurfaceLimit Value) : SurfaceRequirement;

    public SurfaceParameter Kind() => Switch(
        amplitude: static row => row.Parameter, spacing: static row => row.Parameter,
        ratio: static row => row.Parameter, levelRatio: static row => row.Parameter,
        difference: static row => row.Parameter, shape: static row => row.Parameter);

    public bool Valid() => Switch(
        amplitude: static row => row.Parameter is not null && row.Parameter.Measure == SurfaceMeasure.Amplitude
            && row.Micrometers is not null && row.Parameter.Measure.Admits(row.Micrometers),
        spacing: static row => row.Parameter is not null && row.Parameter.Measure == SurfaceMeasure.Spacing
            && row.Millimeters is not null && row.Parameter.Measure.Admits(row.Millimeters),
        ratio: static row => row.Parameter is not null && row.Parameter.Measure == SurfaceMeasure.Ratio
            && row.Percent is not null && row.Parameter.Measure.Admits(row.Percent),
        levelRatio: static row => row.Parameter is not null && row.Parameter.Measure == SurfaceMeasure.LevelRatio
            && row.Percent is not null && row.Parameter.Measure.Admits(row.Percent) && double.IsFinite(row.LevelMicrometers),
        difference: static row => row.Parameter is not null && row.Parameter.Measure == SurfaceMeasure.Difference
            && double.IsFinite(row.FromPercent) && double.IsFinite(row.ToPercent)
            && row.FromPercent is >= 0.0 and <= 100.0 && row.ToPercent > row.FromPercent && row.ToPercent <= 100.0
            && row.Micrometers is not null && row.Parameter.Measure.Admits(row.Micrometers),
        shape: static row => row.Parameter is not null && row.Parameter.Measure == SurfaceMeasure.Shape
            && row.Value is not null && row.Parameter.Measure.Admits(row.Value));
}

[ComplexValueObject]
public sealed partial class TransmissionBand {
    public double CutoffMm { get; }
    public double SamplingMm { get; }
    public double EvaluationMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double cutoffMm,
        ref double samplingMm, ref double evaluationMm) =>
        validationError = double.IsFinite(cutoffMm) && cutoffMm > 0.0 && double.IsFinite(samplingMm) && samplingMm > 0.0
            && double.IsFinite(evaluationMm) && evaluationMm >= samplingMm ? null : new ValidationError(message: "transmission-band");
}

[ComplexValueObject]
public sealed partial class SurfaceTexture {
    public ContentKey Source { get; }
    public Arr<SurfaceRequirement> Requirements { get; }
    public SurfaceLay Lay { get; }
    public ProcessMark Mark { get; }
    public Option<TransmissionBand> Band { get; }
    public Option<double> MachiningAllowanceMm { get; }
    public Option<string> Treatment { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ContentKey source,
        ref Arr<SurfaceRequirement> requirements, ref SurfaceLay lay, ref ProcessMark mark, ref Option<TransmissionBand> band,
        ref Option<double> machiningAllowanceMm, ref Option<string> treatment) =>
        validationError = source is not null && requirements.Count > 0 && requirements.ForAll(static row => row is not null)
            && requirements.ForAll(static row => row.Valid())
            && requirements.Map(static row => row.Kind()).Distinct().Count == requirements.Count && lay is not null && mark is not null
            && machiningAllowanceMm.ForAll(static value => double.IsFinite(value) && value >= 0.0)
            && treatment.ForAll(static value => !string.IsNullOrWhiteSpace(value)) ? null : new ValidationError(message: "surface-texture");
}

[ComplexValueObject]
public sealed partial class RaTarget {
    public double Micrometers { get; }
    public ScallopFactor Factor { get; }
    public double ScallopHeightMm => Micrometers * Factor.ToValue() / 1000.0;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double micrometers,
        ref ScallopFactor factor) =>
        validationError = double.IsFinite(micrometers) && micrometers > 0.0 && double.IsFinite(factor.ToValue())
            && factor.ToValue() > 0.0 ? null : new ValidationError(message: "ra-target");

    // SurfaceTexture amplitude rows drive the target; a parameter without a declared Ra ratio cannot.
    public static Fin<RaTarget> From(SurfaceTexture texture, SurfaceParameter source, ScallopFactor factor) =>
        from admittedTexture in Optional(texture).ToFin(Tolerance.Invalid("surface-texture:raw"))
        from admittedSource in Optional(source).ToFin(Tolerance.Invalid("surface-texture:source"))
        from ratio in admittedSource.RaRatio.ToFin(Tolerance.Invalid("surface-texture",
            $"{admittedSource.ToValue()} declares no Ra correspondence"))
        from measured in toSeq(admittedTexture.Requirements).Choose(requirement => requirement switch {
                SurfaceRequirement.Amplitude { Parameter: var parameter, Micrometers: var limit }
                    when parameter == admittedSource => limit.Upper(),
                _ => None,
            }).Head.ToFin(Tolerance.Invalid("surface-texture",
                $"{admittedSource.ToValue()} amplitude upper limit"))
        let micrometers = measured * ratio
        from _ in guard(double.IsFinite(micrometers) && micrometers > 0.0, Tolerance.Range("surface-texture:ra",
            micrometers, "finite and positive")).ToFin()
        select Create(micrometers, factor);
}

[ComplexValueObject]
public sealed partial class ToleranceInterval {
    public double LowerMm { get; }
    public double UpperMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double lowerMm, ref double upperMm) =>
        validationError = double.IsFinite(lowerMm) && double.IsFinite(upperMm) && lowerMm <= upperMm
            ? null : new ValidationError(message: "tolerance-interval");
}

[SmartEnum<string>]
public sealed partial class ProcessDistribution {
    public static readonly ProcessDistribution Normal = new("normal", 1.0);
    public static readonly ProcessDistribution Uniform = new("uniform", Math.Sqrt(3.0));
    public static readonly ProcessDistribution Triangular = new("triangular", 3.0 / Math.Sqrt(6.0));
    public static readonly ProcessDistribution Skewed = new("skewed", 1.5);

    public double QuadratureWeight { get; }
}

[ComplexValueObject]
public sealed partial class ToleranceTerm {
    public string Key { get; }
    public ToleranceInterval Interval { get; }
    public double Sensitivity { get; }
    public ProcessDistribution Distribution { get; }
    public double LowerMm => double.Min(Interval.LowerMm * Sensitivity, Interval.UpperMm * Sensitivity);
    public double UpperMm => double.Max(Interval.LowerMm * Sensitivity, Interval.UpperMm * Sensitivity);
    public double HalfRangeMm => (UpperMm - LowerMm) * 0.5;
    public double StatisticalHalfRangeMm => HalfRangeMm * Distribution.QuadratureWeight;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string key,
        ref ToleranceInterval interval, ref double sensitivity, ref ProcessDistribution distribution) {
        key = key?.Trim() ?? string.Empty;
        validationError = key.Length > 0 && interval is not null && double.IsFinite(sensitivity) && sensitivity != 0.0
            && distribution is not null ? null : new ValidationError(message: "tolerance-term");
    }

    public static Fin<ToleranceTerm> Of(string key, FitClass fit, double nominalMm, double sensitivity,
        ProcessDistribution distribution) =>
        from admitted in Optional(fit).ToFin(Tolerance.Invalid("tolerance-term:fit"))
        from _ in guard(double.IsFinite(nominalMm) && admitted.Grade.Diameter.Contains(nominalMm),
            Tolerance.Range("tolerance-term:nominal", nominalMm, "finite and inside the fit diameter band")).ToFin()
        let sizes = admitted.Sizes(nominalMm)
        from interval in Tolerance.Mint(ToleranceInterval.Validate(sizes.LowerMm - nominalMm,
            sizes.UpperMm - nominalMm, out ToleranceInterval? bounds), bounds, "tolerance-interval")
        from term in Tolerance.Mint(Validate(key, interval, sensitivity, distribution,
            out ToleranceTerm? value), value, "tolerance-term")
        select term;
}

[SmartEnum<string>]
public sealed partial class StackMethod {
    public static readonly StackMethod WorstCase = new("worst-case",
        static terms => terms.Fold(0.0, static (sum, term) => sum + Math.Abs(term.HalfRangeMm)));
    public static readonly StackMethod Rss = new("rss", static terms => Quadrature(terms, 1.0));
    public static readonly StackMethod ModifiedRss = new("modified-rss", static terms => Quadrature(terms, 1.5));
    public static readonly StackMethod Estimated = new("estimated",
        static terms => 0.5 * (WorstCase.Combine(terms) + Quadrature(terms, 1.0)));

    private static double Quadrature(Seq<ToleranceTerm> terms, double inflation) => inflation * Math.Sqrt(
        terms.Fold(0.0, static (sum, term) => sum + (term.StatisticalHalfRangeMm * term.StatisticalHalfRangeMm)));

    [UseDelegateFromConstructor]
    public partial double Combine(Seq<ToleranceTerm> terms);

    // A term's share is its own combined magnitude under the same algebra, so the ranking never forks the law.
    public double Share(ToleranceTerm term, double totalHalfRangeMm) =>
        totalHalfRangeMm > 0.0 ? Combine(Seq(term)) / totalHalfRangeMm : 0.0;
}

[ComplexValueObject]
public sealed partial class ToleranceChain {
    public ContentKey Source { get; }
    public Arr<ToleranceTerm> Terms { get; }
    public double BoundMm { get; }
    public StackMethod Method { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ContentKey source,
        ref Arr<ToleranceTerm> terms, ref double boundMm, ref StackMethod method) =>
        validationError = source is not null && terms.Count > 0 && terms.ForAll(static row => row is not null)
            && terms.Map(static row => row.Key).Distinct().Count == terms.Count && method is not null
            && double.IsFinite(boundMm) && boundMm > 0.0 ? null : new ValidationError(message: "tolerance-chain");

    public ChainReceipt Evaluate() => (Rows: toSeq(Terms), Total: Method.Combine(toSeq(Terms))) switch {
        var chain => new ChainReceipt(Source, Method,
            chain.Rows.Fold(0.0, static (sum, term) => sum + term.LowerMm),
            chain.Rows.Fold(0.0, static (sum, term) => sum + term.UpperMm),
            chain.Total,
            chain.Rows.Map(term => (Key: term.Key, Share: Method.Share(term, chain.Total)))
                .OrderByDescending(static row => row.Share).ThenBy(static row => row.Key).ToArr(),
            BoundMm),
    };
}

public sealed record ChainReceipt(ContentKey Source, StackMethod Method, double WorstLowerMm, double WorstUpperMm,
    double HalfRangeMm, Arr<(string Key, double Share)> Contributions, double BoundMm) {
    public double CentreMm => (WorstLowerMm + WorstUpperMm) * 0.5;
    public bool Conforming => double.Max(Math.Abs(CentreMm - HalfRangeMm), Math.Abs(CentreMm + HalfRangeMm)) <= BoundMm;
    public Option<(string Key, double Share)> Dominant => HalfRangeMm > 0.0
        ? toSeq(Contributions).Head
        : None;
}

[ComplexValueObject]
public sealed partial class SpecQuantity {
    public SpecAxis Axis { get; }
    public double Canonical { get; }
    public string Received { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SpecAxis axis,
        ref double canonical, ref string received) {
        received = received?.Trim() ?? string.Empty;
        validationError = axis is not null && double.IsFinite(canonical) && received.Length > 0
            ? null : new ValidationError(message: "spec-quantity");
    }

    public static Fin<SpecQuantity> Admit(SpecAxis axis, string text) =>
        axis is not null && !string.IsNullOrWhiteSpace(text)
            && Quantity.TryParse(CultureInfo.InvariantCulture, axis.QuantityType, text, out IQuantity? quantity)
            && quantity is not null && quantity.QuantityInfo.Name == axis.QuantityType.Name
                ? Fin.Succ(Create(axis, axis.Canonical(quantity), text))
                : Fin.Fail<SpecQuantity>(Tolerance.Invalid("spec-quantity",
                    $"{axis?.QuantityType.Name} parseable under the invariant culture"));
}

public interface IToleranceEncoder {
    Fin<ReadOnlyMemory<byte>> Encode(Tolerance value);
}

// --- [OWNER_FOLD] ---------------------------------------------------------------------------------------------------------------------------------
[Union]
public abstract partial record Tolerance {
    private Tolerance() { }
    public sealed record Geometric(FeatureControl Value) : Tolerance;
    public sealed record Fit(FitLimits Value) : Tolerance;
    public sealed record Texture(SurfaceTexture Value) : Tolerance;
    public sealed record General(GeneralTolerance Value) : Tolerance;
    public sealed record Chain(ToleranceChain Value) : Tolerance;

    internal static readonly Op SpecOp = Op.Of(name: "fabrication:tolerance");

    public ContentKey Source() => Switch(
        geometric: static row => row.Value.Source, fit: static row => row.Value.Source,
        texture: static row => row.Value.Source, general: static row => row.Value.Source,
        chain: static row => row.Value.Source);

    public QifKind Qif() => Switch(
        geometric: static _ => QifKind.FeatureControlFrame, fit: static _ => QifKind.DimensionalTolerance,
        texture: static _ => QifKind.SurfaceTexture, general: static _ => QifKind.GeneralTolerance,
        chain: static _ => QifKind.DimensionalTolerance);

    public static Fin<ToleranceReceipt> Apply(ToleranceRequest request) =>
        Optional(request).ToFin(Invalid("request")).Bind(static admitted => admitted.Switch(
            feature: static demand => Admit(demand),
            fit: static demand => Admit(demand),
            texture: static demand => Admit(demand),
            general: static demand => Admit(demand),
            chain: static demand => Admit(demand),
            quantity: static demand => SpecQuantity.Admit(demand.Axis, demand.Text)
                .Map<ToleranceReceipt>(static value => new ToleranceReceipt.Quantity(value)),
            effective: static demand => Effective(demand.Control, demand.DepartureMm),
            scallop: static demand => Scallop(demand.Target, demand.Cutter),
            allowance: static demand => Allowance(demand.Grade),
            project: static demand => Project(demand)));

    internal static Error Invalid(string axis, string requirement = "admitted tolerance shape") =>
        new Fault.InvalidValue(Label: axis, Requirement: requirement, Key: Some(SpecOp));
    internal static Error Range(string axis, double scalar, string requirement) => new Fault.OutOfRange(
        Label: axis, Scalar: scalar, Requirement: requirement, Key: Some(SpecOp));

    // One bridge lifts every generated factory outcome; a refusal keeps the generator's own message as the requirement.
    internal static Fin<T> Mint<T>(ValidationError? refusal, T? value, string axis) where T : class =>
        refusal is { } error ? Fin.Fail<T>(Invalid(axis, error.Message))
            : Optional(value).ToFin(Invalid(axis, "generated factory yielded a value"));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Feature raw) =>
        from control in FeatureControl.Admit(raw)
        from _ in control.AchievableMm.Filter(achievable => achievable > control.Zone.Width.ToValue()).Match(
            Some: achievable => Fin.Fail<Unit>(FabricationFault.ToleranceUnsatisfiable(
                new FaultSubject.Specification(control.Source), achievable)),
            None: static () => Fin.Succ(unit))
        select new ToleranceReceipt.Frame(new Geometric(control), FeatureFrameReceipt.Create(control));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Fit demand) =>
        from admitted in Optional(demand).ToFin(Invalid("fit:raw"))
        from _1 in guard(admitted.Source is not null && admitted.Hole is not null && admitted.Shaft is not null,
            Invalid("fit:shape")).ToFin()
        from _2 in guard(admitted.Hole.Member == FitMember.Hole && admitted.Shaft.Member == FitMember.Shaft
            && admitted.Hole.Grade.Diameter == admitted.Shaft.Grade.Diameter, Invalid("fit:pair")).ToFin()
        let maximum = (admitted.Hole.Limits.UpperUm - admitted.Shaft.Limits.LowerUm) / 1000.0
        let minimum = (admitted.Hole.Limits.LowerUm - admitted.Shaft.Limits.UpperUm) / 1000.0
        from _3 in guard(double.IsFinite(maximum) && double.IsFinite(minimum) && maximum >= minimum,
            Invalid("fit:limits")).ToFin()
        let character = minimum >= 0.0 ? FitCharacter.Clearance
            : maximum <= 0.0 ? FitCharacter.Interference : FitCharacter.Transition
        from limits in Mint(FitLimits.Validate(admitted.Source, admitted.NominalMm, admitted.Hole, admitted.Shaft,
            character, out FitLimits? value), value, "fit-limits")
        select (ToleranceReceipt)new ToleranceReceipt.Fitted(new Fit(limits));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Texture demand) =>
        from admitted in Optional(demand).ToFin(Invalid("surface-texture:raw"))
        from texture in Mint(SurfaceTexture.Validate(admitted.Source, admitted.Requirements, admitted.Lay,
            admitted.Mark, admitted.Band, admitted.MachiningAllowanceMm, admitted.Treatment,
            out SurfaceTexture? value), value, "surface-texture")
        select (ToleranceReceipt)new ToleranceReceipt.Textured(new Texture(texture));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.General demand) =>
        from admitted in Optional(demand).ToFin(Invalid("general-tolerance:raw"))
        from standard in Optional(admitted.Standard).ToFin(Invalid("general-tolerance:standard"))
        from limit in standard.Resolve(admitted.Class, admitted.Kind, admitted.NominalMm)
        from value in Mint(GeneralTolerance.Validate(admitted.Source, admitted.Class, admitted.Kind,
            admitted.NominalMm, limit, out GeneralTolerance? tolerance), tolerance, "general-tolerance")
        select (ToleranceReceipt)new ToleranceReceipt.Generalized(new General(value));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Chain demand) =>
        from admitted in Optional(demand).ToFin(Invalid("tolerance-chain:raw"))
        from chain in Mint(ToleranceChain.Validate(admitted.Source, admitted.Terms, admitted.BoundMm,
            admitted.Method, out ToleranceChain? value), value, "tolerance-chain")
        select (ToleranceReceipt)new ToleranceReceipt.Stacked(new Chain(chain), chain.Evaluate());

    private static Fin<ToleranceReceipt> Effective(FeatureControl control, double departureMm) =>
        from admitted in Optional(control).ToFin(Invalid("effective:control"))
        from _1 in guard(double.IsFinite(departureMm) && departureMm >= 0.0,
            Range("effective:departure", departureMm, "finite and nonnegative")).ToFin()
        from _2 in guard(admitted.Material != MaterialCondition.Regardless || departureMm == 0.0,
            Range("effective:departure", departureMm, "zero under a regardless-of-feature-size control")).ToFin()
        let width = admitted.Zone.Width.ToValue()
        let boundaries = admitted.Size.Bind(size => admitted.Material.Boundaries(size, width))
        select (ToleranceReceipt)new ToleranceReceipt.Effective(admitted,
            admitted.Material.Effective(width, departureMm), departureMm,
            boundaries.Map(static row => row.VirtualMm), boundaries.Map(static row => row.ResultantMm));

    private static Fin<ToleranceReceipt> Allowance(ItGrade grade) =>
        Optional(grade).ToFin(Invalid("allowance:grade")).Map<ToleranceReceipt>(static admitted =>
            new ToleranceReceipt.Allowance(admitted.ToleranceMillimeters * admitted.AllowanceFactor.ToValue()));

    private static Fin<ToleranceReceipt> Project(ToleranceRequest.Project demand) =>
        from _ in guard(demand.Value is not null && demand.Encoder is not null, Invalid("project:shape")).ToFin()
        from bytes in Try.lift<Fin<ReadOnlyMemory<byte>>>(f: () => demand.Encoder.Encode(demand.Value)).Run()
            .MapFail(error => Invalid("project:encode", error.Message)).Bind(static result => result)
        select (ToleranceReceipt)new ToleranceReceipt.Projected(demand.Value, bytes);

    // Only a rotationally swept cutter leaves a cusp, and its radius is the family's own, never the shank's.
    private static Fin<ToleranceReceipt> Scallop(RaTarget target, CutterForm cutter) =>
        from admittedTarget in Optional(target).ToFin(Invalid("scallop:target"))
        from admittedCutter in Optional(cutter).ToFin(Invalid("scallop:cutter"))
        from radius in admittedCutter.Family.Switch(
            flat: static () => Fin.Fail<double>(Invalid("scallop:family", "a cusp-forming cutter family")),
            ball: () => Fin.Succ(admittedCutter.Diameter * 0.5),
            bull: () => Fin.Succ(admittedCutter.CornerRadius),
            taper: () => Fin.Succ(admittedCutter.CornerRadius),
            drill: static () => Fin.Fail<double>(Invalid("scallop:family", "a cusp-forming cutter family")),
            chamfer: static () => Fin.Fail<double>(Invalid("scallop:family", "a cusp-forming cutter family")),
            threadMill: static () => Fin.Fail<double>(Invalid("scallop:family", "a cusp-forming cutter family")))
        let height = admittedTarget.ScallopHeightMm
        let radicand = (2.0 * radius * height) - (height * height)
        from _1 in guard(double.IsFinite(radius) && radius > 0.0,
            Range("scallop:radius", radius, "finite and positive")).ToFin()
        from _2 in guard(double.IsFinite(radicand) && radicand > 0.0,
            Range("scallop:radicand", radicand, "finite and positive")).ToFin()
        select (ToleranceReceipt)new ToleranceReceipt.Scallop(2.0 * Math.Sqrt(radicand));
}

[Union]
public abstract partial record ToleranceRequest {
    private ToleranceRequest() { }
    public sealed record Feature(CharacteristicId Id, ContentKey Source, FeatureCharacteristic Characteristic, FeatureScope Scope,
        ToleranceZone Zone, DatumSystem Datums, MaterialCondition Material, FrameExtension Extension,
        Option<FeatureSize> Size, Option<double> AchievableMm) : ToleranceRequest;
    public sealed record Fit(ContentKey Source, double NominalMm, FitClass Hole, FitClass Shaft) : ToleranceRequest;
    public sealed record Texture(ContentKey Source, Arr<SurfaceRequirement> Requirements, SurfaceLay Lay,
        ProcessMark Mark, Option<TransmissionBand> Band, Option<double> MachiningAllowanceMm,
        Option<string> Treatment) : ToleranceRequest;
    public sealed record General(ContentKey Source, GeneralToleranceClass Class, GeneralToleranceKind Kind,
        double NominalMm, GeneralStandard Standard) : ToleranceRequest;
    public sealed record Chain(ContentKey Source, Arr<ToleranceTerm> Terms, double BoundMm,
        StackMethod Method) : ToleranceRequest;
    public sealed record Quantity(SpecAxis Axis, string Text) : ToleranceRequest;
    public sealed record Effective(FeatureControl Control, double DepartureMm) : ToleranceRequest;
    public sealed record Scallop(RaTarget Target, CutterForm Cutter) : ToleranceRequest;
    public sealed record Allowance(ItGrade Grade) : ToleranceRequest;
    public sealed record Project(Tolerance Value, IToleranceEncoder Encoder) : ToleranceRequest;
}

[ValueObject<UInt128>]
public readonly partial struct CharacteristicId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref UInt128 value) =>
        validationError = value == UInt128.Zero ? new ValidationError("tolerance:characteristic-id") : null;
}

[Union]
public abstract partial record ToleranceReceipt {
    private ToleranceReceipt() { }
    public sealed record Frame(Tolerance.Geometric Value, FeatureFrameReceipt Receipt) : ToleranceReceipt;
    public sealed record Fitted(Tolerance.Fit Value) : ToleranceReceipt;
    public sealed record Textured(Tolerance.Texture Value) : ToleranceReceipt;
    public sealed record Generalized(Tolerance.General Value) : ToleranceReceipt;
    public sealed record Stacked(Tolerance.Chain Value, ChainReceipt Receipt) : ToleranceReceipt;
    public sealed record Quantity(SpecQuantity Value) : ToleranceReceipt;
    public sealed record Effective(FeatureControl Control, double WidthMm, double DepartureMm,
        Option<double> VirtualConditionMm, Option<double> ResultantConditionMm) : ToleranceReceipt;
    public sealed record Scallop(double StepMm) : ToleranceReceipt;
    public sealed record Allowance(double Millimeters) : ToleranceReceipt;
    public sealed record Projected(Tolerance Value, ReadOnlyMemory<byte> Bytes) : ToleranceReceipt;

    // Every receipt that carries a specification exposes it once, so a consumer never re-matches the case set.
    public Option<Tolerance> Specification() => Switch(
        frame: static row => Some<Tolerance>(row.Value), fitted: static row => Some<Tolerance>(row.Value),
        textured: static row => Some<Tolerance>(row.Value), generalized: static row => Some<Tolerance>(row.Value),
        stacked: static row => Some<Tolerance>(row.Value), quantity: static _ => None,
        effective: static _ => None, scallop: static _ => None, allowance: static _ => None,
        projected: static row => Some(row.Value));

    public Option<bool> Conforming() => Switch(
        frame: static row => row.Receipt.AchievableMm.Map(achievable => achievable <= row.Receipt.WidthMm),
        fitted: static _ => None, textured: static _ => None, generalized: static _ => None,
        stacked: static row => Some(row.Receipt.Conforming), quantity: static _ => None,
        effective: static _ => None, scallop: static _ => None, allowance: static _ => None,
        projected: static _ => None);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
