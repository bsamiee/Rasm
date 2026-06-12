"""py_analyzer suite wiring: SUT registration for the law-coverage gate (second-package pilot).

The two-line integration recipe: import ``register_sut``, call it with the package and its exempt set.
The generic gate (``tests.python._testkit.test_policy``) walks this package's public surface on every
full-suite run with no further configuration.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import Final

from tests.python._testkit.laws import register_sut


# --- [CONSTANTS] ------------------------------------------------------------------------

# PILOT DEBT LEDGER, not an endorsement: every entry is a currently-uncovered public symbol awaiting
# laws. Shrink this set by authoring laws, never by deleting symbols from it without one. cli.py now
# declares __all__, so its import-noise symbols (Path/TYPE_CHECKING/annotations/assert_never) leave the
# walked surface — only the 15 real public symbols remain on the ledger.
_EXEMPT: Final = frozenset({
    "analyze_paths", "classify_scope", "PY_ANALYZER_ROOT",                                  # analyzer: walk + scope engine
    "Diagnostic", "JsonValue", "OutputFormat", "Rule", "RULES",                             # rules: catalog + wire shape
    "RuleCategory", "RuleId", "Scope", "Severity", "diagnostic",                            # rules: vocabularies + constructor
    "emit", "main",                                                                  # cli: output contract + entrypoint
})  # fmt: skip

register_sut("tools.py_analyzer", exempt=_EXEMPT)
