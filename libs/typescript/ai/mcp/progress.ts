import { Effect, Option, Schema, Stream } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';

// --- [TYPES] ---------------------------------------------------------------------------

interface Step {
    readonly progress: number;
    readonly total: Option.Option<number>;
    readonly message: Option.Option<string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const token = Schema.decodeUnknownOption(McpSchema.ProgressToken);
const progress = Effect.fnUntraced(function* <E, R>(work: Stream.Stream<Step, E, R>) {
    const [server, request] = yield* Effect.all([McpServer.McpServer, Effect.serviceOption(McpSchema.McpRequestContext)]);
    return yield* Option.match(
        Option.flatMap(request, ({ requestMetadata }) => token(requestMetadata?.progressToken)),
        {
            onNone: () => Stream.runDrain(work),
            onSome: (progressToken) => Stream.runForEach(work, (step) => server.notifications['notifications/progress']({ progressToken, progress: step.progress, total: Option.getOrUndefined(step.total), message: Option.getOrUndefined(step.message) })),
        },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { progress };
