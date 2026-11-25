import type { Tree } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import AppleVersionActions from '@rasm/nx-apple/release/version-actions';
import { assert, asyncProperty, nat, tuple } from 'fast-check';
import { describe, expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFESTS = {
    'Info.plist': `<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>CFBundleName</key>
	<string>Tool</string>
	<key>CFBundleShortVersionString</key>
	<string>1.0.0</string>
</dict>
</plist>
`,
    'Viewer.xcodeproj/project.xcproj': `{
  "default-configuration": "Release",
  "configurations": [
    "Debug",
    "Release",
  ],
  "localizations": {
    "development": "en",
    "supported": [
      "Base",
    ],
  },
  "files": [
    { "kind": "folder", "path": "Sources", "target-membership": [ "Viewer" ] },
  ],
  "targets": [
    {
      "name": "Viewer",
      "id": "A10000000000000000000002",
      "product-type": "application",
      "build-phases": [
        "compile-sources",
      ],
    },
  ],
  "build-settings": {
    "MARKETING_VERSION": "1.0.0",
    "PRODUCT_NAME": "$(TARGET_NAME)",
  },
}
`,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const createVersionActions = async (tree: Tree, manifest: string): Promise<AppleVersionActions> => {
    const releaseGroup: ConstructorParameters<typeof AppleVersionActions>[0] = { name: 'release-group', projects: ['my-lib'], projectsRelationship: 'independent', resolvedVersionPlans: false };
    const finalConfigForProject: ConstructorParameters<typeof AppleVersionActions>[2] = {
        specifierSource: 'prompt',
        currentVersionResolver: 'disk',
        currentVersionResolverMetadata: {},
        fallbackCurrentVersionResolver: 'disk',
        versionPrefix: 'auto',
        preserveLocalDependencyProtocols: false,
        preserveMatchingDependencyRanges: false,
        adjustSemverBumpsForZeroMajorVersion: false,
        applyPreidToDependents: false,
        versionActionsOptions: { manifest },
        manifestRootsToUpdate: [],
        dockerOptions: {},
    };
    const versionActions = new AppleVersionActions(releaseGroup, { name: 'my-lib', type: 'app', data: { root: 'packages/my-lib' } }, finalConfigForProject);
    await versionActions.init(tree);
    return versionActions;
};

describe(AppleVersionActions, () => {
    it.each([
        { name: 'Info.plist', manifest: 'Info.plist', input: MANIFESTS['Info.plist'], stated: '<string>1.0.0</string>' },
        { name: 'project.xcproj', manifest: 'Viewer.xcodeproj/project.xcproj', input: MANIFESTS['Viewer.xcodeproj/project.xcproj'], stated: '"MARKETING_VERSION": "1.0.0"' },
    ])('writes any major.minor.patch version into $name, keeping the rest of its text, and reads that version back', async ({ manifest, input, stated }) => {
        expect.hasAssertions();
        await assert(
            asyncProperty(tuple(nat(), nat(), nat()), async (parts) => {
                const version = parts.join('.');
                const tree = createTreeWithEmptyWorkspace();
                tree.write(`packages/my-lib/${manifest}`, input);
                const versionActions = await createVersionActions(tree, manifest);

                await versionActions.updateProjectVersion(tree, version);

                expect(tree.read(`packages/my-lib/${manifest}`, 'utf-8')).toBe(input.replace(stated, stated.replace('1.0.0', version)));
                await expect(versionActions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: version, manifestPath: `packages/my-lib/${manifest}` });
            }),
            { numRuns: 20 },
        );
    });

    it.each([
        { name: 'Info.plist', manifest: 'Info.plist', input: MANIFESTS['Info.plist'].replace('</dict>\n', '') },
        { name: 'project.xcproj', manifest: 'Viewer.xcodeproj/project.xcproj', input: MANIFESTS['Viewer.xcodeproj/project.xcproj'].slice(0, -3) },
    ])('rejects a malformed $name without changing it', async ({ manifest, input }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(`packages/my-lib/${manifest}`, input);
        const versionActions = await createVersionActions(tree, manifest);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow(`Cannot update packages/my-lib/${manifest}`);
        expect(tree.read(`packages/my-lib/${manifest}`, 'utf-8')).toBe(input);
    });

    it('rejects a version update shadowed by a duplicate key', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const manifest = MANIFESTS['Viewer.xcodeproj/project.xcproj'].replace('"MARKETING_VERSION": "1.0.0",', '"MARKETING_VERSION": "1.0.0",\n    "MARKETING_VERSION": "5.5.5",');
        tree.write('packages/my-lib/Viewer.xcodeproj/project.xcproj', manifest);
        const versionActions = await createVersionActions(tree, 'Viewer.xcodeproj/project.xcproj');

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow(/^Cannot update packages\/my-lib\/Viewer\.xcodeproj\/project\.xcproj: .* differs from the form Xcode writes, so rewriting it would change more than MARKETING_VERSION/su);
        expect(tree.read('packages/my-lib/Viewer.xcodeproj/project.xcproj', 'utf-8')).toBe(manifest);
    });
});
