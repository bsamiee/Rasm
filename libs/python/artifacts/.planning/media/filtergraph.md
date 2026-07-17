# [PY_ARTIFACTS_MEDIA_FILTERGRAPH]

Capability-detection filter-routing core of the media plane: the one closed `FilterNode` family over the verified `libavfilter` vocabulary, the ONE `wired` entrypoint — its FilterNode variant and source arity selecting the chain, multi-source, or dissolve arm — that routes each logical media operation to its native filter when the linked FFmpeg build exposes it AND wires cleanly in-process, else to a verified in-process substitute, and the `AudioGraph` capsule `media/audio#MEDIA` composes for every mastering and mixing chain. `av.filter.filters_available` is the single routing source of truth, read once per build, never a hardcoded assumption a filter exists. This page owns no container open and no encode; it is the reference impl `media/container#CONTAINER` `Transcode` composes for its in-line chain, `media/timeline#TIMELINE` for `Xfade`/`Concat`, and `media/audio#MEDIA` for `Amix` and the mastering sink, contributing the filter-node count onto the composing producer's receipt rather than a receipt of its own — routing substrate exactly as `visualization/diagram/glyphset#GLYPHSET` and `drawing/standard#STANDARD` mint none.

Substitute arms are the live path where a build lacks a filter or the filter cannot preserve the admitted payload: text burn-in renders through `pillow` `ImageDraw`+`ImageFont(Layout.RAQM)` to an RGBA plane composited by `Image.Image.alpha_composite`, while native `drawtext` admits only its structurally representable subset; subtitle burn is composite-ALWAYS — the native `subtitles`/`ass` filters read a FILENAME, not the in-memory event tuple, so the arm renders one plane per event text and gates each by the pulled frame's own presentation time — color grade derives `curves`+`hue` args from the eq knobs (native `eq`), and denoise routes to `nlmeans` (native `hqdn3d`). Wiring is equally load-bearing: single-input filters chain through `Graph.link_nodes`, but multi-input filters (`overlay`/`concat`/`amix`/`acrossfade`/`blend`) wire through `FilterContext.link_to(target, out_pad, in_pad)` because the `link_nodes` sequential form raises `ArgumentError 22` at `configure()` for every multi-input pad, and `xfade` fails `configure()` in-process under both forms so its only arm is the numpy `_cross_dissolve` over the overlapping frames. An audio graph closes on an explicit `abuffersink` node the `Graph.configure()` `auto_buffer` default omits (the `EINVAL` fix); source arity rides the case payload, never a `Filter(name).inputs` pad probe — a dynamic-input filter (`amix`) reports an empty static pad tuple. Every dynamic value entering a native filter string crosses `_escaped`, so no text can splice new options into the filter grammar. `FilterFault` reuses the `media/container#CONTAINER` `MediaFault` vocabulary — a filter absent from both the native registry and every substitute rails `MediaFault.unregistered`, never a `FilterNotFoundError` raised deep in the worker.

## [01]-[INDEX]

- [01]-[FILTER]: the closed `FilterNode` family over the logical media operations — `Scale`/`Crop`/`Fps`/`Format`/`ColorGrade`/`Denoise`/`Sharpen`/`Transpose`/`Pad`/`Deinterlace`/`Speed`/`TextBurn`/`SubtitleBurn`/`Xfade`/`Overlay`/`Concat`/`Amix`/`Acrossfade` — routed native-or-substitute by the `media_filters` probe through the one `wired` dispatcher, plus the `AudioGraph` mastering/mixing capsule.

## [02]-[FILTER]

- Owner: `FilterNode` the one closed `expression.tagged_union` over every logical media operation, never a per-filter class nor a stringly `(name, args)` tuple the caller hand-assembles — each case carries the typed knobs its filter reads and `_FILTER` carries its native name, substitute names, and route in one `FilterRow`, so a new operation is one case plus one policy row plus one `_wire` arm; `SubstituteKind` the closed routing vocabulary (`NATIVE_LINEAR`/`SUBSTITUTE_LINEAR`/`NATIVE_MULTI`/`COMPOSITE`/`DISSOLVE`) each row carries, so the builder reads routing off data rather than an `if` ladder; `WiredGraph` the frozen video build product carrying the ORDERED `stages` (each one configured native sub-graph or one Pillow composite pass at its own program position), the `driven` per-frame stage walk the container driver composes, and the `node_count` fact; `AudioGraphSpec` the closed mastering/mixing construction policy and `AudioGraph.of` its single capsule entry, with `frames` discriminating a linear frame iterator from an N-source iterator tuple and owning push/drain/flush; `FilterFault` the reused `media/container#CONTAINER` `MediaFault` vocabulary; `media_filters()` the `frozenset(av.filter.filters_available)` probe read once per build.
- Cases: the single-input transcode-chain ops `Scale`/`Crop`/`Fps`/`Format`/`Sharpen`/`Transpose`/`Pad`/`Deinterlace`/`Speed` (always-native: `scale`/`crop`/`fps`/`format`/`unsharp`/`transpose`/`pad`/`yadif`/`setpts`), `ColorGrade` (native `eq`, substitute the derived `curves`+`hue`), and `Denoise` (native `hqdn3d`, substitute `nlmeans`); the composite ops `TextBurn` (native `drawtext` only when its payload is representable, otherwise Pillow `alpha_composite`) and `SubtitleBurn` (composite-always: per-event planes gated by `frame.time`); the multi-source clip ops `Xfade` (the numpy `_cross_dissolve` only, since native `xfade` does not configure in-process), `Overlay` (native `overlay` via `link_to`, the second video source placed at `(x, y)` — watermark and picture-in-picture), `Concat` (native `concat` via `link_to`, or the timeline packet-concat), `Amix` (native `amix` via `link_to`), and `Acrossfade` (native `acrossfade` via `link_to`, the audio leg of a timeline transition) — one total `match` recovers the modality. `Scale`..`SubtitleBurn` ride `wired`'s chain arm, `Overlay`/`Concat`/`Amix`/`Acrossfade` its exact-arity multi-source arm, `Xfade` its dissolve arm — ONE entrypoint whose FilterNode variant and source arity discriminate the modality, never a builder-name suffix or a knob.
- Entry: `wired(program, source)` the ONE filter-build entrypoint every composer calls, its FilterNode variant and source arity selecting the arm — a node tuple + one template runs the single-source transcode chain `media/container#CONTAINER` `_transcode` composes (probe once, fold each contiguous native/substitute run through `_wire` into its own configured `link_nodes` sub-graph, land each `COMPOSITE` Pillow pass as its own stage at its program position, return the staged `WiredGraph` whose `driven` walk the driver folds per frame); one multi-source node + its source tuple builds the `link_to` explicit-pad graph `media/timeline#TIMELINE` composes — the arity proven EXACT before any buffer mints (two for `overlay`/`acrossfade`, the declared count for `concat`/`amix`; an underfill or surplus refuses), one `add_buffer`/`add_abuffer` per source onto the arity-correct sink; the `xfade` node + an (under, over) frame pair alpha-blends the overlapping `window` frames, clamped to both clips' extents. `AudioGraph.of(spec).frames(streams)` stays the one audio capsule `media/audio#MEDIA` composes, its input shape selecting linear mastering or N-source mixing while the driver owns the push/drain protocol and flushes each exhausted source immediately. Every arm runs inside the composing process-lane worker, never on the event loop, and rails a registry miss through the worker's `av.error.FFmpegError` capture onto `MediaFault.unregistered`.
- Auto: `_wire` reads `_FILTER[node.tag]` and appends the resolved arm — a `SUBSTITUTE_LINEAR` op the row's substitutes, a `COMPOSITE` op the pass `_composited` dispatches per case: `text_burn` the RGBA plane rendered through `_render_text` once, `subtitle_burn` the `_timed` pass whose per-event planes render once and composite only inside their `[start, end)` window; `_render_text` selects `ImageFont.Layout.RAQM` when `features.check("raqm")` confirms the HarfBuzz build else `Layout.BASIC`; `_grade_args` samples the eq luma transfer at the control points and formats the `curves` string paired with `hue=s=`, routing a CAM16/gamut-safe grade to `graphic/color/derive#DERIVE` rather than re-deriving color math here; `_escaped` backslash-escapes the filter-grammar metacharacters on every dynamic native-arg value.
- Receipt: this page mints NO `ArtifactReceipt` case of its own — it is routing substrate exactly as `visualization/diagram/glyphset#GLYPHSET` and `drawing/standard#STANDARD` mint none; `WiredGraph.node_count`/`AudioGraph.node_count` are the filter-node facts the composing producer folds onto its `core/receipt#RECEIPT` `ArtifactReceipt.Media` `facts` band (`{"filter_nodes": N}`), so the routing evidence rides the one shared media receipt beside the container's HDR/segment facts and the audio's LUFS facts. `Transcode`'s receipt carries `{"filter_nodes": ...}`, the `Xfade` receipt `{"dissolve_frames": window}`, and the `Amix` receipt `{"mix_inputs": count}` — three facts on one band.
- Growth: a new logical filter operation is one `FilterNode` case plus one `facet` alternative plus one `_FILTER` row plus one `_wire` arm; a new substitute for an existing op extends that row's `substitutes`; a new multi-source clip op is one case plus one `_link_clips` arm plus its `_arity` row; a new mastering/mixing shape is one `AudioGraphSpec` case plus one `AudioGraph.of` arm, never a second graph owner; a filter that gains in-process wiring changes one row from `DISSOLVE` to `NATIVE_MULTI` while retaining the substitute; a new build-capability gate is one probe read — every addition a case, row, or arm, never a hardcoded assumption.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from dataclasses import dataclass
from enum import StrEnum
from functools import cache, reduce
from itertools import zip_longest
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from beartype import beartype
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

lazy import av
lazy import av.filter
lazy from PIL import Image, ImageDraw, ImageFont, features  # the drawtext substitute; module-scope (a lazy stmt inside a function is a SyntaxError)

if TYPE_CHECKING:
    from collections.abc import Callable, Iterator

# --- [TYPES] ----------------------------------------------------------------------------

type FilterNodeTag = Literal[
    "scale", "crop", "fps", "format", "color_grade", "denoise", "sharpen", "transpose", "pad", "deinterlace", "speed",
    "text_burn", "subtitle_burn", "xfade", "overlay", "concat", "amix", "acrossfade",
]
type AudioGraphTag = Literal["master", "mix"]
type Rgba = NDArray[np.uint8]  # (H, W, 4) rendered text/subtitle plane
type SubtitleEvent = tuple[float, float, str]  # (start, end, text) — media/subtitle#SUBTITLE owns the pysubs2 parse


class SubstituteKind(StrEnum):
    NATIVE_LINEAR = "native_linear"  # single-input native filter into the link_nodes chain
    SUBSTITUTE_LINEAR = "substitute_linear"  # native absent -> other single-input native filters (eq -> curves+hue)
    NATIVE_MULTI = "native_multi"  # multi-input native filter via per-context link_to (concat/amix/acrossfade)
    COMPOSITE = "composite"  # Pillow alpha-composite pass (drawtext substitute, time-gated subtitle burn-in)
    DISSOLVE = "dissolve"  # native xfade refuses in-process configure -> numpy cross-dissolve


# --- [CONSTANTS] ------------------------------------------------------------------------

_CURVE_X: tuple[float, ...] = (0.0, 0.25, 0.5, 0.75, 1.0)  # eq-substitute luma-transfer sample points -> curves control string

# --- [MODELS] ---------------------------------------------------------------------------


class TextSpec(Struct, frozen=True):
    text: str
    font: str  # a font path the RAQM/BASIC layout loads
    size: int = 48
    x: int = 24
    y: int = 24
    color: tuple[int, int, int, int] = (255, 255, 255, 255)
    anchor: str = "la"
    stroke: int = 0
    stroke_fill: tuple[int, int, int, int] = (0, 0, 0, 255)
    features: tuple[str, ...] = ()  # OpenType features for the complex-script run
    language: str | None = None
    direction: str | None = None  # "rtl"/"ltr"/"ttb" for bidi/vertical


class FilterRow(Struct, frozen=True):
    name: str
    route: SubstituteKind
    substitutes: tuple[str, ...] = ()


_FILTER: frozendict[FilterNodeTag, FilterRow] = frozendict({
    "scale": FilterRow("scale", SubstituteKind.NATIVE_LINEAR),
    "crop": FilterRow("crop", SubstituteKind.NATIVE_LINEAR),
    "fps": FilterRow("fps", SubstituteKind.NATIVE_LINEAR),
    "format": FilterRow("format", SubstituteKind.NATIVE_LINEAR),
    "color_grade": FilterRow("eq", SubstituteKind.SUBSTITUTE_LINEAR, ("curves", "hue")),
    "denoise": FilterRow("hqdn3d", SubstituteKind.SUBSTITUTE_LINEAR, ("nlmeans",)),
    "sharpen": FilterRow("unsharp", SubstituteKind.NATIVE_LINEAR),
    "transpose": FilterRow("transpose", SubstituteKind.NATIVE_LINEAR),
    "pad": FilterRow("pad", SubstituteKind.NATIVE_LINEAR),
    "deinterlace": FilterRow("yadif", SubstituteKind.NATIVE_LINEAR),
    "speed": FilterRow("setpts", SubstituteKind.NATIVE_LINEAR),
    "text_burn": FilterRow("drawtext", SubstituteKind.COMPOSITE),
    "subtitle_burn": FilterRow("subtitles", SubstituteKind.COMPOSITE),
    "xfade": FilterRow("xfade", SubstituteKind.DISSOLVE),
    "overlay": FilterRow("overlay", SubstituteKind.NATIVE_MULTI),
    "concat": FilterRow("concat", SubstituteKind.NATIVE_MULTI),
    "amix": FilterRow("amix", SubstituteKind.NATIVE_MULTI),
    "acrossfade": FilterRow("acrossfade", SubstituteKind.NATIVE_MULTI),
})


@tagged_union(frozen=True)
class FilterNode:
    tag: FilterNodeTag = tag()
    scale: tuple[int, int] = case()
    crop: tuple[int, int, int, int] = case()  # (w, h, x, y)
    fps: int = case()
    format: str = case()  # pix_fmt
    color_grade: tuple[float, float, float, float] = case()  # (brightness, contrast, saturation, gamma)
    denoise: float = case()  # strength
    sharpen: float = case()  # unsharp luma amount
    transpose: int = case()  # 0=ccw+vflip, 1=cw, 2=ccw, 3=cw+vflip
    pad: tuple[int, int, int, int, str] = case()  # (w, h, x, y, color)
    deinterlace: int = case()  # yadif mode: 0 one frame per frame, 1 one frame per field
    speed: float = case()  # setpts PTS/factor — >1 speeds up, <1 slows down; the audio leg is a Stage.atempo
    text_burn: "TextSpec" = case()
    subtitle_burn: tuple[tuple[SubtitleEvent, ...], str] = case()  # (events, style)
    xfade: tuple[float, float, str] = case()  # (offset, duration, transition)
    overlay: tuple[int, int] = case()  # (x, y) top-left placement of the second video source — watermark/picture-in-picture
    concat: int = case()  # input count
    amix: tuple[int, tuple[float, ...]] = case()  # (count, weights)
    acrossfade: tuple[float, str] = case()  # (duration, curve) — exactly two audio sources

    def facet(self) -> tuple[str, object]:
        # projects the node-owned canonical identity — tag plus the case's plain-value payload, the stable
        # preimage chunk media/container#CONTAINER's `_canon` deterministic-msgpack-encodes in place of a repr.
        match self:
            case (
                FilterNode(tag="scale", scale=payload)
                | FilterNode(tag="crop", crop=payload)
                | FilterNode(tag="fps", fps=payload)
                | FilterNode(tag="format", format=payload)
                | FilterNode(tag="color_grade", color_grade=payload)
                | FilterNode(tag="denoise", denoise=payload)
                | FilterNode(tag="sharpen", sharpen=payload)
                | FilterNode(tag="transpose", transpose=payload)
                | FilterNode(tag="pad", pad=payload)
                | FilterNode(tag="deinterlace", deinterlace=payload)
                | FilterNode(tag="speed", speed=payload)
                | FilterNode(tag="text_burn", text_burn=payload)
                | FilterNode(tag="subtitle_burn", subtitle_burn=payload)
                | FilterNode(tag="xfade", xfade=payload)
                | FilterNode(tag="overlay", overlay=payload)
                | FilterNode(tag="concat", concat=payload)
                | FilterNode(tag="amix", amix=payload)
                | FilterNode(tag="acrossfade", acrossfade=payload)
            ):
                return (self.tag, payload)
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class AudioGraphSpec:
    tag: AudioGraphTag = tag()
    master: tuple[int, str, str, tuple[tuple[str, str], ...]] = case()
    mix: tuple[int, tuple[str, ...], str, tuple[float, ...]] = case()


@dataclass(frozen=True, slots=True)
class WiredGraph:
    # a frozen dataclass, NOT a msgspec.Struct: it carries live av.filter.Graph handles and closure passes (never
    # serialized), and deferred annotations keep the TYPE_CHECKING-only `Callable` from a class-creation NameError.
    # ordered STAGES preserve the declared FilterNode program: each stage is one configured native sub-graph or one
    # Pillow composite pass at its OWN program position — a composite before a later scale/crop/pad executes there
    # and targets the geometry present there, never a deferred tail pass over terminal extents.
    stages: tuple[tuple[str, object], ...] = ()  # ("native", av.filter.Graph) | ("composite", Callable[[frame], frame])
    node_count: int = 0  # the filter-node fact the producer folds onto ArtifactReceipt.Media

    def driven(self, frame: "object | None") -> tuple[object, ...]:
        # ONE ordered walk per frame: native stages push-and-drain, composite stages apply per frame — program
        # order IS execution order. The flush walk (frame None) forwards each stage's tail downstream AND flushes
        # every native stage in order, so a buffering filter's tail still crosses the later stages.
        flushing = frame is None
        pending: tuple[object, ...] = () if flushing else (frame,)
        for kind, stage in self.stages:
            if kind == "composite":
                pending = tuple(stage(item) for item in pending)
            else:
                drained = tuple(pulled for item in pending for pulled in _staged_pull(stage, item))
                pending = (*drained, *(_staged_pull(stage, None) if flushing else ()))
        return pending


@dataclass(frozen=True, slots=True)
class AudioGraph:
    # audio capsule media/audio#MEDIA composes for every filter chain — the live configured graph plus its
    # source contexts, since a multi-input graph pushes per FilterContext, never through the single-source Graph.push.
    graph: object
    sources: tuple[object, ...]
    node_count: int = 0  # the filter-node fact the composing producer folds onto ArtifactReceipt.Media

    @staticmethod
    def of(spec: AudioGraphSpec, /) -> "AudioGraph":
        graph = av.filter.Graph()
        match spec:
            case AudioGraphSpec(tag="master", master=(rate, ingest, layout, stages)):
                source = graph.add_abuffer(sample_rate=rate, format=ingest, layout=layout)
                linked = [source, *(graph.add(name, args) for name, args in stages), graph.add("abuffersink")]
                graph.link_nodes(*linked)
                graph.configure()
                return AudioGraph(graph=graph, sources=(source,), node_count=len(linked) - 2)
            case AudioGraphSpec(tag="mix", mix=(rate, ingests, layout, weights)):
                count = len(ingests)
                taps = tuple(graph.add_abuffer(sample_rate=rate, format=ingest, layout=layout) for ingest in ingests)
                args = f"inputs={count}" + (f":weights={' '.join(f'{weight:.3f}' for weight in weights)}" if weights else "")
                node, sink = graph.add("amix", args), graph.add("abuffersink")
                for index, tap in enumerate(taps):
                    tap.link_to(node, 0, index)
                node.link_to(sink, 0, 0)
                graph.configure()
                return AudioGraph(graph=graph, sources=taps, node_count=1)  # processing nodes only — amix; taps and sink are plumbing
            case _ as unreachable:
                assert_never(unreachable)

    def frames(self, subject: "Iterator[object] | tuple[Iterator[object], ...]", /) -> "Iterator[object]":
        match subject:
            case tuple() as streams:
                yield from self._mixed(streams)
            case stream:
                yield from self._filtered(stream)

    def _filtered(self, frames: "Iterator[object]", /) -> "Iterator[object]":
        # push/drain per frame, then the None flush drains a loudnorm/alimiter lookahead tail before EOF.
        for frame in frames:
            self.sources[0].push(frame)
            yield from self._pulled()
        self.sources[0].push(None)
        yield from self._pulled()

    def _mixed(self, streams: "tuple[Iterator[object], ...]", /) -> "Iterator[object]":
        ended = object()
        closed: set[int] = set()
        for group in zip_longest(*streams, fillvalue=ended):
            for index, (tap, frame) in enumerate(zip(self.sources, group, strict=True)):
                if frame is ended:
                    if index not in closed:
                        tap.push(None)
                        closed.add(index)
                else:
                    tap.push(frame)
            yield from self._pulled()
        for index, tap in enumerate(self.sources):
            if index not in closed:
                tap.push(None)
        yield from self._pulled()

    def _pulled(self) -> "Iterator[object]":
        while True:  # libavfilter pull protocol: drain until EAGAIN (needs input) or EOF (flushed)
            try:
                yield self.graph.pull()
            except BlockingIOError, EOFError:
                return


# --- [OPERATIONS] -----------------------------------------------------------------------


def media_filters() -> frozenset[str]:
    return frozenset(av.filter.filters_available)  # the routing probe, read once per build


def _escaped(value: str, /) -> str:
    # FFmpeg filter-arg grammar escape — backslash first, then the quote and option separator — so a dynamic text
    # value can never splice new options into a native filter string.
    return value.replace("\\", "\\\\").replace("'", "\\'").replace(":", "\\:")


def _grade_args(brightness: float, contrast: float, gamma: float, /) -> str:
    # sample the eq luma transfer at the control points and format the `curves` all-channel control string; the eq
    # substitute is `curves` (brightness/contrast/gamma) + `hue=s=` (saturation), both native-present where `eq` is absent.
    ys = np.clip((((np.asarray(_CURVE_X) - 0.5) * contrast) + 0.5 + brightness) ** (1.0 / max(gamma, 1e-3)), 0.0, 1.0)
    points = " ".join(f"{x:.3f}/{y:.3f}" for x, y in zip(_CURVE_X, ys, strict=True))
    return f"all='{points}'"


def _render_text(spec: TextSpec, width: int, height: int, /) -> Rgba:
    # drawtext substitute: a transparent RGBA plane drawn through Pillow, RAQM complex-script shaping gated on the
    # build probe (the same capability-detection shape one level down), returned as an (H, W, 4) uint8 array.
    layout = ImageFont.Layout.RAQM if features.check("raqm") else ImageFont.Layout.BASIC
    font = ImageFont.truetype(spec.font, spec.size, layout_engine=layout)
    canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    ImageDraw.Draw(canvas).text(
        (spec.x, spec.y),
        spec.text,
        font=font,
        fill=spec.color,
        anchor=spec.anchor,
        stroke_width=spec.stroke,
        stroke_fill=spec.stroke_fill,
        features=list(spec.features) or None,
        language=spec.language,
        direction=spec.direction,
    )
    return np.asarray(canvas)


def _composite(plane: Rgba, /) -> "Callable[[object], object]":
    overlay = Image.fromarray(plane, mode="RGBA")

    def blend(frame: object, /) -> object:
        base = Image.fromarray(frame.to_ndarray(format="rgb24"), mode="RGB").convert("RGBA")
        base.alpha_composite(overlay)
        composited = av.VideoFrame.from_image(base.convert("RGB"))
        composited.pts, composited.time_base = frame.pts, frame.time_base
        for field in ("color_primaries", "color_trc", "colorspace", "color_range"):
            if (value := getattr(frame, field, None)) is not None:
                setattr(composited, field, value)
        return composited

    return blend


def _timed(events: tuple[SubtitleEvent, ...], style: str, width: int, height: int, /) -> "Callable[[object], object]":
    # subtitle-burn composite: one plane per event text, rendered on the first frame inside its window and cached
    # for the rest — a long track never materializes every plane up front — gated by the pulled frame's own
    # presentation time; `style` names the substitute face path (the pysubs2 style resolution is media/subtitle#SUBTITLE's).
    banner = lambda text: TextSpec(text=text, font=style, size=max(height // 18, 16), x=width // 2, y=height - max(height // 12, 32), anchor="ms")

    @cache
    def rendered(index: int, /) -> "Callable[[object], object]":
        return _composite(_render_text(banner(events[index][2]), width, height))

    def blend(frame: object, /) -> object:
        live = (rendered(index) for index, (start, end, _text) in enumerate(events) if frame.time is not None and start <= frame.time < end)
        return reduce(lambda acc, pass_: pass_(acc), live, frame)

    return blend


def _composited(node: FilterNode, width: int, height: int, /) -> "Callable[[object], object]":
    # COMPOSITE dispatch: each case consumes only its own admitted payload — the static text plane, or the
    # time-gated event planes — never a cross-case attribute read.
    match node:
        case FilterNode(tag="text_burn", text_burn=spec):
            return _composite(_render_text(spec, width, height))
        case FilterNode(tag="subtitle_burn", subtitle_burn=(events, style)):
            return _timed(events, style, width, height)
        case _ as other:
            raise ValueError(f"composited: {other.tag} carries no composite arm")  # a programming defect, never railed as input


def _cross_dissolve(under: tuple[NDArray[np.uint8], ...], over: tuple[NDArray[np.uint8], ...], window: int, /) -> tuple[NDArray[np.uint8], ...]:
    # `wired` xfade arm over rgb24 frames (native `xfade` fails in-process `configure()`): the overlap clamps to
    # both clips' extents, under[-span:] fades out as over[:span] fades in, and a zero overlap degrades to a butt joint.
    span = max(0, min(window, len(under), len(over)))
    if span == 0:
        return (*under, *over)
    ramp = np.linspace(1.0, 0.0, span, dtype=np.float32)
    tail, head = under[-span:], over[:span]
    blended = tuple(
        (a.astype(np.float32) * w + b.astype(np.float32) * (1.0 - w)).astype(np.uint8)
        for a, b, w in zip(tail, head, ramp[:, None, None], strict=True)
    )
    return (*under[:-span], *blended, *over[span:])


def _arity(op: FilterNode, /) -> int:
    # names the exact source arity each multi-source op demands: two for the pairwise ops, the declared count for concat/amix.
    match op:
        case FilterNode(tag="overlay") | FilterNode(tag="acrossfade"):
            return 2
        case (FilterNode(tag="concat", concat=count) | FilterNode(tag="amix", amix=(count, _))) if count <= 0:
            # a zero or negative declared count would pass the exact-arity equality against an empty source tuple and
            # mint an invalid `n=0` graph spec; refused here so no buffer ever builds against it.
            raise ValueError(f"wired: {op.tag} declares a non-positive source count {count}")
        case FilterNode(tag="concat", concat=count) | FilterNode(tag="amix", amix=(count, _)):
            return count
        case _ as other:
            raise ValueError(f"wired: {other.tag} is not a multi-source op")  # a programming defect, never railed as input


def _link_clips(op: FilterNode, sources: tuple[object, ...], /) -> "av.filter.Graph":
    # `wired` multi-source arm for `overlay`/`concat`/`amix`/`acrossfade`: EXACT arity proven before any buffer
    # mints — an underfill or surplus refuses rather than silently slicing — then one buffer per source, the clip
    # node, each source wired through `link_to(node, 0, index)` explicit pads (link_nodes' sequential form raises
    # ArgumentError 22 for multi-input pads), closed on an explicit (a)buffersink.
    if len(sources) != (required := _arity(op)):
        raise ValueError(f"wired: {op.tag} takes exactly {required} sources, received {len(sources)}")
    if op.tag == "amix" and (weights := op.amix[1]) and (len(weights) != op.amix[0] or not np.isfinite(np.asarray(weights)).all()):
        # amix weight law proven before any graph or buffer mints: an omitted tuple takes the filter's equal-weight
        # default, a supplied one carries exactly one finite weight per declared input — a short, long, or
        # non-finite row would splice a malformed `weights=` option into the native spec.
        raise ValueError(f"wired: amix declares {op.amix[0]} inputs, weights {weights}")
    graph = av.filter.Graph()
    match op:
        case FilterNode(tag="overlay", overlay=(x, y)):
            buffers = tuple(graph.add_buffer(template=src) for src in sources)
            node, sink = graph.add("overlay", f"{x}:{y}"), graph.add("buffersink")
        case FilterNode(tag="concat", concat=count):
            buffers = tuple(graph.add_buffer(template=src) for src in sources)
            node, sink = graph.add("concat", f"n={count}:v=1:a=0"), graph.add("buffersink")
        case FilterNode(tag="amix", amix=(count, weights)):
            buffers = tuple(graph.add_abuffer(template=src) for src in sources)
            spec = f"inputs={count}" + (f":weights={' '.join(f'{w:.3f}' for w in weights)}" if weights else "")
            node, sink = graph.add("amix", spec), graph.add("abuffersink")
        case FilterNode(tag="acrossfade", acrossfade=(duration, curve)):
            buffers = tuple(graph.add_abuffer(template=src) for src in sources)
            node, sink = graph.add("acrossfade", f"d={duration}:c1={_escaped(curve)}:c2={_escaped(curve)}"), graph.add("abuffersink")
        case _ as other:
            raise ValueError(f"wired: {other.tag} is not a multi-source op")  # unreachable past _arity; kept as the defect witness
    for index, buffer in enumerate(buffers):
        buffer.link_to(node, 0, index)
    node.link_to(sink, 0, 0)
    graph.configure()
    return graph


def _extent(template: object, /) -> tuple[int, int]:
    return template.width, template.height


def _resized(node: FilterNode, extent: tuple[int, int], /) -> tuple[int, int]:
    # tracks the running output geometry a chain node imposes, so a composite plane appended AFTER a scale/crop/pad/transpose
    # renders at the graph's live extent rather than the source's — a mismatched plane fails alpha_composite outright.
    match node:
        case FilterNode(tag="scale", scale=(w, h)) | FilterNode(tag="crop", crop=(w, h, *_)) | FilterNode(tag="pad", pad=(w, h, *_)):
            return w, h
        case FilterNode(tag="transpose"):
            return extent[1], extent[0]
        case _:
            return extent


def _native_text(spec: TextSpec, /) -> bool:
    return spec.anchor == "lt" and not spec.features and spec.language is None and spec.direction is None


def _burn_args(spec: TextSpec, /) -> str:
    # native drawtext arg string: every dynamic value crosses `_escaped` so no caption can extend the filter
    # grammar, and `expansion=none` disarms drawtext's own %{...} function expansion — which runs AFTER unescape,
    # so escaping alone cannot stop a caption from evaluating expressions or reading metadata; the substitute
    # `_render_text` path takes the text directly with no grammar splice at all.
    color = f"0x{spec.color[0]:02x}{spec.color[1]:02x}{spec.color[2]:02x}@{spec.color[3] / 255:.3f}"
    stroke = f"0x{spec.stroke_fill[0]:02x}{spec.stroke_fill[1]:02x}{spec.stroke_fill[2]:02x}@{spec.stroke_fill[3] / 255:.3f}"
    return f"text='{_escaped(spec.text)}':expansion=none:fontfile='{_escaped(spec.font)}':fontsize={spec.size}:x={spec.x}:y={spec.y}:fontcolor={color}:borderw={spec.stroke}:bordercolor={stroke}"


def _wire(node: FilterNode, available: frozenset[str], /) -> tuple[tuple[str, str], ...]:
    # single-input node -> (filter, args) pairs: native when the build exposes it, else the derived substitute;
    # multi-source and composite ops carry no single-input chain node and fold to (), proven total by assert_never.
    row = _FILTER[node.tag]
    match node:
        case FilterNode(tag="scale", scale=(w, h)):
            return ((row.name, f"{w}:{h}"),)
        case FilterNode(tag="crop", crop=(w, h, x, y)):
            return ((row.name, f"{w}:{h}:{x}:{y}"),)
        case FilterNode(tag="fps", fps=rate):
            return ((row.name, f"fps={rate}"),)
        case FilterNode(tag="format", format=pix_fmt):
            return ((row.name, f"pix_fmts={_escaped(pix_fmt)}"),)
        case FilterNode(tag="color_grade", color_grade=(brightness, contrast, saturation, gamma)):
            return (
                ((row.name, f"brightness={brightness}:contrast={contrast}:saturation={saturation}:gamma={gamma}"),)
                if row.name in available
                else ((row.substitutes[0], _grade_args(brightness, contrast, gamma)), (row.substitutes[1], f"s={saturation}"))
            )
        case FilterNode(tag="denoise", denoise=strength):
            return ((row.name, f"{strength}"),) if row.name in available else ((row.substitutes[0], f"s={strength}"),)
        case FilterNode(tag="sharpen", sharpen=amount):
            return ((row.name, f"luma_amount={amount}"),)
        case FilterNode(tag="transpose", transpose=direction):
            return ((row.name, f"dir={direction}"),)
        case FilterNode(tag="pad", pad=(w, h, x, y, color)):
            return ((row.name, f"{w}:{h}:{x}:{y}:{_escaped(color)}"),)
        case FilterNode(tag="deinterlace", deinterlace=mode):
            return ((row.name, f"mode={mode}"),)
        case FilterNode(tag="speed", speed=factor):
            return ((row.name, f"PTS/{factor}"),)
        case (
            FilterNode(tag="text_burn")
            | FilterNode(tag="subtitle_burn")
            | FilterNode(tag="xfade")
            | FilterNode(tag="overlay")
            | FilterNode(tag="concat")
            | FilterNode(tag="amix")
            | FilterNode(tag="acrossfade")
        ):
            return ()
        case _ as unreachable:
            assert_never(unreachable)


@beartype
def _staged_pull(segment: object, frame: "object | None") -> tuple[object, ...]:
    # push-one-pull-all per native stage; a None push flushes the stage tail, and the EAGAIN/EOF stop conditions
    # end the pull inside the worker capture rather than leaking a provider raise.
    segment.push(frame)
    pulled: list[object] = []
    while True:
        try:
            pulled.append(segment.pull())
        except (av.error.BlockingIOError, av.error.EOFError):
            return tuple(pulled)


def _build_graph(nodes: tuple[FilterNode, ...], template: object, /) -> WiredGraph:
    # `wired` single-source transcode-chain arm: probe once, fold each contiguous native/substitute run into its
    # OWN configured link_nodes sub-graph, and land each Pillow composite as its own stage AT ITS PROGRAM POSITION
    # — a composite plane targets the geometry present at its own position, never the terminal extent.
    available = media_filters()
    stages: list[tuple[str, object]] = []
    pending: list[tuple[str, str]] = []
    extent = _extent(template)  # the running output geometry each stage's planes and buffers must match
    live_fmt: str = template.format.name
    segment_geometry: tuple[tuple[int, int], str] = (extent, live_fmt)
    node_count = 0

    def _closed_run() -> None:
        # close the pending native run into one configured sub-graph whose buffer carries the geometry the
        # PREVIOUS stage left; the first segment binds the source template directly.
        nonlocal pending, segment_geometry
        if not pending:
            segment_geometry = (extent, live_fmt)
            return
        segment = av.filter.Graph()
        (seg_extent, seg_fmt) = segment_geometry
        head = segment.add_buffer(template=template) if not stages else segment.add_buffer(width=seg_extent[0], height=seg_extent[1], format=seg_fmt)
        linked = [head, *(segment.add(spelled, args) for spelled, args in pending), segment.add("buffersink")]
        segment.link_nodes(*linked)
        segment.configure()
        stages.append(("native", segment))
        pending = []
        segment_geometry = (extent, live_fmt)

    for node in nodes:
        row = _FILTER[node.tag]
        match row.route:
            case SubstituteKind.NATIVE_LINEAR | SubstituteKind.SUBSTITUTE_LINEAR:
                for spelled, args in _wire(node, available):
                    # registry law: EVERY spelling — always-native rows and chosen substitutes alike — proves
                    # `filters_available` membership before Graph.add, so a limited build rails
                    # MediaFault.unregistered at the worker capture, never a deep FilterNotFoundError.
                    if spelled not in available:
                        raise av.FilterNotFoundError(38, f"filter '{spelled}' absent from this FFmpeg build")
                    pending.append((spelled, args))
                    node_count += 1
            case SubstituteKind.COMPOSITE if node.tag == "text_burn" and row.name in available and _native_text(node.text_burn):
                pending.append((row.name, _burn_args(node.text_burn)))
                node_count += 1
            case SubstituteKind.COMPOSITE:
                _closed_run()
                stages.append(("composite", _composited(node, *extent)))
                node_count += 1
            case SubstituteKind.NATIVE_MULTI | SubstituteKind.DISSOLVE:
                # a multi-source op inside a single-source program is a caller error — silently skipping it would
                # corrupt the declared program, so the chain arm refuses and routes the caller to `wired`'s
                # multi-source modality with the op's own source tuple.
                raise ValueError(f"wired: {node.tag} is multi-source — pass it to `wired` with its source tuple, not inside a chain program")
        extent = _resized(node, extent)
        live_fmt = node.format if node.tag == "format" else live_fmt
    _closed_run()
    return WiredGraph(stages=tuple(stages), node_count=node_count)


def wired(
    program: FilterNode | tuple[FilterNode, ...],
    source: "object | tuple[object, ...]",
    /,
    *,
    window: int = 0,
) -> "WiredGraph | av.filter.Graph | tuple[NDArray[np.uint8], ...]":
    # THE one filter-build entrypoint: the FilterNode variant and the source arity discriminate the modality,
    # never a builder-name suffix — a node program + one template runs the single-source transcode chain, one
    # NATIVE_MULTI node + its exact source tuple builds the link_to multi-source graph, and the `xfade` node +
    # an (under, over) frame pair runs the numpy dissolve whose frame `window` the caller derives from
    # rate * duration (the one payload the node cannot carry, since only the caller knows the frame rate).
    match program, source:
        case FilterNode(tag="xfade"), (tuple() as under, tuple() as over):
            return _cross_dissolve(under, over, window)
        case FilterNode() as clip, tuple() as sources:
            return _link_clips(clip, sources)
        case tuple() as nodes, template:
            return _build_graph(nodes, template)
        case _:
            raise ValueError("wired: program/source shapes match no modality")  # a programming defect, never railed as input
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
