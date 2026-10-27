# [EXPRESSIONS]

## [01]-[ALGEBRA]

Operators on frozen records return the record of the node computing them:
- Operators come from one parameterized method bound per operation through `partialmethod`, reflected forms swapping operands
- Operand shape picks the node: Math for scalars, Vector Math for vectors with scalars spread, Mix at factor 1 for colors with `blend_type` as operation
- Node kinds per operation and record case stay typed (`type[bpy.types.Node]`, `rna_enums.NodeMathItems`), and lowering derives `bl_idname` from the class
- Classes differing by tree kind (Separate Color, Combine Color) resolve at lowering through one table keyed by tree class
- Group bodies are functions over expressions, built once at declaration and placed as a group node when called
- Body parameters become input sockets titled through `bpy.path.display_name`, and a returned mapping becomes outputs

## [02]-[LOWERING]

Keys digest each record with its whole operand tree, equal subexpressions share one node, and lowering removes keys the expression lacks and creates keys the tree lacks:

```python
from collections.abc import Iterator, Mapping
from functools import partialmethod
from hashlib import blake2b
from typing import TYPE_CHECKING

import bpy
import msgspec

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

type Term = Read | Math


class Expr(msgspec.Struct, frozen=True, tag=True):
    def _math(self, operation: "rna_enums.NodeMathItems", other: "Expr | float", *, reflected: bool = False) -> "Math":
        return Math(operation=operation, operands=(other, self) if reflected else (self, other))

    __add__ = partialmethod(_math, "ADD")
    __radd__ = partialmethod(_math, "ADD", reflected=True)
    __sub__ = partialmethod(_math, "SUBTRACT")
    __rsub__ = partialmethod(_math, "SUBTRACT", reflected=True)
    __mul__ = partialmethod(_math, "MULTIPLY")
    __rmul__ = partialmethod(_math, "MULTIPLY", reflected=True)
    __truediv__ = partialmethod(_math, "DIVIDE")
    __rtruediv__ = partialmethod(_math, "DIVIDE", reflected=True)
    __pow__ = partialmethod(_math, "POWER")
    __rpow__ = partialmethod(_math, "POWER", reflected=True)

    @property
    def key(self) -> str:
        return blake2b(msgspec.msgpack.encode(self, enc_hook=lambda kind: kind.__name__), digest_size=8).hexdigest()


class Read(Expr, frozen=True, tag=True):
    kind: type[bpy.types.Node]

    def terms(self) -> Iterator[Term]:
        yield self

    def made(self, tree: bpy.types.NodeTree, _nodes: Mapping[str, bpy.types.Node]) -> bpy.types.Node:
        return tree.nodes.new(self.kind.__name__)


class Math(Expr, frozen=True, tag=True):
    operation: "rna_enums.NodeMathItems"
    operands: tuple[Expr | float, ...]

    def terms(self) -> Iterator[Term]:
        for operand in self.operands:
            if isinstance(operand, Read | Math):
                yield from operand.terms()
        yield self

    def made(self, tree: bpy.types.NodeTree, nodes: Mapping[str, bpy.types.Node]) -> bpy.types.Node:
        node: bpy.types.ShaderNodeMath = tree.nodes.new(bpy.types.ShaderNodeMath.__name__)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        node.operation = self.operation
        inputs: tuple[bpy.types.NodeSocketFloat, ...] = tuple(socket for socket in node.inputs if socket.enabled)  # type: ignore[union-attr, misc]  # ty: ignore[not-iterable, invalid-assignment]
        for socket, operand in zip(inputs, self.operands, strict=True):
            if isinstance(operand, Read | Math):
                tree.links.new(nodes[operand.key].outputs[0], socket)
            elif isinstance(operand, float | int):
                socket.default_value = operand
        return node


def lowered(tree: bpy.types.NodeTree, roots: Mapping[bpy.types.NodeSocket, Term], mark: str) -> tuple[str, ...]:
    desired: dict[str, Term] = {expr.key: expr for root in roots.values() for expr in root.terms()}
    marked: tuple[tuple[str, bpy.types.Node], ...] = tuple((key, node) for node in tree.nodes if isinstance(key := node.get(mark), str))
    nodes: dict[str, bpy.types.Node] = dict(reversed(marked))
    for key, node in marked:
        if key not in desired or nodes[key] != node:
            tree.nodes.remove(node)
    nodes = {key: node for key, node in nodes.items() if key in desired}
    added: tuple[str, ...] = tuple(key for key in desired if key not in nodes)
    for key in added:
        nodes[key] = desired[key].made(tree, nodes)
        nodes[key][mark] = key
    for target, root in roots.items():
        tree.links.new(nodes[root.key].outputs[0], target)
    return added
```

- `msgspec.msgpack.encode` walks the whole record in one call, `enc_hook` encoding node classes by name, and the tag keeps cases with equal fields apart
- `terms` yields operands before their consumer, every operand node existing when its consumer links
- Edits rekey the edited node and every node downstream of it, upstream nodes keeping their nodes and links
- Second lowerings of an unchanged expression create, remove, and link nothing past the root links
- Nodes code edits or reads later take a label key in place of the digest, and nodes no input reads join through an explicit keep list
