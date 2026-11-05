import { Array, Effect, FileSystem, Option, Order, Path, Record, Schema, Stream, String, Struct } from 'effect';
import { Digest } from 'effect/crypto';
import { Ndjson } from 'effect/encoding';
import { ChildProcess, CommandLine } from 'effect/process';
import { SymbolKind } from 'vscode-languageserver-types';

// --- [TYPES] ---------------------------------------------------------------------------

interface Span {
    readonly start: number;
    readonly end: number;
}
interface Row {
    readonly path: string;
    readonly language: string;
    readonly symbolType: Uncapitalize<keyof typeof SymbolKind>;
    readonly owner: Option.Option<string>;
    readonly name: string;
    readonly signature: string;
    readonly astKind: string;
    readonly import: boolean;
    readonly public: boolean;
    readonly line: number;
    readonly span: Span;
    readonly body: string;
    readonly shape: string;
    readonly size: number;
    readonly members: readonly Row[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FLAGS = ['--custom-languages', 'allow', '--no-ignore', 'vcs', '--items', 'all', '--view', 'expanded', '--json=stream', '-j', '1'] as const;
const IMPL = 'impl_item';
const MASK = '\0';
const SEPARATOR = /\.|::/u;
const WHITESPACE = /\s+/gu;
const SPAN_MINIMUM = 60;
const MEMBER_MINIMUM = 4;
const ENCODER = new TextEncoder();
const DECODER = new TextDecoder();

// --- [MODELS] --------------------------------------------------------------------------

const Declaration = {
    symbolType: Schema.Literals(Record.keys(SymbolKind).map(String.uncapitalize)),
    name: Schema.String,
    signature: Schema.String,
    astKind: Schema.String,
    range: Schema.Struct({ byteOffset: Schema.Struct({ start: Schema.Int, end: Schema.Int }), start: Schema.Struct({ line: Schema.Int }) }),
};
const Member = Schema.Struct({ ...Declaration, isPublic: Schema.Boolean });
const Item = Schema.Struct({ ...Declaration, isImport: Schema.Boolean, isExported: Schema.Boolean, members: Schema.optionalKey(Schema.Array(Member)) });
const FileRecord = Schema.Struct({ path: Schema.String, language: Schema.String, items: Schema.Array(Item) });

// --- [ERRORS] --------------------------------------------------------------------------

class OutlineError extends Schema.TaggedError<OutlineError, { readonly brand: unique symbol }>()('OutlineError', { argv: Schema.NonEmptyArray(Schema.String), code: Schema.Int, stderr: Schema.String }) {
    override get message() {
        return [`${CommandLine.display(this.argv)} exited with code ${this.code}`, String.trim(this.stderr)].filter(String.isNonEmpty).join('\n');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SPANS]
const fold = (text: string) => String.trim(text.replaceAll(WHITESPACE, ' '));
const mask = (text: string, name: string) => {
    const token = Array.lastNonEmpty(String.split(name, SEPARATOR));
    return String.isNonEmpty(token) ? text.replace(new RegExp(`(?<![\\p{ID_Continue}$])${RegExp.escape(token)}(?![\\p{ID_Continue}$])`, 'u'), MASK) : text;
};
const within = (name: string, owner: string) => name.startsWith(owner) && name.slice(owner.length).search(SEPARATOR) === 0;
const encloses = (outer: Span, inner: Span) => outer.start <= inner.start && inner.end <= outer.end && (outer.start !== inner.start || outer.end !== inner.end);
const gaps = (span: Span, spans: readonly Span[]): readonly Span[] => {
    const [cursor, open] = Array.mapAccum(
        Array.sort(
            spans.filter((inner) => encloses(span, inner)),
            Order.mapInput(Order.Number, (hole: Span) => hole.start),
        ),
        span.start,
        (at, hole) => [Math.max(at, hole.end), { start: at, end: Math.max(at, hole.start) }],
    );
    return [...open, { start: cursor, end: span.end }];
};
const bounds = (members: readonly (typeof Member.Type)[], end: number) =>
    members.map((member, index) =>
        Math.max(
            member.range.byteOffset.end,
            Option.match(
                Array.findFirst(members.slice(index + 1), (later) => !within(later.name, member.name)),
                { onNone: () => end, onSome: (later) => later.range.byteOffset.start },
            ),
        ),
    );
const derive = Effect.fnUntraced(function* (record: typeof FileRecord.Type, bytes: Uint8Array) {
    const digest = yield* Digest.Digest;
    const slice = ({ start, end }: Span) => DECODER.decode(bytes.subarray(start, end));
    const tight = (start: number, end: number): Span => ({ start, end: start + ENCODER.encode(String.trimEnd(slice({ start, end }))).length });
    const placed = Array.dedupeWith(record.items, (left, right) => left.name === right.name && left.astKind === right.astKind && left.range.byteOffset.start === right.range.byteOffset.start && left.range.byteOffset.end === right.range.byteOffset.end).map((item) => {
        const members = item.members ?? [];
        return { item, span: tight(item.range.byteOffset.start, item.range.byteOffset.end), members: Array.zipWith(members, bounds(members, item.range.byteOffset.end), (member, end) => ({ member, span: tight(member.range.byteOffset.start, end) })) };
    });
    const spans = placed.flatMap(({ span, members }) => [span, ...members.map(Struct.get('span'))]);
    const row = Effect.fnUntraced(function* (declaration: typeof Member.Type | typeof Item.Type, span: Span, place: Pick<Row, 'owner' | 'import' | 'public' | 'members'>) {
        const shaped = fold(mask(slice(span), declaration.name));
        const [body, shape] = yield* Effect.all([digest.digestHex(Digest.sha1, ENCODER.encode(gaps(span, spans).map(slice).join(MASK))), digest.digestHex(Digest.sha1, ENCODER.encode(shaped))]);
        return { path: record.path, language: record.language, symbolType: declaration.symbolType, name: declaration.name, signature: fold(declaration.signature), astKind: declaration.astKind, line: declaration.range.start.line + 1, span, body, shape, size: shaped.length, ...place } satisfies Row;
    });
    return yield* Effect.forEach(placed, ({ item, span, members }) =>
        Effect.flatMap(
            Effect.forEach(members, ({ member, span: extent }) => row(member, extent, { owner: Option.some(item.name), import: false, public: member.isPublic && (item.isExported || item.astKind === IMPL), members: [] })),
            (owned) => row(item, span, { owner: Option.none(), import: item.isImport, public: item.isExported, members: owned }),
        ),
    );
});

// --- [READING]
const read = Effect.fn('read')(function* (paths: Array.NonEmptyReadonlyArray<string>, { cwd = '.' }: { readonly cwd?: string } = {}) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const argv: Array.NonEmptyReadonlyArray<string> = ['ast-grep', 'outline', ...paths, ...FLAGS];
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { cwd, stdin: 'ignore' });
    const [records, stderr, code] = yield* Effect.all([Stream.runCollect(Stream.pipeThroughChannel(handle.stdout, Ndjson.decodeSchema(FileRecord)())), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? (yield* Effect.forEach(records, (record) => Effect.flatMap(fs.readFile(path.resolve(cwd, record.path)), (bytes) => derive(record, bytes)))).flat() : yield* OutlineError.make({ argv, code, stderr });
}, Effect.scoped);

// --- [GROUPING]
const equal = (rows: readonly Row[]): readonly Array.NonEmptyReadonlyArray<Row>[] => {
    const candidates = Object.values(Array.groupBy(rows, Struct.get('path'))).flatMap((items) =>
        items.some((item) => [item, ...item.members].some((row) => row.symbolType === 'null')) ? [] : items.filter((row) => row.symbolType !== 'module' && !row.import && row.astKind !== IMPL && !items.some((other) => encloses(other.span, row.span))),
    );
    const groups = (included: (row: Row) => boolean, key: (row: Row) => string) => Object.values(Array.groupBy(candidates.filter(included), key)).filter((group) => group.length > 1);
    return Array.dedupeWith(
        [
            ...groups(
                (row) => row.size >= SPAN_MINIMUM,
                (row) => JSON.stringify([row.language, row.symbolType, row.shape]),
            ),
            ...groups(
                (row) => row.members.length >= MEMBER_MINIMUM,
                (row) =>
                    JSON.stringify(
                        Array.sort(
                            row.members.map((member) => JSON.stringify([member.name, member.shape])),
                            Order.String,
                        ),
                    ),
            ),
        ],
        (left, right) => left.length === right.length && left.every((row) => right.includes(row)),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { equal, OutlineError, type Row, read };
