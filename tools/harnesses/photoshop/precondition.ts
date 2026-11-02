import { Schema, SchemaGetter, Struct } from 'effect';
import { Created, Document, Enumerated, Placed, type Survey } from '#/photoshop/contract';

// --- [OPERATIONS] ----------------------------------------------------------------------

const many = <S extends Schema.Top>(item: S) => Schema.Union([Schema.Array(item), item.pipe(Schema.decodeTo(Schema.Array(Schema.toType(item)), { decode: SchemaGetter.transform((one) => [one] as const), encode: SchemaGetter.forbiddenEncoding }))]);
const member = (enumeration: string) =>
    Schema.NonEmptyString.pipe(Schema.decodeTo(Enumerated, { decode: SchemaGetter.transform((name) => ({ $: `enum:${enumeration}.${name}` as const })), encode: SchemaGetter.forbiddenEncoding })).annotate({ description: `${enumeration} member name, one the host lacks failing the case` });
const Made = Schema.Struct({ ...Created.fields, mode: member('NewDocumentMode'), initialFill: member('DocumentFill'), bitsPerChannel: member('BitsPerChannelType') }).pipe(
    Schema.decodeTo(Schema.toType(Document.cases.Made), { decode: SchemaGetter.transform((held) => Document.cases.Made.make({ arguments: held })), encode: SchemaGetter.forbiddenEncoding }),
);
const Opened = Schema.Struct({ open: Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' }) }).pipe(
    Schema.decodeTo(Schema.toType(Document.cases.Opened), { decode: SchemaGetter.transform(({ open }) => Document.cases.Opened.make({ open })), encode: SchemaGetter.forbiddenEncoding }),
);
const precondition = (survey: typeof Survey.Type) =>
    Schema.Struct({
        ...Struct.omit(Placed.fields, ['documents', 'units']),
        documents: Schema.optionalKey(many(Schema.Union([Made, Opened])).annotate({ description: 'app.documents.add arguments or an open path, one or many, none for an application case' })),
        units: Schema.Struct({ ruler: member(survey.units.ruler), type: member(survey.units.type) }),
    }).pipe(Schema.decodeTo(Schema.toType(Placed), { decode: SchemaGetter.transform(({ documents = [], ...rest }) => ({ ...rest, documents })), encode: SchemaGetter.forbiddenEncoding }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
