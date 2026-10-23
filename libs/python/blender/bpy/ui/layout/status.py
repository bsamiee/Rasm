"""Report banners, held failures, and cursor progress."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from typing import TYPE_CHECKING

import bpy
from expression import Ok

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.ui.layout.rows import Line, RowDraw

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Status(Line, frozen=True, kw_only=True):
    """Report banner over held failures."""

    @staticmethod
    def drawn(identity: Identity) -> BlenderResult[RowDraw]:
        """Draws report banner, then held entries."""

        def marked(kind: "rna_enums.WmReportItems") -> tuple["rna_enums.IconItems", bool]:
            match kind:
                case "ERROR" | "ERROR_INVALID_INPUT" | "ERROR_INVALID_CONTEXT" | "ERROR_OUT_OF_MEMORY":
                    return "STATUS_ERROR_FILLED", True
                case "WARNING":
                    return "STATUS_WARNING_FILLED", False
                case "INFO":
                    return "STATUS_INFO_FILLED", False
                case "DEBUG":
                    return "SYSTEM", False
                case "PROPERTY":
                    return "OPTIONS", False
                case "OPERATOR":
                    return "CHECKMARK", False

        def draw(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
            layout.template_reports_banner()
            for kind, text in identity.reports.entries:
                icon, alert = marked(kind)
                line = layout.row()
                line.alert = alert
                line.label(text=text, icon=icon)

        return Ok(draw)


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def progressed(manager: bpy.types.WindowManager, total: int) -> Generator[Callable[[float], None]]:
    """Scopes cursor progress over synchronous item count."""
    manager.progress_begin(0, total)
    try:
        yield manager.progress_update
    finally:
        manager.progress_end()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Status", "progressed"]
