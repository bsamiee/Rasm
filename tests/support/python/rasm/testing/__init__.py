"""Fixtures for pytest, each a plugin module behind the extra of its name: `sqlite` a private SQLite database, `hosting` HTTP servers a test hosts on a loopback port, over the outcomes `rasm.testing.plugin` records."""

from rasm.testing._outcomes import raised, settled

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["raised", "settled"]
