import { Array, Equal, Glob, Match, Option, String } from 'effect';
import { type Invocation, Written } from '#/analysis/invocation';
import { Field, Matching, Origin, Piece, Unmatched } from '#/evaluation/value';
import { Usage } from '#/programs/usage';

// --- [TYPES] ---------------------------------------------------------------------------

type Flag = Usage['flags'][number];
type Slot = Usage['values'][string];
type Bound = Invocation['flags'][number];
interface Parsed {
    readonly usage: Usage;
    readonly path: readonly string[];
    readonly flags: readonly Bound[];
    readonly operands: readonly Written[];
    readonly rest: readonly Written[];
    readonly ended: boolean;
    readonly dashed: boolean;
    readonly ambiguous: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const blank = Usage.make({ name: '', aliases: [], flags: [], inherited: [], values: {}, operands: Option.none(), input: Option.none(), output: Option.none(), abbreviates: Option.none(), interspersed: Option.none(), commands: [] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const named = (after: readonly Written[]) => ({ zero: Array.head(after), operands: after.slice(1) });
const piece = (text: string) => new Written({ field: Field.known(text), exact: true, node: Option.none() });
const valued = (state: Parsed, flag: Flag, spelling: string, spelled: Written, attached: Option.Option<Written>, tail: readonly Written[]): readonly [Parsed, readonly Written[]] => {
    const ends = Option.match(state.usage.value(flag), { onNone: (): readonly string[] => [], onSome: (slot) => slot.ends });
    const [values, left] = Match.value(flag.arity).pipe(
        Match.when('none', () => [[], tail] as const),
        Match.when('optional', () => [Option.toArray(attached), tail] as const),
        Match.when('one', () => Option.match(attached, { onNone: () => [tail.slice(0, 1), tail.slice(1)] as const, onSome: (value) => [[value], tail] as const })),
        Match.orElse(() => {
            const [taken, after] = Array.span(tail, (word) => !Option.exists(word.text, (text) => ends.includes(text)));
            return [taken, after.slice(1)] as const;
        }),
    );
    return [{ ...state, flags: [...state.flags, { flag, spelling, spelled, values }] }, left];
};
const unknown = (state: Parsed, spelled: Written, name: string, attached: Option.Option<Written>, tail: readonly Written[]): readonly [Parsed, readonly Written[]] => [
    {
        ...state,
        flags: [...state.flags, { flag: { spellings: Array.of(name), arity: 'optional' as const }, spelling: name, spelled, values: Option.toArray(attached) }],
        ambiguous:
            state.ambiguous ||
            !(
                Option.isSome(attached) ||
                Array.isReadonlyArrayEmpty(tail) ||
                Option.exists(
                    Option.flatMap(Array.head(tail), ({ text }) => text),
                    (text) => text.startsWith('-'),
                )
            ),
    },
    tail,
];
const clustered = (state: Parsed, spelled: Written, letters: string, tail: readonly Written[]): readonly [Parsed, readonly Written[]] => {
    const spelling = `-${letters.slice(0, 1)}`;
    return Option.match(state.usage.flag(spelling), {
        onNone: () => unknown(state, spelled, `-${letters}`, Option.none(), tail),
        onSome: (flag) =>
            flag.arity === 'none' && letters.length > 1
                ? clustered(valued(state, flag, spelling, spelled, Option.none(), tail)[0], spelled, letters.slice(1), tail)
                : valued(
                      state,
                      flag,
                      spelling,
                      spelled,
                      Option.map(
                          Option.liftPredicate(letters.slice(1), (rest) => rest !== ''),
                          piece,
                      ),
                      tail,
                  ),
    });
};
const operand = (state: Parsed, word: Written, text: Option.Option<string>): Parsed =>
    Option.match(
        Option.flatMap(
            Option.filter(text, () => Array.isReadonlyArrayEmpty(state.operands) && !state.ended),
            (name) => Option.map(state.usage.command(name), (usage) => ({ usage, name })),
        ),
        {
            onNone: () => ({ ...state, operands: [...state.operands, word], rest: state.dashed ? [...state.rest, word] : state.rest, ended: state.ended || !Option.getOrElse(state.usage.interspersed, () => true) }),
            onSome: ({ usage, name }) => ({ ...state, usage, path: [...state.path, name] }),
        },
    );
const stepped = (state: Parsed, word: Written, tail: readonly Written[]): readonly [Parsed, readonly Written[]] => {
    const { text } = word;
    const lead = Option.isNone(text) ? (/^--[^=]+=/u.exec(word.field.prefix)?.[0] ?? '') : '';
    const written = Option.getOrElse(text, () => lead);
    const [name = '', ...parts] = written.split('=');
    const attached =
        lead === ''
            ? Option.map(
                  Option.liftPredicate(parts.join('='), () => Array.isReadonlyArrayNonEmpty(parts)),
                  piece,
              )
            : Option.some(new Written({ field: word.field.after(lead.length), exact: false, node: Option.none() }));
    return Match.value({ ended: state.ended || written === '', long: written.startsWith('--') && written.length > 2, short: written.startsWith('-') && written.length > 1 }).pipe(
        Match.when({ ended: true }, () => [operand(state, word, text), tail] as const),
        Match.when(
            () => written === '--',
            () => [{ ...state, ended: true, dashed: true }, tail] as const,
        ),
        Match.when({ long: true }, () =>
            Option.match(
                Option.orElse(state.usage.flag(name), () => Option.filter(Array.head(state.usage.abbreviated(name)), () => state.usage.abbreviated(name).length === 1)),
                {
                    onNone: () => unknown(state, word, name, attached, tail),
                    onSome: (flag) => valued(state, flag, name, word, attached, tail),
                },
            ),
        ),
        Match.when({ short: true }, () => Option.match(state.usage.flag(written), { onNone: () => clustered(state, word, written.slice(1), tail), onSome: (flag) => valued(state, flag, written, word, Option.none(), tail) })),
        Match.orElse(() => [operand(state, word, text), tail] as const),
    );
};
const parsed = (state: Parsed, words: readonly Written[]): Parsed =>
    Array.match(words, {
        onEmpty: () => state,
        onNonEmpty: (all) => {
            const [next, tail] = stepped(state, Array.headNonEmpty(all), Array.tailNonEmpty(all));
            return parsed(next, tail);
        },
    });
const placed = (word: Written, cwd: Field, tree: boolean): readonly Field[] => {
    const field = word.field.under(cwd);
    return Option.match(
        Option.filter(field.text, () => tree),
        {
            onNone: () => [field],
            onSome: (text) => [field, new Field({ pieces: [...field.pieces, Piece.Wild({ text: '/**' })], matching: Option.some(Matching.Paths({ glob: Option.some(Glob.make(`${Glob.escape(text)}/**`, { dot: true })), unmatched: Unmatched.Kept() })) })],
        },
    );
};
const bound = (usage: Option.Option<Usage>, words: readonly Written[], cwd: Field) => {
    const state = parsed({ usage: Option.getOrElse(usage, () => blank), path: [], flags: [], operands: [], rest: [], ended: false, dashed: false, ambiguous: false }, words.slice(1));
    const spelled = new Set(state.flags.flatMap(({ flag }) => flag.spellings));
    const active = (slot: Slot) => (Array.isReadonlyArrayEmpty(slot.when) || slot.when.some((spelling) => spelled.has(spelling))) && !slot.unless.some((spelling) => spelled.has(spelling));
    const slots = Option.getOrElse(state.usage.operands, () => []).filter(active);
    const [left, taken] = Array.mapAccum(slots, state.operands, (remaining, slot, index) => {
        const reserved = slots.slice(index + 1).filter(({ count }) => count === 'one').length;
        const count = Match.value(slot).pipe(
            Match.when({ roles: (roles: Slot['roles']) => roles.includes('command') }, () => remaining.length),
            Match.when({ count: 'one' }, () => Math.min(1, remaining.length)),
            Match.when({ count: 'optional' }, () => (remaining.length > reserved ? 1 : 0)),
            Match.when({ roles: (roles: Slot['roles']) => roles.includes('assignment') }, () => Array.span(remaining, (word) => Option.isSome(Option.flatMap(word.text, String.match(/^[A-Za-z_]\w*=/u))))[0].length),
            Match.orElse(() => Math.max(0, remaining.length - reserved)),
        );
        return [remaining.slice(count), { slot, words: remaining.slice(0, count) }] as const;
    });
    const operands = [...taken.flatMap(({ slot, words: held }) => held.map((word) => ({ word, roles: slot.roles }))), ...left.map((word) => ({ word, roles: [] }))];
    const valuedSlots = state.flags.flatMap(({ flag, values }) => Option.toArray(Option.map(state.usage.value(flag), (slot) => ({ slot, words: values }))));
    const filled = [...taken, ...valuedSlots];
    const stdin = !operands.some(({ word, roles }) => roles.includes('read') && !Option.contains(word.text, '-'));
    const effect = (...roles: Slot['roles']) => filled.flatMap(({ slot, words: held }) => (slot.roles.some((role) => roles.includes(role)) ? held.flatMap((word) => placed(word, cwd, slot.tree)) : []));
    const token = Option.flatMap(
        Array.findFirst(valuedSlots, ({ slot }) => slot.roles.includes('placeholder')),
        ({ words: held }) => Option.flatMap(Array.head(held), ({ text }) => text),
    );
    const lists = filled
        .filter(({ slot, words: held }) => slot.roles.includes('command') && Array.isReadonlyArrayNonEmpty(held))
        .map(({ slot, words: held }) => {
            const replaced = Array.isReadonlyArrayNonEmpty(slot.ends) ? Option.some('{}') : token;
            return held.map((word) => (Option.isSome(replaced) && Equal.equals(word.text, replaced) ? new Written({ field: Field.unknown(Origin.Input()), exact: word.exact, node: Option.none() }) : word));
        });
    const scripts = filled.flatMap(({ slot, words: held }, at) =>
        (slot.roles.includes('script') || slot.roles.includes('shell')) && Array.isReadonlyArrayNonEmpty(held)
            ? [{ text: Option.map(Option.all(held.map(({ text }) => text)), (texts) => texts.join(' ')), shell: slot.roles.includes('shell'), ...named(at < taken.length && slot.roles.includes('shell') ? [...taken.slice(at + 1).flatMap(({ words: after }) => after), ...left] : []) }]
            : [],
    );
    const shell = [...Option.getOrElse(state.usage.operands, Array.empty), ...Object.values(state.usage.values), ...Option.toArray(state.usage.input)].some(({ roles }) => roles.includes('shell'));
    const files = filled.flatMap(({ slot, words: [file, ...after] }, at) =>
        slot.roles.includes('execute') && file !== undefined ? [{ path: file.field.under(cwd), shell, zero: Option.some(file), operands: [...after, ...(at < taken.length ? [...taken.slice(at + 1).flatMap(({ words: held }) => held), ...left] : [])] }] : [],
    );
    const host = Option.flatMap(
        Array.findFirst(filled, ({ slot }) => slot.roles.includes('host')),
        ({ words: held }) => Array.head(held),
    );
    const input = Option.filter(state.usage.input, (slot) => stdin && active(slot));
    return {
        usage: Option.as(usage, state.usage),
        path: state.path,
        flags: state.flags,
        operands,
        rest: state.rest,
        reads: effect('read', 'execute'),
        writes: effect('write', 'create'),
        deletes: effect('delete'),
        opaque: Option.isNone(usage) || state.ambiguous || filled.some(({ slot }) => slot.roles.includes('code')) || Option.exists(input, (slot) => slot.roles.includes('command') || slot.roles.includes('code')),
        lists,
        scripts,
        files,
        running: filled.filter(({ slot }) => slot.roles.some((role) => role === 'shell' || role === 'script' || role === 'code')).flatMap(({ words: held }) => held),
        host,
        input,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { bound };
