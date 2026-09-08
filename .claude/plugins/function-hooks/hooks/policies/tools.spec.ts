import type { ToolCallInput } from 'claude-code';
import { describe, expect, it } from 'vitest';
import { describeLine, type Facts, toolOnce, toolRecords, toolRule } from './tools.ts';

const _ID = 'call';
const _FACTS: Facts = { seen: new Set(), snapshots: new Set(), dns: new Set(), prompt: 'restore the machine' };

const _fetch = (url: string): ToolCallInput => ({ tool: 'WebFetch', ['tool_use_id']: _ID, url, prompt: 'read the page' });

const _find: ToolCallInput = {
    tool: 'mcp__ast-grep__find_code',
    ['tool_use_id']: _ID,
    pattern: 'const $A = $B',
    language: 'tsx',
    ['project_folder']: '/abs',
};

const _reason = (e: ToolCallInput, facts = _FACTS): string => {
    const decision = toolRule(facts)(e);
    return decision.kind === 'deny' ? decision.reason : '';
};

describe('toolRule', () => {
    it('refuses the web tools and routes WebFetch by host', () => {
        expect(_reason({ tool: 'WebSearch', ['tool_use_id']: _ID, query: 'x' })).toContain('mcp__exa__web_search_exa');
        expect(_reason(_fetch('https://github.com/a/b'))).toContain('mcp__github__get_file_contents');
        expect(_reason(_fetch('https://example.com'))).toContain('tvly extract');
        expect(_reason(_fetch('not a url'))).toContain('tvly extract');
    });

    it('holds a destructive hostinger family to its recorded fact and a purchase to the prompt', () => {
        const restore: ToolCallInput = { tool: 'mcp__hostinger__VPS_restoreSnapshotV1', ['tool_use_id']: _ID, virtualMachineId: 7 };
        expect(_reason(restore)).toContain('VPS_createSnapshotV1');
        expect(_reason(restore, { ..._FACTS, snapshots: new Set(['7']) })).toBe('');
        expect(toolRecords({ tool: 'mcp__hostinger__VPS_createSnapshotV1', ['tool_use_id']: _ID, virtualMachineId: 7 })).toStrictEqual({
            namespace: 'snapshot',
            id: '7',
        });
        const order: ToolCallInput = {
            tool: 'mcp__hostinger__billing_createPurchaseOrderV1',
            ['tool_use_id']: _ID,
            items: [{ ['item_id']: 'kvm2' }],
        };
        expect(_reason(order)).toContain('names no target');
        expect(_reason(order, { ..._FACTS, prompt: 'buy kvm2' })).toBe('');
    });

    it('passes a server tool with its skill line once and none the session has seen', () => {
        expect(toolRule(_FACTS)(_find)).toStrictEqual({ kind: 'rewrite', e: _find, context: ['Load the ast-grep skill for the ast-grep tools'] });
        expect(toolOnce(_find).map((line) => line.key)).toStrictEqual(['ast-grep']);
        expect(toolRule({ ..._FACTS, seen: new Set(['ast-grep']) })(_find)).toStrictEqual({ kind: 'rewrite', e: _find, context: [] });
    });
});

describe('describeLine', () => {
    it('prepends the skill line, then the tool row, and nothing for an unlisted tool', () => {
        expect(describeLine('mcp__ast-grep__find_code')?.split('\n')[0]).toBe('Load the ast-grep skill before the first call');
        expect(describeLine('WebSearch')).toContain('WebSearch is refused');
        expect(describeLine('Bash')).toBeUndefined();
    });
});
