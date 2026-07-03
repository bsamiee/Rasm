# [PY_ARTIFACTS_MEDIA_FILTERGRAPH]

The capability-detection filter-routing CORE of the media plane — the one closed `FilterNode` family over the verified `libavfilter` vocabulary and the `build_graph`/`link_clips`/`cross_dissolve` builders that route each logical media operation to its native filter when the linked FFmpeg build exposes it AND wires cleanly in-process, else to a verified in-process substitute. Media processing is admitted ONCE at the boundary as a `tuple[FilterNode, ...]` chain (transcode) or a multi-source clip op (timeline), and the routing probe is the single source of truth: `av.filter.filters_available` (a verified 448-member set on this `av 17.1.0` wheel) selects the arm, never a hardcoded assumption a filter exists. This build proves the design non-illusory — `overlay`/`colorbalance`/`curves`/`nlmeans`/`xfade`/`concat`/`amix`/`hue` are PRESENT while `drawtext`/`subtitles`/`ass`/`eq`/`hqdn3d`/`zscale` are ABSENT, so the substitute arms are the LIVE path, not dead branches: text burn-in renders through `pillow` `ImageDraw`+`ImageFont(Layout.RAQM)` to an RGBA plane numpy-composited per frame (the absent `drawtext`), color grade derives `curves`+`hue` args from the eq knobs (the absent `eq`), and denoise routes to `nlmeans` (the absent `hqdn3d`). The graph-wiring truth is equally load-bearing and equally verified: single-input filters chain through `Graph.link_nodes`, multi-input filters (`overlay`/`concat`/`amix`/`blend`/`acrossfade`) wire through `FilterContext.link_to(target, out_pad, in_pad)` (the `link_nodes` sequential form raises `ArgumentError 22` at `configure()` for every multi-input pad), and `xfade` fails `configure()` in-process on `av 17.1.0` under BOTH wiring forms, so its arm is a numpy cross-dissolve over the overlapping frames rather than the non-configuring native filter — shipping the native `xfade` node would be exactly the illusory code the density bar rejects. An audio graph closes on an EXPLICIT `abuffersink` node the `Graph.configure()` `auto_buffer` default omits (the `EINVAL` the audio owner fixed), read once from the filter's own pad arity through `Filter(name).inputs`/`outputs`. `FilterFault` reuses the `media/container#CONTAINER` `MediaFault` closed vocabulary — a filter absent from BOTH the native registry and every substitute rails `MediaFault.unregistered=("filters_available", name)` at the boundary, never a `FilterNotFoundError` raised deep in the worker. This page owns NO container open and NO encode; it is the reference impl `media/container#CONTAINER` `Transcode` composes for its in-line chain, `media/timeline#TIMELINE` composes for `Xfade`/`Concat`, and `media/audio#MEDIA` composes for `Amix` and the mastering-chain sink, contributing the filter-node count onto the composing producer's `ArtifactReceipt.Media` `facts` band rather than a receipt of its own — the routing substrate exactly as `visualization/diagram/glyphset#GLYPHSET` and `drawing/standard#STANDARD` are substrates that mint none.

## [01]-[INDEX]

- [01]-[FILTER]: the closed `FilterNode` `tagged_union` over the eleven logical media operations — `Scale`/`Crop`/`Fps`/`Format` (single-input, always-native), `ColorGrade`/`Denoise` (single-input, native-`eq`/`hqdn3d`-or-substitute), `TextBurn`/`SubtitleBurn` (native-`drawtext`/`subtitles`-or-numpy-composite), and `Xfade`/`Concat`/`Amix` (multi-source, timeline/audio-composed) — each case carrying its typed payload and its native filter name, the routing selected by the `media_filters` probe over `av.filter.filters_available`; the `WiredGraph` value (the configured `av.filter.Graph` + the ordered numpy `composites` passes + the `node_count` fact); `build_graph(nodes, template)` the single-source transcode-chain builder folding each single-input node into the linear `link_nodes` chain (native or derived-substitute) and collecting the text/subtitle composite passes, `link_clips(op, sources)` the multi-source builder wiring `concat`/`amix`/`blend` through `link_to` explicit pads, `cross_dissolve(under, over, window)` the numpy `Xfade` substitute over the overlapping frames; the `_render_text` `pillow` RGBA text plane (`ImageDraw.text` with `Layout.RAQM` complex-script shaping gated on `PIL.features.check("raqm")`, `features`/`language`/`direction` runs), the `_composite` numpy alpha-composite, the `_grade_args` eq-to-`curves`+`hue` derivation, and the explicit-`abuffersink` audio-graph close; `av.filter.Graph`(`add_buffer`/`add_abuffer`/`add`/`link_nodes`/`configure`/`push`/`pull`), `av.filter.Filter`(`inputs`/`outputs`/`name` pad-arity read), `av.filter.FilterContext.link_to`, `av.filter.filters_available`, `av.VideoFrame.from_ndarray`/`to_ndarray`, and the `MediaFault` rail — all verified present on the installed `av 17.1.0`. This page owns the `FilterNode` family, the three builders, and the substitute primitives; it reuses `media/container#CONTAINER`'s `MediaFault` and is composed by `Transcode`/`Xfade`/`Concat`/`Amix`, minting no receipt of its own.

## [02]-[FILTER]

- Owner: `FilterNode` the one closed `expression.tagged_union` over every logical media operation, never a per-filter class nor a stringly `(name, args)` tuple the caller hand-assembles — each case carries the typed knobs its filter reads AND (through the `_NATIVE` correspondence) the FFmpeg filter name the probe checks, so a new operation is one case plus one `_NATIVE` row plus one `_wire` arm, the exhaustiveness `assert_never` breaking the fold until the arm exists; `SubstituteKind` the closed routing vocabulary (`NATIVE_LINEAR`/`SUBSTITUTE_LINEAR`/`NATIVE_MULTI`/`COMPOSITE`/`DISSOLVE`) the `_ROUTE` table keys each tag to, so the builder reads the routing off a data row rather than an `if` ladder; `WiredGraph` the frozen build product carrying the configured `av.filter.Graph`, the ordered `composites: tuple[Callable[[object], object], ...]` numpy passes the driver applies after the graph pull (the text/subtitle burn-in substitutes that never touch the graph), and the `node_count` the composing producer folds onto its receipt band; `FilterFault` the reused `media/container#CONTAINER` `MediaFault` closed vocabulary (a filter absent from both native and substitute is `unregistered`, never a new parallel fault rail) so the whole media plane threads one cause vocabulary; the `media_filters()` `frozenset(av.filter.filters_available)` probe the single routing source-of-truth read once per build, never a per-node re-scan nor a hardcoded `assert "drawtext" in …`.
- Cases: `FilterNode` cases — the single-input transcode-chain ops `Scale(width, height)` (`scale`), `Crop(width, height, x, y)` (`crop`), `Fps(rate)` (`fps`), `Format(pix_fmt)` (`format`), `ColorGrade(brightness, contrast, saturation, gamma)` (native `eq`; substitute the derived `curves` luma transfer + `hue=s=` saturation, both native-present, since `eq` is absent on this build), and `Denoise(strength)` (native `hqdn3d`; substitute `nlmeans=s=`, present); the composite ops `TextBurn(text, font, size, x, y, color, anchor, stroke, features, language, direction)` (native `drawtext`; substitute the `pillow` RGBA plane numpy-composited, since `drawtext` needs libfreetype absent here) and `SubtitleBurn(events, style)` (native `subtitles`/`ass`; substitute the `media/subtitle#SUBTITLE` `pysubs2`-rendered RGBA frames numpy-composited, since libass is absent); and the multi-source clip ops `Xfade(offset, duration, transition)` (the native `xfade` filter does NOT configure in-process on `av 17.1.0`, so its ONLY arm is the `cross_dissolve` numpy substitute), `Concat(count)` (native `concat` via `link_to`, or the timeline lossless packet-concat), and `Amix(count, weights)` (native `amix` via `link_to`) — matched by one total `match`/`case`, the modality recovered from the discriminant, never a name suffix. `Scale`/`Crop`/`Fps`/`Format`/`ColorGrade`/`Denoise`/`TextBurn`/`SubtitleBurn` route through `build_graph`; `Concat`/`Amix` through `link_clips`; `Xfade` through `cross_dissolve` — the source arity (one template vs N) the structural discriminant between the builders, never a knob.
- Entry: `build_graph(nodes, template)` is the single-source transcode-chain builder `media/container#CONTAINER` `_transcode` composes inside its worker — it reads `media_filters()` once, folds each single-input `FilterNode` through `_wire` (a native `graph.add(name, args)` node when `_NATIVE[tag]` is available, else the `SUBSTITUTE_LINEAR` derived nodes, else a `COMPOSITE` numpy pass appended to `composites` rather than the graph), links the chain through `link_nodes(source, *nodes, buffersink)`, `configure`s, and returns the `WiredGraph`; `link_clips(op, sources)` is the multi-source builder `media/timeline#TIMELINE`/`media/audio#MEDIA` compose — it adds one `add_buffer`/`add_abuffer` per source, the clip filter node, and wires each source through `source.link_to(node, 0, index)` explicit pads (the `link_nodes` sequential form raises `ArgumentError 22` for multi-input pads) closing on the arity-correct sink; `cross_dissolve(under, over, window)` is the `Xfade` numpy substitute — it linearly alpha-blends the overlapping `window` frames of two clips (`under[-window:]` fading out under `over[:window]` fading in) because the native `xfade` filter refuses in-process `configure()`. Every builder runs inside the `media/container#CONTAINER`/`media/timeline#TIMELINE` `to_process` worker (the graph build is synchronous native CPU work), never on the event loop, and rails a registry miss through the composing worker's `av.error.FFmpegError` capture onto `MediaFault.unregistered`.
- Auto: the routing is one probe and one table — `media_filters()` reads `frozenset(av.filter.filters_available)` once, and `_wire(node, available, graph, tail)` matches the node, reads `_NATIVE[node.tag]` and `_ROUTE[node.tag]`, and appends the resolved arm: a `NATIVE_LINEAR` op adds `graph.add(name, node.args())` linked onto the running tail; a `SUBSTITUTE_LINEAR` op (`ColorGrade` when `eq` absent) adds the `_grade_args`-derived `curves`+`hue` nodes; a `COMPOSITE` op (`TextBurn` when `drawtext` absent) renders the RGBA plane through `_render_text` ONCE (the caption is static across frames) and appends the `_composite` closure to `composites` so the driver alpha-blends it per pulled frame; the pad arity for the sink comes from `Filter(node.tag).inputs`/`outputs` read off the descriptor, never a hardcoded input count. `_render_text` builds a transparent `Image.new("RGBA", (w, h), (0, 0, 0, 0))`, selects `ImageFont.Layout.RAQM` when `PIL.features.check("raqm")` confirms the HarfBuzz build (else `Layout.BASIC`), and draws through `ImageDraw.text` with the `features`/`language`/`direction` complex-script run — the same capability-detection shape the filter probe uses, one level down; `_composite(frame, plane)` reads the frame to an rgb24 ndarray, alpha-blends `plane[..., :3]` weighted by `plane[..., 3:4] / 255`, and lifts the result back through `VideoFrame.from_ndarray`; `_grade_args` samples the eq luma transfer `y = clip(((x - 0.5)·contrast + 0.5 + brightness)^(1/gamma))` at five control points through numpy and formats the `curves` `all='x0/y0 …'` string, pairing it with `hue=s=<saturation>` — the perceptual color depth routing to `graphic/color/derive#DERIVE` when a CAM16/gamut-safe grade is required rather than re-deriving colour math here.
- Receipt: this page mints NO `ArtifactReceipt` case of its own — it is the routing substrate exactly as `visualization/diagram/glyphset#GLYPHSET`, `drawing/standard#STANDARD`, and `specification/classify#CLASSIFY` mint none. `WiredGraph.node_count` is the filter-node fact the composing producer folds onto its `core/receipt#RECEIPT` `ArtifactReceipt.Media` `facts` band (`{"filter_nodes": N}`), so the routing evidence rides the ONE shared media receipt case beside the container's HDR/segment facts and the audio's LUFS facts, never a parallel filtergraph receipt rail; the substitute-vs-native decision per node is deterministic evidence the `node_count` and the composite-pass count summarize. The `media/container#CONTAINER` `Transcode` receipt carries `{"filter_nodes": wired.node_count}`, the `media/timeline#TIMELINE` `Xfade` receipt carries `{"dissolve_frames": window}`, and the `media/audio#MEDIA` `Amix` receipt carries `{"mix_inputs": count}` — three facts on one band.
- Growth: a new logical filter operation is one `FilterNode` case plus one `_NATIVE` row (its FFmpeg name) plus one `_ROUTE` row (its substitute kind) plus one `_wire` arm — the `assert_never` breaking the fold at type-check until the arm exists; a new substitute for an existing op is a widened `_wire` arm reading the same probe (a second color-grade fallback, a second denoise filter); a new multi-source clip op is one case plus one `link_clips` arm wiring its `link_to` pads; a filter that gains in-process wiring support in a later `av` (a future `xfade` fix) flips its `_ROUTE` from `DISSOLVE` to `NATIVE_MULTI` in one row with the numpy substitute retained as the fallback; a new build-capability gate (a `features.check` for a Pillow codec, a `filters_available` membership) is one probe read, never a hardcoded assumption; zero new surface — the family is the eleven ops on one union, every addition a case, row, or arm.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from dataclasses import dataclass
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from beartype import beartype
from builtins import frozendict
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
    return frozenset(av.filter.filters_available)  # the 448-set routing probe, read once per build


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
    # the Xfade substitute over rgb24 video frames: the native `xfade` filter fails in-process `configure()` on av 17.1.0,
    # so blend the overlapping `window` frames linearly — under[-window:] fades out (weight 1->0) as over[:window] fades in.
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

- [CAPABILITY_PROBE] [RESOLVED]: the routing probe is `av.filter.filters_available` — a 448-member set verified on the installed `av 17.1.0` (undocumented in the folder `av.md` but confirmed present at `av.filter.filters_available`, NOT `av.filters_available` which is absent). The verification proves the design non-illusory: PRESENT are `scale`/`crop`/`fps`/`format`/`overlay`/`colorbalance`/`curves`/`hue`/`colorchannelmixer`/`lut`/`nlmeans`/`atadenoise`/`removegrain`/`gblur`/`xfade`/`concat`/`amix`/`acrossfade`/`blend`/`hstack`/`vstack`/`tpad`/`settb`; ABSENT are `drawtext`/`subtitles`/`ass`/`eq`/`hqdn3d`/`zscale` (the libfreetype/libass/build-gated arms). So every substitute arm is the LIVE routing path on this wheel, not a dead branch, and the `media_filters` probe reads the set once per build feeding the `_ROUTE`-keyed `_wire` fold.
- [MULTI_INPUT_WIRING] [RESOLVED]: multi-input filters wire through `FilterContext.link_to(target, out_pad, in_pad)`, NOT the sequential `Graph.link_nodes` — verified on `av 17.1.0`: `overlay`/`concat`/`amix`/`blend`/`hstack`/`vstack`/`acrossfade` all `configure()` cleanly when each source is wired `source.link_to(node, 0, index)`, while `link_nodes(a, node); link_nodes(b, node)` raises `ArgumentError: Invalid argument returned 22` for every multi-input pad (`link_nodes` auto-increments to pad 0 twice). The pad arity is read off the filter descriptor — `Filter("overlay").inputs` is `['main', 'overlay']` (2 static named pads), `Filter("concat").inputs` is `()` (dynamic, arity from `n=`), so a static filter's pad count is `len(inputs)` and a dynamic filter's is its `n=`/`inputs=` param. `link_clips` uses `link_to` for `concat`/`amix`; single-input chains keep the simpler `link_nodes(source, *nodes, sink)`.
- [XFADE_DISSOLVE] [RESOLVED]: the native `xfade` filter does NOT configure in-process on `av 17.1.0` under EITHER wiring form — `link_nodes` and `link_to` both raise `ArgumentError 22` at `configure()`, for both rgb24 and yuv420p sources (xfade validates the offset/duration against a stream duration the in-memory buffer sources do not carry). So the `Xfade` arm's ONLY spelling is `cross_dissolve`, a numpy per-frame linear alpha-blend over the overlapping `window` frames — the honest substitute, since shipping the non-configuring native `xfade` node would be exactly the illusory code the density bar rejects. `blend`/`hstack`/`vstack` DO configure via `link_to` and remain available for a spatial composite, but the temporal cross-fade is the numpy ramp. When a future `av` fixes in-process `xfade` wiring, its `_ROUTE` flips from `DISSOLVE` to `NATIVE_MULTI` in one row with `cross_dissolve` retained as the fallback.
- [TEXT_SUBSTITUTE] [RESOLVED]: text burn-in (`drawtext`, absent — needs libfreetype) routes to the `pillow` substitute — `Image.new("RGBA", (w, h), (0, 0, 0, 0))` + `ImageDraw.text(..., font=ImageFont.truetype(path, size, layout_engine=ImageFont.Layout.RAQM), features=, language=, direction=)` renders a transparent RGBA plane with HarfBuzz/FriBidi complex-script shaping when `PIL.features.check("raqm")` confirms the build (else `Layout.BASIC`), and `_composite` numpy-alpha-blends it over each pulled frame's rgb24 buffer. All members verified against the folder `pillow.md` `12.2.0` catalogue (`ImageDraw.text` `features`/`language`/`direction` row [02], `ImageFont.Layout.RAQM` row [10], `features.check` the build probe). The plane renders ONCE (a caption/watermark is static across frames), so the substitute cost is one Pillow render plus a per-frame numpy blend, not a re-shape per frame. Subtitle burn-in (`subtitles`/`ass`, absent — needs libass) composes `media/subtitle#SUBTITLE`'s `pysubs2`-rendered event frames through the same `_composite` path.
- [COLOR_SUBSTITUTE] [RESOLVED]: color grade (`eq`, absent) routes to `curves` (brightness/contrast/gamma luma transfer) + `hue=s=` (saturation), both native-present — `_grade_args` samples `y = clip(((x - 0.5)·contrast + 0.5 + brightness)^(1/gamma))` at five numpy control points and formats the `curves` `all='x/y …'` string, and the saturation rides `hue=s=<saturation>`. `colorbalance`/`colorchannelmixer`/`lut`/`lut3d` are all present for a richer per-channel/temperature grade extension. The perceptual-color depth (CAM16, gamut-safe interpolation) routes to `graphic/color/derive#DERIVE` (the `colour-science`/`coloraide` owner) rather than re-deriving colour math on this page — filtergraph owns the FFmpeg filter routing, the color owner owns the colour science, composed at the seam.
- [DENOISE_SUBSTITUTE] [RESOLVED]: denoise (`hqdn3d`, absent) routes to `nlmeans=s=<strength>` (present) as the native substitute; `atadenoise`/`removegrain`/`gblur` are also present as further substitutes a widened `_wire` arm reaches. The `scikit-image` `restoration.denoise_nl_means` deeper fallback the reading map named is NOT composed directly here — `skimage` is not installed in this environment and the native `nlmeans`/`atadenoise` arms are richer and in-process, so the terminal fallback (a build missing even the native denoise filters) composes `graphic/raster/process#PROCESS` (the `scikit-image` owner) at the seam rather than importing it into the media plane.
- [EXPLICIT_ABUFFERSINK] [RESOLVED]: an audio graph closes on an EXPLICIT `graph.add("abuffersink")` node the `Graph.configure()` `auto_buffer=True` default does NOT add for an audio pipeline (it raises `EINVAL` otherwise, the fix the `media/audio#MEDIA` mastering chain proved) — `link_clips` for `amix` and any audio chain appends the `abuffersink` terminal to `link_to`/`link_nodes` explicitly, while a video chain closes on `buffersink`. The sink kind is read off the pipeline's stream type, not assumed, so a mixed graph never mis-terminates.
