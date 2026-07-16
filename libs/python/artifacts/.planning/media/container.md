# [PY_ARTIFACTS_MEDIA_CONTAINER]

Container/codec spine of the media plane: the one `Media` owner over the closed-payload `MediaOp` family and the shared `Media`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat`/`ColorProfile` family every media page composes. `Media` muxes a frame sequence into a single-blob container (MP4/WebM/MKV/GIF) or a segmented sink (HLS/DASH/fMP4/MPEG-TS) over the `av` (PyAV) FFmpeg floor, and reads back on the `Transcode`/`Remux` arms. This page owns the mux/demux capsule, the read-side `seek`+`flush_buffers` random-access primitive, the `HwAccel` decode probe, the segmented `io_open` sink, and the video worker; it produces no frames — `scene/render#SCENE` rasterizes the sequence, `Media` only muxes it.

`MediaProfile` folds every muxer, codec, rate, color, and segment knob into its bound `streamed`/`voiced`/`colored` projections, so a new knob is a field or `options` entry rather than a loose constructor argument. Each op returns `Result[(ContentKey, ArtifactReceipt), MediaFault]` keyed over the muxed bytes, or over the read-back manifest bytes for a segmented encode; `MediaFault` is the closed av-boundary cause vocabulary the whole media rail threads, sibling to `graphic/vector/region#REGION` `RegionFault` and `graphic/raster/io#IO` `RasterFault`, so a registration miss, a malformed stream, a worker death, and a contract violation are each structurally addressable. Every worker crosses the `WORKER_BAND` process lane under the runtime retry class with the finished bytes the only payload back, `emit()` IS the `core/plan#PLAN` `ArtifactWork.work` thunk, and each op contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Media` case whose `facts` band carries the HDR-color tag and segment count. `EncodeAudio` composes at `media/audio#MEDIA`, the capability-routed `FilterNode` graph at `media/filtergraph#FILTER`, and the NLE trim/concat/xfade layer at `media/timeline#TIMELINE`.

## [01]-[INDEX]

- [01]-[CONTAINER]: the `Media` owner over the closed-payload `MediaOp` family — `EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux` folding into the `emit`/`_emit` node contract keyed over the muxed container (or read-back manifest) bytes.

## [02]-[CONTAINER]

- Owner: `Media` discriminates modality over the closed `MediaOp` family, each case carrying its own typed payload — never a shared erased `params` bag, a per-modality subclass, or a parallel `encode_video`/`encode_audio`/`remux` trio; `ContainerFormat` keyed inside `MediaProfile`, its `segmented` predicate branching the sink-open to a `UPath` root (local or `s3://`/`gs://` through the admitted fsspec backends), never a parallel per-container owner; `MediaProfile` folds every muxer/codec/rate/color/segment knob into its `streamed`/`voiced`/`colored` projections; `MediaEvidence` the typed encode receipt one `measure` constructor folds over the muxed/manifest bytes; the `av.open` `OutputContainer`/`InputContainer` one mux/demux capsule per op, always a context manager so the trailer writes and the native handle releases.
- Cases: `EncodeVideo(frames, profile)` the rgb24 (or DLPack device-tensor) sequence `scene/render#SCENE` hands across, segmented when `profile.segment` and keyed over the manifest bytes; `Mux(frames, samples, video, audio)` one interleave axis over two profiles, never a parallel A/V-combine surface; `EncodeAudio(samples, profile)` dispatched here, worked at `media/audio#MEDIA`; `Transcode(source, profile, nodes)` the read+write pair whose encoder stream mints on the first shaped frame so a scale/crop node's output geometry drives its config; `Remux(source, profile, bsf)` the quality-lossless container change, a packet copy over `add_stream_from_template`, never a re-decode — one total `match` recovers the modality from the discriminant. `media/filtergraph#FILTER` owns filter routing; this page composes its `build_graph` product, never re-implements a filter.
- Entry: `emit()` returns the `ArtifactWork` node; `_emit` is the `async_boundary` wrapper returning `RuntimeRail[ArtifactReceipt]`, the domain `MediaFault` folded into the boundary rail because `Work[ArtifactReceipt]` forbids an inner `Result`, the terminal receipt threading the PRE-RUN key; `_mux` dispatches the synchronous `av` body onto the process lane under the retry class, `_keyed` deriving `ContentIdentity.of(container, blob)` and constructing the receipt case.
- Auto: `streamed` mints the stream only after `profile.codec in av.codecs_available`, else `MediaFault.unregistered`, never a blind `add_stream` raising deep in the worker; `_open_sink` is one axis keyed by `container.segmented` — a `BytesIO` blob (segment count 0) or the `io_open` UPath segment set finalized by reading the manifest bytes back; frame ingest, the AVCOL color stamp, and `pts` stamping ride `_lift`/`_drive`.
- Receipt: each op contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media`, the eight-slot case `_keyed` keys over the muxed/manifest bytes, so the receipt owner imports no `MediaEvidence` value nor `av` handle — the flat-scalar-plus-`facts`-band shape forecloses the cycle; `MediaEvidence.facts` carries the `_deployment` libav/ffmpeg majors, the `ColorProfile` tag, and a segmented encode's segment count, and the `media/audio#MEDIA`/`media/filtergraph#FILTER`/`media/timeline#TIMELINE` arms all fold onto this one case through the shared band, never a parallel receipt rail.
- Growth: a new container is one `ContainerFormat` row (muxer name + `segmented` bit); a new codec one `MediaProfile.codec` string (a hardware encoder is a codec row, not a knob); a new HDR band one `ColorProfile` member plus one `_COLOR_CODES` row; a new encode or muxer knob one `options`/`container_options`/`SegmentSpec.options` entry; a new hardware device one `HwAccel.device_type` name; a new av fault leaf one `MediaFault` case plus one `_media_fault` arm; a new evidence fact one `_deployment` band key with zero receipt edit — every addition a row, field, case, or arm on one owner.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from anyio import BrokenWorkerProcess, to_process
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Result, case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from artifacts.media.audio import Master, Pcm
    from artifacts.media.filtergraph import FilterNode

# --- [TYPES] ----------------------------------------------------------------------------

type Frames = tuple[NDArray[np.uint8], ...]  # host rgb24 rasters or DLPack device tensors _lift discriminates
type Samples = tuple["Pcm", ...]
type MediaOpTag = Literal["encode_video", "encode_audio", "mux", "transcode", "remux"]
type MediaFaultTag = Literal["unregistered", "invalid", "codec", "provision", "worker", "contract"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FRAME_FORMAT = "rgb24"

# ColorProfile -> the (color_primaries, color_trc, colorspace, color_range) INTEGER AVCOL quad each frame attribute
# requires; `frame.color_primaries = 9` is the settable path, `= "bt2020"` raises `an integer is required`.
_COLOR_CODES: frozendict[str, tuple[int, int, int, int]] = frozendict({
    "srgb": (1, 13, 1, 2),  # bt709 primaries, IEC61966-2-1 sRGB trc, bt709 matrix, full range
    "bt709": (1, 1, 1, 1),  # HD SDR: bt709 primaries/trc/matrix, tv range
    "bt601": (6, 6, 6, 1),  # SD SDR: smpte170m
    "bt2020_pq": (9, 16, 9, 1),  # HDR10: bt2020 primaries, SMPTE2084 PQ trc, bt2020-ncl matrix
    "bt2020_hlg": (9, 18, 9, 1),  # HLG: bt2020 primaries, ARIB-STD-B67 HLG trc, bt2020-ncl matrix
})

# the transient subprocess-death seam retries; a BeartypeCallHintViolation is NOT in the `.on(...)` set and lifts to `contract`.


class ContainerFormat(StrEnum):
    MP4 = "mp4"
    WEBM = "webm"
    MKV = "matroska"
    GIF = "gif"
    HLS = "hls"  # segmented: io_open sinks the .m3u8 manifest + fMP4/TS segments to the UPath root
    DASH = "dash"
    SEGMENT = "segment"
    MPEGTS = "mpegts"

    @property
    def segmented(self) -> bool:
        return self in (ContainerFormat.HLS, ContainerFormat.DASH, ContainerFormat.SEGMENT, ContainerFormat.MPEGTS)


class ColorProfile(StrEnum):
    SRGB = "srgb"
    BT709 = "bt709"
    BT601 = "bt601"
    BT2020_PQ = "bt2020_pq"  # HDR10
    BT2020_HLG = "bt2020_hlg"


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class MediaFault:
    # each case is structurally addressable so a downstream `match` routes a registration miss apart from a
    # malformed stream apart from a worker death apart from a contract miss.
    tag: MediaFaultTag = tag()
    unregistered: tuple[str, str] = case()  # a probed codec/muxer/filter/bsf absent from the build — (registry, name)
    invalid: str = case()  # av InvalidDataError — malformed frame/stream data
    codec: tuple[str, str] = case()  # any other av FFmpegError during encode/mux/transcode/remux — (op, strerror)
    provision: str = case()  # the bundled FFmpeg build is absent (av ImportError at the worker seam)
    worker: str = case()  # an exhausted BrokenWorkerProcess subprocess death past `_WORKER_RETRY`
    contract: str = case()  # a BeartypeCallHintViolation lifted at the worker seam


class HwAccel(Struct, frozen=True):
    # `_hwaccel` probes device_type against `hwdevices_available` before minting HWAccel; a missing device degrades
    # to software decode when `allow_software_fallback`, never a hard crash on a build without the GPU backend.
    device_type: str = "videotoolbox"  # HWDeviceType name (videotoolbox/cuda/vaapi/qsv/d3d11va)
    allow_software_fallback: bool = True


class Attachment(Struct, frozen=True):
    name: str
    mimetype: str
    data: bytes  # a font for a later subtitle burn, a cover image — `add_attachment` embed


class SegmentSpec(Struct, frozen=True):
    # `_segment_sink` builds the `io_open(url, flags)` callback opening `UPath(root)/url`; `manifest` is the top-level
    # playlist name av.open writes, `options` the muxer knobs (hls_time/hls_segment_type/seg_duration).
    root: str  # a UPath url — file:///…, s3://bucket/prefix, gs://…
    manifest: str = "index.m3u8"
    options: frozendict[str, str] = frozendict()
    storage_options: frozendict[str, str] = frozendict()


class MediaProfile(Struct, frozen=True):
    container: ContainerFormat = ContainerFormat.MP4
    codec: str = "libx264"  # the encoder name; a hardware encoder (h264_videotoolbox/hevc_videotoolbox) is a codec row
    rate: int = 24
    bit_rate: int | None = None
    gop_size: int | None = None
    pix_fmt: str = "yuv420p"  # the encoder pixel format the frame reformats to (yuv420p10le for a 10-bit HDR grade)
    frame_format: str = _FRAME_FORMAT  # the producer ndarray pixel format `from_ndarray` ingests; reformat bridges it to `pix_fmt`
    layout: str = "stereo"
    thread_count: int = 0
    color: ColorProfile = ColorProfile.SRGB  # the HDR/color band; BT2020_PQ over yuv420p10le is a real HDR10 grade
    options: frozendict[str, str] = frozendict()  # codec-private knobs (crf/preset/tune) -> add_stream(options=)
    container_options: frozendict[str, str] = frozendict()  # muxer-private knobs (movflags=frag_keyframe+empty_moov for fMP4)
    hwaccel: "HwAccel | None" = None  # read-side hardware decode (Transcode/random-access); None = software
    segment: "SegmentSpec | None" = None  # segmented io_open sink; None = one BytesIO blob
    attachments: tuple[Attachment, ...] = ()  # add_attachment embeds (fonts/covers)
    master: "Master | None" = None

    def streamed(self, container: object, width: int, height: int) -> object:
        stream = container.add_stream(self.codec, rate=self.rate, options=dict(self.options))
        stream.width, stream.height, stream.pix_fmt = width, height, self.pix_fmt
        stream.codec_context.thread_count = self.thread_count
        for field, value in (("bit_rate", self.bit_rate), ("gop_size", self.gop_size)):
            if value is not None:
                setattr(stream, field, value)
        for embed in self.attachments:
            container.add_attachment(embed.name, embed.mimetype, embed.data)
        return stream

    def voiced(self, container: object) -> object:
        stream = container.add_stream(self.codec, rate=self.rate, options=dict(self.options))
        stream.codec_context.layout = self.layout  # ch-layout set before start_encoding so the resampler's frames are accepted
        stream.codec_context.thread_count = self.thread_count
        if self.bit_rate is not None:
            stream.bit_rate = self.bit_rate
        return stream

    def colored(self) -> frozendict[str, int]:
        # the per-frame INTEGER AVCOL quad the worker sets after the pix_fmt reformat.
        primaries, trc, space, rng = _COLOR_CODES[self.color.value]
        return frozendict({"color_primaries": primaries, "color_trc": trc, "colorspace": space, "color_range": rng})


class MediaEvidence(Struct, frozen=True):
    container: ContainerFormat
    codec: str
    duration: float
    byte_count: int
    frame_count: int
    bit_rate: int
    facts: frozendict[str, float | str] = frozendict()  # libav majors + ffmpeg build + color tag + segment count -> ArtifactReceipt.Media band

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
    transcode: tuple[bytes, MediaProfile, "tuple[FilterNode, ...]"] = case()  # source bytes -> decode -> filtergraph Graph -> encode -> mux
    remux: tuple[bytes, MediaProfile, str] = case()  # source bytes -> demux -> BSF -> mux copy, no re-decode

    @staticmethod
    def EncodeVideo(frames: Frames, profile: MediaProfile) -> "MediaOp":
        return MediaOp(encode_video=(frames, profile))

    @staticmethod
    def EncodeAudio(samples: Samples, profile: MediaProfile) -> "MediaOp":
        return MediaOp(encode_audio=(samples, profile))

    @staticmethod
    def Mux(frames: Frames, samples: Samples, video: MediaProfile, audio: MediaProfile) -> "MediaOp":
        return MediaOp(mux=(frames, samples, video, audio))

    @staticmethod
    def Transcode(source: bytes, profile: MediaProfile, nodes: "tuple[FilterNode, ...]" = ()) -> "MediaOp":
        return MediaOp(transcode=(source, profile, nodes))

    @staticmethod
    def Remux(source: bytes, profile: MediaProfile, bsf: str = "") -> "MediaOp":
        return MediaOp(remux=(source, profile, bsf))


class Media(Struct, frozen=True):
    op: MediaOp

    @staticmethod
    def of(frames: Frames, profile: MediaProfile | None = None) -> "Media":
        return Media(op=MediaOp.EncodeVideo(frames, profile if profile is not None else MediaProfile()))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the canonical frozen op minted PRE-RUN; the muxed output's content address rides the receipt facts.
        return ContentIdentity.of(f"media-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the inner Result[..., MediaFault] folds into the rail's boundary fault (Work[ArtifactReceipt] forbids an
        # inner Result); the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        railed = await async_boundary(f"media.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map(lambda pair: pair[1]).map_error(
                lambda fault: BoundaryFault(boundary=(f"media.{self.op.tag}", f"{fault.tag}:{fault}"))
            )
        )

    async def _folded(self) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
        return (await self._mux()).map(self._keyed)

    def _keyed(self, produced: tuple[bytes, MediaEvidence], /) -> tuple[ContentKey, ArtifactReceipt]:
        blob, evidence = produced
        key = ContentIdentity.of(evidence.container.value, blob)
        return key, ArtifactReceipt.Media(
            key,
            evidence.container.value,
            evidence.codec,
            evidence.duration,
            evidence.byte_count,
            evidence.frame_count,
            evidence.bit_rate,
            evidence.facts,
        )

    async def _mux(self) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
        try:
            match self.op:
                case MediaOp(tag="encode_video", encode_video=(frames, profile)):
                    return (await LanePolicy.offload(_encode_video, frames, profile, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_media_raise)
                case MediaOp(tag="encode_audio", encode_audio=(samples, profile)):
                    return (await LanePolicy.offload(_encode_audio, samples, profile, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_media_raise)
                case MediaOp(tag="mux", mux=(frames, samples, video, audio)):
                    return (await LanePolicy.offload(_mux_av, frames, samples, video, audio, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_media_raise)
                case MediaOp(tag="transcode", transcode=(source, profile, nodes)):
                    return (await LanePolicy.offload(_transcode, source, profile, nodes, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_media_raise)
                case MediaOp(tag="remux", remux=(source, profile, bsf)):
                    return (await LanePolicy.offload(_remux, source, profile, bsf, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_media_raise)
                case _:
                    assert_never(self.op)
        except BrokenWorkerProcess as broken:
            return Error(MediaFault(worker=str(broken)))
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=type(violation).__name__))
```

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable, Iterator
from fractions import Fraction
from itertools import chain
from typing import TYPE_CHECKING, BinaryIO

from beartype import beartype
from expression import Error, Ok, Result

# the workers stay module-level so the process lane dispatches them by qualified name; `av` is worker-scope only,
# and the audio worker and filtergraph builder defer through `lazy from` so neither couples the container import.
lazy import av
lazy import av.error
lazy import av.codec.hwaccel
lazy from upath import UPath  # the segmented-sink egress root; module-scope (a lazy stmt inside a function is a SyntaxError)
lazy from artifacts.media.audio import _encode_audio, _voiced
lazy from artifacts.media.filtergraph import build_graph

if TYPE_CHECKING:
    from artifacts.media.audio import Pcm
    from artifacts.media.filtergraph import FilterNode

# --- [OPERATIONS] -----------------------------------------------------------------------


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


def _deployment(profile: MediaProfile) -> frozendict[str, float | str]:
    # libav majors + ffmpeg build read once off the live linkage, plus the ColorProfile tag; a segmented encode
    # merges its segment count in at finalize.
    versions = av.library_versions
    return frozendict({
        "libavcodec": float(versions["libavcodec"][0]),
        "libavformat": float(versions["libavformat"][0]),
        "ffmpeg": av.ffmpeg_version_info,
        "color": profile.color.value,
    })


def _codec_ok(name: str, /) -> bool:
    return name in av.codecs_available  # build probe before add_stream, so a missing encoder rails `unregistered` not a deep raise


def _lift(profile: MediaProfile, array: object) -> object:
    # host rgb24 ndarray -> `from_ndarray`; a non-numpy DLPack device tensor -> `from_dlpack`. Discriminate on
    # `isinstance(np.ndarray)`, NOT `hasattr("__dlpack__")` — numpy arrays expose `__dlpack__` too and mis-route the
    # host path. Then reformat to `pix_fmt` and stamp the INTEGER AVCOL quad (the bitstream-VUI setattr).
    frame = (
        av.VideoFrame.from_ndarray(array, format=profile.frame_format)
        if isinstance(array, np.ndarray)
        else av.VideoFrame.from_dlpack(array, format=profile.frame_format)
    )
    converted = frame.reformat(format=profile.pix_fmt) if profile.pix_fmt != profile.frame_format else frame
    for attr, code in profile.colored().items():
        setattr(converted, attr, code)
    return converted


def _hwaccel(profile: MediaProfile) -> "av.codec.hwaccel.HWAccel | None":
    # probe the build's device list before minting the context, so a machine without the GPU backend degrades to
    # software (allow_software_fallback) rather than crashing at `av.open(hwaccel=)`.
    spec = profile.hwaccel
    if spec is None or spec.device_type not in av.codec.hwaccel.hwdevices_available:
        return None
    return av.codec.hwaccel.HWAccel(device_type=spec.device_type, allow_software_fallback=spec.allow_software_fallback)


def _segment_sink(spec: SegmentSpec) -> tuple[Callable[[str, int], BinaryIO], list[str]]:
    # the io_open per-segment callback: av.open calls it once per segment/manifest url; each opens `UPath(root)/url` on
    # the fsspec backend so HLS/DASH/fMP4 segments land local or remote. The `opened` list counts non-manifest segments.
    root = UPath(spec.root, **dict(spec.storage_options))
    opened: list[str] = []

    def io_open(url: str, flags: int, /) -> BinaryIO:
        if url != spec.manifest:
            opened.append(url)
        return (root / url).open("wb")

    return io_open, opened


def _open_sink(profile: MediaProfile) -> tuple[object, Callable[[], tuple[bytes, int]]]:
    # one sink axis keyed by `container.segmented`: a single in-memory blob, or the io_open UPath segment set whose
    # keyed product is the read-back manifest bytes and whose segment count rides the receipt band.
    if profile.container.segmented and profile.segment is not None:
        spec = profile.segment
        io_open, opened = _segment_sink(spec)
        container = av.open(spec.manifest, mode="w", format=profile.container.value, options=dict(spec.options), io_open=io_open)

        def finalize() -> tuple[bytes, int]:
            manifest = (UPath(spec.root, **dict(spec.storage_options)) / spec.manifest).read_bytes()
            return manifest, len(opened)

        return container, finalize
    sink = io.BytesIO()
    container = av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options))
    return container, lambda: (sink.getvalue(), 0)


def _drive(container: object, stream: object, frame: object, index: int, rate: int) -> None:
    frame.pts, frame.time_base = index, Fraction(1, rate)
    for packet in stream.encode(frame):
        container.mux_one(packet)  # per-packet mux so an encoder emitting a burst back-pressures one packet at a time


def _flush(container: object, stream: object) -> None:
    for packet in stream.encode(None):
        container.mux_one(packet)


def _filtered(graph: "av.filter.Graph | None", frame: object, /) -> Iterator[object]:
    if graph is None:
        yield frame
        return
    graph.push(frame)
    while True:  # libavfilter pull protocol: drain until EAGAIN (needs input) or EOF (flushed)
        try:
            yield graph.pull()
        except BlockingIOError, EOFError:
            return


def _seek(reader: object, stream: object, seconds: float, /) -> None:
    # the random-access primitive media/timeline#TIMELINE composes for Trim/Segment in-points: seek the demuxer to the
    # keyframe at/before the timestamp then flush the decoder so no stale reference frame bleeds across the cut.
    reader.seek(int(seconds / stream.time_base), backward=True, stream=stream)
    stream.codec_context.flush_buffers()


def _probe(blob: bytes) -> tuple[float, int, int]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        video = reader.streams.video[0]
        duration = float(reader.duration / av.time_base) if reader.duration is not None else 0.0
        return duration, video.frames, int(video.bit_rate or 0)


def _decoded(reader: object, stream: object) -> Iterator[NDArray[np.uint8]]:
    for frame in reader.decode(stream):  # lazy per-frame so a long clip never materializes every frame at once
        yield frame.to_ndarray(format="rgb24")


def _decode_video(blob: bytes) -> tuple[Frames, int]:
    # the read-side inverse of _encode_video: demux+decode one video stream to rgb24 blocks + the source frame rate
    # (VideoStream.average_rate rounded to int).
    with av.open(io.BytesIO(blob), mode="r") as reader:
        stream = reader.streams.best("video")
        return tuple(_decoded(reader, stream)), round(float(stream.average_rate or 24))


def _decode_window(blob: bytes, in_point: float, out_point: float) -> tuple[Frames, int]:
    # _seek to the keyframe at/before the in-point, flush, then keep only frames whose presentation time
    # (VideoFrame.time = pts·time_base) lands in [in_point, out_point) — the pre-roll decoded and dropped, so the
    # cut is frame-exact not GOP-aligned.
    with av.open(io.BytesIO(blob), mode="r") as reader:
        stream = reader.streams.best("video")
        _seek(reader, stream, in_point)
        kept = tuple(
            frame.to_ndarray(format="rgb24") for frame in reader.decode(stream) if frame.time is not None and in_point <= frame.time < out_point
        )
        return kept, round(float(stream.average_rate or 24))


@beartype
def _media_raise(fault: object) -> object:
    raise ValueError(str(fault))


def _encode_video(frames: Frames, profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    try:
        if not _codec_ok(profile.codec):
            return Error(MediaFault(unregistered=("codecs_available", profile.codec)))
        height, width = frames[0].shape[:2]
        container, finalize = _open_sink(profile)
        with container:
            stream = profile.streamed(container, width, height)
            for index, array in enumerate(frames):
                _drive(container, stream, _lift(profile, array), index, profile.rate)
            _flush(container, stream)
        blob, segments = finalize()
        # segmented output's blob IS the manifest (no single-blob video stream to re-probe), so evidence reads the source
        # frame count and the exact temporal extent (frames / rate); a single-blob encode re-opens the muxed bytes.
        duration, count, bit_rate = (
            (len(frames) / profile.rate, len(frames), int(profile.bit_rate or 0)) if profile.container.segmented else _probe(blob)
        )
        facts = _deployment(profile) | {"segments": float(segments)} if profile.container.segmented else _deployment(profile)
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, facts)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("encode_video", exc))


# _mux_av alone is un-`@beartype`d: its `blocks` param is the audio-owned `Pcm` union, a TYPE_CHECKING-only forward ref
# that would NameError at call-time hint resolution; the mux arm's contract is proven statically by ty/mypy.
def _mux_av(frames: Frames, blocks: "tuple[Pcm, ...]", video: MediaProfile, audio: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    try:
        if not (_codec_ok(video.codec) and _codec_ok(audio.codec)):
            return Error(MediaFault(unregistered=("codecs_available", f"{video.codec}/{audio.codec}")))
        height, width = frames[0].shape[:2]
        sink = io.BytesIO()
        with av.open(sink, mode="w", format=video.container.value, container_options=dict(video.container_options)) as container:
            vstream = video.streamed(container, width, height)
            astream = audio.voiced(container)
            container.start_encoding()  # publishes the audio frame_size before `_voiced` builds the resampler
            for index, array in enumerate(frames):
                _drive(container, vstream, _lift(video, array), index, video.rate)
            _voiced(container, astream, blocks, audio)  # shared audio drive: dtype-keyed lift, master/resample/reframe, cumulative-sample pts
            _flush(container, vstream)
            _flush(container, astream)
        blob = sink.getvalue()
        duration, count, bit_rate = _probe(blob)
        return Ok((blob, MediaEvidence.measure(video.container, video.codec, duration, count, bit_rate, blob, _deployment(video))))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("mux", exc))


@beartype
def _transcode(source: bytes, profile: MediaProfile, nodes: "tuple[FilterNode, ...]") -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # decode -> filtergraph Graph -> encode -> mux over one owned read+write pair; the optional HwAccel context
    # accelerates decode and the encoder stream mints on the first shaped frame so a scale/crop node drives its config.
    try:
        if not _codec_ok(profile.codec):
            return Error(MediaFault(unregistered=("codecs_available", profile.codec)))
        sink = io.BytesIO()
        with (
            av.open(io.BytesIO(source), mode="r", hwaccel=_hwaccel(profile)) as reader,
            av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as container,
        ):
            src = reader.streams.best("video")
            wired = build_graph(nodes, src) if nodes else None  # media/filtergraph#FILTER: native/substitute-routed graph + numpy composites
            graph = wired.graph if wired is not None else None
            stream: object | None = None
            index = 0
            for frame in chain(reader.decode(src), (None,) if graph is not None else ()):  # None flush drains a buffering filter's tail
                for pulled in _filtered(graph, frame):
                    shaped = (
                        wired.composited(pulled) if wired is not None else pulled
                    )  # apply the text/subtitle numpy substitutes after the graph pull
                    if stream is None:
                        stream = profile.streamed(container, shaped.width, shaped.height)
                    _drive(container, stream, shaped.reformat(format=profile.pix_fmt), index, profile.rate)
                    index += 1
            if stream is not None:
                _flush(container, stream)
        blob = sink.getvalue()
        duration, count, bit_rate = _probe(blob)
        facts = _deployment(profile) | {"filter_nodes": float(wired.node_count if wired is not None else 0)}
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, facts)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("transcode", exc))


@beartype
def _remux(source: bytes, profile: MediaProfile, bsf: str) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # demux -> BitStreamFilterContext.filter (h264_mp4toannexb/…) -> mux copy, never a re-decode; `add_stream_from_template`
    # clones the source parameters. The bsf name is probed against `av.bitstream_filters_available`, else `unregistered`.
    try:
        if bsf and bsf not in av.bitstream_filters_available:
            return Error(MediaFault(unregistered=("bitstream_filters_available", bsf)))
        sink = io.BytesIO()
        with (
            av.open(io.BytesIO(source), mode="r") as reader,
            av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as container,
        ):
            src = reader.streams.best("video")
            out = container.add_stream_from_template(src)
            bitstream = av.BitStreamFilterContext(bsf, src, out) if bsf else None
            for packet in reader.demux(src):
                if packet.dts is None:  # the demuxer's terminal flush packet carries no timestamp
                    continue
                for shaped in bitstream.filter(packet) if bitstream is not None else (packet,):
                    shaped.stream = out
                    container.mux_one(shaped)
            for shaped in bitstream.filter(None) if bitstream is not None else ():  # filter(None) drains the buffered packets
                shaped.stream = out
                container.mux_one(shaped)
        blob = sink.getvalue()
        duration, count, bit_rate = _probe(blob)
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, _deployment(profile))))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("remux", exc))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
