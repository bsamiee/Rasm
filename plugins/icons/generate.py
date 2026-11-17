#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["msgspec", "numpy", "shapely"]
# ///
"""Writes each plugin icon by sweeping a turning reed along Bézier skeletons, then reports its fit."""

from collections.abc import Callable, Iterable
import math
from pathlib import Path
import sys

import msgspec
import numpy as np
import shapely.affinity
lazy import numpy.typing as npt

# --- [TYPES] ----------------------------------------------------------------------------

type _Point = tuple[float, float]
type _Keys = tuple[tuple[float, float], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_SIZE = 1024
_NIB = 84.0
_EDGE = 112.0
_HAIR = 0.07
_ECHO = (44, 30, 22, 8)
_SAFE = 0.12

# --- [MODELS] ---------------------------------------------------------------------------


class _Palette(msgspec.Struct, frozen=True, kw_only=True):
    file: str
    ground: str
    ink: str
    slate: str

    def svg(self, ink: shapely.Geometry, /) -> str:
        d = "".join("M" + "L".join(f"{x:.1f} {y:.1f}" for x, y in shapely.get_coordinates(ring)[:-1]) + "Z" for ring in shapely.get_rings(shapely.get_parts(ink)))
        body = "".join(f'<use href="#i" y="{dy}" fill="{fill}"/>' for dy, fill in (*zip(_ECHO, (self.slate, self.ground) * 2, strict=True), (0, self.ink)))
        return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {_SIZE} {_SIZE}"><defs><path id="i" d="{d}"/></defs><rect width="{_SIZE}" height="{_SIZE}" fill="{self.ground}"/>{body}</svg>'


class _Fit(msgspec.Struct, frozen=True, kw_only=True):
    bounds: tuple[int, ...]
    inside_safe: bool
    ink_pct: float

    @classmethod
    def of(cls, ink: shapely.Geometry, /) -> _Fit:
        bounds = np.add(shapely.bounds(ink), (0, 0, 0, _ECHO[0]))
        return cls(
            bounds=tuple(round(v) for v in bounds),
            inside_safe=bool(bounds[:2].min() >= _SIZE * _SAFE and bounds[2:].max() <= _SIZE * (1 - _SAFE)),
            ink_pct=round(100 * float(shapely.area(ink)) / _SIZE**2, 1),
        )


_PALETTES = (_Palette(file="icon.svg", ground="#EFEAE2", ink="#090909", slate="#97A9B0"), _Palette(file="icon-dark.svg", ground="#0E0E0E", ink="#EFEAE2", slate="#4A5D66"))

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REED]


def _track(keys: _Keys, s: np.ndarray, /) -> np.ndarray:
    at, val = np.array([k[0] for k in keys], float), np.array([k[1] for k in keys], float)
    i = np.clip(np.searchsorted(at, s, side="right") - 1, 0, len(at) - 2)
    u = np.clip((s - at[i]) / np.maximum(at[i + 1] - at[i], 1e-9), 0, 1)
    return val[i] + (val[i + 1] - val[i]) * u * u * (3 - 2 * u)


def _stroke(points: npt.ArrayLike, /, *, width: _Keys = ((0, 1), (1, 1)), edge: _Keys = ((0, _EDGE), (1, _EDGE)), scale: float = 1.0) -> shapely.Geometry:
    pts = np.asarray(points, float)
    t = np.linspace(0, 1, 200)[:, None]
    segs = [(1 - t) ** 3 * p0 + 3 * (1 - t) ** 2 * t * c1 + 3 * (1 - t) * t**2 * c2 + t**3 * p1 for p0, c1, c2, p1 in (pts[i : i + 4] for i in range(0, len(pts) - 1, 3))]
    c = np.vstack([segs[0], *(s[1:] for s in segs[1:])])
    arc = np.concatenate([[0], np.cumsum(np.linalg.norm(np.diff(c, axis=0), axis=1))])
    s = arc / arc[-1]
    w = _NIB * scale * np.maximum(_track(width, s), _HAIR)[:, None] / 2
    th = np.radians(_track(edge, s))
    v = np.stack([np.cos(th), -np.sin(th)], axis=1)
    a, b = c + w * v, c - w * v
    quads = shapely.buffer(shapely.polygons(np.stack([a[:-1], a[1:], b[1:], b[:-1]], axis=1)), 0)
    return shapely.union_all(quads[~shapely.is_empty(quads)])


def _nuqta(x: float, y: float, /, *, scale: float = 1.0) -> shapely.Geometry:
    d = _NIB * scale * np.array([math.cos(math.radians(40.0)), -math.sin(math.radians(40.0))])
    p0 = np.array([x, y]) - d / 2
    return _stroke([p0, p0 + d / 3, p0 + 2 * d / 3, p0 + d], scale=scale)


def _ink(strokes: Iterable[shapely.Geometry], /) -> shapely.Geometry:
    ink = shapely.simplify(shapely.union_all(strokes), 0.15)
    x0, y0, x1, y1 = shapely.bounds(ink)
    return shapely.affinity.translate(ink, _SIZE / 2 - (x0 + x1) / 2, _SIZE / 2 - (y0 + y1 + _ECHO[0]) / 2)


# --- [STROKES]


def _mad(x: float, y: float, length: float, /, *, sag: float = 22, lift: float = 40, body: float = 1.0) -> shapely.Geometry:
    p1 = (x + 0.22 * length, y + sag * 0.75)
    c2 = (x + 0.45 * length, y + sag * 1.15)
    c3 = (2 * p1[0] - c2[0], 2 * p1[1] - c2[1])
    return _stroke(
        [(x + length, y), (x + 0.75 * length, y + sag * 1.15), c2, p1, c3, (x + 0.01 * length, y - lift * 0.1), (x, y - lift)],
        width=((0, 0.95 * body), (0.22, body), (1, 0.0)),
        edge=((0, 118), (0.5, 114), (1, 108)),
    )


def _alif(x: float, y: float, h: float, /, *, scale: float = 1.0) -> shapely.Geometry:
    dx = 0.14 * h
    return _stroke([(x + dx, y), (x + dx * 0.7, y + h * 0.35), (x + dx * 0.3, y + h * 0.7), (x, y + h)], width=((0, 0.82), (0.5, 0.74), (1, 0.08)), edge=((0, 150), (1, 136)), scale=scale)


def _dot_height(scale: float, /) -> float:
    _, top, _, bottom = shapely.bounds(_nuqta(0, 0, scale=scale))
    return bottom - top


def _column(x: float, y: float, count: int, /, *, scale: float = 1.0) -> list[shapely.Geometry]:
    return [_nuqta(x, y + i * (_dot_height(scale) + 6), scale=scale) for i in range(count)]


def _bowl(x: float, y: float, /, *, span: float = 440) -> shapely.Geometry:
    return _stroke(
        [
            (x + span, y - 56),
            (x + span + 6, y + 84 * 0.5),
            (x + span * 0.84, y + 84 * 0.92),
            (x + span * 0.58, y + 84),
            (x + span * 0.32, y + 84 * 1.04),
            (x + span * 0.08, y + 84 * 0.8),
            (x, y + 84 * 0.6 - 34),
        ],
        width=((0, 0.3), (0.14, 0.9), (0.6, 0.9 * 0.92), (1, 0.03)),
        edge=((0, 124), (0.5, 112), (1, 110)),
    )


def _brick(x: float, y: float, nuqtas: float, /) -> shapely.Geometry:
    run = _NIB * nuqtas
    return _stroke([(x + run, y), (x + run * 0.66, y), (x + run * 0.33, y), (x, y)], scale=0.86)


def _thread(points: npt.ArrayLike, /) -> shapely.Geometry:
    return _stroke(points, width=((0, 0.25), (0.3, 0.8), (0.75, 0.8 * 0.8), (1, 0.03)), edge=((0, 118), (1, 110)))


def _hairline(p: _Point, q: _Point, weight: float, /) -> shapely.Geometry:
    return _stroke([p, p, q, q], width=((0, weight), (1, weight)), edge=((0, 20), (1, 20)))


def _sliced(geom: shapely.Geometry, point: _Point, angle: float, shift: _Point, /) -> list[shapely.Geometry]:
    d = np.array([math.cos(math.radians(angle)), -math.sin(math.radians(angle))])
    n = np.array([-d[1], d[0]]) * 4000
    p = np.asarray(point, float)
    half = shapely.Polygon([p + d * 4000, p - d * 4000, p - d * 4000 - n, p + d * 4000 - n])
    return [shapely.affinity.translate(shapely.intersection(geom, half), *shift), shapely.difference(geom, half)]


# --- [REGISTRY] -------------------------------------------------------------------------

_ICONS: frozendict[str, Callable[[], list[shapely.Geometry]]] = frozendict({
    "rasm": lambda: [*_column(600, 0, 3, scale=1.4), _alif(350, -_dot_height(1.4) / 2, 3 * _dot_height(1.4) + 12, scale=1.4)],
    "python": lambda: [_bowl(0, 0, span=470), _bowl(130, 185, span=470), _bowl(260, 370, span=470)],
    "orchestration": lambda: [
        _thread([(940, 210), (760, 200), (610, 330), (480, 470)]),
        _thread([(990, 470), (820, 462), (650, 470), (480, 478)]),
        _thread([(900, 750), (720, 750), (600, 620), (480, 486)]),
        _nuqta(400, 478, scale=1.2),
    ],
    "msbuild": lambda: [*(_brick(x * _NIB * 2.75, 0, 2.6) for x in range(3)), *(_brick((x + 0.5) * _NIB * 2.75, -_NIB * 1.36, 2.6) for x in range(2)), _brick(_NIB * 2.75, -_NIB * 2.72, 2.6)],
    "dotnet": lambda: [
        _mad(0, 0, 640, sag=10, lift=26),
        _mad(-40, 210, 640, sag=10, lift=26),
        _stroke([(450, -190), (425, -50), (355, 210), (310, 420)], width=((0, 0.9), (0.4, 0.75), (1, 0.04)), edge=((0, 150), (1, 140))),
        _stroke([(270, -190), (245, -50), (175, 210), (130, 420)], width=((0, 0.9), (0.4, 0.75), (1, 0.04)), edge=((0, 150), (1, 140))),
    ],
    "blender": lambda: [
        _nuqta(150, 500, scale=1.2),
        _nuqta(790, 160, scale=1.2),
        _stroke([(730, 170), (530, 170), (420, 490), (210, 490)], width=((0, 0.05), (0.2, 0.75), (0.5, 1.1), (0.8, 0.75), (1, 0.05)), edge=((0, 114), (1, 114))),
    ],
    "uxp": lambda: [
        _mad(100, 100, 520, sag=6, lift=0),
        _hairline((108, 112), (108, 700), 0.15),
        _hairline((616, 112), (616, 700), 0.15),
        _mad(100, 700, 520, sag=6, lift=0),
        _nuqta(362, 360, scale=1.2),
    ],
    "function-hooks": lambda: [
        *_sliced(_mad(0, 0, 660, sag=16, lift=46, body=1.12), (440, 30), 64, (-62, 128)),
        _hairline(
            (440 + 110 * math.cos(math.radians(64)) * 2.4, 30 - 110 * math.sin(math.radians(64)) * 2.4),
            (440 - 110 * math.cos(math.radians(64)) * 3.4, 30 + 110 * math.sin(math.radians(64)) * 3.4),
            0.08,
        ),
    ],
    "cpp": lambda: [
        _mad(0, 0, 320, sag=8, lift=24),
        _stroke([(218, -180), (201, -66), (160, 86), (133, 190)], width=((0, 0.95), (0.4, 0.8), (1, 0.04)), edge=((0, 150), (1, 140))),
        _mad(400, 0, 320, sag=8, lift=24),
        _stroke([(618, -180), (601, -66), (560, 86), (533, 190)], width=((0, 0.95), (0.4, 0.8), (1, 0.04)), edge=((0, 150), (1, 140))),
    ],
    "rust": lambda: [
        _mad(0, 0, 620, sag=14, lift=70),
        _stroke([(330, 26), (230, 40), (130, 80), (70, 140)], width=((0, 0.5), (0.3, 0.6), (1, 0.05)), edge=((0, 116), (1, 104))),
        _nuqta(10, 190, scale=1.2),
    ],
    "swift": lambda: [piece for left, right in (_sliced(_mad(0, 0, 640, sag=10, lift=30, body=1.1), (450, 0), 112, (-36, 96)),) for piece in (right, *_sliced(left, (215, 96), 112, (-36, 96)))],
    "typescript": lambda: [_nuqta(640, -210), _nuqta(560, -10), _nuqta(700, 190), _hairline((470, -270), (470, 290), 0.15), _mad(50, 0, 340, sag=8, lift=0, body=1.1)],
    "search": lambda: [
        _mad(0, 0, 640, sag=6, lift=0, body=0.34),
        _mad(400, 160, 240, sag=6, lift=0, body=0.34),
        _mad(60, 160, 300, sag=12, lift=10, body=1.15),
        _mad(140, 320, 500, sag=6, lift=0, body=0.34),
    ],
    "rhino": lambda: [
        _stroke([(0, 420), (90, 20), (430, -40), (660, 230)], width=((0, 0.05), (0.2, 0.85), (0.55, 1.0), (0.85, 0.7), (1, 0.05)), edge=((0, 112), (1, 112))),
        *(_hairline(p, q, 0.12) for p, q in (((0, 420), (90, 20)), ((90, 20), (430, -40)), ((430, -40), (660, 230)))),
        *(_nuqta(x, y, scale=1.1) for x, y in ((0, 420), (90, 20), (430, -40), (660, 230))),
    ],
    "tool": lambda: [_brick(330, 0, 4.2), _nuqta(230, 6, scale=1.1), _nuqta(120, 6, scale=0.85), _nuqta(30, 6, scale=0.6)],
    "web": lambda: [
        _nuqta(310, 0, scale=1.2),
        _nuqta(0, 500, scale=1.2),
        _nuqta(620, 500, scale=1.2),
        _stroke([(265, 55), (205, 180), (120, 340), (50, 445)], width=((0, 0.05), (0.5, 0.7), (1, 0.05))),
        _stroke([(355, 55), (415, 180), (500, 340), (570, 445)], width=((0, 0.05), (0.5, 0.7), (1, 0.05))),
        _stroke([(75, 510), (260, 515), (360, 515), (545, 510)], width=((0, 0.05), (0.5, 0.7), (1, 0.05))),
    ],
    "delegate": lambda: [_nuqta(290 * math.sin(math.radians(a)), -290 * math.cos(math.radians(a)), scale=1.35) for a in range(0, 360, 72)],
    "terminal": lambda: [
        _stroke([(0, 0), (90, 70), (200, 150), (290, 220)], width=((0, 0.95), (0.6, 0.85), (1, 0.4)), edge=((0, 66), (1, 56))),
        _stroke([(290, 220), (200, 290), (90, 370), (0, 440)], width=((0, 0.4), (0.4, 0.9), (1, 0.04)), edge=((0, 130), (1, 112))),
        _brick(390, 440, 3.6),
    ],
})

# --- [COMPOSITION] ----------------------------------------------------------------------

if __name__ == "__main__":
    inks = {name: _ink(_ICONS[name]()) for name in sys.argv[1:] or _ICONS}
    for name, ink in inks.items():
        Path(__file__).with_name(name).mkdir(exist_ok=True)
        for palette in _PALETTES:
            Path(__file__).with_name(name).joinpath(palette.file).write_text(palette.svg(ink), encoding="utf-8")
    sys.stdout.buffer.write(msgspec.json.encode({name: _Fit.of(ink) for name, ink in inks.items()}) + b"\n")
