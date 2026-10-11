"""Native report banners, held failures, and scoped synchronous cursor progress."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from typing import TYPE_CHECKING

import bpy
from expression import Ok

from rasm.blender.extension.identity import Identity
from rasm.blender.faults import BlenderResult
from rasm.blender.ui.layout.rows import Line, RowDraw

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Status(Line, frozen=True, kw_only=True):
    """Native report banner followed by this extension's held failures."""

    @staticmethod
    def drawn(identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires drawing of complete native report entries in hold order."""

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

        def draw(layout: bpy.types.UILayout, _holder: bpy.types.bpy_struct, _context: bpy.types.Context) -> None:  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
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
    """Scopes native cursor progress over a synchronous item's count."""
    manager.progress_begin(0, total)
    try:
        yield manager.progress_update
    finally:
        manager.progress_end()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Status", "progressed"]
