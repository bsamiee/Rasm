# [CAPABILITY_GAPS] — plan-cs-folders forward backlog

AEC capabilities with NO admissible NuGet package as of the survey campaign. Each item carries the verified candidate landscape (license, osx-arm64, .NET-binding state), the chosen path, and the concrete work, so the gap closes once without re-evaluating the rejected set. Licenses are registry/source-verified. Admission gate unchanged: best-of, osx-arm64 (managed AnyCPU or a real osx-arm64 native asset), newest stable, OSS or free-full-commercial (reject GPL/AGPL viral for host-neutral libs), modern packaging, no duplicate.

Four mechanisms recur: ADMIT (managed NuGet clears the gate), NATIVE-BIND (permissive native lib → osx-arm64 dylib + P/Invoke shim, shipped under `runtimes/osx-arm64/native/`), PY-COMPANION (offline two-hop over the wire — C# requests, Python evaluates, C# re-imports the artifact), HAND-ROLL (author over existing rails from the governing spec).

---

## [01]-[GEOMETRY] volumetric tetrahedral meshing

Target: `libs/csharp/Rasm` geometry kernel, Meshing seam. Need: robust constrained tet meshing (FEM/CFD volume discretization) the kernel lacks; the BVH/octree broad-phase and surface meshers do not cover volume.

PATH: **NATIVE-BIND `fTetWild`** (C++, MPL-2.0, osx-arm64 CI confirmed). MPL is file-level copyleft — modifications to fTetWild's own files must be disclosed; the .NET caller is unaffected. Fallback if MPL is rejected at review: **Geogram** (BSD-3-Clause, Inria, osx-arm64 CI) — same shim pattern, less tet-quality pedigree.

WORK: build `libftetwild.dylib` for osx-arm64 via CMake; expose a minimal C-linkage API (in: surface-mesh verts + triangles; out: tet vert + cell arrays); author a `NativeLibrary`/`[DllImport]` binding in the Meshing seam; ship the dylib under `runtimes/osx-arm64/native/`.

Rejected (do NOT re-evaluate):

| Candidate | License / blocker |
|---|---|
| MeshLib | AMV non-commercial license (not OSI) |
| ManifoldNET | win-x64 native only, no osx-arm64 RID |
| Tetgen.SilverHorn | AGPL-3.0 (TetGen v1.5+ upstream) |
| Aardvark.Geometry.PointSet | AGPL-3.0 |
| TYoshimura.MultiPrecision | unconditional AVX2, no arm64 path |
| TetWild | GPL-3.0/MPL dual; superseded by fTetWild; x86_64 CI only |
| Gmsh | GPL-2.0+ (no libgmsh LGPL carve-out) |
| CGAL Mesh_3 | GPL-3.0 (Mesh_3 component, not the LGPL core) |
| TetGen (C++) / `tetgen` PyPI | AGPL-3.0 core propagates even through the MIT py wrapper |

---

## [02]-[MATERIALS] timber + cold-formed steel design checks

Target: `libs/csharp/Rasm.Materials`, new `TimberChecks` + `ColdFormedSteelChecks` owners parallel to the concrete-RC rail, over the existing section-property + `UnitsNet` quantity inputs. No production NuGet exists for NDS-2018, AISI S100-16, EC5, or EC3-1-3; the NDS gap has no OSS implementation in any language.

PATH: **HAND-ROLL** over the section-property + `UnitsNet` rails, seeding clause structure from the MIT references below. Do NOT take a vendor lock-in or a wrong-edition dependency.

Reference seeds (MIT, GitHub-only — read, do not depend):
- `runtosolve/AISIS100.cs` — AISI S100-16…24, ASD/LRFD/LSD; best C# AISI clause map (active).
- `knippershelbig/StructuralDesignKit_Holz` and `beaverstructures/BeaverCore` — EC5/EN 1995-1-1 ULS+SLS (+ connections), .NET Std 2.0 (stale 2022).
- `eurocodepy` (Python, MIT, active) — EC5 limit-state + material DB; `pyCUFSM` (AFL-3.0) — AISI DSM finite-strip buckling only.
- SKIP `Wosad.Wood`/`Kodestruct` (pre-2018 NDS, abandoned). Commercial-only: CFS (RSG), Forte (gated SDK), Dlubal RFEM.

Governing standards (editions are load-bearing):
- Timber: **NDS-2018** (ANSI/AWC) + NDS Supplement 2018 design values; **EN 1995-1-1:2004+A2:2014** + EN 338:2016 strength classes.
- Cold-formed: **AISI S100-16 with Supplement 2 (2020)** (also S100-24); **EN 1993-1-3:2006+A1:2009** + EN 1993-1-5 (plated/shear buckling).

Limit states to cover:
- Timber NDS: bending (`C_L` beam stability, `C_F`, `C_r`, `C_fu`), shear (notch reduction), compression-∥ (`C_P` column), compression-⊥ (`C_b` bearing), tension-∥, combined bending+axial (§3.9), deflection (`E'min`, camber).
- Timber EC5: `k_crit` (LTB), `k_c` (column, §6.3.2), combined bending+compression (§6.2.4), creep `k_def`, SLS deformation.
- CFS AISI S100-16: local effective-width (Winter, §B), distortional (DSM App.1 or §D), global flexural/torsional/F-T (§E/§F), shear (§G), combined (§H), web crippling (§I), connections (§J).
- CFS EC3-1-3: effective section (§5.5), distortional stiffener spring (§5.5.3), member buckling on effective section + EN 1993-1-1 curves (§6.2/§6.3), web crippling (§6.1.7).

---

## [03]-[FABRICATION] G-code, CAM, sheet-metal, motion, metrology

Target: `libs/csharp/Rasm.Fabrication`. OCCT 7.9.3 (`OcctNet.Wrapper`, osx-arm64 native) is already admitted and carries the B-rep work.

| Capability | Path | Detail |
|---|---|---|
| G-code parse/post (RS-274 / ISO 6983) | **HAND-ROLL** | Simple line grammar; read `gradientspace/gsGCode` (MIT) for shape. `GcodeParserSharp` (NuGet) is 3D-printer-only — skip. |
| CAM toolpath kernel | **PY-COMPANION** | `OpenCAMLib` is LGPL-2.1, C++/Python, no .NET binding. Drive it offline over the wire, or (costly) build an LGPL dylib + P/Invoke. No permissive all-.NET CAM kernel exists. |
| Sheet-metal unfold / flat-pattern | **OCCT-PATH** | Use `BRepOffsetAPI_Unfold` through the admitted `OcctNet.Wrapper`. No permissive .NET package (CADExchanger MTK is commercial). |
| Jerk-limited online trajectory generation (OTG) | **NATIVE-BIND `Ruckig`** | `pantor/ruckig` C++ core is **MIT, no tiers** (the "Pro" confusion is ruckig.com's hosted API, not the lib). Header-only C++17, STL-only deps. Compile an osx-arm64 dylib (`cmake --preset arm64-macos`), ship under `runtimes/osx-arm64/native/`, P/Invoke ~3 entry points (`Ruckig<N>`, `InputParameter`, `OutputParameter`) — binding ≈150 LOC. Gold standard; Reflexxes is abandoned-commercial. |
| QIF metrology (ANSI/ISO 23952) | **HAND-ROLL** | Generate from the DMSC freeware QIF 3.x XSDs with `dotnet-xscgen`/`xsd.exe`; the NIST `QIFdotNET` reference app shows the pattern. No NuGet package. |

---

## [04]-[BIM] IFC geometry kernel + IDS validation

Target: `libs/csharp/Rasm.Bim`.

- **IFC geometry kernel (IFC BRep → tessellated mesh): PY-COMPANION.** `Xbim.Geometry`/`Geometry.Engine.Interop` is a mixed-mode CLR assembly, **Windows-only** — non-functional on osx-arm64, and the maintainers state cross-platform needs a full rewrite. On osx-arm64 only `Xbim.Essentials` (pure-managed, CDDL) runs — schema I/O, property traversal. For tessellation: Python sidecar runs `ifcopenshell.geom.create_shape` (builds natively on osx-arm64) → serializes GLB → C# re-imports via the admitted `SharpGLTF` (MIT). This is the IFC→IfcOpenShell→GLB two-hop the architecture already mandates; do NOT seek a C# IFC geometry engine.
- **IDS validation: ALREADY-COVERED — not a gap.** `Xbim.InformationSpecifications` (MIT) wrapping `ids-lib` (MIT) is admitted to Bim and covers buildingSMART IDS audit. Do NOT admit `Xbim.IDS.Validator.Core` — it is AGPL-3.

---

## [05]-[CROSS-CUTTING] notes carried from the survey + rebuild

- Same-FQN assembly collisions are handled by extern alias on the `PackageReference` (1-2-word semantic alias): `Triangle`→`TriangleNet`, `SharpVoronoiLib`→`Voronoi`. Any future pin that bundles a type colliding with an existing admit takes the same treatment; the collision-law lives in the `Directory.Build.props` global-using section.
- `Bim/.planning/geospatial.md`: `GeoVectorSource` SmartEnum + `GeoVector.Read` should grow managed `FlatGeobuf` + `GeoParquet` arms rather than routing both through `Ogr.Open`.
- Schedule seam: MPXJ is parse-only ingress (`Rasm.Persistence`); the CPM/leveling math is the `QuikGraph` + canonical-schedule-model consumer in `Rasm.Bim`. Keep the split at realization.
