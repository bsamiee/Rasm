# [RASM_BIM]

`Rasm.Bim` is the host-neutral AEC-domain package owning the universal BIM object model and the universal IFC/exchange semantic model. The `.planning/` pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns the IFC semantic graph (in-process GeometryGym ingest, never tessellated BRep), the glTF/IFC/STEP import-export codec over SharpGLTF and GeometryGym, the AP242/native CAD-STEP semantics, the per-importer frame normalization, the `BimElement` element vocabulary, the `ElementSet` set-algebraic query algebra, the `Classification` standard-systems axis, and the host-neutral `BimAssembly` spatial-structure tree; it COMPOSES the `Rasm` kernel geometry, consumes the `csharp:Compute/interchange#CONTENT_ADDRESSING` content-identity and the `csharp:Compute/interchange#TWO_HOP_TESSELLATION` companion tessellation rail at the seam, and meets `python:geometry/ifc-companion` ifcopenshell only at the wire. It independently owns the universal IFC/exchange SEMANTIC model and coexists with the rich Rhino-native `csharp:Rasm.Rhino/Exchange` drafting/Make2D/native-file capture — neither gutted to feed the other. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                  | [OWNS]                                                                       |
| :-----: | :----------------------------------------------------- | :--------------------------------------------------------------------------- |
|   [1]   | [Model/object-model](Model/.planning/object-model.md)  | `BimElement` element vocabulary; `ElementSet` query algebra; `Classification` axis; host-neutral `BimAssembly` |
|   [2]   | [Exchange/interchange](Exchange/.planning/interchange.md) | format/codec axes; glTF/mesh import-export; IFC semantic graph; IFC STEP/XML/JSON serialization; AP242/native CAD-STEP; companion tessellation bridge |

## [2]-[ADMISSIONS_RECORD]

The admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. The IFC object model and STEP/XML/JSON serialization compose `GeometryGymIFC_Core`; the glTF/GLB read-write and the KHR/EXT extension surface compose SharpGLTF; the content-identity and the companion tessellation rail are consumed from `Rasm.Compute` at the seam, never re-minted here. `[STATUS]` is one of `admitted`, `catalogue-pending`.

| [INDEX] | [PACKAGE]                       | [PAGE]               | [CATALOGUE]            | [STATUS]          |
| :-----: | :------------------------------ | :------------------- | :--------------------- | :---------------- |
|   [1]   | GeometryGymIFC_Core             | Exchange/interchange, Model/object-model | api-geometrygym-ifc.md | admitted          |
|   [2]   | SharpGLTF.Core                  | Exchange/interchange | api-sharpgltf.md       | admitted          |
|   [3]   | SharpGLTF.Toolkit               | Exchange/interchange | api-sharpgltf.md       | admitted          |
|   [4]   | SharpGLTF.Runtime               | Exchange/interchange | api-sharpgltf.md       | admitted          |
|   [5]   | Thinktecture.Runtime.Extensions | all pages            | doctrine (stack atlas) | admitted          |
|   [6]   | LanguageExt.Core                | all pages            | doctrine (stack atlas) | admitted          |
|   [7]   | NodaTime                        | Exchange/interchange, Model/object-model | doctrine (stack atlas) | admitted          |
|   [8]   | System.IO.Hashing               | Exchange/interchange | api-highperformance.md | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]                      | [EVIDENCE]                                                              |
| :-----: | :-------------------- | :-------------------------- | :--------------------------------------------------------------------- |
|  [G1]   | locked restore        | Assay restore rail          | clean closure; unchanged `packages.lock.json`                         |
|  [G2]   | API catalogue resolve | `assay api` doctor/resolve  | GeometryGym + SharpGLTF keys resolve; catalogues current               |
|  [G3]   | static plan + build   | Assay static rail           | routed closure compiles, zero `': error '` lines                      |
|  [G4]   | spec law-matrix       | Assay test rail (Bim target) | IFC round-trip identity, GlobalId stability, frame-normalization, set-algebra laws hold |
|  [G5]   | page diagram render   | local mermaid-cli           | page diagrams render through the local renderer                        |
