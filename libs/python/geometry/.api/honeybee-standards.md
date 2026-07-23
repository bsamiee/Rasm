# [PY_GEOMETRY_API_HONEYBEE_STANDARDS]

`honeybee-standards` mints the always-loaded baseline defaults the energy-modeling rail resolves against — `energy_default.json` for the default energy library and `radiance_default.json` for the default radiance library — each exposed as an absolute-path module constant. It carries no code API; the surface is the JSON data contract `honeybee-energy` reads at import through `folders.defaults_file`, materialized only through the `lib.*_by_identifier` loaders. It is the guaranteed floor of the standards band, distinct from the large ASHRAE/DOE `honeybee-energy-standards` extension.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-standards`
- package: `honeybee-standards` (GPL-3.0)
- module: `honeybee_standards` — the `energy_default` and `radiance_default` absolute-path constants
- abi: pure-data `py3-none-any` wheel; JSON resource files and a two-constant `__init__`, no executable code
- rail: energy-modeling defaults floor, located through `honeybee_energy.config.folders.defaults_file`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the entire code surface — two module-level path constants, no classes or functions

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `honeybee_standards.energy_default`   | path constant | absolute path to `energy_default.json`, the energy defaults     |
|  [02]   | `honeybee_standards.radiance_default` | path constant | absolute path to `radiance_default.json`, the radiance defaults |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the JSON data contract and the loader seam — every default reaches the owner through the `honeybee_energy.lib.*_by_identifier` loaders, never a direct file read

| [INDEX] | [SURFACE]                             | [SHAPE]   | [CAPABILITY]                                                        |
| :-----: | :------------------------------------ | :-------- | :------------------------------------------------------------------ |
|  [01]   | `energy_default.json`                 | data file | abridged default energy library, located by `folders.defaults_file` |
|  [02]   | `radiance_default.json`               | data file | default radiance library (`modifiers`, `modifier_sets`)             |
|  [03]   | `user_library.{json,idf,mat}`         | template  | empty user-extension scaffold per domain                            |
|  [04]   | `honeybee_energy.lib.*_by_identifier` | loader    | sole read path; resolve a default by identifier                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `honeybee-energy.lib` reads `energy_default.json` first at import through `folders.defaults_file`, building the locked identifier-keyed default objects — the generic construction set, the generic office and plenum programs, the always-on and seated-activity schedules, and the base type limits — every energy model resolves with no extra package; `radiance_default.json` seeds `honeybee-radiance` the same way.
- Objects inside `energy_default.json` cross-reference by identifier, so the file materializes only through the loaders' ordered `from_dict_abridged` pass (type-limits -> schedules -> materials -> constructions -> construction-sets -> programs); a flat `from_dict` over the raw JSON raises `KeyError` on the unresolved references. Path constants locate the bundle, never inviting a direct parse.

[STACKING]:
- `honeybee-energy`(`.api/honeybee-energy.md`): `energy_default` flows into `honeybee_energy.config.folders.defaults_file`, which the `lib._load*` modules read at import to seed the `*_by_identifier` registries; the owner resolves every default through those loaders.
- `.planning/energy/model.md`: energy-modeling owner binds the seeded floor through `lib.*_by_identifier` under the `check_all` gate, admitting the large `honeybee-energy-standards` extension separately.

[LOCAL_ADMISSION]:
- Admitted as the leaf defaults-data backend of the energy-modeling rail; a model resolving only floor identifiers needs no extension, and a model referencing an ASHRAE 90.1 climate-zone construction set or a DOE-prototype whole-building program requires `honeybee-energy-standards` present under `folders.standards_extension_folders`.

[RAIL_LAW]:
- Package: `honeybee-standards`
- Owns: the always-loaded baseline default energy library (`energy_default.json`), the default radiance library (`radiance_default.json`), the empty user-library extension templates, and the two path constants that locate them
- Accept: seeding the guaranteed default construction-set, program, schedule, material, and type-limit floor `honeybee-energy.lib` loads at import; resolution through the `lib.*_by_identifier` loaders alone
- Reject: reading `energy_default.json` or `radiance_default.json` directly over the loaders; re-implementing the abridged load order the loaders own; conflating this defaults floor with the large `honeybee-energy-standards` extension; adding a code API to a data-only package
