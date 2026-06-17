# [RASM_BIM_ARCHITECTURE]

The professional domain folder structure of `Rasm.Bim`, including the planned-but-empty sub-domains, each with a one-line charter. Mechanics live on the `.planning/` design pages and forward work on `IDEAS.md` and `TASKLOG.md`.

## [1]-[DOMAIN_MAP]

The sub-domains mirror the eventual source tree. Designed sub-domains carry a `.planning/` page; planned sub-domains hold a charter and a visible gap that fuels the ideas and tasks. Every sub-domain composes the one `BimModel` the `exchange` import rail produces rather than minting a parallel model surface.

```text codemap
Rasm.Bim/
├── model/            # Host-neutral BIM object model: BimElement record discriminated by an IfcClass row, BimModel collection, the Project fold.
├── query/            # ElementSet set-algebraic query over a closed ElementPredicate union folded by Union/Intersect/Except/Where.
├── classification/   # bSDD-bound standard-systems classification axis: Classification vocabulary, ClassificationCode, ClassificationRef.
├── assembly/         # Host-neutral spatial-structure tree plus the closed AssemblyRel decomposition union over the IFC IfcRel* relationships.
├── exchange/         # Universal interchange codec: format/codec/extension axes, FrameNormalization, the import/export fold, the tessellation bridge, and the planned host-free wire projection the Python and TypeScript peers decode.
├── properties/       # First-class Pset/Qto owner over the standard Pset_*/Qto_* sets, round-tripped through IfcRelDefinesByProperties. [planned]
├── validation/       # IDS v1.0 model-validation owner folding the IDS facets into one predicate algebra over BimModel. [planned]
├── coordination/     # BCF 3.0 issue exchange and the GlobalId-stable model-diff federation change-set. [planned]
└── georeferencing/   # IFC4.3 coordinate-reference owner projecting the CRS surface onto a host-neutral GeoReference record. [planned]
```
