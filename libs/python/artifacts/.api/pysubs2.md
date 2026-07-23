# [PY_ARTIFACTS_API_PYSUBS2]

`pysubs2` owns the subtitle/caption DOCUMENT model for the artifacts MEDIA rail: parse and dialect-autodetect a timed-text track, edit it as an `SSAEvent`/`SSAStyle` record model, retime by constant shift or framerate rescale, and re-serialize to any registered dialect. It owns the document layer alone — the codec, mux, and container wire belong to `av`. One `SSAFile` (`MutableSequence[SSAEvent]`) is the spine feeding the `media/subtitle` owner, and only serialized subtitle bytes and projected `plaintext` frame text cross the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pysubs2`
- package: `pysubs2` (MIT, tkarabela)
- import: `pysubs2`
- namespaces: `pysubs2`, `pysubs2.formats`, `pysubs2.formats.substation`, `pysubs2.time`
- owner: `artifacts`
- rail: media
- wheel: pure-Python `py3-none-any`, no native extension

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the subtitle document spine and its event/style records

`SSAFile` is-a `MutableSequence[SSAEvent]`: the standard-library ABC supplies the full list-mutation surface, never re-implemented. `SSAEvent` and `SSAStyle` are mutable dataclasses the file reads and mutates; `Color` (range-checked 8-bit RGBA) and `Alignment` (numpad `IntEnum`, ASS semantics) are the value vocabularies a style carries.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :---------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `SSAFile`   | document      | `MutableSequence[SSAEvent]` spine; `events`/`styles`/`info`/`fps`/`format`            |
|  [02]   | `SSAEvent`  | dataclass     | timed line: `start`/`end` ms, tagged `text`, `style`/`name`/`layer`/`margins`/`type`  |
|  [03]   | `SSAStyle`  | dataclass     | named `fontname`/`fontsize`/color/`bold`/`italic`/border/`alignment`/`outline`/margin |
|  [04]   | `Color`     | value         | range-validated 8-bit RGBA a style carries                                            |
|  [05]   | `Alignment` | enum          | numpad 1–9 ASS anchor; `from_ssa_alignment`/`to_ssa_alignment` bridge SSA numbering   |

[PUBLIC_TYPE_SCOPE]: the typed error rail

`Pysubs2Error` roots the hierarchy as the owner's subtitle-boundary `except` anchor; each subclass carries a `__reduce__` for clean cross-process propagation off the worker band, and the `Path.open` ingest/egress raises `OSError`/`UnicodeDecodeError`/`UnicodeEncodeError` beside them.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :----------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Pysubs2Error`                 | base error    | hierarchy root and subtitle-boundary `except` anchor                         |
|  [02]   | `FormatAutodetectionError`     | error         | ambiguous/unknown scanned dialect; carries `content` and candidate `formats` |
|  [03]   | `UnknownFormatIdentifierError` | error         | unregistered `save`/`to_string` `format_`; carries `format_`                 |
|  [04]   | `UnknownFileExtensionError`    | error         | unmapped save extension when `format_` is omitted; carries `ext`             |
|  [05]   | `UnknownFPSError`              | error         | MicroDVD fps neither supplied nor inferred                                   |

[PUBLIC_TYPE_SCOPE]: the format registry and dialect vocabulary

`FORMAT_IDENTIFIERS` carries the dialect set as data: the owner passes a verified `format_` string, never instantiating a `FormatBase` subclass directly.

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `formats.FORMAT_IDENTIFIERS`                  | constant      | canonical writable dialect identifiers                |
|  [02]   | `formats.FILE_EXTENSION_TO_FORMAT_IDENTIFIER` | map           | save-path extension codec                             |
|  [03]   | `formats.FORMAT_IDENTIFIER_TO_FORMAT_CLASS`   | map           | dialect-to-`FormatBase` parse/serialize dispatch      |
|  [04]   | `formats.autodetect_format`                   | function      | unique fragment dialect or `FormatAutodetectionError` |
|  [05]   | `formats.get_format_identifier`               | function      | extension-to-identifier `save`/`load` resolution      |
|  [06]   | `get_file_extension`                          | function      | identifier-to-extension `save`/`load` resolution      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest — parse and autodetect

`from_file` with `format_=None` buffers the stream, autodetects the dialect, and re-parses (pipe-safe), setting `format` and `fps`; `load`/`from_string` build on it, and `errors="surrogateescape"` passes bytes the encoding cannot decode.

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `SSAFile.load(path, encoding, format_, fps, errors)` | factory | path parse; dialect and fps autodetect             |
|  [02]   | `pysubs2.load(...)`                                  | static  | module alias for `SSAFile.load`                    |
|  [03]   | `SSAFile.from_string(string, format_, fps)`          | factory | parse a Unicode `str`, not bytes                   |
|  [04]   | `SSAFile.from_file(fp, format_, fps)`                | factory | buffered autodetect-then-reparse core              |
|  [05]   | `load_from_whisper(result_or_segments) -> SSAFile`   | static  | Whisper `{segments:[{start,end,text}]}` sec→events |

[ENTRYPOINT_SCOPE]: the event surface — read, mutate, project text

Events index and slice as a list; a slice is type-checked and a non-`SSAEvent` raises `TypeError`. `start`/`end` are milliseconds authored through `make_time`.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------------------------- |
|  [01]   | `subs[i]`, `subs[a:b]`                            | operator | event access; a slice yields `list[SSAEvent]`            |
|  [02]   | `subs.insert(i, ev)`                              | instance | indexed insertion via `MutableSequence`                  |
|  [03]   | `subs.append(ev)`                                 | instance | append via `MutableSequence`                             |
|  [04]   | `SSAEvent(start, end, text, style, ...)`          | ctor     | construct a millisecond-timed subtitle line              |
|  [05]   | `ev.plaintext`                                    | property | read/write; `{…}` tags stripped, newlines normalized     |
|  [06]   | `ev.duration`                                     | property | read/write `end - start`; a negative raises `ValueError` |
|  [07]   | `ev.is_text`                                      | property | visible-text verdict, excluding drawings/comments        |
|  [08]   | `ev.is_comment`                                   | property | comment-line verdict                                     |
|  [09]   | `ev.is_drawing`                                   | property | SSA drawing-tag verdict                                  |
|  [10]   | `ev.shift(h, m, s, ms, frames, fps)`              | instance | in-place time/frame retiming of one line                 |
|  [11]   | `SSAStyle(fontname, fontsize, primarycolor, ...)` | ctor     | construct one named style                                |
|  [12]   | `ev.copy() -> SSAEvent`                           | instance | clone an event                                           |
|  [13]   | `ev.as_dict() -> dict`                            | instance | shallow projection preserving nested `Color`             |
|  [14]   | `ev.equals(other) -> bool`                        | instance | event field-equality compare                             |

[ENTRYPOINT_SCOPE]: retiming and style operations on the track

`rename_style` rewrites every referencing event and validates the new name against the SubStation grammar; `remove_miscellaneous_events` is the CLI `--clean`, dropping comments, drawing-tag lines, sub-2-char text, and time-identical duplicates.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `SSAFile.shift(h, m, s, ms, frames, fps)`      | instance | constant time/frame-delta track translation            |
|  [02]   | `SSAFile.transform_framerate(in_fps, out_fps)` | instance | rescale by `in_fps/out_fps`; non-positive `ValueError` |
|  [03]   | `SSAFile.sort()`                               | instance | in-place `(start, end)` event ordering                 |
|  [04]   | `SSAFile.rename_style(old_name, new_name)`     | instance | rename and rewrite references; reject an illegal name  |
|  [05]   | `SSAFile.import_styles(subs, overwrite=True)`  | instance | merge styles with an `overwrite` conflict policy       |
|  [06]   | `SSAFile.remove_miscellaneous_events()`        | instance | drop non-essential `--clean` lines                     |
|  [07]   | `SSAFile.get_text_events() -> list[SSAEvent]`  | instance | select visible (non-comment/drawing) events            |

[ENTRYPOINT_SCOPE]: egress — serialize to any dialect

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `SSAFile.save(path, encoding, format_, fps, errors)` | instance | path serialize; dialect from extension unless forced            |
|  [02]   | `SSAFile.to_string(format_, fps) -> str`             | instance | in-memory `str` convert/restyle egress a `ContentKey` keys over |
|  [03]   | `SSAFile.to_file(fp, format_, fps)`                  | instance | low-level text-stream writer                                    |

[ENTRYPOINT_SCOPE]: time arithmetic and override-tag parsing

`make_time` is the canonical millisecond constructor every `start`/`end`/`shift` delta authors through — `make_time(s=1.5) == 1500`, `make_time(frames=50, fps=25) == 2000`, and frames without fps raises `ValueError`. `parse_tags` splits tagged `text` into `(fragment, computed SSAStyle)` runs, the styled-fragment decomposition an override-faithful burn-in consumes.

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------- | :------ | :------------------------------------------------ |
|  [01]   | `pysubs2.make_time(h, m, s, ms, frames, fps) -> int`      | static  | author ms from unit mixes or frame counts         |
|  [02]   | `time.ms_to_str(ms, fractions=False) -> str`              | static  | render ms as `[-]H:MM:SS[.mmm]` for display       |
|  [03]   | `time.ms_to_times(ms) -> Times`                           | static  | milliseconds to a `(h, m, s, ms)` tuple           |
|  [04]   | `time.times_to_ms(h, m, s, ms) -> int`                    | static  | a `(h, m, s, ms)` tuple to milliseconds           |
|  [05]   | `time.frames_to_ms(frames, fps) -> int`                   | static  | frame-to-ms conversion at a framerate             |
|  [06]   | `time.ms_to_frames(ms, fps) -> int`                       | static  | ms-to-frame; non-positive fps raises `ValueError` |
|  [07]   | `formats.substation.parse_tags(text, style, styles, ...)` | static  | tagged text to override-aware styled fragments    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every subtitle production folds through one `expression.tagged_union` `SubtitleOp` dispatched by a total `match`/`case` closed by `assert_never`, returning `RuntimeRail[ContentKey]` keyed over the produced bytes — one polymorphic owner, never a `convert_srt`/`retime`/`burn` function family.
- `formats.FORMAT_IDENTIFIERS` folds into one `SubtitleDialect` `StrEnum` carried in the op payload, never a free `format_` string re-validated at each arm.
- Each production contributes the single `ArtifactReceipt.Media` case; `SubtitleEvidence` carries no provider handle and the receipt owner imports no `pysubs2` type.

[STACKING]:
- `av`(`.api/av.md`): the `Mux` arm mints a timed-text stream via `add_stream("ass"|"srt")`/`add_stream_from_template` and muxes the serialized bytes through `OutputContainer.mux`/`mux_one` beside the video/audio streams; the `BurnIn` arm composites rendered RGBA onto the rgb24 frame via `VideoFrame.from_bytes(data, w, h, format="rgba")`; the seam is the serialized byte stream and the `av` handle never crosses back.
- `numpy`(`.api/numpy.md`): the RGBA burn buffer and the rgb24 frame the composite reads and writes as an `ndarray`.
- `expression`(`.api/expression.md`): `tagged_union` owns the `SubtitleOp` discriminant and `Result` the typed-error rail the `RuntimeRail` envelope builds on.
- `msgspec`(`.api/msgspec.md`): the frozen `Struct` op and evidence owners.
- within-library convert/retime/restyle: `SSAFile.from_string(text, format_=src).to_string(format_=dst)` is the whole convert axis — one parse, one serialize, no per-pair converter; `Retime` folds `shift`/`transform_framerate`, `Restyle` folds `rename_style`/`import_styles`, `Whisper` folds `load_from_whisper` over an ASR segment set; a `Pysubs2Error`/`OSError`/`UnicodeError` surfaces through the `media.subtitle` `async_boundary` as a typed `RuntimeRail` fault.
- within-library burn-in: `BurnIn` hands `SSAEvent.plaintext` (or `substation.parse_tags` styled runs for override-faithful text) to the `typography/shape#SHAPE` + `graphic/raster#RASTER` render, selected by `media/filtergraph#FILTER` when the linked FFmpeg lacks the `subtitles`/`ass` filter; an event burns onto frame `index` when `event.start <= make_time(frames=index, fps=rate) < event.end`.

[LOCAL_ADMISSION]:
- `import pysubs2` at boundary scope only.
- Parse/serialize is synchronous CPU work carried off the event loop on the `execution/lanes#LANE` `WORKER_BAND` via `LanePolicy.offload`; the `pysubs2` `cli` subprocess is never composed, the owner driving `SSAFile` directly.

[RAIL_LAW]:
- Package: `pysubs2`
- Owns: the subtitle/caption DOCUMENT model — multi-dialect parse + autodetect + convert, the `SSAEvent`/`SSAStyle` record model, constant-shift and framerate-rescale retiming, style rename/import/merge, override-aware plaintext/styled-fragment projection, millisecond/frame time arithmetic, and the Whisper transcript bridge.
- Accept: editing a timed-text track as `SubtitleOp` arms — `Convert`/`Retime`/`Restyle` in-process on the worker band, `Mux` feeding serialized bytes to an `av` `SubtitleStream` passthrough, `BurnIn` feeding `plaintext`/`parse_tags` runs to the `typography`+`graphic` RGBA render composited via `av` `VideoFrame.from_bytes`, and the Whisper segment ingress — under one `async_boundary` contributing the single `ArtifactReceipt.Media` case.
- Reject: a hand-rolled per-dialect parser/serializer, the SubStation override-tag grammar, or the timestamp codec the module owns; a `convert_srt`/`retime_track`/`burn_subtitles` accessor family over the one `SubtitleOp` discriminant; subtitle packetization/muxing or container framing the `av` `SubtitleStream`/`OutputContainer.mux` layer owns; the text rasterization the `typography/shape#SHAPE`+`graphic/raster#RASTER` path owns; the raw `SSAFile`/`SSAEvent` handle crossing the owner boundary; a second receipt rail beside `ArtifactReceipt.Media`.
