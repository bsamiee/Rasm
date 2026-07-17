# [PY_ARTIFACTS_MEDIA_AUDIO]

Temporal-artifact audio owner: decode, encode, resample, layout, and mix over the `av` (PyAV) FFmpeg floor, composing the `media/container#CONTAINER` container spine and the `media/filtergraph#FILTER` `AudioGraph` capsule rather than a second `Media` owner or a local `av.filter.Graph` builder. `_encode_audio` encodes a `tuple[Pcm, ...]` producer PCM-block sequence into one single-stream audio container (FLAC/OGG/WAV or an AAC/Opus delivery container); `_decode_audio` is its inverse and `_mix_audio` the N-source combine, both standalone primitives on the same `MediaFault` rail the `media/timeline#TIMELINE`/`media/analysis#ANALYSIS` pages compose. This page owns the `Pcm` producer-dtype union, the `_INGEST` ingest-format table, the `StageKind`/`_STAGE`/`Stage`/`Master` mastering-and-layout vocabulary, and the `_lift`/`_mastered`/`_voiced`/`_encode_audio`/`_mix_audio`/`_decode_audio` primitives.

It composes the `media/container#CONTAINER` `Media`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family and the `_codec_ok`/`_drive`/`_flush`/`_deployment`/`_media_fault` worker primitives, reading `MediaProfile.voiced` and the `layout` FFmpeg layout-name slot, and composes the `media/filtergraph#FILTER` `AudioGraph.of(AudioGraphSpec).frames` capsule for every filter chain, opening no `av.filter.Graph`. `Stage`'s vocabulary is one primary correspondence — the closed `StageKind` family keyed into the `_STAGE` grammar table (template + typed defaults per libavfilter name) — and `Stage(kind, **knobs)` admits only row-owned knobs while its `name` and `args` projections derive from that row, so an open `(name, args)` bag cannot enter `Master`. One `av.AudioResampler(frame_size=ctx.frame_size or None)` is the single audio-DSP owner per encode — it converts each mastered block to the encoder's format/rate/layout AND reframes it to the exact `frame_size`, folding the former `AudioFifo` rebuffer into one constructor argument. `EncodeAudio` is one `MediaOp` arm the container `_mux` dispatches through `Media.lane.offload(Kernel.of(_encode_audio, KernelTrait.HOSTILE), ...)`, worked here, and contributes the same single `core/receipt#RECEIPT` `ArtifactReceipt.Media` case, never a parallel audio-receipt rail. Audio only APPLIES `loudnorm` at the EBU R128 target; the measured integrated-LUFS / true-peak-dBTP / loudness-range band is `media/analysis#ANALYSIS`'s two-pass read, never a second-pass measure here.

## [01]-[INDEX]

- [01]-[MEDIA]: the audio arm of the closed-payload `MediaOp` family — the `_encode_audio` worker plus the standalone railed `_decode_audio`/`_mix_audio` primitives — lifting, mastering, and resampling a `tuple[Pcm, ...]` sequence into one audio container keyed over the muxed bytes.

## [02]-[MEDIA]

- Owner: the audio arm of the `media/container#CONTAINER` `MediaOp` family, worked by `_encode_audio` plus the standalone `_decode_audio`/`_mix_audio` primitives; it composes the container `Media` owner (reading `voiced` and the `master` slot), the `MediaEvidence`/`ContainerFormat`/`_codec_ok`/`_drive`/`_flush`/`_deployment`/`MediaFault` surface, and the filtergraph `AudioGraph.of(AudioGraphSpec).frames` capsule, opening no `av.filter.Graph` locally; it OWNS the `Pcm` dtype union, the `_INGEST` table, and the `StageKind`/`_STAGE`/`Stage`/`Master` vocabulary — the `_STAGE` grammar table is the one primary correspondence (filter name -> arg template + typed defaults), while `Stage(kind, **knobs)` rejects every knob the active row does not own and derives the provider spelling. One `av.AudioResampler(frame_size=ctx.frame_size or None)` is the one audio-DSP owner per encode, folding the former `AudioFifo` rebuffer into its `frame_size` argument; the `av.open` `OutputContainer` is one mux capsule per encode, always a context manager.
- Cases: `_encode_audio(blocks, profile)` lifts each block through `_lift`, drives it through the optional `AudioGraphSpec.master` chain, then one `AudioResampler` (`frame_size or None` yielding natural frames for FLAC/PCM and fixed `frame_size` chunks for AAC/Opus), mints the stream through `voiced`, and drives each chunk at its cumulative-sample `pts` — dispatched by the container `_mux` on the `MediaOp` discriminant; `_mix_audio(sources, profile, weights)` derives one ingest format per source and performs the N-source `amix` combine over `AudioGraphSpec.mix`, returning `Result[tuple[Pcm, ...], MediaFault]` the `timeline`/`analysis`/encode consumers compose; `_decode_audio(blob)` the inverse, demux+decode one audio stream to interleaved-PCM blocks + the source rate on the same rail — planar codec output (AAC/Opus `fltp`) resampled to its `AudioFormat.packed` twin so the `Pcm` contract holds for every codec — refusing a blob with no audio stream as `invalid`. Layout/channel conversion rides the `StageKind.AFORMAT`/`PAN`/`CHANNELMAP` rows or the `AudioResampler` `layout` param, never a fourth worker; the `Mux` arm's audio leg lives on `media/container#CONTAINER`, composing this page's `_voiced` over its own `audio.voiced` stream.
- Entry: `_encode_audio` is the worker-arm function the container `_mux` `EncodeAudio` case dispatches through `Media._crossed` — the `execution/lanes#LANE` instance offload `self.lane.offload(Kernel.of(_encode_audio, KernelTrait.HOSTILE), samples, profile)` shipping the module-level function `REFERENCE` by qualified name, the whole synchronous open/master/resample/encode/mux loop GIL-holding C; it guards the empty sequence and both codec probes (`_codec_ok` registry, `supported_codecs` muxer), opens one `OutputContainer` against a `BytesIO` sink, stamps `profile.metadata`, mints the `AudioStream` through `voiced`, opens the encoder through `start_encoding`, threads every block through `_voiced`, `_flush`es, and returns `Result[(blob, MediaEvidence), MediaFault]` mapping the `av.error.FFmpegError` leaves through `_media_fault` and an `ImportError` onto `provision`, railing identically to the `_encode_video`/`_transcode`/`_remux` siblings; the container `_emit` arm keys the `ContentKey`, this worker minting none. `_mix_audio`/`_decode_audio` produce railed `Pcm` blocks their composing arm threads without a second capture.
- Auto: `_drive` stamps each chunk's `pts` as the cumulative output-sample offset in the `1/rate` base, never the chunk index, so the presentation timeline is sample-accurate; `MediaEvidence.measure` reports `samples / rate` as the exact temporal extent with no video read-back, the audio-only container carrying no video stream; `Stage.args` lowers a bool knob to the FFmpeg `true`/`false` spelling and formats the row template over defaults-plus-overrides in one pass.
- Receipt: each audio encode contributes the same single `core/receipt#RECEIPT` `ArtifactReceipt.Media` case the container owner contributes, keyed over the muxed bytes at the container `_emit` arm; the worker folds `(container, codec, samples / rate, frames, bit_rate, blob, _deployment)` through `MediaEvidence.measure` once, the receipt owner importing no `MediaEvidence` value nor `av` handle. `_mix_audio`/`_decode_audio` produce `Pcm` blocks and mint no receipt — their composing arm keys and contributes the one `Media` case.
- Growth: a new producer dtype is one `_INGEST` row; a new codec one `MediaProfile.codec` string, the resample/reframe pipeline already adapting; a differing producer rate one `frame.rate` stamp before the `AudioResampler`; a new encode knob one `MediaProfile` field in the `voiced` fold; a new mastering stage one `StageKind` member plus one `_STAGE` row; a louder chain one more ordered `Stage` value in `Master.stages`; a channel conversion one `AFORMAT`/`PAN`/`CHANNELMAP` stage or one `profile.layout` name; a new mix policy one `weights` value; a new evidence fact one `Media.facts` band key — every addition a row, member, field, or capsule argument, never a local `av.filter.Graph`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import math
from dataclasses import dataclass
from enum import StrEnum
from itertools import chain
from typing import TYPE_CHECKING

import numpy as np
from builtins import frozendict
from expression import Error, Ok, Result
from msgspec import Struct
from numpy.typing import NDArray

# media/container#CONTAINER owns the shared Media family + worker primitives; media/filtergraph#FILTER owns the
# AudioGraph capsule audio composes for every filter chain — no `av.filter.Graph` is opened here.
from rasm.artifacts.media.container import MediaEvidence, MediaFault, MediaProfile, _codec_ok, _deployment, _drive, _flush, _media_fault, _worker
from rasm.artifacts.media.filtergraph import AudioGraph, AudioGraphSpec

lazy import av
lazy import av.error

if TYPE_CHECKING:
    from collections.abc import Iterator

# --- [TYPES] ----------------------------------------------------------------------------

type Pcm = NDArray[np.int16] | NDArray[np.int32] | NDArray[np.float32] | NDArray[np.float64]

# channel layout rides `MediaProfile.layout` as the FFmpeg layout NAME (`str`): `av` rejects a `StrEnum` member, so it
# is an open av-name field like `codec`/`pix_fmt`; an arbitrary remap rides a PAN/CHANNELMAP stage row.


class StageKind(StrEnum):
    # closed mastering-and-layout vocabulary: each member IS the libavfilter name its `_STAGE` row renders args for.
    LOUDNORM = "loudnorm"
    HIGHPASS = "highpass"
    LOWPASS = "lowpass"
    DYNAUDNORM = "dynaudnorm"
    ACOMPRESSOR = "acompressor"
    ALIMITER = "alimiter"
    COMPAND = "compand"
    AECHO = "aecho"
    APHASER = "aphaser"
    ASUBBOOST = "asubboost"
    AFADE = "afade"
    ATEMPO = "atempo"
    AFORMAT = "aformat"
    PAN = "pan"
    CHANNELMAP = "channelmap"


# --- [CONSTANTS] ------------------------------------------------------------------------

# producer numpy dtype -> av packed sample format; `block.dtype` selects the `from_ndarray`
# ingest tag, so int16/int32/float32/float64 interleaved PCM all admit with no per-dtype arm.
_INGEST: frozendict[np.dtype, str] = frozendict({
    np.dtype(np.int16): "s16",
    np.dtype(np.int32): "s32",
    np.dtype(np.float32): "flt",
    np.dtype(np.float64): "dbl",
})

# --- [MODELS] ---------------------------------------------------------------------------


class StageRow(Struct, frozen=True):
    template: str
    defaults: frozendict[str, float | int | str | bool]


# ONE primary correspondence: StageKind -> (arg template, typed knob defaults). Every stage's grammar — the
# key=value chains, aecho's positional colons, pan's pipe join, compand's dashed key — is a row, and `Stage` is the
# single constructor over it; a new stage is one member plus one row, never a new factory.
_STAGE: frozendict[StageKind, StageRow] = frozendict({
    StageKind.LOUDNORM: StageRow(
        "I={integrated}:TP={true_peak}:LRA={loudness_range}:linear={linear}:dual_mono={dual_mono}",
        frozendict({"integrated": -16.0, "true_peak": -1.5, "loudness_range": 11.0, "linear": True, "dual_mono": False}),
    ),
    StageKind.HIGHPASS: StageRow("f={frequency}", frozendict({"frequency": 20.0})),
    StageKind.LOWPASS: StageRow("f={frequency}", frozendict({"frequency": 20000.0})),
    StageKind.DYNAUDNORM: StageRow(
        "framelen={frame_len}:gausssize={gauss_size}:peak={peak}:maxgain={max_gain}",
        frozendict({"frame_len": 500, "gauss_size": 31, "peak": 0.95, "max_gain": 10.0}),
    ),
    StageKind.ACOMPRESSOR: StageRow(
        "threshold={threshold}:ratio={ratio}:attack={attack}:release={release}:makeup={makeup}",
        frozendict({"threshold": 0.125, "ratio": 2.0, "attack": 20.0, "release": 250.0, "makeup": 1.0}),
    ),
    StageKind.ALIMITER: StageRow("limit={limit}:attack={attack}:release={release}", frozendict({"limit": 0.95, "attack": 5.0, "release": 50.0})),
    StageKind.COMPAND: StageRow(
        "attacks={attacks}:decays={decays}:points={points}:soft-knee={soft_knee}:gain={gain}",
        frozendict({"attacks": "0.3", "decays": "0.8", "points": "-70/-70|-60/-20|0/0", "soft_knee": 6.0, "gain": 0.0}),
    ),
    StageKind.AECHO: StageRow("{in_gain}:{out_gain}:{delays}:{decays}", frozendict({"in_gain": 0.6, "out_gain": 0.3, "delays": "1000", "decays": "0.5"})),
    StageKind.APHASER: StageRow(
        "in_gain={in_gain}:out_gain={out_gain}:delay={delay}:decay={decay}:speed={speed}:type={kind}",
        frozendict({"in_gain": 0.4, "out_gain": 0.74, "delay": 3.0, "decay": 0.4, "speed": 0.5, "kind": "triangular"}),
    ),
    StageKind.ASUBBOOST: StageRow(
        "dry={dry}:wet={wet}:decay={decay}:feedback={feedback}:cutoff={cutoff}:slope={slope}:delay={delay}",
        frozendict({"dry": 0.7, "wet": 0.7, "decay": 0.7, "feedback": 0.5, "cutoff": 100.0, "slope": 0.5, "delay": 20.0}),
    ),
    StageKind.AFADE: StageRow(
        "type={kind}:start_time={start}:duration={duration}:curve={curve}",
        frozendict({"kind": "in", "start": 0.0, "duration": 1.0, "curve": "tri"}),
    ),
    StageKind.ATEMPO: StageRow("tempo={factor}", frozendict({"factor": 1.0})),
    StageKind.AFORMAT: StageRow(
        "sample_fmts={sample_fmt}:sample_rates={sample_rate}:channel_layouts={layout}",
        frozendict({"sample_fmt": "fltp", "sample_rate": 48000, "layout": "stereo"}),
    ),
    StageKind.PAN: StageRow("{layout}|{spec}", frozendict({"layout": "stereo", "spec": "c0=c0|c1=c1"})),
    StageKind.CHANNELMAP: StageRow("map={mapping}:channel_layout={layout}", frozendict({"mapping": "FL-FL|FR-FR", "layout": "stereo"})),
})


@dataclass(frozen=True, slots=True, init=False)
class Stage:
    kind: StageKind
    knobs: frozendict[str, float | int | str | bool] = frozendict()

    def __init__(self, kind: StageKind, /, **knobs: float | int | str | bool) -> None:
        if unknown := knobs.keys() - _STAGE[kind].defaults.keys():
            raise ValueError(f"stage {kind}: unknown knobs {sorted(unknown)}")
        object.__setattr__(self, "kind", kind)
        object.__setattr__(self, "knobs", frozendict(knobs))

    @property
    def name(self) -> str:
        return self.kind.value

    @property
    def args(self) -> str:
        row = _STAGE[self.kind]
        merged = {name: str(value).lower() if isinstance(value, bool) else value for name, value in (row.defaults | self.knobs).items()}
        return row.template.format(**merged)


class Master(Struct, frozen=True):
    # ordered mastering-and-layout chain `AudioGraphSpec.master` links into one abuffer -> stages -> abuffersink graph;
    # default is EBU R128 `loudnorm`, a louder chain composes more ordered rows.
    stages: tuple[Stage, ...] = (Stage(StageKind.LOUDNORM),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _lift(block: Pcm, ingest: str, profile: MediaProfile) -> "av.AudioFrame":
    # one producer PCM block -> one av AudioFrame with the input timeline stamped; the dtype-keyed `ingest` tag
    # keeps the lift total over the four interleaved-PCM dtypes with no per-dtype arm — the packed
    # (1, samples*channels) rank/channel shape is admission-proven against `profile.layout` before any block lands here.
    frame = av.AudioFrame.from_ndarray(block, format=ingest, layout=profile.layout)
    frame.rate = profile.rate
    return frame


def _mastered(graph: AudioGraph | None, blocks: tuple[Pcm, ...], profile: MediaProfile, ingest: str) -> "Iterator[av.AudioFrame]":
    # yield lifted frames through the master capsule, or raw when no master is set; `_voiced` owns the provider's
    # terminal `resample(None)` flush at the resampler boundary.
    frames = (_lift(block, ingest, profile) for block in blocks)
    yield from graph.frames(frames) if graph is not None else frames


def _voiced(container: object, stream: object, blocks: tuple[Pcm, ...], profile: MediaProfile) -> tuple[int, int]:
    # shared audio drive `_mux_av` also composes: lift -> master -> resample/reframe -> encode -> mux over one
    # already-opened audio stream, returning (frames, samples). The caller opens the encoder, flushes, and reads the bytes.
    ctx = stream.codec_context
    ingest = _INGEST[blocks[0].dtype]
    graph = (
        AudioGraph.of(AudioGraphSpec(master=(profile.rate, ingest, profile.layout, tuple((stage.name, stage.args) for stage in profile.master.stages))))
        if profile.master is not None
        else None
    )
    # frame_size folds the AudioFifo rebuffer into the resampler: one owner converts format/rate/layout and emits ctx.frame_size-sample frames
    resampler = av.AudioResampler(format=ctx.format.name, layout=profile.layout, rate=ctx.rate, frame_size=ctx.frame_size or None)
    frames = samples = 0
    for shaped in chain(_mastered(graph, blocks, profile, ingest), (None,)):
        for chunk in resampler.resample(shaped):
            _drive(container, stream, chunk, samples, ctx.rate)  # pts is the cumulative output-sample offset in 1/rate, not the chunk index
            frames, samples = frames + 1, samples + chunk.samples
    return frames, samples


@_worker
def _encode_audio(blocks: tuple[Pcm, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # Pcm is a real runtime union here (unlike the container's TYPE_CHECKING-only forward ref), so `_worker` catches
    # a beartype hint violation and returns `MediaFault.contract` before the process crossing completes.
    try:
        if not blocks:
            return Error(MediaFault(invalid="empty pcm sequence"))
        if any(block.dtype != blocks[0].dtype for block in blocks):
            return Error(MediaFault(invalid="one pcm sequence must keep one producer dtype"))
        if blocks[0].dtype not in _INGEST:  # membership against the closed _INGEST vocabulary, so int8/uint8/float16 rail instead of KeyError
            return Error(MediaFault(invalid=f"unsupported pcm dtype {blocks[0].dtype}"))
        channels = av.AudioLayout(profile.layout).nb_channels
        if any(block.ndim != 2 or block.shape[0] != 1 or block.shape[1] % channels for block in blocks):
            return Error(MediaFault(invalid=f"pcm blocks must be packed (1, samples*channels) for layout {profile.layout}"))
        if profile.container.segmented or profile.segment is not None:
            return Error(MediaFault(invalid="audio encode requires a single-blob container profile"))
        if not _codec_ok(profile.codec):
            return Error(MediaFault(unregistered=("codecs_available", profile.codec)))
        sink = io.BytesIO()
        with av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as container:
            if profile.codec not in container.supported_codecs:
                return Error(MediaFault(unregistered=("supported_codecs", profile.codec)))
            container.metadata.update(dict(profile.metadata))
            stream = profile.voiced(container)
            container.start_encoding()  # opens the encoder so codec_context.frame_size/format publish real values, not 0/unset
            frames, samples = _voiced(container, stream, blocks, profile)
            rate = stream.codec_context.rate
            sample_format = stream.codec_context.format.name
            _flush(container, stream)
        blob = sink.getvalue()
        return Ok((
            blob,
            MediaEvidence.measure(
                profile.container,
                profile.codec,
                samples / rate if rate else 0.0,
                frames,
                int(profile.bit_rate or 0),
                blob,
                _deployment(profile)
                | {"sample_rate": float(rate), "layout": profile.layout, "sample_format": sample_format}
                | ({"master_stages": float(len(profile.master.stages))} if profile.master is not None else {}),
            ),
        ))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:  # first: av.error.ValueError subclasses both and keeps its FFmpeg taxonomy
        return Error(_media_fault("encode_audio", exc))
    except ValueError as exc:  # a provider-side frame-shape refusal lands as the same invalid case admission mints
        return Error(MediaFault(invalid=str(exc)))


def _mixed(sources: tuple[tuple[Pcm, ...], ...], profile: MediaProfile, ingests: tuple[str, ...], weights: tuple[float, ...]) -> "Iterator[Pcm]":
    # hand N per-source lifted-frame streams to `AudioGraph.frames`, which owns the per-context abuffer push, the
    # amix link_to wiring, and the flush, yielding each mixed frame before `_mix_audio` collects its terminal tuple.
    graph = AudioGraph.of(AudioGraphSpec(mix=(profile.rate, ingests, profile.layout, weights)))
    streams = tuple((_lift(block, ingest, profile) for block in blocks) for blocks, ingest in zip(sources, ingests, strict=True))
    for frame in graph.frames(streams):
        yield frame.to_ndarray()


def _mix_audio(sources: tuple[tuple[Pcm, ...], ...], profile: MediaProfile, weights: tuple[float, ...] = ()) -> Result[tuple[Pcm, ...], MediaFault]:
    # standalone N-source combine `timeline`/`analysis` and the encode arm compose: mixed Pcm blocks on the one
    # MediaFault rail, so an empty source set and a provider raise are each structurally addressable at every seam.
    # `weights` empty = equal mix.
    try:
        if not sources or not all(sources):
            return Error(MediaFault(invalid="empty mix source"))
        if weights and len(weights) != len(sources):
            return Error(MediaFault(invalid="mix weights must be empty or match source count"))
        if not all(math.isfinite(weight) for weight in weights):  # finiteness precedes the amix format — a nan/inf weight poisons every mixed frame
            return Error(MediaFault(invalid="mix weights must be finite"))
        if any(any(block.dtype != source[0].dtype for block in source) for source in sources):
            return Error(MediaFault(invalid="each mix source must keep one producer dtype"))
        if any(source[0].dtype not in _INGEST for source in sources):  # same closed-vocabulary admission the encode arm runs
            return Error(MediaFault(invalid="unsupported pcm dtype in mix source"))
        channels = av.AudioLayout(profile.layout).nb_channels
        if any(block.ndim != 2 or block.shape[0] != 1 or block.shape[1] % channels for source in sources for block in source):
            return Error(MediaFault(invalid=f"pcm blocks must be packed (1, samples*channels) for layout {profile.layout}"))
        ingests = tuple(_INGEST[source[0].dtype] for source in sources)
        return Ok(tuple(_mixed(sources, profile, ingests, weights)))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:  # first: av.error.ValueError subclasses both and keeps its FFmpeg taxonomy
        return Error(_media_fault("mix_audio", exc))
    except ValueError as exc:  # a provider-side frame-shape refusal lands as the same invalid case admission mints
        return Error(MediaFault(invalid=str(exc)))


def _decoded(reader: object, voice: object) -> "Iterator[Pcm]":
    # planar decode output (AAC/Opus decode fltp) normalizes to its `AudioFormat.packed` twin so every yielded block
    # honours the interleaved `Pcm` contract; the terminal None flushes the resampler tail.
    ctx = voice.codec_context
    packed = av.AudioResampler(format=ctx.format.packed, layout=ctx.layout, rate=ctx.rate)
    for frame in chain(reader.decode(audio=0), (None,)):
        for chunk in packed.resample(frame):
            yield chunk.to_ndarray()


def _decode_audio(blob: bytes) -> Result[tuple[tuple[Pcm, ...], int, str], MediaFault]:
    # inverse of `_encode_audio`: demux + decode one audio stream to interleaved-PCM blocks + the source sample
    # rate + the CHANNEL-LAYOUT name on the shared rail — interleaved multichannel PCM stays distinguishable from
    # mono by contract — so a malformed blob or a video-only container is a typed fault at the composing
    # `timeline`/`analysis` seam, never a raw raise.
    try:
        with av.open(io.BytesIO(blob), mode="r") as reader:
            voice = next(iter(reader.streams.audio), None)
            if voice is None:
                return Error(MediaFault(invalid="no audio stream"))
            blocks = tuple(_decoded(reader, voice))
            if not blocks:  # a stream that decodes zero blocks is invalid media — a synthetic silent sample downstream would fake evidence
                return Error(MediaFault(invalid="audio stream decoded no samples"))
            return Ok((blocks, voice.sample_rate, voice.layout.name))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("decode_audio", exc))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
