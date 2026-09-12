// --- [IMPORTS] -------------------------------------------------------------------------

import type { AgentSpawnResult, ClassicHookInputs, PluginOptions } from 'claude-code';
import { all, fault, invalid, ok, type Result } from '../composition/result.ts';
import { quoted } from './sql.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Kind = 'edit';

interface Trigger {
    readonly kind: Kind;
    readonly view: string;
    readonly threshold: number;
    readonly agent: string;
}

interface Settings {
    readonly edits: Trigger;
    readonly categoryThreshold: number;
    readonly categoryAgent: string;
}

interface Lineage {
    readonly worktree: string;
    readonly branch: string;
    readonly key: string;
}

interface Range {
    readonly trigger: Trigger;
    readonly count: number;
    readonly from: number;
    readonly running: boolean;
}

interface Candidate {
    readonly category: string;
    readonly sites: number;
    readonly reported: boolean;
}

interface State {
    readonly edits: Range;
    readonly open: number;
    readonly undelivered: number;
    readonly categoryRunning: boolean;
    readonly candidates: readonly Candidate[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _INTEGER = /^\d+$/u;
const _CELLS = 6;

// --- [SETTINGS] ------------------------------------------------------------------------

const _trigger = (options: PluginOptions, kind: Kind, view: string): Trigger => ({
    kind,
    view,
    threshold: Number(options[`${kind}Threshold`]),
    agent: String(options[`${kind}Agent`]),
});

const settings = (options: PluginOptions): Settings => ({
    edits: _trigger(options, 'edit', 'unjudged_edits'),
    categoryThreshold: Number(options['categoryThreshold']),
    categoryAgent: String(options['categoryAgent']),
});

const lineageOf = (main: string, worktree: string, branch: string): Lineage => ({ worktree, branch, key: `${main}/${worktree}/${branch}` });

// --- [STATEMENTS] ----------------------------------------------------------------------

const _under = (column: string, worktree: string): string => {
    const literal = quoted(worktree);
    return `(${column} = ${literal} or substr(${column}, 1, length(${literal}) + 1) = ${quoted(`${worktree}/`)})`;
};

const _elsewhere = (agent: string, lineage: Lineage): string =>
    `exists (select 1 from running_agents r where r.agent_type = ${quoted(agent)} and ${_under('r.cwd', lineage.worktree)})`;

const _range = (trigger: Trigger, lineage: Lineage, to: number): string =>
    `(select count(1) from ${trigger.view} v where v.ts > r.f and v.ts <= ${to} and ${_under('v.cwd', lineage.worktree)}), r.f, ${trigger.threshold > 0 ? _elsewhere(trigger.agent, lineage) : '0'}`;

const STATE = (lineage: Lineage, to: number, chosen: Settings): string => {
    const key = quoted(lineage.key);
    return [
        '.mode tabs',
        `with r(f) as (select coalesce(max(to_ts), 0) from judged_range where kind = ${quoted(chosen.edits.kind)} and lineage_key = ${key}), o as (select delivered_on from open_findings where subject_hash = lower(hex(sha3(readfile(path), 256)))) select ${_range(chosen.edits, lineage, to)}, (select count(1) from o), (select count(1) from o where not exists (select 1 from json_each(o.delivered_on) where value = ${key})), ${_elsewhere(chosen.categoryAgent, lineage)} from r;`,
        `select category, sites, exists (select 1 from json_each(reported_on) where value = ${key}) from recurring_categories order by sites desc, category;`,
    ].join('\n');
};

const REPORT = (lineage: Lineage, session: string, category: string, now: number): string =>
    [
        'pragma foreign_keys = on;',
        `insert into finding_delivery(finding_id, lineage_key, session_id, agent_id, channel, delivered_at) select finding_id, ${quoted(lineage.key)}, ${quoted(session)}, null, 'report', ${now} from confirmed_findings where category = ${quoted(category)};`,
    ].join('\n');

const REVOKE = (lineage: Lineage, session: string, now: number): string =>
    [
        'pragma foreign_keys = on;',
        `delete from finding_delivery where lineage_key = ${quoted(lineage.key)} and session_id = ${quoted(session)} and channel = 'report' and delivered_at = ${now};`,
    ].join('\n');

const DELIVER = (lineage: Lineage, session: string, now: number): string => {
    const key = quoted(lineage.key);
    return [
        '.mode tabs',
        'pragma foreign_keys = on;',
        `insert into finding_delivery(finding_id, lineage_key, session_id, agent_id, channel, delivered_at) select finding_id, ${key}, ${quoted(session)}, null, 'additionalContext', ${now} from open_findings where subject_hash = lower(hex(sha3(readfile(path), 256))) and not exists (select 1 from json_each(delivered_on) where value = ${key}) returning finding_id;`,
    ].join('\n');
};

// --- [READING] -------------------------------------------------------------------------

const _lines = (stdout: string): readonly string[] => stdout.split('\n').filter((text) => text !== '');

const _cells = (stdout: string): readonly (readonly string[])[] => _lines(stdout).map((text) => text.split('\t'));

const _integer = (text: string): Result<number> => (_INTEGER.test(text) ? ok(Number(text)) : fault(`${text} is not an integer`));

const _flag = (text: string): Result<boolean> => (text === '0' || text === '1' ? ok(text === '1') : fault(`${text} is not 0 or 1`));

const _rangeOf = (trigger: Trigger, count: string, from: string, running: string): Result<Range> => {
    const counted = _integer(count);
    const since = _integer(from);
    const elsewhere = _flag(running);
    return counted.kind === 'ok' && since.kind === 'ok' && elsewhere.kind === 'ok'
        ? ok({ trigger, count: counted.value, from: since.value, running: elsewhere.value })
        : invalid([counted, since, elsewhere]);
};

const _candidate = ([category = '', sites = '', reported = '']: readonly string[]): Result<Candidate> => {
    const counted = _integer(sites);
    const told = _flag(reported);
    return counted.kind === 'ok' && told.kind === 'ok' ? ok({ category, sites: counted.value, reported: told.value }) : invalid([counted, told]);
};

const _state = (chosen: Settings, head: readonly string[], rest: readonly (readonly string[])[]): Result<State> => {
    const [count = '', from = '', running = '', open = '', undelivered = '', categoryRunning = ''] = head;
    const edits = _rangeOf(chosen.edits, count, from, running);
    const opened = _integer(open);
    const waiting = _integer(undelivered);
    const elsewhere = _flag(categoryRunning);
    const candidates = all(rest.map(_candidate));
    return edits.kind === 'ok' && opened.kind === 'ok' && waiting.kind === 'ok' && elsewhere.kind === 'ok' && candidates.kind === 'ok'
        ? ok({ edits: edits.value, open: opened.value, undelivered: waiting.value, categoryRunning: elsewhere.value, candidates: candidates.value })
        : invalid([edits, opened, waiting, elsewhere, candidates]);
};

const state = (stdout: string, chosen: Settings): Result<State> => {
    const [head, ...rest] = _cells(stdout);
    return head === undefined || head.length !== _CELLS ? fault(`state line holds ${head?.length ?? 0} cells`) : _state(chosen, head, rest);
};

// --- [DECISIONS] -----------------------------------------------------------------------

const listed = (tasks: ClassicHookInputs['Stop']['background_tasks']): Result<readonly string[]> =>
    tasks === undefined ? fault('background_tasks absent') : ok(tasks.flatMap((task) => (task.agent_type === undefined ? [] : [task.agent_type])));

const due = (range: Range, busy: readonly string[]): boolean =>
    range.trigger.threshold > 0 && range.count >= range.trigger.threshold && !range.running && !busy.includes(range.trigger.agent);

const dueCategories = (seen: State, chosen: Settings, busy: readonly string[]): readonly Candidate[] =>
    chosen.categoryThreshold > 0 && !seen.categoryRunning && !busy.includes(chosen.categoryAgent)
        ? seen.candidates.filter((candidate) => candidate.sites >= chosen.categoryThreshold && !candidate.reported)
        : [];

const awaiting = (seen: State): readonly Candidate[] => seen.candidates.filter((candidate) => !candidate.reported);

// --- [TEXT] ----------------------------------------------------------------------------

const rangePrompt = (lineage: Lineage, from: number, to: number): string => `range ${lineage.key} ${from} ${to}`;

const categoryPrompt = (category: string, lineage: Lineage): string => `category ${category} lineage ${lineage.key}`;

const _many = (count: number, one: string, many: string): string => `${count} ${count === 1 ? one : many}`;

const context = (stdout: string, branch: string): readonly string[] => {
    const found = _lines(stdout);
    return found.length === 0
        ? []
        : [
              `${_many(found.length, 'finding', 'findings')} on ${branch}, ids ${found.join(', ')}, apply the delivery section of the observation skill`,
          ];
};

const _segment = (count: number, text: string): readonly string[] => (count === 0 ? [] : [text]);

const status = (seen: State): string => {
    const waiting = awaiting(seen).length;
    return [
        ..._segment(seen.edits.count, `${_many(seen.edits.count, 'edit', 'edits')} unjudged`),
        ..._segment(seen.open, `${_many(seen.open, 'finding', 'findings')} open`),
        ..._segment(waiting, _many(waiting, 'recurring category', 'recurring categories')),
    ].join(' · ');
};

const resolved = (agent: string, subject: string, result: AgentSpawnResult): string =>
    result.deny === undefined
        ? `spawned ${agent}${result.agentId === undefined ? '' : ` ${result.agentId}`} over ${subject}`
        : `${agent} refused over ${subject}: ${result.deny}`;

const unbuilt = (waiting: readonly Candidate[]): string =>
    `${waiting.map((candidate) => `${candidate.category} ${_many(candidate.sites, 'site', 'sites')}`).join(', ')} at categoryThreshold 0`;

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Candidate, Lineage, Range, Settings, State };
export {
    awaiting,
    categoryPrompt,
    context,
    DELIVER,
    due,
    dueCategories,
    lineageOf,
    listed,
    REPORT,
    REVOKE,
    rangePrompt,
    resolved,
    STATE,
    settings,
    state,
    status,
    unbuilt,
};
