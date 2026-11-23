import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { execPath } from 'node:process';
import { createNodes } from '@rasm/nx-distribution';
import { execute as attach } from '@rasm/nx-distribution/executors/attach';
import { Package, execute as pack } from '@rasm/nx-distribution/executors/package';
import { execute as publish, Publish } from '@rasm/nx-distribution/executors/publish';
import { shimmed, targetOptions, it as workspaced } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const rhino = {
    'rhino/manifest.yml': 'name: marker\nversion: 1.2.4\nauthors:\n    - Suite\ndescription: Marks points\n',
    'rhino/marker-1.2.4-rh8-any.yak': 'archive',
};
const suite = (name: string) => JSON.stringify({ targets: { 'pack--playwright': { executor: 'nx:noop', outputs: [`{workspaceRoot}/.artifacts/playwright/${name}`] } } });
const clipper = {
    'package.json': '{ "name": "repo", "private": true, "workspaces": ["clipper", "suites/*"] }',
    'clipper/package.json': '{ "name": "clipper" }',
    'clipper/manifest.json': '{ "manifest_version": 3, "name": "Clipper", "version": "1.2.4" }',
    'clipper/worker.js': 'chrome.runtime.id;\n',
    'suites/clipper/package.json': '{ "name": "test-clipper", "devDependencies": { "clipper": "workspace:*" } }',
    'suites/clipper/project.json': suite('test-clipper'),
    'suites/other/package.json': '{ "name": "test-other" }',
    'suites/other/project.json': suite('test-other'),
    '.artifacts/playwright/test-clipper/media/small-promo-tile.png': 'image',
    '.artifacts/playwright/test-clipper/media/screenshot-copy.png': 'image',
    '.artifacts/playwright/test-clipper/media/.last-run.json': '{}',
    '.artifacts/playwright/test-other/media/screenshot-other.png': 'image',
};
const registry = {
    'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nconst [file, ...argv] = process.argv.slice(2);\nwriteFileSync(file, argv.join('\\n'));\n",
    '.artifacts/distribution/lib/lib-1.0.0.tgz': 'archive',
    'lib/project.json': JSON.stringify({
        name: 'lib',
        targets: {
            pack: { executor: 'nx:noop', outputs: ['{workspaceRoot}/.artifacts/distribution/lib'] },
            'nx-release-publish': { executor: '@rasm/nx-distribution:publish', dependsOn: ['pack'], inputs: [{ dependentTasksOutputFiles: '**/*.tgz' }] },
            alone: { executor: '@rasm/nx-distribution:publish', dependsOn: ['pack'] },
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
    .extend('released', ({ workspace }) => async (files: Readonly<Record<string, string>>, tags: readonly string[], release?: object) => {
        const git = (...args: readonly string[]) => execFileSync('git', ['-c', 'user.name=Suite', '-c', 'user.email=suite@example.com', '-c', 'commit.gpgsign=false', '-c', 'tag.gpgsign=false', ...args], { cwd: workspace.root });
        shimmed(join(workspace.root, '.bin'), { gh: String.raw`printf '%s\n' "$@" > "$(dirname "$0")/../gh.txt"` });
        vi.stubEnv('LIB_SECRET', 'token; touch PWNED');
        vi.stubEnv('BLANK_TOKEN', '');
        workspace.write({ '.gitignore': '.bin\n.artifacts\n*.yak\n*.txt\n', 'nx.json': JSON.stringify({ release: release ?? { projects: ['*'], projectsRelationship: 'independent' } }), ...files });
        git('init', '--quiet', '--initial-branch=main');
        git('add', '.');
        git('commit', '--quiet', '--message', 'base');
        tags.forEach((tag) => {
            git('tag', '--annotate', tag, '--message', tag);
        });
        return { graph: await workspace.graph({ createNodes }), git, recorded: (file: string) => (existsSync(join(workspace.root, file)) ? readFileSync(join(workspace.root, file), 'utf-8').trim().split('\n') : []) };
    });

describe('attach--gh', () => {
    it.for([
        { case: 'its own tag', tags: ['other@2.0.0', 'marker@1.2.4'], release: undefined, uploaded: ['release', 'upload', 'marker@1.2.4', 'rhino/marker-1.2.4-rh8-any.yak', '--clobber'] },
        { case: 'only the tag of another project', tags: ['other@2.0.0'], release: undefined, uploaded: [] },
        { case: 'the tag a fixed releaseTag.pattern makes', tags: ['v1.2.4'], release: { projects: ['marker'], releaseTag: { pattern: 'v{version}' } }, uploaded: ['release', 'upload', 'v1.2.4', 'rhino/marker-1.2.4-rh8-any.yak', '--clobber'] },
    ])('uploads the packed files where HEAD holds $case: $uploaded', async ({ tags, release, uploaded }, { workspace, released }) => {
        const { graph, recorded } = await released(rhino, tags, release);

        await attach({}, workspace.context(graph, 'marker', 'attach--gh'));

        expect(recorded('gh.txt')).toStrictEqual(uploaded);
    });

    it('uploads the zip an extension with no Nx configuration packs, with the visible files the pack targets of each package requiring it wrote', async ({ workspace, released }) => {
        const { graph, recorded } = await released(clipper, ['clipper@1.2.4']);

        await pack(await targetOptions(Package, graph, 'clipper', 'build--chrome'), workspace.context(graph, 'clipper', 'build--chrome'));
        await attach({}, workspace.context(graph, 'clipper', 'attach--gh'));

        expect(recorded('gh.txt')).toStrictEqual(['release', 'upload', 'clipper@1.2.4', '.artifacts/playwright/test-clipper/media/screenshot-copy.png', '.artifacts/playwright/test-clipper/media/small-promo-tile.png', 'clipper/clipper.zip', '--clobber']);
    });

    it('uploads nothing where the project tag sits on an earlier commit than HEAD', async ({ workspace, released }) => {
        const { graph, git, recorded } = await released(rhino, ['marker@1.2.4']);
        git('commit', '--quiet', '--allow-empty', '--message', 'later');

        await attach({}, workspace.context(graph, 'marker', 'attach--gh'));

        expect(recorded('gh.txt')).toStrictEqual([]);
    });

    it('previews the upload with the words of the run but the dry-run flag the executor reads itself, and runs no gh', async ({ workspace, released, log }) => {
        const { graph, recorded } = await released(rhino, []);

        await attach({ dryRun: true, __unparsed__: ['--dry-run=true', '--repo', 'owner/marker'] }, workspace.context(graph, 'marker', 'attach--gh'));

        expect(log).toHaveBeenCalledWith(expect.anything(), expect.stringMatching(/^Would run gh release upload .+ rhino\/marker-1\.2\.4-rh8-any\.yak --clobber --repo owner\/marker$/u));
        expect(recorded('gh.txt')).toStrictEqual([]);
    });

    it('refuses where the build it depends on wrote no file, naming the declared output', async ({ workspace, released }) => {
        const { graph } = await released({ 'rhino/manifest.yml': rhino['rhino/manifest.yml'] }, ['marker@1.2.4']);

        await expect(attach({}, workspace.context(graph, 'marker', 'attach--gh'))).rejects.toThrow('marker holds no file its dependentTasksOutputFiles input of attach--gh matches under [rhino/*.yak]');
    });
});

describe('publish through a registry tool', () => {
    const tool = [execPath, 'tools/record.js', 'published.txt', 'push'] as const;

    it.for([
        { case: 'HEAD holds no project tag', tags: [], options: { credential: 'LIB_SECRET' }, target: 'nx-release-publish', published: [] },
        { case: 'every credential variable is unset or empty', tags: ['lib@1.0.0'], options: { credential: ['UNSET_TOKEN', 'BLANK_TOKEN'] }, target: 'nx-release-publish', published: [] },
        { case: 'a credential variable is set', tags: ['lib@1.0.0'], options: { credential: ['BLANK_TOKEN', 'LIB_SECRET'], flag: '--api-key' }, target: 'nx-release-publish', published: ['push', '.artifacts/distribution/lib/lib-1.0.0.tgz', '--api-key', 'token; touch PWNED'] },
        { case: 'a dry run', tags: [], options: { credential: 'LIB_SECRET', flag: '--api-key', preview: ['--dry-run'], dryRun: true }, target: 'nx-release-publish', published: ['push', '.artifacts/distribution/lib/lib-1.0.0.tgz', '--dry-run'] },
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
