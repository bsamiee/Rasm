import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Consumer, Packed } from '@rasm/testing/nx';
import { Duration, Effect, Layer, Record } from 'effect';
import { files, members, placed, plugin, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = 'PROJECT_VERSION printed by a CMAKE_PROJECT_INCLUDE probe while cmake configures the tree';
const INSTALLS = 'CMake file API codemodel-v2 reply directories[].hasInstallRule';
const PACKAGES = 'README.md DELIVERY "C++ packages release as a CPack archive of their install tree"';
const TAGGED = 'README.md PRINCIPLES "Plugins tag a project they give release configuration `release`"';
const DISK = 'nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts:100, version/test-utils.ts:152-164';
const WRITTEN = 'nrwl/nx packages/nx/src/command-line/release/version/test-utils.ts:181-202';
const LAYOUT = 'nrwl/nx packages/js/src/release/version-actions.spec.ts:111-145, 187-208';
const NEXT = '0.2.0';
const bounds = { built: Duration.minutes(5), test: Duration.toMillis(Duration.minutes(5)) };

// --- [OPERATIONS] ----------------------------------------------------------------------

const released = (...args: readonly string[]) => Effect.flatMap(Consumer, ({ nx }) => Effect.map(nx('release', ...args), ({ code, stdout, stderr }) => ({ code: Number(code), output: `${stdout}${stderr}` })));

const installed = Effect.map(members, (stated) => stated.filter((member) => member.installed));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.provideMerge(Packed.layer(plugin.name), layer), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cmake release', (suite) => {
    suite.layer(workspace(), { timeout: bounds.built })('of CMake projects stating a version', (methods) => {
        methods.effect(
            'gives every project with an install rule the release tag',
            Effect.fnUntraced(function* () {
                const [stated, { projects }] = yield* Effect.all([installed, placed]);
                assert.isNotEmpty(stated, `[${INSTALLS}]`);
                assert.includeMembers(
                    projects.filter(({ data }) => (data.tags ?? []).includes('release')).map(({ data }) => data.root),
                    stated.map(({ root }) => root),
                    `[${TAGGED}; ${PACKAGES}; ${INSTALLS}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'reads the current version of each released project from the CMakeLists.txt whose project() call states it',
            Effect.fnUntraced(function* () {
                const [stated, shown] = yield* Effect.all([installed, released('version', NEXT, '--dry-run')]);
                assert.strictEqual(shown.code, 0, shown.output);
                assert.deepStrictEqual(
                    stated.filter(({ manifest, version }) => !shown.output.includes(`Resolved the current version as ${version} from manifest: ${manifest}`)).map(({ manifest }) => manifest),
                    [],
                    `[${DISK}; ${VERSION}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'writes the new version where CMake reads PROJECT_VERSION',
            Effect.fnUntraced(function* () {
                const before = yield* installed;
                const versioned = yield* released('version', NEXT);
                assert.strictEqual(versioned.code, 0, versioned.output);
                const after = yield* members;
                assert.deepStrictEqual(
                    Record.fromIterableWith(
                        after.filter(({ root }) => before.some((member) => member.root === root)),
                        ({ name, version }) => [name, version],
                    ),
                    Record.fromIterableWith(before, ({ name }) => [name, NEXT]),
                    `[${WRITTEN}; ${VERSION}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'changes nothing in a CMakeLists.txt but the version text, comments and layout kept',
            Effect.fnUntraced(function* () {
                const [{ text }, { projects }] = yield* Effect.all([Consumer, placed]);
                const manifests = (['apps/tool/CMakeLists.txt', 'libs/core/CMakeLists.txt'] as const).filter((manifest) => projects.some(({ data }) => `${data.root}/CMakeLists.txt` === manifest && (data.tags ?? []).includes('release')));
                assert.include([...manifests], 'apps/tool/CMakeLists.txt', `[${TAGGED}]`);
                assert.deepStrictEqual(
                    yield* Effect.forEach(manifests, text),
                    manifests.map((manifest) => files[manifest].replace('VERSION 0.1.0', `VERSION ${NEXT}`)),
                    `[${LAYOUT}]`,
                );
            }),
            bounds.test,
        );
    });
});
