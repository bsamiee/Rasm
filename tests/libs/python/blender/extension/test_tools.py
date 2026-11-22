"""Toolbar icons a tool class names: a built-in geometry icon by name or a `.dat` file the extension packages by path, each in the form Blender's toolbar joins onto its own icon folder."""

from pathlib import Path

import bpy
from expression import Error, NOTHING, Ok
import pytest

from rasm.extension.data import Identity
from rasm.extension.ui import Tool, Tools
from rasm.records import Fault, Outcome, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


def _built(identity: Identity, icon: str | Path) -> Outcome[type[bpy.types.WorkSpaceTool]]:
    tool = Tool(key="meter", operator=bpy.ops.image.sample, icon=icon)
    return Tools(space="IMAGE_EDITOR", tools=(tool,)).tool_class(identity, tool, NOTHING)


def test_a_builtin_name_registers_as_the_name_the_toolbar_joins_onto_its_folder(identity: Identity) -> None:
    """A name registers unchanged, the relative stem Blender's toolbar joins onto its own icon folder."""
    match _built(identity, "ops.generic.select_box"):
        case Ok(built):
            assert vars(built)["bl_icon"] == "ops.generic.select_box"
        case other:
            pytest.fail(f"refused: {other}")


def test_a_packaged_path_registers_as_the_absolute_stem_of_the_extension_file(identity: Identity, tmp_path: Path) -> None:
    """A path registers as the absolute path of the extension's own `.dat` without its suffix, which replaces Blender's icon folder in the toolbar's join."""
    match _built(identity, Path("meter.dat")):
        case Ok(built):
            assert Path(f"{vars(built)['bl_icon']}.dat") == tmp_path / "probe_extension" / "icons" / "meter.dat"
            assert Path("/blender/datafiles/icons") / f"{vars(built)['bl_icon']}.dat" == tmp_path / "probe_extension" / "icons" / "meter.dat"
        case other:
            pytest.fail(f"refused: {other}")


@pytest.mark.parametrize("icon", [pytest.param(Path("absent.dat"), id="missing"), pytest.param(Path("flip.svg"), id="not-triangles")])
def test_a_path_to_no_triangle_file_in_the_icons_folder_is_refused_as_absent(identity: Identity, icon: Path) -> None:
    """A path the package's `icons/` folder holds no `.dat` file at refuses the tool class with one absent fault naming that path."""
    (Path(str(identity.folder)) / "probe_extension" / "icons" / "flip.svg").write_text("<svg/>")
    match _built(identity, icon):
        case Error((Fault(reason=Reason.ABSENT, member=member),)):
            assert member == icon.as_posix()
        case other:
            pytest.fail(f"not refused as absent: {other}")
