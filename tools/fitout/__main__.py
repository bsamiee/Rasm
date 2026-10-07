"""Pack host extension projects into archives and install archives into their host, printing one JSON outcome."""

from collections import defaultdict
from collections.abc import Coroutine, Iterator, Mapping
from enum import StrEnum
from functools import partial
import hashlib
from itertools import chain, pairwise
import os
from pathlib import Path
import shutil
import tarfile
import tomllib
from typing import override, Self

import anyio
import cyclopts
from fontTools.pens.basePen import BasePen
from fontTools.pens.explicitClosingLinePen import ExplicitClosingLinePen
from fontTools.pens.transformPen import TransformPen
from fontTools.svgLib.path.parser import parse_path
import httpx2
import msgspec
from packaging.metadata import Metadata
from packaging.pylock import Package, PackageVcs, PackageWheel, Pylock
from packaging.requirements import Requirement
from packaging.utils import canonicalize_name, NormalizedName
from packaging.version import Version
import pathops
import svgelements
import tomlkit

from fitout.vendoring import copied, cored
from interface.blender.packages import BlenderEnvironment, built, bundled
from interface.blender.rows import Interpreter
from interface.blender.session import bridged, windowed
from interface.host import Applied, bootstrap, downloaded, environment, Error, executed, Failed, fetched, Header, outcome, parse, printed, Result
from interface.rhino import packages
from interface.rhino.session import Rhino

# --- [TYPES] ----------------------------------------------------------------------------

type Point = tuple[float, float]
type Knot = tuple[Point, Point, Point]

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [GLYPHS]
def outlined(folder: Path, scale: int) -> dict[str, dict[float, tuple[tuple[Knot, ...], ...]]]:
    """Glyph stem to its opacity groups in ascending alpha, each group's unioned contours as cubic knots (point, left, right) in icon square coordinates."""

    class Knots(BasePen):
        """Cubic knots of explicitly closed contours, with quadratics elevated by `BasePen`."""

        def __init__(self, path: pathops.Path, matrix: tuple[float, ...]) -> None:
            super().__init__()
            self.contours: list[tuple[Knot, ...]] = []
            self.curves: list[Knot] = []
            path.draw(TransformPen(ExplicitClosingLinePen(self), matrix))

        @override
        def _moveTo(self, pt: Point) -> None:
            self.curves = []

        @override
        def _lineTo(self, pt: Point) -> None:
            self._curveToOne(self._getCurrentPoint(), pt, pt)

        @override
        def _curveToOne(self, pt1: Point, pt2: Point, pt3: Point) -> None:
            self.curves.append((pt1, pt2, pt3))

        @override
        def _closePath(self) -> None:
            self.contours.append(tuple((point, entering, leaving) for (_, entering, point), (leaving, _, _) in pairwise(chain(self.curves, self.curves[:1]))))

    def parts(shape: svgelements.Shape, tolerance: float) -> Iterator[tuple[float, pathops.Path]]:
        """Fill and expanded stroke at their paint opacity, the stroke's conics approximated within the tolerance."""
        path = pathops.Path()
        parse_path(shape.d(), path.getPen())
        path.fillType = pathops.FillType.EVEN_ODD if shape.values.get("fill-rule") == "evenodd" else pathops.FillType.WINDING
        if shape.fill.value is not None:
            yield shape.fill.opacity, path
        if shape.stroke.value is not None:
            stroke, cap, join = pathops.Path(path), shape.values.get("stroke-linecap", "butt").upper(), shape.values.get("stroke-linejoin", "miter").upper()
            stroke.stroke(shape.stroke_width, pathops.LineCap[f"{cap}_CAP"], pathops.LineJoin[f"{join}_JOIN"], float(shape.values.get("stroke-miterlimit", 4)))
            stroke.convertConicsToQuads(tolerance)
            yield shape.stroke.opacity, stroke

    def glyph(source: Path) -> dict[float, tuple[tuple[Knot, ...], ...]]:
        """Union per opacity of every shape's parts, drawn as knots."""
        svg = svgelements.SVG.parse(str(source), reify=True)
        unions: defaultdict[float, pathops.OpBuilder] = defaultdict(pathops.OpBuilder)
        for alpha, part in (part for shape in svg.elements() if isinstance(shape, svgelements.Shape) for part in parts(shape, svg.width / scale / 2)):
            unions[alpha].add(part, pathops.PathOp.UNION)
        return {alpha: tuple(Knots(unions[alpha].resolve(), (2 / svg.width, 0, 0, -2 / svg.height, -1, 1)).contours) for alpha in sorted(unions)}

    return {source.stem: glyph(source) for source in sorted(folder.glob("*.svg"))}


# --- [SOURCES]
def committed(archive: Path) -> int:
    """Commit time a repository source archive records as its members' modification time."""
    return int(next(iter(tarfile.open(archive))).mtime)


# --- [WHEELS]
def accepted(interpreter: Interpreter, requirements: Mapping[NormalizedName, tuple[Requirement, ...]], markers: Mapping[str, str]) -> frozenset[str]:
    """Largest set of bundled distributions whose bundled version every active requirement of a distribution outside the set contains."""
    version = Version(interpreter.version)
    evaluated = {
        **markers,
        "python_version": f"{version.major}.{version.minor}",
        "python_full_version": interpreter.version,
        "implementation_name": "cpython",
        "implementation_version": interpreter.version,
        "platform_python_implementation": "CPython",
    }

    def kept(names: frozenset[str]) -> frozenset[str]:
        held = [row for owner, rows in requirements.items() if owner not in names for row in rows if row.marker is None or row.marker.evaluate(evaluated)]
        narrowed = frozenset(name for name in names if all(row.specifier.contains(interpreter.distributions[name], prereleases=True) for row in held if canonicalize_name(row.name) == name))
        return names if narrowed == names else kept(narrowed)

    return kept(frozenset(interpreter.distributions))


# --- [PACK]
async def packed(root: Path, folder: Path, scope: Path, client: httpx2.AsyncClient, environ: Mapping[str, str]) -> tuple[Path, ...] | Failed:
    """Platform archives and the optional theme archive Blender builds from the staged project, its rasm copy, glyph triangles, and dependency wheels."""
    scale, stage, theme = 255, scope / "add-on", anyio.Path(root, "theme")
    icons, wheels, constraints = stage / "icons", stage / "wheels", anyio.Path(scope, "constraints.txt")
    isolated = {**environ, "BLENDER_USER_EXTENSIONS": str(scope / "extensions")}

    class Platform(StrEnum):
        """Blender platform id with the target triple and the `sys_platform`, `platform_machine`, `platform_system`, and `os_name` values uv resolves its wheels for."""

        triple: str
        markers: tuple[str, ...]

        def __new__(cls, value: str, triple: str, *markers: str) -> Self:
            member = str.__new__(cls, value)
            member._value_, member.triple, member.markers = value, triple, markers
            return member

        MACOS_ARM64 = "macos-arm64", "aarch64-apple-darwin", "darwin", "arm64", "Darwin", "posix"
        MACOS_X64 = "macos-x64", "x86_64-apple-darwin", "darwin", "x86_64", "Darwin", "posix"
        WINDOWS_X64 = "windows-x64", "x86_64-pc-windows-msvc", "win32", "AMD64", "Windows", "nt"
        WINDOWS_ARM64 = "windows-arm64", "aarch64-pc-windows-msvc", "win32", "ARM64", "Windows", "nt"
        LINUX_X64 = "linux-x64", "x86_64-unknown-linux-gnu", "linux", "x86_64", "Linux", "posix"

    class Manifest(msgspec.Struct, frozen=True):
        """Manifest fields the pack reads, the platforms Blender splits the archives for."""

        platforms: tuple[Platform, ...]

    def failed(*errors: Error) -> Failed:
        return Failed("blender", tuple(error.text for error in errors))

    async def gathered[T](*steps: Coroutine[object, object, Result[T]]) -> tuple[T, ...] | Failed:
        results = await anyio.gather(*steps)
        errors = tuple(result for result in results if isinstance(result, Error))
        return failed(*errors) if errors else tuple(result for result in results if not isinstance(result, Error))

    async def locked(*command: str) -> Result[Pylock]:
        ran = await executed((*command, "--format", "pylock.toml"), isolated)
        return ran if isinstance(ran, Error) else Pylock.from_dict(tomllib.loads(ran.decode()))

    async def described(package: Package) -> Result[Metadata]:
        match package:
            case Package(name=name, vcs=PackageVcs(url=str() as url, commit_id=commit)) if (repository := httpx2.URL(url)).host == "github.com":
                address, built_folder = f"https://codeload.github.com/{repository.path.strip('/').removesuffix('.git')}/tar.gz/{commit}", scope / "vcs" / name
                if isinstance(archive := await downloaded(client, address, scope / "vcs" / f"{name}.tar.gz"), Error):
                    return archive
                epoch = await anyio.to_thread.run_sync(committed, archive)
                if isinstance(ran := await executed(("uv", "build", "--wheel", "--out-dir", str(built_folder), str(archive)), {**isolated, "SOURCE_DATE_EPOCH": str(epoch)}), Error):
                    return ran
                (wheel,) = [path async for path in anyio.Path(built_folder).glob("*.whl")]
                return await anyio.to_thread.run_sync(cored, Path(await wheel.copy_into(wheels)))
            case Package(name=name, vcs=PackageVcs(url=url)):
                return Error(f"{name} locks {url}, a repository outside github.com")
            case Package(wheels=[PackageWheel(url=str() as url), *_]):
                return await fetched(client, f"{url}.metadata", partial(Metadata.from_email, validate=False))
            case Package(name=name):
                return Error(f"{name} locks neither a wheel nor a repository commit")

    async def resolved(platform: Platform) -> Result[Pylock]:
        excludes = anyio.Path(scope, f"excludes.{platform}.txt")
        markers = dict(zip(("sys_platform", "platform_machine", "platform_system", "os_name"), platform.markers, strict=True))
        await excludes.write_text("".join(f"{name}\n" for name in sorted(accepted(interpreter, requirements, markers) | repositories)), encoding="utf-8")
        compiled = ("pyproject.toml", "--python-version", interpreter.version, "--only-binary", ":all:", "--excludes", str(excludes), "--constraints", str(constraints))
        return await locked("uv", "pip", "compile", *compiled, "--python-platform", platform.triple)

    async def verified(wheel: PackageWheel) -> Result[Path]:
        def digested(path: Path) -> str:
            with path.open("rb") as source:
                return hashlib.file_digest(source, "sha256").hexdigest()

        match wheel:
            case PackageWheel(url=str() as url, hashes={"sha256": expected}):
                if isinstance(target := await downloaded(client, url, wheels / wheel.filename), Error):
                    return target
                digest = await anyio.to_thread.run_sync(digested, target)
                return target if digest == expected else Error(f"{wheel.filename} digests to sha256 {digest} where its lock row records {expected}")
            case _:
                return Error(f"{wheel.filename} locks no url with a sha256 digest")

    if isinstance(variables := environment(BlenderEnvironment, environ), Error):
        return failed(variables)
    application, document = await bundled(variables), tomlkit.parse(await anyio.Path(root, "blender_manifest.toml").read_text(encoding="utf-8"))
    try:
        manifest = msgspec.convert(document.unwrap(), Manifest)
    except msgspec.ValidationError as error:
        return failed(Error(f"{root / 'blender_manifest.toml'} {error}"))
    if await anyio.Path(folder).exists():
        await anyio.to_thread.run_sync(shutil.rmtree, folder)
    await anyio.gather(anyio.Path(folder).mkdir(parents=True), anyio.Path(stage).mkdir(), anyio.Path(scope, "extensions").mkdir())
    await anyio.gather(*[child.copy_into(stage) async for child in anyio.Path(root).iterdir() if child.name not in {theme.name, "tests"}])
    await anyio.gather(anyio.Path(icons).mkdir(exist_ok=True), anyio.Path(wheels).mkdir(exist_ok=True))
    if isinstance(ran := await executed(("uv", "build", "--wheel", "--out-dir", str(scope), "."), isolated), Error):
        return failed(ran)
    (wheel,) = [Path(path) async for path in anyio.Path(scope).glob("*.whl")]
    await anyio.to_thread.run_sync(copied, wheel, stage)
    project = await anyio.to_thread.run_sync(cored, wheel)
    glyphs, record = anyio.Path(scope, "glyphs.json"), anyio.Path(scope, "interpreter.json")
    await glyphs.write_bytes(msgspec.json.encode(await anyio.to_thread.run_sync(outlined, root.parent / icons.name, scale)))
    call = bootstrap("fitout.script", t"pack({str(glyphs)}, {str(icons)}, {str(record)}, {scale})")
    if isinstance(ran := await executed((str(application.executable), "--factory-startup", "--background", "--python-exit-code", "1", "--python-expr", call), isolated), Error):
        return failed(ran)
    interpreter = msgspec.json.decode(await record.read_bytes(), type=Interpreter)
    if isinstance(lock := await locked("uv", "export", "--frozen", "--no-default-groups", "--no-emit-project"), Error):
        return failed(lock)
    if isinstance(cores := await gathered(*map(described, lock.packages)), Failed):
        return cores
    requirements = {canonicalize_name(core.name): tuple(core.requires_dist or ()) for core in (project, *cores)}
    repositories = frozenset(package.name for package in lock.packages if package.vcs is not None)
    await constraints.write_text("".join(f"{package.name}=={package.version}\n" for package in lock.packages if package.vcs is None), encoding="utf-8")
    if isinstance(resolutions := await gathered(*map(resolved, manifest.platforms)), Failed):
        return resolutions
    files = {wheel.filename: wheel for resolution in resolutions for package in resolution.packages if package.wheels is not None for wheel in package.wheels}
    if isinstance(downloads := await gathered(*map(verified, files.values())), Failed):
        return downloads
    document["wheels"] = tomlkit.item(sorted([f"./{wheels.name}/{path.name}" async for path in anyio.Path(wheels).iterdir()])).multiline(multiline=True)
    await anyio.Path(stage, "blender_manifest.toml").write_text(tomlkit.dumps(document), encoding="utf-8")
    themes = [built(isolated, application, Path(theme), "--output-dir", str(folder))] if await theme.is_dir() else []
    if isinstance(builds := await gathered(built(isolated, application, stage, "--split-platforms", "--output-dir", str(folder)), *themes), Failed):
        return builds
    return tuple(sorted([Path(path) async for path in anyio.Path(folder).glob("*.zip")]))


# --- [COMPOSITION] ----------------------------------------------------------------------

app = cyclopts.App(help=__doc__)


@app.command
async def pack(root: Path, folder: Path, /) -> bool:
    """Build a Blender extension project's platform archives into the folder and print their paths or the failures."""
    async with anyio.TemporaryDirectory() as scope, httpx2.AsyncClient(follow_redirects=True) as client:
        result = await packed(root, folder, Path(scope), client, os.environ)
    printed(result)
    return not isinstance(result, Failed)


install = app.command(cyclopts.App(name="install", help="Install a packed project's archives into its host and print the outcome."))


@install.command
async def rhino(folder: Path, /) -> bool:
    """Install the folder's yak package into Rhino and print the outcome."""
    match await Rhino.resolve(os.environ), await packages.packaged(folder):
        case Rhino() as host, Path() as archive:
            result = outcome("rhino", (Header(host.bundle.version, str(host.directory)), *await packages.converged(os.environ, host, archive)))
        case found:
            result = outcome("rhino", tuple(each for each in found if isinstance(each, Error)))
    printed(result)
    return isinstance(result, Applied)


@install.command
async def blender(folder: Path, /) -> bool:
    """Install the folder's zip archives into Blender and print the outcome."""
    if isinstance(variables := environment(BlenderEnvironment, os.environ), Error):
        result = outcome("blender", (variables,))
    else:
        application = await bundled(variables)
        async with anyio.NamedTemporaryFile("w+", encoding="utf-8", delete_on_close=False) as report:
            code = bootstrap("fitout.script", t"install({str(await anyio.Path(folder).resolve())}, {report.name})")
            reply = await bridged(variables.blender_mcp_port, windowed(application), code, msgspec.Raw)
            ran = await executed((str(application.executable), "--background", "--python-exit-code", "1", "--python-expr", code), os.environ) if reply is None else reply
            result = outcome("blender", (*parse(await report.read()), *((ran,) if isinstance(ran, Error) else ())))
    printed(result)
    return isinstance(result, Applied)


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
