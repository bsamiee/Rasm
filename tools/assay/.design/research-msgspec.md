# [H1][RESEARCH_MSGSPEC]
>**Dictum:** *One `Base` declares the policy, one tagged `Detail` carries every algorithm receipt, and `defstruct`+`Raw` absorb irregularity; the rest is premature optimization.*

Verified against `msgspec 0.21.1` / CPython 3.14 / PEP 649. Every snippet below was executed; results are inlined as `# ->`. Config surface read from `msgspec/__init__.pyi` L40-L151 and `structs.pyi` (the live `StructConfig`/`StructMeta`), not docs.

---
## [1][STRUCT_CONFIG_MATRIX]
>**Dictum:** *Fifteen kwargs; assay sets four on `Base`, three on `Detail`, two on `Envelope`, nothing per struct.*

| [KWARG] | [SEMANTICS, verified] | [ASSAY] |
| --- | --- | --- |
| `frozen` | Immutable + hashable; enables `cache_hash`; `__setattr__` raises. | `Base=True`. All shapes immutable. |
| `eq` | Value `__eq__` (default `True`). | inherit `True` (round-trip asserts need it). |
| `order` | Adds `< <= > >=`; costs nothing unless used. | **off** — no struct is ordered; `RailStatus` severity is the only order and it lives in the enum. |
| `gc` | `False` ⇒ instance not GC-tracked: faster alloc, no GC pause; safe **only acyclic**. | `Base=False`. Graph is a tree (Envelope→Report→tuples→Detail); no cycle ⇒ no leak. |
| `weakref` | Adds `__weakref__` slot. | **off** — nothing weak-references a row. Adds a slot for zero benefit. |
| `cache_hash` | Memoizes `__hash__`; **requires `frozen`** (`ValueError: Cannot set cache_hash=True without frozen=True`). | **off by default**; flip on `Tool` *iff* rows become `dict`/`set` keys (dedupe/memo). Today `select` only sorts ⇒ dead. |
| `array_like` | Encode as JSON array (tag first): `["pk",1,2]`; ~2x decode, brittle to field reorder. | **off** — Report/Match are wire-stable. `Completed` is engine-internal/never encoded, so array_like is *moot*, not "rejected" (see §6). |
| `omit_defaults` | Skip fields equal to default on encode (incl. empty `{}`/`()`); shrinks payload. | `Base=True`. Lean Envelopes; defaulted catalog fields vanish. |
| `forbid_unknown_fields` | Unknown key ⇒ `ValidationError` at decode. | `Detail=True` only. `Base` stays open (external C# JSON carries extras). |
| `rename` | Wire-name map: `"camel"/"kebab"/...`/callable/dict. | **off** — snake_case for Python+`jq`. Apply at a single boundary struct only if a JS consumer lands. |
| `tag` / `tag_field` | Union discriminant: value + key. `tag=str.lower` ⇒ classname lowercased. | `Detail`: `tag_field="kind", tag=str.lower`. |
| `kw_only` | Keyword-only init; **permits required field after a defaulted one**. | `Envelope=True` (lets `schema_version=1` precede required `claim`/`verb`). |
| `repr_omit_defaults` | `repr` hides default fields: `B()` not `B(counts={}, name='x')`. | `Base=True` — terse structlog/CLI dumps. |
| `dict` | Adds `__dict__` for ad-hoc attrs. | **off** — defeats the slotted shape; never. |

Mutable empty defaults are idiomatic and **per-instance isolated** (`a.counts is not b.counts -> True`; mutating one never leaks). Do **not** "fix" `= {}`/`= ()` to `field(default_factory=...)`; `field` is needed only for *non-empty* mutable defaults.

---
## [2][BASE_POLICY]
>**Dictum:** *Config is inherited; a base subclass beats a metaclass; one kwarg flip overrides one flag.*

```python
class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True):
    """Fleet policy declared once; ~10 structs inherit it."""

class Tool(Base): ...                                  # frozen/gc=False/omit_defaults all inherited (verified)
class Envelope(Base, omit_defaults=False, kw_only=True): ...   # override is per-flag (verified: omit_defaults False, frozen still True)
```

Inheritance carries every config kwarg to subclasses; a subclass kwarg overrides exactly that one flag and leaves the rest. `StructMeta` (the metaclass) is for **computed** config (e.g. a runtime-decided fleet-wide `kw_only`); none arises here, so a custom metaclass is pure ceremony — reject it. Tagged-union interaction is clean: `Detail(Base, tag=str.lower, tag_field="kind", forbid_unknown_fields=True)` adds three flags on top of the inherited four; subclasses inherit all seven (verified: `VerifySummary.__struct_config__` shows `tag="verifysummary"`, `forbid_unknown_fields=True`).

---
## [3][TAGGED_UNIONS]
>**Dictum:** *`kind` dispatches one decode pass; unknown fields fail loud; `None` is a legal member; self-refs resolve unquoted on 3.14.*

```python
class Detail(Base, forbid_unknown_fields=True, tag=str.lower, tag_field="kind"): ...
class VerifySummary(Detail): ok: int = 0
class TestRun(Detail): killed: int = 0

class Report(Base):
    claim: str
    detail: VerifySummary | TestRun | None = None          # union admits None
    child: Report | None = None                            # self-ref, UNQUOTED, no `from __future__`

raw = msgspec.json.encode(Report(claim="bridge", detail=VerifySummary(ok=3)))
# -> {"claim":"bridge","detail":{"kind":"verifysummary","ok":3}}
back = msgspec.json.decode(raw, type=Report)               # one-pass tag dispatch
assert back.detail.__class__.__name__ == "VerifySummary"   # verified
assert msgspec.json.decode(b'{"claim":"x"}', type=Report).detail is None        # None member verified
msgspec.json.decode(b'{"claim":"x","detail":{"kind":"verifysummary","ok":1,"bogus":2}}', type=Report)
# -> msgspec.ValidationError: Object contains unknown field `bogus` - at `$.detail`   (verified)
```

**PEP 649 confirmed empirically.** `model.py` omits `from __future__ import annotations`; on 3.14, annotations are lazy by default, so msgspec resolves field types through module globals when it first builds a codec — by which point all variants and `Report` exist. The self-referential `Report | None` and the four-arm union resolve **without quotes** (round-trip of a nested `Report(child=Report(...))` succeeded). The `Detail` base itself is never instantiated/encoded, so giving it a `tag` is harmless.

---
## [4][TOOLBOX]
>**Dictum:** *`Meta` constrains, `defstruct` generates, `Raw` defers, hooks bridge alien types, `inspect`/`schema` introspect — each replaces hand-rolled code.*

```python
timeout: Annotated[float, msgspec.Meta(gt=0)] | None = None
# decode -1 -> ValidationError: Expected `float` > 0.0 - at `$.timeout`; absent -> None  (both verified)

Dyn = msgspec.defstruct("dyn_detail", [("v", int, 0)], bases=(Detail,), tag="dyn")
# inherits forbid_unknown_fields+tag_field; tag override -> {"kind":"dyn","v":5}  (verified)

class Carrier(Base): facts: msgspec.Raw = msgspec.Raw(b"null")
c = msgspec.json.decode(b'{"facts":{"a":[1,2,3]}}', type=Carrier)   # facts stays opaque bytes, NOT parsed
later = msgspec.json.decode(c.facts, type=dict)                     # decode lazily only when inspected (verified)

enc = lambda o: str(o) if isinstance(o, Path) else (_ for _ in ()).throw(NotImplementedError)
dec = lambda t, o: Path(o) if t is Path else (_ for _ in ()).throw(NotImplementedError)
msgspec.json.encode(HasPath(cwd=Path("/tmp/x")), enc_hook=enc)      # -> {"cwd":"/tmp/x"} (verified)
msgspec.json.decode(b'{"cwd":"/tmp/x"}', type=HasPath, dec_hook=dec)# -> PosixPath('/tmp/x') (verified)

msgspec.convert(obj, Tool, from_attributes=True)   # adapt any attr-bearing object -> struct (no manual mapping)
msgspec.to_builtins(Tool(name="t", timeout=2.0))   # -> {"name":"t","timeout":2.0} (struct -> builtins, hookable)
msgspec.json.schema(Report)                        # auto JSON Schema for `schema_version` docs
msgspec.inspect.type_info(Tool).fields             # ["name","timeout"]  (catalog projection laws)
msgspec.structs.replace(tool, name="b")            # functional update on a frozen row
```

`defstruct` is the **anti-spam escape hatch**: an irregular one-off detail is `defstruct(name, field_tuple, bases=(Detail,), tag=row.name)` — a catalog-data row, never a tenth hand-written type. A `defstruct` variant is *not* in the static `Report.detail` union, so its owning rail decodes it directly (`msgspec.json.decode(raw, type=variant)` or `convert(builtins, variant, dec_hook=...)`) and sets `Report(detail=instance)`; encode stays uniform via `kind`.

---
## [5][PERF_LEVERS]
>**Dictum:** *assay is not decode-bound; only `gc=False`, `Raw`, and module-level codecs matter — `array_like`/`cache_hash` are premature here.*

- **`gc=False` (real win, fleet-wide):** the only lever that touches every invocation. Many tiny frozen structs (Tool×~25, Match/Artifact rows, Report, Envelope) skip GC tracking ⇒ cheaper alloc and no GC pause mid-run. Safe because the object graph is acyclic.
- **`Raw` (real win, bridge):** C# `verify`/`facts` payloads can be large; hold them as `Raw` on a `defstruct` detail to avoid decode→re-encode round-tripping. Decode only the fields a rail actually reads.
- **Cached codecs (missed in sibling notes):** build `ENC = msgspec.json.Encoder()` and `REPORT_DEC = msgspec.json.Decoder(Report)` **once at module scope**; `msgspec.json.encode/decode` rebuild a codec per call. The Envelope encode (1×/invocation) and per-row parser decodes should reuse cached instances.
- **`array_like` / `cache_hash`: skip.** Hot paths are (a) one Envelope encode, (b) ~5-25 parser decodes, (c) a pure-Python `select` over ~25 rows — none is throughput-critical. `array_like` buys ~2x on a decode that runs a handful of times and adds reorder-fragility; `cache_hash` buys nothing until a `Tool` becomes a hash key. Adopt only on evidence.

---
## [6][COLLAPSE_HUNT]

> **[SUPERSEDED BY TYPE_SYSTEM.md]** Wave 4 reconciled `model.md`, `snippets/model-status.py.md`, and `catalog.md`. Unified `Mode` with `stream`/`writes`, `Parser = Callable[[Completed], Detail | None]` on `Tool`, and process exit → `Completed`/`receipt` (not `Fault`) are canonical. Retain this section for historical collapse reasoning only; do not implement against items 1–3 below without checking `TYPE_SYSTEM.md` §2–§4.

>**Dictum:** *Three real defects (Mode overload, Fault/Completed conflation, parser-as-callable) and two soft bags; the rest holds.*

1. **`Mode` is two enums wearing one name — true spam.** `model.md` defines `Mode={CAPTURE,STREAM}` (capture axis), but `catalog.md`/`engine.md` write `mode=Mode.RUN|LIST|MUTATION|VERIFY|QUERY|STAGE|DEPLOY|PUBLISH|CHECK|WRITE|RESTORE|BUILD|CLIENT`. That second set is the resurrected `ProcessMode`/`DotnetOp`/`StaticMode` the architecture claims to retire (§4 SHAPE_DISCIPLINE). **Collapse:** keep `Mode` capture-only; the sub-verb is already `Report.verb: str` + the `parser` key. Do not let a 13-member operation enum smuggle back in.
2. **`Fault` vs `Completed` overlap + a correctness bug.** Both carry `argv`+`returncode`+`stderr`. `engine.md` maps "non-zero exit → `Fault`", but a lint/analyzer non-zero exit is a **valid `Completed` with `RailStatus.FAILED`**, not an error rail. Reserve `Fault` for spawn/timeout/lease where **no process result exists**; then `Fault` need not duplicate `returncode`/`stderr` from `Completed`. This removes the duplication *and* fixes the `Result` rail's meaning (FAILED is a status, not a `Fault`).
3. **`Tool.parser` type contradiction.** `model.md`: `parser: str = ""` (registry key — correct: a frozen, msgspec-encodable, data-only `Tool`). `catalog.md`: `parser: Parser | None` holding a function object — that violates "Tool is data" and is unencodable. Keep the **string key**; resolve to the `(Completed)->Detail` decoder via a registry lookup.
4. **`ApiSurface.source: dict[str,str]` is a loose bag** — the same shape as the retired `artifact_paths` dict (§4). Prefer `tuple[Artifact,...]` already on `Report`, or typed fields if the keys are fixed.
5. **Count double-accounting (soft).** `VerifySummary.total`/`TestRun.selected` are derivable (`ok+failed`, `killed+survived`). Drop derived fields; keep genuine evidence (`report_dir`, `first_failure`, `coverage`, `mutation`). `Report.counts: dict[str,int]` may then be redundant with typed variant counts — pick one owner per metric.

**msgspec replaces hand-rolled code:** the entire `__main__` `JsonValue`/`QualityPayload`/`payload_json`/`decode_json_value`/`CompletedPayload` ladder (typed `Envelope.report` carries the struct); `msgspec.structs.replace` replaces any manual frozen-copy; `to_builtins`/`convert` replace manual dict-shaping in parsers; `inspect.type_info`/`structs.fields` power catalog projection laws (no reflection hacks); `json.schema(Envelope)` auto-documents `schema_version`; `Raw` removes bridge re-encode. The `defstruct` hatch keeps "new evidence = data, never a tenth type" literally true.
