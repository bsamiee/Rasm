# [PY_ARTIFACTS_MEDIA_SUBTITLE]

`Subtitle` owns the media plane's timed-text arm — the owner over a closed `SubtitleOp` union that holds the subtitle document and the two routes a track reaches a viewer. `Whisper` admits the ASR segment shape through `pysubs2.load_from_whisper`; `Convert`/`Retime`/`Restyle` parse once, edit, and re-serialize; `Mux` lazily interleaves each timed `av.Packet` with the cloned A/V packet stream, carrying the SubStation header in `codec_context.extradata`; `BurnIn` composites styled RGBA lines onto rgb24 frames because the bundled build ships no libass filter. Only typed Whisper payloads, serialized subtitle bytes, and projected styled runs cross the boundary — raw `SSAFile`/`SSAEvent` handles never do.

`SubtitleDialect` folds the writable dialects `pysubs2.formats.FORMAT_IDENTIFIERS` publishes into one vocabulary keyed inside each op's typed payload; `RetimeShift` and `RestyleStep` are per-mode unions so a body reads only the fields its mode carries. `BurnIn` honors the document's layout law: `pysubs2.formats.substation.parse_tags` decomposes each event into styled runs, `BurnStyle.resolve` selects the family/bold/italic face, `_line` lays the run planes in sequence, and `_anchored` derives the origin from `SSAStyle.alignment` plus the ASS margins. `_plane` memoizes each distinct `TextSpec`, while `_composite` clips an oversized or off-frame plane before the NumPy alpha fold. This page composes `MediaProfile`/`MediaFault`/`MediaEvidence`, `_encode_video`, `_media_fault`, `_probe`, and the filtergraph text plane; raw `SSAFile`/`SSAEvent` handles never cross the worker boundary.

## [01]-[INDEX]

- [01]-[SUBTITLE]: the `Subtitle` owner over the closed `SubtitleOp` family — `Whisper` admission, `Convert`/`Retime`/`Restyle` document edits, `Mux` timed-packet passthrough, and `BurnIn` styled overlay — folding into the shared `ArtifactReceipt.Media` case.

## [02]-[SUBTITLE]

- Owner: `Subtitle` discriminates modality over the closed `SubtitleOp` union, each case carrying its own typed payload — never a shared erased `params` bag, a per-modality subclass, or a parallel `convert`/`retime`/`burn` trio. Each `SubtitleDialect.value` is the exact `format_` string `SSAFile.to_string`/`from_string` consume. `RetimeShift` and `RestyleStep` are per-mode unions, while `StyleConflict.KEEP`/`REPLACE` carries import behavior as a policy value instead of an `overwrite` boolean. `BurnStyle.faces` keys `(fontname, bold, italic)` to a file path and `fallback` closes an unmapped face.
- Cases: the `Mux`-versus-`BurnIn` choice derives from `_SOFT_SUB` membership — the muxers whose in-process packet path is verified writable — and a muxer outside the table rails `MediaFault.unregistered` so the caller routes `BurnIn`, the hard-subtitle substitute for every container the packet path cannot reach and for the absent libass filter.
- Entry: `Subtitle.of_whisper` stores the typed `WhisperPayload` in the `whisper` case; `_whisper` performs `pysubs2.load_from_whisper` inside the process worker, so malformed provider material rails through `_subtitle_fault`. Every worker maps runtime-contract violations through `_worker` to `MediaFault.contract`; `_crossed` maps the lane's outer `BoundaryFault` through `_lapsed` and flattens the worker `Result` after retry settles.
- Auto: `parse_tags` decomposes each event into override-honoring styled runs (plaintext fallback for a tagless line), so an inline `\i`/`\b`/`\fn`/`\r` burns faithfully rather than flattening; `make_time` owns the frame→ms mapping; `_anchored` maps the ASS numpad `alignment` (column `(a-1) % 3`, row `(a-1) // 3`) plus margins onto the paint origin so a top-left `an7` title and a bottom-center `an2` caption both land where the document says; `_plane` memoizes the rendered run RGBA on the frozen `TextSpec` so identical text re-rasterizes zero times across a frame span; `_composite`'s numpy alpha fold is the burn floor.
- Receipt: the text arms produce `SubtitleEvidence` directly; `Mux`/`BurnIn` compose `MediaEvidence` and merge the `{events, styles}` band. `SubtitleEvidence` keeps provider handles out of the receipt owner; `_keyed` threads the PRE-RUN node key as the receipt slot — the `core/receipt#RECEIPT` elision law, so an identical op at an identical dialect is a planner cache hit — and lands the produced-bytes content address as the `address` band fact. `_canon` frames the burn-in preimage (text, dialect, profile, sorted faces, raw frame bytes) per the identity framing law; every other case encodes whole through `_CANON`.
- Packages: `pysubs2` owns the timed-text document — the per-dialect parsers, the SubStation override grammar, the ms/frame codec, format autodetection, the shift/framerate retiming, and the style rename/import — so the owner wraps its ingest/egress and track edits, never re-implementing them; `av` owns the mux capsule and the raw-packet subtitle write. Both settled against the folder `.api`.
- Growth: a writable dialect is one `SubtitleDialect` member; a retime mode is one `RetimeShift` case; a restyle operation is one `RestyleStep` case; a packet-writable muxer is one `_SOFT_SUB` row; a face is one `BurnStyle.faces` row; an evidence fact is one band key; a modality is one `SubtitleOp` case plus one total dispatch arm.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable, Iterator
from enum import StrEnum
from fractions import Fraction
from functools import lru_cache, wraps
from heapq import merge
from math import isfinite
from typing import TYPE_CHECKING, Literal, ReadOnly, TypedDict, assert_never

import msgspec
import numpy as np
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.media.container import Frames, MediaEvidence, MediaFault, MediaProfile, _CANON, _framed, _lapsed

lazy import av
lazy import av.error
lazy import pysubs2
lazy import pysubs2.exceptions
lazy import pysubs2.formats.substation  # parse_tags lives at formats.substation (verified 1.8.1), not the retired pysubs2.substation
lazy from rasm.artifacts.media.container import _encode_video, _media_fault, _probe
lazy from rasm.artifacts.media.filtergraph import TextSpec, _render_text  # the RAQM-gated styled-text -> RGBA plane owner

if TYPE_CHECKING:
    from numpy.typing import NDArray

# --- [TYPES] ----------------------------------------------------------------------------

type Styled = tuple[str, "pysubs2.SSAStyle"]  # (fragment text, computed style) from parse_tags
type FaceKey = tuple[str, bool, bool]


class WhisperSegment(TypedDict, extra_items=ReadOnly[object]):
    start: float
    end: float
    text: str


class WhisperResult(TypedDict, extra_items=ReadOnly[object]):
    segments: list[WhisperSegment]


type WhisperPayload = WhisperResult | list[WhisperSegment]

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


class StyleConflict(StrEnum):
    KEEP = "keep"
    REPLACE = "replace"


# muxer -> subtitle codec whose raw-packet path the in-process muxer verifiably admits (matroska opens `ass` from the
# extradata header alone; mp4 `mov_text` and webm `webvtt` refuse avcodec_open2 on a template-less stream, so those
# containers route to BurnIn). A muxer absent here rails `unregistered` and the caller burns.
_SOFT_SUB: frozendict[str, str] = frozendict({"matroska": "ass"})

_ASS_EVENT_FORMAT = "[Events]\nFormat: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n"

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
    import_: tuple[str, SubtitleDialect, StyleConflict] = case()
    clean: None = case()  # `SSAFile.remove_miscellaneous_events()`


class BurnStyle(Struct, frozen=True):
    faces: frozendict[FaceKey, str]
    fallback: str

    def resolve(self, style: "pysubs2.SSAStyle", /) -> str:
        return self.faces.get((style.fontname, bool(style.bold), bool(style.italic)), self.fallback)


@tagged_union(frozen=True)
class SubtitleOp:
    tag: Literal["convert", "retime", "restyle", "mux", "burn_in", "whisper"] = tag()
    convert: tuple[str, SubtitleDialect, SubtitleDialect] = case()  # (text, src, dst)
    retime: tuple[str, SubtitleDialect, RetimeShift] = case()
    restyle: tuple[str, SubtitleDialect, tuple[RestyleStep, ...]] = case()
    mux: tuple[str, bytes, SubtitleDialect] = case()  # (subtitle text, container bytes, dialect)
    burn_in: tuple[str, SubtitleDialect, Frames, MediaProfile, BurnStyle] = case()
    whisper: tuple[WhisperPayload, SubtitleDialect] = case()

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
    def BurnIn(text: str, dialect: SubtitleDialect, frames: Frames, profile: MediaProfile, burn: BurnStyle, /) -> "SubtitleOp":
        return SubtitleOp(burn_in=(text, dialect, frames, profile, burn))


class SubtitleEvidence(Struct, frozen=True):
    container: str  # dialect id (text arms) or muxer name (container arms)
    codec: str  # "text" for a serialized track, the subtitle/video codec for a muxed or burned one
    duration: float
    byte_count: int
    count: int  # event count (text/mux) or frame count (burned)
    facts: frozendict[str, float | str] = frozendict()  # {events, styles} -> ArtifactReceipt.Media band

    @staticmethod
    def measure(container: str, codec: str, duration: float, blob: bytes, count: int, facts: frozendict[str, float | str], /) -> "SubtitleEvidence":
        return SubtitleEvidence(container, codec, duration, len(blob), count, facts)


type SubtitleProduct = tuple[bytes, SubtitleEvidence]


class Subtitle(Struct, frozen=True):
    op: SubtitleOp
    parents: tuple[ContentKey, ...] = ()  # the upstream container/frame producer keys the planner wires
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy

    @staticmethod
    def of(op: SubtitleOp, parents: tuple[ContentKey, ...] = (), /, *, lane: LanePolicy) -> "Subtitle":
        # lane selection rides the public factory — execution policy configurable without direct construction, identity untouched.
        return Subtitle(op=op, parents=parents, lane=lane)

    @staticmethod
    def of_whisper(result: WhisperPayload, dialect: SubtitleDialect = SubtitleDialect.SRT, /, *, lane: LanePolicy) -> "Subtitle":
        return Subtitle(op=SubtitleOp(whisper=(result, dialect)), lane=lane)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=self.parents, admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"media.subtitle-{self.op.tag}", _canon(self.op))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # member MediaFault folds into the boundary fault (Work[ArtifactReceipt] forbids an inner Result).
        railed = await async_boundary(f"media.subtitle.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map_error(lambda fault: BoundaryFault(boundary=(f"media.subtitle.{self.op.tag}", f"{fault.tag}:{fault}")))
        )

    async def _folded(self, /) -> Result[ArtifactReceipt, MediaFault]:
        return (await self._dispatch()).map(self._keyed)

    def _keyed(self, product: SubtitleProduct, /) -> ArtifactReceipt:
        # receipt.slot threads the PRE-RUN node key (the core/receipt elision law); the product address rides the band.
        blob, ev = product
        address = ContentIdentity.key(ev.container, blob)
        return ArtifactReceipt.Media(self._key, ev.container, ev.codec, ev.duration, ev.byte_count, ev.count, 0, ev.facts | {"address": address.hex})

    async def _crossed(self, worker: "Callable[..., Result[SubtitleProduct, MediaFault]]", /, *args: object) -> Result[SubtitleProduct, MediaFault]:
        outcome = await self.lane.offload(Kernel.of(worker, KernelTrait.HOSTILE), *args)
        return outcome.map_error(_lapsed).bind(lambda inner: inner)

    async def _dispatch(self, /) -> Result[SubtitleProduct, MediaFault]:
        match self.op:
            case SubtitleOp(tag="convert", convert=(text, src, dst)):
                return await self._crossed(_convert, text, src, dst)
            case SubtitleOp(tag="retime", retime=(text, dialect, shift)):
                return await self._crossed(_retime, text, dialect, shift)
            case SubtitleOp(tag="restyle", restyle=(text, dialect, ops)):
                return await self._crossed(_restyle, text, dialect, ops)
            case SubtitleOp(tag="mux", mux=(text, container, dialect)):
                return await self._crossed(_mux_subtitle, text, container, dialect)
            case SubtitleOp(tag="burn_in", burn_in=(text, dialect, frames, profile, burn)):
                return await self._crossed(_burn_in, text, dialect, frames, profile, burn)
            case SubtitleOp(tag="whisper", whisper=(result, dialect)):
                return await self._crossed(_whisper, result, dialect)
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _canon(op: SubtitleOp, /) -> tuple[bytes, ...]:
    # burn_in carries rgb24 frame arrays and tuple-keyed faces the deterministic msgpack encoder refuses, so its
    # preimage length-frames each field raw; every other case is msgpack-encodable whole.
    match op:
        case SubtitleOp(tag="burn_in", burn_in=(text, dialect, frames, profile, burn)):
            style = _CANON.encode((tuple(sorted(burn.faces.items())), burn.fallback))
            return _framed(b"burn_in", text.encode(), dialect.value.encode(), _CANON.encode(profile), style, *(np.asarray(plane).tobytes() for plane in frames))
        case _:
            return _framed(op.tag.encode(), _CANON.encode(op))


def _worker[**P](operation: Callable[P, Result[SubtitleProduct, MediaFault]], /) -> Callable[P, Result[SubtitleProduct, MediaFault]]:
    guarded = beartype(operation)

    @wraps(operation)
    def call(*args: P.args, **kwargs: P.kwargs) -> Result[SubtitleProduct, MediaFault]:
        try:
            return guarded(*args, **kwargs)
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=str(violation)))

    return call


def _subtitle_fault(op: str, exc: "pysubs2.exceptions.Pysubs2Error | KeyError | TypeError | ValueError | UnicodeError", /) -> MediaFault:
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
    return blob, SubtitleEvidence.measure(dialect.value, "text", duration, blob, len(subs), _band(subs))


@_worker
def _whisper(result: WhisperPayload, dialect: SubtitleDialect, /) -> Result[SubtitleProduct, MediaFault]:
    try:
        segments = result["segments"] if isinstance(result, dict) else result
        if not segments or not all(
            isfinite(segment["start"])
            and isfinite(segment["end"])
            and 0.0 <= segment["start"] <= segment["end"]
            and bool(segment["text"].strip())
            for segment in segments
        ) or any(left["start"] > right["start"] for left, right in zip(segments, segments[1:], strict=False)):
            return Error(MediaFault(invalid="Whisper segments must carry finite ordered times and non-empty text"))
        return Ok(_texted(pysubs2.load_from_whisper(result), dialect))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, KeyError, TypeError, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("whisper", exc))


@_worker
def _convert(text: str, src: SubtitleDialect, dst: SubtitleDialect, /) -> Result[SubtitleProduct, MediaFault]:
    try:
        return Ok(_texted(pysubs2.SSAFile.from_string(text, format_=src.value), dst))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("convert", exc))


@_worker
def _retime(text: str, dialect: SubtitleDialect, shift: RetimeShift, /) -> Result[SubtitleProduct, MediaFault]:
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        match shift:
            case RetimeShift(tag="constant", constant=ms):
                subs.shift(ms=ms)
            case RetimeShift(tag="rescale", rescale=(in_fps, out_fps)):
                if not (isfinite(in_fps) and isfinite(out_fps) and in_fps > 0.0 and out_fps > 0.0):
                    return Error(MediaFault(invalid="retime frame rates must be finite and positive"))
                subs.transform_framerate(in_fps, out_fps)
            case _ as unreachable:
                assert_never(unreachable)
        subs.sort()
        return Ok(_texted(subs, dialect))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("retime", exc))


@_worker
def _restyle(text: str, dialect: SubtitleDialect, ops: tuple[RestyleStep, ...], /) -> Result[SubtitleProduct, MediaFault]:
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        for step in ops:  # ordered fold over one parsed track; each step mutates the document in place
            match step:
                case RestyleStep(tag="rename", rename=(old, new)):
                    subs.rename_style(old, new)
                case RestyleStep(tag="import_", import_=(source, src_dialect, conflict)):
                    subs.import_styles(
                        pysubs2.SSAFile.from_string(source, format_=src_dialect.value), overwrite=conflict is StyleConflict.REPLACE
                    )
                case RestyleStep(tag="clean"):
                    subs.remove_miscellaneous_events()
                case _ as unreachable:
                    assert_never(unreachable)
        return Ok(_texted(subs, dialect))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("restyle", exc))


def _active(subs: "pysubs2.SSAFile", index: int, rate: int, /) -> Iterator[tuple[Styled, ...]]:
    at = pysubs2.make_time(frames=index, fps=rate)
    for event in subs:
        if event.is_text and event.start <= at < event.end:
            runs = tuple(pysubs2.formats.substation.parse_tags(event.text, styles=subs.styles, skip_empty_fragments=True))
            yield runs or ((event.plaintext, subs.styles.get("Default", pysubs2.SSAStyle())),)


def _anchored(style: "pysubs2.SSAStyle", plane: tuple[int, int], frame: tuple[int, int], /) -> tuple[int, int]:
    # ASS numpad alignment: column (a-1) % 3 (left/center/right), row (a-1) // 3 (bottom/middle/top), margins honored.
    a = int(style.alignment) - 1
    ph, pw = plane
    fh, fw = frame
    x = (style.marginl, (fw - pw) // 2, fw - pw - style.marginr)[a % 3]
    y = (fh - ph - style.marginv, (fh - ph) // 2, style.marginv)[a // 3]
    return x, y


def _specced(fragment: str, style: "pysubs2.SSAStyle", burn: BurnStyle, /) -> "TextSpec":
    # SSAStyle -> the filtergraph text plane spec; ASS alpha is transparency, so RGBA alpha inverts it. An
    # admitted visual axis the plane grammar cannot express — opaque-box borders (`borderstyle == 3`) or a
    # nonzero `shadow` — REFUSES typed here, never a silently different burn appearance.
    if int(style.borderstyle) == 3 or float(style.shadow) > 0.0:
        raise ValueError(f"<burn-style:unsupported:borderstyle={style.borderstyle},shadow={style.shadow}>")
    fill = (style.primarycolor.r, style.primarycolor.g, style.primarycolor.b, 255 - style.primarycolor.a)
    edge = (style.outlinecolor.r, style.outlinecolor.g, style.outlinecolor.b, 255 - style.outlinecolor.a)
    return TextSpec(text=fragment, font=burn.resolve(style), size=round(style.fontsize), x=0, y=0, color=fill, stroke=round(style.outline), stroke_fill=edge)


@lru_cache(maxsize=256)
def _plane(spec: "TextSpec", /) -> "NDArray[np.uint8]":
    # per-run raster memo: a static caption rasterizes ONCE across its frame span; the cached plane is read-only.
    tight = _render_text(spec, spec.size * max(len(spec.text), 1), spec.size * 2)
    live = np.argwhere(tight[..., 3] > 0)
    return tight if live.size == 0 else tight[: live[:, 0].max() + 1, : live[:, 1].max() + 1]


def _line(runs: tuple[Styled, ...], burn: BurnStyle, /) -> tuple["NDArray[np.uint8]", "pysubs2.SSAStyle"]:
    # positioning is a LINE property: runs vary fill/font/outline freely, but alignment and margins anchor the whole
    # concatenated bitmap, so a run disagreeing on them would silently place from runs[0] — refused typed instead,
    # the same admission law _specced applies to the visual axes the plane grammar cannot express.
    anchors = {(int(style.alignment), int(style.marginl), int(style.marginr), int(style.marginv)) for _, style in runs}
    if len(anchors) > 1:
        raise ValueError(f"<burn-style:mixed-anchor:{sorted(anchors)}>")
    planes = tuple(_plane(_specced(fragment, style, burn)) for fragment, style in runs)
    height, width = max(plane.shape[0] for plane in planes), sum(plane.shape[1] for plane in planes)
    line = np.zeros((height, width, 4), dtype=np.uint8)
    offset = 0
    for plane in planes:
        h, w = plane.shape[:2]
        line[:h, offset : offset + w] = plane
        offset += w
    return line, runs[0][1]


def _composite(base: "NDArray[np.uint8]", rgba: "NDArray[np.uint8]", x: int, y: int, /) -> None:
    height, width = base.shape[:2]
    rh, rw = rgba.shape[:2]
    left, top, right, bottom = max(x, 0), max(y, 0), min(x + rw, width), min(y + rh, height)
    if left >= right or top >= bottom:
        return
    source = rgba[top - y : bottom - y, left - x : right - x]
    region = base[top:bottom, left:right]
    alpha = source[..., 3:4].astype(np.float32) / 255.0
    region[...] = (source[..., :3].astype(np.float32) * alpha + region.astype(np.float32) * (1.0 - alpha)).astype(np.uint8)


def _burned(subs: "pysubs2.SSAFile", frames: Frames, profile: MediaProfile, burn: BurnStyle, /) -> Frames:
    def paint(index: int, base: "NDArray[np.uint8]", /) -> "NDArray[np.uint8]":
        canvas = base.copy()
        for runs in _active(subs, index, profile.rate):
            rgba, style = _line(runs, burn)
            x, y = _anchored(style, rgba.shape[:2], canvas.shape[:2])
            _composite(canvas, rgba, x, y)
        return canvas

    return tuple(paint(index, base) for index, base in enumerate(frames))


@_worker
def _burn_in(text: str, dialect: SubtitleDialect, frames: Frames, profile: MediaProfile, burn: BurnStyle, /) -> Result[SubtitleProduct, MediaFault]:
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        return _encode_video(_burned(subs, frames, profile, burn), profile).map(lambda pair: _from_container(pair, subs))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except (pysubs2.exceptions.Pysubs2Error, ValueError, UnicodeError) as exc:
        return Error(_subtitle_fault("burn_in", exc))


def _from_container(pair: tuple[bytes, MediaEvidence], subs: "pysubs2.SSAFile", /) -> SubtitleProduct:
    blob, evidence = pair
    merged = msgspec.structs.replace(evidence, facts=evidence.facts | _band(subs))
    return blob, SubtitleEvidence.measure(evidence.container.value, evidence.codec, evidence.duration, blob, evidence.frame_count, merged.facts)


def _ass_packets(subs: "pysubs2.SSAFile", track: object, /) -> Iterator["av.Packet"]:
    for order, ev in enumerate(sorted(subs, key=lambda e: e.start)):
        payload = f"{order},{ev.layer},{ev.style},{ev.name},{ev.marginl},{ev.marginr},{ev.marginv},{ev.effect},{ev.text}"
        packet = av.Packet(payload.encode("utf-8"))
        packet.stream, packet.time_base = track, Fraction(1, 1000)
        packet.pts, packet.dts, packet.duration = ev.start, ev.start, max(ev.end - ev.start, 0)
        yield packet


def _copied(reader: object, cloned: frozendict[int, object], /) -> Iterator["av.Packet"]:
    # a packet survives with EITHER timestamp — a pts-only packet still muxes — and only the fully
    # timestamp-less flush packet drops; the stream remaps before the yield.
    for packet in reader.demux(*(reader.streams[index] for index in cloned)):
        if packet.dts is not None or packet.pts is not None:
            packet.stream = cloned[packet.stream.index]
            yield packet


def _packet_time(packet: "av.Packet", /) -> Fraction:
    # `dts` wins whenever present — zero included, which a truthiness fold misreads — then `pts`, then zero.
    stamp = packet.dts if packet.dts is not None else packet.pts if packet.pts is not None else 0
    return stamp * packet.time_base


@_worker
def _mux_subtitle(text: str, container: bytes, dialect: SubtitleDialect, /) -> Result[SubtitleProduct, MediaFault]:
    # timed-packet passthrough: clone the source A/V streams, open an `ass` subtitle stream whose extradata is the
    # SubStation header, and mux one packet per event — the track is viewer-selectable, never an opaque attachment.
    try:
        subs = pysubs2.SSAFile.from_string(text, format_=dialect.value)
        sink = io.BytesIO()
        with av.open(io.BytesIO(container), mode="r") as reader:
            admitted = frozenset(reader.format.name.split(",")) & _SOFT_SUB.keys()
            if len(admitted) != 1:
                return Error(MediaFault(unregistered=("soft_subtitle_muxers", reader.format.name)))
            muxer = next(iter(admitted))
            with av.open(sink, mode="w", format=muxer) as out:
                out.metadata.update(reader.metadata)
                cloned = frozendict({src.index: out.add_stream_from_template(src) for src in reader.streams if src.type in ("video", "audio")})
                track = out.add_stream(_SOFT_SUB[muxer])
                header = subs.to_string(format_="ass").partition("[Events]")[0] + _ASS_EVENT_FORMAT
                track.codec_context.extradata = header.encode("utf-8")
                for packet in merge(_copied(reader, cloned), _ass_packets(subs, track), key=_packet_time):
                    out.mux_one(packet)
        blob = sink.getvalue()
        duration, _frames, _bit_rate, measured = _probe(blob)
        return Ok((blob, SubtitleEvidence.measure(muxer, _SOFT_SUB[muxer], duration, blob, len(subs), _band(subs) | measured)))
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
