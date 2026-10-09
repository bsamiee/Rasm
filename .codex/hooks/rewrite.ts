// biome-ignore-all lint/correctness/noNodejsModules: Codex runs this CLI in Node with native argument parsing and process I/O

import process from 'node:process';
import { parseArgs } from 'node:util';
import { none, type Option, rendered, some } from '../../plugins/function-hooks/composition.ts';
import { commandDecision, type Decision, pathRefusal } from '../../plugins/function-hooks/policies/policies.ts';
import { host, payload } from './host.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Call {
    readonly tool_name: 'Bash' | 'apply_patch';
    readonly tool_use_id: string;
    readonly cwd: string;
    readonly tool_input: { readonly command: string };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ADDED = /^\*\*\* Add File: (?<path>.+)$/gmu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isCall = (value: unknown): value is Call =>
    typeof value === 'object' &&
    value !== null &&
    'tool_name' in value &&
    (value.tool_name === 'Bash' || value.tool_name === 'apply_patch') &&
    'tool_use_id' in value &&
    typeof value.tool_use_id === 'string' &&
    'cwd' in value &&
    typeof value.cwd === 'string' &&
    'tool_input' in value &&
    typeof value.tool_input === 'object' &&
    value.tool_input !== null &&
    'command' in value.tool_input &&
    typeof value.tool_input.command === 'string';

const _answered = (decision: Decision): Option<object> => {
    if (decision.kind === 'deny') {
        return some({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: decision.reason } });
    }
    const { rewrite } = decision;
    return rewrite.kind === 'none' ? none : some({ systemMessage: rewrite.value.notice, hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { command: rewrite.value.command }, additionalContext: rewrite.value.context } });
};

const _decision = (call: Call, walkPolicy: boolean): Promise<Decision> => {
    const refusal = pathRefusal([...call.tool_input.command.matchAll(_ADDED)].flatMap(({ groups }) => groups?.path ?? []));
    return call.tool_name === 'Bash' ? commandDecision(host(call.cwd), 'Bash', call.tool_input.command, call.tool_use_id, walkPolicy) : Promise.resolve(refusal.kind === 'some' ? { kind: 'deny', reason: refusal.value } : { kind: 'allow', rewrite: none });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const input = payload(_isCall);
if (input.kind === 'fault') {
    process.stderr.write(`${rendered(input.faults)}\n`);
    process.exitCode = 1;
} else {
    const answer = _answered(await _decision(input.value, parseArgs({ options: { 'walk-policy': { type: 'boolean', default: false } } }).values['walk-policy']));
    process.stdout.write(answer.kind === 'some' ? JSON.stringify(answer.value) : '');
}
