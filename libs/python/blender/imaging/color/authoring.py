"""Native OCIO formation rows, merged configurations, archives, and installations."""

import copy
from enum import auto, Enum
import pathlib
import shutil
import tempfile
from typing import Self

from expression import Error, Ok, Option
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.imaging.color.luts import LutFormat
from blender.imaging.faults import ImagingConfigFault, ImagingFaults, ImagingResult, ImagingWriteFault, paired
from blender.imaging.files.images import published

# --- [TYPES] ----------------------------------------------------------------------------


class DynamicRange(Enum):
    """Range a formation produces or a display represents."""

    STANDARD = auto()
    HIGH = auto()

    @staticmethod
    def absolute(space: PyOpenColorIO.ColorSpace) -> bool:
        """Returns whether native interop metadata identifies absolute PQ luminance.

        Args:
            space: Display color space.
        """
        return space.getInteropID().rpartition(":")[2].startswith("pq_")  # type: ignore[attr-defined, no-any-return]  # ty: ignore[unresolved-attribute]


def dynamic_range_of(space: PyOpenColorIO.ColorSpace) -> DynamicRange:
    """Reads Blender's HDR classification from native encoding.

    Args:
        space: Display color space.

    Returns:
        HIGH for HDR or extended-range video; STANDARD otherwise.
    """
    return DynamicRange.HIGH if space.getEncoding() in {"hdr-video", "edr-video"} else DynamicRange.STANDARD


# --- [MODELS] ---------------------------------------------------------------------------


class Formation(msgspec.Struct, frozen=True, kw_only=True):
    """Scene-to-display view transform with its output range and scoped looks."""

    view: PyOpenColorIO.ViewTransform
    dynamic_range: DynamicRange = DynamicRange.STANDARD
    looks: tuple[PyOpenColorIO.Look, ...] = ()

    @classmethod
    def of(cls, name: str, description: str, transform: PyOpenColorIO.Transform, space: str, dynamic_range: DynamicRange = DynamicRange.STANDARD) -> Self:
        """Brackets an operation in its primaries between scene and display interchange.

        Args:
            name: View transform and shared view name.
            description: Text shown by a host.
            transform: Scene-to-display-linear operation in `space` primaries.
            space: Color space or role the operation reads and writes.
            dynamic_range: Output range.

        Returns:
            A formation with inline tables retained for staging.
        """
        return cls(
            view=PyOpenColorIO.ViewTransform(
                referenceSpace=PyOpenColorIO.REFERENCE_SPACE_SCENE,
                name=name,
                description=description,
                fromReference=PyOpenColorIO.GroupTransform([
                    PyOpenColorIO.ColorSpaceTransform(src=PyOpenColorIO.ROLE_INTERCHANGE_SCENE, dst=space),
                    transform,
                    PyOpenColorIO.ColorSpaceTransform(src=space, dst=PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY),
                ]),
            ),
            dynamic_range=dynamic_range,
        )


class ConfigBuild(msgspec.Struct, frozen=True, kw_only=True):
    """Native formation, display, look, and file-rule rows merged into a base config."""

    # --- [ROWS]

    @staticmethod
    def merging() -> PyOpenColorIO.ConfigMergingParameters:  # type: ignore[name-defined]  # ty: ignore[unresolved-attribute]
        """Returns native merge defaults with base roles and color spaces preferred."""
        parameters = PyOpenColorIO.ConfigMergingParameters.Create()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        parameters.setRoles(PyOpenColorIO.ConfigMergingParameters.STRATEGY_PREFER_BASE)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        parameters.setColorspaces(PyOpenColorIO.ConfigMergingParameters.STRATEGY_PREFER_BASE)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return parameters

    formations: tuple[Formation, ...]
    displays: tuple[PyOpenColorIO.ColorSpace, ...] = ()
    looks: tuple[PyOpenColorIO.Look, ...] = ()
    inactive: frozenset[str] = frozenset()
    file_rules: PyOpenColorIO.FileRules = msgspec.field(default_factory=PyOpenColorIO.FileRules)
    luts: tuple[pathlib.Path, ...] = ()
    merge: PyOpenColorIO.ConfigMergingParameters = msgspec.field(default_factory=merging)  # type: ignore[name-defined]  # ty: ignore[unresolved-attribute]

    # --- [OUTPUTS]

    def staged(self, base: PyOpenColorIO.Config, stage: pathlib.Path) -> ImagingResult[PyOpenColorIO.Config]:
        """Stages native rows and inline tables into a merged configuration.

        Args:
            base: Config supplying reference spaces, roles, rules, and existing views.
            stage: Folder receiving base files, LUT folders, and generated CLFs.

        Returns:
            The merged config, or `config` and `write` faults from native authoring and file operations.
        """
        rows = PyOpenColorIO.Config()
        request = rows
        tables: dict[str, str] = {}
        stage = stage.absolute()
        clf = next(row for row in LutFormat.writable() if row.extension == "clf")

        def tabled(transform: PyOpenColorIO.Transform) -> PyOpenColorIO.Transform:
            match transform:
                case PyOpenColorIO.GroupTransform():
                    group = PyOpenColorIO.GroupTransform([tabled(child) for child in transform], direction=transform.getDirection())
                    group.getFormatMetadata().setName(transform.getFormatMetadata().getName())
                    return group
                case PyOpenColorIO.Lut1DTransform() | PyOpenColorIO.Lut3DTransform():
                    table = PyOpenColorIO.GroupTransform([transform], direction=transform.getDirection())
                    name = f"{rows.getProcessor(table).getCacheID()}.{clf.extension}"
                    tables[name] = table.write(clf.name, rows)
                    return PyOpenColorIO.FileTransform(src=name, interpolation=transform.getInterpolation(), direction=transform.getDirection())
                case _:
                    return transform

        try:
            scene = PyOpenColorIO.ColorSpace(PyOpenColorIO.REFERENCE_SPACE_SCENE, name="ACES2065-1")
            display = PyOpenColorIO.ColorSpace(PyOpenColorIO.REFERENCE_SPACE_DISPLAY, name="CIE-XYZ-D65")
            rules, given = copy.deepcopy(self.file_rules), base.getFileRules()
            rules.setDefaultRuleColorSpace(given.getColorSpace(given.getIndexForRule(PyOpenColorIO.DEFAULT_RULE_NAME)))
            rows.setVersion(base.getMajorVersion(), base.getMinorVersion())
            rows.setDefaultViewTransformName(base.getDefaultViewTransformName())
            rows.setFileRules(rules)
            for path in (".", *(folder.name for folder in self.luts)):
                rows.addSearchPath(path)
            for space in (scene, display, *copy.deepcopy(self.displays)):
                for space_direction in (PyOpenColorIO.COLORSPACE_DIR_TO_REFERENCE, PyOpenColorIO.COLORSPACE_DIR_FROM_REFERENCE):
                    for transform in Option.of_optional(space.getTransform(space_direction)).to_list():
                        space.setTransform(tabled(transform), space_direction)
                rows.addColorSpace(space)
            rows.setRole(PyOpenColorIO.ROLE_INTERCHANGE_SCENE, scene.getName())
            rows.setRole(PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, display.getName())
            for formation in self.formations:
                view = copy.deepcopy(formation.view)
                for view_direction in (PyOpenColorIO.VIEWTRANSFORM_DIR_TO_REFERENCE, PyOpenColorIO.VIEWTRANSFORM_DIR_FROM_REFERENCE):
                    for transform in Option.of_optional(view.getTransform(view_direction)).to_list():
                        view.setTransform(tabled(transform), view_direction)
                rows.addViewTransform(view)
            for original, name in (
                *((look, f"{formation.view.getName()} - {look.getName()}") for formation in self.formations for look in formation.looks),
                *((look, look.getName()) for look in self.looks),
            ):
                look = copy.deepcopy(original)
                look.setName(name)
                for transform in Option.of_optional(look.getTransform()).to_list():
                    look.setTransform(tabled(transform))
                for transform in Option.of_optional(look.getInverseTransform()).to_list():
                    look.setInverseTransform(tabled(transform))
                rows.addLook(look)
            stage.mkdir(parents=True, exist_ok=True)
            working = base.getWorkingDir()
            for source, target in (*(((pathlib.Path(working), stage),) if working else ()), *((folder, stage / folder.name) for folder in self.luts)):
                shutil.copytree(source, target, dirs_exist_ok=True)
            for name, text in tables.items():
                (stage / name).write_text(text, encoding="utf-8", newline="")
            request = PyOpenColorIO.ConfigMergingHelpers.MergeConfigs(self.merge, base, rows)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            built = tuple(space.getName() for space in self.displays)
            names = tuple(formation.view.getName() for formation in self.formations)
            spaces = {name: space for name in dict.fromkeys((*request.getDisplaysAll(), *built)) for space in Option.of_optional(request.getColorSpace(name)).to_list()}
            for name in names:
                request.addSharedView(name, name, PyOpenColorIO.OCIO_VIEW_USE_DISPLAY_NAME)
            for display_name, name in (
                (display_name, formation.view.getName())
                for formation in self.formations
                for display_name, space in spaces.items()
                if formation.dynamic_range is DynamicRange.STANDARD or dynamic_range_of(space) is DynamicRange.HIGH
            ):
                request.addDisplaySharedView(display_name, name)
            for name in built if request.getActiveDisplays() else ():
                request.addActiveDisplay(name)
            for name in names if request.getActiveViews() else ():
                request.addActiveView(name)
            separator = request.getFamilySeparator()
            request.setInactiveColorSpaces(
                ", ".join(
                    space.getName()
                    for space in request.getColorSpaces(PyOpenColorIO.SEARCH_REFERENCE_SPACE_ALL, PyOpenColorIO.COLORSPACE_ALL)
                    if request.isInactiveColorSpace(space.getName()) or any(space.getFamily() == family or space.getFamily().startswith(family + separator) for family in self.inactive)
                )
            )
            request.setWorkingDir(str(stage))
            return Ok(request)
        except (PyOpenColorIO.Exception, PyOpenColorIO.ExceptionMissingFile) as error:
            return Error((ImagingConfigFault(request=request, error=error),))
        except OSError as error:
            return Error((ImagingWriteFault(request=stage, error=error),))

    def archived(self, base: PyOpenColorIO.Config, path: pathlib.Path) -> ImagingResult[pathlib.Path]:
        """Stages and atomically publishes an OCIO archive.

        Args:
            base: Config the rows merge into.
            path: Archive destination.

        Returns:
            The published path, or `config` and `write` faults including resource cleanup failures.
        """

        def encoded(config: PyOpenColorIO.Config) -> ImagingResult[pathlib.Path]:
            def archive(destination: str) -> ImagingResult[None]:
                try:
                    config.archive(destination)
                    return Ok(None)
                except (PyOpenColorIO.Exception, PyOpenColorIO.ExceptionMissingFile) as error:
                    return Error((ImagingConfigFault(request=config, error=error),))

            return published(str(path), archive).map(pathlib.Path)

        try:
            stage = tempfile.TemporaryDirectory()
        except OSError as error:
            return Error((ImagingWriteFault(request=path, error=error),))
        try:
            result = self.staged(base, pathlib.Path(stage.name)).bind(encoded)
        finally:
            removed = catch(stage.cleanup, exception=OSError)().map_error(lambda error: (ImagingWriteFault(request=path, error=error),))
        return paired(result, removed).map(lambda pair: pair[0])

    def installed(self, base: PyOpenColorIO.Config, folder: pathlib.Path) -> ImagingResult[pathlib.Path]:
        """Replaces a configuration folder whole with a staged config and all its files.

        Args:
            base: Config the rows merge into.
            folder: Installation folder a host reads at its next start.

        Returns:
            The installed config path, or `config` and `write` faults including resource cleanup failures.
        """

        def refused(error: OSError) -> ImagingFaults:
            return (ImagingWriteFault(request=folder, error=error),)

        def replaced(config: PyOpenColorIO.Config) -> ImagingResult[pathlib.Path]:
            path = stage / "config.ocio"
            return (
                catch(path.write_text, exception=OSError)(config.serialize(), encoding="utf-8", newline="")
                .bind(lambda _: catch(shutil.rmtree, exception=OSError)(folder).or_else_with(lambda error: Ok(None) if isinstance(error, FileNotFoundError) else Error(error)))
                .bind(lambda _: catch(stage.rename, exception=OSError)(folder))
                .map(lambda installed: installed / path.name)
                .map_error(refused)
            )

        try:
            folder.parent.mkdir(parents=True, exist_ok=True)
            scratch = tempfile.TemporaryDirectory(dir=folder.parent)
        except OSError as error:
            return Error(refused(error))
        try:
            stage = pathlib.Path(scratch.name) / folder.name
            result = self.staged(base, stage).bind(replaced)
        finally:
            removed = catch(scratch.cleanup, exception=OSError)().map_error(refused)
        return paired(result, removed).map(lambda pair: pair[0])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ConfigBuild", "DynamicRange", "Formation", "dynamic_range_of"]
