# [RASM_FABRICATION_BEVEL]

`Bevel.Condition` admits a station-varying preparation field, process evidence, head kinematics, compensation calibration, height-control policy, and pass schedule before it emits guarded tool-axis blocks. `BevelPass` preserves geometry, process, height-control, and inspection evidence as one specialized section.

`Bevel.Condition`, `BevelPass`, `ThcDirective`, and `ThcSpan` remain the bevel contract names. `BevelPolicy` is the admitted, edge-independent half of a bevel job, so one calibration conditions many edges. `Beveled.SpecializedDirective` projects tool axis, pivot, angle, compensation, feed, and derived duration into the admitted `Bevel`-kind `SpecializedToolpathEnvelope` under ONE program-wide block ordinal, while `Beveled.InspectionDirective` carries measured conformance on its own `Inspection`-kind one. `ProcessBudget.Thermal` and `ProcessBudget.Abrasive` own cut physics and kerf width alike, `ArcOffset` owns kerf topology, and head geometry binds to the canonical `ToolMetric` rows `Tooling/magazine` already decoded — this page reaches no provider type.

## [01]-[INDEX]

- [02]-[ADMISSION]: `BevelPolicy` composes admitted dimensions once; generated owners admit preparation, compensation, kinematics, height control, and passes.
- [03]-[CONDITIONING]: `Bevel.Condition` offsets the source edge, samples arc length adaptively, evaluates the preparation field, resolves tool-axis compensation on the kinematics case itself, lowers moves, and guards each pass.
- [04]-[EGRESS]: `Beveled` projects program motion, directives, and posting source through a caller-supplied arrow while preserving coupled `BevelPass` sections and quality evidence.

## [02]-[ADMISSION]

`BevelJob` owns every fact that changes edge preparation as THREE columns beyond the edge — the admitted policy, the inspection record, and the two caller interfaces. Named groove forms are seed values over the same generated section equation; `PrepSection.Custom` and `PrepLaw` extend the section and edge-station spaces without another bevel type or entrypoint.

- Owner: generated `PrepStandard` values supply section-law data for square, top, underside, opposed, land, radius, flare, and scarf forms; `TopShare` and `BottomShare` partition the body into disjoint flank bands, `MemberScale` states how much of the joint's included angle this member cuts, and `BottomAngleScale` carries the asymmetric double-prep flank a single admitted angle cannot express.
- Owner: `PrepSection` closes generated and custom profiles and projects `SideSign` and `Mirrored`; `PrepLaw` admits one coherent side across every station and owns `KerfSideMm`, so kerf compensation follows the prepared side instead of a fixed positive offset.
- Owner: `HeadKinematics` owns the ORIENTATION SOLVE itself — each case answers the tool axis its own machine can reach and refuses the tilt it cannot, so a fixed head refuses a demanded bevel, a single rotary refuses a cross tilt, and the five-axis and robot cases carry both rotations. An injected orientation function was a hole in a chartered algorithm through which any answer could arrive unproven.
- Owner: `HeadPolicy` couples kinematics and pivot to the MOUNTED assembly: tilt limit, corner radius, and chamfer width are read off `ToolAssembly.Snapshot` by their `ToolMeasure` rows and the operating envelope is the package's own `ProcessRange`, so a bevel head states no dimension the catalogue has not already decoded and each absent measurement refuses on its own lane. `Feedable` folds the two OPTIONAL bounds, so a controller that published no ceiling caps nothing.
- Owner: `CompensationPolicy` couples calibrated geometry, axis, lag, and wear terms as the quantities they are; kerf width is NOT among them, because `BevelProcess` carries it as a base column filled off the budget both cases already hold. `BevelPolicy` binds preparation, schedule, budget, head, compensation, height control, and chord error into one admitted law whose `BevelProcess` answers which live run budget it serves, so a lane routing into conditioning proves correspondence instead of re-admitting the policy.
- Law: `BevelProcess` fills speed, kerf width, and budget evidence as BASE COLUMNS at construction — three two-arm folds each read one column and returned it, while the kerf arm reached past the abrasive budget's own `KerfWidth` into a compensation column restating it. Cross tilt stays a fold: the thermal case genuinely holds none.
- Owner: each `PassRow` couples its sensing modality; `ThcPolicy` couples anti-dive, corner hold, response, and activation evidence.
- Packages: `Process/owner#RUN_DISPATCH` `QuantityArrow` is the ONE dimension-text entry, admitting a whole batch in one traversal so a four-dimension section crosses the boundary once; `Tooling/magazine` supplies `ToolAssembly`, `ToolMeasure`, and the canonical metric read; `Process/physics#EQUIPMENT` supplies `ProcessRange`; `UnitsNet` types every calibrated magnitude; `TensorPrimitives.IsFiniteAll` admits numeric batches; `Thinktecture` closes construction.
- Boundary: `BevelDemand` crosses the nullable boundary exactly once, and every interior function consumes `BevelJob`.

## [03]-[CONDITIONING]

`Bevel.Condition` evaluates one continuous preparation field rather than branching on a fixed groove roster. `I` and underside forms are ordinary data rows, so no angle heuristic can make either unreachable, and every generated section includes both terminals.

- Entry: `Condition<TOut>` parameterizes admitted ingress, move lowering, collision guarding, and egress projection.
- Auto: adaptive station count unions bulge-sagitta subdivision with per-knot subdivision scaled by preparation offset change, so each knot is sampled; stations deduplicate at the admitted chord error rather than on exact float equality, so two samples the geometry cannot separate never both emit a block. `PlineSeg.SegTangentVector` preserves arc tangency.
- Auto: `PrepLaw.OffsetAt` interpolates both through-thickness and edge-station dimensions for variable and compound bevels.
- Auto: calibrated compensation partitions geometry shift and tool-axis correction while pivot and head limits remain admitted constraints.
- Auto: anti-dive reads emitted feed ratio and angle rate, and its armed counter resets on every suspension; `ThcSpan.AdmitSchedule` coalesces adjacent equal directives and proves full-pass coverage.
- Packages: `ArcOffset` owns kerf offset; `Polyline<double>.PathLength` and `FindPointAtPathLength` own arc-native stations; `LanguageExt` `TraverseM`, `FoldM`, and query syntax keep the result flat.
- Boundary: unsupported process or head demand returns `FabricationFault.BevelUnsupported`; no silent tilt clamp, swallowed guard failure, or detached THC bag survives. A caller callback that throws surfaces its own cause in the refusal locus rather than being flattened to the slot name.

## [04]-[EGRESS]

`BevelPass` is inverse-sufficient: every `BevelBlock` carries its PROGRAM ordinal, source span, bulge, station, path distance, preparation offset, tool axis, pivot, angle, angle rate, feed, and compensation; its own `ThcSpan` rows cover the same block range, and `BevelEvidence.Passes` remains the single pass owner for every projection.

- Law: the block ordinal is the program-wide index into the emitted move stream, the same convention `MoveTrail` fixes for turning — a pass-local ordinal beside a program-wide `AfterMove` on the directive meant two bases for one question, and a consumer joining them read the wrong block.
- Law: a `SpecializedToolpathEnvelope` carries ONE toolpath kind, proved once at envelope construction through the S0 factory. Blocks ride the `Bevel`-kind carrier and conformance rows the `Inspection`-kind one at zero cut duration, so no consumer re-walks either payload and `ToolpathRowMap` owns both transcriptions.
- Law: posting and simulation retain axis, pivot, inspection, process, and duration evidence, and estimation consumes that simulation ledger.
- Output: `Beveled.Moves` and `Beveled.Directives` are the program egress `Toolpath/motion`'s edge-preparation lane folds into one cut element, and `Beveled.PostingSource` carries the typed envelope into canonical posting; the caller arrow retains other result projections.
- Output: `BevelEvidence` preserves standard/custom law, extrema, conditioned length, catalogued head dimensions, pass evidence, and guard count; conditioning mints no key and reads no clock.
- Growth: a standard groove is one `PrepStandard` seed value; a novel section is one `PrepSection.Custom` payload; a new machine posture is one `HeadKinematics` case answering its own orientation solve; a new sensor is one `HeightSource` case, and `ThcDirective.Regulating` carries it without a mirrored directive arm.
- Boundary: `ThcSpan` rows neither overlap nor gap, and every non-`Off` terminal closes inside the admitted schedule.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics.Tensors;
using CavalierContours.Core;
using CavalierContours.Polyline;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Numerics;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ----------------------------------------------------------------------
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

    public Fin<Vector3d> Orient(Vector3d tangent, Vector3d normal, double angleDeg, double crossTiltDeg) => Switch(
        state: (Tangent: tangent, Normal: normal, Angle: angleDeg, Cross: crossTiltDeg),
        @fixed: static (state, row) => state.Angle == 0.0 && state.Cross == 0.0
            ? Unitized(row.Axis, "bevel:fixed-axis")
            : Unsupported("fixed", state.Angle),
        rotary: static (state, row) => state.Cross == 0.0
            ? Tilted(row.PivotAxis, state.Tangent, state.Normal, state.Angle + row.RotaryZeroDeg, 0.0, "bevel:rotary-axis")
            : Unsupported("rotary", state.Cross),
        fiveAxis: static (state, row) => Tilted(
            row.PrimaryAxis, state.Tangent, state.Normal, state.Angle, state.Cross, "bevel:five-axis"),
        robot: static (state, row) => Tilted(
            row.ToolAxis, state.Tangent, state.Normal, state.Angle, state.Cross, "bevel:robot-axis"));

    private static Fin<Vector3d> Tilted(
        Vector3d nominal, Vector3d tangent, Vector3d normal, double angleDeg, double crossTiltDeg, string locus) {
        Vector3d axis = nominal;
        return axis.Unitize()
            && axis.Rotate(angleDeg * Math.PI / 180.0, tangent)
            && axis.Rotate(crossTiltDeg * Math.PI / 180.0, normal)
                ? Unitized(axis, locus)
                : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Curve, None, locus));
    }

    private static Fin<Vector3d> Unitized(Vector3d candidate, string locus) {
        Vector3d axis = candidate;
        return axis.Unitize()
            ? Fin.Succ(axis)
            : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Curve, None, locus));
    }

    private static Fin<Vector3d> Unsupported(string posture, double demand) =>
        Fin.Fail<Vector3d>(new FabricationFault.BevelUnsupported(new FaultSubject.Bevel(posture), demand));
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

    public bool Valid => Switch(
        arcVoltage: static row => ValidityClaim.Positive(row.TargetVolts),
        capacitive: static row => double.IsFinite(row.HeightMm) && row.HeightMm >= 0.0,
        plateRide: static row => double.IsFinite(row.HeightMm) && row.HeightMm >= 0.0,
        disabled: static _ => true);
}

[Union]
public abstract partial record BevelProcess(double SpeedMmPerMin, double KerfWidthMm, BudgetEvidence Evidence) {
    public sealed record Thermal(ProcessBudget.Thermal Budget)
        : BevelProcess(Budget.CutSpeed, Budget.KerfWidth, Budget.Evidence);
    public sealed record Abrasive(ProcessBudget.Abrasive Budget)
        : BevelProcess(Budget.TraverseSpeed, Budget.KerfWidth, Budget.Evidence);

    public Angle CrossTilt(CompensationPolicy compensation) => Switch(
        state: compensation,
        thermal: static (_, _) => Angle.Zero,
        abrasive: static (policy, _) => policy.CrossTilt);

    public bool Accepts(HeightSource source) => Switch(
        state: source,
        thermal: static (_, _) => true,
        abrasive: static (height, _) => height is HeightSource.Disabled);

    public bool Serves(ProcessBudget budget) => Switch(
        state: budget,
        thermal: static (live, _) => live is ProcessBudget.Thermal,
        abrasive: static (live, _) => live is ProcessBudget.Abrasive);

    public static Fin<BevelProcess> Admit(ProcessBudget budget, PrepLaw law) => budget.Switch(
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
        formed: static (prep, _) => Unsupported(prep));

    private static Fin<BevelProcess> Unsupported(PrepLaw law) =>
        Fin.Fail<BevelProcess>(new FabricationFault.BevelUnsupported(
            Subject(law),
            law.Stations.Fold(0.0, static (peak, row) =>
                Math.Max(peak, Math.Abs(row.Section.OffsetAt(0.0) - row.Section.OffsetAt(1.0))))));

    internal static FaultSubject.Bevel Subject(PrepLaw law) =>
        new(FormattableString.Invariant($"{law.ThicknessMm:R}:{law.Stations.Count}"));
}

[Union]
public abstract partial record ThcDirective {
    public sealed record Regulating(HeightSource Source) : ThcDirective;
    public sealed record Hold : ThcDirective;
    public sealed record Off : ThcDirective;
}

// --- [ADMISSION] -----------------------------------------------------------------------
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
        from measures in Bevel.Length.Admit(Seq(thickness, rootFace, rootOpening, radiusText))
        from admitted in Validate(measures[0], measures[1], measures[2], measures[3], angleDeg,
            out PrepDimensions dimensions).Admitted(dimensions)
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
        standard: static _ => true,
        custom: static row => row.Knots.Count >= 2
            && row.Knots.ForAll(static knot => ValidityClaim.Finite([knot.Through, knot.OffsetMm]))
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
        from total in Bevel.Length.Admit(thickness)
        from admitted in Validate(stations, total, out PrepLaw law).Admitted(law)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Arr<PrepStation> stations,
        ref double thicknessMm) {
        double admittedThickness = thicknessMm;
        bool valid = stations.Count >= 2
            && stations.ForAll(static row => double.IsFinite(row.Station) && row.Section.Valid())
            && stations.ForAll(row => row.Section.Switch(
                standard: section => section.Dimensions.ThicknessMm == admittedThickness,
                custom: static _ => true))
            && stations.ForAll(row => row.Section.SideSign == stations[0].Section.SideSign)
            && stations[0].Station == 0.0 && stations[^1].Station == 1.0
            && toSeq(stations).Zip(toSeq(stations).Skip(1)).ForAll(static pair => pair.First.Station < pair.Second.Station);
        if (!valid || !ValidityClaim.Positive(thicknessMm).Holds)
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
        Validate(ordinal, depthShare, feedScale, height, pierceDelaySeconds, out PassRow pass).Admitted(pass);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int pass,
        ref double depthShare,
        ref double feedScale,
        ref HeightSource height,
        ref double pierceDelaySeconds) {
        if (pass < 1 || !TensorPrimitives.IsFiniteAll<double>([depthShare, feedScale, pierceDelaySeconds])
            || depthShare <= 0.0 || depthShare > 1.0 || feedScale <= 0.0 || pierceDelaySeconds < 0.0
            || !height.Valid)
            validationError = new ValidationError("bevel:pass");
    }
}

[ComplexValueObject]
public sealed partial class HeadPolicy {
    public HeadKinematics Kinematics { get; }
    public Length PivotLength { get; }
    public Angle MaxTilt { get; }
    public ProcessRange Feed { get; }
    public Length CornerRadius { get; }
    public Length ChamferWidth { get; }

    public double Feedable(double demandMmPerMin) => Math.Min(
        Feed.Maximum.IfNone(demandMmPerMin),
        Math.Max(Feed.Minimum.IfNone(demandMmPerMin), demandMmPerMin));

    public static Fin<HeadPolicy> Admit(HeadKinematics kinematics, ToolAssembly assembly, string pivotLength) =>
        from pivot in Bevel.Length.Admit(pivotLength)
        from tilt in Measured(assembly, ToolMeasure.CuttingEdgeAngle)
        from corner in Measured(assembly, ToolMeasure.CornerRadius)
        from chamfer in Measured(assembly, ToolMeasure.ChamferWidth)
        from admitted in Validate(kinematics, Length.FromMillimeters(pivot), Angle.FromDegrees(tilt),
            assembly.Feed, Length.FromMillimeters(corner), Length.FromMillimeters(chamfer),
            out HeadPolicy head).Admitted(head)
        select admitted;

    private static Fin<double> Measured(ToolAssembly assembly, ToolMeasure kind) =>
        assembly.Snapshot.Metric(kind).ToFin(
            new KernelFault.InvalidValue("bevel", $"bevel:head:{kind.Key}"));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HeadKinematics kinematics,
        ref Length pivotLength,
        ref Angle maxTilt,
        ref ProcessRange feed,
        ref Length cornerRadius,
        ref Length chamferWidth) {
        bool axis = kinematics.Switch(
            @fixed: static row => row.Axis.IsValid && row.Axis.Length > 0.0,
            rotary: static row => row.PivotAxis.IsValid && row.PivotAxis.Length > 0.0 && double.IsFinite(row.RotaryZeroDeg),
            fiveAxis: static row => row.PrimaryAxis.IsValid && row.SecondaryAxis.IsValid
                && row.PrimaryAxis.Length > 0.0 && row.SecondaryAxis.Length > 0.0,
            robot: static row => row.ToolAxis.IsValid && row.ToolAxis.Length > 0.0 && Witness.Keyed(row.FrameKey));
        if (!axis
            || !TensorPrimitives.IsFiniteAll<double>([pivotLength.Millimeters, maxTilt.Degrees,
                cornerRadius.Millimeters, chamferWidth.Millimeters])
            || pivotLength.Millimeters < 0.0 || maxTilt.Degrees < 0.0 || maxTilt.Degrees >= 90.0
            || cornerRadius.Millimeters < 0.0 || chamferWidth.Millimeters < 0.0)
            validationError = new ValidationError("bevel:head");
    }
}

[ComplexValueObject]
public sealed partial class CompensationPolicy {
    public CompensationMode Mode { get; }
    public Ratio KerfGain { get; }
    public double LagDegPerMeterPerMinute { get; }
    public Length Wear { get; }
    public Angle AngleBias { get; }
    public Angle CrossTilt { get; }

    public static Fin<CompensationPolicy> Admit(
        CompensationMode mode,
        Ratio kerfGain,
        double lagDegPerMeterPerMinute,
        Length wear,
        Angle angleBias,
        Angle crossTilt) =>
        Validate(mode, kerfGain, lagDegPerMeterPerMinute, wear, angleBias, crossTilt,
            out CompensationPolicy policy).Admitted(policy);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CompensationMode mode,
        ref Ratio kerfGain,
        ref double lagDegPerMeterPerMinute,
        ref Length wear,
        ref Angle angleBias,
        ref Angle crossTilt) {
        if (!TensorPrimitives.IsFiniteAll<double>([kerfGain.DecimalFractions, lagDegPerMeterPerMinute,
                wear.Millimeters, angleBias.Degrees, crossTilt.Degrees])
            || kerfGain.DecimalFractions < 0.0 || lagDegPerMeterPerMinute < 0.0 || wear.Millimeters < 0.0)
            validationError = new ValidationError("bevel:compensation");
    }
}

[ComplexValueObject]
public sealed partial class ThcPolicy {
    public double AntiDiveFeedRatio { get; }
    public double AngleRateHoldDegPerStation { get; }
    public int ResponseBlocks { get; }

    public static Fin<ThcPolicy> Admit(double antiDiveFeedRatio, double angleRateHoldDegPerStation, int responseBlocks) =>
        Validate(antiDiveFeedRatio, angleRateHoldDegPerStation, responseBlocks, out ThcPolicy policy).Admitted(policy);

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

[ComplexValueObject]
public sealed partial class BevelPolicy {
    public PrepLaw Preparation { get; }
    public Arr<PassRow> Passes { get; }
    public BevelProcess Budget { get; }
    public HeadPolicy Head { get; }
    public CompensationPolicy Compensation { get; }
    public ThcPolicy Thc { get; }
    public double ChordErrorMm { get; }

    public static Fin<BevelPolicy> Admit(
        PrepLaw preparation,
        Arr<PassRow> passes,
        ProcessBudget budget,
        HeadPolicy head,
        CompensationPolicy compensation,
        ThcPolicy thc,
        double chordErrorMm) =>
        from process in BevelProcess.Admit(budget, preparation)
        from admitted in Validate(preparation, passes, process, head, compensation, thc, chordErrorMm,
            out BevelPolicy policy).Admitted(policy)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PrepLaw preparation,
        ref Arr<PassRow> passes,
        ref BevelProcess budget,
        ref HeadPolicy head,
        ref CompensationPolicy compensation,
        ref ThcPolicy thc,
        ref double chordErrorMm) {
        BevelProcess process = budget;
        bool schedule = !passes.IsEmpty
            && toSeq(passes).Map(static (pass, index) => pass.Pass == index + 1).ForAll(static row => row)
            && passes[^1].DepthShare == 1.0
            && toSeq(passes).Zip(toSeq(passes).Skip(1)).ForAll(static pair => pair.First.DepthShare <= pair.Second.DepthShare)
            && passes.ForAll(pass => process.Accepts(pass.Height));
        if (!schedule || !ValidityClaim.Positive(chordErrorMm).Holds)
            validationError = new ValidationError("bevel:policy");
    }
}

public sealed record BevelDemand(
    BevelPolicy Policy,
    Loop Edge,
    Arr<BevelObservation> Observations,
    Func<BevelPoint, Fin<Move>> Lower,
    Func<Seq<BevelBlock>, Fin<Unit>> Guard);

[ComplexValueObject]
public sealed partial class BevelJob {
    public BevelPolicy Policy { get; }
    public Loop Edge { get; }
    public Arr<BevelObservation> Observations { get; }
    public Func<BevelPoint, Fin<Move>> Lower { get; }
    public Func<Seq<BevelBlock>, Fin<Unit>> Guard { get; }

    public static Fin<BevelJob> Admit(BevelDemand? candidate) =>
        from raw in Optional(candidate).ToFin(new KernelFault.InvalidValue("bevel", "bevel:demand"))
        from admitted in Validate(raw.Policy, raw.Edge, raw.Observations, raw.Lower, raw.Guard, out BevelJob job)
            .Admitted(job)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref BevelPolicy policy,
        ref Loop edge,
        ref Arr<BevelObservation> observations,
        ref Func<BevelPoint, Fin<Move>> lower,
        ref Func<Seq<BevelBlock>, Fin<Unit>> guard) {
        if (edge.Count < 2 || observations.Exists(static row => !row.IsValid) || lower is null || guard is null)
            validationError = new ValidationError("bevel:job");
    }
}

// --- [EVIDENCE] ------------------------------------------------------------------------
public readonly record struct BevelPoint(
    Point3d Point,
    Vector3d ToolAxis,
    Point3d Pivot,
    double FeedMmPerMin,
    double Station,
    int Pass);

public readonly record struct BevelFrame(
    Point3d Point,
    Vector3d Axis,
    Point3d Pivot,
    double OffsetMm,
    double AngleDeg,
    double AngleRateDegPerStation,
    double CrossTiltDeg,
    double FeedMmPerMin,
    double CompensationMm);

public sealed record BevelBlock(
    int Ordinal,
    Move Motion,
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
        Validate(fromInclusive, toExclusive, directive, out ThcSpan span).Admitted(span);

    public static Fin<Seq<ThcSpan>> AdmitSchedule(
        Seq<BevelBlock> blocks,
        HeightSource source,
        ThcPolicy policy,
        double nominalFeed) =>
        from _ in blocks.IsEmpty
            ? Fin.Fail<Unit>(new KernelFault.InvalidValue("bevel", "bevel:thc-coverage"))
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
            : Fin.Fail<Seq<ThcSpan>>(new KernelFault.InvalidValue("bevel", "bevel:thc-coverage"))
        select covered;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int fromInclusive,
        ref int toExclusive,
        ref ThcDirective directive) {
        if (fromInclusive < 0 || toExclusive <= fromInclusive)
            validationError = new ValidationError("bevel:thc-span");
    }

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
        spans.Head.Map(head => head.FromInclusive == 0).IfNone(false)
        && spans.Last.Map(last => last.ToExclusive == count).IfNone(false)
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
        && AngleToleranceDeg >= 0.0 && OffsetToleranceMm >= 0.0;
}

public sealed record BevelInspection(
    BevelObservation Observation,
    double NominalAngleDeg,
    double NominalOffsetMm,
    double AngleDeviationDeg,
    double OffsetDeviationMm) {
    public bool Conforming => Math.Abs(AngleDeviationDeg) <= Observation.AngleToleranceDeg
        && Math.Abs(OffsetDeviationMm) <= Observation.OffsetToleranceMm;
}

public sealed record BevelEvidence(
    Seq<BevelPass> Passes,
    PrepLaw Preparation,
    double MinOffsetMm,
    double MaxOffsetMm,
    Length CornerRadius,
    Length ChamferWidth,
    Seq<BevelInspection> Inspection,
    int GuardedBlocks);

public sealed record Beveled(
    BevelEvidence Evidence,
    SpecializedToolpathEnvelope Specialized,
    Option<SpecializedToolpathEnvelope> Inspection) {
    public Seq<BevelPass> Passes => Evidence.Passes;

    public Seq<Move> Moves => Passes.Bind(static pass => pass.Blocks).Map(static block => block.Motion);

    public MotionDirective SpecializedDirective => new MotionDirective.Specialized(
        Passes.Sum(static pass => pass.Blocks.Count) - 1, Specialized);

    public Option<MotionDirective> InspectionDirective => Inspection
        .Map(static envelope => (MotionDirective)new MotionDirective.Specialized(-1, envelope));

    public Seq<MotionDirective> Directives => Seq(SpecializedDirective) + InspectionDirective.ToSeq();

    public PostSource PostingSource => new PostSource.Specialized(Specialized);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class ToolpathRowMap {
    [MapProperty(nameof(BevelBlock.Ordinal), nameof(SpecializedToolpathRow.Bevel.Move))]
    [MapperIgnoreSource(nameof(BevelBlock.Motion))]
    [MapperIgnoreSource(nameof(BevelBlock.PathDistanceMm))]
    [MapperIgnoreSource(nameof(BevelBlock.PreparationOffsetMm))]
    [MapperIgnoreSource(nameof(BevelBlock.AngleRateDegPerStation))]
    public static partial SpecializedToolpathRow.Bevel ToRow(BevelBlock block);

    [MapProperty([nameof(BevelInspection.Observation), nameof(BevelObservation.Pass)], [nameof(SpecializedToolpathRow.Inspection.Pass)])]
    [MapProperty([nameof(BevelInspection.Observation), nameof(BevelObservation.FromBlock)], [nameof(SpecializedToolpathRow.Inspection.FromBlock)])]
    [MapProperty([nameof(BevelInspection.Observation), nameof(BevelObservation.ToBlockExclusive)], [nameof(SpecializedToolpathRow.Inspection.ToBlockExclusive)])]
    public static partial SpecializedToolpathRow.Inspection ToRow(BevelInspection inspection);
}

public static class Bevel {
    internal static readonly QuantityArrow Length =
        new(PhysicsQuantity.Length, FabConcern.Toolpath, "bevel:length");

    public static Fin<TOut> Condition<TOut>(BevelDemand? demand, Func<Beveled, TOut> project) =>
        from _ in Optional(project).ToFin(new KernelFault.InvalidValue("bevel", "bevel:projection"))
        from job in BevelJob.Admit(demand)
        from edge in ArcOffset.Single(
            job.Edge, job.Policy.Preparation.KerfSideMm(job.Policy.Budget.KerfWidthMm), "bevel:kerf")
        from passes in job.Policy.Passes.AsIterable().ToSeq().FoldM<Fin, (Seq<BevelPass> Rows, int Ordinal)>(
            (Seq<BevelPass>(), 0),
            (state, pass) => Pass(job, edge, pass, state.Ordinal)
                .Map(walked => (state.Rows.Add(walked), state.Ordinal + walked.Blocks.Count))).As()
        let rows = passes.Rows
        from extrema in Extrema(rows.Bind(static pass => pass.Blocks.Map(static block => block.PreparationOffsetMm)))
        from inspection in Inspect(rows, job.Observations)
        let evidence = new BevelEvidence(
            rows,
            job.Policy.Preparation,
            extrema.Min,
            extrema.Max,
            job.Policy.Head.CornerRadius,
            job.Policy.Head.ChamferWidth,
            inspection,
            rows.Sum(static pass => pass.Blocks.Count))
        from envelope in SpecializedToolpathEnvelope.Admit(
            SpecializedToolpathKind.Bevel,
            rows.Bind(static pass => pass.Blocks.Map(static block => (SpecializedToolpathRow)ToolpathRowMap.ToRow(block))),
            rows.Sum(static pass => pass.Blocks.Zip(pass.Blocks.Skip(1)).Sum(static pair =>
                (pair.Second.PathDistanceMm - pair.First.PathDistanceMm) / pair.Second.FeedMmPerMin * 60.0)
                + pass.PierceDelaySeconds))
        from inspected in inspection.IsEmpty
            ? Fin.Succ(Option<SpecializedToolpathEnvelope>.None)
            : SpecializedToolpathEnvelope.Admit(
                    SpecializedToolpathKind.Inspection,
                    inspection.Map(static row => (SpecializedToolpathRow)ToolpathRowMap.ToRow(row)),
                    0.0)
                .Map(static row => Some(row))
        from projected in Invoke(() => Fin.Succ(project(new Beveled(evidence, envelope, inspected))), "bevel:projection")
        select projected;

    private static Fin<(double Min, double Max)> Extrema(Seq<double> values) =>
        values.Head
            .Map(seed => values.Fold(
                (Min: seed, Max: seed),
                static (row, value) => (Math.Min(row.Min, value), Math.Max(row.Max, value))))
            .ToFin(new KernelFault.InvalidValue("bevel", "bevel:offset-extrema"));

    private static Fin<Seq<BevelInspection>> Inspect(Seq<BevelPass> passes, Arr<BevelObservation> observations) =>
        toSeq(observations).Traverse(observation =>
            from pass in passes.Find(row => row.Pass == observation.Pass)
                .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, observation.Pass, "bevel:inspection-pass"))
            from _ in observation.ToBlockExclusive <= pass.Blocks.Count
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, observation.ToBlockExclusive, "bevel:inspection-range"))
            let span = pass.Blocks.Skip(observation.FromBlock).Take(observation.ToBlockExclusive - observation.FromBlock)
            let angle = span.Average(static row => row.AngleDeg)
            let offset = span.Average(static row => row.PreparationOffsetMm)
            let angleDeviation = observation.MeasuredAngleDeg - angle
            let offsetDeviation = observation.MeasuredOffsetMm - offset
            select new BevelInspection(observation, angle, offset, angleDeviation, offsetDeviation)).As();

    private static Fin<BevelPass> Pass(BevelJob job, Loop edge, PassRow pass, int ordinal) =>
        from blocks in Blocks(job, edge, pass, ordinal)
        from _ in Invoke(() => job.Guard(blocks), "bevel:guard")
        from thc in ThcSpan.AdmitSchedule(
            blocks,
            pass.Height,
            job.Policy.Thc,
            job.Policy.Head.Feedable(job.Policy.Budget.SpeedMmPerMin * pass.FeedScale))
        let length = blocks.Zip(blocks.Skip(1))
            .Sum(static pair => pair.Second.PathDistanceMm - pair.First.PathDistanceMm)
        select new BevelPass(
            pass.Pass,
            blocks,
            thc,
            pass.PierceDelaySeconds,
            length,
            blocks.Fold(0.0, static (peak, row) =>
                Math.Max(peak, Math.Sqrt(row.AngleDeg * row.AngleDeg + row.CrossTiltDeg * row.CrossTiltDeg))),
            job.Policy.Budget.Evidence);

    private static Fin<Seq<BevelBlock>> Blocks(BevelJob job, Loop edge, PassRow pass, int ordinal) {
        Polyline<double> path = Native(edge);
        double length = path.PathLength();
        return from geometric in Stations(edge, job.Policy.ChordErrorMm)
               let law = toSeq(job.Policy.Preparation.Stations)
               let prep = law.Zip(law.Skip(1)).Bind(pair => {
                   double delta = Math.Abs(
                       pair.Second.Section.OffsetAt(pass.DepthShare) - pair.First.Section.OffsetAt(pass.DepthShare));
                   int divisions = Math.Max(1, (int)Math.Ceiling(delta / job.Policy.ChordErrorMm));
                   return toSeq(Range(0, divisions + 1)).Map(step =>
                       (pair.First.Station + (pair.Second.Station - pair.First.Station) * step / divisions) * length);
               })
               let stations = toSeq((geometric + prep)
                   .Filter(distance => distance >= 0.0 && distance <= length)
                   .DistinctBy(distance => Math.Round(distance / job.Policy.ChordErrorMm))
                   .OrderBy(static distance => distance))
               from blocks in stations
                   .Map((distance, index) => (Distance: distance, Ordinal: ordinal + index))
                   .TraverseM(row => Block(job, edge, path, length, row.Distance / length, row.Distance, pass, row.Ordinal))
                   .As()
               select blocks;
    }

    private static Fin<BevelBlock> Block(
        BevelJob job,
        Loop edge,
        Polyline<double> path,
        double length,
        double station,
        double distance,
        PassRow pass,
        int ordinal) =>
        from sample in Sample(path, edge, distance)
        from frame in Frame(path, length, sample, station, pass, job)
        from move in Invoke(() => job.Lower(new BevelPoint(
            frame.Point,
            frame.Axis,
            frame.Pivot,
            frame.FeedMmPerMin,
            station,
            pass.Pass)), "bevel:lower")
        select new BevelBlock(
            ordinal,
            move,
            frame.Point,
            pass.Pass,
            station,
            distance,
            sample.Span,
            sample.Bulge,
            frame.OffsetMm,
            frame.Axis,
            frame.Pivot,
            frame.AngleDeg,
            frame.AngleRateDegPerStation,
            frame.CrossTiltDeg,
            frame.FeedMmPerMin,
            frame.CompensationMm);

    private static Fin<BevelFrame> Frame(
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
            return Fin.Fail<BevelFrame>(new GeometryFault.DegenerateInput(Kind.Curve, sample.Span, "bevel:tangent"));
        double offset = job.Policy.Preparation.OffsetAt(station, pass.DepthShare);
        double speed = job.Policy.Budget.SpeedMmPerMin;
        double linearCompensation = job.Policy.Budget.KerfWidthMm * job.Policy.Compensation.KerfGain.DecimalFractions
            + job.Policy.Compensation.Wear.Millimeters;
        double lever = Math.Max(job.Policy.Head.PivotLength.Millimeters, job.Policy.Preparation.ThicknessMm);
        double correction = (job.Policy.Compensation.AngleBias.Degrees
            + job.Policy.Compensation.LagDegPerMeterPerMinute * speed / 1000.0
            + Math.Atan2(linearCompensation, lever) * 180.0 / Math.PI) * job.Policy.Compensation.Mode.AxisShare;
        double throughDelta = Math.Min(0.5, job.Policy.ChordErrorMm / job.Policy.Preparation.ThicknessMm);
        double stationDelta = Math.Min(0.5, job.Policy.ChordErrorMm / length);
        double angle = Tilt(job.Policy.Preparation, station, pass.DepthShare, throughDelta) + correction;
        double crossTilt = job.Policy.Budget.CrossTilt(job.Policy.Compensation).Degrees;
        if (Math.Sqrt(angle * angle + crossTilt * crossTilt) > job.Policy.Head.MaxTilt.Degrees
            || Math.Abs(offset) > job.Policy.Head.ChamferWidth.Millimeters)
            return Fin.Fail<BevelFrame>(new FabricationFault.BevelUnsupported(
                BevelProcess.Subject(job.Policy.Preparation), angle));
        double compensation = linearCompensation
            + job.Policy.Head.CornerRadius.Millimeters * (1.0 - Math.Cos(angle * Math.PI / 180.0));
        Point3d point = sample.Point + normal * (offset + compensation * job.Policy.Compensation.Mode.GeometryShare);
        double rate = TiltRate(job.Policy.Preparation, station, pass.DepthShare, throughDelta, stationDelta);
        double feed = job.Policy.Head.Feedable(speed / (1.0 + Math.Abs(rate)) * pass.FeedScale);
        return job.Policy.Head.Kinematics.Orient(tangent, normal, angle, crossTilt).Map(axis => new BevelFrame(
            point,
            axis,
            point - axis * job.Policy.Head.PivotLength.Millimeters,
            offset,
            angle,
            rate,
            crossTilt,
            feed,
            compensation));
    }

    private static Fin<T> Invoke<T>(Func<Fin<T>> callback, string slot) =>
        Op.Of(name: slot).Catch(callback);

    private static double Tilt(PrepLaw law, double station, double through, double delta) {
        double from = Math.Max(0.0, through - delta);
        double to = Math.Min(1.0, through + delta);
        return Math.Atan2(law.OffsetAt(station, to) - law.OffsetAt(station, from), (to - from) * law.ThicknessMm)
            * 180.0 / Math.PI;
    }

    private static double TiltRate(PrepLaw law, double station, double through, double throughDelta, double stationDelta) {
        double from = Math.Max(0.0, station - stationDelta);
        double to = Math.Min(1.0, station + stationDelta);
        return Math.Abs(Tilt(law, to, through, throughDelta) - Tilt(law, from, through, throughDelta)) / (to - from);
    }

    private static Polyline<double> Native(Loop loop) =>
        new(toSeq(loop.Vertices).Map((point, index) => PlineVertex<double>.FromSlice([point.X, point.Y, loop.BulgeAt(index)])), loop.Closed);

    private static Fin<(Point3d Point, int Span, double Bulge, Vector2<double> Native)> Sample(
        Polyline<double> path,
        Loop source,
        double length) =>
        path.FindPointAtPathLength(length) switch {
            (true, int span, Vector2<double> point, _) => Fin.Succ((
                new Point3d(point.X, point.Y, source.Plane), span, source.BulgeAt(span), point)),
            _ => Fin.Fail<(Point3d, int, double, Vector2<double>)>(new GeometryFault.DegenerateInput(Kind.Curve, None, "bevel:station")),
        };

    private static Fin<Seq<double>> Stations(Loop edge, double chordError) =>
        from sampled in Range(0, edge.Closed ? edge.Count : edge.Count - 1)
            .FoldM<Fin, (double Length, Seq<double> Rows)>((0.0, Seq(0.0)), (state, index) => {
                Point3d from = edge.At(index);
                Point3d to = edge.At(index + 1);
                double chord = from.DistanceTo(to);
                double bulge = Math.Abs(edge.BulgeAt(index));
                if (chord <= edge.Tolerance.Absolute.Value)
                    return Fin.Fail<(double, Seq<double>)>(new GeometryFault.DegenerateInput(Kind.Curve, index, "bevel:edge-span"));
                (double Length, int Divisions) span = Sagitta(chord, bulge, chordError);
                return Fin.Succ((
                    state.Length + span.Length,
                    state.Rows + Range(1, span.Divisions).ToSeq()
                        .Map(step => state.Length + step * span.Length / span.Divisions)));
            }).As()
        select sampled.Rows;

    private static (double Length, int Divisions) Sagitta(double chord, double bulge, double chordError) {
        if (bulge == 0.0)
            return (chord, 1);
        double sweep = 4.0 * Math.Atan(bulge);
        double radius = chord * (1.0 + bulge * bulge) / (4.0 * bulge);
        double maxSweep = 2.0 * Math.Acos(Math.Clamp(1.0 - chordError / radius, -1.0, 1.0));
        return (radius * sweep, maxSweep <= 0.0 ? 1 : Math.Max(1, (int)Math.Ceiling(sweep / maxSweep)));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
