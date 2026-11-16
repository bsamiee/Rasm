import { join } from 'node:path';
import process from 'node:process';
import { Array, Cause, Config, Effect, Match, Record, type Redacted } from 'effect';
import { App, type Binding } from '#/app/app';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MOUNT = '/data';

// --- [COMPOSITION] ---------------------------------------------------------------------

const container = Effect.fnUntraced(function* (app: App, mode: keyof typeof App.modes, port: number, bundle: { readonly server: string; readonly client: string }) {
    const variables = yield* Effect.all(
        Record.map(app.env, (binding, variable) =>
            Match.value<Binding>(binding).pipe(
                Match.tag('sqlite', () => Effect.succeedSome(join(MOUNT, variable, 'database.sqlite'))),
                Match.when({ _tag: 'bucket', cors: Array.isReadonlyArrayEmpty }, () => Effect.succeedSome(App.resource(app, mode, variable))),
                Match.tag('postgres', 'secret', () => Effect.asSome(Config.Redacted(variable))),
                Match.tag('text', 'json', () => Effect.succeedNone),
                Match.orElse(({ _tag }) => Effect.fail(new Cause.IllegalArgumentError(`${app.name} binds ${variable} as a bucket with cors, a container serves sqlite, postgres, bucket without cors, secret, text, and json`))),
            ),
        ),
    );
    return {
        image: {
            dockerfile: `# syntax=docker/dockerfile:1\nFROM node:${process.versions.node}-slim\nUSER node\nWORKDIR /app\nCOPY --from=client / ./${App.bundle.assets}\nCOPY . .\nENTRYPOINT ["node"]\nCMD ["${App.bundle.entry}.js"]\n`,
            context: { location: bundle.server, named: { client: { location: bundle.client } } },
        },
        healthcheck: ['CMD', 'node', '--input-type=module', '--eval', `const response = await fetch('http://127.0.0.1:${port}${App.bundle.readiness}'); if (response.status !== 204) throw new Error('HTTP ' + response.status);`],
        env: { ...Record.getSomes<string, string | Redacted.Redacted>(variables), [App.bundle.portVariable]: `${port}` },
        volumes: Record.collect(Record.filter(app.env, App.Binding.$is('sqlite')), (variable) => ({ name: App.resource(app, mode, variable), target: join(MOUNT, variable) })),
        buckets: Record.mapEntries(Record.filter(app.env, App.Binding.$is('bucket')), ({ uploads, abort, expire }, variable) => [App.resource(app, mode, variable), { kept: App.modes[mode].kept, expire: { [uploads]: abort, ...expire[mode] } }]),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { container };
