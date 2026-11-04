"""Builds Blender extensions and syncs their Python requirements from uv's lock."""

lazy import ast
lazy from collections.abc import Callable
lazy from functools import partial
lazy from hashlib import sha256
lazy from itertools import chain
lazy from math import ceil, prod, sqrt
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
lazy from expression import Result
lazy from expression.extra.result import catch
lazy from fontTools.misc.bezierTools import cubicPointAtT
lazy from fontTools.pens.basePen import BasePen
lazy from more_itertools import partition
lazy import numpy as np
lazy import numpy.typing as npt
lazy from packaging.utils import parse_wheel_filename
lazy from picosvg.geometric_types import Rect
lazy from picosvg.svg import SVG
lazy from picosvg.svg_pathops import skia_path
lazy from picosvg.svg_transform import Affine2D
lazy from picosvg.svg_types import SVGShape
lazy import shapely
lazy import tomlkit

lazy from blender.records.faults import accumulated, Error, Fault, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Blender = Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]

# --- [CONSTANTS] ------------------------------------------------------------------------

BACKGROUND = frozenset({"-b", "--background", "-c", "--command"})

# --- [MODELS] ---------------------------------------------------------------------------


class _Interpreter(msgspec.Struct, frozen=True):
    executable: str
    platform: str
    user_site: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def ran(*command: str | Path, stdout: int | None = None, stderr: int | None = None) -> Outcome[subprocess.CompletedProcess[bytes]]:
    """Runs a child process, a failed launch or nonzero exit as fault.

    Args:
        command: Executable and its arguments.
        stdout: Output stream target, inherited when None.
        stderr: Error stream target, inherited when None.

    Returns:
        Finished process, or fault of a failed launch or of the nonzero exit's `CalledProcessError`.
    """
    try:
        return Ok(subprocess.run(command, stdout=stdout, stderr=stderr, check=True))
    except (OSError, subprocess.CalledProcessError) as error:
        return Error(Fault.of(Reason.FAILED, str(command[0]), error))


def _interpreter(blender: Path, userbase: Path | None = None) -> Outcome[_Interpreter]:
    """Returns native Python executable, extension platform, and user-site path."""
    variables = None if userbase is None else {"userbase": str(userbase)}
    probe = (
        "import json, sys, sysconfig; from bl_pkg.cli.blender_ext import platform_from_this_system; "
        "sys.stderr.write(json.dumps({'executable': sys.executable, 'platform': platform_from_this_system(), "
        f"'user_site': sysconfig.get_path('purelib', scheme=sysconfig.get_preferred_scheme('user'), vars={variables!r})}}))"
    )
    return ran(blender, "-b", "--factory-startup", "--python-exit-code", "1", "--python-expr", probe, stdout=subprocess.PIPE, stderr=subprocess.PIPE).bind(
        lambda host: catch(lambda: msgspec.json.decode(host.stderr, type=_Interpreter), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, str(blender)))
    )


def _exported(lock: Path, *options: str) -> Outcome[subprocess.CompletedProcess[bytes]]:
    """Writes locked requirements without Blender's distributions."""
    source = Path("uv.lock")
    return (
        catch(source.read_bytes, exception=OSError)()
        .map_error(partial(Fault.of, Reason.UNREADABLE, str(source)))
        .bind(
            lambda raw: ran(
                "uv",
                "export",
                "--locked",
                "--no-default-groups",
                "--no-hashes",
                "--no-header",
                "--no-annotate",
                *(*("--no-emit-package", constraint["name"]) for constraint in msgspec.toml.decode(raw)["manifest"]["constraints"]),
                *options,
                "--output-file",
                lock,
                stdout=subprocess.DEVNULL,
            )
        )
    )


def encoded(master: Path) -> tuple[str, bytes]:
    """Encodes master as the picosvg preview Blender's nanosvg loader rasterizes and as geometry icon triangles of its solid paints.

    Icon files hold `VCO` header, byte XY corners per triangle over -1..1 square, then sRGB byte RGBA per corner, in SVG draw order.
    `currentColor` paints white.
    Shape alpha applies without SVG source-over or isolated-group compositing.

    Args:
        master: WorkSpaceTool SVG master.

    Returns:
        Preview document text and icon file bytes.

    Raises:
        ValueError: If master has no viewport dimensions or holds a nonliteral paint.
    """
    type Point = tuple[float, float]

    size = np.iinfo(np.uint8).max
    grid = 1.0

    class Rings(BasePen):
        """Collects closed contours as polylines within half a byte step of each cubic."""

        def __init__(self) -> None:
            """Initializes pen and ring buffer."""
            super().__init__()  # type: ignore[no-untyped-call]
            self.rings: list[list[Point]] = []

        @override
        def _moveTo(self, pt: Point) -> None:
            self.rings.append([pt])

        @override
        def _lineTo(self, pt: Point) -> None:
            self.rings[-1].append(pt)

        @override
        def _curveToOne(self, pt1: Point, pt2: Point, pt3: Point) -> None:
            start = self.rings[-1][-1]
            knots = np.array((start, pt1, pt2, pt3))
            steps = max(1, ceil(sqrt(3 * 2 / 8 * float(np.hypot(*np.diff(knots, 2, axis=0).T).max()) / (grid / 2))))
            t = np.linspace(0, 1, steps + 1)[1:]
            self.rings[-1].extend(zip(*cubicPointAtT(start, pt1, pt2, pt3, t)))  # type: ignore[no-untyped-call]

    def painted(shape: SVGShape, opacity: float) -> tuple[bytes, npt.NDArray[np.uint8]]:
        color = Color(shape.fill).convert("srgb").fit()
        rgba = bytes(round(channel * size) for channel in (*color.coords(nans=False), color.alpha(nans=False) * shape.opacity * opacity))
        pen = Rings()
        skia_path(shape.as_path().remove_overlaps().as_cmd_seq(), "nonzero").transform(*transform).draw(pen)
        region = shapely.set_precision(shapely.build_area(shapely.GeometryCollection([shapely.LinearRing(ring) for ring in pen.rings])).intersection(shapely.box(0, 0, size, size)), grid)
        corners = shapely.get_coordinates(shapely.orient_polygons(shapely.get_parts(shapely.constrained_delaunay_triangles(region)))).reshape(-1, 4, 2)[:, :3].astype(np.uint8)
        return rgba * corners.shape[0] * 3, corners

    svg: SVG = SVG.parse(master).apply_style_attributes(inplace=True)  # type: ignore[no-untyped-call]  # ty: ignore[unsound-assignment]
    for element in svg.svg_root.iter():
        for name, value in element.attrib.items():
            if name in {"fill", "stroke", "stop-color"} and value.strip().casefold() == "currentcolor":
                element.set(name, "white")
    opacity = float(svg.svg_root.get("opacity", "1"))
    svg.topicosvg(inplace=True)  # type: ignore[no-untyped-call]
    if (viewbox := svg.view_box()) is None:
        raise ValueError(f"SVG master {master} has no viewport dimensions")
    transform = Affine2D(1, 0, 0, -1, 0, size) @ Affine2D.rect_to_rect(viewbox, Rect(0, 0, size, size), svg.svg_root.get("preserveAspectRatio", "xMidYMid meet"))  # type: ignore[call-arg]
    paints = tuple(
        painted(context.shape(), opacity * prod(float(ancestor.get("opacity", "1")) for ancestor in context.element.iterancestors()))
        for context in svg.depth_first()  # type: ignore[no-untyped-call]
        if context.is_shape()
    )
    return svg.tostring(), b"".join((b"VCO\x00", bytes((size, size, 0, 0)), *(corners.tobytes() for _, corners in paints), *(colors for colors, _ in paints)))  # type: ignore[no-untyped-call]  # ty: ignore[unsound-return-statement]


def pack(source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, version: str, maintainer: str, website: str, blender: Blender) -> Outcome[int]:
    """Builds host platform extension archive, first-party packages as submodules and the extension's locked dependency closure as wheels.

    Args:
        source: Extension directory with manifest, dependency rows, and assets.
        directory: Folder built archive replaces.
        version: Semantic version written to manifest.
        maintainer: Maintainer written to manifest.
        website: Website written to manifest.
        blender: Blender executable.

    Returns:
        Zero, or fault of every failed child process, icon master, and folder reset.
    """

    def iconed(master: Path) -> Outcome[int]:
        try:
            preview, icon = encoded(master)
            return Ok((target := stage / master.relative_to(source)).write_text(preview) + target.with_suffix(".dat").write_bytes(icon))
        except (OSError, ValueError, SyntaxError) as error:
            return Error(Fault.of(Reason.FAILED if isinstance(error, OSError) else Reason.INVALID, str(master), error))

    def staged(interpreter: _Interpreter, workspace: frozenset[str]) -> Outcome[None]:
        try:
            external, local = partition(lambda wheel: parse_wheel_filename(wheel.name)[0] in workspace, (stage / "wheels").glob("*.whl"))
            roots: set[str] = set()
            for member in local:
                with ZipFile(member) as wheel:
                    members = [name for name in wheel.namelist() if not PurePosixPath(name).parts[0].endswith(".dist-info")]
                    wheel.extractall(stage, members)
                member.unlink()
                roots.update(PurePosixPath(name).parts[0] for name in members)
            for module in stage.rglob("*.py"):
                syntax = ASTText(text := module.read_text(newline=""), filename=str(module))
                if imports := [
                    (*syntax.get_text_range(node, padded=False), ast.unparse(ast.ImportFrom(module=node.module, names=node.names, level=len(module.relative_to(stage).parent.parts) + 1)))
                    for node in ast.walk(syntax.tree)
                    if isinstance(node, ast.ImportFrom) and node.level == 0 and node.module is not None and node.module.split(".", 1)[0] in roots
                ]:
                    module.write_text(replace(text, imports), newline="")
            document = tomlkit.parse((manifest := stage / "blender_manifest.toml").read_text())
            document.update(
                version=version,
                maintainer=maintainer,
                website=website,
                platforms=[interpreter.platform],
                wheels=tomlkit.item([f"./wheels/{wheel.name}" for wheel in sorted(external)]).multiline(multiline=True),
            )
            manifest.write_text(tomlkit.dumps(document))
        except OSError as error:
            return Error(Fault.of(Reason.UNWRITABLE, str(stage), error))
        return Ok(None)

    try:
        with tempfile.TemporaryDirectory() as folder:
            stage, lock, built = Path(folder, "stage"), Path(folder, "requirements.txt"), Path(folder, "archives")
            shutil.copytree(source, stage, ignore=shutil.ignore_patterns("pyproject.toml"))
            built.mkdir()
            return (
                paired(
                    _interpreter(blender),
                    _exported(lock, "--no-editable", "--project", str(source)),
                    accumulated(map(iconed, source.glob("icons/**/*.svg"))),
                    ran("uv", "workspace", "list", "--color", "never", stdout=subprocess.PIPE).map(lambda members: frozenset(members.stdout.decode().splitlines())),
                )
                .bind(
                    lambda held: ran("pip", "--python", held[0].executable, "--disable-pip-version-check", "wheel", "--wheel-dir", stage / "wheels", "--no-deps", "--requirement", lock).bind(
                        lambda _: staged(held[0], held[3])
                    )
                )
                .bind(lambda _: ran(blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-dir", built, "--split-platforms"))
                .bind(lambda _: reset(directory, built.move))
                .map(lambda _: 0)
            )
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, str(source), error))


def reset[T](folder: Path, made: Callable[[Path], T], /) -> Outcome[T]:
    """Removes `folder` with its tree, then makes it through `made` under created parents.

    Args:
        folder: Folder replaced.
        made: Call making `folder` from its path.

    Returns:
        What `made` returns, or fault of a failed removal, parent creation, or `made` call.
    """
    return (
        catch(shutil.rmtree, exception=OSError)(folder)
        .or_else_with(lambda error: Result[None, OSError].Ok(None) if isinstance(error, FileNotFoundError) else Error(error))
        .bind(lambda _: catch(folder.parent.mkdir, exception=OSError)(parents=True, exist_ok=True))
        .bind(lambda _: catch(made, exception=OSError)(folder))
        .map_error(partial(Fault.of, Reason.UNWRITABLE, str(folder)))
    )


def site(target: ResolvedPath | None = None, /, *, blender: Blender, package: str | None = None, groups: tuple[str, ...] = ()) -> Outcome[Path]:
    """Syncs locked requirements into native Python user-site storage, retaining editable projects.

    Args:
        target: Python user base; omitted values use a cache keyed by requirements and host.
        blender: Blender executable.
        package: Workspace member selecting locked runtime requirements, workspace root when omitted.
        groups: Additional uv dependency group selectors.

    Returns:
        Python user base, or fault of an export, host query, or package sync.
    """
    options = (*(("--package", package) if package is not None else ()), *chain.from_iterable(("--group", group) for group in groups))
    cache = Path(".cache/blender/python")
    try:
        with tempfile.TemporaryDirectory() as folder:
            lock = Path(folder, "requirements.txt")
            return _exported(lock, *options).bind(
                lambda _: (
                    _interpreter(
                        blender, base := target if target is not None else (cache / sha256(msgspec.msgpack.encode((str(blender), blender.stat().st_mtime_ns, lock.read_bytes()))).hexdigest()).resolve()
                    )
                    .bind(lambda host: ran("uv", "pip", "sync", "--python", host.executable, "--target", host.user_site, lock))
                    .map(lambda _: base)
                )
            )
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, str(target or cache), error))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BACKGROUND", "Blender", "encoded", "pack", "ran", "reset", "site"]
