# [PY_ARTIFACTS_MEDIA_ANALYSIS]

The temporal-artifact ANALYSIS arm — the read-side measurement-and-visualization plane that decodes an existing container and produces waveform/spectrogram images, EBU R128 loudness, silence spans, scene-cut detection, and representative thumbnails, EACH routed to its native FFmpeg filter when the linked build exposes it, else a verified in-process substitute. `Analysis` is a frozen `msgspec.Struct` whose op is ONE closed-payload `AnalysisOp` `expression.tagged_union` over the read-side measurements — `Waveform(blob, size)`, `Spectrogram(blob, size)`, `Loudness(blob, target)`, `Silence(blob, threshold_db, min_seconds)`, `SceneDetect(blob, threshold)`, and `Thumbnail(blob, count)` — dispatched by one total `match`/`case` closed by `assert_never`, returning `the `emit()`/`_emit` node contract` keyed over the produced evidence bytes, never a parallel `measure_loudness`/`detect_scenes`/`extract_waveform` function family. Each op is a CAPABILITY-DETECTION case: a native arm reading `media/filtergraph#FILTER`'s `filters_available` probe — `showwavespic`/`showspectrumpic` (audio→image), `av.filter.loudnorm.stats` (the two-pass EBU R128 integrated-LUFS/true-peak-dBTP/loudness-range JSON the single-pass `media/audio#MEDIA` encode does not expose), `select='gt(scene,t)'` (scene-cut frames counted at the sink), `thumbnail` (representative-frame pick) — and a verified substitute arm — a numpy `np.abs`/`np.maximum.reduceat` peak/RMS envelope, a numpy framed-`rfft` magnitude grid, a numpy RMS-dB threshold fold, and `graphic/raster/measure#MEASURE` structural-similarity frame-to-frame drop — selected by the runtime probe, never a hardcoded assumption that a filter exists nor a metadata read PyAV cannot surface. This page composes the `media/container#CONTAINER` decode/demux capsule (the renamed container owner: the `av.open(mode="r")` read capsule, `MediaFault`, `_media_fault`, `_deployment`, `WORKER_BAND`, `_WORKER_RETRY`), the `media/audio#MEDIA` `_decode_audio` Pcm-block ingest, the `media/filtergraph#FILTER` `media_filters` probe, `graphic/raster/io#IO` for the PNG render and thumbnail contact-sheet, and `graphic/raster/measure#MEASURE` for the SSIM scene substitute; it OWNS the `AnalysisOp` vocabulary, the `_NATIVE` capability table, the `AnalysisEvidence` receipt carrier, and the render/measure worker primitives. The richer scipy-backed spectrogram and analytic-signal envelope/centroid are the cross-branch `compute/analysis/signal#DSP` and `compute/analysis/transform#TRANSFORM` seams the substitute reaches for depth; the in-worker numpy substitute is the self-contained floor. Every op contributes the single `ArtifactReceipt.Media` case — the scene-cut/silence-span/loudness facts riding its `facts` band (named on `receipt.md` line 14) — and routes through the one `core/plan#PLAN` `ArtifactPipeline` entry as an `ArtifactWork` node keyed by its content key. This is the analysis leaf of the media plane; it re-authors no encode the container owner holds and re-implements no filter FFmpeg already ships.

## [01]-[INDEX]

- [01]-[ANALYSIS]: the `Analysis` owner over the closed-payload `AnalysisOp` family — `Waveform`/`Spectrogram` the audio→image arms (native `showwavespic`/`showspectrumpic` graph pulling one `VideoFrame` image, substitute a numpy peak/RMS envelope or framed-`rfft` magnitude grid rendered through `graphic/raster/io#IO`), `Loudness` the EBU R128 arm (native `av.filter.loudnorm.stats(args, stream)` parsing the `input_i`/`input_tp`/`input_lra` JSON, substitute a numpy RMS-dBFS approximation), `Silence` the numpy RMS-threshold envelope fold over `_decode_audio` Pcm blocks (the metadata-injecting `silencedetect` native arm is unreachable — PyAV 17 frames expose no `.metadata` — so silence is substitute-driven), `SceneDetect` the cut detector (native `select='gt(scene,t)'` counting the frames the sink emits and reading each `pts`, substitute `graphic/raster/measure#MEASURE` frame-to-frame `structural_similarity` drop), and `Thumbnail` the representative-frame arm (native `thumbnail` filter pick, substitute a numpy variance/entropy pick, both assembled into a contact-sheet through `graphic/raster/io#IO`), each capability-routed by the `_NATIVE` required-filter table against the `media/filtergraph#FILTER` `media_filters` probe and folding into one `AnalysisEvidence` carrier the owner `_keyed` arm spreads onto the single `ArtifactReceipt.Media` case; `av` `open`/`decode`/`streams`/`filter.Graph.add_abuffer`/`add_buffer`/`add`/`link_nodes`/`push`/`pull`/`configure`/`filter.filters_available`/`filter.loudnorm.stats`/`VideoFrame.to_ndarray` settled against the folder `.api`, `numpy` `abs`/`maximum.reduceat`/`fft.rfft`/`hanning`/`sqrt`/`stack`/`clip` settled against the shared `.api`. The `av.open` read capsule and the `MediaFault`/`_media_fault`/`_deployment`/`WORKER_BAND`/`_WORKER_RETRY` family are `media/container#CONTAINER`'s, the `_decode_audio` primitive `media/audio#MEDIA`'s, the SSIM `graphic/raster/measure#MEASURE`'s, the PNG/montage `graphic/raster/io#IO`'s, the `filters_available` probe `media/filtergraph#FILTER`'s; this page owns the `AnalysisOp` vocabulary, the `_NATIVE` capability table, the `AnalysisEvidence` carrier, and the six worker arms.

## [02]-[ANALYSIS]

- Owner: `Analysis` the one read-side measurement owner discriminating modality over the closed `AnalysisOp` family, worked by module-level functions on the `WORKER_BAND` subprocess lane; `AnalysisOp` an `expression.tagged_union` whose every case carries its own typed payload (a source container `blob` plus its op-specific knobs — an output `size`, a loudness `target`, a `threshold_db`/`min_seconds`, a scene `threshold`, a thumbnail `count`), never a shared erased `params` bag nor a per-measurement `Analysis` subclass nor a parallel `waveform`/`loudness`/`scenes` function trio; `AnalysisArm` the closed `NATIVE`/`SUBSTITUTE` route discriminant NOT a `bool` — each op selects its arm by `_NATIVE[op.tag] <= media_filters`, so a limited PyAV wheel routes to the verified substitute and a full build to the native filter with the same evidence shape, the discriminant the probed capability set the value already answers, never a `use_native: bool` knob; `AnalysisEvidence` the frozen receipt carrier this page OWNS — the artifact `container` tag (`"png"` image or `"json"` measurement), the `codec` route (`"analysis-native"`/`"analysis-substitute"`), the source `duration` seconds, the produced `byte_count`, the `count` (frame or span or cut count), and the `facts` band carrying the measured scalars (`integrated_lufs`/`true_peak`/`lra`, `silence_spans`/`silent_ratio`, `scene_cuts`/`cut_seconds`, `peak_db`/`rms_db`, `centroid_hz`) — with its one `measure` constructor, projecting onto the shared `ArtifactReceipt.Media` case at the owner `_keyed` arm so the receipt owner imports no `av` handle, never a second analysis-receipt rail; `MediaFault` the closed cause vocabulary `media/container#CONTAINER` owns, threaded unchanged so an `av` `FFmpegError` maps through `_media_fault`, an `av` `ImportError` to `provision`, and a probe-empty-but-substitute-also-unavailable case to `codec`; `Result[(ContentKey, ArtifactReceipt), MediaFault]` the one carrier every arm returns, keyed over the produced evidence bytes through `ContentIdentity.of(evidence.container, blob)` so an identical analysis at identical knobs is a cache hit by reference. The owner owns no second `Media` subclass, no per-measurement analysis owner, and no parallel `analyze` surface — the modality is one `AnalysisOp` case, the worker one module-level function pair (native, substitute) selected by the capability probe.
- Cases: `AnalysisOp` cases — `Waveform(blob, size)` (native: `av.open(blob,"r")` decode audio frames pushed into a `showwavespic` `s=WxH` graph, `pull` the one rendered `VideoFrame`, `to_ndarray` to rgba, PNG; substitute: `_decode_audio(blob)` -> numpy blocks -> `np.abs` peak / `np.maximum.reduceat` windowed-RMS envelope -> a rendered waveform raster) · `Spectrogram(blob, size)` (native: `showspectrumpic` graph; substitute: numpy framed `np.fft.rfft` over `np.hanning`-windowed hops -> log-magnitude grid -> colormapped PNG, the richer scipy path the `compute/analysis/signal#DSP` `SignalOp.Spectral(time_frequency=True)` seam offers) · `Loudness(blob, target)` (native: `av.filter.loudnorm.stats(f"i={target.i}:tp={target.tp}:lra={target.lra}", reader.streams.audio[0])` parsing the JSON `input_i`/`input_tp`/`input_lra` into the R128 band; substitute: numpy RMS-dBFS mean as an approximate integrated level with a note it is not gated LUFS) · `Silence(blob, threshold_db, min_seconds)` (`_decode_audio` -> per-window RMS-dB fold -> the runs below `threshold_db` longer than `min_seconds` are the silence spans, the numpy floor the only reachable path since PyAV surfaces no `silencedetect` frame metadata) · `SceneDetect(blob, threshold)` (native: decode video into a `select=gt(scene,{threshold})` graph, each frame the sink emits IS a scene cut counted with its `pts`->seconds; substitute: decode video, `graphic/raster/measure#MEASURE` `structural_similarity` on each adjacent pair, a cut where SSIM < `1 - threshold`) · `Thumbnail(blob, count)` (native: `thumbnail` filter picking one representative frame per batch; substitute: a numpy per-frame variance pick over uniform strata; both `graphic/raster/io#IO` montaged into one contact-sheet PNG) — matched by one total `match`/`case`, the six-case modality recovered from the `AnalysisOp` discriminant, never a name suffix.
- Entry: `emit()` is synchronous, returning the `ArtifactWork` node directly (pre-run `_key`, `work=self._emit`); `_emit` is the `async` boundary wrapper over the runtime `async_boundary` returning `RuntimeRail[ArtifactReceipt]` — the domain `MediaFault` nested inside the boundary rail exactly as `media/container#CONTAINER` nests it; `_emit` maps the `_dispatch` outcome through `_keyed` (deriving `ContentIdentity.of(...)` and spreading `AnalysisEvidence` onto the case), and `_dispatch` matches the `AnalysisOp`, reads the `media_filters` capability set once, selects the native-or-substitute arm through `_NATIVE[op.tag] <= media_filters`, and dispatches the whole synchronous av/numpy body onto `_WORKER_RETRY(to_process.run_sync, _worker, op, arm, limiter=WORKER_BAND)` catching `BrokenWorkerProcess` -> `MediaFault(worker=...)` and `BeartypeCallHintViolation` -> `MediaFault(contract=...)`. Each `@beartype`-woven worker opens its own `av.open(BytesIO(blob), "r")` read capsule (always a context manager so the demuxer/IO releases), runs its arm, reads the evidence bytes plus the measured facts, and returns `Result[AnalysisProduct, MediaFault]` as picklable data across the `to_process` seam — an `av.error.FFmpegError` -> `_media_fault`, an `ImportError` -> `provision`, at the arm that incurs it. `Analysis.of` normalizes the construction over a lone `AnalysisOp`; a `Waveform`/`Spectrogram`/`Thumbnail` produces a PNG artifact and a `Loudness`/`Silence`/`SceneDetect` a compact `msgspec.json` facts blob, both keyed and both carrying the measured band, so every analysis is a viewable-or-inspectable artifact plus one facts contribution.
- Auto: the native-vs-substitute route is `_NATIVE[op.tag] <= media_filters`, so a producer never passes a `use_native` flag and a new native filter dependency is one `_NATIVE` row (a `Silence` mapped to the empty set is always-substitute because no `silencedetect` metadata is readable, its intent recorded as data not a special-case); the audio-visualization native arm pushes each decoded `AudioFrame` into the `add_abuffer -> showwavespic|showspectrumpic -> buffersink` graph and pulls the single image `VideoFrame`, the substitute decodes the same stream to numpy Pcm through `_decode_audio` and folds the envelope (`np.abs(block)` peak, `np.sqrt(np.maximum.reduceat(block**2, starts) / window)` windowed RMS) or the STFT (`np.fft.rfft(np.hanning(n) * hop)` magnitude stacked into the time-frequency grid), each rendered through `graphic/raster/io#IO`; the loudness native arm hands `reader.streams.audio[0]` straight to `av.filter.loudnorm.stats` and `msgspec.json.decode`s the `input_i`/`input_tp`/`input_lra` scalars, the scene native arm counts the frames `select=gt(scene,t)` emits and folds each `float(frame.pts * frame.time_base)` cut time, the thumbnail native arm pulls the `thumbnail`-selected frames and the substitute strides the decode picking the max-variance frame per stratum; the `AnalysisEvidence.measure` folds the artifact tag, the route codec, the source duration (`float(reader.duration / av.time_base)`), the byte count, the count, and the measured band once — the deployment facts riding the same band through `_deployment` so an analysis artifact carries the bundled-libav evidence a container encode does.
- Receipt: each analysis contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate=0, facts)` — the artifact tag onto `container`, the `"analysis-native"`/`"analysis-substitute"` route onto `codec`, the source duration onto `duration`, the byte count onto `bytes`, the frame/span/cut count onto `frames`, `bit_rate=0`, and the measured scalars onto the `facts` band (`receipt.md` line 14 already names the `analysis` scene-cut/silence-span facts, so the loudness LUFS/true-peak/LRA and the waveform peak/RMS ride the same band with ZERO receipt edit — the anticipatory collapse the shared `Media` case was shaped for). The media pages all contribute this single `Media` case, never a parallel analysis-receipt rail, and the `AnalysisEvidence` carrier stays analysis-owned — the receipt owner imports no `av` handle. The `Loudness` R128 band is exactly what `media/audio#MEDIA` deferred here: the single-pass encode cannot expose the gated integrated LUFS, so the two-pass `av.filter.loudnorm.stats` measurement lands as this page's `analysis` facts, never a parallel audio receipt. The producer keys through `ContentIdentity.of(...)` and enters the `core/plan#PLAN` `ArtifactPipeline` as one `ArtifactWork` node whose `parents` are the source container's content key.
- Growth: a new measurement (a `Beat` tempo detector, a `Motion` optical-flow magnitude, a `Chapters` marker read) is one `AnalysisOp` case plus one `_NATIVE` row plus one worker arm plus one `_dispatch` arm, the `assert_never` tail breaking the match at type-check until the arm exists; a new native filter dependency is one `_NATIVE` row; a new measured fact (a `peak_hz` dominant tone, a `dynamic_range`) is one band key on the widened `facts`, ZERO receipt edit; a new substitute (a librosa-grade onset detector) swaps one arm body behind the same `AnalysisArm` route; a richer spectrogram is the `compute/analysis/signal#DSP` cross-branch seam swapped behind the `Spectrogram` substitute arm; zero new surface — the modality space stays the six `AnalysisOp` cases on one owner, every addition a case, row, arm, or band key.

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

# the native FFmpeg filters each op requires; an op routes NATIVE only when its whole set is present in the linked
# build's `av.filter.filters_available`, else the verified substitute. `silence` maps to the empty set on purpose:
# PyAV 17 surfaces no `silencedetect`/`astats` frame metadata, so the numpy RMS-threshold fold is the ONLY arm.
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
        # key-over-INPUT: the canonical frozen op minted PRE-RUN — the muxed output's own content
        # address rides the receipt facts, never the elision key.
        return ContentIdentity.of(f"media.analysis-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the renamed private thunk — the inner Result[..., MediaFault] is DEAD: the member's MediaFault
        # folds into ITS OWN rail's boundary fault (Work[ArtifactReceipt] forbids an inner Result), and
        # the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
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
