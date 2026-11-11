import { Effect, Option, Schema, SchemaGetter, String } from 'effect';
import { McpSchema } from 'effect/ai';
import { Server } from '#/server';

// --- [MODELS] --------------------------------------------------------------------------

const make = <S extends Schema.Top>(schema: S, description: string, block: (value: S['Type']) => readonly [McpSchema.ContentBlock, string]) =>
    Schema.String.annotate({ description }).pipe(
        Schema.decodeTo(Schema.toType(schema), {
            decode: SchemaGetter.forbidden(() => `${description} has no JSON decoding, its value sits in a content block`),
            encode: SchemaGetter.transformEffect((value: S['Type']) => {
                const [contentBlock, text] = block(value);
                return Server.ToolCall.use(({ add }) => Effect.as(add(contentBlock), text));
            }),
        }),
    );
const Content = Object.assign(make, {
    image: (mimeType: string) => make(Schema.Uint8Array, 'Media type of image content block', (data) => [McpSchema.ImageContent.make({ data, mimeType }), mimeType]),
    attachment: <S extends Schema.Schema<{ readonly uri: string; readonly mimeType: Option.Option<string>; readonly data: Uint8Array }>>(schema: S) =>
        make(schema, 'URI of file content block', ({ uri, mimeType, data }) => [
            Option.match(Option.filter(mimeType, String.startsWith('image/')), {
                onSome: (imageType) => McpSchema.ImageContent.make({ data, mimeType: imageType }),
                onNone: () => McpSchema.EmbeddedResource.make({ resource: McpSchema.BlobResourceContents.make({ uri, blob: data, mimeType: Option.getOrUndefined(mimeType) }) }),
            }),
            uri,
        ]),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Content };
