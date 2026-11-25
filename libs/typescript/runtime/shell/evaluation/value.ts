import { Array, Data, Glob, HashMap, HashSet, Match, Option, Record, type Redacted } from 'effect';
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
const Matching = Data.taggedEnum<Matching>();
const Role = Data.taggedEnum<Role>();
const Binding = Data.taggedEnum<Binding>();
const separator = String.fromCodePoint(0xe0_00);
const magic = /[*?[\]()<>|#^~]/u;
const inert = (piece: Piece): piece is Extract<Piece, { readonly _tag: 'Text' | 'Wild' }> => piece._tag === 'Text' || (piece._tag === 'Wild' && !magic.test(piece.text));

// --- [MODELS] --------------------------------------------------------------------------

class Field extends Data.Class<{ readonly pieces: readonly Piece[]; readonly matching: Option.Option<Matching> }> {
    get text(): Option.Option<string> {
        return Option.flatMap(
            Option.liftPredicate(this.pieces, () => Option.isNone(this.matching)),
            (pieces) => Option.map(Option.all(pieces.map((piece) => (piece._tag === 'Text' ? Option.some(piece.text) : Option.none()))), (texts) => texts.join('')),
        );
    }

    get origins(): HashSet.HashSet<Origin> {
        return HashSet.fromIterable(this.pieces.map((piece) => (piece._tag === 'Unknown' ? piece.origin : Origin.Literal())));
    }

    get prefix(): string {
        const [known] = Array.span(this.pieces, inert);
        const next = this.pieces[known.length];
        return [...known.map(({ text }) => text), next?._tag === 'Wild' ? (next.text.split(magic)[0] ?? '') : ''].join('');
    }

    get suffix(): string {
        const [known] = Array.span(this.pieces.toReversed(), inert);
        const next = this.pieces.at(-known.length - 1);
        return [next?._tag === 'Wild' ? (next.text.split(magic).at(-1) ?? '') : '', ...known.toReversed().map(({ text }) => text)].join('');
    }

    after(count: number): Field {
        const [, pieces] = Array.mapAccum(this.pieces, count, (left, piece) =>
            Piece.$match(piece, {
                Text: ({ text }): readonly [number, readonly Piece[]] => [Math.max(0, left - text.length), text.length > left ? [Piece.Text({ text: text.slice(left) })] : []],
                Wild: ({ text }): readonly [number, readonly Piece[]] => [Math.max(0, left - text.length), text.length > left ? [Piece.Wild({ text: text.slice(left) })] : []],
                Secret: (held): readonly [number, readonly Piece[]] => [0, [held]],
                Unknown: (held): readonly [number, readonly Piece[]] => [0, [held]],
            }),
        );
        return new Field({ pieces: pieces.flat(), matching: Option.none() });
    }

    matches(candidate: string): Option.Option<boolean> {
        return Option.match(this.matching, {
            onNone: () => Option.map(this.text, (text) => text === candidate),
            onSome: Matching.$match({
                Paths: ({ glob }) => Option.map(glob, Glob.match(candidate)),
                Strings: ({ glob }) => Option.flatMap(glob, (compiled) => (candidate === '' ? Option.some(/^\**$/u.test(compiled.pattern)) : Option.liftPredicate(Glob.match(compiled, candidate.replaceAll('/', separator)), () => !/^\.\.?$/u.test(candidate) && !candidate.includes(separator)))),
            }),
        });
    }

    under(directory: Field): Field {
        const [opening] = this.pieces;
        const placed = this.prefix.startsWith('/') ? this.pieces : [...directory.pieces, Piece.Text({ text: '/' }), ...this.pieces];
        const [texts, rest] = Array.span(placed, (piece): piece is Extract<Piece, { readonly _tag: 'Text' }> => piece._tag === 'Text');
        const joined = texts.map(({ text }) => text).join('');
        const cut = Array.isReadonlyArrayEmpty(rest) ? joined.length : joined.lastIndexOf('/') + 1;
        const segments = joined
            .slice(0, cut)
            .split('/')
            .filter((segment) => segment !== '' && segment !== '.')
            .reduce<readonly string[]>((kept, segment) => (segment === '..' ? kept.slice(0, -1) : Array.append(kept, segment)), []);
        const closed = cut < joined.length || !Array.isReadonlyArrayEmpty(rest);
        const lead = joined.startsWith('/') ? `/${segments.join('/')}${closed && Array.isReadonlyArrayNonEmpty(segments) ? '/' : ''}` : joined.slice(0, cut);
        const text = `${lead}${joined.slice(cut)}`;
        const pieces = text === '' ? rest : [Piece.Text({ text }), ...rest];
        const pattern = Option.all(pieces.map((piece) => Match.value(piece).pipe(Match.tagsExhaustive({ Text: ({ text: literal }) => Option.some(Glob.escape(literal)), Wild: ({ text: wild }) => Option.some(wild.replaceAll(/[{}]/gu, String.raw`\$&`)), Secret: () => Option.none(), Unknown: () => Option.none() }))));
        const matching = Option.map(this.matching, Matching.$match({ Paths: ({ glob, unmatched }) => Matching.Paths({ glob: Option.flatMap(glob, (compiled) => Option.map(pattern, (parts) => Glob.make(parts.join(''), { dot: compiled.dot }))), unmatched }), Strings: (strings) => strings }));
        return opening?._tag === 'Unknown' || opening?._tag === 'Secret' || Option.contains(this.text, '') ? this : new Field({ pieces, matching });
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
}

class Expanded extends Data.TaggedClass('Expanded')<{ readonly fields: readonly Field[]; readonly exact: boolean; readonly refusals: readonly Origin[] }> {
    get text(): Option.Option<string> {
        return this.exact && this.fields.length === 1 ? Option.flatMap(Array.head(this.fields), (field) => field.text) : Option.none();
    }

    get binding(): Binding {
        return Option.match(this.text, { onNone: () => Binding.Unknown({ origins: HashSet.fromIterable([...this.fields.flatMap((field) => [...field.origins]), ...this.refusals]) }), onSome: (value) => Binding.Known({ value }) });
    }

    matches(candidate: string): Option.Option<boolean> {
        return this.fields.length === 1 ? Option.flatMap(Array.head(this.fields), (field) => field.matches(candidate)) : Option.none();
    }
}

class Refused extends Data.TaggedClass('Refused')<{ readonly origin: Origin }> {
    get binding(): Binding {
        return Binding.Unknown({ origins: HashSet.make(this.origin) });
    }
}

class Scope extends Data.Class<{ readonly variables: HashMap.HashMap<string, Binding>; readonly unlisted: Binding; readonly positional: Option.Option<readonly Binding[]>; readonly name: string; readonly substitutions: HashMap.HashMap<Node, readonly string[]> }> {
    get(name: string): Binding {
        return Option.getOrElse(HashMap.get(this.variables, name), () => this.unlisted);
    }

    set(name: string, binding: Binding): Scope {
        return new Scope({ ...this, variables: HashMap.set(this.variables, name, binding) });
    }

    static started(at: { readonly dialect: Dialect; readonly operands: readonly string[]; readonly pwd: Option.Option<string> }): Scope {
        const { environment, variables } = at.dialect;
        const unknown = Binding.Unknown({ origins: HashSet.empty() });
        const held = (name: string, value: Option.Option<string>) => Option.match(value, { onNone: () => unknown, onSome: (found) => (name === 'IFS' || Record.get(environment, name).pipe(Option.contains(found)) ? Binding.Known({ value: found }) : unknown) });
        const special = [
            ['?', Binding.Known({ value: '0' })],
            ['$', unknown],
            ['-', unknown],
            ['PWD', Option.match(at.pwd, { onNone: () => unknown, onSome: (value) => Binding.Known({ value }) })],
        ] as const;
        return new Scope({
            variables: HashMap.fromIterable([...variables.entries().map(([name, value]) => [name, held(name, value)] as const), ...special]),
            unlisted: Binding.Unset(),
            positional: Option.some(at.operands.map((value) => Binding.Known({ value }))),
            name: at.dialect.binary,
            substitutions: HashMap.empty(),
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Binding, Expanded, type Expansion, Field, Matching, Origin, Piece, Refused, Role, Scope, Unmatched };
