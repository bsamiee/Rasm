"""OCIO configuration sources, view transform rows, merged configs, archives, and installations."""

from collections.abc import Callable
import copy
from enum import auto, Enum
import pathlib
import shutil
from typing import Any, Final

from expression import Error, Ok, Option
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.imaging.color.luts import LutFormat
from blender.imaging.color.processors import called, interop
from blender.imaging.faults import ImagingReason
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case
from blender.records.stores import committed, files_copied, folder_made

# --- [TYPES] ----------------------------------------------------------------------------


class DynamicRange(Enum):
    """Range a display represents."""

    STANDARD = auto()
    HIGH = auto()

    @staticmethod
    def of(space: PyOpenColorIO.ColorSpace) -> "DynamicRange":
        """Classifies display colorspace by its OCIO encoding, HIGH for hdr-video or edr-video."""
        return DynamicRange.HIGH if space.getEncoding() in {"hdr-video", "edr-video"} else DynamicRange.STANDARD

    @staticmethod
    def absolute(space: PyOpenColorIO.ColorSpace) -> bool:
        """Returns whether display colorspace interop ID names PQ luminance."""
        return interop(space).rpartition(":")[2].startswith("pq_")


# --- [CONSTANTS] ------------------------------------------------------------------------

_MERGING: Final[type[Any]] = PyOpenColorIO.ConfigMergingParameters  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_MERGE: Final[Callable[..., PyOpenColorIO.Config]] = PyOpenColorIO.ConfigMergingHelpers.MergeConfigs  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_ACTIVE_DISPLAY: Final[Callable[[PyOpenColorIO.Config, str], None]] = PyOpenColorIO.Config.addActiveDisplay  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_ACTIVE_VIEW: Final[Callable[[PyOpenColorIO.Config, str], None]] = PyOpenColorIO.Config.addActiveView  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [SOURCES]


class ConfigFile(Case, frozen=True, kw_only=True):
    """Configuration file, OCIO archive, or `ocio://` built-in URI."""

    path: str

    def opened(self) -> Outcome[PyOpenColorIO.Config]:
        """Opens the configuration, a color fault for one OCIO cannot read."""
        return called(PyOpenColorIO.Config.CreateFromFile, self.path, self.path)


class EnvironmentConfig(Case, frozen=True):
    """Configuration the `OCIO` variable names."""

    @staticmethod
    def opened() -> Outcome[PyOpenColorIO.Config]:
        """Opens the configuration, a color fault for one OCIO cannot read."""
        return called(PyOpenColorIO.Config.CreateFromEnv, PyOpenColorIO.OCIO_CONFIG_ENVVAR)


# --- [BUILDS]


class ConfigBuild(msgspec.Struct, frozen=True, kw_only=True):
    """View transform, display, look, and file-rule rows merged into base config."""

    # --- [ROWS]

    @staticmethod
    def merging() -> object:
        """Returns merge defaults preferring base roles and colorspaces."""
        parameters = _MERGING.Create()
        parameters.setRoles(_MERGING.STRATEGY_PREFER_BASE)
        parameters.setColorspaces(_MERGING.STRATEGY_PREFER_BASE)
        return parameters

    view_transforms: tuple[PyOpenColorIO.ViewTransform, ...]
    displays: tuple[PyOpenColorIO.ColorSpace, ...] = ()
    looks: tuple[PyOpenColorIO.Look, ...] = ()
    inactive: frozenset[str] = frozenset()
    file_rules: PyOpenColorIO.FileRules = msgspec.field(default_factory=PyOpenColorIO.FileRules)
    luts: tuple[pathlib.Path, ...] = ()
    merge: object = msgspec.field(default_factory=merging)

    # --- [OUTPUTS]

    def staged(self, base: PyOpenColorIO.Config, stage: pathlib.Path) -> Outcome[PyOpenColorIO.Config]:
        """Merges rows into `base` with base files, LUT folders, and generated CLF tables staged in `stage`.

        Returns:
            Merged config, or `color` and `write` faults.
        """
        rows = PyOpenColorIO.Config()
        tables: dict[str, str] = {}
        stage = stage.absolute()
        clf = next(row for row in LutFormat.writable() if row.extension == "clf")
        working = base.getWorkingDir()
        copies = (*(((pathlib.Path(working), stage),) if working else ()), *((folder, stage / folder.name) for folder in self.luts))

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

        @catch(exception=PyOpenColorIO.Exception)
        def authored() -> None:
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
            for row in self.view_transforms:
                view = copy.deepcopy(row)
                for view_direction in (PyOpenColorIO.VIEWTRANSFORM_DIR_TO_REFERENCE, PyOpenColorIO.VIEWTRANSFORM_DIR_FROM_REFERENCE):
                    for transform in Option.of_optional(view.getTransform(view_direction)).to_list():
                        view.setTransform(tabled(transform), view_direction)
                rows.addViewTransform(view)
            for original in self.looks:
                look = copy.deepcopy(original)
                for transform in Option.of_optional(look.getTransform()).to_list():
                    look.setTransform(tabled(transform))
                for transform in Option.of_optional(look.getInverseTransform()).to_list():
                    look.setInverseTransform(tabled(transform))
                rows.addLook(look)

        @catch(exception=PyOpenColorIO.Exception)
        def viewed(request: PyOpenColorIO.Config) -> PyOpenColorIO.Config:
            built = tuple(space.getName() for space in self.displays)
            names = tuple(view.getName() for view in self.view_transforms)
            for name in names:
                request.addSharedView(name, name, PyOpenColorIO.OCIO_VIEW_USE_DISPLAY_NAME)
            for display_name in (name for name in dict.fromkeys((*request.getDisplaysAll(), *built)) if Option.of_optional(request.getColorSpace(name)).is_some()):
                for name in names:
                    request.addDisplaySharedView(display_name, name)
            for name in built if request.getActiveDisplays() else ():
                _ACTIVE_DISPLAY(request, name)
            for name in names if request.getActiveViews() else ():
                _ACTIVE_VIEW(request, name)
            separator = request.getFamilySeparator()
            request.setInactiveColorSpaces(
                ", ".join(
                    space.getName()
                    for space in request.getColorSpaces(PyOpenColorIO.SEARCH_REFERENCE_SPACE_ALL, PyOpenColorIO.COLORSPACE_ALL)
                    if request.isInactiveColorSpace(space.getName()) or any(space.getFamily() == family or space.getFamily().startswith(family + separator) for family in self.inactive)
                )
            )
            request.setWorkingDir(str(stage))
            return request

        def unauthored(error: PyOpenColorIO.Exception) -> Faults:
            return (Fault(reason=ImagingReason.COLOR, subject=str(stage), cause=error),)

        return (
            authored()
            .map_error(unauthored)
            .bind(
                lambda _: paired(
                    catch(_MERGE, exception=PyOpenColorIO.Exception)(self.merge, base, rows).bind(viewed).map_error(unauthored),
                    folder_made(stage).bind(
                        lambda _: paired(
                            files_copied(copies),
                            accumulated(
                                catch((stage / name).write_text, exception=OSError)(text, encoding="utf-8", newline="").map_error(
                                    lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(stage), cause=error),)
                                )
                                for name, text in tables.items()
                            ),
                        )
                    ),
                ).map(lambda pair: pair[0])
            )
        )

    def archived(self, base: PyOpenColorIO.Config, path: pathlib.Path) -> Outcome[pathlib.Path]:
        """Stages rows merged into `base` and atomically publishes them as an OCIO archive at `path`.

        Returns:
            Published path, or `color` and `write` faults.
        """
        return committed(path, lambda stage: self.staged(base, stage.parent).bind(lambda config: called(config.archive, str(path), str(stage))))

    def installed(self, base: PyOpenColorIO.Config, folder: pathlib.Path) -> Outcome[pathlib.Path]:
        """Replaces `folder` whole with rows merged into `base` and their files.

        Returns:
            Installed config path, or `color` and `write` faults.
        """

        def replaced(stage: pathlib.Path, config: PyOpenColorIO.Config) -> Outcome[None]:
            return (
                catch((stage / "config.ocio").write_text, exception=OSError)(config.serialize(), encoding="utf-8", newline="")
                .bind(lambda _: catch(shutil.rmtree, exception=OSError)(folder).or_else_with(lambda error: Ok(None) if isinstance(error, FileNotFoundError) else Error(error)))
                .map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(folder), cause=error),))
            )

        return committed(folder, lambda stage: self.staged(base, stage).bind(lambda config: replaced(stage, config))).map(lambda installed: installed / "config.ocio")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ConfigBuild", "ConfigFile", "DynamicRange", "EnvironmentConfig"]
