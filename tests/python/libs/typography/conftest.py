"""A fontspector profile and configuration beside each test's declaration."""

from pathlib import Path

import pytest

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def configuration(tmp_path: Path) -> Path:
    """Writes the fontspector TOML into the test folder, its shaping test directory resolving there."""
    toml = """\
include_profiles = ["universal"]

[sections]
Shaping = ["shaping/regression", "shaping/forbidden"]

[shaping]
test_directory = "shaping"
"""
    written = tmp_path / "fontspector.toml"
    written.write_text(toml, encoding="utf-8")
    return written
