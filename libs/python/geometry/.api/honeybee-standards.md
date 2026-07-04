# [PY_GEOMETRY_API_HONEYBEE_STANDARDS]

`honeybee-standards` is the baseline-defaults data backend for the geometry energy-modeling rail: a data-only package shipping the always-loaded `energy_default.json` (the generic default construction set, base materials/constructions, generic program types, base schedules, and schedule type limits) and `radiance_default.json` (the default radiance modifiers/modifier-sets), exposed as two filesystem-path module constants. It carries NO code API beyond those paths — its surface is the JSON data contract that `honeybee-energy`'s `lib._load*` modules read at import through `honeybee_energy.config.folders.defaults_file` to seed the by-identifier registries before any user or extension library loads. The energy-modeling owner never reads these JSON files directly; it resolves the defaults through the `honeybee-energy` `lib.*_by_identifier` loaders (the single access path), which merge this baseline with the optional `honeybee-energy-standards` extension folder. It is the floor of the standards band — the small guaranteed default set every honeybee energy model can resolve — not the large ASHRAE/DOE library (that is the separate `honeybee-energy-standards` extension).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-standards`
- package: `honeybee-standards`
- import: `import honeybee_standards` then `honeybee_standards.energy_default` / `honeybee_standards.radiance_default` (absolute paths to the bundled JSON); consumed indirectly through `honeybee_energy.config.folders.defaults_file`
- owner: `geometry`
- rail: energy-modeling
- consumer: `.planning/energy/model.md` — reached exclusively through the `honeybee_energy.lib` `RESOLVERS` rows, never a direct JSON read
- installed: `2.0.6`
- license: GPL-3.0 (the wheel declares the `GNU General Public License v3 (GPLv3)` classifier; the bundled `LICENSE` file conflictingly carries the MIT text and the `License` metadata field is unset — a Ladybug Tools metadata inconsistency, read as the conservative GPLv3)
- abi: pure-data `py3-none-any` wheel — JSON/IDF/MAT resource files plus a trivial `__init__.py` exposing two path constants; no compiled payload and no executable code
- depends-on: none (it is a leaf data package); `honeybee-energy` is its consumer, not its dependency
- entry points: none (no console scripts; data-only)
- capability: ships the baseline default energy library (`energy_default.json`: 1 construction set, 16 constructions, 16 materials, 2 program types, 8 schedules, 9 schedule type limits) and default radiance library (`radiance_default.json`: 19 modifiers, 4 modifier sets), plus empty `user_library` extension templates; provides the two filesystem paths the honeybee config layer reads to locate them

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: data path constants (`honeybee_standards`)
- rail: energy-modeling
- The entire code surface: two absolute-path strings to the bundled default-library JSON. There are no classes, functions, or types — the package is a resource carrier.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `honeybee_standards.energy_default`   | path constant | absolute path to `energy_default.json` (the always-loaded energy defaults) |
|  [02]   | `honeybee_standards.radiance_default` | path constant | absolute path to `radiance_default.json` (the always-loaded radiance defaults) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the data contract and the consumer seam
- rail: energy-modeling
- The package has no callable entry points; its "entry points" are the JSON data shape `honeybee-energy` reads and the config-layer path it resolves through. The owner accesses everything below via the `honeybee-energy` `lib.*_by_identifier` loaders, never by opening these files.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]             | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :----------------------- | :------------------------------------------------- |
|  [01]   | `energy_default.json` (= `honeybee_energy.config.folders.defaults_file`) | JSON document            | the abridged default library: keys `construction_sets`/`constructions`/`materials`/`program_types`/`schedules`/`schedule_type_limits` |
|  [02]   | `radiance_default.json`                                        | JSON document            | the default radiance library: keys `modifiers`/`modifier_sets` |
|  [03]   | data folders `constructions/` `constructionsets/` `programtypes/` `schedules/` `modifiers/` `modifiersets/` | `user_library.{json,idf,mat}` | empty user-extension templates (the per-domain user-library scaffold honeybee writes user objects into) |
|  [04]   | `honeybee_energy.lib.*_by_identifier(identifier)`             | identifier string        | the ONLY supported read path: resolves a default (or extension/user) object by identifier, seeding from this bundle at import |

## [04]-[IMPLEMENTATION_LAW]

[ENERGY_MODELING_STANDARDS]:
- import: `import honeybee_standards` only to obtain the two path constants if a tool must locate the bundle; the energy-modeling owner does not import it directly — the defaults reach the owner through `honeybee-energy`'s `lib` registries.
- defaults axis: `energy_default.json` is the abridged baseline library `honeybee_energy.config.folders.defaults_file` points at. At `honeybee_energy.lib` import, `_loadprogramtypes`/`_loadschedules`/`_loadconstructions`/`_loadmaterials`/`_loadconstructionsets`/`_loadtypelimits` read this file FIRST and build the default objects (locked, identifier-keyed), establishing the guaranteed-resolvable baseline (the generic default construction set, the generic office/plenum programs, the always-on and seated-activity schedules, the base schedule type limits). Every honeybee energy model can resolve these identifiers with no extra package.
- extension axis: AFTER the defaults, the same `_load*` modules scan `folders.standards_extension_folders` (default `~/ladybug_tools/resources/standards/`) for installed extension libraries — chiefly `honeybee-energy-standards`, the large ASHRAE 90.1 / DOE-prototype set (thousands of program types and schedules). That extension is a SEPARATE package, NOT honeybee-standards; the lib registry counts (e.g. ~1845 program types, ~3347 schedules — verified against `honeybee-energy-standards 2.3.0`, see `honeybee-energy-standards.md`) reflect the extension being installed, not this baseline bundle (which contributes 2 programs and 8 schedules). The owner treats the big library as an optional, separately-admitted extension and never conflates it with this defaults floor.
- radiance axis: `radiance_default.json` is the parallel baseline for the radiance daylight extension (`honeybee-radiance`), consumed the same way through its own lib loaders; this package ships both the energy and radiance default bundles.
- access-path axis: the data is abridged (objects reference each other by identifier within the file), so it is decoded through the same `from_dict_abridged` ordering `honeybee-energy` owns (schedule type limits -> schedules -> programs/constructions). The owner therefore resolves through `lib.*_by_identifier`, never by hand-parsing the JSON (which would re-implement the abridged-resolution order the lib loaders already own).
- boundary: honeybee-standards owns ONLY the baseline default data and its two locating paths. The loaders, the by-identifier resolution, and the registries are `honeybee-energy.lib`; the large standards library is the separate `honeybee-energy-standards` extension (`honeybee-energy-standards.md`) — which is also `dragonfly-energy`'s `standards` extra, so an urban district resolving ASHRAE/DOE identifiers pulls that extension, never this floor; the radiance loaders are `honeybee-radiance`; the object model is `honeybee-core`. This package contains no executable code; the urban `dragonfly-energy` consumer resolves this same defaults floor transitively through `honeybee-energy.lib`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `honeybee-standards`
- Owns: the always-loaded baseline default energy library (`energy_default.json`) and radiance library (`radiance_default.json`), the empty user-library extension templates, and the two filesystem-path constants that locate them
- Accept: serving the guaranteed default construction-set/program/schedule/material/type-limit floor that `honeybee-energy.lib` seeds at import; resolution exclusively through the `honeybee-energy` `lib.*_by_identifier` loaders
- Reject: reading `energy_default.json`/`radiance_default.json` directly over the `lib.*_by_identifier` loaders; re-implementing the abridged-resolution order the lib loaders own; conflating this baseline-defaults bundle with the large `honeybee-energy-standards` extension library; adding a code API to a data-only package; treating the registry counts as this package's contribution when the large library is the separate extension

[CAPTURE_GAP]:
- this package is the DEFAULTS floor, not the standards library (verified by `assay api` reflection against the installed `honeybee-standards 2.0.6`): `energy_default.json` contributes exactly 1 construction set, 16 constructions, 16 materials, 2 program types, 8 schedules, and 9 schedule type limits, and `radiance_default.json` 19 modifiers + 4 modifier sets — the small always-resolvable baseline. The large library (the ASHRAE 90.1 climate-zone construction sets, the DOE-prototype program types, the thousands of standard schedules) is the SEPARATE `honeybee-energy-standards` package installed into `folders.standards_extension_folders` (`~/ladybug_tools/resources/standards/honeybee_energy_standards`); a lib registry showing ~1845 program types means that extension is installed (now admitted; see `honeybee-energy-standards.md`), not that honeybee-standards provides them. A model that resolves only honeybee-standards identifiers needs no extension; a model referencing ASHRAE/DOE standard identifiers requires `honeybee-energy-standards` to be present.
- the JSON is abridged and load-ordered: objects within `energy_default.json` reference one another by identifier (programs reference schedules, constructions reference materials), so the file is only correctly materialized through the `honeybee-energy` lib loaders' ordered `from_dict_abridged` pass (type-limits -> schedules -> materials -> constructions -> construction-sets -> programs). Opening the JSON and calling a flat `from_dict` raises `KeyError` on the unresolved identifier references — the path constants exist to LOCATE the bundle, not to invite direct parsing.
