// --- [IMPORTS] -------------------------------------------------------------------------

import type { PluginOptions } from 'claude-code';
import { describe, expect, it, vi } from 'vitest';
import { OPTIONS, options, whenEnabled } from './options.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// One value per option row in the manifest's own types
const _RAW: PluginOptions = { packageManager: 'pnpm', speak: false, classify: true, dispatch: true };

// --- [TESTS] ---------------------------------------------------------------------------

describe('options', () => {
    it('narrows a record that matches every row and freezes it', () => {
        const narrowed = options(_RAW);
        expect(narrowed).toStrictEqual(_RAW);
        expect(Object.isFrozen(narrowed)).toBe(true);
        expect(Object.keys(narrowed)).toStrictEqual(Object.keys(OPTIONS));
    });

    it('throws on a wrong type and on a missing option', () => {
        expect(() => options({ ..._RAW, dispatch: 'yes' })).toThrow('Plugin options do not match their declared types');
        expect(() => options({ packageManager: 'pnpm' })).toThrow('Plugin options do not match their declared types');
    });
});

describe('whenEnabled', () => {
    it('registers once when the option is on and never when it is off', () => {
        const register = vi.fn();
        whenEnabled(true, register);
        whenEnabled(false, register);
        expect(register).toHaveBeenCalledTimes(1);
    });
});
