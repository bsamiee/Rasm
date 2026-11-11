import { Context, Effect, Layer, Pool, Predicate } from 'effect';
import { McpClient, McpClientError } from 'effect/ai';

// --- [SERVICES] ------------------------------------------------------------------------

class Client extends Context.Service<Client, { readonly run: <A, E, R>(use: (client: McpClient.McpClient) => Effect.Effect<A, E, R>) => Effect.Effect<A, E | McpClientError.McpClientError, R> }>()('@rasm/mcp/client') {
    static readonly lost = (error: unknown) => McpClientError.isMcpClientError(error) && (Predicate.isTagged(error.reason, 'ClosedError') || Predicate.isTagged(error.reason, 'TransportError'));
    static readonly layer = <R>(connection: Layer.Layer<McpClient.McpClient, McpClientError.McpClientError, R>) =>
        Layer.effect(
            Client,
            Effect.gen(function* () {
                const connections = yield* Pool.make({ acquire: Effect.map(Layer.build(Layer.fresh(connection)), Context.get(McpClient.McpClient)), size: 1, concurrency: Number.POSITIVE_INFINITY });
                const attempt = Effect.fnUntraced(function* <A, E, R2>(use: (client: McpClient.McpClient) => Effect.Effect<A, E, R2>) {
                    const client = yield* Pool.get(connections);
                    return yield* Effect.tapError(use(client), (error) => (Client.lost(error) ? Pool.invalidate(connections, client) : Effect.void));
                }, Effect.scoped);
                return {
                    run: (use) =>
                        Effect.catchIf(
                            attempt(use),
                            (error) => McpClientError.isMcpClientError(error) && Predicate.isTagged(error.reason, 'ClosedError') && error.reason.sessionExpired === true,
                            () => attempt(use),
                        ),
                };
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Client };
