"""API rail: shape_of oracle, _candidates M1 scoring, decompile shape, doctor inventory.

Source surface: ``tools/assay/rails/api.py`` — ``shape_of``, ``_candidates``, ``_resolve_key``,
``_body``, ``_slice``, ``doctor``, ``_inventory``.
Laws: shape_of closed-form oracle ('' → INDEX, 'Foo' → NAMESPACE, 'Foo.Bar' → TYPE, 'Foo.Bar.Baz' → MEMBER),
R1 shape_of('Mesh') → NAMESPACE not TYPE (xfail strict), R2 ambiguous resolver homonym (xfail strict),
R3 decompile shape path fall-through (xfail strict), M1 _candidates scoring exact-match rank (xfail strict),
_resolve_key exact match, ApiParams.bound('query') projects paths[0].
"""

import pytest


def test_bedrock_placeholder() -> None:
    pytest.skip("bedrock: coverage pending")
