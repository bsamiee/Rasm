"""Blender module state the cases read: the Blender Light theme preset applied over the factory theme and an extension package holding one packaged toolbar icon."""

from collections.abc import Generator
from pathlib import Path
import sys

import bpy
import pytest

from rasm.extension.data import Identity, Manifest, Reports

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def light() -> Generator[bpy.types.ThemeUserInterface]:
    """Applies the installed Blender Light preset for the case, then restores the factory theme."""
    match bpy.utils.preset_find("Blender_Light", "interface_theme", ext=".xml"), bpy.context.preferences:
        case str(preset), bpy.types.Preferences(themes=themes):
            bpy.ops.script.execute_preset(filepath=preset, menu_idname="USERPREF_MT_interface_theme_presets")
            yield themes[0].user_interface
            bpy.ops.preferences.reset_default_theme()
        case missing:
            pytest.fail(f"Blender holds no Blender Light preset or preferences: {missing}")


@pytest.fixture
def identity(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> Identity:
    """Returns an extension identity over a package whose `icons/` folder holds `meter.dat`."""
    package = tmp_path / "probe_extension"
    (package / "icons").mkdir(parents=True)
    (package / "__init__.py").write_text('"""Probe extension."""\n')
    (package / "icons" / "meter.dat").write_bytes(b"VCO\x00")
    monkeypatch.syspath_prepend(str(tmp_path))
    monkeypatch.delitem(sys.modules, package.name, raising=False)
    manifest = Manifest(id="probe", name="Probe", version="1.0.0")
    return Identity(package=package.name, manifest=manifest, folder=tmp_path, reports=Reports(package.name, tmp_path, extension=manifest.id, version=manifest.version))
