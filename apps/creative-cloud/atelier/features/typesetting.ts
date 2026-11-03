import { Automation } from '@rasm/automation/automation';
import { Reference } from '@rasm/automation/requests';
import { Kerns } from '@rasm/automation/typesetting';
import { Contract } from '@rasm/indesign/host';
import { progress } from '@rasm/mcp/progress';
import { registerRoutedTool, registerTool } from '@rasm/mcp/tool';
import { Unmatched } from '@rasm/search/unmatched';
import { Face } from '@rasm/typography/face';
import { Kerning } from '@rasm/typography/kerning';
import { Quran } from '@rasm/typography/quran';
import { Symbols } from '@rasm/typography/symbols';
import { Word } from '@rasm/typography/words';
import { Array, Cache, Effect, Equal, FileSystem, Number, Option, Order, Path, Predicate, Record, Ref, Result, Schema, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import type { Rpc, RpcGroup } from 'effect/rpc';
import type { routes } from '#/routes';

// --- [TYPES] ---------------------------------------------------------------------------

type Routed = Effect.Success<ReturnType<typeof routes>>;
type Client = Effect.Success<ReturnType<ReturnType<Routed['serving']>['resolve']>>['client'];
interface Range {
    readonly start: number;
    readonly end: number;
    readonly text: string;
    readonly font: string;
    readonly axes: readonly number[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FIRST = { offset: 0, limit: 1 };
const PAGE = 100;
const TEXTS = 'texts';

// --- [MODELS] --------------------------------------------------------------------------

const Position = Schema.Struct({ index: Schema.Int, parent: Reference });

// --- [OPERATIONS] ----------------------------------------------------------------------

const isText = <V extends { readonly _tag: string }>(view: V): view is Extract<V, { readonly _tag: 'Text' }> => view._tag === 'Text';
const inspected = Effect.fnUntraced(function* (client: Client, payload: { readonly target: string; readonly fields?: readonly string[]; readonly slice: typeof FIRST }) {
    const { payloadSchema } = yield* Effect.fromOption(Array.findFirst([...Contract.requests.values()], (rpc): rpc is Rpc.ExtractTag<RpcGroup.Rpcs<typeof Contract>, 'inspect'> => rpc._tag === 'inspect'));
    return yield* client('inspect', yield* Schema.decodeUnknownEffect(payloadSchema)(payload));
});
const object = Effect.fnUntraced(function* (client: Client, target: string, fields: readonly string[], slice = FIRST) {
    const found = yield* inspected(client, { target, fields, slice });
    return yield* found._tag === 'Object' ? Effect.succeed(found) : Unmatched.make({ subject: 'object', query: Option.some(target), candidates: [] });
});
const page = Effect.fnUntraced(function* (client: Client, target: string, slice = FIRST) {
    const { view } = yield* object(client, target, [TEXTS], slice);
    return yield* Effect.fromOption(Option.filter(view, isText), () => Unmatched.make({ subject: 'text', query: Option.some(target), candidates: [] }));
});
const pages = (client: Client, target: string) =>
    Stream.paginate(
        0,
        Effect.fnUntraced(function* (offset: number) {
            const { runs, ranges } = yield* page(client, target, { offset, limit: PAGE });
            return [runs, Option.liftPredicate(offset + PAGE, (next) => next < ranges)] as const;
        }),
    );
const sameFace = (left: Range, right: Range) => left.font === right.font && Equal.equals(left.axes, right.axes);
const text = Effect.fnUntraced(function* (client: Client, target: string) {
    const { view, collections } = yield* object(client, target, [TEXTS]);
    const owned = Effect.gen(function* () {
        const collection = yield* Effect.fromOption(
            Array.findFirst(collections, ({ name }) => name === TEXTS),
            () => Unmatched.make({ subject: 'text', query: Option.some(target), candidates: Array.map(collections, Struct.get('name')) }),
        );
        const members = yield* inspected(client, { target: collection.specifier, slice: FIRST });
        const member = yield* Effect.fromOption(members._tag === 'Items' ? Array.head(members.items) : Option.none(), () => Unmatched.make({ subject: 'text', query: Option.some(target), candidates: [] }));
        return yield* page(client, member.specifier);
    });
    return yield* Option.match(Option.filter(view, isText), { onNone: () => owned, onSome: Effect.succeed });
});
const story = Effect.fnUntraced(function* (client: Client, target: string) {
    const given = yield* text(client, target);
    const { fields } = yield* object(client, given.specifier, ['index', 'parent']);
    const { index, parent } = yield* Schema.decodeUnknownEffect(Position)(Record.fromIterableWith(fields, ({ name, value }) => [name, Option.getOrNull(Result.getOrElse(value, Option.none))]));
    const owner = yield* text(client, parent.specifier);
    const runs = yield* Stream.runCollect(pages(client, owner.specifier));
    const [, ranges] = Array.mapAccum(runs, 0, (start, { contents, length, appliedFont, designAxes }) => {
        const special = '\n';
        const content = Predicate.isString(contents) ? contents : special.repeat(length);
        return [start + content.length, { start, end: start + content.length, text: content, font: Option.getOrElse(appliedFont.name, () => appliedFont.specifier), axes: Result.getOrElse(designAxes, Array.empty) }] as const;
    });
    const [first, last] = Word.expand(ranges.map(Struct.get('text')).join(''), index, index + given.length);
    const joined = Array.match(ranges, { onEmpty: Array.empty<Array.NonEmptyReadonlyArray<Range>>, onNonEmpty: Array.groupWith(sameFace) }).map((group): Range => ({ ...Array.headNonEmpty(group), end: Array.lastNonEmpty(group).end, text: group.map(Struct.get('text')).join('') }));
    const spans = joined
        .filter(({ start, end }) => start < last && end > first)
        .map((range): Range => {
            const [start, end] = [Math.max(range.start, first), Math.min(range.end, last)];
            return { ...range, start, end, text: range.text.slice(start - range.start, end - range.start) };
        });
    return { owner: owner.specifier, spans };
});
const kerner = Effect.fnUntraced(function* (client: Client, rules: typeof Kerning.Rules.Type, { font, axes }: Range, group: readonly Range[]) {
    const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const characters = group.map(Struct.get('text')).join('');
    const { source } = yield* client('font', { name: font, text: characters, file: path.join(yield* files.makeTempDirectoryScoped(), 'subset.otf') });
    const { file, postscriptName } = yield* Effect.fromOption(source, () => Unmatched.make({ subject: 'font file', query: Option.some(font), candidates: [] }));
    const opened = { data: yield* files.readFile(file), postscriptName };
    const named = yield* Face.open(opened, { variations: [], size: Option.none() });
    const instance = yield* Schema.decodeEffect(Face.Instance)({ variations: Array.zipWith(named.axes, axes, ({ tag }, value) => `${tag}=${value}`) });
    const pair = Kerning.pair(Array.isReadonlyArrayEmpty(axes) ? named : yield* Face.open(opened, instance), rules);
    const pairs = yield* Cache.make({ capacity: characters.length, lookup: ({ before, after }: { readonly before: string; readonly after: string }) => Effect.sync(() => pair(before, after)) });
    return group.map((range) => ({ range, pair: (before: string, after: string) => Cache.get(pairs, { before, after }) }));
}, Effect.scoped);
const kerned = Effect.fnUntraced(function* (client: Client, target: string, rules: typeof Kerning.Rules.Type) {
    const { owner, spans } = yield* story(client, target);
    const kerners = yield* Effect.forEach(Array.dedupeWith(spans, sameFace), (face) =>
        kerner(
            client,
            rules,
            face,
            spans.filter((span) => sameFace(face, span)),
        ),
    );
    const ordered = Array.sort(
        kerners.flat(),
        Order.mapInput(Number.Order, ({ range }: (typeof kerners)[number][number]) => range.start),
    );
    const rows = yield* Ref.make<readonly { readonly index: number; readonly em: number }[]>([]);
    yield* progress(
        Stream.fromIterable(ordered).pipe(
            Stream.zipWithIndex,
            Stream.mapEffect(
                Effect.fnUntraced(function* ([{ range, pair }, at]) {
                    const found = yield* Kerning.of(range.text, pair);
                    yield* Ref.update(rows, Array.appendAll(found.map(({ index, em }) => ({ index: range.start + index, em }))));
                    return { progress: at + 1, total: ordered.length, message: range.font };
                }),
            ),
        ),
    );
    return yield* client('kerns', { target: owner, kerns: yield* Ref.get(rows) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = Effect.fnUntraced(function* ({ serving }: Routed) {
    const [automation, path, files] = yield* Effect.all([Automation, Path.Path, FileSystem.FileSystem]);
    yield* registerRoutedTool(serving('kerns'))(
        Tool.make('spacing', {
            description: 'Computes pair kerning from glyph outlines across text, story or frame and writes it in one undo step, widening text to whole words and keeping manual kerning',
            parameters: Schema.Struct({
                target: Kerns.fields.target,
                rules: Kerning.Rules.annotate({ description: 'Gap rules by ICU joining group, first match wins, none when absent' }).pipe(Schema.withDecodingDefaultKey(Schema.encodeEffect(Kerning.Rules)(Kerning.Rules.make({})))),
            }),
            success: Kerns.Result,
        }),
        ({ target, rules }, { client }) => kerned(client, target, rules),
    );
    yield* registerTool(
        Tool.make('glossary', {
            description: 'Reads surah text from CSV files, change inserts it',
            parameters: Schema.Struct({
                ayahs: Schema.String.annotate({ description: 'Ayah CSV path' }),
                surahs: Schema.String.annotate({ description: 'Surah CSV path' }),
                columns: Quran.Columns,
                surah: Schema.String.annotate({ description: 'Surah name or number' }),
                ayah: Schema.Union([Schema.Int, Schema.Literal('all')]).annotate({ description: 'Ayah number, all for every ayah' }),
                separator: Schema.String.annotate({ description: 'Text after each ayah' }),
            }),
            success: Schema.Struct({ text: Schema.String }),
        })
            .annotate(Tool.Readonly, true)
            .annotate(Tool.OpenWorld, false),
        Effect.fnUntraced(function* ({ ayahs, surahs, columns, surah, ayah, separator }) {
            const quran = yield* Quran.decode(yield* Effect.all({ ayahs: files.readFileString(ayahs), surahs: files.readFileString(surahs) }), columns);
            return { text: yield* Quran.text(yield* quran.surah(surah), ayah, separator) };
        }),
    );
    yield* registerTool(
        Tool.make('symbols', {
            description: 'Lists symbols of TSV file that named CoreText face covers, by set',
            parameters: Schema.Struct({
                file: Schema.String.annotate({ description: 'Symbol TSV path, one column of hexadecimal code points per set' }),
                face: Schema.String.annotate({ description: 'CoreText face name' }),
            }),
            success: Schema.Struct({ sets: Schema.Record(Schema.String, Schema.Array(Schema.String)) }),
        })
            .annotate(Tool.Readonly, true)
            .annotate(Tool.OpenWorld, false),
        Effect.fnUntraced(function* ({ file, face: name }) {
            const [symbols, found] = yield* Effect.all([Effect.flatMap(files.readFileString(file), Symbols.decode), automation.face({ name })]);
            const face = yield* Face.open({ data: yield* files.readFile(yield* path.fromFileUrl(found.file)), postscriptName: Option.some(found.postscriptName) }, { variations: [], size: Option.none() });
            return {
                sets: Record.map(
                    symbols.supported(face).sets,
                    Array.map((point) => String.fromCodePoint(point)),
                ),
            };
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
