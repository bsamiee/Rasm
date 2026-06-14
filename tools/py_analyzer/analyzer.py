"""Tree-sitter semantic analyzer for Rasm Python doctrine."""

from collections import Counter
from dataclasses import dataclass
from datetime import date, datetime, UTC
from functools import cache
from itertools import groupby
from operator import attrgetter, itemgetter
from pathlib import Path
import re
from typing import TYPE_CHECKING

from tree_sitter import Node, Parser
import tree_sitter_python

from tools.assay.rails.code import ts_language  # shared tree-sitter kernel
from tools.py_analyzer.rules import diagnostic, RuleId, Scope


if TYPE_CHECKING:
    from collections.abc import Iterator, Sequence

    from tools.py_analyzer.rules import Diagnostic


# --- [TYPES] ----------------------------------------------------------------------------

type ModelShape = tuple[tuple[str, str], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_PYTHON_GRAMMAR = tree_sitter_python.language
_EXCLUDED_DIRS = frozenset({".artifacts", ".cache", ".git", ".nx", ".venv", "coverage", "node_modules", "test-results", "tmp"})
_EXCLUDED_PREFIXES = (("tests", "tools", "ast-grep"),)
_BOUNDARY_EXEMPTION = re.compile(
    r"#\s*RASM_BOUNDARY_EXEMPTION:\s+"
    r"rule=(?P<rule>PYS\d{4})\s+"
    r"reason=(?P<reason>[a-z][a-z0-9-]*)\s+"
    r"ticket=(?P<ticket>[A-Z][A-Z0-9]+-\d+)\s+"
    r"expires=(?P<expires>\d{4}-\d{2}-\d{2})\s+"
    r"rationale=(?P<rationale>\S.+)"
)
_BOUNDARY_REASONS = frozenset({"protocol-required", "cleanup-finally", "cancellation-guard", "adapter-normalization"})
_FALLIBLE_PREFIXES = ("try_", "parse_", "load_", "fetch_", "read_", "decode_", "validate_", "ensure_", "resolve_", "find_")
_MODEL_DECORATORS = frozenset({"dataclass", "pydantic.dataclasses.dataclass"})
_PYDANTIC_MODEL_BASES = frozenset({
    "BaseModel",
    "pydantic.BaseModel",
    "RootModel",
    "pydantic.RootModel",
    "BaseSettings",
    "pydantic_settings.BaseSettings",
})
_MSGSPEC_MODEL_BASES = frozenset({"Struct", "msgspec.Struct"})
_MODEL_BASES = _PYDANTIC_MODEL_BASES | _MSGSPEC_MODEL_BASES
_PYDANTIC_CONFIG_CALLS = frozenset({"ConfigDict", "pydantic.ConfigDict", "SettingsConfigDict", "pydantic_settings.SettingsConfigDict"})
_CLASSVAR_NAMES = frozenset({"ClassVar"})
_EFFECT_BUILDER_DECORATORS = frozenset({"effect.result", "effect.async_result", "result", "async_result"})
_ERASED_NAMES = frozenset({"Any", "object"})
_MUTABLE_FIELD_NAMES = frozenset({"dict", "list", "set", "Dict", "List", "Set", "MutableMapping", "MutableSequence", "MutableSet"})
_PRIMITIVE_NAMES = frozenset({"bool", "float", "int", "str", "tuple"})
_RAIL_NAMES = frozenset({"Option", "Result"})
_RAIL_ESCAPE_METHODS = frozenset({"unwrap", "value_or"})
_PY_ANALYZER_ROOT = ("tools", "py_analyzer")
_QUALITY_ROOT = ("tools", "quality")
_TOOLING_ROOTS = ((".claude", "hooks"), (".claude", "skills"), _PY_ANALYZER_ROOT, _QUALITY_ROOT)
_DOMAIN_SCOPES = frozenset({Scope.domain, Scope.application})
_PUBLIC_SIGNATURE_BANNED_NAMES = _ERASED_NAMES | _MUTABLE_FIELD_NAMES | _PRIMITIVE_NAMES
_FLOW_NODE_TYPES = frozenset({"if_statement", "for_statement", "while_statement", "try_statement", "raise_statement"})

# --- [MODELS] ---------------------------------------------------------------------------


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
    annotation: Node
    node: Node


@dataclass(frozen=True, slots=True)
class ModuleFacts:
    """Immutable per-module facts emitted by one tree-sitter pass."""

    diagnostics: tuple[Diagnostic, ...]
    private_functions: tuple[FunctionFact, ...]
    private_calls: tuple[str, ...]
    models: tuple[ModelFact, ...]


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(slots=True)
class _ModulePass:
    """Mutable collector folded into immutable ModuleFacts after one module walk."""

    path: Path
    root: Path
    src: bytes
    source_lines: Sequence[str]
    scope: Scope
    diagnostics: list[Diagnostic]
    private_functions: list[FunctionFact]
    private_calls: list[str]
    models: list[ModelFact]
    effect_builder_depth: int = 0

    def facts(self) -> ModuleFacts:
        """Project mutable pass state into immutable analyzer facts.

        Returns:
            Immutable module facts for cross-file diagnostic passes.
        """
        return ModuleFacts(tuple(self.diagnostics), tuple(self.private_functions), tuple(self.private_calls), tuple(self.models))

    def report(self, rule_id: RuleId, node: Node, detail: str) -> None:
        line, column = _position(node)
        self.diagnostics.append(diagnostic(rule_id, self.path, line, column, detail))

    def walk(self, node: Node, parent: Node | None, grandparent: Node | None) -> None:
        effect_entry = False
        match node.type:
            case t if t in _FLOW_NODE_TYPES:
                self._check_flow(node, _flow_construct(node))
            case "decorated_definition":
                effect_entry = _decorated_is_effect_builder(node, self.src)
                match effect_entry:
                    case True:
                        self.effect_builder_depth += 1
                    case False:
                        pass
            case "function_definition":
                self._check_function(node, parent, grandparent)
            case "call":
                self._check_call(node)
            case "class_definition":
                self._check_model(node, parent)
            case _:
                pass
        for child in node.children:
            self.walk(child, node, parent)
        match effect_entry:
            case True:
                self.effect_builder_depth -= 1
            case False:
                pass

    def _in_domain_or_application(self) -> bool:
        return self.scope in _DOMAIN_SCOPES

    def _check_function(self, node: Node, parent: Node | None, grandparent: Node | None) -> None:
        name = _definition_name(node, self.src)
        module_parent = parent if parent is not None and parent.type == "module" else grandparent
        match (self._in_domain_or_application(), _is_module_level(parent, module_parent), name.startswith("_")):
            case (True, True, True):
                self.private_functions.append(_function_fact(self, node, parent, name))
            case (True, _, False):
                self._check_public_signature(node, name)
                self._check_fallible_return(node, name)
            case _:
                return

    def _check_call(self, node: Node) -> None:
        callee = node.child_by_field_name("function")
        match callee:
            case Node(type="identifier"):
                name = _node_text(self.src, callee)
                match name.startswith("_"):
                    case True:
                        self.private_calls.append(name)
                    case False:
                        pass
            case Node(type="attribute") as attr:
                method = attr.child_by_field_name("attribute")
                match (method, self._in_domain_or_application()):
                    case (Node() as m, True) if _node_text(self.src, m) in _RAIL_ESCAPE_METHODS:
                        self.report(RuleId.rail_escape, node, f"{_node_text(self.src, m)} collapses rails in {self.scope.value} scope.")
                    case (Node() as m, True) if _node_text(self.src, m) == "or_else_with" and self.effect_builder_depth > 0:
                        self.report(
                            RuleId.recovery_inside_effect,
                            node,
                            "or_else_with recovery belongs at the composition boundary, outside the effect builder.",
                        )
                    case _:
                        pass
            case _:
                pass

    def _check_model(self, node: Node, parent: Node | None) -> None:
        name = _definition_name(node, self.src)
        match (self._in_domain_or_application(), _is_model_class(node, parent, self.src), _model_shape(self, node, parent)):
            case (True, True, shape) if shape:
                self._check_model_policy(node, parent, name)
                line, column = _position(node)
                self.models.append(ModelFact(name, self.path, line, column, shape))
            case _:
                return

    def _check_flow(self, node: Node, construct: str) -> None:
        match self.scope:
            case Scope.domain | Scope.application:
                self.report(RuleId.domain_flow, node, f"{construct} is forbidden in {self.scope.value} scope.")
            case Scope.boundary:
                self._check_boundary_exemption(node, construct)
            case _:
                return

    def _check_model_policy(self, node: Node, parent: Node | None, class_name: str) -> None:
        match _model_immutability_violation(node, parent, self.src):
            case str(detail):
                self.report(RuleId.model_immutability, node, detail)
            case None:
                pass
        for field in _model_fields(node, self.src):
            match (
                _annotation_contains(field.annotation, self.src, _CLASSVAR_NAMES, nested=False),
                _annotation_contains(field.annotation, self.src, _MUTABLE_FIELD_NAMES),
            ):
                case (False, True):
                    self.report(
                        RuleId.mutable_model_field,
                        field.node,
                        f"{class_name}.{field.name} uses mutable annotation {_node_text(self.src, field.annotation)}.",
                    )
                case _:
                    pass

    def _check_boundary_exemption(self, node: Node, construct: str) -> None:
        line, _ = _position(node)
        match _valid_boundary_exemption(self.source_lines, line):
            case True:
                return
            case False:
                self.report(RuleId.boundary_exemption, node, f"{construct} requires rule/reason/ticket/expiry/rationale metadata.")

    def _check_public_signature(self, node: Node, name: str) -> None:
        primitive = next(
            (
                _node_text(self.src, annotation)
                for annotation in _function_annotations(node)
                if _annotation_contains(annotation, self.src, _PUBLIC_SIGNATURE_BANNED_NAMES)
            ),
            None,
        )
        match primitive:
            case str(value):
                self.report(RuleId.primitive_signature, node, f"{name} exposes primitive, erased, or mutable annotation {value}.")
            case None:
                return

    def _check_fallible_return(self, node: Node, name: str) -> None:
        returns = node.child_by_field_name("return_type")
        rail = bool(returns and _annotation_contains(returns, self.src, _RAIL_NAMES, nested=False))
        optional = bool(returns and _annotation_contains(returns, self.src, frozenset({"Optional"}), nested=False, none=True))
        match (name.startswith(_FALLIBLE_PREFIXES) or optional, rail):
            case (True, False):
                self.report(RuleId.fallible_return, node, f"{name} must return Result[T, E] or Option[T].")
            case _:
                return


# --- [OPERATIONS] -----------------------------------------------------------------------


def analyze_paths(root: Path, paths: Sequence[Path]) -> tuple[Diagnostic, ...]:
    """Analyze Python files under root and return stable diagnostics.

    Returns:
        Sorted analyzer diagnostics from local and cross-file rules.
    """
    resolved_root = root.resolve()
    module_facts = tuple(_analyze_file(resolved_root, path) for path in _discover_python_files(resolved_root, paths))
    diagnostics = tuple(item for facts in module_facts for item in facts.diagnostics)
    return tuple(
        sorted(
            diagnostics + _single_use_private_function_diagnostics(module_facts) + _duplicate_model_shape_diagnostics(module_facts),
            key=lambda value: (value.path.as_posix(), value.line, value.column, value.rule_id.value),
        )
    )


def classify_scope(path: Path, root: Path) -> Scope:
    """Classify a path into the Rasm semantic policy scope.

    Returns:
        Semantic scope assigned to the path.
    """
    parts = _relative_parts(path, root)
    part_set = frozenset(parts)
    tool = any(parts[: len(prefix)] == prefix for prefix in _TOOLING_ROOTS) or (
        len(parts) > 3 and parts[0] == ".claude" and parts[1] == "skills" and "scripts" in part_set
    )
    match (tool, bool({"adapters", "boundary", "boundaries"} & part_set), "domain" in part_set, "application" in part_set):
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


@cache
def _parser() -> Parser:
    return Parser(ts_language(_PYTHON_GRAMMAR))


def _analyze_file(root: Path, path: Path) -> ModuleFacts:
    resolved = path.resolve()
    try:
        source = resolved.read_text(encoding="utf-8")
        src = source.encode("utf-8")
    except OSError as error:
        return ModuleFacts((diagnostic(RuleId.parse, resolved, 1, 1, str(error)),), (), (), ())
    tree = _parser().parse(src)
    root_node = tree.root_node
    match root_node.has_error:
        case True:
            return ModuleFacts((diagnostic(RuleId.parse, resolved, 1, 1, "tree-sitter parse error"),), (), (), ())
        case False:
            pass
    module_pass = _ModulePass(resolved, root, src, source.splitlines(), classify_scope(resolved, root), [], [], [], [])
    for child in root_node.children:
        module_pass.walk(child, root_node, None)
    return module_pass.facts()


def _discover_python_files(root: Path, paths: Sequence[Path]) -> tuple[Path, ...]:
    anchors = paths or (root,)
    return tuple(
        sorted({path.resolve() for anchor in anchors for path in _expand_anchor(root, anchor) if path.suffix == ".py" and not _excluded(path, root)})
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
        names[:] = sorted(name for name in names if name not in _EXCLUDED_DIRS)
        yield from (directory / filename for filename in filenames if filename.endswith(".py"))


def _excluded(path: Path, root: Path) -> bool:
    parts = _relative_parts(path, root)
    return bool(_EXCLUDED_DIRS.intersection(parts) or any(parts[: len(prefix)] == prefix for prefix in _EXCLUDED_PREFIXES))


def _relative_parts(path: Path, root: Path) -> tuple[str, ...]:
    absolute = path.resolve()
    return absolute.relative_to(root).parts if absolute.is_relative_to(root) else absolute.parts


def _valid_boundary_exemption(source_lines: Sequence[str], line: int) -> bool:
    window = source_lines[max(line - 4, 0) : line]
    return any(
        match
        and match.group("rule") == RuleId.domain_flow.value
        and match.group("reason") in _BOUNDARY_REASONS
        and _future_date(match.group("expires"))
        and bool(match.group("rationale").strip())
        for match in (_BOUNDARY_EXEMPTION.search(text) for text in window)
    )


def _future_date(value: str) -> bool:
    try:
        return date.fromisoformat(value) > datetime.now(UTC).date()
    except ValueError:
        return False


def _position(node: Node) -> tuple[int, int]:
    return node.start_point.row + 1, node.start_point.column + 1


def _node_text(src: bytes, node: Node) -> str:
    return src[node.start_byte : node.end_byte].decode(errors="replace")


def _is_module_level(parent: Node | None, module_parent: Node | None) -> bool:
    match parent:
        case Node(type="module"):
            return True
        case Node(type="decorated_definition") if module_parent is not None and module_parent.type == "module":
            return True
        case _:
            return False


def _definition_name(node: Node, src: bytes) -> str:
    name = node.child_by_field_name("name")
    return _node_text(src, name) if name is not None else ""


def _function_fact(module_pass: _ModulePass, node: Node, parent: Node | None, name: str) -> FunctionFact:
    line, column = _position(node)
    owner = _decorator_owner(node, parent)
    decorators = frozenset(_decorator_name(decorator, module_pass.src) for decorator in _decorator_nodes(owner))
    return FunctionFact(
        name, module_pass.path, line, column, bool(decorators & {"overload", "singledispatch", "singledispatchmethod"} or name.startswith("__"))
    )


def _decorator_nodes(defn: Node) -> tuple[Node, ...]:
    match defn.type:
        case "decorated_definition":
            return tuple(child for child in defn.children if child.type == "decorator")
        case _:
            return ()


def _unwrap_definition(node: Node) -> Node:
    match node.type:
        case "decorated_definition":
            return next(child for child in node.children if child.type in {"function_definition", "class_definition"})
        case _:
            return node


def _function_annotations(node: Node) -> tuple[Node, ...]:
    defn = _unwrap_definition(node) if node.type == "decorated_definition" else node
    params = defn.child_by_field_name("parameters")
    param_annotations = tuple(
        child.children[2] for child in (params.children if params is not None else ()) if child.type == "typed_parameter" and len(child.children) >= 3
    )
    returns = defn.child_by_field_name("return_type")
    return_annotation = (returns,) if returns is not None else ()
    return param_annotations + return_annotation


def _decorator_name(node: Node, src: bytes) -> str:
    expr = node.children[-1] if node.children else node
    return _dotted_name(expr, src)


def _decorated_is_effect_builder(node: Node, src: bytes) -> bool:
    return bool(_EFFECT_BUILDER_DECORATORS.intersection(frozenset(_decorator_name(dec, src) for dec in _decorator_nodes(node))))


def _dotted_name(node: Node, src: bytes) -> str:
    match node.type:
        case "identifier":
            return _node_text(src, node)
        case "attribute":
            obj = node.child_by_field_name("object") or node.children[0]
            attr = node.child_by_field_name("attribute") or node.children[-1]
            prefix = _dotted_name(obj, src)
            suffix = _node_text(src, attr)
            return f"{prefix}.{suffix}" if prefix else suffix
        case "call":
            func = node.child_by_field_name("function") or node.children[0]
            return _dotted_name(func, src)
        case "subscript":
            value = node.child_by_field_name("value") or node.children[0]
            return _dotted_name(value, src)
        case _:
            return ""


def _annotation_head_name(node: Node, src: bytes) -> str:
    root = _annotation_root(node)
    name = _dotted_name(root, src)
    return name.rsplit(".", maxsplit=1)[-1] if name else ""


def _annotation_root(node: Node) -> Node:
    match node.type:
        case "type":
            return _annotation_root(node.children[0]) if node.children else node
        case _:
            return node


def _annotation_type_args(node: Node) -> tuple[Node, ...]:
    match node.type:
        case "type_parameter":
            return tuple(child for child in node.children if child.type not in {"[", "]", ","})
        case "subscript" | "generic_type":
            return tuple(arg for child in node.children if child.type == "type_parameter" for arg in _annotation_type_args(child)) + tuple(
                child for child in node.children if child.type not in {"identifier", "[", "]", ",", "type", "type_parameter"}
            )
        case _:
            return ()


def _annotation_contains(node: Node, src: bytes, names: frozenset[str], *, nested: bool = True, none: bool = False) -> bool:
    root = _annotation_root(node)
    match root.type:
        case "none":
            return none
        case "identifier" | "attribute":
            return _annotation_head_name(root, src) in names
        case "subscript" | "generic_type":
            value = root.child_by_field_name("type") or root.children[0]
            return _annotation_head_name(value, src) in names or (
                nested and any(_annotation_contains(arg, src, names, nested=nested, none=none) for arg in _annotation_type_args(root))
            )
        case "binary_operator":
            return any(_annotation_contains(child, src, names, nested=nested, none=none) for child in root.children if child.type != "|")
        case "tuple_expression" | "tuple_type":
            return nested and any(_annotation_contains(child, src, names, nested=nested, none=none) for child in root.children if child.type != ",")
        case _:
            return False


def _is_model_class(node: Node, parent: Node | None, src: bytes) -> bool:
    owner = _decorator_owner(node, parent)
    decorators = frozenset(_decorator_name(dec, src) for dec in _decorator_nodes(owner))
    bases = frozenset(_dotted_name(arg, src) for arg in _class_bases(node))
    return bool(decorators & _MODEL_DECORATORS or bases & _MODEL_BASES)


def _class_bases(node: Node) -> tuple[Node, ...]:
    arg_list = node.child_by_field_name("superclasses")
    return tuple(child for child in (arg_list.children if arg_list is not None else ()) if child.type not in {"(", ")", ","})


def _model_shape(module_pass: _ModulePass, node: Node, _parent: Node | None) -> ModelShape:
    return tuple(
        sorted(
            (
                (field.name, _annotation_key(field.annotation, module_pass.src))
                for field in _model_fields(node, module_pass.src)
                if not _annotation_contains(field.annotation, module_pass.src, _CLASSVAR_NAMES, nested=False)
            ),
            key=itemgetter(0),
        )
    )


def _model_fields(node: Node, src: bytes) -> tuple[ModelField, ...]:
    body = node.child_by_field_name("body")
    return tuple(field for statement in (body.children if body is not None else ()) if (field := _model_field(statement, src)))


def _model_field(statement: Node, src: bytes) -> ModelField | None:
    match statement.type:
        case "expression_statement":
            assignment = statement.children[0] if statement.children else None
            match assignment:
                case Node(type="assignment"):
                    target = assignment.children[0]
                    annotation = assignment.children[2] if len(assignment.children) >= 3 and _node_text(src, assignment.children[1]) == ":" else None
                    match (target, annotation):
                        case (Node(type="identifier"), Node() as ann):
                            return ModelField(_node_text(src, target), ann, assignment)
                        case _:
                            return None
                case _:
                    return None
        case _:
            return None


def _model_immutability_violation(node: Node, parent: Node | None, src: bytes) -> str | None:
    class_name = _definition_name(node, src)
    owner = _decorator_owner(node, parent)
    dataclass = _is_dataclass_model(owner, src)
    pydantic = _model_has_base(node, src, _PYDANTIC_MODEL_BASES)
    msgspec = _model_has_base(node, src, _MSGSPEC_MODEL_BASES)
    match (
        dataclass and not _dataclass_is_frozen_slots(owner, src),
        pydantic and not _pydantic_is_frozen(node, src),
        msgspec and not _msgspec_is_frozen(node, src),
    ):
        case (True, _, _):
            return f"{class_name} dataclass models require @dataclass(frozen=True, slots=True)."
        case (_, True, _):
            return f"{class_name} Pydantic models require frozen=True config."
        case (_, _, True):
            return f"{class_name} msgspec.Struct models require frozen=True."
        case _:
            return None


def _decorator_owner(defn: Node, parent: Node | None) -> Node:
    match parent:
        case Node(type="decorated_definition"):
            return parent
        case _:
            return defn


def _is_dataclass_model(node: Node, src: bytes) -> bool:
    return bool(_MODEL_DECORATORS.intersection(frozenset(_decorator_name(dec, src) for dec in _decorator_nodes(node))))


def _model_has_base(node: Node, src: bytes, bases: frozenset[str]) -> bool:
    return bool(bases.intersection(frozenset(_dotted_name(arg, src) for arg in _class_bases(node))))


def _dataclass_is_frozen_slots(owner: Node, src: bytes) -> bool:
    return any(
        _decorator_name(dec, src) in _MODEL_DECORATORS
        and (expr := dec.children[-1]).type == "call"
        and _call_has_true_keyword(expr, src, "frozen")
        and _call_has_true_keyword(expr, src, "slots")
        for dec in _decorator_nodes(owner)
    )


def _pydantic_is_frozen(node: Node, src: bytes) -> bool:
    return _class_has_true_keyword(node, src, "frozen") or _pydantic_configdict_is_frozen(node, src)


def _msgspec_is_frozen(node: Node, src: bytes) -> bool:
    return _class_has_true_keyword(node, src, "frozen")


def _class_has_true_keyword(node: Node, src: bytes, name: str) -> bool:
    arg_list = node.child_by_field_name("superclasses")
    return any(
        _keyword_is_true(arg, src, name) or (arg.type == "call" and _call_has_true_keyword(arg, src, name))
        for arg in (arg_list.children if arg_list is not None else ())
        if arg.type not in {"(", ")", ","}
    )


def _keyword_is_true(arg: Node, src: bytes, name: str) -> bool:
    return arg.type == "keyword_argument" and _node_text(src, arg.children[0]) == name and _is_true_literal(arg.children[-1], src)


def _pydantic_configdict_is_frozen(node: Node, src: bytes) -> bool:
    body = node.child_by_field_name("body")
    return any(
        _call_has_true_keyword(call, src, "frozen")
        for statement in (body.children if body is not None else ())
        for call in (_model_configdict_call(statement, src),)
        if call is not None
    )


def _model_configdict_call(statement: Node, src: bytes) -> Node | None:
    match statement.type:
        case "expression_statement":
            assignment = statement.children[0] if statement.children else None
            match assignment:
                case Node(type="assignment"):
                    target = assignment.children[0]
                    value = assignment.children[-1]
                    match (target, value):
                        case (Node(type="identifier"), Node(type="call") as call) if (
                            _node_text(src, target) == "model_config"
                            and _dotted_name(call.child_by_field_name("function") or call.children[0], src) in _PYDANTIC_CONFIG_CALLS
                        ):
                            return call
                        case _:
                            return None
                case _:
                    return None
        case _:
            return None


def _call_has_true_keyword(call: Node, src: bytes, name: str) -> bool:
    args = call.child_by_field_name("arguments")
    return any(
        _node_text(src, arg.children[0]) == name and _is_true_literal(arg.children[-1], src)
        for arg in (args.children if args is not None else ())
        if arg.type == "keyword_argument"
    ) or any(
        _node_text(src, child) == name and _is_true_literal(child.children[-1], src)
        for child in (args.children if args is not None else ())
        if child.type == "keyword_argument"
    )


def _is_true_literal(node: Node, src: bytes) -> bool:
    return node.type == "true" or (node.type == "identifier" and _node_text(src, node) == "True")


def _annotation_key(node: Node, src: bytes) -> str:
    root = _annotation_root(node)
    match root.type:
        case "identifier" | "attribute":
            return _annotation_head_name(root, src)
        case "subscript" | "generic_type":
            head = _annotation_key(root.children[0], src)
            inner = ",".join(
                _annotation_key(child, src) for child in root.children if child.type not in {"[", "]", ",", "type"} and child is not root.children[0]
            )
            return f"{head}[{inner}]" if inner else head
        case "binary_operator":
            parts = sorted(_annotation_key(child, src) for child in root.children if child.type != "|")
            return "|".join(parts)
        case _:
            return _node_text(src, root).replace(" ", "")


def _flow_construct(node: Node) -> str:
    match node.type:
        case "if_statement":
            return "if"
        case "for_statement" | "while_statement":
            return "loop"
        case "try_statement":
            return "try"
        case "raise_statement":
            return "raise"
        case _:
            return "flow"


def _single_use_private_function_diagnostics(module_facts: Sequence[ModuleFacts]) -> tuple[Diagnostic, ...]:
    return tuple(
        diagnostic(RuleId.single_use_private, function.path, function.line, function.column, f"{function.name} has exactly one same-module call.")
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
        diagnostic(RuleId.duplicate_model, model.path, model.line, model.column, f"{model.name} duplicates {group[0].name} in {group[0].path.name}.")
        for _, grouped in groupby(models, key=attrgetter("shape"))
        for group in (tuple(grouped),)
        if len(group) > 1
        for model in group[1:]
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("analyze_paths", "classify_scope", "PY_ANALYZER_ROOT")

PY_ANALYZER_ROOT = _PY_ANALYZER_ROOT
