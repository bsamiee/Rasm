import { assert } from '@effect/vitest';
import { createNodes } from '@rasm/nx-storybook';
import manifest from '@rasm/nx-storybook/package.json' with { type: 'json' };
import framework from '@storybook/react-vite/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STORY = "export default { title: 'Sheet' };\n\nexport const Plain = {};\n";
const main = (addons: string) => `export default { framework: '${framework.name}', stories: ['../*.stories.ts'], addons: [${addons}] };\n`;

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['storybook', framework.name, manifest.name],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["signalled", "silent"] }\n',
        'signalled/package.json': '{ "name": "signalled", "type": "module" }\n',
        'signalled/.storybook/main.ts': main("'@rasm/nx-storybook/preset'"),
        'signalled/sheet.stories.ts': STORY,
        'silent/package.json': '{ "name": "silent", "type": "module" }\n',
        'silent/.storybook/main.ts': main(''),
        'silent/sheet.stories.ts': STORY,
    },
    broken: ['silent/.storybook/main.ts', 'export default { framework:\n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('builds a Storybook whose config is main.js', () =>
        inferred({ ...subject, files: { ...subject.files, 'package.json': '{ "name": "root", "workspaces": ["signalled", "silent", "plain"] }\n', 'plain/package.json': '{ "name": "plain", "type": "module" }\n', 'plain/.storybook/main.js': main(''), 'plain/sheet.stories.ts': STORY } }).pipe(
            Effect.map(running('storybook build')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((built) => assert.include(built, 'plain', 'nrwl/nx@2a424d3 packages/storybook/src/plugins/plugin.spec.ts "should support main.js"')),
        ),
    );

    it.effect('builds each Storybook with storybook build, cached, writing storybook-static', () =>
        inferred(subject).pipe(
            Effect.map(running('storybook build')),
            Effect.map(Array.map(({ project, cache, outputs }) => ({ project, cache, outputs }))),
            Effect.map((built) =>
                assert.deepStrictEqual(built, [
                    { project: 'signalled', cache: true, outputs: ['signalled/storybook-static'] },
                    { project: 'silent', cache: true, outputs: ['silent/storybook-static'] },
                ]),
            ),
        ),
    );

    it.effect('serves the Storybook that loads the readiness preset as an uncached continuous task ready on its signal, and no Storybook that loads none', () =>
        inferred(subject).pipe(
            Effect.map(running('storybook dev')),
            Effect.map(Array.map(({ project, continuous, cache, readyWhen }) => ({ project, continuous, cache, readyWhen }))),
            Effect.map((served) => assert.deepStrictEqual(served, [{ project: 'signalled', continuous: true, cache: false, readyWhen: { signal: true } }])),
        ),
    );
});
