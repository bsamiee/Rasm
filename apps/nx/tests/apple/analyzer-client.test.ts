import { chmodSync } from 'node:fs';
import { AggregateCreateNodesError, type CreateNodesContext } from '@nx/devkit';
import { resetWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-apple';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PACKAGE = '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Proj", products: [.library(name: "Lib", targets: ["Lib"])], targets: [.target(name: "Lib")])\n';

// --- [OPERATIONS] ----------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('apple-analyzer');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        await tempFs.createFiles({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n', 'broken/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: \n' });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }))
    .extend('spied', ({ tempFs }, { onCleanup }) => {
        onCleanup(() => {
            vi.unstubAllEnvs();
        });
        return async () => {
            await tempFs.createFiles({
                '.bin/swift': `#!/bin/sh\nprintf '%s\\n' "$*" >> "\${0%/*}/calls.log"\nPATH="\${PATH#*:}"\n[ -e "\${0%/*}/upgraded" ] && [ "$1" = -print-target-info ] && { swift "$@" | sed 's|"target"|"upgraded": true, "target"|'; exit; }\nexec swift "$@"\n`,
                '.bin/calls.log': '',
            });
            chmodSync(`${tempFs.tempDir}/.bin/swift`, 0o755);
            vi.stubEnv('PATH', `${tempFs.tempDir}/.bin:/usr/bin:/bin`);
            return () => tempFs.readFile('.bin/calls.log').then((log) => log.split('\n').filter((line) => line.startsWith('package --package-path ')));
        };
    });

describe('analyzeProjects', { timeout: 120_000 }, () => {
    const [, createNodesFunction] = createNodes;

    it('should return an error result when swift package describe exits non-zero', async ({ context }) => {
        const result = createNodesFunction(['broken/Package.swift', 'proj/Package.swift'], {}, context);

        await expect(result).rejects.toBeInstanceOf(AggregateCreateNodesError);
        await expect(result).rejects.toHaveProperty('errors.length', 1);
        await expect(result).rejects.toHaveProperty(['errors', 0, 0], 'broken/Package.swift');
        await expect(result).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/swift package --package-path broken .*describe --type json exits 1/u));
        await expect(result).rejects.toMatchObject({ partialResults: [['proj/Package.swift', { projects: { proj: { name: 'Proj' }, 'proj/Sources/Lib': { name: 'Lib' } } }]] });
    });

    it('should parse an undecodable project into an attributable error', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'Viewer.xcodeproj/project.xcproj': 'not json at all' });

        await expect(createNodesFunction(['Viewer.xcodeproj/project.xcproj'], {}, context)).rejects.toMatchObject({
            errors: [['Viewer.xcodeproj/project.xcproj', { message: 'Viewer.xcodeproj is undecodable: The data couldn’t be read because it isn’t in the correct format.' }]],
        });
    });

    it('should name the absent tool and read nothing when swift is absent', async ({ tempFs, context, spied }) => {
        await spied();
        vi.stubEnv('PATH', `${tempFs.tempDir}/.absent`);
        const warn = vi.spyOn(console, 'warn').mockReturnValue(undefined);

        await expect(createNodesFunction(['proj/Package.swift'], {}, context)).resolves.toStrictEqual([]);
        expect(warn).toHaveBeenCalledWith('proj/Package.swift presents no project: swift is absent on this host');
        warn.mockRestore();
    });

    it('should pass a package path as a literal argument, never through a shell', async ({ tempFs, context }) => {
        const malicious = 'x; touch pwned';
        await tempFs.createFiles({ [`${malicious}/Package.swift`]: PACKAGE, [`${malicious}/Sources/Lib/Lib.swift`]: 'public let lib = 1\n' });

        const results = await createNodesFunction([`${malicious}/Package.swift`], {}, context);

        expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual([malicious, `${malicious}/Sources/Lib`]);
        await expect(tempFs.readFile('pwned')).rejects.toThrow('ENOENT');
    });

    describe('evaluation inputs', () => {
        it('should reuse a cached result whose inputs are unchanged', async ({ context, spied }) => {
            const first = await createNodesFunction(['proj/Package.swift'], {}, context);
            const calls = await spied();

            await expect(createNodesFunction(['proj/Package.swift'], {}, context)).resolves.toStrictEqual(first);
            await expect(calls()).resolves.toStrictEqual([]);
        });

        it('should rerun swift when a manifest changed', async ({ tempFs, context, spied }) => {
            await createNodesFunction(['proj/Package.swift'], {}, context);
            await tempFs.createFiles({ 'proj/Package.swift': PACKAGE.replace('"Proj"', '"Renamed"') });
            resetWorkspaceContext();
            const calls = await spied();

            await createNodesFunction(['proj/Package.swift'], {}, context);

            await expect(calls()).resolves.toStrictEqual([expect.stringMatching(/^package --package-path proj .*describe --type json$/u)]);
        });

        it('should rerun swift when the toolchain changed', async ({ tempFs, context, spied }) => {
            await createNodesFunction(['proj/Package.swift'], {}, context);
            await tempFs.createFiles({ '.bin/upgraded': '' });
            const calls = await spied();

            await createNodesFunction(['proj/Package.swift'], {}, context);

            await expect(calls()).resolves.toStrictEqual([expect.stringMatching(/^package --package-path proj .*describe --type json$/u)]);
        });

        it('should not cache a run holding a failure', async ({ context, spied }) => {
            await expect(createNodesFunction(['broken/Package.swift', 'proj/Package.swift'], {}, context)).rejects.toBeInstanceOf(AggregateCreateNodesError);
            const calls = await spied();

            await expect(createNodesFunction(['broken/Package.swift', 'proj/Package.swift'], {}, context)).rejects.toBeInstanceOf(AggregateCreateNodesError);

            await expect(calls()).resolves.toHaveLength(2);
            await expect(calls()).resolves.toStrictEqual(expect.arrayContaining([expect.stringMatching(/^package --package-path broken .*describe --type json$/u), expect.stringMatching(/^package --package-path proj .*describe --type json$/u)]));
        });
    });
});
