import { Array, Data, Effect, Function, Match, Option, Predicate, Redacted, Result } from 'effect';
import { CommandLine } from 'effect/process';
import { BinTestOperator, createSourceIndex, type File, LangVariant, type Node, type Redirect, RedirOperator, UnTestOperator, walk, type Word, type WordPart } from 'sh-syntax';
import { bound, texted } from '#/analysis/binding';
import type { Written } from '#/analysis/invocation';
import { text } from '#/evaluation/expansion';
import { Field, Origin } from '#/evaluation/value';
import type { Usage } from '#/programs/usage';
import type { Dialect } from '#/syntax/dialect';
import { type Script, ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Site = Data.TaggedEnum<{ Bare: { readonly whole: boolean; readonly command: boolean }; Double: object; Single: object; Body: { readonly expands: boolean } }>;
interface Link {
    readonly node: Node;
    readonly prop: string | null;
    readonly index: number | null;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Site = Data.taggedEnum<Site>();
const escaped = /(?<!\\)(?:\\\\)*[\\$]$/u;
const evaluating: ReadonlySet<number> = new Set([BinTestOperator.TsEql, BinTestOperator.TsNeq, BinTestOperator.TsLeq, BinTestOperator.TsGeq, BinTestOperator.TsLss, BinTestOperator.TsGtr, UnTestOperator.TsVarSet, UnTestOperator.TsRefVar]);
const modifiers: ReadonlySet<string> = new Set(['command', 'builtin', 'exec', 'noglob', 'nocorrect']);
const running: readonly (readonly [ReadonlySet<string>, RegExp])[] = [
    [new Set(['read', 'integer', 'float']), /^/u],
    [new Set(['printf', 'print', 'test', '[']), /^-v$/u],
    [new Set(['declare', 'typeset', 'local', 'export', 'readonly']), /^-\w*[aAin]/u],
];

// --- [OPERATIONS] ----------------------------------------------------------------------

const opened = (part: WordPart): string => (part.type === 'Lit' ? part.value : '\u0001');
const masked = (found: string) => (word: Word) => word.parts.map(opened).join('').replace(found, '\0');
const delimited = (body: string, spans: readonly (readonly [number, number])[], dash: boolean, delimiter: Option.Option<string>): boolean => {
    const lines = new Set(Array.dropRight(body.split('\n'), 1).map((line) => (dash ? line.replace(/^\t+/u, '') : line)));
    const tabbed = spans.some(([from, to]) => body.slice(from, to + 1).includes('\n\t') || (body[from] === '\t' && /^\t*$/u.test(body.slice(body.lastIndexOf('\n', from - 1) + 1, from))));
    return (body === '' || body.endsWith('\n')) && !(dash && tabbed) && !Option.exists(delimiter, (closing) => lines.has(closing));
};
const unused = (strings: readonly string[], base = 'hole'): string => (strings.some((part) => part.includes(base)) ? unused(strings, `${base}_`) : base);
const executed = (path: readonly Link[], literal: (word: Word) => Option.Option<string>): boolean =>
    path
        .flatMap(({ node }, at) =>
            Match.value({ node, link: path[at + 1] }).pipe(
                Match.when({ node: { type: 'CallExpr' }, link: { prop: 'args' } }, ({ node: call, link }) => [call.args.slice(0, link.index ?? 0).map(literal)]),
                Match.when({ node: { type: 'DeclClause' }, link: { prop: 'args' } }, ({ node: clause, link }) => [[Option.fromNullishOr(clause.variant?.value), ...clause.args.slice(0, link.index ?? 0).map(({ value }) => Option.flatMap(Option.fromNullishOr(value), literal))]]),
                Match.orElse(() => []),
            ),
        )
        .map((words) => Array.dropWhile(words, (word) => Option.exists(word, (name) => modifiers.has(name) || name.startsWith('-'))).map(Option.map((written) => written.slice(written.lastIndexOf('/') + 1))))
        .some(([program = Option.none(), ...rest]) =>
            Option.exists(
                Option.flatMap(program, (name) => Array.findFirst(running, ([programs]) => programs.has(name))),
                ([, flag]) => [...Option.toArray(program), ...Array.getSomes(rest)].some((word) => flag.test(word)),
            ),
        );
const coded = Effect.fnUntraced(function* (programs: (program: string) => Effect.Effect<Option.Option<Usage>, ShellError>, words: readonly Written[], target: Word | undefined): Effect.fn.Return<boolean, ShellError> {
    const kept = Array.dropWhile(words, (word) => Option.exists(texted(word), (name) => modifiers.has(name)));
    const program = Option.map(Option.flatMap(Array.head(kept), texted), (name) => name.slice(name.lastIndexOf('/') + 1));
    const binding = bound(Option.flatten(yield* Effect.transposeOption(Option.map(program, programs))), kept, Field.known('/'));
    const holds = (held: readonly Written[]) => held.some(({ node }) => Option.exists(node, (word) => word === target));
    return holds(binding.running) || Option.isSome(yield* Effect.findFirst(binding.lists.filter(holds), (list) => coded(programs, list, target)));
});
const sited = (path: readonly Link[], marked: (word: Word) => string, preceding: string, dialect: Dialect): Option.Option<Site> => {
    const [leaf, parent] = [path.at(-1)?.node, path.at(-2)?.node];
    const word = Option.flatMap(
        Array.findLastIndex(path, ({ node }) => node.type === 'Word'),
        (at) => Option.all({ word: Array.get(path, at), outer: Array.get(path, at - 1) }),
    );
    const shape = Option.match(word, { onNone: () => '', onSome: ({ word: { node } }) => (node.type === 'Word' ? marked(node) : '') });
    const braced = dialect.variant === LangVariant.LangZsh && dialect.options.braceexpand && /\{[^{},]*\0[^{},]*\}/u.test(shape);
    const escaping = escaped.test(preceding);
    const context = (whole: boolean, command = false) =>
        Option.some(
            Match.value({ leaf: leaf?.type, parent: parent?.type }).pipe(
                Match.when({ leaf: 'SglQuoted' }, () => Site.Single()),
                Match.when({ leaf: 'DblQuoted' }, () => Site.Double()),
                Match.when({ parent: 'DblQuoted' }, () => Site.Double()),
                Match.orElse(() => Site.Bare({ whole, command: whole && command })),
            ),
        );
    const placed = Option.flatMap(word, ({ word: { prop, index }, outer: { node } }) =>
        Match.value({ node, prop }).pipe(
            Match.when({ node: { type: 'Redirect' }, prop: 'hdoc' }, ({ node: redirect }) => Option.some(Site.Body({ expands: redirect.word?.parts.every((part) => part.type === 'Lit' && !part.value.includes('\\')) ?? false }))),
            Match.when({ node: { type: 'Redirect', op: Match.is(RedirOperator.Hdoc, RedirOperator.DashHdoc) } }, () => Option.none()),
            Match.when({ node: { type: Match.is('CallExpr', 'WordIter', 'ArrayElem') }, prop: Match.is('args', 'items', 'value') }, () => context(shape === '\0', node.type === 'CallExpr' && index === 0)),
            Match.when({ node: { type: 'Assign', naked: false }, prop: 'value' }, () => context(false)),
            Match.when({ node: { type: Match.is('BinaryTest', 'UnaryTest'), op: (op: number) => evaluating.has(op) } }, () => Option.none()),
            Match.when({ node: { type: 'BinaryTest', op: BinTestOperator.TsReMatch }, prop: 'y' }, () => (dialect.variant === LangVariant.LangBash ? context(false) : Option.none())),
            Match.when({ node: { type: Match.is('Assign', 'WordIter', 'ArrayElem', 'RepeatClause', 'CoprocClause', 'ArithmExp', 'ArithmCmd', 'LetClause', 'CStyleLoop', 'BinaryArithm', 'UnaryArithm', 'ParenArithm', 'CallArithm', 'FlagsArithm', 'ParamExp') } }, () => Option.none()),
            Match.orElse(() => context(false)),
        ),
    );
    const refused = (leaf?.type === 'SglQuoted' && leaf.dollar) || (leaf?.type === 'Lit' && parent?.type !== 'Word' && parent?.type !== 'DblQuoted') || path.some(({ node }) => node.type === 'CmdSubst' && node.backquotes);
    return refused ? Option.none() : Option.filter(placed, (site) => !(braced && Site.$is('Bare')(site)) && (Site.$is('Single')(site) || (Site.$is('Body')(site) && !site.expands) || !escaping));
};
const rendered = (site: Option.Option<Site>, hole: CommandLine.TemplateExpression, dialect: Dialect): Result.Result<{ readonly text: string; readonly redacted: Script['redacted'] }, { readonly kind: 'Value' | 'Position'; readonly cause?: unknown }> => {
    const words = Array.ensure(hole).map((item) => (Predicate.isString(item) || Redacted.isRedacted(item) ? item : `${item}`));
    const raw = words.map((word) => (Redacted.isRedacted(word) ? Redacted.value(word) : word));
    const aliased = (word: string, command: boolean) => Option.exists(Option.fromUndefinedOr(dialect.aliases.get(word)), ({ global }) => global || (command && dialect.options.aliases)) || (command && dialect.suffixes.has(word.slice(word.lastIndexOf('.') + 1)) && word.includes('.'));
    return Result.flatMap(
        Result.mapError(Result.all(words.map((word, at) => (Option.exists(site, (found) => Site.$is('Bare')(found) && found.command && at === 0) ? CommandLine.join([word]) : CommandLine.quote(word)))), (cause) => ({ kind: 'Value' as const, cause })),
        (quoted) =>
            Option.match(
                Option.filter(
                    Option.map(
                        site,
                        Site.$match({
                            Bare: ({ whole, command }) => quoted.map((written, at) => (written === raw[at] && (!whole || aliased(written, command && at === 0)) ? `'${written}'` : written)),
                            Double: () => raw.map((written) => written.replaceAll(/[\\$`"]/gu, String.raw`\$&`)),
                            Single: () => raw.map((written) => written.replaceAll("'", String.raw`'\''`)),
                            Body: ({ expands }) => raw.map((written) => (expands ? written.replaceAll(/[\\$`]/gu, String.raw`\$&`) : written)),
                        }),
                    ),
                    () => Option.exists(site, (found) => Site.$is('Bare')(found) && found.whole) || !Array.isArray(hole),
                ),
                {
                    onNone: () => Result.fail({ kind: 'Position' as const }),
                    onSome: (texts) => {
                        const [, spans] = Array.mapAccum(Array.zip(texts, words), 0, (at, [piece, word]) => [at + piece.length + 1, Redacted.isRedacted(word) ? [{ start: at, end: at + piece.length, shown: word }] : []]);
                        return Result.succeed({ text: texts.join(' '), redacted: spans.flat() });
                    },
                },
            ),
    );
};
const constructed = Effect.fnUntraced(function* (parse: (source: string) => Effect.Effect<File, ShellError>, dialect: Dialect, strings: TemplateStringsArray, holes: readonly CommandLine.TemplateExpression[], programs: (program: string) => Effect.Effect<Option.Option<Usage>, ShellError>) {
    const invalid = strings.findIndex(Predicate.isUndefined);
    yield* Effect.when(ShellError.of({ _tag: 'Construction', hole: invalid, kind: 'Escape' }, `text ${invalid}`)(), Effect.succeed(invalid !== -1));
    const base = unused(strings);
    const pattern = new RegExp(String.raw`\+${base}(\d+)_`, 'gu');
    const marked = strings.reduce((written, part, index) => `${written}+${base}${index - 1}_${part}`);
    const tree = yield* Effect.catchReason(parse(marked), 'ShellError', 'Syntax', (reason, error) => ShellError.of({ ...reason, offset: Option.map(reason.offset, (at) => marked.slice(0, at).replaceAll(pattern, '').length) }, error.subject)(error.cause));
    const raw = holes.map((hole) =>
        Array.ensure(hole)
            .map((item) => (Redacted.isRedacted(item) ? Redacted.value(item) : item))
            .join(' '),
    );
    const literal = (word: Word) => Option.map(text(word, dialect), (written) => written.replaceAll(pattern, (_, hole: string) => raw[Number(hole)] ?? ''));
    const path: Link[] = [];
    const marks: { readonly hole: number; readonly site: Option.Option<Site>; readonly calls: readonly { readonly words: readonly Written[]; readonly target: Word | undefined }[]; readonly heredocs: readonly { readonly redirect: Redirect; readonly body: Word }[] }[] = [];
    walk(tree, {
        enter: (node, _, prop, index) => {
            path.push({ node, prop, index });
            const written = node.type === 'Lit' || node.type === 'SglQuoted' ? node.value : '';
            const links = Array.window(path, 2);
            const calls = links.flatMap(([outer, inner]) =>
                outer.node.type === 'CallExpr' && inner.prop === 'args'
                    ? [{ words: outer.node.args.map((word): Written => ({ field: Option.getOrElse(Option.map(literal(word), Field.known), Function.constant(Field.unknown(Origin.Unsupported()))), exact: true, node: Option.some(word) })), target: outer.node.args[inner.index ?? 0] }]
                    : [],
            );
            const heredocs = links.flatMap(([outer, inner]) => (outer.node.type === 'Redirect' && inner.node.type === 'Word' && inner.prop === 'hdoc' ? [{ redirect: outer.node, body: inner.node }] : []));
            marks.push(...written.matchAll(pattern).map(([found, hole = '']) => ({ hole: Number(hole), site: executed(path, literal) ? Option.none() : sited(path, masked(found), strings[Number(hole)] ?? '', dialect), calls, heredocs })));
        },
        exit: () => {
            path.pop();
        },
    });
    const sites = new Map(
        yield* Effect.forEach(
            marks,
            Effect.fnUntraced(function* ({ hole, site, calls }) {
                const ran = yield* Effect.findFirst(calls, ({ words, target }) => coded(programs, words, target));
                return [hole, Option.isSome(ran) ? Option.none<Site>() : site] as const;
            }),
        ),
    );
    const segments = yield* Effect.forEach(holes, (hole, index) => Effect.mapError(Effect.fromResult(rendered(sites.get(index) ?? Option.none(), hole, dialect)), ({ kind, cause }) => ShellError.of({ _tag: 'Construction', hole: index, kind }, `hole ${index}`)(cause)));
    const texts = segments.map(({ text: written }) => written);
    const located = createSourceIndex(marked);
    const delimiting = Array.findFirst(
        Array.dedupeWith(
            marks.flatMap(({ hole, heredocs }) => heredocs.map((heredoc) => ({ hole, ...heredoc }))),
            (left, right) => left.redirect === right.redirect,
        ),
        ({ redirect, body }) => {
            const lines = marked.slice(located(body.start), located(body.end));
            const [written, spans] = Array.mapAccum(lines.slice(0, lines.lastIndexOf('\n') + 1).split(pattern), '', (held, piece, at) => (at % 2 === 0 ? [`${held}${piece}`, []] : [`${held}${texts[Number(piece)] ?? ''}`, [[held.length, held.length + (texts[Number(piece)] ?? '').length] as const]]));
            const delimiter = Option.flatMap(Option.fromNullishOr(redirect.word), (word) => text(word, dialect));
            return !Option.exists(delimiter, () => delimited(written, spans.flat(), redirect.op === RedirOperator.DashHdoc, delimiter));
        },
    );
    yield* Option.match(delimiting, { onNone: () => Effect.void, onSome: ({ hole }) => ShellError.of({ _tag: 'Construction', hole, kind: 'Delimiter' }, `hole ${hole}`)() });
    const [source, spans] = Array.mapAccum(segments, strings[0] ?? '', (written, segment, index) => [`${written}${segment.text}${strings[index + 1] ?? ''}`, segment.redacted.map((span) => ({ ...span, start: span.start + written.length, end: span.end + written.length }))]);
    return { source, redacted: spans.flat() };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { constructed, delimited, executed, type Link, opened, rendered, Site, sited };
