import { assert, it } from '@effect/vitest';
import { Effect, Predicate, Record } from 'effect';
import { fetched, type Located, matched, Workflows } from '#/delivery/workflows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PINNED = /^(?:\.\/|[\w./-]+@[0-9a-f]{40}$)/u;
const LOGIN = /^(?:NuGet\/login|rust-lang\/crates-io-auth-action|pypa\/gh-action-pypi-publish)@/u;
const PUSH = /\b(?:nx release publish|uv publish|pnpm publish|npm publish|cargo publish|dotnet nuget push)\b/u;
const RELEASE = /\bnx release\b/u;
const TOKEN = /^(?:NODE_AUTH_TOKEN|NPM_TOKEN|NUGET_API_KEY|UV_PUBLISH_TOKEN|UV_PUBLISH_PASSWORD|TWINE_PASSWORD|PYPI_TOKEN|CARGO_REGISTRY_TOKEN)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const publishing = ({ step }: Located) => matched(LOGIN, step.uses) || matched(PUSH, step.run);

const releasing = ({ step }: Located) => matched(RELEASE, step.run);

const hosted = ({ runner }: Located) => Predicate.isString(runner) && !runner.includes('self-hosted');

const stored = ({ at, step: { env = {} } }: Located) =>
    Record.collect(
        Record.filter(env, (value, name) => TOKEN.test(name) && String(value).includes('secrets.')),
        (name) => `${at} ${name}`,
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Workflows.live, { excludeTestServices: true })('registry and store publishing', (methods) => {
    methods.effect(
        'pins every action to a full commit SHA',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter(({ step: { uses } }) => uses !== undefined && !PINNED.test(uses)).map(({ at }) => at),
                [],
                "delivery/github-docs/src/workflows/tests/actions-workflows.ts 'requires specific hash', delivery/github-docs/content/actions/reference/security/secure-use.md",
            );
        }),
    );
    methods.effect(
        'states the token permissions of every workflow at its top',
        Effect.fnUntraced(function* () {
            const { files } = yield* Workflows;
            assert.deepStrictEqual(
                Record.keys(Record.filter(files, ({ permissions }) => permissions === undefined)),
                [],
                "delivery/github-docs/src/workflows/tests/actions-workflows.ts 'contains contents:read permissions when permissions are used', delivery/github-docs/content/actions/reference/security/secure-use.md",
            );
        }),
    );
    methods.effect(
        'grants id-token write to each job that publishes through trusted publishing',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.isNotEmpty(steps.filter(publishing), 'delivery/nx/astro-docs/src/content/docs/guides/Nx Release/publish-in-ci-cd.mdoc:225 the publish job');
            assert.deepStrictEqual(
                steps.filter((step) => publishing(step) && step.grants['id-token'] !== 'write').map(({ at }) => at),
                [],
                'delivery/NuGet-login/README.md:56, delivery/npm-documentation/content/packages-and-modules/securing-your-code/trusted-publishers.mdx:111-131, delivery/warehouse/docs/user/trusted-publishers/using-a-publisher.md:63',
            );
        }),
    );
    methods.effect(
        'publishes from runners GitHub hosts',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => publishing(step) && !hosted(step)).map(({ at }) => at),
                [],
                'delivery/npm-documentation/content/packages-and-modules/securing-your-code/trusted-publishers.mdx:23 "GitHub Actions (GitHub-hosted runners)"',
            );
        }),
    );
    methods.effect(
        'passes no stored registry token to a step',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.flatMap(stored),
                [],
                'delivery/NuGet-login/README.md:15-21 key from `steps.login.outputs.NUGET_API_KEY`, delivery/warehouse/docs/user/trusted-publishers/using-a-publisher.md:59-60 no `password`, delivery/crates-io-auth-action/README.md token from the action output, delivery/uv/docs/guides/package.md',
            );
        }),
    );
    methods.effect(
        'starts a release by workflow_dispatch, never by pushed tags',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.isNotEmpty(steps.filter(releasing), 'delivery/nx/astro-docs/src/content/docs/guides/Nx Release/publish-in-ci-cd.mdoc:20 `nx release --skip-publish`');
            assert.deepStrictEqual(
                steps.filter((step) => releasing(step) && !(step.triggers.includes('workflow_dispatch') && !step.triggers.includes('push'))).map(({ at }) => at),
                [],
                'delivery/nx/astro-docs/src/content/docs/guides/Nx Release/publish-in-ci-cd.mdoc:194-211 "workflows triggered by the push event will not run if more than 3 tags are created at once", Option 1 `workflow_dispatch`',
            );
        }),
    );
    methods.effect(
        'checks out the full history before a release',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => releasing(step) && !fetched(step)).map(({ at }) => at),
                [],
                'delivery/nx/astro-docs/src/content/docs/guides/Nx Release/publish-in-ci-cd.mdoc:151,230 `fetch-depth: 0` on the checkout of the release job',
            );
        }),
    );
});
