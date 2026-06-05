"""Static rail: report/plan routing, S1 splice guard law, S2 language-fan scope.

Source surface: ``tools/assay/rails/static.py`` — ``thin_rail``, ``StaticParams``, ``plan``, ``report``,
``_languages``.
Laws: S1 splice guard (xfail strict — non-scoped verbs receive --artifacts-path), S2 language fan
(xfail strict — py-only --paths fans whole-repo), plan with --paths=a.cs escalates CSHARP only,
report with empty change-set → Report(EMPTY), _languages(None) returns all Language members,
StaticParams.bound('fix') with no extra positionals → returns self.
"""

import pytest


def test_bedrock_placeholder() -> None:
    pytest.skip("bedrock: coverage pending")
