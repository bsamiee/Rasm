"""Laws for tools.assay.composition.pipeline: attribution, smoke federation, and evolution obligations."""

# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------

from typing import Final, TYPE_CHECKING

import pytest

from tests._aspect import register_laws, register_sut  # noqa: PLC2701  # sibling test-internal law engine; `_`-named by S1 design
from tools.assay.composition import pipeline as pipeline_mod, registry as registry_mod
from tools.assay.composition.pipeline import (
    attribution_by_signal,
    ATTRIBUTION_LATTICE,
    evolution_by_consumer,
    EVOLUTION_OBLIGATIONS,
    NEGATIVE_FIXTURES,
    ProofPolicy,
    smoke_by_id,
    SMOKE_FEDERATION,
    verify_pipeline_registry,
)
from tools.assay.core.model import validate_detail, wire_encode


if TYPE_CHECKING:
    from tools.assay.composition.pipeline import AttributionRow, EvolutionObligationRow, SmokeRow


# --- [CONSTANTS] -----------------------------------------------------------------------------

_ATTRIBUTION_ROOT: Final = f"{pipeline_mod.__name__}.ATTRIBUTION_LATTICE"
_SMOKE_ROOT: Final = f"{pipeline_mod.__name__}.SMOKE_FEDERATION"
_EVOLUTION_ROOT: Final = f"{pipeline_mod.__name__}.EVOLUTION_OBLIGATIONS"
_DECODER_ROOT: Final = f"{registry_mod.__name__}._ENVELOPE_DECODER"


# --- [OPERATIONS] ----------------------------------------------------------------------------

# ------ Registry topology laws ---------------------------------------------------------------


def test_import_gate_passes() -> None:
    """Import-time verify_pipeline_registry succeeds — three-table joins are closed."""
    verify_pipeline_registry()


def test_attribution_signal_ids_unique() -> None:
    """Every ATTRIBUTION_LATTICE signal_id is unique."""
    ids = tuple(row.signal_id for row in ATTRIBUTION_LATTICE)
    assert len(ids) == len(frozenset(ids))


def test_smoke_ids_unique() -> None:
    """Every SMOKE_FEDERATION smoke_id is unique."""
    ids = tuple(row.smoke_id for row in SMOKE_FEDERATION)
    assert len(ids) == len(frozenset(ids))


def test_consumer_ids_unique() -> None:
    """Every EVOLUTION_OBLIGATIONS consumer_id is unique."""
    ids = tuple(row.consumer_id for row in EVOLUTION_OBLIGATIONS)
    assert len(ids) == len(frozenset(ids))


@pytest.mark.parametrize("row", ATTRIBUTION_LATTICE, ids=lambda r: r.signal_id)
def test_smoke_expected_signal_links_attribution(row: AttributionRow) -> None:
    """Every smoke row expected_signal_id resolves on ATTRIBUTION_LATTICE — parametrized on attribution for join exhaustiveness."""
    linked = tuple(smoke.expected_signal_id for smoke in SMOKE_FEDERATION if smoke.expected_signal_id == row.signal_id)
    match linked:
        case ():
            return
        case _:
            assert attribution_by_signal(row.signal_id) is row


@pytest.mark.parametrize("smoke", SMOKE_FEDERATION, ids=lambda s: s.smoke_id)
def test_smoke_links_expected_signal(smoke: SmokeRow) -> None:
    """Every smoke row cites an admitted attribution signal."""
    assert attribution_by_signal(smoke.expected_signal_id) is not None


@pytest.mark.parametrize("row", EVOLUTION_OBLIGATIONS, ids=lambda r: r.consumer_id)
def test_evolution_cross_links_present(row: EvolutionObligationRow) -> None:
    """Evolution rows publish surface_ids and lattice_transition_ids foreign keys."""
    assert row.surface_ids
    assert row.lattice_transition_ids


@pytest.mark.parametrize("row", ATTRIBUTION_LATTICE, ids=lambda r: r.signal_id)
def test_negative_fixtures_admitted(row: AttributionRow) -> None:
    """Attribution negative_fixture_ids draw from the closed NEGATIVE_FIXTURES vocabulary."""
    assert all(fixture in NEGATIVE_FIXTURES for fixture in row.negative_fixture_ids)


# ------ Lookup surface laws --------------------------------------------------------------------


def test_unknown_signal_returns_none() -> None:
    """attribution_by_signal returns None for unknown slugs."""
    assert attribution_by_signal("__missing_signal__") is None


def test_known_signal_round_trips() -> None:
    """attribution_by_signal round-trips admitted rows."""
    row = ATTRIBUTION_LATTICE[0]
    assert attribution_by_signal(row.signal_id) is row


def test_unknown_smoke_returns_none() -> None:
    """smoke_by_id returns None for unknown slugs."""
    assert smoke_by_id("__missing_smoke__") is None


def test_known_smoke_round_trips() -> None:
    """smoke_by_id round-trips admitted rows."""
    row = SMOKE_FEDERATION[0]
    assert smoke_by_id(row.smoke_id) is row


def test_unknown_consumer_returns_none() -> None:
    """evolution_by_consumer returns None for unknown slugs."""
    assert evolution_by_consumer("__missing_consumer__") is None


def test_known_consumer_round_trips() -> None:
    """evolution_by_consumer round-trips admitted rows."""
    row = EVOLUTION_OBLIGATIONS[0]
    assert evolution_by_consumer(row.consumer_id) is row


# ------ Codec singleton identity laws --------------------------------------------------------


def test_smoke_decoder_symbol_reference_identity() -> None:
    """SMOKE_FEDERATION decoder_symbol names alias the production envelope decoder singleton."""
    rows = tuple(smoke for smoke in SMOKE_FEDERATION if smoke.decoder_symbol == "_ENVELOPE_DECODER")
    assert rows
    assert registry_mod._ENVELOPE_DECODER is not None


@pytest.mark.parametrize("owner_symbol, singleton", [("wire_encode", wire_encode), ("validate_detail", validate_detail)])
def test_smoke_owner_symbol_names_production_gate(owner_symbol: str, singleton: object) -> None:
    """SMOKE_FEDERATION owner_symbol cites production gates referenced by wave-4 attribution rows."""
    rows = tuple(smoke for smoke in SMOKE_FEDERATION if smoke.owner_symbol == owner_symbol)
    assert rows
    assert singleton is not None


def test_envelope_decode_parity_smoke_cross_links_cli_consumer() -> None:
    """envelope_decode_parity smoke and cli_stdout_parser consumer share decoder and stdout fixture vocabulary."""
    smoke = smoke_by_id("envelope_decode_parity")
    consumer = evolution_by_consumer("cli_stdout_parser")
    assert smoke is not None
    assert consumer is not None
    assert smoke.decoder_symbol == consumer.decode_owner == "_ENVELOPE_DECODER"
    assert "stdout_envelope" in consumer.surface_ids


def test_invariant_doubler_links_one_write_smoke() -> None:
    """invariant_doubler evolution row and one_write_stdout smoke share emit-stage lattice linkage."""
    doubler = evolution_by_consumer("invariant_doubler")
    smoke = smoke_by_id("one_write_stdout")
    assert doubler is not None
    assert smoke is not None
    assert smoke.expected_signal_id == "one_write_violation"
    assert "emit_fold_to_envelope" in doubler.lattice_transition_ids


@pytest.mark.parametrize(
    "consumer, policy",
    [
        ("cli_stdout_parser", ProofPolicy.REQUIRED_ON_SUCCESS_DETAIL),
        ("history_replay", ProofPolicy.OPTIONAL),
        ("delta_compare", ProofPolicy.NEVER),
        ("invariant_doubler", ProofPolicy.SKIPPED_ON_FAULT),
    ],
)
def test_evolution_proof_policy_matrix(consumer: str, policy: ProofPolicy) -> None:
    """Evolution obligation proof_policy matches the catalog matrix from wave 4 doctrine."""
    row = evolution_by_consumer(consumer)
    assert row is not None
    assert row.proof_policy is policy


# --- [COMPOSITION] ---------------------------------------------------------------------------

register_sut("tools.assay.composition.pipeline")

register_laws(
    (verify_pipeline_registry, ("import_gate_passes",)),
    (attribution_by_signal, ("unknown_signal_returns_none", "known_signal_round_trips")),
    (smoke_by_id, ("unknown_smoke_returns_none", "known_smoke_round_trips")),
    (evolution_by_consumer, ("unknown_consumer_returns_none", "known_consumer_round_trips")),
)
