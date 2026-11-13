import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-uv/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { pairs, placed, publishing, released, resolved } from '@rasm/testing/nx/projects';
import { Array, Duration, Effect, Layer, Option, pipe } from 'effect';
import { built, files, UV, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('5 minutes') } as const;
const MANIFESTS = ['packages/middle/pyproject.toml', 'packages/app/pyproject.toml', 'packages/core/pyproject.toml'] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-uv release of a uv workspace versioned together', (methods) => {
    const NEXT = '0.2.0';

    methods.effect(
        'gives every member uv builds a distribution for the release tag',
        Effect.fnUntraced(function* () {
            const [packaged, { tagged }] = yield* Effect.all([built, placed]);
            assert.includeMembers(
                tagged,
                packaged.map(({ root }) => root),
                `[${README.tagged}; ${UV.build}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'reads the current version of each member from its manifest',
        Effect.fnUntraced(function* () {
            const [packaged, shown] = yield* Effect.all([built, released('version', NEXT, '--dry-run')]);
            assert.strictEqual(shown.code, 0, shown.output);
            assert.deepStrictEqual(
                Array.difference(
                    packaged.map(({ manifest, version }) => `Resolved the current version as ${version} from manifest: ${manifest}`),
                    resolved(shown.output),
                ),
                [],
                `[${NX.disk}; ${UV.metadata} resolution[].version]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'writes the new version where uv reads it',
        Effect.fnUntraced(function* () {
            const [{ called }, packaged] = yield* Effect.all([Consumer, built]);
            const versioned = yield* released('version', NEXT);
            assert.strictEqual(versioned.code, 0, versioned.output);
            const read = yield* Effect.forEach(packaged, ({ name }) => called('uv', ['version', '--package', name, '--short']));
            assert.deepStrictEqual(
                read.map(({ stdout }) => stdout.trim()),
                packaged.map(() => NEXT),
                `[${NX.written}; ${UV.version}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'moves the pinned requirement of a dependent to the new version, and leaves the lock in line with the manifests',
        Effect.fnUntraced(function* () {
            const { called, text } = yield* Consumer;
            const checked = yield* called('uv', ['lock', '--check']);
            assert.include(yield* text('packages/middle/pyproject.toml'), `"rasm-nx-fixture-core==${NEXT}"`, `[${NX.dependents}]`);
            assert.strictEqual(Number(checked.code), 0, `[${NX.lock}; ${UV.checked}]: ${checked.stderr}`);
        }),
        bounds.test,
    );

    methods.effect(
        'changes nothing in a manifest but the version text, comments and layout kept',
        Effect.fnUntraced(function* () {
            const { text } = yield* Consumer;
            assert.deepStrictEqual(
                yield* Effect.forEach(MANIFESTS, text),
                MANIFESTS.map((manifest) => files[manifest].replaceAll('0.1.0', NEXT)),
                `[${NX.layout}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'gives every member uv builds a distribution for a publish target waiting on the publish of the members it depends on, and none to a member uv builds nothing for',
        Effect.fnUntraced(function* () {
            const [packaged, planned] = yield* Effect.all([built, publishing]);
            assert.sameMembers(
                planned.folders,
                packaged.map(({ root }) => root),
                `[${NX.publish}; ${UV.build}]`,
            );
            assert.sameMembers(planned.pairs, pairs(packaged), `[${NX.publish}; ${UV.metadata} resolution[].dependencies]`);
        }),
        bounds.test,
    );

    methods.effect(
        'runs uv own dry run for every publishable member under nx release publish --dry-run, uploading nothing',
        Effect.fnUntraced(function* () {
            const { committed } = yield* Consumer;
            yield* committed('release');
            const published = yield* released('publish', '--dry-run');
            assert.strictEqual(published.code, 0, `[${NX.published}]: ${published.output}`);
            assert.include(published.output, 'Checking', `[${NX.published}; ${UV.dryRun}]`);
            assert.notInclude(published.output, 'Uploading', `[${NX.published}; ${UV.dryRun}]`);
        }),
        bounds.test,
    );
});

it.layer(
    Layer.effectDiscard(Effect.flatMap(Consumer, ({ called }) => called('uv', ['lock']))).pipe(
        Layer.provideMerge(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: [plugin.name], release: { projects: ['tag:release'], version: { currentVersionResolver: 'registry' } } },
                ignored: ['.venv', 'dist', '.latest'],
                files: { 'packages/six/pyproject.toml': files['packages/core/pyproject.toml'].replace('rasm-nx-fixture-core', 'six'), 'packages/six/src/six/__init__.py': '' },
            }),
        ),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    ),
    { timeout: bounds.built, excludeTestServices: true },
)('@rasm/nx-uv release of a project PyPI holds', (methods) => {
    methods.effect(
        'reads the current version from the index uv installs from',
        Effect.fnUntraced(function* () {
            const { root, called } = yield* Consumer;
            const found = yield* called('uv', ['pip', 'install', '--dry-run', '--no-deps', '--target', `${root}/.latest`, 'six']);
            const latest = yield* pipe(
                Array.findFirst(`${found.stdout}${found.stderr}`.split('\n'), (line) => line.startsWith(' + six==')),
                Option.map((line) => line.replace(' + six==', '').trim()),
                Effect.fromOption,
                Effect.orDie,
            );
            const shown = yield* released('version', 'patch', '--dry-run');
            assert.include(shown.output, `Resolved the current version as ${latest} from the remote registry`, `[${NX.registry}; ${UV.latest}]`);
        }),
        bounds.test,
    );
});
