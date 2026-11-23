import { createServer } from 'node:http';
import { isDeepStrictEqual } from 'node:util';
import { layer as listening } from '@effect/platform-node/NodeHttpServer';
import { assert, layer } from '@effect/vitest';
import { cases, content } from '@rasm/test-portfolio/rendering';
import { OWNER, platform, published, served } from '@rasm/test-portfolio/served';
import { Context, Effect, Function, Layer, Schedule } from 'effect';
import { HttpClient, HttpClientRequest, HttpServer, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Builder, type WebDriver } from 'selenium-webdriver';
import BrowsingContext from 'selenium-webdriver/bidi/browsingContext.js';
import { Options } from 'selenium-webdriver/firefox.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEIGHT = 900;

// --- [SERVICES] ------------------------------------------------------------------------

class Site extends Context.Service<Site, Readonly<Record<(typeof cases)[number]['viewer'], string>>>()('@rasm/test-portfolio-webdriver/rendering.test/Site') {
    static readonly layer = Layer.effect(
        Site,
        Effect.gen(function* () {
            const [client, server, { origin }] = yield* Effect.all([HttpClient.HttpClient, HttpServer.HttpServer, served]);
            yield* published(origin, content);
            yield* server.serve(
                Effect.gen(function* () {
                    const request = yield* HttpServerRequest.HttpServerRequest;
                    return HttpServerResponse.fromClientResponse(yield* client.execute(HttpServerRequest.toClientRequest(request).pipe(HttpClientRequest.setUrl(new URL(request.url, origin)), HttpClientRequest.setHeaders(OWNER))));
                }),
            );
            return { visitor: origin, owner: HttpServer.formatAddress(server.address) };
        }),
    ).pipe(Layer.provide(listening(createServer, { host: '127.0.0.1', port: 0 })), Layer.provide(platform));
}

class Session extends Context.Service<Session, { readonly driver: WebDriver; readonly viewed: Awaited<ReturnType<typeof BrowsingContext>> }>()('@rasm/test-portfolio-webdriver/rendering.test/Session') {
    static readonly layer = Layer.effect(
        Session,
        Effect.gen(function* () {
            const driver = yield* Effect.acquireRelease(
                Effect.promise(() => new Builder().withCapabilities(new Options().addArguments('-headless').enableBidi()).build()),
                (held) => Effect.promise(() => held.quit()),
            );
            return { driver, viewed: yield* Effect.promise(() => driver.getWindowHandle().then((browsingContextId) => BrowsingContext(driver, { browsingContextId }))) };
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const drawn = Effect.fnUntraced(function* ({ viewer, path, width, read, expected }: (typeof cases)[number]) {
    const [site, { driver, viewed }] = yield* Effect.all([Site, Session]);
    yield* Effect.promise(() => driver.get('about:blank'));
    yield* Effect.promise(() => viewed.setViewport(width, HEIGHT));
    yield* Effect.promise(() => driver.get(new URL(path, site[viewer]).href));
    const seen = yield* Effect.filterOrFail(
        Effect.promise(() => driver.executeScript(read)),
        (value) => isDeepStrictEqual(value, expected),
        (value) => ({ value }),
    ).pipe(Effect.retry({ schedule: Schedule.upTo(Schedule.spaced('50 millis'), { duration: '10 seconds' }) }), Effect.match({ onFailure: ({ value }) => value, onSuccess: Function.identity }));
    assert.deepStrictEqual(seen, expected);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Layer.mergeAll(Site.layer, Session.layer), { timeout: '2 minutes', excludeTestServices: true })('the portfolio in Firefox', (it) => {
    it.effect.each(cases)('$viewer at $width: $shows', drawn, 30_000);
});
