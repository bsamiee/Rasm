import { assert, it } from '@effect/vitest';
import { App } from '@rasm/hosting';
import { Access } from '@rasm/hosting/identity';
import { serveNode } from '@rasm/hosting/node';
import { ByteSize, ConfigProvider, Effect, type Record, Redacted, Schema } from 'effect';
import { HttpRouter, HttpServerResponse } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const app = new App({ name: 'stated', env: { GREETING: App.Binding.text({ value: 'hello' }), TOKEN: App.Binding.secret() }, allowed: [], targets: [], limits: { cpus: 1, memory: ByteSize.megabytes(256) } });

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect.each([
    { label: 'no sign-in source in its environment', variables: () => ({ PORT: '0', TOKEN: 'issued' }) },
    { label: 'no variable for a secret its app declares', variables: (access: string) => ({ PORT: '0', [Access.variable]: access }) },
    { label: 'no port in its environment', variables: (access: string) => ({ [Access.variable]: access, TOKEN: 'issued' }) },
])(
    'fails at start with ConfigError given $label',
    Effect.fnUntraced(function* ({ variables }: { readonly variables: (access: string) => Record.ReadonlyRecord<string, string> }) {
        const access = yield* Schema.encodeEffect(Access.json)(Access.make({ source: { _tag: 'Forwarded', id: 'x-forwarded-user', email: 'x-forwarded-email' }, logout: Access.cloudflare.logout }));
        const served = serveNode(app, (env) => HttpRouter.add('GET', '/env', HttpServerResponse.json({ greeting: env.GREETING, token: Redacted.value(env.TOKEN) })));
        const failure = yield* Effect.flip(Effect.provide(served, ConfigProvider.layer(ConfigProvider.fromEnv({ env: variables(access) }))));
        assert.strictEqual(failure._tag, 'ConfigError');
    }),
);
