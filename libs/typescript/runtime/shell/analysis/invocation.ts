import { Array, Data, Match, Option } from 'effect';
import { RedirOperator, type Stmt, type Word } from 'sh-syntax';
import type { Arguments } from '#/analysis/binding';
import { Binding, Field, Origin } from '#/evaluation/value';

// --- [TYPES] ---------------------------------------------------------------------------

type Place = Data.TaggedEnum<{ Local: { readonly cwd: Field }; Remote: { readonly host: Field } }>;
type Program = Data.TaggedEnum<{ Builtin: { readonly name: string }; Function: { readonly name: string }; External: { readonly name: string }; Unknown: { readonly origin: Origin } }>;
type Within = Data.TaggedEnum<{ Function: { readonly name: string }; Substitution: object; Subshell: object; Eval: object; Wrapper: { readonly name: string }; File: { readonly path: string } }>;
type Stdin = Data.TaggedEnum<{ Inherited: object; Piped: { readonly pipeline: number; readonly index: number }; File: { readonly path: Field }; Text: { readonly text: Option.Option<string> }; Closed: object }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const Place = Data.taggedEnum<Place>();
const Program = Data.taggedEnum<Program>();
const Within = Data.taggedEnum<Within>();
const Stdin = Data.taggedEnum<Stdin>();
const modes: Partial<Record<RedirOperator, readonly ('read' | 'write' | 'append' | 'print' | 'feed' | 'duplicate')[]>> = {
    [RedirOperator.RdrIn]: ['read'],
    [RedirOperator.RdrInOut]: ['read', 'write'],
    [RedirOperator.DplIn]: ['duplicate'],
    [RedirOperator.Hdoc]: ['feed'],
    [RedirOperator.DashHdoc]: ['feed'],
    [RedirOperator.WordHdoc]: ['feed'],
    [RedirOperator.RdrOut]: ['write', 'print'],
    [RedirOperator.ClbOut]: ['write', 'print'],
    [RedirOperator.AppOut]: ['write', 'print', 'append'],
    [RedirOperator.AppClob]: ['write', 'print', 'append'],
    [RedirOperator.RdrAll]: ['write'],
    [RedirOperator.RdrAllClob]: ['write'],
    [RedirOperator.AppAll]: ['write'],
    [RedirOperator.AppAllClob]: ['write'],
};

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

    static of(text: string): Written {
        return new Written({ field: Field.known(text), exact: true, node: Option.none() });
    }
}

class Redirection extends Data.Class<{ readonly modes: NonNullable<(typeof modes)[RedirOperator]>; readonly descriptor: Option.Option<string>; readonly target: Option.Option<Field>; readonly text: Option.Option<string> }> {
    get stdin(): Stdin {
        const [copied, unknown] = [this.modes.includes('duplicate'), Field.unknown(Origin.Unsupported())];
        const path = Option.getOrElse(this.target, () => unknown);
        return Match.value({ fed: this.modes.includes('feed'), closed: Option.contains(path.text, copied ? '-' : '/dev/null') }).pipe(
            Match.when({ fed: true }, () => Stdin.Text({ text: this.text })),
            Match.when({ closed: true }, () => Stdin.Closed()),
            Match.orElse(() => Stdin.File({ path: copied ? unknown : path })),
        );
    }

    opens(descriptor: string, ...some: Redirection['modes']): boolean {
        return Option.getOrElse(this.descriptor, () => descriptor) === descriptor && some.some((mode) => this.modes.includes(mode));
    }

    static of(operator: RedirOperator, descriptor: Redirection['descriptor'], written: Redirection['target'], cwd: Field, text: Redirection['text']): Redirection {
        const held = modes[operator] ?? [];
        return new Redirection({ modes: held, descriptor, target: held.includes('duplicate') ? written : Option.map(written, (field) => field.resolve(cwd)), text });
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
    readonly arguments: Arguments;
    readonly redirections: readonly Redirection[];
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
        return this.program._tag === 'Unknown' ? Option.none() : Option.some(this.program.name);
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

    // --- [ARGUMENTS]

    get usage(): Arguments['usage'] {
        return this.arguments.usage;
    }

    get subcommands(): Arguments['subcommands'] {
        return this.arguments.subcommands;
    }

    get path(): readonly string[] {
        return Array.getSomes(this.subcommands.map(({ text }) => text));
    }

    get flags(): Arguments['flags'] {
        return this.arguments.flags;
    }

    get operands(): Arguments['operands'] {
        return this.arguments.operands;
    }

    get rest(): Arguments['rest'] {
        return this.arguments.rest;
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

    // --- [EFFECTS]

    get reads(): readonly Field[] {
        return [...this.arguments.effects.reads, ...this.redirections.flatMap((redirection) => (redirection.modes.includes('read') ? Option.toArray(redirection.target) : []))];
    }

    get writes(): readonly Field[] {
        return [...this.arguments.effects.writes, ...this.redirections.flatMap((redirection) => (redirection.modes.includes('write') ? Option.toArray(redirection.target) : []))];
    }

    get deletes(): readonly Field[] {
        return this.arguments.effects.deletes;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invocation, Loop, Place, Program, Redirection, Stdin, Within, Written };
