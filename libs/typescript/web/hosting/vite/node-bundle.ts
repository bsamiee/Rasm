import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Effect, FileSystem, Formatter, Function, Option, Record } from 'effect';
import type { Plugin } from 'vite';
import { App } from '#/app/app';
import { HostingError } from '#/app/error';

// --- [CONSTANTS] -----------------------------------------------------------------------

const START = `\0@rasm/hosting/bundle/${App.bundle.entry}`;
const STARTED = { id: new RegExp(`^${RegExp.escape(START)}$`, 'u') };

// --- [OPERATIONS] ----------------------------------------------------------------------

const sites = Effect.fnUntraced(function* (app: App, root: string, project: string) {
    const [fs, folder] = [yield* FileSystem.FileSystem, join(root, '.openai')];
    const [d1, r2] = [Record.keys(Record.filter(app.env, App.Binding.$is('sqlite'))), Record.keys(Record.filter(app.env, App.Binding.$is('bucket')))];
    yield* d1.length > 1 || r2.length > 1 ? Effect.fail(new Cause.IllegalArgumentError(`${app.name} binds sqlite stores ${d1.join(', ')} and bucket stores ${r2.join(', ')}, OpenAI Sites binds at most one of each`)) : Effect.void;
    yield* fs.makeDirectory(folder, { recursive: true });
    yield* fs.writeFileString(join(folder, 'hosting.json'), `${Formatter.formatJson({ ...Record.getSomes({ d1: Array.head(d1), r2: Array.head(r2) }), project_id: project }, { space: 4 })}\n`);
}, Effect.provide(layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const nodeBundle = (app: App, entry: string): Plugin => ({
    name: '@rasm/hosting/bundle',
    config: ({ build, environments }) => ({
        builder: {},
        environments: {
            [App.bundle.environment]: {
                consumer: 'server',
                resolve: { noExternal: true },
                build: {
                    outDir: environments?.[App.bundle.environment]?.build?.outDir ?? join(build?.outDir ?? 'dist', App.bundle.environment),
                    assetsDir: 'chunks',
                    emptyOutDir: true,
                    copyPublicDir: false,
                    sourcemap: true,
                    target: 'esnext',
                    rolldownOptions: { input: { [App.bundle.entry]: START }, platform: 'node' },
                },
            },
        },
    }),
    configResolved: ({ command, root }) => Effect.runPromise(Effect.forEach(command === 'build' ? app.targets.filter(App.Target.$is('Sites')) : [], ({ project }) => Effect.mapError(sites(app, root, project), HostingError.of('Build', `OpenAI Sites hosting of ${app.name}`)), { discard: true })),
    resolveId: { filter: STARTED, handler: Function.identity },
    load: {
        filter: STARTED,
        handler: () => `import { setSourceMapsSupport } from 'node:module';\nsetSourceMapsSupport(true);\nawait import(${JSON.stringify(entry)});`,
    },
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Option.fromNullishOr(builder.environments[App.bundle.environment]).pipe(
                Effect.fromOption,
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: HostingError.of('Build', `${App.bundle.environment} environment of ${entry}`) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeBundle };
