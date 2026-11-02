import { Effect, Option, Schema, Stream } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';

// --- [OPERATIONS] ----------------------------------------------------------------------

const token = Schema.decodeUnknownOption(McpSchema.ProgressToken);
const progress = Effect.fnUntraced(function* <E, R>(work: Stream.Stream<Omit<typeof McpSchema.ProgressNotification.payloadSchema.Type, 'progressToken' | '_meta'>, E, R>) {
    const [server, request] = yield* Effect.all([McpServer.McpServer, Effect.serviceOption(McpSchema.McpRequestContext)]);
    return yield* Option.match(
        Option.flatMap(request, ({ requestMetadata }) => token(requestMetadata?.progressToken)),
        {
            onNone: () => Stream.runDrain(work),
            onSome: (progressToken) => Stream.runForEach(work, (step) => server.notifications['notifications/progress']({ progressToken, ...step })),
        },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { progress };
