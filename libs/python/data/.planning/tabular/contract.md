# [PY_DATA_CONTRACT]

The data-contract owner: a quality gate, a structural admission path, and a cross-frame referential covenant on one page. `DataQuality` folds IDS-style `QualityRule` rows into one `pandera.polars` schema; `FrameAdmission` proves required `FieldShape`s resolve against the live agnostic schema before routing enforcement to that gate; `FrameCovenant` folds `RelationEdge` rows into one `dataframely` collection over the integrity of a system of produced frames. Every contract records into one `ContractClaim` — discriminated by its `subject` literal (`data-quality`/`data-covenant`) — and never raises: enforcement is the caller's `match` on `ContractClaim.status`. There is exactly one `ContractClaim`, one `ClaimStatus`, one pandera gate, and one dataframely collection for the whole package.

`FieldShape` is declared by its minter, the `tabular/interop#INTEROP` `FrameInterop.schema_of` owner, and imported strictly downward beside `Backend`/`FrameInterop` — interop is earlier in the `[00]` order, so the edge is one-way and this page holds no `TYPE_CHECKING` guard, function-local dodge, or second declaration. The covenant `ContractClaim` keys off `ContentIdentity` Merkle-folding the admitted member content-keys, composing the `tabular/materialize#MATERIALIZE` `PartitionBundle` and `spatial/catalog#CATALOG` `StacDiscovery` fingerprints by reference without re-minting them.

## [01]-[INDEX]

- [01]-[QUALITY]: the pandera data-quality gate, the recorded non-enforcing schema claim.
- [02]-[ADMISSION]: structural field shapes, the narwhals frame-admission route into the gate.
- [03]-[COLLECTION]: the dataframely cross-frame referential covenant, the Merkle-keyed collection claim.

## [02]-[QUALITY]

- Owner: `DataQuality` over `pandera.polars`; `QualityRule` the row family modeling one column claim (dtype/nullable/unique/required plus a closed `CheckKind` predicate set), folded into one `DataFrameSchema`. A new validation is one `QualityRule` row.
- Cases: `CheckKind` is the one predicate axis — every case maps to a concrete `pandera.Check` through one of four `expression.collections.Map` behavior tables (`_CMP`/`_SET`/`_TEXT`/`_INCLUSIVE`), so the IDS-style rule vocabulary is one closed switch, never a per-check builder. `length` threads its `int | None` bounds into `Check.str_length`, the distinct numeric-bound case the `str`-pattern `_TEXT` table cannot own (its values type-mismatch a length bound). `unique` is the `QualityRule.unique` column flag, not a `CheckKind` case — pandera routes uniqueness to `Column(unique=)`, never a `Check`, so the axis stays total over real `Check`s and `to_check` never returns `None`.
- Entry: `DataQuality.of` carries the validation policy (`lazy`/`sample`/`seed`) as frozen owner fields, so `validate(frame)` is one modal entrypoint that never grows a per-call disposition or sampling knob. The content key derives off `ContentIdentity.of("schema", self._wire())` — the canonical msgspec-JSON fingerprint over the rule fields plus a policy header — so two owners with identical rules but differing policy never collide onto one key. `lazy=True` raises `SchemaErrors` with the full `failure_cases` frame (accumulate), `lazy=False` the first `SchemaError` (abort), the disposition fixed once on the owner; `sample`/`seed` restrict validation to a deterministic row subset, the pandera large-frame sampling policy. The rail is `Ok` even on validation failure, `Error` only when the collect or the key derivation faults.
- Auto: a pass yields `ContractClaim.of("data-quality", (columns,), (), key)` at `PASSED`; a failing lazy validation folds the `SchemaErrors.failure_cases` `column`/`check` pairs into `breaches`, `FAILED` deriving from the non-empty tuple. The frame stays lazy through admission and collects to eager once at the gate, the only point the polars backend surfaces a breach.
- Receipt: `ContractClaim.contribute` emits evidence keyed by the schema fingerprint; every settled claim fires `VERDICT_POINT` in its owner scope, so composition-root telemetry taps project the same fact.
- Packages: `pandera` (`DataFrameSchema`/`Column(unique=)`/`Check.ge`/`le`/`gt`/`lt`/`equal_to`/`not_equal_to`/`in_range(min_value=, max_value=, include_min=, include_max=)`/`isin`/`notin`/`str_matches`/`str_contains`/`str_length(min_value=, max_value=)`/`is_monotonic(dim, increasing=)`/`errors.SchemaError`/`SchemaErrors`), `expression` (`tagged_union`/`tag`/`case`, `expression.collections.Map` the four behavior tables), `msgspec` (`Struct` the frozen owners), `polars` (`LazyFrame`, `TYPE_CHECKING`-only — the runtime frame arrives pre-lowered through `narwhals`), `beartype` (`@beartype(conf=FAULT_CONF)` on the public `DataQuality.of` factory), runtime (`RuntimeRail`/`boundary`/`FAULT_CONF`/`ContentIdentity`/`ContentKey`/`Receipt`/`ReceiptContributor`).
- Growth: a new check is one `CheckKind` row threading its `_CMP`/`_SET`/`_TEXT`/`_INCLUSIVE` table; a new column claim is one `QualityRule`; the narwhals-lazy backend is a pandera row on this owner, never a parallel gate.
- Boundary: no raising in domain logic, no global schema registry, no coercion (`coerce=False`); a per-check validator family, an exception-driven gate, and an undecorated `DataQuality.of` are the rejected forms.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from re import Pattern
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import pandera.polars as pap
from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec import json as msgjson
from pandera import Check
from pandera.errors import SchemaError, SchemaErrors

from rasm.data.tabular.interop import Backend, FieldShape, FrameInterop
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

if TYPE_CHECKING:
    import polars as pl

type Cmp = Literal["ge", "le", "gt", "lt", "eq", "ne"]
type Text = Literal["matches", "contains"]
type Inclusive = Literal["both", "neither", "left", "right"]

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

    def contribute(self) -> tuple[Receipt, ...]:
        scope = "/".join(map(str, self.shape))
        facts: dict[str, object] = {"status": self.status, "breaches": len(self.breaches), "key": self.content_key.hex}
        return (Receipt.of(self.subject, ("emitted", f"{self.subject}[{scope}]", facts)),)


# verdict observe edge: every settled claim fires in its composition scope; the data composition fold registers the row.
VERDICT_POINT: Final[HookPoint[ContractClaim]] = HookPoint(id="rasm.data.contract.verdict", payload=ContractClaim, modality=Modality.OBSERVE)


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
        # canonical fingerprint: a policy header (lazy/sample/seed) ahead of one key-sorted
        # msgspec-JSON row per rule, so owners differing only in policy never share a key.
        header = msgjson.encode((self.lazy, self.sample, self.seed), order="deterministic")
        rows = sorted(
            msgjson.encode(
                (r.column, str(r.dtype), r.nullable, r.unique, r.required, [(c.tag, getattr(c, c.tag)) for c in r.checks]),
                order="deterministic",
            )
            for r in self.rules
        )
        return b"\n".join((header, *rows))

    def validate(self, frame: pl.LazyFrame) -> "RuntimeRail[ContractClaim]":
        # the settled claim fires the verdict OBSERVE point — the fired fact passes through untouched on the rail.
        schema = self._schema()
        return (
            ContentIdentity.of("schema", self._wire())
            .bind(lambda key: boundary("quality.validate", lambda: self._validate(schema, frame, key)))
            .bind(lambda claim: Hooks.fire(VERDICT_POINT.id, claim, scope=self.scope))
        )

    def _validate(self, schema: pap.DataFrameSchema, frame: pl.LazyFrame, key: ContentKey) -> ContractClaim:
        # pandera.polars defers a LazyFrame validation unraised, so the lazy plan collects once here.
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
- Entry: `admit` resolves the live shapes through one `FrameInterop.schema_of` call — the single backend-agnostic derivation reading the per-column null-mask via `null_count()`, never a second inline `collect_schema()` — then folds the resolved `FieldShape` tuple against the required shapes through `FieldShape.resolve`: a required field absent, carrying a non-matching `logical_type`, or declaring `nullable=False` where the live mask observed nulls is one breach token, so a present-but-wrong-dtype field is a structural breach, not a silent pass. `schema_of` lifts through `narwhals.from_native` inside its own `boundary`, so admission binds the sibling rail and the backend rides the `narwhals.Implementation` axis — one path for every backend. `enforce` routes validation to `DataQuality.validate`, lowering through `FrameInterop.translate(frame, Backend.POLARS)` then `.frame.lazy()`, never a second hand-spelled `to_polars().lazy()`.
- Packages: `tabular/interop#INTEROP` (`FieldShape`/`Backend`/`FrameInterop.schema_of`/`translate`/`source` — one module-top prelude importing the strictly-earlier interop module downward), `expression` (`Error`/`Ok` the breach-rail arms, `expression.collections.Map`/`Map.of_seq` the live `field->FieldShape` resolve map), `msgspec` (`Struct` the frozen owners), `beartype` (`@beartype(conf=FAULT_CONF)` on `FrameAdmission.of` and the caller-facing `admit`), runtime (`RuntimeRail`/`BoundaryFault`/`FAULT_CONF`). The admit path holds no inline `boundary` since `schema_of`/`translate` carry their own fences and `_resolve` projects the structural verdict directly through `Ok`/`Error(BoundaryFault(...))`, never a no-op thunk wrapping an already-built value.
- Growth: a new structural attribute is one column on `FieldShape` read once by the interop `schema_of` owner; a new quality rule is one `QualityRule`/`CheckKind` row on `DataQuality`; a new backend is admitted free by the interop `Backend` axis.
- Boundary: no Persistence migration law, no live Rhino/GH mutation; a hand-rolled validation loop, a per-backend admission branch, a second inline `collect_schema()` derivation, a presence-only check that passes a wrong-dtype field, a duplicate `ContractClaim`, a second pandera gate, a `boundary("frame.admit", ...)` no-op thunk over an already-built verdict, and an undecorated `of`/`admit` are the rejected forms.

```python signature
from typing import Any

from beartype import beartype
from expression import Error, Ok
from expression.collections import Map
from msgspec import Struct

from rasm.data.tabular.interop import Backend, FieldShape, FrameInterop
from rasm.runtime.faults import BoundaryFault, FAULT_CONF, RuntimeRail


class AdmittedFrame(Struct, frozen=True):
    frame: Any
    backend: Backend
    shapes: tuple[FieldShape, ...]


class FrameAdmission(Struct, frozen=True):
    interop: FrameInterop
    required: tuple[FieldShape, ...]
    quality: "DataQuality"

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, interop: FrameInterop, required: tuple[FieldShape, ...], *rules: QualityRule) -> "FrameAdmission":
        return cls(interop=interop, required=required, quality=DataQuality.of(*rules))

    @beartype(conf=FAULT_CONF)
    def admit(self, frame: Any) -> RuntimeRail[AdmittedFrame]:
        return self.interop.schema_of(frame).bind(lambda shapes: self._resolve(frame, shapes))

    def enforce(self, admitted: AdmittedFrame) -> RuntimeRail[ContractClaim]:
        return self.interop.translate(admitted.frame, Backend.POLARS).bind(lambda lowered: self.quality.validate(lowered.frame.lazy()))

    def _resolve(self, frame: Any, shapes: tuple[FieldShape, ...]) -> RuntimeRail[AdmittedFrame]:
        live = Map.of_seq((s.field, s) for s in shapes)
        breach = ", ".join(token for s in self.required if (token := s.resolve(live)))
        return (
            Ok(AdmittedFrame(frame=frame, backend=self.interop.source, shapes=shapes))
            if not breach
            else Error(BoundaryFault(boundary=("frame.admit", breach)))
        )
```

## [04]-[COLLECTION]

- Owner: `FrameCovenant` over `dataframely.Collection`; `RelationEdge` the row family modeling one foreign-key covenant between two named member frames (left, right, shared `on` keys, a closed `RelationCardinality`), folded into one `dy.Collection` subclass whose `@dy.filter` methods return the `require_relationship_*` keep-set. A new covenant is one `RelationEdge` row. The covenant composes `tabular/interop#INTEROP` `FrameInterop` as the one backend-agnostic lowering seam — every member lowers to a polars `LazyFrame` through `FrameInterop.translate(frame, Backend.POLARS)`, so a `narwhals.Implementation.DUCKDB` relation member admits through the same axis. The covenant `ContractClaim` (`subject="data-covenant"`) records the system-of-frames contract and never enforces.
- Cases: `RelationCardinality` is a `StrEnum` whose value IS the exact `dataframely` builder name; `relate` resolves the bound builder by one `getattr(dy, self.value)` at boundary scope. The enum value is the dispatch key, so a new cardinality is one enum row whose value names its builder, never a switch arm plus a table entry.
- Member: a covenant member is one admitted frame carried by name; its `ContentKey` composes a sibling content-keyed bundle by reference (`tabular/materialize#MATERIALIZE` `PartitionBundle.content_key`, `spatial/catalog#CATALOG` `StacDiscovery.content_key`, or any `ContentKey`-bearing frame), read off the sibling owner, never re-minted. `CovenantMember` pairs name, frame, `ContentKey`, and a `MemberPolicy` folding the `dataframely` `CollectionMember` per-member axes (`ignored_in_filters`/`propagate_row_failures`/`inline_for_sampling`); `MemberPolicy.member()` fuses onto the member's `dy.LazyFrame[S]` annotation as `Annotated[dy.LazyFrame[S], policy.member()]`, riding the annotation the dataframely metaclass reads, never a parallel class-attribute.
- Entry: `run` binds the railed `ContentIdentity.of("covenant", member_keys)` Merkle key, lowers every member once through `_lower` (`FrameInterop.translate(frame, Backend.POLARS)` per member `traversed` `by=Disposition.ABORT`, so the first lowering fault aborts the run), then dispatches over the `CovenantOp` union — `Prove`/`Consistent`/`Restrict`/`Extend`/`Persist`/`Sample` — through one `boundary(f"covenant.{op.tag}")` `match` closed by `assert_never`. The lowered carrier is the ordered `(name, frame)` `Block` rather than a name-keyed dict so `Extend` runs survive a member name repeated across runs; single-occurrence arms project `dict(pairs)` once. The `cast` policy field threads into every `filter`/`validate`/`is_valid`/`cast` call as the one dtype-coercion knob, and `dy.Config(max_failure_examples=self.failure_examples)` bounds the captured example budget. `Prove` runs `filter(data, cast=, eager=True)`, splitting each member into `(valid, FailureInfo)` so a violation lands in one `CollectionFilterResult` without raising. `Consistent` folds the `is_valid` bool into a status-only claim rather than leaking a bare `bool`. `Restrict` derives the cross-member key off `collection.common_primary_key()` rather than a parallel `keys` parameter, composing `validate`→`join(anchor_keys, how="semi")`→`collect_all`. `Extend` folds an accreting series of runs through `concat_collection_members`, the `_runs` slicer cutting the ordered pair `Block` back to per-run slices by member count so each run keeps its own frames even when runs share names, casting each run to its schema before unioning run-wise. `Persist` proves the artifact itself: `Frames` re-scans the `write_parquet`/`scan_parquet` round-trip by the `Validation` policy row (`"allow"`/`"warn"`/`"skip"`), `Contract` proves the `serialize`→`deserialize_collection`→`matches` round-trip (a `None` restore or structural mismatch is one `("contract", "round-trip")` breach). `Sample` gates the `sample(num_rows, generator=)` synthetic system so a sampled covenant is self-consistent by construction. Every verb returns one `RuntimeRail[ContractClaim]`.
- Auto: a pass yields `ContractClaim.of("data-covenant", (members, edges), (), key)`; a failure folds each member's `FailureInfo` into one breach stream carrying four kinds under a slot discriminant — `(member, "rule", ...)` from `counts()`, `(member, "co-occur", ...)` from `cooccurrence_counts()`, `(member, "detail", column, ...)` from the `details()` per-rule frame naming which column drove each rejection, and `(member, "invalid", "rows", ...)` from `invalid().height` totaling rejected rows — read off the dataframely receipt, never re-derived. `invalid()` and `details()` are `FailureInfo` methods bound once per member, never property reads; `CollectionFilterResult.failure` is the per-member `FailureInfo` map (singular, keyed by member name), never a `failures` plural. The runtime-synthesized `type("Covenant", (dy.Collection,), namespace)` is admitted by the dataframely metaclass directly (member `__annotations__` plus `dy.filter()`-decorated edges enforcing the shared-primary-key invariant), so no literal `class` body is required.
- Receipt: the covenant `ContractClaim` keys off the railed `ContentIdentity.of` Merkle-fold over the admitted member `ContentKey`s (the same composition the `tabular/materialize#MATERIALIZE` `snapshot_key` folds), resolved once on the `run` rail and threaded into every arm, so a single changed member flips the covenant key while the rest stay byte-stable. `contribute` emits the emitted-phase system-of-frames evidence, never replacing the typed `QueryReceipt` and never re-minting a member's content-key.
- Packages: `dataframely` supplies the collection covenant and defers through one module-scope lazy import; `polars` and `numpy` stay type-only; `expression` owns the tagged operation, immutable maps, and traversal rail; `msgspec` owns frozen rows; `beartype` guards the public factories; runtime owns fault, identity, receipt, and scoped-hook surfaces.
- Growth: a new covenant is one `RelationEdge`; a new cardinality one `RelationCardinality` row whose value names its builder; a new backend member admitted free by the `Backend` axis; a new produced-frame member one `CovenantMember` carrying its sibling-owned `ContentKey`; a new member-participation rule one `MemberPolicy` row; a cross-member grouped invariant beyond foreign-key cardinality one `@dy.filter` keep-set; a new breach diagnostic one slot row on the stream; a `cast`/`failure_examples` knob one policy field threaded into the existing call; a new verb one `CovenantOp` case; a new IO direction one `ContractIo` case on `Persist`.
- Boundary: `dataframely` owns the Polars-native cross-frame integrity; `FrameInterop.translate` owns every member lowering; no raising in domain logic. An inline `narwhals.from_native(...).to_polars().lazy()` lowering, a hand-stitched anti/semi-join where `require_relationship_*` owns integrity, a second `ContentIdentity` mint over a sibling-owned key, a per-cardinality filter family, a per-verb method tree, a per-arm re-lowering, a re-derived member key, a hardwired per-arm `cast=False`, a parallel gate or per-kind breach record, a second claim type, a heterogeneous `RuntimeRail[ContractClaim | bool | bytes]` outcome union, and an undecorated `of`/`run` are the rejected forms.

```python signature
from collections.abc import Iterator, Mapping
from contextlib import nullcontext
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Any, Literal, assert_never

lazy import dataframely as dy

from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.data.tabular.interop import Backend, FrameInterop
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import Disposition, FAULT_CONF, RuntimeRail, boundary, traversed

if TYPE_CHECKING:
    import numpy as np
    import polars as pl

type Validation = Literal["allow", "warn", "skip"]


class RelationCardinality(StrEnum):
    ONE_TO_ONE = "require_relationship_one_to_one"
    ONE_TO_AT_LEAST_ONE = "require_relationship_one_to_at_least_one"

    def relate(self, lhs: "pl.LazyFrame", rhs: "pl.LazyFrame", on: tuple[str, ...]) -> "pl.LazyFrame":
        return getattr(dy, self.value)(lhs, rhs, on=list(on), drop_duplicates=True)


class ContractIo(StrEnum):
    FRAMES = "frames"
    CONTRACT = "contract"


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
    tag: Literal["prove", "consistent", "restrict", "extend", "persist", "sample"] = tag()
    prove: tuple[CovenantMember, ...] = case()
    consistent: tuple[CovenantMember, ...] = case()
    restrict: tuple[str, tuple[CovenantMember, ...]] = case()
    extend: tuple[tuple[CovenantMember, ...], ...] = case()
    persist: tuple[ContractIo, str, tuple[CovenantMember, ...]] = case()
    sample: tuple[int, tuple[CovenantMember, ...]] = case()

    @property
    def members(self) -> tuple[CovenantMember, ...]:
        match self:
            case (
                CovenantOp(tag="prove", prove=members)
                | CovenantOp(tag="consistent", consistent=members)
                | CovenantOp(tag="restrict", restrict=(_, members))
                | CovenantOp(tag="persist", persist=(_, _, members))
                | CovenantOp(tag="sample", sample=(_, members))
            ):
                return members
            case CovenantOp(tag="extend", extend=runs):
                return tuple(m for run in runs for m in run)
            case unreachable:
                assert_never(unreachable)


class FrameCovenant(Struct, frozen=True):
    interop: FrameInterop
    edges: tuple[RelationEdge, ...]
    schemas: "Map[str, type[dy.Schema]]"
    validation: Validation = "warn"
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
        validation: Validation = "warn",
        cast: bool = False,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> "FrameCovenant":
        return cls(interop=interop, edges=edges, schemas=Map.of_seq(schemas.items()), validation=validation, cast=cast, scope=scope)

    @beartype(conf=FAULT_CONF)
    def run(self, op: CovenantOp) -> "RuntimeRail[ContractClaim]":
        # both contract entrypoints converge on one verdict fire — the covenant claim rides the same OBSERVE point.
        return (
            ContentIdentity.of("covenant", tuple(m.content_key for m in op.members))
            .bind(lambda key: self._lower(op.members).bind(lambda pairs: boundary(f"covenant.{op.tag}", lambda: self._dispatch(op, pairs, key))))
            .bind(lambda claim: Hooks.fire(VERDICT_POINT.id, claim, scope=self.scope))
        )

    def _dispatch(self, op: CovenantOp, pairs: "Block[tuple[str, pl.LazyFrame]]", key: ContentKey) -> ContractClaim:
        with dy.Config(max_failure_examples=self.failure_examples) if self.failure_examples is not None else nullcontext():
            match op:
                case CovenantOp(tag="prove", prove=members):
                    return self._claim(members, dict(pairs), key)
                case CovenantOp(tag="consistent", consistent=members):
                    passed = self._collection(members).is_valid(dict(pairs), cast=self.cast)
                    return ContractClaim.of(
                        "data-covenant", (len(self.schemas), len(self.edges)), () if passed else (("collection", "consistent"),), key
                    )
                case CovenantOp(tag="restrict", restrict=(anchor, members)):
                    collection = self._collection(members)
                    data = dict(pairs)
                    keys = data[anchor].select(collection.common_primary_key())
                    restricted = collection.validate(data, cast=self.cast).join(keys, how="semi", maintain_order="none").collect_all()
                    return self._claim(members, _members(restricted), key)
                case CovenantOp(tag="extend", extend=runs):
                    return self._claim(
                        tuple(m for run in runs for m in run),
                        dy.concat_collection_members([self._collection(run).cast(dict(run_pairs)) for run, run_pairs in _runs(runs, pairs)]),
                        key,
                    )
                case CovenantOp(tag="persist", persist=(ContractIo.FRAMES, directory, members)):
                    collection = self._collection(members)
                    collection.validate(dict(pairs), cast=self.cast).write_parquet(directory)
                    return self._claim(members, _members(collection.scan_parquet(directory, validation=self.validation)), key)
                case CovenantOp(tag="persist", persist=(ContractIo.CONTRACT, _, members)):
                    collection = self._collection(members)
                    restored = dy.deserialize_collection(collection.serialize())
                    matched = restored is not None and restored.matches(collection)
                    return ContractClaim.of(
                        "data-covenant", (len(self.schemas), len(self.edges)), () if matched else (("contract", "round-trip"),), key
                    )
                case CovenantOp(tag="sample", sample=(rows, members)):
                    return self._claim(members, _members(self._collection(members).sample(rows, generator=self.generator)), key)
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

    def _lower(self, members: "tuple[CovenantMember, ...]") -> "RuntimeRail[Block[tuple[str, pl.LazyFrame]]]":
        return traversed(
            Block.of_seq(self.interop.translate(m.frame, Backend.POLARS).map(lambda t, _name=m.name: (_name, t.frame.lazy())) for m in members),
            by=Disposition.ABORT,
        )

    def _claim(self, members: "tuple[CovenantMember, ...]", data: "dict[str, pl.LazyFrame]", key: ContentKey) -> ContractClaim:
        result = self._collection(members).filter(data, cast=self.cast, eager=True)
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
        return ContractClaim.of("data-covenant", (len(self.schemas), len(self.edges)), breaches, key)


def _members(collection: "dy.Collection") -> "dict[str, pl.LazyFrame]":
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
