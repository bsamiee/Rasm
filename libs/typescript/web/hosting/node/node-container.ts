import { join } from 'node:path';
import process from 'node:process';
import { Config, Effect, Match, Record, type Redacted } from 'effect';
import { App, type Binding } from '#/app/app';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MOUNT = '/data';

// --- [COMPOSITION] ---------------------------------------------------------------------

const container = Effect.fnUntraced(function* (app: App, mode: keyof typeof App.modes, port: number, bundle: string) {
    const variables = yield* Effect.all(
        Record.map(app.env, (binding, variable) =>
            Match.value<Binding>(binding).pipe(
                Match.tag('sqlite', () => Effect.succeedSome(join(MOUNT, variable, 'database.sqlite'))),
                Match.tag('bucket', () => Effect.succeedSome(App.resource(app, mode, variable))),
                Match.tag('postgres', 'secret', () => Effect.asSome(Config.Redacted(variable))),
                Match.tag('text', 'json', () => Effect.succeedNone),
                Match.exhaustive,
            ),
        ),
    );
    const volumes = Record.collect(Record.filter(app.env, App.Binding.$is('sqlite')), (variable) => ({ name: App.resource(app, mode, variable), target: join(MOUNT, variable) }));
    return {
        image: {
            dockerfile: `# syntax=docker/dockerfile:1\nFROM node:${process.versions.node}-slim\nRUN install -d -o node -g node ${[MOUNT, ...volumes.map(({ target }) => target)].join(' ')}\nUSER node\nWORKDIR /app\nCOPY . .\nENTRYPOINT ["node"]\nCMD ["${App.bundle.entry}.js"]\n`,
            context: { location: bundle },
        },
        healthcheck: ['CMD', 'node', '--input-type=module', '--eval', `const response = await fetch('http://127.0.0.1:${port}${App.bundle.readiness}'); if (response.status !== 204) throw new Error('HTTP ' + response.status);`],
        env: { ...Record.getSomes<string, string | Redacted.Redacted>(variables), [App.bundle.portVariable]: `${port}` },
        volumes,
        buckets: Record.mapEntries(Record.filter(app.env, App.Binding.$is('bucket')), ({ abort, expire, cors }, variable) => [App.resource(app, mode, variable), { kept: App.modes[mode].kept, abort, expire: expire[mode], cors }]),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { container };
