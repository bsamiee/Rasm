import process from 'node:process';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/automation/node';
import { serve } from '@rasm/mcp/node';
import { Array, Config, Duration, Effect, identity, Layer, Result, ScopedRef, Sink, Stream, SubscriptionRef } from 'effect';
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
    yield* Effect.exit(automation.exited).pipe(
        Effect.flatMap((exit) => Effect.logError('Atelier host ended', exit)),
        Effect.andThen(Effect.sync(() => process.exit(1))),
        Effect.forkScoped,
    );
    yield* Effect.all([applications, faces], { discard: true });
    const round = Effect.flatMap(routes(dependencies, links), (routed) => Effect.all([dictionaries(routed), launch(routed, dependencies), screenshot(routed), perform(routed, dependencies), command(routed), font(routed), typesetting(routed), plugins(routed, links)], { discard: true }));
    const discovered = SubscriptionRef.changes(automation.installations).pipe(
        Stream.tap(Result.match({ onSuccess: () => Effect.void, onFailure: (error) => Effect.logError('Adobe application discovery failed, tools keep the last installations', error) })),
        Stream.filterMap(identity),
        Stream.map(Array.map((row) => [row.name, row.url.href] as const)),
        Stream.changes,
    );
    const [first, rest] = yield* Stream.peel(discovered, Sink.head());
    const generation = yield* ScopedRef.fromAcquire(Effect.forEach(first, () => round, { discard: true }));
    yield* Effect.forkScoped(Stream.runForEach(Stream.merge(rest, Stream.drop(links.changes, 1)), () => ScopedRef.set(generation, round)));
});

serve(
    manifest,
    { retention: Duration.hours(1), instructions: ['Specifiers come from results', 'State-changing calls return early when a dialog requires an answer', 'No tool changes frontmost application'] },
    Layer.effectDiscard(served).pipe(Layer.provide(Automation.layer(Config.String('ATELIER_PATH'))), Layer.provide(layerDigest)),
).pipe(Effect.provide(layer), runMain({ teardown: serve.teardown }));
