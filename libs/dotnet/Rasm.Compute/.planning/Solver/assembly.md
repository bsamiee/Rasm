# [COMPUTE_SOLVER_ASSEMBLY]

Rasm.Compute discrete-operator assembly: the fold that turns a `Solver/contract#SOLVE_REQUEST` `SolveProblem` and its admitted `Solver/field#DISCRETE_FIELD` `DiscreteMesh` into the constrained sparse system every `Solver/route#SOLVE_ROUTES` body consumes. Two operator families and one inertia fold live here — the isoparametric `Bᵀ·D·B` accumulation over cells for continuum elements, the closed-form 12-DOF member scatter for the frame rows, the dense `n × n` lowering the radiosity and energy-network payloads take instead, and the lumped inertia that carries real mass for the elastic forms and real volumetric heat capacity for the scalar ones.

The boundary-condition family owns constraint application on both sides: elimination and penalty mutate operator and right-hand side in the existing pattern, Lagrange borders the system with multiplier rows, and contact contributes NOTHING to the linear system because its enforcement is nonlinear-only and lives in the residual the Newton route evaluates. Every ingest crosses `Tensor/factor#SPARSE_SOLVE` `SparseOps.Ingest`; nothing here mints a storage type or hand-rolls a triplet finalization.

## [01]-[INDEX]

- [02]-[OPERATOR_ASSEMBLY]: the cell fold, the strain-displacement rows, the payload operator terms, and the dense network lowering.
- [03]-[DOF_CONSTRAINT]: the boundary-condition family, its canonical framing, and the constrained-system carrier.
- [04]-[INERTIA]: lumped mass and volumetric capacity, the peak-relative inertia floor, and the mass-singular refusal.

## [02]-[OPERATOR_ASSEMBLY]

- Owner: `OperatorAssembly` the assembly fold; `CellAssembly` the `IAction` struct the parallel cell range fans; `LocalBlock` the per-cell delegate the continuum and frame legs both satisfy.
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Assemble(SolveProblem problem, DiscreteMesh mesh, LanePolicy policy)` — forks on the operator row's own dense column, folds every other payload through the parallel cell assembly, and lands one CSR through `SparseOps.Ingest`.
- Auto: the coefficient lowering binds ONCE at entry, so the per-cell reader is total and no Gauss point re-runs a fallible payload dispatch; each cell writes its `block × block` local into a pooled scratch plane, scatters into three preallocated flat triplet columns at its own offset, and lands its outcome in a per-cell rail the fold traverses after the fan.
- Result: none of its own — the assembly's evidence is the DOF count and the residual on `Solver/contract`'s `Solve` result.
- Packages: MathNet.Numerics, CSparse, CommunityToolkit.HighPerformance (`SpanOwner<T>`/`MemoryOwner<T>`/`Span2D<T>`/`ParallelHelper`), System.Numerics.Tensors, LanguageExt.Core, BCL inbox
- Growth: a new payload operator term is one arm on the Gauss-point fold keyed by the payload case; a new element family is one `LocalBlock` binding; zero new surface.
- Boundary: the dense network lowering is `n × n` by the payload's own shape, so it builds its operator directly rather than staging three `n²` triplet columns to say the same thing — a coordinate staging of a dense matrix pays three quadratic allocations for a form the storage factory takes whole.
- Boundary: the per-cell local block is a POOLED plane rented once per cell, and the strain-displacement rows write into a second pooled plane rather than a fresh `double[strain × cols]` per Gauss point — the assembly fold visits every Gauss point of every cell, so a per-point allocation is the dominant garbage of a building-order run.
- Exemption: the `Bᵀ·D·B` accumulation, the strain-row scatter, the flow advection, and the mass terms are MEASURED span kernels over fixed small arities — each dies with the cell that fills it and none crosses a page surface.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class OperatorAssembly {
    public static Fin<SparseCompressedRowMatrixStorage<double>> Assemble(SolveProblem problem, DiscreteMesh mesh, LanePolicy policy) {
        int dim = problem.Dof * checked((int)mesh.NodeCount);
        return problem.Payload switch {
            PhysicsPayload.Radiosity radiosity => Network(dim, (row, column) =>
                (row == column ? 1.0 : 0.0) - radiosity.Reflectance.Span[row] * radiosity.ViewFactors.Span[row * dim + column]),
            PhysicsPayload.EnergyNetwork energy => Network(dim, (row, column) => energy.Conductance.Span[row * dim + column]),
            _ => Triplets(mesh, problem).Bind(t => SparseOps.Ingest(SparseFormat.Coo, dim, dim, t.Rows, t.Cols, t.Vals)),
        };
    }

    static Fin<SparseCompressedRowMatrixStorage<double>> Network(int size, Func<int, int, double> coefficient) =>
        SparseOps.Ingest(SparseFormat.Csr, size, size,
            [.. Enumerable.Range(0, size + 1).Select(row => row * size)],
            [.. Enumerable.Range(0, checked(size * size)).Select(slot => slot % size)],
            [.. Enumerable.Range(0, checked(size * size)).Select(slot => coefficient(slot / size, slot % size))]);

    internal static Fin<(int[] Rows, int[] Cols, double[] Vals)> Triplets(DiscreteMesh mesh, SolveProblem problem) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof;
        int cells = checked((int)mesh.ElementCount), entries = cells * block * block;
        return Local(mesh, problem).Map(local => {
            Fin<Unit>[] outcomes = new Fin<Unit>[cells];
            CellAssembly assembly = new(mesh, per, dof, local, new int[entries], new int[entries], new double[entries], outcomes);
            ParallelHelper.For(0, cells, in assembly);
            return (assembly, outcomes);
        }).Bind(fan => toSeq(fan.outcomes).Traverse(static outcome => outcome)
            .Map(_ => (fan.assembly.Rows, fan.assembly.Cols, fan.assembly.Vals)).As());
    }

    static Fin<LocalBlock> Local(DiscreteMesh mesh, SolveProblem problem) =>
        mesh.Element.Family == ShapeFamily.Frame
            ? Fin.Succ<LocalBlock>((cell, block) => problem.Field.MechanicalAt(cell).Bind(properties =>
                FrameKernel.Member(mesh.Element, mesh.NodalXyz(cell), problem.Members[cell], properties.Young, properties.Poisson, block)))
            : problem.Field.Lower(problem.Physics, problem.Payload).Map(coefficient => (LocalBlock)((cell, block) => {
                LocalStiffness(mesh, problem, coefficient(cell), cell, block);
                return Fin.Succ(unit);
            }));

    internal delegate Fin<Unit> LocalBlock(int cell, Span<double> local);

    readonly struct CellAssembly(DiscreteMesh mesh, int per, int dof, LocalBlock localBlock, int[] rows, int[] cols, double[] vals, Fin<Unit>[] outcomes) : IAction {
        public int[] Rows => rows;
        public int[] Cols => cols;
        public double[] Vals => vals;

        public void Invoke(int cell) {
            ReadOnlySpan<long> conn = mesh.Indices;
            int block = per * dof;
            using SpanOwner<double> scratch = SpanOwner<double>.Allocate(block * block, AllocationMode.Clear);
            outcomes[cell] = localBlock(cell, scratch.Span);
            if (outcomes[cell].IsFail) { return; }
            ReadOnlySpan2D<double> local = ReadOnlySpan2D<double>.DangerousCreate(in scratch.Span[0], block, block, 0);
            int t = cell * block * block;
            for (int a = 0; a < per; a++)
                for (int ci = 0; ci < dof; ci++)
                    for (int b = 0; b < per; b++)
                        for (int cj = 0; cj < dof; cj++, t++) {
                            rows[t] = (int)conn[cell * per + a] * dof + ci;
                            cols[t] = (int)conn[cell * per + b] * dof + cj;
                            vals[t] = local[a * dof + ci, b * dof + cj];
                        }
        }
    }

    static void LocalStiffness(DiscreteMesh mesh, SolveProblem problem, double[] material, int cell, Span<double> local) {
        ElementClass element = mesh.Element;
        int per = element.Nodes, dof = problem.Dof, strain = problem.Physics.StrainDim, cols = per * dof;
        ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
        using SpanOwner<double> rows = SpanOwner<double>.Allocate(strain * cols);
        foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Rule.Points) {
            ShapeSample sample = element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
            double weight = gauss.Weight * Math.Abs(sample.DetJ);
            rows.Span.Clear();
            Strain(problem.Physics.Form, sample.Grad, per, dof, cols, rows.Span);
            Accumulate(rows.Span, material, strain, cols, weight, local);
            switch (problem.Payload) {
                case PhysicsPayload.Flow flow:
                    FlowAdvection(sample, flow, cell, per, dof, weight, local);
                    break;
                case PhysicsPayload.Helmholtz wave:
                    ScalarMass(sample, per, dof, -wave.WaveNumber * wave.WaveNumber, weight, local);
                    break;
                case PhysicsPayload.EddyCurrent eddy:
                    EddyMass(sample, per, dof, eddy.AngularFrequency * eddy.Conductivity, weight, local);
                    break;
            }
        }
    }

    static void FlowAdvection(ShapeSample sample, PhysicsPayload.Flow flow, int cell, int per, int dof, double weight, Span<double> local) {
        ReadOnlySpan<double> velocity = flow.Velocity.Span.Slice(cell * 3, 3);
        for (int a = 0; a < per; a++)
            for (int b = 0; b < per; b++) {
                double convection = flow.Density * sample.Shape[a] * (velocity[0] * sample.Grad[b * 3] + velocity[1] * sample.Grad[b * 3 + 1] + velocity[2] * sample.Grad[b * 3 + 2]);
                for (int component = 0; component < 3; component++) { local[(a * dof + component) * (per * dof) + b * dof + component] += weight * convection; }
            }
    }

    static void ScalarMass(ShapeSample sample, int per, int dof, double coefficient, double weight, Span<double> local) {
        int cols = per * dof;
        for (int a = 0; a < per; a++)
            for (int b = 0; b < per; b++)
                for (int component = 0; component < dof; component++) { local[(a * dof + component) * cols + b * dof + component] += weight * coefficient * sample.Shape[a] * sample.Shape[b]; }
    }

    static void EddyMass(ShapeSample sample, int per, int dof, double coefficient, double weight, Span<double> local) {
        int cols = per * dof;
        for (int a = 0; a < per; a++)
            for (int b = 0; b < per; b++)
                for (int component = 0; component < 3; component++) {
                    double value = weight * coefficient * sample.Shape[a] * sample.Shape[b];
                    local[(a * dof + component) * cols + b * dof + component + 3] -= value;
                    local[(a * dof + component + 3) * cols + b * dof + component] += value;
                }
    }

    internal static void Strain(MaterialForm form, ReadOnlySpan<double> grad, int per, int dof, int cols, Span<double> b) {
        for (int a = 0; a < per; a++) {
            double gx = grad[a * 3], gy = grad[a * 3 + 1], gz = grad[a * 3 + 2];
            if (form == MaterialForm.Elasticity) {
                int x = a * 3, y = a * 3 + 1, z = a * 3 + 2;
                b[0 * cols + x] = gx; b[1 * cols + y] = gy; b[2 * cols + z] = gz;
                b[3 * cols + x] = gy; b[3 * cols + y] = gx;
                b[4 * cols + y] = gz; b[4 * cols + z] = gy;
                b[5 * cols + x] = gz; b[5 * cols + z] = gx;
            }
            else if (form == MaterialForm.Isotropic) {
                b[0 * cols + a] = gx; b[1 * cols + a] = gy; b[2 * cols + a] = gz;
            }
            else if (form == MaterialForm.MixedFlow) {
                for (int component = 0; component < 3; component++) {
                    int column = a * 4 + component;
                    b[(3 * component + 0) * cols + column] = gx;
                    b[(3 * component + 1) * cols + column] = gy;
                    b[(3 * component + 2) * cols + column] = gz;
                }
                b[9 * cols + a * 4 + 3] = 1.0;
            }
            else {
                for (int field = 0; field < 2; field++) {
                    int offset = a * 6 + field * 3, row = field * 3;
                    b[(row + 0) * cols + offset + 1] = -gz; b[(row + 0) * cols + offset + 2] = gy;
                    b[(row + 1) * cols + offset + 0] = gz; b[(row + 1) * cols + offset + 2] = -gx;
                    b[(row + 2) * cols + offset + 0] = -gy; b[(row + 2) * cols + offset + 1] = gx;
                }
            }
        }
    }

    internal static void Accumulate(ReadOnlySpan<double> b, ReadOnlySpan<double> d, int strain, int cols, double weight, Span<double> local) {
        for (int i = 0; i < cols; i++)
            for (int j = 0; j < cols; j++) {
                double sum = 0.0;
                for (int r = 0; r < strain; r++) {
                    double db = 0.0;
                    for (int s = 0; s < strain; s++) { db += d[r * strain + s] * b[s * cols + j]; }
                    sum += b[r * cols + i] * db;
                }
                local[i * cols + j] += weight * sum;
            }
    }
}
```

## [03]-[DOF_CONSTRAINT]

- Owner: `BoundaryCondition` `[Union]` closes the constraint family and owns its own validation, application, and canonical framing; `ConstrainedSystem` carries the operator, right-hand side, prescribed set, and penalty the routes read.
- Cases: `Dirichlet` · `Neumann` · `Robin` · `Periodic` · `Contact` — the contact case carries the constitutive-owned `ContactConstraint` discriminant beside its dof pairing, and each id names the BASE dof of a node's translational triple because the gap projects onto the constraint normal, so a pair binds six rows and not two.
- Entry: `public static Fin<ConstrainedSystem> Constrained(SparseCompressedRowMatrixStorage<double> operatorCsr, Seq<BoundaryCondition> conditions, LanePolicy policy)` — validates every condition on ONE accumulating pass, then folds each application in declaration order.
- Auto: elimination and penalty rewrite values in the existing pattern; Lagrange borders the system with one multiplier row and column per constraint through a fresh ingest; contact returns the system unchanged.
- Boundary: the Dirichlet elimination walks the constrained NODE SET in one CSR pass rather than the whole operator once per constrained node — the pair-of-loops form was quadratic in `constrained × nnz`, which at building scale is the dominant cost of admitting a support condition.
- Boundary: `Augment` and `AddAt` were two spellings of ONE operation — add into an existing CSR slot — one scanning the row linearly and one binary-searching the same sorted row. The binary search survives and the scan deletes; connectivity guarantees the slot exists for a pattern-preserving add, and a contact coupling no element makes enters through the re-ingest instead.
- Boundary: contact contributes NOTHING to the linear system: it is nonlinear-only, enforced per residual evaluation through the constitutive `ContactEnforcement` owner with current kinematics and the step's committed multipliers — a precomputed constant force is the deleted form.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundaryCondition {
    private BoundaryCondition() { }

    public sealed record Dirichlet(FieldStation Station, long[] Nodes, double[] Values) : BoundaryCondition;
    public sealed record Neumann(long[] Faces, double[] Flux) : BoundaryCondition;
    public sealed record Robin(long[] Faces, double Coefficient, double Ambient) : BoundaryCondition;
    public sealed record Periodic(long[] Master, long[] Slave) : BoundaryCondition;
    public sealed record Contact(ContactConstraint Constraint, long[] Slave, long[] Master, double Penalty) : BoundaryCondition;

    public Fin<Unit> Validate(int dofs) =>
        Switch(
            state: dofs,
            dirichlet: static (n, bc) => Admit(
                Claim(bc.Nodes.Length == bc.Values.Length, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(bc.Values.Length, bc.Nodes.Length))),
                Claim(bc.Nodes.Length > 0, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(bc.Nodes.Length, 1L))),
                Claim(InRange(bc.Nodes, n), new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(bc.Nodes.Length))),
                Claim(bc.Values.All(double.IsFinite), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(bc.Values.Length)))),
            neumann: static (n, bc) => Admit(
                Claim(bc.Faces.Length == bc.Flux.Length, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(bc.Flux.Length, bc.Faces.Length))),
                Claim(bc.Faces.Length > 0, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(bc.Faces.Length, 1L))),
                Claim(InRange(bc.Faces, n), new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(bc.Faces.Length))),
                Claim(bc.Flux.All(double.IsFinite), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(bc.Flux.Length)))),
            robin: static (n, bc) => Admit(
                Claim(bc.Faces.Length > 0, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(bc.Faces.Length, 1L))),
                Claim(InRange(bc.Faces, n), new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(bc.Faces.Length))),
                Claim(double.IsFinite(bc.Coefficient), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(bc.Coefficient))),
                Claim(double.IsFinite(bc.Ambient), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(bc.Ambient)))),
            periodic: static (n, bc) => Admit(
                Claim(bc.Master.Length == bc.Slave.Length, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(bc.Slave.Length, bc.Master.Length))),
                Claim(bc.Master.Length > 0, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(bc.Master.Length, 1L))),
                Claim(InRange(bc.Master, n), new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(bc.Master.Length))),
                Claim(InRange(bc.Slave, n), new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(bc.Slave.Length)))),
            contact: static (n, bc) => Admit(
                Claim(bc.Master.Length == bc.Slave.Length, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(bc.Slave.Length, bc.Master.Length))),
                Claim(bc.Master.Length > 0, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(bc.Master.Length, 1L))),
                Claim(Triples(bc.Master, n), new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(bc.Master.Length))),
                Claim(Triples(bc.Slave, n), new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(bc.Slave.Length))),
                Claim(double.IsFinite(bc.Penalty), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(bc.Penalty))),
                Claim(bc.Penalty > 0.0, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(bc.Penalty)))));

    public Fin<ConstrainedSystem> Apply(ConstrainedSystem system, ConstraintMethod constraint) =>
        constraint.Bordered ? ApplyBordered(system) : Fin.Succ(ApplyFixed(system, constraint));

    ConstrainedSystem ApplyFixed(ConstrainedSystem system, ConstraintMethod constraint) =>
        Switch(
            state: (System: system, Constraint: constraint),
            dirichlet: static (s, bc) => s.Constraint == ConstraintMethod.Penalty
                ? Penalized(s.System, bc)
                : Eliminated(s.System, bc),
            neumann: static (s, bc) => s.System with { Rhs = Loaded(s.System.Rhs, bc.Faces, index => bc.Flux[index]) },
            robin: static (s, bc) => s.System with { Rhs = Loaded(s.System.Rhs, bc.Faces, _ => bc.Coefficient * bc.Ambient) },
            periodic: static (s, bc) => Tied(s.System, bc),
            contact: static (s, _) => s.System);

    static ConstrainedSystem Penalized(ConstrainedSystem system, Dirichlet bc) {
        double[] rhs = (double[])system.Rhs.Clone();
        double[] values = (double[])system.Operator.Values.Clone();
        LanguageExt.HashSet<long> prescribed = system.Constrained;
        for (int i = 0; i < bc.Nodes.Length; i++) {
            int node = (int)bc.Nodes[i];
            AddAt(system.Operator, values, node, node, system.Penalty);
            rhs[node] = system.Penalty * bc.Values[i];
            prescribed = prescribed.Add(bc.Nodes[i]);
        }
        return system with { Operator = Rebuilt(system.Operator, values), Rhs = rhs, Constrained = prescribed };
    }

    static ConstrainedSystem Eliminated(ConstrainedSystem system, Dirichlet bc) {
        double[] rhs = (double[])system.Rhs.Clone();
        double[] values = (double[])system.Operator.Values.Clone();
        int[] rowPtr = system.Operator.RowPointers, colIdx = system.Operator.ColumnIndices;
        Map<long, double> prescribed = toSeq(bc.Nodes).Zip(toSeq(bc.Values)).Fold(
            Map<long, double>(), static (acc, pair) => acc.AddOrUpdate(pair.Item1, pair.Item2));
        for (int row = 0; row < system.Operator.RowCount; row++) {
            bool rowFixed = prescribed.ContainsKey(row);
            for (int slot = rowPtr[row]; slot < rowPtr[row + 1]; slot++) {
                long column = colIdx[slot];
                if (rowFixed) { values[slot] = column == row ? 1.0 : 0.0; continue; }
                if (prescribed.Find(column).Case is double value) { rhs[row] -= values[slot] * value; values[slot] = 0.0; }
            }
        }
        prescribed.Iter((node, value) => rhs[(int)node] = value);
        return system with {
            Operator = Rebuilt(system.Operator, values), Rhs = rhs,
            Constrained = prescribed.Keys.Fold(system.Constrained, static (acc, node) => acc.Add(node)),
        };
    }

    static ConstrainedSystem Tied(ConstrainedSystem system, Periodic bc) {
        double penalty = system.Penalty;
        double[] values = (double[])system.Operator.Values.Clone();
        LanguageExt.HashSet<long> prescribed = system.Constrained;
        int pairs = Math.Min(bc.Master.Length, bc.Slave.Length);
        for (int p = 0; p < pairs; p++) {
            int master = (int)bc.Master[p], slave = (int)bc.Slave[p];
            AddAt(system.Operator, values, master, master, penalty);
            AddAt(system.Operator, values, slave, slave, penalty);
            AddAt(system.Operator, values, master, slave, -penalty);
            AddAt(system.Operator, values, slave, master, -penalty);
            prescribed = prescribed.Add(bc.Slave[p]);
        }
        return system with { Operator = Rebuilt(system.Operator, values), Constrained = prescribed };
    }

    static double[] Loaded(double[] rhs, long[] faces, Func<int, double> value) {
        double[] loaded = (double[])rhs.Clone();
        for (int i = 0; i < faces.Length; i++) { loaded[faces[i]] += value(i); }
        return loaded;
    }

    Fin<ConstrainedSystem> ApplyBordered(ConstrainedSystem system) =>
        Switch(
            state: system,
            dirichlet: static (current, bc) => Border(current, bc.Nodes, [], bc.Values),
            neumann: static (current, bc) => Fin.Succ(((BoundaryCondition)bc).ApplyFixed(current, ConstraintMethod.Elimination)),
            robin: static (current, bc) => Fin.Succ(((BoundaryCondition)bc).ApplyFixed(current, ConstraintMethod.Elimination)),
            periodic: static (current, bc) => Border(current, bc.Master, bc.Slave, new double[bc.Master.Length]),
            contact: static (current, bc) => Fin.Succ(((BoundaryCondition)bc).ApplyFixed(current, ConstraintMethod.Elimination)));

    static Fin<ConstrainedSystem> Border(ConstrainedSystem system, long[] positive, long[] negative, double[] prescribed) {
        int original = system.Operator.RowCount, constraints = positive.Length, dimension = original + constraints;
        List<int> rows = new(system.Operator.Values.Length + constraints * (negative.Length == 0 ? 2 : 4));
        List<int> columns = new(rows.Capacity);
        List<double> values = new(rows.Capacity);
        for (int row = 0; row < original; row++) {
            for (int slot = system.Operator.RowPointers[row]; slot < system.Operator.RowPointers[row + 1]; slot++) {
                rows.Add(row); columns.Add(system.Operator.ColumnIndices[slot]); values.Add(system.Operator.Values[slot]);
            }
        }
        for (int constraint = 0; constraint < constraints; constraint++) {
            int multiplier = original + constraint, plus = checked((int)positive[constraint]);
            rows.Add(plus); columns.Add(multiplier); values.Add(1.0);
            rows.Add(multiplier); columns.Add(plus); values.Add(1.0);
            if (negative.Length != 0) {
                int minus = checked((int)negative[constraint]);
                rows.Add(minus); columns.Add(multiplier); values.Add(-1.0);
                rows.Add(multiplier); columns.Add(minus); values.Add(-1.0);
            }
        }
        double[] rhs = new double[dimension];
        system.Rhs.CopyTo(rhs, 0);
        prescribed.CopyTo(rhs, original);
        return SparseOps.Ingest(SparseFormat.Coo, dimension, dimension, [.. rows], [.. columns], [.. values])
            .Map(operatorCsr => system with { Operator = operatorCsr, Rhs = rhs });
    }

    public void WriteCanonical(CanonicalWriter sink) =>
        Switch(
            state: sink,
            dirichlet: static (w, bc) => Digest(w, "d", bc.Nodes, bc.Values),
            neumann: static (w, bc) => Digest(w, "n", bc.Faces, bc.Flux),
            robin: static (w, bc) => Digest(w, "r", bc.Faces, [bc.Coefficient, bc.Ambient]),
            periodic: static (w, bc) => Digest(w, "p", [.. bc.Master, .. bc.Slave], []),
            contact: static (w, bc) => Digest(w, "c", [.. bc.Slave, .. bc.Master],
                [bc.Constraint.Normal.X, bc.Constraint.Normal.Y, bc.Constraint.Normal.Z, bc.Constraint.BaseGap, bc.Constraint.Smoothing.Value, bc.Penalty]));

    static void Digest(CanonicalWriter sink, string tag, long[] ids, double[] values) {
        sink.String(tag);
        sink.Rows(toSeq(ids), static (id, writer) => writer.I64(id));
        sink.Doubles(values);
    }

    internal static void AddAt(SparseCompressedRowMatrixStorage<double> csr, double[] values, int row, int column, double delta) {
        int index = Array.BinarySearch(csr.ColumnIndices, csr.RowPointers[row], csr.RowPointers[row + 1] - csr.RowPointers[row], column);
        if (index >= 0) { values[index] += delta; }
    }

    static bool InRange(long[] indices, int dofs) => indices.All(index => index >= 0 && index < dofs);
    static bool Triples(long[] bases, int dofs) => bases.All(index => index >= 0 && index + 2 < dofs);

    static Validation<Error, Unit> Claim(bool held, ComputeViolation evidence) =>
        held ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new ComputeFault.Violation(ComputeArea.Solver, evidence));

    static Fin<Unit> Admit(params Validation<Error, Unit>[] claims) =>
        toSeq(claims).Traverse(static claim => claim).As().ToFin();

    static SparseCompressedRowMatrixStorage<double> Rebuilt(SparseCompressedRowMatrixStorage<double> operatorCsr, double[] values) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            operatorCsr.RowCount, operatorCsr.ColumnCount, values.Length, operatorCsr.RowPointers, operatorCsr.ColumnIndices, values);
}

public sealed record ConstrainedSystem(
    SparseCompressedRowMatrixStorage<double> Operator,
    double[] Rhs,
    LanguageExt.HashSet<long> Constrained,
    double Penalty);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class OperatorAssembly {
    public static Fin<ConstrainedSystem> Constrained(SparseCompressedRowMatrixStorage<double> operatorCsr, Seq<BoundaryCondition> conditions, LanePolicy policy) =>
        conditions.Traverse(condition => condition.Validate(operatorCsr.RowCount).ToValidation()).As().ToFin()
            .Bind(_ => conditions.Fold(
                Fin.Succ(new ConstrainedSystem(operatorCsr, new double[operatorCsr.RowCount], Prelude.HashSet<long>(), policy.PenaltyFactor.Value)),
                (acc, condition) => acc.Bind(system => condition.Apply(system, policy.Constraint))))
            .As();
}
```

## [04]-[INERTIA]

- Owner: `OperatorAssembly.Lumped` the one inertia fold both marching routes and both modal routes read; `InertiaFloor` the peak-relative masslessness threshold; `MassSingular` the refusal probe.
- Entry: `internal static Fin<double[]> Lumped(DiscreteMesh mesh, SolveProblem problem)` and `internal static double[] Capacity(SolveProblem problem, double[] lumped)`.
- Boundary: lumped inertia is REAL mass where the form is elastic — `ρ·A·L` over a frame cell's two joints and `ρ·∫|detJ|` over a continuum cell's nodes, the density read off the ONE `MechanicalAt` per-cell accessor. A lumped-mass frame carries NO rotary inertia, so the rotational slots stay exactly zero and static condensation of those rows is spectrally exact. For the scalar forms the coefficient is the volumetric heat capacity the ONE `CapacityAt` read supplies, so the vector a first-order march divides by is a real storage in J/K.
- Boundary: the peak-relative inertia floor is the ONE scale-derived threshold the mass-singularity refusals and the condensation partition all read, so masslessness is a fraction of the model's own peak inertia and never an absolute kilogramme literal. The fraction exists only to absorb assembly round-off: the frame idealization writes EXACT zeros on its inertia-free rows, so nothing near the floor is a judgement call.
- Exemption: the per-cell inertia scatter is a measured span kernel over the connectivity the mesh already carries.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class OperatorAssembly {
    internal const double InertiaFraction = 1e-12;

    internal static double InertiaFloor(ReadOnlySpan<double> mass) => TensorPrimitives.Max(mass) * InertiaFraction;

    internal static Option<long> MassSingular(double[] mass, LanguageExt.HashSet<long> constrained, double floor) =>
        toSeq(Enumerable.Range(0, mass.Length)).Find(dof => !constrained.Contains(dof) && mass[dof] <= floor).Map(static dof => (long)dof);

    internal static Fin<double[]> Lumped(DiscreteMesh mesh, SolveProblem problem) {
        int nodes = checked((int)mesh.NodeCount), per = mesh.Element.Nodes, dof = problem.Dof;
        bool frame = mesh.Element.Family == ShapeFamily.Frame, elastic = problem.Physics.Form == MaterialForm.Elasticity;
        int inertial = frame ? 3 : dof;
        double[] mass = new double[nodes * dof];
        ReadOnlySpan<long> conn = mesh.Indices;
        return toSeq(Enumerable.Range(0, checked((int)mesh.ElementCount))).Traverse(cell => {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            double extent = frame ? Length(xyz) : Volume(mesh, xyz);
            return (elastic
                ? problem.Field.MechanicalAt(cell).Map(row => row.Density * (frame ? problem.Members[cell].Area : 1.0))
                : problem.Field.CapacityAt(cell))
                .Map(scale => {
                    double share = extent * scale / per;
                    for (int a = 0; a < per; a++) {
                        int node = (int)conn[cell * per + a];
                        for (int ci = 0; ci < inertial; ci++) { mass[node * dof + ci] += share; }
                    }
                    return unit;
                });
        }).As().Map(_ => mass);
    }

    static double Length(ReadOnlySpan<double> xyz) {
        double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    static double Volume(DiscreteMesh mesh, ReadOnlySpan<double> xyz) {
        double extent = 0.0;
        foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Rule.Points) {
            extent += gauss.Weight * Math.Abs(mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz).DetJ);
        }
        return extent;
    }

    internal static double[] Capacity(SolveProblem problem, double[] lumped) => problem.Payload switch {
        PhysicsPayload.EnergyNetwork energy => energy.Capacity.ToArray(),
        PhysicsPayload.Flow => [.. lumped.Select((value, index) => index % 4 == 3 ? 0.0 : value)],
        _ => lumped,
    };
}
```
