# [CSHARP_BRANCH_TASKLOG]

The cross-package C# work no single folder owns, plus the mature-folder open work that carries no `.planning/` scaffold. Per-folder work lives in each package `TASKLOG.md`; this node carries the cross-folder seam arbitrations, the mature `Rasm` cleanup, and the host-boundary seams the future app root composes. Cross-language amendments (the CRDT wire-vocabulary change, the SDK-codegen consumption) live in `libs/.planning/TASKLOG.md` and are referenced as wire seams, never restated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[CAPTURE_TWO_SIDED_CLASH_SEAM]-[QUEUED]: capture the two-sided clash-seam golden fixture between the kernel spatial index and the Compute clash compute.
- Capability: freeze the kernel `NodeLinkProjection` bytes and the Compute `BvhPairs` decode as one two-sided clash fixture.
- Shape: `Rasm.Geometry/Spatial/index#CLASH_SEAM` emits the per-node interleaved AABB, `(FirstChild << 21) | ChildCount` descriptor, and leaf primitive-id tail; `Compute/Solver/clash#CLASH_AND_TWIN` decodes the contiguous child range through the `Rasm.Compute.Solver` `AccelerationStructure` union.
- Unlocks: kernel and Compute specs read one shared corpus fixture, proving the 8-box `CLASH_GOLDEN` stream, `NodeCount == 3`, descriptors `2097154`/`-5`/`-8388613`, identity `Order` tail, and clash pairs `{(0,1),(2,3),(4,5),(6,7)}` without reviving the flat O(N²) decode.
- Anchors: `Spatial/index#CLASH_GOLDEN`, `Compute/Solver/clash#CLASH_GOLDEN`, `BRANCH_TEST_NODE_PROVISIONING`, and the `testing-cs` testkit.

[CAPTURE_CANONICAL_ADJACENCY_BYTE_IDENTITY]-[BLOCKED]: capture the canonical-adjacency byte-identity golden fixture between the kernel topology hash and the Persistence structural diff.
- Capability: freeze the canonical int32-LE adjacency layout as the byte-identity fixture shared by the kernel topology hash and Persistence structural diff.
- Shape: `Rasm.Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` `NamingHashOps.Encode` owns the 52-byte single-triangle stream; `Persistence/Version/diff#STRUCTURAL_DIFF` `GeometryHash` and `StructuralMerge` read it verbatim before `XxHash128.HashToUInt128`.
- Unlocks: both packages assert the same `VertexCount=3`, edges `(0,1),(0,2),(1,2)`, face cycle `[0,1,2]`, digest `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, morph equal-hash law, and topology-break distinct-hash law.
- Anchors: `System.IO.Hashing`, `CsCheck`, `testing-cs`, the Rhino 9 WIP `Mesh.TopologyVertices.IndicesFromFace` CCW probe, and the shared C# corpus node.
- Tension: blocked only on the frozen golden-bytes fixture landing in both packages' test scope; the winding, digest, rotation law, and bytes are confirmed and freeze-ready.

[LAND_CAPABILITY_CONTROL_PLANE_RUNTIME]-[QUEUED]: land the capability control plane across the runtime spine, the execution lane, and the UI.
- Capability: build one self-describing operation catalog across AppHost, Compute, and AppUi.
- Shape: `AppHost/Agent/capability` owns each `CapabilityDescriptor`, projects it to MCP tools and SDK codegen through `AppHost/Agent/mcp`, routes command algebra to `Compute/Runtime/admission`, and gates exposure through `AppUi/Shell/commands`.
- Unlocks: Compute executes invocations, AppUi projects availability, sibling branches consume SDK-codegen and MCP-agent seams from `libs/.planning`, and signed grant attestations chain into the determinism event log over the Persistence op-log.
- Anchors: ModelContextProtocol, `Microsoft.Extensions.AI.Abstractions`, the Compute proto vocabulary, the AppUi intent table, and the `libs/.planning` capability-catalog seam.

[LAND_FEDERATED_COORDINATION_COCKPIT_BIM]-[QUEUED]: land the federated coordination cockpit across BIM semantics, durable annotation, and the UI.
- Capability: compose BIM coordination, durable annotation, federated rules, and the UI board into one openBIM cockpit.
- Shape: `Rasm.Bim/coordination` owns BCF 3.0 topic/component semantics and GlobalId-stable diff; `Persistence/annotation` owns anchors and CDE sync; `Persistence/federation` owns the entity graph and rule engine; `AppUi/coordination` owns the viewpoint board and CRDT comment projection.
- Unlocks: IDS audit and rule evaluation ride the federated graph, the diff joins by GlobalId plus content key, and round-trip comments persist through the op-log changefeed without a second BCF schema or BCF-XML writer in AppUi.
- Anchors: BCF, IDS, bSDD, the Python ifctester companion oracle, and the `libs/.planning` companion seam.

[LAND_DETERMINISTIC_REPLAY_OBSERVATORY_SPINE]-[QUEUED]: land the deterministic replay observatory across the spine, the op-log, and the notebook.
- Capability: build the deterministic replay observatory across AppHost, Persistence, Compute, AppUi, and the Python graduation seam.
- Shape: `AppHost/Runtime/determinism` owns pinned RNG, float mode, environment fingerprint, and the hash-chained event log over `Persistence/Version/ledger#CHANGEFEED`; replay-verify checks per-step content hashes; `AppUi/Editing/notebook` pins capabilities and exports replay bundles.
- Unlocks: notebook cell edits project onto the op-log CRDT delta, detached signatures bind command history, cross-runtime seed reproduction stays in `libs/.planning`, and Python `HandoffAxis` graduation evidence re-imports by content key.
- Anchors: `System.IO.Hashing`, BCL cryptography, the Compute provider-determinism fingerprint, the content-address seam, and the graduation-evidence seam.
- Tension: cross-machine replay-verify remains unsound until the Compute provider-determinism fingerprint lands.

[SYMBOLIC_PARAMETRIC_ALGEBRA]-[QUEUED]: stand up the `Rasm.Compute/Symbolic/` sub-domain and register the orphaned CAS manifest rows.
- Capability: create the `Rasm.Compute/Symbolic/expression` owner for symbolic formulas over the admitted CAS stack.
- Shape: `SymbolicExpr` wraps MathNet.Symbolics `Expression`, `Calculus.differentiate`, `Compile.compileExpression`, `Infix`, `Rational`, and `Algebraic` simplify with FParsec parsing and an `XxHash128` canonical-normal-form key; symbolic Jacobians feed `Tensor/quadrature#INTEGRATOR_TABLEAU` and `Solver/optimizer`, dimensions feed `Symbolic/units`, and results graduate through the cross-libs `symbolic` `HandoffAxis`.
- Unlocks: Persistence cost formulas, QTO formulas, and Materials parametric constitutive laws consume one parsed expression contract instead of string `eval`, hand-bound lambdas, or a second expression model.
- Anchors: `MathNet.Symbolics`, `MathNet.Numerics.FSharp`, `FParsec`, transitive `FSharp.Core`, `Rasm.Compute/README.md#[SYMBOLIC]`, `Rasm.Compute/.api/api-mathnet-symbolics.md`, and the catalogue's two `[4]-[RESEARCH]` flags.
- Tension: close the compile arity, `Func<>` return shape, and `SymbolicExpression` operator/conversion/`.Variables` research before transcribing any compile or case-matching fence.

[SYMBOLIC_FORMULA_CONSUMERS]-[QUEUED]: realize symbolic-formula consumers across Persistence and Materials.
- Capability: thread `SymbolicExpr` into Persistence and Materials formula consumers.
- Shape: `Rasm.Persistence/Store/profiles#COST_CATALOG`, `Rasm.Persistence/Query/federation#RULE_PLAN`, and `Rasm.Materials/physical-properties#PROPERTY_SETS` parse and dimension-check formulas once at admission, then cache compiled delegates by content key.
- Unlocks: cost rollups, QTO derived quantities, and parametric temperature/strain constitutive curves use `SymbolicExpr.Parse`, `Compile`, and `Differentiate` instead of runtime string eval, hand-bound lambdas, or frozen constants.
- Anchors: `Rasm.Compute/Symbolic/expression#SYMBOLIC_EXPR`, the AEC-domain/app-platform wire boundary, and the no-second-parser drift rule.

[REALITY_CAPTURE_TO_BIM]-[QUEUED]: open the `Rasm.Bim/reconstruction/` sub-domain for scan-to-BIM primitive fitting.
- Capability: open `Rasm.Bim/reconstruction` as the scan-to-BIM primitive-fitting owner.
- Shape: `ReconstructionPrimitive` folds plane, cylinder, torus, and freeform fits from kernel segmented clouds into `BimElement` rows with `ElementPredicate`-classified `IfcClass` and `properties#PROPERTY_SETS` confidence bands.
- Unlocks: captured splat/point payloads become BIM semantics through one reconstruction owner, while Persistence joins source-cloud lineage by `(GeometryHash, content-key)` and AppUi replays reality capture over the reconstructed model.
- Anchors: `Rasm/Vectors` `Align` and cloud-ICP, `Rasm/Geometry/spatial`, `Rasm.Compute/Runtime/codecs`, `Model/query#ELEMENT_SET`, `libs/.planning` graduation `reconstructed-mesh` and `topology-graph` geometry axes, and `Rasm.AppUi/Render/reality`.
- Tension: C# re-imports offline RANSAC or learned-segmentation results; it does not host an in-process learned segmenter.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
