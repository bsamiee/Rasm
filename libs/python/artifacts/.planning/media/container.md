# [PY_ARTIFACTS_MEDIA_CONTAINER]

The container/codec SPINE of the seven-page media plane — the one `Media` owner and the shared `Media`/`MediaOp`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat`/`ColorProfile` family every media page composes, the mux/demux capsule, and the video worker. `Media` muxes a frame sequence into a container (MP4/WebM/MKV/GIF or the segmented HLS/DASH/fMP4/MPEG-TS sinks) over the FFmpeg floor `av` (PyAV, `17.1.0`) provides: `av.open(sink, mode="w", format=)` mints the `OutputContainer`, `OutputContainer.add_stream(codec_name, rate=, options=)` mints the typed `VideoStream` after `codec in av.codecs_available` PROBES the build for the encoder (fault `MediaFault.unregistered` on a miss, never a blind `add_stream` that raises deep in the worker), `VideoFrame.from_ndarray(array, format=profile.frame_format)` lifts each `scene/render#SCENE` rgb24 raster with no PNG intermediary (or `VideoFrame.from_dlpack(capsule)` when a producer hands a CUDA/torch DLPack device tensor, the zero-copy GPU ingest that skips the host round-trip), `VideoFrame.reformat(format=pix_fmt)` bridges it to the encoder pixel format (`yuv420p10le` for a 10-bit HDR grade), the `ColorProfile`-derived INTEGER `AVCOL` codes tag each frame's `color_primaries`/`color_trc`/`colorspace`/`color_range` (the real-HDR fix over the prior fence's `str`-set that raised `TypeError: an integer is required`), `stream.encode(frame)` returns the `Packet` list, and `OutputContainer.mux_one(packet)` writes each with per-packet back-pressure, the `stream.encode(None)` flush muxed at end-of-stream. `MediaOp` is ONE closed-payload `expression.tagged_union` over the media modalities — `EncodeVideo`/`EncodeAudio`/`Mux` plus the read-side `Transcode` (`av.open(mode="r", hwaccel=)` demux/`decode` -> `media/filtergraph#FILTER` `build_graph` -> encode -> mux) and `Remux` (`demux` -> `BitStreamFilterContext.filter` -> `mux` copy, no re-decode) arms — each case carrying its typed payload, dispatched by one total `match` returning `Result[(ContentKey, ArtifactReceipt), MediaFault]` keyed over the muxed container bytes (or, for a segmented encode, over the read-back manifest bytes). `MediaFault` is the closed av-boundary cause vocabulary the whole media rail threads (`unregistered`/`invalid`/`codec`/`provision`/`worker`/`contract`), the sibling of `graphic/vector#VECTOR` `VectorFault` and `graphic/raster/io#IO` `RasterFault`, so a codec/muxer/filter/bsf registration miss, a malformed stream, an exhausted subprocess death, and a contract violation are each structurally addressable rather than the message-collapsed `boundary` catch-all a bare `async_boundary(catch=Exception)` mints. `ContainerFormat` is the closed muxer `StrEnum` keyed inside the profile, its segmented members (`HLS`/`DASH`/`SEGMENT`/`MPEGTS`) routing the `io_open` per-segment sink to a parameterized `UPath` root (local or remote — `s3://`/`gs://` through the admitted fsspec backends), never a parallel per-format encoder. `MediaProfile` is the one frozen encode-policy value folding the `ContainerFormat` muxer, the `codec` name, the frame/sample `rate`, the target `bit_rate`, the `gop_size` keyframe interval, the `pix_fmt`/`frame_format` pixel pair, the `ColorProfile` HDR/color band, the codec-private `options` and muxer-private `container_options`, the channel `layout`, the `thread_count`, the optional `hwaccel` read-side hardware-decode spec, the optional `segment` segmented-sink spec, the optional `attachments`/`data` embed rows, and the audio-mastering `master` slot — into its bound `streamed`/`voiced`/`colored` projections, never loose constructor knobs the implementer re-derives per call. This page owns the shared media family plus the container/mux/demux capsule, the read-side `seek`+`flush_buffers` random-access primitive, the hardware-decode `HWAccel` probe, the segmented `io_open` sink, and the video worker; the AUDIO-stream encode arm (`AudioFrame`/`AudioResampler`/`Master`) composes this owner at `media/audio#MEDIA`, the capability-routed `FilterNode` graph at `media/filtergraph#FILTER`, and the NLE trim/concat/segment/xfade layer at `media/timeline#TIMELINE`. Every worker crossing runs over the `WORKER_BAND`-bounded `anyio.to_process.run_sync` lane wrapped in `stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)`, and the finished container/manifest bytes are the only payload crossing back. This is container/codec encode, not visualization; it owns no frame production — `scene/render` rasterizes the orbit sequence, `Media` only muxes it — and `Media.encode` IS the `core/plan#PLAN` `ArtifactWork.work` thunk, contributing one `core/receipt#RECEIPT` `ArtifactReceipt.Media` case whose `facts` band carries the HDR-color tag and the HLS/DASH segment count.

## [01]-[INDEX]

- [01]-[CONTAINER]: the one `Media` owner over the closed-payload `MediaOp` family — `EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux` folding into one `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]` keyed over the muxed container (or read-back manifest) bytes, the `ContainerFormat` muxer vocabulary (`MP4`/`WEBM`/`MKV`/`GIF` single-blob + `HLS`/`DASH`/`SEGMENT`/`MPEGTS` segmented) keyed inside the `MediaProfile` policy value, the `ColorProfile` closed HDR/color vocabulary deriving the INTEGER `AVCOL` code quad per frame through the `_COLOR_CODES` table, the `MediaProfile` encode-policy value with its bound `streamed`/`voiced` stream-configure projections and its `colored` per-frame HDR band folded into every arm and the shared `_drive` `pts`/`time_base` per-frame stamp, the `tuple[NDArray[np.uint8], ...]` frame seam ingested from `scene/render#SCENE` through `VideoFrame.from_ndarray(array, format=profile.frame_format)` (or `from_dlpack`/`from_numpy_buffer` the zero-copy device/contiguous rows) reformatted to `pix_fmt`, the segmented `_segment_sink` `io_open` `UPath` egress, the `_hwaccel` read-side `HWAccel` probe, the `_seek` random-access `seek`+`flush_buffers` primitive `media/timeline#TIMELINE` composes, and the `MediaEvidence` encode receipt the `_emit` arm spreads onto `ArtifactReceipt.Media` including the deployment/color/segment `facts` band; `av` `open` (`mode="r"`/`"w"`, `hwaccel=`, `io_open=`, `container_options=`)/`OutputContainer.add_stream`(`options=`)/`add_stream_from_template`/`add_attachment`/`add_data_stream`/`VideoStream.encode`/`OutputContainer.mux_one`/`start_encoding`, `InputContainer.demux`/`decode`/`seek`/`streams.best`, `CodecContext.flush_buffers`, `VideoFrame.from_ndarray`/`from_dlpack`/`reformat`/`color_primaries`/`color_trc`/`colorspace`/`color_range`, `BitStreamFilterContext.filter`, `av.codecs_available`/`av.bitstream_filters_available` and `av.filter.filters_available` the registry probes, `av.codec.hwaccel.HWAccel`/`hwdevices_available`/`Codec.hardware_configs`, `av.error.FFmpegError` and its `EncoderNotFoundError`/`DecoderNotFoundError`/`MuxerNotFoundError`/`DemuxerNotFoundError`/`FilterNotFoundError`/`BSFNotFoundError`/`InvalidDataError` leaves, and `av.library_versions`/`av.ffmpeg_version_info` READ once at worker scope into the receipt `facts` band — all verified present on the installed `av 17.1.0`. The shared `Media`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat`/`ColorProfile` family and the `_lift`/`_drive`/`_flush`/`_probe`/`_filtered`/`_seek`/`_hwaccel`/`_segment_sink`/`_deployment`/`_media_fault` worker primitives are this page's; the `EncodeAudio` arm and its `AudioResampler`/master worker compose this owner at `media/audio#MEDIA`, the capability-detection filter routing at `media/filtergraph#FILTER`, and the NLE layer at `media/timeline#TIMELINE`.

## [02]-[CONTAINER]

- Owner: `Media` the one container/codec encode owner discriminating modality over the closed `MediaOp` family; `MediaOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Media` subclass nor a parallel `encode_video`/`encode_audio`/`remux` function trio; `ContainerFormat` the closed `StrEnum` of muxers keyed inside the `MediaProfile` — the single-blob `MP4`/`WEBM`/`MKV`/`GIF` plus the segmented `HLS`/`DASH`/`SEGMENT`/`MPEGTS` — carrying the FFmpeg muxer name `av.open(format=)` reads and a `segmented` predicate the sink-open branches on, never a parallel per-container output owner; `ColorProfile` the closed HDR/color vocabulary (`SRGB`/`BT709`/`BT601`/`BT2020_PQ`/`BT2020_HLG`) whose `_COLOR_CODES` row is the `(primaries, trc, space, range)` INTEGER `AVCOL` quad the frame's `color_*` attributes require — the real-HDR fix, since the prior fence set the band from `str | None` fields and `frame.color_primaries = "bt2020"` raises `TypeError: an integer is required` while `frame.color_primaries = 9` is the verified settable path; `MediaProfile` the one frozen encode-policy value (the `ContainerFormat` muxer, the `codec` encoder name, the frame/sample `rate`, the target `bit_rate`, the `gop_size`, the `pix_fmt`/`frame_format` pixel pair, the `ColorProfile` color band, the codec-private `options` and muxer-private `container_options` maps, the `layout` channel layout, the `thread_count`, the optional `hwaccel: HwAccel | None` read-side hardware-decode spec, the optional `segment: SegmentSpec | None` segmented-sink spec, the optional `attachments: tuple[Attachment, ...]` embed rows, and the optional `master` audio-mastering slot) carrying its own `streamed` video-stream-configure projection (one `add_stream(options=)` plus the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`thread_count` codec-context fold and the `add_attachment` embeds), its `voiced` audio-stream-configure projection the audio arm reads, and its `colored` per-frame `AVCOL`-quad projection the `_lift` worker stamps, so a new encode knob is one `MediaProfile` field or `options`/`container_options` entry bound into an existing projection, never a constructor-parameter tail; `HwAccel` the read-side hardware-decode value (the `HWDeviceType` name + `allow_software_fallback`) `_hwaccel` probes against `av.codec.hwaccel.hwdevices_available` before minting `av.codec.hwaccel.HWAccel(...)` for `av.open(hwaccel=)`, never a blind GPU assumption; `SegmentSpec` the segmented-sink value (the manifest name, the `UPath` root url + `storage_options`, and the muxer `options`) `_segment_sink` reads to build the `io_open` per-segment callback; `Attachment` the `(name, mimetype, data)` embed row (a font for a later subtitle burn, a cover) `streamed` folds through `add_attachment`; `MediaFault` the closed av-boundary cause vocabulary (`unregistered`/`invalid`/`codec`/`provision`/`worker`/`contract`) the whole media rail threads, the sibling of `VectorFault`/`RasterFault`; `MediaEvidence` the typed encode receipt (container, codec, duration, byte count, frame count, bit rate, plus the deployment/color/segment `facts` band) the one `MediaEvidence.measure` constructor folds once over the muxed/manifest bytes, the read-back, and `_deployment`; `Result[(ContentKey, ArtifactReceipt), MediaFault]` the one carrier every arm returns, keyed over the finished bytes through `ContentIdentity.of(container.value, blob)` so an identical frame sequence at an identical profile is a cache hit by reference; the `av.open(sink, mode="w", io_open=)` `OutputContainer` (and the `Transcode`/`Remux` read-side `av.open(BytesIO(source), mode="r", hwaccel=)` `InputContainer`) is the one mux/demux capsule per op, always a context manager so the trailer is written and the IO/native handle released, never retained across arms.
- Cases: `MediaOp` cases — `EncodeVideo(frames, profile)` (the `tuple[NDArray[np.uint8], ...]` rgb24 sequence — or a DLPack device-tensor tuple — `scene/render#SCENE` hands across, lifted array-by-array through `_lift` discriminating `from_ndarray`/`from_dlpack`, the `MediaProfile.streamed` projection minting one `VideoStream` after the `av.codecs_available` codec probe and folding its codec-context knobs plus the `Attachment` embeds, the shared `_drive` loop stamping each frame's `pts`/`time_base`, the `stream.encode(frame)` -> `container.mux_one(packets)` loop, the `stream.encode(None)` flush; the sink is a single `BytesIO` blob or, when `profile.segment is not None`, the `io_open` `UPath`-rooted segment set whose manifest bytes are the keyed product and whose segment count rides the `facts` band) · `Mux(frames, samples, video, audio)` (the one interleave axis muxing a video frame sequence and an audio sample sequence into a single container — one `video.streamed` video stream plus one `audio.voiced` audio stream off the two profiles, the `pts`-ordered `mux` interleaving, the dual flush — never a parallel A/V-combine surface) · `EncodeAudio(samples, profile)` (the `tuple[Pcm, ...]` sample-block sequence the `media/audio#MEDIA` arm owns, dispatched here by the shared `MediaOp` discriminant and worked by the `media/audio#MEDIA` `_encode_audio` function) · `Transcode(source, profile, nodes)` (the read+write pair — `av.open(BytesIO(source), mode="r", hwaccel=_hwaccel(profile))` demux/`decode` off `streams.best("video")` -> the `media/filtergraph#FILTER` capability-routed `build_graph(nodes, template)` `av.filter.Graph` -> `reformat` to `pix_fmt` -> `_drive` encode/mux, the encoder stream minted on the first shaped frame so a scale/crop node's output geometry drives its config) · `Remux(source, profile, bsf)` (the quality-lossless container/codec change — `demux` -> `BitStreamFilterContext.filter` after the `av.bitstream_filters_available` bsf probe -> `mux_one` copy over an `add_stream_from_template` cloned stream, never a re-decode) — matched by one total `match`/`case`, the five-case modality recovered from the `MediaOp` discriminant, never a name suffix; the `EncodeVideo`/`Mux`/`Transcode`/`Remux` arms own the video worker on this page and the `EncodeAudio` arm dispatches to the same-folder audio worker. `media/filtergraph#FILTER` owns the closed `FilterNode` family and the native-vs-substitute routing; this page composes its `build_graph` product, never re-implementing a filter.
- Entry: `Media.encode` is `async` over the runtime `async_boundary`, returning `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]` — the domain `MediaFault` nested inside the boundary rail exactly as `graphic/raster/io#IO` nests `RuntimeRail[Block[Result[ArtifactReceipt, RasterFault]]]`, so `async_boundary` owns only a truly unexpected raise while every classified av cause stays structurally addressable on the inner rail, and the coroutine IS the `core/plan#PLAN` `ArtifactWork.work` thunk the planner schedules. `_emit` maps the `_mux` outcome through `_keyed` (deriving `ContentIdentity.of(container, blob)` and constructing the receipt case), and `_mux` dispatches the whole synchronous `av` body onto the runtime subprocess lane (`anyio.to_process.run_sync`, CPU-bound GIL-holding C) wrapped in `_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)` and catches the two seam raises the worker cannot rail as data — `BrokenWorkerProcess` -> `MediaFault(worker=...)` and `BeartypeCallHintViolation` -> `MediaFault(contract=...)`. Each `@beartype`-woven worker opens one `av.open(...)` container against a `BytesIO` sink (or the `io_open` `UPath` segment root), runs its arm, reads the muxed/manifest `bytes` plus the read-back `MediaEvidence`, and returns `Result[(blob, evidence), MediaFault]` — catching `ImportError` -> `provision` (the FFmpeg build absent) and `av.error.FFmpegError` -> the `_media_fault` leaf mapping at the arm that incurs it, each a module-level function dispatched by qualified name across the subprocess lane (`to_process.run_sync` cannot target a bound method or closure). The arm keys one `ContentKey` over the single blob; the inbound `scene/render` frame-set Merkle-parent `ContentKey` is the by-reference cache key the runtime lane elides on, the muxed-output key the encode product.
- Auto: frame ingest is the parameterized `_lift` — `from_dlpack(array)` when `hasattr(array, "__dlpack__")` (a CUDA/torch producer, zero host copy) else `VideoFrame.from_ndarray(array, format=profile.frame_format)` (the host rgb24 surface, no PNG decode), then `reformat(format=profile.pix_fmt)` bridges the producer format to the encoder pixel format (a bespoke rgb24->yuv420p pixel loop is the deleted form), and `profile.colored()` stamps the INTEGER `AVCOL` quad on the frame (`setattr(frame, "color_primaries", 9)`, never the `TypeError`-raising `str` set); `MediaProfile.streamed` mints `container.add_stream(profile.codec, rate=profile.rate, options=dict(profile.options))` only after `profile.codec in av.codecs_available` (else `MediaFault.unregistered`), folds the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`thread_count` and the `Attachment` embeds, dropping `None` knobs; the shared `_drive` loop stamps `frame.pts`/`frame.time_base` from the running frame index against `rate` for a monotonic timeline, folds `stream.encode(frame)` over the sequence muxing each `Packet` through `container.mux_one`, then `_flush` at end-of-stream; the sink is `_open_sink(profile)` — a `BytesIO` finalized by `getvalue()` (segment count `0`) or, for a segmented `ContainerFormat`, the `av.open(spec.manifest, format=, io_open=_segment_sink(spec))` `UPath`-rooted sink finalized by reading the manifest bytes back and counting the non-manifest segment opens; the `Transcode` arm drives the read-side `decode` -> `media/filtergraph#FILTER` `Graph` pull -> `reformat` -> `_drive` pipeline under the optional `_hwaccel(profile)` decode context and the `Remux` arm the `demux` -> `BitStreamFilterContext.filter` -> `mux_one` copy; the muxed/manifest `bytes` plus the read-back `_probe` frame-count/duration and the `_deployment` libav/ffmpeg/color facts fold through `MediaEvidence.measure` once.
- Receipt: each media op contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media` — the eight-slot case `(ContentKey, container, codec, duration, bytes, frames, bit_rate, facts)` — keyed by the content key `_keyed` derives over the muxed/manifest bytes; `_keyed` spreads `evidence.container`/`codec`/`duration`/`byte_count`/`frame_count`/`bit_rate` and `evidence.facts` onto the case (the receipt owner's `media` `_facts` arm flattens the band exactly as `preview` flattens `scores`), so the receipt owner imports no `MediaEvidence` value object nor any `av` container handle — the receipt-side cycle the flat-scalar-plus-band shape forecloses. `MediaEvidence.facts` carries the deployment evidence `_deployment` reads once off `av.library_versions` (the bundled `libavcodec`/`libavformat` majors) and `av.ffmpeg_version_info`, the `ColorProfile` HDR-color tag (`profile.color.value`), and the segment count a segmented encode adds after finalize. The `media/audio#MEDIA` `EncodeAudio` arm, the `media/filtergraph#FILTER` filter-node count, and the `media/timeline#TIMELINE` clip/segment counts all contribute onto this same single `ArtifactReceipt.Media` case through the shared band, never a parallel per-page receipt rail.
- Growth: a new container is one `ContainerFormat` row carrying its muxer name and its `segmented` predicate bit; a new codec is one `MediaProfile.codec` string the probed `add_stream` row reads (a hardware encoder `h264_videotoolbox`/`hevc_videotoolbox` is a codec row, not a knob — the read-side `HwAccel` is the decode counterpart); a new HDR/color band is one `ColorProfile` member plus one `_COLOR_CODES` row; a new encode knob (CRF, preset, tune) is one `MediaProfile.options` entry and a muxer knob (HLS `hls_time`, fMP4 `movflags`) one `container_options`/`SegmentSpec.options` entry; a new hardware device is one `HWDeviceType` name the `HwAccel` value carries; a new segmented protocol is one `ContainerFormat` segmented row plus its muxer `options`; a new av fault leaf is one `MediaFault` case plus one `_media_fault` arm; a new evidence fact is one `_deployment` band key with ZERO receipt edit (the `facts` band absorbs it exactly as the `preview` `scores` band does); zero new surface — the modality space is the five cases on one owner, every addition a row, field, case, or arm.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
import stamina
from anyio import BrokenWorkerProcess, to_process
from beartype.roar import BeartypeCallHintViolation
from builtins import frozendict
from expression import Error, Result, case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from artifacts.media.audio import Master, Pcm
    from artifacts.media.filtergraph import FilterNode

# --- [TYPES] ----------------------------------------------------------------------------

type Frames = tuple[NDArray[np.uint8], ...]      # host rgb24 rasters, or DLPack-exposing device tensors _lift discriminates
type Samples = tuple["Pcm", ...]
type MediaOpTag = Literal["encode_video", "encode_audio", "mux", "transcode", "remux"]
type MediaFaultTag = Literal["unregistered", "invalid", "codec", "provision", "worker", "contract"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FRAME_FORMAT = "rgb24"

# ColorProfile -> the (color_primaries, color_trc, colorspace, color_range) INTEGER AVCOL quad each frame attribute
# requires; `frame.color_primaries = 9` is the verified settable path, `= "bt2020"` raises `an integer is required`.
_COLOR_CODES: frozendict[str, tuple[int, int, int, int]] = frozendict({
    "srgb": (1, 13, 1, 2),          # bt709 primaries, IEC61966-2-1 sRGB trc, bt709 matrix, full range
    "bt709": (1, 1, 1, 1),          # HD SDR: bt709 primaries/trc/matrix, tv range
    "bt601": (6, 6, 6, 1),          # SD SDR: smpte170m
    "bt2020_pq": (9, 16, 9, 1),     # HDR10: bt2020 primaries, SMPTE2084 PQ trc, bt2020-ncl matrix
    "bt2020_hlg": (9, 18, 9, 1),    # HLG: bt2020 primaries, ARIB-STD-B67 HLG trc, bt2020-ncl matrix
})

# retry the transient subprocess-death seam only, mirroring graphic/raster/io#IO: a BrokenWorkerProcess OOM/signal
# death recovers before the slot faults; a BeartypeCallHintViolation is NOT in the `.on(...)` set — it lifts to `contract`.
_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)


class ContainerFormat(StrEnum):
    MP4 = "mp4"
    WEBM = "webm"
    MKV = "matroska"
    GIF = "gif"
    HLS = "hls"                      # segmented: io_open sinks the .m3u8 manifest + fMP4/TS segments to the UPath root
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
    BT2020_PQ = "bt2020_pq"         # HDR10
    BT2020_HLG = "bt2020_hlg"

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class MediaFault:
    # the closed av-boundary cause vocabulary the whole media rail threads, the sibling of graphic/vector#VECTOR
    # `VectorFault` and graphic/raster/io#IO `RasterFault`: each case is structurally addressable so a downstream
    # `match` routes a registration miss apart from a malformed stream apart from a worker death apart from a contract miss.
    tag: MediaFaultTag = tag()
    unregistered: tuple[str, str] = case()  # a probed codec/muxer/filter/bsf absent from the build — (registry, name)
    invalid: str = case()                   # av InvalidDataError — malformed frame/stream data
    codec: tuple[str, str] = case()         # any other av FFmpegError during encode/mux/transcode/remux — (op, strerror)
    provision: str = case()                 # the bundled FFmpeg build is absent (av ImportError at the worker seam)
    worker: str = case()                    # an exhausted BrokenWorkerProcess subprocess death past `_WORKER_RETRY`
    contract: str = case()                  # a BeartypeCallHintViolation lifted at the worker seam


class HwAccel(Struct, frozen=True):
    # read-side hardware-decode spec `_hwaccel` probes against `av.codec.hwaccel.hwdevices_available` before minting
    # `HWAccel(device_type=, allow_software_fallback=)` for `av.open(mode="r", hwaccel=)`; a missing device degrades
    # to software decode when `allow_software_fallback`, never a hard crash on a build without the GPU backend.
    device_type: str = "videotoolbox"       # HWDeviceType name (videotoolbox/cuda/vaapi/qsv/d3d11va)
    allow_software_fallback: bool = True


class Attachment(Struct, frozen=True):
    name: str
    mimetype: str
    data: bytes                             # a font for a later subtitle burn, a cover image — `add_attachment` embed


class SegmentSpec(Struct, frozen=True):
    # the segmented-sink policy: `_segment_sink` builds the `io_open(url, flags)` callback opening `UPath(root)/url`
    # so HLS/DASH/fMP4 segments land locally or on s3://,gs:// through the admitted fsspec backends; `manifest` is the
    # top-level playlist name av.open writes, `options` the muxer knobs (hls_time/hls_segment_type/seg_duration).
    root: str                               # a UPath url — file:///…, s3://bucket/prefix, gs://…
    manifest: str = "index.m3u8"
    options: frozendict[str, str] = frozendict()
    storage_options: frozendict[str, str] = frozendict()


class MediaProfile(Struct, frozen=True):
    container: ContainerFormat = ContainerFormat.MP4
    codec: str = "libx264"                  # the encoder name; a hardware encoder (h264_videotoolbox/hevc_videotoolbox) is a codec row
    rate: int = 24
    bit_rate: int | None = None
    gop_size: int | None = None
    pix_fmt: str = "yuv420p"                # the encoder pixel format the frame reformats to (yuv420p10le for a 10-bit HDR grade)
    frame_format: str = _FRAME_FORMAT       # the producer ndarray pixel format `from_ndarray` ingests; reformat bridges it to `pix_fmt`
    layout: str = "stereo"
    thread_count: int = 0
    color: ColorProfile = ColorProfile.SRGB              # the HDR/color band; BT2020_PQ over yuv420p10le is a real HDR10 grade
    options: frozendict[str, str] = frozendict()         # codec-private knobs (crf/preset/tune) -> add_stream(options=)
    container_options: frozendict[str, str] = frozendict()  # muxer-private knobs (movflags=frag_keyframe+empty_moov for fMP4)
    hwaccel: "HwAccel | None" = None                     # read-side hardware decode (Transcode/random-access); None = software
    segment: "SegmentSpec | None" = None                 # segmented io_open sink; None = one BytesIO blob
    attachments: tuple[Attachment, ...] = ()             # add_attachment embeds (fonts/covers)
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
        # the per-frame INTEGER AVCOL quad the worker sets after the pix_fmt reformat; the values are FFmpeg enum
        # codes (`frame.color_primaries = 9`), never the strings the prior fence set that raised `an integer is required`.
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
    def measure(container: ContainerFormat, codec: str, duration: float, frames: int, bit_rate: int, blob: bytes, facts: frozendict[str, float | str] = frozendict()) -> "MediaEvidence":
        return MediaEvidence(container, codec, duration, len(blob), frames, bit_rate, facts)


@tagged_union(frozen=True)
class MediaOp:
    tag: MediaOpTag = tag()
    encode_video: tuple[Frames, MediaProfile] = case()
    encode_audio: tuple[Samples, MediaProfile] = case()
    mux: tuple[Frames, Samples, MediaProfile, MediaProfile] = case()
    transcode: tuple[bytes, MediaProfile, "tuple[FilterNode, ...]"] = case()  # source bytes -> decode -> filtergraph Graph -> encode -> mux
    remux: tuple[bytes, MediaProfile, str] = case()                           # source bytes -> demux -> BSF -> mux copy, no re-decode

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

    async def encode(self) -> RuntimeRail[Result[tuple[ContentKey, ArtifactReceipt], MediaFault]]:
        # the domain MediaFault rides inside the boundary rail exactly as graphic/raster/io#IO nests
        # `RuntimeRail[Block[Result[ArtifactReceipt, RasterFault]]]`; this coroutine IS the core/plan#PLAN
        # `ArtifactWork.work` thunk the pipeline schedules, keyed by the muxed-output ContentKey.
        return await async_boundary(f"media.{self.op.tag}", self._emit)

    async def _emit(self) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
        return (await self._mux()).map(self._keyed)

    def _keyed(self, produced: tuple[bytes, MediaEvidence], /) -> tuple[ContentKey, ArtifactReceipt]:
        blob, evidence = produced
        key = ContentIdentity.of(evidence.container.value, blob)
        return key, ArtifactReceipt.Media(key, evidence.container.value, evidence.codec, evidence.duration, evidence.byte_count, evidence.frame_count, evidence.bit_rate, evidence.facts)

    async def _mux(self) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
        try:
            match self.op:
                case MediaOp(tag="encode_video", encode_video=(frames, profile)):
                    return await _WORKER_RETRY(to_process.run_sync, _encode_video, frames, profile, limiter=WORKER_BAND)
                case MediaOp(tag="encode_audio", encode_audio=(samples, profile)):
                    return await _WORKER_RETRY(to_process.run_sync, _encode_audio, samples, profile, limiter=WORKER_BAND)
                case MediaOp(tag="mux", mux=(frames, samples, video, audio)):
                    return await _WORKER_RETRY(to_process.run_sync, _mux_av, frames, samples, video, audio, limiter=WORKER_BAND)
                case MediaOp(tag="transcode", transcode=(source, profile, nodes)):
                    return await _WORKER_RETRY(to_process.run_sync, _transcode, source, profile, nodes, limiter=WORKER_BAND)
                case MediaOp(tag="remux", remux=(source, profile, bsf)):
                    return await _WORKER_RETRY(to_process.run_sync, _remux, source, profile, bsf, limiter=WORKER_BAND)
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
from builtins import frozendict
from expression import Error, Ok, Result

# same `artifacts.media.container` module as the owner fence; the workers stay module-level so `to_process.run_sync`
# dispatches them by qualified name, and the shared family resolves from the owner above. `av` is worker-scope only;
# the audio worker and the filtergraph builder defer through `lazy from` so neither couples the container import.
lazy import av
lazy import av.error
lazy import av.codec.hwaccel
lazy from upath import UPath                                  # the segmented-sink egress root; module-scope (a lazy stmt inside a function is a SyntaxError)
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
        case (av.error.EncoderNotFoundError() | av.error.DecoderNotFoundError() | av.error.MuxerNotFoundError()
              | av.error.DemuxerNotFoundError() | av.error.FilterNotFoundError() | av.error.BSFNotFoundError()):
            return MediaFault(unregistered=(type(exc).__name__, str(exc)))
        case _:
            return MediaFault(codec=(op, str(exc)))


def _deployment(profile: MediaProfile) -> frozendict[str, float | str]:
    # the deployment facts ride the receipt band: the bundled libav majors + the ffmpeg build read once off the live
    # linkage, plus the ColorProfile HDR-color tag; a segmented encode merges its segment count in at finalize.
    versions = av.library_versions
    return frozendict({
        "libavcodec": float(versions["libavcodec"][0]),
        "libavformat": float(versions["libavformat"][0]),
        "ffmpeg": av.ffmpeg_version_info,
        "color": profile.color.value,
    })


def _codec_ok(name: str, /) -> bool:
    return name in av.codecs_available  # the 540-set build probe before add_stream, so a missing encoder rails `unregistered` not a deep raise


def _lift(profile: MediaProfile, array: object) -> object:
    # parameterized ingress: a host rgb24 ndarray goes through `from_ndarray`; a NON-numpy DLPack device tensor (a
    # torch/cupy CUDA producer, no host round-trip) through `from_dlpack`. The discriminant is `isinstance(np.ndarray)`,
    # NOT `hasattr("__dlpack__")` — numpy arrays expose `__dlpack__` too, so the hasattr probe would mis-route the host
    # path. Then reformat to the encoder `pix_fmt` and stamp the INTEGER AVCOL color quad (the bitstream-VUI setattr).
    frame = av.VideoFrame.from_ndarray(array, format=profile.frame_format) if isinstance(array, np.ndarray) else av.VideoFrame.from_dlpack(array, format=profile.frame_format)
    converted = frame.reformat(format=profile.pix_fmt) if profile.pix_fmt != profile.frame_format else frame
    for attr, code in profile.colored().items():
        setattr(converted, attr, code)
    return converted


def _hwaccel(profile: MediaProfile) -> "av.codec.hwaccel.HWAccel | None":
    # read-side hardware decode: probe the build's device list before minting the context, so a machine without the GPU
    # backend degrades to software (allow_software_fallback) rather than crashing at `av.open(hwaccel=)`.
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
    # ONE sink axis keyed by `container.segmented`: a single in-memory blob, or the io_open UPath segment set whose
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
        except (BlockingIOError, EOFError):
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
    for frame in reader.decode(stream):  # lazy per-frame so a long clip never materializes every frame at once (LAZY_COMBINATORS)
        yield frame.to_ndarray(format="rgb24")


def _decode_video(blob: bytes) -> tuple[Frames, int]:
    # the read-side inverse of _encode_video the media/timeline#TIMELINE and media/analysis pages compose: demux+decode
    # one video stream to rgb24 blocks + the source frame rate (VideoStream.average_rate rounded to int).
    with av.open(io.BytesIO(blob), mode="r") as reader:
        stream = reader.streams.best("video")
        return tuple(_decoded(reader, stream)), round(float(stream.average_rate or 24))


def _decode_window(blob: bytes, in_point: float, out_point: float) -> tuple[Frames, int]:
    # frame-accurate trim read the timeline Trim composes: _seek to the keyframe at/before the in-point, flush, then
    # keep only frames whose presentation time (VideoFrame.time = pts·time_base) lands in [in_point, out_point) — the
    # pre-roll from the keyframe to the in-point decoded and dropped, so the cut is frame-exact not GOP-aligned.
    with av.open(io.BytesIO(blob), mode="r") as reader:
        stream = reader.streams.best("video")
        _seek(reader, stream, in_point)
        kept = tuple(
            frame.to_ndarray(format="rgb24")
            for frame in reader.decode(stream)
            if frame.time is not None and in_point <= frame.time < out_point
        )
        return kept, round(float(stream.average_rate or 24))


@beartype
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
        duration, count, bit_rate = (len(frames) / profile.rate, len(frames), int(profile.bit_rate or 0)) if profile.container.segmented else _probe(blob)
        facts = _deployment(profile) | {"segments": float(segments)} if profile.container.segmented else _deployment(profile)
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, facts)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("encode_video", exc))


# _mux_av alone is un-`@beartype`d: its `blocks` param is the audio-owned `Pcm` union (a TYPE_CHECKING-only forward ref
# that never enters the runtime namespace, so a call-time hint resolution would NameError); the mux arm's contract is
# proven by ty/mypy statically while the three video-only workers carry the runtime @beartype -> MediaFault.contract weave.
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
    # decode -> capability-routed filtergraph Graph -> encode -> mux over one owned read+write pair; media/filtergraph#FILTER
    # `build_graph` selects each node's native-or-substitute arm against `av.filter.filters_available`, so this arm never
    # re-implements a filter. The optional HwAccel context accelerates decode; the encoder stream mints on the first shaped frame.
    try:
        if not _codec_ok(profile.codec):
            return Error(MediaFault(unregistered=("codecs_available", profile.codec)))
        sink = io.BytesIO()
        with av.open(io.BytesIO(source), mode="r", hwaccel=_hwaccel(profile)) as reader, av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as container:
            src = reader.streams.best("video")
            wired = build_graph(nodes, src) if nodes else None  # media/filtergraph#FILTER: native/substitute-routed graph + numpy composites
            graph = wired.graph if wired is not None else None
            stream: object | None = None
            index = 0
            for frame in chain(reader.decode(src), (None,) if graph is not None else ()):  # None flush drains a buffering filter's tail
                for pulled in _filtered(graph, frame):
                    shaped = wired.composited(pulled) if wired is not None else pulled  # apply the text/subtitle numpy substitutes after the graph pull
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
    # container/codec change without quality loss: demux -> BitStreamFilterContext.filter (h264_mp4toannexb/…) -> mux copy,
    # never a re-decode; `add_stream_from_template` clones the source parameters and `mux_one` copies each packet. The bsf
    # name is probed against `av.bitstream_filters_available` (the 49-set) so an unregistered filter rails `unregistered`.
    try:
        if bsf and bsf not in av.bitstream_filters_available:
            return Error(MediaFault(unregistered=("bitstream_filters_available", bsf)))
        sink = io.BytesIO()
        with av.open(io.BytesIO(source), mode="r") as reader, av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as container:
            src = reader.streams.best("video")
            out = container.add_stream_from_template(src)
            bitstream = av.BitStreamFilterContext(bsf, src, out) if bsf else None
            for packet in reader.demux(src):
                if packet.dts is None:  # the demuxer's terminal flush packet carries no timestamp
                    continue
                for shaped in (bitstream.filter(packet) if bitstream is not None else (packet,)):
                    shaped.stream = out
                    container.mux_one(shaped)
            for shaped in (bitstream.filter(None) if bitstream is not None else ()):  # filter(None) drains the buffered packets
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

- [SEGMENTED_SINK] [RESOLVED]: HLS/DASH/fMP4/MPEG-TS segmented output rides `av.open(spec.manifest, mode="w", format="hls", options=, io_open=callback)` — verified on `av 17.1.0`: an fMP4 HLS open with `{"hls_segment_type": "fmp4", "hls_time": "1"}` drives the `io_open(url, flags)` callback once per url, opening `init.mp4`, the `.m4s` segments, and the `.m3u8.tmp` manifest, so the `_segment_sink` `UPath`-rooted callback lands each on the fsspec backend (local `file://` or remote `s3://`/`gs://` through the admitted `s3fs`/`gcsfs`). `_open_sink` is the ONE sink axis keyed by `ContainerFormat.segmented`, so a single-blob and a segmented encode share `_lift`/`_drive`/`_flush` and differ only at open and finalize — the segmented finalize reads the manifest bytes back for the `ContentKey` and counts the non-manifest opens for the `facts` band. The `UPath` root pickles across the `to_process` seam as a path value plus `storage_options`, so the worker reconstructs the fsspec backend inside the subprocess; `universal-pathlib`/`fsspec`/`s3fs`/`gcsfs` are admitted. The prior fence wrote only one `BytesIO` sink — segmented HLS/DASH was the campaign's `container.md` terminal.
- [HARDWARE_DECODE] [RESOLVED]: the read-side hardware decode is `av.open(source, mode="r", hwaccel=av.codec.hwaccel.HWAccel(device_type="videotoolbox", allow_software_fallback=True))` — verified: `av.codec.hwaccel.HWAccel(device_type=, allow_software_fallback=)` constructs, `av.codec.hwaccel.hwdevices_available` is the runtime device-list probe, and `Codec("hevc_videotoolbox", "w").hardware_configs` enumerates one `HWConfig(device_type=videotoolbox, format=videotoolbox_vld, is_supported=…)`. `_hwaccel` probes `hwdevices_available` before minting the context so a build without the GPU backend degrades to software (`allow_software_fallback`) rather than raising at `av.open`. `av.HWAccel` is NOT a top-level member — the constructor is `av.codec.hwaccel.HWAccel`, the exact path the prior fence's [16] deferral named without spelling. Hardware ENCODE stays a codec row (`h264_videotoolbox`/`hevc_videotoolbox`, both in `av.codecs_available`); the `HWAccel` context is the decode counterpart the `Transcode` arm composes.
- [HDR_COLOR] [RESOLVED]: real HDR is `reformat(format="yuv420p10le")` (verified: rgb24 -> yuv420p10le converts) plus the INTEGER `AVCOL` color quad on the frame. The verification exposed the prior fence's latent DEFECT: `frame.color_primaries = "bt2020-ncl"` raises `TypeError: an integer is required`, and `reformat(color_range="tv")` raises `unexpected keyword argument`, and `reformat(dst_colorspace="bt2020nc")` raises `KeyError` (the `reformat` `Colorspace` vocabulary is the limited `ITU601`/`ITU709`/`SMPTE240M`/`FCC` set, NOT bt2020) — so the reading-map "reformat color kwargs for real HDR" was an overclaim. The verified path is the frame-attribute set with FFmpeg enum CODES (`frame.color_primaries = 9`, `color_trc = 16` for SMPTE2084 PQ, `colorspace = 9` for bt2020-ncl, `color_range = 1`), which `frame.color_primaries = 9` confirms settable. `ColorProfile` is therefore a closed vocabulary over the `_COLOR_CODES` `(primaries, trc, space, range)` int quad, replacing the four nullable `color_*: str | None` fields the prior fence carried, and `colored()` derives the int-keyed band the worker `setattr`s — the capability extension (a real HDR10/HLG grade) grounded in the verified defect.
- [REGISTRY_PROBE] [RESOLVED]: the capability-detection premise is verified on this wheel build — `av.codecs_available` is a 540-member set, `av.bitstream_filters_available` a 49-member set, and `av.filter.filters_available` a 448-member set (the last undocumented in the folder `av.md` but confirmed present). `_codec_ok(name)` probes the codec set before `add_stream` so an encoder absent from the linked FFmpeg rails `MediaFault.unregistered=("codecs_available", name)` at the boundary rather than raising `EncoderNotFoundError` deep in the worker, and `_remux` probes `bitstream_filters_available` before minting the `BitStreamFilterContext`; the `media/filtergraph#FILTER` owner runs the same probe over `filters_available` to route native-vs-substitute. `h264_videotoolbox`/`hevc_videotoolbox`/`libx264`/`libsvtav1` all verify present.
- [RANDOM_ACCESS] [RESOLVED]: the `_seek` primitive is `reader.seek(int(seconds / stream.time_base), backward=True, stream=stream)` then `stream.codec_context.flush_buffers()` — verified: `InputContainer.seek` is present and `CodecContext.flush_buffers` (NOT an `InputContainer` method — the flush lives on the codec context per `av.md` row [14]) resets the decoder so no stale reference frame bleeds across a cut. This is the primitive `media/timeline#TIMELINE` composes for its `Trim`/`Segment` in-points (seek to the keyframe at/before the in-point, flush, then decode-and-keep from the exact frame), owned on the spine because it is the read-side counterpart of the mux/demux capsule.
- [DLPACK_INGEST] [RESOLVED]: `VideoFrame.from_dlpack` is verified present; `_lift` discriminates on `hasattr(array, "__dlpack__")` so a CUDA/torch/cupy device tensor ingests zero-copy (no host round-trip) while a host rgb24 ndarray takes `from_ndarray` — one ingest fold, two source shapes, the device edge the `scene/render` GPU producer reaches without a `.cpu()` copy. The `add_attachment`/`add_data_stream` embed surface is verified present and folds through `MediaProfile.attachments` in `streamed` (a font embedded for a later `media/subtitle` burn-in, a cover image), one `Attachment` row per embed rather than a bespoke attachment writer.
- [ABSORB_VIDEO] [RESOLVED]: this page absorbs `media/video.md` — the five-arm `MediaOp` owner (`EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux`), the `_encode_video`/`_mux_av`/`_transcode`/`_remux` workers, the `MediaFault`/`_media_fault` fault rail, the `_drive`/`_flush`/`_probe`/`_deployment`/`_filtered` primitives, and the `MediaProfile`/`MediaEvidence`/`ContainerFormat` family moved intact and widened in place — after which `media/video.md` is deleted so no duplicate owner remains. `media/audio#MEDIA` rebinds its `from artifacts.media.video import …` to `from artifacts.media.container import …` and its `media/video#MEDIA` anchor to `media/container#CONTAINER`; the `Transcode` arm's `filters: tuple[FilterStage, ...]` `(name, args)` tuple is superseded by `nodes: tuple[FilterNode, ...]` routed through `media/filtergraph#FILTER` `build_graph`, the campaign's split of the filter routing off the spine.
