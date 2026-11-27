import { execFileSync } from 'node:child_process';
import { createNodes } from '@rasm/nx-distribution';
import { execute, Publish } from '@rasm/nx-distribution/executors/release-publish';
import { it as workspaced } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const extension = { 'tools/blender_manifest.toml': 'schema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\n' };
const clipper = { 'clipper/manifest.json': '{ "manifest_version": 3, "name": "Clipper", "version": "1.2.3", "key": "dGVzdA==" }' };

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced
    .extend('log', { auto: true }, ({}, { onCleanup }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        onCleanup(() => log.mockRestore());
        return log;
    })
    .extend('fetched', { scope: 'file' }, ({}, { onCleanup }) => {
        const [asked, answers] = [new Array<string>(), new Map<string, Response>()];
        vi.stubGlobal('fetch', async (input: string | URL | Request, init?: RequestInit) => {
            const request = new Request(input, init);
            const [said, body] = [`${request.method} ${request.url}`, await request.text()];
            asked.push([said, request.headers.get('authorization') ?? '', said.includes('upload') ? '' : body].join(' ').trim());
            return answers.get(said)?.clone() ?? Response.json({ error: `${said} is unknown` }, { status: 404 });
        });
        onCleanup(() => {
            vi.unstubAllGlobals();
        });
        return { asked, answers };
    })
    .extend('store', ({ fetched: { asked, answers } }) => {
        asked.splice(0);
        answers.clear();
        return { asked: () => [...asked], answer: (said: string, response: Response) => answers.set(said, response) };
    })
    .extend('credentials', { scope: 'file', auto: true }, ({}, { onCleanup }) => {
        vi.stubEnv('BLENDER_EXTENSIONS_TOKEN', 'token-4');
        vi.stubEnv('BLANK_TOKEN', '');
        vi.stubEnv('UNSET_TOKEN', undefined);
        vi.stubEnv('CHROME_WEB_STORE_TOKEN', 'access-9');
        vi.stubEnv('CHROME_WEB_STORE_PUBLISHER', 'publisher-7');
        onCleanup(() => {
            vi.unstubAllEnvs();
        });
    })
    .extend('published', ({ workspace }) => async (files: Readonly<Record<string, string>>, archive: readonly [folder: string, name: string, member: string], tags: readonly string[]) => {
        const git = (...args: readonly string[]) => execFileSync('git', ['-c', 'user.name=Suite', '-c', 'user.email=suite@example.com', '-c', 'commit.gpgsign=false', '-c', 'tag.gpgsign=false', ...args], { cwd: workspace.root });
        workspace.write({ '.gitignore': '*.zip\n', 'nx.json': '{ "release": { "projects": ["*"], "projectsRelationship": "independent" } }', ...files });
        execFileSync('zip', ['--quiet', archive[1], archive[2]], { cwd: `${workspace.root}/${archive[0]}` });
        git('init', '--quiet', '--initial-branch=main');
        git('add', '.');
        git('commit', '--quiet', '--message', 'base');
        tags.forEach((tag) => {
            git('tag', '--annotate', tag, '--message', tag);
        });
        const graph = await workspace.graph({ createNodes });
        const [name = ''] = Object.keys(graph.nodes);
        return (options: Readonly<Record<string, unknown>> = {}) => {
            const stated: unknown = { ...graph.nodes[name]?.data.targets?.['nx-release-publish']?.options, ...options };
            assert(Schema.is(Publish)(stated), 'nx-release-publish states publish options');
            return execute(stated, workspace.context(graph, name, 'nx-release-publish'));
        };
    });

describe('nx-release-publish to the Blender extensions platform', () => {
    it('skips where HEAD holds no tag of the project', async ({ published, store, log }) => {
        const publish = await published(extension, ['tools', 'marker_tools-1.2.3.zip', 'blender_manifest.toml'], []);

        await publish();

        expect(log).toHaveBeenCalledWith('untagged project: marker_tools');
        expect(store.asked()).toStrictEqual([]);
    });

    it.for(['UNSET_TOKEN', 'BLANK_TOKEN'])('skips naming the credential variable where it is %s', async (credential, { published, store, log }) => {
        const publish = await published(extension, ['tools', 'marker_tools-1.2.3.zip', 'blender_manifest.toml'], ['marker_tools@1.2.3']);

        await publish({ credential });

        expect(log).toHaveBeenCalledWith(expect.stringMatching(new RegExp(`^Skipped \\S+marker_tools-1\\.2\\.3\\.zip for https://extensions\\.blender\\.org: ${credential} is not set$`, 'u')));
        expect(store.asked()).toStrictEqual([]);
    });

    it('previews each package and calls no store under a dry run, tagged or not', async ({ published, store, log }) => {
        const publish = await published(extension, ['tools', 'marker_tools-1.2.3.zip', 'blender_manifest.toml'], []);

        await publish({ dryRun: true });

        expect(log).toHaveBeenCalledWith(expect.stringMatching(/^Would publish \S+marker_tools-1\.2\.3\.zip to https:\/\/extensions\.blender\.org/u));
        expect(store.asked()).toStrictEqual([]);
    });

    it.for([
        { listed: '1.2.3', asked: ['GET https://extensions.blender.org/api/v1/extensions/'] },
        { listed: '1.2.2', asked: ['GET https://extensions.blender.org/api/v1/extensions/', 'POST https://extensions.blender.org/api/v1/extensions/marker_tools/versions/upload/ Bearer token-4'] },
    ])('uploads under the token only where the store lists $listed in place of 1.2.3', async ({ listed, asked }, { published, store }) => {
        const publish = await published(extension, ['tools', 'marker_tools-1.2.3.zip', 'blender_manifest.toml'], ['marker_tools@1.2.3']);
        store.answer('GET https://extensions.blender.org/api/v1/extensions/', Response.json({ data: [{ id: 'marker_tools', version: listed }] }));
        store.answer('POST https://extensions.blender.org/api/v1/extensions/marker_tools/versions/upload/', Response.json({}));

        await publish();

        expect(store.asked()).toStrictEqual(asked);
    });

    it('names the first upload by hand where the store holds no such extension', async ({ published, store, log }) => {
        const publish = await published(extension, ['tools', 'marker_tools-1.2.3.zip', 'blender_manifest.toml'], ['marker_tools@1.2.3']);
        store.answer('GET https://extensions.blender.org/api/v1/extensions/', Response.json({ data: [] }));

        await expect(publish()).resolves.toStrictEqual({ success: true });

        expect(log).toHaveBeenCalledWith(expect.stringMatching(/holds no extension marker_tools: its first version uploads by hand at https:\/\/extensions\.blender\.org\/submit\//u));
    });

    it('refuses where the build it depends on wrote no archive, naming the outputs to run first', async ({ workspace, published }) => {
        const publish = await published(extension, ['tools', 'marker_tools-1.2.3.zip', 'blender_manifest.toml'], ['marker_tools@1.2.3']);
        workspace.remove('tools/marker_tools-1.2.3.zip');

        await expect(publish()).rejects.toThrow(/^marker_tools:nx-release-publish unmatched input: \S+, outputs: tools\/\*\.zip, tasks: \S+$/u);
    });
});

describe('nx-release-publish to the Chrome Web Store', () => {
    const [item, media] = ['v2', 'upload/v2'].map((path) => `https://chromewebstore.googleapis.com/${path}/publishers/publisher-7/items/jpignaibiiemhngfjkcpokkamffknabf`);
    const [status, upload, submit] = [`GET ${item}:fetchStatus`, `POST ${media}:upload`, `POST ${item}:publish`].map((said) => `${said} Bearer access-9`);

    it('names the first upload by hand and calls no store where the manifest states no key', async ({ published, store, log }) => {
        const publish = await published({ 'clipper/manifest.json': '{ "manifest_version": 3, "name": "Clipper", "version": "1.2.3" }' }, ['clipper', 'Clipper.zip', 'manifest.json'], ['clipper@1.2.3']);

        await publish();

        expect(log).toHaveBeenCalledWith(expect.stringMatching(/^Skipped \S+Clipper\.zip for https:\/\/chrome\.google\.com\/webstore\/devconsole: .*first version uploads by hand.* key/u));
        expect(store.asked()).toStrictEqual([]);
    });

    it.for([
        { channel: 'publishedItemRevisionStatus', asked: [status] },
        { channel: 'submittedItemRevisionStatus', asked: [status] },
        { channel: 'none', asked: [status, upload, submit] },
    ])('uploads then submits for review under the access token alone where no store channel holds the version, here $channel', async ({ channel, asked }, { published, store }) => {
        const publish = await published(clipper, ['clipper', 'Clipper.zip', 'manifest.json'], ['clipper@1.2.3']);
        store.answer(`GET ${item}:fetchStatus`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf', publishedItemRevisionStatus: { distributionChannels: [{ crxVersion: '1.2.2' }] }, [channel]: { distributionChannels: [{ crxVersion: '1.2.3' }] } }));
        store.answer(`POST ${media}:upload`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf', crxVersion: '1.2.3', uploadState: 'SUCCEEDED' }));
        store.answer(`POST ${item}:publish`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf', state: 'PENDING_REVIEW' }));

        await publish();

        expect(store.asked()).toStrictEqual(asked);
    });

    it('polls the item status while the upload is in progress, then submits it for review', async ({ published, store }) => {
        const publish = await published(clipper, ['clipper', 'Clipper.zip', 'manifest.json'], ['clipper@1.2.3']);
        store.answer(`GET ${item}:fetchStatus`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf', lastAsyncUploadState: 'SUCCEEDED' }));
        store.answer(`POST ${media}:upload`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf', uploadState: 'IN_PROGRESS' }));
        store.answer(`POST ${item}:publish`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf', state: 'PENDING_REVIEW' }));

        await publish();

        expect(store.asked()).toStrictEqual([status, upload, status, submit]);
    });

    it('fails naming the upload state where the store refuses the zip, submitting nothing', async ({ published, store }) => {
        const publish = await published(clipper, ['clipper', 'Clipper.zip', 'manifest.json'], ['clipper@1.2.3']);
        store.answer(`GET ${item}:fetchStatus`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf' }));
        store.answer(`POST ${media}:upload`, Response.json({ itemId: 'jpignaibiiemhngfjkcpokkamffknabf', crxVersion: '1.2.3', uploadState: 'FAILED' }));

        await expect(publish()).rejects.toThrow(/uploads to https:\/\/chrome\.google\.com\/webstore\/devconsole in state FAILED/u);
        expect(store.asked()).not.toContain(submit);
    });
});
