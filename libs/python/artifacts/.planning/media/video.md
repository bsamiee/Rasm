# [PY_ARTIFACTS_MEDIA_VIDEO]

The temporal-artifact container/codec VIDEO encode/mux owner — the lead encoder and the primary `Media` owner. `Media` muxes a frame sequence into MP4/WebM/GIF/MKV over the FFmpeg floor `av` (PyAV) provides: `av.open(sink, mode="w", format=)` mints the `OutputContainer`, `OutputContainer.add_stream(codec_name, rate=)` mints the typed `VideoStream`, `VideoFrame.from_ndarray(array, format="rgb24")` lifts each `scene/render#SCENE` raster array directly with no PNG intermediary, `stream.encode(frame)` returns the `Packet` list, and `OutputContainer.mux(packets)` writes them, the `stream.encode(None)` flush muxed at end-of-stream. `MediaOp` is ONE closed-payload `expression.tagged_union` over the encode modalities — `EncodeVideo`/`EncodeAudio`/`Mux` — each case carrying its typed payload, dispatched by one total `match` returning `RuntimeRail[ContentKey]` keyed over the muxed container bytes; `ContainerFormat` is the closed muxer `StrEnum` keyed inside the profile, never a parallel per-format encoder. `MediaProfile` is the one frozen encode-policy value — the `ContainerFormat` muxer, the `codec` name, the frame/sample `rate`, the target `bit_rate`, the `gop_size` keyframe interval, the `pix_fmt` output format, the channel `layout`, the `thread_count`, and the audio-mastering `master` slot — folded into the stream-configure through its bound `streamed`/`voiced` projections and into the per-frame timeline through the shared `_drive` `pts`/`time_base` stamp, never loose constructor knobs the implementer re-derives per call. This page owns the shared `Media`/`MediaOp`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family plus the container/mux capsule and the video worker; the AUDIO-stream encode arm (`AudioFrame`/`AudioResampler`/`Master`) composes this owner at `media/audio#MEDIA`. The encode loop runs over the `WORKER_BAND`-bounded `anyio.to_process.run_sync` lane, and the finished container bytes are the only payload crossing back. This is container/codec encode, not visualization; it owns no frame production — `scene/render` rasterizes the orbit sequence, `Media` only muxes it — and contributes `ArtifactReceipt.Media`. This page closes the VIDEO half of the `MEDIA` idea.

## [01]-[INDEX]

- [01]-[MEDIA]: the one `Media` owner over the closed-payload `MediaOp` family — `EncodeVideo`/`EncodeAudio`/`Mux` folding into one `RuntimeRail[ContentKey]` keyed over the muxed container bytes, the `ContainerFormat` muxer vocabulary keyed inside the `MediaProfile` policy value, the `MediaProfile` encode-policy value with its bound `streamed`/`voiced` stream-configure projections folded into every arm and the shared `_drive` `pts`/`time_base` per-frame stamp, the `tuple[NDArray[np.uint8], ...]` rgb24 frame seam ingested from `scene/render#SCENE` through `VideoFrame.from_ndarray(array, format="rgb24")` with zero PNG round-trip, the `MediaEvidence` encode receipt the `_emit` arm spreads onto `ArtifactReceipt.Media`; `av` `open`/`OutputContainer.add_stream`/`VideoStream.encode`/`OutputContainer.mux`/`OutputContainer.mux_one`/`VideoFrame.from_ndarray`/`av.library_versions`/`av.ffmpeg_version_info` settled against the folder `.api`, the `Packet` per-packet mux granularity, and the `InputContainer` read-back frame-count/duration probe carried as the muxed-output evidence read. The shared `Media`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family and the `_drive`/`_flush`/`_probe` worker primitives are this page's; the `EncodeAudio` arm and its `AudioResampler`/master worker compose this owner at `media/audio#MEDIA`, and the `Mux` arm composes that owner's shared `_voiced` audio-drive primitive.

## [02]-[MEDIA]

- Owner: `Media` the one container/codec encode owner discriminating modality over the closed `MediaOp` family; `MediaOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Media` subclass nor a parallel `encode_video`/`encode_audio`/`remux` function trio; `ContainerFormat` the closed `StrEnum` of muxers keyed inside the `MediaProfile` — `MP4`/`WEBM`/`GIF`/`MKV` — carrying the FFmpeg muxer name `av.open(format=)` reads and the file extension, never a parallel per-container output owner; `MediaProfile` the one frozen encode-policy value (the `ContainerFormat` muxer, the `codec` encoder name, the frame/sample `rate`, the target `bit_rate`, the `gop_size` keyframe interval, the `pix_fmt` video output format, the `layout` channel layout, the `thread_count` parallel-encode width, and the optional `master` audio-mastering slot) carrying its own `streamed` video-stream-configure projection (one `add_stream` plus the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`thread_count` codec-context fold) and its `voiced` audio-stream-configure projection (one `add_stream` plus the `layout`/`thread_count`/`bit_rate` codec-context fold, the audio arm reads), the shared `_drive` loop owning the `pts`/`time_base` per-frame stamp the encode arms read, so a new encode knob is one `MediaProfile` field bound into an existing projection, never a constructor-parameter tail nor a re-derived stream-configure call; `MediaEvidence` the typed encode receipt (container, codec, duration, byte count, frame count, bit rate) the one `MediaEvidence.measure` constructor folds once over the muxed bytes and the encoded-stream read-back; `RuntimeRail[ContentKey]` the one carrier every arm returns, keyed over the finished container bytes through `ContentIdentity.of(container.value, blob)` so an identical frame sequence at an identical profile is a cache hit by reference; the `av.open(sink, mode="w")` `OutputContainer` is the one mux capsule per encode, always a context manager so the trailer is written and the IO released, never retained across arms.
- Cases: `MediaOp` cases — `EncodeVideo(frames, profile)` (the `tuple[NDArray[np.uint8], ...]` rgb24 frame sequence `scene/render#SCENE` hands across, lifted array-by-array through `VideoFrame.from_ndarray(array, format=_FRAME_FORMAT)`, the `MediaProfile.streamed` projection minting one `VideoStream` and folding its codec-context knobs, the shared `_drive` loop stamping each frame's `pts`/`time_base`, the `stream.encode(frame)` -> `container.mux(packets)` loop driving the sequence and the `stream.encode(None)` flush muxed at end-of-stream) · `Mux(frames, samples, video, audio)` (the one interleave axis muxing a video frame sequence and an audio sample sequence into a single container — one `video.streamed` video stream plus one `audio.voiced` audio stream off the two profiles (the video profile owns the container/codec, the audio profile owns the audio codec/rate/layout/master), the `pts`-ordered `mux` interleaving each stream's packets, the dual flush at end-of-stream — never a parallel A/V-combine surface, the muxer discriminated by the typed `ContainerFormat` value the video profile carries) · `EncodeAudio(samples, profile)` (the `tuple[Pcm, ...]` sample-block sequence the `media/audio#MEDIA` arm owns, lifted through `AudioFrame.from_ndarray` (the producer dtype keying the ingest format) and resampled/reframed through `AudioResampler`, dispatched here by the shared `MediaOp` discriminant and worked by the `media/audio#MEDIA` `_encode_audio` function) — matched by one total `match`/`case`, the `EncodeVideo`/`EncodeAudio`/`Mux` modality recovered from the `MediaOp` discriminant, never a name suffix; the `EncodeVideo`/`Mux` arms own the video worker on this page and the `EncodeAudio` arm dispatches to the same-folder audio worker `media/audio#MEDIA` declares.
- Entry: `Media.encode` is `async` over the runtime `async_boundary`, dispatching the whole synchronous `av` mux loop onto the runtime subprocess lane (`anyio.to_process.run_sync`) because the `add_stream`/`encode`/`mux` body is CPU-bound C that holds the GIL, keyed by the content key the `_emit` arm derives through `ContentIdentity.of` over the finished container bytes; the worker opens one `av.open(sink, mode="w", format=profile.container.value)` `OutputContainer` against an in-memory `io.BytesIO` sink, mints the stream(s) through `MediaProfile.streamed`, runs the `_drive` encode-then-mux loop over the lifted frames, reads the muxed `bytes` plus the read-back `MediaEvidence`, and returns `(blob, evidence)` — so `EncodeVideo` via `_encode_video`, `Mux` via `_mux_av`, `EncodeAudio` via the `media/audio#MEDIA` `_encode_audio`, each a module-level function dispatched by qualified name across the subprocess lane (`to_process.run_sync` cannot target a bound method or closure). The arm keys one `ContentKey` over the single container blob through `ContentIdentity.of(profile.container.value, blob)`; the inbound `scene/render` frame-set Merkle-parent `ContentKey` is the by-reference cache key the runtime lane elides on, the muxed-output key the encode product.
- Auto: frame ingest folds each `NDArray[np.uint8]` rgb24 array into a `VideoFrame.from_ndarray(array, format=_FRAME_FORMAT)` on the worker with no PNG decode — the array is the host pixel surface PyAV admits directly; `MediaProfile.streamed` mints `container.add_stream(profile.codec, rate=profile.rate)` then folds the post-add `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size` onto the stream and `thread_count` onto its `codec_context` from the first frame's shape, dropping `None` knobs; the shared `_drive` loop stamps `frame.pts`/`frame.time_base` from the running frame index against the `rate` so the presentation timeline is monotonic; the `_drive` loop folds `stream.encode(frame)` over the sequence muxing each returned `Packet` through `container.mux`, then `_flush` flushes with `stream.encode(None)` muxed at end-of-stream, never a fictional lazy-encode iterator; the `Mux` arm runs one `streamed` video configure plus one audio stream and one interleaved `mux` over both packet streams ordered by `pts`; the muxed `bytes` read off the `BytesIO` sink plus the read-back `InputContainer` frame-count/duration probe fold through `MediaEvidence.measure` once.
- Receipt: each encode contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media` carrying the content key and the `MediaEvidence` facts — container, codec, duration, byte count, frame count, bit rate — keyed by the content key the `_emit` arm derives over the muxed bytes; `_emit` spreads `evidence.container`/`evidence.codec`/`evidence.duration`/`evidence.byte_count`/`evidence.frame_count`/`evidence.bit_rate` onto the flat-scalar `ArtifactReceipt.Media` case (the receipt owner's own `_facts` arm is the single string-map projector for the whole union), so `MediaEvidence` carries no second `facts` projection and the receipt owner imports no `MediaEvidence` value object nor any `av` container handle — the receipt-side cycle the flattening forecloses, mirroring the flat-scalar `Egress`/`Bundle` cases. The `duration` is the one observable temporal-encode value the runtime `observability/metrics` `MeterProvider` reads off the receipt fold. The `media/audio#MEDIA` `EncodeAudio` arm contributes the same single `ArtifactReceipt.Media` case, never a parallel audio-receipt rail.
- Growth: a new container is one `ContainerFormat` row carrying its muxer name; a new codec is one `MediaProfile.codec` string the `add_stream` row reads with one `pix_fmt` default (an audio encoder publishes its own sample format after `start_encoding`); a new encode knob (CRF, preset, tune, max-B-frames) is one `MediaProfile` field bound into the `streamed` codec-context fold through the `options` map; a new modality (a filter-graph transcode, a bitstream-filter remux) is one `MediaOp` case plus one acceptor arm folding the `av.filter.Graph`/`BitStreamFilterContext` the folder `.api` catalogues; a new evidence fact is one `(label, value)` row spread onto the receipt case; zero new surface — the modality space stays three cases (`EncodeVideo`/`EncodeAudio`/`Mux`) on one owner, every addition a row, field, case, or arm.

```python signature
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from artifacts.media.audio import Master

type Frames = tuple[object, ...]
type Samples = tuple[object, ...]
type MediaOpTag = Literal["encode_video", "encode_audio", "mux"]

_FRAME_FORMAT = "rgb24"


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
    layout: str = "stereo"
    thread_count: int = 0
    master: "Master | None" = None

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
        stream.codec_context.layout = self.layout  # encoder ch-layout set before start_encoding so the resampler's profile.layout frames are accepted
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
    mux: tuple[Frames, Samples, MediaProfile, MediaProfile] = case()

    @staticmethod
    def EncodeVideo(frames: Frames, profile: MediaProfile) -> "MediaOp":
        return MediaOp(encode_video=(frames, profile))

    @staticmethod
    def EncodeAudio(samples: Samples, profile: MediaProfile) -> "MediaOp":
        return MediaOp(encode_audio=(samples, profile))

    @staticmethod
    def Mux(frames: Frames, samples: Samples, video: MediaProfile, audio: MediaProfile) -> "MediaOp":
        return MediaOp(mux=(frames, samples, video, audio))


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
        return key, ArtifactReceipt.Media(key, evidence.container.value, evidence.codec, evidence.duration, evidence.byte_count, evidence.frame_count, evidence.bit_rate)

    async def _mux(self) -> tuple[bytes, MediaEvidence]:
        match self.op:
            case MediaOp(tag="encode_video", encode_video=(frames, profile)):
                return await to_process.run_sync(_encode_video, frames, profile, limiter=WORKER_BAND)
            case MediaOp(tag="encode_audio", encode_audio=(samples, profile)):
                return await to_process.run_sync(_encode_audio, samples, profile, limiter=WORKER_BAND)
            case MediaOp(tag="mux", mux=(frames, samples, video, audio)):
                return await to_process.run_sync(_mux_av, frames, samples, video, audio, limiter=WORKER_BAND)
            case _:
                assert_never(self.op)
```

```python signature
import io
from fractions import Fraction
from typing import TYPE_CHECKING

import numpy as np
from numpy.typing import NDArray

# same `artifacts.media.video` module as the owner fence; the workers stay module-level so `to_process.run_sync`
# dispatches them by qualified name, and `MediaEvidence`/`MediaProfile`/`_FRAME_FORMAT` resolve from the owner above.

lazy import av
lazy from artifacts.media.audio import _encode_audio, _voiced

if TYPE_CHECKING:
    from artifacts.media.audio import Pcm


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


def _mux_av(frames: tuple[NDArray[np.uint8], ...], blocks: tuple["Pcm", ...], video: MediaProfile, audio: MediaProfile) -> tuple[bytes, MediaEvidence]:
    height, width = frames[0].shape[:2]
    sink = io.BytesIO()
    with av.open(sink, mode="w", format=video.container.value) as container:
        vstream = video.streamed(container, width, height)
        astream = audio.voiced(container)
        container.start_encoding()  # publishes the audio frame_size before `_voiced` builds the resampler
        for index, array in enumerate(frames):
            _drive(container, vstream, av.VideoFrame.from_ndarray(array, format=_FRAME_FORMAT), index, video.rate)
        _voiced(container, astream, blocks, audio)  # shared audio drive: dtype-keyed lift, master/resample/reframe, cumulative-sample pts
        _flush(container, vstream)
        _flush(container, astream)
    blob = sink.getvalue()
    duration, count, bit_rate = _probe(blob)
    return blob, MediaEvidence.measure(video.container, video.codec, duration, count, bit_rate, blob)
```

## [03]-[RESEARCH]

- [MUXED_EVIDENCE_PROBE] [RESOLVED]: the `MediaEvidence` duration/frame-count/bit-rate facts are read by re-opening the muxed `bytes` through `av.open(io.BytesIO(blob), mode="r")` and reading `reader.duration`/`reader.streams.video[0].frames`/`video.bit_rate` (the `InputContainer.duration`/`StreamContainer.video`/`VideoStream.frames` accessors the folder `av` `.api` `[03]-[ENTRYPOINTS]` demux/streams table catalogues, rows `[04]`/`[06]`, exercised on the installed distribution returning `frames=5`/`duration=208333` for the five-frame probe), with `reader.duration / av.time_base` converting the `AV_TIME_BASE` micro-second duration to seconds; the muxed byte length is `len(blob)` off the `BytesIO` sink. The `MediaEvidence.measure` constructor folds the probe and the blob length once, mirroring the `package/codec#COMPRESSION` `BundleEvidence.measure` single-fold pattern, and `_emit` spreads the named scalars onto `ArtifactReceipt.Media(key, container, codec, duration, byte_count, frame_count, bit_rate)` so the receipt owner imports no `MediaEvidence` value object — the same flat-scalar acyclic rule the `Bundle`/`Egress` cases hold (the `media/video` owner imports `ArtifactReceipt`, so a reciprocal `receipt.py` import of `MediaEvidence` would close a module-scope cycle).
- [RECEIPT_MEDIA_CASE] [RESOLVED]: the `ArtifactReceipt.Media` case carries `tuple[ContentKey, str, str, float, int, int, int]` (key, container, codec, duration, byte count, frame count, bit rate) on the same-folder `core/receipt#RECEIPT` owner (verified present: `receipt.md` declares the `media` tag token, the `Media(key, container, codec, duration, bytes_, frames, bit_rate=0)` mint with `bit_rate` defaulting `0`, the `media=(...)` case, and the `_KEYS["media"]` row `("container", "codec", "duration", "bytes", "frames", "bit_rate")` the general `_facts` arm zips the scalar tail against), so this owner only constructs the case — `_emit` threads `evidence.bit_rate` as the seventh scalar and no widening edit lands here. The `media` token is on the union `tag` `Literal`; the `Credential` and `Media` cases are the two flat-scalar producer cases the receipt owner declared so neither `c2pa-python` nor `av` crosses into that owner, and the `Media` case is the single one the `media/video#MEDIA` and `media/audio#MEDIA` arms both contribute.
- [AUDIO_ARM_COMPOSE] [RESOLVED]: the `EncodeAudio` arm is one `MediaOp` case dispatched here but worked by the `media/audio#MEDIA` `_encode_audio` module-level function, which composes this owner's `MediaProfile.voiced` audio-stream-configure projection, the shared `_drive`/`_flush`/`_probe` worker primitives, the `MediaEvidence.measure` constructor, and the `ContainerFormat` vocabulary — never re-owning any of them. The `Mux` arm's audio leg lives here on `media/video` because the container/mux capsule is this owner's, composing `media/audio`'s shared `_voiced` audio-drive primitive over its own `audio.voiced` stream; `media/audio` owns only the `Pcm`/`_INGEST`/`Master` vocabulary, the `_voiced`/`_mastered` primitives, and the standalone `_encode_audio` arm. Both contribute the single `ArtifactReceipt.Media` case, the receipt owner citing both producers.
- [TRANSCODE_GROWTH] [RESEARCH]: a future filter-graph transcode arm (a `decode` -> `av.filter.Graph` scale/crop/overlay/fps -> `encode` -> `mux` pipeline) and a bitstream-filter remux arm (`av.BitStreamFilterContext` `demux` -> `bsf.filter` -> `mux` copy with no re-decode) are catalogued capability the `av` `.api` `[03]-[ENTRYPOINTS]` filter-graph table (`Graph.add_buffer`/`Graph.add`/`Graph.link_nodes`/`Graph.push`/`Graph.pull`) and bitstream-filter rows (`BitStreamFilterContext`/`BitStreamFilterContext.filter`) carry, landing as one `MediaOp` case plus one acceptor arm each when a transcode/remux consumer materializes — the modality space stays the three encode cases (`EncodeVideo`/`EncodeAudio`/`Mux`) until then, the growth a row on the existing owner, never a parallel transcoder package. Close-condition: a transcode/remux consumer page declares the seam, at which point the `Filter`/`Remux` case folds into the `MediaOp` `match`.
