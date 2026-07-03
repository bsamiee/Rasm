"""Law matrix for the Python semantic analyzer: scope engine, rule diagnostics, and output contracts."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

import msgspec
import pytest

from tools.py_analyzer.analyzer import analyze_paths, classify_scope, PY_ANALYZER_ROOT
from tools.py_analyzer.cli import emit, main
from tools.py_analyzer.rules import Diagnostic, diagnostic, OutputFormat, RuleCategory, RuleId, RULES, Scope, Severity


if TYPE_CHECKING:
    from pathlib import Path

    from tests.python._testkit.seams import TmpRoot


# --- [TYPES] ----------------------------------------------------------------------------

type DiagnosticRow = tuple[str, str, int, int, str, str]
type DiagnosticJson = dict[str, str | int]

# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (analyze_paths, classify_scope, Diagnostic, diagnostic, emit, main)

PY_ANALYZER_SAMPLE = "/".join((*PY_ANALYZER_ROOT, "sample.py"))

_IMPERATIVE_BOOL_BODY = "def run(value: object) -> bool:\n    if value:\n        return True\n    return False\n"

_VALID_EXEMPTION = (
    "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=protocol-required ticket=RASM-123 expires=2999-01-01 rationale=external protocol gate"
)

# Each row perturbs exactly one metadata field of the valid header; all must fail closed to governance.
_INVALID_EXEMPTIONS = (
    ("unknown-rule", _VALID_EXEMPTION.replace("rule=PYS0001", "rule=PYS9999")),
    ("bad-reason", _VALID_EXEMPTION.replace("reason=protocol-required", "reason=bad-reason")),
    ("bad-ticket", _VALID_EXEMPTION.replace("ticket=RASM-123", "ticket=bad-ticket")),
    ("expired", _VALID_EXEMPTION.replace("expires=2999-01-01", "expires=2000-01-01")),
    ("malformed-date", _VALID_EXEMPTION.replace("expires=2999-01-01", "expires=2026-99-99")),
    ("empty-rationale", _VALID_EXEMPTION.replace("rationale=external protocol gate", "rationale=")),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def diagnostic_rows(root: Path, paths: tuple[Path, ...]) -> tuple[DiagnosticRow, ...]:
    return tuple(
        (diag.rule_id.value, diag.path.relative_to(root).as_posix(), diag.line, diag.column, diag.title, diag.category.value)
        for diag in analyze_paths(root, paths)
    )


def test_rule_catalog_is_total_over_rule_ids() -> None:
    assert frozenset(RULES) == frozenset(RuleId)
    assert all(rule.title and rule.category and rule.message for rule in RULES.values())


@pytest.mark.parametrize(
    "relative, source, expected",
    [
        (
            "src/domain/flow.py",
            "def run(value: Input) -> Output:\n    if value:\n        return Output()\n    return Output()\n",
            ("PYS0001", "src/domain/flow.py", 2, 5, "DomainImperativeFlow", "FunctionalDiscipline"),
        ),
        (
            "src/domain/for_flow.py",
            "def run(items: Input) -> Output:\n    for item in items:\n        return Output()\n    return Output()\n",
            ("PYS0001", "src/domain/for_flow.py", 2, 5, "DomainImperativeFlow", "FunctionalDiscipline"),
        ),
        (
            "src/domain/while_flow.py",
            "def run(flag: Input) -> Output:\n    while flag:\n        return Output()\n    return Output()\n",
            ("PYS0001", "src/domain/while_flow.py", 2, 5, "DomainImperativeFlow", "FunctionalDiscipline"),
        ),
        (
            "src/domain/try_flow.py",
            "def run() -> Output:\n    try:\n        return Output()\n    except OSError:\n        return Output()\n",
            ("PYS0001", "src/domain/try_flow.py", 2, 5, "DomainImperativeFlow", "FunctionalDiscipline"),
        ),
        (
            "src/domain/raise_flow.py",
            "def run() -> Output:\n    raise ValueError('x')\n",
            ("PYS0001", "src/domain/raise_flow.py", 2, 5, "DomainImperativeFlow", "FunctionalDiscipline"),
        ),
        ("src/adapters/http.py", _IMPERATIVE_BOOL_BODY, ("PYS0002", "src/adapters/http.py", 2, 5, "BoundaryExemptionGovernance", "Governance")),
        (
            "src/domain/signature.py",
            "def render(value: str) -> User:\n    return User()\n",
            ("PYS0003", "src/domain/signature.py", 1, 1, "PrimitivePublicSignature", "TypeDiscipline"),
        ),
        (
            "src/domain/fallible.py",
            "def maybe_user(value: UserInput) -> User | None:\n    return None\n",
            ("PYS0004", "src/domain/fallible.py", 1, 1, "FallibleReturnRail", "FunctionalDiscipline"),
        ),
        (
            "src/domain/private.py",
            (
                "def _normalize(value: UserInput) -> UserInput:\n"
                "    return value\n\n"
                "def run(value: UserInput) -> UserInput:\n"
                "    return _normalize(value)\n"
            ),
            ("PYS0005", "src/domain/private.py", 1, 1, "SingleUsePrivateFunction", "SurfaceArea"),
        ),
        (
            "src/domain/models_b.py",
            "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass B:\n    value: UserId\n",
            ("PYS0006", "src/domain/models_b.py", 4, 1, "DuplicateModelShape", "TypeDiscipline"),
        ),
    ],
)
def test_rule_cases_emit_exact_diagnostic(kit: TmpRoot[None], relative: str, source: str, expected: DiagnosticRow) -> None:
    kit.write("src/domain/models_a.py", "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass A:\n    value: UserId\n")
    path = kit.write(relative, source)
    rows = tuple(row for row in diagnostic_rows(kit.root, (path, kit.root / "src/domain/models_a.py")) if row[1] == relative)
    assert rows == (expected,)


@pytest.mark.parametrize(
    "prelude, expected",
    [
        *(
            pytest.param(f"{header}\n", (("PYS0002", "src/adapters/http.py", 3, 5, "BoundaryExemptionGovernance", "Governance"),), id=tag)
            for tag, header in _INVALID_EXEMPTIONS
        ),
        pytest.param(
            f"{_VALID_EXEMPTION}\n\n\n\n",
            (("PYS0002", "src/adapters/http.py", 6, 5, "BoundaryExemptionGovernance", "Governance"),),
            id="window-exceeded",
        ),
        pytest.param(f"{_VALID_EXEMPTION}\n", (), id="valid-suppresses"),
    ],
)
def test_boundary_exemption_matrix(kit: TmpRoot[None], prelude: str, expected: tuple[DiagnosticRow, ...]) -> None:
    path = kit.write("src/adapters/http.py", prelude + _IMPERATIVE_BOOL_BODY)
    assert diagnostic_rows(kit.root, (path,)) == expected


@pytest.mark.parametrize(
    "relative, scope, expected_rule",
    [
        ("src/domain/flow.py", Scope.domain, "PYS0001"),
        ("src/application/flow.py", Scope.application, "PYS0001"),
        ("src/domain/adapters/http.py", Scope.boundary, "PYS0002"),
        ("src/neutral/flow.py", Scope.neutral, ""),
        (PY_ANALYZER_SAMPLE, Scope.tooling, ""),
        (".claude/skills/example/scripts/tool.py", Scope.tooling, ""),
    ],
)
def test_scope_matrix(kit: TmpRoot[None], relative: str, scope: Scope, expected_rule: str) -> None:
    path = kit.write(relative, "def run(value: Input) -> Output:\n    if value:\n        return Output()\n    return Output()\n")
    assert classify_scope(path, kit.root) is scope
    rows = diagnostic_rows(kit.root, (path,))
    assert tuple(row[0] for row in rows) == ((expected_rule,) if expected_rule else ())


@pytest.mark.parametrize(
    "source, expected",
    [
        ("def maybe_user(value: UserInput) -> User | None:\n    return None\n", ("PYS0004",)),
        ("def maybe_user(value: UserInput) -> None | User:\n    return None\n", ("PYS0004",)),
        ("def parse_user(value: UserInput) -> User:\n    return User()\n", ("PYS0004",)),
        ("def maybe_user(value: UserInput) -> Result[User, DomainError]:\n    return result\n", ()),
        ("def maybe_user(value: UserInput) -> Option[User]:\n    return user\n", ()),
    ],
)
def test_fallible_return_rail_matrix(kit: TmpRoot[None], source: str, expected: tuple[str, ...]) -> None:
    path = kit.write("src/domain/fallible.py", source)
    assert tuple(row[0] for row in diagnostic_rows(kit.root, (path,))) == expected


@pytest.mark.parametrize(
    "relative, source, expected",
    [
        pytest.param(
            "src/domain/rail.py",
            "def run(value: Result[User, DomainError]) -> User:\n    return value.unwrap()\n",
            (("PYS0007", "src/domain/rail.py", 2, 12, "RailEscape", "FunctionalDiscipline"),),
            id="unwrap-domain",
        ),
        pytest.param(
            "src/application/rail.py",
            "def run(value: Option[User]) -> User:\n    return value.value_or(User())\n",
            (("PYS0007", "src/application/rail.py", 2, 12, "RailEscape", "FunctionalDiscipline"),),
            id="value_or-application",
        ),
        pytest.param("src/domain/rail.py", "def run(value: Option[User]) -> User:\n    return value.default_value(User())\n", (), id="default-value"),
        pytest.param("src/boundary/rail.py", "def run(value: Result[User, DomainError]) -> User:\n    return value.unwrap()\n", (), id="boundary"),
        pytest.param(PY_ANALYZER_SAMPLE, "def run(value: Result[User, DomainError]) -> User:\n    return value.unwrap()\n", (), id="tooling"),
        pytest.param("src/domain/rail.py", "def run(value: User) -> User:\n    return value.unwraps()\n", (), id="non-rail-name"),
    ],
)
def test_rail_escape_matrix(kit: TmpRoot[None], relative: str, source: str, expected: tuple[DiagnosticRow, ...]) -> None:
    path = kit.write(relative, source)
    assert diagnostic_rows(kit.root, (path,)) == expected


@pytest.mark.parametrize(
    "source, expected",
    [
        *(
            (source, ("PYS0003",))
            for source in (
                "def run(value: str | None) -> User:\n    return User()\n",
                "def run(value: Sequence[str]) -> User:\n    return User()\n",
                "def run(value: Mapping[str, User]) -> User:\n    return User()\n",
                "def run(value: tuple[User, ...]) -> User:\n    return User()\n",
                "def run(*values: str) -> User:\n    return User()\n",
                "def run(**values: int) -> User:\n    return User()\n",
                "def run(value: UserInput) -> Result[str, DomainError]:\n    return result\n",
                "def run(value: builtins.str) -> User:\n    return User()\n",
                "def run(value: object) -> User:\n    return User()\n",
                "def run(value: Any) -> User:\n    return User()\n",
                "def run(value: typing.Any) -> User:\n    return User()\n",
            )
        ),
        ("def run(value: UserInput) -> User:\n    return User()\n", ()),
    ],
)
def test_primitive_signature_matrix(kit: TmpRoot[None], source: str, expected: tuple[str, ...]) -> None:
    path = kit.write("src/domain/signature.py", source)
    assert tuple(row[0] for row in diagnostic_rows(kit.root, (path,))) == expected


@pytest.mark.parametrize(
    "source, expected",
    [
        (
            (
                "def _normalize(value: UserInput) -> UserInput:\n"
                "    return value\n\n"
                "def run(value: UserInput) -> UserInput:\n"
                "    return _normalize(value)\n"
            ),
            ("PYS0005",),
        ),
        (
            (
                "def _normalize(value: UserInput) -> UserInput:\n"
                "    return value\n\n"
                "def run(value: UserInput) -> UserInput:\n"
                "    return _normalize(_normalize(value))\n"
            ),
            (),
        ),
        (("def _normalize(value: UserInput) -> UserInput:\n    return value\n\ndef run(value: UserInput) -> UserInput:\n    return value\n"), ()),
        (
            (
                "from typing import overload\n\n"
                "@overload\n"
                "def _normalize(value: UserInput) -> UserInput: ...\n\n"
                "def run(value: UserInput) -> UserInput:\n"
                "    return _normalize(value)\n"
            ),
            (),
        ),
    ],
)
def test_single_use_private_function_matrix(kit: TmpRoot[None], source: str, expected: tuple[str, ...]) -> None:
    path = kit.write("src/domain/private.py", source)
    assert tuple(row[0] for row in diagnostic_rows(kit.root, (path,))) == expected


@pytest.mark.parametrize(
    "source, expected",
    [
        (
            "from dataclasses import dataclass\n\n@dataclass\nclass User:\n    value: UserId\n",
            (("PYS0008", "src/domain/model.py", 4, 1, "ModelImmutability", "TypeDiscipline"),),
        ),
        (
            "from dataclasses import dataclass\n\n@dataclass(frozen=True)\nclass User:\n    value: UserId\n",
            (("PYS0008", "src/domain/model.py", 4, 1, "ModelImmutability", "TypeDiscipline"),),
        ),
        ("class User(BaseModel):\n    value: UserId\n", (("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),)),
        ("class User(msgspec.Struct):\n    value: UserId\n", (("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),)),
        ("class User(RootModel):\n    root: UserId\n", (("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),)),
        ("class Settings(BaseSettings):\n    value: UserId\n", (("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),)),
        *(
            (source, ())
            for source in (
                "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass User:\n    value: UserId\n",
                "class User(BaseModel, frozen=True):\n    value: UserId\n",
                "class User(BaseModel):\n    model_config = ConfigDict(frozen=True)\n    value: UserId\n",
                "class User(BaseModel):\n    model_config = pydantic.ConfigDict(frozen=True)\n    value: UserId\n",
                "class User(RootModel, frozen=True):\n    root: UserId\n",
                "class User(RootModel):\n    model_config = ConfigDict(frozen=True)\n    root: UserId\n",
                "class Settings(BaseSettings):\n    model_config = SettingsConfigDict(frozen=True)\n    value: UserId\n",
                (
                    "class Settings(pydantic_settings.BaseSettings):\n"
                    "    model_config = pydantic_settings.SettingsConfigDict(frozen=True)\n"
                    "    value: UserId\n"
                ),
                "class User(msgspec.Struct, frozen=True):\n    value: UserId\n",
            )
        ),
    ],
)
def test_model_immutability_matrix(kit: TmpRoot[None], source: str, expected: tuple[DiagnosticRow, ...]) -> None:
    path = kit.write("src/domain/model.py", source)
    assert diagnostic_rows(kit.root, (path,)) == expected


@pytest.mark.parametrize(
    "annotation, expected",
    [
        *(
            (annotation, (("PYS0009", "src/domain/model.py", 5, 5, "MutableModelField", "TypeDiscipline"),))
            for annotation in (
                "list[UserId]",
                "dict[str, UserId]",
                "set[UserId]",
                "MutableMapping[str, UserId]",
                "MutableSequence[UserId]",
                "MutableSet[UserId]",
                "collections.abc.MutableMapping[str, UserId]",
                "builtins.list[UserId]",
                "Annotated[list[UserId], Marker]",
                "Result[tuple[list[UserId]], DomainError]",
                "Option[Sequence[dict[str, UserId]]]",
            )
        ),
        *(
            (annotation, ())
            for annotation in ("tuple[UserId, ...]", "frozenset[UserId]", "Mapping[str, UserId]", "Sequence[UserId]", "Block[UserId]")
        ),
    ],
)
def test_model_field_mutability_matrix(kit: TmpRoot[None], annotation: str, expected: tuple[DiagnosticRow, ...]) -> None:
    path = kit.write(
        "src/domain/model.py", (f"from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass User:\n    value: {annotation}\n")
    )
    assert diagnostic_rows(kit.root, (path,)) == expected


@pytest.mark.parametrize(
    "relative, source, expected",
    [
        *(
            (
                "src/domain/effect.py",
                f"@{decorator}\ndef run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n    return value.or_else_with(recover)\n",
                (("PYS0010", "src/domain/effect.py", 3, 12, "RecoveryInsideEffectBuilder", "FunctionalDiscipline"),),
            )
            for decorator in ("effect.result", "effect.async_result", "result", "async_result", "effect.result[User, DomainError]()")
        ),
        (
            "src/domain/effect.py",
            "def run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n    return value.or_else_with(recover)\n",
            (),
        ),
        (
            "src/boundary/effect.py",
            "@effect.result\ndef run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n    return value.or_else_with(recover)\n",
            (),
        ),
        (
            "src/domain/effect.py",
            "@effect.result\ndef run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n    return value.map(transform)\n",
            (),
        ),
    ],
)
def test_recovery_inside_effect_builder_matrix(kit: TmpRoot[None], relative: str, source: str, expected: tuple[DiagnosticRow, ...]) -> None:
    path = kit.write(relative, source)
    assert diagnostic_rows(kit.root, (path,)) == expected


def test_duplicate_model_shape_ignores_classvar_and_neutral_scope(kit: TmpRoot[None]) -> None:
    model_a = kit.write(
        "src/domain/models_a.py",
        (
            "from dataclasses import dataclass\n"
            "from typing import ClassVar\n\n"
            "@dataclass(frozen=True, slots=True)\n"
            "class A:\n"
            "    cache: ClassVar[list[str]]\n"
            "    value: UserId\n"
        ),
    )
    model_b = kit.write(
        "src/domain/models_b.py", "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass B:\n    value: UserId\n"
    )
    neutral = kit.write(
        "src/neutral/models_c.py", "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass C:\n    value: UserId\n"
    )
    assert diagnostic_rows(kit.root, (model_a, model_b, neutral)) == (
        ("PYS0006", "src/domain/models_b.py", 4, 1, "DuplicateModelShape", "TypeDiscipline"),
    )


def test_invalid_cli_invocation_returns_two(tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    assert main(("check", "--root", (tmp_path / "missing").as_posix(), "--format", "json")) == 2
    assert "does not exist" in capsys.readouterr().err
    assert main(("check", "--root", tmp_path.as_posix(), "missing.py")) == 2
    assert "path does not exist" in capsys.readouterr().err


def test_parse_failure_emits_pys0000(kit: TmpRoot[None]) -> None:
    path = kit.write("src/domain/broken.py", "def broken(:\n")
    assert tuple(row[0:4] for row in diagnostic_rows(kit.root, (path,))) == (("PYS0000", "src/domain/broken.py", 1, 1),)


def test_excluded_trees_are_pruned_before_parse(kit: TmpRoot[None]) -> None:
    kit.write(".venv/broken.py", "def broken(:\n")
    fixture = kit.write(
        "tests/ast-grep/pass/flow.py", "def run(value: Input) -> Output:\n    if value:\n        return Output()\n    return Output()\n"
    )
    assert analyze_paths(kit.root, ()) == ()
    assert analyze_paths(kit.root, (fixture,)) == ()


def test_output_formats(kit: TmpRoot[None], capsys: pytest.CaptureFixture[str]) -> None:
    kit.write("src/domain/flow.py", "def run(value: Input) -> Output:\n    if value:\n        return Output()\n")
    assert main(("check", "--root", kit.root.as_posix(), "--format", "text")) == 1
    assert "src/domain/flow.py:2:5: PYS0001 DomainImperativeFlow:" in capsys.readouterr().out
    assert main(("check", "--root", kit.root.as_posix(), "--format", "json")) == 1
    payload = msgspec.json.decode(capsys.readouterr().out, type=list[DiagnosticJson])
    assert payload[0] | {"message": payload[0]["message"]} == {
        "rule_id": "PYS0001",
        "severity": "error",
        "path": "src/domain/flow.py",
        "line": 2,
        "column": 5,
        "title": "DomainImperativeFlow",
        "message": payload[0]["message"],
        "category": "FunctionalDiscipline",
    }
    assert main(("check", "--root", kit.root.as_posix(), "--format", "github")) == 1
    assert "::error file=src/domain/flow.py,line=2,col=5,title=PYS0001 DomainImperativeFlow::" in capsys.readouterr().out


def test_github_output_escapes_properties_and_message(tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    emit(
        (
            Diagnostic(
                RuleId.domain_flow,
                Severity.error,
                tmp_path / "src/domain/a,b:c.py",
                1,
                1,
                "Title,With:Property",
                "body %\r\n",
                RuleCategory.functional,
            ),
        ),
        tmp_path,
        OutputFormat.github,
    )
    assert capsys.readouterr().out == ("::error file=src/domain/a%2Cb%3Ac.py,line=1,col=1,title=PYS0001 Title%2CWith%3AProperty::body %25%0D%0A\n")
