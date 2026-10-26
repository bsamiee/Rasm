"""Builds Blender extension archives with geometry tool icons, and import folders, from uv lock."""

lazy import ast
lazy from collections.abc import Callable
lazy from math import ceil, comb, prod, sqrt
lazy from pathlib import Path, PurePosixPath
lazy import shutil
lazy import subprocess
lazy import tempfile
lazy from typing import Annotated, override
lazy from zipfile import ZipFile

import msgspec
lazy from asttokens import ASTText
lazy from asttokens.util import replace
lazy from coloraide import Color
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingDirectory, ResolvedExistingFile, ResolvedPath
lazy from expression import Error, Ok, Result
lazy from expression.extra.result import catch
lazy from fontTools.pens.basePen import BasePen
lazy import numpy as np
lazy import numpy.typing as npt
lazy from packaging.requirements import Requirement
lazy from packaging.utils import canonicalize_name, parse_wheel_filename
lazy from picosvg.geometric_types import Rect
lazy from picosvg.svg import SVG
lazy from picosvg.svg_pathops import skia_path
lazy from picosvg.svg_transform import Affine2D
lazy from picosvg.svg_types import SVGShape
lazy import shapely
lazy import tomlkit

lazy from eng.python.fitout.faults import Fault

# --- [TYPES] ----------------------------------------------------------------------------

type _Point = tuple[float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class _Interpreter(msgspec.Struct, frozen=True):
    executable: str
    platform: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def _accumulated[T](head: Result[T, Fault], *others: Result[object, Fault]) -> Result[T, Fault]:
    """Returns `head`, else fault of every failed independent outcome."""
    failures = [*outcome.swap().to_option().to_list() for outcome in (head, *others)]
    return Error(Fault.joined(failures)) if failures else head


def _ran(*command: str | Path, stdout: int | None = None, stderr: int | None = None) -> Result[subprocess.CompletedProcess[bytes], Fault]:
    """Runs child process, nonzero exit as fault."""
    done = subprocess.run(command, stdout=stdout, stderr=stderr, check=False)
    return catch(done.check_returncode, exception=subprocess.CalledProcessError)().map(lambda _: done).map_error(Fault.of)


def _interpreter(blender: Path) -> Result[_Interpreter, Fault]:
    """Returns Blender Python executable and host extension platform."""
    probe = "import json, sys; from bl_pkg.cli.blender_ext import platform_from_this_system; sys.stderr.write(json.dumps({'executable': sys.executable, 'platform': platform_from_this_system()}))"
    return _ran(blender, "-b", "--factory-startup", "--python-exit-code", "1", "--python-expr", probe, stderr=subprocess.PIPE).map(lambda host: msgspec.json.decode(host.stderr, type=_Interpreter))


def _exported(lock: Path, *options: str) -> Result[Path, Fault]:
    """Returns `lock` holding the locked runtime closure of the project `options` select, workspace members and distributions Blender bundles left out."""
    bundled = (*("--no-emit-package", Requirement(entry).name) for entry in msgspec.toml.decode(Path("pyproject.toml").read_bytes())["tool"]["uv"]["constraint-dependencies"])
    return _ran("uv", "export", "--locked", "--no-default-groups", "--no-hashes", *bundled, *options, "--output-file", lock, stdout=subprocess.DEVNULL).map(lambda _: lock)


def encoded(master: Path) -> bytes:
    """Encodes master solid paints as Blender geometry icon triangles.

    Icon files hold `VCO` header, byte XY corners per triangle over -1..1 square, then sRGB byte RGBA per corner, in SVG draw order.
    `currentColor` paints white tool ink.
    Shape alpha applies without SVG source-over or isolated-group compositing.

    Args:
        master: WorkSpaceTool SVG master.

    Returns:
        Icon file bytes.

    Raises:
        ValueError: If master has no viewport dimensions or holds a nonliteral paint.
    """
    size = 255

    class Rings(BasePen):
        """Collects closed contours as polylines within half a byte step of each cubic."""

        def __init__(self) -> None:
            """Initializes pen and ring buffer."""
            super().__init__()  # type: ignore[no-untyped-call]
            self.rings: list[list[_Point]] = []

        @override
        def _moveTo(self, pt: _Point) -> None:
            self.rings.append([pt])

        @override
        def _lineTo(self, pt: _Point) -> None:
            self.rings[-1].append(pt)

        @override
        def _curveToOne(self, pt1: _Point, pt2: _Point, pt3: _Point) -> None:
            start: _Point = self._getCurrentPoint()  # type: ignore[no-untyped-call]
            knots = np.array((start, pt1, pt2, pt3))
            steps = max(1, ceil(sqrt(3 * 2 / 8 * float(np.hypot(*np.diff(knots, 2, axis=0).T).max()) * size)))
            t = np.linspace(0, 1, steps + 1)[1:]
            self.rings[-1].extend(map(tuple, (np.array([comb(3, k) * t**k * (1 - t) ** (3 - k) for k in range(4)]).T @ knots).tolist()))

    def painted(shape: SVGShape, opacity: float, transform: Affine2D) -> tuple[bytes, npt.NDArray[np.uint8]]:
        color = Color("white" if shape.fill.lower() == "currentcolor" else shape.fill).convert("srgb").fit()
        rgba = bytes(round(channel * size) for channel in (*color.coords(nans=False), color.alpha(nans=False) * shape.opacity * opacity))
        pen = Rings()
        skia_path(shape.as_path().remove_overlaps().as_cmd_seq(), "nonzero").transform(*transform).draw(pen)
        region = shapely.build_area(shapely.GeometryCollection([shapely.LinearRing(ring) for ring in pen.rings])).intersection(shapely.box(-1, -1, 1, 1))
        triangles = shapely.get_coordinates(shapely.orient_polygons(shapely.get_parts(shapely.constrained_delaunay_triangles(region)))).reshape(-1, 4, 2)[:, :3]
        corners = np.rint((triangles + 1) / 2 * size).astype(np.int64)
        (first, second, third) = corners.transpose(1, 0, 2)
        kept = corners[(first[:, 0] - second[:, 0]) * (second[:, 1] - third[:, 1]) + (first[:, 1] - second[:, 1]) * (third[:, 0] - second[:, 0]) > 0]
        return rgba * kept.shape[0] * 3, kept.astype(np.uint8)

    styled: SVG = SVG.parse(master).apply_style_attributes()
    if (viewbox := (svg := styled.topicosvg()).view_box()) is None:
        raise ValueError(f"SVG master {master} has no viewport dimensions")
    transform = Affine2D.flip_y() @ Affine2D.rect_to_rect(viewbox, Rect(-1, -1, 2, 2), svg.svg_root.get("preserveAspectRatio", "xMidYMid meet"))
    paints = tuple(
        painted(context.shape(), float(styled.svg_root.get("opacity", "1")) * prod(float(ancestor.get("opacity", "1")) for ancestor in context.element.iterancestors()), transform)
        for context in svg.depth_first()
        if context.is_shape()
    )
    return b"".join((b"VCO\x00", bytes((size, size, 0, 0)), *(corners.tobytes() for _, corners in paints), *(colors for colors, _ in paints)))


def pack(
    source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, version: str, maintainer: str, website: str, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]
) -> Result[int, Fault]:
    """Builds host platform extension archive, first-party packages as submodules and the extension's locked dependency closure as wheels.

    Args:
        source: Extension directory with manifest, dependency rows, and assets.
        directory: Folder built archive replaces.
        version: Semantic version written to manifest.
        maintainer: Maintainer written to manifest.
        website: Website written to manifest.
        blender: Blender executable.

    Returns:
        Zero, or fault of every failed child process, icon master, and archive placement.
    """
    first = frozenset(
        canonicalize_name(msgspec.toml.decode(Path(member, "pyproject.toml").read_bytes())["project"]["name"])
        for member in msgspec.toml.decode(Path("pyproject.toml").read_bytes())["tool"]["uv"]["workspace"]["members"]
    )
    icons = source / "icons"

    def staged(stage: Path, interpreter: _Interpreter) -> None:
        wheels = stage / "wheels"
        roots: set[str] = set()
        for first_party in [wheel for wheel in wheels.glob("*.whl") if parse_wheel_filename(wheel.name)[0] in first]:
            with ZipFile(first_party) as wheel:
                members = [name for name in wheel.namelist() if not PurePosixPath(name).parts[0].endswith(".dist-info")]
                wheel.extractall(stage, members)
            first_party.unlink()
            roots.update(PurePosixPath(name).parts[0] for name in members)
        for module in stage.rglob("*.py"):
            syntax = ASTText(text := module.read_text(newline=""), filename=str(module))
            module.write_text(
                replace(
                    text,
                    [
                        (*syntax.get_text_range(node, padded=False), ast.unparse(ast.ImportFrom(module=node.module, names=node.names, level=len(module.relative_to(stage).parent.parts) + 1)))
                        for node in ast.walk(syntax.tree)
                        if isinstance(node, ast.ImportFrom) and node.level == 0 and node.module is not None and node.module.split(".")[0] in roots
                    ],
                ),
                newline="",
            )
        document = tomlkit.parse((manifest := stage / "blender_manifest.toml").read_text())
        document.update(
            version=version,
            maintainer=maintainer,
            website=website,
            platforms=[interpreter.platform],
            wheels=tomlkit.item([f"./wheels/{wheel.name}" for wheel in sorted(wheels.glob("*.whl"))]).multiline(multiline=True),
        )
        manifest.write_text(tomlkit.dumps(document))

    with tempfile.TemporaryDirectory() as folder:
        stage = Path(folder, "stage")
        previews = stage / icons.name
        shutil.copytree(source, stage, ignore=shutil.ignore_patterns("pyproject.toml", "*.svg"))
        lock = Path(folder, "requirements.txt")
        built = Path(folder, "archives")
        built.mkdir()
        return (
            _accumulated(
                _interpreter(blender),
                _exported(lock, "--no-editable", "--project", str(source)),
                *(
                    *(
                        catch(encoded, exception=ValueError)(master).map((previews / f"{master.stem}.dat").write_bytes).map_error(Fault.of),
                        _ran("resvg", "--width", "256", "--height", "256", master, previews / f"{master.stem}.png"),
                    )
                    for master in icons.glob("*.svg")
                ),
            )
            .bind(
                lambda interpreter: _ran("pip", "--python", interpreter.executable, "--disable-pip-version-check", "wheel", "--wheel-dir", stage / "wheels", "--no-deps", "--requirement", lock).map(
                    lambda _: staged(stage, interpreter)
                )
            )
            .bind(lambda _: _ran(blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-dir", built, "--split-platforms"))
            .bind(lambda _: reset(directory, built.move))
            .map(lambda _: 0)
        )


def reset[T](folder: Path, made: Callable[[Path], T], /) -> Result[T, Fault]:
    """Removes `folder` with its tree, a missing one counting as removed, then makes it through `made` under created parents.

    Args:
        folder: Folder replaced.
        made: Call making `folder` from its path.

    Returns:
        What `made` returns, or fault of a failed removal, parent creation, or `made` call.
    """
    return (
        catch(shutil.rmtree, exception=OSError)(folder)
        .or_else_with(lambda error: Ok(None) if isinstance(error, FileNotFoundError) else Error(error))
        .bind(lambda _: catch(folder.parent.mkdir, exception=OSError)(parents=True, exist_ok=True))
        .bind(lambda _: catch(made, exception=OSError)(folder))
        .map_error(Fault.of)
    )


def site(target: ResolvedPath, /, *, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]) -> Result[int, Fault]:
    """Syncs Blender import folder to distributions `pack` bundles, reinstalling only on version or tag change.

    Args:
        target: Folder of synced packages.
        blender: Blender executable.

    Returns:
        Zero, or fault of every failed child process.
    """
    with tempfile.TemporaryDirectory() as folder:
        lock = Path(folder, "requirements.txt")
        return (
            _accumulated(_interpreter(blender), _exported(lock, "--no-emit-workspace"))
            .bind(lambda interpreter: _ran("uv", "pip", "sync", "--python", interpreter.executable, "--target", target, lock))
            .map(lambda _: 0)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["encoded", "pack", "reset", "site"]
