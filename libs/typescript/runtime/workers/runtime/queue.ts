import { Effect, Layer, Schema, type Scope } from 'effect';
import { Execution, type Kind } from '#/runtime/execution';

// --- [COMPOSITION] ---------------------------------------------------------------------

const queue = <S extends Schema.Top, E, R>(body: S, work: (body: S['Type'], delivery: Pick<Message, 'id' | 'attempts' | 'timestamp'>) => Effect.Effect<unknown, E, R | Execution | Scope.Scope>): Kind<R | S['DecodingServices']> => ({
    layer: Layer.empty,
    handlers: (ready) => ({
        queue: (batch, _, execution) => {
            const delivered = Effect.forEach(
                batch.messages,
                (message) =>
                    Schema.decodeUnknownEffect(body)(message.body).pipe(
                        Effect.flatMap((decoded) => work(decoded, message)),
                        Effect.tapCause(Effect.logError),
                        Effect.matchCause({ onFailure: () => message.retry(), onSuccess: () => message.ack() }),
                        Effect.withSpan(`process ${batch.queue}`, { kind: 'consumer', attributes: { 'messaging.operation.type': 'process', 'messaging.destination.name': batch.queue, 'messaging.message.id': message.id, 'messaging.message.delivery_attempt': message.attempts } }),
                    ),
                { concurrency: 'unbounded', discard: true },
            );
            return ready().then((isolate) => Execution.run(isolate, execution)(delivered));
        },
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { queue };
