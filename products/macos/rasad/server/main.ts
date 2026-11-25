import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationServer } from '@rasm/applications/client';
import { Tools } from '@rasm/applications/tools';
import { Inventory } from '@rasm/applications/workspace';
import { serve } from '@rasm/mcp/cli';
import { teardown } from '@rasm/telemetry/node';
import { Effect, Layer, Option } from 'effect';
import { Host } from 'effect/net';
import manifest from '#/package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { version, identifier, layer: helper, ended } = yield* ApplicationServer.bundled;
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, domain: identifier },
        {
            listen: Option.some(Host.hostPortFromStringUnsafe('localhost:7491')),
            instructions: ['Rasad drives the applications installed on this Mac that no dedicated MCP server claims: their windows, dialogs, menus, documents, and scripting dictionaries.', ...Tools.instructions],
            settings: Tools.settings,
        },
        Layer.effectDiscard(Tools.register()).pipe(Layer.provide(Inventory.layer(Inventory.installed)), Layer.provideMerge(helper)),
        ended,
    );
}).pipe(Effect.provide(layer), runMain({ teardown }));
