import { describe, expect, it } from 'vitest';
import { BINLOG_DENY, OP_LINE, type PathEvent, pathOnce, pathRule } from './paths.ts';

const _ID = 'call';

const _read = (path: string): PathEvent => ({ tool: 'Read', ['tool_use_id']: _ID, ['file_path']: path });

const _edit = (path: string, text: string, old = ''): PathEvent => ({
    tool: 'Edit',
    ['tool_use_id']: _ID,
    ['file_path']: path,
    ['old_string']: old,
    ['new_string']: text,
});

const _context = (e: PathEvent, seen: readonly string[] = []): readonly string[] => {
    const decision = pathRule(new Set(seen))(e);
    return decision.kind === 'rewrite' ? decision.context : [];
};

describe('pathRule', () => {
    it('refuses a binary read and names an added secret reference outside .claude', () => {
        expect(pathRule(new Set())(_read('a.binlog'))).toStrictEqual({ kind: 'deny', reason: BINLOG_DENY });
        expect(_context(_edit('x.md', 'op://a'))).toStrictEqual([OP_LINE]);
        expect(_context(_edit('x.md', 'op://a', 'op://a'))).toStrictEqual([]);
        expect(_context(_edit('.claude/x.md', 'op://a'))).toStrictEqual(['Search the docs with mcp__claudeCodeDocs__search_claude_code_docs']);
    });

    it('adds a skill line once per key and none the session has seen', () => {
        expect(pathOnce(_read('libs/dotnet/a.cs')).map((line) => line.key)).toStrictEqual(['dotnet-roslyn-codelens', 'dotnet-coding']);
        expect(_context(_read('libs/dotnet/a.cs'), ['dotnet-coding'])).toStrictEqual(['Load the dotnet-roslyn-codelens skill']);
        expect(_context(_read('infra/x.ts'))).toStrictEqual(['Load the manage-repo skill', 'Load the pulumi skill']);
    });

    it('names the added and dropped rows of a dependency manifest edit', () => {
        const lines = _context(_edit('package.json', '"a": "catalog:"', '"b": "^1.0.0"'));
        expect(lines[0]).toBe('Record a in the owning README.md dependency list');
        expect(lines[1]).toContain('Dropped b from package.json');
        expect(_context(_edit('package.json', '"a": "^1.0.0"'))).toContain('The lock file alone pins versions, spell the row unpinned');
    });
});
