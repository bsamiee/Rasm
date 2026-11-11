import { assert, it } from '@effect/vitest';
import { Effect, Record } from 'effect';
import { type Located, matched, Workflows } from '#/delivery/workflows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const KEYCHAIN = /login\.keychain|\bgenerate_keys\b|security\s+(?:add-generic-password|default-keychain)\b/u;
const KEY = /secrets\.\w*(?:SPARKLE|EDDSA|ED_PRIVATE)\w*/iu;
const FEED = /\b(?:release publish|generate_appcast|sign_update)\b/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const keyed = ({ step: { env = {} } }: Located) => Record.some(env, (value) => KEY.test(String(value)));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Workflows.live, { excludeTestServices: true })('update feed and notarization', (methods) => {
    methods.effect(
        'keeps signing keys out of the login keychain',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter(({ step }) => matched(KEYCHAIN, step.run)).map(({ at }) => at),
                [],
                'delivery/sparkle-project.github.io/documentation/index.md:95 `generate_keys` saves the private key in the login Keychain, delivery/cmux/.github/workflows/release.yml:105-106 "signing does not depend on mutable runner login-keychain state", delivery/cmux/scripts/sparkle_generate_appcast.sh:120 `--ed-key-file`',
            );
        }),
    );
    methods.effect(
        'passes the feed signing key to the publishing step alone',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => keyed(step) && !matched(FEED, step.step.run)).map(({ at }) => at),
                [],
                'delivery/cmux/.github/workflows/release.yml:310 `SPARKLE_PRIVATE_KEY` on the appcast step, delivery/sparkle-project.github.io/documentation/security-and-reliability/index.md:19 no key on a command line',
            );
        }),
    );
});
