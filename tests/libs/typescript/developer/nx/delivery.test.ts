import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { execPath } from 'node:process';
import { EXECUTORS } from '@rasm/nx';
import { execute as attach } from '@rasm/nx/executors/attach';
import { execute as publish, Publish } from '@rasm/nx/executors/publish';
import { shimmed, it as workspaced } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { getLatestGitTagForPattern } from 'nx/src/command-line/release/utils/git';
import { RepoGitTags } from 'nx/src/command-line/release/utils/repository-git-tags';
import { assert, describe, expect, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const rhino = {
    'rhino/marker-1.2.4-rh8-any.yak': 'archive',
    'rhino/project.json': JSON.stringify({ name: 'marker', targets: { pack: { executor: 'nx:noop', outputs: ['{projectRoot}/*.yak'] }, attach: { executor: EXECUTORS.attach, dependsOn: ['pack'], inputs: [{ dependentTasksOutputFiles: '**/*' }] } } }),
};
const registry = {
    'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nconst [file, ...argv] = process.argv.slice(2);\nwriteFileSync(file, argv.join('\\n'));\n",
    '.artifacts/lib/lib-1.0.0.tgz': 'archive',
    'lib/project.json': JSON.stringify({
        name: 'lib',
        targets: {
            pack: { executor: 'nx:noop', outputs: ['{workspaceRoot}/.artifacts/lib'] },
            'nx-release-publish': { executor: EXECUTORS.publish, dependsOn: ['pack'], inputs: [{ dependentTasksOutputFiles: '**/*.tgz' }] },
            alone: { executor: EXECUTORS.publish, dependsOn: ['pack'] },
        },
    }),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced
    .extend('log', { auto: true }, ({}, { onCleanup }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        onCleanup(() => log.mockRestore());
        return log;
    })
    .extend('released', ({ workspace }) => async (files: Readonly<Record<string, string>>, tags: readonly string[], release?: object, view = 'exit 0') => {
        const git = (...args: readonly string[]) => execFileSync('git', ['-c', 'user.name=Suite', '-c', 'user.email=suite@example.com', '-c', 'commit.gpgsign=false', '-c', 'tag.gpgsign=false', ...args], { cwd: workspace.root });
        shimmed(join(workspace.root, '.bin'), { gh: String.raw`[ "$1 $2" = 'release view' ] && { ${view}; }; printf '%s\n' "$@" > "$(dirname "$0")/../gh.txt"` });
        vi.stubEnv('LIB_SECRET', 'token; touch PWNED');
        vi.stubEnv('BLANK_TOKEN', '');
        workspace.write({ '.gitignore': '.bin\n.artifacts\n*.yak\n*.txt\n', 'nx.json': JSON.stringify({ release: release ?? { projects: ['*'], projectsRelationship: 'independent' } }), ...files });
        git('init', '--quiet', '--initial-branch=main');
        git('add', '.');
        git('commit', '--quiet', '--message', 'base');
        tags.forEach((tag) => {
            git('tag', '--annotate', tag, '--message', tag);
        });
        return { graph: await workspace.graph({}), git, recorded: (file: string) => (existsSync(join(workspace.root, file)) ? readFileSync(join(workspace.root, file), 'utf-8').trim().split('\n') : []) };
    });

describe('attach executor', () => {
    it.for([
        { case: 'its own tag', tags: ['other@2.0.0', 'marker@1.2.4'], release: undefined, uploaded: ['release', 'upload', 'marker@1.2.4', 'rhino/marker-1.2.4-rh8-any.yak', '--clobber'] },
        { case: 'only the tag of another project', tags: ['other@2.0.0'], release: undefined, uploaded: [] },
        { case: 'the tag a fixed releaseTag.pattern makes', tags: ['v1.2.4'], release: { projects: ['marker'], releaseTag: { pattern: 'v{version}' } }, uploaded: ['release', 'upload', 'v1.2.4', 'rhino/marker-1.2.4-rh8-any.yak', '--clobber'] },
    ])('uploads the packed files where HEAD holds $case: $uploaded', async ({ tags, release, uploaded }, { workspace, released }) => {
        const { graph, recorded } = await released(rhino, tags, release);

        await attach({}, workspace.context(graph, 'marker', 'attach'));

        expect(recorded('gh.txt')).toStrictEqual(uploaded);
    });

    it.for([
        { case: 'two releases tagged oldest first', tags: ['marker@1.2.3', 'marker@1.2.4'] },
        { case: 'two releases tagged newest first', tags: ['marker@1.2.4', 'marker@1.2.3'] },
        { case: 'a prerelease tagged before its release', tags: ['marker@1.2.4-beta.1', 'marker@1.2.4'] },
        { case: 'a release tagged before its prerelease', tags: ['marker@1.2.4', 'marker@1.2.4-beta.1'] },
    ])('uploads to the tag Nx resolves where HEAD holds $case', async ({ tags }, { workspace, released }) => {
        const { graph, recorded } = await released(rhino, tags);
        const resolved = await getLatestGitTagForPattern('{projectName}@{version}', { projectName: 'marker' }, (when) => RepoGitTags.create().resolveTags(when), { requireSemver: true, strictPreid: false });

        await attach({}, workspace.context(graph, 'marker', 'attach'));

        expect(recorded('gh.txt')).toStrictEqual(['release', 'upload', resolved?.tag, 'rhino/marker-1.2.4-rh8-any.yak', '--clobber']);
    });

    it('creates the release of a tag gh finds no release for, with the packed files', async ({ workspace, released }) => {
        const { graph, recorded } = await released(rhino, ['marker@1.2.4'], undefined, "echo 'release not found' >&2; exit 1");

        await attach({}, workspace.context(graph, 'marker', 'attach'));

        expect(recorded('gh.txt')).toStrictEqual(['release', 'create', 'marker@1.2.4', '--verify-tag', 'rhino/marker-1.2.4-rh8-any.yak']);
    });

    it('fails where gh cannot read the release of the tag, and creates and uploads nothing', async ({ workspace, released }) => {
        const { graph, recorded } = await released(rhino, ['marker@1.2.4'], undefined, "echo 'non-200 OK status code: 401 Unauthorized' >&2; exit 1");

        const attached = attach({}, workspace.context(graph, 'marker', 'attach'));

        await expect(attached).rejects.toThrow('gh release view marker@1.2.4 --json tagName exits 1: non-200 OK status code: 401 Unauthorized');
        expect(recorded('gh.txt')).toStrictEqual([]);
    });

    it('uploads nothing where the project tag sits on an earlier commit than HEAD', async ({ workspace, released }) => {
        const { graph, git, recorded } = await released(rhino, ['marker@1.2.4']);
        git('commit', '--quiet', '--allow-empty', '--message', 'later');

        await attach({}, workspace.context(graph, 'marker', 'attach'));

        expect(recorded('gh.txt')).toStrictEqual([]);
    });

    it('previews the upload and the release creation with the words of the run but the dry-run flag the executor reads itself, and runs no gh', async ({ workspace, released, log }) => {
        const { graph, recorded } = await released(rhino, []);

        await attach({ dryRun: true, __unparsed__: ['--dry-run=true', '--repo', 'owner/marker'] }, workspace.context(graph, 'marker', 'attach'));

        expect(log).toHaveBeenCalledWith(expect.anything(), expect.stringMatching(/^Would run gh release upload .+ rhino\/marker-1\.2\.4-rh8-any\.yak --clobber --repo owner\/marker where .+ has a GitHub release, else gh release create .+ --verify-tag rhino\/marker-1\.2\.4-rh8-any\.yak --repo owner\/marker$/u));
        expect(recorded('gh.txt')).toStrictEqual([]);
    });

    it('refuses where the build it depends on wrote no file, naming the declared output', async ({ workspace, released }) => {
        const { graph } = await released({ 'rhino/project.json': rhino['rhino/project.json'] }, ['marker@1.2.4']);

        await expect(attach({}, workspace.context(graph, 'marker', 'attach'))).rejects.toThrow('marker holds no file its dependentTasksOutputFiles input of attach matches under [rhino/*.yak]');
    });
});

describe('publish through a registry tool', () => {
    const tool = [execPath, 'tools/record.js', 'published.txt', 'push'] as const;

    it.for([
        { case: 'HEAD holds no project tag', tags: [], options: { credential: 'LIB_SECRET' }, target: 'nx-release-publish', published: [] },
        { case: 'every credential variable is unset or empty', tags: ['lib@1.0.0'], options: { credential: ['UNSET_TOKEN', 'BLANK_TOKEN'] }, target: 'nx-release-publish', published: [] },
        { case: 'a credential variable is set', tags: ['lib@1.0.0'], options: { credential: ['BLANK_TOKEN', 'LIB_SECRET'], flag: '--api-key' }, target: 'nx-release-publish', published: ['push', '.artifacts/lib/lib-1.0.0.tgz', '--api-key', 'token; touch PWNED'] },
        { case: 'a dry run', tags: [], options: { credential: 'LIB_SECRET', flag: '--api-key', preview: ['--dry-run'], dryRun: true }, target: 'nx-release-publish', published: ['push', '.artifacts/lib/lib-1.0.0.tgz', '--dry-run'] },
        { case: 'a target stating no dependentTasksOutputFiles input', tags: ['lib@1.0.0'], options: { credential: 'LIB_SECRET' }, target: 'alone', published: ['push'] },
    ])('runs the tool with $published where $case', async ({ tags, options, target, published }, { workspace, released }) => {
        const { graph, recorded } = await released(registry, tags);

        const stated: unknown = { publish: [...tool], ...options };
        assert(Schema.is(Publish)(stated), 'the row states publish options');

        await publish(stated, workspace.context(graph, 'lib', target));

        expect(recorded('published.txt')).toStrictEqual(published);
        expect(existsSync(join(workspace.root, 'PWNED'))).toBe(false);
    });

    it('names each unset credential variable where it publishes nothing', async ({ workspace, released, log }) => {
        const { graph } = await released(registry, ['lib@1.0.0']);

        await publish({ publish: [...tool], credential: ['UNSET_TOKEN', 'BLANK_TOKEN'] }, workspace.context(graph, 'lib', 'nx-release-publish'));

        expect(log).toHaveBeenCalledWith(expect.anything(), expect.stringMatching(/: UNSET_TOKEN or BLANK_TOKEN is not set$/u));
    });
});
