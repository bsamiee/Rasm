# [PY_ARTIFACTS_MEDIA_SYNTHESIS]

The temporal-artifact SYNTHESIS arm — the INVERSE of `media/analysis#ANALYSIS`: it GENERATES an audio waveform from a numpy oscillator/noise/envelope bank and ENCODES it into a container through the `media/audio#MEDIA` `_encode_audio` worker, reusing that owner's `Pcm`/`MediaProfile`/`Master`/`MediaEvidence`/`MediaFault` vocabulary rather than a second encode surface. `Synthesis` is a frozen `msgspec.Struct` whose op is ONE closed-payload `SynthOp` `expression.tagged_union` over the generator modalities — `Oscillator(waveform, hz, seconds, amplitude)`, `Noise(color, seconds, amplitude)`, `Additive(partials, seconds)`, `Fm(carrier_hz, ratio, index, seconds)`, `Am(carrier_hz, rate_hz, depth, seconds)`, `Sweep(low_hz, high_hz, seconds, logarithmic)`, and `Impulse(seconds)` — dispatched by one total `match`/`case` closed by `assert_never`, each producing a numpy `float32` mono buffer, shaped by an `Adsr` envelope and mastered by the `MediaProfile.master` chain, chunked into `Pcm` blocks, and handed to `media/audio#MEDIA` `_encode_audio` for the container/mux, returning `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]` keyed over the encoded bytes, never a parallel `make_sine`/`make_noise`/`make_chirp` function family nor a second audio encoder. The oscillator bank is numpy-native — verified that NO `compute` owner exposes `scipy.signal.chirp`/`sawtooth`/`gausspulse` (the `compute/analysis` band is transform/measure, not generation) — so the phase-accumulation `np.cumsum` oscillators (band-limited saw/square/triangle by odd/even harmonic sum), the `np.random.Generator` white/pink/brown noise, the additive/FM/AM synthesis, the `np.interp` piecewise ADSR, and the `np.cumsum`-of-an-instantaneous-frequency-track chirp are the generation floor this page OWNS. This page composes the `media/audio#MEDIA` encode owner — `_encode_audio`, the `Pcm` producer-dtype union (`float32` selecting the `flt`/`fltp` ingest through the `_INGEST` table with no per-dtype arm), the `MediaProfile` encode-policy value with its `master` mastering slot, the `Master`/`Stage` mastering vocabulary applied pre-encode, the `MediaEvidence`/`MediaFault` receipt/fault family, and the `WORKER_BAND`/`_WORKER_RETRY` subprocess lane — and READS them, re-owning none; it OWNS the `SynthOp` generator vocabulary, the `Waveform`/`NoiseColor` value axes, the `Adsr` envelope value, and the numpy generator primitives. The cross-branch `compute/analysis/signal#DSP` `SignalOp.Filter`/`Resample` band-limit and the `compute/analysis/transform#TRANSFORM` spectral verification are the deeper seams a generated signal reaches for shaping and QA; the numpy oscillator bank is the self-contained floor. Every generation contributes the single `ArtifactReceipt.Media` case — the synthesis parameters (`fundamental_hz`/`waveform`/`duration`) riding its `facts` band — never a parallel synth-receipt rail, and routes through the one `core/plan#PLAN` `ArtifactPipeline` entry as an `ArtifactWork` node keyed by its content key. This is the synthesis leaf of the media plane.

## [01]-[INDEX]

- [01]-[SYNTHESIS]: the `Synthesis` owner over the closed-payload `SynthOp` family — `Oscillator` the `Waveform` phase-accumulation generator (`np.cumsum` phase, `np.sin` for `SINE`, the band-limited odd/even harmonic sum for `SAW`/`SQUARE`/`TRIANGLE`), `Noise` the `NoiseColor` generator (`np.random.default_rng().standard_normal` white, the FFT `1/sqrt(f)` pink, the `np.cumsum` brown), `Additive` the harmonic-partial sum, `Fm`/`Am` the frequency/amplitude modulation pair, `Sweep` the `np.cumsum`-of-instantaneous-frequency chirp (linear or logarithmic), and `Impulse` the click — each generating one `float32` mono buffer shaped by the `Adsr` `np.interp` envelope, mastered by the `MediaProfile.master` `Master.stages` chain, chunked into `Pcm` blocks, and encoded by `media/audio#MEDIA` `_encode_audio`, folding into the single `ArtifactReceipt.Media` case with the synthesis params on its `facts` band; `numpy` `cumsum`/`sin`/`sign`/`arctan`/`interp`/`random.default_rng`/`fft.rfft`/`fft.irfft`/`hanning`/`clip`/`concatenate` settled against the shared `.api`. The `_encode_audio`/`Pcm`/`_INGEST`/`MediaProfile`/`Master`/`Stage`/`MediaEvidence`/`MediaFault`/`WORKER_BAND`/`_WORKER_RETRY` family is `media/audio#MEDIA`'s (itself composing `media/container#CONTAINER`); this page owns the `SynthOp` generator vocabulary, the `Waveform`/`NoiseColor` axes, the `Adsr` envelope, and the numpy generator primitives.

## [02]-[SYNTHESIS]

- Owner: `Synthesis` the one generator owner discriminating modality over the closed `SynthOp` family, worked by the `_synthesize` module-level function on the `WORKER_BAND` subprocess lane; `SynthOp` an `expression.tagged_union` whose every case carries its own typed generation payload (an oscillator `(waveform, hz, seconds, amplitude)`, a noise `(color, seconds, amplitude)`, an additive partial set, an FM/AM triple, a chirp `(low_hz, high_hz, seconds, logarithmic)`, an impulse `seconds`), never a shared erased `params` bag nor a per-generator `Synthesis` subclass nor a parallel `sine`/`noise`/`chirp` function trio; `Waveform` the closed `SINE`/`SAW`/`SQUARE`/`TRIANGLE` `StrEnum` selecting the oscillator harmonic structure and `NoiseColor` the `WHITE`/`PINK`/`BROWN` `StrEnum` selecting the spectral shape, each a value the generator reads not a body it re-derives; `Adsr` the frozen envelope value (`attack`/`decay`/`sustain`/`release` seconds and the sustain level) shaping every generated buffer through one `np.interp` piecewise gain, applied uniformly across the generator family so a new generator inherits the envelope for free; `SynthProfile` the frozen synth-policy value bundling the `Adsr` envelope and the `MediaProfile` encode policy (the container/codec/rate/layout and the `master` mastering slot), so the generation shaping and the encode policy are ONE value the op carries, never loose knobs; `MediaProfile`/`Master`/`Stage`/`Pcm`/`MediaEvidence`/`MediaFault` the encode family `media/audio#MEDIA` owns, threaded unchanged — the `float32` generated blocks admit through the `_INGEST` `flt` row with no per-dtype arm, the optional `Master.stages` (the `highpass`/`acompressor`/`alimiter`/`loudnorm` broadcast chain) master the synthesized signal before encode, and the `MediaFault` av-boundary vocabulary rails the encode; `Result[(ContentKey, ArtifactReceipt), MediaFault]` the one carrier keyed over the encoded bytes through the `media/audio#MEDIA` `ContentIdentity.of(container, blob)` derivation so an identical synthesis at an identical profile is a cache hit by reference. The owner owns no second audio encoder, no per-waveform generator owner, and no parallel `synthesize` surface beside `media/audio` — the generation is one `SynthOp` case producing a numpy buffer, the encode the shared `_encode_audio` worker.
- Cases: `SynthOp` cases — `Oscillator(waveform, hz, seconds, amplitude)` (`np.cumsum` of the constant per-sample phase increment `2π·hz/rate`, then `np.sin` for `SINE` or the band-limited harmonic sum for `SAW` (all harmonics `1/k`), `SQUARE` (odd harmonics `1/k`), and `TRIANGLE` (odd harmonics `(-1)^m/k²`), summed only up to the Nyquist-safe harmonic count so no partial aliases) · `Noise(color, seconds, amplitude)` (`np.random.default_rng(seed).standard_normal` white, the `np.fft.rfft` white spectrum scaled by `1/sqrt(f)` then `irfft` pink, the `np.cumsum(white)` DC-removed brown) · `Additive(partials, seconds)` (the `(hz, amplitude)` partial set summed as sines over the sample grid) · `Fm(carrier_hz, ratio, index, seconds)` (`np.sin(carrier_phase + index·np.sin(ratio·carrier_phase))`, the modulator a ratio of the carrier) · `Am(carrier_hz, rate_hz, depth, seconds)` (`(1 + depth·np.sin(mod_phase))·np.sin(carrier_phase)`) · `Sweep(low_hz, high_hz, seconds, logarithmic)` (the chirp: `np.cumsum` of an instantaneous-frequency track linear or `np.geomspace`-logarithmic from `low_hz` to `high_hz`, scaled to phase, then `np.sin`) · `Impulse(seconds)` (a unit click at t=0, the impulse response probe) — matched by one total `match`/`case`, the seven-case modality recovered from the `SynthOp` discriminant, never a name suffix; every case returns one `float32` mono buffer the `_shaped` envelope and the profile master then process.
- Entry: `Synthesis.produce` is `async` over the runtime `async_boundary` returning `RuntimeRail[Result[(ContentKey, ArtifactReceipt), MediaFault]]`; `_emit` maps the `_dispatch` outcome, and `_dispatch` runs the whole synchronous generate-shape-master-encode body on `_WORKER_RETRY(to_process.run_sync, _synthesize, op, profile, limiter=WORKER_BAND)` catching `BrokenWorkerProcess` -> `MediaFault(worker=...)` and `BeartypeCallHintViolation` -> `MediaFault(contract=...)`, exactly as the audio/container owners do. The `_synthesize` worker generates the numpy buffer through the `SynthOp` match, applies the `Adsr` envelope through `_shaped`, chunks it into `Pcm` blocks through `_blocks`, and hands the blocks to `media/audio#MEDIA` `_encode_audio(blocks, profile.media)` — which opens the `av.open(sink, "w")` container, masters each block through `profile.media.master` (the `Master.stages` `loudnorm`/`alimiter` chain), resamples/reframes to the encoder's `frame_size`, drives the encode-mux loop, and returns `Result[(bytes, MediaEvidence), MediaFault]` — then merges the synthesis `facts` band onto the returned evidence through `msgspec.structs.replace`. `Synthesis.of` normalizes the construction over a lone `SynthOp` (defaulting the `SynthProfile`), so a caller emits a mastered A440 sine, a pink-noise bed, or a log sweep with one op and no encoder knowledge.
- Auto: the waveform harmonic structure is `Waveform`, so a producer never re-derives the partial series and a new oscillator shape is one `StrEnum` member plus one `_HARMONICS` row; the chirp instantaneous-frequency track is `np.linspace` or `np.geomspace` keyed by the `logarithmic` flag the case carries (never a `mode` string re-parsed), and the phase is the ONE `np.cumsum` accumulation both the oscillator and the chirp share so a frequency-varying signal never re-implements phase; the noise color selects the spectral shaping (white passthrough, the FFT `1/sqrt(f)` pink, the `np.cumsum` brown) off the `NoiseColor` value; the `Adsr` envelope is one `np.interp` over the `(0, attack, attack+decay, total-release, total)` breakpoints against the `(0, 1, sustain, sustain, 0)` gains, applied to every buffer uniformly so the envelope is generation-family policy not a per-case body; the `float32` block dtype selects the `_INGEST` `flt` ingest at `media/audio#MEDIA` with no per-dtype arm, and the `MediaProfile.layout` (mono here) rides the audio owner's existing resample/reframe path; `_blocks` chunks the generated buffer into `(1, block)` `float32` frames so a long synthesis streams block-by-block through `_encode_audio` rather than one giant frame; the synthesis `facts` band (`fundamental_hz`, `waveform`, `duration`) merges onto the `MediaEvidence.facts` the audio encode already carries (the bundled-libav deployment band) through `msgspec.structs.replace`, so the receipt carries both the encode and the synthesis evidence with no second fold.
- Receipt: each generation contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate, facts)` through the `media/audio#MEDIA` encode — the audio container/codec/duration/frame/bit-rate facts plus the synthesis `{fundamental_hz, waveform, duration}` band merged onto the `facts` slot. `receipt.md`'s band enumeration lists `container`/`filtergraph`/`audio`/`subtitle`/`analysis` but not `synthesis` by name; the shared `Media` `facts` band still absorbs the synthesis params by the anticipatory collapse the case was shaped for (one band key per parameter, ZERO case widening), and `synthesis` is added to the band-enumeration prose for parity. The media pages all contribute this single `Media` case, never a parallel synth-receipt rail, and the synthesis carrier stays synth-owned — the receipt owner imports no numpy handle. The producer keys through the audio owner's `ContentIdentity.of(...)` over the encoded bytes and enters the `core/plan#PLAN` `ArtifactPipeline` as one `ArtifactWork` node (a root — a synthesis has no upstream content-key parent, it generates its source).
- Growth: a new oscillator shape is one `Waveform` member plus one `_HARMONICS` row (the phase accumulation already deriving its samples); a new noise color is one `NoiseColor` member plus one spectral-shaping arm; a new generator modality (a `Karplus` plucked string, a `Wavetable` lookup, a `Granular` cloud) is one `SynthOp` case plus one `_synthesize` arm, the `assert_never` tail breaking the match until the arm exists; a new envelope stage (a hold segment, an exponential curve) is one `Adsr` field plus one `np.interp` breakpoint; a band-limit or spectral QA is the `compute/analysis/signal#DSP` `Filter`/`Resample` or `compute/analysis/transform#TRANSFORM` cross-branch seam applied to the generated buffer pre-encode; a new synthesis fact is one band key merged onto `MediaEvidence.facts`, ZERO receipt edit; zero new surface — the modality space stays the seven `SynthOp` cases on one owner, every addition a case, member, row, field, or band key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Literal, assert_never

import msgspec
import numpy as np
from anyio import BrokenWorkerProcess, to_process
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from builtins import frozendict

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.receipt import ArtifactReceipt
from artifacts.media.audio import (
    Master,
    Pcm,
    _encode_audio,
)  # the shared audio encode owner; float32 blocks admit through _INGEST["flt"] (av deferred inside the audio module)
from artifacts.media.container import ContainerFormat, MediaEvidence, MediaFault, MediaProfile, _WORKER_RETRY

# --- [TYPES] ----------------------------------------------------------------------------

type Partials = tuple[tuple[float, float], ...]  # (hz, amplitude) partial set for Additive

# --- [CONSTANTS] ------------------------------------------------------------------------

_BLOCK: int = 4096  # per-frame sample chunk so a long synthesis streams block-by-block


class Waveform(StrEnum):
    SINE = "sine"
    SAW = "saw"
    SQUARE = "square"
    TRIANGLE = "triangle"


class NoiseColor(StrEnum):
    WHITE = "white"
    PINK = "pink"
    BROWN = "brown"


# the band-limited harmonic recipe per waveform: (harmonic-index step, amplitude law, alternating sign) — the SINE
# row is the single fundamental, the others sum partials up to the Nyquist-safe count so no harmonic aliases.
type Harmonic = tuple[int, Literal["inverse", "inverse_square"], bool]
_HARMONICS: frozendict[Waveform, Harmonic] = frozendict({
    Waveform.SINE: (1, "inverse", False),  # fundamental only (max_harmonic pinned to 1 in `_oscillator`)
    Waveform.SAW: (1, "inverse", False),  # every harmonic, 1/k
    Waveform.SQUARE: (2, "inverse", False),  # odd harmonics, 1/k
    Waveform.TRIANGLE: (2, "inverse_square", True),  # odd harmonics, (-1)^m/k^2
})

# --- [MODELS] ---------------------------------------------------------------------------


class Adsr(Struct, frozen=True):
    attack: float = 0.01
    decay: float = 0.1
    sustain: float = 0.7  # sustain level 0..1
    release: float = 0.2

    def gain(self, samples: int, rate: int, /) -> NDArray[np.float64]:
        # one np.interp piecewise ramp over the ADSR breakpoints; a generator inherits this shaping uniformly.
        total = samples / rate
        knots = np.array([0.0, self.attack, self.attack + self.decay, max(total - self.release, self.attack + self.decay), total])
        levels = np.array([0.0, 1.0, self.sustain, self.sustain, 0.0])
        return np.interp(np.arange(samples) / rate, knots, levels)


class SynthProfile(Struct, frozen=True):
    media: MediaProfile = MediaProfile(container=ContainerFormat.WEBM, codec="libopus", rate=48000, layout="mono", master=Master())
    envelope: Adsr = Adsr()
    seed: int = 0


@tagged_union(frozen=True)
class SynthOp:
    tag: Literal["oscillator", "noise", "additive", "fm", "am", "sweep", "impulse"] = tag()
    oscillator: tuple[Waveform, float, float, float] = case()  # (waveform, hz, seconds, amplitude)
    noise: tuple[NoiseColor, float, float] = case()  # (color, seconds, amplitude)
    additive: tuple[Partials, float] = case()  # (partials, seconds)
    fm: tuple[float, float, float, float] = case()  # (carrier_hz, ratio, index, seconds)
    am: tuple[float, float, float, float] = case()  # (carrier_hz, rate_hz, depth, seconds)
    sweep: tuple[float, float, float, bool] = case()  # (low_hz, high_hz, seconds, logarithmic)
    impulse: float = case()  # seconds

    @staticmethod
    def Oscillator(waveform: Waveform, hz: float, seconds: float, amplitude: float = 0.8, /) -> "SynthOp":
        return SynthOp(oscillator=(waveform, hz, seconds, amplitude))

    @staticmethod
    def Noise(color: NoiseColor, seconds: float, amplitude: float = 0.5, /) -> "SynthOp":
        return SynthOp(noise=(color, seconds, amplitude))

    @staticmethod
    def Additive(partials: Partials, seconds: float, /) -> "SynthOp":
        return SynthOp(additive=(partials, seconds))

    @staticmethod
    def Fm(carrier_hz: float, ratio: float, index: float, seconds: float, /) -> "SynthOp":
        return SynthOp(fm=(carrier_hz, ratio, index, seconds))

    @staticmethod
    def Am(carrier_hz: float, rate_hz: float, depth: float, seconds: float, /) -> "SynthOp":
        return SynthOp(am=(carrier_hz, rate_hz, depth, seconds))

    @staticmethod
    def Sweep(low_hz: float, high_hz: float, seconds: float, logarithmic: bool = False, /) -> "SynthOp":
        return SynthOp(sweep=(low_hz, high_hz, seconds, logarithmic))

    @staticmethod
    def Impulse(seconds: float = 1.0, /) -> "SynthOp":
        return SynthOp(impulse=seconds)

    @property
    def fundamental(self) -> float:
        match self:
            case SynthOp(tag="oscillator", oscillator=(_, hz, *_)) | SynthOp(tag="fm", fm=(hz, *_)) | SynthOp(tag="am", am=(hz, *_)):
                return hz
            case SynthOp(tag="sweep", sweep=(low, *_)):
                return low
            case SynthOp(tag="additive", additive=(partials, _)):
                return partials[0][0] if partials else 0.0
            case SynthOp(tag="noise") | SynthOp(tag="impulse"):
                return 0.0
            case _ as unreachable:
                assert_never(unreachable)


class Synthesis(Struct, frozen=True):
    op: SynthOp
    profile: SynthProfile = SynthProfile()

    @staticmethod
    def of(op: SynthOp, profile: SynthProfile = SynthProfile(), /) -> "Synthesis":
        return Synthesis(op=op, profile=profile)

    async def produce(self, /) -> RuntimeRail[Result[tuple[ContentKey, ArtifactReceipt], MediaFault]]:
        return await async_boundary(f"media.synthesis.{self.op.tag}", self._emit)

    async def _emit(self, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
        return await self._dispatch()

    async def _dispatch(self, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
        try:
            return await _WORKER_RETRY(to_process.run_sync, _synthesize, self.op, self.profile, limiter=WORKER_BAND)
        except BrokenWorkerProcess as broken:
            return Error(MediaFault(worker=str(broken)))
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=type(violation).__name__))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _phase(hz: NDArray[np.float64] | float, samples: int, rate: int, /) -> NDArray[np.float64]:
    # the ONE phase accumulation both the constant-frequency oscillators and the frequency-varying chirp share:
    # cumulative sum of the per-sample increment 2π·f/rate, so a swept frequency never re-implements phase.
    increment = 2.0 * np.pi * np.asarray(hz, np.float64) / rate
    return np.cumsum(np.broadcast_to(increment, (samples,)))


def _oscillator(waveform: Waveform, hz: float, samples: int, rate: int, /) -> NDArray[np.float64]:
    phase = _phase(hz, samples, rate)
    step, law, alternate = _HARMONICS[waveform]
    ceiling = 1 if waveform is Waveform.SINE else max(1, int((rate / 2) / hz))
    orders = range(1, ceiling + 1, step)
    weight = (lambda k: 1.0 / k) if law == "inverse" else (lambda k: 1.0 / (k * k))
    sign = (lambda m: (-1.0) ** m) if alternate else (lambda _m: 1.0)
    return sum((sign(m) * weight(k) * np.sin(k * phase) for m, k in enumerate(orders)), start=np.zeros(samples))


def _noise(color: NoiseColor, samples: int, seed: int, /) -> NDArray[np.float64]:
    white = np.random.default_rng(seed).standard_normal(samples)
    match color:
        case NoiseColor.WHITE:
            return white
        case NoiseColor.PINK:
            spectrum = np.fft.rfft(white)
            shaped = spectrum / np.sqrt(np.arange(1, spectrum.size + 1))
            return np.fft.irfft(shaped, n=samples)
        case NoiseColor.BROWN:
            walk = np.cumsum(white)
            return walk - walk.mean()
        case _ as unreachable:
            assert_never(unreachable)


def _generated(op: SynthOp, rate: int, seed: int, /) -> NDArray[np.float64]:
    match op:
        case SynthOp(tag="oscillator", oscillator=(waveform, hz, seconds, amplitude)):
            samples = int(seconds * rate)
            return amplitude * _oscillator(waveform, hz, samples, rate)
        case SynthOp(tag="noise", noise=(color, seconds, amplitude)):
            raw = _noise(color, int(seconds * rate), seed)
            return amplitude * raw / (np.abs(raw).max() + 1e-9)
        case SynthOp(tag="additive", additive=(partials, seconds)):
            samples = int(seconds * rate)
            return sum((amp * np.sin(_phase(hz, samples, rate)) for hz, amp in partials), start=np.zeros(samples))
        case SynthOp(tag="fm", fm=(carrier_hz, ratio, index, seconds)):
            samples = int(seconds * rate)
            return np.sin(_phase(carrier_hz, samples, rate) + index * np.sin(_phase(carrier_hz * ratio, samples, rate)))
        case SynthOp(tag="am", am=(carrier_hz, rate_hz, depth, seconds)):
            samples = int(seconds * rate)
            return (1.0 + depth * np.sin(_phase(rate_hz, samples, rate))) * np.sin(_phase(carrier_hz, samples, rate))
        case SynthOp(tag="sweep", sweep=(low_hz, high_hz, seconds, logarithmic)):
            samples = int(seconds * rate)
            track = np.geomspace(low_hz, high_hz, samples) if logarithmic else np.linspace(low_hz, high_hz, samples)
            return np.sin(np.cumsum(2.0 * np.pi * track / rate))
        case SynthOp(tag="impulse", impulse=seconds):
            buffer = np.zeros(int(seconds * rate))
            buffer[0] = 1.0
            return buffer
        case _ as unreachable:
            assert_never(unreachable)


def _shaped(buffer: NDArray[np.float64], envelope: Adsr, rate: int, /) -> NDArray[np.float64]:
    return np.clip(buffer * envelope.gain(buffer.size, rate), -1.0, 1.0)


def _blocks(buffer: NDArray[np.float64], /) -> tuple[Pcm, ...]:
    # chunk the mono float32 buffer into (1, _BLOCK) packed frames the media/audio _INGEST["flt"] row admits.
    packed = buffer.astype(np.float32).reshape(1, -1)
    return tuple(packed[:, start : start + _BLOCK] for start in range(0, packed.shape[1], _BLOCK))


@beartype
def _synthesize(op: SynthOp, profile: SynthProfile, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
    # generate -> ADSR-shape -> chunk -> hand to the shared media/audio encode (which masters + resamples + muxes),
    # then merge the synthesis band onto the returned MediaEvidence and key over the encoded bytes.
    rate = profile.media.rate
    buffer = _shaped(_generated(op, rate, profile.seed), profile.envelope, rate)
    band = frozendict({"fundamental_hz": op.fundamental, "waveform": op.tag, "duration": buffer.size / rate})
    return _encode_audio(_blocks(buffer), profile.media).map(lambda pair: _keyed(pair, band))


def _keyed(pair: tuple[bytes, MediaEvidence], band: frozendict[str, float | str], /) -> tuple[ContentKey, ArtifactReceipt]:
    # the same `ContentIdentity.of(container, blob)` derivation the media/audio#MEDIA `_emit` arm keys over the
    # encoded bytes — a by-reference cache key, never a re-minted identity beside the audio owner's.
    blob, evidence = pair
    merged = msgspec.structs.replace(evidence, facts=evidence.facts | band)
    key = ContentIdentity.of(evidence.container.value, blob)
    return key, ArtifactReceipt.Media(
        key, merged.container.value, merged.codec, merged.duration, merged.byte_count, merged.frame_count, merged.bit_rate, merged.facts
    )
```

## [03]-[RESEARCH]

- [NUMPY_NATIVE_GENERATION] [RESOLVED]: the oscillator bank is numpy-native — verified that NO `compute` owner exposes a generation primitive (`compute/analysis` owns `transform`/`signal`/`spatial`/`symbolic` — DFT/DCT/Hankel/analytic transforms and Welch/spectrogram/wavelet ANALYSIS, not `scipy.signal.chirp`/`sawtooth`/`square`/`gausspulse` generation), so the phase-accumulation `np.cumsum` oscillators, `np.random.default_rng` noise, additive/FM/AM synthesis, `np.interp` ADSR, and the `np.cumsum`-of-instantaneous-frequency chirp stay the numpy floor this page owns rather than a phantom `compute` import. All members verified live: `np.cumsum`/`np.sin`/`np.interp`/`np.random.default_rng`/`np.fft.rfft`/`np.fft.irfft`/`np.geomspace`/`np.linspace`/`np.clip`/`np.concatenate`. The band-limited saw/square/triangle are the odd/even harmonic sums up to the Nyquist-safe order so a partial never aliases — the depth the naive "sawtooth via `2*(t*f % 1) - 1`" ignores. CROSS-BRANCH (RESIDUAL): `compute/analysis/signal#DSP` `SignalOp.Filter`/`Resample` band-limits the generated signal and `compute/analysis/transform#TRANSFORM` verifies its spectral content — the deeper QA seams a generated buffer reaches, composed pre-encode, not re-imported into the generation floor.
- [AUDIO_ENCODE_COMPOSE] [RESOLVED]: the encode is the shared `media/audio#MEDIA` `_encode_audio(blocks, profile)` — the generated `float32` mono buffer chunks into `(1, _BLOCK)` packed frames the `_INGEST` `flt` row admits with no per-dtype arm, the `MediaProfile.master` `Master.stages` mastering chain (the `loudnorm`/`alimiter` broadcast master) processes the signal before encode exactly as it masters a decoded producer, and `_encode_audio` opens the `av.open(sink,"w")` container, resamples/reframes to the encoder `frame_size`, drives the encode-mux loop, and returns `Result[(bytes, MediaEvidence), MediaFault]` — never a second encoder minted here. The `MediaProfile` defaults to a `libopus`/`webm` 48 kHz mono profile with a `Master()` mastering pass; a caller overrides the container/codec/rate/master through the one `SynthProfile.media` value. The synthesis `{fundamental_hz, waveform, duration}` band merges onto the `MediaEvidence.facts` (the bundled-libav deployment band the audio encode already carries) through `msgspec.structs.replace`, so one receipt carries both encode and synthesis evidence.
- [ADSR_AND_MODULATION] [RESOLVED]: the `Adsr` envelope is one `np.interp` over the `(0, attack, attack+decay, total-release, total)` time knots against the `(0, 1, sustain, sustain, 0)` gain levels, applied uniformly to every generated buffer so a new generator inherits the shaping for free (generation-family policy, not a per-case body); FM is `np.sin(carrier_phase + index·np.sin(ratio·carrier_phase))` with the modulator a ratio of the carrier, AM is `(1 + depth·np.sin(mod_phase))·np.sin(carrier_phase)`, and the chirp is `np.sin(np.cumsum(2π·track/rate))` over a `np.linspace`/`np.geomspace` instantaneous-frequency track — the `np.cumsum` phase accumulation shared by the constant-frequency oscillators and the swept chirp, so a frequency-varying signal never re-implements phase. A new modulation modality (a `Karplus` string, a `Wavetable` lookup) is one `SynthOp` case; a new envelope stage is one `Adsr` field plus one `np.interp` breakpoint.
- [RECEIPT_MEDIA_CASE] [RESOLVED]: each generation contributes the EXISTING `ArtifactReceipt.Media` case through the `media/audio#MEDIA` encode — the audio container/codec/duration/frame/bit-rate facts plus the synthesis `{fundamental_hz, waveform, duration}` band merged onto the shared `facts` slot. `receipt.md`'s band enumeration (line 14) lists `container`/`filtergraph`/`audio`/`subtitle`/`analysis` but not `synthesis` by name; the shared `Media` `facts` band still absorbs the synthesis params by the anticipatory collapse the case was shaped for (one band key per parameter, ZERO case widening), and `synthesis` is added to that prose enumeration for parity, no receipt-case edit. The single `Media` case, never a parallel synth-receipt rail — the receipt owner imports no numpy handle. The producer keys over the encoded bytes and enters `core/plan#PLAN` `ArtifactPipeline` as one root `ArtifactWork` node (a synthesis generates its source, so it has no upstream content-key parent). CROSS-FILE (RESIDUAL): the `ContentKey` derivation over the encoded bytes rides the `media/audio#MEDIA` `_emit` arm's `ContentIdentity.of(container, blob)`; the `_keyed` seam here names the shared derivation the audio owner exposes, not a re-minted key.
