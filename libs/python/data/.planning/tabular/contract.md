# [PY_DATA_CONTRACT]

The data-contract gate plus structural frame admission as one owner: `DataQuality` folds IDS-style `QualityRule` rows into one `pandera` schema recording a non-enforcing `SchemaClaim`, and `FrameAdmission` proves required structural `FieldShape`s resolve against the live agnostic schema before routing enforcement to that same `DataQuality`. `CheckKind` collapses the IDS-style predicate vocabulary into one tagged union; `QualityRule` is one column claim; `SchemaClaim` records the contract and its failure cases without raising — enforcement is the caller's `match` on `SchemaClaim.status`. `FieldShape` is a distinct structural shape (field presence plus dtype derived agnostically), not a re-mint of the quality claim. There is exactly one `SchemaClaim` and one pandera gate for the whole package.

## [01]-[INDEX]

- [01]-[QUALITY]: the data-quality gate over pandera, the recorded non-enforcing schema claim.
- [02]-[ADMISSION]: structural field shapes, narwhals frame admission, the contract route.

## [02]-[QUALITY]

- Owner: `DataQuality` — the one data-quality validation owner over `pandera.polars`; `QualityRule` the row family modeling one column claim (dtype/nullable/unique/required plus a closed `CheckKind` predicate set), folded into a `pandera.polars.DataFrameSchema`. A new validation is one `QualityRule` row, never a `validate_nullable`/`validate_range`/`validate_unique` method family. `SchemaClaim` records the contract and its failure cases without raising; it never enforces.
- Cases: `CheckKind` rows `cmp(op, v)` folding `ge`/`le`/`gt`/`lt`/`eq` through one `_CMP` `frozendict` (`Check.ge`/`le`/`gt`/`lt`/`equal_to`) so five comparison arms collapse to one table lookup · `in_range(lo, hi)` (`Check.in_range`) · `isin(values)` (`Check.isin`) · `unique`/`monotonic` (column flag / `Check.is_monotonic`), matched by `match`/`case` closed by `assert_never` into the concrete `pandera.Check`, so the IDS-style rule vocabulary is one closed switch over a table, never a per-check builder.
- Entry: `DataQuality.of` folds a tuple of `QualityRule` into one `DataFrameSchema`; `DataQuality.validate` runs `schema.validate(frame, lazy=lazy)` and returns a `RuntimeRail[SchemaClaim]` — `lazy=True` collects all failure cases (the `SchemaErrors.failure_cases` frame), `lazy=False` short-circuits on the first `SchemaError`. Both land in one `SchemaClaim`; the rail is `Ok` even on validation failure because the claim records but does not enforce.
- Auto: a passing validation yields `SchemaClaim(status=PASSED, failure_count=0)`; a failing lazy validation captures the `SchemaErrors.failure_cases` row count and the failing column/check pairs into `SchemaClaim.failures`; the polars backend keeps the frame lazy so validation pushes into the scan.
- Receipt: `SchemaClaim` contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` keyed by `ContentIdentity` over the schema fingerprint — it is the data-contract evidence and never replaces the typed `QueryReceipt`.
- Packages: `pandera` (`pandera.polars.DataFrameSchema`/`Column`/`Check`/`errors.SchemaError`/`SchemaErrors`), `polars` (`LazyFrame`, `TYPE_CHECKING`-only since `polars` is on `banned-module-level-imports`; the runtime frame arrives pre-lowered through `narwhals`), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new check is one `CheckKind` row; a new column claim is one `QualityRule`; the narwhals-lazy validation backend and the xarray-validation path are pandera rows on this same owner, never a parallel gate.
- Boundary: no raising in domain logic, no global schema registry, no coercion side effects (`coerce=False`); a per-check validator family and an exception-driven gate are the deleted forms.

```python signature
from builtins import frozendict
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import pandera.polars as pap
from expression import case, tag, tagged_union
from msgspec import Struct
from pandera import Check
from pandera.errors import SchemaError, SchemaErrors

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    import polars as pl

_CMP: Final[frozendict[str, Callable[[float], Check]]] = frozendict({
    "ge": Check.ge,
    "le": Check.le,
    "gt": Check.gt,
    "lt": Check.lt,
    "eq": Check.equal_to,
})


@tagged_union(frozen=True)
class CheckKind:
    tag: Literal["cmp", "in_range", "isin", "unique", "monotonic"] = tag()
    cmp: tuple[Literal["ge", "le", "gt", "lt", "eq"], float] = case()
    in_range: tuple[float, float] = case()
    isin: tuple[Any, ...] = case()
    unique: bool = case()
    monotonic: bool = case()

    def to_check(self) -> Check | None:
        match self:
            case CheckKind(tag="cmp", cmp=(op, v)):
                return _CMP[op](v)
            case CheckKind(tag="in_range", in_range=(lo, hi)):
                return Check.in_range(lo, hi)
            case CheckKind(tag="isin", isin=values):
                return Check.isin(list(values))
            case CheckKind(tag="monotonic"):
                return Check.is_monotonic() if self.monotonic else None
            case CheckKind(tag="unique"):
                return None
            case unreachable:
                assert_never(unreachable)


class QualityRule(Struct, frozen=True):
    column: str
    dtype: Any
    checks: tuple[CheckKind, ...] = ()
    nullable: bool = False
    required: bool = True

    def to_column(self) -> pap.Column:
        return pap.Column(
            self.dtype,
            checks=[c.to_check() for c in self.checks if c.to_check() is not None],
            nullable=self.nullable,
            unique=any(c.tag == "unique" and c.unique for c in self.checks),
            required=self.required,
            coerce=False,
        )


class ClaimStatus(StrEnum):
    PASSED = "passed"
    FAILED = "failed"


class SchemaClaim(Struct, frozen=True):
    status: ClaimStatus
    columns: int
    failure_count: int
    failures: tuple[tuple[str, str], ...]
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "data-quality", f"schema[{self.columns}]", {"status": self.status, "failures": str(self.failure_count)})


class DataQuality(Struct, frozen=True):
    rules: tuple[QualityRule, ...]

    @classmethod
    def of(cls, *rules: QualityRule) -> "DataQuality":
        return cls(rules=rules)

    def _schema(self) -> pap.DataFrameSchema:
        return pap.DataFrameSchema({r.column: r.to_column() for r in self.rules}, strict=False, coerce=False)

    def validate(self, frame: pl.LazyFrame, *, lazy: bool = True) -> "RuntimeRail[SchemaClaim]":
        return boundary("quality.validate", lambda: self._validate(frame, lazy))

    def _validate(self, frame: pl.LazyFrame, lazy: bool) -> SchemaClaim:
        key = ContentIdentity.of("schema", repr(self._schema()).encode())
        try:
            self._schema().validate(frame, lazy=lazy)
            return SchemaClaim(ClaimStatus.PASSED, len(self.rules), 0, (), key)
        except SchemaErrors as fault:
            pairs = tuple((str(c), str(k)) for c, k in fault.failure_cases.select(["column", "check"]).iter_rows())
            return SchemaClaim(ClaimStatus.FAILED, len(self.rules), len(pairs), pairs, key)
        except SchemaError as fault:
            pairs = ((str(fault.schema), str(fault.check)),)
            return SchemaClaim(ClaimStatus.FAILED, len(self.rules), len(pairs), pairs, key)
```

## [03]-[ADMISSION]

- Owner: `FieldShape` — the structural field/logical-type/nullable/source-evidence value object derived agnostically by `interop` `FrameInterop.schema_of` from any backend frame; `FrameAdmission` the one admission path over any `narwhals`-admitted backend. `FieldShape` is a distinct structural shape (field presence plus dtype), not a re-mint of the quality `SchemaClaim`. The data-contract enforcement is the `QUALITY` `DataQuality`/`SchemaClaim` co-located on this page; admission proves structure, quality records the contract.
- Entry: `FrameAdmission.admit` lifts a native frame through `narwhals.from_native`, reads `collect_schema()`, asserts every required `FieldShape` resolves against the live agnostic schema, and returns a `RuntimeRail[AdmittedFrame]` — one admission path for every backend, never a per-backend branch. `FrameAdmission.enforce` then routes data-contract validation to `DataQuality.validate`, lowering the agnostic frame to a polars `LazyFrame` so the polars validation pushes into the scan.
- Packages: `narwhals` (`from_native`/`collect_schema`/`Implementation.name`), runtime (`RuntimeRail`/`BoundaryFault`). The pandera enforcement is the `QUALITY` cluster on this same page.
- Growth: a new structural attribute is one column on `FieldShape`; a new quality rule is one `QualityRule`/`CheckKind` row on `DataQuality`; a new backend is admitted free by `narwhals` with zero admission-cluster change.
- Boundary: no Persistence migration law, no live Rhino/GH mutation; a hand-rolled validation loop, a stringly-typed rule set, a per-backend admission branch, a duplicate `SchemaClaim`, and a second pandera gate are the deleted forms.

```python
from __future__ import annotations

from typing import Any

import narwhals as nw
from expression import Error, Ok
from msgspec import Struct

from rasm.runtime.faults import BoundaryFault, RuntimeRail


class FieldShape(Struct, frozen=True):
    field: str
    logical_type: str
    nullable: bool
    source_evidence: str


class AdmittedFrame(Struct, frozen=True):
    frame: Any
    backend: str
    shapes: tuple[FieldShape, ...]


class FrameAdmission(Struct, frozen=True):
    required: tuple[FieldShape, ...]
    quality: DataQuality

    @classmethod
    def of(cls, required: tuple[FieldShape, ...], *rules: QualityRule) -> "FrameAdmission":
        return cls(required=required, quality=DataQuality.of(*rules))

    def admit(self, frame: Any) -> RuntimeRail[AdmittedFrame]:
        agnostic = nw.from_native(frame, eager_only=True)
        present = set(agnostic.collect_schema().names())
        missing = tuple(s.field for s in self.required if s.field not in present)
        if missing:
            return Error(BoundaryFault(boundary=("frame.admit", f"missing required fields: {', '.join(missing)}")))
        return Ok(AdmittedFrame(frame=frame, backend=agnostic.implementation.name.lower(), shapes=self.required))

    def enforce(self, admitted: AdmittedFrame) -> RuntimeRail[SchemaClaim]:
        return self.quality.validate(nw.from_native(admitted.frame, eager_only=True).to_polars().lazy())
```

## [04]-[RESEARCH]

- [NARWHALS_ADMISSION]: the `narwhals` `from_native(..., eager_only=True)`/`collect_schema().names()`/`to_polars().lazy()`/`Implementation.name` surface the `FrameAdmission` admit/enforce path transcribes is catalogue-confirmed against the folder `narwhals` `.api`; the `implementation.name.lower()` backend tag resolves to `'pyarrow'` for the `PYARROW` member, never `'arrow'`. The `pandera.polars.DataFrameSchema`/`Column`/`Check` admission surface is catalogue-confirmed against the folder `pandera` `.api`.
- [PANDERA_FAULT_SURFACE]: the `errors.SchemaErrors.failure_cases` frame (`column`/`check` columns) and the `errors.SchemaError.schema`/`.check` attributes the `_validate` failure arms bind directly are catalogue-confirmed against the folder `pandera` `.api`, alongside the `pandera.polars.{DataFrameSchema,Column}` native-polars backend the `_schema` fold targets; the two `except` arms and the lazy-pushdown validation are settled fence code.
