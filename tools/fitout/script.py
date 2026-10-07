# ty: ignore[invalid-argument-type, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="arg-type, import-not-found, union-attr"
"""Blender side of extension packing and installation."""

from collections.abc import Iterator
from contextlib import ExitStack
from functools import partial
from pathlib import Path
import zipfile

from bl_pkg.cli.blender_ext import pkg_manifest_from_archive_and_validate, PkgManifest, platform_from_this_system
import bpy

from interface.blender.rows import Archive, Archived, JSON, stamped
from interface.blender.script import addons
from interface.report import Action, Change, converged, Error, Header, Item, reported, Row, subscript

# --- [TYPES] ----------------------------------------------------------------------------

type Point = tuple[float, float]
type Contours = tuple[tuple[tuple[Point, Point, Point], ...], ...]

# --- [COMPOSITION] ----------------------------------------------------------------------


def pack(glyphs: str, icons: str, interpreter: str, scale: int) -> None:
    """Write the interpreter record and one triangle file per glyph, its byte coordinates spanning the scale."""

    def triangles(contours: Contours) -> tuple[tuple[tuple[int, ...], ...], ...]:
        """Byte triangles of positive area filling the closed Bézier contours, with Blender resources released on exit."""
        with ExitStack() as resources:
            curve = bpy.data.curves.new("glyph", "CURVE")
            resources.callback(bpy.data.curves.remove, curve)
            curve.dimensions, curve.fill_mode = "2D", "FRONT"
            for contour in contours:
                spline = curve.splines.new("BEZIER")
                spline.use_cyclic_u = True
                spline.bezier_points.add(len(contour) - 1)
                for point, (co, left, right) in zip(spline.bezier_points, contour, strict=True):
                    point.handle_left_type = point.handle_right_type = "FREE"
                    point.co.xy, point.handle_left.xy, point.handle_right.xy = co, left, right
            holder = bpy.data.objects.new(curve.name, curve)
            resources.callback(bpy.data.objects.remove, holder, do_unlink=True)
            bpy.context.scene.collection.objects.link(holder)
            evaluated = holder.evaluated_get(bpy.context.evaluated_depsgraph_get())
            mesh = evaluated.to_mesh()
            resources.callback(evaluated.to_mesh_clear)
            mesh.calc_loop_triangles()
            placed = (tuple(tuple(round((axis + 1) * scale / 2) for axis in mesh.vertices[index].co.xy) for index in triangle.vertices) for triangle in mesh.loop_triangles)
            return tuple((a, b, c) for a, b, c in placed if (a[0] - b[0]) * (b[1] - c[1]) + (a[1] - b[1]) * (c[0] - b[0]) > 0)

    header = b"VCO\x00" + bytes((scale, scale, 0, 0))
    Path(interpreter).write_text(JSON.dumps(addons.interpreter()), encoding="utf-8")
    for stem, groups in JSON.loads(Path(glyphs).read_text(encoding="utf-8"), dict[str, dict[float, Contours]]).items():
        kept = [(alpha, triangle) for alpha, contours in groups.items() for triangle in triangles(contours)]
        coordinates = bytes(coordinate for _, triangle in kept for point in triangle for coordinate in point)
        Path(icons, f"{stem}.dat").write_bytes(header + coordinates + b"".join(bytes((255, 255, 255, round(alpha * 255))) * len(triangle) for alpha, triangle in kept))


def install(folder: str, report: str) -> None:
    """Install this system's archives into Blender's user repository and write the report of changes or failure."""

    def declared() -> Iterator[Item]:
        """Header line, then per archive for this platform an error line naming Blender's manifest refusal or a row installing an add-on enabled or, the one other type Blender validates, a theme disabled where installed stamps differ from archive stamps."""
        preferences = bpy.context.preferences
        repository, platform = addons.user_repository(preferences), platform_from_this_system()
        yield Header(bpy.app.version_string, repository.directory)
        for path in sorted(Path(folder).glob("*.zip")):
            with zipfile.ZipFile(path) as archive:
                built = Archive(str(path), stamped(archive))
            match pkg_manifest_from_archive_and_validate(str(path), strict=False):
                case str() as refusal:
                    yield Error(f"{subscript('packages', path.name)} {refusal}")
                case PkgManifest(platforms=[_, *_] as platforms) if platform not in platforms:
                    continue
                case PkgManifest(type="add-on", id=identifier):
                    yield addons.package(preferences, repository.module, identifier, Archived(identifier, built, ()))
                case theme:
                    yield Action(
                        label=subscript("packages", theme.id),
                        read=partial(addons.stamped, Path(repository.directory, theme.id)),
                        act=partial(addons.extension_command, partial(bpy.ops.extensions.package_install_files, filepath=built.path, repo=repository.module, enable_on_install=False)),
                        target=built.stamp,
                    )

    with reported(report) as body:
        try:
            for item in declared():
                body.extend(converged(item) if isinstance(item, Row) else (item,))
        finally:
            if any(isinstance(line, Change) for line in body):
                bpy.ops.wm.save_userpref()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install", "pack"]
