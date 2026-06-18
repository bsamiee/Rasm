# [CSHARP_BRANCH_TASKLOG]

The cross-package C# work no single folder owns, plus the mature-folder open work that carries no `.planning/` scaffold. Per-folder work lives in each package `TASKLOG.md`; this node carries the cross-folder seam arbitrations, the mature `Rasm` cleanup, and the host-boundary seams the future app root composes. Cross-language amendments (the CRDT wire-vocabulary change, the SDK-codegen consumption) live in `libs/.planning/TASKLOG.md` and are referenced as wire seams, never restated. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed.

## [1]-[OPEN]

[BLOCKED] Capture the two-sided clash-seam golden fixture between the kernel spatial index and the Compute clash compute.
- The node-link acceleration layout is the AGREED frozen contract: `Rasm.Geometry/spatial/index#CLASH_SEAM` `SpatialIndex.ToAcceleration`/`NodeLinkProjection` emits the per-node interleaved AABB plus the `(FirstChild << 21) | ChildCount` descriptor with a leaf primitive-id tail, and `Compute/solver/clash#CLASH_AND_TWIN` `ClashScale.BvhPairs` decodes it by a proper BVH descent over the contiguous `[FirstChild, FirstChild+ChildCount)` child range — the prior flat per-primitive O(N²) decode is the deleted form on both pages.
- No new package; the seam is the `Rasm.Compute.Solver` `AccelerationStructure` union the projection returns; integrate the testkit under `testing-cs`.
- The contract is settled across the kernel/app-platform boundary; the residual is the `[CLASH_GOLDEN]` two-sided frozen golden-bytes fixture — the canonical 8-primitive `BoundingBox[]` set built with `BuildPolicy.Canonical`, serialized once into the shared test corpus, with the Rasm `spatial/index#CLASH_GOLDEN` spec asserting `NodeLinkProjection` emits those bytes and the Compute `solver/clash#CLASH_GOLDEN` spec asserting `BvhPairs` decodes them into the agreed clash-pair set.
- Blocked only on that frozen fixture existing and passing in both packages' test scope; the degradation-keyed refit and agglomerative builder ride the same projection transparently with no layout change.

[BLOCKED] Capture the canonical-adjacency byte-identity golden fixture between the kernel topology hash and the Persistence structural diff.
- The canonical int32-LE adjacency layout is the AGREED frozen contract owned by `Rasm.Geometry/topology/reconciliation#CANONICAL_BYTE_IDENTITY` `NamingHashOps.Encode`; `Persistence/versioning/version-control#STRUCTURAL_DIFF` `GeometryHash`/`StructuralMerge` reads the IDENTICAL layout VERBATIM before `XxHash128.HashToUInt128` and never re-derives a second encoding.
- Integrate System.IO.Hashing (`XxHash128`); CsCheck and the testkit under `testing-cs`.
- The byte order is frozen and the digest pinned; the residual is the shared golden-bytes fixture — the single-triangle reference (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) whose 52-byte stream and `0x9462A71A5DD13DCFA3B1D6D225FCBE70` digest both packages assert, with the morph (equal hash) and topology-break (distinct hash) discriminating laws. The live-host `Mesh.TopologyVertices.IndicesFromFace` boundary-cycle winding is confirmed via `Rasm.Vectors`.
- Blocked only on that frozen golden-bytes fixture existing in both packages' test scope and on confirming `IndicesFromFace` returns consistent winding before the rotation law finalizes.

[QUEUED] Reconcile the mature `Domain/Geometry.cs` owner with the greenfield `Geometry/` robust-core sub-domain.
- The mature `Rasm/Domain/Geometry.cs` geometry-normalization owner and the greenfield `Rasm/Geometry/` robust-core share the name `Geometry`; rename or re-scope so the `Rasm.Geometry.*` robust-core namespaces (`Numerics`/`Spatial`/`Topology`/`Healing`/`Constraints`/`Tessellation`/`Faults`) do not collide with the `Domain` owner.
- No external package; a co-located source rename or namespace re-scope internal to the kernel.
- Internal to `Rasm` — aligns the mature `Domain` source with the greenfield `Geometry` sub-domain tree; no cross-package consumer reads the `Domain` geometry owner by the colliding name.
- Settle the rename before the kernel transcription task creates the `Geometry/` source tree so no two owners claim the name.

[QUEUED] Mature-folder cleanup of the kernel `Vectors`/`Analysis`/`Domain` source.
- Carry the open split, cleanup, and re-architecture work for the three mature kernel sub-domains as task cards here rather than in a `.planning/` scaffold those folders no longer hold: the `Vectors` operator/spectral vocabulary through the `VectorIntent.Project` rail, the `Analysis` measure/query/intersect/topology algebra, and the `Domain` Rhino-normalization/context/stats/validation owners, each against the `Vectors/_ARCHITECTURE.md` co-located source note.
- No new external package; the mature lanes compose the admitted CSparse, System.Numerics.Tensors, and MathNet.Numerics already registered.
- Internal to `Rasm` — the mature siblings are settled source consumed by every stratum above, so a cleanup preserves the public operator vocabulary; the greenfield `Geometry/` robust-core composes `Vectors` as settled value-objects, never re-minting a primitive.
- The mature folders carry no `.planning/`; their design surface is the co-located source architecture note and this card is the only open-work home.

[QUEUED] Compose the host-boundary seams at the future app root.
- The out-of-scope-durable `Rasm.Rhino` and `Rasm.Grasshopper` source own the live-host RhinoCommon/GH2 surfaces (capture, events, commands, exchange, drafting, components); the future app root composes them with the app-platform — the `AppUi/hosts/surface-hosts` NSView embed and host-shared `GRContext` lease, the `Rasm.Rhino` host-attach fact stream feeding the surface-host axis, and the document-transaction commit at the grid edge.
- No new package admission in the host-boundary source; the app root admits the host packages and pins the app-root-only surfaces (OTLP exporter, the MCP HTTP transport, the WASM and industrial-protocol runtimes, Kestrel/gRPC, the Serilog host bridge).
- The host boundaries reference only `Rasm` and enter at the app root, never as an interior dependency of a host-neutral package; `Rasm.AppUi` references the abstract surface seam columns, never a host API, and Rhino owns Make2D/sheet-layout/native file I/O while `Rasm.Bim` independently owns the universal IFC semantics — the two coexist.
- The composition is the final implementation-start gate: the in-host Generic Host boot and cooperative-then-forced drain across all four app packages, the cross-process correlation/tenant threading over the UDS/HTTP2 hop, and the live host-shared GPU lease are the live-host probes the app root resolves, each named on its own folder `TASKLOG.md` as a blocked live-host probe.
- The HLC two-half causal stamp and tenant context the `AppHost/ports/runtime-ports` fan-in mints are exported once on the wire for the peers to read; the cross-runtime reproduction of that causal stamp and tenant frame is the `libs/.planning` causal-tenant-identity seam, never re-minted branch-side.

[QUEUED] Land the capability control plane across the runtime spine, the execution lane, and the UI.
- Build the one self-describing op catalog spanning packages: project each `CapabilityDescriptor` to an MCP tool and an SDK at `AppHost/capability/registry` and `AppHost/agent/mcp-projection`, route the command algebra onto the `Compute/intent/admission` rail, and gate exposure through the `AppUi/commands/commands-availability` algebra.
- Integrate ModelContextProtocol and Microsoft.Extensions.AI.Abstractions (AppHost), composing the Compute proto vocabulary and the AppUi intent table.
- The descriptor source is owned once in AppHost; Compute executes the invocation and AppUi projects availability, each consuming the descriptor as settled vocabulary, never minting a second op-metadata owner. The signed grant attestation chains into the determinism event log over the Persistence op-log at the wire.
- The cross-language SDK-codegen and MCP-agent consumption by the sibling branches is the `libs/.planning` capability-catalog seam, referenced there and never restated; this card lands only the C# concert.

[QUEUED] Land the federated coordination cockpit across BIM semantics, durable annotation, and the UI.
- Compose the openBIM coordination surface: the `Rasm.Bim/coordination` BCF 3.0 topic/component model and GlobalId-stable diff, the `Persistence/annotation` anchor algebra and CDE sync, the `Persistence/federation` entity graph and rule engine, and the `AppUi/coordination` board projection over the viewpoint codec and a CRDT comment thread.
- No new branch-level package; each folder admits its own BCF/IDS/bSDD surface, and the board composes admitted AppUi owners.
- The BCF semantic model is owned once in `Rasm.Bim` and consumed at the boundary; AppUi owns the board projection and never re-mints a BCF schema or writes BCF-XML in the app-platform leaf. The diff joins by GlobalId plus the one content-key, and the round-trip persists through the op-log changefeed.
- The IDS audit and the rule engine ride the one federated graph; the C# Bim owner authors and parses the IDS spec while the Python ifctester companion is the deterministic cross-tool oracle at the `libs/.planning` companion seam.

[QUEUED] Land the deterministic replay observatory across the spine, the op-log, and the notebook.
- Build the reproducibility kernel spanning packages: the `AppHost/determinism/determinism-and-replay` pinned RNG/float-mode/fingerprint and the hash-chained event log over the `Persistence/sync/collaboration` op-log, replay-verify with per-step content-hash proof, and the `AppUi/notebook/notebook-document` capability pin and replay bundle.
- Integrate System.IO.Hashing (the content-address seed) and the BCL cryptography inbox (the detached signature); no new central admission.
- The event log is the single hash-chained command log riding the durable op-log at the Persistence wire, never coupled to its interior; the notebook cell-edit op projects onto the op-log CRDT delta, and the determinism kernel owns the seed and float mode that the cross-machine replay-verify consumes from the Compute provider-determinism fingerprint.
- Cross-machine replay-verify is unsound without the Compute provider-determinism fingerprint; the cross-runtime reproduction of the seed is the `libs/.planning` content-address seam, and the inward re-import of the Python `HandoffAxis` graduation evidence by the one content key is the `libs/.planning` graduation-evidence seam — the event log references the offline result by key, never re-running the Python training loop.

[QUEUED] Branch registry-truth sweep — reconcile stale README registry and design-card prose against the one C# manifest.
- The per-folder README registries and a handful of design-card status markers drifted from manifest reality: AppUi cards on `GPU_BACKEND_PORT`/`T-EXPORT-WRITERS` claim a "new manifest admission" for VelloSharp/ACadSharp/netDxf/OpenXml already admitted (re-ground to "row/factory work remains"); Fabrication `CLIPPER2_API_CATALOGUE [QUEUED]` and `KINEMATICS_SPLIT [QUEUED]` are stale where the catalogue and the serial-chain page already exist (mark `[COMPLETE]`); the Clipper2 `Triangulate` catalogue-vs-prose contradiction re-grounds on the author's "buggy, I regret releasing it" caveat rather than a false "internal demo type"; Persistence README omits the used `Npgsql.NetTopologySuite` entry and AppUi README omits bare `VelloSharp` under `[GPU_BACKEND]`.
- No package change; a README/card prose reconciliation internal to each folder against the one C# manifest, never a per-folder manifest.
- Each folder owns its README and design cards; the sweep aligns prose to the manifest truth at the folder boundary and lands as folder-local edits, never a branch-level registry.
- Prose-only remediation that proposes no capability; reclassified from an idea because it reconciles stale prose rather than opening a surface.

[QUEUED] Branch README marker sweep — trim the universal kernels from the folder README registries.
- The Bim/Materials/Compute/AppUi folder READMEs redundantly list `LanguageExt.Core` and `Thinktecture.Runtime.Extensions` core in their flat package lists; those universal kernels are branch-registered (`README#[2]-[FUNCTIONAL_CORE]`, "Every C# folder") and excluded from catalogues entirely, so they are trimmed from the folder READMEs and each folder lists only its own additions.
- No package change; a folder-README trim internal to each folder.
- The branch README §2 owns the cross-cutting universal-kernel registration; a C# folder README carries no `[CROSS_CUTTING]` section (C# holds no branch `.api/`) and never re-lists the universal kernels.
- Prose-only remediation, no capability proposed; aligns the folder READMEs to the form standard.

## [2]-[CLOSED]

[COMPLETE] Cross-cluster fence-truth reconciliation across the eight folders — the red-team pass that aligned the per-folder fences each folder agent authored against the sibling owners they could not see. Reconciled the content-identity fabric (`XxHash128` `Identity`/`InterchangeIdentity` as the one content address; `XxHash3` `Content` demoted to the fast non-identity tag), the asset digest (`XxHash128` over `System.IO.Hashing`, never `SHA256`), the CRDT op-log amendment (the `version-control#CRDT_WIRE` eight-case `CrdtOpWire` union now mirrored field-for-field on the `sync/collaboration#TS_PROJECTION` wire so `write`/`increment`/`maintain` cross the wire), the `EventLog.ToOpLog` projection (the real `OpLogEntry` constructor on `SyncOpKind.Upsert`, never a phantom `OpLog.Entry`/`SyncOpKind.Insert`), the `OpLogEntry.Codec` spelling (`SnapshotCodec.MessagePackBinary`, never the `SnapshotCodec.Binary` options field), and the artifact-blob index `Interchange` kind row that hosts every Compute interchange artifact.

[COMPLETE] Doc-model migration of the C# branch index docs — rebuilt `ARCHITECTURE.md` as the strata-ordered package roster with the single dependency-direction statement, `README.md` as the branch router plus cross-cutting package registry, and authored `IDEAS.md` as the cross-package concert and this `TASKLOG.md`, with cross-language ideas routed to `libs/.planning/`.
