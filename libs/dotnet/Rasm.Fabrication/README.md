# [RASM_FABRICATION]

`Rasm.Fabrication` is a production digital-fabrication engine: one polymorphic `Fabrication` owner closes 3D model to verified machine program across subtractive CAM, production additive, true-shape nesting, sheet-stock and tube forming, welding, steel exchange, and controller-family posting. Machine truth is the output bar: every program survives voxel removal verification, on-machine probing, and modal cycle-time simulation before it posts, and the spec plane, shop documentation, and signed quality records retain content identity.

Every manufacturing process folds through a single `FabricationPolicy` dispatch to its canonical domain result. Addressable outputs carry their `EgressKind` content key on that result.

## [01]-[ROUTER]

[PROCESS]:
- [01]-[OWNER](.planning/Process/owner.md): Content keys, the policy family, and the polymorphic `Run` dispatch.
- [02]-[ATOMS](.planning/Process/atoms.md): Acyclic atoms floor — arc-native profile geometry, admitted motion, decoded equipment, and plan carriers.
- [03]-[FAMILY](.planning/Process/family.md): Generated vocabulary floor — `Machine.Admit` equipment generation and `PostDialect` grammar binding.
- [04]-[PHYSICS](.planning/Process/physics.md): State-dependent material laws, coolant-coupled cutting response, and energy budgets.
- [05]-[FAULTS](.planning/Process/faults.md): Direct generated `FabricationFault` union partitioned by owning concern.
- [06]-[DERIVATION](.planning/Process/derivation.md): Aggregate-admitted plan derivation with lot scheduling and critical-path evidence.
- [07]-[TELEMETRY](.planning/Process/telemetry.md): Instruments, spans, hook rail, and descriptor pack.

[TOOLING]:
- [08]-[MAGAZINE](.planning/Tooling/magazine.md): Provider-detached `ToolAssembly` owner — typed-shortfall kitting, reserve-adjusted life schedule.
- [09]-[CUTTINGDATA](.planning/Tooling/cuttingdata.md): Kienzle seeds, chatter-stability recommendation, and the cutter-form projection.
- [10]-[WEAR](.planning/Tooling/wear.md): Flank-wear and condition-based remaining-life estimation over decoded machine telemetry.

[GEOMETRY2D]:
- [11]-[ALGEBRA](.planning/Geometry2D/algebra.md): Line-space operation algebra — offset, clipping, morphology, topology, and field rasters.
- [12]-[ARCS](.planning/Geometry2D/arcs.md): Admitted arc forests, exact arc set operations, engagement motion, and witnessed chord projection.
- [13]-[CURVES](.planning/Geometry2D/curves.md): Free-form curve admission and witnessed lowering under one `CurveAlgebra.Apply`.

[INGRESS]:
- [14]-[PROFILE](.planning/Ingress/profile.md): DXF/DWG census, profile-lane resolution, arc-preserving contour healing, and `Ingress.Admit`.
- [15]-[SOLID](.planning/Ingress/solid.md): STEP/IGES/STL/3DM/3MF unit-resolved admission with conditioning and repair evidence.
- [16]-[STEEL](.planning/Ingress/steel.md): DSTV/NC1 admission into typed steel features and arc-aware contours.
- [17]-[ELEMENT](.planning/Ingress/element.md): `ElementGraph` admission into components, connections, relationships, and facts.

[TOOLPATH]:
- [18]-[MOTION](.planning/Toolpath/motion.md): CAM generator arms over process modality and cut strategy.
- [19]-[SURFACE](.planning/Toolpath/surface.md): Cutter-location surface finishing — waterline, scallop, pencil, rest.
- [20]-[PARTITION](.planning/Toolpath/partition.md): Generative site field to boundary-clipped cells, density closure, and the 3D complex.
- [21]-[GUARD](.planning/Toolpath/guard.md): Planar, medial, voxel, and robot collision verdicts.
- [22]-[SKELETON](.planning/Toolpath/skeleton.md): Constant-engagement walk over the kernel clearance family.
- [23]-[TURNING](.planning/Toolpath/turning.md): Controller-neutral lathe algebra under one `CutSide` row.
- [24]-[WIRE](.planning/Toolpath/wire.md): Wire-EDM demand owner — guide correspondence, wire-bow evidence, simultaneous blocks.
- [25]-[LINK](.planning/Toolpath/link.md): Precedence-safe refined tour over routed transitions with volumetric keepouts.
- [26]-[BEVEL](.planning/Toolpath/bevel.md): Station-varying edge preparation with tilt compensation and coupled THC evidence.

[KINEMATICS]:
- [27]-[CELL](.planning/Kinematics/cell.md): Robot-cell target compilation, batch placement search, and the planner timing census.
- [28]-[MACHINE](.planning/Kinematics/machine.md): Parameterized machine-chain inverse with TCP/RTCP and dynamics-true timing.
- [29]-[FLEET](.planning/Kinematics/fleet.md): Shop registry ranking capability and seating finite capacity over generated availability.
- [30]-[OBSERVATION](.planning/Kinematics/observation.md): Decoded machine-telemetry slice every measured consumer reads.

[ADDITIVE]:
- [31]-[SLICING](.planning/Additive/slicing.md): FFF/DED planar slicing — shells, infill, and the bead-section flow law.
- [32]-[IMPLICIT](.planning/Additive/implicit.md): Implicit-voxel TPMS, lattice, and cellular infill firebreaked from the mesh world.
- [33]-[PRODUCTION](.planning/Additive/production.md): Additive build plan — orientation search, plate placement, layer programs, `3MF` publication.
- [34]-[SCANPATH](.planning/Additive/scanpath.md): LPBF vector planning — one `ScanPolicy` law deriving hatch partitions and source election.
- [35]-[SUPPORT](.planning/Additive/support.md): One support topology every downstream consumer reads — branching, contact, removal-access evidence.

[NESTING]:
- [36]-[NFP](.planning/Nesting/nfp.md): True-shape placement over heterogeneous stock — configuration-space topology, exact arc-space collision.
- [37]-[STOCK](.planning/Nesting/stock.md): Rectangular cutting-stock yield engine over the nesting-strategy union.
- [38]-[REMNANT](.planning/Nesting/remnant.md): Offcut lifecycle over canonical content identity and the generation carried into the next inventory.
- [39]-[LINKING](.planning/Nesting/linking.md): Post-placement cut topology — shared cuts, containment precedence, bridge-gap evidence.

[FIXTURING]:
- [40]-[WORKHOLDING](.planning/Fixturing/workholding.md): Fixture admission over one `WorkholdingKind` row table — datums, lifecycle, restraints.
- [41]-[SETUPS](.planning/Fixturing/setups.md): Setup scheduling with WCS lineage and bounded branch-and-bound optimality evidence.
- [42]-[ASSEMBLY](.planning/Fixturing/assembly.md): `JoinMethod`-collapsed join planning — fit-up, precedence, load-path stability, release.

[POSTING]:
- [43]-[PROGRAM](.planning/Posting/program.md): Dialect-neutral `CutProgram` AST, command vocabulary, RS274 parse, and the posting boundaries.
- [44]-[CONDITIONING](.planning/Posting/conditioning.md): Dimensioned cut, fit, and compensation admission and the source-to-node assembly fold.
- [45]-[DIALECT](.planning/Posting/dialect.md): `CutProgram`-to-`PostImage` emission with block, checksum, and frame lowering.
- [46]-[OPTIMIZATION](.planning/Posting/optimization.md): Admitted recursive optimization with machine-minute evidence and pattern folding.

[VERIFY]:
- [47]-[REMOVAL](.planning/Verify/removal.md): Voxel material-removal verification of gouge, uncut, overcut, and residual measurements.
- [48]-[PROBING](.planning/Verify/probing.md): Post-cycle metrology truth — probe cycles, ICP datum best-fit, conformance verdicts.
- [49]-[SIMULATE](.planning/Verify/simulate.md): Modal-state execution walk — the authoritative cycle-time owner.
- [50]-[ESTIMATION](.planning/Verify/estimation.md): Cost and carbon estimation into parallel signed ledgers.
- [51]-[AUDIT](.planning/Verify/audit.md): Additive layer-stack pre-flight censused by `AuditRisk`.

[SPEC]:
- [52]-[TOLERANCE](.planning/Spec/tolerance.md): GD&T frames, ISO fits, general tolerances, datums, texture, and ranked stackup.
- [53]-[CAPABILITY](.planning/Spec/capability.md): Variable and attribute process capability, MSA, generated SPC, and identity-scoped plan gates.
- [54]-[MANUFACTURABILITY](.planning/Spec/manufacturability.md): Provenance-graded DfM evidence and multi-objective ranked routing.

[DOCUMENTATION]:
- [55]-[PROJECTION](.planning/Documentation/projection.md): Multi-view drafting projection with pose, convention, scale, and characteristic anchors.
- [56]-[TRAVELER](.planning/Documentation/traveler.md): Content-keyed shop-execution document: immutable as-run amendment chain, hold-release gate.
- [57]-[REPORT](.planning/Documentation/report.md): As-built quality records, the evidence census, and shop-schedule deliverables.
- [58]-[PASSPORT](.planning/Documentation/passport.md): Release-gated seal, attestation quorum, and the signed digital product passport.

[FORMING]:
- [59]-[SHEET](.planning/Forming/sheet.md): One unfold owner and the `FormSource` modality family the run spine dispatches forming through.
- [60]-[BRAKE](.planning/Forming/brake.md): Executable press-brake plan from an unfolded pattern to ordered instructions under one admitted policy.
- [61]-[TUBE](.planning/Forming/tube.md): Tube-forming algebra — discrete bending, axis-specific section roll curving, and cope projection.

[JOINING]:
- [62]-[DEPOSITION](.planning/Joining/deposition.md): Standards-as-data derate rows, deposition physics, the bead programme, and the arc-fit gate.
- [63]-[WELD](.planning/Joining/weld.md): Joint-by-prep bead-lattice composition over the typed `JointPrep` groove law under the arc-fit gate.
- [64]-[SEQUENCE](.planning/Joining/sequence.md): Depth-interleaved distortion-control weld ordering under thermal, preload, and release loads.
- [65]-[PROCEDURE](.planning/Joining/procedure.md): Profile-generated WPS/PQR, personnel qualification, inspection scope, and the hold-point plan.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[GEOMETRY_ENGINES]:
- `CavalierContours` — Arc-native bulge-polyline offset and boolean owner.
- `OpenCAMLib` — 3-axis cutter-location engine for surface finishing; vendored over shared `libocl`.
- `RectangleBinPack.CSharp` — Rectangular cutting-stock packer suite and NFP rectangle fast-path.

[EXCHANGE_INGRESS]:
- `DSTV.Net` — DSTV/NC1 steel-fabrication exchange for profile-cut programs.
- `OcctNet.Wrapper` — STEP/IGES B-rep ingress to shape and mesh.

[KINEMATICS]:
- `Robots` — Serial-chain robot kinematics: forward, inverse, and external axes.
- `Rhino3dm` — `extern alias R3` boundary assembly the robot seam copies through; read-side only, document authoring stays host-side.

[ADDITIVE]:
- `PicoGK` — Implicit-voxel kernel for lattice infill and layer rasterization; companion-only.
- `lib3mf` — 3MF reader and writer for core, production, and beam-lattice egress; vendored.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/dotnet/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json` — STJ converters for the generated value objects, smart enums, and unions on every egress.
- `JetBrains.Annotations`
- `System.IO.Hashing` — Reached only through the kernel content-hash mint every egress key seeds from.
- `NodaTime` — Instant stamps on travelers, quality records, probing results, and tool-life schedules.
- `NodaTime.Serialization.SystemTextJson` — STJ codec carrying those instants, intervals, and zones across the content-keyed wire.
- `QuikGraph` — Setup-precedence, assembly, and rapid-link routing graphs, bipartite fixture assignment, and the mesh-shell disjoint-set components.
- `Riok.Mapperly` — Source-generated boundary projections over non-aliased shapes; a shape behind an `extern alias` keeps a hand copyist.
- `Generator.Equals` — Compile-time structural equality and member-level difference results over attributed partial owners.
- `UnitsNet` — Cut-parameter and tolerance quantity boundary.
- `System.Numerics.Tensors` — Sampling folds across the hot toolpath and nesting lanes.
- `CommunityToolkit.HighPerformance` — Grayscale, engagement, and layer-census raster grids.

[NUMERIC_SUBSTRATE]:
- `MathNet.Numerics` — Capability fits, tolerance simulation, and bounded inverse-kinematics conditioning.

[GEOMETRY_INTERCHANGE]:
- `ACadSharp` — DWG/DXF profile-read leg into `Loop` values and markings.

[MESH_PROCESSING]:
- `geometry3Sharp` — Line-sourced `BiArcFit2` biarc fit feeding `G2`/`G3` arc emit.

[PLANAR_GEOSPATIAL]:
- `Clipper2` — Line-space lanes behind the `FillOf` seam; offset, boolean, and morphology lower onto the `Rasm` kernel owners.

[WIRE_CODEGEN]:
- `Rasm.Contracts` — Generated fabrication messages consumed by the feature-control egress, referenced by project.
- `Google.Protobuf` — Official protobuf binary emission of the generated feature-control message.
- `Celly.Protovalidate` — Descriptor-compiled `buf.validate` evaluation at feature-control egress.

[SERVICE_CONTRACTS]:
- `Microsoft.Extensions.Caching.Hybrid` — Solver memo seat behind `HybridCache`; durable L2 federates at the Persistence cache seam.

[OBSERVABILITY]:
- `Microsoft.Extensions.Compliance.Redaction` — Classification attributes on protected evidence members; redactor binding stays at the app root.

[MACHINE_CONNECTIVITY]:
- `MTConnect.NET-Common` — ISO-13399 cutting-tool slice behind the magazine and tool telemetry.

[RUNTIME_INBOX]:
- `System.Diagnostics.Metrics` — In-box owner of the instrument surface.
- `System.Text.Json` — Generated wire contexts and `Utf8JsonWriter` codecs behind the traveler, report, and telemetry egress payloads.
