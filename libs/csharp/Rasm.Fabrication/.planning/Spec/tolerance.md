# [RASM_FABRICATION_TOLERANCE]

`Tolerance` owns every production-specification value from raw quantity admission through geometric control, ISO 286 fit, general tolerance, surface texture, method-parameterized stackup, typed derivation, and parameterized wire projection. `FeatureControl`, `FitClass`, `SurfaceTexture`, and `ToleranceChain` admit once, while `Tolerance.Apply` dispatches every ingress, operation, and egress modality on payload-complete `ToleranceRequest` cases.

`Tolerance` preserves the cross-runtime wire name consumed by the artifacts plane without exporting its C# shape, keys structural refusals through `SpecOp`, carries `ContentKey` into `ToleranceUnsatisfiable`, consumes measured cutter geometry through admitted `CutterForm`, and accepts capability evidence only as input-carried achievable width. `Rasm.Solving` owns the name `FitReceipt`, so the ISO 286 pairing result is `FitLimits`.

## [01]-[INDEX]

- [02]-[GEOMETRIC_VOCABULARY]: `FeatureCharacteristic` and the symbol-bearing scope, zone, modifier, and material rows carrying ISO 1101 legality as row behavior.
- [03]-[FEATURE_CONTROL]: payload-shaped `ToleranceZone`, the datum system and frame extension, `FeatureControl.Admit`, and the layout-free `FeatureFrameReceipt.Annotation` stream.
- [04]-[FIT_ALGEBRA]: generated `ItGradeName`, closed-form `FitLetter` deviations over `DiameterBand`, the `FitException` carve, and the validated fit and general-tolerance seed laws.
- [05]-[SURFACE_TEXTURE]: the ISO 21920 parameter roster, its measure-owned units and bands, the one-shape requirement, and the `RaTarget` scallop projection.
- [06]-[STACK_CHAIN]: `ToleranceTerm`, the `StackMethod` analytic algebra, and the `ChainReceipt` every stackup consumer reads.
- [07]-[OWNER_FOLD]: `SpecAxis` quantity admission and the `Tolerance.Apply` fold over `ToleranceRequest` into `ToleranceReceipt`.

## [02]-[GEOMETRIC_VOCABULARY]

- Owner: `FeatureCharacteristic` owns the ISO 1101 control roster and the legality each control admits; `FeatureScope`, `ToleranceZoneKind`, `MaterialCondition`, and `ZoneModifier` own the axes a frame is graded against.
- Cases: `FeatureScope` distinguishes surface, axis, median-line, median-plane, and center-point controls before material-condition policy resolves; `DatumUse` closes whether a control forbids, admits, or demands datums.
- Auto: `ZoneModifier.Admits`, `FeatureCharacteristic.AdmitsScope`, and `MaterialCondition.Boundaries` carry the ISO 1101 legality and virtual-condition law as ROW BEHAVIOR, so the admitting owner never re-derives it; `FeatureCharacteristic.EffectiveClass` grades a profile control across all three ISO 1101 steps, so a singly-referenced profile claims orientation rather than the location its drawing never constrained.
- Growth: a geometric characteristic, zone kind, or modifier is one row carrying its own legality columns.
- Boundary: a row states legality alone — the frame that composes them, its datum system, and its receipt live at `[03]`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Threading;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using MathNet.Numerics.Distributions;
using Rasm.Domain;
using Rasm.Element.Projection;
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

    // ISO 1101 grades a profile control in THREE steps, not two: no datum controls FORM alone, a single datum adds
    // ORIENTATION, and a full datum system adds LOCATION. Collapsing the last two makes a singly-referenced profile
    // claim a located zone the drawing never constrained.
    public FeatureClass EffectiveClass(int datumCount) => !ProfileContextual
        ? Class
        : datumCount switch {
            0 => FeatureClass.Form,
            1 => FeatureClass.Orientation,
            _ => FeatureClass.Location,
        };
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
```

## [03]-[FEATURE_CONTROL]

- Owner: `FeatureControl` owns the admitted feature-control frame; `DatumSystem` owns precedence-ordered datum references; `FrameExtension` owns basics, targets, and the composite lower segment; `FeatureFrameReceipt` owns the settled projection.
- Cases: `ToleranceZone` carries its kind beside the ONE payload that kind admits — plain, offset, or projected — so four cases carrying width and modifiers alone collapse to one and the kind stays the discriminant every legality row already reads.
- Law: ISO 1101 legality is EIGHT independent questions. `FeatureControl.Legal` is the one predicate both the accumulating admission and the generated hook read, so a direct `Create` can never seat a frame the admission would refuse and the two can never state different law; `FeatureControl.Admit` accumulates through `AdmissionSlots`, so a caller repairing a datum count learns in the same verdict that its zone kind and material condition are also inadmissible.
- Law: `FeatureFrameReceipt.Annotation` is the ONE annotation surface and it is a LAYOUT-FREE row stream — compartment, symbol, ordinal — never a concatenated glyph run a drawing consumer would have to re-parse into the structure this owner already holds. Placement, size, and font stay the drawing plane's, and a second joined-glyph projection on a datum owner is the deleted form.
- Growth: a frame axis is one column on `FrameExtension`; a compartment is one `FrameCompartment` row.
- Boundary: an achievable width enters as input-carried capability evidence and never as a reach into `Spec/capability`.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<double>]
[ValidationError<FabricationFault>]
public readonly partial struct ZoneWidth {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0.0 ? null : Tolerance.Refusal("tolerance-zone-width");
}

// A zone case exists for its PAYLOAD, never for its name: four kinds carrying width and modifiers alone were four
// spellings of one shape, and two carrying an offset differed only in what bounds it. The kind stays a carried
// discriminant every legality row already reads, so the vocabulary loses nothing and the payloads stop repeating.
[Union]
public abstract partial record ToleranceZone(ToleranceZoneKind Kind, ZoneWidth Width, Set<ZoneModifier> Modifiers) {
    public sealed record Simple(ToleranceZoneKind Kind, ZoneWidth Width, Set<ZoneModifier> Modifiers)
        : ToleranceZone(Kind, Width, Modifiers);
    public sealed record Offset(ToleranceZoneKind Kind, ZoneWidth Width, double OffsetMm, Set<ZoneModifier> Modifiers)
        : ToleranceZone(Kind, Width, Modifiers);
    public sealed record Projected(ZoneWidth Width, double HeightMm, Set<ZoneModifier> Modifiers)
        : ToleranceZone(ToleranceZoneKind.Projected, Width, Modifiers);

    // The kinds each payload admits: a simple zone carries no second dimension, a unilateral offset is signed and
    // unbounded while an unequally-disposed one is a fraction of its own width.
    private static readonly Set<ToleranceZoneKind> SimpleKinds = Set(
        ToleranceZoneKind.Bilateral, ToleranceZoneKind.Diameter,
        ToleranceZoneKind.Spherical, ToleranceZoneKind.Profile);

    public (Option<double> ProjectedHeightMm, Option<double> UnequalOffsetMm) Dimensions() => Switch(
        simple: static _ => (Option<double>.None, Option<double>.None),
        offset: static zone => (Option<double>.None, Some(zone.OffsetMm)),
        projected: static zone => (Some(zone.HeightMm), Option<double>.None));

    public bool Valid() => double.IsFinite(Width.ToValue()) && Width.ToValue() > 0.0 && Switch(
        simple: static zone => SimpleKinds.Contains(zone.Kind),
        offset: static zone => double.IsFinite(zone.OffsetMm)
            && (zone.Kind == ToleranceZoneKind.Unilateral
                || (zone.Kind == ToleranceZoneKind.UnequallyDisposed
                    && zone.OffsetMm >= 0.0 && zone.OffsetMm <= zone.Width.ToValue())),
        projected: static zone => double.IsFinite(zone.HeightMm) && zone.HeightMm > 0.0);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DatumReference {
    public string Label { get; }
    public DatumPrecedence Precedence { get; }
    public MaterialCondition Material { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref string label,
        ref DatumPrecedence precedence, ref MaterialCondition material) {
        label = label?.Trim().ToUpperInvariant() ?? string.Empty;
        validationError = label.Length > 0 ? null : Tolerance.Refusal("datum-reference");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DatumPoint {
    public double XMm { get; }
    public double YMm { get; }
    public double ZMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double xMm, ref double yMm, ref double zMm) =>
        validationError = double.IsFinite(xMm) && double.IsFinite(yMm) && double.IsFinite(zMm)
            ? null : Tolerance.Refusal("datum-point");
}

[Union]
public abstract partial record DatumTarget(string Label, DatumPoint At) {
    public sealed record Point(string Label, DatumPoint At) : DatumTarget(Label, At);
    public sealed record Line(string Label, DatumPoint At, double LengthMm) : DatumTarget(Label, At);
    public sealed record Area(string Label, DatumPoint At, double LengthMm, double WidthMm) : DatumTarget(Label, At);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DatumSystem {
    public Arr<DatumReference> References { get; }
    public QifKind Qif => QifKind.DatumSystem;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref Arr<DatumReference> references) {
        // Precedence ORDERS the system, so admission seats the references in it rather than trusting a caller's order.
        references = toSeq(references.OrderBy(static row => row.Precedence.Order)).ToArr();
        validationError = references.Count <= 3
            && references.Map(static row => row.Label).Distinct().Count == references.Count
            && references.Map(static row => row.Precedence).Distinct().Count == references.Count
            && references.ForAll(row => row.Precedence.Order <= references.Count)
            ? null : Tolerance.Refusal("datum-system");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class BasicDimension {
    public string Label { get; }
    public double NominalMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref string label, ref double nominalMm) {
        label = label?.Trim() ?? string.Empty;
        validationError = label.Length > 0 && double.IsFinite(nominalMm) ? null : Tolerance.Refusal("basic-dimension");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class CompositeSegment {
    public ZoneWidth Width { get; }
    public Set<ZoneModifier> Modifiers { get; }
    public DatumSystem Datums { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref ZoneWidth width,
        ref Set<ZoneModifier> modifiers, ref DatumSystem datums) =>
        validationError = double.IsFinite(width.ToValue()) && width.ToValue() > 0.0
            && datums.References.Count > 0 ? null : Tolerance.Refusal("composite-segment");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class FrameExtension {
    public Arr<BasicDimension> Basics { get; }
    public Arr<DatumTarget> Targets { get; }
    public Option<CompositeSegment> Composite { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref Arr<BasicDimension> basics,
        ref Arr<DatumTarget> targets, ref Option<CompositeSegment> composite) =>
        validationError = basics.Map(static row => row.Label).Distinct().Count == basics.Count
            && targets.Map(static row => row.Label).Distinct().Count == targets.Count
            && targets.ForAll(ValidTarget)
                ? null : Tolerance.Refusal("frame-extension");

    private static bool ValidTarget(DatumTarget target) => target.Switch(
        point: static row => !string.IsNullOrWhiteSpace(row.Label),
        line: static row => !string.IsNullOrWhiteSpace(row.Label)
            && double.IsFinite(row.LengthMm) && row.LengthMm > 0.0,
        area: static row => !string.IsNullOrWhiteSpace(row.Label)
            && double.IsFinite(row.LengthMm) && row.LengthMm > 0.0
            && double.IsFinite(row.WidthMm) && row.WidthMm > 0.0);

    // A target label carries its datum letter as a prefix, and a composite lower segment refines the upper datums.
    public bool Anchored(DatumSystem datums) =>
        Targets.ForAll(target => datums.References.Exists(row =>
            target.Label.StartsWith(row.Label, StringComparison.Ordinal)))
        && Composite.ForAll(segment => segment.Datums.References.ForAll(row =>
            datums.References.Exists(upper => upper.Label == row.Label)));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class FeatureSize {
    public FeatureGeometry Geometry { get; }
    public double LowerMm { get; }
    public double UpperMm { get; }
    public double MaximumMaterialMm => Geometry.Material(LowerMm, UpperMm).MaximumMm;
    public double LeastMaterialMm => Geometry.Material(LowerMm, UpperMm).LeastMm;
    public double RangeMm => UpperMm - LowerMm;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref FeatureGeometry geometry,
        ref double lowerMm, ref double upperMm) =>
        validationError = double.IsFinite(lowerMm) && lowerMm > 0.0
            && double.IsFinite(upperMm) && upperMm >= lowerMm ? null : Tolerance.Refusal("feature-size");

    public bool Contains(double actualMm) => actualMm >= LowerMm && actualMm <= UpperMm;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
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

    // ONE legality predicate, two readers: the accumulating admission reports WHICH clauses a frame violated and
    // the generated hook proves the same conjunction, so a direct `Create` can never seat a frame the admission
    // would have refused and the two can never state different law.
    internal static bool Legal(
        FeatureCharacteristic characteristic,
        FeatureScope scope,
        ToleranceZone zone,
        DatumSystem datums,
        MaterialCondition material,
        FrameExtension extension,
        Option<FeatureSize> size,
        Option<double> achievableMm) =>
        zone.Valid()
        && characteristic.Datums.Admits(datums.References.Count)
        && characteristic.AdmitsScope(scope)
        && characteristic.AdmitsZone(zone.Kind)
        && zone.Modifiers.ForAll(modifier => modifier.Admits(characteristic, scope))
        && (material == MaterialCondition.Regardless || (characteristic.AdmitsMaterial(scope) && size.IsSome))
        && extension.Anchored(datums)
        && achievableMm.ForAll(static value => double.IsFinite(value) && value > 0.0);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref CharacteristicId id, ref ContentKey source,
        ref FeatureCharacteristic characteristic, ref FeatureScope scope, ref ToleranceZone zone, ref DatumSystem datums,
        ref MaterialCondition material, ref FrameExtension extension, ref Option<FeatureSize> size,
        ref Option<double> achievableMm) {
        if (!Legal(characteristic, scope, zone, datums, material, extension, size, achievableMm))
            validationError = Tolerance.Refusal("feature-control");
    }

    // ISO 1101 legality is EIGHT independent questions, so a refusal names every clause the frame violated rather
    // than the first one a boolean ladder happened to reach: a caller repairing a datum count learns in the same
    // verdict that its zone kind and its material condition are also inadmissible.
    public static Fin<FeatureControl> Admit(ToleranceRequest.Feature raw) =>
        from _clauses in (
            Gate(raw.Zone.Valid(), "zone-payload"),
            Gate(raw.Characteristic.Datums.Admits(raw.Datums.References.Count), "datum-count"),
            Gate(raw.Characteristic.AdmitsScope(raw.Scope), "scope"),
            Gate(raw.Characteristic.AdmitsZone(raw.Zone.Kind), "zone-kind"),
            Gate(raw.Zone.Modifiers.ForAll(modifier => modifier.Admits(raw.Characteristic, raw.Scope)), "modifier"),
            Gate(raw.Material == MaterialCondition.Regardless
                || (raw.Characteristic.AdmitsMaterial(raw.Scope) && raw.Size.IsSome), "material-condition"),
            Gate(raw.Extension.Anchored(raw.Datums), "extension-anchor"),
            Gate(raw.AchievableMm.ForAll(static value => double.IsFinite(value) && value > 0.0), "achievable"))
            .Apply(static (_, _, _, _, _, _, _, _) => unit)
            .As()
            .ToFin()
        from admitted in Validate(raw.Id, raw.Source, raw.Characteristic, raw.Scope, raw.Zone, raw.Datums, raw.Material,
            raw.Extension, raw.Size, raw.AchievableMm, out FeatureControl value).Admitted(value)
        select admitted;

    private static K<Validation<Error>, Unit> Gate(bool holds, string locus) =>
        AdmissionSlots.Gate(holds, Tolerance.Refusal($"feature-control:{locus}"));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class FeatureFrameReceipt {
    public CharacteristicId Id => Control.Id;
    public FeatureControl Control { get; }
    public Tolerance.Geometric Specification => new(Control);
    public QifKind Qif => Specification.Qif();
    public FeatureCharacteristic Characteristic => Control.Characteristic;
    public FeatureScope Scope => Control.Scope;
    public ToleranceZoneKind Kind => Control.Zone.Kind;
    public double WidthMm => Control.Zone.Width.ToValue();
    public Arr<ZoneModifier> Modifiers => toSeq(Control.Zone.Modifiers
        .OrderBy(static modifier => modifier.ToValue())).ToArr();
    public Arr<DatumReference> Datums => Control.Datums.References;
    public MaterialCondition Material => Control.Material;
    public Option<FeatureSize> Size => Control.Size;
    public Option<double> ProjectedHeightMm => Control.Zone.Dimensions().ProjectedHeightMm;
    public Option<double> UnequalOffsetMm => Control.Zone.Dimensions().UnequalOffsetMm;
    public FrameExtension Extension => Control.Extension;
    public Option<double> AchievableMm => Control.AchievableMm;
    // The frame as LAYOUT-FREE ROWS, never a concatenated glyph run: a drafting consumer places compartments,
    // stacks a composite segment, and sizes a datum box from the ROW STREAM, while a pre-joined string forces it to
    // re-parse the very structure this owner already holds. Every row carries its compartment, its symbol text,
    // and its ordinal, so the drawing plane composes a feature-control frame without re-opening the specification.
    public Seq<FrameSymbolRow> Annotation =>
        Seq(new FrameSymbolRow(FrameCompartment.Characteristic, Control.Characteristic.Symbol, 0))
        + Seq(new FrameSymbolRow(
            FrameCompartment.Zone,
            string.Concat(Control.Zone.Kind.Prefix, WidthMm.ToString("0.###", CultureInfo.InvariantCulture)),
            0))
        + (Control.Material == MaterialCondition.Regardless
            ? Seq<FrameSymbolRow>()
            : Seq(new FrameSymbolRow(FrameCompartment.Material, Control.Material.Symbol, 0)))
        + Modifiers.Map(static (modifier, index) => new FrameSymbolRow(FrameCompartment.Modifier, modifier.Symbol, index)).ToSeq()
        + Datums.Map(static (datum, index) => new FrameSymbolRow(FrameCompartment.Datum, datum.Label, index)).ToSeq();

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref FeatureControl control) =>
        validationError = control is null ? Tolerance.Refusal("feature-frame-receipt") : null;
}

// The compartment a symbol row occupies in an ISO 1101 feature-control frame. The vocabulary is the FRAME's own
// structure, so a drawing consumer partitions by row rather than by position in a joined string.
[SmartEnum<string>]
public sealed partial class FrameCompartment {
    public static readonly FrameCompartment Characteristic = new("characteristic");
    public static readonly FrameCompartment Zone = new("zone");
    public static readonly FrameCompartment Material = new("material");
    public static readonly FrameCompartment Modifier = new("modifier");
    public static readonly FrameCompartment Datum = new("datum");
}

// One symbol row: what compartment it belongs to, the symbol text itself, and its ordinal inside that compartment.
// No placement, no size, no font — layout is the drawing plane's, and this row carries only the specification.
public readonly record struct FrameSymbolRow(FrameCompartment Compartment, string Symbol, int Ordinal);

[ValueObject<UInt128>]
[ValidationError<FabricationFault>]
public readonly partial struct CharacteristicId {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref UInt128 value) =>
        validationError = value == UInt128.Zero ? Tolerance.Refusal("tolerance:characteristic-id") : null;
}
```

## [04]-[FIT_ALGEBRA]

- Owner: `FitLetter` owns the fundamental-deviation closed form per letter, `ItGradeName` the standard tolerance grade formulas, `DiameterBand` the reference diameter every one of them evaluates at, and `FitStandard` the generative resolution both feed; `GeneralStandard` owns the ISO 2768 seed roster.
- Law: ISO 286 is ALGEBRA here, not a transcribed grid. `ItGradeName` generates `IT01` through `IT18` over the standard tolerance unit, `DiameterBand.ReferenceMm` derives the geometric mean, and the hole derives as the shaft's mirror under the general rule with its correction — so a revision widening the band roster costs one row and a revision changing a formula costs one delegate.
- Law: `FitException` holds ONLY what the standard publishes outside its own formulas — shaft j and hole J, the p step, k outside grades 4 through 7 — so a row duplicating what `FitLetter` derives is the deleted form, because the two would then disagree silently, and a TABULATED letter with no exception refuses rather than returning the zero its unused delegate would hand back.
- Auto: `FitCharacter.Of` is the ONE pairing law, so the admitting fold and the receipt's own proof cannot disagree on whether a pair clears, transitions, or interferes.
- Growth: a fit letter, IT grade, diameter band, or general-tolerance class is one row; a tabular standard revision is seed data under the existing admission proof.
- Boundary: deviations are published in micrometres and sizes read in millimetres, so the conversion rides the quantity owner at the one derivation site rather than a bare divisor per call.

```csharp signature
// --- [VOCABULARIES] -------------------------------------------------------------------------------------------------------------------------------
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

    // The ONE pairing law. A never-negative minimum clears, a never-positive maximum interferes, and anything
    // spanning zero transitions — stated once so the admitting fold and the receipt's own proof cannot disagree.
    public static FitCharacter Of(double minimumMm, double maximumMm) =>
        minimumMm >= 0.0 ? Clearance : maximumMm <= 0.0 ? Interference : Transition;
}

[SmartEnum<string>]
// ISO 286-1 fundamental deviation as the CLOSED FORM the standard publishes, not a transcribed grid. Each row
// carries the bound its letter governs and the micrometre formula over the diameter band's geometric mean, so a
// band the standard adds is one `DiameterBand` row needing no deviation transcription at all. The genuinely
// irregular rows — shaft j and hole J, the p step, and k outside grades 4 through 7 — ride `FitException`, which
// is what a table on this page is now FOR.
public sealed partial class FitLetter {
    public static readonly FitLetter A = Upper("a", static (d, _) => d <= 120.0 ? -(265.0 + (1.3 * d)) : -3.5 * d);
    public static readonly FitLetter B = Upper("b", static (d, _) => d <= 160.0 ? -(140.0 + (0.85 * d)) : -1.8 * d);
    public static readonly FitLetter C = Upper("c", static (d, _) => d <= 40.0 ? -52.0 * Math.Pow(d, 0.2) : -(95.0 + (0.8 * d)));
    public static readonly FitLetter Cd = Upper("cd", static (d, series) => -Blend("c", "d", d, series));
    public static readonly FitLetter D = Upper("d", static (d, _) => -16.0 * Math.Pow(d, 0.44));
    public static readonly FitLetter E = Upper("e", static (d, _) => -11.0 * Math.Pow(d, 0.41));
    public static readonly FitLetter Ef = Upper("ef", static (d, series) => -Blend("e", "f", d, series));
    public static readonly FitLetter F = Upper("f", static (d, _) => -5.5 * Math.Pow(d, 0.41));
    public static readonly FitLetter Fg = Upper("fg", static (d, series) => -Blend("f", "g", d, series));
    public static readonly FitLetter G = Upper("g", static (d, _) => -2.5 * Math.Pow(d, 0.34));
    public static readonly FitLetter H = Upper("h", static (_, _) => 0.0);
    // Js is symmetric about the nominal, so the grade alone sets both deviations and the fundamental term is zero.
    public static readonly FitLetter Js = new("js", FitBound.Symmetric, tabulates: false, static (_, _) => 0.0);
    public static readonly FitLetter J = new("j", FitBound.Upper, tabulates: true, static (_, _) => 0.0);
    public static readonly FitLetter K = Lower("k", static (d, _) => 0.6 * Math.Cbrt(d));
    public static readonly FitLetter M = Lower("m", static (_, series) => series.At(7) - series.At(6));
    public static readonly FitLetter N = Lower("n", static (d, _) => 5.0 * Math.Pow(d, 0.34));
    public static readonly FitLetter P = Lower("p", static (_, series) => series.At(7));
    public static readonly FitLetter R = Lower("r", static (d, series) => Blend("p", "s", d, series));
    public static readonly FitLetter S = Lower("s", static (d, series) =>
        d <= 50.0 ? series.At(8) + 1.0 : series.At(7) + (0.4 * d));
    public static readonly FitLetter T = Lower("t", static (d, series) => series.At(7) + (0.63 * d));
    public static readonly FitLetter U = Lower("u", static (d, series) => series.At(7) + d);
    public static readonly FitLetter V = Lower("v", static (d, series) => series.At(7) + (1.25 * d));
    public static readonly FitLetter X = Lower("x", static (d, series) => series.At(7) + (1.6 * d));
    public static readonly FitLetter Y = Lower("y", static (d, series) => series.At(7) + (2.0 * d));
    public static readonly FitLetter Z = Lower("z", static (d, series) => series.At(7) + (2.5 * d));
    public static readonly FitLetter Za = Lower("za", static (d, series) => series.At(8) + (3.15 * d));
    public static readonly FitLetter Zb = Lower("zb", static (d, series) => series.At(9) + (4.0 * d));
    public static readonly FitLetter Zc = Lower("zc", static (d, series) => series.At(10) + (5.0 * d));

    public FitBound Bound { get; }

    // A letter the standard tabulates rather than derives resolves ONLY through `FitException`; the formula path
    // refuses it rather than returning the zero its unused delegate would hand back.
    public bool Tabulates { get; }

    // Shaft deviation in micrometres. A hole mirrors it under the general rule, so one column serves both members.
    [UseDelegateFromConstructor]
    public partial double ShaftMicrometers(double geometricMeanMm, ItSeries series);

    private static FitLetter Upper(string key, Func<double, ItSeries, double> shaft) =>
        new(key, FitBound.Upper, tabulates: false, shaft);

    private static FitLetter Lower(string key, Func<double, ItSeries, double> shaft) =>
        new(key, FitBound.Lower, tabulates: false, shaft);

    // The composite letters are the GEOMETRIC MEAN of their neighbours' magnitudes, resolved at CALL time so a
    // static field initializer never reads a sibling row the runtime has not seated yet.
    private static double Blend(string first, string second, double meanMm, ItSeries series) =>
        Math.Sqrt(Math.Abs(Get(first).ShaftMicrometers(meanMm, series))
            * Math.Abs(Get(second).ShaftMicrometers(meanMm, series)));
}

// The IT series indexed by grade NUMBER, so the m, p, s, t, u, v, x, y, z, za, zb, and zc formulas read the grade
// terms `ItGradeName` already computes rather than carrying a per-band tabulation of the same values.
public readonly record struct ItSeries(double GeometricMeanMm) {
    public double At(int grade) => ItGradeName.Of(grade).Map(row => row.Micrometers(GeometricMeanMm)).IfNone(0.0);
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

    // Items-derived grade index, materialized on first read: every fundamental-deviation formula expressed in IT
    // terms resolves its grade here rather than re-scanning the roster per evaluation.
    private static readonly Lazy<FrozenDictionary<int, ItGradeName>> ByNumber = new(
        static () => Items.ToFrozenDictionary(static row => row.Number),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Option<ItGradeName> Of(int grade) => ByNumber.Value.TryGetValue(grade, out ItGradeName? row)
        ? Some(row)
        : None;

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

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<double>]
[ValidationError<FabricationFault>]
public readonly partial struct FinishingAllowanceFactor {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value >= 0.0
            ? null : Tolerance.Refusal("finishing-allowance-factor");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DiameterBand {
    public double LowerMm { get; }
    public double UpperMm { get; }

    // ISO 286-1 evaluates every IT and deviation formula at the band's GEOMETRIC MEAN, and the first band's zero
    // lower bound takes the standard's own 1 mm substitute. A caller-supplied reference is a second truth that can
    // disagree with the formula the same standard publishes, so the band derives it.
    public double ReferenceMm => Math.Sqrt(Math.Max(LowerMm, 1.0) * UpperMm);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double lowerMm, ref double upperMm) =>
        validationError = double.IsFinite(lowerMm) && lowerMm >= 0.0 && double.IsFinite(upperMm) && upperMm > lowerMm
            ? null : Tolerance.Refusal("diameter-band");

    public bool Contains(double diameterMm) => diameterMm > LowerMm && diameterMm <= UpperMm;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ItGrade {
    public ItGradeName Name { get; }
    public DiameterBand Diameter { get; }
    public FinishingAllowanceFactor AllowanceFactor { get; }
    public int Number => Name.Number;
    public double ToleranceMicrometers => Name.Micrometers(Diameter.ReferenceMm);
    public double ToleranceMillimeters => Length.FromMicrometers(ToleranceMicrometers).Millimeters;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref ItGradeName name,
        ref DiameterBand diameter, ref FinishingAllowanceFactor allowanceFactor) =>
        validationError = double.IsFinite(allowanceFactor.ToValue())
            && allowanceFactor.ToValue() >= 0.0 && double.IsFinite(name.Micrometers(diameter.ReferenceMm))
            && name.Micrometers(diameter.ReferenceMm) > 0.0 ? null : Tolerance.Refusal("it-grade");
}

// An EXCEPTION, not a transcription: a row exists only where ISO 286-1 publishes a value its own formulas do not
// generate — shaft j and hole J across grades 5 through 8, the p step, and k outside grades 4 through 7. A row
// duplicating what `FitLetter` derives is the deleted form, because the two would then disagree silently.
public readonly record struct FitException(
    FitMember Member,
    FitLetter Letter,
    DiameterBand Diameter,
    Option<ItGradeName> Grade,
    FitBound Bound,
    double FundamentalMicrometers);

// The GENERATIVE deviation owner. `Resolve` reads an exception where one exists and derives the value from the
// letter's own closed form otherwise, so a standard revision that widens a band roster costs one `DiameterBand`
// row and a revision that changes a formula costs one delegate — never a thousand transcribed micrometre cells.
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class FitStandard {
    public Arr<DiameterBand> Diameters { get; }
    public Arr<FitException> Exceptions { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref Arr<DiameterBand> diameters,
        ref Arr<FitException> exceptions) =>
        validationError = diameters.Count > 0
            && exceptions.ForAll(static row => double.IsFinite(row.FundamentalMicrometers))
            && exceptions.ForAll(row => diameters.Exists(candidate => candidate == row.Diameter))
            && exceptions.Map(static row => (row.Member, row.Letter, row.Diameter, row.Grade)).Distinct().Count == exceptions.Count
                ? null : Tolerance.Refusal("fit-standard");

    // A grade-specific exception outranks the grade-agnostic row of the same member, letter, and band; absence of
    // both hands the letter's own formula, and only a TABULATED letter with no exception refuses.
    public Fin<(FitBound Bound, double FundamentalMicrometers)> Resolve(
        FitMember member, FitLetter letter, ItGradeName grade, DiameterBand diameter) =>
        toSeq(Exceptions)
            .Filter(row => row.Member == member && row.Letter == letter && row.Diameter == diameter)
            .Fold(Option<FitException>.None, (held, row) =>
                row.Grade.Exists(candidate => candidate == grade) ? Some(row)
                    : held.IsSome || row.Grade.IsSome ? held : Some(row))
            .Map(static row => Fin.Succ((row.Bound, row.FundamentalMicrometers)))
            .IfNone(() => letter.Tabulates
                ? Fin.Fail<(FitBound, double)>(Tolerance.Invalid("fit-standard",
                    $"a tabulated exception for {member.ToValue()}{letter.ToValue()}{grade.ToValue()}"))
                : Fin.Succ(Derived(member, letter, grade, diameter)));

    // The general rule: the shaft deviation is the formula's own value, and the hole is its MIRROR — sign and
    // bound both flip. The Δ correction ISO 286-1 applies to holes K through ZC at grades at or below 8 is the
    // difference between that grade's tolerance and the next finer one, added back so a mirrored hole preserves
    // the fit character its shaft partner was designed against.
    private static (FitBound Bound, double FundamentalMicrometers) Derived(
        FitMember member, FitLetter letter, ItGradeName grade, DiameterBand diameter) {
        ItSeries series = new(diameter.ReferenceMm);
        double shaft = letter.ShaftMicrometers(diameter.ReferenceMm, series);
        return member == FitMember.Shaft
            ? (letter.Bound, shaft)
            : (Mirror(letter.Bound), -shaft + Delta(letter, grade, series));
    }

    private static double Delta(FitLetter letter, ItGradeName grade, ItSeries series) =>
        letter.Bound == FitBound.Lower && grade.Number <= 8
            ? series.At(grade.Number) - series.At(grade.Number - 1)
            : 0.0;

    private static FitBound Mirror(FitBound bound) => bound.Switch(
        lower: static _ => FitBound.Upper,
        upper: static _ => FitBound.Lower,
        symmetric: static _ => FitBound.Symmetric);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class FitClass {
    public FitMember Member { get; }
    public FitLetter Letter { get; }
    public ItGrade Grade { get; }
    public FitBound FundamentalBound { get; }
    public double FundamentalMicrometers { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref FitMember member,
        ref FitLetter letter, ref ItGrade grade, ref FitBound fundamentalBound, ref double fundamentalMicrometers) =>
        validationError = double.IsFinite(fundamentalMicrometers) ? null : Tolerance.Refusal("fit-class");

    public (double LowerUm, double UpperUm) Limits =>
        FundamentalBound.Deviations(FundamentalMicrometers, Grade.ToleranceMicrometers);

    public string Designation => Member == FitMember.Hole
        ? string.Concat(Letter.ToValue().ToUpperInvariant(), Grade.Number.ToString(CultureInfo.InvariantCulture))
        : string.Concat(Letter.ToValue(), Grade.Number.ToString(CultureInfo.InvariantCulture));

    // Deviations are published in micrometres and sizes read in millimetres, so the conversion rides the quantity
    // owner: a bare divisor at each site is one more place the unit regime can be transposed silently.
    public (double LowerMm, double UpperMm) Sizes(double nominalMm) =>
        (nominalMm + Length.FromMicrometers(Limits.LowerUm).Millimeters,
         nominalMm + Length.FromMicrometers(Limits.UpperUm).Millimeters);

    public static Fin<FitClass> Admit(FitMember member, FitLetter letter, ItGrade grade, FitStandard standard) =>
        from seed in standard.Resolve(member, letter, grade.Name, grade.Diameter)
        select Create(member, letter, grade, seed.Bound, seed.FundamentalMicrometers);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref ContentKey source,
        ref double nominalMm, ref FitClass hole, ref FitClass shaft, ref FitCharacter character) {
        if (source is null || hole is null || shaft is null || character is null) {
            validationError = Tolerance.Refusal("fit-limits");
            return;
        }
        (double LowerMm, double UpperMm) holeSizes = hole.Sizes(nominalMm);
        (double LowerMm, double UpperMm) shaftSizes = shaft.Sizes(nominalMm);
        double maximum = holeSizes.UpperMm - shaftSizes.LowerMm;
        double minimum = holeSizes.LowerMm - shaftSizes.UpperMm;
        FitCharacter derived = FitCharacter.Of(minimum, maximum);
        validationError = hole.Member == FitMember.Hole && shaft.Member == FitMember.Shaft
            && hole.Grade.Diameter == shaft.Grade.Diameter
            && double.IsFinite(nominalMm) && hole.Grade.Diameter.Contains(nominalMm)
            && double.IsFinite(maximum) && double.IsFinite(minimum) && maximum >= minimum
            && character == derived ? null : Tolerance.Refusal("fit-limits");
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
[ValidationError<FabricationFault>]
public sealed partial class GeneralStandard {
    public Arr<GeneralSeed> Seeds { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref Arr<GeneralSeed> seeds) =>
        validationError = seeds.Count > 0
            && seeds.ForAll(static row => row.Limit.Valid() && row.Kind.Admits(row.Limit))
            && seeds.Map(static row => (row.Class, row.Kind, row.Band)).Distinct().Count == seeds.Count
            && !seeds.Exists(left => seeds.Exists(right => left != right && left.Overlaps(right)))
                ? null : Tolerance.Refusal("general-standard");

    public Fin<GeneralLimit> Resolve(GeneralToleranceClass @class, GeneralToleranceKind kind, double nominalMm) =>
        Seeds.Filter(row => row.Class == @class && row.Kind == kind && row.Band.Contains(nominalMm))
            .Map(static row => row.Limit).Head.ToFin(Tolerance.Invalid("general-standard:band"));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class GeneralTolerance {
    public ContentKey Source { get; }
    public GeneralToleranceClass Class { get; }
    public GeneralToleranceKind Kind { get; }
    public double NominalMm { get; }
    public GeneralLimit Limit { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref ContentKey source,
        ref GeneralToleranceClass @class, ref GeneralToleranceKind kind, ref double nominalMm, ref GeneralLimit limit) =>
        validationError = double.IsFinite(nominalMm)
            && nominalMm > 0.0 && limit.Valid()
            && kind.Admits(limit)
                ? null : Tolerance.Refusal("general-tolerance");
}
```

## [05]-[SURFACE_TEXTURE]

- Owner: `SurfaceParameter` owns the ISO 21920 parameter roster and its Ra correspondence; `SurfaceMeasure` owns BOTH halves of what a limit means — the band it must hold and the unit it is read in; `SurfaceTexture` owns the admitted requirement set and `RaTarget` its scallop projection.
- Cases: `SurfaceLimit` closes exact, maximum, minimum, and ranged acceptance; `SurfaceRequirement` is ONE shape whose measure decides which optional column it demands, so six cases mirroring six measure rows — where a parameter and a case could disagree — collapse to one.
- Boundary: roughness correspondence is not a strategy — `SurfaceParameter.RaRatio` is the declared datum, and a parameter without one refuses rather than inferring a ratio.
- Growth: a surface parameter is one row naming its profile and its measure; a measure is one row carrying its unit and its admitted band.

```csharp signature
// --- [VOCABULARIES] -------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SurfaceProfile {
    public static readonly SurfaceProfile Roughness = new("roughness");
    public static readonly SurfaceProfile Waviness = new("waviness");
    public static readonly SurfaceProfile Primary = new("primary");
}

// The measure owns BOTH halves of what a limit means: the band it must hold and the unit it is read in. A
// requirement case naming the unit in its own member name kept the second half where the vocabulary could not
// see it.
[SmartEnum<string>]
public sealed partial class SurfaceMeasure {
    public static readonly SurfaceMeasure Amplitude = Positive("amplitude", LengthUnit.Micrometer);
    public static readonly SurfaceMeasure Spacing = Positive("spacing", LengthUnit.Millimeter);
    public static readonly SurfaceMeasure Ratio = Percent("ratio");
    public static readonly SurfaceMeasure LevelRatio = Percent("level-ratio");
    public static readonly SurfaceMeasure Difference = Positive("difference", LengthUnit.Micrometer);
    // A shape parameter is a dimensionless moment, so its limit reads as a bare decimal fraction.
    public static readonly SurfaceMeasure Shape = new("shape", RatioUnit.DecimalFraction,
        static limit => limit.Valid(static value => double.IsFinite(value)));

    public Enum Unit { get; }

    private static SurfaceMeasure Positive(string key, Enum unit) => new(key, unit,
        static limit => limit.Valid(static value => double.IsFinite(value) && value > 0.0));
    private static SurfaceMeasure Percent(string key) => new(key, RatioUnit.Percent,
        static limit => limit.Valid(static value => double.IsFinite(value) && value is >= 0.0 and <= 100.0));

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

    internal bool Valid(Func<double, bool> admits) => Switch(
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

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<double>]
[ValidationError<FabricationFault>]
public readonly partial struct ScallopFactor {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0.0 ? null : Tolerance.Refusal("scallop-factor");
}

// The requirement is ONE shape: its parameter already names the measure, and the measure already owns the unit
// its limit reads and the band that limit must hold. Six cases mirroring six measure rows were a shadow of the
// vocabulary — a parameter and a case could disagree, and the admission existed only to catch a contradiction
// the second family created. The two payload extras a measure genuinely needs ride as their own optional
// columns, present exactly where their measure demands them.
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class SurfaceRequirement {
    public SurfaceParameter Parameter { get; }
    public SurfaceLimit Limit { get; }

    // The evaluation level a bearing-ratio requirement is read at, and the two material-ratio depths a difference
    // requirement spans: each present exactly when its own measure names it.
    public Option<double> LevelMicrometers { get; }
    public Option<(double FromPercent, double ToPercent)> MaterialBand { get; }

    public SurfaceMeasure Measure => Parameter.Measure;
    public Enum Unit => Parameter.Measure.Unit;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref SurfaceParameter parameter,
        ref SurfaceLimit limit,
        ref Option<double> levelMicrometers,
        ref Option<(double FromPercent, double ToPercent)> materialBand) {
        if (!parameter.Measure.Admits(limit)
            || levelMicrometers.IsSome != (parameter.Measure == SurfaceMeasure.LevelRatio)
            || materialBand.IsSome != (parameter.Measure == SurfaceMeasure.Difference)
            || levelMicrometers.Exists(static value => !double.IsFinite(value))
            || materialBand.Exists(static band => !double.IsFinite(band.FromPercent) || !double.IsFinite(band.ToPercent)
                || band.FromPercent is < 0.0 or > 100.0 || band.ToPercent <= band.FromPercent || band.ToPercent > 100.0))
            validationError = Tolerance.Refusal("surface-requirement");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class TransmissionBand {
    public double CutoffMm { get; }
    public double SamplingMm { get; }
    public double EvaluationMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double cutoffMm,
        ref double samplingMm, ref double evaluationMm) =>
        validationError = double.IsFinite(cutoffMm) && cutoffMm > 0.0 && double.IsFinite(samplingMm) && samplingMm > 0.0
            && double.IsFinite(evaluationMm) && evaluationMm >= samplingMm ? null : Tolerance.Refusal("transmission-band");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class SurfaceTexture {
    public ContentKey Source { get; }
    public Arr<SurfaceRequirement> Requirements { get; }
    public SurfaceLay Lay { get; }
    public ProcessMark Mark { get; }
    public Option<TransmissionBand> Band { get; }
    public Option<double> MachiningAllowanceMm { get; }
    public Option<string> Treatment { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref ContentKey source,
        ref Arr<SurfaceRequirement> requirements, ref SurfaceLay lay, ref ProcessMark mark, ref Option<TransmissionBand> band,
        ref Option<double> machiningAllowanceMm, ref Option<string> treatment) =>
        validationError = requirements.Count > 0
            && requirements.Map(static row => row.Parameter).Distinct().Count == requirements.Count
            && machiningAllowanceMm.ForAll(static value => double.IsFinite(value) && value >= 0.0)
            && treatment.ForAll(static value => !string.IsNullOrWhiteSpace(value)) ? null : Tolerance.Refusal("surface-texture");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class RaTarget {
    public double Micrometers { get; }
    public ScallopFactor Factor { get; }
    public double ScallopHeightMm => Length.FromMicrometers(Micrometers * Factor.ToValue()).Millimeters;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double micrometers,
        ref ScallopFactor factor) =>
        validationError = double.IsFinite(micrometers) && micrometers > 0.0 && double.IsFinite(factor.ToValue())
            && factor.ToValue() > 0.0 ? null : Tolerance.Refusal("ra-target");

    // SurfaceTexture amplitude rows drive the target; a parameter without a declared Ra ratio cannot.
    public static Fin<RaTarget> From(SurfaceTexture texture, SurfaceParameter source, ScallopFactor factor) =>
        from admittedTexture in Optional(texture).ToFin(Tolerance.Invalid("surface-texture:raw"))
        from admittedSource in Optional(source).ToFin(Tolerance.Invalid("surface-texture:source"))
        from ratio in admittedSource.RaRatio.ToFin(Tolerance.Invalid("surface-texture",
            $"{admittedSource.ToValue()} declares no Ra correspondence"))
        from measured in toSeq(admittedTexture.Requirements)
            .Filter(requirement => requirement.Parameter == admittedSource
                && requirement.Measure == SurfaceMeasure.Amplitude)
            .Choose(static requirement => requirement.Limit.Upper())
            .Head.ToFin(Tolerance.Invalid("surface-texture",
                $"{admittedSource.ToValue()} amplitude upper limit"))
        let micrometers = measured * ratio
        from _ in guard(double.IsFinite(micrometers) && micrometers > 0.0, Tolerance.Range("surface-texture:ra",
            micrometers, "finite and positive")).ToFin()
        select Create(micrometers, factor);
}
```

## [06]-[STACK_CHAIN]

- Owner: `ToleranceChain` owns the term roster, its declared method, and its bound; `StackMethod` owns the analytic combination and the contribution ranking; `ChainReceipt` is the ONE stackup receipt `Spec/capability`, `Spec/manufacturability`, and `Documentation/report` all read.
- Law: a `ProcessDistribution` weight is the standard deviation a term contributes PER UNIT half-range, so a root-sum-square combines comparable variances and the widest-spreading distribution carries the SMALLEST weight; the row's SEEDED family is what the correlated Monte-Carlo route draws from, so a statistical stack is simulated rather than approximated by an inflation factor with no distribution behind it.
- Law: the declared method is the default reading and `Evaluate(StackMethod)` evaluates the SAME terms under any other, so a consumer wanting the arithmetic bound beside the statistical one reads two rows of one algebra and a second worst-case fold has no site. A term's share is its own combined magnitude under that same algebra, so the ranking never forks the law.
- Growth: a stackup method is one row carrying its combination delegate; a process distribution is one row carrying its quadrature weight and its seeded family.
- Boundary: the chain declares terms and combines them; the shared-factor loadings, systematic offsets, and measured fits a simulation needs are `Spec/capability` contributors bound to these terms by key.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ToleranceInterval {
    public double LowerMm { get; }
    public double UpperMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref double lowerMm, ref double upperMm) =>
        validationError = double.IsFinite(lowerMm) && double.IsFinite(upperMm) && lowerMm <= upperMm
            ? null : Tolerance.Refusal("tolerance-interval");
}

// The quadrature weight is the standard deviation a term contributes PER UNIT half-range, so a root-sum-square
// combines comparable variances. Every row states the closed form of that ratio for its own distribution: a
// uniform half-range is sqrt(3) standard deviations wide, so its weight DIVIDES by sqrt(3) — multiplying inflates
// the very distribution that spreads least. A skewed process is the beta family the shop actually observes, and
// its weight is the beta standard deviation at the declared shape rather than a bare inflation factor.
[SmartEnum<string>]
public sealed partial class ProcessDistribution {
    public static readonly ProcessDistribution Normal = new("normal", 1.0 / 3.0,
        static source => MathNet.Numerics.Distributions.Normal.WithMeanStdDev(0.0, 1.0 / 3.0, source));
    public static readonly ProcessDistribution Uniform = new("uniform", 1.0 / Math.Sqrt(3.0),
        static source => new ContinuousUniform(-1.0, 1.0, source));
    public static readonly ProcessDistribution Triangular = new("triangular", 1.0 / Math.Sqrt(6.0),
        static source => new Triangular(-1.0, 1.0, 0.0, source));
    // Beta(2, 4) over the half-range interval: mean displaced toward the lower limit, which is the tool-wear drift
    // a single-sided process shows, and a REAL sampler the Monte-Carlo route draws from.
    public static readonly ProcessDistribution Skewed = new("skewed", Math.Sqrt(2.0 * 4.0 / (36.0 * 7.0)) * 2.0,
        static source => new Beta(2.0, 4.0, source));

    private ProcessDistribution(string key, double quadratureWeight, Func<Random, IContinuousDistribution> seeded) : this(key) =>
        (QuadratureWeight, Seeded) = (quadratureWeight, seeded);

    public double QuadratureWeight { get; }

    // The row's family SEEDED by the caller's own stream. A pre-built shared instance cannot serve a simulation
    // whose receipt publishes a replay seed — every draw would come off a stream nobody can reproduce, and a
    // parallel trial fold would race one generator. The caller holds the instance across its trial run.
    [IgnoreEquality]
    public Func<Random, IContinuousDistribution> Seeded { get; }

    // The STANDARDIZED deviate the term scales: each arm centres and normalizes its own support, so a draw is
    // comparable across rows and the caller multiplies by whichever spread it is spending.
    public double Standardize(double sample) => Switch(
        state: sample,
        normal: static (value, _) => Math.Clamp(value * 3.0, -3.0, 3.0) / 3.0,
        uniform: static (value, _) => value,
        triangular: static (value, _) => value,
        skewed: static (value, _) => (2.0 * value) - 1.0);

    public double Draw(Random source, double halfRangeMm) => Standardize(Seeded(source).Sample()) * halfRangeMm;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ToleranceTerm {
    public string Key { get; }
    public ToleranceInterval Interval { get; }
    public double Sensitivity { get; }
    public ProcessDistribution Distribution { get; }
    public double LowerMm => double.Min(Interval.LowerMm * Sensitivity, Interval.UpperMm * Sensitivity);
    public double UpperMm => double.Max(Interval.LowerMm * Sensitivity, Interval.UpperMm * Sensitivity);
    public double HalfRangeMm => (UpperMm - LowerMm) * 0.5;
    public double StatisticalHalfRangeMm => HalfRangeMm * Distribution.QuadratureWeight;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref string key,
        ref ToleranceInterval interval, ref double sensitivity, ref ProcessDistribution distribution) {
        key = key?.Trim() ?? string.Empty;
        validationError = key.Length > 0 && double.IsFinite(sensitivity) && sensitivity != 0.0
            ? null : Tolerance.Refusal("tolerance-term");
    }

    public static Fin<ToleranceTerm> Of(string key, FitClass fit, double nominalMm, double sensitivity,
        ProcessDistribution distribution) =>
        from admitted in Optional(fit).ToFin(Tolerance.Invalid("tolerance-term:fit"))
        from _ in guard(double.IsFinite(nominalMm) && admitted.Grade.Diameter.Contains(nominalMm),
            Tolerance.Range("tolerance-term:nominal", nominalMm, "finite and inside the fit diameter band")).ToFin()
        let sizes = admitted.Sizes(nominalMm)
        from interval in ToleranceInterval.Validate(sizes.LowerMm - nominalMm, sizes.UpperMm - nominalMm,
            out ToleranceInterval bounds).Admitted(bounds)
        from term in Validate(key, interval, sensitivity, distribution, out ToleranceTerm value).Admitted(value)
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
[ValidationError<FabricationFault>]
public sealed partial class ToleranceChain {
    public ContentKey Source { get; }
    public Arr<ToleranceTerm> Terms { get; }
    public double BoundMm { get; }
    public StackMethod Method { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref ContentKey source,
        ref Arr<ToleranceTerm> terms, ref double boundMm, ref StackMethod method) =>
        validationError = terms.Count > 0
            && terms.Map(static row => row.Key).Distinct().Count == terms.Count
            && double.IsFinite(boundMm) && boundMm > 0.0 ? null : Tolerance.Refusal("tolerance-chain");

    // The declared method is the default reading, and the overload evaluates the SAME terms under any method — so a
    // consumer wanting the arithmetic bound beside the statistical one reads two rows of ONE algebra rather than
    // re-spelling a worst-case fold of its own beside a simulation.
    public ChainReceipt Evaluate() => Evaluate(Method);

    public ChainReceipt Evaluate(StackMethod method) =>
        (Rows: toSeq(Terms), Total: method.Combine(toSeq(Terms))) switch {
            var chain => new ChainReceipt(Source, method,
                chain.Rows.Fold(0.0, static (sum, term) => sum + term.LowerMm),
                chain.Rows.Fold(0.0, static (sum, term) => sum + term.UpperMm),
                chain.Total,
                toSeq(chain.Rows.Map(term => (Term: term.Key, Share: method.Share(term, chain.Total)))
                    .OrderByDescending(static row => row.Share).ThenBy(static row => row.Key)).ToArr(),
                BoundMm),
        };
}

public sealed record ChainReceipt(ContentKey Source, StackMethod Method, double WorstLowerMm, double WorstUpperMm,
    double HalfRangeMm, Arr<(string Term, double Share)> Contributions, double BoundMm) {
    public double CentreMm => (WorstLowerMm + WorstUpperMm) * 0.5;
    public bool Conforming => double.Max(Math.Abs(CentreMm - HalfRangeMm), Math.Abs(CentreMm + HalfRangeMm)) <= BoundMm;
    public Option<(string Term, double Share)> Dominant => HalfRangeMm > 0.0
        ? toSeq(Contributions).Head
        : None;
}
```

## [07]-[OWNER_FOLD]

- Owner: `Tolerance` is the canonical `[Union]` and `Tolerance.Apply` the one fold; each raw case enters through one generated invariant owner and leaves through `ToleranceReceipt`.
- Cases: `Tolerance` closes geometric, fit, texture, general, and chain specifications and projects `Source` and `Qif` over all five; `ToleranceRequest` adds the derivation and egress modalities — quantity, effective condition, scallop, allowance, and projection — as payload-complete cases.
- Law: generated owner validation is the single admission authority and every owner refuses on the fabrication band under its own locus, so `Admission.Admitted` is the one bridge onto `Fin` and a page-local lift re-wrapping that refusal is the deleted form.
- Law: the axis names a QUANTITY FAMILY and `QuantityInfo` is what UnitsNet gives that family as identity — a `Type` compares by CLR reflection while the parse, the unit roster, and the base dimensions all resolve off the info row, so two axes over one family stay distinct by axis while sharing one identity.
- Packages: `Thinktecture.Runtime.Extensions` owns admission and dispatch; `LanguageExt.Core` owns accumulating admission, closed-fault sequencing, and immutable folds; `Rasm.Domain` owns `Op`, `Fault.InvalidValue`, and `Fault.OutOfRange`; `UnitsNet` owns runtime-selected quantity parsing and `IQuantity.As` unit projection; `MathNet.Numerics` owns the stack distribution families; `CutterForm` carries MTConnect-derived ISO-13399 geometry and its `CutterFamily` decides whether a cusp exists at all.
- Boundary: `IToleranceEncoder` is the open egress strategy; format and culture state close inside its implementation, so `ToleranceRequest.Project` carries one policy value instead of delegate and provider knobs.

```csharp signature
// --- [VOCABULARIES] -------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SpecAxis {
    public static readonly SpecAxis Length = new("length", UnitsNet.Length.Info, LengthUnit.Millimeter);
    public static readonly SpecAxis Angle = new("angle", UnitsNet.Angle.Info, AngleUnit.Degree);
    public static readonly SpecAxis Roughness = new("roughness", UnitsNet.Length.Info, LengthUnit.Micrometer);
    public static readonly SpecAxis Reference = new("reference", UnitsNet.Temperature.Info, TemperatureUnit.DegreeCelsius);
    public static readonly SpecAxis Restraint = new("restraint", UnitsNet.Force.Info, ForceUnit.Newton);

    public QuantityInfo Quantity { get; }
    public Enum CanonicalUnit { get; }

    public bool Admits(IQuantity value) => value.QuantityInfo.BaseDimensions.Equals(Quantity.BaseDimensions);

    public double Canonical(IQuantity value) => value.As(CanonicalUnit);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class SpecQuantity {
    public SpecAxis Axis { get; }
    public double Canonical { get; }
    public string Received { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref SpecAxis axis,
        ref double canonical, ref string received) {
        received = received?.Trim() ?? string.Empty;
        validationError = double.IsFinite(canonical) && received.Length > 0
            ? null : Tolerance.Refusal("spec-quantity");
    }

    public static Fin<SpecQuantity> Admit(SpecAxis axis, string text) =>
        !string.IsNullOrWhiteSpace(text)
            && Quantity.TryParse(CultureInfo.InvariantCulture, axis.Quantity.ValueType, text, out IQuantity? quantity)
            && axis.Admits(quantity)
                ? Fin.Succ(Create(axis, axis.Canonical(quantity), text))
                : Fin.Fail<SpecQuantity>(Tolerance.Invalid("spec-quantity",
                    $"{axis.Quantity.Name} parseable under the invariant culture"));
}

// --- [SERVICES] -----------------------------------------------------------------------------------------------------------------------------------
public interface IToleranceEncoder {
    Fin<ReadOnlyMemory<byte>> Encode(Tolerance value);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
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

    // Every generated owner on this page refuses on the fabrication band under its own locus, so one `Admitted`
    // read closes every admission and a page-local lift re-wrapping the refusal in a second message is deleted.
    internal static FabricationFault Refusal(string locus) =>
        new FabricationFault.PolicyInadmissible(FabConcern.Spec, $"tolerance:{locus}");

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Feature raw) =>
        from control in FeatureControl.Admit(raw)
        from _ in control.AchievableMm.Filter(achievable => achievable > control.Zone.Width.ToValue()).Match(
            Some: achievable => Fin.Fail<Unit>(FabricationFault.ToleranceUnsatisfiable(
                new FaultSubject.Specification(control.Source), achievable)),
            None: static () => Fin.Succ(unit))
        select new ToleranceReceipt.Frame(new Geometric(control), FeatureFrameReceipt.Create(control));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Fit demand) =>
        from admitted in Optional(demand).ToFin(Invalid("fit:raw"))
        from _pair in guard(admitted.Hole.Member == FitMember.Hole && admitted.Shaft.Member == FitMember.Shaft
            && admitted.Hole.Grade.Diameter == admitted.Shaft.Grade.Diameter, Invalid("fit:pair")).ToFin()
        let maximum = Length.FromMicrometers(admitted.Hole.Limits.UpperUm - admitted.Shaft.Limits.LowerUm).Millimeters
        let minimum = Length.FromMicrometers(admitted.Hole.Limits.LowerUm - admitted.Shaft.Limits.UpperUm).Millimeters
        from _limits in guard(double.IsFinite(maximum) && double.IsFinite(minimum) && maximum >= minimum,
            Invalid("fit:limits")).ToFin()
        let character = FitCharacter.Of(minimum, maximum)
        from limits in FitLimits.Validate(admitted.Source, admitted.NominalMm, admitted.Hole, admitted.Shaft,
            character, out FitLimits value).Admitted(value)
        select (ToleranceReceipt)new ToleranceReceipt.Fitted(new Fit(limits));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Texture demand) =>
        from admitted in Optional(demand).ToFin(Invalid("surface-texture:raw"))
        from texture in SurfaceTexture.Validate(admitted.Source, admitted.Requirements, admitted.Lay,
            admitted.Mark, admitted.Band, admitted.MachiningAllowanceMm, admitted.Treatment,
            out SurfaceTexture value).Admitted(value)
        select (ToleranceReceipt)new ToleranceReceipt.Textured(new Texture(texture));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.General demand) =>
        from admitted in Optional(demand).ToFin(Invalid("general-tolerance:raw"))
        from standard in Optional(admitted.Standard).ToFin(Invalid("general-tolerance:standard"))
        from limit in standard.Resolve(admitted.Class, admitted.Kind, admitted.NominalMm)
        from value in GeneralTolerance.Validate(admitted.Source, admitted.Class, admitted.Kind,
            admitted.NominalMm, limit, out GeneralTolerance tolerance).Admitted(tolerance)
        select (ToleranceReceipt)new ToleranceReceipt.Generalized(new General(value));

    private static Fin<ToleranceReceipt> Admit(ToleranceRequest.Chain demand) =>
        from admitted in Optional(demand).ToFin(Invalid("tolerance-chain:raw"))
        from chain in ToleranceChain.Validate(admitted.Source, admitted.Terms, admitted.BoundMm,
            admitted.Method, out ToleranceChain value).Admitted(value)
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
        from bytes in Try.lift<Fin<ReadOnlyMemory<byte>>>(f: () => demand.Encoder.Encode(demand.Value)).Run()
            .MapFail(error => Invalid("project:encode", error.Message)).Bind(static result => result)
        select (ToleranceReceipt)new ToleranceReceipt.Projected(demand.Value, bytes);

    // Only a rotationally swept corner leaves a cusp, and the sweep radius is the family's own `CornerRule` — the
    // same behavior column `CutterFamily.Fits` admits against. Dispatching the sixteen-row family arm by arm strands
    // every row the roster gains; the four-row corner rule is total over all sixteen and stays total as they grow.
    private static Fin<ToleranceReceipt> Scallop(RaTarget target, CutterForm cutter) =>
        from admittedTarget in Optional(target).ToFin(Invalid("scallop:target"))
        from admittedCutter in Optional(cutter).ToFin(Invalid("scallop:cutter"))
        from radius in admittedCutter.Family.Corner.Switch(
                state: (Half: admittedCutter.Diameter * 0.5, Corner: admittedCutter.CornerRadius),
                sharp: static _ => Option<double>.None,
                full: static state => Some(state.Half),
                partial: static state => Some(state.Corner),
                any: static state => Some(state.Corner))
            .ToFin(Invalid("scallop:family", $"a cusp-forming cutter family, not {admittedCutter.Family.Key}"))
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

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
