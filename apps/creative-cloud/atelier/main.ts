import { Automation } from '@rasm/automation/automation.ts';
import { main } from '@rasm/mcp/main.ts';
import { Array, Config, Effect, flow, identity, Layer, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { applications, launch, screenshot } from './features/applications.ts';
import { faces, font } from './features/fonts.ts';
import { indesign } from './features/indesign.ts';
import { command, dictionaries, executeScript } from './features/scripting.ts';
import { InDesignScript } from './indesign-script.ts';
import manifest from './package.json' with { type: 'json' };
import { routes } from './routes.ts';
import { status } from './status.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.gen(function* () {
    const automation = yield* Automation;
    yield* Effect.all([applications, faces, dictionaries], { discard: true });
    const statuses = yield* status;
    const tools = Effect.fnUntraced(function* (names: readonly string[]) {
        const routed = yield* routes(names, statuses);
        yield* Effect.all([indesign(routed), launch(routed, statuses), screenshot(routed), font(routed), executeScript(routed), command(routed, statuses)], { discard: true });
    });
    const [first, rest] = yield* Stream.peel(SubscriptionRef.changes(automation.installations).pipe(Stream.filterMap(identity), Stream.map(flow(Array.map(Struct.get('name')), Array.dedupe)), Stream.changes), Sink.take(1));
    yield* Effect.forEach(first, tools, { discard: true });
    yield* Effect.forkScoped(Stream.runForEach(rest, tools));
});

main(manifest, Layer.unwrap(Effect.map(Config.String('ATELIER_PATH'), (executable) => Layer.effectDiscard(served).pipe(Layer.provide(Layer.effect(InDesignScript, InDesignScript.make(executable))), Layer.provide(Automation.spawned(executable))))));
