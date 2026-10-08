import { counted, decoded, type Fault, map, none, type Option, ok, type Result, some } from '../composition.ts';
import type { Diagnostic } from '../hooks/state.d.ts';
import { basename } from '../policies/invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface AstGrepMatch {
    readonly ruleId: string;
    readonly message: string;
    readonly note: string | null;
    readonly file: string;
    readonly range: { readonly start: { readonly line: number } };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _RULE = /^Error: (?<code>Cannot parse rule) (?<file>.+)$/mu;
const _AT = / at line (?<line>\d+) column \d+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODING]

const _unparsed = (value: Fault, file: string): readonly Diagnostic[] => {
    if (value.kind !== 'exited') {
        return [];
    }
    const groups = _RULE.exec(value.stderr)?.groups;
    if (groups?.code === undefined || groups.file !== file) {
        return [];
    }
    const cause = '╰▻ ';
    const message = value.stderr
        .split('\n')
        .flatMap((line) => (line.startsWith(cause) ? [line.slice(cause.length)] : []))
        .join(': ');
    const at = _AT.exec(message)?.groups?.line;
    return [{ code: groups.code, file, line: at === undefined ? none : some(Number(at)), message }];
};

const fromAstGrep = (printed: Result<string>, file: string): Result<readonly Diagnostic[]> => {
    if (printed.kind === 'ok') {
        return map(decoded<readonly AstGrepMatch[]>('ast-grep scan', printed), (found) => found.map((match): Diagnostic => ({ code: match.ruleId, file: match.file, line: some(match.range.start.line + 1), message: match.note === null ? match.message : `${match.message}. ${match.note}` })));
    }
    const unparsed = printed.faults.flatMap((value) => _unparsed(value, file));
    return unparsed.length === printed.faults.length ? ok(unparsed) : printed;
};

// --- [TEXT]

const _location = (file: string, line: Option<number>): string => (line.kind === 'some' ? `${file}:${line.value}` : file);

const summary = (rows: readonly Diagnostic[]): string => {
    const limit = 3;
    const shown = rows.slice(0, limit).map((row) => `${row.code} ${_location(basename(row.file), row.line)} ${row.message}`);
    return [counted(rows.length, 'diagnostic', 'diagnostics'), ...shown, ...(rows.length > limit ? [`+${rows.length - limit} more`] : [])].join(' · ');
};

const lines = (rows: readonly Diagnostic[]): readonly string[] => rows.map((row) => `${row.code} ${_location(row.file, row.line)} ${row.message}`);

// --- [EXPORTS] -------------------------------------------------------------------------

export { fromAstGrep, lines, summary };
