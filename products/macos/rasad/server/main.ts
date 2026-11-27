import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationTools } from '@rasm/application-tools/toolkit';
import { ApplicationServer } from '@rasm/applications/client';
import { Workspace } from '@rasm/applications/workspace';
import { Program } from '@rasm/current-process';
import { serve } from '@rasm/mcp/cli';
import { teardown } from '@rasm/telemetry/node';
import { Effect, Layer } from 'effect';
import manifest from '#/package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { identifier, version, address, descriptors, share, preferences } = yield* Program;
    const { layer: application, ended } = yield* ApplicationServer.fromBundle;
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, identifier },
        {
            instructions: ['Rasad drives the applications installed on this Mac that no dedicated MCP server claims: their windows, dialogs, menus, documents, and scripting dictionaries. Use its tools for any work in one of them in place of computer use, osascript, or System Events.'],
            settings: ApplicationTools.settings,
        },
        { address, descriptors, share, provider: preferences },
        ApplicationTools.layer().pipe(Layer.provideMerge(Workspace.layer(Workspace.fromInstallations)), Layer.provideMerge(application)),
        ended,
    );
}).pipe(Effect.provide(Layer.provideMerge(Program.layer, layer)), runMain({ teardown }));
