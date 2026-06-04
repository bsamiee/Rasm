# [H1][RESEARCH_PYDANTIC_SETTINGS]
>**Dictum:** *Cyclopts owns argv; settings own env; one frozen surface validates at load; one `artifact(kind,*parts)` folds every path.*

Verified empirically against `pydantic-settings==2.14.0`, `pydantic==2.13.3`, Python `>=3.14` (repo `uv.lock`). Every snippet below was executed; behavioral claims are observed, not cited. `BaseSettings.settings_customise_sources(settings_cls, init_settings, env_settings, dotenv_settings, file_secret_settings) -> tuple[PydanticBaseSettingsSource, ...]` — **first element = highest precedence** (confirmed: `init` overrides `env`).

---
## [1][SETTINGSCONFIGDICT_MATRIX]

| [FLAG] | [TYPE/DEFAULT] | [BEHAVIOR] | [ASSAY] |
| --- | --- | --- | --- |
| `env_prefix` | `str=""` | Prepended to field name for the env source **only**. | `"ASSAY_"`. |
| `env_nested_delimiter` | `str\|None=None` | Splits `ASSAY_A__B` into nested models. | `"__"` (harmless; no nested model today — drop until one exists). |
| `case_sensitive` | `bool=False` | Env match casefold. | `False` (default); agents export upper-case. |
| `env_file` / `env_file_encoding` | `path\|None` | Activates `DotEnvSettingsSource`. | **`None` (disable)** — see [6]; shared `.env` breaks hermeticity. |
| `env_ignore_empty` | `bool=False` | Treat `ASSAY_X=` as unset. | `True` — empty exports from a sibling agent must not bind. |
| `extra` | `"ignore"` | `forbid` rejects **init kwargs + dotenv keys**, *not* stray OS env (env source drops undeclared prefixed vars). | `"forbid"` (catches Cyclopts-forwarded typos). |
| `frozen` | `bool=False` | Immutable + hashable instance. | `True`. |
| `secrets_dir` | `path\|None` | Docker-secret file source. | `None` (no secrets). |
| `cli_*` (15 keys) | — | Govern the built-in `CliSettingsSource`/`CliApp`. | **All unset** — Cyclopts owns CLI; see [3]. |
| `validate_default` | `bool=False` | Run validators on defaults too. | `True` — anchor/coerce the `default_factory` paths at load, not first use. |
| `nested_model_default_partial_update` | `bool=False` | — | irrelevant (flat model). |

---
## [2][SOURCE_PRECEDENCE]

```python
@classmethod
def settings_customise_sources(cls, settings_cls, init_settings, env_settings, dotenv_settings, file_secret_settings):
    return (init_settings, env_settings)  # init (registry overrides) > ASSAY_* env; dotenv/secrets dropped
```

Verified: `S(n=9)` with `ASSAY_N=5` yields `9`; `ASSAY_N=5` alone yields `5`. **Why determinism matters:** N agents share one shell. The exported `ASSAY_*` block is the *floor*; per-invocation `init` kwargs (Cyclopts→registry) always win, and the `run_id` factory + `artifact(kind,…,run_id)` guarantee disjoint write scopes under identical env. Returning a fixed tuple makes the resolution order a pinned property, not an accident of pydantic's default ladder — a testable invariant for parallel safety.

---
## [3][CLI_BOUNDARY_VERDICT]

`pydantic-settings` ships a full CLI engine: `CliApp.run(model_cls, cli_args=...)`, `CliSettingsSource`, and typed markers (`CliSubCommand`, `CliPositionalArg`, `CliImplicitFlag`, `CliMutuallyExclusiveGroup`, `get_subcommand`). This **directly overlaps Cyclopts** — both parse argv into a typed object. Running both is dual-paradigm rot.

**Verdict (definitive): Cyclopts owns argv; settings own env. Do not instantiate `CliSettingsSource` at all.**

- Verified inert: `CliSettingsSource(settings_cls, cli_parse_args=False)` reads nothing from `sys.argv` (`['prog','--n','7']` → `n` stays `5`). Placing it in the precedence tuple is **dead code that advertises a CLI plane that never fires** — delete it. This resolves `settings.md` open-decision #2 against the `CliSettingsSource` line in its [4].
- The data path is one-directional: Cyclopts parses the verb `NamedTuple` (per `main.md` §4) → registry constructs `AssaySettings(**overrides)` → `init_settings` carries them at top precedence. Pydantic never sees argv.
- Consequence: the entire `cli_*` config family stays unset; `CliApp` is unused. No second arg parser, no `--help` contention, no kebab/JSON-arg coupling.

---
## [4][VALIDATION_SURFACE]

- **`@field_validator(..., mode="before")`** for coercion (anchor `root`, `expanduser` targets, Rhino bundle); **`mode="after"`** sees a typed value. Decorate `@classmethod`.
- **`@model_validator(mode="after")`** for cross-field invariants. On a `frozen` model it may **assert/raise but not assign** (`object.__setattr__` only, discouraged) — expose derived predicates as `@property`, never validator-set state.
- **`AliasChoices` — critical gotcha (verified):** `validation_alias` **overrides `env_prefix` entirely**. `AliasChoices("rhino_app","RHINO_WIP_APP_PATH")` makes the env source look for literal `rhino_app`/`RHINO_WIP_APP_PATH` and **silently ignore `ASSAY_RHINO_APP`**. To keep the prefixed name, list it explicitly:

```python
rhino_app: Path | None = Field(default=None,
    validation_alias=AliasChoices("ASSAY_RHINO_APP", "RHINO_WIP_APP_PATH"))  # include prefixed name by hand
```

- **`Annotated[int, Field(ge=1, le=64)]` / `Annotated[float, Field(gt=0.0)]`** for bounded scalars — bounds visible to `ty`, enforced at load.
- **`computed_field` vs `@property` (verified leak):** `@computed_field @property` **serializes into `model_dump()`/`model_dump_json()`** (`artifact_root` appeared in both). A bare `@property` does not. For paths that must never cross the wire, prefer plain `@property`; reserve `@computed_field` for values you *want* in a debug dump.
- **`@cached_property` + `frozen=True`:** caching mutates `__dict__` → conflicts with frozen; keep paths recomputed (cheap) or precompute in `model_post_init` into a slot. Do not cache.
- **`SecretStr`** (`pydantic`): only when an OTLP header or yak token enters config; none today. **`Discriminator`/`Tag`** belong to the `msgspec` `detail` union, not config — no discriminated field exists in `AssaySettings`.

---
## [5][ARTIFACT_DERIVATION]

`ArtifactKind(StrEnum)` is `str`, so `Path.joinpath` consumes members directly. One method retires every `*_lock`/`*_dir` property:

```python
def artifact(self, kind: ArtifactKind, *parts: str | Path) -> Path:
    return (self.root / ".artifacts" / "assay").joinpath(kind, *(str(p) for p in parts))
# locks   -> artifact(ArtifactKind.LOCKS, "mutation.lock")
# test    -> artifact(ArtifactKind.TEST, "all" if all_targets else self.test_target.stem, self.run_id)
# rhino   -> artifact(ArtifactKind.RHINO, "verify", self.run_id)
# scope   -> artifact(ArtifactKind.PROCESS, rail, self.run_id) / "dotnet-cli"
```

Leaf tokens (`"mutation.lock"`, `"verify"`, `"dotnet-cli"`) are call-site arguments, not standing constants — `_QUALITY`/`_ARTIFACTS`/`_DOTNET_CLI`/`_MARKER` all die.

---
## [6][HARSH_CRITIQUE_OF_SETTINGS_MD]

1. **Layering inversion (real defect).** `settings.md` makes `composition/settings.py` *own* `ArtifactKind`. But `ARCHITECTURE.md` §5/§8 has `msgspec` payloads in `core/model.py` reference `ArtifactKind`, and the acyclic order is `core → composition`. A `core` import of a `composition` enum is a **cycle**. Define `ArtifactKind` in `core/model.py`; `settings.py` imports it. Then "exposes exactly two symbols" is wrong — settings re-exports a borrowed enum.
2. **Dead `CliSettingsSource`.** The [4] snippet's `CliSettingsSource(..., cli_parse_args=False)` is verified-inert. It contradicts the Cyclopts-owns-argv stance and must be deleted (collapse to `(init_settings, env_settings)`).
3. **`AliasChoices` bug.** [1]'s `rhino_app` row is marked `[SOURCE]=env`, implying `ASSAY_RHINO_APP` works; the alias **kills the prefix** (verified). Either add `"ASSAY_RHINO_APP"` to the choices or stop claiming env coverage.
4. **Marker token duplicated.** [2] states `Workspace.slnx` "lives only in the anchor validator **and** `solution` computed field" — that is **two** literals, the exact duplication the note claims to retire. Anchor once into `root`; derive `solution = root / root_marker` from a single module `Final`, or fold the marker into the anchor's closure.
5. **`extra="forbid"` mis-sold.** [1]/[3] lean on `forbid` for shared-env determinism; verified, `forbid` ignores stray OS env entirely — it guards init typos and **dotenv** keys. The real hazard is a shared `.env` whose one bogus `ASSAY_*` key aborts *every* agent at load. Resolve open-decision #4 now: **disable `env_file`** (hermetic), do not defer.
6. **Flag-spam by another name.** Marking 11 fields `env + flag` violates "no flag spam" (`ARCHITECTURE` goal). Agent-only ⇒ env is the config plane; expose flags **only** for the 1–3 values flipped per call (`configuration`, scope/`--mutation`, optionally `run_id`). Demote the timeouts, CPU caps, and project paths to env-only.
7. **`run_id` factory underspecified.** [1] shows `"%Y-%m-%dT%H-%M-%S.%f-{pid}"` without a timezone; predecessor pins `tz=UTC`. Naive local time is non-deterministic across CI/host TZ — pin UTC and keep it flag/init-only (no `ASSAY_RUN_ID`) so env cannot collide scopes; add the env alias only if CI replay is later required.
8. **`mutation_eligible` placement.** [3] says it is "exposed as the resolved-equality check" inside `@model_validator(mode="after")`; on `frozen` the validator can only *assert*. Make it a `@property`; let the validator raise when mutation is requested but ineligible.

**Boundary handoff:** correct in spirit — settings emit `Path`/`int`/`float`/`str`; `msgspec` consumes; no value defined twice. The single shared symbol is the `ArtifactKind` *vocabulary* (a `StrEnum`, not a shape) — legitimate reuse **once it lives in `core/model.py`** (point 1).

---
## [FURTHER_CONSIDERATION]

- **`PyprojectTomlConfigSettingsSource` is shipped (2.14).** assay could fold static defaults from a `[tool.assay]` table via a source in the precedence tuple — a repo-committed config plane below env, above field defaults, with zero new dependency. Decide deliberately; do not adopt silently.
- **`validate_default=True` + `default_factory` interplay.** Rhino auto-discovery runs at *construction* even when unused. If a no-Rhino rail (e.g. `static`) constructs `AssaySettings`, discovery still fires. Gate discovery behind the consuming rail, or make the factory return `None` and validate lazily in `require_rhino_app()`.
- **`frozen=True` makes the instance hashable** — usable as a cache key / `contextvars` token across the `anyio` fan-out, tying one immutable config identity to a whole parallel run for trace correlation (`structlog` bind), an esoteric lever the note does not exploit.
