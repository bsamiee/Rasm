import { execFileSync } from 'node:child_process';
import { chmodSync, existsSync } from 'node:fs';
import { delimiter, join } from 'node:path';
import { env } from 'node:process';
import type { ExecutorContext, NxJsonConfiguration } from '@nx/devkit';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const attach = await import('@rasm/nx-distribution/executors/attach');
const publish = await import('@rasm/nx-distribution/executors/publish');
const releasePublish = await import('@rasm/nx-distribution/executors/release-publish');
const upstream = [{ dependentTasksOutputFiles: '**/*', transitive: true }];

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('log', { auto: true }, ({}, { onCleanup }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        onCleanup(() => log.mockRestore());
        return log;
    })
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('release-publish');
        onCleanup(() => {
            vi.unstubAllEnvs();
            tempFs.cleanup();
        });
        tempFs.createFilesSync({
            '.gitignore': '.artifacts\n*.txt\n',
            'lib/index.js': 'export const lib = 1;\n',
            'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nconst [file, ...argv] = process.argv.slice(2);\nwriteFileSync(file, argv.join('\\n'));\n",
            'bin/gh': '#!/bin/sh\nprintf \'%s\\n\' "$@" > "$(dirname "$0")/../gh.txt"\n',
            '.artifacts/distribution/lib/lib.tgz': 'archive',
        });
        chmodSync(join(tempFs.tempDir, 'bin/gh'), 0o755);
        vi.stubEnv('PATH', [join(tempFs.tempDir, 'bin'), env['PATH']].join(delimiter));
        vi.stubEnv('GIT_CONFIG_GLOBAL', '/dev/null');
        vi.stubEnv('GIT_CONFIG_NOSYSTEM', '1');
        vi.stubEnv('GIT_AUTHOR_NAME', 'Suite');
        vi.stubEnv('GIT_AUTHOR_EMAIL', 'suite@example.com');
        vi.stubEnv('GIT_COMMITTER_NAME', 'Suite');
        vi.stubEnv('GIT_COMMITTER_EMAIL', 'suite@example.com');
        vi.stubEnv('NX_DRY_RUN', '');
        vi.stubEnv('LIB_SECRET', 'token; touch NX_PWNED');
        return tempFs;
    })
    .extend('git', { auto: true }, ({ tempFs }) => {
        const git = (...args: readonly string[]) => execFileSync('git', args, { cwd: tempFs.tempDir, encoding: 'utf-8' });
        git('init', '--quiet', '--initial-branch=main');
        git('add', '.');
        git('commit', '--quiet', '--message', 'base');
        return git;
    })
    .extend(
        'recorded',
        ({ tempFs }) =>
            (name: string) =>
                tempFs
                    .readFile(name)
                    .then((text) => text.trim().split('\n'))
                    .catch((): string[] => []),
    )
    .extend('extension', async ({ tempFs }) => {
        await tempFs.createFile('lib/blender_manifest.toml', 'schema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\n');
        execFileSync('zip', ['--quiet', '../.artifacts/distribution/lib/marker_tools-1.2.3.zip', 'blender_manifest.toml'], { cwd: join(tempFs.tempDir, 'lib') });
        return join(tempFs.tempDir, '.artifacts/distribution/lib/marker_tools-1.2.3.zip');
    })
    .extend('context', ({ tempFs }): ExecutorContext => ({
        root: tempFs.tempDir,
        cwd: tempFs.tempDir,
        isVerbose: false,
        projectName: 'lib',
        targetName: 'publish',
        nxJsonConfiguration: { release: { projects: ['lib'], projectsRelationship: 'independent' } },
        projectsConfigurations: { version: 2, projects: { lib: { root: 'lib' } } },
        projectGraph: {
            nodes: {
                lib: {
                    name: 'lib',
                    type: 'lib',
                    data: {
                        root: 'lib',
                        metadata: { technologies: ['blender'] },
                        targets: {
                            publish: { executor: 'nx:noop', inputs: upstream, dependsOn: ['pack'] },
                            attach: { executor: 'nx:noop', inputs: upstream, dependsOn: ['pack'] },
                            'nx-release-publish': { executor: 'nx:noop', inputs: upstream, dependsOn: ['pack'] },
                            alone: { executor: 'nx:noop', dependsOn: ['pack'] },
                            bare: { executor: 'nx:noop', inputs: upstream, dependsOn: ['hollow'] },
                            pack: { executor: 'nx:noop', outputs: ['{workspaceRoot}/.artifacts/distribution/lib'] },
                            hollow: { executor: 'nx:noop', outputs: ['{projectRoot}/absent'] },
                        },
                    },
                },
            },
            dependencies: {},
        },
    }));

describe('release-publish executor', () => {
    it('skips publishing when HEAD holds no tag of the project, as nx release resolved no new version for it', async ({ context, extension, log }) => {
        const options = { credential: 'LIB_TOKEN', nxReleaseVersionData: { lib: { currentVersion: '1.2.3', newVersion: null } } };

        await expect(releasePublish.execute(options, { ...context, targetName: 'nx-release-publish' })).resolves.toStrictEqual({ success: true });
        expect(extension).toMatch(/\.zip$/u);
        expect(log).toHaveBeenCalledWith(expect.anything(), 'Skipped lib: HEAD holds no tag of lib, so nx release released no version of it at this commit');
    });

    it('skips each package with success, naming the credential variable, where the variable is unset', async ({ context, extension, git, log }) => {
        git('tag', '--annotate', 'lib@1.2.3', '--message', 'lib@1.2.3');

        await expect(releasePublish.execute({ credential: 'LIB_TOKEN' }, { ...context, targetName: 'nx-release-publish' })).resolves.toStrictEqual({ success: true });
        expect(log).toHaveBeenCalledWith(expect.anything(), `Skipped ${extension} for https://extensions.blender.org: LIB_TOKEN is not set`);
    });

    it('previews each package and publishes nothing under a dry run', async ({ context, extension, log }) => {
        await expect(releasePublish.execute({ credential: 'LIB_TOKEN', dryRun: true }, { ...context, targetName: 'nx-release-publish' })).resolves.toStrictEqual({ success: true });
        expect(log).toHaveBeenCalledWith(expect.anything(), `Would publish ${extension} to https://extensions.blender.org through POST /api/v1/extensions/<id>/versions/upload/`);
    });
});

describe('publish executor', () => {
    const options = { publish: ['node', 'tools/record.js', 'published.txt', 'push'], preview: ['--dry-run'], credential: 'LIB_TOKEN', flag: '--api-key' } as const;

    it('skips publishing when HEAD holds no tag of the project', async ({ context, recorded }) => {
        await expect(publish.execute({ ...options, credential: 'LIB_SECRET' }, context)).resolves.toStrictEqual({ success: true });
        await expect(recorded('published.txt')).resolves.toStrictEqual([]);
    });

    it('publishes nothing, naming the credential variable, where the variable is unset', async ({ context, git, log, recorded }) => {
        git('tag', '--annotate', 'lib@1.2.3', '--message', 'lib@1.2.3');

        await expect(publish.execute(options, context)).resolves.toStrictEqual({ success: true });
        expect(log).toHaveBeenCalledWith(expect.anything(), 'Skipped node tools/record.js published.txt push .artifacts/distribution/lib/lib.tgz: LIB_TOKEN is not set');
        await expect(recorded('published.txt')).resolves.toStrictEqual([]);
    });

    it('passes each file the packaging target wrote under a dot folder, then the credential under its flag, as single argv elements', async ({ tempFs, context, git, recorded }) => {
        git('tag', '--annotate', 'lib@1.2.3', '--message', 'lib@1.2.3');

        await expect(publish.execute({ ...options, credential: 'LIB_SECRET' }, context)).resolves.toStrictEqual({ success: true });
        await expect(recorded('published.txt')).resolves.toStrictEqual(['push', '.artifacts/distribution/lib/lib.tgz', '--api-key', 'token; touch NX_PWNED']);
        expect(existsSync(join(tempFs.tempDir, 'NX_PWNED'))).toBe(false);
    });

    it("runs the tool's own dry run after the files and publishes nothing under a dry run", async ({ context, recorded }) => {
        await expect(publish.execute({ ...options, dryRun: true }, context)).resolves.toStrictEqual({ success: true });
        await expect(recorded('published.txt')).resolves.toStrictEqual(['push', '.artifacts/distribution/lib/lib.tgz', '--dry-run']);
    });

    it('passes no file where the target states no dependentTasksOutputFiles input', async ({ context, git, recorded }) => {
        git('tag', '--annotate', 'lib@1.2.3', '--message', 'lib@1.2.3');

        await expect(publish.execute({ publish: options.publish, credential: 'LIB_SECRET' }, { ...context, targetName: 'alone' })).resolves.toStrictEqual({ success: true });
        await expect(recorded('published.txt')).resolves.toStrictEqual(['push']);
    });
});

describe('attach executor', () => {
    it('uploads nothing where HEAD holds only the tag of another project', async ({ context, git, log, recorded }) => {
        git('tag', '--annotate', 'other@2.0.0', '--message', 'other@2.0.0');

        await expect(attach.execute({}, { ...context, targetName: 'attach' })).resolves.toStrictEqual({ success: true });
        expect(log).toHaveBeenCalledWith(expect.anything(), 'Skipped lib: HEAD holds no tag of lib, so nx release released no version of it at this commit');
        await expect(recorded('gh.txt')).resolves.toStrictEqual([]);
    });

    it('uploads each file the packaging target wrote to the release of the project tag', async ({ context, git, recorded }) => {
        git('tag', '--annotate', 'other@2.0.0', '--message', 'other@2.0.0');
        git('tag', '--annotate', 'lib@1.2.4', '--message', 'lib@1.2.4');

        await expect(attach.execute({}, { ...context, targetName: 'attach' })).resolves.toStrictEqual({ success: true });
        await expect(recorded('gh.txt')).resolves.toStrictEqual(['release', 'upload', 'lib@1.2.4', '.artifacts/distribution/lib/lib.tgz', '--clobber']);
    });

    it('uploads to the tag a fixed release group pattern makes', async ({ context, git, recorded }) => {
        git('tag', '--annotate', 'v1.2.4', '--message', 'v1.2.4');

        await expect(attach.execute({}, { ...context, targetName: 'attach', nxJsonConfiguration: { release: { projects: ['lib'], releaseTag: { pattern: 'v{version}' } } } })).resolves.toStrictEqual({ success: true });
        await expect(recorded('gh.txt')).resolves.toStrictEqual(['release', 'upload', 'v1.2.4', '.artifacts/distribution/lib/lib.tgz', '--clobber']);
    });

    it('reads a prerelease tag as release.releaseTag states, skipping it under the default strictPreid', async ({ context, git, log, recorded }) => {
        git('tag', '--annotate', 'lib@1.2.4-alpha.0', '--message', 'lib@1.2.4-alpha.0');
        const stated = (releaseTag: NonNullable<NonNullable<NxJsonConfiguration['release']>['releaseTag']>) => ({ ...context, targetName: 'attach', nxJsonConfiguration: { release: { projects: ['lib'], projectsRelationship: 'independent' as const, releaseTag } } });

        await expect(attach.execute({}, stated({}))).resolves.toStrictEqual({ success: true });
        expect(log).toHaveBeenCalledWith(expect.anything(), 'Skipped lib: HEAD holds no tag of lib, so nx release released no version of it at this commit');
        await expect(recorded('gh.txt')).resolves.toStrictEqual([]);
        await expect(attach.execute({}, stated({ strictPreid: false }))).resolves.toStrictEqual({ success: true });
        await expect(recorded('gh.txt')).resolves.toStrictEqual(['release', 'upload', 'lib@1.2.4-alpha.0', '.artifacts/distribution/lib/lib.tgz', '--clobber']);
    });

    it('previews the upload and runs gh for none under a dry run', async ({ context, log, recorded }) => {
        await expect(attach.execute({ dryRun: true }, { ...context, targetName: 'attach' })).resolves.toStrictEqual({ success: true });
        expect(log).toHaveBeenCalledWith(expect.anything(), "Would run gh release upload '<tag nx release makes for lib>' .artifacts/distribution/lib/lib.tgz --clobber");
        await expect(recorded('gh.txt')).resolves.toStrictEqual([]);
    });

    it('fails where its packaging target wrote no file, naming the declared output', async ({ context, git, recorded }) => {
        git('tag', '--annotate', 'lib@1.2.4', '--message', 'lib@1.2.4');

        await expect(attach.execute({}, { ...context, targetName: 'bare' })).rejects.toThrow('lib holds no file its dependentTasksOutputFiles input of bare matches under [lib/absent]');
        await expect(recorded('gh.txt')).resolves.toStrictEqual([]);
    });
});
