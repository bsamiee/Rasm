"""Tests for the runtime plugin's tool fixtures."""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path

from honeybee_energy.config import folders

# --- [TOOLS] ----------------------------------------------------------------------------


def test_energyplus_fixture_points_honeybee_at_the_provisioned_install(energyplus: Path) -> None:
    """honeybee-energy reads the executable and its version from the folder provision linked."""
    assert folders.energyplus_exe == str(energyplus / "energyplus"), f"honeybee-energy resolved {folders.energyplus_exe!r}"
    assert folders.energyplus_version_str, "honeybee-energy read no EnergyPlus version from the executable"
