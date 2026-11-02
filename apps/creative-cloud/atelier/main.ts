import { NodeCrypto } from '@effect/platform-node';
import { Automation } from '@rasm/automation/automation';
import { Script } from '@rasm/indesign/script';
import { main } from '@rasm/mcp/node';
import { Array, Config, Effect, identity, Layer, Path, Sink, Stream, SubscriptionRef } from 'effect';
import { applications, launch, screenshot } from '#/features/applications';
import { faces, font } from '#/features/fonts';
import { indesign } from '#/features/indesign';
import { command, dictionaries, executeScript } from '#/features/scripting';
import manifest from '#/package.json' with { type: 'json' };
import { routes } from '#/routes';
import { status } from '#/status';

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.gen(function* () {
    const automation = yield* Automation;
    yield* Effect.all([applications, faces, dictionaries], { discard: true });
    const statuses = yield* status;
    const tools = Effect.gen(function* () {
        const routed = yield* routes(statuses);
        yield* Effect.all([indesign(routed), launch(routed, statuses), screenshot(routed), font(routed), executeScript(routed), command(routed, statuses)], { discard: true });
    });
    const [first, rest] = yield* Stream.peel(SubscriptionRef.changes(automation.installations).pipe(Stream.filterMap(identity), Stream.map(Array.map((row) => [row.name, row.url.href] as const)), Stream.changes), Sink.take(1));
    yield* Effect.forEach(first, () => tools, { discard: true });
    yield* Effect.forkScoped(Stream.runForEach(rest, () => tools));
});

main(
    manifest,
    Layer.unwrap(
        Effect.gen(function* () {
            const [executable, path] = yield* Effect.all([Config.String('ATELIER_PATH'), Path.Path]);
            return Layer.effectDiscard(served).pipe(Layer.provide(Layer.effect(Script, Script.make(new URL('../Resources/main.idjs', yield* path.toFileUrl(executable))))), Layer.provide(Automation.layer(executable)), Layer.provide(NodeCrypto.layer));
        }),
    ),
);
