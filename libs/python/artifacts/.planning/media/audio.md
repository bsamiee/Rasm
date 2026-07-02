# [PY_ARTIFACTS_MEDIA_AUDIO]

The temporal-artifact AUDIO owner — decode / encode / resample / layout / mix over the FFmpeg floor `av` (PyAV) provides, composing the `media/container#CONTAINER` container spine and the `media/filtergraph#FILTER` capability-detection `FilterNode` owner, never a second `Media` owner and never a local `av.filter.Graph` builder. `_encode_audio` encodes a `tuple[Pcm, ...]` producer PCM-block sequence into one single-stream audio container: `av.open(sink, mode="w", format=)` mints the `OutputContainer`, the container-owned `MediaProfile.voiced` projection mints the typed `AudioStream`, `OutputContainer.start_encoding` opens the encoder so `AudioCodecContext.frame_size`/`format`/`rate` publish the codec's real values (each reads `0`/unset until the encoder opens), the mastering chain is BUILT AND DRIVEN by `media/filtergraph#FILTER` (audio hands the ordered `Master.stages` and the source spec to `AudioGraph.master`, which routes every filter through its `av.filter.filters_available` probe and configures the `abuffer -> stages -> abuffersink` graph the local builder could not reliably terminate), and one `av.AudioResampler(format=ctx.format.name, layout=profile.layout, rate=ctx.rate, frame_size=ctx.frame_size or None)` converts each mastered block to the encoder's sample format/rate/layout AND reframes it to the encoder's exact `frame_size` in a single owner — folding the legacy `AudioFifo` rebuffer into the resampler's `frame_size` param — before the shared `_drive` `stream.encode(frame)` -> `container.mux(packets)` loop drives the chunks at their cumulative output-sample `pts` in the `1/rate` audio time base and `_flush` flushes with `stream.encode(None)` at end-of-stream. The producer block's `numpy` dtype selects the `AudioFrame.from_ndarray` ingest format through the `_INGEST` `frozendict` correspondence (`s16`/`s32`/`flt`/`dbl`), so an `int16`/`int32`/`float32`/`float64` producer all admit with no per-dtype arm and no hardcoded sample tag. This page composes the `media/container#CONTAINER` owner — the shared `Media`/`MediaOp`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family, the `_drive`/`_flush`/`_deployment`/`_media_fault` worker primitives — and READS its `MediaProfile.voiced` audio-stream-configure projection plus its `layout` FFmpeg layout-name `str` slot; it composes the `media/filtergraph#FILTER` `AudioGraph.master`/`AudioGraph.mix` graph capsules for every filter chain (the master mastering pass and the N-source `amix` combine) so no `av.filter.Graph` is built or driven here; and it OWNS only the `Pcm` producer-dtype union, the `_INGEST` ingest-format table, the `Stage`/`Master` mastering-and-layout vocabulary (the full `loudnorm`/`highpass`/`lowpass`/`dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aformat`/`pan`/`channelmap`/`aecho`/`aphaser`/`asubboost`/`compand` smart-constructor set), the `_lift`/`_mastered`/`_voiced` graph-composing primitives (`_voiced` the shared master/resample/encode-mux drive the `media/container#CONTAINER` `Mux` arm composes over its own muxer-opened audio stream), the `_encode_audio` worker, the `_mix_audio`/`_mixed` N-source combine primitive that closes the deferred `[AUDIO_DECODE_MIX]` residual through the filtergraph owner's multi-input configure, and the `_decode_audio`/`_decoded` inverse primitive the media restructure's `timeline`/`analysis` pages compose. The `EncodeAudio` case is one `MediaOp` arm dispatched at `media/container#CONTAINER`'s `_mux` `match`, worked here by the `_encode_audio` module-level function across the same `anyio.to_process.run_sync` subprocess lane, the muxer discriminated by the typed `ContainerFormat` value the profile carries. The encode loop runs over the subprocess lane, and the finished container bytes are the only payload crossing back. Every audio artifact routes through the single `core/plan#PLAN` `ArtifactPipeline` production entry as an `ArtifactWork` producer node and contributes the single `ArtifactReceipt.Media` case the `media/container#CONTAINER` owner also contributes — never a parallel audio-receipt rail; audio only APPLIES `loudnorm` at the EBU R128 target, the MEASURED integrated-LUFS / true-peak-dBTP / loudness-range band riding `analysis.md`'s `ebur128` read, never a second-pass measure here. This page closes the AUDIO decode/encode/resample/layout/mix half of the `MEDIA` idea.

## [01]-[INDEX]

- [01]-[MEDIA]: the audio arm of the closed-payload `MediaOp` family `media/container#CONTAINER` owns — the `EncodeAudio` worker plus the standalone `_decode_audio`/`_mix_audio` primitives — the `tuple[Pcm, ...]` producer PCM-block sequence lifted block-by-block through `_lift` (`AudioFrame.from_ndarray(block, format=_INGEST[block.dtype], layout=profile.layout)` with the producer `rate` stamped on `frame.rate`), optionally mastered through the `media/filtergraph#FILTER` `AudioGraph.master(rate, ingest, layout, stages)` capsule (EBU R128 `loudnorm` by default, or the full mastering/layout `Stage` chain), then converted to the encoder's published `format`/`rate`/`layout` AND reframed to the exact `frame_size` by one `AudioResampler(frame_size=ctx.frame_size or None)` whose `frame_size` param subsumes the legacy `AudioFifo` rebuffer; the container-owned `MediaProfile.voiced` projection minting one `AudioStream`, `OutputContainer.start_encoding` opening the encoder before the framing loop so the `frame_size`/`format`/`rate` reads are real, the shared `_drive`/`_flush`/`_deployment` worker primitives and `MediaEvidence.measure` constructor composed from `media/container#CONTAINER`, folding into the same single `RuntimeRail[ContentKey]` keyed over the muxed container bytes and the same `ArtifactReceipt.Media` case; `av` `open`/`InputContainer.decode`/`OutputContainer.start_encoding`/`AudioStream.encode`/`AudioFrame.from_ndarray`/`AudioFrame.to_ndarray`/`AudioResampler`/`AudioResampler.resample`/`OutputContainer.mux` settled against the folder `.api`, the `AudioCodecContext.frame_size`/`format`/`rate` read off the encoder the `start_encoding` open publishes and the `AudioStream.sample_rate` off the demuxed stream; the `av.filter.Graph`/`add_abuffer`/`add`/`link_nodes`/`push`/`pull`/`configure`/`filters_available` filter surface is `media/filtergraph#FILTER`'s, composed here through `AudioGraph`, never opened locally. The container/mux capsule, the `Media`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family, and the `_drive`/`_flush` primitives are `media/container#CONTAINER`'s; the filter-graph build/route/drive is `media/filtergraph#FILTER`'s; this page owns the `Pcm`/`_INGEST`/`Stage`/`Master` vocabulary, the `_lift`/`_mastered`/`_voiced`/`_encode_audio` encode worker, the `_mix_audio`/`_mixed` N-source combine, and the `_decode_audio`/`_decoded` inverse primitive, and reads the `MediaProfile.voiced` projection plus the `MediaProfile.master` policy slot it requires.

## [02]-[MEDIA]

- Owner: the audio arm of the `media/container#CONTAINER` `MediaOp` family, worked by the `_encode_audio` module-level function plus the standalone `_decode_audio`/`_mix_audio` primitives; this page composes the `media/container#CONTAINER` `Media` owner discriminating modality over the closed `MediaOp` family (reading its `voiced` audio-stream-configure projection and its `master` mastering-policy slot), the `MediaEvidence` typed encode receipt and its single `measure` constructor, the `ContainerFormat` muxer `StrEnum`, the `_drive`/`_flush`/`_deployment` worker primitives, and the `MediaFault`/`_media_fault` av-boundary fault owner — re-owning none of them — and composes the `media/filtergraph#FILTER` `AudioGraph.master`/`AudioGraph.mix` capsules for every filter chain, opening no `av.filter.Graph` locally. It OWNS the `Pcm` producer-dtype union, the `_INGEST` dtype-to-sample-format table, the `Stage`/`Master` mastering-and-layout policy (each `Stage` a typed-knob-to-`key=value`-arg derivation over one verified `libavfilter` name, the ordered `Master.stages` tuple the filtergraph capsule links and configures), the `_lift`/`_mastered`/`_voiced` graph-composing primitives (`_voiced` the shared master/resample/encode-mux drive `media/container#CONTAINER`'s `Mux` arm composes over its own muxer-opened `audio.voiced` stream), the `_encode_audio` worker, the `_mix_audio`/`_mixed` N-source combine, and the `_decode_audio`/`_decoded` inverse primitive. The single `av.AudioResampler(format=ctx.format.name, layout=profile.layout, rate=ctx.rate, frame_size=ctx.frame_size or None)` is the one audio-DSP owner per encode — it converts the mastered block to the encoder's sample format/rate/layout AND emits exactly `ctx.frame_size`-sample frames, so the separate `AudioFifo.write`/`read` rebuffer the AAC/Opus/FLAC encoders once required is one constructor argument, never a second owner; the mastering/mix graph is `media/filtergraph#FILTER`'s `AudioGraph` between the lift and the resampler. The `av.open(sink, mode="w")` `OutputContainer` is the one mux capsule per encode, always a context manager so the trailer is written and the IO released; `RuntimeRail[ContentKey]` is the one carrier the arm folds into, keyed over the finished container bytes through `ContentIdentity.of(container.value, blob)` at the `media/container#CONTAINER` `_emit` arm so an identical PCM sequence at an identical profile is a cache hit by reference the `core/plan#PLAN` pipeline elides on. The audio arm owns no second `Media` subclass, no parallel `encode_audio` surface beside `media/container`, no per-format audio container owner, and no local filter-graph builder — the modality is one `MediaOp` case plus two composable primitives, the graph mechanics one `media/filtergraph#FILTER` compose.
- Cases: the audio worker set — `_encode_audio(blocks, profile)` (each `Pcm` block lifted through `_lift`, optionally driven through the `AudioGraph.master(profile.rate, ingest, profile.layout, stages)` filtergraph capsule whose `filtered(frames)` push/pull/flush drive is the filter owner's, then converted to the encoder's `format`/`rate`/`layout` AND reframed to the encoder's required fixed `frame_size` by one `AudioResampler(frame_size=ctx.frame_size or None)` (`frame_size or None` yielding natural-sized frames for the variable-frame encoders FLAC/PCM and exact `frame_size` chunks for AAC `1024`/Opus `960`), the `MediaProfile.voiced` projection minting one `AudioStream`, the shared `_drive` loop driving each chunk at its cumulative-sample `pts` and `_flush` flushing at end-of-stream — dispatched by the `media/container#CONTAINER` `_mux` total `match`/`case` on the `MediaOp` discriminant, the `EncodeAudio` modality recovered from the discriminant, never a name suffix) · `_mix_audio(sources, profile, weights)` (the N-source combine over the `AudioGraph.mix(profile.rate, ingest, profile.layout, len(sources), weights)` filtergraph capsule whose `mixed(streams)` drives the multi-input `amix` graph the local `link_nodes` fan-in could not configure — closing the deferred `[AUDIO_DECODE_MIX]` residual — returning mixed `Pcm` blocks the `timeline`/`analysis`/encode consumers compose, a standalone primitive like `_decode_audio`, not a container blob) · `_decode_audio(blob)` (the inverse of `_encode_audio` — demux + decode an existing single audio stream to interleaved-PCM blocks + the source sample rate, lazily yielded through `_decoded` so a long stream never materializes every block at once). The layout/channel-conversion concern is carried by the `Stage` vocabulary (`aformat`/`pan`/`channelmap`) composed into the mastering chain PLUS the `AudioResampler` `layout` param for the simple conversion, never a fourth worker. The audio leg of the `Mux` interleave arm lives on `media/container#CONTAINER` because the container/mux capsule is that owner's, composing this page's shared `_voiced` audio-drive over its own `audio.voiced` stream; this page owns only the standalone single-stream audio ops.
- Entry: `_encode_audio(blocks, profile)` is the one worker-arm function the `media/container#CONTAINER` `_mux` `EncodeAudio` case dispatches over the `WORKER_BAND`-bounded `to_process.run_sync(_encode_audio, samples, profile, limiter=WORKER_BAND)` — a module-level function dispatched by qualified name across the runtime subprocess lane (`to_process.run_sync` cannot target a bound method or closure), the whole synchronous `av` open/master/resample/encode/mux loop CPU-bound C that holds the GIL; it opens one `av.open(sink, mode="w", format=profile.container.value)` `OutputContainer` against an in-memory `io.BytesIO` sink, mints the `AudioStream` through `MediaProfile.voiced`, opens the encoder through `OutputContainer.start_encoding`, composes the optional `AudioGraph.master` mastering capsule from `profile.master`, threads every producer block through `_voiced` (lift -> filtergraph master -> `AudioResampler` -> `_drive`), `_flush`es at end-of-stream, reads the muxed `bytes` plus the `MediaEvidence`, and returns `Result[(blob, MediaEvidence), MediaFault]` — the closed av-boundary fault rail `media/container#CONTAINER` owns, mapping the `av.error.FFmpegError` leaves through the shared `_media_fault` and an `av` `ImportError` onto `provision`, so the `EncodeAudio` worker rails identically to the `_encode_video`/`_transcode`/`_remux` siblings the same `_mux` dispatches and `_emit`'s `.map(self._keyed)` fold receives a `Result`, never a bare tuple. The `media/container#CONTAINER` `_emit` arm keys one `ContentKey` over the single container blob through `ContentIdentity.of(profile.container.value, blob)`; this worker mints no key. `_mix_audio`/`_decode_audio` are standalone primitives the `timeline`/`analysis` pages dispatch over their own `to_process` seams and rail at the composing arm, producing `Pcm` blocks rather than the `(blob, MediaEvidence)` container contract.
- Auto: the ingest format is `_INGEST[blocks[0].dtype]` — the producer dtype derives the `av` packed sample tag, so a producer never re-declares its format and a new dtype is one table row; `_lift` folds each block into an `AudioFrame.from_ndarray(block, format=ingest, layout=profile.layout)` with the producer rate stamped on `frame.rate` so the filtergraph abuffer and the resampler know the input timeline; `MediaProfile.voiced` mints `container.add_stream(profile.codec, rate=profile.rate)` then folds `layout`/`thread_count`/`bit_rate` onto its `codec_context` (the `layout` set before `start_encoding` so the encoder accepts the resampler's `profile.layout` frames), dropping `None` knobs; `OutputContainer.start_encoding` opens the encoder so `codec_context.frame_size`/`format`/`rate` publish the codec's real values (each is `0`/unset until the encoder opens — a `frame_size` read before the open returns `0`, and `AudioResampler(frame_size=0)` is the variable-frame path, so the open is what makes the AAC/Opus rebuffer real); `_voiced` composes the `AudioGraph.master(profile.rate, ingest, profile.layout, tuple((s.name, s.args) for s in profile.master.stages))` capsule only when `profile.master is not None`, and `_mastered` yields the lifted frames driven through `chain.filtered(frames)` (the filtergraph owner's push/drain/flush of the `loudnorm`/limiter lookahead tail) then the resampler-flush `None` sentinel last; `av.AudioResampler(..., frame_size=ctx.frame_size or None)` converts each mastered (or raw) frame to the encoder's sample format/rate/layout and emits exactly `frame_size`-sample frames, flushed by the `resample(None)` the sentinel drives; the shared `_drive` loop folds `stream.encode(chunk)` over the reframed sequence at each chunk's cumulative output-sample `pts` in the `1/rate` audio time base (the count of samples already encoded, never the chunk index, so the presentation timeline is sample-accurate), muxing each returned `Packet` through `container.mux`, then `_flush` flushes with `stream.encode(None)` muxed at end-of-stream; the muxed `bytes` read off the `BytesIO` sink fold through `MediaEvidence.measure` once, the audio arm reporting `samples / rate` as the exact temporal extent (the cumulative output-sample count over the encoder rate) and the encoded-frame count as the frame count — no video read-back probe, the audio-only container carrying no video stream and its sample fold already exact. `_mix_audio` composes `AudioGraph.mix` over the N per-source lifted streams whose `mixed(streams)` drives the `amix` output back through `AudioFrame.to_ndarray()` to mixed `Pcm` blocks; `_decode_audio` composes `av.open(BytesIO(blob), mode="r")` -> `reader.decode(audio=0)` -> `AudioFrame.to_ndarray()` lazily through `_decoded`.
- Receipt: each audio encode contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media` carrying the content key and the `MediaEvidence` facts — container, codec, duration, byte count, frame count, bit rate — keyed by the content key the `media/container#CONTAINER` `_emit` arm derives over the muxed bytes; the worker folds `(profile.container, profile.codec, samples / rate, frame_count, int(profile.bit_rate or 0), blob, _deployment(profile))` through `MediaEvidence.measure(..., facts=...)` once, and `_emit` spreads the named scalars plus `evidence.facts` onto the `ArtifactReceipt.Media` case's eighth `facts: frozendict[str, float | str]` band, the receipt owner importing no `MediaEvidence` value object nor any `av` container handle. This arm contributes the same single `ArtifactReceipt.Media` case the `media/container#CONTAINER` owner contributes — `core/receipt#RECEIPT` cites both as the producers of the one media case, never a parallel audio-receipt rail; the `duration` is the one observable temporal-encode value the runtime `observability/metrics` `MeterProvider` reads off the receipt fold, and the `bit_rate` slot carries the constant-bitrate audio target. Audio only APPLIES the `loudnorm` EBU R128 normalization through the filtergraph master chain; the MEASURED integrated-LUFS / true-peak-dBTP / loudness-range band (a two-pass `loudnorm=print_format=json` read the single-pass encode does not expose) rides `analysis.md`'s `ebur128` measurement onto the same `Media.facts` band, never a second-pass measure or a parallel audio receipt here. `_mix_audio`/`_decode_audio` produce `Pcm` blocks and mint no receipt — their composing arm (a `timeline`/`analysis` producer or the encode worker) keys and contributes the one `Media` case.
- Growth: a new producer sample dtype is one `_INGEST` row, `_lift` already deriving its format; a new audio codec is one `MediaProfile.codec` string the `voiced` `add_stream` row reads, the resample/reframe pipeline already adapting to its published `format`/`frame_size`; a producer whose sample rate differs from the encoder rate is one `frame.rate` stamp in front of the existing `AudioResampler` (already the rate/format/layout converter, no new surface); a new encode knob (bit rate, VBR mode) is one `MediaProfile` field bound into the `voiced` codec-context fold; a louder mastering chain is one ordered `Stage` in the `Master.stages` tuple (the `dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aecho`/`aphaser`/`asubboost`/`compand` constructors already minted, a new `libavfilter` one more `Stage` smart constructor deriving its arg string, the filtergraph owner routing it through `filters_available`), never a local graph edit; a new channel-layout conversion is one `Stage.aformat`/`pan`/`channelmap` in the chain or one `profile.layout` FFmpeg layout name the `AudioResampler` carries; a new mix policy (per-source weight, duration mode) is one `weights` argument the `AudioGraph.mix` capsule reads; a new evidence fact is one `(label, value)` band key on the `Media.facts` band (the `analysis.md` LUFS/true-peak/loudness-range audio-native band riding it); zero new surface — the modality space stays the three `media/container#CONTAINER` cases (`EncodeVideo`/`EncodeAudio`/`Mux`) on one owner plus the two composable audio primitives, every addition a row, field, `Stage`, or capsule argument, never a local `av.filter.Graph`.

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

from builtins import frozendict

# media/container#CONTAINER (the renamed video owner) owns the shared Media family + worker primitives;
# media/filtergraph#FILTER owns the closed FilterNode family and the capability-detection AudioGraph capsule
# audio composes for EVERY filter chain — no `av.filter.Graph` is opened, linked, configured, or drained here.
from artifacts.media.container import MediaEvidence, MediaFault, MediaProfile, _deployment, _drive, _flush, _media_fault
from artifacts.media.filtergraph import AudioGraph

lazy import av
lazy import av.error

# --- [TYPES] ----------------------------------------------------------------------------

type Pcm = NDArray[np.int16] | NDArray[np.int32] | NDArray[np.float32] | NDArray[np.float64]

# channel layout rides `MediaProfile.layout` as the FFmpeg layout NAME (`str`) — `av` types it `string | av.AudioLayout`
# and rejects a `StrEnum` member, so it is an open av-name field like `codec`/`pix_fmt`, never a partial-expose local enum;
# an arbitrary channel remap rides a `Stage.pan`/`Stage.channelmap` node in the master chain.

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
    # one libavfilter node the filtergraph `AudioGraph` links and routes through its `filters_available` probe;
    # the smart constructors are the closed mastering-and-layout vocabulary, each deriving its verified
    # `key=value:...` arg string from typed knobs. The filter NAME is the logical op; the filtergraph owner
    # resolves native-vs-substitute, so a build missing one filter routes to its verified substitute, never a raise here.
    name: str
    args: str = ""

    # --- mastering (loudness / dynamics) ---
    @staticmethod
    def loudnorm(integrated: float = -16.0, true_peak: float = -1.5, loudness_range: float = 11.0, linear: bool = True, dual_mono: bool = False) -> "Stage":
        return Stage("loudnorm", f"I={integrated}:TP={true_peak}:LRA={loudness_range}:linear={str(linear).lower()}:dual_mono={str(dual_mono).lower()}")

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
    def aphaser(in_gain: float = 0.4, out_gain: float = 0.74, delay: float = 3.0, decay: float = 0.4, speed: float = 0.5, kind: str = "triangular") -> "Stage":
        return Stage("aphaser", f"in_gain={in_gain}:out_gain={out_gain}:delay={delay}:decay={decay}:speed={speed}:type={kind}")

    @staticmethod
    def asubboost(dry: float = 0.7, wet: float = 0.7, decay: float = 0.7, feedback: float = 0.5, cutoff: float = 100.0, slope: float = 0.5, delay: float = 20.0) -> "Stage":
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
    # the ordered mastering-and-layout chain `media/filtergraph#FILTER`'s `AudioGraph.master` links into one
    # `abuffer -> stages -> abuffersink` graph; the default is the single EBU R128 `loudnorm` normalizer, a louder
    # chain composes more ordered `Stage`s (highpass -> acompressor -> alimiter -> loudnorm is the standard broadcast master).
    stages: tuple[Stage, ...] = (Stage.loudnorm(),)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _lift(block: Pcm, ingest: str, profile: MediaProfile) -> "av.AudioFrame":
    # one producer PCM block -> one av AudioFrame with the input timeline stamped; the dtype-keyed `ingest` tag
    # keeps the lift total over the four interleaved-PCM dtypes with no per-dtype arm.
    frame = av.AudioFrame.from_ndarray(block, format=ingest, layout=profile.layout)
    frame.rate = profile.rate
    return frame


def _mastered(chain: AudioGraph | None, blocks: tuple[Pcm, ...], profile: MediaProfile, ingest: str) -> Iterator["av.AudioFrame | None"]:
    # yield the lifted frames driven through the filtergraph master capsule (its `filtered` owns the push/drain/flush of
    # the loudnorm/limiter lookahead tail), or the raw lifted frames when no master is set, then the resampler-flush
    # sentinel `None` last — no `av.filter.Graph` is opened here, the graph build/route/drive is the filter owner's.
    frames = (_lift(block, ingest, profile) for block in blocks)
    yield from chain.filtered(frames) if chain is not None else frames
    yield None  # the resampler-flush sentinel `resample(None)` consumes last


def _voiced(container: object, stream: object, blocks: tuple[Pcm, ...], profile: MediaProfile) -> tuple[int, int]:
    # the shared audio drive `media/container#CONTAINER` `_mux_av` also composes: lift -> filtergraph master ->
    # resample/reframe -> encode -> mux over one already-opened audio stream, returning (frames, samples). The caller
    # opens the encoder (`start_encoding`), flushes it (`_flush`), and reads the container bytes; `_voiced` owns only the
    # per-block lift, the filtergraph-master compose, the resample, and the encode-mux loop.
    ctx = stream.codec_context
    ingest = _INGEST[blocks[0].dtype]
    chain = AudioGraph.master(profile.rate, ingest, profile.layout, tuple((s.name, s.args) for s in profile.master.stages)) if profile.master is not None else None
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
    # rails identically to the container _encode_video/_transcode/_remux siblings the same media/container#CONTAINER `_mux`
    # dispatches: the av FFmpegError leaves map through the shared `_media_fault`, an `av` ImportError onto `provision`, and
    # the `@beartype` weave lifts a hint violation onto the `_mux`-caught `contract` fault — Pcm is a real runtime union here
    # (unlike the container's TYPE_CHECKING-only forward ref), so this worker carries the contract weave `_mux_av` cannot.
    try:
        sink = io.BytesIO()
        with av.open(sink, mode="w", format=profile.container.value) as container:
            stream = profile.voiced(container)
            container.start_encoding()  # opens the encoder so codec_context.frame_size/format publish real values, not 0/unset
            frames, samples = _voiced(container, stream, blocks, profile)
            rate = stream.codec_context.rate
            _flush(container, stream)
        blob = sink.getvalue()
        return Ok((blob, MediaEvidence.measure(profile.container, profile.codec, samples / rate if rate else 0.0, frames, int(profile.bit_rate or 0), blob, _deployment(profile))))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("encode_audio", exc))


def _mixed(sources: tuple[tuple[Pcm, ...], ...], profile: MediaProfile, ingest: str, weights: tuple[float, ...]) -> Iterator[Pcm]:
    # the N-source amix combine: hand N per-source lifted-frame streams to the filtergraph `AudioGraph.mix` capsule whose
    # `mixed` owns the multi-input abuffer/amix/abuffersink link + configure the local `link_nodes` fan-in could not settle,
    # yielding each mixed frame back to an interleaved-PCM block. lazy so a long mix never materializes every block at once.
    chain = AudioGraph.mix(profile.rate, ingest, profile.layout, len(sources), weights)
    streams = tuple((_lift(block, ingest, profile) for block in blocks) for blocks in sources)
    for frame in chain.mixed(streams):
        yield frame.to_ndarray()


def _mix_audio(sources: tuple[tuple[Pcm, ...], ...], profile: MediaProfile, weights: tuple[float, ...] = ()) -> tuple[Pcm, ...]:
    # the standalone N-source combine primitive `timeline`/`analysis` and the encode arm compose (produces `Pcm` blocks,
    # not the MediaOp `(blob, MediaEvidence)` container contract, so a bare worker primitive like `_decode_audio` — its
    # composing arm rails the av fault at its own `to_process` seam); closes the deferred `[AUDIO_DECODE_MIX]` residual
    # through the filtergraph owner's multi-input configure. `weights` empty = equal mix.
    ingest = _INGEST[sources[0][0].dtype]
    return tuple(_mixed(sources, profile, ingest, weights))


def _decoded(reader: object) -> Iterator[Pcm]:
    for frame in reader.decode(audio=0):  # lazy per-frame so a long stream never materializes every block in the worker at once
        yield frame.to_ndarray()


def _decode_audio(blob: bytes) -> tuple[tuple[Pcm, ...], int]:
    # the inverse of `_encode_audio`: demux + decode an existing single audio stream to interleaved-PCM blocks + the source
    # sample rate — the standalone primitive the media restructure's `timeline`/`analysis` consumers compose (it produces
    # `Pcm` blocks, not the MediaOp `(blob, MediaEvidence)` container contract, so it is a worker primitive, not a MediaOp case).
    with av.open(io.BytesIO(blob), mode="r") as reader:
        return tuple(_decoded(reader)), reader.streams.audio[0].sample_rate
```

## [03]-[RESEARCH]

- [AUDIO_FORMAT] [RESOLVED]: the producer block's `numpy` dtype selects the `AudioFrame.from_ndarray` ingest format through the `_INGEST` `frozendict` correspondence (`int16` -> `s16`, `int32` -> `s32`, `float32` -> `flt`, `float64` -> `dbl`), so an `int16`/`int32`/`float32`/`float64` interleaved-PCM producer all admit with one declared table and no hardcoded sample tag — the prior design's single `_SAMPLE_FORMAT = "s16"` import modelled only the `int16` producer and mislabelled any other dtype. `AudioFrame.from_ndarray(array, format="s16", layout="stereo")` is the `av` `.api` `[03]-[ENTRYPOINTS]` frame-encode table row `[13]`; `_lift` stamps `frame.rate = profile.rate` so the abuffer and resampler know the input timeline. The `AudioResampler` downstream converts the lifted dtype to the encoder's required `codec_context.format` (read after `start_encoding`), so a `float32` producer feeding an `fltp`-only encoder (AAC, Opus) and an `int16` producer feeding a `s16` encoder (FLAC) each convert cleanly. A new producer dtype is one `_INGEST` row; the lift never re-declares the format.
- [AUDIO_MASTER] [RESOLVED]: the loudness-mastering pass is composed from `media/filtergraph#FILTER`'s `AudioGraph.master(rate, sample_format, layout, filters)` capsule — audio hands the ordered `(name, args)` projection of `Master.stages` and the source spec, and the filter owner builds the `abuffer -> stages -> abuffersink` graph, routes every filter through its `av.filter.filters_available` probe (verified `True`; `loudnorm`/`highpass`/`lowpass`/`dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aformat`/`pan`/`channelmap`/`aecho`/`aphaser`/`asubboost`/`compand` all present), and OWNS the push/drain/flush drive through `chain.filtered(frames)`. This REPLACES the prior local `_master` builder that opened `av.filter.Graph()` and relied on an EXPLICIT `graph.add("abuffersink")` terminal (`configure()`'s `auto_buffer` adds no sink for an audio graph, `ArgumentError: Invalid argument (22)`) — the sink-termination and configure quirk is now the filtergraph owner's single fixed concern, never re-derived per composing page. The `Stage` mastering-and-layout vocabulary is MINTED to depth: `dynaudnorm`/`acompressor`/`alimiter` (dynamics), `aecho`/`aphaser`/`asubboost` (colour), `compand` (soft-knee companding), `afade`/`atempo` (timeline), and `aformat`/`pan`/`channelmap` (layout/channel conversion) — each a `Stage` smart constructor deriving its verified `key=value:...` arg string from typed knobs, so the standard broadcast master `highpass -> acompressor -> alimiter -> loudnorm` is `Master(stages=(Stage.highpass(), Stage.acompressor(), Stage.alimiter(), Stage.loudnorm()))`, one ordered tuple the filtergraph owner links and configures. Cross-file requirement: `MediaProfile.master: Master | None = None` is the container-owned policy slot this arm reads, the `Master` owner imported under `TYPE_CHECKING` in `media/container#CONTAINER` so the runtime `None` default closes no `media/container` <-> `media/audio` import cycle; and `media/filtergraph#FILTER` must land with the `AudioGraph.master(rate, sample_format, layout, filters)` -> `.filtered(frames)` capsule contract this page composes.
- [AUDIO_DECODE_MIX] [RESOLVED]: the `_decode_audio` primitive is the verified inverse of `_encode_audio` — `av.open(BytesIO(blob), mode="r")` -> `reader.decode(audio=0)` -> `AudioFrame.to_ndarray()` lazily yielded through `_decoded` (an `Iterator[Pcm]` so a long stream never materializes every block at once, LAZY_COMBINATORS) -> `tuple` at the worker return with `reader.streams.audio[0].sample_rate`. The N-source MIX path (`amix`, verified present in `filters_available`) is now CLOSED: `_mix_audio`/`_mixed` compose `media/filtergraph#FILTER`'s `AudioGraph.mix(rate, sample_format, layout, inputs, weights)` capsule whose `mixed(streams)` owns the multi-input `N abuffers -> amix -> abuffersink` link + configure the prior local `link_nodes` fan-in NOR the `link_to(amix, 0, input_idx)` form could settle on `av 17.1.0` (both raised at `graph.configure()`). The multi-input configure was exactly the illusory non-configuring graph the density bar rejected when it lived locally; moving the graph build/route/drive to the `FilterNode` owner the brief's [04] restructure declares is what makes the mix real. `_mix_audio` produces mixed `Pcm` blocks (not the MediaOp `(blob, MediaEvidence)` container contract), a standalone worker primitive the `timeline`/`analysis` pages and the encode arm compose. Cross-file requirement: `media/filtergraph#FILTER` must land with the `AudioGraph.mix(...)` -> `.mixed(streams)` multi-input capsule contract this page composes.
- [AUDIO_LAYOUT] [RESOLVED]: the channel-conversion / layout axis the brief names has two composed forms and no fourth worker. The simple conversion (a stereo->mono downmix, a mono->stereo upmix) rides the `AudioResampler(layout=profile.layout, ...)` already in `_voiced` — the resampler is the rate/format/layout converter, so `profile.layout` alone re-lays a producer whose channel count differs from the encoder's. The arbitrary conversion (a per-channel gain matrix, a fixed source-to-output channel reorder) rides a `Stage.aformat`/`Stage.pan`/`Stage.channelmap` node in the `Master.stages` chain — `pan=stereo|c0=0.5*c0|c1=0.5*c1` for a per-channel down/up-mix the resampler cannot express, `channelmap=map=FL-FL|FR-FR:channel_layout=stereo` for a fixed reorder, `aformat=channel_layouts=...` for a layout constraint — each verified present in `filters_available` and routed by the filtergraph owner. So the layout concern is one existing param plus three `Stage` rows, never a fourth graph-building worker.
- [AUDIO_EVIDENCE] [RESOLVED]: the audio arm's `MediaEvidence` reports `samples / rate` as the temporal extent — the cumulative output-sample count (summed `AudioFrame.samples` across the encoded chunks) over the encoder rate — and the encoded-frame count as the frame count, computed in the encode fold rather than by re-opening the muxed bytes through the `media/container#CONTAINER` `_probe`, because `_probe` reads `reader.streams.video[0]` (an audio-only container carries no video stream) and an audio-only re-probe is unreliable (verified on `av 17.1.0`: an Opus/WebM and a FLAC/MKV read-back report `streams.audio[0].frames == 0` while the exact sample fold yields the true count). The byte count is `len(blob)` off the `BytesIO` sink, folded through the shared `MediaEvidence.measure` constructor once. The bundled-`libav` deployment facts ride the `facts` band as the container arm's do: `_encode_audio` threads `_deployment(profile)` through `MediaEvidence.measure(..., facts=...)`, so an audio artifact carries the same deployment evidence a video one does. Audio only APPLIES `loudnorm` at the EBU R128 target through the filtergraph master chain; the MEASURED integrated-LUFS/true-peak-dBTP/loudness-range band needs a two-pass `loudnorm=print_format=json` (or `ebur128`) read the single-pass chain here does not expose, so it rides `analysis.md`'s `ebur128` loudness measurement onto the same `core/receipt#RECEIPT` `Media.facts` band (the brief's [04] restructure), never a parallel audio receipt.
- [RECEIPT_MEDIA_CASE] [RESOLVED]: the `ArtifactReceipt.Media` case is the widened eight-slot `tuple[ContentKey, str, str, float, int, int, int, frozendict[str, float | str]]` (key, container, codec, duration, byte count, frame count, bit rate, per-page `facts` band) on the same-folder `core/receipt#RECEIPT` owner (verified against `receipt.md`: the `media` tag token, the `Media(key, container, codec, duration, bytes_, frames, bit_rate=0, facts=frozendict())` mint defaulting `facts`, the eight-element `media=(...)` case, and the `media` `_facts` arm flattening the band exactly as `preview` flattens `scores`). The receipt owner names `media/container#CONTAINER`/`media/audio#MEDIA`/`media/*` as the contributors of the ONE shared `Media` case, the seven restructured media producers spreading their own `av`/`pysubs2` evidence onto the band (the `audio` EBU R128 integrated-LUFS/true-peak-dBTP/loudness-range facts arriving via `analysis.md`), so the widening is DONE receipt-side. `MediaEvidence` (on `media/container#CONTAINER`) carries the seventh `facts` field and its `measure` constructor accepts `facts`, so this arm threads `_deployment(profile)` and the `_emit` `_keyed` arm spreads `evidence.facts` onto the eighth `Media` slot — audio and container share ONE `facts` path with no cross-file widening outstanding. No parallel audio-receipt rail is minted.
- [AUDIO_FAULT_RAIL] [RESOLVED]: `_encode_audio` returns `Result[tuple[bytes, MediaEvidence], MediaFault]`, not a bare tuple — the exact worker contract the `media/container#CONTAINER` `_mux` `EncodeAudio` arm dispatches (`_WORKER_RETRY(to_process.run_sync, _encode_audio, ...)`) and `_emit` folds through `.map(self._keyed)`, so a bare-tuple return would have been an `AttributeError` at `.map` and a static type miss on `_mux`'s `Result[...]` annotation. The `MediaFault` union and the `_media_fault` av-leaf mapping are `media/container#CONTAINER`'s single owners (imported here, never re-declared), and this worker maps its own av raises at the arm that incurs them — `av.error.FFmpegError` -> `_media_fault("encode_audio", exc)`, an `av` `ImportError` -> `MediaFault(provision=...)` — returning the `Result` as picklable data across the `to_process` seam exactly as `_encode_video` does. The `@beartype` weave is real here because `Pcm` is a runtime union on this page (unlike the container's TYPE_CHECKING-only forward ref that keeps `_mux_av` un-woven), so a hint violation lifts onto the `_mux`-caught `contract` fault; the transient `BrokenWorkerProcess` retry stays on the container `_mux` dispatch site (`_WORKER_RETRY`), never re-minted here. `_mix_audio`/`_decode_audio` are bare primitives (like the container's non-railed reads) whose composing arm rails the av fault at its own `to_process` seam.
