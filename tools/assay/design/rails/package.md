# [H1][PACKAGE_RAIL]
>**Dictum:** *One yak lifecycle: stage builds and atomically commits; deploy installs; publish pushes; rasm-bridge bridges the bridge lock.*

## [1][PURPOSE]

`rails/package.py` owns `Claim.PACKAGE`: the five verbs `stage`, `deploy`, `publish`, `list`, `plan` against `*.csproj` rows carrying a `YakPackageSlug`. It resolves a package project by slug, evaluates MSBuild yak metadata, stages the distribution under a per-dir `package-stage-<dir>` lease, runs `yak build`/`install`/`push` as `Tool` rows, and commits the staged tree atomically. No orphan yak rows exist outside this handler; the `rasm-bridge` slug is the single lifecycle special-case that takes the `bridge` lock (§4) to quit/install/refresh the running host before swapping its `.rhp`. The fold emits one `Report` with a `PackageRun` `detail`; `list`/`plan` are zero-side-effect reads.

## [2][CANONICAL_SHAPES]

`PackageRun` is the only `Detail` variant this rail owns. It is a `msgspec` tagged struct under the `Detail` base (`forbid_unknown_fields=True, tag_field="kind"`, short tag `"package"`), decoded in one pass onto `Report.detail`. Every field defaults to `""` so the `list`/`plan`/empty-commit projections are total.

```python
class PackageRun(Detail, frozen=True, tag="package"):   # kind="package"
    stage: str = ""                                      # committed package_dir (empty for list/plan)
    project: str = ""                                    # owning .csproj, repo-relative
    pattern: str = ""                                    # YakPackagePattern (e.g. "*-rh9_*-mac.yak")
    version: str = ""                                    # YakVersion staged
```

`YakMeta` is the internal validated carrier (NOT a `Detail` variant, NOT a `detail_type`/`defstruct`): a regular typed `Base` struct carrying `Path` fields the wire never sees plus behavior (`from_props` → `validate`). It reuses the `_META_PROPS` vocabulary (`YakPackageSlug`, `YakPath`, `YakPlatform`, `YakPackageDirectory`, `YakPackagePattern`, `YakPushSource`, `TargetDir`, `AssemblyName`, `TargetExt`, `YakManifestDirectory`, `TargetFramework`, `MSBuildProjectDirectory`), validated for slug match, `.rhp` `target_ext`, output-dir containment under `bin/<config>/<tfm>`, `YAK_PLATFORM=="mac"`, `YAK_DISTRIBUTION_GLOB` pattern, and an executable `YakPath`.

Yak verbs are catalog rows (§4, `tools-cs.md`), never inline argv builders — `_yak_tool` replaces the catalog base row's `command` so the head is the resolved `YakPath` and the tail is the full step argv (`place` appends one empty tail for `Input.NONE`):

| [ROW] | `runner` | `command` tail | `input` | `mode` | [TAIL VIA `_yak_*_tail`] |
| ----- | -------- | -------------- | ------- | ------ | ------------------ |
| `yak build`   | `DIRECT` | `("build",)`   | `NONE` | `Mode.STAGE`   | `--platform <plat> --version <ver>` (`cwd=stage`) |
| `yak install` | `DIRECT` | `("install",)` | `NONE` | `Mode.DEPLOY`  | `<package_file>` (`cwd=package_dir`) |
| `yak push`    | `DIRECT` | `("push",)`    | `NONE` | `Mode.PUBLISH` | `--source <push_source>? <package_file>` |
| `dotnet msbuild` (yak meta) | `DOTNET` | `("msbuild", project, …)` | `NONE` | `Mode.QUERY` | `-getProperty:<each _META_PROPS> -nologo` → JSON `Properties` |

## [3][VALIDATED_SNIPPET]

The core staging pattern: take the per-dir lease (keyed on `package_dir.name`, never the slug), run the `yak build` `Check`, then commit atomically (temp → rotate → swap, restore-on-`OSError`) folding to a `PackageRun` `Report`. Every handler is a plain function folding the `Result` via statement-form `match` — it `return`s, never `yield`s (not an `@effect.result` generator), and never `return match`. `Fault` carries only `{argv, status, message}`.

```python
from expression import Error, Ok, Result   # NOT @effect.result: this returns, never yields

def _commit(meta: YakMeta, staged: Path, slug: str) -> Result[Report, Fault]:
    previous = meta.package_dir.with_name(f"{meta.package_dir.name}.previous.{os.getpid()}")
    try:                                                       # boundary OSError → Fault
        rmtree(previous, ignore_errors=True)
        meta.package_dir.replace(previous) if meta.package_dir.exists() else None
        staged.replace(meta.package_dir)                       # atomic rotate → swap
        rmtree(previous, ignore_errors=True)
    except OSError as exc:
        previous.replace(meta.package_dir) if previous.exists() and not meta.package_dir.exists() else None
        rmtree(staged, ignore_errors=True)                     # restore pre-stage tree
        return Error(Fault(("yak", "build", slug), message=str(exc)[:1024]))   # status=FAULTED default
    return Ok(fold(Claim.PACKAGE, "stage",
                   (Completed(("yak", "build", slug), 0, status=RailStatus.OK),),
                   detail=PackageRun(stage=str(meta.package_dir), project=meta.project,
                                     pattern=meta.package_pattern, version="")))

def _stage_meta(settings, scope, meta: YakMeta, slug: str, version: str) -> Result[Report, Fault]:
    meta.package_dir.parent.mkdir(parents=True, exist_ok=True)
    staged = Path(mkdtemp(prefix=f"{meta.package_dir.name}.", dir=meta.package_dir.parent))
    resource = f"package-stage-{meta.package_dir.name}"        # per-dir lease, never slug
    splice = _stage_artifacts(meta, staged)                    # copy manifest + non-host .dll/.json/.rhp

    def locked(_held: object) -> Result[Report, Fault]:        # runs only under an Ok lease
        return splice.bind(lambda _: _run_yak(meta, _yak_build_tail(meta, version), Mode.STAGE,
                                              cwd=staged, settings=settings, scope=scope)) \
                     .bind(lambda done: _commit_or_fail(meta, staged, slug, version, done))

    outcome = leased(resource, locked, settings=settings, run_id=settings.run_id, project=slug, mode="exclusive")
    match outcome:                                             # a lease/spawn Fault discards the staged temp
        case Result(tag="error"):
            rmtree(staged, ignore_errors=True)
            return outcome
        case _:
            return outcome
```

`evaluate_meta` runs the `dotnet msbuild … -getProperty:… -nologo` `Mode.QUERY` `Check`, decodes the JSON `Properties` envelope via one cached `_DECODER`, then projects-and-validates `YakMeta` *before* any lease so a slug/`.rhp`/platform/glob/executable mismatch faults at exit 2 without rotating a live dir. A `yak build` defect rides `Ok(Completed(FAILED))` and folds to a `FAILED` `Report` without committing; a held lease rides `Fault(BUSY)` (exit 5) without waiting.

## [4][SEAMS]

| [SEAM] | [DIRECTION] | [CONTRACT] |
| ------ | ----------- | ---------- |
| `composition/catalog.py` | in | `select(Claim.PACKAGE, None) -> tuple[Tool, ...]`; the catalog yak row asserts `Claim.PACKAGE` owns the program. `_yak_tool` `msgspec.structs.replace`s its `command`/`mode`. |
| `core/engine.py` | in | `run_check(check, *, settings, scope, routed, deadline=None) -> Result[Completed, Fault]`; `leased(resource, action, *, settings, run_id, project, mode) -> Result[T, Fault]` is the per-dir/bridge fail-fast lease (psutil liveness) — `action` is a thunk evaluated only under an `Ok` lease, busy → `Fault(BUSY)`, never blocks. |
| `core/routing.py` | in | `Routed(language, scope=Scope.CHANGED)` is the minimal `Input.NONE` shape; `place` appends one empty tail. Package routing resolves the csproj by `YakPackageSlug`, not by changed-set. |
| `core/model.py` | in | `Report`, `fold(claim, verb, outcomes, *, detail=None)`, `Completed`, `Fault`, `Detail`/`PackageRun`, `Bind`, `Claim`, `Mode`, `Runner`, `Input`, `Language`; `RailStatus` (`OK`/`FAILED`/`FAULTED`/`TIMEOUT`/`BUSY`). Counts/status derive once in `fold`. |
| `rails/bridge.py` | in/out | `rasm-bridge` lifecycle: `quit`/`refresh` run as `Claim.BRIDGE` `Mode.CLIENT` rows under the shared `bridge` lease (`leased(_BRIDGE_LOCK, …)`) before/after the `.rhp` swap, so `quit → install → refresh` is atomic under one lease. |
| `composition/registry.py` | out | `Bind(Claim.PACKAGE, verb, package_rail.{stage,deploy,publish,list,plan}, PackageParams, …)`; the runner weaves them `compose(*_RAIL_LAYERS)(_narrow(handler))` — `checked ▷ logged ▷ traced`. Retry correlation (`run_id`) is rebound in `traced` (the engine seam, outside the `retried` loop), not in `logged`. |
| `composition/settings.py` | in | `RASM_BRIDGE_SLUG`, `YAK_PLATFORM`, `YAK_DISTRIBUTION_GLOB`, `AssaySettings.configuration`, `settings.root`, `settings.run_id`. |

`deploy`/`publish` reuse the `_lifecycle` resolve → evaluate → stage → finish fold, then fold a step policy keyed by `(verb, slug == RASM_BRIDGE_SLUG)` in `_STEP_POLICY`: `deploy`→`(install,)`; `publish`→`(install, push)`; the `rasm-bridge` rows prepend `quit` and append `refresh` (`("quit", "install", "refresh")` / `("quit", "install", "refresh", "push")`), and `_drive_steps` runs the whole sequence inside the `bridge` lease whenever a step is `quit`/`refresh`. `list`/`plan` short-circuit before any lease, MSBuild, or yak invocation — `list` folds `slug=project` pairs into `notes`; `plan` runs the same fail-fast evaluate path and emits the `YakMeta` rows into `notes` with an empty `PackageRun` (`stage=""`).

## [5][EXTENSIBILITY]

A new distribution platform or a sixth lifecycle step (e.g. `notarize`) is one catalog `Tool` row plus one entry in the `_STEP_POLICY` `(verb, slug)` table — never a new handler, struct, or module; the `PackageRun` detail and the atomic-commit fold are platform-agnostic.

## [6][CONSIDERATIONS]

- The atomic commit rotates `package_dir → .previous.<pid>` before the swap so a crash mid-rotate is recoverable, but two distinct slugs sharing one `YakPackageDirectory` parent would collide on the `.previous` name; the per-dir lease (resource `package-stage-<package_dir.name>`, never the slug) is the actual serializer — keep the lease resource derived from `meta.package_dir.name`, not `slug`, when two projects publish into one directory.
- `yak build` defects ride the success channel as `Completed(FAILED)`, but a non-executable `YakPath` or a slug/`.rhp`/platform mismatch is a precondition fault that must surface as `FAULTED` *before* the lease is taken — validate `YakMeta` inside `evaluate_meta`, not inside `_stage_meta`, so a misconfigured project fails fast at exit 2 without ever stealing the stage lease or rotating a live package dir.
- The `rasm-bridge` deploy holds the `bridge` lease across `quit → install → refresh`; if `refresh` faults after `install`, the on-disk `.rhp` is the new version while the (quit) host is down — `refresh` failure is recoverable by a bare `bridge launch`, so it rides `Completed(FAILED)` (which folds into the step `Report`) rather than rolling back the committed install, and the operator relaunches.
