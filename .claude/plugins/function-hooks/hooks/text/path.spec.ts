// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { basename, extension, relative, under } from './path.ts';

// --- [TESTS] ---------------------------------------------------------------------------

describe('path', () => {
    it('reads the last segment and its extension', () => {
        expect(basename('/repo/tools/nx/workspace.ts')).toBe('workspace.ts');
        expect(basename('git')).toBe('git');
        expect(extension('/repo/NuGet.config')).toBe('.config');
        expect(extension('/repo/a/b.spec.ts')).toBe('.ts');
        expect(extension('Makefile')).toBe('.Makefile');
    });

    it('tests a directory prefix on relative and absolute paths', () => {
        expect(under('tools/nx/workspace.ts', 'tools/nx')).toBe(true);
        expect(under('/repo/tools/nx/workspace.ts', 'tools/nx')).toBe(true);
        expect(under('/repo/tools/nxt/workspace.ts', 'tools/nx')).toBe(false);
    });

    it('strips the working directory prefix and keeps a path outside it', () => {
        expect(relative('/repo', '/repo/nx.json')).toBe('nx.json');
        expect(relative('/repo', '/repository/nx.json')).toBe('/repository/nx.json');
        expect(relative('/repo', 'nx.json')).toBe('nx.json');
    });
});
