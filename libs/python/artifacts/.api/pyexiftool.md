# [PY_ARTIFACTS_API_PYEXIFTOOL]

`PyExifTool` is the richest cross-format descriptive-metadata read/write engine for the artifacts metadata rail — the one provider that recovers and binds EXIF, IPTC-IIM, XMP, ICC-profile, GPS, maker-note, and container (PDF/video/audio) metadata across every format the exiftool binary supports, through a single long-lived `exiftool -stay_open` subprocess rather than a per-call shell-out or a format-specific Python codec. It REPLACES the abandoned `exif` package (JPEG-EXIF-IFD-only, no maker notes, no XMP, no video) as the `exchange/metadata#METADATA` RASTER carrier's EXIF-and-beyond provider: where `exif.Image(payload)` can read only the EXIF IFD of a JPEG, `ExifToolHelper.get_tags(files, tags)` returns the full `-G`-grouped tag set (`EXIF:`, `IPTC:`, `XMP:`, `ICC_Profile:`, `MakerNotes:`, `Composite:`, `QuickTime:`, `PDF:`) of any path as parsed JSON, and `set_tags(files, {tag: value})` writes any of them back. The owner composes exactly the `ExifToolHelper` context-manager (`with ExifToolHelper(...) as et`), the `get_tags`/`set_tags`/`get_metadata` JSON surface keyed into the page's one `_FIELD_KEYS`/`_EXIF_ATTR` logical→tag correspondence, and the `executable=`/`config_file=`/`common_args=` boundary parameterization that resolves the system `exiftool` binary (discovery-env → configured-path → bundled-fallback) through `shutil.which`; it never re-implements the EXIF/IPTC/XMP codec, the tag-name vocabulary, or the format dispatch the exiftool binary already owns, the persistent `-stay_open` process lives as a `WORKER_BAND` resource imported at boundary scope, and the raw `ExifToolHelper` handle never crosses the owner boundary — only the projected `dict[str, object]` logical fold does.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyexiftool`
- package: `PyExifTool` (PyPI distribution name; the pip/`pyproject` requirement is `pyexiftool`, case-folded)
- import: `exiftool`
- owner: `artifacts`
- rail: metadata
- version: `0.5.6`
- license: `GPLv3+/BSD` (dual-licensed at the user's option — the BSD arm carries no copyleft obligation; both `COPYING.BSD` and `COPYING.GPL` ship in the dist-info)
- build-floor: `Requires-Python >=3.6`; pure-Python package (`exiftool/` — `exiftool.py` core, `helper.py`, `experimental.py`, `constants.py`, `exceptions.py`), NO native extension, NO cp-gate — resolves on cp315 directly and runs on the `execution/lanes#LANE` `WORKER_BAND` process lane with the rest of the raster metadata cluster
- runtime dependency (external binary): Phil Harvey's `exiftool` command-line tool, minimum version `12.15` (`constants.EXIFTOOL_MINIMUM_VERSION` — `12.10`/`12.15` first shipped `-echo4` exit-status reporting the `execute` parser depends on); resolved on the system `PATH` via `shutil.which` or pinned through the `executable=` constructor parameter. This is the brief [03] system-tool subprocess boundary (`exiftool` machine-provided), NOT a wheel-bundled binary — `PyExifTool` is the Python driver only
- entry points: none (library only; no console script)
- namespace exports (`exiftool/__init__.py`): `exiftool.exceptions` (the typed error submodule), `ExifTool` (low-level core), `ExifToolHelper` (the friendly/safe extension — the owner's surface), `ExifToolAlpha` (active/niche extension)
- capability: cross-format descriptive-metadata read/write over a persistent `exiftool -stay_open` batch process — EXIF/IPTC/XMP/ICC/GPS/maker-note scalar + list tags on raster, PDF, and media artifacts, JSON-grouped by tag namespace (`-G`), print-conversion-controlled (`-n`), batch-over-many-files in one subprocess round-trip, with exit-status checking, tag-name validation, custom-config support, and a pluggable JSON parser

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the subprocess-driver class hierarchy
- rail: metadata

Named classes form a single inheritance chain `ExifTool → ExifToolHelper → ExifToolAlpha`, each layering capability over the persistent subprocess; the owner instantiates only `ExifToolHelper` (auto-start + exit-status + tag-name safety over the raw core). `ExifTool` owns the `-stay_open` process lifecycle and the raw `execute`/`execute_json` pipe; `ExifToolHelper` adds the `get_tags`/`set_tags`/`get_metadata` tag-oriented JSON surface with error checking; `ExifToolAlpha` adds niche single-tag/keyword/copy helpers (brittle, not used by the owner). The raw class handle never crosses the owner boundary — only the JSON `list[dict[str, object]]` projection folds onward.

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]          | [CAPABILITY]                                                                      |
| :-----: | :-------------------- | :---------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `ExifToolHelper`      | metadata driver (owner) | auto-start subprocess; `get_tags`/`set_tags`/`get_metadata` JSON R/W with checks  |
|  [02]   | `ExifTool`            | subprocess core         | `-stay_open` lifecycle, raw `execute`/`execute_json` pipe, R/W properties         |
|  [03]   | `ExifToolAlpha`       | alpha driver            | niche `get_tag`/`get_tag_batch`/`copy_tags`/`set_keywords` helpers (brittle)      |
|  [04]   | `exiftool.exceptions` | error submodule         | typed `ExifToolException` hierarchy (process-state/execute/JSON/version/tag-name) |

[PUBLIC_TYPE_SCOPE]: the typed exception hierarchy (`exiftool.exceptions`)
- rail: metadata

Every failure is a typed subclass of `ExifToolException`, never a bare `subprocess.CalledProcessError` — the execute-family errors carry `.returncode`/`.cmd`/`.stdout`/`.stderr` mirroring `subprocess.CalledProcessError`, so the owner's boundary adapter maps the tree to its `RuntimeRail` typed-error envelope by class, not by string-scraping stderr. `ExifToolExecuteError` (non-zero exit when `check_execute=True`) is the common write/read failure; `ExifToolOutputEmptyError`/`ExifToolJSONInvalidError` fire only from `execute_json` (empty or non-JSON stdout — usually a programmer error, e.g. a `-w` flag or a write op routed through the JSON reader); `ExifToolRunning`/`ExifToolNotRunning` guard the process-state preconditions; `ExifToolVersionError` fires when the binary is below `12.15` or returns unexpected sentinel text; `ExifToolTagNameError` fires when `check_tag_names=True` rejects a malformed tag.

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]      | [CAPABILITY]                                                                      |
| :-----: | :------------------------- | :------------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `ExifToolException`        | base error          | root of every PyExifTool error; the single `except` the boundary adapter traps    |
|  [02]   | `ExifToolExecuteError`     | execute error       | non-zero exit (`check_execute=True`); carries `.returncode`/`.stdout`/`.stderr`   |
|  [03]   | `ExifToolExecuteException` | execute base        | execute-error base (never raised); `subprocess.CalledProcessError`-shaped         |
|  [04]   | `ExifToolOutputEmptyError` | JSON error          | `execute_json` got empty stdout (a set/write op via the JSON reader)              |
|  [05]   | `ExifToolJSONInvalidError` | JSON error          | `execute_json` got non-JSON stdout (a `-w`/`-textOut` flag in `common_args`)      |
|  [06]   | `ExifToolRunning`          | process-state error | a running-only property (`executable`/`common_args`/`config_file`) set while live |
|  [07]   | `ExifToolNotRunning`       | process-state error | `execute`/`version` before `run()` (`ExifToolHelper` auto-starts)                 |
|  [08]   | `ExifToolVersionError`     | version error       | binary below `12.15`, or unexpected sentinel while parsing `-echo4` delimiters    |
|  [09]   | `ExifToolTagNameError`     | tag-name error      | `check_tag_names=True` and a tag fails the `[w*][w:-*]*(# \| )` validity regex    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, read, write — the owner's `ExifToolHelper` surface
- rail: metadata

`ExifToolHelper(auto_start=True, check_execute=True, check_tag_names=True, **kwargs)` is the owner's entry: `auto_start` launches the `-stay_open` subprocess on the first command (no explicit `run()` needed), `check_execute` raises `ExifToolExecuteError` on any non-zero exiftool exit, `check_tag_names` validates tag spellings before they reach the pipe, and `**kwargs` forward to the `ExifTool` core (`executable`/`common_args`/`config_file`/`encoding`/`logger`). Used as a context manager (`with ExifToolHelper(...) as et:`), the subprocess terminates on `__exit__` — the owner holds ONE long-lived helper as a `WORKER_BAND` resource and reuses it across many files rather than spawning per call. `get_tags(files, tags)` is the read: `files` is one path or an iterable of paths (non-str/bytes are `str()`-cast — a `Path` passes through), `tags=None` returns ALL tags, a single tag or list scopes the read; it returns `list[dict[str, object]]`, one dict per file, each keyed `"<group>:<tag>"` under the default `-G` plus the always-present `"SourceFile"`. `set_tags(files, {tag: value})` is the write: a `str` value sets `-tag=value`, a `list` value emits a repeated directive per item (`-Keywords=a -Keywords=b ...`), and the call returns the exiftool stdout summary (e.g. `"1 image files updated"`). `get_metadata(files, params)` is the all-tags convenience (`get_tags(files, None)`). The full constructor is `ExifToolHelper(auto_start=True, check_execute=True, check_tag_names=True, *, executable=None, common_args=["-G","-n"], config_file=None, encoding=None, logger=None)`; `files` is `str | Path | list` and `params` is `str | list | None = None`.

| [INDEX] | [CALL_SHAPE]                                                 | [CAPABILITY]                                                            |
| :-----: | :----------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `ExifToolHelper(auto_start=True, …)`                         | construct the safe driver; `**kwargs` forward to `ExifTool`             |
|  [02]   | `get_tags(files, tags, params=None) -> list[dict]`           | read scoped tags (all when `tags=None`); one `-G` dict per file         |
|  [03]   | `get_metadata(files, params=None) -> list[dict]`             | read ALL tags (`get_tags(files, None)`); full cross-format namespace    |
|  [04]   | `set_tags(files, tags: dict[str, str \| list], params=None)` | write; `str`→`-tag=value`, `list`→repeated; returns stdout summary      |
|  [05]   | `with ExifToolHelper(...) as et:`                            | ctx manager: enter runs the subprocess, exit terminates it              |
|  [06]   | `et.check_execute` (get/set)                                 | toggle exit-status checking at runtime (continue-on-error over a batch) |
|  [07]   | `et.check_tag_names` (get/set)                               | toggle tag-name validation (if the binary's tag regex diverges)         |
|  [08]   | `et.auto_start` (read-only)                                  | whether the first command auto-launches the subprocess                  |

[ENTRYPOINT_SCOPE]: the subprocess core — lifecycle, raw pipe, boundary parameterization (`ExifTool`)
- rail: metadata

`ExifTool` owns the `-stay_open True -@ -` batch process the helper drives. `run()` spawns it (`subprocess.Popen` with `preexec_fn` setting `PR_SET_PDEATHSIG` on Linux so the child dies with the parent), `terminate()` sends `-stay_open\nFalse\n` and reaps it, and the context-manager/`__del__` paths guarantee cleanup. `execute(*params, raw_bytes=False)` is the raw command pipe — it appends the `-execute<N>` sentinel, writes the params (each `str` encoded by `encoding`, each `bytes` passed verbatim), reads stdout to the `{ready<N>}` sentinel and stderr to the exit-status `-echo4` marker, and returns stdout (also setting `last_stdout`/`last_stderr`/`last_status`); `raw_bytes=True` returns undecoded `bytes` (binary tag extraction). `execute_json(*params)` prepends `-j` and parses the result through the pluggable loader. The boundary is fully parameterized: `executable` pins or `shutil.which`-resolves the binary (set raises `ExifToolRunning` while live), `common_args` is the per-process arg prefix (default `["-G", "-n"]` — `-G` groups tags by namespace, `-n` disables print-conversion for machine-parsable numeric output; append `#` to a tag to re-enable conversion per-tag), `config_file` injects a `-config` file (`""` disables the default config), and `set_json_loads(loader, **kwargs)` swaps `json.loads` for a faster parser.

| [INDEX] | [CALL_SHAPE]                                        | [CAPABILITY]                                                                       |
| :-----: | :-------------------------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `run() -> None`                                     | launch the `-stay_open` subprocess; `ExifToolVersionError` if binary below `12.15` |
|  [02]   | `terminate(timeout=30) -> None`                     | send `-stay_open False` and reap; idempotent (no-op if not running)                |
|  [03]   | `execute(*params, raw_bytes=False) -> str \| bytes` | raw command pipe; sets `last_stdout`/`last_stderr`/`last_status`                   |
|  [04]   | `execute_json(*params) -> list[dict]`               | `-j` JSON; `ExifToolOutputEmptyError`/`ExifToolJSONInvalidError` on empty/non-JSON |
|  [05]   | `et.executable` (get/set)                           | pin or `shutil.which`-resolve the binary path (discovery→configured)               |
|  [06]   | `et.common_args` (get/set)                          | per-process arg prefix; `-G` grouping + `-n` no-print-conversion                   |
|  [07]   | `et.config_file` (get/set)                          | `-config` custom-tag file; `""` disables default, `None` omits the flag            |
|  [08]   | `set_json_loads(json_loads, **kwargs) -> None`      | swap `json.loads` for `orjson`/`ujson` on the `execute_json` decode path           |
|  [09]   | `et.last_status -> int \| None`                     | exit code of the most recent `execute` (survives termination)                      |
|  [10]   | `et.last_stdout -> str \| bytes \| None`            | stdout of the most recent `execute`; the continue-on-error inspection path         |
|  [11]   | `et.version -> str`                                 | cached `exiftool -ver` (`ExifToolNotRunning` if not started); the receipt fact     |
|  [12]   | `et.logger` (write-only)                            | attach a `logging.Logger`-shaped sink; a `structlog` adapter fits                  |

[ENTRYPOINT_SCOPE]: active niche helpers (`ExifToolAlpha`) — NOT composed by the owner
- rail: metadata

`ExifToolAlpha` extends the helper with brittle single-tag and keyword conveniences the package documents as alpha-quality; the owner does NOT compose them (polymorphic-collapse law forbids a per-tag `get_tag`/`set_keywords` accessor family when the `get_tags`/`set_tags` dict surface already discriminates). Catalogued for completeness and to mark the boundary: `copy_tags(from, to)` (the `-TagsFromFile` whole-file metadata copy) is the one member with genuine standalone value (a credential/derivation lineage transfer), reachable directly through `execute("-overwrite_original", "-TagsFromFile", src, dst)` on the helper without subclassing.

| [INDEX] | [CALL_SHAPE]                                    | [CAPABILITY]                                                           |
| :-----: | :---------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `get_tag(filename, tag) -> object \| None`      | single tag of a single file with existence check (`FileNotFoundError`) |
|  [02]   | `get_tag_batch(filenames, tag) -> list`         | one tag across many files, `None` per missing                          |
|  [03]   | `copy_tags(from_filename, to_filename) -> None` | `-TagsFromFile` whole-file metadata copy with `-overwrite_original`    |
|  [04]   | `set_keywords(filename, mode, keywords) -> str` | `IPTC:Keywords` mutate by `KW_REPLACE`/`KW_ADD`/`KW_REMOVE`            |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_STACK]:
- `exchange/metadata#METADATA` ownership: `PyExifTool` is the EXIF-and-cross-format RASTER provider that replaces the abandoned `exif` arm in the `_CARRIER[MetaCarrier.RASTER]` `to_process` fold.
- metadata binding: the current `lazy import exif` prelude and `exif.Image(payload)` calls re-bind onto `lazy from exiftool import ExifToolHelper` and the `ExifToolHelper.get_tags`/`set_tags` surface.
- read fold: one exiftool subprocess covers maker notes, video/PDF tags, ICC, and IPTC/XMP. It becomes the cross-format superset the page folds first.
- helper resource: the owner holds one `ExifToolHelper(common_args=["-G", "-n"])` as a `WORKER_BAND` resource; `_exif_fields(payload)` writes the raster to a `NamedTemporaryFile` and calls `et.get_tags(str(path), list(_TAGS))`.
- tag fold: returned `dict[str, object]` rows are keyed `"<group>:<tag>"` under `-G` and fold through the logical→tag map into `MetaFacts.from_logical`. `"SourceFile"` is dropped; absent tags stay absent; `-n` keeps numeric tags machine-parsable.
- write fold: `_write_exif_fields(payload, facts, bind)` writes the raster to a temp path, derives the tag dict from the page's `_flat(facts)` projection (logical → `"<group>:<tag>"` spelling), and calls `et.set_tags(str(path), tags, params=["-overwrite_original"])`, the keyword tuple passed as a `list` value so exiftool emits the repeated `-IPTC:Keywords=...`/`-XMP-dc:Subject=...` directives; on `MetaBind.STRIP`/`REPLACE` the owner first clears the namespace via `et.execute("-all=", "-overwrite_original", str(path))` (the exiftool group-wildcard delete on the raw pipe — `set_tags` rejects an empty `tags` dict with `ValueError`, so the wildcard scrub rides `execute`, not a `set_tags` call), then re-sets under the bind policy, before `path.read_bytes()` returns the re-encoded artifact. `check_execute=True` raises `ExifToolExecuteError` on a non-zero exit so a failed write surfaces as a typed fault, never a silent miss; the cross-format write is exiftool's authority (it owns the IFD-type round-trip), so the page's separate `_rational` re-encoding is unnecessary on this arm.
- universal-tier stacking (`libs/python/.api`): the exiftool fold rides the shared rails the owner already composes —
 - universal `anyio` tier (`libs/python/.api/anyio.md`): every `get_tags`/`set_tags` call blocks on a synchronous subprocess pipe (`os.read` to the `{ready}` sentinel), so the boundary owner drives the helper inside `anyio.to_process.run_sync(limiter=WORKER_BAND)` — the `CarrierPolicy.lane` `partial(to_process.run_sync, limiter=WORKER_BAND)` value the `RASTER` row carries, never the unbounded per-loop default — and the long-lived `-stay_open` process is acquired once per worker (an `AsyncExitStack`/`anyio` resource), reused across the batch, and `terminate()`d on teardown, so the Perl interpreter startup cost is paid once, not per file.
 - universal `expression` tier (`libs/python/.api/expression.md`): the `ExifToolException` subtree maps at the boundary to the page's `RuntimeRail` — `ExifToolExecuteError` (non-zero exit, carrying `.returncode`/`.stderr`) is the write-fault arm, `ExifToolOutputEmptyError`/`ExifToolJSONInvalidError` the malformed-read arm, `ExifToolVersionError` the binary-too-old/absent arm — so the `try/except ExifToolException` lives only in the carrier function, never in the `Metadata` tagged-union dispatch.
 - universal `msgspec` tier (`libs/python/.api/msgspec.md`): the `list[dict[str, object]]` exiftool JSON folds through the page's one `MetaFacts.from_logical` (`msgspec.convert(strict=False)` per facet), coercing the `-G`-grouped tag-string values into the `Descriptive`/`Rights`/`Capture`/`Place` facet field types — no `pyexiftool`-specific struct, the JSON dict is the logical map the existing materializer consumes; `set_json_loads(msgspec.json.decode)` can replace the built-in `json.loads` on the `execute_json` decode path for a faster parse, the one `set_json_loads`-signature contract (a `loads`-shaped callable over the stdout string) `msgspec.json.decode` satisfies.
 - universal `structlog`/`opentelemetry` tier: the whole `metadata.raster.{read,write}` op is wrapped by the runtime `rasm.runtime.faults.async_boundary`, so the exiftool call inherits the structured event/span and `RuntimeRail` envelope without a `pyexiftool`-specific log rail; `ExifToolHelper(logger=...)` additionally accepts a `logging.Logger`-shaped sink (the `info`/`warning`/`error`/`critical`/`exception` duck-type a `structlog` `BoundLogger` adapter satisfies) for the subprocess's own command-level trace, and `ExifTool.version` is read once at boundary init so the exiftool binary version rides the receipt as a deployment fact.
 - universal `stamina` tier (`libs/python/runtime/.api/stamina.md`): a `-stay_open` spawn failure or wedged subprocess is the one retriable boundary fault.
 - retry seam: `@stamina.retry(on=(RuntimeError, ExifToolVersionError))` wraps helper acquisition and recovers transient spawn failure. Deterministic `ExifToolExecuteError` stays non-retriable.
- boundary resolution: the `exiftool` binary is the brief [03] system-tool subprocess boundary, resolved discovery-env → configured-path → bundled-fallback: `ExifToolHelper()` defaults to `shutil.which("exiftool")` on `PATH`, an `executable=` constructor parameter pins a configured absolute path, and the provisioning fallback supplies the binary where `PATH` lacks it — the same parameterized-boundary law the `ghostscript`/`qpdf`/`veraPDF` oracles follow, never a hardcoded path and never a wheel-bundled binary assumption (`PyExifTool` ships NO binary).
- vocabulary seam: the owner's `_FIELD_KEYS`/`_EXIF_ATTR` logical→tag correspondence keys into exiftool's `"<group>:<tag>"` namespace (`EXIF:Make`, `EXIF:Artist`, `IPTC:Keywords`, `XMP-dc:Subject`, `GPS:GPSLatitude`, `ICC_Profile:ProfileDescription`); these are real exiftool tag names, never invented labels, and the `check_tag_names=True` guard rejects a malformed spelling before it reaches the pipe (catching a read-tag accidentally written as `tag=value`). The page reaches exiftool at the GROUPED-JSON depth (`-G` namespace disambiguation, batch-over-many-files, `-n` numeric output) — never a thin per-file `get_metadata` one-shot that re-spawns the binary per call.

[RAIL_LAW]:
- Package: `pyexiftool`
- Owns: cross-format descriptive-metadata read/write over a persistent `exiftool -stay_open` subprocess — the EXIF/IPTC/XMP/ICC/GPS/maker-note/container tag set across every format the binary supports, JSON-grouped by namespace (`-G`), print-conversion-controlled (`-n`), batched over many files in one process round-trip, with exit-status checking, tag-name validation, custom-config injection, and a pluggable JSON parser
- Accept: recovering and binding the cross-format descriptive tag set of a RASTER (and, where the page extends it, PDF/media) artifact as the categorical-best provider in the `exchange/metadata#METADATA` worker-band fold — the EXIF-and-beyond superset that REPLACES the abandoned `exif` arm — projected to the `Descriptive`/`Rights`/`Capture`/`Place`/`Color` `MetaFacts` facets through the page's one logical→tag correspondence and one `MetaFacts.from_logical` materialization
- Reject: a per-tag `get_author`/`set_title` accessor family over the `get_tags`/`set_tags` dict surface; the brittle `ExifToolAlpha` niche helpers in place of the dict surface; a hand-rolled EXIF/IPTC/XMP codec or tag-name vocabulary the exiftool binary already owns; a per-call `ExifToolHelper(...)` re-spawn instead of one `WORKER_BAND`-scoped reused `-stay_open` process; the raw `ExifToolHelper`/`ExifTool` handle crossing the owner boundary; a hardcoded binary path instead of the `shutil.which`/`executable=` discovery→configured resolution; treating `PyExifTool` as bundling the binary (it ships none — the `exiftool` Perl tool is the brief [03] machine-provided system boundary); routing the SIGNED C2PA assertion (`exchange/credential#CREDENTIAL`) or the PDF/A·PDF/X conformance verdict (`exchange/conformance#CONFORMANCE`) through this package (exiftool reads the unsigned descriptive tag, not the signed manifest or the validation oracle)
