# [RASM_FABRICATION_CONDITIONING]

`CutConditioning` owns every dimensioned decision a posted program conditions its geometry with — kerf, lead, tabs, pierce, assist, link feed, biarc fit, cutter compensation, cooling, committed chains, and the machine dynamics the lookahead prices against — and `Post.Assemble` is the one fold that turns an admitted `PostSource` into the `Posting/program#AST` node stream. Admission happens ONCE here: the interior of the fold never re-parses a dimension text and never re-validates a policy.

`PostPolicy` is the `FabricationPolicy.Post` payload the run spine carries, and it CONTAINS `CutConditioning` as its cut column — no consumer reads the conditioning value directly, and `Posting/optimization#ADMISSION` reaches every column of it through `OptimizePolicy.Post.Cut`. `QuantityArrow` at `Process/owner#RUN_DISPATCH` is the ONE dimension-text entry this page reaches, and `PostArrow` pairs each `PhysicsQuantity` axis with the quantity its own row rebuilds, so no slot can admit a length and construct a pressure. `SurfaceSpeed` at `Process/physics#BUDGET_FOLD` is the one spindle law, composed over the CUTTING diameter the tool snapshot measures; `CuttingLoad.TangentialPerEdge` at `Tooling/cuttingdata#CUTTING_LOAD` is the one force evaluation, so this page holds no force body of its own.

## [01]-[INDEX]

- [02]-[ADMISSION]: `PostArrow`, `CutPolicy`, `PostFit`, `CompPolicy`, `CutConditioning`, `ProgramTooling`, `ProgramSetup`, `OperationBoundary`, and `PostPolicy`.
- [03]-[CONDITIONING]: placement, tooling, setup, workholding, arc conditioning, tab partition, and the lookahead fold.

## [02]-[ADMISSION]

- Owner: `CutPolicy`, `PostFit`, and `CompPolicy` own the dimensioned cut, fit, and compensation decisions; `CutConditioning` composes them with cooling, dynamics, cutting data, committed chains, and profiles; `PostPolicy` composes that with tooling, setup, and emission.
- Law: `PostArrow` is the ONE dimension-text entry this page reaches. Each row names its axis, its locus, and the quantity the axis's canonical scalar rebuilds, so a refusal is addressable at the slot that produced it and a length axis cannot be paired with a pressure constructor. A `PhysicsQuantity.<axis>.Admit` call here is a second text boundary answering on a foreign plane and is the deleted form.
- Law: dwell rides `PhysicsQuantity.Duration` like every other axis. The hand `UnitsNet.Duration.TryParse` boundary this replaces claimed posting held one quantity `PhysicsQuantity` carried no row for — the row is on the axis roster and owns textual dwell, so the hand parse was a second text boundary beside the admitted one.
- Law: every admitted column carries its UnitsNet quantity past admission. A millimetre double surviving admission puts the unit in the NAME, so a caller reading `KerfMm` beside `PierceSeconds` holds two scales one signature cannot check; the exception is `ThermalCoefficient`, which is reciprocal kelvin and has no admitted quantity family, so it stays a bare per-kelvin scalar with its unit stated at the column.
- Law: `PostFit` is the biarc admission on the POSTING plane and shares no vocabulary with the kernel `Rasm.Solving` `FitPolicy` — that owner fits geometry to samples, this one decides when a sampled run may become two arcs, and one name across both would let a caller pass either into the other.
- Entry: every policy admits through its generated `Validate` and the one `Admitted` bridge; independent dimension failures accumulate through `Validation<Error, _>` before the `Fin` rail.
- Auto: `CompPolicy` derives cantilever stiffness, deflection, and thermal growth from its admitted columns, so no caller re-derives a compensation term; the load that stiffness divides is `CuttingLoad.TangentialPerEdge` off `Tooling/cuttingdata#CUTTING_LOAD`.
- Packages: `UnitsNet` supplies `Length`, `Speed`, `Pressure`, `Duration`, `Force`, and `Ratio`; `Thinktecture.Runtime.Extensions` generates every value object; `LanguageExt.Core` supplies `Validation<Error, _>`, applicative `Apply`, and the `Fin` rail.
- Boundary: raw dimension text never crosses admission, and no column past it carries a unit in its name.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.Linq;
using CavalierContours.Polyline;
using g3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Nesting;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] ------------------------------------------------------------------------------------------------------------------------------------------
public sealed record CutRaw(
    string Kerf,
    LeadStyle Lead,
    string LeadRadius,
    string TabWidth,
    string TabSpacing,
    string Pierce,
    Option<string> Assist,
    string FeedCeiling,
    double LinkFeedFactor);
public sealed record FitRaw(string Deviation, string MinimumRun, string SplitDistance, int ProbeFloor);
public sealed record CompRaw(
    string ToolDiameter,
    string CutWidth,
    string AxialDepth,
    string Stickout,
    int Teeth,
    string Modulus,
    double ThermalCoefficientPerKelvin,
    string TemperatureDelta);

// The one dimension-text arrow family this page reaches. The canonical scalar each axis answers in is the unit its
// own `PhysicsQuantity` row names, so the axis and the quantity it rebuilds ride ONE row here and no slot can admit
// a length and construct a pressure.
public static class PostArrow {
    public static Validation<Error, Length> Length(string locus, string text) =>
        Admit(PhysicsQuantity.Length, locus, text, UnitsNet.Length.FromMillimeters);

    public static Validation<Error, Speed> Feed(string locus, string text) =>
        Admit(PhysicsQuantity.Feed, locus, text, Speed.FromMillimetersPerMinutes);

    public static Validation<Error, Pressure> Pressure(string locus, string text) =>
        Admit(PhysicsQuantity.Pressure, locus, text, UnitsNet.Pressure.FromBars);

    public static Validation<Error, Duration> Duration(string locus, string text) =>
        Admit(PhysicsQuantity.Duration, locus, text, UnitsNet.Duration.FromSeconds);

    public static Validation<Error, TemperatureDelta> Temperature(string locus, string text) =>
        Admit(PhysicsQuantity.Temperature, locus, text, TemperatureDelta.FromDegreesCelsius);

    private static Validation<Error, TQuantity> Admit<TQuantity>(
        PhysicsQuantity axis, string locus, string text, Func<double, TQuantity> rebuild) =>
        new QuantityArrow(axis, FabConcern.Posting, locus).Admit(text).Map(rebuild).ToValidation();
}

// --- [POLICIES] ---------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CutPolicy {
    public Length Kerf { get; }
    public LeadStyle Lead { get; }
    public Length LeadRadius { get; }
    public Length TabWidth { get; }
    public Length TabSpacing { get; }
    public Duration Pierce { get; }
    public Option<Pressure> Assist { get; }
    public Speed FeedCeiling { get; }

    // The link feed is a SHARE of the cutting ceiling, so it rides the dimensionless ratio family rather than a
    // bare double a caller could hand a percentage to.
    public Ratio LinkFeed { get; }

    public Speed LinkFeedRate => FeedCeiling * LinkFeed.DecimalFractions;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Length kerf, ref LeadStyle lead,
        ref Length leadRadius, ref Length tabWidth, ref Length tabSpacing, ref Duration pierce,
        ref Option<Pressure> assist, ref Speed feedCeiling, ref Ratio linkFeed) {
        if (kerf < Length.Zero || pierce < Duration.Zero || feedCeiling <= Speed.Zero
            || assist.Exists(static value => value <= Pressure.Zero)
            || linkFeed.DecimalFractions is <= 0.0 or > 1.0
            || (lead != LeadStyle.None && leadRadius <= Length.Zero)
            || tabWidth < Length.Zero || tabSpacing < Length.Zero
            || (tabWidth > Length.Zero && tabSpacing <= tabWidth))
            validationError = new ValidationError("post-cut-policy");
    }

    public static Fin<CutPolicy> Admit(CutRaw raw) =>
        (PostArrow.Length("post-cut:kerf", raw.Kerf),
         PostArrow.Length("post-cut:lead-radius", raw.LeadRadius),
         PostArrow.Length("post-cut:tab-width", raw.TabWidth),
         PostArrow.Length("post-cut:tab-spacing", raw.TabSpacing),
         PostArrow.Duration("post-cut:pierce", raw.Pierce),
         raw.Assist.TraverseM(source => PostArrow.Pressure("post-cut:assist", source).ToFin()).As().ToValidation(),
         PostArrow.Feed("post-cut:feed-ceiling", raw.FeedCeiling))
        .Apply((kerf, lead, tabWidth, tabSpacing, pierce, assist, feed) =>
            Validate(kerf, raw.Lead, lead, tabWidth, tabSpacing, pierce, assist, feed,
                Ratio.FromDecimalFractions(raw.LinkFeedFactor), out CutPolicy policy).Admitted(policy))
        .As().ToFin().Bind(static value => value);
}

[ComplexValueObject]
public sealed partial class PostFit {
    // The admitted deviation gate rides the kernel `ToleranceLane.Deviation` band, so the shop's own override
    // reaches this fit through `Context.For` exactly as it reaches every other deviation gate in the branch.
    public Tolerance Deviation { get; }

    public Length MinimumRun { get; }
    public Length SplitDistance { get; }
    public int ProbeFloor { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Tolerance deviation,
        ref Length minimumRun, ref Length splitDistance, ref int probeFloor) {
        // A biarc fit needs three interior samples before a tangent pair means anything, so the probe floor is the
        // arity the fit itself demands rather than a tuning knob.
        if (!deviation.IsValid || deviation.Lane != ToleranceLane.Deviation
            || minimumRun <= Length.Zero || splitDistance <= Length.Zero || probeFloor < 3)
            validationError = new ValidationError("post-fit-policy");
    }

    public static Fin<PostFit> Admit(FitRaw raw, Op key) =>
        (PostArrow.Length("post-fit:deviation", raw.Deviation)
            .Bind(value => Tolerance.Of(ToleranceLane.Deviation, value.Millimeters, key).ToValidation()),
         PostArrow.Length("post-fit:minimum-run", raw.MinimumRun),
         PostArrow.Length("post-fit:split-distance", raw.SplitDistance))
        .Apply((deviation, run, split) =>
            Validate(deviation, run, split, raw.ProbeFloor, out PostFit policy).Admitted(policy))
        .As().ToFin().Bind(static value => value);
}

[ComplexValueObject]
public sealed partial class CompPolicy {
    public Length ToolDiameter { get; }
    public Length CutWidth { get; }

    // The engaged edge length — the chip WIDTH the force model prices its per-edge load over. Radial width alone
    // decides how much of the cutter is in material, never how much of the edge is, so both axes are declared.
    public Length AxialDepth { get; }

    public Length Stickout { get; }
    public int Teeth { get; }
    public Pressure Modulus { get; }

    // Reciprocal kelvin. No admitted UnitsNet family owns the dimension and none is catalogued, so the axis stays a
    // bare scalar with its unit stated here rather than reaching for an unverified quantity.
    public double ThermalCoefficientPerKelvin { get; }

    public TemperatureDelta TemperatureRise { get; }

    // Cantilever stiffness in newtons per millimetre: the second moment of a round section over a cubed overhang,
    // with the modulus and both lengths read in the units the quotient is stated in.
    public double StiffnessNewtonsPerMillimetre =>
        3.0 * Modulus.Megapascals * (Math.PI * Math.Pow(ToolDiameter.Millimeters, 4.0) / 64.0)
        / Math.Pow(Stickout.Millimeters, 3.0);

    public Length Deflection(Force edgeForce) =>
        Length.FromMillimeters(edgeForce.Newtons / StiffnessNewtonsPerMillimetre);

    public Length ThermalGrowth =>
        Stickout * (ThermalCoefficientPerKelvin * TemperatureRise.Kelvins);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Length toolDiameter,
        ref Length cutWidth, ref Length axialDepth, ref Length stickout, ref int teeth, ref Pressure modulus,
        ref double thermalCoefficientPerKelvin, ref TemperatureDelta temperatureRise) {
        // The radial width is bounded by the cutter it engages, so the intent this policy builds admits at the
        // tool owner rather than refusing there on a bound this page already knows.
        if (toolDiameter <= Length.Zero || cutWidth <= Length.Zero || axialDepth <= Length.Zero
            || stickout <= Length.Zero || modulus <= Pressure.Zero
            || cutWidth > toolDiameter || teeth <= 0
            || !double.IsFinite(thermalCoefficientPerKelvin) || thermalCoefficientPerKelvin < 0.0)
            validationError = new ValidationError("post-comp-policy");
    }

    public static Fin<CompPolicy> Admit(CompRaw raw) =>
        (PostArrow.Length("post-comp:tool-diameter", raw.ToolDiameter),
         PostArrow.Length("post-comp:cut-width", raw.CutWidth),
         PostArrow.Length("post-comp:axial-depth", raw.AxialDepth),
         PostArrow.Length("post-comp:stickout", raw.Stickout),
         PostArrow.Pressure("post-comp:modulus", raw.Modulus),
         PostArrow.Temperature("post-comp:temperature-delta", raw.TemperatureDelta))
        .Apply((diameter, width, axial, stickout, modulus, temperature) =>
            Validate(diameter, width, axial, stickout, raw.Teeth, modulus,
                raw.ThermalCoefficientPerKelvin, temperature, out CompPolicy policy).Admitted(policy))
        .As().ToFin().Bind(static value => value);
}

// --- [MODELS] -----------------------------------------------------------------------------------------------------------------------------------------
public sealed record CutConditioning(
    Option<CutPolicy> Cut,
    Option<PostFit> Fit,
    MotionDynamics Dynamics,
    Option<CuttingData> Cutting,
    Option<CompPolicy> Compensation,
    CoolingLaw Cooling,
    Seq<ChainRow> Chains,
    HashMap<int, Loop> Profiles) {
    // An absent cut policy falls back to the machine's own straight-span law, which is the ceiling every fed block
    // rides where the job declares none.
    public Speed FeedCeiling => Cut.Map(static value => value.FeedCeiling)
        .IfNone(Speed.FromMillimetersPerMinutes(Dynamics.LinearFeed));

    public Speed FeedFloor => Cut.Map(static value => value.LinkFeedRate)
        .IfNone(Speed.FromMillimetersPerMinutes(Dynamics.LinearFeed));
}

public sealed record ProgramTooling(SlotMap Slots, Seq<WorkItem> Work, MagazinePolicy Policy, Seq<OperationBoundary> Boundaries);
public sealed record WorkholdingPlan(Fixture Fixture, FixtureState State);
public sealed record ProgramSetup(SetupPlan Schedule, WorkholdingPlan Workholding);
public readonly record struct OperationBoundary(Operation Op, int Node, HashMap<ToolLifeBasis, double> Consumed);

public sealed record PostRaw(CutConditioningRaw Cut, ProgramTooling Tooling, ProgramSetup Setup, EmitPolicy Emit);
public sealed record CutConditioningRaw(
    Option<CutRaw> Cut,
    Option<FitRaw> Fit,
    MotionDynamics Dynamics,
    Option<CuttingData> Cutting,
    Option<CompRaw> Compensation,
    CoolingLaw Cooling,
    Seq<ChainRow> Chains,
    HashMap<int, Loop> Profiles);

[ComplexValueObject]
public sealed partial class PostPolicy {
    public CutConditioning Cut { get; }
    public ProgramTooling Tooling { get; }
    public ProgramSetup Setup { get; }
    public EmitPolicy Emit { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref CutConditioning cut,
        ref ProgramTooling tooling, ref ProgramSetup setup, ref EmitPolicy emit) {
        // Posting is the FINAL egress, so a measurement-only block limit here would post a program past the
        // controller's own storage cap; `BlockLimit.Observe` belongs to the optimization measurement leg alone.
        if (emit.Limit is not BlockLimit.Enforce)
            validationError = new ValidationError("post-policy:block-limit");
    }

    public static Fin<PostPolicy> Admit(PostRaw raw) =>
        (raw.Cut.Cut.TraverseM(CutPolicy.Admit).As().ToValidation(),
         raw.Cut.Fit.TraverseM(row => PostFit.Admit(row, Mint)).As().ToValidation(),
         raw.Cut.Compensation.TraverseM(CompPolicy.Admit).As().ToValidation())
        .Apply((cut, fit, compensation) => new CutConditioning(cut, fit, raw.Cut.Dynamics, raw.Cut.Cutting,
            compensation, raw.Cut.Cooling, raw.Cut.Chains, raw.Cut.Profiles))
        .As().ToFin()
        .Bind(conditioning => Validate(conditioning, raw.Tooling, raw.Setup, raw.Emit, out PostPolicy policy).Admitted(policy));

    private static readonly Op Mint = Op.Of(name: nameof(PostPolicy));
}
```

## [03]-[CONDITIONING]

- Owner: `Post.Assemble` owns the source fold; `CutConditioning` composes cut, fit, compensation, dynamics, cooling, and committed-chain policy as admitted values.
- Law: `SpindleNodes` composes `Process/physics#BUDGET_FOLD` `SurfaceSpeed.Rpm` over the CUTTING diameter the tool snapshot measures — a shank diameter is not a cutting diameter and produces a surface speed the cut never sees. A tool carrying no measured cutting diameter refuses rather than posting a spindle word derived from the wrong geometry.
- Law: `SpecializedToolpathEnvelope.Admit` folded kind correspondence, non-empty rows, and finite duration once, so a local revalidation here is the deleted form and its ROWS ride the AST intact — `Dialect` renders each row's own evidence rather than a flattening to moves.
- Law: emitted node values are canonical millimetres and revolutions per minute, because `GParam.Number` writes into the `NodeKey` preimage. Every admitted quantity therefore projects at the ONE site that builds its word and never earlier, so the typed column stays typed everywhere a decision reads it.
- Entry: `PostSource.Motion`, `PostSource.Placement`, and `PostSource.Specialized` enter one `Post.Assemble` fold and diverge only inside `PostSource.Switch`; every arm opens its program on `Prologue`, which prepends the run's keyed drawing marks as one verbatim comment block ahead of the frame assignments.
- Auto: `ToolMagazine.Schedule` carries lifecycle and process-range evidence; `SetupSchedule.Apply` supplies WCS assignment; `Workholding.Apply` conditions motion; `ArcAlgebra.Apply` owns kerf, lead, and compensation. `Lookahead` interprets the NODES it is handed and never mints a content key for an intermediate tree.
- Exemption: `LookaheadKernel`, `Segments`, `Fit`, and `BulgeArc` are the named numeric kernels; every other join uses `Fold`, `FoldM`, `TraverseM`, generated `Switch`, and query syntax.
- Boundary: only a thermal-only controller spells beam-on as the torch word, and the declared modality set decides it, so no dialect identity is tested.

```csharp signature
// --- [CONDITIONING] -----------------------------------------------------------------------------------------------------------------------------------
public static partial class Post {
    internal static Fin<CutProgram> Assemble(
        PostSource source,
        PostDialect dialect,
        FabricationInput input,
        PostPolicy policy) =>
        from _ in dialect.Admits(input.Process.Modality)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.Pairing(new RelationFault.DialectModality(dialect, input.Process.Modality)))
        from changes in ToolMagazine.Schedule(policy.Tooling.Slots, policy.Tooling.Work, policy.Tooling.Policy)
        from scheduled in SetupSchedule.Apply(new SetupOp.Schedule(policy.Setup.Schedule))
        from schedule in scheduled is SetupResult.Scheduled value
            ? Fin.Succ(value.Schedule)
            : Fin.Fail<SetupSchedule>(FabricationFault.Inadmissible(FabConcern.Posting, "post:setup-result"))
        from program in source.Switch(
            state: (Dialect: dialect, Input: input, Policy: policy, Changes: changes, Schedule: schedule),
            motion: static (state, value) => MotionProgram(value.Value, state.Dialect, state.Policy, state.Changes, state.Schedule, state.Input.Tags),
            placement: static (state, value) => PlacementProgram(value.Value, state.Dialect, state.Policy, state.Changes, state.Schedule, state.Input.Tags),
            specialized: static (state, value) => SpecializedProgram(value.Value, state.Dialect, state.Schedule, state.Input.Tags))
        select program;

    // The envelope's rows ride the AST whole. A specialized lane's evidence — wire lag, bevel cross-tilt, link
    // transition, inspection deviation, turning form — is exactly what a posted program must carry forward, so the
    // directive keeps the admitted payload and `Dialect` renders one record per row.
    private static Fin<CutProgram> SpecializedProgram(
        SpecializedToolpathEnvelope payload,
        PostDialect dialect,
        SetupSchedule schedule,
        Map<string, Arr<ProfileMarking>> tags) =>
        Fin.Succ(CutProgram.Of(Prologue(schedule, tags)
            .Add(new GNode.Directive(new MotionDirective.Specialized(-1, payload)))
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect));

    private static Fin<CutProgram> MotionProgram(
        FabricationResult.Motion motion,
        PostDialect dialect,
        PostPolicy policy,
        Seq<ToolChange> changes,
        SetupSchedule schedule,
        Map<string, Arr<ProfileMarking>> tags) =>
        from held in Workholding.Apply(new WorkholdingOp.Condition(
            policy.Setup.Workholding.Fixture,
            policy.Setup.Workholding.State,
            motion.Moves))
        from moves in held is WorkholdingResult.Conditioned conditioned
            ? Fin.Succ(conditioned.Moves)
            : Fin.Fail<Seq<Move>>(FabricationFault.Inadmissible(FabConcern.Posting, "post:workholding-result"))
        from body in ToolSections(GNode.Moves(moves, motion.Directives, Point3d.Origin), changes, policy)
        from looked in Lookahead(body, policy.Cut.Dynamics)
        select CutProgram.Of(Prologue(schedule, tags).Concat(looked)
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect);

    private static Fin<CutProgram> PlacementProgram(
        FabricationResult.Placement placement,
        PostDialect dialect,
        PostPolicy policy,
        Seq<ToolChange> changes,
        SetupSchedule schedule,
        Map<string, Arr<ProfileMarking>> tags) =>
        from paths in policy.Cut.Chains.IsEmpty
            ? Unlinked(placement, dialect, policy)
            : policy.Cut.Chains.TraverseM(chain => ChainPath(chain, dialect, policy)).As().Map(static rows => rows.Bind(identity))
        from body in ToolSections(paths, changes, policy)
        from looked in Lookahead(body, policy.Cut.Dynamics)
        select CutProgram.Of(Prologue(schedule, tags).Concat(looked)
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect);

    private static Fin<Seq<GNode>> Unlinked(FabricationResult.Placement placement, PostDialect dialect, PostPolicy policy) =>
        from profiles in placement.Parts.Map(transform => policy.Cut.Profiles.Find(transform.PartId)
            .ToFin(FabricationFault.Inadmissible(FabConcern.Posting, $"post:profile:{transform.PartId}"))
            .Bind(transform.Apply)).TraverseM(identity).As()
        from ordered in PolygonAlgebra.Apply(new PolygonOp.Topology(profiles.ToSeq(), PolygonFill.NonZero))
        from topology in ordered.Regioned(FabricationFault.Inadmissible(FabConcern.Posting, "post:placement-topology"))
        let loops = toSeq(topology.Nodes.OrderByDescending(static node => node.Depth)
            .ThenBy(static node => Math.Abs(node.SignedArea)).Select(static node => node.Boundary))
        from paths in loops.TraverseM(loop => Condition(loop, policy.Cut).Bind(conditioned => CutPath(conditioned, dialect, policy.Cut))).As()
        select paths.Bind(identity);

    private static Fin<Seq<GNode>> ChainPath(ChainRow chain, PostDialect dialect, PostPolicy policy) =>
        from _ in chain.Members.IsEmpty
            ? Fin.Fail<Unit>(FabricationFault.Inadmissible(FabConcern.Posting, $"post:chain:{chain.Chain}"))
            : Fin.Succ(unit)
        let contours = chain.Members.Bind(static member => member.Contours)
        from _shared in chain.Shared.IsEmpty && contours.ForAll(static contour => contour.Omitted.IsEmpty)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.Inadmissible(FabConcern.Posting, $"post:chain-shared:{chain.Chain}"))
        from _routing in contours.Filter(static contour => contour.Pierce).Count == chain.Pierces.Count
            && chain.RapidPaths.Count == chain.Pierces.Count
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(FabricationFault.Inadmissible(FabConcern.Posting, $"post:chain-routing:{chain.Chain}"))
        from folded in contours.FoldM<Fin, (Seq<GNode> Nodes, int Pierce)>(
            (Seq<GNode>(), 0),
            (state, contour) =>
                from loop in Condition(contour.Path, policy.Cut)
                from nodes in Walk(loop, dialect, policy.Cut)
                let prefix = contour.Pierce
                    ? chain.RapidPaths[state.Pierce].Tail.Map(point => (GNode)new GNode.Word(GCommand.Rapid, XY(point), None)).ToSeq()
                        .Concat(PierceBlock(policy.Cut.Cut, dialect))
                    : Seq<GNode>(new GNode.Word(GCommand.Feed,
                        XY(contour.Entry).Add(GParam.Number('F', policy.Cut.FeedFloor.MillimetersPerMinutes, ProgramUnits.Metric)), None))
                select (state.Nodes.Concat(prefix).Concat(nodes), state.Pierce + (contour.Pierce ? 1 : 0))).As()
        select folded.Nodes;

    private static Fin<Loop> Condition(Loop profile, CutConditioning policy) =>
        !profile.Closed
            ? Fin.Fail<Loop>(new FabricationFault.OpenLoop(FabConcern.Posting, profile.Count))
            : policy.Cut.Match(
                Some: cut =>
                    from forest in ArcForest.Admit(Seq(profile), profile.Tolerance, profile.Plane)
                    from trace in ArcAlgebra.Apply(new ArcOp.Kerf(forest, cut.Kerf.Millimeters,
                        profile.Winding() == Sign.Negative ? MaterialSide.Inside : MaterialSide.Outside))
                    from loop in trace is ArcTrace.Forest result
                        ? result.Result.Loops.Head.ToFin(FabricationFault.Kerf(new KerfWitness.Vanished(0), cut.Kerf.Millimeters))
                        : Fin.Fail<Loop>(FabricationFault.Inadmissible(FabConcern.Posting, "post:kerf-trace"))
                    from compensated in Compensate(loop, policy)
                    select compensated,
                None: () => Compensate(profile, policy));

    private static Fin<Loop> Compensate(Loop loop, CutConditioning policy) => policy.Compensation.Match(
        Some: compensation =>
            from mechanical in policy.Cutting.Match(
                Some: cutting => cutting.FeedBasis == FeedBasis.PerTooth
                    ? Deflection(compensation, cutting)
                    : Fin.Fail<Length>(FabricationFault.Inadmissible(FabConcern.Posting, $"post:compensation-feed-basis:{cutting.FeedBasis.Key}")),
                None: () => Fin.Succ(Length.Zero))
            let delta = mechanical + compensation.ThermalGrowth
            from offset in Math.Abs(delta.Millimeters) <= loop.Tolerance.Absolute.Value
                ? Fin.Succ(loop)
                : ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Path(loop),
                        loop.Winding() == Sign.Negative ? -delta.Millimeters : delta.Millimeters))
                    .Bind(trace => trace is ArcTrace.Paths paths
                        ? paths.Result.Head.ToFin(FabricationFault.Inadmissible(FabConcern.Posting, "post:compensation-empty"))
                        : Fin.Fail<Loop>(FabricationFault.Inadmissible(FabConcern.Posting, "post:compensation-trace")))
            select offset,
        None: () => Fin.Succ(loop));

    // Cantilever deflection is the load ONE cutting edge carries, so the compensation reads `TangentialPerEdge` off
    // `Tooling/cuttingdata`'s single force evaluation rather than a second force body here — the same receipt a
    // torque or removal-rate consumer reads its engaged column from. The spindle the intent prices at composes the
    // one `SurfaceSpeed` law over the declared cutting diameter, so no rate on this page is derived twice.
    private static Fin<Length> Deflection(CompPolicy compensation, CuttingData cutting) {
        double spindle = SurfaceSpeed.Rpm(cutting.SurfaceSpeed, compensation.ToolDiameter.Millimeters);
        return CutIntent.Admit(
                chipThickness: Length.FromMillimeters(cutting.Feed),
                chipWidth: compensation.AxialDepth,
                axialDepth: compensation.AxialDepth,
                radialDepth: compensation.CutWidth,
                diameter: compensation.ToolDiameter,
                teeth: compensation.Teeth,
                spindle: RotationalSpeed.FromRevolutionsPerMinute(spindle),
                feed: Speed.FromMillimetersPerMinutes(cutting.Feed * compensation.Teeth * spindle))
            .Bind(cutting.Evaluate)
            .Map(load => compensation.Deflection(load.TangentialPerEdge));
    }

    private static Fin<Seq<GNode>> CutPath(Loop loop, PostDialect dialect, CutConditioning policy) =>
        from pierce in Sample(loop, 0.0)
        from lead in Lead(loop, policy.Cut)
        from body in Walk(loop, dialect, policy)
        select Seq<GNode>(new GNode.Word(GCommand.Rapid,
                XY(lead.Head.Map(GNode.Target).IfNone(pierce)), None))
            .Concat(PierceBlock(policy.Cut, dialect))
            .Concat(lead.IsEmpty ? Seq<GNode>() : GNode.Moves(lead, pierce))
            .Concat(body);

    private static Fin<Seq<Move>> Lead(Loop loop, Option<CutPolicy> policy) => policy.Match(
        Some: cut => cut.Lead.Shape(cut.LeadRadius.Millimeters).Match(
            Some: shape => ArcAlgebra.Apply(new ArcOp.Lead(loop, 0.0, cut.FeedCeiling.MillimetersPerMinutes, shape,
                    loop.Winding() == Sign.Negative ? MaterialSide.Inside : MaterialSide.Outside,
                    LeadRole.Entry))
                .Bind(trace => trace is ArcTrace.Motion motion
                    ? Fin.Succ(motion.Receipt.Moves)
                    : Fin.Fail<Seq<Move>>(FabricationFault.Inadmissible(FabConcern.Posting, "post:lead-trace"))),
            None: () => Fin.Succ(Seq<Move>())),
        None: () => Fin.Succ(Seq<Move>()));

    private static Fin<Seq<GNode>> Walk(Loop loop, PostDialect dialect, CutConditioning policy) =>
        from segments in Segments(loop, policy.Cut)
        from folded in segments.FoldM<Fin, (Seq<GNode> Output, Seq<Point3d> Run)>(
            (Seq<GNode>(), Seq(loop.At(0))),
            (state, segment) => segment.Tab
                ? FlushRun(state.Run, policy.Fit, policy.FeedCeiling).Map(flushed =>
                    (state.Output.Concat(flushed).Concat(Bridge(segment.To, policy.Cut, dialect)), Seq(segment.To)))
                : Math.Abs(segment.Bulge) <= loop.Tolerance.Absolute.Value
                    ? Fin.Succ((state.Output, state.Run.Add(segment.To)))
                    : FlushRun(state.Run, policy.Fit, policy.FeedCeiling).Map(flushed =>
                        (state.Output.Concat(flushed).Add(BulgeArc(segment.From, segment.To, segment.Bulge,
                            Feedrate(loop, segment.Span, policy))), Seq(segment.To)))).As()
        from tail in FlushRun(folded.Run, policy.Fit, policy.FeedCeiling)
        select folded.Output.Concat(tail);

    private static Fin<Seq<PathSegment>> Segments(Loop loop, Option<CutPolicy> policy) {
        double total = loop.Length();
        Seq<TabWindow> tabs = policy.Bind(cut => cut.TabSpacing > Length.Zero && cut.TabWidth > Length.Zero
            ? Some(Range(0, (int)Math.Floor(total / cut.TabSpacing.Millimeters)).ToSeq()
                .Map(index => cut.TabSpacing.Millimeters * (index + 0.5))
                .Map(center => new TabWindow(center - cut.TabWidth.Millimeters / 2.0, center + cut.TabWidth.Millimeters / 2.0))
                .Filter(window => window.Start > loop.Tolerance.Absolute.Value
                    && window.End < total - loop.Tolerance.Absolute.Value))
            : None).IfNone(Seq<TabWindow>());
        Seq<double> stations = toSeq(Range(0, loop.Spans).ToSeq().Map(index => loop.At(index).DistanceTo(loop.At(index + 1)))
            .Fold(Seq(0.0), static (state, length) => state.Add(state.Last.IfNone(0.0) + length))
            .Concat(tabs.Bind(static window => Seq(window.Start, window.End))).Add(total)
            .Distinct().OrderBy(static value => value));
        return Range(0, stations.Count - 1).ToSeq().Map(index =>
            from from in Sampled(loop, stations[index])
            from to in Sampled(loop, stations[index + 1])
            let midpoint = (stations[index] + stations[index + 1]) / 2.0
            let sourceBulge = loop.BulgeAt(from.Segment)
            let sourceLength = Math.Max(loop.Tolerance.Absolute.Value,
                loop.At(from.Segment).DistanceTo(loop.At(from.Segment + 1)))
            let fraction = (stations[index + 1] - stations[index]) / sourceLength
            let bulge = Math.Abs(sourceBulge) <= loop.Tolerance.Absolute.Value
                ? 0.0 : Math.Sign(sourceBulge) * Math.Tan(Math.Atan(Math.Abs(sourceBulge)) * fraction)
            select new PathSegment(from.Segment, from.Point, to.Point, bulge,
                tabs.Exists(window => midpoint > window.Start && midpoint < window.End)))
            .TraverseM(identity).As();
    }

    private static Fin<ProfileResult.Sampled> Sampled(Loop loop, double station) =>
        loop.Apply(new ProfileOp.Sample(Length.FromMillimeters(station))).Bind(result => result is ProfileResult.Sampled sampled
            ? Fin.Succ(sampled)
            : Fin.Fail<ProfileResult.Sampled>(FabricationFault.Inadmissible(FabConcern.Posting, "post:sample-result")));

    private static Fin<Point3d> Sample(Loop loop, double station) => Sampled(loop, station).Map(static result => result.Point);

    private static Fin<Seq<GNode>> FlushRun(Seq<Point3d> run, Option<PostFit> policy, Speed feed) => policy.Match(
        Some: fit => run.Count < fit.ProbeFloor
            || run.Zip(run.Skip(1)).Sum(static pair => pair.Item1.DistanceTo(pair.Item2)) < fit.MinimumRun.Millimeters
            ? Fin.Succ(Lines(run, feed))
            : Fit(run, fit, feed),
        None: () => Fin.Succ(Lines(run, feed)));

    // Exemption: the biarc fit is a numeric kernel — the tangent pair, the deviation probe, and the admission
    // verdict all read one constructed fit, and splitting them rebuilds it.
    private static Fin<Seq<GNode>> Fit(Seq<Point3d> run, PostFit policy, Speed feed) {
        Point3d first = run[0];
        Point3d last = run[run.Count - 1];
        Vector2d start = new(first.X, first.Y);
        Vector2d end = new(last.X, last.Y);
        Vector2d tangentA = new(run[1].X - first.X, run[1].Y - first.Y);
        Vector2d tangentB = new(last.X - run[run.Count - 2].X, last.Y - run[run.Count - 2].Y);
        if (tangentA.Length <= policy.Deviation.Value || tangentB.Length <= policy.Deviation.Value)
            return Fin.Succ(Lines(run, feed));
        BiArcFit2 fit = new(start, tangentA.Normalized, end, tangentB.Normalized, policy.SplitDistance.Millimeters);
        double deviation = run.Tail.Init.Fold(0.0, (held, probe) => {
            Vector2d sample = new(probe.X, probe.Y);
            Vector2d nearest = fit.NearestPoint(sample);
            return Math.Max(held, Math.Max(fit.Distance(sample),
                Math.Sqrt(Math.Pow(nearest.x - sample.x, 2.0) + Math.Pow(nearest.y - sample.y, 2.0))));
        });
        bool admitted = fit.FitD1 > 0.0 && fit.FitD2 > 0.0 && deviation <= policy.Deviation.Value;
        return admitted
            ? toSeq(fit.Curves).TraverseM(curve => CurveNode(curve, feed)).As()
            : Fin.Succ(Lines(run, feed));
    }

    private static Fin<GNode> CurveNode(IParametricCurve2d curve, Speed feed) => curve switch {
        Arc2d arc => Fin.Succ<GNode>(ArcNode(arc, feed)),
        Segment2d segment => Fin.Succ<GNode>(SegmentNode(segment, feed)),
        _ => Fin.Fail<GNode>(FabricationFault.Inadmissible(FabConcern.Posting, $"post:fit-curve:{curve.GetType().Name}")),
    };

    private static GNode SegmentNode(Segment2d segment, Speed feed) {
        Vector2d end = segment.SampleArcLength(segment.Length);
        return new GNode.Word(GCommand.Feed,
            XY(new Point3d(end.x, end.y, 0.0)).Add(GParam.Number('F', feed.MillimetersPerMinutes, ProgramUnits.Metric)), None);
    }

    private static GNode ArcNode(Arc2d arc, Speed feed) {
        Vector2d start = arc.SampleArcLength(0.0);
        Vector2d end = arc.SampleArcLength(arc.ArcLength);
        return new GNode.Word(arc.IsReversed ? GCommand.ArcCw : GCommand.ArcCcw,
            Arr(GParam.Number('X', end.x, ProgramUnits.Metric), GParam.Number('Y', end.y, ProgramUnits.Metric),
                GParam.Number('I', arc.Center.x - start.x, ProgramUnits.Metric), GParam.Number('J', arc.Center.y - start.y, ProgramUnits.Metric),
                GParam.Number('F', feed.MillimetersPerMinutes, ProgramUnits.Metric)), None);
    }

    private static Seq<GNode> Lines(Seq<Point3d> points, Speed feed) => points.Tail.Map(point =>
        (GNode)new GNode.Word(GCommand.Feed,
            XY(point).Add(GParam.Number('F', feed.MillimetersPerMinutes, ProgramUnits.Metric)), None)).ToSeq();

    // Exemption: the bulge-to-arc conversion is a numeric kernel — the provider resolves radius and centre from one
    // vertex pair, and the emitted word reads both.
    private static GNode BulgeArc(Point3d first, Point3d last, double bulge, Speed feed) {
        PlineVertex<double> start = new(first.X, first.Y, bulge);
        PlineVertex<double> end = new(last.X, last.Y, 0.0);
        var (_, center) = PlineSeg.SegArcRadiusAndCenter(start, end);
        return new GNode.Word(bulge > 0.0 ? GCommand.ArcCcw : GCommand.ArcCw,
            XY(last).Add(GParam.Number('I', center.X - first.X, ProgramUnits.Metric))
                .Add(GParam.Number('J', center.Y - first.Y, ProgramUnits.Metric))
                .Add(GParam.Number('F', feed.MillimetersPerMinutes, ProgramUnits.Metric)), None);
    }

    private static Seq<GNode> Bridge(Point3d target, Option<CutPolicy> policy, PostDialect dialect) =>
        Seq<GNode>(new GNode.Word(GCommand.SpindleStop, Arr<GParam>(), None),
            new GNode.Word(GCommand.Rapid, XY(target), None)).Concat(PierceBlock(policy, dialect));

    private static Seq<GNode> PierceBlock(Option<CutPolicy> policy, PostDialect dialect) => policy.Match(
        Some: cut => cut.Assist.Map(assist => (GNode)new GNode.Word(
                GCommand.AssistGas, Arr(GParam.Number('S', assist.Bars, ProgramUnits.Metric)), None)).ToSeq()
            .Add(new GNode.Word(BeamOn(dialect), Arr<GParam>(), None))
            .Concat(cut.Pierce > Duration.Zero
                ? Seq<GNode>(new GNode.CannedCycle(GCommand.Dwell,
                    Arr(GParam.Number('P', cut.Pierce.Seconds, ProgramUnits.Metric)), Seq<Move>(), 1, None))
                : Seq<GNode>()),
        None: () => Seq<GNode>());

    // Only a thermal-only controller spells beam-on as the torch word; a controller carrying a contact modality
    // spells it as the spindle word, so the declared modality set decides and no dialect identity is tested.
    private static GCommand BeamOn(PostDialect dialect) =>
        dialect.Modalities.Contains(ProcessModality.Thermal)
        && dialect.Modalities.ForAll(static modality => modality == ProcessModality.Thermal)
            ? GCommand.TorchOn : GCommand.Spindle;

    private static Fin<Seq<GNode>> ToolSections(Seq<GNode> nodes, Seq<ToolChange> changes, PostPolicy policy) =>
        from _ in changes.Exists(static change => change.Previous.IsSome) && policy.Tooling.Boundaries.IsEmpty
            ? Fin.Fail<Unit>(FabricationFault.Inadmissible(FabConcern.Posting, "post:tool-boundaries"))
            : Fin.Succ(unit)
        from placements in changes.TraverseM(change => change.Previous.IsNone
            ? Fin.Succ((Node: 0, Change: change))
            : policy.Tooling.Boundaries
                    .Filter(boundary => boundary.Op == change.Op && boundary.Node >= 0 && boundary.Node < nodes.Count
                        && boundary.Consumed.Find(change.LimitingBasis).Exists(consumed => consumed >= change.Trigger))
                    .Fold(Option<OperationBoundary>.None, static (best, boundary) =>
                        best.Filter(held => held.Node <= boundary.Node).IfNone(boundary))
                .ToFin(FabricationFault.Inadmissible(
                    FabConcern.Posting, $"post:tool-boundary:{change.Op.Key}:{change.LimitingBasis.Key}"))
                .Map(boundary => (boundary.Node, Change: change))).As()
        from sectioned in Range(0, nodes.Count).ToSeq().TraverseM(index => placements.Filter(row => row.Node == index)
            .TraverseM(row => SpindleNodes(policy.Cut.Cutting, row.Change.Assembly)
                .Map(spindle => ToolChangeNodes(row.Change).Concat(spindle).Concat(CoolingNodes(policy.Cut.Cooling)))).As()
            .Map(prefixes => prefixes.Bind(identity)
                .Add(ClampFeed(nodes[index], placements.Filter(row => row.Node <= index).Last.Map(static row => row.Change.Assembly.Feed))))).As()
        select sectioned.Bind(identity);

    private static Seq<GNode> ToolChangeNodes(ToolChange change) =>
        Seq<GNode>(
            new GNode.Word(GCommand.SpindleStop, Arr<GParam>(), None),
            new GNode.Word(GCommand.CoolantOff, Arr<GParam>(), None),
            new GNode.Word(GCommand.LengthCancel, Arr<GParam>(), None),
            new GNode.Word(GCommand.Rapid, Arr(GParam.Number('Z', change.Retract, ProgramUnits.Metric)), None))
        .Concat(change.Behaviors.Contains(MagazineBehavior.Confirm)
            ? Seq<GNode>(new GNode.Word(GCommand.OptionalStop, Arr<GParam>(), None)) : Seq<GNode>())
        .Add(new GNode.Word(GCommand.ToolChange, Arr(GParam.Number('T', change.ProgramTool, ProgramUnits.Metric)), None))
        .Add(new GNode.Word(GCommand.LengthOffset,
            Arr(GParam.Number('H', change.ProgramTool, ProgramUnits.Metric), GParam.Number('Z', change.LengthOffset, ProgramUnits.Metric)), None));

    // The ONE spindle law composed over the CUTTING diameter: `Process/physics#BUDGET_FOLD` owns `n = vc*1000/(pi*D)`
    // and the measured cutting diameter is what the cut actually sees. A shank diameter posts a surface speed the
    // edge never runs at, so a tool carrying no cutting measurement refuses rather than substituting the shank.
    private static Fin<Seq<GNode>> SpindleNodes(Option<CuttingData> cutting, ToolAssembly assembly) => cutting.Match(
        Some: data => assembly.Snapshot.Metric(ToolMeasure.CuttingDiameter)
            .OrElse(assembly.Snapshot.Metric(ToolMeasure.MaximumCuttingDiameter))
            .Filter(static value => ValidityClaim.Positive(value).Holds)
            .ToFin(FabricationFault.Inadmissible(FabConcern.Posting, $"post:cutting-diameter:{assembly.Key.Value}"))
            .Map(diameter => Seq<GNode>(new GNode.Word(GCommand.Spindle,
                Arr(GParam.Number('S', Clamp(SurfaceSpeed.Rpm(data.SurfaceSpeed, diameter), assembly.Spindle), ProgramUnits.Metric)),
                None))),
        None: () => Fin.Succ(Seq<GNode>()));

    private static Seq<GNode> CoolingNodes(CoolingLaw cooling) => cooling.Word().Map(command =>
        (GNode)new GNode.Word(command, Arr<GParam>(), None)).ToSeq();

    private static GNode ClampFeed(GNode node, Option<ProcessRange> range) => node is GNode.Word word && word.P('F').IsSome
        ? word.With('F', range.Map(value => Clamp(word.P('F').IfNone(0.0), value)).IfNone(word.P('F').IfNone(0.0)))
        : node;

    // An absent bound is `None`, so the clamp reads what the range declares rather than an infinity standing in for
    // a bound the equipment never published.
    private static double Clamp(double requested, ProcessRange range) {
        double selected = Math.Min(requested, range.Resolve(requested));
        double floored = range.Minimum.Map(minimum => Math.Max(minimum, selected)).IfNone(selected);
        return range.Maximum.Map(maximum => Math.Min(maximum, floored)).IfNone(floored);
    }

    // Lookahead interprets the NODES it is handed: the prior form wrapped them in a keyed program, so every pass
    // that ran it paid a whole-tree serialization for a key it discarded.
    internal static Fin<Seq<GNode>> Lookahead(Seq<GNode> nodes, MotionDynamics dynamics) =>
        Interpret(nodes).Map(trace => {
            ProgramEvent.Motion[] motions = trace.Events.Choose(static item => item is ProgramEvent.Motion motion
                ? Some(motion) : None).ToArray();
            return RewriteLookahead(nodes, Seq<int>(), new LookaheadKernel(motions, dynamics).Run());
        });

    private static Seq<GNode> RewriteLookahead(Seq<GNode> nodes, Seq<int> prefix, Seq<LookaheadCap> caps) =>
        nodes.Map((node, index) => (Node: node, Locus: prefix.Add(index))).Map(row => row.Node.Switch(
            state: (Locus: row.Locus, Caps: caps),
            block: static (context, block) => block with {
                Body = RewriteLookahead(block.Body.ToSeq(), context.Locus, context.Caps).ToArr(),
            },
            // Absence of a cap is `None`, so a word no cap names keeps its programmed feed rather than reading an
            // infinity a fold seeded.
            word: static (context, word) => context.Caps
                .Filter(cap => cap.Locus.SequenceEqual(context.Locus))
                .Map(static cap => cap.Feed)
                .Fold(Option<double>.None, static (held, feed) => Some(held.Map(value => Math.Min(value, feed)).IfNone(feed)))
                .Match(Some: feed => word.With('F', feed), None: () => word),
            cannedCycle: static (_, cycle) => cycle,
            coordinateFrame: static (_, frame) => frame,
            macro: static (context, macro) => macro with {
                Body = RewriteLookahead(macro.Body.ToSeq(), context.Locus, context.Caps).ToArr(),
            },
            subprogram: static (context, subprogram) => subprogram with {
                Body = RewriteLookahead(subprogram.Body.ToSeq(), context.Locus, context.Caps).ToArr(),
            },
            additiveLayer: static (_, layer) => layer,
            nc1: static (_, nc1) => nc1,
            directive: static (_, directive) => directive));

    private readonly record struct LookaheadCap(Seq<int> Locus, double Feed);

    // Exemption: the lookahead kernel is a measured numeric pass over one motion array — the forward and reverse
    // sweeps each read the caps the other wrote, so the arrays ARE the algorithm.
    private ref struct LookaheadKernel {
        private readonly ProgramEvent.Motion[] motions;
        private readonly MotionDynamics dynamics;
        private readonly double[] caps;
        private readonly double[] ceilings;
        private readonly double[] distances;
        private readonly bool[] cutting;
        private readonly Vector3d[] vectors;

        public LookaheadKernel(ProgramEvent.Motion[] motions, MotionDynamics dynamics) {
            this.motions = motions;
            this.dynamics = dynamics;
            caps = new double[motions.Length];
            ceilings = new double[motions.Length];
            distances = new double[motions.Length];
            cutting = new bool[motions.Length];
            vectors = new Vector3d[motions.Length];
        }

        public Seq<LookaheadCap> Run() {
            for (int index = 0; index < motions.Length; index++) {
                ProgramEvent.Motion motion = motions[index];
                vectors[index] = motion.To - motion.From;
                distances[index] = vectors[index].Length;
                cutting[index] = motion.Cutting && motion.Word.P('F').IsSome && distances[index] > 0.0;
                // A span rides the ceiling its own SHAPE declares: the arc law bounds a circular span and the
                // linear law a straight one, so the block the machine cannot hold at its programmed rate is capped
                // by the limit that actually governs it.
                ceilings[index] = motion.Arc.IsSome ? dynamics.ArcFeed : dynamics.LinearFeed;
                caps[index] = cutting[index] ? motion.Word.P('F').IfNone(ceilings[index]) : ceilings[index];
            }
            for (int index = 0; index < motions.Length; index++)
                if (cutting[index])
                    caps[index] = Math.Min(caps[index], Junction(index));
            Sweep(0, motions.Length, 1);
            Sweep(motions.Length - 1, -1, -1);
            return Range(0, motions.Length).ToSeq().Filter(index => cutting[index])
                .Map(index => new LookaheadCap(motions[index].Locus.Source, caps[index])).ToSeq();
        }

        private void Sweep(int start, int end, int step) {
            double held = 0.0;
            for (int index = start; index != end; index += step) {
                if (!cutting[index]) {
                    held = 0.0;
                    continue;
                }
                caps[index] = Math.Min(caps[index], Reachable(held, distances[index], dynamics));
                held = caps[index] / 60.0;
            }
        }

        private double Junction(int index) {
            Vector3d incoming = vectors[index];
            _ = incoming.Unitize();
            double turn = 0.0;
            int inspected = 0;
            for (int cursor = index + 1; cursor < motions.Length && inspected < dynamics.LookaheadBlocks; cursor++) {
                if (!cutting[cursor])
                    continue;
                Vector3d outgoing = vectors[cursor];
                _ = outgoing.Unitize();
                turn = Math.Max(turn, Vector3d.VectorAngle(incoming, outgoing));
                incoming = outgoing;
                inspected++;
            }
            return turn <= 0.0 ? ceilings[index] : Math.Min(ceilings[index], dynamics.JunctionFeed(turn));
        }
    }

    private static double Reachable(double entry, double distance, MotionDynamics dynamics) => Math.Min(
        Math.Sqrt(entry * entry + 2.0 * dynamics.Acceleration * distance),
        entry + Math.Cbrt(6.0 * dynamics.Jerk * distance * distance)) * 60.0;

    private static Speed Feedrate(Loop loop, int span, CutConditioning policy) {
        double ceiling = policy.FeedCeiling.MillimetersPerMinutes;
        int before = (span - 1 + loop.Spans) % loop.Spans;
        int after = (span + 1) % loop.Spans;
        Vector3d incoming = loop.At(span) - loop.At(before);
        Vector3d outgoing = loop.At(after) - loop.At(span);
        _ = incoming.Unitize();
        _ = outgoing.Unitize();
        return Speed.FromMillimetersPerMinutes(
            Math.Min(ceiling, policy.Dynamics.JunctionFeed(Vector3d.VectorAngle(incoming, outgoing))));
    }

    // Every program opens on the drawing's keyed marks — part mark, heat number, shop tag — as one comment block
    // ahead of the frame assignments, so an operator verifies the material in the machine against the sheet the
    // program was posted from. Marks ride the dialect's verbatim comment channel and never an executable word, so a
    // controller that ignores comments loses nothing and no dialect needs a marking spelling of its own. A run with
    // no marks emits no block rather than an empty one.
    private static Seq<GNode> Prologue(SetupSchedule schedule, Map<string, Arr<ProfileMarking>> tags) =>
        Marks(tags) + schedule.Wcs.Map(assignment => (GNode)new GNode.CoordinateFrame(
            assignment,
            schedule.Setups[assignment.Setup].Mounting.Frame)).ToSeq();

    // Rows sort by name so two posts of one drawing emit byte-identical headers and a program diff reads as a real
    // change; a tag whose content carries several lines joins them under one row rather than fanning comment lines
    // a controller's line-length rule then truncates independently.
    private static Seq<GNode> Marks(Map<string, Arr<ProfileMarking>> tags) =>
        toSeq(tags.Fold(Seq<string>(), static (rows, name, marks) => rows + marks.ToSeq()
            .Choose(static mark => mark.Content is MarkingContent.Tag tag ? Some(tag.Type.Text.Replace('\n', ' ')) : None)
            .Map(text => $"{name}={text}"))
        .OrderBy(static row => row, StringComparer.Ordinal)) switch {
            { IsEmpty: true } => Seq<GNode>(),
            var rows => Seq<GNode>(new GNode.Block(
                new BlockFrame(None, None, false, false, None, rows, "marks"), Arr<GNode>())),
        };

    private static Arr<GParam> XY(Point3d point) => Arr(
        GParam.Number('X', point.X, ProgramUnits.Metric),
        GParam.Number('Y', point.Y, ProgramUnits.Metric));

    private readonly record struct TabWindow(double Start, double End);
    private readonly record struct PathSegment(int Span, Point3d From, Point3d To, double Bulge, bool Tab);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
