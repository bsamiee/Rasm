// --- [IMPORTS] -------------------------------------------------------------------------

import type { AgentSpawnResult, ClassicHookInputs, PluginOptions } from 'claude-code';
import { all, fault, map, ok, type Result } from '../composition.ts';
import { basename } from '../path.ts';
import { normalized, quoted } from './sql.ts';

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
    readonly main: string;
    readonly worktree: string;
    readonly branch: string;
    readonly key: string;
}

interface Range {
    readonly trigger: Trigger;
    readonly count: number;
    readonly from: number;
    readonly running: boolean;
    readonly holding: number;
}

interface Task {
    readonly id: string;
    readonly agentType: string;
}

interface Judging {
    readonly kind: 'range';
    readonly agent: string;
    readonly lineage: Lineage;
    readonly range: Range;
    readonly to: number;
}

interface Building {
    readonly kind: 'category';
    readonly agent: string;
    readonly lineage: Lineage;
    readonly session: string;
    readonly category: string;
}

type Spawned = Judging | Building;

type Head = readonly [string, string, string, string, string, string, string, string];

type Category = readonly [string, string, string];

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
    readonly rules: number;
    readonly candidates: readonly Candidate[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _INTEGER = /^\d+$/u;
const _CELLS = 8;
const _CATEGORY_CELLS = 3;
const _PRESENT = `instr(${normalized('cast(readfile(path) as text)')}, ntext) > 0`;
const _APPLY = 'apply the delivery section of the observation skill';

// --- [SETTINGS] ------------------------------------------------------------------------

const _number = (options: PluginOptions, key: string): Result<number> => {
    const value = options[key];
    return typeof value === 'number' ? ok(value) : fault(`${key} is not a number`);
};

const _text = (options: PluginOptions, key: string): Result<string> => {
    const value = options[key];
    return typeof value === 'string' ? ok(value) : fault(`${key} is not a string`);
};

const _trigger = (options: PluginOptions, kind: Kind, view: string): Result<Trigger> =>
    map(all([_number(options, `${kind}Threshold`), _text(options, `${kind}Agent`)]), ([threshold, agent]) => ({ kind, view, threshold, agent }));

const settings = (options: PluginOptions): Result<Settings> =>
    map(all([_trigger(options, 'edit', 'unjudged_edits'), _number(options, 'categoryThreshold'), _text(options, 'categoryAgent')]), ([edits, categoryThreshold, categoryAgent]) => ({
        edits,
        categoryThreshold,
        categoryAgent,
    }));

const lineageOf = (main: string, worktree: string, branch: string): Lineage => ({ main, worktree, branch, key: `${worktree === main ? '.' : basename(worktree)}/${branch}` });

// --- [STATEMENTS] ----------------------------------------------------------------------

const _under = (column: string, worktree: string): string => {
    const literal = quoted(worktree);
    return `(${column} = ${literal} or substr(${column}, 1, length(${literal}) + 1) = ${quoted(`${worktree}/`)})`;
};

const _elsewhere = (agent: string, lineage: Lineage): string => `exists (select 1 from running_agents r where r.agent_type = ${quoted(agent)} and ${_under('r.cwd', lineage.worktree)})`;

const _untold = (key: string): string => `from placed_rules p where p.lineage_key = ${key} and not exists (select 1 from json_each(p.told_on) where value = ${key})`;

const STATE = (lineage: Lineage, to: number, chosen: Settings): string => {
    const key = quoted(lineage.key);
    const edited = `from ${chosen.edits.view} v where v.ts > r.f and v.ts <= ${to} and ${_under('v.cwd', lineage.worktree)}`;
    return [
        '.mode tabs',
        `with r(f) as (select coalesce(max(to_ts), 0) from judged_range where kind = ${quoted(chosen.edits.kind)} and lineage_key = ${key}), o as (select delivered_on from open_findings where ${_PRESENT}) select (select count(distinct v.file_path) ${edited}), r.f, ${chosen.edits.threshold > 0 ? _elsewhere(chosen.edits.agent, lineage) : '0'}, (select count(1) from o), (select count(1) from o where not exists (select 1 from json_each(o.delivered_on) where value = ${key})), ${_elsewhere(chosen.categoryAgent, lineage)}, (select count(1) from running_agents a where a.agent_id in (select v.agent_id ${edited} and v.agent_id is not null)), (select count(1) ${_untold(key)}) from r;`,
        `select category, sites, exists (select 1 from json_each(reported_on) where value = ${key}) from recurring_categories order by sites desc, category;`,
    ].join('\n');
};

const LEDGER = (judging: Judging, agent: string, at: number): string =>
    [
        '.mode tabs',
        'pragma foreign_keys = on;',
        `insert into judged_range(kind, main_worktree, worktree, branch, from_ts, to_ts, agent_id, at) select ${quoted(judging.range.trigger.kind)}, ${quoted(judging.lineage.main)}, ${quoted(judging.lineage.worktree)}, ${quoted(judging.lineage.branch)}, ${judging.range.from}, ${judging.to}, ${quoted(agent)}, ${at} where exists (select 1 from finding_transition t where t.by = ${quoted(`agent:${agent}`)}) returning rowid;`,
    ].join('\n');

const REPORT = (building: Building, agent: string, at: number): string =>
    [
        'pragma foreign_keys = on;',
        `insert into finding_delivery(finding_id, lineage_key, session_id, agent_id, channel, delivered_at) select finding_id, ${quoted(building.lineage.key)}, ${quoted(building.session)}, ${quoted(agent)}, 'report', ${at} from finding where category = ${quoted(building.category)};`,
    ].join('\n');

const DELIVER = (lineage: Lineage, session: string, now: number): string => {
    const key = quoted(lineage.key);
    const told = quoted(session);
    const untold = _untold(key);
    return [
        '.mode tabs',
        'pragma foreign_keys = on;',
        `insert into finding_delivery(finding_id, lineage_key, session_id, agent_id, channel, delivered_at) select finding_id, ${key}, ${told}, null, 'additionalContext', ${now} from open_findings where ${_PRESENT} and not exists (select 1 from json_each(delivered_on) where value = ${key}) returning 'finding', finding_id;`,
        `select 'rule', p.category, (select group_concat(value, ', ') from json_each(p.placed)) ${untold};`,
        `insert into finding_delivery(finding_id, lineage_key, session_id, agent_id, channel, delivered_at) select d.finding_id, ${key}, ${told}, d.agent_id, 'additionalContext', ${now} from finding_delivery d where d.channel = 'report' and d.agent_id in (select p.agent_id ${untold});`,
    ].join('\n');
};

// --- [READING] -------------------------------------------------------------------------

const _lines = (stdout: string): readonly string[] => stdout.split('\n').filter((text) => text !== '');

const _cells = (stdout: string): readonly (readonly string[])[] => _lines(stdout).map((text) => text.split('\t'));

const _integer = (text: string): Result<number> => (_INTEGER.test(text) ? ok(Number(text)) : fault(`${text} is not an integer`));

const _flag = (text: string): Result<boolean> => (text === '0' || text === '1' ? ok(text === '1') : fault(`${text} is not 0 or 1`));

const _isHead = (cells: readonly string[]): cells is Head => cells.length === _CELLS;

const _isCategory = (cells: readonly string[]): cells is Category => cells.length === _CATEGORY_CELLS;

const _candidateOf = ([category, sites, reported]: Category): Result<Candidate> => map(all([_integer(sites), _flag(reported)]), ([counted, told]) => ({ category, sites: counted, reported: told }));

const _candidate = (cells: readonly string[]): Result<Candidate> => (_isCategory(cells) ? _candidateOf(cells) : fault(`category line holds ${cells.length} cells`));

const _state = (chosen: Settings, [count, from, running, open, undelivered, categoryRunning, holding, rules]: Head, rest: readonly (readonly string[])[]): Result<State> =>
    map(
        all([_integer(count), _integer(from), _flag(running), _integer(open), _integer(undelivered), _flag(categoryRunning), _integer(holding), _integer(rules), all(rest.map(_candidate))]),
        ([counted, since, elsewhere, opened, waiting, categoryElsewhere, held, placed, candidates]) => ({
            edits: { trigger: chosen.edits, count: counted, from: since, running: elsewhere, holding: held },
            open: opened,
            undelivered: waiting,
            categoryRunning: categoryElsewhere,
            rules: placed,
            candidates,
        }),
    );

const state = (stdout: string, chosen: Settings): Result<State> => {
    const [head = [], ...rest] = _cells(stdout);
    return _isHead(head) ? _state(chosen, head, rest) : fault(`state line holds ${head.length} cells`);
};

// --- [DECISIONS] -----------------------------------------------------------------------

const listed = (tasks: ClassicHookInputs['Stop']['background_tasks']): Result<readonly Task[]> =>
    tasks === undefined ? fault('background_tasks absent') : ok(tasks.flatMap((task) => (task.agent_type === undefined ? [] : [{ id: task.id, agentType: task.agent_type }])));

const occupied = (tasks: readonly Task[], claims: ReadonlySet<string>): readonly string[] => [...tasks.map((task) => task.agentType), ...claims];

const due = (range: Range, busy: readonly string[], quiet: boolean): boolean =>
    range.trigger.threshold > 0 && range.count >= range.trigger.threshold && !range.running && quiet && !busy.includes(range.trigger.agent);

const dueCategories = (seen: State, chosen: Settings, busy: readonly string[], quiet: boolean): readonly Candidate[] =>
    chosen.categoryThreshold > 0 && quiet && !seen.categoryRunning && !busy.includes(chosen.categoryAgent)
        ? seen.candidates.filter((candidate) => candidate.sites >= chosen.categoryThreshold && !candidate.reported)
        : [];

const delivering = (seen: State, chosen: Settings, busy: readonly string[], quiet: boolean): boolean =>
    quiet && !seen.edits.running && !busy.includes(chosen.edits.agent) && (seen.undelivered > 0 || seen.rules > 0);

// --- [TEXT] ----------------------------------------------------------------------------

const prompt = (spawned: Spawned): string =>
    spawned.kind === 'range' ? `range ${spawned.lineage.key} ${spawned.range.from} ${spawned.to}` : `category ${spawned.category} lineage ${spawned.lineage.key}`;

const description = (spawned: Spawned): string => (spawned.kind === 'range' ? `judge ${spawned.range.trigger.kind}s` : 'judge category');

const subject = (spawned: Spawned): string => (spawned.kind === 'range' ? `${spawned.range.from}..${spawned.to}` : spawned.category);

const _many = (count: number, one: string, many: string): string => `${count} ${count === 1 ? one : many}`;

const _segment = (count: number, text: string): readonly string[] => (count === 0 ? [] : [text]);

const context = (stdout: string, branch: string): readonly string[] => {
    const rows = _cells(stdout);
    const ids = rows.flatMap(([kind, id]) => (kind === 'finding' && id !== undefined ? [id] : []));
    const rules = rows.flatMap(([kind, category, paths]) => (kind === 'rule' && category !== undefined && paths !== undefined ? [`${category} at ${paths}`] : []));
    return [
        ..._segment(ids.length, `${_many(ids.length, 'finding', 'findings')} on ${branch}, ids ${ids.join(', ')}, ${_APPLY}`),
        ..._segment(rules.length, `${_many(rules.length, 'rule', 'rules')} placed on ${branch}, ${rules.join('; ')}, ${_APPLY}`),
    ];
};

const status = (seen: State): string => {
    const waiting = seen.candidates.filter((candidate) => !candidate.reported).length;
    return [
        ..._segment(seen.edits.count, `${_many(seen.edits.count, 'file', 'files')} unjudged`),
        ..._segment(seen.edits.holding, `${_many(seen.edits.holding, 'editor', 'editors')} running`),
        ..._segment(seen.open, `${_many(seen.open, 'finding', 'findings')} open`),
        ..._segment(waiting, _many(waiting, 'recurring category', 'recurring categories')),
    ].join(' · ');
};

const resolved = (spawned: Spawned, result: AgentSpawnResult): string =>
    result.deny === undefined
        ? `spawned ${spawned.agent}${result.agentId === undefined ? '' : ` ${result.agentId}`} over ${subject(spawned)}`
        : `${spawned.agent} refused over ${subject(spawned)}: ${result.deny}`;

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Building, Candidate, Judging, Lineage, Range, Settings, Spawned, Task };
export { context, DELIVER, delivering, description, due, dueCategories, LEDGER, lineageOf, listed, occupied, prompt, REPORT, resolved, STATE, settings, state, status, subject };
