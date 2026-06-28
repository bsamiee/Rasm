# [ELEMENT_CLASSIFICATION]

The two neutral cross-cutting axes every node and assessment keys on: the generic `Classification` `[ComplexValueObject]` (`System` + `Code`) an `Object` node carries, and the one `Discipline` `[SmartEnum<string>]` (`Structural`/`Thermal`/`Energy`/`Acoustic`/`Fire`/`Environmental`/`Cost`) a `MaterialPropertySet` case and an `Assessment` node key on. Both are LIBRARY-NEUTRAL: `Classification` is a `(system, code)` pair with NO standard-systems roster, NO bSDD dictionary, and NO code-shape regex — a downstream `Rasm.Bim` projector owns the standard-systems vocabulary, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto a seam `Classification` value at ingest; the seam carries only the typed pair so a consumer reads `element.Classification.System`/`Code` without learning the IFC schema. The `IfcClass` entity-class roster and the `PredefinedType` valid-set live in `Rasm.Bim` — they are an IFC-schema concern, not a seam axis. `Discipline` is the single discriminant the typed property, the assessment payload, and the analysis route all share: a `MaterialPropertySet.Thermal` is the `Discipline.Thermal` row, an `Assessment` keyed `Discipline.Energy` carries an energy-simulation receipt, and a future analysis discipline is one row, never a parallel enum per consumer.

## [01]-[INDEX]

- [01]-[CLASSIFICATION_AXIS]: the `Classification` `[ComplexValueObject]` system-and-code pair an `Object` node carries, admitted once, with the parent-code derivation and the `Within` containment predicate.
- [02]-[DISCIPLINE_AXIS]: the `Discipline` `[SmartEnum<string>]` seven-row analysis-discipline vocabulary the typed property, the assessment, and the analysis route key on.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `Classification` the `[ComplexValueObject]` carrying the `System` token (the standard identifier — `"uniclass2015"`, `"omniclass"`, `"masterformat"`, a free string the seam never enumerates) and the `Code` (the code-within-the-system, trimmed), the generic system-and-code pair an `Graph/element#NODE_MODEL` `Object` node carries; a downstream projector resolves the standard-systems roster and the bSDD dictionary and lowers the resolved code onto this value.
- Entry: `Classification.Create(system, code)` admits the pair (both trimmed, neither blank) through the generated `[ComplexValueObject]` factory — `Validate`/`TryCreate` carry the admission evidence and a blank system or code is a `ValidationError`; `Parent` derives the containing code by trimming the last `_`/`-`/` ` segment so a `"Ss_25_10_30"` resolves its `"Ss_25_10"` parent without a per-call split; `Within(other)` tests system equality and code-prefix containment so a query selects every code under a branch.
- Auto: the `[ComplexValueObject]` generates structural equality over the `(System, Code)` pair under the seam string comparer policy (ordinal system, ordinal-ignore-case code), the `Validate` admission trimming both members and rejecting a blank, and the diagnostic `ToString` rendering `"{System}:{Code}"`; `Parent` and `Within` are pure projections over the admitted code so the containment hierarchy is derived, never a stored parent reference.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`), LanguageExt.Core (`Option`).
- Growth: a new standard system is one `System` token a projector supplies (the seam never grows a row); a new code-derivation rule is one projection on `Classification` (the `Parent`/`Within` family); never a per-system value-object and never a standard-systems roster on the seam — the roster, the bSDD dictionary, and the code-shape policy are the downstream projector's, the seam carrying only the typed pair.
- Boundary: `Classification` is ONE generic value-object — a per-system `Uniclass`/`OmniClass` type or an `IfcClass`-style entity-class roster on the seam is the deleted form (the `IfcClass` roster and the `PredefinedType` valid-set are `Rasm.Bim`'s IFC-schema concern, never lowered into the seam); the `System` is an opaque token the seam never validates against a roster so a new standard needs no seam edit, the projector validating the code shape and resolving the bSDD class at ingest before lowering onto this value; the `Object` node carries the typed `Classification` so a query matches a system-and-code pair (`Within` for a branch) rather than a stringly-keyed property lookup, and a free `string` classification field on a node is the named defect; the `Code` parent-derivation is a pure projection over the admitted code, never a stored parent edge or a per-call regex.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class Classification {
 public string System { get; }
 public string Code { get; }

 static partial void NormalizeValidate(ref string system, ref string code) {
 system = system.Trim().ToLowerInvariant();
 code = code.Trim();
 }

 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string system, ref string code) =>
 validationError = string.IsNullOrWhiteSpace(system) || string.IsNullOrWhiteSpace(code)
 ? new ValidationError($"classification requires a non-blank system and code; got '{system}':'{code}'")
 : null;

 // Containment is derived from the code, never a stored parent edge: a hierarchical code's
 // parent is its prefix up to the last separator; a flat code has no parent.
 public Option<Classification> Parent =>
 Code.LastIndexOfAny(['_', ' ', '-', '.']) is var cut and > 0
 ? Some(Create(System, Code[..cut]))
 : None;

 // System-equal prefix containment: every code under a branch matches the branch's Within.
 public bool Within(Classification branch) =>
 string.Equals(System, branch.System, StringComparison.Ordinal)
 && Code.StartsWith(branch.Code, StringComparison.OrdinalIgnoreCase);
}
```

## [03]-[DISCIPLINE_AXIS]

- Owner: `Discipline` the `[SmartEnum<string>]` seven-row analysis-discipline vocabulary keyed on a stable lowercase token, each row carrying its `Title` display name and its `Physical` flag (a measured physics discipline versus a procurement/lifecycle one) — the single discriminant a `Composition/material#MATERIAL_NODE` `MaterialPropertySet` case maps to, an `Assessment/assessment#ASSESSMENT_NODE` node keys on, and the `Rasm.Compute` analysis route selects by.
- Entry: the declaration list IS the vocabulary; `Discipline.Get(key)`/`TryGet(key)` resolve a row from its wire token, `Discipline.Items` enumerates the seven, and the `[SmartEnum<string>]` generated key codec round-trips the token at the wire so an `Assessment` node persists `Discipline.Energy` as `"energy"` and re-admits it.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`).
- Growth: a new analysis discipline is one `Discipline` row carrying its token, title, and physical flag — a damping/seismic/daylight discipline lands as one row the `MaterialPropertySet`/`Assessment`/analysis route all read; never a parallel discipline enum per consumer and never a `bool IsThermal` flag set the implementation re-derives.
- Boundary: `Discipline` is the ONE analysis axis — a `StructuralDiscipline`/`ThermalDiscipline` parallel enum or a per-consumer discipline string is the deleted form; the seven rows are the closed set the seam owns, a consumer dispatching on `Discipline` through the generated `Switch` rather than a `string` compare; the `MaterialPropertySet` case-to-discipline map is the one correspondence (`Mechanical`→`Structural`, `Thermal`→`Thermal`, `Acoustic`→`Acoustic`, `Fire`→`Fire`, `Environmental`→`Environmental`, `Cost`→`Cost`), `Energy` the discipline carried only on an `Assessment` node (a whole-element simulation has no intrinsic single-material property), so the axis is total over both the property family and the assessment family without a parallel vocabulary.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Discipline {
 public static readonly Discipline Structural = new("structural", "Structural", physical: true);
 public static readonly Discipline Thermal = new("thermal", "Thermal", physical: true);
 public static readonly Discipline Energy = new("energy", "Energy", physical: true);
 public static readonly Discipline Acoustic = new("acoustic", "Acoustic", physical: true);
 public static readonly Discipline Fire = new("fire", "Fire", physical: true);
 public static readonly Discipline Environmental = new("environmental", "Environmental", physical: false);
 public static readonly Discipline Cost = new("cost", "Cost", physical: false);

 public string Title { get; }
 public bool Physical { get; }
}
```

## [04]-[RESEARCH]

- [CLASSIFICATION_NEUTRALITY]: the seam `Classification` carries the `(system, code)` pair only — the standard-systems roster (`Uniclass2015`/`OmniClass`/`MasterFormat`/`Uniformat`), the bSDD dictionary URI, the code-shape regex, and the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip stay the `Rasm.Bim` `Semantics/classification` projector's, which resolves a code against the live bSDD service and lowers the resolved `(system, code)` onto a seam `Classification` value at ingest; the seam never re-opens the IFC-schema strata, and the `Object` node carries the generic pair rather than the `IfcClass` entity-class enum, which is a Bim egress concern validated at `Emit`.
- [DISCIPLINE_TOTALITY]: the seven `Discipline` rows are total over both the `MaterialPropertySet` `[Union]` (six physical/lifecycle cases) and the `Assessment` payload (any discipline, including `Energy`), so the analysis route the `Rasm.Compute` solver selects, the typed material property, and the assessment receipt share one axis; the `Physical` column distinguishes a measured-physics discipline (a solver input) from a procurement/lifecycle one (a catalog lookup) so the analysis dispatcher routes by the column rather than a per-discipline branch.
