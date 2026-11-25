import { Data, type Option } from 'effect';
import type { Stmt, Word } from 'sh-syntax';
import type { Field, Origin } from '#/evaluation/value';
import type { Usage } from '#/programs/usage';

// --- [TYPES] ---------------------------------------------------------------------------

type Place = Data.TaggedEnum<{ Local: { readonly cwd: Field }; Remote: { readonly host: Field } }>;
type Program = Data.TaggedEnum<{ Builtin: { readonly name: string }; Function: { readonly name: string }; External: { readonly name: string }; Unknown: { readonly origin: Origin } }>;
type Within = Data.TaggedEnum<{ Function: { readonly name: string }; Substitution: object; Subshell: object; Eval: object; Wrapper: { readonly name: string } }>;
interface Written {
    readonly field: Field;
    readonly exact: boolean;
    readonly node: Option.Option<Word>;
}
type Flag = Usage['flags'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const Place = Data.taggedEnum<Place>();
const Program = Data.taggedEnum<Program>();
const Within = Data.taggedEnum<Within>();

// --- [MODELS] --------------------------------------------------------------------------

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
    readonly flags: readonly { readonly flag: Flag; readonly spelled: Written; readonly values: readonly Written[] }[];
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
    readonly background: boolean;
    readonly repeated: Option.Option<Loop>;
}> {
    has(...spellings: readonly string[]): boolean {
        return this.flags.some(({ flag }) => flag.spellings.some((spelling) => spellings.includes(spelling)));
    }

    values(...spellings: readonly string[]): readonly Written[] {
        return this.flags.filter(({ flag }) => flag.spellings.some((spelling) => spellings.includes(spelling))).flatMap(({ values }) => values);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invocation, Loop, Place, Program, Within, type Written };
