import { D1Database, getZeroTrustOrganizationOutput, HyperdriveConfig, Provider, R2Bucket, R2BucketCors, R2BucketLifecycle, ZeroTrustAccessApplication, ZeroTrustAccessPolicy } from '@pulumi/cloudflare';
import { local } from '@pulumi/command';
import { type Input, interpolate, output, type Resource, secret } from '@pulumi/pulumi';
import { deployWorker } from '@rasm/workers/deploy';
import { Array, Config, Duration, Effect, Option, Record, Redacted, Schema } from 'effect';
import { App } from '#/app/app';
import { Access, type Identity } from '#/identity/identity';

// --- [OPERATIONS] ----------------------------------------------------------------------

const aged = (age: Duration.Duration) => ({ condition: { type: 'Age', maxAge: Duration.toSeconds(age) } });
const stored = ({ accountId, token, mode, variable, name, provided }: { readonly accountId: string; readonly token: Input<string>; readonly mode: keyof typeof App.modes; readonly variable: string; readonly name: string; readonly provided: { readonly provider: Provider } }) =>
    App.Binding.$match({
        sqlite: () => {
            const database = new D1Database(name, { accountId, name }, { ...provided, protect: App.modes[mode].kept });
            return Effect.succeed({ binding: { type: 'd1', id: database.id }, resources: Array.of<Resource>(database) });
        },
        postgres: () =>
            Effect.map(Config.URL(variable), ({ hostname, port, pathname, username, password }) => {
                const origin = { scheme: 'postgres', host: hostname, ...(port !== '' && { port: Number(port) }), database: pathname.slice(1), user: decodeURIComponent(username), password: secret(decodeURIComponent(password)) };
                const pooled = new HyperdriveConfig(name, { accountId, name, origin }, provided);
                return { binding: { type: 'hyperdrive', id: pooled.id }, resources: Array.of<Resource>(pooled) };
            }),
        bucket: ({ abort, expire, infrequentAccess, cors }) => {
            const bucket = new R2Bucket(name, { accountId, name }, { ...provided, protect: App.modes[mode].kept });
            const rules = [
                { id: 'abort', enabled: true, conditions: { prefix: '' }, abortMultipartUploadsTransition: aged(abort) },
                ...Record.collect({ [App.uploads]: abort, ...expire[mode] }, (prefix, age) => ({ id: `expire ${prefix}*`, enabled: true, conditions: { prefix }, deleteObjectsTransition: aged(age) })),
                ...Record.collect(infrequentAccess, (prefix, age) => ({ id: `infrequent access ${prefix}*`, enabled: true, conditions: { prefix }, storageClassTransitions: [{ storageClass: 'InfrequentAccess', ...aged(age) }] })),
            ];
            const allowed = cors.map(({ origins, methods, headers, exposed, age }) => ({ allowed: { origins: [...origins], methods: [...methods], headers: [...headers] }, exposeHeaders: [...exposed], maxAgeSeconds: Duration.toSeconds(age) }));
            const emptying = {
                interpreter: ['node', '--input-type=module', '--eval'],
                environment: { JOBS: interpolate`https://api.cloudflare.com/client/v4/accounts/${accountId}/r2/buckets/${bucket.name}/jobs`, CLOUDFLARE_API_TOKEN: secret(token) },
                delete: `const call = async (url, init) => {
                    const answer = await fetch(url, { ...init, headers: { authorization: 'Bearer ' + process.env.CLOUDFLARE_API_TOKEN, 'content-type': 'application/json' } });
                    const { result, errors } = await answer.json();
                    if (!answer.ok || result.status === 'FAILED' || result.status === 'CANCELLED') throw new Error(url + ' answered ' + answer.status + ' ' + JSON.stringify(answer.ok ? result : errors));
                    return result;
                };
                let job = await call(process.env.JOBS, { method: 'POST', body: '{"jobType":"prefixDelete","prefix":""}' });
                while (job.status !== 'COMPLETED') {
                    await new Promise((resume) => setTimeout(resume, 2000));
                    job = await call(process.env.JOBS + '/' + job.id, {});
                }`,
            };
            return Effect.succeed({
                binding: { type: 'r2_bucket', bucketName: bucket.name },
                resources: [
                    bucket,
                    new R2BucketLifecycle(name, { accountId, bucketName: bucket.name, rules }, provided),
                    ...(Array.isArrayNonEmpty(allowed) ? [new R2BucketCors(name, { accountId, bucketName: bucket.name, rules: allowed }, provided)] : []),
                    ...(App.modes[mode].kept ? [] : [new local.Command(`${name}-objects`, emptying)]),
                ],
            });
        },
        secret: () => Effect.map(Config.Redacted(variable), (value) => ({ binding: { type: 'secret_text', text: secret(Redacted.value(value)) }, resources: Array.empty<Resource>() })),
        text: ({ value }) => Effect.succeed({ binding: { type: 'plain_text', text: value }, resources: Array.empty<Resource>() }),
        json: ({ value }) => Effect.succeed({ binding: { type: 'json', json: JSON.stringify(value) }, resources: Array.empty<Resource>() }),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const deployApp = Effect.fn('deployApp')(function* ({ app, mode, identity, root }: { readonly app: App; readonly mode: keyof typeof App.modes; readonly identity: Identity; readonly root: string }) {
    const [{ accountId, token }, context] = yield* Effect.all([Config.all({ accountId: Config.String('CLOUDFLARE_ACCOUNT_ID'), token: Config.Redacted('CLOUDFLARE_API_TOKEN') }), Effect.context<never>()]);
    const name = App.resource(app, mode);
    const provided = { provider: new Provider(name, { apiToken: Redacted.value(token) }) };
    const stores = yield* Effect.forEach(Record.toEntries(app.env), ([variable, binding]) => Effect.map(stored({ accountId, token: Redacted.value(token), mode, variable, name: App.resource(app, mode, variable), provided })(binding), (store) => ({ ...store, variable })));
    return yield* deployWorker({
        accountId,
        root,
        production: App.modes[mode].kept,
        named: (configured) => `${configured}${App.modes[mode].suffix}`,
        options: { ...provided, dependsOn: stores.flatMap(({ resources }) => resources) },
        bindings: (hosts) => ({
            ...Record.fromIterableWith(stores, ({ variable, binding }) => [variable, binding]),
            ...Option.match(Option.liftPredicate(app.allowed, Array.isReadonlyArrayNonEmpty), {
                onNone: () => ({}),
                onSome: (addresses) => {
                    const policy = new ZeroTrustAccessPolicy(name, { accountId, name, decision: 'allow', includes: addresses.map((email) => ({ email: { email } })) }, provided);
                    const destinations = hosts.apply(Array.map((host) => ({ type: 'public', uri: `${host}${identity.paths.signin}` })));
                    const guarded = new ZeroTrustAccessApplication(name, { accountId, name, type: 'self_hosted', destinations, policies: [{ id: policy.id, precedence: 1 }], httpOnlyCookieAttribute: true, sameSiteCookieAttribute: 'lax' }, provided);
                    const source = { _tag: 'Cloudflare', teamDomain: getZeroTrustOrganizationOutput({ accountId }, provided).authDomain, audience: guarded.aud } as const;
                    return { [Access.variable]: { type: 'plain_text', text: output({ source, logout: Access.cloudflare.logout }).apply((access) => Effect.runPromiseWith(context)(Schema.encodeEffect(Access.json)(Access.make(access)))) } };
                },
            }),
        }),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { deployApp };
