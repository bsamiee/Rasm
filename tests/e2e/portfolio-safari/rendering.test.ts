import { isDeepStrictEqual } from 'node:util';
import { assert, layer } from '@effect/vitest';
import { cases, content } from '@rasm/test-portfolio/rendering';
import { platform, published, served } from '@rasm/test-portfolio/served';
import { Context, Effect, Function, Layer, Schedule } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Browser, Builder, type WebDriver } from 'selenium-webdriver';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SAFARI = 'com.apple.Safari';
const HEIGHT = 900;

// --- [SERVICES] ------------------------------------------------------------------------

class Visit extends Context.Service<Visit, { readonly driver: WebDriver; readonly origin: string }>()('@rasm/test-portfolio-safari/rendering.test/Visit') {
    static readonly layer = Layer.effect(
        Visit,
        Effect.gen(function* () {
            const [spawner, { origin }] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, served]);
            yield* published(origin, content);
            const [running, front] = yield* Effect.all([spawner.exitCode(ChildProcess.make('pgrep', ['-x', 'Safari'])), spawner.string(ChildProcess.make('osascript', ['-l', 'JavaScript', '-e', "ObjC.import('AppKit'); $.NSWorkspace.sharedWorkspace.frontmostApplication.bundleIdentifier.js"]))]);
            const driver = yield* Effect.acquireRelease(
                Effect.promise(() => new Builder().forBrowser(Browser.SAFARI).build()),
                (held) =>
                    Effect.andThen(
                        Effect.promise(() => held.quit()),
                        running === 0 ? Effect.void : Effect.ignore(spawner.exitCode(ChildProcess.make('osascript', ['-l', 'JavaScript', '-e', `ObjC.import('AppKit'); $.NSRunningApplication.runningApplicationsWithBundleIdentifier('${SAFARI}').js.map((app) => app.terminate)`]))),
                    ),
            );
            yield* spawner.exitCode(ChildProcess.make('open', ['-b', front.trim()]));
            return { driver, origin };
        }),
    ).pipe(Layer.provide(platform));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Visit.layer, { timeout: '2 minutes', excludeTestServices: true })('the published portfolio in Safari', (it) => {
    it.effect.each(cases)(
        '$shows',
        Effect.fnUntraced(function* ({ width, read, expected }) {
            const { driver, origin } = yield* Visit;
            yield* Effect.promise(() => driver.manage().window().setRect({ width, height: HEIGHT }));
            yield* Effect.promise(() => driver.get(origin));
            const seen = yield* Effect.filterOrFail(
                Effect.promise(() => driver.executeScript(read)),
                (value) => isDeepStrictEqual(value, expected),
                (value) => ({ value }),
            ).pipe(Effect.retry({ schedule: Schedule.upTo(Schedule.spaced('50 millis'), { duration: '10 seconds' }) }), Effect.match({ onFailure: ({ value }) => value, onSuccess: Function.identity }));
            assert.deepStrictEqual(seen, expected);
        }),
        30_000,
    );
});
