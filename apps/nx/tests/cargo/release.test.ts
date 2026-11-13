import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-cargo/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { placed, publishing, released, resolved } from '@rasm/testing/nx/projects';
import { Array, Duration, Effect, Layer, Option, pipe } from 'effect';
import { CARGO, files, members, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('5 minutes') } as const;
const MANIFESTS = ['crates/middle/Cargo.toml', 'crates/app/Cargo.toml', 'crates/core/Cargo.toml'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const open = Effect.map(members, (stated) => stated.filter((crate) => crate.open));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cargo release of a Cargo workspace versioned together', (methods) => {
    const NEXT = '0.2.0';

    methods.effect(
        'gives every crate cargo may publish the release tag',
        Effect.fnUntraced(function* () {
            const [stated, { tagged }] = yield* Effect.all([open, placed]);
            assert.includeMembers(
                tagged,
                stated.map(({ root }) => root),
                `[${README.tagged}; ${CARGO.metadata} packages[].publish]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'reads the current version of each crate from the manifest that states it',
        Effect.fnUntraced(function* () {
            const [stated, shown] = yield* Effect.all([open, released('version', NEXT, '--dry-run')]);
            assert.strictEqual(shown.code, 0, shown.output);
            assert.deepStrictEqual(
                Array.difference(
                    stated.filter(({ manifest }) => manifest !== 'vendor/shim/Cargo.toml').map(({ manifest, version }) => `Resolved the current version as ${version} from manifest: ${manifest}`),
                    resolved(shown.output),
                ),
                [],
                `[${NX.disk}; ${CARGO.metadata} packages[].version]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'writes the new version where cargo reads it, a version inherited from the workspace included',
        Effect.fnUntraced(function* () {
            const versioned = yield* released('version', NEXT);
            assert.strictEqual(versioned.code, 0, versioned.output);
            const stated = yield* open;
            assert.deepStrictEqual(
                stated.filter(({ version }) => version !== NEXT).map(({ name, version }) => `${name} ${version}`),
                [],
                `[${NX.written}; ${CARGO.metadata} packages[].version]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'moves the requirement of every dependent to the new version, a renamed dependency and a table included, and leaves a path-only one alone',
        Effect.fnUntraced(function* () {
            const stated = yield* members;
            assert.sameMembers(
                stated.flatMap(({ requirements }) => requirements),
                [`crates/core -> vendor/shim ^${NEXT}`, `crates/middle -> crates/core ^${NEXT}`, `crates/app -> crates/middle ^${NEXT}`, 'tools/generator -> crates/core *'],
                `[${NX.dependents}; ${CARGO.metadata} packages[].dependencies[].req]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'leaves the lock in line with the manifests',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const locked = yield* called('cargo', ['metadata', '--format-version', '1', '--locked']);
            assert.strictEqual(Number(locked.code), 0, `[${NX.lock}; ${CARGO.locked}]: ${locked.stderr}`);
        }),
        bounds.test,
    );

    methods.effect(
        'changes nothing in a manifest but the version text, comments and layout kept',
        Effect.fnUntraced(function* () {
            const { text } = yield* Consumer;
            assert.deepStrictEqual(
                yield* Effect.forEach(MANIFESTS, text),
                MANIFESTS.map((manifest) => files[manifest].replaceAll('"0.1.0"', `"${NEXT}"`)),
                `[${NX.layout}]`,
            );
            assert.strictEqual(yield* text('vendor/shim/Cargo.toml'), files['vendor/shim/Cargo.toml'], `[${NX.layout}; Cargo reference "workspace.package"]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives every crate cargo may publish a publish target waiting on the publish of the crates it depends on, and none to a crate with publish = false',
        Effect.fnUntraced(function* () {
            const [stated, planned] = yield* Effect.all([open, publishing]);
            assert.sameMembers(
                planned.folders,
                stated.map(({ root }) => root),
                `[${NX.publish}; ${NX.private}; ${CARGO.metadata} packages[].publish]`,
            );
            assert.sameMembers(
                planned.pairs,
                stated.flatMap(({ waits }) => waits),
                `[${NX.publish}; ${CARGO.metadata} packages[].dependencies]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'runs cargo own dry run for every publishable crate under nx release publish --dry-run, uploading nothing',
        Effect.fnUntraced(function* () {
            const { committed } = yield* Consumer;
            yield* committed('release');
            const published = yield* released('publish', '--dry-run');
            assert.strictEqual(published.code, 0, `[${NX.published}]: ${published.output}`);
            assert.include(published.output, 'aborting upload due to dry run', `[${NX.published}; ${CARGO.dryRun}]`);
            assert.notInclude(published.output, 'Uploaded', `[${NX.published}; ${CARGO.dryRun}]`);
        }),
        bounds.test,
    );
});

it.layer(
    Layer.effectDiscard(Effect.flatMap(Consumer, ({ called }) => called('cargo', ['generate-lockfile']))).pipe(
        Layer.provideMerge(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: [plugin.name], release: { projects: ['tag:release'], version: { currentVersionResolver: 'registry' } } },
                ignored: ['target'],
                files: { 'Cargo.toml': '[package]\nname = "itoa"\nversion = "0.0.1"\nedition = "2024"\n', 'src/lib.rs': '' },
            }),
        ),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    ),
    { timeout: bounds.built, excludeTestServices: true },
)('@rasm/nx-cargo release of a crate crates.io holds', (methods) => {
    methods.effect(
        'reads the current version from the registry cargo searches',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const found = yield* called('cargo', ['search', 'itoa', '--limit', '1']);
            const latest = yield* pipe(
                Array.findFirst(found.stdout.split('\n'), (line) => line.startsWith('itoa = "')),
                Option.flatMap((line) => Array.get(line.split('"'), 1)),
                Effect.fromOption,
                Effect.orDie,
            );
            const shown = yield* released('version', 'patch', '--dry-run');
            assert.include(shown.output, `Resolved the current version as ${latest} from the remote registry`, `[${NX.registry}; ${CARGO.search}]`);
        }),
        bounds.test,
    );
});
