# [COMPUTE_SOLVER_FIELD]

Rasm.Compute discrete field carrier: `DiscreteMesh` is the frozen volumetric mesh every solve reads — nodes, connectivity, the admitted `ElementClass`, the PROVEN `QuadratureRule` its policy elected, the measured `CellQuality` verdict, and the refinement provenance — and `FieldSpace` over `FieldStation` rows is the solve-native scalar/vector/tensor representation the assembly writes into. The mesh is the boundary between `Solver/discretization`'s generation half and `Solver/contract`'s assembly half: generation freezes into it, assembly reads out of it, and nothing above either half re-derives a station count or a chunk grid it already carries.

Quadrature rides the mesh as PROVEN evidence. `ElementClass.Quadrature` is the kernel's `Fin<QuadratureRule>` election and `Solver/discretization`'s admission binds it exactly once, so the frozen mesh carries the rule itself and every downstream read — station cardinality, Gauss-point assembly, field encode — is total. A consumer re-electing per read reopens the refusal the kernel's ceiling law exists to publish, and a consumer reading a bare rule off an unadmitted element reads a rule no admission ever proved.

## [01]-[INDEX]

- [02]-[DISCRETE_FIELD]: the frozen mesh carrier, its nodal gather and archive, and the station/rank field representation over it.

## [02]-[DISCRETE_FIELD]

- Owner: `DiscreteMesh` the conforming/non-conforming volumetric mesh carrier with its proven quadrature rule, measured quality, refinement provenance, nodal gather, and HDF archive projection; `FieldStation` `[SmartEnum<string>]` nodal/integration-point/cell/boundary rows carrying their count derivation; `FieldRank` `[SmartEnum<string>]` scalar/vector/tensor rows carrying their component derivation; `FieldSpace` the station × rank field the solve writes.
- Cases: `FieldStation` rows nodal · integration-point · cell · boundary; `FieldRank` rows scalar · vector · tensor — the component count is the ROW's own derivation over the ambient dimension, so a rank-2 field carrying five components is unrepresentable rather than merely wrong.
- Entry: `DiscreteMesh.FieldOf(FieldStation, FieldRank, int dim)` mints the field space; `FieldSpace.OfKey(DiscreteMesh, string station, string rank, int dim)` resolves both vocabulary keys and accumulates every unrostered one; `DiscreteMesh.NodalXyz(long element)` is the per-cell nodal gather every assembly, metric, and inertia fold reads; `DiscreteMesh.Archive(Stream, HdfArchivePolicy)` writes the two-dataset container through the ONE archive session owner.
- Auto: `FieldSpace.Layout` projects the `Runtime/archive#CHUNK_CURSOR` station-outermost chunk grid, so every `FieldSpace`-shaped producer — solve history, ensemble store, field encode — reads one derivation and no second chunk arithmetic forks the address a consumer computes.
- Result: none of its own — the mesh is `Solver/discretization`'s `DiscreteMesh` and the solve result is `Solver/contract`'s.
- Packages: Rasm (project — kernel `QuadratureRule`), PureHDF (through `Runtime/archive#HDF_ARCHIVE` alone), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new field station is one `FieldStation` row carrying its count derivation; a new field rank is one `FieldRank` row carrying its component derivation; a new mesh column is one field on `DiscreteMesh` with its `Pack` line; zero new surface.
- Boundary: HDF5 crosses through `Runtime/archive#HDF_ARCHIVE` ALONE — `HdfArchive.Begin` opens the ONE deferred-write session, and a second `H5File` construction with a direct `Write(Stream)` anywhere in the package is the form the archive catalog names as rejected regardless of dataset scale. The container is create-only, both datasets are single-shot, and each rides its own monotone chunk cursor so an out-of-order ordinal refuses at admission rather than mid-encode.
- Boundary: `[Equatable]` with explicit member equality is load-bearing — `Nodes` and `Connectivity` are `ReadOnlyMemory<T>`, which synthesized record equality compares by REFERENCE, so two identical meshes read unequal and one mesh reloaded reads unequal to itself. Every content key, memo probe, and sweep dedupe over a mesh depends on that equality being structural.
- Boundary: the nodal gather writes into a PER-THREAD scratch buffer, live only until that thread's next gather — the assembly, the metric fold, and the inertia scatter each call it once per cell over a parallel range, so a fresh array per call is one allocation per cell per pass, and every consumer reads the span inside the same cell iteration, which is exactly the contract that makes the buffer reusable.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldStation {
    public static readonly FieldStation Nodal = new("nodal", static m => m.NodeCount);
    public static readonly FieldStation IntegrationPoint = new("integration-point", static m => m.ElementCount * m.Rule.Points.Length);
    public static readonly FieldStation Cell = new("cell", static m => m.ElementCount);
    public static readonly FieldStation Boundary = new("boundary", static m => m.BoundaryCount);

    [UseDelegateFromConstructor]
    public partial long Count(DiscreteMesh mesh);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldRank {
    public static readonly FieldRank Scalar = new("scalar", order: 0, static _ => 1);
    public static readonly FieldRank Vector = new("vector", order: 1, static dim => dim);
    public static readonly FieldRank Tensor = new("tensor", order: 2, static dim => dim * dim);

    public int Order { get; }

    [UseDelegateFromConstructor]
    public partial int Components(int dim);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record FieldSpace(FieldStation Station, FieldRank Rank, int Dim, long Count) {
    public int Components => Rank.Components(Dim);

    public static Fin<FieldSpace> OfKey(DiscreteMesh mesh, string station, string rank, int dim) =>
        (FieldStation.TryGet(station, out FieldStation resolvedStation)
            ? Success<Error, FieldStation>(resolvedStation)
            : Fail<Error, FieldStation>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(station)))),
         FieldRank.TryGet(rank, out FieldRank resolvedRank)
            ? Success<Error, FieldRank>(resolvedRank)
            : Fail<Error, FieldRank>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Key(rank)))))
            .Apply((s, r) => mesh.FieldOf(s, r, dim)).As().ToFin();

    public Validation<Error, ChunkGrid> Layout(int targetChunkElements = FieldPack.ChunkElementTarget) =>
        ChunkGrid.Derive([checked((int)Count)], Components, targetChunkElements);
}

[Equatable]
public sealed partial record DiscreteMesh(
    ElementClass Element,
    MeshAlgorithm Algorithm,
    QuadratureRule Rule,
    [property: OrderedEquality] ReadOnlyMemory<float> Nodes,
    [property: OrderedEquality] ReadOnlyMemory<long> Connectivity,
    long NodeCount,
    long ElementCount,
    long BoundaryCount,
    int BoundaryLayers,
    int RefineLevel,
    CellQuality Metric,
    double WorstQuality,
    Option<double> ErrorEstimate,
    Instant At) {
    public FieldSpace FieldOf(FieldStation station, FieldRank rank, int dim) => new(station, rank, dim, station.Count(this));

    public Fin<Unit> Archive(Stream sink, HdfArchivePolicy policy) =>
        (ChunkGrid.Derive([checked((int)NodeCount)], components: 3, FieldPack.ChunkElementTarget),
         ChunkGrid.Derive([checked((int)ElementCount)], Element.Nodes, FieldPack.ChunkElementTarget))
            .Apply(static (nodes, cells) => (Nodes: nodes, Cells: cells)).As().ToFin()
            .Bind(grids => Try.lift(() => {
                H5Dataset<float[]> nodes = new(grids.Nodes.FileDims.ToArray(), grids.Nodes.Chunk.ToArray(), datasetCreation: policy.Creation());
                H5Dataset<long[]> cells = new(grids.Cells.FileDims.ToArray(), grids.Cells.Chunk.ToArray(), datasetCreation: policy.Creation());
                H5File graph = new() { ["nodes"] = nodes, ["connectivity"] = cells };
                graph.Attributes["element"] = Element.Key;
                graph.Attributes["algorithm"] = Algorithm.Key;
                graph.Attributes["metric"] = Metric.Key;
                graph.Attributes["worst-quality"] = WorstQuality;
                graph.Attributes["refine-level"] = RefineLevel;
                ErrorEstimate.Iter(estimate => graph.Attributes["error-estimate"] = estimate);
                using HdfWriter writer = HdfArchive.Begin(graph, sink, policy);
                return writer.Open(nodes, grids.Nodes).WriteAll(Nodes.ToArray())
                    .Bind(_ => writer.Open(cells, grids.Cells).WriteAll(Connectivity.ToArray()));
            }).Run().Bind(static inner => inner));

    public ReadOnlySpan<float> Coordinates => Nodes.Span;
    public ReadOnlySpan<long> Indices => Connectivity.Span;

    [ThreadStatic]
    static double[]? scratch;

    public ReadOnlySpan<double> NodalXyz(long element) {
        ReadOnlySpan<long> conn = Indices;
        ReadOnlySpan<float> pos = Coordinates;
        int per = Element.Nodes;
        double[] xyz = scratch is { } held && held.Length >= per * 3 ? held : scratch = new double[per * 3];
        for (int v = 0; v < per; v++) {
            long node = conn[(int)(element * per + v)];
            xyz[v * 3] = pos[(int)node * 3]; xyz[v * 3 + 1] = pos[(int)node * 3 + 1]; xyz[v * 3 + 2] = pos[(int)node * 3 + 2];
        }
        return xyz.AsSpan(0, per * 3);
    }
}
```
