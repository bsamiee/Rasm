# [RASM_FABRICATION]

`Rasm.Fabrication` is a production digital-fabrication engine: one polymorphic `Fabrication` owner closes 3D model to verified machine program across subtractive CNC/CAM at full depth — 2.5D through multi-axis surface finishing, rest machining, engagement control — additive manufacturing at production depth — planar and implicit lanes, support structures, orientation, lattice infill, 3MF hand-off — true-shape nesting and cutting-stock yield, sheet and tube forming, welding, steel NC1 exchange, and post-processing at controller-family breadth. Its output bar is machine truth, not toolpath sketching: every program survives voxel material-removal verification, on-machine probing, and modal cycle-time simulation before it posts, and the spec plane — GD&T vocabulary, process capability, setup sheets, shop travelers, as-built quality records — rides the same content-keyed spine. Its posting plane feeds real shop controllers, its verify plane gates real material, and every artifact lands machine-consumable.

Every manufacturing process folds through a single `FabricationPolicy` dispatch to a content-keyed artifact minted on the `EgressKind` egress spine, over the `Rasm` geometry kernel and the `Rasm.Element` seam. It references no AEC peer — alignment travels through seam contracts and the content-keyed wire.

## [01]-[ROUTER]

[PROCESS]:
- [01]-[OWNER](.planning/Process/owner.md): Fabrication entry owner — atoms vocabulary and the polymorphic `Run` dispatch every flagship enters.
- [02]-[FAMILY](.planning/Process/family.md): Process and machine axis families that discriminate every downstream generator arm.
- [03]-[PHYSICS](.planning/Process/physics.md): One material identity carrying per-modality physics and the removal budget CAM lanes consume.
- [04]-[FAULTS](.planning/Process/faults.md): `FabricationFault` registry entry every sub-domain lowers its fault arms onto.
- [05]-[DERIVATION](.planning/Process/derivation.md): Plan orchestrator staging manufacturability, routing, fleet, and documentation.

[TOOLING]:
- [06]-[MAGAZINE](.planning/Tooling/magazine.md): ISO-13399 tool-assembly magazine and the minimal-swap tool-life schedule posting consumes.
- [07]-[CUTTINGDATA](.planning/Tooling/cuttingdata.md): Kienzle machinability seeds and the cutter-form projection feeds and speeds resolve through.
- [08]-[WEAR](.planning/Tooling/wear.md): Flank-wear and condition-based remaining-life estimation over decoded machine telemetry.

[GEOMETRY2D]:
- [09]-[ALGEBRA](.planning/Geometry2D/algebra.md): Line-space owner — offset, boolean clip, area, and Minkowski over closed and open paths.
- [10]-[ARCS](.planning/Geometry2D/arcs.md): Arc-space owner — kerf, lead, and adaptive bulge-polyline offsets orthogonal to the line lane.
- [11]-[CURVES](.planning/Geometry2D/curves.md): Parametric-curve substrate feeding turning, pocket profiles, posting stations, and 5-axis smoothing.

[INGRESS]:
- [12]-[PROFILE](.planning/Ingress/profile.md): DXF/DWG profile arm of the one polymorphic `Ingress.Admit` fold.
- [13]-[SOLID](.planning/Ingress/solid.md): STEP/IGES/STL B-rep ingress triangulated into kernel mesh admission.
- [14]-[STEEL](.planning/Ingress/steel.md): DSTV NC1 steel-exchange read arm projected into the loop vocabulary.
- [15]-[ELEMENT](.planning/Ingress/element.md): Element-graph bake arm resolving an admitted component from the AEC seam.

[TOOLPATH]:
- [16]-[MOTION](.planning/Toolpath/motion.md): CAM generator — process-modality and cut-strategy arms emitting the conditioned motion.
- [17]-[SURFACE](.planning/Toolpath/surface.md): Cutter-location surface finishing over on-mesh path layout — waterline, scallop, pencil, rest.
- [18]-[PARTITION](.planning/Toolpath/partition.md): Voronoi region decomposition for pocket, stipple, engrave, and pen-plot strategies.
- [19]-[GUARD](.planning/Toolpath/guard.md): Swept tool-plus-holder gouge and collision guard consulted per committed feed move.
- [20]-[SKELETON](.planning/Toolpath/skeleton.md): Trochoidal constant-engagement walk over the kernel clearance family.
- [21]-[TURNING](.planning/Toolpath/turning.md): Lathe cycle owner — roughing, finishing, grooving, and threading over the revolved envelope.
- [22]-[WIRE](.planning/Toolpath/wire.md): Wire-EDM cycle owner — skim offsets, taper, corner modes, and slug retention.
- [23]-[LINK](.planning/Toolpath/link.md): Rapid-travel minimization routing obstacle-aware retracts over guard clearance.
- [24]-[BEVEL](.planning/Toolpath/bevel.md): Per-edge bevel-type coupled condition rows and the one weld-prep Z-law custodian.

[KINEMATICS]:
- [25]-[CELL](.planning/Kinematics/cell.md): Serial-chain robot-cell solve with per-manufacturer posts at the boundary assembly.
- [26]-[MACHINE](.planning/Kinematics/machine.md): 5-axis rotary inverse and TCP/RTCP admission owning the motion-dynamics law.
- [27]-[FLEET](.planning/Kinematics/fleet.md): Machine-capability registry ranking a component against the available fleet.

[ADDITIVE]:
- [28]-[SLICING](.planning/Additive/slicing.md): FFF/DED planar slicing — shells, infill, and the variable-bead law over the kernel slice stack.
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
- [37]-[WORKHOLDING](.planning/Fixturing/workholding.md): Clamp and exclusion-zone keep-out family and the conditioning fold CAM composes.
- [38]-[SETUPS](.planning/Fixturing/setups.md): Multi-fixture setup scheduler owning setup-to-WCS assignment and datum lineage.
- [39]-[ASSEMBLY](.planning/Fixturing/assembly.md): Join-precedence planning over the assembly DAG with topological order.

[POSTING]:
- [40]-[PROGRAM](.planning/Posting/program.md): Dialect-neutral cut-program AST, cut conditioning, and the round-trip parse.
- [41]-[DIALECT](.planning/Posting/dialect.md): Per-dialect emission over the post-dialect grammar family, including the NC1 target.
- [42]-[OPTIMIZATION](.planning/Posting/optimization.md): Feedrate adaptation, corner smoothing, and block compaction over the program AST.

[VERIFY]:
- [43]-[REMOVAL](.planning/Verify/removal.md): Voxel material-removal verify producing gouge, uncut, overcut, and residual-stock receipts.
- [44]-[PROBING](.planning/Verify/probing.md): In-process metrology — probe cycles, ICP datum best-fit, and conformance verdicts.
- [45]-[SIMULATE](.planning/Verify/simulate.md): Modal-state execution walk over the parsed program — authoritative cycle-time owner.
- [46]-[ESTIMATION](.planning/Verify/estimation.md): Cost estimation folding clock, waste, wear, remnant credit, and fleet rates into a receipt.
- [47]-[AUDIT](.planning/Verify/audit.md): Additive layer-stack pre-flight over connected-component and cross-layer lineage checks.

[SPEC]:
- [48]-[TOLERANCE](.planning/Spec/tolerance.md): Typed GD&T vocabulary — fits, feature-control frames, datums, and surface finish.
- [49]-[CAPABILITY](.planning/Spec/capability.md): Process-capability statistics and the plan-time capability gate verdict.
- [50]-[MANUFACTURABILITY](.planning/Spec/manufacturability.md): Cross-modality design-for-manufacture verdicts and the ranked routing feed.

[DOCUMENTATION]:
- [51]-[PROJECTION](.planning/Documentation/projection.md): Hidden-line-removal emission preserving the receipt the app UI is insulated at.
- [52]-[TRAVELER](.planning/Documentation/traveler.md): Content-keyed setup sheets and shop travelers — widest fan-in receipt validator.
- [53]-[REPORT](.planning/Documentation/report.md): As-built quality records — FAI, mill-cert, and weld-inspection, content-keyed.

[FORMING]:
- [54]-[SHEET](.planning/Forming/sheet.md): One unfold owner — bend-allowance flat patterning over the kernel development.
- [55]-[BRAKE](.planning/Forming/brake.md): Best-first bend-sequence planning over the reach, collision, and occlusion matrix.
- [56]-[TUBE](.planning/Forming/tube.md): Tube centerline folding, elongation carry, and analytic cope development.

[JOINING]:
- [57]-[WELD](.planning/Joining/weld.md): Joint-by-prep bead-stack composition over the Materials-owned groove geometry.
- [58]-[SEQUENCE](.planning/Joining/sequence.md): Distortion-control weld ordering, tack planning, and interpass scheduling.
- [59]-[PROCEDURE](.planning/Joining/procedure.md): WPS/PQR essential-variable rows and the heat-input compliance gate.

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
- `UnitsNet` — cut-parameter and tolerance quantity boundary.
- `MathNet.Numerics` — capability distribution fits and Monte-Carlo tolerance stackup.
- `System.Numerics.Tensors` — SIMD-lowered sampling folds across the hot toolpath and nesting lanes.
- `CommunityToolkit.HighPerformance` — 2D span grids for grayscale, engagement, and layer-census rasters.

[IDENTITY_TIME]:
- `System.IO.Hashing` — reached only through the kernel content-hash mint every egress key seeds from.
- `NodaTime` — instant stamps on travelers, quality records, probing receipts, and tool-life schedules.

[MAPPING_GRAPH]:
- `Riok.Mapperly` — source-generated boundary projections, including the `extern alias R3` seam copyists.
- `QuikGraph` — setup-precedence, assembly, and rapid-link routing graphs.

[TEST]:
- `xunit.v3.assert`
- `xunit.v3.common`
- `xunit.v3.extensibility.core`
- `xunit.v3.mtp-v2`
- `CsCheck`
- `coverlet.MTP`
