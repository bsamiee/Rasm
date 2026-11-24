import { execFileSync } from 'node:child_process';
import { execute } from '@rasm/nx-apple/executors/release-publish';
import { outputs, it as workspaced } from '@rasm/testing/nx';
import { getLatestGitTagForPattern } from 'nx/src/command-line/release/utils/git';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATTERN = '{projectName}-v{version}';
const TAGS = ['Other-v1.0.0', 'Relay-v1.0.0'];
const relay = {
    '.gitignore': 'build\n',
    'nx.json': JSON.stringify({ release: { groups: { apps: { projects: ['Relay'], projectsRelationship: 'independent', releaseTag: { pattern: PATTERN } } } } }),
    'Relay/project.json': JSON.stringify({
        name: 'Relay',
        targets: {
            'pack--ditto': { executor: 'nx:noop', outputs: ['{projectRoot}/build/Relay/*.dmg', '{projectRoot}/build/Relay/*.zip', '{projectRoot}/build/Relay/Relay.xcarchive'] },
            'nx-release-publish': { executor: '@rasm/nx-apple:release-publish', dependsOn: ['pack--ditto'], inputs: [{ dependentTasksOutputFiles: '**/*' }] },
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
    .extend('tagged', ({ workspace }) => (files: Readonly<Record<string, string>>, tags: readonly string[]) => {
        const git = (...args: readonly string[]) => execFileSync('git', ['-c', 'user.name=Suite', '-c', 'user.email=suite@example.com', '-c', 'commit.gpgsign=false', '-c', 'tag.gpgsign=false', ...args], { cwd: workspace.root });
        workspace.write(files);
        git('init', '--quiet', '--initial-branch=main');
        git('add', '.');
        git('commit', '--quiet', '--message', 'base');
        tags.forEach((tag) => {
            git('tag', '--annotate', tag, '--message', tag);
        });
        return workspace.graph({});
    });

describe('release-publish', () => {
    it('previews the feed under the tag the release group pattern makes for the project at HEAD', async ({ workspace, tagged, log, onTestFinished }) => {
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        vi.stubEnv('SPARKLE_ED_PRIVATE_KEY', 'key');
        vi.stubEnv('SPARKLE_FEED_BUCKET', 'feeds');
        const graph = await tagged({ ...relay, 'Relay/build/Relay/Relay.dmg': 'image' }, TAGS);
        const expected = await getLatestGitTagForPattern(PATTERN, { projectName: 'Relay', releaseGroupName: 'apps' }, () => Promise.resolve(TAGS), { checkAllBranchesWhen: false, requireSemver: true, strictPreid: false });

        await execute({ exports: [{ archive: 'Relay/build/Relay/Relay.xcarchive' }], dryRun: true }, workspace.context(graph, 'Relay', 'nx-release-publish'));

        expect(log).toHaveBeenCalledWith(expect.anything(), `Would publish the Sparkle feed of Relay.dmg for ${expected?.tag} through generate_appcast and cf r2 objects put`);
    });

    it('skips the feed of an archive with a zip and no disk image beside it, as pack--ditto writes without notary credentials', async ({ workspace, tagged, log, onTestFinished }) => {
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        vi.stubEnv('SPARKLE_ED_PRIVATE_KEY', 'key');
        vi.stubEnv('SPARKLE_FEED_BUCKET', 'feeds');
        const graph = await tagged({ ...relay, 'Relay/build/Relay/Relay.zip': 'archive' }, TAGS);

        const { success } = await execute({ exports: [{ archive: 'Relay/build/Relay/Relay.xcarchive' }] }, workspace.context(graph, 'Relay', 'nx-release-publish'));

        expect({ success, logged: log.mock.calls.map((call) => String(call.at(-1))) }).toStrictEqual({ success: true, logged: [expect.stringMatching(/^Skipped the Sparkle feed of Relay\/build\/Relay\/Relay\.xcarchive: no disk image sits beside it/u)] });
    });

    it('refuses where pack--ditto wrote nothing, naming its declared outputs', async ({ workspace, tagged }) => {
        const graph = await tagged(relay, TAGS);

        const published = execute({ exports: [{ archive: 'Relay/build/Relay/Relay.xcarchive' }] }, workspace.context(graph, 'Relay', 'nx-release-publish'));

        await expect(published).rejects.toThrow(`Relay holds no file its dependentTasksOutputFiles input of nx-release-publish matches under [${outputs(graph, 'Relay', 'pack--ditto').join(', ')}]`);
    });
});
