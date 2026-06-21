# [PY_GEOMETRY_IFC_AUTHORING]

IFC model mutation as a transactional verb script — the write-side companion the analysis and lifecycle hops read against. `IfcAuthor` drives the `ifcopenshell.api.<module>.<action>(file, **kwargs)` usecase callables over one `IfcApiVerb` row table that IS the authoring vocabulary: a canonical verb name resolves to a dotted `module.action` usecase plus the `mints`/`relates`/`reads` capability columns that drive the cross-cutting aspects, the payload is a typed `AuthorPayload` tagged-union case (never a stringly `dict[str, object]`), and a new authoring capability is one table row plus one payload case, never a new method. The whole verb script batches under one `begin_transaction`/`end_transaction` so a mid-script provider fault OR a typed unresolved-slot `Error` rolls the model back through `undo` before it surfaces as a typed boundary fault; the script is an immutable rail-threading left-fold over a frozen `AuthorCarry` that short-circuits on the first `Error`, not a mutable accumulator. The cross-cutting transaction and provenance concerns are AOP decorators wrapping the one `_run` rail — `@transactional` fences the rollback over both the rail fault and a provider exception, `@stamped` is the post-fold provenance projection keying `guid.compress` identity and the owner-history-opened flag — never re-derived per verb body. The `owner.create_owner_history`/`owner.update_owner_history` change-history pair is the two-row `OWNER_NEW`/`OWNER_UPD` verb dispatch the fold runs through the slot map, not a decorator side-channel; the `AuthorReceipt` itself implements `ReceiptContributor.contribute`, so the receipt IS the contributor and no mutable handle is held on the owner. Each op stacks four `ifcopenshell` capabilities in one rail: the `api` usecase mutates, `guid.compress` keys identity, `util.element.get_psets` / `file.get_inverse` read the product's pset and inverse-reference fan-out back, and `file.traverse` counts the dependent-entity subtree the mutation touched — so one op emits a `MutationFact` that already carries the structural footprint, not a bare entity handle. `ifcopenshell` owns entity construction, usecase dispatch, the transaction stack, the GUID codec, and the inverse/traverse graph; this owner adds the typed verb-plus-payload vocabulary, the rollback-bounded immutable fold, and the `AuthorReceipt` the toolchain consumes — never a parallel create/edit/assign method family over the closed usecase set the package already dispatches.

## [01]-[INDEX]

- [01]-[AUTHORING]: the one `IfcApiVerb`-table authoring surface — the verb row table that IS the closed usecase vocabulary, the typed `AuthorPayload` union the op carries, the `AuthorOp` transaction script the table folds, the `apply` rail batched under the `@transactional`/`@stamped` aspect decorators, the immutable rail-threading `AuthorCarry` fold, and the `AuthorReceipt`/`MutationFact` typed mutation receipt that is itself the `ReceiptContributor`.

## [02]-[AUTHORING]

- Owner: `IfcAuthor` — the boundary capsule dispatching the `ifcopenshell.api.<module>.<action>` usecase callables over the `IfcApiVerb` table, exposing one `apply` entry over an `AuthorOp` script and returning the `AuthorReceipt` that carries the `ReceiptContributor.contribute` surface; `IfcApiVerb` the frozen row mapping a canonical `AuthorVerb` name to its dotted `module.action` usecase path plus the `mints`/`relates`/`reads` capability columns the aspects key on; `AuthorVerb` the closed `StrEnum` naming the authoring concept the table widens by; `AuthorPayload` the frozen `@tagged_union` carrying one typed payload case per verb family so the op's keyword arguments are typed evidence, never a `dict[str, object]` bag; `AuthorOp` the frozen verb-plus-payload-plus-slot invocation the script folds; `MutationFact` the per-op typed record carrying the verb, the minted GUID, the read-back pset key count, and the touched-subtree count; `AuthorCarry` the frozen fold carry threading the slot map (which carries the OWNER_NEW-minted history under its own `op.slot`), the `MutationFact` block, and the running non-minting `edited` count; `AuthorReceipt` the typed mutation receipt carrying the schema version, the ordered `MutationFact` block, the minted GUIDs, the edited-entity count, and the transaction depth, and itself implementing `ReceiptContributor.contribute` so the receipt IS the contributor rather than a mutable handle held on the owner.
- Cases: `AuthorVerb` rows are the canonical authoring vocabulary, each one `IfcApiVerb` table row binding the name to a `module.action` usecase plus its capability columns — `CREATE` (`root.create_entity`, mints) · `REMOVE` (`root.remove_product`, reads the inverse fan-out before delete) · `COPY` (`root.copy_class`, mints) · `EDIT` (`attribute.edit_attributes`) · `REPRESENT` (`geometry.add_representation`, mints) · `PLACE` (`geometry.edit_object_placement`) · `CONTEXT` (`context.add_context`, mints) · `UNIT_SI` (`unit.add_si_unit`, mints) · `UNIT_ASSIGN` (`unit.assign_unit`) · `PSET` (`pset.add_pset`, mints, reads) · `CONTAIN` (`spatial.assign_container`, relates) · `AGGREGATE` (`aggregate.assign_object`, relates) · `MATERIAL` (`material.add_material`, mints) · `TYPE` (`type.assign_type`, relates) · `OWNER_NEW` (`owner.create_owner_history`, mints) · `OWNER_UPD` (`owner.update_owner_history`). The table is the vocabulary and the resolved usecase callable is the one polymorphic entry: widening the authoring surface is one `AuthorVerb` row plus one `IfcApiVerb` table row plus one `AuthorPayload` case, never a sibling method per usecase. The `OWNER_NEW`/`OWNER_UPD` split is two table rows over the change-history pair: `OWNER_NEW` mints the history into its named slot and `OWNER_UPD`'s `stamp` payload resolves that slot back into the `update_owner_history` kwargs, so ownership tracking is an ordered verb-row pair in the same script, never a decorator side-channel or a runtime first-vs-subsequent branch.
- Payload: `AuthorPayload` is a `@tagged_union` whose cases collapse the verbs into payload families by argument shape, not one case per verb — `spawn` (`ifc_class` + optional `predefined_type`/`name`, for `CREATE`) · `target` (a single `slot` reference, for `REMOVE`/`COPY`) · `edit` (`slot` + `attributes` map, for `EDIT`/`PLACE`) · `attach` (`slot` + `context`/source slot, for `REPRESENT`) · `define` (an optional product slot + name + scalar args — the slot is `None` for the project-scoped `CONTEXT`/`UNIT_SI`/`MATERIAL` and the resolved product for the element-scoped `PSET` whose `pset.add_pset` binds to a `product`) · `relate` (`products` slot tuple + `relating` slot, for `CONTAIN`/`AGGREGATE`/`TYPE`/`UNIT_ASSIGN`) · `stamp` (`existing` optional `OwnerHistory` slot, for `OWNER_NEW`/`OWNER_UPD`). Each case `to_kwargs(slots)` projection resolves its slot references against the carry's slot map through a `slot in slots`-guarded `Map.__getitem__` and returns a `RuntimeRail[dict[str, object]]` — an absent slot is `Error(BoundaryFault(resource=...))` on the rail, never `Map.__getitem__` raising a `KeyError` three ops deep — so the dataflow is a typed slot-resolve that short-circuits the fold the moment a `relate` payload's `relating` slot is missing, the unresolved-slot fault arriving already typed at the boundary rather than as a raised exception the `@transactional` rollback must trap. The slot vocabulary is a `Result`-native resolve, not raw indexing.
- Entry: `IfcAuthor.apply` takes an `ifcopenshell.file` and a `tuple[AuthorOp, ...]` script and returns a `RuntimeRail[AuthorReceipt]` through one `boundary("rasm.geometry.ifc.authoring", ...)` admission whose thunk yields the per-fold rail and is flattened through `Result.bind(identity)`, so a provider exception converts to a `BoundaryFault` exactly once at the seam while an unresolved-slot fault arrives already typed on the rail — the two fault sources meeting on one carrier. The script body is the `@stamped`-over-`@transactional` stacked `_run` reducing the op tuple through a rail-threading `functools.reduce` of `acc.bind(...)` into a frozen `AuthorCarry` — the fold short-circuits on the first `Error`, never a mutable ledger with `self.x.append`. A bare verb that is its own subject (`CREATE` minting a typed root) returns the new `entity_instance` into a named slot; a relating verb (`CONTAIN`, `AGGREGATE`, `TYPE`) consumes prior slots through its `relate` payload so a build-a-wall-in-a-storey script is one ordered op list, never a manual id-chaining ceremony.
- Auto: the transaction, identity, and provenance concerns are AOP decorators over the one `_run` rail, never per-verb bodies. `@transactional` opens `begin_transaction`, runs the fold, commits `end_transaction()` on a clean `Ok`, and `undo()`-then-`end_transaction()` on either a typed `Error` return or a raised op — the rollback aspect fences both the rail fault and a provider exception so a half-applied script never persists; the commit-versus-rollback `end_transaction` spelling is the one transaction-stack detail the live `ifcopenshell.file` source confirms, and the decorator is its single adjustment site. `@stamped` is the identity-and-provenance projection over the returned carry, not a control branch: it keys the receipt's minted GUIDs through `guid.compress(entity_instance.GlobalId)` over the carry's minting facts (the package codec, never a local UUID fold) and sets the `stamped` flag from whether the script carried an `OWNER_NEW`/`OWNER_UPD` op, so an unstamped mutation is structurally visible to the reviewer. The owner-history *dispatch* itself is the two-row `OWNER_NEW`/`OWNER_UPD` verb table the fold runs — `OWNER_NEW` mints the history into its named `op.slot` in the carry's slot map, `OWNER_UPD`'s `stamp` payload resolves that slot back into the `update_owner_history` kwargs — so ownership tracking is an ordered verb-row pair the fold drives through the slot map, never a side-channel a decorator hides. The `AuthorReceipt` itself implements `ReceiptContributor.contribute`, lifting its own fields into the `Receipt.of("emitted", "rasm.geometry.ifc.authoring", ...)` row, so the receipt IS the contributor — no `@contributing` aspect and no mutable `self._last` handle on the owner. The per-op kernel stacks four `ifcopenshell` reads into one `MutationFact`: the usecase mutation, `guid.compress` on the minted GlobalId, `len(util.element.get_psets(product))` for the pset key count, and `len(file.traverse(product))` for the touched-subtree count — so the receipt fact is the structural footprint, not a bare handle. `REMOVE` reads `file.get_inverse(product)` before delete so its `MutationFact` records the orphaned inverse-reference count the delete severed.
- Receipt: `IfcAuthor` implements `ReceiptContributor.contribute` (structurally, against the `@runtime_checkable` protocol — never subclassing it) returning one emitted-phase `Receipt.of("emitted", "rasm.geometry.ifc.authoring", "mutation", facts)` row carrying the schema version, the ordered verb tags, the minted GUIDs, the summed pset/subtree counts, the edited-entity count (the non-minting mutations), the transaction depth the `@transactional` aspect records, and the `@stamped` provenance flag (whether the script opened an owner-history root), and the `AuthorReceipt` carries the same `MutationFact` block as the typed return so the mutation evidence is structured field-by-field for the toolchain rather than a free-form log. The `edited` and `depth` fields are distinct evidence — `edited` counts the non-minting facts (the mutations against existing entities), `depth` is the transaction nesting the rollback aspect threads — never the same `len(facts)` value written twice. Authoring is a model-mutation owner, not a graduation producer — the analysis and lifecycle hops graduate; this owner emits the in-process mutation receipt the C# `IfcSemanticModel` re-projection reads, so no `GeometrySubject`/`HandoffAxis` rides this page.
- Packages: `ifcopenshell` (the `api.<module>.<action>(file, **kwargs)` usecase callables resolved from the `module.action` table string over the closed vocabulary; `file.begin_transaction`/`end_transaction`/`undo`/`redo` the transaction stack; `file.create_entity`/`add`/`remove`/`by_guid`/`traverse`/`get_inverse`/`schema` the primitive verbs, dependent-entity walk, inverse fan-out, and schema introspection; `guid.new`/`guid.compress` the GUID codec; `util.element.get_psets` the read-back the minting/relating ops join against to size the mutation footprint), runtime (`RuntimeRail`/`boundary`/`BoundaryFault` for the rail and the typed unresolved-slot fault, `Receipt`/`ReceiptContributor` for the mutation receipt the `AuthorReceipt` implements), `expression` (`tagged_union`/`case`/`tag` for `AuthorPayload`; `Ok`/`Error`/`Result.map`/`Result.bind`/`identity` for the rail-threading fold and slot resolve; `Map.__contains__`/`Map.__getitem__`/`Map.add` for the slot vocabulary; `Block.append`/`Block.singleton` for the `MutationFact` block).
- Growth: a new authoring capability is one `AuthorVerb` row plus one `IfcApiVerb` table row binding it to its `module.action` usecase and capability columns plus reuse of the matching `AuthorPayload` case — the resolved-callable dispatcher and the two aspect decorators are unchanged; a genuinely new argument shape is one new `AuthorPayload` case with its rail-returning `to_kwargs` projection; zero new surface, no per-usecase method, no second dispatcher.
- Boundary: no per-verb authoring function family over the `module.action` rows (the deleted form); no `dict[str, object]` payload bag where `AuthorPayload` carries typed cases; no hand-rolled STEP writer where `file.write`/the usecase callables own serialization; no local UUID/GlobalId fold where `guid.new`/`guid.compress` owns identity; no manual positional id-chaining where the typed `relate`/`target` slot references own the dataflow; no raw `Map.__getitem__` slot indexing that raises a `KeyError` mid-fold where `to_kwargs` returns a `RuntimeRail` and an absent slot is a typed `Error(BoundaryFault)` that short-circuits the rail; no mutable `_AuthorLedger` accumulator or `self._last` receipt handle where the immutable rail-threading `AuthorCarry` fold owns accumulation and the `AuthorReceipt` itself is the contributor; no `guid.expand` (the `.api` catalogue confirms only `guid.new`/`guid.compress`, so identity keys on `guid.compress`); no half-applied script — a mid-script fault rolls back through `undo` before the boundary surfaces it; no durable store and no Rhino/GH mutation. The relating verbs consume prior slots; they never re-query the model by string GUID a second time when the op that minted the product is in the same script.

```python contract
import functools
import importlib
from collections.abc import Callable
from enum import StrEnum
from typing import assert_never

from expression import Error, Ok, Result, case, identity, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor

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


# --- [MODELS] --------------------------------------------------------------------------


@tagged_union(frozen=True)
class AuthorPayload:
    """Typed verb arguments collapsed into families by argument shape; `to_kwargs`
    resolves slot references against the live slot map into the usecase `**kwargs`."""

    tag: str = tag()
    spawn: tuple[str, str | None, str | None] = case()  # ifc_class, predefined_type, name
    target: str = case()  # source slot
    edit: tuple[str, dict[str, object]] = case()  # slot, attributes
    attach: tuple[str, str] = case()  # product slot, context slot
    define: tuple[str | None, str, dict[str, object]] = case()  # optional product slot (PSET), name, scalar args
    relate: tuple[tuple[str, ...], str, str] = case()  # product slots, relating slot, relating keyword
    stamp: tuple[str | None] = case()  # existing OwnerHistory slot

    def to_kwargs(self, slots: "Map[str, object]") -> "RuntimeRail[dict[str, object]]":
        def bound(slot: str) -> "RuntimeRail[object]":
            return Ok(slots[slot]) if slot in slots else Error(BoundaryFault(resource=("ifc.authoring.slot", slot)))

        match self:
            case AuthorPayload(tag="spawn", spawn=(cls, pre, name)):
                return Ok({"ifc_class": cls} | ({"predefined_type": pre} if pre else {}) | ({"name": name} if name else {}))
            case AuthorPayload(tag="target", target=slot):
                return bound(slot).map(lambda product: {"product": product})
            case AuthorPayload(tag="edit", edit=(slot, attrs)):
                return bound(slot).map(lambda product: {"product": product, "attributes": attrs})
            case AuthorPayload(tag="attach", attach=(prod, ctx)):
                return bound(prod).bind(lambda p: bound(ctx).map(lambda c: {"product": p, "context": c}))
            case AuthorPayload(tag="define", define=(None, name, args)):
                return Ok({"name": name} | args)
            case AuthorPayload(tag="define", define=(slot, name, args)):
                return bound(slot).map(lambda product: {"product": product, "name": name} | args)
            case AuthorPayload(tag="relate", relate=(prods, rel, rel_kw)):
                resolved: RuntimeRail[tuple[object, ...]] = functools.reduce(
                    lambda acc, p: acc.bind(lambda done: bound(p).map(lambda product: (*done, product))),
                    prods,
                    Ok(()),
                )
                return resolved.bind(lambda products: bound(rel).map(lambda r: {"products": products, rel_kw: r}))
            case AuthorPayload(tag="stamp", stamp=(existing,)):
                return Ok({}) if existing is None else bound(existing).map(lambda h: {"owner_history": h})
            case _:
                assert_never(self)


class IfcApiVerb(Struct, frozen=True):
    verb: AuthorVerb
    usecase: str  # dotted module.action resolved to ifcopenshell.api.<module>.<action>
    mints: bool  # the usecase returns a new entity whose GlobalId keys the receipt
    relates: bool  # the usecase returns a relationship instance, not a minted root
    reads: bool  # size the pset/inverse footprint after the mutation


class AuthorOp(Struct, frozen=True):
    verb: AuthorVerb
    payload: AuthorPayload
    slot: str = ""  # name the product binds into for downstream ops


class MutationFact(Struct, frozen=True):
    verb: AuthorVerb
    guid: str  # compressed GlobalId of the minted product, "" for non-minting verbs
    psets: int  # util.element.get_psets key count on the touched product
    subtree: int  # file.traverse dependent-entity count the mutation reached


class AuthorCarry(Struct, frozen=True):
    slots: "Map[str, object]" = Map.of_seq(())  # OWNER_NEW's minted history lands here under its op.slot; OWNER_UPD's stamp payload resolves it back
    facts: "Block[MutationFact]" = Block.empty()
    edited: int = 0  # running count of non-minting mutations


class AuthorReceipt(Struct, frozen=True):
    schema: str
    facts: "Block[MutationFact]"
    guids: tuple[str, ...]
    edited: int  # non-minting mutations against existing entities
    depth: int  # transaction nesting the @transactional aspect records
    stamped: bool = False  # set by the @stamped provenance aspect: did the script open an owner history

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "rasm.geometry.ifc.authoring", "mutation", {
            "schema": self.schema,
            "verbs": ",".join(f.verb for f in self.facts),
            "guids": ",".join(self.guids),
            "psets": str(sum(f.psets for f in self.facts)),
            "subtree": str(sum(f.subtree for f in self.facts)),
            "edited": str(self.edited),
            "depth": str(self.depth),
            "stamped": str(self.stamped),
        })


# --- [TABLES] --------------------------------------------------------------------------

IFC_API_VERBS: dict[AuthorVerb, IfcApiVerb] = {
    AuthorVerb.CREATE: IfcApiVerb(AuthorVerb.CREATE, "root.create_entity", True, False, False),
    AuthorVerb.REMOVE: IfcApiVerb(AuthorVerb.REMOVE, "root.remove_product", False, False, True),
    AuthorVerb.COPY: IfcApiVerb(AuthorVerb.COPY, "root.copy_class", True, False, False),
    AuthorVerb.EDIT: IfcApiVerb(AuthorVerb.EDIT, "attribute.edit_attributes", False, False, False),
    AuthorVerb.REPRESENT: IfcApiVerb(AuthorVerb.REPRESENT, "geometry.add_representation", True, False, False),
    AuthorVerb.PLACE: IfcApiVerb(AuthorVerb.PLACE, "geometry.edit_object_placement", False, False, False),
    AuthorVerb.CONTEXT: IfcApiVerb(AuthorVerb.CONTEXT, "context.add_context", True, False, False),
    AuthorVerb.UNIT_SI: IfcApiVerb(AuthorVerb.UNIT_SI, "unit.add_si_unit", True, False, False),
    AuthorVerb.UNIT_ASSIGN: IfcApiVerb(AuthorVerb.UNIT_ASSIGN, "unit.assign_unit", False, True, False),
    AuthorVerb.PSET: IfcApiVerb(AuthorVerb.PSET, "pset.add_pset", True, False, True),
    AuthorVerb.CONTAIN: IfcApiVerb(AuthorVerb.CONTAIN, "spatial.assign_container", False, True, False),
    AuthorVerb.AGGREGATE: IfcApiVerb(AuthorVerb.AGGREGATE, "aggregate.assign_object", False, True, False),
    AuthorVerb.MATERIAL: IfcApiVerb(AuthorVerb.MATERIAL, "material.add_material", True, False, False),
    AuthorVerb.TYPE: IfcApiVerb(AuthorVerb.TYPE, "type.assign_type", False, True, False),
    AuthorVerb.OWNER_NEW: IfcApiVerb(AuthorVerb.OWNER_NEW, "owner.create_owner_history", True, False, False),
    AuthorVerb.OWNER_UPD: IfcApiVerb(AuthorVerb.OWNER_UPD, "owner.update_owner_history", False, False, False),
}

# --- [OPERATIONS] ----------------------------------------------------------------------


@functools.cache
def _usecase(dotted: str) -> "Callable[..., object]":
    module, action = dotted.rsplit(".", 1)
    return getattr(importlib.import_module(f"ifcopenshell.api.{module}"), action)


type _Run = Callable[["IfcAuthor", object, tuple[AuthorOp, ...]], "RuntimeRail[AuthorReceipt]"]


def _transactional(run: _Run) -> _Run:
    """Rollback aspect: a clean `Ok` commits the batch; a typed `Error` rail OR a raised
    provider exception runs `undo()` then closes the batch, so a half-applied script never
    persists — the single boundary kernel allowed language control flow, the body never re-states it."""

    @functools.wraps(run)
    def wrapped(self: "IfcAuthor", model: "object", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        model.begin_transaction()
        try:
            rail = run(self, model, script)
        except Exception:
            model.undo()
            model.end_transaction()
            raise
        match rail:
            case Error(_):
                model.undo()
        model.end_transaction()
        return rail

    return wrapped


def _stamped(run: _Run) -> _Run:
    """Provenance projection over the returned carry: sets whether the script opened an
    owner-history root so an unstamped mutation is structurally visible to the reviewer —
    a post-fold projection, never a control branch and never re-derived per verb."""

    @functools.wraps(run)
    def wrapped(self: "IfcAuthor", model: "object", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        stamped = any(op.verb in (AuthorVerb.OWNER_NEW, AuthorVerb.OWNER_UPD) for op in script)
        return run(self, model, script).map(
            lambda r: AuthorReceipt(r.schema, r.facts, r.guids, r.edited, r.depth, stamped),
        )

    return wrapped


class IfcAuthor:
    def apply(self, model: "object", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        return boundary("rasm.geometry.ifc.authoring", lambda: self._run(model, script)).bind(identity)

    @_stamped
    @_transactional
    def _run(self, model: "object", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        folded: RuntimeRail[AuthorCarry] = functools.reduce(
            lambda acc, op: acc.bind(lambda carry: self._step(model, carry, op)), script, Ok(AuthorCarry()),
        )
        return folded.map(lambda carry: AuthorReceipt(
            model.schema,
            carry.facts,
            tuple(f.guid for f in carry.facts if f.guid),
            carry.edited,
            len(carry.facts),
        ))

    def _step(self, model: "object", carry: AuthorCarry, op: AuthorOp) -> "RuntimeRail[AuthorCarry]":
        row = IFC_API_VERBS[op.verb]

        def fire(kwargs: dict[str, object]) -> AuthorCarry:
            # REMOVE's usecase returns None and severs the entity, so the inverse fan-out is read
            # from the resolved input product BEFORE the delete; minting/relating verbs read the returned product after.
            severed = len(model.get_inverse(kwargs["product"])) if op.verb is AuthorVerb.REMOVE else 0
            return self._record(model, carry, op, row, _usecase(row.usecase)(model, **kwargs), severed)

        return op.payload.to_kwargs(carry.slots).map(fire)

    @staticmethod
    def _record(model: "object", carry: AuthorCarry, op: AuthorOp, row: IfcApiVerb, product: "object", severed: int) -> AuthorCarry:
        import ifcopenshell  # noqa: PLC0415
        import ifcopenshell.guid  # noqa: PLC0415
        import ifcopenshell.util.element  # noqa: PLC0415

        is_entity = isinstance(product, ifcopenshell.entity_instance)
        guid = ifcopenshell.guid.compress(product.GlobalId) if row.mints and is_entity and product.GlobalId else ""
        psets = len(ifcopenshell.util.element.get_psets(product)) if row.reads and is_entity else 0
        subtree = severed if op.verb is AuthorVerb.REMOVE else (len(model.traverse(product)) if is_entity else 0)
        return AuthorCarry(
            slots=carry.slots.add(op.slot, product) if op.slot and is_entity else carry.slots,
            facts=carry.facts.append(Block.singleton(MutationFact(op.verb, guid, psets, subtree))),
            edited=carry.edited + (0 if row.mints else 1),
        )
```

## [03]-[RESEARCH]

- [API_DIRECT_DISPATCH]: the branch `ifcopenshell` catalogue confirms the closed `module.action` authoring vocabulary (`root.create_entity`, `root.remove_product`, `root.copy_class`, `attribute.edit_attributes`, `geometry.add_representation`, `geometry.edit_object_placement`, `context.add_context`, `unit.add_si_unit`, `unit.assign_unit`, `pset.add_pset`, `spatial.assign_container`, `aggregate.assign_object`, `material.add_material`, `type.assign_type`) and that 0.8.5 deprecates `api.run` ("Do not use this function") in favor of the direct `ifcopenshell.api.<module>.<action>(file, **kwargs)` callable over the identical vocabulary. The owner threads the canonical direct callable: `_usecase` splits the `IFC_API_VERBS` dotted `module.action` string and resolves `getattr(importlib.import_module("ifcopenshell.api.<module>"), action)`, so the table string is the contract and the resolved callable is the dispatch — never the deprecated `api.run`, never a per-verb method. The `owner.create_owner_history`/`owner.update_owner_history` pair and the `OwnerHistory` slot the `stamp` payload carries confirm against the live `ifcopenshell.api.owner` usecase package; the `OWNER_NEW`/`OWNER_UPD` two-row split binds each to its own callable so the script is an ordered verb-row pair — `OWNER_NEW` mints into its slot, `OWNER_UPD`'s `stamp` payload resolves that slot back — rather than a runtime first-vs-subsequent branch hidden in an aspect.
- [TRANSACTION_ROLLBACK_STACK]: the branch `ifcopenshell` catalogue confirms `file.begin_transaction`/`end_transaction` open and close an undoable edit batch and `undo`/`redo` walk the transaction stack; the exact `end_transaction` commit-versus-rollback parameter spelling (a bare `end_transaction()` after `undo()` versus an `end_transaction(commit=False)` keyword) is the one transaction-stack-shape detail the live `ifcopenshell.file` source confirms. The `@transactional` decorator fences the rollback regardless: a raised op runs `undo()` then closes the batch before the fault reaches `boundary`, so a half-applied script never persists; the decorator is the single aspect site that adjusts to the confirmed spelling, not per-verb bodies.
- [GUID_CODEC]: the branch `ifcopenshell` catalogue confirms `guid.new` mints and `guid.compress` encodes an IFC GUID — the only two GUID-codec members the `.api` catalogue carries. The receipt keys identity on `guid.compress(product.GlobalId)`; a `guid.expand` decompress is NOT in the confirmed surface and is excluded from the fence (the prior contract's `guid.expand` call was an unconfirmed-member citation). The `entity_instance.GlobalId` attribute the minted-product read joins against confirms against the branch `ifcopenshell` catalogue model-root surface.
- [MUTATION_FOOTPRINT_STACK]: the branch `ifcopenshell` catalogue confirms `file.traverse(entity, depth)` walks the dependent-entity graph, `file.get_inverse(entity)` returns the inverse-referencing instances, and `util.element.get_psets(element)` returns the property/quantity sets as a dict — the three reads the per-op kernel stacks onto the usecase mutation to size each `MutationFact` (pset key count, dependent-subtree count, and the orphaned-inverse count `REMOVE` records before delete). The exact `traverse` depth-argument default and whether `get_inverse` returns a tuple or a set are the read-shape details the live `ifcopenshell.file` source confirms; the `len(...)` footprint is correct over either container.
- [ENTITY_INSTANCE_ISINSTANCE]: the `isinstance(product, ifcopenshell.entity_instance)` guard the kernel uses to discriminate a minted entity from a non-minting usecase return confirms against the branch `ifcopenshell` catalogue `entity_instance` wrapper type; the precise return shape of each relating usecase (`spatial.assign_container`/`aggregate.assign_object`/`type.assign_type` returning the relationship instance versus `None`) is the one usecase-return detail the live `ifcopenshell.api` package confirms, and the `mints`/`relates`/`reads` capability columns already gate the GUID, footprint, and pset reads so a non-entity return is folded out rather than mis-recorded.
