"""Catalog law matrix for codecs, tool census, and selector algebra."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import pytest

from assay.composition.catalog import BUF_DEFECT_EXIT, launch, PROBE_TIMEOUT_S, select, TOOLS
from assay.core.model import Claim, Input, Language, Mode, Parser, Runner, Tool
from assay.diagnostics import AST_MATCHES, Capture, CAPTURE_ENCODER, CAPTURES, RG_EVENT
from tests.python._testkit.laws import spec
from tests.python._testkit.spec import assert_roundtrip, idempotent
from tests.python._testkit.strategies import (
    resolve as _resolve,  # ruff:ignore[unused-import]
)


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (launch, select)

_VALID_RG_JSON: bytes = b'{"type":"match","data":{"path":{"text":"foo.py"},"lines":{"text":"x = 1\\n"},"line_number":7}}'

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


# --- [CAPTURES_STRUCTURAL]


def test_captures_empty_array_decodes_to_empty_tuple() -> None:
    """Empty capture arrays decode to the tuple contract."""
    result = CAPTURES.decode(b"[]")
    assert result == (), f"expected empty tuple, got {result!r}"


# --- [CAPTURE_ASSERT_ROUNDTRIP]


@spec(Capture, law="capture_assert_roundtrip_oracle")
def test_capture_assert_roundtrip(capture: Capture) -> None:
    assert_roundtrip(capture, Capture)


# --- [AST_MATCHES_STRUCTURAL]


def test_ast_matches_empty_array() -> None:
    result = AST_MATCHES.decode(b"[]")
    assert result == (), f"expected empty tuple, got {result!r}"


def test_ast_matches_field_identity() -> None:
    """AST_MATCHES preserves text, file, and line identity."""
    rows = AST_MATCHES.decode(_AST_MATCH_PAYLOAD)
    assert len(rows) == 1
    m = rows[0]
    assert m.text == "def f()"
    assert m.file == "a.py"
    assert m.lines == "1-3"


# --- [RG_EVENT_ALIAS]


def test_rg_event_type_to_kind_alias() -> None:
    """RG_EVENT maps JSON type onto the kind field."""
    ev = RG_EVENT.decode(_VALID_RG_JSON)
    assert ev.kind == "match"
    assert ev.data.path.text == "foo.py"
    assert ev.data.line_number == 7


def test_rg_event_default_fields() -> None:
    ev = RG_EVENT.decode(b"{}")
    assert not ev.kind
    assert not ev.data.path.text
    assert ev.data.line_number == 0


# --- [TOOLS_CENSUS]


@pytest.mark.parametrize("claim", list(Claim))
def test_catalog_census_every_tool_selects_back(claim: Claim) -> None:
    """Every catalog row selects back through its claim/language axes."""
    failures = [t for t in TOOLS if t.claim is claim and t not in select(t.claim, t.language)]
    assert not failures, f"select did not return these TOOLS rows: {failures}"


# --- [SELECT_TOTAL]


@pytest.mark.parametrize("claim", list(Claim))
def test_select_total_subset_of_tools(claim: Claim) -> None:
    assert all(t in TOOLS for t in select(claim)), f"select({claim!r}) returned rows not in TOOLS"


# --- [SELECT_MONOTONE]


@pytest.mark.parametrize("claim", list(Claim))
@pytest.mark.parametrize("language", list(Language))
def test_select_monotone_language_refinement(claim: Claim, language: Language) -> None:
    """Language refinement only removes rows from a claim selection."""
    broad = select(claim)
    refined = select(claim, language)
    extras = [t for t in refined if t not in broad]
    assert not extras, f"select({claim!r}, {language!r}) returned rows outside select({claim!r}): {extras}"


# --- [SELECT_IDEMPOTENT]


@pytest.mark.parametrize("claim", list(Claim))
def test_select_idempotent(claim: Claim) -> None:
    """Repeated claim selection is deterministic and side-effect-free."""
    idempotent(select(claim), lambda _: select(claim))


# --- [TOOL_GENERATED]


@spec(Tool, law="tool_select_back_for_generated")
def test_tool_generated_instance_selects_back(tool: Tool) -> None:
    """Generated tool axes constrain every selected row."""
    rows = select(tool.claim, tool.language)
    assert all(t.claim is tool.claim and t.language is tool.language for t in rows), (
        "select returned a row whose claim/language mismatches the query axes"
    )


# --- [PARSER_CENSUS]


def test_parser_families_all_declared() -> None:
    """Every diagnostics family is declared by at least one catalog row, and no row invents a family."""
    declared = {t.parser for t in TOOLS}
    assert declared == frozenset(Parser), f"parser families drifted: declared={sorted(declared)}"


def test_parser_rows_key_static_diagnostic_tools() -> None:
    """The argv-sniffing replacement: the exact static rows whose output the fold parses carry their family key."""
    by_name = {(t.name, t.language): t.parser for t in select(Claim.STATIC)}
    assert by_name["ruff", Language.PYTHON] is Parser.RUFF
    assert by_name["ruff-format", Language.PYTHON] is Parser.RUFF_FORMAT
    assert by_name["ty", Language.PYTHON] is Parser.TY
    assert by_name["mypy", Language.PYTHON] is Parser.MYPY
    assert by_name["biome", Language.TYPESCRIPT] is Parser.BIOME
    assert by_name["tsc", Language.TYPESCRIPT] is Parser.TSC
    assert by_name["dotnet-build", Language.DOTNET] is Parser.CS_CONSOLE
    assert by_name["dotnet-format", Language.DOTNET] is Parser.CS_CONSOLE
    assert by_name["lint-imports", Language.PYTHON] is Parser.NONE
    contracts = {(t.name, t.mode): t for t in select(Claim.CONTRACTS, Language.PROTO)}
    assert contracts["buf-lint", Mode.CHECK].parser is Parser.BUF
    assert contracts["buf-format", Mode.CHECK].parser is Parser.NONE


# --- [CONTRACTS_ROWS]


def test_contracts_rows_type_the_defect_exit_and_own_their_input() -> None:
    """The executable buf defect lanes declare exit 100, every contracts row owns its input, and no other row declares one."""
    rows = select(Claim.CONTRACTS)
    assert {t.name for t in rows if t.defect_exit is not None} == {"buf-lint", "buf-format"}
    assert all(t.defect_exit == BUF_DEFECT_EXIT for t in rows if t.defect_exit is not None)
    assert all(t.input is Input.OWNED and t.language is Language.PROTO for t in rows)
    assert all(t.defect_exit is None for t in TOOLS if t.claim is not Claim.CONTRACTS)


def test_contracts_descriptor_image_includes_reachable_imports() -> None:
    build = next(tool for tool in TOOLS if tool.name == "buf-build" and tool.mode is Mode.QUERY)

    assert build.command == ("buf", "build", "libs/contracts", "-o", "{output}", "--as-file-descriptor-set")


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


# --- [STATIC_INPUT_OWNERSHIP]


def test_validate_pyproject_owns_its_single_input() -> None:
    """validate-pyproject owns its single pyproject input."""
    rows = [t for t in select(Claim.STATIC, Language.PYTHON) if t.name == "validate-pyproject"]
    assert len(rows) == 1
    assert rows[0].input is Input.OWNED


def test_static_native_fixers_are_scoped_rows() -> None:
    """Native static fixers stay scoped, with Biome carrying the write row."""
    dotnet_format = [t for t in select(Claim.STATIC, Language.DOTNET) if t.name == "dotnet-format"]
    biome_write = [t for t in select(Claim.STATIC, Language.TYPESCRIPT) if t.name == "biome" and t.mode is Mode.WRITE]
    assert dotnet_format
    assert all(t.input is Input.INCLUDE for t in dotnet_format)
    assert len(biome_write) == 1
    assert biome_write[0].command[:2] == ("biome", "check")
    assert "--write" in biome_write[0].command


# --- [LAUNCH_SPELLER]


@pytest.mark.parametrize("tool", TOOLS, ids=[f"{i}-{t.name}-{t.claim.value}-{t.mode.value}" for i, t in enumerate(TOOLS)])
def test_launch_is_the_one_prefix_speller(tool: Tool) -> None:
    """launch() derives every row's launcher prefix: uv lock + dependency groups for UV, raw prefix otherwise."""
    prefix = launch(tool)
    match tool.runner:
        case Runner.UV:
            groups = tuple(part for group in tool.uv_groups() for part in ("--group", group.value))
            assert prefix == ("uv", "run", "--locked", *groups)
        case _:
            assert prefix == tool.runner.prefix


# --- [PROBE_TIMEOUT]


def test_probe_timeout_bounds_query_probe_rows() -> None:
    """PROBE_TIMEOUT_S is a positive bound and every probe/program template row that declares it stays under the mutation ceiling."""
    assert PROBE_TIMEOUT_S > 0
    probes = tuple(t for t in TOOLS if t.name in {"git-head", "git-dirty", "tool-probe"})
    assert probes, "probe rows must exist in the catalog"
    assert all(t.timeout == PROBE_TIMEOUT_S for t in probes)
