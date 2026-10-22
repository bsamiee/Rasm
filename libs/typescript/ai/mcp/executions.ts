import { Array, Deferred, Effect, Option, Ref, Schema } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';

// --- [OPERATIONS] ----------------------------------------------------------------------

const executions = Effect.fnUntraced(function* (scheme: string) {
    const Row = Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: () => undefined })]), McpSchema.McpError, Schema.Defect())) });
    const [server, scope, kept] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make(Array.empty<Effect.Effect<typeof Row.Type>>())]);
    const listed = {
        uri: `${scheme}://executions`,
        name: 'executions',
        description:
            'Calls that change state answered before their outcome, cancelled or refused while running, each with its arguments and completed outcome, including failures and requested input. A cancelled call, or a refusal linking here, can have run, so read its outcome here before repeating it. Reads drop a completed call after listing it',
        mimeType: 'application/json',
    };
    const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row))));
    const registration = yield* Effect.cached(
        McpServer.registerResource({
            ...listed,
            content: Effect.gen(function* () {
                const rows = yield* Ref.get(kept);
                const read = yield* Effect.all(rows);
                yield* Ref.update(kept, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                return { contents: [{ uri: listed.uri, mimeType: listed.mimeType, text: yield* Effect.orDie(encode(read)) }] };
            }),
        }).pipe(Effect.provideService(McpServer.McpServer, server)),
    );
    const record = Effect.fnUntraced(function* <E extends McpSchema.McpError, R>(tool: string, given: Schema.Json, answer: Effect.Effect<McpSchema.CallToolResult | McpSchema.InputRequired, E, R>, early: Deferred.Deferred<McpSchema.CallToolResult>) {
        const done = yield* Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, E>();
        const recorded = Ref.update(
            kept,
            Array.append(
                Deferred.poll(done).pipe(
                    Effect.map(Option.map(Effect.exit)),
                    Effect.flatMap(Effect.transposeOption),
                    Effect.map((result) => ({ tool, arguments: given, result })),
                ),
            ),
        );
        yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
        const [outcome, pending] = yield* Effect.raceFirst(
            Effect.map(Deferred.await(done), (value) => [value, false] as const),
            Effect.map(Deferred.await(early), (value) => [value, true] as const),
        ).pipe(
            Effect.interruptible,
            Effect.onInterrupt(() => recorded),
        );
        return yield* Effect.as(pending ? recorded : Effect.void, outcome);
    }, Effect.uninterruptible);
    return { registration, record, link: McpSchema.ResourceLink.make(listed) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { executions };
