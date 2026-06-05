"""Bridge rail: _fold_scenarios summary, _decode_result adversarial variants, _affirm table.

Source surface: ``tools/assay/rails/bridge.py`` — ``_fold_scenarios``, ``_decode_result``,
``_affirm``, ``_first_fault``, ``_exceptions``.
Laws: _decode_result(valid) → Ok(_Scenario(OK, exceptions=0)), _decode_result(malformed) → degraded FAILED,
_decode_result(partial) → degraded FAILED, _decode_result(missing) → FAILED (missing file),
_affirm(Ok(receipt rc=0 OK)) → Ok(None), _affirm(Ok(receipt rc=0 FAILED)) → Error(Fault(FAULTED)),
_fold_scenarios closed arithmetic oracle.
"""

import pytest


def test_bedrock_placeholder() -> None:
    pytest.skip("bedrock: coverage pending")
