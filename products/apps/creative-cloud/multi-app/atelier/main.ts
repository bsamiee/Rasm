import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationServer } from '@rasm/applications/node';
import { Typesetting } from '@rasm/indesign';
import { Link, Upstream } from '@rasm/mcp';
import { serve } from '@rasm/mcp/node';
import { Rendering, Script } from '@rasm/object-model';
import { teardown } from '@rasm/telemetry/node';
import { UxpManifest } from '@rasm/uxp';
import { Cause, Deferred, Effect, Layer, Option } from 'effect';
import { Host } from 'effect/net';
import { applications, Inventory } from '#/applications';
import { Declarations, declarations } from '#/declarations';
import { diagnostics } from '#/diagnostics';
import { native } from '#/native';
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
        const links = yield* Link.listen(Script.merge(Rendering).merge(Typesetting), UxpManifest.origin);
        yield* Upstream.serve([Upstream.linked(links), native], { route });
        yield* Effect.all([applications, windows(links), scripts(links), declarations, diagnostics, typography(links)], { discard: true });
    });
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, domain: identifier },
        {
            listen: Option.some(Host.hostPortFromStringUnsafe(UxpManifest.link.host)),
            instructions: [
                'Atelier drives the Adobe applications installed on this Mac (InDesign, Photoshop, Illustrator, After Effects, Premiere Pro, Media Encoder, Acrobat): their documents, windows, dialogs, scripts, and typography. Search for its tools for any task in one of those applications. Start with applications, which lists what is installed and running, and whose open makes one application the target. Every result returns a handle: pass it on each later call, since it holds your target and your place in the events.',
                'Each result lists the events since your last call: a dialog asking for an answer, a window opening or closing, an application exiting, crashing, or turning unresponsive, memory or thermal pressure, and host events a linked extension reports. Answer a dialog before other work in its application.',
                'A call returns at once when a dialog needs an answer, one open before it included, or its application exits or stops responding; the call keeps running under the call number in early, and finished of a later result holds its result once it finishes, act answering the dialog waiting for it.',
                'Take element, window, and document ids from results and events, never compose them.',
                "Change document content through scripts in the application's own object model, and drive the interface through its Accessibility tree only where scripts cannot reach, such as dialogs and panels.",
                Declarations.instruction,
                "Opening an application launches it in the background or targets its running instance, never a second one; closing it reports each unsaved document's save dialog as an event.",
                "An Adobe extension linked from a running application adds tools of its own and renders its documents for view of object while that application runs, and a running application's own MCP server adds its tools under the prefix illustrator_, aftereffects_, premiere_, or mediaencoder_.",
            ],
            settings: [Preview],
        },
        Layer.effectDiscard(tools).pipe(Layer.provide(Layer.provideMerge(Declarations.layer, Inventory.layer)), Layer.provide([layerUndici, layerDigest]), Layer.provideMerge(helper)),
        Deferred.await(started).pipe(
            Effect.flatMap(({ exited }) => exited),
            Effect.flatMap((code) => Effect.fail(new Cause.UnknownError(code, `Atelier helper exited ${code}`))),
        ),
    );
}).pipe(Effect.provide(layer), runMain({ teardown }));
