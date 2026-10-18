import { extension } from '@rasm/indesign/extension';
import { main } from '@rasm/mcp/server';
import { Effect, Layer } from 'effect';
import { applications, installed } from './applications.ts';
import { execute } from './execute.ts';
import manifest from './package.json' with { type: 'json' };
import { Scripting } from './scripting.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const { application, route } = yield* installed;
            const galley = { name: 'Galley', identifier: 'com.adobe.InDesign' };
            const linked = yield* extension(galley.name, route(galley.identifier));
            yield* execute(application, yield* applications(application, [{ ...galley, peers: linked.peers }]));
            yield* linked.publish;
        }),
    ).pipe(Layer.provide(Scripting.layer)),
);
