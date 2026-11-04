import { Option } from '../monads.ts';
import { appRoute, processKills, runningApps } from './internal/apps.ts';
import { applyEdits, parse, type Script } from './internal/command.ts';
import { deletes } from './internal/delete.ts';
import { ghidra, projectCopies } from './internal/ghidra.ts';
import { git } from './internal/git.ts';
import { inputSources } from './internal/input.ts';
import type { Invocation } from './internal/invocation.ts';
import { pipefail } from './internal/pipeline.ts';
import { noOpReplacements } from './internal/replace.ts';
import { loopReruns } from './internal/rerun.ts';
import { commandRewrites, timeoutCommands } from './internal/rewrite.ts';
import { searchRoots } from './internal/search-root.ts';
import { type Call, MAX_TIMEOUT } from './internal/verdict.ts';
import { appPolls, backgroundJobs, foregroundWaits, selfMatches, taskTails, taskWriters } from './internal/wait.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow' } | { readonly kind: 'rewrite'; readonly command: string; readonly notice: Option<string>; readonly timeout: Option<number>; readonly background: boolean };
type CommandRequest = { readonly kind: 'foreground'; readonly timeout: Option<number> } | { readonly kind: 'background' | 'monitor' | 'plain' };

interface System {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly read: (path: string) => Promise<string>;
    readonly environment: (name: string) => Promise<Option<string>>;
    readonly scanner: string;
    readonly plugin: string;
    readonly root: Option<string>;
    readonly cwd: string;
    readonly home: Option<string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const policyFailure = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const scriptDecision = async (system: System, source: Uint8Array, script: Script, project: string, request: CommandRequest): Promise<Decision> => {
    const { commands } = script;
    const [apps, writers] = await Promise.all([runningApps(system, commands), taskWriters(system, request, commands)]);
    const routes = new Map((await Promise.all(commands.map(async (each) => (await appRoute(system, request, apps, each)).values.map((route) => [each, route] as const)))).flat());
    const call: Call = { system, source, script, project, request, apps, writers, routes, projects: new Map(), parses: new Map() };
    const verdicts = (
        await Promise.all([...[deletes, git, searchRoots, noOpReplacements, inputSources, selfMatches, foregroundWaits, taskTails, backgroundJobs, timeoutCommands, ghidra, projectCopies, pipefail, loopReruns, processKills, appPolls].map((policy) => policy(call)), ...commands.map((each) => commandRewrites(call, each))])
    ).flat();
    const reasons = [...new Set(verdicts.flatMap((verdict) => (verdict.kind === 'refuse' ? [verdict.reason] : [])))];
    const edits = verdicts.flatMap((verdict) => (verdict.kind === 'edit' ? [verdict] : []));
    const backgrounds = verdicts.flatMap((verdict) => (verdict.kind === 'background' ? [verdict.notice] : []));
    const notices = [...new Set([...edits.flatMap(({ notice }) => notice.values), ...backgrounds])];
    const timeouts = edits.flatMap(({ timeout }) => timeout.values);
    const total = timeouts.reduce((sum, value) => sum + value, 0);
    if (reasons.length > 0) {
        return { kind: 'deny', reason: reasons.join('. ') };
    }
    if (edits.length + backgrounds.length === 0) {
        return { kind: 'allow' };
    }
    return {
        kind: 'rewrite',
        command: applyEdits(source, 0, edits),
        notice: notices.length === 0 ? Option.none : Option.some(notices.join('. ')),
        timeout: timeouts.length === 0 ? Option.none : Option.some(Math.min(MAX_TIMEOUT, total)),
        background: backgrounds.length > 0,
    };
};

const commandDecision = async (system: System, command: string, project: string, request: CommandRequest): Promise<Decision> => {
    const source = new TextEncoder().encode(command);
    return (await parse(system, command)).match<Decision | Promise<Decision>>(
        (script) => scriptDecision(system, source, script, project, request),
        (faults) => ({ kind: 'deny', reason: `Command not parsed. ${faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, ...(value.output === '' ? [] : [value.output])].join('. ') : `${value.program} failed. ${String(value.cause)}`)).join('. ')}` }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type CommandRequest, commandDecision, policyFailure, type System };
