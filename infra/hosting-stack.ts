import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { getOutputsForTargetAndConfiguration, type ProjectGraphProjectNode, workspaceRoot } from '@nx/devkit';
import { all, secret } from '@pulumi/pulumi';
import { App } from '@rasm/hosting';
import { deploy } from '@rasm/hosting/deploy';
import { Access, Enrollment } from '@rasm/hosting/identity';
import { Array, ByteSize, Cause, ConfigProvider, Effect, Option, Record, Redacted, Schema, Struct } from 'effect';
import type { Plugin } from 'vite';
import { bucketed, hosts, local, Machine, Role } from '#/platform-stack.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const placed = Effect.fnUntraced(function* (node: ProjectGraphProjectNode, app: App, mode: keyof typeof App.modes, host: string, owner: Option.Option<string>) {
    const [port, name, project, { machine, storage }] = [8080, App.resource(app, mode), node.name, Machine.joined(host)];
    const [outputs, { resolveConfig }] = yield* Effect.all([
        Effect.try({ try: () => getOutputsForTargetAndConfiguration({ project, target: 'build--vite' }, {}, node), catch: (cause) => new Cause.UnknownError(cause, `${project} states no build--vite outputs`) }),
        Effect.tryPromise({ try: () => import('vite'), catch: (cause) => new Cause.UnknownError(cause, 'vite does not load') }),
    ]);
    const outDir = yield* Effect.fromOption(Array.head(outputs), () => new Cause.NoSuchElementError(`${project} build--vite declares no output`));
    const { root, environments } = yield* Effect.tryPromise({
        try: () =>
            resolveConfig({ root: join(workspaceRoot, node.data.root), logLevel: 'silent', build: { outDir: join(workspaceRoot, outDir) } }, 'build', 'production', 'production', false, undefined, (plugins) => {
                plugins.splice(0, plugins.length, ...plugins.map(Struct.omit<Plugin, ['configResolved']>(['configResolved'])));
            }),
        catch: (cause) => new Cause.UnknownError(cause, `${project} holds no Vite config that resolves`),
    });
    const server = yield* Effect.fromOption(Record.get(environments, App.bundle.environment), () => new Cause.NoSuchElementError(`${project} builds no ${App.bundle.environment} environment`));
    const [projected, access] = yield* Effect.all([deploy.Machine({ app, mode, port, bundle: resolve(root, server.build.outDir) }), Effect.orDie(Schema.encodeEffect(Access.json)(machine.access))]);
    const { address, named, labels } = machine.routed(name, port, { [`traefik.http.routers.${name}.middlewares`]: Machine.guard });
    const serving = machine.contained(
        name,
        projected.healthcheck,
        {
            image: machine.imaged(name, projected.image.dockerfile, { context: projected.image.context }),
            envs: all({
                ...Record.map(projected.env, (value) => (Redacted.isRedacted(value) ? secret(Redacted.value(value)) : value)),
                ...(Record.isEmptyRecord(projected.buckets) ? {} : { ...bucketed(name, storage, projected.buckets), S3_PUBLIC_ENDPOINT_URL: storage.origin }),
                ...Option.match(owner, { onNone: () => ({}), onSome: (email) => ({ [Enrollment.variable]: JSON.stringify([email]) }) }),
                [Access.variable]: access,
            }).apply(Record.collect((variable, value) => `${variable}=${value}`)),
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

const hostingStacks = Effect.fnUntraced(function* (node: ProjectGraphProjectNode, released: Option.Option<{ readonly production: boolean; readonly rows: Record.ReadonlyRecord<string, string> }>) {
    const config = join(node.data.root, 'hosting.config.ts');
    const app = yield* Effect.tryPromise({ try: () => import(pathToFileURL(join(workspaceRoot, config)).href), catch: (cause) => new Cause.UnknownError(cause, `Cannot load ${config}`) }).pipe(
        Effect.map(Struct.get('default')),
        Effect.filterOrFail(
            (declared): declared is App => declared instanceof App,
            (declared) => new Cause.UnknownError(declared, `${config} exports no App by default`),
        ),
    );
    const provided = Effect.provide(ConfigProvider.layerAdd(ConfigProvider.fromUnknown(Option.match(released, { onNone: () => ({}), onSome: Struct.get('rows') })), { asPrimary: true }));
    const machined = (mode: keyof typeof App.modes, host: string, owner: Option.Option<string>) => ({ stackName: `${App.resource(app, mode)}.${host}`, program: provided(placed(node, app, mode, host, owner)) });
    return yield* Option.match(released, {
        onNone: () => Effect.succeed([machined('disposable', local.host, Option.some(local.owner))]),
        onSome: ({ production }) => {
            const mode = production ? 'production' : 'disposable';
            const apps = Record.filter<string, (typeof hosts)[keyof typeof hosts]>(hosts, ({ roles }) => roles.some(Role.$is('Apps')));
            return Effect.forEach(
                app.targets,
                App.Target.$match({
                    Cloudflare: () => Effect.succeed({ stackName: `${App.resource(app, mode)}.cloudflare`, program: provided(deploy.Cloudflare({ app, mode, root: join(workspaceRoot, node.data.root) })) }),
                    Machine: ({ host }) =>
                        Effect.as(
                            Effect.fromOption(Record.get(apps, host), () => new Cause.NoSuchElementError(`${app.name} deploys to ${host}, machines with role Apps are ${Record.keys(apps).join(', ')}`)),
                            machined(mode, host, Option.none()),
                        ),
                }),
            );
        },
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingStacks };
