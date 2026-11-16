import { createNodes } from '@rasm/nx-uv';
import { minimatch } from 'minimatch';
import { describe, expect, it } from 'vitest';

describe('@rasm/nx-uv - createNodes', () => {
    describe('config file glob', () => {
        const [glob] = createNodes;
        const matches = (path: string) => minimatch(path, glob);

        it.each(['uv.lock', 'nested/proj/uv.lock', 'pyproject.toml', 'libs/core/pyproject.toml', 'uv.toml', '.python-version', 'apps/api/.python-version', 'ty.toml', 'mypy.ini', '.mypy.ini', 'setup.cfg', 'pytest.toml', '.pytest.toml', 'pytest.ini', '.pytest.ini', 'tox.ini', 'pixi.toml'])('should match %s', (path) => {
            expect(matches(path)).toBe(true);
        });

        it.each(['apps/api/main.py', 'package.json', 'apps/api/requirements.txt', 'pylock.toml', 'pixi.lock'])('should not match %s', (path) => {
            expect(matches(path)).toBe(false);
        });
    });
});
