import { Array, Data, Equal, Function, Glob, Match, Option, Record } from 'effect';
import { Written } from '#/analysis/invocation';
import { Field, Matching, Origin, Piece, Unmatched } from '#/evaluation/value';
import { Usage } from '#/programs/usage';

// --- [TYPES] ---------------------------------------------------------------------------

type Flag = Usage['flags'][number];
type Slot = Usage['values'][string];
interface Argument {
    readonly slot: Slot;
    readonly words: readonly Written[];
}
interface Reading extends Pick<Arguments, 'subcommands' | 'flags' | 'ambiguous'> {
    readonly usage: Usage;
    readonly operands: readonly Written[];
    readonly dashed: Option.Option<number>;
    readonly ended: boolean;
    readonly doubted: boolean;
    readonly taking: number;
    readonly ends: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const arities = { none: { attached: 0, following: 0 }, optional: { attached: 1, following: 0 }, one: { attached: 1, following: 1 }, rest: { attached: 1, following: Number.POSITIVE_INFINITY } } as const;
const counts = { one: { least: 1, most: 1 }, optional: { least: 0, most: 1 }, rest: { least: 0, most: Number.POSITIVE_INFINITY } } as const;
const effects = { reads: ['read', 'execute'], writes: ['write', 'create'], deletes: ['delete'] } as const;
const assignment = /^[A-Za-z_]\w*=/u;
const valued = /^--[^=]+=/u;

// --- [MODELS] --------------------------------------------------------------------------

class Arguments extends Data.Class<{
    readonly usage: Option.Option<Usage>;
    readonly subcommands: readonly Written[];
    readonly flags: readonly { readonly flag: Flag; readonly spelling: string; readonly spelled: Written; readonly values: readonly Written[] }[];
    readonly positional: readonly Argument[];
    readonly extras: readonly Written[];
    readonly rest: readonly Written[];
    readonly ambiguous: boolean;
    readonly cwd: Field;
}> {
    // --- [WORDS]

    get operands(): readonly { readonly word: Written; readonly roles: Slot['roles'] }[] {
        return [...this.positional.flatMap(({ slot, words }) => words.map((word) => ({ word, roles: slot.roles }))), ...this.extras.map((word) => ({ word, roles: [] }))];
    }

    get all(): readonly Argument[] {
        return gather(this);
    }

    get input(): Option.Option<Slot> {
        const fed = this.operands.some(({ word, roles }) => (roles.includes('read') && !word.spells('-')) || roles.includes('script') || roles.includes('shell'));
        return Option.filter(
            Option.flatMap(this.usage, ({ input }) => input),
            (slot) => !fed && meets(this.flags)(slot),
        );
    }

    collect(...roles: Slot['roles']) {
        return this.all.flatMap(({ slot, words }) => (Array.isReadonlyArrayNonEmpty(words) && slot.roles.some((role) => roles.includes(role)) ? [{ slot, words }] : []));
    }

    follow(slot: Slot): readonly Written[] {
        const at = this.positional.findIndex((argument) => argument.slot === slot);
        return at === -1 ? [] : [...this.positional.slice(at + 1).flatMap(({ words }) => words), ...this.extras];
    }

    // --- [EFFECTS]

    get effects(): Readonly<Record<keyof typeof effects, readonly Field[]>> {
        return touch(this);
    }

    get host(): Option.Option<Written> {
        return Option.flatMap(Array.head(this.collect('host')), ({ words }) => Array.head(words));
    }

    get opaque(): boolean {
        return Option.isNone(this.usage) || this.ambiguous || this.all.some(({ slot }) => slot.roles.includes('code')) || Option.exists(this.input, ({ roles }) => roles.includes('command') || roles.includes('code'));
    }

    // --- [CODE]

    get code(): readonly Written[] {
        return this.collect('shell', 'script', 'code', 'variable').flatMap(({ words }) => words);
    }

    get commands(): readonly (readonly Written[])[] {
        const token = Option.flatMap(Array.head(this.collect('placeholder')), ({ words }) => Option.flatMap(Array.head(words), ({ text }) => text));
        return this.collect('command').map(({ slot, words }) => {
            const hole = Array.isReadonlyArrayNonEmpty(slot.ends) ? Option.some('{}') : token;
            return words.map((word) => (Option.isSome(hole) && Equal.equals(word.text, hole) ? new Written({ field: Field.unknown(Origin.Input()), exact: word.exact, node: Option.none() }) : word));
        });
    }

    get scripts() {
        return this.collect('script', 'shell').map(({ slot, words }) => {
            const [shell, after] = [slot.roles.includes('shell'), this.follow(slot)];
            return { text: Option.map(Option.all(words.map(({ text }) => text)), Array.join(' ')), shell, zero: Option.filter(Array.head(after), () => shell), operands: shell ? after.slice(1) : [] };
        });
    }

    get files() {
        const shell = Option.exists(this.usage, ({ operands, values, input }) => [...Option.getOrElse(operands, Array.empty), ...Record.values(values), ...Option.toArray(input)].some(({ roles }) => roles.includes('shell')));
        return this.collect('execute').map(({ slot, words }) => ({ path: Array.headNonEmpty(words).field.resolve(this.cwd), shell, zero: Option.some(Array.headNonEmpty(words)), operands: [...Array.tailNonEmpty(words), ...this.follow(slot)] }));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const meets = (flags: Arguments['flags']) => {
    const spelled = (names: readonly string[]) => flags.some(({ flag }) => flag.spellings.some((spelling) => names.includes(spelling)));
    return (slot: Slot) => (Array.isReadonlyArrayEmpty(slot.when) || spelled(slot.when)) && !spelled(slot.unless);
};
const reach =
    (cwd: Field) =>
    ({ slot, words }: Argument): readonly Field[] =>
        words.flatMap((word) => {
            const field = word.field.resolve(cwd);
            const below = Option.map(
                Option.filter(field.text, () => slot.tree),
                (text) => new Field({ pieces: [...field.pieces, Piece.Wild({ text: '/**' })], matching: Option.some(Matching.Paths({ glob: Option.some(Glob.make(`${Glob.escape(text)}/**`, { dot: true })), unmatched: Unmatched.Kept() })) }),
            );
            return [field, ...Option.toArray(below)];
        });
const gather = Function.memoize((held: Arguments): readonly Argument[] => [
    ...held.positional,
    ...held.flags.flatMap(({ flag, values }) =>
        Option.toArray(
            Option.map(
                Option.flatMap(held.usage, (usage) => usage.value(flag)),
                (slot) => ({ slot, words: values }),
            ),
        ),
    ),
]);
const touch = Function.memoize((held: Arguments) => Record.map(effects, (roles) => held.collect(...roles).flatMap(reach(held.cwd))));
const open = (state: Reading, flag: Flag, spelling: string, spelled: Written, attached: Option.Option<Written>): Reading => {
    const values = Array.take(Option.toArray(attached), arities[flag.arity].attached);
    const ends = Option.match(state.usage.value(flag), { onNone: () => [], onSome: (slot) => slot.ends });
    return { ...state, flags: [...state.flags, { flag, spelling, spelled, values }], taking: arities[flag.arity].following - values.length, ends };
};
const doubt = (state: Reading, spelled: Written, name: string, attached: Option.Option<Written>): Reading => ({ ...open(state, { spellings: Array.of(name), arity: 'optional' }, name, spelled, attached), doubted: Option.isNone(attached) });
const fill = (state: Reading, word: Written): Reading => (word.spells(...state.ends) ? { ...state, taking: 0 } : { ...state, taking: state.taking - 1, flags: state.flags.map((held, at) => (at === state.flags.length - 1 ? { ...held, values: [...held.values, word] } : held)) });
const place = (state: Reading, word: Written): Reading =>
    Option.match(
        Option.flatMap(
            Option.filter(word.text, () => Array.isReadonlyArrayEmpty(state.operands) && !state.ended),
            (name) => state.usage.command(name),
        ),
        {
            onNone: () => ({ ...state, operands: [...state.operands, word], ended: state.ended || !Option.getOrElse(state.usage.interspersed, () => true) }),
            onSome: (usage) => ({ ...state, usage, subcommands: [...state.subcommands, word] }),
        },
    );
const uncluster = (state: Reading, spelled: Written, letters: readonly string[]): Reading =>
    letters.reduce(
        ({ read, open: unbound }, letter, at) =>
            unbound
                ? Option.match(read.usage.flag(`-${letter}`), {
                      onNone: () => ({ read: doubt(read, spelled, `-${letters.slice(at).join('')}`, Option.none()), open: false }),
                      onSome: (flag) => {
                          const [lone, value] = [arities[flag.arity].attached === 0, letters.slice(at + 1).join('')];
                          return { read: open(read, flag, `-${letter}`, spelled, lone || value === '' ? Option.none() : Option.some(Written.of(value))), open: lone };
                      },
                  })
                : { read, open: unbound },
        { read: state, open: true },
    ).read;
const spell = (state: Reading, word: Written): Reading => {
    const lead = Option.match(word.text, { onSome: () => '', onNone: () => valued.exec(word.field.prefix)?.[0] ?? '' });
    const written = Option.getOrElse(word.text, () => lead);
    const cut = written.indexOf('=');
    const name = cut === -1 ? written : written.slice(0, cut);
    const attached =
        lead === ''
            ? Option.map(
                  Option.liftPredicate(written.slice(cut + 1), () => cut !== -1),
                  Written.of,
              )
            : Option.some(new Written({ field: word.field.drop(lead.length), exact: false, node: Option.none() }));
    return Match.value({ flagged: !state.ended && written.startsWith('-') && written.length > 1, closing: written === '--', long: written.startsWith('--') }).pipe(
        Match.when({ flagged: false }, () => place(state, word)),
        Match.when({ closing: true }, () => ({ ...state, ended: true, dashed: Option.some(state.operands.length) })),
        Match.when({ long: true }, () => Option.match(state.usage.flag(name), { onNone: () => doubt(state, word, name, attached), onSome: (flag) => open(state, flag, name, word, attached) })),
        Match.orElse(() => Option.match(state.usage.flag(written), { onNone: () => uncluster(state, word, Array.fromIterable(written.slice(1))), onSome: (flag) => open(state, flag, written, word, Option.none()) })),
    );
};
const step = (state: Reading, word: Written): Reading => (state.taking > 0 ? fill(state, word) : spell({ ...state, doubted: false, ambiguous: state.ambiguous || (state.doubted && !word.test(/^-/u)) }, word));
const bind = (usage: Option.Option<Usage>, words: readonly Written[], cwd: Field): Arguments => {
    const read = words.slice(1).reduce(step, { usage: Option.getOrElse(usage, () => Usage.blank), subcommands: [], flags: [], operands: [], dashed: Option.none(), ended: false, ambiguous: false, doubted: false, taking: 0, ends: [] });
    const slots = Option.getOrElse(read.usage.operands, () => []).filter(meets(read.flags));
    const [extras, positional] = Array.mapAccum(slots, read.operands, (left, slot, at) => {
        const fitting = slot.roles.includes('assignment') ? Array.takeWhile(left, (word) => word.test(assignment)) : left;
        const reserved = slots.slice(at + 1).filter(({ count }) => counts[count].least > 0).length;
        const taken = Math.min(fitting.length, counts[slot.count].most, Math.max(counts[slot.count].least, fitting.length - reserved));
        return [left.slice(taken), { slot, words: left.slice(0, taken) }];
    });
    return new Arguments({ usage: Option.as(usage, read.usage), subcommands: read.subcommands, flags: read.flags, positional, extras, rest: Option.match(read.dashed, { onNone: () => [], onSome: (at) => read.operands.slice(at) }), ambiguous: read.ambiguous, cwd });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Arguments, bind };
