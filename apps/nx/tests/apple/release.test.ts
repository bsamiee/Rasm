import { assert, it } from '@effect/vitest';
import { Consumer } from '@rasm/testing/nx/consumer';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { attaching, placed, publishing, released } from '@rasm/testing/nx/projects';
import { Duration, Effect } from 'effect';
import { APPLE, files, settings, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('5 minutes') } as const;
const [PROJECT, MANIFEST, VIEWER] = ['apps/Viewer/Viewer.xcodeproj', 'apps/Viewer/Viewer.xcodeproj/project.pbxproj', 'apps/Viewer'] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-apple release of an Xcode project stating a marketing version', (methods) => {
    const NEXT = '0.2.0';

    methods.effect(
        'gives the Xcode project the release tag',
        Effect.fnUntraced(function* () {
            const { tagged } = yield* placed;
            assert.include(tagged, VIEWER, `[${README.tagged}; ${APPLE.settings} MARKETING_VERSION]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives the Xcode project an attach target waiting on the target that packages its files',
        Effect.fnUntraced(function* () {
            const { folders, loose } = yield* attaching;
            assert.include(folders, VIEWER, `[${README.attached}]`);
            assert.deepStrictEqual(loose, [], `[${README.attached}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'reads the current version xcodebuild resolves for the project',
        Effect.fnUntraced(function* () {
            const [{ version }, shown] = yield* Effect.all([settings(PROJECT, 'Viewer', 'Release'), released('version', NEXT, '--dry-run')]);
            assert.strictEqual(shown.code, 0, shown.output);
            assert.include(shown.output, `Resolved the current version as ${version} from manifest: ${MANIFEST}`, `[${NX.disk}; ${APPLE.settings} MARKETING_VERSION]`);
        }),
        bounds.test,
    );

    methods.effect(
        'writes the new version where xcodebuild resolves it, for every configuration',
        Effect.fnUntraced(function* () {
            const versioned = yield* released('version', NEXT);
            assert.strictEqual(versioned.code, 0, versioned.output);
            const resolved = yield* Effect.forEach(['Debug', 'Release'], (configuration) => settings(PROJECT, 'Viewer', configuration));
            assert.deepStrictEqual(
                resolved.map(({ version }) => version),
                [NEXT, NEXT],
                `[${NX.written}; ${APPLE.settings} MARKETING_VERSION]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'changes nothing in the project file but the version text',
        Effect.fnUntraced(function* () {
            const { text } = yield* Consumer;
            assert.strictEqual(yield* text(MANIFEST), files[MANIFEST].replaceAll('MARKETING_VERSION = 0.1.0;', `MARKETING_VERSION = ${NEXT};`), `[${NX.layout}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'plans a publish target for the Xcode project',
        Effect.fnUntraced(function* () {
            const { folders } = yield* publishing;
            assert.include(folders, VIEWER, `[${NX.publish}; README.md DELIVERY "a .dmg gets a Sparkle feed in R2"]`);
        }),
        bounds.test,
    );
});
