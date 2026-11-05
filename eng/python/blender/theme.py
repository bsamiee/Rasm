"""Writes each interface theme of the `[tool.blender.theme]` table of the `pyproject.toml` `--` names to `<name>.xml` in the folder after it, resolved by the running Blender."""

import os
from pathlib import Path
import sys

import bpy
from expression import Result
import msgspec

from blender.bpy.ui.resources.interface_theme import InterfaceTheme, written
from blender.records.faults import accumulated, Error, Fault, Outcome, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


def _written(declaration: Path, folder: Path, /) -> Outcome[object]:
    """Writes every declared theme, a relative base file read from the declaration's folder, keeping every fault."""
    try:
        os.chdir(declaration.parent)
        themes = msgspec.convert(msgspec.toml.decode(declaration.read_bytes())["tool"]["blender"]["theme"], dict[str, InterfaceTheme])
    except (OSError, msgspec.DecodeError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(declaration), error))
    return accumulated(written(bpy.context, theme, folder / f"{name}.xml") for name, theme in themes.items())


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Writes the declared themes, exiting with each fault's message."""
    declaration, folder = map(Path, sys.argv[sys.argv.index("--") + 1 :])
    match _written(declaration, folder):
        case Result(tag="ok"):
            sys.exit(None)
        case Result(error=faults):
            sys.exit("\n".join(fault.message for fault in faults))


if __name__ == "__main__":
    main()
