# [COMPUTE_FRAME]

Rasm.Compute frame idealization and its owned-spine solve. It reads the concrete `Rasm.Element` `ElementGraph` directly, folds member axes, the M7-resolved `SectionProperties`, the seam `Mechanical` strengths, and the projected structural edges into one `FrameModel`, lowers that model onto the `Solver/contract` `SolveLane`, and bounds one signed per-combination `MemberResponse` per member. `MemberResponse` is the seam this page publishes: `Analysis/capacity` reads it and this page never learns a design code.

Frame assembly enters the shared lane as `PhysicsKind.FeaStatic` under `LanePolicy.CanonicalStatic` over `SolveRoute.Direct`, so the structural lane assembles and factors through the same CSparse owner the continuum lane holds. A caller sweeping many combinations over one unchanged stiffness threads one `SolveSession`, which the lane re-values instead of re-factorizing.

## [01]-[INDEX]

- [02]-[FRAME_MODEL]: `FrameModel` folds the graph into the analysis idealization its load, support, combination, and policy vocabulary carries.
- [03]-[FRAME_BACKEND]: `Solve` lowers that model onto the owned frame spine and bounds one per-combination `MemberResponse` every limit state reads.

## [02]-[FRAME_MODEL]

- Owner: `FrameModel` the analysis idealization (members, combinations, policy, joint tolerance); `MemberLoad` the per-member applied-action `[Union]` (`Point`/`Uniform`/`Trapezoid`); `DofRestraint` the per-degree-of-freedom restraint reading `[Union]` (`Free`/`Rigid`/`Spring`); `MemberEnd` the `[SmartEnum<string>]` end axis carrying its own endpoint and release SEXTET; `BendingAxis` the `[SmartEnum<string>]` bending-plane axis carrying its local-triad row, its section constants, its end-spring accessor, and its four fixed-end flux slots; `MemberBand` the `[SmartEnum<string>]` gravity-collection band carrying its own derived-action set; `StructuralCase` the load-case row; `MemberSupport` the SEVEN-degree-of-freedom restraint at a member end carrying the stated release and rigid-end offset columns it admits off the connection's own wire, and `SupportFrame` its skewed orientation basis; `LoadCombinationSpec` the factored case map; `StructuralPolicy` the formulation/deflection/station policy carrying the `Formulation` frame `ElementClass` column and the EN 1992 member-scope truss inputs; `StructuralMember` the resolved member; `WindExposureClass`/`LiveLoadClass` the ASCE 7 exposure-profile and live-load vocabularies; `SiteActionPolicy` the per-engagement code-parameter record admitted through one accumulating `Validation`; `ActionDerivation` the load-takedown table; `FrameInputs` the projection shape both structural request cases supply; `DivisorBand` the ONE guarded-quotient operator the frame, capacity, and physics pages all divide through.
- Entry: `static Fin<FrameModel> Project(ElementGraph graph, FrameInputs inputs, GeometrySource geometry)` — folds the input `Targets` member `Node.Object`s into the idealization, reading each member's `StructuralReads.AxisOf` (the analytical line resolved one-hop by content key through the seam `GeometrySource` port off `member.Representations.Axis`), `graph.PropertiesOf(id).Mechanical`, `graph.SectionOf` (the seam Op-free M7 accessor), `StructuralReads.SupportsOf`, and `StructuralReads.LoadsOf`, `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a member lacks a section, a strength, or an axis, and onto `AnalysisFailed(Admission, Input)` when a connection states a PARTIAL release core.
- Auto: self-weight derives per member from `Section.Area.Si × Mechanical.Density.Si × StandardGravity` as a global-down `Uniform` in the `Dead` case; the request's projected `MemberLoad`s supply the applied live/wind/snow/seismic actions, and where a variant carries none `ActionDerivation.Derive` mints them from tributary geometry under one `SiteActionPolicy` — the `MemberBand` row deciding which gravity actions the member collects — so a generated design screens without a human load engineer per variant; `LoadCombinationSpec` factors the cases per code (ASCE 7 / EN 1990) so a combination is data the backend reads; the member's `MaterialFamily` is `Classify`-derived off the seam evidence and validated at `Analysis/capacity` `Check`, never here.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`/`Validation`/`TraverseM`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[UseDelegateFromConstructor]`), Generator.Equals (`[Equatable]`+`[UnorderedEquality]` — the `LoadCombinationSpec` frozen-dictionary latent-trap repair), `Solver/element` (`DofRelease`, `CapabilitySet<DofRelease>` — the stated release vocabulary the member ends and supports carry), Rasm (kernel — `EpsilonPolicy.SeamUlp` the ONE divisor band, `Tolerance`/`ToleranceLane.Joint` the joint-merge band, `PositiveMagnitude`/`Dimension` the policy bands, `Op`), Rasm.Element (project — `ElementGraph`, `Node`, `NodeId`, `Vector3`, `AxisCurve`, `GeometrySource`, `SectionProperties`, `FrameConstants`, `MaterialPropertySet`, `Relationship`, `PropertyName`, `PropertyValue`, `StructuralRows`), BCL inbox (`FrozenDictionary`).
- Growth: a new applied-action kind is one `MemberLoad` case (the lowering's total `Switch` breaks until its fixed-end arm lands); a new restraint reading is one `DofRestraint` case; a new per-support fact one `MemberSupport` column; a new combination basis one `LoadCombinationSpec` row; a new exposure or live-load category one `WindExposureClass`/`LiveLoadClass` row; a new derived action family one weighted arm on the `MemberBand` row's own action delegate.
- Boundary: the section is the M7-resolved seam `SectionProperties` read once off the `ProfileSet` composition (the `VividOrange` `ProfileRef`→section resolution happens once in the `Rasm.Materials` projector, so this runner never re-resolves a profile and Compute admits no VividOrange), and it lowers through the seam's OWN `SectionProperties.Lower()` `FrameConstants` projection — a second local spelling of `Area`/`AvY`/`Iyy` is the deleted fork. Strength is the seam `Mechanical.YieldStrength`/`UltimateStrength`/`YoungsModulus`/`ShearModulus`/`Density`/`PoissonsRatio` (the seam field is `PoissonsRatio`, never `PoissonRatio`). The analytical line is the seam `AxisCurve` (`Start`/`End`/`Up`) content-keyed under `member.Representations.Axis`, never inlined on the node. Supports and loads traverse the projected `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` neutral `Generic` edges by wire-name, so the runner reads the idealization fully baked and never re-reads IFC. Every row name the reader keys resolves to a `Rasm.Element` `StructuralRows` static — a call-site `PropertyName.Create` forks the bag's key space between the projector and this non-referencing reader — and the shape of the read follows the declarer: ONE row per degree of freedom whose `PropertyValue` case carries restraint-versus-spring, and ONE positional `StructuralRows.Frame` list of six direction ratios. `StructuralRows.Dofs` publishes SEVEN rows and the Bim projector stamps `WarpingAxial`, so `MemberSupport` models seven — the widening is COMPUTE-side and the producer already exists.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MemberLoad {
    private MemberLoad() { }
    public sealed record Point(StructuralCase Case, Vector3 Force, Vector3 Moment, double Station) : MemberLoad;
    public sealed record Uniform(StructuralCase Case, Vector3 ForcePerLength) : MemberLoad;
    public sealed record Trapezoid(StructuralCase Case, Vector3 Start, Vector3 End) : MemberLoad;

    public StructuralCase Case => Switch(
        point: static p => p.Case, uniform: static u => u.Case, trapezoid: static t => t.Case);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DofRestraint {
    private DofRestraint() { }
    public sealed record Free : DofRestraint;
    public sealed record Rigid : DofRestraint;
    public sealed record Spring(double RateSi) : DofRestraint;

    public static readonly DofRestraint Released = new Free();
    public static readonly DofRestraint Locked = new Rigid();

    public static Option<DofRestraint> Stated(Option<PropertyValue> row) => row.Map(static value => value switch {
        PropertyValue.Boolean fixity => fixity.Value ? Locked : Released,
        PropertyValue.Measure spring when spring.Value.Si > 0.0 && double.IsFinite(spring.Value.Si) => new Spring(spring.Value.Si),
        _ => Released,
    });

    public static DofRestraint Of(Option<PropertyValue> row) => Stated(row).IfNone(Released);

    public bool Constrains => Switch(free: static _ => false, rigid: static _ => true, spring: static _ => true);

    public Option<double> Rate => Switch<Option<double>>(
        free: static _ => None, rigid: static _ => None, spring: static spring => Some(spring.RateSi));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StructuralCase {
    public static readonly StructuralCase Dead    = new("dead");
    public static readonly StructuralCase Live    = new("live");
    public static readonly StructuralCase Snow    = new("snow");
    public static readonly StructuralCase Wind    = new("wind");
    public static readonly StructuralCase Seismic = new("seismic");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MemberEnd {
    public static readonly MemberEnd Start = new("start", static a => a.Start,
        DofRelease.AxialI, DofRelease.ShearYI, DofRelease.ShearZI, DofRelease.TorsionI, DofRelease.BendingYI, DofRelease.BendingZI);
    public static readonly MemberEnd End   = new("end",   static a => a.End,
        DofRelease.AxialJ, DofRelease.ShearYJ, DofRelease.ShearZJ, DofRelease.TorsionJ, DofRelease.BendingYJ, DofRelease.BendingZJ);

    public DofRelease Axial { get; }
    public DofRelease ShearY { get; }
    public DofRelease ShearZ { get; }
    public DofRelease Torsion { get; }
    public DofRelease BendingY { get; }
    public DofRelease BendingZ { get; }

    public Seq<DofRelease> Rows => Seq(Axial, ShearY, ShearZ, Torsion, BendingY, BendingZ);

    [UseDelegateFromConstructor]
    public partial Vector3 Point(AxisCurve axis);

    public static MemberEnd Of(bool atStart) => atStart ? Start : End;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BendingAxis {
    public static readonly BendingAxis Major = new("major", triadRow: 6, shearI: 2, momentI: 4, shearJ: 8, momentJ: 10,
        static c => c.Iy, static support => support.Ry);
    public static readonly BendingAxis Minor = new("minor", triadRow: 3, shearI: 1, momentI: 5, shearJ: 7, momentJ: 11,
        static c => c.Iz, static support => support.Rz);

    public int TriadRow { get; }
    public int ShearI { get; }
    public int MomentI { get; }
    public int ShearJ { get; }
    public int MomentJ { get; }

    [UseDelegateFromConstructor]
    public partial double Inertia(FrameConstants constants);

    [UseDelegateFromConstructor]
    public partial DofRestraint Restraint(MemberSupport support);

    public Func<Vector3, double> Local(double[] triad) {
        double r0 = triad[TriadRow], r1 = triad[TriadRow + 1], r2 = triad[TriadRow + 2];
        return v => v.X * r0 + v.Y * r1 + v.Z * r2;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MemberBand {
    public static readonly MemberBand Floor = new("floor", static (site, width) =>
        Seq<MemberLoad>(new MemberLoad.Uniform(StructuralCase.Live, new Vector3(0.0, 0.0, -site.LiveLoad.LiveLoadPa * width))));
    public static readonly MemberBand Roof = new("roof", static (site, width) =>
        Seq<MemberLoad>(new MemberLoad.Uniform(StructuralCase.Live, new Vector3(0.0, 0.0, -LiveLoadClass.Roof.LiveLoadPa * width)))
        + site.RoofSnowPa.Map(pf => (MemberLoad)new MemberLoad.Uniform(StructuralCase.Snow, new Vector3(0.0, 0.0, -pf * width))).ToSeq());

    [UseDelegateFromConstructor]
    public partial Seq<MemberLoad> Gravity(SiteActionPolicy site, double tributaryWidthM);

    public static MemberBand Of(StructuralMember member, double topZ, SiteActionPolicy site) =>
        site.RoofBandM.Exists(band => topZ - Math.Max(member.Axis.Start.Z, member.Axis.End.Z) <= band.Value) ? Roof : Floor;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WindExposureClass {
    public static readonly WindExposureClass B = new("b", alpha: 7.0, gradientHeightM: 365.76);
    public static readonly WindExposureClass C = new("c", alpha: 9.5, gradientHeightM: 274.32);
    public static readonly WindExposureClass D = new("d", alpha: 11.5, gradientHeightM: 213.36);

    public double Alpha { get; }
    public double GradientHeightM { get; }

    public double Kz(double heightM) => 2.01 * Math.Pow(Math.Max(heightM, 4.6) / GradientHeightM, 2.0 / Alpha);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LiveLoadClass {
    public static readonly LiveLoadClass Residential = new("residential", liveLoadPa: 1_920.0);
    public static readonly LiveLoadClass Office      = new("office",      liveLoadPa: 2_400.0);
    public static readonly LiveLoadClass Assembly    = new("assembly",    liveLoadPa: 4_790.0);
    public static readonly LiveLoadClass Storage     = new("storage",     liveLoadPa: 6_000.0);
    public static readonly LiveLoadClass Roof        = new("roof",        liveLoadPa: 960.0);

    public double LiveLoadPa { get; }
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class DivisorBand {
    public static double Over(this double numerator, double denominator) =>
        numerator / Math.Max(denominator, EpsilonPolicy.SeamUlp);
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public static partial class StructuralAnalysis {
    const double StandardGravity = 9.80665;

    const int LoweredJointDofs = 6;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SupportFrame(Vector3 Axis, Vector3 Ref);

public sealed record MemberSupport(
    NodeId At, MemberEnd End,
    DofRestraint Dx, DofRestraint Dy, DofRestraint Dz, DofRestraint Rx, DofRestraint Ry, DofRestraint Rz,
    DofRestraint Warp,
    Option<SupportFrame> Frame = default, Option<double> SupportedLengthM = default,
    Option<CapabilitySet<DofRelease>> Releases = default, Option<Vector3> Offset = default) {
    public bool RotationallyFixed => Rx is DofRestraint.Rigid && Ry is DofRestraint.Rigid && Rz is DofRestraint.Rigid;

    public Seq<DofRestraint> Degrees => Seq(Dx, Dy, Dz, Rx, Ry, Rz, Warp);
}

[Equatable]
public sealed partial record LoadCombinationSpec(string Label, [property: UnorderedEquality] FrozenDictionary<StructuralCase, double> Factors) {
    public static readonly LoadCombinationSpec SeismicUnit = new("seismic-unit", FrozenDictionary<StructuralCase, double>.Empty);

    public Option<double> FactorOf(MemberLoad load) =>
        Factors.TryGetValue(load.Case, out double factor) ? Some(factor) : None;
}

public sealed record StructuralPolicy(
    ElementClass Formulation, PositiveMagnitude DeflectionLimitRatio, Dimension StationCount,
    Option<PositiveMagnitude> StirrupSpacing, PositiveMagnitude CotTheta) {
    public static readonly StructuralPolicy Canonical = new(
        ElementClass.Beam2Euler, PositiveMagnitude.Create(1.0 / 250.0), Dimension.Create(11),
        None, PositiveMagnitude.Create(2.5));
}

public sealed record StructuralMember(
    NodeId Id, AxisCurve Axis, SectionProperties Section, MaterialPropertySet.Mechanical Strength,
    Option<MaterialPropertySet.Orthotropic> Directional, Seq<MemberLoad> Loads, Seq<MemberSupport> Supports,
    Option<RcShearLink> ShearLink = default, Option<BucklingCurve> Buckling = default) {
    public double Length => Vector3.Distance(Axis.Start, Axis.End);

    public FrameConstants Constants => Section.Lower();

    public double EffectiveLengthFactor {
        get {
            int fixedEnds = Supports.Count(static s => s.RotationallyFixed);
            return Supports.Count == 1 && fixedEnds == 1 ? 2.0 : fixedEnds >= 2 ? 0.5 : fixedEnds == 1 ? 0.7 : 1.0;
        }
    }

    public Option<MemberSupport> At(MemberEnd end) => Supports.Find(s => s.End == end);
}

public sealed record FrameModel(Seq<StructuralMember> Members, Seq<LoadCombinationSpec> Combinations, StructuralPolicy Policy, Tolerance Joint);

public sealed record SiteActionPolicy(
    PositiveMagnitude BasicWindSpeedMPerS, WindExposureClass Exposure, PositiveMagnitude Kzt, PositiveMagnitude Kd,
    PositiveMagnitude GcpNet, Option<PositiveMagnitude> GroundSnowPa, PositiveMagnitude Ce, PositiveMagnitude Ct,
    PositiveMagnitude SnowImportance, UnitInterval RoofSlopeFactor, LiveLoadClass LiveLoad,
    PositiveMagnitude TributaryWidthM, Option<PositiveMagnitude> RoofBandM) {
    public static Validation<Error, SiteActionPolicy> Of(
        double basicWindSpeedMPerS, WindExposureClass exposure, double kzt, double kd, double gcpNet,
        Option<double> groundSnowPa, double ce, double ct, double snowImportance, double roofSlopeFactor,
        LiveLoadClass liveLoad, double tributaryWidthM, Option<double> roofBandM, Op key) =>
        (Magnitude(basicWindSpeedMPerS, key), Magnitude(kzt, key), Magnitude(kd, key), Magnitude(gcpNet, key),
         groundSnowPa.Traverse(value => Magnitude(value, key)).As(), Magnitude(ce, key), Magnitude(ct, key),
         Magnitude(snowImportance, key), key.AcceptValidated<UnitInterval>(candidate: roofSlopeFactor).ToValidation(),
         Magnitude(tributaryWidthM, key), roofBandM.Traverse(value => Magnitude(value, key)).As())
        .Apply((wind, kzt, kd, gcp, snow, ce, ct, importance, slope, width, band) =>
            new SiteActionPolicy(wind, exposure, kzt, kd, gcp, snow, ce, ct, importance, slope, liveLoad, width, band))
        .As();

    static Validation<Error, PositiveMagnitude> Magnitude(double value, Op key) =>
        key.AcceptValidated<PositiveMagnitude>(candidate: value).ToValidation();

    public static readonly Validation<Error, SiteActionPolicy> Canonical = Of(
        basicWindSpeedMPerS: 51.0, WindExposureClass.C, kzt: 1.0, kd: 0.85, gcpNet: 0.8,
        groundSnowPa: Some(1_000.0), ce: 1.0, ct: 1.0, snowImportance: 1.0, roofSlopeFactor: 1.0,
        LiveLoadClass.Office, tributaryWidthM: 3.0, roofBandM: Some(0.5), Op.Of(name: nameof(Canonical)));

    public double VelocityPressurePa(double heightM) =>
        0.613 * Exposure.Kz(heightM) * Kzt.Value * Kd.Value * BasicWindSpeedMPerS.Value * BasicWindSpeedMPerS.Value;

    public Option<double> RoofSnowPa =>
        GroundSnowPa.Map(pg => 0.7 * Ce.Value * Ct.Value * SnowImportance.Value * pg.Value * RoofSlopeFactor.Value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ActionDerivation {
    const double HorizontalCosine = 0.9;

    public static Seq<MemberLoad> Derive(StructuralMember member, MemberBand band, SiteActionPolicy site) {
        double width = site.TributaryWidthM.Value;
        double meanHeight = 0.5 * (member.Axis.Start.Z + member.Axis.End.Z);
        double run = Math.Sqrt(Math.Pow(member.Axis.End.X - member.Axis.Start.X, 2.0) + Math.Pow(member.Axis.End.Y - member.Axis.Start.Y, 2.0));
        Seq<MemberLoad> wind = Seq<MemberLoad>(new MemberLoad.Uniform(
            StructuralCase.Wind, new Vector3(site.VelocityPressurePa(meanHeight) * site.GcpNet.Value * width, 0.0, 0.0)));
        return run.Over(member.Length) >= HorizontalCosine ? wind + band.Gravity(site, width) : wind;
    }
}

public readonly record struct FrameInputs(Seq<NodeId> Targets, Seq<LoadCombinationSpec> Combinations, StructuralPolicy Policy, Option<SiteActionPolicy> Site) {
    public static FrameInputs Of(AssessmentRequest.Structural request) => new(request.Targets, request.Combinations, request.Policy, request.Site);
    public static FrameInputs Of(AssessmentRequest.Seismic request) => new(request.Targets, Seq(LoadCombinationSpec.SeismicUnit), request.Policy, request.Site);
}

public static partial class StructuralAnalysis {
    public static Fin<FrameModel> Project(ElementGraph graph, FrameInputs inputs, GeometrySource geometry) =>
        inputs.Targets.TraverseM(id =>
            from axis     in graph.AxisOf(id, geometry)
            from strength in graph.PropertiesOf(id).Mechanical.ToFin(Missing(AssessmentInputReason.MeasureAbsent, $"mechanical:{id.Value}"))
            from section  in graph.SectionOf(id).ToFin(Missing(AssessmentInputReason.MeasureAbsent, $"section:{id.Value}"))
            let directional = graph.PropertiesOf(id).Orthotropic
            let shearLink = graph.ShearLinkOf(id)
            let buckling  = graph.BucklingOf(id)
            let selfWeight = new MemberLoad.Uniform(StructuralCase.Dead,
                new Vector3(0d, 0d, -(section.Area.Si * strength.Density.Si * StandardGravity)))
            from supports in graph.SupportsOf(id)
            select new StructuralMember(
                id, axis, section, strength, directional, graph.LoadsOf(id).Add(selfWeight), supports, shearLink, buckling)).As()
            .Map(members => DeriveAbsent(members, inputs.Site))
            .Bind(members => Tolerance.Of(ToleranceLane.Joint, graph.Header.Tolerance, ProjectKey)
                .Map(joint => new FrameModel(members, inputs.Combinations, inputs.Policy, joint)));

    static readonly Op ProjectKey = Op.Of(name: nameof(Project));

    static Seq<StructuralMember> DeriveAbsent(Seq<StructuralMember> members, Option<SiteActionPolicy> declared) =>
        declared.Match(
            None: () => members,
            Some: site => {
                double top = members.Map(static m => Math.Max(m.Axis.Start.Z, m.Axis.End.Z)).Max(double.NegativeInfinity);
                return members.Map(member => member.Loads.Count > 1
                    ? member
                    : member with { Loads = member.Loads + ActionDerivation.Derive(member, MemberBand.Of(member, top, site), site) });
            });

    static ComputeFault Missing(AssessmentInputReason reason, string witness) =>
        new ComputeFault.AssessmentInputMissing(reason, witness);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class StructuralReads {
    const string ConnectsMember   = "IfcRelConnectsStructuralMember";
    const string ConnectsActivity = "IfcRelConnectsStructuralActivity";

    public static Fin<AxisCurve> AxisOf(this ElementGraph graph, NodeId member, GeometrySource geometry) =>
        graph.Find<Node.Object>(member).Bind(o => geometry.Axis(o.Representations))
            .ToFin(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.MemberInputAbsent, $"axis:{member.Value}"));

    public static Option<RcShearLink> ShearLinkOf(this ElementGraph graph, NodeId member) =>
        from area in MeasuredRow(graph, member, StructuralRows.ShearLinkArea)
        from fywd in MeasuredRow(graph, member, StructuralRows.ShearLinkYield)
        from ceiling in MeasuredRow(graph, member, StructuralRows.ShearLinkCeiling)
        select new RcShearLink(area, fywd, ceiling);

    public static Option<BucklingCurve> BucklingOf(this ElementGraph graph, NodeId member) =>
        from alpha in NumberRow(graph, member, StructuralRows.BucklingAlpha)
        from plateau in NumberRow(graph, member, StructuralRows.BucklingPlateau)
        select new BucklingCurve(alpha, plateau);

    static Option<double> MeasuredRow(ElementGraph graph, NodeId owner, PropertyName row) =>
        BagRow(graph, owner, row).Bind(static value => value is PropertyValue.Measure m ? Some(m.Value.Si) : Option<double>.None);

    static Option<double> NumberRow(ElementGraph graph, NodeId owner, PropertyName row) =>
        BagRow(graph, owner, row).Bind(static value => value is PropertyValue.Number n ? Some(n.Value) : Option<double>.None);

    static Option<PropertyValue> BagRow(ElementGraph graph, NodeId owner, PropertyName row) =>
        toSeq(graph.EdgesAt(owner))
            .Filter(e => e.Kind == RelationshipKind.Assign && e.Relating == owner)
            .Choose(e => graph.Find(e.Related))
            .Choose(node => node is Node.PropertySet set ? set.Bag.Find(row) : Option<PropertyValue>.None)
            .Head;

    public static Fin<Seq<MemberSupport>> SupportsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member)
            .Choose(e => e is Relationship.Generic g && g.WireName == ConnectsMember && g.Relating == member ? Some(g) : None)
            .ToSeq()
            .TraverseM(g => {
                MemberEnd end = MemberEnd.Of(g.Flag(StructuralRows.AtStart));
                return ReleasesOf(g, end, member).Map(releases => new MemberSupport(
                    g.Related, end,
                    Dof(g, StructuralRows.Translation["X"]), Dof(g, StructuralRows.Translation["Y"]), Dof(g, StructuralRows.Translation["Z"]),
                    Dof(g, StructuralRows.Rotation["X"]), Dof(g, StructuralRows.Rotation["Y"]), Dof(g, StructuralRows.Rotation["Z"]),
                    Dof(g, StructuralRows.Warping["Axial"]),
                    FrameOf(g), g.Magnitude(StructuralRows.SupportedLength), releases, OffsetOf(g)));
            }).As();

    public static Seq<MemberLoad> LoadsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsActivity && g.Relating == member
            ? Some(ToLoad(g)) : None).ToSeq();

    static MemberLoad ToLoad(Relationship.Generic g) => Kind(g) switch {
        "uniform"   => new MemberLoad.Uniform(CaseOf(g), Vec(g, StructuralRows.Force)),
        "trapezoid" => new MemberLoad.Trapezoid(CaseOf(g), Vec(g, StructuralRows.Start), Vec(g, StructuralRows.End)),
        _           => new MemberLoad.Point(CaseOf(g), Vec(g, StructuralRows.Force), Vec(g, StructuralRows.Moment), g.Magnitude(StructuralRows.Station).IfNone(0.5)),
    };

    static string Kind(Relationship.Generic g) => g.Text(StructuralRows.LoadKind).IfNone("point");

    static StructuralCase CaseOf(Relationship.Generic g) =>
        g.Text(StructuralRows.Case)
            .Bind(static value => StructuralCase.TryGet(value, out StructuralCase c) ? Some(c) : None)
            .IfNone(StructuralCase.Live);

    static DofRestraint Dof(Relationship.Generic g, PropertyName row) => DofRestraint.Of(g.Attribute(row));

    static Option<DofRestraint> Stated(Relationship.Generic g, PropertyName row) => DofRestraint.Stated(g.Attribute(row));

    static Fin<Option<CapabilitySet<DofRelease>>> ReleasesOf(Relationship.Generic g, MemberEnd end, NodeId member) {
        Seq<Option<DofRestraint>> core = StructuralRows.ReleaseCore.Map(row => Stated(g, row));
        return core.ForAll(static reading => reading.IsNone)
            ? Fin.Succ(Option<CapabilitySet<DofRelease>>.None)
            : core.Traverse(identity).As()
                .Map(readings => CapabilitySet<DofRelease>.Of([.. readings.Zip(end.Rows)
                    .Filter(static pair => pair.Item1 is DofRestraint.Free).Map(static pair => pair.Item2)]))
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input,
                    $"<frame-release-partial:{member.Value}:{end.Key}>"))
                .Map(static column => Some(column));
    }

    static Option<Vector3> OffsetOf(Relationship.Generic g) =>
        from x in g.Magnitude(StructuralRows.Offset["X"])
        from y in g.Magnitude(StructuralRows.Offset["Y"])
        from z in g.Magnitude(StructuralRows.Offset["Z"])
        select new Vector3(x, y, z);

    static Option<SupportFrame> FrameOf(Relationship.Generic g) =>
        g.Attribute(StructuralRows.Frame)
            .Bind(static value => value is PropertyValue.List list
                ? list.Values.Traverse(static ratio => ratio is PropertyValue.Number number ? Some(number.Value) : None).As()
                : None)
            .Bind(static ratios => ratios.ToArray() is [var ax, var ay, var az, var rx, var ry, var rz]
                ? Some(new SupportFrame(new Vector3(ax, ay, az), new Vector3(rx, ry, rz)))
                : None);

    static Vector3 Vec(Relationship.Generic g, Map<string, PropertyName> family) =>
        new(g.Si(family["X"]), g.Si(family["Y"]), g.Si(family["Z"]));
}
```

## [03]-[FRAME_BACKEND]

- Owner: the `Solve` owned-spine route — `FrameModel` lowers onto the `Solver/contract#SOLVE_CONTRACT` `SolveLane` over the `Solver/element` frame `ElementClass` rows (`beam2-euler`/`beam2-timoshenko`, the `StructuralPolicy.Formulation` column), the owned rows carrying end releases by static condensation, rigid-end offsets by eccentricity transform, and semi-rigid end springs as row behavior; `SectionDemand` the signed per-station internal-force sample; `MemberResponse` the SIGNED two-extreme bound with deflection every limit state reads; `FrameLowering` the model→mesh projection; `StationRecovery` the per-member station fold off the solved displacement field.
- Entry: `static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model, IClock clock, Option<SolveSession> session = default)` — lowers the model once, then per `LoadCombinationSpec` scales the case actions, solves through `SolveLane.Solve` under `LanePolicy.CanonicalStatic` over `SolveRoute.Direct`, recovers the worst-station `SectionDemand` and transverse deflection per member, and folds `StationRecovery.Envelope` across combinations. The optional `SolveSession` is the STANDING factorization the lane re-values per combination: the stiffness is unchanged across a combination sweep and only the right-hand side moves, so a caller that opens one pays the symbolic phase once for the whole sweep and one that passes `None` keeps the per-combination factor. The parameter is where the reuse SEATS — the session itself is minted from an assembled operator this runner never holds, so the lane owes the mint and this entry is already shaped to take it. `Fin<T>` lowers a singular or ill-conditioned factorization onto the typed `ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, …)` — deterministic, cached by the spine, never re-run blind.
- Auto: joints merge by tolerance-quantized coordinate (never fragile exact-float `Vector3` equality) and the merge yields a `Fin<long>` address, so an unresolvable joint rails by name where a `-1` sentinel once became a negative DOF address; each `MemberSupport` lowers its `Degrees` projection to the `BoundaryCondition.Dirichlet` constraint set on its endpoint-resolved shared joint, `DofRestraint.Constrains` selecting the slots; each `MemberLoad` case lowers through a TOTAL `Switch` to its fixed-end equivalent nodal actions (Point by the closed-form ab²/L² pair, Uniform by wL/2 + wL²/12, Trapezoid by the exact linear-varying closed form) landing as `Neumann` rows; per-station recovery reads the solved field back through each member's local frame — end displacements gathered and rotated local, local end forces `f = k_l·u_l − f_fixed`, station N/V/M by statics, station transverse deflection by the Hermite end-displacement interpolation with the span-load particular deflection — so the `Deflection` limit state is a REAL displacement check.
- Packages: `Solver/contract` (`SolveLane`, `LanePolicy.CanonicalStatic`, `SolveRoute.Direct`, `SolveProblem`, `SolveResult`, `SolveSession`, `MaterialField`), `Solver/element` (`ElementClass`, `FrameMember`, `DofRelease`, `CellQuality`), `Solver/field` (`DiscreteMesh`), `Solver/assembly` (`BoundaryCondition`), Rasm (kernel — `QuadratureRule.Line2`, `EpsilonPolicy`), CommunityToolkit.HighPerformance (`MemoryOwner<T>`/`SpanOwner<T>`/`Span2D<T>` — every mesh, flux, and station buffer), Rasm.Element (project — `SectionProperties`/`FrameConstants`), BCL inbox (`TensorPrimitives`).
- Growth: a new frame formulation is one `ElementClass` frame row; a new end condition a column on `FrameMember` the closed form reads; a new load kind one `MemberLoad` case with one fixed-end arm on the total `Switch`; the response bound is one `MemberResponse` shape the checks read regardless of formulation — an external FE backend beside the owned spine is the rejected duplicate-mechanism form.
- Boundary: the frame solve is the `Solver/contract` spine — one `SolveLane`, one CSparse factorization owner, one `MaterialField` elasticity admission — and a hand-rolled stiffness assembler beside it is the rejected form; member releases, rigid-end offsets, and semi-rigid springs are ROW BEHAVIOR on the `ElementClass` frame closed form, and all three columns are FED here (`Releases` off the connection's own STATED release core, which the row REFUSES by name when a support leaves it unstated rather than folding `Empty`; `OffsetI`/`OffsetJ` off the connection's stated rigid-end `Offset` vector, its along-axis ordinate, an unstamped offset being the honest zero rigid end; the springs off the rotational rates) rather than defaulted; the local frame orders moments `(T=torsion about x, My/Mz=bending)` and the demand maps `SectionDemand(N, Vy, Vz, My, Mz, T)` off the local end-force vector — never a torsion/bending swap; `MemberResponse` keeps BOTH signed extremes per component, so a sense-selecting limit state reads the extreme its own capacity bounds and an `|magnitude|` fold reporting a tension-carrying member as untensioned cannot form; a singular system surfaces as the typed `(Solve, Numeric)` `AnalysisFailed`, never an exception crossing the rail. The joint address is `joint × LoweredJointDofs + slot` over SIX lowered degrees while `MemberSupport` models seven: the warping restraint is a NAMED lowering, dropped because the landed frame rows carry a 12-DOF member block with no warping ordinate and no `Iw` column, and the fact is stamped and read here so the widening is one row change with the producer already in place. The TRANSVERSE rigid-end offset is the second named lowering: the connection states a whole `Offset` vector while `FrameMember` carries one scalar per end, so the Y/Z pair is dropped BY NAME rather than folded into the along-axis ordinate, and the seventh release rides the same law — `StructuralRows.ReleaseWarping` sits outside the stated core the declarer admits whole, so its silence lowers beside the warping restraint and never voids the six.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SectionDemand(double N, double Vy, double Vz, double My, double Mz, double T) {
    public static readonly SectionDemand Zero = new(0, 0, 0, 0, 0, 0);

    public SectionDemand Lower(SectionDemand b) => new(
        Math.Min(N, b.N), Math.Min(Vy, b.Vy), Math.Min(Vz, b.Vz), Math.Min(My, b.My), Math.Min(Mz, b.Mz), Math.Min(T, b.T));
    public SectionDemand Upper(SectionDemand b) => new(
        Math.Max(N, b.N), Math.Max(Vy, b.Vy), Math.Max(Vz, b.Vz), Math.Max(My, b.My), Math.Max(Mz, b.Mz), Math.Max(T, b.T));

    public static SectionDemand operator -(SectionDemand d) => new(-d.N, -d.Vy, -d.Vz, -d.My, -d.Mz, -d.T);

    public void WriteRow(Span<double> row, Func<double, double> quantize) {
        row[0] = quantize(N); row[1] = quantize(Vy); row[2] = quantize(Vz);
        row[3] = quantize(My); row[4] = quantize(Mz); row[5] = quantize(T);
    }

    public const int Columns = 6;
}

public readonly record struct MemberResponse(SectionDemand Min, SectionDemand Max, double MaxDeflection) {
    public static readonly MemberResponse Zero = new(SectionDemand.Zero, SectionDemand.Zero, 0.0);

    public MemberResponse Absorb(SectionDemand d, double deflection) =>
        new(Min.Lower(d), Max.Upper(d), Math.Max(MaxDeflection, Math.Abs(deflection)));
    public MemberResponse Merge(MemberResponse b) =>
        new(Min.Lower(b.Min), Max.Upper(b.Max), Math.Max(MaxDeflection, b.MaxDeflection));

    public double Span(Func<SectionDemand, double> component) =>
        Math.Max(Math.Abs(component(Min)), Math.Abs(component(Max)));

    public SectionDemand TensionCorner => Corner(Math.Max(Max.N, 0.0));
    public SectionDemand CompressionCorner => Corner(Math.Min(Min.N, 0.0));

    SectionDemand Corner(double n) => new(n,
        Span(static d => d.Vy), Span(static d => d.Vz), Span(static d => d.My), Span(static d => d.Mz), Span(static d => d.T));

    public const int Columns = (2 * SectionDemand.Columns) + 1;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class StructuralAnalysis {
    static readonly Op SolveKey = Op.Of(name: nameof(Solve));

    public static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model, IClock clock, Option<SolveSession> session = default) =>
        model.Members.IsEmpty
            ? Fin.Succ(FrozenDictionary<NodeId, MemberResponse>.Empty)
            : FrameLowering.Lower(model, clock).Bind(lowered =>
                model.Combinations.Fold(
                    Fin.Succ(model.Members.Map(static m => (m.Id, Response: MemberResponse.Zero)).ToFrozenDictionary(static p => p.Id, static p => p.Response)),
                    (acc, combo) => acc.Bind(envelope =>
                        lowered.Problem(PhysicsKind.FeaStatic, combo).Bind(problem =>
                            SolveLane.Solve(problem, lowered.Mesh, LanePolicy.CanonicalStatic, new SolveRoute.Direct(), clock, session: session)
                                .Bind(solution => StationRecovery.Envelope(model, lowered, combo, solution.Field, envelope))))));

    internal sealed record FrameLowered(
        DiscreteMesh Mesh, ImmutableArray<FrameMember> Members, MaterialField Field,
        Func<LoadCombinationSpec, Fin<Seq<BoundaryCondition>>> Conditions, Func<StructuralMember, Fin<(long I, long J)>> EndJoints) {
        public Fin<SolveProblem> Problem(PhysicsKind physics, LoadCombinationSpec combo) =>
            Conditions(combo).Bind(conditions =>
                SolveProblem.Of(physics, Mesh, conditions, Field, new PhysicsPayload.Continuum(), Members, None));
    }

    internal static class FrameLowering {
        static (long X, long Y, long Z) Quantized(Vector3 point, Tolerance joint) =>
            ((long)Math.Round(point.X / joint.Value), (long)Math.Round(point.Y / joint.Value), (long)Math.Round(point.Z / joint.Value));

        public static Fin<FrameLowered> Lower(FrameModel model, IClock clock) =>
            MaterialField.OfMechanical(model.Members.Map(static m => Some(m.Strength)))
                .Bind(field => {
                    (Seq<Vector3> joints, Func<(long, long, long), Fin<long>> jointOf) = Joints(model);
                    return from mesh in Mesh(model, joints, jointOf, clock)
                           from rows in model.Members.TraverseM(Row).As()
                           select new FrameLowered(
                               mesh,
                               [.. rows],
                               field,
                               combo => Conditions(model, jointOf, combo),
                               member => from i in jointOf(Quantized(member.Axis.Start, model.Joint))
                                         from j in jointOf(Quantized(member.Axis.End, model.Joint))
                                         select (i, j));
                });

        static Fin<FrameMember> Row(StructuralMember m) =>
            Releases(m).Map(releases => {
                FrameConstants c = m.Constants;
                return new FrameMember(
                    c.Area, c.Iy, c.Iz, c.Torsion, Iw: c.Warping,
                    UpX: m.Axis.Up.X, UpY: m.Axis.Up.Y, UpZ: m.Axis.Up.Z,
                    Releases: releases,
                    OffsetI: Offset(m, MemberEnd.Start), OffsetJ: Offset(m, MemberEnd.End),
                    SpringYi: Spring(m, MemberEnd.Start, BendingAxis.Major), SpringZi: Spring(m, MemberEnd.Start, BendingAxis.Minor),
                    SpringYj: Spring(m, MemberEnd.End, BendingAxis.Major), SpringZj: Spring(m, MemberEnd.End, BendingAxis.Minor),
                    ShearAreaY: c.ShearAreaY, ShearAreaZ: c.ShearAreaZ);
            });

        static Fin<CapabilitySet<DofRelease>> Releases(StructuralMember m) =>
            m.Supports.TraverseM(s => s.Releases.ToFin(Unstated(m, s.End))).As()
                .Map(static columns => columns.Fold(CapabilitySet<DofRelease>.Empty, static (all, column) => all | column));

        static ComputeFault Unstated(StructuralMember m, MemberEnd end) =>
            new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input,
                $"<frame-release-unstated:{m.Id.Value}:{end.Key}>");

        static double Offset(StructuralMember m, MemberEnd end) =>
            m.At(end).Bind(static s => s.Offset).Map(static o => o.X).IfNone(0.0);

        static double Spring(StructuralMember m, MemberEnd end, BendingAxis axis) =>
            m.At(end).Bind(support => axis.Restraint(support).Rate).IfNone(double.PositiveInfinity);

        static (Seq<Vector3> Joints, Func<(long, long, long), Fin<long>> JointOf) Joints(FrameModel model) {
            (Seq<Vector3> Order, Map<(long, long, long), long> Map) index =
                model.Members.Bind(static m => Seq(m.Axis.Start, m.Axis.End)).Fold(
                    (Order: Seq<Vector3>(), Map: Map<(long, long, long), long>()),
                    (acc, p) => acc.Map.ContainsKey(Quantized(p, model.Joint))
                        ? acc
                        : (acc.Order.Add(p), acc.Map.Add(Quantized(p, model.Joint), acc.Order.Count)));
            return (index.Order, key => index.Map.Find(key).ToFin(
                new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<frame-joint-unmerged:{key}>")));
        }

        static Fin<DiscreteMesh> Mesh(FrameModel model, Seq<Vector3> joints, Func<(long, long, long), Fin<long>> jointOf, IClock clock) =>
            model.Members.TraverseM(member =>
                from i in jointOf(Quantized(member.Axis.Start, model.Joint))
                from j in jointOf(Quantized(member.Axis.End, model.Joint))
                select (I: i, J: j)).As()
                .Map(ends => {
                    float[] nodes = new float[joints.Count * 3];
                    long[] connectivity = new long[model.Members.Count * 2];
                    Span2D<float> points = nodes.AsSpan().AsSpan2D(joints.Count, 3);
                    Vector3[] ordered = joints.ToArray();
                    for (int row = 0; row < ordered.Length; row++) {
                        points[row, 0] = (float)ordered[row].X; points[row, 1] = (float)ordered[row].Y; points[row, 2] = (float)ordered[row].Z;
                    }
                    Span2D<long> cells = connectivity.AsSpan().AsSpan2D(model.Members.Count, 2);
                    (long I, long J)[] pairs = ends.ToArray();
                    for (int cell = 0; cell < pairs.Length; cell++) { cells[cell, 0] = pairs[cell].I; cells[cell, 1] = pairs[cell].J; }
                    return new DiscreteMesh(
                        model.Policy.Formulation, MeshAlgorithm.Sweep, QuadratureRule.Line2, nodes, connectivity,
                        NodeCount: joints.Count, ElementCount: model.Members.Count, BoundaryCount: 0,
                        BoundaryLayers: 0, RefineLevel: 0,
                        Metric: CellQuality.ScaledJacobian, WorstQuality: 1.0, ErrorEstimate: None, At: clock.GetCurrentInstant());
                });

        static Fin<Seq<BoundaryCondition>> Conditions(FrameModel model, Func<(long, long, long), Fin<long>> jointOf, LoadCombinationSpec combo) =>
            (from supports in model.Members.TraverseM(m => m.Supports.TraverseM(s =>
                 jointOf(Quantized(s.End.Point(m.Axis), model.Joint)).Map(joint => {
                     Seq<long> dofs = s.Degrees.Take(LoweredJointDofs)
                         .Map(static (dof, slot) => (Dof: dof, Slot: slot))
                         .Choose(row => row.Dof.Constrains ? Some((joint * LoweredJointDofs) + row.Slot) : None);
                     return (BoundaryCondition)new BoundaryCondition.Dirichlet(FieldStation.Nodal, [.. dofs], [.. dofs.Map(static _ => 0.0)]);
                 })).As()).As()
             from actions in model.Members.TraverseM(m => m.Loads
                 .Choose(load => combo.FactorOf(load).Map(factor => (Load: load, Factor: factor)))
                 .TraverseM(row => LoadCondition(m, model.Joint, jointOf, row.Load, row.Factor)).As()).As()
             select supports.Bind(identity) + actions.Bind(identity)).As();

        static Fin<BoundaryCondition> LoadCondition(StructuralMember member, Tolerance joint, Func<(long, long, long), Fin<long>> jointOf, MemberLoad load, double factor) =>
            from i in jointOf(Quantized(member.Axis.Start, joint))
            from j in jointOf(Quantized(member.Axis.End, joint))
            select Action(member, load, factor, i, j);

        static BoundaryCondition Action(StructuralMember member, MemberLoad load, double factor, long i, long j) {
            using MemoryOwner<double> scratch = MemoryOwner<double>.Allocate(12, AllocationMode.Clear);
            Memory<double> flux = scratch.Memory;
            double[] triad = LocalTriad(member);
            double length = member.Length;
            Func<Vector3, double> axial = Axial(triad);
            Seq<(BendingAxis Axis, Func<Vector3, double> Local)> planes =
                toSeq(BendingAxis.Items).Map(axis => (Axis: axis, Local: axis.Local(triad)));
            foreach ((BendingAxis axis, Func<Vector3, double> local) in planes) {
                double[] ends = FixedEnd(load, length, local).EndForces;
                Span<double> slots = flux.Span;
                slots[axis.ShearI] += ends[0]; slots[axis.MomentI] += ends[1];
                slots[axis.ShearJ] += ends[2]; slots[axis.MomentJ] += ends[3];
            }
            load.Switch(
                point: p => {
                    Span<double> slots = flux.Span;
                    double n = axial(p.Force), torque = axial(p.Moment);
                    slots[0] += n * (1.0 - p.Station); slots[6] += n * p.Station;
                    slots[3] += torque * (1.0 - p.Station); slots[9] += torque * p.Station;
                    double a = p.Station * length, b = length - a, l2 = length * length;
                    foreach ((BendingAxis axis, Func<Vector3, double> local) in planes) {
                        double m0 = local(p.Moment);
                        slots[axis.ShearI] += -6.0 * m0 * a * b / (l2 * length);
                        slots[axis.ShearJ] += 6.0 * m0 * a * b / (l2 * length);
                        slots[axis.MomentI] += m0 * b * ((2.0 * a) - b) / l2;
                        slots[axis.MomentJ] += m0 * a * ((2.0 * b) - a) / l2;
                    }
                },
                uniform: u => { double w = axial(u.ForcePerLength) * length; flux.Span[0] += w / 2.0; flux.Span[6] += w / 2.0; },
                trapezoid: t => { double w = (axial(t.Start) + axial(t.End)) / 2.0 * length; flux.Span[0] += w / 2.0; flux.Span[6] += w / 2.0; });
            TensorPrimitives.Multiply(flux.Span, factor, flux.Span);
            return new BoundaryCondition.Neumann(
                [(i * LoweredJointDofs) + 0, (i * LoweredJointDofs) + 1, (i * LoweredJointDofs) + 2,
                 (i * LoweredJointDofs) + 3, (i * LoweredJointDofs) + 4, (i * LoweredJointDofs) + 5,
                 (j * LoweredJointDofs) + 0, (j * LoweredJointDofs) + 1, (j * LoweredJointDofs) + 2,
                 (j * LoweredJointDofs) + 3, (j * LoweredJointDofs) + 4, (j * LoweredJointDofs) + 5],
                flux.ToArray());
        }

        static Func<Vector3, double> Axial(double[] triad) {
            double r0 = triad[0], r1 = triad[1], r2 = triad[2];
            return v => (v.X * r0) + (v.Y * r1) + (v.Z * r2);
        }

        internal static double[] LocalTriad(StructuralMember member) {
            Span<double> r = stackalloc double[9];
            Topology.Triad(
                member.Axis.End.X - member.Axis.Start.X, member.Axis.End.Y - member.Axis.Start.Y, member.Axis.End.Z - member.Axis.Start.Z,
                member.Axis.Up.X, member.Axis.Up.Y, member.Axis.Up.Z, r);
            return r.ToArray();
        }

        public static (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) FixedEnd(MemberLoad load, double length, Func<Vector3, double> local) =>
            load.Switch(
                point: p => {
                    double magnitude = local(p.Force), a = p.Station * length, b = length - a, l2 = length * length;
                    double mi = magnitude * a * b * b / l2, mj = -magnitude * a * a * b / l2;
                    double vi = magnitude * b * b * (length + (2.0 * a)) / (l2 * length);
                    return (new[] { vi, mi, magnitude - vi, mj },
                        (Func<double, double>)(x => x < a ? 0.0 : -magnitude * (x - a)),
                        (Func<double, double>)(x => x < a ? 0.0 : magnitude * Math.Pow(x - a, 3) / 6.0));
                },
                uniform: u => {
                    double w = local(u.ForcePerLength), l2 = length * length;
                    return (new[] { w * length / 2.0, w * l2 / 12.0, w * length / 2.0, -w * l2 / 12.0 },
                        (Func<double, double>)(x => -w * x * x / 2.0),
                        (Func<double, double>)(x => w * Math.Pow(x, 4) / 24.0));
                },
                trapezoid: t => {
                    double w1 = local(t.Start), w2 = local(t.End), l2 = length * length;
                    double vi = length * ((7.0 * w1) + (3.0 * w2)) / 20.0, vj = length * ((3.0 * w1) + (7.0 * w2)) / 20.0;
                    double mi = l2 * ((w1 / 20.0) + (w2 / 30.0)), mj = -l2 * ((w1 / 30.0) + (w2 / 20.0));
                    return (new[] { vi, mi, vj, mj },
                        (Func<double, double>)(x => -((w1 * x * x / 2.0) + ((w2 - w1) * x * x * x / (6.0 * length)))),
                        (Func<double, double>)(x => (w1 * Math.Pow(x, 4) / 24.0) + ((w2 - w1) * Math.Pow(x, 5) / (120.0 * length))));
                });
    }

    internal static class StationRecovery {
        public static Fin<FrozenDictionary<NodeId, MemberResponse>> Envelope(FrameModel model, FrameLowered lowered, LoadCombinationSpec combo, ReadOnlyMemory<double> field, FrozenDictionary<NodeId, MemberResponse> prior) =>
            Demands(model, lowered, combo, field).Map(rows => rows
                .Map(row => (row.Id, Response: prior[row.Id].Merge(row.Response)))
                .ToFrozenDictionary(static row => row.Id, static row => row.Response));

        public static Fin<Seq<(NodeId Id, MemberResponse Response)>> Demands(FrameModel model, FrameLowered lowered, LoadCombinationSpec combo, ReadOnlyMemory<double> field) =>
            model.Members.TraverseM(member => lowered.EndJoints(member)
                .Map(ends => (member.Id, March(model, member, combo, field, ends)))).As();

        static MemberResponse March(FrameModel model, StructuralMember member, LoadCombinationSpec combo, ReadOnlyMemory<double> field, (long I, long J) ends) {
            ReadOnlySpan<double> u = field.Span;
            double length = member.Length;
            double[] r = FrameLowering.LocalTriad(member);
            double[] li = Localized(u, ends.I, r), lj = Localized(u, ends.J, r);
            FrameConstants c = member.Constants;
            double axial = (lj[0] - li[0]).Over(length) * member.Strength.YoungsModulus.Si * c.Area;
            double torsion = (lj[3] - li[3]).Over(length) * member.Strength.ShearModulus.Si * c.Torsion;
            Seq<(double Factor, (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) Action)> Actions(BendingAxis axis) =>
                member.Loads.Choose(load => combo.FactorOf(load).Map(factor => (factor, FrameLowering.FixedEnd(load, length, axis.Local(r)))));
            Seq<(double Factor, (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) Action)> major = Actions(BendingAxis.Major), minor = Actions(BendingAxis.Minor);
            double eiMajor = member.Strength.YoungsModulus.Si * BendingAxis.Major.Inertia(c);
            double eiMinor = member.Strength.YoungsModulus.Si * BendingAxis.Minor.Inertia(c);
            double majorShear = Seed(major, 0), majorMoment = Seed(major, 1);
            double minorShear = Seed(minor, 0), minorMoment = Seed(minor, 1);
            MemberResponse response = MemberResponse.Zero;
            int stations = model.Policy.StationCount.Value;
            for (int s = 0; s < stations; s++) {
                double x = length * s / Math.Max(stations - 1, 1);
                double xi = x.Over(length);
                double vz = eiMajor * HermiteJerk(li[2], li[4], lj[2], lj[4], length) + majorShear;
                double my = eiMajor * HermiteCurvature(li[2], li[4], lj[2], lj[4], xi, length) + majorMoment + (majorShear * x) + Particular(major, x);
                double vy = eiMinor * HermiteJerk(li[1], li[5], lj[1], lj[5], length) + minorShear;
                double mz = eiMinor * HermiteCurvature(li[1], li[5], lj[1], lj[5], xi, length) + minorMoment + (minorShear * x) + Particular(minor, x);
                double zDeflection = Hermite(li[2], li[4], lj[2], lj[4], xi, length) + Deflection(major, x, eiMajor);
                double yDeflection = Hermite(li[1], li[5], lj[1], lj[5], xi, length) + Deflection(minor, x, eiMinor);
                response = response.Absorb(new SectionDemand(axial, vy, vz, my, mz, torsion),
                    Math.Sqrt((zDeflection * zDeflection) + (yDeflection * yDeflection)));
            }
            return response;
        }

        static double Seed(Seq<(double Factor, (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) Action)> actions, int slot) =>
            actions.Fold(0.0, (acc, row) => acc + (row.Factor * row.Action.EndForces[slot]));

        static double Particular(Seq<(double Factor, (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) Action)> actions, double x) =>
            actions.Fold(0.0, (acc, row) => acc + (row.Factor * row.Action.ParticularMoment(x)));

        static double Deflection(Seq<(double Factor, (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) Action)> actions, double x, double ei) =>
            actions.Fold(0.0, (acc, row) => acc + (row.Factor * row.Action.ParticularDeflection(x)).Over(ei));

        static double[] Localized(ReadOnlySpan<double> field, long joint, double[] r) {
            double[] g = [At(field, joint, 0), At(field, joint, 1), At(field, joint, 2), At(field, joint, 3), At(field, joint, 4), At(field, joint, 5)];
            return [
                (r[0] * g[0]) + (r[1] * g[1]) + (r[2] * g[2]), (r[3] * g[0]) + (r[4] * g[1]) + (r[5] * g[2]), (r[6] * g[0]) + (r[7] * g[1]) + (r[8] * g[2]),
                (r[0] * g[3]) + (r[1] * g[4]) + (r[2] * g[5]), (r[3] * g[3]) + (r[4] * g[4]) + (r[5] * g[5]), (r[6] * g[3]) + (r[7] * g[4]) + (r[8] * g[5]),
            ];
        }

        static double At(ReadOnlySpan<double> field, long joint, int dof) =>
            field[checked((int)((joint * LoweredJointDofs) + dof))];

        static double Hermite(double uzI, double ryI, double uzJ, double ryJ, double xi, double length) =>
            ((1.0 - (3.0 * xi * xi)) + (2.0 * xi * xi * xi)) * uzI + length * (xi - (2.0 * xi * xi) + (xi * xi * xi)) * ryI
            + ((3.0 * xi * xi) - (2.0 * xi * xi * xi)) * uzJ + length * ((xi * xi * xi) - (xi * xi)) * ryJ;

        static double HermiteCurvature(double uzI, double ryI, double uzJ, double ryJ, double xi, double length) =>
            (((-6.0 + (12.0 * xi)) * uzI) + (length * (-4.0 + (6.0 * xi)) * ryI)
            + ((6.0 - (12.0 * xi)) * uzJ) + (length * ((6.0 * xi) - 2.0) * ryJ)).Over(length * length);

        static double HermiteJerk(double uzI, double ryI, double uzJ, double ryJ, double length) =>
            ((12.0 * uzI) + (6.0 * length * ryI) - (12.0 * uzJ) + (6.0 * length * ryJ)).Over(length * length * length);
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
