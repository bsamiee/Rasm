# [PY_ARTIFACTS_MEDIA_VIDEO]

The temporal-artifact container/codec VIDEO encode/mux/transcode owner — the lead encoder and the primary `Media` owner. `Media` muxes a frame sequence into MP4/WebM/GIF/MKV over the FFmpeg floor `av` (PyAV) provides: `av.open(sink, mode="w", format=)` mints the `OutputContainer`, `OutputContainer.add_stream(codec_name, rate=, options=)` mints the typed `VideoStream`, `VideoFrame.from_ndarray(array, format=profile.frame_format)` lifts each `scene/render#SCENE` raster array directly with no PNG intermediary and `VideoFrame.reformat(format=pix_fmt)` bridges it to the encoder pixel format, `stream.encode(frame)` returns the `Packet` list, and `OutputContainer.mux_one(packet)` writes each with per-packet back-pressure, the `stream.encode(None)` flush muxed at end-of-stream. `MediaOp` is ONE closed-payload `expression.tagged_union` over the media modalities — `EncodeVideo`/`EncodeAudio`/`Mux` plus the read-side `Transcode` (`av.open(mode="r")` demux/`decode` -> `av.filter.Graph` -> encode -> mux) and `Remux` (`demux` -> `BitStreamFilterContext.filter` -> `mux` copy, no re-decode) arms — each case carrying its typed payload, dispatched by one total `match` returning `Result[(ContentKey, ArtifactReceipt), MediaFault]` keyed over the muxed container bytes. `MediaFault` is the closed av-boundary cause vocabulary the rail threads (`unregistered`/`invalid`/`codec`/`provision`/`worker`/`contract`), the sibling of `graphic/vector#VECTOR` `VectorFault` and `graphic/raster/io#IO` `RasterFault`, so a codec/muxer registration miss, a malformed stream, an exhausted subprocess death, and a contract violation are each structurally addressable rather than the message-collapsed `boundary` catch-all a bare `async_boundary(catch=Exception)` mints. `ContainerFormat` is the closed muxer `StrEnum` keyed inside the profile, never a parallel per-format encoder. `MediaProfile` is the one frozen encode-policy value — the `ContainerFormat` muxer, the `codec` name, the frame/sample `rate`, the target `bit_rate`, the `gop_size` keyframe interval, the `pix_fmt`/`frame_format` pixel pair, the `color_primaries`/`color_trc`/`colorspace`/`color_range` HDR/color band, the codec-private `options` (crf/preset/tune) and muxer-private `container_options` (fMP4 `movflags`), the channel `layout`, the `thread_count`, and the audio-mastering `master` slot — folded into the stream-configure through its bound `streamed`/`voiced` projections, into the per-frame color band through `colored`, and into the per-frame timeline through the shared `_drive` `pts`/`time_base` stamp, never loose constructor knobs the implementer re-derives per call. This page owns the shared `Media`/`MediaOp`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat` family plus the container/mux/demux capsule and the video worker; the AUDIO-stream encode arm (`AudioFrame`/`AudioResampler`/`Master`) composes this owner at `media/audio#MEDIA`. Every worker crossing runs over the `WORKER_BAND`-bounded `anyio.to_process.run_sync` lane wrapped in `stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)`, and the finished container bytes are the only payload crossing back. This is container/codec encode, not visualization; it owns no frame production — `scene/render` rasterizes the orbit sequence, `Media` only muxes it — and contributes `ArtifactReceipt.Media` with the bundled-libav/ffmpeg deployment facts and HDR/color band on the receipt's `facts` slot. This page closes the VIDEO half of the `MEDIA` idea; the campaign rebuilds it into `media/container#CONTAINER` (container spine, `io_open` HLS/DASH/fMP4 segmented sinks, hardware `av.open(hwaccel=)` GPU decode) and splits the filter routing into `media/filtergraph#FILTER`, absorbing this page then.

## [01]-[INDEX]

- [01]-[MEDIA]: the one `Media` owner over the closed-payload `MediaOp` family — `EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux` folding into one `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]` keyed over the muxed container bytes, the `ContainerFormat` muxer vocabulary keyed inside the `MediaProfile` policy value, the `MediaProfile` encode-policy value with its bound `streamed`/`voiced` stream-configure projections and its `colored` per-frame HDR/color band folded into every arm and the shared `_drive` `pts`/`time_base` per-frame stamp, the `tuple[NDArray[np.uint8], ...]` frame seam ingested from `scene/render#SCENE` through `VideoFrame.from_ndarray(array, format=profile.frame_format)` reformatted to `pix_fmt` with zero PNG round-trip (`VideoFrame.from_dlpack`/`from_numpy_buffer` the zero-copy GPU/torch and contiguous-buffer ingest rows when a producer holds a CUDA tensor rather than a host array), the `MediaEvidence` encode receipt the `_emit` arm spreads onto `ArtifactReceipt.Media` including the deployment/color `facts` band; `av` `open` (`mode="r"`/`"w"`, `container_options=`)/`OutputContainer.add_stream`(`options=`)/`add_stream_from_template`/`VideoStream.encode`/`OutputContainer.mux_one`/`start_encoding`, `InputContainer.demux`/`decode`/`streams.best`, `VideoFrame.from_ndarray`/`reformat`/`color_primaries`/`color_trc`/`colorspace`/`color_range`, `av.filter.Graph`(`add_buffer`/`add`/`link_nodes`/`configure`/`push`/`pull`), `BitStreamFilterContext.filter`, `av.error.FFmpegError` and its `EncoderNotFoundError`/`DecoderNotFoundError`/`MuxerNotFoundError`/`DemuxerNotFoundError`/`FilterNotFoundError`/`BSFNotFoundError`/`InvalidDataError` leaves, and `av.library_versions`/`av.ffmpeg_version_info` READ once at worker scope into the receipt `facts` band as deployment evidence — all settled against the folder `.api`; the `InputContainer` read-back frame-count/duration probe carried as the muxed-output evidence read. The shared `Media`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat` family and the `_lift`/`_drive`/`_flush`/`_probe`/`_graph`/`_filtered`/`_deployment`/`_media_fault` worker primitives are this page's; the `EncodeAudio` arm and its `AudioResampler`/master worker compose this owner at `media/audio#MEDIA`, and the `Mux` arm composes that owner's shared `_voiced` audio-drive primitive.

## [02]-[MEDIA]

- Owner: `Media` the one container/codec encode owner discriminating modality over the closed `MediaOp` family; `MediaOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Media` subclass nor a parallel `encode_video`/`encode_audio`/`remux` function trio; `ContainerFormat` the closed `StrEnum` of muxers keyed inside the `MediaProfile` — `MP4`/`WEBM`/`GIF`/`MKV` — carrying the FFmpeg muxer name `av.open(format=)` reads and the file extension, never a parallel per-container output owner; `MediaProfile` the one frozen encode-policy value (the `ContainerFormat` muxer, the `codec` encoder name, the frame/sample `rate`, the target `bit_rate`, the `gop_size` keyframe interval, the `pix_fmt`/`frame_format` pixel pair, the `color_primaries`/`color_trc`/`colorspace`/`color_range` HDR/color band, the codec-private `options` and muxer-private `container_options` maps, the `layout` channel layout, the `thread_count` parallel-encode width, and the optional `master` audio-mastering slot) carrying its own `streamed` video-stream-configure projection (one `add_stream(options=)` plus the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`thread_count` codec-context fold), its `voiced` audio-stream-configure projection (one `add_stream` plus the `layout`/`thread_count`/`bit_rate` codec-context fold, the audio arm reads), and its `colored` per-frame HDR/color-band projection the `_lift` worker stamps, the shared `_drive` loop owning the `pts`/`time_base` per-frame stamp the encode arms read, so a new encode knob is one `MediaProfile` field or `options`/`container_options` entry bound into an existing projection, never a constructor-parameter tail nor a re-derived stream-configure call; `MediaFault` the closed av-boundary cause vocabulary (`unregistered`/`invalid`/`codec`/`provision`/`worker`/`contract`) the whole rail threads, the sibling of `VectorFault`/`RasterFault`; `MediaEvidence` the typed encode receipt (container, codec, duration, byte count, frame count, bit rate, plus the deployment/color `facts` band) the one `MediaEvidence.measure` constructor folds once over the muxed bytes, the read-back, and `_deployment`; `Result[(ContentKey, ArtifactReceipt), MediaFault]` the one carrier every arm returns, keyed over the finished container bytes through `ContentIdentity.of(container.value, blob)` so an identical frame sequence at an identical profile is a cache hit by reference; the `av.open(sink, mode="w")` `OutputContainer` (and the `Transcode`/`Remux` read-side `av.open(BytesIO(source), mode="r")` `InputContainer`) is the one mux/demux capsule per op, always a context manager so the trailer is written and the IO released, never retained across arms.
- Cases: `MediaOp` cases — `EncodeVideo(frames, profile)` (the `tuple[NDArray[np.uint8], ...]` rgb24 frame sequence `scene/render#SCENE` hands across, lifted array-by-array through `VideoFrame.from_ndarray(array, format=_FRAME_FORMAT)`, the `MediaProfile.streamed` projection minting one `VideoStream` and folding its codec-context knobs, the shared `_drive` loop stamping each frame's `pts`/`time_base`, the `stream.encode(frame)` -> `container.mux(packets)` loop driving the sequence and the `stream.encode(None)` flush muxed at end-of-stream) · `Mux(frames, samples, video, audio)` (the one interleave axis muxing a video frame sequence and an audio sample sequence into a single container — one `video.streamed` video stream plus one `audio.voiced` audio stream off the two profiles (the video profile owns the container/codec, the audio profile owns the audio codec/rate/layout/master), the `pts`-ordered `mux` interleaving each stream's packets, the dual flush at end-of-stream — never a parallel A/V-combine surface, the muxer discriminated by the typed `ContainerFormat` value the video profile carries) · `EncodeAudio(samples, profile)` (the `tuple[Pcm, ...]` sample-block sequence the `media/audio#MEDIA` arm owns, lifted through `AudioFrame.from_ndarray` (the producer dtype keying the ingest format) and resampled/reframed through `AudioResampler`, dispatched here by the shared `MediaOp` discriminant and worked by the `media/audio#MEDIA` `_encode_audio` function) · `Transcode(source, profile, filters)` (the read+write pair — `av.open(BytesIO(source), mode="r")` demux/`decode` off `streams.best("video")` -> an optional `av.filter.Graph` linear chain of `(name, args)` `FilterStage`s over `add_buffer`/`add`/`link_nodes`/`configure`/`push`/`pull` -> `reformat` to `pix_fmt` -> `_drive` encode/mux, the encoder stream minted on the first shaped frame so a scale/crop node's output geometry drives its config) · `Remux(source, profile, bsf)` (the quality-lossless container/codec change — `demux` -> `BitStreamFilterContext.filter` (`h264_mp4toannexb`/`hevc_mp4toannexb`/`extract_extradata`) -> `mux_one` copy over an `add_stream_from_template` cloned output stream (`add_mux_stream` the passthrough alternate), never a re-decode) — matched by one total `match`/`case`, the five-case modality recovered from the `MediaOp` discriminant, never a name suffix; the `EncodeVideo`/`Mux`/`Transcode`/`Remux` arms own the video worker on this page and the `EncodeAudio` arm dispatches to the same-folder audio worker `media/audio#MEDIA` declares. `media/filtergraph#FILTER` owns the closed `FilterNode` family and the capability-detection native-vs-substitute filter routing the campaign splits out; this page composes a bounded linear `Graph` chain, the terminal split flagged.
- Entry: `Media.encode` is `async` over the runtime `async_boundary`, returning `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]` — the domain `MediaFault` nested inside the boundary rail exactly as `graphic/raster/io#IO` nests `RuntimeRail[Block[Result[ArtifactReceipt, RasterFault]]]`, so `async_boundary` owns only a truly unexpected raise while every classified av cause stays structurally addressable on the inner rail. `_emit` maps the `_mux` outcome through `_keyed` (deriving `ContentIdentity.of(container, blob)` and constructing the receipt case), and `_mux` dispatches the whole synchronous `av` body onto the runtime subprocess lane (`anyio.to_process.run_sync`, CPU-bound GIL-holding C) wrapped in `_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)` and catches the two seam raises the worker cannot rail as data — `BrokenWorkerProcess` -> `MediaFault(worker=...)` (an exhausted subprocess death past the retry) and `BeartypeCallHintViolation` -> `MediaFault(contract=...)`. Each `@beartype`-woven worker opens one `av.open(...)` container against an in-memory `io.BytesIO` sink, runs its arm (`_encode_video`/`_mux_av`/`_transcode`/`_remux` here, `media/audio#MEDIA` `_encode_audio` for `EncodeAudio`), reads the muxed `bytes` plus the read-back `MediaEvidence`, and returns `Result[(blob, evidence), MediaFault]` — catching `ImportError` -> `provision` (the FFmpeg build absent) and `av.error.FFmpegError` -> the `_media_fault` leaf mapping (`InvalidDataError` -> `invalid`, the not-found leaves -> `unregistered`, any other -> `codec`) at the arm that incurs it, each a module-level function dispatched by qualified name across the subprocess lane (`to_process.run_sync` cannot target a bound method or closure). The arm keys one `ContentKey` over the single container blob through `ContentIdentity.of(container, blob)`; the inbound `scene/render` frame-set Merkle-parent `ContentKey` is the by-reference cache key the runtime lane elides on, the muxed-output key the encode product.
- Auto: frame ingest is the parameterized `_lift` — `VideoFrame.from_ndarray(array, format=profile.frame_format)` admits the host pixel surface directly with no PNG decode, then `reformat(format=profile.pix_fmt)` bridges the producer format to the encoder pixel format (the deleted form is trusting the encoder to auto-convert an rgb24 frame, which the frame-accurate encoders reject) and `profile.colored()` stamps the HDR/color-metadata band on the frame; `MediaProfile.streamed` mints `container.add_stream(profile.codec, rate=profile.rate, options=dict(profile.options))` then folds the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size` onto the stream and `thread_count` onto its `codec_context` from the first frame's shape, dropping `None` knobs; the shared `_drive` loop stamps `frame.pts`/`frame.time_base` from the running frame index against the `rate` so the presentation timeline is monotonic, folds `stream.encode(frame)` over the sequence muxing each returned `Packet` through `container.mux_one` for per-packet back-pressure, then `_flush` flushes with `stream.encode(None)` muxed at end-of-stream, never a fictional lazy-encode iterator; the `Mux` arm runs one `streamed` video configure plus one audio stream and one interleaved `mux_one` over both packet streams ordered by `pts`; the `Transcode` arm drives the read-side `decode` -> `_filtered` `Graph` pull -> `reformat` -> `_drive` pipeline and the `Remux` arm the `demux` -> `BitStreamFilterContext.filter` -> `mux_one` copy; the muxed `bytes` read off the `BytesIO` sink plus the read-back `InputContainer` frame-count/duration probe and the `_deployment` libav/ffmpeg facts fold through `MediaEvidence.measure` once.
- Receipt: each media op contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media` — the eight-slot case `(ContentKey, container, codec, duration, bytes, frames, bit_rate, facts)` the receipt owner declares, the six shared `MediaEvidence` scalars plus the `bit_rate` slot plus the `facts: frozendict[str, float | str]` per-page evidence band — keyed by the content key the `_keyed` arm derives over the muxed bytes; `_keyed` spreads `evidence.container`/`codec`/`duration`/`byte_count`/`frame_count`/`bit_rate` and `evidence.facts` onto the case (the receipt owner's own `_facts` arm flattens the band exactly as `preview` flattens `scores`), so the receipt owner imports no `MediaEvidence` value object nor any `av` container handle — the receipt-side cycle the flat-scalar-plus-band shape forecloses. `MediaEvidence.facts` carries the deployment evidence the `_deployment` worker reads once off `av.library_versions` (the bundled `libavcodec`/`libavformat` majors) and `av.ffmpeg_version_info` (the ffmpeg build) plus the `MediaProfile.colored` HDR/color band, resolving the [01] INDEX citation of those members into a real read rather than a prose assertion. The `duration` is the one observable temporal-encode value the runtime `observability/metrics` `MeterProvider` reads off the receipt fold. The `media/audio#MEDIA` `EncodeAudio` arm contributes the same single `ArtifactReceipt.Media` case (its own EBU R128 LUFS/true-peak/loudness-range facts riding the same band), never a parallel audio-receipt rail.
- Growth: a new container is one `ContainerFormat` row carrying its muxer name; a new codec is one `MediaProfile.codec` string the `add_stream` row reads (a hardware encoder `h264_videotoolbox`/`hevc_videotoolbox` is a codec row, not a knob); a new encode knob (CRF, preset, tune, max-B-frames) is one `MediaProfile.options` entry the `streamed` `add_stream(options=)` fold carries and a muxer knob (HLS `segment_time`, fMP4 `movflags`) one `container_options` entry; a new av fault leaf is one `MediaFault` case plus one `_media_fault` arm; a new evidence fact is one `_deployment` band key with ZERO receipt edit (the `facts` band absorbs it exactly as the `preview` `scores` band does); zero new surface — the modality space is the five cases (`EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux`) on one owner, every addition a row, field, case, or arm. The remaining av surface — `io_open` HLS/DASH/fMP4 segmented sinks, `av.open(hwaccel=)` GPU decode, and the `media/filtergraph#FILTER` capability-detection filter routing — is the campaign's `media/container#CONTAINER` rebuild terminal, not open growth here.

```python signature
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

type Frames = tuple[NDArray[np.uint8], ...]
type Samples = tuple["Pcm", ...]
type FilterStage = tuple[str, str]  # (libavfilter name, args); media/filtergraph#FILTER owns the closed FilterNode family + capability-detection routing
type MediaOpTag = Literal["encode_video", "encode_audio", "mux", "transcode", "remux"]
type MediaFaultTag = Literal["unregistered", "invalid", "codec", "provision", "worker", "contract"]

_FRAME_FORMAT = "rgb24"

# retry the transient subprocess-death seam only, mirroring graphic/raster/io#IO: a BrokenWorkerProcess OOM/signal
# death recovers before the slot faults; a BeartypeCallHintViolation is NOT in the `.on(...)` set — it lifts to `contract`.
_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)


class ContainerFormat(StrEnum):
    MP4 = "mp4"
    WEBM = "webm"
    GIF = "gif"
    MKV = "matroska"


@tagged_union(frozen=True)
class MediaFault:
    # the closed av-boundary cause vocabulary the whole media rail threads, the sibling of graphic/vector#VECTOR
    # `VectorFault` and graphic/raster/io#IO `RasterFault`: each case is structurally addressable so a downstream
    # `match` routes a registration miss apart from a malformed stream apart from a worker death apart from a
    # contract miss — never the message-collapsed `boundary` catch-all a bare `async_boundary(catch=Exception)` mints.
    tag: MediaFaultTag = tag()
    unregistered: tuple[str, str] = case()  # av (En|De)coderNotFound/(Mux|Demux)erNotFound/Filter/BSFNotFound — (leaf, detail)
    invalid: str = case()                   # av InvalidDataError — malformed frame/stream data
    codec: tuple[str, str] = case()         # any other av FFmpegError during encode/mux/transcode/remux — (op, strerror)
    provision: str = case()                 # the bundled FFmpeg build is absent (av ImportError at the worker seam)
    worker: str = case()                    # an exhausted BrokenWorkerProcess subprocess death past `_WORKER_RETRY`
    contract: str = case()                  # a BeartypeCallHintViolation lifted at the worker seam


class MediaProfile(Struct, frozen=True):
    container: ContainerFormat = ContainerFormat.MP4
    codec: str = "libx264"                  # the encoder name; a hardware encoder (h264_videotoolbox/hevc_videotoolbox) is a codec row, never a knob
    rate: int = 24
    bit_rate: int | None = None
    gop_size: int | None = None
    pix_fmt: str = "yuv420p"                # the encoder pixel format the frame reformats to before encode
    frame_format: str = _FRAME_FORMAT      # the producer ndarray pixel format `from_ndarray` ingests; reformat bridges it to `pix_fmt`
    layout: str = "stereo"
    thread_count: int = 0
    color_primaries: str | None = None     # HDR/color-metadata band set on each frame after reformat (bt709/bt2020-ncl/…)
    color_trc: str | None = None
    colorspace: str | None = None
    color_range: str | None = None
    options: frozendict[str, str] = frozendict()            # codec-private knobs (crf/preset/tune) -> add_stream(options=)
    container_options: frozendict[str, str] = frozendict()  # muxer-private knobs (movflags=frag_keyframe+empty_moov for fMP4) -> av.open(container_options=)
    master: "Master | None" = None

    def streamed(self, container: object, width: int, height: int) -> object:
        stream = container.add_stream(self.codec, rate=self.rate, options=dict(self.options))
        stream.width, stream.height, stream.pix_fmt = width, height, self.pix_fmt
        stream.codec_context.thread_count = self.thread_count
        for field, value in (("bit_rate", self.bit_rate), ("gop_size", self.gop_size)):
            if value is not None:
                setattr(stream, field, value)
        return stream

    def voiced(self, container: object) -> object:
        stream = container.add_stream(self.codec, rate=self.rate, options=dict(self.options))
        stream.codec_context.layout = self.layout  # encoder ch-layout set before start_encoding so the resampler's profile.layout frames are accepted
        stream.codec_context.thread_count = self.thread_count
        if self.bit_rate is not None:
            stream.bit_rate = self.bit_rate
        return stream

    def colored(self) -> frozendict[str, str]:
        # the per-frame color-metadata band the worker sets after the pix_fmt reformat; empty when no HDR/color is declared.
        return frozendict({
            attr: value for attr, value in
            (("color_primaries", self.color_primaries), ("color_trc", self.color_trc), ("colorspace", self.colorspace), ("color_range", self.color_range))
            if value is not None
        })


class MediaEvidence(Struct, frozen=True):
    container: ContainerFormat
    codec: str
    duration: float
    byte_count: int
    frame_count: int
    bit_rate: int
    facts: frozendict[str, float | str] = frozendict()  # bundled libav majors + ffmpeg build + color band -> ArtifactReceipt.Media facts band

    @staticmethod
    def measure(container: ContainerFormat, codec: str, duration: float, frames: int, bit_rate: int, blob: bytes, facts: frozendict[str, float | str] = frozendict()) -> "MediaEvidence":
        return MediaEvidence(container, codec, duration, len(blob), frames, bit_rate, facts)


@tagged_union(frozen=True)
class MediaOp:
    tag: MediaOpTag = tag()
    encode_video: tuple[Frames, MediaProfile] = case()
    encode_audio: tuple[Samples, MediaProfile] = case()
    mux: tuple[Frames, Samples, MediaProfile, MediaProfile] = case()
    transcode: tuple[bytes, MediaProfile, tuple[FilterStage, ...]] = case()  # source container bytes -> demux/decode -> Graph -> encode -> mux
    remux: tuple[bytes, MediaProfile, str] = case()                          # source container bytes -> demux -> BSF -> mux copy, no re-decode

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
    def Transcode(source: bytes, profile: MediaProfile, filters: tuple[FilterStage, ...] = ()) -> "MediaOp":
        return MediaOp(transcode=(source, profile, filters))

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
        # `RuntimeRail[Block[Result[ArtifactReceipt, RasterFault]]]`: `async_boundary` owns only a truly
        # unexpected raise, the per-encode av cause stays structurally addressable on the inner rail.
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
                case MediaOp(tag="transcode", transcode=(source, profile, filters)):
                    return await _WORKER_RETRY(to_process.run_sync, _transcode, source, profile, filters, limiter=WORKER_BAND)
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
import io
from collections.abc import Iterator
from fractions import Fraction
from itertools import chain
from typing import TYPE_CHECKING

from beartype import beartype
from expression import Error, Ok, Result

from builtins import frozendict

# same `artifacts.media.video` module as the owner fence; the workers stay module-level so `to_process.run_sync`
# dispatches them by qualified name, and `Frames`/`Samples`/`MediaEvidence`/`MediaProfile`/`MediaFault`/`FilterStage`
# resolve from the owner above. The video-only workers are `@beartype`-woven so a hint violation raises through the
# seam onto `MediaFault.contract`; av's FFmpegError leaves rail to their case at the arm, no bare `except Exception`.

lazy import av
lazy import av.error
lazy import av.filter
lazy from artifacts.media.audio import _encode_audio, _voiced

if TYPE_CHECKING:
    from artifacts.media.audio import Pcm


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
    # resolves the deployment facts the [01] INDEX prose names: the bundled libav majors + the ffmpeg build ride the
    # receipt band, read once off the live linkage rather than asserted in prose and never read; the color band joins them.
    versions = av.library_versions
    return frozendict({
        "libavcodec": float(versions["libavcodec"][0]),
        "libavformat": float(versions["libavformat"][0]),
        "ffmpeg": av.ffmpeg_version_info,
        **profile.colored(),
    })


def _lift(profile: MediaProfile, array: object) -> object:
    # parameterized ingress: the producer ndarray format is `profile.frame_format`, reformatted to the encoder `pix_fmt`
    # (a bespoke rgb24->yuv420p pixel loop is the deleted form) and stamped with the declared HDR/color metadata.
    frame = av.VideoFrame.from_ndarray(array, format=profile.frame_format)
    converted = frame.reformat(format=profile.pix_fmt) if profile.pix_fmt != profile.frame_format else frame
    for attr, value in profile.colored().items():
        setattr(converted, attr, value)
    return converted


def _graph(filters: tuple[FilterStage, ...], template: object, /) -> "av.filter.Graph":
    # a bounded video filter chain (scale/crop/fps/format) over the av.filter.Graph; media/filtergraph#FILTER owns the
    # closed FilterNode family + the capability-detection native-vs-substitute routing, composed here as one linear chain.
    graph = av.filter.Graph()
    source = graph.add_buffer(template=template)
    nodes = tuple(graph.add(name, args) for name, args in filters)
    graph.link_nodes(source, *nodes)
    graph.configure()  # auto_buffer adds the sink on the last node's unlinked output the push/pull drive reads
    return graph


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


def _drive(container: object, stream: object, frame: object, index: int, rate: int) -> None:
    frame.pts, frame.time_base = index, Fraction(1, rate)
    for packet in stream.encode(frame):
        container.mux_one(packet)  # per-packet mux so an encoder emitting a burst back-pressures one packet at a time


def _flush(container: object, stream: object) -> None:
    for packet in stream.encode(None):
        container.mux_one(packet)


def _probe(blob: bytes) -> tuple[float, int, int]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        video = reader.streams.video[0]
        duration = float(reader.duration / av.time_base) if reader.duration is not None else 0.0
        return duration, video.frames, int(video.bit_rate or 0)


@beartype
def _encode_video(frames: Frames, profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    try:
        height, width = frames[0].shape[:2]
        sink = io.BytesIO()
        with av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as container:
            stream = profile.streamed(container, width, height)
            for index, array in enumerate(frames):
                _drive(container, stream, _lift(profile, array), index, profile.rate)
            _flush(container, stream)
        blob = sink.getvalue()
        duration, count, bit_rate = _probe(blob)
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, _deployment(profile))))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("encode_video", exc))


# _mux_av alone is un-`@beartype`d: its `blocks` param is the audio-owned `Pcm` union (a TYPE_CHECKING-only forward
# ref that never enters the runtime namespace, so a call-time hint resolution would NameError), so the mux arm's
# contract is proven by ty/mypy statically; the three video-only workers carry the runtime @beartype -> MediaFault.contract weave.
def _mux_av(frames: Frames, blocks: "tuple[Pcm, ...]", video: MediaProfile, audio: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    try:
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
def _transcode(source: bytes, profile: MediaProfile, filters: tuple[FilterStage, ...]) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # decode -> optional Graph -> encode -> mux over one owned read+write pair; the stream mints on the first shaped
    # frame so the filter graph's output geometry (a scale/crop node changes width/height) drives the encoder config.
    try:
        sink = io.BytesIO()
        with av.open(io.BytesIO(source), mode="r") as reader, av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as container:
            src = reader.streams.best("video")
            graph = _graph(filters, src) if filters else None
            stream: object | None = None
            index = 0
            # append a None flush sentinel only when a graph exists so `_filtered(graph, None)` -> `graph.push(None)`
            # drains a buffering filter's (fps/tmix) tail frames; a graph-less pass iterates the decoded frames alone.
            for frame in chain(reader.decode(src), (None,) if graph is not None else ()):
                for shaped in _filtered(graph, frame):
                    if stream is None:
                        stream = profile.streamed(container, shaped.width, shaped.height)
                    _drive(container, stream, shaped.reformat(format=profile.pix_fmt), index, profile.rate)
                    index += 1
            if stream is not None:
                _flush(container, stream)
        blob = sink.getvalue()
        duration, count, bit_rate = _probe(blob)
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob, _deployment(profile))))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("transcode", exc))


@beartype
def _remux(source: bytes, profile: MediaProfile, bsf: str) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # container/codec change without quality loss: demux -> BitStreamFilterContext.filter (h264_mp4toannexb/…) -> mux
    # copy, never a re-decode; `add_stream_from_template` clones the source parameters (the `add_mux_stream(codec_name)`
    # passthrough is the alternate row when a fresh codec name is in hand) and `mux_one` copies each packet.
    try:
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
            for shaped in (bitstream.filter(None) if bitstream is not None else ()):  # filter(None) drains the buffered packets; flush() only resets state
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

- [MUXED_EVIDENCE_PROBE] [RESOLVED]: the `MediaEvidence` duration/frame-count/bit-rate facts are read by re-opening the muxed `bytes` through `av.open(io.BytesIO(blob), mode="r")` and reading `reader.duration`/`reader.streams.video[0].frames`/`video.bit_rate` (the `InputContainer.duration`/`StreamContainer.video`/`VideoStream.frames` accessors the folder `av` `.api` `[03]-[ENTRYPOINTS]` demux/streams table catalogues, rows `[04]`/`[06]`, exercised on the installed distribution returning `frames=5`/`duration=208333` for the five-frame probe), with `reader.duration / av.time_base` converting the `AV_TIME_BASE` micro-second duration to seconds; the muxed byte length is `len(blob)` off the `BytesIO` sink. The `MediaEvidence.measure` constructor folds the probe, the blob length, and the `_deployment` `facts` band once, mirroring the `package/codec#COMPRESSION` `BundleEvidence.measure` single-fold pattern, and `_keyed` spreads the named scalars plus the band onto `ArtifactReceipt.Media(key, container, codec, duration, byte_count, frame_count, bit_rate, facts)` so the receipt owner imports no `MediaEvidence` value object — the same scalar-plus-native-band acyclic rule the `Bundle`/`Egress`/`Preview` cases hold (the `media/video` owner imports `ArtifactReceipt`, so a reciprocal `receipt.py` import of `MediaEvidence` would close a module-scope cycle).
- [RECEIPT_MEDIA_CASE] [RESOLVED]: the `ArtifactReceipt.Media` case carries the eight-slot `tuple[ContentKey, str, str, float, int, int, int, frozendict[str, float | str]]` (key, container, codec, duration, byte count, frame count, bit rate, facts band) on the same-folder `core/receipt#RECEIPT` owner (verified present: `receipt.md` declares the `media` tag token, the `Media(key, container, codec, duration, bytes_, frames, bit_rate=0, facts=frozendict())` mint, the `media=(...)` case, the `_KEYS["media"]` scalar row, and the special `media` `_facts` arm `{... , **facts}` that flattens the band exactly as `preview` flattens `scores`), so this owner constructs the case with its eighth `facts` argument — `_keyed` threads `evidence.facts` (the `_deployment` libav/ffmpeg majors + the `colored` HDR/color band) and no receipt-case widening lands here (the band already exists, the prior 7-scalar spelling was the stale form). The `media` token is on the union `tag` `Literal`; the `Credential` and `Media` cases are the two producer cases the receipt owner declared without importing a producer value object (the band is a native `frozendict` the producer fills, never an `av` handle), and the `Media` case is the single one `media/video#MEDIA` and `media/audio#MEDIA` both contribute — audio's EBU R128 LUFS/true-peak facts and this page's deployment/color facts riding the same band.
- [AUDIO_ARM_COMPOSE] [RESOLVED]: the `EncodeAudio` arm is one `MediaOp` case dispatched here but worked by the `media/audio#MEDIA` `_encode_audio` module-level function, which composes this owner's `MediaProfile.voiced` audio-stream-configure projection, the shared `_drive`/`_flush`/`_deployment` worker primitives, the `MediaEvidence.measure` constructor, and the `ContainerFormat` vocabulary — never re-owning any of them (it computes its own temporal evidence in-fold, never the video-only `_probe`). The `Mux` arm's audio leg lives here on `media/video` because the container/mux capsule is this owner's, composing `media/audio`'s shared `_voiced` audio-drive primitive over its own `audio.voiced` stream; `media/audio` owns only the `Pcm`/`_INGEST`/`Master` vocabulary, the `_voiced`/`_mastered` primitives, and the standalone `_encode_audio` arm. Both contribute the single `ArtifactReceipt.Media` case, the receipt owner citing both producers.
- [TRANSCODE_GROWTH] [RESOLVED]: the filter-graph transcode arm (`av.open(mode="r")` `decode` -> `av.filter.Graph` scale/crop/fps/format -> `reformat` -> `encode` -> `mux`) and the bitstream-filter remux arm (`BitStreamFilterContext.filter` `demux` -> `bsf.filter` -> `mux_one` copy, no re-decode) landed as the `Transcode`/`Remux` `MediaOp` cases plus one `_transcode`/`_remux` worker arm each, closing the prior deferral. The read side composes `InputContainer.demux`/`decode`/`streams.best` and the `av.filter.Graph` (`add_buffer`/`add`/`link_nodes`/`configure`/`push`/`pull` via `_graph`/`_filtered`, the exact drain protocol the audio `_master`/`_drain` uses) and the remux side `add_stream_from_template`/`add_mux_stream` + `BitStreamFilterContext.filter` — every member verified present on the installed `av 17.1.0`. The modality space is now the five cases (`EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux`) on one owner. Terminal split: `media/filtergraph#FILTER` (the NEW page the campaign authors) owns the closed `FilterNode` family and the capability-detection native-vs-substitute routing (text/subtitle burn-in, `eq`->`colorbalance`+`curves`, `hqdn3d`->`nlmeans`); this page composes a bounded linear `Graph` chain of `(name, args)` stages, and the `media/container#CONTAINER` rebuild absorbs the read/write/`io_open`/`hwaccel` container spine.
- [FAULT_RAIL] [RESOLVED]: the closed `MediaFault` `@tagged_union` (`unregistered`/`invalid`/`codec`/`provision`/`worker`/`contract`) is the cause vocabulary the whole media rail threads, the sibling of the `graphic/vector#VECTOR` `VectorFault` and `graphic/raster/io#IO` `RasterFault` unions and the exact anti-pattern-reversal the prior design owed: the old fence trusted `async_boundary(catch=Exception)` to swallow every av raise into the runtime `boundary` catch-all, erasing which FFmpeg component failed. Each `@beartype`-woven worker captures its av raises at the arm that incurs them — `av.error.InvalidDataError` -> `invalid`, the six not-found leaves (`EncoderNotFoundError`/`DecoderNotFoundError`/`MuxerNotFoundError`/`DemuxerNotFoundError`/`FilterNotFoundError`/`BSFNotFoundError`) -> `unregistered`, any other `av.error.FFmpegError` -> `codec`, an `av` `ImportError` -> `provision` — and returns `Result[(bytes, MediaEvidence), MediaFault]` as data across the `to_process` seam (the picklable-Result-across-subprocess pattern `graphic/raster/io#IO` `_worker_raster` proves); `_mux` catches the two raises the worker cannot rail — `BrokenWorkerProcess` -> `worker` past the `_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)` (only the subprocess-death seam retries, never a contract violation) and `BeartypeCallHintViolation` -> `contract`. The `av.error.FFmpegError` hierarchy and its leaves verify present on `av 17.1.0` (`av.error.EncoderNotFoundError` … `av.error.BSFNotFoundError`/`InvalidDataError`); the `av` `.api` `[04]-[IMPLEMENTATION_LAW]` `[STACK_INTEGRATION]` expression tier names this exact mapping. Cross-file: the worker return-type widening (`tuple[bytes, MediaEvidence]` -> `Result[..., MediaFault]`) ripples to `media/audio#MEDIA` `_encode_audio` (dispatched by this owner's `_mux` and must return the same shape), and the `MediaEvidence.facts` band widening lets audio's EBU R128 facts ride the same receipt slot.
