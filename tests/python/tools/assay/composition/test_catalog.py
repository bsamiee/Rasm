"""Catalog law matrix for codecs, tool census, and selector algebra."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import pytest

from tests.python._testkit.laws import register_law, spec
from tests.python._testkit.spec import assert_roundtrip, idempotent
from tests.python._testkit.strategies import resolve as _resolve  # noqa: F401  # registers the Tool Hypothesis strategy on import; no call site
from tools.assay.composition.catalog import AST_MATCHES, Capture, CAPTURE_ENCODER, CAPTURES, RG_EVENT, select, TOOLS
from tools.assay.core.model import Claim, Input, Language, Mode, Tool


# --- [CONSTANTS] ------------------------------------------------------------------------

_VALID_RG_JSON: bytes = b'{"type":"match","data":{"path":{"text":"foo.py"},"lines":{"text":"x = 1\\n"},"line_number":7}}'

# Concrete payload pins field mapping; generated rows only prove shape.
_AST_MATCH_PAYLOAD: bytes = (
    b'[{"text":"def f()","file":"a.py","lines":"1-3","replacement":"","range":{"start":{"line":1,"column":0},"end":{"line":3,"column":1}}}]'
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CAPTURE_ROUNDTRIP]


@spec(Capture, law="capture_codec_roundtrip")
def test_capture_roundtrip(capture: Capture) -> None:
    """Generated Capture rows survive the encoder/decoder pair."""
    encoded = CAPTURE_ENCODER.encode([capture])
    decoded = CAPTURES.decode(encoded)
    assert decoded == (capture,), f"roundtrip broken: {capture!r} -> {decoded!r}"


register_law("tools.assay.composition.catalog.CAPTURES", "capture_codec_roundtrip", module=__name__)
register_law("tools.assay.composition.catalog.CAPTURE_ENCODER", "capture_codec_roundtrip", module=__name__)

# --- [CAPTURES_STRUCTURAL]


def test_captures_empty_array_decodes_to_empty_tuple() -> None:
    """Empty capture arrays decode to the tuple contract."""
    result = CAPTURES.decode(b"[]")
    assert result == (), f"expected empty tuple, got {result!r}"


register_law("tools.assay.composition.catalog.CAPTURES", "captures_empty_array", module=__name__)

# --- [CAPTURE_ASSERT_ROUNDTRIP]


@spec(Capture, law="capture_assert_roundtrip_oracle")
def test_capture_assert_roundtrip(capture: Capture) -> None:
    assert_roundtrip(capture, Capture)


# --- [AST_MATCHES_STRUCTURAL]


def test_ast_matches_empty_array() -> None:
    result = AST_MATCHES.decode(b"[]")
    assert result == (), f"expected empty tuple, got {result!r}"


register_law("tools.assay.composition.catalog.AST_MATCHES", "ast_matches_empty_array", module=__name__)


def test_ast_matches_field_identity() -> None:
    """AST_MATCHES preserves text, file, and line identity."""
    rows = AST_MATCHES.decode(_AST_MATCH_PAYLOAD)
    assert len(rows) == 1
    m = rows[0]
    assert m.text == "def f()"
    assert m.file == "a.py"
    assert m.lines == "1-3"


register_law("tools.assay.composition.catalog.AST_MATCHES", "ast_matches_field_identity", module=__name__)

# --- [RG_EVENT_ALIAS]


def test_rg_event_type_to_kind_alias() -> None:
    """RG_EVENT maps JSON type onto the kind field."""
    ev = RG_EVENT.decode(_VALID_RG_JSON)
    assert ev.kind == "match"
    assert ev.data.path.text == "foo.py"
    assert ev.data.line_number == 7


register_law("tools.assay.composition.catalog.RG_EVENT", "rg_event_type_alias", module=__name__)


def test_rg_event_default_fields() -> None:
    ev = RG_EVENT.decode(b"{}")
    assert not ev.kind
    assert not ev.data.path.text
    assert ev.data.line_number == 0


register_law("tools.assay.composition.catalog.RG_EVENT", "rg_event_defaults", module=__name__)

# --- [TOOLS_CENSUS]


@pytest.mark.parametrize("claim", list(Claim))
def test_catalog_census_every_tool_selects_back(claim: Claim) -> None:
    """Every catalog row selects back through its claim/language axes."""
    failures = [t for t in TOOLS if t.claim is claim and t not in select(t.claim, t.language)]
    assert not failures, f"select did not return these TOOLS rows: {failures}"


register_law("tools.assay.composition.catalog.TOOLS", "tools_census_select_back", module=__name__)

# --- [SELECT_TOTAL]


@pytest.mark.parametrize("claim", list(Claim))
def test_select_total_subset_of_tools(claim: Claim) -> None:
    assert all(t in TOOLS for t in select(claim)), f"select({claim!r}) returned rows not in TOOLS"


register_law(select, "select_total_subset", module=__name__)

# --- [SELECT_MONOTONE]


@pytest.mark.parametrize("claim", list(Claim))
@pytest.mark.parametrize("language", list(Language))
def test_select_monotone_language_refinement(claim: Claim, language: Language) -> None:
    """Language refinement only removes rows from a claim selection."""
    broad = select(claim)
    refined = select(claim, language)
    extras = [t for t in refined if t not in broad]
    assert not extras, f"select({claim!r}, {language!r}) returned rows outside select({claim!r}): {extras}"


register_law(select, "select_monotone_language", module=__name__)

# --- [SELECT_IDEMPOTENT]


@pytest.mark.parametrize("claim", list(Claim))
def test_select_idempotent(claim: Claim) -> None:
    """Repeated claim selection is deterministic and side-effect-free."""
    idempotent(select(claim), lambda _: select(claim))


register_law(select, "select_idempotent", module=__name__)

# --- [TOOL_GENERATED]


@spec(Tool, law="tool_select_back_for_generated")
def test_tool_generated_instance_selects_back(tool: Tool) -> None:
    """Generated tool axes constrain every selected row."""
    rows = select(tool.claim, tool.language)
    assert all(t.claim is tool.claim and t.language is tool.language for t in rows), (
        "select returned a row whose claim/language mismatches the query axes"
    )


# --- [MUTATION_INPUT_OWNERSHIP]


def test_mutation_rows_own_input_placement() -> None:
    """Mutation rows must own input placement.

    Input.NONE re-appends routed files as mutmut positional filters, which select zero mutant
    names and abort the run; OWNED/PROJECT keeps file placement with the row.
    """
    rows = [t for claim in Claim for t in select(claim) if t.mode is Mode.MUTATION]
    assert rows, "census expects at least one mutation row"
    offenders = [t.name for t in rows if t.input is Input.NONE]
    assert not offenders, f"mutation rows must own input placement, got NONE on: {offenders}"


register_law(select, "mutation_rows_own_input_placement", module=__name__)

# --- [STATIC_INPUT_OWNERSHIP]


def test_validate_pyproject_owns_its_single_input() -> None:
    """validate-pyproject owns its single pyproject input."""
    rows = [t for t in select(Claim.STATIC, Language.PYTHON) if t.name == "validate-pyproject"]
    assert len(rows) == 1
    assert rows[0].input is Input.OWNED


register_law(select, "validate_pyproject_owns_single_input", module=__name__)


def test_static_native_fixers_are_scoped_rows() -> None:
    """Native static fixers stay scoped, with Biome carrying the write row."""
    csharp_format = [t for t in select(Claim.STATIC, Language.CSHARP) if t.name == "dotnet-format"]
    biome_write = [t for t in select(Claim.STATIC, Language.TYPESCRIPT) if t.name == "biome" and t.mode is Mode.WRITE]
    assert csharp_format
    assert all(t.input is Input.INCLUDE for t in csharp_format)
    assert len(biome_write) == 1
    assert biome_write[0].command[:2] == ("biome", "check")
    assert "--write" in biome_write[0].command


register_law(select, "static_native_fixers_are_scoped_rows", module=__name__)
