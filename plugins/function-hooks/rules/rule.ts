import { Array, Effect, Function, Option, type Predicate, Record, Result, String, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import type { Host, HostError } from '#/host';
import { type Request, Verdict } from '#/rules/decision';
import type { Argument } from '#/shell/arguments';
import type { Analysis, Invocation, Parser } from '#/shell/invocation';
import type { Program } from '#/shell/programs';
import type { Span, Word } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

interface Call {
    readonly request: typeof Request.Type;
    readonly analysis: Analysis;
    readonly host: Host['Service'];
    readonly programs: ReadonlyMap<string, Ruleset['programs'][string]>;
}
type Rule = (invocation: Invocation, call: Call) => readonly Verdict[] | Effect.Effect<readonly Verdict[], HostError, Host | Parser>;
interface Ruleset {
    readonly programs: Readonly<Record<string, Program & { readonly rules?: readonly Rule[] }>>;
    readonly every?: readonly Rule[];
    readonly calls?: readonly ((call: Call) => readonly Verdict[])[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PREDICATES]

const holds =
    (pattern: RegExp, ...names: readonly string[]): Predicate.Predicate<Invocation> =>
    (invocation) =>
        (names.length === 0 ? invocation.operands.map(({ text }) => text) : invocation.values(...names)).some((value) => pattern.test(value));

// --- [WORDS]

const quote = Function.flow(CommandLine.quote, Result.getOrThrow);

const commandOf = ({ program, path }: Invocation): string => [program, ...path].join(' ');

const spelled = (words: readonly Word[]): string => words.map(({ raw }) => raw).join(' ');

const folderOf = (host: Host['Service'], { values, command }: Invocation, ...flags: readonly string[]): Option.Option<string> => values(...flags).reduce((folder, value) => host.resolve(value, folder), command.cwd);

const deleted = (invocation: Invocation, { host }: Call, ...base: readonly string[]) => {
    const walker = invocation.row.recursive === undefined ? invocation.outer.findLast(({ row }) => row.recursive !== undefined) : invocation;
    return walker === undefined
        ? { walker, words: invocation.inputs, start: folderOf(host, invocation, ...base), patterns: [] }
        : { walker, words: walker.roots, start: folderOf(host, walker, ...(walker.row.recursive?.base ?? [])), patterns: [...walker.leading.map(({ text }) => text), ...walker.values(...(walker.row.recursive?.names ?? []))] };
};

// --- [PIPELINES]

const stages = ({ command }: Invocation, { analysis: { invocations } }: Call) => {
    const own = Option.getOrUndefined(command.stage);
    const piped = invocations.filter((other) => other.command.source === command.source && Option.exists(other.command.stage, ({ pipeline }) => pipeline === own?.pipeline));
    return { earlier: piped.filter((other) => other.command.start < command.start), later: piped.filter((other) => other.command.start > command.start) };
};

const within = ({ command }: Invocation, { analysis: { invocations } }: Call): readonly Invocation[] => invocations.filter(({ command: other }) => other !== command && other.source === command.source && other.start >= command.start && other.end <= command.end);

const feedsJq = (invocation: Invocation, call: Call) => stages(invocation, call).later.at(0)?.program === 'jq';

const redirected = ({ command }: Invocation, op: string, descriptor?: string, target?: string) =>
    command.redirects.filter((redirect) => redirect.op === op && (descriptor === undefined || Option.contains(redirect.descriptor, descriptor)) && (target === undefined || Option.exists(redirect.target, ({ text }) => text === target)));

// --- [VERDICTS]

const refusal = (reason: string): readonly Verdict[] => [Verdict.Refuse({ reason })];

const changed = ({ command }: Invocation, { start, end, field }: Span, text: Option.Option<string>, before?: string, timeout?: number): readonly Verdict[] => {
    if (command.origin === 'call') {
        return [Verdict.Edit({ start, end, text, subject: Option.fromNullishOr(before), timeout: Option.fromNullishOr(timeout), ...(field === undefined ? {} : { field }) })];
    }
    return before === undefined ? [] : refusal(`Nested ${before}, run ${Option.getOrElse(text, () => 'without it')}`);
};

const edited = (invocation: Invocation, span: Span, text: string, before?: string) => changed(invocation, span, Option.some([span.prefix, text].join('')), before);

const erased = (invocation: Invocation, span: Span, before?: string, timeout?: number) => changed(invocation, span, Option.none(), before, timeout);

const inserted = (invocation: Invocation, at: number, text: string) => edited(invocation, { start: at, end: at }, text);

const silent = (verdicts: readonly Verdict[]): readonly Verdict[] => verdicts.map((verdict) => (Verdict.$is('Edit')(verdict) ? { ...verdict, subject: Option.none() } : verdict));

const whole = (invocation: Invocation, text: string, before?: string, { start }: Span = invocation.head) => {
    const { end } = Array.lastNonEmpty(invocation.words);
    const kept = invocation.command.redirects.filter((redirect) => redirect.start >= start && redirect.end <= end);
    return edited(invocation, { start, end }, [text, ...kept.map(({ raw }) => raw)].join(' '), before);
};

const cut = (invocation: Invocation, parts: Array.NonEmptyReadonlyArray<Span>, before?: string) => {
    const first = Array.headNonEmpty(parts);
    if (parts.length === 1 && first.field !== undefined) {
        return erased(invocation, first, before);
    }
    const { command } = invocation;
    const ends = [...command.words, ...command.redirects].map(({ end }) => end).filter((end) => end <= first.start);
    return erased(invocation, { start: Math.max(command.start, ...ends), end: Array.lastNonEmpty(parts).end }, before);
};

// --- [RULES]

const all =
    (...rules: readonly Rule[]) =>
    (invocation: Invocation, call: Call) =>
        Effect.map(
            Effect.forEach(rules, (rule) => {
                const verdicts = rule(invocation, call);
                return Effect.isEffect(verdicts) ? verdicts : Effect.succeed(verdicts);
            }),
            (verdicts) => verdicts.flat(),
        );

const when =
    (applies: Predicate.Predicate<Invocation>, ...rules: readonly Rule[]): Rule =>
    (invocation, call) =>
        applies(invocation) ? all(...rules)(invocation, call) : [];

const absent =
    (rule: Rule): Rule =>
    (invocation, call) =>
        Effect.flatMap(call.host.onPath(invocation.program), (found) => all(...(found ? [] : [rule]))(invocation, call));

const perSubcommand =
    (table: Record.ReadonlyRecord<string, readonly Rule[]>): Rule =>
    (invocation, call) =>
        all(
            ...Option.getOrElse(
                Option.flatMap(Option.firstSomeOf([Array.head(invocation.path), Option.map(Array.head(invocation.operands), Struct.get('text'))]), (name) => Record.get(table, name)),
                () => [],
            ),
        )(invocation, call);

const refuse =
    (fact: string, action: string, ...names: readonly string[]) =>
    (invocation: Invocation) => {
        const named = names.filter((name) => invocation.has(name)).slice(0, 1);
        return names.length > 0 && named.length === 0 ? [] : refusal(`${[commandOf(invocation), ...named].join(' ')} ${fact}, ${action}`);
    };

const unresolved = refuse('folder unresolved', 'cd to a literal path');

const unassign =
    (pattern: RegExp): Rule =>
    (invocation) =>
        [...invocation.outer, invocation]
            .flatMap(({ assigned, head }) => Array.window([...assigned, head], 2))
            .filter(([word]) => pattern.test(word.text))
            .flatMap(([word, next]) => erased(invocation, { start: word.start, end: next.start }, word.text.slice(0, word.text.indexOf('='))));

const ensure =
    (words: Array.NonEmptyReadonlyArray<string>, ...waived: readonly string[]) =>
    (invocation: Invocation) =>
        invocation.has(words[0], ...waived) ? [] : inserted(invocation, (invocation.words.find(({ text }) => text === invocation.path.at(-1)) ?? invocation.head).end, ` ${words.join(' ')}`);

const respelledFlags = (kept: readonly string[], attached: readonly string[]): string => {
    const [longs, shorts] = Array.partition(
        kept,
        Result.liftPredicate(String.startsWith('--'), (name) => name.slice(1)),
    );
    const flags = [...(shorts.length > 0 ? [`-${shorts.join('')}`] : []), ...longs].join(' ');
    return longs.length > 0 ? [flags, ...attached].join('=') : `${flags}${attached.join('')}`;
};

const respell =
    ({ renames = {}, drops = [], values, only, notify = false }: { readonly renames?: Record.ReadonlyRecord<string, string>; readonly drops?: readonly string[]; readonly values?: (word: Word, names: readonly string[]) => string; readonly only?: RegExp; readonly notify?: boolean }) =>
    (invocation: Invocation) => {
        const respelled = ({ names, words, values: held }: Argument, raws: readonly string[]) => {
            const before = notify ? `${commandOf(invocation)} ${words[0].raw}` : undefined;
            const kept = Array.difference(names, drops).map((name) => renames[name] ?? name);
            const valueGoes = Option.exists(Array.last(names), (name) => drops.includes(name));
            const attached = words.length === 1;
            if (words.some(({ spread }) => spread)) {
                return refusal(`${commandOf(invocation)} ${words[0].text} expands later, pass it literally`);
            }
            if (kept.length === 0) {
                return words.flatMap((word) => cut(invocation, [word], before));
            }
            return [
                ...edited(invocation, words[0], respelledFlags(kept, attached && !valueGoes ? raws : []), before),
                ...(valueGoes ? words.slice(1).flatMap((word) => cut(invocation, [word], before)) : []),
                ...(attached || valueGoes ? [] : Array.zip(held, raws).flatMap(([word, raw]) => (raw === word.raw ? [] : edited(invocation, word, raw, before)))),
            ];
        };
        return invocation.args
            .map((argument) => [argument, argument.values.map((word) => values?.(word, argument.names) ?? word.raw)] as const)
            .filter(([{ names, words, values: held }, raws]) => (names.some((name) => drops.includes(name) || (renames[name] ?? name) !== name) || raws.some((raw, at) => raw !== held[at]?.raw)) && held.every(({ text }) => only?.test(text) !== false) && (words.every(({ spread }) => !spread) || notify))
            .flatMap(([argument, raws]) => respelled(argument, raws));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { absent, all, type Call, commandOf, cut, deleted, edited, ensure, erased, feedsJq, folderOf, holds, inserted, perSubcommand, quote, type Rule, type Ruleset, redirected, refusal, refuse, respell, silent, spelled, stages, unassign, unresolved, when, whole, within };
