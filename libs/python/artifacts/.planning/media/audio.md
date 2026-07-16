# [PY_ARTIFACTS_MEDIA_AUDIO]

Temporal-artifact audio owner: decode, encode, resample, layout, and mix over the `av` (PyAV) FFmpeg floor, composing the `media/container#CONTAINER` container spine and the `media/filtergraph#FILTER` `AudioGraph` capsule rather than a second `Media` owner or a local `av.filter.Graph` builder. `_encode_audio` encodes a `tuple[Pcm, ...]` producer PCM-block sequence into one single-stream audio container; `_decode_audio` is its inverse and `_mix_audio` the N-source combine, both standalone primitives the `media/timeline#TIMELINE`/`media/analysis#ANALYSIS` pages compose. This page owns the `Pcm` producer-dtype union, the `_INGEST` ingest-format table, the `Stage`/`Master` mastering-and-layout vocabulary, and the `_lift`/`_mastered`/`_voiced`/`_encode_audio`/`_mix_audio`/`_decode_audio` primitives.

It composes the `media/container#CONTAINER` `Media`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family and the `_drive`/`_flush`/`_deployment`/`_media_fault` worker primitives, reading `MediaProfile.voiced` and the `layout` FFmpeg layout-name slot, and composes the `media/filtergraph#FILTER` `AudioGraph.master`/`AudioGraph.mix` capsules for every filter chain, opening no `av.filter.Graph`. One `av.AudioResampler(frame_size=ctx.frame_size or None)` is the single audio-DSP owner per encode — it converts each mastered block to the encoder's format/rate/layout AND reframes it to the exact `frame_size`, folding the former `AudioFifo` rebuffer into one constructor argument. `EncodeAudio` is one `MediaOp` arm the container `_mux` dispatches over the process lane, worked here, and contributes the same single `core/receipt#RECEIPT` `ArtifactReceipt.Media` case, never a parallel audio-receipt rail. Audio only APPLIES `loudnorm` at the EBU R128 target; the measured integrated-LUFS / true-peak-dBTP / loudness-range band is `media/analysis#ANALYSIS`'s two-pass read, never a second-pass measure here.

## [01]-[INDEX]

- [01]-[MEDIA]: the audio arm of the closed-payload `MediaOp` family — the `_encode_audio` worker plus the standalone `_decode_audio`/`_mix_audio` primitives — lifting, mastering, and resampling a `tuple[Pcm, ...]` sequence into one audio container keyed over the muxed bytes.

## [02]-[MEDIA]

- Owner: the audio arm of the `media/container#CONTAINER` `MediaOp` family, worked by `_encode_audio` plus the standalone `_decode_audio`/`_mix_audio` primitives; it composes the container `Media` owner (reading `voiced` and the `master` slot), the `MediaEvidence`/`ContainerFormat`/`_drive`/`_flush`/`_deployment`/`MediaFault` surface, and the filtergraph `AudioGraph.master`/`mix` capsules, opening no `av.filter.Graph` locally; it OWNS the `Pcm` dtype union, the `_INGEST` table, the `Stage`/`Master` vocabulary (each `Stage` a typed-knob-to-`key=value` derivation over one verified `libavfilter` name), and the encode/mix/decode primitives. One `av.AudioResampler(frame_size=ctx.frame_size or None)` is the one audio-DSP owner per encode, folding the former `AudioFifo` rebuffer into its `frame_size` argument; the `av.open` `OutputContainer` is one mux capsule per encode, always a context manager.
- Cases: `_encode_audio(blocks, profile)` lifts each block through `_lift`, drives it through the optional `AudioGraph.master` chain, then one `AudioResampler` (`frame_size or None` yielding natural frames for FLAC/PCM and fixed `frame_size` chunks for AAC/Opus), mints the stream through `voiced`, and drives each chunk at its cumulative-sample `pts` — dispatched by the container `_mux` on the `MediaOp` discriminant; `_mix_audio(sources, profile, weights)` the N-source `amix` combine over `AudioGraph.mix`, returning mixed `Pcm` blocks the `timeline`/`analysis`/encode consumers compose; `_decode_audio(blob)` the inverse, demux+decode one audio stream to interleaved-PCM blocks + the source rate, lazily through `_decoded`. Layout/channel conversion rides the `Stage` `aformat`/`pan`/`channelmap` nodes or the `AudioResampler` `layout` param, never a fourth worker; the `Mux` arm's audio leg lives on `media/container#CONTAINER`, composing this page's `_voiced` over its own `audio.voiced` stream.
- Entry: `_encode_audio` is the worker-arm function the container `_mux` `EncodeAudio` case dispatches over `to_process.run_sync(..., limiter=WORKER_BAND)` — a module-level function the process lane targets by qualified name (never a bound method or closure), the whole synchronous open/master/resample/encode/mux loop GIL-holding C; it opens one `OutputContainer` against a `BytesIO` sink, mints the `AudioStream` through `voiced`, opens the encoder through `start_encoding`, threads every block through `_voiced`, `_flush`es, and returns `Result[(blob, MediaEvidence), MediaFault]` mapping the `av.error.FFmpegError` leaves through `_media_fault` and an `ImportError` onto `provision`, railing identically to the `_encode_video`/`_transcode`/`_remux` siblings; the container `_emit` arm keys the `ContentKey`, this worker minting none. `_mix_audio`/`_decode_audio` produce `Pcm` blocks and rail at their composing arm's own `to_process` seam.
- Auto: `_drive` stamps each chunk's `pts` as the cumulative output-sample offset in the `1/rate` base, never the chunk index, so the presentation timeline is sample-accurate; `MediaEvidence.measure` reports `samples / rate` as the exact temporal extent with no video read-back, the audio-only container carrying no video stream.
- Receipt: each audio encode contributes the same single `core/receipt#RECEIPT` `ArtifactReceipt.Media` case the container owner contributes, keyed over the muxed bytes at the container `_emit` arm; the worker folds `(container, codec, samples / rate, frames, bit_rate, blob, _deployment)` through `MediaEvidence.measure` once, the receipt owner importing no `MediaEvidence` value nor `av` handle. `_mix_audio`/`_decode_audio` produce `Pcm` blocks and mint no receipt — their composing arm keys and contributes the one `Media` case.
- Growth: a new producer dtype is one `_INGEST` row; a new codec one `MediaProfile.codec` string, the resample/reframe pipeline already adapting; a differing producer rate one `frame.rate` stamp before the `AudioResampler`; a new encode knob one `MediaProfile` field in the `voiced` fold; a louder chain one ordered `Stage` in `Master.stages`; a channel conversion one `Stage.aformat`/`pan`/`channelmap` or one `profile.layout` name; a new mix policy one `weights` argument; a new evidence fact one `Media.facts` band key — every addition a row, field, `Stage`, or capsule argument, never a local `av.filter.Graph`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Iterable, Iterator
from typing import Literal

import numpy as np
from beartype import beartype
from expression import Error, Ok, Result
from msgspec import Struct
from numpy.typing import NDArray


# media/container#CONTAINER owns the shared Media family + worker primitives; media/filtergraph#FILTER owns the
# AudioGraph capsule audio composes for every filter chain — no `av.filter.Graph` is opened here.
from artifacts.media.container import MediaEvidence, MediaFault, MediaProfile, _deployment, _drive, _flush, _media_fault
from artifacts.media.filtergraph import AudioGraph

lazy import av
lazy import av.error

# --- [TYPES] ----------------------------------------------------------------------------

type Pcm = NDArray[np.int16] | NDArray[np.int32] | NDArray[np.float32] | NDArray[np.float64]

# channel layout rides `MediaProfile.layout` as the FFmpeg layout NAME (`str`): `av` rejects a `StrEnum` member, so it
# is an open av-name field like `codec`/`pix_fmt`; an arbitrary remap rides a `Stage.pan`/`Stage.channelmap` node.

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


class Stage(Struct, frozen=True):
    # one libavfilter node the `AudioGraph` links and routes through `filters_available`; each smart constructor
    # derives its `key=value:...` arg string from typed knobs, the filter NAME the logical op the owner routes.
    name: str
    args: str = ""

    # --- mastering (loudness / dynamics) ---
    @staticmethod
    def loudnorm(
        integrated: float = -16.0, true_peak: float = -1.5, loudness_range: float = 11.0, linear: bool = True, dual_mono: bool = False
    ) -> "Stage":
        return Stage(
            "loudnorm", f"I={integrated}:TP={true_peak}:LRA={loudness_range}:linear={str(linear).lower()}:dual_mono={str(dual_mono).lower()}"
        )

    @staticmethod
    def highpass(frequency: float = 20.0) -> "Stage":
        return Stage("highpass", f"f={frequency}")

    @staticmethod
    def lowpass(frequency: float = 20000.0) -> "Stage":
        return Stage("lowpass", f"f={frequency}")

    @staticmethod
    def dynaudnorm(frame_len: int = 500, gauss_size: int = 31, peak: float = 0.95, max_gain: float = 10.0) -> "Stage":
        return Stage("dynaudnorm", f"framelen={frame_len}:gausssize={gauss_size}:peak={peak}:maxgain={max_gain}")

    @staticmethod
    def acompressor(threshold: float = 0.125, ratio: float = 2.0, attack: float = 20.0, release: float = 250.0, makeup: float = 1.0) -> "Stage":
        return Stage("acompressor", f"threshold={threshold}:ratio={ratio}:attack={attack}:release={release}:makeup={makeup}")

    @staticmethod
    def alimiter(limit: float = 0.95, attack: float = 5.0, release: float = 50.0) -> "Stage":
        return Stage("alimiter", f"limit={limit}:attack={attack}:release={release}")

    @staticmethod
    def compand(attacks: str = "0.3", decays: str = "0.8", points: str = "-70/-70|-60/-20|0/0", soft_knee: float = 6.0, gain: float = 0.0) -> "Stage":
        return Stage("compand", f"attacks={attacks}:decays={decays}:points={points}:soft-knee={soft_knee}:gain={gain}")

    # --- colour / space (reverb / phase / sub) ---
    @staticmethod
    def aecho(in_gain: float = 0.6, out_gain: float = 0.3, delays: str = "1000", decays: str = "0.5") -> "Stage":
        return Stage("aecho", f"{in_gain}:{out_gain}:{delays}:{decays}")

    @staticmethod
    def aphaser(
        in_gain: float = 0.4, out_gain: float = 0.74, delay: float = 3.0, decay: float = 0.4, speed: float = 0.5, kind: str = "triangular"
    ) -> "Stage":
        return Stage("aphaser", f"in_gain={in_gain}:out_gain={out_gain}:delay={delay}:decay={decay}:speed={speed}:type={kind}")

    @staticmethod
    def asubboost(
        dry: float = 0.7, wet: float = 0.7, decay: float = 0.7, feedback: float = 0.5, cutoff: float = 100.0, slope: float = 0.5, delay: float = 20.0
    ) -> "Stage":
        return Stage("asubboost", f"dry={dry}:wet={wet}:decay={decay}:feedback={feedback}:cutoff={cutoff}:slope={slope}:delay={delay}")

    # --- timeline ---
    @staticmethod
    def afade(kind: Literal["in", "out"] = "in", start: float = 0.0, duration: float = 1.0, curve: str = "tri") -> "Stage":
        return Stage("afade", f"type={kind}:start_time={start}:duration={duration}:curve={curve}")

    @staticmethod
    def atempo(factor: float = 1.0) -> "Stage":
        return Stage("atempo", f"tempo={factor}")

    # --- layout / channel conversion ---
    @staticmethod
    def aformat(sample_fmt: str = "fltp", sample_rate: int = 48000, layout: str = "stereo") -> "Stage":
        return Stage("aformat", f"sample_fmts={sample_fmt}:sample_rates={sample_rate}:channel_layouts={layout}")

    @staticmethod
    def pan(layout: str = "stereo", spec: str = "c0=c0|c1=c1") -> "Stage":
        # arbitrary channel down/up-mix and per-channel gain the resampler's `layout` cannot express (`pan=stereo|c0=0.5*c0|c1=0.5*c1`)
        return Stage("pan", f"{layout}|{spec}")

    @staticmethod
    def channelmap(mapping: str = "FL-FL|FR-FR", layout: str = "stereo") -> "Stage":
        # a fixed source-to-output channel reorder/relabel (`channelmap=map=FL-FL|FR-FR:channel_layout=stereo`)
        return Stage("channelmap", f"map={mapping}:channel_layout={layout}")


class Master(Struct, frozen=True):
    # the ordered mastering-and-layout chain `AudioGraph.master` links into one abuffer -> stages -> abuffersink graph;
    # the default is EBU R128 `loudnorm`, a louder chain composes more ordered `Stage`s.
    stages: tuple[Stage, ...] = (Stage.loudnorm(),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _lift(block: Pcm, ingest: str, profile: MediaProfile) -> "av.AudioFrame":
    # one producer PCM block -> one av AudioFrame with the input timeline stamped; the dtype-keyed `ingest` tag
    # keeps the lift total over the four interleaved-PCM dtypes with no per-dtype arm.
    frame = av.AudioFrame.from_ndarray(block, format=ingest, layout=profile.layout)
    frame.rate = profile.rate
    return frame


def _mastered(chain: AudioGraph | None, blocks: tuple[Pcm, ...], profile: MediaProfile, ingest: str) -> Iterator["av.AudioFrame | None"]:
    # yield the lifted frames driven through the master capsule (its `filtered` owns the loudnorm/limiter lookahead-tail
    # push/drain/flush), or the raw lifted frames when no master is set, then the resampler-flush `None` sentinel last.
    frames = (_lift(block, ingest, profile) for block in blocks)
    yield from chain.filtered(frames) if chain is not None else frames
    yield None  # the resampler-flush sentinel `resample(None)` consumes last


def _voiced(container: object, stream: object, blocks: tuple[Pcm, ...], profile: MediaProfile) -> tuple[int, int]:
    # the shared audio drive `_mux_av` also composes: lift -> master -> resample/reframe -> encode -> mux over one
    # already-opened audio stream, returning (frames, samples). The caller opens the encoder, flushes, and reads the bytes.
    ctx = stream.codec_context
    ingest = _INGEST[blocks[0].dtype]
    chain = (
        AudioGraph.master(profile.rate, ingest, profile.layout, tuple((s.name, s.args) for s in profile.master.stages))
        if profile.master is not None
        else None
    )
    # frame_size folds the AudioFifo rebuffer into the resampler: one owner converts format/rate/layout and emits ctx.frame_size-sample frames
    resampler = av.AudioResampler(format=ctx.format.name, layout=profile.layout, rate=ctx.rate, frame_size=ctx.frame_size or None)
    frames = samples = 0
    for shaped in _mastered(chain, blocks, profile, ingest):
        for chunk in resampler.resample(shaped):
            _drive(container, stream, chunk, samples, ctx.rate)  # pts is the cumulative output-sample offset in 1/rate, not the chunk index
            frames, samples = frames + 1, samples + chunk.samples
    return frames, samples


@beartype
def _encode_audio(blocks: tuple[Pcm, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # Pcm is a real runtime union here (unlike the container's TYPE_CHECKING-only forward ref), so the `@beartype` weave
    # lifts a hint violation onto the `_mux`-caught `contract` fault — the weave `_mux_av` cannot carry.
    try:
        sink = io.BytesIO()
        with av.open(sink, mode="w", format=profile.container.value) as container:
            stream = profile.voiced(container)
            container.start_encoding()  # opens the encoder so codec_context.frame_size/format publish real values, not 0/unset
            frames, samples = _voiced(container, stream, blocks, profile)
            rate = stream.codec_context.rate
            _flush(container, stream)
        blob = sink.getvalue()
        return Ok((
            blob,
            MediaEvidence.measure(
                profile.container, profile.codec, samples / rate if rate else 0.0, frames, int(profile.bit_rate or 0), blob, _deployment(profile)
            ),
        ))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("encode_audio", exc))


def _mixed(sources: tuple[tuple[Pcm, ...], ...], profile: MediaProfile, ingest: str, weights: tuple[float, ...]) -> Iterator[Pcm]:
    # hand N per-source lifted-frame streams to `AudioGraph.mix`, whose `mixed` owns the multi-input abuffer/amix/abuffersink
    # link+configure, yielding each mixed frame to an interleaved-PCM block; lazy so a long mix never materializes at once.
    chain = AudioGraph.mix(profile.rate, ingest, profile.layout, len(sources), weights)
    streams = tuple((_lift(block, ingest, profile) for block in blocks) for blocks in sources)
    for frame in chain.mixed(streams):
        yield frame.to_ndarray()


def _mix_audio(sources: tuple[tuple[Pcm, ...], ...], profile: MediaProfile, weights: tuple[float, ...] = ()) -> tuple[Pcm, ...]:
    # the standalone N-source combine `timeline`/`analysis` and the encode arm compose: produces `Pcm` blocks, not the
    # MediaOp `(blob, MediaEvidence)` contract, so its composing arm rails the av fault at its own seam. `weights` empty = equal mix.
    ingest = _INGEST[sources[0][0].dtype]
    return tuple(_mixed(sources, profile, ingest, weights))


def _decoded(reader: object) -> Iterator[Pcm]:
    for frame in reader.decode(audio=0):  # lazy per-frame so a long stream never materializes every block in the worker at once
        yield frame.to_ndarray()


def _decode_audio(blob: bytes) -> tuple[tuple[Pcm, ...], int]:
    # the inverse of `_encode_audio`: demux + decode one audio stream to interleaved-PCM blocks + the source sample rate,
    # a standalone primitive the `timeline`/`analysis` consumers compose (Pcm blocks, not a MediaOp case).
    with av.open(io.BytesIO(blob), mode="r") as reader:
        return tuple(_decoded(reader)), reader.streams.audio[0].sample_rate
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
