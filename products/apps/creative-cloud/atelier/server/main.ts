import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationServer } from '@rasm/applications/client';
import { Tools } from '@rasm/applications/tools';
import { Workspace } from '@rasm/applications/workspace';
import { TypesettingRpcs } from '@rasm/indesign';
import { Link, Upstream } from '@rasm/mcp';
import { serve } from '@rasm/mcp/cli';
import { EvaluationRpcs, RenderRpcs, Specifier } from '@rasm/object-model';
import { teardown } from '@rasm/telemetry/node';
import { UxpManifest } from '@rasm/uxp';
import { Effect, Layer, Option, Predicate, Schema } from 'effect';
import { Host } from 'effect/net';
import { catalog, DESCRIPTIONS, RegistryApplications, readLogs, RegistryPath, route } from '#/applications';
import { Declarations, declarations } from '#/declarations';
import { native } from '#/native';
import manifest from '#/package.json' with { type: 'json' };
import { DESCRIPTION, scripts } from '#/scripts';
import { typography } from '#/typography';

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Effect.gen(function* () {
    const links = yield* Link.listen(EvaluationRpcs.merge(RenderRpcs).merge(TypesettingRpcs), UxpManifest.origin);
    yield* Upstream.serve([Upstream.fromLinks(links), native], { route: yield* route });
    yield* Tools.register({
        descriptions: { ...DESCRIPTIONS, execute_script: DESCRIPTION },
        scripts: yield* scripts(links),
        render: Effect.fnUntraced(function* (row, id) {
            const [target, client] = yield* Effect.all([Schema.decodeEffect(Specifier.FromString)(id), links.call(Predicate.and(Link.runsIn(row), Link.serves(RenderRpcs.requests.keys())), `linked extension of ${row.name} rendering objects`)]);
            return { image: (yield* client('render', { target })).image, title: target.leaf };
        }),
        logs: readLogs,
    });
    yield* Effect.all([declarations, typography(links)], { discard: true });
});
Effect.gen(function* () {
    const { version, identifier, layer: application, ended } = yield* ApplicationServer.fromBundle;
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, identifier },
        {
            listen: Option.some(Host.hostPortFromStringUnsafe(new URL(UxpManifest.link).host)),
            instructions: [
                'Atelier drives the Adobe applications installed on this Mac (InDesign, Photoshop, Illustrator, After Effects, Premiere Pro, Media Encoder, Acrobat): their documents, windows, dialogs, scripts, and typography. Search for its tools for any task in one of those applications.',
                ...Tools.instructions,
                "Change document content through scripts in the application's own object model, and drive the interface through its Accessibility tree only where scripts cannot reach, such as dialogs and panels.",
                Declarations.instruction,
                "Opening an application launches it in the background or targets its running instance; closing it reports each unsaved document's save dialog as an event, and an application comes forward by itself to show one, so save or close its documents through a script first.",
                "An Adobe extension linked from a running application adds tools of its own, reports host events among the events, and renders its documents for view of object while that application runs, and a running application's own MCP server adds its tools under the prefix illustrator_, aftereffects_, premiere_, or mediaencoder_.",
            ],
            settings: [...Tools.settings, RegistryPath],
        },
        Layer.effectDiscard(tools).pipe(Layer.provide(Layer.provideMerge(Declarations.layer, Layer.provideMerge(Workspace.layer(catalog), RegistryApplications.layer))), Layer.provide([layerUndici, layerDigest]), Layer.provideMerge(application)),
        ended,
    );
}).pipe(Effect.provide(layer), runMain({ teardown }));
