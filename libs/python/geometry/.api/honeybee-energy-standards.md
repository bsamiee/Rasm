# [PY_GEOMETRY_API_HONEYBEE_ENERGY_STANDARDS]

`honeybee-energy-standards` owns the large ASHRAE 90.1 / DOE-prototype standard energy library as pure JSON data on the energy-modeling rail. It carries no Python code — its surface is the JSON contract dropped into `honeybee_energy.config.folders.standards_extension_folders`, resolved through the `honeybee-energy` `lib.*_by_identifier` loaders that merge it additively onto the `honeybee-standards` floor. `building_program_type_by_identifier` and an ASHRAE climate-zone `construction_set_by_identifier` require it; absent this extension only the small `honeybee-standards` floor resolves.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-energy-standards`
- package: `honeybee-energy-standards`
- import: `import honeybee_energy_standards` — a docstring-only module exposing no callable API; consumed through `honeybee_energy.config.folders.standards_extension_folders` and the `honeybee_energy.lib.*_by_identifier` loaders
- owner: `geometry`
- rail: energy-modeling
- consumer: `.planning/energy/model.md` — reached through the `honeybee_energy.lib` resolver rows, never a direct JSON read
- license: AGPL-3.0 (Ladybug Tools copyleft)
- abi: pure-data `py3-none-any` wheel — JSON resource tree with a docstring-only `__init__.py`; no compiled payload, no console scripts, no install dependencies (a leaf data package `honeybee-energy` reads once it sits under `folders.standards_extension_folders`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: data library layout (`honeybee_energy_standards/`)
- rail: energy-modeling
- No classes, functions, or path constants: the entire surface is the on-disk JSON tree `honeybee-energy` scans, organized by domain and by the 8 ASHRAE vintages.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `constructions/`          | data folder   | standard opaque/window constructions and their materials (abridged)   |
|  [02]   | `constructionsets/`       | data folder   | construction sets across the 8 vintage files, keyed climate + vintage |
|  [03]   | `programtypes/`           | data folder   | program types (DOE-prototype whole-building + ASHRAE space-by-space)  |
|  [04]   | `programtypes_registry/`  | data folder   | per-vintage `<vintage>_registry.json` building-type -> program-id map |
|  [05]   | `schedules/schedule.json` | data file     | standard ruleset/fixed-interval profiles the programs reference       |
|  [06]   | `building_mix.json`       | data file     | whole-building program-mix weights (space-type fractions per type)    |
|  [07]   | `hvac_registry.json`      | data file     | standard HVAC template registry keyed by vintage                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the data contract and the consumer seam
- rail: energy-modeling
- Package has no callable entry points: its contract is the JSON shape and the install location `honeybee_energy.lib._load*` scans at import. Loader rows elide the shared `honeybee_energy.lib.` prefix; the owner resolves through these `*_by_identifier` loaders, never by opening the files.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]      | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `folders.standards_extension_folders`              | install folder    | scanned at import; merges this library onto the defaults floor |
|  [02]   | `programtypes.program_type_by_identifier`          | identifier string | any loaded program type (defaults + this extension) by id      |
|  [03]   | `programtypes.building_program_type_by_identifier` | building type     | whole-building DOE-prototype program; requires this extension  |
|  [04]   | `constructionsets.construction_set_by_identifier`  | identifier string | climate-zone/vintage construction set (the ASHRAE 90.1 sets)   |
|  [05]   | `constructions.opaque_construction_by_identifier`  | identifier string | a standard opaque construction by id                           |
|  [06]   | `constructions.window_construction_by_identifier`  | identifier string | a standard window construction by id                           |
|  [07]   | `schedules.schedule_by_identifier`                 | identifier string | a standard schedule by id                                      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- At `honeybee_energy.lib` import the `_load*` modules read `folders.defaults_file` first (the `honeybee-standards` baseline), then scan `folders.standards_extension_folders` and merge this extension additively; the registries enumerate the union, so an ASHRAE/DOE identifier resolves only with this library present.
- Construction sets and programs partition by the 8 ASHRAE vintages (`pre_1980`, `1980_2004`, `2004`, `2007`, `2010`, `2013`, `2016`, `2019`); `construction_set_by_identifier` for a climate-zone/vintage set and `building_program_type_by_identifier` — backed by `programtypes_registry/<vintage>_registry.json` — resolve only from this library.
- Abridged objects reference materials and schedules by identifier, materialized only through the lib loaders' ordered `from_dict_abridged` pass (type-limits -> schedules -> materials -> constructions -> construction-sets -> programs); the owner resolves by identifier and never hand-parses the JSON.

[STACKING]:
- `honeybee-standards`(`.api/honeybee-standards.md`): the defaults floor this library layers onto — `folders.defaults_file` seeds the registries first, this extension merges on top through `folders.standards_extension_folders`.
- `honeybee-energy`(`.api/honeybee-energy.md`): the `lib.*_by_identifier` loaders and `_load*` scan modules are the sole resolution rail; this library's JSON becomes registry objects only across that seam.
- `dragonfly-energy`(`.api/dragonfly-energy.md`): consumes this library as its `standards` extra, assigning realistic ASHRAE/DOE defaults to the urban model through the same `honeybee-energy.lib` resolvers.

[LOCAL_ADMISSION]:
- Admit for by-identifier resolution of ASHRAE/DOE standard constructions, construction-sets, programs, and schedules merged onto the `honeybee-standards` floor, and as the `standards` extra of `dragonfly-energy`; resolution enters exclusively through the `honeybee-energy.lib` loaders.

[RAIL_LAW]:
- Package: `honeybee-energy-standards`
- Owns: the large ASHRAE 90.1 / DOE-prototype standard energy library data — climate-zone/vintage construction sets, whole-building and space-by-space program types, standard constructions/materials, standard schedules, building program mixes, and the HVAC/programtype vintage registries — dropped into `folders.standards_extension_folders`
- Accept: by-identifier resolution through the `honeybee-energy.lib` loaders; the `standards` extra of `dragonfly-energy`
- Reject: reading the extension JSON directly over the loaders; re-implementing the abridged-resolution order the lib loaders own; conflating this library with the `honeybee-standards` defaults floor; re-shipping the standard construction/program/schedule data; adding a code API to a data-only package; assuming `building_program_type_by_identifier` or an ASHRAE climate-zone construction set resolves without this extension present
