import process from 'node:process';
import { isSea } from 'node:sea';
import { Array, Effect, identity, Result, ScopedRef, Sink, Stream, SubscriptionRef } from 'effect';
import { Adobe } from '#/adobe';
import { command, dictionaries } from '#/features/apple-events';
import { applications, launch, perform, quit, read, screenshot } from '#/features/applications';
import { delivered } from '#/features/delivery';
import { faces, font } from '#/features/fonts';
import { plugins } from '#/features/plugins';
import { typesetting } from '#/features/typesetting';
import { listening } from '#/libraries';
import { routes } from '#/routes';
import { status } from '#/status';

// --- [COMPOSITION] ---------------------------------------------------------------------

const registerAdobeTools = Effect.gen(function* () {
    const [adobe, links, dependencies] = yield* Effect.all([Adobe, listening, status]);
    yield* adobe.exited.pipe(
        Effect.exit,
        Effect.flatMap((exit) => Effect.logError('Atelier host ended', exit)),
        Effect.andThen(Effect.sync(() => process.exit(1))),
        Effect.forkScoped,
    );
    yield* isSea() ? Effect.forkScoped(delivered) : Effect.void;
    yield* Effect.all([applications, faces], { discard: true });
    const round = Effect.flatMap(routes(dependencies, links), (routed) => Effect.all([dictionaries(routed), launch(routed, dependencies), read(routed), screenshot(routed), perform(routed, dependencies), quit(routed), command(routed), font(routed), typesetting(routed), plugins(routed, links)], { discard: true }));
    const discovered = SubscriptionRef.changes(adobe.installations).pipe(
        Stream.tap(Result.match({ onSuccess: () => Effect.void, onFailure: (error) => Effect.logError('Adobe application discovery failed, tools keep the last installations', error) })),
        Stream.filterMap(identity),
        Stream.map(Array.map((row) => [row.name, row.url.href] as const)),
        Stream.changes,
    );
    const [first, rest] = yield* Stream.peel(discovered, Sink.head());
    const generation = yield* ScopedRef.fromAcquire(Effect.forEach(first, () => round, { discard: true }));
    yield* Effect.forkScoped(Stream.runForEach(Stream.merge(rest, Stream.drop(links.changes, 1)), () => ScopedRef.set(generation, round)));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerAdobeTools };
