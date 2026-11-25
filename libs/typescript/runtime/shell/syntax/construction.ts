import { Array, Data, Effect, Function, Match, Option, Predicate, Record, Redacted, Result } from 'effect';
import { CommandLine } from 'effect/process';
import { BinTestOperator, type File, type Node, RedirOperator, UnTestOperator, type Word, type WordPart } from 'sh-syntax';
import { bind } from '#/analysis/binding';
import { Written } from '#/analysis/invocation';
import { unquote } from '#/evaluation/expansion';
import { Field, Origin } from '#/evaluation/value';
import type { Usage } from '#/programs/usage';
import type { Dialect } from '#/syntax/dialect';
import { Fragment, index, type Link, type Refusal, type Script, ShellError, type Tree } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Quoting = Data.TaggedEnum<{
    Unquoted: { readonly whole: boolean; readonly command: boolean };
    Double: object;
    Single: object;
    Heredoc: { readonly expands: boolean };
}>;
type Content = Data.TaggedEnum<{
    Words: { readonly words: readonly CommandLine.Word[]; readonly listed: boolean };
    Source: { readonly fragment: Fragment; readonly command: boolean };
}>;
interface Refused {
    readonly kind: Refusal;
    readonly cause?: unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Quoting = Data.taggedEnum<Quoting>();
const Content = Data.taggedEnum<Content>();
const hole = '\0';
const opaque = '\u0001';
const escaping = /(?<!\\)(?:\\\\)*[\\$]$/u;
const evaluators: ReadonlySet<number> = new Set([BinTestOperator.TsEql, BinTestOperator.TsNeq, BinTestOperator.TsLeq, BinTestOperator.TsGeq, BinTestOperator.TsLss, BinTestOperator.TsGtr, UnTestOperator.TsVarSet, UnTestOperator.TsRefVar]);
const verbatim = /(?<plain>[\s\S])/gu;
const backslash = Record.map({ Unquoted: String.raw`\s\S`, Double: '$`"\\\\', Heredoc: '$`\\\\' }, (set) => {
    const escapable = new RegExp(`[${set}]`, 'gu');
    return { escape: (text: string) => text.replaceAll(escapable, String.raw`\$&`), units: new RegExp(String.raw`\\\n|\\(?<escaped>[${set}])|(?<plain>[\s\S])`, 'gu') };
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const refusal = (kind: Refusal) => (): Refused => ({ kind });
const reveal = (word: CommandLine.Word) => (Redacted.isRedacted(word) ? Redacted.value(word) : word);
const mask = (part: WordPart) => (part.type === 'Lit' ? part.value : opaque);
const shapeOf = (word: Word, target?: WordPart, text = hole) => word.parts.map((part) => (part === target ? text : mask(part))).join('');
const grammarOf = Quoting.$match({
    Unquoted: () => backslash.Unquoted.units,
    Double: () => backslash.Double.units,
    Single: () => verbatim,
    Heredoc: ({ expands }) => (expands ? backslash.Heredoc.units : verbatim),
});
const keeps = (tree: Tree, word: Word, dialect: Dialect, body: string, spans: readonly (readonly [number, number])[]): boolean =>
    Option.match(tree.heredoc(word), {
        onNone: Function.constTrue,
        onSome: ({ redirect, strips }) => {
            const text = strips ? body.replace(/(?<=\n|^)\t+$/u, '') : body;
            const lines = new Set(Array.dropRight(text.split('\n'), 1).map((line) => (strips ? line.replace(/^\t+/u, '') : line)));
            const tabbed = spans.some(([from, to]) => text.slice(from, to + 1).includes('\n\t') || (text[from] === '\t' && /^\t*$/u.test(text.slice(text.lastIndexOf('\n', from - 1) + 1, from))));
            const delimiter = Option.flatMap(Option.fromNullOr(redirect.word), (closing) => unquote(closing, dialect));
            const closed = tree.located(word.end) === tree.end(word) || text === '' || text.endsWith('\n');
            return closed && !(strips && tabbed) && Option.exists(delimiter, (closing) => !lines.has(closing));
        },
    });
const quotingAt = (tree: Tree, node: Node, shape: (word: Word) => string, preceding: string, dialect: Dialect): Option.Option<Quoting> => {
    const path = tree.path(node);
    const [leaf, parent] = [path.at(-1)?.node, path.at(-2)?.node];
    const held = Array.findLast(path, ({ node: word, parent: outer, prop, index: at }) => (word.type === 'Word' && outer !== null ? Option.some({ word, outer, prop, at }) : Option.none()));
    const shaped = Option.match(held, { onNone: () => '', onSome: ({ word }) => shape(word) });
    const context = (whole: boolean, command = false) =>
        Match.value({ leaf: leaf?.type, parent: parent?.type }).pipe(
            Match.when({ leaf: 'SglQuoted' }, () => Quoting.Single()),
            Match.whenOr({ leaf: 'DblQuoted' }, { parent: 'DblQuoted' }, () => Quoting.Double()),
            Match.orElse(() => Quoting.Unquoted({ whole, command: whole && command })),
        );
    const placed = Option.flatMap(held, ({ word, outer, prop, at }) =>
        Match.value({ outer, prop }).pipe(
            Match.when({ outer: { type: 'Redirect' }, prop: 'hdoc' }, () => Option.map(tree.heredoc(word), ({ expands }) => Quoting.Heredoc({ expands }))),
            Match.when({ outer: { type: 'Redirect', op: Match.is(RedirOperator.Hdoc, RedirOperator.DashHdoc) } }, () => Option.none()),
            Match.when({ outer: { type: Match.is('CallExpr', 'WordIter', 'ArrayElem') }, prop: Match.is('args', 'items', 'value') }, () => Option.some(context(shaped === hole, outer.type === 'CallExpr' && at === 0))),
            Match.when({ outer: { type: 'Assign', naked: false }, prop: 'value' }, () => Option.some(context(false))),
            Match.when({ outer: { type: Match.is('BinaryTest', 'UnaryTest'), op: (op: number) => evaluators.has(op) } }, () => Option.none()),
            Match.when({ outer: { type: 'BinaryTest', op: BinTestOperator.TsReMatch }, prop: 'y' }, () => Option.liftPredicate(context(false), () => dialect.options.literalregex)),
            Match.when({ outer: { type: Match.is('Assign', 'WordIter', 'ArrayElem', 'RepeatClause', 'CoprocClause', 'ArithmExp', 'ArithmCmd', 'LetClause', 'CStyleLoop', 'BinaryArithm', 'UnaryArithm', 'ParenArithm', 'CallArithm', 'FlagsArithm', 'ParamExp') } }, () => Option.none()),
            Match.orElse(() => Option.some(context(false))),
        ),
    );
    const braced = !dialect.options.literalbraces && /\{[^{},]*\0[^{},]*\}/u.test(shaped);
    const escapes = escaping.test(preceding);
    const refused = (leaf?.type === 'SglQuoted' && leaf.dollar) || (leaf?.type === 'Lit' && parent?.type !== 'Word' && parent?.type !== 'DblQuoted') || path.some(({ node: outer }) => outer.type === 'CmdSubst' && outer.backquotes);
    const sound = Quoting.$match({ Unquoted: () => !braced && !escapes, Double: () => !escapes, Single: Function.constTrue, Heredoc: ({ expands }) => !(expands && escapes) });
    return Option.filter(placed, (quoting) => !refused && sound(quoting));
};
const quote = (quoting: Option.Option<Quoting>, content: Content, dialect: Dialect): Result.Result<Fragment, Refused> =>
    Content.$match(content, {
        Source: ({ fragment, command }) => Result.liftPredicate(fragment, () => Option.exists(quoting, (held) => held._tag === 'Unquoted' && held.whole && (command || !held.command)), refusal('Position')),
        Words: ({ words, listed }) =>
            Result.gen(function* () {
                const commands = Option.exists(quoting, (held) => held._tag === 'Unquoted' && held.command);
                const spelled = yield* Result.mapError(Result.all(words.map((word, at) => (commands && at === 0 ? CommandLine.join([word]) : CommandLine.quote(word)))), (cause): Refused => ({ kind: 'Value', cause }));
                const fitting = Option.filter(quoting, (held) => !listed || (held._tag === 'Unquoted' && held.whole));
                const raw = words.map(reveal);
                const texts = yield* Result.fromOption(
                    Option.map(
                        fitting,
                        Quoting.$match({
                            Unquoted: ({ whole, command }) => Array.zip(spelled, raw).map(([text, plain], at) => (text === plain && (!whole || Option.isSome(dialect.alias(text, command && at === 0))) ? `'${text}'` : text)),
                            Double: () => raw.map(backslash.Double.escape),
                            Single: () => raw.map((text) => text.replaceAll("'", String.raw`'\''`)),
                            Heredoc: ({ expands }) => (expands ? raw.map(backslash.Heredoc.escape) : raw),
                        }),
                    ),
                    refusal('Position'),
                );
                return Fragment.ReducerConcat.combineAll(
                    Array.intersperse(
                        Array.zipWith(texts, words, (text, word) => Fragment.of(text, word)),
                        Fragment.of(' '),
                    ),
                );
            }),
    });
const evaluates = Effect.fnUntraced(function* (programs: (program: string) => Effect.Effect<Option.Option<Usage>, ShellError>, words: readonly Written[], target: Word): Effect.fn.Return<boolean, ShellError> {
    const program = Option.map(
        Option.flatMap(Array.head(words), (held) => held.text),
        (name) => name.slice(name.lastIndexOf('/') + 1),
    );
    const { code, commands, flags } = bind(Option.flatten(yield* Effect.transposeOption(Option.map(program, programs))), words, Field.known('/'));
    const holds = ({ node }: Written) => Option.exists(node, (word) => word === target);
    const attached = flags.some(({ spelled, values }) => holds(spelled) && values.some((value) => code.includes(value)));
    return code.some(holds) || attached || Option.isSome(yield* Effect.findFirst(commands.filter(Array.some(holds)), (list) => evaluates(programs, list, target)));
});
const spell = (text: Option.Option<string>, node: Option.Option<Word>) => new Written({ field: Option.match(text, { onNone: () => Field.unknown(Origin.Unsupported()), onSome: Field.known }), exact: true, node });
const argvOf = (command: Node, literal: (word: Word) => Option.Option<string>, held: number | null, target: Option.Option<Word>): readonly Written[] =>
    Match.value(command).pipe(
        Match.when({ type: 'CallExpr' }, ({ args }) => args.map((word) => spell(literal(word), Option.some(word)))),
        Match.when({ type: 'DeclClause' }, ({ variant, args }) => [
            spell(Option.fromNullishOr(variant?.value), Option.none()),
            ...args.map(({ name, value }, at) => {
                const assigned = Option.map(Option.flatMap(Option.fromNullOr(value), literal), (text) => (name === null ? text : `${name.value}=${text}`));
                return spell(assigned, at === held ? target : Option.fromNullOr(value));
            }),
        ]),
        Match.orElse(() => []),
    );
const commandsAt = (path: readonly Link[], literal: (word: Word) => Option.Option<string>) =>
    Array.window(path, 2).flatMap(([{ node: outer }, { prop, index: held }], depth) => {
        const target = Array.findFirst(path.slice(depth + 1), ({ node }) => (node.type === 'Word' ? Option.some(node) : Option.none()));
        const words = prop === 'args' ? argvOf(outer, literal, held, target) : [];
        return Option.toArray(
            Option.map(
                Option.filter(target, () => Array.isReadonlyArrayNonEmpty(words)),
                (found) => ({ words, target: found }),
            ),
        );
    });
const wordsOf = (given: CommandLine.TemplateExpression) => Content.Words({ words: Array.ensure(given).map((item) => (Predicate.isString(item) || Redacted.isRedacted(item) ? item : `${item}`)), listed: Array.isArray(given) });
const isNode = (given: CommandLine.TemplateExpression | Word | WordPart): given is Word | WordPart => Predicate.hasProperty(given, 'type') && Predicate.isString(given.type);
const carry = (node: Word | WordPart, origin: Option.Option<Script>): Result.Result<Content, Refused> =>
    Result.fromOption(
        Option.gen(function* () {
            const { tree, text } = yield* origin;
            const path = tree.path(node);
            const held = yield* Array.get(path, path.length - (node.type === 'Word' ? 1 : 2));
            const word = yield* Option.liftPredicate(held.node, (found): found is Word => found.type === 'Word');
            const quoted = path.slice(0, -1).some(({ node: outer }) => outer.type === 'CmdSubst' && outer.backquotes);
            yield* Option.liftPredicate(word, (found) => tree.role(found)._tag === 'Argument' && !quoted);
            return Content.Source({ fragment: text.slice(tree.start(node), tree.located(node.end)), command: false });
        }),
        refusal('Value'),
    );
const marker = (strings: readonly string[], base = 'hole'): string => (strings.some((part) => part.includes(base)) ? marker(strings, `${base}_`) : base);
const interpolate = Effect.fnUntraced(function* (
    at: { readonly parse: (source: string) => Effect.Effect<File, ShellError>; readonly programs: (program: string) => Effect.Effect<Option.Option<Usage>, ShellError>; readonly dialect: Dialect; readonly origin: Option.Option<Script> },
    strings: TemplateStringsArray,
    holes: readonly (CommandLine.TemplateExpression | Word | WordPart)[],
) {
    const invalid = strings.findIndex(Predicate.isUndefined);
    yield* Effect.when(ShellError.of({ _tag: 'Construction', hole: invalid, kind: 'Escape' }, `text ${invalid}`)(), Effect.succeed(invalid !== -1));
    const base = marker(strings);
    const pattern = new RegExp(String.raw`\+${base}(\d+)_`, 'gu');
    const marked = strings.reduce((written, part, order) => `${written}+${base}${order - 1}_${part}`);
    const syntax = yield* Effect.catchReason(at.parse(marked), 'ShellError', 'Syntax', (reason, error) => ShellError.of({ ...reason, offset: Option.map(reason.offset, (found) => marked.slice(0, found).replaceAll(pattern, '').length) }, error.subject)(error.cause));
    const tree = index(marked, syntax);
    const contents = holes.map((given) => (isNode(given) ? carry(given, at.origin) : Result.succeed(wordsOf(given))));
    const raw = contents.map(Result.match({ onFailure: () => '', onSuccess: Content.$match({ Words: ({ words }) => words.map(reveal).join(' '), Source: ({ fragment }) => fragment.source }) }));
    const fill = (text: string, by: readonly string[]) => text.replaceAll(pattern, (_, order: string) => by[Number(order)] ?? '');
    const literal = (word: Word) => Option.map(unquote(word, at.dialect), (written) => fill(written, raw));
    const marks = tree.links
        .keys()
        .flatMap((leaf) => (leaf.type === 'Lit' || leaf.type === 'SglQuoted' ? leaf.value.matchAll(pattern).map(([found, order = '']) => ({ order: Number(order), found, leaf })) : []))
        .toArray();
    const sites = yield* Effect.forEach(
        marks,
        Effect.fnUntraced(function* ({ order, found, leaf }) {
            const path = tree.path(leaf);
            const quoting = quotingAt(tree, leaf, (word) => shapeOf(word).replace(found, hole), strings[order] ?? '', at.dialect);
            const evaluated = yield* Effect.findFirst(commandsAt(path, literal), ({ words, target }) => evaluates(at.programs, words, target));
            const bodies = path.flatMap(({ node }) => (node.type === 'Word' && Option.isSome(tree.heredoc(node)) ? [{ order, body: node }] : []));
            return { order, quoting: Option.filter(quoting, () => Option.isNone(evaluated)), bodies };
        }),
    );
    const quotings = new Map(sites.map(({ order, quoting }) => [order, quoting]));
    const fragments = yield* Effect.forEach(contents, (content, order) =>
        Result.flatMap(content, (held) => quote(Option.flatten(Option.fromUndefinedOr(quotings.get(order))), held, at.dialect)).pipe(
            Effect.fromResult,
            Effect.mapError(({ kind, cause }) => ShellError.of({ _tag: 'Construction', hole: order, kind }, `hole ${order}`)(cause)),
        ),
    );
    const bodies = Array.dedupeWith(
        sites.flatMap((site) => site.bodies),
        (left, right) => left.body === right.body,
    );
    const sources = fragments.map(({ source }) => source);
    const ended = Array.findFirst(bodies, ({ body }) => {
        const text = marked.slice(tree.start(body), tree.end(body));
        const [, spans] = Array.mapAccum(text.matchAll(pattern), 0, (shift, found) => {
            const [start, put] = [found.index + shift, fill(found[0], sources)];
            return [shift + put.length - found[0].length, [start, start + put.length] as const];
        });
        return !keeps(tree, body, at.dialect, fill(text, sources), spans);
    });
    yield* Effect.transposeOption(Option.map(ended, ({ order }) => ShellError.of({ _tag: 'Construction', hole: order, kind: 'Delimiter' }, `hole ${order}`)()));
    return Fragment.ReducerConcat.combineAll(strings.flatMap((text, order) => [Fragment.of(text), ...Array.fromNullishOr(fragments[order])]));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Content, grammarOf, hole, interpolate, keeps, quote, Quoting, quotingAt, type Refused, refusal, shapeOf };
