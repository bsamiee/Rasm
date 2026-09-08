import { describe, expect, it } from 'vitest';
import { AGENTS, briefed } from './agents.ts';

describe('briefed', () => {
    it('appends the brief of a listed type after a blank line and leaves an unlisted type as given', () => {
        expect(briefed('fork', 'task')).toBe(['task', '', ...(AGENTS.fork ?? [])].join('\n'));
        expect(briefed('Explore', 'task')).toBe('task');
    });
});
