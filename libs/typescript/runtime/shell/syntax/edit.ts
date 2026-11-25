import { Array, Data, Match, Option, Result } from 'effect';
import type { CommandLine } from 'effect/process';
import { type BinaryCmd, type Comment, createSourceIndex, type Node, RedirOperator, type Redirect, type Stmt, walk, type Word, type WordPart } from 'sh-syntax';
import { text } from '#/evaluation/expansion';
import { delimited, type Link, opened, rendered, Site, sited } from '#/syntax/construction';
import { Script, ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Kind = Exclude<Extract<ShellError['reason'], { readonly _tag: 'Edit' }>['kind'], 'Overlap'>;
type Span = Script['redacted'][number];
type Joint = keyof typeof joints;
type Content = Script | Array.NonEmptyReadonlyArray<CommandLine.Word>;
interface Change {
    readonly start: number;
    readonly end: number;
    readonly text: string;
    readonly redacted: readonly Span[];
}
interface Unit {
    readonly start: number;
    readonly end: number;
    readonly container: Node;
    readonly text: string;
}
type Edit = Data.TaggedEnum<{ Splice: { readonly changes: Array.NonEmptyReadonlyArray<Change> }; Refused: { readonly kind: Kind; readonly offset: number } }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const Edit = Data.taggedEnum<Edit>();
const joints = { ';': true, '&': true, '&&': false, '||': false, '|': false, '|&': false } as const;
const spaced: Readonly<Record<Joint, string>> = { ';': '; ', '&': ' & ', '&&': ' && ', '||': ' || ', '|': ' | ', '|&': ' |& ' };
const emptied: ReadonlySet<string> = new Set(['File', 'CmdSubst', 'ProcSubst', 'CaseItem']);
const identifier = /^[A-Za-z_]\w*$/u;
const bare = /\\\n|\\(?<escaped>[\s\S])|(?<plain>[\s\S])/gu;
const double = /\\\n|\\(?<escaped>[$`"\\])|(?<plain>[\s\S])/gu;
const body = /\\\n|\\(?<escaped>[$`\\])|(?<plain>[\s\S])/gu;
const verbatim = /(?<plain>[\s\S])/gu;

// --- [MODELS] --------------------------------------------------------------------------

class Editor {
    readonly script: Script;
    readonly #located: (offset: number) => number;
    readonly #links = new Map<Node, { readonly parent: Node | null; readonly prop: string | null; readonly index: number | null }>();
    readonly #ends = new Map<Node, number>();
    readonly #heredocs: Redirect[] = [];
    readonly #comments: Comment[] = [];
    readonly #lists = new Map<Node, Map<string, Stmt[]>>();

    // --- [INDEX]

    constructor(script: Script) {
        this.script = script;
        this.#located = createSourceIndex(script.source);
        const root = { end: -1, visible: -1 };
        const open = [root];
        walk(script.syntax, {
            enter: (node, parent, prop, index) => {
                this.#links.set(node, { parent, prop, index });
                open.push({ end: -1, visible: -1 });
                if (node.type === 'Stmt' && parent !== null && prop !== null && index !== null) {
                    const lists = this.#lists.get(parent) ?? new Map<string, Stmt[]>();
                    this.#lists.set(parent, lists.set(prop, [...(lists.get(prop) ?? []), node]));
                }
            },
            exit: (node, _, prop) => {
                const inner = open.pop() ?? root;
                const outer = open.at(-1) ?? root;
                const own = node.end > inner.end ? node.end : inner.visible;
                const visible = node.type === 'Stmt' && node.semicolon.line > 0 ? Math.max(own, node.semicolon.offset + 1) : own;
                this.#ends.set(node, visible);
                outer.end = Math.max(outer.end, node.end);
                outer.visible = prop === 'hdoc' || node.type === 'Comment' ? outer.visible : Math.max(outer.visible, visible);
                this.#heredocs.push(...(node.type === 'Redirect' && node.hdoc !== null ? [node] : []));
                this.#comments.push(...(node.type === 'Comment' ? [node] : []));
            },
        });
    }

    #path(node: Node): readonly Link[] {
        return Array.unfold<Node | null, Link>(node, (at) =>
            Option.map(Option.fromNullishOr(at), (current) => {
                const link = this.#links.get(current);
                return [{ node: current, prop: link?.prop ?? null, index: link?.index ?? null }, link?.parent ?? null] as const;
            }),
        ).toReversed();
    }

    #end(node: Node): number {
        const last = node.type === 'Word' ? Array.last(node.parts) : Option.none();
        return node.type === 'Lit' ? this.#located(node.start) + node.value.length : Option.match(last, { onNone: () => this.#located(node.end), onSome: (part) => this.#end(part) });
    }

    #visible(node: Node): number {
        return this.#located(this.#ends.get(node) ?? node.end);
    }

    #body(stmt: Stmt): number {
        return this.#located(Math.max(stmt.start, ...[stmt.cmd, ...stmt.redirs].flatMap((node) => (node === null ? [] : [this.#ends.get(node) ?? node.end]))));
    }

    #siblings(stmt: Stmt): Option.Option<readonly Stmt[]> {
        const link = this.#links.get(stmt);
        return Option.fromUndefinedOr(link?.parent === null || link?.parent === undefined || link.prop === null ? undefined : this.#lists.get(link.parent)?.get(link.prop));
    }

    #bodies(node: Node, after: number): readonly Change[] {
        const [low, high] = [node.start, this.#ends.get(node) ?? node.end];
        return this.#heredocs.flatMap(({ start, hdoc }) => {
            const [from, to] = hdoc === null ? [-1, -1] : [this.#located(hdoc.start), this.#located(hdoc.end)];
            return start >= low && start < high && from >= after ? [{ start: from, end: to + (this.script.source[to] === '\n' ? 1 : 0), text: '', redacted: [] }] : [];
        });
    }

    #pending(at: number): boolean {
        return this.#heredocs.some(({ start, hdoc }) => this.#located(start) < at && hdoc !== null && this.#located(hdoc.start) > at);
    }

    #placed(target: Word | WordPart): { readonly path: readonly Link[]; readonly marked: (word: Word) => string; readonly preceding: string } {
        const container = target.type === 'Word' ? Option.none() : Option.fromNullishOr(this.#links.get(target)?.parent);
        return Option.match(container, {
            onNone: () => ({ path: this.#path(target), marked: () => '\0', preceding: '' }),
            onSome: (parent) => ({
                path: this.#path(parent),
                marked: (word: Word) => word.parts.map((part) => (part === target ? '\0' : opened(part))).join(''),
                preceding: this.script.source.slice(this.#located(parent.start), this.#located(target.start)),
            }),
        });
    }

    #delimits(word: Word, written: string, span: readonly [number, number]): boolean {
        const redirect = Option.filter(Option.fromNullishOr(this.#links.get(word)?.parent), (parent): parent is Redirect => parent.type === 'Redirect');
        return Option.exists(redirect, (heredoc) => {
            const delimiter = Option.flatMap(Option.fromNullishOr(heredoc.word), (closing) => text(closing, this.script.dialect));
            return Option.exists(delimiter, () => delimited(written, [span], heredoc.op === RedirOperator.DashHdoc, delimiter));
        });
    }

    #grouped(list: boolean): { readonly text: string; readonly redacted: readonly Span[] } {
        const { stmts } = this.script.syntax;
        const lined = this.#heredocs.length > 0 || this.#comments.length > 0;
        const closed = Option.exists(Array.last(stmts), (last) => last.background || last.semicolon.line > 0);
        const trimmed = this.script.source.trimEnd();
        const single = Option.filter(Option.fromUndefinedOr(stmts.length === 1 ? stmts[0] : undefined), (stmt) => !lined && !stmt.background && !stmt.coprocess && (list || (!stmt.negated && stmt.cmd?.type !== 'BinaryCmd')));
        return Option.match(single, {
            onSome: (stmt) => shown(this.script, this.#located(stmt.start), this.#body(stmt), '', ''),
            onNone: () =>
                Match.value({ lined, closed }).pipe(
                    Match.when({ lined: true }, () => shown(this.script, 0, this.script.source.length, '{ ', '\n}')),
                    Match.when({ closed: true }, () => shown(this.script, 0, trimmed.length, '{ ', ' }')),
                    Match.orElse(() => shown(this.script, 0, trimmed.length, '{ ', '; }')),
                ),
        });
    }

    #units(word: Word): Option.Option<readonly Unit[]> {
        const link = this.#links.get(word);
        const dash = link?.prop === 'hdoc' && link.parent?.type === 'Redirect' && link.parent.op === RedirOperator.DashHdoc;
        const containers = Option.all(
            word.parts.flatMap((part): readonly Option.Option<WordPart>[] =>
                Match.value(part).pipe(
                    Match.when({ type: 'DblQuoted' }, ({ parts }) => parts.map(Option.liftPredicate((inner: WordPart) => inner.type === 'Lit'))),
                    Match.when({ type: Match.is('Lit', 'SglQuoted') }, (held) => [Option.some(held)]),
                    Match.orElse(() => [Option.none()]),
                ),
            ),
        );
        const decoded = Option.flatMap(containers, (found) =>
            Option.all(
                found.map((container) =>
                    Option.map(
                        sited(this.#path(container), () => '', '', this.script.dialect),
                        (site) => ({
                            container,
                            origin: this.#located(container.start) + (container.type === 'SglQuoted' ? 1 : 0),
                            pattern: Site.$match(site, { Bare: () => bare, Double: () => double, Single: () => verbatim, Body: ({ expands }) => (expands ? body : verbatim) }),
                        }),
                    ),
                ),
            ),
        );
        return Option.map(decoded, (sources) => {
            const entries = sources.flatMap(({ container, origin, pattern }) =>
                (container.type === 'Lit' || container.type === 'SglQuoted' ? container.value : '')
                    .matchAll(pattern)
                    .map((found) => ({ start: origin + found.index, end: origin + found.index + found[0].length, container, text: found.groups?.['escaped'] ?? found.groups?.['plain'] ?? '' }))
                    .toArray(),
            );
            const [, kept] = Array.mapAccum(entries, true, (opening, entry) => (dash && opening && entry.text === '\t' ? ([true, { ...entry, text: '' }] as const) : ([this.script.source[entry.end - 1] === '\n', entry] as const)));
            return kept.flatMap((entry) => Array.makeBy(entry.text.length, (at) => ({ ...entry, text: entry.text.charAt(at) })));
        });
    }

    #outer(word: Word, units: readonly Unit[], change: Change): Result.Result<readonly Change[], Kind> {
        const inner = units.map(({ text: unit }) => unit).join('');
        const anchor = Option.orElse(
            Option.map(Array.get(units, change.start), ({ start, container }) => ({ point: start, container })),
            () => Option.map(Array.last(units), ({ end, container }) => ({ point: end, container })),
        );
        return Result.gen(this, function* () {
            const { point, container } = yield* Result.fromOption(anchor, () => 'Position' as const);
            const offset = point - this.#located(container.start);
            const path = this.#path(container);
            const marked = (held: Word) => held.parts.map((part) => (part === container && part.type === 'Lit' ? `${part.value.slice(0, offset)}\0${part.value.slice(offset)}` : opened(part))).join('');
            const site = sited(path, marked, this.script.source.slice(this.#located(container.start), point), this.script.dialect);
            const cuts = Array.dedupe([0, ...change.redacted.flatMap(({ start, end }) => [start, end]), change.text.length].toSorted((left, right) => left - right));
            const pieces = yield* Result.all(Array.window(cuts, 2).map(([from, to]) => Result.mapError(rendered(site, change.text.slice(from, to), this.script.dialect), ({ kind }) => kind)));
            const [written, spans] = Array.mapAccum(Array.zip(Array.window(cuts, 2), pieces), '', (held, [[from], piece]) => [`${held}${piece.text}`, change.redacted.filter(({ start }) => start === from).map(({ shown: hidden }) => ({ start: held.length, end: held.length + piece.text.length, shown: hidden }))]);
            yield* Result.liftPredicate(
                change,
                (held) => this.#links.get(word)?.prop !== 'hdoc' || this.#delimits(word, `${inner.slice(0, held.start)}${held.text}${inner.slice(held.end)}`, [held.start, held.start + held.text.length]),
                () => 'Delimiter' as const,
            );
            const covered = units.slice(change.start, change.end);
            const groups = Array.isReadonlyArrayNonEmpty(covered) ? Array.groupWith(covered, (left, right) => left.container === right.container) : [];
            return Array.match(groups, {
                onEmpty: () => [{ start: point, end: point, text: written, redacted: spans.flat() }],
                onNonEmpty: (found) => found.map((group, at) => ({ start: Array.headNonEmpty(group).start, end: Array.lastNonEmpty(group).end, text: at === 0 ? written : '', redacted: at === 0 ? spans.flat() : [] })),
            });
        });
    }

    // --- [WORDS]

    replace(target: Word | WordPart | readonly [Word, Word], content: Content): Edit {
        const [first, last] = spanned(target) ? target : [target, target];
        const { path, marked, preceding } = this.#placed(first);
        const site = sited(path, marked, preceding, this.script.dialect);
        const [start, end] = [this.#located(first.start), this.#end(last)];
        const siblings = this.#links.get(first)?.parent === this.#links.get(last)?.parent;
        return settled(start)(
            Result.flatMap(siblings ? filled(site, content, this.script.dialect) : Result.fail('Position' as const), (shown) => {
                const enclosing = Array.findLast(path, ({ node }) => node.type === 'Word');
                const [from, to] = Option.match(enclosing, { onNone: () => [start, end] as const, onSome: ({ node }) => [this.#located(node.start), this.#end(node)] as const });
                const written = `${this.script.source.slice(from, start)}${shown.text}${this.script.source.slice(end, to)}`;
                return Option.exists(site, Site.$is('Body')) && !Option.exists(enclosing, ({ node }) => node.type === 'Word' && this.#delimits(node, written, [start - from, start - from + shown.text.length])) ? Result.fail('Delimiter' as const) : Result.succeed(Array.of({ start, end, ...shown }));
            }),
        );
    }

    #beside(word: Word, content: Content): Result.Result<{ readonly text: string; readonly redacted: readonly Span[] }, Kind> {
        const whole = Option.filter(
            sited(this.#path(word), () => '\0', '', this.script.dialect),
            (site) => Site.$is('Bare')(site) && site.whole,
        );
        return Option.isSome(whole) ? filled(whole, content, this.script.dialect) : Result.fail('Position');
    }

    before(word: Word, content: Content): Edit {
        const at = this.#located(word.start);
        return settled(at)(Result.map(this.#beside(word, content), (shown) => Array.of(inserted(at, '', shown, ' '))));
    }

    after(word: Word, content: Content): Edit {
        const at = this.#end(word);
        return settled(at)(Result.map(this.#beside(word, content), (shown) => Array.of(inserted(at, ' ', shown, ''))));
    }

    assign(stmt: Stmt, name: string, value: CommandLine.Word): Edit {
        const head = stmt.cmd?.type === 'CallExpr' ? Array.head(stmt.cmd.args) : Option.none();
        return settled(this.#located(stmt.start))(
            Result.gen(this, function* () {
                const word = yield* Result.fromOption(head, () => 'Position' as const);
                yield* Result.liftPredicate(
                    name,
                    (given) => identifier.test(given),
                    () => 'Value' as const,
                );
                const shown = yield* Result.mapError(rendered(Option.some(Site.Bare({ whole: false, command: false })), value, this.script.dialect), ({ kind }) => kind);
                return Array.of(inserted(this.#located(word.start), `${name}=`, shown, ' '));
            }),
        );
    }

    // --- [COMMANDS]

    remove(target: Word | readonly [Word, Word] | Redirect | Stmt | Extract<Node, { readonly type: 'Assign' }>): Edit {
        return spanned(target) || target.type !== 'Stmt' ? this.#excised(target) : this.#dropped(target);
    }

    #excised(target: Word | readonly [Word, Word] | Redirect | Extract<Node, { readonly type: 'Assign' }>): Edit {
        const [first, last] = spanned(target) ? target : [target, target];
        const stmt = Option.filter(
            Option.map(
                Array.findLast(this.#path(first).slice(0, -1), ({ node }) => node.type === 'Stmt'),
                ({ node }) => node,
            ),
            (node): node is Stmt => node.type === 'Stmt',
        );
        return settled(this.#located(first.start))(
            Result.gen(this, function* () {
                const { cmd, redirs } = yield* Result.fromOption(stmt, () => 'Position' as const);
                const args = cmd?.type === 'CallExpr' ? cmd.args : [];
                const elements = [...(cmd?.type === 'CallExpr' ? [...cmd.assigns, ...cmd.args] : Option.toArray(Option.fromNullishOr(cmd))), ...redirs].toSorted((left, right) => left.start - right.start);
                const [from, to] = [elements.indexOf(first), elements.indexOf(last)];
                const removed: readonly Node[] = elements.slice(from, to + 1);
                const remaining = args.filter((word) => !removed.includes(word));
                const run = spanned(target) ? removed.every((node) => node.type === 'Word' && args.includes(node)) : true;
                yield* Result.liftPredicate(
                    removed,
                    (nodes) => from >= 0 && to >= from && run && elements.length > nodes.length && (args.length === 0 || remaining.length > 0),
                    () => 'Position' as const,
                );
                const head = Option.filter(Array.head(remaining), () => Option.exists(Array.head(args), (program) => removed.includes(program)));
                const respelled = yield* Option.match(head, {
                    onNone: () => Result.succeed<readonly Change[]>([]),
                    onSome: (word) =>
                        Option.match(text(word, this.script.dialect), {
                            onNone: () => (word.parts[0]?.type === 'Lit' ? Result.fail('Position' as const) : Result.succeed([])),
                            onSome: (literal) =>
                                Result.map(
                                    Result.mapError(rendered(Option.some(Site.Bare({ whole: true, command: true })), [literal], this.script.dialect), ({ kind }) => kind),
                                    (shown) => [{ start: this.#located(word.start), end: this.#end(word), ...shown }],
                                ),
                        }),
                });
                const [before, after] = [elements[from - 1], elements[to + 1]];
                const [start, end] = before === undefined ? [this.#located(first.start), after === undefined ? this.#visible(last) : this.#located(after.start)] : [this.#visible(before), this.#visible(last)];
                return Array.prepend([...respelled, ...removed.flatMap((node) => this.#bodies(node, end))], { start, end, text: '', redacted: [] });
            }),
        );
    }

    #dropped(stmt: Stmt): Edit {
        const link = this.#links.get(stmt);
        const binary = Option.filter(Option.fromNullishOr(link?.parent), (parent): parent is BinaryCmd => parent.type === 'BinaryCmd');
        const siblings = this.#siblings(stmt);
        const listed = Option.getOrElse(siblings, Array.empty<Stmt>);
        const index = listed.indexOf(stmt);
        const [start, end] = [this.#located(stmt.start), this.#visible(stmt)];
        const { source } = this.script;
        const comments = stmt.comments.map((comment) => [this.#located(comment.start), this.#located(comment.end)] as const);
        const [from, to] = [Math.min(start, ...comments.map(([at]) => at)), Math.max(end, ...comments.map(([, at]) => at))];
        const opening = source.slice(0, from).lastIndexOf('\n') + 1;
        const closing = Option.getOrElse(
            Option.liftPredicate(source.indexOf('\n', to), (at) => at >= 0),
            () => source.length,
        );
        const removal = ([low, high]: readonly [number, number], replacement = '') => Result.succeed(Array.prepend(this.#bodies(stmt, high), { start: low, end: high, text: replacement, redacted: [] }));
        const shape = {
            side: Option.isSome(binary) ? link?.prop : undefined,
            listed: Option.isSome(siblings),
            sole: listed.length === 1,
            loop: link?.parent?.type === 'WhileClause' && link.prop === 'cond',
            filled: !emptied.has(link?.parent?.type ?? 'File'),
            owned: source.slice(opening, from).trim() === '' && source.slice(to, closing).trim() === '',
            next: Option.filter(
                Option.map(Array.get(listed, index + 1), (held) => this.#located(held.start)),
                (at) => !source.slice(to, at).includes('\n'),
            ),
            previous: Option.filter(
                Option.map(Array.get(listed, index - 1), (held) => this.#visible(held)),
                (at) => !source.slice(at, from).includes('\n'),
            ),
        };
        return settled(start)(
            Match.value(shape).pipe(
                Match.when({ side: 'x' }, () =>
                    removal([
                        start,
                        Option.getOrElse(
                            Option.map(
                                Option.flatMap(binary, ({ y }) => Option.fromNullishOr(y)),
                                (y) => this.#located(y.start),
                            ),
                            () => end,
                        ),
                    ]),
                ),
                Match.when({ side: 'y' }, () =>
                    removal([
                        Option.getOrElse(
                            Option.map(
                                Option.flatMap(binary, ({ x }) => Option.fromNullishOr(x)),
                                (x) => this.#visible(x),
                            ),
                            () => start,
                        ),
                        end,
                    ]),
                ),
                Match.when({ listed: false }, () => Result.fail('Position' as const)),
                Match.when({ sole: true, loop: true }, () => Result.fail('Position' as const)),
                Match.when({ sole: true, filled: true }, () => removal([start, this.#body(stmt)], ':')),
                Match.when({ owned: true }, () => removal([opening, Math.min(closing + 1, source.length)])),
                Match.when({ next: { _tag: 'Some' } }, ({ next }) => removal([from, Option.getOrElse(next, () => to)])),
                Match.when({ previous: { _tag: 'Some' } }, ({ previous }) => removal([Option.getOrElse(previous, () => from), to])),
                Match.orElse(() => removal([from, to])),
            ),
        );
    }

    command(stmt: Stmt, script: Script): Edit {
        const [start, end] = [this.#located(stmt.start), this.#body(stmt)];
        const list = Option.isSome(this.#siblings(stmt));
        const shown = new Editor(script).#grouped(list);
        const placeable = (list || this.#links.get(stmt)?.parent?.type === 'BinaryCmd') && !(shown.text.includes('\n') && this.#pending(start));
        return settled(start)(placeable ? Result.succeed(Array.prepend(this.#bodies(stmt, end), { start, end, ...shown })) : Result.fail('Position'));
    }

    precede(script: Script, joint: Joint, stmt: Stmt): Edit {
        const start = this.#located(stmt.start);
        const shown = new Editor(script).#grouped(joints[joint]);
        const list = Option.isSome(this.#siblings(stmt));
        const binary = this.#links.get(stmt)?.parent?.type === 'BinaryCmd';
        const placeable = (list || binary) && !(shown.text.includes('\n') && this.#pending(start));
        return settled(start)(placeable ? Result.succeed(list || (joint.startsWith('|') && !stmt.negated) ? Array.of(inserted(start, '', shown, spaced[joint])) : Array.make(inserted(start, '{ ', shown, spaced[joint]), inserted(this.#visible(stmt), '; }', { text: '', redacted: [] }, ''))) : Result.fail('Position'));
    }

    follow(stmt: Stmt, joint: Joint, script: Script): Edit {
        const shown = new Editor(script).#grouped(joints[joint]);
        const list = Option.isSome(this.#siblings(stmt));
        const binary = this.#links.get(stmt)?.parent?.type === 'BinaryCmd';
        const placed = Match.value({ list, background: joints[joint] && stmt.background, piped: binary && joint.startsWith('|') && !stmt.negated }).pipe(
            Match.when({ list: true, background: true }, () => Array.of(inserted(this.#visible(stmt), ' ', shown, ''))),
            Match.when({ list: true }, () => Array.of(inserted(this.#body(stmt), spaced[joint], shown, ''))),
            Match.when({ piped: true }, () => Array.of(inserted(this.#visible(stmt), spaced[joint], shown, ''))),
            Match.orElse(() => Array.make(inserted(this.#located(stmt.start), '{ ', { text: '', redacted: [] }, ''), inserted(this.#visible(stmt), spaced[joint], shown, '; }'))),
        );
        const placeable = (list || binary) && !(shown.text.includes('\n') && placed.some(({ start }) => this.#pending(start)));
        return settled(this.#located(stmt.start))(placeable ? Result.succeed(placed) : Result.fail('Position'));
    }

    // --- [NESTING]

    nested(word: Word, inner: Edit): Edit {
        const literal = this.#links.get(word)?.prop === 'hdoc' || Option.isSome(text(word, this.script.dialect));
        return Edit.$match(inner, {
            Refused: () => inner,
            Splice: ({ changes }) =>
                settled(this.#located(word.start))(
                    Result.gen(this, function* () {
                        const units = yield* Result.fromOption(
                            Option.filter(this.#units(word), () => literal),
                            () => 'Position' as const,
                        );
                        const outer = yield* Result.all(changes.map((change) => this.#outer(word, units, change)));
                        return yield* Result.fromOption(Array.match(outer.flat(), { onEmpty: Option.none, onNonEmpty: Option.some }), () => 'Position' as const);
                    }),
                ),
        });
    }

    // --- [APPLICATION]

    apply(...edits: readonly Edit[]): Result.Result<{ readonly source: string; readonly redacted: Script['redacted'] }, ShellError> {
        const { source, redacted } = this.script;
        return Result.flatMap(
            Result.all(
                edits.map((edit, index) =>
                    Edit.$match(edit, {
                        Splice: ({ changes }) => Result.succeed(changes.map((change) => ({ ...change, index }))),
                        Refused: ({ kind, offset }) => Result.fail(ShellError.of({ _tag: 'Edit', offset, kind }, `edit ${index}`)()),
                    }),
                ),
            ),
            (lists) => {
                const ordered = lists.flat().toSorted((left, right) => left.start - right.start || left.end - right.end);
                const crossed = Array.findFirst(Array.window(ordered, 2), ([left, right]) => right.start < left.end);
                const moved = (at: number, closing: boolean) =>
                    ordered.reduce(
                        (offset, change) =>
                            Match.value({ before: change.end <= at && !(closing && change.start === at), inside: change.start < at && change.end > at }).pipe(
                                Match.when({ before: true }, () => offset + change.text.length - (change.end - change.start)),
                                Match.when({ inside: true }, () => offset + (closing ? change.start + change.text.length - at : change.start - at)),
                                Match.orElse(() => offset),
                            ),
                        at,
                    );
                const [last, added] = Array.mapAccum(ordered, { written: '', at: 0 }, ({ written, at }, change) => {
                    const head = `${written}${source.slice(at, change.start)}`;
                    return [{ written: `${head}${change.text}`, at: change.end }, change.redacted.map((span) => ({ ...span, start: span.start + head.length, end: span.end + head.length }))];
                });
                const kept = redacted.map((span) => ({ ...span, start: moved(span.start, false), end: moved(span.end, true) })).filter(({ start, end }) => end > start);
                return Option.match(crossed, {
                    onSome: ([left, right]) => Result.fail(ShellError.of({ _tag: 'Edit', offset: right.start, kind: 'Overlap' }, `edit ${right.index} over edit ${left.index}`)()),
                    onNone: () => Result.succeed({ source: `${last.written}${source.slice(last.at)}`, redacted: [...kept, ...added.flat()].toSorted((left, right) => left.start - right.start) }),
                });
            },
        );
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spanned = (target: Node | readonly [Word, Word]): target is readonly [Word, Word] => Array.isArray(target);
const shown = (script: Script, start: number, end: number, prefix: string, suffix: string): { readonly text: string; readonly redacted: readonly Span[] } => ({
    text: `${prefix}${script.source.slice(start, end)}${suffix}`,
    redacted: script.redacted.filter((span) => span.start >= start && span.end <= end).map((span) => ({ ...span, start: span.start - start + prefix.length, end: span.end - start + prefix.length })),
});
const spoken = (script: Script): Option.Option<{ readonly text: string; readonly redacted: readonly Span[] }> => {
    const located = createSourceIndex(script.source);
    const [stmt] = script.syntax.stmts.length === 1 ? script.syntax.stmts : [];
    const call = Option.filter(Option.fromNullishOr(stmt?.cmd), (cmd) => cmd.type === 'CallExpr' && cmd.assigns.length === 0 && stmt?.redirs.length === 0 && stmt.comments.length === 0 && !stmt.background && !stmt.negated && !stmt.coprocess);
    return Option.flatMap(call, (cmd) => (cmd.type === 'CallExpr' ? Option.map(Option.all([Array.head(cmd.args), Array.last(cmd.args)]), ([first, last]) => shown(script, located(first.start), located(last.end), '', '')) : Option.none()));
};
const filled = (site: Option.Option<Site>, content: Content, dialect: Script['dialect']): Result.Result<{ readonly text: string; readonly redacted: readonly Span[] }, Kind> =>
    content instanceof Script
        ? Result.fromOption(
              Option.filter(spoken(content), () => Option.exists(site, (found) => Site.$is('Bare')(found) && found.whole)),
              () => 'Position' as const,
          )
        : Result.mapError(rendered(site, content.length === 1 ? content[0] : content, dialect), ({ kind }) => kind);
const inserted = (at: number, prefix: string, { text: written, redacted }: { readonly text: string; readonly redacted: readonly Span[] }, suffix: string): Change => ({
    start: at,
    end: at,
    text: `${prefix}${written}${suffix}`,
    redacted: redacted.map((span) => ({ ...span, start: span.start + prefix.length, end: span.end + prefix.length })),
});
const settled =
    (offset: number) =>
    (outcome: Result.Result<Array.NonEmptyReadonlyArray<Change>, Kind>): Edit =>
        Result.match(outcome, { onFailure: (kind) => Edit.Refused({ kind, offset }), onSuccess: (changes) => Edit.Splice({ changes }) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Edit, Editor, type Joint };
