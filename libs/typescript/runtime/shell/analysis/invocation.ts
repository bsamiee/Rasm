import { Array, Data, Option } from 'effect';
import type { Stmt, Word } from 'sh-syntax';
import { Binding, type Field, type Origin } from '#/evaluation/value';
import type { Usage } from '#/programs/usage';

// --- [TYPES] ---------------------------------------------------------------------------

type Place = Data.TaggedEnum<{ Local: { readonly cwd: Field }; Remote: { readonly host: Field } }>;
type Program = Data.TaggedEnum<{ Builtin: { readonly name: string }; Function: { readonly name: string }; External: { readonly name: string }; Unknown: { readonly origin: Origin } }>;
type Within = Data.TaggedEnum<{ Function: { readonly name: string }; Substitution: object; Subshell: object; Eval: object; Wrapper: { readonly name: string }; File: { readonly path: string } }>;
type Stdin = Data.TaggedEnum<{ Inherited: object; Piped: { readonly pipeline: number; readonly index: number }; File: { readonly path: Field }; Text: { readonly text: Option.Option<string> }; Closed: object }>;
type Flag = Usage['flags'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const Place = Data.taggedEnum<Place>();
const Program = Data.taggedEnum<Program>();
const Within = Data.taggedEnum<Within>();
const Stdin = Data.taggedEnum<Stdin>();

// --- [MODELS] --------------------------------------------------------------------------

class Written extends Data.Class<{ readonly field: Field; readonly exact: boolean; readonly node: Option.Option<Word> }> {
    get text(): Option.Option<string> {
        return Option.filter(this.field.text, () => this.exact);
    }

    get binding(): Binding {
        return Option.match(this.text, { onNone: () => Binding.Unknown({ origins: this.field.origins }), onSome: (value) => Binding.Known({ value }) });
    }

    test(pattern: RegExp): boolean {
        return Option.exists(this.text, (text) => pattern.test(text));
    }

    spells(...words: readonly string[]): boolean {
        return Option.exists(this.text, (text) => words.includes(text));
    }
}

class Loop extends Data.Class<{ readonly source: string; readonly start: number; readonly end: number; readonly condition: boolean }> {}

class Invocation extends Data.Class<{
    readonly program: Program;
    readonly source: string;
    readonly start: number;
    readonly end: number;
    readonly words: readonly Written[];
    readonly node: Option.Option<Stmt>;
    readonly assigns: readonly { readonly name: string; readonly value: Written }[];
    readonly usage: Option.Option<Usage>;
    readonly path: readonly string[];
    readonly flags: readonly { readonly flag: Flag; readonly spelling: string; readonly spelled: Written; readonly values: readonly Written[] }[];
    readonly operands: readonly { readonly word: Written; readonly roles: Usage['values'][string]['roles'] }[];
    readonly rest: readonly Written[];
    readonly reads: readonly Field[];
    readonly writes: readonly Field[];
    readonly deletes: readonly Field[];
    readonly opaque: boolean;
    readonly inner: readonly Invocation[];
    readonly place: Place;
    readonly within: readonly Within[];
    readonly stage: Option.Option<{ readonly pipeline: number; readonly index: number; readonly count: number }>;
    readonly stdin: Stdin;
    readonly background: boolean;
    readonly repeated: Option.Option<Loop>;
}> {
    // --- [IDENTITY]

    get name(): Option.Option<string> {
        return Program.$match(this.program, { Unknown: () => Option.none(), Builtin: ({ name }) => Option.some(name), Function: ({ name }) => Option.some(name), External: ({ name }) => Option.some(name.slice(name.lastIndexOf('/') + 1)) });
    }

    get label(): string {
        return [...Option.toArray(this.name), ...this.path].join(' ');
    }

    get leaves(): readonly Invocation[] {
        return [this, ...this.inner.flatMap(({ leaves }) => leaves)];
    }

    runs(...names: readonly string[]): boolean {
        return Option.exists(this.name, (name) => names.includes(name));
    }

    // --- [WORDS]

    get head(): Option.Option<Word> {
        return Option.flatMap(Array.head(this.words), ({ node }) => node);
    }

    get span(): Option.Option<readonly [Word, Word]> {
        return Option.all([this.head, Option.flatMap(Array.last(this.words), ({ node }) => node)]);
    }

    operand(at = 0): Option.Option<string> {
        return Option.flatMap(Array.get(this.operands, at), ({ word }) => word.text);
    }

    has(...spellings: readonly string[]): boolean {
        return this.flags.some(({ flag }) => flag.spellings.some((spelling) => spellings.includes(spelling)));
    }

    values(...spellings: readonly string[]): readonly Written[] {
        return this.flags.filter(({ flag }) => flag.spellings.some((spelling) => spellings.includes(spelling))).flatMap(({ values }) => values);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invocation, Loop, Place, Program, Stdin, Within, Written };
