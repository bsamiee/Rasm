import { Array, Data, Glob, HashMap, HashSet, Match, Option, Predicate, type Redacted, String } from 'effect';
import type { Node } from 'sh-syntax';
import type { Dialect } from '#/syntax/dialect';

// --- [TYPES] ---------------------------------------------------------------------------

type Unmatched = Data.TaggedEnum<{ Kept: object; Dropped: object; Refused: object }>;
type Origin = Data.TaggedEnum<{
    Variable: { readonly name: string; readonly origins: HashSet.HashSet<Origin> };
    Substitution: { readonly programs: readonly string[] };
    Arithmetic: object;
    Home: { readonly user: Option.Option<string> };
    Command: { readonly name: string };
    Glob: { readonly text: string };
    Braces: object;
    Bytes: object;
    Unsupported: object;
    Ambiguous: object;
    Input: object;
    Script: object;
    File: object;
    Recursion: object;
    Literal: object;
}>;
type Piece = Data.TaggedEnum<{
    Text: { readonly text: string };
    Wild: { readonly text: string };
    Secret: { readonly text: Redacted.Redacted<string> };
    Unknown: { readonly origin: Origin };
}>;
type Matching = Data.TaggedEnum<{ Paths: { readonly glob: Option.Option<Glob.Glob>; readonly unmatched: Unmatched }; Strings: { readonly glob: Option.Option<Glob.Glob> } }>;
type Role = Data.TaggedEnum<{ Argument: object; Assignment: object; Heredoc: { readonly expands: boolean }; Pattern: object; Operand: object; Redirect: object }>;
type Binding = Data.TaggedEnum<{ Unset: object; Known: { readonly value: string }; Listed: { readonly items: readonly string[] }; Unknown: { readonly origins: HashSet.HashSet<Origin> } }>;
type Expansion = Expanded | Refused;

// --- [CONSTANTS] -----------------------------------------------------------------------

const Unmatched = Data.taggedEnum<Unmatched>();
const Origin = Data.taggedEnum<Origin>();
const Piece = Data.taggedEnum<Piece>();
const Role = Data.taggedEnum<Role>();
const separator = '';
const magic = /[*?[\]()<>|#^~]/u;
const Matching = Data.taggedEnum<Matching>();
const Binding = Data.taggedEnum<Binding>();
const binding = (text: Option.Option<string>, origins: Iterable<Origin> = []): Binding => Option.match(text, { onNone: () => Binding.Unknown({ origins: HashSet.fromIterable(origins) }), onSome: (value) => Binding.Known({ value }) });
const known = (held: Binding): Option.Option<string> => Option.map(Option.liftPredicate(held, Binding.$is('Known')), ({ value }) => value);
const trace = (held: Binding): HashSet.HashSet<Origin> => (held._tag === 'Unknown' ? held.origins : HashSet.make(Origin.Literal()));

// --- [MODELS] --------------------------------------------------------------------------

class Field extends Data.Class<{ readonly pieces: readonly Piece[]; readonly matching: Option.Option<Matching> }> {
    get text(): Option.Option<string> {
        return Option.map(Option.liftPredicate(this.pieces, Array.every(Piece.$is('Text'))), (texts) => texts.map(({ text }) => text).join(''));
    }

    get origins(): HashSet.HashSet<Origin> {
        return HashSet.fromIterable(this.pieces.map((piece) => (piece._tag === 'Unknown' ? piece.origin : Origin.Literal())));
    }

    get prefix(): string {
        return Field.#edge(this.pieces, Array.headNonEmpty).join('');
    }

    get suffix(): string {
        return Field.#edge(this.pieces.toReversed(), Array.lastNonEmpty).toReversed().join('');
    }

    matches(candidate: string): Option.Option<boolean> {
        return Option.match(this.matching, {
            onNone: () => Option.map(this.text, (text) => text === candidate),
            onSome: Matching.$match({
                Paths: ({ glob }) => Option.map(glob, Glob.match(candidate)),
                Strings: ({ glob }) =>
                    Option.map(
                        Option.filter(glob, () => !/^\.\.?$/u.test(candidate) && !candidate.includes(separator)),
                        (held) => (candidate === '' ? /^\**$/u.test(held.pattern) : Glob.match(held, candidate.replaceAll('/', separator))),
                    ),
            }),
        });
    }

    drop(count: number): Field {
        const [, pieces] = Array.mapAccum(this.pieces, count, (left, piece): readonly [number, readonly Piece[]] => (piece._tag === 'Text' || piece._tag === 'Wild' ? [Math.max(0, left - piece.text.length), piece.text.length > left ? [Piece[piece._tag]({ text: piece.text.slice(left) })] : []] : [0, [piece]]));
        return new Field({ pieces: pieces.flat(), matching: Option.none() });
    }

    resolve(directory: Field): Field {
        const [own, tail] = Array.span(this.pieces, Piece.$is('Text'));
        const rooted = this.prefix.startsWith('/');
        const [texts, rest] = rooted ? [own, tail] : Array.span([...directory.pieces, Piece.Text({ text: '/' }), ...this.pieces], Piece.$is('Text'));
        const joined = texts.map(({ text }) => text).join('');
        const cut = Array.isReadonlyArrayEmpty(rest) ? joined.length : joined.lastIndexOf('/') + 1;
        const segments = joined
            .slice(0, cut)
            .split('/')
            .filter((segment) => segment !== '' && segment !== '.')
            .reduce<readonly string[]>((kept, segment) => (segment === '..' ? kept.slice(0, -1) : Array.append(kept, segment)), []);
        const closing = (cut < joined.length || Array.isReadonlyArrayNonEmpty(rest)) && Array.isReadonlyArrayNonEmpty(segments) ? '/' : '';
        const text = `${joined.startsWith('/') ? `/${segments.join('/')}${closing}` : joined.slice(0, cut)}${joined.slice(cut)}`;
        const head = Glob.escape(own.map((piece) => piece.text).join('')).length;
        const whole = rooted || directory.pieces.every(Piece.$is('Text'));
        const matching = Option.map(
            this.matching,
            Matching.$match({
                Paths: ({ glob, unmatched }) =>
                    Matching.Paths({
                        glob: Option.map(
                            Option.filter(glob, () => whole),
                            (held) => Glob.make(`${Glob.escape(text)}${held.pattern.slice(head)}`, { dot: held.dot }),
                        ),
                        unmatched,
                    }),
                Strings: (strings) => strings,
            }),
        );
        return Option.exists(Array.head(this.pieces), Predicate.or(Piece.$is('Unknown'), Piece.$is('Secret'))) || Option.contains(this.text, '') ? this : new Field({ pieces: text === '' ? rest : [Piece.Text({ text }), ...rest], matching });
    }

    static known(text: string): Field {
        return new Field({ pieces: [Piece.Text({ text })], matching: Option.none() });
    }

    static unknown(origin: Origin): Field {
        return new Field({ pieces: [Piece.Unknown({ origin })], matching: Option.none() });
    }

    static strings(pattern: string): Glob.Glob {
        return Glob.make(pattern.replaceAll('/', separator), { dot: true });
    }

    static #edge(pieces: readonly Piece[], cut: (parts: Array.NonEmptyReadonlyArray<string>) => string): readonly string[] {
        const [inert, rest] = Array.span(pieces, (piece) => piece._tag === 'Text' || (piece._tag === 'Wild' && !magic.test(piece.text)));
        return [...inert, ...Array.take(rest, 1)].flatMap(Piece.$match({ Text: ({ text }) => [text], Wild: ({ text }) => [cut(String.split(text, magic))], Secret: () => [], Unknown: () => [] }));
    }
}

class Expanded extends Data.TaggedClass('Expanded')<{ readonly fields: readonly Field[]; readonly exact: boolean; readonly refusals: readonly Origin[] }> {
    get text(): Option.Option<string> {
        return Option.flatMap(
            Option.filter(this.#only, () => this.exact),
            (field) => field.text,
        );
    }

    get binding(): Binding {
        return binding(this.text, [...this.fields.flatMap((field) => [...field.origins]), ...this.refusals]);
    }

    matches(candidate: string): Option.Option<boolean> {
        return Option.flatMap(this.#only, (field) => field.matches(candidate));
    }

    get #only(): Option.Option<Field> {
        return Option.map(Option.liftPredicate(this.fields, Predicate.isTupleOf(1)), Array.headNonEmpty);
    }
}

class Refused extends Data.TaggedClass('Refused')<{ readonly origin: Origin }> {
    get binding(): Binding {
        return binding(Option.none(), [this.origin]);
    }
}

class Scope extends Data.Class<{ readonly variables: HashMap.HashMap<string, Binding>; readonly unlisted: Binding; readonly positional: Option.Option<readonly Binding[]>; readonly name: string; readonly substitutions: HashMap.HashMap<Node, readonly string[]> }> {
    get(name: string): Binding {
        return Match.value(name).pipe(
            Match.when('0', () => Binding.Known({ value: this.name })),
            Match.when('#', () => binding(Option.map(this.positional, (given) => `${given.length}`))),
            Match.when(Match.is('@', '*'), () =>
                Option.match(
                    Option.flatMap(this.positional, (given) => Option.all(given.map(known))),
                    { onNone: () => Binding.Unknown({ origins: HashSet.fromIterable(Option.getOrElse(this.positional, Array.empty<Binding>).flatMap((held) => [...trace(held)])) }), onSome: (items) => Binding.Listed({ items }) },
                ),
            ),
            Match.when(
                (held: string) => /^[1-9]\d*$/u.test(held),
                (place) => Option.match(this.positional, { onNone: () => binding(Option.none()), onSome: (given) => given[Number(place) - 1] ?? Binding.Unset() }),
            ),
            Match.orElse(() => Option.getOrElse(HashMap.get(this.variables, name), () => this.unlisted)),
        );
    }

    text(name: string): Option.Option<string> {
        return known(this.get(name));
    }

    set(name: string, held: Binding): Scope {
        return new Scope({ ...this, variables: HashMap.set(this.variables, name, held) });
    }

    static start(at: { readonly dialect: Dialect; readonly operands: readonly string[]; readonly pwd: Option.Option<string> }): Scope {
        const { binary, environment, variables } = at.dialect;
        const probed = variables.entries().map(([name, value]) => [name, binding(Option.filter(value, (found) => name === 'IFS' || environment[name] === found))] as const);
        return new Scope({
            variables: HashMap.fromIterable([...probed, ['?', Binding.Known({ value: '0' })], ['$', binding(Option.none())], ['-', binding(Option.none())], ['PWD', binding(at.pwd)]]),
            unlisted: Binding.Unset(),
            positional: Option.some(at.operands.map((value) => Binding.Known({ value }))),
            name: binary,
            substitutions: HashMap.empty(),
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Binding, binding, Expanded, type Expansion, Field, Matching, Origin, Piece, Refused, Role, Scope, trace, Unmatched };
