# [PY_ARTIFACTS_MEDIA_AUDIO]

The temporal-artifact AUDIO-stream encode arm — the `EncodeAudio` worker the `media/video#MEDIA` `MediaOp` family dispatches, never a second `Media` owner. `_encode_audio` encodes a `tuple[Pcm, ...]` producer PCM-block sequence into one single-stream audio container over the FFmpeg floor `av` (PyAV) provides. `av.open(sink, mode="w", format=)` mints the `OutputContainer`, the video-owned `MediaProfile.voiced` projection mints the typed `AudioStream`, `OutputContainer.start_encoding` opens the encoder so `AudioCodecContext.frame_size`/`format`/`rate` publish the codec's real values (each reads `0`/unset until the encoder opens), an optional `av.filter.Graph` mastering chain (`Master.stages` — the EBU R128 `loudnorm` normalizer by default, or the full mastering vocabulary `highpass`/`lowpass`/`dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aformat` as ordered `Stage` smart constructors, each terminated by an EXPLICIT `abuffersink` node `configure()` does not auto-add for an audio graph) masters every block to its target, and one `av.AudioResampler(format=ctx.format.name, layout=profile.layout, rate=ctx.rate, frame_size=ctx.frame_size or None)` converts each block to the encoder's required sample format/rate/layout AND reframes it to the encoder's exact `frame_size` in a single owner — folding the separate `AudioFifo` rebuffer into the resampler's `frame_size` param — before the shared `_drive` `stream.encode(frame)` -> `container.mux(packets)` loop drives the chunks at their cumulative output-sample `pts` in the `1/rate` audio time base and `_flush` flushes with `stream.encode(None)` at end-of-stream. The producer block's `numpy` dtype selects the `AudioFrame.from_ndarray` ingest format through the `_INGEST` `frozendict` correspondence (`s16`/`s32`/`flt`/`dbl`), so an `int16`/`int32`/`float32`/`float64` producer all admit with no per-dtype arm and no hardcoded sample tag. This page composes the `media/video#MEDIA` container owner — the shared `Media`/`MediaOp`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family, the `_drive`/`_flush` worker primitives — and READS its `MediaProfile.voiced` audio-stream-configure projection plus its `layout` FFmpeg layout-name `str` slot; it OWNS only the `Pcm` producer-dtype union, the `_INGEST` ingest-format table, the `Stage`/`Master` mastering-chain policy (the full `loudnorm`/`highpass`/`lowpass`/`dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aformat` smart-constructor vocabulary), the `_master`/`_drain`/`_mastered`/`_voiced` graph-and-resample primitives (`_voiced` the shared audio-drive the `media/video#MEDIA` `Mux` arm composes over its own muxer-opened audio stream), the `_encode_audio` worker, and the `_decode_audio`/`_decoded` inverse primitive the media restructure's `timeline`/`analysis` pages compose. The `EncodeAudio` case is one `MediaOp` arm dispatched at `media/video#MEDIA`'s `_mux` `match`, worked here by the `_encode_audio` module-level function across the same `anyio.to_process.run_sync` subprocess lane, the muxer discriminated by the typed `ContainerFormat` value the profile carries. The encode loop runs over the subprocess lane, and the finished container bytes are the only payload crossing back. This arm contributes the single `ArtifactReceipt.Media` case the `media/video#MEDIA` container owner also contributes, never a parallel audio-receipt rail. This page closes the AUDIO half of the `MEDIA` idea.

## [01]-[INDEX]

- [01]-[MEDIA]: the `EncodeAudio` arm of the closed-payload `MediaOp` family `media/video#MEDIA` owns — the `tuple[Pcm, ...]` producer PCM-block sequence lifted block-by-block through `AudioFrame.from_ndarray(block, format=_INGEST[block.dtype], layout=profile.layout)`, optionally mastered through the `av.filter.Graph` `Master.stages` chain (EBU R128 `loudnorm` by default) folded by `_master`, then converted to the encoder's published `format`/`rate`/`layout` AND reframed to the exact `frame_size` by one `AudioResampler(frame_size=ctx.frame_size or None)` whose `frame_size` param subsumes the legacy `AudioFifo` rebuffer; the video-owned `MediaProfile.voiced` projection minting one `AudioStream`, `OutputContainer.start_encoding` opening the encoder before the framing loop so the `frame_size`/`format`/`rate` reads are real, the shared `_drive`/`_flush`/`_deployment` worker primitives and `MediaEvidence.measure` constructor composed from `media/video#MEDIA`, folding into the same single `RuntimeRail[ContentKey]` keyed over the muxed container bytes and the same `ArtifactReceipt.Media` case; `av` `open`/`InputContainer.decode`/`OutputContainer.start_encoding`/`AudioStream.encode`/`AudioFrame.from_ndarray`/`AudioFrame.to_ndarray`/`AudioResampler`/`AudioResampler.resample`/`OutputContainer.mux`/`Graph.add_abuffer`/`Graph.add`/`Graph.link_nodes`/`Graph.push`/`Graph.pull`/`Graph.configure` (with the explicit `abuffersink` terminal the auto_buffer default omits) settled against the folder `.api`, the `AudioCodecContext.frame_size`/`format`/`rate` read off the encoder the `start_encoding` open publishes and the `AudioStream.sample_rate` off the demuxed stream. The container/mux capsule, the `Media`/`MediaProfile`/`MediaEvidence`/`ContainerFormat` family, and the `_drive`/`_flush` primitives are `media/video#MEDIA`'s; this page owns the `Pcm`/`_INGEST`/`Stage`/`Master` vocabulary (the full `loudnorm`/`highpass`/`lowpass`/`dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aformat` mastering constructors), the `_master`/`_drain`/`_mastered`/`_voiced`/`_encode_audio` worker (`_voiced` the shared master/resample/encode-mux drive `media/video#MEDIA` `_mux_av` reuses), and the `_decode_audio`/`_decoded` inverse primitive, and reads the `MediaProfile.voiced` projection plus the `MediaProfile.master` policy slot it requires.

## [02]-[MEDIA]

- Owner: the `EncodeAudio` arm of the `media/video#MEDIA` `MediaOp` family, worked by the `_encode_audio` module-level function; this page composes the `media/video#MEDIA` `Media` owner discriminating modality over the closed `MediaOp` family, the `MediaProfile` frozen encode-policy value (reading its `voiced` audio-stream-configure projection and its `master` loudnorm slot), the `MediaEvidence` typed encode receipt and its single `measure` constructor, the `ContainerFormat` muxer `StrEnum`, and the `_drive`/`_flush` worker primitives — re-owning none of them. It OWNS the `Pcm` producer-dtype union, the `_INGEST` dtype-to-sample-format table, the `Stage`/`Master` mastering policy (the `loudnorm`/`highpass`/`lowpass`/`dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aformat` smart-constructor vocabulary, each `Stage` a typed-knob-to-`key=value`-arg derivation), the `_master`/`_drain`/`_mastered`/`_voiced` graph-and-resample primitives (`_master` terminating the chain on an EXPLICIT `abuffersink` node since `configure()`'s auto_buffer adds none for an audio graph), the `_encode_audio` worker, and the `_decode_audio`/`_decoded` inverse primitive — `_voiced` the shared audio-drive the `media/video#MEDIA` `Mux` arm composes over its own muxer-opened `audio.voiced` stream. The single `av.AudioResampler(format=ctx.format.name, layout=profile.layout, rate=ctx.rate, frame_size=ctx.frame_size or None)` is the one audio DSP owner per encode — it converts the producer block to the encoder's sample format/rate/layout AND emits exactly `ctx.frame_size`-sample frames, so the separate `AudioFifo.write`/`read` rebuffer the AAC/Opus/FLAC encoders once required is one constructor argument, never a second owner; the optional `av.filter.Graph` is the `loudnorm` master pass between the lift and the resampler. The `av.open(sink, mode="w")` `OutputContainer` is the one mux capsule per encode, always a context manager so the trailer is written and the IO released; `RuntimeRail[ContentKey]` is the one carrier the arm folds into, keyed over the finished container bytes through `ContentIdentity.of(container.value, blob)` at the `media/video#MEDIA` `_emit` arm so an identical PCM sequence at an identical profile is a cache hit by reference. The audio arm owns no second `Media` subclass, no parallel `encode_audio` surface beside `media/video`, and no per-format audio container owner — the modality is one `MediaOp` case, the worker one module-level function.
- Cases: the `EncodeAudio(samples, profile)` arm — each `Pcm` block (an `int16`/`int32`/`float32`/`float64` interleaved-PCM array) lifted through `AudioFrame.from_ndarray(block, format=_INGEST[block.dtype], layout=profile.layout)` with the producer `rate` stamped on `source.rate`, optionally driven through the `_master` `av.filter.Graph` (`abuffer` -> `loudnorm` -> auto `abuffersink`, the `Master.rendered()` `I=:TP=:LRA=` args normalizing to the EBU R128 target, drained through the `Graph.push`/`Graph.pull` protocol), then converted to the encoder's `format`/`rate`/`layout` AND reframed to the encoder's required fixed `frame_size` by one `AudioResampler(frame_size=ctx.frame_size or None)` (`frame_size or None` yielding natural-sized frames for the variable-frame encoders FLAC/PCM and exact `frame_size` chunks for AAC `1024`/Opus `960`), the `MediaProfile.voiced` projection minting one `AudioStream`, the shared `_drive` `stream.encode(frame)` -> `container.mux(packets)` loop driving each chunk at its cumulative-sample `pts` and `_flush` flushing with `stream.encode(None)` at end-of-stream — dispatched by the `media/video#MEDIA` `_mux` total `match`/`case` on the `MediaOp` discriminant, the `EncodeAudio` modality recovered from the discriminant, never a name suffix. The audio leg of the `Mux` interleave arm lives on `media/video#MEDIA` because the container/mux capsule is that owner's; this page owns only the standalone single-stream audio encode.
- Entry: `_encode_audio(blocks, profile)` is the one worker-arm function the `media/video#MEDIA` `_mux` `EncodeAudio` case dispatches over the `WORKER_BAND`-bounded `to_process.run_sync(_encode_audio, samples, profile, limiter=WORKER_BAND)` — a module-level function dispatched by qualified name across the runtime subprocess lane (`to_process.run_sync` cannot target a bound method or closure), the whole synchronous `av` open/master/resample/encode/mux loop CPU-bound C that holds the GIL; it opens one `av.open(sink, mode="w", format=profile.container.value)` `OutputContainer` against an in-memory `io.BytesIO` sink, mints the `AudioStream` through `MediaProfile.voiced`, opens the encoder through `OutputContainer.start_encoding`, builds the optional `loudnorm` graph from `profile.master`, threads every producer block through the graph and `AudioResampler` to the encoder's fixed `frame_size`, runs the shared `_drive` encode-then-mux loop over the reframed chunks, `_flush`es at end-of-stream, reads the muxed `bytes` plus the `MediaEvidence`, and returns `Result[(blob, MediaEvidence), MediaFault]` — the closed av-boundary fault rail `media/video#MEDIA` owns, mapping the `av.error.FFmpegError` leaves through the shared `_media_fault` and an `av` `ImportError` onto `provision`, so the `EncodeAudio` worker rails identically to the `_encode_video`/`_transcode`/`_remux` siblings the same `_mux` dispatches and `_emit`'s `.map(self._keyed)` fold receives a `Result`, never a bare tuple. The `media/video#MEDIA` `_emit` arm keys one `ContentKey` over the single container blob through `ContentIdentity.of(profile.container.value, blob)`; this worker mints no key.
- Auto: the ingest format is `_INGEST[blocks[0].dtype]` — the producer dtype derives the `av` packed sample tag, so a producer never re-declares its format and a new dtype is one table row; each block folds into an `AudioFrame.from_ndarray(block, format=ingest, layout=profile.layout)` with the producer rate stamped on `source.rate` so the resampler and the abuffer know the input timeline; `MediaProfile.voiced` mints `container.add_stream(profile.codec, rate=profile.rate)` then folds `layout`/`thread_count`/`bit_rate` onto its `codec_context` (the `layout` set before `start_encoding` so the encoder accepts the resampler's `profile.layout` frames), dropping `None` knobs; `OutputContainer.start_encoding` opens the encoder so `codec_context.frame_size`/`format`/`rate` publish the codec's real values (each is `0`/unset until the encoder opens — a `frame_size` read before the open returns `0`, and `AudioResampler(frame_size=0)` is the variable-frame path, so the open is what makes the AAC/Opus rebuffer real); `_master` builds the `av.filter.Graph` `abuffer` -> `loudnorm` -> auto-sink chain only when `profile.master is not None`, and `_mastered` pushes each lifted frame, drains the graph through `_drain` (the `Graph.pull` loop that returns on `BlockingIOError`/`EOFError`), flushes the `loudnorm` lookahead tail with `Graph.push(None)`, and yields the resampler-flush `None` sentinel last; `av.AudioResampler(..., frame_size=ctx.frame_size or None)` converts each mastered (or raw) frame to the encoder's sample format/rate/layout and emits exactly `frame_size`-sample frames, flushed by the `resample(None)` the sentinel drives; the shared `_drive` loop folds `stream.encode(chunk)` over the reframed sequence at each chunk's cumulative output-sample `pts` in the `1/rate` audio time base (the count of samples already encoded, never the chunk index, so the presentation timeline is sample-accurate), muxing each returned `Packet` through `container.mux`, then `_flush` flushes with `stream.encode(None)` muxed at end-of-stream, never a fictional lazy-encode iterator; the muxed `bytes` read off the `BytesIO` sink fold through `MediaEvidence.measure` once, the audio arm reporting `samples / rate` as the exact temporal extent (the cumulative output-sample count over the encoder rate) and the encoded-frame count as the frame count — no video read-back probe, the audio-only container carrying no video stream and its sample fold already exact.
- Receipt: each audio encode contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media` carrying the content key and the `MediaEvidence` facts — container, codec, duration, byte count, frame count, bit rate — keyed by the content key the `media/video#MEDIA` `_emit` arm derives over the muxed bytes; the worker folds `(profile.container, profile.codec, samples / rate, frame_count, int(profile.bit_rate or 0), blob)` through `MediaEvidence.measure` once, and `_emit` spreads the named scalars onto the `ArtifactReceipt.Media` case, the receipt owner importing no `MediaEvidence` value object nor any `av` container handle. This arm contributes the same single `ArtifactReceipt.Media` case the `media/video#MEDIA` container owner contributes — `core/receipt#RECEIPT` cites both as the producers of the one media case, never a parallel audio-receipt rail; the `duration` is the one observable temporal-encode value the runtime `observability/metrics` `MeterProvider` reads off the receipt fold, and the `bit_rate` slot carries the constant-bitrate audio target. The `ArtifactReceipt.Media` case carries the eighth `facts: frozendict[str, float | str]` band (receipt.md, defaulting empty) and `media/video#MEDIA`'s `MediaEvidence.facts` field plus its `measure(..., facts=)` constructor have BOTH landed, so `_encode_audio` threads `_deployment(profile)` through `MediaEvidence.measure(..., facts=...)` and the `media/video#MEDIA` `_keyed` arm spreads `evidence.facts` onto the eighth `Media` slot — audio and video share ONE `facts` path with no cross-file widening outstanding; the `loudnorm`-MEASURED integrated-LUFS / true-peak-dBTP / loudness-range band (a two-pass `loudnorm=print_format=json` read the single-pass encode does not expose) rides `analysis.md`'s `ebur128` measurement, never a parallel audio receipt.
- Growth: a new producer sample dtype is one `_INGEST` row, the lift already deriving its format; a new audio codec is one `MediaProfile.codec` string the `voiced` `add_stream` row reads, the resample/reframe pipeline already adapting to its published `format`/`frame_size`; a producer whose sample rate differs from the encoder rate is one `source.rate` stamp in front of the existing `AudioResampler` (already the rate/format/layout converter, no new surface); a new encode knob (bit rate, VBR mode) is one `MediaProfile` field bound into the `voiced` codec-context fold; a louder mastering chain is one ordered `Stage` in the `Master.stages` tuple (the `dynaudnorm`/`acompressor`/`alimiter`/`afade`/`atempo`/`aformat` constructors already minted, a new libavfilter one more `Stage` smart constructor deriving its arg string), the corrected `_master` linking it in the `link_nodes(source, *nodes, abuffersink)` sequence; a new channel layout is one FFmpeg layout name the `MediaProfile.layout` `str` carries (the `av` calls pass it directly, `av` rejecting a `StrEnum`); a new evidence fact is one `(label, value)` band key on the widened `Media.facts` (the LUFS/true-peak/loudness-range audio-native band once the upstream `MediaEvidence.facts` field lands); the N-source `amix` MIX and a `MediaOp.DecodeAudio` container-contract case are the `filtergraph.md`/`container.md` restructure's, not this in-place surface; zero new surface — the modality space stays the three `media/video#MEDIA` cases (`EncodeVideo`/`EncodeAudio`/`Mux`) on one owner, every addition a row, field, node, or arm.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Iterator
from fractions import Fraction
from typing import Literal

import numpy as np
from beartype import beartype
from expression import Error, Ok, Result
from msgspec import Struct
from numpy.typing import NDArray

from builtins import frozendict

from artifacts.media.video import MediaEvidence, MediaFault, MediaProfile, _deployment, _drive, _flush, _media_fault

lazy import av
lazy import av.error
lazy import av.filter

# --- [TYPES] ----------------------------------------------------------------------------

type Pcm = NDArray[np.int16] | NDArray[np.int32] | NDArray[np.float32] | NDArray[np.float64]

# channel layout rides `MediaProfile.layout` as the FFmpeg layout NAME (`str`) — `av` types it `string | av.AudioLayout`
# and rejects a `StrEnum` member, so it is an open av-name field like `codec`/`pix_fmt`, never a partial-expose local enum.

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
    # one libavfilter mastering node `Graph.add(name, args)` adds; the smart constructors are the
    # closed mastering vocabulary, each deriving its verified `key=value:...` arg string from typed knobs.
    name: str
    args: str = ""

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
    def afade(kind: Literal["in", "out"] = "in", start: float = 0.0, duration: float = 1.0, curve: str = "tri") -> "Stage":
        return Stage("afade", f"type={kind}:start_time={start}:duration={duration}:curve={curve}")

    @staticmethod
    def atempo(factor: float = 1.0) -> "Stage":
        return Stage("atempo", f"tempo={factor}")

    @staticmethod
    def aformat(sample_fmt: str = "fltp", sample_rate: int = 48000, layout: str = "stereo") -> "Stage":
        return Stage("aformat", f"sample_fmts={sample_fmt}:sample_rates={sample_rate}:channel_layouts={layout}")


class Master(Struct, frozen=True):
    # the ordered mastering chain `_master` folds into one `abuffer -> stages -> abuffersink` graph;
    # the default is the single EBU R128 `loudnorm` normalizer, a louder chain composes more ordered `Stage`s
    # (highpass -> acompressor -> alimiter -> loudnorm is the standard broadcast master).
    stages: tuple[Stage, ...] = (Stage.loudnorm(),)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _master(profile: MediaProfile, ingest: str, master: Master) -> av.filter.Graph:
    graph = av.filter.Graph()
    source = graph.add_abuffer(sample_rate=profile.rate, format=ingest, layout=profile.layout, time_base=Fraction(1, profile.rate))
    nodes = tuple(graph.add(stage.name, stage.args) for stage in master.stages)
    graph.link_nodes(source, *nodes, graph.add("abuffersink"))  # explicit audio sink: configure()'s auto_buffer adds no sink for an audio graph (EINVAL)
    graph.configure()
    return graph


def _drain(graph: av.filter.Graph) -> Iterator[av.AudioFrame]:
    while True:  # libavfilter pull protocol: drain until EAGAIN (needs input) or EOF (flushed)
        try:
            yield graph.pull()
        except (BlockingIOError, EOFError):
            return


def _mastered(graph: av.filter.Graph | None, blocks: tuple[Pcm, ...], profile: MediaProfile, ingest: str) -> Iterator[av.AudioFrame | None]:
    for block in blocks:
        source = av.AudioFrame.from_ndarray(block, format=ingest, layout=profile.layout)
        source.rate = profile.rate
        if graph is None:
            yield source
            continue
        graph.push(source)
        yield from _drain(graph)
    if graph is not None:
        graph.push(None)  # flush the chain's lookahead tail (loudnorm/limiter)
        yield from _drain(graph)
    yield None  # the resampler-flush sentinel `resample(None)` consumes last


def _voiced(container: object, stream: object, blocks: tuple[Pcm, ...], profile: MediaProfile) -> tuple[int, int]:
    # the shared audio drive `media/video#MEDIA` `_mux_av` also composes: master -> resample/reframe -> encode -> mux over one
    # already-opened audio stream, returning (frames, samples). The caller opens the encoder (`start_encoding`), flushes it
    # (`_flush`), and reads the container bytes; `_voiced` owns only the per-block lift, master pass, resample, and encode-mux loop.
    ctx = stream.codec_context
    ingest = _INGEST[blocks[0].dtype]
    graph = _master(profile, ingest, profile.master) if profile.master is not None else None
    # frame_size folds the AudioFifo rebuffer into the resampler: one owner converts format/rate/layout and emits ctx.frame_size-sample frames
    resampler = av.AudioResampler(format=ctx.format.name, layout=profile.layout, rate=ctx.rate, frame_size=ctx.frame_size or None)
    frames = samples = 0
    for shaped in _mastered(graph, blocks, profile, ingest):
        for chunk in resampler.resample(shaped):
            _drive(container, stream, chunk, samples, ctx.rate)  # pts is the cumulative output-sample offset in 1/rate, not the chunk index
            frames, samples = frames + 1, samples + chunk.samples
    return frames, samples


@beartype
def _encode_audio(blocks: tuple[Pcm, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # rails identically to the video _encode_video/_transcode/_remux siblings the same media/video#MEDIA `_mux` dispatches:
    # the av FFmpegError leaves map through the shared `_media_fault`, an `av` ImportError onto `provision`, and the `@beartype`
    # weave lifts a hint violation onto the `_mux`-caught `contract` fault — Pcm is a real runtime union here (unlike video's
    # TYPE_CHECKING-only forward ref), so this worker carries the contract weave the un-beartyped `_mux_av` cannot.
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

- [AUDIO_FORMAT] [RESOLVED]: the producer block's `numpy` dtype selects the `AudioFrame.from_ndarray` ingest format through the `_INGEST` `frozendict` correspondence (`int16` -> `s16`, `int32` -> `s32`, `float32` -> `flt`, `float64` -> `dbl`), so an `int16`/`int32`/`float32`/`float64` interleaved-PCM producer all admit with one declared table and no hardcoded sample tag — the prior design's single `_SAMPLE_FORMAT = "s16"` import modelled only the `int16` producer and mislabelled any other dtype. `AudioFrame.from_ndarray(array, format="s16", layout="stereo")` is the `av` `.api` `[03]-[ENTRYPOINTS]` frame-encode table row `[13]`; the `AudioResampler` downstream converts the lifted dtype to the encoder's required `codec_context.format` (read after `start_encoding`), so a `float32` producer feeding an `fltp`-only encoder (AAC, Opus) and an `int16` producer feeding a `s16` encoder (FLAC) each convert cleanly. A new producer dtype is one `_INGEST` row; the lift never re-declares the format.
- [AUDIO_MASTER] [RESOLVED]: the loudness-mastering pass is an `av.filter.Graph` `abuffer` -> `stages` -> `abuffersink` chain normalizing every block to the `Master` EBU R128 target (streaming defaults `I=-16.0:TP=-1.5:LRA=11.0`) before the resampler, built by `_master` only when `profile.master is not None` — `Graph.add_abuffer(sample_rate=, format=, layout=, time_base=)`, `Graph.add(name, args)`, `Graph.link_nodes`, `Graph.push`/`Graph.pull`, and `Graph.configure` are the `av` `.api` filter-graph rows. FIX (verified on `av 17.1.0`): the prior `graph.configure()` relied on the `auto_buffer=True` default adding a sink on the last node's unlinked output — this FAILS for an audio graph with `ArgumentError: Invalid argument (22)`, so `_master` now appends an EXPLICIT `graph.add("abuffersink")` as the terminal `link_nodes` node (`link_nodes(source, *nodes, graph.add("abuffersink"))`), the corrected chain confirmed to configure and drain end-to-end. The Stage mastering vocabulary is now MINTED to depth beyond the lone `loudnorm`/`highpass`/`lowpass`: `dynaudnorm` (dynamic normalizer), `acompressor` (compressor), `alimiter` (brickwall limiter), `afade` (fade in/out over a `Literal["in","out"]` kind), `atempo` (tempo), and `aformat` (sample-format/rate/layout convert over the FFmpeg `sample_fmts`/`sample_rates`/`channel_layouts` args) — each a `Stage` smart constructor deriving its verified `key=value:...` arg string from typed knobs, so the standard broadcast master `highpass -> acompressor -> alimiter -> loudnorm` is `Master(stages=(Stage.highpass(), Stage.acompressor(), Stage.alimiter(), Stage.loudnorm()))`, one ordered tuple the corrected `_master` links in sequence (the multi-node linear chain confirmed to configure+drain). `_drain` drives the `Graph.pull` protocol (returning on `BlockingIOError`/`EOFError`), `_mastered` flushes the lookahead with `Graph.push(None)`, and the resampler normalizes whatever format the chain emits to the encoder's. Cross-file requirement: `MediaProfile.master: Master | None = None` is the video-owned policy slot this arm reads, the `Master` owner imported under `TYPE_CHECKING` so the runtime `None` default closes no `media/video` <-> `media/audio` import cycle.
- [AUDIO_DECODE_MIX] [RESOLVED]: the `_decode_audio` primitive is the verified inverse of `_encode_audio` — `av.open(BytesIO(blob), mode="r")` -> `reader.decode(audio=0)` -> `AudioFrame.to_ndarray()` lazily yielded through `_decoded` (an `Iterator[Pcm]` so a long stream never materializes every block at once, LAZY_COMBINATORS) -> `tuple` at the worker return with `reader.streams.audio[0].sample_rate`, confirmed end-to-end (a round-tripped WAV decoded its blocks and rate). It is a standalone worker primitive, not a MediaOp case, because it produces `Pcm` blocks rather than the MediaOp `(blob, MediaEvidence)` container contract — the media restructure's `timeline`/`analysis` pages compose it, and a `MediaOp.DecodeAudio` dispatch that changes that output contract is the restructure's, not this in-place improve's. The N-source MIX path (`amix`) is DEFERRED to `filtergraph.md`, NOT minted here, because the multi-input `amix` filter graph does not configure through the naive `link_nodes` fan-in NOR the `link_to(amix, 0, input_idx)` form on `av 17.1.0` (both raise at `graph.configure()`); shipping a non-configuring mix graph would be exactly the illusory code the density bar rejects, so the amix N-source combine rides the `filtergraph.md` `FilterNode` owner the brief's [04] restructure declares (RESIDUAL).
- [AUDIO_EVIDENCE] [RESOLVED]: the audio arm's `MediaEvidence` reports `samples / rate` as the temporal extent — the cumulative output-sample count (summed `AudioFrame.samples` across the encoded chunks) over the encoder rate — and the encoded-frame count as the frame count, computed in the encode fold rather than by re-opening the muxed bytes through the `media/video#MEDIA` `_probe`, because `_probe` reads `reader.streams.video[0]` (an audio-only container carries no video stream) and an audio-only re-probe is unreliable (verified on `av 17.1.0`: an Opus/WebM and a FLAC/MKV read-back report `streams.audio[0].frames == 0` while the exact sample fold yields the true count). The byte count is `len(blob)` off the `BytesIO` sink, folded through the shared `MediaEvidence.measure` constructor once (mirroring the `package/codec#COMPRESSION` `BundleEvidence.measure` single-fold pattern). The bundled-`libav` deployment facts ride the `facts` band as the video arm's do: `_encode_audio` threads `_deployment(profile)` (the `av.library_versions` majors + `av.ffmpeg_version_info`; `profile.colored()` is empty for an audio profile) through `MediaEvidence.measure(..., facts=...)`, so an audio artifact carries the same deployment evidence a video one does. The `loudnorm`-MEASURED integrated-LUFS/true-peak-dBTP/loudness-range band needs a two-pass `loudnorm=print_format=json` (or `ebur128`) read the single-pass chain here does not expose, so it rides `analysis.md`'s `ebur128` loudness measurement (the brief's [04] restructure), never a parallel audio receipt.
- [RECEIPT_MEDIA_CASE] [RESOLVED]: the `ArtifactReceipt.Media` case is ALREADY the widened eight-slot `tuple[ContentKey, str, str, float, int, int, int, frozendict[str, float | str]]` (key, container, codec, duration, byte count, frame count, bit rate, per-page `facts` band) on the same-folder `core/receipt#RECEIPT` owner (verified against `receipt.md` line: the `media` tag token, the `Media(key, container, codec, duration, bytes_, frames, bit_rate=0, facts=frozendict())` mint defaulting `facts`, the eight-element `media=(...)` case, and the `media` `_facts` arm flattening the band exactly as `preview` flattens `scores`) — so the receipt SIDE of the audio-native-evidence widening is DONE: the deployment and (future `analysis.md`) loudness facts have a home on the `facts` band, and the `media/video#MEDIA` `_keyed` arm passes the eighth `evidence.facts` argument to the `Media(...)` case (the receipt-side `facts` default keeps a producer that omits it valid). The upstream widening has ALSO landed: `media/video#MEDIA`'s `MediaEvidence` carries the seventh `facts: frozendict[str, float | str]` field and its `measure` constructor accepts `facts`, so this arm threads `_deployment(profile)` through `MediaEvidence.measure(..., facts=...)` and the `_emit` `_keyed` arm spreads `evidence.facts` onto the eighth `Media` slot — audio and video share ONE `facts` path with no cross-file widening outstanding. The remaining `loudnorm`-measured LUFS/true-peak band is `analysis.md`'s `ebur128` concern, not a `MediaEvidence` dependency. No parallel audio-receipt rail is minted; the `Credential` and `Media` cases remain the two producer cases so neither `c2pa-python` nor `av` crosses into the receipt owner.
- [AUDIO_FAULT_RAIL] [RESOLVED]: `_encode_audio` returns `Result[tuple[bytes, MediaEvidence], MediaFault]`, not a bare tuple — the exact worker contract the `media/video#MEDIA` `_mux` `EncodeAudio` arm dispatches (`_WORKER_RETRY(to_process.run_sync, _encode_audio, ...)`) and `_emit` folds through `.map(self._keyed)`, so a bare-tuple return would have been an `AttributeError` at `.map` and a static type miss on `_mux`'s `Result[...]` annotation. The `MediaFault` union and the `_media_fault` av-leaf mapping are `media/video#MEDIA`'s single owners (imported here, never re-declared), and this worker maps its own av raises at the arm that incurs them — `av.error.FFmpegError` -> `_media_fault("encode_audio", exc)`, an `av` `ImportError` -> `MediaFault(provision=...)` — returning the `Result` as picklable data across the `to_process` seam exactly as `_encode_video` does. The `@beartype` weave is real here because `Pcm` is a runtime union on this page (unlike video's TYPE_CHECKING-only forward ref that keeps `_mux_av` un-woven), so a hint violation lifts onto the `_mux`-caught `contract` fault; the transient `BrokenWorkerProcess` retry stays on the video `_mux` dispatch site (`_WORKER_RETRY`), never re-minted here.
