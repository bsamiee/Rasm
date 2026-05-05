"""Rule catalog and diagnostic wire shape for the Rasm Python analyzer."""

from __future__ import annotations

from dataclasses import dataclass
from enum import StrEnum
from typing import TYPE_CHECKING


if TYPE_CHECKING:
    from collections.abc import Mapping
    from pathlib import Path


# --- [TYPES] ---------------------------------------------------------------------------

type JsonValue = bool | int | str | list[JsonValue] | dict[str, JsonValue] | None


class RuleId(StrEnum):
    """Stable analyzer rule identifiers."""

    parse = "PYS0000"
    domain_flow = "PYS0001"
    boundary_exemption = "PYS0002"
    primitive_signature = "PYS0003"
    fallible_return = "PYS0004"
    single_use_private = "PYS0005"
    duplicate_model = "PYS0006"
    rail_escape = "PYS0007"
    model_immutability = "PYS0008"
    mutable_model_field = "PYS0009"
    recovery_inside_effect = "PYS0010"


class OutputFormat(StrEnum):
    """Supported diagnostic output formats."""

    text = "text"
    json = "json"
    github = "github"


class Scope(StrEnum):
    """Analyzer scope classification."""

    domain = "domain"
    application = "application"
    boundary = "boundary"
    tooling = "tooling"
    neutral = "neutral"


# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Rule:
    """Static rule metadata shared by output formats."""

    title: str
    category: str
    message: str


@dataclass(frozen=True, slots=True)
class Diagnostic:
    """Analyzer diagnostic with stable external wire shape."""

    rule_id: RuleId
    severity: str
    path: Path
    line: int
    column: int
    title: str
    message: str
    category: str

    def relative_path(self, root: Path) -> str:
        """Return a root-relative path when the diagnostic belongs to the project root."""
        resolved = self.path.resolve()
        return resolved.relative_to(root).as_posix() if resolved.is_relative_to(root) else resolved.as_posix()

    def as_json(self, root: Path) -> dict[str, JsonValue]:
        """Project diagnostic into JSON-compatible fields."""
        return {
            "rule_id": self.rule_id.value,
            "severity": self.severity,
            "path": self.relative_path(root),
            "line": self.line,
            "column": self.column,
            "title": self.title,
            "message": self.message,
            "category": self.category,
        }


# --- [CONSTANTS] -----------------------------------------------------------------------

RULES: Mapping[RuleId, Rule] = {
    RuleId.parse: Rule("AnalyzerInput", "Infrastructure", "Analyzer could not parse or read this Python file."),
    RuleId.domain_flow: Rule(
        "DomainImperativeFlow",
        "FunctionalDiscipline",
        "Domain/application flow uses imperative control; use match/case, folds, dispatch tables, or Result rails.",
    ),
    RuleId.boundary_exemption: Rule(
        "BoundaryExemptionGovernance",
        "Governance",
        "Boundary imperative flow requires exact RASM_BOUNDARY_EXEMPTION metadata.",
    ),
    RuleId.primitive_signature: Rule(
        "PrimitivePublicSignature",
        "TypeDiscipline",
        (
            "Public domain/application signatures must use typed atoms or models instead of primitive, "
            "erased, or mutable annotation leakage."
        ),
    ),
    RuleId.fallible_return: Rule(
        "FallibleReturnRail",
        "FunctionalDiscipline",
        "Fallible domain/application functions must return Result[T, E] or Option[T].",
    ),
    RuleId.single_use_private: Rule(
        "SingleUsePrivateFunction",
        "SurfaceArea",
        "Private module-level function has one call site; inline it into the owning public surface.",
    ),
    RuleId.duplicate_model: Rule(
        "DuplicateModelShape",
        "TypeDiscipline",
        "Domain/application models cannot duplicate the same field shape across modules.",
    ),
    RuleId.rail_escape: Rule(
        "RailEscape",
        "FunctionalDiscipline",
        "Domain/application code must not collapse Result/Option rails before a boundary.",
    ),
    RuleId.model_immutability: Rule(
        "ModelImmutability", "TypeDiscipline", "Domain/application models must declare frozen runtime shape policy."
    ),
    RuleId.mutable_model_field: Rule(
        "MutableModelField",
        "TypeDiscipline",
        "Domain/application model fields must use immutable container annotations.",
    ),
    RuleId.recovery_inside_effect: Rule(
        "RecoveryInsideEffectBuilder",
        "FunctionalDiscipline",
        "Effect result builders must not perform recovery inside the builder body.",
    ),
}


# --- [OPERATIONS] ----------------------------------------------------------------------


def diagnostic(rule_id: RuleId, path: Path, line: int, column: int, detail: str) -> Diagnostic:
    """Create a stable error diagnostic from a rule id and location."""
    rule = RULES[rule_id]
    return Diagnostic(
        rule_id, "error", path.resolve(), line, column, rule.title, f"{rule.message} {detail}", rule.category
    )
