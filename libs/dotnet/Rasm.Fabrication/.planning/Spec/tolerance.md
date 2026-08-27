# [RASM_FABRICATION_TOLERANCE]

`ToleranceSpec` owns every production-specification value from raw quantity admission through geometric control, ISO 286 fit, general tolerance, surface texture, method-parameterized stackup, typed derivation, and parameterized wire projection. `FeatureControl`, `FitClass`, `SurfaceTexture`, and `ToleranceChain` admit once, while `ToleranceSpec.Apply` dispatches every ingress, operation, and egress modality on payload-complete `ToleranceRequest` cases and answers each in the one result case its request seats.

The GD&T algebra is a DRAWING-STANDARD vocabulary, so its glyphs, datum letters, and characteristic legality compose the kernel `Drawing/sheet` owners — `GeometricCharacteristic`, `DatumDesignator`, `DatumRegime`, `ZoneModifier`, and `SymbolSet.For(standard)` — and this page declares only the specification structure those rows are read into. The bare-scalar tolerance carrier is the kernel `Domain/context` `Tolerance`, which is why the specification union is `ToleranceSpec`: two types named `Tolerance` in one compilation shadow by namespace proximity, and the closer name silently wins at every boundary that meant the other.

`ToleranceSpec` preserves the cross-runtime wire name consumed by the artifacts plane without exporting its C# shape, keys structural refusals through `SpecOp`, carries `ContentKey` into `ToleranceUnsatisfiable`, consumes measured cutter geometry through admitted `CutterForm`, and accepts capability evidence only as input-carried achievable width. `Rasm.Solving` owns the name `Fitted`, so the ISO 286 pairing result is `FitLimits`.

## [01]-[INDEX]

- [02]-[GEOMETRIC_VOCABULARY]: `FeatureCharacteristic` over the kernel characteristic rows, and the scope, zone, modifier, and material vocabularies carrying ISO 1101 legality as row behavior.
- [03]-[FEATURE_CONTROL]: kind-parameterized `ToleranceZone`, the datum system and frame extension, `FeatureControl.Admit`, and the layout-free `FeatureFrame.Annotation` stream.
- [04]-[FIT_ALGEBRA]: generated `ItGradeName`, closed-form `FitLetter` deviations over `DiameterBand`, the `FitException` carve, and the validated fit and general-tolerance seed laws.
- [05]-[SURFACE_TEXTURE]: the ISO 21920 parameter roster, its measure-owned units and bands, the one-shape requirement, and the `RaTarget` scallop projection.
- [06]-[STACK_CHAIN]: `ToleranceTerm`, the `StackMethod` analytic algebra, and the `ChainEvidence` every stackup consumer reads.
- [07]-[OWNER_FOLD]: `SpecAxis` quantity admission, the request-indexed `ToleranceSpec.Apply` fold carrying each `ToleranceRequest` case onto the `ToleranceResult` case it seats, and the generated `FeatureControlWire` protobuf egress.

## [02]-[GEOMETRIC_VOCABULARY]

- Owner: `FeatureCharacteristic` owns the SPECIFICATION legality of each ISO 1101 control and names the kernel `GeometricCharacteristic` row that owns its glyph and datum regime; `FeatureScope`, `ToleranceZoneKind`, `MaterialCondition`, and `FrameModifier` own the axes a frame is graded against.
- Cases: `FeatureScope` distinguishes surface, axis, median-line, median-plane, and center-point controls before material-condition policy resolves; datum dependence is the kernel `DatumRegime` the characteristic's own row carries, so this page states no second tri-valued vocabulary for it.
- Law: a glyph is the DRAWING STANDARD's, never this page's. The characteristic symbol, the diameter prefix, and the material-condition and free-state marks read the kernel `GeometricCharacteristic.Glyph` and `ZoneModifier.Glyph` rows, and `SymbolSet.For(standard)` decides which characteristics a publishing standard admits at all — so an ASME Y14.5 frame cannot spell concentricity or symmetry, which a roster admitting all fourteen unconditionally had no way to refuse. The one glyph this page still declares is the regardless-of-feature-size mark, because ISO 2692 retired it and the kernel roster carries no row for a symbol no current standard prints.
- Auto: `FrameModifier.Admits`, `FeatureCharacteristic.AdmitsScope`, and `MaterialCondition.Boundaries` carry the ISO 1101 legality and virtual-condition law as ROW BEHAVIOR, so the admitting owner never re-derives it; `FeatureCharacteristic.EffectiveClass` grades a profile control across all three ISO 1101 steps, so a singly-referenced profile claims orientation rather than the location its drawing never constrained.
- Growth: a geometric characteristic is one row here naming its kernel counterpart; a zone kind is one row carrying its own second-dimension admission; a modifier is one row carrying its applicability.
- Boundary: a row states legality alone — the frame that composes them, its datum system, and its settled projection live at `[03]`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Threading;
using Celly.Protovalidate;
using Foundation.CSharp.Analyzers.Contracts;
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using MathNet.Numerics.Distributions;
using NodaTime;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Projection;
using Rasm.Fabrication.Process;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
// Contracts are retired from this logic.
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Spec;

// --- [VOCABULARIES] --------------------------------------------------------------------
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
public sealed partial class ToleranceZoneKind {
    public static readonly ToleranceZoneKind Bilateral = Plain("bilateral", string.Empty);
    public static readonly ToleranceZoneKind Unilateral = new("unilateral", string.Empty, projects: false,
        static (second, _) => second.Exists(double.IsFinite));
    public static readonly ToleranceZoneKind Diameter = Plain("diameter", Diametral);
    public static readonly ToleranceZoneKind Spherical = Plain("spherical", "S" + Diametral);
    public static readonly ToleranceZoneKind Profile = Plain("profile", string.Empty);
    public static readonly ToleranceZoneKind Projected = new("projected", "Ⓟ", projects: true,
        static (second, _) => second.Exists(static height => double.IsFinite(height) && height > 0.0));
    public static readonly ToleranceZoneKind UnequallyDisposed = new("unequally-disposed", "UZ", projects: false,
        static (second, widthMm) => second.Exists(offset =>
            double.IsFinite(offset) && offset >= 0.0 && offset <= widthMm));

    private static string Diametral => ZoneModifier.Diametral.Glyph.ToString(CultureInfo.InvariantCulture);

    private static ToleranceZoneKind Plain(string key, string prefix) =>
        new(key, prefix, projects: false, static (second, _) => second.IsNone);

    public string Prefix { get; }

    public bool Projects { get; }

    [UseDelegateFromConstructor]
    public partial bool AdmitsSecond(Option<double> secondMm, double widthMm);
}

[SmartEnum<string>]
public sealed partial class FeatureCharacteristic {
    public static readonly FeatureCharacteristic Straightness = Row(GeometricCharacteristic.Straightness, FeatureClass.Form,
        static scope => scope == FeatureScope.Surface || scope == FeatureScope.Axis || scope == FeatureScope.MedianLine,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianLine,
        static zone => zone == ToleranceZoneKind.Bilateral || zone == ToleranceZoneKind.Diameter);
    public static readonly FeatureCharacteristic Flatness = Row(GeometricCharacteristic.Flatness, FeatureClass.Form,
        static scope => scope == FeatureScope.Surface || scope == FeatureScope.MedianPlane,
        static scope => scope == FeatureScope.MedianPlane,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic Circularity = Surface(GeometricCharacteristic.Circularity, FeatureClass.Form,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic Cylindricity = Surface(GeometricCharacteristic.Cylindricity, FeatureClass.Form,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic ProfileLine = Profile(GeometricCharacteristic.ProfileLine);
    public static readonly FeatureCharacteristic ProfileSurface = Profile(GeometricCharacteristic.ProfileSurface);
    public static readonly FeatureCharacteristic Parallelism = Orientation(GeometricCharacteristic.Parallelism);
    public static readonly FeatureCharacteristic Perpendicularity = Orientation(GeometricCharacteristic.Perpendicularity);
    public static readonly FeatureCharacteristic Angularity = Orientation(GeometricCharacteristic.Angularity);
    public static readonly FeatureCharacteristic Position = Row(GeometricCharacteristic.Position, FeatureClass.Location,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane || scope == FeatureScope.CenterPoint,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane || scope == FeatureScope.CenterPoint,
        static zone => zone == ToleranceZoneKind.Diameter || zone == ToleranceZoneKind.Spherical || zone == ToleranceZoneKind.Projected);
    public static readonly FeatureCharacteristic Concentricity = Row(GeometricCharacteristic.Concentricity, FeatureClass.Location,
        static scope => scope == FeatureScope.Axis || scope == FeatureScope.CenterPoint, static _ => false,
        static zone => zone == ToleranceZoneKind.Diameter);
    public static readonly FeatureCharacteristic Symmetry = Row(GeometricCharacteristic.Symmetry, FeatureClass.Location,
        static scope => scope == FeatureScope.MedianPlane, static _ => false,
        static zone => zone == ToleranceZoneKind.Bilateral);
    public static readonly FeatureCharacteristic CircularRunout = Runout(GeometricCharacteristic.CircularRunout);
    public static readonly FeatureCharacteristic TotalRunout = Runout(GeometricCharacteristic.TotalRunout);

    private static FeatureCharacteristic Row(GeometricCharacteristic drawn, FeatureClass @class,
        Func<FeatureScope, bool> admitsScope, Func<FeatureScope, bool> admitsMaterial,
        Func<ToleranceZoneKind, bool> admitsZone) =>
        new(drawn.Key, drawn, @class, admitsScope, admitsMaterial, admitsZone);
    private static FeatureCharacteristic Surface(GeometricCharacteristic drawn, FeatureClass @class,
        Func<ToleranceZoneKind, bool> admitsZone) => Row(drawn, @class,
            static scope => scope == FeatureScope.Surface, static _ => false, admitsZone);
    private static FeatureCharacteristic Profile(GeometricCharacteristic drawn) =>
        Row(drawn, FeatureClass.Orientation,
            static scope => scope == FeatureScope.Surface, static _ => false,
            static zone => zone == ToleranceZoneKind.Profile || zone == ToleranceZoneKind.UnequallyDisposed);
    private static FeatureCharacteristic Orientation(GeometricCharacteristic drawn) =>
        Row(drawn, FeatureClass.Orientation,
            static scope => scope == FeatureScope.Surface || scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane,
            static scope => scope == FeatureScope.Axis || scope == FeatureScope.MedianPlane,
            static zone => zone == ToleranceZoneKind.Bilateral || zone == ToleranceZoneKind.Diameter);
    private static FeatureCharacteristic Runout(GeometricCharacteristic drawn) =>
        Surface(drawn, FeatureClass.Runout, static zone => zone == ToleranceZoneKind.Bilateral);

    public GeometricCharacteristic Drawn { get; }
    public FeatureClass Class { get; }
    public string Symbol => Drawn.Glyph.ToString(CultureInfo.InvariantCulture);

    public bool ProfileContextual => Drawn.Datums == DatumRegime.Optional;

    [UseDelegateFromConstructor]
    public partial bool AdmitsScope(FeatureScope scope);
    [UseDelegateFromConstructor]
    public partial bool AdmitsMaterial(FeatureScope scope);
    [UseDelegateFromConstructor]
    public partial bool AdmitsZone(ToleranceZoneKind zone);

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
    public static readonly MaterialCondition Maximum = new("mmc", Glyph(ZoneModifier.Maximum),
        static (widthMm, departureMm) => widthMm + departureMm,
        static (size, widthMm) => Some((
            size.Geometry.Boundary(size.MaximumMaterialMm, widthMm),
            size.Geometry.Boundary(size.LeastMaterialMm, -(widthMm + size.RangeMm)))));
    public static readonly MaterialCondition Least = new("lmc", Glyph(ZoneModifier.Least),
        static (widthMm, departureMm) => widthMm + departureMm,
        static (size, widthMm) => Some((
            size.Geometry.Boundary(size.LeastMaterialMm, -widthMm),
            size.Geometry.Boundary(size.MaximumMaterialMm, widthMm + size.RangeMm))));

    private static string Glyph(ZoneModifier drawn) => drawn.Glyph.ToString(CultureInfo.InvariantCulture);

    public string Symbol { get; }

    [UseDelegateFromConstructor]
    public partial double Effective(double widthMm, double departureMm);
    [UseDelegateFromConstructor]
    public partial Option<(double VirtualMm, double ResultantMm)> Boundaries(FeatureSize size, double widthMm);
}

[SmartEnum<string>]
public sealed partial class FrameModifier {
    public static readonly FrameModifier TangentPlane = new("tangent-plane", "Ⓣ",
        static (characteristic, scope) => scope == FeatureScope.Surface
            && (characteristic.Class == FeatureClass.Orientation || characteristic.Class == FeatureClass.Form));
    public static readonly FrameModifier FreeState = Anywhere("free-state",
        ZoneModifier.FreeState.Glyph.ToString(CultureInfo.InvariantCulture));
    public static readonly FrameModifier Statistical = Anywhere("statistical", "〈ST〉");
    public static readonly FrameModifier CommonZone = Associated("common-zone", "CZ");
    public static readonly FrameModifier ContinuousFeature = new("continuous-feature", "〈CF〉",
        static (_, scope) => scope == FeatureScope.Surface);
    public static readonly FrameModifier AllAround = Profiled("all-around", "○");
    public static readonly FrameModifier AllOver = Profiled("all-over", "◎");
    public static readonly FrameModifier Envelope = Sized("envelope", "Ⓔ");
    public static readonly FrameModifier Independency = Sized("independency", "Ⓘ");
    public static readonly FrameModifier Reciprocity = Sized("reciprocity", "Ⓡ");
    public static readonly FrameModifier MinimumCircumscribed = Associated("minimum-circumscribed", "Ⓒ");
    public static readonly FrameModifier MaximumInscribed = Associated("maximum-inscribed", "Ⓧ");
    public static readonly FrameModifier LeastSquares = Associated("least-squares", "Ⓖ");
    public static readonly FrameModifier MinimaxTangent = Associated("minimax-tangent", "Ⓝ");

    private static FrameModifier Anywhere(string key, string symbol) => new(symbol, static (_, _) => true);
    private static FrameModifier Associated(string key, string symbol) => new(symbol,
        static (characteristic, _) => characteristic.Class != FeatureClass.Runout);
    private static FrameModifier Profiled(string key, string symbol) => new(symbol,
        static (characteristic, _) => characteristic.ProfileContextual);
    private static FrameModifier Sized(string key, string symbol) => new(symbol,
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

- Owner: `FeatureControl` owns the admitted feature-control frame; `DatumSystem` owns precedence-ordered datum references over the kernel `DatumDesignator`; `FrameExtension` owns basics, targets, and the composite lower segment; `FeatureFrame` owns the settled projection.
- Cases: `ToleranceZone` is ONE shape — kind, width, an optional second magnitude, and modifiers — because the kind row already decides whether a second magnitude exists, what it must hold, and whether it is a projection height or a disposition offset. The three payload cases could each be constructed against a kind that contradicted them, so the pairing is now an admission fact rather than a post-construction verdict.
- Law: ISO 1101 legality is NINE independent questions. `FeatureControl.Legal` is the one predicate both the accumulating admission and the generated hook read, so a direct `Create` can never seat a frame the admission would refuse and the two can never state different law; `FeatureControl.Admit` accumulates through `AdmissionSlots`, so a caller repairing a datum count learns in the same verdict that its zone kind, its material condition, and its publishing standard are also inadmissible.
- Law: the publishing standard is a FRAME column because the same characteristic is legal on one standard and unspellable on another — ASME Y14.5-2018 removed concentricity and symmetry, whose callouts respell as position or profile — and `SymbolSet.For(standard)` is the kernel row that answers it. The column does NOT cross the wire: glyph and presentation belong to the CONSUMING standard, so a producer that stated its own would be publishing a decision the reader already owns.
- Law: `FeatureFrame.Annotation` is the ONE annotation surface and it is a LAYOUT-FREE row stream — compartment, symbol, ordinal — never a concatenated glyph run a drawing consumer would have to re-parse into the structure this owner already holds. Placement, size, and font stay the drawing plane's, and a second joined-glyph projection on a datum owner is the deleted form.
- Growth: a frame axis is one column on `FrameExtension`; a compartment is one `FrameCompartment` row.
- Boundary: an achievable width enters as input-carried capability evidence and never as a reach into `Spec/capability`.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct ZoneWidth {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0.0 ? null : ToleranceSpec.Validation("tolerance-zone-width");
}

[ComplexValueObject]
public sealed partial class ToleranceZone {
    public ToleranceZoneKind Kind { get; }
    public ZoneWidth Width { get; }

    public Option<double> SecondMm { get; }
    public Set<FrameModifier> Modifiers { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ToleranceZoneKind kind,
        ref ZoneWidth width, ref Option<double> secondMm, ref Set<FrameModifier> modifiers) =>
        validationError = Legal(kind, width, secondMm) ? null : ToleranceSpec.Validation("tolerance-zone");

    internal static bool Legal(ToleranceZoneKind kind, ZoneWidth width, Option<double> secondMm) =>
        double.IsFinite(width.ToValue()) && width.ToValue() > 0.0
        && kind.AdmitsSecond(secondMm, width.ToValue());

    public Option<double> ProjectedHeightMm => Kind.Projects ? SecondMm : None;
    public Option<double> UnequalOffsetMm => Kind.Projects ? None : SecondMm;
}

[ComplexValueObject]
public sealed partial class DatumReference {
    public DatumDesignator Label { get; }
    public DatumPrecedence Precedence { get; }
    public MaterialCondition Material { get; }
}

[ComplexValueObject]
public sealed partial class DatumPoint {
    public double XMm { get; }
    public double YMm { get; }
    public double ZMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double xMm, ref double yMm, ref double zMm) =>
        validationError = double.IsFinite(xMm) && double.IsFinite(yMm) && double.IsFinite(zMm)
            ? null : ToleranceSpec.Validation("datum-point");
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

    public Option<DatumDesignator> Primary => References.HeadOrNone().Map(static row => row.Label);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<DatumReference> references) {
        references = toSeq(references.OrderBy(static row => row.Precedence.Order)).ToArr();
        validationError = references.Count <= 3
            && toSeq(references).Map(static row => row.Label).Distinct().Count == references.Count
            && toSeq(references).Map(static row => row.Precedence).Distinct().Count == references.Count
            && references.ForAll(row => row.Precedence.Order <= references.Count)
            ? null : ToleranceSpec.Validation("datum-system");
    }
}

[ComplexValueObject]
public sealed partial class BasicDimension {
    public string Label { get; }
    public double NominalMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string label, ref double nominalMm) {
        label = label?.Trim() ?? string.Empty;
        validationError = label.Length > 0 && double.IsFinite(nominalMm) ? null : ToleranceSpec.Validation("basic-dimension");
    }
}

[ComplexValueObject]
public sealed partial class CompositeSegment {
    public ZoneWidth Width { get; }
    public Set<FrameModifier> Modifiers { get; }
    public DatumSystem Datums { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ZoneWidth width,
        ref Set<FrameModifier> modifiers, ref DatumSystem datums) =>
        validationError = double.IsFinite(width.ToValue()) && width.ToValue() > 0.0
            && datums.References.Count > 0 ? null : ToleranceSpec.Validation("composite-segment");
}

[ComplexValueObject]
public sealed partial class FrameExtension {
    public Arr<BasicDimension> Basics { get; }
    public Arr<DatumTarget> Targets { get; }
    public Option<CompositeSegment> Composite { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<BasicDimension> basics,
        ref Arr<DatumTarget> targets, ref Option<CompositeSegment> composite) =>
        validationError = toSeq(basics).Map(static row => row.Label).Distinct().Count == basics.Count
            && toSeq(targets).Map(static row => row.Label).Distinct().Count == targets.Count
            && targets.ForAll(ValidTarget)
                ? null : ToleranceSpec.Validation("frame-extension");

    private static bool ValidTarget(DatumTarget target) => target.Switch(
        point: static row => !string.IsNullOrWhiteSpace(row.Label),
        line: static row => !string.IsNullOrWhiteSpace(row.Label)
            && double.IsFinite(row.LengthMm) && row.LengthMm > 0.0,
        area: static row => !string.IsNullOrWhiteSpace(row.Label)
            && double.IsFinite(row.LengthMm) && row.LengthMm > 0.0
            && double.IsFinite(row.WidthMm) && row.WidthMm > 0.0);

    public bool Anchored(DatumSystem datums) =>
        Targets.ForAll(target => datums.References.Exists(row =>
            target.Label.StartsWith(row.Label.Text, StringComparison.Ordinal)))
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
        validationError = double.IsFinite(lowerMm) && lowerMm > 0.0
            && double.IsFinite(upperMm) && upperMm >= lowerMm ? null : ToleranceSpec.Validation("feature-size");

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

    public SheetStandard Standard { get; }
    public FeatureClass Class => Characteristic.EffectiveClass(Datums.References.Count);

    internal static bool Legal(
        FeatureCharacteristic characteristic,
        FeatureScope scope,
        ToleranceZone zone,
        DatumSystem datums,
        MaterialCondition material,
        FrameExtension extension,
        Option<FeatureSize> size,
        Option<double> achievableMm,
        SheetStandard standard) =>
        ToleranceZone.Legal(zone.Kind, zone.Width, zone.SecondMm)
        && characteristic.Drawn.Datums.Admits(datums.Primary)
        && characteristic.AdmitsScope(scope)
        && characteristic.AdmitsZone(zone.Kind)
        && zone.Modifiers.ForAll(modifier => modifier.Admits(characteristic, scope))
        && (material == MaterialCondition.Regardless || (characteristic.AdmitsMaterial(scope) && size.IsSome))
        && extension.Anchored(datums)
        && achievableMm.ForAll(static value => double.IsFinite(value) && value > 0.0)
        && SymbolSet.For(standard).Admits(characteristic.Drawn);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref CharacteristicId id, ref ContentKey source,
        ref FeatureCharacteristic characteristic, ref FeatureScope scope, ref ToleranceZone zone, ref DatumSystem datums,
        ref MaterialCondition material, ref FrameExtension extension, ref Option<FeatureSize> size,
        ref Option<double> achievableMm, ref SheetStandard standard) {
        if (!Legal(characteristic, scope, zone, datums, material, extension, size, achievableMm, standard))
            validationError = ToleranceSpec.Validation("feature-control");
    }

    public static Fin<FeatureControl> Admit(ToleranceRequest.Feature raw) =>
        from _clauses in (
            AdmissionSlots.Gate(ToleranceZone.Legal(raw.Zone.Kind, raw.Zone.Width, raw.Zone.SecondMm), FabConcern.Spec, "tolerance:feature-control:zone-payload", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(raw.Characteristic.Drawn.Datums.Admits(raw.Datums.Primary), FabConcern.Spec, "tolerance:feature-control:datum-regime", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(raw.Characteristic.AdmitsScope(raw.Scope), FabConcern.Spec, "tolerance:feature-control:scope", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(raw.Characteristic.AdmitsZone(raw.Zone.Kind), FabConcern.Spec, "tolerance:feature-control:zone-kind", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(raw.Zone.Modifiers.ForAll(modifier => modifier.Admits(raw.Characteristic, raw.Scope)), FabConcern.Spec, "tolerance:feature-control:modifier", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(raw.Material == MaterialCondition.Regardless
                || (raw.Characteristic.AdmitsMaterial(raw.Scope) && raw.Size.IsSome), FabConcern.Spec, "tolerance:feature-control:material-condition", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(raw.Extension.Anchored(raw.Datums), FabConcern.Spec, "tolerance:feature-control:extension-anchor", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(raw.AchievableMm.ForAll(static value => double.IsFinite(value) && value > 0.0), FabConcern.Spec, "tolerance:feature-control:achievable", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(SymbolSet.For(raw.Standard).Admits(raw.Characteristic.Drawn), FabConcern.Spec, "tolerance:feature-control:standard-symbol", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _, _, _, _, _, _) => unit)
            .As()
            .ToFin()
        from admitted in Validate(raw.Id, raw.Source, raw.Characteristic, raw.Scope, raw.Zone, raw.Datums, raw.Material,
            raw.Extension, raw.Size, raw.AchievableMm, raw.Standard, out FeatureControl value).Admitted(value)
        select admitted;
}

[ComplexValueObject]
public sealed partial class FeatureFrame {
    public CharacteristicId Id => Control.Id;
    public FeatureControl Control { get; }
    public ToleranceSpec.Geometric Specification => new(Control);
    public QifKind Qif => Specification.Qif();
    public FeatureCharacteristic Characteristic => Control.Characteristic;
    public FeatureScope Scope => Control.Scope;
    public ToleranceZoneKind Kind => Control.Zone.Kind;
    public double WidthMm => Control.Zone.Width.ToValue();
    public Arr<FrameModifier> Modifiers => toSeq(Control.Zone.Modifiers
        .OrderBy(static modifier => modifier.Key, StringComparer.Ordinal)).ToArr();
    public Arr<DatumReference> Datums => Control.Datums.References;
    public MaterialCondition Material => Control.Material;
    public Option<FeatureSize> Size => Control.Size;
    public Option<double> ProjectedHeightMm => Control.Zone.ProjectedHeightMm;
    public Option<double> UnequalOffsetMm => Control.Zone.UnequalOffsetMm;
    public FrameExtension Extension => Control.Extension;
    public Option<double> AchievableMm => Control.AchievableMm;
    public Seq<FrameSymbolRow> Annotation =>
        Seq(new FrameSymbolRow(FrameCompartment.Characteristic, Control.Characteristic.Symbol, 0))
        + Seq(new FrameSymbolRow(
            FrameCompartment.Zone,
            string.Concat(Control.Zone.Kind.Prefix, WidthMm.ToString(CultureInfo.InvariantCulture)),
            0))
        + (Control.Material == MaterialCondition.Regardless
            ? Seq<FrameSymbolRow>()
            : Seq(new FrameSymbolRow(FrameCompartment.Material, Control.Material.Symbol, 0)))
        + toSeq(Modifiers).Map(static (modifier, index) => new FrameSymbolRow(FrameCompartment.Modifier, modifier.Symbol, index))
        + toSeq(Datums).Map(static (datum, index) => new FrameSymbolRow(FrameCompartment.Datum, datum.Label.Text, index));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FeatureControl control) =>
        validationError = control is null ? ToleranceSpec.Validation("feature-frame") : null;
}

[SmartEnum<string>]
public sealed partial class FrameCompartment {
    public static readonly FrameCompartment Characteristic = new("characteristic");
    public static readonly FrameCompartment Zone = new("zone");
    public static readonly FrameCompartment Material = new("material");
    public static readonly FrameCompartment Modifier = new("modifier");
    public static readonly FrameCompartment Datum = new("datum");
}

public readonly record struct FrameSymbolRow(FrameCompartment Compartment, string Symbol, int Ordinal);

[ValueObject<UInt128>]
public readonly partial struct CharacteristicId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref UInt128 value) =>
        validationError = value == UInt128.Zero ? ToleranceSpec.Validation("tolerance:characteristic-id") : null;
}
```

## [04]-[FIT_ALGEBRA]

- Owner: `FitLetter` owns the fundamental-deviation closed form per letter, `ItGradeName` the standard tolerance grade formulas, `DiameterBand` the reference diameter every one of them evaluates at, and `FitStandard` the generative resolution both feed; `GeneralStandard` owns the ISO 2768 seed roster.
- Law: ISO 286 is ALGEBRA here, not a transcribed grid. `ItGradeName` generates `IT01` through `IT18` over the standard tolerance unit, `DiameterBand.ReferenceMm` derives the geometric mean, and the hole derives as the shaft's mirror under the general rule with its correction — so a revision widening the band roster costs one row and a revision changing a formula costs one delegate.
- Law: `FitException` holds ONLY what the standard publishes outside its own formulas — shaft j and hole J, the p step, k outside grades 4 through 7 — so a row duplicating what `FitLetter` derives is the deleted form, because the two would then disagree silently, and a TABULATED letter with no exception refuses rather than returning the zero its unused delegate would hand back.
- Auto: `FitCharacter.Of` is the ONE pairing law, so the admitting fold and the result's own proof cannot disagree on whether a pair clears, transitions, or interferes.
- Growth: a fit letter, IT grade, diameter band, or general-tolerance class is one row; a tabular standard revision is seed data under the existing admission proof.
- Boundary: deviations are published in micrometres and sizes read in millimetres, so the conversion rides the quantity owner at the one derivation site rather than a bare divisor per call.

```csharp
// --- [VOCABULARIES] --------------------------------------------------------------------
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

    public static FitCharacter Of(double minimumMm, double maximumMm) =>
        minimumMm >= 0.0 ? Clearance : maximumMm <= 0.0 ? Interference : Transition;
}

[SmartEnum<string>]
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

    public bool Tabulates { get; }

    [UseDelegateFromConstructor]
    public partial double ShaftMicrometers(double geometricMeanMm, ItSeries series);

    private static FitLetter Upper(string key, Func<double, ItSeries, double> shaft) =>
        new(key, FitBound.Upper, tabulates: false, shaft);

    private static FitLetter Lower(string key, Func<double, ItSeries, double> shaft) =>
        new(key, FitBound.Lower, tabulates: false, shaft);

    private static double Blend(string first, string second, double meanMm, ItSeries series) =>
        Math.Sqrt(Math.Abs(Get(first).ShaftMicrometers(meanMm, series))
            * Math.Abs(Get(second).ShaftMicrometers(meanMm, series)));
}

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

    private static ItGradeName Interpolated(string key, int grade) => new(grade,
        diameterMm => InterpolatedMicrometers(grade, diameterMm));
    private static ItGradeName Multiple(string key, int grade, double factor) => new(grade, d => Rounded(factor * Unit(d)));
    private static double InterpolatedMicrometers(int grade, double diameterMm) =>
        Rounded((0.8 + (0.020 * diameterMm))
            * Math.Pow(7.0 * Unit(diameterMm) / (0.8 + (0.020 * diameterMm)), (grade - 1) / 4.0));
    private static double Unit(double diameterMm) => (0.45 * Math.Cbrt(diameterMm)) + (0.001 * diameterMm);
    private static double Rounded(double micrometers) => Math.Round(micrometers, micrometers < 2.0 ? 1 : 0,
        MidpointRounding.AwayFromZero);

    public int Number { get; }

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

    private static GeneralToleranceKind Measured(string key) => new(static limit => limit is GeneralLimit.Linear);

    [UseDelegateFromConstructor]
    public partial bool Admits(GeneralLimit limit);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct FinishingAllowanceFactor {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value >= 0.0
            ? null : ToleranceSpec.Validation("finishing-allowance-factor");
}

[ComplexValueObject]
public sealed partial class DiameterBand {
    public double LowerMm { get; }
    public double UpperMm { get; }

    public double ReferenceMm => Math.Sqrt(Math.Max(LowerMm, 1.0) * UpperMm);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double lowerMm, ref double upperMm) =>
        validationError = double.IsFinite(lowerMm) && lowerMm >= 0.0 && double.IsFinite(upperMm) && upperMm > lowerMm
            ? null : ToleranceSpec.Validation("diameter-band");

    public bool Contains(double diameterMm) => diameterMm > LowerMm && diameterMm <= UpperMm;
}

[ComplexValueObject]
public sealed partial class ItGrade {
    public ItGradeName Name { get; }
    public DiameterBand Diameter { get; }
    public FinishingAllowanceFactor AllowanceFactor { get; }
    public int Number => Name.Number;
    public double ToleranceMicrometers => Name.Micrometers(Diameter.ReferenceMm);
    public double ToleranceMillimeters => Length.FromMicrometers(ToleranceMicrometers).Millimeters;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ItGradeName name,
        ref DiameterBand diameter, ref FinishingAllowanceFactor allowanceFactor) =>
        validationError = double.IsFinite(allowanceFactor.ToValue())
            && allowanceFactor.ToValue() >= 0.0 && double.IsFinite(name.Micrometers(diameter.ReferenceMm))
            && name.Micrometers(diameter.ReferenceMm) > 0.0 ? null : ToleranceSpec.Validation("it-grade");
}

public readonly record struct FitException(
    FitMember Member,
    FitLetter Letter,
    DiameterBand Diameter,
    Option<ItGradeName> Grade,
    FitBound Bound,
    double FundamentalMicrometers);

[ComplexValueObject]
public sealed partial class FitStandard {
    public Arr<DiameterBand> Diameters { get; }
    public Arr<FitException> Exceptions { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<DiameterBand> diameters,
        ref Arr<FitException> exceptions) =>
        validationError = diameters.Count > 0
            && exceptions.ForAll(static row => double.IsFinite(row.FundamentalMicrometers))
            && exceptions.ForAll(row => diameters.Exists(candidate => candidate == row.Diameter))
            && exceptions.Map(static row => (row.Member, row.Letter, row.Diameter, row.Grade)).Distinct().Count == exceptions.Count
                ? null : ToleranceSpec.Validation("fit-standard");

    public Fin<(FitBound Bound, double FundamentalMicrometers)> Resolve(
        FitMember member, FitLetter letter, ItGradeName grade, DiameterBand diameter) =>
        toSeq(Exceptions)
            .Filter(row => row.Member == member && row.Letter == letter && row.Diameter == diameter)
            .Fold(Option<FitException>.None, (held, row) =>
                row.Grade.Exists(candidate => candidate == grade) ? Some(row)
                    : held.IsSome || row.Grade.IsSome ? held : Some(row))
            .Map(static row => Fin.Succ((row.Bound, row.FundamentalMicrometers)))
            .IfNone(() => letter.Tabulates
                ? Fin.Fail<(FitBound, double)>(ToleranceSpec.Invalid("fit-standard",
                    $"a tabulated exception for {member.ToValue()}{letter.ToValue()}{grade.ToValue()}"))
                : Fin.Succ(Derived(member, letter, grade, diameter)));

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
public sealed partial class FitClass {
    public FitMember Member { get; }
    public FitLetter Letter { get; }
    public ItGrade Grade { get; }
    public FitBound FundamentalBound { get; }
    public double FundamentalMicrometers { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FitMember member,
        ref FitLetter letter, ref ItGrade grade, ref FitBound fundamentalBound, ref double fundamentalMicrometers) =>
        validationError = double.IsFinite(fundamentalMicrometers) ? null : ToleranceSpec.Validation("fit-class");

    public (double LowerUm, double UpperUm) Limits =>
        FundamentalBound.Deviations(FundamentalMicrometers, Grade.ToleranceMicrometers);

    public string Designation => Member == FitMember.Hole
        ? string.Concat(Letter.ToValue().ToUpperInvariant(), Grade.Number.ToString(CultureInfo.InvariantCulture))
        : string.Concat(Letter.ToValue(), Grade.Number.ToString(CultureInfo.InvariantCulture));

    public (double LowerMm, double UpperMm) Sizes(double nominalMm) =>
        (nominalMm + Length.FromMicrometers(Limits.LowerUm).Millimeters,
         nominalMm + Length.FromMicrometers(Limits.UpperUm).Millimeters);

    public static Fin<FitClass> Admit(FitMember member, FitLetter letter, ItGrade grade, FitStandard standard) =>
        from seed in standard.Resolve(member, letter, grade.Name, grade.Diameter)
        select Create(member, letter, grade, seed.Bound, seed.FundamentalMicrometers);
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
    public string Designation => string.Concat(ZoneModifier.Diametral.Glyph,
        NominalMm.ToString(CultureInfo.InvariantCulture), " ", Hole.Designation, "/", Shaft.Designation);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ContentKey source,
        ref double nominalMm, ref FitClass hole, ref FitClass shaft, ref FitCharacter character) {
        if (source is null || hole is null || shaft is null || character is null) {
            validationError = ToleranceSpec.Validation("fit-limits");
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
            && character == derived ? null : ToleranceSpec.Validation("fit-limits");
    }
}

[Union]
public abstract partial record GeneralLimit : IValidityEvidence {
    private GeneralLimit() { }
    public sealed record Linear(double Millimeters) : GeneralLimit;
    public sealed record Angular(double Degrees) : GeneralLimit;

    public bool IsValid => Switch(
        linear: static row => ValidityClaim.Positive(row.Millimeters),
        angular: static row => ValidityClaim.Positive(row.Degrees));
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
        validationError = seeds.Count > 0
            && seeds.ForAll(static row => row.Limit.IsValid && row.Kind.Admits(row.Limit))
            && seeds.Map(static row => (row.Class, row.Kind, row.Band)).Distinct().Count == seeds.Count
            && !seeds.Exists(left => seeds.Exists(right => left != right && left.Overlaps(right)))
                ? null : ToleranceSpec.Validation("general-standard");

    public Fin<GeneralLimit> Resolve(GeneralToleranceClass @class, GeneralToleranceKind kind, double nominalMm) =>
        Seeds.Filter(row => row.Class == @class && row.Kind == kind && row.Band.Contains(nominalMm))
            .Map(static row => row.Limit).Head.ToFin(ToleranceSpec.Invalid("general-standard:band"));
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
        validationError = double.IsFinite(nominalMm)
            && nominalMm > 0.0 && limit.IsValid
            && kind.Admits(limit)
                ? null : ToleranceSpec.Validation("general-tolerance");
}
```

## [05]-[SURFACE_TEXTURE]

- Owner: `SurfaceParameter` owns the ISO 21920 parameter roster and its Ra correspondence; `SurfaceMeasure` owns BOTH halves of what a limit means — the band it must hold and the unit it is read in; `SurfaceTexture` owns the admitted requirement set and `RaTarget` its scallop projection.
- Cases: `SurfaceLimit` closes exact, maximum, minimum, and ranged acceptance; `SurfaceRequirement` is ONE shape whose measure decides which optional column it demands, so six cases mirroring six measure rows — where a parameter and a case could disagree — collapse to one.
- Boundary: roughness correspondence is not a strategy — `SurfaceParameter.RaRatio` is the declared datum, and a parameter without one refuses rather than inferring a ratio.
- Growth: a surface parameter is one row naming its profile and its measure; a measure is one row carrying its unit and its admitted band.

```csharp
// --- [VOCABULARIES] --------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SurfaceProfile {
    public static readonly SurfaceProfile Roughness = new("roughness");
    public static readonly SurfaceProfile Waviness = new("waviness");
    public static readonly SurfaceProfile Primary = new("primary");
}

[SmartEnum<string>]
public sealed partial class SurfaceMeasure {
    public static readonly SurfaceMeasure Amplitude = Positive("amplitude", LengthUnit.Micrometer);
    public static readonly SurfaceMeasure Spacing = Positive("spacing", LengthUnit.Millimeter);
    public static readonly SurfaceMeasure Ratio = Percent("ratio");
    public static readonly SurfaceMeasure LevelRatio = Percent("level-ratio");
    public static readonly SurfaceMeasure Difference = Positive("difference", LengthUnit.Micrometer);
    public static readonly SurfaceMeasure Shape = new("shape", RatioUnit.DecimalFraction,
        static limit => limit.Holds(static value => double.IsFinite(value)));

    public Enum Unit { get; }

    private static SurfaceMeasure Positive(string key, Enum unit) => new(unit,
        static limit => limit.Holds(static value => double.IsFinite(value) && value > 0.0));
    private static SurfaceMeasure Percent(string key) => new(RatioUnit.Percent,
        static limit => limit.Holds(static value => double.IsFinite(value) && value is >= 0.0 and <= 100.0));

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

    internal bool Holds(Func<double, bool> admits) => Switch(
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

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct ScallopFactor {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0.0 ? null : ToleranceSpec.Validation("scallop-factor");
}

[ComplexValueObject]
public sealed partial class SurfaceRequirement {
    public SurfaceParameter Parameter { get; }
    public SurfaceLimit Limit { get; }

    public Option<double> LevelMicrometers { get; }
    public Option<(double FromPercent, double ToPercent)> MaterialBand { get; }

    public SurfaceMeasure Measure => Parameter.Measure;
    public Enum Unit => Parameter.Measure.Unit;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
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
            validationError = ToleranceSpec.Validation("surface-requirement");
    }
}

[ComplexValueObject]
public sealed partial class TransmissionBand {
    public double CutoffMm { get; }
    public double SamplingMm { get; }
    public double EvaluationMm { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double cutoffMm,
        ref double samplingMm, ref double evaluationMm) =>
        validationError = double.IsFinite(cutoffMm) && cutoffMm > 0.0 && double.IsFinite(samplingMm) && samplingMm > 0.0
            && double.IsFinite(evaluationMm) && evaluationMm >= samplingMm ? null : ToleranceSpec.Validation("transmission-band");
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
        validationError = requirements.Count > 0
            && requirements.Map(static row => row.Parameter).Distinct().Count == requirements.Count
            && machiningAllowanceMm.ForAll(static value => double.IsFinite(value) && value >= 0.0)
            && treatment.ForAll(static value => !string.IsNullOrWhiteSpace(value)) ? null : ToleranceSpec.Validation("surface-texture");
}

[ComplexValueObject]
public sealed partial class RaTarget {
    public double Micrometers { get; }
    public ScallopFactor Factor { get; }
    public double ScallopHeightMm => Length.FromMicrometers(Micrometers * Factor.ToValue()).Millimeters;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double micrometers,
        ref ScallopFactor factor) =>
        validationError = double.IsFinite(micrometers) && micrometers > 0.0 && double.IsFinite(factor.ToValue())
            && factor.ToValue() > 0.0 ? null : ToleranceSpec.Validation("ra-target");

    public static Fin<RaTarget> From(SurfaceTexture texture, SurfaceParameter source, ScallopFactor factor) =>
        from admittedTexture in Optional(texture).ToFin(ToleranceSpec.Invalid("surface-texture:raw"))
        from admittedSource in Optional(source).ToFin(ToleranceSpec.Invalid("surface-texture:source"))
        from ratio in admittedSource.RaRatio.ToFin(ToleranceSpec.Invalid("surface-texture",
            $"{admittedSource.ToValue()} declares no Ra correspondence"))
        from measured in toSeq(admittedTexture.Requirements)
            .Filter(requirement => requirement.Parameter == admittedSource
                && requirement.Measure == SurfaceMeasure.Amplitude)
            .Choose(static requirement => requirement.Limit.Upper())
            .Head.ToFin(ToleranceSpec.Invalid("surface-texture",
                $"{admittedSource.ToValue()} amplitude upper limit"))
        let micrometers = measured * ratio
        from _ in guard(double.IsFinite(micrometers) && micrometers > 0.0, ToleranceSpec.Range("surface-texture:ra",
            micrometers, "finite and positive"))
        select Create(micrometers, factor);
}
```

## [06]-[STACK_CHAIN]

- Owner: `ToleranceChain` owns the term roster, its declared method, and its bound; `StackMethod` owns the analytic combination and the contribution ranking; `ChainEvidence` is the ONE stackup result `Spec/capability`, `Spec/manufacturability`, and `Documentation/report` all read.
- Law: a term declares its own signed deviation bounds. The bound pair was a separately-constructible interval owner whose sole consumer and sole mint were this term, so the ordering invariant now rides the term's own admission and a bound pair cannot exist apart from the term it bounds.
- Law: a `ProcessDistribution` weight is the standard deviation a term contributes PER UNIT half-range, so a root-sum-square combines comparable variances and the widest-spreading distribution carries the SMALLEST weight; the row's SEEDED family is what the correlated Monte-Carlo route draws from, so a statistical stack is simulated rather than approximated by an inflation factor with no distribution behind it.
- Law: the declared method is the default reading and `Evaluate(StackMethod)` evaluates the same terms under any other, so a consumer wanting the arithmetic bound beside the statistical one reads two rows of one algebra and a second worst-case fold has no site. A term's share is its own combined magnitude under that same algebra, so the ranking never forks the law.
- Result: `ChainEvidence` carries the specification key, method, worst-case interval, combined half-range, ranked contributions, and bound; `Conforming` derives from those facts.
- Growth: a stackup method is one row carrying its combination delegate; a process distribution is one row carrying its quadrature weight and its seeded family.
- Boundary: the chain declares terms and combines them; the shared-factor loadings, systematic offsets, and measured fits a simulation needs are `Spec/capability` contributors bound to these terms by key. Evaluation is pure and reads no clock.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ProcessDistribution {
    public static readonly ProcessDistribution Normal = new("normal", 1.0 / 3.0,
        static source => MathNet.Numerics.Distributions.Normal.WithMeanStdDev(0.0, 1.0 / 3.0, source));
    public static readonly ProcessDistribution Uniform = new("uniform", 1.0 / Math.Sqrt(3.0),
        static source => new ContinuousUniform(-1.0, 1.0, source));
    public static readonly ProcessDistribution Triangular = new("triangular", 1.0 / Math.Sqrt(6.0),
        static source => new Triangular(-1.0, 1.0, 0.0, source));
    public static readonly ProcessDistribution Skewed = new("skewed", Math.Sqrt(2.0 * 4.0 / (36.0 * 7.0)) * 2.0,
        static source => new Beta(2.0, 4.0, source));

    private ProcessDistribution(string key, double quadratureWeight, Func<Random, IContinuousDistribution> seeded) : this(key) =>
        (QuadratureWeight, Seeded) = (quadratureWeight, seeded);

    public double QuadratureWeight { get; }

    public Func<Random, IContinuousDistribution> Seeded { get; }

    public double Standardize(double sample) => Switch(
        state: sample,
        normal: static (value, _) => Math.Clamp(value * 3.0, -3.0, 3.0) / 3.0,
        uniform: static (value, _) => value,
        triangular: static (value, _) => value,
        skewed: static (value, _) => (2.0 * value) - 1.0);

    public double Draw(Random source, double halfRangeMm) => Standardize(Seeded(source).Sample()) * halfRangeMm;
}

[ComplexValueObject]
[ComplexValueObject]
public sealed partial class ToleranceTerm {
    public string Key { get; }
    public double DeviationLowerMm { get; }
    public double DeviationUpperMm { get; }
    public double Sensitivity { get; }
    public ProcessDistribution Distribution { get; }
    public double LowerMm => double.Min(DeviationLowerMm * Sensitivity, DeviationUpperMm * Sensitivity);
    public double UpperMm => double.Max(DeviationLowerMm * Sensitivity, DeviationUpperMm * Sensitivity);
    public double HalfRangeMm => (UpperMm - LowerMm) * 0.5;
    public double StatisticalHalfRangeMm => HalfRangeMm * Distribution.QuadratureWeight;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string key,
        ref double deviationLowerMm, ref double deviationUpperMm, ref double sensitivity,
        ref ProcessDistribution distribution) {
        key = key?.Trim() ?? string.Empty;
        validationError = key.Length > 0 && double.IsFinite(sensitivity) && sensitivity != 0.0
            && double.IsFinite(deviationLowerMm) && double.IsFinite(deviationUpperMm)
            && deviationLowerMm <= deviationUpperMm
            ? null : ToleranceSpec.Validation("tolerance-term");
    }

    public static Fin<ToleranceTerm> Of(string key, FitClass fit, double nominalMm, double sensitivity,
        ProcessDistribution distribution) =>
        from admitted in Optional(fit).ToFin(ToleranceSpec.Invalid("tolerance-term:fit"))
        from _ in guard(double.IsFinite(nominalMm) && admitted.Grade.Diameter.Contains(nominalMm),
            ToleranceSpec.Range("tolerance-term:nominal", nominalMm, "finite and inside the fit diameter band"))
        let sizes = admitted.Sizes(nominalMm)
        from term in Validate(key, sizes.LowerMm - nominalMm, sizes.UpperMm - nominalMm, sensitivity, distribution,
            out ToleranceTerm value).Admitted(value)
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
        validationError = terms.Count > 0
            && terms.Map(static row => row.Key).Distinct().Count == terms.Count
            && double.IsFinite(boundMm) && boundMm > 0.0 ? null : ToleranceSpec.Validation("tolerance-chain");

    public ChainEvidence Evaluate() => Evaluate(Method);

    public ChainEvidence Evaluate(StackMethod method) =>
        (Rows: toSeq(Terms), Total: method.Combine(toSeq(Terms))) switch {
            var chain => new ChainEvidence(Source, method,
                chain.Rows.Fold(0.0, static (sum, term) => sum + term.LowerMm),
                chain.Rows.Fold(0.0, static (sum, term) => sum + term.UpperMm),
                chain.Total,
                toSeq(chain.Rows.Map(term => (Term: term.Key, Share: method.Share(term, chain.Total)))
                    .OrderByDescending(static row => row.Share).ThenBy(static row => row.Key)).ToArr(),
                BoundMm),
        };
}

public sealed record ChainEvidence(ContentKey Key, StackMethod Method, double WorstLowerMm, double WorstUpperMm,
    double HalfRangeMm, Arr<(string Term, double Share)> Contributions, double BoundMm) {
    public double CentreMm => (WorstLowerMm + WorstUpperMm) * 0.5;
    public bool Conforming => double.Max(Math.Abs(CentreMm - HalfRangeMm), Math.Abs(CentreMm + HalfRangeMm)) <= BoundMm;
    public Option<(string Term, double Share)> Dominant => HalfRangeMm > 0.0
        ? toSeq(Contributions).Head
        : None;

    public static CanonicalWriter Frame(ChainEvidence evidence, CanonicalWriter writer) => writer
        .Discriminant(evidence.Method)
        .Double(evidence.WorstLowerMm).Double(evidence.WorstUpperMm).Double(evidence.HalfRangeMm)
        .Rows(toSeq(evidence.Contributions), static (row, share) => row.String(share.Term).Double(share.Share))
        .Double(evidence.BoundMm);
}
```

## [07]-[OWNER_FOLD]

- Owner: `ToleranceSpec` is the canonical `[Union]` and `ToleranceSpec.Apply` the one fold; each raw case enters through one generated invariant owner and leaves through the `ToleranceResult` case its request seats.
- Law: the fold is REQUEST-INDEXED — `ISpecDemand<TResult>` seats one result case per request case, `Apply<TResult>` returns that case, and `Answer` binds arm to seat where the compiler checks it. A caller re-discriminating the answer it already asked for is the deleted form, and so is a caller-side locus for the mismatch: the only path to one is an arm that broke its own seat, which refuses at the owner under `tolerance:correspondence`.
- Cases: `ToleranceSpec` closes geometric, fit, texture, general, and chain specifications and projects `Source` and `Qif` over all five; `ToleranceRequest` adds the derivation and egress modalities — quantity, effective condition, scallop, allowance, and projection — as payload-complete cases.
- Law: `FeatureControl.Admit` is the single ISO 1101 domain-admission authority; its ephemeral `ValidationError` crosses once through `Admission.Admitted`, while the generated protobuf descriptor owns only transport-shape validation at egress.
- Law: the axis names a QUANTITY FAMILY and `QuantityInfo` is what UnitsNet gives that family as identity — a `Type` compares by CLR reflection while the parse, the unit roster, and the base dimensions all resolve off the info row, so two axes over one family stay distinct by axis while sharing one identity.
- Law: `SpecAxis.CanonicalUnit` is the CANONICAL unit an admitted quantity is stored in, which is not the unit a sheet declares it is drawn in. The kernel `DrawingUnits` row answers the second question, carries a length unit alone, and cannot express the angle, temperature, and force axes this roster admits — so a specification quantity resolves here and a plotted dimension resolves at the sheet, and the two authorities are the same three-way split the drawing owner already states.
- Boundary: `IToleranceEncoder` is the open egress strategy; format and culture state close inside its implementation, so `ToleranceRequest.Project` carries one policy value instead of delegate and provider knobs.
- Boundary: `ISpecDemand` is a SEAT, not a contract — it declares no member, because a member would move the arm bodies onto the ten cases and dissolve the one fold into ten of them. It is invariant for the same reason it is empty: covariance admits a widened answer, and the widened answer is what the seat refuses.
- Wire: the generated `FeatureControl` message is the framing authority. `FeatureControlWire` projects each admitted domain value once, evaluates the embedded `buf.validate` rules through the shared Celly validator, and serializes with `Google.Protobuf`; no field numbers, presence grammar, or byte layout are restated here.
- Wire: each closed domain vocabulary derives onto its generated enum by normalized member name and proves a bijection before the encoder becomes available; `ContentKey` crosses whole with its egress kind and 16-byte digest, so equal payload digests in different families remain distinct.
- Wire: `CorpusFrame` freezes the byte-deriving input, so the pinned vector regenerates from this page rather than from a captured buffer.
- Law: zone width crosses as its exact magnitude and a symbol never crosses at all — decimal presentation and glyph belong to the consuming drawing standard, where a producer-rounded string draws a sub-micron zone as zero.
- Boundary: frame-box facts cross while model-space geometry does not — datum targets and basic dimensions need a view transform this wire has no view to apply, so each rides the geometry boundary.

```csharp
// --- [VOCABULARIES] --------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SpecQuantity {
    public SpecAxis Axis { get; }
    public double Canonical { get; }
    public string Received { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SpecAxis axis,
        ref double canonical, ref string received) {
        received = received?.Trim() ?? string.Empty;
        validationError = double.IsFinite(canonical) && received.Length > 0
            ? null : ToleranceSpec.Validation("spec-quantity");
    }

    public static Fin<SpecQuantity> Admit(SpecAxis axis, string text) =>
        !string.IsNullOrWhiteSpace(text)
            && Quantity.TryParse(CultureInfo.InvariantCulture, axis.Quantity.ValueType, text, out IQuantity? quantity)
            && axis.Admits(quantity)
                ? Fin.Succ(Create(axis, axis.Canonical(quantity), text))
                : Fin.Fail<SpecQuantity>(ToleranceSpec.Invalid("spec-quantity",
                    $"{axis.Quantity.Name} parseable under the invariant culture"));
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface IToleranceEncoder {
    Fin<ReadOnlyMemory<byte>> Encode(ToleranceSpec value);
}

// --- [FEATURE_CONTROL_WIRE] ------------------------------------------------------------

public sealed class FeatureControlWire : IToleranceEncoder {
    static readonly Validator Rules = new(Contract.FabricationReflection.Descriptor);

    static readonly Lazy<FrozenDictionary<FeatureCharacteristic, Contract.Characteristic>> Characteristics =
        Total<FeatureCharacteristic, Contract.Characteristic>(static () => FeatureCharacteristic.Items, static row => row.Key);
    static readonly Lazy<FrozenDictionary<FeatureScope, Contract.Scope>> Scopes =
        Total<FeatureScope, Contract.Scope>(static () => FeatureScope.Items, static row => row.Key);
    static readonly Lazy<FrozenDictionary<ToleranceZoneKind, Contract.ZoneKind>> ZoneKinds =
        Total<ToleranceZoneKind, Contract.ZoneKind>(static () => ToleranceZoneKind.Items, static row => row.Key);
    static readonly Lazy<FrozenDictionary<FrameModifier, Contract.Modifier>> Modifiers =
        Total<FrameModifier, Contract.Modifier>(static () => FrameModifier.Items, static row => row.Key);
    static readonly Lazy<FrozenDictionary<EgressKind, Contract.Egress>> Egresses =
        Total<EgressKind, Contract.Egress>(static () => EgressKind.Items, static row => row.Key);
    static readonly Lazy<FrozenDictionary<MaterialCondition, Contract.Material>> Materials =
        Total<MaterialCondition, Contract.Material>(static () => MaterialCondition.Items, static row => row.Key, static row =>
            row == MaterialCondition.Regardless ? Contract.Material.Regardless
            : row == MaterialCondition.Maximum ? Contract.Material.Maximum
            : row == MaterialCondition.Least ? Contract.Material.Least
            : throw new InvalidOperationException($"<feature-control-material-unmapped:{row.Key}>"));

    public FeatureControlWire() {
        _ = Characteristics.Value;
        _ = Scopes.Value;
        _ = ZoneKinds.Value;
        _ = Modifiers.Value;
        _ = Egresses.Value;
        _ = Materials.Value;
        _ = Rules.Validate(new Contract.SourceKey());
        _ = Rules.Validate(new Contract.Datum());
        _ = Rules.Validate(new Contract.Segment());
        _ = Rules.Validate(new Contract.FeatureControl());
    }

    public static readonly ToleranceRequest.Feature CorpusFrame = new(
        CharacteristicId.Create(ContentHash.Of("tolerance-frame:corpus-a:characteristic"u8)),
        ContentKey.Of(EgressKind.QualityRecord, "tolerance-frame:corpus-a"u8),
        FeatureCharacteristic.Position,
        FeatureScope.Axis,
        ToleranceZone.Create(ToleranceZoneKind.Diameter, ZoneWidth.Create(0.25), Option<double>.None,
            Set(FrameModifier.CommonZone, FrameModifier.FreeState)),
        DatumSystem.Create(Arr(
            DatumReference.Create(Letter('A'), DatumPrecedence.Primary, MaterialCondition.Regardless),
            DatumReference.Create(Letter('B'), DatumPrecedence.Secondary, MaterialCondition.Maximum),
            DatumReference.Create(Letter('C'), DatumPrecedence.Tertiary, MaterialCondition.Regardless))),
        MaterialCondition.Maximum,
        FrameExtension.Create(Arr<BasicDimension>(), Arr<DatumTarget>(),
            Some(CompositeSegment.Create(ZoneWidth.Create(0.08), Set<FrameModifier>(),
                DatumSystem.Create(Arr(
                    DatumReference.Create(Letter('A'), DatumPrecedence.Primary, MaterialCondition.Regardless)))))),
        Some(FeatureSize.Create(FeatureGeometry.Internal, 9.9, 10.1)),
        Option<double>.None,
        SheetStandard.Iso);

    private static DatumDesignator Letter(char primary) => DatumDesignator.Create(primary, Option<char>.None);

    public Fin<ReadOnlyMemory<byte>> Encode(ToleranceSpec value) => value.Switch(
        geometric: static row => Encode(row.Value),
        fit: static _ => Refuse("fit"), texture: static _ => Refuse("texture"),
        general: static _ => Refuse("general"), chain: static _ => Refuse("chain"));

    private static Fin<ReadOnlyMemory<byte>> Refuse(string arm) => Fin.Fail<ReadOnlyMemory<byte>>(
        ToleranceSpec.Invalid($"feature-control-wire:{arm}",
            "a geometric tolerance, the one arm an ISO 1101 feature-control message spells"));

    static Fin<ReadOnlyMemory<byte>> Encode(FeatureControl control) =>
        Try.lift(() => {
            Contract.FeatureControl wire = Project(control);
            IReadOnlyList<Buf.Validate.Violation> violations = Rules.Validate(wire);
            return violations.Count == 0
                ? Fin.Succ<ReadOnlyMemory<byte>>(wire.ToByteArray())
                : Fin.Fail<ReadOnlyMemory<byte>>(ToleranceSpec.Invalid("feature-control-wire:contract",
                    $"generated FeatureControl satisfying {string.Join(',', violations.Select(static row => row.RuleId))}"));
        }).Run().Bind(static inner => inner);

    static Contract.FeatureControl Project(FeatureControl control) {
        Contract.FeatureControl wire = new() {
            Id = ContentHash.Wire(control.Id.ToValue()),
            Source = new Contract.SourceKey {
                Kind = Egresses.Value[control.Source.Kind],
                Digest = ContentHash.Wire(control.Source.Digest),
            },
            Characteristic = Characteristics.Value[control.Characteristic],
            Scope = Scopes.Value[control.Scope],
            ZoneKind = ZoneKinds.Value[control.Zone.Kind],
            WidthMm = control.Zone.Width.ToValue(),
            Material = Materials.Value[control.Material],
        };
        control.Zone.SecondMm.Iter(value => wire.SecondMm = value);
        wire.Modifiers.Add(control.Zone.Modifiers
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .Select(row => Modifiers.Value[row]));
        wire.Datums.Add(control.Datums.References.Map(Datum));
        control.Extension.Composite.Iter(segment => wire.Composite = Segment(segment));
        return wire;
    }

    static Contract.Datum Datum(DatumReference datum) => new() {
        Label = datum.Label.Text,
        Material = Materials.Value[datum.Material],
    };

    static Contract.Segment Segment(CompositeSegment segment) {
        Contract.Segment wire = new() { WidthMm = segment.Width.ToValue() };
        wire.Modifiers.Add(segment.Modifiers
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .Select(row => Modifiers.Value[row]));
        wire.Datums.Add(segment.Datums.References.Map(Datum));
        return wire;
    }

    static Lazy<FrozenDictionary<TRow, TEnum>> Total<TRow, TEnum>(
        Func<IReadOnlyList<TRow>> rows, Func<TRow, string> key)
        where TRow : notnull where TEnum : struct, Enum =>
        Total(rows, row => Lift<TEnum>(key(row)));

    static Lazy<FrozenDictionary<TRow, TEnum>> Total<TRow, TEnum>(
        Func<IReadOnlyList<TRow>> rows, Func<TRow, string> key, Func<TRow, TEnum> lift)
        where TRow : notnull where TEnum : struct, Enum => new(() => {
            IReadOnlyList<TRow> domain = rows();
            (TRow Row, string Key, TEnum Value)[] mapped = domain
                .Select(row => (Row: row, Key: key(row), Value: lift(row)))
                .ToArray();
            FrozenSet<TEnum> generated = Enum.GetValues<TEnum>()
                .Where(static value => !EqualityComparer<TEnum>.Default.Equals(value, default))
                .ToFrozenSet();
            bool uniqueKeys = mapped.Select(static row => row.Key)
                .Distinct(StringComparer.Ordinal).Count() == mapped.Length;
            FrozenSet<TEnum> projected = mapped.Select(static row => row.Value).ToFrozenSet();
            if (!uniqueKeys || projected.Count != mapped.Length || !projected.SetEquals(generated))
                throw new InvalidOperationException($"<feature-control-vocabulary-not-bijective:{typeof(TRow).Name}:{typeof(TEnum).Name}>");
            return mapped.ToFrozenDictionary(static row => row.Row, static row => row.Value);
        });

    static TEnum Lift<TEnum>(string key) where TEnum : struct, Enum =>
        Enum.TryParse(key.Replace("-", string.Empty), ignoreCase: true, out TEnum value)
        && !EqualityComparer<TEnum>.Default.Equals(value, default)
            ? value
            : throw new InvalidOperationException($"<feature-control-vocabulary-unmapped:{typeof(TEnum).Name}:{key}>");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union]
public abstract partial record ToleranceSpec {
    private ToleranceSpec() { }
    public sealed record Geometric(FeatureControl Value) : ToleranceSpec;
    public sealed record Fit(FitLimits Value) : ToleranceSpec;
    public sealed record Texture(SurfaceTexture Value) : ToleranceSpec;
    public sealed record General(GeneralTolerance Value) : ToleranceSpec;
    public sealed record Chain(ToleranceChain Value) : ToleranceSpec;


    internal static ValidationError Validation(string locus) => new($"tolerance:{locus}");

    public ContentKey Source() => Switch(
        geometric: static row => row.Value.Source, fit: static row => row.Value.Source,
        texture: static row => row.Value.Source, general: static row => row.Value.Source,
        chain: static row => row.Value.Source);

    public QifKind Qif() => Switch(
        geometric: static _ => QifKind.FeatureControlFrame, fit: static _ => QifKind.DimensionalTolerance,
        texture: static _ => QifKind.SurfaceTexture, general: static _ => QifKind.GeneralTolerance,
        chain: static _ => QifKind.DimensionalTolerance);

    public static Fin<TResult> Apply<TResult>(ISpecDemand<TResult> demand) where TResult : ToleranceResult =>
        from request in Optional(demand as ToleranceRequest).ToFin(Invalid("request"))
        from result in Dispatch(request)
        from answer in Optional(result as TResult).ToFin(Refusal($"correspondence:{typeof(TResult).Name}"))
        select answer;

    private static Fin<ToleranceResult> Dispatch(ToleranceRequest request) => request.Switch(
        feature: static demand => Answer(demand, Frame),
        fit: static demand => Answer(demand, Fitted),
        texture: static demand => Answer(demand, Textured),
        general: static demand => Answer(demand, Generalized),
        chain: static demand => Answer(demand, Stacked),
        quantity: static demand => Answer(demand, Measured),
        effective: static demand => Answer(demand, Effective),
        scallop: static demand => Answer(demand, Scallop),
        allowance: static demand => Answer(demand, Allowance),
        project: static demand => Answer(demand, Projected));

    private static Fin<ToleranceResult> Answer<TDemand, TResult>(TDemand demand, Func<TDemand, Fin<TResult>> arm)
        where TDemand : ToleranceRequest, ISpecDemand<TResult>
        where TResult : ToleranceResult =>
        arm(demand).Map<ToleranceResult>(static row => row);

    internal static Error Invalid(string axis, string requirement = "admitted tolerance shape") =>
        new KernelFault.InvalidValue(Label: axis, Requirement: requirement, Key: Some(SpecOp));
    internal static Error Range(string axis, double scalar, string requirement) => new KernelFault.OutOfRange(
        Label: axis, Scalar: scalar, Requirement: requirement, Key: Some(SpecOp));

    internal static FabricationFault Refusal(string locus) =>
        FabricationFault.Inadmissible(FabConcern.Spec, $"tolerance:{locus}");

    private static Fin<ToleranceResult.Frame> Frame(ToleranceRequest.Feature demand) =>
        from control in FeatureControl.Admit(demand)
        from _ in control.AchievableMm.Filter(achievable => achievable > control.Zone.Width.ToValue()).Match(
            Some: achievable => Fin.Fail<Unit>(FabricationFault.ToleranceUnsatisfiable(
                new FaultSubject.Specification(control.Source), achievable)),
            None: static () => Fin.Succ(unit))
        select new ToleranceResult.Frame(new Geometric(control), FeatureFrame.Create(control));

    private static Fin<ToleranceResult.Fitted> Fitted(ToleranceRequest.Fit demand) =>
        from _pair in guard(demand.Hole.Member == FitMember.Hole && demand.Shaft.Member == FitMember.Shaft
            && demand.Hole.Grade.Diameter == demand.Shaft.Grade.Diameter, Invalid("fit:pair")).ToFin()
        let maximum = Length.FromMicrometers(demand.Hole.Limits.UpperUm - demand.Shaft.Limits.LowerUm).Millimeters
        let minimum = Length.FromMicrometers(demand.Hole.Limits.LowerUm - demand.Shaft.Limits.UpperUm).Millimeters
        from _limits in guard(double.IsFinite(maximum) && double.IsFinite(minimum) && maximum >= minimum,
            Invalid("fit:limits"))
        let character = FitCharacter.Of(minimum, maximum)
        from limits in FitLimits.Validate(demand.Source, demand.NominalMm, demand.Hole, demand.Shaft,
            character, out FitLimits value).Admitted(value)
        select new ToleranceResult.Fitted(new Fit(limits));

    private static Fin<ToleranceResult.Textured> Textured(ToleranceRequest.Texture demand) =>
        from texture in SurfaceTexture.Validate(demand.Source, demand.Requirements, demand.Lay,
            demand.Mark, demand.Band, demand.MachiningAllowanceMm, demand.Treatment,
            out SurfaceTexture value).Admitted(value)
        select new ToleranceResult.Textured(new Texture(texture));

    private static Fin<ToleranceResult.Generalized> Generalized(ToleranceRequest.General demand) =>
        from standard in Optional(demand.Standard).ToFin(Invalid("general-tolerance:standard"))
        from limit in standard.Resolve(demand.Class, demand.Kind, demand.NominalMm)
        from value in GeneralTolerance.Validate(demand.Source, demand.Class, demand.Kind,
            demand.NominalMm, limit, out GeneralTolerance tolerance).Admitted(tolerance)
        select new ToleranceResult.Generalized(new General(value));

    private static Fin<ToleranceResult.Stacked> Stacked(ToleranceRequest.Chain demand) =>
        from chain in ToleranceChain.Validate(demand.Source, demand.Terms, demand.BoundMm,
            demand.Method, out ToleranceChain value).Admitted(value)
        select new ToleranceResult.Stacked(new Chain(chain), chain.Evaluate());

    private static Fin<ToleranceResult.Quantity> Measured(ToleranceRequest.Quantity demand) =>
        SpecQuantity.Admit(demand.Axis, demand.Text).Map(static value => new ToleranceResult.Quantity(value));

    private static Fin<ToleranceResult.Effective> Effective(ToleranceRequest.Effective demand) =>
        from admitted in Optional(demand.Control).ToFin(Invalid("effective:control"))
        from _1 in guard(double.IsFinite(demand.DepartureMm) && demand.DepartureMm >= 0.0,
            Range("effective:departure", demand.DepartureMm, "finite and nonnegative"))
        from _2 in guard(admitted.Material != MaterialCondition.Regardless || demand.DepartureMm == 0.0,
            Range("effective:departure", demand.DepartureMm, "zero under a regardless-of-feature-size control"))
        let width = admitted.Zone.Width.ToValue()
        let boundaries = admitted.Size.Bind(size => admitted.Material.Boundaries(size, width))
        select new ToleranceResult.Effective(admitted,
            admitted.Material.Effective(width, demand.DepartureMm), demand.DepartureMm,
            boundaries.Map(static row => row.VirtualMm), boundaries.Map(static row => row.ResultantMm));

    private static Fin<ToleranceResult.Allowance> Allowance(ToleranceRequest.Allowance demand) =>
        Optional(demand.Grade).ToFin(Invalid("allowance:grade")).Map(static admitted =>
            new ToleranceResult.Allowance(admitted.ToleranceMillimeters * admitted.AllowanceFactor.ToValue()));

    private static Fin<ToleranceResult.Projected> Projected(ToleranceRequest.Project demand) =>
        from bytes in Try.lift(() => demand.Encoder.Encode(demand.Value)).Run().Bind(static inner => inner)
        select new ToleranceResult.Projected(demand.Value, bytes);

    private static Fin<ToleranceResult.Scallop> Scallop(ToleranceRequest.Scallop demand) =>
        from admittedTarget in Optional(demand.Target).ToFin(Invalid("scallop:target"))
        from admittedCutter in Optional(demand.Cutter).ToFin(Invalid("scallop:cutter"))
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
            Range("scallop:radius", radius, "finite and positive"))
        from _2 in guard(double.IsFinite(radicand) && radicand > 0.0,
            Range("scallop:radicand", radicand, "finite and positive"))
        select new ToleranceResult.Scallop(2.0 * Math.Sqrt(radicand));
}

public interface ISpecDemand<TResult> where TResult : ToleranceResult { }

[Union]
public abstract partial record ToleranceRequest {
    private ToleranceRequest() { }
    public sealed record Feature(CharacteristicId Id, ContentKey Source, FeatureCharacteristic Characteristic, FeatureScope Scope,
        ToleranceZone Zone, DatumSystem Datums, MaterialCondition Material, FrameExtension Extension,
        Option<FeatureSize> Size, Option<double> AchievableMm, SheetStandard Standard)
        : ToleranceRequest, ISpecDemand<ToleranceResult.Frame>;
    public sealed record Fit(ContentKey Source, double NominalMm, FitClass Hole, FitClass Shaft)
        : ToleranceRequest, ISpecDemand<ToleranceResult.Fitted>;
    public sealed record Texture(ContentKey Source, Arr<SurfaceRequirement> Requirements, SurfaceLay Lay,
        ProcessMark Mark, Option<TransmissionBand> Band, Option<double> MachiningAllowanceMm,
        Option<string> Treatment) : ToleranceRequest, ISpecDemand<ToleranceResult.Textured>;
    public sealed record General(ContentKey Source, GeneralToleranceClass Class, GeneralToleranceKind Kind,
        double NominalMm, GeneralStandard Standard) : ToleranceRequest, ISpecDemand<ToleranceResult.Generalized>;
    public sealed record Chain(ContentKey Source, Arr<ToleranceTerm> Terms, double BoundMm,
        StackMethod Method, Instant Stamped) : ToleranceRequest, ISpecDemand<ToleranceResult.Stacked>;
    public sealed record Quantity(SpecAxis Axis, string Text)
        : ToleranceRequest, ISpecDemand<ToleranceResult.Quantity>;
    public sealed record Effective(FeatureControl Control, double DepartureMm)
        : ToleranceRequest, ISpecDemand<ToleranceResult.Effective>;
    public sealed record Scallop(RaTarget Target, CutterForm Cutter)
        : ToleranceRequest, ISpecDemand<ToleranceResult.Scallop>;
    public sealed record Allowance(ItGrade Grade)
        : ToleranceRequest, ISpecDemand<ToleranceResult.Allowance>;
    public sealed record Project(ToleranceSpec Value, IToleranceEncoder Encoder)
        : ToleranceRequest, ISpecDemand<ToleranceResult.Projected>;
}

[Union]
public abstract partial record ToleranceResult {
    private ToleranceResult() { }
    public sealed record Frame(ToleranceSpec.Geometric Value, FeatureFrame Frame) : ToleranceResult;
    public sealed record Fitted(ToleranceSpec.Fit Value) : ToleranceResult;
    public sealed record Textured(ToleranceSpec.Texture Value) : ToleranceResult;
    public sealed record Generalized(ToleranceSpec.General Value) : ToleranceResult;
    public sealed record Stacked(ToleranceSpec.Chain Value, ChainEvidence Settled) : ToleranceResult;
    public sealed record Quantity(SpecQuantity Value) : ToleranceResult;
    public sealed record Effective(FeatureControl Control, double WidthMm, double DepartureMm,
        Option<double> VirtualConditionMm, Option<double> ResultantConditionMm) : ToleranceResult;
    public sealed record Scallop(double StepMm) : ToleranceResult;
    public sealed record Allowance(double Millimeters) : ToleranceResult;
    public sealed record Projected(ToleranceSpec Value, ReadOnlyMemory<byte> Bytes) : ToleranceResult;

    public Option<ToleranceSpec> Specification() => Switch(
        frame: static row => Some<ToleranceSpec>(row.Value), fitted: static row => Some<ToleranceSpec>(row.Value),
        textured: static row => Some<ToleranceSpec>(row.Value), generalized: static row => Some<ToleranceSpec>(row.Value),
        stacked: static row => Some<ToleranceSpec>(row.Value), quantity: static _ => None,
        effective: static _ => None, scallop: static _ => None, allowance: static _ => None,
        projected: static row => Some(row.Value));

    public Option<bool> Conforming() => Switch(
        frame: static row => row.Frame.AchievableMm.Map(achievable => achievable <= row.Frame.WidthMm),
        fitted: static _ => None, textured: static _ => None, generalized: static _ => None,
        stacked: static row => Some(row.Settled.Conforming), quantity: static _ => None,
        effective: static _ => None, scallop: static _ => None, allowance: static _ => None,
        projected: static _ => None);
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
