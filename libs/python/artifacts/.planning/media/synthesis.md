# [PY_ARTIFACTS_MEDIA_SYNTHESIS]

`Synthesis` owns media generation through one closed `SynthOp`: harmonic oscillators, duty-cycle pulse, periodic wavetable, spectral-color noise, additive/FM/AM, sweep, unit impulse, and calibration video. Video cases cover SMPTE-style `75%` bars with PLUGE bands, ramp, grid, countdown, checker, standalone PLUGE, and radial zone plate. Audio produces `float32` mono `Pcm` blocks through `_encode_audio`, video produces `rgb24` frames through `_encode_video`, tone-bearing bars compose both through `_mux_av`, and `MediaProfile`, `MediaEvidence`, and `MediaFault` keep encode policy and evidence.

`_admitted` validates only the selected mode's profile and payload fields — each arm carries its own bounds. `_phase` gives every periodic generator one zero-origin cumulative phase, `_TINT` derives noise colors from one spectral-exponent table, `_framed` paints test patterns from typed per-mode payloads, and ADSR shapes continuous audio while `Impulse` preserves its unit sample. Every generation projects its full mode parameters into `ArtifactReceipt.Media.facts` and enters `ArtifactPipeline` as one root `ArtifactWork` keyed by `SynthOp` with only its relevant `SynthProfile` policy.

## [01]-[INDEX]

- [01]-[SYNTHESIS]: `Synthesis` owns the closed audio and calibration-video `SynthOp` family, admits every payload once, encodes through the media spine, and folds full generation parameters into `ArtifactReceipt.Media`.

## [02]-[SYNTHESIS]

- Owner: `Synthesis` discriminates modality over `SynthOp`; every case carries its typed payload, `domain` derives audio versus video, and `SynthProfile` carries audio policy, video policy, envelope, deterministic noise seed, and harmonic ceiling. `Waveform`, `_HARMONICS`, `NoiseColor`, and `_TINT` keep roster growth in data over shared kernels.
- Cases: `_phase` is polymorphic over scalar or per-sample frequency and starts at zero. `_oscillator` sums only Nyquist-safe harmonics and normalizes the band-limited series; `_wavetable` interpolates a periodic table; `Pulse` derives duty from phase; `Sweep` selects linear or geometric frequency data. Video painters cover bars/PLUGE, ramp, grid, countdown, checker, and zone-plate calibration families.
- Entry: `Synthesis.of` binds one `SynthOp` and `SynthProfile` under the composition-root `lane`, so every factory-built owner is fully initialized. `_synthesize` admits once, `_encoded` dispatches by derived `domain`, and the container `_worker` aspect maps call-contract violations to `MediaFault.contract`. `LanePolicy.offload` maps outer `BoundaryFault` through `_lapsed` and flattens the worker `Result`.
- Auto: `_HARMONICS[waveform]`, `_TINT[color]`, scalar-or-track `_phase`, and `_framed` per-mode payloads drive generation. `_blocks` reflects the current eager `tuple[Pcm, ...]` audio contract; still-video tuples share one frame array, while countdown frames retain their distinct raster payloads.
- Receipt: pre-run identity hashes `SynthOp` with `_identity_policy`, so irrelevant audio, video, seed, envelope, and harmonic fields never perturb another mode. `_keyed` threads that pre-run key as the receipt slot — the `core/receipt#RECEIPT` elision law — and lands the `ContentIdentity.key(container, bytes)` product address as the `address` band fact. `_audio_band` carries only active shaping/rate/duration facts with each mode's payload, while `_framed` carries pattern geometry and policy values.
- Packages: `numpy` owns oscillator, noise FFT, interpolation, and test-pattern kernels; `_encode_audio`, `_encode_video`, and `_mux_av` own egress.
- Growth: a harmonic waveform is one `Waveform` and `_HARMONICS` row; a noise color is one `NoiseColor` and `_TINT` row; a distinct payload modality is one `SynthOp` case with admission, kernel, and evidence arms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from math import ceil, isfinite
from typing import TYPE_CHECKING, Literal, assert_never

import msgspec
import numpy as np
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.media.audio import Master, Pcm, _encode_audio  # float32 blocks admit through the audio _INGEST["flt"] row
from rasm.artifacts.media.container import ContainerFormat, MediaFault, MediaProfile, Produced, _CANON, _lapsed, _worker

lazy from rasm.artifacts.media.container import _encode_video, _mux_av

if TYPE_CHECKING:
    from numpy.typing import NDArray

    from rasm.artifacts.media.container import Frames

# --- [TYPES] ----------------------------------------------------------------------------

type Partials = tuple[tuple[float, float], ...]  # (hz, amplitude) partial set for Additive
type SynthTag = Literal[
    "oscillator", "pulse", "wavetable", "noise", "additive", "fm", "am", "sweep", "impulse",
    "bars", "ramp", "grid", "countdown", "checker", "pluge", "zone_plate",
]

# --- [CONSTANTS] ------------------------------------------------------------------------

_BLOCK: int = 4096  # per-frame chunk; the audio encoder contract takes the eager tuple these slices fill
_TONE_LEVEL: float = 0.1  # sync-tone amplitude, ~-20 dBFS
_GRID_PITCH: int = 64


class Waveform(StrEnum):
    SINE = "sine"
    SAW = "saw"
    SQUARE = "square"
    TRIANGLE = "triangle"


class NoiseColor(StrEnum):
    WHITE = "white"
    PINK = "pink"
    BROWN = "brown"
    BLUE = "blue"
    VIOLET = "violet"


# per-waveform recipe: (harmonic-index step, amplitude law, alternating sign); each sums to the Nyquist-safe count.
type Harmonic = tuple[int, Literal["inverse", "inverse_square"], bool]
_HARMONICS: frozendict[Waveform, Harmonic] = frozendict({
    Waveform.SINE: (1, "inverse", False),  # fundamental only
    Waveform.SAW: (1, "inverse", False),  # every harmonic, 1/k
    Waveform.SQUARE: (2, "inverse", False),  # odd harmonics, 1/k
    Waveform.TRIANGLE: (2, "inverse_square", True),  # odd harmonics, (-1)^m/k^2
})

# spectral exponent per noise color: rfft magnitude scaled by k^exponent — ONE shaping body, five rows.
_TINT: frozendict[NoiseColor, float] = frozendict({
    NoiseColor.WHITE: 0.0,
    NoiseColor.PINK: -0.5,
    NoiseColor.BROWN: -1.0,
    NoiseColor.BLUE: 0.5,
    NoiseColor.VIOLET: 1.0,
})

# `75%` top-band palette, left to right; `_bars_frame` adds cross-color and PLUGE lower bands.
_BARS75: tuple[tuple[int, int, int], ...] = ((191, 191, 191), (191, 191, 0), (0, 191, 191), (0, 191, 0), (191, 0, 191), (191, 0, 0), (0, 0, 191))

# seven-segment digit table: normalized (x0, y0, x1, y1) boxes indexed a..g, digits as lit-segment sets.
_SEG_BOX: tuple[tuple[float, float, float, float], ...] = (
    (0.1, 0.0, 0.9, 0.12),   # a top
    (0.78, 0.06, 0.9, 0.5),  # b upper right
    (0.78, 0.5, 0.9, 0.94),  # c lower right
    (0.1, 0.88, 0.9, 1.0),   # d bottom
    (0.1, 0.5, 0.22, 0.94),  # e lower left
    (0.1, 0.06, 0.22, 0.5),  # f upper left
    (0.1, 0.44, 0.9, 0.56),  # g middle
)
_SEVEN_SEG: frozendict[str, frozenset[int]] = frozendict({
    "0": frozenset({0, 1, 2, 3, 4, 5}),
    "1": frozenset({1, 2}),
    "2": frozenset({0, 1, 6, 4, 3}),
    "3": frozenset({0, 1, 6, 2, 3}),
    "4": frozenset({5, 6, 1, 2}),
    "5": frozenset({0, 5, 6, 2, 3}),
    "6": frozenset({0, 5, 4, 3, 2, 6}),
    "7": frozenset({0, 1, 2}),
    "8": frozenset({0, 1, 2, 3, 4, 5, 6}),
    "9": frozenset({0, 1, 2, 3, 5, 6}),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Adsr(Struct, frozen=True):
    attack: float = 0.01
    decay: float = 0.1
    sustain: float = 0.7  # sustain level 0..1
    release: float = 0.2

    def gain(self, samples: int, rate: int, /) -> "NDArray[np.float64]":
        total = samples / rate
        fixed = self.attack + self.decay + self.release
        scale = min(1.0, total / fixed) if fixed else 1.0
        attack, decay, release = self.attack * scale, self.decay * scale, self.release * scale
        knots = np.array([0.0, attack, attack + decay, max(attack + decay, total - release), total])
        levels = np.array([0.0, 1.0, self.sustain, self.sustain, 0.0])
        return np.interp(np.arange(samples) / rate, knots, levels)


class SynthProfile(Struct, frozen=True):
    media: MediaProfile = MediaProfile(container=ContainerFormat.WEBM, codec="libopus", rate=48000, layout="mono", master=Master())
    video: MediaProfile = MediaProfile(container=ContainerFormat.WEBM, codec="libvpx-vp9")
    envelope: Adsr = Adsr()
    seed: int = 0
    harmonic_limit: int = 2048


@tagged_union(frozen=True)
class SynthOp:
    tag: SynthTag = tag()
    oscillator: tuple[Waveform, float, float, float] = case()  # (waveform, hz, seconds, amplitude)
    pulse: tuple[float, float, float, float] = case()  # (hz, duty 0..1, seconds, amplitude)
    wavetable: tuple[tuple[float, ...], float, float, float] = case()  # (one-cycle table, hz, seconds, amplitude)
    noise: tuple[NoiseColor, float, float] = case()  # (color, seconds, amplitude)
    additive: tuple[Partials, float] = case()  # (partials, seconds)
    fm: tuple[float, float, float, float] = case()  # (carrier_hz, ratio, index, seconds)
    am: tuple[float, float, float, float] = case()  # (carrier_hz, rate_hz, depth, seconds)
    sweep: tuple[float, float, float, bool] = case()  # (low_hz, high_hz, seconds, logarithmic)
    impulse: float = case()  # seconds
    bars: tuple[float, tuple[int, int], float] = case()  # (seconds, (width, height), tone_hz; 0 = silent bars)
    ramp: tuple[float, tuple[int, int]] = case()  # grayscale calibration ramp
    grid: tuple[float, tuple[int, int]] = case()  # alignment grid
    countdown: tuple[float, tuple[int, int]] = case()  # leader: remaining-second number over a radial sweep
    checker: tuple[float, tuple[int, int], int] = case()  # (seconds, size, cells per short axis)
    pluge: tuple[float, tuple[int, int]] = case()  # black-level calibration pattern
    zone_plate: tuple[float, tuple[int, int], float] = case()  # (seconds, size, radial cycles)

    @staticmethod
    def Oscillator(waveform: Waveform, hz: float, seconds: float, amplitude: float = 0.8, /) -> "SynthOp":
        return SynthOp(oscillator=(waveform, hz, seconds, amplitude))

    @staticmethod
    def Pulse(hz: float, duty: float, seconds: float, amplitude: float = 0.8, /) -> "SynthOp":
        return SynthOp(pulse=(hz, duty, seconds, amplitude))

    @staticmethod
    def Wavetable(table: tuple[float, ...], hz: float, seconds: float, amplitude: float = 0.8, /) -> "SynthOp":
        return SynthOp(wavetable=(table, hz, seconds, amplitude))

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

    @staticmethod
    def Bars(seconds: float, size: tuple[int, int] = (1280, 720), tone_hz: float = 1000.0, /) -> "SynthOp":
        return SynthOp(bars=(seconds, size, tone_hz))

    @staticmethod
    def Ramp(seconds: float, size: tuple[int, int] = (1280, 720), /) -> "SynthOp":
        return SynthOp(ramp=(seconds, size))

    @staticmethod
    def Grid(seconds: float, size: tuple[int, int] = (1280, 720), /) -> "SynthOp":
        return SynthOp(grid=(seconds, size))

    @staticmethod
    def Countdown(seconds: float = 8.0, size: tuple[int, int] = (1280, 720), /) -> "SynthOp":
        return SynthOp(countdown=(seconds, size))

    @staticmethod
    def Checker(seconds: float, size: tuple[int, int] = (1280, 720), cells: int = 16, /) -> "SynthOp":
        return SynthOp(checker=(seconds, size, cells))

    @staticmethod
    def Pluge(seconds: float, size: tuple[int, int] = (1280, 720), /) -> "SynthOp":
        return SynthOp(pluge=(seconds, size))

    @staticmethod
    def ZonePlate(seconds: float, size: tuple[int, int] = (1280, 720), cycles: float = 32.0, /) -> "SynthOp":
        return SynthOp(zone_plate=(seconds, size, cycles))

    @property
    def domain(self) -> Literal["audio", "video"]:
        return "video" if self.tag in ("bars", "ramp", "grid", "countdown", "checker", "pluge", "zone_plate") else "audio"

    @property
    def fundamental(self) -> float:
        match self:
            case SynthOp(tag="oscillator", oscillator=(_, hz, *_)) | SynthOp(tag="pulse", pulse=(hz, *_)) | SynthOp(
                tag="wavetable", wavetable=(_, hz, *_)
            ) | SynthOp(tag="fm", fm=(hz, *_)) | SynthOp(tag="am", am=(hz, *_)):
                return hz
            case SynthOp(tag="sweep", sweep=(low, *_)):
                return low
            case SynthOp(tag="additive", additive=(partials, _)):
                return min((hz for hz, _ in partials), default=0.0)
            case SynthOp(tag="bars", bars=(_, _, tone_hz)):
                return tone_hz
            case SynthOp(tag="noise") | SynthOp(tag="impulse") | SynthOp(tag="ramp") | SynthOp(tag="grid") | SynthOp(
                tag="countdown"
            ) | SynthOp(tag="checker") | SynthOp(tag="pluge") | SynthOp(tag="zone_plate"):
                return 0.0
            case _ as unreachable:
                assert_never(unreachable)


class Synthesis(Struct, frozen=True):
    op: SynthOp
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    profile: SynthProfile = SynthProfile()

    @staticmethod
    def of(op: SynthOp, profile: SynthProfile = SynthProfile(), /, *, lane: LanePolicy) -> "Synthesis":
        return Synthesis(op=op, lane=lane, profile=profile)

    def emit(self, /) -> ArtifactWork:
        # a synthesis generates its source, so the node is a DAG root with no parents.
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"media.synthesis-{self.op.tag}", _CANON.encode((self.op, _identity_policy(self.op, self.profile))))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # member MediaFault folds into the boundary fault (Work[ArtifactReceipt] forbids an inner Result).
        railed = await async_boundary(f"media.synthesis.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map_error(lambda fault: BoundaryFault(boundary=(f"media.synthesis.{self.op.tag}", f"{fault.tag}:{fault}")))
        )

    async def _folded(self, /) -> Result[ArtifactReceipt, MediaFault]:
        # a segmented egress row writes a manifest and segment set through the container sink — a worker-death replay must
        # never repeat it — so idempotency derives from the profile's own media rows, never a per-call convention.
        replayable = not any(row.container.segmented and row.segment is not None for row in (self.profile.media, self.profile.video))
        railed = await self.lane.offload(Kernel.of(_synthesize, KernelTrait.HOSTILE, idempotent=replayable), self.op, self.profile)
        return railed.map_error(_lapsed).bind(lambda inner: inner).map(self._keyed)

    def _keyed(self, produced: Produced, /) -> ArtifactReceipt:
        # receipt.slot threads the PRE-RUN node key (the core/receipt elision law); the product address rides the band.
        blob, evidence = produced
        address = ContentIdentity.key(evidence.container.value, blob)
        return ArtifactReceipt.Media(
            self._key,
            evidence.container.value,
            evidence.codec,
            evidence.duration,
            evidence.byte_count,
            evidence.frame_count,
            evidence.bit_rate,
            evidence.facts | {"address": address.hex},
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _identity_policy(op: SynthOp, profile: SynthProfile, /) -> tuple[object, ...]:
    match op:
        case SynthOp(tag="oscillator") | SynthOp(tag="pulse"):
            return profile.media, profile.envelope, profile.harmonic_limit
        case SynthOp(tag="noise"):
            return profile.media, profile.envelope, profile.seed
        case SynthOp(tag="wavetable") | SynthOp(tag="additive") | SynthOp(tag="fm") | SynthOp(tag="am") | SynthOp(tag="sweep"):
            return profile.media, profile.envelope
        case SynthOp(tag="impulse"):
            return (profile.media,)
        case SynthOp(tag="bars", bars=(*_, tone_hz)) if tone_hz > 0.0:
            return profile.video, profile.media
        case SynthOp(tag="bars") | SynthOp(tag="ramp") | SynthOp(tag="grid") | SynthOp(tag="countdown") | SynthOp(
            tag="checker"
        ) | SynthOp(tag="pluge") | SynthOp(tag="zone_plate"):
            return (profile.video,)
        case _ as unreachable:
            assert_never(unreachable)


def _admitted(op: SynthOp, profile: SynthProfile, /) -> Result[SynthOp, MediaFault]:
    rate, video_rate = profile.media.rate, profile.video.rate
    nyquist = rate / 2.0
    envelope = profile.envelope
    envelope_valid = all(isfinite(value) for value in (envelope.attack, envelope.decay, envelope.sustain, envelope.release)) and envelope.attack >= 0.0 and envelope.decay >= 0.0 and envelope.release >= 0.0 and 0.0 <= envelope.sustain <= 1.0
    match op:
        case SynthOp(tag="oscillator") | SynthOp(tag="pulse"):
            profile_checks = ((rate > 0, "audio rate must be positive"), (envelope_valid, "ADSR policy is invalid"), (profile.harmonic_limit > 0, "harmonic limit must be positive"))
        case SynthOp(tag="wavetable") | SynthOp(tag="noise") | SynthOp(tag="additive") | SynthOp(tag="fm") | SynthOp(
            tag="am"
        ) | SynthOp(tag="sweep"):
            profile_checks = ((rate > 0, "audio rate must be positive"), (envelope_valid, "ADSR policy is invalid"))
        case SynthOp(tag="impulse"):
            profile_checks = ((rate > 0, "audio rate must be positive"),)
        case SynthOp(tag="bars", bars=(*_, tone_hz)) if tone_hz > 0.0:
            profile_checks = ((rate > 0, "audio rate must be positive"), (video_rate > 0, "video rate must be positive"))
        case SynthOp(tag="bars") | SynthOp(tag="ramp") | SynthOp(tag="grid") | SynthOp(tag="countdown") | SynthOp(
            tag="checker"
        ) | SynthOp(tag="pluge") | SynthOp(tag="zone_plate"):
            profile_checks = ((video_rate > 0, "video rate must be positive"),)
        case _ as unreachable:
            assert_never(unreachable)
    profile_reason = next((message for valid, message in profile_checks if not valid), None)
    match op:
        case SynthOp(tag="oscillator", oscillator=(_, hz, seconds, amplitude)):
            reason = None if all(map(isfinite, (hz, seconds, amplitude))) and 0.0 < hz < nyquist and seconds * rate >= 1.0 and 0.0 <= amplitude <= 1.0 else "frequency, duration, or amplitude outside the audio domain"
        case SynthOp(tag="pulse", pulse=(hz, duty, seconds, amplitude)):
            reason = None if all(map(isfinite, (hz, duty, seconds, amplitude))) and 0.0 < hz < nyquist and 0.0 < duty < 1.0 and seconds * rate >= 1.0 and 0.0 <= amplitude <= 1.0 else "pulse frequency, duty, duration, or amplitude outside the audio domain"
        case SynthOp(tag="wavetable", wavetable=(table, hz, seconds, amplitude)):
            reason = None if all(map(isfinite, (hz, seconds, amplitude))) and 0.0 < hz < nyquist and seconds * rate >= 1.0 and 0.0 <= amplitude <= 1.0 and len(table) >= 2 and all(isfinite(value) and -1.0 <= value <= 1.0 for value in table) else "wavetable samples, frequency, duration, or amplitude outside the audio domain"
        case SynthOp(tag="noise", noise=(_, seconds, amplitude)):
            reason = None if all(map(isfinite, (seconds, amplitude))) and seconds * rate >= 1.0 and 0.0 <= amplitude <= 1.0 else "duration or amplitude outside the audio domain"
        case SynthOp(tag="additive", additive=(partials, seconds)):
            reason = None if isfinite(seconds) and seconds * rate >= 1.0 and partials and all(isfinite(hz) and isfinite(amp) and 0.0 < hz < nyquist and amp >= 0.0 for hz, amp in partials) else "additive partials or duration outside the audio domain"
        case SynthOp(tag="fm", fm=(carrier, ratio, index, seconds)):
            deviation = carrier * ratio * index
            reason = None if all(map(isfinite, (carrier, ratio, index, seconds))) and 0.0 < carrier - deviation and carrier + deviation < nyquist and ratio > 0.0 and index >= 0.0 and seconds * rate >= 1.0 else "FM carrier, deviation, or duration outside the audio domain"
        case SynthOp(tag="am", am=(carrier, rate_hz, depth, seconds)):
            reason = None if all(map(isfinite, (carrier, rate_hz, depth, seconds))) and 0.0 < carrier < nyquist and 0.0 < rate_hz < nyquist and 0.0 <= depth <= 1.0 and seconds * rate >= 1.0 else "AM carrier, rate, depth, or duration outside the audio domain"
        case SynthOp(tag="sweep", sweep=(low, high, seconds, _)):
            reason = None if all(map(isfinite, (low, high, seconds))) and 0.0 < low <= high < nyquist and seconds * rate >= 1.0 else "sweep endpoints or duration outside the audio domain"
        case SynthOp(tag="impulse", impulse=seconds):
            reason = None if isfinite(seconds) and seconds * rate >= 1.0 else "impulse duration outside the audio domain"
        case SynthOp(tag="bars", bars=(seconds, (width, height), tone_hz)):
            reason = None if isfinite(seconds) and isfinite(tone_hz) and seconds * video_rate >= 1.0 and width > 0 and height > 0 and (tone_hz == 0.0 or 0.0 < tone_hz < nyquist) and (tone_hz == 0.0 or profile.media.container == profile.video.container) else "bars timing, size, tone, or mux container mismatch"
        case SynthOp(tag="ramp", ramp=(seconds, (width, height))) | SynthOp(tag="grid", grid=(seconds, (width, height))) | SynthOp(
            tag="pluge", pluge=(seconds, (width, height))
        ):
            reason = None if isfinite(seconds) and seconds * video_rate >= 1.0 and width > 0 and height > 0 else "test-pattern timing or size outside the video domain"
        case SynthOp(tag="countdown", countdown=(seconds, (width, height))):
            reason = None if isfinite(seconds) and seconds >= 1.0 and width >= 4 and height >= 2 else "countdown requires at least one second and a drawable frame"
        case SynthOp(tag="checker", checker=(seconds, (width, height), cells)):
            reason = None if isfinite(seconds) and seconds * video_rate >= 1.0 and width > 0 and height > 0 and 1 <= cells <= min(width, height) else "checker timing, size, or cell count outside the video domain"
        case SynthOp(tag="zone_plate", zone_plate=(seconds, (width, height), cycles)):
            reason = None if all(map(isfinite, (seconds, cycles))) and seconds * video_rate >= 1.0 and width > 0 and height > 0 and cycles > 0.0 else "zone-plate timing, size, or cycles outside the video domain"
        case _ as unreachable:
            assert_never(unreachable)
    failure = profile_reason or reason
    return Ok(op) if failure is None else Error(MediaFault(invalid=f"synthesis.{op.tag}: {failure}"))


def _phase(hz: "NDArray[np.float64] | float", samples: int, rate: int, /) -> "NDArray[np.float64]":
    increment = 2.0 * np.pi * np.asarray(hz, np.float64) / rate
    track = np.broadcast_to(increment, (samples,))
    return np.cumsum(track) - track


def _oscillator(waveform: Waveform, hz: float, samples: int, rate: int, harmonic_limit: int, /) -> "NDArray[np.float64]":
    phase = _phase(hz, samples, rate)
    step, law, alternate = _HARMONICS[waveform]
    ceiling = 1 if waveform is Waveform.SINE else min(harmonic_limit, max(1, int((rate / 2) / hz)))
    orders = range(1, ceiling + 1, step)
    weight = (lambda k: 1.0 / k) if law == "inverse" else (lambda k: 1.0 / (k * k))
    sign = (lambda m: (-1.0) ** m) if alternate else (lambda _m: 1.0)
    signal = sum((sign(m) * weight(k) * np.sin(k * phase) for m, k in enumerate(orders)), start=np.zeros(samples))
    return signal / (np.abs(signal).max() + 1e-12)


def _pulse(hz: float, duty: float, samples: int, rate: int, harmonic_limit: int, /) -> "NDArray[np.float64]":
    phase = _phase(hz, samples, rate)
    ceiling = min(harmonic_limit, max(1, int((rate / 2) / hz)))
    signal = 2.0 * duty - 1.0 + sum(
        (4.0 * np.sin(np.pi * order * duty) * np.cos(order * (phase - np.pi * duty)) / (np.pi * order) for order in range(1, ceiling + 1)),
        start=np.zeros(samples),
    )
    return signal / (np.abs(signal).max() + 1e-12)


def _wavetable(table: tuple[float, ...], hz: float, samples: int, rate: int, /) -> "NDArray[np.float64]":
    position = ((_phase(hz, samples, rate) / (2.0 * np.pi)) % 1.0) * len(table)
    values = np.array((*table, table[0]), np.float64)
    return np.interp(position, np.arange(values.size), values)


def _noise(color: NoiseColor, samples: int, seed: int, /) -> "NDArray[np.float64]":
    white = np.random.default_rng(seed).standard_normal(samples)
    bins = np.arange(samples // 2 + 1, dtype=np.float64)
    weights = np.where(bins == 0.0, 0.0, np.power(np.maximum(bins, 1.0), _TINT[color]))
    spectrum = np.fft.rfft(white) * weights
    tinted = np.fft.irfft(spectrum, n=samples)
    return tinted / (np.abs(tinted).max() + 1e-9)


def _generated(op: SynthOp, rate: int, seed: int, harmonic_limit: int, /) -> "NDArray[np.float64]":
    match op:
        case SynthOp(tag="oscillator", oscillator=(waveform, hz, seconds, amplitude)):
            return amplitude * _oscillator(waveform, hz, int(seconds * rate), rate, harmonic_limit)
        case SynthOp(tag="pulse", pulse=(hz, duty, seconds, amplitude)):
            return amplitude * _pulse(hz, duty, int(seconds * rate), rate, harmonic_limit)
        case SynthOp(tag="wavetable", wavetable=(table, hz, seconds, amplitude)):
            return amplitude * _wavetable(table, hz, int(seconds * rate), rate)
        case SynthOp(tag="noise", noise=(color, seconds, amplitude)):
            return amplitude * _noise(color, int(seconds * rate), seed)
        case SynthOp(tag="additive", additive=(partials, seconds)):
            samples = int(seconds * rate)
            signal = sum((amp * np.sin(_phase(hz, samples, rate)) for hz, amp in partials), start=np.zeros(samples))
            return signal / max(1.0, sum(amplitude for _, amplitude in partials))
        case SynthOp(tag="fm", fm=(carrier_hz, ratio, index, seconds)):
            samples = int(seconds * rate)
            return np.sin(_phase(carrier_hz, samples, rate) + index * np.sin(_phase(carrier_hz * ratio, samples, rate)))
        case SynthOp(tag="am", am=(carrier_hz, rate_hz, depth, seconds)):
            samples = int(seconds * rate)
            return ((1.0 + depth * np.sin(_phase(rate_hz, samples, rate))) / (1.0 + depth)) * np.sin(_phase(carrier_hz, samples, rate))
        case SynthOp(tag="sweep", sweep=(low_hz, high_hz, seconds, logarithmic)):
            samples = int(seconds * rate)
            track = np.geomspace(low_hz, high_hz, samples) if logarithmic else np.linspace(low_hz, high_hz, samples)
            return np.sin(_phase(track, samples, rate))
        case SynthOp(tag="impulse", impulse=seconds):
            buffer = np.zeros(int(seconds * rate))
            buffer[0] = 1.0
            return buffer
        case _ as unreachable:
            assert_never(unreachable)


def _shaped(op: SynthOp, buffer: "NDArray[np.float64]", envelope: Adsr, rate: int, /) -> "NDArray[np.float64]":
    gain = np.ones(buffer.size) if op.tag == "impulse" else envelope.gain(buffer.size, rate)
    return np.clip(buffer * gain, -1.0, 1.0)


def _blocks(buffer: "NDArray[np.float64]", /) -> tuple[Pcm, ...]:
    # chunk the mono float32 buffer into (1, _BLOCK) frames; the audio _encode_audio contract takes this eager tuple.
    packed = buffer.astype(np.float32).reshape(1, -1)
    return tuple(packed[:, start : start + _BLOCK] for start in range(0, packed.shape[1], _BLOCK))


def _bars_frame(size: tuple[int, int], /) -> "NDArray[np.uint8]":
    w, h = size
    top = np.array(_BARS75, np.uint8)[np.arange(w) * len(_BARS75) // w]
    middle_palette = np.array(((0, 0, 191), (0, 0, 0), (191, 0, 191), (0, 0, 0), (0, 191, 191), (0, 0, 0), (191, 191, 191)), np.uint8)
    bottom_palette = np.array(((0, 33, 76), (255, 255, 255), (50, 0, 106), (0, 0, 0), (9, 9, 9), (18, 18, 18), (0, 0, 0)), np.uint8)
    middle = middle_palette[np.arange(w) * len(middle_palette) // w]
    bottom = bottom_palette[np.arange(w) * len(bottom_palette) // w]
    split, pluge = 2 * h // 3, 3 * h // 4
    return np.concatenate((np.broadcast_to(top, (split, w, 3)), np.broadcast_to(middle, (pluge - split, w, 3)), np.broadcast_to(bottom, (h - pluge, w, 3))), axis=0).copy()


def _ramp_frame(size: tuple[int, int], /) -> "NDArray[np.uint8]":
    w, h = size
    gray = np.linspace(0.0, 255.0, w).astype(np.uint8)
    return np.broadcast_to(np.stack([gray] * 3, axis=-1), (h, w, 3)).copy()


def _grid_frame(size: tuple[int, int], /) -> "NDArray[np.uint8]":
    w, h = size
    lined = ((np.arange(w)[None, :] % _GRID_PITCH < 2) | (np.arange(h)[:, None] % _GRID_PITCH < 2)).astype(np.uint8)
    return np.stack([lined * 255 + (1 - lined) * 64] * 3, axis=-1).astype(np.uint8)


def _checker_frame(size: tuple[int, int], cells: int, /) -> "NDArray[np.uint8]":
    w, h = size
    short = min(w, h)
    pitch = short / cells
    checked = ((np.arange(w)[None, :] // pitch + np.arange(h)[:, None] // pitch) % 2).astype(np.uint8) * 255
    return np.stack((checked, checked, checked), axis=-1)


def _pluge_frame(size: tuple[int, int], /) -> "NDArray[np.uint8]":
    w, h = size
    levels = np.array((0, 8, 16, 24, 235), np.uint8)
    columns = levels[np.arange(w) * levels.size // w]
    gray = np.broadcast_to(columns, (h, w))
    return np.stack((gray, gray, gray), axis=-1).copy()


def _zone_plate_frame(size: tuple[int, int], cycles: float, /) -> "NDArray[np.uint8]":
    w, h = size
    x = np.linspace(-1.0, 1.0, w)[None, :]
    y = np.linspace(-1.0, 1.0, h)[:, None]
    gray = np.clip(127.5 * (1.0 + np.sin(np.pi * cycles * (x * x + y * y))), 0.0, 255.0).astype(np.uint8)
    return np.stack((gray, gray, gray), axis=-1)


def _digit_mask(char: str, size: tuple[int, int], /) -> "NDArray[np.bool_]":
    w, h = size
    x = np.arange(w)[None, :] / w
    y = np.arange(h)[:, None] / h
    boxes = (_SEG_BOX[seg] for seg in _SEVEN_SEG[char])
    return np.any(np.stack([(x0 <= x) & (x < x1) & (y0 <= y) & (y < y1) for x0, y0, x1, y1 in boxes]), axis=0)


def _countdown_frames(seconds: float, size: tuple[int, int], rate: int, /) -> "Frames":
    # Remaining seconds over a radial sweep wedge, one frame per tick.
    w, h = size
    cx, cy = w / 2, h / 2
    theta = np.arctan2(np.arange(h)[:, None] - cy, np.arange(w)[None, :] - cx) + np.pi

    def painted(index: int, /) -> "NDArray[np.uint8]":
        frame = np.full((h, w, 3), 32, np.uint8)
        frame[theta < ((index % rate) / rate) * 2.0 * np.pi] = 96
        digits = str(max(0, ceil(seconds - index / rate)))
        dw, dh = max(w // (4 * len(digits)), 1), h // 2
        # _countdown_frames clamps the digit run to the frame width: dw floors at 1 and the [:, :w] slice caps mw at w,
        # so a narrow frame under 4*len(digits) columns never mints a negative slice start wrapping paint onto the wrong edge.
        mask = np.concatenate(tuple(_digit_mask(digit, (dw, dh)) for digit in digits), axis=1)[:, :w]
        mw = mask.shape[1]
        frame[(h - dh) // 2 : (h + dh) // 2, (w - mw) // 2 : (w + mw) // 2][mask] = 255
        return frame

    return tuple(painted(index) for index in range(int(seconds * rate)))


def _framed(op: SynthOp, rate: int, /) -> tuple["Frames", frozendict[str, float | str]]:
    # video test-signal painters: one still tiled per tick, or the per-tick countdown paint.
    match op:
        case SynthOp(tag="bars", bars=(seconds, size, _)):
            frames, kind, parameters = (_bars_frame(size),) * int(seconds * rate), "bars", frozendict()
        case SynthOp(tag="ramp", ramp=(seconds, size)):
            frames, kind, parameters = (_ramp_frame(size),) * int(seconds * rate), "ramp", frozendict()
        case SynthOp(tag="grid", grid=(seconds, size)):
            frames, kind, parameters = (_grid_frame(size),) * int(seconds * rate), "grid", frozendict({"pitch": float(_GRID_PITCH)})
        case SynthOp(tag="countdown", countdown=(seconds, size)):
            frames, kind, parameters = _countdown_frames(seconds, size, rate), "countdown", frozendict()
        case SynthOp(tag="checker", checker=(seconds, size, cells)):
            frames, kind, parameters = (_checker_frame(size, cells),) * int(seconds * rate), "checker", frozendict({"cells": float(cells)})
        case SynthOp(tag="pluge", pluge=(seconds, size)):
            frames, kind, parameters = (_pluge_frame(size),) * int(seconds * rate), "pluge", frozendict()
        case SynthOp(tag="zone_plate", zone_plate=(seconds, size, cycles)):
            frames, kind, parameters = (_zone_plate_frame(size, cycles),) * int(seconds * rate), "zone_plate", frozendict({"cycles": cycles})
        case _ as unreachable:
            assert_never(unreachable)
    band = frozendict({"pattern": kind, "duration": len(frames) / rate, "width": float(frames[0].shape[1]), "height": float(frames[0].shape[0])}) | parameters
    return frames, band


@_worker
def _synthesize(op: SynthOp, profile: SynthProfile, /) -> Result[Produced, MediaFault]:
    return _admitted(op, profile).bind(lambda admitted: _encoded(admitted, profile))


def _encoded(op: SynthOp, profile: SynthProfile, /) -> Result[Produced, MediaFault]:
    match op.domain:
        case "audio":
            rate = profile.media.rate
            return _encode_audio(_blocks(_shaped(op, _generated(op, rate, profile.seed, profile.harmonic_limit), profile.envelope, rate)), profile.media).map(
                lambda pair: _banded(pair, _audio_band(op, profile))
            )
        case "video":
            return _screened(op, profile)
        case _ as unreachable:
            assert_never(unreachable)


def _screened(op: SynthOp, profile: SynthProfile, /) -> Result[Produced, MediaFault]:
    # a tone-carrying bars case muxes frames + sync tone (_mux_av); every other pattern encodes video-only.
    frames, band = _framed(op, profile.video.rate)
    match op:
        case SynthOp(tag="bars", bars=(seconds, _, tone_hz)) if tone_hz > 0.0:
            arate = profile.media.rate
            tone = _blocks(_TONE_LEVEL * np.sin(_phase(tone_hz, int(seconds * arate), arate)))
            railed = _mux_av(frames, tone, profile.video, profile.media)
            return railed.map(
                lambda pair: _banded(pair, band | {"tone_hz": tone_hz, "tone_level": _TONE_LEVEL, "tone_rate": float(arate)})
            )
        case _:
            return _encode_video(frames, profile.video).map(lambda pair: _banded(pair, band))


def _seconds(op: SynthOp, /) -> float:
    match op:
        case SynthOp(tag="oscillator", oscillator=(_, _, seconds, _)) | SynthOp(tag="pulse", pulse=(_, _, seconds, _)) | SynthOp(
            tag="wavetable", wavetable=(_, _, seconds, _)
        ) | SynthOp(tag="noise", noise=(_, seconds, _)):
            return seconds
        case SynthOp(tag="additive", additive=(_, seconds)) | SynthOp(tag="impulse", impulse=seconds):
            return seconds
        case SynthOp(tag="fm", fm=(*_, seconds)) | SynthOp(tag="am", am=(*_, seconds)) | SynthOp(tag="sweep", sweep=(_, _, seconds, _)):
            return seconds
        case _:
            return 0.0


def _audio_band(op: SynthOp, profile: SynthProfile, /) -> frozendict[str, float | str]:
    match op:
        case SynthOp(tag="oscillator", oscillator=(waveform, hz, _, amplitude)):
            specific = frozendict({"waveform": waveform.value, "frequency_hz": hz, "amplitude": amplitude})
        case SynthOp(tag="pulse", pulse=(hz, duty, _, amplitude)):
            specific = frozendict({"frequency_hz": hz, "duty": duty, "amplitude": amplitude})
        case SynthOp(tag="wavetable", wavetable=(table, hz, _, amplitude)):
            specific = frozendict({"frequency_hz": hz, "table_samples": float(len(table)), "amplitude": amplitude})
        case SynthOp(tag="noise", noise=(color, _, amplitude)):
            specific = frozendict({"color": color.value, "amplitude": amplitude, "seed": float(profile.seed)})
        case SynthOp(tag="additive", additive=(partials, _)):
            specific = frozendict({"partials": float(len(partials)), "partial_gain": float(sum(amplitude for _, amplitude in partials))})
        case SynthOp(tag="fm", fm=(carrier, ratio, index, _)):
            specific = frozendict({"carrier_hz": carrier, "ratio": ratio, "index": index})
        case SynthOp(tag="am", am=(carrier, rate_hz, depth, _)):
            specific = frozendict({"carrier_hz": carrier, "modulation_hz": rate_hz, "depth": depth})
        case SynthOp(tag="sweep", sweep=(low, high, _, logarithmic)):
            specific = frozendict({"low_hz": low, "high_hz": high, "track": "logarithmic" if logarithmic else "linear"})
        case SynthOp(tag="impulse"):
            specific = frozendict({"unit_impulse": "true"})
        case _ as unreachable:
            assert_never(unreachable)
    envelope = profile.envelope
    common = frozendict({
        "generator": op.tag,
        "duration": _seconds(op),
        "sample_rate": float(profile.media.rate),
        "fundamental_hz": op.fundamental,
    })
    fixed = envelope.attack + envelope.decay + envelope.release
    scale = min(1.0, _seconds(op) / fixed) if fixed else 1.0
    shaping = (
        frozendict()
        if op.tag == "impulse"
        else frozendict({"attack": envelope.attack, "decay": envelope.decay, "sustain": envelope.sustain, "release": envelope.release, "envelope_scale": scale})
    )
    harmonics = frozendict({"harmonic_limit": float(profile.harmonic_limit)}) if op.tag in ("oscillator", "pulse") else frozendict()
    return common | shaping | harmonics | specific


def _banded(pair: Produced, band: frozendict[str, float | str], /) -> Produced:
    # fold the mode's generation parameters onto the spine's evidence band; the owner mints the receipt.
    blob, evidence = pair
    return blob, msgspec.structs.replace(evidence, facts=evidence.facts | band)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
