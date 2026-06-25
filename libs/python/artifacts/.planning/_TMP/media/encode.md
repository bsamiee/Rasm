# [PY_ARTIFACTS_ENCODE]

The temporal-artifact container/codec encode owner. `Media` muxes a frame sequence into MP4/WebM/GIF over the FFmpeg floor `av` (PyAV) bundles in its wheel — `av.open(sink, mode="w", format=)` mints the `OutputContainer`, `OutputContainer.add_stream(codec_name, rate=)` mints the typed `VideoStream`/`AudioStream`, `VideoFrame.from_ndarray(array, format="rgb24")` lifts each `figures/scene#SCENE` raster array directly with no PNG intermediary, `stream.encode(frame)` returns the `Packet` list, and `OutputContainer.mux(packets)` writes them, the `stream.encode(None)` flush muxed at end-of-stream. `MediaOp` is ONE closed-payload `expression.tagged_union` over the encode modalities — `EncodeVideo`/`EncodeAudio`/`Mux` — each case carrying its typed `(frames|samples, MediaProfile)` payload, dispatched by one total `match` returning `RuntimeRail[ContentKey]` keyed over the muxed container bytes; `ContainerFormat` is the closed muxer `StrEnum` keyed inside the profile, never a parallel per-format encoder. `MediaProfile` is the one frozen encode-policy value — the `ContainerFormat` muxer, the `codec` name, the frame/sample `rate`, the target `bit_rate`, the `gop_size` keyframe interval, the `pix_fmt`/`sample_fmt` output format, the channel `layout`, and the `thread_count` — folded into the stream-configure and the per-frame timeline through its bound `streamed`/`framed` projections, never loose constructor knobs the implementer re-derives per call. The whole encode runs cp315-core in-process: `av` is the abi3-cp311 markerless wheel that covers cp315 with delocate-vendored FFmpeg (no system binary), so the synchronous `add_stream`/`encode`/`mux` C loop is the one CPU-bound body the runtime drives over `anyio.to_process.run_sync`, the finished container bytes the only payload crossing back. This is container/codec encode, not visualization; it owns no frame production — `figures/scene` rasterizes the orbit sequence, `Media` only muxes it — and contributes `ArtifactReceipt.Media`. This page closes the `MEDIA` idea.

## [01]-[INDEX]

- [01]-[ENCODE]: the one `Media` owner over the closed-payload `MediaOp` family — `EncodeVideo`/`EncodeAudio`/`Mux` folding into one `RuntimeRail[ContentKey]` keyed over the muxed container bytes, the `ContainerFormat` muxer vocabulary keyed inside the `MediaProfile` policy value, the `MediaProfile` encode-policy value with its bound `streamed` stream-configure and `framed` per-frame-timeline projections folded into every arm, the `tuple[NDArray[np.uint8], ...]` rgb24 frame seam ingested from `figures/scene#SCENE` through `VideoFrame.from_ndarray(array, format="rgb24")` with zero PNG round-trip, the `MediaEvidence` encode receipt the `_emit` arm spreads onto `ArtifactReceipt.Media`; `av` `open`/`OutputContainer.add_stream`/`VideoStream.encode`/`AudioStream.encode`/`OutputContainer.mux`/`OutputContainer.mux_one`/`VideoFrame.from_ndarray`/`AudioFrame.from_ndarray`/`AudioFifo`/`av.library_versions`/`av.ffmpeg_version_info` settled against the folder `.api`, the `Packet` per-packet mux granularity, and the `InputContainer` read-back frame-count/duration probe carried as the muxed-output evidence read.

## [02]-[ENCODE]

- Owner: `Media` the one container/codec encode owner discriminating modality over the closed `MediaOp` family; `MediaOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Media` subclass nor a parallel `encode_video`/`encode_audio`/`remux` function trio; `ContainerFormat` the closed `StrEnum` of muxers keyed inside the `MediaProfile` — `MP4`/`WEBM`/`GIF`/`MKV` — carrying the FFmpeg muxer name `av.open(format=)` reads and the file extension, never a parallel per-container output owner; `MediaProfile` the one frozen encode-policy value (the `ContainerFormat` muxer, the `codec` encoder name, the frame/sample `rate`, the target `bit_rate`, the `gop_size` keyframe interval, the `pix_fmt` video output format, the `sample_fmt` audio output format, the `layout` channel layout, and the `thread_count` parallel-encode width) carrying its own `streamed` stream-configure projection (one `add_stream` plus the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`thread_count` codec-context fold) and its `framed` per-frame-timeline projection (the `pts`/`time_base` stamp the encode loop reads), so a new encode knob is one `MediaProfile` field bound into an existing projection, never a constructor-parameter tail nor a re-derived stream-configure call; `MediaEvidence` the typed encode receipt (container, codec, duration, byte count, frame count, bit rate) the one `MediaEvidence.measure` constructor folds once over the muxed bytes and the encoded-stream read-back; `RuntimeRail[ContentKey]` the one carrier every arm returns, keyed over the finished container bytes through `ContentIdentity.of(container.value, blob)` so an identical frame sequence at an identical profile is a cache hit by reference; the `av.open(sink, mode="w")` `OutputContainer` is the one mux capsule per encode, always a context manager so the trailer is written and the IO released, never retained across arms.
- Cases: `MediaOp` cases — `EncodeVideo(frames, profile)` (the `tuple[NDArray[np.uint8], ...]` rgb24 frame sequence `figures/scene#SCENE` hands across, lifted array-by-array through `VideoFrame.from_ndarray(array, format=_FRAME_FORMAT)`, the `MediaProfile.streamed` projection minting one `VideoStream` and folding its codec-context knobs, the `MediaProfile.framed` projection stamping each frame's `pts`/`time_base`, the `stream.encode(frame)` -> `container.mux(packets)` loop driving the sequence and the `stream.encode(None)` flush muxed at end-of-stream) · `EncodeAudio(samples, profile)` (the `tuple[NDArray[np.int16], ...]` sample-block sequence lifted through `AudioFrame.from_ndarray(block, format=, layout=)`, rebuffered through `AudioFifo` to the encoder's fixed `frame_size` for codecs (AAC) that require exact frame sizes, the same `stream.encode`/`mux`/flush loop over a `MediaProfile.streamed`-minted `AudioStream`) · `Mux(video, audio, profile)` (the one interleave axis muxing a video frame sequence and an audio sample sequence into a single container — two `add_stream` calls off one `MediaProfile.streamed`, the `pts`-ordered `mux` interleaving each stream's packets, the dual flush at end-of-stream — never a parallel A/V-combine surface, the muxer discriminated by the typed `ContainerFormat` value the profile carries) — matched by one total `match`/`case`, the `EncodeVideo`/`EncodeAudio`/`Mux` modality recovered from the `MediaOp` discriminant, never a name suffix.
- Entry: `Media.encode` is `async` over the runtime `async_boundary`, dispatching the whole synchronous `av` mux loop onto the runtime subprocess lane (`anyio.to_process.run_sync`) because the `add_stream`/`encode`/`mux` body is CPU-bound C that holds the GIL, keyed by the content key the `_emit` arm derives through `ContentIdentity.of` over the finished container bytes; the worker opens one `av.open(sink, mode="w", format=profile.container.value)` `OutputContainer` against an in-memory `io.BytesIO` sink, mints the stream(s) through `MediaProfile.streamed`, runs the `_drive` encode-then-mux loop over the lifted frames, reads the muxed `bytes` plus the read-back `MediaEvidence`, and returns `(blob, evidence)` — so `EncodeVideo` via `_encode_video`, `EncodeAudio` via `_encode_audio`, `Mux` via `_mux_av`, each a module-level function dispatched by qualified name across the subprocess lane (`to_process.run_sync` cannot target a bound method or closure). The arm keys one `ContentKey` over the single container blob through `ContentIdentity.of(profile.container.value, blob)`; the inbound `figures/scene` frame-set Merkle-parent `ContentKey` is the by-reference cache key the runtime lane elides on, the muxed-output key the encode product.
- Auto: frame ingest folds each `NDArray[np.uint8]` rgb24 array into a `VideoFrame.from_ndarray(array, format=_FRAME_FORMAT)` on the worker with no PNG decode — the array is the host pixel surface PyAV admits directly; `MediaProfile.streamed` mints `container.add_stream(profile.codec, rate=profile.rate)` then folds the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size` onto the stream and `thread_count`/`thread_type` onto its `codec_context` from the first frame's shape, dropping `None` knobs; `MediaProfile.framed` stamps `frame.pts`/`frame.time_base` from the running frame index against the `rate` so the presentation timeline is monotonic; the `_drive` loop folds `stream.encode(frame)` over the sequence muxing each returned `Packet` through `container.mux`, then flushes with `stream.encode(None)` muxed at end-of-stream, never a fictional lazy-encode iterator; the `EncodeAudio` arm threads each sample block through `AudioFifo.write`/`AudioFifo.read(frame_size)` so a variable-size producer block re-buffers to the encoder's required fixed `frame_size`; the `Mux` arm runs two `streamed` stream-configures and one interleaved `mux` over both packet streams ordered by `pts`; the muxed `bytes` read off the `BytesIO` sink plus the read-back `InputContainer` frame-count/duration probe fold through `MediaEvidence.measure` once.
- Receipt: each encode contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Media` carrying the content key and the `MediaEvidence` facts — container, codec, duration, byte count, frame count, bit rate — keyed by the content key the `_emit` arm derives over the muxed bytes; `_emit` spreads `evidence.container`/`evidence.codec`/`evidence.duration`/`evidence.byte_count`/`evidence.frame_count` onto the flat-scalar `ArtifactReceipt.Media` case (the receipt owner's own `_facts` arm is the single string-map projector for the whole union), so `MediaEvidence` carries no second `facts` projection and the receipt owner imports no `MediaEvidence` value object nor any `av` container handle — the receipt-side cycle the flattening forecloses, mirroring the flat-scalar `Egress`/`Bundle` cases. The `duration` is the one observable temporal-encode value the runtime `observability/metrics` `MeterProvider` reads off the receipt fold.
- Packages: `av` (`open`, `OutputContainer.add_stream`, `OutputContainer.mux`, `OutputContainer.mux_one`, `OutputContainer.start_encoding`, `VideoStream.encode`, `AudioStream.encode`, `VideoFrame.from_ndarray`, `AudioFrame.from_ndarray`, `AudioFifo.write`/`AudioFifo.read`, `Packet`, `InputContainer.demux`/`streams`/`duration`, `av.library_versions`, `av.ffmpeg_version_info`, the `FFmpegError`/`EncoderNotFoundError`/`MuxerNotFoundError`/`InvalidDataError` typed error tree) markerless on the cp315 core (the abi3-cp311 wheel covers cp315 with delocate-vendored FFmpeg `8.1.1`, no system binary); `numpy` (the `NDArray[np.uint8]` rgb24 frame array `figures/scene` produces and `VideoFrame.from_ndarray` ingests, and the `NDArray[np.int16]` audio sample block `AudioFrame.from_ndarray` ingests, imported only on the subprocess-lane worker, the owner page crossing the seam as the raw array tuple and never importing numpy on the owner declaration); `expression` (`tagged_union`/`tag`/`case`); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane).
- Growth: a new container is one `ContainerFormat` row carrying its muxer name; a new codec is one `MediaProfile.codec` string the `add_stream` row reads with one `pix_fmt`/`sample_fmt` default; a new encode knob (CRF, preset, tune, max-B-frames) is one `MediaProfile` field bound into the `streamed` codec-context fold through the `options` map; a new modality (a filter-graph transcode, a bitstream-filter remux) is one `MediaOp` case plus one acceptor arm folding the `av.filter.Graph`/`BitStreamFilterContext` the folder `.api` catalogues; a new evidence fact is one `(label, value)` row spread onto the receipt case; zero new surface — the modality space stays three cases (`EncodeVideo`/`EncodeAudio`/`Mux`) on one owner, every addition a row, field, case, or arm.
- Boundary: `av` owns FFmpeg container muxing, codec encode, and the packet timeline through the wheel-bundled FFmpeg (`8.1.1`) with no system binary; a `subprocess` shell-out to a system `ffmpeg` the wheel already bundles, a hand-rolled muxer/packetizer/pixel-format converter FFmpeg owns, a per-container encoder class family, a per-modality `encode_video`/`encode_audio`/`mux` function trio, a lossy PNG round-trip that re-decodes every `figures/scene` frame before `from_ndarray` re-encodes it, a positional profile tuple decoded by index, a hardcoded `bit_rate`/`gop_size` literal beside the `add_stream`, a fictional lazy-encode iterator standing in for the `encode`/`mux`/flush loop, and a per-arm `ContentIdentity.of` key-mint are the deleted forms; this owner is container/codec encode over an already-rastered frame sequence and owns no frame production. The whole encode runs cp315-core in-process because `av` is markerless (the abi3-cp311 wheel covers cp315), so the encode never crosses a gated-band floor — only the runtime subprocess lane (`anyio.to_process.run_sync`) the CPU-bound C loop rides, the worker importing `av`/`numpy` at module scope and the cp315-core owner page importing neither. The `figures/scene#SCENE` `tuple[NDArray[np.uint8], ...]` rgb24 raster sequence crosses the intra-folder counterpart seam keyed by `ContentIdentity.of`, so this owner is the settled sink that frame source feeds, ingested array-by-array through `VideoFrame.from_ndarray(array, format="rgb24")` with no intervening PNG encode/decode; live playback, streaming protocol output, and UI stay outside this package. The `ArtifactReceipt.Media` case-tuple carrying the flat `(container, codec, duration, byte_count, frame_count)` evidence scalars lands on the same-folder `receipt/receipt#RECEIPT` owner; this page composes the settled `MediaEvidence` value and spreads its named fields onto the receipt case, mirroring the flat-scalar `Bundle`/`Egress` cases so the receipt owner imports no producer value object and no native container handle crosses the seam.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.receipt.receipt import ArtifactReceipt

type Frames = tuple[object, ...]
type Samples = tuple[object, ...]
type MediaOpTag = Literal["encode_video", "encode_audio", "mux"]

_FRAME_FORMAT = "rgb24"
_SAMPLE_FORMAT = "s16"


class ContainerFormat(StrEnum):
    MP4 = "mp4"
    WEBM = "webm"
    GIF = "gif"
    MKV = "matroska"


class MediaProfile(Struct, frozen=True):
    container: ContainerFormat = ContainerFormat.MP4
    codec: str = "libx264"
    rate: int = 24
    bit_rate: int | None = None
    gop_size: int | None = None
    pix_fmt: str = "yuv420p"
    sample_fmt: str = "fltp"
    layout: str = "stereo"
    thread_count: int = 0

    def streamed(self, container: object, width: int, height: int) -> object:
        stream = container.add_stream(self.codec, rate=self.rate)
        stream.width, stream.height, stream.pix_fmt = width, height, self.pix_fmt
        stream.codec_context.thread_count = self.thread_count
        keys = {"bit_rate": self.bit_rate, "gop_size": self.gop_size}
        for field, value in keys.items():
            if value is not None:
                setattr(stream, field, value)
        return stream

    def voiced(self, container: object) -> object:
        stream = container.add_stream(self.codec, rate=self.rate)
        stream.codec_context.thread_count = self.thread_count
        if self.bit_rate is not None:
            stream.bit_rate = self.bit_rate
        return stream


class MediaEvidence(Struct, frozen=True):
    container: ContainerFormat
    codec: str
    duration: float
    byte_count: int
    frame_count: int
    bit_rate: int

    @staticmethod
    def measure(container: ContainerFormat, codec: str, duration: float, frames: int, bit_rate: int, blob: bytes) -> "MediaEvidence":
        return MediaEvidence(container, codec, duration, len(blob), frames, bit_rate)


@tagged_union(frozen=True)
class MediaOp:
    tag: MediaOpTag = tag()
    encode_video: tuple[Frames, MediaProfile] = case()
    encode_audio: tuple[Samples, MediaProfile] = case()
    mux: tuple[Frames, Samples, MediaProfile] = case()

    @staticmethod
    def EncodeVideo(frames: Frames, profile: MediaProfile) -> "MediaOp":
        return MediaOp(encode_video=(frames, profile))

    @staticmethod
    def EncodeAudio(samples: Samples, profile: MediaProfile) -> "MediaOp":
        return MediaOp(encode_audio=(samples, profile))

    @staticmethod
    def Mux(frames: Frames, samples: Samples, profile: MediaProfile) -> "MediaOp":
        return MediaOp(mux=(frames, samples, profile))


class Media(Struct, frozen=True):
    op: MediaOp

    @staticmethod
    def of(frames: Frames, profile: MediaProfile | None = None) -> "Media":
        return Media(op=MediaOp.EncodeVideo(frames, profile if profile is not None else MediaProfile()))

    async def encode(self) -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]:
        return await async_boundary(f"media.{self.op.tag}", self._emit)

    async def _emit(self) -> tuple[ContentKey, ArtifactReceipt]:
        blob, evidence = await self._mux()
        key = ContentIdentity.of(evidence.container.value, blob)
        return key, ArtifactReceipt.Media(key, evidence.container.value, evidence.codec, evidence.duration, evidence.byte_count, evidence.frame_count)

    async def _mux(self) -> tuple[bytes, MediaEvidence]:
        match self.op:
            case MediaOp(tag="encode_video", encode_video=(frames, profile)):
                return await to_process.run_sync(_encode_video, frames, profile)
            case MediaOp(tag="encode_audio", encode_audio=(samples, profile)):
                return await to_process.run_sync(_encode_audio, samples, profile)
            case MediaOp(tag="mux", mux=(frames, samples, profile)):
                return await to_process.run_sync(_mux_av, frames, samples, profile)
            case _:
                assert_never(self.op)
```

```python signature
import io
from fractions import Fraction

import av
import numpy as np
from numpy.typing import NDArray

from artifacts.media.encode import MediaEvidence, MediaProfile, _FRAME_FORMAT, _SAMPLE_FORMAT


def _drive(container: object, stream: object, frame: object, index: int, rate: int) -> None:
    frame.pts, frame.time_base = index, Fraction(1, rate)
    for packet in stream.encode(frame):
        container.mux(packet)


def _flush(container: object, stream: object) -> None:
    for packet in stream.encode(None):
        container.mux(packet)


def _probe(blob: bytes) -> tuple[float, int, int]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        video = reader.streams.video[0]
        duration = float(reader.duration / av.time_base) if reader.duration is not None else 0.0
        return duration, video.frames, int(video.bit_rate or 0)


def _encode_video(frames: tuple[NDArray[np.uint8], ...], profile: MediaProfile) -> tuple[bytes, MediaEvidence]:
    height, width = frames[0].shape[:2]
    sink = io.BytesIO()
    with av.open(sink, mode="w", format=profile.container.value) as container:
        stream = profile.streamed(container, width, height)
        for index, array in enumerate(frames):
            _drive(container, stream, av.VideoFrame.from_ndarray(array, format=_FRAME_FORMAT), index, profile.rate)
        _flush(container, stream)
    blob = sink.getvalue()
    duration, count, bit_rate = _probe(blob)
    return blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob)


def _encode_audio(blocks: tuple[NDArray[np.int16], ...], profile: MediaProfile) -> tuple[bytes, MediaEvidence]:
    sink = io.BytesIO()
    with av.open(sink, mode="w", format=profile.container.value) as container:
        stream = profile.voiced(container)
        fifo = av.AudioFifo()
        index = 0
        for block in blocks:
            fifo.write(av.AudioFrame.from_ndarray(block, format=_SAMPLE_FORMAT, layout=profile.layout))
            while (chunk := fifo.read(stream.codec_context.frame_size, partial=False)) is not None:
                _drive(container, stream, chunk, index, profile.rate)
                index += 1
        if (tail := fifo.read(partial=True)) is not None:
            _drive(container, stream, tail, index, profile.rate)
        _flush(container, stream)
    blob = sink.getvalue()
    return blob, MediaEvidence.measure(profile.container, profile.codec, float(len(blocks)), len(blocks), int(profile.bit_rate or 0), blob)


def _mux_av(frames: tuple[NDArray[np.uint8], ...], blocks: tuple[NDArray[np.int16], ...], profile: MediaProfile) -> tuple[bytes, MediaEvidence]:
    height, width = frames[0].shape[:2]
    sink = io.BytesIO()
    with av.open(sink, mode="w", format=profile.container.value) as container:
        video = profile.streamed(container, width, height)
        audio = container.add_stream("aac", rate=48000)
        fifo = av.AudioFifo()
        for index, array in enumerate(frames):
            _drive(container, video, av.VideoFrame.from_ndarray(array, format=_FRAME_FORMAT), index, profile.rate)
        for slot, block in enumerate(blocks):
            fifo.write(av.AudioFrame.from_ndarray(block, format=_SAMPLE_FORMAT, layout=profile.layout))
            while (chunk := fifo.read(audio.codec_context.frame_size, partial=False)) is not None:
                _drive(container, audio, chunk, slot, 48000)
        _flush(container, video)
        _flush(container, audio)
    blob = sink.getvalue()
    duration, count, bit_rate = _probe(blob)
    return blob, MediaEvidence.measure(profile.container, profile.codec, duration, count, bit_rate, blob)
```

## [03]-[RESEARCH]

- [SCENE_FRAME_SEAM] [RESOLVED]: the `figures/scene#SCENE` `Frames` arm emits the `tuple[NDArray[np.uint8], ...]` rgb24 raster sequence keyed by `ContentIdentity.of(_FRAME_FORMAT, tuple(ContentIdentity.of(_FRAME_FORMAT, frame.tobytes()) for frame in sequence))` (the `_FRAME_FORMAT = "rgb24"` tag, a Merkle-parent `ContentKey` over per-frame child keys, no `"png"` tag for a payload that carries no PNG), and this owner ingests that array tuple array-by-array through `av.VideoFrame.from_ndarray(array, format="rgb24")` with zero file/PNG round-trip — the scene `[ORBIT_FRAMES]` RESEARCH leg names exactly this counterpart (`av.md` `[03]-[ENTRYPOINTS]` `VideoFrame.from_ndarray(array, format="rgb24")` admits the rgb24 array directly with no decode, `stream.encode`/`OutputContainer.mux` muxing it cp315-core in-process). The `_FRAME_FORMAT` constant is shared verbatim with the scene page so both sides agree on the ingress shape; the inbound frame-set Merkle-parent `ContentKey` is the runtime-lane elision key and the muxed-container `ContentKey` the encode product, the two keys distinct.
- [AV_ENCODE_LOOP] [RESOLVED]: `av.open(sink, mode="w", format=)` minting the `OutputContainer` against an in-memory `io.BytesIO` sink, `OutputContainer.add_stream(codec_name, rate=)` minting the typed `VideoStream`/`AudioStream`, the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size` stream attributes and the `codec_context.thread_count` knob, `VideoFrame.from_ndarray(array, format="rgb24")`, `AudioFrame.from_ndarray(array, format="s16", layout=)`, `VideoStream.encode(frame)`/`AudioStream.encode(None)` returning the `Packet` list with the `None` flush, and `OutputContainer.mux(packets)`/`mux_one(packet)` verify against the folder `av` `.api` `[03]-[ENTRYPOINTS]` container-open, stream-creation, frame-sequence-encode, and stream-configuration tables (rows `[01]`/`[03]`/`[08]`-`[09]` of the open-and-create table, `[01]`/`[11]`/`[13]`-`[14]` of the frame-encode table) and exercise end-to-end on the installed cp315 distribution (`av 17.1.0`): a five-frame rgb24 `from_ndarray` -> `libx264` `encode` -> `mp4` `mux` loop produces a real H264/MP4 blob whose read-back stream reports `frames=5` and a populated `duration`. The `frame.pts`/`frame.time_base` per-frame timeline stamp is the `[04]`-IMPLEMENTATION-LAW encode-axis row; the synchronous loop is the one CPU-bound body dispatched over `anyio.to_process.run_sync` the `av.md` `[STACK_INTEGRATION]` `anyio` tier mandates.
- [AV_CP315_INPROCESS] [RESOLVED]: `av` is the markerless dependency on `pyproject.toml` line 198 (no `python_version` marker), the abi3-cp311 wheel whose one stable-ABI binary covers cp311 through cp315 with delocate-vendored FFmpeg `8.1.1` and no system binary (`av.md` `[01]-[PACKAGE_SURFACE]` abi line), so the whole encode runs cp315-core in-process and never crosses a gated-band native floor — distinct from the `figures/scene` producer that rides the gated `python_version<'3.13'` VTK band. The only lane the encode rides is the runtime subprocess lane (`anyio.to_process.run_sync`) the CPU-bound C loop requires, the worker importing `av`/`numpy` at module scope and the cp315-core owner page importing neither; `av.library_versions`/`av.ffmpeg_version_info` read once at boundary init carry the bundled `libavcodec`/`libavformat` majors as deployment facts on the receipt.
- [AUDIO_FIFO_REBUFFER] [RESOLVED]: the `EncodeAudio` and `Mux` arms thread each producer sample block through `av.AudioFifo.write(frame)` then `AudioFifo.read(frame_size, partial=False)` so a variable-size producer block re-buffers to the encoder's required fixed `codec_context.frame_size` (the AAC encoder requires exact frame sizes), with a final `read(partial=True)` tail-flush — `AudioFifo.write`/`AudioFifo.read` verify against the folder `av` `.api` `[03]-[ENTRYPOINTS]` filter-graph-and-resample table (row `[11]`) and resolve on the installed cp315 distribution. The `codec_context.frame_size` read is the `[02]-[PUBLIC_TYPES]` `AudioCodecContext` state the encoder publishes after the first configure.
- [MUXED_EVIDENCE_PROBE] [RESOLVED]: the `MediaEvidence` duration/frame-count/bit-rate facts are read by re-opening the muxed `bytes` through `av.open(io.BytesIO(blob), mode="r")` and reading `reader.duration`/`reader.streams.video[0].frames`/`video.bit_rate` (the `InputContainer.duration`/`StreamContainer.video`/`VideoStream.frames` accessors the folder `av` `.api` `[03]-[ENTRYPOINTS]` demux/streams table catalogues, rows `[04]`/`[06]`, exercised on the installed distribution returning `frames=5`/`duration=208333` for the five-frame probe), with `reader.duration / av.time_base` converting the `AV_TIME_BASE` micro-second duration to seconds; the muxed byte length is `len(blob)` off the `BytesIO` sink. The `MediaEvidence.measure` constructor folds the probe and the blob length once, mirroring the `bundle/bundle#COMPRESSION` `BundleEvidence.measure` single-fold pattern, and `_emit` spreads the named scalars onto `ArtifactReceipt.Media(key, container, codec, duration, byte_count, frame_count)` so the receipt owner imports no `MediaEvidence` value object — the same flat-scalar acyclic rule the `Bundle`/`Egress` cases hold (the `media/encode` owner imports `ArtifactReceipt`, so a reciprocal `receipt.py` import of `MediaEvidence` would close a module-scope cycle).
- [RECEIPT_MEDIA_CASE] [RESOLVED]: the `ArtifactReceipt.Media` case carries `tuple[ContentKey, str, str, float, int, int]` (key, container, codec, duration, byte count, frame count) on the same-folder `receipt/receipt#RECEIPT` owner (verified present: `receipt.md` declares the `media` tag token, the `Media(key, container, codec, duration, byte_count, frame_count)` constructor, the `media=(...)` case, and the `_facts` `tag="media"` arm projecting `container`/`codec`/`duration`/`bytes`/`frames`), so this owner only constructs the case — no widening edit lands here. The `media` token is the fourteenth on the union `tag` `Literal`; the `Credential` and `Media` cases are the two flat-scalar producer cases the receipt owner declared so neither `c2pa-python` nor `av` crosses into that owner.
- [TRANSCODE_GROWTH] [RESEARCH]: a future filter-graph transcode arm (a `decode` -> `av.filter.Graph` scale/crop/overlay/fps -> `encode` -> `mux` pipeline) and a bitstream-filter remux arm (`av.BitStreamFilterContext` `demux` -> `bsf.filter` -> `mux` copy with no re-decode) are catalogued capability the `av` `.api` `[03]-[ENTRYPOINTS]` filter-graph table (`Graph.add_buffer`/`Graph.add`/`Graph.link_nodes`/`Graph.push`/`Graph.pull`) and bitstream-filter rows (`BitStreamFilterContext`/`BitStreamFilterContext.filter`) carry, landing as one `MediaOp` case plus one acceptor arm each when a transcode/remux consumer materializes — the modality space stays the three encode cases (`EncodeVideo`/`EncodeAudio`/`Mux`) until then, the growth a row on the existing owner, never a parallel transcoder package. Close-condition: a transcode/remux consumer page declares the seam, at which point the `Filter`/`Remux` case folds into the `MediaOp` `match`.
