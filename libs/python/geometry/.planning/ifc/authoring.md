# [PY_GEOMETRY_IFC_AUTHORING]

IFC model mutation as a transactional verb script — write side the analysis and lifecycle hops read against. `IfcAuthor` dispatches the `ifcopenshell.api.<module>.<action>(file, **kwargs)` usecase callables over one `AuthorVerb` vocabulary whose member VALUE is the dotted usecase, so the vocabulary IS the name-to-usecase map; every keyword spelling, argument arity, requiredness, and mint capability DERIVES from the live signature at a `@cache`-memoized `_row`, and one `apply` entry folds a whole op tuple under a single `begin_transaction`/`end_transaction`. Widening the surface is one enum row, never a parallel create/edit/assign method family over the usecase set `ifcopenshell` already dispatches and never a second transcription of a contract the package publishes.

Each script is an immutable left-fold over a frozen `AuthorCarry` short-circuiting on the first `Error`; transaction and provenance concerns are AOP decorators over the one `_run` rail — `@transactional` fences rollback across both a rail fault and a provider exception, `@stamped` projects owner-history provenance onto the receipt — never re-derived per verb body. `AuthorReceipt` is itself the `ReceiptContributor`, and `apply` returns through the folder's own `evidence_run` weave under `EvidenceScope.IFC_AUTHORING`, so the folder's one MUTATING surface carries the same span, cost bracket, and conditional harvest every sibling producer does. `apply_async` is the awaitable twin carrying that same fold's regulatory audit trail onto the durable evidence plane once the transaction fence has closed. `IfcWire` carries a format key, raw IFC bytes, schema key, semantic graph address, and mint time; this companion decodes the bytes and projects its own graph to reproduce the semantic address, while `AuthorReceipt` remains local mutation evidence.

## [01]-[INDEX]

- [02]-[AUTHORING]: the signature-derived authoring surface — the closed dotted-usecase vocabulary, the one `AuthorPayload` slot/value shape, the `AuthorCarry` transaction fold under the `@transactional`/`@stamped` aspects woven inside the `evidence_run` weave, the `AuthorReceipt`/`MutationFact` receipt that is itself the `ReceiptContributor`, and the `apply_async` twin recording the regulatory mutation trail past the transaction fence.

## [02]-[AUTHORING]

- Owner: `IfcAuthor` — boundary capsule over one `apply` entry, holding only the `composition` custody key it threads into the weave; the usecase callable resolved from the derived row is the single polymorphic dispatch, never a method per usecase, and accumulation state lives in the fold, not the owner.
- Cases: `AuthorVerb` rows ARE the vocabulary and each row is exactly its dotted usecase, so the enum carries the whole name-to-usecase correspondence and no parallel table restates it. `_row` derives everything else from `inspect.signature` over the resolved callable: one `VerbArgument` per parameter carrying its keyword, whether it takes entity instances, whether it takes a COLLECTION of them, and whether it is required, plus the `Capability.MINTS` flag off an entity-bearing return. The per-usecase relating spelling is a fact the signature publishes, never a policy this owner owns, so a transcribed keyword column and the re-keyer that would consume it are both deleted forms — a transcription drifts silently, and `owner.update_owner_history` taking `element` is exactly the drift it drifts into. `AuthorPayload` is one shape for the same reason: `bind` maps a usecase keyword to the ordered slots filling it — one slot for a scalar entity parameter, several for a collection, the arity read off the parameter's own declared type — and `values` carries the literal arguments, so a case family split by argument shape has nothing left to discriminate. Host-coupled usecases stay out of the vocabulary by the same law the derivation serves: `geometry.add_representation` declares `blender_object`/`geometry` parameters this lane cannot supply, so the host-free `add_mesh_representation`/`add_profile_representation` mint and the `assign_representation` bind are the rows that carry representation authoring.
- Entry: `apply` takes an `ifcopenshell.file` and an `AuthorOp` tuple, returning `RuntimeRail[AuthorReceipt]` through the `evidence_run` weave — a provider exception converts to a `BoundaryFault` once at the weave's fence, an unresolved slot and every admission divergence arrive already typed on the rail, and both fault sources meet on one carrier. A relating verb consumes prior slots through its `bind` map, so a build-a-wall-in-a-storey script is one ordered op list, never manual id-chaining.
- Law: the durable mutation trail lands on the `python:runtime/observability/journal#LEDGER` plane as one `REGULATORY` `AuditFact` per mutation, and `apply_async` is its ONE seat — the awaitable twin this wholly-synchronous owner mints over the band hop, since recording suspends and the live pybind11 handle admits no async fold of its own. The seat is PAST the transaction fence by law, never inside it: a suspending record between `begin_transaction` and `end_transaction` lets an unrelieved intake hold a half-applied model open, and the facts mint off the settled receipt so a rolled-back script records nothing rather than a trail of mutations no model kept. The verb is the usecase's own dotted spelling and the subjects are `MutationFact.guid` alone — a non-minting verb indexes nothing rather than forging a subject — and no meter rides this leg, the crossing's cpu being the graduation weave's one charge.
- Law: authoring runs on the caller floor by charter — the live `ifcopenshell.file` is the engine's in-process resource, a pybind11 handle no pickle seam carries, and a transactional mutation script is not idempotent: it earns no lane crossing, and any future kernel wrapping a mutating script declares `idempotent=False` so a worker-death retry never re-applies a half-committed mutation the rollback fence cannot see.
- Auto: `apply` threads the graduation weave, so the span opens under the caller's composition and nests beneath it, `_priced` brackets the transaction's real cost on the settle, the refusal, AND the unwind, and the weave's conditional harvest emits `AuthorReceipt.contribute` on the cleared `Ok` — the transaction depth, the minted GUIDs, the `edited` census, and the provenance flag reach the receipt stream by the same path every sibling's evidence does. Inside it `@transactional` closes the batch on a clean `Ok`, runs `undo()` before `end_transaction()` on a typed `Error` rail OR a raised op so a half-applied script never persists, and projects transaction depth onto the receipt via `replace`, never the `len(facts)` op count; `@stamped` sets the provenance flag from whether the script carried an owner-history op. `to_kwargs` is the admission gate the derived roster makes possible: an argument the usecase never declares, a required one left unsupplied, a scalar entity parameter bound to anything but one slot, and an unresolved slot each name themselves BEFORE the transaction commits, where an unproven kwargs bag surfaces every one of them as a provider `TypeError` several ops into a committed batch. Per-op kernel stacks four `ifcopenshell` reads into one `MutationFact` — the usecase mutation, `guid.compress` on the minted GlobalId, the `get_psets` key count, the `traverse` subtree count — and `REMOVE` reads `get_inverse` BEFORE the delete, off the usecase's OWN first entity keyword, because the usecase severs the entity and the inverse-reference count is unreadable after.
- Receipt: `AuthorReceipt` implements `contribute` structurally (against the `@runtime_checkable` protocol, never subclassing it) and carries the same `MutationFact` block as the typed return, so evidence is structured field-by-field, not a free-form log. `edited` and `depth` are distinct evidence — `depth` is the transaction nesting the `@transactional` aspect projects, `edited` counts the mutations that minted NO addressable entity, read off the actual outcome rather than the declared capability, since a relating usecase returning `None` mints nothing and a capability-declared count misses exactly that case.
- Packages: `ifcopenshell` (the `api.<module>.<action>` usecase callables and the listener shim that carries their unwrapped `__signature__`, the `begin_transaction`/`undo`/`end_transaction` stack, the `guid.compress` codec, and the `get_psets`/`get_inverse`/`traverse` read graph the mutation footprint joins against), geometry graduation (`evidence_run`/`EvidenceScope` the weave), runtime (`RuntimeRail`/`BoundaryFault`, `railed` the bound `effect.result` builder, `Receipt`/`ReceiptContributor`, `ScopeKey`/`DEFAULT_SCOPE` the custody key, `Journal` with the `AuditFact`/`Party`/`Actor`/`Retain`/`Change` vocabulary the durable trail records through), `expression` (the `Result` rails, `Map` for the slot vocabulary and the payload bindings, `Block` for the fact stream and the divergence roster, `Option` for the derived-shape probe), `msgspec` (`Struct`, `structs.replace`), stdlib `enum` (`Flag` for `Capability`, `StrEnum` for `AuthorVerb`), stdlib `inspect`/`typing` (the signature and annotation readers the contract derives from), stdlib `functools`/`importlib` (the memoized resolution).
- Growth: a new authoring capability is one `AuthorVerb` row — its keyword contract, arity, requiredness, and mint capability all derive at first use, its audit verb arriving with it; a new capability dimension is one `Capability` member the `in row.cap` tests pick up without a new column; a new footprint-read verb is one `_FOOTPRINT` member, the one axis no signature publishes; a newly audited footprint column is one `_changed` arm. Zero new dispatcher, no per-usecase method, no payload case.
- Boundary: `ifcopenshell` owns entity construction, usecase dispatch, the transaction stack, the GUID codec, the inverse/traverse graph, AND the argument contract — no hand-rolled STEP writer, no local UUID/GlobalId fold, no transcribed keyword spelling, no `guid.expand` on the write side (the minted GlobalId is already compressed; `expand` is the read-side codec). `api.extract_docs` is NOT that contract's reader: it introspects a `Usecase.__init__` only a handful of legacy api modules still define and reads `object.__init__` on the rest, so the signature over the wrapped callable is the one live source. No durable store and no Rhino/GH mutation, and no ledger, custody, or retention window minted here — the evidence plane arrives bound at the composition root and `apply_async` records a `Retain` class alone. No Blender-coupled usecase enters the vocabulary. Relating verbs consume in-script slots and never re-query the model by string GUID when the minting op is in the same script.

```python signature
import functools
import importlib
import inspect
from collections.abc import Callable, Generator, Iterable, Sequence
from enum import Flag, StrEnum, auto
from typing import TYPE_CHECKING, Final, get_args, get_origin

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace

from rasm.geometry.graduation import EvidenceScope, evidence_run
from rasm.runtime.faults import BoundaryFault, RuntimeRail, railed
from rasm.runtime.journal import Actor, Assigned, AuditFact, Change, Cleared, Fact, Journal, Party, Retain
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

if TYPE_CHECKING:  # worker-only: every runtime `ifcopenshell` reference rides a function-local `import ifcopenshell.<sub>  # ruff:ignore[import-outside-top-level]`, so the module loads clean under the boundary-scope import policy
    import ifcopenshell

# --- [TYPES] ---------------------------------------------------------------------------


class AuthorVerb(StrEnum):
    # the member VALUE is the dotted `ifcopenshell.api.<module>.<action>` usecase, so this enum IS the whole
    # name-to-usecase map and no parallel table restates it. Every keyword spelling, arity, requiredness, and mint
    # capability derives from the live signature at `_row`, so a new capability is ONE row here.
    # `geometry.add_representation` is deliberately absent: it is the Blender-coupled dispatcher taking
    # `blender_object`/`geometry`, and the host-free mint-then-bind pair below is what this lane can actually call.
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
    PSET = "pset.add_pset"
    CONTAIN = "spatial.assign_container"
    AGGREGATE = "aggregate.assign_object"
    MATERIAL = "material.add_material"
    TYPE = "type.assign_type"
    OWNER_NEW = "owner.create_owner_history"
    OWNER_UPD = "owner.update_owner_history"


class Capability(Flag):
    # Composable caps in one column — `MINTS|READS` composes, so aspects test `Capability.MINTS in row.cap`, never
    # parallel bool columns. `MINTS` derives from an entity-bearing return; `READS` is the one axis no signature
    # publishes, so it alone stays a declared row. There is no relationship member: the per-usecase relating spelling
    # derives from the signature, and a flag member no consumer reads governs nothing.
    NONE = 0
    MINTS = auto()  # the usecase returns an entity; its GlobalId keys the receipt
    READS = auto()  # size the pset/inverse footprint after the mutation


# --- [CONSTANTS] -----------------------------------------------------------------------

# this owner's one name, serving the receipt label and the audit actor identity alike, so a rename cannot strand a
# receipt stream under one spelling and a durable actor column under another.
OWNER: Final[str] = "rasm.geometry.ifc.authoring"

# annotation origins that mark a COLLECTION parameter, so a `list[entity_instance]` keyword consumes several slots
# while a scalar one consumes exactly one — arity read off the parameter's declared type, never a payload flag.
_SEQUENCES: Final[frozenset[object]] = frozenset({list, tuple, set, frozenset, Sequence, Iterable})

# footprint-read policy: the two verbs whose post-mutation pset or inverse count is evidence worth the extra reads.
# No signature publishes this — it is this owner's measurement choice, so it is the one declared capability row.
_FOOTPRINT: Final[frozenset[AuthorVerb]] = frozenset({AuthorVerb.REMOVE, AuthorVerb.PSET})

# --- [MODELS] --------------------------------------------------------------------------


class VerbArgument(Struct, frozen=True, gc=False):
    # one derived parameter contract: whether it carries entity instances (so it binds SLOTS rather than literals),
    # whether it carries a collection of them, and whether the usecase declares a default for it.
    keyword: str
    entity: bool
    collection: bool
    required: bool


class IfcApiVerb(Struct, frozen=True, gc=False):
    usecase: str  # dotted module.action resolved to ifcopenshell.api.<module>.<action>
    cap: Capability  # derived MINTS composed with the declared READS policy
    arguments: tuple[VerbArgument, ...]  # the usecase's OWN parameter roster, `file` excluded, in declaration order


class AuthorPayload(Struct, frozen=True, gc=False):
    # ONE payload shape, because the usecase's own signature already discriminates every argument: `bind` maps a
    # usecase keyword to the ordered slots filling it, `values` carries the literal arguments. A case family split by
    # argument shape has nothing left to discriminate once the keyword spellings derive, so it is the deleted form.
    bind: "Map[str, tuple[str, ...]]" = Map.empty()
    values: "Map[str, object]" = Map.empty()

    def divergences(self, row: IfcApiVerb) -> "Block[str]":
        # one admission fold naming EVERY divergence at once, before the transaction commits: an argument the usecase
        # never declares, a literal bound where an entity parameter is wanted, a required argument left unsupplied,
        # and a scalar entity parameter bound to anything but one slot. An unproven kwargs bag surfaces each of these
        # as a provider `TypeError` several ops into a committed batch, which no rollback fence can diagnose.
        declared = Map.of_seq((argument.keyword, argument) for argument in row.arguments)
        bound = dict(self.bind.items())
        supplied = frozenset(bound) | frozenset(name for name, _ in self.values.items())
        return Block.of_seq([
            *(f"unknown:{name}" for name in sorted(supplied) if declared.try_find(name).is_none()),
            *(f"not-entity:{name}" for name in sorted(bound) if declared.try_find(name).map(lambda a: not a.entity).default_value(False)),
            *(f"unsupplied:{a.keyword}" for a in row.arguments if a.required and a.keyword not in supplied),
            *(
                f"arity:{a.keyword}={len(bound[a.keyword])}"
                for a in row.arguments
                if a.entity and not a.collection and a.keyword in bound and len(bound[a.keyword]) != 1
            ),
        ])

    @railed
    def to_kwargs(self, row: IfcApiVerb, slots: "Map[str, object]") -> "Generator[RuntimeRail[object], object, dict[str, object]]":
        # `railed` `effect.result` builder: the roster proof short-circuits first, then each `yield from` resolves one
        # slot, so an absent slot is `Error(BoundaryFault)` on the rail rather than a `KeyError` three ops deep. The
        # collection-versus-scalar projection reads the parameter's declared type, never a payload discriminant.
        collections = Map.of_seq((argument.keyword, argument.collection) for argument in row.arguments)
        yield from _proven(row, self.divergences(row))
        bound: dict[str, object] = {}
        for name, names in self.bind.items():
            resolved: list[object] = []
            for slot in names:
                resolved.append((yield from _slotted(slots, slot)))
            # a collection parameter receives the resolved `list` its own annotation declares, a scalar one the single
            # entity — the arity the roster proved, so neither branch re-checks what admission already settled.
            bound[name] = resolved if collections[name] else resolved[0]
        return bound | dict(self.values.items())


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
    # OWNER_NEW mints its history into `op.slot`; every later op reaching it binds that slot by name through its own
    # `bind` map, so the change-history pair is ordinary slot flow rather than a decorator side-channel.
    slots: "Map[str, object]" = Map.of_seq(())
    facts: "Block[MutationFact]" = Block.empty()
    edited: int = 0  # running count of mutations that minted no addressable entity


class AuthorReceipt(Struct, frozen=True):
    schema: str
    # the mutated model's own durable identity — the compressed `IfcProject` GlobalId — because a schema token names
    # a FORMAT and the audit target, the C# re-projection, and a support bundle all ask WHICH model this batch touched.
    model: str
    facts: "Block[MutationFact]"
    guids: tuple[str, ...]
    edited: int  # non-minting mutations against existing entities
    depth: int = 0  # transaction nesting the @transactional aspect projects via replace
    stamped: bool = False  # set by the @stamped provenance aspect: did the script open an owner history

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
                    "psets": sum(f.psets for f in self.facts),
                    "subtree": sum(f.subtree for f in self.facts),
                    "edited": self.edited,
                    "depth": self.depth,
                    "stamped": self.stamped,
                },
            ),
        )


# --- [OPERATIONS] ----------------------------------------------------------------------


@functools.cache
def _usecase(dotted: str) -> "Callable[..., object]":
    module, action = dotted.rsplit(".", 1)
    return getattr(importlib.import_module(f"ifcopenshell.api.{module}"), action)


def _entity_shape(hint: object, instance: type) -> "Option[bool]":
    # structural annotation read — `get_origin`/`get_args` over the annotation OBJECT, never a rendered string:
    # `Some(False)` a scalar entity parameter, `Some(True)` a collection of them, `Nothing` a literal argument. An
    # `Optional`/`Union` recurses to its first entity member, so an optional entity parameter reads as one entity
    # parameter and its requiredness rides the separate default probe.
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
    # ONE derivation of the whole keyword contract off the live usecase signature. `ifcopenshell.api` wraps every
    # usecase in a listener shim that assigns the UNWRAPPED `__signature__`, so this read IS the real argument
    # contract; `api.extract_docs` is not, because it introspects a `Usecase.__init__` only a handful of legacy
    # modules still define and reads `object.__init__` on the rest. Memoized per verb, so the reflection is paid once.
    import ifcopenshell  # ruff:ignore[import-outside-top-level]

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
    # the model's own identity, read once beside `model.schema` at the same fold: the `IfcProject` GlobalId compressed
    # exactly as every minted GUID on this page is, empty for a file carrying no project root, so a durable audit row
    # and the receipt name one model rather than a format token that names every IFC4 file in the estate.
    import ifcopenshell.guid  # ruff:ignore[import-outside-top-level]

    projects = model.by_type("IfcProject")
    return ifcopenshell.guid.compress(projects[0].GlobalId) if projects and projects[0].GlobalId else ""


def _slotted(slots: "Map[str, object]", slot: str) -> "RuntimeRail[object]":
    return Ok(slots[slot]) if slot in slots else Error(BoundaryFault(resource=("ifc.authoring.slot", slot)))


def _changed(fact: MutationFact) -> tuple[Change, ...]:
    # the mutation's own footprint as a TYPED diff rather than a rendered sentence: a minting verb ASSIGNS the
    # compressed GlobalId it produced, `REMOVE` CLEARS the dependent subtree it severed — the one count unreadable
    # after the usecase runs, which is why `_step` reads it first — and a pset footprint rides only where the verb's
    # `READS` capability actually took one. A zero row asserting a count nobody measured is the shape this omits.
    return (
        *((Assigned(path="/guid", next=fact.guid),) if fact.guid else ()),
        *(
            (Cleared(path="/subtree", prior=str(fact.subtree)),)
            if fact.verb is AuthorVerb.REMOVE
            else (Assigned(path="/subtree", next=str(fact.subtree)),)
        ),
        *((Assigned(path="/psets", next=str(fact.psets)),) if fact.psets else ()),
    )


def _evidence(receipt: AuthorReceipt) -> "Block[Fact]":
    # ONE audit row per mutation, offered as one batch, minted off the SETTLED receipt: `@transactional` undoes on an
    # `Error` rail or a raised op, so a rolled-back script records nothing rather than an audit trail of mutations no
    # model kept. The verb IS the usecase's own dotted `<module>.<action>` spelling — already the runtime verb
    # grammar — so an audit line greps against the `ifcopenshell.api` namespace the mutation actually ran and no
    # second verb table stands between them. Retention is REGULATORY because a model mutation is the disposal
    # evidence a project reads back years later, and `MutationFact.guid` is this branch's one honest subject source:
    # a verb that minted no addressable entity indexes nothing rather than forging a subject from a schema token. No
    # meter rides this leg — it moves no bytes, and the crossing's cpu is the graduation weave's one COMPUTE charge.
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


def _proven(row: IfcApiVerb, divergences: "Block[str]") -> "RuntimeRail[tuple[()]]":
    # one refusal names EVERY divergence, so an operator repairs a whole malformed op rather than one keyword per run.
    return Ok(()) if divergences.is_empty() else Error(BoundaryFault(boundary=(row.usecase, ";".join(divergences))))


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
    def __init__(self, composition: ScopeKey = DEFAULT_SCOPE) -> None:
        # the custody key is the owner's ONLY state — no handle, no accumulator; the fold carries the rest.
        self._composition = composition

    def apply(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        # the folder's one MUTATING surface rides the folder's own weave, not a bare fence: the span opens under the
        # caller's composition, `_flat` absorbs the already-railed fold un-nested, a provider raise converts at the
        # weave's fence, `_priced` brackets the transaction's real cost on settle, refusal, and unwind alike, and the
        # conditional harvest emits `AuthorReceipt.contribute` on the cleared `Ok`. A bare `boundary` here is the
        # deleted form: it converts the raise and nothing else, so the transaction depth, the minted GUIDs, the
        # `edited` census, and the provenance flag reach no receipt stream on any path — the folder's weakest evidence
        # on the one surface that changes the model.
        return evidence_run(
            EvidenceScope.IFC_AUTHORING, f"apply.{len(script)}", lambda: self._run(model, script), composition=self._composition
        )

    async def apply_async(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        # the awaitable twin over the band hop, and the folder's ONE durable mutation trail. `apply` is synchronous
        # whole — the live `ifcopenshell.file` is an in-process pybind11 handle and the transaction stack blocks — so
        # the record cannot land inside it: recording SUSPENDS, and a suspension between `begin_transaction` and
        # `end_transaction` hands an unrelieved intake the power to hold a half-applied model open past its fence.
        # This leg runs the same fold and records once the fence has closed, so the transaction boundary stays exactly
        # where `@transactional` puts it. The record rail BINDS into the verdict — an armed plane refusing a mutation
        # fact is a governance failure this caller owns — while a composition that installed no plane folds to the
        # lawful no-op and pays one map read.
        match self.apply(model, script):
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(_evidence(receipt), scope=self._composition)).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    @_stamped
    @_transactional
    def _run(self, model: "ifcopenshell.file", script: tuple[AuthorOp, ...]) -> "RuntimeRail[AuthorReceipt]":
        folded: RuntimeRail[AuthorCarry] = functools.reduce(
            lambda acc, op: acc.bind(lambda carry: self._step(model, carry, op)), script, Ok(AuthorCarry())
        )
        return folded.map(
            lambda carry: AuthorReceipt(model.schema, _project(model), carry.facts, tuple(f.guid for f in carry.facts if f.guid), carry.edited)
        )

    def _step(self, model: "ifcopenshell.file", carry: AuthorCarry, op: AuthorOp) -> "RuntimeRail[AuthorCarry]":
        row = _row(op.verb)

        def fire(kwargs: dict[str, object]) -> AuthorCarry:
            # REMOVE severs the entity, so the inverse fan-out is read from the input product BEFORE the delete — off
            # the usecase's OWN first entity keyword, never a hardcoded `product` that drifts with the signature.
            severed = len(model.get_inverse(kwargs[row.arguments[0].keyword])) if op.verb is AuthorVerb.REMOVE else 0
            return self._record(model, carry, op, row, _usecase(row.usecase)(model, **kwargs), severed)

        return op.payload.to_kwargs(row, carry.slots).map(fire)

    @staticmethod
    def _record(model: "ifcopenshell.file", carry: AuthorCarry, op: AuthorOp, row: IfcApiVerb, product: "object", severed: int) -> AuthorCarry:
        import ifcopenshell  # ruff:ignore[import-outside-top-level]  boundary-scope: the entity_instance probe needs the name bound
        import ifcopenshell.guid  # ruff:ignore[import-outside-top-level]
        import ifcopenshell.util.element  # ruff:ignore[import-outside-top-level]

        is_entity = isinstance(product, ifcopenshell.entity_instance)
        guid = ifcopenshell.guid.compress(product.GlobalId) if Capability.MINTS in row.cap and is_entity and product.GlobalId else ""
        psets = len(ifcopenshell.util.element.get_psets(product)) if Capability.READS in row.cap and is_entity else 0
        subtree = severed if op.verb is AuthorVerb.REMOVE else (len(model.traverse(product)) if is_entity else 0)
        return AuthorCarry(
            slots=carry.slots.add(op.slot, product) if op.slot and is_entity else carry.slots,
            facts=carry.facts.append(Block.singleton(MutationFact(op.verb, guid, psets, subtree))),
            # `edited` counts what the call DID, not what its capability declared: a relating usecase returning `None`
            # mints nothing addressable, and a declared-capability count misses exactly that case.
            edited=carry.edited + (0 if guid else 1),
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [VERB_ROSTER_DERIVATION]-[OPEN]: does `_row` resolve a complete `VerbArgument` roster for every `AuthorVerb` under a LIVE import — the listener shim assigns `__signature__` from the unwrapped usecase at wrap time, so a usecase whose annotations reference a name the wrap-time evaluation cannot resolve would surface as an import failure rather than an empty roster; the estate interpreter cannot import the wrapper to prove it. Route: run `_row` over the whole vocabulary on an interpreter the `ifcopenshell` extension targets, else read each usecase module's own annotation imports in the installed distribution.
