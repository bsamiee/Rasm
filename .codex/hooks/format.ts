import { resolve } from 'node:path';
import process from 'node:process';
import { all, bind, both, decoded, map, none, ok, rendered } from '../../plugins/function-hooks/composition.ts';
import { reformatted } from '../../plugins/function-hooks/repository/format.ts';
import { exec, formatter, payload, read } from './host.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stop {
    readonly transcript_path: string;
    readonly turn_id: string;
    readonly cwd: string;
    readonly stop_hook_active: boolean;
}
interface Row {
    readonly payload?: { readonly input?: string; readonly internal_chat_message_metadata_passthrough?: { readonly turn_id?: string } };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HEADER = /\*\*\* (?<operation>Add File|Update File|Delete File|Move to): (?<path>.+?)(?=\\+n)/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isStop = (value: unknown): value is Stop =>
    typeof value === 'object' && value !== null && 'transcript_path' in value && typeof value.transcript_path === 'string' && 'turn_id' in value && typeof value.turn_id === 'string' && 'cwd' in value && typeof value.cwd === 'string' && 'stop_hook_active' in value && typeof value.stop_hook_active === 'boolean';

const _patched = (rows: readonly Row[], { turn_id, cwd }: Stop): readonly string[] => {
    const headers = rows
        .flatMap((row) => (row.payload?.input !== undefined && row.payload.internal_chat_message_metadata_passthrough?.turn_id === turn_id ? [...row.payload.input.matchAll(_HEADER)] : []))
        .flatMap(({ groups }) => (groups?.operation === undefined || groups.path === undefined ? [] : [{ operation: groups.operation, path: resolve(cwd, groups.path) }]));
    return headers.reduce<readonly string[]>((paths, { operation, path }, index) => {
        const moved = operation === 'Move to' ? [headers[index - 1]?.path] : [];
        const kept = paths.filter((known) => known !== path && !moved.includes(known));
        return operation === 'Delete File' ? kept : [...kept, path];
    }, []);
};

const _messages = async (stop: Stop): Promise<readonly string[]> => {
    const root = map(exec(['git', 'rev-parse', '--show-toplevel'], stop.cwd, none, [0]), (printed) => printed.trim());
    const rows = bind(await read(stop.transcript_path), (text) =>
        all(
            text
                .split('\n')
                .filter((line) => line.length > 0)
                .map((line) => decoded<Row>(stop.transcript_path, ok(line))),
        ),
    );
    const held = both(root, rows);
    return held.kind === 'fault' ? [] : reformatted(formatter(held.value[0]), held.value[0], _patched(held.value[1], stop));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const input = payload(_isStop);
if (input.kind === 'fault') {
    process.stderr.write(`${rendered(input.faults)}\n`);
    process.exitCode = 1;
} else {
    const lines = input.value.stop_hook_active ? [] : await _messages(input.value);
    process.stdout.write(lines.length === 0 ? '{}' : JSON.stringify({ decision: 'block', reason: lines.join('. ') }));
}
