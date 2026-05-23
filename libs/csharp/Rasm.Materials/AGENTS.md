# Rasm.Materials Agent Instructions

## Purpose

`Rasm.Materials` is the architectural material catalogue. It is not a geometry library and not a Rhino-aware module.

Encode building-material reference data (sizes, families, grades, types, bonds, special shapes, joints, properties) as typed in-memory catalogues queried through `FrozenDictionary` statics. Downstream consumers — geometry generators, Grasshopper components, structural assemblers — receive immutable typed records keyed by closed `SmartEnum` vocabularies. No JSON, no SQL, no source generator, no native binary, no I/O.

## Design Contract

- Build per-material polymorphic surfaces, not flat constant pools. Each material folder owns its full domain (types + data + query) through one consumer surface and dense FP internals.
- Author data directly in C#. The type system is the schema. Refactor across the catalogue with IDE tools, never through stringly-typed JSON migration.
- Keep public usage small and typed. Consumers access through SmartEnum-attached properties (`BrickDesignation.UsModular.Spec`, `BondName.English.Courses`, `brick.Region.Policy`) — total lookups, no `Fin` rail. `BrickCatalog.Shapes` (FrozenSet) and `BrickCatalog.InRegion(region)` cover set/projection queries.
- Encode regional standards (BIA TN 10, BS EN 771-1, DIN 105, AS/NZS 4455.1, IS 1077) as closed vocabularies inside a single `BrickDesignation` `SmartEnum`. Adding a brick = adding one factory line.
- Express patterns as data + interpreter. Bonds are `CourseTemplate` records (orientations + offset fractions), never class hierarchies. Special shapes are catalogue records, not subtypes.

## Folder Ownership

- `Bricks/` owns the masonry catalogue: regional brick sizes (US/UK/DIN/AU/IS), bond patterns as course-template data, special shapes, joint profiles with attached defaults (thickness, description), regional masonry policy (movement/expansion/weep/tie attached to `BrickRegion`), arch rules attached to `ArchProfile`, ASTM type tolerances attached to `BrickType`, and coring archetypes with attached void-fraction and classification.
- Future material folders (`Steel/`, `Concrete/`, `Timber/`, `Glass/`, `Stone/`, …) follow the same pattern: one folder, one polymorphic catalogue, one query surface, one error union.

## Domain Extension Rules

- Treat each material folder as a closed bounded context. No cross-material data sharing at the catalogue level — a brick joint is not a stone joint; encode separately even when names overlap.
- Add new bricks by extending the `BrickDesignation` `SmartEnum` and the corresponding `BrickCatalog` static. Never branch the regional taxonomy into per-region SmartEnums.
- Flow outward from `Rasm.Materials.Bricks` into consumers. `Rasm.Materials` itself never depends on geometry, Rhino, or Grasshopper.
- Regional masonry policy attaches to `BrickRegion` SmartEnum directly (movement coefficients, expansion-joint spacing, weep-hole spacing, tie spacing). US baseline (TMS 402-16 / BIA TN 18A-2019) serves as universal default for UK/DIN/AU/IS until authoritative regional values emerge — no fabricated splits.

## Surface Rules

- Expose one query surface per material. `Brick/` routes through `BrickCatalog`. No `BrickFinder`, `BrickRepository`, or sibling accessor types.
- Do not export the underlying `Dictionary` builders. Only the `FrozenDictionary` statics and typed query methods are public.
- Do not bolt new materials onto `Rasm.Materials.Brick`. New materials get their own folder + namespace + catalogue.

## Implementation Rules

- Author dimensions in metric (millimetres). Imperial standards (BIA TN 10) convert at authoring time; the record stores mm only.
- Keep brick records immutable: `sealed record Brick` with primary constructor, no setters, no mutation.
- Bond patterns are `Seq<Orientation>` sequences with offset fractions, never strings or magic numbers.
- Use `Fin<T>` for lookups that may miss; switch expressions for dispatch; `[Union]` for orientation/cut/shape/closure/error variance; `[SmartEnum<string>]` for closed designations.
- `JointProfile` SmartEnum is self-describing (carries default thickness and description). No separate `JointSpec` record — attached SmartEnum properties are the canonical lookup surface.
- Apply the Coring methodology (SmartEnum factory params → readonly auto-properties) to every closed vocabulary with canonical data: `Coring`, `BrickType`, `JointProfile`, `BrickRegion`, `BondName`, `ArchProfile`, `BrickDesignation` all carry their authoritative payload inline. Catalogue records (`Brick`, course templates) are SmartEnum-attached, not separate `FrozenDictionary` entries — lookups are total at the type level.
- Validated value-objects: `Dim3` is `[ComplexValueObject]` with `ValidateFactoryArguments(ref ValidationError?, ref widthMm, ref heightMm, ref lengthMm)`; `AspectRatio` is `[ValueObject<double>(KeyMemberName = "LengthOverWidth", ...)]`. Constructor sites use `Dim3.Create(widthMm: …, …)` and `AspectRatio.Create(lengthOverWidth: …)` — throws `ValidationException` on invalid input, by design (build-time catalogue literals never trigger; consumer-input call sites surface validation failures).
- Standards-of-record: `BrickSource` `[Union]` (BiaTn10Table1/Table2/BsEn771Part1/Din105/AsNzs4455Part1/Is1077, each carrying `string Note`). Every `Brick` carries a typed `BrickSource Source` — no stringly-typed citations.
- Use `docs/system-api-map` for `FrozenDictionary`, BCL collection, and package/reference policy; keep catalog data in static frozen lookups unless a generated smart enum owns the behavior.
- Cite the source standard inline as XML doc comment on each catalogue entry (e.g., `/// <summary>BIA TN 10 Table 1.</summary>`).
