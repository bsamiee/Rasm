# [RASM_FABRICATION_BEVEL]

`Bevel.Condition` admits a station-varying preparation field, process evidence, head kinematics, compensation calibration, height-control policy, and pass schedule before it emits guarded tool-axis blocks. `BevelPass` preserves geometry, process, height-control, and inspection evidence as one specialized section.

`Bevel.Condition`, `BevelPass`, `ThcDirective`, and `ThcSpan` remain the bevel seam names. `Beveled.SpecializedDirective` projects tool axis, pivot, angle, compensation, feed, inspection, and derived duration into the shared specialized-toolpath envelope. `ProcessBudget.Thermal` and `ProcessBudget.Abrasive` own cut physics, `ArcAlgebra.Apply` owns kerf topology, and typed `MTConnect` measurements bind head geometry without string codes.

## [01]-[INDEX]

- [01]-[ADMISSION]: `BevelDemand` composes admitted dimensions once; generated owners admit preparation, compensation, kinematics, height control, and passes.
- [02]-[CONDITIONING]: `Bevel.Condition` offsets the source edge, samples arc length adaptively, evaluates the preparation field, resolves tool-axis compensation, lowers moves, and guards each pass.
- [03]-[EGRESS]: `Beveled` projects through a caller-supplied arrow while preserving coupled `BevelPass` sections and quality evidence.

## [02]-[ADMISSION]

`BevelJob` owns every fact that changes edge preparation. Named groove forms are seed values over the same generated section equation; `PrepSection.Custom` and `PrepLaw` extend the section and edge-station spaces without another bevel type or entrypoint.

- Owner: generated `PrepStandard` values supply section-law data for square, top, underside, opposed, land, radius, flare, and scarf forms; `TopShare` and `BottomShare` partition the body into disjoint flank bands, `MemberScale` states how much of the joint's included angle this member cuts, and `BottomAngleScale` carries the asymmetric double-prep flank a single admitted angle cannot express.
- Owner: `PrepSection` closes generated and custom profiles and projects `SideSign` and `Mirrored`; `PrepLaw` admits one coherent side across every station and owns `KerfSideMm`, so kerf compensation follows the prepared side instead of a fixed positive offset.
- Owner: `HeadPolicy` couples kinematics, an injected orientation solve, and pivot beside the typed `ToolCuttingEdgeAngleMeasurement` tilt limit, the `ProcessFeedRate` envelope, and corner-radius/chamfer measurements; no hand-declared scalar restates a catalogued tool measurement.
- Owner: `CompensationPolicy` couples calibrated geometry, axis, lag, kerf, and wear terms.
- Owner: each `PassRow` couples its sensing modality; `ThcPolicy` couples anti-dive, corner hold, response, and activation evidence.
- Packages: `UnitsNet` converts dimensional text; `TensorPrimitives.IsFiniteAll` admits numeric batches; `Thinktecture` closes construction.
- Boundary: `BevelDemand` crosses the nullable boundary exactly once, and every interior function consumes `BevelJob`.

## [03]-[CONDITIONING]

`Bevel.Condition` evaluates one continuous preparation field rather than branching on a fixed groove roster. `I` and underside forms are ordinary data rows, so no angle heuristic can make either unreachable, and every generated section includes both terminals.

- Entry: `Condition<TOut>` parameterizes admitted ingress, move lowering, collision guarding, and egress projection.
- Auto: adaptive station count unions bulge-sagitta subdivision with per-knot subdivision scaled by preparation offset change, so each knot is sampled; `PlineSeg.SegTangentVector` preserves arc tangency.
- Auto: `PrepLaw.OffsetAt` interpolates both through-thickness and edge-station dimensions for variable and compound bevels.
- Auto: calibrated compensation partitions geometry shift and tool-axis correction while pivot and head limits remain admitted constraints.
- Auto: anti-dive reads emitted feed ratio and angle rate, and its armed counter resets on every suspension; `ThcSpan.AdmitSchedule` coalesces adjacent equal directives and proves full-pass coverage.
- Packages: `ArcAlgebra.Apply` owns kerf offset; `Polyline<double>.PathLength` and `FindPointAtPathLength` own arc-native stations; `LanguageExt` `TraverseM`, `FoldM`, and query syntax keep the rail flat.
- Boundary: unsupported process or head demand returns `FabricationFault.BevelUnsupported`; no silent tilt clamp, swallowed guard failure, or detached THC bag survives.

## [04]-[EGRESS]

`BevelPass` is inverse-sufficient: every `BevelBlock` carries source span, bulge, station, path distance, preparation offset, tool axis, pivot, angle, angle rate, feed, and compensation; its own `ThcSpan` rows cover the same block range, and `BevelReceipt.Passes` remains the single pass owner for every projection.

- Receipt: `BevelReceipt` preserves standard/custom law, extrema, conditioned length, head measurements, pass evidence, and guard count.
- Projection: `Beveled.PostingSource` carries the typed envelope into canonical posting; the caller arrow retains other result projections.
- Consumer: posting and simulation retain axis, pivot, inspection, process, and duration evidence, and estimation consumes that simulation receipt.
- Growth: a standard groove is one `PrepStandard` seed value; a novel section is one `PrepSection.Custom` payload; a new sensor is one `HeightSource` case, and `ThcDirective.Regulating` carries it without a mirrored directive arm.
- Boundary: `ThcSpan` rows neither overlap nor gap, and every non-`Off` terminal closes inside the admitted schedule.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Numerics.Tensors;
using CavalierContours.Core;
using CavalierContours.Polyline;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using MTConnect.Assets.CuttingTools;
using MTConnect.Assets.CuttingTools.Measurements;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class PrepStandard {
    public static readonly PrepStandard I = Create(0.0, 0.0, 0.0, 0.0, 1.0);
    public static readonly PrepStandard V = Create(1.0, 0.0, 0.0, 0.5, 1.0);
    public static readonly PrepStandard A = Create(0.0, 1.0, 0.0, 0.5, 1.0);
    public static readonly PrepStandard Y = Create(0.7, 0.0, 0.0, 0.5, 1.0);
    public static readonly PrepStandard X = Create(0.5, 0.5, 0.0, 0.5, 1.0);
    public static readonly PrepStandard K = Create(0.5, 0.5, 0.0, 1.0, 1.0);
    public static readonly PrepStandard J = Create(1.0, 0.0, 1.0, 1.0, 1.0);
    public static readonly PrepStandard U = Create(0.5, 0.5, 1.0, 0.5, 1.0);
    public static readonly PrepStandard Flare = Create(0.5, 0.0, 1.0, 1.0, 1.0);
    public static readonly PrepStandard Scarf = Create(1.0, 0.0, 0.35, 1.0, 1.0);

    public double TopShare { get; }
    public double BottomShare { get; }
    public double RadiusBlend { get; }
    public double MemberScale { get; }
    public double BottomAngleScale { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double topShare,
        ref double bottomShare,
        ref double radiusBlend,
        ref double memberScale,
        ref double bottomAngleScale) {
        if (!TensorPrimitives.IsFiniteAll<double>([topShare, bottomShare, radiusBlend, memberScale, bottomAngleScale])
            || topShare is < 0.0 or > 1.0 || bottomShare is < 0.0 or > 1.0 || radiusBlend is < 0.0 or > 1.0
            || memberScale is < 0.0 or > 1.0 || bottomAngleScale is < 0.0 or > 2.0
            || topShare + bottomShare > 1.0)
            validationError = new ValidationError("bevel:standard");
    }

    // Top and bottom flanks occupy disjoint bands of the body and carry independent angles, so an asymmetric double prep is expressible.
    public double OffsetAt(PrepDimensions dimensions, double through) {
        double body = Math.Max(0.0, dimensions.ThicknessMm - dimensions.RootFaceMm);
        double topDepth = body * TopShare;
        double bottomDepth = body * BottomShare;
        double depth = through * dimensions.ThicknessMm;
        double top = topDepth <= 0.0 ? 0.0 : Profile(Math.Clamp((topDepth - depth) / topDepth, 0.0, 1.0));
        double bottom = bottomDepth <= 0.0
            ? 0.0
            : Profile(Math.Clamp((depth - (dimensions.ThicknessMm - bottomDepth)) / bottomDepth, 0.0, 1.0));
        double topAngle = dimensions.AngleDeg * MemberScale;
        return dimensions.RootOpeningMm * 0.5
            + Math.Tan(topAngle * Math.PI / 180.0) * topDepth * top
            + Math.Tan(topAngle * BottomAngleScale * Math.PI / 180.0) * bottomDepth * bottom
            + dimensions.RadiusMm * RadiusBlend * Math.Sin(Math.PI * Math.Clamp(through, 0.0, 1.0));
    }

    private double Profile(double value) => (1.0 - RadiusBlend) * value + RadiusBlend * Math.Sin(value * Math.PI * 0.5);
}

[SmartEnum<string>]
public sealed partial class PrepSide {
    public static readonly PrepSide Left = new("left", sign: 1.0, mirrored: false);
    public static readonly PrepSide Right = new("right", sign: -1.0, mirrored: false);
    public static readonly PrepSide Centered = new("centered", sign: 1.0, mirrored: true);

    public double Sign { get; }
    public bool Mirrored { get; }
}

[ValueObject<double>]
public sealed partial class CompensationMode {
    public static readonly CompensationMode Geometry = Create(1.0);
    public static readonly CompensationMode Head = Create(0.0);
    public static readonly CompensationMode Hybrid = Create(0.5);

    public double GeometryShare => Value;
    public double AxisShare => 1.0 - Value;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0
            ? null
            : new ValidationError("bevel:compensation-mode");
}

[Union]
public abstract partial record HeadKinematics {
    public sealed record Fixed(Vector3d Axis) : HeadKinematics;
    public sealed record Rotary(Vector3d PivotAxis, double RotaryZeroDeg) : HeadKinematics;
    public sealed record FiveAxis(Vector3d PrimaryAxis, Vector3d SecondaryAxis) : HeadKinematics;
    public sealed record Robot(Vector3d ToolAxis, string FrameKey) : HeadKinematics;
}

[Union]
public abstract partial record HeightSource {
    public sealed record ArcVoltage(double TargetVolts) : HeightSource;
    public sealed record Capacitive(double HeightMm) : HeightSource;
    public sealed record PlateRide(double HeightMm) : HeightSource;
    public sealed record Disabled : HeightSource;

    public bool Regulates => Switch(
        arcVoltage: static _ => true,
        capacitive: static _ => true,
        plateRide: static _ => true,
        disabled: static _ => false);
}

[Union]
public abstract partial record BevelProcess {
    public sealed record Thermal(ProcessBudget.Thermal Budget) : BevelProcess;
    public sealed record Abrasive(ProcessBudget.Abrasive Budget) : BevelProcess;

    public double Speed() => Switch(
        thermal: static row => row.Budget.CutSpeed,
        abrasive: static row => row.Budget.TraverseSpeed);

    public double KerfWidth(CompensationPolicy compensation) => Switch(
        state: compensation,
        thermal: static (_, row) => row.Budget.KerfWidth,
        abrasive: static (policy, _) => policy.NominalKerfMm);

    public double CrossTilt(CompensationPolicy compensation) => Switch(
        state: compensation,
        thermal: static (_, _) => 0.0,
        abrasive: static (policy, _) => policy.CrossTiltDeg);

    public BudgetEvidence Evidence() => Switch(
        thermal: static row => row.Budget.Evidence,
        abrasive: static row => row.Budget.Evidence);

    public bool Accepts(HeightSource source) => Switch(
        state: source,
        thermal: static (_, _) => true,
        abrasive: static (height, _) => height is HeightSource.Disabled);

    public static Fin<BevelProcess> Admit(ProcessBudget? budget, PrepLaw law) =>
        from source in Optional(budget).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:budget").ToError())
        from admitted in source.Switch(
            state: law,
            subtractive: static (prep, _) => Unsupported(prep),
            turning: static (prep, _) => Unsupported(prep),
            thermal: static (_, row) => Fin.Succ<BevelProcess>(new Thermal(row)),
            abrasive: static (_, row) => Fin.Succ<BevelProcess>(new Abrasive(row)),
            fff: static (prep, _) => Unsupported(prep),
            deposition: static (prep, _) => Unsupported(prep),
            joining: static (prep, _) => Unsupported(prep),
            erosion: static (prep, _) => Unsupported(prep),
            resin: static (prep, _) => Unsupported(prep),
            powder: static (prep, _) => Unsupported(prep),
            formed: static (prep, _) => Unsupported(prep))
        select admitted;

    private static Fin<BevelProcess> Unsupported(PrepLaw law) =>
        Fin.Fail<BevelProcess>(FabricationFault.BevelUnsupported(
            Subject(law),
            law.Stations.Map(static row => Math.Abs(row.Section.OffsetAt(0.0) - row.Section.OffsetAt(1.0))).Max()).ToError());

    private static FaultSubject.Bevel Subject(PrepLaw law) =>
        new(FormattableString.Invariant($"{law.ThicknessMm:R}:{law.Stations.Count}"));
}

// Regulating carries the admitted HeightSource rather than mirroring its cases, so a new sensor is one HeightSource case and no directive arm.
[Union]
public abstract partial record ThcDirective {
    public sealed record Regulating(HeightSource Source) : ThcDirective;
    public sealed record Hold : ThcDirective;
    public sealed record Off : ThcDirective;
}

// --- [ADMISSION] ----------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class PrepDimensions {
    public double ThicknessMm { get; }
    public double RootFaceMm { get; }
    public double RootOpeningMm { get; }
    public double RadiusMm { get; }
    public double AngleDeg { get; }

    public static Fin<PrepDimensions> Admit(
        string thickness,
        string rootFace,
        string rootOpening,
        string radiusText,
        double angleDeg) =>
        from total in Bevel.Millimeters(thickness, "bevel:thickness")
        from face in Bevel.Millimeters(rootFace, "bevel:root-face")
        from opening in Bevel.Millimeters(rootOpening, "bevel:root-opening")
        from radius in Bevel.Millimeters(radiusText, "bevel:radius")
        from admitted in Validate(total, face, opening, radius, angleDeg, out PrepDimensions dimensions) is { } error
            ? Fin.Fail<PrepDimensions>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(dimensions)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double thicknessMm,
        ref double rootFaceMm,
        ref double rootOpeningMm,
        ref double radiusMm,
        ref double angleDeg) {
        if (!TensorPrimitives.IsFiniteAll<double>([thicknessMm, rootFaceMm, rootOpeningMm, radiusMm, angleDeg])
            || thicknessMm <= 0.0 || rootFaceMm < 0.0 || rootFaceMm > thicknessMm || rootOpeningMm < 0.0
            || radiusMm < 0.0 || angleDeg < 0.0 || angleDeg >= 90.0)
            validationError = new ValidationError("bevel:dimensions");
    }
}

public readonly record struct PrepKnot(double Through, double OffsetMm);

[Union]
public abstract partial record PrepSection {
    public sealed record Standard(PrepStandard Kind, PrepDimensions Dimensions, PrepSide Side) : PrepSection;
    public sealed record Custom(Arr<PrepKnot> Knots) : PrepSection;

    public double OffsetAt(double through) => Switch(
        state: Math.Clamp(through, 0.0, 1.0),
        standard: static (value, row) => row.Side.Sign * row.Kind.OffsetAt(row.Dimensions, value),
        custom: static (value, row) => toSeq(row.Knots).Zip(toSeq(row.Knots).Skip(1))
            .Find(pair => value >= pair.First.Through && value <= pair.Second.Through)
            .Map(pair => pair.First.OffsetMm
                + ((value - pair.First.Through) / (pair.Second.Through - pair.First.Through))
                * (pair.Second.OffsetMm - pair.First.OffsetMm))
            .IfNone(row.Knots[^1].OffsetMm));

    public double SideSign => Switch(
        standard: static row => row.Side.Sign,
        custom: static row => row.Knots
            .Find(static knot => knot.OffsetMm != 0.0)
            .Map(static knot => knot.OffsetMm < 0.0 ? -1.0 : 1.0)
            .IfNone(1.0));

    public bool Mirrored => Switch(
        standard: static row => row.Side.Mirrored,
        custom: static _ => false);

    public bool Valid() => Switch(
        standard: static row => row.Kind is not null && row.Dimensions is not null && row.Side is not null,
        custom: static row => row.Knots.Count >= 2
            && row.Knots.ForAll(static knot => double.IsFinite(knot.Through) && double.IsFinite(knot.OffsetMm))
            && (row.Knots.ForAll(static knot => knot.OffsetMm >= 0.0)
                || row.Knots.ForAll(static knot => knot.OffsetMm <= 0.0))
            && row.Knots[0].Through == 0.0 && row.Knots[^1].Through == 1.0
            && toSeq(row.Knots).Zip(toSeq(row.Knots).Skip(1))
                .ForAll(static pair => pair.First.Through < pair.Second.Through));
}

public readonly record struct PrepStation(double Station, PrepSection Section);

[ComplexValueObject]
public sealed partial class PrepLaw {
    public Arr<PrepStation> Stations { get; }
    public double ThicknessMm { get; }

    // Kerf compensates toward the prepared side, so an outside and an inside preparation never share one offset direction.
    public double KerfSideMm(double kerfWidthMm) => 0.5 * kerfWidthMm * Stations[0].Section.SideSign;

    public double OffsetAt(double station, double through) => toSeq(Stations).Zip(toSeq(Stations).Skip(1))
        .Find(pair => station >= pair.First.Station && station <= pair.Second.Station)
        .Map(pair => {
            double scale = (station - pair.First.Station) / (pair.Second.Station - pair.First.Station);
            return pair.First.Section.OffsetAt(through)
                + scale * (pair.Second.Section.OffsetAt(through) - pair.First.Section.OffsetAt(through));
        })
        .IfNone(Stations[^1].Section.OffsetAt(through));

    public static Fin<PrepLaw> Admit(string thickness, Arr<PrepStation> stations) =>
        from total in Bevel.Millimeters(thickness, "bevel:thickness")
        from admitted in Validate(stations, total, out PrepLaw law) is { } error
                    ? Fin.Fail<PrepLaw>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
                    : Fin.Succ(law)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Arr<PrepStation> stations,
        ref double thicknessMm) {
        double admittedThickness = thicknessMm;
        bool valid = stations.Count >= 2
            && stations.ForAll(static row => double.IsFinite(row.Station) && row.Section is not null && row.Section.Valid())
            && stations.ForAll(row => row.Section.Switch(
                standard: section => section.Dimensions.ThicknessMm == admittedThickness,
                custom: static _ => true))
            && stations.ForAll(row => row.Section.SideSign == stations[0].Section.SideSign)
            && stations[0].Station == 0.0 && stations[^1].Station == 1.0
            && toSeq(stations).Zip(toSeq(stations).Skip(1)).ForAll(static pair => pair.First.Station < pair.Second.Station);
        if (!valid || !double.IsFinite(thicknessMm) || thicknessMm <= 0.0)
            validationError = new ValidationError("bevel:prep-law");
    }
}

[ComplexValueObject]
public sealed partial class PassRow {
    public int Pass { get; }
    public double DepthShare { get; }
    public double FeedScale { get; }
    public HeightSource Height { get; }
    public double PierceDelaySeconds { get; }

    public static Fin<PassRow> Admit(
        int ordinal,
        double depthShare,
        double feedScale,
        HeightSource height,
        double pierceDelaySeconds) =>
        Validate(ordinal, depthShare, feedScale, height, pierceDelaySeconds, out PassRow pass) is { } error
            ? Fin.Fail<PassRow>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(pass);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int pass,
        ref double depthShare,
        ref double feedScale,
        ref HeightSource height,
        ref double pierceDelaySeconds) {
        if (pass < 1 || height is null || !TensorPrimitives.IsFiniteAll<double>([depthShare, feedScale, pierceDelaySeconds])
            || depthShare <= 0.0 || depthShare > 1.0 || feedScale <= 0.0 || pierceDelaySeconds < 0.0
            || !Valid(height))
            validationError = new ValidationError("bevel:pass");
    }

    private static bool Valid(HeightSource source) => source is not null && source.Switch(
        arcVoltage: static row => double.IsFinite(row.TargetVolts) && row.TargetVolts > 0.0,
        capacitive: static row => double.IsFinite(row.HeightMm) && row.HeightMm >= 0.0,
        plateRide: static row => double.IsFinite(row.HeightMm) && row.HeightMm >= 0.0,
        disabled: static _ => true);
}

[ComplexValueObject]
public sealed partial class HeadPolicy {
    public HeadKinematics Kinematics { get; }
    public Func<HeadKinematics, Vector3d, Vector3d, double, double, Fin<Vector3d>> Orient { get; }
    public double PivotLengthMm { get; }
    public ToolCuttingEdgeAngleMeasurement TiltLimit { get; }
    public ProcessFeedRate Feed { get; }
    public CornerRadiusMeasurement CornerRadius { get; }
    public ChamferWidthMeasurement ChamferWidth { get; }

    public double MaxTiltDeg => TiltLimit.Value;
    public double MinFeedMmPerMin => Feed.Minimum;
    public double MaxFeedMmPerMin => Feed.Maximum;

    public static Fin<HeadPolicy> Admit(
        HeadKinematics kinematics,
        Func<HeadKinematics, Vector3d, Vector3d, double, double, Fin<Vector3d>> orient,
        string pivotLength,
        ToolCuttingEdgeAngleMeasurement tiltLimit,
        ProcessFeedRate feed,
        CornerRadiusMeasurement cornerRadius,
        ChamferWidthMeasurement chamferWidth) =>
        from pivot in Bevel.Millimeters(pivotLength, "bevel:pivot")
        from admitted in Validate(kinematics, orient, pivot, tiltLimit, feed,
            cornerRadius, chamferWidth, out HeadPolicy head) is { } error
            ? Fin.Fail<HeadPolicy>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(head)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HeadKinematics kinematics,
        ref Func<HeadKinematics, Vector3d, Vector3d, double, double, Fin<Vector3d>> orient,
        ref double pivotLengthMm,
        ref ToolCuttingEdgeAngleMeasurement tiltLimit,
        ref ProcessFeedRate feed,
        ref CornerRadiusMeasurement cornerRadius,
        ref ChamferWidthMeasurement chamferWidth) {
        bool axis = kinematics is not null && kinematics.Switch(
            @fixed: static row => row.Axis.IsValid && row.Axis.Length > 0.0,
            rotary: static row => row.PivotAxis.IsValid && row.PivotAxis.Length > 0.0 && double.IsFinite(row.RotaryZeroDeg),
            fiveAxis: static row => row.PrimaryAxis.IsValid && row.SecondaryAxis.IsValid
                && row.PrimaryAxis.Length > 0.0 && row.SecondaryAxis.Length > 0.0,
            robot: static row => row.ToolAxis.IsValid && row.ToolAxis.Length > 0.0 && !string.IsNullOrWhiteSpace(row.FrameKey));
        if (!axis || orient is null || tiltLimit is null || feed is null || cornerRadius is null || chamferWidth is null
            || !TensorPrimitives.IsFiniteAll<double>([pivotLengthMm, tiltLimit.Value, feed.Minimum, feed.Maximum,
                cornerRadius.Value, chamferWidth.Value])
            || pivotLengthMm < 0.0 || tiltLimit.Value < 0.0 || tiltLimit.Value >= 90.0
            || feed.Minimum <= 0.0 || feed.Maximum < feed.Minimum
            || cornerRadius.Value < 0.0 || chamferWidth.Value < 0.0)
            validationError = new ValidationError("bevel:head");
    }
}

[ComplexValueObject]
public sealed partial class CompensationPolicy {
    public CompensationMode Mode { get; }
    public double NominalKerfMm { get; }
    public double KerfGain { get; }
    public double LagDegPerMeterPerMinute { get; }
    public double WearMm { get; }
    public double AngleBiasDeg { get; }
    public double CrossTiltDeg { get; }

    public static Fin<CompensationPolicy> Admit(
        CompensationMode mode,
        string nominalKerf,
        double kerfGain,
        double lagDegPerMeterPerMinute,
        double wearMm,
        double angleBiasDeg,
        double crossTiltDeg) =>
        from kerf in Bevel.Millimeters(nominalKerf, "bevel:nominal-kerf")
        from admitted in Validate(mode, kerf, kerfGain, lagDegPerMeterPerMinute, wearMm,
            angleBiasDeg, crossTiltDeg, out CompensationPolicy policy) is { } error
            ? Fin.Fail<CompensationPolicy>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(policy)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CompensationMode mode,
        ref double nominalKerfMm,
        ref double kerfGain,
        ref double lagDegPerMeterPerMinute,
        ref double wearMm,
        ref double angleBiasDeg,
        ref double crossTiltDeg) {
        if (mode is null || !TensorPrimitives.IsFiniteAll<double>(
                [nominalKerfMm, kerfGain, lagDegPerMeterPerMinute, wearMm, angleBiasDeg, crossTiltDeg])
            || nominalKerfMm <= 0.0 || kerfGain < 0.0 || lagDegPerMeterPerMinute < 0.0 || wearMm < 0.0)
            validationError = new ValidationError("bevel:compensation");
    }
}

[ComplexValueObject]
public sealed partial class ThcPolicy {
    public double AntiDiveFeedRatio { get; }
    public double AngleRateHoldDegPerStation { get; }
    public int ResponseBlocks { get; }

    public static Fin<ThcPolicy> Admit(double antiDiveFeedRatio, double angleRateHoldDegPerStation, int responseBlocks) =>
        Validate(antiDiveFeedRatio, angleRateHoldDegPerStation, responseBlocks, out ThcPolicy policy) is { } error
            ? Fin.Fail<ThcPolicy>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(policy);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double antiDiveFeedRatio,
        ref double angleRateHoldDegPerStation,
        ref int responseBlocks) {
        if (!TensorPrimitives.IsFiniteAll<double>([antiDiveFeedRatio, angleRateHoldDegPerStation])
            || antiDiveFeedRatio <= 0.0 || antiDiveFeedRatio > 1.0 || angleRateHoldDegPerStation < 0.0 || responseBlocks < 1)
            validationError = new ValidationError("bevel:thc-policy");
    }
}

public sealed record BevelDemand(
    Loop Edge,
    PrepLaw Preparation,
    Arr<PassRow> Passes,
    BevelProcess Budget,
    HeadPolicy Head,
    CompensationPolicy Compensation,
    ThcPolicy Thc,
    Arr<BevelObservation> Observations,
    double ChordErrorMm,
    Func<BevelPoint, Fin<Move>> Lower,
    Func<Seq<BevelBlock>, Fin<Unit>> Guard);

[ComplexValueObject]
public sealed partial class BevelJob {
    public Loop Edge { get; }
    public PrepLaw Preparation { get; }
    public Arr<PassRow> Passes { get; }
    public BevelProcess Budget { get; }
    public HeadPolicy Head { get; }
    public CompensationPolicy Compensation { get; }
    public ThcPolicy Thc { get; }
    public Arr<BevelObservation> Observations { get; }
    public double ChordErrorMm { get; }
    public Func<BevelPoint, Fin<Move>> Lower { get; }
    public Func<Seq<BevelBlock>, Fin<Unit>> Guard { get; }

    public static Fin<BevelJob> Admit(BevelDemand? candidate) =>
        from raw in Optional(candidate).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:demand").ToError())
        from admitted in Validate(raw.Edge, raw.Preparation, raw.Passes, raw.Budget, raw.Head, raw.Compensation, raw.Thc, raw.Observations,
            raw.ChordErrorMm,
            raw.Lower, raw.Guard, out BevelJob job) is { } error
            ? Fin.Fail<BevelJob>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(job)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Loop edge,
        ref PrepLaw preparation,
        ref Arr<PassRow> passes,
        ref BevelProcess budget,
        ref HeadPolicy head,
        ref CompensationPolicy compensation,
        ref ThcPolicy thc,
        ref Arr<BevelObservation> observations,
        ref double chordErrorMm,
        ref Func<BevelPoint, Fin<Move>> lower,
        ref Func<Seq<BevelBlock>, Fin<Unit>> guard) {
        BevelProcess admittedBudget = budget;
        bool schedule = !passes.IsEmpty && passes.ForAll(static pass => pass is not null)
            && passes.Map(static (pass, index) => pass.Pass == index + 1).ForAll(static row => row)
            && passes[^1].DepthShare == 1.0
            && toSeq(passes).Zip(toSeq(passes).Skip(1)).ForAll(static pair => pair.First.DepthShare <= pair.Second.DepthShare)
            && admittedBudget is not null && passes.ForAll(pass => admittedBudget.Accepts(pass.Height));
        if (edge is null || edge.Count < 2 || preparation is null || budget is null || head is null || compensation is null
            || thc is null || observations.Exists(static row => !row.IsValid)
            || !double.IsFinite(chordErrorMm) || chordErrorMm <= 0.0 || lower is null || guard is null || !schedule)
            validationError = new ValidationError("bevel:job");
    }

}

// --- [EVIDENCE] -----------------------------------------------------------------------------------------------------------------------------------
public readonly record struct BevelPoint(
    Point3d Point,
    Vector3d ToolAxis,
    Point3d Pivot,
    double FeedMmPerMin,
    double Station,
    int Pass);

public sealed record BevelBlock(
    Move Move,
    Point3d Point,
    int Pass,
    double Station,
    double PathDistanceMm,
    int SourceSpan,
    double SourceBulge,
    double PreparationOffsetMm,
    Vector3d ToolAxis,
    Point3d Pivot,
    double AngleDeg,
    double AngleRateDegPerStation,
    double CrossTiltDeg,
    double FeedMmPerMin,
    double CompensationMm);

[ComplexValueObject]
public sealed partial class ThcSpan {
    public int FromInclusive { get; }
    public int ToExclusive { get; }
    public ThcDirective Directive { get; }

    public static Fin<ThcSpan> Admit(int fromInclusive, int toExclusive, ThcDirective directive) =>
        Validate(fromInclusive, toExclusive, directive, out ThcSpan span) is { } error
            ? Fin.Fail<ThcSpan>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(span);

    // Run boundaries derive from adjacent-directive inequality, so coalescing stays one linear pass rather than a per-block tail rewrite.
    public static Fin<Seq<ThcSpan>> AdmitSchedule(
        Seq<BevelBlock> blocks,
        HeightSource source,
        ThcPolicy policy,
        double nominalFeed) {
        return from _ in blocks.IsEmpty
                   ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:thc-coverage").ToError())
                   : Fin.Succ(unit)
               let directives = Directives(blocks, source, policy, nominalFeed)
               let starts = Seq(0) + toSeq(Range(1, Math.Max(directives.Count - 1, 0)))
                   .Filter(index => !Equals(directives[index], directives[index - 1]))
               from spans in starts
                   .Map((start, ordinal) => (
                       From: start,
                       To: ordinal + 1 < starts.Count ? starts[ordinal + 1] : directives.Count,
                       Directive: directives[start]))
                   .Traverse(row => Admit(row.From, row.To, row.Directive).ToValidation()).As().ToFin()
               from covered in Covers(spans, blocks.Count)
                   ? Fin.Succ(spans)
                   : Fin.Fail<Seq<ThcSpan>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:thc-coverage").ToError())
               select covered;
    }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int fromInclusive,
        ref int toExclusive,
        ref ThcDirective directive) {
        if (fromInclusive < 0 || toExclusive <= fromInclusive || directive is null)
            validationError = new ValidationError("bevel:thc-span");
    }

    // Armed counter resets on every suspension, so control re-arms after a hold on the response delay observed at pass start.
    private static Seq<ThcDirective> Directives(
        Seq<BevelBlock> blocks,
        HeightSource source,
        ThcPolicy policy,
        double nominalFeed) =>
        blocks
            .Map(block => block.FeedMmPerMin / nominalFeed < policy.AntiDiveFeedRatio
                || block.AngleRateDegPerStation > policy.AngleRateHoldDegPerStation)
            .Fold(
                (Armed: 0, Rows: Seq<ThcDirective>()),
                (state, suspended) => suspended
                    ? (0, state.Rows.Add(new ThcDirective.Hold()))
                    : (state.Armed + 1, state.Rows.Add(state.Armed + 1 >= policy.ResponseBlocks && source.Regulates
                        ? new ThcDirective.Regulating(source)
                        : new ThcDirective.Off())))
            .Rows;

    private static bool Covers(Seq<ThcSpan> spans, int count) =>
        !spans.IsEmpty && spans.Head.FromInclusive == 0 && spans.Last.ToExclusive == count
        && spans.Zip(spans.Skip(1)).ForAll(static pair => pair.First.ToExclusive == pair.Second.FromInclusive);
}

public sealed record BevelPass(
    int Pass,
    Seq<BevelBlock> Blocks,
    Seq<ThcSpan> Thc,
    double PierceDelaySeconds,
    double ConditionedLengthMm,
    double MaxAngleDeg,
    BudgetEvidence Evidence);

public sealed record BevelObservation(
    int Pass,
    int FromBlock,
    int ToBlockExclusive,
    double MeasuredAngleDeg,
    double MeasuredOffsetMm,
    double AngleToleranceDeg,
    double OffsetToleranceMm,
    ContentKey Source) {
    public bool IsValid => Pass > 0 && FromBlock >= 0 && ToBlockExclusive > FromBlock
        && Seq(MeasuredAngleDeg, MeasuredOffsetMm, AngleToleranceDeg, OffsetToleranceMm).ForAll(double.IsFinite)
        && AngleToleranceDeg >= 0.0 && OffsetToleranceMm >= 0.0 && Source is not null;
}

public sealed record BevelInspection(
    BevelObservation Observation,
    double NominalAngleDeg,
    double NominalOffsetMm,
    double AngleDeviationDeg,
    double OffsetDeviationMm,
    bool Conforming);

public sealed record BevelReceipt(
    Seq<BevelPass> Passes,
    PrepLaw Preparation,
    double MinOffsetMm,
    double MaxOffsetMm,
    CornerRadiusMeasurement CornerRadius,
    ChamferWidthMeasurement ChamferWidth,
    Seq<BevelInspection> Inspection,
    int GuardedBlocks);

public sealed record Beveled(BevelReceipt Receipt) {
    public Seq<BevelPass> Passes => Receipt.Passes;

    public SpecializedToolpathEnvelope Specialized => new(
        SpecializedToolpathKind.Bevel,
        Passes.Bind(static pass => pass.Blocks.Map((block, move) =>
                (SpecializedToolpathRow)new SpecializedToolpathRow.Bevel(
                    move, block.Pass, block.Station, block.SourceSpan, block.SourceBulge,
                    block.Point, block.ToolAxis, block.Pivot, block.AngleDeg,
                    block.CrossTiltDeg, block.FeedMmPerMin, block.CompensationMm)))
            .Concat(Receipt.Inspection.Map(static row =>
                (SpecializedToolpathRow)new SpecializedToolpathRow.Inspection(
                    row.Observation.Pass, row.Observation.FromBlock, row.Observation.ToBlockExclusive,
                    row.NominalAngleDeg, row.NominalOffsetMm,
                    row.AngleDeviationDeg, row.OffsetDeviationMm, row.Conforming))),
        Passes.Sum(static pass => pass.Blocks.Zip(pass.Blocks.Skip(1)).Sum(static pair =>
            pair.Second.FeedMmPerMin > 0.0
                ? (pair.Second.PathDistanceMm - pair.First.PathDistanceMm) / pair.Second.FeedMmPerMin * 60.0
                : 0.0) + pass.PierceDelaySeconds));

    public MotionDirective SpecializedDirective => new MotionDirective.Specialized(
        Passes.Bind(static pass => pass.Blocks).IsEmpty ? -1 : Passes.Sum(static pass => pass.Blocks.Count) - 1,
        Specialized);
    public PostSource PostingSource => new PostSource.Specialized(Specialized);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Bevel {
    public static Fin<TOut> Condition<TOut>(BevelDemand? demand, Func<Beveled, TOut> project) =>
        from _ in Optional(project).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:projection").ToError())
        from job in BevelJob.Admit(demand)
        from edge in Offset(job.Edge, job.Preparation.KerfSideMm(job.Budget.KerfWidth(job.Compensation)))
        from passes in job.Passes.AsIterable().ToSeq().TraverseM(pass => Pass(job, edge, pass)).As()
        let receipts = passes.ToSeq()
        let offsets = receipts.Bind(static pass => pass.Blocks.Map(static block => block.PreparationOffsetMm))
        from inspection in Inspect(receipts, job.Observations)
        let receipt = new BevelReceipt(
            receipts,
            job.Preparation,
            offsets.Min(),
            offsets.Max(),
            job.Head.CornerRadius,
            job.Head.ChamferWidth,
            inspection,
            receipts.Sum(static pass => pass.Blocks.Count))
        from projected in Invoke(() => Fin.Succ(project(new Beveled(receipt))), "bevel:projection")
        select projected;

    private static Fin<Seq<BevelInspection>> Inspect(Seq<BevelPass> passes, Arr<BevelObservation> observations) =>
        toSeq(observations).Traverse(observation =>
            from pass in passes.Find(row => row.Pass == observation.Pass)
                .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, observation.Pass, "bevel:inspection-pass").ToError())
            from _ in observation.ToBlockExclusive <= pass.Blocks.Count
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, observation.ToBlockExclusive, "bevel:inspection-range").ToError())
            let span = pass.Blocks.Skip(observation.FromBlock).Take(observation.ToBlockExclusive - observation.FromBlock)
            let angle = span.Average(static row => row.AngleDeg)
            let offset = span.Average(static row => row.PreparationOffsetMm)
            let angleDeviation = observation.MeasuredAngleDeg - angle
            let offsetDeviation = observation.MeasuredOffsetMm - offset
            select new BevelInspection(
                observation,
                angle,
                offset,
                angleDeviation,
                offsetDeviation,
                Math.Abs(angleDeviation) <= observation.AngleToleranceDeg
                    && Math.Abs(offsetDeviation) <= observation.OffsetToleranceMm));

    private static Fin<BevelPass> Pass(BevelJob job, Loop edge, PassRow pass) =>
        from blocks in Blocks(job, edge, pass)
        from _ in Invoke(() => job.Guard(blocks), "bevel:guard")
        from thc in ThcSpan.AdmitSchedule(
            blocks,
            pass.Height,
            job.Thc,
            Math.Clamp(job.Budget.Speed() * pass.FeedScale, job.Head.MinFeedMmPerMin, job.Head.MaxFeedMmPerMin))
        let length = blocks.Zip(blocks.Skip(1))
            .Sum(static pair => pair.Second.PathDistanceMm - pair.First.PathDistanceMm)
        select new BevelPass(
            pass.Pass,
            blocks,
            thc,
            pass.PierceDelaySeconds,
            length,
            blocks.Map(static row => Math.Sqrt(row.AngleDeg * row.AngleDeg + row.CrossTiltDeg * row.CrossTiltDeg)).Max(),
            job.Budget.Evidence());

    // Geometric sagitta stations alone smooth a preparation knot away, so every knot interval contributes its own offset-error subdivision.
    private static Fin<Seq<BevelBlock>> Blocks(BevelJob job, Loop edge, PassRow pass) {
        Polyline<double> path = Native(edge);
        double length = path.PathLength();
        return from geometric in Stations(edge, job.ChordErrorMm)
               let law = toSeq(job.Preparation.Stations)
               let prep = law.Zip(law.Skip(1)).Bind(pair => {
                   double delta = Math.Abs(
                       pair.Second.Section.OffsetAt(pass.DepthShare) - pair.First.Section.OffsetAt(pass.DepthShare));
                   int divisions = Math.Max(1, (int)Math.Ceiling(delta / job.ChordErrorMm));
                   return toSeq(Range(0, divisions + 1)).Map(step =>
                       (pair.First.Station + (pair.Second.Station - pair.First.Station) * step / divisions) * length);
               })
               let stations = (geometric + prep)
                   .Filter(distance => distance >= 0.0 && distance <= length)
                   .Distinct()
                   .OrderBy(static distance => distance)
                   .ToSeq()
               from blocks in stations.TraverseM(distance =>
                   Block(job, edge, path, length, distance / length, distance, pass)).As()
               select blocks;
    }

    private static Fin<BevelBlock> Block(
        BevelJob job,
        Loop edge,
        Polyline<double> path,
        double length,
        double station,
        double distance,
        PassRow pass) =>
        from sample in Sample(path, edge, distance)
        from frame in Frame(path, length, sample, station, pass, job)
        from move in Invoke(() => job.Lower(new BevelPoint(
            frame.Point,
            frame.Axis,
            frame.Pivot,
            frame.Feed,
            station,
            pass.Pass)), "bevel:lower")
        select new BevelBlock(
            move,
            frame.Point,
            pass.Pass,
            station,
            distance,
            sample.Span,
            sample.Bulge,
            frame.Offset,
            frame.Axis,
            frame.Pivot,
            frame.Angle,
            frame.AngleRate,
            frame.CrossTilt,
            frame.Feed,
            frame.Compensation);

    private static Fin<(Point3d Point, Vector3d Axis, Point3d Pivot, double Offset, double Angle,
        double AngleRate, double CrossTilt, double Feed, double Compensation)> Frame(
        Polyline<double> path,
        double length,
        (Point3d Point, int Span, double Bulge, Vector2<double> Native) sample,
        double station,
        PassRow pass,
        BevelJob job) {
        Vector2<double> nativeTangent = PlineSeg.SegTangentVector(
            path[sample.Span],
            path[path.NextWrappingIndex(sample.Span)],
            sample.Native);
        Vector3d tangent = new(nativeTangent.X, nativeTangent.Y, 0.0);
        Vector3d normal = new(-nativeTangent.Y, nativeTangent.X, 0.0);
        if (!tangent.Unitize() || !normal.Unitize())
            return Fin.Fail<(Point3d, Vector3d, Point3d, double, double, double, double, double, double)>(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:tangent").ToError());
        double offset = job.Preparation.OffsetAt(station, pass.DepthShare);
        double speed = job.Budget.Speed();
        double linearCompensation = job.Budget.KerfWidth(job.Compensation) * job.Compensation.KerfGain
            + job.Compensation.WearMm;
        double lever = Math.Max(job.Head.PivotLengthMm, job.Preparation.ThicknessMm);
        double correction = (job.Compensation.AngleBiasDeg
            + job.Compensation.LagDegPerMeterPerMinute * speed / 1000.0
            + Math.Atan2(linearCompensation, lever) * 180.0 / Math.PI) * job.Compensation.Mode.AxisShare;
        double throughDelta = Math.Min(0.5, job.ChordErrorMm / job.Preparation.ThicknessMm);
        double stationDelta = Math.Min(0.5, job.ChordErrorMm / length);
        double angle = Angle(job.Preparation, station, pass.DepthShare, throughDelta)
            + correction;
        double crossTilt = job.Budget.CrossTilt(job.Compensation);
        if (Math.Sqrt(angle * angle + crossTilt * crossTilt) > job.Head.MaxTiltDeg
            || Math.Abs(offset) > job.Head.ChamferWidth.Value)
            return Fin.Fail<(Point3d, Vector3d, Point3d, double, double, double, double, double, double)>(
                FabricationFault.BevelUnsupported(
                    new FaultSubject.Bevel(FormattableString.Invariant($"{job.Preparation.ThicknessMm:R}:{job.Preparation.Stations.Count}")),
                    angle).ToError());
        double compensation = linearCompensation
            + job.Head.CornerRadius.Value * (1.0 - Math.Cos(angle * Math.PI / 180.0));
        Point3d point = sample.Point + normal * (offset + compensation * job.Compensation.Mode.GeometryShare);
        double rate = AngleRate(job.Preparation, station, pass.DepthShare, throughDelta, stationDelta);
        double feed = Math.Clamp(
            speed / (1.0 + Math.Abs(rate)) * pass.FeedScale,
            job.Head.MinFeedMmPerMin,
            job.Head.MaxFeedMmPerMin);
        return from oriented in Invoke(
                   () => job.Head.Orient(job.Head.Kinematics, tangent, normal, angle, crossTilt),
                   "bevel:orient")
               from axis in UnitAxis(oriented, "bevel:oriented-axis")
               select (
                   point,
                   axis,
                   point - axis * job.Head.PivotLengthMm,
                   offset,
                   angle,
                   rate,
                   crossTilt,
                   feed,
                   compensation);
    }

    private static Fin<T> Invoke<T>(Func<Fin<T>> callback, string slot) =>
        Try.lift(callback).Run()
            .MapFail(_ => new GeometryFault.DegenerateInput(Kind.Curve, -1, slot).ToError())
            .Bind(static result => result);

    internal static Fin<double> Millimeters(string text, string field) =>
        PhysicsQuantity.Length.Admit(text)
            .MapFail(_ => new GeometryFault.DegenerateInput(Kind.Curve, -1, field).ToError());

    private static double Angle(PrepLaw law, double station, double through, double delta) {
        double from = Math.Max(0.0, through - delta);
        double to = Math.Min(1.0, through + delta);
        double left = law.OffsetAt(station, from);
        double right = law.OffsetAt(station, to);
        return Math.Atan2(right - left, (to - from) * law.ThicknessMm) * 180.0 / Math.PI;
    }

    private static double AngleRate(PrepLaw law, double station, double through, double throughDelta, double stationDelta) {
        double from = Math.Max(0.0, station - stationDelta);
        double to = Math.Min(1.0, station + stationDelta);
        double left = Angle(law, from, through, throughDelta);
        double right = Angle(law, to, through, throughDelta);
        return Math.Abs(right - left) / (to - from);
    }

    private static Fin<Loop> Offset(Loop edge, double distance) =>
        ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Path(edge), distance)).Bind(trace => trace.Switch(
            forest: static row => row.Result.Loops.Count == 1
                ? Fin.Succ(row.Result.Loops.Head)
                : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:offset-topology").ToError()),
            paths: static row => row.Result.Count == 1
                ? Fin.Succ(row.Result.Head)
                : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:offset-topology").ToError()),
            motion: static _ => Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:offset-topology").ToError()),
            inspection: static _ => Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:offset-topology").ToError()),
            densified: static _ => Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:offset-topology").ToError()),
            recovered: static _ => Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:offset-topology").ToError())));

    private static Polyline<double> Native(Loop loop) =>
        new(loop.Vertices.Map((point, index) => PlineVertex<double>.FromSlice([point.X, point.Y, loop.BulgeAt(index)])), loop.Closed);

    private static Fin<(Point3d Point, int Span, double Bulge, Vector2<double> Native)> Sample(
        Polyline<double> path,
        Loop source,
        double length) =>
        path.FindPointAtPathLength(length) switch {
            (true, int span, Vector2<double> point, _) => Fin.Succ((
                new Point3d(point.X, point.Y, source.Plane), span, source.BulgeAt(span), point)),
            _ => Fin.Fail<(Point3d, int, double, Vector2<double>)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:station").ToError()),
        };

    private static Fin<Seq<double>> Stations(Loop edge, double chordError) =>
        from sampled in Range(0, edge.Closed ? edge.Count : edge.Count - 1)
            .FoldM<Fin, (double Length, Seq<double> Rows)>((0.0, Seq(0.0)), (state, index) => {
                Point3d from = edge.At(index);
                Point3d to = edge.At(index + 1);
                double chord = from.DistanceTo(to);
                double bulge = Math.Abs(edge.BulgeAt(index));
                if (chord <= edge.Tolerance.Absolute.Value)
                    return Fin.Fail<(double, Seq<double>)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "bevel:edge-span").ToError());
                double sweep = 4.0 * Math.Atan(bulge);
                double radius = bulge == 0.0 ? double.PositiveInfinity : chord * (1.0 + bulge * bulge) / (4.0 * bulge);
                double spanLength = bulge == 0.0 ? chord : radius * sweep;
                double maxSweep = bulge == 0.0
                    ? sweep
                    : 2.0 * Math.Acos(Math.Clamp(1.0 - chordError / radius, -1.0, 1.0));
                int divisions = bulge == 0.0 || maxSweep <= 0.0
                    ? 1
                    : Math.Max(1, (int)Math.Ceiling(sweep / maxSweep));
                return Fin.Succ((
                    state.Length + spanLength,
                    state.Rows + Range(1, divisions).Map(step => state.Length + step * spanLength / divisions)));
            }).As()
        select sampled.Rows;

    private static Fin<Vector3d> UnitAxis(Vector3d candidate, string field) {
        Vector3d axis = candidate;
        return axis.Unitize()
            ? Fin.Succ(axis)
            : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Curve, -1, field).ToError());
    }

}
```
