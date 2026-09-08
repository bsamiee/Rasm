import type { ToolCallInput } from 'claude-code';
import { describe, expect, it } from 'vitest';
import { describeLine, type Facts, toolOnce, toolRecords, toolRule } from './tools.ts';

const _ID = 'call';
const _FACTS: Facts = { seen: new Set(), snapshots: new Set(), dns: new Set(), prompt: 'restore the machine', cwd: '/Users/x/Rasm' };
const _TSX_LINE = 'Ran with language tsx, sgconfig.yml languageGlobs maps every .ts file to tsx and typescript finds nothing';
const _FOLDER_LINE = 'Resolved project_folder against the working directory, the server reads an absolute path';

const _fetch = (url: string): ToolCallInput => ({ tool: 'WebFetch', ['tool_use_id']: _ID, url, prompt: 'read the page' });

const _find = (language: string, folder: string): ToolCallInput => ({
    tool: 'mcp__ast-grep__find_code',
    ['tool_use_id']: _ID,
    pattern: 'const $A = $B',
    language,
    ['project_folder']: folder,
});

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

    it('answers the trusted solution without the server and passes another', () => {
        const trust = (path: string): ToolCallInput => ({ tool: 'mcp__roslyn-codelens__trust_solution', ['tool_use_id']: _ID, path });
        expect(toolRule(_FACTS)(trust('/x/Workspace.slnx')).kind).toBe('answer');
        expect(toolRule(_FACTS)(trust('/x/Other.slnx')).kind).toBe('rewrite');
    });

    it('rewrites the ast-grep search language and folder in table order with the once line first unseen', () => {
        const decision = toolRule(_FACTS)(_find('typescript', 'libs'));
        const found = decision.kind === 'rewrite' && decision.e.tool === 'mcp__ast-grep__find_code' ? decision.e : undefined;
        expect([found?.language, found?.project_folder]).toStrictEqual(['tsx', '/Users/x/Rasm/libs']);
        expect(decision.kind === 'rewrite' ? decision.context : []).toStrictEqual([
            _TSX_LINE,
            _FOLDER_LINE,
            'Load the ast-grep skill for the ast-grep tools',
        ]);
        expect(toolOnce(_find('tsx', '/abs')).map((line) => line.key)).toStrictEqual(['ast-grep']);
        expect(toolRule({ ..._FACTS, seen: new Set(['ast-grep']) })(_find('tsx', '/abs'))).toStrictEqual({
            kind: 'rewrite',
            e: _find('tsx', '/abs'),
            context: [],
        });
    });
});

describe('describeLine', () => {
    it('prepends the skill line, then the tool row, and nothing for an unlisted tool', () => {
        expect(describeLine('mcp__ast-grep__find_code')?.split('\n')[0]).toBe('Load the ast-grep skill before the first call');
        expect(describeLine('WebSearch')).toContain('WebSearch is refused');
        expect(describeLine('Bash')).toBeUndefined();
    });
});
