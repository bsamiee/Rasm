import { Effect, Option, Schema, SchemaGetter } from 'effect';
import { McpSchema } from 'effect/ai';
import { ToolCall } from '#/server';

// --- [MODELS] --------------------------------------------------------------------------

const Content = <S extends Schema.Top>(schema: S, description: string, block: (value: S['Type']) => readonly [McpSchema.ContentBlock, string]) =>
    schema.annotate({
        toCodecJson: () =>
            Schema.link<S['Type']>()(Schema.String.annotate({ description }), {
                decode: SchemaGetter.forbidden(() => 'Content blocks hold value'),
                encode: SchemaGetter.transformEffect((value: S['Type']) => {
                    const [added, shown] = block(value);
                    return ToolCall.use(({ add }) => Effect.as(add(added), shown));
                }),
            }),
    });
const Image = (mimeType: string) => Content(Schema.Uint8Array, 'Media type of image content block', (data) => [McpSchema.ImageContent.make({ data, mimeType }), mimeType]);
const Attachment = <S extends Schema.Top & { readonly Type: { readonly uri: string; readonly mimeType: Option.Option<string>; readonly data: Uint8Array } }>(schema: S) =>
    Content(schema, 'URI of file content block', ({ uri, mimeType, data }) => [
        Option.match(
            Option.filter(mimeType, (type) => type.startsWith('image/')),
            {
                onSome: (image) => McpSchema.ImageContent.make({ data, mimeType: image }),
                onNone: () => McpSchema.EmbeddedResource.make({ resource: McpSchema.BlobResourceContents.make({ uri, blob: data, mimeType: Option.getOrUndefined(mimeType) }) }),
            },
        ),
        uri,
    ]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Attachment, Content, Image };
