"""Rule catalog and diagnostic payload model for the Python analyzer."""

from enum import StrEnum
from pathlib import Path
from types import MappingProxyType
from typing import Final

import msgspec


# --- [TYPES] ----------------------------------------------------------------------------

type JsonValue = bool | int | str | list[JsonValue] | dict[str, JsonValue] | None


class RuleId(StrEnum):
    """Rule identifiers used in rendered diagnostics."""

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
    """Output formats accepted by the command boundary."""

    text = "text"
    json = "json"
    github = "github"


class Severity(StrEnum):
    """Diagnostic severity vocabulary."""

    error = "error"


class RuleCategory(StrEnum):
    """Diagnostic category vocabulary."""

    functional = "FunctionalDiscipline"
    governance = "Governance"
    infrastructure = "Infrastructure"
    surface = "SurfaceArea"
    type_discipline = "TypeDiscipline"


class Scope(StrEnum):
    """Semantic scope assigned from path ownership markers."""

    domain = "domain"
    application = "application"
    boundary = "boundary"
    tooling = "tooling"
    neutral = "neutral"


# --- [MODELS] ---------------------------------------------------------------------------


class Rule(msgspec.Struct, frozen=True, gc=False):
    """Rule metadata shared by every renderer."""

    title: str
    category: RuleCategory
    message: str


class Diagnostic(msgspec.Struct, frozen=True, gc=False):
    """Diagnostic payload consumed by every renderer."""

    rule_id: RuleId
    severity: Severity
    path: Path
    line: int
    column: int
    title: str
    message: str
    category: RuleCategory

    def relative_path(self, root: Path) -> str:
        """Render diagnostics under root as relative paths.

        Returns:
            Relative path under root, otherwise an absolute path.
        """
        resolved = self.path.resolve()
        return resolved.relative_to(root).as_posix() if resolved.is_relative_to(root) else resolved.as_posix()

    def as_json(self, root: Path) -> dict[str, JsonValue]:
        """Convert diagnostic fields to JSON-compatible values.

        Returns:
            JSON-compatible diagnostic payload.
        """
        return {
            "rule_id": self.rule_id.value,
            "severity": self.severity.value,
            "path": self.relative_path(root),
            "line": self.line,
            "column": self.column,
            "title": self.title,
            "message": self.message,
            "category": self.category.value,
        }


# --- [TABLES] ---------------------------------------------------------------------------

RULES: Final[MappingProxyType[RuleId, Rule]] = MappingProxyType({
    RuleId.parse: Rule("AnalyzerInput", RuleCategory.infrastructure, "Analyzer could not parse or read this Python file."),
    RuleId.domain_flow: Rule(
        "DomainImperativeFlow",
        RuleCategory.functional,
        "Domain/application flow uses imperative control; use match/case, folds, dispatch tables, or Result rails.",
    ),
    RuleId.boundary_exemption: Rule(
        "BoundaryExemptionGovernance", RuleCategory.governance, "Boundary imperative flow requires exact RASM_BOUNDARY_EXEMPTION metadata."
    ),
    RuleId.primitive_signature: Rule(
        "PrimitivePublicSignature",
        RuleCategory.type_discipline,
        ("Public domain/application signatures must use typed atoms or models instead of primitive, erased, or mutable annotation leakage."),
    ),
    RuleId.fallible_return: Rule(
        "FallibleReturnRail", RuleCategory.functional, "Fallible domain/application functions must return Result[T, E] or Option[T]."
    ),
    RuleId.single_use_private: Rule(
        "SingleUsePrivateFunction", RuleCategory.surface, "Private module-level function has one call site; inline it into the owning public surface."
    ),
    RuleId.duplicate_model: Rule(
        "DuplicateModelShape", RuleCategory.type_discipline, "Domain/application models cannot duplicate the same field shape across modules."
    ),
    RuleId.rail_escape: Rule(
        "RailEscape", RuleCategory.functional, "Domain/application code must not collapse Result/Option rails before a boundary."
    ),
    RuleId.model_immutability: Rule(
        "ModelImmutability", RuleCategory.type_discipline, "Domain/application models must declare frozen runtime shape policy."
    ),
    RuleId.mutable_model_field: Rule(
        "MutableModelField", RuleCategory.type_discipline, "Domain/application model fields must use immutable container annotations."
    ),
    RuleId.recovery_inside_effect: Rule(
        "RecoveryInsideEffectBuilder", RuleCategory.functional, "Effect result builders must not perform recovery inside the builder body."
    ),
})

# --- [OPERATIONS] -----------------------------------------------------------------------


def diagnostic(rule_id: RuleId, path: Path, line: int, column: int, detail: str) -> Diagnostic:
    """Create an error diagnostic from catalog metadata and source location.

    Returns:
        Diagnostic populated from the rule catalog.
    """
    rule = RULES[rule_id]
    return Diagnostic(rule_id, Severity.error, path.resolve(), line, column, rule.title, f"{rule.message} {detail}", rule.category)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("Diagnostic", "JsonValue", "OutputFormat", "Rule", "RULES", "RuleCategory", "RuleId", "Scope", "Severity", "diagnostic")
