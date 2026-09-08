// The tool.call adapter folds the policy rules over every call and maps the decision onto the result union

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, ToolCallInput, ToolCallResult } from 'claude-code';
import { type Decision, fold, type Rule, rewrite, when } from '../composition/decision.ts';
import type { Options } from '../host/options.ts';
import { decode, ids, isPrompt, isStringRecord, key, type StringRecord } from '../host/store.ts';
import { gitGuard, gitPaths } from '../policies/git.ts';
import { type PathEvent, type PathTool, pathOnce, pathRule } from '../policies/paths.ts';
import { restore } from '../policies/secrets.ts';
import { commandCeiling, commandTimeout, packageManager, shellOnce, shellRule } from '../policies/shell.ts';
import { type Facts, isTool, type Named, toolOnce, toolRecords, toolRule } from '../policies/tools.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The built-in tools with the command the guard reads, four MCP tools hold a command field the guard leaves alone
type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };

type Answered = Exclude<ToolCallResult, { readonly deny: string }>;

type Succeeded = Extract<ToolCallResult, { readonly deny?: undefined; readonly isError?: undefined }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PATH_TOOLS: readonly PathTool[] = ['Read', 'Edit', 'Write', 'NotebookEdit'];

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isBash = isTool('Bash');

const _isPath = (e: ToolCallInput): e is PathEvent => _PATH_TOOLS.some((tool) => tool === e.tool);

// Bash always, Monitor when it watches a command, and a ws monitor never
const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';

const _answered = (result: ToolCallResult): result is Answered => result.deny === undefined;

const _succeeded = (result: ToolCallResult): result is Succeeded => result.deny === undefined && result.isError === undefined;

// --- [OPERATIONS] ----------------------------------------------------------------------

// Every once key the call injects that the session has not seen
const _onceKeys = (e: ToolCallInput, seen: ReadonlySet<string>): readonly string[] =>
    [...(_isBash(e) ? shellOnce(e.command) : []), ...(_isPath(e) ? pathOnce(e) : []), ...toolOnce(e)]
        .flatMap((line) => line.key ?? [])
        .filter((name) => !seen.has(name));

// Answered results take the lines, and an empty context leaves the result as next resolved it
const _withContext = (result: ToolCallResult, context: readonly string[]): ToolCallResult =>
    _answered(result) && context.length > 0 ? { ...result, context: [...(result.context ?? []), ...context] } : result;

// The shell rules over a Bash command with the secret values restored for the guards to read
const _shelled = (e: ToolCallInput, seen: ReadonlySet<string>, secrets: StringRecord): Decision<ToolCallInput> =>
    fold<ToolCallInput>([
        when(_isBash, (bash): Decision<Named<'Bash'>> => rewrite({ ...bash, command: restore(bash.command, secrets) })),
        when(_isBash, shellRule(seen)),
        when(_isBash, commandTimeout),
        when(_isBash, commandCeiling),
    ])(e);

// The guard and the rows over the command the shell rules produced, the package manager rewrite last for the model to read
const _decider =
    (manager: Rule<Named<'Bash'>>) =>
    (e: ToolCallInput, shelled: Decision<ToolCallInput>, facts: Facts, existing: ReadonlySet<string>): Decision<ToolCallInput> =>
        fold<ToolCallInput>([
            (): Decision<ToolCallInput> => shelled,
            when(_hasCommand, gitGuard(existing)),
            when(_isPath, pathRule(facts.seen)),
            toolRule(facts),
            when(_isBash, manager),
        ])(e);

// The session facts the tool rows read, from the key list and the redacted prompt row
const _facts = (session: string, all: readonly string[], promptRow: unknown): Facts => ({
    seen: new Set([...ids('injected', session)(all), ...ids('loaded', session)(all)]),
    snapshots: new Set(ids('snapshot', session)(all)),
    dns: new Set(ids('dns', session)(all)),
    prompt: decode(isPrompt)(promptRow)?.text ?? '',
});

const _record = (row: unknown): StringRecord => decode(isStringRecord)(row) ?? {};

// The command the guard reads, the one the shell rules produced
const _command = (shelled: Decision<ToolCallInput>): string => (shelled.kind === 'rewrite' && _hasCommand(shelled.e) ? shelled.e.command : '');

// --- [REGISTRATION] --------------------------------------------------------------------

const toolCall = (on: On, options: Options): void => {
    const decide = _decider(packageManager(options.packageManager));
    on('tool.call', async ($, e, next) => {
        const [session, secrets, all] = await Promise.all([$.session.id(), $.store.get(key('secrets')), $.store.keys()]);
        const facts = _facts(session, all, await $.store.get(key('prompt', session)));
        // The guard's refinements read which paths exist, over the command the shell rules produce
        const shelled = _shelled(e, facts.seen, _record(secrets));
        const existing: ReadonlySet<string> = new Set(
            (
                await Promise.all(
                    gitPaths(_command(shelled)).map(async (path) => ((await $.fs.exists(path).catch((): false => false)) ? [path] : [])),
                )
            ).flat(),
        );
        const decision = decide(e, shelled, facts, existing);
        if (decision.kind === 'deny') {
            return { deny: decision.reason };
        }
        await Promise.all(_onceKeys(decision.e, facts.seen).map((name) => $.store.set(key('injected', session, name), $.clock.now())));
        $.ui.notice(e.tool_use_id, decision.context.length > 0 ? decision.context.join(' ') : undefined);
        const result = await next(decision.e);
        // A successful call of a recording tool stamps its target id for the hostinger families
        const recorded = _succeeded(result) ? toolRecords(e) : undefined;
        if (recorded !== undefined) {
            await $.store.set(key(recorded.namespace, session, recorded.id), $.clock.now());
        }
        return _withContext(result, decision.context);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toolCall };
