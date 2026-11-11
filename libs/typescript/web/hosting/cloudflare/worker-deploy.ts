import { getZeroTrustOrganizationOutput, HyperdriveConfig, Provider, R2BucketCors, R2BucketLifecycle, ZeroTrustAccessApplication, ZeroTrustAccessPolicy } from '@pulumi/cloudflare';
import { local } from '@pulumi/command';
import { interpolate, output, secret } from '@pulumi/pulumi';
import { deployWorker } from '@rasm/workers/deploy';
import { Array, Cause, Config, Duration, Effect, Option, Record, Redacted, Schema } from 'effect';
import { App } from '#/app/app';
import { Access, type Identity } from '#/identity/identity';

// --- [OPERATIONS] ----------------------------------------------------------------------

const aged = (age: Duration.Duration) => ({ condition: { type: 'Age', maxAge: Duration.toSeconds(age) } });

// --- [COMPOSITION] ---------------------------------------------------------------------

const deployApp = Effect.fn('deployApp')(function* ({ app, mode, identity, root }: { readonly app: App; readonly mode: keyof typeof App.modes; readonly identity: Identity; readonly root: string }) {
    const [{ accountId, token }, context] = yield* Effect.all([Config.all({ accountId: Config.String('CLOUDFLARE_ACCOUNT_ID'), token: Config.Redacted('CLOUDFLARE_API_TOKEN') }), Effect.context<never>()]);
    const name = App.resource(app, mode);
    const provided = { provider: new Provider(name, { apiToken: Redacted.value(token) }) };
    const pooled = yield* Effect.forEach(Record.keys(Record.filter(app.env, App.Binding.$is('postgres'))), (variable) =>
        Effect.map(Config.URL(variable), ({ hostname, port, pathname, username, password }) => {
            const [store, origin] = [App.resource(app, mode, variable), { scheme: 'postgres', host: hostname, ...(port !== '' && { port: Number(port) }), database: pathname.slice(1), user: decodeURIComponent(username), password: secret(decodeURIComponent(password)) }];
            return [variable, { type: 'hyperdrive', id: new HyperdriveConfig(store, { accountId, name: store, origin }, provided).id }] as const;
        }),
    );
    const { stores, workers } = yield* deployWorker({
        accountId,
        root,
        production: App.modes[mode].kept,
        named: (configured) => `${configured}${App.modes[mode].suffix}`,
        options: provided,
        bindings: (hosts) => ({
            ...Record.fromEntries(pooled),
            ...Option.match(Option.liftPredicate(app.allowed, Array.isReadonlyArrayNonEmpty), {
                onNone: () => ({}),
                onSome: (addresses) => {
                    const policy = new ZeroTrustAccessPolicy(name, { accountId, name, decision: 'allow', includes: addresses.map((email) => ({ email: { email } })) }, provided);
                    const destinations = hosts.apply(Array.map((host) => ({ type: 'public', uri: `${host}${identity.paths.signin}` })));
                    const guarded = new ZeroTrustAccessApplication(name, { accountId, name, type: 'self_hosted', destinations, policies: [{ id: policy.id, precedence: 1 }], httpOnlyCookieAttribute: true, sameSiteCookieAttribute: 'lax' }, provided);
                    const source = { _tag: 'Cloudflare', teamDomain: getZeroTrustOrganizationOutput({ accountId }, provided).authDomain, audience: guarded.aud } as const;
                    return { [Access.variable]: { type: 'plain_text', text: output({ source, logout: Access.cloudflare.logout }).apply((access) => Effect.runPromiseWith(context)(Effect.flatMap(Schema.decodeEffect(Access)(access), Schema.encodeEffect(Access.json)))) } };
                },
            }),
        }),
    });
    yield* Effect.forEach(
        Record.toEntries(Record.filter(app.env, App.Binding.$is('bucket'))),
        Effect.fnUntraced(function* ([variable, { abort, expire, infrequentAccess, cors }]) {
            const [stated, store] = [App.resource(app, 'production', variable), App.resource(app, mode, variable)];
            const bucket = yield* Effect.fromOption(Record.get(stores.r2, stated), () => new Cause.NoSuchElementError(`Build Output of ${root} binds no bucket ${stated}, the store ${variable} of ${app.name}`));
            const rules = [
                { id: 'abort', enabled: true, conditions: { prefix: '' }, abortMultipartUploadsTransition: aged(abort) },
                ...Record.collect({ [App.uploads]: abort, ...expire[mode] }, (prefix, age) => ({ id: `expire ${prefix}*`, enabled: true, conditions: { prefix }, deleteObjectsTransition: aged(age) })),
                ...Record.collect(infrequentAccess, (prefix, age) => ({ id: `infrequent access ${prefix}*`, enabled: true, conditions: { prefix }, storageClassTransitions: [{ storageClass: 'InfrequentAccess', ...aged(age) }] })),
            ];
            const allowed = cors.map(({ origins, methods, headers, exposed, age }) => ({ allowed: { origins: [...origins], methods: [...methods], headers: [...headers] }, exposeHeaders: [...exposed], maxAgeSeconds: Duration.toSeconds(age) }));
            const emptying = {
                interpreter: ['node', '--input-type=module', '--eval'],
                environment: { JOBS: interpolate`https://api.cloudflare.com/client/v4/accounts/${accountId}/r2/buckets/${bucket.name}/jobs`, CLOUDFLARE_API_TOKEN: secret(Redacted.value(token)) },
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
            return [
                new R2BucketLifecycle(store, { accountId, bucketName: bucket.name, rules }, provided),
                ...(Array.isArrayNonEmpty(allowed) ? [new R2BucketCors(store, { accountId, bucketName: bucket.name, rules: allowed }, provided)] : []),
                ...(App.modes[mode].kept ? [] : [new local.Command(`${store}-objects`, emptying)]),
            ];
        }),
        { discard: true },
    );
    return workers;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { deployApp };
