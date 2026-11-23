import { createServer } from 'node:http';
import { platform as system } from 'node:os';
import { isDeepStrictEqual } from 'node:util';
import { layer as listening } from '@effect/platform-node/NodeHttpServer';
import { assert, layer } from '@effect/vitest';
import { run } from '@rasm/process';
import { cases, content } from '@rasm/test-portfolio/rendering';
import { OWNER, platform, published, served } from '@rasm/test-portfolio/served';
import { Context, Effect, Function, Layer, Option, Record, Schedule } from 'effect';
import { HttpClient, HttpClientRequest, HttpServer, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Browser, Builder, type WebDriver } from 'selenium-webdriver';
import BrowsingContext from 'selenium-webdriver/bidi/browsingContext.js';
import { Options } from 'selenium-webdriver/firefox.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SAFARI = 'com.apple.Safari';
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

class Session extends Context.Service<Session, { readonly driver: WebDriver; readonly sized: (width: number) => Effect.Effect<void> }>()('@rasm/test-portfolio-webdriver/rendering.test/Session') {
    static readonly engines = Record.getSomes({
        Safari: Option.liftPredicate(
            Effect.gen(function* () {
                const [running, front] = yield* Effect.all([Effect.isSuccess(run(['pgrep', '-x', 'Safari'])), run(['osascript', '-l', 'JavaScript', '-e', "ObjC.import('AppKit'); $.NSWorkspace.sharedWorkspace.frontmostApplication.bundleIdentifier.js"])]);
                const driver = yield* Effect.acquireRelease(
                    Effect.promise(() => new Builder().forBrowser(Browser.SAFARI).build()),
                    (held) =>
                        Effect.andThen(
                            Effect.promise(() => held.quit()),
                            running ? Effect.void : Effect.ignore(run(['osascript', '-l', 'JavaScript', '-e', `ObjC.import('AppKit'); $.NSRunningApplication.runningApplicationsWithBundleIdentifier('${SAFARI}').js.map((app) => app.terminate)`])),
                        ),
                );
                yield* run(['open', '-b', front.stdout.trim()]);
                const [outer, [across, down, floor]] = yield* Effect.promise(() => Promise.all([driver.manage().window().getRect(), driver.executeScript<readonly [number, number, number]>(() => [innerWidth, innerHeight, screen.height])]));
                return {
                    driver,
                    sized: (width: number) =>
                        Effect.asVoid(
                            Effect.promise(() =>
                                driver
                                    .manage()
                                    .window()
                                    .setRect({ x: 0, y: floor, width: width + outer.width - across, height: HEIGHT + outer.height - down }),
                            ),
                        ),
                };
            }),
            () => system() === 'darwin',
        ),
        Firefox: Option.some(
            Effect.gen(function* () {
                const driver = yield* Effect.acquireRelease(
                    Effect.promise(() => new Builder().withCapabilities(new Options().addArguments('-headless').enableBidi()).build()),
                    (held) => Effect.promise(() => held.quit()),
                );
                const viewed = yield* Effect.promise(() => driver.getWindowHandle().then((browsingContextId) => BrowsingContext(driver, { browsingContextId })));
                return { driver, sized: (width: number) => Effect.promise(() => viewed.setViewport(width, HEIGHT)) };
            }),
        ),
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const drawn = Effect.fnUntraced(function* ({ viewer, path, width, read, expected }: (typeof cases)[number]) {
    const [site, { driver, sized }] = yield* Effect.all([Site, Session]);
    yield* Effect.promise(() => driver.get('about:blank'));
    yield* sized(width);
    yield* Effect.promise(() => driver.get(new URL(path, site[viewer]).href));
    assert.deepStrictEqual(yield* Effect.promise(() => driver.executeScript(() => [innerWidth, innerHeight, document.visibilityState])), [width, HEIGHT, 'visible'], 'a browser draws no frame while opaque windows cover all of its window, or it is minimized or on another desktop');
    const seen = yield* Effect.filterOrFail(
        Effect.promise(() => driver.executeScript(read)),
        (value) => isDeepStrictEqual(value, expected),
        (value) => ({ value }),
    ).pipe(Effect.retry({ schedule: Schedule.upTo(Schedule.spaced('50 millis'), { duration: '10 seconds' }) }), Effect.match({ onFailure: ({ value }) => value, onSuccess: Function.identity }));
    assert.deepStrictEqual(seen, expected);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Site.layer, { timeout: '2 minutes', excludeTestServices: true })('the portfolio', (shared) => {
    Record.collect(Session.engines, (engine, session) =>
        shared.layer(Layer.provide(Layer.effect(Session, session), platform), { timeout: '2 minutes' })(`in ${engine}`, (it) => {
            it.effect.each(cases)('$viewer at $width: $shows', drawn, 30_000);
        }),
    );
});
