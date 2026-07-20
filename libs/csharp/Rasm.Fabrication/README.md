# [RASM_FABRICATION]

`Rasm.Fabrication` is a production digital-fabrication engine: one polymorphic `Fabrication` owner closes 3D model to verified machine program across subtractive CNC/CAM at full depth — 2.5D through multi-axis surface finishing, rest machining, engagement control — additive manufacturing at production depth — planar and implicit lanes, support structures, orientation, lattice infill, 3MF hand-off — true-shape nesting and cutting-stock yield, sheet and tube forming, welding, steel NC1 exchange, and post-processing at controller-family breadth. Its output bar is machine truth, not toolpath sketching: every program survives voxel material-removal verification, on-machine probing, and modal cycle-time simulation before it posts, and the spec plane — GD&T vocabulary, process capability, setup sheets, shop travelers, as-built quality records — rides the same content-keyed spine. Its posting plane feeds real shop controllers, its verify plane gates real material, and every artifact lands machine-consumable.

Every manufacturing process folds through a single `FabricationPolicy` dispatch to a content-keyed artifact minted on the `EgressKind` egress spine, over the `Rasm` geometry kernel and the `Rasm.Element` seam. It references no AEC peer — alignment travels through seam contracts and the content-keyed wire.

## [01]-[ROUTER]

[PROCESS]:
- [01]-[OWNER](.planning/Process/owner.md): Fabrication entry owner — atoms vocabulary and the polymorphic `Run` dispatch every flagship enters.
- [02]-[FAMILY](.planning/Process/family.md): Process and machine axis families that discriminate every downstream generator arm.
- [03]-[PHYSICS](.planning/Process/physics.md): State-dependent material laws over a six-axis constitutive state, coolant-coupled cutting response, equipment evidence, and typed energy-closure budgets.
- [04]-[FAULTS](.planning/Process/faults.md): `FabricationFault` partitions typed rejections by owning concern through one witness-admission fold.
- [05]-[DERIVATION](.planning/Process/derivation.md): Aggregate-admitted plan derivation with proven operation topology, lap-phased lot scheduling, critical-path evidence, and typed element projection.

[TOOLING]:
- [06]-[MAGAZINE](.planning/Tooling/magazine.md): ISO-13399 tool-assembly magazine and the minimal-swap tool-life schedule posting consumes.
- [07]-[CUTTINGDATA](.planning/Tooling/cuttingdata.md): Kienzle machinability seeds and the cutter-form projection feeds and speeds resolve through.
- [08]-[WEAR](.planning/Tooling/wear.md): Flank-wear and condition-based remaining-life estimation over decoded machine telemetry.

[GEOMETRY2D]:
- [09]-[ALGEBRA](.planning/Geometry2D/algebra.md): Line-space operation algebra — offset, Boolean and open clipping, windowing, hygiene, morphology, measurement, containment, topology, and sampled field planes.
- [10]-[ARCS](.planning/Geometry2D/arcs.md): Arc-space owner — kerf, lead, and adaptive bulge-polyline offsets orthogonal to the line lane.
- [11]-[CURVES](.planning/Geometry2D/curves.md): Parametric-curve substrate feeding turning, pocket profiles, posting stations, and 5-axis smoothing.

[INGRESS]:
- [12]-[PROFILE](.planning/Ingress/profile.md): DXF/DWG census, fabrication-lane resolution, OCS-correct arc-preserving contour healing, MINSERT expansion, region nesting, and projected receipt ingress.
- [13]-[SOLID](.planning/Ingress/solid.md): STEP/IGES/STL/3DM/3MF unit-resolved admission with conditioning, topology evidence, extension probing, transform composition, and kernel repair evidence.
- [14]-[STEEL](.planning/Ingress/steel.md): DSTV/NC1 path, text, or byte admission into typed steel features, closed face and profile vocabularies, arc-aware contours, and part-space placement.
- [15]-[ELEMENT](.planning/Ingress/element.md): Single or accumulating batch `ElementGraph` admission into component, connection, relationship, typed-fact, and canonical-property receipts.

[TOOLPATH]:
- [16]-[MOTION](.planning/Toolpath/motion.md): CAM generator — process-modality and cut-strategy arms emitting the conditioned motion.
- [17]-[SURFACE](.planning/Toolpath/surface.md): Cutter-location surface finishing over on-mesh path layout — waterline, scallop, pencil, rest.
- [18]-[PARTITION](.planning/Toolpath/partition.md): Seeded Voronoi cells retaining border, centroid, perimeter, boundary, and Lloyd-residual evidence.
- [19]-[GUARD](.planning/Toolpath/guard.md): Scope-stamped planar, medial, voxel, and robot collision receipts over committed motion.
- [20]-[SKELETON](.planning/Toolpath/skeleton.md): Per-component constant-engagement walk over the kernel clearance family — `WalkStrategy` rows against an `EngagementLimit` ceiling fold.
- [21]-[TURNING](.planning/Toolpath/turning.md): Controller-neutral lathe algebra — one `CutSide` row owning internal and external sweeps, plunges, axial cycles, threads, knurling, transfer, and channel evidence.
- [22]-[WIRE](.planning/Toolpath/wire.md): Wire-EDM demand owner — schedule context, access, retention, recovery, registered guide correspondence, wire-bow evidence, and simultaneous process blocks.
- [23]-[LINK](.planning/Toolpath/link.md): Precedence-aware transition owner — generated entries, tool and work-offset objective terms, volumetric keepouts, closed-tour clearance routing, and guarded segment receipts.
- [24]-[BEVEL](.planning/Toolpath/bevel.md): Station-varying edge-preparation owner — generated or custom sections, thermal and abrasive tilt compensation, and coupled THC pass evidence.

[KINEMATICS]:
- [25]-[CELL](.planning/Kinematics/cell.md): Robot-cell target compilation, controller and library boundaries, and batch-kinematics placement search over one loaded cell.
- [26]-[MACHINE](.planning/Kinematics/machine.md): Parameterized Cartesian, serial, delta, and coordinated inverse with TCP/RTCP and dynamics-true timing.
- [27]-[FLEET](.planning/Kinematics/fleet.md): Evidence-retaining shop registry ranking component capability over stations, tooling state, measured performance, and the generated shift calendar every schedule advances through.

[ADDITIVE]:
- [28]-[SLICING](.planning/Additive/slicing.md): FFF/DED planar slicing — shells, bridges, gap fill, infill, and the bead-section flow law over the kernel slice stack.
- [29]-[IMPLICIT](.planning/Additive/implicit.md): Implicit-voxel lane — TPMS, lattice, and cellular infill firebreaked from the mesh world.
- [30]-[PRODUCTION](.planning/Additive/production.md): Build orientation, machine profiles, and 3MF core, production, and beam-lattice egress.
- [31]-[SCANPATH](.planning/Additive/scanpath.md): LPBF hatch strategies and rotation recurrence emitting the scan-vector egress.
- [32]-[SUPPORT](.planning/Additive/support.md): Overhang census, tree accumulation, and interface carve for the printed part.

[NESTING]:
- [33]-[NFP](.planning/Nesting/nfp.md): True-shape NFP-feasibility nesting over stock inventory with the rectangle fast-path.
- [34]-[STOCK](.planning/Nesting/stock.md): Rectangular cutting-stock yield engine over the nesting-strategy union.
- [35]-[REMNANT](.planning/Nesting/remnant.md): Offcut lifecycle — reconcile, claim, release, and re-mint into the next inventory.
- [36]-[LINKING](.planning/Nesting/linking.md): Collision-checked cut linking — common-line, chain-cut, bridge, and skeleton edits.

[FIXTURING]:
- [37]-[WORKHOLDING](.planning/Fixturing/workholding.md): Clamp and exclusion-zone planning with friction-cone contact-wrench closure rank and the folder fault-witness family.
- [38]-[SETUPS](.planning/Fixturing/setups.md): Multi-fixture setup scheduling with setup-to-WCS lineage, a bounded branch-and-bound search, and proven optimality-gap evidence.
- [39]-[ASSEMBLY](.planning/Fixturing/assembly.md): Join-precedence planning with a `JoinProgram` lifecycle discriminant and per-phase duration model.

[POSTING]:
- [40]-[PROGRAM](.planning/Posting/program.md): Dialect-neutral `CutProgram` AST, evidence-preserving RS274/NC1 ingress, modal interpretation carrying plane and arc evidence, cut conditioning, rendered-record receipts, and parameterized toolpath projection.
- [41]-[DIALECT](.planning/Posting/dialect.md): `CutProgram`-to-`PostImage` emission with block, sequence, checksum, and coordinate-frame lowering.
- [42]-[OPTIMIZATION](.planning/Posting/optimization.md): Admitted recursive optimization with accel-limited machine-minute evidence and subprogram pattern folding.

[VERIFY]:
- [43]-[REMOVAL](.planning/Verify/removal.md): Voxel material-removal verify producing gouge, uncut, overcut, and residual-stock receipts.
- [44]-[PROBING](.planning/Verify/probing.md): In-process metrology — probe cycles, ICP datum best-fit, and conformance verdicts.
- [45]-[SIMULATE](.planning/Verify/simulate.md): Modal-state execution walk over the parsed program — authoritative cycle-time owner.
- [46]-[ESTIMATION](.planning/Verify/estimation.md): Cost and carbon estimation folding clock, waste, wear, remnant credit, fleet rates, and the planned promise interval into parallel signed ledgers.
- [47]-[AUDIT](.planning/Verify/audit.md): Additive layer-stack pre-flight over component lineage, void escape, thin-wall, bound, thermal, and recoater risk censused by `AuditRisk`.

[SPEC]:
- [48]-[TOLERANCE](.planning/Spec/tolerance.md): Typed production-specification vocabulary — GD&T frames and boundaries, ISO fits and size limits, general tolerances, datums, texture, and ranked stackup.
- [49]-[CAPABILITY](.planning/Spec/capability.md): Variable and attribute capability by moment and percentile method, MSA, generated SPC, correlated stackup, and identity-scoped plan gates.
- [50]-[MANUFACTURABILITY](.planning/Spec/manufacturability.md): Provenance-graded cross-modality DfM evidence, remediation, and multi-objective ranked process routing.

[DOCUMENTATION]:
- [51]-[PROJECTION](.planning/Documentation/projection.md): Multi-view drafting projection — hidden-line, silhouette, outline, and section runs with view pose, sheet convention, scale, and characteristic anchors in one receipt.
- [52]-[TRAVELER](.planning/Documentation/traveler.md): Content-keyed shop-execution document over the typed receipt corpus, canonical artifact descriptor, passport lineage, and an immutable as-run amendment chain.
- [53]-[REPORT](.planning/Documentation/report.md): Signed as-built quality records and passport artifacts over sampled inspection, material, process, nonconformance, calibration, declaration, genealogy, and sustainability evidence.

[FORMING]:
- [54]-[SHEET](.planning/Forming/sheet.md): One unfold owner — bend-allowance flat patterning over the kernel development.
- [55]-[BRAKE](.planning/Forming/brake.md): Best-first bend-sequence planning over the reach, collision, and occlusion matrix.
- [56]-[TUBE](.planning/Forming/tube.md): Tube centerline folding, elongation carry, and analytic cope development.

[JOINING]:
- [57]-[WELD](.planning/Joining/weld.md): Joint-by-prep bead-stack composition over the typed `JointPrep` groove law.
- [58]-[SEQUENCE](.planning/Joining/sequence.md): Depth-interleaved distortion-control weld ordering — parameter-generated candidates, staged joint actions, interpass thermal state, robot timing, and restraint-weighted inherent-strain evidence.
- [59]-[PROCEDURE](.planning/Joining/procedure.md): Profile-generated WPS/PQR and personnel qualification keyed on `VariableKey` — dimensional compliance, welder continuity, and derived inspection-scope receipts.

## [02]-[DOMAIN_PACKAGES]

Fabrication-specific libraries admitted by this folder; versions centralize in the C# manifest and corroborate against this folder's `.api/`. Vendored natives ride `vendor/runtimes/` outside NuGet restore.

[GEOMETRY_ENGINES]:
- `Clipper2` — line-space offset and boolean-clip substrate.
- `CavalierContours` — arc-native bulge-polyline offset and boolean owner.
- `geometry3Sharp` — biarc and 2D curve-fit owner.
- `SharpVoronoiLib` — 2D Fortune Voronoi with Lloyd relaxation for region decomposition.
- `OpenCAMLib` — 3-axis cutter-location engine for surface finishing; vendored over shared `libocl`.

[EXCHANGE_INGRESS]:
- `ACadSharp` — DWG/DXF profile read; the write leg lives in the app UI.
- `DSTV.Net` — DSTV/NC1 steel-fabrication exchange for profile-cut programs.
- `OcctNet.Wrapper` — STEP/IGES B-rep ingress to shape and mesh.

[KINEMATICS]:
- `Robots` — serial-chain robot kinematics: forward, inverse, and external axes.
- `Rhino3dm` — `extern alias R3` boundary assembly the robot seam copies through.

[ADDITIVE]:
- `PicoGK` — implicit-voxel kernel for lattice infill and layer rasterization; companion-only.
- `lib3mf` — 3MF reader and writer for core, production, and beam-lattice egress; vendored.

[TOOLING_DATA]:
- `MTConnect.NET-Common` — ISO-13399 cutting-tool model behind the magazine and tool telemetry.

[NESTING]:
- `RectangleBinPack.CSharp` — rectangular cutting-stock packer suite and NFP rectangle fast-path.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[NUMERIC]:
- `CSparse` — direct sparse Cholesky, LDL', LU, and QR factorization with pattern-reusing refactorization and rank-1 update.
- `UnitsNet` — cut-parameter and tolerance quantity boundary.
- `MathNet.Numerics` — capability distribution fits and Monte-Carlo tolerance stackup.
- `System.Numerics.Tensors` — SIMD-lowered sampling folds across the hot toolpath and nesting lanes.
- `CommunityToolkit.HighPerformance` — 2D span grids for grayscale, engagement, and layer-census rasters.

[IDENTITY_TIME]:
- `System.IO.Hashing` — reached only through the kernel content-hash mint every egress key seeds from.
- `NodaTime` — instant stamps on travelers, quality records, probing receipts, and tool-life schedules.
- `NodaTime.Serialization.SystemTextJson` — the STJ codec carrying those instants, intervals, and zones across the content-keyed wire.

[MAPPING_GRAPH]:
- `Generator.Equals` — compile-time structural equality and member-level difference receipts over attributed partial owners.
- `Riok.Mapperly` — source-generated boundary projections, including the `extern alias R3` seam copyists.
- `QuikGraph` — setup-precedence, assembly, and rapid-link routing graphs.
- `Thinktecture.Runtime.Extensions.Json` — STJ converters for the generated value objects, smart enums, and unions on every egress.

[TEST]:
- `xunit.v3.assert`
- `xunit.v3.common`
- `xunit.v3.extensibility.core`
- `xunit.v3.mtp-v2`
- `CsCheck`
- `coverlet.MTP`
