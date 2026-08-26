# [PY_GEOMETRY_API_HONEYBEE_ENERGY_STANDARDS]

`honeybee-energy-standards` owns the large ASHRAE 90.1 / DOE-prototype standard energy library as pure JSON data on the energy-modeling domain. It carries no Python code — its surface is the JSON contract dropped into `honeybee_energy.config.folders.standards_extension_folders`, resolved through the `honeybee-energy` `lib.*_by_identifier` loaders that merge it additively onto the `honeybee-standards` floor. `building_program_type_by_identifier` and an ASHRAE climate-zone `construction_set_by_identifier` require it; absent this extension only the small `honeybee-standards` floor resolves.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: data library layout (`honeybee_energy_standards/`)
- concern: energy-modeling
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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the data contract and the consumer boundary
- concern: energy-modeling
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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- At `honeybee_energy.lib` import the `_load*` modules read `folders.defaults_file` first (the `honeybee-standards` baseline), then scan `folders.standards_extension_folders` and merge this extension additively; the registries enumerate the union, so an ASHRAE/DOE identifier resolves only with this library present.
- Construction sets and programs partition by the 8 ASHRAE vintages (`pre_1980`, `1980_2004`, `2004`, `2007`, `2010`, `2013`, `2016`, `2019`); `construction_set_by_identifier` for a climate-zone/vintage set and `building_program_type_by_identifier` — backed by `programtypes_registry/<vintage>_registry.json` — resolve only from this library.
- Abridged objects reference materials and schedules by identifier, materialized only through the lib loaders' ordered `from_dict_abridged` pass (type-limits -> schedules -> materials -> constructions -> construction-sets -> programs); the owner resolves by identifier and never hand-parses the JSON.

[STACKING]:
- `honeybee-standards`(`.api/honeybee-standards.md`): the defaults floor this library layers onto — `folders.defaults_file` seeds the registries first, this extension merges on top through `folders.standards_extension_folders`.
- `honeybee-energy`(`.api/honeybee-energy.md`): the `lib.*_by_identifier` loaders and `_load*` scan modules are the sole resolution domain; this library's JSON becomes registry objects only across that boundary.
- `dragonfly-energy`(`.api/dragonfly-energy.md`): consumes this library as its `standards` extra, assigning realistic ASHRAE/DOE defaults to the urban model through the same `honeybee-energy.lib` resolvers.

[LOCAL_ADMISSION]:
- Admit for by-identifier resolution of ASHRAE/DOE standard constructions, construction-sets, programs, and schedules merged onto the `honeybee-standards` floor, and as the `standards` extra of `dragonfly-energy`; resolution enters exclusively through the `honeybee-energy.lib` loaders.
