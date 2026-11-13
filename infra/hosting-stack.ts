import { join, relative, resolve } from 'node:path';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { all } from '@pulumi/pulumi';
import { Access } from '@rasm/hosting';
import { App, type Binding } from '@rasm/hosting/app';
import { Sites } from '@rasm/hosting/openai-sites';
import { deployApp } from '@rasm/hosting/pulumi';
import { Array, ByteSize, Cause, ConfigProvider, Effect, Match, Option, Record, Schema, Struct } from 'effect';
import { bucketed, hosts, local, Machine, Role } from '#/platform-stack';

// --- [OPERATIONS] ----------------------------------------------------------------------

const bundled = Effect.fnUntraced(function* (context: ExecutorContext, directory: string) {
    const project = yield* Effect.fromNullishOr(context.projectName);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const [outputs, { resolveConfig }] = yield* Effect.all([
        Effect.try({ try: () => getOutputsForTargetAndConfiguration({ project, target: 'build--vite' }, {}, node), catch: (cause) => new Cause.UnknownError(cause, `${project} states no build--vite outputs`) }),
        Effect.tryPromise({ try: () => import('vite'), catch: (cause) => new Cause.UnknownError(cause, 'vite does not load') }),
    ]);
    const outDir = yield* Effect.fromOption(Array.head(outputs), () => new Cause.NoSuchElementError(`${project} build--vite declares no output`));
    const { root, environments } = yield* Effect.tryPromise({
        try: () => resolveConfig({ root: directory, logLevel: 'silent', build: { outDir: join(context.root, outDir) } }, 'build', 'production', 'production'),
        catch: (cause) => new Cause.UnknownError(cause, `${directory} does not resolve as a Vite config`),
    });
    const [server, client] = yield* Effect.all([
        Effect.fromOption(Record.get(environments, App.bundle.environment), () => new Cause.NoSuchElementError(`${directory} builds no ${App.bundle.environment} environment`)),
        Effect.fromOption(
            Array.findFirst(Object.values(environments), ({ consumer }) => consumer === 'client'),
            () => new Cause.NoSuchElementError(`${directory} builds no client environment`),
        ),
    ]);
    return Record.map({ server, client }, ({ build }) => resolve(root, build.outDir));
});

const placed = Effect.fnUntraced(function* (context: ExecutorContext, directory: string, app: App, mode: keyof typeof App.modes, host: string) {
    const port = 8080;
    const [mount, name, { kept }] = ['/data', App.resource(app, mode), App.modes[mode]];
    const variables = yield* Effect.all(
        Record.map(app.env, (binding, variable) =>
            Match.value<Binding>(binding).pipe(
                Match.tag('sqlite', () => Effect.succeedSome(join(mount, variable, 'database.sqlite'))),
                Match.when({ _tag: 'bucket', cors: Array.isReadonlyArrayEmpty }, () => Effect.succeedSome(App.resource(app, mode, variable))),
                Match.tag('text', 'json', () => Effect.succeedNone),
                Match.orElse(({ _tag }) => Effect.fail(new Cause.IllegalArgumentError(`${app.name} binds ${variable} as ${_tag}, ${host} serves sqlite, bucket without cors, text, and json`))),
            ),
        ),
    );
    const [bundle, { machine, storage }] = [yield* bundled(context, directory), Machine.joined(host)];
    const access = yield* Effect.orDie(Schema.encodeEffect(Access.json)(machine.access));
    const buckets = Record.filter(app.env, App.Binding.$is('bucket'));
    const scoped = Record.isEmptyRecord(buckets)
        ? {}
        : bucketed(
              name,
              storage,
              Record.mapEntries(buckets, ({ abort, expire }, variable) => [App.resource(app, mode, variable), { kept, expire: { [App.uploads]: abort, ...expire[mode] } }]),
          );
    const image = machine.imaged(
        name,
        `# syntax=docker/dockerfile:1
FROM jdxcode/mise:2026.11.7-debian AS node
ARG NODE_VERSION
ENV MISE_DATA_DIR=/mise MISE_NODE_VERSION=\${NODE_VERSION}
RUN ["mise", "install", "node"]
FROM gcr.io/distroless/cc-debian13:nonroot
ARG NODE_VERSION
COPY --from=node /usr/lib/*-linux-gnu/libatomic.so.1 /usr/lib/
COPY --from=node /mise/installs/node/\${NODE_VERSION}/bin/node /usr/local/bin/node
WORKDIR /app
COPY --from=client / ./${App.bundle.assets}
COPY . .
ENTRYPOINT ["node"]
CMD ["${App.bundle.entry}.js"]
`,
        { context: { location: bundle.server, named: { client: { location: bundle.client } } }, buildArgs: { NODE_VERSION: process.versions.node } },
    );
    const { address, named, labels } = machine.routed(name, port, { [`traefik.http.routers.${name}.middlewares`]: Machine.guard });
    const serving = machine.contained(
        name,
        ['CMD', 'node', '--input-type=module', '--eval', `const response = await fetch('http://127.0.0.1:${port}${App.bundle.readiness}'); if (response.status !== 204) throw new Error('HTTP ' + response.status);`],
        {
            image,
            envs: all({ ...Record.getSomes(variables), ...scoped, [App.bundle.portVariable]: `${port}`, [Access.variable]: access }).apply(Record.collect((variable, value) => `${variable}=${value}`)),
            cpus: `${app.limits.cpus}`,
            memory: ByteSize.toUnit(app.limits.memory, 'MiB'),
            mounts: Record.keys(Record.filter(app.env, App.Binding.$is('sqlite'))).map((variable) => ({ type: 'volume', source: machine.kept(App.resource(app, mode, variable)), target: join(mount, variable) })),
            labels,
        },
        { dependsOn: named, protect: kept },
    );
    return { url: serving.id.apply(() => `https://${address}`) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingStacks = Effect.fnUntraced(function* (context: ExecutorContext, deploy: Option.Option<{ readonly production: boolean; readonly rows: { readonly CLOUDFLARE_ACCOUNT_ID: string; readonly CLOUDFLARE_API_TOKEN: string } }>) {
    const project = yield* Effect.fromNullishOr(context.projectName);
    const directory = join(context.root, (yield* Effect.fromNullishOr(context.projectGraph.nodes[project])).data.root);
    const app = yield* Effect.tryPromise({ try: () => import(pathToFileURL(join(directory, 'hosting.config.ts')).href), catch: (cause) => new Cause.UnknownError(cause, `Cannot load ${relative(context.root, directory)}/hosting.config.ts`) }).pipe(
        Effect.map(Struct.get('default')),
        Effect.filterOrFail(
            (declared): declared is App => declared instanceof App,
            (declared) => new Cause.UnknownError(declared, `${relative(context.root, directory)}/hosting.config.ts exports no App by default`),
        ),
    );
    const on = (mode: keyof typeof App.modes, host: string) => ({ stackName: `${App.resource(app, mode)}.${host}`, program: placed(context, directory, app, mode, host) });
    return yield* Option.match(deploy, {
        onNone: () => Effect.succeed([on('disposable', local.host)]),
        onSome: ({ production, rows }) => {
            const mode = production ? 'production' : 'disposable';
            const apps = Record.filter<string, (typeof hosts)[keyof typeof hosts]>(hosts, ({ role }) => Role.$is('Apps')(role));
            return Effect.forEach(
                app.targets,
                App.Target.$match({
                    Cloudflare: () => Effect.succeed({ stackName: `${App.resource(app, mode)}.cloudflare`, program: deployApp({ app, mode, identity: Sites.identity, root: directory }).pipe(Effect.provide(ConfigProvider.layer(ConfigProvider.fromUnknown(rows)))) }),
                    Machine: ({ host }) =>
                        Effect.as(
                            Effect.fromOption(Record.get(apps, host), () => new Cause.NoSuchElementError(`${app.name} deploys to ${host}, machines with role Apps are ${Record.keys(apps).join(', ')}`)),
                            on(mode, host),
                        ),
                }),
            );
        },
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingStacks };
