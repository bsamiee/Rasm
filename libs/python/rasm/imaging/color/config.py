"""Color configuration sources and required role resolution."""

from collections.abc import Callable, Iterable
from functools import partial

from expression import Error, Ok, Option
import PyOpenColorIO

from rasm.imaging.faults import ImagingConfigFault, ImagingResult, ImagingUnresolvedFault
from rasm.imaging.parameters import Case

# --- [MODELS] ---------------------------------------------------------------------------


class BuiltinConfig(Case, frozen=True, kw_only=True, tag=1):
    """Built-in configuration identified by its registry name."""

    title = "Built-in"
    name: str


class ConfigFile(Case, frozen=True, kw_only=True, tag=2):
    """Configuration file, OCIO archive, or built-in URI."""

    title = "File"
    path: str


class EnvironmentConfig(Case, frozen=True, tag=3):
    """Configuration selected by the process OCIO variable."""

    title = "Environment"


type ConfigSource = BuiltinConfig | ConfigFile | EnvironmentConfig

# --- [OPERATIONS] -----------------------------------------------------------------------


def admitted(config: PyOpenColorIO.Config, spaces: Iterable[str]) -> ImagingResult[PyOpenColorIO.Config]:
    """Returns a configuration resolving required roles and declared process spaces."""
    names = dict.fromkeys((PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_SCENE, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, PyOpenColorIO.ROLE_DATA, *spaces))
    unresolved = tuple(name for name in names if Option.of_optional(config.getColorSpace(name)).is_none())
    return Error((ImagingUnresolvedFault(request=config, error=unresolved),)) if unresolved else Ok(config)


def opened(source: ConfigSource, spaces: Iterable[str]) -> ImagingResult[PyOpenColorIO.Config]:
    """Opens an external configuration and resolves imaging's required spaces."""
    create: Callable[[], PyOpenColorIO.Config]
    match source:
        case BuiltinConfig(name=name):
            request, create = name, partial(PyOpenColorIO.Config.CreateFromBuiltinConfig, name)
        case ConfigFile(path=path):
            request, create = path, partial(PyOpenColorIO.Config.CreateFromFile, path)
        case EnvironmentConfig():
            request, create = PyOpenColorIO.OCIO_CONFIG_ENVVAR, PyOpenColorIO.Config.CreateFromEnv
    try:
        return admitted(create(), spaces)
    except (PyOpenColorIO.Exception, PyOpenColorIO.ExceptionMissingFile) as error:
        return Error((ImagingConfigFault(request=request, error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BuiltinConfig", "ConfigFile", "ConfigSource", "EnvironmentConfig", "admitted", "opened"]
