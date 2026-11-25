import type { Shell } from '@rasm/shell';
import { Array, type Cause, Data, type Effect, Equal, type FileSystem, Match, Option, type Path, Record, String } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Action, Kind } from '#/judgment/action';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Dialect = Parameters<Shell['Service']['read']>[1]['dialect'];
type Editor = Parameters<Parameters<Shell['Service']['edit']>[1]>[0];
type Edit = ReturnType<Editor['replace']>;
type Invocation = Effect.Success<Script['invocations']>[number];
type Written = Invocation['words'][number];
type Bound = Invocation['flags'][number];
type Tool = Exclude<Kind, 'Shell'>;
type Once = 'session' | 'agent' | 'call';
interface Refusal {
    readonly refuse: string;
    readonly instruct: string;
}
interface Note {
    readonly note: string;
    readonly once: Once;
}
interface Edited {
    readonly edit: (editor: Editor, within: (nested: Script) => Editor) => readonly Edit[];
}
type Respelling = { readonly drop: readonly string[] } | { readonly ensure: Array.NonEmptyReadonlyArray<string>; readonly unless?: readonly string[] } | { readonly rename: Readonly<Record<string, string>> };
type Settled = Refusal | Note | Edited | { readonly run: Exclude<Run, 'foreground'> };
type Verdict = Settled | Respelling;
type Needs = Shell | FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner;
type Verdicts<V> = readonly V[] | Effect.Effect<readonly V[], Cause.YieldableError, Needs>;
type Run = 'foreground' | 'background' | 'monitor';
interface Call {
    readonly action: Action;
    readonly script: Script;
    readonly cwd: string;
    readonly shell: Shell['Service'];
    readonly run: Run;
}
type Plain = (Refusal & { readonly when?: readonly string[]; readonly unless?: readonly string[] }) | Respelling;
type Rule =
    | { readonly script: true; readonly program?: never; readonly tool?: never; readonly judge: (script: Script, call: Call) => Verdicts<Settled> }
    | ({ readonly program: string; readonly aliases?: readonly string[]; readonly path?: readonly string[]; readonly script?: never; readonly tool?: never } & (Plain | { readonly judge: (invocation: Invocation, call: Call) => Verdicts<Verdict> }))
    | { [K in Tool]: Used<K> }[Tool];
type Used<K extends Tool> = { readonly tool: K; readonly script?: never; readonly program?: never } & (Refusal | { judge(action: (typeof Action)[K]['Type']): Verdicts<Refusal | Note> });
type Table = Readonly<Record<string, Rule | null>>;
interface Wanted {
    readonly kept: readonly Settled[];
    readonly drop: readonly string[];
    readonly rename: Readonly<Record<string, string>>;
    readonly ensure: readonly Extract<Respelling, { readonly ensure: unknown }>[];
}

// --- [MODELS] --------------------------------------------------------------------------

class Told extends Data.Class<{ readonly text: string; readonly once: Once; readonly by: string }> {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spelling = (written: Written) => Option.map(written.text, (text) => Array.headNonEmpty(String.split(text, '=')));
const meets =
    (names: readonly string[]) =>
    ({ flag, spelled }: Bound) =>
        flag.spellings.some((name) => names.includes(name)) || Option.exists(spelling(spelled), (name) => names.includes(name));
const shown = (invocation: Invocation, path: readonly string[], names: readonly string[]) => [...Option.toArray(Option.flatMap(Array.head(invocation.words), ({ text }) => text)), ...path, ...Array.dedupe(invocation.flags.filter(meets(names)).flatMap(({ spelled }) => Option.toArray(spelled.text)))].join(' ');
const respelling = (to: string, value: string) => (to.startsWith('--') ? [value === '' ? to : `${to}=${value}`] : [to, ...(value === '' ? [] : [value])]);
const respelled = (invocation: Invocation, path: readonly string[], wanted: Wanted): readonly ((editor: Editor) => Edit)[] => {
    const named = [...wanted.drop, ...Record.keys(wanted.rename)];
    const renamed = (bound: Bound, spelled: string) =>
        Option.flatMap(
            Array.findFirst(Record.keys(wanted.rename), (name) => bound.flag.spellings.includes(name) || name === spelled),
            (name) => Record.get(wanted.rename, name),
        );
    const placed = invocation.flags.filter(({ spelled }) => Option.isSome(spelled.node));
    const words = (Array.isReadonlyArrayNonEmpty(placed) ? Array.groupWith(placed, (left, right) => Equal.equals(left.spelled.node, right.spelled.node)) : [])
        .flatMap((group) => Option.toArray(Option.map(Option.all({ word: Array.headNonEmpty(group).spelled.node, text: Array.headNonEmpty(group).spelled.text, spelled: spelling(Array.headNonEmpty(group).spelled) }), (found) => ({ ...found, group }))))
        .flatMap(({ group, word, text, spelled }) => {
            const whole = text.startsWith('--') || [...Array.headNonEmpty(group).flag.spellings, ...named].includes(spelled);
            const letters = Array.fromIterable(text);
            const parts = (whole ? [Array.headNonEmpty(group)] : group).map((bound, index) => {
                const to = renamed(bound, spelled);
                const held = whole ? '' : [letters[1 + index], ...(index === group.length - 1 ? letters.slice(1 + group.length) : [])].join('');
                const values = Array.getSomes(bound.values.map(({ node }) => node)).filter((value) => value !== word);
                return Match.value({ dropped: meets(wanted.drop)(bound), short: !whole && Option.exists(to, (name) => /^-[^-]$/u.test(name)), to: Option.getOrElse(to, () => '') }).pipe(
                    Match.when({ dropped: true }, () => ({ held: '', said: [], values })),
                    Match.when({ short: true }, ({ to: name }) => ({ held: `${name.slice(1)}${held.slice(1)}`, said: [], values: [] })),
                    Match.when({ to: Match.nonEmptyString }, ({ to: name }) => ({ held: '', said: respelling(name, whole ? text.slice(spelled.length + 1) : held.slice(1)), values: [] })),
                    Match.orElse(() => ({ held, said: whole ? [text] : [], values: [] })),
                );
            });
            const kept = parts.map(({ held }) => held).join('');
            const result = [...(kept === '' ? [] : [`-${kept}`]), ...parts.flatMap(({ said }) => said)];
            const target = Option.getOrElse(
                Option.map(Array.last(parts.flatMap((part) => part.values)), (last) => [word, last] as const),
                () => word,
            );
            return Array.match(result, {
                onEmpty: () => [(editor: Editor) => editor.remove(target)],
                onNonEmpty: (written) => (written.length === 1 && written[0] === text && target === word ? [] : [(editor: Editor) => editor.replace(target, written)]),
            });
        });
    const anchor = Option.flatMap(
        Option.orElse(
            Option.flatMap(Array.last(path), (last) => Array.findFirst(Array.drop(invocation.words, 1), (written) => Option.contains(written.text, last))),
            () => Array.head(invocation.words),
        ),
        ({ node }) => node,
    );
    const ensured = wanted.ensure.filter(({ ensure, unless = [] }) => !invocation.flags.some(meets([Array.headNonEmpty(ensure), ...unless]))).flatMap(({ ensure }) => Option.toArray(Option.map(anchor, (word) => (editor: Editor) => editor.after(word, ensure))));
    return [...words, ...ensured];
};
const settled = (verdicts: readonly Verdict[], invocation: Invocation, path: readonly string[]): readonly Settled[] => {
    const wanted = verdicts.reduce<Wanted>(
        (held, verdict) =>
            Match.value(verdict).pipe(
                Match.when({ drop: Match.defined }, ({ drop }) => ({ ...held, drop: [...held.drop, ...drop] })),
                Match.when({ rename: Match.defined }, ({ rename }) => ({ ...held, rename: { ...held.rename, ...rename } })),
                Match.when({ ensure: Match.defined }, (ensure) => ({ ...held, ensure: [...held.ensure, ensure] })),
                Match.orElse((kept) => ({ ...held, kept: [...held.kept, kept] })),
            ),
        { kept: [], drop: [], rename: {}, ensure: [] },
    );
    const edits = respelled(invocation, path, wanted);
    return [...wanted.kept, ...(Array.isReadonlyArrayNonEmpty(edits) ? [{ edit: (editor: Editor) => edits.map((edit) => edit(editor)) }] : [])];
};
const plain = (rule: Plain, invocation: Invocation, path: readonly string[]): readonly Settled[] =>
    Match.value(rule).pipe(
        Match.when({ refuse: Match.string }, ({ refuse, instruct, when, unless = [] }) => ((when === undefined || invocation.flags.some(meets(when))) && !invocation.flags.some(meets(unless)) ? [{ refuse: `${shown(invocation, path, when ?? [])} ${refuse}.`, instruct }] : [])),
        Match.orElse((form) => settled([form], invocation, path)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Call, type Dialect, type Edit, type Editor, type Invocation, type Needs, type Note, plain, type Rule, type Run, type Script, type Settled, settled, type Table, Told, type Tool, type Used, type Verdicts };
