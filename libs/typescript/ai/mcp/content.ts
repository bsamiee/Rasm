import { Effect, Option, Schema, SchemaGetter, String } from 'effect';
import { McpSchema } from 'effect/ai';
import { Server } from '#/server';

// --- [MODELS] --------------------------------------------------------------------------

const make = <S extends Schema.Top>(schema: S, description: string, blocks: (value: S['Type']) => readonly [readonly McpSchema.ContentBlock[], string]) =>
    Schema.String.annotate({ description }).pipe(
        Schema.decodeTo(Schema.toType(schema), {
            decode: SchemaGetter.forbidden(() => `${description} has no JSON decoding, its value sits in content blocks`),
            encode: SchemaGetter.transformEffect((value: S['Type']) => {
                const [added, text] = blocks(value);
                return Server.ToolCall.use(({ add }) => Effect.as(Effect.forEach(added, add, { discard: true }), text));
            }),
        }),
    );
const Content = Object.assign(make, {
    image: (mimeType: string) => make(Schema.Uint8Array, 'Media type of image content block', (data) => [[McpSchema.ImageContent.make({ data, mimeType })], mimeType]),
    embeddedResource: <S extends Schema.Schema<{ readonly uri: string; readonly mimeType: Option.Option<string>; readonly data: Uint8Array }>>(schema: S) =>
        make(schema, 'URI of file content block', ({ uri, mimeType, data }) => [
            [
                Option.match(Option.filter(mimeType, String.startsWith('image/')), {
                    onSome: (imageType) => McpSchema.ImageContent.make({ data, mimeType: imageType }),
                    onNone: () => McpSchema.EmbeddedResource.make({ resource: McpSchema.BlobResourceContents.make({ uri, blob: data, mimeType: Option.getOrUndefined(mimeType) }) }),
                }),
            ],
            uri,
        ]),
    preview: make(Schema.Struct({ data: Schema.Uint8Array, mimeType: Schema.String, file: Schema.URL, width: Schema.Int, height: Schema.Int }), 'Full-resolution file the image content block previews', ({ data, mimeType, file, width, height }) => [
        [McpSchema.ImageContent.make({ data, mimeType }), McpSchema.ResourceLink.make({ uri: file.href, name: decodeURIComponent(file.pathname.slice(file.pathname.lastIndexOf('/') + 1)), mimeType, description: `${width}x${height} pixels, the image block a preview of it` })],
        file.href,
    ]),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Content };
