# [PY_ARTIFACTS_API_PYSUBS2]

`pysubs2` is the subtitle/caption document engine for the artifacts MEDIA rail — the in-memory editing model for the timed-text track that rides alongside the `media/container#MEDIA` audio/video streams, distinct from the codec/mux layer `av` owns and from the descriptive-metadata block the `exchange/metadata#METADATA` providers carry. One `SSAFile` is the spine: a `MutableSequence[SSAEvent]` that parses any of nine timed-text formats (SubRip/ASS/SSA/WebVTT/MicroDVD/SAMI/TTML/MPL2/TMP, plus the `json` and `whisper_jax` exchange shapes) by autodetecting the source dialect, exposes each subtitle as an `SSAEvent` dataclass (`start`/`end` milliseconds + tagged `text`) and each named style as an `SSAStyle` dataclass, retimes the whole track by constant shift or framerate rescale, and re-serializes to any target dialect — the format-converter, retimer, and restyler the `media/subtitle#SUBTITLE` owner composes. The owner wraps exactly `SSAFile.load`/`from_string`/`from_file` ingest, the `MutableSequence` event surface, `SSAFile.shift`/`transform_framerate`/`sort`/`rename_style`/`import_styles`/`remove_miscellaneous_events` track edits, the `SSAEvent.plaintext`/`is_text`/`is_drawing` text projections, `pysubs2.make_time` for millisecond authoring, `load_from_whisper` for the ASR-segment bridge, and `SSAFile.save`/`to_string`/`to_file` egress into the `media/subtitle#SUBTITLE` `ContentKey`; it never re-implements the per-dialect parsers, the SubStation override-tag grammar, the timestamp codec, or the format-autodetection the module already owns, and the raw `SSAFile`/`SSAEvent` handles never cross the owner boundary — only the rendered subtitle bytes (passthrough-mux input for `av` `SubtitleStream`) and the projected `plaintext` frame text (RGBA burn-in input for `av` `VideoFrame.from_bytes`) cross.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pysubs2`
- package: `pysubs2`
- import: `pysubs2`
- owner: `artifacts`
- rail: media
- version: `1.8.1`
- license: `MIT`
- build-floor: `Requires-Python >=3.9`; pure-Python wheel (`py3-none-any`, `Root-Is-Purelib: true`, hatchling-built), no native extension, no cp-gate — resolves and runs on cp315 directly; carried on the `execution/lanes#LANE` `WORKER_BAND` process lane with the rest of the media cluster, since the parse/serialize body is synchronous CPU work dispatched off the event loop
- entry points: one console script `pysubs2 = pysubs2.cli:__main__` (batch convert/retime/clean over files; the owner composes the library `SSAFile` surface, never the CLI subprocess — the `cli` module is exposed but the owner drives `SSAFile` directly)
- capability: timed-text/subtitle document model — parse + autodetect + convert across SubRip/SubStation(ASS/SSA)/WebVTT/MicroDVD/SAMI/TTML/MPL2/TMP/JSON/WhisperJAX, the `SSAEvent` (start/end ms + tagged text + style/actor/layer/margins/effect) and `SSAStyle` (font/size/colors/bold/italic/border/alignment/margins) dataclass model, constant-shift and framerate-rescale retiming, style rename/import/merge, override-tag-aware plaintext projection, drawing/comment classification, millisecond/frame time arithmetic, and the OpenAI-Whisper transcript bridge

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the subtitle document spine and its event/style records
- rail: media

`SSAFile` is the sole stateful owner the page constructs; `SSAEvent` and `SSAStyle` are the two `dataclasses` records it reads and mutates (mutable, field-exposed — `SSAEvent.start`/`end`/`text`/`style`/`layer`/`name`/`marginl`/`marginr`/`marginv`/`effect`/`type`, `SSAStyle.fontname`/`fontsize`/`primarycolor`/`bold`/`italic`/`alignment`/`borderstyle`/`outline`/`shadow`/`marginl`/`marginr`/`marginv`). `Color` (8-bit RGBA, range-checked 0–255) and `Alignment` (numpad-style `IntEnum` 1–9, ASS semantics) are the two value vocabularies styles carry. `SSAFile` IS-A `MutableSequence[SSAEvent]`, so the standard-library ABC supplies `append`/`extend`/`index`/`count`/`pop`/`remove`/`reverse`/`__contains__`/`__iadd__` as mixins over the four concrete dunders (`__getitem__`/`__setitem__`/`__delitem__`/`__len__`) plus `insert` — the owner never re-implements list mutation, it slices and appends `SSAEvent` rows directly.

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE]              | [CAPABILITY]                                                                               |
| :-----: | :---------- | :-------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `SSAFile`   | `MutableSequence[SSAEvent]` | parse/convert/retime/restyle spine; `events`/`styles`/`info`/`fps`/`format`                |
|  [02]   | `SSAEvent`  | mutable `@dataclass`        | timed line: `start`/`end`, tagged `text`, `style`/`name`/`layer`/`margins`/`effect`/`type` |
|  [03]   | `SSAStyle`  | mutable `@dataclass`        | named font/size/color/bold/italic/border/`alignment`/`outline`/`shadow`/margin style       |
|  [04]   | `Color`     | RGBA `@dataclass` value     | range-validated 8-bit RGB+alpha styles carry                                               |
|  [05]   | `Alignment` | numpad 1–9 `IntEnum`        | ASS text anchor; `from_ssa_alignment`/`to_ssa_alignment` bridge SSA numbering              |

[PUBLIC_TYPE_SCOPE]: the typed error rail
- rail: media

The module roots one exception hierarchy at `Pysubs2Error`; the owner maps it onto the `media/subtitle#SUBTITLE` `RuntimeRail` typed-error envelope at the parse/serialize boundary (a malformed or ambiguous source surfaces as a typed media fault, never a bare `Exception`), and traps the standard-library `OSError`/`UnicodeDecodeError`/`UnicodeEncodeError` the `Path.open` ingest/egress can raise beside them. `FormatAutodetectionError` carries the analyzed `content` and the candidate `formats` list; `UnknownFileExtensionError` carries `ext`; `UnknownFormatIdentifierError` carries `format_` — each with a `__reduce__` for clean cross-process propagation off the worker band.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                                                                 |
| :-----: | :----------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Pysubs2Error`                 | base error     | hierarchy root and owner's subtitle-boundary `except` anchor                 |
|  [02]   | `FormatAutodetectionError`     | error          | ambiguous/unknown scanned dialect; carries `content` and candidate `formats` |
|  [03]   | `UnknownFormatIdentifierError` | error          | unregistered `save`/`to_string` `format_`; carries `format_`                 |
|  [04]   | `UnknownFileExtensionError`    | error          | unmapped save extension when `format_` is omitted; carries `ext`             |
|  [05]   | `UnknownFPSError`              | error          | MicroDVD fps neither supplied nor inferred                                   |

[PUBLIC_TYPE_SCOPE]: the format registry and dialect vocabulary
- rail: media
- values: `srt`/`ass`/`ssa`/`microdvd`/`json`/`mpl2`/`tmp`/`vtt`/`sami`/`whisper_jax`/`ttml`
- mapping: `.srt→srt`/`.ass→ass`/`.vtt→vtt`/`.sub→microdvd`/`.smi→sami`…

The dialect set is data, not code: `pysubs2.formats` carries the closed format-identifier ↔ file-extension ↔ implementation tables the owner's convert axis keys into. `FORMAT_IDENTIFIERS` is the canonical `list[str]` of every writable dialect the owner's target-format discriminant ranges over; `FILE_EXTENSION_TO_FORMAT_IDENTIFIER` and `FORMAT_IDENTIFIER_TO_FORMAT_CLASS` are the two maps `SSAFile.save` and `SSAFile.from_file` resolve through. The owner never instantiates a `FormatBase` subclass directly — it passes a verified `format_` string from this table.

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE]    | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `formats.FORMAT_IDENTIFIERS`                  | `list[str]`       | canonical writable dialect identifiers                |
|  [02]   | `formats.FILE_EXTENSION_TO_FORMAT_IDENTIFIER` | `dict[str, str]`  | save-path extension codec                             |
|  [03]   | `formats.FORMAT_IDENTIFIER_TO_FORMAT_CLASS`   | `dict[str, type]` | dialect-to-`FormatBase` parse/serialize dispatch      |
|  [04]   | `formats.autodetect_format`                   | `(str) -> str`    | unique fragment dialect or `FormatAutodetectionError` |
|  [05]   | `formats.get_format_identifier`               | function          | extension-to-identifier `save`/`load` resolution      |
|  [06]   | `get_file_extension`                          | function          | identifier-to-extension `save`/`load` resolution      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest — parse and autodetect
- rail: media
- call: `load(path: str \| PathLike, encoding: str = "utf-8", format_: str \| None = None, fps: float \| None = None, errors: str \| None = None, **kwargs) -> SSAFile`
- call: `from_string(string: str, format_: str \| None = None, fps: float \| None = None, **kwargs) -> SSAFile`
- call: `from_file(fp: TextIO, format_: str \| None = None, fps: float \| None = None, **kwargs) -> SSAFile`
- call: `load_from_whisper(result_or_segments: dict \| list[dict]) -> SSAFile`

`SSAFile.load(path, encoding="utf-8", format_=None, fps=None, errors=None)` is the primary parse: it opens the path in text mode and delegates to `from_file`, which — when `format_` is `None` — buffers the stream, scans the first 10 000 chars through `autodetect_format`, and re-parses with the resolved dialect (so a pipe is read twice safely), setting `SSAFile.format` and `SSAFile.fps` from the detection. `from_string(string, …)` is the in-memory mirror over a `StringIO`; `from_file(fp, …)` is the low-level `TextIO` reader both build on. `errors="surrogateescape"` (added release) passes through bytes the chosen encoding cannot decode — the owner's posture for unknown-encoding subtitle sidecars. The module-level `pysubs2.load` is the `SSAFile.load` alias; `load_from_whisper(result_or_segments)` is the ASR bridge that folds a Whisper `{"segments": [{start, end, text}]}` dict (or the bare segment list) into a fresh `SSAFile`, seconds→ms via `make_time(s=…)`.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                      | [CAPABILITY]                                                 |
| :-----: | :--------------------------------- | :-------------------------------- | :----------------------------------------------------------- |
|  [01]   | `SSAFile.load`                     | `load(…) -> SSAFile`              | path parse; dialect/fps detect; `errors="surrogateescape"`   |
|  [02]   | `pysubs2.load`                     | `load(…) -> SSAFile`              | module alias for path parse                                  |
|  [03]   | `SSAFile.from_string`              | `from_string(…) -> SSAFile`       | parse Unicode `str`, not bytes                               |
|  [04]   | `SSAFile.from_file`                | `from_file(…) -> SSAFile`         | buffered text-stream autodetect-then-reparse core            |
|  [05]   | `load_from_whisper`                | `load_from_whisper(…) -> SSAFile` | Whisper `segments` of `{start, end, text}` seconds to events |
|  [06]   | `formats.whisper.WhisperJAXFormat` | parse `format_="whisper_jax"`     | registered, autodetected `[hh:mm:ss.mmm -> …] text` dialect  |

[ENTRYPOINT_SCOPE]: the event surface — read, mutate, project text
- rail: media
- call: `subs[i] -> SSAEvent`; `subs[a:b] -> list[SSAEvent]`; `subs.insert(i, ev)`; `subs.append(ev)`
- call: `SSAEvent(start: int = 0, end: int = 10000, text: str = "", style: str = "Default", layer: int = 0, name: str = "", marginl: int = 0, marginr: int = 0, marginv: int = 0, effect: str = "", type: Literal["Dialogue","Comment"] = "Dialogue")`
- call: `ev.plaintext -> str` (read/write)
- call: `ev.duration -> int` (read/write)
- call: `ev.is_text -> bool` …
- call: `shift(h=0, m=0, s=0, ms=0, frames=None, fps=None) -> None`
- call: `SSAStyle(fontname="Arial", fontsize=20.0, primarycolor=Color(255,255,255), bold=False, italic=False, alignment=Alignment.BOTTOM_CENTER, borderstyle=1, outline=2.0, shadow=2.0, marginl=10, marginr=10, marginv=10, …)`
- call: `ev.copy() -> SSAEvent`; `ev.as_dict() -> dict[str, Any]`; `ev.equals(other) -> bool`

`SSAFile` mutates as a list of `SSAEvent`: `subs[i]`/`subs[i] = ev`/`del subs[i]`/`len(subs)`/`subs.insert(i, ev)` are the concrete dunders (slices supported and type-checked — non-`SSAEvent` values raise `TypeError`), and the `MutableSequence` mixins (`append`/`extend`/`index`/`count`/`pop`/`remove`/`reverse`/`in`) come free. Each `SSAEvent` carries `start`/`end` as milliseconds (use `make_time` to author them), `text` with raw SubStation override tags, and the read/write `plaintext` property that strips `{…}` override sequences and normalizes `\N`/`\n`/`\h` to newlines/spaces — the projection the burn-in path renders. `duration` is a read/write property (writing adjusts `end`; negative raises `ValueError`); `is_comment`/`is_text`/`is_drawing` classify a line (drawing-tag detection runs `parse_tags`); `SSAEvent.shift(...)` retimes one line. `SSAEvent(start=…, end=…, text=…)` and `SSAStyle(...)` construct directly; `.copy()`/`.as_dict()`/`.equals(other)` clone/project/compare without dictifying nested `Color`s.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                     | [CAPABILITY]                                                       |
| :-----: | :------------------- | :------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `SSAFile.getitem`    | `subs[i]`; `subs[a:b]`           | event access; type-checked slices yield `list[SSAEvent]`           |
|  [02]   | `insert`             | `subs.insert(i, ev)`             | indexed event insertion via `MutableSequence`                      |
|  [03]   | `append`             | `subs.append(ev)`                | event append via `MutableSequence`                                 |
|  [04]   | `SSAEvent.init`      | `SSAEvent(…)`                    | construct a millisecond-timed subtitle line                        |
|  [05]   | `SSAEvent.plaintext` | `ev.plaintext` read/write        | tags stripped; newlines normalized; writing re-encodes `n`→`N`     |
|  [06]   | `SSAEvent.duration`  | `ev.duration` read/write         | `end - start`; writing adjusts `end`; negatives raise `ValueError` |
|  [07]   | `SSAEvent.is_text`   | `ev.is_text -> bool`             | classify visible text excluding drawings/comments                  |
|  [08]   | `is_comment`         | boolean property                 | classify comment lines                                             |
|  [09]   | `is_drawing`         | boolean property                 | classify SSA drawing-tag lines                                     |
|  [10]   | `SSAEvent.shift`     | `shift(…) -> None`               | in-place time- or frame-based line retiming                        |
|  [11]   | `SSAStyle.init`      | `SSAStyle(…)`                    | construct one named style                                          |
|  [12]   | `SSAEvent.copy`      | `ev.copy() -> SSAEvent`          | clone an event                                                     |
|  [13]   | `as_dict`            | `ev.as_dict() -> dict[str, Any]` | shallow projection preserving nested `Color` values                |
|  [14]   | `equals`             | `ev.equals(other) -> bool`       | event field-equality comparison                                    |

[ENTRYPOINT_SCOPE]: retiming and style operations on the track
- rail: media
- call: `shift(h=0, m=0, s=0, ms=0, frames: int \| None = None, fps: float \| None = None) -> None`
- call: `transform_framerate(in_fps: float, out_fps: float) -> None`
- call: `sort() -> None`
- call: `rename_style(old_name: str, new_name: str) -> None`
- call: `import_styles(subs: SSAFile, overwrite: bool = True) -> None`
- call: `remove_miscellaneous_events() -> None`; `get_text_events() -> list[SSAEvent]`

`SSAFile.shift(h, m, s, ms, frames, fps)` translates every event by one delta (time- or frame-based; `make_time` builds the delta); `transform_framerate(in_fps, out_fps)` rescales every timestamp by the `in_fps/out_fps` ratio (the fix for a frame-rate-mismatched conversion; non-positive fps raises `ValueError`); `sort()` orders events by `(start, end)` in place. Style ops: `rename_style(old, new)` renames a style and rewrites every event referencing it (validates the new name against the SubStation field grammar); `import_styles(other, overwrite=True)` merges styles from another `SSAFile`. `remove_miscellaneous_events()` is the CLI `--clean` — drops comments, drawing-tag lines, sub-2-char text, and time-identical duplicates; `get_text_events()` returns the non-comment/non-drawing subset. `equals(other)` is the deep structural compare over `info`/`styles`/`events`.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                     | [CAPABILITY]                                              |
| :-----: | :------------------------------------ | :------------------------------- | :-------------------------------------------------------- |
|  [01]   | `SSAFile.shift`                       | `shift(…) -> None`               | constant time- or frame-delta track translation           |
|  [02]   | `SSAFile.transform_framerate`         | `transform_framerate(…) -> None` | rescale by `in_fps/out_fps`; fix framerate mismatch       |
|  [03]   | `SSAFile.sort`                        | `sort() -> None`                 | in-place `(start, end)` event ordering                    |
|  [04]   | `SSAFile.rename_style`                | `rename_style(…) -> None`        | rename and rewrite references; reject taken/illegal names |
|  [05]   | `SSAFile.import_styles`               | `import_styles(…) -> None`       | merge styles with `overwrite` conflict policy             |
|  [06]   | `SSAFile.remove_miscellaneous_events` | `remove_miscellaneous_events()`  | drop non-essential `--clean` lines                        |
|  [07]   | `get_text_events`                     | `get_text_events()`              | select visible text events as `list[SSAEvent]`            |

[ENTRYPOINT_SCOPE]: egress — serialize to any dialect
- rail: media
- call: `save(path: str \| PathLike, encoding: str = "utf-8", format_: str \| None = None, fps: float \| None = None, errors: str \| None = None, **kwargs) -> None`
- call: `to_string(format_: str, fps: float \| None = None, **kwargs) -> str`
- call: `to_file(fp: TextIO, format_: str, fps: float \| None = None, **kwargs) -> None`
- call: `SSAFile.from_string(...).to_string(...)` parses `format_=src`, serializes `format_=dst`, and supplies one-shot `srt`→`vtt` or `ass`→`srt` conversion without a per-pair function

`SSAFile.save(path, encoding="utf-8", format_=None, fps=None, errors=None)` writes the track; with `format_=None` the dialect is inferred from the path extension (else `UnknownFileExtensionError`). `to_string(format_, fps=None)` returns the serialized text directly (the in-memory egress the owner keys a `ContentKey` over — no temp file), and `to_file(fp, format_, …)` is the low-level `TextIO` writer. A target `format_` not in `FORMAT_IDENTIFIERS` raises `UnknownFormatIdentifierError`; a MicroDVD save without an fps raises `UnknownFPSError`. The owner's convert axis is exactly `SSAFile.from_string(text, format_=src).to_string(format_=dst)` — one parse, one serialize, no per-pair converter.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]          | [CAPABILITY]                                                   |
| :-----: | :------------------ | :-------------------- | :------------------------------------------------------------- |
|  [01]   | `SSAFile.save`      | `save(…) -> None`     | path serialization; infer dialect from extension unless forced |
|  [02]   | `SSAFile.to_string` | `to_string(…) -> str` | in-memory `str` and `ContentKey` convert/restyle egress        |
|  [03]   | `SSAFile.to_file`   | `to_file(…) -> None`  | low-level text-stream writer                                   |

[ENTRYPOINT_SCOPE]: time arithmetic and override-tag parsing
- rail: media
- call: `make_time(h=0, m=0, s=0, ms=0, frames: int \| None = None, fps: float \| None = None) -> int`
- call: `ms_to_str(ms, fractions: bool = False) -> str`
- call: `ms_to_times(ms) -> Times(h,m,s,ms)`; `times_to_ms(h=0,m=0,s=0,ms=0) -> int`
- call: `frames_to_ms(frames, fps) -> int`; `ms_to_frames(ms, fps) -> int`
- call: `parse_tags(text: str, style: SSAStyle = SSAStyle.DEFAULT_STYLE, styles: dict[str, SSAStyle] \| None = None, skip_empty_fragments: bool = False) -> list[tuple[str, SSAStyle]]`

`pysubs2.make_time(h, m, s, ms, frames, fps)` is the canonical ms constructor (`make_time(s=1.5) == 1500`; `make_time(frames=50, fps=25) == 2000`; mixing frames without fps raises `ValueError`) — every `start`/`end`/`shift` delta authors through it. The `pysubs2.time` module exposes the full conversion family the owner reads for frame-accurate retiming and timestamp rendering; `pysubs2.formats.substation.parse_tags(text, style=…, styles=…)` splits a tagged `text` into `(fragment, computed_SSAStyle)` runs — the styled-fragment decomposition the per-run RGBA burn-in render consumes when the burn must honor inline `\i`/`\b`/`\fn`/`\r` overrides rather than flatten to `plaintext`.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                  | [CAPABILITY]                                                 |
| :-----: | :------------------------------ | :-------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `pysubs2.make_time`             | `make_time(…) -> int`                         | author milliseconds from unit mixes or frame counts          |
|  [02]   | `time.ms_to_str`                | `ms_to_str(…) -> str`                         | render ms as `[-]H:MM:SS[.mmm]` for display/serialization    |
|  [03]   | `time.ms_to_times`              | `ms_to_times(…) -> Times`                     | normalized `(h,m,s,ms)` tuple from milliseconds              |
|  [04]   | `times_to_ms`                   | `times_to_ms(…) -> int`                       | normalized `(h,m,s,ms)` tuple to milliseconds                |
|  [05]   | `time.frames_to_ms`             | `frames_to_ms(…) -> int`                      | frame-to-ms conversion at a framerate                        |
|  [06]   | `ms_to_frames`                  | `ms_to_frames(…) -> int`                      | ms-to-frame conversion; non-positive fps raises `ValueError` |
|  [07]   | `formats.substation.parse_tags` | `parse_tags(…) -> list[tuple[str, SSAStyle]]` | tagged text to styled fragments for override-aware burn-in   |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_STACK]:
- `media/subtitle#SUBTITLE` ownership: `pysubs2` is the single subtitle-document provider on the NEW `media/subtitle` page, the timed-text arm of the MEDIA rail that sits beside `media/container#MEDIA` (the `av` mux spine), `media/audio#MEDIA`, and `media/filtergraph#FILTER`. The owner is a frozen `msgspec.Struct` whose op is ONE `expression.tagged_union` `SubtitleOp` over the subtitle modalities — `Whisper(payload, dialect)` (ASR-segment admission through `load_from_whisper`), `Convert(text, src, dst)` (dialect transcode), `Retime(text, dialect, shift)` (constant-shift / framerate-rescale), `Restyle(text, dialect, *ops)` (style rename/import/merge), `Mux(text, container, dialect)` (passthrough subtitle-stream mux into the `av` container), and `BurnIn(text, dialect, frames, profile, burn)` (RGBA overlay composite onto a video frame sequence) — dispatched by one total `match`/`case` closed by `assert_never`, returning `RuntimeRail[ContentKey]` keyed over the produced subtitle/container bytes, never a parallel `convert_srt`/`retime`/`burn` function family nor a per-dialect owner. The dialect set is the `formats.FORMAT_IDENTIFIERS` table folded into one `SubtitleDialect` `StrEnum` keyed inside the op's typed payload, never a free `format_` string the implementer re-validates.
- convert/retime/restyle fold: the three pure-edit arms are in-process synchronous `SSAFile` work — `SSAFile.from_string(text, format_=src.value)` parses once, the arm applies the edit (`save`→`to_string` for convert; `shift`/`transform_framerate` for retime; `rename_style`/`import_styles` for restyle), and `to_string(format_=dst.value)` re-serializes to the bytes the `ContentKey` is keyed over — one parse, one serialize, the module owning the per-dialect codec and the override grammar. A `Pysubs2Error` (autodetect ambiguity, unknown dialect, missing fps) or an `OSError`/`UnicodeError` surfaces through the `media.subtitle.{convert,retime,restyle}` `async_boundary` as a typed `RuntimeRail` fault, never an `SSAFile`-specific try/except rail on the page.
- passthrough-mux fold (`av` stack): the `Mux` arm composes the folder `av` `.api` `SubtitleStream` capability — `out = container.add_stream("ass" | "srt")` (or `add_stream_from_template` cloning a demuxed subtitle stream) mints the timed-text output stream, and the `pysubs2`-serialized subtitle bytes mux through `OutputContainer.mux`/`mux_one` packet-by-packet beside the `av` video/audio streams (`av` `.api` `[02]` row `[05]` `SubtitleStream` "demux/mux passthrough", `[03]` rows `[03]`/`[05]` `add_stream`/`add_stream_from_template`). `pysubs2` owns the subtitle DOCUMENT (parse/convert/restyle into the muxable dialect); `av` owns the container/packet layer — the seam is the serialized subtitle byte stream, never a re-implementation of subtitle packetization, and the `av` handle never crosses back into `pysubs2`.
- burn-in fold (`av` + `typography`/`graphic` stack): the `BurnIn` arm is the in-process substitute the `media/filtergraph#FILTER` capability-detection rail selects when the linked FFmpeg lacks the `subtitles`/`ass` filter (libass absent). Per event active at a frame's `pts`, the owner takes `SSAEvent.plaintext` (or the `substation.parse_tags` styled-fragment runs for override-faithful rendering), renders it to an RGBA buffer through the `typography/shape#SHAPE` + `graphic/raster#RASTER` text path, and composites via `av` `VideoFrame.from_bytes(data, width, height, format="rgba")` overlaid onto the `media/container#MEDIA` rgb24 frame (the `overlay` filter node or a numpy alpha-composite), the exact absent-filter→verified-substitute map the brief fixes. The subtitle timing drives which event burns onto which frame: an event burns onto frame `index` when `event.start <= make_time(frames=index, fps=rate) < event.end`, the `time.make_time`/`ms_to_frames` arithmetic owning the frame↔ms mapping.
- whisper bridge fold: the `Whisper(payload, dialect)` case is the ASR ingress arm — `Subtitle.of_whisper` admits the typed `WhisperPayload` (`{"segments": [...]}` result or bare segment list), the worker's `_whisper` runs `load_from_whisper` and serializes to the case's dialect, so an `media/analysis#ANALYSIS` speech-to-text segment set lands as an editable subtitle track the `Convert`/`Retime`/`Restyle`/`BurnIn` arms then drive, never a hand-rolled segment→subtitle loop.
- universal-tier stacking (`libs/python/.api`): `LanePolicy.offload(worker, ..., modality=Modality.PROCESS, retry=RetryClass.OCCT)` carries synchronous `SSAFile` work off the event loop; `expression.tagged_union` owns the `SubtitleOp` discriminant and `Result`/`RuntimeRail` the typed-error fold; `msgspec.Struct` carries the op/evidence owners; `numpy` carries the RGBA/rgb24 burn buffers. `_crossed` maps the lane's outer `BoundaryFault` through the media `_lapsed` fold and flattens the worker rail.
- receipt seam: each subtitle production contributes the existing `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate, facts)` case. `Convert`/`Retime`/`Restyle`/`Whisper` project dialect, `"text"`, track duration, byte count, event count, zero bit rate, and `{events, styles}` facts; `Mux`/`BurnIn` merge those counts onto probed container facts. `SubtitleEvidence` contains no provider handle, and the receipt owner imports no `pysubs2` type.

[RAIL_LAW]:
- Package: `pysubs2`
- Owns: the subtitle/caption DOCUMENT model — multi-dialect parse + autodetect + convert (SubRip/ASS/SSA/WebVTT/MicroDVD/SAMI/TTML/MPL2/TMP/JSON/WhisperJAX), the `SSAEvent`/`SSAStyle` record model, constant-shift and framerate-rescale retiming, style rename/import/merge, override-tag-aware plaintext/styled-fragment projection, millisecond/frame time arithmetic, and the OpenAI-Whisper transcript bridge
- Accept: editing a timed-text track as the `media/subtitle#SUBTITLE` `SubtitleOp` arms — `Convert`/`Retime`/`Restyle` in-process on the worker band, `Mux` feeding serialized subtitle bytes to an `av` `SubtitleStream` passthrough, `BurnIn` feeding `SSAEvent.plaintext`/`parse_tags` runs to the `typography`+`graphic` RGBA render composited via `av` `VideoFrame.from_bytes`, the Whisper segment ingress, all under one `async_boundary` contributing the single `ArtifactReceipt.Media` case
- Reject: a hand-rolled per-dialect subtitle parser/serializer, the SubStation override-tag grammar, or the timestamp codec the module already owns; a `convert_srt`/`retime_track`/`burn_subtitles` accessor family over one `SubtitleOp` discriminant; subtitle PACKETIZATION/muxing or container framing (the `av` `SubtitleStream`/`OutputContainer.mux` layer owns the wire, `pysubs2` owns the document); the text-rendering/rasterization the `typography/shape#SHAPE`+`graphic/raster#RASTER` path owns (`pysubs2` hands `plaintext`/styled-fragment runs, never pixels); the codec/audio/video encode `av` owns (`media/container#MEDIA`); the `cli` subprocess where the library `SSAFile` surface composes directly; the raw `SSAFile`/`SSAEvent` handle crossing the owner boundary (only serialized subtitle bytes and projected text cross); a second receipt rail where the single `ArtifactReceipt.Media` case carries the subtitle evidence
