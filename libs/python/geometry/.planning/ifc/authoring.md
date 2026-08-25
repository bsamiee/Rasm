# [PY_GEOMETRY_IFC_AUTHORING]

IFC model mutation as a transactional verb script — write side the analysis and lifecycle hops read against. `IfcAuthor` dispatches the `ifcopenshell.api.<module>.<action>(file, **kwargs)` usecase callables over one `AuthorVerb` vocabulary whose member VALUE is the dotted usecase, so the vocabulary IS the name-to-usecase map; every keyword spelling, argument arity, requiredness, and mint capability DERIVES from the live signature at a `@cache`-memoized `_row`, and one `apply` entry folds a whole op tuple under a single `begin_transaction`/`end_transaction`. Widening the surface is one enum row, never a parallel create/edit/assign method family over the usecase set `ifcopenshell` already dispatches and never a second transcription of a contract the package publishes.

Each script is an immutable left-fold over a frozen `AuthorCarry` short-circuiting on the first `Error`; transaction and provenance concerns are AOP decorators over the one `_run` rail — `@transactional` fences rollback across both a rail fault and a provider exception, `@stamped` projects owner-history provenance onto the receipt — never re-derived per verb body. `AuthorReceipt` is itself the `ReceiptContributor`, and `apply` returns through the folder's own `evidence_run` weave under `EvidenceScope.IFC_AUTHORING`, so the folder's verb-scripted MUTATING surface carries the same span, cost bracket, and conditional harvest every sibling producer does. `apply_async` is the awaitable twin carrying that same fold's regulatory audit trail onto the durable evidence plane once the transaction fence has closed. `IfcWire` carries a format key, raw IFC bytes, schema key, semantic graph address, and mint time; this companion decodes the bytes and projects its own graph to reproduce the semantic address, while `AuthorReceipt` remains local mutation evidence.

## [01]-[INDEX]

- [02]-[AUTHORING]: the signature-derived authoring surface — the closed dotted-usecase vocabulary, the one `AuthorPayload` slot/value shape, the `AuthorCarry` transaction fold under the `@transactional`/`@stamped` aspects woven inside the `evidence_run` weave, the `AuthorReceipt`/`MutationFact` receipt that is itself the `ReceiptContributor`, and the `apply_async` twin recording the regulatory mutation trail past the transaction fence.

## [02]-[AUTHORING]

- Owner: `IfcAuthor` — boundary capsule over one `apply` entry, holding only the `composition` custody key it threads into the weave; the usecase callable resolved from the derived row is the single polymorphic dispatch, never a method per usecase, and accumulation state lives in the fold, not the owner.
- Cases: `AuthorVerb` rows ARE the vocabulary and each row is exactly its dotted usecase, so the enum carries the whole name-to-usecase correspondence and no parallel table restates it. `_row` derives everything else from `inspect.signature` over the resolved callable: one `VerbArgument` per parameter carrying its keyword, whether it takes entity instances, whether it takes a COLLECTION of them, and whether it is required, plus the `Capability.MINTS` flag off an entity-bearing return. The per-usecase relating spelling is a fact the signature publishes, never a policy this owner owns, so a transcribed keyword column and the re-keyer that would consume it are both deleted forms — a transcription drifts silently, and `owner.update_owner_history` taking `element` is exactly the drift it drifts into. `AuthorPayload` is one shape for the same reason: `bind` maps a usecase keyword to the ordered slots filling it — one slot for a scalar entity parameter, several for a collection, the arity read off the parameter's own declared type — and `values` carries the literal arguments, so a case family split by argument shape has nothing left to discriminate. Host-coupled usecases stay out of the vocabulary by the same law the derivation serves: `geometry.add_representation` declares `blender_object`/`geometry` parameters this lane cannot supply, so the host-free `add_mesh_representation`/`add_profile_representation` mint and the `assign_representation` bind are the rows that carry representation authoring.
- Entry: `apply` takes an `ifcopenshell.file` and an `AuthorOp` tuple, returning `RuntimeRail[AuthorReceipt]` through the `evidence_run` weave — a provider exception converts to a `BoundaryFault` once at the weave's fence, an unresolved slot and every admission divergence arrive already typed on the rail, and both fault sources meet on one carrier. A relating verb consumes prior slots through its `bind` map, so a build-a-wall-in-a-storey script is one ordered op list, never manual id-chaining.
- Law: durable mutation evidence lands on the `python:runtime/observability/journal#LEDGER` plane as one `REGULATORY` `AuditFact` per mutation, and `apply_async` is its ONE seat — the awaitable twin this wholly-synchronous owner mints over the band hop, since recording suspends and the live pybind11 handle admits no async fold of its own. By law the seat is PAST the transaction fence, never inside it: a suspending record between `begin_transaction` and `end_transaction` lets an unrelieved intake hold a half-applied model open, and the facts mint off the settled receipt so a rolled-back script records nothing rather than a trail of mutations no model kept. Each verb is the usecase's own dotted spelling and the subjects are `MutationFact.guid` alone — a non-minting verb indexes nothing rather than forging a subject — and no meter rides this leg, the crossing's cpu being the graduation weave's one charge. Record refusal from an armed plane folds onto the settled receipt's `unrecorded` slot as committed evidence, never onto the verdict: the fence closed before the record ran and the script is non-idempotent, so an `Error` there inverts a kept mutation into a failure whose one repair re-applies it — the discriminant separating this seat from the lifecycle sibling's verdict-bound record, whose refused run loses only recomputable work.
- Law: authoring runs on the caller floor by charter — the live `ifcopenshell.file` is the engine's in-process resource, a pybind11 handle no pickle seam carries, and a transactional mutation script is not idempotent: it earns no lane crossing, and any future kernel wrapping a mutating script declares `idempotent=False` so a worker-death retry never re-applies a half-committed mutation the rollback fence cannot see.
- Auto: `apply` threads the graduation weave, so the span opens under the caller's composition and nests beneath it, `_priced` brackets the transaction's real cost on the settle, the refusal, AND the unwind, and the weave's conditional harvest emits `AuthorReceipt.contribute` on the cleared `Ok` — the transaction depth, the minted GUIDs, the `edited` census, and the provenance flag reach the receipt stream by the same path every sibling's evidence does. Inside it `@transactional` closes the batch on a clean `Ok`, runs `undo()` before `end_transaction()` on a typed `Error` rail OR a raised op so a half-applied script never persists, and projects transaction depth onto the receipt via `replace`, never the `len(facts)` op count; `@stamped` sets the provenance flag from whether the script carried an owner-history op. `to_kwargs` is the admission gate the derived roster makes possible: an argument the usecase never declares, a required one left unsupplied, a scalar entity parameter bound to anything but one slot, and an unresolved slot each name themselves BEFORE the transaction commits, where an unproven kwargs bag surfaces every one of them as a provider `TypeError` several ops into a committed batch. Per-op kernel stacks four `ifcopenshell` reads into one `MutationFact` — the usecase mutation, `guid.compress` on the minted GlobalId, the `get_psets` key count, and the depth-bounded `traverse` sweep whose per-hop differences ARE the shell census — and `REMOVE` reads `get_inverse` BEFORE the delete, off the usecase's OWN first entity keyword, because the usecase severs the entity and its referencing step ids are unreadable after. The sweep asks the walk for the `max_levels` bound the provider already publishes and ends when a level admits nothing new, so a footprint carries the hop distribution it computed rather than the cardinality that projects from it; a caller's declared `Depth` spends there and answers a typed exhaustion, never a truncated census.
- Receipt: `AuthorReceipt` implements `contribute` structurally (against the `@runtime_checkable` protocol, never subclassing it) and carries the same `MutationFact` block as the typed return, so evidence is structured field-by-field, not a free-form log. `edited` and `depth` are distinct evidence — `depth` is the transaction nesting the `@transactional` aspect projects, `edited` counts the mutations that minted NO addressable entity, read off the actual outcome rather than the declared capability, since a relating usecase returning `None` mints nothing and a capability-declared count misses exactly that case. Both merged footprints STATE their law at the fold rather than summing blind: the subtree census is a nearest-seed minimum over the single-source walks, so a dependent two ops share counts once at its shorter hop, and the pset count is a per-product maximum, because a pset footprint is a reading of one product's roster and two readings are not two populations. `unrecorded` carries an armed plane's audit-record refusal as committed evidence — the mutation persisted, so the refusal never inverts the verdict — and the harvest omits the key when the trail landed.
- Packages: `ifcopenshell` (the `api.<module>.<action>` usecase callables and the listener shim that carries their unwrapped `__signature__`, the `begin_transaction`/`undo`/`end_transaction` stack, the `guid.compress` codec, and the `get_psets`/`get_inverse`/`traverse` read graph the mutation footprint joins against), geometry graduation (`evidence_run`/`EvidenceScope` the weave), runtime (`RuntimeRail`/`BoundaryFault`, `railed` the bound `effect.result` builder, `FaultRow`/`RAISES` the two raise coordinates this page spends, `Depth` the shared walk bound every footprint sweep spends and whose exhaustion names the walking row, `Receipt`/`ReceiptContributor`, `ScopeKey`/`DEFAULT_SCOPE` the custody key, `Journal` with the `AuditFact`/`Party`/`Actor`/`Retain`/`Change` vocabulary the durable trail records through), `ifc/selector#SELECTOR` (the band-wide `IfcFault` family and the `ArgumentFlaw` vocabulary its divergence case carries — this page's one intra-band edge, one-way and cycle-free), `expression` (the `Result` rails, `Map` for the slot vocabulary, the payload bindings, and the hop-keyed reachability merge, `Block` for the fact stream and the divergence roster, `Option` for the derived-shape probe and the arity count only one divergence law measures), `msgspec` (`Struct`, `structs.replace`), stdlib `enum` (`Flag` for `Capability`, `StrEnum` for `AuthorVerb`), stdlib `inspect`/`typing` (the signature and annotation readers the contract derives from), stdlib `functools`/`importlib` (the memoized resolution).
- Growth: a new authoring capability is one `AuthorVerb` row — its keyword contract, arity, requiredness, and mint capability all derive at first use, its audit verb arriving with it; a new capability dimension is one `Capability` member the `in row.cap` tests pick up without a new column; a new footprint-read verb is one `_FOOTPRINT` member, the one axis no signature publishes; a newly audited footprint column is one `_changed` arm; a new admission divergence law is one `ArgumentFlaw` member at `ifc/selector#SELECTOR` and one `divergences` arm. Zero new dispatcher, no per-usecase method, no payload case.
- Boundary: `ifcopenshell` owns entity construction, usecase dispatch, the transaction stack, the GUID codec, the inverse/traverse graph, AND the argument contract — no hand-rolled STEP writer, no local UUID/GlobalId fold, no transcribed keyword spelling, no `guid.expand` on the write side (the minted GlobalId is already compressed; `expand` is the read-side codec). `api.extract_docs` is NOT that contract's reader: it introspects a `Usecase.__init__` only a handful of legacy api modules still define and reads `object.__init__` on the rest, so the signature over the wrapped callable is the one live source. No durable store and no Rhino/GH mutation, and no ledger, custody, or retention window minted here — the evidence plane arrives bound at the composition root and `apply_async` records a `Retain` class alone. No Blender-coupled usecase enters the vocabulary. Relating verbs consume in-script slots and never re-query the model by string GUID when the minting op is in the same script. `IfcAuthor` is the folder's verb-scripted mutation surface, not its only mutating one: the lifecycle exchange re-import arm (`ifc/costing#LIFECYCLE`, the `ifccsv` table write-back) is its ONE sibling, and it mutates under THIS page's transaction-fence law — the batch opens at `begin_transaction`, a typed `Error` rail or a raised provider call runs `undo()` before `end_transaction()`, and a half-applied import never persists. Any third mutating arm the folder mints lands under that same fence rather than a second rollback dialect, because a rollback posture per surface is a posture no operator can reason about across a run.

```python
import functools
import importlib
import inspect
from collections.abc import Callable, Generator, Iterable, Sequence
from enum import Flag, StrEnum, auto
from typing import Final, get_args, get_origin

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace

lazy import ifcopenshell
lazy from ifcopenshell.guid import compress
lazy from ifcopenshell.util.element import get_psets

from rasm.geometry.graduation import EvidenceScope, GeometryLeg, evidence_run
from rasm.geometry.ifc.selector import ArgumentFlaw, IfcFault
from rasm.runtime.faults import PACKAGE, TERMINAL, BoundaryFault, Depth, FaultRow, RuntimeRail, railed, rostered
from rasm.runtime.journal import Actor, Assigned, AuditFact, Change, Cleared, Fact, Journal, Party, Retain
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------


class AuthorVerb(StrEnum):
    CREATE = "root.create_entity"
    REMOVE = "root.remove_product"
    COPY = "root.copy_class"
    EDIT = "attribute.edit_attributes"
    CONTEXT = "context.add_context"
    MESH = "geometry.add_mesh_representation"
    PROFILE = "geometry.add_profile_representation"
    SHAPE = "geometry.assign_representation"
    PLACE = "geometry.edit_object_placement"
    UNIT_SI = "unit.add_si_unit"
    UNIT_ASSIGN = "unit.assign_unit"
    GEOREF_ADD = "georeference.add_georeferencing"
    GEOREF_EDIT = "georeference.edit_georeferencing"
    GEOREF_REMOVE = "georeference.remove_georeferencing"
    WCS = "georeference.edit_wcs"
    PSET = "pset.add_pset"
    CONTAIN = "spatial.assign_container"
    AGGREGATE = "aggregate.assign_object"
    MATERIAL = "material.add_material"
    TYPE = "type.assign_type"
    OWNER_NEW = "owner.create_owner_history"
    OWNER_UPD = "owner.update_owner_history"


class Capability(Flag):
    NONE = 0
    MINTS = auto()
    READS = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

OWNER: Final[str] = f"{PACKAGE}.{GeometryLeg.AUTHORING.value}"

_SEQUENCES: Final[frozenset[object]] = frozenset({list, tuple, set, frozenset, Sequence, Iterable})

_FOOTPRINT: Final[frozenset[AuthorVerb]] = frozenset({AuthorVerb.REMOVE, AuthorVerb.PSET})

# --- [MODELS] ---------------------------------------------------------------------------


class VerbArgument(Struct, frozen=True, gc=False):
    keyword: str
    entity: bool
    collection: bool
    required: bool


class IfcApiVerb(Struct, frozen=True, gc=False):
    usecase: str
    cap: Capability
    arguments: tuple[VerbArgument, ...]


class AuthorPayload(Struct, frozen=True, gc=False):
    bind: "Map[str, tuple[str, ...]]" = Map.empty()
    values: "Map[str, object]" = Map.empty()

    def divergences(self, row: IfcApiVerb) -> "Block[tuple[ArgumentFlaw, str, Option[int]]]":
        declared = Map.of_seq((argument.keyword, argument) for argument in row.arguments)
        bound = dict(self.bind.items())
        supplied = frozenset(bound) | frozenset(name for name, _ in self.values.items())
        return Block.of_seq([
            *((ArgumentFlaw.UNKNOWN, name, Nothing) for name in sorted(supplied) if declared.try_find(name).is_none()),
            *(
                (ArgumentFlaw.NOT_ENTITY, name, Nothing)
                for name in sorted(bound)
                if declared.try_find(name).map(lambda a: not a.entity).default_value(False)
            ),
            *((ArgumentFlaw.UNSUPPLIED, a.keyword, Nothing) for a in row.arguments if a.required and a.keyword not in supplied),
            *(
                (ArgumentFlaw.ARITY, a.keyword, Some(len(bound[a.keyword])))
                for a in row.arguments
                if a.entity and not a.collection and a.keyword in bound and len(bound[a.keyword]) != 1
            ),
        ])

    @railed
    def to_kwargs(self, row: IfcApiVerb, slots: "Map[str, object]") -> "Generator[RuntimeRail[object], object, dict[str, object]]":
        collections = Map.of_seq((argument.keyword, argument.collection) for argument in row.arguments)
        yield from _proven(row, self.divergences(row))
        bound: dict[str, object] = {}
        for name, names in self.bind.items():
            resolved: list[object] = []
            for slot in names:
                resolved.append((yield from _slotted(slots, slot)))
            bound[name] = resolved if collections[name] else resolved[0]
        return bound | dict(self.values.items())


class AuthorOp(Struct, frozen=True):
    verb: AuthorVerb
    payload: AuthorPayload
    slot: str = ""


class MutationFact(Struct, frozen=True):
    verb: AuthorVerb
    guid: str
    step: int
    psets: int
    subtree: tuple[int, ...]

    @property
    def reach(self) -> int:
        return sum(self.subtree)

    @property
    def depth(self) -> int:
        return len(self.subtree)


class AuthorCarry(Struct, frozen=True):
    slots: "Map[str, object]" = Map.of_seq(())
    facts: "Block[MutationFact]" = Block.empty()
    reached: "Map[int, int]" = Map.empty()
    edited: int = 0


class AuthorReceipt(Struct, frozen=True):
    schema: str
    model: str
    facts: "Block[MutationFact]"
    guids: tuple[str, ...]
    reached: "Map[int, int]"
    edited: int

    @property
    def shells(self) -> tuple[int, ...]:
        hops = tuple(hop for _, hop in self.reached.items())
        return tuple(sum(1 for hop in hops if hop == level) for level in range(1, max(hops, default=0) + 1))
    depth: int = 0
    stamped: bool = False
    unrecorded: BoundaryFault | None = None

    def contribute(self) -> "Iterable[Receipt]":
        yield Receipt.of(
            OWNER,
            (
                "emitted",
                "mutation",
                {
                    "schema": self.schema,
                    "model": self.model,
                    "verbs": ",".join(f.verb for f in self.facts),
                    "guids": ",".join(self.guids),
                    "psets": sum(count for _, count in self.facts.fold(_footprint, Map.empty()).items()),
                    "subtree": ",".join(str(count) for count in self.shells),
                    "reach": sum(self.shells),
                    "edited": self.edited,
                    "depth": self.depth,
                    "stamped": self.stamped,
                }
                | ({} if self.unrecorded is None else {"unrecorded": self.unrecorded.tag}),
            ),
        )


# --- [ERRORS] ---------------------------------------------------------------------------

MUTATION_REFUSED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.AUTHORING, point="mutation", arm="boundary", defect="op-refused", retriability=TERMINAL
)
MUTATION_WALK: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.AUTHORING, point="mutation.footprint", arm="boundary", defect="walk-spent", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([MUTATION_REFUSED, MUTATION_WALK]))


def _domain(fault: IfcFault) -> BoundaryFault:
    return BoundaryFault.of(MUTATION_REFUSED, fault)


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def _usecase(dotted: str) -> "Callable[..., object]":
    module, action = dotted.rsplit(".", 1)
    return getattr(importlib.import_module(f"ifcopenshell.api.{module}"), action)


def _entity_shape(hint: object, instance: type) -> "Option[bool]":
    origin, args = get_origin(hint), get_args(hint)
    return (
        Some(False)
        if hint is instance
        else Nothing
        if origin is None
        else Some(True)
        if origin in _SEQUENCES and any(arg is instance for arg in args)
        else Block.of_seq(args).choose(lambda arg: _entity_shape(arg, instance)).try_head()
    )


@functools.cache
def _row(verb: AuthorVerb) -> IfcApiVerb:
    signature = inspect.signature(_usecase(verb.value))
    arguments = tuple(
        VerbArgument(
            keyword=name,
            entity=shape.is_some(),
            collection=shape.default_value(False),
            required=parameter.default is inspect.Parameter.empty,
        )
        for name, parameter in signature.parameters.items()
        if name != "file"
        for shape in (_entity_shape(parameter.annotation, ifcopenshell.entity_instance),)
    )
    mints = Capability.MINTS if _entity_shape(signature.return_annotation, ifcopenshell.entity_instance).is_some() else Capability.NONE
    return IfcApiVerb(usecase=verb.value, cap=mints | (Capability.READS if verb in _FOOTPRINT else Capability.NONE), arguments=arguments)


def _project(model: "ifcopenshell.file") -> str:
    projects = model.by_type("IfcProject")
    return compress(projects[0].GlobalId) if projects and projects[0].GlobalId else ""


def _footprint(seat: "Map[int, int]", fact: MutationFact) -> "Map[int, int]":
    return seat.add(fact.step, max(fact.psets, seat.try_find(fact.step).default_value(0))) if fact.step else seat


def _reached(model: "ifcopenshell.file", product: "ifcopenshell.entity_instance", reach: Depth) -> "RuntimeRail[Map[int, int]]":
    def swept(seat: "Map[int, int]", level: int, budget: Depth) -> "RuntimeRail[Map[int, int]]":
        widened = Block.of_seq(model.traverse(product, max_levels=level)).fold(
            lambda held, node: held if node.id() in held else held.add(node.id(), level), seat
        )
        if len(widened) == len(seat):
            return Ok(seat)
        return budget.stepped().to_result(budget.exhausted(MUTATION_WALK)).bind(lambda left: swept(widened, level + 1, left))

    return swept(Map.of_seq([(product.id(), 0)]), 1, reach)


def _shells(reached: "Map[int, int]") -> tuple[int, ...]:
    hops = tuple(hop for _, hop in reached.items() if hop)
    return tuple(sum(1 for hop in hops if hop == level) for level in range(1, max(hops, default=0) + 1))


def _slotted(slots: "Map[str, object]", slot: str) -> "RuntimeRail[object]":
    return Ok(slots[slot]) if slot in slots else Error(_domain(IfcFault(unresolved_slots=("ifc.authoring.slot", (slot,)))))


def _changed(fact: MutationFact) -> tuple[Change, ...]:
    spelled = ",".join(str(count) for count in fact.subtree)
    return (
        *((Assigned(path="/guid", next=fact.guid),) if fact.guid else ()),
        *(
            (Cleared(path="/subtree", prior=spelled),)
            if fact.verb is AuthorVerb.REMOVE
            else (Assigned(path="/subtree", next=spelled),)
        ),
        *((Assigned(path="/psets", next=str(fact.psets)),) if fact.psets else ()),
    )


def _evidence(receipt: AuthorReceipt) -> "Block[Fact]":
    target = Party(kind="model", key=receipt.model)
    actor = Party(kind=Actor.SERVICE, key=OWNER)
    return receipt.facts.map(
        lambda fact: AuditFact(
            action=fact.verb.value,
            actor=actor,
            target=target,
            retention=Retain.REGULATORY,
            change=_changed(fact),
            subjects=(fact.guid,) if fact.guid else (),
        )
    )


def _proven(row: IfcApiVerb, divergences: "Block[tuple[ArgumentFlaw, str, Option[int]]]") -> "RuntimeRail[tuple[()]]":
    return Ok(()) if divergences.is_empty() else Error(_domain(IfcFault(divergent_arguments=(row.usecase, tuple(divergences)))))


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
            model.end_transaction()
            raise
        if rail.is_error():
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
    def __init__(self, composition: ScopeKey = DEFAULT_SCOPE, reach: Depth = Depth(fixpoint=None)) -> None:
        self._composition = composition
        self._reach = reach

    def apply(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        return evidence_run(
            EvidenceScope.IFC_AUTHORING, f"apply.{len(script)}", lambda: self._run(model, script), composition=self._composition
        )

    async def apply_async(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        match self.apply(model, script):
            case Result(tag="ok", ok=receipt):
                landed = await Journal.record(_evidence(receipt), scope=self._composition)
                return Ok(landed.map(lambda _landed: receipt).default_with(lambda fault: replace(receipt, unrecorded=fault)))
            case refused:
                return Error(refused.error)

    @_stamped
    @_transactional
    def _run(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        folded: RuntimeRail[AuthorCarry] = functools.reduce(
            lambda acc, op: acc.bind(lambda carry: self._step(model, carry, op)), script, Ok(AuthorCarry())
        )
        return folded.map(
            lambda carry: AuthorReceipt(
                model.schema,
                _project(model),
                carry.facts,
                tuple(f.guid for f in carry.facts if f.guid),
                carry.reached,
                carry.edited,
            )
        )

    def _step(self, model: "ifcopenshell.file", carry: AuthorCarry, op: AuthorOp) -> "RuntimeRail[AuthorCarry]":
        row = _row(op.verb)

        def fire(kwargs: dict[str, object]) -> "RuntimeRail[AuthorCarry]":
            severed = (
                Block.of_seq(model.get_inverse(kwargs[row.arguments[0].keyword])).map(lambda node: node.id())
                if op.verb is AuthorVerb.REMOVE
                else Block.empty()
            )
            return self._record(model, carry, op, row, _usecase(row.usecase)(model, **kwargs), severed)

        return op.payload.to_kwargs(row, carry.slots).bind(fire)

    def _record(
        self, model: "ifcopenshell.file", carry: AuthorCarry, op: AuthorOp, row: IfcApiVerb, product: "object", severed: "Block[int]"
    ) -> "RuntimeRail[AuthorCarry]":
        is_entity = isinstance(product, ifcopenshell.entity_instance)
        guid = compress(product.GlobalId) if Capability.MINTS in row.cap and is_entity and product.GlobalId else ""
        psets = len(get_psets(product)) if Capability.READS in row.cap and is_entity else 0
        walked = (
            Ok(Map.of_seq((step, 1) for step in severed))
            if op.verb is AuthorVerb.REMOVE
            else _reached(model, product, self._reach)
            if is_entity
            else Ok(Map.empty())
        )
        return walked.map(
            lambda reached: AuthorCarry(
                slots=carry.slots.add(op.slot, product) if op.slot and is_entity else carry.slots,
                facts=carry.facts.append(
                    Block.singleton(MutationFact(op.verb, guid, product.id() if is_entity else 0, psets, _shells(reached)))
                ),
                reached=Block.of_seq(reached.items()).fold(
                    lambda held, entry: held.add(entry[0], min(entry[1], held.try_find(entry[0]).default_value(entry[1]))),
                    carry.reached,
                ),
                edited=carry.edited + (0 if guid else 1),
            )
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
