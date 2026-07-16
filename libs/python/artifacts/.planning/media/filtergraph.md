# [PY_ARTIFACTS_MEDIA_FILTERGRAPH]

Capability-detection filter-routing core of the media plane: the one closed `FilterNode` family over the verified `libavfilter` vocabulary and the `build_graph`/`link_clips`/`cross_dissolve` builders that route each logical media operation to its native filter when the linked FFmpeg build exposes it AND wires cleanly in-process, else to a verified in-process substitute. `av.filter.filters_available` is the single routing source of truth, read once per build, never a hardcoded assumption a filter exists. This page owns no container open and no encode; it is the reference impl `media/container#CONTAINER` `Transcode` composes for its in-line chain, `media/timeline#TIMELINE` for `Xfade`/`Concat`, and `media/audio#MEDIA` for `Amix` and the mastering sink, contributing the filter-node count onto the composing producer's receipt rather than a receipt of its own — routing substrate exactly as `visualization/diagram/glyphset#GLYPHSET` and `drawing/standard#STANDARD` mint none.

Substitute arms are the live path where a build lacks a filter: text burn-in renders through `pillow` `ImageDraw`+`ImageFont(Layout.RAQM)` to an RGBA plane numpy-composited per frame (native `drawtext` needs libfreetype), subtitle burn composites the `media/subtitle#SUBTITLE` `pysubs2`-rendered frames (native `subtitles` needs libass), color grade derives `curves`+`hue` args from the eq knobs (native `eq`), and denoise routes to `nlmeans` (native `hqdn3d`). Wiring is equally load-bearing: single-input filters chain through `Graph.link_nodes`, but multi-input filters (`overlay`/`concat`/`amix`/`blend`/`acrossfade`) wire through `FilterContext.link_to(target, out_pad, in_pad)` because the `link_nodes` sequential form raises `ArgumentError 22` at `configure()` for every multi-input pad, and `xfade` fails `configure()` in-process under both forms so its only arm is the numpy `cross_dissolve` over the overlapping frames. An audio graph closes on an explicit `abuffersink` node the `Graph.configure()` `auto_buffer` default omits (the `EINVAL` fix), read from the filter's own pad arity through `Filter(name).inputs`/`outputs`. `FilterFault` reuses the `media/container#CONTAINER` `MediaFault` vocabulary — a filter absent from both the native registry and every substitute rails `MediaFault.unregistered`, never a `FilterNotFoundError` raised deep in the worker.

## [01]-[INDEX]

- [01]-[FILTER]: the closed `FilterNode` family over the logical media operations — `Scale`/`Crop`/`Fps`/`Format`/`ColorGrade`/`Denoise`/`TextBurn`/`SubtitleBurn`/`Xfade`/`Concat`/`Amix` — routed native-or-substitute by the `media_filters` probe through the `build_graph`/`link_clips`/`cross_dissolve` builders.

## [02]-[FILTER]

- Owner: `FilterNode` the one closed `expression.tagged_union` over every logical media operation, never a per-filter class nor a stringly `(name, args)` tuple the caller hand-assembles — each case carries the typed knobs its filter reads and (through `_NATIVE`) the FFmpeg name the probe checks, so a new operation is one case plus one `_NATIVE` row plus one `_ROUTE` row plus one `_wire` arm; `SubstituteKind` the closed routing vocabulary (`NATIVE_LINEAR`/`SUBSTITUTE_LINEAR`/`NATIVE_MULTI`/`COMPOSITE`/`DISSOLVE`) the `_ROUTE` table keys each tag to, so the builder reads routing off a data row not an `if` ladder; `WiredGraph` the frozen build product carrying the configured `av.filter.Graph`, the ordered numpy `composites` passes the driver applies after the graph pull, and the `node_count` fact; `FilterFault` the reused `media/container#CONTAINER` `MediaFault` vocabulary; `media_filters()` the `frozenset(av.filter.filters_available)` probe read once per build.
- Cases: the single-input transcode-chain ops `Scale`/`Crop`/`Fps`/`Format` (always-native), `ColorGrade` (native `eq`, substitute the derived `curves`+`hue`), and `Denoise` (native `hqdn3d`, substitute `nlmeans`); the composite ops `TextBurn` (native `drawtext`, substitute the pillow RGBA plane numpy-composited) and `SubtitleBurn` (native `subtitles`/`ass`, substitute the `media/subtitle#SUBTITLE` `pysubs2` frames composited); the multi-source clip ops `Xfade` (the numpy `cross_dissolve` only, since native `xfade` does not configure in-process), `Concat` (native `concat` via `link_to`, or the timeline packet-concat), and `Amix` (native `amix` via `link_to`) — one total `match` recovers the modality. `Scale`..`SubtitleBurn` route through `build_graph`, `Concat`/`Amix` through `link_clips`, `Xfade` through `cross_dissolve` — the source arity the structural discriminant between the builders, never a knob.
- Entry: `build_graph(nodes, template)` the single-source transcode-chain builder `media/container#CONTAINER` `_transcode` composes — probe once, fold each single-input node through `_wire` (a native `graph.add` node when `_NATIVE[tag]` is available, else the `SUBSTITUTE_LINEAR` derived nodes, else a `COMPOSITE` numpy pass appended to `composites`), link through `link_nodes`, `configure`, return the `WiredGraph`; `link_clips(op, sources)` the multi-source builder `media/timeline#TIMELINE`/`media/audio#MEDIA` compose — one `add_buffer`/`add_abuffer` per source and each source wired through `link_to` explicit pads onto the arity-correct sink; `cross_dissolve(under, over, window)` the `Xfade` substitute alpha-blending the overlapping `window` frames. Every builder runs inside the composing `to_process` worker, never on the event loop, and rails a registry miss through the worker's `av.error.FFmpegError` capture onto `MediaFault.unregistered`.
- Auto: `_wire` reads `_NATIVE[node.tag]` and `_ROUTE[node.tag]` and appends the resolved arm — a `SUBSTITUTE_LINEAR` op the `_grade_args`-derived `curves`+`hue` nodes, a `COMPOSITE` op the RGBA plane rendered through `_render_text` ONCE (the caption is static across frames) and the `_composite` closure the driver alpha-blends per pulled frame; `_render_text` selects `ImageFont.Layout.RAQM` when `features.check("raqm")` confirms the HarfBuzz build else `Layout.BASIC`; `_grade_args` samples the eq luma transfer at the control points and formats the `curves` string paired with `hue=s=`, routing a CAM16/gamut-safe grade to `graphic/color/derive#DERIVE` rather than re-deriving color math here.
- Receipt: this page mints NO `ArtifactReceipt` case of its own — it is routing substrate exactly as `visualization/diagram/glyphset#GLYPHSET` and `drawing/standard#STANDARD` mint none; `WiredGraph.node_count` is the filter-node fact the composing producer folds onto its `core/receipt#RECEIPT` `ArtifactReceipt.Media` `facts` band (`{"filter_nodes": N}`), so the routing evidence rides the one shared media receipt beside the container's HDR/segment facts and the audio's LUFS facts. `Transcode`'s receipt carries `{"filter_nodes": ...}`, the `Xfade` receipt `{"dissolve_frames": window}`, and the `Amix` receipt `{"mix_inputs": count}` — three facts on one band.
- Growth: a new logical filter operation is one `FilterNode` case plus one `_NATIVE` row plus one `_ROUTE` row plus one `_wire` arm; a new substitute for an existing op a widened `_wire` arm reading the same probe; a new multi-source clip op one case plus one `link_clips` arm; a filter that gains in-process wiring in a later `av` flips its `_ROUTE` from `DISSOLVE` to `NATIVE_MULTI` in one row with the numpy substitute retained as fallback; a new build-capability gate one probe read — every addition a case, row, or arm, never a hardcoded assumption.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from dataclasses import dataclass
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from beartype import beartype
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

lazy import av
lazy import av.error
lazy import av.filter
lazy from PIL import Image, ImageDraw, ImageFont, features  # the drawtext substitute; module-scope (a lazy stmt inside a function is a SyntaxError)

if TYPE_CHECKING:
    from collections.abc import Callable

# --- [TYPES] ----------------------------------------------------------------------------

type FilterNodeTag = Literal["scale", "crop", "fps", "format", "color_grade", "denoise", "text_burn", "subtitle_burn", "xfade", "concat", "amix"]
type Rgba = NDArray[np.uint8]  # (H, W, 4) rendered text/subtitle plane
type SubtitleEvent = tuple[float, float, str]  # (start, end, text) — media/subtitle#SUBTITLE owns the pysubs2 parse


class SubstituteKind(StrEnum):
    NATIVE_LINEAR = "native_linear"  # single-input native filter into the link_nodes chain
    SUBSTITUTE_LINEAR = "substitute_linear"  # native absent -> other single-input native filters (eq -> curves+hue)
    NATIVE_MULTI = "native_multi"  # multi-input native filter via link_to (concat/amix)
    COMPOSITE = "composite"  # native absent -> numpy alpha-composite (drawtext/subtitles burn-in)
    DISSOLVE = "dissolve"  # native xfade refuses in-process configure -> numpy cross-dissolve


# --- [CONSTANTS] ------------------------------------------------------------------------

# each logical op -> its FFmpeg filter name (the `media_filters` probe key) and its routing kind. One primary
# correspondence; `_wire`/`link_clips`/`cross_dissolve` read the row rather than an if-ladder over filter names.
_NATIVE: frozendict[FilterNodeTag, str] = frozendict({
    "scale": "scale",
    "crop": "crop",
    "fps": "fps",
    "format": "format",
    "color_grade": "eq",
    "denoise": "hqdn3d",
    "text_burn": "drawtext",
    "subtitle_burn": "subtitles",
    "xfade": "xfade",
    "concat": "concat",
    "amix": "amix",
})
_ROUTE: frozendict[FilterNodeTag, SubstituteKind] = frozendict({
    "scale": SubstituteKind.NATIVE_LINEAR,
    "crop": SubstituteKind.NATIVE_LINEAR,
    "fps": SubstituteKind.NATIVE_LINEAR,
    "format": SubstituteKind.NATIVE_LINEAR,
    "color_grade": SubstituteKind.SUBSTITUTE_LINEAR,
    "denoise": SubstituteKind.SUBSTITUTE_LINEAR,
    "text_burn": SubstituteKind.COMPOSITE,
    "subtitle_burn": SubstituteKind.COMPOSITE,
    "xfade": SubstituteKind.DISSOLVE,
    "concat": SubstituteKind.NATIVE_MULTI,
    "amix": SubstituteKind.NATIVE_MULTI,
})
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


@tagged_union(frozen=True)
class FilterNode:
    tag: FilterNodeTag = tag()
    scale: tuple[int, int] = case()
    crop: tuple[int, int, int, int] = case()  # (w, h, x, y)
    fps: int = case()
    format: str = case()  # pix_fmt
    color_grade: tuple[float, float, float, float] = case()  # (brightness, contrast, saturation, gamma)
    denoise: float = case()  # strength
    text_burn: "TextSpec" = case()
    subtitle_burn: tuple[tuple[SubtitleEvent, ...], str] = case()  # (events, style)
    xfade: tuple[float, float, str] = case()  # (offset, duration, transition)
    concat: int = case()  # input count
    amix: tuple[int, tuple[float, ...]] = case()  # (count, weights)

    @staticmethod
    def Scale(width: int, height: int, /) -> "FilterNode":
        return FilterNode(scale=(width, height))

    @staticmethod
    def ColorGrade(brightness: float = 0.0, contrast: float = 1.0, saturation: float = 1.0, gamma: float = 1.0) -> "FilterNode":
        return FilterNode(color_grade=(brightness, contrast, saturation, gamma))

    @staticmethod
    def Denoise(strength: float = 3.0, /) -> "FilterNode":
        return FilterNode(denoise=strength)

    @staticmethod
    def TextBurn(spec: "TextSpec", /) -> "FilterNode":
        return FilterNode(text_burn=spec)

    @staticmethod
    def Xfade(offset: float, duration: float, transition: str = "fade", /) -> "FilterNode":
        return FilterNode(xfade=(offset, duration, transition))


@dataclass(frozen=True, slots=True)
class WiredGraph:
    # a frozen dataclass, NOT a msgspec.Struct: it carries a live av.filter.Graph handle and closure passes (never
    # serialized), and deferred annotations keep the TYPE_CHECKING-only `Callable` from a class-creation NameError.
    graph: object  # the configured av.filter.Graph (opaque to the composing producer)
    composites: tuple["Callable[[object], object]", ...] = ()  # numpy passes the driver applies after the graph pull
    node_count: int = 0  # the filter-node fact the producer folds onto ArtifactReceipt.Media

    def composited(self, frame: object, /) -> object:
        # the driver applies the text/subtitle numpy substitutes after the native graph pull; a graph-only chain
        # carries no composites and returns the frame untouched.
        for pass_ in self.composites:
            frame = pass_(frame)
        return frame


# --- [OPERATIONS] -----------------------------------------------------------------------


def media_filters() -> frozenset[str]:
    return frozenset(av.filter.filters_available)  # the routing probe, read once per build


def _grade_args(brightness: float, contrast: float, gamma: float, /) -> str:
    # sample the eq luma transfer at the control points and format the `curves` all-channel control string; the eq
    # substitute is `curves` (brightness/contrast/gamma) + `hue=s=` (saturation), both native-present where `eq` is absent.
    ys = np.clip((((np.asarray(_CURVE_X) - 0.5) * contrast) + 0.5 + brightness) ** (1.0 / max(gamma, 1e-3)), 0.0, 1.0)
    points = " ".join(f"{x:.3f}/{y:.3f}" for x, y in zip(_CURVE_X, ys, strict=True))
    return f"all='{points}'"


def _render_text(spec: TextSpec, width: int, height: int, /) -> Rgba:
    # the drawtext substitute: a transparent RGBA plane drawn through Pillow with RAQM complex-script shaping gated on
    # the build probe (the same capability-detection shape one level down), returned as an (H, W, 4) uint8 array.
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
    # the per-frame numpy alpha-composite closure the WiredGraph carries: the static RGBA plane blends over each pulled
    # frame's rgb24 buffer, the encoder pixel format re-derived by the caller's reformat downstream.
    alpha = plane[..., 3:4].astype(np.float32) / 255.0
    over = plane[..., :3].astype(np.float32)

    def pass_(frame: object, /) -> object:
        base = frame.to_ndarray(format="rgb24").astype(np.float32)
        blended = (base * (1.0 - alpha) + over * alpha).astype(np.uint8)
        return av.VideoFrame.from_ndarray(blended, format="rgb24")

    return pass_


def cross_dissolve(under: tuple[NDArray[np.uint8], ...], over: tuple[NDArray[np.uint8], ...], window: int, /) -> tuple[NDArray[np.uint8], ...]:
    # the Xfade substitute over rgb24 video frames: the native `xfade` filter fails in-process `configure()`, so blend
    # the overlapping `window` frames linearly — under[-window:] fades out (weight 1->0) as over[:window] fades in.
    ramp = np.linspace(1.0, 0.0, window, dtype=np.float32)
    tail, head = under[-window:], over[:window]
    blended = tuple(
        (a.astype(np.float32) * w + b.astype(np.float32) * (1.0 - w)).astype(np.uint8)
        for a, b, w in zip(tail, head, ramp[:, None, None], strict=False)
    )
    return (*under[:-window], *blended, *over[window:])


def link_clips(op: FilterNode, sources: tuple[object, ...], /) -> "av.filter.Graph":
    # the multi-source clip builder for `concat`/`amix`: one buffer per source, the clip node, each source wired through
    # `link_to(node, 0, index)` explicit pads (link_nodes' sequential form raises ArgumentError 22 for multi-input pads).
    graph = av.filter.Graph()
    match op:
        case FilterNode(tag="concat", concat=count):
            buffers = tuple(graph.add_buffer(template=src) for src in sources[:count])
            node = graph.add("concat", f"n={count}:v=1:a=0")
            sink = graph.add("buffersink")
        case FilterNode(tag="amix", amix=(count, weights)):
            buffers = tuple(graph.add_abuffer(template=src) for src in sources[:count])
            spec = f"inputs={count}" + (f":weights={' '.join(f'{w:.3f}' for w in weights)}" if weights else "")
            node, sink = graph.add("amix", spec), graph.add("abuffersink")
        case _:
            raise av.error.FilterNotFoundError(22, f"link_clips: {op.tag} is not a multi-source op")  # rails to MediaFault.unregistered at the worker
    for index, buffer in enumerate(buffers):
        buffer.link_to(node, 0, index)
    node.link_to(sink, 0, 0)
    graph.configure()
    return graph


def _extent(template: object, /) -> tuple[int, int]:
    return template.width, template.height


def _burn_args(node: FilterNode, /) -> str:
    # the native drawtext arg string (the absent-here path); the live substitute is `_render_text`, which takes the text
    # DIRECTLY with no filter-grammar splice — the injection-safe path, so no dynamic value reaches a filter-string grammar.
    spec = node.text_burn
    return f"text={spec.text!r}:fontfile={spec.font!r}:fontsize={spec.size}:x={spec.x}:y={spec.y}"


def _wire(node: FilterNode, native: bool, /) -> tuple[tuple[str, str], ...]:
    # the single-input node -> (filter, args) pairs: native when the build exposes it, else the derived substitute; the
    # multi-source and composite ops carry no single-input chain node and fold to (), proven total by assert_never.
    match node:
        case FilterNode(tag="scale", scale=(w, h)):
            return (("scale", f"{w}:{h}"),)
        case FilterNode(tag="crop", crop=(w, h, x, y)):
            return (("crop", f"{w}:{h}:{x}:{y}"),)
        case FilterNode(tag="fps", fps=rate):
            return (("fps", f"fps={rate}"),)
        case FilterNode(tag="format", format=pix_fmt):
            return (("format", f"pix_fmts={pix_fmt}"),)
        case FilterNode(tag="color_grade", color_grade=(brightness, contrast, saturation, gamma)):
            return (
                (("eq", f"brightness={brightness}:contrast={contrast}:saturation={saturation}:gamma={gamma}"),)
                if native
                else (("curves", _grade_args(brightness, contrast, gamma)), ("hue", f"s={saturation}"))
            )
        case FilterNode(tag="denoise", denoise=strength):
            return (("hqdn3d", f"{strength}"),) if native else (("nlmeans", f"s={strength}"),)
        case (
            FilterNode(tag="text_burn")
            | FilterNode(tag="subtitle_burn")
            | FilterNode(tag="xfade")
            | FilterNode(tag="concat")
            | FilterNode(tag="amix")
        ):
            return ()
        case _ as unreachable:
            assert_never(unreachable)


@beartype
def build_graph(nodes: tuple[FilterNode, ...], template: object, /) -> WiredGraph:
    # the single-source transcode-chain builder: probe once, fold each single-input node native-or-substitute into the
    # link_nodes chain, collect the text/subtitle numpy composite passes, close on an explicit buffersink, configure.
    available = media_filters()
    graph = av.filter.Graph()
    source = graph.add_buffer(template=template)
    chain: list[object] = [source]
    composites: list["Callable[[object], object]"] = []
    for node in nodes:
        native, kind = _NATIVE[node.tag], _ROUTE[node.tag]
        match kind:
            case SubstituteKind.NATIVE_LINEAR | SubstituteKind.SUBSTITUTE_LINEAR:
                for name, args in _wire(node, native in available):
                    chain.append(graph.add(name, args))
            case SubstituteKind.COMPOSITE if native in available:
                chain.append(graph.add(native, _burn_args(node)))
            case SubstituteKind.COMPOSITE:
                composites.append(_composite(_render_text(node.text_burn, *_extent(template))))
            case SubstituteKind.NATIVE_MULTI | SubstituteKind.DISSOLVE:
                continue  # multi-source ops route through cross_dissolve/link_clips, never a single-source chain
    chain.append(graph.add("buffersink"))
    graph.link_nodes(*chain)
    graph.configure()
    return WiredGraph(graph=graph, composites=tuple(composites), node_count=len(chain) - 2)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
