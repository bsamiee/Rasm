# [PY_ARTIFACTS_API_PYEXIFTOOL]

`PyExifTool` recovers and binds descriptive metadata — EXIF, IPTC-IIM, XMP, ICC profile, GPS, maker notes, and PDF/video/audio container tags — across every format the `exiftool` binary reads, through one long-lived `exiftool -stay_open` subprocess, not a per-call shell-out. `ExifToolHelper.get_tags` returns any path's `-G`-grouped tag set as parsed JSON and `set_tags` writes any tag back; one worker-static helper serves the RASTER carrier, keyed through the page's `_FIELD_KEYS` logical-to-tag map. `PyExifTool` re-implements neither the codec nor the format dispatch the binary owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyexiftool`
- package: `PyExifTool` (dual-licensed GPL-3.0-or-later OR BSD-3-Clause, at the user's option)
- module: `exiftool`
- namespaces: `exiftool`, `exiftool.exceptions`
- owner: `artifacts`
- rail: metadata
- depends: the `exiftool` command-line tool — a system-tool subprocess boundary resolved through `shutil.which` or the `executable=` parameter; `PyExifTool` is the pure-Python driver, shipping no binary and no native extension

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the subprocess-driver class hierarchy

`ExifTool → ExifToolHelper → ExifToolAlpha` is a single inheritance chain, each layer adding capability over the persistent subprocess; the owner instantiates only `ExifToolHelper` for auto-start, exit-status, and tag-name safety over the raw core.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `ExifToolHelper`      | class         | auto-start subprocess; `get_tags`/`set_tags`/`get_metadata` JSON R/W with checks  |
|  [02]   | `ExifTool`            | class         | `-stay_open` lifecycle, raw `execute`/`execute_json` pipe, R/W properties         |
|  [03]   | `ExifToolAlpha`       | class         | niche `get_tag`/`get_tag_batch`/`copy_tags`/`set_keywords` helpers (brittle)      |
|  [04]   | `exiftool.exceptions` | module        | typed `ExifToolException` hierarchy (process-state/execute/JSON/version/tag-name) |

[PUBLIC_TYPE_SCOPE]: the typed exception hierarchy (`exiftool.exceptions`)

Every failure is a typed `ExifToolException` subclass, never a bare `subprocess.CalledProcessError`; the execute-family errors mirror its `.returncode`/`.cmd`/`.stdout`/`.stderr`, so the boundary adapter maps the tree to `RuntimeRail` by class, never by scraping stderr.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `ExifToolException`        | class         | root of every PyExifTool error; the single `except` the boundary adapter traps    |
|  [02]   | `ExifToolExecuteError`     | class         | non-zero exit (`check_execute=True`); carries `.returncode`/`.stdout`/`.stderr`   |
|  [03]   | `ExifToolExecuteException` | class         | execute-error base (never raised); `subprocess.CalledProcessError`-shaped         |
|  [04]   | `ExifToolOutputEmptyError` | class         | `execute_json` got empty stdout (a set/write op via the JSON reader)              |
|  [05]   | `ExifToolJSONInvalidError` | class         | `execute_json` got non-JSON stdout (a `-w`/`-textOut` flag in `common_args`)      |
|  [06]   | `ExifToolRunning`          | class         | a running-only property (`executable`/`common_args`/`config_file`) set while live |
|  [07]   | `ExifToolNotRunning`       | class         | `execute`/`version` before `run()` (`ExifToolHelper` auto-starts)                 |
|  [08]   | `ExifToolVersionError`     | class         | binary below `constants.EXIFTOOL_MINIMUM_VERSION`, or unexpected sentinel text    |
|  [09]   | `ExifToolTagNameError`     | class         | `check_tag_names=True` and a tag fails the validity regex                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, read, write — the owner's `ExifToolHelper` surface

`ExifToolHelper` is the owner's entry: `get_tags` returns one `"<group>:<tag>"`-keyed dict per file under `-G`, `set_tags` writes scalar or repeated-list values, and `execute(..., raw_bytes=True)` recovers embedded binary tags such as the thumbnail.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `ExifToolHelper(auto_start=True, ...)`             | ctor     | safe driver; `**kwargs` forward to `ExifTool`                   |
|  [02]   | `get_tags(files, tags, params=None) -> list[dict]` | instance | read scoped tags (`tags=None` → all); one `-G` dict per file    |
|  [03]   | `get_metadata(files, params=None) -> list[dict]`   | instance | all tags (`get_tags(files, None)`); full cross-format namespace |
|  [04]   | `set_tags(files, tags, params=None) -> str`        | instance | write; `str`→`-tag=value`, `list`→repeated; stdout summary      |
|  [05]   | `with ExifToolHelper(...) as et`                   | instance | ctx manager: enter runs the subprocess, exit terminates it      |
|  [06]   | `check_execute`                                    | property | toggle exit-status checking (continue-on-error over a batch)    |
|  [07]   | `check_tag_names`                                  | property | toggle tag-name validation before the pipe                      |
|  [08]   | `auto_start`                                       | property | first command auto-launches the subprocess (read-only)          |

[ENTRYPOINT_SCOPE]: the subprocess core — lifecycle, raw pipe, boundary parameterization (`ExifTool`)

`ExifTool` owns the `-stay_open` batch process the helper drives — `run()` spawns it, `terminate()` reaps it, and every teardown path guarantees cleanup. `executable`, `common_args`, `config_file`, and `set_json_loads` parameterize the process, and `execute` sets `last_stdout`/`last_stderr`/`last_status` as it reads.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                                     |
| :-----: | :-------------------------------------------------- | :------- | :--------------------------------------------------------------- |
|  [01]   | `run() -> None`                                     | instance | launch `-stay_open`; `ExifToolVersionError` on a stale binary    |
|  [02]   | `terminate(timeout=30) -> None`                     | instance | send `-stay_open False` and reap; idempotent no-op if stopped    |
|  [03]   | `execute(*params, raw_bytes=False) -> str \| bytes` | instance | raw command pipe; sets `last_stdout`/`last_stderr`/`last_status` |
|  [04]   | `execute_json(*params) -> list[dict]`               | instance | `-j` JSON; `ExifToolOutputEmptyError`/`ExifToolJSONInvalidError` |
|  [05]   | `set_json_loads(json_loads, **kwargs) -> None`      | instance | swap `json.loads` for a faster `execute_json` decoder            |
|  [06]   | `executable`                                        | property | pin or `shutil.which`-resolve the binary path                    |
|  [07]   | `common_args`                                       | property | arg prefix; `-G` groups, `-n` disables print-conversion          |
|  [08]   | `config_file`                                       | property | `-config` file; `""` disables the default, `None` omits it       |
|  [09]   | `last_status -> int \| None`                        | property | exit code of the most recent `execute`                           |
|  [10]   | `last_stdout -> str \| bytes \| None`               | property | stdout of the most recent `execute`; continue-on-error path      |
|  [11]   | `version -> str`                                    | property | cached `exiftool -ver` (`ExifToolNotRunning` if not started)     |
|  [12]   | `logger`                                            | property | `logging.Logger` sink (write-only); a `structlog` adapter fits   |

- `executable`/`common_args`/`config_file`: set raises `ExifToolRunning` while the process is live.

[ENTRYPOINT_SCOPE]: active niche helpers (`ExifToolAlpha`) — not composed by the owner

`ExifToolAlpha` adds brittle single-tag and keyword conveniences the owner does not compose; `copy_tags` (the `-TagsFromFile` whole-file copy) is reachable directly through `execute("-overwrite_original", "-TagsFromFile", src, dst)` without subclassing.

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------- | :------- | :--------------------------------------------------------------------- |
|  [01]   | `get_tag(filename, tag) -> object`       | instance | single tag of a single file with existence check (`FileNotFoundError`) |
|  [02]   | `get_tag_batch(filenames, tag) -> list`  | instance | one tag across many files, `None` per missing                          |
|  [03]   | `copy_tags(from_filename, to_filename)`  | instance | `-TagsFromFile` whole-file metadata copy with `-overwrite_original`    |
|  [04]   | `set_keywords(filename, mode, keywords)` | instance | `IPTC:Keywords` mutate by `KW_REPLACE`/`KW_ADD`/`KW_REMOVE`            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One worker-process-static `ExifToolHelper` drives every carrier op: `run()` once, `running` re-probed, `terminate()` registered with `atexit`, the live process reused across the batch so the Perl-interpreter startup is paid once.
- `get_tags` returns one `"<group>:<tag>"`-keyed dict per file under `-G`; rows fold through `_FIELD_KEYS` into `MetaFacts.from_logical`, `"SourceFile"` dropped and absent tags left absent.
- Each numeric-typed logical carries the per-tag `#` suffix (type-directed off `_NUMERIC`, the response key staying bare) so numeric fields arrive machine-parsable while string fields keep PrintConv words — never the global `-n`, whose integer codes `msgspec.convert(strict=False)` rejects into `str`-typed fields.
- `set_tags(path, tags, params=["-overwrite_original"])` writes; `MetaBind.STRIP`/`REPLACE` first clears the namespace through `execute("-all=", ...)`, REPLACE appending `--icc_profile:all` so the profile survives and STRIP scrubbing it; container bytes return without pixel re-encoding.
- Only the projected `MetaFacts` crosses the owner boundary; the raw helper never does.

[STACKING]:
- `anyio`(`.api/anyio.md`): every `get_tags`/`set_tags` blocks on a synchronous subprocess pipe, so the boundary crosses `LanePolicy.offload(Kernel.of(..., KernelTrait.HOSTILE), ...)` — the `CarrierPolicy(reader, writer, trait)` row the RASTER arm carries, never the per-loop default.
- `expression`(`.api/expression.md`): `LanePolicy.offload` lifts the `ExifToolException` subtree into `RuntimeRail` at the PROCESS boundary; carrier functions raise the provider faults unchanged, with no local `try`.
- `msgspec`(`.api/msgspec.md`): the `list[dict]` JSON folds through `MetaFacts.from_logical` (`msgspec.convert(strict=False)` per facet); `set_json_loads(msgspec.json.decode)` swaps the built-in decoder on the `execute_json` path.
- `structlog`/`opentelemetry`: `LanePolicy.offload` supplies the structured event/span and `RuntimeRail` envelope; `ExifToolHelper(logger=...)` accepts a `logging.Logger`-shaped sink for command traces.
- `stamina`(`runtime/.api/stamina.md`): `@stamina.retry(on=(RuntimeError, ExifToolVersionError))` wraps helper acquisition and recovers a transient `-stay_open` spawn failure; a deterministic `ExifToolExecuteError` stays non-retriable.
- `exchange/metadata#METADATA`: binds `PyExifTool` as `_CARRIER[MetaCarrier.RASTER]` → `KernelTrait.HOSTILE`; `_exiftool_read`/`_exiftool_write` own the `get_tags`/`set_tags`/`execute` calls, and the one subprocess covers maker notes, video/PDF, ICC, and IPTC/XMP as the cross-format superset the page folds first.

[LOCAL_ADMISSION]:
- `ExifToolHelper()` resolves the `exiftool` binary discovery-env → configured-path → provisioning-fallback: it defaults to `shutil.which("exiftool")`, `executable=` pins an absolute path, and provisioning supplies it where `PATH` lacks it — the same parameterized-boundary law the `ghostscript`/`qpdf`/`veraPDF` oracles follow; `PyExifTool` ships no binary.
- `_FIELD_KEYS` keys into the `"<group>:<tag>"` namespace (`EXIF:Make`, `IPTC:Keywords`, `XMP-dc:Subject`, `Composite:GPSLatitude`, `ICC_Profile:ProfileDescription`); `check_tag_names=True` rejects malformed spellings before the pipe.

[RAIL_LAW]:
- Package: `pyexiftool`
- Owns: cross-format descriptive-metadata read/write over a persistent `exiftool -stay_open` subprocess — the EXIF/IPTC/XMP/ICC/GPS/maker-note/container tag set the binary reads, JSON-grouped by namespace (`-G`), print-conversion-controlled (`-n`), batched over many files in one round-trip, with exit-status checking, tag-name validation, custom-config injection, and a pluggable JSON parser
- Accept: recovering and binding the cross-format descriptive tag set of a RASTER artifact in the `exchange/metadata#METADATA` PROCESS fold, projected to the `Descriptive`/`Rights`/`Capture`/`Place`/`Color`/`RasterInfo` facets through `_FIELD_KEYS` and `MetaFacts.from_logical`
- Reject: a per-tag accessor family over `get_tags`/`set_tags`; the `ExifToolAlpha` helpers in place of the dict surface; a hand-rolled EXIF/IPTC/XMP codec or a JPEG-only EXIF reader; a per-call helper respawn instead of one worker-static `-stay_open` process; a raw helper crossing the owner boundary; a hardcoded binary path; treating `PyExifTool` as bundling the binary; routing signed C2PA assertions or PDF/A·PDF/X conformance verdicts through descriptive metadata
