# [RASM_FABRICATION_DERIVATION]

The plan-derivation orchestrator: `Derivation.Plan` the ONE `Run(Derive)` lowering — a typed stage rail manufacturability → routing → fleet → setup → assembly → program → documentation folded as ONE `Fin`-bound expression, each stage a fold over its owning plane's entry, the whole pipeline depth-gated by the `DeriveDepth` policy row (`assess-only` stops after the DfM gate, `route` after the fleet match, `full-plan` runs the rail to documentation intent). The stage vocabulary is the `DerivationStage` `[SmartEnum<string>]` — seven ordered rows, ONE per rail segment — and it is fault-load-bearing: `FabricationFault.RoutingInfeasible` 2730 carries the exhausted stage row, so a component that passes DfM but matches no machine fails typed AT `fleet`, never as a silent empty plan. The rail ORCHESTRATES and never re-derives: process routing is `Spec/manufacturability`'s verdict (this rail consumes the `DfmReport` routing rows), machine matching is `Kinematics/fleet`'s join (`Fleet.Capable` — the EMPTY match set is fleet's valid verdict and THIS page's fault), join precedence is `Fixturing/assembly`'s graph (`Assembly.Sequence`), and the plan RECORDS rather than emits — every `PlannedStep.Program` key stays `Option.None` until `Run(Post)` runs over a produced `Motion`; a derivation that invokes posting intra-run is the deleted form. The capability gate stays input-carried: the rail READS `input.Capability` onto the plan and NEVER invokes `Capability.Gate` intra-run — the spec→plan→verify→capability loop closes run-N→run-N+1 exactly as `ResidualStock` does, the standing cycle-break restated, never re-opened.

The result is the owner's `FabricationResult.FabricationPlan` — `Seq<PlannedStep>` (primary-process step from the ranked `MachineMatch` head under the `PreferProcess`/`PreferMachine` policy overrides, plus one weld step per THERMAL final join re-classified through the public `JoinClass.Classify` over the component's `ComponentConnection` rows), the input-carried `Option<CapabilityVerdict>`, the artifact key ledger (EMPTY at derivation — egress stages append as they run), and the plan's own `ContentKey` under the `plan` egress row. The page also REALIZES the deferred cross-package growth seam: `FabricationProjector : IElementProjection` — the seam interface `Project(ProjectionContext) → Fin<GraphDelta>` (`Rasm.Element` `Projection/projection#IElementProjection`), registered as ONE row in the app-wired `Seq<IElementProjection>` that `ProjectionAssembly.Assemble` folds (the registration lives in app wiring by the seam's own law; the Materials `ComponentProjector` is the peer exemplar at `Projection/component#[05]`). The projector is the REVERSE direction of the `Ingress/element` seam — element.md READS the graph through `ElementGraph.Bake` into `AdmittedComponent`, the projector LOWERS plan and quantity facts back onto the graph as a `ctx.Owns`-gated `GraphDelta` monoid fold, the quantity-bag rows riding the Element delta-authoring vocabulary with `NestWasteArea` staying the frozen SI-m² wire key Compute decodes.

Wire posture: HOST-LOCAL. The plan crosses only the in-process seam — `FabricationPlan` to the caller and `Verify/estimation`'s quote lane (`Estimate.Of` prices the composed plan; derivation never prices), plan facts to the element graph through the ONE projector registration row; the stage vocabulary never sits between wire and rail.

## [01]-[INDEX]

- [01]-[DERIVATION]: owns the `DerivationStage` seven-row stage vocabulary, the `DeriveDepth` ceiling policy, the `DerivePolicy` carrier the owner `Derive` case threads, the `Derivation.Plan` stage rail composing manufacturability/fleet/assembly and recording setup/program/documentation intent, and the `FabricationProjector : IElementProjection` registration row — the one plan-derivation surface `owner#run`'s landed `derive` arm lowers to.

## [02]-[DERIVATION]

- Owner: `DerivationStage` `[SmartEnum<string>]` the ordered stage vocabulary (`manufacturability`/`routing`/`fleet`/`setup`/`assembly`/`program`/`documentation`, each carrying `Order`) — fault-payload load-bearing on `RoutingInfeasible` 2730; `DeriveDepth` `[SmartEnum<string>]` the ceiling policy (`assess-only`→manufacturability, `route`→fleet, `full-plan`→documentation), `Admits(stage)` the one gating relation; `DerivePolicy` the case carrier — depth, target quantity, the shop `MachineFleet`, the `DfmPolicy`/`AssemblyPolicy` plane policies threaded through, `PreferProcess`/`PreferMachine` overrides as Options, the documentation toggle; `Derivation` the static surface owning `Plan`; `FabricationProjector` the `IElementProjection` implementation lowering plan facts onto the element graph.
- Cases: `DerivationStage` rows 7; `DeriveDepth` rows 3; the stage rail is ONE `Fin` LINQ expression — DfM gate → routing rows → fleet match → depth-gated assembly → compose; setup stays the single-setup dormant fold until `Fixturing/setups` lands` is the TYPE contract the setup stage composes; `PlannedStep.Setup` reads its partition then, `0` now); program and documentation are RECORDING stages by law (keys `None`, the `Document` toggle marks traveler intent).
- Entry: `public static Fin<FabricationResult> Plan(FabricationPolicy.Derive policy, FabricationInput input)` — satisfies the landed owner arm `derive: static (i, p) => Derivation.Plan(p, i)`; `Fin<T>` routes `RoutingInfeasible` 2730 at the exhausted stage (empty routing rows at `routing`, empty feasible match at `fleet`), passes plane faults through unre-cased (`AssemblyPrecedenceCyclic` 2737 is assembly's, DfM verdicts are receipts and a DfM REJECTION routes 2730 at `manufacturability`), and routes kernel `GeometryFault.DegenerateInput` for a component the fleet join cannot bound.
- Auto: `Plan` binds `Manufacturability.Assess(component, policy.Dfm)` (`Spec/manufacturability` — Spec OWNS routing; the rail reads `DfmReport.Routing`, the ranked admitted `ProcessKind` rows, as the TYPE contract), filters the rows by `PreferProcess`, folds `Fleet.Capable(component, policy.Fleet)` and filters by routing membership + `PreferMachine`, gates emptiness per stage, depth-gates `Assembly.Sequence(component, policy.Assembly)`, then composes: the primary step from the ranked match head, weld steps for thermal final joins (`JoinClass.Classify` re-read over `Connections` — the plan altitude never re-derives precedence, it honors `AssemblyPlan.Steps` order), `input.Capability` carried, artifacts empty, the key minted over the canonical step census. `Verify/estimation` consumes the composed plan on its quote lane (`Estimate.Of(FabricationResult) → CostReceipt` — the Verify page's fixed contract; machine rates read off `MachineMatch.Instance.HourlyRate`, never a derivation rate table).
- Receipt: `FabricationPlan` IS the derivation evidence — typed steps, carried verdict, key ledger, content key; the `DfmReport`/`MachineMatch`/`AssemblyPlan` stage receipts stay plane-local per the ruling-5 payload discipline, never on the result case.
- Packages: `Process/owner#FABRICATION_OWNER` (`AdmittedComponent`/`PlannedStep`/`FabricationPlan`/`EgressKind`/`ContentKey` — composed), `Process/family#PROCESS_FAMILY` (`ProcessKind`/`Machine` axes), `Spec/manufacturability` (`Manufacturability.Assess` → `DfmReport` — the routing verdict), `Kinematics/fleet#MACHINE_FLEET` (`Fleet.Capable` → ranked `MachineMatch`), `Fixturing/assembly#ASSEMBLY` (`Assembly.Sequence` → `AssemblyPlan`; `JoinClass.Classify`), `Rasm.Element` (`IElementProjection`/`ProjectionContext`/`GraphDelta`/`NodeId` — the projection seam floor), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox; landed seams: `Fixturing/setups` (the setup partition), `Spec/capability` (`Capability.Gate` produces the verdict the NEXT run's input carries).
- Growth: a new rail segment is one `DerivationStage` row + one stage fold in the ONE expression; a new ceiling is one `DeriveDepth` row; a new plan fact is one `PlannedStep` column ripple on owner#atoms; a new lowered element fact is one `Lower` row on the projector; a persisted plan is the `plan` `ArtifactKind` enrollment ripple riding this page; zero new entrypoint surface.
- Boundary: `Derivation.Plan` is the ONE orchestrator and a second public plan fold, a per-stage public API, or a plane re-derivation (routing logic here, a fleet filter here, a precedence sort here) is the deleted form; the plan RECORDS and a derivation that posts programs or renders documents intra-run is the deleted form; `RoutingInfeasible` mints ONLY here and a plane-minted routing fault is the rejected form; the `DfmReport` read is the `Routing` rows ONLY — a derivation reaching into per-feature DfM verdicts is the seam violation; the capability read is `input.Capability` and an intra-run `Capability.Gate` call is the named cycle re-opening; the plan keys under `EgressKind.Plan` — the one-row owner#atoms growth this landing carries; the projector registers as ONE app-wired row and a Fabrication-side `Assemble` call, second projector, or graph-authoring re-derivation is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Element;                       // IElementProjection · ProjectionContext · GraphDelta · NodeId — the projection seam floor
using Rasm.Fabrication.Fixturing;         // Assembly.Sequence · AssemblyPolicy · AssemblyPlan · JoinClass · JoinPhase
using Rasm.Fabrication.Kinematics;        // Fleet.Capable · MachineFleet · MachineMatch
using Rasm.Fabrication.Spec;              // Manufacturability.Assess · DfmPolicy · DfmReport (TYPE contract — Spec owns routing)
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Process;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Fault-load-bearing stage vocabulary: RoutingInfeasible 2730 carries the exhausted row; Order drives the depth ceiling.
[SmartEnum<string>]
public sealed partial class DerivationStage {
    public static readonly DerivationStage Manufacturability = new("manufacturability", order: 1);
    public static readonly DerivationStage Routing = new("routing", order: 2);
    public static readonly DerivationStage Fleet = new("fleet", order: 3);
    public static readonly DerivationStage Setup = new("setup", order: 4);
    public static readonly DerivationStage Assembly = new("assembly", order: 5);
    public static readonly DerivationStage Program = new("program", order: 6);
    public static readonly DerivationStage Documentation = new("documentation", order: 7);

    public int Order { get; }
}

[SmartEnum<string>]
public sealed partial class DeriveDepth {
    public static readonly DeriveDepth AssessOnly = new("assess-only", DerivationStage.Manufacturability);
    public static readonly DeriveDepth Route = new("route", DerivationStage.Fleet);
    public static readonly DeriveDepth FullPlan = new("full-plan", DerivationStage.Documentation);

    public DerivationStage Ceiling { get; }

    public bool Admits(DerivationStage stage) => stage.Order <= Ceiling.Order;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// The Derive-case carrier: plane policies thread THROUGH (Dfm is Spec's, Assembly is Fixturing's), the shop fleet is
// registry data, the overrides are Options — the component itself rides the policy CASE, never a new input field.
public sealed record DerivePolicy(
    DeriveDepth Depth,
    int Quantity,
    MachineFleet Fleet,
    DfmPolicy Dfm,
    AssemblyPolicy Assembly,
    Option<ProcessKind> PreferProcess,
    Option<Machine> PreferMachine,
    bool Document);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Derivation {
    // The ONE stage rail: DfM gate -> routing rows -> fleet match -> depth-gated assembly -> compose. Setup stays the
    // setup stage lowers to Fixturing/setups Setup.Schedule; program/documentation RECORD — Run(Post)/Run(Document) emit.
    public static Fin<FabricationResult> Plan(FabricationPolicy.Derive policy, FabricationInput input) =>
        from dfm in Manufacturability.Assess(policy.Component, policy.Policy.Dfm)
        from routed in RouteOf(dfm, policy.Component, policy.Policy)
        from matches in MatchOf(policy.Component, policy.Policy, routed)
        from joins in JoinsOf(policy.Component, policy.Policy)
        select Compose(policy.Component, input, matches, joins);

    // TYPE contract (Spec/manufacturability, concurrent): DfmReport.Routing — the ranked admitted ProcessKind rows.
    static Fin<Seq<ProcessKind>> RouteOf(DfmReport dfm, AdmittedComponent component, DerivePolicy policy) {
        if (!policy.Depth.Admits(DerivationStage.Routing)) return Fin.Succ(Seq<ProcessKind>());
        Seq<ProcessKind> routed = policy.PreferProcess.Match(p => dfm.Routing.Filter(r => r == p), () => dfm.Routing);
        return routed.IsEmpty
            ? Fin.Fail<Seq<ProcessKind>>(FabricationFault.RoutingInfeasible(component.RepresentationKey, DerivationStage.Routing).ToError())
            : Fin.Succ(routed);
    }

    static Fin<Seq<MachineMatch>> MatchOf(AdmittedComponent component, DerivePolicy policy, Seq<ProcessKind> routed) =>
        !policy.Depth.Admits(DerivationStage.Fleet) ? Fin.Succ(Seq<MachineMatch>())
        : Fleet.Capable(component, policy.Fleet).Bind(matches => {
            Seq<MachineMatch> admitted = matches
                .Filter(m => routed.Contains(m.Process))
                .Filter(m => policy.PreferMachine.Match(k => m.Instance.Kind == k, () => true));
            return admitted.IsEmpty
                ? Fin.Fail<Seq<MachineMatch>>(FabricationFault.RoutingInfeasible(component.RepresentationKey, DerivationStage.Fleet).ToError())
                : Fin.Succ(admitted);
        });

    // Assembly precedence is Fixturing/assembly's graph; its faults pass through unre-cased.
    static Fin<Option<AssemblyPlan>> JoinsOf(AdmittedComponent component, DerivePolicy policy) =>
        policy.Depth.Admits(DerivationStage.Assembly) && !component.Connections.IsEmpty
            ? Assembly.Sequence(component, policy.Assembly).Map(Some)
            : Fin.Succ(Option<AssemblyPlan>.None);

    static FabricationResult Compose(AdmittedComponent component, FabricationInput input, Seq<MachineMatch> matches, Option<AssemblyPlan> joins) {
        Seq<PlannedStep> steps = StepsOf(component, matches, joins);
        return new FabricationResult.FabricationPlan(steps, input.Capability, Seq<ContentKey>(), KeyOf(component, steps));
    }

    // Primary step from the ranked head; one weld step per THERMAL final join in AssemblyPlan.Steps order —
    // the plan honors assembly's precedence, never re-sorts it. Program keys stay None until Run(Post).
    static Seq<PlannedStep> StepsOf(AdmittedComponent component, Seq<MachineMatch> matches, Option<AssemblyPlan> joins) {
        Seq<PlannedStep> steps = matches
            .Filter(static m => m.Process != ProcessKind.Weld).HeadOrNone()
            .Map(m => Seq1(new PlannedStep(0, m.Process, m.Instance.Kind, Setup: 0, Program: None))).IfNone(Seq<PlannedStep>());
        Option<MachineMatch> torch = matches.Filter(static m => m.Process == ProcessKind.Weld).HeadOrNone();
        foreach (JoinStep join in joins.Map(static p => p.Steps).IfNone(Seq<JoinStep>()))
            if (join.Phase == JoinPhase.Final && Thermal(component, join))
                torch.IfSome(m => steps = steps.Add(new PlannedStep(steps.Count, ProcessKind.Weld, m.Instance.Kind, Setup: 0, Program: None)));
        return steps;
    }

    static bool Thermal(AdmittedComponent component, JoinStep join) =>
        join.Joint >= 0 && join.Joint < component.Connections.Count
        && JoinClass.Classify(component.Connections[join.Joint].RealizingKey).Map(static k => k.Thermal).IfNone(false);

    // The plan keys under EgressKind.Plan over the canonical step census — the one-row owner#atoms growth this landing carries.
    static ContentKey KeyOf(AdmittedComponent component, Seq<PlannedStep> steps) =>
        ContentKey.Of(EgressKind.Plan, System.Text.Encoding.UTF8.GetBytes(
            $"{component.RepresentationKey:x32}|{string.Join(';', steps.Map(static s => $"{s.Order}:{s.Process.Key}:{s.Machine.Key}:{s.Setup}"))}"));
}

// --- [COMPOSITION] ----------------------------------------------------------------------------------------------------------------------------------
// The deferred growth seam REALIZED: ONE registration row in the app-wired Seq<IElementProjection> that
// ProjectionAssembly.Assemble folds — the Materials ComponentProjector is the peer exemplar, the wiring is the apps'.
public sealed class FabricationProjector(Seq<(NodeId Element, FabricationResult.FabricationPlan Plan)> facts) : IElementProjection {
    // Reverse of Ingress/element's Bake read: ctx.Owns-gated plan facts lower onto the graph as a GraphDelta monoid fold.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        Fin.Succ(facts.Filter(f => ctx.Owns(f.Element))
            .Fold(GraphDelta.Empty.Reheader(ctx.Header), static (acc, f) => acc.Merge(Lower(f.Element, f.Plan))));

    // Seam body: plan-step + quantity rows author through the Element delta vocabulary (NestWasteArea stays the
    // frozen SI-m2 wire key Compute decodes) — composed at the Element surface, never re-derived in Fabrication.
    static GraphDelta Lower(NodeId element, FabricationResult.FabricationPlan plan) => GraphDelta.Empty;
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Derive["owner Run(Derive) landed arm"] --> Plan["Derivation.Plan stage rail"]
    Plan -->|1 manufacturability| Dfm["Spec/manufacturability Assess → DfmReport.Routing"]
    Plan -->|2 routing PreferProcess filter| Routed["ranked ProcessKind rows"]
    Plan -->|3 fleet| Matches["Kinematics/fleet Fleet.Capable → MachineMatch"]
    Plan -->|"4 setup Fixturing/setups Setup.Schedule"| Setup["setup partition"]
    Plan -->|5 assembly depth-gated| Joins["Fixturing/assembly Assembly.Sequence → AssemblyPlan"]
    Plan -.->|6 program RECORDS Option.None| Post["Run(Post) emits later"]
    Plan -.->|7 documentation toggle| Doc["Run(Document) composes later"]
    Routed --> Compose["Compose → FabricationPlan(Steps, input.Capability, artifacts, plan key)"]
    Matches --> Compose
    Joins --> Compose
    Compose -->|quote lane| Estimate["Verify/estimation Estimate.Of"]
    Compose -->|plan facts| Projector["FabricationProjector : IElementProjection — one app-wired registration row"]
    Projector -->|"GraphDelta (NestWasteArea frozen wire)"| Graph["Rasm.Element graph"]
    Capability["Spec/capability Gate"] -.->|CapabilityVerdict input-carried run-N→N+1| Plan
```
