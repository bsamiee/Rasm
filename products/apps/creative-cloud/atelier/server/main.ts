import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationTools } from '@rasm/application-tools/toolkit';
import { Catalog } from '@rasm/applications/catalog';
import { Description } from '@rasm/applications/description';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Program } from '@rasm/current-process';
import { TypesettingRpcs } from '@rasm/indesign/typesetting';
import { serve } from '@rasm/mcp/cli';
import { Link } from '@rasm/mcp/link';
import { Upstream } from '@rasm/mcp/upstream';
import { Specifier } from '@rasm/object-model/model';
import { Dialogs, EvaluationRpcs, Failure, RenderRpcs } from '@rasm/object-model/rpcs';
import { teardown } from '@rasm/telemetry/node';
import { UxpManifest } from '@rasm/uxp/manifest';
import { Effect, type FileSystem, flow, Layer, type Path, Predicate, Schema } from 'effect';
import { catalog, DESCRIPTIONS, RegistryApplications, readLogs, RegistryPath, route, toolsOf } from '#/applications';
import { Declarations, declarations } from '#/declarations';
import { native } from '#/native';
import manifest from '#/package.json' with { type: 'json' };
import { scripts } from '#/scripts';
import { typography } from '#/typography';

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Effect.fnUntraced(function* (identifier: string) {
    const links = yield* Link.listen(EvaluationRpcs.merge(RenderRpcs, TypesettingRpcs, Description.failing(Failure), Link.Rpcs.omit('tools/list', 'events')).middleware(Dialogs), UxpManifest.origin);
    yield* Upstream.serve([Upstream.fromLinks(links), native], { route: yield* route });
    yield* declarations;
    const { describe, run } = yield* scripts(links, identifier);
    return Layer.merge(
        ApplicationTools.layer({
            descriptions: { ...DESCRIPTIONS, execute_script: describe },
            script: { run },
            render: Effect.fnUntraced(function* (row, id) {
                const [target, client] = yield* Effect.all([Schema.decodeEffect(Specifier.FromString)(id), links.call(Predicate.and(Link.runsIn(row), Link.serves(RenderRpcs)), `linked plugin of ${row.name} rendering objects`)]);
                return { image: (yield* client('render', { target })).image, title: target.leaf };
            }),
            logs: flow(readLogs, Effect.provideContext(yield* Effect.context<FileSystem.FileSystem | Path.Path>())),
            source: (row) => Effect.map(links.call(Predicate.and(Link.runsIn(row), Link.serves(Description.Rpcs)), `linked plugin of ${row.name} describing its interface`), (client) => ({ describe: (payload) => client('describe', payload), operate: (operation) => client('operate', operation) })),
            toolbar: yield* toolsOf,
        }),
        yield* typography(links),
    );
}, Layer.unwrap);
Effect.gen(function* () {
    const { identifier, version, address, descriptors, share, preferences } = yield* Program;
    const { layer: application, ended } = yield* ApplicationServer.fromBundle;
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, identifier },
        {
            instructions: [
                'Atelier drives the Adobe applications installed on this Mac (InDesign, Photoshop, Illustrator, After Effects, Premiere Pro, Media Encoder, Acrobat): their documents, windows, dialogs, scripts, and typography. Search for its tools for any task in one of those applications, and work in the Beta of each, which a product code or name opens, opening a year release by its listed name only for a file that needs it.',
                "Change document content through scripts in the application's own object model, and drive the interface through its Accessibility tree only where scripts cannot reach, such as dialogs and panels.",
                Declarations.instruction,
                "Opening an application launches it in the background or targets its running instance; closing it reports each unsaved document's save dialog as an event, and an application comes forward by itself to show one, so save or close its documents through a script first.",
                "An Adobe plugin linked from a running application adds tools of its own, reports host events among the events, and renders its documents for view of object while that application runs, and a running application's own MCP server adds its tools under the prefix illustrator_, aftereffects_, premiere_, or mediaencoder_.",
            ],
            settings: [...ApplicationTools.settings, RegistryPath],
        },
        { address, descriptors, share, provider: preferences },
        tools(identifier).pipe(Layer.provide(Layer.provideMerge(Declarations.layer, Layer.provideMerge(Catalog.layer(catalog), RegistryApplications.layer))), Layer.provide([layerUndici, layerDigest]), Layer.provideMerge(application)),
        ended,
    );
}).pipe(Effect.provide(Layer.provideMerge(Program.layer, layer)), runMain({ teardown }));
