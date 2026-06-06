# [PYTHON_04_SCHEMA_METADATA_RESEARCH]

Python schema metadata should own field-level validation and generated-schema facts when a schema tool consumes them; Google docstrings should own only caller-visible semantics that annotations, field metadata, validators, and generated JSON Schema cannot carry. The active Python capsule already states this boundary for Pydantic, msgspec, and beartype; this report narrows the rule so future edits can distinguish field descriptions, validation docs, generated schema text, and sensitive data exposure.

## [1][SOURCE_BASIS]

Research scope: Pydantic, msgspec, beartype, stdlib dataclasses, `typing.Annotated` metadata, field descriptions, generated JSON Schema, validation docs, and secrets or data-exposure boundaries.
Source of truth: active Rasm standards, current official library documentation, Python 3.15 beta stdlib documentation, JSON Schema 2020-12 documentation, and OWASP API Security guidance.
Last verified: 2026-06-05.
Review trigger: Rasm changes its Python code-documentation capsule, adopts a Python API generator, changes Python schema libraries, or publishes generated schema artifacts.

Primary source set:
- Pydantic JSON Schema: `https://pydantic.dev/docs/validation/dev/concepts/json_schema/`.
- Pydantic fields API: `https://pydantic.dev/docs/validation/latest/api/pydantic/fields/`.
- Pydantic validators: `https://docs.pydantic.dev/latest/concepts/validators/`.
- Pydantic serialization: `https://pydantic.dev/docs/validation/dev/concepts/serialization`.
- Pydantic secret types: `https://pydantic.dev/docs/validation/latest/api/pydantic/types`.
- msgspec JSON Schema: `https://jcristharif.com/msgspec/jsonschema.html`.
- msgspec API, including `Struct`, `field`, `Meta`, and JSON Schema functions: `https://jcristharif.com/msgspec/api.html`.
- msgspec structs and rename rules: `https://jcristharif.com/msgspec/structs.html`.
- msgspec type inspection metadata: `https://jcristharif.com/msgspec/inspect.html`.
- beartype validators: `https://beartype.readthedocs.io/en/latest/api_vale/`.
- Python 3.15 `dataclasses`: `https://docs.python.org/3.15/library/dataclasses.html`.
- JSON Schema annotations: `https://json-schema.org/understanding-json-schema/reference/annotations`.
- JSON Schema validation vocabulary: `https://json-schema.org/draft/2020-12/json-schema-validation`.
- OWASP API Security project: `https://owasp.org/www-project-api-security/`.

Context7 lookup used official or source-backed documentation for `/pydantic/pydantic`, `/jcrist/msgspec`, and `/beartype/beartype`.

## [2][ACTIVE_STANDARD]

`docs/standards/reference/code-documentation.md:3` already sets the controlling rule: signatures, annotations, schemas, shell declarations, SQL objects, and catalogs own machine shape, while source comments own omitted caller-visible obligations, outcomes, failure channels, side effects, resource contracts, security exposure, lifecycle signals, and routes.

`docs/standards/reference/code-documentation.md:33-35` already says schema metadata can make a source comment unnecessary when it carries the complete caller-visible fact, and it rejects text that echoes a field name or type fact.

`docs/standards/reference/code-documentation.md:71-83` already separates machine shape, semantic contract, generated reference, and routed documentation, and it requires repository truth or command output before claiming configured generated artifacts.

`docs/standards/reference/code-documentation.md:230-258` already gives the Python capsule the needed core sentence: Pydantic, msgspec, and beartype metadata own schema-facing field descriptions, validation, aliases, strictness, immutability, generated JSON Schema, and runtime validation; docstrings own model purpose and cross-field invariants.

`docs/standards/proof.md:78-85` requires maintained current sources for changing tools and says unavailable current source must be marked as a gap.

`:131-135` requires generated or mirrored files to exclude secrets, personal data, task notes, and private machine details, and it separates public, internal, restricted, and secret material.

## [3][FINDINGS]

[F1][KEEP]: The active Python capsule's ownership rule is correct. Field-level facts consumed by Pydantic `Field`, msgspec `Meta`, msgspec `field`, dataclass `field(metadata=...)`, dataclass `field(doc=...)`, and beartype `Annotated` validators should not be duplicated in Google docstrings unless callers need semantics the field metadata cannot express.

[F2][CHANGE]: Split schema metadata into four owners. `Field` or `Meta` owns generated JSON Schema annotations and constraints; validators own executable validation behavior; generated JSON Schema owns public schema-facing field descriptions and examples; docstrings own model purpose, cross-field invariants, security or data-exposure obligations, and caller-visible failure/resource semantics that schema output does not express.

[F3][CHANGE]: Treat generated JSON Schema annotations as public documentation surfaces. JSON Schema `title`, `description`, `default`, and `examples` are annotations for documentation or UI display rather than validation; Pydantic and msgspec both emit descriptions and examples into generated schema output. Therefore schema metadata must not contain literal secrets, raw personal data, private tenant identifiers, credential routes, internal hostnames, nonpublic local paths, or real sensitive examples.

[F4][CHANGE]: Dataclasses need a narrower rule than Pydantic and msgspec. Stdlib `dataclasses.field(metadata=...)` is a read-only third-party extension mechanism and is not used by dataclasses itself; Python 3.14 added `field(doc=...)` as an optional per-field docstring. A dataclass field's metadata or `doc` owns generated schema only when a named third-party generator consumes that namespace and generated output proves it.

[F5][CHANGE]: Beartype should not be grouped with schema generators. Beartype validators are `typing.Annotated` runtime type hints used by `@beartype`; the current beartype docs state other static and runtime type checkers silently ignore beartype validators. No official beartype JSON Schema generator surfaced in the checked docs. Beartype metadata can own runtime validation constraints, but it does not own schema-facing field descriptions or generated JSON Schema unless another generator explicitly consumes those annotations.

[F6][CHANGE]: Pydantic model docstrings are a special case. Pydantic's JSON Schema docs show a model docstring becoming the schema `description` for the model, while `Field(description=...)` becomes the property description. The standard should allow class docstrings to carry model purpose when the generator consumes them, but still reject field-level schema prose buried only in docstrings.

[F7][CHANGE]: Validation docs should prefer executable metadata over prose. Pydantic field constraints and validators, msgspec `Meta` constraints, msgspec decoder options such as `forbid_unknown_fields`, and beartype validators are the validation owners. Docstrings should describe validation failures only when the public API exposes an exception, result rail, warning, or recovery contract that the validator and generated schema do not explain.

[F8][NO_CHANGE]: Do not add a generated Python API or schema rail to `code-documentation.md` from this report. Current local evidence from the neighboring generation report says the repo proves Google docstring style but does not prove configured Griffe, mkdocstrings, or generated Python API output in this checkout.

## [4][OWNER_MATRIX]

| [INDEX] | [SURFACE]                                | [OWNS]                                                                                                                                                       | [DOCSTRING_ROLE]                                                                                                                |
| :-----: | :--------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | Pydantic `Field(...)`                    | aliases, validation and serialization aliases, title, description, examples, constraints, strictness, exclusion, `json_schema_extra`, generated field schema | model purpose, cross-field invariant, caller obligation, security boundary, failure or resource semantics not carried by schema |
|   [2]   | Pydantic validators                      | field-level and model-level executable validation, coercion, mutation, and raised `ValidationError` detail                                                   | public failure or recovery semantics only when callers must act on them                                                         |
|   [3]   | msgspec `Meta(...)`                      | numeric, string, timezone, title, description, examples, `extra_json_schema`, and user metadata on a type or field                                           | concept or invariant not expressible as a single annotated field constraint                                                     |
|   [4]   | msgspec `Struct` config and `field(...)` | wire names, explicit field names, omit-default behavior, unknown-field policy, frozen or repr behavior                                                       | public encoding contract only when generated schema or API route does not expose it                                             |
|   [5]   | dataclass `field(metadata=...)`          | third-party extension data under that third party's namespace; not stdlib validation                                                                         | explain only the consuming generator or caller-visible invariant when configured output proves consumption                      |
|   [6]   | dataclass `field(doc=...)`               | optional per-field doc text exposed through dataclass field metadata                                                                                         | avoid duplicating it in class docstrings; use class docstrings for object purpose and invariants                                |
|   [7]   | beartype `Annotated[..., Is...]`         | runtime parameter and return constraints checked by `@beartype`                                                                                              | document caller-visible violation behavior only when exceptions are public contract                                             |
|   [8]   | generated JSON Schema                    | public machine-readable shape, constraints, annotations, defaults, examples, read/write hints, refs                                                          | route schema catalogs to generated or API docs; do not rebuild by hand in source docstrings                                     |

## [5][LIBRARY_NOTES]

[PYDANTIC]:
- `BaseModel.model_json_schema()` and `TypeAdapter.json_schema()` generate JSON Schema dictionaries, distinct from model serialization methods.
- `Field` carries `alias`, `validation_alias`, `serialization_alias`, `title`, `description`, `examples`, `exclude`, `exclude_if`, `json_schema_extra`, `frozen`, `validate_default`, `strict`, numeric constraints, length constraints, and union behavior in the current API.
- Pydantic's JSON Schema docs show a model docstring emitted as the model-level `description`, while field `description`, constraints such as `gt` and `lt`, aliases, and required fields are emitted from declarations and metadata.
- Pydantic's serialization docs make `Field(exclude=True)` and `Field(exclude_if=...)` field-level serialization owners, and field-level exclusion takes priority over runtime include parameters.
- Pydantic secret types mark `SecretStr` and `SecretBytes` as `writeOnly` in JSON Schema, and secret serialization/display can redact by default; explicit field serializers can expose plain text, so docstrings must not compensate for an unsafe serializer.

[MSGSPEC]:
- `msgspec.json.schema()` and `msgspec.json.schema_components()` generate JSON Schema 2020-12 and OpenAPI 3.1-compatible schemas from msgspec-compatible types and constraints.
- `msgspec.Meta` carries constraints and schema annotations such as `gt`, `ge`, `lt`, `le`, `multiple_of`, `pattern`, `min_length`, `max_length`, `title`, `description`, `examples`, `extra_json_schema`, and `extra`.
- msgspec inspection wraps annotated metadata in `msgspec.inspect.Metadata`, and JSON Schema-specific fields are merged into `extra_json_schema`.
- `Struct` configuration owns `rename`, `omit_defaults`, `forbid_unknown_fields`, `frozen`, and related wire or validation behavior. Explicit `msgspec.field(name=...)` takes precedence over `rename`.
- Struct class docstrings can become schema descriptions in official examples, but field descriptions belong in `Meta(description=...)` or another metadata path consumed by the generator.

[DATACLASSES]:
- The stdlib dataclass decorator examines type annotations to find fields, but with limited exceptions it does not examine the annotated type.
- `field(metadata=...)` is wrapped read-only and is explicitly provided as a third-party extension mechanism; dataclasses do not use it.
- `field(doc=...)` is an optional field docstring added in Python 3.14.
- `repr=False` and related dataclass field flags affect representation, but they are not a secret-management system, a validation rule, or a JSON Schema rule by themselves.
- A dataclass used as schema input needs a named generator owner. Without a generator, metadata is inert extension data and docstrings are ordinary Python docs.

[BEARTYPE]:
- Beartype validators use `typing.Annotated` with validator factories such as `beartype.vale.Is`, `IsAttr`, `IsEqual`, `IsInstance`, and `IsSubclass`.
- Functional validators can enforce arbitrary runtime constraints; declarative validators generate inline checks for narrower constraints.
- Beartype's latest validator docs say other static and runtime type checkers silently ignore beartype validators during type checking.
- Beartype validators are not field descriptions, public schema annotations, or generated JSON Schema by themselves. They own runtime validation only at `@beartype`-decorated boundaries.

[JSON_SCHEMA_SECURITY]:
- JSON Schema annotations such as `title`, `description`, `default`, and `examples` are descriptive metadata and can be used by documentation or UI generators; they are not the validation assertion itself.
- OWASP API Security describes APIs as exposing application logic and sensitive data, and the 2023 API3 category combines excessive data exposure and mass assignment under property-level authorization risk.
- Schema descriptions and examples should therefore name data class, format, unit, redaction expectation, and access class without publishing live examples of secrets, personal data, tenant identifiers, credential routes, or internal topology.

## [6][RECOMMENDATIONS]

[ADD][SCHEMA_METADATA_OWNER]:
Add a compact Python-specific ownership block under `[TYPE_TRUTH]` or immediately after the existing Pydantic/msgspec/beartype sentence:

```text conceptual
[SCHEMA_METADATA]:
- Pydantic `Field` and msgspec `Meta` own field descriptions, schema examples, aliases, generated JSON Schema annotations, and validation constraints when generated schema or runtime validation consumes them.
- Dataclass `field(metadata=...)` owns only the third-party extension namespace that consumes it, and dataclass `field(doc=...)` owns per-field doc text without implying validation or JSON Schema.
- Beartype `Annotated` validators own runtime parameter and return constraints at `@beartype` boundaries; they do not own generated JSON Schema unless a named generator consumes them.
- Docstrings own model purpose, cross-field invariants, security or data-exposure obligations, failure semantics, and resource contracts that metadata and generated schema cannot carry.
```

Reason: this preserves the current capsule's rule while preventing active editors from treating all metadata surfaces as interchangeable.

[ADD][PUBLIC_SCHEMA_EXPOSURE]:
Add a Python capsule safety sentence:

```text conceptual
Generated schema annotations are publishable documentation; keep literal secrets, personal data, tenant IDs, credential routes, internal hostnames, nonpublic paths, and real sensitive examples out of `description`, `examples`, `json_schema_extra`, dataclass `doc`, and generator-consumed metadata.
```

Reason: Pydantic and msgspec emit descriptions and examples into JSON Schema, and JSON Schema annotation docs frame those values as documentation or UI metadata.

[CHANGE][VALIDATION_WORDING]:
Refine the current sentence that groups Pydantic, msgspec, and beartype:

Before: `Pydantic, msgspec, and beartype metadata own schema-facing field descriptions, validation, aliases, strictness, immutability, generated JSON Schema, and runtime validation; docstrings own model purpose and cross-field invariants.`

After: Pydantic and msgspec metadata own schema-facing field descriptions, aliases, strictness, immutability, generated JSON Schema, and declarative validation; beartype `Annotated` validators own runtime validation at decorated boundaries; docstrings own model purpose, cross-field invariants, security exposure, resource obligations, and caller-visible failure semantics that schema metadata cannot carry.

Reason: beartype is a runtime validation library, not a schema metadata or JSON Schema generator in the checked official docs.

[ADD][REJECT_CASES]:
Add or preserve rejects for these Python shapes:
- field descriptions documented only in class docstrings when `Field(description=...)`, `Meta(description=...)`, dataclass `field(doc=...)`, or generator metadata owns the field;
- validation behavior described only in prose while the validator, constraint, alias, strictness, or unknown-field policy is absent from executable metadata;
- `Raises:` entries for ordinary Pydantic validation data unless the public API intentionally exposes native exceptions as the failure channel;
- schema examples containing real secrets, raw personal data, tenant identifiers, private hostnames, nonpublic paths, or live internal payloads;
- dataclass `metadata` described as validation or JSON Schema without naming the third-party consumer and generated output.

[ADD][VALIDATION_ITEM]:
Add one checklist item under `[COMMENTS_BOUNDARY]` only if the active editor touches the validation section:

```text conceptual
- [ ] Python schema fields use generator-consumed metadata for field descriptions, examples, aliases, constraints, and generated schema facts; docstrings carry only model purpose, cross-field invariants, security exposure, and caller-visible failure or resource semantics omitted by schema metadata.
```

Reason: this makes the split reviewable without introducing a new tool gate.

[REMOVE][NO_GENERATED_CATALOG]:
Do not add hand-maintained Pydantic or msgspec field catalogs to `code-documentation.md`. Generated JSON Schema or generated API reference should own those catalogs when configured, and `api.md` or generated output should carry the reader-facing reference.

## [7][NO_CHANGE_CONFIRMATIONS]

[NO_CHANGE][CORE_RULE]:
Keep the opening rule that code documentation exists only when a public caller needs semantics the declaration cannot express.

[NO_CHANGE][PYTHON_STYLE]:
Keep Google docstrings as the Python source-comment syntax. This report changes ownership boundaries, not docstring dialect.

[NO_CHANGE][GENERATED_PROFILE]:
Keep Griffe and mkdocstrings as adoptable generated-reference profiles only when generated Python API documentation is adopted. Do not claim configured output from this report.

[NO_CHANGE][SCHEMA_ROUTING]:
Keep generated or contract-backed API mirrors routed to `api.md` or generated API reference. Schema docs generated from Pydantic or msgspec should not be copied into source docstrings or hand-maintained Markdown tables.

[NO_CHANGE][DOCS_ONLY_GATES]:
No Python, static, generated-schema, test, bridge, or API-generation rail is required for this research report. A future wording-only standards edit likely needs `git diff --check -- docs/standards` and link or anchor validation only if links or headings change.

## [8][DRAFT_EDIT_MAP]

Primary owner: `docs/standards/reference/code-documentation.md` `[6.3][PYTHON_3_15]`.

Supporting owner: `` already owns generated or mirrored artifact separation and exclusion of secrets, personal data, task notes, and private machine details.

Proof owner: `docs/standards/proof.md` owns current-source, generated-artifact, and proof-gap labels.

Do not edit for this finding:
- `docs/standards/README.md`: routing already points code documentation to the reference standard.
- `docs/standards/AGENTS.md`: local standards editing rules already reject secrets, nonpublic machine paths, and metadata spam.
- `docs/standards/reference/api.md`: generated schema or API reference details belong there only after an active edit changes generated-reference routing.

## [9][CONFIDENCE]

[HIGH]:
- Pydantic field metadata and model docstrings affect generated JSON Schema, and `Field` owns aliases, descriptions, examples, constraints, exclusion, and `json_schema_extra`.
- msgspec `Meta` owns constraints and JSON Schema annotations, and msgspec JSON Schema utilities generate JSON Schema 2020-12 and OpenAPI 3.1-compatible output.
- Dataclass metadata is a third-party extension mechanism that dataclasses itself does not use, and `field(doc=...)` exists in Python 3.14+.
- JSON Schema annotation values are documentation or UI metadata, so real sensitive examples and private implementation details do not belong there.

[MEDIUM]:
- Pydantic secret-type guidance is enough to support a redaction rule, but individual framework serializers can bypass or reinterpret Pydantic serialization. Future active edits should word this as "metadata and serializers own exposure behavior" rather than "Pydantic always protects secrets."
- Beartype should be excluded from schema-facing field-description ownership. Official docs checked here did not surface a first-party JSON Schema generator, but a third-party generator could consume `Annotated` metadata later and would become the owner for that route.

[LOW]:
- Any claim about Rasm's configured Python API generation. This report did not find or run a configured generated-reference rail, and the neighboring generation report says local repo truth proves style, not generated Python API output.

## [10][TRANSCRIPT]

1. Read memory routing for recent `docs/standards` work and treated `_reports/` as report-only source material.
2. Discovered instruction and standards files with `fd`.
3. Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, `docs/standards/reference/code-documentation.md`, ``, `docs/standards/information-structure.md`, `docs/standards/proof.md`, `docs/standards/style-guide.md`, `docs/standards/formatting.md`, and `docs/standards/agents-md.md`.
4. Checked existing `_reports/` report style and confirmed `docs/standards/reference/code-documentation.md` was already modified before this report write.
5. Queried Context7 for Pydantic, msgspec, and beartype documentation.
6. Queried current primary web sources for Pydantic, msgspec, beartype, Python dataclasses, JSON Schema annotations, and OWASP API data-exposure guidance.
7. Wrote only `docs/standards/_reports/code-documentation-050626/track-python/04-python-schema-metadata.md`.

## [11][CLOSE_CHECK]

- [x] Read requested `docs/standards/reference/code-documentation.md` and governing standards.
- [x] Used current primary sources for Pydantic, msgspec, beartype, dataclasses, JSON Schema, and security exposure.
- [x] Edited only the assigned report file.
- [x] Left active standards and sibling reports untouched.
