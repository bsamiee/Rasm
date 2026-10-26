"""Color configuration sources and required role resolution."""

from collections.abc import Iterable
from functools import partial

from expression import Error, Ok, Option
import PyOpenColorIO

from blender.imaging.faults import colored, ImagingResult, ImagingUnresolvedFault
from blender.records.fields import Case

# --- [MODELS] ---------------------------------------------------------------------------


class ConfigFile(Case, frozen=True, kw_only=True):
    """Configuration file, OCIO archive, or `ocio://` built-in URI."""

    title = "File"
    path: str


class EnvironmentConfig(Case, frozen=True):
    """Configuration the `OCIO` variable names."""

    title = "Environment"


type ConfigSource = ConfigFile | EnvironmentConfig

# --- [OPERATIONS] -----------------------------------------------------------------------


def resolved(config: PyOpenColorIO.Config, spaces: Iterable[str]) -> ImagingResult[PyOpenColorIO.Config]:
    """Returns `config`, or unresolved fault naming missing roles and `spaces`."""
    names = dict.fromkeys((PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_SCENE, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, PyOpenColorIO.ROLE_DATA, *spaces))
    unresolved = tuple(name for name in names if Option.of_optional(config.getColorSpace(name)).is_none())
    return Error((ImagingUnresolvedFault(request=config, error=unresolved),)) if unresolved else Ok(config)


def opened(source: ConfigSource, spaces: Iterable[str]) -> ImagingResult[PyOpenColorIO.Config]:
    """Opens `source` and resolves required roles and `spaces`."""
    match source:
        case ConfigFile(path=path):
            built = colored(path, partial(PyOpenColorIO.Config.CreateFromFile, path))
        case EnvironmentConfig():
            built = colored(PyOpenColorIO.OCIO_CONFIG_ENVVAR, PyOpenColorIO.Config.CreateFromEnv)
    return built.bind(lambda config: resolved(config, spaces))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ConfigFile", "ConfigSource", "EnvironmentConfig", "opened", "resolved"]
