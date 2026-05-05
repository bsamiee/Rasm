"""LibCST semantic analyzer for Rasm Python doctrine."""

from __future__ import annotations

from collections import Counter
from dataclasses import dataclass
from datetime import date, datetime, UTC
from itertools import groupby
from operator import attrgetter, itemgetter
import re
from typing import override, TYPE_CHECKING

import libcst as cst
from libcst.metadata import ParentNodeProvider, PositionProvider

from tools.py_analyzer.rules import diagnostic, RuleId, Scope


if TYPE_CHECKING:
    from collections.abc import Iterator, Sequence
    from pathlib import Path

    from tools.py_analyzer.rules import Diagnostic


# --- [TYPES] ---------------------------------------------------------------------------

type ModelShape = tuple[tuple[str, str], ...]


# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class FunctionFact:
    """Module-level private function needed for cross-file surface checks."""

    name: str
    path: Path
    line: int
    column: int
    exempt: bool


@dataclass(frozen=True, slots=True)
class ModelFact:
    """Model shape fact needed for duplicate domain shape checks."""

    name: str
    path: Path
    line: int
    column: int
    shape: ModelShape


@dataclass(frozen=True, slots=True)
class ModelField:
    """Instance model field with source node for local policy diagnostics."""

    name: str
    annotation: cst.CSTNode
    node: cst.CSTNode


@dataclass(frozen=True, slots=True)
class ModuleFacts:
    """Immutable per-module facts emitted by one LibCST visitor pass."""

    diagnostics: tuple[Diagnostic, ...]
    private_functions: tuple[FunctionFact, ...]
    private_calls: tuple[str, ...]
    models: tuple[ModelFact, ...]


# --- [CONSTANTS] -----------------------------------------------------------------------

EXCLUDED_DIRS = frozenset({
    ".artifacts",
    ".cache",
    ".git",
    ".nx",
    ".venv",
    "coverage",
    "node_modules",
    "test-results",
    "tmp",
})
BOUNDARY_EXEMPTION = re.compile(
    r"#\s*RASM_BOUNDARY_EXEMPTION:\s+"
    r"rule=(?P<rule>PYS\d{4})\s+"
    r"reason=(?P<reason>[a-z][a-z0-9-]*)\s+"
    r"ticket=(?P<ticket>[A-Z][A-Z0-9]+-\d+)\s+"
    r"expires=(?P<expires>\d{4}-\d{2}-\d{2})\s+"
    r"rationale=(?P<rationale>\S.+)"
)
BOUNDARY_REASONS = frozenset({"protocol-required", "cleanup-finally", "cancellation-guard", "adapter-normalization"})
FALLIBLE_PREFIXES = (
    "try_",
    "parse_",
    "load_",
    "fetch_",
    "read_",
    "decode_",
    "validate_",
    "ensure_",
    "resolve_",
    "find_",
)
MODEL_DECORATORS = frozenset({"dataclass", "pydantic.dataclasses.dataclass"})
PYDANTIC_MODEL_BASES = frozenset({
    "BaseModel",
    "pydantic.BaseModel",
    "RootModel",
    "pydantic.RootModel",
    "BaseSettings",
    "pydantic_settings.BaseSettings",
})
MSGSPEC_MODEL_BASES = frozenset({"Struct", "msgspec.Struct"})
MODEL_BASES = PYDANTIC_MODEL_BASES | MSGSPEC_MODEL_BASES
PYDANTIC_CONFIG_CALLS = frozenset({
    "ConfigDict",
    "pydantic.ConfigDict",
    "SettingsConfigDict",
    "pydantic_settings.SettingsConfigDict",
})
CLASSVAR_NAMES = frozenset({"ClassVar"})
EFFECT_BUILDER_DECORATORS = frozenset({"effect.result", "effect.async_result", "result", "async_result"})
ERASED_NAMES = frozenset({"Any", "object"})
MUTABLE_FIELD_NAMES = frozenset({
    "dict",
    "list",
    "set",
    "Dict",
    "List",
    "Set",
    "MutableMapping",
    "MutableSequence",
    "MutableSet",
})
PRIMITIVE_NAMES = frozenset({"bool", "float", "int", "str", "tuple"})
RAIL_NAMES = frozenset({"Option", "Result"})
RAIL_ESCAPE_METHODS = frozenset({"unwrap", "value_or"})
PY_ANALYZER_ROOT = ("tools", "py_analyzer")
TOOLING_ROOTS = ((".claude", "hooks"), (".claude", "skills"), PY_ANALYZER_ROOT)
DOMAIN_SCOPES = frozenset({Scope.domain, Scope.application})
PUBLIC_SIGNATURE_BANNED_NAMES = ERASED_NAMES | MUTABLE_FIELD_NAMES | PRIMITIVE_NAMES


# --- [SERVICES] ------------------------------------------------------------------------


class ModuleAnalyzer(cst.CSTVisitor):
    """Collect local diagnostics and facts from one parsed Python module."""

    METADATA_DEPENDENCIES = (PositionProvider, ParentNodeProvider)

    def __init__(self, path: Path, root: Path, module: cst.Module, source_lines: Sequence[str]) -> None:
        self.path = path.resolve()
        self.root = root.resolve()
        self.module = module
        self.source_lines = source_lines
        self.scope = classify_scope(self.path, self.root)
        self.diagnostics: list[Diagnostic] = []
        self.private_functions: list[FunctionFact] = []
        self.private_calls: list[str] = []
        self.models: list[ModelFact] = []
        self.effect_builder_depth = 0

    @override
    def on_visit(self, node: cst.CSTNode) -> bool:
        match node:
            case cst.If() | cst.For() | cst.While() | cst.Try() | cst.Raise():
                self._check_flow(node, _flow_construct(node))
            case cst.FunctionDef():
                self._check_function(node)
            case cst.Call():
                self._check_call(node)
            case cst.ClassDef():
                self._check_model(node)
            case _:
                return True
        return True

    @override
    def on_leave(self, original_node: cst.CSTNode) -> None:
        match original_node:
            case cst.FunctionDef() if _function_is_effect_builder(original_node):
                self.effect_builder_depth -= 1
            case _:
                return

    def facts(self) -> ModuleFacts:
        """Project mutable visitor state into immutable analyzer facts."""
        return ModuleFacts(
            tuple(self.diagnostics), tuple(self.private_functions), tuple(self.private_calls), tuple(self.models)
        )

    def _in_domain_or_application(self) -> bool:
        return self.scope in DOMAIN_SCOPES

    def _check_function(self, node: cst.FunctionDef) -> None:
        if _function_is_effect_builder(node):
            self.effect_builder_depth += 1
        match (self._in_domain_or_application(), _is_module_level(self, node), node.name.value.startswith("_")):
            case (True, True, True):
                self.private_functions.append(_function_fact(self, node))
            case (True, _, False):
                self._check_public_signature(node)
                self._check_fallible_return(node)
            case _:
                return

    def _check_call(self, node: cst.Call) -> None:
        match node.func:
            case cst.Name(value=str(name)) if name.startswith("_"):
                self.private_calls.append(name)
            case _:
                pass
        match node.func:
            case cst.Attribute(attr=cst.Name(value=str(method))) if (
                self._in_domain_or_application() and method in RAIL_ESCAPE_METHODS
            ):
                self._report(RuleId.rail_escape, node, f"{method} collapses rails in {self.scope.value} scope.")
            case cst.Attribute(attr=cst.Name(value="or_else_with")) if (
                self._in_domain_or_application() and self.effect_builder_depth > 0
            ):
                self._report(
                    RuleId.recovery_inside_effect,
                    node,
                    "or_else_with recovery belongs at the composition boundary, outside the effect builder.",
                )
            case _:
                return

    def _check_model(self, node: cst.ClassDef) -> None:
        match (self._in_domain_or_application(), _is_model_class(node), _model_shape(self, node)):
            case (True, True, tuple(shape)):
                self._check_model_policy(node)
                if shape:
                    position = self.get_metadata(PositionProvider, node)
                    self.models.append(
                        ModelFact(node.name.value, self.path, position.start.line, position.start.column + 1, shape)
                    )
            case _:
                return

    def _check_flow(self, node: cst.CSTNode, construct: str) -> None:
        match self.scope:
            case Scope.domain | Scope.application:
                self._report(RuleId.domain_flow, node, f"{construct} is forbidden in {self.scope.value} scope.")
            case Scope.boundary:
                self._check_boundary_exemption(node, construct)
            case _:
                return

    def _check_model_policy(self, node: cst.ClassDef) -> None:
        match _model_immutability_violation(node):
            case str(detail):
                self._report(RuleId.model_immutability, node, detail)
            case None:
                pass
        for field in _model_fields(node):
            if not _annotation_is_classvar(field.annotation) and _annotation_contains_mutable(field.annotation):
                self._report(
                    RuleId.mutable_model_field,
                    field.node,
                    (
                        f"{node.name.value}.{field.name} uses mutable annotation "
                        f"{self.module.code_for_node(field.annotation)}."
                    ),
                )

    def _check_boundary_exemption(self, node: cst.CSTNode, construct: str) -> None:
        position = self.get_metadata(PositionProvider, node)
        match _valid_boundary_exemption(self.source_lines, position.start.line):
            case True:
                return
            case False:
                self._report(
                    RuleId.boundary_exemption,
                    node,
                    f"{construct} requires rule/reason/ticket/expiry/rationale metadata.",
                )

    def _check_public_signature(self, node: cst.FunctionDef) -> None:
        primitive = next(
            (
                self.module.code_for_node(annotation)
                for annotation in _function_annotations(node)
                if _annotation_exposes_primitive(annotation)
            ),
            None,
        )
        match primitive:
            case str(value):
                self._report(
                    RuleId.primitive_signature,
                    node,
                    f"{node.name.value} exposes primitive, erased, or mutable annotation {value}.",
                )
            case None:
                return

    def _check_fallible_return(self, node: cst.FunctionDef) -> None:
        returns = node.returns.annotation if node.returns else None
        rail = bool(returns and _annotation_head_in(returns, RAIL_NAMES))
        optional = bool(returns and _annotation_is_optional(returns))
        match (node.name.value.startswith(FALLIBLE_PREFIXES) or optional, rail):
            case (True, False):
                self._report(RuleId.fallible_return, node, f"{node.name.value} must return Result[T, E] or Option[T].")
            case _:
                return

    def _report(self, rule_id: RuleId, node: cst.CSTNode, detail: str) -> None:
        position = self.get_metadata(PositionProvider, node)
        self.diagnostics.append(diagnostic(rule_id, self.path, position.start.line, position.start.column + 1, detail))


# --- [OPERATIONS] ----------------------------------------------------------------------


def analyze_paths(root: Path, paths: Sequence[Path]) -> tuple[Diagnostic, ...]:
    """Analyze Python files under root and return stable diagnostics."""
    resolved_root = root.resolve()
    module_facts = tuple(_analyze_file(resolved_root, path) for path in _discover_python_files(resolved_root, paths))
    diagnostics = tuple(diagnostic for facts in module_facts for diagnostic in facts.diagnostics)
    return tuple(
        sorted(
            diagnostics
            + _single_use_private_function_diagnostics(module_facts)
            + _duplicate_model_shape_diagnostics(module_facts),
            key=_diagnostic_key,
        )
    )


def classify_scope(path: Path, root: Path) -> Scope:
    """Classify a path into the Rasm semantic policy scope."""
    parts = _relative_parts(path, root)
    part_set = frozenset(parts)
    tool = any(parts[: len(prefix)] == prefix for prefix in TOOLING_ROOTS) or (
        len(parts) > 3 and parts[0] == ".claude" and parts[1] == "skills" and "scripts" in part_set
    )
    match (
        tool,
        bool({"adapters", "boundary", "boundaries"} & part_set),
        "domain" in part_set,
        "application" in part_set,
    ):
        case (True, _, _, _):
            return Scope.tooling
        case (_, True, _, _):
            return Scope.boundary
        case (_, _, True, _):
            return Scope.domain
        case (_, _, _, True):
            return Scope.application
        case _:
            return Scope.neutral


def _analyze_file(root: Path, path: Path) -> ModuleFacts:
    resolved = path.resolve()
    try:
        source = resolved.read_text(encoding="utf-8")
        module = cst.parse_module(source)
    except cst.ParserSyntaxError as error:
        return ModuleFacts(
            (diagnostic(RuleId.parse, resolved, error.raw_line, error.raw_column + 1, error.message),), (), (), ()
        )
    except OSError as error:
        return ModuleFacts((diagnostic(RuleId.parse, resolved, 1, 1, str(error)),), (), (), ())
    analyzer = ModuleAnalyzer(resolved, root, module, source.splitlines())
    cst.MetadataWrapper(module).visit(analyzer)
    return analyzer.facts()


def _discover_python_files(root: Path, paths: Sequence[Path]) -> tuple[Path, ...]:
    anchors = paths or (root,)
    return tuple(
        sorted({
            path.resolve()
            for anchor in anchors
            for path in _expand_anchor(root, anchor)
            if path.suffix == ".py" and not _excluded(path, root)
        })
    )


def _expand_anchor(root: Path, anchor: Path) -> tuple[Path, ...]:
    resolved = (root / anchor).resolve() if not anchor.is_absolute() else anchor.resolve()
    match resolved:
        case path if path.is_file():
            return (path,)
        case path if path.is_dir():
            return tuple(_walk_python_files(path))
        case _:
            return ()


def _walk_python_files(root: Path) -> Iterator[Path]:
    for directory, names, filenames in root.walk(top_down=True):
        names[:] = sorted(name for name in names if name not in EXCLUDED_DIRS)
        yield from (directory / filename for filename in filenames if filename.endswith(".py"))


def _excluded(path: Path, root: Path) -> bool:
    parts = _relative_parts(path, root)
    return bool(EXCLUDED_DIRS.intersection(parts) or parts[:2] == ("tests", "ast-grep"))


def _relative_parts(path: Path, root: Path) -> tuple[str, ...]:
    absolute = path.resolve()
    return absolute.relative_to(root).parts if absolute.is_relative_to(root) else absolute.parts


def _valid_boundary_exemption(source_lines: Sequence[str], line: int) -> bool:
    window = source_lines[max(line - 4, 0) : line]
    return any(
        match
        and match.group("rule") == RuleId.domain_flow.value
        and match.group("reason") in BOUNDARY_REASONS
        and _future_date(match.group("expires"))
        and bool(match.group("rationale").strip())
        for match in (BOUNDARY_EXEMPTION.search(text) for text in window)
    )


def _future_date(value: str) -> bool:
    try:
        return date.fromisoformat(value) > datetime.now(UTC).date()
    except ValueError:
        return False


def _is_module_level(visitor: ModuleAnalyzer, node: cst.CSTNode) -> bool:
    return isinstance(visitor.get_metadata(ParentNodeProvider, node), cst.Module)


def _function_fact(visitor: ModuleAnalyzer, node: cst.FunctionDef) -> FunctionFact:
    position = visitor.get_metadata(PositionProvider, node)
    decorators = frozenset(_decorator_name(decorator.decorator) for decorator in node.decorators)
    return FunctionFact(
        node.name.value,
        visitor.path,
        position.start.line,
        position.start.column + 1,
        bool(decorators & {"overload", "singledispatch", "singledispatchmethod"} or node.name.value.startswith("__")),
    )


def _function_annotations(node: cst.FunctionDef) -> tuple[cst.CSTNode, ...]:
    params = (*node.params.posonly_params, *node.params.params, *node.params.kwonly_params)
    star_params = tuple(
        param for param in (node.params.star_arg, node.params.star_kwarg) if isinstance(param, cst.Param)
    )
    param_annotations = tuple(param.annotation.annotation for param in (*params, *star_params) if param.annotation)
    return_annotation = (node.returns.annotation,) if node.returns else ()
    return param_annotations + return_annotation


def _decorator_name(node: cst.CSTNode) -> str:
    match node:
        case cst.Name(value=str(value)):
            return value
        case cst.Attribute():
            return _dotted_name(node)
        case cst.Call(func=func):
            return _decorator_name(func)
        case _:
            return ""


def _function_is_effect_builder(node: cst.FunctionDef) -> bool:
    return bool(
        EFFECT_BUILDER_DECORATORS.intersection(
            frozenset(_decorator_name(decorator.decorator) for decorator in node.decorators)
        )
    )


def _dotted_name(node: cst.CSTNode) -> str:
    match node:
        case cst.Name(value=str(value)):
            return value
        case cst.Attribute(value=value, attr=cst.Name(value=str(attr))):
            prefix = _dotted_name(value)
            return f"{prefix}.{attr}" if prefix else attr
        case _:
            return ""


def _annotation_head_name(node: cst.CSTNode) -> str:
    name = _dotted_name(node)
    return name.rsplit(".", maxsplit=1)[-1] if name else ""


def _annotation_head_in(node: cst.CSTNode, names: frozenset[str]) -> bool:
    match node:
        case cst.Subscript(value=value):
            return _annotation_head_name(value) in names
        case cst.Name() | cst.Attribute():
            return _annotation_head_name(node) in names
        case _:
            return False


def _annotation_exposes_primitive(node: cst.CSTNode) -> bool:
    match node:
        case cst.Name() | cst.Attribute():
            return _annotation_head_name(node) in PUBLIC_SIGNATURE_BANNED_NAMES
        case cst.Subscript(value=value, slice=slices):
            return _annotation_head_name(value) in PUBLIC_SIGNATURE_BANNED_NAMES or any(
                _annotation_exposes_primitive(element.slice.value)
                for element in slices
                if isinstance(element.slice, cst.Index)
            )
        case cst.BinaryOperation(left=left, operator=cst.BitOr(), right=right):
            return _annotation_exposes_primitive(left) or _annotation_exposes_primitive(right)
        case cst.Tuple(elements=elements):
            return any(_annotation_exposes_primitive(element.value) for element in elements)
        case _:
            return False


def _annotation_is_optional(node: cst.CSTNode) -> bool:
    match node:
        case cst.Name(value="None"):
            return True
        case cst.Subscript(value=value):
            return _annotation_head_name(value) == "Optional"
        case cst.BinaryOperation(left=left, operator=cst.BitOr(), right=right):
            return _annotation_is_optional(left) or _annotation_is_optional(right)
        case _:
            return False


def _annotation_is_classvar(node: cst.CSTNode) -> bool:
    match node:
        case cst.Subscript(value=value):
            return _annotation_head_name(value) in CLASSVAR_NAMES
        case cst.Name() | cst.Attribute():
            return _annotation_head_name(node) in CLASSVAR_NAMES
        case _:
            return False


def _annotation_contains_mutable(node: cst.CSTNode) -> bool:
    match node:
        case cst.Name() | cst.Attribute():
            return _annotation_head_name(node) in MUTABLE_FIELD_NAMES
        case cst.Subscript(value=value, slice=slices):
            return _annotation_head_name(value) in MUTABLE_FIELD_NAMES or any(
                _annotation_contains_mutable(element.slice.value)
                for element in slices
                if isinstance(element.slice, cst.Index)
            )
        case cst.BinaryOperation(left=left, operator=cst.BitOr(), right=right):
            return _annotation_contains_mutable(left) or _annotation_contains_mutable(right)
        case cst.Tuple(elements=elements):
            return any(_annotation_contains_mutable(element.value) for element in elements)
        case _:
            return False


def _is_model_class(node: cst.ClassDef) -> bool:
    decorators = frozenset(_decorator_name(decorator.decorator) for decorator in node.decorators)
    bases = frozenset(_dotted_name(arg.value) for arg in node.bases)
    return bool(decorators & MODEL_DECORATORS or bases & MODEL_BASES)


def _model_shape(module: ModuleAnalyzer, node: cst.ClassDef) -> ModelShape:
    return tuple(
        sorted(
            (
                (field.name, _annotation_key(field.annotation, module.module))
                for field in _model_fields(node)
                if not _annotation_is_classvar(field.annotation)
            ),
            key=itemgetter(0),
        )
    )


def _model_fields(node: cst.ClassDef) -> tuple[ModelField, ...]:
    return tuple(field for statement in node.body.body if (field := _model_field(statement)))


def _model_field(statement: cst.CSTNode) -> ModelField | None:
    match statement:
        case cst.SimpleStatementLine(
            body=(cst.AnnAssign(target=cst.Name(value=str(name)), annotation=annotation) as assignment,)
        ):
            return ModelField(name, annotation.annotation, assignment)
        case _:
            return None


def _model_immutability_violation(node: cst.ClassDef) -> str | None:
    dataclass = _is_dataclass_model(node)
    pydantic = _model_has_base(node, PYDANTIC_MODEL_BASES)
    msgspec = _model_has_base(node, MSGSPEC_MODEL_BASES)
    match (
        dataclass and not _dataclass_is_frozen_slots(node),
        pydantic and not _pydantic_is_frozen(node),
        msgspec and not _msgspec_is_frozen(node),
    ):
        case (True, _, _):
            return f"{node.name.value} dataclass models require @dataclass(frozen=True, slots=True)."
        case (_, True, _):
            return f"{node.name.value} Pydantic models require frozen=True config."
        case (_, _, True):
            return f"{node.name.value} msgspec.Struct models require frozen=True."
        case _:
            return None


def _is_dataclass_model(node: cst.ClassDef) -> bool:
    return bool(
        MODEL_DECORATORS.intersection(frozenset(_decorator_name(decorator.decorator) for decorator in node.decorators))
    )


def _model_has_base(node: cst.ClassDef, bases: frozenset[str]) -> bool:
    return bool(bases.intersection(frozenset(_dotted_name(arg.value) for arg in node.bases)))


def _dataclass_is_frozen_slots(node: cst.ClassDef) -> bool:
    return any(
        _decorator_name(decorator.decorator) in MODEL_DECORATORS
        and isinstance(decorator.decorator, cst.Call)
        and _call_has_true_keyword(decorator.decorator.args, "frozen")
        and _call_has_true_keyword(decorator.decorator.args, "slots")
        for decorator in node.decorators
    )


def _pydantic_is_frozen(node: cst.ClassDef) -> bool:
    return _class_has_true_keyword(node, "frozen") or _pydantic_configdict_is_frozen(node)


def _msgspec_is_frozen(node: cst.ClassDef) -> bool:
    return _class_has_true_keyword(node, "frozen")


def _class_has_true_keyword(node: cst.ClassDef, name: str) -> bool:
    return _call_has_true_keyword(node.keywords, name)


def _pydantic_configdict_is_frozen(node: cst.ClassDef) -> bool:
    return any(
        _call_has_true_keyword(call.args, "frozen")
        for statement in node.body.body
        for call in (_model_configdict_call(statement),)
        if call is not None
    )


def _model_configdict_call(statement: cst.CSTNode) -> cst.Call | None:
    match statement:
        case cst.SimpleStatementLine(
            body=(
                cst.Assign(
                    targets=(cst.AssignTarget(target=cst.Name(value="model_config")),),
                    value=cst.Call(func=func) as call,
                ),
            )
        ) if _dotted_name(func) in PYDANTIC_CONFIG_CALLS:
            return call
        case _:
            return None


def _call_has_true_keyword(args: Sequence[cst.Arg], name: str) -> bool:
    return any(
        isinstance(arg.keyword, cst.Name) and arg.keyword.value == name and _is_true_literal(arg.value) for arg in args
    )


def _is_true_literal(node: cst.CSTNode) -> bool:
    match node:
        case cst.Name(value="True"):
            return True
        case _:
            return False


def _annotation_key(node: cst.CSTNode, module: cst.Module) -> str:
    match node:
        case cst.Name() | cst.Attribute():
            return _annotation_head_name(node)
        case cst.Subscript(value=value, slice=slices):
            values = ",".join(
                _annotation_key(element.slice.value, module)
                for element in slices
                if isinstance(element.slice, cst.Index)
            )
            return f"{_annotation_key(value, module)}[{values}]"
        case cst.BinaryOperation(left=left, operator=cst.BitOr(), right=right):
            return "|".join(sorted((_annotation_key(left, module), _annotation_key(right, module))))
        case _:
            return module.code_for_node(node).replace(" ", "")


def _flow_construct(node: cst.CSTNode) -> str:
    match node:
        case cst.If():
            return "if"
        case cst.For() | cst.While():
            return "loop"
        case cst.Try():
            return "try"
        case cst.Raise():
            return "raise"
        case _:
            return "flow"


def _single_use_private_function_diagnostics(module_facts: Sequence[ModuleFacts]) -> tuple[Diagnostic, ...]:
    return tuple(
        diagnostic(
            RuleId.single_use_private,
            function.path,
            function.line,
            function.column,
            f"{function.name} has exactly one same-module call.",
        )
        for facts in module_facts
        for calls in (Counter(facts.private_calls),)
        for function in facts.private_functions
        if not function.exempt and calls[function.name] == 1
    )


def _duplicate_model_shape_diagnostics(module_facts: Sequence[ModuleFacts]) -> tuple[Diagnostic, ...]:
    models = tuple(
        sorted(
            (model for facts in module_facts for model in facts.models),
            key=lambda model: (model.shape, model.path.as_posix(), model.line, model.column),
        )
    )
    return tuple(
        diagnostic(
            RuleId.duplicate_model,
            model.path,
            model.line,
            model.column,
            f"{model.name} duplicates {group[0].name} in {group[0].path.name}.",
        )
        for _, grouped in groupby(models, key=attrgetter("shape"))
        for group in (tuple(grouped),)
        if len(group) > 1
        for model in group[1:]
    )


def _diagnostic_key(value: Diagnostic) -> tuple[str, int, int, str]:
    return (value.path.as_posix(), value.line, value.column, value.rule_id.value)
