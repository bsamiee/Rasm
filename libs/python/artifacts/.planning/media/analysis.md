# [PY_ARTIFACTS_MEDIA_ANALYSIS]

`Analysis` is the read-side media leaf: one closed `AnalysisOp` union produces waveform/spectrogram images, EBU R128 loudness, silence and black-frame spans, scene cuts, representative thumbnails, and a generated `AudioMetric` measurement set, never a parallel function family. `AnalysisArm.of` selects a native FFmpeg filter only when the non-empty `_NATIVE` requirement is present in `media_filters()`; an empty row declares a substitute-only operation. Every worker admits finite, bounded knobs through `_admitted`, preserves `_decode_audio` on `Result`, normalizes integer PCM to full-scale float before any dB calculation, and returns one `(bytes, AnalysisEvidence)` payload.

It composes the container decode/fault surface, audio `_decode_audio`, filter capability probe, and raster `_save_array` egress. Native routes are `showwavespic`/`showspectrumpic`, `av.filter.loudnorm.stats`, `select='gt(scene,t)'`, and `thumbnail`; `Silence` and `BlackDetect` are substitute-only because PyAV does not expose the `lavfi.*` metadata those filters write. `_flag_spans` is their shared maximal-run algebra. Native loudness carries `integrated_lufs`/`true_peak`/`lra`; the NumPy fallback carries explicitly weaker `gated_loudness`/`sample_peak_dbfs`/`short_term_spread` with `k_weighted=false`, never labels an unweighted estimate as BS.1770 evidence.

## [01]-[INDEX]

- [01]-[ANALYSIS]: the `Analysis` owner over the closed `AnalysisOp` family — image, loudness, span, scene, thumbnail, and generated `AudioMetric` operations — each capability-routed native-or-substitute and folding into one `AnalysisEvidence` carrier.

## [02]-[ANALYSIS]

- Owner: `Analysis` discriminates modality over the closed `AnalysisOp` family, each case carrying its typed payload (a source `blob` plus its op knobs), never a shared erased `params` bag, a per-measurement subclass, or a parallel `waveform`/`loudness`/`scenes` trio; `AnalysisArm` is the closed `NATIVE`/`SUBSTITUTE` route vocabulary — a `StrEnum`, token identity with no payload — whose `of` derivation routes native only when the op's `_NATIVE` row is non-empty AND present in the probe, so a limited wheel routes to the substitute, a full build to the native filter with the same evidence shape, and a substitute-only row can never route native; `AnalysisEvidence` the frozen carrier this page owns (the artifact `container` tag, the route `codec`, source `duration`, `byte_count`, `count`, and the measured `facts` band) projecting onto `ArtifactReceipt.Media` at `_keyed`; `MediaFault` the container cause vocabulary threaded unchanged.
- Cases: `Waveform`/`Spectrogram` render native filter output or NumPy envelope/STFT images while both routes derive the same peak/RMS or centroid facts from normalized PCM. `Loudness` returns exact R128 facts from `loudnorm.stats` or explicitly named unweighted fallback facts. `Silence`/`BlackDetect` fold threshold flags through `_flag_spans`; `SceneDetect` uses native `select` or normalized mean-absolute frame delta; `Thumbnail` uses native `thumbnail` or variance picks and refuses an empty video before `_sheet`. `Metrics(blob, selected)` generates any subset of peak/RMS/crest/DC/zero-crossing/centroid/rolloff/bandwidth/flatness/dynamic-range facts from one `AudioMetric` vocabulary and one FFT correspondence.
- Entry: `emit()` threads `parents` into `ArtifactWork`; `_key` derives the pre-run key through `_CANON` plus `ContentIdentity.key` and `_keyed` threads it as the receipt slot with the product address on the `address` band fact; `_dispatch` offloads `_analyze`, which derives `AnalysisArm.of(op.tag, media_filters())` on the process side so `av` stays worker-scope, and maps the lane's outer `BoundaryFault` through `_lapsed` before flattening the worker `Result`. `_worker` maps `BeartypeCallHintViolation` to `MediaFault.contract` at definition time.
- Auto: the route is `AnalysisArm.of(op.tag, media_filters())`, so a producer never passes a `use_native` flag, a new native dependency is one `_NATIVE` row, and a substitute-only op is the empty row read as data; a `Waveform`/`Spectrogram`/`Thumbnail` produces a PNG and a `Loudness`/`Silence`/`BlackDetect`/`SceneDetect` a `msgspec.json` facts blob, both keyed and both carrying the measured band.
- Receipt: each analysis contributes one `ArtifactReceipt.Media` whose slot threads the PRE-RUN node key — the `core/receipt#RECEIPT` elision law — with the produced-bytes content address on the `address` band fact. Route identity rides `codec`, numeric output rides `facts`, and `AnalysisEvidence` keeps provider handles out of the receipt owner. Exact R128 facts exist only on the native route; substitute fact names expose their weaker measurement.
- Growth: a structurally distinct measurement is one `AnalysisOp` case, `_NATIVE` row, admission arm, and `_analyzed` arm; another audio scalar is one `AudioMetric` member plus one `measured` row; a native dependency is one requirement-set edit; a substitute replaces one route body behind the same `AnalysisArm` value.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable, Iterator
from enum import StrEnum
from functools import wraps
from heapq import nlargest
from itertools import groupby, islice
from math import isfinite
from operator import itemgetter
from typing import TYPE_CHECKING, Literal, assert_never

import msgspec
import numpy as np
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct
from numpy.lib.stride_tricks import sliding_window_view

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.media.container import MediaFault, _CANON, _lapsed

lazy import av
lazy import av.error
lazy import av.filter
lazy import av.filter.loudnorm  # stats(loudnorm_args, stream) -> JSON bytes, the two-pass R128 measurement
lazy from rasm.artifacts.media.audio import _decode_audio
lazy from rasm.artifacts.media.container import _media_fault
lazy from rasm.artifacts.media.filtergraph import media_filters  # av.filter.filters_available canonicalized as the capability probe
lazy from rasm.artifacts.graphic.raster.process import _save_array  # array -> PNG RasterFact; .data is the encoded bytes

if TYPE_CHECKING:
    from numpy.typing import NDArray

    from rasm.artifacts.media.audio import Pcm

# --- [TYPES] ----------------------------------------------------------------------------

type AnalysisTag = Literal["waveform", "spectrogram", "loudness", "silence", "black_detect", "scene_detect", "thumbnail", "metrics"]

# --- [CONSTANTS] ------------------------------------------------------------------------

# an op routes NATIVE only when its `_NATIVE` row is NON-EMPTY and present in `media_filters()`. The empty row is the
# substitute-only declaration: PyAV frames expose side_data but not the lavfi.* frame metadata silencedetect/blackdetect
# write, so those two ops fold the numpy span algebra as their only arm.
_NATIVE: frozendict[AnalysisTag, frozenset[str]] = frozendict({
    "waveform": frozenset({"showwavespic"}),
    "spectrogram": frozenset({"showspectrumpic"}),
    "loudness": frozenset({"loudnorm"}),
    "silence": frozenset(),
    "black_detect": frozenset(),
    "scene_detect": frozenset({"select"}),
    "thumbnail": frozenset({"thumbnail"}),
    "metrics": frozenset(),
})

_STFT_WINDOW: int = 1024
_STFT_HOP: int = 256

_MAX_EXTENT: int = 8192  # waveform/spectrogram render ceiling — a pic filter allocates width*height*4 up front
_MAX_THUMBNAILS: int = 64
_MAX_SPAN_SECONDS: float = 3600.0  # silence/black minimum-duration ceiling; a span past the hour cap is a mis-keyed knob
_MAX_FLAG_FRAMES: int = 1_000_000  # per-frame flag decode budget — a source past it is hostile or mis-keyed, refused rather than truncated


class AnalysisArm(StrEnum):
    NATIVE = "native"
    SUBSTITUTE = "substitute"

    @classmethod
    def of(cls, op_tag: AnalysisTag, filters: frozenset[str], /) -> "AnalysisArm":
        need = _NATIVE[op_tag]
        return cls.NATIVE if need and need <= filters else cls.SUBSTITUTE


class AudioMetric(StrEnum):
    PEAK_DBFS = "peak_dbfs"
    RMS_DBFS = "rms_dbfs"
    CREST_FACTOR = "crest_factor"
    DC_OFFSET = "dc_offset"
    ZERO_CROSSING = "zero_crossing_rate"
    CENTROID_HZ = "centroid_hz"
    ROLLOFF_HZ = "rolloff_85_hz"
    BANDWIDTH_HZ = "bandwidth_hz"
    FLATNESS = "spectral_flatness"
    DYNAMIC_RANGE_DB = "dynamic_range_db"


# --- [MODELS] ---------------------------------------------------------------------------


class LoudnessTarget(Struct, frozen=True):
    i: float = -24.0  # integrated-LUFS target the two-pass measurement gates against
    tp: float = -2.0  # true-peak dBTP ceiling
    lra: float = 7.0  # loudness range


@tagged_union(frozen=True)
class AnalysisOp:
    tag: AnalysisTag = tag()
    waveform: tuple[bytes, tuple[int, int]] = case()  # (container bytes, (width, height))
    spectrogram: tuple[bytes, tuple[int, int]] = case()
    loudness: tuple[bytes, LoudnessTarget] = case()
    silence: tuple[bytes, float, float] = case()  # (bytes, threshold_db, min_seconds)
    black_detect: tuple[bytes, float, float] = case()  # (bytes, luma threshold 0..1, min_seconds)
    scene_detect: tuple[bytes, float] = case()  # (bytes, scene threshold 0..1)
    thumbnail: tuple[bytes, int] = case()  # (bytes, thumbnail count)
    metrics: tuple[bytes, tuple[AudioMetric, ...]] = case()  # sorted dedup at the constructor keeps the identity bytes canonical

    @staticmethod
    def Waveform(blob: bytes, size: tuple[int, int] = (960, 240), /) -> "AnalysisOp":
        return AnalysisOp(waveform=(blob, size))

    @staticmethod
    def Spectrogram(blob: bytes, size: tuple[int, int] = (960, 480), /) -> "AnalysisOp":
        return AnalysisOp(spectrogram=(blob, size))

    @staticmethod
    def Loudness(blob: bytes, target: LoudnessTarget = LoudnessTarget(), /) -> "AnalysisOp":
        return AnalysisOp(loudness=(blob, target))

    @staticmethod
    def Silence(blob: bytes, threshold_db: float = -30.0, min_seconds: float = 0.5, /) -> "AnalysisOp":
        return AnalysisOp(silence=(blob, threshold_db, min_seconds))

    @staticmethod
    def BlackDetect(blob: bytes, luma: float = 0.1, min_seconds: float = 0.5, /) -> "AnalysisOp":
        return AnalysisOp(black_detect=(blob, luma, min_seconds))

    @staticmethod
    def SceneDetect(blob: bytes, threshold: float = 0.4, /) -> "AnalysisOp":
        return AnalysisOp(scene_detect=(blob, threshold))

    @staticmethod
    def Thumbnail(blob: bytes, count: int = 9, /) -> "AnalysisOp":
        return AnalysisOp(thumbnail=(blob, count))

    @staticmethod
    def Metrics(blob: bytes, metrics: frozenset[AudioMetric] = frozenset(AudioMetric), /) -> "AnalysisOp":
        return AnalysisOp(metrics=(blob, tuple(sorted(metrics))))


class AnalysisEvidence(Struct, frozen=True):
    container: str  # "png" (image) or "json" (measurement)
    codec: str  # "analysis-native" | "analysis-substitute"
    duration: float
    byte_count: int
    count: int
    facts: frozendict[str, float | str] = frozendict()

    @staticmethod
    def measure(
        container: str, arm: AnalysisArm, duration: float, blob: bytes, count: int, facts: frozendict[str, float | str], /
    ) -> "AnalysisEvidence":
        return AnalysisEvidence(container, f"analysis-{arm.value}", duration, len(blob), count, facts)


type AnalysisProduct = tuple[bytes, AnalysisEvidence]


class Analysis(Struct, frozen=True):
    op: AnalysisOp
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    parents: tuple[ContentKey, ...] = ()  # the upstream media producer keys the planner wires

    @staticmethod
    def of(op: AnalysisOp, parents: tuple[ContentKey, ...] = (), /, *, lane: LanePolicy) -> "Analysis":
        # admission canonicalizes the identity payload — a directly-constructed metrics case with duplicate or
        # unordered members lands on the same `_key` the sorted-dedup factory mints, one owner key per semantic request.
        match op:
            case AnalysisOp(tag="metrics", metrics=(blob, members)):
                return Analysis(op=AnalysisOp(metrics=(blob, tuple(sorted(set(members))))), lane=lane, parents=parents)
            case _:
                return Analysis(op=op, lane=lane, parents=parents)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=self.parents, admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"media.analysis-{self.op.tag}", _CANON.encode(self.op))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # member MediaFault folds into the boundary fault (Work[ArtifactReceipt] forbids an inner Result).
        railed = await async_boundary(f"media.analysis.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map_error(lambda fault: BoundaryFault(boundary=(f"media.analysis.{self.op.tag}", f"{fault.tag}:{fault}")))
        )

    async def _folded(self, /) -> Result[ArtifactReceipt, MediaFault]:
        return (await self._dispatch()).map(self._keyed)

    def _keyed(self, product: AnalysisProduct, /) -> ArtifactReceipt:
        # receipt.slot threads the PRE-RUN node key (the core/receipt elision law); the product address rides the band.
        blob, ev = product
        address = ContentIdentity.key(ev.container, blob)
        return ArtifactReceipt.Media(self._key, ev.container, ev.codec, ev.duration, ev.byte_count, ev.count, 0, ev.facts | {"address": address.hex})

    async def _dispatch(self, /) -> Result[AnalysisProduct, MediaFault]:
        # worker derives its own AnalysisArm from media_filters() on the process side, so av stays worker-scope.
        # analysis is a pure decode-and-measure over the source blob — no external write, so a worker-death
        # replay is safe by declaration and idempotent=True is truth, not convention.
        outcome = await self.lane.offload(Kernel.of(_analyze, KernelTrait.HOSTILE, idempotent=True), self.op)
        return outcome.map_error(_lapsed).bind(lambda inner: inner)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _source_seconds(reader: object, /) -> float:
    return float(reader.duration / av.time_base) if reader.duration is not None else 0.0


def _audio_image(reader: object, graph: object, /) -> "NDArray[np.uint8]":
    # audio->image native arm: push each decoded AudioFrame into a showwavespic/showspectrumpic graph, flush,
    # and pull the single rendered image VideoFrame back as an rgba array (no metadata read, the frame IS the image).
    for frame in reader.decode(audio=0):
        graph.push(frame)
    graph.push(None)
    return graph.pull().to_ndarray(format="rgba")


def _mono(decoded: "tuple[tuple[Pcm, ...], int, str]", /) -> "NDArray[np.float64]":
    # packed (1, samples*channels) blocks reshape to frames x channels off the decoded layout before the downmix,
    # so interleaved stereo never reads as double-length mono and every mono.size-derived duration counts frames.
    blocks, _rate, layout = decoded

    def normalized(block: "Pcm", /) -> "NDArray[np.float64]":
        raw = np.asarray(block)
        scale = float(max(abs(np.iinfo(raw.dtype).min), np.iinfo(raw.dtype).max)) if np.issubdtype(raw.dtype, np.integer) else 1.0
        return raw.astype(np.float64) / scale

    channels = av.AudioLayout(layout).nb_channels
    # blocks are non-empty by _decode_audio's admission — an empty decode rails invalid at the seam, never a synthetic sample here
    return np.concatenate([normalized(block).reshape(-1, channels).mean(axis=1) for block in blocks])


def _envelope(mono: "NDArray[np.float64]", window: int, /) -> tuple["NDArray[np.float64]", "NDArray[np.float64]"]:
    width = max(window, 1)
    starts = np.arange(0, mono.size, width)
    counts = np.minimum(width, mono.size - starts)
    peak = np.maximum.reduceat(np.abs(mono), starts)
    rms = np.sqrt(np.add.reduceat(mono * mono, starts) / counts)
    return peak, rms


def _stft(mono: "NDArray[np.float64]", /) -> "NDArray[np.float64]":
    # framed rfft over hanning-windowed hops -> log-magnitude grid; compute/analysis/signal#DSP is the richer scipy path.
    win = np.hanning(_STFT_WINDOW)
    hops = tuple(range(0, max(mono.size - _STFT_WINDOW, 0) + 1, _STFT_HOP))
    grid = np.stack([np.abs(np.fft.rfft(win * mono[h : h + _STFT_WINDOW])) for h in hops], axis=1) if hops else np.zeros((_STFT_WINDOW // 2 + 1, 1))
    return 20.0 * np.log10(grid + 1e-9)


def _block_power(mono: "NDArray[np.float64]", rate: int, seconds: float, hop: float, /) -> "NDArray[np.float64]":
    # mean-square power per gating block: sliding windows of `seconds` advanced by `hop`, strided view with no copy.
    width, step = max(int(seconds * rate), 1), max(int(hop * rate), 1)
    return np.array([float((mono * mono).mean())]) if mono.size < width else (sliding_window_view(mono, width)[::step] ** 2).mean(axis=-1)


def _gated_loudness(mono: "NDArray[np.float64]", rate: int, /) -> tuple[float, float]:
    # Absolute and relative block gates produce an explicitly unweighted fallback, never R128 evidence.
    lk = -0.691 + 10.0 * np.log10(_block_power(mono, rate, 0.4, 0.1) + 1e-12)
    absolute = lk > -70.0
    relative = (-0.691 + 10.0 * np.log10(np.power(10.0, (lk[absolute] + 0.691) / 10.0).mean() + 1e-12) - 10.0) if absolute.any() else -70.0
    gated = lk[absolute & (lk > relative)]
    integrated = float(-0.691 + 10.0 * np.log10(np.power(10.0, (gated + 0.691) / 10.0).mean() + 1e-12)) if gated.size else -70.0
    st = -0.691 + 10.0 * np.log10(_block_power(mono, rate, 3.0, 1.0) + 1e-12)
    ranged = st[(st > -70.0) & (st > integrated - 20.0)]
    lra = float(np.percentile(ranged, 95) - np.percentile(ranged, 10)) if ranged.size else 0.0
    return integrated, lra


def _flag_spans(flags: "NDArray[np.bool_]", step: float, min_seconds: float, /) -> tuple[tuple[float, float], ...]:
    # ONE run algebra silence and black-detect share: maximal True runs longer than min_seconds become spans.
    runs = ((run[0][0], run[-1][0] + 1) for flag, group in groupby(enumerate(flags.tolist()), key=itemgetter(1)) if flag and (run := list(group)))
    return tuple((lo * step, hi * step) for lo, hi in runs if (hi - lo) * step >= min_seconds)


def _pull_frames(graph: object, /) -> Iterator[object]:
    while True:  # each emitted frame is one graph output; the sink drains until EAGAIN/EOF
        try:
            yield graph.pull()
        except BlockingIOError, EOFError:
            return


def _scenes_native(reader: object, threshold: float, /) -> tuple[float, ...]:
    graph = av.filter.Graph()
    src = graph.add_buffer(template=reader.streams.video[0])
    graph.link_nodes(src, graph.add("select", f"gt(scene,{threshold})"), graph.add("buffersink"))
    graph.configure()
    cuts: list[float] = []
    for frame in reader.decode(video=0):  # Exemption: imperative graph drive over one owned filter handle
        graph.push(frame)
        cuts.extend(float(cut.pts * cut.time_base) if cut.pts is not None else 0.0 for cut in _pull_frames(graph))
    graph.push(None)
    cuts.extend(float(cut.pts * cut.time_base) if cut.pts is not None else 0.0 for cut in _pull_frames(graph))
    return tuple(cuts)


def _frame_delta(prior: "NDArray[np.uint8]", current: "NDArray[np.uint8]", /) -> float:
    # normalized mean absolute difference, the numpy scene floor; measure Transform.SSIM is the perceptual-grade seam.
    return float(np.abs(current.astype(np.int16) - prior.astype(np.int16)).mean() / 255.0)


def _scenes_substitute(reader: object, threshold: float, /) -> tuple[float, ...]:
    prior: "NDArray[np.uint8] | None" = None
    cuts: list[float] = []
    for frame in reader.decode(video=0):  # Exemption: sequential decode holds one prior frame; a fold would retain the stream
        current = frame.to_ndarray(format="rgb24")
        if prior is not None and _frame_delta(prior, current) > threshold:
            cuts.append(float(frame.pts * frame.time_base) if frame.pts is not None else 0.0)
        prior = current
    return tuple(cuts)


def _colormap(grid: "NDArray[np.float64]", /) -> "NDArray[np.uint8]":
    # normalize a magnitude/level grid to 0..255 and lift to the rgba raster `_save_array` encodes.
    span = float(grid.max() - grid.min())
    norm = np.clip((grid - grid.min()) / (span + 1e-9), 0.0, 1.0)
    gray = (norm * 255.0).astype(np.uint8)
    return np.stack([gray, gray, gray, np.full_like(gray, 255)], axis=-1)


def _sheet(picked: tuple["NDArray[np.uint8]", ...], /) -> "NDArray[np.uint8]":
    # numpy contact sheet: a near-square grid padded with black cells; graphic/raster io's Montage is the styled owner.
    columns = max(int(np.ceil(np.sqrt(len(picked)))), 1)
    rows = int(np.ceil(len(picked) / columns))
    padded = (*picked, *(np.zeros_like(picked[0]),) * (rows * columns - len(picked)))
    return np.concatenate([np.concatenate(padded[r * columns : (r + 1) * columns], axis=1) for r in range(rows)], axis=0)


def _png(rgba: "NDArray[np.uint8]", /) -> bytes:
    return _save_array(rgba, frozendict()).data


def _wave_facts(mono: "NDArray[np.float64]", /) -> frozendict[str, float | str]:
    peak, rms = _envelope(mono, _STFT_WINDOW)
    return frozendict({"peak_db": float(20.0 * np.log10(peak.max() + 1e-9)), "rms_db": float(20.0 * np.log10(rms.mean() + 1e-9))})


def _spectral_facts(mono: "NDArray[np.float64]", rate: int, /) -> frozendict[str, float | str]:
    # a self-contained spectral-centroid read; compute/analysis/transform#TRANSFORM SpectralReadout.CENTROID is the deeper seam.
    grid = np.power(10.0, _stft(mono) / 20.0)
    freqs = np.fft.rfftfreq(_STFT_WINDOW, 1.0 / rate)
    centroid = float((freqs[:, np.newaxis] * grid).sum() / (grid.sum() + 1e-9))
    return frozendict({"centroid_hz": centroid})


_SPECTRAL_FAMILY: frozenset[AudioMetric] = frozenset({
    AudioMetric.CENTROID_HZ, AudioMetric.ROLLOFF_HZ, AudioMetric.BANDWIDTH_HZ, AudioMetric.FLATNESS,
})


def _time_family(mono: "NDArray[np.float64]", /) -> frozendict[AudioMetric, float]:
    peak, rms = float(np.abs(mono).max()), float(np.sqrt(np.mean(mono * mono)))
    levels = np.abs(mono)
    return frozendict({
        AudioMetric.PEAK_DBFS: float(20.0 * np.log10(peak + 1e-12)),
        AudioMetric.RMS_DBFS: float(20.0 * np.log10(rms + 1e-12)),
        AudioMetric.CREST_FACTOR: float(peak / (rms + 1e-12)),
        AudioMetric.DC_OFFSET: float(mono.mean()),
        AudioMetric.ZERO_CROSSING: float(np.mean(np.signbit(mono[1:]) != np.signbit(mono[:-1]))) if mono.size > 1 else 0.0,
        AudioMetric.DYNAMIC_RANGE_DB: float(20.0 * np.log10((np.percentile(levels, 95) + 1e-12) / (np.percentile(levels, 10) + 1e-12))),
    })


def _spectral_family(mono: "NDArray[np.float64]", rate: int, /) -> frozendict[AudioMetric, float]:
    magnitude = np.abs(np.fft.rfft(mono * np.hanning(mono.size))) + 1e-12
    power = magnitude * magnitude
    freqs = np.fft.rfftfreq(mono.size, 1.0 / rate)
    total = float(power.sum()) + 1e-12
    centroid = float((freqs * power).sum() / total)
    return frozendict({
        AudioMetric.CENTROID_HZ: centroid,
        AudioMetric.ROLLOFF_HZ: float(freqs[min(int(np.searchsorted(np.cumsum(power), 0.85 * total)), freqs.size - 1)]),
        AudioMetric.BANDWIDTH_HZ: float(np.sqrt((((freqs - centroid) ** 2) * power).sum() / total)),
        AudioMetric.FLATNESS: float(np.exp(np.log(magnitude).mean()) / magnitude.mean()),
    })


def _metrics(decoded: "tuple[tuple[Pcm, ...], int, str]", selected: tuple[AudioMetric, ...], /) -> AnalysisProduct:
    # dependency-aware evaluation: the time family reads only the mono block and the spectral family alone pays the
    # windowed FFT — each computed IFF the selection names one of its members, so a partial selection skips
    # every unrelated statistic while the payload format stays byte-identical for the same selection.
    _blocks, rate, _layout = decoded
    mono = _mono(decoded)
    wanted = frozenset(selected)
    values: frozendict[AudioMetric, float] = (_time_family(mono) if wanted - _SPECTRAL_FAMILY else frozendict()) | (
        _spectral_family(mono, rate) if wanted & _SPECTRAL_FAMILY else frozendict()
    )
    facts = frozendict({metric.value: values[metric] for metric in selected})
    payload = msgspec.json.encode(dict(facts))
    return payload, AnalysisEvidence.measure("json", AnalysisArm.SUBSTITUTE, mono.size / rate, payload, len(facts), facts)


def _worker[**P](operation: Callable[P, Result[AnalysisProduct, MediaFault]], /) -> Callable[P, Result[AnalysisProduct, MediaFault]]:
    guarded = beartype(operation)

    @wraps(operation)
    def call(*args: P.args, **kwargs: P.kwargs) -> Result[AnalysisProduct, MediaFault]:
        try:
            return guarded(*args, **kwargs)
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=str(violation)))

    return call


def _admitted(op: AnalysisOp, /) -> Result[AnalysisOp, MediaFault]:
    # every knob proves BOTH edges — the declared ceilings above are the upper bounds — and every metrics
    # member proves `AudioMetric` membership here, so `_metrics` dispatch is total and never raises `KeyError`.
    match op:
        case AnalysisOp(tag="waveform", waveform=(blob, (width, height))) | AnalysisOp(tag="spectrogram", spectrogram=(blob, (width, height))):
            valid = bool(blob) and 0 < width <= _MAX_EXTENT and 0 < height <= _MAX_EXTENT
        case AnalysisOp(tag="loudness", loudness=(blob, target)):
            valid = bool(blob) and all(isfinite(value) for value in (target.i, target.tp, target.lra)) and target.lra > 0.0
        case AnalysisOp(tag="silence", silence=(blob, threshold, minimum)):
            valid = bool(blob) and isfinite(threshold) and isfinite(minimum) and 0.0 < minimum <= _MAX_SPAN_SECONDS
        case AnalysisOp(tag="black_detect", black_detect=(blob, luma, minimum)):
            valid = bool(blob) and isfinite(luma) and 0.0 <= luma <= 1.0 and isfinite(minimum) and 0.0 < minimum <= _MAX_SPAN_SECONDS
        case AnalysisOp(tag="scene_detect", scene_detect=(blob, threshold)):
            valid = bool(blob) and isfinite(threshold) and 0.0 <= threshold <= 1.0
        case AnalysisOp(tag="thumbnail", thumbnail=(blob, count)):
            valid = bool(blob) and 0 < count <= _MAX_THUMBNAILS
        case AnalysisOp(tag="metrics", metrics=(blob, metrics)):
            # canonical-tuple equality also rejects duplicates and unsorted members, so a directly-constructed
            # op can never reach key derivation on a noncanonical identity payload the `of` factory would dedup.
            valid = bool(blob) and bool(metrics) and all(isinstance(metric, AudioMetric) for metric in metrics) and metrics == tuple(sorted(set(metrics)))
        case _ as unreachable:
            assert_never(unreachable)
    return Ok(op) if valid else Error(MediaFault(invalid=f"invalid analysis payload for {op.tag}"))


@_worker
def _analyze(op: AnalysisOp, /) -> Result[AnalysisProduct, MediaFault]:
    # arm derivation runs HERE, on the process side, so the av registry probe never loads on the event loop.
    try:
        return _admitted(op).bind(lambda admitted: _analyzed(admitted, AnalysisArm.of(op.tag, media_filters())))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault(f"analysis.{op.tag}", exc))


def _analyzed(op: AnalysisOp, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    # one capability-routed body per op: the native arm runs the FFmpeg filter, the substitute the numpy floor; both
    # fold into one AnalysisProduct keyed and carrying the measured band onto the shared Media case.
    match op:
        case AnalysisOp(tag="waveform", waveform=(blob, size)) | AnalysisOp(tag="spectrogram", spectrogram=(blob, size)):
            return _rendered(op.tag, blob, size, arm)
        case AnalysisOp(tag="loudness", loudness=(blob, target)):
            return _loudness(blob, target, arm)
        case AnalysisOp(tag="silence", silence=(blob, threshold_db, min_seconds)):
            return _silence(blob, threshold_db, min_seconds)
        case AnalysisOp(tag="black_detect", black_detect=(blob, luma, min_seconds)):
            return _black(blob, luma, min_seconds)
        case AnalysisOp(tag="scene_detect", scene_detect=(blob, threshold)):
            return _scene(blob, threshold, arm)
        case AnalysisOp(tag="thumbnail", thumbnail=(blob, count)):
            return _thumbnail(blob, count, arm)
        case AnalysisOp(tag="metrics", metrics=(blob, selected)):
            return _decode_audio(blob).map(lambda decoded: _metrics(decoded, selected))
        case _ as unreachable:
            assert_never(unreachable)


def _rendered(kind: AnalysisTag, blob: bytes, size: tuple[int, int], arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    node = "showwavespic" if kind == "waveform" else "showspectrumpic"
    with av.open(io.BytesIO(blob), mode="r") as reader:
        if not reader.streams.audio:
            return Error(MediaFault(invalid=f"{kind} requires an audio stream"))
        stream = reader.streams.audio[0]
        duration = _source_seconds(reader)
        match arm:
            case AnalysisArm.NATIVE:
                graph = av.filter.Graph()
                src = graph.add_abuffer(template=stream)
                graph.link_nodes(src, graph.add(node, f"s={size[0]}x{size[1]}"), graph.add("buffersink"))
                graph.configure()
                rgba = _audio_image(reader, graph)
                return _decode_audio(blob).map(lambda decoded: _rendered_product(kind, rgba, decoded, arm, duration))
            case AnalysisArm.SUBSTITUTE:
                return _decode_audio(blob).map(lambda decoded: _rendered_substitute(kind, decoded, arm, duration, size))
            case _ as unreachable:
                assert_never(unreachable)


def _rendered_product(
    kind: AnalysisTag,
    rgba: "NDArray[np.uint8]",
    decoded: "tuple[tuple[Pcm, ...], int, str]",
    arm: AnalysisArm,
    duration: float,
    /,
) -> AnalysisProduct:
    # count is 1 — one rendered image; feeding a pixel dimension into the receipt frame-count slot is false evidence.
    _blocks, rate, _layout = decoded
    mono = _mono(decoded)
    facts = _spectral_facts(mono, rate) if kind == "spectrogram" else _wave_facts(mono)
    png = _png(rgba)
    return png, AnalysisEvidence.measure("png", arm, duration, png, 1, facts | {"render": arm.value})


def _rendered_substitute(
    kind: AnalysisTag, decoded: "tuple[tuple[Pcm, ...], int, str]", arm: AnalysisArm, duration: float, size: tuple[int, int], /
) -> AnalysisProduct:
    _blocks, rate, _layout = decoded
    mono = _mono(decoded)
    grid = _stft(mono) if kind == "spectrogram" else _envelope(mono, rate // size[0] or 1)[1][np.newaxis, :]
    rows = np.linspace(0, grid.shape[0] - 1, size[1]).astype(np.intp)
    columns = np.linspace(0, grid.shape[1] - 1, size[0]).astype(np.intp)
    return _rendered_product(kind, _colormap(grid[np.ix_(rows, columns)]), decoded, arm, duration)


def _loudness(blob: bytes, target: LoudnessTarget, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        if not reader.streams.audio:
            return Error(MediaFault(invalid="loudness requires an audio stream"))
        match arm:
            case AnalysisArm.NATIVE:
                raw = av.filter.loudnorm.stats(f"i={target.i}:tp={target.tp}:lra={target.lra}", reader.streams.audio[0])
                stats = msgspec.json.decode(raw, type=dict[str, str])
                band: frozendict[str, float | str] = frozendict({
                    "integrated_lufs": float(stats["input_i"]),
                    "true_peak": float(stats["input_tp"]),
                    "lra": float(stats["input_lra"]),
                })
            case AnalysisArm.SUBSTITUTE:
                return _decode_audio(blob).map(lambda decoded: _loudness_substitute(decoded, arm, _source_seconds(reader)))
            case _ as unreachable:
                assert_never(unreachable)
        payload = msgspec.json.encode(dict(band))
        return Ok((payload, AnalysisEvidence.measure("json", arm, _source_seconds(reader), payload, 0, band)))


def _loudness_substitute(decoded: "tuple[tuple[Pcm, ...], int, str]", arm: AnalysisArm, duration: float, /) -> AnalysisProduct:
    _blocks, rate, _layout = decoded
    mono = _mono(decoded)
    gated, spread = _gated_loudness(mono, rate)
    sample_peak = float(20.0 * np.log10(np.abs(mono).max() + 1e-12))
    band = frozendict({"gated_loudness": gated, "sample_peak_dbfs": sample_peak, "short_term_spread": spread, "k_weighted": "false"})
    payload = msgspec.json.encode(dict(band))
    return payload, AnalysisEvidence.measure("json", arm, duration, payload, 0, band)


def _spanned(spans: tuple[tuple[float, float], ...], duration: float, kind: str, /) -> AnalysisProduct:
    total = sum(hi - lo for lo, hi in spans)
    band = frozendict({f"{kind}_spans": float(len(spans)), f"{kind}_ratio": total / duration if duration else 0.0})
    payload = msgspec.json.encode({"spans": spans, **dict(band)})
    return payload, AnalysisEvidence.measure("json", AnalysisArm.SUBSTITUTE, duration, payload, len(spans), band)


def _silence(blob: bytes, threshold_db: float, min_seconds: float, /) -> Result[AnalysisProduct, MediaFault]:
    return _decode_audio(blob).map(lambda decoded: _silenced(decoded, threshold_db, min_seconds))


def _silenced(decoded: "tuple[tuple[Pcm, ...], int, str]", threshold_db: float, min_seconds: float, /) -> AnalysisProduct:
    _blocks, rate, _layout = decoded
    mono = _mono(decoded)
    window = rate // 10 or 1
    _peak, rms = _envelope(mono, window)
    quiet = (20.0 * np.log10(rms + 1e-9)) < threshold_db
    return _spanned(_flag_spans(quiet, window / rate, min_seconds), mono.size / rate, "silence")


def _black(blob: bytes, luma: float, min_seconds: float, /) -> Result[AnalysisProduct, MediaFault]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        if not reader.streams.video:
            return Error(MediaFault(invalid="black detection requires a video stream"))
        rate = float(reader.streams.video[0].average_rate or 24)
        # `gray` is swscale's Rec. 601 luma plane — the Y the blackdetect filter gates on — where an equal-weight
        # rgb24 mean over-lights saturated blues and under-lights greens against the luma threshold; np.fromiter
        # streams one bool per decoded frame under the _MAX_FLAG_FRAMES budget, and _flag_spans stays the shared
        # maximal-run algebra the silence arm folds through.
        flags = (frame.to_ndarray(format="gray").mean() / 255.0 < luma for frame in islice(reader.decode(video=0), _MAX_FLAG_FRAMES + 1))
        dark = np.fromiter(flags, dtype=np.bool_)
        if dark.size > _MAX_FLAG_FRAMES:
            return Error(MediaFault(invalid=f"black detection frame budget exceeded ({_MAX_FLAG_FRAMES})"))
        return Ok(_spanned(_flag_spans(dark, 1.0 / rate, min_seconds), dark.size / rate, "black"))


def _scene(blob: bytes, threshold: float, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        if not reader.streams.video:
            return Error(MediaFault(invalid="scene detection requires a video stream"))
        cuts = _scenes_native(reader, threshold) if arm is AnalysisArm.NATIVE else _scenes_substitute(reader, threshold)
        band: frozendict[str, float | str] = frozendict({"scene_cuts": float(len(cuts)), "cut_seconds": ",".join(f"{c:.3f}" for c in cuts)})
        payload = msgspec.json.encode({"cuts": cuts})
        return Ok((payload, AnalysisEvidence.measure("json", arm, _source_seconds(reader), payload, len(cuts), band)))


def _thumbnail(blob: bytes, count: int, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        if not reader.streams.video:
            return Error(MediaFault(invalid="thumbnail extraction requires a video stream"))
        video = reader.streams.video[0]
        match arm:
            case AnalysisArm.NATIVE:
                graph = av.filter.Graph()
                src = graph.add_buffer(template=video)
                graph.link_nodes(src, graph.add("thumbnail", f"n={max(1, (video.frames or count) // count)}"), graph.add("buffersink"))
                graph.configure()
                picked: list["NDArray[np.uint8]"] = []
                for frame in reader.decode(video=0):  # Exemption: imperative graph drive over one owned filter handle
                    graph.push(frame)
                    picked.extend(pulled.to_ndarray(format="rgb24") for pulled in _pull_frames(graph))
                graph.push(None)
                picked.extend(pulled.to_ndarray(format="rgb24") for pulled in _pull_frames(graph))
            case AnalysisArm.SUBSTITUTE:
                ranked = nlargest(
                    count,
                    (
                        (float(np.var(array)), index, array)
                        for index, frame in enumerate(reader.decode(video=0))
                        if (array := frame.to_ndarray(format="rgb24")) is not None
                    ),
                    key=itemgetter(0),
                )
                picked = [array for _, _, array in sorted(ranked, key=itemgetter(1))]
            case _ as unreachable:
                assert_never(unreachable)
        chosen = tuple(picked[:count])
        if not chosen:
            return Error(MediaFault(invalid="thumbnail source produced no video frames"))
        sheet = _png(_sheet(chosen))
        return Ok(
            (
                sheet,
                AnalysisEvidence.measure("png", arm, _source_seconds(reader), sheet, len(chosen), frozendict({"thumbnails": float(len(chosen))})),
            )
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
