import { Effect, Option, Schema, SchemaGetter } from 'effect';
import { McpSchema } from 'effect/ai';
import { ContentBlocks } from './server.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Content = <S extends Schema.Top>(schema: S, description: string, block: (value: S['Type']) => readonly [McpSchema.ContentBlock, string]) =>
    schema.annotate({
        toCodecJson: () =>
            Schema.link<S['Type']>()(Schema.String.annotate({ description }), {
                decode: SchemaGetter.forbidden(() => 'Content blocks hold the value'),
                encode: SchemaGetter.transformEffect((value: S['Type']) => {
                    const [added, shown] = block(value);
                    return ContentBlocks.use((add) => Effect.as(add(added), shown));
                }),
            }),
    });
const Image = (mimeType: string) => Content(Schema.Uint8Array, 'Media type of the image content block beside the result', (data) => [McpSchema.ImageContent.make({ data, mimeType }), mimeType]);
const Attachment = Content(Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8Array }), 'URI of the file in the content block beside the result', ({ uri, mimeType, data }) => [
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
