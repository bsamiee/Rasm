# [H1][API_RAIL]
>**Dictum:** *One `ilspycmd` surface, one fingerprint-keyed cache, one polymorphic `query` that reads the symbol's shape — index, namespace, type, member, or search — and folds into one `ApiSurface` detail.*

<br>

## [1][PURPOSE]

`rails/api.py` owns the `api` claim: the four C#-host / NuGet metadata verbs `doctor | resolve | query | show`. It is the bespoke C#-only rail — it does not `select(claim, language)` and fold N rows like the thin polyglot rails; it emits two hand-shaped `Detail` variants — `ApiSurface` (tag `"api"`, the hit shape) and `ApiResolution` (tag `"resolution"`, the **richer-on-failure** miss shape) — and shares only the engine `run_check` spawn, the `model.fold` count derivation, and the artifact cache seam. Every verb is **read-only**: no `dotnet build`, no source mutation, no exclusive lease. The single `ilspycmd` catalog row drives `-l cisde` (surface roster) and `-t <type> --no-dead-code --no-dead-stores` (decompile); the index/namespace/type/member/search distinction is computed from the *shape of the symbol token*, not from a verb proliferation.

## [2][CANONICAL_SHAPES]

`ApiSurface` is the rail's hit `Detail`; `ApiResolution` (the sanctioned extension instance, tag `"resolution"`) is the **miss** `Detail`. Both inherit `Detail`'s config (`forbid_unknown_fields=True, tag_field="kind"`) and carry **typed** evidence — never a `dict`. Every field defaults, so `omit_defaults` keeps the wire lean:

```python
class ApiSurface(Detail, frozen=True, tag="api"):              # short tag "api"; typed source — the HIT shape
    source_kind: SourceKind = SourceKind.TOOL                  # ASSEMBLY | NUGET | TOOL (StrEnum)
    source_id: str = ""                                        # resolved key, e.g. "RhinoCommon"
    version: str = ""                                          # assembly/pkg version or ""
    shape: SymbolShape = SymbolShape.SEARCH                    # INDEX|NAMESPACE|TYPE|MEMBER|SEARCH
    signature: str = ""                                        # decompiled member/type signature
    doc: str = ""                                              # /// xml-doc prose, optional
    preview: str = ""                                          # bounded body window (full → Artifact)

class ApiResolution(Detail, frozen=True, tag="resolution"):    # the richer-on-failure MISS shape (core/model.py)
    candidates: tuple[tuple[str, int], ...] = ()              # top-N (name, score) nearest matches
    reason: str = ""                                          # unknown | ambiguous | partial
```

`SymbolShape` and `SourceKind` are `StrEnum` axes (behavior-carrying, never `Literal`). `query` discriminates the request into a `SymbolShape` once via `shape_of`, then a single `match` projects it onto either an `ApiSurface` (index/namespace/type/member, or a search **hit** with ranked `Match{id, kind, text, line, score}` rows) or — on a key/symbol **miss** — an `ApiResolution` on an `UNSUPPORTED` `Report`. The full surface/decompile text is content-addressed under the api scope store as `<safe-key>.<fingerprint>.txt` and rides an `Artifact(kind=ArtifactKind.SCOPE)`; bodies exceeding `--max-lines` truncate inline while the artifact carries the whole text.

**Richer-on-failure (the headline agent use case).** A `resolve`/`query` miss never returns a bare empty/failed `Report`. `_resolve_key` returns `Result[str, ApiResolution]`: a unique hit on `Ok`, else an `ApiResolution` carrying the top-N scored `candidates` (one shared `_candidates(names, needle)` scoring projection — exact 100 ▷ prefix 70 ▷ substring 40 ▷ segment-overlap, shorter-name tiebreak) and a `reason` (`unknown` for an empty match set, `ambiguous` for a multi-hit set). `_source` threads that miss through `Result[_Source, ApiResolution]`; the `resolve`/`query` handlers fold it via `_miss_report` onto an `UNSUPPORTED` `Report` (success channel, exit **3**, detail=`ApiResolution`). A *symbol* miss is symmetric: when `_search_report` finds zero substring hits it folds an `UNSUPPORTED` `ApiResolution` with `reason="partial"` carrying the nearest scored type names. The agent thus receives the nearest matches and *why* the lookup failed as crucial next-step evidence — not noise. (A *resolved* source whose declared `kind` paths are merely all-absent still folds `EMPTY`/exit 0: evidence absent ≠ key unresolved.)

| [VERB] | [TOKEN(s)] | [READS] | [PRODUCES] | [MISS] |
| ------ | ---------- | ------- | ---------- | ------ |
| `doctor` | `[--strict]` | host/NuGet/tool inventory (`ilspycmd --version`, Rhino bundle, central packages) | `Report(notes=inventory rows)` | `--strict`: `EMPTY`/`SKIP` → `FAULTED` fault (sole `api` promoter) |
| `resolve` | `<key> [kind]` | fuzzy asset-path resolution; `kind` ∈ `all\|assembly\|xml\|nuspec\|deps\|package-root` | `Report` (`OK`/`EMPTY`) + path `Artifact` | unknown/ambiguous key → `UNSUPPORTED` + `ApiResolution` |
| `query` | `<key> [symbol] [--max-lines\|--full\|--grep]` | `ilspycmd -l cisde` surface + `-t <type>` decompile | `ApiSurface` detail (hit) | key/symbol miss → `UNSUPPORTED` + `ApiResolution` |
| `show` | `<token> [--lines\|--grep\|--full\|--latest]` | prior `Artifact` preview | `Report` (artifact text in `preview`) | `EMPTY` (artifact not yet produced) |

## [3][VALIDATED_SNIPPET]

The rail's verbs are **plain functions returning `Result`** — not generators — so they build `Ok(...)` / `Error(...)` directly and fold a statement-form `match`. The CORE pattern: `shape_of` is the once-computed discriminant, and `_query_shape` is the single polymorphic dispatch. Each arm builds its `Report` via `fold(Claim.API, "query", (done,), detail=...)` — the rail never hand-builds counts — then attaches artifacts/results with `msgspec.structs.replace`. (`receipt` is the `Completed` *constructor* in `core/model.py`; the rail uses raw `Completed(...)` plus `fold`, never a `receipt(detail, ...)` lift.)

```python
from expression import Ok, Error, Result                       # factory funcs, not @effect.result here

def shape_of(symbol: str) -> SymbolShape:                       # discriminant, computed once
    match symbol.strip():                                       # statement form only
        case "":                                       return SymbolShape.INDEX
        case s if "." not in s:                        return SymbolShape.NAMESPACE
        case s if s.rsplit(".", 1)[-1][:1].isupper() and "(" not in s:
            return SymbolShape.TYPE                             # SEARCH never produced here — it is
        case _:                                        return SymbolShape.MEMBER  # the decompile-miss fallback

def _query_shape(settings, scope, surface: _Surface, p: ApiParams) -> Result[Report, Fault]:
    match shape_of(p.symbol):
        case SymbolShape.INDEX:
            return Ok(_roster_report(settings, surface, SymbolShape.INDEX, surface.namespaces, "namespace", p))
        case SymbolShape.NAMESPACE:
            owned = surface.by_namespace.get(_rank_namespace(surface, p.symbol), ())
            return Ok(_roster_report(settings, surface, SymbolShape.NAMESPACE, owned, "type", p))
        case SymbolShape.TYPE | SymbolShape.MEMBER as shape:                       # decompile -t <type>
            return _decompile(settings, scope, surface, p.symbol, p).map(
                lambda body: _decompile_report(settings, surface, shape, body, p))  # empty sig → SEARCH
        case SymbolShape.SEARCH:                                                    # unreachable; total guard
            return Ok(_search_report(surface, p))
        case never:
            assert_never(never)

# hit arm — detail mint + report assembly:
detail = _api_detail(source, shape, signature=body.signature, doc=body.xml, preview=body.window)
report = fold(Claim.API, "query", (Completed(("api", "query", source.key), 0, status=RailStatus.OK),), detail=detail)
return msgspec.structs.replace(report, artifacts=(artifact,) if body.truncated else (), truncated=body.truncated)

# miss path — one shared scoring projection + one UNSUPPORTED-fold builder (richer-on-failure):
def _candidates(names, needle, *, n=_CANDIDATE_CAP):                     # top-N (name, score)
    ...                                                                  # exact▷prefix▷substring▷overlap
def _resolve_key(packages, key) -> Result[str, ApiResolution]:          # Ok(id) | Error(ApiResolution)
    match (bool(exact) or len(hits) == 1, len(hits)):                    # bool(exact): not a truthy tuple
        case (True, _):    return Ok(hits[0])
        case (_, 0):       return Error(ApiResolution(candidates=_candidates(tuple(packages), key), reason="unknown"))
        case _:            return Error(ApiResolution(candidates=_candidates(hits, key, n=len(hits)), reason="ambiguous"))
def _miss_report(verb, key, resolution: ApiResolution) -> Report:        # fold onto UNSUPPORTED (exit 3)
    done = Completed(("api", verb, key), 0, status=RailStatus.UNSUPPORTED, notes=(...,))
    return fold(Claim.API, verb, (done,), detail=resolution)
```

A non-zero `ilspycmd` exit rides `Completed(FAILED)`, never a `Fault`. A *resolved-but-absent* poll folds `EMPTY` (exit 0). A *key/symbol miss* folds `UNSUPPORTED` (exit 3) carrying an `ApiResolution` on the **success** channel — never a `Fault`, so the agent always gets the typed report with nearest candidates. Only a spawn/no-process failure crosses the `Error` channel from `query`, and `doctor --strict` is the sole verb that promotes a folded `EMPTY`/`SKIP` to an `Error(Fault(FAULTED))`.

## [4][SEAMS]

| [SEAM] | [DIRECTION] | [CONTRACT] |
| ------ | ----------- | ---------- |
| `composition/catalog.py` | reads | The one `ilspycmd` row of the 39-row catalog: `Tool("ilspycmd", DOTNET, ("tool","run","ilspycmd","--","-l","cisde"), NONE, CS, Claim.API, mode=Mode.QUERY, parser=parse_surface)`. `select(Claim.API, Language.CSHARP)` yields it; the rail splices `<asm>` (surface) or `-t <type> --no-dead-code --no-dead-stores <asm>` (decompile) onto `tool.command` via `structs.replace`. |
| `core/engine.py` | calls | `run_check` per spawn (capture mode, `Mode.QUERY`). **No** `exclusive_lease` (read-only); the engine seam weaves `checked ▷ traced ▷ retried` over the inner spawn — retry correlation (`run_id`) is bound in `traced`, never `logged`. A spawn `Error` degrades to a synthetic non-zero `Completed` so the surface fold reads exit code uniformly. |
| `composition/settings.py` | reads | `AssaySettings.artifact(ArtifactKind.SCOPE, "api", …)` for the content-addressed `<safe-key>.<fingerprint>.txt` cache and for written report artifacts; `settings.root` resolves the Rhino bundle and `Directory.Packages.props`. |
| `composition/registry.py` | exports | `Bind(Claim.API, verb, handler, ApiParams, help)` rows for `doctor\|resolve\|query\|show`. The rail runner weaves `checked ▷ logged ▷ traced` once over each `Bind.handler`, narrowed from `object` to the 3-arg `Handler` by `_narrow` (FunctionType arm; a non-function bind is a fail-fast `TypeError`). **No** `retried` at the rail seam — `compose` filters any `Slot.retried`. |
| `core/model.py` | imports | `ApiResolution`, `ApiSurface`, `Artifact`, `ArtifactKind`, `BaseParams`, `Check`, `Claim`, `Completed`, `Fault`, `fold`, `Language`, `Match`, `Report`, `SourceKind`, `SymbolShape`, `Tool`. `fold` is the SOLE count-derivation site; `ApiSurface` (hit) and `ApiResolution` (miss) are the rail's only struct additions — both join `AnyDetail`, never a parallel type. |

## [5][EXTENSIBILITY]

A new metadata source (e.g. a `.deps.json` graph) is one `SourceKind` member plus one catalog `ilspycmd`/`dotnet` row; a sixth symbol shape is one `SymbolShape` member plus one `match` arm in `_query_shape` — never a new verb, struct, or module. The `ApiSurface` typed-source fields absorb any additional provenance an asset kind needs without re-introducing a `dict`. A new miss class (e.g. a version-pin mismatch) is one new `reason` token on the existing `ApiResolution`, never a parallel miss struct; the single `_candidates` scoring projection serves every present and future miss path (key, symbol, and beyond).

## [6][CONSIDERATIONS]

- **Fingerprint, not mtime, gates the cache.** `_fingerprint` hashes *assembly content* (`size + mtime_ns + SHA-256(bytes)`), never bare mtime — RhinoWIP bundle reinstalls preserve mtime on unchanged DLLs, and a stale surface silently misreports `[Obsolete]` markers and return-type asymmetries that only a fresh decompile reveals (memory: recon-agent API verification). The cache is the one shared-across-runs read-only artifact, so a content hash is the correctness boundary.
- **`signature` extraction is a regex boundary, and a bare word boundary over-matches.** `_body` anchors on the modifier-prefixed *declaration* line and excludes `///` doc-comment lines; a naive word boundary on the simple name conflates `Weld`/`Unweld` and skips past doc-comment occurrences, returning a sibling overload. `--grep` feeds `ilspycmd`'s body filter before windowing rather than post-hoc scanning when the symbol is ambiguous.
- **`doctor --strict` is the only `api` Fault promoter; a miss is `UNSUPPORTED` evidence, never `FAILED`/`FAULTED`.** Reserve the `Error`/`Fault` channel strictly for spawn/no-process/`--strict`. A miss splits two ways on the **success** channel: a *resolved-but-absent* poll (source found, declared paths absent) stays `EMPTY` (exit 0, speculative-poll evidence absent), while an *unknown/ambiguous key* or *missing symbol* folds `UNSUPPORTED` (exit 3) carrying an `ApiResolution`. The agent never parses stderr — it reads the typed `reason` + scored `candidates` off the report and retries against the nearest match. The exit-code algebra stays branchable: 0 (have-it or absent-but-resolved) ▷ 3 (miss-with-suggestions) ▷ 2 (broken/strict).
- **One scoring projection, one miss-fold builder — no parallel body.** `_candidates` is the sole fuzzy ranker for both the NuGet-key miss (over `Directory.Packages.props` includes) and the symbol miss (over the cached type roster); `_miss_report` is the sole `UNSUPPORTED`-fold for the `resolve`/`query` key-miss handlers, and `_search_report` folds the symbol miss inline with `reason="partial"`. The `bool(exact)` coercion in `_resolve_key`'s match-tuple is load-bearing: a non-empty `exact` tuple is *truthy* but `!= True`, so `case (True, _)` would silently skip an exact-casefold hit (e.g. `cscheck`→`CsCheck`) into the ambiguous arm without it.
