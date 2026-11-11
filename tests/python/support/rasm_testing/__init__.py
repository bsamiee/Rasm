"""Fixtures for pytest, each a plugin module behind the extra of its name: `sqlite` a private SQLite database, `containers` Testcontainers servers a test controls, `hosting` HTTP servers a test hosts on a loopback port, over the outcomes and limits of `rasm_testing.plugin`."""

from rasm_testing._outcomes import FAILED, LIMIT, raised, reaching, settled

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FAILED", "LIMIT", "raised", "reaching", "settled"]
