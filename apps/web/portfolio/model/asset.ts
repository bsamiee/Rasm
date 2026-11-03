import { Array, ByteSize, Context, type Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const uploadLimit = ByteSize.megabytes(100);

// --- [MODELS] --------------------------------------------------------------------------

const AssetId = Schema.String.check(Schema.isUUID()).pipe(Schema.brand('AssetId'));
const extent = Schema.Finite.check(Schema.isGreaterThan(0));
const Dimensions = Schema.Struct({ width: extent, height: extent });
const Page = Schema.Struct({ ...Dimensions.fields, label: Schema.optionalKey(Schema.String) });
const Sheet = Schema.Struct({ ...Page.fields, number: Schema.Int.check(Schema.isGreaterThan(0)) });
const file = { id: AssetId, name: Schema.NonEmptyString, size: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: ByteSize.toNumberUnsafe(uploadLimit) })) };
const Rendition = Schema.Struct({ width: Schema.Int.check(Schema.isGreaterThan(0)), size: file.size, mime: Schema.Literals(['image/jpeg', 'image/png']) });
class ImageAsset extends Schema.Class<ImageAsset, { readonly brand: unique symbol }>('ImageAsset')({
    ...file,
    kind: Schema.tag('image'),
    mime: Schema.Literals(['image/jpeg', 'image/png', 'image/webp', 'image/avif']),
    ...Dimensions.fields,
    renditions: Schema.optionalKey(Schema.NonEmptyArray(Rendition).check(Schema.makeFilter((items) => new Set(items.map(({ width }) => width)).size === items.length, { expected: 'unique rendition widths' }))),
}) {
    static readonly Rendition = Rendition;
}
class VideoAsset extends Schema.Class<VideoAsset, { readonly brand: unique symbol }>('VideoAsset')({ ...file, kind: Schema.tag('video'), mime: Schema.Literals(['video/mp4', 'video/webm']), ...Dimensions.fields }) {}
class PdfAsset extends Schema.Class<PdfAsset, { readonly brand: unique symbol }>('PdfAsset')({ ...file, kind: Schema.tag('pdf'), mime: Schema.Literals(['application/pdf']), pages: Schema.NonEmptyArray(Page) }) {
    get sheets() {
        return Array.map(this.pages, (page, index) => ({ ...page, number: index + 1 }));
    }
}
const Asset = Schema.Union([ImageAsset, VideoAsset, PdfAsset]).pipe(Schema.toTaggedUnion('kind'));
const AssetJson = Schema.fromJsonString(Asset);
const mediaTypes = Asset.members.flatMap((member) => member.fields.mime.literals);

// --- [SERVICES] ------------------------------------------------------------------------

class Assets extends Context.Service<Assets, Record.ReadonlyRecord<typeof AssetId.Type, typeof Asset.Type>>()('@rasm/portfolio/model/asset/Assets') {
    static readonly schema = Schema.Record(AssetId, Asset);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Asset, AssetId, AssetJson, Assets, ImageAsset, mediaTypes, PdfAsset, Sheet, uploadLimit, VideoAsset };
