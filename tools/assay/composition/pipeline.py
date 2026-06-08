"""Materialization pipeline registry: attribution, smoke federation, and evolution obligations."""

from dataclasses import dataclass
from enum import StrEnum
from operator import attrgetter
from typing import Final, TYPE_CHECKING


if TYPE_CHECKING:
    from collections.abc import Callable, Mapping


# --- [TYPES] ----------------------------------------------------------------------------


class StageOwner(StrEnum):
    """Pipeline stage or composition-root guard owner."""

    INGRESS = "ingress"
    VALIDATION = "validation"
    NORMALIZATION = "normalization"
    CONSTRUCTION = "construction"
    ENRICHMENT = "enrichment"
    MATERIALIZATION = "materialization"
    SERIALIZATION = "serialization"
    BIND = "bind"
    STRICT = "strict"
    VALIDATED = "validated"
    EMIT = "emit"
    ENCODE = "encode"


class PlaneTag(StrEnum):
    """Evidence plane tag per static/compiled/runtime doctrine."""

    STATIC = "static"
    COMPILED = "compiled"
    RUNTIME = "runtime"


class SmokeInvariantKind(StrEnum):
    """Closed smoke invariant vocabulary."""

    SINGLE_EMIT = "single_emit"
    DECODE_PARITY = "decode_parity"
    DETERMINISTIC_BYTES = "deterministic_bytes"
    CAP_TRUNCATED_ARTIFACT_SPILL = "cap_truncated_artifact_spill"
    STAGE_INJECTION = "stage_injection"
    SUBPROCESS_ENVELOPE = "subprocess_envelope"


class ProofPolicy(StrEnum):
    """Consumer detail proof posture."""

    REQUIRED_ON_SUCCESS_DETAIL = "required_on_success_detail"
    SKIPPED_ON_FAULT = "skipped_on_fault"
    OPTIONAL = "optional"
    NEVER = "never"


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class AttributionRow:
    """One frozen attribution signal row."""

    signal_id: str
    stage_owner: StageOwner
    plane_tag: PlaneTag
    owner_symbol: str
    lattice_transition_ids: tuple[str, ...] = ()
    surface_ids: tuple[str, ...] = ()
    prefix_pattern: str = ""
    remediation_row_id: str = ""
    negative_fixture_ids: tuple[str, ...] = ()


@dataclass(frozen=True, slots=True, kw_only=True)
class SmokeRow:
    """One integration smoke federation row."""

    smoke_id: str
    invariant_kind: SmokeInvariantKind
    owner_symbol: str
    decoder_symbol: str
    inject_fixture_id: str
    expected_signal_id: str
    consumer_fixture_ref: str = ""


@dataclass(frozen=True, slots=True, kw_only=True)
class EvolutionObligationRow:
    """One evolution consumer obligation row."""

    consumer_id: str
    upstream_artifact: str
    decode_owner: str
    version_gate_fields: tuple[str, ...]
    proof_policy: ProofPolicy
    surface_ids: tuple[str, ...]
    lattice_transition_ids: tuple[str, ...]
    absence_encoding: str = ""


# --- [CONSTANTS] ------------------------------------------------------------------------

NEGATIVE_FIXTURES: Final[frozenset[str]] = frozenset((
    "conflated_attribution",
    "consumer_json_loads",
    "domain_reproof",
    "double_emit",
    "dual_parser_logic",
    "duplicate_validation_surface",
    "none_coercion_delta",
    "proof_on_fault_line",
    "seam_remap_context_leak",
    "stage_skip_dict_to_owner",
))


# --- [TABLES] ---------------------------------------------------------------------------


ATTRIBUTION_LATTICE: Final[tuple[AttributionRow, ...]] = (
    AttributionRow(
        signal_id="validation_field_path_fault",
        stage_owner=StageOwner.VALIDATION,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="TypeAdapter",
        lattice_transition_ids=("ingress_carrier_to_validation_exit",),
        prefix_pattern="validation:",
        negative_fixture_ids=("conflated_attribution",),
    ),
    AttributionRow(
        signal_id="wire_decode_type_mismatch",
        stage_owner=StageOwner.SERIALIZATION,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="Decoder",
        lattice_transition_ids=("serialization_exit_to_consumer_decode",),
        negative_fixture_ids=("consumer_json_loads",),
    ),
    AttributionRow(
        signal_id="smart_constructor_fault",
        stage_owner=StageOwner.CONSTRUCTION,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="smart_constructor",
        lattice_transition_ids=("construction_exit_to_enrichment_exit",),
        negative_fixture_ids=("stage_skip_dict_to_owner",),
    ),
    AttributionRow(
        signal_id="detail_proof_failure",
        stage_owner=StageOwner.VALIDATED,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="validate_detail",
        lattice_transition_ids=("detail_proof_validated",),
        negative_fixture_ids=("domain_reproof", "proof_on_fault_line"),
    ),
    AttributionRow(
        signal_id="beartype_handler_violation",
        stage_owner=StageOwner.MATERIALIZATION,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="checked_call",
        lattice_transition_ids=("enrichment_exit_to_materialization_exit",),
        prefix_pattern="validation:",
        negative_fixture_ids=("conflated_attribution",),
    ),
    AttributionRow(
        signal_id="duplicate_validation_surface",
        stage_owner=StageOwner.VALIDATION,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="TypeAdapter",
        remediation_row_id="remove_duplicate_validation",
        negative_fixture_ids=("duplicate_validation_surface",),
    ),
    AttributionRow(
        signal_id="stage_skip_without_exemption",
        stage_owner=StageOwner.INGRESS,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="rail",
        remediation_row_id="add_root_exemption_or_adapter",
        lattice_transition_ids=("stage_skip_dict_to_owner",),
    ),
    AttributionRow(
        signal_id="seam_remap_context_leak",
        stage_owner=StageOwner.MATERIALIZATION,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="anti_corruption_adapter",
        lattice_transition_ids=("patch_dict_to_successor_owner",),
        negative_fixture_ids=("seam_remap_context_leak",),
    ),
    AttributionRow(
        signal_id="version_oracle_default",
        stage_owner=StageOwner.SERIALIZATION,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="_ENVELOPE_DECODER",
        surface_ids=("stdout_envelope",),
        remediation_row_id="add_migration_hop",
    ),
    AttributionRow(
        signal_id="params_bind_fault",
        stage_owner=StageOwner.BIND,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="BaseParams.bound",
        lattice_transition_ids=("params_bind_to_bound_or_fault",),
        prefix_pattern="parse:",
    ),
    AttributionRow(
        signal_id="strict_empty_fold",
        stage_owner=StageOwner.STRICT,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="_strict",
        lattice_transition_ids=("strict_policy_promotion",),
        prefix_pattern="strict:",
    ),
    AttributionRow(
        signal_id="uncaptured_promotion_fault",
        stage_owner=StageOwner.EMIT,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="_guard",
        lattice_transition_ids=("emit_fold_to_envelope",),
    ),
    AttributionRow(
        signal_id="cap_truncation_without_spill",
        stage_owner=StageOwner.EMIT,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="_emit",
        surface_ids=("stdout_envelope",),
        lattice_transition_ids=("cap_truncation_spill",),
        remediation_row_id="artifact_spill",
    ),
    AttributionRow(
        signal_id="surrogate_encode_fault",
        stage_owner=StageOwner.ENCODE,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="wire_encode",
        remediation_row_id="wire_safe_scrub",
        lattice_transition_ids=("wire_projection_to_serialization_exit",),
    ),
    AttributionRow(
        signal_id="one_write_violation",
        stage_owner=StageOwner.EMIT,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="_emit",
        lattice_transition_ids=("emit_fold_to_envelope",),
        negative_fixture_ids=("double_emit",),
    ),
    AttributionRow(
        signal_id="version_literal_failure",
        stage_owner=StageOwner.SERIALIZATION,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="_ENVELOPE_DECODER",
        surface_ids=("stdout_envelope",),
        remediation_row_id="add_migration_hop",
    ),
    AttributionRow(
        signal_id="encoder_identity_skew",
        stage_owner=StageOwner.SERIALIZATION,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="wire_encode",
        surface_ids=("cache_bytes", "probe_fingerprint"),
        remediation_row_id="re_key_cache",
    ),
    AttributionRow(
        signal_id="migration_arm_miss",
        stage_owner=StageOwner.SERIALIZATION,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="msgspec.convert",
        surface_ids=("migration_stored",),
        remediation_row_id="add_migration_hop",
    ),
    AttributionRow(
        signal_id="domain_preformatted_diagnostic",
        stage_owner=StageOwner.EMIT,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="_distill",
        remediation_row_id="collapse_to_root_distillation",
    ),
    AttributionRow(
        signal_id="typeguard_cast_consumer",
        stage_owner=StageOwner.MATERIALIZATION,
        plane_tag=PlaneTag.STATIC,
        owner_symbol="consumer_narrow",
        lattice_transition_ids=("consumer_detail_narrow",),
        negative_fixture_ids=("domain_reproof",),
    ),
    AttributionRow(
        signal_id="config_bind_fault",
        stage_owner=StageOwner.BIND,
        plane_tag=PlaneTag.COMPILED,
        owner_symbol="AssaySettings",
        prefix_pattern="config:",
        lattice_transition_ids=("settings_snapshot_roundtrip",),
        surface_ids=("settings_env",),
    ),
    AttributionRow(
        signal_id="parse_surplus_fault",
        stage_owner=StageOwner.BIND,
        plane_tag=PlaneTag.RUNTIME,
        owner_symbol="parse_fault",
        prefix_pattern="parse:",
        lattice_transition_ids=("params_bind_to_bound_or_fault",),
    ),
)

SMOKE_FEDERATION: Final[tuple[SmokeRow, ...]] = (
    SmokeRow(
        smoke_id="one_write_stdout",
        invariant_kind=SmokeInvariantKind.SINGLE_EMIT,
        owner_symbol="_emit",
        decoder_symbol="_ENVELOPE_DECODER",
        inject_fixture_id="double_emit",
        expected_signal_id="one_write_violation",
        consumer_fixture_ref="stdout_single_line",
    ),
    SmokeRow(
        smoke_id="deterministic_line_shape",
        invariant_kind=SmokeInvariantKind.DETERMINISTIC_BYTES,
        owner_symbol="wire_encode",
        decoder_symbol="_ENVELOPE_DECODER",
        inject_fixture_id="nondeterministic_encode",
        expected_signal_id="encoder_identity_skew",
        consumer_fixture_ref="stdout_bytes_fixture",
    ),
    SmokeRow(
        smoke_id="envelope_decode_parity",
        invariant_kind=SmokeInvariantKind.DECODE_PARITY,
        owner_symbol="_emit_envelope",
        decoder_symbol="_ENVELOPE_DECODER",
        inject_fixture_id="shadow_decoder",
        expected_signal_id="wire_decode_type_mismatch",
        consumer_fixture_ref="stdout_bytes_fixture",
    ),
    SmokeRow(
        smoke_id="cap_truncation_spill",
        invariant_kind=SmokeInvariantKind.CAP_TRUNCATED_ARTIFACT_SPILL,
        owner_symbol="_emit",
        decoder_symbol="_ENVELOPE_DECODER",
        inject_fixture_id="long_defect_tuple",
        expected_signal_id="cap_truncation_without_spill",
        consumer_fixture_ref="history_spill_fixture",
    ),
    SmokeRow(
        smoke_id="injected_validation_gate",
        invariant_kind=SmokeInvariantKind.STAGE_INJECTION,
        owner_symbol="TypeAdapter",
        decoder_symbol="_ENVELOPE_DECODER",
        inject_fixture_id="validation_inject",
        expected_signal_id="validation_field_path_fault",
        consumer_fixture_ref="stdout_bytes_fixture",
    ),
    SmokeRow(
        smoke_id="injected_proof_gate",
        invariant_kind=SmokeInvariantKind.STAGE_INJECTION,
        owner_symbol="validate_detail",
        decoder_symbol="_ENVELOPE_DECODER",
        inject_fixture_id="proof_inject",
        expected_signal_id="detail_proof_failure",
        consumer_fixture_ref="stdout_bytes_fixture",
    ),
    SmokeRow(
        smoke_id="subprocess_child_fold",
        invariant_kind=SmokeInvariantKind.SUBPROCESS_ENVELOPE,
        owner_symbol="receipt",
        decoder_symbol="_ENVELOPE_DECODER",
        inject_fixture_id="foreign_child_bytes",
        expected_signal_id="wire_decode_type_mismatch",
        consumer_fixture_ref="subprocess_stdout_fixture",
    ),
)

EVOLUTION_OBLIGATIONS: Final[tuple[EvolutionObligationRow, ...]] = (
    EvolutionObligationRow(
        consumer_id="cli_stdout_parser",
        upstream_artifact="bytes",
        decode_owner="_ENVELOPE_DECODER",
        version_gate_fields=("schema_version", "claim", "verb"),
        proof_policy=ProofPolicy.REQUIRED_ON_SUCCESS_DETAIL,
        surface_ids=("stdout_envelope",),
        lattice_transition_ids=("serialization_exit_to_consumer_decode",),
    ),
    EvolutionObligationRow(
        consumer_id="subprocess_parent_fold",
        upstream_artifact="bytes",
        decode_owner="_ENVELOPE_DECODER",
        version_gate_fields=("schema_version", "claim", "verb"),
        proof_policy=ProofPolicy.REQUIRED_ON_SUCCESS_DETAIL,
        surface_ids=("stdout_envelope", "subprocess_child_stdout"),
        lattice_transition_ids=("subprocess_child_stdout_fold", "serialization_exit_to_consumer_decode"),
    ),
    EvolutionObligationRow(
        consumer_id="history_replay",
        upstream_artifact="bytes",
        decode_owner="_ENVELOPE_DECODER",
        version_gate_fields=("schema_version", "claim", "verb"),
        proof_policy=ProofPolicy.OPTIONAL,
        surface_ids=("history_replay", "stdout_envelope"),
        lattice_transition_ids=("history_envelope_rehydrate", "cache_read_trusted_replay"),
    ),
    EvolutionObligationRow(
        consumer_id="delta_compare",
        upstream_artifact="Envelope",
        decode_owner="",
        version_gate_fields=(),
        proof_policy=ProofPolicy.NEVER,
        surface_ids=("history_replay",),
        lattice_transition_ids=("history_envelope_rehydrate",),
        absence_encoding="EMPTY_with_notes",
    ),
    EvolutionObligationRow(
        consumer_id="probe_cache_key",
        upstream_artifact="bytes",
        decode_owner="wire_encode",
        version_gate_fields=("schema_version",),
        proof_policy=ProofPolicy.NEVER,
        surface_ids=("probe_fingerprint", "cache_bytes"),
        lattice_transition_ids=("cache_write_after_proof",),
    ),
    EvolutionObligationRow(
        consumer_id="invariant_doubler",
        upstream_artifact="Envelope",
        decode_owner="_ENVELOPE_DECODER",
        version_gate_fields=("schema_version", "claim", "verb"),
        proof_policy=ProofPolicy.SKIPPED_ON_FAULT,
        surface_ids=("stdout_envelope",),
        lattice_transition_ids=("emit_fold_to_envelope",),
    ),
    EvolutionObligationRow(
        consumer_id="settings_bootstrap",
        upstream_artifact="env_slice",
        decode_owner="AssaySettings",
        version_gate_fields=(),
        proof_policy=ProofPolicy.OPTIONAL,
        surface_ids=("settings_env",),
        lattice_transition_ids=("settings_snapshot_roundtrip",),
    ),
    EvolutionObligationRow(
        consumer_id="migration_read",
        upstream_artifact="Struct",
        decode_owner="msgspec.convert",
        version_gate_fields=("schema_version",),
        proof_policy=ProofPolicy.NEVER,
        surface_ids=("migration_stored",),
        lattice_transition_ids=("version_migration_single_hop",),
    ),
)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _index_by[T](rows: tuple[T, ...], key: Callable[[T], str]) -> Mapping[str, T]:
    return {key(row): row for row in rows}


def attribution_by_signal(signal_id: str) -> AttributionRow | None:
    """Resolve one attribution row by stable signal slug.

    Returns:
        Matching row when admitted, otherwise ``None``.
    """
    return _ATTRIBUTION_INDEX.get(signal_id)


def smoke_by_id(smoke_id: str) -> SmokeRow | None:
    """Resolve one smoke federation row by stable smoke slug.

    Returns:
        Matching row when admitted, otherwise ``None``.
    """
    return _SMOKE_INDEX.get(smoke_id)


def evolution_by_consumer(consumer_id: str) -> EvolutionObligationRow | None:
    """Resolve one evolution obligation row by consumer slug.

    Returns:
        Matching row when admitted, otherwise ``None``.
    """
    return _EVOLUTION_INDEX.get(consumer_id)


def _duplicate_ids(rows: tuple[object, ...], field: str) -> tuple[str, ...]:
    grouped = tuple((slug, tuple(row for row in rows if getattr(row, field) == slug)) for slug in frozenset(getattr(row, field) for row in rows))
    return tuple(slug for slug, bucket in grouped if len(bucket) > 1)


def _missing_signals(smoke: tuple[SmokeRow, ...], signals: frozenset[str]) -> tuple[str, ...]:
    return tuple(row.expected_signal_id for row in smoke if row.expected_signal_id not in signals)


def _missing_cross_links(evolution: tuple[EvolutionObligationRow, ...]) -> tuple[str, ...]:
    return tuple(row.consumer_id for row in evolution if not row.surface_ids or not row.lattice_transition_ids)


def _orphan_negative_fixtures(attribution: tuple[AttributionRow, ...]) -> tuple[str, ...]:
    cited = frozenset(fixture for row in attribution for fixture in row.negative_fixture_ids)
    return tuple(fixture for fixture in cited if fixture not in NEGATIVE_FIXTURES)


def verify_pipeline_registry() -> None:
    """Prove attribution, smoke, and evolution tables are join-closed at import.

    Raises:
        ValueError: When duplicate ids, broken smoke joins, or incomplete evolution rows appear.
    """
    signals = frozenset(row.signal_id for row in ATTRIBUTION_LATTICE)
    failures = (
        *(
            (f"duplicate {label} id: {slug}",)
            for label, rows, field in (
                ("signal", ATTRIBUTION_LATTICE, "signal_id"),
                ("smoke", SMOKE_FEDERATION, "smoke_id"),
                ("consumer", EVOLUTION_OBLIGATIONS, "consumer_id"),
            )
            for slug in _duplicate_ids(rows, field)
        ),
        *((f"smoke missing attribution: {signal}",) for signal in _missing_signals(SMOKE_FEDERATION, signals)),
        *((f"evolution missing cross-links: {consumer}",) for consumer in _missing_cross_links(EVOLUTION_OBLIGATIONS)),
        *((f"unknown negative fixture: {fixture}",) for fixture in _orphan_negative_fixtures(ATTRIBUTION_LATTICE)),
    )
    match failures:
        case ():
            return
        case msgs:
            raise ValueError("; ".join(msgs))


# --- [COMPOSITION] ----------------------------------------------------------------------

_ATTRIBUTION_INDEX: Final[Mapping[str, AttributionRow]] = _index_by(ATTRIBUTION_LATTICE, attrgetter("signal_id"))
_SMOKE_INDEX: Final[Mapping[str, SmokeRow]] = _index_by(SMOKE_FEDERATION, attrgetter("smoke_id"))
_EVOLUTION_INDEX: Final[Mapping[str, EvolutionObligationRow]] = _index_by(EVOLUTION_OBLIGATIONS, attrgetter("consumer_id"))

verify_pipeline_registry()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ATTRIBUTION_LATTICE",
    "AttributionRow",
    "EVOLUTION_OBLIGATIONS",
    "EvolutionObligationRow",
    "NEGATIVE_FIXTURES",
    "PlaneTag",
    "ProofPolicy",
    "SMOKE_FEDERATION",
    "SmokeInvariantKind",
    "SmokeRow",
    "StageOwner",
    "attribution_by_signal",
    "evolution_by_consumer",
    "smoke_by_id",
    "verify_pipeline_registry",
]
