import { layerTest } from '@effect/platform-node/NodeHttpServer';
import { assert, it } from '@effect/vitest';
import { Access, Identity } from '@rasm/hosting';
import { App } from '@rasm/hosting/app';
import { serveNode } from '@rasm/hosting/node';
import { ByteSize, ConfigProvider, Effect, Layer, type Record, Redacted, Schema } from 'effect';
import { HttpRouter, HttpServerResponse } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const identity = new Identity({ headers: { id: 'x-user-id', email: 'x-user-email' }, paths: { signin: '/signin', signout: '/signout' }, returnTo: 'return_to' });
const megabytes = 256;
const limits = { cpus: 1, memory: ByteSize.megabytes(megabytes) };
const apps = {
    stated: new App({ name: 'stated', env: { GREETING: App.Binding.text({ value: 'hello' }), TOKEN: App.Binding.secret() }, allowed: [], limits }),
    structured: new App({ name: 'structured', env: { GREETING: App.Binding.text({ value: 'hello' }), TOKEN: App.Binding.secret(), SETTINGS: App.Binding.json({ value: { depth: 2 } }) }, allowed: [], limits }),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = Effect.fnUntraced(function* (app: (typeof apps)[keyof typeof apps], variables: (access: string) => Record.ReadonlyRecord<string, string>) {
    const access = yield* Schema.encodeEffect(Access.json)(Access.make({ source: { _tag: 'Forwarded', id: 'x-forwarded-user', email: 'x-forwarded-email' }, logout: Access.cloudflare.logout }));
    const served = serveNode(app, identity, (env) => HttpRouter.add('GET', '/env', HttpServerResponse.json({ greeting: env.GREETING, token: Redacted.value(env.TOKEN) })));
    const failure = yield* Effect.flip(Layer.build(Layer.provide(served, ConfigProvider.layer(ConfigProvider.fromEnv({ env: variables(access) })))));
    return failure._tag;
}, Effect.provide(layerTest));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect.each([
    { label: 'no sign-in source in its environment', app: apps.stated, variables: () => ({ TOKEN: 'issued' }), failure: 'ConfigError' },
    { label: 'no variable for a secret its app declares', app: apps.stated, variables: (access: string) => ({ [Access.variable]: access }), failure: 'ConfigError' },
    { label: 'no template in its bundle', app: apps.stated, variables: (access: string) => ({ [Access.variable]: access, TOKEN: 'issued' }), failure: 'HostingError' },
    { label: 'no template in the bundle of an app declaring a JSON value', app: apps.structured, variables: (access: string) => ({ [Access.variable]: access, TOKEN: 'issued' }), failure: 'HostingError' },
])(
    'fails at start with $failure given $label',
    Effect.fnUntraced(function* ({ app, variables, failure }) {
        assert.strictEqual(yield* refused(app, variables), failure);
    }),
);
