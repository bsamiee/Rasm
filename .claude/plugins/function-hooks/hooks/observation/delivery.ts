import type { AgentSpawnResult, PluginOptions } from 'claude-code';
import { fromUndefined, none, type Option, some } from '../composition.ts';
import type { Spawned } from '../state.d.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Settings {
    readonly editThreshold: number;
    readonly editAgent: string;
    readonly categoryThreshold: number;
    readonly categoryAgent: string;
}
interface State {
    readonly key: string;
    readonly count: number;
    readonly from: number;
    readonly running: number;
    readonly categoryRunning: number;
    readonly editors: number;
    readonly open: number;
    readonly undelivered: number;
    readonly rules: number;
    readonly categories: readonly { readonly category: string; readonly sites: number }[];
}
interface Delivered {
    readonly findings: readonly string[];
    readonly rules: readonly { readonly category: string; readonly paths: readonly string[] }[];
}
interface Decided {
    readonly range: boolean;
    readonly category: Option<string>;
    readonly deliver: boolean;
}
interface Request {
    readonly prompt: string;
    readonly description: string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const settings = (options: PluginOptions): Settings => ({
    editThreshold: Number(options['editThreshold']),
    editAgent: String(options['editAgent']),
    categoryThreshold: Number(options['categoryThreshold']),
    categoryAgent: String(options['categoryAgent']),
});

const decided = (seen: State, chosen: Settings, busy: readonly string[]): Decided => {
    const quiet = seen.editors === 0;
    const editing = seen.running > 0 || busy.includes(chosen.editAgent);
    const categorizing = chosen.categoryThreshold > 0 && quiet && seen.categoryRunning === 0 && !busy.includes(chosen.categoryAgent);
    return {
        range: chosen.editThreshold > 0 && seen.count >= chosen.editThreshold && quiet && !editing,
        category: categorizing ? fromUndefined(seen.categories.find(({ sites }) => sites >= chosen.categoryThreshold)?.category) : none,
        deliver: quiet && !editing && (seen.undelivered > 0 || seen.rules > 0),
    };
};

// --- [TEXT] ----------------------------------------------------------------------------

const _plural = (count: number, singular: string, plural: string): string => `${count} ${count === 1 ? singular : plural}`;
const _segment = (count: number, text: string): readonly string[] => (count === 0 ? [] : [text]);

const request = (spawned: Spawned, key: string): Request =>
    spawned.kind === 'range'
        ? { prompt: `range ${key} ${spawned.from} ${spawned.to}`, description: 'judge edits' }
        : { prompt: `category ${spawned.category} lineage ${key}`, description: 'build category rule' };

const subject = (spawned: Spawned): string => (spawned.kind === 'range' ? `${spawned.from}..${spawned.to}` : spawned.category);

const context = ({ findings, rules }: Delivered, branch: string): readonly string[] => {
    const pointer = 'use the observation skill for delivered findings';
    return [
        ..._segment(findings.length, `${_plural(findings.length, 'finding', 'findings')} on ${branch}, ids ${findings.join(', ')}, ${pointer}`),
        ..._segment(rules.length, `${_plural(rules.length, 'rule', 'rules')} placed on ${branch}, ${rules.map(({ category, paths }) => `${category} at ${paths.join(', ')}`).join('; ')}, ${pointer}`),
    ];
};

const status = (seen: State): Option<string> => {
    const text = [
        ..._segment(seen.count, `${_plural(seen.count, 'file', 'files')} unjudged`),
        ..._segment(seen.editors, `${_plural(seen.editors, 'editor', 'editors')} running`),
        ..._segment(seen.open, `${_plural(seen.open, 'finding', 'findings')} open`),
        ..._segment(seen.categories.length, _plural(seen.categories.length, 'recurring category', 'recurring categories')),
    ].join(' · ');
    return text === '' ? none : some(text);
};

const outcome = (spawned: Spawned, result: AgentSpawnResult): string => {
    const over = subject(spawned);
    return result.deny === undefined ? `spawned ${spawned.agent}${result.agentId === undefined ? '' : ` ${result.agentId}`} over ${over}` : `${spawned.agent} refused over ${over}: ${result.deny}`;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Delivered, Settings, State };
export { context, decided, outcome, request, settings, status, subject };
