"""`Result` errors typed as `str` or as a bare exception class."""

from expression import Result
from mypy.nodes import Block, ClassDef, Decorator, FuncDef, MypyFile, Node, Statement, TypeAlias
from mypy.types import CallableType, get_proper_type, Instance, ProperType, TupleType, Type, TypeAliasType
from refurb.error import Error

# --- [CONSTANTS] ------------------------------------------------------------------------

_RESULT = f"{Result.__module__}.{Result.__qualname__}"

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorResultFaultError(Error):
    """`Result` error typed as `str` or as a bare exception class.

    Bad:

    ```
    def parsed(text: str) -> Result[int, str]: ...
    ```

    Good:

    ```
    def parsed(text: str) -> Result[int, Fault]: ...
    ```
    """

    prefix = "RASM"
    code = 101
    name = "result-fault"
    categories = ("expression",)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _defined(statement: Statement) -> tuple[FuncDef, ...]:
    match statement:
        case FuncDef():
            return (statement,)
        case Decorator(func=func):
            return (func,)
        case ClassDef(defs=Block(body=body)):
            return tuple(*_defined(inner) for inner in body)
        case _:
            return ()


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each `Result` error typed as `str` or as a bare exception class on a module-level alias, function, or method."""

    def element(error: Type) -> ProperType:
        match get_proper_type(error):
            case TupleType(items=[first, *_]):
                return get_proper_type(first)
            case Instance(type=info, args=[inner]) if info.fullname == "builtins.tuple":
                return get_proper_type(inner)
            case other:
                return other

    def reported(declaration: Node, name: str, returned: Type, *, aliased: bool) -> tuple[Error, ...]:
        match get_proper_type(returned):
            case Instance(type=info, args=[_, error]) if info.fullname == _RESULT and (aliased or not isinstance(returned, TypeAliasType)):
                match element(error):
                    case Instance(type=untyped) if untyped.fullname in {"builtins.str", "builtins.Exception", "builtins.BaseException"}:
                        return (ErrorResultFaultError.from_node(declaration, f"`{name}` types its `Result` error as `{untyped.name}`, type it as the package fault"),)
                    case _:
                        return ()
            case _:
                return ()

    errors.extend(
        *reported(alias, name, alias.target, aliased=True) for name, symbol in node.names.items() if isinstance(alias := symbol.node, TypeAlias) and alias.fullname == f"{node.fullname}.{name}"
    )
    errors.extend(*reported(func, func.name, signature.ret_type, aliased=False) for statement in node.defs for func in _defined(statement) if isinstance(signature := func.type, CallableType))
