from __future__ import annotations

from pathlib import Path
import tomllib

import msgspec
import pytest

from tools.py_analyzer.analyzer import analyze_paths, PY_ANALYZER_ROOT
from tools.py_analyzer.cli import emit, main
from tools.py_analyzer.rules import Diagnostic, OutputFormat, RuleId


type DiagnosticRow = tuple[str, str, int, int, str, str]
type DiagnosticJson = dict[str, str | int]

PY_ANALYZER_SAMPLE = "/".join((*PY_ANALYZER_ROOT, "sample.py"))


def write_module(root: Path, relative: str, source: str) -> Path:
    path = root / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(source, encoding="utf-8")
    return path


def diagnostic_rows(root: Path, paths: tuple[Path, ...]) -> tuple[DiagnosticRow, ...]:
    return tuple(
        (
            diagnostic.rule_id.value,
            diagnostic.path.relative_to(root).as_posix(),
            diagnostic.line,
            diagnostic.column,
            diagnostic.title,
            diagnostic.category,
        )
        for diagnostic in analyze_paths(root, paths)
    )


@pytest.mark.parametrize(
    "relative, source, expected",
    [
        (
            "src/domain/flow.py",
            "def run(value: Input) -> Output:\n    if value:\n        return Output()\n    return Output()\n",
            ("PYS0001", "src/domain/flow.py", 2, 5, "DomainImperativeFlow", "FunctionalDiscipline"),
        ),
        (
            "src/adapters/http.py",
            "def run(value: object) -> bool:\n    if value:\n        return True\n    return False\n",
            ("PYS0002", "src/adapters/http.py", 2, 5, "BoundaryExemptionGovernance", "Governance"),
        ),
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
def test_rule_cases_emit_exact_diagnostic(tmp_path: Path, relative: str, source: str, expected: DiagnosticRow) -> None:
    write_module(
        tmp_path,
        "src/domain/models_a.py",
        "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass A:\n    value: UserId\n",
    )
    path = write_module(tmp_path, relative, source)
    rows = tuple(
        row for row in diagnostic_rows(tmp_path, (path, tmp_path / "src/domain/models_a.py")) if row[1] == relative
    )
    assert rows == (expected,)


@pytest.mark.parametrize(
    "header",
    [
        (
            "# RASM_BOUNDARY_EXEMPTION: rule=PYS9999 reason=protocol-required ticket=RASM-123 "
            "expires=2999-01-01 rationale=external protocol gate"
        ),
        (
            "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=bad-reason ticket=RASM-123 "
            "expires=2999-01-01 rationale=external protocol gate"
        ),
        (
            "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=protocol-required ticket=bad-ticket "
            "expires=2999-01-01 rationale=external protocol gate"
        ),
        (
            "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=protocol-required ticket=RASM-123 "
            "expires=2000-01-01 rationale=external protocol gate"
        ),
        (
            "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=protocol-required ticket=RASM-123 "
            "expires=2026-99-99 rationale=external protocol gate"
        ),
        (
            "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=protocol-required ticket=RASM-123 "
            "expires=2999-01-01 rationale="
        ),
    ],
)
def test_invalid_boundary_exemptions_emit_governance(tmp_path: Path, header: str) -> None:
    path = write_module(
        tmp_path,
        "src/adapters/http.py",
        f"{header}\ndef run(value: object) -> bool:\n    if value:\n        return True\n    return False\n",
    )
    assert diagnostic_rows(tmp_path, (path,)) == (
        ("PYS0002", "src/adapters/http.py", 3, 5, "BoundaryExemptionGovernance", "Governance"),
    )


def test_boundary_exemption_window_is_enforced(tmp_path: Path) -> None:
    path = write_module(
        tmp_path,
        "src/adapters/http.py",
        "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=protocol-required "
        "ticket=RASM-123 expires=2999-01-01 rationale=external protocol gate\n\n\n\n"
        "def run(value: object) -> bool:\n"
        "    if value:\n"
        "        return True\n"
        "    return False\n",
    )
    assert diagnostic_rows(tmp_path, (path,)) == (
        ("PYS0002", "src/adapters/http.py", 6, 5, "BoundaryExemptionGovernance", "Governance"),
    )


def test_valid_boundary_exemption_suppresses_governance(tmp_path: Path) -> None:
    path = write_module(
        tmp_path,
        "src/adapters/http.py",
        "# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=protocol-required "
        "ticket=RASM-123 expires=2999-01-01 rationale=external protocol gate\n"
        "def run(value: object) -> bool:\n"
        "    if value:\n"
        "        return True\n"
        "    return False\n",
    )
    assert diagnostic_rows(tmp_path, (path,)) == ()


@pytest.mark.parametrize(
    "relative, expected_rule",
    [
        ("src/domain/flow.py", "PYS0001"),
        ("src/application/flow.py", "PYS0001"),
        ("src/domain/adapters/http.py", "PYS0002"),
        ("src/neutral/flow.py", ""),
        (PY_ANALYZER_SAMPLE, ""),
        (".claude/skills/example/scripts/tool.py", ""),
    ],
)
def test_scope_matrix(tmp_path: Path, relative: str, expected_rule: str) -> None:
    path = write_module(
        tmp_path,
        relative,
        "def run(value: Input) -> Output:\n    if value:\n        return Output()\n    return Output()\n",
    )
    rows = diagnostic_rows(tmp_path, (path,))
    assert tuple(row[0] for row in rows) == ((expected_rule,) if expected_rule else ())


@pytest.mark.parametrize(
    "source",
    [
        "def maybe_user(value: UserInput) -> User | None:\n    return None\n",
        "def maybe_user(value: UserInput) -> None | User:\n    return None\n",
        "def parse_user(value: UserInput) -> User:\n    return User()\n",
    ],
)
def test_fallible_return_rail_diagnostics(tmp_path: Path, source: str) -> None:
    path = write_module(tmp_path, "src/domain/fallible.py", source)
    assert tuple(row[0] for row in diagnostic_rows(tmp_path, (path,))) == ("PYS0004",)


@pytest.mark.parametrize(
    "source",
    [
        "def maybe_user(value: UserInput) -> Result[User, DomainError]:\n    return result\n",
        "def maybe_user(value: UserInput) -> Option[User]:\n    return user\n",
    ],
)
def test_result_and_option_are_valid_fallible_rails(tmp_path: Path, source: str) -> None:
    path = write_module(tmp_path, "src/domain/fallible.py", source)
    assert diagnostic_rows(tmp_path, (path,)) == ()


@pytest.mark.parametrize(
    "relative, source, expected",
    [
        (
            "src/domain/rail.py",
            "def run(value: Result[User, DomainError]) -> User:\n    return value.unwrap()\n",
            ("PYS0007", "src/domain/rail.py", 2, 12, "RailEscape", "FunctionalDiscipline"),
        ),
        (
            "src/application/rail.py",
            "def run(value: Option[User]) -> User:\n    return value.value_or(User())\n",
            ("PYS0007", "src/application/rail.py", 2, 12, "RailEscape", "FunctionalDiscipline"),
        ),
    ],
)
def test_rail_escape_emits_in_domain_application(
    tmp_path: Path, relative: str, source: str, expected: DiagnosticRow
) -> None:
    path = write_module(tmp_path, relative, source)
    assert diagnostic_rows(tmp_path, (path,)) == (expected,)


def test_default_value_is_not_blanket_rail_escape(tmp_path: Path) -> None:
    path = write_module(
        tmp_path,
        "src/domain/rail.py",
        "def run(value: Option[User]) -> User:\n    return value.default_value(User())\n",
    )
    assert diagnostic_rows(tmp_path, (path,)) == ()


@pytest.mark.parametrize("relative", ["src/boundary/rail.py", PY_ANALYZER_SAMPLE])
def test_rail_escape_scope_boundaries(tmp_path: Path, relative: str) -> None:
    path = write_module(
        tmp_path, relative, "def run(value: Result[User, DomainError]) -> User:\n    return value.unwrap()\n"
    )
    assert diagnostic_rows(tmp_path, (path,)) == ()


def test_non_rail_method_names_do_not_emit_rail_escape(tmp_path: Path) -> None:
    path = write_module(tmp_path, "src/domain/rail.py", "def run(value: User) -> User:\n    return value.unwraps()\n")
    assert diagnostic_rows(tmp_path, (path,)) == ()


@pytest.mark.parametrize(
    "source",
    [
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
    ],
)
def test_primitive_signature_nested_forms(tmp_path: Path, source: str) -> None:
    path = write_module(tmp_path, "src/domain/signature.py", source)
    assert tuple(row[0] for row in diagnostic_rows(tmp_path, (path,))) == ("PYS0003",)


def test_typed_atoms_and_models_do_not_emit_primitive_signature(tmp_path: Path) -> None:
    path = write_module(tmp_path, "src/domain/signature.py", "def run(value: UserInput) -> User:\n    return User()\n")
    assert diagnostic_rows(tmp_path, (path,)) == ()


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
        (
            (
                "def _normalize(value: UserInput) -> UserInput:\n"
                "    return value\n\n"
                "def run(value: UserInput) -> UserInput:\n"
                "    return value\n"
            ),
            (),
        ),
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
def test_single_use_private_function_cases(tmp_path: Path, source: str, expected: tuple[str, ...]) -> None:
    path = write_module(tmp_path, "src/domain/private.py", source)
    assert tuple(row[0] for row in diagnostic_rows(tmp_path, (path,))) == expected


@pytest.mark.parametrize(
    "source, expected",
    [
        (
            "from dataclasses import dataclass\n\n@dataclass\nclass User:\n    value: UserId\n",
            ("PYS0008", "src/domain/model.py", 4, 1, "ModelImmutability", "TypeDiscipline"),
        ),
        (
            "from dataclasses import dataclass\n\n@dataclass(frozen=True)\nclass User:\n    value: UserId\n",
            ("PYS0008", "src/domain/model.py", 4, 1, "ModelImmutability", "TypeDiscipline"),
        ),
        (
            "class User(BaseModel):\n    value: UserId\n",
            ("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),
        ),
        (
            "class User(msgspec.Struct):\n    value: UserId\n",
            ("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),
        ),
        (
            "class User(RootModel):\n    root: UserId\n",
            ("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),
        ),
        (
            "class Settings(BaseSettings):\n    value: UserId\n",
            ("PYS0008", "src/domain/model.py", 1, 1, "ModelImmutability", "TypeDiscipline"),
        ),
    ],
)
def test_model_immutability_diagnostics(tmp_path: Path, source: str, expected: DiagnosticRow) -> None:
    path = write_module(tmp_path, "src/domain/model.py", source)
    assert diagnostic_rows(tmp_path, (path,)) == (expected,)


@pytest.mark.parametrize(
    "source",
    [
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
    ],
)
def test_model_immutability_valid_forms(tmp_path: Path, source: str) -> None:
    path = write_module(tmp_path, "src/domain/model.py", source)
    assert diagnostic_rows(tmp_path, (path,)) == ()


@pytest.mark.parametrize(
    "annotation",
    [
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
    ],
)
def test_mutable_model_field_diagnostics(tmp_path: Path, annotation: str) -> None:
    path = write_module(
        tmp_path,
        "src/domain/model.py",
        (
            "from dataclasses import dataclass\n\n"
            "@dataclass(frozen=True, slots=True)\n"
            "class User:\n"
            f"    value: {annotation}\n"
        ),
    )
    assert diagnostic_rows(tmp_path, (path,)) == (
        ("PYS0009", "src/domain/model.py", 5, 5, "MutableModelField", "TypeDiscipline"),
    )


@pytest.mark.parametrize(
    "annotation",
    ["tuple[UserId, ...]", "frozenset[UserId]", "Mapping[str, UserId]", "Sequence[UserId]", "Block[UserId]"],
)
def test_immutable_model_field_annotations_pass(tmp_path: Path, annotation: str) -> None:
    path = write_module(
        tmp_path,
        "src/domain/model.py",
        (
            "from dataclasses import dataclass\n\n"
            "@dataclass(frozen=True, slots=True)\n"
            "class User:\n"
            f"    value: {annotation}\n"
        ),
    )
    assert diagnostic_rows(tmp_path, (path,)) == ()


def test_classvar_model_field_is_ignored_for_mutability_and_shape(tmp_path: Path) -> None:
    model_a = write_module(
        tmp_path,
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
    model_b = write_module(
        tmp_path,
        "src/domain/models_b.py",
        "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass B:\n    value: UserId\n",
    )
    assert diagnostic_rows(tmp_path, (model_a, model_b)) == (
        ("PYS0006", "src/domain/models_b.py", 4, 1, "DuplicateModelShape", "TypeDiscipline"),
    )


@pytest.mark.parametrize("decorator", ["effect.result", "effect.async_result", "result", "async_result"])
def test_recovery_inside_effect_builder_emits(tmp_path: Path, decorator: str) -> None:
    path = write_module(
        tmp_path,
        "src/domain/effect.py",
        (
            f"@{decorator}\n"
            "def run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n"
            "    return value.or_else_with(recover)\n"
        ),
    )
    assert diagnostic_rows(tmp_path, (path,)) == (
        ("PYS0010", "src/domain/effect.py", 3, 12, "RecoveryInsideEffectBuilder", "FunctionalDiscipline"),
    )


@pytest.mark.parametrize(
    "relative, source",
    [
        (
            "src/domain/effect.py",
            (
                "def run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n"
                "    return value.or_else_with(recover)\n"
            ),
        ),
        (
            "src/boundary/effect.py",
            (
                "@effect.result\n"
                "def run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n"
                "    return value.or_else_with(recover)\n"
            ),
        ),
        (
            "src/domain/effect.py",
            (
                "@effect.result\n"
                "def run(value: Result[User, DomainError]) -> Result[User, DomainError]:\n"
                "    return value.map(transform)\n"
            ),
        ),
    ],
)
def test_recovery_inside_effect_builder_allowed_cases(tmp_path: Path, relative: str, source: str) -> None:
    path = write_module(tmp_path, relative, source)
    assert diagnostic_rows(tmp_path, (path,)) == ()


def test_duplicate_model_shape_scope_boundaries(tmp_path: Path) -> None:
    domain_a = write_module(
        tmp_path,
        "src/domain/models_a.py",
        "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass A:\n    value: UserId\n",
    )
    domain_b = write_module(
        tmp_path,
        "src/domain/models_b.py",
        "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass B:\n    value: UserId\n",
    )
    neutral = write_module(
        tmp_path,
        "src/neutral/models_c.py",
        "from dataclasses import dataclass\n\n@dataclass(frozen=True, slots=True)\nclass C:\n    value: UserId\n",
    )
    assert diagnostic_rows(tmp_path, (domain_a, domain_b, neutral)) == (
        ("PYS0006", "src/domain/models_b.py", 4, 1, "DuplicateModelShape", "TypeDiscipline"),
    )


def test_invalid_cli_invocation_returns_two(tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    assert main(("check", "--root", (tmp_path / "missing").as_posix(), "--format", "json")) == 2
    assert "does not exist" in capsys.readouterr().err
    assert main(("check", "--root", tmp_path.as_posix(), "missing.py")) == 2
    assert "path does not exist" in capsys.readouterr().err


def test_parse_failure_emits_pys0000(tmp_path: Path) -> None:
    path = write_module(tmp_path, "src/domain/broken.py", "def broken(:\n")
    assert tuple(row[0:4] for row in diagnostic_rows(tmp_path, (path,))) == (("PYS0000", "src/domain/broken.py", 1, 1),)


def test_excluded_directories_are_pruned_before_parse(tmp_path: Path) -> None:
    write_module(tmp_path, ".venv/broken.py", "def broken(:\n")
    assert analyze_paths(tmp_path, ()) == ()


def test_output_formats(tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    write_module(
        tmp_path, "src/domain/flow.py", "def run(value: Input) -> Output:\n    if value:\n        return Output()\n"
    )
    assert main(("check", "--root", tmp_path.as_posix(), "--format", "text")) == 1
    assert "src/domain/flow.py:2:5: PYS0001 DomainImperativeFlow:" in capsys.readouterr().out
    assert main(("check", "--root", tmp_path.as_posix(), "--format", "json")) == 1
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
    assert main(("check", "--root", tmp_path.as_posix(), "--format", "github")) == 1
    assert (
        "::error file=src/domain/flow.py,line=2,col=5,title=PYS0001 DomainImperativeFlow::" in capsys.readouterr().out
    )


def test_github_output_escapes_properties_and_message(tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    emit(
        (
            Diagnostic(
                RuleId.domain_flow,
                "error",
                tmp_path / "src/domain/a,b:c.py",
                1,
                1,
                "Title,With:Property",
                "body %\r\n",
                "FunctionalDiscipline",
            ),
        ),
        tmp_path,
        OutputFormat.github,
    )
    assert capsys.readouterr().out == (
        "::error file=src/domain/a%2Cb%3Ac.py,line=1,col=1,title=PYS0001 Title%2CWith%3AProperty::body %25%0D%0A\n"
    )


def test_typeguard_is_banned_by_ruff_config() -> None:
    config = tomllib.loads(Path("pyproject.toml").read_text(encoding="utf-8"))
    banned_api = config["tool"]["ruff"]["lint"]["flake8-tidy-imports"]["banned-api"]
    assert "typing.TypeGuard" in banned_api
    assert "typing_extensions.TypeGuard" in banned_api
