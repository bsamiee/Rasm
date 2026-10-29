"""The graph expression algebra: output sockets of desired nodes as values, their operators, and the node builders they compose."""

from collections.abc import Mapping
from types import MappingProxyType
from typing import Final, Literal, TYPE_CHECKING

import bpy
from expression import Nothing, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.desired import Content, End, INPUTS, member_paths, Members
from blender.bpy.nodes.interface import MenuItem, MenuItems
from blender.bpy.nodes.trees import base_socket, DATA_TYPES

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Term = Expr | End | Value
type Scalar = Expr | End | float
type Component = Literal["X", "Y", "Z", "Red", "Green", "Blue", "Alpha"]
type Shape = type[bpy.types.NodeSocket]

# --- [CONSTANTS] ------------------------------------------------------------------------

_SPREAD: Final = frozenset({"ADD", "SUBTRACT", "MULTIPLY", "DIVIDE", "POWER"})

# --- [MODELS] ---------------------------------------------------------------------------


class Expr(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Output socket of one desired node over term inputs, keyed at lowering by label or content digest.

    Arithmetic and comparison operators build Math, Vector Math, or Mix nodes by the wider operand shape, scalar literals spreading
    over vector and color channels; subscripts read a vector or color component. Kept expressions lower beside the node unlinked. Node classes are the shader
    tree's, each tree's own class resolved at lowering, and `shape` is the socket class the output reads as.
    """

    kind: type[bpy.types.Node]
    inputs: tuple[tuple[str | int, Term], ...] = ()
    members: Members = MappingProxyType({})
    content: Option[Content] = Nothing
    socket: str | int = "Value"
    shape: Shape = bpy.types.NodeSocketFloat
    label: Option[Key] = Nothing
    kept: tuple["Expr", ...] = ()

    def __post_init__(self) -> None:
        """Holds members under their paths and the shape as its socket class."""
        msgspec.structs.force_setattr(self, "members", member_paths(self.members))
        msgspec.structs.force_setattr(self, "shape", base_socket(self.shape))

    def __add__(self, other: Term, /) -> "Expr":
        """Builds the sum node."""
        return _combined("ADD", self, other)

    def __radd__(self, other: Term, /) -> "Expr":
        """Builds the sum node of a leading term."""
        return _combined("ADD", other, self)

    def __sub__(self, other: Term, /) -> "Expr":
        """Builds the difference node."""
        return _combined("SUBTRACT", self, other)

    def __rsub__(self, other: Term, /) -> "Expr":
        """Builds the difference node from a leading term."""
        return _combined("SUBTRACT", other, self)

    def __mul__(self, other: Term, /) -> "Expr":
        """Builds the product node."""
        return _combined("MULTIPLY", self, other)

    def __rmul__(self, other: Term, /) -> "Expr":
        """Builds the product node of a leading term."""
        return _combined("MULTIPLY", other, self)

    def __truediv__(self, other: Term, /) -> "Expr":
        """Builds the quotient node."""
        return _combined("DIVIDE", self, other)

    def __rtruediv__(self, other: Term, /) -> "Expr":
        """Builds the quotient node of a leading term."""
        return _combined("DIVIDE", other, self)

    def __pow__(self, other: Term, /) -> "Expr":
        """Builds the power node."""
        return _combined("POWER", self, other)

    def __rpow__(self, other: Term, /) -> "Expr":
        """Builds the power node of a leading base."""
        return _combined("POWER", other, self)

    def __mod__(self, other: Term, /) -> "Expr":
        """Builds the floored modulo node."""
        return _combined("FLOORED_MODULO", self, other)

    def __rmod__(self, other: Term, /) -> "Expr":
        """Builds the floored modulo node of a leading term."""
        return _combined("FLOORED_MODULO", other, self)

    def __lt__(self, other: Term, /) -> "Expr":
        """Builds the less-than node."""
        return _combined("LESS_THAN", self, other)

    def __gt__(self, other: Term, /) -> "Expr":
        """Builds the greater-than node."""
        return _combined("GREATER_THAN", self, other)

    def __neg__(self) -> "Expr":
        """Builds the negation node."""
        return _combined("MULTIPLY", self, -1.0)

    def __abs__(self) -> "Expr":
        """Takes the magnitude of every component.

        Returns:
            Absolute Math or Vector Math expression.

        Raises:
            TypeError: For a color, which no Math or Vector Math node takes.
        """
        match self.shape:
            case bpy.types.NodeSocketVector:
                return vector("ABSOLUTE", self)
            case bpy.types.NodeSocketColor:
                raise TypeError(f"{self.kind.__name__}.{self.socket} is a color, which has no absolute value")
            case _:
                return math("ABSOLUTE", self)

    def __getitem__(self, component: Component) -> "Expr":
        """Reads one vector or color component."""
        match component:
            case "X" | "Y" | "Z":
                return Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", self),), socket=component)
            case "Red" | "Green" | "Blue" | "Alpha":
                return Expr(kind=bpy.types.ShaderNodeSeparateColor, inputs=((0, self),), members={"mode": "RGB"}, socket=component)

    def named(self, key: Key) -> "Expr":
        """Returns the same node under a stable key, for nodes stored, edited, viewed, or inspected by key."""
        return msgspec.structs.replace(self, label=Some(key))

    def keeping(self, *roots: "Expr") -> "Expr":
        """Returns the same node holding `roots` in every graph it lowers into, though no input reads them."""
        return msgspec.structs.replace(self, kept=(*self.kept, *roots))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ALGEBRA]


def math(operation: "rna_enums.NodeMathItems", /, *operands: Term, clamp: bool = False) -> Expr:
    """Builds Math node of one operation over its enabled inputs in order, clamped to [0, 1] under `clamp`, read at `Value`."""
    return Expr(kind=bpy.types.ShaderNodeMath, inputs=tuple(enumerate(operands)), members={"operation": operation, "use_clamp": clamp})


def vector(operation: "rna_enums.NodeVecMathItems", /, *operands: Term) -> Expr:
    """Builds Vector Math node of one operation over its enabled inputs in order, the scalar `Scale` among them where the operation reads one.

    Returns:
        `Value` socket expression for scalar results, else `Vector`.
    """
    scalar, scaled = operation in {"DOT_PRODUCT", "DISTANCE", "LENGTH"}, operation in {"SCALE", "REFRACT"}
    spread = tuple(operand if scaled and index == len(operands) - 1 else _spread(operand, bpy.types.NodeSocketVector) for index, operand in enumerate(operands))
    return Expr(
        kind=bpy.types.ShaderNodeVectorMath,
        inputs=tuple(enumerate(spread)),
        members={"operation": operation},
        socket="Value" if scalar else "Vector",
        shape=bpy.types.NodeSocketFloat if scalar else bpy.types.NodeSocketVector,
    )


def mixed(factor: Scalar, a: Term, b: Term, /, *, blend: "rna_enums.RampBlendItems" = "MIX") -> Expr:
    """Builds Mix node of the operands' shape, `a` at factor 0 and `b` at factor 1, blended by `ShaderNodeMix.blend_type` identifier."""
    return _mixed(widest(a, b), factor, a, b, blend=blend)


def switched(gate: Scalar, off: Term, on: Term, /) -> Expr:
    """Builds lazy Switch node of the branches' shape over a single-value gate, `off` for false and `on` for true."""
    shape = widest(off, on)
    return Expr(kind=bpy.types.GeometryNodeSwitch, inputs=(("Switch", gate), ("False", off), ("True", on)), members={"input_type": DATA_TYPES[shape]}, socket="Output", shape=shape)


def menu(selector: Term | MenuItem, cases: Mapping[MenuItem, Term], /, *, shape: Option[Shape] = Nothing) -> Expr:
    """Builds Menu Switch over items in case order, each case operand at the socket of its declaring item, every item's boolean output beside `Output`.

    Args:
        selector: Menu input operand, or the item chosen.
        cases: Term per declaring item, the items the switch holds.
        shape: Socket class of the cases where literals cannot state it, such as menu cases given as item names.

    Returns:
        `Output` socket expression of the cases' widest shape.
    """
    kind = shape.default_with(lambda: widest(*cases.values()))
    chosen: Term = selector.name if isinstance(selector, MenuItem) else selector
    return Expr(
        kind=bpy.types.GeometryNodeMenuSwitch,
        inputs=(("Menu", chosen), *((item.name, term) for item, term in cases.items())),
        members={"data_type": DATA_TYPES[kind]},
        content=Some(MenuItems(items=tuple(cases))),
        socket="Output",
        shape=kind,
    )


def where(gate: Term, on: Term, off: Term) -> Expr:
    """Selects each pixel by binary mask over both evaluated branches, dropping the unselected branch's nonfinite values.

    Args:
        gate: Boolean or comparison term, 1 for `on` and 0 for `off`.
        on: Term selected at 1.
        off: Term selected at 0.

    Returns:
        `Value`, `Vector`, or `Image` expression by the branches' shape, `off / (1 - gate) + on / gate` under safe division.
    """
    shape = widest(on, off)
    inverse = math("SUBTRACT", 1.0, gate)
    match shape:
        case bpy.types.NodeSocketVector:
            return vector("ADD", vector("DIVIDE", _spread(off, shape), inverse), vector("DIVIDE", _spread(on, shape), gate))
        case bpy.types.NodeSocketColor:
            red, green, blue, alpha = (
                where(gate, *(Expr(kind=bpy.types.ShaderNodeSeparateColor, inputs=((0, _spread(branch, shape)),), members={"mode": "RGB"}, socket=channel) for branch in (on, off)))
                for channel in ("Red", "Green", "Blue", "Alpha")
            )
            return rgba(red, green, blue, alpha)
        case _:
            return math("ADD", math("DIVIDE", off, inverse), math("DIVIDE", on, gate))


def equal(a: Scalar, b: Scalar, /) -> Expr:
    """Builds exact float Compare of two scalars, the geometry and compositor trees' test, true where they hold one value."""
    return Expr(
        kind=bpy.types.FunctionNodeCompare, inputs=(("A", a), ("B", b), ("Epsilon", 0.0)), members={"data_type": "FLOAT", "operation": "EQUAL"}, socket="Result", shape=bpy.types.NodeSocketBool
    )


def group_input(key: Key, shape: Shape = bpy.types.NodeSocketFloat) -> Expr:
    """Reads one Group Input output of the group being built, by interface item key."""
    return Expr(kind=bpy.types.NodeGroupInput, socket=key, shape=shape, label=Some(INPUTS))


def xyz(x: Scalar = 0.0, y: Scalar = 0.0, z: Scalar = 0.0) -> Expr:
    """Builds Combine XYZ vector of three scalars."""
    return Expr(kind=bpy.types.ShaderNodeCombineXYZ, inputs=(("X", x), ("Y", y), ("Z", z)), socket="Vector", shape=bpy.types.NodeSocketVector)


def rgba(red: Scalar, green: Scalar, blue: Scalar, alpha: Scalar = 1.0) -> Expr:
    """Builds Combine Color of four RGB channels."""
    return Expr(kind=bpy.types.ShaderNodeCombineColor, inputs=(("Red", red), ("Green", green), ("Blue", blue), ("Alpha", alpha)), members={"mode": "RGB"}, socket=0, shape=bpy.types.NodeSocketColor)


def widest(*terms: Term) -> Shape:
    """Returns the widest socket class among terms, any other class over floats, a literal reading as the class that holds it."""

    def of(term: Term) -> Shape:
        match term:
            case Expr(shape=shape):
                return shape
            case bool():
                return bpy.types.NodeSocketBool
            case str():
                return bpy.types.NodeSocketString
            case (_, _, _):
                return bpy.types.NodeSocketVector
            case (_, _, _, _):
                return bpy.types.NodeSocketColor
            case _:
                return bpy.types.NodeSocketFloat

    return next((shape for shape in map(of, terms) if shape is not bpy.types.NodeSocketFloat), bpy.types.NodeSocketFloat)


def _combined(operation: str, a: Term, b: Term) -> Expr:
    shape = widest(a, b)
    match shape:
        case bpy.types.NodeSocketVector if operation in _SPREAD:
            return Expr(
                kind=bpy.types.ShaderNodeVectorMath, inputs=((0, _spread(a, shape)), (1, _spread(b, shape))), members={"operation": operation}, socket="Vector", shape=bpy.types.NodeSocketVector
            )
        case bpy.types.NodeSocketColor if operation == "POWER":
            raise TypeError("Mix nodes hold no power blend")
        case bpy.types.NodeSocketColor if operation in _SPREAD:
            return _mixed(shape, 1.0, _spread(a, shape), _spread(b, shape), blend=operation)
        case _:
            return Expr(kind=bpy.types.ShaderNodeMath, inputs=((0, a), (1, b)), members={"operation": operation, "use_clamp": False})


def _mixed(shape: Shape, factor: Term, a: Term, b: Term, *, blend: str) -> Expr:
    return Expr(kind=bpy.types.ShaderNodeMix, inputs=tuple(enumerate((factor, a, b))), members={"data_type": DATA_TYPES[shape], "blend_type": blend}, socket=0, shape=shape)


def _spread(term: Term, shape: Shape) -> Term:
    match term, shape:
        case (int() | float()) as value, bpy.types.NodeSocketVector if not isinstance(value, bool):
            return (value, value, value)
        case (int() | float()) as value, bpy.types.NodeSocketColor if not isinstance(value, bool):
            return (value, value, value, 1.0)
        case _:
            return term


# --- [MATRICES]


def matrix(cells: Mapping[tuple[int, int], Scalar], /) -> Expr:
    """Builds Combine Matrix of cells keyed by row and column counted from one, identity elsewhere."""
    return Expr(
        kind=bpy.types.FunctionNodeCombineMatrix, inputs=tuple((f"Column {column} Row {row}", value) for (row, column), value in cells.items()), socket="Matrix", shape=bpy.types.NodeSocketMatrix
    )


def cell(source: Expr, row: int, column: int, /) -> Expr:
    """Reads one matrix entry by row and column counted from one."""
    return Expr(kind=bpy.types.FunctionNodeSeparateMatrix, inputs=(("Matrix", source),), socket=f"Column {column} Row {row}")


def composed(first: Expr, second: Expr, /) -> Expr:
    """Matrix product of two matrix terms, `second` applied first."""
    return Expr(kind=bpy.types.FunctionNodeMatrixMultiply, inputs=(("Matrix", first), ("Matrix_001", second)), socket="Matrix", shape=bpy.types.NodeSocketMatrix)


def inverted(source: Expr, /) -> Expr:
    """Inverse of a matrix term, its `Invertible` output beside."""
    return Expr(kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", source),), socket="Matrix", shape=bpy.types.NodeSocketMatrix)


def transform(component: Literal["Translation", "Rotation", "Scale"], value: Expr, /) -> Expr:
    """Transform matrix of one component."""
    return Expr(kind=bpy.types.FunctionNodeCombineTransform, inputs=((component, value),), socket="Transform", shape=bpy.types.NodeSocketMatrix)


def transformed(point: Term, source: Expr, /) -> Expr:
    """Point transformed by a matrix term."""
    return Expr(kind=bpy.types.FunctionNodeTransformPoint, inputs=(("Vector", point), ("Transform", source)), socket="Vector", shape=bpy.types.NodeSocketVector)


def directed(direction: Term, source: Expr, /) -> Expr:
    """Direction transformed by a matrix term, translation left out."""
    return Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", direction), ("Transform", source)), socket="Direction", shape=bpy.types.NodeSocketVector)


def projected(point: Term, source: Expr, /) -> Expr:
    """Point projected through a matrix term with perspective division."""
    return Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", point), ("Transform", source)), socket="Vector", shape=bpy.types.NodeSocketVector)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Component",
    "Expr",
    "Scalar",
    "Shape",
    "Term",
    "cell",
    "composed",
    "directed",
    "equal",
    "group_input",
    "inverted",
    "math",
    "matrix",
    "menu",
    "mixed",
    "projected",
    "rgba",
    "switched",
    "transform",
    "transformed",
    "vector",
    "where",
    "widest",
    "xyz",
]
