import type { PromptAttachmentResult } from 'claude-code';
import { counted, decoded, map, none, type Option, ok, type Result, some } from '../composition.ts';
import type { Diagnostic } from '../hooks/state.d.ts';
import { basename } from '../policies/invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface CodelensDiagnostics {
    readonly items: readonly { readonly id: string; readonly message: string; readonly file: string; readonly line: number }[];
}
interface AstGrepMatch {
    readonly ruleId: string;
    readonly message: string;
    readonly note: string | null;
    readonly file: string;
    readonly range: { readonly start: { readonly line: number } };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROW = /\n(?= {2}\S+ \[Line \d+:\d+\] )/u;
const _BLOCK = /\n\n(?=[^\n]+:\n {2}\S+ \[Line \d+:\d+\] )/u;
const _CLOSING = /\n*<\/new-diagnostics>$/u;
const _ERROR = /^(?<code>[A-Z]+\d+) (?<file>[^(]+?)(?:\((?<line>\d+),\d+\))?: (?<message>.+?)(?: \[[^\]]*\])?$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ATTACHMENT]

const trimmed = (text: string, edited: readonly string[]): PromptAttachmentResult => {
    const hint = '  ★ ';
    const start = text.indexOf('\n\n') + 2;
    const end = text.search(_CLOSING);
    const headers = new Set(edited.map((path) => `${basename(path)}:`));
    const kept = text
        .slice(start, end)
        .split(_BLOCK)
        .flatMap((block) => {
            const header = block.slice(0, block.indexOf('\n'));
            const rows = block
                .slice(header.length + 1)
                .split(_ROW)
                .filter((row) => !row.startsWith(hint));
            return headers.has(header) && rows.length > 0 ? [[header, ...rows].join('\n')] : [];
        });
    return { text: kept.length === 0 ? null : `${text.slice(0, start)}${kept.join('\n\n')}${text.slice(end)}` };
};

// --- [DECODING]

const fromCodelens = (text: string, file: string): Result<readonly Diagnostic[]> =>
    map(decoded<CodelensDiagnostics>('roslyn-codelens get_diagnostics', ok(text)), ({ items }) => items.filter((item) => item.file === file).map((item): Diagnostic => ({ code: item.id, file: item.file, line: some(item.line), message: item.message, source: 'roslyn-codelens' })));

const fromAstGrep = (text: string): Result<readonly Diagnostic[]> =>
    map(decoded<readonly AstGrepMatch[]>('ast-grep scan', ok(text)), (found) => found.map((match): Diagnostic => ({ code: match.ruleId, file: match.file, line: some(match.range.start.line + 1), message: match.note === null ? match.message : `${match.message}. ${match.note}`, source: 'ast-grep' })));

const fromBinlog = (text: string): readonly Diagnostic[] =>
    text.split('\n').flatMap((line) => {
        const groups = _ERROR.exec(line)?.groups;
        return groups?.code === undefined || groups.file === undefined || groups.message === undefined ? [] : [{ code: groups.code, file: groups.file, line: groups.line === undefined ? none : some(Number(groups.line)), message: groups.message, source: 'binlog' as const }];
    });

// --- [TEXT]

const _location = (file: string, line: Option<number>): string => (line.kind === 'some' ? `${file}:${line.value}` : file);

const summary = (rows: readonly Diagnostic[]): string => {
    const limit = 3;
    const shown = rows.slice(0, limit).map((row) => `${row.code} ${_location(basename(row.file), row.line)} ${row.message}`);
    return [counted(rows.length, 'diagnostic', 'diagnostics'), ...shown, ...(rows.length > limit ? [`+${rows.length - limit} more`] : [])].join(' · ');
};

const lines = (rows: readonly Diagnostic[]): readonly string[] => rows.map((row) => `${row.code} ${_location(row.file, row.line)} ${row.message} (${row.source})`);

// --- [EXPORTS] -------------------------------------------------------------------------

export { fromAstGrep, fromBinlog, fromCodelens, lines, summary, trimmed };
