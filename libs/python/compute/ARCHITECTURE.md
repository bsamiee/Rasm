# [PY_COMPUTE_ARCHITECTURE]

`compute` maps host-free offline scientific evidence outward through one rail: independent numeric-science sub-domains meet at the graduation rail, solve routes return the one `Solve` result, and each graduating axis owner clears the one admission gate, while the numeric substrate every sub-domain admits through carries any backend array. Columnar data and tensor sessions cross their owned seams; geometry retains its canonical results outside the compute graduation axis.

## [01]-[DOMAIN_MAP]

```text
compute/                    # Offline scientific evidence, graduating outward through one rail
├── solvers/                # Unified solve routes plus sensitivity, weak-form assembly, and field readout
│   ├── solve.py            # Method-tagged tuple payloads, the bounded SolveStatus vocabulary, the graduate fold
│   ├── linear.py           # LinearIntent dispatch; the sparse exchange pairs with the C# factor lane
│   ├── nonlinear.py        # NonlinearIntent arms; each solve returns the method-discriminated Solve
│   ├── quadrature.py       # QuadratureIntent arms; composes jit and the Solve graduate fold
│   ├── differential.py     # DifferentialIntent arms; adjoint integration returns the Solve
│   ├── sensitivity.py      # DiffModeTag tagged union entered through differentiate; no per-mode method family
│   ├── mesh.py             # ElementKind and FemForm originate here; CTOR table composed downward, AssembledSystem out
│   └── field.py            # ReadoutKind axis over interpolate/project/resample; consumes solutions, produces none
├── optimization/           # Offline optimization discriminated by problem structure
│   ├── design.py           # Field/Mesh/Density objectives over optimistix minimise under the ImplicitAdjoint default
│   ├── program.py          # ProgramIntent classes; verdicts read the solver SolveStatus
│   └── convex.py           # Cone-family discriminant, the Backend row axis, normalized dual multipliers as proof
├── experiments/            # Study spine, run history, inference, and model assets
│   ├── study.py            # Study spine; DOE frames admit through the published data contract
│   ├── history.py          # Multi-run cohort; Partial resume recomputing indices over the whole response vector
│   ├── inference.py        # Inference owner; Posterior projects onto the graduation rail
│   └── model.py            # ModelAsset owner; Python writes the native GraduationEnvelope law that the C# identity gate admits
├── numerics/               # Numeric substrate every sub-domain admits through
│   ├── array.py            # ArrayPayload admission floor every producer stratum composes
│   ├── jit.py              # JitBackend capture table; experiments and quadrature compose it
│   ├── interval.py         # IntervalOp union over the module-probed Floor rows; Certificate and the width/refuted/vacuous bars
│   ├── quantity.py         # UncertainQuantity; correlated unit-bearing uncertainty on one owner
│   └── statistics.py       # TestIntent hypothesis routes and MLE fit; report keys intent-owned over sample bytes
├── analysis/               # Classical-math evidence producers
│   ├── signal.py           # SignalOp folds; artifacts hands SignalOp shapes across the seam
│   ├── transform.py        # FOURIER_ROUTES row table, native coefficient and roundtrip products
│   ├── symbolic.py         # Block[SymbolicOp] left-fold over ExprForm; terminal provider value paired with its content key
│   └── spatial.py          # Native point-set query products; one-way graduation, no trimesh surface re-owned
└── graduation/             # Multi-domain graduation hub and C# stub codegen
    ├── handoff.py          # Rail mint: HandoffAxis, Graduation, EvidenceScope, ComputeLeg, StageTap, minted once
    ├── codegen.py          # ast-built msgspec stubs and JSON Schema $defs from the CamelCase bundle decode
    └── observability.py    # ComputePoint roster folded onto the runtime registry; cpu-rss-io-switch band and stage stream
```

## [02]-[STRATA]

Strata rank the compute interior; seating rows carry only the law the fence cannot show.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Compute interior import strata
    accDescr: Import strata from the producers down to the graduation foundation, each labeled edge naming one sourced type.
    subgraph S2["S2 PRODUCERS"]
        Analysis[analysis]
        Experiments[experiments]
        Optimization[optimization]
    end
    subgraph S1["S1 NUMERICS + SOLVERS"]
        Numerics[numerics]
        Solvers[solvers]
    end
    subgraph S0["S0 GRADUATION"]
        Codegen[codegen]
        Handoff[handoff]
        Observability[observability]
    end
    Analysis e1@-->|"[IMPORT]: ArrayPayload"| Numerics
    Analysis e2@-->|"[IMPORT]: EvidenceScope"| Handoff
    Experiments e3@-->|"[IMPORT]: JitBackend"| Numerics
    Experiments e4@-->|"[IMPORT]: EvidenceScope"| Handoff
    Optimization e5@-->|"[IMPORT]: SolveStatus"| Solvers
    Optimization e6@-->|"[IMPORT]: EvidenceScope"| Handoff
    Numerics e7@-->|"[IMPORT]: Graduation"| Handoff
    Solvers e8@-->|"[IMPORT]: Graduation"| Handoff
    Codegen e9@-->|"[IMPORT]: EvidenceScope"| Handoff
    Observability e10@-->|"[IMPORT]: EvidenceScope"| Handoff
    Handoff f1@-->|"forbidden: upward import"| S2
```

- S0 `graduation` — mints the outward rail (`HandoffAxis`, `Graduation`, `EvidenceScope`) exactly once and imports no compute sibling.
- S0 `codegen` and `observability` compose `handoff` in-stratum; the hub weave re-enters `observability` through one lazy seam.
- S1 `numerics` + `solvers` — one module-acyclic stratum: `solvers` folds `Solve`/`SolveStatus` onto the rail.
- S1 merged rank stays module-acyclic — `interval` reads `solve`'s `graduate` while `solve` reads `array`'s floor, no module pair looping.
- S1 interleave: `quadrature` composes `jit` and the `solve` `graduate` fold, `interval` that fold, `solve` mounting the `EngineProfile` band.
- S2 `analysis` + `experiments` + `optimization` — the producer stratum no sibling imports.
- S2 producers hold no intra-stratum edge — `design` and `program` stay parallel programs, the floor asymmetry a policy fact, never an import.
- S2→S0 `EvidenceScope` edges skip S1 lawfully — the weave is floor capability every stratum reaches, and no S1 mediation exists to bypass.

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Compute package seam registry
    accDescr: Compute sub-domain owners exchanging graduation evidence, content keys, and the Kernel port with peers.
    subgraph compute[COMPUTE]
        Graduation[Graduation rail]
        Solvers[Solve result]
        Numerics[Numeric substrate]
        Experiments[Study spine]
        Analysis[Analysis producers]
        Optimization[Optimization programs]
    end
    Compute{{Rasm.Compute}}
    Runtime{{python:runtime}}
    Artifacts([python:artifacts])
    Data([python:data])
    Graduation e1@-->|"[GRADUATION]: HandoffAxis"| Compute
    Compute e2@-->|"[GRADUATION]: GraduationEvidence"| Graduation
    Experiments e3@-->|"[WIRE]: GraduationEnvelope"| Compute
    Solvers e4@<-->|"[WIRE]: SparseExchange"| Compute
    Artifacts e6@-->|"[GRADUATION]: HandoffAxis"| Graduation
    Artifacts e7@-->|"[SHAPE]: SignalOp"| Analysis
    Runtime e8@-->|"[CONTENT_KEY]: Parity"| Numerics
    Runtime e9@-->|"[BOUNDARY]: ResourceRef"| Experiments
    Data e10@-->|"[SHAPE]: FrameAdmission"| Experiments
    Experiments e11@-->|"[SHAPE]: Benchmark"| Runtime
    Graduation e12@-->|"[SHAPE]: Fact"| Runtime
    Experiments e13@-->|"[SHAPE]: Fact"| Runtime
    Solvers e14@-->|"[SHAPE]: Fact"| Runtime
    Runtime e15@-->|"[PORT]: Kernel"| Solvers
    Runtime e16@-->|"[PORT]: measured"| Graduation
    Runtime e17@-->|"[PORT]: Hooks"| Graduation
    Runtime e18@-->|"[PORT]: Kernel"| Experiments
    Runtime e19@-->|"[PORT]: Kernel"| Analysis
    Runtime e20@-->|"[PORT]: Kernel"| Optimization
```

`ContentIdentity` keys ride beneath the `Parity` parity seam, and the graduation reverse leg spells the owner's `GraduationEvidence`, landed interior as `EvidenceBundle`. `experiments/model#ENVELOPE` rides the forward model crossing as the `GraduationEnvelope` copy the C# identity gate ingests, `solvers/linear#EXCHANGE` pairs the sparse containers with the C# factor lane, and each collapsed edge stands for every contract at its kind, per-contract wiring on the owning pages.

Every leg admitting, writing, or consuming a countable population records through the runtime `Journal` writer on the `[SHAPE]: Fact` durable half, whose `Ledger` a composition root binds and this package never implements. Producing legs are awaitable by law, so a synchronous entrypoint reaches the plane through its own twin. `Resource.COMPUTE` charges once per dispatch at the resource band's async close, and each `Fact` edge carries the evidence its own fold owns.

## [04]-[INTERNAL]

Independent sub-domains produce nothing outward on their own: every producer runs under the hub evidence-weave span for its `EvidenceScope` row, the graduating axis owners project `Graduation` onto the single rail that crosses outward, and an owner with no `HandoffAxis` case stops at the span by charter.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Compute internal result spine
    accDescr: Numeric-science sub-domains fold solve results and graduating axis evidence onto the single graduation rail crossing outward.
    SolveRoutes[[Solve routes]] e1@-->|"Solve"| SolveResult[Solve result]
    Optimization[[Optimization programs]] e2@-->|"Optimum"| SolveResult
    Optimization e3@-->|"ConvexOptimum"| Handoff[Graduation rail]
    History[[Run history]] e4@-->|"StudyRun"| Study[Study spine]
    Experiments[[Inference and models]] e5@-->|"Posterior"| Handoff
    Experiments e6@-->|"ModelAssetManifest"| Handoff
    Numerics[[Numeric substrate]] e7@-->|"graduate fold"| SolveResult
    SolveResult e8@-->|"Graduation"| Handoff
    Analysis[[Analysis producers]] e9@-->|"terminal value"| Handoff
    Handoff e10@-->|"HandoffAxis"| Out([Outward egress])
```

Sub-domains are independent numeric-science concerns. Each discriminates variation by its own problem structure and composes the numeric substrate rather than re-owning it, so a new route or program class lands as a case on the owning discriminant, never a parallel owner. Per-owner wiring lives on the owning implementation pages.

## [05]-[BOUNDARIES]

- `compute` owns offline evidence on the one rail — no runtime, benchmark authority, substrate selector, tensor session, or product result seats.
- Columnar and labelled-array interchange stays in the `data` branch; `compute` composes its shapes, never re-owning the data interior.
- Columnar and gridded statistical aggregation is the `data` branch gridded/field owner; `numerics/statistics` operates on in-memory samples only.
- Geometry tessellation, registration, and topology stay in the `geometry` branch on their canonical results and runtime observation seam.
- `compute` neither imports geometry nor assigns geometry a `HandoffAxis` case.
- Scope admits classical evidence — a capability lands where its result fits a `HandoffAxis` case or the span; deep-learning authoring fits none.
