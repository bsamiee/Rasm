"""Result-returning functions whose error type another first-party package owns or holds as text."""

from expression import Result
from mypy.nodes import Block, ClassDef, Decorator, FuncDef, MypyFile, Statement, TypeAlias, TypeInfo
from mypy.types import CallableType, get_proper_type, Instance, TupleType, Type, TypeAliasType, UnionType, UnpackType
from refurb.error import Error

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorResultFaultError(Error):
    """A `Result` carries faults of the package that returns it.

    Errors are typed variants of the raising package, each consumer maps another package's faults into its own at the package boundary, and recovery reads a variant in place of message text:

    Bad:

    ```
    def whitened(record: ToneCurve) -> ImagingResult[ToneCurve]: ...  # in blender.bpy
    def parsed(text: str) -> Result[int, str]: ...
    ```

    Good:

    ```
    def whitened(record: ToneCurve) -> BlenderResult[ToneCurve]: ...  # maps through BlenderImagingFault.lifted
    def parsed(text: str) -> Result[int, ParseFaults]: ...
    ```
    """

    prefix = "RASM"
    code = 101
    name = "result-fault"
    categories = ("expression",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: MypyFile, errors: list[Error]) -> None:
    """Reports each module-level function and method whose `Result` error is text or a fault another first-party package declares."""
    root = node.fullname.partition(".")[0]

    def defined(statement: Statement) -> tuple[FuncDef, ...]:
        match statement:
            case FuncDef():
                return (statement,)
            case Decorator(func=func):
                return (func,)
            case ClassDef(defs=Block(body=body)):
                return tuple(func for inner in body for func in defined(inner))
            case _:
                return ()

    def owners(error: Type) -> frozenset[str]:
        match error:
            case TypeAliasType(alias=TypeAlias(module=module), args=[]):
                return frozenset({module})
            case TypeAliasType(args=items) | UnionType(items=items) | TupleType(items=items) | Instance(type=TypeInfo(module_name="builtins"), args=items):
                return frozenset().union(*map(owners, items))
            case UnpackType(type=inner):
                return owners(inner)
            case Instance(type=TypeInfo(module_name=module)):
                return frozenset({module})
            case _:
                return frozenset()

    for func, error in (
        (func, result.args[1])
        for statement in node.defs
        for func in defined(statement)
        if isinstance(func.type, CallableType) and isinstance(result := get_proper_type(func.type.ret_type), Instance) and result.type.fullname == f"{Result.__module__}.{Result.__qualname__}"
    ):
        text = get_proper_type(error)
        foreign = sorted(owner for owner in owners(error) if owner.partition(".")[0] == root and not f"{node.fullname}.".startswith(f"{owner.rpartition('.')[0] or owner}."))
        if isinstance(text, Instance) and text.type.fullname == "builtins.str":
            errors.append(ErrorResultFaultError.from_node(func, f"`{func.name}` carries its error as text, return a typed fault"))
        elif foreign:
            errors.append(ErrorResultFaultError.from_node(func, f"`{func.name}` returns faults of `{', '.join(foreign)}` from `{node.fullname}`, map them into this package's fault"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ErrorResultFaultError", "check"]
