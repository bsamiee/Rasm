import type { WebhookError } from '@rasm/webhooks';
import { TailscaleEvent } from '@rasm/webhooks/tailscale';
import { Config, Effect, type Layer, Record, type Redacted, Schema, String } from 'effect';
import type { CryptoError, Digest, Mac, Signature } from 'effect/crypto';
import type { EncodingError } from 'effect/encoding';
import { HttpRouter, type HttpServerError } from 'effect/http';
import type { PersistedQueue } from 'effect/persistence';

// --- [TABLES] --------------------------------------------------------------------------

const senders = {
    tailscale: Effect.fnUntraced(function* (name: string, secret: Redacted.Redacted) {
        const webhook = yield* TailscaleEvent.webhook([secret]);
        const logged = TailscaleEvent.matchOrElse({ KeyExpiry: () => Effect.logWarning, Policy: () => Effect.logWarning }, () => Effect.logInfo);
        const record = Effect.fnUntraced(function* (id: string, occurred: typeof TailscaleEvent.Type) {
            const { data, ...fields } = yield* Effect.orDie(Schema.encodeEffect(TailscaleEvent)(occurred));
            const labels = Record.mapKeys<string, unknown, string>({ ...fields, ...data }, (field) => `${name}.${field}`);
            yield* Effect.annotateLogs(logged(occurred)('Webhook event'), { 'webhook.sender': name, 'webhook.id': id, ...labels });
        });
        const accept = yield* webhook.queued({ name }, ({ id, event }) =>
            Effect.withSpan(
                Effect.forEach(event, (occurred) => record(id, occurred), { discard: true }),
                `${name} delivery`,
            ),
        );
        return HttpRouter.route('POST', path(name), webhook.receive(accept));
    }),
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const path = <S extends string>(sender: S) => `/${sender}` as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const paths = Record.map(senders, (_, sender) => path(sender));
const routes: Layer.Layer<
    never,
    EncodingError.EncodingError | CryptoError.CryptoError | Config.ConfigError,
    | HttpRouter.HttpRouter
    | Digest.Digest
    | Mac.Mac
    | Signature.Signature
    | PersistedQueue.PersistedQueueFactory
    | HttpRouter.Request<'Error', Schema.SchemaError>
    | HttpRouter.Request<'Error', WebhookError>
    | HttpRouter.Request<'Error', CryptoError.CryptoError>
    | HttpRouter.Request<'Error', HttpServerError.HttpServerError>
    | HttpRouter.Request<'Error', PersistedQueue.PersistedQueueError>
> = HttpRouter.use((router) =>
    Effect.flatMap(
        Effect.forEach(Record.toEntries(senders), ([name, sender]) => Effect.flatMap(Config.Redacted(`${String.toUpperCase(name)}_WEBHOOK_SECRET`), (secret) => sender(name, secret))),
        (served) => router.addAll(served),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { paths, routes };
