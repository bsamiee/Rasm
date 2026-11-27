import { mkdir } from 'node:fs/promises';
import { join } from 'node:path';
import { layer as sqlite } from '@effect/sql-sqlite-node/SqliteClient';
import { Catalog } from '@rasm/database/catalog';
import { Column } from '@rasm/database/column';
import { Relation } from '@rasm/database/relation';
import { Sqlite } from '@rasm/database/sqlite';
import { Array, Cause, Context, DateTime, Duration, Effect, Exit, type Fiber, FiberSet, Function, Layer, Number, Option, RcMap, Record, Redacted, Ref, Result, Schedule, Schema, Sink, Stream, Struct, Tuple, TxRef, TxSubscriptionRef, UndefinedOr } from 'effect';
import { McpSchema } from 'effect/ai';
import { Jwk } from 'effect/jose';
import { SqlClient } from 'effect/sql';
import { Handles } from '#/handles';

// --- [TYPES] ---------------------------------------------------------------------------

interface Entry {
    readonly sequence: number;
    readonly text: Effect.Effect<string>;
    readonly owns: (state: Context.Context<never>) => boolean;
    readonly leads: (state: Context.Context<never>) => boolean;
    readonly standing: Option.Option<typeof Handles.Standing.Type>;
    readonly delivers: (state: Context.Context<never>) => Option.Option<typeof Handles.Kind.Type | typeof Handles.Standing.cases.Waiting.Type>;
}
interface HandleState {
    readonly state: Context.Context<never>;
    readonly pending: Record.ReadonlyRecord<string, Schema.Json>;
    readonly touched: Option.Option<DateTime.Utc>;
    readonly dropped: number;
    readonly held: readonly Entry[];
    readonly entries: readonly Entry[];
    readonly sent: readonly Entry[];
    readonly calls: number;
    readonly answering: number;
    readonly running: readonly { readonly call: number; readonly tool: string; readonly fiber: Fiber.Fiber<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError>; readonly ends: Stream.Stream<Effect.Effect<string>> }[];
}

// --- [MODELS] --------------------------------------------------------------------------

const HandleRecord = Schema.Struct({
    state: Schema.Record(Schema.String, Schema.Json).annotate({ description: 'Encoded value of each reference a product persists, keyed by reference key' }),
    dropped: Schema.Int,
    entries: Schema.Array(
        Schema.Struct({
            sequence: Schema.Int,
            text: Schema.String,
            standing: Schema.OptionFromOptionalKey(Handles.Standing),
            delivers: Schema.OptionFromOptionalKey(Schema.Union([Handles.Kind, Handles.Standing.cases.Waiting])),
            leads: Schema.Boolean,
        }),
    ).annotate({ description: 'Events owned and not yet acknowledged, sent ones included, in sequence order' }),
    calls: Schema.Int,
    ended: Schema.Array(Schema.Struct({ call: Schema.Int, tool: Schema.String, result: McpSchema.CallToolResult })).annotate({ description: 'Calls that returned early, with the outcome each reached or its interruption' }),
});
const tables = {
    servers: Relation.make('servers', { scheme: Schema.String, sequence: Schema.Int, secret: Schema.fromJsonString(Jwk.SecretJwk) }, { key: ['scheme'], withoutRowid: true }),
    handles: Relation.make(
        'handles',
        {
            handle: Column.Uuid,
            touched: Schema.DateTimeUtcFromMillis,
            body: HandleRecord.pipe(
                Schema.OptionFromNullOr,
                Schema.catchDecoding((issue) => Effect.as(Effect.logInfo('Handle store dropped a handle this build no longer reads', new Schema.SchemaError(issue)), Option.some(Option.none()))),
                Schema.fromJsonString,
            ),
        },
        { key: ['handle'], withoutRowid: true },
    ),
};
const catalog = Catalog.make({ tables: [tables.servers, tables.handles] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const callResult = (exit: Exit.Exit<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError>) =>
    Exit.match(exit, {
        onSuccess: (result) => (result instanceof McpSchema.InputRequired ? McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: 'Asked for client input, call the tool again' })] }) : result),
        onFailure: (cause) => McpSchema.CallToolResult.make({ isError: true, content: Array.map(Cause.hasInterruptsOnly(cause) ? ['interrupted by a server restart'] : Array.map(Cause.prettyErrors(cause), Struct.get('message')), (text) => McpSchema.TextContent.make({ text })) }),
    });
const encodeHandle = Effect.fnUntraced(function* (handle: string, slot: HandleState, registry: Record.ReadonlyRecord<string, (state: Context.Context<never>) => Effect.Effect<Schema.Json>>, render: (text: Effect.Effect<string>) => Effect.Effect<string>) {
    const [now, state, entries] = yield* Effect.all([
        DateTime.now,
        Effect.all(Record.map(registry, Function.apply(slot.state))),
        Effect.forEach(Array.sortWith([...slot.sent, ...slot.entries], Struct.get('sequence'), Number.Order), (entry) => Effect.map(render(entry.text), (text) => ({ sequence: entry.sequence, text, standing: entry.standing, delivers: entry.delivers(slot.state), leads: entry.leads(slot.state) }))),
    ]);
    return {
        handle,
        touched: Option.getOrElse(slot.touched, () => now),
        body: Option.some({ state: { ...slot.pending, ...state }, dropped: slot.dropped, entries, calls: slot.calls, ended: Array.map(slot.running, ({ call, tool, fiber }) => ({ call, tool, result: callResult(fiber.pollUnsafe() ?? Exit.interrupt()) })) }),
    };
});

const openStore = Effect.fnUntraced(function* ({
    scheme,
    retention,
    folder,
    live,
    handles,
    persisted,
    render,
}: {
    readonly scheme: string;
    readonly retention: Duration.Duration;
    readonly folder: string | undefined;
    readonly live: TxSubscriptionRef.TxSubscriptionRef<Record.ReadonlyRecord<string, TxSubscriptionRef.TxSubscriptionRef<HandleState>>>;
    readonly handles: RcMap.RcMap<string, { readonly id: string; readonly row: TxSubscriptionRef.TxSubscriptionRef<HandleState>; readonly fibers: FiberSet.FiberSet<unknown, unknown> }>;
    readonly persisted: Ref.Ref<Record.ReadonlyRecord<string, (state: Context.Context<never>) => Effect.Effect<Schema.Json>>>;
    readonly render: (row: TxSubscriptionRef.TxSubscriptionRef<HandleState>) => (text: Effect.Effect<string>) => Effect.Effect<string>;
}) {
    const filename = yield* UndefinedOr.match(folder, {
        onUndefined: () => Effect.succeed(':memory:'),
        onDefined: (root) => Effect.as(Effect.tryPromise({ try: () => mkdir(root, { recursive: true }), catch: (cause) => new Cause.UnknownError(cause, `State folder ${root} cannot be created`) }), join(root, 'handles.sqlite')),
    });
    const context = yield* Catalog.layer(
        catalog,
        Sqlite.layer((names) => sqlite({ filename, ...names })),
    ).pipe(Layer.build);
    const [now, servers, stored] = yield* Effect.provideContext(Effect.all([DateTime.now, Relation.select(tables.servers, { scheme: [scheme] }), Relation.select(tables.handles, {})]), context);
    const secret = yield* Option.match(Array.head(servers), { onNone: () => Jwk.generateSecret('dir'), onSome: (row) => Effect.succeed(Redacted.make(row.secret)) });
    const counted = Option.match(Array.head(servers), { onNone: () => 0, onSome: Struct.get('sequence') });
    const [restored, expired] = Array.partition(stored, (row) =>
        Option.match(
            Option.filter(row.body, () => DateTime.isGreaterThan(DateTime.addDuration(row.touched, retention), now)),
            { onNone: () => Result.fail(row.handle), onSome: (body) => Result.succeed({ ...row, body }) },
        ),
    );
    yield* Effect.provideContext(Effect.all([Relation.upsert(tables.servers, [{ scheme, sequence: counted, secret: Redacted.value(secret) }]), Array.match(expired, { onEmpty: () => Effect.void, onNonEmpty: (gone) => Relation.remove(tables.handles, { handle: gone }) })]), context);
    const [scope, sequence] = yield* Effect.all([Effect.scope, TxRef.make(counted)]);
    yield* Effect.forEach(
        restored,
        Effect.fnUntraced(function* ({ handle, touched, body }: (typeof restored)[number]) {
            const { row, fibers } = yield* RcMap.get(handles, handle);
            const running = yield* Effect.forEach(
                body.ended,
                Effect.fnUntraced(function* ({ call, tool, result }: (typeof body.ended)[number]) {
                    return { call, tool, fiber: yield* FiberSet.run(fibers)(Effect.succeed(result)), ends: Stream.never };
                }),
            );
            const entries = Array.map(body.entries, (entry): Entry => ({ sequence: entry.sequence, text: Effect.succeed(entry.text), owns: Function.constTrue, leads: Function.constant(entry.leads), standing: entry.standing, delivers: Function.constant(entry.delivers) }));
            yield* TxSubscriptionRef.update(row, (slot) => ({ ...slot, pending: body.state, touched: Option.some(touched), dropped: body.dropped, entries, calls: body.calls, running }));
            yield* TxSubscriptionRef.get(row).pipe(
                Effect.flatMap((slot) => (slot.answering === 0 && Option.contains(slot.touched, touched) ? RcMap.invalidate(handles, handle) : Effect.void)),
                Effect.delay(Duration.subtract(retention, DateTime.distance(touched, now))),
                Effect.forkIn(scope),
            );
        }, Effect.scoped),
        { discard: true },
    );
    const written = yield* Ref.make({ sequence: counted, slots: yield* Effect.all(Record.map(yield* TxSubscriptionRef.get(live), TxSubscriptionRef.get)) });
    const flush = Effect.fnUntraced(
        function* (whole: boolean) {
            const [rows, at, registry, before] = yield* Effect.all([TxSubscriptionRef.get(live), TxRef.get(sequence), Ref.get(persisted), Ref.get(written)]);
            const slots = yield* Effect.all(Record.map(rows, TxSubscriptionRef.get));
            const changed = yield* Effect.all(
                Record.collect(
                    Record.filter(Record.intersection(slots, rows, Tuple.make), ([slot], handle) => whole || slot !== before.slots[handle]),
                    (handle, [slot, row]) => encodeHandle(handle, slot, registry, render(row)),
                ),
            );
            const gone = Array.filter(Record.keys(before.slots), (handle) => !Record.has(rows, handle));
            const statements = [
                ...(at === before.sequence ? [] : [Relation.upsert(tables.servers, [{ scheme, sequence: at, secret: Redacted.value(secret) }])]),
                ...Array.match(changed, { onEmpty: Array.empty, onNonEmpty: (each) => [Relation.upsert(tables.handles, each)] }),
                ...Array.match(gone, { onEmpty: Array.empty, onNonEmpty: (each) => [Relation.remove(tables.handles, { handle: each })] }),
            ];
            yield* Array.match(statements, { onEmpty: () => Effect.void, onNonEmpty: (each) => Context.get(context, SqlClient.SqlClient).withTransaction(Effect.all(each)) });
            yield* Ref.set(written, { sequence: at, slots });
        },
        Effect.provideContext(context),
        Effect.catchCause((cause) => Effect.logWarning('Handle store flush failed, the next change writes again', cause)),
    );
    yield* Effect.addFinalizer(() => flush(true));
    yield* live.pipe(
        TxSubscriptionRef.changesStream,
        Stream.switchMap((rows) => Stream.mergeAll(Array.map(Record.values(rows), TxSubscriptionRef.changesStream), { concurrency: 'unbounded' })),
        Stream.aggregateWithin(Sink.drain, Schedule.spaced('250 millis')),
        Stream.runForEach(() => flush(false)),
        Effect.forkScoped,
    );
    return { secret, sequence };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { callResult, type Entry, type HandleState, openStore };
