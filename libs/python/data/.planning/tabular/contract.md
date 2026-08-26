# [PY_DATA_CONTRACT]

The data-contract owner: a quality gate, a structural admission path, and a cross-frame referential covenant on one page. `DataQuality` folds IDS-style `QualityRule` rows into one `pandera.polars` schema; `FrameAdmission` proves required `FieldShape`s resolve against the live agnostic schema before routing enforcement to that gate; `FrameCovenant` folds `RelationEdge` rows into one `dataframely` collection over the integrity of a system of produced frames. Every contract records into one `ContractClaim` — discriminated by its `subject` literal (`data-quality`/`data-covenant`) — and never raises: enforcement is the caller's `match` on `ContractClaim.status`. There is exactly one `ContractClaim`, one `ClaimStatus`, one pandera gate, and one dataframely collection for the whole package.

`FieldShape` is declared by its minter, the `tabular/interop#INTEROP` `FrameInterop.schema_of` owner, and imported strictly downward beside `Backend`/`FrameInterop` — interop is earlier in the `[00]` order, so the edge is one-way and this page holds no `TYPE_CHECKING` guard, function-local dodge, or second declaration. The covenant `ContractClaim` keys off `ContentIdentity` Merkle-folding the admitted member content-keys, composing the `tabular/materialize#MATERIALIZE` `PartitionBundle` and `spatial/catalog#CATALOG` `StacDiscovery` fingerprints by reference without re-minting them.

## [01]-[INDEX]

- [02]-[QUALITY]: the pandera data-quality gate, the recorded non-enforcing schema claim.
- [03]-[ADMISSION]: structural field shapes, the narwhals frame-admission route into the gate.
- [04]-[COLLECTION]: the dataframely cross-frame referential covenant, the Merkle-keyed collection claim.

## [02]-[QUALITY]

- Owner: `DataQuality` over `pandera.polars`; `QualityRule` the row family modeling one column claim (dtype/nullable/unique/required plus a closed `CheckKind` predicate set), folded into one `DataFrameSchema`. A new validation is one `QualityRule` row.
- Cases: `CheckKind` is the one predicate axis — every case maps to a concrete `pandera.Check` through one of four `expression.collections.Map` behavior tables (`_CMP`/`_SET`/`_TEXT`/`_INCLUSIVE`), so the IDS-style rule vocabulary is one closed switch, never a per-check builder. `length` threads its `int | None` bounds into `Check.str_length`, the distinct numeric-bound case the `str`-pattern `_TEXT` table cannot own (its values type-mismatch a length bound). `unique` is the `QualityRule.unique` column flag, not a `CheckKind` case — pandera routes uniqueness to `Column(unique=)`, never a `Check`, so the axis stays total over real `Check`s and `to_check` never returns `None`.
- Entry: `DataQuality.of` carries the validation policy (`lazy`/`sample`/`seed`) as frozen owner fields, so `validate(frame)` is one modal entrypoint that never grows a per-call disposition or sampling knob. The content key derives off `ContentIdentity.of("schema", self._wire())` — the canonical msgspec-JSON fingerprint over the rule fields plus a policy header — so two owners with identical rules but differing policy never collide onto one key. `lazy=True` raises `SchemaErrors` with the full `failure_cases` frame (accumulate), `lazy=False` the first `SchemaError` (abort), the disposition fixed once on the owner; `sample`/`seed` restrict validation to a deterministic row subset, the pandera large-frame sampling policy. The result is `Ok` even on validation failure, `Error` only when the collect or the key derivation faults.
- Auto: a pass yields `ContractClaim.of("data-quality", (columns,), (), key)` at `PASSED`; a failing lazy validation folds the `SchemaErrors.failure_cases` `column`/`check` pairs into `breaches`, `FAILED` deriving from the non-empty tuple. The frame stays lazy through admission and collects to eager once at the gate, the only point the polars backend surfaces a breach.
- Growth: a new check is one `CheckKind` row threading its `_CMP`/`_SET`/`_TEXT`/`_INCLUSIVE` table; a new column claim is one `QualityRule`; the narwhals-lazy backend is a pandera row on this owner, never a parallel gate.
- Boundary: no raising in domain logic, no global schema registry, no coercion (`coerce=False`); a per-check validator family, an exception-driven gate, and an undecorated `DataQuality.of` are the rejected forms.

```python
from collections.abc import Callable, Iterable
from enum import StrEnum
from re import Pattern
from typing import Any, Final, Literal, assert_never

lazy import polars as pl

import pandera.polars as pap
from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson
from pandera import Check
from pandera.errors import SchemaError, SchemaErrors

from rasm.data.tabular.interop import Backend, DataHook, DataLeg, FieldShape, FrameInterop
from rasm.runtime.faults import FAULT_CONF, TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeResult, boundary, rostered
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------

type Cmp = Literal["ge", "le", "gt", "lt", "eq", "ne"]
type Text = Literal["matches", "contains"]
type Inclusive = Literal["both", "neither", "left", "right"]

# --- [ERRORS] ---------------------------------------------------------------------------


def _collect_raises() -> Catch:
    return (pl.exceptions.PolarsError,)

QUALITY_COLLECT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CONTRACT, point="quality", arm="boundary", defect="collect", retriability=TRANSIENT
)
ADMIT_BREACH: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CONTRACT, point="admit", arm="boundary", defect="field-breach", retriability=TERMINAL, slots=("field", "kind", "declared", "observed")
)
COVENANT_RUN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CONTRACT, point="covenant", arm="boundary", defect="covenant-run", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([QUALITY_COLLECT, ADMIT_BREACH, COVENANT_RUN]))

# --- [TABLES] ---------------------------------------------------------------------------

_CMP: Final[Map[Cmp, Callable[[float], Check]]] = Map.of_seq([
    ("ge", Check.ge),
    ("le", Check.le),
    ("gt", Check.gt),
    ("lt", Check.lt),
    ("eq", Check.equal_to),
    ("ne", Check.not_equal_to),
])
_SET: Final[Map[bool, Callable[[Iterable[Any]], Check]]] = Map.of_seq([(True, Check.isin), (False, Check.notin)])
_TEXT: Final[Map[Text, Callable[[str | Pattern[str]], Check]]] = Map.of_seq([("matches", Check.str_matches), ("contains", Check.str_contains)])
_INCLUSIVE: Final[Map[Inclusive, tuple[bool, bool]]] = Map.of_seq([
    ("both", (True, True)),
    ("neither", (False, False)),
    ("left", (True, False)),
    ("right", (False, True)),
])


# --- [MODELS] ---------------------------------------------------------------------------


class ClaimStatus(StrEnum):
    PASSED = "passed"
    FAILED = "failed"


class ContractClaim(Struct, frozen=True):
    subject: Literal["data-quality", "data-covenant"]
    status: ClaimStatus
    shape: tuple[int, ...]
    breaches: tuple[tuple[str, ...], ...]
    content_key: ContentKey

    @classmethod
    def of(
        cls, subject: Literal["data-quality", "data-covenant"], shape: tuple[int, ...], breaches: tuple[tuple[str, ...], ...], key: ContentKey
    ) -> "ContractClaim":
        return cls(subject, ClaimStatus.PASSED if not breaches else ClaimStatus.FAILED, shape, breaches, key)

VERDICT_POINT: Final[HookPoint[ContractClaim]] = HookPoint(id=DataHook.CONTRACT_VERDICT, payload=ContractClaim, modality=Modality(observe=None))


@tagged_union(frozen=True)
class CheckKind:
    tag: Literal["cmp", "in_range", "member", "text", "length", "monotonic"] = tag()
    cmp: tuple[Cmp, float] = case()
    in_range: tuple[float, float, Inclusive] = case()
    member: tuple[bool, tuple[Any, ...]] = case()
    text: tuple[Text, str] = case()
    length: tuple[int | None, int | None] = case()
    monotonic: tuple[str, bool] = case()

    def to_check(self) -> Check:
        match self:
            case CheckKind(tag="cmp", cmp=(op, v)):
                return _CMP[op](v)
            case CheckKind(tag="in_range", in_range=(lo, hi, inc)):
                lo_closed, hi_closed = _INCLUSIVE[inc]
                return Check.in_range(min_value=lo, max_value=hi, include_min=lo_closed, include_max=hi_closed)
            case CheckKind(tag="member", member=(present, values)):
                return _SET[present](list(values))
            case CheckKind(tag="text", text=(op, pattern)):
                return _TEXT[op](pattern)
            case CheckKind(tag="length", length=(lo, hi)):
                return Check.str_length(min_value=lo, max_value=hi)
            case CheckKind(tag="monotonic", monotonic=(dim, increasing)):
                return Check.is_monotonic(dim, increasing=increasing)
            case unreachable:
                assert_never(unreachable)


class QualityRule(Struct, frozen=True):
    column: str
    dtype: Any
    checks: tuple[CheckKind, ...] = ()
    nullable: bool = False
    unique: bool = False
    required: bool = True

    def to_column(self) -> pap.Column:
        return pap.Column(
            self.dtype, checks=[c.to_check() for c in self.checks], nullable=self.nullable, unique=self.unique, required=self.required, coerce=False
        )


# --- [SERVICES] -------------------------------------------------------------------------


class DataQuality(Struct, frozen=True):
    rules: tuple[QualityRule, ...]
    lazy: bool = True
    sample: int | None = None
    seed: int | None = None
    scope: ScopeKey = DEFAULT_SCOPE

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(
        cls,
        *rules: QualityRule,
        lazy: bool = True,
        sample: int | None = None,
        seed: int | None = None,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> "DataQuality":
        return cls(rules=rules, lazy=lazy, sample=sample, seed=seed, scope=scope)

    def _schema(self) -> pap.DataFrameSchema:
        return pap.DataFrameSchema({r.column: r.to_column() for r in self.rules}, strict=False, coerce=False)

    def _wire(self) -> bytes:
        header = msgjson.encode((self.lazy, self.sample, self.seed), order="deterministic")
        rows = sorted(
            msgjson.encode(
                (r.column, str(r.dtype), r.nullable, r.unique, r.required, [(c.tag, getattr(c, c.tag)) for c in r.checks]),
                order="deterministic",
            )
            for r in self.rules
        )
        return b"\n".join((header, *rows))

    def validate(self, frame: pl.LazyFrame) -> "RuntimeResult[ContractClaim]":
        schema = self._schema()
        return (
            ContentIdentity.of("schema", self._wire())
            .bind(lambda key: boundary(QUALITY_COLLECT, lambda: self._validate(schema, frame, key), catch=_collect_raises()))
            .bind(lambda claim: Hooks.fire(VERDICT_POINT.id, claim, scope=self.scope))
        )

    def _validate(self, schema: pap.DataFrameSchema, frame: pl.LazyFrame, key: ContentKey) -> ContractClaim:
        try:
            schema.validate(frame.collect(), lazy=self.lazy, sample=self.sample, random_state=self.seed)
            return ContractClaim.of("data-quality", (len(self.rules),), (), key)
        except SchemaErrors as fault:
            pairs = tuple((str(c), str(k)) for c, k in fault.failure_cases.select(["column", "check"]).iter_rows())
            return ContractClaim.of("data-quality", (len(self.rules),), pairs, key)
        except SchemaError as fault:
            return ContractClaim.of("data-quality", (len(self.rules),), ((str(fault.schema), str(fault.check)),), key)
```

## [03]-[ADMISSION]

- Owner: `FrameAdmission` composes the `tabular/interop#INTEROP` `FrameInterop.schema_of` derivation over the interop-declared `FieldShape` (imported downward, never re-declared). `FieldShape` is a distinct structural shape (field presence plus dtype plus observed nullability), not a re-mint of the quality `ContractClaim`: admission proves structure, the `QUALITY` gate records the contract.
- Entry: `admit` resolves the live shapes through one `FrameInterop.schema_of` call — the single backend-agnostic derivation reading the per-column null-mask via `null_count()`, never a second inline `collect_schema()` — then folds the resolved `FieldShape` tuple against the required shapes through `FieldShape.resolve`: a required field absent, carrying a non-matching `logical_type`, or declaring `nullable=False` where the live mask observed nulls answers one keyed `FieldBreach`, so a present-but-wrong-dtype field is a structural breach, not a silent pass. Every required column is an independent admission, so the census folds through `traversed(..., by=Disposition.ACCUMULATE)` and ONE combined fault names EVERY divergent column under the four `ADMIT_BREACH` coordinates a consumer matches on. `schema_of` lifts through `narwhals.from_native` inside its own `boundary`, so admission binds the sibling result and the backend rides the `narwhals.Implementation` axis — one path for every backend. `enforce` routes validation to `DataQuality.validate`, lowering through `FrameInterop.translate(frame, Backend.POLARS)` then `.frame.lazy()`, never a second hand-spelled `to_polars().lazy()`.
- Packages: `tabular/interop#INTEROP` (`FieldShape`/`Backend`/`FrameInterop.schema_of`/`translate`/`source` — one module-top prelude importing the strictly-earlier interop module downward), `expression` (`Error`/`Ok` the breach-result arms, `expression.collections.Block`/`Map`/`Map.of_seq` the live `field->FieldShape` resolve map and the accumulated census), `msgspec` (`Struct` the frozen owners), `beartype` (`@beartype(conf=FAULT_CONF)` on `FrameAdmission.of` and the caller-facing `admit`), runtime (`RuntimeResult`/`FAULT_CONF`/`Disposition`/`traversed`). The admit path holds no inline `boundary` since `schema_of`/`translate` carry their own fences and `_resolve` projects the structural verdict through the accumulating result, never a no-op thunk wrapping an already-built value.
- Growth: a new structural attribute is one column on `FieldShape` read once by the interop `schema_of` owner; a new structural divergence is one `BreachKind` member the interop `resolve` owner arms; a new quality rule is one `QualityRule`/`CheckKind` row on `DataQuality`; a new backend is admitted free by the interop `Backend` axis; a new refusal law on this page is one `FaultRow` on `RAISES`.
- Boundary: no Persistence generation law, no live Rhino/GH mutation; a hand-rolled validation loop, a per-backend admission branch, a second inline `collect_schema()` derivation, a presence-only check that passes a wrong-dtype field, a duplicate `ContractClaim`, a second pandera gate, a no-op thunk fenced over an already-built verdict, a joined breach STRING erasing the per-column code, a first-breach abort where every required column admits independently, and an undecorated `of`/`admit` are the rejected forms.

```python
from typing import Any

from beartype import beartype
from expression import Error, Ok
from expression.collections import Block, Map
from msgspec import Struct

from rasm.data.tabular.interop import Backend, FieldBreach, FieldShape, FrameInterop
from rasm.runtime.faults import Disposition, FAULT_CONF, RuntimeResult, traversed


# --- [MODELS] ---------------------------------------------------------------------------


class AdmittedFrame(Struct, frozen=True):
    frame: Any
    backend: Backend
    shapes: tuple[FieldShape, ...]


# --- [SERVICES] -------------------------------------------------------------------------


class FrameAdmission(Struct, frozen=True):
    interop: FrameInterop
    required: tuple[FieldShape, ...]
    quality: "DataQuality"

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, interop: FrameInterop, required: tuple[FieldShape, ...], *rules: QualityRule) -> "FrameAdmission":
        return cls(interop=interop, required=required, quality=DataQuality.of(*rules))

    @beartype(conf=FAULT_CONF)
    def admit(self, frame: Any) -> RuntimeResult[AdmittedFrame]:
        return self.interop.schema_of(frame).bind(lambda shapes: self._resolve(frame, shapes))

    def enforce(self, admitted: AdmittedFrame) -> RuntimeResult[ContractClaim]:
        return self.interop.translate(admitted.frame, Backend.POLARS).bind(lambda lowered: self.quality.validate(lowered.frame.lazy()))

    def _resolve(self, frame: Any, shapes: tuple[FieldShape, ...]) -> RuntimeResult[AdmittedFrame]:
        live = Map.of_seq((s.field, s) for s in shapes)
        censused = Block.of_seq(
            required.resolve(live).map(_breached).default_value(Ok(required)) for required in self.required
        )
        return traversed(censused, by=Disposition.ACCUMULATE).map(
            lambda _conformant: AdmittedFrame(frame=frame, backend=self.interop.source, shapes=shapes)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _breached(breach: FieldBreach) -> RuntimeResult[FieldShape]:
    return Error(ADMIT_BREACH.raised(breach.field, breach.kind.value, breach.declared, breach.observed))
```

## [04]-[COLLECTION]

- Owner: `FrameCovenant` over `dataframely.Collection`; `RelationEdge` the row family modeling one foreign-key covenant between two named member frames (left, right, shared `on` keys, a closed `RelationCardinality`), folded into one `dy.Collection` subclass whose `@dy.filter` methods return the `require_relationship_*` keep-set. A new covenant is one `RelationEdge` row. The covenant composes `tabular/interop#INTEROP` `FrameInterop` as the one backend-agnostic lowering boundary — every member lowers to a polars `LazyFrame` through `FrameInterop.translate(frame, Backend.POLARS)`, so a `narwhals.Implementation.DUCKDB` relation member admits through the same axis. The covenant `ContractClaim` (`subject="data-covenant"`) records the system-of-frames contract and never enforces.
- Cases: `RelationCardinality` is a `StrEnum` whose value IS the exact `dataframely` builder name; `relate` resolves the bound builder by one `getattr(dy, self.value)` at boundary scope. The enum value is the dispatch key, so a new cardinality is one enum row whose value names its builder, never a switch arm plus a table entry. `ContractIo` and `Restore` carry behavior the same way — one `round_trip`/`prove` method each, closed by `assert_never` — because a module-scope row table keyed on a closed family both restates its cases and reifies the lazy `dataframely`/`polars` proxies at import.
- Law: a covenant member is one admitted frame carried by name; its `ContentKey` composes a sibling content-keyed bundle by reference (`tabular/materialize#MATERIALIZE` `PartitionBundle.content_key`, `spatial/catalog#CATALOG` `StacDiscovery.content_key`, or any `ContentKey`-bearing frame), read off the sibling owner, never re-minted. `CovenantMember` pairs name, frame, `ContentKey`, and a `MemberPolicy` folding the `dataframely` `CollectionMember` per-member axes (`ignored_in_filters`/`propagate_row_failures`/`inline_for_sampling`); `MemberPolicy.member()` fuses onto the member's `dy.LazyFrame[S]` annotation as `Annotated[dy.LazyFrame[S], policy.member()]`, riding the annotation the dataframely metaclass reads, never a parallel class-attribute.
- Entry: `run` binds the result-typed `ContentIdentity.of("covenant", member_keys)` Merkle key, lowers every member once through `_lower` (`FrameInterop.translate(frame, Backend.POLARS)` per member `traversed` `by=Disposition.ABORT`, so the first lowering fault aborts the run), then dispatches over the `CovenantOp` union — `Prove`/`Consistent`/`Restrict`/`Extend`/`Persist`/`Contract`/`Sample` — through one `boundary(COVENANT_RUN, …, catch=_covenant_raises())` `match` closed by `assert_never`. The lowered carrier is the ordered `(name, frame)` `Block` rather than a name-keyed dict so `Extend` runs survive a member name repeated across runs; single-occurrence arms project `dict(pairs)` once. The `cast` policy field threads into every `filter`/`validate`/`is_valid`/`cast` call as the one dtype-coercion knob, and `dy.Config(max_failure_examples=self.failure_examples)` bounds the captured example budget. `Prove` runs `filter(data, cast=, eager=True)`, splitting each member into `(valid, FailureInfo)` so a violation lands in one `CollectionFilterResult` without raising. `Consistent` folds the `is_valid` bool into a status-only claim rather than leaking a bare `bool`. `Restrict` derives the cross-member key off `collection.common_primary_key()` rather than a parallel `keys` parameter, composing `validate`→`join(anchor_keys, how="semi")`→`collect_all`. `Extend` folds an accreting series of runs through `concat_collection_members`, the `_runs` slicer cutting the ordered pair `Block` back to per-run slices by member count so each run keeps its own frames even when runs share names, casting each run to its schema before unioning run-wise. `Persist` proves the artifact itself over one directory, one `ContractIo` direction, and one `Restore` disposition: `Parquet` writes through `Collection.write_parquet`, the one provider writer stamping the serialized covenant into every `<member>.parquet`, and re-scans under `validation="skip"`; `Delta` writes each member through `polars.DataFrame.write_delta` under its own `<member>` leaf and scans each back through `polars.scan_delta`, the folder's OWN durable format. Restoring grades unconditionally — `_claim` runs the same member-schema and cross-member `@filter` algebra over every restored frame and folds each violation into the breach stream, so validation is stricter than the read-time argument that ran it only on a metadata miss, and it regrades under the owner's own `cast` rather than the `cast=True` that argument forced. `round_trip` answers separately whether the artifact's own metadata PROVES this covenant — every member file's stamp for parquet, never for delta, where polars owns the commit — and `Restore` decides what an unproven artifact costs: `SKIP` reads no footers at all, `ALLOW` trusts silently, `WARN` meters one `domain="contract"` point, `FORBID` records one `("restore", "contract", "unproven")` breach. `Contract` proves the `serialize`→`deserialize_collection`→`matches` round-trip as its own verb over members alone (a `None` restore or structural mismatch is one `("contract", "round-trip")` breach), carrying no directory or sink slot for its arm to ignore. Frame persistence keeps the `sink` slot, which lands each member's rejected rows under a `_failures/<member>` leaf of that same directory and names the leaf on the claim. `Sample` gates the `sample(num_rows, generator=)` synthetic system so a sampled covenant is self-consistent by construction. Every verb returns one `RuntimeResult[ContractClaim]`.
- Auto: a pass yields `ContractClaim.of("data-covenant", (members, edges), (), key)`; a failure folds each member's `FailureInfo` into one breach stream carrying four kinds under a slot discriminant — `(member, "rule", ...)` from `counts()`, `(member, "co-occur", ...)` from `cooccurrence_counts()`, `(member, "detail", column, ...)` from the `details()` per-rule frame naming which column drove each rejection, and `(member, "invalid", "rows", ...)` from `invalid().height` totaling rejected rows — read off the dataframely result, never re-derived. A `Persist` carrying its sink adds a fifth kind, `(member, "sunk", leaf, rows)`, naming where that member's rejected FRAMES landed, emitted only where a member rejected something so a passing claim keeps its `PASSED` status. `Restore.FORBID` prepends the one artifact-scoped kind, `("restore", "contract", "unproven")`, which names no member because the missing stamp is a property of the artifact rather than of any frame inside it. `invalid()` and `details()` are `FailureInfo` methods bound once per member, never property reads; `CollectionFilterResult.failure` is the per-member `FailureInfo` map (singular, keyed by member name), never a `failures` plural. The runtime-synthesized `type("Covenant", (dy.Collection,), namespace)` is admitted by the dataframely metaclass directly (member `__annotations__` plus `dy.filter()`-decorated edges enforcing the shared-primary-key invariant), so no literal `class` body is required.
- Packages: `dataframely` supplies the collection covenant and `polars` every frame write and scan the provider deleted, both deferring through one module-scope lazy import; `numpy` stays type-only; `expression` owns the tagged operation, immutable maps, and traversal results; `msgspec` owns frozen rows; `beartype` guards the public factories; runtime owns fault, identity, metric, result, and scoped-hook surfaces.
- Growth: a new covenant is one `RelationEdge`; a new cardinality one `RelationCardinality` row whose value names its builder; a new backend member admitted free by the `Backend` axis; a new produced-frame member one `CovenantMember` carrying its sibling-owned `ContentKey`; a new member-participation rule one `MemberPolicy` row; a cross-member grouped invariant beyond foreign-key cardinality one `@dy.filter` keep-set; a new breach diagnostic one slot row on the stream; a `cast`/`failure_examples` knob one policy field threaded into the existing call; a new verb one `CovenantOp` case; a new IO direction one `ContractIo` case owning its `round_trip` arm; a new unproven-artifact disposition one `Restore` case owning its `prove` arm.
- Boundary: `dataframely` owns the Polars-native cross-frame integrity, `polars` every frame read and write beneath it, `FrameInterop.translate` every member lowering; no raising in domain logic. An inline `narwhals.from_native(...).to_polars().lazy()` lowering, a hand-stitched anti/semi-join where `require_relationship_*` owns integrity, a second `ContentIdentity` mint over a sibling-owned key, a per-cardinality filter family, a per-verb method tree, a per-arm re-lowering, a re-derived member key, a covenant type re-synthesized per helper, a hardwired per-arm `cast=False`, a parallel gate or per-kind breach record, a second claim type, a heterogeneous `RuntimeResult[ContractClaim | bool | bytes]` outcome union, a `Schema`- or `Collection`-tier delta call, a read delegating validation to a `validation=` argument, a restore dropping the grading that argument ran, a module-scope row table keyed on `ContractIo`/`Restore`, a union case carrying slots its own arm ignores, and an undecorated `of`/`run` are the rejected forms.

```python
from collections.abc import Iterator, Mapping
from contextlib import nullcontext
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Any, Literal, assert_never

lazy import dataframely as dy
lazy import polars as pl

from beartype import beartype
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.data.tabular.interop import Backend, FrameInterop
from rasm.runtime.hooks import Hooks
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import Catch, Disposition, FAULT_CONF, RuntimeResult, boundary, traversed
from rasm.runtime.metrics import Metrics
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

if TYPE_CHECKING:
    import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type Frames = dict[str, "pl.DataFrame | pl.LazyFrame"]

# --- [MODELS] ---------------------------------------------------------------------------


class RelationCardinality(StrEnum):
    ONE_TO_ONE = "require_relationship_one_to_one"
    ONE_TO_AT_LEAST_ONE = "require_relationship_one_to_at_least_one"

    def relate(self, lhs: "pl.LazyFrame", rhs: "pl.LazyFrame", on: tuple[str, ...]) -> "pl.LazyFrame":
        return getattr(dy, self.value)(lhs, rhs, on=list(on), drop_duplicates=True)


class ContractIo(StrEnum):
    PARQUET = "parquet"
    DELTA = "delta"

    def round_trip(self, validated: "dy.Collection", directory: str, probe: bool) -> "tuple[Frames, bool]":
        covenant = type(validated)
        match self:
            case ContractIo.PARQUET:
                validated.write_parquet(directory)
                stamps = (dy.read_parquet_metadata_collection(f"{directory}/{name}.parquet") for name in covenant.member_schemas())
                proven = probe and all(stamp is not None and stamp.matches(covenant) for stamp in stamps)
                return _members(covenant.scan_parquet(directory, validation="skip")), proven
            case ContractIo.DELTA:
                for name, frame in _members(validated).items():
                    frame.write_delta(f"{directory}/{name}")
                return {name: pl.scan_delta(f"{directory}/{name}") for name in covenant.member_schemas()}, False
            case unreachable:
                assert_never(unreachable)


class Restore(StrEnum):
    SKIP = "skip"
    ALLOW = "allow"
    WARN = "warn"
    FORBID = "forbid"

    def prove(self, io: ContractIo, validated: "dy.Collection", directory: str) -> "tuple[Frames, tuple[tuple[str, ...], ...]]":
        restored, proven = io.round_trip(validated, directory, probe=self is not Restore.SKIP)
        if proven:
            return restored, ()
        match self:
            case Restore.SKIP | Restore.ALLOW:
                return restored, ()
            case Restore.WARN:
                Metrics.record({"rasm.contract.unproven": 1.0}, domain="contract", kind=self)
                return restored, ()
            case Restore.FORBID:
                return restored, (("restore", "contract", "unproven"),)
            case unreachable:
                assert_never(unreachable)


class RelationEdge(Struct, frozen=True):
    name: str
    left: str
    right: str
    on: tuple[str, ...]
    kind: RelationCardinality = RelationCardinality.ONE_TO_ONE


class MemberPolicy(Struct, frozen=True):
    ignored_in_filters: bool = False
    propagate_row_failures: bool = True
    inline_for_sampling: bool = False

    def member(self) -> "dy.CollectionMember":
        return dy.CollectionMember(
            ignored_in_filters=self.ignored_in_filters,
            propagate_row_failures=self.propagate_row_failures,
            inline_for_sampling=self.inline_for_sampling,
        )


class CovenantMember(Struct, frozen=True):
    name: str
    frame: Any
    content_key: ContentKey
    policy: MemberPolicy = MemberPolicy()


@tagged_union(frozen=True)
class CovenantOp:
    tag: Literal["prove", "consistent", "restrict", "extend", "persist", "contract", "sample"] = tag()
    prove: tuple[CovenantMember, ...] = case()
    consistent: tuple[CovenantMember, ...] = case()
    restrict: tuple[str, tuple[CovenantMember, ...]] = case()
    extend: tuple[tuple[CovenantMember, ...], ...] = case()
    persist: tuple[ContractIo, str, bool, tuple[CovenantMember, ...]] = case()
    contract: tuple[CovenantMember, ...] = case()
    sample: tuple[int, tuple[CovenantMember, ...]] = case()

    @property
    def members(self) -> tuple[CovenantMember, ...]:
        match self:
            case (
                CovenantOp(tag="prove", prove=members)
                | CovenantOp(tag="consistent", consistent=members)
                | CovenantOp(tag="restrict", restrict=(_, members))
                | CovenantOp(tag="persist", persist=(_, _, _, members))
                | CovenantOp(tag="contract", contract=members)
                | CovenantOp(tag="sample", sample=(_, members))
            ):
                return members
            case CovenantOp(tag="extend", extend=runs):
                return tuple(m for run in runs for m in run)
            case unreachable:
                assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class FrameCovenant(Struct, frozen=True):
    interop: FrameInterop
    edges: tuple[RelationEdge, ...]
    schemas: "Map[str, type[dy.Schema]]"
    restore: Restore = Restore.WARN
    cast: bool = False
    failure_examples: int | None = None
    generator: "np.random.Generator | None" = None
    scope: ScopeKey = DEFAULT_SCOPE

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(
        cls,
        interop: FrameInterop,
        schemas: "Mapping[str, type[dy.Schema]]",
        *edges: RelationEdge,
        restore: Restore = Restore.WARN,
        cast: bool = False,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> "FrameCovenant":
        return cls(interop=interop, edges=edges, schemas=Map.of_seq(schemas.items()), restore=restore, cast=cast, scope=scope)

    @beartype(conf=FAULT_CONF)
    def run(self, op: CovenantOp) -> "RuntimeResult[ContractClaim]":
        return (
            ContentIdentity.of("covenant", tuple(m.content_key for m in op.members))
            .bind(
                lambda key: self._lower(op.members).bind(
                    lambda pairs: boundary(COVENANT_RUN, lambda: self._dispatch(op, pairs, key), catch=_covenant_raises())
                )
            )
            .bind(lambda claim: Hooks.fire(VERDICT_POINT.id, claim, scope=self.scope))
        )

    def _dispatch(self, op: CovenantOp, pairs: "Block[tuple[str, pl.LazyFrame]]", key: ContentKey) -> ContractClaim:
        with dy.Config(max_failure_examples=self.failure_examples) if self.failure_examples is not None else nullcontext():
            match op:
                case CovenantOp(tag="prove", prove=members):
                    return self._claim(self._collection(members), dict(pairs), key)
                case CovenantOp(tag="consistent", consistent=members):
                    passed = self._collection(members).is_valid(dict(pairs), cast=self.cast)
                    return ContractClaim.of(
                        "data-covenant", (len(self.schemas), len(self.edges)), () if passed else (("collection", "consistent"),), key
                    )
                case CovenantOp(tag="restrict", restrict=(anchor, members)):
                    covenant = self._collection(members)
                    data = dict(pairs)
                    keys = data[anchor].select(covenant.common_primary_key())
                    restricted = covenant.validate(data, cast=self.cast).join(keys, how="semi", maintain_order="none").collect_all()
                    return self._claim(covenant, _members(restricted), key)
                case CovenantOp(tag="extend", extend=runs):
                    return self._claim(
                        self._collection(tuple(m for run in runs for m in run)),
                        dy.concat_collection_members([self._collection(run).cast(dict(run_pairs)) for run, run_pairs in _runs(runs, pairs)]),
                        key,
                    )
                case CovenantOp(tag="persist", persist=(io, directory, sink, members)):
                    covenant = self._collection(members)
                    validated = covenant.validate(dict(pairs), cast=self.cast, eager=True)
                    restored, unproven = self.restore.prove(io, validated, directory)
                    return self._claim(covenant, restored, key, directory if sink else None, unproven)
                case CovenantOp(tag="contract", contract=members):
                    covenant = self._collection(members)
                    deserialized = dy.deserialize_collection(covenant.serialize())
                    matched = deserialized is not None and deserialized.matches(covenant)
                    return ContractClaim.of(
                        "data-covenant", (len(self.schemas), len(self.edges)), () if matched else (("contract", "round-trip"),), key
                    )
                case CovenantOp(tag="sample", sample=(rows, members)):
                    covenant = self._collection(members)
                    return self._claim(covenant, _members(covenant.sample(rows, generator=self.generator)), key)
                case unreachable:
                    assert_never(unreachable)

    def _collection(self, members: "tuple[CovenantMember, ...]") -> "type[dy.Collection]":
        policy = {m.name: m.policy for m in members}
        namespace: dict[str, Any] = {
            "__annotations__": {
                name: Annotated[dy.LazyFrame[schema], policy.get(name, MemberPolicy()).member()] for name, schema in self.schemas.items()
            },
            **{
                edge.name: dy.filter()(
                    lambda collection, _edge=edge: _edge.kind.relate(getattr(collection, _edge.left), getattr(collection, _edge.right), _edge.on)
                )
                for edge in self.edges
            },
        }
        return type("Covenant", (dy.Collection,), namespace)

    def _lower(self, members: "tuple[CovenantMember, ...]") -> "RuntimeResult[Block[tuple[str, pl.LazyFrame]]]":
        return traversed(
            Block.of_seq(self.interop.translate(m.frame, Backend.POLARS).map(lambda t, _name=m.name: (_name, t.frame.lazy())) for m in members),
            by=Disposition.ABORT,
        )

    def _claim(
        self,
        covenant: "type[dy.Collection]",
        data: Frames,
        key: ContentKey,
        sink: str | None = None,
        prior: tuple[tuple[str, ...], ...] = (),
    ) -> ContractClaim:
        result = covenant.filter(data, cast=self.cast, eager=True)
        sunk = Block.empty() if sink is None else Block.of_seq(sorted(result.failure)).choose(lambda name: _sunk(sink, name, result.failure[name]))
        breaches = tuple(
            row
            for name, failure in result.failure.items()
            for invalid, details in ((failure.invalid(), failure.details()),)
            for row in (
                *((name, "rule", rule, str(count)) for rule, count in failure.counts().items()),
                *((name, "co-occur", "|".join(sorted(ruleset)), str(count)) for ruleset, count in failure.cooccurrence_counts().items()),
                *(
                    (name, "detail", column, str(details.filter(details[column] == "invalid").height))
                    for column in details.columns
                    if column not in invalid.columns
                ),
                *(((name, "invalid", "rows", str(invalid.height)),) if invalid.height else ()),
            )
        )
        return ContractClaim.of("data-covenant", (len(self.schemas), len(self.edges)), (*prior, *breaches, *sunk), key)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _covenant_raises() -> Catch:
    return (dy.exc.ValidationError, dy.exc.SchemaError, dy.exc.ImplementationError, pl.exceptions.PolarsError, OSError)


def _sunk(sink: str, name: str, failure: "dy.FailureInfo") -> "Option[tuple[str, str, str, str]]":
    rejected = failure.invalid()
    if not rejected.height:
        return Nothing
    leaf = f"{sink}/_failures/{name}"
    failure.write_parquet(leaf)
    return Some((name, "sunk", leaf, str(rejected.height)))


def _members(collection: "dy.Collection") -> Frames:
    return {name: getattr(collection, name) for name in type(collection).member_schemas()}


def _runs(
    runs: "tuple[tuple[CovenantMember, ...], ...]", pairs: "Block[tuple[str, pl.LazyFrame]]"
) -> "Iterator[tuple[tuple[CovenantMember, ...], tuple[tuple[str, pl.LazyFrame], ...]]]":
    cursor = 0
    for run in runs:
        yield run, tuple(pairs[cursor : cursor + len(run)])
        cursor += len(run)
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
