import { Array, Data, type Effect, Equal, Formatter, Inspectable, Match, Option, Redactable, Redacted, Reducer, Schema } from 'effect';
import type { CommandLine } from 'effect/process';
import { BinTestOperator, createSourceIndex, type File, type Node, type Redirect, RedirOperator, type Stmt, walk, type Word } from 'sh-syntax';
import type { Invocation, Place } from '#/analysis/invocation';
import { expand } from '#/evaluation/expansion';
import { type Expansion, Role, type Scope } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';

// --- [TYPES] ---------------------------------------------------------------------------

type Refusal = (typeof refusals)[number];
interface Link {
    readonly node: Node;
    readonly parent: Node | null;
    readonly prop: string | null;
    readonly index: number | null;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const refusals = ['Value', 'Position', 'Delimiter'] as const;

// --- [MODELS] --------------------------------------------------------------------------

class Fragment extends Data.Class<{ readonly source: string; readonly redacted: readonly { readonly start: number; readonly end: number; readonly shown: Redacted.Redacted<unknown> }[] }> {
    static readonly ReducerConcat = Reducer.make<Fragment>(
        (left, right) => new Fragment({ source: `${left.source}${right.source}`, redacted: [...left.redacted, ...right.redacted.map((span) => ({ ...span, start: span.start + left.source.length, end: span.end + left.source.length }))] }),
        new Fragment({ source: '', redacted: [] }),
    );

    static of(source: string, word: CommandLine.Word = source): Fragment {
        return new Fragment({ source, redacted: Redacted.isRedacted(word) ? [{ start: 0, end: source.length, shown: word }] : [] });
    }

    slice(start: number, end: number): Fragment {
        const kept = this.redacted.map((span) => ({ ...span, start: Math.max(span.start, start) - start, end: Math.min(span.end, end) - start }));
        return new Fragment({ source: this.source.slice(start, end), redacted: kept.filter((span) => span.start < span.end) });
    }

    replace(by: Fragment): Fragment {
        const hidden = by.source === '' ? [] : Array.take(this.redacted, 1).map(({ shown }) => ({ start: 0, end: by.source.length, shown }));
        return Array.isReadonlyArrayEmpty(this.redacted) ? by : new Fragment({ source: by.source, redacted: hidden });
    }

    wrap(prefix: string, suffix: string): Fragment {
        return Fragment.ReducerConcat.combineAll([Fragment.of(prefix), this, Fragment.of(suffix)]);
    }

    override toString() {
        const [rest, shown] = Array.mapAccum(this.redacted, 0, (at, { start, end, shown: hidden }) => [end, `${this.source.slice(at, start)}${Formatter.format(hidden)}`]);
        return `${shown.join('')}${this.source.slice(rest)}`;
    }
}

class Tree extends Data.Class<{
    readonly syntax: File;
    readonly located: (offset: number) => number;
    readonly links: ReadonlyMap<Node, Link>;
    readonly children: ReadonlyMap<Node | null, readonly Link[]>;
    readonly ends: ReadonlyMap<Node, number>;
    readonly heredocs: readonly Redirect[];
}> {
    // --- [STRUCTURE]

    path(node: Node): readonly Link[] {
        const linked = (held: Node | null) => Option.flatMap(Option.fromNullOr(held), (found) => Option.fromUndefinedOr(this.links.get(found)));
        return Array.unfold(
            linked(node),
            Option.map((link) => [link, linked(link.parent)] as const),
        ).toReversed();
    }

    siblings(stmt: Stmt): Option.Option<readonly Stmt[]> {
        const listed = Option.filter(Option.fromUndefinedOr(this.links.get(stmt)), ({ index }) => index !== null);
        return Option.map(listed, ({ parent, prop }) => (this.children.get(parent) ?? []).flatMap(({ node, prop: held, index }) => (node.type === 'Stmt' && held === prop && index !== null ? [node] : [])));
    }

    heredoc(word: Word): Option.Option<{ readonly redirect: Redirect; readonly expands: boolean; readonly strips: boolean }> {
        const body = Option.filter(Option.fromUndefinedOr(this.links.get(word)), ({ prop }) => prop === 'hdoc');
        const redirect = Option.flatMap(body, ({ parent }) => (parent?.type === 'Redirect' ? Option.some(parent) : Option.none()));
        return Option.map(redirect, (held) => ({ redirect: held, expands: held.word?.parts.every((part) => part.type === 'Lit' && !part.value.includes('\\')) ?? false, strips: held.op === RedirOperator.DashHdoc }));
    }

    role(word: Word): Role {
        const link = this.links.get(word);
        const placed = Match.value({ parent: link?.parent ?? null, prop: link?.prop ?? null }).pipe(
            Match.when({ parent: { type: 'Redirect', op: Match.is(RedirOperator.Hdoc, RedirOperator.DashHdoc, RedirOperator.WordHdoc) } }, () => Role.Operand()),
            Match.when({ parent: { type: 'Redirect' } }, () => Role.Redirect()),
            Match.when({ parent: { type: 'Assign' }, prop: 'value' }, () => Role.Assignment()),
            Match.when({ parent: { type: 'CaseItem' } }, () => Role.Pattern()),
            Match.when({ parent: { type: 'BinaryTest', op: Match.is(BinTestOperator.TsMatch, BinTestOperator.TsMatchShort, BinTestOperator.TsNoMatch) }, prop: 'y' }, () => Role.Pattern()),
            Match.when({ parent: { type: Match.is('CaseClause', 'BinaryTest', 'UnaryTest', 'TestClause', 'ParenTest') } }, () => Role.Operand()),
            Match.orElse(() => Role.Argument()),
        );
        return Option.match(this.heredoc(word), { onNone: () => placed, onSome: ({ expands }) => Role.Heredoc({ expands }) });
    }

    // --- [POSITIONS]

    start(node: Node): number {
        return this.located(node.start);
    }

    end(node: Node): number {
        return Match.value(node).pipe(
            Match.when({ type: 'Lit' }, (literal) => this.located(literal.start) + literal.value.length),
            Match.when({ type: 'Word' }, (word) => Option.match(Array.last(word.parts), { onNone: () => this.located(word.end), onSome: (part) => this.end(part) })),
            Match.orElse(() => this.located(node.end)),
        );
    }

    visible(node: Node): number {
        return this.located(this.ends.get(node) ?? node.end);
    }

    body(stmt: Stmt): number {
        return this.located(Math.max(stmt.start, ...Array.getSomes([stmt.cmd, ...stmt.redirs].map(Option.fromNullOr)).map((node) => this.ends.get(node) ?? node.end)));
    }
}

class Script extends Data.Class<{
    readonly text: Fragment;
    readonly dialect: Dialect;
    readonly place: Place;
    readonly scope: Scope;
    readonly tree: Tree;
    readonly braces: ReadonlyMap<Word, Word>;
    readonly invocations: Effect.Effect<readonly Invocation[], ShellError>;
}> {
    // --- [SOURCE]

    get source(): string {
        return this.text.source;
    }

    get syntax(): File {
        return this.tree.syntax;
    }

    get located(): Tree['located'] {
        return this.tree.located;
    }

    argv(...operands: readonly CommandLine.Word[]): Array.NonEmptyReadonlyArray<CommandLine.Word> {
        return Array.make(this.dialect.binary, '-c', Array.isReadonlyArrayNonEmpty(this.text.redacted) ? Redacted.make(this.source) : this.source, this.dialect.binary, ...operands);
    }

    // --- [EXPANSION]

    expand(word: Word, scope: Scope = this.scope): readonly [Expansion, Scope] {
        const { expansion, scope: next } = expand(word, scope, this.dialect, {
            braces: Option.fromUndefinedOr(this.braces.get(word)),
            role: this.tree.role(word),
            secret: (part) => this.text.redacted.some(({ start, end }) => this.located(part.start) < end && this.located(part.end) > start),
        });
        return [expansion, next];
    }

    expanded(word: Word): Option.Option<string> {
        const [expansion] = this.expand(word);
        return expansion._tag === 'Expanded' ? expansion.text : Option.none();
    }

    // --- [DISPLAY]

    override toString() {
        return this.text.toString();
    }

    toJSON() {
        return this.toString();
    }

    [Inspectable.NodeInspectSymbol]() {
        return this.toString();
    }

    [Redactable.symbolRedactable]() {
        return this.toString();
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

class ShellError extends Schema.TaggedError<ShellError, { readonly brand: unique symbol }>()('ShellError', {
    reason: Schema.TaggedUnion({
        Syntax: { offset: Schema.Option(Schema.Int), incomplete: Schema.Boolean },
        Dialect: {},
        Construction: { hole: Schema.Int, kind: Schema.Literals([...refusals, 'Escape']) },
        Edit: { offset: Schema.Int, kind: Schema.Literals([...refusals, 'Overlap']) },
        Host: {},
    }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: ShellError['reason'], subject: string) => (cause?: unknown) => ShellError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: [`${reason._tag}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const index = (source: string, syntax: File): Tree => {
    const visits: Link[] = [];
    walk(syntax, (node, parent, prop, at) => {
        visits.push({ node: Equal.byReferenceUnsafe(node), parent, prop, index: at });
    });
    const children = Map.groupBy(visits, ({ parent }) => parent);
    const ends = visits.reduceRight((held, { node }) => {
        const inner = children.get(node) ?? [];
        const shown = inner.flatMap((child) => (child.prop === 'hdoc' || child.node.type === 'Comment' ? [] : Array.fromNullishOr(held.get(child.node))));
        const own = node.end > Math.max(-1, ...inner.map((child) => child.node.end)) ? node.end : Math.max(-1, ...shown);
        return held.set(node, node.type === 'Stmt' && node.semicolon.line > 0 ? Math.max(own, node.semicolon.offset + 1) : own);
    }, new Map<Node, number>());
    const heredocs = visits.flatMap(({ node }) => (node.type === 'Redirect' && node.hdoc !== null ? [node] : []));
    return Equal.byReferenceUnsafe(new Tree({ syntax, located: createSourceIndex(source), links: new Map(visits.map((link) => [link.node, link])), children, ends, heredocs }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Fragment, index, type Link, type Refusal, Script, ShellError, Tree };
