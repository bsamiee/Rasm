# [PY_ARTIFACTS_MEDIA_SYNTHESIS]

`Synthesis` owns the media plane's synthesis arm — the inverse of `media/analysis#ANALYSIS`: it generates an audio waveform from a numpy oscillator/noise/envelope bank and encodes it through the `media/audio#MEDIA` `_encode_audio` worker, reusing that owner's `Pcm`/`MediaProfile`/`Master`/`MediaEvidence`/`MediaFault` vocabulary rather than a second encode surface. `Synthesis` discriminates the generator modalities over a closed `SynthOp` union — oscillator, noise, additive, FM, AM, sweep, impulse — each producing a `float32` mono buffer shaped by an `Adsr` envelope, mastered by the `MediaProfile.master` chain, chunked into `Pcm` blocks, and handed to the shared encoder. Numpy owns the oscillator bank: no `compute` owner exposes `scipy.signal.chirp`/`sawtooth`/`gausspulse` (the `compute/analysis` band is transform/measure, not generation), so the phase-accumulation oscillators, the noise generators, the additive/FM/AM synthesis, the `np.interp` ADSR, and the `np.cumsum` chirp are the generation floor this page owns.

This page composes the `media/audio#MEDIA` encode owner — `_encode_audio`, the `Pcm` producer-dtype union (`float32` selecting the `_INGEST` `flt` ingest with no per-dtype arm), the `MediaProfile` encode policy with its `master` slot, the `Master`/`Stage` mastering vocabulary, and the `WORKER_BAND`/`_WORKER_RETRY` lane — and reads them, re-owning none; it owns the `SynthOp` generator vocabulary, the `Waveform`/`NoiseColor` axes, the `Adsr` envelope, and the numpy generator primitives. `compute/analysis/signal#DSP`'s `Filter`/`Resample` band-limit and `compute/analysis/transform#TRANSFORM`'s spectral verification are the deeper seams a generated signal reaches for shaping and QA; the numpy bank is the self-contained floor. Every generation contributes the shared `core/receipt#RECEIPT` `ArtifactReceipt.Media` case — the synthesis params (`fundamental_hz`/`waveform`/`duration`) on its `facts` band — and enters the `core/plan#PLAN` `ArtifactPipeline` as one root `ArtifactWork` node keyed by content key (a synthesis generates its source, so it has no upstream parent).

## [01]-[INDEX]

- [01]-[SYNTHESIS]: the `Synthesis` owner over the closed `SynthOp` family — the `Oscillator`/`Noise`/`Additive`/`Fm`/`Am`/`Sweep`/`Impulse` numpy generators, each shaped by the `Adsr` envelope and encoded through `media/audio#MEDIA`, folding into the shared `ArtifactReceipt.Media` case.

## [02]-[SYNTHESIS]

- Owner: `Synthesis` discriminates modality over the closed `SynthOp` union, each case carrying its own typed generation payload — never a shared erased `params` bag, a per-generator subclass, or a parallel `sine`/`noise`/`chirp` trio. `Waveform` and `NoiseColor` are values the generator reads, not bodies it re-derives; `Adsr` shapes every buffer through one `np.interp` piecewise gain, applied uniformly so a new generator inherits the envelope for free; `SynthProfile` bundles the `Adsr` envelope and the `MediaProfile` encode policy into one value the op carries, never loose knobs. `media/audio#MEDIA`'s encode family threads unchanged — the `float32` blocks admit through the `_INGEST` `flt` row, the optional `Master.stages` master the signal before encode.
- Cases: the phase is the one `np.cumsum` accumulation both the constant-frequency oscillators and the frequency-varying chirp share, so a swept signal never re-implements phase; the oscillators sum harmonics only up to the Nyquist-safe count so no partial aliases.
- Entry: `Synthesis.of` normalizes construction over a lone `SynthOp`, defaulting the `SynthProfile`, so a caller emits a mastered A440 sine, a pink-noise bed, or a log sweep with one op and no encoder knowledge. `_synthesize` generates, ADSR-shapes, chunks, and hands the blocks to `media/audio#MEDIA` `_encode_audio`, then merges the synthesis band onto the returned `MediaEvidence`.
- Auto: the waveform harmonic recipe is `_HARMONICS[waveform]`; the chirp track is `np.linspace` or `np.geomspace` keyed by the `logarithmic` flag the case carries, never a `mode` string re-parsed; the noise color selects the spectral shaping off the `NoiseColor` value; the `Adsr` envelope is generation-family policy, not a per-case body. `_blocks` chunks the buffer into `(1, _BLOCK)` frames so a long synthesis streams block-by-block.
- Receipt: each generation keys through the audio owner's `ContentIdentity.of(...)` over the encoded bytes and merges the `{fundamental_hz, waveform, duration}` band onto the audio `MediaEvidence.facts` through `msgspec.structs.replace` — the shared `Media` band absorbs the synthesis params by anticipatory collapse (one key per parameter, ZERO case widening), and `synthesis` joins the `core/receipt#RECEIPT` band enumeration for parity, the carrier staying synth-owned so the receipt owner imports no numpy handle.
- Packages: the numpy oscillator/noise/FFT/interp primitives are the generation floor; `media/audio#MEDIA` owns the encode. numpy settled against the shared `.api`.
- Growth: a new oscillator shape is one `Waveform` member plus one `_HARMONICS` row; a new noise color one `NoiseColor` member plus one spectral arm; a new generator modality one `SynthOp` case plus one `_synthesize` arm, `assert_never` breaking the match until it exists; a new envelope stage one `Adsr` field plus one `np.interp` breakpoint; a band-limit or spectral QA the `compute/analysis/signal#DSP`/`compute/analysis/transform#TRANSFORM` cross-branch seam applied pre-encode; a new synthesis fact one band key (ZERO receipt edit).

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


from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.media.audio import (
    Master,
    Pcm,
    _encode_audio,
)  # the shared audio encode owner; float32 blocks admit through _INGEST["flt"]
from artifacts.media.container import ContainerFormat, MediaEvidence, MediaFault, MediaProfile, _WORKER_RETRY

# --- [TYPES] ----------------------------------------------------------------------------

type Partials = tuple[tuple[float, float], ...]  # (hz, amplitude) partial set for Additive

# --- [CONSTANTS] ------------------------------------------------------------------------

_BLOCK: int = 4096  # per-frame chunk so a long synthesis streams block-by-block


class Waveform(StrEnum):
    SINE = "sine"
    SAW = "saw"
    SQUARE = "square"
    TRIANGLE = "triangle"


class NoiseColor(StrEnum):
    WHITE = "white"
    PINK = "pink"
    BROWN = "brown"


# per-waveform recipe: (harmonic-index step, amplitude law, alternating sign); the others sum to the Nyquist-safe count so no aliases.
type Harmonic = tuple[int, Literal["inverse", "inverse_square"], bool]
_HARMONICS: frozendict[Waveform, Harmonic] = frozendict({
    Waveform.SINE: (1, "inverse", False),  # fundamental only
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
        # one np.interp piecewise ramp over the ADSR breakpoints.
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

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key over the INPUT op minted pre-run; the output's own address rides the receipt facts.
        return ContentIdentity.of(f"media.synthesis-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the member MediaFault folds into the boundary fault (Work[ArtifactReceipt] forbids an inner Result).
        railed = await async_boundary(f"media.synthesis.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map(lambda pair: pair[1]).map_error(
                lambda fault: BoundaryFault(boundary=(f"media.synthesis.{self.op.tag}", f"{fault.tag}:{fault}"))
            )
        )

    async def _folded(self, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
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
    # the ONE phase accumulation the oscillators and the chirp share: cumsum of 2π·f/rate, so a swept frequency never re-implements phase.
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
    # chunk the mono float32 buffer into (1, _BLOCK) frames the media/audio _INGEST["flt"] row admits.
    packed = buffer.astype(np.float32).reshape(1, -1)
    return tuple(packed[:, start : start + _BLOCK] for start in range(0, packed.shape[1], _BLOCK))


@beartype
def _synthesize(op: SynthOp, profile: SynthProfile, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
    # generate -> ADSR-shape -> chunk -> media/audio encode; merge the synthesis band onto the returned MediaEvidence.
    rate = profile.media.rate
    buffer = _shaped(_generated(op, rate, profile.seed), profile.envelope, rate)
    band = frozendict({"fundamental_hz": op.fundamental, "waveform": op.tag, "duration": buffer.size / rate})
    return _encode_audio(_blocks(buffer), profile.media).map(lambda pair: _keyed(pair, band))


def _keyed(pair: tuple[bytes, MediaEvidence], band: frozendict[str, float | str], /) -> tuple[ContentKey, ArtifactReceipt]:
    # the same ContentIdentity.of(container, blob) key the media/audio#MEDIA _emit arm derives — by-reference, not re-minted.
    blob, evidence = pair
    merged = msgspec.structs.replace(evidence, facts=evidence.facts | band)
    key = ContentIdentity.of(evidence.container.value, blob)
    return key, ArtifactReceipt.Media(
        key, merged.container.value, merged.codec, merged.duration, merged.byte_count, merged.frame_count, merged.bit_rate, merged.facts
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
