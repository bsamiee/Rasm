"""Color configuration sources."""

from functools import partial

from expression import Error, Ok
import PyOpenColorIO

from blender.imaging.faults import ImagingReason
from blender.records.faults import Fault, Outcome
from blender.records.fields import Case

# --- [MODELS] ---------------------------------------------------------------------------


class ConfigFile(Case, frozen=True, kw_only=True):
    """Configuration file, OCIO archive, or `ocio://` built-in URI."""

    path: str


class EnvironmentConfig(Case, frozen=True):
    """Configuration the `OCIO` variable names."""


type ConfigSource = ConfigFile | EnvironmentConfig

# --- [OPERATIONS] -----------------------------------------------------------------------


def opened(source: ConfigSource) -> Outcome[PyOpenColorIO.Config]:
    """Opens `source`, a color fault for a configuration OCIO cannot read."""
    match source:
        case ConfigFile(path=path):
            subject, load = path, partial(PyOpenColorIO.Config.CreateFromFile, path)
        case EnvironmentConfig():
            subject, load = PyOpenColorIO.OCIO_CONFIG_ENVVAR, partial(PyOpenColorIO.Config.CreateFromEnv)
    try:
        return Ok(load())
    except (PyOpenColorIO.Exception, PyOpenColorIO.ExceptionMissingFile) as error:
        return Error((Fault(reason=ImagingReason.COLOR, subject=subject, cause=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ConfigFile", "ConfigSource", "EnvironmentConfig", "opened"]
