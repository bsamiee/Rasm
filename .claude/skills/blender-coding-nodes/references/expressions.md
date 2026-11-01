# [EXPRESSIONS]

## [01]-[ALGEBRA]

Each node is a frozen record of class, operands over enabled inputs, mode settings, and the enabled output its consumer reads. Python operators build Math records, and each key digests class, settings, and every operand's key and output:

```python
if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import NodeMathItems

type Operation = NodeMathItems
type Value = bool | int | float | str | tuple[float, ...] | bpy.types.ID
type Operand = Expr | Value | None


class Expr(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    kind: type[bpy.types.Node]
    inputs: tuple[Operand, ...] = ()
    settings: tuple[tuple[str, Value], ...] = ()
    output: int = 0

    def _math(self, operation: Operation, *others: Operand, reflected: bool = False) -> Self:
        return type(self)(kind=bpy.types.ShaderNodeMath, inputs=(*others, self) if reflected else (self, *others), settings=(("operation", operation),))

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
    __neg__ = partialmethod(_math, "MULTIPLY", -1.0)
    __abs__ = partialmethod(_math, "ABSOLUTE")
    __floor__ = partialmethod(_math, "FLOOR")
    __ceil__ = partialmethod(_math, "CEIL")

    @cached_property
    def key(self) -> str:
        operands = tuple((operand.key, operand.output) if isinstance(operand, Expr) else operand for operand in self.inputs)
        return blake2b(msgspec.msgpack.encode((self.kind.__name__, operands, self.settings), enc_hook=lambda block: block.name_full), digest_size=8).hexdigest()
```

`position * 2.0 + Expr(kind=bpy.types.ShaderNodeCombineXYZ, inputs=(None, None, abs(height) + 1))` over `position = Expr(kind=bpy.types.GeometryNodeInputPosition)` and `height = Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(position,), output=2)` is one graph value:
- `kind.__name__` is the `bl_idname` `nodes.new` takes, stock classes reading `bl_idname` as `None` at class level
- `inputs` and `output` count sockets `settings` enables: Math `ABSOLUTE` takes one operand, Vector Math `DOT_PRODUCT` output 0 is `Value`
- `None` operands keep an input's default (`inputs=(coordinates, None, 4.0)`)
- Operators bind operations Math and Vector Math both hold, Vector Math lacking `FLOORED_MODULO`, `TRUNC`, and the comparisons
- `-expr` multiplies by -1, and `abs`, `math.floor`, and `math.ceil` dispatch to `__abs__`, `__floor__`, and `__ceil__`
- Keys chain through operand keys, digesting each shared subexpression once, and equal records anywhere in a graph share one node
- Keys leave out a record's own `output`, Separate XYZ read at `output=0` and `output=2` staying one node
- `enc_hook` encodes ID settings and constants as `name_full`, keeping keys stable across sessions where `session_uid` changes per load
- `cached_property` holds the key under `dict=True`, equality and hashing staying by fields
- Group nodes are records with `settings=(("node_tree", group),)`, bodies lowering with Group Output sockets as roots
- `Expr(kind=bpy.types.NodeGroupInput, output=n)` reads group input `n` inside a body
- Geometry, shader, and compositor trees take Math, Vector Math, Mix, and Separate XYZ, and texture trees take `TextureNode*` classes alone

## [02]-[LOWERING]

Lowering reuses the node each key marks, creates the rest, links what `tree.links` lacks, and removes marked nodes the graph no longer reaches:

```python
def lowered(tree: bpy.types.NodeTree, roots: Mapping[bpy.types.NodeSocket, Expr], /, *, mark: str = Expr.__name__) -> tuple[bpy.types.Node, ...]:
    held: dict[str, bpy.types.Node] = {key: node for node in tuple(tree.nodes)[::-1] if isinstance(key := node.get(mark), str)}
    linked = frozenset((link.from_socket, link.to_socket) for link in tree.links)
    reached: dict[str, bpy.types.Node] = {}
    wanted: list[tuple[bpy.types.NodeSocket, bpy.types.NodeSocket]] = []

    def placed(expr: Expr) -> bpy.types.NodeSocket:
        if (key := expr.key) not in reached:
            operands = tuple(placed(operand) if isinstance(operand, Expr) else operand for operand in expr.inputs)
            vector = expr.kind is bpy.types.ShaderNodeMath and any(
                operand.type in {"VECTOR", "RGBA"} if isinstance(operand, bpy.types.NodeSocket) else isinstance(operand, tuple) for operand in operands
            )
            node = reached[key] = held.get(key) or tree.nodes.new((bpy.types.ShaderNodeVectorMath if vector else expr.kind).__name__)
            if fresh := key not in held:
                node[mark] = key
                for member, value in expr.settings:
                    setattr(node, member, value)
            for socket, operand in zip((socket for socket in node.inputs if socket.enabled), operands, strict=False):
                if isinstance(operand, bpy.types.NodeSocket):
                    wanted.append((operand, socket))
                elif fresh and operand is not None:
                    socket.default_value = np.broadcast_to(operand, np.shape(socket.default_value)).tolist()  # type: ignore[attr-defined, arg-type]  # ty: ignore[unresolved-attribute, no-matching-overload]
        return tuple(socket for socket in reached[key].outputs if socket.enabled)[expr.output]

    wanted.extend([(placed(expr), target) for target, expr in roots.items()])
    for source, target in wanted:
        if (source, target) not in linked:
            tree.links.new(source, target)
    for node in tuple(tree.nodes):
        if isinstance(key := node.get(mark), str) and reached.get(key) != node:
            tree.nodes.remove(node)
    return tuple(node for key, node in reached.items() if key not in held)
```

`lowered(tree, {set_position.inputs["Position"]: graph})` returns the nodes it created for a layout to place:
- Math records lower to Vector Math once an operand is a vector or color output or a tuple constant
- Scalar writes to vector `default_value` raise `TypeError`
- `np.broadcast_to(...).tolist()` fills every vector component and alpha, and passes scalars, strings, and IDs through
- Settings and constants reach created nodes alone, the key standing for them on a held node
- `links.new` on a linked pair tags the tree, and lowering compares links against one `tree.links` pass
- Sockets hash and compare equal across wrapper rebuilds and key the `linked` set
- Repeat lowerings of an unchanged graph write nothing and fire no depsgraph update
- Edits rekey the edited record and every record downstream, upstream nodes keeping their nodes, links, and positions
- Removal compares each marked node against the one its key reached, dropping stale nodes and duplicate holders in one pass and leaving unmarked nodes
- `mark` names the owner, and graphs sharing one tree take a mark each
- Refused classes raise `RuntimeError` at `nodes.new` and refused settings and constants `TypeError`, uncaught as a defect in the graph
