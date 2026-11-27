import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationServer } from '@rasm/applications/client';
import { InterfaceMaps } from '@rasm/applications/interface-map';
import { Tools } from '@rasm/applications/tools';
import { Workspace } from '@rasm/applications/workspace';
import { serve } from '@rasm/mcp/cli';
import { teardown } from '@rasm/telemetry/node';
import { Effect, Layer } from 'effect';
import manifest from '#/package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { version, identifier, layer: application, ended } = yield* ApplicationServer.fromBundle;
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, identifier },
        {
            instructions: ['Rasad drives the applications installed on this Mac that no dedicated MCP server claims: their windows, dialogs, menus, documents, and scripting dictionaries. Use its tools for any work in one of them in place of computer use, osascript, or System Events.', ...Tools.instructions],
            settings: Tools.settings,
        },
        (stop) => Layer.effectDiscard(Tools.register()).pipe(Layer.provide(InterfaceMaps.layer), Layer.provideMerge(Workspace.layer(Workspace.fromInstallations)), Layer.provideMerge(application(stop))),
        ended,
    );
}).pipe(Effect.provide(layer), runMain({ teardown }));
