import { join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { getOutputsForTargetAndConfiguration, type ProjectGraphProjectNode } from '@nx/devkit';
import { all, secret } from '@pulumi/pulumi';
import { App } from '@rasm/hosting';
import { container } from '@rasm/hosting/container';
import { deployApp } from '@rasm/hosting/deploy';
import { Access } from '@rasm/hosting/identity';
import { Array, ByteSize, Cause, ConfigProvider, Effect, Option, Record, Redacted, Schema, Struct } from 'effect';
import { bucketed, hosts, local, Machine, Role } from '#/platform-stack';

// --- [OPERATIONS] ----------------------------------------------------------------------

const bundled = Effect.fnUntraced(function* (workspace: string, node: ProjectGraphProjectNode, directory: string) {
    const project = node.name;
    const [outputs, { resolveConfig }] = yield* Effect.all([
        Effect.try({ try: () => getOutputsForTargetAndConfiguration({ project, target: 'build--vite' }, {}, node), catch: (cause) => new Cause.UnknownError(cause, `${project} states no build--vite outputs`) }),
        Effect.tryPromise({ try: () => import('vite'), catch: (cause) => new Cause.UnknownError(cause, 'vite does not load') }),
    ]);
    const outDir = yield* Effect.fromOption(Array.head(outputs), () => new Cause.NoSuchElementError(`${project} build--vite declares no output`));
    const { root, environments } = yield* Effect.tryPromise({
        try: () => resolveConfig({ root: directory, logLevel: 'silent', build: { outDir: join(workspace, outDir) } }, 'build', 'production', 'production'),
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

const placed = Effect.fnUntraced(function* (workspace: string, node: ProjectGraphProjectNode, directory: string, app: App, mode: keyof typeof App.modes, host: string) {
    const [port, name, { machine, storage }] = [8080, App.resource(app, mode), Machine.joined(host)];
    const [projected, access] = yield* Effect.all([Effect.flatMap(bundled(workspace, node, directory), (bundle) => container(app, mode, port, bundle)), Effect.orDie(Schema.encodeEffect(Access.json)(machine.access))]);
    const image = machine.imaged(name, projected.image.dockerfile, { context: projected.image.context });
    const { address, named, labels } = machine.routed(name, port, { [`traefik.http.routers.${name}.middlewares`]: Machine.guard });
    const environment = Record.map(projected.env, (value) => (Redacted.isRedacted(value) ? secret(Redacted.value(value)) : value));
    const serving = machine.contained(
        name,
        projected.healthcheck,
        {
            image,
            envs: all({ ...environment, ...(Record.isEmptyRecord(projected.buckets) ? {} : bucketed(name, storage, projected.buckets)), [Access.variable]: access }).apply(Record.collect((variable, value) => `${variable}=${value}`)),
            cpus: `${app.limits.cpus}`,
            memory: ByteSize.toUnit(app.limits.memory, 'MiB'),
            mounts: projected.volumes.map(({ name: volume, target }) => ({ type: 'volume', source: machine.kept(volume), target })),
            labels,
        },
        { dependsOn: named, protect: App.modes[mode].kept },
    );
    return { url: serving.id.apply(() => `https://${address}`) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingStacks = Effect.fnUntraced(function* (workspace: string, node: ProjectGraphProjectNode, deploy: Option.Option<{ readonly production: boolean; readonly rows: { readonly CLOUDFLARE_ACCOUNT_ID: string; readonly CLOUDFLARE_API_TOKEN: string } }>) {
    const directory = join(workspace, node.data.root);
    const app = yield* Effect.tryPromise({ try: () => import(pathToFileURL(join(directory, 'hosting.config.ts')).href), catch: (cause) => new Cause.UnknownError(cause, `Cannot load ${relative(workspace, directory)}/hosting.config.ts`) }).pipe(
        Effect.map(Struct.get('default')),
        Effect.filterOrFail(
            (declared): declared is App => declared instanceof App,
            (declared) => new Cause.UnknownError(declared, `${relative(workspace, directory)}/hosting.config.ts exports no App by default`),
        ),
    );
    const on = (mode: keyof typeof App.modes, host: string) => ({ stackName: `${App.resource(app, mode)}.${host}`, program: placed(workspace, node, directory, app, mode, host) });
    return yield* Option.match(deploy, {
        onNone: () => Effect.succeed([on('disposable', local.host)]),
        onSome: ({ production, rows }) => {
            const mode = production ? 'production' : 'disposable';
            const apps = Record.filter<string, (typeof hosts)[keyof typeof hosts]>(hosts, ({ roles }) => roles.some(Role.$is('Apps')));
            return Effect.map(
                Effect.forEach(
                    app.targets,
                    App.Target.$match({
                        Cloudflare: () => Effect.succeedSome({ stackName: `${App.resource(app, mode)}.cloudflare`, program: deployApp({ app, mode, root: directory }).pipe(Effect.provide(ConfigProvider.layer(ConfigProvider.fromUnknown(rows)))) }),
                        Machine: ({ host }) =>
                            Effect.as(
                                Effect.fromOption(Record.get(apps, host), () => new Cause.NoSuchElementError(`${app.name} deploys to ${host}, machines with role Apps are ${Record.keys(apps).join(', ')}`)),
                                Option.some(on(mode, host)),
                            ),
                        Sites: () => Effect.succeedNone,
                    }),
                ),
                Array.getSomes,
            );
        },
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingStacks };
