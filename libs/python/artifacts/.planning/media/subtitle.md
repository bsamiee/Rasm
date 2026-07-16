# [PY_ARTIFACTS_MEDIA_SUBTITLE]

`Subtitle` owns the media plane's timed-text arm — the owner over a closed `SubtitleOp` union that holds the subtitle document (parse, convert, retime, restyle over `pysubs2`) and the two routes a track reaches a viewer. `Convert`/`Retime`/`Restyle` are the in-process `pysubs2` edits (parse once, apply, re-serialize); `Mux` passes a soft-subtitle stream through into an `av` container by packet copy; `BurnIn` composites hard subtitles as per-event RGBA overlays onto an rgb24 frame sequence, the substitute the capability rail selects when the linked FFmpeg lacks the libass `subtitles`/`ass` filter. Only serialized subtitle bytes and projected `plaintext`/styled-fragment runs cross the boundary — the raw `SSAFile`/`SSAEvent` handles never do.

`SubtitleDialect` folds the writable dialects `pysubs2.formats.FORMAT_IDENTIFIERS` publishes into one vocabulary keyed inside each op's typed payload; `RetimeShift` and `RestyleStep` are per-mode unions so a body reads only the fields its mode carries. This page composes `media/container#CONTAINER`'s shared `MediaProfile`/`MediaFault`/`MediaEvidence` family and its `_encode_video`/`_media_fault`/`_WORKER_RETRY` primitives, `media/filtergraph#FILTER`'s `filters_available` probe routing Mux-versus-BurnIn and native-overlay-versus-numpy-composite, `typography/shape#SHAPE` for the RTL/bidi glyph→RGBA render, and `media/analysis#ANALYSIS` through the `of_whisper` ASR ingress; it owns the `SubtitleOp`/`RetimeShift`/`RestyleStep`/`SubtitleDialect` vocabulary, the `SubtitleEvidence` carrier, and the five worker arms. Each production contributes the shared `core/receipt#RECEIPT` `ArtifactReceipt.Media` case — the subtitle event/style counts on its `facts` band — and enters the `core/plan#PLAN` `ArtifactPipeline` as one `ArtifactWork` node keyed by content key.

## [01]-[INDEX]

- [01]-[SUBTITLE]: the `Subtitle` owner over the closed `SubtitleOp` family — the `Convert`/`Retime`/`Restyle` `pysubs2` text edits, the `Mux` soft-subtitle passthrough, and the `BurnIn` hard-subtitle overlay — folding into the shared `ArtifactReceipt.Media` case.

## [02]-[SUBTITLE]

- Owner: `Subtitle` discriminates modality over the closed `SubtitleOp` union, each case carrying its own typed payload — never a shared erased `params` bag, a per-modality subclass, or a parallel `convert`/`retime`/`burn` trio. `SubtitleDialect`'s each `.value` is the exact `format_` string `SSAFile.to_string`/`from_string` consume, never a free string the implementer re-validates. `RetimeShift` (`Constant`/`Rescale`) and `RestyleStep` (`Rename`/`Import`/`Clean`) are per-mode unions so the constant-shift arm carries only a millisecond delta and the rescale arm only the two framerates — the permissive `(shift_ms, in_fps, out_fps)` bag whose fields go dead per mode is the rejected derived-not-parallel form.
- Cases: the `Mux`-versus-`BurnIn` choice is capability-derived, not a knob — `filters_available` and muxer support select the soft passthrough, else route to `BurnIn`, the hard-subtitle substitute when libass is absent.
- Entry: `Subtitle.of_whisper` is the ASR ingress — a `media/analysis#ANALYSIS` Whisper transcript folds through `load_from_whisper` into a fresh track the edit arms then drive. Each worker maps its own `pysubs2` raises through `_subtitle_fault` onto `media/container#CONTAINER`'s `MediaFault` cases (an unknown or ambiguous dialect to `unregistered`, malformed source or bad framerate to `invalid`), its `av` raises through the shared `_media_fault`, and an `ImportError` to `provision` — the subtitle causes fold onto the same rail, never a parallel `SubtitleFault`.
- Auto: `parse_tags` decomposes each event into override-honoring styled runs (plaintext fallback for a tagless line), so an inline `\i`/`\b`/`\fn`/`\r` burns faithfully rather than flattening; `make_time` owns the frame→ms mapping; `_composite`'s numpy alpha-fold is the burn floor, the native `overlay` filter the optional `media/filtergraph#FILTER` optimization the capability rail routes when present. `_SOFT_SUB` maps each muxer to its compatible soft-subtitle encoder; a muxer absent routes to `BurnIn`.
- Receipt: the text arms produce `SubtitleEvidence` directly; `Mux`/`BurnIn` compose `media/container#CONTAINER` `MediaEvidence` and merge the `{events, styles}` band through `msgspec.structs.replace` — a band fill with ZERO receipt edit. `SubtitleEvidence` stays subtitle-owned so the receipt owner imports no `pysubs2` handle, and the artifact keys through `ContentIdentity.of(...)` over the produced bytes so an identical track at an identical dialect is a cache hit by reference.
- Packages: `pysubs2` owns the timed-text document — the per-dialect parsers, the SubStation override grammar, the ms/frame codec, format autodetection, the shift/framerate retiming, and the style rename/import — so the owner wraps its ingest/egress and track edits, never re-implementing them; `av` owns the mux capsule. Both settled against the folder `.api`.
- Growth: a new writable dialect is one `SubtitleDialect` member; a new retime mode one `RetimeShift` case plus arm; a new restyle op one `RestyleStep` case plus fold arm; a new burn compositor one entry on the `filters_available` selector; a new evidence fact one band key (ZERO receipt edit); a new modality one `SubtitleOp` case plus worker plus dispatch arm, `assert_never` breaking the match until the arm exists.

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


from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.media.container import Frames, MediaEvidence, MediaFault, MediaProfile, _WORKER_RETRY, _encode_video, _media_fault

lazy import av
lazy import av.error
lazy import av.filter
lazy import pysubs2
lazy import pysubs2.exceptions
lazy import pysubs2.formats.substation  # parse_tags lives at formats.substation, not the phantom top-level pysubs2.substation
lazy from artifacts.typography.shape import (
    shaped_rgba,
)  # styled-fragment -> RGBA raster (uharfbuzz + python-bidi, not the un-bundled Pillow RAQM backend)

# --- [TYPES] ----------------------------------------------------------------------------

type Styled = tuple[str, object]  # (fragment text, SSAStyle) from parse_tags

# --- [CONSTANTS] ------------------------------------------------------------------------


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


# muxer -> compatible soft-subtitle encoder; a muxer absent here routes to BurnIn.
_SOFT_SUB: frozendict[str, str] = frozendict({"mp4": "mov_text", "matroska": "ass", "webm": "webvtt"})

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RetimeShift:
    tag: Literal["constant", "rescale"] = tag()
    constant: int = case()  # `SSAFile.shift(ms=...)` millisecond delta
    rescale: tuple[float, float] = case()  # `SSAFile.transform_framerate(in_fps, out_fps)`


@tagged_union(frozen=True)
class RestyleStep:
    tag: Literal["rename", "import_", "clean"] = tag()
    rename: tuple[str, str] = case()  # `SSAFile.rename_style(old, new)`
    import_: tuple[str, SubtitleDialect, bool] = case()  # (source text, source dialect, overwrite) -> import_styles
    clean: None = case()  # `SSAFile.remove_miscellaneous_events()`


@tagged_union(frozen=True)
class SubtitleOp:
    tag: Literal["convert", "retime", "restyle", "mux", "burn_in"] = tag()
    convert: tuple[str, SubtitleDialect, SubtitleDialect] = case()  # (text, src, dst)
    retime: tuple[str, SubtitleDialect, RetimeShift] = case()
    restyle: tuple[str, SubtitleDialect, tuple[RestyleStep, ...]] = case()
    mux: tuple[str, bytes, SubtitleDialect] = case()  # (subtitle text, container bytes, dialect)
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
    container: str  # dialect id (text arms) or muxer name (container arms)
    codec: str  # "text" for a serialized track, the video codec for a burned one
    duration: float
    byte_count: int
    count: int  # event count (text) or frame count (burned)
    facts: frozendict[str, float | str] = frozendict()  # {events, styles} -> ArtifactReceipt.Media band

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
        # ASR ingress: a Whisper transcript folds into a fresh SSAFile serialized to `dialect`.
        text = pysubs2.load_from_whisper(result).to_string(format_=dialect.value)
        return Subtitle(op=SubtitleOp.Convert(text, dialect, dialect))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key over the INPUT op minted pre-run; the output's own address rides the receipt facts.
        return ContentIdentity.of(f"media.subtitle-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the member MediaFault folds into the boundary fault (Work[ArtifactReceipt] forbids an inner Result).
        railed = await async_boundary(f"media.subtitle.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map(lambda pair: pair[1]).map_error(
                lambda fault: BoundaryFault(boundary=(f"media.subtitle.{self.op.tag}", f"{fault.tag}:{fault}"))
            )
        )

    async def _folded(self, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
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
    # unknown/ambiguous dialect -> unregistered; malformed source or bad framerate -> invalid.
    match exc:
        case (
            pysubs2.exceptions.FormatAutodetectionError()
            | pysubs2.exceptions.UnknownFormatIdentifierError()
            | pysubs2.exceptions.UnknownFileExtensionError()
            | pysubs2.exceptions.UnknownFPSError()
        ):
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
        for step in ops:  # ordered fold over one parsed track; each step mutates in place
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
    # make_time owns frame->ms (never i*1000//rate); parse_tags -> styled runs, plaintext fallback for a tagless line.
    at = pysubs2.make_time(frames=index, fps=rate)
    for event in subs:
        if event.is_text and event.start <= at < event.end:
            runs = pysubs2.formats.substation.parse_tags(event.text, styles=subs.styles)
            yield from runs if runs else ((event.plaintext, subs.styles.get("Default")),)


def _composite(base: NDArray[np.uint8], rgba: NDArray[np.uint8], x: int, y: int, /) -> None:
    # numpy alpha-composite substitute for the native `overlay` filter: RGBA over rgb24 in place.
    h, w = rgba.shape[:2]
    region = base[y : y + h, x : x + w]
    alpha = rgba[..., 3:4].astype(np.float32) / 255.0
    region[...] = (rgba[..., :3].astype(np.float32) * alpha + region.astype(np.float32) * (1.0 - alpha)).astype(np.uint8)


def _burned(subs: "pysubs2.SSAFile", frames: Frames, profile: MediaProfile, /) -> Frames:
    # per frame: render each active styled run to RGBA, alpha-composite; numpy fold substitutes the native overlay.
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
    return SubtitleProduct(
        blob, SubtitleEvidence.measure(evidence.container.value, evidence.codec, evidence.duration, blob, evidence.frame_count, merged.facts)
    )


@beartype
def _mux_subtitle(text: str, container: bytes, dialect: SubtitleDialect, /) -> Result[SubtitleProduct, MediaFault]:
    # soft-subtitle passthrough by template packet-copy; a muxer absent from `_SOFT_SUB` routes the caller to BurnIn.
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
