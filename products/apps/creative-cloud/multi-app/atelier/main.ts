import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationServer } from '@rasm/applications/node';
import { Typesetting } from '@rasm/indesign';
import { Link } from '@rasm/mcp';
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
        yield* Link.host(links, { route });
        yield* Effect.all([applications, windows, scripts(links), diagnostics, typography(links)], { discard: true });
    });
    return yield* serve(
        { name: manifest.name, description: manifest.description, version, domain: identifier },
        { listen: Option.some(Host.hostPortFromStringUnsafe('localhost:7490')), instructions: ['applications open targets an application and returns the handle each later call passes', 'Ids come from results', 'Linked extensions add tools while their application runs'], settings: [Preview] },
        Layer.effectDiscard(tools).pipe(Layer.provide(Inventory.layer), Layer.provideMerge(helper)),
        Deferred.await(started).pipe(
            Effect.flatMap(({ exited }) => exited),
            Effect.flatMap((code) => Effect.fail(new Cause.UnknownError(code, `Atelier helper exited ${code}`))),
        ),
    );
}).pipe(Effect.provide(layer), runMain({ teardown }));
