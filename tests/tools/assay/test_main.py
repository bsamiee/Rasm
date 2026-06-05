"""Main entrypoint: parse-fault Envelope E2, exit-code contract, CycloptsError handling.

Source surface: ``tools/assay/__main__.py`` — ``meta``, ``main``, parse-fault envelope path,
``CycloptsError`` handling.
Laws: E2 meta parse-fault emits to stderr without structured Envelope (xfail strict), main('--help')
exits 0 with structured output on stdout, unknown verb → Envelope(FAULTED) not traceback, exit code
from Envelope.exit_code is the process exit code.
"""

import pytest


def test_bedrock_placeholder() -> None:
    pytest.skip("bedrock: coverage pending")
