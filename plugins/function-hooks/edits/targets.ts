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

const target = (operation: Operation, project: string): string => `${project}:${operation === 'format' ? 'format:files' : 'lint:ast-grep'}`;
const targets = (operation: Operation, project: string, paths: readonly string[]): Invocation => ['nx', 'run', target(operation, project), '--output-style=static', '--', ...(operation === 'format' ? [] : ['--json=compact']), ...paths];
const written = (named: string, { code }: Ran): readonly string[] => (code === 0 ? [] : [`nx run ${named} exited ${code}`]);

const scanned = (named: string, { code, stdout, stderr }: Ran): Result<readonly string[]> => {
    const report = stdout.split('\n').findLast((line) => line.startsWith('['));
    return report === undefined
        ? fault({ kind: 'exited', subject: `nx run ${named}`, code, stderr: stderr.trim() })
        : map(decoded<readonly Match[]>('ast-grep', ok(report)), (matches) => matches.map(({ file, range: { start }, ruleId, severity, message }) => `${file}:${start.line + 1}:${start.column + 1} ${severity} ${ruleId}: ${message}`));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Operation, Ran };
export { scanned, target, targets, written };
