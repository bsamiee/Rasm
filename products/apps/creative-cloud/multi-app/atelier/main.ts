import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationServer } from '@rasm/applications/node';
import { Typesetting } from '@rasm/indesign';
import { Link, Upstream } from '@rasm/mcp';
import { serve } from '@rasm/mcp/node';
import { Script } from '@rasm/object-model';
import { teardown } from '@rasm/telemetry/node';
import { UxpManifest } from '@rasm/uxp';
import { Cause, Deferred, Effect, Layer, Option } from 'effect';
import { Host } from 'effect/net';
import { applications, Inventory } from '#/applications';
import { diagnostics } from '#/diagnostics';
import manifest from '#/package.json' with { type: 'json' };
import { scripts } from '#/scripts';
import { typography } from '#/typography';
import { Preview, windows } from '#/windows';

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [{ version, identifier, layer: helper }, started] = yield* Effect.all([ApplicationServer.bundled, Deferred.make<ApplicationServer['Service']>()]);
    const tools = Effect.gen(function* () {
        const [server, { route }] = yield* Effect.all([ApplicationServer, Inventory]);
        yield* Deferred.succeed(started, server);
        const links = yield* Link.listen(Script.merge(Typesetting), UxpManifest.origin);
        yield* Upstream.serve([Upstream.linked(links)], { route });
        yield* Effect.all([applications, windows, scripts(links), diagnostics, typography(links)], { discard: true });
    });
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, domain: identifier },
        {
            listen: Option.some(Host.hostPortFromStringUnsafe('localhost:7490')),
            instructions: [
                'Atelier drives the Adobe applications installed on this Mac (InDesign, Photoshop, Illustrator, After Effects, Premiere Pro, Media Encoder, Acrobat): their documents, windows, dialogs, scripts, and typography. Search for its tools for any task in one of those applications. Start with applications, which lists what is installed and running, and whose open makes one application the target. Every result returns a handle: pass it on each later call, since it holds your target and your place in the events.',
                'Each result lists the events since your last call: a dialog asking for an answer, a window opening or closing, an application exiting, crashing, or turning unresponsive, and memory or thermal pressure. Answer a dialog before other work in its application.',
                'A call returns at once when a dialog needs an answer or its application exits or stops responding; the call keeps running, and the resource link in that result holds its result once it finishes.',
                'Take element, window, and document ids from results and events, never compose them.',
                "Change document content through scripts in the application's own object model, and drive the interface through its Accessibility tree only where scripts cannot reach, such as dialogs and panels.",
                "Opening an application launches it in the background or targets its running instance, never a second one; closing it reports each unsaved document's save dialog as an event.",
                'An Adobe extension linked from a running application adds tools of its own while that application runs.',
            ],
            settings: [Preview],
        },
        Layer.effectDiscard(tools).pipe(Layer.provide(Inventory.layer), Layer.provideMerge(helper)),
        Deferred.await(started).pipe(
            Effect.flatMap(({ exited }) => exited),
            Effect.flatMap((code) => Effect.fail(new Cause.UnknownError(code, `Atelier helper exited ${code}`))),
        ),
    );
}).pipe(Effect.provide(layer), runMain({ teardown }));
