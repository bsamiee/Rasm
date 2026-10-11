import { decoded, fault, map, ok, type Result } from '../composition.ts';
import type { Invocation } from '../policies/invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Operation = 'format' | 'diagnostics';

interface Ran {
    readonly code: number;
    readonly stdout: string;
    readonly stderr: string;
}
interface Match {
    readonly file: string;
    readonly range: { readonly start: { readonly line: number; readonly column: number } };
    readonly ruleId: string;
    readonly severity: string;
    readonly message: string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const targets = (operation: Operation, project: string, paths: readonly string[]): Invocation => (operation === 'format' ? ['nx', 'run', `${project}:format:files`, '--output-style=static', '--', ...paths] : ['nx', 'run', `${project}:lint:ast-grep`, '--output-style=static', '--', '--json=compact', ...paths]);

const written = ({ code, stdout, stderr }: Ran): readonly string[] => (code === 0 ? [] : [`format exited ${code}, ${`${stdout}${stderr}`.trim()}`]);

const scanned = ({ code, stdout, stderr }: Ran): Result<readonly string[]> => {
    const report = stdout.split('\n').findLast((line) => line.startsWith('['));
    return report === undefined
        ? fault({ kind: 'exited', subject: 'ast-grep', code, stderr: `${stdout}${stderr}`.trim() })
        : map(decoded<readonly Match[]>('ast-grep', ok(report)), (matches) => matches.map(({ file, range: { start }, ruleId, severity, message }) => `${file}:${start.line + 1}:${start.column + 1} ${severity} ${ruleId}: ${message}`));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Operation, Ran };
export { scanned, targets, written };
