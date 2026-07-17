# [PY_ARTIFACTS_MEDIA_CONTAINER]

Container/codec spine of the media plane: the one `Media` owner over the closed-payload `MediaOp` family and the shared `Media`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat`/`ColorProfile` family every media page composes. `Media` muxes a frame sequence into a single-blob container (MP4/WebM/MKV/GIF/MPEG-TS, plus the audio-only FLAC/OGG/WAV/MP3 rows) or a segmented sink (HLS/DASH/fMP4 or MPEG-TS segments) over the `av` (PyAV) FFmpeg floor, and reads back on the `Transcode`/`Remux` arms. This page owns the mux/demux capsule, the read-side `seek`+`flush_buffers` random-access primitive, the lazy `_decode_video` stream and the bounded `_decode_window`, the `HwAccel` decode probe, the segmented `io_open` sink, the `MEZZANINE` archival profile rows, and the video workers; it produces no frames — `scene/render#SCENE` rasterizes the sequence, `Media` only muxes it.

`MediaProfile` folds every muxer, codec, rate, color, segment, metadata, and archival knob into its bound `streamed`/`voiced`/`colored` projections, so a new knob is a field or `options` entry rather than a loose constructor argument. Each op returns `Result[ArtifactReceipt, MediaFault]` whose receipt threads the PRE-RUN node key (`receipt.slot == node.key`, the `core/receipt#RECEIPT` elision law) and lands the muxed-bytes — or read-back-manifest — content address as the `address` band fact; `MediaFault` is the closed av-boundary cause vocabulary the whole media rail threads, sibling to `graphic/vector/region#REGION` `RegionFault` and `graphic/raster/io#IO` `RasterFault`, so a registration miss, a malformed stream, a worker death, and a contract violation are each structurally addressable. Every worker crosses the process lane through the `execution/lanes#LANE` instance offload — `self.lane.offload(Kernel.of(worker, KernelTrait.HOSTILE), ...)`, the HOSTILE trait routing onto the warm loky pool whose `WORKER_BAND` and trait-row worker-death retry ride `execution/workers#POOL` — with the finished bytes the only payload back; `emit()` IS the `core/plan#PLAN` `ArtifactWork.work` thunk, and each op contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Media` case whose `facts` band carries the HDR-color tag, segment count, and archival round-trip verdict. `EncodeAudio` composes at `media/audio#MEDIA`, the capability-routed `FilterNode` graph at `media/filtergraph#FILTER`, and the NLE trim/concat/xfade layer at `media/timeline#TIMELINE`.

## [01]-[INDEX]

- [01]-[CONTAINER]: the `Media` owner over the closed-payload `MediaOp` family — `EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux` folding into the `emit`/`_emit` node contract keyed over the muxed container (or read-back manifest) bytes.

## [02]-[CONTAINER]

- Owner: `Media` discriminates modality over the closed `MediaOp` family, each case carrying its own typed payload — never a shared erased `params` bag, a per-modality subclass, or a parallel `encode_video`/`encode_audio`/`remux` trio; `Media.lane` is the `execution/lanes#LANE` `LanePolicy` every worker crossing rides; `ContainerFormat` keyed inside `MediaProfile`, its `segmented` predicate branching the sink-open to a `UPath` root (local or `s3://`/`gs://` through the admitted fsspec backends), never a parallel per-container owner; `MediaProfile` folds every muxer/codec/rate/color/segment/metadata knob into its `streamed`/`voiced`/`colored` projections, and `MEZZANINE` holds its archival preservation-master rows; `MediaEvidence` the typed encode receipt one `measure` constructor folds over the muxed/manifest bytes; the `av.open` `OutputContainer`/`InputContainer` one mux/demux capsule per op, always a context manager so the trailer writes and the native handle releases.
- Cases: `EncodeVideo(frames, profile)` the rgb24 (or DLPack device-tensor) sequence `scene/render#SCENE` hands across, segmented when `profile.segment` and keyed over the manifest bytes; `Mux(frames, samples, video, audio)` one interleave axis over two profiles, never a parallel A/V-combine surface; `EncodeAudio(samples, profile)` dispatched here, worked at `media/audio#MEDIA`; `Transcode(source, profile, nodes)` the read+write pair whose encoder stream mints on the first shaped frame so a scale/crop node's output geometry drives its config, the source's audio stream carried across as a packet copy over `add_stream_from_template` so a transcode never silently drops the soundtrack; `Remux(source, profile, bsf)` the quality-lossless container change, a packet copy over `add_stream_from_template` spanning EVERY source stream (video, audio, subtitles), never a re-decode — one total `match` recovers the modality from the discriminant. `media/filtergraph#FILTER` owns filter routing; this page composes its `wired` chain product, never re-implements a filter.
- Entry: `Media.of` discriminates construction on input shape — a `MediaOp` passes through and a frame tuple becomes the `encode_video` case under the default profile — while non-default payloads construct the active `MediaOp` case directly, never through modality-named one-hop factories; `emit()` returns the `ArtifactWork` node; `_emit` is the `async_boundary` wrapper returning `RuntimeRail[ArtifactReceipt]`, the domain `MediaFault` folded into the boundary rail because `Work[ArtifactReceipt]` forbids an inner `Result`, the terminal receipt threading the PRE-RUN key; `_mux` is one total `match` handing each worker to `_crossed`, which awaits `self.lane.offload(Kernel.of(worker, KernelTrait.HOSTILE), ...)`, folds the crossing's terminal `BoundaryFault` onto `MediaFault` through `_lapsed`, and flattens the worker's inner rail — no raise ever bridges the two rails; `_keyed` threads the node key into the receipt case and demotes the output's content address to the `address` band fact.
- Auto: `_key` folds the op's canonical byte stream through the bare `ContentIdentity.key` under the default `CANONICAL_POLICY` — profile bytes via one deterministic msgpack encoder, frame/sample bytes raw, every chunk length-framed and the tuple count-framed through `scene/spec#SPEC`'s `framed`/`CANON` per `docs/laws/patterns` row [05], one owner and no local fork — so the PRE-RUN key is content-true over the input; codec admission is two probes, the build registry (`av.codecs_available`) before open and the muxer admission (`container.supported_codecs`) after, so a missing encoder and a codec the muxer refuses each rail `unregistered` rather than raising deep in the worker; `_hwaccel` probes `hwdevices_available()` (a callable, not a set) before minting the `HWAccel` context, with `HwPolicy` projecting the provider boolean; `_open_sink` is one axis keyed by `container.segmented` — a `BytesIO` blob (segment count 0) or the `io_open` UPath segment set staged under a run-scoped prefix, published segments-first manifest-last only after a clean mux and discarded whole on any failure path — and both arms stamp `profile.metadata` onto `container.metadata`; frame ingest, the AVCOL color stamp, and `pts` stamping ride `_lift`/`_drive`; a `MEZZANINE` row with `Verification.BYTE_EXACT` decodes its own blob back and compares frame bytes, the `"roundtrip"` verdict landing on the facts band.
- Receipt: each op contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media`, the eight-slot case `_keyed` mints under the PRE-RUN node key with the muxed/manifest content address on the `address` band fact, so the receipt owner imports no `MediaEvidence` value nor `av` handle — the flat-scalar-plus-`facts`-band shape forecloses the cycle; `MediaEvidence.facts` carries the `_deployment` libav/ffmpeg majors, encoding arms add their `ColorProfile`/format/rate policy, `_probe` measures every delivered video and audio stream, segmented output adds its segment count, and archival encode adds its round-trip verdict; `Remux` reports the copied stream codec and no unapplied encode policy. `media/audio#MEDIA`/`media/filtergraph#FILTER`/`media/timeline#TIMELINE` arms all fold onto this one case through the shared band, never a parallel receipt rail.
- Growth: a new container is one `ContainerFormat` row (muxer name + `segmented` bit); a new codec one `MediaProfile.codec` string (a hardware encoder is a codec row, not a knob); a new HDR band one `ColorProfile` member plus one `_COLOR_CODES` row; a new encode or muxer knob one `options`/`container_options`/`SegmentSpec.options` entry; a new container tag one `metadata` entry; a new archival grade one `MEZZANINE` row; a new hardware device one `HwAccel.device_type` name; a new av fault leaf one `MediaFault` case plus one `_media_fault` arm; a new evidence fact one `_deployment` band key with zero receipt edit — every addition a row, field, case, or arm on one owner.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from builtins import frozendict
from expression import Result, case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from collections.abc import Callable

    from rasm.artifacts.media.audio import Master, Pcm
    from rasm.artifacts.media.filtergraph import FilterNode

# --- [TYPES] ----------------------------------------------------------------------------

type Frames = tuple[NDArray[np.uint8], ...]  # host rgb24 rasters or DLPack device tensors _lift discriminates
type Samples = tuple["Pcm", ...]
type Produced = tuple[bytes, "MediaEvidence"]  # every worker's success payload — (muxed or manifest bytes, evidence)
type MediaOpTag = Literal["encode_video", "encode_audio", "mux", "transcode", "remux"]
type MediaFaultTag = Literal["unregistered", "invalid", "codec", "provision", "worker", "contract"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FRAME_FORMAT = "rgb24"

# ColorProfile -> the (color_primaries, color_trc, colorspace, color_range) INTEGER AVCOL quad each frame attribute
# requires; `frame.color_primaries = 9` is the settable path, `= "bt2020"` raises `an integer is required`.
_COLOR_CODES: frozendict[str, tuple[int, int, int, int]] = frozendict({
    "srgb": (1, 13, 1, 2),  # bt709 primaries, IEC61966-2-1 sRGB trc, bt709 matrix, full range
    "display_p3": (12, 13, 1, 2),  # P3-D65 (SMPTE432) primaries, sRGB trc, bt709 matrix, full range
    "bt709": (1, 1, 1, 1),  # HD SDR: bt709 primaries/trc/matrix, tv range
    "bt601": (6, 6, 6, 1),  # SD SDR: smpte170m
    "bt2020_pq": (9, 16, 9, 1),  # HDR10: bt2020 primaries, SMPTE2084 PQ trc, bt2020-ncl matrix
    "bt2020_hlg": (9, 18, 9, 1),  # HLG: bt2020 primaries, ARIB-STD-B67 HLG trc, bt2020-ncl matrix
})


class ContainerFormat(StrEnum):
    MP4 = "mp4"
    WEBM = "webm"
    MKV = "matroska"
    GIF = "gif"
    FLAC = "flac"  # audio-only single-blob rows: archival, streaming, interchange, and delivery audio containers
    OGG = "ogg"
    WAV = "wav"
    MP3 = "mp3"
    HLS = "hls"  # segmented: io_open sinks the .m3u8 manifest + fMP4/TS segments to the UPath root
    DASH = "dash"
    SEGMENT = "segment"
    MPEGTS = "mpegts"  # one MPEG-TS blob; SegmentSpec selects MPEG-TS segments under HLS/segment

    @property
    def segmented(self) -> bool:
        return self in (ContainerFormat.HLS, ContainerFormat.DASH, ContainerFormat.SEGMENT)


class ColorProfile(StrEnum):
    SRGB = "srgb"
    DISPLAY_P3 = "display_p3"
    BT709 = "bt709"
    BT601 = "bt601"
    BT2020_PQ = "bt2020_pq"  # HDR10
    BT2020_HLG = "bt2020_hlg"


class HwPolicy(StrEnum):
    REQUIRED = "required"
    FALLBACK = "fallback"


class Verification(StrEnum):
    NONE = "none"
    BYTE_EXACT = "byte_exact"


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class MediaFault:
    # each case is structurally addressable so a downstream `match` routes a registration miss apart from a
    # malformed stream apart from a worker death apart from a contract miss.
    tag: MediaFaultTag = tag()
    unregistered: tuple[str, str] = case()  # a probed codec/muxer/filter/bsf absent from the build or muxer — (registry, name)
    invalid: str = case()  # av InvalidDataError or an empty/degenerate input the worker refuses
    codec: tuple[str, str] = case()  # any other av FFmpegError during encode/mux/transcode/remux — (op, strerror)
    provision: str = case()  # the bundled FFmpeg build is absent (av ImportError at the worker seam)
    worker: str = case()  # the offload's terminal BoundaryFault past the OCCT retry, folded by _lapsed
    contract: str = case()  # the rail-preserving worker aspect catches a beartype violation before offload return


class HwAccel(Struct, frozen=True):
    # `_hwaccel` probes device_type against the `hwdevices_available()` CALL before minting HWAccel; a missing device
    # degrades to software decode under `HwPolicy.FALLBACK` and rails under `HwPolicy.REQUIRED`.
    device_type: str = "videotoolbox"  # HWDeviceType name (videotoolbox/cuda/vaapi/qsv/d3d11va)
    policy: HwPolicy = HwPolicy.FALLBACK


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
    max_b_frames: int | None = None  # B-frame ceiling -> codec_context; 0 disables B-frames for a low-latency stream
    pix_fmt: str = "yuv420p"  # the encoder pixel format the frame reformats to (yuv420p10le for a 10-bit HDR grade)
    frame_format: str = _FRAME_FORMAT  # the producer ndarray pixel format `from_ndarray` ingests; reformat bridges it to `pix_fmt`
    layout: str = "stereo"
    thread_count: int = 0
    color: ColorProfile = ColorProfile.SRGB  # the HDR/color band; BT2020_PQ over yuv420p10le is a real HDR10 grade
    options: frozendict[str, str] = frozendict()  # codec-private knobs (crf/preset/tune) -> add_stream(options=)
    container_options: frozendict[str, str] = frozendict()  # muxer-private knobs (movflags=frag_keyframe+empty_moov for fMP4)
    metadata: frozendict[str, str] = frozendict()  # container tags (title/artist/comment) -> container.metadata
    hwaccel: "HwAccel | None" = None  # read-side hardware decode (Transcode/random-access); None = software
    segment: "SegmentSpec | None" = None  # segmented io_open sink; None = one BytesIO blob
    attachments: tuple[Attachment, ...] = ()  # add_attachment embeds (fonts/covers)
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
        stream.codec_context.layout = self.layout  # ch-layout set before start_encoding so the resampler's frames are accepted
        stream.codec_context.thread_count = self.thread_count
        if self.bit_rate is not None:
            stream.bit_rate = self.bit_rate
        return stream

    def colored(self, frame: object, /) -> object:
        # one output-frame projection owns pixel-format conversion plus the INTEGER AVCOL quad for every producer arm.
        converted = frame.reformat(format=self.pix_fmt)
        primaries, trc, space, rng = _COLOR_CODES[self.color.value]
        for field, code in zip(
            ("color_primaries", "color_trc", "colorspace", "color_range"),
            (primaries, trc, space, rng),
            strict=True,
        ):
            setattr(converted, field, code)
        return converted


# archival mezzanine rows — mathematically lossless preservation masters with decode-verified round-trip evidence;
# FFV1 level 3 with per-slice CRC is the moving-image preservation grade, gbrp keeping the rgb24 master bit-exact.
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
    facts: frozendict[str, float | str] = frozendict()  # libav majors + ffmpeg build + color tag + segment count + roundtrip verdict -> ArtifactReceipt.Media band

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
    remux: tuple[bytes, MediaProfile, str] = case()  # source bytes -> demux -> BSF -> mux copy over every stream, no re-decode


class Media(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    op: MediaOp
    lane: LanePolicy

    @staticmethod
    def of(subject: "Frames | MediaOp", lane: LanePolicy) -> "Media":
        match subject:
            case MediaOp() as op:
                return Media(op=op, lane=lane)
            case frames:
                return Media(op=MediaOp(encode_video=(frames, MediaProfile())), lane=lane)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the bare `ContentIdentity.key` folds the op's canonical byte stream PRE-RUN under the default
        # CANONICAL_POLICY; the muxed output's content address rides the `address` band fact.
        return ContentIdentity.key(f"media-{self.op.tag}", _canon(self.op))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # Inner Result[..., MediaFault] folds into the rail's boundary fault (Work[ArtifactReceipt] forbids a nested
        # Result); the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        railed = await async_boundary(f"media.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map_error(lambda fault: BoundaryFault(boundary=(f"media.{self.op.tag}", f"{fault.tag}:{fault}")))
        )

    async def _folded(self) -> Result[ArtifactReceipt, MediaFault]:
        return (await self._mux()).map(self._keyed)

    def _keyed(self, produced: Produced, /) -> ArtifactReceipt:
        blob, evidence = produced
        address = ContentIdentity.key(evidence.container.value, blob)
        return ArtifactReceipt.Media(
            self._key,
            evidence.container.value,
            evidence.codec,
            evidence.duration,
            evidence.byte_count,
            evidence.frame_count,
            evidence.bit_rate,
            evidence.facts | {"address": address.hex},
        )

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
        # HOSTILE routes the GIL-holding av worker onto the warm process pool, its trait row owning the worker-death
        # retry and the crossing converting the terminal raise; _lapsed folds the outer BoundaryFault onto the closed
        # vocabulary and the worker's inner rail flattens — no raise ever bridges the runtime rail and the media rail.
        # A segmented profile's io_open sink writes a manifest plus segment set — external state a worker-death replay must
        # never repeat — so idempotency derives structurally from the crossing's own profile argument, never a per-arm convention.
        replayable = not any(isinstance(a, MediaProfile) and a.container.segmented and a.segment is not None for a in args)
        outcome = await self.lane.offload(Kernel.of(worker, KernelTrait.HOSTILE, idempotent=replayable), *args)
        return outcome.map_error(_lapsed).bind(lambda inner: inner)
```

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable, Iterator
from fractions import Fraction
from functools import wraps
from itertools import chain, takewhile, zip_longest
from typing import TYPE_CHECKING, BinaryIO
from uuid import uuid4

from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result

from rasm.artifacts.scene.spec import CANON, framed  # parse-floor identity-preimage discipline; loads no native provider

# Workers stay module-level so the process lane dispatches them by qualified name; `av` is worker-scope only,
# and the audio worker and filtergraph builder defer through `lazy from` so neither couples the container import.
lazy import av
lazy import av.error
lazy import av.codec.hwaccel
lazy from upath import UPath  # the segmented-sink egress root; module-scope (a lazy stmt inside a function is a SyntaxError)
lazy from rasm.artifacts.media.audio import _encode_audio, _voiced
lazy from rasm.artifacts.media.filtergraph import wired

if TYPE_CHECKING:
    from rasm.artifacts.media.audio import Pcm
    from rasm.artifacts.media.filtergraph import FilterNode

# --- [OPERATIONS] -----------------------------------------------------------------------


def _worker[**P](operation: Callable[P, Result[Produced, MediaFault]], /) -> Callable[P, Result[Produced, MediaFault]]:
    guarded = beartype(operation)

    @wraps(operation)
    def call(*args: P.args, **kwargs: P.kwargs) -> Result[Produced, MediaFault]:
        try:
            return guarded(*args, **kwargs)
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=str(violation)))

    return call


def _canon(op: MediaOp) -> tuple[bytes, ...]:
    # PRE-RUN identity preimage: op tag, deterministic-msgpack profile bytes, then the raw payload bytes; a
    # FilterNode chunk rides its own `facet()` projection through the deterministic encoder, so equivalent graphs
    # share one key and behaviorally distinct graphs never collide — a repr carries no identity here.
    match op:
        case MediaOp(tag="encode_video", encode_video=(frames, profile)):
            return framed(b"encode_video", CANON.encode(profile), *(np.asarray(part).tobytes() for part in frames))
        case MediaOp(tag="encode_audio", encode_audio=(samples, profile)):
            return framed(b"encode_audio", CANON.encode(profile), *(np.asarray(part).tobytes() for part in samples))
        case MediaOp(tag="mux", mux=(frames, samples, video, audio)):
            return framed(b"mux", CANON.encode(video), CANON.encode(audio), *(np.asarray(part).tobytes() for part in (*frames, *samples)))
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
    # libav majors + ffmpeg build always ride evidence; encoding arms add applied profile policy, while packet-copy
    # arms pass None because codec/color/rate remain source facts rather than target encode policy.
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
    return name in av.codecs_available  # build-registry probe before add_stream, so a missing encoder rails `unregistered` not a deep raise


def _lift(profile: MediaProfile, array: object) -> object:
    # host rgb24 ndarray -> `from_ndarray`; a non-numpy DLPack device tensor -> `from_dlpack`. Discriminate on
    # `isinstance(np.ndarray)`, NOT `hasattr("__dlpack__")` — numpy arrays expose `__dlpack__` too and mis-route the
    # host path. Then reformat to `pix_fmt` and stamp the INTEGER AVCOL quad (the bitstream-VUI setattr).
    frame = (
        av.VideoFrame.from_ndarray(array, format=profile.frame_format)
        if isinstance(array, np.ndarray)
        else av.VideoFrame.from_dlpack(array, format=profile.frame_format)
    )
    return profile.colored(frame)


def _frame_flaw(array: object, extent: tuple[int, int], index: int, /) -> str | None:
    # every host ndarray frame revalidates against the head-derived extent — rank-3 (h, w, 3) uint8 — so a drifting
    # producer rails `invalid` instead of raising ValueError out of `from_ndarray` mid-encode; a DLPack device tensor
    # crosses to `_lift` untouched, its refusal converted by the callers' ValueError arm.
    match array:
        case np.ndarray() if array.ndim != 3 or array.shape != (*extent, 3) or array.dtype != np.uint8:
            return f"frame {index} must be uint8 ({extent[0]}, {extent[1]}, 3)"
        case _:
            return None


def _hwaccel(spec: "HwAccel | None") -> "av.codec.hwaccel.HWAccel | None":
    # probe the build's device list — `hwdevices_available()` is a CALL returning the name list — before minting the
    # context; None survives only for absent acceleration or a FALLBACK degrade, while REQUIRED refuses the silent
    # software path with the typed provider raise every caller's av arm converts to `unregistered` — the read paths
    # (`_frames`, `_decode_window`) and the encode/transcode arms all inherit this one gate.
    if spec is None:
        return None
    if spec.device_type not in av.codec.hwaccel.hwdevices_available():
        if spec.policy is HwPolicy.REQUIRED:
            raise av.error.DecoderNotFoundError(19, f"hwdevices_available: {spec.device_type}")
        return None
    return av.codec.hwaccel.HWAccel(device_type=spec.device_type, allow_software_fallback=spec.policy is HwPolicy.FALLBACK)


def _segment_sink(spec: SegmentSpec) -> tuple["Callable[[str, int], BinaryIO]", "Callable[[], tuple[bytes, int]]", "Callable[[], None]"]:
    # io_open is the per-segment callback av.open calls once per segment/manifest url; every write lands under a
    # run-scoped staging prefix on the same fsspec backend, so a mid-encode failure never exposes partial objects at
    # the public prefix. `publish` moves the validated set into place segments-first and manifest-LAST — readers key
    # on the manifest, so it never names a segment not yet in place — then reads the manifest back; `discard` removes
    # the staging prefix whole and no-ops after a publish, so one `finally` serves every failure path.
    root = UPath(spec.root, **dict(spec.storage_options))
    staging = root / f".staging-{uuid4().hex}"
    opened: list[str] = []

    def io_open(url: str, flags: int, /) -> BinaryIO:
        if url != spec.manifest:
            opened.append(url)
        target = staging / url
        target.parent.mkdir(parents=True, exist_ok=True)
        return target.open("wb")

    def publish() -> tuple[bytes, int]:
        for url in (*opened, spec.manifest):  # Exemption: the move walk is the publication seam; manifest-last is the ordering law
            (staging / url).rename(root / url)
        discard()
        return (root / spec.manifest).read_bytes(), len(opened)

    def discard() -> None:
        if staging.exists():
            staging.fs.rm(staging.path, recursive=True)

    return io_open, publish, discard


def _open_sink(profile: MediaProfile) -> tuple[object, "Callable[[], tuple[bytes, int]]", "Callable[[], None]"]:
    # one sink axis keyed by `container.segmented`: a single in-memory blob, or the STAGED io_open UPath segment set
    # whose publish is the read-back manifest bytes plus segment count; both arms stamp the profile's container tags,
    # and the blob arm's discard is the no-op its memory sink needs — every consumer runs publish-on-success,
    # discard-in-finally, one shape across the four encode/mux/transcode/remux arms.
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
        container.mux_one(packet)  # per-packet mux so an encoder emitting a burst back-pressures one packet at a time


def _flush(container: object, stream: object) -> None:
    for packet in stream.encode(None):
        container.mux_one(packet)


def _seek(reader: object, stream: object, seconds: float, /) -> None:
    # random-access primitive media/timeline#TIMELINE composes for Trim/Segment in-points: seek the demuxer to the
    # keyframe at/before the timestamp then flush the decoder so no stale reference frame bleeds across the cut.
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
    # an audio-only source has no best("video"); the absence raises the typed provider error the callers' av
    # except arms convert to MediaFault — never an AttributeError escaping outside the fault vocabulary.
    stream = reader.streams.best("video")
    if stream is None:
        raise av.error.InvalidDataError(22, "source carries no video stream")
    return stream


def _source_rate(blob: bytes) -> int:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        return round(float(_video_stream(reader).average_rate or 24))


def _frames(blob: bytes, accel: "HwAccel | None" = None) -> "Iterator[NDArray[np.uint8]]":
    # This generator owns its reader for the whole pull, so a long clip streams frame by frame to its consumer and
    # never materializes; the reader closes when the stream exhausts or the consumer drops it.
    with av.open(io.BytesIO(blob), mode="r", hwaccel=_hwaccel(accel)) as reader:
        for frame in reader.decode(_video_stream(reader)):
            yield frame.to_ndarray(format="rgb24")


def _decode_video(blob: bytes, accel: "HwAccel | None" = None) -> tuple[int, "Iterator[NDArray[np.uint8]]"]:
    # read-side inverse of _encode_video: (source frame rate, lazy rgb24 stream); the consumer materializes only the
    # working set it needs — a dissolve tail, a concat join — never the whole clip by default.
    return _source_rate(blob), _frames(blob, accel)


def _decode_window(blob: bytes, in_point: float, out_point: float, accel: "HwAccel | None" = None) -> tuple[int, Frames]:
    # _seek to the keyframe at/before the in-point, flush, decode only until the out-point (takewhile stops the pull),
    # and keep the frames whose presentation time (VideoFrame.time = pts·time_base) lands in [in_point, out_point) —
    # pre-roll decodes and drops, so the cut is frame-exact not GOP-aligned, and the eager tuple IS the working set.
    with av.open(io.BytesIO(blob), mode="r", hwaccel=_hwaccel(accel)) as reader:
        stream = _video_stream(reader)
        _seek(reader, stream, in_point)
        rolled = (frame for frame in reader.decode(stream) if frame.time is not None)
        window = takewhile(lambda frame: frame.time < out_point, rolled)
        kept = tuple(frame.to_ndarray(format="rgb24") for frame in window if frame.time >= in_point)
        return round(float(stream.average_rate or 24)), kept


def _roundtrip(blob: bytes, frames: Frames, /) -> str:
    # archival mezzanine evidence: a mathematically lossless profile round-trips bit-exact through its own decoder,
    # so the verdict is byte equality over every frame — streamed against the lazy decode, never a second whole-clip
    # buffer beside the source frames; a length mismatch surfaces as the None fill.
    _rate, decoded = _decode_video(blob)
    exact = all(
        out is not None and src is not None and np.array_equal(np.asarray(src), out)
        for src, out in zip_longest(frames, decoded, fillvalue=None)
    )
    return "verified" if exact else "diverged"


@_worker
def _encode_video(frames: "Frames | Iterable[NDArray[np.uint8]]", profile: MediaProfile) -> Result[Produced, MediaFault]:
    # ingress is ITERABLE: a synthesis generator or timeline stream encodes frame by frame in ONE pass — the eager
    # Frames tuple stays admitted as the already-materialized working set, and only the BYTE_EXACT verification
    # profile retains frames (its roundtrip compare re-reads them by definition).
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
            discard()  # any failure path — typed refusal, torn frame, provider raise — drops the staged set whole; a publish leaves nothing to drop
        # segmented output's blob IS the manifest (no single-blob video stream to re-probe), so evidence reads the source
        # frame count and the exact temporal extent (frames / rate); a single-blob encode re-opens the muxed bytes.
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
    except ValueError as exc:  # a `from_ndarray`/`from_dlpack` layout refusal the shape gate cannot pre-prove (av's own ValueError kinds route above)
        return Error(MediaFault(invalid=str(exc)))


# _mux_av alone is outside `_worker`: its `blocks` param is the audio-owned `Pcm` union, a TYPE_CHECKING-only forward ref
# that would NameError at call-time hint resolution; the mux arm's contract is proven statically by ty/mypy.
def _mux_av(frames: "Frames | Iterable[NDArray[np.uint8]]", blocks: "tuple[Pcm, ...]", video: MediaProfile, audio: MediaProfile) -> Result[Produced, MediaFault]:
    try:
        # video ingress is ITERABLE and drives in ONE pass (the timeline/synthesis stream never materializes here);
        # audio blocks stay the small eager working set `_voiced` folds.
        stream_in = iter(frames)
        head = next(stream_in, None)
        if head is None or not blocks:
            return Error(MediaFault(invalid="empty frame or sample sequence"))
        if any(block.dtype != blocks[0].dtype for block in blocks):
            return Error(MediaFault(invalid="one muxed pcm sequence must keep one producer dtype"))
        # the `_encode_audio` packed-shape law re-proven before any sink opens: `_voiced` assumes (1, samples*channels)
        # against the audio layout, and a malformed block must rail typed, never raise mid-mux.
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
                container.start_encoding()  # publishes the audio frame_size before `_voiced` builds the resampler
                for index, array in enumerate(chain((head,), stream_in)):
                    if (flaw := _frame_flaw(array, (height, width), index)) is not None:
                        return Error(MediaFault(invalid=flaw))
                    _drive(container, vstream, _lift(video, array), index, video.rate)
                    count_in = index + 1
                _voiced(container, astream, blocks, audio)  # shared audio drive: dtype-keyed lift, master/resample/reframe, cumulative-sample pts
                _flush(container, vstream)
                _flush(container, astream)
            blob, segments = publish()
        finally:
            discard()  # any failure path drops the staged set whole; a publish leaves nothing to drop
        duration, count, bit_rate, measured = (
            (count_in / video.rate, count_in, int(video.bit_rate or 0), frozendict({"width": float(width), "height": float(height), "frame_rate": float(video.rate), "pix_fmt": video.pix_fmt}))
            if video.container.segmented
            else _probe(blob)
        )
        facts = _deployment(video) | measured | {"sample_rate": float(audio.rate), "layout": audio.layout} | ({"segments": float(segments)} if video.container.segmented else {})
        return Ok((blob, MediaEvidence.measure(video.container, video.codec, duration, count, bit_rate, blob, facts)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("mux", exc))
    except ValueError as exc:  # an `AudioLayout`/`from_ndarray` refusal the shape gates cannot pre-prove (av's own ValueError kinds route above)
        return Error(MediaFault(invalid=str(exc)))


def _carried(packet: object, container: object, voice: object, carried: object, /) -> "Iterator[object]":
    # transcode's audio leg is a straight packet copy onto the cloned stream; video packets decode into the
    # filter chain, and the demuxer's terminal flush packet (dts None) drains the decoder without muxing.
    if carried is not None and packet.stream is voice:
        if packet.dts is not None:
            packet.stream = carried
            container.mux_one(packet)
        return iter(())
    return iter(packet.decode())


@_worker
def _transcode(source: bytes, profile: MediaProfile, nodes: "tuple[FilterNode, ...]") -> Result[Produced, MediaFault]:
    # decode -> filtergraph Graph -> encode -> mux over one owned read+write pair; the optional HwAccel context
    # accelerates decode, the encoder stream mints on the first shaped frame so a scale/crop node drives its config,
    # and the source's audio stream rides across as a packet copy so a transcode never silently drops the soundtrack.
    try:
        if not _codec_ok(profile.codec):
            return Error(MediaFault(unregistered=("codecs_available", profile.codec)))
        if profile.container.segmented != (profile.segment is not None):
            return Error(MediaFault(invalid="segmented containers require SegmentSpec and single-blob containers reject it"))
        with av.open(io.BytesIO(source), mode="r", hwaccel=_hwaccel(profile.hwaccel)) as reader:
            # reader opens FIRST: a malformed-source raise — or the `_hwaccel` REQUIRED refusal — must not strand an
            # already-opened, never-entered sink; the video-stream probe rails an audio-only source here for the
            # same reason, before any sink exists.
            src = _video_stream(reader)
            container, publish, discard = _open_sink(profile)
            try:
                with container:
                    if profile.codec not in container.supported_codecs:
                        return Error(MediaFault(unregistered=("supported_codecs", profile.codec)))
                    voice = next(iter(reader.streams.audio), None)
                    carried = container.add_stream_from_template(voice) if voice is not None else None
                    staged = wired(nodes, src) if nodes else None  # media/filtergraph#FILTER: ordered native/composite STAGES
                    selected = tuple(live for live in (src, voice) if live is not None)
                    decoded = (frame for packet in reader.demux(selected) for frame in _carried(packet, container, voice, carried))
                    stream: object | None = None
                    extent: tuple[int, int] | None = None
                    index = 0
                    for frame in chain(decoded, (None,) if staged is not None else ()):  # None flush drains every buffering stage's tail in order
                        # `WiredGraph.driven` walks the ordered stages, so a composite executes AT its program position.
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
                discard()  # any failure path — no-frame source included — drops the staged set whole; a publish leaves nothing to drop
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


@_worker
def _remux(source: bytes, profile: MediaProfile, bsf: str) -> Result[Produced, MediaFault]:
    # demux -> optional BitStreamFilterContext.filter (h264_mp4toannexb/…) -> mux copy over EVERY source stream
    # (video, audio, subtitles) via `add_stream_from_template`, never a re-decode — a remux is a container change,
    # so dropping a stream is corruption, not thrift. The bsf applies to the video streams alone and is probed
    # against `av.bitstream_filters_available`, else `unregistered`.
    try:
        if bsf and bsf not in av.bitstream_filters_available:
            return Error(MediaFault(unregistered=("bitstream_filters_available", bsf)))
        if profile.container.segmented != (profile.segment is not None):
            return Error(MediaFault(invalid="segmented containers require SegmentSpec and single-blob containers reject it"))
        with av.open(io.BytesIO(source), mode="r") as reader:
            # reader opens FIRST: a malformed-source raise must not strand an already-opened, never-entered sink.
            container, publish, discard = _open_sink(profile)
            try:
                with container:
                    copied = next((stream.codec_context.name for stream in reader.streams if stream.type in ("video", "audio")), "copy")
                    mapped = {src.index: container.add_stream_from_template(src) for src in reader.streams}
                    shaping = {src.index: av.BitStreamFilterContext(bsf, src, mapped[src.index]) for src in reader.streams.video} if bsf else {}
                    muxed, end = 0, 0.0  # delivered-output evidence accrues on the mux walk — a segmented sink admits no post-hoc probe
                    for packet in reader.demux():
                        if packet.dts is None:  # the demuxer's terminal flush packet carries no timestamp
                            continue
                        stamp = packet.dts if packet.pts is None else packet.pts
                        muxed += 1 if packet.stream.type == "video" else 0
                        end = max(end, float((stamp + (packet.duration or 0)) * packet.stream.time_base))
                        bitstream = shaping.get(packet.stream.index)
                        for shaped in bitstream.filter(packet) if bitstream is not None else (packet,):
                            shaped.stream = mapped[packet.stream.index]
                            container.mux_one(shaped)
                    for index, bitstream in shaping.items():  # filter(None) drains the buffered packets per filtered stream
                        for shaped in bitstream.filter(None):
                            shaped.stream = mapped[index]
                            container.mux_one(shaped)
                blob, segments = publish()
            finally:
                discard()  # any failure path drops the staged set whole; a publish leaves nothing to drop
        # segmented evidence derives from the mux walk over the DELIVERED packets — the published blob is a manifest a
        # duration probe cannot read, and probing the source would attest the input, never the artifact.
        duration, count, bit_rate, measured = (end, muxed, int(profile.bit_rate or 0), frozendict()) if profile.container.segmented else _probe(blob)
        facts = _deployment(None) | measured | ({"segments": float(segments)} if profile.container.segmented else {})
        return Ok((blob, MediaEvidence.measure(profile.container, copied, duration, count, bit_rate, blob, facts)))
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
