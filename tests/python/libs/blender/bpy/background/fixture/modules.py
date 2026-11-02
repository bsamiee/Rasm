"""Import probe over every module of the `blender` library under Blender's interpreter, naming each module whose import raises."""

import importlib
from pathlib import Path
from typing import Final

from expression.extra.result import catch

import blender
from blender.bpy.extension.identity import Identity

# --- [OPERATIONS] -----------------------------------------------------------------------


def imported(_identity: Identity) -> tuple[str, ...]:
    """Returns each library module whose import raises, with its error."""
    roots = tuple(Path(folder) for folder in blender.__path__)
    names = sorted(".".join(("blender", *path.relative_to(root).with_suffix("").parts)) for root in roots for path in root.rglob("*.py") if path.name != "__init__.py")
    return tuple(f"{name}: {error!r}" for name in names for error in catch(importlib.import_module, exception=Exception)(name).swap().to_option().to_list())


# --- [COMPOSITION] ----------------------------------------------------------------------

rows: Final = ()
PROBES: Final = (imported,)
RELEASED: Final = ()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
