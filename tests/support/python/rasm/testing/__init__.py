"""Fixtures for pytest, each a plugin module behind the extra of its name: `sqlite` a private SQLite database, `hosting` HTTP servers a test hosts on a loopback port, over the outcomes and limits of `rasm.testing.plugin`."""

from rasm.testing._outcomes import FAILED, LIMIT, raised, reaching, settled

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FAILED", "LIMIT", "raised", "reaching", "settled"]
