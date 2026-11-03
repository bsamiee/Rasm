"""Foundry command: one pipeline stage over the faces a name selects in a declared font set."""

from pathlib import Path

from typography.builds.pipeline import Action, performed, Report
from typography.faces.faults import Outcome

# --- [OPERATIONS] -----------------------------------------------------------------------


def foundry(action: Action, declaration: Path, name: str = "", destination: Path | None = None, /) -> Outcome[tuple[Report, ...]]:
    """Run one stage over the faces a name selects, a face rebuilt only when its inputs changed.

    Args:
        action: `source` writes the UFO, `build` compiles a changed face, `check` verifies the current build, `approve` records its shaping, `run` builds and checks, `publish` copies its TrueType and WOFF2 out.
        declaration: Font set declaration.
        name: Set, family, ornament set, or face stem, the whole set when empty.
        destination: Folder `publish` copies into.

    Returns:
        One report per face, or every face's faults.
    """
    return performed(action, declaration, name=name, destination=destination)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["foundry"]
