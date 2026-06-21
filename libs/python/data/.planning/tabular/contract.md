# [PY_DATA_CONTRACT]

The data-contract gate plus structural frame admission plus cross-frame referential covenant as one owner: `DataQuality` folds IDS-style `QualityRule` rows into one `pandera` schema, `FrameAdmission` proves required structural `FieldShape`s resolve against the live agnostic schema before routing enforcement to that same `DataQuality`, and `FrameCovenant` folds `RelationEdge` rows into one `dataframely` `dy.Collection` over the integrity of a system of produced frames. One `ContractClaim` — discriminated by its `subject` literal (`data-quality`/`data-covenant`) — records every contract and its breach cases without raising; enforcement is the caller's `match` on `ContractClaim.status`, never an exception. `CheckKind` collapses the IDS-style predicate vocabulary into one tagged union over four `expression.collections.Map` behavior tables; `QualityRule` is one column claim; `unique` rides the `QualityRule` column flag, not the predicate axis. `FieldShape` is a distinct structural shape (field presence plus dtype derived agnostically), not a re-mint of the claim. `RelationCardinality` collapses the dataframely referential-cardinality builder pair (`require_relationship_one_to_one`/`require_relationship_one_to_at_least_one`) into one `StrEnum` over one boundary-resolved builder table; the covenant `ContractClaim` keys off `ContentIdentity` Merkle-folding the admitted member content-keys, composing the `columnar#MATERIALIZE` `PartitionBundle` and `spatial/catalog#CATALOG` `StacDiscovery` fingerprints without re-minting their content-keys. There is exactly one `ContractClaim`, one `ClaimStatus`, one pandera gate, and one dataframely collection for the whole package.

## [01]-[INDEX]

- [01]-[QUALITY]: the data-quality gate over pandera, the recorded non-enforcing schema claim.
- [02]-[ADMISSION]: structural field shapes, narwhals frame admission, the contract route.
- [03]-[COLLECTION]: the cross-frame referential covenant over dataframely, the recorded non-enforcing collection claim Merkle-keyed over member content-keys.

## [02]-[QUALITY]

- Owner: `DataQuality` — the one data-quality validation owner over `pandera.polars`; `QualityRule` the row family modeling one column claim (dtype/nullable/unique/required plus a closed `CheckKind` predicate set), folded into a `pandera.polars.DataFrameSchema`. A new validation is one `QualityRule` row, never a `validate_nullable`/`validate_range`/`validate_unique` method family. `ContractClaim` is the one claim shape both clusters mint, discriminated by its `subject` literal (`data-quality`/`data-covenant`); it records the contract and its breach cases without raising and never enforces. There is exactly one claim type, one `ClaimStatus`, and one `ContractClaim.contribute` receipt path for the whole package.
- Cases: `CheckKind` is the one predicate axis — every case maps to a concrete `pandera.Check`, so the axis never carries a non-predicate. `cmp(op, v)` folds `ge`/`le`/`gt`/`lt`/`eq`/`ne` through one `_CMP` `Map` (`Check.ge`/`le`/`gt`/`lt`/`equal_to`/`not_equal_to`) so six comparison arms collapse to one `_CMP[op]` lookup · `in_range(lo, hi, inc)` reads the `(include_min, include_max)` pair off one `_INCLUSIVE` `Map` keyed by the closed `Inclusive` literal and threads it into `Check.in_range(min_value=, max_value=, include_min=, include_max=)`, so the four endpoint-closure combinations are one table row, never four inline `inc in (...)` membership tests · `member(present, values)` folds `isin`/`notin` through one `_SET` `Map` (`Check.isin`/`Check.notin`) so the membership polarity is a row, never two cases · `text(op, pattern)` folds the `str`-pattern family `matches`/`contains` through one `_TEXT` `Map` (`Check.str_matches`/`Check.str_contains`), both taking the `str | Pattern[str]` pattern · `length(lo, hi)` threads the `int | None` bounds straight into `Check.str_length(min_value=, max_value=)`, the distinct numeric-bound case `str_length` owns (never the `str`-pattern `_TEXT` table, whose values type-mismatch a length bound) · `monotonic(dim, increasing)` threads the required `dim` column name and the `increasing` direction into `Check.is_monotonic(dim, increasing=)`, matched by `match`/`case` closed by `assert_never` into the concrete `pandera.Check`, so the IDS-style rule vocabulary is one closed switch over four `expression.collections.Map` tables, never a per-check builder. `unique` is the `QualityRule.unique` column flag, not a `CheckKind` case — pandera routes uniqueness to `Column(unique=)`, never a `Check`, so the predicate axis stays total over real `Check`s and `to_check` never returns `None`.
- Entry: `DataQuality.of` folds a tuple of `QualityRule` into one `DataFrameSchema` and carries the validation policy (`lazy`/`sample`/`seed`) as frozen owner fields, the same policy-on-the-owner shape the sibling `FrameCovenant` `cast`/`failure_examples`/`validation` fields hold — so `validate(frame)` is one modal entrypoint over the bare lazy plan and never grows a per-call disposition or sampling knob; `DataQuality.validate` binds the railed `ContentIdentity.of("schema", repr(schema).encode())` key through `.bind` into one `boundary("quality.validate", ...)` running `schema.validate(frame.collect(), lazy=self.lazy, sample=self.sample, random_state=self.seed)`, returning a `RuntimeRail[ContractClaim]` — the schema fold is bound once and threaded as both the railed key source and the `_validate` argument, never reconstructed per branch and never re-railed inside the boundary thunk. The `pandera.polars` backend defers a `LazyFrame` validation into the scan and returns the frame unraised, so the error rail never fires on a lazy input; the gate collects the lazy plan to a `pl.DataFrame` exactly once at the boundary so the `lazy` policy field at `True` raises `SchemaErrors` with the full `failure_cases` frame (the accumulating disposition, every breach reported) and at `False` raises the first `SchemaError` (the abort disposition) — the accumulate-vs-abort disposition fixed once on the owner, never a per-call flag. The optional `sample` row-count bound plus its `seed` (`random_state`) restrict validation to a deterministic row subset of the collected frame for a large input, the pandera large-frame sampling policy the gate mines rather than checking every row unconditionally. Both outcomes land in one `ContractClaim`; the rail is `Ok` even on validation failure because the claim records but does not enforce, and the rail is `Error` only when the collect or the railed content-key derivation faults.
- Auto: a passing validation yields `ContractClaim.of("data-quality", (columns,), (), key)` with `status=PASSED`; a failing lazy validation folds the `SchemaErrors.failure_cases` failing `column`/`check` pairs into `ContractClaim.breaches`, the `FAILED` status deriving from the non-empty breach tuple through `ContractClaim.of`; the frame stays lazy through admission and lowering and collects to eager once at the gate, the only point the polars backend surfaces a breach.
- Receipt: `ContractClaim.contribute` emits an emitted-phase `Receipt.of(owner, ("emitted", subject, facts))` row satisfying the `ReceiptContributor` `contribute() -> Iterable[Receipt]` Protocol, keyed by the `ContentKey` over the schema fingerprint — it is the data-contract evidence and never replaces the typed `QueryReceipt`.
- Packages: `pandera` (`pandera.polars.DataFrameSchema`/`Column(unique=)`/`Check.ge`/`le`/`gt`/`lt`/`equal_to`/`not_equal_to`/`in_range(min_value=, max_value=, include_min=, include_max=)`/`isin`/`notin`/`str_matches`/`str_contains`/`str_length(min_value=, max_value=)`/`is_monotonic(dim, increasing=)`/`errors.SchemaError`/`SchemaErrors`), `expression` (`tagged_union`/`tag`/`case` the `CheckKind` ADT, `expression.collections.Map` the four `_CMP`/`_SET`/`_TEXT`/`_INCLUSIVE` behavior tables — never a `from builtins import frozendict` table, the same `Map`-keyed dispatch the `interop#INTEROP` `_BACKEND` table carries), `msgspec` (`Struct` the frozen `ClaimStatus`-bearing `ContractClaim`/`QualityRule`/`DataQuality` owners), `polars` (`LazyFrame`, `TYPE_CHECKING`-only since `polars` is on `banned-module-level-imports`; the runtime frame arrives pre-lowered through `narwhals`), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `DataQuality.of` factory so a non-`QualityRule` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the caller's enclosing fence, the shared `FAULT_CONF` the sibling data admission seams bind; the `CheckKind.to_check`/`QualityRule.to_column`/`ContractClaim.of` projections over the owner's own already-admitted rows carry no decorator), runtime (`RuntimeRail`/`boundary`/`FAULT_CONF` the shared beartype violation-redirect config/`ContentIdentity`/`ContentKey`/`Receipt`/`ReceiptContributor`).
- Growth: a new comparison/range/membership/pattern/length/order check is one `CheckKind` row threading its `_CMP`/`_SET`/`_TEXT`/`_INCLUSIVE` table; a new column claim is one `QualityRule`; the narwhals-lazy validation backend is a pandera row on this same owner, never a parallel gate.
- Boundary: no raising in domain logic, no global schema registry, no coercion side effects (`coerce=False`); a per-check validator family, an exception-driven gate, and an undecorated `DataQuality.of` admitting a caller `QualityRule` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling data admission factories share are the deleted forms.

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
from pandera import Check
from pandera.errors import SchemaError, SchemaErrors

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    import polars as pl

type Cmp = Literal["ge", "le", "gt", "lt", "eq", "ne"]
type Text = Literal["matches", "contains"]
type Inclusive = Literal["both", "neither", "left", "right"]

_CMP: Final[Map[Cmp, Callable[[float], Check]]] = Map.of_seq([
    ("ge", Check.ge), ("le", Check.le), ("gt", Check.gt), ("lt", Check.lt),
    ("eq", Check.equal_to), ("ne", Check.not_equal_to),
])
_SET: Final[Map[bool, Callable[[Iterable[Any]], Check]]] = Map.of_seq([(True, Check.isin), (False, Check.notin)])
_TEXT: Final[Map[Text, Callable[[str | Pattern[str]], Check]]] = Map.of_seq([
    ("matches", Check.str_matches), ("contains", Check.str_contains),
])
_INCLUSIVE: Final[Map[Inclusive, tuple[bool, bool]]] = Map.of_seq([
    ("both", (True, True)), ("neither", (False, False)), ("left", (True, False)), ("right", (False, True)),
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
    def of(cls, subject: Literal["data-quality", "data-covenant"], shape: tuple[int, ...], breaches: tuple[tuple[str, ...], ...], key: ContentKey) -> "ContractClaim":
        return cls(subject, ClaimStatus.PASSED if not breaches else ClaimStatus.FAILED, shape, breaches, key)

    def contribute(self) -> tuple[Receipt, ...]:
        scope = "/".join(map(str, self.shape))
        facts: dict[str, object] = {"status": self.status, "breaches": len(self.breaches), "key": self.content_key.hex}
        return (Receipt.of(self.subject, ("emitted", f"{self.subject}[{scope}]", facts)),)


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
            self.dtype,
            checks=[c.to_check() for c in self.checks],
            nullable=self.nullable,
            unique=self.unique,
            required=self.required,
            coerce=False,
        )


class DataQuality(Struct, frozen=True):
    rules: tuple[QualityRule, ...]
    lazy: bool = True
    sample: int | None = None
    seed: int | None = None

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, *rules: QualityRule, lazy: bool = True, sample: int | None = None, seed: int | None = None) -> "DataQuality":
        return cls(rules=rules, lazy=lazy, sample=sample, seed=seed)

    def _schema(self) -> pap.DataFrameSchema:
        return pap.DataFrameSchema({r.column: r.to_column() for r in self.rules}, strict=False, coerce=False)

    def validate(self, frame: pl.LazyFrame) -> "RuntimeRail[ContractClaim]":
        schema = self._schema()
        return ContentIdentity.of("schema", repr(schema).encode()).bind(
            lambda key: boundary("quality.validate", lambda: self._validate(schema, frame, key))
        )

    def _validate(self, schema: pap.DataFrameSchema, frame: pl.LazyFrame, key: ContentKey) -> ContractClaim:
        # pandera.polars defers a LazyFrame validation into the scan and never raises on it; the
        # error rail fires only on a materialized frame, so the lazy plan collects once at the gate.
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

- Owner: `FieldShape` — the structural field/logical-type/nullable/source-evidence value object the `tabular/interop#INTEROP` `FrameInterop.schema_of` mints from any backend frame off the live null-mask; `FrameAdmission` the one admission path composing that sibling derivation. `FieldShape` is a distinct structural shape (field presence plus dtype plus observed nullability), not a re-mint of the quality `ContractClaim`. The data-contract enforcement is the `QUALITY` `DataQuality`/`ContractClaim` co-located on this page; admission proves structure, quality records the contract.
- Entry: `FrameAdmission.admit` resolves the live shapes through one `FrameInterop.schema_of` call — the single backend-agnostic schema-derivation owner that reads the per-column null-mask through `null_count()`, never a second inline `collect_schema()` fold — then folds the resolved `FieldShape` tuple against the required shapes through `FieldShape.resolve`: a required field absent from the live schema, carrying a non-matching `logical_type`, or declaring `nullable=False` where the live null-mask observed nulls is one breach token, so a present-but-wrong-dtype or wrongly-nullable field is a structural breach, not a silent pass. `schema_of` already lifts through `narwhals.from_native` inside its own `boundary`, so the admission rail binds the sibling `RuntimeRail` and the backend tag rides the `narwhals.Implementation` axis the interop owner exposes — one admission path for every backend, never a per-backend branch. `FrameAdmission.enforce` then routes data-contract validation to `DataQuality.validate`, lowering the agnostic frame to a polars frame through `FrameInterop.translate(frame, Backend.POLARS)` inside the same rail and lazifying it through `.frame.lazy()` to feed the gate's `pl.LazyFrame` boundary, never a second hand-spelled `to_polars().lazy()` lowering; the gate collects that lazy plan to eager once at the validation point because `pandera.polars` defers a `LazyFrame` validation unraised.
- Packages: `tabular/interop#INTEROP` (`FrameInterop.schema_of`/`FrameInterop.translate`/`FrameInterop.source`/`Backend.POLARS` the one backend-agnostic schema-derivation and lowering owner; `FrameInterop` annotations resolve under `TYPE_CHECKING` and the one runtime `Backend.POLARS` use imports boundary-scoped under `# noqa: PLC0415`, since `interop` imports `FieldShape` back from this page and a module-top import would close the cycle), `expression` (`Error` the explicit breach-rail `Error(BoundaryFault(...))` arm, `expression.collections.Map`/`Map.of_seq` the live `field->FieldShape` resolve map), `msgspec` (`Struct` the frozen `FieldShape`/`AdmittedFrame`/`FrameAdmission` owners), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `FrameAdmission.of` factory and the caller-facing `admit` submission so a malformed `FrameInterop`/`required`/`QualityRule` argument or a non-frame `admit` payload that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the caller's enclosing fence, the shared `FAULT_CONF` the sibling data admission seams bind; the `FieldShape.resolve` fold over an already-admitted shape carries no decorator), runtime (`RuntimeRail`/`BoundaryFault`/`FAULT_CONF` the shared beartype violation-redirect config — the admit path holds no inline `boundary` since `schema_of`/`translate` carry their own fences and `_resolve` projects the structural-breach verdict directly through `Ok`/`Error(BoundaryFault(...))`, never a `boundary("frame.admit", lambda: admitted)` no-op thunk wrapping an already-built value where the rail is constructed directly). The pandera enforcement is the `QUALITY` cluster on this same page.
- Growth: a new structural attribute is one column on `FieldShape` read once by the interop `schema_of` owner; a new quality rule is one `QualityRule`/`CheckKind` row on `DataQuality`; a new backend is admitted free by the interop `Backend` axis with zero admission-cluster change.
- Boundary: no Persistence migration law, no live Rhino/GH mutation; a hand-rolled validation loop, a stringly-typed rule set, a per-backend admission branch, a second inline `collect_schema()` schema-derivation path where `FrameInterop.schema_of` owns it, a presence-only check that passes a present-but-wrong-dtype or wrongly-nullable field, a duplicate `ContractClaim`, a second pandera gate, a `boundary("frame.admit", lambda: admitted)` no-op thunk wrapping an already-built `AdmittedFrame` where the structural verdict projects directly through `Ok`/`Error(BoundaryFault(...))` (the `boundary` fence is for a throwing provider call, not an `Ok` constructor), and an undecorated `FrameAdmission.of`/`admit` admitting a caller `FrameInterop`/`required`/`QualityRule`/frame argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling data admission entrypoints share are the deleted forms.

```python
from typing import TYPE_CHECKING, Any

from beartype import beartype
from expression import Error, Ok
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.faults import BoundaryFault, FAULT_CONF, RuntimeRail

if TYPE_CHECKING:
    from rasm.data.tabular.interop import Backend, FrameInterop


class FieldShape(Struct, frozen=True):
    field: str
    logical_type: str
    nullable: bool = True
    source_evidence: str = ""

    def resolve(self, live: "Map[str, FieldShape]") -> str:
        found = live.get(self.field)
        match found:
            case None:
                return f"{self.field}:absent"
            case FieldShape(logical_type=actual) if actual != self.logical_type:
                return f"{self.field}:{self.logical_type}!={actual}"
            case FieldShape(nullable=True) if not self.nullable:
                return f"{self.field}:nullable"
            case _:
                return ""


class AdmittedFrame(Struct, frozen=True):
    frame: Any
    backend: "Backend"
    shapes: tuple[FieldShape, ...]


class FrameAdmission(Struct, frozen=True):
    interop: "FrameInterop"
    required: tuple[FieldShape, ...]
    quality: "DataQuality"

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, interop: "FrameInterop", required: tuple[FieldShape, ...], *rules: QualityRule) -> "FrameAdmission":
        return cls(interop=interop, required=required, quality=DataQuality.of(*rules))

    @beartype(conf=FAULT_CONF)
    def admit(self, frame: Any) -> RuntimeRail[AdmittedFrame]:
        return self.interop.schema_of(frame).bind(lambda shapes: self._resolve(frame, shapes))

    def enforce(self, admitted: AdmittedFrame) -> RuntimeRail[ContractClaim]:
        from rasm.data.tabular.interop import Backend  # noqa: PLC0415

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

- Owner: `FrameCovenant` — the one cross-frame referential-integrity owner over `dataframely.Collection`; `RelationEdge` the row family modeling one foreign-key covenant between two named member frames (left member, right member, shared `on` keys, plus a closed `RelationCardinality`), folded into one `dy.Collection` subclass whose `@dy.filter` methods return the `require_relationship_*` keep-set. A new covenant is one `RelationEdge` row, never a `covenant_one_to_one`/`covenant_one_to_at_least_one` method family. The covenant composes the `tabular/interop#INTEROP` `FrameInterop` owner as the one backend-agnostic lowering seam — every member frame lowers to a polars `LazyFrame` through `FrameInterop.translate(frame, Backend.POLARS)`, never a second inline `narwhals.from_native(...).to_polars().lazy()` lowering, so a DuckDB-relation member admits through the same interop axis that already carries the `narwhals.Implementation.DUCKDB` backend. The covenant `ContractClaim` (`subject="data-covenant"`) records the system-of-frames contract and its per-member breach cases without raising; it never enforces.
- Cases: `RelationCardinality` is a `StrEnum` whose value is the exact `dataframely` builder name (`require_relationship_one_to_one` / `require_relationship_one_to_at_least_one`); `relate` resolves the bound builder by one `getattr(dy, self.value)` at boundary scope under `# noqa: PLC0415`, the import-ban-respecting collapse of the two referential-cardinality arms into one name-keyed dispatch with no parallel `@dy.filter` declaration and no dead bool payload. The enum value is the dispatch key, so a new cardinality is one enum row whose value names its builder, never a switch arm plus a table entry.
- Member: a covenant member is one admitted frame carried by name — the keys compose the sibling content-keyed bundles by reference: a `columnar#MATERIALIZE` `PartitionBundle.content_key`, a `spatial/catalog#CATALOG` `StacDiscovery.content_key`, or any `ContentKey`-bearing produced frame — so the member fingerprint is read off the sibling owner, never re-minted. `CovenantMember` pairs the member name, the agnostic frame, the pre-computed `ContentKey`, and a `MemberPolicy` value folding the `dataframely` `CollectionMember` per-member axes (`ignored_in_filters`/`propagate_row_failures`/`inline_for_sampling`) so a derived-snapshot member excluded from the foreign-key keep-set, or a primary member whose row failures cascade to its dependents, is one policy row on the member, never a parallel member shape. `MemberPolicy.member()` projects the row into the live `dy.CollectionMember` once, fused onto the member's `dy.LazyFrame[S]` annotation as one `Annotated[dy.LazyFrame[S], policy.member()]` — the member-policy value rides the type annotation the dataframely metaclass reads, never a parallel class-attribute the synthesized namespace splits off the annotation.
- Entry: `FrameCovenant.of` folds a tuple of `RelationEdge` plus the member-schema map and the composed `FrameInterop` into one covenant; `FrameCovenant.run` first binds the railed `ContentIdentity.of("covenant", member_keys)` Merkle key, then lowers every member of the op's payload exactly once through `_lower` — one `FrameInterop.translate(frame, Backend.POLARS)` per member `traversed` with `by=Disposition.ABORT` so the first lowering fault aborts the whole run, each translation's lowered native frame read off `FrameTranslation.frame` and lazified through `.frame.lazy()` — then binds the resolved key plus the pre-lowered ordered `Block[tuple[str, polars.LazyFrame]]` into one `boundary(f"covenant.{op.tag}")` dispatch over the `CovenantOp` tagged union — `Prove`/`Consistent`/`Restrict`/`Extend`/`Persist`/`Sample` — through one `match` closed by `assert_never`, never a `validate`/`consistent`/`restrict`/`extend` sibling-method tree sharing a `boundary` prefix and never a per-arm re-lowering of the same member. The lowered carrier is the ordered `(name, frame)` pair `Block` rather than a name-keyed dict so the `Extend` runs survive a member-name repeated across runs (a dict would collapse the repeated name to the last run's frame); the single-occurrence arms project `dict(pairs)` once. The `self.cast` policy field threads into every `filter`/`validate`/`is_valid`/`cast` call as the one dtype-coercion knob — a covenant over loosely-typed produced frames coerces by one field, never a per-arm hardwired `cast=False` — and `dy.Config(max_failure_examples=self.failure_examples)` bounds the captured `FailureInfo` example budget over the whole dispatch when set, the dataframely diagnostics cap the covenant mines rather than collecting unbounded failure rows. `Prove` runs `collection.filter(data, cast=self.cast, eager=True)`, returning a `RuntimeRail[ContractClaim]` — `filter` splits each member into `(valid, FailureInfo)` so every covenant violation lands in one `CollectionFilterResult` without raising, the rail `Ok` even on violation. `Consistent` runs `collection.is_valid(data, cast=self.cast)` for the breach-detail-free fast-path — it skips the `FailureInfo` materialization `Prove` pays for and folds the resulting `bool` into one status-only `ContractClaim` (empty breaches on pass, one `("collection", "consistent")` token on fail), so the cheap conformance check lands on the same `ContractClaim` outcome every other verb does rather than leaking a bare `bool` the caller re-shapes. `Restrict` derives the cross-member shared key off the covenant's own `collection.common_primary_key()` — the keys frame is one named anchor member projected to those key columns, never a parallel `keys` parameter the cardinality already owns — composing `collection.validate(data, cast=self.cast)`→`collection.join(anchor_keys, how="semi", maintain_order="none")`→`collection.collect_all()`, then projects the restricted members through `_members` (since `collect_all` returns the collection `Self`, not a frame map) and gates them back through `_claim` so the outcome is one `ContractClaim` over the restricted system, never a bare erased frame-map outcome and never a hand-stitched per-member semi-join. `Extend` folds an accreting series of member runs through `concat_collection_members`, the `_runs` slicer cutting the one-shot ordered pair `Block` back to per-run slices by each run's member count (so each run keeps its own lowered frames even when runs share member names) and casting each run to its schema before unioning the same-typed members run-wise into one claim, the first malformed run's `cast` raise converting once at the owning `run` boundary — the incremental covenant over an accreting system of frames, never a fresh full re-scan per run, never a re-lowering per run, and never a swallowed run fault. `Persist` is one IO case folding a `ContractIo` discriminant — `Frames`/`Contract` — over the embedded-schema member directory: `Frames` writes the validated members through `collection.write_parquet(directory)`, re-scans them through `collection.scan_parquet(directory, validation=self.validation)` so the parquet round-trip re-validates by the `Validation` policy row (`"allow"`/`"warn"`/`"skip"`), and gates the re-scanned members back through `_claim` so the embedded-schema round-trip itself is the proven artifact — the `self.validation` policy is the live re-validation knob, never a dead field. `Contract` routes `collection.serialize()`→`deserialize_collection(blob)`→`restored.matches(collection)` so the contract-string round-trip itself is the proven artifact — the restored `dy.Collection` subclass structurally equals the original or the round-trip is one `("contract", "round-trip")` breach (the `deserialize_collection` `None` restore-failure folding into the same breach), gated into one `ContractClaim` exactly as `Persist`/`Frames` gates its re-scan, so the system-of-frames IO is a claim-returning `CovenantOp` case over `dataframely`'s own format rather than a bare `bytes` leak, a side-file schema store, or a per-member parquet loop. `Sample` runs `collection.sample(num_rows, generator=)` for the deterministic synthetic system-of-frames the covenant defines, gating each generated member back through `_claim` so a sampled covenant is self-consistent by construction, never a hand-built fixture frame.
- Auto: a passing covenant yields `ContractClaim.of("data-covenant", (members, edges), (), key)` with `status=PASSED`; a failing covenant folds each member's `FailureInfo` into one breach stream carrying four kinds under a slot discriminant — `(member, "rule", rule_name, count)` rows from `FailureInfo.counts()`, `(member, "co-occur", ruleset, count)` rows from `FailureInfo.cooccurrence_counts()`, `(member, "detail", column, height)` rows from the `FailureInfo.details()` per-rule `Enum(["valid", "invalid", "unknown"])` frame projecting which column drove each rejection the bare counts do not name, and one `(member, "invalid", "rows", height)` row from `FailureInfo.invalid().height` carrying the absolute count of rejected member rows the per-rule counts alone do not total — `invalid()` and `details()` are `FailureInfo` methods bound once per member into the fold, never property reads. The typed referential-failure diagnostics read off the dataframely receipt, never re-derived from the raw frames and never a parallel breach record per kind; the member frames enter the collection lazy and `Collection.filter(..., eager=True)` runs the anti/semi-join keep-set and collects each member's failures into the one `CollectionFilterResult`.
- Receipt: the covenant `ContractClaim` keys off the railed `ContentIdentity.of` Merkle-fold over the admitted member `ContentKey`s through the `tuple[ContentKey, ...]` source — the same Merkle composition the `columnar#MATERIALIZE` `snapshot_key` folds — resolved once on the `run` rail through `.bind` and threaded into every claim arm as the resolved `ContentKey`, never re-railed inside the boundary thunk, so a single changed member flips the covenant key while the unchanged members stay byte-stable; `ContractClaim.contribute` emits an emitted-phase `Receipt.of(owner, ("emitted", subject, facts))` row satisfying the `ReceiptContributor` `contribute() -> Iterable[Receipt]` Protocol, keyed by that `ContentKey`, the system-of-frames evidence, never replacing the typed `QueryReceipt` and never re-minting a member's content-key.
- Packages: `dataframely` (`dy.Collection`/`dy.LazyFrame[S]` member annotations/`dy.CollectionMember(ignored_in_filters=/propagate_row_failures=/inline_for_sampling=)` per-member policy fused onto the member annotation as `Annotated[dy.LazyFrame[S], member]`/`dy.filter()`/`Collection.filter`->`CollectionFilterResult`/`Collection.is_valid`/`Collection.validate`/`Collection.cast`/`Collection.join(primary_keys, how="semi", maintain_order=)`/`Collection.collect_all`/`Collection.member_schemas`/`Collection.common_primary_key`/`Collection.sample(num_rows, generator=)`/`Collection.write_parquet`/`Collection.scan_parquet(directory, validation=)`/`Collection.serialize`/`deserialize_collection`/`Collection.matches`/`concat_collection_members`/`require_relationship_one_to_one`/`require_relationship_one_to_at_least_one`/`Config(max_failure_examples=)` the failure-example diagnostics cap context manager/`FailureInfo.invalid()`/`FailureInfo.details()`/`FailureInfo.counts()`/`FailureInfo.cooccurrence_counts()` (all four `FailureInfo` accessors are methods, not properties)/`Validation` policy literal, import at boundary scope under `# noqa: PLC0415` since the boundary-scoped `dataframely` import transitively loads the manifest-banned module-level `polars`), `tabular/interop#INTEROP` (`FrameInterop.translate`/`Backend.POLARS` the one backend-agnostic lowering owner the covenant composes so every member — a polars frame, a pandas frame, or a `narwhals.Implementation.DUCKDB` relation — lowers to a polars `LazyFrame` through the same axis, no inline `narwhals` seam on this cluster), `polars` (`LazyFrame`, `TYPE_CHECKING`-only since `polars` is on `banned-module-level-imports`; the lowered member arrives pre-translated through `FrameInterop`), `numpy` (`np.random.Generator` the deterministic `Collection.sample` seed, `TYPE_CHECKING`-only), `expression` (`tagged_union`/`tag`/`case` the `CovenantOp` ADT, `expression.collections.Block`/`Block.of_seq` the `_lower` traversed-rail carrier, `expression.collections.Map`/`Map.of_seq` the frozen `schemas` member-schema map), `msgspec` (`Struct` the frozen `RelationEdge`/`MemberPolicy`/`CovenantMember`/`FrameCovenant` owners), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `FrameCovenant.of` factory and the caller-facing `run` submission so a malformed `FrameInterop`/`schemas`/`RelationEdge` argument or a non-`CovenantOp` operation that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the enclosing `boundary` fence, the shared `FAULT_CONF` the sibling data admission seams bind; the `_dispatch`/`_claim`/`ContractClaim.of` folds over the owner's own lowered members carry no decorator), runtime (`RuntimeRail`/`boundary`/`traversed`/`Disposition`/`FAULT_CONF` the shared beartype violation-redirect config/`ContentIdentity`/`ContentKey`/`ReceiptContributor`/`Receipt`).
- Growth: a new covenant is one `RelationEdge`; a new cardinality is one `RelationCardinality` enum row whose value names its `dataframely` builder; a new backend member (a DuckDB relation, a pandas frame) is admitted free by the `tabular/interop#INTEROP` `Backend` axis with zero covenant-cluster change; a new produced-frame member is one `CovenantMember` carrying its sibling-owned `ContentKey` (`columnar#MATERIALIZE` `PartitionBundle.content_key`, `spatial/catalog#CATALOG` `StacDiscovery.content_key`); a new member-participation rule is one `MemberPolicy` row over the `dy.CollectionMember` axes fused onto the member annotation; a cross-member grouped invariant beyond foreign-key cardinality is one `@dy.filter` returning a custom keep-set `pl.LazyFrame`; a new breach diagnostic is one slot row on the breach stream; a dtype-coercion or failure-example diagnostics knob is one `cast`/`failure_examples` policy field on the covenant threaded into the existing `filter`/`validate`/`Config` call, never a parallel coercing or capped gate; a new covenant verb is one `CovenantOp` case under the same `run` dispatch; a new IO direction is one `ContractIo` case on the `Persist` payload (`Frames` the `scan_parquet`/`write_parquet` member-directory arm under the `Validation` policy row, `Contract` the `serialize`/`deserialize_collection` contract-string arm), never a parallel covenant gate or a second persistence verb.
- Boundary: dataframely owns the Polars-native cross-frame integrity over its Rust `_native` core; the `tabular/interop#INTEROP` `FrameInterop.translate` owns every member lowering; no raising in domain logic, no inline `narwhals.from_native(...).to_polars().lazy()` member lowering where `FrameInterop.translate(frame, Backend.POLARS)` owns it, no hand-stitched anti/semi-join where `require_relationship_*` owns referential integrity, no second `ContentIdentity` mint where the sibling bundle already carries the member key; a per-cardinality filter family, a per-verb `validate`/`consistent`/`restrict`/`extend` method tree where one `CovenantOp` dispatch discriminates the verb, a second inline narwhals lowering seam beside the interop owner, a per-arm re-lowering of the same member, a re-derived member content-key, a hardwired per-arm `cast=False`, a parallel collection gate, a parallel breach record per failure kind, a second claim type beside `ContractClaim`, a heterogeneous `RuntimeRail[ContractClaim | bool | bytes]` outcome union forcing every `run` consumer to re-`match` three unrelated success shapes where the one `RuntimeRail[ContractClaim]` outcome folds the `Consistent` bool and the `Contract` round-trip into the status the claim already carries, and an undecorated `FrameCovenant.of`/`run` admitting a caller `FrameInterop`/`schemas`/`RelationEdge`/`CovenantOp` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling data admission entrypoints share are the deleted forms.

```python signature
from collections.abc import Iterator, Mapping
from contextlib import nullcontext
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Any, Literal, assert_never

from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.data.tabular.interop import Backend, FrameInterop
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import Disposition, FAULT_CONF, RuntimeRail, boundary, traversed

if TYPE_CHECKING:
    import dataframely as dy
    import numpy as np
    import polars as pl

type Validation = Literal["allow", "warn", "skip"]


class RelationCardinality(StrEnum):
    ONE_TO_ONE = "require_relationship_one_to_one"
    ONE_TO_AT_LEAST_ONE = "require_relationship_one_to_at_least_one"

    def relate(self, lhs: "pl.LazyFrame", rhs: "pl.LazyFrame", on: tuple[str, ...]) -> "pl.LazyFrame":
        import dataframely as dy  # noqa: PLC0415

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
        import dataframely as dy  # noqa: PLC0415

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

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, interop: FrameInterop, schemas: "Mapping[str, type[dy.Schema]]", *edges: RelationEdge, validation: Validation = "warn", cast: bool = False) -> "FrameCovenant":
        return cls(interop=interop, edges=edges, schemas=Map.of_seq(schemas.items()), validation=validation, cast=cast)

    @beartype(conf=FAULT_CONF)
    def run(self, op: CovenantOp) -> "RuntimeRail[ContractClaim]":
        return ContentIdentity.of("covenant", tuple(m.content_key for m in op.members)).bind(
            lambda key: self._lower(op.members).bind(lambda pairs: boundary(f"covenant.{op.tag}", lambda: self._dispatch(op, pairs, key)))
        )

    def _dispatch(self, op: CovenantOp, pairs: "Block[tuple[str, pl.LazyFrame]]", key: ContentKey) -> ContractClaim:
        import dataframely as dy  # noqa: PLC0415

        with dy.Config(max_failure_examples=self.failure_examples) if self.failure_examples is not None else nullcontext():
            match op:
                case CovenantOp(tag="prove", prove=members):
                    return self._claim(members, dict(pairs), key)
                case CovenantOp(tag="consistent", consistent=members):
                    passed = self._collection(members).is_valid(dict(pairs), cast=self.cast)
                    return ContractClaim.of("data-covenant", (len(self.schemas), len(self.edges)), () if passed else (("collection", "consistent"),), key)
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
                    return ContractClaim.of("data-covenant", (len(self.schemas), len(self.edges)), () if matched else (("contract", "round-trip"),), key)
                case CovenantOp(tag="sample", sample=(rows, members)):
                    return self._claim(members, _members(self._collection(members).sample(rows, generator=self.generator)), key)
                case unreachable:
                    assert_never(unreachable)

    def _collection(self, members: "tuple[CovenantMember, ...]") -> "type[dy.Collection]":
        import dataframely as dy  # noqa: PLC0415

        policy = {m.name: m.policy for m in members}
        namespace: dict[str, Any] = {
            "__annotations__": {
                name: Annotated[dy.LazyFrame[schema], policy.get(name, MemberPolicy()).member()]
                for name, schema in self.schemas.items()
            },
            **{edge.name: dy.filter()(
                lambda collection, _edge=edge: _edge.kind.relate(getattr(collection, _edge.left), getattr(collection, _edge.right), _edge.on)
            ) for edge in self.edges},
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
                *((name, "detail", column, str(details.filter(details[column] == "invalid").height)) for column in details.columns if column not in invalid.columns),
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

- [ADMISSION_COMPOSITION]: `FrameAdmission` composes the `tabular/interop#INTEROP` `FrameInterop.schema_of`/`FrameInterop.translate`/`Backend.POLARS`/`FrameInterop.source` surface as the one backend-agnostic schema-derivation and lowering owner, so the admit path holds zero inline `narwhals` calls — `schema_of` reads the per-column null-mask through `null_count()` off the live frame and mints the null-mask-accurate `FieldShape` tuple, and `translate(frame, Backend.POLARS)` returns a `FrameTranslation` whose `.frame` is the native polars `DataFrame` the admit path lazifies through `.frame.lazy()` to feed `DataQuality.validate` (never `.lazy()` on the `FrameTranslation` carrier itself); the interop `[NARWHALS_SURFACE]` RESEARCH item already settles the underlying `from_native`/`collect_schema`/`null_count`/`to_polars`/`Implementation` spellings against the folder `narwhals` `.api`, so admission carries no duplicate narwhals seam. The `pandera.polars.DataFrameSchema`/`Column`/`Check` enforcement surface is catalogue-confirmed against the folder `pandera` `.api`. Settled fence code.
- [PANDERA_FAULT_SURFACE]: the `errors.SchemaErrors.failure_cases` frame (the live columns `failure_case`/`schema_context`/`column`/`check`/`check_number`/`index`, so the `select(["column", "check"])` projection resolves) and the `errors.SchemaError.schema`/`.check` attributes the `_validate` failure arms bind directly are signature-confirmed against the live `pandera` `0.31.1` surface and catalogue-confirmed against the folder `pandera` `.api`, alongside the `pandera.polars.{DataFrameSchema,Column}` native-polars backend the `_schema` fold targets; the `DataFrameSchema.validate(check_obj, *, head, tail, sample, random_state, lazy, inplace)` signature the gate threads `lazy=`/`sample=`/`random_state=` into is signature-confirmed against the live surface (`sample` is the integer row-count subset, `random_state` its seed). The `pandera.polars` backend raise behavior is the load-bearing fact: handed a `pl.LazyFrame`, `validate` defers into the scan and returns the frame unraised (neither `validate` nor the subsequent `.collect()` surfaces a breach), so the error rail fires only on a materialized `pl.DataFrame` — `validate(frame.collect(), ...)` raises `SchemaErrors` under `lazy=True` and the first `SchemaError` under `lazy=False`. The `frame.collect()` materialization at the gate, the two `except` arms, and the `failure_cases.select(["column", "check"]).iter_rows()` fold are settled fence code against the live `0.31.1` distribution.
- [PANDERA_CHECK_VOCABULARY]: the `pandera` `Check.ge`/`le`/`gt`/`lt`/`equal_to`/`not_equal_to` (the `_CMP` table), `Check.in_range(*, min_value, max_value, include_min, include_max)` (the `_INCLUSIVE` endpoint-closure fold), `Check.isin(allowed_values)`/`Check.notin(forbidden_values)` (the `_SET` table, the positional `list(values)` binding the variadic head), `Check.str_matches`/`Check.str_contains` taking `str | Pattern[str]` (the `_TEXT` table), `Check.str_length(*, min_value, max_value, exact_value)` taking numeric bounds (the distinct `length` case, never the `_TEXT` pattern table), and `Check.is_monotonic(dim, *, increasing)` requiring the column-name positional plus the direction toggle (the `monotonic` case) are signature-confirmed against the live `pandera` `Check` surface and catalogue-confirmed against the folder `pandera` `.api`; the `Column(unique=)` flag routing (uniqueness is a column policy, never a `Check`) and the total `to_check` returning a concrete `Check` for every case are settled fence code. `Check.in_range` keys `min_value`/`max_value` by keyword (positional binds the variadic head, never the bounds); `Check.is_monotonic` rejects a `strict=` keyword and demands `dim` plus `increasing`; `Check.str_length` is a numeric-bound check that type-mismatches a `str` pattern — each is settled against the live signature.
- [DATAFRAMELY_COVENANT_SURFACE]: the `dataframely` `dy.Collection`/`dy.LazyFrame[S]` member annotation/`dy.CollectionMember(ignored_in_filters=, propagate_row_failures=, inline_for_sampling=)`/`dy.filter()`/`Collection.validate(data, cast=, eager=)`/`Collection.filter`/`Collection.is_valid`/`Collection.cast`/`Collection.join(primary_keys, how="semi", maintain_order=)`/`Collection.collect_all`/`Collection.member_schemas`/`Collection.common_primary_key()`/`Collection.sample(num_rows, generator=)`/`Collection.write_parquet(directory)`/`Collection.scan_parquet(directory, validation=)`/`Collection.serialize() -> str`/`deserialize_collection(blob) -> type[Collection] | None`/`Collection.matches(other) -> bool`/`concat_collection_members(collections)`/`require_relationship_one_to_one(lhs, rhs, on=, drop_duplicates=)`/`require_relationship_one_to_at_least_one`/`Config(max_failure_examples=)`/`FailureInfo.invalid`/`FailureInfo.counts()`/`FailureInfo.cooccurrence_counts()` members the `FrameCovenant` owner binds are catalogue-confirmed against the folder `dataframely` `.api` (`require_relationship_*`/`concat_collection_members` are catalogue-confirmed bare top-level `dataframely` exports, so the `getattr(dy, RelationCardinality.value)` builder resolution and `dy.concat_collection_members` resolve directly); `require_relationship_*` returns the keep-set `pl.LazyFrame`, the `Collection.{filter,validate,is_valid,cast}` family threads the one `self.cast` dtype-coercion policy as the catalogue-confirmed `cast=` call row, the `Collection.join` semi-restriction returns the filtered `Self`, `Collection.common_primary_key()` returns the `list[str]` the `Restrict` anchor projection selects (so the cross-member key derives off the covenant, never a smuggled `keys` parameter), `Collection.sample(num_rows, *, generator=None)` returns the synthetic collection `Self` the `Sample` arm projects back through `_members`, `Collection.serialize()` returns the contract `str` the `Persist`/`Contract` arm round-trips through `deserialize_collection(blob) -> type[Collection] | None` and proves with `Collection.matches(other) -> bool` (the `None` restore-failure folding into the same `("contract", "round-trip")` breach), the `Validation` literal `"allow"`/`"warn"`/`"skip"` is the catalogue-confirmed `scan_parquet(validation=)` policy the `Persist`/`Frames` arm threads, `Config` is the catalogue-confirmed context manager whose `max_failure_examples` override bounds the captured `FailureInfo` examples over the whole dispatch (the inline `contextlib.nullcontext()` is the plain-CPython no-op arm when `failure_examples is None`), `concat_collection_members(Sequence[C])` takes collection instances and returns the `dict[str, pl.LazyFrame]` member map, `CollectionMember` carries the per-member behavior axes the `MemberPolicy.member()` projection folds, `FailureInfo.invalid()` is the method returning the rejected-row `pl.DataFrame` whose `.height` totals the absolute breach count, `write_parquet`/`scan_parquet` embed and re-validate the serialized member contract, and the record-don't-enforce `filter`-over-`validate` split are settled fence code against the live `dataframely 2.10.1` distribution.
- [DATAFRAMELY_CONFIG_CAP]: `dy.Config` is the sampling/failure-example cap context manager (`Config(**options: Options)` over the `Options` `TypedDict` keys `max_sampling_iterations`/`max_failure_examples`, plus the `Config.set_max_failure_examples(n)`/`set_max_sampling_iterations(n)`/`restore_defaults()` setters); the `_dispatch` fold opens `dy.Config(max_failure_examples=self.failure_examples)` over the whole verb when `failure_examples` is set and an inline `contextlib.nullcontext()` otherwise. The `Config(max_failure_examples=)` constructor-kwarg binding is signature-confirmed against the live `2.10.1` `Options` `TypedDict` and entering the context with the kwarg succeeds, so the constructor-kwarg spelling and the `nullcontext` no-op arm are settled fence code.
- [DATAFRAMELY_COLLECTION_CONSTRUCTION]: the `FrameCovenant._collection` fold declares each member as `Annotated[dy.LazyFrame[Schema], MemberPolicy.member()]` — the member-field form fusing the `dy.CollectionMember` per-member policy onto the `dy.LazyFrame[S]` annotation, never a parallel class-attribute split off the annotation — and the `dy.filter()`-decorated edge methods through a runtime `__annotations__`/attribute namespace dict synthesized into `type("Covenant", (dy.Collection,), namespace)`. The `dataframely 2.10.1` collection metaclass (`dataframely.collection._base`) admits this runtime-constructed `type(...)` namespace dict directly: building a two-member collection through `type("Covenant", (dy.Collection,), {"__annotations__": {...Annotated[dy.LazyFrame[S], dy.CollectionMember()]...}, edge_name: dy.filter()(edge_fn)})` resolves `member_schemas()` and `common_primary_key()` and runs the edge filter, with no literal `class` body required (the metaclass enforces the overlapping-primary-key invariant on the synthesized members, so each member schema declares the shared `primary_key=True` key). Settled fence code.
- [DATAFRAMELY_FILTER_RESULT]: `Collection.filter(data, /, *, cast=, eager=True)` returns a `CollectionFilterResult[Self]` — a 2-field generic `NamedTuple` (`_fields == ("result", "failure")`: `result: C` the valid collection, `failure: dict[str, FailureInfo]` the per-member failure map) carrying a `collect_all()` method; `count`/`index` are inherited `tuple` methods, not result members. The per-member `FailureInfo` map is the `result.failure` field (singular, keyed by member name; `.failure` is a NamedTuple field — a `_tuplegetter` descriptor — never a `property`), never a `result.failures` plural attribute or an `invalid()`/`failure_info()` method — the `_claim` breach fold reads `result.failure.items()` directly (equivalently `_, failure = collection.filter(...)` tuple-unpacks the same field), the per-member `FailureInfo` flowing off the `filter` return type rather than a single-caller `_failures` passthrough hop renaming the field. Signature-confirmed against the live `2.10.1` distribution; settled fence code.
- [DATAFRAMELY_COLLECTION_MEMBERS]: `Collection.member_schemas()` (a classmethod returning the member-name-to-schema map) plus `getattr(collection, name)` over those names yields each member as a `pl.LazyFrame` attribute under its declared name — confirmed against the `validate()`/`sample()`/`scan_parquet()` returns (each yields the collection `Self` instance exposing its members as `LazyFrame` attributes), so the `_members` projection `{name: getattr(collection, name) for name in type(collection).member_schemas()}` resolves on all three. The `Restrict`, `Sample`, and `Persist`/`Frames` arms read through this one `_members` seam. Settled fence code against the live `2.10.1` distribution.
- [DATAFRAMELY_FAILURE_DETAILS]: `FailureInfo.invalid()` and `FailureInfo.details()` are methods (signature `(self) -> pl.DataFrame`), not properties, so the `_claim` fold binds both once per member (`invalid, details = failure.invalid(), failure.details()`) and never reads them as attributes. `details()` returns a `pl.DataFrame` of the verbatim invalid input rows extended by one per-rule column whose dtype is `Enum(["valid", "invalid", "unknown"])` and whose name is the rule name (`primary_key`/`<col>|nullability`/`<col>|min_length`/the edge-filter name); the per-rule columns are exactly `details().columns` minus `invalid().columns`, and the rejecting cell value is the `"invalid"` Enum category — so `details.filter(details[column] == "invalid").height` counts which column drove each rejection. The method-versus-property fact, the `Enum` dtype, the `"invalid"` literal, and the per-rule-column layout are all confirmed against the live `2.10.1` distribution; the `"rule"`/`"co-occur"`/`"detail"`/`"invalid"` breach arms over `counts()`/`cooccurrence_counts()`/`details()`/`invalid().height` are settled fence code.
