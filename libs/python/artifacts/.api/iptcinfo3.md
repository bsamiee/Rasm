# [PY_ARTIFACTS_API_IPTCINFO3]

`iptcinfo3` is the IPTC-IIM (Information Interchange Model, NewsML record 2) read/write engine for the artifacts descriptive-metadata rail — the legacy editorial-metadata block Photoshop and DAM systems embed in JPEG APP13/`8BIM` Adobe Resource segments, distinct from the XMP packet (`python-xmp-toolkit`) and the EXIF IFD (`pyexiftool`/`pyvips`). A single `IPTCInfo` object parses the APP13 IIM record-2 datasets out of a JPEG (or blind-scans any container for the `0x1c` IIM marker), exposes them as a `dict`-like mapping keyed by either the integer dataset code or its canonical IPTC name, and rewrites the file by re-assembling the Photoshop resource block around fresh packed IIM data while preserving every non-IIM `8BIM` part Photoshop wrote. The owner composes exactly `IPTCInfo(fobj, force=True, inp_charset="utf_8", out_charset="utf_8")` construction, the `__getitem__`/`__setitem__`/`__contains__` mapping surface keyed by the `c_datasets` IIM vocabulary, and `save_as(path)` re-encode into the `exchange/metadata#METADATA` RASTER carrier; it never re-implements the IIM dataset codec, the Adobe `8BIM` resource-block framing, or the JPEG marker scanner the module already owns, and the raw `IPTCInfo` handle never crosses the owner boundary — only the projected `dict[str, object]` logical fold does.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `iptcinfo3`
- package: `iptcinfo3`
- import: `iptcinfo3`
- owner: `artifacts`
- rail: metadata
- version: `2.3.0`
- license: `Artistic-1.0 OR GPL-1.0-or-later` (permissive at the user's option via Artistic-1.0; no copyleft obligation when the Artistic arm is chosen)
- build-floor: `Requires-Python >=3.8`; pure-Python single-module wheel (`iptcinfo3.py`), no native extension, no cp-gate — resolves on cp315 directly, runs on the `execution/lanes#LANE` `WORKER_BAND` process lane with the rest of the raster metadata cluster
- entry points: none (library only; the `__main__` block is a debug dumper, never a console script)
- capability: IPTC-IIM record-2 dataset read/write embedded in JPEG APP13 `8BIM`/Photoshop-3.0 Adobe Resource blocks — editorial fields (object name, headline, caption/abstract, by-line, by-line title, credit line, source, copyright notice, special instructions, writer/editor), repeating keyword/category/contact lists, location fields (city, sub-location, province/state, country code/name), and date/time + urgency/priority datasets; charset-aware decode/encode (ISO-2022 + UTF-8 IIM record 1:90 charset record), Adobe-part-preserving JPEG rewrite, and blind-scan recovery for the IIM marker in non-JPEG carriers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the IIM object and its mapping substrate
- rail: metadata

`IPTCInfo` is the sole public object; `IPTCData` and `UniqueList` are the internal mapping primitives the owner never instantiates directly but whose semantics it depends on (the `__getitem__` return shape and the keyword dedup). The module carries no exception hierarchy beyond the bare `EOFException`/`Exception` raised on a truncated or non-JPEG stream; the owner traps that as `(OSError, ValueError, KeyError, AssertionError)` at the `_read_raster`/`_write_raster` companion seam rather than a typed package rail, since IPTC recovery is best-effort enrichment after the `pyvips` decode.

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                  |
| :-----: | :-------------------- | :------------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `IPTCInfo`            | metadata object     | parse + mutate + re-encode the IIM record-2 block of a JPEG path or file-like stream          |
|  [02]   | `IPTCData`            | mapping (`dict`)    | int/name-keyed IIM dataset store; `__setitem__` enforces iterable values on list-typed datasets |
|  [03]   | `UniqueList`          | list                | dedup-on-`append`/`extend` list backing the repeating datasets (`keywords`/`contact`/categories) |
|  [04]   | `EOFException`        | error               | raised by `read_exactly`/`seek_exactly` on a short read; surfaces as a parse fault to the owner |

[PUBLIC_TYPE_SCOPE]: the IIM dataset vocabulary tables
- rail: metadata

The dataset codec is data, not code: two module-level dicts carry the closed IPTC IIM record-2 vocabulary and its reverse, and two carry the charset code map. These are the substrate the `exchange/metadata#METADATA` `_IPTC_KEY` correspondence rows key into, so the owner's logical→IIM names (`headline`, `caption/abstract`, `keywords`, `by-line`, `credit`, `source`, `copyright notice`, `special instructions`) resolve through `IPTCData._key_as_int` exactly as published.

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]   | [CAPABILITY]                                                                                       |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `c_datasets`     | `dict[int, str]` | IIM record-2 dataset code → canonical name (`5:'object name'` … `120:'caption/abstract'`, custom1-20) |
|  [02]   | `c_datasets_r`   | `dict[str, int]` | reverse name → code map `__setitem__`/`__getitem__` route string keys through                      |
|  [03]   | `c_charset`      | `dict[int, str]` | IIM 1:90 charset record code → Python codec (`196:'utf_8'`, the ISO-8859 family)                   |
|  [04]   | `c_charset_r`    | `dict[str, int]` | reverse codec → charset code the writer emits when `SURELY_WRITE_CHARSET_INFO` is enabled          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, read, mutate, persist
- rail: metadata

`IPTCInfo(fobj, ...)` parses on construction: `smart_open` duck-types `fobj` on `.read` so a path string or an open binary file-like both work, `scanToFirstIMMTag` runs the smart JPEG scan (or `blindScan` fallback) to the first `0x1c` record-2 marker, and `collectIIMInfo` folds every dataset into `self._data`. With `force=True` an object is always returned even when no IIM block exists, so the owner can seed datasets on a metadata-free JPEG and `save_as` it; without `force` a missing block logs a warning and leaves `_data` at its three-empty-`UniqueList` default. Reads return decoded `str` when `inp_charset` is set (the owner passes `"utf_8"`), raw `bytes` otherwise, and a `UniqueList` for the repeating datasets — the owner's `value.decode("utf-8", "replace") if isinstance(value, bytes)` branch and its `tuple(... for item in value)` keyword fold cover both shapes exactly. `save_as(newfile)` is JPEG-only (it returns `None` and logs when the source is not a JPEG), re-collects the file parts via `jpeg_collect_file_parts`, packs fresh IIM via `packedIIMData`, wraps it in a Photoshop-3.0 `8BIM` resource block via `photoshopIIMBlock` (carrying every non-IIM Adobe part forward), and atomically moves the temp file over the destination.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                                                   |
| :-----: | :----------------------- | :--------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `IPTCInfo.__init__`      | `IPTCInfo(fobj: str \| BinaryIO, force: bool = False, inp_charset: str \| None = None, out_charset: str \| None = None)` | parse the IIM block of a path/stream; `force=True` always returns a (possibly empty) object; `out_charset` defaults to `inp_charset` |
|  [02]   | `IPTCInfo.__getitem__`   | `info[key: int \| str] -> str \| bytes \| UniqueList \| None`                             | read one dataset by integer code or canonical name; `None` when absent, `UniqueList` for repeating datasets |
|  [03]   | `IPTCInfo.__setitem__`   | `info[key: int \| str] = value: str \| list[str]`                                         | set one dataset; a list-typed dataset (keywords/contact/categories) rejects a non-iterable with `ValueError` |
|  [04]   | `IPTCInfo.__contains__`  | `key in info -> bool`                                                                     | dataset presence test routed through `_key_as_int` (unknown key → `False`, never `KeyError`)   |
|  [05]   | `IPTCInfo.__len__`       | `len(info) -> int`                                                                        | populated-dataset count over the backing `IPTCData`                                            |
|  [06]   | `IPTCInfo.save_as`       | `save_as(newfile: str, options: list[str] \| None = None) -> bool \| None`               | re-encode the JPEG with fresh IIM to `newfile`; `options` admits `'discardAdobeParts'` and `'overwrite'`; `None` on non-JPEG source |
|  [07]   | `IPTCInfo.save`          | `save(options: list[str] \| None = None) -> bool \| None`                                 | re-encode back to the source path (asserts a path-backed `_filename`; not used for a stream-constructed object) |

[ENTRYPOINT_SCOPE]: the key-resolution and charset contract
- rail: metadata

`IPTCData._key_as_int` is the load-bearing key resolver the owner's `_IPTC_KEY` names depend on: an integer passes through; a known name resolves via `c_datasets_r`; `'credit'` aliases to code 110 (`'credit line'`, the IPTC Core 1.1 rename) and `'destination'` aliases to code 103 (`'original transmission reference'`); a `nonstandard_<n>` name resolves to the bare custom code; any other key raises `KeyError`. This is why the owner keys `creator → 'by-line'`, `copyright → 'copyright notice'`, `caption → 'caption/abstract'`, `instructions → 'special instructions'`, and `credit → 'credit'` (the alias) — every one is a verified `c_datasets`/alias name, never an invented label. Charset handling is automatic on read (the IIM 1:90 record sets `inp_charset` when present, else the constructor's `inp_charset` governs) and explicit on write through `_enc`; the module default `SURELY_WRITE_CHARSET_INFO = False` means the charset record is NOT re-emitted, so a UTF-8 round-trip relies on the owner passing matching `inp_charset`/`out_charset="utf_8"`.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                       | [CAPABILITY]                                                                            |
| :-----: | :------------------------- | :------------------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `IPTCData._key_as_int`     | `_key_as_int(key: int \| str) -> int`              | resolve a name/alias/`nonstandard_<n>` key to its IIM dataset code (the owner's `_IPTC_KEY` contract) |
|  [02]   | `IPTCData._key_as_str`     | `_key_as_str(key: int \| str) -> str`              | inverse code→name projection (`__str__` debug render of the dataset map)                |
|  [03]   | `UniqueList.append`        | `append(value) -> None`                            | dedup-preserving append — repeating-dataset writes never duplicate a keyword/contact    |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_STACK]:
- `exchange/metadata#METADATA` ownership: `iptcinfo3` is one of the four RASTER-carrier providers folded in a single `to_process` crossing alongside `pyvips` (decode/re-encode + ICC byte carrier), `pyexiftool`/`exif` (EXIF IFD scalars), and `python-xmp-toolkit`/`libxmp` (the XMP packet). It is reached only through the `_iptc_fields(payload)` reader and `_write_iptc_fields(payload, facts, bind)` writer — never imported at module scope (the manifest boundary law forbids it), `lazy from iptcinfo3 import IPTCInfo` at the page prelude, resolved inside the worker that imports every raster metadata provider at boundary scope.
- read fold: `_iptc_fields` writes the JPEG `payload` to a `NamedTemporaryFile(suffix=".jpg")`, constructs `IPTCInfo(str(path), force=True, inp_charset="utf_8")`, and projects each `_IPTC_KEY` logical → IIM name through `info[key]` into the one `dict[str, object]` — the `keywords` dataset coerced to `tuple(str(item).strip() for item in value)` (the `UniqueList` return) and scalar fields decoded to `str` — that folds into the page's single `MetaFacts.from_logical` materialization beside the EXIF and XMP projections. A missing dataset raises `KeyError` on `info[key]`, caught per-field so one absent IIM tag never sinks the read; the temp file is `unlink(missing_ok=True)` in a `finally`.
- write fold: `_write_iptc_fields` constructs `IPTCInfo(str(path), force=True, inp_charset="utf_8", out_charset="utf_8")`, clears the `_IPTC_KEY` datasets on `MetaBind.REPLACE`/`STRIP` (`info[key] = []` for the list-typed `keywords`, `""` otherwise), sets each writable field from the `_flat(facts)` projection and the `facts.descriptive.keywords` tuple as a `list`, then `info.save_as(str(path))` re-encodes the APP13 block before `path.read_bytes()` returns the bytes. This is the best-effort JPEG enricher applied AFTER the `pyvips` `write_to_buffer` encode — a non-JPEG payload makes `save_as` return `None` and the writer falls back to the pyvips/XMP result, so IPTC never blocks the raster re-encode.
- universal-tier stacking (`libs/python/.api`): the IPTC fold rides the shared rails the owner already composes — `anyio.to_process.run_sync(limiter=WORKER_BAND)` carries the synchronous `IPTCInfo` parse off the event loop onto the bounded worker band (the page's `partial(to_process.run_sync, limiter=WORKER_BAND)` `CarrierPolicy.lane`), `msgspec` (`convert(strict=False)` per facet, `structs.asdict` in `_flat`) materializes the IPTC scalars into the `Descriptive`/`Rights` facets, `expression.tagged_union` owns the two-case `Metadata` read/write discriminant the IPTC arm folds under, and `rasm.runtime.faults.async_boundary` wraps the whole `metadata.raster.{read,write}` op so the IPTC parse inherits the `structlog`+OpenTelemetry span and `RuntimeRail` typed-error envelope without an `iptcinfo3`-specific try/except rail.
- vocabulary seam: the owner's `_IPTC_KEY` logical→IIM-name table is the single correspondence into `c_datasets`; the IPTC NewsML Extension `Iptc4xmpExt:DigitalSourceType` AI-provenance field is NOT an IIM dataset (it is XMP-only) and routes through the `_xmp`/`pikepdf` arms, so `iptcinfo3` owns strictly the IIM record-2 editorial block, never the XMP-extension namespace.

[RAIL_LAW]:
- Package: `iptcinfo3`
- Owns: IPTC-IIM record-2 dataset read/write embedded in JPEG APP13 `8BIM` Adobe Resource blocks — the editorial/keyword/location/date dataset vocabulary, charset-aware decode/encode, Adobe-part-preserving rewrite, and blind-scan IIM recovery
- Accept: recovering and binding the IPTC editorial block of a JPEG RASTER artifact as one provider in the `exchange/metadata#METADATA` worker-band fold, projected to `Descriptive`/`Rights` `MetaFacts` facets
- Reject: a hand-rolled IIM dataset codec or `8BIM`/Photoshop-3.0 resource-block framing; a per-tag `get_headline`/`set_caption` accessor family over the `info[key]` mapping; an invented IIM key not in `c_datasets`/its aliases; the raw `IPTCInfo` handle crossing the owner boundary; routing EXIF (`pyexiftool`/`pyvips`), the XMP packet (`python-xmp-toolkit`), or the `Iptc4xmpExt` AI-provenance field (XMP-only) through this package; using it for any non-JPEG carrier (`save_as` is JPEG-only)
