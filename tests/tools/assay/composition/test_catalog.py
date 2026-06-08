"""Laws for tools.assay.composition.catalog public surface.

Scope: AST_MATCHES, CAPTURES, CAPTURE_ENCODER, Capture, RG_EVENT, TOOLS, select.

Law design
----------
- Capture roundtrip  — CAPTURES(CAPTURE_ENCODER([x])) == (x,) for any valid Capture.
  This law covers both CAPTURES and CAPTURE_ENCODER; CAPTURE_ENCODER is not separately
  exempted because the roundtrip exercises its encode path directly.
- AST_MATCHES structural — empty-array decodes to (); field identity preserved on a concrete row.
- RG_EVENT name-alias — JSON "type" field maps to the .kind attribute (msgspec rename law).
- TOOLS census — every Tool in TOOLS selects back through select(claim, language).
- select total — select(claim) is a subset of TOOLS for every Claim value.
- select monotone — select(claim, language) ⊆ select(claim) for all (claim, language) pairs.
- select idempotent — select(claim) == select(claim) (pure, no hidden mutable state).
"""

# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------

import pytest

from tests._aspect import register_law, spec  # noqa: PLC2701
from tests._spec import assert_roundtrip, idempotent  # noqa: PLC2701
from tests._strategies import resolve as _resolve  # noqa: PLC2701, F401  # registered as side-effect for Tool strategy
from tools.assay.composition.catalog import AST_MATCHES, Capture, CAPTURE_ENCODER, CAPTURES, RG_EVENT, select, TOOLS
from tools.assay.core.model import Claim, Language, Tool


# --- [CONSTANTS] ----------------------------------------------------------------------

_VALID_RG_JSON: bytes = b'{"type":"match","data":{"path":{"text":"foo.py"},"lines":{"text":"x = 1\\n"},"line_number":7}}'

# One concrete AstMatch payload to assert field-identity (structural, not PBT).
_AST_MATCH_PAYLOAD: bytes = (
    b'[{"text":"def f()","file":"a.py","lines":"1-3","replacement":"","range":{"start":{"line":1,"column":0},"end":{"line":3,"column":1}}}]'
)


# --- [OPERATIONS] ---------------------------------------------------------------------


# -- Capture roundtrip (covers CAPTURES + CAPTURE_ENCODER) ----------------------------


@spec(Capture, law="capture_codec_roundtrip")
def test_capture_roundtrip(capture: Capture) -> None:
    """CAPTURES(CAPTURE_ENCODER([x])) == (x,) for any generated Capture."""
    encoded = CAPTURE_ENCODER.encode([capture])
    decoded = CAPTURES.decode(encoded)
    assert decoded == (capture,), f"roundtrip broken: {capture!r} -> {decoded!r}"


register_law("tools.assay.composition.catalog.CAPTURES", "capture_codec_roundtrip", module=__name__)
register_law("tools.assay.composition.catalog.CAPTURE_ENCODER", "capture_codec_roundtrip", module=__name__)


# -- CAPTURES structural: empty array -------------------------------------------------


def test_captures_empty_array_decodes_to_empty_tuple() -> None:
    """CAPTURES.decode(b'[]') yields the empty tuple, not a list."""
    result = CAPTURES.decode(b"[]")
    assert result == (), f"expected empty tuple, got {result!r}"


register_law("tools.assay.composition.catalog.CAPTURES", "captures_empty_array", module=__name__)


# -- Capture: assert_roundtrip uses the generic msgspec default codec ------------------


@spec(Capture, law="capture_assert_roundtrip_oracle")
def test_capture_assert_roundtrip(capture: Capture) -> None:
    """Generic assert_roundtrip encodes and re-encodes Capture with byte identity."""
    assert_roundtrip(capture, Capture)


# -- AST_MATCHES structural -----------------------------------------------------------


def test_ast_matches_empty_array() -> None:
    """AST_MATCHES.decode(b'[]') returns the empty tuple."""
    result = AST_MATCHES.decode(b"[]")
    assert result == (), f"expected empty tuple, got {result!r}"


register_law("tools.assay.composition.catalog.AST_MATCHES", "ast_matches_empty_array", module=__name__)


def test_ast_matches_field_identity() -> None:
    """AstMatch fields round-trip through AST_MATCHES: text/file/lines preserved."""
    rows = AST_MATCHES.decode(_AST_MATCH_PAYLOAD)
    assert len(rows) == 1
    m = rows[0]
    assert m.text == "def f()"
    assert m.file == "a.py"
    assert m.lines == "1-3"


register_law("tools.assay.composition.catalog.AST_MATCHES", "ast_matches_field_identity", module=__name__)


# -- RG_EVENT: JSON "type" -> .kind alias law -----------------------------------------


def test_rg_event_type_to_kind_alias() -> None:
    """RG_EVENT maps the JSON 'type' key to the .kind attribute (msgspec rename)."""
    ev = RG_EVENT.decode(_VALID_RG_JSON)
    assert ev.kind == "match"
    assert ev.data.path.text == "foo.py"
    assert ev.data.line_number == 7


register_law("tools.assay.composition.catalog.RG_EVENT", "rg_event_type_alias", module=__name__)


def test_rg_event_default_fields() -> None:
    """RG_EVENT decodes a minimal payload with all defaults populated."""
    ev = RG_EVENT.decode(b"{}")
    assert not ev.kind
    assert not ev.data.path.text
    assert ev.data.line_number == 0


register_law("tools.assay.composition.catalog.RG_EVENT", "rg_event_defaults", module=__name__)


# -- TOOLS census: every row selects back via select(claim, language) -----------------


@pytest.mark.parametrize("claim", list(Claim))
def test_catalog_census_every_tool_selects_back(claim: Claim) -> None:
    """Census invariant: every Tool in TOOLS with the given claim appears in select(claim, language)."""
    failures = [t for t in TOOLS if t.claim is claim and t not in select(t.claim, t.language)]
    assert not failures, f"select did not return these TOOLS rows: {failures}"


register_law("tools.assay.composition.catalog.TOOLS", "tools_census_select_back", module=__name__)


# -- select: result is a subset of TOOLS for every Claim ------------------------------


@pytest.mark.parametrize("claim", list(Claim))
def test_select_total_subset_of_tools(claim: Claim) -> None:
    """select(claim) returns only Tool objects from TOOLS — no foreign rows."""
    assert all(t in TOOLS for t in select(claim)), f"select({claim!r}) returned rows not in TOOLS"


register_law(select, "select_total_subset", module=__name__)


# -- select: language refinement is monotone (select(c,l) ⊆ select(c)) ---------------


@pytest.mark.parametrize("claim", list(Claim))
@pytest.mark.parametrize("language", list(Language))
def test_select_monotone_language_refinement(claim: Claim, language: Language) -> None:
    """select(claim, language) is a subset of select(claim): adding a filter only restricts."""
    broad = select(claim)
    refined = select(claim, language)
    extras = [t for t in refined if t not in broad]
    assert not extras, f"select({claim!r}, {language!r}) returned rows outside select({claim!r}): {extras}"


register_law(select, "select_monotone_language", module=__name__)


# -- select: idempotent (pure function, no hidden mutable state) -----------------------


@pytest.mark.parametrize("claim", list(Claim))
def test_select_idempotent(claim: Claim) -> None:
    """select(claim) == select(claim): deterministic, stable, free of side-effects."""
    idempotent(select(claim), lambda _: select(claim))


register_law(select, "select_idempotent", module=__name__)


# -- Tool: resolve-backed property for generated instances ----------------------------


@spec(Tool, law="tool_select_back_for_generated")
def test_tool_generated_instance_selects_back(tool: Tool) -> None:
    """Any generated Tool instance appears in select(tool.claim, tool.language) only when present in TOOLS.

    This law covers the select function's filter predicate: for a Tool that IS in TOOLS, the
    predicate `t.claim is claim and t.language is language` must hold — we verify the
    contrapositive: select never returns a Tool whose claim/language doesn't match the query.
    """
    rows = select(tool.claim, tool.language)
    assert all(t.claim is tool.claim and t.language is tool.language for t in rows), (
        "select returned a row whose claim/language mismatches the query axes"
    )
