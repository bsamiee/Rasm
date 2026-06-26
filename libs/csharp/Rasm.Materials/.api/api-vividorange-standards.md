# [RASM_MATERIALS_API_VIVIDORANGE_STANDARDS]

`VividOrange.Standards` supplies the Eurocode design-code IDENTITY DATA owner — the concrete implementation of
the `VividOrange.IStandards` `IStandard` interface floor for the EN/Eurocode body. It carries the ten Eurocode
classes `En1990` (basis of design), `En1991` (actions), `En1992` (concrete), `En1993` (steel, incl. Part 1-8
joints + Part 1-3 cold-formed), `En1994` (composite), `En1995` (timber), `En1996` (masonry), `En1997`
(geotechnical), `En1998` (seismic), `En1999` (aluminium) — each a typed `IStandard` record `{ StandardBody Body;
En19xxPart Part; NationalAnnex NationalAnnex; string Title; }` carrying its code part + national-annex identity and
a derived human-readable `Title`. It is the design-code IDENTITY a structural material grade CITES and a design
page NAMES instead of an inline code-number literal: an `EnConcreteMaterial.Standard` is an `En1992`, an
`EnSteelMaterial.Standard` is an `En1993` (`api-vividorange-materials.md`). It is the hard transitive floor of
`VividOrange.Materials` (the EN grade records construct an `En1992`/`En1993` on every ctor). Every type is
`ITaxonomySerializable` and round-trips through `VividOrange.Serialization` (`api-vividorange-serialization.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Standards`
- package: `VividOrange.Standards`
- version: `0.1.0`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Standards`
- namespace: `VividOrange.Standards.Eurocode`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `netstandard2.0` / `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the
  bound TFM — no `net9.0`+ asset, so the `api resolve` primary `net8.0` IS the consumed surface).
- rail: standards (design-code identity)
- ABI floor: a `0.1.0` PRE-1.0 contract — the standard-class member set may break across a minor bump. The
  `IStandard` interface, the `StandardBody` enum, the ten `En19xxPart` enums (one per code), and the `NationalAnnex`
  enum live in the transitive `VividOrange.IStandards` `0.1.0` floor (centrally pinned). Sole runtime dependency:
  `VividOrange.IStandards 0.1.0` (`ITaxonomySerializable` rides it via the `VividOrange.ISerialization` floor) — this
  package has NO `UnitsNet` dependency (it is pure identity DATA, no quantities).

[BODY_SCOPE_GATE]: the `VividOrange.IStandards` `StandardBody` floor enum declares ELEVEN bodies (AASHTO, ACI, AISC,
ANSI, AS, BS, CSA, EN, HK, IS, SANS), but `VividOrange.Standards` implements ONLY the EN/Eurocode body — every class
here is `Body => StandardBody.EN` and lives in the `.Eurocode` namespace. There is NO BS/ACI/AISC/AS standard class
in this assembly; a non-EN `StandardBody` is an UNADMITTED floor enum with no concrete here. The composable design-code
vocabulary from this package is the EN/Eurocode set.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the ten Eurocode standard records (`IStandard` identity DATA)
- rail: standards
- Each is a typed `IStandard` record: `Body` (constant `StandardBody.EN`), `Part` (its `En19xxPart` enum), `NationalAnnex`
  (the chosen annex), and a derived `Title` (the human-readable code + part description). `En1990` is the lone
  exception — the basis-of-design code is UNPARTITIONED, so it has no `Part`. They are pure identity (no quantities);
  the material grades + design pages reference them as the design-code citation.

| [INDEX] | [SYMBOL]  | [PACKAGE_ROLE]    | [CAPABILITY]                                                                                  |
| :-----: | :-------- | :---------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `En1990`  | basis standard    | `IStandard, ITaxonomySerializable` — basis of structural design; `{ Body=EN; NationalAnnex; Title }` (NO `Part` — unpartitioned) |
|  [02]   | `En1991`  | actions standard  | `IStandard, …` — actions on structures; `{ Body=EN; En1991Part Part; NationalAnnex; Title }` |
|  [03]   | `En1992`  | concrete standard | `IStandard, …` — design of concrete structures; `En1992Part` (1-1 general / 1-2 fire / 2 bridges / 3 containment) — the standard `EnConcreteMaterial`/`EnRebarMaterial` cite |
|  [04]   | `En1993`  | steel standard    | `IStandard, …` — design of steel structures; `En1993Part` (members `Part1_1` general, `Part1_3` cold-formed, `Part1_5` plated, `Part1_8` joints, `Part1_9` fatigue, `Part1_12` high-strength, … `Part2`+ bridges/towers) — the standard `EnSteelMaterial` cites |
|  [05]   | `En1994`  | composite standard | `IStandard, …` — design of composite steel-and-concrete structures; `En1994Part` |
|  [06]   | `En1995`  | timber standard   | `IStandard, …` — design of timber structures; `En1995Part` |
|  [07]   | `En1996`  | masonry standard  | `IStandard, …` — design of masonry structures; `En1996Part` |
|  [08]   | `En1997`  | geotechnical standard | `IStandard, …` — geotechnical design; `En1997Part` |
|  [09]   | `En1998`  | seismic standard  | `IStandard, …` — design of structures for earthquake resistance; `En1998Part` |
|  [10]   | `En1999`  | aluminium standard | `IStandard, …` — design of aluminium structures; `En1999Part` |

[PUBLIC_TYPE_SCOPE]: code-description utilities — NOT consumer-callable (`internal`)
- rail: standards
- gate: `NationalAnnexUtility` and the per-code `En1991Utility`..`En1999Utility` are `internal` to this assembly —
  they are the `Title`-composition + national-annex-abbreviation MECHANISM behind each standard's `Title` getter, NOT a
  consumer entrypoint. A consumer does NOT call `NationalAnnexUtility.GetAbbreviation(na)` or
  `En1993Utility.GetPartDescription(part)`; the only surface is the standard classes' `Title` property (which folds the
  part description + the national-annex abbreviation internally). They are documented here for the identity LAW, not as
  an API — the README/ARCHITECTURE framing of `NationalAnnexUtility` as a public "37 national-annex bodies" utility is
  a CONSUMABLE only via `Title`, never a direct call.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cite a Eurocode standard
- rail: standards

| [INDEX] | [SURFACE]                                                                                     | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new En1992(En1992Part part, NationalAnnex na)`                                                | constructor    | the concrete design code for a part + annex (the citation an `EnConcreteMaterial` carries) |
|  [02]   | `new En1993(En1993Part part, NationalAnnex na)`                                                | constructor    | the steel design code for a part + annex (e.g. `En1993Part.Part1_8` for joints, the citation an `EnSteelMaterial` carries) |
|  [03]   | `new En1990(NationalAnnex na)`                                                                 | constructor    | the basis-of-design code (unpartitioned — no `Part`)                             |
|  [04]   | `new En19xx(En19xxPart part, NationalAnnex na)`                                                | constructor    | any of `En1991`/`En1994`/`En1995`/`En1996`/`En1997`/`En1998`/`En1999` for its part + annex |
|  [05]   | `new En19xx()`                                                                                | constructor    | the default (Part 1-1 where partitioned, `NationalAnnex.RecommendedValues`)      |
|  [06]   | `standard.Body`                                                                                | property       | `StandardBody.EN` — the issuing body (constant for every class here)             |
|  [07]   | `standard.Part`                                                                                | property       | the `En19xxPart` enum — the code part (absent on `En1990`)                        |
|  [08]   | `standard.NationalAnnex`                                                                        | property       | the `NationalAnnex` enum — the chosen national annex (`RecommendedValues` + 36 countries) |
|  [09]   | `standard.Title`                                                                               | property       | `string` — the derived human-readable title (code name + part description + national-annex abbreviation) |

## [04]-[IMPLEMENTATION_LAW]

[STANDARD_IDENTITY_ALGEBRA]:
- root: an `En19xx` is a pure IDENTITY record — `{ Body (constant EN), Part (its En19xxPart), NationalAnnex, Title (derived) }`,
  implementing the `IStandard` floor. It carries NO quantities and NO design rules — it is the design-code CITATION, the
  authority a material grade or a design check names.
- the `Part` axis: each code partitions into its `En19xxPart` enum (`En1992Part` = 4 parts general/fire/bridges/containment;
  `En1993Part` = ~14 parts incl. 1-1 general, 1-3 cold-formed, 1-5 plated, 1-8 joints, 1-9 fatigue, 1-12 high-strength;
  the rest one enum each). `En1990` is unpartitioned (the basis code has no `Part`).
- the annex axis: `NationalAnnex` is the `VividOrange.IStandards` floor enum — `RecommendedValues` (the EC recommended
  values) + 36 country annexes (the EU members + UK + Singapore). A standard cites both its `Part` and its `NationalAnnex`;
  the material partial factors that vary by annex are read from the MATERIAL grade record (`api-vividorange-materials.md`),
  not from this identity record.
- `Title` derivation: the `Title` getter folds the code name + the part description (the `internal` `En19xxUtility.GetPartDescription`)
  + the national-annex abbreviation (the `internal` `NationalAnnexUtility.GetAbbreviation`) into one human-readable string —
  the only consumer-visible projection of those internal tables.

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
- Package: `VividOrange.Standards` `0.1.0` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract, sole
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
