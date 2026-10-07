import { Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const uploadLimit = 100_000_000;

// --- [MODELS] --------------------------------------------------------------------------

const Id = Schema.String.check(Schema.isUUID());
const extent = Schema.Finite.check(Schema.isGreaterThan(0));
const Dimensions = Schema.Struct({ width: extent, height: extent });
const Page = Schema.Struct({ ...Dimensions.fields, label: Schema.optionalKey(Schema.String) });
const file = { id: Id, name: Schema.NonEmptyString, size: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: uploadLimit })) };
const ImageAsset = Schema.Struct({ ...file, mime: Schema.Literals(['image/jpeg', 'image/png', 'image/webp', 'image/avif']), ...Dimensions.fields });
const VideoAsset = Schema.Struct({ ...file, mime: Schema.Literals(['video/mp4', 'video/webm']), ...Dimensions.fields });
const PdfAsset = Schema.Struct({ ...file, mime: Schema.Literals(['application/pdf']), pages: Schema.NonEmptyArray(Page) });
const Asset = Schema.Union([ImageAsset, VideoAsset, PdfAsset]);
const AssetCollection = Schema.Record(Schema.String, Asset);
const mediaTypes = Asset.members.flatMap((member) => member.fields.mime.literals);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Asset, AssetCollection, Dimensions, Id, ImageAsset, mediaTypes, Page, PdfAsset, uploadLimit, VideoAsset };
