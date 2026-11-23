import { layer as crypto } from '@effect/platform-node/NodeCrypto';
import { assert, it } from '@effect/vitest';
import { Scheme } from '@rasm/webhooks';
import { Duration, Effect, Function, Record } from 'effect';
import { Headers } from 'effect/http';
import { TestClock } from 'effect/testing';
import { made, outcome, pair, tolerance, vectors } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const second = Duration.toMillis('1 second');
const clocks = { early: -Duration.toMillis(tolerance) - second, soon: -Duration.toMillis(tolerance), edge: Duration.toMillis(tolerance), late: Duration.toMillis(tolerance) + second };

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect(
    'accepts the timestamp of each scheme row at the tolerance in either direction and refuses one a second past it',
    Effect.fnUntraced(function* () {
        const results = yield* Effect.all(
            Record.map(
                vectors,
                Effect.fnUntraced(function* ({ scheme, secret, id, at, bytes }) {
                    const [webhook, signedAt] = [yield* made(scheme, secret), Duration.toMillis(Duration.seconds(at))];
                    const headers = yield* Effect.andThen(TestClock.setTime(signedAt), webhook.sign(id, bytes));
                    return yield* Effect.all(Record.map(clocks, (offset) => Effect.andThen(TestClock.setTime(signedAt + offset), outcome(webhook.verify(headers, bytes)))));
                }),
            ),
        );
        assert.deepStrictEqual(
            results,
            Record.map(vectors, ({ accepted, late }) => ({ early: late, soon: accepted, edge: accepted, late })),
        );
    }, Effect.provide(crypto)),
);
it.effect(
    'accepts either secret of a sender signing with two, in each header syntax, and verifies with a public key that cannot sign',
    Effect.fnUntraced(function* () {
        const { id, bytes, secret, foreign } = vectors.standard;
        const rotated = yield* Effect.all(
            Record.map(
                { spaced: Scheme.standard, keyed: Scheme.stripe },
                Effect.fnUntraced(function* (scheme) {
                    const [both, former, latter, mixed, stranger] = yield* Effect.all([made(scheme, secret, foreign), made(scheme, secret), made(scheme, foreign), made(scheme, vectors.svix.secret, foreign), made(scheme, vectors.svix.secret)]);
                    const [doubled, single] = yield* Effect.all([both.sign(id, bytes), former.sign(id, bytes)]);
                    return yield* Effect.forEach([former.verify(doubled, bytes), latter.verify(doubled, bytes), mixed.verify(doubled, bytes), both.verify(single, bytes), stranger.verify(doubled, bytes), latter.verify(single, bytes)], outcome);
                }),
            ),
        );
        const [signer, verifier, shared] = yield* Effect.all([made(Scheme.standard, pair.private), made(Scheme.standard, pair.public), made(Scheme.standard, secret)]);
        const signed = yield* signer.sign(id, bytes);
        const asymmetric = yield* Effect.all([outcome(verifier.verify(signed, bytes)), Effect.match(verifier.sign(id, bytes), { onSuccess: Function.constant('signed'), onFailure: ({ _tag }) => _tag }), outcome(shared.verify(signed, bytes))]);
        const accepted = ['verified', 'verified', 'verified', 'verified', 'Mismatch', 'Mismatch'];
        assert.deepStrictEqual([rotated, asymmetric], [{ spaced: accepted, keyed: accepted }, ['verified', 'IllegalArgumentError', 'Mismatch']]);
    }, Effect.provide(crypto)),
);
it.effect(
    'keys a delivery by the id its signature covers, and by the SHA-256 of its body where the signature covers none',
    Effect.fnUntraced(function* () {
        const { id, bytes, secret } = vectors.standard;
        const bodyDigest = 'ae858931f67887e8150d6f96c9fe03062c1df36b4464c4ddc8e002c084d5d198';
        const identified = Effect.fnUntraced(function* (scheme: Scheme, change: (headers: Headers.Headers) => Headers.Headers) {
            const webhook = yield* made(scheme, secret);
            const [initial, repeated] = yield* Effect.all([webhook.sign(id, bytes), Effect.andThen(TestClock.adjust('1 minute'), webhook.sign(id, bytes))]);
            return yield* Effect.forEach([initial, repeated, change(initial)], (headers) => webhook.verify(headers, bytes).pipe(Effect.unwrapReason('WebhookError'), Effect.match({ onSuccess: (delivery) => delivery.id, onFailure: ({ _tag }) => _tag })));
        });
        const keys = yield* Effect.all([identified(Scheme.standard, Headers.set('webhook-id', 'replayed')), identified(Scheme.github, Headers.set('x-github-delivery', 'replayed')), identified(Scheme.tailscale, Function.identity)]);
        assert.deepStrictEqual(keys, [
            [id, id, 'Mismatch'],
            [bodyDigest, bodyDigest, bodyDigest],
            [bodyDigest, bodyDigest, bodyDigest],
        ]);
    }, Effect.provide(crypto)),
);
