"""Color configuration sources and required role resolution."""

from collections.abc import Callable, Iterable
from functools import partial

from expression import Error, Ok, Option
import PyOpenColorIO

from blender.imaging.faults import colored, ImagingResult, ImagingUnresolvedFault
from blender.imaging.parameters import Case

# --- [MODELS] ---------------------------------------------------------------------------


class BuiltinConfig(Case, frozen=True, kw_only=True, tag=1):
    """Built-in configuration by registry name."""

    title = "Built-in"
    name: str


class ConfigFile(Case, frozen=True, kw_only=True, tag=2):
    """Configuration file, OCIO archive, or built-in URI."""

    title = "File"
    path: str


class EnvironmentConfig(Case, frozen=True, tag=3):
    """Configuration the `OCIO` variable names."""

    title = "Environment"


type ConfigSource = BuiltinConfig | ConfigFile | EnvironmentConfig

# --- [OPERATIONS] -----------------------------------------------------------------------


def resolved(config: PyOpenColorIO.Config, spaces: Iterable[str]) -> ImagingResult[PyOpenColorIO.Config]:
    """Returns `config`, or unresolved fault naming missing roles and `spaces`."""
    names = dict.fromkeys((PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_SCENE, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, PyOpenColorIO.ROLE_DATA, *spaces))
    unresolved = tuple(name for name in names if Option.of_optional(config.getColorSpace(name)).is_none())
    return Error((ImagingUnresolvedFault(request=config, error=unresolved),)) if unresolved else Ok(config)


def opened(source: ConfigSource, spaces: Iterable[str]) -> ImagingResult[PyOpenColorIO.Config]:
    """Opens `source` and resolves required roles and `spaces`."""
    create: Callable[[], PyOpenColorIO.Config]
    match source:
        case BuiltinConfig(name=name):
            request, create = name, partial(PyOpenColorIO.Config.CreateFromBuiltinConfig, name)
        case ConfigFile(path=path):
            request, create = path, partial(PyOpenColorIO.Config.CreateFromFile, path)
        case EnvironmentConfig():
            request, create = PyOpenColorIO.OCIO_CONFIG_ENVVAR, PyOpenColorIO.Config.CreateFromEnv
    return colored(request, create).bind(lambda config: resolved(config, spaces))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BuiltinConfig", "ConfigFile", "ConfigSource", "EnvironmentConfig", "opened", "resolved"]
