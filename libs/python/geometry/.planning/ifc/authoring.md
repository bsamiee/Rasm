# [PY_GEOMETRY_IFC_AUTHORING]

IFC model mutation as a transactional verb script — write side the analysis and lifecycle hops read against. `IfcAuthor` dispatches the `ifcopenshell.api.<module>.<action>(file, **kwargs)` usecase callables over one `IfcApiVerb` row table that IS the authoring vocabulary: a verb name keys a dotted `module.action` usecase plus one composed `Capability` flag, its arguments ride a typed `AuthorPayload` case, and one `apply` entry folds a whole op tuple under a single `begin_transaction`/`end_transaction`. Widening the surface is one table row plus one payload case, never a parallel create/edit/assign method family over the usecase set `ifcopenshell` already dispatches.

Each script is an immutable left-fold over a frozen `AuthorCarry` short-circuiting on the first `Error`; transaction and provenance concerns are AOP decorators over the one `_run` rail — `@transactional` fences rollback across both a rail fault and a provider exception, `@stamped` projects owner-history provenance onto the receipt — never re-derived per verb body. `AuthorReceipt` is itself the `ReceiptContributor`, so the owner holds no mutable handle. Across the `IfcWire` seam this mutation receipt is what the C# `Rasm.Bim` `IfcSemanticModel` re-projection reads — Authoring mutates the model and emits evidence, never graduating a `GeometrySubject`.

## [01]-[INDEX]

- [01]-[AUTHORING]: the `IfcApiVerb`-table authoring surface — the closed verb vocabulary, the typed `AuthorPayload` union, the `AuthorCarry` transaction fold under the `@transactional`/`@stamped` aspects, and the `AuthorReceipt`/`MutationFact` receipt that is itself the `ReceiptContributor`.

## [02]-[AUTHORING]

- Owner: `IfcAuthor` — boundary capsule over one `apply` entry; the usecase callable resolved from the table row is the single polymorphic dispatch, never a method per usecase, and accumulation state lives in the fold, not the owner.
- Cases: `AuthorVerb` rows ARE the vocabulary, each an `IfcApiVerb` row binding a usecase to its composed `Capability`. `OWNER_NEW`/`OWNER_UPD` is a change-history pair the fold runs through the slot map — `OWNER_NEW` mints the history into its `op.slot`, `OWNER_UPD`'s `stamp` payload resolves it back into the `update_owner_history` kwargs — never a decorator side-channel or a first-vs-subsequent runtime branch. `UNIT_ASSIGN` reuses the `relate` payload over a bespoke case: its coincident `("units", "units")` row folds the relating value into the single `assign_unit(units=...)` keyword. `AuthorPayload` cases collapse verbs by argument shape, not one per verb — `define` folds `name` away to span the name-taking and name-free verbs — and slot resolution is `Result`-native: an absent slot is `Error(BoundaryFault)` on the rail, never a `KeyError` three ops deep, so the fold short-circuits already typed at the boundary. `_keyed` re-keys the canonical `products`/`relating` pair to the row's true per-usecase spelling (`relating_structure`/`relating_object`/`relating_type`), which DIFFERS per usecase and stays table-driven, never one generic keyword.
- Entry: `apply` takes an `ifcopenshell.file` and an `AuthorOp` tuple, returning `RuntimeRail[AuthorReceipt]` through one `boundary` admission — a provider exception converts to a `BoundaryFault` once at the seam, an unresolved-slot fault arrives already typed on the rail, two fault sources meeting on one carrier. A relating verb consumes prior slots through its `relate` payload, so a build-a-wall-in-a-storey script is one ordered op list, never manual id-chaining.
- Law: authoring runs on the caller floor by charter — the live `ifcopenshell.file` is the companion's in-process resource, a pybind11 handle no pickle seam carries, and a transactional mutation script is not idempotent: it earns no lane crossing, and any future kernel wrapping a mutating script declares `idempotent=False` so a worker-death retry never re-applies a half-committed mutation the rollback fence cannot see.
- Auto: `@transactional` — a clean `Ok` closes the batch, a typed `Error` rail OR a raised op runs `undo()` before `end_transaction()` so a half-applied script never persists, and the aspect projects transaction depth onto the receipt via `replace`, never the `len(facts)` op count. `@stamped` — a field-named `replace` sets the provenance flag from whether the script carried an `OWNER_NEW`/`OWNER_UPD` op. This owner applies only these two; receipt emission is the consumer's aspect, never inlined here. Per-op kernel stacks four `ifcopenshell` reads into one `MutationFact` — the usecase mutation, `guid.compress` on the minted GlobalId, the `get_psets` key count, the `traverse` subtree count — and `REMOVE` reads `get_inverse` BEFORE the delete because the usecase severs the entity and the inverse-reference count is unreadable after.
- Receipt: `AuthorReceipt` implements `contribute` structurally (against the `@runtime_checkable` protocol, never subclassing it) and carries the same `MutationFact` block as the typed return, so evidence is structured field-by-field, not a free-form log. `edited` and `depth` are distinct evidence — `edited` counts non-minting mutations against existing entities, `depth` is the transaction nesting the `@transactional` aspect projects — never the same `len(facts)` written twice.
- Packages: `ifcopenshell` (the `api.<module>.<action>` usecase callables, the `begin_transaction`/`undo`/`end_transaction` stack, the `guid.compress` codec, and the `get_psets`/`get_inverse`/`traverse` read graph the mutation footprint joins against), runtime (`RuntimeRail`/`boundary`/`BoundaryFault`, `railed` the bound `effect.result` builder, `Receipt`/`ReceiptContributor`), `expression` (`tagged_union`/`case`/`tag`, the `Result` rails and `identity`, `Map` for the slot vocabulary, `Block` for the fact stream), `msgspec` (`Struct`, `structs.replace`), stdlib `enum` (`Flag` for `Capability`, `StrEnum` for `AuthorVerb`).
- Growth: a new authoring capability is one `AuthorVerb` row plus one `IfcApiVerb` table row plus reuse of the matching `AuthorPayload` case; a new capability dimension is one `Capability` member the `in row.cap` tests pick up without a new column; a new argument shape is one `AuthorPayload` case with its `to_kwargs` projection. Zero new dispatcher, no per-usecase method.
- Boundary: `ifcopenshell` owns entity construction, usecase dispatch, the transaction stack, the GUID codec, and the inverse/traverse graph — no hand-rolled STEP writer, no local UUID/GlobalId fold, no `guid.expand` on the write side (the minted GlobalId is already compressed; `expand` is the read-side codec). No durable store and no Rhino/GH mutation. Relating verbs consume in-script slots and never re-query the model by string GUID when the minting op is in the same script.

```python signature
import functools
import importlib
from collections.abc import Callable, Generator, Iterable
from enum import Flag, StrEnum, auto
from typing import TYPE_CHECKING, assert_never

from expression import Error, Ok, case, identity, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace

from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, railed
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # worker-only: every runtime `ifcopenshell` reference rides a function-local `import ifcopenshell.<sub>  # noqa: PLC0415`, so the module loads clean under the boundary-scope import policy
    import ifcopenshell

# --- [TYPES] ---------------------------------------------------------------------------


class AuthorVerb(StrEnum):
    CREATE = "create"
    REMOVE = "remove"
    COPY = "copy"
    EDIT = "edit"
    REPRESENT = "represent"
    PLACE = "place"
    CONTEXT = "context"
    UNIT_SI = "unit-si"
    UNIT_ASSIGN = "unit-assign"
    PSET = "pset"
    CONTAIN = "contain"
    AGGREGATE = "aggregate"
    MATERIAL = "material"
    TYPE = "type"
    OWNER_NEW = "owner-new"
    OWNER_UPD = "owner-upd"


class Capability(Flag):
    # Composable usecase-return caps in one column — `MINTS|READS` composes, so aspects test
    # `Capability.MINTS in row.cap`, never parallel bool columns.
    NONE = 0
    MINTS = auto()  # new entity; its GlobalId keys the receipt
    RELATES = auto()  # relationship instance, not a minted root
    READS = auto()  # size the pset/inverse footprint after the mutation


# --- [MODELS] --------------------------------------------------------------------------


@tagged_union(frozen=True)
class AuthorPayload:
    """Verb arguments collapsed into families by argument shape; `to_kwargs` resolves slots into the usecase `**kwargs`."""

    tag: str = tag()
    spawn: tuple[str, str | None, str | None] = case()  # ifc_class, predefined_type, name
    target: str = case()  # source slot
    edit: tuple[str, dict[str, object]] = case()  # slot, attributes
    attach: tuple[str, str] = case()  # product slot, context slot
    define: tuple[str | None, str, dict[str, object]] = case()  # optional product slot (PSET), name, scalar args
    relate: tuple[tuple[str, ...], str] = case()  # product slots, relating slot
    stamp: tuple[str | None] = case()  # existing OwnerHistory slot

    @railed
    def to_kwargs(self, slots: "Map[str, object]") -> "Generator[RuntimeRail[object], object, dict[str, object]]":
        # `railed` `effect.result` builder threads each `yield from bound(slot)` slot-resolve.
        def bound(slot: str) -> "RuntimeRail[object]":
            return Ok(slots[slot]) if slot in slots else Error(BoundaryFault(resource=("ifc.authoring.slot", slot)))

        match self:
            case AuthorPayload(tag="spawn", spawn=(cls, pre, name)):
                return {"ifc_class": cls} | ({"predefined_type": pre} if pre else {}) | ({"name": name} if name else {})
            case AuthorPayload(tag="target", target=slot):
                return {"product": (yield from bound(slot))}
            case AuthorPayload(tag="edit", edit=(slot, attrs)):
                return {"product": (yield from bound(slot)), "attributes": attrs}
            case AuthorPayload(tag="attach", attach=(prod, ctx)):
                return {"product": (yield from bound(prod)), "context": (yield from bound(ctx))}
            case AuthorPayload(tag="define", define=(None, name, args)):
                # `name` folds away when empty — one case spans the name-taking and name-free verbs.
                return ({"name": name} if name else {}) | args
            case AuthorPayload(tag="define", define=(slot, name, args)):
                return {"product": (yield from bound(slot))} | ({"name": name} if name else {}) | args
            case AuthorPayload(tag="relate", relate=(prods, rel)):
                # `yield from` is illegal in a comprehension, so product slots bind in an explicit loop.
                resolved: list[object] = []
                for slot in prods:
                    resolved.append((yield from bound(slot)))
                return {"products": tuple(resolved), "relating": (yield from bound(rel))}
            case AuthorPayload(tag="stamp", stamp=(existing,)):
                return {} if existing is None else {"owner_history": (yield from bound(existing))}
            case _ as unreachable:
                assert_never(unreachable)


class IfcApiVerb(Struct, frozen=True):
    usecase: str  # dotted module.action resolved to ifcopenshell.api.<module>.<action>
    cap: Capability  # composable capability set the aspects key on
    # (products, relating) keyword pair; `("", "")` for non-relating verbs where the re-key never fires
    relating_kw: tuple[str, str] = ("", "")


class AuthorOp(Struct, frozen=True):
    verb: AuthorVerb
    payload: AuthorPayload
    slot: str = ""  # name the product binds into for downstream ops


class MutationFact(Struct, frozen=True):
    verb: AuthorVerb
    guid: str  # compressed GlobalId of the minted product, "" for non-minting verbs
    psets: int  # get_psets key count on the touched product
    subtree: int  # traverse dependent-entity count the mutation reached


class AuthorCarry(Struct, frozen=True):
    slots: "Map[str, object]" = Map.of_seq(())  # OWNER_NEW mints history here under op.slot; OWNER_UPD's stamp resolves it back
    facts: "Block[MutationFact]" = Block.empty()
    edited: int = 0  # running count of non-minting mutations


class AuthorReceipt(Struct, frozen=True):
    schema: str
    facts: "Block[MutationFact]"
    guids: tuple[str, ...]
    edited: int  # non-minting mutations against existing entities
    depth: int = 0  # transaction nesting the @transactional aspect projects via replace
    stamped: bool = False  # set by the @stamped provenance aspect: did the script open an owner history

    def contribute(self) -> "Iterable[Receipt]":
        yield Receipt.of(
            "rasm.geometry.ifc.authoring",
            (
                "emitted",
                "mutation",
                {
                    "schema": self.schema,
                    "verbs": ",".join(f.verb for f in self.facts),
                    "guids": ",".join(self.guids),
                    "psets": sum(f.psets for f in self.facts),
                    "subtree": sum(f.subtree for f in self.facts),
                    "edited": self.edited,
                    "depth": self.depth,
                    "stamped": self.stamped,
                },
            ),
        )


# --- [TABLES] --------------------------------------------------------------------------

# closed authoring vocabulary, one row per verb; keyword spellings are decompile-confirmed and DIFFER per usecase.
IFC_API_VERBS: dict[AuthorVerb, IfcApiVerb] = {
    AuthorVerb.CREATE: IfcApiVerb("root.create_entity", Capability.MINTS),
    AuthorVerb.REMOVE: IfcApiVerb("root.remove_product", Capability.READS),
    AuthorVerb.COPY: IfcApiVerb("root.copy_class", Capability.MINTS),
    AuthorVerb.EDIT: IfcApiVerb("attribute.edit_attributes", Capability.NONE),
    AuthorVerb.REPRESENT: IfcApiVerb("geometry.add_representation", Capability.MINTS),
    AuthorVerb.PLACE: IfcApiVerb("geometry.edit_object_placement", Capability.NONE),
    AuthorVerb.CONTEXT: IfcApiVerb("context.add_context", Capability.MINTS),
    AuthorVerb.UNIT_SI: IfcApiVerb("unit.add_si_unit", Capability.MINTS),
    AuthorVerb.UNIT_ASSIGN: IfcApiVerb("unit.assign_unit", Capability.RELATES, ("units", "units")),
    AuthorVerb.PSET: IfcApiVerb("pset.add_pset", Capability.MINTS | Capability.READS),
    AuthorVerb.CONTAIN: IfcApiVerb("spatial.assign_container", Capability.RELATES, ("products", "relating_structure")),
    AuthorVerb.AGGREGATE: IfcApiVerb("aggregate.assign_object", Capability.RELATES, ("products", "relating_object")),
    AuthorVerb.MATERIAL: IfcApiVerb("material.add_material", Capability.MINTS),
    AuthorVerb.TYPE: IfcApiVerb("type.assign_type", Capability.RELATES, ("related_objects", "relating_type")),
    AuthorVerb.OWNER_NEW: IfcApiVerb("owner.create_owner_history", Capability.MINTS),
    AuthorVerb.OWNER_UPD: IfcApiVerb("owner.update_owner_history", Capability.NONE),
}

# --- [OPERATIONS] ----------------------------------------------------------------------


@functools.cache
def _usecase(dotted: str) -> "Callable[..., object]":
    module, action = dotted.rsplit(".", 1)
    return getattr(importlib.import_module(f"ifcopenshell.api.{module}"), action)


def _keyed(row: IfcApiVerb, kwargs: dict[str, object]) -> dict[str, object]:
    # Re-keys `products`/`relating` to the row's pair; coincident keywords (`assign_unit`) fold into one, never a collision.
    if Capability.RELATES not in row.cap:
        return kwargs
    products_kw, relating_kw = row.relating_kw
    rest = {k: v for k, v in kwargs.items() if k not in ("products", "relating")}
    relating = {relating_kw: kwargs["relating"]} if relating_kw != products_kw else {}
    return rest | {products_kw: kwargs["products"]} | relating


type _Run = Callable[["IfcAuthor", "ifcopenshell.file", tuple[AuthorOp, ...]], "RuntimeRail[AuthorReceipt]"]


def _transactional(run: _Run) -> _Run:
    """Rollback aspect: a clean `Ok` commits; an `Error` rail or a raised op runs `undo()` before closing."""

    @functools.wraps(run)
    def wrapped(self: "IfcAuthor", model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        model.begin_transaction()
        try:
            rail = run(self, model, script)
        except Exception:
            model.undo()
            model.end_transaction()  # no commit= arg; rollback is undo() before the close
            raise
        if rail.is_error():  # `Ok`/`Error` are both `Result`, so discriminate by method, never isinstance
            model.undo()
        model.end_transaction()
        return rail.map(lambda r: replace(r, depth=r.depth + 1))

    return wrapped


def _stamped(run: _Run) -> _Run:
    """Provenance projection: sets whether the script opened an owner-history root, never a control branch."""

    @functools.wraps(run)
    def wrapped(self: "IfcAuthor", model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        stamped = any(op.verb in (AuthorVerb.OWNER_NEW, AuthorVerb.OWNER_UPD) for op in script)
        return run(self, model, script).map(lambda r: replace(r, stamped=stamped))

    return wrapped


class IfcAuthor:
    def apply(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        return boundary("rasm.geometry.ifc.authoring", lambda: self._run(model, script)).bind(identity)

    @_stamped
    @_transactional
    def _run(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        folded: RuntimeRail[AuthorCarry] = functools.reduce(
            lambda acc, op: acc.bind(lambda carry: self._step(model, carry, op)), script, Ok(AuthorCarry())
        )
        return folded.map(lambda carry: AuthorReceipt(model.schema, carry.facts, tuple(f.guid for f in carry.facts if f.guid), carry.edited))

    def _step(self, model: "ifcopenshell.file", carry: AuthorCarry, op: AuthorOp) -> "RuntimeRail[AuthorCarry]":
        row = IFC_API_VERBS[op.verb]

        def fire(kwargs: dict[str, object]) -> AuthorCarry:
            # REMOVE severs the entity, so the inverse fan-out is read from the input product BEFORE the delete.
            severed = len(model.get_inverse(kwargs["product"])) if op.verb is AuthorVerb.REMOVE else 0
            return self._record(model, carry, op, row, _usecase(row.usecase)(model, **_keyed(row, kwargs)), severed)

        return op.payload.to_kwargs(carry.slots).map(fire)

    @staticmethod
    def _record(model: "ifcopenshell.file", carry: AuthorCarry, op: AuthorOp, row: IfcApiVerb, product: "object", severed: int) -> AuthorCarry:
        import ifcopenshell.guid  # noqa: PLC0415
        import ifcopenshell.util.element  # noqa: PLC0415

        mints = Capability.MINTS in row.cap
        is_entity = isinstance(product, ifcopenshell.entity_instance)
        guid = ifcopenshell.guid.compress(product.GlobalId) if mints and is_entity and product.GlobalId else ""
        psets = len(ifcopenshell.util.element.get_psets(product)) if Capability.READS in row.cap and is_entity else 0
        subtree = severed if op.verb is AuthorVerb.REMOVE else (len(model.traverse(product)) if is_entity else 0)
        return AuthorCarry(
            slots=carry.slots.add(op.slot, product) if op.slot and is_entity else carry.slots,
            facts=carry.facts.append(Block.singleton(MutationFact(op.verb, guid, psets, subtree))),
            edited=carry.edited + (0 if mints else 1),
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
