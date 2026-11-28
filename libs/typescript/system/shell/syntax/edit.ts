import { Array, Equal, Match, Option, Redacted, Result } from 'effect';
import type { CommandLine } from 'effect/process';
import type { Assign, BinaryCmd, Lit, Node, Redirect, SglQuoted, Stmt, Word, WordPart } from 'sh-syntax';
import { unquote } from '#/evaluation/expansion';
import { Content, grammarOf, hole, keeps, quote, Quoting, quotingAt, type Refused, refusal, shapeOf } from '#/syntax/construction';
import { identifier } from '#/syntax/dialect';
import { Fragment, Script, ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Joint = keyof typeof joints;
type Replacement = Script | Array.NonEmptyReadonlyArray<CommandLine.Word>;
type Edit = Result.Result<Array.NonEmptyReadonlyArray<Change>, Rejected>;
interface Rejected extends Refused {
    readonly offset: number;
}
interface Change {
    readonly start: number;
    readonly end: number;
    readonly fragment: Fragment;
}
interface Unit {
    readonly start: number;
    readonly end: number;
    readonly container: Lit | SglQuoted;
    readonly text: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const joints = {
    ';': { text: '; ', separates: true, pipes: false },
    '&': { text: ' & ', separates: true, pipes: false },
    '&&': { text: ' && ', separates: false, pipes: false },
    '||': { text: ' || ', separates: false, pipes: false },
    '|': { text: ' | ', separates: false, pipes: true },
    '|&': { text: ' |& ', separates: false, pipes: true },
} as const;
const emptiable: ReadonlySet<Node['type']> = new Set(['File', 'CmdSubst', 'ProcSubst', 'CaseItem']);

// --- [MODELS] --------------------------------------------------------------------------

class Editor {
    readonly script: Script;

    constructor(script: Script) {
        this.script = script;
    }

    // --- [WORDS]

    #quoting(target: Word | WordPart): Option.Option<Quoting> {
        const { tree, source, dialect } = this.script;
        const parent = tree.links.get(target)?.parent ?? null;
        return target.type === 'Word' || parent === null ? quotingAt(tree, target, () => hole, '', dialect) : quotingAt(tree, parent, (word) => shapeOf(word, target), source.slice(tree.start(parent), tree.start(target)), dialect);
    }

    #argument(word: Word, content: Replacement): Result.Result<Fragment, Refused> {
        const whole = Option.filter(this.#quoting(word), (quoting) => quoting._tag === 'Unquoted' && quoting.whole);
        return Result.flatMap(contentOf(content), (held) => quote(whole, held, this.script.dialect));
    }

    replace(target: Word | WordPart | readonly [Word, Word], content: Replacement): Edit {
        const { tree, source, dialect } = this.script;
        const [first, last] = isSpan(target) ? target : [target, target];
        const [start, end] = [tree.start(first), tree.end(last)];
        const body = Array.findLast(tree.path(first), ({ node }) => (node.type === 'Word' ? Option.some(node) : Option.none()));
        const adjacent = tree.links.get(first)?.parent === tree.links.get(last)?.parent;
        return refuse(start)(
            Result.gen(this, function* () {
                const held = yield* adjacent ? contentOf(content) : Result.fail(refusal('Position')());
                const fragment = yield* quote(this.#quoting(first), held, dialect);
                const kept = Option.match(body, {
                    onNone: () => true,
                    onSome: (word) => {
                        const [from, to] = [tree.start(word), tree.end(word)];
                        return keeps(tree, word, dialect, `${source.slice(from, start)}${fragment.source}${source.slice(end, to)}`, [[start - from, start - from + fragment.source.length]]);
                    },
                });
                return yield* kept ? Result.succeed(Array.of({ start, end, fragment })) : Result.fail(refusal('Delimiter')());
            }),
        );
    }

    before(word: Word, content: Replacement): Edit {
        const at = this.script.tree.start(word);
        return refuse(at)(Result.map(this.#argument(word, content), (fragment) => Array.of(insert(at, fragment.wrap('', ' ')))));
    }

    after(word: Word, content: Replacement): Edit {
        const at = this.script.tree.end(word);
        return refuse(at)(Result.map(this.#argument(word, content), (fragment) => Array.of(insert(at, fragment.wrap(' ', '')))));
    }

    assign(stmt: Stmt, name: string, value: CommandLine.Word): Edit {
        const { tree, dialect } = this.script;
        const head = stmt.cmd?.type === 'CallExpr' ? Array.head(stmt.cmd.args) : Option.none();
        return refuse(tree.start(stmt))(
            Result.gen(function* () {
                const word = yield* Result.fromOption(head, refusal('Position'));
                const named = yield* Result.liftPredicate(name, (given) => identifier.test(given), refusal('Value'));
                const fragment = yield* quote(Option.some(Quoting.Unquoted({ whole: false, command: false })), Content.Words({ words: [value], listed: false }), dialect);
                return Array.of(insert(tree.start(word), fragment.wrap(`${named}=`, ' ')));
            }),
        );
    }

    // --- [COMMANDS]

    #bodies(node: Node, after: number): readonly Change[] {
        const { tree, source } = this.script;
        const inside = tree.heredocs.filter(({ start }) => start >= node.start && start < (tree.ends.get(node) ?? node.end));
        return inside.flatMap(({ hdoc }) => {
            const [from, to] = hdoc === null ? [-1, -1] : [tree.start(hdoc), tree.located(hdoc.end)];
            return from >= after ? [{ start: from, end: to + (source[to] === '\n' ? 1 : 0), fragment: Fragment.of('') }] : [];
        });
    }

    #place(stmt: Stmt, fragment: Fragment, at: readonly number[], changes: Array.NonEmptyReadonlyArray<Change>): Edit {
        const { tree } = this.script;
        const joined = Option.isSome(tree.siblings(stmt)) || tree.links.get(stmt)?.parent?.type === 'BinaryCmd';
        const pending = (point: number) => tree.heredocs.some(({ start, hdoc }) => tree.located(start) < point && hdoc !== null && tree.start(hdoc) > point);
        return refuse(tree.start(stmt))(joined && !(fragment.source.includes('\n') && at.some(pending)) ? Result.succeed(changes) : Result.fail(refusal('Position')()));
    }

    #excise(target: Word | readonly [Word, Word] | Redirect | Assign): Edit {
        const { tree, dialect } = this.script;
        const [first, last] = isSpan(target) ? target : [target, target];
        const stmt = Array.findLast(tree.path(first).slice(0, -1), ({ node }) => (node.type === 'Stmt' ? Option.some(node) : Option.none()));
        return refuse(tree.start(first))(
            Result.gen(this, function* () {
                const { cmd, redirs } = yield* Result.fromOption(stmt, refusal('Position'));
                const args = cmd?.type === 'CallExpr' ? cmd.args : [];
                const elements = [...(cmd?.type === 'CallExpr' ? [...cmd.assigns, ...cmd.args] : Array.fromNullishOr(cmd)), ...redirs].toSorted((left, right) => left.start - right.start);
                const [from, to] = [elements.indexOf(first), elements.indexOf(last)];
                const removed: readonly Node[] = elements.slice(from, to + 1);
                const remaining = args.filter((word) => !removed.includes(word));
                const run = !isSpan(target) || removed.every((node) => node.type === 'Word' && args.includes(node));
                yield* Result.liftPredicate(removed, (nodes) => from >= 0 && to >= from && run && elements.length > nodes.length && (args.length === 0 || remaining.length > 0), refusal('Position'));
                const head = Option.filter(Array.head(remaining), () => Option.exists(Array.head(args), (program) => removed.includes(program)));
                const leading = Option.some(Quoting.Unquoted({ whole: true, command: true }));
                const respelled = yield* Result.transposeMapOption(head, (word) =>
                    Option.match(unquote(word, dialect), {
                        onNone: () => Result.liftPredicate<readonly Change[], Refused>([], () => word.parts[0]?.type !== 'Lit', refusal('Position')),
                        onSome: (literal) => Result.map(quote(leading, Content.Words({ words: [literal], listed: true }), dialect), (fragment) => [{ start: tree.start(word), end: tree.end(word), fragment }]),
                    }),
                );
                const [before, after] = [elements[from - 1], elements[to + 1]];
                const start = before === undefined ? tree.start(first) : tree.visible(before);
                const end = before === undefined && after !== undefined ? tree.start(after) : tree.visible(last);
                return Array.prepend([...Option.getOrElse(respelled, Array.empty<Change>), ...removed.flatMap((node) => this.#bodies(node, end))], { start, end, fragment: Fragment.of('') });
            }),
        );
    }

    #drop(stmt: Stmt): Edit {
        const { tree, source } = this.script;
        const link = tree.links.get(stmt);
        const listed = tree.siblings(stmt);
        const [start, end] = [tree.start(stmt), tree.visible(stmt)];
        const comments = stmt.comments.map((comment) => [tree.start(comment), tree.located(comment.end)] as const);
        const [from, to] = [Math.min(start, ...comments.map(([low]) => low)), Math.max(end, ...comments.map(([, high]) => high))];
        const opening = source.slice(0, from).lastIndexOf('\n') + 1;
        const closing = Option.getOrElse(
            Option.liftPredicate(source.indexOf('\n', to), (found) => found >= 0),
            () => source.length,
        );
        const cut = (low: number, high: number, replacement = '') => Result.succeed(Array.prepend(this.#bodies(stmt, high), { start: low, end: high, fragment: Fragment.of(replacement) }));
        const sides: Readonly<Record<string, (binary: BinaryCmd) => ReturnType<typeof cut>>> = {
            x: ({ y }) => cut(start, y === null ? end : tree.start(y)),
            y: ({ x }) => cut(x === null ? start : tree.visible(x), end),
        };
        const binary = Option.filter(Option.fromNullishOr(link?.parent), (parent): parent is BinaryCmd => parent.type === 'BinaryCmd');
        const sided = Option.flatMap(binary, (held) => Option.map(Option.fromUndefinedOr(sides[link?.prop ?? '']), (side) => side(held)));
        const all = Option.getOrElse(listed, Array.empty<Stmt>);
        const adjoins = (low: number, high: number) => !source.slice(low, high).includes('\n');
        const next = Option.filter(
            Option.map(Array.get(all, all.indexOf(stmt) + 1), (held) => tree.start(held)),
            (found) => adjoins(to, found),
        );
        const previous = Option.filter(
            Option.map(Array.get(all, all.indexOf(stmt) - 1), (held) => tree.visible(held)),
            (found) => adjoins(found, from),
        );
        const whole = () =>
            Match.value({
                listed: Option.isSome(listed),
                sole: all.length === 1,
                loop: link?.parent?.type === 'WhileClause' && link.prop === 'cond',
                filled: !emptiable.has(link?.parent?.type ?? 'File'),
                owned: source.slice(opening, from).trim() === '' && source.slice(to, closing).trim() === '',
            }).pipe(
                Match.when({ listed: false }, () => Result.fail(refusal('Position')())),
                Match.when({ sole: true, loop: true }, () => Result.fail(refusal('Position')())),
                Match.when({ sole: true, filled: true }, () => cut(start, tree.body(stmt), ':')),
                Match.when({ owned: true }, () => cut(opening, Math.min(closing + 1, source.length))),
                Match.orElse(() =>
                    cut(
                        Option.match(next, { onSome: () => from, onNone: () => Option.getOrElse(previous, () => from) }),
                        Option.getOrElse(next, () => to),
                    ),
                ),
            );
        return refuse(start)(Option.getOrElse(sided, whole));
    }

    remove(target: Word | readonly [Word, Word] | Redirect | Stmt | Assign): Edit {
        return isSpan(target) || target.type !== 'Stmt' ? this.#excise(target) : this.#drop(target);
    }

    command(stmt: Stmt, script: Script): Edit {
        const { tree } = this.script;
        const [start, end] = [tree.start(stmt), tree.body(stmt)];
        const fragment = group(script, Option.isSome(tree.siblings(stmt)));
        return this.#place(stmt, fragment, [start], Array.prepend(this.#bodies(stmt, end), { start, end, fragment }));
    }

    precede(script: Script, joint: Joint, stmt: Stmt): Edit {
        const { tree } = this.script;
        const { text, separates, pipes } = joints[joint];
        const [start, fragment] = [tree.start(stmt), group(script, separates)];
        const bare = Option.isSome(tree.siblings(stmt)) || (pipes && !stmt.negated);
        return this.#place(stmt, fragment, [start], bare ? Array.of(insert(start, fragment.wrap('', text))) : Array.make(insert(start, fragment.wrap('{ ', text)), insert(tree.visible(stmt), Fragment.of('; }'))));
    }

    follow(stmt: Stmt, joint: Joint, script: Script): Edit {
        const { tree } = this.script;
        const { text, separates, pipes } = joints[joint];
        const fragment = group(script, separates);
        const changes = Match.value({ list: Option.isSome(tree.siblings(stmt)), background: separates && stmt.background, piped: pipes && !stmt.negated }).pipe(
            Match.when({ list: true, background: true }, () => Array.of(insert(tree.visible(stmt), fragment.wrap(' ', '')))),
            Match.when({ list: true }, () => Array.of(insert(tree.body(stmt), fragment.wrap(text, '')))),
            Match.when({ piped: true }, () => Array.of(insert(tree.visible(stmt), fragment.wrap(text, '')))),
            Match.orElse(() => Array.make(insert(tree.start(stmt), Fragment.of('{ ')), insert(tree.visible(stmt), fragment.wrap(text, '; }')))),
        );
        return this.#place(
            stmt,
            fragment,
            changes.map(({ start }) => start),
            changes,
        );
    }

    // --- [NESTING]

    #units(word: Word): Option.Option<readonly Unit[]> {
        const { tree, source, dialect } = this.script;
        const strips = Option.exists(tree.heredoc(word), (heredoc) => heredoc.strips);
        const containers = word.parts.flatMap((part): readonly Option.Option<Lit | SglQuoted>[] =>
            Match.value(part).pipe(
                Match.when({ type: 'DblQuoted' }, ({ parts }) => parts.map((inner) => (inner.type === 'Lit' ? Option.some(inner) : Option.none()))),
                Match.when({ type: Match.is('Lit', 'SglQuoted') }, (held) => [Option.some(held)]),
                Match.orElse(() => [Option.none()]),
            ),
        );
        const read = (container: Lit | SglQuoted) =>
            Option.map(
                quotingAt(tree, container, () => '', '', dialect),
                (quoting) => {
                    const origin = tree.start(container) + (container.type === 'SglQuoted' ? 1 : 0);
                    return container.value.matchAll(grammarOf(quoting)).map((found) => ({ start: origin + found.index, end: origin + found.index + found[0].length, container, text: found.groups?.['escaped'] ?? found.groups?.['plain'] ?? '' }));
                },
            );
        return Option.map(Option.all(containers.map(Option.flatMap(read))), (found) => {
            const [, kept] = Array.mapAccum(
                found.flatMap((units) => units.toArray()),
                true,
                (opening, unit) => (strips && opening && unit.text === '\t' ? [true, { ...unit, text: '' }] : [source[unit.end - 1] === '\n', unit]),
            );
            return kept.flatMap((unit) => Array.makeBy(unit.text.length, (at) => ({ ...unit, text: unit.text.charAt(at) })));
        });
    }

    #requote(word: Word, units: readonly Unit[], change: Change): Result.Result<readonly Change[], Refused> {
        const { tree, source, dialect } = this.script;
        const inner = units.map(({ text }) => text).join('');
        const anchor = Option.orElse(
            Option.map(Array.get(units, change.start), ({ start, container }) => ({ point: start, container })),
            () => Option.map(Array.last(units), ({ end, container }) => ({ point: end, container })),
        );
        return Result.gen(function* () {
            const { point, container } = yield* Result.fromOption(anchor, refusal('Position'));
            const offset = point - tree.start(container);
            const split = `${container.value.slice(0, offset)}${hole}${container.value.slice(offset)}`;
            const quoting = quotingAt(tree, container, (held) => (container.type === 'Lit' ? shapeOf(held, container, split) : shapeOf(held)), source.slice(tree.start(container), point), dialect);
            const cuts = Array.dedupe([0, ...change.fragment.redacted.flatMap(({ start, end }) => [start, end]), change.fragment.source.length].toSorted((left, right) => left - right));
            const pieces = yield* Result.all(
                Array.window(cuts, 2).map(([from, to]) => {
                    const piece = change.fragment.slice(from, to);
                    const shown = Option.match(Array.head(piece.redacted), { onNone: () => piece.source, onSome: (span) => Redacted.make(piece.source, { label: span.shown.label }) });
                    return quote(quoting, Content.Words({ words: [shown], listed: false }), dialect);
                }),
            );
            const written = `${inner.slice(0, change.start)}${change.fragment.source}${inner.slice(change.end)}`;
            yield* Result.liftPredicate(written, (body) => keeps(tree, word, dialect, body, [[change.start, change.start + change.fragment.source.length]]), refusal('Delimiter'));
            const fragment = Fragment.ReducerConcat.combineAll(pieces);
            const covered = units.slice(change.start, change.end);
            const runs = Array.isReadonlyArrayNonEmpty(covered) ? Array.groupWith(covered, (left, right) => left.container === right.container) : [];
            return Array.match(runs, {
                onEmpty: () => [insert(point, fragment)],
                onNonEmpty: Array.map((run, at) => ({ start: Array.headNonEmpty(run).start, end: Array.lastNonEmpty(run).end, fragment: at === 0 ? fragment : Fragment.of('') })),
            });
        });
    }

    nested(word: Word, inner: Edit): Edit {
        const { tree, dialect } = this.script;
        const literal = Option.isSome(tree.heredoc(word)) || Option.isSome(unquote(word, dialect));
        return Result.flatMap(inner, (changes) =>
            refuse(tree.start(word))(
                Result.gen(this, function* () {
                    const units = yield* Result.fromOption(
                        Option.filter(this.#units(word), () => literal),
                        refusal('Position'),
                    );
                    const outer = yield* Result.all(changes.map((change) => this.#requote(word, units, change)));
                    return yield* Result.fromOption(Option.liftPredicate(outer.flat(), Array.isArrayNonEmpty), refusal('Position'));
                }),
            ),
        );
    }

    // --- [APPLICATION]

    apply(...edits: readonly Edit[]): Result.Result<Fragment, ShellError> {
        const { text } = this.script;
        return Result.gen(function* () {
            const settled = yield* Result.all(
                edits.map((edit, order) =>
                    Result.mapBoth(edit, {
                        onSuccess: Array.map((change) => ({ ...change, order })),
                        onFailure: ({ kind, offset, cause }) => ShellError.of({ _tag: 'Edit', offset, kind }, `edit ${order}`)(cause),
                    }),
                ),
            );
            const changes = Array.dedupeWith(settled.flat(), (left, right) => left.start === right.start && left.end === right.end && Equal.equals(left.fragment, right.fragment)).toSorted((left, right) => left.start - right.start || left.end - right.end);
            const overlap = Array.findFirst(Array.window(changes, 2), ([left, right]) => right.start < left.end);
            const disjoint = yield* Option.match(overlap, {
                onNone: () => Result.succeed(changes),
                onSome: ([left, right]) => Result.fail(ShellError.of({ _tag: 'Edit', offset: right.start, kind: 'Overlap' }, `edit ${right.order} over edit ${left.order}`)()),
            });
            const [rest, pieces] = Array.mapAccum(disjoint, 0, (at, { start, end, fragment }) => [end, [text.slice(at, start), text.slice(start, end).replace(fragment)]]);
            return Fragment.ReducerConcat.combineAll([...pieces.flat(), text.slice(rest, text.source.length)]);
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const isSpan = (target: Node | readonly [Word, Word]): target is readonly [Word, Word] => Array.isArray(target);
const refuse =
    (offset: number) =>
    <A>(outcome: Result.Result<A, Refused>) =>
        Result.mapError(outcome, (refused): Rejected => ({ ...refused, offset }));
const insert = (at: number, fragment: Fragment): Change => ({ start: at, end: at, fragment });
const wordsOf = (script: Script): Option.Option<Fragment> => {
    const plain = Option.filter(Array.head(script.syntax.stmts), (stmt) => script.syntax.stmts.length === 1 && stmt.redirs.length === 0 && stmt.comments.length === 0 && !stmt.background && !stmt.negated && !stmt.coprocess);
    const args = Option.flatMap(plain, ({ cmd }) => (cmd?.type === 'CallExpr' && cmd.assigns.length === 0 ? Option.some(cmd.args) : Option.none()));
    return Option.map(
        Option.flatMap(args, (words) => Option.all([Array.head(words), Array.last(words)])),
        ([first, last]) => script.text.slice(script.located(first.start), script.located(last.end)),
    );
};
const contentOf = (content: Replacement): Result.Result<Content, Refused> =>
    content instanceof Script
        ? Result.fromOption(
              Option.map(wordsOf(content), (fragment) => Content.Source({ fragment, command: true })),
              refusal('Position'),
          )
        : Result.succeed(Content.Words({ words: content, listed: content.length !== 1 }));
const group = (script: Script, list: boolean): Fragment => {
    const { tree, text } = script;
    const lined = tree.heredocs.length > 0 || tree.links.keys().some((node) => node.type === 'Comment');
    const single = Option.filter(Array.head(tree.syntax.stmts), (stmt) => tree.syntax.stmts.length === 1 && !lined && !stmt.background && !stmt.coprocess && (list || (!stmt.negated && stmt.cmd?.type !== 'BinaryCmd')));
    const closed = Option.exists(Array.last(tree.syntax.stmts), (last) => last.background || last.semicolon.line > 0);
    return Option.match(single, {
        onSome: (stmt) => text.slice(tree.start(stmt), tree.body(stmt)),
        onNone: () => (lined ? text.wrap('{ ', '\n}') : text.slice(0, text.source.trimEnd().length).wrap('{ ', closed ? ' }' : '; }')),
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Edit, Editor, type Joint };
