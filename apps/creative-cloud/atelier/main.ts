import process from 'node:process';
import { NodeCrypto } from '@effect/platform-node';
import { Automation } from '@rasm/automation/automation';
import { main } from '@rasm/mcp/node';
import { Unicode } from '@rasm/typography/unicode';
import { Array, Config, Effect, identity, Layer, Sink, Stream, SubscriptionRef } from 'effect';
import { command, dictionaries } from '#/features/apple-events';
import { applications, launch, perform, screenshot } from '#/features/applications';
import { faces, font } from '#/features/fonts';
import { linked, plugins } from '#/features/plugins';
import { typesetting } from '#/features/typesetting';
import manifest from '#/package.json' with { type: 'json' };
import { routes } from '#/routes';
import { status } from '#/status';

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.gen(function* () {
    const [automation, { libraries: links, changes }] = yield* Effect.all([Automation, linked]);
    yield* Effect.forkScoped(
        Effect.gen(function* () {
            yield* Effect.logError('Atelier host ended', yield* Effect.exit(automation.exited));
            return yield* Effect.sync(() => process.exit(1));
        }),
    );
    const statuses = yield* status;
    yield* Effect.all([applications, faces, Effect.flatMap(routes(statuses, links), dictionaries)], { discard: true });
    const tools = Effect.gen(function* () {
        const routed = yield* routes(statuses, links);
        yield* Effect.all([launch(routed, statuses), screenshot(routed), perform(routed, statuses), command(routed), font(routed), typesetting(routed), plugins(routed, links)], { discard: true });
    });
    const [first, rest] = yield* Stream.peel(SubscriptionRef.changes(automation.installations).pipe(Stream.filterMap(identity), Stream.map(Array.map((row) => [row.name, row.url.href] as const)), Stream.changes), Sink.take(1));
    yield* Effect.forEach(first, () => tools, { discard: true });
    yield* Effect.forkScoped(Stream.runForEach(Stream.merge(rest, Stream.drop(changes, 1)), () => tools));
});

main(manifest, Layer.unwrap(Effect.map(Config.String('ATELIER_PATH'), (executable) => Layer.effectDiscard(served).pipe(Layer.provide(Automation.layer(executable)), Layer.provide(Unicode.layer), Layer.provide(NodeCrypto.layer)))));
