import { Array, Cache, Effect, Option, type Predicate, Record, Result, String, Struct } from 'effect';
import { type Request, Verdict } from '#/decision';
import type { HookError, Host } from '#/host';
import type { Item } from '#/shell/grammar';
import type { Program } from '#/shell/programs';
import type { Span, Word } from '#/shell/script';
import { type Analysis, Shell, type Site } from '#/shell/site';

// --- [TYPES] ---------------------------------------------------------------------------

interface Call {
    readonly request: Request;
    readonly analysis: Analysis;
    readonly host: typeof Host.Service;
}
type Verdicts = readonly Verdict[];
interface Respelling {
    readonly renames?: Record.ReadonlyRecord<string, string>;
    readonly drops?: readonly string[];
}
type Fix = (site: Site, call: Call) => Verdicts | Effect.Effect<Verdicts, HookError, Host | Shell>;
interface Policy {
    readonly programs: Readonly<Record<string, Program>>;
    readonly every?: readonly Fix[];
    readonly calls?: readonly ((call: Call) => Verdicts)[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHELL_SAFE = /^[\w@%+=:,./-]+$/u;
const BACKGROUNDED: Verdicts = [Verdict.Background({ notice: 'Call runs in the background, wait for its notice' })];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PREDICATES]

const has =
    (...names: readonly string[]): Predicate.Predicate<Site> =>
    (site) =>
        site.has(...names);

const holds =
    (pattern: RegExp, ...names: readonly string[]): Predicate.Predicate<Site> =>
    (site) =>
        (names.length === 0 ? site.operands.map(({ text }) => text) : site.values(...names)).some((value) => pattern.test(value));

// --- [WORDS]

const quote = (word: string): string => (SHELL_SAFE.test(word) ? word : `'${word.replaceAll("'", "'\\''")}'`);

const subject = ({ program, path }: Site): string => [program, ...path].join(' ');

const spoken = (words: readonly Word[]): string => words.map(({ raw }) => raw).join(' ');

const folderOf = (host: typeof Host.Service, { values, command }: Site, ...flags: readonly string[]): Option.Option<string> => values(...flags).reduce((folder, value) => host.place(value, folder), command.cwd);

// --- [NEIGHBORS]

const stages = ({ command }: Site, { analysis: { sites } }: Call) => {
    const own = Option.getOrUndefined(command.stage);
    const piped = sites.filter((other) => other.command.source === command.source && Option.exists(other.command.stage, ({ pipeline }) => pipeline === own?.pipeline));
    return { earlier: piped.filter((other) => other.command.start < command.start), later: piped.filter((other) => other.command.start > command.start) };
};

const within = ({ command }: Site, { analysis: { sites } }: Call): readonly Site[] => sites.filter(({ command: other }) => other !== command && other.source === command.source && other.start >= command.start && other.end <= command.end);

const feedsJq = (site: Site, call: Call) => stages(site, call).later.at(0)?.program === 'jq';

const redirected = ({ command }: Site, op: string, descriptor?: string, target?: string) =>
    command.redirects.filter((redirect) => redirect.op === op && (descriptor === undefined || Option.contains(redirect.descriptor, descriptor)) && (target === undefined || Option.exists(redirect.target, ({ text }) => text === target)));

// --- [VERDICTS]

const refusal = (reason: string): Verdicts => [Verdict.Refuse({ reason })];

const changed = (site: Site, { start, end }: Span, text: Option.Option<string>, before?: string, timeout?: number): Verdicts => {
    if (site.command.origin === 'call') {
        return [Verdict.Edit({ start, end, text, subject: Option.fromNullishOr(before), timeout: Option.fromNullishOr(timeout) })];
    }
    return before === undefined ? [] : refusal(`Nested ${before}, run ${Option.getOrElse(text, () => 'without it')}`);
};

const edited = (site: Site, span: Span, text: string, before?: string, timeout?: number) => changed(site, span, Option.some(`${span.prefix ?? ''}${text}`), before, timeout);

const erased = (site: Site, span: Span, before?: string, timeout?: number) => changed(site, span, Option.none(), before, timeout);

const inserted = (site: Site, at: number, text: string): Verdicts => edited(site, { start: at, end: at }, text);

const silent = (verdicts: Verdicts): Verdicts => verdicts.map((verdict) => (Verdict.$is('Edit')(verdict) ? { ...verdict, subject: Option.none() } : verdict));

const whole = (site: Site, text: string, before?: string, { start }: Span = site.head): Verdicts => {
    const { end } = Array.lastNonEmpty(site.words);
    const kept = site.command.redirects.filter((redirect) => redirect.start >= start && redirect.end <= end);
    return edited(site, { start, end }, [text, ...kept.map(({ raw }) => raw)].join(' '), before);
};

const cut = (site: Site, parts: Array.NonEmptyReadonlyArray<Span>, before?: string): Verdicts => {
    const { start } = Array.headNonEmpty(parts);
    const ends = [...site.command.words, ...site.command.redirects].map(({ end }) => end).filter((end) => end <= start);
    return erased(site, { start: Math.max(site.command.start, ...ends), end: Array.lastNonEmpty(parts).end }, before);
};

const dropped = (site: Site, before: string): Verdicts => edited(site, site.command, ':', before);

const unassign =
    (pattern: RegExp): Fix =>
    (site) =>
        [...site.outer, site]
            .flatMap(({ assigned, head }) => Array.window([...assigned, head], 2))
            .filter(([word]) => pattern.test(word.text))
            .flatMap(([word, next]) => erased(site, { start: word.start, end: next.start }, word.text.slice(0, word.text.indexOf('='))));

// --- [FIXES]

const all =
    (...fixes: readonly Fix[]) =>
    (site: Site, call: Call) =>
        Effect.map(
            Effect.forEach(fixes, (fix) => {
                const answer = fix(site, call);
                return Effect.isEffect(answer) ? answer : Effect.succeed(answer);
            }),
            (verdicts): Verdicts => verdicts.flat(),
        );

const absent =
    (fix: Fix): Fix =>
    (site, call) =>
        Effect.flatMap(
            Shell.use(({ installed }) => Cache.get(installed, site.program)),
            (installed) => all(...(installed ? [] : [fix]))(site, call),
        );

const when =
    (applies: Predicate.Predicate<Site>, ...fixes: readonly Fix[]): Fix =>
    (site, call) =>
        applies(site) ? all(...fixes)(site, call) : [];

const perSubcommand =
    (table: Record.ReadonlyRecord<string, readonly Fix[]>): Fix =>
    (site, call) =>
        all(
            ...Option.getOrElse(
                Option.flatMap(Option.firstSomeOf([Array.head(site.path), Option.map(Array.head(site.operands), Struct.get('text'))]), (name) => Record.get(table, name)),
                () => [],
            ),
        )(site, call);

const refuse =
    (fact: string, action: string, ...names: readonly string[]) =>
    (site: Site): Verdicts => {
        const named = names.filter((name) => site.has(name)).slice(0, 1);
        return names.length > 0 && named.length === 0 ? [] : refusal(`${[subject(site), ...named].join(' ')} ${fact}, ${action}`);
    };

const unresolved = refuse('folder unresolved', 'cd to a literal path');

const ensure =
    (words: string, ...waived: readonly string[]) =>
    (site: Site): Verdicts =>
        site.has(words.split(' ', 1).join(''), ...waived) ? [] : inserted(site, (site.words.find(({ text }) => text === site.path.at(-1)) ?? site.head).end, ` ${words}`);

const become =
    (text: string, notify = true) =>
    (site: Site): Verdicts =>
        edited(site, site.head, text, notify ? site.head.raw : undefined);

const spelledFlags = (kept: readonly string[], attached: readonly string[]): string => {
    const [longs, shorts] = Array.partition(
        kept,
        Result.liftPredicate(String.startsWith('--'), (name) => name.slice(1)),
    );
    const spelled = [...(shorts.length > 0 ? [`-${shorts.join('')}`] : []), ...longs].join(' ');
    return longs.length > 0 ? [spelled, ...attached].join('=') : `${spelled}${attached.join('')}`;
};

const respelled = (site: Site, { renames = {}, drops = [] }: Respelling, { names, words, values }: Item, before: string | undefined): Verdicts => {
    const kept = Array.difference(names, drops).map((name) => renames[name] ?? name);
    const valueGoes = Option.exists(Array.last(names), (name) => drops.includes(name));
    return kept.length === 0 ? cut(site, words, before) : edited(site, valueGoes ? { start: words[0].start, end: Array.lastNonEmpty(words).end } : words[0], spelledFlags(kept, words.length === 1 && !valueGoes ? values.map(({ raw }) => raw) : []), before);
};

const respell =
    (respelling: Respelling, { only, notify = false }: { readonly only?: RegExp; readonly notify?: boolean } = {}) =>
    (site: Site): Verdicts =>
        site.items
            .filter(({ names, words, values }) => names.some((name) => respelling.drops?.includes(name) === true || (respelling.renames?.[name] ?? name) !== name) && values.every(({ text }) => only?.test(text) !== false) && (words.every(({ spread }) => !spread) || notify))
            .flatMap((item) => (item.words.some(({ spread }) => spread) ? refusal(`${subject(site)} ${item.words[0].text} expands later, pass it literally`) : respelled(site, respelling, item, notify ? `${subject(site)} ${item.words[0].raw}` : undefined)));

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    absent,
    all,
    BACKGROUNDED,
    become,
    type Call,
    cut,
    dropped,
    edited,
    ensure,
    erased,
    type Fix,
    feedsJq,
    folderOf,
    has,
    holds,
    inserted,
    type Policy,
    perSubcommand,
    quote,
    type Respelling,
    redirected,
    refusal,
    refuse,
    respell,
    silent,
    spoken,
    stages,
    subject,
    unassign,
    unresolved,
    type Verdicts,
    when,
    whole,
    within,
};
