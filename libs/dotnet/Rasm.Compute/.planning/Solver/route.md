# [COMPUTE_SOLVER_ROUTE]

Rasm.Compute solve execution: the eight route bodies the `Solver/contract#SOLVE_REQUEST` dispatch names, the adaptive-recovery ladder that re-attempts a refused one, the archive sessions their artifacts land through, and the multi-physics fold that runs a set of them under Aitken relaxation. Every body takes the same `RouteRequest` — the constrained system, the mesh, the admitted problem, the lane policy, the route case, the clock, and the optional archive and factorization capabilities — so a route grows by one case and one arm rather than by a signature.

Every bounded fold on the page runs the ONE `Fixpoint` iteration: a step either advances the state or settles it with a MEASURED residual, and running the budget out is `Convergence.Exhausted` carrying that budget. The nine hand folds this replaced each threaded a `bool Converged` through a tuple, and each of them could return a state its own convergence test never accepted while the flag said otherwise.

Terminal evidence is likewise measured, never stamped: a direct factorization on an ill-conditioned operator returns exactly the field a `(0.0, true)` verdict would bless, and a march that went unstable in its last decade reports identically to a settled one unless the residual is recomputed at the state the route actually ends on.

## [01]-[INDEX]

- [02]-[SOLVE_ROUTES]: linear, transient, nonlinear, modal, and buckling bodies over one request carrier and one bounded fixpoint.
- [03]-[SOLVE_RECOVERY]: the recovery ladder, its re-attempt schedule, and the recovery receipt.
- [04]-[SOLVE_ARCHIVE]: the route-borne archive capability and its three container sessions.
- [05]-[COUPLED_FIELDS]: field-transfer pairing, staggering, and Aitken relaxation over a field set.

## [02]-[SOLVE_ROUTES]

- Owner: `RouteRequest` the one request carrier every body takes; `MarchRequest` the transient carrier the integrator row's own body reads; `Step<TState>` the continue-or-settle carrier; `Fixpoint` the ONE bounded fold; `ResidualEvaluation` the per-evaluation internal force, trial ledger, trial multipliers, and consistent tangent; `DofSplit`/`Reduced` the condensation carriers; `SolveRoutes` the bodies.
- Entry: every body is `static Fin<SolveResult> <Route>(RouteRequest request, <Case> row)`, reached from the one `SolveLane.Solve` `Switch` and from nowhere else. `Traced` is the `Tensor/quadrature#TRAJECTORY_DRIVER` consumer: the semi-discrete first-order system integrates on `Trajectory.Trace` over a `FieldCarrier` module, and the driver's `TrajectoryTerminal` and `ConvergenceClaim` land as the result's `Convergence` verdict and its `QuadratureEvidence`.
- Auto: the transient route reads its integrator ROW's own march body, so the three-deep ternary over form, implicitness, and mass singularity becomes one form test and one row delegate; the modal routes are separate cases, so the buckling-versus-condensed ternary and the condensation `Option` match both delete with the union that carries the payload.
- Receipt: the result's `Convergence` verdict, iteration count, and measured residual land on `Solver/contract`'s `Solve` receipt; the condensed route adds its `CondensationEvidence`.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, LanguageExt.Core (`Fin`/`Option`/`IO.Bracket`/`Schedule`), NodaTime, Thinktecture.Runtime.Extensions
- Growth: a new route is one `SolveRoute` case, one dispatch arm, and one body here; a new time scheme is one `TimeIntegrator` row carrying its march body; zero new surface.
- Boundary: static condensation is the modal lane's ONE sparse lowering and it composes owners rather than minting an eigensolver — the condensed block factors through `FactorKind.Spd`, the transformation columns ride `FactoredOp.Solve` under its own true-residual witness, the block sweeps ride the held `SparseTensorOps.Spmv` in both directions so no transposed storage materializes, and the retained pencil terminates on `Admission.Definite`, `DenseOps.Decompose`, and `SpectralOps.Decompose`. Kernel `Numerics/matrix` generalized-eigen rivals none of that: it densifies both operands at full order and factors the WHOLE inertia operator, so it neither scales at building order nor admits a singular lumped mass.
- Boundary: every dense terminal is ceilinged and every ceiling refuses by name with its measured quantity — `LanePolicy.MaxDenseDofs` over the whole-operator modal and buckling routes (the modal refusal naming the condensed route that does serve the model) and `CondensationPolicy.MaxTransformBytes` over the `retained × condensed` transformation the reduction cannot stream. An allocation the machine answers with an out-of-memory leaves a receipt that explains nothing.
- Boundary: implicit schemes add stiffness and damping to the effective operator, so an inertia-free row still carries a solvable diagonal; the explicit scheme DIVIDES by inertia, so the same row is unmarchable and a guard would freeze it at zero and publish that as motion. A frame's rotational rows are exactly those rows, so the explicit integrator refuses the model by naming the row.
- Boundary: contact is nonlinear-only. Its ids name the base dof of a translational triple, the gap projects onto the constraint normal, and ONE `ContactEnforcement.Enforce` per residual evaluation returns the force, the gap-space stiffness, and the advanced multipliers. Both derivative legs project through `∂g/∂u = ±w·n`: the force scatters over the triple and the stiffness scatters as `h·n⊗n` over the pair's four blocks through a re-ingest, because the elastic sparsity holds no slot for a coupling no element makes.
- Boundary: material history is path-dependent across steps and NOT across probes. Residual evaluations evolve TRIAL rows from the committed ledger, line-search probes and rejected iterations never advance history, and only a converged load or arc step commits its trial ledger and contact multipliers.
- Boundary: the error-controlled march ELECTS its `StepLaw` on the `FieldIntegrator` it mints and composes the `Trajectory` driver; it never calls the kernel `FieldIntegrator.Step` itself. `Step` is a PURE step function the kernel deliberately surrounds with no run loop — reject retry, underflow floor, step budget, and the `StepHistory` thread a PI or Gustafsson law reads are all the driver's, and a second loop here would re-derive the four run-level facts `Trajectory` already partitions. A route electing a fixed integrator therefore reports `ConvergenceClaim.Unwitnessed`, and unless its own accuracy intent said `RequireErrorWitness = false` it REFUSES rather than publishing an unmeasured march under a converged verdict.
- Boundary: a route holding a live archive session brackets it — the session releases on the refusing path exactly as on the settling one, because a `using` inside a `Bind` lambda leaks the handle on every rail the lambda does not return through.
- Exemption: the block partition walk, the transformation column march, the pencil sweep, the geometric-stiffness scatter, and the mode recovery are MEASURED span kernels over indices the partition already carries; each dies with the reduction that fills it.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

public readonly record struct Step<TState>(TState State, Option<double> Settled) {
    public static Step<TState> Advance(TState state) => new(state, None);
    public static Step<TState> Settle(TState state, double residual) => new(state, Some(residual));
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record RouteRequest(
    ConstrainedSystem System,
    DiscreteMesh Mesh,
    SolveProblem Problem,
    LanePolicy Policy,
    SolveRoute Route,
    IClock Clock,
    Option<SolveArchive> Archive,
    Option<SolveSession> Session) {
    public int Dofs => System.Rhs.Length;
    public Instant At => Clock.GetCurrentInstant();

    public SolveResult Settled(ReadOnlyMemory<double> field, int iterations, int newtonSteps, Convergence verdict) =>
        new(Problem, Policy.Method, Route, field, None, None, None, Dofs, iterations, newtonSteps, verdict, At);

    public Fin<double[]> Solve(SparseCompressedRowMatrixStorage<double> operatorCsr, double[] rhs, FactorKind kind) =>
        Session.Match(
            Some: session => session.Solve(operatorCsr, rhs, Policy.Tolerance.Value * 1e3),
            None: () => SparseOps.Factor(operatorCsr, kind, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
                .Bind(factored => factored.Solve(rhs, Policy.Tolerance.Value * 1e3)));
}

public sealed record MarchRequest(RouteRequest Request, SolveRoute.Transient Grid, double[] Lumped);

public sealed record ResidualEvaluation(
    double[] InternalForce, ConstitutiveState[] Trial, Seq<double[]> ContactMultipliers, SparseCompressedRowMatrixStorage<double> Tangent);

public readonly record struct DofSplit(int[] Masters, int[] Slaves, int[] MasterOf, int[] SlaveOf) {
    public long TransformBytes => (long)Masters.Length * Slaves.Length * sizeof(double);
}

public readonly record struct Reduced(Matrix<double> Stiffness, Matrix<double> Mass, double[][] Transform, double Residual, double Conditioning);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Fixpoint {
    public static Fin<(TState State, int Steps, Convergence Verdict)> Run<TState>(
        TState seed, Dimension budget, Func<TState, int, Fin<Step<TState>>> step) =>
        toSeq(Enumerable.Range(0, budget.Value)).Fold(
            Fin.Succ((State: seed, Steps: 0, Verdict: (Convergence)new Convergence.Exhausted(budget.Value))),
            (acc, index) => acc.Bind(state => state.Verdict is Convergence.Converged
                ? Fin.Succ(state)
                : step(state.State, index).Map(next => next.Settled.Match(
                    Some: residual => (next.State, state.Steps + 1, (Convergence)new Convergence.Converged(residual)),
                    None: () => (next.State, state.Steps + 1, state.Verdict)))));
}

public static class SolveRoutes {
    static Convergence Terminal(SparseCompressedRowMatrixStorage<double> csr, double[] rhs, double[] field, double tolerance) {
        double[] applied = new SparseMatrix(csr).Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray();
        double residual = TensorPrimitives.Norm<double>(Difference(rhs, applied)) / Math.Max(1.0, TensorPrimitives.Norm<double>(rhs));
        return residual <= tolerance ? new Convergence.Converged(residual) : new Convergence.Stalled();
    }

    static double[] Difference(double[] forcing, double[] applied) {
        double[] residual = (double[])forcing.Clone();
        TensorPrimitives.Subtract(residual, applied, residual);
        return residual;
    }

    public static Fin<SolveResult> Direct(RouteRequest request) =>
        request.Solve(request.System.Operator, request.System.Rhs,
                request.Policy.Method.Kind == FactorizationKind.Cholesky ? FactorKind.Spd : FactorKind.Lu)
            .Map(field => request.Settled(field.AsMemory(), 1, 1,
                Terminal(request.System.Operator, request.System.Rhs, field, request.Policy.Tolerance.Value)));

    public static Fin<SolveResult> Iterative(RouteRequest request) =>
        request.Policy.Method.Krylov.ToFin(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.IterativeSolver)))
            .Bind(krylov => SparseOps.SolveIterative(request.System.Operator, krylov, request.System.Rhs, request.Policy.Iteration(request.Clock)))
            .Map(run => request.Settled(run.Field.ToArray().AsMemory(), 0, 1,
                run.Terminal is SolveTerminal.Admitted ? new Convergence.Converged(run.Residual) : new Convergence.Stalled()));

    public static Fin<SolveResult> March(RouteRequest request, SolveRoute.Transient grid) =>
        OperatorAssembly.Lumped(request.Mesh, request.Problem).Bind(lumped =>
            request.Problem.Physics.Form != MaterialForm.Elasticity
                ? FirstOrder(new MarchRequest(request, grid, OperatorAssembly.Capacity(request.Problem, lumped)))
                : grid.Integrator.Advance(new MarchRequest(request, grid, lumped)));

    static Fin<SolveResult> FirstOrder(MarchRequest march) {
        RouteRequest request = march.Request;
        int n = request.Dofs;
        double dt = march.Grid.Step.Value;
        double[] capacity = (double[])march.Lumped.Clone();
        foreach (long constrained in request.System.Constrained) { capacity[(int)constrained] = 0.0; }
        double[] effective = (double[])request.System.Operator.Values.Clone();
        int[] rows = request.System.Operator.RowPointers, columns = request.System.Operator.ColumnIndices;
        for (int row = 0; row < n; row++) { BoundaryCondition.AddAt(request.System.Operator, effective, row, row, capacity[row] / dt); }
        SparseCompressedRowMatrixStorage<double> effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(n, n, effective.Length, rows, columns, effective);
        return Sealed(request, march.Grid, n, kinematic: false, history =>
            Fixpoint.Run((Field: new double[n], Forcing: (double[])request.System.Rhs.Clone()), march.Grid.Steps, (state, step) => {
                double[] forcing = (double[])request.System.Rhs.Clone();
                for (int i = 0; i < n; i++) { forcing[i] += capacity[i] * state.Field[i] / dt; }
                return request.Solve(effectiveCsr, forcing, FactorKind.Lu)
                    .Bind(field => history.Step(step, field).Map(_ => Step.Advance((Field: field, Forcing: forcing))));
            }))
            .Map(run => request.Settled(run.State.Field.AsMemory(), run.Steps, 1,
                Terminal(effectiveCsr, run.State.Forcing, run.State.Field, request.Policy.Tolerance.Value)));
    }

    public static Fin<SolveResult> Newmark(MarchRequest march) {
        RouteRequest request = march.Request;
        int n = request.Dofs;
        double dt = march.Grid.Step.Value, beta = march.Grid.Integrator.Beta, gamma = march.Grid.Integrator.Gamma;
        RayleighPair damping = request.Policy.Damping;
        ReadOnlySpan<double> stiffness = request.System.Operator.Values;
        int[] rowPtr = request.System.Operator.RowPointers, colIdx = request.System.Operator.ColumnIndices;
        double[] massEntry = new double[stiffness.Length], viscous = new double[stiffness.Length];
        for (int row = 0; row < n; row++)
            for (int slot = rowPtr[row]; slot < rowPtr[row + 1]; slot++) {
                massEntry[slot] = colIdx[slot] == row ? march.Lumped[row] : 0.0;
                viscous[slot] = damping.Mass * massEntry[slot] + damping.Stiffness * stiffness[slot];
            }
        double[] effective = march.Grid.Integrator.Effective(massEntry, viscous, stiffness, dt);
        SparseCompressedRowMatrixStorage<double> effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            n, n, effective.Length, rowPtr, colIdx, effective);
        SparseMatrix tangent = new(request.System.Operator);
        NewmarkCoefficients coefficients = NewmarkCoefficients.Of(march.Grid.Integrator, dt);
        return Sealed(request, march.Grid, n, kinematic: true, history =>
            Fixpoint.Run((U: new double[n], V: new double[n], A: new double[n]), march.Grid.Steps, (state, step) =>
                request.Solve(effectiveCsr, NewmarkForce(request.System.Rhs, march.Lumped, tangent, state, damping, coefficients), FactorKind.Lu)
                    .Map(next => Correct(next, state, coefficients))
                    .Bind(corrected => history.Step(step, corrected.U, corrected.V, corrected.A).Map(_ => Step.Advance(corrected)))))
            .Map(run => request.Settled(run.State.U.AsMemory(), run.Steps, 1,
                Equilibrium(request.System.Rhs, tangent, march.Lumped, damping, run.State, request.Policy.Tolerance.Value)));
    }

    readonly record struct NewmarkCoefficients(double A0, double A1, double A2, double A3, double A4, double A5, double A6, double A7) {
        public static NewmarkCoefficients Of(TimeIntegrator integrator, double dt) {
            double beta = integrator.Beta, gamma = integrator.Gamma;
            return new(1.0 / (beta * dt * dt), gamma / (beta * dt), 1.0 / (beta * dt), 1.0 / (2.0 * beta) - 1.0,
                gamma / beta - 1.0, dt * 0.5 * (gamma / beta - 2.0), dt * (1.0 - gamma), gamma * dt);
        }
    }

    static double[] NewmarkForce(
        double[] forcing, double[] mass, SparseMatrix tangent, (double[] U, double[] V, double[] A) state,
        RayleighPair damping, NewmarkCoefficients c) {
        int n = forcing.Length;
        double[] massCombo = new double[n], dampCombo = new double[n];
        double[] priorStiffness = tangent.Multiply(Vector<double>.Build.DenseOfArray(state.U)).AsArray();
        for (int i = 0; i < n; i++) {
            massCombo[i] = mass[i] * (c.A0 * state.U[i] + c.A2 * state.V[i] + c.A3 * state.A[i]);
            dampCombo[i] = c.A1 * state.U[i] + c.A4 * state.V[i] + c.A5 * state.A[i];
        }
        double[] stiffnessLeg = tangent.Multiply(Vector<double>.Build.DenseOfArray(dampCombo)).AsArray();
        double[] force = new double[n];
        for (int i = 0; i < n; i++) {
            force[i] = forcing[i] + massCombo[i] + damping.Mass * mass[i] * dampCombo[i] + damping.Stiffness * stiffnessLeg[i];
        }
        return force;
    }

    static (double[] U, double[] V, double[] A) Correct(double[] next, (double[] U, double[] V, double[] A) prior, NewmarkCoefficients c) {
        int n = next.Length;
        double[] accel = new double[n], velocity = new double[n];
        for (int i = 0; i < n; i++) {
            accel[i] = c.A0 * (next[i] - prior.U[i]) - c.A2 * prior.V[i] - c.A3 * prior.A[i];
            velocity[i] = prior.V[i] + c.A6 * prior.A[i] + c.A7 * accel[i];
        }
        return (next, velocity, accel);
    }

    static Convergence Equilibrium(
        double[] forcing, SparseMatrix stiffness, double[] mass, RayleighPair damping,
        (double[] U, double[] V, double[] A) state, double tolerance) {
        double[] elastic = stiffness.Multiply(Vector<double>.Build.DenseOfArray(state.U)).AsArray();
        double[] viscous = stiffness.Multiply(Vector<double>.Build.DenseOfArray(state.V)).AsArray();
        double[] residual = new double[forcing.Length];
        for (int i = 0; i < residual.Length; i++) {
            residual[i] = forcing[i] - mass[i] * state.A[i] - damping.Mass * mass[i] * state.V[i] - damping.Stiffness * viscous[i] - elastic[i];
        }
        double norm = TensorPrimitives.Norm<double>(residual) / Math.Max(1.0, TensorPrimitives.Norm<double>(forcing));
        return norm <= tolerance ? new Convergence.Converged(norm) : new Convergence.Stalled();
    }

    public static Fin<SolveResult> CentralDifference(MarchRequest march) {
        RouteRequest request = march.Request;
        double floor = OperatorAssembly.InertiaFloor(march.Lumped);
        return OperatorAssembly.MassSingular(march.Lumped, request.System.Constrained, floor).Match(
            Some: dof => Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))),
            None: () => Explicit(march));
    }

    static Fin<SolveResult> Explicit(MarchRequest march) {
        RouteRequest request = march.Request;
        int n = request.Dofs;
        double dt = march.Grid.Step.Value, dt2 = dt * dt;
        RayleighPair damping = request.Policy.Damping;
        SparseMatrix tangent = new(request.System.Operator);
        double[] effMass = new double[n];
        for (int i = 0; i < n; i++) { effMass[i] = march.Lumped[i] / dt2 + damping.Mass * march.Lumped[i] / (2.0 * dt); }
        return Sealed(request, march.Grid, n, kinematic: false, history =>
            Fixpoint.Run((Curr: new double[n], Prev: new double[n], Prior: new double[n]), march.Grid.Steps, (state, step) => {
                double[] internalForce = tangent.Multiply(Vector<double>.Build.DenseOfArray(state.Curr)).AsArray();
                double[] lagged = new double[n];
                for (int i = 0; i < n; i++) { lagged[i] = (state.Curr[i] - state.Prev[i]) / dt; }
                double[] viscous = tangent.Multiply(Vector<double>.Build.DenseOfArray(lagged)).AsArray();
                double[] next = new double[n];
                for (int i = 0; i < n; i++) {
                    double rhs = request.System.Rhs[i] - internalForce[i] - damping.Stiffness * viscous[i]
                        + march.Lumped[i] / dt2 * (2.0 * state.Curr[i] - state.Prev[i])
                        + damping.Mass * march.Lumped[i] / (2.0 * dt) * state.Prev[i];
                    next[i] = rhs / effMass[i];
                }
                return history.Step(step, next).Map(_ => Step.Advance((Curr: next, Prev: state.Curr, Prior: state.Prev)));
            }))
            .Map(run => {
                double[] velocity = new double[n], acceleration = new double[n];
                for (int i = 0; i < n; i++) {
                    velocity[i] = (run.State.Curr[i] - run.State.Prior[i]) / (2.0 * dt);
                    acceleration[i] = (run.State.Curr[i] - 2.0 * run.State.Prev[i] + run.State.Prior[i]) / dt2;
                }
                return request.Settled(run.State.Curr.AsMemory(), run.Steps, 1,
                    Equilibrium(request.System.Rhs, tangent, march.Lumped, damping,
                        (run.State.Prev, velocity, acceleration), request.Policy.Tolerance.Value));
            });
    }

    static Fin<(TState State, int Steps, Convergence Verdict)> Sealed<TState>(
        RouteRequest request, SolveRoute.Transient grid, int dofs, bool kinematic,
        Func<SolveHistory, Fin<(TState State, int Steps, Convergence Verdict)>> march) =>
        SolveHistory.Open(request.Archive, request.Problem, grid, dofs, kinematic)
            .Bind(history => IO.pure(history)
                .Bracket(Use: open => IO.lift(() => march(open)), Fin: open => IO.lift(fun(open.Dispose)))
                .Run());

    public static Fin<SolveResult> Traced(RouteRequest request, SolveRoute.Traced row) =>
        OperatorAssembly.Lumped(request.Mesh, request.Problem).Bind(lumped => {
            double[] capacity = OperatorAssembly.Capacity(request.Problem, lumped);
            return OperatorAssembly.MassSingular(capacity, request.System.Constrained, OperatorAssembly.InertiaFloor(capacity)).Match(
                Some: dof => Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))),
                None: () => Trace(request, row, capacity));
        });

    static Fin<SolveResult> Trace(RouteRequest request, SolveRoute.Traced row, double[] capacity) {
        int n = request.Dofs;
        SparseMatrix tangent = new(request.System.Operator);
        Func<FieldState, Fin<FieldState>> field = state => {
            double[] elastic = tangent.Multiply(Vector<double>.Build.DenseOfArray([.. state.Values])).AsArray();
            double[] rate = new double[n];
            for (int i = 0; i < n; i++) { rate[i] = (request.System.Rhs[i] - elastic[i]) / capacity[i]; }
            return TensorPrimitives.IsFiniteAll<double>(rate)
                ? Fin.Succ(new FieldState(1.0, [.. rate]))
                : Fin.Fail<FieldState>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(rate.Length))));
        };
        TrajectorySpec<FieldState, FieldState> spec = new(
            Integrator: row.Integrator,
            Carrier: state => FieldCarrier.Of(state, row.Control),
            Field: field,
            Initial: new FieldState(0.0, [.. new double[n]]),
            Start: 0.0,
            Horizon: row.Control.MaxStep * row.Control.MaxSteps,
            FirstStep: row.Control.MinStep,
            Stations: row.Stations,
            Project: static state => state.Values.AsSpan());
        return Trajectory.Trace(spec: spec, control: row.Control, key: TraceKey)
            .Bind(run => Witnessed(request, row, run));
    }

    private static readonly Op TraceKey = Op.Of(name: "solve-traced");

    static Fin<SolveResult> Witnessed(RouteRequest request, SolveRoute.Traced row, TrajectoryRun<FieldState> run) {
        ConvergenceClaim claim = run.LastError.IsSome ? ConvergenceClaim.Estimated : ConvergenceClaim.Unwitnessed;
        return claim == ConvergenceClaim.Unwitnessed && row.Accuracy.RequireErrorWitness
            ? Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Keys(row.Integrator.Kind.Key, run.Terminal.Key))))
            : run.Terminal == TrajectoryTerminal.NonFinite
                ? Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(run.Steps))))
                : Fin.Succ(request.Settled([.. run.Final.Values], run.Steps, 1, Verdict(run, row)) with {
                    Evidence = Some(new QuadratureEvidence(
                        Value: run.Achieved, Error: run.LastError, L1Norm: None, Ratio: None,
                        Skipped: run.Rejects, Claim: claim)),
                });
    }

    static Convergence Verdict(TrajectoryRun<FieldState> run, SolveRoute.Traced row) =>
        run.Terminal == TrajectoryTerminal.Completed
            ? new Convergence.Converged(run.LastError.IfNone(0.0))
            : run.Terminal == TrajectoryTerminal.BudgetExhausted
                ? new Convergence.Exhausted(row.Control.MaxSteps)
                : new Convergence.Stalled();

    static ConstitutiveState[] Pristine(DiscreteMesh mesh, SolveProblem problem, ConstitutiveModel model) {
        int components = model is ConstitutiveModel.Hyperelastic ? 9 : problem.Physics.StrainDim;
        return [.. Enumerable.Range(0, checked((int)mesh.ElementCount) * mesh.Rule.Points.Length)
            .Select(_ => ConstitutiveState.Pristine(components))];
    }

    public static Fin<SolveResult> NewtonLoad(RouteRequest request, SolveRoute.Nonlinear row) {
        SparseMatrix tangent = new(request.System.Operator);
        ConstitutiveState[] committed = request.Problem.Material.Match(
            Some: law => Pristine(request.Mesh, request.Problem, law.Model), None: static () => []);
        double scale = Math.Max(1.0, TensorPrimitives.Norm<double>(request.System.Rhs));
        return Fixpoint.Run(
            (Field: new double[request.Dofs], Committed: committed, Multipliers: Seq<double[]>()),
            row.NewtonIterations,
            (state, _) => InternalForce(request, tangent, state.Field, state.Committed, state.Multipliers).Bind(evaluation => {
                double[] residual = Difference(request.System.Rhs, evaluation.InternalForce);
                double norm = TensorPrimitives.Norm<double>(residual);
                return norm <= request.Policy.Tolerance.Value * scale
                    ? Fin.Succ(Step.Settle((state.Field, evaluation.Trial, evaluation.ContactMultipliers), norm / scale))
                    : request.Policy.Method.Krylov.ToFin(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.IterativeSolver)))
                        .Bind(krylov => SparseOps.SolveIterative(evaluation.Tangent, krylov, residual, request.Policy.Iteration(request.Clock)))
                        .Bind(run => LineSearch(request, tangent, state.Field, run.Field.ToArray(), norm, state.Committed, state.Multipliers)
                            .Map(alpha => {
                                double[] updated = new double[state.Field.Length];
                                TensorPrimitives.MultiplyAdd(run.Field, alpha, state.Field, updated);
                                return Step.Advance((updated, state.Committed, state.Multipliers));
                            }));
            }))
            .Bind(run => run.Verdict is Convergence.Converged
                ? Fin.Succ(request.Settled(run.State.Field.AsMemory(), run.Steps, run.Steps, run.Verdict))
                : Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.Count(run.Steps, row.NewtonIterations.Value)))));
    }

    static Fin<double> LineSearch(
        RouteRequest request, SparseMatrix tangent, double[] field, double[] direction, double baseline,
        ConstitutiveState[] committed, Seq<double[]> multipliers) =>
        Fixpoint.Run(1.0, LineSearchBudget, (alpha, _) => {
            double[] trial = new double[field.Length];
            TensorPrimitives.MultiplyAdd(direction, alpha, field, trial);
            return InternalForce(request, tangent, trial, committed, multipliers)
                .Map(evaluation => TensorPrimitives.Norm<double>(Difference(request.System.Rhs, evaluation.InternalForce)))
                .Map(norm => norm <= (1.0 - ArmijoSlope * alpha) * baseline ? Step.Settle(alpha, norm) : Step.Advance(alpha * 0.5));
        })
        .Bind(run => run.Verdict is Convergence.Converged
            ? Fin.Succ(run.State)
            : Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.Scalar(baseline)))));

    static readonly Dimension LineSearchBudget = Dimension.Create(8);
    const double ArmijoSlope = 1e-4;

    public static Fin<SolveResult> ArcLength(RouteRequest request, SolveRoute.Continuation row) {
        SparseMatrix tangent = new(request.System.Operator);
        ConstitutiveState[] pristine = request.Problem.Material.Match(
            Some: law => Pristine(request.Mesh, request.Problem, law.Model), None: static () => []);
        Func<double[], Fin<double[]>> solve = rhs => request.Policy.Method.Krylov
            .ToFin(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
            .Bind(krylov => SparseOps.SolveIterative(request.System.Operator, krylov, rhs, request.Policy.Iteration(request.Clock)))
            .Bind(run => run.Terminal is SolveTerminal.Admitted
                ? Fin.Succ(run.Field.ToArray())
                : Fin.Fail<double[]>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Scalar(run.Residual)))));
        return Fixpoint.Run(
            (Field: new double[request.Dofs], Load: 0.0, Direction: new double[request.Dofs],
             Iterations: 0, Committed: pristine, Multipliers: Seq<double[]>()),
            row.Path.Steps,
            (outer, arcStep) => solve(request.System.Rhs).Bind(loadDirection =>
                Corrected(request, row, tangent, solve, outer, loadDirection, arcStep)))
            .Bind(run => run.Verdict is Convergence.Converged
                ? Fin.Succ(request.Settled(run.State.Field.AsMemory(), run.State.Iterations, run.State.Iterations, run.Verdict))
                : Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.Count(run.Steps, row.Path.Steps.Value)))));
    }

    static Fin<Step<(double[] Field, double Load, double[] Direction, int Iterations, ConstitutiveState[] Committed, Seq<double[]> Multipliers)>> Corrected(
        RouteRequest request, SolveRoute.Continuation row, SparseMatrix tangent, Func<double[], Fin<double[]>> solve,
        (double[] Field, double Load, double[] Direction, int Iterations, ConstitutiveState[] Committed, Seq<double[]> Multipliers) outer,
        double[] loadDirection, int arcStep) {
        ArcLengthPolicy path = row.Path;
        double orientation = TensorPrimitives.Dot(loadDirection, outer.Direction) < 0.0 ? -1.0 : 1.0;
        double increment = orientation * path.Radius.Value
            / Math.Sqrt(TensorPrimitives.SumOfSquares<double>(loadDirection) + path.LoadScale.Value * path.LoadScale.Value);
        double[] origin = (double[])outer.Field.Clone();
        double originLoad = outer.Load;
        double[] predicted = new double[outer.Field.Length];
        TensorPrimitives.MultiplyAdd(loadDirection, increment, outer.Field, predicted);
        return Fixpoint.Run(
            (Field: predicted, Load: outer.Load + increment, Trial: outer.Committed, TrialMultipliers: outer.Multipliers),
            row.NewtonIterations,
            (point, _) => InternalForce(request, tangent, point.Field, outer.Committed, outer.Multipliers).Bind(evaluation => {
                double[] forcing = new double[request.Dofs];
                TensorPrimitives.Multiply(request.System.Rhs, point.Load, forcing);
                double[] residual = Difference(forcing, evaluation.InternalForce);
                double norm = TensorPrimitives.Norm<double>(residual);
                double scale = Math.Max(1.0, TensorPrimitives.Norm<double>(forcing));
                return norm <= path.ResidualTolerance.Value * scale
                    ? Fin.Succ(Step.Settle((point.Field, point.Load, evaluation.Trial, evaluation.ContactMultipliers), norm / scale))
                    : from correction in solve(residual)
                      from response in solve(request.System.Rhs)
                      from corrected in ArcCorrect(point.Field, point.Load, origin, originLoad, correction, response, path)
                      select Step.Advance((corrected.Field, corrected.Load, outer.Committed, outer.Multipliers));
            }))
            .Bind(inner => inner.Verdict is Convergence.Converged
                ? SolveCheckpoint.Commit(request.Archive, request.Problem, arcStep, inner.State.Field, inner.State.Load, inner.State.Trial, inner.State.TrialMultipliers)
                    .Map(_ => Step.Advance((inner.State.Field, inner.State.Load, ArcDirection(inner.State.Field, origin),
                        outer.Iterations + inner.Steps, inner.State.Trial, inner.State.TrialMultipliers)))
                : Fin.Fail<Step<(double[], double, double[], int, ConstitutiveState[], Seq<double[]>)>>(
                    new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.Count(inner.Steps, row.NewtonIterations.Value)))));
    }

    static Fin<(double[] Field, double Load)> ArcCorrect(
        double[] field, double load, double[] origin, double originLoad, double[] correction, double[] response, ArcLengthPolicy path) {
        double[] displacement = new double[field.Length];
        TensorPrimitives.Subtract(field, origin, displacement);
        double loadDelta = load - originLoad, scale = path.LoadScale.Value * path.LoadScale.Value;
        double constraint = TensorPrimitives.SumOfSquares<double>(displacement) + scale * loadDelta * loadDelta - path.Radius.Value * path.Radius.Value;
        double denominator = TensorPrimitives.Dot(displacement, response) + scale * loadDelta;
        if (Math.Abs(denominator) <= EpsilonPolicy.ZeroTolerance) {
            return Fin.Fail<(double[], double)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Feasible, new ContractEvidence.Scalar(denominator))));
        }
        double loadCorrection = (-0.5 * constraint - TensorPrimitives.Dot(displacement, correction)) / denominator;
        double[] increment = new double[field.Length], next = new double[field.Length];
        TensorPrimitives.MultiplyAdd(response, loadCorrection, correction, increment);
        TensorPrimitives.Add(field, increment, next);
        return Fin.Succ((next, load + loadCorrection));
    }

    static double[] ArcDirection(double[] field, double[] origin) {
        double[] direction = new double[field.Length];
        TensorPrimitives.Subtract(field, origin, direction);
        return direction;
    }

    static Fin<ResidualEvaluation> InternalForce(
        RouteRequest request, SparseMatrix elastic, double[] field, ConstitutiveState[] committed, Seq<double[]> multipliers) =>
        request.Problem.Material.Match(
            Some: law => Constitutive(request, law.Model, law.Law, field, committed, elastic),
            None: () => Fin.Succ(new ResidualEvaluation(
                elastic.Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray(),
                committed, Seq<double[]>(), (SparseCompressedRowMatrixStorage<double>)elastic.Storage)))
        .Bind(evaluation => ContactAugment(request, field, evaluation, multipliers));

    static Fin<ResidualEvaluation> ContactAugment(
        RouteRequest request, double[] field, ResidualEvaluation evaluation, Seq<double[]> multipliers) {
        Seq<BoundaryCondition.Contact> contacts = request.Problem.Conditions
            .Choose(static bc => bc is BoundaryCondition.Contact contact ? Some(contact) : None);
        if (contacts.IsEmpty) { return Fin.Succ(evaluation); }
        double[] force = (double[])evaluation.InternalForce.Clone();
        HashMap<(int Row, int Column), double> block = HashMap<(int, int), double>();
        return contacts.Map(static (contact, index) => (Index: index, Contact: contact))
            .Traverse(row => {
                Seq<(int Slave, int Master)> pairs = toSeq(row.Contact.Slave.Zip(row.Contact.Master, static (s, m) => ((int)s, (int)m)));
                double[] lambda = row.Index < multipliers.Count ? multipliers[row.Index] : new double[pairs.Count];
                return ContactEnforcement.Enforce(row.Contact.Constraint, field.AsMemory(), lambda.AsMemory(), row.Contact.Penalty, pairs, request.At)
                    .Map(result => {
                        Vector3 normal = row.Contact.Constraint.Normal;
                        for (int i = 0; i < pairs.Count; i++) {
                            double weight = row.Contact.Constraint.Weight(i);
                            double f = result.Force.Span[i] * weight;
                            double h = result.Stiffness.Span[i * pairs.Count + i] * weight * weight;
                            (int slave, int master) = pairs[i];
                            Span<double> axis = [normal.X, normal.Y, normal.Z];
                            for (int k = 0; k < 3; k++) {
                                force[slave + k] += f * axis[k];
                                force[master + k] -= f * axis[k];
                                for (int l = 0; l < 3; l++) {
                                    double coupled = h * axis[k] * axis[l];
                                    block = Tally(block, slave + k, slave + l, coupled);
                                    block = Tally(block, master + k, master + l, coupled);
                                    block = Tally(block, slave + k, master + l, -coupled);
                                    block = Tally(block, master + k, slave + l, -coupled);
                                }
                            }
                        }
                        return result.Multipliers.ToArray();
                    });
            }).As()
            .Bind(advanced => Augmented(evaluation.Tangent, block)
                .Map(tangent => evaluation with { InternalForce = force, ContactMultipliers = advanced, Tangent = tangent }));
    }

    static HashMap<(int Row, int Column), double> Tally(HashMap<(int Row, int Column), double> block, int row, int column, double value) =>
        block.AddOrUpdate((row, column), held => held + value, value);

    static Fin<SparseCompressedRowMatrixStorage<double>> Augmented(
        SparseCompressedRowMatrixStorage<double> tangent, HashMap<(int Row, int Column), double> block) {
        HashMap<(int Row, int Column), double> merged = block;
        for (int row = 0; row < tangent.RowCount; row++) {
            for (int slot = tangent.RowPointers[row]; slot < tangent.RowPointers[row + 1]; slot++) {
                merged = Tally(merged, row, tangent.ColumnIndices[slot], tangent.Values[slot]);
            }
        }
        Seq<((int Row, int Column) Key, double Value)> entries = toSeq(merged.AsIterable().Map(static pair => (pair.Key, pair.Value)));
        return SparseOps.Ingest(SparseFormat.Coo, tangent.RowCount, tangent.ColumnCount,
            [.. entries.Map(static entry => entry.Key.Row)],
            [.. entries.Map(static entry => entry.Key.Column)],
            [.. entries.Map(static entry => entry.Value)]);
    }

    static Fin<ResidualEvaluation> Constitutive(
        RouteRequest request, ConstitutiveModel model, ConstitutiveParameters law, double[] field,
        ConstitutiveState[] committed, SparseMatrix elastic) {
        DiscreteMesh mesh = request.Mesh;
        SolveProblem problem = request.Problem;
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof;
        int gaussCount = mesh.Rule.Points.Length;
        bool finiteStrain = model is ConstitutiveModel.Hyperelastic;
        int components = finiteStrain ? 9 : problem.Physics.StrainDim;
        double[] global = new double[field.Length];
        ConstitutiveState[] trial = (ConstitutiveState[])committed.Clone();
        SparseCompressedRowMatrixStorage<double> tangent = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            elastic.RowCount, elastic.ColumnCount, ((SparseCompressedRowMatrixStorage<double>)elastic.Storage).Values.Length,
            ((SparseCompressedRowMatrixStorage<double>)elastic.Storage).RowPointers,
            ((SparseCompressedRowMatrixStorage<double>)elastic.Storage).ColumnIndices,
            new double[((SparseCompressedRowMatrixStorage<double>)elastic.Storage).Values.Length]);
        ReadOnlySpan<long> conn = mesh.Indices;
        using SpanOwner<double> rows = SpanOwner<double>.Allocate(components * block);
        using SpanOwner<double> strain = SpanOwner<double>.Allocate(components);
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            int point = 0;
            foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Rule.Points) {
                ShapeSample sample = mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
                double weight = gauss.Weight * Math.Abs(sample.DetJ);
                rows.Span.Clear();
                if (finiteStrain) { DeformationRows(sample, per, dof, block, rows.Span); }
                else { OperatorAssembly.Strain(problem.Physics.Form, sample.Grad, per, dof, block, rows.Span); }
                for (int r = 0; r < components; r++) {
                    double e = 0.0;
                    for (int j = 0; j < block; j++) { e += rows.Span[r * block + j] * field[(int)conn[cell * per + j / dof] * dof + j % dof]; }
                    strain.Span[r] = e + (finiteStrain && r is 0 or 4 or 8 ? 1.0 : 0.0);
                }
                Fin<Unit> accumulated = StressUpdate.Stress(model, strain.Span.ToArray().AsMemory(), trial[cell * gaussCount + point], law, request.At)
                    .Map(result => {
                        trial[cell * gaussCount + point] = result.State;
                        Scatter(result, rows.Span, conn, tangent, global, cell, per, dof, block, components, weight);
                        return unit;
                    });
                if (accumulated.IsFail) { return accumulated.Map(static _ => default(ResidualEvaluation)!); }
                point++;
            }
        }
        return Fin.Succ(new ResidualEvaluation(global, trial, Seq<double[]>(), tangent));
    }

    static void DeformationRows(ShapeSample sample, int per, int dof, int block, Span<double> rows) {
        for (int node = 0; node < per; node++)
            for (int displacement = 0; displacement < 3; displacement++)
                for (int derivative = 0; derivative < 3; derivative++) {
                    rows[(displacement * 3 + derivative) * block + node * dof + displacement] = sample.Grad[node * 3 + derivative];
                }
    }

    static void Scatter(
        ConstitutiveResult result, ReadOnlySpan<double> b, ReadOnlySpan<long> conn,
        SparseCompressedRowMatrixStorage<double> tangent, double[] global,
        int cell, int per, int dof, int block, int components, double weight) {
        ReadOnlySpan<double> stress = result.Stress.Span, d = result.Tangent.Span;
        for (int i = 0; i < block; i++) {
            double f = 0.0;
            for (int r = 0; r < components; r++) { f += b[r * block + i] * (r < stress.Length ? stress[r] : 0.0); }
            int rowDof = (int)conn[cell * per + i / dof] * dof + i % dof;
            global[rowDof] += weight * f;
            for (int j = 0; j < block; j++) {
                double k = 0.0;
                for (int r = 0; r < components; r++)
                    for (int s = 0; s < components; s++) { k += b[r * block + i] * d[r * components + s] * b[s * block + j]; }
                BoundaryCondition.AddAt(tangent, tangent.Values, rowDof, (int)conn[cell * per + j / dof] * dof + j % dof, weight * k);
            }
        }
    }

    public static Fin<SolveResult> Vibration(RouteRequest request, SolveRoute.Vibration row) =>
        request.System.Operator.RowCount > request.Policy.MaxDenseDofs.Value
            ? Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.WithinLimit, new CapacityEvidence.Count(request.System.Operator.RowCount, request.Policy.MaxDenseDofs.Value))))
            : OperatorAssembly.Lumped(request.Mesh, request.Problem).Bind(mass =>
                OperatorAssembly.MassSingular(mass, request.System.Constrained, OperatorAssembly.InertiaFloor(mass)).Match(
                    Some: dof => Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Index(dof, mass.Length)))),
                    None: () => DenseOps.Decompose(MassNormalized(Matrix<double>.Build.OfStorage(request.System.Operator), mass), FactorizationKind.Evd)
                        .Bind(factorization => EigenPairs(factorization, row.Pairs.Value, mass))
                        .Bind(pairs => Sealed(request, request.Settled(pairs.Vectors, 1, 1, new Convergence.Converged(0.0)) with {
                            EigenValues = Some(pairs.Values),
                            Participation = Some(Participated(pairs.Vectors, mass, request.Problem.Dof, pairs.Count)),
                            TotalMass = Some(ExcitableMass(mass, request.Problem.Dof)),
                        }))));

    public static Fin<SolveResult> Condensed(RouteRequest request, SolveRoute.Condensed row) =>
        OperatorAssembly.Lumped(request.Mesh, request.Problem).Bind(mass =>
            Partition(mass, request.System.Constrained, OperatorAssembly.InertiaFloor(mass)).Bind(split =>
                Seq(
                    Claim(split.Masters.Length >= row.Pairs.Value, new ComputeViolation.Capacity(
                        CapacityRequirement.Sufficient,
                        new CapacityEvidence.Count(split.Masters.Length, row.Pairs.Value))),
                    Claim(split.Masters.Length <= row.Reduction.MaxRetained.Value, new ComputeViolation.Capacity(
                        CapacityRequirement.WithinLimit,
                        new CapacityEvidence.Count(split.Masters.Length, row.Reduction.MaxRetained.Value))),
                    Claim(split.TransformBytes <= row.Reduction.MaxTransformBytes, new ComputeViolation.Capacity(
                        CapacityRequirement.WithinLimit,
                        new CapacityEvidence.Count(split.TransformBytes, row.Reduction.MaxTransformBytes))))
                    .Traverse(static claim => claim).As().ToFin()
                    .Bind(_ => Reduce(request.System.Operator, mass, split, row.Reduction.ResidualCap.Value))
                    .Bind(reduced => Spectrum(reduced, row.Pairs.Value)
                        .Bind(spectrum => Sealed(request, Reported(request, split, mass, reduced, spectrum))))));

    static SolveResult Reported(
        RouteRequest request, DofSplit split, double[] mass, Reduced reduced,
        (Matrix<double> Modes, ReadOnlyMemory<double> Values, double Defect) spectrum) {
        ReadOnlyMemory<double> modes = Recovered(reduced, split, mass.Length, spectrum.Modes, spectrum.Values.Length);
        return request.Settled(modes, 1, 1, new Convergence.Converged(spectrum.Defect)) with {
            EigenValues = Some(spectrum.Values),
            Participation = Some(Participated(modes, mass, request.Problem.Dof, spectrum.Values.Length)),
            TotalMass = Some(ExcitableMass(mass, request.Problem.Dof)),
            Condensation = Some(new CondensationEvidence(split.Masters.Length, split.Slaves.Length, reduced.Residual, reduced.Conditioning)),
        };
    }

    static Fin<SolveResult> Sealed(RouteRequest request, SolveResult result) =>
        SolveModes.Seal(request.Archive, request.Problem, result).Map(_ => result);

    static Fin<DofSplit> Partition(double[] mass, LanguageExt.HashSet<long> constrained, double floor) {
        int[] masterOf = new int[mass.Length], slaveOf = new int[mass.Length];
        List<int> masters = new(mass.Length), slaves = new(mass.Length);
        for (int dof = 0; dof < mass.Length; dof++) {
            masterOf[dof] = -1;
            slaveOf[dof] = -1;
            if (constrained.Contains(dof)) { continue; }
            if (mass[dof] > floor) { masterOf[dof] = masters.Count; masters.Add(dof); }
            else { slaveOf[dof] = slaves.Count; slaves.Add(dof); }
        }
        return Seq(
            Claim(masters.Count > 0, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(masters.Count, 1L))),
            Claim(slaves.Count > 0, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(slaves.Count, 1L))))
            .Traverse(static claim => claim).As().ToFin()
            .Map(_ => new DofSplit([.. masters], [.. slaves], masterOf, slaveOf));
    }

    static Fin<(SparseCompressedRowMatrixStorage<double> Condensed, CompressedColumnStorage<double> Coupling, Matrix<double> Retained)> Blocks(
        SparseCompressedRowMatrixStorage<double> csr, DofSplit split) {
        int slaves = split.Slaves.Length, masters = split.Masters.Length;
        List<int> ssRows = new(csr.ValueCount), ssCols = new(csr.ValueCount), smRows = new(csr.ValueCount), smCols = new(csr.ValueCount);
        List<double> ssVals = new(csr.ValueCount), smVals = new(csr.ValueCount);
        Matrix<double> retained = Matrix<double>.Build.Dense(masters, masters);
        for (int row = 0; row < csr.RowCount; row++) {
            for (int slot = csr.RowPointers[row]; slot < csr.RowPointers[row + 1]; slot++) {
                int column = csr.ColumnIndices[slot];
                double value = csr.Values[slot];
                if (split.SlaveOf[row] >= 0 && split.SlaveOf[column] >= 0) { ssRows.Add(split.SlaveOf[row]); ssCols.Add(split.SlaveOf[column]); ssVals.Add(value); }
                else if (split.SlaveOf[row] >= 0 && split.MasterOf[column] >= 0) { smRows.Add(split.SlaveOf[row]); smCols.Add(split.MasterOf[column]); smVals.Add(value); }
                else if (split.MasterOf[row] >= 0 && split.MasterOf[column] >= 0) { retained[split.MasterOf[row], split.MasterOf[column]] += value; }
            }
        }
        return SparseOps.Ingest(SparseFormat.Coo, slaves, slaves, [.. ssRows], [.. ssCols], [.. ssVals])
            .Bind(condensed => SparseOps.Ingest(SparseFormat.Coo, slaves, masters, [.. smRows], [.. smCols], [.. smVals])
                .Map(coupling => (condensed, (CompressedColumnStorage<double>)SparseOps.ToCsc(coupling), retained)));
    }

    static Fin<Reduced> Reduce(SparseCompressedRowMatrixStorage<double> csr, double[] mass, DofSplit split, double cap) =>
        Blocks(csr, split).Bind(blocks =>
            SparseOps.Factor(blocks.Condensed, FactorKind.Spd, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
                .Bind(condensedOp => Transform(condensedOp, blocks.Coupling, split.Slaves.Length, split.Masters.Length, cap)
                    .Bind(transform => Pencil(blocks.Coupling, blocks.Retained, transform.Columns, mass, split, transform.Residual))));

    static Fin<(double[][] Columns, double Residual)> Transform(
        FactoredOp condensedOp, CompressedColumnStorage<double> coupling, int slaves, int masters, double cap) {
        double[][] columns = new double[masters][];
        double[] rhs = new double[slaves], defect = new double[slaves];
        double defectMass = 0.0, couplingMass = 0.0;
        for (int column = 0; column < masters; column++) {
            coupling.Column(column, rhs);
            couplingMass += TensorPrimitives.SumOfSquares<double>(rhs);
            rhs.AsSpan().CopyTo(defect);
            TensorPrimitives.Multiply<double>(rhs, -1.0, rhs);
            Fin<double[]> solved = condensedOp.Solve(rhs, cap);
            if (solved.Case is not double[] response) { return solved.Map(static _ => default((double[][], double))); }
            columns[column] = response;
            Fin<Unit> swept = SparseTensorOps.Spmv(condensedOp.A, GemvForm.Accumulate(1.0, 1.0), response, defect);
            if (swept.IsFail) { return swept.Map(static _ => default((double[][], double))); }
            defectMass += TensorPrimitives.SumOfSquares<double>(defect);
        }
        return Math.Sqrt(defectMass) / Math.Max(1.0, Math.Sqrt(couplingMass)) is var residual && residual <= cap
            ? Fin.Succ((columns, residual))
            : Fin.Fail<(double[][], double)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.WithinLimit, new CapacityEvidence.Scalar(residual, cap))));
    }

    static Fin<Reduced> Pencil(
        CompressedColumnStorage<double> coupling, Matrix<double> retained, double[][] transform, double[] mass, DofSplit split, double residual) {
        int masters = split.Masters.Length, slaves = split.Slaves.Length;
        double[] condensedMass = new double[slaves];
        for (int row = 0; row < slaves; row++) { condensedMass[row] = mass[split.Slaves[row]]; }
        CompressedColumnStorage<double> inertia = SparseOps.Diagonal(condensedMass);
        Matrix<double> reducedMass = Matrix<double>.Build.Dense(masters, masters);
        double[] adjoint = new double[masters], weighted = new double[slaves];
        for (int column = 0; column < masters; column++) {
            Fin<Unit> stiffness = SparseTensorOps.Spmv(coupling, GemvForm.Transposed, transform[column], adjoint);
            if (stiffness.IsFail) { return stiffness.Map(static _ => default(Reduced)); }
            for (int row = 0; row < masters; row++) { retained[row, column] += adjoint[row]; }
            Fin<Unit> weighting = SparseTensorOps.Spmv(inertia, GemvForm.Apply, transform[column], weighted);
            if (weighting.IsFail) { return weighting.Map(static _ => default(Reduced)); }
            reducedMass[column, column] = mass[split.Masters[column]] + TensorPrimitives.Dot<double>(transform[column], weighted);
            for (int row = 0; row < column; row++) {
                double coupled = TensorPrimitives.Dot<double>(transform[row], weighted);
                reducedMass[row, column] = coupled;
                reducedMass[column, row] = coupled;
            }
        }
        Matrix<double> pencil = Admission.Symmetrize(retained);
        return Conditioned(pencil).Map(conditioning => new Reduced(pencil, Admission.Symmetrize(reducedMass), transform, residual, conditioning));
    }

    static Fin<double> Conditioned(Matrix<double> pencil) =>
        pencil.Svd(computeVectors: false).ConditionNumber is var conditioning && double.IsFinite(conditioning)
            ? Fin.Succ(conditioning)
            : Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(conditioning))));

    static Fin<(Matrix<double> Modes, ReadOnlyMemory<double> Values, double Defect)> Spectrum(Reduced reduced, int pairs) =>
        Admission.Definite(reduced.Mass).Bind(chol => {
            LU<double> lower = chol.Factor.LU(), upper = chol.Factor.Transpose().LU();
            Matrix<double> congruent = Admission.Symmetrize(lower.Solve(lower.Solve(reduced.Stiffness).Transpose()));
            return DenseOps.Decompose(congruent, FactorizationKind.Evd).Bind(factorization =>
                factorization is Factorization.Evd { Decomposition: Evd<double> evd }
                && SpectralOps.Decompose(congruent, evd, Symmetricity.Symmetric) is SpectralResult.Symmetric spectrum
                    ? Fin.Succ((upper.Solve(spectrum.Vectors), spectrum.Values.Take(Math.Min(pairs, spectrum.Values.Count)).ToArray().AsMemory(), spectrum.Defect))
                    : Fin.Fail<(Matrix<double>, ReadOnlyMemory<double>, double)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))));
        });

    static ReadOnlyMemory<double> Recovered(Reduced reduced, DofSplit split, int dofs, Matrix<double> modes, int pairs) {
        double[] flat = new double[dofs * pairs];
        for (int mode = 0; mode < pairs; mode++) {
            for (int master = 0; master < split.Masters.Length; master++) { flat[mode * dofs + split.Masters[master]] = modes[master, mode]; }
            for (int slave = 0; slave < split.Slaves.Length; slave++) {
                double response = 0.0;
                for (int master = 0; master < split.Masters.Length; master++) { response += reduced.Transform[master][slave] * modes[master, mode]; }
                flat[mode * dofs + split.Slaves[slave]] = response;
            }
        }
        return flat.AsMemory();
    }

    static ReadOnlyMemory<ModalParticipation> Participated(ReadOnlyMemory<double> modes, double[] mass, int dof, int pairs) {
        ModalParticipation[] factors = new ModalParticipation[pairs];
        int n = mass.Length;
        for (int mode = 0; mode < pairs; mode++) {
            ReadOnlySpan<double> phi = modes.Span.Slice(mode * n, n);
            double x = 0.0, y = 0.0, z = 0.0;
            for (int node = 0; node < n; node += dof) {
                x += mass[node] * phi[node];
                y += mass[node + 1] * phi[node + 1];
                z += mass[node + 2] * phi[node + 2];
            }
            factors[mode] = new ModalParticipation(x, y, z);
        }
        return factors.AsMemory();
    }

    static ModalParticipation ExcitableMass(double[] mass, int dof) {
        double x = 0.0, y = 0.0, z = 0.0;
        for (int node = 0; node < mass.Length; node += dof) { x += mass[node]; y += mass[node + 1]; z += mass[node + 2]; }
        return new ModalParticipation(x, y, z);
    }

    public static Fin<SolveResult> Buckle(RouteRequest request, SolveRoute.Buckling row) =>
        request.System.Operator.RowCount > request.Policy.MaxDenseDofs.Value
            ? Fin.Fail<SolveResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
            : request.Solve(request.System.Operator, request.System.Rhs, FactorKind.Lu).Bind(prestress =>
                Op.Of(name: "solve.buckle-reduction").Catch(() => {
                    int n = request.System.Operator.RowCount;
                    Span2D<double> kg = GeometricStiffness(request, prestress, n);
                    foreach (long dof in request.System.Constrained) {
                        for (int k = 0; k < n; k++) { kg[(int)dof, k] = 0.0; kg[k, (int)dof] = 0.0; }
                    }
                    Matrix<double> linv = Matrix<double>.Build.OfStorage(request.System.Operator).Cholesky().Factor.Inverse();
                    Matrix<double> reduced = linv.Multiply(Matrix<double>.Build.Dense(n, n, (r, c) => -kg[r, c])).Multiply(linv.Transpose());
                    return Fin.Succ((Linv: linv, Reduced: reduced));
                })
                .Bind(reduction => DenseOps.Decompose(reduction.Reduced, FactorizationKind.Evd)
                    .Bind(factorization => BucklingPairs(factorization, row.Pairs.Value, reduction.Linv.Transpose()))
                    .Map(pairs => request.Settled(pairs.Vectors, 1, 1, new Convergence.Converged(0.0)) with {
                        EigenValues = Some(pairs.Values),
                    })));

    static Span2D<double> GeometricStiffness(RouteRequest request, double[] prestress, int n) {
        DiscreteMesh mesh = request.Mesh;
        SolveProblem problem = request.Problem;
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof, strain = problem.Physics.StrainDim;
        Span2D<double> kg = new(new double[checked(n * n)], n, n);
        Fin<Func<int, double[]>> coefficient = problem.Field.Lower(problem.Physics, problem.Payload);
        if (coefficient.Case is not Func<int, double[]> materialOf) { return kg; }
        ReadOnlySpan<long> conn = mesh.Indices;
        Span<double> rows = new double[strain * block];
        Span<double> eps = stackalloc double[strain], sigma = stackalloc double[strain];
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            double[] d = materialOf(cell);
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Rule.Points) {
                ShapeSample sample = mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
                double weight = gauss.Weight * Math.Abs(sample.DetJ);
                rows.Clear();
                OperatorAssembly.Strain(problem.Physics.Form, sample.Grad, per, dof, block, rows);
                for (int r = 0; r < strain; r++) {
                    double e = 0.0;
                    for (int j = 0; j < block; j++) { e += rows[r * block + j] * prestress[(int)conn[cell * per + j / dof] * dof + j % dof]; }
                    eps[r] = e;
                }
                for (int r = 0; r < strain; r++) {
                    double v = 0.0;
                    for (int q = 0; q < strain; q++) { v += d[r * strain + q] * eps[q]; }
                    sigma[r] = v;
                }
                Scatter(kg, sample, sigma, conn, cell, per, dof, strain, weight);
            }
        }
        return kg;
    }

    static void Scatter(
        Span2D<double> kg, ShapeSample sample, ReadOnlySpan<double> s, ReadOnlySpan<long> conn,
        int cell, int per, int dof, int strain, double weight) {
        double sxy = strain > 3 ? s[3] : 0.0, syz = strain > 4 ? s[4] : 0.0, szx = strain > 5 ? s[5] : 0.0;
        ReadOnlySpan<double> sigma = [s[0], sxy, szx, sxy, s[1], syz, szx, syz, s[2]];
        ReadOnlySpan<double> grad = sample.Grad;
        for (int a = 0; a < per; a++)
            for (int b = 0; b < per; b++) {
                double g = 0.0;
                for (int p = 0; p < 3; p++) { for (int q = 0; q < 3; q++) { g += grad[a * 3 + p] * sigma[p * 3 + q] * grad[b * 3 + q]; } }
                int ga = (int)conn[cell * per + a] * dof, gb = (int)conn[cell * per + b] * dof;
                for (int i = 0; i < dof; i++) { kg[ga + i, gb + i] += weight * g; }
            }
    }

    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values)> BucklingPairs(Factorization factorization, int pairs, Matrix<double> linvT) {
        if (factorization is not Factorization.Evd { Decomposition: Evd<double> evd }) {
            return Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.EigenSystem)));
        }
        (int Index, double Factor)[] ordered = [.. Enumerable.Range(0, evd.EigenValues.Count)
            .Select(k => (Index: k, Factor: Math.Abs(evd.EigenValues[k].Real) > EpsilonPolicy.ZeroTolerance ? 1.0 / evd.EigenValues[k].Real : double.PositiveInfinity))
            .Where(static p => double.IsFinite(p.Factor))
            .OrderBy(static p => Math.Abs(p.Factor))
            .Take(pairs)];
        int n = evd.EigenVectors.RowCount;
        double[] flat = new double[n * ordered.Length];
        for (int m = 0; m < ordered.Length; m++) {
            Vector<double> phi = linvT.Multiply(evd.EigenVectors.Column(ordered[m].Index));
            for (int i = 0; i < n; i++) { flat[m * n + i] = phi[i]; }
        }
        return Fin.Succ((flat.AsMemory(), ordered.Select(static p => p.Factor).ToArray().AsMemory()));
    }

    static Matrix<double> MassNormalized(Matrix<double> stiffness, double[] mass) {
        int n = stiffness.RowCount;
        double[] inv = new double[n];
        for (int i = 0; i < n; i++) { inv[i] = 1.0 / Math.Sqrt(mass[i]); }
        return Matrix<double>.Build.Dense(n, n, (r, c) => stiffness[r, c] * inv[r] * inv[c]);
    }

    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values, int Count)> EigenPairs(Factorization factorization, int pairs, double[] mass) =>
        factorization is Factorization.Evd { Decomposition: Evd<double> evd }
            ? Fin.Succ((PhysicalModes(evd, Math.Min(pairs, evd.EigenVectors.ColumnCount), mass),
                evd.EigenValues.Take(Math.Min(pairs, evd.EigenVectors.ColumnCount)).Select(static c => c.Real).ToArray().AsMemory(),
                Math.Min(pairs, evd.EigenVectors.ColumnCount)))
            : Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>, int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.EigenSystem)));

    static ReadOnlyMemory<double> PhysicalModes(Evd<double> evd, int modes, double[] mass) {
        int n = evd.EigenVectors.RowCount;
        double[] flat = new double[n * modes];
        for (int mode = 0; mode < modes; mode++) {
            Vector<double> phi = evd.EigenVectors.Column(mode);
            for (int i = 0; i < n; i++) { flat[mode * n + i] = phi[i] / Math.Sqrt(mass[i]); }
        }
        return flat.AsMemory();
    }

    static Validation<Error, Unit> Claim(bool held, ComputeViolation evidence) =>
        held ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new ComputeFault.Violation(ComputeArea.Solver, evidence));
}
```

## [03]-[SOLVE_RECOVERY]

- Owner: `RecoveryAction` `[SmartEnum<string>]` carries each rung's own repair as a delegate column, so the ladder is data and the fold has no per-action arm; `RecoveryPolicy` the ladder and its growth factors; `RecoveryReceipt` the ordered trail.
- Cases: `RecoveryAction` refine-mesh · relax · reorder-dofs · switch-method · restart.
- Entry: `public static (Fin<SolveResult> Result, RecoveryReceipt Trace) SolveAdaptive(…, RecoveryPolicy recovery, …)` — walks the ladder on a refusal and stops at the first attempt that settles.
- Auto: each rung's attempt calls the archive sink factory afresh, so every rung that runs an archiving route emits its OWN create-only containers and never appends into a prior rung's artifact; each attempt also re-anchors its wall budget, so a relax rung retries against a fresh window rather than an expired one.
- Boundary: a failed rung's residual is ABSENT, not a sentinel. The trail carries `Option<double>`, so a reader distinguishes "this rung ran and left this residual" from "this rung could not run at all" — a `double.PositiveInfinity` in that slot reads as a measured divergence.
- Boundary: the `RebuildsOperator` column had no reader and the recovery fold re-derived the same fact per arm; the rung's own repair delegate now carries it, so the fact exists once and nothing re-derives it.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RecoveryAction {
    public static readonly RecoveryAction RefineMesh = new("refine-mesh", SolveRecovery.RefineMesh);
    public static readonly RecoveryAction Relax = new("relax", SolveRecovery.Relax);
    public static readonly RecoveryAction ReorderDofs = new("reorder-dofs", SolveRecovery.Reorder);
    public static readonly RecoveryAction SwitchMethod = new("switch-method", SolveRecovery.SwitchMethod);
    public static readonly RecoveryAction Restart = new("restart", SolveRecovery.Restart);

    [UseDelegateFromConstructor]
    public partial Fin<RecoveryAttempt> Repair(RecoveryAttempt attempt, RecoveryPolicy recovery, IClock clock);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record RecoveryAttempt(SolveProblem Problem, DiscreteMesh Mesh, LanePolicy Policy, SolveRoute Route);

public sealed record RecoveryPolicy(
    Seq<RecoveryAction> Ladder, MeshPolicy MeshPolicy, PositiveMagnitude RelaxFactor, PositiveMagnitude IterationGrowth, SolveMethod Fallback) {
    public static readonly RecoveryPolicy Canonical = new(
        Ladder: Seq(RecoveryAction.Relax, RecoveryAction.ReorderDofs, RecoveryAction.RefineMesh, RecoveryAction.SwitchMethod, RecoveryAction.Restart),
        MeshPolicy: MeshPolicy.CanonicalTet, RelaxFactor: PositiveMagnitude.Create(10.0),
        IterationGrowth: PositiveMagnitude.Create(2.0), Fallback: SolveMethod.MlkBiCgStab);
}

public sealed record RecoveryReceipt(string Physics, Seq<(string Action, Option<double> Residual)> Steps, bool Recovered, Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class SolveRecovery {
    public static (Fin<SolveResult> Result, RecoveryReceipt Trace) SolveAdaptive(
        SolveProblem problem, DiscreteMesh mesh, LanePolicy policy, SolveRoute route, RecoveryPolicy recovery,
        IClock clock, Option<SolveArchive> archive = default, Option<SolveSession> session = default) {
        (Fin<SolveResult> Result, RecoveryAttempt Attempt, Seq<(string Action, Option<double> Residual)> Steps) final =
            recovery.Ladder.Fold(
                (Result: SolveLane.Solve(problem, mesh, policy, route, clock, archive, session),
                 Attempt: new RecoveryAttempt(problem, mesh, policy, route),
                 Steps: Seq<(string, Option<double>)>()),
                (state, action) => state.Result.IsSucc
                    ? state
                    : action.Repair(state.Attempt, recovery, clock).Match(
                        Succ: next => {
                            Fin<SolveResult> attempt = SolveLane.Solve(next.Problem, next.Mesh, next.Policy, next.Route, clock, archive, session);
                            return (attempt, next, state.Steps.Add((action.Key, attempt.Map(static r => r.Residual).ToOption())));
                        },
                        Fail: fault => (Fin.Fail<SolveResult>(fault), state.Attempt, state.Steps.Add((action.Key, None)))));
        return (final.Result, new RecoveryReceipt(problem.Physics.Key, final.Steps, final.Result.IsSucc, clock.GetCurrentInstant()));
    }

    internal static Fin<RecoveryAttempt> RefineMesh(RecoveryAttempt attempt, RecoveryPolicy recovery, IClock clock) =>
        MeshLane.Refine(attempt.Mesh, recovery.MeshPolicy, RefinementError(attempt.Mesh), clock)
            .Map(refined => attempt with { Mesh = refined, Problem = attempt.Problem with { Element = refined.Element } });

    internal static Fin<RecoveryAttempt> Relax(RecoveryAttempt attempt, RecoveryPolicy recovery, IClock clock) =>
        Fin.Succ(attempt with {
            Policy = attempt.Policy with {
                Tolerance = PositiveMagnitude.Create(attempt.Policy.Tolerance.Value * recovery.RelaxFactor.Value),
                MaxIterations = Dimension.Create((int)(attempt.Policy.MaxIterations.Value * recovery.IterationGrowth.Value)),
            },
        });

    internal static Fin<RecoveryAttempt> SwitchMethod(RecoveryAttempt attempt, RecoveryPolicy recovery, IClock clock) =>
        Fin.Succ(attempt with { Policy = attempt.Policy with { Method = recovery.Fallback } });

    internal static Fin<RecoveryAttempt> Restart(RecoveryAttempt attempt, RecoveryPolicy recovery, IClock clock) =>
        Fin.Succ(attempt with {
            Policy = attempt.Policy with {
                Method = recovery.Fallback,
                MaxIterations = Dimension.Create(attempt.Policy.MaxIterations.Value * 2),
            },
        });

    internal static Fin<RecoveryAttempt> Reorder(RecoveryAttempt attempt, RecoveryPolicy recovery, IClock clock) {
        int dof = attempt.Problem.Dof, nodes = checked((int)attempt.Mesh.NodeCount);
        return OperatorAssembly.Triplets(attempt.Mesh, attempt.Problem).Map(t => {
            CoordinateStorage<double> coords = new(nodes, nodes, t.Vals.Length);
            for (int entry = 0; entry < t.Vals.Length; entry++) { coords.At(t.Rows[entry] / dof, t.Cols[entry] / dof, t.Vals[entry]); }
            CompressedColumnStorage<double> csc = CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
            return attempt with { Mesh = Renumbered(attempt.Mesh, AMD.Generate(csc, ColumnOrdering.MinimumDegreeAtPlusA), clock) };
        });
    }

    static DiscreteMesh Renumbered(DiscreteMesh mesh, int[] permutation, IClock clock) {
        int nodes = checked((int)mesh.NodeCount);
        if (permutation.Length < nodes) { return mesh; }
        int[] inverse = new int[nodes];
        for (int slot = 0; slot < nodes; slot++) { inverse[permutation[slot]] = slot; }
        float[] reordered = new float[nodes * 3];
        ReadOnlySpan<float> source = mesh.Coordinates;
        for (int old = 0; old < nodes; old++) {
            int fresh = inverse[old];
            reordered[fresh * 3] = source[old * 3]; reordered[fresh * 3 + 1] = source[old * 3 + 1]; reordered[fresh * 3 + 2] = source[old * 3 + 2];
        }
        long[] connectivity = new long[checked((int)mesh.ElementCount) * mesh.Element.Nodes];
        ReadOnlySpan<long> conn = mesh.Indices;
        for (int entry = 0; entry < conn.Length; entry++) { connectivity[entry] = inverse[(int)conn[entry]]; }
        return mesh with { Nodes = reordered.AsMemory(), Connectivity = connectivity.AsMemory(), At = clock.GetCurrentInstant() };
    }

    static double[] RefinementError(DiscreteMesh mesh) {
        double[] error = new double[checked((int)mesh.ElementCount)];
        for (long cell = 0; cell < error.Length; cell++) {
            error[cell] = 1.0 - Math.Abs(mesh.Element.Metric(CellQuality.ScaledJacobian, mesh.NodalXyz(cell)));
        }
        return error;
    }
}
```

## [04]-[SOLVE_ARCHIVE]

- Owner: `SolveArchiveKind` `[SmartEnum<string>]` closes the container roster; `SolveArchive` the route-borne capability; `SolveHistory`/`SolveModes`/`SolveCheckpoint` the three sessions.
- Boundary: route-borne archive CAPABILITY, never a policy value — a sink is a live resource no value record carries. Its caller supplies one fresh pooled stream per requested container, the producing route lands its corpus through the one `Runtime/archive#HDF_ARCHIVE` owner, and an archive write fault FAULTS the run: the caller asked for the artifact, and a partial container published as evidence is worse than a refused solve.
- Boundary: each container class is its own session because their write laws differ — a HISTORY writes `[steps, dofs]` kinematic datasets one chunk per accepted step (monotone by construction, because a march is strictly step-ordered); MODES writes `[pairs, dofs]` mode-outermost as recovery produces them; a CHECKPOINT is one create-only container per COMMITTED step keyed `(ContentKey, step)`, because create-only forbids a growing checkpoint file.
- Boundary: an absent session is a `[Union]` case, never a null-object holding four nulls. The inert arm answers every call with success and the live arm carries its writer and its dataset roster, so the integrator folds thread ONE call shape and no field on the page is nullable.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolveArchiveKind {
    public static readonly SolveArchiveKind History = new("history");
    public static readonly SolveArchiveKind Modes = new("modes");
    public static readonly SolveArchiveKind Checkpoint = new("checkpoint");
}

public sealed record SolveArchive(Func<SolveArchiveKind, Stream> Sink, HdfArchivePolicy Policy);

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolveHistory : IDisposable {
    private SolveHistory() { }

    public sealed record Inert : SolveHistory;
    public sealed record Live(
        HdfWriter Writer, ChunkCursor<double> Field, Option<(ChunkCursor<double> V, ChunkCursor<double> A)> Kinematics) : SolveHistory;

    public static Fin<SolveHistory> Open(Option<SolveArchive> archive, SolveProblem problem, SolveRoute.Transient grid, int dofs, bool kinematic) =>
        archive.Match(
            None: () => Fin.Succ<SolveHistory>(new Inert()),
            Some: capability => ChunkGrid.Derive([grid.Steps.Value], components: dofs, FieldPack.ChunkElementTarget).ToFin()
                .Bind(chunks => Op.Of(name: "solve.archive-open").Catch(() => {
                    H5DatasetCreation creation = capability.Policy.Creation();
                    H5Dataset<double[]> u = new(chunks.FileDims.ToArray(), chunks.Chunk.ToArray(), datasetCreation: creation);
                    Option<(H5Dataset<double[]> V, H5Dataset<double[]> A)> slots = kinematic
                        ? Some((new H5Dataset<double[]>(chunks.FileDims.ToArray(), chunks.Chunk.ToArray(), datasetCreation: creation),
                                new H5Dataset<double[]>(chunks.FileDims.ToArray(), chunks.Chunk.ToArray(), datasetCreation: creation)))
                        : None;
                    H5File graph = new() { ["u"] = u };
                    slots.Iter(rows => { graph["v"] = rows.V; graph["a"] = rows.A; });
                    graph.Attributes["content-key"] = $"{problem.ContentKey:x32}";
                    graph.Attributes["physics"] = problem.Physics.Key;
                    graph.Attributes["integrator"] = grid.Integrator.Key;
                    graph.Attributes["dt"] = grid.Step.Value;
                    HdfWriter writer = HdfArchive.Begin(graph, capability.Sink(SolveArchiveKind.History), capability.Policy);
                    return Fin.Succ<SolveHistory>(new Live(
                        writer, writer.Open(u, chunks),
                        slots.Map(rows => (writer.Open(rows.V, chunks), writer.Open(rows.A, chunks)))));
                }));

    public Fin<Unit> Step(int step, double[] field, double[]? velocity = null, double[]? acceleration = null) =>
        Switch(
            state: (Field: field, Velocity: velocity, Acceleration: acceleration),
            inert: static (_, _) => Fin.Succ(unit),
            live: static (s, live) => live.Field.Write(s.Field).Bind(_ => live.Kinematics.Match(
                Some: rows => (s.Velocity is null ? Fin.Succ(unit) : rows.V.Write(s.Velocity))
                    .Bind(_ => s.Acceleration is null ? Fin.Succ(unit) : rows.A.Write(s.Acceleration)),
                None: static () => Fin.Succ(unit))));

    public void Dispose() => Switch(inert: static _ => unit, live: static live => { live.Writer.Dispose(); return unit; });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class SolveModes {
    public static Fin<Unit> Seal(Option<SolveArchive> archive, SolveProblem problem, SolveResult result) =>
        (from capability in archive
         from values in result.EigenValues
         select (Capability: capability, Values: values)).Match(
            None: () => Fin.Succ(unit),
            Some: row => ChunkGrid.Derive([row.Values.Length], components: checked((int)result.Dofs), FieldPack.ChunkElementTarget).ToFin()
                .Bind(grid => Op.Of(name: "solve.archive-modes").Catch(() => {
                    int pairs = row.Values.Length, dofs = checked((int)result.Dofs);
                    H5Dataset<double[]> modes = new(grid.FileDims.ToArray(), grid.Chunk.ToArray(), datasetCreation: row.Capability.Policy.Creation());
                    H5File graph = new() { ["modes"] = modes, ["eigenvalues"] = row.Values.ToArray() };
                    result.Participation.Iter(rows => graph["participation"] =
                        rows.ToArray().SelectMany(static factor => new[] { factor.X, factor.Y, factor.Z }).ToArray());
                    graph.Attributes["content-key"] = $"{problem.ContentKey:x32}";
                    graph.Attributes["physics"] = problem.Physics.Key;
                    result.Condensation.Iter(evidence => {
                        graph.Attributes["retained"] = evidence.Retained;
                        graph.Attributes["condensed"] = evidence.Condensed;
                        graph.Attributes["residual"] = evidence.Residual;
                        graph.Attributes["conditioning"] = evidence.Conditioning;
                    });
                    using HdfWriter writer = HdfArchive.Begin(graph, row.Capability.Sink(SolveArchiveKind.Modes), row.Capability.Policy);
                    ChunkCursor<double> cursor = writer.Open(modes, grid);
                    ReadOnlySpan<double> flat = result.Field.Span;
                    Fin<Unit> landed = Fin.Succ(unit);
                    for (int mode = 0; mode < pairs && landed.IsSucc; mode++) {
                        landed = cursor.Write(flat.Slice(mode * dofs, dofs).ToArray());
                    }
                    return landed;
                })));
}

public static class SolveCheckpoint {
    public static Fin<Unit> Commit(
        Option<SolveArchive> archive, SolveProblem problem, int step, double[] field, double load,
        ConstitutiveState[] committed, Seq<double[]> multipliers) =>
        archive.Match(
            None: () => Fin.Succ(unit),
            Some: capability => Op.Of(name: "solve.archive-checkpoint").Catch(() => {
                H5File graph = new() { ["field"] = field };
                Ledger(graph, committed);
                if (!multipliers.IsEmpty) {
                    graph["multipliers"] = multipliers.Bind(static row => row.AsIterable()).ToArray();
                    graph["multiplier-offsets"] = multipliers.Fold(Seq(0), static (offsets, row) => offsets.Add(offsets.Last + row.Length)).ToArray();
                }
                graph.Attributes["content-key"] = $"{problem.ContentKey:x32}";
                graph.Attributes["step"] = step;
                graph.Attributes["load"] = load;
                using HdfWriter writer = HdfArchive.Begin(graph, capability.Sink(SolveArchiveKind.Checkpoint), capability.Policy);
                return Fin.Succ(unit);
            }));

    static void Ledger(H5File graph, ConstitutiveState[] committed) {
        if (committed.Length == 0) { return; }
        int components = committed[0].PlasticStrain.Length;
        double[] plastic = new double[committed.Length * components];
        double[] hardening = new double[committed.Length], damage = new double[committed.Length];
        double[] volumetric = new double[committed.Length], driving = new double[committed.Length];
        for (int row = 0; row < committed.Length; row++) {
            committed[row].PlasticStrain.Span.CopyTo(plastic.AsSpan(row * components, components));
            hardening[row] = committed[row].Hardening;
            damage[row] = committed[row].Damage;
            volumetric[row] = committed[row].VolumetricPlasticStrain;
            driving[row] = committed[row].DamageDriving;
        }
        graph["plastic-strain"] = new H5Dataset(plastic, fileDims: [(ulong)committed.Length, (ulong)components]);
        graph["hardening"] = hardening;
        graph["damage"] = damage;
        graph["volumetric"] = volumetric;
        graph["damage-driving"] = driving;
        committed[0].PreconsolidationPressure.Iter(_ =>
            graph["preconsolidation"] = committed.Map(static row => row.PreconsolidationPressure.IfNone(0.0)).ToArray());
        committed[0].PorePressure.Iter(_ =>
            graph["pore-pressure"] = committed.Map(static row => row.PorePressure.IfNone(0.0)).ToArray());
    }
}
```

## [05]-[COUPLED_FIELDS]

- Owner: `CouplingScheme` `[SmartEnum<string>]` carries each scheme's own round behaviour; `FieldTransfer` binds explicit `(donor, receiver)` index pairs; `CouplingPolicy`/`CoupledProblem`/`CoupledResult` the carriers; `CoupledLane` the fold.
- Cases: `CouplingScheme` one-way · two-way · staggered — the two bools they replaced spelled four corners for three legal ones, and the `Relaxes` half had zero readers while the fold consulted a separate policy column for the same fact.
- Entry: `public static Fin<CoupledResult> Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, LanePolicy policy, SolveRoute route, IClock clock)`.
- Boundary: transfer rows are EXPLICIT `(donor, receiver)` index pairs per weight, never a positional map — two coupled fields are discretized independently, so binding donor slot `k` to receiver dof `k` injects one field's boundary values onto unrelated degrees of freedom and the staggering converges on an answer nothing asked for. Both ends of every pair are range-checked against the field they index, because zero-filling an out-of-range donor publishes a transferred zero indistinguishable from a measured one.
- Boundary: the relaxation factor rides the SCHEME row, so a scheme that relaxes and a policy that carries an Aitken column can never disagree about whether relaxation runs.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CouplingScheme {
    public static readonly CouplingScheme OneWay = new("one-way", rounds: false, static (_, _, fixedFactor) => fixedFactor);
    public static readonly CouplingScheme TwoWay = new("two-way", rounds: true, static (_, _, fixedFactor) => fixedFactor);
    public static readonly CouplingScheme Staggered = new("staggered", rounds: true, CoupledLane.Aitken);

    public bool Rounds { get; }

    [UseDelegateFromConstructor]
    public partial double Blend(double priorFactor, (Seq<double> Prior, Seq<double> Current) deltas, double fixedFactor);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record FieldTransfer(
    int From, int To, FieldStation Source, FieldStation Target,
    ImmutableArray<(long Donor, long Receiver)> Pairs, ImmutableArray<double> Weights) {
    public Fin<BoundaryCondition> Lower(ReadOnlyMemory<double> donor, int receiverDofs) =>
        Pairs.Length != Weights.Length || Pairs.IsEmpty
            ? Fin.Fail<BoundaryCondition>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(Pairs.Length, Weights.Length))))
            : toSeq(Pairs).Map(static (pair, index) => (Index: index, Pair: pair))
                .Traverse(row =>
                    row.Pair.Donor < 0 || row.Pair.Donor >= donor.Length
                        ? Fail<Error, (long Node, double Value)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(row.Pair.Donor, donor.Length))))
                        : row.Pair.Receiver < 0 || row.Pair.Receiver >= receiverDofs
                            ? Fail<Error, (long Node, double Value)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(row.Pair.Receiver, receiverDofs))))
                            : Success<Error, (long Node, double Value)>((row.Pair.Receiver, Weights[row.Index] * donor.Span[(int)row.Pair.Donor])))
                .As().ToFin()
                .Map(rows => (BoundaryCondition)new BoundaryCondition.Dirichlet(
                    Target, [.. rows.Map(static row => row.Node)], [.. rows.Map(static row => row.Value)]));
}

public sealed record CouplingPolicy(CouplingScheme Scheme, Dimension MaxRounds, PositiveMagnitude Tolerance, PositiveMagnitude Relaxation) {
    public static readonly CouplingPolicy ThermalStructural = new(
        CouplingScheme.Staggered, Dimension.Create(50), PositiveMagnitude.Create(1e-6), PositiveMagnitude.Create(0.5));
    public static readonly CouplingPolicy FluidStructure = new(
        CouplingScheme.TwoWay, Dimension.Create(100), PositiveMagnitude.Create(1e-5), PositiveMagnitude.Create(0.3));
}

public sealed record CoupledProblem(Seq<SolveProblem> Fields, Seq<FieldTransfer> Transfers, CouplingPolicy Policy) {
    public bool WellPosed => Fields.Count >= 2 && Transfers.ForAll(t => t.From < Fields.Count && t.To < Fields.Count);
}

public sealed record CoupledResult(Seq<SolveResult> Fields, int Rounds, Convergence Verdict, Seq<double> BlendHistory, Instant At) {
    public double CouplingResidual => Verdict switch { Convergence.Converged converged => converged.Residual, _ => double.PositiveInfinity };
    public bool Converged => Verdict is Convergence.Converged;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class CoupledLane {
    public static Fin<CoupledResult> Couple(
        CoupledProblem coupling, Seq<DiscreteMesh> meshes, LanePolicy policy, SolveRoute route, IClock clock) =>
        !coupling.WellPosed
            ? Fin.Fail<CoupledResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Counts(coupling.Fields.Count, coupling.Transfers.Count, meshes.Count))))
            : coupling.Policy.Scheme.Rounds
                ? Iterate(coupling, meshes, policy, route, clock)
                : SolveRound(coupling, meshes, policy, route, Seq<SolveResult>(), clock)
                    .Map(fields => new CoupledResult(fields, 1, new Convergence.Converged(0.0), Seq<double>(), clock.GetCurrentInstant()));

    public static ComputeReceipt.Coupling Receipt(CoupledProblem coupling, CoupledResult result, CorrelationId correlation, Duration elapsed) =>
        new(coupling.Policy.Scheme.Key, coupling.Fields.Count, coupling.Transfers.Count, result.Rounds, result.CouplingResidual, result.Converged) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    static Fin<CoupledResult> Iterate(
        CoupledProblem coupling, Seq<DiscreteMesh> meshes, LanePolicy policy, SolveRoute route, IClock clock) =>
        SolveRound(coupling, meshes, policy, route, Seq<SolveResult>(), clock).Bind(seed =>
            Fixpoint.Run(
                (Fields: seed, Factor: coupling.Policy.Relaxation.Value, Prior: Seq<double>(), History: Seq<double>()),
                coupling.Policy.MaxRounds,
                (state, _) => SolveRound(coupling, meshes, policy, route, state.Fields, clock).Map(next => {
                    Seq<double> delta = Delta(state.Fields, next);
                    double residual = Math.Sqrt(delta.Sum(static d => d * d));
                    double factor = coupling.Policy.Scheme.Blend(state.Factor, (state.Prior, delta), coupling.Policy.Relaxation.Value);
                    var advanced = (Relax(state.Fields, next, factor), factor, delta, state.History.Add(factor));
                    return residual <= coupling.Policy.Tolerance.Value ? Step.Settle(advanced, residual) : Step.Advance(advanced);
                })))
            .Bind(run => run.Verdict is Convergence.Converged
                ? Fin.Succ(new CoupledResult(run.State.Fields, run.Steps, run.Verdict, run.State.History, clock.GetCurrentInstant()))
                : Fin.Fail<CoupledResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.Count(run.Steps, coupling.Policy.MaxRounds.Value))))));

    static Fin<Seq<SolveResult>> SolveRound(
        CoupledProblem coupling, Seq<DiscreteMesh> meshes, LanePolicy policy, SolveRoute route, Seq<SolveResult> prior, IClock clock) =>
        toSeq(Enumerable.Range(0, coupling.Fields.Count)).Fold(Fin.Succ(Seq<SolveResult>()), (acc, index) =>
            acc.Bind(solved => {
                SolveProblem field = coupling.Fields[index];
                int receiverDofs = checked((int)meshes[index].NodeCount) * field.Dof;
                return coupling.Transfers
                    .Filter(t => t.To == index && (t.From < solved.Count || t.From < prior.Count))
                    .Traverse(t => t.Lower(t.From < solved.Count ? solved[t.From].Field : prior[t.From].Field, receiverDofs))
                    .As()
                    .Bind(injected => SolveLane.Solve(field with { Conditions = field.Conditions + injected }, meshes[index], policy, route, clock))
                    .Map(result => solved.Add(result));
            }));

    static Seq<double> Delta(Seq<SolveResult> previous, Seq<SolveResult> current) =>
        previous.Count != current.Count
            ? Seq(double.MaxValue)
            : toSeq(Enumerable.Range(0, current.Count)).Bind(field => {
                ReadOnlySpan<double> a = previous[field].Field.Span, b = current[field].Field.Span;
                List<double> diffs = new(b.Length);
                for (int i = 0; i < a.Length && i < b.Length; i++) { diffs.Add(b[i] - a[i]); }
                return toSeq(diffs);
            });

    internal static double Aitken(double priorFactor, (Seq<double> Prior, Seq<double> Current) deltas, double fixedFactor) {
        if (deltas.Prior.Count != deltas.Current.Count || deltas.Prior.IsEmpty) { return priorFactor; }
        double dotDiff = 0.0, normDiff = 0.0;
        for (int i = 0; i < deltas.Current.Count; i++) {
            double dr = deltas.Current[i] - deltas.Prior[i];
            dotDiff += deltas.Prior[i] * dr;
            normDiff += dr * dr;
        }
        return normDiff > EpsilonPolicy.ZeroTolerance ? Math.Clamp(-priorFactor * dotDiff / normDiff, 0.05, 1.0) : priorFactor;
    }

    static Seq<SolveResult> Relax(Seq<SolveResult> previous, Seq<SolveResult> current, double factor) =>
        previous.Count != current.Count
            ? current
            : toSeq(Enumerable.Range(0, current.Count)).Map(field => {
                ReadOnlySpan<double> a = previous[field].Field.Span, b = current[field].Field.Span;
                double[] blended = new double[b.Length];
                int shared = Math.Min(a.Length, b.Length);
                TensorPrimitives.Lerp(a[..shared], b[..shared], factor, blended.AsSpan(0, shared));
                b[shared..].CopyTo(blended.AsSpan(shared));
                return current[field] with { Field = blended.AsMemory() };
            });
}
```

## [06]-[RESEARCH]

(none)
