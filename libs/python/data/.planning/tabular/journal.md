# [PY_DATA_JOURNAL]

`FactJournal` implements the `python:runtime/observability/journal#LEDGER` `Ledger` port over the two tabular owners that already hold its halves: every committing member rides `tabular/lakehouse#LAKEHOUSE`'s `(format, tag)` commit matrix and every reading member rides `tabular/columnar#SCAN`'s engine-polymorphic reader. Composition rather than widening is forced by both boundaries — the commit owner reads no data and the scan owner holds no durable store — so this page is the one place the two meet, and it opens no provider of its own.

Two CALLER tables riding `Lakehouse.run` carry the plane — the append-only stream keyed by `FactRow.key` beside one wrapped data key per `(tenant, subject)` identity — never `Residence` rows, which name the planes the commit owner writes its own receipt evidence into. Each arms through `LakeOp.Ensure` off its own `TableLayout`, rows land through `LakeOp.Merge` on the content key so an at-least-once drain dedups structurally, and erasure deletes the custody row alone, since the port rules unreadable IS erased.

`SCHEMA` at the port is the schema truth this page transcribes: `_FACT_SCHEMA` mirrors its `FactRow` component — every column the port LIFTS out of the payload lands here as a real field, the metering pair included, which is what lets `tallied` group in the engine — and `payload` crosses as opaque msgpack this owner never decodes on the write side.

## [01]-[INDEX]

- [02]-[JOURNAL]: `FactJournal` implementing the `Ledger` port, its two tables, and the commit-plus-scan split per member.

## [02]-[JOURNAL]

- Owner: `FactJournal` — the one `Ledger` implementer, holding an opened `Lakehouse` handle beside a `DatasetRef` per residence so a committing member reaches the matrix and a reading member reaches the scan plane without either re-opening the other's provider. `_FACT_SCHEMA` and `_CUSTODY_SCHEMA` pin the two durable shapes, `_FACT_LAYOUT` and `_CUSTODY_LAYOUT` carry them as arming specs, and `_subject` spells a custody identity once so claim, read, and destroy cannot key differently.
- Cases: every member awaits, because the port's install proof refuses a member present yet not a coroutine function. Committing members ride `Lakehouse.run_async`, which owns the band hop; reading members fold the synchronous `columnar.execute` rail inside `async_boundary` over `on_thread`, since a scan on the drain's own loop stalls every producer suspended behind it.
- Entry: `FactJournal.of` admits the two dataset refs, one `TableFormat`, and the in-engine credential rows both planes share, opening both handles through `Lakehouse.open` so a format-refused ref fails at admission rather than inside a member. Separate refs are load-bearing: a custody plane and an evidence plane carry different retention and different access posture, and folding them onto one table turns an erasure into a rewrite of the stream it must not touch.
- Auto: `landed` proves the offered keys distinct, resolves the matched KEYS through one bounded probe, and then merges on `key`, so `accepted` names the rows the plane did not hold and `duplicate` names the redeliveries, the two partitioning the offered batch by construction and giving the port's drain the identity its accepted-only projection filters on. Distinctness is the merge's own precondition — two source rows matching one target abort the commit — and refusing the batch by name keeps each half counting every offered key exactly once. Order carries the whole correctness here: after the commit every offered key reads present, and no merge receipt slot names which rows inserted — deriving either half from a fused output tally reports zero duplicates forever, because that count includes rows merely COPIED into a rewritten file and so exceeds the batch it was handed. `scanned` lowers a `Scan` case to one predicate over the pinned columns and decodes each `payload` through the port's own `DECODE`, never a local codec.
- Auto: `tallied` pushes its group-by INTO the engine — the port lifts `(resource, quantity)` onto `FactRow`, so `_TALLY_SQL` groups on `(tenant, resource)` over the bound `source` view and only the aggregate rows cross, where a row-wise fold allocates one object per metered fact of a settlement month to produce a handful of slots. `_billed` rebuilds `Priced`/`Aggregate` off those four columns, deriving `attributed` from the empty-tenant spelling exactly as `Priced.of` derives it from an absent one, so a pushed-down tally and `Aggregate.rolled` answer the identical `Map`.
- Auto: `groomed` folds the horizon map into ONE predicate per retention class and answers the summed reclaim; a class whose horizon is absent contributes no clause, so a permanent class is untouched by construction rather than by a guard a later edit can drop.
- Receipt: each member contributes through the `LakeReceipt` and `QueryReceipt` its composed owner already mints — this page adds no receipt family, records no measure, and opens no span, because the port's own drain projects the series and a second recording double-counts the spine.
- Packages: `tabular/lakehouse#LAKEHOUSE` (`Lakehouse.open`/`run_async`, `LakeOp.Ensure`/`Merge`/`Delete`/`Read`, `LakePlane`, `TableLayout`, `LakeReceipt`), `tabular/columnar#SCAN` (`DatasetRef`/`ScanPlan.DuckDb`/`SecretRow`/`execute`/`quote_literal`), `pyarrow` (the row projection and the scan-side column reads), `expression` (`Block`/`Map`/`Option` the folds ride), `msgspec` (`Struct` the frozen owner), `beartype` (`@beartype(conf=FAULT_CONF)` on `of`), runtime (`RuntimeRail`/`FAULT_CONF`/`BoundaryFault`/`async_boundary`/`on_thread`, `clock.Hlc`, `identity.ContentKey`) and the port's own `FactRow`/`Fact`/`DECODE`/`Scan`/`Landing`/`Billed`/`Priced`/`Aggregate`/`Resource`/`Groomed`/`Tombstone`/`SubjectKey` vocabulary.
- Growth: a new port member is one method composing the same two owners; a new object-plane identity is one `SecretRow` both handles inherit; a new landing half is one `Landing` column the probe already resolves; a new lifted column is one `_FACT_SCHEMA` field beside its `_rowed` projection at the port and one `_framed` row here; a new rollup axis is one `_TALLY_SQL` group key beside its `_billed` key field; a new table posture is one `TableLayout` edit; a new table format under this ledger costs zero edits here, the matrix already carrying every arm.
- Boundary: this implementer records NO journal fact and its two handles carry `LakePlane.LEDGER` so the commit owner records none either — the port's own recursion law: a fact minted for a landing on this plane lands through that same landing. Its `landed`, `groomed`, and `claimed` commits therefore appear on the durable stream only as the rows they carry, never as commits of their own.
- Boundary: no provider opens here, no engine is named, and no duration is spelled — `WINDOWS` prices retention at the port and this owner executes the reclaim the horizon hands it. Deleted forms: a second codec beside the port's `ENCODE`/`DECODE`, a landing half derived from a merge receipt's fused output tally, a matched-key probe run after the commit that resolved it, an erasure touching the fact stream, and a custody row on the evidence table.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final, assert_never

import pyarrow as pa
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.data.tabular.columnar import DatasetRef, ScanPlan, SecretRow, execute, quote_literal
from rasm.data.tabular.lakehouse import Lakehouse, LakeOp, LakePlane, TableFormat, TableLayout
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.clock import Hlc
from rasm.runtime.identity import ContentKey
from rasm.runtime.journal import (
    DECODE,
    Aggregate,
    Billed,
    Fact,
    FactRow,
    Groomed,
    Landing,
    Priced,
    Resource,
    Retain,
    Scan,
    SubjectKey,
    Tombstone,
)
from rasm.runtime.lanes import on_thread

# --- [TABLES] ---------------------------------------------------------------------------

# `SCHEMA` at the port publishes the machine-readable `FactRow` component this table transcribes — `payload` stays OPAQUE msgpack this plane never decodes on the write side, and `subjects` lands as a
# list so a subject read prunes on the same column the erasure keys on.
_FACT_SCHEMA: Final[pa.Schema] = pa.schema([
    pa.field("key", pa.string(), nullable=False),
    pa.field("stamp", pa.int64(), nullable=False),
    pa.field("stream", pa.string(), nullable=False),
    pa.field("service", pa.string(), nullable=False),
    pa.field("retention", pa.string(), nullable=False),
    pa.field("payload", pa.large_binary(), nullable=False),
    pa.field("subjects", pa.list_(pa.string()), nullable=False),
    pa.field("tenant", pa.string(), nullable=False),
    pa.field("resource", pa.string(), nullable=False),
    pa.field("quantity", pa.int64(), nullable=False),
])

# custody holds a KEY-VALUE relation, never evidence: one wrapped data key per identity, and `destroyed` empties the slot
# rather than marking it, so a resurrected subject seals under a NEW key and every prior envelope stays unreadable.
_CUSTODY_SCHEMA: Final[pa.Schema] = pa.schema([
    pa.field("tenant", pa.string(), nullable=False),
    pa.field("subject", pa.string(), nullable=False),
    pa.field("wrapped", pa.large_binary(), nullable=False),
])

# arming specs the residence rows carry: the fact plane partitions on stream and retention because a groom scopes by
# retention class and a settlement scans one stream, and buckets tenant rather than pathing per tenant, which would
# mint one directory per customer. Custody partitions on tenant alone — its cardinality IS the tenant roster.
_FACT_LAYOUT: Final[TableLayout] = TableLayout(
    schema=_FACT_SCHEMA,
    partition_by=(("stream", "identity", None), ("retention", "identity", None), ("tenant", "bucket", 16)),
)
_CUSTODY_LAYOUT: Final[TableLayout] = TableLayout(schema=_CUSTODY_SCHEMA, partition_by=(("tenant", "identity", None),))

# --- [OPERATIONS] -----------------------------------------------------------------------


def _armed(handle: Lakehouse, layout: TableLayout) -> "RuntimeRail[Lakehouse]":
    # arming rides the SYNC arm because admission runs before the composition's loop carries producers, and `Ensure`
    # is idempotent, so a re-opened journal re-proves its layout rather than re-creating a relation. The handle rides
    # back out on the rail, so the two opens compose as one chain instead of a construction discarding its own
    # arming verdict — a refused plant then fails admission where a port member cannot tell it from a dead ledger.
    return handle.run(LakeOp.Ensure(layout)).map(lambda _receipt: handle)


def _subject(key: SubjectKey) -> str:
    # ONE predicate spelling for a custody identity: claim, read, and destroy keying differently strands a wrapped
    # key under a row no erasure reaches, which silently defeats the shred the whole plane exists to guarantee.
    return f"tenant = {quote_literal(key.tenant)} AND subject = {quote_literal(key.subject)}"


# ONE grouped statement over the lifted metering columns: the audit stream carries `resource` empty, so the
# `<> ''` clause drops it here rather than at the read, keeping one scan surface serving both a subject export and
# a billing period. `source` is the view the scan arm binds the admitted ref as.
_TALLY_SQL: Final[str] = (
    "SELECT tenant, resource, count(*) AS count, sum(quantity) AS total "
    "FROM source WHERE ({predicate}) AND resource <> '' GROUP BY tenant, resource"
)


def _billed(table: pa.Table) -> Billed:
    # `_billed` walks AGGREGATE rows alone because the engine already reduced the window — one `Priced` key per
    # `(tenant, resource)` pair, `attributed` deriving off the empty-tenant spelling exactly as `Priced.of` derives
    # it from an absent one, so a pushed-down tally and a folded one answer the identical `Map`.
    return Map.of_seq(
        (
            Priced(attributed=bool(tenant), tenant=tenant, resource=Resource(resource)),
            Aggregate(count=count, total=total),
        )
        for tenant, resource, count, total in zip(
            table.column("tenant").to_pylist(),
            table.column("resource").to_pylist(),
            table.column("count").to_pylist(),
            table.column("total").to_pylist(),
            strict=True,
        )
    )


def _framed(rows: Block[FactRow]) -> pa.Table:
    # column-wise by construction — the port hands an already-projected row block, so this fold builds arrays rather
    # than a list of dicts, and `tenant` fills its empty string because a partition column is TOTAL by residence law.
    return pa.table(
        {
            "key": [row.key.hex for row in rows],
            "stamp": [row.stamp.packed for row in rows],
            "stream": [row.stream for row in rows],
            "service": [row.service for row in rows],
            "retention": [row.retention.value for row in rows],
            "payload": [row.payload for row in rows],
            "subjects": [list(row.subjects) for row in rows],
            "tenant": [row.tenant or "" for row in rows],
            "resource": [row.resource.value if row.resource is not None else "" for row in rows],
            "quantity": [row.quantity for row in rows],
        },
        schema=_FACT_SCHEMA,
    )


def _predicate(scan: Scan) -> str:
    # ONE lowering of the port's read coordinate onto the pinned columns: a billing period is a half-open stamp window
    # inside one stream, a portability export is a membership test on the `subjects` list. Both spell the tenant
    # narrowing the same way, so a settlement and an export cannot disagree about which rows belong to a customer.
    match scan:
        case Scan(tag="period", period=window):
            tenant = f" AND tenant = {quote_literal(window.tenant)}" if window.tenant is not None else ""
            return f"stream = {quote_literal(window.stream)} AND stamp >= {window.since.packed} AND stamp < {window.until.packed}{tenant}"
        case Scan(tag="subject", subject=key):
            return f"tenant = {quote_literal(key.tenant)} AND list_contains(subjects, {quote_literal(key.subject)})"
        case unreachable:
            assert_never(unreachable)

# --- [COMPOSITION] ----------------------------------------------------------------------


class FactJournal(Struct, frozen=True):
    facts: Lakehouse
    custody: Lakehouse
    fact_ref: DatasetRef
    custody_ref: DatasetRef

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(
        cls, facts: DatasetRef, custody: DatasetRef, table_format: TableFormat = TableFormat.DELTA, *, secrets: tuple[SecretRow, ...] = ()
    ) -> "RuntimeRail[FactJournal]":
        # both handles admit through the SAME `_ADMIT` row read every other lakehouse consumer crosses, so a ref whose
        # source shape the format refuses fails here rather than inside a port member the retry cannot distinguish
        # from a dead ledger. Both take the SAME credential rows: the two planes carry different retention and
        # different access posture but one object-plane identity, and a custody plane whose wrapped keys sit under a
        # credential the fact plane resolved and it did not is a shred no erasure can reach.
        # both handles declare `LakePlane.LEDGER`: their commits ARE the durable landing of the port's own facts, so
        # a producer leg on the commit owner would record a fact whose landing re-enters that commit forever. The
        # declaration rides admission because the commit owner cannot infer it — this plane's commits carry no
        # residence, exactly as a caller's table's do.
        # both relations ARM here, at admission, off the layouts this page pins: a port member reaching an unplanted
        # table answers a provider fault the drain's never-shedding retry then chases forever, indistinguishable from
        # a dead ledger, and the very first `landed` on a fresh residence is exactly that call. Opening two handles
        # and returning them unarmed left the two layouts declared and unreachable.
        return (
            Lakehouse.open(facts, table_format, secrets=secrets, plane=LakePlane.LEDGER)
            .bind(lambda held: _armed(held, _FACT_LAYOUT))
            .bind(
                lambda held: Lakehouse.open(custody, table_format, secrets=secrets, plane=LakePlane.LEDGER)
                .bind(lambda keys: _armed(keys, _CUSTODY_LAYOUT))
                .map(lambda keys: cls(facts=held, custody=keys, fact_ref=facts, custody_ref=custody))
            )
        )

    async def landed(self, rows: Block[FactRow], /) -> "RuntimeRail[Landing]":
        # MERGE on the content key alone is what makes an at-least-once drain idempotent: a redelivered row matches and
        # updates to its own values, a genuine row inserts. The matched half resolves BEFORE that commit, because after
        # one every offered key reads present and the halves become indistinguishable — and no merge receipt slot names
        # WHICH rows landed, so the port's accepted-only projection would have nothing to filter on. `num_output_rows`
        # is the trap the probe replaces: it counts the rewritten output files (inserted + updated + COPIED), so
        # reading it as an acceptance tally reports more rows than were offered and drives `duplicate` to zero
        # permanently, converting every redelivery into a silent double-charge.
        keys = rows.map(lambda row: row.key)
        # a MERGE admits ONE source row per matched target, so a batch carrying one key twice aborts the commit at
        # every admitted format rather than landing half of it. The batch refuses by name here instead: the halves
        # below count each offered key exactly once, so folding a repeat away would report an accepted row the plane
        # never separately held, and content identity makes the repeat a producer defect rather than a lawful
        # redelivery — a redelivery arrives in a LATER batch and lands on the matched half.
        if len(set(keys)) != len(keys):
            return Error(BoundaryFault(boundary=("journal.landed", "the offered batch carries one content key twice")))
        # HEX is what the probe answers, because that is the spelling the durable `key` column holds: `_framed`
        # writes `row.key.hex` and a scan reads text back, so both halves partition on that same projection.
        # Comparing a `ContentKey` against those strings matches nothing ever, which empties `duplicate` permanently
        # and hands the port's accepted-only projection every redelivery as a fresh row — the exact double-charge
        # this probe replaced `num_output_rows` to prevent, reintroduced one type below it.
        match await self._matched(keys):
            case Result(tag="ok", ok=held):
                landed = await self.facts.run_async(
                    LakeOp.Merge("target.key = source.key", Map.of_seq([("key", "source.key")]), delete_unmatched=False), _framed(rows)
                )
                return landed.map(
                    lambda _receipt: Landing(
                        accepted=keys.filter(lambda key: key.hex not in held), duplicate=keys.filter(lambda key: key.hex in held)
                    )
                )
            case refused:
                return Error(refused.error)

    async def _matched(self, keys: Block[ContentKey]) -> "RuntimeRail[frozenset[str]]":
        # ONE bounded key probe per batch on the pinned `key` column, which the layout sorts, so the read prunes to the
        # few files a batch of drain width can touch. Each fact carries its own stamp inside the payload it keys on, so
        # a key standing here is a redelivery of THIS plane's own prior attempt and never a distinct fact a peer minted
        # — which is what makes the pre-commit read exact rather than a race. The port's batching window never offers an
        # empty block, so the membership list always spells at least one literal.
        # `quote_literal` takes the TEXT the column stores, so the literal list is built off `key.hex`: handing it a
        # `ContentKey` renders that struct's repr into the `IN` list and the predicate matches no row on any engine.
        listed = ", ".join(quote_literal(key.hex) for key in keys)
        plan = ScanPlan.DuckDb(f"SELECT key FROM source WHERE key IN ({listed})", ())
        railed = await async_boundary("journal.matched", lambda: on_thread(lambda: execute(plan, self.fact_ref)))
        return railed.map(lambda table: frozenset(table.column("key").to_pylist()))

    async def scanned(self, scan: Scan, /) -> "RuntimeRail[Block[Fact]]":
        # each payload decodes through the PORT's own `DECODE`, so a family widened there lifts here with zero edits and
        # no second codec can drift from the encoder that keyed the row.
        return (await self._read(scan)).map(lambda table: Block.of_seq(DECODE(payload) for payload in table.column("payload").to_pylist()))

    async def tallied(self, scan: Scan, /) -> "RuntimeRail[Billed]":
        # whichever engine indexes the window owns the rollup, which is exactly what the port delegates here: a
        # settlement month folded row-wise allocates one object per metered fact to produce a handful of slots. The
        # port lifts `(resource, quantity)` onto `FactRow`, so the group-by pushes down as SQL and only the aggregate
        # rows cross, `Priced`/`Aggregate` rebuilding off those four columns. `""` is the unattributed spelling on
        # both sides, so `attributed` derives here exactly as `Priced.of` derives it from an absent `tenant`.
        # Engines carrying no grouped read fold `Aggregate.rolled` over their own scan instead; DuckDB has one.
        plan = ScanPlan.DuckDb(_TALLY_SQL.format(predicate=_predicate(scan)), ())
        railed = await async_boundary("journal.tally", lambda: on_thread(lambda: execute(plan, self.fact_ref)))
        return railed.map(_billed)

    async def groomed(self, horizon: Map[Retain, Hlc], /) -> "RuntimeRail[Groomed]":
        # one clause per priced class, folded into ONE delete: a class absent from the horizon contributes no clause, so
        # a permanent class survives by construction rather than by a guard a later edit can drop.
        clauses = " OR ".join(f"(retention = {quote_literal(clazz.value)} AND stamp < {cutoff.packed})" for clazz, cutoff in horizon.items())
        if not clauses:
            return Ok(Groomed(reclaimed=0))
        reclaimed = await self.facts.run_async(LakeOp.Delete(clauses))
        return reclaimed.map(lambda receipt: Groomed(reclaimed=receipt.quantity))

    async def claimed(self, subject: SubjectKey, wrapped: bytes, /) -> "RuntimeRail[bytes]":
        # ATOMIC claim: the merge inserts when absent and leaves a standing row untouched — an empty update map is what
        # makes the matched arm a no-op — then the read answers whichever key won. Two recorders racing one subject
        # therefore seal under ONE data key, where sealing under two leaves half that subject's evidence readable
        # after either is destroyed. The claim ORDERS before the seal, so the loser unwraps the winner's key.
        row = pa.table({"tenant": [subject.tenant], "subject": [subject.subject], "wrapped": [wrapped]}, schema=_CUSTODY_SCHEMA)
        merged = await self.custody.run_async(
            LakeOp.Merge("target.tenant = source.tenant AND target.subject = source.subject", Map.empty(), delete_unmatched=False), row
        )
        match merged:
            case Result(tag="error", error=fault):
                return Error(fault)
            case Result(tag="ok"):
                return (await self.held(subject)).bind(
                    lambda standing: standing.map(Ok).default_with(
                        lambda: Error(BoundaryFault(boundary=("journal.claim", "custody row vanished between claim and read")))
                    )
                )
            case _ as unreachable:
                assert_never(unreachable)

    async def held(self, subject: SubjectKey, /) -> "RuntimeRail[Option[bytes]]":
        # custody reads as a point lookup on the one `_subject` predicate every custody member spells, so a wrapped
        # key can never sit under a row an erasure fails to reach.
        plan = ScanPlan.DuckDb(f"SELECT wrapped FROM source WHERE {_subject(subject)}", ())
        railed = await async_boundary("journal.custody", lambda: on_thread(lambda: execute(plan, self.custody_ref)))
        return railed.map(lambda table: Some(table.column("wrapped")[0].as_py()) if table.num_rows else Nothing)

    async def destroyed(self, stone: Tombstone, /) -> "RuntimeRail[Option[Tombstone]]":
        # erasure empties the CUSTODY slot and touches no fact row — the append-only invariant survives the right to
        # erasure because unreadable IS erased — and the echo carries back exactly what this plane persisted, since the
        # port mints the stamp and supplies both the order and the identity this member never invents.
        key = SubjectKey(tenant=stone.tenant, subject=stone.subject)
        removed = await self.custody.run_async(LakeOp.Delete(_subject(key)))
        return removed.map(lambda receipt: Some(stone) if receipt.quantity else Nothing)

    async def _read(self, scan: Scan) -> "RuntimeRail[pa.Table]":
        # `columnar.execute` is a synchronous rail, so the port's awaitable contract holds through one offload rather
        # than an on-loop scan that stalls every producer suspended behind the drain. The scan arm binds the admitted
        # ref as the one `source` view, so every statement this owner writes selects FROM it and never self-sources.
        return await async_boundary(
            "journal.scan", lambda: on_thread(lambda: execute(ScanPlan.DuckDb(f"SELECT * FROM source WHERE {_predicate(scan)}", ()), self.fact_ref))
        )

```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
