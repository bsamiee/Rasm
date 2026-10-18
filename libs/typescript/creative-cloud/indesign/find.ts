import indesign, { type PageItem, type Paragraph, type ParagraphStyle, type Text } from 'adobe:indesign';
import { Unmatched } from '@rasm/mcp/link';
import { Array, Effect, flow, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { FindChange, type Mode, type Prompt, Queries } from './contract.ts';
import { active, encoded, held, members, owning, read, resolve, stepped, write } from './dom.ts';
import { previewed } from './render.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PREFERENCES]
const preferences = Effect.fnUntraced(function* (mode: typeof Mode.Type) {
    const { app, NothingEnum, SearchModes } = indesign;
    const { owners, search, reset } = Match.value(mode).pipe(
        Match.when('Text', () => ({
            owners: [app.findTextPreferences, app.changeTextPreferences, app.findChangeTextOptions] as const,
            search: SearchModes.TEXT_SEARCH,
            reset: () => {
                app.findTextPreferences = NothingEnum.NOTHING;
                app.changeTextPreferences = NothingEnum.NOTHING;
                app.findChangeTextOptions = NothingEnum.NOTHING;
            },
        })),
        Match.when('Grep', () => ({
            owners: [app.findGrepPreferences, app.changeGrepPreferences, app.findChangeGrepOptions] as const,
            search: SearchModes.GREP_SEARCH,
            reset: () => {
                app.findGrepPreferences = NothingEnum.NOTHING;
                app.changeGrepPreferences = NothingEnum.NOTHING;
                app.findChangeGrepOptions = NothingEnum.NOTHING;
            },
        })),
        Match.when('Glyph', () => ({
            owners: [app.findGlyphPreferences, app.changeGlyphPreferences, app.findChangeGlyphOptions] as const,
            search: SearchModes.GLYPH_SEARCH,
            reset: () => {
                app.findGlyphPreferences = NothingEnum.NOTHING;
                app.changeGlyphPreferences = NothingEnum.NOTHING;
                app.findChangeGlyphOptions = NothingEnum.NOTHING;
            },
        })),
        Match.when('Object', () => ({
            owners: [app.findObjectPreferences, app.changeObjectPreferences, app.findChangeObjectOptions] as const,
            search: SearchModes.OBJECT_SEARCH,
            reset: () => {
                app.findObjectPreferences = NothingEnum.NOTHING;
                app.changeObjectPreferences = NothingEnum.NOTHING;
                app.findChangeObjectOptions = NothingEnum.NOTHING;
            },
        })),
        Match.exhaustive,
    );
    yield* Effect.forEach(owners, (owner) => held(owner, { properties: {} }), { discard: true });
    reset();
    const defaults: readonly object[] = Array.map(owners, (owner) => owner.properties);
    const keyed = (value: unknown): string | undefined => encoded(value, false);
    return {
        owners,
        search,
        differing: (at: 0 | 1 | 2): Readonly<Record<string, unknown>> => Object.fromEntries(Object.entries(owners[at].properties).filter(([field, value]) => keyed(value) !== keyed(Reflect.get(defaults[at] ?? {}, field)))),
    };
});

// --- [FIND]
const searched = Effect.fn('searched')(function* ({ mode, find: sought, action, options, scope, query, save, preview }: Rpc.Payload<typeof FindChange>, limit: number) {
    const { app } = indesign;
    const reach = 20;
    const [[, textual], [, placed], [, styled]] = [owning<Text>('parentTextFrames'), owning<PageItem>('itemLayer'), owning<ParagraphStyle>('nestedGrepStyles')];
    const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (each instanceof indesign.Enumerator ? `<${String(each)}>` : String(each))).join('');
    const changing = Option.exists(action, ({ _tag }) => _tag === 'Change');
    const {
        owners: [finding, changes, choosing],
        search,
        differing,
    } = yield* preferences(mode);
    yield* Effect.transposeOption(Option.map(query, (name) => Effect.sync(() => app.loadFindChangeQuery(name, search))));
    const replacing = Option.flatMap(action, (chosen) => (chosen._tag === 'Change' ? Option.some(chosen.preferences) : Option.none()));
    const written = yield* Effect.forEach(
        [
            [finding, sought],
            [choosing, options],
            [changes, replacing],
        ] as const,
        ([owner, fields]) =>
            write(
                owner,
                Option.getOrElse(fields, () => ({})),
            ),
    );
    const grepped = yield* Effect.forEach(
        Array.flatMap(Option.toArray(action), (chosen) => (chosen._tag === 'Style' ? [chosen] : [])),
        Effect.fnUntraced(function* ({ paragraphStyle, characterStyle }) {
            const expression = yield* Effect.fromOption(Schema.decodeUnknownOption(Schema.String)(Reflect.get(finding, 'findWhat')), () => Unmatched.make({ subject: `${mode} find pattern`, query: 'findWhat', candidates: [] }));
            const styles = Array.ensure<unknown>(yield* resolve(paragraphStyle)).filter(styled);
            const applied = yield* write(finding, { appliedParagraphStyle: paragraphStyle });
            const nested = yield* Effect.forEach(styles, (style) => write(style.nestedGrepStyles.add(), { grepExpression: expression, appliedCharacterStyle: characterStyle }));
            return [...applied, ...Array.flatten(nested)];
        }),
    );
    const targets = Array.ensure(yield* Option.match(scope, { onSome: resolve, onNone: () => Effect.flatMap(active, (document) => resolve(document.toSpecifier())) }));
    const called = (method: string): Effect.Effect<readonly unknown[], Unmatched> =>
        Effect.map(
            Effect.forEach(targets, (target) => {
                const call = target[method];
                return Predicate.isFunction(call) ? Effect.succeed(Array.ensure<unknown>(call.call(target))) : Effect.fail(Unmatched.make({ subject: `${mode} search target`, query: target.toSpecifier(), candidates: [] }));
            }),
            Array.flatten,
        );
    const hits = yield* called(`find${mode}`);
    const firsts: readonly Paragraph[] = Array.flatMap(hits, (hit) => (changing && textual(hit) ? [hit.paragraphs.firstItem()] : []));
    const before = Array.map(
        Array.dedupeWith(firsts, (left, right) => left.toSpecifier() === right.toSpecifier()),
        (paragraph) => ({ paragraph, specifier: paragraph.toSpecifier(), ordinal: Array.findFirstIndex(paragraph.parentStory.paragraphs.everyItem().index, (at) => at === paragraph.index), contents: rendered(paragraph.contents) }),
    );
    const matches = Array.map(Array.take(hits, limit), (hit) => {
        const frame = textual(hit) ? hit.parentTextFrames[0] : undefined;
        return textual(hit)
            ? {
                  specifier: hit.toSpecifier(),
                  text: rendered(hit.contents),
                  context: rendered(hit.parentStory.characters.itemByRange(Math.max(0, hit.index - reach), Math.min(hit.parentStory.characters.length - 1, hit.index + hit.length + reach - 1)).contents),
                  page: frame !== undefined && 'parentPage' in frame ? frame.parentPage?.name : undefined,
                  paragraphStyle: hit.appliedParagraphStyle,
              }
            : { specifier: hit, page: placed(hit) ? hit.parentPage?.name : undefined };
    });
    const changed = changing ? (yield* called(`change${mode}`)).length : 0;
    yield* Option.isSome(save) && Option.isNone(preview) ? Effect.sync(() => app.saveFindChangeQuery(save.value, search)) : Effect.void;
    const altered = Array.map(before, ({ paragraph, specifier, ordinal, contents }) => ({ specifier, before: contents, after: rendered(Option.isSome(ordinal) ? paragraph.parentStory.paragraphs.item(ordinal.value).contents : paragraph.contents) }));
    const found = yield* read(FindChange.successSchema.mapFields(Struct.omit(['preview', 'corrections'])), () => ({ find: differing(0), change: changing ? differing(1) : undefined, count: { matches: hits.length, changes: changed }, matches, changes: altered }));
    return { found, notes: [...Array.flatten(written), ...Array.flatten(grepped)] };
});
const find = (payload: Rpc.Payload<typeof FindChange>, limit: number) => Effect.map(previewed(payload, Effect.scoped(searched(payload, limit))), ({ result: { found, notes }, preview }) => ({ ...found, corrections: Option.liftPredicate(notes, Array.isArrayNonEmpty), preview }));

// --- [QUERIES]
const queries = Effect.fn('queries')(function* ({ name }: Rpc.Payload<typeof Queries>) {
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
    const { origin, mode } = Queries.successSchema.value.fields;
    const modes = Record.fromIterableWith(mode.literals, (each) => [each.toLowerCase(), each]);
    const listed = yield* Effect.forEach(origin.literals, (from) =>
        Effect.map(
            read(Schema.Array(Schema.Tuple([Schema.String, Schema.String])), () => app.doScript(listing, ScriptLanguage.JAVASCRIPT, [from === 'factory'])),
            (found) => [from, found] as const,
        ),
    );
    const rows: readonly { readonly name: string; readonly mode: typeof Mode.Type; readonly origin: (typeof origin.literals)[number] }[] = Array.dedupeWith(
        Array.flatMap(listed, ([from, found]) =>
            Array.flatMap(found, ([folder, query]) => {
                const kind = modes[folder.toLowerCase()];
                return kind === undefined ? [] : [{ name: query, mode: kind, origin: from }];
            }),
        ),
        (left, right) => left.name === right.name && left.mode === right.mode && left.origin === right.origin,
    );
    const chosen = Option.match(name, { onNone: () => rows, onSome: (query) => rows.filter((row) => row.name === query) });
    yield* Option.isSome(name) && Array.isReadonlyArrayEmpty(chosen) ? Effect.fail(Unmatched.make({ subject: 'saved query', query: name.value, candidates: Array.map(rows, Struct.get('name')) })) : Effect.void;
    return yield* stepped(
        'Reading saved queries',
        false,
        Effect.forEach(
            chosen,
            Effect.fnUntraced(function* (row) {
                const { search, differing } = yield* preferences(row.mode);
                app.loadFindChangeQuery(row.name, search);
                return yield* read(Queries.successSchema.value, () => ({ ...row, find: differing(0), change: differing(1), options: differing(2) }));
            }, Effect.scoped),
        ),
    );
});

// --- [DIALECT]
const dialect = read(Schema.Json, () => ({
    supported: {
        escapes: ['\\d and \\w over Unicode', '\\s \\h \\v \\R \\X \\b \\A \\G \\Z \\z', '\\Q…\\E', '\\K at the top level', '\\x41 \\x{2014} \\x{1F600}', '\\N{EM DASH}'],
        groups: ['a++', '(?>…)', '(?i) (?-i) (?i:…) (?s) (?m) (?x)', '(?|…)', '(?(1)…)', '(?<name>…) with \\k<name>', 'lookbehind whose alternatives share one length'],
        classes: ['[[=e=]]', '[[:alpha:]] [[:upper:]] [[:lower:]] [[:punct:]] [[:word:]]', '[[:unicode:]] above U+00FF'],
        properties: ['\\p{Lu} \\p{Ll} \\p{Lo} \\p{Nd} \\p{Zs} \\p{Sc} \\p{Sm} \\p{Mn} \\p{No} \\P{Lu}', '\\p{Letter} \\p{Number} \\p{Punctuation} \\p{Symbol} \\p{Mark} \\p{Lowercase_Letter} \\p{Uppercase_Letter}'],
        tilde: { '~b': 'paragraph return', '~a': 'U+FFFC anchored object marker', '~s': 'U+202F', '~S': 'U+00A0', '~_': 'U+2014', '~<': 'U+2009 thin space', '~#': 'any page number marker', '~N': 'current page number marker' },
    },
    traps: [
        { written: '\\p{L} \\p{S} \\p{P} \\p{N} \\p{M}', reads: 'lowercase, whitespace, nothing, nothing, nothing', write: '\\p{Letter} \\p{Symbol} \\p{Punctuation} \\p{Number} \\p{Mark}' },
        { written: '\\uXXXX', reads: 'an uppercase letter, then the digits', write: '\\x{XXXX}' },
        { written: '\\x{D83D}\\x{DE00}', reads: 'nothing', write: '\\x{1F600}' },
        { written: '[a-z\\p{Lu}]', reads: '\\p{Lu} as literal characters', write: '(?:[a-z]|\\p{Lu}), negated (?![a-z]|\\p{Lu})(?s:.)' },
        { written: '^a|bc, c$|x', reads: 'the anchor binds every alternative', write: '(?<![^\\r])a|bc, c(?![^\\r])|x' },
        { written: '(?<=\\$|USD)', reads: 'invalid', write: '(?:(?<=\\$)|(?<=USD)), negated (?<!\\$)(?<!USD)' },
        { written: '(?<=a+), (?<=a{1,2})', reads: 'invalid', write: 'a+\\K' },
        { written: '\\p{Han}, \\p{Script=Han}, \\p{IsArabic}', reads: 'nothing', write: 'a class of code points' },
        { written: '\\n', reads: 'the forced line break U+000A alone', write: '\\r ends a paragraph' },
        { written: '^ $', reads: 'paragraph start and end', write: '(?-m)^ for the story start' },
        { written: '.', reads: 'any character but \\r, special characters and markers included', write: '(?s) to cross paragraphs' },
        { written: 'a case-insensitive option', reads: 'absent from findChangeGrepOptions', write: '(?i)' },
    ],
    change: {
        groups: '$0 to $9, one digit each: $10 is $1 then 0',
        case: { '\\U': 'uppercases what follows', '\\u': 'capitalizes', '\\L': 'lowercases' },
        inserts: { '\\r': 'paragraph return', '~b': 'paragraph return', '\\t': 'tab', '\\n': 'forced line break', '~~': 'nonbreaking hyphen', '~8': 'bullet', '~_': 'em dash', '\\x{2014}': 'em dash', '\\$': '$', '\\\\': '\\' },
        literal: ['\\1', '$&', 'braced and named group references', '\\E', '$$'],
    },
    specialCharacters: Array.map(members(indesign.SpecialCharacters), String),
}));
const prompt = Effect.fn('prompt')(function* ({ task, scope }: Rpc.Payload<typeof Prompt>) {
    const { app } = indesign;
    const document = yield* Effect.option(active);
    const text = flow(Schema.encodeEffect(Schema.fromJsonString(Schema.Json)), Effect.orDie);
    const near = Option.match(document, { onNone: () => [], onSome: (open) => [open, ...app.selection, ...open.stories.everyItem().getElements()] });
    yield* Effect.mapError(resolve(scope), ({ query }) => Unmatched.make({ subject: 'search scope', query, candidates: Array.map(near, (each) => each.toSpecifier()) }));
    const [reference, current] = [yield* text(yield* dialect), yield* text(yield* read(Schema.Json, () => ({ findGrepPreferences: app.findGrepPreferences.properties, changeGrepPreferences: app.changeGrepPreferences.properties, findChangeGrepOptions: app.findChangeGrepOptions.properties })))];
    return [reference, current, `${task}\nScope: ${scope}\nCompose the pattern in InDesign's dialect, run find_change with preview, read the matches, then run find_change without preview to apply.`].join('\n\n');
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { dialect, find, prompt, queries };
