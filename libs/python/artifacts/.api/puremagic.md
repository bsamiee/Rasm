# [PY_ARTIFACTS_API_PUREMAGIC]

`puremagic` mints content identity from a self-contained signature table: it carries the head and foot of bytes, a path, or a seekable stream against the table and returns a confidence-ranked roster of `(extension, mime_type, name)` matches. Its `scanners/` deep-scan layer disambiguates the ambiguous `PK\x03\x04` ZIP and `\xd0\xcf\x11\xe0` CFBF containers to the exact OOXML/ODF/EPUB/USDZ or legacy-Office subtype. It is the artifacts file-control rail's default sniffer: pure-Python with a bundled `magic_data.json`, running in-process on the event loop with no native dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `puremagic`
- package: `puremagic` (MIT)
- module: `puremagic` — re-exports `puremagic.main`, the implementation module
- namespaces: `puremagic`, `puremagic.main`, `puremagic.scanners`
- abi: pure-Python `py3-none-any`, no native extension
- rail: file control

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the match record and fault rail

`PureMagic` is a `namedtuple` the owner folds into `DetectIdentity` by field or `._asdict()`; the deep-scan `scanners.helpers.Match` dataclass re-wraps as `PureMagicWithConfidence` before it crosses the module boundary, so the owner only ever sees the namedtuple shape. `PureError`/`PureMagic`/`PureMagicWithConfidence` import from top-level `puremagic`, `PureValueError` reaches as `puremagic.main.PureValueError`, and both faults lift to the file-control rail at the boundary.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `PureMagic`               | namedtuple    | `(byte_match, offset, extension, mime_type, name)`                         |
|  [02]   | `PureMagicWithConfidence` | namedtuple    | `PureMagic` + `confidence` 0.0-1.0 (1.0 = deep-scan exact); roster element |
|  [03]   | `PureError`               | exception     | `LookupError` — no signature matched                                       |
|  [04]   | `PureValueError`          | exception     | `ValueError` — empty input                                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cooked single-answer detection

`from_*` is one polymorphic surface over three source shapes (bytes/str, path, seekable stream), each returning ONE cooked `str` — the extension by default, the MIME type under `mime=True` (`from_extension` defaults `mime=True`). `filename=` on the string/stream rows adds the extension hint and, when it names a real file, triggers the deep-scan layer.

| [INDEX] | [SURFACE]                             | [CAPABILITY]                                               |
| :-----: | :------------------------------------ | :--------------------------------------------------------- |
|  [01]   | `from_string(string, mime, filename)` | in-memory bytes/str; canonical `Buffer`; deep-scan hook    |
|  [02]   | `from_file(filename, mime)`           | a path: head+foot read, ext hint, deep-scan                |
|  [03]   | `from_stream(stream, mime, filename)` | seekable stream (head, foot-seek, rewind); fsspec-tolerant |
|  [04]   | `from_extension(extension, mime)`     | reverse: ext → MIME/name; longest sig wins                 |
|  [05]   | `ext_from_filename(filename)`         | parse on-disk ext, recovering a double ext (`.tar.gz`)     |

[ENTRYPOINT_SCOPE]: ranked match-roster detection

`magic_*` returns the full confidence-ranked `list[PureMagicWithConfidence]` (highest first) — the roster `Detect` folds for MIME, extension, human name, and the multi-match tail in one call. `filename=` runs the deep-scan and folds its exact resolution (`confidence == 1.0`) to the front. `identify_all` is not top-level; reach it as `puremagic.main.identify_all` when pre-slicing head/foot.

| [INDEX] | [SURFACE]                           | [CAPABILITY]                                                              |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `magic_string(string, filename)`    | roster from bytes/str; deep-scans on a real `filename`                    |
|  [02]   | `magic_file(filename)`              | roster from a path, deep-scan-folded; unknown → `[]`, never raises        |
|  [03]   | `magic_stream(stream, filename)`    | roster from a seekable stream; deep-scans on real `filename`              |
|  [04]   | `magic_extension(extension)`        | all rows for an extension, ranked by `(confidence, len(byte_match))` desc |
|  [05]   | `identify_all(header, footer, ext)` | core matcher over sliced head/foot; multi-part + confidence fold; no I/O  |

[ENTRYPOINT_SCOPE]: head/foot slicing and the deep-scan layer

Lower primitives control I/O and the container pass directly, each reached as `puremagic.main.*`. `*_details` rows return `(head, foot)` sliced to `get_max_lengths()` — the longest header signature and its offset with the longest footer span — so a multi-gigabyte payload is identified from a bounded read, never a full load. `PUREMAGIC_DEEPSCAN=0` disables the whole deep-scan layer, dropping every `from_*`/`magic_*` to table-only.

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]                                                        |
| :-----: | :---------------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `get_max_lengths()`                                               | bounded byte budget each end is sliced to; folds multi-part offsets |
|  [02]   | `file_details(filename)`                                          | `(head, foot)` from a path; `PureError` on a non-regular file       |
|  [03]   | `stream_details(stream)`                                          | `(head, foot)` from a seekable stream, rewind; fsspec-tolerant      |
|  [04]   | `string_details(string)`                                          | `(head, foot)` off an in-memory bytes payload — no I/O              |
|  [05]   | `single_deep_scan(bytes_match, filename, head, foot, confidence)` | route leading bytes to the container scanner for the exact subtype  |
|  [06]   | `magic_data(filename)`                                            | load a custom or the bundled signature DB                           |

`magic_data()` returns `(magic_header_array, magic_footer_array, extension_only_array, multi_part_dict)` — three `list[PureMagic]` sorted by `byte_match` and a `dict[bytes, list[PureMagic]]` of header→secondary rows, the module-level substrate the confidence fold and multi-part correlation read.

[ENTRYPOINT_SCOPE]: the deep-scan result shape and container scanners

Each `scanners.<name>` module exposes a `match_bytes` constant and `main(file_path, head, foot) -> Match | None`; the public roster re-wraps `scanners.helpers.Match(extension, name, mime_type, confidence=1.0)` as `PureMagicWithConfidence`. No owner calls a scanner directly — the `from_*`/`magic_*` rows route them — yet the `zip_scanner`/`cfbf_scanner` resolutions are the `Container.ZIP`/`Container.OLE` → exact-subtype disambiguation the `Detect` `MediaClass`/`Container` fold consumes.

| [INDEX] | [SURFACE]                    | [SIGNATURE]               | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :------------------------ | :---------------------------------------------------------------------- |
|  [01]   | `zip_scanner`                | `b"PK\x03\x04"`           | `[Content_Types].xml` → OOXML set, ODF/EPUB/USDZ/CBZ                    |
|  [02]   | `cfbf_scanner`               | `b"\xd0\xcf\x11\xe0"…`    | CFB stream + CLSID → legacy Office                                      |
|  [03]   | `ebml` / `ogg` / `asf`       | per-format `match_bytes`  | Matroska/WebM, Ogg stream, ASF/WMV subtype                              |
|  [04]   | `mpeg_audio` / `sndhdr`      | per-format `match_bytes`  | MPEG-audio frame, AU/HCOM/FSSD/SNDR audio subtype                       |
|  [05]   | `pdf`/`json`/`hdf5`/`python` | first-match chain         | confirm a true PDF/JSON/HDF5/Python payload past a generic signature    |
|  [06]   | `text_scanner`               | `decode_any`; `eml_check` | charset (utf-8/cp1252), line-terminator + CSV + RFC-822 `.eml` recovery |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Detection runs in-process on the event loop: pure-Python with a bundled `magic_data.json`, so no native library reifies in a worker and an `import` fault is a packaging defect, never a missing-host-lib gap. A bounded `get_max_lengths()` head+foot read keeps even a multi-gigabyte path off the heap; only a deliberately latency-bounded sniff moves to `anyio.to_thread.run_sync`, never a process.
- Source is one axis: bytes, path, and stream are three rows on the one `from_*`/`magic_*` surface, never parallel detector types. `from_string`/`magic_string` is the canonical in-memory row because admission already holds the payload; `filename=` is how that buffer row still reaches the deep-scan.
- Output is two read shapes over one detection: the cooked `str` (`from_*`, `mime=True` selecting MIME over extension) and the ranked `list[PureMagicWithConfidence]` (`magic_*`). A full identity is ONE `magic_*` call returning the roster the owner folds — highest-confidence head as primary, tail as the multi-match ambiguity evidence — never a per-output flag cookie.
- Deep-scan is the categorical axis: `single_deep_scan` resolves the `PK\x03\x04` ZIP family and the `\xd0\xcf\x11\xe0` CFBF compound to the exact subtype, folding in at `confidence == 1.0`. `PUREMAGIC_DEEPSCAN=0` is the one runtime switch, set through `os.environ` for an untrusted-ingest table-only pass, never a per-call argument.
- Faults are two narrow rails: `from_*`/`from_extension` raise `PureError` on no match and `PureValueError` on empty input; the ranked `magic_*` rows return the table matches or `[]` rather than raising, so an empty or low-confidence roster reads as `MediaClass.UNKNOWN`. Both lift to the file-control rail once at the boundary.
- Boundary: `puremagic` owns content sniffing only, minting no `ContentKey` and contributing no `ArtifactReceipt`. Its resolved identity is the admission gate the per-format readers dispatch on; the descriptive metadata inside the identified container is the `exchange/metadata` owner's concern, never re-read here.

[STACKING]:
- `exchange/detect`: reads the `magic_string(payload, filename=spill)`/`magic_file(path)` ranked roster into the `DetectIdentity`/`MediaClass`/`Container`/`Trust` fold as the default in-process sniffer — highest-confidence `mime_type` through `MediaClass.of`/`Container.of`, `extension` into the `extensions` tuple, float `confidence` and multi-row tail into the `Trust.AMBIGUOUS`/`UNKNOWN` verdict — and takes `single_deep_scan`'s OOXML/CFBF resolution to close the generic-container floor.
- `expression`(`.api/expression.md`): `tagged_union` owns the two-source `Source` (`Buffer`/`File`) discriminant the `from_string`/`from_file` rows dispatch under; `Result`/`RuntimeRail` carries the railed verdict.
- `msgspec`(`.api/msgspec.md`): folds the `PureMagicWithConfidence` roster into the frozen `DetectIdentity` `Struct` — `roster[0].mime_type`/`.extension`/`.name`/`.confidence` scalars, `[r.mime_type for r in roster]` for the tail — and projects the result to span attributes through `to_builtins`.
- `beartype`(`.api/beartype.md`): validates the `str`/`bytes`/`PathLike` ingress shapes at the boundary.
- `anyio`(`.api/anyio.md`): runs the in-process call under `async_boundary`, inheriting the `structlog` event, OpenTelemetry span, and `RuntimeRail` envelope; only a latency-bounded sniff crosses to `to_thread.run_sync`.

[LOCAL_ADMISSION]:
- Admitted MIT and pure-Python, no native provisioning and no `.mgc` loader, as the default `artifacts` file-control sniffer. `python-magic`/libmagic joins only for the broad-leaf-signature fallback its compiled database owns, never the default sniff.

[RAIL_LAW]:
- Package: `puremagic`
- Owns: pure-Python signature-table content identification (bytes/path/stream → confidence-ranked `(extension, mime_type, name, confidence)` roster) with multi-part header+footer correlation, double-extension recovery, extension↔MIME reverse lookup, and the per-container deep-scan resolving OOXML/ODF/EPUB/USDZ/legacy-Office/EBML/Ogg/ASF/MPEG/JSON/HDF5/text to the exact subtype — the default in-process artifacts sniffer
- Accept: producing the `exchange/detect` `DetectIdentity` fold from the `magic_string`/`magic_file`/`magic_stream` ranked roster, in-process under `async_boundary`; using `single_deep_scan`'s OOXML/CFBF resolution to close the generic-container floor; setting `PUREMAGIC_DEEPSCAN=0` for an untrusted or latency-bounded table-only pass
- Reject: a wrapper-rename of `from_string`/`from_file`/`magic_file`; a per-output function family over the one ranked roster; a per-source detector type where the `from_*`/`magic_*` rows discriminate on input; routing this default sniff through a `to_process` worker where no native dependency exists to subprocess; re-implementing the signature table, multi-part correlation, confidence arithmetic, or deep-scan the module owns; minting a `ContentKey` or `ArtifactReceipt`; reading the metadata inside the identified container; admitting `python-magic`/libmagic for the default path where it is not strictly broader on a leaf signature
