import { Maybe } from '../../monads.ts';
import type { CommandRequest, System } from '../command-decision.ts';
import { namedPids, PID, WATCHER, watchAction, watchedApps } from './apps.ts';
import type { Command } from './command.ts';
import { argumentGroups, enabled, informational, operands, programRow } from './invocation.ts';
import { background, type Call, edit, editOrRefuse, invocationSites, invocationsOf, nestedIn, pipelineNeighbors, quote, refuse, type Verdict } from './verdict.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LETTER = /^\w/u;
const TASK_OUTPUT = /\/tasks\/(?<id>[^/]+)\.output$/u;
const TASK_OUTPUTS = /\/tasks\/(?<id>[^/\s'"]+)\.output/gu;
const SPACES = /\s+/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SELF_MATCH]

const selfMatches = ({ script: { commands } }: Call): readonly Verdict[] =>
    invocationSites(commands).flatMap(({ command, index, invocation, at }) => {
        const [program] = invocation;
        const { options } = operands(invocation);
        const listing = commands[index - 1];
        const grepped = program === 'grep' && command.hasInput && at === 0 && listing?.pipesOutput === true && listing.invocations.at(-1)?.[0] === 'ps' && options.length === 0;
        const word = ((program === 'pgrep' && options.includes('-f')) || grepped) && command.origin !== 'file' ? argumentGroups(program, (command.parts[at]?.words.slice(1) ?? []).values()).find(({ names }) => names.length === 0)?.words[0] : undefined;
        const text = word === undefined || word.expands || !LETTER.test(word.text) ? undefined : quote(`[${word.text.slice(0, 1)}]${word.text.slice(1)}`);
        return word === undefined || text === undefined ? [] : [editOrRefuse(command, edit(word.span.start, word.span.end, text), `${program} ${word.raw} matches its own shell, use ${text}`)];
    });

// --- [LOOPS]

const removedSleeps = ({ request, routes, script: { commands } }: Call): readonly { readonly command: Command; readonly verdict: Verdict }[] => {
    const own = commands.filter(({ origin }) => origin === 'call').toSorted((left, right) => left.extent.start - right.extent.start);
    const driving = own.some((command) => !command.inLoop && (routes.get(command)?.kind === 'rewrite' || command.invocations.some(([program]) => program === WATCHER)));
    const answered = request.kind === 'foreground' || request.kind === 'plain';
    return own.flatMap((command, at) => {
        const [only, ...more] = command.invocations;
        const [next, last] = [own[at + 1], own[at - 1]];
        if (!(driving && answered && only?.[0] === 'sleep' && more.length === 0) || command.inLoop || command.hasInput || command.pipesOutput) {
            return [];
        }
        const [start, end] = next === undefined ? [last?.extent.end ?? command.extent.start, command.extent.end] : [command.extent.start, next.extent.start];
        return [{ command, verdict: edit(start, end, '', Maybe.some(`sleep removed, ${WATCHER} prints current states`)) }];
    });
};

const taskNotice = (id: string): string => `Task ${id} notifies on completion, continue until its notice arrives`;

const pollingLoops = ({ request, apps, source, script: { commands } }: Call): readonly string[] => {
    const loopCommands = commands.filter(({ inLoop }) => inLoop);
    const sleeping = loopCommands.some(({ invocations: chain }) => chain.some(([program]) => enabled(programRow(program).timer, [])));
    const waited = sleeping ? [...new TextDecoder().decode(source).matchAll(TASK_OUTPUTS)].flatMap((match) => Maybe.fromNullable(match.groups?.id).values) : [];
    const names = invocationSites(loopCommands).flatMap(({ invocation }) =>
        invocation[0] === WATCHER
            ? [watchedApps(invocation)]
            : namedPids(invocation)
                  .flatMap((pid) => Maybe.fromNullable(apps.get(pid)).values)
                  .map((bundle) => [bundle]),
    );
    return [...[...new Set(waited)].map(taskNotice), ...names.map((name) => `Loop polls ${name.join(' ') || WATCHER}, ${watchAction(request, name)}`)];
};

const appPolls = (call: Call): readonly Verdict[] => {
    const { request, script } = call;
    const { commands } = script;
    const streaming = invocationsOf(commands, WATCHER).filter((invocation) => operands(invocation).inputs[0] === 'watch' && !informational(WATCHER, operands(invocation).options));
    return [...pollingLoops(call).map(refuse), ...(request.kind === 'monitor' ? [] : streaming.map((invocation) => refuse(`${WATCHER} watch never exits, ${watchAction(request, watchedApps(invocation))}`)))];
};

// --- [TASKS]

const tailedTasks = ({ invocations: chain }: Command): readonly string[] => chain.flatMap((invocation) => (invocation[0] === 'tail' && enabled(programRow('tail').endless, operands(invocation).options) ? operands(invocation).inputs.filter((file) => TASK_OUTPUT.test(file)) : []));

const taskWriters = async (system: System, request: CommandRequest, commands: readonly Command[]): Promise<ReadonlyMap<string, string>> => {
    const files = request.kind === 'monitor' ? [...new Set(commands.flatMap(tailedTasks))] : [];
    const probed = await Promise.all(
        files.map(async (file) => {
            const holders = (await system.run(['lsof', '-t', file], { cwd: system.plugin, stdin: '' })).stdout.split('\n').filter((pid) => PID.test(pid));
            const table = holders.length === 0 ? '' : (await system.run(['ps', '-o', 'pid=,ppid=', '-p', holders.join(',')], { cwd: system.plugin, stdin: '' })).stdout;
            const [root] = table.split('\n').flatMap((row) => {
                const [pid, parent] = row.trim().split(SPACES);
                return pid !== undefined && PID.test(pid) && !holders.includes(parent ?? '') ? [pid] : [];
            });
            return root === undefined ? [] : [[file, root] as const];
        }),
    );
    return new Map(probed.flat());
};

const taskTails = ({ request, writers, script: { commands } }: Call): readonly Verdict[] =>
    request.kind === 'monitor'
        ? commands.flatMap((command, index) => {
              const files = tailedTasks(command);
              const words = command.parts[command.invocations.findIndex(([program]) => program === 'tail')]?.words ?? [];
              const [first, last] = [words[0], words.at(-1)];
              const ended = invocationSites(pipelineNeighbors(commands, index, 1)).some(({ invocation }) => enabled(programRow(invocation[0]).closesInput, operands(invocation).options));
              const roots = files.flatMap((file) => Maybe.fromNullable(writers.get(file)).values);
              const ids = files.flatMap((file) => Maybe.fromNullable(TASK_OUTPUT.exec(file)?.groups?.id).values).join(' ');
              if (files.length === 0 || first === undefined || last === undefined || ended) {
                  return [];
              }
              return roots.length < files.length ? [refuse(taskNotice(ids))] : [edit(first.span.start, first.span.start, '{ '), edit(last.span.end, last.span.end, ` & ${roots.map((pid) => `caffeinate -w ${pid}`).join('; ')}; kill $!; }`, Maybe.some(`tail ends with task ${ids}`))];
          })
        : [];

// --- [FOREGROUND]

const foregroundWaits = (call: Call): readonly Verdict[] => {
    const { request, script } = call;
    const { commands } = script;
    const dropped = removedSleeps(call);
    const polling = pollingLoops(call).length > 0;
    const watched = invocationsOf(commands, WATCHER);
    const waitReason = watched.length === 0 ? 'Command waits in the foreground, use Monitor or run_in_background' : `Command waits on ${WATCHER} in the foreground, ${watchAction(request, watched.flatMap(watchedApps))}`;
    const kept = commands.filter((command) => !(dropped.some((drop) => drop.command === command) || (polling && command.inLoop)));
    return [
        ...dropped.map(({ verdict }) => verdict),
        ...invocationSites(kept).flatMap(({ command, invocation, at }) => {
            const [program] = invocation;
            const { timer, endless } = programRow(program);
            const { options, values } = operands(invocation);
            const holding = at === command.invocations.length - 1 && (enabled(timer, options) || enabled(endless, options));
            const [first] = command.parts[at]?.words ?? [];
            const next = command.parts[at + 1]?.words[0];
            const [amount] = values.flatMap(([name, value]) => (name === '-t' ? [value] : []));
            const leaking = program === 'read' && enabled(timer, options) && command.hasInput && nestedIn(commands, command).some(({ inSubstitution }) => inSubstitution);
            return [
                ...(request.kind === 'foreground' && (holding || command.inBusyLoop) ? [refuse(waitReason)] : []),
                ...(program === 'caffeinate' && first !== undefined && next !== undefined ? [editOrRefuse(command, edit(first.span.start, next.span.start, '', Maybe.some(`caffeinate removed, ${next.text} runs directly`)), `caffeinate wraps ${next.text}, run ${next.text} directly`)] : []),
                ...(leaking && amount !== undefined ? [editOrRefuse(command, edit(command.extent.start, command.extent.end, `sleep ${amount}`, Maybe.some(`read -t ${amount} became sleep ${amount}`)), `read -t ${amount} leaks its input process, use sleep ${amount}`)] : []),
            ];
        }),
    ];
};

const backgroundJobs = ({ request, source, script: { commands, jobs } }: Call): readonly Verdict[] => {
    const [job, ...more] = jobs;
    if (job === undefined || invocationsOf(commands, 'wait').length > 0) {
        return [];
    }
    const trailing = more.length === 0 && job.origin === 'call' && new TextDecoder().decode(source.subarray(job.end)).trim() === '';
    if (!trailing) {
        return [refuse(`& job outlives the call, ${request.kind === 'plain' ? 'run it as its own call' : 'set run_in_background'}`)];
    }
    return [edit(job.start, job.end, ''), ...(request.kind === 'foreground' ? [background('&')] : [])];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { appPolls, backgroundJobs, foregroundWaits, selfMatches, taskTails, taskWriters };
