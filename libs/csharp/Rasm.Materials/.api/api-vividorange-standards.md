# [RASM_MATERIALS_API_VIVIDORANGE_STANDARDS]

`VividOrange.Standards` owns the EN/Eurocode design-code IDENTITY DATA — the concrete `VividOrange.IStandards` `IStandard` implementation for the EN body, one typed class per Eurocode. Each class is pure citation identity carrying no quantity, so a material grade or design page names the governing code here and reads every code-derived factor from the material grade record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Standards`
- package: `VividOrange.Standards` (MIT)
- assembly: `VividOrange.Standards`
- namespace: `VividOrange.Standards.Eurocode`
- asset: pure-managed AnyCPU runtime library, no native RID; the `net10.0` consumer binds the `lib/net8.0` managed asset.
- depends: `VividOrange.IStandards` alone — the floor owning `IStandard`, `StandardBody`, the `En19xxPart` enums, `NationalAnnex`, and `MissingNationalAnnexException`; no `UnitsNet`, since this surface carries identity, not quantities.
- rail: standards (design-code identity)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the ten Eurocode `IStandard` identity classes
- rail: standards
- contract: each class implements `IStandard, ITaxonomySerializable`, carrying constant `Body=EN`, a `NationalAnnex`, and a derived `Title`.
- partition: each partitioned class carries its `En19xxPart`; `En1990` is the unpartitioned basis-of-design exception.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :------- | :------------ | :--------------------------- |
|  [01]   | `En1990` | class         | basis of structural design   |
|  [02]   | `En1991` | class         | actions on structures        |
|  [03]   | `En1992` | class         | concrete structures          |
|  [04]   | `En1993` | class         | steel structures             |
|  [05]   | `En1994` | class         | composite steel and concrete |
|  [06]   | `En1995` | class         | timber structures            |
|  [07]   | `En1996` | class         | masonry structures           |
|  [08]   | `En1997` | class         | geotechnical design          |
|  [09]   | `En1998` | class         | earthquake resistance        |
|  [10]   | `En1999` | class         | aluminium structures         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct and read a Eurocode citation
- rail: standards

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------- | :------- | :--------------------------- |
|  [01]   | `En19xx(En19xxPart, NationalAnnex)` | ctor     | partitioned code citation    |
|  [02]   | `En1990(NationalAnnex)`             | ctor     | unpartitioned basis citation |
|  [03]   | `En19xx()`                          | ctor     | default-valued identity      |
|  [04]   | `.Body`                             | property | `StandardBody.EN`            |
|  [05]   | `.Part`                             | property | `En19xxPart`                 |
|  [06]   | `.NationalAnnex`                    | property | `NationalAnnex`              |
|  [07]   | `.Title`                            | property | derived `string`             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- An `En19xx` is pure identity — constant `Body`, its `En19xxPart`, a `NationalAnnex`, and a derived `Title` — implementing the `IStandard` floor with no quantity and no design rule, the citation a grade or design check names.
- Each code partitions on its own `En19xxPart` enum; `En1990` alone is unpartitioned. `NationalAnnex` is the `VividOrange.IStandards` floor enum spanning `RecommendedValues` and the national annexes.
- `StandardBody` is a floor enum spanning many bodies; this assembly implements only the EN case under `.Eurocode`, every class returning `Body => StandardBody.EN`. A non-EN `StandardBody` names a floor case with no concrete type here.
- `Title` is the sole consumer-visible projection of the `internal` tables: its getter folds the code name, the `En19xxUtility.GetPartDescription` part text, and the `NationalAnnexUtility.GetAbbreviation` annex abbreviation into one string. Both table families are `internal`, so a consumer reads `standard.Title`, never a utility method.

[STACKING]:
- `VividOrange.Materials`(`api-vividorange-materials.md`): every EN grade ctor constructs an `En19xx` — `EnConcreteMaterial.Standard`/`EnRebarMaterial.Standard` is `En1992`, `EnSteelMaterial.Standard` is `En1993` — so this package is the hard transitive floor of the grade records, identity meeting grade DATA at the `IStandard Standard` property while code-derived factors read from the grade.
- `VividOrange.Sections`(`api-vividorange-sections.md`): the `NationalAnnex` a standard cites is the same floor enum `MinimumReinforcementSpacing(NationalAnnex)` reads, so one annex axis spans the code citation and the spacing/factor tables.
- `VividOrange.Serialization`(`api-vividorange-serialization.md`): each class is `ITaxonomySerializable` and round-trips through `ToJson<T>`/`FromJson<T>`, the concrete `En19xx` runtime type preserved by the `$type` tag so a serialized `Standard` citation decodes precisely.
- within-lib: a Materials design page names the governing `En19xx` (`En1993Part.Part1_8` for a joint, `En1995` for timber) as the typed citation in place of an inline `"EN 1992-1-1"` string.

[LOCAL_ADMISSION]:
- Admit the identity at the Materials boundary that records a design-code citation — a grade's governing code, a design check's basis; the edge maps the `IStandard` onto the design-code field, and a design page names the `En19xx` + `NationalAnnex` in place of a code-number literal.

[RAIL_LAW]:
- Package: `VividOrange.Standards`
- Owns: the EN/Eurocode design-code identity DATA — the ten `En1990`..`En1999` `IStandard` classes (constant `Body=EN`, `En19xxPart Part`, `NationalAnnex`, derived `Title`; `En1990` unpartitioned), the EN concrete implementation of the `VividOrange.IStandards` floor.
- Accept: a Eurocode citation named by its `En19xx` class + `En19xxPart` + `NationalAnnex`, carried as the typed `IStandard` a grade cites or a page names, read as pure identity.
- Reject: an inline `"EN 1992-1-1"` string or code-number literal where an `En19xx` carries it; a non-EN `StandardBody`, unimplemented in this assembly; a direct call to the `internal` `NationalAnnexUtility`/`En19xxUtility` in place of `standard.Title`; duplicating on this record the code-derived factor DATA the material grade owns.
