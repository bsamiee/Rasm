# [H1][ASSAY_TYPE_SYSTEM]
>**Dictum:** *msgspec owns every wire and evidence shape; pydantic-settings owns env ingress only; one StrEnum instance is CLI token, wire value, and match key.*

Consolidates `research-msgspec.md`, `research-enum-typing.md`, `research-pydantic-core.md`, `research-pydantic-settings.md`, and the collapse list from `research-holistic-shapes.md`. Shard files retained until Wave 5.

---
## [1][TWO_SYSTEMS_NO_OVERLAP]

| [SYSTEM] | [OWNS] | [NEVER] |
| --- | --- | --- |
| `msgspec` | `Tool`, `Check`, `Report`, `Detail`, `Envelope`, `Artifact`, `Match`, `Fault`, `Routed`, `Bind` | Env, argv, `AssaySettings` |
| `pydantic-settings` | `AssaySettings` scalars/`Path` | Wire structs, CLI params |
| frozen `@dataclass` | Per-verb `Params` (Cyclopts) | Wire, config |
| `ty` + `beartype` | Static proof; two seam boundaries | Duplicate msgspec decode validation |

Ingress rule: env/flag → pydantic once → plain primitives into structs; wire → msgspec once; in-process construction is unchecked except `@checked` seams (`research-pydantic-core.md` §1).

---
## [2][ENUM_MASTERY]

Multi-payload `StrEnum.__new__` carries behavior; `_add_value_alias_` for wire aliases (`SKIP` → `"skipped"`). `RailStatus.join` is the fold monoid (`status.md` §3).

| [ENUM] | [PAYLOAD] | [HOME] |
| --- | --- | --- |
| `RailStatus` | `exit_code`, severity | `status.py` |
| `Runner` | `prefix` tuple | `model.py` |
| `Input` | `flag`, `scoped` | `model.py` |
| `Language` | `strategy` (`closure`/`glob`), `suffixes` | `model.py` — **no** standalone `Strategy` |
| `Mode` | `stream`, `writes` + operation kind | `model.py` — unified; retires CAPTURE/STREAM + `mutates` |
| `Claim` | (value only) | `model.py` |
| `ArtifactKind` | path/lease namespace | `model.py` |
| `Configuration` | Debug/Release | `settings.py` |

Exhaustive dispatch: `match` + `assert_never`; Cyclopts derives choices from members; msgspec encodes `_value_`.

---
## [3][MSGSPEC_POLICY]

```python
class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True): ...
class Detail(Base, forbid_unknown_fields=True, tag_field="kind"): ...
class Envelope(Base, omit_defaults=False, kw_only=True): ...
```

| [FLAG] | [ASSAY] |
| --- | --- |
| `frozen` / `gc=False` | All structs; acyclic graph |
| `omit_defaults` | `Base=True`; `Envelope` opts out for `schema_version` |
| `forbid_unknown_fields` | `Detail` only — drift fails loud |
| `tag` / `tag_field` | Explicit short tags: `verify`, `package`, `api`, `test` |
| `defstruct` | Irregular evidence from catalog metadata |
| `Raw` | Opaque bridge JSON (optional) |

Tagged union: one-pass `kind` decode; `None` admitted on `Report.detail`. Direct struct construction does not enforce `Meta` — constraints apply at decode (`research-msgspec.md` §1).

---
## [4][SHAPE_CATALOG_POST_COLLAPSE]

**KEEP:** `Tool`, `Check`, `Completed`, `Fault`, `Report`, `Artifact`, `Match`, `Envelope`, `Counts`, `Routed`, `Bind`, four `Detail` variants.

**RETIRED:** 14 per-rail reports, `DotnetInvocation`, free `Literal` aliases, `Report.counts: dict`, `ApiSurface.source: dict`, `Envelope.data`/`rail`, `NamedTuple` params, `CliSettingsSource`, `Engine`/`Parser` Protocols.

**DO NOT MERGE:** `Completed`≠`Detail` (pre-fold bytes vs wire proof); `Artifact`≠`Match` (file vs ranked text); `Fault`≠`Detail` (error channel vs success evidence).

---
## [5][PYDANTIC_SETTINGS_SURFACE]

```python
def settings_customise_sources(...):
    return (init_settings, env_settings)  # no CliSettingsSource, no dotenv by default
```

- `Configuration(StrEnum)` replaces `Literal["Debug","Release"]`.
- `artifact(kind: ArtifactKind, *parts)` only — no `*_lock` properties.
- Cyclopts → registry → `AssaySettings(**overrides)` as `init_settings`.
- `AliasChoices` must include `ASSAY_*` prefixed names explicitly.

---
## [6][CLI_PARAMS]

Frozen `@dataclass` per verb (`TID251` bans `NamedTuple`). Cyclopts `Parameter(name="*")` flattens fields. Wire stays `msgspec`.

---
## [7][VERIFICATION_LAWS]

- `encode(decode(x)) == x` for `Report`/`Envelope`/`RailStatus` aliases.
- Unknown `detail` field → `ValidationError`.
- No `Literal` alias for bounded vocabularies already on a `StrEnum`.

---
## [FURTHER_CONSIDERATION]

- Fold monoid: fuse `RailStatus.join` + `Counts` in one `reduce` over outcomes.
- `msgspec.Raw` on bridge fields avoids double JSON for C# facts.
- `cache_hash=True` on `Tool` only if rows become dict keys.
