# ty: ignore[unresolved-import]
# mypy: disable-error-code="attr-defined"
# ruff: file-ignore[import-outside-top-level]
"""Window layout, declared display mode, and startup handshake that the host and the Rhino side share, run at launch through `_-ScriptEditor _Run`."""

from configparser import ConfigParser
from enum import auto, StrEnum
from pathlib import Path
from typing import Final
import uuid

# --- [TYPES] ----------------------------------------------------------------------------


class Panel(StrEnum):
    """Registered panel type ids of the right-hand column's panels, the panels that open on demand and return there, the Osnap bar's Osnap and Selection Filters, and the id of the Command History dock bar."""

    PROPERTIES = "34ffb674-c504-49d9-9fcd-99cc811dcda2"
    NAMED_VIEWS = "77d33034-194d-4cd5-957c-730d9a9eac50"
    LAYOUTS = "562bda2a-184f-4b22-9607-79d992f28557"
    BLOCK_DEFINITIONS = "6b6ffd64-c279-4b45-9959-e7e5a8eef806"
    MATERIALS = "6df2a957-f12d-42ea-9fa6-95d7920c1b76"
    SUN = "1012681e-d276-49d3-9cd9-7de92dc2404a"
    DISPLAY = "b68e9e9f-c79c-473c-a7ef-846a11dc4e7b"
    SNAPSHOTS = "f4424a46-8281-430a-b03d-911dc9b40294"
    LAYERS = "3610bf83-047d-4f7f-93fd-163ea305b493"
    ENVIRONMENTS = "7df2a957-f12d-42ea-9fa6-95d7920c1b76"
    LIBRARIES = "b70a4973-99ca-40c0-b2b2-f03417a5ff1d"
    NOTES = "1d55d702-028c-4aab-99cc-acfdd441fe5f"
    NAMED_POSITIONS = "91799cf0-059a-46f8-854c-cc1c1419e29f"
    LAYER_STATES = "6df55e69-e102-4a72-b181-11664046c93f"
    NAMED_CPLANES = "8f23551a-a05b-4a03-a8d5-3e2fc55e4d8a"
    TEXTURES = "8df2a957-f12d-42ea-9fa6-95d7920c1b76"
    RENDERING = "d9ac0269-811b-47d1-aa33-777986b13715"
    GROUND_PLANE = "987b1930-ecde-4e62-8282-97ab4ad325fe"
    LIGHTS = "86777b3d-3d68-4965-84f8-9e019c402433"
    CONTEXT_HELP = "0f8fb4f9-c213-4a6e-8e79-0bece02df82a"
    BLOCK_CONTENT = "072f97a4-6861-4113-a2d5-838d97999336"
    FILE_EXPLORER = "cd3ab54f-0213-4d30-b4f5-39a018e39604"
    OSNAP = "d3c4a392-88de-4c4f-88a4-ba5636ef7f38"
    SELECTION_FILTERS = "918191ca-1105-43f9-a34a-dda4276883c1"
    COMMAND_HISTORY = "1d3d1785-2332-428b-a838-b2fe39ec50f4"


class Scope(StrEnum):
    """Viewports a display mode macro sets, as the `_Viewport` option names them."""

    ACTIVE = "_Active"
    ALL = "_All"


class Site(StrEnum):
    """Dock sites of the main window as Rhino names their locations, each also naming the height the in-process stage measures for it."""

    LEFT = "Left"
    RIGHT = "Right"
    TOP = "Top"
    BOTTOM = "Bottom"


class Extent(StrEnum):
    """Lengths in points and row counts the in-process stage measures besides the dock sites and the file stage sizes the bands, containers, and panel splitters from."""

    TAB_STRIP = auto()
    BUTTON = auto()
    RESIZER = auto()
    LAYERS_CHROME = auto()
    LAYERS_HEADER = auto()
    LAYERS_ROW = auto()
    OSNAP = auto()
    MATERIALS_STRIP = auto()
    MATERIALS_ROW = auto()
    LIBRARIES_CHROME = auto()
    LIBRARIES_BORDER = auto()
    LIBRARIES_ROW = auto()
    LIBRARIES_FOLDERS = auto()
    LIBRARIES_LIST_MINIMUM = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

DISPLAY_MODE_FILE: Final = Path(__file__).with_name("display-mode.ini")
READY_VARIABLE: Final = "INTERFACE_READY_ENDPOINT"
BUTTON_SIZE: Final = 19
BUTTON_PADDING: Final = 2

# --- [TABLES] ---------------------------------------------------------------------------

RIGHT_TOP: Final = (Panel.PROPERTIES, Panel.NAMED_VIEWS, Panel.LAYOUTS, Panel.BLOCK_DEFINITIONS, Panel.MATERIALS, Panel.SUN, Panel.DISPLAY, Panel.SNAPSHOTS)
RIGHT_BOTTOM: Final = (Panel.LAYERS,)

# --- [OPERATIONS] -----------------------------------------------------------------------


def declared_mode() -> tuple[uuid.UUID, str]:
    """Id and English name of the display mode the INI declares."""
    parser = ConfigParser(interpolation=None, comment_prefixes=("=",))
    parser.read(DISPLAY_MODE_FILE, encoding="utf-8")
    section = parser.sections()[0]
    return uuid.UUID(section.rpartition("\\")[2]), parser[section]["Name"]


def mode_macro(scope: Scope) -> str:
    """Macro that sets the declared display mode in the scope's viewports, transparent inside a running command."""
    return f"'_SetDisplayMode _Viewport={scope} _Mode={MODE_NAME}"


def ready() -> None:
    """Send Rhino's process id over one connection to the endpoint the host names."""
    from System import Environment
    from System.Net import IPEndPoint
    from System.Net.Sockets import TcpClient
    from System.Text import Encoding

    client = TcpClient()
    pid = Encoding.ASCII.GetBytes(str(Environment.ProcessId))
    try:
        client.Connect(IPEndPoint.Parse(Environment.GetEnvironmentVariable(READY_VARIABLE)))
        client.GetStream().Write(pid, 0, pid.Length)
    finally:
        client.Dispose()


# --- [COMPOSITION] ----------------------------------------------------------------------

MODE_ID, MODE_NAME = declared_mode()

if __name__ == "__main__":
    ready()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BUTTON_PADDING", "BUTTON_SIZE", "DISPLAY_MODE_FILE", "MODE_ID", "MODE_NAME", "READY_VARIABLE", "RIGHT_BOTTOM", "RIGHT_TOP", "Extent", "Panel", "Scope", "Site", "mode_macro"]
