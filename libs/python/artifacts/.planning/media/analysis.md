# [PY_ARTIFACTS_MEDIA_ANALYSIS]

`Analysis` is the read-side analysis leaf of the media plane: it decodes an existing container and produces waveform/spectrogram images, EBU R128 loudness, silence spans, scene-cut detection, and representative thumbnails, discriminating the measurements over the closed `AnalysisOp` union keyed over the produced evidence bytes, never a parallel `measure_loudness`/`detect_scenes`/`extract_waveform` function family. Each op is a capability-detection case: a native arm running the FFmpeg filter when the linked build exposes it, else a verified in-process substitute, selected by the `_NATIVE` required-filter table against the `media/filtergraph#FILTER` `media_filters` probe. This page owns the `AnalysisOp` vocabulary, the `_NATIVE` capability table, the `AnalysisEvidence` carrier, and the six worker arms; it re-authors no encode and re-implements no filter FFmpeg already ships.

It composes the `media/container#CONTAINER` decode capsule and its `MediaFault`/`_media_fault`/`_deployment`/`WORKER_BAND`/`_WORKER_RETRY` surface, the `media/audio#MEDIA` `_decode_audio` Pcm ingest, the `media/filtergraph#FILTER` `media_filters` probe, `graphic/raster/io#IO` for the PNG render and thumbnail montage, and `graphic/raster/measure#MEASURE` for the SSIM scene substitute. Native routings are `showwavespic`/`showspectrumpic` (audio->image), `av.filter.loudnorm.stats` (the two-pass R128 JSON the single-pass `media/audio#MEDIA` encode cannot expose), `select='gt(scene,t)'`, and `thumbnail`; `Silence` is substitute-only because PyAV frames surface no `silencedetect` metadata, its `_NATIVE` row the empty set. Richer scipy spectrogram and analytic-signal reads are the `compute/analysis/signal#DSP` and `compute/analysis/transform#TRANSFORM` cross-branch seams the substitutes reach for depth; the numpy floor is self-contained. Every op contributes the single `core/receipt#RECEIPT` `ArtifactReceipt.Media` case, the scene-cut/silence/loudness scalars riding its `facts` band, and enters the `core/plan#PLAN` pipeline as an `ArtifactWork` node keyed by its content key.

## [01]-[INDEX]

- [01]-[ANALYSIS]: the `Analysis` owner over the closed `AnalysisOp` family — `Waveform`/`Spectrogram`/`Loudness`/`Silence`/`SceneDetect`/`Thumbnail` — each capability-routed native-or-substitute and folding into one `AnalysisEvidence` carrier keyed over the produced evidence bytes.

## [02]-[ANALYSIS]

- Owner: `Analysis` discriminates modality over the closed `AnalysisOp` family, each case carrying its typed payload (a source `blob` plus its op knobs), never a shared erased `params` bag, a per-measurement subclass, or a parallel `waveform`/`loudness`/`scenes` trio; `AnalysisArm` the closed `NATIVE`/`SUBSTITUTE` route selected by `_NATIVE[op.tag] <= media_filters` — a limited wheel routes to the substitute and a full build to the native filter with the same evidence shape, never a `use_native: bool` knob; `AnalysisEvidence` the frozen carrier this page owns (the artifact `container` tag, the route `codec`, source `duration`, `byte_count`, `count`, and the measured `facts` band) projecting onto `ArtifactReceipt.Media` at `_keyed`; `MediaFault` the container cause vocabulary threaded unchanged; `Result[(ContentKey, ArtifactReceipt), MediaFault]` the one carrier keyed over the produced bytes.
- Cases: `Waveform(blob, size)`/`Spectrogram(blob, size)` the audio->image arms (native the `showwavespic`/`showspectrumpic` graph pulling one image `VideoFrame`, substitute a numpy peak/RMS envelope or framed-`rfft` magnitude grid rendered through `graphic/raster/io#IO`); `Loudness(blob, target)` the EBU R128 arm (native `av.filter.loudnorm.stats` parsing the `input_i`/`input_tp`/`input_lra` JSON, substitute a numpy RMS-dBFS approximation flagged un-gated); `Silence(blob, threshold_db, min_seconds)` the numpy RMS-threshold fold over `_decode_audio` blocks, substitute-only; `SceneDetect(blob, threshold)` (native `select='gt(scene,t)'` counting the emitted frames and their `pts`, substitute `graphic/raster/measure#MEASURE` `structural_similarity` drop); `Thumbnail(blob, count)` (native `thumbnail` pick, substitute a numpy variance pick, both montaged into one contact sheet) — one total `match` recovers the modality from the discriminant.
- Entry: `emit()` returns the `ArtifactWork` node; `_emit` is the `async_boundary` wrapper returning `RuntimeRail[ArtifactReceipt]`, the domain `MediaFault` folded into the boundary rail exactly as `media/container#CONTAINER` folds it, `_keyed` deriving `ContentIdentity.of(container, blob)` and spreading `AnalysisEvidence` onto the case; `_dispatch` reads the `media_filters` set once, selects the arm through `_NATIVE[op.tag] <= media_filters`, and dispatches the synchronous av/numpy body onto `_WORKER_RETRY(to_process.run_sync, _worker, ..., limiter=WORKER_BAND)`, each `@beartype`-woven worker opening its own read capsule and returning picklable `Result[AnalysisProduct, MediaFault]`.
- Auto: the route is `_NATIVE[op.tag] <= media_filters`, so a producer never passes a `use_native` flag and a new native dependency is one `_NATIVE` row (a `Silence` mapped to the empty set is always-substitute, its intent recorded as data); a `Waveform`/`Spectrogram`/`Thumbnail` produces a PNG and a `Loudness`/`Silence`/`SceneDetect` a `msgspec.json` facts blob, both keyed and both carrying the measured band.
- Receipt: each analysis contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, 0, facts)` — the artifact tag onto `container`, the route onto `codec`, the measured scalars onto the `facts` band, so the loudness LUFS/true-peak/LRA and waveform peak/RMS ride the shared band with zero receipt edit; the `AnalysisEvidence` carrier stays analysis-owned, the receipt owner importing no `av` handle. `Loudness`'s R128 band is exactly what `media/audio#MEDIA` deferred: the single-pass encode cannot expose the gated integrated LUFS, so the two-pass `loudnorm.stats` measurement lands here, never a parallel audio receipt.
- Growth: a new measurement (a beat detector, an optical-flow magnitude, a chapters read) is one `AnalysisOp` case plus one `_NATIVE` row plus one worker arm plus one `_dispatch` arm; a new native filter dependency one `_NATIVE` row; a new measured fact one band key on the widened `facts` with zero receipt edit; a new substitute swaps one arm body behind the same `AnalysisArm` route; a richer spectrogram is the `compute/analysis/signal#DSP` cross-branch seam behind the `Spectrogram` substitute — every addition a case, row, arm, or band key on one owner.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Iterator
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
from artifacts.media.container import MediaFault, _WORKER_RETRY, _deployment, _media_fault
from artifacts.media.audio import Pcm, _decode_audio

lazy import av
lazy import av.error
lazy import av.filter
lazy import av.filter.loudnorm  # loudnorm.stats(args, stream) -> JSON bytes (the two-pass R128 measurement)
lazy from artifacts.media.filtergraph import media_filters  # av.filter.filters_available canonicalized as the capability probe
lazy from artifacts.graphic.raster.io import render_png, montage  # rgba -> PNG bytes / frame strip -> contact-sheet rgba
lazy from artifacts.graphic.raster.measure import frame_similarity  # structural_similarity(prev, cur) -> float, the scene substitute

# --- [TYPES] ----------------------------------------------------------------------------

type AnalysisTag = Literal["waveform", "spectrogram", "loudness", "silence", "scene_detect", "thumbnail"]

# --- [CONSTANTS] ------------------------------------------------------------------------

# each op routes NATIVE only when its whole `_NATIVE` set is present in `av.filter.filters_available`, else the
# substitute. `silence` maps to the empty set: PyAV surfaces no `silencedetect`/`astats` frame metadata, so the
# numpy RMS-threshold fold is the only arm.
_NATIVE: frozendict[AnalysisTag, frozenset[str]] = frozendict({
    "waveform": frozenset({"showwavespic"}),
    "spectrogram": frozenset({"showspectrumpic"}),
    "loudness": frozenset({"loudnorm"}),
    "silence": frozenset(),
    "scene_detect": frozenset({"select"}),
    "thumbnail": frozenset({"thumbnail"}),
})

_STFT_WINDOW: int = 1024
_STFT_HOP: int = 256

# --- [MODELS] ---------------------------------------------------------------------------


class LoudnessTarget(Struct, frozen=True):
    i: float = -24.0  # integrated-LUFS target the two-pass measurement gates against
    tp: float = -2.0  # true-peak dBTP ceiling
    lra: float = 7.0  # loudness range


@tagged_union(frozen=True)
class AnalysisArm:
    tag: Literal["native", "substitute"] = tag()
    native: None = case()
    substitute: None = case()

    @staticmethod
    def of(op_tag: AnalysisTag, filters: frozenset[str], /) -> "AnalysisArm":
        return AnalysisArm(native=None) if _NATIVE[op_tag] <= filters else AnalysisArm(substitute=None)


@tagged_union(frozen=True)
class AnalysisOp:
    tag: AnalysisTag = tag()
    waveform: tuple[bytes, tuple[int, int]] = case()  # (container bytes, (width, height))
    spectrogram: tuple[bytes, tuple[int, int]] = case()
    loudness: tuple[bytes, LoudnessTarget] = case()
    silence: tuple[bytes, float, float] = case()  # (bytes, threshold_db, min_seconds)
    scene_detect: tuple[bytes, float] = case()  # (bytes, scene threshold 0..1)
    thumbnail: tuple[bytes, int] = case()  # (bytes, thumbnail count)

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
    def SceneDetect(blob: bytes, threshold: float = 0.4, /) -> "AnalysisOp":
        return AnalysisOp(scene_detect=(blob, threshold))

    @staticmethod
    def Thumbnail(blob: bytes, count: int = 9, /) -> "AnalysisOp":
        return AnalysisOp(thumbnail=(blob, count))


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
        return AnalysisEvidence(container, f"analysis-{arm.tag}", duration, len(blob), count, facts)


class AnalysisProduct(Struct, frozen=True):
    blob: bytes
    evidence: AnalysisEvidence


class Analysis(Struct, frozen=True):
    op: AnalysisOp

    @staticmethod
    def of(op: AnalysisOp, /) -> "Analysis":
        return Analysis(op=op)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the canonical frozen op minted PRE-RUN; the produced bytes' content address rides the receipt facts.
        return ContentIdentity.of(f"media.analysis-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the inner Result[..., MediaFault] folds into the rail's boundary fault (Work[ArtifactReceipt] forbids an
        # inner Result); the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        railed = await async_boundary(f"media.analysis.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map(lambda pair: pair[1]).map_error(
                lambda fault: BoundaryFault(boundary=(f"media.analysis.{self.op.tag}", f"{fault.tag}:{fault}"))
            )
        )

    async def _folded(self, /) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
        return (await self._dispatch()).map(self._keyed)

    def _keyed(self, product: AnalysisProduct, /) -> tuple[ContentKey, ArtifactReceipt]:
        ev, blob = product.evidence, product.blob
        key = ContentIdentity.of(ev.container, blob)
        return key, ArtifactReceipt.Media(key, ev.container, ev.codec, ev.duration, ev.byte_count, ev.count, 0, ev.facts)

    async def _dispatch(self, /) -> Result[AnalysisProduct, MediaFault]:
        arm = AnalysisArm.of(self.op.tag, media_filters())
        try:
            return await _WORKER_RETRY(to_process.run_sync, _worker, self.op, arm, limiter=WORKER_BAND)
        except BrokenWorkerProcess as broken:
            return Error(MediaFault(worker=str(broken)))
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=type(violation).__name__))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _source_seconds(reader: object, /) -> float:
    return float(reader.duration / av.time_base) if reader.duration is not None else 0.0


def _audio_image(reader: object, graph: object, /) -> NDArray[np.uint8]:
    # the audio->image native arm: push each decoded AudioFrame into a showwavespic/showspectrumpic graph, flush,
    # and pull the single rendered image VideoFrame back as an rgba array (no metadata read, the frame IS the image).
    for frame in reader.decode(audio=0):
        graph.push(frame)
    graph.push(None)
    return graph.pull().to_ndarray(format="rgba")


def _mono(blocks: tuple[Pcm, ...], /) -> NDArray[np.float64]:
    return np.concatenate([np.asarray(b, np.float64).reshape(-1, max(b.ndim, 1))[..., 0] for b in blocks]) if blocks else np.zeros(1)


def _envelope(blocks: tuple[Pcm, ...], window: int, /) -> tuple[NDArray[np.float64], NDArray[np.float64]]:
    # the numpy waveform substitute: mono-mix the interleaved blocks, then windowed peak and RMS via one reduceat.
    mono = _mono(blocks)
    starts = np.arange(0, mono.size, max(window, 1))
    peak = np.maximum.reduceat(np.abs(mono), starts)
    rms = np.sqrt(np.maximum.reduceat(mono * mono, starts) / max(window, 1))
    return peak, rms


def _stft(blocks: tuple[Pcm, ...], /) -> NDArray[np.float64]:
    # the numpy spectrogram substitute: framed rfft over hanning-windowed hops -> log-magnitude time-frequency grid;
    # the compute/analysis/signal#DSP `SignalOp.Spectral` seam is the richer scipy path this floor stands in for.
    mono = _mono(blocks)
    win = np.hanning(_STFT_WINDOW)
    hops = tuple(range(0, max(mono.size - _STFT_WINDOW, 0) + 1, _STFT_HOP))
    grid = np.stack([np.abs(np.fft.rfft(win * mono[h : h + _STFT_WINDOW])) for h in hops], axis=1) if hops else np.zeros((_STFT_WINDOW // 2 + 1, 1))
    return 20.0 * np.log10(grid + 1e-9)


def _silence_spans(blocks: tuple[Pcm, ...], rate: int, threshold_db: float, min_seconds: float, /) -> tuple[tuple[float, float], ...]:
    # the numpy silence fold: per-window RMS-dB, the maximal runs below threshold longer than min_seconds are spans.
    window = rate // 10 or 1
    _peak, rms = _envelope(blocks, window)
    quiet = (20.0 * np.log10(rms + 1e-9)) < threshold_db
    step = window / rate
    spans: list[tuple[float, float]] = []
    start: int | None = None
    for index, low in enumerate((*quiet.tolist(), False)):
        if low and start is None:
            start = index
        elif not low and start is not None:
            if (index - start) * step >= min_seconds:
                spans.append((start * step, index * step))
            start = None
    return tuple(spans)


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
    for frame in reader.decode(video=0):
        graph.push(frame)
        cuts.extend(float(cut.pts * cut.time_base) if cut.pts is not None else 0.0 for cut in _pull_frames(graph))
    graph.push(None)
    cuts.extend(float(cut.pts * cut.time_base) if cut.pts is not None else 0.0 for cut in _pull_frames(graph))
    return tuple(cuts)


def _scenes_substitute(reader: object, threshold: float, /) -> tuple[float, ...]:
    prior: NDArray[np.uint8] | None = None
    cuts: list[float] = []
    for frame in reader.decode(video=0):
        current = frame.to_ndarray(format="rgb24")
        if prior is not None and frame_similarity(prior, current) < 1.0 - threshold:
            cuts.append(float(frame.pts * frame.time_base) if frame.pts is not None else 0.0)
        prior = current
    return tuple(cuts)


def _colormap(grid: NDArray[np.float64], /) -> NDArray[np.uint8]:
    # the substitute image render: normalize a magnitude/level grid to 0..255 and lift to an rgba raster the io owner encodes.
    span = float(grid.max() - grid.min())
    norm = np.clip((grid - grid.min()) / (span + 1e-9), 0.0, 1.0)
    gray = (norm * 255.0).astype(np.uint8)
    return np.stack([gray, gray, gray, np.full_like(gray, 255)], axis=-1)


def _wave_facts(blocks: tuple[Pcm, ...], /) -> frozendict[str, float | str]:
    peak, rms = _envelope(blocks, _STFT_WINDOW)
    return frozendict({"peak_db": float(20.0 * np.log10(peak.max() + 1e-9)), "rms_db": float(20.0 * np.log10(rms.mean() + 1e-9))})


def _spectral_facts(blocks: tuple[Pcm, ...], rate: int, /) -> frozendict[str, float | str]:
    # a self-contained spectral-centroid read; compute/analysis/transform#TRANSFORM SpectralReadout.CENTROID is the deeper seam.
    grid = np.power(10.0, _stft(blocks) / 20.0)
    freqs = np.fft.rfftfreq(_STFT_WINDOW, 1.0 / rate)
    centroid = float((freqs[:, np.newaxis] * grid).sum() / (grid.sum() + 1e-9))
    return frozendict({"centroid_hz": centroid})


def _worker(op: AnalysisOp, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    try:
        return _analyzed(op, arm)
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault(f"analysis.{op.tag}", exc))


@beartype
def _analyzed(op: AnalysisOp, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    # one capability-routed body per op: the native arm runs the FFmpeg filter, the substitute the numpy/measure
    # floor; both fold into one AnalysisProduct keyed and carrying the measured band onto the shared Media case.
    match op:
        case AnalysisOp(tag="waveform", waveform=(blob, size)) | AnalysisOp(tag="spectrogram", spectrogram=(blob, size)):
            return _rendered(op.tag, blob, size, arm)
        case AnalysisOp(tag="loudness", loudness=(blob, target)):
            return _loudness(blob, target, arm)
        case AnalysisOp(tag="silence", silence=(blob, threshold_db, min_seconds)):
            return _silence(blob, threshold_db, min_seconds)
        case AnalysisOp(tag="scene_detect", scene_detect=(blob, threshold)):
            return _scene(blob, threshold, arm)
        case AnalysisOp(tag="thumbnail", thumbnail=(blob, count)):
            return _thumbnail(blob, count, arm)
        case _ as unreachable:
            assert_never(unreachable)


def _rendered(kind: AnalysisTag, blob: bytes, size: tuple[int, int], arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    node = "showwavespic" if kind == "waveform" else "showspectrumpic"
    with av.open(io.BytesIO(blob), mode="r") as reader:
        stream = reader.streams.audio[0]
        match arm:
            case AnalysisArm(tag="native"):
                graph = av.filter.Graph()
                src = graph.add_abuffer(sample_rate=stream.sample_rate, format="fltp", layout="stereo")
                graph.link_nodes(src, graph.add(node, f"s={size[0]}x{size[1]}"), graph.add("buffersink"))
                graph.configure()
                rgba = _audio_image(reader, graph)
                facts: frozendict[str, float | str] = frozendict({"render": "native"})
            case AnalysisArm(tag="substitute"):
                blocks, rate = _decode_audio(blob)
                grid = _stft(blocks) if kind == "spectrogram" else _envelope(blocks, rate // size[0] or 1)[1][np.newaxis, :]
                rgba = _colormap(grid)
                facts = _spectral_facts(blocks, rate) if kind == "spectrogram" else _wave_facts(blocks)
            case _ as unreachable:
                assert_never(unreachable)
        png = render_png(rgba)
        return Ok(AnalysisProduct(png, AnalysisEvidence.measure("png", arm, _source_seconds(reader), png, size[1], facts)))


def _loudness(blob: bytes, target: LoudnessTarget, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        match arm:
            case AnalysisArm(tag="native"):
                raw = av.filter.loudnorm.stats(f"i={target.i}:tp={target.tp}:lra={target.lra}", reader.streams.audio[0])
                stats = msgspec.json.decode(raw, type=dict[str, str])
                band: frozendict[str, float | str] = frozendict({
                    "integrated_lufs": float(stats["input_i"]),
                    "true_peak": float(stats["input_tp"]),
                    "lra": float(stats["input_lra"]),
                })
            case AnalysisArm(tag="substitute"):
                blocks, _rate = _decode_audio(blob)
                _peak, rms = _envelope(blocks, _STFT_WINDOW)
                band = frozendict({"integrated_lufs": float(20.0 * np.log10(rms.mean() + 1e-9)), "gated": "false"})
            case _ as unreachable:
                assert_never(unreachable)
        payload = msgspec.json.encode(dict(band))
        return Ok(AnalysisProduct(payload, AnalysisEvidence.measure("json", arm, _source_seconds(reader), payload, 0, band)))


def _silence(blob: bytes, threshold_db: float, min_seconds: float, /) -> Result[AnalysisProduct, MediaFault]:
    blocks, rate = _decode_audio(blob)
    spans = _silence_spans(blocks, rate, threshold_db, min_seconds)
    total = sum(hi - lo for lo, hi in spans)
    duration = _mono(blocks).size / rate if blocks else 0.0
    band = frozendict({"silence_spans": len(spans), "silent_ratio": total / duration if duration else 0.0})
    payload = msgspec.json.encode({"spans": spans, **dict(band)})
    return Ok(AnalysisProduct(payload, AnalysisEvidence.measure("json", AnalysisArm(substitute=None), duration, payload, len(spans), band)))


def _scene(blob: bytes, threshold: float, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        cuts = _scenes_native(reader, threshold) if arm.tag == "native" else _scenes_substitute(reader, threshold)
        band: frozendict[str, float | str] = frozendict({"scene_cuts": len(cuts), "cut_seconds": ",".join(f"{c:.3f}" for c in cuts)})
        payload = msgspec.json.encode({"cuts": cuts})
        return Ok(AnalysisProduct(payload, AnalysisEvidence.measure("json", arm, _source_seconds(reader), payload, len(cuts), band)))


def _thumbnail(blob: bytes, count: int, arm: AnalysisArm, /) -> Result[AnalysisProduct, MediaFault]:
    with av.open(io.BytesIO(blob), mode="r") as reader:
        video = reader.streams.video[0]
        match arm:
            case AnalysisArm(tag="native"):
                graph = av.filter.Graph()
                src = graph.add_buffer(template=video)
                graph.link_nodes(src, graph.add("thumbnail", f"n={max(1, (video.frames or count) // count)}"), graph.add("buffersink"))
                graph.configure()
                picked: list[NDArray[np.uint8]] = []
                for frame in reader.decode(video=0):
                    graph.push(frame)
                    picked.extend(pulled.to_ndarray(format="rgb24") for pulled in _pull_frames(graph))
                graph.push(None)
                picked.extend(pulled.to_ndarray(format="rgb24") for pulled in _pull_frames(graph))
            case AnalysisArm(tag="substitute"):
                frames = tuple(frame.to_ndarray(format="rgb24") for frame in reader.decode(video=0))
                stride = max(1, len(frames) // count)
                picked = [max(frames[i : i + stride], key=lambda f: float(np.var(f))) for i in range(0, len(frames), stride)]
            case _ as unreachable:
                assert_never(unreachable)
        chosen = tuple(picked[:count])
        sheet = render_png(montage(chosen))
        return Ok(
            AnalysisProduct(
                sheet, AnalysisEvidence.measure("png", arm, _source_seconds(reader), sheet, len(chosen), frozendict({"thumbnails": len(chosen)}))
            )
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
