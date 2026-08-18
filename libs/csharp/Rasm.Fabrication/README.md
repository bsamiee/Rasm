# [RASM_FABRICATION]

`Rasm.Fabrication` is a production digital-fabrication engine: one polymorphic `Fabrication` owner closes 3D model to verified machine program across subtractive CAM, production additive, true-shape nesting, sheet and tube forming, welding, steel exchange, and controller-family posting. Machine truth is the output bar — every program survives voxel removal verification, on-machine probing, and modal cycle-time simulation before it posts — and the spec plane, shop documentation, and signed quality records ride the same content-keyed spine.

Every manufacturing process folds through a single `FabricationPolicy` dispatch to a content-keyed artifact minted on the `EgressKind` egress spine, over the `Rasm` geometry kernel and the `Rasm.Element` seam.

## [01]-[ROUTER]

[PROCESS]:
- [01]-[OWNER](.planning/Process/owner.md): Fabrication entry owner — atoms vocabulary and the polymorphic `Run` dispatch.
- [02]-[FAMILY](.planning/Process/family.md): Process and machine axis families discriminating every generator arm.
- [03]-[PHYSICS](.planning/Process/physics.md): State-dependent material laws, coolant-coupled cutting response, and energy budgets.
- [04]-[FAULTS](.planning/Process/faults.md): `FabricationFault` typed-rejection registry partitioned by owning concern.
- [05]-[DERIVATION](.planning/Process/derivation.md): Aggregate-admitted plan derivation with lot scheduling and critical-path evidence.
- [06]-[TELEMETRY](.planning/Process/telemetry.md): `FabricationFact` union, instrument roster, projection fan, span scopes, descriptor pack.

[TOOLING]:
- [07]-[MAGAZINE](.planning/Tooling/magazine.md): ISO-13399 tool-assembly magazine and the minimal-swap tool-life schedule.
- [08]-[CUTTINGDATA](.planning/Tooling/cuttingdata.md): Kienzle seeds, chatter-stability recommendation, and the cutter-form projection.
- [09]-[WEAR](.planning/Tooling/wear.md): Flank-wear and condition-based remaining-life estimation over decoded machine telemetry.

[GEOMETRY2D]:
- [10]-[ALGEBRA](.planning/Geometry2D/algebra.md): Line-space operation algebra — offset, clipping, morphology, topology, and field planes.
- [11]-[ARCS](.planning/Geometry2D/arcs.md): Arc-space kerf, lead, and adaptive bulge-polyline offsets.
- [12]-[CURVES](.planning/Geometry2D/curves.md): Parametric-curve substrate feeding turning, pockets, posting stations, and 5-axis smoothing.

[INGRESS]:
- [13]-[PROFILE](.planning/Ingress/profile.md): DXF/DWG census, lane resolution, arc-preserving contour healing, and the `Ingress.Admit` fold.
- [14]-[SOLID](.planning/Ingress/solid.md): STEP/IGES/STL/3DM/3MF unit-resolved admission with conditioning and repair evidence.
- [15]-[STEEL](.planning/Ingress/steel.md): DSTV/NC1 admission into typed steel features and arc-aware contours.
- [16]-[ELEMENT](.planning/Ingress/element.md): `ElementGraph` admission into component, connection, relationship, and fact receipts.

[TOOLPATH]:
- [17]-[MOTION](.planning/Toolpath/motion.md): CAM generator arms over process modality and cut strategy.
- [18]-[SURFACE](.planning/Toolpath/surface.md): Cutter-location surface finishing — waterline, scallop, pencil, rest.
- [19]-[PARTITION](.planning/Toolpath/partition.md): Generative site field to boundary-clipped cells, density closure, and the 3D complex.
- [20]-[GUARD](.planning/Toolpath/guard.md): Scope-stamped planar, medial, voxel, and robot collision receipts.
- [21]-[SKELETON](.planning/Toolpath/skeleton.md): Constant-engagement walk over the kernel clearance family.
- [22]-[TURNING](.planning/Toolpath/turning.md): Controller-neutral lathe algebra under one `CutSide` row.
- [23]-[WIRE](.planning/Toolpath/wire.md): Wire-EDM demand owner — guide correspondence, wire-bow evidence, simultaneous blocks.
- [24]-[LINK](.planning/Toolpath/link.md): Precedence-safe refined tour over routed transitions with volumetric keepouts.
- [25]-[BEVEL](.planning/Toolpath/bevel.md): Station-varying edge preparation with tilt compensation and coupled THC evidence.

[KINEMATICS]:
- [26]-[CELL](.planning/Kinematics/cell.md): Robot-cell target compilation, batch placement search, and the planner timing census.
- [27]-[MACHINE](.planning/Kinematics/machine.md): Parameterized machine-chain inverse with TCP/RTCP and dynamics-true timing.
- [28]-[FLEET](.planning/Kinematics/fleet.md): Shop registry ranking capability and seating finite capacity over generated availability.
- [29]-[OBSERVATION](.planning/Kinematics/observation.md): Decoded machine-telemetry slice every measured consumer reads.

[ADDITIVE]:
- [30]-[SLICING](.planning/Additive/slicing.md): FFF/DED planar slicing — shells, infill, and the bead-section flow law.
- [31]-[IMPLICIT](.planning/Additive/implicit.md): Implicit-voxel TPMS, lattice, and cellular infill firebreaked from the mesh world.
- [32]-[PRODUCTION](.planning/Additive/production.md): Build orientation, machine profiles, and 3MF egress.
- [33]-[SCANPATH](.planning/Additive/scanpath.md): LPBF hatch strategies emitting the scan-vector egress.
- [34]-[SUPPORT](.planning/Additive/support.md): Overhang census, tree accumulation, and interface carve.

[NESTING]:
- [35]-[NFP](.planning/Nesting/nfp.md): True-shape NFP-feasibility nesting over stock inventory with the rectangle fast-path.
- [36]-[STOCK](.planning/Nesting/stock.md): Rectangular cutting-stock yield engine over the nesting-strategy union.
- [37]-[REMNANT](.planning/Nesting/remnant.md): Offcut lifecycle — reconcile, claim, release, and re-mint.
- [38]-[LINKING](.planning/Nesting/linking.md): Collision-checked cut linking — common-line, chain-cut, bridge, skeleton.

[FIXTURING]:
- [39]-[WORKHOLDING](.planning/Fixturing/workholding.md): Clamp and exclusion-zone planning with contact-wrench closure rank.
- [40]-[SETUPS](.planning/Fixturing/setups.md): Setup scheduling with WCS lineage and bounded branch-and-bound optimality evidence.
- [41]-[ASSEMBLY](.planning/Fixturing/assembly.md): Join-precedence planning with a `JoinProgram` lifecycle discriminant.

[POSTING]:
- [42]-[PROGRAM](.planning/Posting/program.md): Dialect-neutral `CutProgram` AST with modal interpretation and cut conditioning.
- [43]-[DIALECT](.planning/Posting/dialect.md): `CutProgram`-to-`PostImage` emission with block, checksum, and frame lowering.
- [44]-[OPTIMIZATION](.planning/Posting/optimization.md): Admitted recursive optimization with machine-minute evidence and pattern folding.

[VERIFY]:
- [45]-[REMOVAL](.planning/Verify/removal.md): Voxel material-removal verify into gouge, uncut, overcut, and residual receipts.
- [46]-[PROBING](.planning/Verify/probing.md): In-process metrology — probe cycles, ICP datum best-fit, conformance verdicts.
- [47]-[SIMULATE](.planning/Verify/simulate.md): Modal-state execution walk — the authoritative cycle-time owner.
- [48]-[ESTIMATION](.planning/Verify/estimation.md): Cost and carbon estimation into parallel signed ledgers.
- [49]-[AUDIT](.planning/Verify/audit.md): Additive layer-stack pre-flight censused by `AuditRisk`.

[SPEC]:
- [50]-[TOLERANCE](.planning/Spec/tolerance.md): GD&T frames, ISO fits, general tolerances, datums, texture, and ranked stackup.
- [51]-[CAPABILITY](.planning/Spec/capability.md): Variable and attribute capability, MSA, generated SPC, and identity-scoped plan gates.
- [52]-[MANUFACTURABILITY](.planning/Spec/manufacturability.md): Provenance-graded DfM evidence and multi-objective ranked routing.

[DOCUMENTATION]:
- [53]-[PROJECTION](.planning/Documentation/projection.md): Multi-view drafting projection with pose, convention, scale, and characteristic anchors.
- [54]-[TRAVELER](.planning/Documentation/traveler.md): Content-keyed shop-execution document: immutable as-run amendment chain, hold-release gate.
- [55]-[REPORT](.planning/Documentation/report.md): Signed as-built quality records, shop-schedule deliverables, and the attested passport.

[FORMING]:
- [56]-[SHEET](.planning/Forming/sheet.md): One unfold owner — bend-allowance flat patterning over the kernel development.
- [57]-[BRAKE](.planning/Forming/brake.md): Best-first bend-sequence planning over the reach, collision, and occlusion matrix.
- [58]-[TUBE](.planning/Forming/tube.md): Tube centerline folding, elongation carry, and analytic cope development.

[JOINING]:
- [59]-[WELD](.planning/Joining/weld.md): Joint-by-prep bead-lattice composition over the typed `JointPrep` groove law under an arc-fit gate.
- [60]-[SEQUENCE](.planning/Joining/sequence.md): Depth-interleaved distortion-control weld ordering under thermal, preload, and release loads.
- [61]-[PROCEDURE](.planning/Joining/procedure.md): Profile-generated WPS/PQR, personnel qualification, inspection scope, and the hold-point plan.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[GEOMETRY_ENGINES]:
- `CavalierContours` — arc-native bulge-polyline offset and boolean owner.
- `OpenCAMLib` — 3-axis cutter-location engine for surface finishing; vendored over shared `libocl`.
- `RectangleBinPack.CSharp` — rectangular cutting-stock packer suite and NFP rectangle fast-path.

[EXCHANGE_INGRESS]:
- `DSTV.Net` — DSTV/NC1 steel-fabrication exchange for profile-cut programs.
- `OcctNet.Wrapper` — STEP/IGES B-rep ingress to shape and mesh.

[KINEMATICS]:
- `Robots` — serial-chain robot kinematics: forward, inverse, and external axes.
- `Rhino3dm` — `extern alias R3` boundary assembly the robot seam copies through; read-side only, document authoring stays host-side.

[ADDITIVE]:
- `PicoGK` — implicit-voxel kernel for lattice infill and layer rasterization; companion-only.
- `lib3mf` — 3MF reader and writer for core, production, and beam-lattice egress; vendored.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json` — STJ converters for the generated value objects, smart enums, and unions on every egress.
- `JetBrains.Annotations`
- `System.IO.Hashing` — reached only through the kernel content-hash mint every egress key seeds from.
- `NodaTime` — instant stamps on travelers, quality records, probing receipts, and tool-life schedules.
- `NodaTime.Serialization.SystemTextJson` — STJ codec carrying those instants, intervals, and zones across the content-keyed wire.
- `QuikGraph` — setup-precedence, assembly, and rapid-link routing graphs, bipartite fixture assignment, and the mesh-shell disjoint-set partition.
- `Riok.Mapperly` — source-generated boundary projections over non-aliased shapes; a shape behind an `extern alias` keeps a hand copyist.
- `Generator.Equals` — compile-time structural equality and member-level difference receipts over attributed partial owners.

[NUMERIC_SUBSTRATE]:
- `UnitsNet` — cut-parameter and tolerance quantity boundary.
- `MathNet.Numerics` — capability distribution fits and Monte-Carlo tolerance stackup.
- `System.Numerics.Tensors` — SIMD-lowered sampling folds across the hot toolpath and nesting lanes.
- `CommunityToolkit.HighPerformance` — 2D span grids for grayscale, engagement, and layer-census rasters.

[PLANAR_GEOMETRY]:
- `Clipper2` — line-space lanes behind the `FillOf` seam; offset, boolean, and morphology lower onto the `Rasm` kernel owners.
- `geometry3Sharp` — line-sourced `BiArcFit2` biarc fit feeding `G2`/`G3` arc emit.

[EXCHANGE_SUBSTRATE]:
- `ACadSharp` — DWG/DXF profile-read leg into `Loop` values and markings; Bim holds the mesh read, AppUi the drafting write.
- `MTConnect.NET-Common` — ISO-13399 cutting-tool partition behind the magazine and tool telemetry.

[HOST_SERVICES]:
- `Microsoft.Extensions.Caching.Hybrid` — solver memo tier behind `HybridCache`; durable L2 federates at the Persistence cache seam.
- `Microsoft.Extensions.Compliance.Redaction` — classification attributes on classified receipt members; redactor binding stays at the app root.

[RUNTIME_INBOX]:
- `System.Diagnostics.Metrics` — in-box owner of the instrument surface.
- `System.Diagnostics.ActivitySource` — in-box owner of the engine spans.
- `System.Text.Json` — generated wire contexts and `Utf8JsonWriter` codecs behind the traveler, report, and telemetry egress payloads.
