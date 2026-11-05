import type { Launching } from '@rasm/applications';
import { Automation } from '@rasm/applications/node';
import { HostError } from '@rasm/automation';
import { Unmatched } from '@rasm/search';
import { Cause, Context, Effect, Fiber, Layer, Match, Result, Schema, Sink, Stream, SubscriptionRef } from 'effect';
import { Rpc, RpcClient, RpcGroup } from 'effect/rpc';
import { AvailableFace } from '#/available-face';

// --- [MODELS] --------------------------------------------------------------------------

const Helper = RpcGroup.make(Rpc.make('faces', { success: Schema.Array(AvailableFace) }), Rpc.make('face', { payload: { name: AvailableFace.Name }, success: AvailableFace, error: Unmatched }));

// --- [SERVICES] ------------------------------------------------------------------------

class Adobe extends Context.Service<Adobe>()('@rasm/atelier/adobe', {
    make: Effect.gen(function* () {
        const [automation, helper] = yield* Effect.all([Automation, RpcClient.make(Helper)]);
        const [[installations, discovering], [instances, tracking]] = yield* Effect.all([watch('installations', automation.installations({})), watch('instances', automation.instances({}))]);
        return {
            ...helper,
            installations,
            instances,
            installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult),
            exited: Effect.raceFirst(automation.exited, Effect.andThen(Fiber.joinAll([discovering, tracking]), automation.exited)),
            opening: (toOpen: URL) => automation.opening(toOpen, {}),
            launched: (launch: Parameters<typeof automation.launch>[0]) =>
                automation.launch(launch).pipe(
                    Stream.filterMapEffect(
                        Match.type<typeof Launching.Type>().pipe(
                            Match.when({ failed: Match.defined }, ({ failed }) => Effect.fail(HostError.refused(failed.subject)(failed.message))),
                            Match.when({ launched: Match.defined }, ({ launched }) => Effect.succeed(Result.succeed(launched))),
                            Match.orElse(() => Effect.succeed(Result.failVoid)),
                        ),
                    ),
                    Stream.runLast,
                    Effect.flatMap(Effect.fromOption),
                ),
        };
    }),
}) {
    static readonly layer = Layer.effect(Adobe, Adobe.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const watch = Effect.fn('watch')(function* <A, E, R>(subject: string, changes: Stream.Stream<A, E, R>) {
    const [first, rest] = yield* Stream.peel(changes, Sink.head());
    const latest = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} ended before its first value`)));
    return [latest, yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)))] as const;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Adobe };
