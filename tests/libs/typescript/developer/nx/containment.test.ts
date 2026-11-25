import { below, inside, tokened, within } from '@rasm/nx';
import { describe, expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLACED = [
    { folder: 'libs/core', file: 'libs/core/index.ts', held: true, partition: 'a file directly inside' },
    { folder: 'libs/core', file: 'libs/core/src/deep/main.ts', held: true, partition: 'a file nested inside' },
    { folder: 'libs/core', file: 'libs/core', held: true, partition: 'the folder itself' },
    { folder: 'libs/core', file: 'libs/coreutils/index.ts', held: false, partition: 'a sibling sharing the name as a prefix' },
    { folder: 'libs/core/src', file: 'libs/core', held: false, partition: 'the parent' },
    { folder: '.', file: 'apps/tool/main.ts', held: true, partition: 'the root as folder' },
    { folder: 'apps/tool', file: '.', held: false, partition: 'the root as file' },
    { folder: '.', file: '.', held: true, partition: 'the root as both' },
    { folder: 'apps/tool', file: 'apps/tool/..cache/out.txt', held: true, partition: 'a name starting with two dots inside the folder' },
    { folder: '..cache', file: '..cache/out', held: true, partition: 'a name starting with two dots as the folder' },
    { folder: '.', file: '..cache/out', held: true, partition: 'a name starting with two dots under the root' },
    { folder: '.', file: '../sibling/file.ts', held: false, partition: 'a path climbing out of the root' },
    { folder: 'apps/tool', file: 'apps/tool/../core/main.ts', held: false, partition: 'a path climbing out through a dot-dot segment' },
    { folder: '/srv/workspace/apps', file: '/srv/workspace/apps/tool/main.ts', held: true, partition: 'an absolute path inside an absolute folder' },
    { folder: '/srv/workspace/apps', file: '/srv/workspace/..apps/main.ts', held: false, partition: 'an absolute path in a two-dot sibling of an absolute folder' },
] as const;
const BELOW = [
    { folder: 'libs/core', file: 'libs/core/index.ts', held: true, partition: 'a file directly below' },
    { folder: 'libs/core', file: 'libs/core/src/deep/main.ts', held: true, partition: 'a file nested below' },
    { folder: 'libs/core', file: 'libs/core', held: false, partition: 'the folder itself' },
    { folder: 'libs/core/', file: 'libs/core', held: false, partition: 'the folder itself spelled with a trailing slash' },
    { folder: 'libs/core', file: 'libs/core-kit/index.ts', held: false, partition: 'a sibling sharing the name as a prefix' },
    { folder: '.', file: 'apps/tool', held: true, partition: 'a folder below the root' },
    { folder: '.', file: '.', held: false, partition: 'the root as both' },
    { folder: 'apps/tool', file: 'apps/tool/..cache', held: true, partition: 'a name starting with two dots below the folder' },
    { folder: '.', file: '../sibling', held: false, partition: 'a path climbing out of the root' },
    { folder: '/srv/workspace/apps', file: '/srv/workspace/apps/tool', held: true, partition: 'an absolute path below an absolute folder' },
] as const;
const WITHIN = [
    { at: 'libs/core/index.ts', held: true, partition: 'a workspace file' },
    { at: '..cache/out.txt', held: true, partition: 'a workspace file under a two-dot folder' },
    { at: '../outside/tsconfig.json', held: false, partition: 'a file above the workspace' },
    { at: 'node_modules/effect/package.json', held: false, partition: 'an installed package file' },
] as const;
const TOKENED = [
    { root: 'libs/core', at: 'libs/core/src/index.ts', token: '{projectRoot}/src/index.ts', partition: 'a file inside the project' },
    { root: 'libs/core', at: 'tsconfig.base.json', token: '{workspaceRoot}/tsconfig.base.json', partition: 'a file outside the project' },
    { root: '.', at: 'libs/core/index.ts', token: '{workspaceRoot}/libs/core/index.ts', partition: 'a file of the root project' },
    { root: 'libs/core', at: 'libs/core/..generated/index.ts', token: '{projectRoot}/..generated/index.ts', partition: 'a file under a two-dot folder of the project' },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('containment', () => {
    it.for(PLACED)('reads $file inside $folder as $held, $partition', ({ folder, file, held }) => {
        expect(inside(folder)(file)).toBe(held);
    });

    it.for(BELOW)('reads $file below $folder as $held, $partition', ({ folder, file, held }) => {
        expect(below(folder)(file)).toBe(held);
    });

    it.for(WITHIN)('reads $at within the workspace as $held, $partition', ({ at, held }) => {
        expect(within(at)).toBe(held);
    });

    it.for(TOKENED)('states $at of project $root as $token, $partition', ({ root, at, token }) => {
        expect(tokened(root)(at)).toBe(token);
    });
});
