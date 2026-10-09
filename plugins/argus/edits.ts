// biome-ignore lint/correctness/noNodejsModules: LSP file URIs require Node's native path escaping and platform path conversion.
import { pathToFileURL } from 'node:url';
import { Effect, type FileSystem, Match, Option, Schema, Stream } from 'effect';
import { FileChangeType, type FileEvent } from 'vscode-languageserver-protocol/node';

// --- [MODELS] --------------------------------------------------------------------------

const EditContext = Schema.Struct({
    sessionId: Schema.NonEmptyString,
    turnId: Schema.NullOr(Schema.String),
    transcriptPath: Schema.NullOr(Schema.String),
    toolUseId: Schema.NullOr(Schema.String),
    toolName: Schema.NullOr(Schema.String),
});

// --- [ERRORS] --------------------------------------------------------------------------

class TranscriptFailed extends Schema.TaggedError<TranscriptFailed>()('TranscriptFailed', { cause: Schema.Defect() }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const nativeEdits = Effect.fn('nativeEdits')(function* (fs: FileSystem.FileSystem, context: typeof EditContext.Type) {
    if (context.toolName !== 'apply_patch' || context.transcriptPath === null || context.turnId === null || context.toolUseId === null) {
        return [];
    }
    const turn = Schema.Literal(context.turnId);
    const call = Schema.Literal(context.toolUseId);
    const completed = Schema.Literal('completed');
    const event = Schema.Struct({
        type: Schema.Literal('event_msg'),
        payload: Schema.Union([
            Schema.Struct({ type: Schema.Literal('patch_apply_end'), turn_id: turn, call_id: call, status: completed, success: Schema.Literal(true), changes: Schema.Unknown }),
            Schema.Struct({ type: Schema.Literal('item_completed'), turn_id: turn, item: Schema.Struct({ type: Schema.Literal('FileChange'), id: call, status: completed, changes: Schema.Unknown }) }),
        ]),
    });
    const changes = Schema.Record(Schema.String, Schema.Union([Schema.Struct({ type: Schema.Literals(['add', 'delete']) }), Schema.Struct({ type: Schema.Literal('update'), move_path: Schema.NullOr(Schema.String) })]));
    const found = yield* fs.stream(context.transcriptPath).pipe(
        Stream.decodeText(),
        Stream.splitLines,
        Stream.mapEffect((line) => Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Unknown))(line)),
        Stream.map((line) => Schema.decodeUnknownOption(event)(line)),
        Stream.filter(Option.isSome),
        Stream.map(({ value: { payload } }) => (payload.type === 'patch_apply_end' ? payload.changes : payload.item.changes)),
        Stream.mapEffect((files) => Schema.decodeUnknownEffect(changes)(files)),
        Stream.runHead,
        Effect.mapError((cause) => new TranscriptFailed({ cause })),
    );
    return Option.match(found, {
        onNone: (): readonly FileEvent[] => [],
        onSome: (files): readonly FileEvent[] =>
            Object.entries(files).flatMap<FileEvent>(([path, change]) => {
                const uri = pathToFileURL(path).href;
                return Match.value(change).pipe(
                    Match.withReturnType<readonly FileEvent[]>(),
                    Match.when({ type: 'add' }, () => [{ uri, type: FileChangeType.Created }]),
                    Match.when({ type: 'delete' }, () => [{ uri, type: FileChangeType.Deleted }]),
                    Match.when({ type: 'update' }, ({ move_path }) =>
                        move_path === null
                            ? [{ uri, type: FileChangeType.Changed }]
                            : [
                                  { uri, type: FileChangeType.Deleted },
                                  { uri: pathToFileURL(move_path).href, type: FileChangeType.Created },
                              ],
                    ),
                    Match.exhaustive,
                );
            }),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { EditContext, nativeEdits, TranscriptFailed };
