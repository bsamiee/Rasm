# [PY_GEOMETRY_IFC_AUTHORING]

IFC model mutation as a transactional verb script — the write-side companion the analysis and lifecycle hops read against. `IfcAuthor` drives the `ifcopenshell.api.<module>.<action>(file, **kwargs)` usecase callables over one `IfcApiVerb` row table that IS the authoring vocabulary: a canonical verb name keys a dotted `module.action` usecase plus one composed `Capability` flag (`MINTS | RELATES | READS`) that drives the cross-cutting aspects, the payload is a typed `AuthorPayload` tagged-union case (never a stringly `dict[str, object]`), and a new authoring capability is one table row plus one payload case, never a new method. The whole verb script batches under one `begin_transaction`/`end_transaction` so a mid-script provider fault OR a typed unresolved-slot `Error` rolls the model back through `undo` before it surfaces as a typed boundary fault; the script is an immutable rail-threading left-fold over a frozen `AuthorCarry` that short-circuits on the first `Error`, not a mutable accumulator. The cross-cutting transaction and provenance concerns are AOP decorators wrapping the one `_run` rail — `@transactional` fences the rollback over both the rail fault and a provider exception, `@stamped` is the post-fold provenance projection keying `guid.compress` identity and the owner-history-opened flag — never re-derived per verb body. The `owner.create_owner_history`/`owner.update_owner_history` change-history pair is the two-row `OWNER_NEW`/`OWNER_UPD` verb dispatch the fold runs through the slot map, not a decorator side-channel; the `AuthorReceipt` itself implements `ReceiptContributor.contribute`, so the receipt IS the contributor and no mutable handle is held on the owner. Each op stacks four `ifcopenshell` capabilities in one rail: the `api` usecase mutates, `guid.compress` keys identity, `util.element.get_psets` / `file.get_inverse` read the product's pset and inverse-reference fan-out back, and `file.traverse` counts the dependent-entity subtree the mutation touched — so one op emits a `MutationFact` that already carries the structural footprint, not a bare entity handle. `ifcopenshell` owns entity construction, usecase dispatch, the transaction stack, the GUID codec, and the inverse/traverse graph; this owner adds the typed verb-plus-payload vocabulary, the rollback-bounded immutable fold, and the `AuthorReceipt` the toolchain consumes — never a parallel create/edit/assign method family over the closed usecase set the package already dispatches.

## [01]-[INDEX]

- [01]-[AUTHORING]: the one `IfcApiVerb`-table authoring surface — the verb row table that IS the closed usecase vocabulary, the typed `AuthorPayload` union the op carries, the `AuthorOp` transaction script the table folds, the `apply` rail batched under the `@transactional`/`@stamped` aspect decorators, the immutable rail-threading `AuthorCarry` fold, and the `AuthorReceipt`/`MutationFact` typed mutation receipt that is itself the `ReceiptContributor`.

## [02]-[AUTHORING]

- Owner: `IfcAuthor` — the boundary capsule dispatching the `ifcopenshell.api.<module>.<action>` usecase callables over the `IfcApiVerb` table, exposing one `apply` entry over an `AuthorOp` script and returning the `AuthorReceipt` that carries the `ReceiptContributor.contribute` surface; `IfcApiVerb` the frozen row binding a dotted `module.action` usecase path to one composed `Capability` flag and the per-usecase relating keyword pair (the verb is the dict key, never restated in the row); `Capability` the `enum.Flag` collapsing the orthogonal mint/relate/read capability set into one composable column the aspects test through `Capability.MINTS in row.cap`; `AuthorVerb` the closed `StrEnum` naming the authoring concept the table widens by; `AuthorPayload` the frozen `@tagged_union` carrying one typed payload case per verb family so the op's keyword arguments are typed evidence, never a `dict[str, object]` bag; `AuthorOp` the frozen verb-plus-payload-plus-slot invocation the script folds; `MutationFact` the per-op typed record carrying the verb, the minted GUID, the read-back pset key count, and the touched-subtree count; `AuthorCarry` the frozen fold carry threading the slot map (which carries the OWNER_NEW-minted history under its own `op.slot`), the `MutationFact` block, and the running non-minting `edited` count; `AuthorReceipt` the typed mutation receipt carrying the schema version, the ordered `MutationFact` block, the minted GUIDs, the edited-entity count, and the transaction depth, and itself implementing `ReceiptContributor.contribute` so the receipt IS the contributor rather than a mutable handle held on the owner.
- Cases: `AuthorVerb` rows are the canonical authoring vocabulary, each one `IfcApiVerb` table row binding the usecase to its composed `Capability` flag — `CREATE` (`root.create_entity`, `MINTS`) · `REMOVE` (`root.remove_product`, `READS` the inverse fan-out before delete) · `COPY` (`root.copy_class`, `MINTS`) · `EDIT` (`attribute.edit_attributes`, `NONE`) · `REPRESENT` (`geometry.add_representation`, `MINTS`) · `PLACE` (`geometry.edit_object_placement`, `NONE`) · `CONTEXT` (`context.add_context`, `MINTS`) · `UNIT_SI` (`unit.add_si_unit`, `MINTS`) · `UNIT_ASSIGN` (`unit.assign_unit`, `RELATES`) · `PSET` (`pset.add_pset`, `MINTS | READS`) · `CONTAIN` (`spatial.assign_container`, `RELATES`) · `AGGREGATE` (`aggregate.assign_object`, `RELATES`) · `MATERIAL` (`material.add_material`, `MINTS`) · `TYPE` (`type.assign_type`, `RELATES`) · `OWNER_NEW` (`owner.create_owner_history`, `MINTS`) · `OWNER_UPD` (`owner.update_owner_history`, `NONE`). The table is the vocabulary and the resolved usecase callable is the one polymorphic entry: widening the authoring surface is one `AuthorVerb` row plus one `IfcApiVerb` table row plus one `AuthorPayload` case, never a sibling method per usecase. The `OWNER_NEW`/`OWNER_UPD` split is two table rows over the change-history pair: `OWNER_NEW` mints the history into its named slot and `OWNER_UPD`'s `stamp` payload resolves that slot back into the `update_owner_history` kwargs, so ownership tracking is an ordered verb-row pair in the same script, never a decorator side-channel or a runtime first-vs-subsequent branch. `UNIT_ASSIGN` rides the same `relate` payload as the spatial verbs — its `products` slot carries the unit tuple and its `relating` slot the `IfcProject` target, which `_keyed` folds away under the coincident `("units", "units")` row so `assign_unit(units=...)` receives one keyword, the single relationship payload covering both arities rather than a bespoke unit-assign case.
- Payload: `AuthorPayload` is a `@tagged_union` whose cases collapse the verbs into payload families by argument shape, not one case per verb — `spawn` (`ifc_class` + optional `predefined_type`/`name`, for `CREATE`) · `target` (a single `slot` reference, for `REMOVE`/`COPY`) · `edit` (`slot` + `attributes` map, for `EDIT`/`PLACE`) · `attach` (`slot` + `context`/source slot, for `REPRESENT`) · `define` (an optional product slot + an optional `name` + scalar args — the slot is `None` for the project-scoped `CONTEXT`/`UNIT_SI`/`MATERIAL` and the resolved product for the element-scoped `PSET` whose `pset.add_pset` binds to a `product`; `name` folds away when empty so the one case covers both the `name`-taking verbs (`material.add_material`/`pset.add_pset`) and the `name`-free verbs (`context.add_context`/`unit.add_si_unit`) whose scalars ride `args`) · `relate` (`products` slot tuple + `relating` slot, for `CONTAIN`/`AGGREGATE`/`TYPE`/`UNIT_ASSIGN`) · `stamp` (`existing` optional `OwnerHistory` slot, for `OWNER_NEW`/`OWNER_UPD`). Each case is a `@railed` `effect.result` generator (`to_kwargs`): `yield from bound(slot)` resolves each slot reference against the carry's slot map through a `slot in slots`-guarded `Map.__getitem__` and the builder returns a `RuntimeRail[dict[str, object]]` — an absent slot is `Error(BoundaryFault(resource=...))` on the rail, never `Map.__getitem__` raising a `KeyError` three ops deep — so the dataflow is a typed interleaved-bind slot-resolve that short-circuits the fold the moment a `relate` payload's `relating` slot is missing, the unresolved-slot fault arriving already typed at the boundary rather than as a raised exception the `@transactional` rollback must trap. The `relate` case binds canonical `products`/`relating` keys; the per-usecase keyword spelling lives on the `IfcApiVerb.relating_kw` column and `_keyed(row, kwargs)` re-keys the pair to the row's true `relating_structure`/`relating_object`/`relating_type`/`units` name before dispatch, folding the relating value away when both row keywords coincide (`assign_unit`'s `("units", "units")`), so the keyword stays table-driven and never carried per payload. The slot vocabulary is a `Result`-native resolve, not raw indexing.
- Entry: `IfcAuthor.apply` takes an `ifcopenshell.file` and a `tuple[AuthorOp, ...]` script and returns a `RuntimeRail[AuthorReceipt]` through one `boundary("rasm.geometry.ifc.authoring", ...)` admission whose thunk yields the per-fold rail and is flattened through `Result.bind(identity)`, so a provider exception converts to a `BoundaryFault` exactly once at the seam while an unresolved-slot fault arrives already typed on the rail — the two fault sources meeting on one carrier. The script body is the `@stamped`-over-`@transactional` stacked `_run` reducing the op tuple through a rail-threading `functools.reduce` of `acc.bind(...)` into a frozen `AuthorCarry` — the fold short-circuits on the first `Error`, never a mutable ledger with `self.x.append`. A bare verb that is its own subject (`CREATE` minting a typed root) returns the new `entity_instance` into a named slot; a relating verb (`CONTAIN`, `AGGREGATE`, `TYPE`) consumes prior slots through its `relate` payload so a build-a-wall-in-a-storey script is one ordered op list, never a manual id-chaining ceremony.
- Auto: the transaction, identity, and provenance concerns are AOP decorators over the one `_run` rail, never per-verb bodies. `@transactional` opens `begin_transaction`, runs the fold, closes `end_transaction()` on a clean `Ok`, and runs `undo()`-then-`end_transaction()` on either a typed `Error` return or a raised op — the rollback aspect fences both the rail fault and a provider exception so a half-applied script never persists, and the `end_transaction()` carries no `commit=` arg (rollback is `undo()` before the close, the live `file` contract), the decorator the single site that spells it; the same aspect projects the batch's transaction depth onto the receipt through `msgspec.structs.replace(r, depth=r.depth + 1)`, so `depth` is the rollback aspect's own evidence rather than the `len(facts)` op count `_run` once duplicated. `@stamped` is the identity-and-provenance projection over the returned carry, not a control branch: it sets the `stamped` flag through `msgspec.structs.replace(r, stamped=...)` (a field-named copy robust to receipt-field reorder, never a positional rebuild) from whether the script carried an `OWNER_NEW`/`OWNER_UPD` op, so an unstamped mutation is structurally visible to the reviewer; the minted GUIDs are keyed by `guid.compress` inside the `_record` kernel (the package codec, never a local UUID fold), so the projection adds only the provenance flag. The owner-history *dispatch* itself is the two-row `OWNER_NEW`/`OWNER_UPD` verb table the fold runs — `OWNER_NEW` mints the history into its named `op.slot` in the carry's slot map, `OWNER_UPD`'s `stamp` payload resolves that slot back into the `update_owner_history` kwargs — so ownership tracking is an ordered verb-row pair the fold drives through the slot map, never a side-channel a decorator hides. The `AuthorReceipt` itself implements `ReceiptContributor.contribute`, yielding its own fields as the runtime `Receipt.of("rasm.geometry.ifc.authoring", ("emitted", "mutation", facts))` stream the runtime `@receipted` aspect harvests at the consuming boundary (this page applies only `_transactional`/`_stamped`; emission is the consumer's aspect, never inlined here), so the receipt IS the contributor — no inline `emit` and no mutable `self._last` handle on the owner. The per-op kernel stacks four `ifcopenshell` reads into one `MutationFact`: the usecase mutation, `guid.compress` on the minted GlobalId, `len(util.element.get_psets(product))` for the pset key count, and `len(file.traverse(product))` for the touched-subtree count — so the receipt fact is the structural footprint, not a bare handle. `REMOVE` reads `file.get_inverse(product)` before delete so its `MutationFact` records the orphaned inverse-reference count the delete severed.
- Receipt: `AuthorReceipt` implements `ReceiptContributor.contribute` (structurally, against the `@runtime_checkable` protocol — never subclassing it), yielding the runtime `Iterable[Receipt]` stream the runtime `@receipted` aspect harvests at the consuming boundary rather than returning one forced `Receipt`, the single emitted-phase row minted through `Receipt.of("rasm.geometry.ifc.authoring", ("emitted", "mutation", facts))` — the runtime `Receipt.of(owner, evidence)` two-arg contract, the `(phase, subject, facts)` evidence triple the factory discriminates, never a four-positional call. The facts carry the schema version, the ordered verb tags, the minted GUIDs, the summed pset/subtree counts, the edited-entity count (the non-minting mutations), the transaction depth the `@transactional` aspect records, and the `@stamped` provenance flag (whether the script opened an owner-history root) as native `int`/`bool` scalars the runtime `EventDict` (`dict[str, object]`) carries through its `enc_hook=repr` renderer, never a pre-`str()`-coerced `dict[str, str]` the renderer is built to avoid; the `AuthorReceipt` carries the same `MutationFact` block as the typed return so the mutation evidence is structured field-by-field for the toolchain rather than a free-form log. The `edited` and `depth` fields are distinct evidence — `edited` counts the non-minting facts (the mutations against existing entities), `depth` is the transaction nesting the rollback aspect projects via `replace` over the returned receipt — never the same `len(facts)` value written twice. Authoring is a model-mutation owner, not a graduation producer — the analysis and lifecycle hops graduate; this owner emits the in-process mutation receipt the C# `IfcSemanticModel` re-projection reads, so no `GeometrySubject`/`HandoffAxis` rides this page.
- Packages: `ifcopenshell` (the `api.<module>.<action>(file, **kwargs)` usecase callables resolved from the `module.action` table string over the closed vocabulary; `file.begin_transaction`/`end_transaction()` — no `commit=` arg — `undo`/`redo`/`discard_transaction` the transaction stack; `file.create_entity`/`add`/`remove`/`by_guid`/`traverse(inst, max_levels=None)`/`get_inverse`/`schema` the primitive verbs, dependent-entity walk, inverse fan-out, and schema introspection; `guid.new`/`guid.compress` the write-side GUID codec; `util.element.get_psets` the read-back the minting/relating ops join against to size the mutation footprint), runtime (`RuntimeRail`/`boundary`/`BoundaryFault` for the rail and the typed unresolved-slot fault, `railed` the bound `effect.result` builder threading the `to_kwargs` slot-resolve, `Receipt.of(owner, evidence)`/`ReceiptContributor.contribute` returning the `Iterable[Receipt]` stream the runtime `@receipted` aspect harvests and emits at the consuming boundary — the native-scalar `EventDict` the mutation facts ride), `expression` (`tagged_union`/`case`/`tag` for `AuthorPayload`; `Ok`/`Error`/`Result.map`/`Result.bind`/`identity` for the rail-threading fold and slot resolve; `Map.__contains__`/`Map.__getitem__`/`Map.add` for the slot vocabulary; `Block.append`/`Block.singleton` for the `MutationFact` block), `msgspec` (`Struct` for the row/op/carry/receipt records, `structs.replace` the `@stamped` field-named provenance copy), stdlib `enum` (`Flag`/`auto` for the composable `Capability` column, `StrEnum` for `AuthorVerb`).
- Growth: a new authoring capability is one `AuthorVerb` row plus one `IfcApiVerb` table row binding it to its `module.action` usecase and its composed `Capability` flag plus reuse of the matching `AuthorPayload` case — the resolved-callable dispatcher and the two aspect decorators are unchanged; a genuinely new capability dimension is one `Capability` member the existing `in row.cap` tests pick up without a new column; a genuinely new argument shape is one new `AuthorPayload` case with its rail-returning `to_kwargs` projection; zero new surface, no per-usecase method, no second dispatcher.
- Boundary: no per-verb authoring function family over the `module.action` rows (the deleted form); no `dict[str, object]` payload bag where `AuthorPayload` carries typed cases; no hand-rolled STEP writer where `file.write`/the usecase callables own serialization; no local UUID/GlobalId fold where `guid.new`/`guid.compress` owns identity; no manual positional id-chaining where the typed `relate`/`target` slot references own the dataflow; no single generic `relating=`/`products=` keyword threaded into the relating usecases where the per-usecase spelling DIFFERS (`relating_structure`/`relating_object`/`relating_type`) and `_keyed` re-keys the canonical pair off the row's `relating_kw` column; no raw `Map.__getitem__` slot indexing that raises a `KeyError` mid-fold where `to_kwargs` returns a `RuntimeRail` and an absent slot is a typed `Error(BoundaryFault)` that short-circuits the rail; no mutable `_AuthorLedger` accumulator or `self._last` receipt handle where the immutable rail-threading `AuthorCarry` fold owns accumulation and the `AuthorReceipt` itself is the contributor; no `guid.expand` decompress on the write side where the minted GlobalId is already compressed and `expand` is the read-side codec; no `end_transaction(commit=...)` keyword where the live `file` carries no `commit=` arg and rollback is `undo()`/`discard_transaction()` before the close; no half-applied script — a mid-script fault rolls back through `undo` before the boundary surfaces it; no durable store and no Rhino/GH mutation. The relating verbs consume prior slots; they never re-query the model by string GUID a second time when the op that minted the product is in the same script.

```python contract
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
from rasm.runtime.receipts import Receipt, ReceiptContributor

if TYPE_CHECKING:  # worker: the usecase callables resolve via `importlib.import_module("ifcopenshell.api.<module>")` and every runtime `ifcopenshell` reference rides a function-local `import ifcopenshell.<sub>  # noqa: PLC0415` that binds the name, so the runtime module loads clean and the boundary-scope import policy holds (the selector sibling shape)
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
    # Orthogonal usecase-return capability set folded into one table column; `MINTS|READS`
    # composes (PSET both mints a pset and sizes the read-back), so the aspects test
    # `Capability.MINTS in row.cap` rather than three parallel `bool` columns racing.
    NONE = 0
    MINTS = auto()  # usecase returns a new entity whose GlobalId keys the receipt
    RELATES = auto()  # usecase returns a relationship instance, not a minted root
    READS = auto()  # size the pset/inverse footprint after the mutation


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
    relate: tuple[tuple[str, ...], str] = case()  # product slots, relating slot
    stamp: tuple[str | None] = case()  # existing OwnerHistory slot

    @railed
    def to_kwargs(self, slots: "Map[str, object]") -> "Generator[RuntimeRail[object], object, dict[str, object]]":
        # The `railed` `effect.result` builder threads each `yield from bound(slot)` slot-resolve
        # and short-circuits the whole projection on the first absent-slot `Error` — the canonical
        # interleaved-bind rail the runtime owns, never a hand-rolled `functools.reduce(acc.bind...)`.
        # Canonical relating keys (`products`/`relating`) bind here; `_keyed(row, kwargs)` re-keys
        # them to the verb row's true `relating_kw` pair before dispatch, so the per-usecase keyword
        # stays table-driven, never carried per payload.
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
                # `name` folds away when empty: `context.add_context`/`unit.add_si_unit` take no
                # `name`, while `material.add_material`/`pset.add_pset` do — one case spans both.
                return ({"name": name} if name else {}) | args
            case AuthorPayload(tag="define", define=(slot, name, args)):
                return {"product": (yield from bound(slot))} | ({"name": name} if name else {}) | args
            case AuthorPayload(tag="relate", relate=(prods, rel)):
                # `yield from` is illegal inside a comprehension, so each product slot binds in an
                # explicit loop accumulator — the same first-absent short-circuit, legal in a `for` body.
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
    cap: Capability  # the composable mint/relate/read capability set the aspects key on
    # the per-usecase relating/products keyword, table-driven because it DIFFERS per usecase
    # (relating_structure | relating_object | relating_type | related_objects); "" for the
    # non-relating verbs whose `relate` payload re-key never fires
    relating_kw: tuple[str, str] = ("", "")  # (products-keyword, relating-keyword)


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
    depth: int = 0  # transaction nesting the @transactional aspect projects via replace, never the fact count
    stamped: bool = False  # set by the @stamped provenance aspect: did the script open an owner history

    def contribute(self) -> "Iterable[Receipt]":
        # Native scalars ride the runtime `EventDict` (`dict[str, object]`) whose `enc_hook=repr`
        # renderer serializes them without a `str()` coerce; only the `verbs`/`guids` joins are strings.
        yield Receipt.of("rasm.geometry.ifc.authoring", ("emitted", "mutation", {
            "schema": self.schema,
            "verbs": ",".join(f.verb for f in self.facts),
            "guids": ",".join(self.guids),
            "psets": sum(f.psets for f in self.facts),
            "subtree": sum(f.subtree for f in self.facts),
            "edited": self.edited,
            "depth": self.depth,
            "stamped": self.stamped,
        }))


# --- [TABLES] --------------------------------------------------------------------------

# the closed authoring vocabulary: one row per verb binding its dotted usecase, the composed
# Capability flag the aspects key on, and the per-usecase (products, relating) keyword pair
# the relating verbs re-key `to_kwargs` to. The verb is the dict key, never restated in the row.
# Keyword spellings are decompile-confirmed per usecase — they DIFFER per row, never one generic name.
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
    # The `relate` payload binds canonical `products`/`relating` keys; a relating verb re-keys
    # them to its true per-usecase keyword pair (`relating_structure`/`relating_object`/
    # `relating_type` | `units`) off the row, so the per-usecase keyword stays table-driven and
    # never carried per payload. When both row keywords coincide (`assign_unit` -> `("units",
    # "units")`) the relating value folds into the single keyword rather than colliding.
    if Capability.RELATES not in row.cap:
        return kwargs
    products_kw, relating_kw = row.relating_kw
    rest = {k: v for k, v in kwargs.items() if k not in ("products", "relating")}
    relating = {relating_kw: kwargs["relating"]} if relating_kw != products_kw else {}
    return rest | {products_kw: kwargs["products"]} | relating


type _Run = Callable[["IfcAuthor", "ifcopenshell.file", tuple[AuthorOp, ...]], "RuntimeRail[AuthorReceipt]"]


def _transactional(run: _Run) -> _Run:
    """Rollback aspect: a clean `Ok` commits the batch; a typed `Error` rail OR a raised
    provider exception runs `undo()` then closes the batch, so a half-applied script never
    persists — the single boundary kernel allowed language control flow, the body never re-states it."""

    @functools.wraps(run)
    def wrapped(self: "IfcAuthor", model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        model.begin_transaction()
        try:
            rail = run(self, model, script)
        except Exception:
            model.undo()
            model.end_transaction()  # no commit= arg; rollback is undo() before the close
            raise
        if rail.is_error():  # `Ok`/`Error` are both `Result` instances, so discriminate by method, never isinstance
            model.undo()
        model.end_transaction()
        # The aspect projects the single batch frame it opened as the receipt depth — the
        # transaction-nesting evidence this aspect owns, never the `len(facts)` op count.
        return rail.map(lambda r: replace(r, depth=r.depth + 1))

    return wrapped


def _stamped(run: _Run) -> _Run:
    """Provenance projection over the returned carry: sets whether the script opened an
    owner-history root so an unstamped mutation is structurally visible to the reviewer —
    a post-fold projection, never a control branch and never re-derived per verb."""

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
            lambda acc, op: acc.bind(lambda carry: self._step(model, carry, op)), script, Ok(AuthorCarry()),
        )
        return folded.map(lambda carry: AuthorReceipt(
            model.schema,
            carry.facts,
            tuple(f.guid for f in carry.facts if f.guid),
            carry.edited,
        ))

    def _step(self, model: "ifcopenshell.file", carry: AuthorCarry, op: AuthorOp) -> "RuntimeRail[AuthorCarry]":
        row = IFC_API_VERBS[op.verb]

        def fire(kwargs: dict[str, object]) -> AuthorCarry:
            # REMOVE's usecase returns None and severs the entity, so the inverse fan-out is read
            # from the resolved input product BEFORE the delete; minting/relating verbs read the returned product after.
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

- [API_DIRECT_DISPATCH]: the folder `ifcopenshell` catalogue confirms the closed `module.action` authoring vocabulary (`root.create_entity`, `root.remove_product`, `root.copy_class`, `attribute.edit_attributes`, `geometry.add_representation`, `geometry.edit_object_placement`, `context.add_context`, `unit.add_si_unit`, `unit.assign_unit`, `pset.add_pset`, `spatial.assign_container`, `aggregate.assign_object`, `material.add_material`, `type.assign_type`) and that 0.8.5 deprecates `api.run` ("Do not use this function") in favor of the direct `ifcopenshell.api.<module>.<action>(file, **kwargs)` callable over the identical vocabulary. The owner threads the canonical direct callable: `_usecase` splits the `IFC_API_VERBS` dotted `module.action` string and resolves `getattr(importlib.import_module("ifcopenshell.api.<module>"), action)`, so the table string is the contract and the resolved callable is the dispatch — never the deprecated `api.run`, never a per-verb method. The `owner.create_owner_history`/`owner.update_owner_history` pair and the `OwnerHistory` slot the `stamp` payload carries confirm against the live `ifcopenshell.api.owner` usecase package; the `OWNER_NEW`/`OWNER_UPD` two-row split binds each to its own callable so the script is an ordered verb-row pair — `OWNER_NEW` mints into its slot, `OWNER_UPD`'s `stamp` payload resolves that slot back — rather than a runtime first-vs-subsequent branch hidden in an aspect.
- [TRANSACTION_ROLLBACK_STACK]: the folder `ifcopenshell` catalogue confirms `file.begin_transaction`/`end_transaction()` open and close an undoable edit batch with `end_transaction()` taking NO `commit=` argument, and `undo`/`redo`/`discard_transaction` step the stack — a rollback is `discard_transaction()`/`undo()`, never an `end_transaction(commit=False)`. The `@transactional` decorator fences the rollback at one site: a raised op or a typed `Error` rail runs `undo()` then `end_transaction()` to close the batch before the fault reaches `boundary`, so a half-applied script never persists; the rollback verb lives in the single aspect, not per-verb bodies.
- [GUID_CODEC]: the folder `ifcopenshell` catalogue confirms the four positional-only GUID-codec members `guid.new`/`guid.compress(uuid, /)`/`guid.expand(guid, /)`/`guid.split(uuid, /)`. The receipt keys identity on `guid.compress(product.GlobalId)` — the minted GlobalId is already in the compressed 22-character IFC form, so authoring never round-trips through `guid.expand`; `expand` is the read-side decompress the analysis hops own, not a mutation receipt key. The `entity_instance.GlobalId` attribute the minted-product read joins against confirms against the folder `ifcopenshell` catalogue model-root surface.
- [MUTATION_FOOTPRINT_STACK]: the folder `ifcopenshell` catalogue confirms `file.traverse(inst, max_levels=None, breadth_first=False)` walks the dependent-entity graph (`max_levels` the depth bound, `None`/`-1` infinite — never a `depth` kwarg), `file.get_inverse(entity)` returns the inverse-referencing instances, and `util.element.get_psets(element)` returns the property/quantity sets as a dict — the three reads the per-op kernel stacks onto the usecase mutation to size each `MutationFact` (pset key count, the full dependent-subtree count under the default unbounded `max_levels`, and the orphaned-inverse count `REMOVE` records before delete). The `len(...)` footprint is correct over whichever container `get_inverse`/`traverse` return, so no read-shape assumption rides the fold.
- [ENTITY_INSTANCE_ISINSTANCE]: the `isinstance(product, ifcopenshell.entity_instance)` guard the kernel uses to discriminate a minted entity from a non-minting usecase return confirms against the folder `ifcopenshell` catalogue `entity_instance` wrapper type; the precise return shape of each relating usecase (`spatial.assign_container`/`aggregate.assign_object`/`type.assign_type` returning the relationship instance versus `None`) is the one usecase-return detail the live `ifcopenshell.api` package confirms, and the composed `Capability` flag (`Capability.MINTS`/`READS in row.cap`) already gates the GUID, footprint, and pset reads so a non-entity return is folded out rather than mis-recorded.
