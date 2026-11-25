import { assert, it } from '@effect/vitest';
import { Scheme, WebhookError } from '@rasm/webhooks';
import { Bytes, DateTime, Duration, Effect, Function, Option, Record } from 'effect';
import { Headers } from 'effect/http';
import { TestClock } from 'effect/testing';
import { type Crypto, encoder, made, outcome, standard, vectors } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const standardwebhooks = await import('standardwebhooks');
const svix = await import('svix');
const { default: Stripe } = await import('stripe');
const octokit = await import('@octokit/webhooks-methods');
const label = /\bv1a?\b|sha256/u;
const tail = /.{4}$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const references = {
    standardwebhooks: {
        scheme: Scheme.standard,
        secret: standard.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers, now: DateTime.Utc) {
            const reference = new standardwebhooks.Webhook(standard.secret);
            const accepted = yield* Effect.as(Effect.try({ try: () => reference.verify(standard.body, { ...ours }), catch: WebhookError.of({ _tag: 'Mismatch' }, 'standardwebhooks reference') }), true);
            return { accepted, theirs: { 'webhook-id': standard.id, 'webhook-timestamp': `${DateTime.toEpochSeconds(now)}`, 'webhook-signature': reference.sign(standard.id, DateTime.toDate(now), standard.body) } };
        }),
    },
    svix: {
        scheme: Scheme.svix,
        secret: vectors.svix.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers, now: DateTime.Utc) {
            const reference = new svix.Webhook(vectors.svix.secret);
            const accepted = yield* Effect.as(Effect.try({ try: () => reference.verify(standard.body, { ...ours }), catch: WebhookError.of({ _tag: 'Mismatch' }, 'svix reference') }), true);
            return { accepted, theirs: { 'svix-id': standard.id, 'svix-timestamp': `${DateTime.toEpochSeconds(now)}`, 'svix-signature': reference.sign(standard.id, DateTime.toDate(now), standard.body) } };
        }),
    },
    stripe: {
        scheme: Scheme.stripe,
        secret: vectors.stripe.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers, now: DateTime.Utc) {
            const header = yield* Effect.fromOption(Headers.get(ours, Scheme.stripe.signature));
            const accepted = yield* Effect.as(Effect.tryPromise({ try: () => Stripe.webhooks.constructEventAsync(standard.body, header, vectors.stripe.secret), catch: WebhookError.of({ _tag: 'Mismatch' }, 'stripe reference') }), true);
            return { accepted, theirs: { [Scheme.stripe.signature]: yield* Effect.promise(() => Stripe.webhooks.generateTestHeaderStringAsync({ payload: standard.body, secret: vectors.stripe.secret, timestamp: DateTime.toEpochSeconds(now) })) } };
        }),
    },
    '@octokit/webhooks-methods': {
        scheme: Scheme.github,
        secret: vectors.octokit.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers) {
            const header = yield* Effect.fromOption(Headers.get(ours, Scheme.github.signature));
            const [accepted, signed] = yield* Effect.all([Effect.promise(() => octokit.verify(vectors.octokit.secret, standard.body, header)), Effect.promise(() => octokit.sign(vectors.octokit.secret, standard.body))]);
            return { accepted, theirs: { [Scheme.github.signature]: signed } };
        }),
    },
};
const signatures = (crypto: Crypto) => {
    it.effect.each(Record.keys(vectors))(
        'signs the known-answer vector of scheme row %s, verifies it among other signatures, and refuses another body, a cut signature, another label, another key, and no signature',
        Effect.fnUntraced(function* (row) {
            const { scheme, secret, foreign, id, at, bytes, signed, crowded, accepted } = vectors[row];
            const [webhook, stranger] = yield* Effect.all([made(scheme, secret), made(scheme, foreign)]);
            yield* TestClock.setTime(Duration.toMillis(Duration.seconds(at)));
            const [headers, forged] = yield* Effect.all([webhook.sign(id, bytes), stranger.sign(id, bytes)]);
            const rewritten = Record.map({ crowded: crowded(signed), cut: crowded(signed.replace(tail, '')), relabelled: signed.replace(label, 'v9') }, (text) => Headers.set(headers, scheme.signature, text));
            const presented = yield* Effect.all(Record.map({ signed: headers, ...rewritten, forged, unsigned: Headers.remove(headers, scheme.signature) }, (changed) => outcome(webhook.verify(changed, bytes))));
            const verified = { header: Headers.get(headers, scheme.signature), body: yield* outcome(webhook.verify(headers, bytes.subarray(1))), ...presented };
            assert.deepStrictEqual(verified, { header: Option.some(signed), body: 'Mismatch', signed: accepted, crowded: accepted, cut: 'Mismatch', relabelled: 'Mismatch', forged: 'Mismatch', unsigned: 'Unsigned' });
        }, Effect.provide(crypto)),
    );
    it.effect.each([
        { label: 'no id header', change: Headers.remove('webhook-id'), verified: 'Unsigned' },
        { label: 'no timestamp header', change: Headers.remove('webhook-timestamp'), verified: 'Unsigned' },
        { label: 'a timestamp that is no number', change: Headers.set('webhook-timestamp', 'hello'), verified: 'Unsigned' },
        { label: 'a signature entry holding no value', change: Headers.set('webhook-signature', 'v1,'), verified: 'Mismatch' },
        { label: 'malformed signature entries before a valid one', change: Headers.set('webhook-signature', `v1 v1,not-base64! ${vectors.standard.signed}`), verified: 'verified' },
    ])(
        'standard scheme: a request with $label verifies as $verified',
        Effect.fnUntraced(function* ({ change, verified }) {
            const { scheme, secret, id, at, bytes } = vectors.standard;
            const webhook = yield* made(scheme, secret);
            yield* TestClock.setTime(Duration.toMillis(Duration.seconds(at)));
            assert.strictEqual(yield* outcome(webhook.verify(change(yield* webhook.sign(id, bytes)), bytes)), verified);
        }, Effect.provide(crypto)),
    );
    it.effect(
        'standard scheme: refuses a secret holding no key, verifies with a secret written without its whsec_ prefix, and signs the bytes of a view alone',
        Effect.fnUntraced(function* () {
            const { scheme, secret, id, at, bytes } = vectors.binary;
            const [empty, webhook] = yield* Effect.all([Effect.match(made(scheme, 'whsec_'), { onSuccess: () => 'built', onFailure: ({ _tag }) => _tag }), made(scheme, secret)]);
            yield* TestClock.setTime(Duration.toMillis(Duration.seconds(at)));
            const margin = encoder.encode('--');
            const headers = yield* webhook.sign(id, Bytes.concat([margin, bytes, margin]).subarray(margin.length, margin.length + bytes.length));
            const unprefixed = yield* Effect.flatMap(made(scheme, secret.slice('whsec_'.length)), (reader) => outcome(reader.verify(headers, bytes))).pipe(Effect.orElseSucceed(Function.constant('refused at construction')));
            assert.deepStrictEqual([empty, Headers.get(headers, scheme.signature), unprefixed], ['CryptoError', Option.some(vectors.binary.signed), vectors.binary.accepted]);
        }, Effect.provide(crypto)),
    );
    it.effect(
        'standard scheme: signs a UTF-8 byte order mark like any other byte, and decodes the JSON after it',
        Effect.fnUntraced(function* () {
            const { scheme, secret, id, bytes } = vectors.standard;
            const webhook = yield* made(scheme, secret);
            const marked = Bytes.concat([encoder.encode('﻿'), bytes]);
            const headers = yield* webhook.sign(id, marked);
            const [event, unmarked] = yield* Effect.all([Effect.match(webhook.verify(headers, marked), { onSuccess: (delivery) => delivery.event, onFailure: ({ _tag }) => _tag }), outcome(webhook.verify(headers, bytes))]);
            assert.deepStrictEqual([event, unmarked], [{ test: 2_432_232_314 }, 'Mismatch']);
        }, Effect.provide(crypto)),
    );
    it.live.each(Record.keys(references))(
        'exchanges signatures with the %s reference library',
        Effect.fnUntraced(function* (name) {
            const { scheme, secret, exchanged } = references[name];
            const [webhook, now, bytes] = [yield* made(scheme, secret), yield* DateTime.now, encoder.encode(standard.body)];
            const { accepted, theirs } = yield* exchanged(yield* webhook.sign(standard.id, bytes), now);
            const { event } = yield* webhook.verify(Headers.fromInput(theirs), bytes);
            assert.deepStrictEqual([accepted, event], [true, { test: 2_432_232_314 }]);
        }, Effect.provide(crypto)),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { signatures };
