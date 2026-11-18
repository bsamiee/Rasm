import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { core } from '@rasm/extendscript/node';
import { Array, Effect, Exit, FileSystem, Layer, Path, Ref } from 'effect';
import { HttpClient, HttpClientResponse } from 'effect/http';

// --- [OPERATIONS] ----------------------------------------------------------------------

const unserved = (requested: Ref.Ref<readonly string[]>) => HttpClient.make((request) => Effect.as(Ref.update(requested, Array.append(request.url)), HttpClientResponse.fromWeb(request, new Response(undefined, { status: 404 }))));
const read = Effect.fnUntraced(function* (cached: readonly string[]) {
    const [fs, path, requested] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Ref.make<readonly string[]>([])]);
    const folder = yield* fs.makeTempDirectoryScoped();
    yield* Effect.forEach(cached, (name) => fs.writeFileString(path.join(folder, name), '<dictionary/>'), { discard: true });
    const exit = yield* Effect.exit(Effect.provide(core(folder), Layer.succeed(HttpClient.HttpClient, unserved(requested))));
    return { failed: Exit.isFailure(exit), requested: yield* Ref.get(requested) };
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('core declarations', () => {
    it.live(
        'refuses cached files whose hash differs from the pinned one and fetches the image again',
        Effect.fnUntraced(
            function* () {
                const { failed, requested } = yield* read(['javascript.xml', 'scriptui.xml']);
                assert.deepStrictEqual([failed, Array.map(requested, (url) => new URL(url).pathname.endsWith('.dmg'))], [true, [true]]);
            },
            Effect.provide([layer, layerDigest]),
        ),
    );
});
