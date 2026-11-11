import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { Json5 } from 'effect/encoding';
import { type Located, matched, Workflows } from '#/delivery/workflows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ACTION = /^renovatebot\/github-action@/u;
const ISSUED = /\b(?:secrets\.(?!GITHUB_TOKEN\b)\w+|steps\.[\w-]+\.outputs\.\w+)/u;
const BUMP = /\b(?:mise lock|pinact run|cargo update|uv lock (?:-U|--upgrade)|pnpm (?:up|update))\b/u;

// --- [MODELS] --------------------------------------------------------------------------

const Policy = Schema.Struct({ automerge: Schema.optionalKey(Schema.Boolean), ignoreTests: Schema.optionalKey(Schema.Boolean), lockFileMaintenance: Schema.optionalKey(Schema.Struct({ enabled: Schema.optionalKey(Schema.Boolean) })) }).annotate({
    description: 'Renovate repository configuration keys deciding lock file maintenance and automerge',
});
const Allowed = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const renovating = ({ step }: Located) => matched(ACTION, step.uses);

const issued = ({ step: { with: given = {} } }: Located) => Option.exists(Record.get(given, 'token'), (token) => ISSUED.test(String(token)));

const allowed = ({ step: { env = {} } }: Located) => Effect.map(Schema.decodeUnknownEffect(Allowed)(Option.getOrElse(Record.get(env, 'RENOVATE_ALLOWED_UNSAFE_EXECUTIONS'), () => '[]')), (names) => names.includes('mise'));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Workflows.live, { excludeTestServices: true })('Renovate bumps', (methods) => {
    methods.effect(
        'runs Renovate on a schedule and on dispatch',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.isNotEmpty(steps.filter(renovating), 'delivery/cilium/.github/workflows/renovate.yaml `renovatebot/github-action`');
            assert.deepStrictEqual(
                steps.filter((step) => renovating(step) && !(step.triggers.includes('schedule') && step.triggers.includes('workflow_dispatch'))).map(({ at }) => at),
                [],
                'delivery/cilium/.github/workflows/renovate.yaml:2-10 `schedule` and `workflow_dispatch`',
            );
        }),
    );
    methods.effect(
        'gives Renovate a token that starts workflows on its pull requests',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => renovating(step) && !issued(step)).map(({ at }) => at),
                [],
                'delivery/renovatebot-github-action/README.md:172-185 a personal access token or a GitHub App token, never `GITHUB_TOKEN`, delivery/cilium/.github/workflows/renovate.yaml `steps.get_token.outputs.token`',
            );
        }),
    );
    methods.effect(
        'allows the mise lock run a repository with a mise lock file needs',
        Effect.fnUntraced(function* () {
            const [{ steps }, fs, path] = yield* Effect.all([Workflows, FileSystem.FileSystem, Path.Path]);
            const locked = yield* fs.exists(path.join(workspaceRoot, '.config/mise.lock'));
            const allowing = yield* Effect.forEach(steps.filter(renovating), allowed);
            assert.deepStrictEqual(
                allowing,
                allowing.map(() => locked),
                'delivery/renovate/lib/modules/manager/mise/readme.md "Allowing Unsafe Executions", delivery/renovate/lib/modules/manager/mise/artifacts.spec.ts:171 no MISE_SAFE on the allowlisted path',
            );
        }),
    );
    methods.effect(
        'maintains lock files and merges an update once its checks pass',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const text = yield* fs.readFileString(path.join(workspaceRoot, '.github/renovate.json5'));
            const { automerge, ignoreTests = false, lockFileMaintenance } = yield* Effect.flatMap(Effect.fromResult(Json5.parse(text)), Schema.decodeUnknownEffect(Policy));
            assert.deepStrictEqual(
                { automerge, ignoreTests, maintained: lockFileMaintenance?.enabled },
                { automerge: true, ignoreTests: false, maintained: true },
                'delivery/renovate/lib/modules/manager/mise/readme.md "Lock File Maintenance" runs `mise lock --bump`, delivery/renovate/docs/usage/key-concepts/automerge.md:260 "will not automerge until it sees passing status checks"',
            );
        }),
    );
    methods.effect(
        'leaves every dependency bump to Renovate',
        Effect.fnUntraced(function* () {
            const [{ steps }, fs, path] = yield* Effect.all([Workflows, FileSystem.FileSystem, Path.Path]);
            const second = yield* fs.exists(path.join(workspaceRoot, '.github/dependabot.yml'));
            assert.deepStrictEqual({ second, bumping: steps.filter(({ step }) => matched(BUMP, step.run)).map(({ at }) => at) }, { second: false, bumping: [] }, 'delivery/cilium/.github/renovate.json5 and delivery/cilium/.github/workflows/renovate.yaml: one updater moves actions, tools and lock files');
        }),
    );
});
