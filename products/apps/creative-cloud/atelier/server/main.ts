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
import { Effect, Layer, Predicate, Schema } from 'effect';
import { Applications } from '#/applications';
import { Declarations, declarations } from '#/declarations';
import { Native } from '#/native';
import manifest from '#/package.json' with { type: 'json' };
import { scripts } from '#/scripts';
import { typography } from '#/typography';

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Effect.fnUntraced(function* (identifier: string) {
    const links = yield* Link.listen(EvaluationRpcs.merge(RenderRpcs, TypesettingRpcs, Description.Rpcs.failing(Failure), Link.Rpcs.omit('tools/list', 'events')).middleware(Dialogs), UxpManifest.origin);
    const applications = yield* Applications;
    yield* Upstream.serve([Upstream.fromLinks(links), Native.upstreams], { route: applications.route });
    yield* declarations;
    return Layer.merge(
        ApplicationTools.layer({
            descriptions: {
                applications: 'each Adobe product lists its Beta first, the form a product code or name opens',
                view: 'object renders a document, page, spread, artboard, layer, composition, or sequence from its object specifier through the linked plugin of the target, no window needed',
                execute_script: `code is a TypeScript function body, return giving its result, run through the newest script engine of the target (a linked plugin, do script uxpscript, Acrobat JavaScript, or ExtendScript), a failure naming the body line. Acrobat bodies reach app, this, event, security, spell, util, Report, and every object read from them through stand-ins that answer each dialog as dismissed without showing it, or refuse it naming the member and the parameter that avoids it (a dirty closeDoc, execMenuItem SaveAs or Open, a picker with no stated cancel value); code a body hands Acrobat as a string (Function, indirect eval, setTimeOut, action scripts) runs against the real app and shows its dialogs. UXP bodies take their host module from require('<host>') (${Declarations.checked.join(', ')}), checked first against its declarations, a wrong member, type, read-only write, or enumeration value refused with a link to atelier://object-model/{host}/{name}`,
                diagnostics: 'an Adobe application adds its UXP log lines',
            },
            script: yield* scripts(links, identifier),
            render: Effect.fnUntraced(function* (row, id) {
                const [target, client] = yield* Effect.all([Schema.decodeEffect(Specifier.FromString)(id), links.call(Predicate.and(Link.runsIn(row), Link.serves(RenderRpcs)), `linked plugin of ${row.name} rendering objects`)]);
                return { image: (yield* client('render', { target })).image, title: target.leaf };
            }),
            logs: applications.logs,
            source: (row) => Effect.map(links.call(Predicate.and(Link.runsIn(row), Link.serves(Description.Rpcs)), `linked plugin of ${row.name} describing its interface`), (client) => ({ describe: (payload) => client('describe', payload), operate: (operation) => client('operate', operation) })),
            toolbar: applications.toolbar,
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
                'An Adobe plugin linked from a running application adds tools of its own, reports host events among the events, and renders its documents for view of object while that application runs.',
                Native.instruction,
            ],
            settings: [...ApplicationTools.settings, Applications.Registry],
        },
        { address, descriptors, share, provider: preferences },
        tools(identifier).pipe(Layer.provide(Layer.provideMerge(Declarations.layer, Layer.provideMerge(Catalog.layer(Applications.catalog), Applications.layer))), Layer.provide([layerUndici, layerDigest]), Layer.provideMerge(application)),
        ended,
    );
}).pipe(Effect.provide(Layer.provideMerge(Program.layer, layer)), runMain({ teardown }));
