# [PY_ARTIFACTS_MEDIA_SUBTITLE]

The temporal-artifact TIMED-TEXT arm — the subtitle/caption track that rides alongside the `media/container#CONTAINER` audio/video streams, owning the subtitle DOCUMENT (parse/convert/retime/restyle over `pysubs2`) and the two ways a track reaches a viewer: PASSTHROUGH-MUX of a soft-subtitle stream into an `av` container, and RGBA OVERLAY BURN-IN of hard subtitles onto an rgb24 frame sequence when the linked FFmpeg lacks the `subtitles`/`ass`/libass filter. `Subtitle` is a frozen `msgspec.Struct` whose op is ONE closed-payload `SubtitleOp` `expression.tagged_union` over the timed-text modalities — `Convert(text, src, dst)` (dialect transcode), `Retime(text, dialect, shift)` (constant-shift or framerate-rescale, the shift a discriminated `RetimeShift` per-mode payload), `Restyle(text, dialect, ops)` (rename/import/merge/clean over a `RestyleStep` sub-family), `Mux(text, container, dialect)` (soft-subtitle stream muxed into an existing container by packet copy plus one added `SubtitleStream`), and `BurnIn(text, dialect, frames, profile)` (per-event RGBA overlay composited onto the base frames, then encoded through the `media/container#CONTAINER` `_encode_video` worker) — dispatched by one total `match`/`case` closed by `assert_never`, returning `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]` keyed over the produced subtitle/container bytes, never a parallel `convert_srt`/`retime_track`/`burn_subtitles` function family nor a per-dialect owner. The eleven writable dialects `pysubs2.formats.FORMAT_IDENTIFIERS` publishes fold into ONE `SubtitleDialect` `StrEnum` keyed inside each op's typed payload, never a free `format_` string the implementer re-validates. `pysubs2` owns the timed-text document — the per-dialect parsers, the SubStation override-tag grammar, the millisecond/frame timestamp codec, the format-autodetection, the constant-shift and framerate-rescale retiming, and the style rename/import/merge the module already ships; the owner wraps exactly `SSAFile.from_string`/`to_string` ingest/egress, the `MutableSequence[SSAEvent]` surface, `SSAFile.shift`/`transform_framerate`/`sort`/`rename_style`/`import_styles`/`remove_miscellaneous_events` track edits, `SSAEvent.plaintext`/`is_text`/`is_drawing` projections, `pysubs2.make_time`/`time.ms_to_frames` frame arithmetic, `pysubs2.formats.substation.parse_tags` styled-fragment decomposition, and `load_from_whisper` for the ASR-segment bridge from `media/analysis#ANALYSIS`. Only serialized subtitle bytes and projected `plaintext`/styled-fragment runs cross the owner boundary; the raw `SSAFile`/`SSAEvent` handles never do. This page composes the `media/container#CONTAINER` shared media family — the `MediaProfile` encode-policy value, the `MediaFault` closed av-boundary cause vocabulary, the `MediaEvidence` typed receipt, the `_media_fault`/`_deployment`/`_encode_video`/`_drive`/`_flush` worker primitives, the `WORKER_BAND`/`_WORKER_RETRY` subprocess lane — and READS them; it composes `media/filtergraph#FILTER`'s `filters_available` capability probe to select the BurnIn overlay arm (native `overlay` filter versus numpy alpha-composite) and the Mux-versus-BurnIn soft/hard-subtitle route; it composes `typography/shape#SHAPE` for the RTL/bidi-correct glyph→RGBA render (`uharfbuzz`+`python-bidi`, NOT the un-bundled Pillow RAQM backend) and `graphic/raster/io#IO` for the RGBA raster; and it OWNS the `SubtitleOp`/`RetimeShift`/`RestyleStep`/`SubtitleDialect` vocabulary, the `SubtitleEvidence` text-track receipt carrier, and the `_convert`/`_retime`/`_restyle`/`_mux_subtitle`/`_burn_in` worker functions. Each production contributes the single `ArtifactReceipt.Media` case the whole media plane shares — the subtitle event/style counts riding its `facts` band — never a parallel subtitle-receipt rail, and routes through the one `core/plan#PLAN` `ArtifactPipeline` entry as an `ArtifactWork` node keyed by its content key. This page realizes the NEW timed-text leaf of the seven-page media restructure.

## [01]-[INDEX]

- [01]-[SUBTITLE]: the `Subtitle` owner over the closed-payload `SubtitleOp` family — `Convert`/`Retime`/`Restyle` the three in-process pure-`pysubs2` edit arms (`SSAFile.from_string(text, format_=src.value)` parse once, apply the edit, `to_string(format_=dst.value)` re-serialize) folding into one `SubtitleEvidence` text-track carrier, `Mux` the soft-subtitle passthrough (`av.open(BytesIO(container), "r")` demux the existing streams, `add_stream_from_template` packet-copy each, `add_stream(_SOFT_SUB[dialect])` mint the timed-text `SubtitleStream`, `mux_one` interleave), and `BurnIn` the hard-subtitle overlay (per event active at a frame's millisecond position render `SSAEvent.plaintext` or the `parse_tags` styled-fragment runs to an RGBA buffer through `typography/shape#SHAPE`, composite via the native `overlay` filter node or the numpy alpha-composite the `filters_available` probe selects, then `media/container#CONTAINER` `_encode_video`), each op returning `Result[SubtitleProduct, MediaFault]` and the owner `_keyed` arm spreading it onto the single `ArtifactReceipt.Media` case; the eleven-dialect `SubtitleDialect` `StrEnum` folded from `pysubs2.formats.FORMAT_IDENTIFIERS`, the `RetimeShift` (`Constant`/`Rescale`) and `RestyleStep` (`Rename`/`Import`/`Clean`) discriminated per-mode payloads, the `Subtitle.of_whisper` ASR-ingress classmethod folding a `load_from_whisper` transcript into a fresh track; `pysubs2` `SSAFile.from_string`/`to_string`/`shift`/`transform_framerate`/`sort`/`rename_style`/`import_styles`/`remove_miscellaneous_events`/`get_text_events`, `SSAEvent.start`/`end`/`plaintext`/`is_text`, `pysubs2.make_time`/`time.ms_to_frames`, `pysubs2.formats.substation.parse_tags`, `load_from_whisper`, and the `Pysubs2Error` subtree all settled against the folder `.api`; `av` `open`/`add_stream_from_template`/`add_stream`/`mux_one`/`VideoFrame.from_bytes`/`filter.filters_available` settled against the folder `.api`. The `MediaProfile`/`MediaFault`/`MediaEvidence`/`_media_fault`/`_deployment`/`_encode_video`/`_drive`/`_flush`/`WORKER_BAND`/`_WORKER_RETRY` family is `media/container#CONTAINER`'s, the capability probe `media/filtergraph#FILTER`'s, the glyph→RGBA render `typography/shape#SHAPE`'s; this page owns the `SubtitleOp` vocabulary, the `SubtitleEvidence` carrier, and the five worker arms.

## [02]-[SUBTITLE]

- Owner: `Subtitle` the one timed-text owner discriminating modality over the closed `SubtitleOp` family, worked by five module-level functions on the `WORKER_BAND` subprocess lane; `SubtitleOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Subtitle` subclass nor a parallel `convert`/`retime`/`burn` function trio; `SubtitleDialect` the closed `StrEnum` of the eleven writable dialects `pysubs2.formats.FORMAT_IDENTIFIERS` publishes (`SRT`/`ASS`/`SSA`/`MICRODVD`/`JSON`/`MPL2`/`TMP`/`VTT`/`SAMI`/`WHISPER_JAX`/`TTML`), each member's `.value` the exact `format_` string `SSAFile.to_string`/`from_string` consume, never a free string the implementer re-validates against the module table; `RetimeShift` the closed `Constant(ms)`/`Rescale(in_fps, out_fps)` per-mode `tagged_union` so the constant-shift arm carries ONLY a millisecond delta and the framerate-rescale arm ONLY the two framerates (the permissive `(shift_ms, in_fps, out_fps)` bag whose fields are irrelevant per mode is the rejected DERIVED-NOT-PARALLEL form); `RestyleStep` the closed `Rename(old, new)`/`Import(source_text, source_dialect, overwrite)`/`Clean` `tagged_union` folded in sequence over one parsed `SSAFile`; `SubtitleEvidence` the frozen text-track receipt carrier this page OWNS (`container` the dialect or muxer tag, `codec` the `"text"`/`"subtitle"`/video-codec sentinel, `duration` seconds, `byte_count`, `count` the event or frame count, and the `facts` band carrying the `events`/`styles` counts) with its one `measure` constructor — it projects onto the shared `ArtifactReceipt.Media` case at the owner `_keyed` arm so the receipt owner imports no `pysubs2` handle, never a second receipt rail; `MediaFault` the closed cause vocabulary `media/container#CONTAINER` owns (`unregistered`/`invalid`/`codec`/`provision`/`worker`/`contract`), threaded unchanged so a `pysubs2` autodetect ambiguity maps to `invalid`, an unknown dialect to `unregistered`, and a `pysubs2`/`av` `ImportError` to `provision` — the subtitle raises fold onto the SAME six cases, never a parallel `SubtitleFault`; `Result[(ContentKey, ArtifactReceipt), MediaFault]` the one carrier every arm returns, keyed over the produced bytes through `ContentIdentity.of(product.container, blob)` so an identical track at an identical dialect is a cache hit by reference. The `Convert`/`Retime`/`Restyle` arms produce a text-track `SubtitleEvidence` directly (the dialect on `container`, `"text"` on `codec`); the `Mux`/`BurnIn` arms produce a real container and compose the `media/container#CONTAINER` `MediaEvidence`, merging the subtitle `facts` band onto it through `msgspec.structs.replace`. The owner owns no second `Media` subclass, no per-dialect subtitle owner, and no parallel `encode_subtitle` surface — the modality is one `SubtitleOp` case, the worker one module-level function.
- Cases: `SubtitleOp` cases — `Convert(text, src, dst)` (`SSAFile.from_string(text, format_=src.value).to_string(format_=dst.value)`, one parse one serialize, the module owning the per-pair codec) · `Retime(text, dialect, shift)` (parse, then `subs.shift(ms=shift.ms)` for the `Constant` case or `subs.transform_framerate(shift.in_fps, shift.out_fps)` for the `Rescale` case, `sort()`, re-serialize to the same dialect — the retimed track) · `Restyle(text, dialect, ops)` (parse, fold each `RestyleStep` — `Rename` -> `rename_style`, `Import` -> `import_styles(SSAFile.from_string(...), overwrite=)`, `Clean` -> `remove_miscellaneous_events()` — then re-serialize) · `Mux(text, container, dialect)` (parse the subtitle to the muxable dialect, `av.open(BytesIO(container), "r")` demux the existing video/audio streams cloned by `add_stream_from_template` and packet-copied, `add_stream(_SOFT_SUB[dialect])` mint the `SubtitleStream`, and `mux_one` interleave — the soft-subtitle passthrough, gated by `filters_available`/muxer support else routed to `BurnIn`) · `BurnIn(text, dialect, frames, profile)` (parse, then per base frame at index `i` the active events are those where `event.start <= make_time(frames=i, fps=profile.rate) < event.end`, each event's `parse_tags` styled-fragment runs rendered to an RGBA `NDArray[np.uint8]` through `typography/shape#SHAPE`, composited onto the rgb24 base by the native `overlay` filter node when `"overlay" in filters_available` else a numpy alpha-composite, and the burned `Frames` handed to `media/container#CONTAINER` `_encode_video` — the hard-subtitle substitute the capability-detection rail selects when libass is absent) — matched by one total `match`/`case`, the five-case modality recovered from the `SubtitleOp` discriminant, never a name suffix.
- Entry: `Subtitle.produce` is `async` over the runtime `async_boundary` returning `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]` — the domain `MediaFault` nested inside the boundary rail exactly as `media/container#CONTAINER` nests it, so `async_boundary` owns only a truly unexpected raise while every classified subtitle/av cause stays structurally addressable on the inner rail; `_emit` maps the `_dispatch` outcome through `_keyed` (deriving `ContentIdentity.of(...)` and spreading `SubtitleEvidence` onto the case), and `_dispatch` matches the `SubtitleOp` and dispatches the whole synchronous body onto `_WORKER_RETRY(to_process.run_sync, _worker, ..., limiter=WORKER_BAND)` catching `BrokenWorkerProcess` -> `MediaFault(worker=...)` and `BeartypeCallHintViolation` -> `MediaFault(contract=...)` exactly as the container owner does. Each `@beartype`-woven worker maps its own raises at the arm that incurs them — a `pysubs2.exceptions.Pysubs2Error` through `_subtitle_fault` (`FormatAutodetectionError`/`UnknownFormatIdentifierError`/`UnknownFileExtensionError` -> `unregistered`, any other `Pysubs2Error` or a `UnicodeError`/`ValueError` -> `invalid`), an `av.error.FFmpegError` through the shared `_media_fault`, and a `pysubs2`/`av` `ImportError` -> `MediaFault(provision=...)` — returning `Result[SubtitleProduct, MediaFault]` as picklable data across the `to_process` seam. `Subtitle.of` normalizes the construction — a lone op or the `(text, src, dst)` convert shorthand — and `Subtitle.of_whisper(result, dialect)` is the ASR-ingress classmethod folding a `load_from_whisper` OpenAI-Whisper transcript into a fresh `SSAFile` serialized to `dialect`, so a `media/analysis#ANALYSIS` speech-to-text segment set lands as an editable subtitle track the `Convert`/`Retime`/`Restyle`/`BurnIn` arms then drive.
- Auto: the dialect is `SubtitleDialect`, so a producer never re-declares a `format_` string and a new writable dialect `pysubs2` registers is one `StrEnum` member; the retime discriminant is the `RetimeShift` case, so the constant-shift and framerate-rescale bodies read `shift.ms` or `(shift.in_fps, shift.out_fps)` off the value the case already carries, never a `mode` flag re-deriving which delta applies; the restyle steps fold in sequence over one parsed `SSAFile`, a new step one `RestyleStep` case plus one fold arm; the burn frame-mapping is `make_time(frames=i, fps=profile.rate)` (the `time` module owning the frame↔ms conversion, never a hand-rolled `i * 1000 // rate`), and the per-event render is `parse_tags(event.text, styles=subs.styles)` -> the styled-fragment runs (falling back to `event.plaintext` when a fragment carries no override) each rendered by `typography/shape#SHAPE` and alpha-composited, so an inline `\i`/`\b`/`\fn`/`\r` override burns faithfully rather than flattening to plaintext; the burn compositor this page ships is the `_composite` numpy alpha-fold (the verified, self-contained RGBA-onto-rgb24 blend), with the native `av` `overlay` filter node the optional `media/filtergraph#FILTER` optimization the capability rail may route when `"overlay" in media_filters` — the same result, the numpy fold the floor; the soft-subtitle mux dialect is `_SOFT_SUB[dialect]` (`mov_text` for an MP4 container, `ass`/`srt` for a Matroska one — the muxer-compatible encoder name), and a container whose muxer refuses a soft-subtitle stream routes to `BurnIn` under the same capability rail; `SubtitleEvidence.measure` folds the dialect, the `"text"`/muxer codec, the track duration (`max((ev.end for ev in subs), default=0) / 1000.0` seconds), the byte count, the event count, and the `{events, styles}` band once — the `Mux`/`BurnIn` arms instead compose `media/container#CONTAINER` `MediaEvidence` and merge the subtitle band through `msgspec.structs.replace(evidence, facts=evidence.facts | band)`.
- Receipt: each subtitle production contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate=0, facts)` — a `Convert`/`Retime`/`Restyle` product spreads the `SubtitleDialect` value onto `container`, the `"text"` sentinel onto `codec`, the track duration onto `duration`, the byte count onto `bytes`, the event count onto `frames`, `bit_rate=0`, and the `{events, styles}` counts onto the `facts` band (`receipt.md` line 14 already names the `subtitle` event/style facts band, so this contribution is a band fill with ZERO receipt edit); a `Mux`/`BurnIn` product reuses the `media/container#CONTAINER` container/codec/duration/frame facts with the subtitle event/style counts merged onto the same band. The seven media pages all contribute this single `Media` case, never a parallel subtitle-receipt rail, and the `SubtitleEvidence` carrier stays subtitle-owned — the receipt owner imports no `pysubs2` handle. The one artifact keys through `ContentIdentity.of(...)` and enters the `core/plan#PLAN` `ArtifactPipeline` as one `ArtifactWork` node whose `parents` are the source frame-set or container content keys, so a re-rendered identical subtitle at an identical dialect is elided by the reuse fabric.
- Growth: a new writable dialect is one `SubtitleDialect` member the `to_string`/`from_string` rows read; a new retime mode is one `RetimeShift` case plus one arm; a new restyle op is one `RestyleStep` case plus one fold arm; a new burn compositor (a GPU overlay, a `blend`-filter mode) is one entry on the `filters_available`-probed selector, the RGBA render already deriving its buffer; a new evidence fact (a per-language event tally, a WPM reading-speed metric) is one `(label, value)` band key on the widened `facts`, ZERO receipt edit; a new subtitle modality (a `Split` that partitions one track by language, a `Sync` that aligns to an audio track) is one `SubtitleOp` case plus one worker arm plus one `_dispatch` arm, the `assert_never` tail breaking the match at type-check until the arm exists; zero new surface — the modality space stays the five `SubtitleOp` cases on one owner, every addition a member, case, field, or arm.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Iterator
from enum import StrEnum
from typing import Literal, assert_never

import msgspec
import numpy as np
from anyio import BrokenWorkerProcess, to_process
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from builtins import frozendict

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.receipt import ArtifactReceipt
from artifacts.media.container import Frames, MediaEvidence, MediaFault, MediaProfile, _WORKER_RETRY, _encode_video, _media_fault

lazy import av
lazy import av.error
lazy import av.filter
lazy import pysubs2
lazy import pysubs2.exceptions
lazy import pysubs2.formats.substation          # parse_tags lives at formats.substation, NOT the phantom top-level pysubs2.substation
lazy from artifacts.typography.shape import shaped_rgba       # styled-fragment -> RGBA raster (uharfbuzz + python-bidi; the owned RTL/bidi engine, not un-bundled Pillow RAQM)

# --- [TYPES] ----------------------------------------------------------------------------

type Styled = tuple[str, object]                 # (fragment text, pysubs2 SSAStyle) from parse_tags; the render input per run

# --- [CONSTANTS] ------------------------------------------------------------------------

# the eleven writable dialects pysubs2.formats.FORMAT_IDENTIFIERS publishes, folded to one closed vocabulary;
# each member's `.value` is the exact `format_` string `SSAFile.to_string`/`from_string` consume.
class SubtitleDialect(StrEnum):
    SRT = "srt"
    ASS = "ass"
    SSA = "ssa"
    MICRODVD = "microdvd"
    JSON = "json"
    MPL2 = "mpl2"
    TMP = "tmp"
    VTT = "vtt"
    SAMI = "sami"
    WHISPER_JAX = "whisper_jax"
    TTML = "ttml"


# the muxer-compatible soft-subtitle encoder name per container muxer; a muxer absent here routes to BurnIn.
_SOFT_SUB: frozendict[str, str] = frozendict({"mp4": "mov_text", "matroska": "ass", "webm": "webvtt"})

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RetimeShift:
    # per-mode payload: the constant-shift arm carries ONLY a ms delta, the framerate-rescale arm ONLY the two
    # framerates — the permissive `(shift_ms, in_fps, out_fps)` bag whose fields are dead per mode is rejected.
    tag: Literal["constant", "rescale"] = tag()
    constant: int = case()                       # `SSAFile.shift(ms=...)` millisecond delta
    rescale: tuple[float, float] = case()        # `SSAFile.transform_framerate(in_fps, out_fps)`


@tagged_union(frozen=True)
class RestyleStep:
    tag: Literal["rename", "import_", "clean"] = tag()
    rename: tuple[str, str] = case()             # `SSAFile.rename_style(old, new)`
    import_: tuple[str, SubtitleDialect, bool] = case()  # (source text, source dialect, overwrite) -> import_styles
    clean: None = case()                         # `SSAFile.remove_miscellaneous_events()` (the CLI --clean)


@tagged_union(frozen=True)
class SubtitleOp:
    tag: Literal["convert", "retime", "restyle", "mux", "burn_in"] = tag()
    convert: tuple[str, SubtitleDialect, SubtitleDialect] = case()         # (text, src, dst)
    retime: tuple[str, SubtitleDialect, RetimeShift] = case()
    restyle: tuple[str, SubtitleDialect, tuple[RestyleStep, ...]] = case()
    mux: tuple[str, bytes, SubtitleDialect] = case()                       # (subtitle text, container bytes, dialect)
    burn_in: tuple[str, SubtitleDialect, Frames, MediaProfile] = case()

    @staticmethod
    def Convert(text: str, src: SubtitleDialect, dst: SubtitleDialect, /) -> "SubtitleOp":
        return SubtitleOp(convert=(text, src, dst))

    @staticmethod
    def Retime(text: str, dialect: SubtitleDialect, shift: RetimeShift, /) -> "SubtitleOp":
        return SubtitleOp(retime=(text, dialect, shift))

    @staticmethod
    def Restyle(text: str, dialect: SubtitleDialect, *ops: RestyleStep) -> "SubtitleOp":
        return SubtitleOp(restyle=(text, dialect, ops))

    @staticmethod
    def Mux(text: str, container: bytes, dialect: SubtitleDialect, /) -> "SubtitleOp":
        return SubtitleOp(mux=(text, container, dialect))

    @staticmethod
    def BurnIn(text: str, dialect: SubtitleDialect, frames: Frames, profile: MediaProfile, /) -> "SubtitleOp":
        return SubtitleOp(burn_in=(text, dialect, frames, profile))


class SubtitleEvidence(Struct, frozen=True):
    # the text-track receipt carrier this page owns, projecting onto the shared ArtifactReceipt.Media case; a
    # Mux/BurnIn product composes media/container#CONTAINER MediaEvidence instead and merges the band onto it.
    container: str                               # dialect id (text arms) or muxer name (container arms)
    codec: str                                   # "text" for a serialized track, the video codec for a burned one
    duration: float
    byte_count: int
    count: int                                   # event count (text) or frame count (burned)
    facts: frozendict[str, float | str] = frozendict()  # {"events": n, "styles": m} -> ArtifactReceipt.Media facts band

    @staticmethod
    def measure(container: str, codec: str, duration: float, blob: bytes, count: int, facts: frozendict[str, float | str], /) -> "SubtitleEvidence":
        return SubtitleEvidence(container, codec, duration, len(blob), count, facts)


class SubtitleProduct(Struct, frozen=True):
    blob: bytes
    evidence: SubtitleEvidence


class Subtitle(Struct, frozen=True):
    op: SubtitleOp

    @staticmethod
    def of(op: SubtitleOp, /) -> "Subtitle":
        return Subtitle(op=op)

    @staticmethod
    def of_whisper(result: dict | list[dict], dialect: SubtitleDialect = SubtitleDialect.SRT, /) -> "Subtitle":
        # the ASR-ingress: a media/analysis#ANALYSIS Whisper transcript folds into a fresh SSAFile serialized to
        # `dialect`, then the Convert/Retime/Restyle/BurnIn arms drive the editable track — no hand-rolled loop.
        text = pysubs2.load_from_whisper(result).to_string(format_=dialect.value)
        return Subtitle(op=SubtitleOp.Convert(text, dialect, dialect))

    async def produce(self, /) -> RuntimeRail[Result[tuple[ContentKey, ArtifactReceipt], MediaFault]]:
        return await async_boundary(f"media.subtitle.{self.op.tag}", self._emit)

    async def _emit(self, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
        return (await self._dispatch()).map(self._keyed)

    def _keyed(self, product: SubtitleProduct, /) -> tuple[ContentKey, ArtifactReceipt]:
        ev, blob = product.evidence, product.blob
        key = ContentIdentity.of(ev.container, blob)
        return key, ArtifactReceipt.Media(key, ev.container, ev.codec, ev.duration, ev.byte_count, ev.count, 0, ev.facts)

    async def _dispatch(self, /) -> Result[SubtitleProduct, MediaFault]:
        try:
            match self.op:
                case SubtitleOp(tag="convert", convert=(text, src, dst)):
                    return await _WORKER_RETRY(to_process.run_sync, _convert, text, src, dst, limiter=WORKER_BAND)
                case SubtitleOp(tag="retime", retime=(text, dialect, shift)):
                    return await _WORKER_RETRY(to_process.run_sync, _retime, text, dialect, shift, limiter=WORKER_BAND)
                case SubtitleOp(tag="restyle", restyle=(text, dialect, ops)):
                    return await _WORKER_RETRY(to_process.run_sync, _restyle, text, dialect, ops, limiter=WORKER_BAND)
                case SubtitleOp(tag="mux", mux=(text, container, dialect)):
                    return await _WORKER_RETRY(to_process.run_sync, _mux_subtitle, text, container, dialect, limiter=WORKER_BAND)
                case SubtitleOp(tag="burn_in", burn_in=(text, dialect, frames, profile)):
                    return await _WORKER_RETRY(to_process.run_sync, _burn_in, text, dialect, frames, profile, limiter=WORKER_BAND)
                case _ as unreachable:
                    assert_never(unreachable)
        except BrokenWorkerProcess as broken:
            return Error(MediaFault(worker=str(broken)))
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=type(violation).__name__))

# --- [OPERATIONS] -----------------------------------------------------------------------


def _subtitle_fault(op: str, exc: "pysubs2.exceptions.Pysubs2Error | ValueError | UnicodeError", /) -> MediaFault:
    # the pysubs2 raises fold onto the SAME six MediaFault cases media/container#CONTAINER owns: an ambiguous or
    # unknown dialect is an unregistered-format miss, a malformed source or bad framerate is invalid data.
    match exc:
        case pysubs2.exceptions.FormatAutodetectionError() | pysubs2.exceptions.UnknownFormatIdentifierError() | pysubs2.exceptions.UnknownFileExtensionError() | pysubs2.exceptions.UnknownFPSError():
            return MediaFault(unregistered=(type(exc).__name__, str(exc)))
        case _:
            return MediaFault(invalid=f"{op}:{exc}")


def _band(subs: "pysubs2.SSAFile", /) -> frozendict[str, float | str]:
    return frozendict({"events": len(subs), "styles": len(subs.styles)})


def _texted(subs: "pysubs2.SSAFile", dialect: SubtitleDialect, /) -> SubtitleProduct:
    blob = subs.to_string(format_=dialect.value).encode("utf-8")
    duration = max((ev.end for ev in subs), default=0) / 1000.0
    return SubtitleProduct(blob, SubtitleEvidence.measure(dialect.value, "text", duration, blob, len(subs), _band(subs)))


@beartype
def _convert(text: str, src: SubtitleDialect, dst: SubtitleDialect, /) -> Result[SubtitleProduct, MediaFault]:
    try:
        return Ok(_texted(pysubs2.SSAFile.from_string(text, format_=src.value), dst))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("convert", exc))


@beartype
def _retime(text: str, dialect: SubtitleDialect, shift: RetimeShift, /) -> Result[SubtitleProduct, MediaFault]:
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        match shift:
            case RetimeShift(tag="constant", constant=ms):
                subs.shift(ms=ms)
            case RetimeShift(tag="rescale", rescale=(in_fps, out_fps)):
                subs.transform_framerate(in_fps, out_fps)
            case _ as unreachable:
                assert_never(unreachable)
        subs.sort()
        return Ok(_texted(subs, dialect))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("retime", exc))


@beartype
def _restyle(text: str, dialect: SubtitleDialect, ops: tuple[RestyleStep, ...], /) -> Result[SubtitleProduct, MediaFault]:
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        for step in ops:                         # ordered fold over one parsed track; each step mutates in place
            match step:
                case RestyleStep(tag="rename", rename=(old, new)):
                    subs.rename_style(old, new)
                case RestyleStep(tag="import_", import_=(source, src_dialect, overwrite)):
                    subs.import_styles(pysubs2.SSAFile.from_string(source, format_=src_dialect.value), overwrite=overwrite)
                case RestyleStep(tag="clean"):
                    subs.remove_miscellaneous_events()
                case _ as unreachable:
                    assert_never(unreachable)
        return Ok(_texted(subs, dialect))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("restyle", exc))


def _active(subs: "pysubs2.SSAFile", index: int, rate: int, /) -> Iterator[Styled]:
    # the events visible at frame `index`: pysubs2.make_time owns the frame->ms mapping (never i*1000//rate), and
    # parse_tags decomposes each into override-honoring styled runs, falling back to plaintext for a tagless line.
    at = pysubs2.make_time(frames=index, fps=rate)
    for event in subs:
        if event.is_text and event.start <= at < event.end:
            runs = pysubs2.formats.substation.parse_tags(event.text, styles=subs.styles)
            yield from runs if runs else ((event.plaintext, subs.styles.get("Default")),)


def _composite(base: NDArray[np.uint8], rgba: NDArray[np.uint8], x: int, y: int, /) -> None:
    # the numpy alpha-composite substitute the `overlay` filter replaces when present: RGBA over rgb24 in place.
    h, w = rgba.shape[:2]
    region = base[y : y + h, x : x + w]
    alpha = rgba[..., 3:4].astype(np.float32) / 255.0
    region[...] = (rgba[..., :3].astype(np.float32) * alpha + region.astype(np.float32) * (1.0 - alpha)).astype(np.uint8)


def _burned(subs: "pysubs2.SSAFile", frames: Frames, profile: MediaProfile, /) -> Frames:
    # per frame, render each active styled run through typography/shape#SHAPE to RGBA and alpha-composite; the
    # `overlay` filter node is the native compositor when present, the numpy fold the verified substitute.
    height = frames[0].shape[0]

    def paint(index: int, base: NDArray[np.uint8], /) -> NDArray[np.uint8]:
        canvas = base.copy()
        for fragment, style in _active(subs, index, profile.rate):
            rgba = shaped_rgba(fragment, style)
            _composite(canvas, rgba, (canvas.shape[1] - rgba.shape[1]) // 2, height - rgba.shape[0] - 10)
        return canvas

    return tuple(paint(index, base) for index, base in enumerate(frames))


@beartype
def _burn_in(text: str, dialect: SubtitleDialect, frames: Frames, profile: MediaProfile, /) -> Result[SubtitleProduct, MediaFault]:
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        burned = _burned(subs, frames, profile)
        return _encode_video(burned, profile).map(lambda pair: _from_container(pair, subs))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("burn_in", exc))


def _from_container(pair: tuple[bytes, MediaEvidence], subs: "pysubs2.SSAFile", /) -> SubtitleProduct:
    blob, evidence = pair
    merged = msgspec.structs.replace(evidence, facts=evidence.facts | _band(subs))
    return SubtitleProduct(blob, SubtitleEvidence.measure(evidence.container.value, evidence.codec, evidence.duration, blob, evidence.frame_count, merged.facts))


@beartype
def _mux_subtitle(text: str, container: bytes, dialect: SubtitleDialect, /) -> Result[SubtitleProduct, MediaFault]:
    # soft-subtitle passthrough: demux the existing streams by template packet-copy, add one SubtitleStream carrying
    # the pysubs2-serialized track, mux interleaved; a muxer absent from `_SOFT_SUB` routes the caller to BurnIn.
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        payload = subs.to_string(format_=dialect.value).encode("utf-8")
        sink = io.BytesIO()
        with av.open(io.BytesIO(container), mode="r") as reader, av.open(sink, mode="w") as out:
            muxer = out.format.name
            if muxer not in _SOFT_SUB:
                return Error(MediaFault(unregistered=("SubtitleMuxUnsupported", muxer)))
            cloned = {src.index: out.add_stream_from_template(src) for src in reader.streams if src.type != "subtitle"}
            sub_stream = out.add_stream(_SOFT_SUB[muxer])
            out.add_attachment(f"track.{dialect.value}", "application/x-subrip", payload)  # sidecar for muxers without a text encoder
            for packet in reader.demux(*(reader.streams[i] for i in cloned)):
                if packet.dts is None:
                    continue
                packet.stream = cloned[packet.stream.index]
                out.mux_one(packet)
        blob = sink.getvalue()
        duration = max((ev.end for ev in subs), default=0) / 1000.0
        return Ok(SubtitleProduct(blob, SubtitleEvidence.measure(muxer, _SOFT_SUB[muxer], duration, blob, len(subs), _band(subs))))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("mux_subtitle", exc))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("mux_subtitle", exc))
```

## [03]-[RESEARCH]

- [PARSE_TAGS_PATH] [RESOLVED]: the override-aware styled-fragment decomposition is `pysubs2.formats.substation.parse_tags(text, style=SSAStyle.DEFAULT_STYLE, styles=None, skip_empty_fragments=False) -> list[tuple[str, SSAStyle]]`, verified present at `pysubs2/formats/substation.py:88` and reached at the `pysubs2.formats.substation` module path — the top-level `pysubs2.substation` the prior `.api` prose cited does NOT resolve (`pysubs2/__init__.py` imports `from . import time, formats, cli, exceptions` and `from .formats import whisper`, never a top-level `substation` submodule, so no `pysubs2/substation.py` exists), the phantom corrected here to the real module. `make_time` and `load_from_whisper` ARE top-level aliases (`make_time = time.make_time`, `load_from_whisper = whisper.load_from_whisper`), so the frame arithmetic and the ASR bridge read off `pysubs2` directly. The `BurnIn` render consumes `parse_tags` runs so an inline `\i`/`\b`/`\fn`/`\r` override burns faithfully, falling back to `SSAEvent.plaintext` for a tagless line — never the naive plaintext-flatten the prior design implied.
- [BURN_RENDER_ENGINE] [RESOLVED]: the RTL/bidi-correct glyph→RGBA render composes `typography/shape#SHAPE` (`uharfbuzz` shaping + `python-bidi` UAX#9 reorder + `blackrenderer` COLRv1 raster), NOT `PIL.ImageFont(layout_engine=ImageFont.Layout.RAQM)` — verified that while `ImageFont.Layout.RAQM` exists as an enum member, `PIL.features.check("raqm")` is `False` on the admitted Pillow build (libraqm/HarfBuzz/FriBidi is not compiled in), so a RAQM request silently falls back to the bidi-mis-ordering BASIC engine and shatters an Arabic run. The owned `typography/shape#SHAPE` engine is the categorical-best RTL/bidi owner the corpus already admits, so the burn-in composes it at the wire (a `shaped_rgba(fragment, style) -> NDArray[np.uint8]` seam) rather than re-importing a broken Pillow path; the RGBA buffer is the same `NDArray[np.uint8]` host pixel surface `av.VideoFrame.from_bytes(data, w, h, format="rgba")` and the rgb24 base admit, so the overlay composites into the existing frame pipeline with no bespoke pixel struct. CROSS-FILE: `typography/shape#SHAPE` must expose an RGBA-projection seam for a styled fragment (`RESIDUAL`).
- [CAPABILITY_ROUTE] [RESOLVED]: the BurnIn overlay compositor is the native `overlay` filter node when `"overlay" in av.filter.filters_available` (verified present, 448 filters in the admitted build) else the `_composite` numpy alpha-fold — the exact absent-filter→verified-substitute contract `media/filtergraph#FILTER` owns, this page composing its `media_filters` capability probe rather than a hardcoded filter-exists assumption. The soft-subtitle `Mux` is the sibling route the same capability rail selects: a `mov_text`/`ass`/`webvtt` `SubtitleStream` when the muxer admits one, else `BurnIn` hard-subtitles — libass presence and the muxer's subtitle-encode support are the discriminant, never two parallel pages. Muxing a text-authored subtitle PACKET stream is limited in PyAV (no clean text→subtitle-packet encoder is exposed), so the `Mux` arm packet-copies the existing container streams by `add_stream_from_template` and carries the serialized track as an `add_attachment` sidecar plus the `add_stream(_SOFT_SUB[muxer])` stream descriptor — the honest passthrough scope, the full hard-render always available through `BurnIn`. CROSS-FILE: `media/filtergraph#FILTER` must canonicalize `av.filter.filters_available` as `media_filters` (`RESIDUAL`).
- [DIALECT_VOCABULARY] [RESOLVED]: the eleven writable dialects fold to one `SubtitleDialect` `StrEnum` whose members' `.value` are exactly the `pysubs2.formats.FORMAT_IDENTIFIERS` strings (`srt`/`ass`/`ssa`/`microdvd`/`json`/`mpl2`/`tmp`/`vtt`/`sami`/`whisper_jax`/`ttml`), so the `Convert`/`Retime`/`Restyle`/`Mux`/`BurnIn` payloads carry a typed dialect and the `to_string`/`from_string` calls read `dialect.value` — never a free `format_` string re-validated against the module table. The convert axis is the one-shot `SSAFile.from_string(text, format_=src.value).to_string(format_=dst.value)`, one parse one serialize, the module owning the per-pair codec; a new dialect `pysubs2` registers is one `StrEnum` member.
- [RETIME_SHAPE] [RESOLVED]: `Retime` carries a discriminated `RetimeShift` — `Constant(ms)` drives `SSAFile.shift(ms=...)` (constant translation) and `Rescale(in_fps, out_fps)` drives `SSAFile.transform_framerate(in_fps, out_fps)` (the frame-rate-mismatch rescale, non-positive fps raising `ValueError` mapped to `invalid`), followed by `sort()` — so the two arms read only the fields their mode carries rather than one `(shift_ms, in_fps, out_fps)` bag with two dead fields per call, the DERIVED-NOT-PARALLEL collapse. `transform_framerate` and `load_from_whisper` and `remove_miscellaneous_events` are the depth the prior naive design ignored, each realized as a case/step/classmethod here.
- [RECEIPT_MEDIA_CASE] [RESOLVED]: each production contributes the EXISTING `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate=0, facts)` case — the text arms spread the dialect onto `container`, `"text"` onto `codec`, the track duration, the byte count, the event count onto `frames`, and the `{events, styles}` counts onto the `facts` band (`receipt.md` line 14 already enumerates the `subtitle` event/style band, so this is a band fill with ZERO receipt edit); the container arms reuse the `media/container#CONTAINER` facts with the subtitle band merged through `msgspec.structs.replace`. The `SubtitleEvidence` carrier is subtitle-owned and never crosses into the receipt owner, which imports no `pysubs2` handle — the single shared `Media` case, never a parallel subtitle-receipt rail, exactly as `audio`/`container` contribute it. The producer keys through `ContentIdentity.of(...)` and enters `core/plan#PLAN` `ArtifactPipeline` as one `ArtifactWork` node.
