import { join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import aws from '@pulumi/aws';
import { local } from '@pulumi/command';
import { IamServiceAccount, IlmPolicy, Provider } from '@pulumi/minio';
import postgresql from '@pulumi/postgresql';
import { all, getOrganization, getProject, type Input, interpolate, jsonStringify, type Resource, runtime, StackReference, secret } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { Access } from '@rasm/hosting';
import { App, type Binding } from '@rasm/hosting/app';
import { Sites } from '@rasm/hosting/openai-sites';
import { deployApp } from '@rasm/hosting/pulumi';
import { Forward, Host, Ssh } from '@rasm/ssh';
import { Array, ByteSize, Cause, Clock, Config, ConfigProvider, Data, Duration, Effect, FileSystem, flow, Layer, ManagedRuntime, Match, Option, Path, Predicate, Record, Redacted, Result, Schema, SchemaGetter, SchemaIssue, Scope, Struct } from 'effect';
import { Yaml } from 'effect/encoding';
import { CommandLine } from 'effect/process';
import { deployed, run } from '#/command';
import { backupTarget, connections, Deployed, definition, identified, type platformStacks, uploaded } from '#/platform-stack';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stack {
    readonly app: App;
    readonly mode: keyof typeof App.modes;
    readonly directory: string;
    readonly context: ExecutorContext;
}
interface Operation {
    readonly create: Array.NonEmptyReadonlyArray<string>;
    readonly delete: Array.NonEmptyReadonlyArray<string>;
    readonly stdin: NonNullable<local.CommandArgs['stdin']>;
    readonly environment: Readonly<Record<string, Input<string>>>;
}
interface Applied {
    readonly protect: boolean;
    readonly dependsOn: readonly Resource[];
}
type Row = (stack: Stack) => Effect.Effect<Readonly<Record<string, unknown>>, Error, Layer.Success<typeof layer>>;
type Engine = Data.TaggedEnum<{ Machine: Record.ReadonlyRecord<never, never>; Host: { readonly name: string; readonly domain: string; readonly values: Readonly<Record<string, string>> } }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const mount = '/data';
const forwarded = { id: 'Remote-User', email: 'Remote-Email' };
const compose = Array.make('docker', 'compose', '--file', '-');

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Data.taggedEnum<Engine>();
const Stated = Schema.Record(Schema.String, Schema.Unknown).annotate({ description: 'Host a platform stack outputs, less the identity file each machine places' });
const Document = Schema.fromJsonString(Schema.Unknown);
const Bound = Schema.Struct({ target: Schema.Int, published: Schema.FiniteFromString, host_ip: Schema.String, app_protocol: Schema.String });
const Session = Schema.Struct({ session: Schema.Struct({ cookies: Schema.NonEmptyArray(Schema.Struct({ domain: Schema.NonEmptyString, authelia_url: Schema.URLFromString })) }) });
const Platform = Schema.fromJsonString(
    Schema.Struct({
        networks: Schema.Struct({ default: Schema.Struct({ name: Schema.NonEmptyString }) }),
        services: Schema.Struct({
            postgres: Schema.Struct({ ports: Schema.NonEmptyArray(Bound), environment: Schema.Struct({ POSTGRES_USER: Schema.NonEmptyString, POSTGRES_PASSWORD: Schema.NonEmptyString }) }),
            pgbouncer: Schema.Struct({ ports: Schema.NonEmptyArray(Bound.mapFields(Struct.pick(['target', 'app_protocol']))) }),
            storage: Schema.Struct({ ports: Schema.NonEmptyArray(Bound), environment: Schema.Struct({ RUSTFS_ACCESS_KEY: Schema.NonEmptyString, RUSTFS_SECRET_KEY: Schema.NonEmptyString, RUSTFS_REGION: Schema.NonEmptyString }) }),
            traefik: Schema.Struct({ networks: Schema.Struct({ default: Schema.Struct({ aliases: Schema.NonEmptyArray(Schema.NonEmptyString) }) }) }),
        }),
        configs: Schema.Struct({
            authelia: Schema.Struct({
                content: Schema.String.pipe(
                    Schema.decodeTo(Schema.Unknown, {
                        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => Yaml.parse(text), catch: () => new SchemaIssue.InvalidValue({ expected: 'a YAML document' }, text, options) })),
                        encode: SchemaGetter.forbiddenEncoding,
                    }),
                    Schema.decodeTo(Session),
                ),
            }),
        }),
        'x-hosted-app': Schema.Struct({ configs: Schema.Unknown, services: Schema.Struct(Struct.Record(['app', 'litestream', 'litestream-restore'], Schema.Unknown)) }),
    }),
);

// --- [SERVICES] ------------------------------------------------------------------------

const tunnels = ManagedRuntime.make(Ssh.layer(connections).pipe(Layer.provideMerge(layer)));

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = <A, E>(effect: Effect.Effect<A, E, Ssh | Layer.Success<typeof layer> | Scope.Scope>) => Effect.flatten(Effect.promise(() => tunnels.runPromiseExit(Effect.flatMap(Scope.make(), (scope) => Scope.provide(scope)(effect)))));

const bundled = Effect.fnUntraced(function* ({ context, directory }: Stack) {
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
    return Record.map({ server, client }, ({ build }) => relative(context.root, resolve(root, build.outDir)));
});

const machine = (root: string) => ({
    variables: {},
    settings: ConfigProvider.fromEnv(),
    backup: Option.none(),
    reach: ({
        postgres: {
            ports: [postgres],
        },
        storage: {
            ports: [storage],
        },
    }: (typeof Platform.Type)['services']) => Effect.succeed({ postgres: { host: postgres.host_ip, port: postgres.published }, storage: { endpoint: new URL(`${storage.app_protocol}://${storage.host_ip}:${storage.published}`), tls: false } }),
    apply: (project: string, { create, delete: removed, stdin, environment }: Operation, _: readonly string[], { protect, dependsOn }: Applied) =>
        Effect.map(Effect.fromResult(Result.all({ create: CommandLine.join(create), delete: CommandLine.join(removed) })), (lines) => new local.Command(project, { ...lines, stdin, environment: secret(environment), dir: root, addPreviousOutputInEnv: false }, { protect, dependsOn: [...dependsOn] })),
});

const tunneled = Effect.fnUntraced(function* (
    host: Host,
    {
        postgres: {
            ports: [postgres],
        },
        traefik,
    }: (typeof Platform.Type)['services'],
) {
    const folder = yield* Effect.flatMap(FileSystem.FileSystem, (fs) => fs.makeTempDirectoryScoped());
    const forward = yield* Schema.decodeEffect(Forward)({ _tag: 'Local', bind: join(folder, `.s.PGSQL.${postgres.published}`), connect: `${postgres.host_ip}:${postgres.published}` });
    yield* runtime.isDryRun() ? Effect.void : Ssh.use((ssh) => ssh.forward(host, forward));
    return { postgres: { host: folder, port: postgres.published }, storage: { endpoint: new URL(`https://${Array.headNonEmpty(traefik.networks.default.aliases)}`), tls: true } };
});

const copied = Effect.fnUntraced(function* (host: Host, root: string, project: string, { create, delete: removed, stdin, environment }: Operation, bundles: readonly string[], { protect, dependsOn }: Applied) {
    const paths = yield* Path.Path;
    const over = (words: Array.NonEmptyReadonlyArray<string>) => Effect.fromResult(Result.flatMap(host.command(paths, connections, words, Record.keys(environment)), CommandLine.join));
    const [placed, up, down] = yield* Effect.all([Effect.forEach(bundles, (bundle) => uploaded(host, project, root, bundle, [])), over(create), over(removed)]);
    return new local.Command(project, { create: up, delete: down, stdin, environment: secret(environment), addPreviousOutputInEnv: false }, { protect, dependsOn: [...dependsOn, ...placed] });
});

const hosted = Effect.fnUntraced(function* (root: string, { name, domain, values }: Data.TaggedEnum.Value<Engine, 'Host'>) {
    const reference = new StackReference(`${getOrganization()}/${getProject()}/${name}`);
    const read = (member: keyof Effect.Success<Effect.Success<ReturnType<typeof platformStacks>>[number]['program']>) => Effect.tryPromise({ try: () => reference.getOutputDetails(member), catch: (cause) => new Cause.UnknownError(cause, `stack ${name} holds no ${member} output`) });
    const [connection, identity, backup] = yield* Effect.all([read('connection'), read('identity'), backupTarget(values, domain)]);
    const file = identified(name);
    const [stated, material] = yield* Effect.all([Schema.decodeUnknownEffect(Stated)(connection.value), Schema.decodeUnknownEffect(Schema.NonEmptyString)(identity.secretValue)]);
    const host = yield* Schema.decodeUnknownEffect(Host)({ ...stated, identity: { _tag: 'Key', file } });
    yield* runtime.isDryRun() ? Effect.void : held(Ssh.use((ssh) => ssh.written(host, { [file]: material })));
    return {
        variables: { ...values, PLATFORM_DOMAIN: domain },
        settings: ConfigProvider.fromUnknown(values),
        backup,
        reach: (services: (typeof Platform.Type)['services']) => held(tunneled(host, services)),
        apply: (project: string, operation: Operation, bundles: readonly string[], options: Applied) => copied(host, root, project, operation, bundles, options),
    };
});

const engaged = (engine: Engine, root: string) => Engine.$match(engine, { Machine: () => Effect.succeed(machine(root)), Host: (fields) => hosted(root, fields) });

const bucketed =
    ({ app, mode }: Stack, providers: { readonly storage: aws.Provider; readonly admin: Provider }) =>
    (variable: string, { abort, expire, cors }: Data.TaggedEnum.Value<Binding, 'bucket'>) => {
        const named = App.resource(app, mode, variable);
        const bucket = new aws.s3.Bucket(named, { bucket: named, forceDestroy: !App.modes[mode].kept }, { provider: providers.storage, protect: App.modes[mode].kept });
        const rules = Record.collect({ [App.uploads]: abort, ...expire[mode] }, (prefix, age) => ({ id: `expire ${prefix}*`, filter: prefix, expiration: `${Math.ceil(Duration.toDays(age))}d` }));
        const lifecycle = new IlmPolicy(named, { bucket: bucket.bucket, rules }, { provider: providers.admin });
        const allowed = cors.map(({ origins, methods, headers, exposed, age }) => ({ allowedOrigins: [...origins], allowedMethods: [...methods], allowedHeaders: [...headers], exposeHeaders: [...exposed], maxAgeSeconds: Duration.toSeconds(age) }));
        return { bucket, resources: [bucket, lifecycle, ...(Array.isArrayNonEmpty(allowed) ? [new aws.s3.BucketCorsConfiguration(named, { bucket: bucket.id, corsRules: allowed }, { provider: providers.storage })] : [])] };
    };

const stored = (stack: Stack, { storage: { environment } }: (typeof Platform.Type)['services'], reached: { readonly endpoint: URL; readonly tls: boolean }, within: URL) =>
    Option.map(Option.liftPredicate(Record.filter(stack.app.env, App.Binding.$is('bucket')), Predicate.not(Record.isEmptyRecord)), (named) => {
        const { RUSTFS_ACCESS_KEY: accessKey, RUSTFS_SECRET_KEY: secretKey, RUSTFS_REGION: region } = environment;
        const name = App.resource(stack.app, stack.mode);
        const admin = new Provider(name, { minioServer: reached.endpoint.host, minioSsl: reached.tls, minioUser: accessKey, minioPassword: secret(secretKey), minioRegion: region });
        const storage = new aws.Provider(name, { region, accessKey, secretKey: secret(secretKey), endpoints: [{ s3: reached.endpoint.href }], s3UsePathStyle: true, skipCredentialsValidation: true, skipMetadataApiCheck: true, skipRegionValidation: true, skipRequestingAccountId: true });
        const created = Record.collect(named, bucketed(stack, { storage, admin }));
        const policy = jsonStringify({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: created.flatMap(({ bucket }) => [bucket.arn, interpolate`${bucket.arn}/*`]) }] });
        const account = new IamServiceAccount(name, { targetUser: accessKey, policy }, { provider: admin, dependsOn: created.flatMap(Struct.get('resources')) });
        return { environment: { AWS_ENDPOINT_URL_S3: within.href, AWS_REGION: region, AWS_ACCESS_KEY_ID: account.accessKey, AWS_SECRET_ACCESS_KEY: account.secretKey }, resources: [account] };
    });

const postgresRoles = ({ app, mode }: Stack, variable: string, server: Required<Pick<postgresql.ProviderArgs, 'host' | 'port' | 'username' | 'password'>>, within: Readonly<Record<'postgres' | 'pgbouncer', URL>>) => {
    const database = App.resource(app, mode, variable);
    const provided = { provider: new postgresql.Provider(database, { ...server, sslmode: 'disable' }) };
    const roles = Record.map({ migrator: { idleInTransactionSessionTimeout: 60_000 }, runtime: { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000 } }, (settings: postgresql.RoleArgs, role) => {
        const key = new RandomPassword(`${database}-${role}`, { length: 40, special: false }).result;
        return { name: new postgresql.Role(`${database}-${role}`, { name: `${database}-${role}`, login: true, password: key, ...settings }, provided).name, key };
    });
    const owned = new postgresql.Database(database, { name: database, owner: roles.migrator.name }, { ...provided, protect: App.modes[mode].kept });
    const granted = Record.map({ table: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'], sequence: ['USAGE', 'SELECT'] }, (privileges, objectType) => new postgresql.DefaultPrivileges(`${database}-${objectType}`, { database: owned.name, owner: roles.migrator.name, role: roles.runtime.name, objectType, privileges }, provided));
    const url = (endpoint: URL, role: Input<string>, key: Input<string>) =>
        all([role, key, owned.name]).apply(([username, password, pathname]) => {
            const target = Object.assign(new URL(endpoint), { username, password, pathname });
            target.searchParams.set('sslmode', 'disable');
            return target.href;
        });
    return {
        [variable]: url(
            within.pgbouncer,
            all([granted.table.role, granted.sequence.role]).apply(([role]) => role),
            roles.runtime.key,
        ),
        [`${variable}_MIGRATOR`]: url(within.postgres, roles.migrator.name, roles.migrator.key),
    };
};

const stores = Effect.fnUntraced(function* (stack: Stack, { services }: typeof Platform.Type, reached: Effect.Success<ReturnType<Effect.Success<ReturnType<typeof engaged>>['reach']>>) {
    const { app, mode } = stack;
    const within = Record.map(Struct.pick(services, ['postgres', 'pgbouncer', 'storage']), ({ ports: [{ target, app_protocol }] }, service) => new URL(`${app_protocol}://${service}:${target}`));
    const server = { ...reached.postgres, username: services.postgres.environment.POSTGRES_USER, password: secret(services.postgres.environment.POSTGRES_PASSWORD) };
    const bound = yield* Effect.forEach(Record.toEntries(app.env), ([variable, binding]) =>
        Match.value<Binding>(binding).pipe(
            Match.tag('sqlite', () => Effect.succeed<Readonly<Record<string, Input<string>>>>({ [variable]: join(mount, variable, 'database.sqlite') })),
            Match.tag('postgres', () => Effect.succeed(postgresRoles(stack, variable, server, within))),
            Match.tag('bucket', () => Effect.succeed({ [variable]: App.resource(app, mode, variable) })),
            Match.tag('secret', () => Config.all({ [variable]: Config.map(Config.Redacted(variable), Redacted.value) })),
            Match.tag('text', 'json', () => Effect.succeed({})),
            Match.exhaustive,
        ),
    );
    const buckets = stored(stack, services, reached.storage, within.storage);
    return { environment: Record.fromEntries([...bound, ...Option.toArray(Option.map(buckets, Struct.get('environment')))].flatMap(Record.toEntries)), resources: Option.match(buckets, { onNone: () => [], onSome: Struct.get('resources') }) };
});

const documented = Effect.fnUntraced(function* ({ app, mode }: Stack, { networks, configs, 'x-hosted-app': template }: typeof Platform.Type, bundle: Effect.Success<ReturnType<typeof bundled>>, variables: readonly string[], backup: Effect.Success<ReturnType<typeof engaged>>['backup']) {
    const fs = yield* FileSystem.FileSystem;
    const [name, { domain }] = [App.resource(app, mode), Array.headNonEmpty(configs.authelia.content.session.cookies)];
    const [base, file, memory] = yield* Effect.all([fs.makeTempFileScoped({ suffix: '.json' }), fs.makeTempFileScoped({ suffix: '.json' }), Effect.fromOption(ByteSize.toNumber(app.limits.memory))]);
    const extended = Record.map(template.services, (_, service) => ({ extends: { file: base, service } }));
    const mounts = Record.fromIterableWith(Record.keys(Record.filter(app.env, App.Binding.$is('sqlite'))), (variable) => [App.resource(app, mode, variable), join(mount, variable)]);
    const replicas = Option.match(backup, {
        onNone: () => [],
        onSome: ({ replica }) =>
            Record.keys(mounts).map((source) => {
                const [restore, volumes, environment] = [`litestream-restore-${source}`, [{ type: 'volume', source, target: mount }], { LITESTREAM_REPLICA_URL: replica(source) }];
                return { restore, services: { [restore]: { ...extended['litestream-restore'], environment, volumes }, [`litestream-${source}`]: { ...extended.litestream, depends_on: { [restore]: { condition: 'service_completed_successfully' } }, environment, volumes } } };
            }),
    });
    const written = (path: string, document: unknown) => Effect.flatMap(Effect.orDie(Schema.encodeEffect(Document)(document)), (text) => fs.writeFileString(path, text));
    yield* Effect.all([
        written(base, { services: template.services }),
        written(file, {
            name,
            configs: template.configs,
            services: {
                app: {
                    ...extended.app,
                    build: { context: bundle.server, additional_contexts: { assets: bundle.client }, args: { ASSETS: App.bundle.assets } },
                    cpus: app.limits.cpus,
                    mem_limit: memory,
                    environment: Record.fromIterableWith(variables, (variable) => [variable, null]),
                    labels: { 'traefik.enable': 'true', [`traefik.http.routers.${name}.rule`]: `Host(\`${name}.${domain}\`)`, [`traefik.http.routers.${name}.entrypoints`]: 'websecure', [`traefik.http.routers.${name}.middlewares`]: 'authelia' },
                    volumes: Record.collect(mounts, (source, target) => ({ type: 'volume', source, target })),
                    depends_on: Record.fromIterableWith(replicas, ({ restore }) => [restore, { condition: 'service_completed_successfully' }]),
                },
                ...Record.fromEntries(replicas.flatMap(({ services }) => Record.toEntries(services))),
            },
            networks: { default: { name: networks.default.name, external: true } },
            volumes: Record.map(mounts, (_, source) => ({ name: source })),
        }),
    ]);
    return { host: `${name}.${domain}`, stdin: yield* run('docker', ['compose', '--file', file, 'config', '--no-path-resolution', '--format', 'json']) };
}, Effect.scoped);

const served = (engine: Engine): Row =>
    Effect.fnUntraced(function* (stack) {
        const [bundle, row, applied, commands] = yield* Effect.all([bundled(stack), engaged(engine, stack.context.root), Clock.currentTimeMillis, Effect.succeed({ create: Array.appendAll(compose, ['up', '--build', '--detach', '--wait', '--remove-orphans']), delete: Array.appendAll(compose, ['down', '--volumes']) })]);
        const platform = yield* Effect.flatMap(run('docker', ['compose', '--file', definition, 'config', '--format', 'json'], row.variables), Schema.decodeEffect(Platform));
        const [{ environment, resources }, access] = yield* Effect.all([
            Effect.flatMap(row.reach(platform.services), (reached) => stores(stack, platform, reached)).pipe(Effect.provide(ConfigProvider.layer(row.settings))),
            Effect.orDie(Schema.encodeEffect(Access.json)(Access.make({ source: { _tag: 'Forwarded', ...forwarded }, logout: new URL('logout', Array.headNonEmpty(platform.configs.authelia.content.session.cookies).authelia_url).href }))),
        ]);
        const variables = { ...environment, [Access.variable]: access };
        const { host, stdin } = yield* documented(stack, platform, bundle, Record.keys(variables), row.backup);
        const replicated = Option.match(row.backup, { onNone: () => ({}), onSome: Struct.get('environment') });
        const project = yield* row.apply(App.resource(stack.app, stack.mode), { ...commands, stdin, environment: { ...replicated, ...variables, APPLIED: `${applied}` } }, Object.values(bundle), { protect: App.modes[stack.mode].kept, dependsOn: resources });
        return { url: `https://${host}`, project: project.id };
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingValues = {
    local: served(Engine.Machine()),
    cloudflare: ({ app, mode, directory }: Stack) => deployApp({ app, mode, identity: Sites.identity, root: directory }).pipe(Effect.provide(Layer.unwrap(Effect.map(deployed, flow(ConfigProvider.fromUnknown, ConfigProvider.layer))))),
};
const hostingRows = Effect.gen(function* () {
    const values = yield* deployed;
    const { DEPLOYED_HOSTS } = yield* Schema.decodeUnknownEffect(Deployed)(values);
    return { ...hostingValues, ...Record.map(DEPLOYED_HOSTS, ({ domain }, name) => served(Engine.Host({ name, domain: domain.name, values }))) } satisfies Readonly<Record<string, Row>>;
});
const hostingStack = Effect.fn('hostingStack')(function* (context: ExecutorContext, rows: Readonly<Record<string, Row>>, value: string, production: boolean) {
    const project = yield* Effect.fromNullishOr(context.projectName);
    const directory = join(context.root, (yield* Effect.fromNullishOr(context.projectGraph.nodes[project])).data.root);
    const [[mode], row] = yield* Effect.all([Effect.fromOption(Record.findFirst(App.modes, ({ kept }) => kept === production)), Effect.fromOption(Record.get(rows, value), () => new Cause.NoSuchElementError(`${project} has no hosting row ${value}, rows are ${Record.keys(rows).join(', ')}`))]);
    const app = yield* Effect.tryPromise({ try: () => import(pathToFileURL(join(directory, 'hosting.config.ts')).href), catch: (cause) => new Cause.UnknownError(cause, `Cannot load ${directory}/hosting.config.ts`) }).pipe(
        Effect.map(Struct.get('default')),
        Effect.filterOrFail(
            (declared): declared is App => declared instanceof App,
            (declared) => new Cause.UnknownError(declared, `${directory}/hosting.config.ts exports no App by default`),
        ),
    );
    return { stackName: `${App.resource(app, mode)}.${value}`, program: row({ app, mode, directory, context }) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingRows, hostingStack, hostingValues };
