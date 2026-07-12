# [RASM_MATERIALS_API_VIVIDORANGE_STANDARDS]

`VividOrange.Standards` owns Eurocode design-code IDENTITY DATA as the concrete implementation of the `VividOrange.IStandards` `IStandard` interface floor for the EN/Eurocode body.

The ten typed `IStandard` records are `En1990` (basis of design), `En1991` (actions), `En1992` (concrete), `En1993` (steel, including Part 1-8 joints and Part 1-3 cold-formed), `En1994` (composite), `En1995` (timber), `En1996` (masonry), `En1997` (geotechnical), `En1998` (seismic), and `En1999` (aluminium). Each carries `StandardBody Body`, `NationalAnnex NationalAnnex`, and a derived human-readable `string Title`; every partitioned record also carries `En19xxPart Part`.

A structural material grade cites this design-code identity, and a design page names it instead of an inline code-number literal: `EnConcreteMaterial.Standard` is an `En1992`, and `EnSteelMaterial.Standard` is an `En1993` (`api-vividorange-materials.md`). The EN grade records construct an `En1992` or `En1993` in every constructor, so this package is the hard transitive floor of `VividOrange.Materials`. Every type implements `ITaxonomySerializable` and round-trips through `VividOrange.Serialization` (`api-vividorange-serialization.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Standards`

- package: `VividOrange.Standards`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Standards`
- namespace: `VividOrange.Standards.Eurocode`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `netstandard2.0` / `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the
  bound TFM — no `net9.0`+ asset, so the `api resolve` primary `net8.0` IS the consumed surface).
- rail: standards (design-code identity)
- ABI floor: a PRE-1.0 contract — the standard-class member set may break across a minor bump. The
  `IStandard` interface, the `StandardBody` enum, the ten `En19xxPart` enums (one per code), and the `NationalAnnex`
  enum live in the transitive `VividOrange.IStandards` floor (centrally pinned). Sole runtime dependency:
  `VividOrange.IStandards (`ITaxonomySerializable`rides it via the`VividOrange.ISerialization`floor) — this
package has NO`UnitsNet` dependency (it is pure identity DATA, no quantities).

[BODY_SCOPE_GATE]: the `VividOrange.IStandards` `StandardBody` floor enum declares ELEVEN bodies (AASHTO, ACI, AISC, ANSI, AS, BS, CSA, EN, HK, IS, SANS), but `VividOrange.Standards` implements ONLY the EN/Eurocode body. Every class is `Body => StandardBody.EN` and lives in the `.Eurocode` namespace.

This assembly has no BS, ACI, AISC, or AS standard class; a non-EN `StandardBody` is an UNADMITTED floor enum without a concrete implementation here. The composable design-code vocabulary is the EN/Eurocode set.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the ten Eurocode standard records (`IStandard` identity DATA)

- rail: standards
- contract: each pure-identity record implements `IStandard, ITaxonomySerializable` and carries constant `Body=EN`, its chosen `NationalAnnex`, and a derived human-readable `Title`.
- partition: each record carries its `En19xxPart`; `En1990` is the unpartitioned basis-of-design exception without `Part`.

| [INDEX] | [SYMBOL] | [DOMAIN]                     | [PART]       |
| :-----: | :------- | :--------------------------- | :----------- |
|  [01]   | `En1990` | basis of structural design   | none         |
|  [02]   | `En1991` | actions on structures        | `En1991Part` |
|  [03]   | `En1992` | concrete structures          | `En1992Part` |
|  [04]   | `En1993` | steel structures             | `En1993Part` |
|  [05]   | `En1994` | composite steel and concrete | `En1994Part` |
|  [06]   | `En1995` | timber structures            | `En1995Part` |
|  [07]   | `En1996` | masonry structures           | `En1996Part` |
|  [08]   | `En1997` | geotechnical design          | `En1997Part` |
|  [09]   | `En1998` | earthquake resistance        | `En1998Part` |
|  [10]   | `En1999` | aluminium structures         | `En1999Part` |

`En1992Part` spans 1-1 general, 1-2 fire, 2 bridges, and 3 containment. `En1993Part` includes `Part1_1` general, `Part1_3` cold-formed, `Part1_5` plated, `Part1_8` joints, `Part1_9` fatigue, `Part1_12` high-strength, and `Part2` or later for bridges and towers. Concrete and rebar grade standards cite `En1992`, and the steel grade standard cites `En1993`; material grades and design pages reference these records as design-code citations.

[PUBLIC_TYPE_SCOPE]: code-description utilities — NOT consumer-callable (`internal`)

- rail: standards
- gate: `NationalAnnexUtility` and the per-code `En1991Utility` through `En1999Utility` are `internal`; they compose each standard's `Title` from its part description and national-annex abbreviation.
- surface: consumers read the standard record's `Title` and do not call `NationalAnnexUtility.GetAbbreviation(na)` or `En1993Utility.GetPartDescription(part)` directly.
- identity: the 37-national-annex-body utility is consumable only through `Title`, never as a direct API.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cite a Eurocode standard

- rail: standards
- citation: a concrete material carries `En1992`, a steel material carries `En1993`, and `En1990` supplies the unpartitioned basis of design. `En1993Part.Part1_8` selects the joints code.
- family: `En19xx` denotes `En1991`, `En1994`, `En1995`, `En1996`, `En1997`, `En1998`, or `En1999` with the matching part enum and national annex.
- default: a partitioned parameterless constructor selects Part 1-1 and `NationalAnnex.RecommendedValues`.

| [INDEX] | [SURFACE]                                       | [KIND]      | [RESULT]               |
| :-----: | :---------------------------------------------- | :---------- | :--------------------- |
|  [01]   | `new En1992(En1992Part part, NationalAnnex na)` | constructor | concrete citation      |
|  [02]   | `new En1993(En1993Part part, NationalAnnex na)` | constructor | steel citation         |
|  [03]   | `new En1990(NationalAnnex na)`                  | constructor | basis citation         |
|  [04]   | `new En19xx(En19xxPart part, NationalAnnex na)` | constructor | code-specific citation |
|  [05]   | `new En19xx()`                                  | constructor | default identity       |
|  [06]   | `standard.Body`                                 | property    | `StandardBody.EN`      |
|  [07]   | `standard.Part`                                 | property    | `En19xxPart`           |
|  [08]   | `standard.NationalAnnex`                        | property    | `NationalAnnex`        |
|  [09]   | `standard.Title`                                | property    | `string`               |

## [04]-[IMPLEMENTATION_LAW]

[STANDARD_IDENTITY_ALGEBRA]:

- root: an `En19xx` is a pure IDENTITY record — `{ Body (constant EN), Part (its En19xxPart), NationalAnnex, Title (derived) }`,
  implementing the `IStandard` floor. It carries NO quantities and NO design rules — it is the design-code CITATION, the
  authority a material grade or a design check names.
- the `Part` axis: each code partitions into its `En19xxPart` enum (`En1992Part` = 4 parts general/fire/bridges/containment;
  `En1993Part` = ~14 parts incl. 1-1 general, 1-3 cold-formed, 1-5 plated, 1-8 joints, 1-9 fatigue, 1-12 high-strength;
  the rest one enum each). `En1990` is unpartitioned (the basis code has no `Part`).
- the annex axis: `NationalAnnex` is the `VividOrange.IStandards` floor enum — `RecommendedValues` (the EC standard
  values) + 36 country annexes (the EU members + UK + Singapore). A standard cites both its `Part` and its `NationalAnnex`;
  the material partial factors that vary by annex are read from the MATERIAL grade record (`api-vividorange-materials.md`),
  not from this identity record.
- `Title` derivation: the `Title` getter folds the code name, the part description from the `internal` `En19xxUtility.GetPartDescription`, and the national-annex abbreviation from the `internal` `NationalAnnexUtility.GetAbbreviation` into one human-readable string, the only consumer-visible projection of those internal tables.

[INTERNAL_UTILITY_GATE]:

- `NationalAnnexUtility` (the `NationalAnnex -> ISO abbreviation` table, e.g. Germany->"DIN", France->"NF", UK->"BSI")
  and the per-code `En19xxUtility` (the `En19xxPart -> part description` table) are `internal` — a consumer NEVER calls
  them. They feed the standard classes' `Title` getter; the public surface is the standard record + its `Title`. A page
  that needs a part description reads `standard.Title`, never a utility method.

[LOCAL_ADMISSION]:

- The standard identity is admitted at the Materials boundary that records a design-code CITATION — a material grade's
  governing code, a design check's basis. A grade record carries its `En1992`/`En1993` and a design page NAMES the
  `En19xx` + `NationalAnnex` instead of an inline "EN 1992-1-1" string or a code-number literal; the canonical Materials
  standard/identity concept maps the `IStandard` onto the design-code field at the edge.
- The standard is pure identity — it carries no partial factors or quantities; the factor DATA that varies by code +
  annex lives on the MATERIAL grade record (`api-vividorange-materials.md`). An owner reads the design-code IDENTITY
  here and the code-derived NUMBERS from the material grade, never duplicating either.

[STACK]:

- material seam: every `VividOrange.Materials` EN grade record CITES an `En19xx` — `EnConcreteMaterial.Standard` /
  `EnRebarMaterial.Standard` = `En1992`, `EnSteelMaterial.Standard` = `En1993` (`api-vividorange-materials.md`) — and a
  `NationalAnnex`; this package is the hard transitive floor of `VividOrange.Materials` (every grade ctor constructs an
  `En1992`/`En1993`). The standard IDENTITY and the material grade DATA meet at the `IStandard Standard` property.
- annex seam: the `NationalAnnex` enum a standard cites is the SAME floor enum the material partial-factor validation
  reads (`EnConcreteMaterial`/`EnRebarMaterial` validate the annex, `MinimumReinforcementSpacing(NationalAnnex)`,
  `api-vividorange-sections.md`) — one annex axis spans the standard citation and the material/spacing factor tables.
- design-page seam: a Materials design page (steel/timber/masonry/connection) NAMES the governing `En19xx` (e.g.
  `En1993Part.Part1_8` for a joint, `En1995` for timber) as the typed design-code citation, replacing an inline
  code-string literal — the standards owner the README/ARCHITECTURE design pages cite.
- wire seam: every standard is `ITaxonomySerializable` — it round-trips through `VividOrange.Serialization`
  `ToJson<T>`/`FromJson<T>` (`api-vividorange-serialization.md`) with its exact `En19xx` runtime type preserved via the
  `$type` tag, so a serialized material's `Standard` citation reconstructs precisely on decode.

[RAIL_LAW]:

- Package: `VividOrange.Standards` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract, sole
  dep `VividOrange.IStandards`, NO `UnitsNet`)
- Owns: the EN/Eurocode design-code IDENTITY DATA — the ten `En1990`..`En1999` typed `IStandard` records (`{ Body=EN;
En19xxPart Part; NationalAnnex; derived Title }`, `En1990` unpartitioned), the concrete impl of the
  `VividOrange.IStandards` floor for the EN body. The `IStandard`/`StandardBody`/`En19xxPart`/`NationalAnnex` types are
  the floor; the `NationalAnnexUtility`/`En19xxUtility` title-composition tables are `internal` ([INTERNAL_UTILITY_GATE]).
- Accept: a Eurocode design-code CITATION NAMED by its `En19xx` class + `En19xxPart` + `NationalAnnex`, carried as the
  typed `IStandard` a material grade cites or a design page names; read as pure identity (the code-derived factor DATA
  comes from the material grade, not here); admitted at the Materials standard/identity boundary.
- Reject: an inline "EN 1992-1-1" code string or a code-number literal where an `En19xx` record carries it; a non-EN
  `StandardBody` (no concrete in this EN-only assembly, [BODY_SCOPE_GATE]); a direct call to the `internal`
  `NationalAnnexUtility`/`En19xxUtility` instead of reading `standard.Title` ([INTERNAL_UTILITY_GATE]); duplicating the
  partial-factor/quantity DATA here (it lives on the material grade record).
