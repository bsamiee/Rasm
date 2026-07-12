# [PY_GEOMETRY_API_HONEYBEE_ENERGY_STANDARDS]

`honeybee-energy-standards` is the large standards-extension data backend for the geometry energy-modeling rail: a data-only package shipping the full ASHRAE 90.1 / DOE-prototype library — the climate-zone-and-vintage construction sets, the whole-building and space-by-space program types, the standard constructions/materials, and the thousands of standard schedules — as JSON dropped into `honeybee_energy.config.folders.standards_extension_folders` (`~/ladybug_tools/resources/standards/honeybee_energy_standards`) where `honeybee-energy`'s `lib._load*` modules scan and merge it on top of the always-loaded `honeybee-standards` defaults floor. It carries NO Python code (the `__init__` is a docstring only); its surface is the JSON data contract resolved through the `honeybee-energy` `lib.*_by_identifier` loaders. It is the package that makes a standard ASHRAE/DOE identifier resolvable — `building_program_type_by_identifier` (the whole-building DOE-prototype resolver) and `construction_set_by_identifier` for a climate-zone/vintage set both REQUIRE this extension; without it only the ~20-construction / 2-program `honeybee-standards` floor resolves. It is the `standards` extra of `dragonfly-energy` (the urban model assigns realistic defaults from this library), the optional large-library complement of `honeybee-standards` (the small floor), and is consumed exclusively through `honeybee-energy.lib` — never re-shipped, re-parsed, or conflated with the defaults floor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-energy-standards`
- package: `honeybee-energy-standards`
- import: `import honeybee_energy_standards` (a docstring-only module; it exposes NO callable API); consumed indirectly through `honeybee_energy.config.folders.standards_extension_folders` and the `honeybee_energy.lib.*_by_identifier` loaders
- owner: `geometry`
- rail: energy-modeling
- consumer: `.planning/energy/model.md` — reached exclusively through the `honeybee_energy.lib` `RESOLVERS` rows, never a direct JSON read
- version: `2.3.0`
- license: AGPL-3.0 (Ladybug Tools copyleft)
- abi: pure-data `py3-none-any` wheel — JSON resource files plus a trivial docstring-only `__init__.py`; no compiled payload and no executable code (the module note: "this package contains no Python code and only contains JSON objects that is consumed by the honeybee-energy package when installed")
- depends-on: none at install (it is a leaf data package); `honeybee-energy` is its consumer, not its dependency — the data is read by `honeybee_energy.lib` once placed under `folders.standards_extension_folders`
- entry points: none (no console scripts; data-only)
- capability: ships the large standard energy library resolvable by identifier — 1039 constructions, 256 climate-zone/vintage construction sets across 8 ASHRAE vintages, 1845 program types (whole-building DOE-prototype + space-by-space), 3347 schedules, plus the `building_mix.json` whole-building program mixes and the `hvac_registry.json` / `programtypes_registry` vintage indices — the data backend that turns `honeybee-energy.lib.*_by_identifier` from a ~20-item floor into the full ASHRAE 90.1 / DOE-prototype catalog

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: data library layout (`honeybee_energy_standards/`)
- rail: energy-modeling
- The entire surface is the on-disk JSON tree honeybee-energy scans; there are no classes, functions, or path constants (unlike `honeybee-standards`, which exposes two path constants — this package exposes none, only the docstring module). The data is organized by domain and ASHRAE vintage.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                                                                                                  |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `constructions/`          | data folder   | 1039 standard opaque/window constructions + their materials (abridged JSON)                                                                                   |
|  [02]   | `constructionsets/`       | data folder   | 256 construction sets across 8 vintage files (`pre_1980`/`1980_2004`/`2004`/`2007`/`2010`/`2013`/`2016`/`2019`), keyed by climate zone + vintage              |
|  [03]   | `programtypes/`           | data folder   | 1845 program types (DOE-prototype whole-building + ASHRAE space-by-space), abridged JSON                                                                      |
|  [04]   | `programtypes_registry/`  | data folder   | per-vintage registry indices (`<vintage>_registry.json`) mapping building type -> program-type identifier (the `building_program_type_by_identifier` backing) |
|  [05]   | `schedules/schedule.json` | data file     | 3347 standard schedules (the ruleset/fixed-interval profiles the programs reference)                                                                          |
|  [06]   | `building_mix.json`       | data file     | whole-building program-mix weights (space-type fractions per building type)                                                                                   |
|  [07]   | `hvac_registry.json`      | data file     | the standard HVAC template registry keyed by vintage/efficiency                                                                                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the data contract and the consumer seam
- rail: energy-modeling
- The package has no callable entry points; its "entry points" are the JSON data shape and the `folders.standards_extension_folders` location `honeybee-energy.lib` reads. The owner accesses everything below via the `honeybee-energy` `lib.*_by_identifier` loaders, never by opening these files — the same single access path `honeybee-standards` uses.

| [INDEX] | [SURFACE]                                                                                                                                         | [CALL_SHAPE]      | [CAPABILITY]                                                                                             |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------ | :---------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | install location = `honeybee_energy.config.folders.standards_extension_folders` (`~/ladybug_tools/resources/standards/honeybee_energy_standards`) | filesystem folder | the directory `honeybee_energy.lib._load*` scans at import to merge this library onto the defaults floor |
|  [02]   | `honeybee_energy.lib.programtypes.program_type_by_identifier(identifier)`                                                                         | identifier string | resolve any loaded program type (defaults + this extension) by id                                        |
|  [03]   | `honeybee_energy.lib.programtypes.building_program_type_by_identifier(building_type)`                                                             | building type     | resolve a whole-building DOE-prototype program (REQUIRES this extension; absent on the bare floor)       |
|  [04]   | `honeybee_energy.lib.constructionsets.construction_set_by_identifier(identifier)`                                                                 | identifier string | resolve a climate-zone/vintage construction set (the ASHRAE 90.1 sets ship here)                         |
|  [05]   | `honeybee_energy.lib.constructions.opaque_construction_by_identifier` / `window_construction_by_identifier`                                       | identifier string | resolve a standard construction by id                                                                    |
|  [06]   | `honeybee_energy.lib.schedules.schedule_by_identifier(identifier)`                                                                                | identifier string | resolve a standard schedule by id                                                                        |

## [04]-[IMPLEMENTATION_LAW]

[ENERGY_MODELING_STANDARDS_EXTENSION]:
- import: `import honeybee_energy_standards` is meaningless for the owner (docstring-only module, no symbols); the library reaches the owner only after its JSON is present under `folders.standards_extension_folders` and `honeybee_energy.lib` has scanned it. The owner never imports this package directly.
- layering axis: the library is ADDITIVE on top of the `honeybee-standards` defaults floor. At `honeybee_energy.lib` import, `_load*` reads `folders.defaults_file` FIRST (the small always-resolvable `honeybee-standards` baseline: 16 constructions, 2 program types, 8 schedules, 1 construction set), THEN scans `folders.standards_extension_folders` and merges this extension. The registries (`OPAQUE_CONSTRUCTIONS`/`PROGRAM_TYPES`/`SCHEDULES`/`CONSTRUCTION_SETS`) enumerate the union, so a count of ~1845 program types / ~3347 schedules means this extension is installed — those are its contribution, not `honeybee-standards`'.
- vintage axis: the construction sets and programs are organized by 8 ASHRAE vintages (`pre_1980`, `1980_2004`, `2004`, `2007`, `2010`, `2013`, `2016`, `2019`); a `construction_set_by_identifier` for an ASHRAE 90.1 climate-zone/vintage set (e.g. a 2019 climate-zone-5 set) resolves only from this library. The `programtypes_registry/<vintage>_registry.json` indices back `building_program_type_by_identifier(building_type)`, the whole-building DOE-prototype resolver that is unavailable on the bare floor.
- access-path axis: the data is abridged (objects reference materials/schedules by identifier), so it is materialized only through the `honeybee-energy` `lib` loaders' ordered `from_dict_abridged` pass (type-limits -> schedules -> materials -> constructions -> construction-sets -> programs). The owner resolves by identifier through `lib.*_by_identifier` and never hand-parses the JSON (which does re-implement the abridged-resolution order the lib loaders own).
- boundary: honeybee-energy-standards owns ONLY the large standard library DATA. The loaders, by-identifier resolution, and registries are `honeybee-energy.lib`; the small always-loaded defaults floor is the separate `honeybee-standards`; the object model is `honeybee-core`; the urban consumer is `dragonfly-energy` (this is its `standards` extra). This package contains no executable code.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `honeybee-energy-standards`
- Owns: the large ASHRAE 90.1 / DOE-prototype standard energy library DATA — the climate-zone/vintage construction sets, whole-building and space-by-space program types, standard constructions/materials, standard schedules, building program mixes, and HVAC/programtype vintage registries — dropped into `folders.standards_extension_folders`
- Accept: serving the by-identifier resolution of ASHRAE/DOE standard constructions/construction-sets/programs/schedules that `honeybee-energy.lib` merges onto the `honeybee-standards` floor at import; the `standards` extra of `dragonfly-energy` for assigning realistic urban-model defaults; resolution exclusively through the `honeybee-energy` `lib.*_by_identifier` loaders
- Reject: reading the extension JSON directly over the `lib.*_by_identifier` loaders; re-implementing the abridged-resolution order the lib loaders own; conflating this large library with the `honeybee-standards` defaults floor; re-shipping or re-deriving the standard construction/program/schedule data; adding a code API to a data-only package; assuming `building_program_type_by_identifier`/an ASHRAE climate-zone construction set resolves without this extension present

[CAPTURE_GAP]:
- this is the LARGE library, not the defaults floor (verified against the installed `honeybee-energy-standards 2.3.0`): the data tree contributes 1039 constructions, 256 construction sets (8 ASHRAE vintages), 1845 program types, and 3347 schedules — the full ASHRAE 90.1 / DOE-prototype catalog. The small always-resolvable baseline (1 construction set, 16 constructions, 16 materials, 2 program types, 8 schedules, 9 type limits) is the SEPARATE `honeybee-standards` package (`folders.defaults_file`). The `honeybee-energy.lib` registry counts are the UNION of whatever is installed, so a registry showing ~1845 program types means THIS extension is present; a model that resolves only the floor identifiers needs no extension, while a model referencing ASHRAE/DOE standard identifiers (an ASHRAE 90.1 climate-zone construction set, a DOE-prototype whole-building program) requires this package installed under `folders.standards_extension_folders`.
- the package exposes NO Python surface (verified `2.3.0`): unlike `honeybee-standards` (which exposes the `energy_default`/`radiance_default` path constants), `honeybee_energy_standards.__init__` is a docstring only — there are no symbols, no path constants, no functions. The "API" is entirely the on-disk JSON contract scanned by `honeybee_energy.config.folders.standards_extension_folders`; any consumer that tries to `import` a member finds none. Resolution is exclusively through `honeybee-energy.lib`.
