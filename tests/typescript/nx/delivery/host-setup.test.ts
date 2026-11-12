import { assert, it } from '@effect/vitest';
import { Effect } from 'effect';
import { type Located, matched, Workflows } from '#/delivery/workflows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SETUP = /^[^./][^/]*\/setup-[^@]+@/u;
const OUTPUT = /\b(?:steps|needs)\.[\w-]+\.outputs\./u;
const BLENDER = /blender/iu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const installing = ({ triggers, step }: Located) => triggers.some((name) => ['pull_request', 'pull_request_target'].includes(name)) && matched(SETUP, step.uses);

const derived = ({ gates }: Located) => gates.some((gate) => OUTPUT.test(gate));

const common = ({ triggers, step }: Located) => triggers.some((name) => ['pull_request', 'pull_request_target', 'push'].includes(name)) && (matched(BLENDER, step.uses) || matched(BLENDER, step.run));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Workflows.live, { excludeTestServices: true })('conditional host setup', (methods) => {
    methods.effect(
        'gates every setup action of a pull request workflow on its step or job',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => installing(step) && step.gates.length === 0).map(({ at }) => at),
                [],
                "delivery/blender-plugin/.github/workflows/ci.yml job `blender` `if: needs.changes.outputs.native == 'true'`, delivery/supabase-js/.github/workflows/ci-core.yml `if: needs.check-affected.outputs.<pkg> != '0'`",
            );
        }),
    );
    methods.effect(
        'reads each gate of a setup action from an output of the run',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => installing(step) && step.gates.length > 0 && !derived(step)).map(({ at }) => at),
                [],
                'delivery/ag-grid/.github/workflows/ci.yml `needs.init.outputs.e2e_test_count > 0`, delivery/blender-plugin/.github/workflows/ci.yml `needs.changes.outputs.native`',
            );
        }),
    );
    methods.effect(
        'installs Blender in no workflow a pull request or a push starts',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter(common).map(({ at }) => at),
                [],
                'delivery/blender-plugin/.github/workflows/blender-baseline.yml holds the Blender install behind `workflow_call`, delivery/MolecularNodes/.github/workflows/release.yml installs it in the release job',
            );
        }),
    );
});
