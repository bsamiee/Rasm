# [PY_BRANCH_API_JSONSCHEMA]

`jsonschema` validates an instance against a FOREIGN JSON Schema document — the direction a `msgspec` schema emission cannot answer, which generates a document from a Python type where this owner grades arbitrary data against one someone else authored. Six draft validators ship as generated classes over one `create` factory, reference resolution belongs to the separate `referencing` registry, and format checking is opt-in with most checkers behind an extra this branch does not install. It is the payload gate beneath the Schema Registry JSON serializer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jsonschema`
- package: `jsonschema` (MIT)
- module: `jsonschema`
- namespaces: `jsonschema`, `jsonschema.{validators,exceptions,protocols}`; `_format`, `_types`, `_keywords`, `_legacy_keywords`, `_utils` are private
- target: pure Python over `attrs` classes; `py.typed` with inline annotations
- dependencies: `attrs`, `jsonschema-specifications`, `referencing`, `rpds-py` — all required, none optional
- extras: `[format]` pulls eight checker libraries and `[format-nongpl]` nine, ten across their union and neither set identical; NEITHER is installed, so the format vocabulary is the built-in twelve alone
- rail: schema-validation

Installation adds a `jsonschema` console script beside the library. `jsonschema.__version__` raises a `DeprecationWarning` on access and resolves through `importlib.metadata` instead.

## [02]-[PUBLIC_TYPES]

[VALIDATOR_SCOPE]: `jsonschema.validators`, the six re-exported at the package root

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :---------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Draft202012Validator`              | generated     | the current draft; the default `validator_for` fallback  |
|  [02]   | `Draft201909Validator`              | generated     | the prior draft, split vocabularies                      |
|  [03]   | `Draft7Validator` `Draft6Validator` | generated     | the two drafts every registry-fronted producer emits     |
|  [04]   | `Draft4Validator` `Draft3Validator` | generated     | legacy drafts carrying their own keyword implementations |
|  [05]   | `protocols.Validator`               | `Protocol`    | the structural contract all six satisfy                  |
|  [06]   | `SPECIFICATIONS`                    | `Registry`    | a `referencing` registry of the six meta-schemas         |
|  [07]   | `TypeChecker`                       | `attrs` class | the JSON-type predicate table a validator binds          |
|  [08]   | `FormatChecker`                     | class         | the opt-in `format` keyword dispatch table               |

Every validator class carries the same class attributes — `META_SCHEMA`, `VALIDATORS`, `TYPE_CHECKER`, `FORMAT_CHECKER`, `ID_OF` — so a caller reads a draft's keyword table off the class rather than a module constant.

[INSTANCE_SCOPE]: the members a bound validator answers

| [INDEX] | [SURFACE]                                      | [ANSWERS]                                                             |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `iter_errors(instance)`                        | a lazy generator of `ValidationError`, the ONLY accumulating surface  |
|  [02]   | `validate(instance)`                           | `None`, raising the FIRST error `iter_errors` yields                  |
|  [03]   | `is_valid(instance)`                           | `bool`, short-circuiting on the first error                           |
|  [04]   | `check_schema(schema, format_checker=<unset>)` | classmethod; raises `SchemaError` against the draft's own meta-schema |
|  [05]   | `evolve(**changes)`                            | a new validator sharing the compiled state, one field replaced        |
|  [06]   | `descend(instance, schema, ...)`               | the sub-schema recursion a custom keyword implementation drives       |
|  [07]   | `is_type(instance, type)`                      | the bound `TypeChecker` predicate                                     |
|  [08]   | `schema` `format_checker` `resolver`           | the bound fields; `resolver` is the deprecated pre-`referencing` slot |

`Draft202012Validator(schema, resolver=None, format_checker=None, *, registry=<20 resources>, _resolver=None)` is the constructor shape; `registry` is the `referencing` resolution surface and the only admitted one.

[FAULT_SCOPE]: `jsonschema.exceptions` — two roots and three orphans

| [INDEX] | [SYMBOL]                           | [BASES]               | [CARRIES]                                                             |
| :-----: | :--------------------------------- | :-------------------- | :-------------------------------------------------------------------- |
|  [01]   | `ValidationError`                  | `_Error`              | one instance breach                                                   |
|  [02]   | `SchemaError`                      | `_Error`              | one meta-schema breach of the schema document itself                  |
|  [03]   | `FormatError`                      | `Exception`           | one `format` checker refusal; NOT under `_Error`                      |
|  [04]   | `UnknownType` `UndefinedTypeCheck` | `Exception`           | a `type` keyword no bound `TypeChecker` answers                       |
|  [05]   | `_WrappedReferencingError`         | `_RefResolutionError` | an unresolvable `$ref`, its base rooted on `referencing.Unresolvable` |

`ValidationError` fields: `message`, `validator`, `validator_value`, `instance`, `schema`, `path`/`relative_path`/`absolute_path`, `schema_path`/`relative_schema_path`/`absolute_schema_path`, `context`, `cause`, `parent`, and the `json_path` property rendering `$.a.0`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module-level

| [INDEX] | [SURFACE]                                                          | [SHAPE]   | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------- | :-------- | :---------------------------------------------- |
|  [01]   | `validate(instance, schema, cls=None, *args, **kwargs)`            | function  | check the schema, then raise the BEST match     |
|  [02]   | `validators.validator_for(schema, default=<unset>)`                | function  | select the class off the schema's own `$schema` |
|  [03]   | `validators.create(meta_schema, validators=(), version=None, ...)` | function  | mint a validator class from a keyword table     |
|  [04]   | `validators.extend(validator, validators=(), version=None, ...)`   | function  | derive a class, replacing or removing keywords  |
|  [05]   | `validators.validates(version)`                                    | decorator | register a class in `SPECIFICATIONS` by draft   |
|  [06]   | `exceptions.best_match(errors, key=by_relevance())`                | function  | the most relevant error out of an iterable      |
|  [07]   | `exceptions.by_relevance(weak=frozenset({"anyOf","oneOf"}), ...)`  | function  | the relevance key `best_match` sorts on         |

`create` continues `type_checker`, `format_checker`, `id_of`, `applicable_validators`; `extend` takes `None` for the two tables it re-declares (`type_checker`, `format_checker`) and inherits `id_of` and `applicable_validators` implicitly.

[ENTRYPOINT_SCOPE]: `FormatChecker` and `TypeChecker`

| [INDEX] | [SURFACE]                                          | [SHAPE]   | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `FormatChecker(formats=None)`                      | ctor      | `None` binds every registered checker                 |
|  [02]   | `checks(format, raises=())` / `cls_checks(...)`    | decorator | register one checker; `raises` names its refusal type |
|  [03]   | `conforms(instance, format)` / `check(...)`        | instance  | the bool probe and the `FormatError`-raising twin     |
|  [04]   | `checkers`                                         | dict      | the live registry, mutated by every registration      |
|  [05]   | `TypeChecker.redefine(type, fn)` / `redefine_many` | instance  | a NEW checker; the value is immutable                 |
|  [06]   | `TypeChecker.remove(*types)` / `is_type(...)`      | instance  | drop a type, or run the predicate                     |

Built-in checkers, the twelve reachable with no extra installed: `date`, `date-time`, `email`, `idn-email`, `idn-hostname`, `ipv4`, `ipv6`, `json-pointer`, `regex`, `relative-json-pointer`, `time`, `uuid`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Format checking is OPT-IN and silent when absent: a validator constructed without `format_checker` ignores every `format` keyword entirely, so a document declaring `"format": "date-time"` validates a malformed string without complaint. Passing the class's own `FORMAT_CHECKER` is the arming step.
- Checkers whose library is absent are NOT REGISTERED, so `conforms` answers `True` for that format rather than raising. Neither extra installs here, so `hostname`, `uri`, `uri-reference`, `iri`, `duration`, `color`, and the rest silently pass and only the twelve built-ins check.
- `validate` runs `check_schema` FIRST and raises `SchemaError` on a malformed document, then folds the instance errors through `best_match`. `iter_errors` skips that meta-check, so a hand-driven leg validates against an unproven schema unless it runs `check_schema` itself.
- `iter_errors` is the ONLY accumulating surface: `validate` raises the first best match and `is_valid` short-circuits, so a caller wanting every breach in one pass drives the generator and folds it.
- Nested `anyOf`/`oneOf` breaches ride `ValidationError.context` as a sub-list rather than flattening, so a whole-tree walk descends `context` and a flat iteration reports the branch keyword alone.
- `best_match` DESCENDS into `context` and answers a leaf, so the returned error's `path` is the deepest one, not the branch's.
- Reference resolution is `referencing`'s whole: the `registry` keyword is the resolution surface, `resolver` is the deprecated pre-`referencing` slot, and an unresolvable `$ref` raises `referencing.Unresolvable` rather than any `jsonschema` exception a caller catches by name.
- `FormatError` derives from bare `Exception` and NOT from `_Error`, so an `except ValidationError` catch-all misses it; `UnknownType` and `UndefinedTypeCheck` sit outside both roots for the same reason.
- Validator classes are GENERATED by `create` and registered by `validates`, so `Draft202012Validator` has no source-declared body and its keyword table is the `VALIDATORS` mapping, editable only by `extend`.
- `TypeChecker` is immutable — `redefine`/`remove` answer a NEW checker — while `FormatChecker.checkers` is a live mutable dict, so `cls_checks` is a process-global registration.
- `validator_for` reads the schema's own `$schema` and falls back to `Draft202012Validator`, so a document declaring no `$schema` validates under the newest draft whatever draft its author wrote against.
- `evolve` shares the compiled keyword state, so replacing `schema` on a bound validator is cheaper than constructing a second one over the same registry.
- Deprecations: `jsonschema.__version__`, the `resolver` constructor slot with `RefResolver`, the package-root `ErrorTree` RE-EXPORT, and the module-level `draft*_format_checker` singletons, each warning on access; `jsonschema.exceptions.ErrorTree` is the undeprecated owning path and warns nothing.

[STACKING]:
- `confluent-kafka`(`.api/confluent-kafka.md`): `JSONSerializer`/`JSONDeserializer` compose this validator beneath the registry's magic-byte frame, so a registry-fronted JSON payload validates against the registered subject's own document and the framing never reaches a schema.
- `cloudevents`(`.api/cloudevents.md`): `dataschema` is an optional absolute URI identifying the payload schema; this owner grades the payload only after composition separately resolves that URI to an admitted schema document. Registry subject/version and protobuf `Any.type_url` remain independent configuration, while message-envelope attribute validation stays `core.v1.event.CloudEvent`'s.
- `msgspec`(`.api/msgspec.md`): `msgspec.json.schema` emits a document FROM a Python type while this owner grades data AGAINST a foreign one, so the two are inverse directions of one concern and neither substitutes for the other.

[LOCAL_ADMISSION]:
- Validators construct ONCE per schema document at admission and the bound value is what every payload crosses; a per-payload `validate(instance, schema)` re-runs `check_schema` and re-compiles the keyword table on every record.
- `check_schema` runs at admission on every foreign document, so a malformed schema refuses where it was fetched rather than on the first payload that happens to reach it.
- `iter_errors` is the admitted surface and its breaches accumulate onto the branch rail, since a first-error raise discards every other repair a producer owes.
- Each class's own `FORMAT_CHECKER` binds explicitly wherever a document declares `format`, and the branch states the twelve built-in checkers as its whole vocabulary rather than reading an unregistered format's `True` as conformance.
- `registry` is the one resolution surface; `RefResolver` and the `resolver` slot are refused.
- `referencing.Unresolvable` is caught by NAME beside `ValidationError` and `SchemaError`, since neither root reaches it, and `FormatError` likewise.

[RAIL_LAW]:
- Package: `jsonschema`
- Owns: JSON Schema instance validation across six drafts, meta-schema checking, the keyword and type tables a custom validator extends, format-checker dispatch, and error relevance ranking
- Accept: the six draft classes, `validator_for`, `iter_errors`/`check_schema`/`is_valid`, `create`/`extend`, `FormatChecker` with the class's own `FORMAT_CHECKER`, `TypeChecker`, `best_match`/`by_relevance`, the `registry` keyword
- Reject: `RefResolver` and the `resolver` slot; `ErrorTree`; the module-level `draft*_format_checker` singletons; `jsonschema.__version__`; a per-payload `validate(instance, schema)`; an unregistered format read as conformance; `except ValidationError` standing in for `FormatError` or `Unresolvable`
