# [RASM_BIM]

`Rasm.Bim` is the host-neutral AEC-domain package owning the universal BIM object model and the universal IFC/glTF/STEP exchange and validation semantics. It owns the IFC semantic graph (in-process GeometryGym ingest, never tessellated BRep), the glTF/IFC/STEP import-export codec, the per-importer frame normalization, the `BimElement` element vocabulary, the `ElementSet` query algebra, the bSDD-bound classification axis, and the host-neutral assembly tree; it composes the kernel `Rasm` geometry, consumes the `Rasm.Compute` content-identity and companion tessellation rail at the seam, and meets `python:geometry/ifc-companion` ifcopenshell only at the wire. The professional domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/` mirror the eventual source tree, one page per source file.

- [model/elements](.planning/model/elements.md): `BimElement` element record, `IfcClass` discriminant, `BimModel` collection, and the `Project` fold from the IFC semantic graph.
- [query/element-set](.planning/query/element-set.md): the set-algebraic `ElementSet` query over a closed `ElementPredicate` union.
- [classification/systems](.planning/classification/systems.md): the bSDD-bound standard-systems classification axis and the `IfcRelAssociatesClassification` round-trip.
- [assembly/spatial-structure](.planning/assembly/spatial-structure.md): the host-neutral spatial-structure tree and the closed `AssemblyRel` decomposition algebra.
- [exchange/format-axis](.planning/exchange/format-axis.md): the `InterchangeFormat`/`InterchangeCodec`/`KhrExtension` format-codec-extension table, the `FrameNormalization` per-importer frame coercion, and the `Detect` row resolution.
- [exchange/import-rail](.planning/exchange/import-rail.md): the `BimIo` foreign-bytes ingest fold — managed glTF/mesh decode and the in-process semantic IFC/IFC5/STEP `IfcSemanticModel` graph.
- [exchange/export-rail](.planning/exchange/export-rail.md): the `BimExport` artifact emit — GLB mesh-and-scene with Draco/meshopt encode, IFC STEP/XML/JSON serialization, the `InterchangePolicy` and `ExportArtifact` carriers.
- [exchange/tessellation-bridge](.planning/exchange/tessellation-bridge.md): the `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.

The `properties`, `validation`, `coordination`, and `georeferencing` sub-domains, plus the host-free `exchange/wire` projection page within the existing `exchange` sub-domain, are planned with charters in `ARCHITECTURE.md` and tasks in `TASKLOG.md`; they hold no design page yet.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions are centralized in the one C# manifest.

- GeometryGymIFC_Core
- SharpGLTF.Core
- SharpGLTF.Toolkit
- SharpGLTF.Runtime
- Openize.Drako
- Alimer.Bindings.MeshOptimizer
- Thinktecture.Runtime.Extensions
- Thinktecture.Runtime.Extensions.Json
- LanguageExt.Core
- NodaTime
- System.IO.Hashing
