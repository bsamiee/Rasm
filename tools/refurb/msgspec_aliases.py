"""Type aliases msgspec expands without end from a `msgspec.Struct` field or a msgspec type argument."""

from collections.abc import Iterable, Iterator

from mypy.nodes import ArgKind, AssignmentStmt, Block, CallExpr, ClassDef, Expression, IndexExpr, ListExpr, NameExpr, OpExpr, RefExpr, TupleExpr, TypeAlias, TypeApplication, TypeInfo, Var
from mypy.types import Instance, TupleType, Type, TypeAliasType, TypedDictType, UnionType, UnpackType
from refurb.error import Error

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorMsgspecRecursiveAliasError(Error):
    """Recursive type alias with no `msgspec.Struct`, dataclass, TypedDict, or NamedTuple on its cycle.

    Bad:

    ```
    type Plain = bool | int | float | str | list[Plain] | dict[str, Plain] | None


    class Held(msgspec.Struct, frozen=True):
        values: dict[str, Plain]


    msgspec.json.decode(data, type=dict[str, Plain])
    ```

    Good:

    ```
    class Held(msgspec.Struct, frozen=True):
        values: dict[str, object]


    class Node(msgspec.Struct, frozen=True):
        children: list[Node]
    ```
    """

    prefix = "RASM"
    code = 104
    name = "msgspec-recursive-alias"
    categories = ("msgspec",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: ClassDef | CallExpr, errors: list[Error]) -> None:
    """Reports each `msgspec.Struct` field and msgspec type argument reaching an alias cycle no cached class breaks."""
    typed = {
        "msgspec.convert": (1, "type"),
        "msgspec.json.decode": (None, "type"),
        "msgspec.msgpack.decode": (None, "type"),
        "msgspec.yaml.decode": (None, "type"),
        "msgspec.toml.decode": (None, "type"),
        "msgspec.json.Decoder": (0, "type"),
        "msgspec.msgpack.Decoder": (0, "type"),
        "msgspec.json.schema": (0, "type"),
        "msgspec.json.schema_components": (0, "types"),
        "msgspec.inspect.type_info": (0, "type"),
        "msgspec.inspect.multi_type_info": (0, "types"),
    }
    containers = frozenset({
        *(f"builtins.{name}" for name in ("list", "tuple", "set", "frozenset", "dict", "frozendict")),
        *(f"typing.{name}" for name in ("Collection", "Sequence", "MutableSequence", "AbstractSet", "MutableSet", "Mapping", "MutableMapping")),
    })

    def recorded(info: TypeInfo, members: Iterable[Type], expanded: frozenset[str]) -> Iterator[str]:
        if info.fullname not in expanded:
            for member in members:
                yield from looped(member, frozenset(), expanded | {info.fullname})

    def looped(kind: Type, trail: frozenset[str], expanded: frozenset[str]) -> Iterator[str]:
        match kind:
            case TypeAliasType(alias=TypeAlias(fullname=name)) if name in trail:
                yield name
            case TypeAliasType(alias=TypeAlias(fullname=name)) if name in expanded:
                pass
            case TypeAliasType(alias=TypeAlias(fullname=name, target=target), args=args):
                yield from looped(target, trail | {name}, expanded | {name})
                for arg in args:
                    yield from looped(arg, trail, expanded)
            case Instance(type=info, args=args) if info.has_base("msgspec.Struct"):
                for arg in args:
                    yield from looped(arg, frozenset(), expanded)
            case Instance(type=TypeInfo(metadata={"dataclass": {"attributes": attributes}}) as info):
                yield from recorded(
                    info, (var.type for attribute in attributes if (symbol := info.get(attribute["name"])) is not None and isinstance(var := symbol.node, Var) and var.type is not None), expanded
                )
            case TypedDictType(items=items, fallback=Instance(type=info)):
                yield from recorded(info, items.values(), expanded)
            case TupleType(items=items, partial_fallback=Instance(type=info)) if info.is_named_tuple:
                yield from recorded(info, items, expanded)
            case Instance(type=TypeInfo(fullname=name), args=args) if name in containers:
                for arg in args:
                    yield from looped(arg, trail, expanded)
            case UnionType(items=items) | TupleType(items=items):
                for item in items:
                    yield from looped(item, trail, expanded)
            case UnpackType(type=inner):
                yield from looped(inner, trail, expanded)
            case _:
                pass

    def expressed(expression: Expression) -> Iterator[str]:
        match expression:
            case RefExpr(node=TypeAlias(fullname=name, target=target)):
                yield from looped(target, frozenset({name}), frozenset({name}))
            case RefExpr(node=TypeInfo(typeddict_type=TypedDictType() as record) | TypeInfo(tuple_type=TupleType() as record)):
                yield from looped(record, frozenset(), frozenset())
            case RefExpr(node=TypeInfo() as info):
                yield from looped(Instance(info, []), frozenset(), frozenset())
            case IndexExpr(base=base, analyzed=TypeApplication(types=types)):
                yield from expressed(base)
                for kind in types:
                    yield from looped(kind, frozenset(), frozenset())
            case IndexExpr(base=base, index=index):
                yield from expressed(base)
                yield from expressed(index)
            case TupleExpr(items=items) | ListExpr(items=items):
                for item in items:
                    yield from expressed(item)
            case OpExpr(op="|", left=left, right=right):
                yield from expressed(left)
                yield from expressed(right)
            case _:
                pass

    def report(at: AssignmentStmt | Expression, site: str, alias: str) -> None:
        errors.append(
            ErrorMsgspecRecursiveAliasError.from_node(
                at, f"{site} reaches `{alias.rpartition('.')[2]}`, an alias msgspec expands without end, recurse through a `msgspec.Struct` or type the value `object`"
            )
        )

    match node:
        case ClassDef(info=info, defs=Block(body=body)) if info.has_base("msgspec.Struct"):
            for statement in body:
                match statement:
                    case AssignmentStmt(lvalues=[NameExpr(name=field, node=Var(is_classvar=False))], type=Type() as kind) if (alias := next(looped(kind, frozenset(), frozenset()), None)) is not None:
                        report(statement, f"`{node.name}.{field}`", alias)
                    case _:
                        pass
        case CallExpr(callee=RefExpr(fullname=callee)) if callee in typed:
            index, keyword = typed[callee]
            positional = [argument for argument, kind in zip(node.args, node.arg_kinds) if kind == ArgKind.ARG_POS]
            named = [argument for argument, name in zip(node.args, node.arg_names) if name == keyword]
            for argument in (*named, *([positional[index]] if index is not None and len(positional) > index else ())):
                if (alias := next(expressed(argument), None)) is not None:
                    report(argument, f"`{callee}` type", alias)
        case _:
            pass


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ErrorMsgspecRecursiveAliasError", "check"]
