import { mkdir } from 'node:fs/promises';
import { join } from 'node:path';
import { layer as sqlite } from '@effect/sql-sqlite-node/SqliteClient';
import { Catalog } from '@rasm/database/catalog';
import { Column } from '@rasm/database/column';
import { Relation } from '@rasm/database/relation';
import { Sqlite } from '@rasm/database/sqlite';
import { Array, Cause, Context, Effect, Exit, type Fiber, type FiberSet, Function, Layer, Option, RcMap, Record, Redacted, Ref, Result, Schema, Stream, Struct, TxRef, TxSubscriptionRef, UndefinedOr } from 'effect';
import { McpSchema } from 'effect/ai';
import { Jwk } from 'effect/jose';
import { SqlClient } from 'effect/sql';
import { Handles, Notice } from '#/handles';

// --- [TYPES] ---------------------------------------------------------------------------

interface HandleState {
    readonly state: Context.Context<never>;
    readonly pending: Record.ReadonlyRecord<string, Schema.Json>;
    readonly dropped: number;
    readonly held: readonly ((slot: HandleState) => HandleState)[];
    readonly notices: readonly Notice[];
    readonly handed: readonly Notice[];
    readonly rows: typeof Handles.Rows.Type;
    readonly blocks: readonly McpSchema.ContentBlock[];
    readonly calls: number;
    readonly answering: number;
    readonly running: readonly {
        readonly call: number;
        readonly tool: string;
        readonly fiber: Fiber.Fiber<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError>;
        readonly ends: Stream.Stream<Effect.Effect<string>>;
        readonly done: TxRef.TxRef<Option.Option<Exit.Exit<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError>>>;
    }[];
}

// --- [MODELS] --------------------------------------------------------------------------

const HandleRecord = Schema.Struct({
    state: Schema.Record(Schema.String, Schema.Json),
    dropped: Schema.Int,
    notices: Schema.Array(Notice),
    handed: Schema.Array(Notice),
    rows: Handles.Rows,
    calls: Schema.Int,
    running: Schema.Array(Schema.Struct({ call: Schema.Int, tool: Schema.String })),
});
const STORED = [...Struct.keys(HandleRecord.fields), 'pending'] as const;
const tables = {
    servers: Relation.make('servers', { scheme: Schema.String, sequence: Schema.Int, secret: Schema.fromJsonString(Jwk.SecretJwk) }, { key: ['scheme'], withoutRowid: true }),
    handles: Relation.make(
        'handles',
        {
            handle: Column.Uuid,
            body: HandleRecord.pipe(
                Schema.OptionFromNullOr,
                Schema.catchDecoding((issue) => Effect.as(Effect.logInfo('Handle store dropped a handle this build no longer reads', new Schema.SchemaError(issue)), Option.some(Option.none()))),
                Schema.fromJsonString,
            ),
        },
        { key: ['handle'], withoutRowid: true, retired: ['touched'] },
    ),
};
const catalog = Catalog.make({ tables: [tables.servers, tables.handles] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const unchanged = (before: HandleState | undefined, slot: HandleState) => STORED.every((field) => slot[field] === before?.[field]);
const callResult = (exit: Exit.Exit<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError>) =>
    Exit.match(exit, {
        onSuccess: (result) => (result instanceof McpSchema.InputRequired ? McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: 'Asked for client input, call the tool again' })] }) : result),
        onFailure: (cause) =>
            Cause.hasInterruptsOnly(cause)
                ? McpSchema.CallToolResult.make({ content: [McpSchema.TextContent.make({ text: 'interrupted, server restarted, outcome unread' })] })
                : McpSchema.CallToolResult.make({ isError: true, content: Array.map(Cause.prettyErrors(cause), ({ message }) => McpSchema.TextContent.make({ text: message })) }),
    });
const encodeHandle = (handle: string, slot: HandleState, registry: Record.ReadonlyRecord<string, (state: Context.Context<never>) => Effect.Effect<Schema.Json>>) =>
    Effect.map(Effect.all(Record.map(registry, Function.apply(slot.state))), (state) => ({
        handle,
        body: Option.some({ state: { ...slot.pending, ...state }, dropped: slot.dropped, notices: slot.notices, handed: slot.handed, rows: slot.rows, calls: slot.calls, running: Array.map(slot.running, ({ call, tool }) => ({ call, tool })) }),
    }));

const openStore = Effect.fnUntraced(function* ({
    scheme,
    folder,
    live,
    handles,
    persisted,
}: {
    readonly scheme: string;
    readonly folder: string | undefined;
    readonly live: TxSubscriptionRef.TxSubscriptionRef<Record.ReadonlyRecord<string, TxSubscriptionRef.TxSubscriptionRef<HandleState>>>;
    readonly handles: RcMap.RcMap<string, { readonly id: string; readonly row: TxSubscriptionRef.TxSubscriptionRef<HandleState>; readonly fibers: FiberSet.FiberSet<unknown, unknown> }>;
    readonly persisted: Ref.Ref<Record.ReadonlyRecord<string, (state: Context.Context<never>) => Effect.Effect<Schema.Json>>>;
}) {
    const filename = yield* UndefinedOr.match(folder, {
        onUndefined: () => Effect.succeed(':memory:'),
        onDefined: (root) => Effect.as(Effect.tryPromise({ try: () => mkdir(root, { recursive: true }), catch: (cause) => new Cause.UnknownError(cause, `State folder ${root} cannot be created`) }), join(root, 'handles.sqlite')),
    });
    const context = yield* Catalog.layer(
        catalog,
        Sqlite.layer((names) => sqlite({ filename, ...names })),
    ).pipe(Layer.build);
    const [servers, stored] = yield* Effect.provideContext(Effect.all([Relation.select(tables.servers, { scheme: [scheme] }), Relation.select(tables.handles, {})]), context);
    const secret = yield* Option.match(Array.head(servers), { onNone: () => Jwk.generateSecret('dir'), onSome: (row) => Effect.succeed(Redacted.make(row.secret)) });
    const counted = Option.match(Array.head(servers), { onNone: () => 0, onSome: Struct.get('sequence') });
    const [restored, expired] = Array.partition(stored, (row) => Option.match(row.body, { onNone: () => Result.fail(row.handle), onSome: (body) => Result.succeed({ ...row, body }) }));
    yield* Effect.provideContext(Effect.all([Relation.upsert(tables.servers, [{ scheme, sequence: counted, secret: Redacted.value(secret) }]), Array.match(expired, { onEmpty: () => Effect.void, onNonEmpty: (gone) => Relation.remove(tables.handles, { handle: gone }) })]), context);
    const sequence = yield* TxRef.make(counted);
    const cut = yield* Effect.forEach(
        restored,
        Effect.fnUntraced(function* ({ handle, body }: (typeof restored)[number]) {
            const { row } = yield* RcMap.get(handles, handle);
            yield* TxSubscriptionRef.update(row, (slot) => ({ ...slot, pending: body.state, dropped: body.dropped, notices: body.notices, handed: body.handed, rows: body.rows, calls: body.calls }));
            return Array.map(body.running, (each) => ({ ...each, row }));
        }, Effect.scoped),
    );
    const read = Effect.tx(Effect.flatMap(Effect.all([TxRef.get(sequence), TxSubscriptionRef.get(live)]), ([at, rows]) => Effect.map(Effect.all(Record.map(rows, TxSubscriptionRef.get)), (slots) => ({ at, slots }))));
    const written = yield* Ref.make((yield* read).slots);
    const flush = Effect.fnUntraced(
        function* (whole: boolean) {
            const [{ at, slots }, registry, before] = yield* Effect.all([read, Ref.get(persisted), Ref.get(written)]);
            const changed = yield* Effect.all(
                Record.collect(
                    Record.filter(slots, (slot, handle) => whole || !unchanged(before[handle], slot)),
                    (handle, slot) => encodeHandle(handle, slot, registry),
                ),
            );
            const gone = Array.filter(Record.keys(before), (handle) => !Record.has(slots, handle));
            const statements = [
                ...Array.match(changed, { onEmpty: Array.empty, onNonEmpty: (each) => [Relation.upsert(tables.servers, [{ scheme, sequence: at, secret: Redacted.value(secret) }]), Relation.upsert(tables.handles, each)] }),
                ...Array.match(gone, { onEmpty: Array.empty, onNonEmpty: (each) => [Relation.remove(tables.handles, { handle: each })] }),
            ];
            yield* Array.match(statements, { onEmpty: () => Effect.void, onNonEmpty: (each) => Context.get(context, SqlClient.SqlClient).withTransaction(Effect.all(each)) });
            yield* Ref.set(written, slots);
        },
        Effect.provideContext(context),
        Effect.catchCause((cause) => Effect.logWarning('Handle store flush failed, the next change writes again', cause)),
    );
    yield* Effect.addFinalizer(() => flush(true));
    yield* live.pipe(
        TxSubscriptionRef.changesStream,
        Stream.switchMap((rows) =>
            Stream.merge(
                Stream.make(rows),
                Stream.mergeAll(
                    Array.map(Record.values(rows), (row) => Stream.changesWith(TxSubscriptionRef.changesStream(row), unchanged)),
                    { concurrency: 'unbounded' },
                ),
            ),
        ),
        Stream.runForEach(() => flush(false)),
        Effect.forkScoped,
    );
    return { secret, sequence, cut: Array.flatten(cut) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { callResult, type HandleState, openStore };
