# [PY_ARTIFACTS_API_PYTHON_MAGIC]

`python-magic` binds the host `libmagic` library through a `ctypes` cookie, cooking bytes, a path, or a descriptor into a MIME type, description, charset, or extension list under a `MAGIC_*` bitmask. libmagic sits off the runtime loader path, so `exchange/detect` reifies every detection across the `anyio.to_process` `WORKER_BAND` subprocess seam and folds the cookie into its `DetectIdentity`/`MediaClass`/`Container`/`Trust` verdict. It is the broad-leaf-signature fallback behind the default `puremagic` sniffer, retained where its compiled database recognizes a signature `puremagic` lacks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-magic`
- package: `python-magic` (MIT, Adam Hupp)
- module: `magic` (the `magic/__init__.py` ctypes binding)
- namespaces: `magic`, `magic.loader`
- abi: pure-Python `py3-none-any` wheel over a runtime `ctypes` binding to the host `libmagic`; `import magic` raises `ImportError('failed to find libmagic')` when `magic.loader.load_lib` finds no candidate, so libmagic is a Forge-provisioned host dependency off the runtime loader path
- rail: file control

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cookie and fault

`Magic` carries a `threading.Lock` so one cookie is reusable across calls under the worker's GIL; `_handle509Bug` returns `application/octet-stream` for the libmagic 5.09 null-MIME quirk rather than raising. `MagicException` is synthesized by the two `ctypes` errcheck hooks on a NULL or `-1` C return and lifts to the file-control fault rail at the boundary, never escaping as a bare exception into domain logic.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :--------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Magic`          | class         | detector cookie; `flags: int`, `cookie` (`magic_t`), `lock`       |
|  [02]   | `MagicException` | exception     | libmagic NULL/-1 return; `.message` (`None` on the 5.09 null bug) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stateless module detection

Module functions own an `_instances`-cached default cookie keyed by the `mime` boolean and expose ONLY the `mime` switch; a richer flag policy requires a `Magic` cookie, which is why the `Detect` owner constructs cookies rather than calling these rows.

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                    |
| :-----: | :--------------------------------------- | :------ | :------------------------------ |
|  [01]   | `from_buffer(buffer, mime=False) -> str` | static  | in-memory bytes; `str` coerced  |
|  [02]   | `from_file(filename, mime=False) -> str` | static  | from a path; pre-open `IOError` |
|  [03]   | `from_descriptor(fd, mime=False) -> str` | static  | from an open descriptor         |
|  [04]   | `version() -> int`                       | static  | libmagic version int            |

[ENTRYPOINT_SCOPE]: configured cookie

`Magic.__init__` owns every detection flag; the per-call methods take only the source and return the cooked string under the cookie's flags, so one cookie per flag policy is the canonical owner and the flags are never per-call arguments. `Magic.__init__` also auto-applies `setparam(MAGIC_PARAM_NAME_MAX, 64)` (the issue-190 fixed-limit workaround) and tolerates a libmagic that rejects the call.

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------- | :------- | :------------------------------- |
|  [01]   | `Magic.from_buffer(buf) -> str`     | instance | thread-locked; `str` utf-8 first |
|  [02]   | `Magic.from_file(filename) -> str`  | instance | from a path                      |
|  [03]   | `Magic.from_descriptor(fd) -> str`  | instance | from an open descriptor          |
|  [04]   | `Magic.setparam(param, val) -> int` | instance | set a `MAGIC_PARAM_*` cap        |
|  [05]   | `Magic.getparam(param) -> int`      | instance | read a `MAGIC_PARAM_*` cap       |

- `Magic.setparam`: raises `MagicException` when libmagic lacks the param; `extension=True` raises `NotImplementedError` below libmagic 5.24.

[ENTRYPOINT_SCOPE]: flag and param vocabulary

Constructor booleans set the `MAGIC_*` bitmask; `MAGIC_PARAM_*` ordinals address `setparam`/`getparam`. `_FACET_FLAG` pins exactly ONE output boolean per facet: `Magic(mime=True, mime_encoding=True)` returns the combined `text/plain; charset=utf-8` form, not two facets.

| [INDEX] | [FLAG_BOOLEAN]  | [MAGIC_BIT]                     | [EFFECT]                                                     |
| :-----: | :-------------- | :------------------------------ | :----------------------------------------------------------- |
|  [01]   | `mime`          | `MAGIC_MIME_TYPE` (`0x10`)      | MIME type (`application/pdf`) instead of a description       |
|  [02]   | `mime_encoding` | `MAGIC_MIME_ENCODING` (`0x400`) | the charset (`utf-8`, `binary`)                              |
|  [03]   | `extension`     | `MAGIC_EXTENSION` (`0x1000000`) | `/`-separated valid-extension list; requires libmagic 5.24   |
|  [04]   | `uncompress`    | `MAGIC_COMPRESS` (`0x4`)        | sniff inside gzip/bzip2/xz containers                        |
|  [05]   | `keep_going`    | `MAGIC_CONTINUE` (`0x20`)       | all matches separated by `\n- `, not just the first          |
|  [06]   | `raw`           | `MAGIC_RAW` (`0x100`)           | do not translate unprintable characters                      |
|  [07]   | `magic_file=`   | (passed to `magic_load`)        | load a custom `.mgc` or text database instead of the default |

- `MAGIC_NO_CHECK_*` (`COMPRESS`/`TAR`/`SOFT`/`APPTYPE`/`ELF`/`TEXT`/`CDF`/`CSV`/`JSON`/`TOKENS`): disable a libmagic test class to narrow the check set on an untrusted or latency-bounded ingest.
- `MAGIC_PARAM_*` (`INDIR_MAX`/`NAME_MAX`/`REGEX_MAX`/`BYTES_MAX` recursion/name/regex/byte caps, `ELF_PHNUM_MAX`/`ELF_SHNUM_MAX`/`ELF_NOTES_MAX` ELF-table caps): the `setparam`/`getparam` magic-bomb/ELF-bomb defense.

[ENTRYPOINT_SCOPE]: loader and boundary coercion

`magic.loader.load_lib` is the native-resolution mechanism the import guard depends on, not a call surface — it raises `ImportError('failed to find libmagic')` when no candidate loads, the provisioning fault the owner surfaces distinct from a content fault. `maybe_decode` and `coerce_filename` are the C-string boundary coercions every cooked return crosses.

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------- | :------ | :------------------------------------------------------ |
|  [01]   | `magic.loader.load_lib() -> CDLL`             | static  | resolve+load host `libmagic`; `ImportError` when none   |
|  [02]   | `errorcheck_null` / `errorcheck_negative_one` | static  | ctypes hooks synthesizing `MagicException` on NULL/-1   |
|  [03]   | `maybe_decode(s) -> str`                      | static  | decode a C-string as utf-8/backslashreplace             |
|  [04]   | `magic.magic_setflags(cookie, flags) -> int`  | static  | apply a recomputed `MAGIC_*` bitmask onto a live cookie |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `import magic` at boundary scope only, reified inside the `to_process` worker, so every detection crosses the `anyio.to_process.run_sync(..., limiter=WORKER_BAND)` subprocess seam because libmagic is off the loader path, never the in-process call the `puremagic` peer rides; an import-time `ImportError` is a Forge-provisioning fault, a transient `BrokenWorkerProcess` death recovers under a bounded `stamina` retry, and a deterministic magic-bomb crash rails through `async_boundary` unretried behind the `MAGIC_PARAM_*` caps.
- One `Magic` cookie per `MagicFacet` owns the flags, the `threading.Lock`, and the loaded database, `functools.cache`-memoised so the compiled database loads once per config per worker; the module rows are the stateless `mime`-only path over the `_instances` default cookie.
- Source is one axis: `Source.Buffer`→`Magic.from_buffer` and `Source.File`→`Magic.from_file` on one cookie surface, matched by one `match` in `_cooked`; the file row reads on the worker side so the parent never pickles the payload, and the descriptor row is excluded because a parent-process fd does not cross the seam.
- Output is one axis: `mime`/`mime_encoding`/`extension` are flag selections pinned one-per-facet, so a full identity holds four cookies in one crossing and folds the four cooked strings into `DetectIdentity`; the strongest match is `raw.split("\n- ", 1)[0]` cutting the `MAGIC_CONTINUE` separator.
- Fault is one axis: a failed call raises `MagicException`, and `_handle509Bug` returns `application/octet-stream` for the 5.09 null quirk classified `MediaClass.UNKNOWN`; a broken-database `MagicException` lands at the `async_boundary` `CLASSIFY` catch, lifted once at the boundary.
- Each detection folds the MIME, description, charset, extension tuple, `keep_going` match set, byte length, and resolving `libmagic_version` into the frozen `DetectIdentity` `msgspec.Struct` the worker pickles back, `version()` gating the `EXTENSION` facet (`>= 524`); it mints no `ContentKey` and no `ArtifactReceipt`, and the resolved `DetectIdentity` is the admission gate the per-format readers dispatch on through the `MediaClass` discriminant.

[STACKING]:
- `exchange/detect#DETECT`: composes this as its WORKER-BAND libmagic arm; its `DetectPolicy` folds the `DetectFlag` set, the `MagicParam` caps, and the custom `.mgc` `magic_file` onto every facet cookie, `DetectProfile` selects the `MagicFacet` tuple via `_PROFILE_FACETS`, and `_FACET_FLAG` maps facet→boolean; the cooked strings fold through `MediaClass.of`/`Container.of` and the `extension`/`keep_going` splits into the `Trust` verdict (`IDENTIFIED`/`AMBIGUOUS`/`MISMATCH`/`UNKNOWN`).
- `anyio`(`.api/anyio.md`): `to_process.run_sync(..., limiter=WORKER_BAND)` over the shared `WORKER_BAND` `CapacityLimiter` from `rasm.runtime.lanes` — the universal native-arm seam `exchange/metadata#METADATA` (`pyexiftool`) and `graphic/raster/io#RASTER` (`pyvips`) also cross.
- `stamina`(`runtime/.api/stamina.md`): `AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)` → a `BoundAsyncRetryingCaller` recovering a transient worker death; a deterministic crash stays non-retriable.
- `msgspec`(`.api/msgspec.md`): the cooked strings fold into the frozen `DetectIdentity` `Struct` (`frozen=True, gc=False`) pickled back across the seam; `to_builtins(identity, str_keys=True)` projects it to OpenTelemetry span attributes.
- `expression`(`.api/expression.md`): `tagged_union` owns the two-source `Source` (`Buffer`/`File`) discriminant the `from_buffer`/`from_file` rows dispatch under; `Result`/`RuntimeRail` carries the railed verdict and `Block` the batch `traversed(..., by=Disposition)` fold.
- `beartype`(`.api/beartype.md`): validates the `bytes`/`PathLike` ingress shapes at the boundary.

[LOCAL_ADMISSION]:
- Admitted MIT as the broad-leaf-signature worker-band fallback behind `puremagic`, off the runtime loader path; libmagic is a Forge-provisioned host dependency the wheel does not carry. `compat`, the deprecation-wrapped file-5.x shim, collides with the upstream binding and is not admitted — the `Magic` cookie is the one surface.

[RAIL_LAW]:
- Package: `python-magic`
- Owns: worker-band `libmagic` content sniffing from bytes or path — MIME / description / MIME-encoding / extension-list outputs, compressed look-through, `MAGIC_NO_CHECK_*` check-set narrowing, custom `.mgc` databases, and the `MAGIC_PARAM_*` recursion/ELF-bomb caps — the broad-leaf-signature fallback behind the default `puremagic` sniffer
- Accept: producing the `exchange/detect#DETECT` `DetectIdentity`/`MediaClass`/`Container`/`Trust` fold from `Magic.from_buffer`/`from_file` cooked strings, one flag-pinned `functools.cache`-memoised cookie per `MagicFacet`, crossed through `anyio.to_process.run_sync(..., limiter=WORKER_BAND)` under a `stamina` `BrokenWorkerProcess` retry and `async_boundary`; tuning an untrusted ingest through `setparam`; gating the `EXTENSION` facet on `version() >= 524`
- Reject: wrapper-renames of `from_buffer`/`from_file`; the deprecation-wrapped `compat.*` shim; per-call detection flags where a flag-pinned `Magic` cookie is the owner; `Magic(mime=True, mime_encoding=True)` where one boolean per facet is the law; routing this native arm in-process without the `to_process` `WORKER_BAND` crossing; `Magic.from_descriptor` in the worker; re-implementing the magic-pattern database, the `.mgc` loader, the per-flag bit math, or the recursion-cap engine; minting a `ContentKey` or `ArtifactReceipt`; admitting libmagic for the default path where it is not strictly broader than `puremagic` on a leaf signature
