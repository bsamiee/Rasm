// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, Encoding, FileSystem, identity, Option, Result, Schema } from 'effect';
import { McpSchema } from 'effect/unstable/ai';
import sharp, { type OutputInfo, type Sharp } from 'sharp';
import { Spilled } from './jobs.ts';
import { Results } from './photoshop/jobs.ts';
import { AbsolutePath, OptionalInt } from './values.ts';

// --- [BOUNDARY] ------------------------------------------------------------------------

const _BASE64 = { maximumCharacters: 400_000, bytesPerBlock: 3, charactersPerBlock: 4 } as const;
const _Source = Schema.Union([
    Schema.Struct({
        kind: Schema.Literal('image'),
        path: AbsolutePath,
        contentWidthPx: OptionalInt,
        contentHeightPx: OptionalInt,
        isolated: Schema.OptionFromOptionalKey(Schema.Boolean),
    }),
    Results.fields.snapshot,
]);
const _image = Schema.decodeUnknownOption(_Source);
const _spilled = Schema.decodeUnknownOption(Spilled);

const MediaError: Schema.TaggedStruct<'imageInvalid', { readonly cause: Schema.Defect }> = Schema.TaggedStruct('imageInvalid', { cause: Schema.Defect() });

const _encoded = (pipeline: () => Sharp): Effect.Effect<{ readonly data: Uint8Array; readonly info: OutputInfo }, (typeof MediaError)['Type']> =>
    Effect.tryPromise({ try: () => pipeline().toUint8Array(), catch: (cause) => MediaError.make({ cause }) });

const _preview = Effect.fnUntraced(function* (image: (typeof _Source)['Type']) {
    const fs = yield* FileSystem.FileSystem;
    const source = yield* (image.kind === 'image' ? fs.readFile(image.path) : Effect.fromResult(Encoding.decodeBase64(image.base64))).pipe(
        Effect.mapError((cause: unknown) => MediaError.make({ cause })),
    );
    const budget = _BASE64.bytesPerBlock * Math.floor(_BASE64.maximumCharacters / _BASE64.charactersPerBlock);
    const pipeline = yield* Effect.try({ try: () => sharp(source), catch: (cause) => MediaError.make({ cause }) });
    const metadata = yield* Effect.tryPromise({ try: () => pipeline.metadata(), catch: (cause) => MediaError.make({ cause }) });
    const dimension = (actual: number, content: Option.Option<number>): number =>
        image.kind === 'jpeg' || Option.contains(image.isolated, true)
            ? actual
            : Math.min(
                  actual,
                  Option.getOrElse(content, () => actual),
              );
    const crop = {
        left: 0,
        top: 0,
        width: dimension(metadata.width, image.kind === 'image' ? image.contentWidthPx : Option.none()),
        height: dimension(metadata.height, image.kind === 'image' ? image.contentHeightPx : Option.none()),
    };
    const pixels = yield* _encoded(() => pipeline.extract(crop).raw());
    if (crop.width === metadata.width && crop.height === metadata.height && source.byteLength <= budget && (metadata.format === 'png' || metadata.format === 'jpeg')) {
        return { type: 'image' as const, data: source, mimeType: `image/${metadata.format}` };
    }
    const resize: (width: number, height: number) => ReturnType<typeof _encoded> = Effect.fnUntraced(function* (width: number, height: number) {
        const encoded = yield* _encoded(() => {
            const raster = sharp(pixels.data, { raw: pixels.info }).resize({ width, height, fit: 'fill', withoutEnlargement: true });
            return metadata.hasAlpha ? raster.png() : raster.jpeg({ mozjpeg: true });
        });
        if (encoded.data.byteLength <= budget) {
            return encoded;
        }
        const scale = Math.sqrt(budget / encoded.data.byteLength);
        const smaller = (size: number): number => Math.max(1, Math.min(size - 1, Math.floor(size * scale)));
        return yield* resize(smaller(width), smaller(height));
    });
    const fitted = yield* resize(crop.width, crop.height);
    return { type: 'image' as const, data: fitted.data, mimeType: `image/${fitted.info.format}` };
});

const rendered: (value: unknown, isError: boolean) => Effect.Effect<McpSchema.CallToolResult, Schema.SchemaError, FileSystem.FileSystem> = Effect.fnUntraced(function* (
    value: unknown,
    isError: boolean,
) {
    const structuredContent = yield* Schema.decodeUnknownEffect(Schema.Record(Schema.String, Schema.Json))(value);
    const result: Effect.Effect<Schema.Json | undefined, (typeof MediaError)['Type'] | Schema.SchemaError, FileSystem.FileSystem> = Option.match(_spilled(structuredContent['result']), {
        onNone: () => Effect.succeed(structuredContent['result']),
        onSome: ({ path }) =>
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const text = yield* Effect.mapError(fs.readFileString(path), (cause) => MediaError.make({ cause }));
                return yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Json))(text);
            }),
    });
    const content = yield* result.pipe(
        Effect.flatMap((resolved) => Option.match(_image(resolved), { onNone: () => Effect.succeed<readonly McpSchema.ImageContent[]>([]), onSome: (image) => Effect.map(_preview(image), Array.of) })),
        Effect.result,
    );
    return new McpSchema.CallToolResult({
        isError: Result.isFailure(content) || isError,
        structuredContent,
        content: [
            { type: 'text', text: JSON.stringify(structuredContent) },
            ...Result.match(content, {
                onFailure: (error) => [{ type: 'text' as const, text: Schema.encodeSync(Schema.fromJsonString(Schema.Defect()))(error) }],
                onSuccess: identity,
            }),
        ],
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { rendered };
