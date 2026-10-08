import { resolve } from 'node:path';
import process from 'node:process';
import { all, bind, both, decoded, map, none, ok, type Result, rendered } from '../../plugins/function-hooks/composition.ts';
import { reformatted } from '../../plugins/function-hooks/repository/format.ts';
import { exec, formatter, payload, read } from './host.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stop {
    readonly transcript_path: string;
    readonly turn_id: string;
    readonly cwd: string;
}
interface Row {
    readonly payload?: { readonly input?: string; readonly internal_chat_message_metadata_passthrough?: { readonly turn_id?: string } };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HEADER = /\*\*\* (?<operation>Add File|Update File|Delete File|Move to): (?<path>.+?)(?=\\+n)/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isStop = (value: unknown): value is Stop => typeof value === 'object' && value !== null && 'transcript_path' in value && typeof value.transcript_path === 'string' && 'turn_id' in value && typeof value.turn_id === 'string' && 'cwd' in value && typeof value.cwd === 'string';

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

const _messages = async (stop: Stop): Promise<Result<readonly string[]>> => {
    const root = map(exec(['git', 'rev-parse', '--show-toplevel'], stop.cwd, none, [0]), (printed) => printed.trim());
    const rows = bind(await read(stop.transcript_path), (text) =>
        all(
            text
                .split('\n')
                .filter((line) => line.length > 0)
                .map((line) => decoded<Row>(stop.transcript_path, ok(line))),
        ),
    );
    return bind(both(root, rows), async ([top, held]) => {
        const { context, failed } = await reformatted(formatter(top), top, _patched(held, stop));
        return ok([...(context.kind === 'some' ? [context.value] : []), ...(failed.length === 0 ? [] : [rendered(failed)])]);
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const input = payload(_isStop);
const messages = input.kind === 'ok' ? await _messages(input.value) : input;
const lines = messages.kind === 'ok' ? messages.value : [rendered(messages.faults)];
process.stdout.write(lines.length === 0 ? '' : JSON.stringify({ systemMessage: lines.join('. ') }));
