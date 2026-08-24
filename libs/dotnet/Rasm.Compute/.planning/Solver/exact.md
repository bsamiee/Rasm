# [COMPUTE_EXACT]

Rasm.Compute solver exact lane: the three OR-Tools engines a `Solver/optimizer#OPTIMIZER_LANE` row dispatches to when the design problem carries a lowered model rather than a black-box oracle — CP-SAT over the package's own `Domain` set algebra, the LinearSolver MILP backend, and the ConstraintSolver vehicle-routing rail. Each answers the same `KernelRun` the search kernels answer, so an exact row and a stochastic row are interchangeable at the dispatch and nothing above them branches on which ran.

Owned surface: `ExactEvidence`/`ShadowPrice` the measured search receipt with its dual prices, reduced costs, and optimality bound; `BoundStream` the `SolutionCallback` capsule projecting the optimality gap onto a `ProgressCell`; `ExactLane` the CP-SAT and MILP lowerings with their shared harvest; and the `RoutingNode`/`RoutingVehicle`/`RoutingDimensionSpec`/`RoutingProblem`/`RoutingResult`/`RoutingPolicy`/`RoutingSearch` routing vocabulary.

The design space, its variables, its policy, and the `ParetoFront`/`KernelRun` carriers all arrive from `Solver/optimizer#OPTIMIZER_LANE` and are composed here, never re-declared: `DesignProblem.Admissible`/`Scale`/`Offsets` are the lowering's whole coordinate story, `Optimizer.Probe` is the one oracle funnel a harvested assignment re-enters through, and `Convergence` (`Solver/contract#SOLVE_REQUEST`) is the verdict every row reports. `ComputeReceipt`, `CancelScope`, `ProgressCell`, and NodaTime `Duration` arrive settled. Page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[EXACT_LANE]: CP-SAT and MILP lowering over the typed `LinearModel`, assumption-literal explanation, and the measured evidence both publish.
- [03]-[ROUTING_ROW_SHAPE]: typed vehicle-routing model over the OR-Tools ConstraintSolver rail and its assignment answer.
- [04]-[ROUTING_SEARCH_POLICY]: first-solution and metaheuristic search behaviour as policy data.

## [02]-[EXACT_LANE]

- Owner: `ShadowPrice` one binding row's dual price beside its activity; `ExactEvidence` the engine's own measures — explored nodes, conflicts, objective beside bound, dual prices, reduced costs, wall time — with the derived `Gap`; `BoundStream` the `SolutionCallback` subclass publishing the shrinking optimality gap as an anytime completion fraction; `ExactLane` the static owner of both lowerings, the assumption-literal core read, the integer-coordinate scaling pair, and the shared `Harvest`.
- Cases: `Convergence` (composed) `Converged` on a gap inside the policy tolerance · `Exhausted` at the nodes explored when the bound never met the incumbent · `Stalled` where the engine published no gap at all.
- Entry: `ExactLane.SolveCpSat(DesignProblem, OptimizerPolicy, SearchContext, Func<DesignPoint, Fin<Seq<double>>>, ParetoFront)` and `ExactLane.SolveMilp(...)` — the two arms `Solver/optimizer#OPTIMIZER_LANE`'s `Invoke` dispatches its `cp-sat` and `milp` rows to. Both fault by name when `DesignProblem.Exact` is absent, because an exact solver cannot optimize a black-box FEA objective.
- Auto: every variable takes the `Domain` its case and its activation rule jointly admit, every row takes its band union, every conditional axis reifies both ways as one literal, and one assumption literal per row makes an UNSATISFIABLE return name its conflicting rows. The harvested assignment re-enters `Optimizer.Probe` in PHYSICAL space, so the oracle never rewrites what the solver returned.
- Receipt: no receipt case of its own — `ExactEvidence` rides `KernelRun.Exact` onto `OptimizationResult`, which the optimizer's own `Receipt` projects; `RoutingResult` rides its own slot beside it. A `Runtime/receipts#RECEIPT_UNION` slot this page does not own is never claimed.
- Packages: Google.OrTools (CP-SAT `CpModel`/`CpSolver`/`SolutionCallback` + the `Google.OrTools.Util` `Domain` set algebra + LinearSolver `Solver` + the `Google.OrTools.ConstraintSolver` routing rail), LanguageExt.Core, NodaTime, Rasm.Compute `Solver/optimizer` (the design vocabulary and carriers), BCL inbox.
- Growth: a further engine measure is one `ExactEvidence` field read off the solve handle; a new exact engine is one `OptimizerKind` row at the optimizer, one `Invoke` arm, and one lowering here; zero new surface — a `CpSatSolver`/`MilpSolver`/`VrpSolver` sibling family collapses onto these three lowerings.
- Boundary: ADMISSIBLE SETS are the package's own `Domain` algebra, never a `(lower, upper)` pair: a variable lowers through `DesignVariable.Admissible` (a range, a holed categorical roster, or a linked singleton), a conditional axis unions the inactive value the resolve fold writes, and a row lowers through `AddLinearExpressionInDomain` over its flat band set — a contiguous range standing in for a conditional or disjoint set admits exactly the states the design rules forbid, so the solver returns an assignment the oracle then rewrites and the exact lane silently answers a different program than the one authored. `LinearSolver`'s face carries ONE interval per row and ONE range per variable, so THREE shapes refuse there by name pointing at cp-sat — a banded row, a holed categorical roster, and a conditional axis — rather than relaxing to a hull that admits the forbidden states.
- Boundary: EXPLANATION is the exact lane's obligation, not a status token: each row reifies under its own assumption literal and an UNSATISFIABLE return names the conflicting rows through `SufficientAssumptionsForInfeasibility`, matching the law the sibling SMT page already holds on the identical capability. EVIDENCE publishes measured: the exact rows carry the engine's own branch or node count, its conflicts, its objective beside its bound, its dual prices and reduced costs, and its wall time — the literal `1` iteration count is a fabricated constant, and a `Feasible`-but-not-`Optimal` return without its bound is indistinguishable from a proven optimum on the receipt. OR-Tools native handles enter only through declared `IDisposable` roots (`CpSolver`/`Solver`/`RoutingModel`/`RoutingIndexManager`) released by `Dispose`; a hand-rolled branch-and-bound, simplex, or routing search beside the solver is rejected.
- Boundary: CP-SAT solves integer/boolean natively and discretizes continuous through `IntegerStep` under ONE declared coordinate system — coefficients, bounds, hints, and band edges scale through `DesignProblem.Scale` so the integer model preserves the physical `LinearModel` semantics — while MILP routes the integer part to SCIP and the continuous part through the linear backend with no discretization. CP-SAT parameter text formats as a WIRE format under the invariant culture; a comma-decimal locale renders a malformed deadline key the solver silently ignores.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// One binding row's price: what the objective gains per unit of relaxation, and where the row actually sits.
// This is the decision-grade half of a relaxation — "one more square metre of floor plate is worth X" — that a
// solve computes and an argmin-only harvest discards.
public sealed record ShadowPrice(string Row, double Dual, double Activity);

// What the exact engines MEASURE about their own search, beside the assignment. Every field is a read off the
// solver handle the harvest already holds: a literal iteration count is a fabricated constant, and a `Feasible`
// return without its bound is indistinguishable from a proven optimum on the receipt.
public sealed record ExactEvidence(
    string Engine,
    long Explored,
    Option<long> Conflicts,
    Option<double> Objective,
    Option<double> Bound,
    Seq<ShadowPrice> Prices,
    Seq<(string Variable, double Reduced)> Reduced,
    Duration Wall) {
    // Relative optimality gap; absent where either half is absent or the objective is zero, never a zero standing
    // in for an unmeasured bound.
    public Option<double> Gap =>
        (Objective, Bound) switch {
            ({ IsSome: true, Case: double value }, { IsSome: true, Case: double bound })
                when double.IsFinite(value) && double.IsFinite(bound) && Math.Abs(value) > 1e-12 =>
                Some(Math.Abs(value - bound) / Math.Abs(value)),
            _ => None,
        };
}

// --- [SERVICES] -------------------------------------------------------------------------

// Anytime completion for an exact solve. Every other Compute long-run advances by counted segments; an exact
// search has no segment count but DOES have a converging bound, so the optimality gap is the one honest fraction the
// estate can publish for it — and `SolutionCallback` carries both halves at every improving solution, which is why
// one subclass owns the stream rather than a bound callback paired with a separate incumbent source. Every hook
// invocation runs on the solver's own worker, so the cell's Atom-backed commit is the concurrency contract; a shrinking
// gap only ever raises the fraction, which is exactly the monotonic guard `ProgressCell.Advance` already holds.
public sealed class BoundStream(ProgressCell cell) : SolutionCallback {
    private readonly Atom<(double Incumbent, double Bound)> held = Atom((double.NaN, double.NaN));

    public override void OnSolutionCallback() => Publish(ObjectiveValue(), BestObjectiveBound());

    // Between solutions the bound keeps improving; the last incumbent this stream saw completes the pair.
    public void Observe(double bound) => Publish(held.Value.Incumbent, bound);

    private void Publish(double incumbent, double bound) =>
        ignore(cell.Advance(ProgressPhase.Running, fraction: Gap(held.Swap(_ => (incumbent, bound)))));

    // Relative gap folded to a completion fraction; an unpaired or degenerate pair advances the phase with no
    // fraction rather than manufacturing one, so a run that never found a solution never reads as partly done.
    private static double Gap((double Incumbent, double Bound) pair) =>
        double.IsFinite(pair.Incumbent) && double.IsFinite(pair.Bound) && Math.Abs(pair.Incumbent) > 1e-12
            ? Math.Clamp(1.0 - (Math.Abs(pair.Incumbent - pair.Bound) / Math.Abs(pair.Incumbent)), 0.0, 1.0)
            : 0.0;
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// The two OR-Tools lowerings and the harvest they share. Both re-enter `Optimizer.Probe` in PHYSICAL space, so a
// solved assignment is scored by the same oracle every other row is scored by.
public static class ExactLane {
    // CP-SAT lowers through the package's OWN set algebra: every variable takes the `Domain` its case and its
    // activation rule jointly admit, every row takes its band union, and every activation reifies as one literal
    // — so the model's feasible set IS the set `DesignProblem.Resolve` leaves standing, and the assignment the
    // harvest re-evaluates is one the oracle will not rewrite underneath it.
    internal static Fin<KernelRun> SolveCpSat(DesignProblem problem, OptimizerPolicy policy, SearchContext search, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        problem.Exact.Match(
            Some: model => {
                CpModel cp = new();
                long q = DesignProblem.Scale(policy.IntegerStep);
                // One solver variable per COORDINATE SLOT: a density field lowers as `Cells` variables under one
                // admissible domain, so the exact model searches the same space the oracle evaluates.
                IntVar[] vars = [.. Enumerable.Range(0, problem.Dimension).Select(slot =>
                    cp.NewIntVarFromDomain(problem.Admissible(slot, q), $"{problem.Variables[problem.VariableAt(slot)].VariableName}#{slot - problem.Offsets[problem.VariableAt(slot)]}"))];
                Reify(cp, problem, vars, q);
                // One assumption literal per row: an UNSATISFIABLE return then names the exact conflicting rows
                // through `SufficientAssumptionsForInfeasibility`, matching the explanation law the sibling engine
                // page already holds — an opaque status where the identical capability is one literal away is the
                // rejected refusal.
                HashMap<int, string> tracked = model.Rows.Fold(HashMap<int, string>(), (held, row) => {
                    BoolVar lit = cp.NewBoolVar(row.Name);
                    cp.AddLinearExpressionInDomain(LinearExpr.WeightedSum(vars, Scaled(problem, row.Coefficients, q)), Domain.FromFlatIntervals(row.Flattened(q)))
                        .OnlyEnforceIf(lit);
                    cp.AddAssumption(lit);
                    return held.Add(lit.GetIndex(), row.Name);
                });
                cp.Minimize(LinearExpr.WeightedSum(vars, Scaled(problem, model.Objective, q)));
                // Incumbent fronts carry a known-good assignment in the SAME coordinate the vars carry; the two
                // wrapping rows re-enter this kernel against a near-identical model, so discarding it pays a cold
                // search per restart.
                seed.Points.Head.Iter(best => {
                    for (int slot = 0; slot < vars.Length; slot++) {
                        cp.AddHint(vars[slot], Coded(problem.Variables[problem.VariableAt(slot)], best.Coordinates.ElementAtOrDefault(slot), q));
                    }
                });
                using CpSolver solver = new() {
                    // `num_search_workers` rides the SAME proto-text channel the deadline already writes; without
                    // it CP-SAT fans over every core and the one lane the AppHost seal exists to bound is the one
                    // that saturates longest. The proto text is a WIRE format, so the deadline formats under the
                    // invariant culture — a comma-decimal locale renders `30,0` and CP-SAT reads a malformed key.
                    StringParameters = string.Create(System.Globalization.CultureInfo.InvariantCulture,
                        $"max_time_in_seconds:{policy.SolveSeconds},num_search_workers:{policy.Parallelism}"),
                };
                // Cooperative stop, bracketed with the handle: the registration disposes before the solver does,
                // so a latch firing into a released native search is structurally impossible. An exact solve runs
                // longest of every kernel in the lane, and without this latch only its own deadline can end it
                // — a cancelled request would hold a native SCIP or CP-SAT thread for the full policy budget.
                using CancellationTokenRegistration latch = search.Scope.Source.Token.Register(solver.StopSearch);
                using BoundStream? stream = search.Progress.Map(static cell => new BoundStream(cell)).ValueUnsafe();
                if (stream is not null) { solver.SetBestBoundCallback(stream.Observe); }
                CpSolverStatus status = solver.Solve(cp, stream);
                solver.ClearBestBoundCallback();
                return status is CpSolverStatus.Optimal or CpSolverStatus.Feasible
                    ? Harvest(problem, policy, oracle, seed,
                        [.. Enumerable.Range(0, vars.Length).Select(slot =>
                            DiscreteValue(problem.Variables[problem.VariableAt(slot)], solver.Value(vars[slot]), policy.IntegerStep))],
                        Some(new ExactEvidence(
                            Engine: "cp-sat",
                            Explored: solver.NumBranches(),
                            Conflicts: Some(solver.NumConflicts()),
                            Objective: Some(solver.ObjectiveValue),
                            Bound: Some(solver.BestObjectiveBound),
                            Prices: Seq<ShadowPrice>(),
                            Reduced: Seq<(string, double)>(),
                            Wall: Duration.FromSeconds(solver.WallTime()))))
                    : Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
            },
            None: () => Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Resource))));

    // Conditional axes reify BOTH ways: the literal implies its source lies in the trigger set, its negation
    // implies the source lies in the complement, and the negation pins the gated axis at the inactive value the
    // resolve fold writes. One-way enforcement would leave the literal free to read false over a triggering
    // source and re-open the disagreement.
    static void Reify(CpModel cp, DesignProblem problem, IntVar[] vars, long q) {
        for (int axis = 0; axis < problem.Activation.Count; axis++) {
            if (problem.Activation[axis].Trigger(q).Case is not Domain trigger
                || problem.Activation[axis].Reads.Case is not int source) { continue; }
            BoolVar active = cp.NewBoolVar($"{problem.Variables[axis].VariableName}@active");
            // Both the trigger read and the gated pin address the variable through its LEADING slot and its whole
            // span respectively — the same split `DesignProblem.Resolve` applies, so the two lanes agree.
            cp.AddLinearExpressionInDomain(vars[problem.Offsets[source]], trigger).OnlyEnforceIf(active);
            cp.AddLinearExpressionInDomain(vars[problem.Offsets[source]], trigger.Complement()).OnlyEnforceIf(active.Not());
            for (int slot = problem.Offsets[axis]; slot < problem.Offsets[axis + 1]; slot++) {
                cp.AddLinearExpressionInDomain(vars[slot], Domain.FromValues([0L])).OnlyEnforceIf(active.Not());
            }
        }
    }

    // Infeasibility core: the response carries LITERAL indices, so the tracked map keys on `GetIndex()` and an
    // index no row claims never fabricates a name.
    static string Core(CpSolver solver, HashMap<int, string> tracked) =>
        string.Join(',', toSeq(solver.SufficientAssumptionsForInfeasibility()).Choose(index => tracked.Find(index)));

    static long[] Scaled(DesignProblem problem, ImmutableArray<double> coefficients, long q) {
        long[] scaled = new long[problem.Dimension];
        for (int slot = 0; slot < scaled.Length && slot < coefficients.Length; slot++) {
            scaled[slot] = (long)Math.Round(problem.Variables[problem.VariableAt(slot)] is DesignVariable.Continuous ? coefficients[slot] : coefficients[slot] * q);
        }
        return scaled;
    }

    // Physical coordinate back into the integer system the vars inhabit — the inverse of `DiscreteValue`, so a
    // hint and a harvest never disagree about which coordinate system they are in.
    static long Coded(DesignVariable variable, double physical, long q) =>
        variable is DesignVariable.Continuous or DesignVariable.Symbolic
            ? (long)Math.Round(physical * q)
            : (long)Math.Round(variable.Clamp(physical));

    // SCIP's constraint face is ONE interval, so a banded row is unrepresentable here and refuses rather than
    // relaxing to its hull — the hull admits exactly the states the band set excludes, the substitution this exact
    // lane exists to foreclose.
    internal static Fin<KernelRun> SolveMilp(DesignProblem problem, OptimizerPolicy policy, SearchContext search, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        problem.Exact.Match(
            Some: model => Representable(problem, model).Match(
                Some: Fin.Fail<KernelRun>,
                None: () => Scip(problem, policy, search, oracle, seed, model)),
            None: () => Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Resource))));

    // The LinearSolver face is ONE INTERVAL per row and ONE RANGE per variable, so three shapes the CP-SAT rail
    // expresses natively are unrepresentable here and each REFUSES by name pointing at cp-sat — a banded row
    // relaxed to its hull, a holed categorical roster relaxed to its span, and a conditional axis whose inactive
    // value the model cannot union all admit exactly the states the design rules forbid, and the solver then
    // returns an assignment the oracle rewrites underneath it.
    static Option<Error> Representable(DesignProblem problem, LinearModel model) =>
        model.Rows.Find(static row => !row.Contiguous) is { IsSome: true, Case: LinearRow banded }
            ? Some((Error)new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
            : problem.Variables.Find(static v => v is DesignVariable.Categorical { Admissible.IsEmpty: false }) is { IsSome: true, Case: DesignVariable holed }
                ? Some((Error)new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(holed.VariableName))))
                : problem.Variables
                    .Map((v, axis) => (Variable: v, Rule: axis < problem.Activation.Count ? problem.Activation[axis] : new ActivationRule.Always()))
                    .Find(static row => row.Rule is not ActivationRule.Always) is { IsSome: true, Case: var conditional }
                    ? Some((Error)new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(conditional.Variable.VariableName))))
                    : None;

    static Fin<KernelRun> Scip(DesignProblem problem, OptimizerPolicy policy, SearchContext search, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed, LinearModel model) {
        using Google.OrTools.LinearSolver.Solver? solver = Google.OrTools.LinearSolver.Solver.CreateSolver("SCIP");
        if (solver is null) { return Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.MilpSolver))); }
        // One solver variable per COORDINATE SLOT, matching the CP-SAT rail and the oracle's own point shape. The
        // slot names are kept beside the handles: the reduced-cost evidence reads them, and re-deriving a name from
        // the handle would bind a member the OR-Tools catalogue does not carry.
        string[] names = [.. Enumerable.Range(0, problem.Dimension).Select(slot =>
            $"{problem.Variables[problem.VariableAt(slot)].VariableName}#{slot - problem.Offsets[problem.VariableAt(slot)]}")];
        Google.OrTools.LinearSolver.Variable[] vars = [.. Enumerable.Range(0, problem.Dimension).Select(slot => {
            DesignVariable variable = problem.Variables[problem.VariableAt(slot)];
            string name = names[slot];
            return variable switch {
                DesignVariable.Integer i => solver.MakeIntVar(i.Lower, i.Upper, name),
                DesignVariable.Categorical c => solver.MakeIntVar(c.Codes.Min(int.MaxValue), c.Codes.Max(int.MinValue), name),
                DesignVariable.Continuous co => solver.MakeNumVar(co.Lower, co.Upper, name),
                DesignVariable.Symbolic s => solver.MakeNumVar(s.Lower, s.Upper, name),
                DesignVariable.Density => solver.MakeNumVar(0.0, 1.0, name),
                var other => solver.MakeNumVar(0.0, 0.0, name),
            };
        })];
        // Row handles are kept, not discarded: the dual price and activity a relaxation computes are read off
        // exactly these handles after the solve, and a fold that drops them publishes the least informative half
        // of the answer an AEC cost model is asked for.
        Seq<(string Name, Google.OrTools.LinearSolver.Constraint Handle)> rows = model.Rows.Map(row => {
            Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(row.Lower, row.Upper, row.Name);
            for (int axis = 0; axis < vars.Length && axis < row.Coefficients.Length; axis++) {
                constraint.SetCoefficient(vars[axis], row.Coefficients[axis]);
            }
            return (row.Name, constraint);
        });
        Google.OrTools.LinearSolver.Objective objective = solver.Objective();
        for (int axis = 0; axis < vars.Length && axis < model.Objective.Length; axis++) { objective.SetCoefficient(vars[axis], model.Objective[axis]); }
        objective.SetMinimization();
        solver.SetTimeLimit((long)(policy.SolveSeconds * 1000.0));
        solver.SetNumThreads(policy.Parallelism);
        seed.Points.Head.Iter(best => solver.SetHint(
            new Google.OrTools.LinearSolver.MPVariableVector(vars),
            [.. problem.Clamp(best.Coordinates.AsSpan())]));
        // LinearSolver carries no per-solution hook on its face, so the MILP row publishes its gap once on the
        // receipt rather than streaming it; `InterruptSolve` is the same cooperative stop the CP-SAT row takes.
        using CancellationTokenRegistration latch = search.Scope.Source.Token.Register(() => ignore(solver.InterruptSolve()));
        return solver.Solve() is Google.OrTools.LinearSolver.Solver.ResultStatus.OPTIMAL or Google.OrTools.LinearSolver.Solver.ResultStatus.FEASIBLE
            ? Harvest(problem, policy, oracle, seed,
                problem.Clamp([.. vars.Select(static v => v.SolutionValue())]),
                Some(new ExactEvidence(
                    Engine: "milp-scip",
                    Explored: solver.Nodes(),
                    Conflicts: None,
                    Objective: Some(objective.Value()),
                    Bound: Some(objective.BestBound()),
                    Prices: rows.Map(row => new ShadowPrice(row.Name, solver.DualValue(row.Handle), solver.Activity(row.Handle))),
                    Reduced: toSeq(Enumerable.Range(0, vars.Length)).Map(slot => (names[slot], solver.ReducedCost(vars[slot]))),
                    Wall: Duration.FromMilliseconds(solver.WallTime()))))
            : Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
    }

    // Exact rows report the search they RAN: `Generations` takes the explored count the engine measured, never the
    // literal 1 that reads as one iteration on every receipt whatever the tree cost. The verdict reads the engine's
    // own OPTIMALITY GAP, which is the one honest convergence measure an exact search publishes — a `Feasible`
    // return whose bound never met its incumbent is `Exhausted` at the nodes it explored, and an evidence-free
    // harvest claims nothing.
    static Fin<KernelRun> Harvest(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed, ImmutableArray<double> coordinates, Option<ExactEvidence> evidence) =>
        Optimizer.Probe(problem, oracle, coordinates).Map(point => new KernelRun(
            seed.Insert(point),
            evidence.Map(static held => (int)Math.Min(held.Explored, int.MaxValue)).IfNone(1),
            evidence.Bind(static held => held.Gap).Match(
                Some: gap => gap <= policy.Tolerance ? new Convergence.Converged(gap) : new Convergence.Exhausted(evidence.Map(static held => (int)Math.Min(held.Explored, int.MaxValue)).IfNone(1)),
                None: static () => (Convergence)new Convergence.Stalled()),
            policy.TrustRadius,
            Seq(point.Violation),
            evidence));

    static double DiscreteValue(DesignVariable variable, long raw, double step) =>
        variable is DesignVariable.Continuous or DesignVariable.Symbolic ? raw * step : variable.Clamp(raw);

}
```

## [03]-[ROUTING_ROW_SHAPE]

- Owner: `RoutingNode`/`RoutingVehicle`/`RoutingDimensionSpec` the typed routing vocabulary; `RoutingProblem` the validated node/vehicle/dimension record with its per-dimension transit costs; `RoutingResult` the per-vehicle sequence answer with its total cost, dropped nodes, and engine status; `RoutingSearch` the lowering fold the `routing` row dispatches to.
- Entry: the `routing` `OptimizerKind` row on the one `Optimize` total `Switch`; `RoutingSearch.Solve` reads `DesignProblem.Routing` and faults `<routing-needs-model>` when absent, exactly as the exact rows fault on an absent `LinearModel`.
- Cases: a node carries an optional demand and an optional `(Open, Close)` time window; a vehicle carries its capacity and its start and end depot node; a dimension spec names the accumulated quantity, its per-vehicle capacity, and its slack. `RoutingSearchStatus.Types.Value` classifies the outcome — `NOT_SOLVED`, `FAIL`, and `FAIL_TIMEOUT` are typed faults, `SUCCESS` and its partial sibling carry the assignment.
- Packages: Google.OrTools (the `Google.OrTools.ConstraintSolver` `RoutingIndexManager`/`RoutingModel`/`RoutingDimension`/`Assignment` rail beside the CP-SAT and LinearSolver rails already admitted)
- Growth: a new accumulated quantity is one `RoutingDimensionSpec` row; a new per-node obligation (a pickup-delivery pair, an optional-visit penalty, a visit-type incompatibility) is one column on `RoutingNode` and one cataloged `RoutingModel` call in the lowering; a further engine measure is one field on `ExactEvidence`, the same carrier the CP-SAT and MILP rows publish; zero new surface — a `VrpSolver`/`TspSolver`/`CvrpSolver` sibling family collapses onto this one row.
- Boundary: routing is a GRAPH program — nodes, arcs, and vehicles — so it lowers to the ConstraintSolver rail rather than to a coefficient matrix, and a routing problem forced through the `LinearModel` as a flattened assignment matrix is the rejected form. Cost is a typed `Func<int,int,long>` PER DIMENSION over caller node indices, and the manager owns the caller-index-to-solver-index mapping — a callback registered against raw solver indices reads a different graph than the one authored. Native handles enter through the declared `IDisposable` roots the OR-Tools circulation precedent already sets and release by `Dispose`. `RoutingResult` publishes its `ExactEvidence` beside the assignment so a routing solve is auditable on the same receipt slots the other two exact rails fill.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// One caller-indexed node. Demand and time window are OPTIONAL because a depot carries neither and a plain TSP node
// carries neither — an absent window is genuinely unconstrained, never a `(0, long.MaxValue)` sentinel a dimension
// would then enforce as a real bound.
public readonly record struct RoutingNode(int Id, Option<long> Demand, Option<(long Open, long Close)> Window) {
    public bool Invalid => Id < 0 || Demand.Exists(static d => d < 0L) || Window.Exists(static w => w.Open > w.Close);
}

public readonly record struct RoutingVehicle(long Capacity, int Start, int End) {
    public bool Invalid => Capacity < 0L || Start < 0 || End < 0;
}

// One accumulated quantity over a route — load, elapsed time, distance. `Slack` is the waiting a vehicle may absorb
// at a node, which is what makes a time-window dimension solvable at all.
public readonly record struct RoutingDimensionSpec(string Name, long Capacity, long Slack) {
    public bool Invalid => string.IsNullOrWhiteSpace(Name) || Capacity < 0L || Slack < 0L;
}

public sealed record RoutingProblem(
    Seq<RoutingNode> Nodes,
    Seq<RoutingVehicle> Vehicles,
    Seq<RoutingDimensionSpec> Dimensions,
    // Arc cost is the FIRST transit row; every further row is the accumulation its matching dimension spec bounds.
    // A `Func<int,int,long>` over CALLER node indices keeps the authored graph and the solver's internal indexing
    // separate, which is the whole reason the index manager exists.
    Seq<Func<int, int, long>> Transit) {
    public Fin<Unit> Validate() =>
        Nodes.Count < 2 || Vehicles.IsEmpty || Transit.IsEmpty
        || Transit.Count != Dimensions.Count + 1
        || Nodes.Exists(static n => n.Invalid) || Vehicles.Exists(static v => v.Invalid) || Dimensions.Exists(static d => d.Invalid)
        || Nodes.Map(static n => n.Id).ToHashSet().Count != Nodes.Count
        || Vehicles.Exists(v => v.Start >= Nodes.Count || v.End >= Nodes.Count)
        || Dimensions.Map(static d => d.Name).ToHashSet(StringComparer.Ordinal).Count != Dimensions.Count
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
            : Fin.Succ(unit);
}

// Per-vehicle visit sequences in caller node indices, the total arc cost, and the nodes no vehicle served. Dropped
// nodes are EVIDENCE: a disjunction-carrying model answers with an unserved roster, and reporting only the routes
// hides exactly the demand the campaign could not meet.
public sealed record RoutingResult(Seq<Seq<int>> Sequences, long TotalCost, Seq<int> Dropped, string Status);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class RoutingSearch {
    // The lowering spine, in the catalogued spellings alone: the manager maps caller nodes onto solver indices, the
    // model registers one transit callback per cost row, the first row becomes the global arc cost, and each further
    // row becomes its dimension. Every native root is bracketed by `using`, matching the OR-Tools disposal law the
    // circulation runner already sets.
    public static Fin<KernelRun> Solve(DesignProblem problem, OptimizerPolicy policy, SearchContext search, ParetoFront seed) =>
        problem.Routing.Match(
            Some: model => policy.Routing.Invalid
                ? Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
                : model.Validate().Bind(_ => Lower(model, policy, search, seed)),
            None: () => Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Resource))));

    static Fin<KernelRun> Lower(RoutingProblem model, OptimizerPolicy policy, SearchContext search, ParetoFront seed) =>
        Op.Of(name: "routing.solve").Catch(() => {
            using RoutingIndexManager manager = new(model.Nodes.Count, model.Vehicles.Count,
                [.. model.Vehicles.Map(static v => v.Start)], [.. model.Vehicles.Map(static v => v.End)]);
            using RoutingModel routing = new(manager);
            // Callbacks take SOLVER indices and the authored costs take CALLER nodes, so every registration crosses
            // the manager — a callback reading raw indices scores a different graph than the one written.
            int[] callbacks = [.. model.Transit.Map(cost => routing.RegisterTransitCallback(
                (long from, long to) => cost(Node(manager, from), Node(manager, to))))];
            routing.SetArcCostEvaluatorOfAllVehicles(callbacks[0]);
            model.Dimensions.Iter((spec, index) => routing.AddDimensionWithVehicleCapacity(
                callbacks[index + 1], spec.Slack, [.. model.Vehicles.Map(static v => v.Capacity)], fix_start_cumul_to_zero: true, spec.Name));
            return Harvest(manager, routing, model, policy, search, seed);
        });

    // Every routing read below is decompile-verified on the pinned assembly: the default-parameter proto factory,
    // the dimension-level time-window bound (`SetCumulVarRange` — the range member lives on the DIMENSION, never the
    // cumulative IntVar), the Start/IsEnd/NextVar walk read through `Assignment.Value`, and the terminal `GetStatus()`.
    static Fin<KernelRun> Harvest(RoutingIndexManager manager, RoutingModel routing, RoutingProblem model, OptimizerPolicy policy, SearchContext search, ParetoFront seed) {
        RoutingSearchParameters parameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        parameters.FirstSolutionStrategy = policy.Routing.FirstSolution;
        parameters.LocalSearchMetaheuristic = policy.Routing.Metaheuristic;
        parameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = (long)policy.Routing.Limit.TotalSeconds };
        if (policy.Routing.SolutionLimit > 0) { parameters.SolutionLimit = policy.Routing.SolutionLimit; }
        model.Dimensions.Iter(spec => {
            RoutingDimension dimension = routing.GetDimensionOrDie(spec.Name);
            for (int node = 0; node < model.Nodes.Count; node++) {
                model.Nodes[node].Window.IfSome(window => dimension.SetCumulVarRange(manager.NodeToIndex(node), window.Open, window.Close));
            }
        });
        using Assignment solution = routing.SolveWithParameters(parameters);
        RoutingSearchStatus.Types.Value status = routing.GetStatus();
        if (solution is null) { return Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Feasible, new ContractEvidence.Status((int)status)))); }
        Seq<Seq<int>> sequences = default;
        for (int vehicle = 0; vehicle < model.Vehicles.Count; vehicle++) {
            Seq<int> stops = default;
            for (long index = routing.Start(vehicle); !routing.IsEnd(index); index = solution.Value(routing.NextVar(index))) {
                stops = stops.Add(Node(manager, index));
            }
            sequences = sequences.Add(stops);
        }
        // The unserved roster is a SET DIFFERENCE over immutable sets, not two BCL hash sets in a domain fold; and
        // it can only be non-empty once the model carries disjunctions, which is why the column names its own
        // producer gap rather than reading as a measured zero.
        Set<int> served = toSet(sequences.Bind(static s => s));
        Set<int> depots = toSet(model.Vehicles.Bind(static v => Seq(v.Start, v.End)));
        Seq<int> dropped = toSeq(Range(0, model.Nodes.Count).Filter(node => !served.Contains(node) && !depots.Contains(node)));
        RoutingResult result = new(sequences, solution.ObjectiveValue(), dropped, status.ToString());
        // The ConstraintSolver either returns an assignment or it does not — the null case already faulted above —
        // so a returned assignment is a settled search at zero residual and no budget was left unspent.
        return Fin.Succ(new KernelRun(seed, Generations: 0, new Convergence.Converged(Residual: 0.0), policy.TrustRadius, Violation: default, Routing: Some(result)));
    }

    static int Node(RoutingIndexManager manager, long index) => manager.IndexToNode(index);
}
```

## [04]-[ROUTING_SEARCH_POLICY]

- Owner: `RoutingPolicy` the first-solution/metaheuristic/limit record the lowering builds its `RoutingSearchParameters` from.
- Cases: `Canonical` is path-cheapest-arc construction, guided-local-search improvement, and a thirty-second limit — the OR-Tools reference pairing for a capacitated problem, where a cheap constructive route feeds a metaheuristic that can escape the local optimum it lands in.
- Boundary: search behaviour is POLICY DATA, never call-site knobs — a caller that tunes the metaheuristic by passing enum values into the kernel forks the tuning across every call site and leaves the receipt unable to say which search ran. `RoutingPolicy` rides `OptimizerPolicy` the same way `LineSearch` and `AcquisitionFunction` do, and the chosen strategy names land on the routing evidence so a slow solve is diagnosable from its own receipt.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// The search the routing row runs, as data. Both enum columns are the OR-Tools proto value types the catalogue
// names, so a row selects a strategy rather than a call site constructing one.
public sealed record RoutingPolicy(
    FirstSolutionStrategy.Types.Value FirstSolution,
    LocalSearchMetaheuristic.Types.Value Metaheuristic,
    Duration Limit,
    int SolutionLimit) {
    public static readonly RoutingPolicy Canonical = new(
        FirstSolutionStrategy.Types.Value.PathCheapestArc,
        LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch,
        Duration.FromSeconds(30),
        SolutionLimit: 0);

    public bool Invalid => Limit <= Duration.Zero || SolutionLimit < 0;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
