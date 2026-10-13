import indesign from 'adobe:indesign';
import { Array, Effect, Match, Option, Predicate, pipe, Record, Result, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import MagicString from 'magic-string';
import regjsparser, { type AstNode, type CharacterClassBody, type UnicodePropertyEscape, type Value } from 'regjsparser';
import aliases from 'unicode-property-value-aliases';
import { Find, Queries } from './contract.ts';
import { active, Callable, type Checks, missing, prepared, read, resolve, type Untranslatable, unwired } from './dom.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refusal = Untranslatable['constructs'][number];
type Rows = readonly Result.Result<readonly [start: number, end: number, text: string, construct: string], Refusal>[];

// --- [CONSTANTS] -----------------------------------------------------------------------

const SETS = /--|&&|\\q\{/u;
const ASCII = /^[dw]$/u;
const POSIX = /\[\[[:=]/u;
const CATEGORY = /^(?:gc|General_Category)=/u;
const PREFIXED = /^Is/u;
const DIGITS = /^\d+$/u;
const TOKEN = /\\Q[\s\S]*?(?:\\E|$)|\\(?<escape>[\s\S])|\[\^?\]?(?:\\[\s\S]|\[[:=.][\s\S]*?[:=.]\]|[^\]\\])*\]|\(\?\([^)]*\)|\(\?P<(?<python>\w+)>|\(\?P=(?<back>\w+)\)|\(\?(?<recursion>R|[+-]?\d+|&\w+)\)|\(\?<(?<named>[A-Za-z_]\w*)>|(?<plain>\((?![?*]))|(?<trailing>\\$)/gu;
const CHANGE = /\\(?<back>[1-9])|(?<whole>\$&)|\$\{(?<braced>\w+)\}|\$<(?<angled>\w+)>|(?<dollar>\$\$)|\\u(?:(?<hex>[\dA-Fa-f]{4})|\{(?<point>[\dA-Fa-f]+)\})|(?<many>\$\d{2,})|\\E(?<rest>[\s\S]*)|\\[\s\S]/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const grep = ((): ((sought: Option.Option<string>) => Checks) => {
    const radix = 16;
    const features = { lookbehind: true, namedGroups: true, unicodePropertyEscape: true, unicodeSet: true } as const;
    const general = aliases.get('General_Category');
    const classes = new Map<string, string>();
    const hex = (point: number): string => `\\x{${point.toString(radix).toUpperCase()}}`;
    const compiles = (source: string): boolean => Result.isSuccess(Result.try(() => new RegExp(source, 'u')));
    const escapes = (source: string): readonly string[] => Array.flatMap(Array.fromIterable(source.matchAll(TOKEN)), ({ groups }) => Array.fromNullishOr(groups?.escape));
    const derived = (source: string, flags: 'u' | 'v'): string =>
        Option.getOrElse(Option.fromUndefinedOr(classes.get(source)), () => {
            const last = 0x10_ff_ff;
            const member = new RegExp(`^${source}$`, flags);
            const inside = Array.makeBy(last + 2, (point) => point <= last && String.fromCodePoint(point).isWellFormed() && member.test(String.fromCodePoint(point)));
            const edges = Array.filter(Array.range(0, last + 1), (point) => inside[point] !== (inside[point - 1] ?? false));
            const side = (parity: number): readonly number[] => Array.filter(edges, (_, index) => index % 2 === parity);
            const ranges = Array.join(
                Array.zipWith(side(0), side(1), (first, after) => (first === after - 1 ? hex(first) : `${hex(first)}-${hex(after - 1)}`)),
                '',
            );
            classes.set(source, ranges);
            return ranges;
        });
    const scanned = (source: string, tokens: RegExp, handlers: Record.ReadonlyRecord<string, (captured: string) => Result.Result<string, Refusal['alternative']>>): Rows =>
        Array.flatMap(Array.fromIterable(source.matchAll(tokens)), (token) =>
            pipe(
                Record.toEntries(token.groups ?? {}),
                Array.filter(([, captured]) => Predicate.isNotUndefined(captured)),
                Array.flatMap(([name, captured]) => Option.toArray(Option.map(Record.get(handlers, name), (handler) => handler(captured)))),
                Array.map(Result.mapBoth({ onSuccess: (text) => [token.index, token.index + token[0].length, text, token[0]] as const, onFailure: (alternative) => ({ construct: token[0], alternative }) })),
            ),
        );
    const settled = (source: string, rows: Rows): Result.Result<readonly [text: string, notes: readonly string[]], Array.NonEmptyReadonlyArray<Refusal>> => {
        const [kept, refused] = Array.separate(rows);
        const applied = Array.filter(kept, ([start, end, text]) => source.slice(start, end) !== text);
        return Array.match(refused, {
            onNonEmpty: Result.fail,
            onEmpty: () =>
                Result.succeed([
                    Array.reduce(applied, new MagicString(source), (spliced, [start, end, text]) => (start === end ? spliced.appendLeft(start, text) : spliced.overwrite(start, end, text))).toString(),
                    Array.match(Array.dedupe(Array.map(applied, ([, , , construct]) => construct)), { onEmpty: () => [], onNonEmpty: (rewritten) => [`rewritten for InDesign: ${Array.join(rewritten, ' ')}`] }),
                ]),
        });
    };
    const total = (nodes: readonly AstNode<typeof features>[]): Option.Option<number> =>
        Option.map(
            Option.all(Array.map(nodes, width)),
            Array.reduce(0, (sum, each) => sum + each),
        );
    const width = (node: AstNode<typeof features>): Option.Option<number> => {
        switch (node.type) {
            case 'anchor':
                return Option.some(0);
            case 'alternative':
                return total(node.body);
            case 'disjunction':
                return Option.flatMap(Option.all(Array.map(node.body, width)), (widths) => Option.liftPredicate(Array.headNonEmpty(widths), () => new Set(widths).size === 1));
            case 'group':
                return node.behavior === 'normal' || node.behavior === 'ignore' ? total(node.body) : Option.some(0);
            case 'quantifier':
                return Option.flatMap(total(node.body), (each) => Option.liftPredicate(node.min * each, () => node.min === node.max));
            case 'reference':
                return Option.none();
            default:
                return Option.some(1);
        }
    };
    const walked = (script: boolean, multiline: boolean, sets: boolean): ((node: AstNode<typeof features>, branched: boolean) => Rows) => {
        const [word, wild] = [derived('\\w', 'u'), multiline ? '[^\\r]' : '(?s:.)'];
        const point = ({ kind, codePoint, raw }: Value): string => (kind === 'unicodeEscape' || kind === 'unicodeCodePointEscape' || String.fromCodePoint(codePoint).length > 1 ? hex(codePoint) : raw);
        const shorthand = (value: string): Option.Option<readonly [negated: boolean, set: string]> =>
            Option.map(
                Option.liftPredicate(value.toLowerCase(), (lower) => script && ASCII.test(lower)),
                (lower) => [value !== lower, derived(`\\${lower}`, 'u')],
            );
        const property = ({ value, negative, raw }: UnicodePropertyEscape): Result.Result<string, Refusal> => {
            const category = value.replace(CATEGORY, '');
            const names = Array.flatMap([value, value.replace(PREFIXED, '')], (name) => [name, `Script=${name}`]);
            return compiles(`\\p{gc=${category}}`)
                ? Result.succeed(`\\${negative ? 'P' : 'p'}{${category.length === 1 ? (general?.get(category) ?? category) : category}}`)
                : pipe(
                      Array.findFirst(names, (name) => compiles(`\\p{${name}}`)),
                      Result.fromOption((): Refusal => ({ construct: raw, alternative: Option.none() })),
                      Result.map((name) => `[${negative ? '^' : ''}${derived(`\\p{${name}}`, 'u')}]`),
                  );
        };
        const member = (node: CharacterClassBody): Result.Result<readonly [inside: readonly string[], apart: readonly string[]], Refusal> => {
            switch (node.type) {
                case 'unicodePropertyEscape':
                    return Result.map(property(node), (text) => [[], [text]]);
                case 'characterClassEscape':
                    return Result.succeed(Option.match(shorthand(node.value), { onNone: () => [[node.raw], []], onSome: ([negated, set]) => (negated ? [[], [`[^${set}]`]] : [[set], []]) }));
                case 'characterClassRange':
                    return Result.succeed([[`${point(node.min)}-${point(node.max)}`], []]);
                default:
                    return Result.succeed([[point(node)], []]);
            }
        };
        const splices = (node: AstNode<typeof features>, branched: boolean): Rows => {
            const [start, end] = node.range;
            const put = (text: string): Rows => [Result.succeed([start, end, text, node.raw])];
            switch (node.type) {
                case 'value':
                    return put(point(node));
                case 'anchor':
                    return Array.flatMap(
                        {
                            start: branched ? [`(?<!${wild})`] : [],
                            end: branched ? [`(?!${wild})`] : [],
                            boundary: script ? [`(?:(?<=[${word}])(?![${word}])|(?<![${word}])(?=[${word}]))`] : [],
                            'not-boundary': script ? [`(?:(?<=[${word}])(?=[${word}])|(?<![${word}])(?![${word}]))`] : [],
                        }[node.kind],
                        put,
                    );
                case 'characterClassEscape':
                    return Array.flatMap(Option.toArray(shorthand(node.value)), ([negated, set]) => put(`[${negated ? '^' : ''}${set}]`));
                case 'unicodePropertyEscape':
                    return [Result.map(property(node), (text) => [start, end, text, node.raw])];
                case 'characterClass': {
                    const [members, refused] = Array.separate(sets ? [] : Array.map(node.body, member));
                    const kept = Array.join(
                        Array.flatMap(members, ([inside]) => inside),
                        '',
                    );
                    const built = Array.match(
                        Array.flatMap(members, ([, apart]) => apart),
                        {
                            onEmpty: () => `[${node.negative ? '^' : ''}${kept}]`,
                            onNonEmpty: (pulled) => `(?:${node.negative ? '(?!' : ''}${Array.join([...(kept === '' ? [] : [`[${kept}]`]), ...pulled], '|')}${node.negative ? ')(?s:.)' : ''})`,
                        },
                    );
                    return Array.match(refused, { onNonEmpty: Array.map(Result.fail), onEmpty: () => put(sets ? `[${derived(node.raw, 'v')}]` : built) });
                }
                case 'disjunction':
                    return Array.flatMap(node.body, (branch) => splices(branch, true));
                case 'alternative':
                case 'quantifier':
                    return Array.flatMap(node.body, (each) => splices(each, branched));
                case 'group': {
                    const [negative, [only]] = [node.behavior === 'negativeLookbehind', node.body];
                    const [behind, open] = [negative || node.behavior === 'lookbehind', `(?<${negative ? '!' : '='}`];
                    const branches = node.body.length === 1 && only?.type === 'disjunction' ? only.body : [];
                    const widths = Array.match(branches, { onEmpty: () => [total(node.body)], onNonEmpty: Array.map(width) });
                    const split: Rows =
                        behind && new Set(Array.getSomes(widths)).size > 1
                            ? [
                                  ...(negative ? [] : [Result.succeed([start, start + open.length, `(?:${open}`, node.raw] as const), Result.succeed([end - 1, end, '))', node.raw] as const)]),
                                  ...Array.zipWith(branches, Array.drop(branches, 1), (left, right) => Result.succeed([left.range[1], right.range[0], `)${negative ? '' : '|'}${open}`, node.raw] as const)),
                              ]
                            : [];
                    return behind && Array.some(widths, Option.isNone) ? [Result.fail({ construct: node.raw, alternative: Option.some('\\K') })] : [...split, ...Array.flatMap(node.body, (each) => splices(each, branched))];
                }
                default:
                    return [];
            }
        };
        return splices;
    };
    const found = (value: string | RegExp): ReturnType<typeof settled> => {
        const script = Predicate.isRegExp(value);
        const source = script ? value.source : value;
        const told = escapes(source);
        return Result.flatMap(
            settled(source, [
                ...(source === '' ? [Result.fail({ construct: source, alternative: Option.none() })] : []),
                ...scanned(source, TOKEN, {
                    python: (name) => Result.succeed(`(?<${name}>`),
                    back: (name) => Result.succeed(`\\k<${name}>`),
                    recursion: () => Result.fail(Option.none()),
                    trailing: () => Result.fail(Option.some('\\\\')),
                }),
            ]),
            ([plain, tokens]) => {
                const modes: readonly ('u' | 'v')[] = SETS.test(plain) && !POSIX.test(plain) ? ['v', 'u'] : ['u'];
                const tree = Array.findFirst(modes, (mode) => Option.map(Result.getSuccess(Result.try(() => regjsparser.parse(plain, mode, features))), (root) => [mode, root] as const));
                return Result.map(
                    settled(plain, [
                        ...(script ? [Result.succeed([0, 0, `${value.ignoreCase ? '(?i)' : ''}${value.dotAll ? '(?s)' : ''}${value.multiline ? '' : '(?-m)'}`, `/${value.flags}`] as const)] : []),
                        ...Option.match(tree, { onNone: () => [], onSome: ([mode, root]) => walked(script, !script || value.multiline, mode === 'v')(root, false) }),
                    ]),
                    ([text, notes]) => [text, [...tokens, ...notes, ...(Array.contains(told, 'n') && !Array.contains(told, 'r') ? ['\\n matches a forced line break, \\r ends a paragraph'] : [])]],
                );
            },
        );
    };
    const changed = (value: string, sought: Option.Option<string>): ReturnType<typeof settled> => {
        const last = 9;
        const groups = Array.getSomes(Array.map(Array.fromIterable(Option.getOrElse(sought, () => '').matchAll(TOKEN)), ({ groups: token }) => Array.findFirst([token?.named, token?.python, token?.plain], Predicate.isNotUndefined)));
        const ordinal = (reference: string): Result.Result<string, Refusal['alternative']> =>
            pipe(
                DIGITS.test(reference)
                    ? Option.some(Number(reference))
                    : Option.map(
                          Array.findFirstIndex(groups, (name) => name === reference),
                          (at) => at + 1,
                      ),
                Option.filter((index) => index <= last),
                Result.fromOption(() => Option.none<string>()),
                Result.map((index) => `$${index}`),
            );
        const point = (digits: string): Result.Result<string, Refusal['alternative']> => Result.succeed(`\\x{${digits}}`);
        return settled(
            value,
            scanned(value, CHANGE, {
                back: (digit) => Result.succeed(`$${digit}`),
                whole: () => Result.succeed('$0'),
                braced: ordinal,
                angled: ordinal,
                dollar: () => Result.succeed('\\$'),
                hex: point,
                point,
                many: () => Result.fail(Option.none()),
                rest: (after) =>
                    Result.liftPredicate(
                        '',
                        () => after === '',
                        () => Option.none(),
                    ),
            }),
        );
    };
    const sound = (expression: string): Effect.Effect<number, Untranslatable> => {
        const probe =
            'var pattern = arguments[0], sentinel = arguments[1], hits = 0, done = false, scratch = app.documents.add(false); try { app.doScript(function () { var frame = scratch.textFrames.add(); frame.contents = sentinel; app.findGrepPreferences = NothingEnum.NOTHING; app.findChangeGrepOptions = NothingEnum.NOTHING; app.findGrepPreferences.findWhat = pattern; hits = frame.parentStory.findGrep().length; done = true; throw 0; }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT); } catch (error) { if (!done) throw error; } finally { scratch.close(SaveOptions.NO); } hits';
        const sentinel = 'galleysentinel';
        return Effect.orDie(read(Schema.Int, () => indesign.app.doScript(probe, indesign.ScriptLanguage.JAVASCRIPT, [Array.contains(escapes(expression), 'K') ? `${expression}|${sentinel}` : `(?:${expression})|${sentinel}`, sentinel]))).pipe(
            Effect.filterOrFail(
                (count) => count > 0,
                (): Untranslatable => ({ pattern: expression, constructs: [{ construct: expression, alternative: Option.none() }] }),
            ),
        );
    };
    const raised =
        (value: unknown) =>
        (refused: Array.NonEmptyReadonlyArray<Refusal>): Untranslatable => ({ pattern: String(value), constructs: refused });
    const pattern = (value: unknown): Effect.Effect<readonly [value: unknown, notes: readonly string[]], Untranslatable> => (Predicate.isString(value) || Predicate.isRegExp(value) ? Effect.tap(Effect.mapError(Effect.fromResult(found(value)), raised(value)), ([text]) => sound(text)) : Effect.succeed([value, []]));
    return (sought) => ({
        FindGrepPreference: { findWhat: pattern },
        ChangeGrepPreference: { changeTo: (value: unknown): ReturnType<typeof pattern> => (Predicate.isString(value) ? Effect.mapError(Effect.fromResult(changed(value, sought)), raised(value)) : Effect.succeed([value, []])) },
        NestedGrepStyle: { grepExpression: pattern },
    });
})();
const roles = <M extends Rpc.Payload<typeof Find>['mode']>(mode: M): { readonly names: readonly [`find${M}Preferences`, `change${M}Preferences`, `findChange${M}Options`]; readonly search: string; readonly methods: readonly [`find${M}`, `change${M}`] } => ({
    names: [`find${mode}Preferences`, `change${mode}Preferences`, `findChange${mode}Options`] as const,
    search: `${mode.toUpperCase()}_SEARCH`,
    methods: [`find${mode}`, `change${mode}`] as const,
});
const find = Effect.fnUntraced(function* ({ mode, find: sought, action, options, scope: region, query, save }: Rpc.Payload<typeof Find>, limit: Option.Option<number>) {
    const { app } = indesign;
    const reach = 20;
    const { names, search, methods } = roles(mode);
    const rendered = (contents: unknown): Option.Option<string> =>
        Option.map(Option.fromNullishOr(contents), (given) =>
            Array.join(
                Array.map(Array.ensure(given), (each) => (each instanceof indesign.Enumerator ? `<${String(each)}>` : String(each))),
                '',
            ),
        );
    const targets = Array.ensure(yield* Option.match(region, { onSome: resolve, onNone: () => Effect.flatMap(active, (document) => resolve(document.toSpecifier())) }));
    yield* Effect.forEach(targets, (each) => (Predicate.isFunction(each[methods[0]]) ? Effect.void : Effect.fail(missing(`${mode} search of ${each.toSpecifier()}`, mode, []))), { discard: true });
    const found = yield* prepared(
        app[names[0]],
        Option.getOrElse(sought, () => ({})),
        grep(Option.none()),
    );
    const chosen = yield* prepared(
        app[names[2]],
        Option.getOrElse(options, () => ({})),
        grep(Option.none()),
    );
    const [edits, styled] = Option.match(action, {
        onNone: () => [{}, Option.none()] as const,
        onSome: Match.valueTags({ Change: ({ preferences: fields }) => [fields, Option.none()] as const, Style: (styling) => [{}, Option.some(styling)] as const }),
    });
    const changed = yield* prepared(
        app[names[1]],
        edits,
        grep(
            Option.flatMap(
                Array.findFirst(found.rows, ([, field]) => field === 'findWhat'),
                ([, , , value]) => Option.liftPredicate(value, Predicate.isString),
            ),
        ),
    );
    const style = yield* Effect.transposeOption(
        Option.map(styled, ({ paragraphStyle, characterStyle }) =>
            Effect.map(
                Effect.filterOrFail(
                    Effect.map(resolve(paragraphStyle), (owners) => Array.filter(Array.ensure(owners), ({ nestedGrepStyles }) => Schema.is(Schema.Struct({ add: Callable }))(nestedGrepStyles))),
                    Array.isReadonlyArrayNonEmpty,
                    () => missing(`nestedGrepStyles of ${paragraphStyle}`, 'nestedGrepStyles', []),
                ),
                (owners) => [paragraphStyle, characterStyle, Array.map(owners, (each) => each.toSpecifier())],
            ),
        ),
    );
    const Found = Schema.Tuple([Schema.Unknown, Schema.Unknown, Schema.Int, Schema.Int, Schema.Array(Schema.Tuple([Schema.Unknown, Schema.Unknown, Schema.Unknown, Schema.NullOr(Schema.String), Schema.Unknown])), Schema.Array(Schema.Tuple([Schema.Unknown, Schema.Unknown, Schema.Unknown]))]);
    return {
        program: 'Find' as const,
        args: [names, search, methods, Array.map(targets, (each) => each.toSpecifier()), Option.getOrNull(query), [found.rows, changed.rows, chosen.rows], Option.exists(action, ({ _tag }) => _tag === 'Change'), Option.getOrNull(style), Option.getOrNull(save), Option.getOrNull(limit), reach],
        refusals: [...found.refusals, ...chosen.refusals, ...changed.refusals],
        notes: [...found.notes, ...chosen.notes, ...changed.notes],
        settle: (result: unknown) =>
            Effect.flatMap(Effect.orDie(Schema.decodeUnknownEffect(Found)(result)), ([searched, replaced, matched, changes, rows, paragraphs]) =>
                read(Find.successSchema.mapFields(Struct.omit(['preview', 'corrections'])), () => ({
                    find: unwired(searched),
                    change: replaced === null ? undefined : unwired(replaced),
                    count: { matches: matched, changes },
                    matches: Array.map(rows, ([hit, contents, context, page, paragraphStyle]) => ({ specifier: hit, text: Option.getOrUndefined(rendered(contents)), context: Option.getOrUndefined(rendered(context)), page: page ?? undefined, paragraphStyle: paragraphStyle ?? undefined })),
                    changes: Array.map(paragraphs, ([paragraph, before, after]) => ({ specifier: paragraph, before: Option.getOrUndefined(rendered(before)), after: Option.getOrUndefined(rendered(after)) })),
                })),
            ),
    };
});
const queries = Effect.fnUntraced(function* () {
    const { app, ScriptLanguage } = indesign;
    const listing = `(function (factory) {
    var rows = [], modes = Folder(factory ? app.filePath + '/Presets/Find-Change Queries' : app.scriptPreferences.scriptsFolder.parent.parent + '/Find-Change Queries').getFiles() || [];
    function walk(folder, mode) {
        var items = folder.getFiles() || [];
        for (var at = 0; at < items.length; at++) {
            if (items[at] instanceof Folder) walk(items[at], mode);
            else if (/\\.xml$/i.test(items[at].name)) rows.push([mode, items[at].displayName.replace(/\\.xml$/i, '')]);
        }
    }
    for (var at = 0; at < modes.length; at++) if (modes[at] instanceof Folder) walk(modes[at], modes[at].displayName);
    return rows;
})(arguments[0])`;
    const listed = yield* Effect.forEach(Queries.successSchema.value.fields.origin.literals, (origin) =>
        Effect.map(
            read(Schema.Array(Schema.Tuple([Schema.String, Schema.String])), () => app.doScript(listing, ScriptLanguage.JAVASCRIPT, [origin === 'factory'])),
            Array.map(([folder, name]) => ({ folder: folder.toLowerCase(), name, origin })),
        ),
    );
    const rows = Array.dedupeWith(Array.flatten(listed), (left, right) => left.folder === right.folder && left.name === right.name && left.origin === right.origin);
    const picked = Array.flatMap(Queries.successSchema.value.fields.mode.literals, (mode) =>
        Array.map(
            Array.filter(rows, ({ folder }) => folder === mode.toLowerCase()),
            ({ name, origin }) => ({ name, mode, origin }),
        ),
    );
    return {
        program: 'Queries' as const,
        args: [
            Array.map(picked, ({ name, mode }) => {
                const { names, search } = roles(mode);
                return [...names, search, name];
            }),
        ],
        refusals: [],
        notes: [],
        settle: (result: unknown) =>
            Effect.flatMap(Effect.orDie(Schema.decodeUnknownEffect(Schema.Array(Schema.Tuple([Schema.Unknown, Schema.Unknown, Schema.Unknown])))(result)), (loaded) =>
                read(Queries.successSchema, () => Array.zipWith(picked, loaded, (row, [sought, replaced, chosen]) => ({ ...row, find: unwired(sought), change: unwired(replaced), options: unwired(chosen) }))),
            ),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { find, grep, queries };
