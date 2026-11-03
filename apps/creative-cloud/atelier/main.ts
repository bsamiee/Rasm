import process from 'node:process';
import { NodeCrypto } from '@effect/platform-node';
import { Automation } from '@rasm/automation/automation';
import { main } from '@rasm/mcp/node';
import { Array, Config, Duration, Effect, identity, Layer, Option, Result, ScopedRef, Sink, Stream, SubscriptionRef } from 'effect';
import { command, dictionaries } from '#/features/apple-events';
import { applications, launch, perform, screenshot } from '#/features/applications';
import { faces, font } from '#/features/fonts';
import { listening, plugins } from '#/features/plugins';
import { typesetting } from '#/features/typesetting';
import manifest from '#/package.json' with { type: 'json' };
import { routes } from '#/routes';
import { status } from '#/status';

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.gen(function* () {
    const [automation, links, dependencies] = yield* Effect.all([Automation, listening, status]);
    yield* Effect.forkScoped(
        Effect.gen(function* () {
            yield* Effect.logError('Atelier host ended', yield* Effect.exit(automation.exited));
            return yield* Effect.sync(() => process.exit(1));
        }),
    );
    yield* Effect.all([applications, faces], { discard: true });
    const round = Effect.gen(function* () {
        const routed = yield* routes(dependencies, links);
        yield* Effect.all([dictionaries(routed), launch(routed, dependencies), screenshot(routed), perform(routed, dependencies), command(routed), font(routed), typesetting(routed), plugins(routed, links)], { discard: true });
    });
    const discovered = SubscriptionRef.changes(automation.installations).pipe(
        Stream.tap(Result.match({ onSuccess: () => Effect.void, onFailure: (error) => Effect.logError('Adobe application discovery failed, tools keep the last installations', error) })),
        Stream.filterMap(identity),
        Stream.map(Array.map((row) => [row.name, row.url.href] as const)),
        Stream.changes,
    );
    const [first, rest] = yield* Stream.peel(discovered, Sink.head());
    const generation = yield* ScopedRef.fromAcquire(Option.match(first, { onNone: () => Effect.void, onSome: () => round }));
    yield* Effect.forkScoped(Stream.runForEach(Stream.merge(rest, Stream.drop(links.changes, 1)), () => ScopedRef.set(generation, round)));
});

main(
    manifest,
    { retention: Duration.hours(1), instructions: ['Specifiers come from results', 'State-changing calls return early when a dialog requires an answer', 'No tool changes frontmost application'] },
    Layer.effectDiscard(served).pipe(Layer.provide(Automation.layer(Config.String('ATELIER_PATH'))), Layer.provide(NodeCrypto.layer)),
);
