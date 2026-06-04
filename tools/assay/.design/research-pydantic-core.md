# [H1][PYDANTIC_CORE_BOUNDARY_RESEARCH]
>**Dictum:** *msgspec owns every value; pydantic-core touches only the env/flag ingress. No value is modelled, validated, or shaped by both.*

Verified against msgspec upstream (struct/tagged-union/validation semantics) and pydantic v2 + pydantic-settings upstream (TypeAdapter, Discriminator, source precedence). The governing risk is a dual paradigm: two libraries modelling one value. The ruling below makes that structurally impossible, not merely discouraged.

---
## [1][BOUNDARY_RULING]
>**Dictum:** *Four validators, four disjoint ingresses, fired at most once per value.*

| [LAYER] | [OWNS] | [FIRES] | [WHY IT, NOT THE OTHERS] |
| --- | --- | --- | --- |
| `ty` | Static shape proof of all layers | Compile time | Erases nothing at runtime; cost-free; covers struct construction msgspec leaves unchecked. |
| `pydantic-settings` | Env + flag ingress → config scalars/`Path` | Once, at `AssaySettings()` load | Lax env coercion, `AliasChoices`, source precedence, `@computed_field`, `SecretStr` — capabilities msgspec deliberately lacks. |
| `msgspec` decode/convert | Wire ingress (external JSON: bridge facts, subprocess stdout, round-tripped `Envelope`/`Report`) | Once, at `json.decode`/`convert` | C-level single-pass validate-on-decode; `Meta` constraints + `forbid_unknown_fields` enforced here and **only** here. |
| `beartype` | In-process struct seams (`run_check`, runner) | Per call | Catches the gap below: `Tool`/`Check`/`Completed` are *constructed*, never decoded, so msgspec never validates them. |

**The linchpin (upstream-confirmed):** `msgspec.Struct(...)` direct construction performs **no** type check and **ignores all `Meta` constraints**; validation runs solely in `decode`/`convert` ([structs](https://jcristharif.com/msgspec/structs.html#type-validation), [type-conversion](https://deepwiki.com/jcrist/msgspec/4.2-type-conversion-and-constraints), maintainer in [#812](https://github.com/jcrist/msgspec/issues/812)). Therefore `Tool`/`Check`/`Report` built in-process (catalog rows, fold results) get their `Meta(gt=0, ge=0, le=100)` enforced *never* at runtime — those constraints are `ty`/doc hints. Only externally-decoded shapes (`Detail` from a drifting emitter, bridge payloads) are truly validated. This is the exact seam that forbids double-validation: a config scalar validated once by pydantic flows as a plain `int`/`float`/`Path` into a struct that does not re-validate it.

**No-overlap rule:** a value crosses exactly one ingress. Env/flag → pydantic (once) → emitted as primitive → consumed by msgspec structs unchecked. Wire → msgspec (once). Neither library ever declares the other's value; `AssaySettings` never embeds a `Struct`, and no `Struct` reads env.

---
## [2][WHERE_PYDANTIC_CORE_EARNS_ITS_PLACE]
>**Dictum:** *Use msgspec unless the value is born from the environment or needs coercion msgspec refuses.*

The definitive rule: **use `msgspec` for every value, except one whose source is the process environment/flags or whose validation needs lax coercion, alias resolution, computed derivation, secret masking, or chained custom validators — that value, and only that value, is pydantic-settings.** Concretely:

- `TypeAdapter` ([docs](https://pydantic.dev/docs/validation/latest/concepts/type_adapter/)) is the *only* pydantic-core surface that could appear outside `AssaySettings` — a one-off validated parse of a non-struct env-adjacent scalar (e.g. a JSON-encoded `ASSAY_*` list). **Reject it anyway:** pydantic-settings already parses complex field types from env, so a standalone `TypeAdapter` would re-open the env ingress outside the one settings surface. Any wire-shaped one-off belongs to `msgspec.convert`/`defstruct`, not `TypeAdapter`.
- `BaseModel`/`pydantic.dataclass` outside `AssaySettings`: **never.** That is the dual-paradigm failure mode by definition.
- msgspec wins outright for: speed (~6–15x decode, ~15–30x encode, ~17x construction vs pydantic v2 — [benchmarks](https://jcristharif.com/msgspec/benchmarks.html), [gist](https://gist.github.com/jcrist/d62f450594164d284fbea957fd48b743)), zero-copy `Raw`, `gc=False`, MessagePack/JSON parity, ~15x smaller footprint.

---
## [3][DISCRIMINATED_UNIONS_FOR_DETAIL]
>**Dictum:** *`Report.detail` is a wire concern; the wire owner is msgspec; the decision is closed.*

| [AXIS] | msgspec tagged union (`tag_field="kind"`, `tag=str.lower`) | pydantic `Field(discriminator=)` / callable `Discriminator`+`Tag` |
| --- | --- | --- |
| Dispatch | One C-level pass: read tag, select variant, fill, reject unknown ([structs](https://jcristharif.com/msgspec/structs.html#tagged-unions)) | Validate-after-decode in Python; faster than naive union but a second pass |
| Drift | `forbid_unknown_fields` → loud `ValidationError` (invariant 6) | `extra='forbid'` per model; more ceremony |
| Extensibility | `defstruct(tag=row.id)` generates a one-off variant from catalog data | New `BaseModel` subclass; hand-written |
| PEP 695 | Native; `Report.detail: A \| B \| C \| None` resolves unquoted | **Broken:** callable `Discriminator` rejects `type X = ...` aliases ([pydantic#12843](https://github.com/pydantic/pydantic/issues/12843)) |
| Paradigm | Stays in the wire owner | Drags `Detail` into pydantic → dual paradigm |

**Confirmed: msgspec is correct for `Detail`, decisively.** `Detail` is decoded from the wire, demands one-pass dispatch and loud drift, must support `defstruct` extension, and lives inside the msgspec-encoded `Envelope`. Adopting pydantic's discriminator would (a) move a wire shape into pydantic, (b) lose `defstruct` data-driven variants, and (c) collide with the repo-mandated PEP 695 `type` syntax that pydantic's discriminator cannot consume. pydantic's callable `Discriminator` only wins when the tag is *not* a stored field (runtime-computed discrimination) — `Detail` has a clean `kind` field, so that flexibility is dead weight.

---
## [4][REAL_WORLD_BOUNDARIES]
>**Dictum:** *World-class projects draw the line at the boundary, not down the middle of a value.*

- **Litestar** ([github](https://github.com/litestar-org/litestar), [msgspec DTO](https://docs.litestar.dev/latest/reference/dto/msgspec_dto.html), [pydantic plugin](https://docs.litestar.dev/latest/reference/plugins/pydantic.html)). msgspec is the *native* core (validation, serialization, OpenAPI); pydantic is an opt-in **plugin** activated only when a handler's declared type is a pydantic model. The two never model the same value — the model's own type selects exactly one backend. assay mirrors this: msgspec native, pydantic confined to one declared surface (`AssaySettings`).
- **A documented FastAPI→Litestar / pydantic→msgspec migration** ([shanechang](https://www.shanechang.com/p/pydantic-to-msgspec-migration-story/)). Its lesson is the boundary thesis itself: pydantic validates *at the model boundary* (validation = construction); msgspec separates *type* validation (decode) from *business* validation (explicit). The author keeps both — "pydantic for complex validation where you want the ecosystem, msgspec for high-performance paths" — but never lets one value belong to both. That is assay's env-vs-wire split, verbatim.

---
## [5][HARSH_CRITIQUE]
>**Dictum:** *Three concrete overlaps in the current design; fix each before code.*

1. **`Configuration = Literal["Debug","Release"]` is a banned free Literal AND a duplicated vocabulary** (`settings.md` §1 type alias + the `configurations` validator's hand-rolled `{"Debug","Release"}` set in §3). `model.md` itself outlaws free `Literal` aliases and collapses ~25 of them into `StrEnum`s; settings.md then reintroduces one and re-implements set-membership that the type system gives free. **Fix:** one `Configuration(StrEnum)`, shared by reference exactly as `ArtifactKind` is; type `configurations: frozenset[Configuration] | None` with a single `before` validator that *splits* the string — pydantic then validates each token against the enum natively, deleting the bespoke set check (a true redundant-validation removal).
2. **`row.params: type[NamedTuple]` (`main.md` §4) violates the repo's own `ruff` ban.** `pyproject.toml` bans `typing.NamedTuple` ("Use pydantic BaseModel(frozen=True) or msgspec.Struct(frozen=True)"). CLI params must stay out of *both* the wire (msgspec) and config (pydantic) systems, yet the chosen escape hatch is the banned construct — and would also become a *third* shape system the architecture's "two model systems" claim denies. **Fix:** use a `@dataclass(frozen=True)` for params — Cyclopts introspects dataclasses natively for field expansion, it is not on the banned-API list, and it keeps params disjoint from wire and config without tripping `TID251`.
3. **`__post_init__` smart constructors that raise (`model.md` §6 open decision 5) reintroduce exception control flow into the domain** and fire on *both* construction and decode ([structs](https://jcristharif.com/msgspec/structs.html#post-init-processing)), contradicting the `Result`-rail invariant (ARCHITECTURE §9). **Fix:** keep invariants in `Meta` (decode-only) where expressible; for cross-field rules, validate at the rail boundary returning `Result[Report, Fault]`, never raise across it. Minor: `Artifact.kind: str` (`model.md` §3) is a free `str` while `settings.md` §5 claims `ArtifactKind` is "shared into msgspec payloads" — either type the field `ArtifactKind` or drop the sharing claim; today the claim is unbacked.

No shape exists in both systems (verified field-by-field: `AssaySettings` is scalars/`Path`/`Literal`; structs are `Tool`/`Check`/`Report`/`Detail`/`Artifact`/`Match`/`Envelope`). The overlaps above are *vocabulary* and *construct-choice* leaks, not value duplication — but each is a crack the dual paradigm grows through if left.

---
## [FURTHER_CONSIDERATION]
- **`msgspec.convert` as the validating constructor.** Since `__init__` skips validation, the one place you *want* runtime checks on an in-process struct (a `defstruct` detail assembled from untrusted parser output) should use `msgspec.convert(mapping, type=Variant)` — not raw construction — so `Meta`/`forbid_unknown_fields` actually fire. Make this the documented factory for parser-produced details.
- **pydantic-settings `CliSettingsSource` vs Cyclopts (open decision §2, settings.md).** Running both a pydantic CLI source and Cyclopts is a latent dual ingress for flags. Pick one owner: let Cyclopts parse and forward as `init_settings` kwargs, collapsing the source tuple to `(init, env, dotenv)`. Two CLI parsers is the same dual-paradigm smell at the flag boundary.
- **`beartype` is load-bearing, not decorative.** Because construction is unvalidated, beartype at `run_check`/runner is the *sole* runtime guard for `Tool`/`Check`. Treat its removal as a validation regression, and ensure `@checked` wraps every seam that accepts a constructed (not decoded) struct.

---
## [SUMMARY]

The definitive boundary: **msgspec owns every value end-to-end; pydantic-settings owns only the environment/flag ingress (one surface, `AssaySettings`); the two are forbidden from modelling the same value.** The rule of thumb is "use msgspec unless the value is born from the env/flags or needs coercion msgspec refuses (lax parsing, aliases, computed/secret fields)" — and even `TypeAdapter` stays inside settings, never as a standalone wire parser. Double-validation is structurally impossible because msgspec validates *only at decode/convert* (never on construction) while pydantic validates *only at load*, so a config scalar passes through one gate then flows as a plain primitive into unvalidated structs. msgspec tagged unions are decisively correct for `Report.detail` (one-pass C dispatch, loud drift, `defstruct` extensibility, and PEP 695 compatibility that pydantic's discriminator currently lacks). Three fixes precede code: collapse `Configuration` into a shared `StrEnum`, swap the banned `NamedTuple` CLI params for a `@dataclass(frozen=True)`, and keep `__post_init__` from raising across the `Result` rail.
