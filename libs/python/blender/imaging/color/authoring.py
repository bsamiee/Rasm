"""OCIO configuration authoring: view transform, display, look, and file-rule rows merged into a base config, archived, or installed."""

from collections.abc import Callable
import copy
from functools import partial
import pathlib
from typing import Final

from expression import Option
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.imaging.color.luts import LutFormat
from blender.imaging.color.processors import called
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.stores import committed, files_copied, folder_made

# --- [TYPES] ----------------------------------------------------------------------------

type _Texts = tuple[tuple[str, str], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_CREATE: Final[Callable[[], object]] = PyOpenColorIO.ConfigMergingParameters.Create  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_PREFER_BASE: Final[object] = PyOpenColorIO.ConfigMergingParameters.STRATEGY_PREFER_BASE  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_SET_ROLES: Final[Callable[[object, object], None]] = PyOpenColorIO.ConfigMergingParameters.setRoles  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_SET_COLORSPACES: Final[Callable[[object, object], None]] = PyOpenColorIO.ConfigMergingParameters.setColorspaces  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_MERGE: Final[Callable[[object, PyOpenColorIO.Config, PyOpenColorIO.Config], PyOpenColorIO.Config]] = PyOpenColorIO.ConfigMergingHelpers.MergeConfigs  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_ACTIVE_DISPLAY: Final[Callable[[PyOpenColorIO.Config, str], None]] = PyOpenColorIO.Config.addActiveDisplay  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_ACTIVE_VIEW: Final[Callable[[PyOpenColorIO.Config, str], None]] = PyOpenColorIO.Config.addActiveView  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

# --- [MODELS] ---------------------------------------------------------------------------


class ConfigBuild(msgspec.Struct, frozen=True, kw_only=True):
    """View transform, display, look, and file-rule rows merged into base config, base roles and colorspaces preferred by default."""

    @staticmethod
    def merging() -> object:
        """Returns merge parameters preferring base roles and colorspaces."""
        parameters = _CREATE()
        _SET_ROLES(parameters, _PREFER_BASE)
        _SET_COLORSPACES(parameters, _PREFER_BASE)
        return parameters

    view_transforms: tuple[PyOpenColorIO.ViewTransform, ...]
    displays: tuple[PyOpenColorIO.ColorSpace, ...] = ()
    looks: tuple[PyOpenColorIO.Look, ...] = ()
    inactive: frozenset[str] = frozenset()
    file_rules: PyOpenColorIO.FileRules = msgspec.field(default_factory=PyOpenColorIO.FileRules)
    luts: tuple[pathlib.Path, ...] = ()
    merge: object = msgspec.field(default_factory=merging)

    def staged(self, base: PyOpenColorIO.Config, stage: pathlib.Path) -> Outcome[PyOpenColorIO.Config]:
        """Merges rows into `base` with base files, LUT folders, and generated CLF tables staged in `stage`.

        Returns:
            Merged config, or `color` and `write` faults.
        """
        rows = PyOpenColorIO.Config()
        stage = stage.absolute()
        clf = {row.extension: row.name for row in LutFormat.writable()}["clf"]
        working = base.getWorkingDir()
        copies = (*(((pathlib.Path(working), stage),) if working else ()), *((folder, stage / folder.name) for folder in self.luts))

        def tabled(transform: PyOpenColorIO.Transform) -> tuple[PyOpenColorIO.Transform, _Texts]:
            match transform:
                case PyOpenColorIO.GroupTransform():
                    children = tuple(map(tabled, transform))
                    group = PyOpenColorIO.GroupTransform([child for child, _ in children], direction=transform.getDirection())
                    group.getFormatMetadata().setName(transform.getFormatMetadata().getName())
                    return group, tuple(text for _, texts in children for text in texts)
                case PyOpenColorIO.Lut1DTransform() | PyOpenColorIO.Lut3DTransform():
                    table = PyOpenColorIO.GroupTransform([transform], direction=transform.getDirection())
                    name = f"{rows.getProcessor(table).getCacheID()}.clf"
                    return PyOpenColorIO.FileTransform(src=name, interpolation=transform.getInterpolation(), direction=transform.getDirection()), ((name, table.write(clf, rows)),)
                case _:
                    return transform, ()

        def retabled(transform: PyOpenColorIO.Transform | None, write: Callable[[PyOpenColorIO.Transform], None]) -> _Texts:
            found = Option.of_optional(transform).map(tabled)
            for held, _ in found.to_list():
                write(held)
            return found.map(lambda pair: pair[1]).default_value(())

        def authored() -> _Texts:
            scene = PyOpenColorIO.ColorSpace(PyOpenColorIO.REFERENCE_SPACE_SCENE, name="ACES2065-1")
            display = PyOpenColorIO.ColorSpace(PyOpenColorIO.REFERENCE_SPACE_DISPLAY, name="CIE-XYZ-D65")
            spaces = (scene, display, *copy.deepcopy(self.displays))
            views, looks = tuple(map(copy.deepcopy, self.view_transforms)), tuple(map(copy.deepcopy, self.looks))
            rules, given = copy.deepcopy(self.file_rules), base.getFileRules()
            rules.setDefaultRuleColorSpace(given.getColorSpace(given.getIndexForRule(PyOpenColorIO.DEFAULT_RULE_NAME)))
            rows.setVersion(base.getMajorVersion(), base.getMinorVersion())
            rows.setDefaultViewTransformName(base.getDefaultViewTransformName())
            rows.setFileRules(rules)
            for path in (".", *(folder.name for folder in self.luts)):
                rows.addSearchPath(path)
            texts = (
                *(
                    text
                    for space in spaces
                    for direction in (PyOpenColorIO.COLORSPACE_DIR_TO_REFERENCE, PyOpenColorIO.COLORSPACE_DIR_FROM_REFERENCE)
                    for text in retabled(space.getTransform(direction), partial(space.setTransform, direction=direction))
                ),
                *(
                    text
                    for view in views
                    for direction in (PyOpenColorIO.VIEWTRANSFORM_DIR_TO_REFERENCE, PyOpenColorIO.VIEWTRANSFORM_DIR_FROM_REFERENCE)
                    for text in retabled(view.getTransform(direction), partial(view.setTransform, direction=direction))
                ),
                *(text for look in looks for text in (*retabled(look.getTransform(), look.setTransform), *retabled(look.getInverseTransform(), look.setInverseTransform))),
            )
            for space in spaces:
                rows.addColorSpace(space)
            rows.setRole(PyOpenColorIO.ROLE_INTERCHANGE_SCENE, scene.getName())
            rows.setRole(PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, display.getName())
            for view in views:
                rows.addViewTransform(view)
            for look in looks:
                rows.addLook(look)
            return texts

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

        return called(authored, str(stage)).bind(
            lambda texts: paired(
                called(_MERGE, str(stage), self.merge, base, rows).bind(lambda merged: called(viewed, str(stage), merged)),
                folder_made(stage).bind(
                    lambda _: paired(
                        files_copied(copies),
                        accumulated(
                            catch((stage / name).write_text, exception=OSError)(text, encoding="utf-8", newline="").map_error(partial(Fault.of, Reason.UNWRITABLE, str(stage)))
                            for name, text in dict(texts).items()
                        ),
                    )
                ),
            ).map(lambda pair: pair[0])
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
                .map(lambda _: None)
                .map_error(partial(Fault.of, Reason.UNWRITABLE, str(folder)))
            )

        return committed(folder, lambda stage: self.staged(base, stage).bind(lambda config: replaced(stage, config))).map(lambda installed: installed / "config.ocio")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ConfigBuild"]
