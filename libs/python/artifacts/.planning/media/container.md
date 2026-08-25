# [PY_ARTIFACTS_MEDIA_CONTAINER]

Container/codec spine of the media plane: the one `Media` owner over the closed-payload `MediaOp` family and the shared `Media`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat`/`ColorProfile` family every media page composes. `Media` muxes a frame sequence into a single-blob container (MP4/WebM/MKV/GIF/MPEG-TS, plus the audio-only FLAC/OGG/WAV/MP3 rows) or a segmented sink (HLS/DASH/fMP4 or MPEG-TS segments) over the `av` (PyAV) FFmpeg floor, and reads back on the `Transcode`/`Remux` arms. This page owns the mux/demux capsule, the read-side `seek`+`flush_buffers` random-access primitive, the lazy `_decode_video` stream and the bounded `_decode_window`, the `HwAccel` decode probe, the segmented `io_open` sink, the `MEZZANINE` archival profile rows, and the video workers; it produces no frames — `scene/render#SCENE` rasterizes the sequence, `Media` only muxes it.

## [01]-[INDEX]

- [02]-[CONTAINER]: the `Media` owner over the closed-payload `MediaOp` family — `EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux` folding into the `emit`/`_emit` node contract keyed over the muxed container (or read-back manifest) bytes.

## [02]-[CONTAINER]

- Owner: `Media` discriminates modality over the closed `MediaOp` family, each case carrying its own typed payload — never a shared erased `params` bag, a per-modality subclass, or a parallel `encode_video`/`encode_audio`/`remux` trio; `Media.lane` is the `runtime/execution/lanes#LANE` `LanePolicy` every worker crossing rides; `ContainerFormat` keyed inside `MediaProfile`, its `segmented` predicate branching the sink-open onto the runtime `runtime/transport/roots#STORE` `ObjectStoreLane` (`file://`, `s3://`, `gs://` — one code path over the branch's own `StoreBackend` roster), never a parallel per-container owner and never a second fsspec provider stack beside the branch's; `MediaProfile` folds every muxer/codec/rate/color/segment/metadata knob into its `streamed`/`voiced`/`colored` projections, and `MEZZANINE` holds its archival preservation-master rows; `MediaEvidence` the typed encode output one `measure` constructor folds over the muxed/manifest bytes; the `av.open` `OutputContainer`/`InputContainer` one mux/demux capsule per op, always a context manager so the trailer writes and the native handle releases.
- Cases: `EncodeVideo(frames, profile)` the rgb24 (or DLPack device-tensor) sequence `scene/render#SCENE` hands across, segmented when `profile.segment` and keyed over the manifest bytes; `Mux(frames, samples, video, audio)` one interleave axis over two profiles, never a parallel A/V-combine surface; `EncodeAudio(samples, profile)` dispatched here, worked at `media/audio#MEDIA`; `Transcode(source, profile, nodes)` the read+write pair whose encoder stream mints on the first shaped frame so a scale/crop node's output geometry drives its config, the source's audio stream carried across as a packet copy over `add_stream_from_template` so a transcode never silently drops the soundtrack; `Remux(source, profile, bsf)` the quality-lossless container change, a packet copy over `add_stream_from_template` spanning EVERY source stream (video, audio, subtitles), never a re-decode — one total `match` recovers the modality from the discriminant. `media/filtergraph#FILTER` owns filter routing; this page composes its `wired` chain product, never re-implements a filter.
- Auto: `_key` folds the op's canonical byte stream through the bare `ContentIdentity.key` under the default `CANONICAL_POLICY` — profile bytes via one deterministic msgpack encoder, each frame/sample array as its `(shape, dtype)` header chunk beside its raw bytes so byte-identical buffers under permuted shapes or re-typed dtypes never share a key, every chunk length-framed and the tuple count-framed through `scene/spec#SPEC`'s `framed`/`CANON` — this page is the media plane's one import site for that pair and every media sibling composes it from here under the source spelling, never a page-local encoder — so the PRE-RUN key is content-true over the input; codec admission is two probes, the build registry (`av.codecs_available`) before open and the muxer admission (`container.supported_codecs`) after, so a missing encoder and a codec the muxer refuses each rail `unregistered` rather than raising deep in the worker; `_hwaccel` probes `hwdevices_available()` (a callable, not a set) before minting the `HWAccel` context, with `HwPolicy` projecting the provider boolean; `_open_sink` is one axis keyed by `container.segmented` — a `BytesIO` blob (segment count 0) or the `io_open` segment set staged on a worker-local tree and put through the one runtime object-store lane, published segments-first manifest-last only after a clean mux and discarded whole on any failure path, its lane refusal riding `_Lapse` onto `MediaFault.worker` because a synchronous provider callback can return no rail — and both arms stamp `profile.metadata` onto `container.metadata`; `_transcode` rails the `media/filtergraph#FILTER` builder's `ValueError` — a multi-source node inside a chain program, a non-positive declared count, a malformed weight row — as `invalid` beside its `ImportError`/`FFmpegError` arms, because a `FilterNode` program is caller data and never an engine fault; frame ingest, the AVCOL color stamp, and `pts` stamping ride `_lift`/`_drive`; a `MEZZANINE` row with `Verification.BYTE_EXACT` decodes its own blob back and compares frame bytes, the `"roundtrip"` verdict landing on the facts band.
- Growth: a new container is one `ContainerFormat` row (muxer name + `segmented` bit); a new codec one `MediaProfile.codec` string (a hardware encoder is a codec row, not a knob); a new HDR band one `ColorProfile` member plus one `_COLOR_CODES` row; a new encode or muxer knob one `options`/`container_options`/`SegmentSpec.options` entry; a new container tag one `metadata` entry; a new archival grade one `MEZZANINE` row; a new hardware device one `HwAccel.device_type` name; a new av fault leaf one `MediaFault` case plus one `_media_fault` arm; a new evidence fact one `_deployment` band key — every addition a row, field, case, or arm on one owner.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from builtins import frozendict
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.faults import TRANSIENT, BoundaryFault, Catch, FaultRow, RuntimeRail, async_boundary, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.hooks import ArtifactsLeg, BYTE_VOLUME, DOMAIN
from rasm.artifacts.core.plan import Admission, ArtifactWork

if TYPE_CHECKING:
    from collections.abc import Callable

    from rasm.artifacts.media.audio import Master, Pcm

# --- [TYPES] ----------------------------------------------------------------------------

type Frames = tuple[NDArray[np.uint8], ...]
type Samples = tuple["Pcm", ...]
type Produced = tuple[bytes, "MediaEvidence"]
type MediaOpTag = Literal["encode_video", "encode_audio", "mux", "transcode", "remux"]
type MediaFaultTag = Literal["unregistered", "invalid", "codec", "provision", "worker", "contract"]

# --- [CONSTANTS] ------------------------------------------------------------------------

MEDIA_RESIDUE: Final[Catch] = (BeartypeCallHintViolation, ValueError, OSError)

_FRAME_FORMAT = "rgb24"

_COLOR_CODES: frozendict[str, tuple[int, int, int, int]] = frozendict({
    "srgb": (1, 13, 1, 2),
    "display_p3": (12, 13, 1, 2),
    "bt709": (1, 1, 1, 1),
    "bt601": (6, 6, 6, 1),
    "bt2020_pq": (9, 16, 9, 1),
    "bt2020_hlg": (9, 18, 9, 1),
})


class ContainerFormat(StrEnum):
    MP4 = "mp4"
    WEBM = "webm"
    MKV = "matroska"
    GIF = "gif"
    FLAC = "flac"
    OGG = "ogg"
    WAV = "wav"
    MP3 = "mp3"
    HLS = "hls"
    DASH = "dash"
    SEGMENT = "segment"
    MPEGTS = "mpegts"

    @property
    def segmented(self) -> bool:
        return self in (ContainerFormat.HLS, ContainerFormat.DASH, ContainerFormat.SEGMENT)


class ColorProfile(StrEnum):
    SRGB = "srgb"
    DISPLAY_P3 = "display_p3"
    BT709 = "bt709"
    BT601 = "bt601"
    BT2020_PQ = "bt2020_pq"
    BT2020_HLG = "bt2020_hlg"


class HwPolicy(StrEnum):
    REQUIRED = "required"
    FALLBACK = "fallback"


class Verification(StrEnum):
    NONE = "none"
    BYTE_EXACT = "byte_exact"


_COVERED: tuple[tuple[frozenset[object], frozenset[object]], ...] = (
    (frozenset(_COLOR_CODES), frozenset(profile.value for profile in ColorProfile)),
)
if any(rows != vocabulary for rows, vocabulary in _COVERED):
    raise RuntimeError("container tables do not cover their vocabularies")


# --- [TABLES] ---------------------------------------------------------------------------

MEDIA_MUX: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.CONTAINER, point="mux", arm="boundary", defect="mux-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([MEDIA_MUX]))

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class MediaFault:
    tag: MediaFaultTag = tag()
    unregistered: tuple[str, str] = case()
    invalid: str = case()
    codec: tuple[str, str] = case()
    provision: str = case()
    worker: str = case()
    contract: str = case()


class _Lapse(Exception):
    def __init__(self, fault: BoundaryFault, /) -> None:
        super().__init__(fault)
        self.fault: BoundaryFault = fault


class HwAccel(Struct, frozen=True):
    device_type: str = "videotoolbox"
    policy: HwPolicy = HwPolicy.FALLBACK


class Attachment(Struct, frozen=True):
    name: str
    mimetype: str
    data: bytes


class SegmentSpec(Struct, frozen=True):
    root: str
    manifest: str = "index.m3u8"
    options: frozendict[str, str] = frozendict()
    storage_options: frozendict[str, str] = frozendict()


class MediaProfile(Struct, frozen=True):
    container: ContainerFormat = ContainerFormat.MP4
    codec: str = "libx264"
    rate: int = 24
    bit_rate: int | None = None
    gop_size: int | None = None
    max_b_frames: int | None = None
    pix_fmt: str = "yuv420p"
    frame_format: str = _FRAME_FORMAT
    layout: str = "stereo"
    thread_count: int = 0
    color: ColorProfile = ColorProfile.SRGB
    options: frozendict[str, str] = frozendict()
    container_options: frozendict[str, str] = frozendict()
    metadata: frozendict[str, str] = frozendict()
    hwaccel: "HwAccel | None" = None
    segment: "SegmentSpec | None" = None
    attachments: tuple[Attachment, ...] = ()
    master: "Master | None" = None
    verification: Verification = Verification.NONE

    def streamed(self, container: object, width: int, height: int) -> object:
        stream = container.add_stream(self.codec, rate=self.rate, options=dict(self.options))
        stream.width, stream.height, stream.pix_fmt = width, height, self.pix_fmt
        stream.codec_context.thread_count = self.thread_count
        for owner, field, value in ((stream, "bit_rate", self.bit_rate), (stream, "gop_size", self.gop_size), (stream.codec_context, "max_b_frames", self.max_b_frames)):
            if value is not None:
                setattr(owner, field, value)
        for embed in self.attachments:
            container.add_attachment(embed.name, embed.mimetype, embed.data)
        return stream

    def voiced(self, container: object) -> object:
        stream = container.add_stream(self.codec, rate=self.rate, options=dict(self.options))
        stream.codec_context.layout = self.layout
        stream.codec_context.thread_count = self.thread_count
        if self.bit_rate is not None:
            stream.bit_rate = self.bit_rate
        return stream

    def colored(self, frame: object, /) -> object:
        converted = frame.reformat(format=self.pix_fmt)
        primaries, trc, space, rng = _COLOR_CODES[self.color.value]
        for field, code in zip(
            ("color_primaries", "color_trc", "colorspace", "color_range"),
            (primaries, trc, space, rng),
            strict=True,
        ):
            setattr(converted, field, code)
        return converted


MEZZANINE: frozendict[str, MediaProfile] = frozendict({
    "ffv1": MediaProfile(container=ContainerFormat.MKV, codec="ffv1", pix_fmt="gbrp", gop_size=1, verification=Verification.BYTE_EXACT, options=frozendict({"level": "3", "slicecrc": "1"})),
    "flac": MediaProfile(container=ContainerFormat.FLAC, codec="flac"),
})


class MediaEvidence(Struct, frozen=True):
    container: ContainerFormat
    codec: str
    duration: float
    byte_count: int
    frame_count: int
    bit_rate: int
    facts: frozendict[str, float | str] = frozendict()

    @staticmethod
    def measure(
        container: ContainerFormat,
        codec: str,
        duration: float,
        frames: int,
        bit_rate: int,
        blob: bytes,
        facts: frozendict[str, float | str] = frozendict(),
    ) -> "MediaEvidence":
        return MediaEvidence(container, codec, duration, len(blob), frames, bit_rate, facts)


@tagged_union(frozen=True)
class MediaOp:
    tag: MediaOpTag = tag()
    encode_video: tuple[Frames, MediaProfile] = case()
    encode_audio: tuple[Samples, MediaProfile] = case()
    mux: tuple[Frames, Samples, MediaProfile, MediaProfile] = case()
    transcode: tuple[bytes, MediaProfile, "tuple[FilterNode, ...]"] = case()
    remux: tuple[bytes, MediaProfile, str] = case()


class Media(Struct, frozen=True):
    op: MediaOp
    lane: LanePolicy

    @staticmethod
    def of(subject: "Frames | MediaOp", lane: LanePolicy) -> "Media":
        match subject:
            case MediaOp() as op:
                return Media(op=op, lane=lane)
            case frames:
                return Media(op=MediaOp(encode_video=(frames, MediaProfile())), lane=lane)

    def emit(self, /) -> ArtifactWork[Produced]:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"media-{self.op.tag}", _canon(self.op))

    async def _emit(self) -> RuntimeRail[Produced]:
        railed = await async_boundary(MEDIA_MUX, self._folded, catch=MEDIA_RESIDUE)
        settled = railed.bind(lambda res: res.map_error(lambda fault: BoundaryFault(domain=(MEDIA_MUX.subject, fault))))
        match settled:
            case Result(tag="ok", ok=product):
                Metrics.record({BYTE_VOLUME: float(len(product[0]))}, domain=DOMAIN, kind="media", scope=self.lane.scope)
        return settled

    async def _folded(self) -> Result[Produced, MediaFault]:
        return await self._mux()

    async def _mux(self) -> Result[Produced, MediaFault]:
        match self.op:
            case MediaOp(tag="encode_video", encode_video=(frames, profile)):
                return await self._crossed(_encode_video, frames, profile)
            case MediaOp(tag="encode_audio", encode_audio=(samples, profile)):
                return await self._crossed(_encode_audio, samples, profile)
            case MediaOp(tag="mux", mux=(frames, samples, video, audio)):
                return await self._crossed(_mux_av, frames, samples, video, audio)
            case MediaOp(tag="transcode", transcode=(source, profile, nodes)):
                return await self._crossed(_transcode, source, profile, nodes)
            case MediaOp(tag="remux", remux=(source, profile, bsf)):
                return await self._crossed(_remux, source, profile, bsf)
            case _ as unreachable:
                assert_never(unreachable)

    async def _crossed(self, worker: "Callable[..., Result[Produced, MediaFault]]", /, *args: object) -> Result[Produced, MediaFault]:
        replayable = not any(isinstance(a, MediaProfile) and a.container.segmented and a.segment is not None for a in args)
        outcome = await self.lane.offload(Kernel.of(worker, KernelTrait.HOSTILE, idempotent=replayable), *args)
        return outcome.map_error(_lapsed).bind(lambda inner: inner)
```

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable, Iterator
from fractions import Fraction
from functools import wraps
from itertools import chain, takewhile, zip_longest
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import TYPE_CHECKING, BinaryIO
from urllib.parse import urlsplit

from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result

from rasm.runtime.roots import ObjectStoreLane, ResourceRef, StoreOp

from rasm.artifacts.scene.spec import CANON, framed

lazy import av
lazy import av.error
lazy import av.codec.hwaccel
lazy from rasm.artifacts.media.audio import _encode_audio, _voiced
lazy from rasm.artifacts.media.filtergraph import FilterNode, wired

if TYPE_CHECKING:
    from rasm.artifacts.media.audio import Pcm

# --- [OPERATIONS] -----------------------------------------------------------------------


def _worker[**P, R](operation: Callable[P, Result[R, MediaFault]], /) -> Callable[P, Result[R, MediaFault]]:
    guarded = beartype(operation)

    @wraps(operation)
    def call(*args: P.args, **kwargs: P.kwargs) -> Result[R, MediaFault]:
        try:
            return guarded(*args, **kwargs)
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=str(violation)))
        except _Lapse as lapse:
            return Error(MediaFault(worker=str(lapse.fault)))

    return call


def _arrayed(part: object, /) -> tuple[bytes, bytes]:
    array = np.asarray(part)
    return CANON.encode((array.shape, array.dtype.str)), array.tobytes()


def _canon(op: MediaOp) -> tuple[bytes, ...]:
    match op:
        case MediaOp(tag="encode_video", encode_video=(frames, profile)):
            return framed(b"encode_video", CANON.encode(profile), *chain.from_iterable(map(_arrayed, frames)))
        case MediaOp(tag="encode_audio", encode_audio=(samples, profile)):
            return framed(b"encode_audio", CANON.encode(profile), *chain.from_iterable(map(_arrayed, samples)))
        case MediaOp(tag="mux", mux=(frames, samples, video, audio)):
            return framed(b"mux", CANON.encode(video), CANON.encode(audio), *chain.from_iterable(map(_arrayed, (*frames, *samples))))
        case MediaOp(tag="transcode", transcode=(source, profile, nodes)):
            return framed(b"transcode", CANON.encode(profile), *(CANON.encode(node.facet()) for node in nodes), source)
        case MediaOp(tag="remux", remux=(source, profile, bsf)):
            return framed(b"remux", CANON.encode(profile), bsf.encode(), source)
        case _ as unreachable:
            assert_never(unreachable)


def _lapsed(fault: BoundaryFault, /) -> MediaFault:
    return MediaFault(worker=str(fault))


def _media_fault(op: str, exc: "av.error.FFmpegError", /) -> MediaFault:
    match exc:
        case av.error.InvalidDataError():
            return MediaFault(invalid=str(exc))
        case (
            av.error.EncoderNotFoundError()
            | av.error.DecoderNotFoundError()
            | av.error.MuxerNotFoundError()
            | av.error.DemuxerNotFoundError()
            | av.error.FilterNotFoundError()
            | av.error.BSFNotFoundError()
        ):
            return MediaFault(unregistered=(type(exc).__name__, str(exc)))
        case _:
            return MediaFault(codec=(op, str(exc)))


def _deployment(profile: MediaProfile | None) -> frozendict[str, float | str]:
    versions = av.library_versions
    runtime = frozendict({
        "libavcodec": float(versions["libavcodec"][0]),
        "libavformat": float(versions["libavformat"][0]),
        "ffmpeg": av.ffmpeg_version_info,
    })
    return (
        runtime
        if profile is None
        else runtime
        | {"color": profile.color.value, "pix_fmt": profile.pix_fmt, "rate": float(profile.rate)}
        | ({"target_bit_rate": float(profile.bit_rate)} if profile.bit_rate is not None else {})
        | ({"gop_size": float(profile.gop_size)} if profile.gop_size is not None else {})
    )


def _codec_ok(name: str, /) -> bool:
    return name in av.codecs_available


def _lift(profile: MediaProfile, array: object) -> object:
    frame = (
        av.VideoFrame.from_ndarray(array, format=profile.frame_format)
        if isinstance(array, np.ndarray)
        else av.VideoFrame.from_dlpack(array, format=profile.frame_format)
    )
    return profile.colored(frame)


def _frame_flaw(array: object, extent: tuple[int, int], index: int, /) -> str | None:
    match array:
        case np.ndarray() if array.ndim != 3 or array.shape != (*extent, 3) or array.dtype != np.uint8:
            return f"frame {index} must be uint8 ({extent[0]}, {extent[1]}, 3)"
        case _:
            return None


def _hwaccel(spec: "HwAccel | None") -> "av.codec.hwaccel.HWAccel | None":
    if spec is None:
        return None
    if spec.device_type not in av.codec.hwaccel.hwdevices_available():
        if spec.policy is HwPolicy.REQUIRED:
            raise av.error.DecoderNotFoundError(19, f"hwdevices_available: {spec.device_type}")
        return None
    return av.codec.hwaccel.HWAccel(device_type=spec.device_type, allow_software_fallback=spec.policy is HwPolicy.FALLBACK)


def _segment_sink(spec: SegmentSpec) -> tuple["Callable[[str, int], BinaryIO]", "Callable[[], tuple[bytes, int]]", "Callable[[], None]"]:
    staging = TemporaryDirectory(prefix="rasm-media-segments-")
    root = Path(staging.name)
    lane = ObjectStoreLane.of(
        ResourceRef(scheme=urlsplit(spec.root).scheme or "file", root=spec.root, relative=spec.manifest, owner="artifacts.media.segment"),
        config=dict(spec.storage_options) or None,
    )
    opened: list[str] = []

    def io_open(url: str, flags: int, /) -> BinaryIO:
        if url != spec.manifest:
            opened.append(url)
        target = root / url
        target.parent.mkdir(parents=True, exist_ok=True)
        return target.open("wb")

    def _put(url: str, /) -> bytes:
        payload = (root / url).read_bytes()
        match lane.run(StoreOp.Put(payload), url):
            case Result(tag="error", error=fault):
                raise _Lapse(fault)
            case _:
                return payload

    def publish() -> tuple[bytes, int]:
        for url in opened:
            _put(url)
        return _put(spec.manifest), len(opened)

    def discard() -> None:
        if root.exists():
            staging.cleanup()

    return io_open, publish, discard


def _open_sink(profile: MediaProfile) -> tuple[object, "Callable[[], tuple[bytes, int]]", "Callable[[], None]"]:
    if profile.container.segmented and profile.segment is not None:
        spec = profile.segment
        io_open, publish, discard = _segment_sink(spec)
        container = av.open(
            spec.manifest,
            mode="w",
            format=profile.container.value,
            options=dict(spec.options),
            container_options=dict(profile.container_options),
            io_open=io_open,
        )
        container.metadata.update(dict(profile.metadata))
        return container, publish, discard
    sink = io.BytesIO()
    container = av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options))
    container.metadata.update(dict(profile.metadata))
    return container, lambda: (sink.getvalue(), 0), lambda: None


def _drive(container: object, stream: object, frame: object, index: int, rate: int) -> None:
    frame.pts, frame.time_base = index, Fraction(1, rate)
    for packet in stream.encode(frame):
        container.mux_one(packet)


def _flush(container: object, stream: object) -> None:
    for packet in stream.encode(None):
        container.mux_one(packet)


def _seek(reader: object, stream: object, seconds: float, /) -> None:
    reader.seek(int(seconds / stream.time_base), backward=True, stream=stream)
    stream.codec_context.flush_buffers()


def _probe(blob: bytes) -> tuple[float, int, int, frozendict[str, float | str]]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        duration = float(reader.duration / av.time_base) if reader.duration is not None else 0.0
        video = next(iter(reader.streams.video), None)
        audio = next(iter(reader.streams.audio), None)
        measured = frozendict(
            ({
                "width": float(video.width),
                "height": float(video.height),
                "frame_rate": float(video.average_rate or 0),
                "pix_fmt": str(video.pix_fmt or "unknown"),
            } if video is not None else {})
            | ({"sample_rate": float(audio.sample_rate), "layout": str(audio.layout)} if audio is not None else {})
        )
        return (
            duration,
            video.frames if video is not None else 0,
            int(video.bit_rate or reader.bit_rate or 0) if video is not None else int(reader.bit_rate or 0),
            measured,
        )


def _video_stream(reader: "av.container.InputContainer") -> "av.video.stream.VideoStream":
    stream = reader.streams.best("video")
    if stream is None:
        raise av.error.InvalidDataError(22, "source carries no video stream")
    return stream


def _source_rate(blob: bytes) -> int:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        return round(float(_video_stream(reader).average_rate or 24))


def _frames(blob: bytes, accel: "HwAccel | None" = None) -> "Iterator[NDArray[np.uint8]]":
    with av.open(io.BytesIO(blob), mode="r", hwaccel=_hwaccel(accel)) as reader:
        for frame in reader.decode(_video_stream(reader)):
            yield frame.to_ndarray(format="rgb24")


def _decode_video(blob: bytes, accel: "HwAccel | None" = None) -> tuple[int, "Iterator[NDArray[np.uint8]]"]:
    return _source_rate(blob), _frames(blob, accel)


def _decode_window(blob: bytes, in_point: float, out_point: float, accel: "HwAccel | None" = None) -> tuple[int, Frames]:
    with av.open(io.BytesIO(blob), mode="r", hwaccel=_hwaccel(accel)) as reader:
        stream = _video_stream(reader)
        _seek(reader, stream, in_point)
        rolled = (frame for frame in reader.decode(stream) if frame.time is not None)
        window = takewhile(lambda frame: frame.time < out_point, rolled)
        kept = tuple(frame.to_ndarray(format="rgb24") for frame in window if frame.time >= in_point)
        return round(float(stream.average_rate or 24)), kept


def _roundtrip(blob: bytes, frames: Frames, /) -> str:
    _rate, decoded = _decode_video(blob)
    exact = all(
        out is not None and src is not None and np.array_equal(np.asarray(src), out)
        for src, out in zip_longest(frames, decoded, fillvalue=None)
    )
    return "verified" if exact else "diverged"


@_worker
def _encode_video(frames: "Frames | Iterable[NDArray[np.uint8]]", profile: MediaProfile) -> Result[Produced, MediaFault]:
    try:
        stream_in = iter(frames)
        head = next(stream_in, None)
        if head is None:
            return Error(MediaFault(invalid="empty frame sequence"))
        if profile.container.segmented != (profile.segment is not None):
            return Error(MediaFault(invalid="segmented containers require SegmentSpec and single-blob containers reject it"))
        if not _codec_ok(profile.codec):
            return Error(MediaFault(unregistered=("codecs_available", profile.codec)))
        if isinstance(head, np.ndarray) and (head.ndim != 3 or head.shape[2:] != (3,) or head.dtype != np.uint8):
            return Error(MediaFault(invalid="frames must be uint8 (h, w, 3) rgb24"))
        height, width = head.shape[:2]
        keep = profile.verification is Verification.BYTE_EXACT and not profile.container.segmented
        retained: list["NDArray[np.uint8]"] = []
        count_in = 0
        container, publish, discard = _open_sink(profile)
        try:
            with container:
                if profile.codec not in container.supported_codecs:
                    return Error(MediaFault(unregistered=("supported_codecs", profile.codec)))
                stream = profile.streamed(container, width, height)
                for index, array in enumerate(chain((head,), stream_in)):
                    if (flaw := _frame_flaw(array, (height, width), index)) is not None:
                        return Error(MediaFault(invalid=flaw))
                    if keep:
                        retained.append(array)
                    _drive(container, stream, _lift(profile, array), index, profile.rate)
                    count_in = index + 1
                _flush(container, stream)
            blob, segments = publish()
        finally:
            discard()
        duration, count, bit_rate, measured = (
            (count_in / profile.rate, count_in, int(profile.bit_rate or 0), frozendict({"width": float(width), "height": float(height), "frame_rate": float(profile.rate), "pix_fmt": profile.pix_fmt}))
            if profile.container.segmented
            else _probe(blob)
        )
        facts = (
            _deployment(profile) | measured
            | ({"segments": float(segments)} if profile.container.segmented else {})
            | ({"roundtrip": _roundtrip(blob, tuple(retained))} if keep else {})
        )
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, facts)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("encode_video", exc))
    except ValueError as exc:
        return Error(MediaFault(invalid=str(exc)))


def _mux_av(frames: "Frames | Iterable[NDArray[np.uint8]]", blocks: "tuple[Pcm, ...]", video: MediaProfile, audio: MediaProfile) -> Result[Produced, MediaFault]:
    try:
        stream_in = iter(frames)
        head = next(stream_in, None)
        if head is None or not blocks:
            return Error(MediaFault(invalid="empty frame or sample sequence"))
        if any(block.dtype != blocks[0].dtype for block in blocks):
            return Error(MediaFault(invalid="one muxed pcm sequence must keep one producer dtype"))
        channels = av.AudioLayout(audio.layout).nb_channels
        if any(block.ndim != 2 or block.shape[0] != 1 or block.shape[1] % channels for block in blocks):
            return Error(MediaFault(invalid=f"pcm blocks must be packed (1, samples*channels) for layout {audio.layout}"))
        if video.container != audio.container:
            return Error(MediaFault(invalid="mux profiles must name one container"))
        if video.container.segmented != (video.segment is not None):
            return Error(MediaFault(invalid="segmented containers require SegmentSpec and single-blob containers reject it"))
        if not (_codec_ok(video.codec) and _codec_ok(audio.codec)):
            return Error(MediaFault(unregistered=("codecs_available", f"{video.codec}/{audio.codec}")))
        if isinstance(head, np.ndarray) and (head.ndim != 3 or head.shape[2:] != (3,) or head.dtype != np.uint8):
            return Error(MediaFault(invalid="frames must be uint8 (h, w, 3) rgb24"))
        height, width = head.shape[:2]
        count_in = 0
        container, publish, discard = _open_sink(video)
        try:
            with container:
                if video.codec not in container.supported_codecs or audio.codec not in container.supported_codecs:
                    return Error(MediaFault(unregistered=("supported_codecs", f"{video.codec}/{audio.codec}")))
                vstream = video.streamed(container, width, height)
                astream = audio.voiced(container)
                container.start_encoding()
                for index, array in enumerate(chain((head,), stream_in)):
                    if (flaw := _frame_flaw(array, (height, width), index)) is not None:
                        return Error(MediaFault(invalid=flaw))
                    _drive(container, vstream, _lift(video, array), index, video.rate)
                    count_in = index + 1
                _voiced(container, astream, blocks, audio)
                _flush(container, vstream)
                _flush(container, astream)
            blob, segments = publish()
        finally:
            discard()
        duration, count, bit_rate, measured = (
            (count_in / video.rate, count_in, int(video.bit_rate or 0), frozendict({"width": float(width), "height": float(height), "frame_rate": float(video.rate), "pix_fmt": video.pix_fmt}))
            if video.container.segmented
            else _probe(blob)
        )
        facts = _deployment(video) | measured | {"sample_rate": float(audio.rate), "layout": audio.layout} | ({"segments": float(segments)} if video.container.segmented else {})
        return Ok((blob, MediaEvidence.measure(video.container, video.codec, duration, count, bit_rate, blob, facts)))
    except _Lapse as lapse:
        return Error(MediaFault(worker=str(lapse.fault)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("mux", exc))
    except ValueError as exc:
        return Error(MediaFault(invalid=str(exc)))


def _carried(packet: object, container: object, voice: object, carried: object, /) -> "Iterator[object]":
    if carried is not None and packet.stream is voice:
        if packet.dts is not None:
            packet.stream = carried
            container.mux_one(packet)
        return iter(())
    return iter(packet.decode())


@_worker
def _transcode(source: bytes, profile: MediaProfile, nodes: "tuple[FilterNode, ...]") -> Result[Produced, MediaFault]:
    try:
        if not _codec_ok(profile.codec):
            return Error(MediaFault(unregistered=("codecs_available", profile.codec)))
        if profile.container.segmented != (profile.segment is not None):
            return Error(MediaFault(invalid="segmented containers require SegmentSpec and single-blob containers reject it"))
        with av.open(io.BytesIO(source), mode="r", hwaccel=_hwaccel(profile.hwaccel)) as reader:
            src = _video_stream(reader)
            container, publish, discard = _open_sink(profile)
            try:
                with container:
                    if profile.codec not in container.supported_codecs:
                        return Error(MediaFault(unregistered=("supported_codecs", profile.codec)))
                    voice = next(iter(reader.streams.audio), None)
                    carried = container.add_stream_from_template(voice) if voice is not None else None
                    staged = wired(nodes, src) if nodes else None
                    selected = tuple(live for live in (src, voice) if live is not None)
                    decoded = (frame for packet in reader.demux(selected) for frame in _carried(packet, container, voice, carried))
                    stream: object | None = None
                    extent: tuple[int, int] | None = None
                    index = 0
                    for frame in chain(decoded, (None,) if staged is not None else ()):
                        for shaped in (staged.driven(frame) if staged is not None else ((frame,) if frame is not None else ())):
                            if stream is None:
                                extent = (shaped.width, shaped.height)
                                stream = profile.streamed(container, shaped.width, shaped.height)
                            _drive(container, stream, profile.colored(shaped), index, profile.rate)
                            index += 1
                    if stream is not None:
                        _flush(container, stream)
                if stream is None or extent is None:
                    return Error(MediaFault(invalid="source produced no video frames"))
                blob, segments = publish()
            finally:
                discard()
        duration, count, bit_rate, measured = (
            (index / profile.rate, index, int(profile.bit_rate or 0), frozendict({"width": float(extent[0]), "height": float(extent[1]), "frame_rate": float(profile.rate), "pix_fmt": profile.pix_fmt}))
            if profile.container.segmented
            else _probe(blob)
        )
        facts = _deployment(profile) | measured | {"filter_nodes": float(staged.node_count if staged is not None else 0)} | ({"segments": float(segments)} if profile.container.segmented else {})
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, facts)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("transcode", exc))
    except ValueError as exc:
        return Error(MediaFault(invalid=str(exc)))


@_worker
def _remux(source: bytes, profile: MediaProfile, bsf: str) -> Result[Produced, MediaFault]:
    try:
        if bsf and bsf not in av.bitstream_filters_available:
            return Error(MediaFault(unregistered=("bitstream_filters_available", bsf)))
        if profile.container.segmented != (profile.segment is not None):
            return Error(MediaFault(invalid="segmented containers require SegmentSpec and single-blob containers reject it"))
        with av.open(io.BytesIO(source), mode="r") as reader:
            container, publish, discard = _open_sink(profile)
            try:
                with container:
                    copied = next((stream.codec_context.name for stream in reader.streams if stream.type in ("video", "audio")), "copy")
                    mapped = {src.index: container.add_stream_from_template(src) for src in reader.streams}
                    shaping = {src.index: av.BitStreamFilterContext(bsf, src, mapped[src.index]) for src in reader.streams.video} if bsf else {}
                    muxed, end = 0, 0.0
                    for packet in reader.demux():
                        if packet.dts is None:
                            continue
                        stamp = packet.dts if packet.pts is None else packet.pts
                        muxed += 1 if packet.stream.type == "video" else 0
                        end = max(end, float((stamp + (packet.duration or 0)) * packet.stream.time_base))
                        bitstream = shaping.get(packet.stream.index)
                        for shaped in bitstream.filter(packet) if bitstream is not None else (packet,):
                            shaped.stream = mapped[packet.stream.index]
                            container.mux_one(shaped)
                    for index, bitstream in shaping.items():
                        for shaped in bitstream.filter(None):
                            shaped.stream = mapped[index]
                            container.mux_one(shaped)
                blob, segments = publish()
            finally:
                discard()
        duration, count, bit_rate, measured = (end, muxed, int(profile.bit_rate or 0), frozendict()) if profile.container.segmented else _probe(blob)
        facts = _deployment(None) | measured | ({"segments": float(segments)} if profile.container.segmented else {})
        return Ok((blob, MediaEvidence.measure(profile.container, copied, duration, count, bit_rate, blob, facts)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("remux", exc))
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
