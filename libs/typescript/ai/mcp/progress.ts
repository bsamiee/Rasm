import { Effect, Option, Schema, Stream } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';

// --- [OPERATIONS] ----------------------------------------------------------------------

const decodeToken = Schema.decodeUnknownOption(McpSchema.ProgressToken);
const progress =
    <A>(report: (value: A, index: number) => Omit<typeof McpSchema.ProgressNotification.payloadSchema.Type, 'progressToken' | '_meta'>) =>
    <E, R>(self: Stream.Stream<A, E, R>): Stream.Stream<A, E, R | McpServer.McpServer> =>
        Stream.unwrap(
            Effect.map(Effect.all([McpServer.McpServer, Effect.serviceOption(McpSchema.McpRequestContext)]), ([server, request]) =>
                Option.match(
                    Option.flatMap(request, ({ requestMetadata }) => decodeToken(requestMetadata?.progressToken)),
                    {
                        onNone: () => self,
                        onSome: (progressToken) => Stream.mapEffect(self, (value, index) => Effect.as(server.notifications['notifications/progress']({ progressToken, ...report(value, index) }), value)),
                    },
                ),
            ),
        );

// --- [EXPORTS] -------------------------------------------------------------------------

export { progress };
