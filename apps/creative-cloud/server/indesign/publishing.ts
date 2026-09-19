// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, FileSystem, Option, Record, Schema, Struct } from 'effect';
import { HostRejection, WriteRejection } from '../errors.ts';
import { AbsolutePath, OptionalInt, OptionalPath } from '../values.ts';
import { enumerations, members } from './indesign.ts';

// --- [NATIVE VALUES] -------------------------------------------------------------------

const _names: Schema.$Array<Schema.String> = Schema.Array(Schema.String);
const _ids: Schema.$Array<Schema.Int> = Schema.Array(Schema.Int);
const _path: Schema.NonEmptyArray<Schema.String> = Schema.NonEmptyArray(Schema.String);
const _flag: Schema.Literals<Array<keyof typeof enumerations.PreflightRuleFlag>> = Schema.Literals(Struct.keys(enumerations.PreflightRuleFlag));
const _status: Schema.Literals<Array<keyof typeof enumerations.CommentStatusEnum>> = Schema.Literals(Struct.keys(enumerations.CommentStatusEnum));
const _empty = <S extends Schema.Constraint>(schema: S): Schema.withDecodingDefaultKey<Schema.$Array<S>> => Schema.Array(schema).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const _values: Schema.$Array<Schema.Struct<{ readonly key: Schema.String; readonly value: Schema.Codec<Schema.Json> }>> = Schema.Array(
    Schema.Struct({ key: Schema.String.pipe(Schema.check(Schema.makeFilter((key) => !Record.has<string, boolean>(members.Preference, key)))), value: Schema.Json }),
).pipe(Schema.check(Schema.makeFilter((values) => Array.dedupe(Array.map(values, Struct.get('key'))).length === values.length || 'A property can be assigned once in each policy')));
const _target: Schema.TaggedUnion<{
    readonly application: Schema.TaggedStruct<'application', Record<never, never>>;
    readonly document: Schema.TaggedStruct<'document', { readonly id: typeof OptionalInt }>;
}> = Schema.TaggedUnion({ application: {}, document: { id: OptionalInt } });
const _readback: Schema.$Record<Schema.String, Schema.Result<Schema.Codec<Schema.Json>, typeof HostRejection>> = Schema.Record(Schema.String, Schema.Result(Schema.Json, HostRejection));
const PublishingSections = {
    preflightOptions: 'PreflightOption',
    dictionaryPreferences: 'DictionaryPreference',
    pdfExportPreferences: 'PDFExportPreference',
    printPreferences: 'PrintPreference',
} as const;
const _policy: Schema.Struct<{ readonly section: Schema.Literals<Array<keyof typeof PublishingSections>>; readonly values: typeof _values }> = Schema.Struct({
    section: Schema.Literals(Struct.keys(PublishingSections)),
    values: _values,
}).pipe(Schema.check(Schema.makeFilter(({ section, values }) => Array.every(values, ({ key }) => Reflect.get(members[PublishingSections[section]], key) === true))));
const _rule: Schema.Struct<{ readonly id: Schema.String; readonly flag: typeof _flag; readonly data: Schema.$Record<Schema.String, Schema.Codec<Schema.Json>> }> = Schema.Struct({
    id: Schema.String,
    flag: _flag,
    data: Schema.Record(Schema.String, Schema.Json),
});
const _preset: Schema.Struct<{
    readonly kind: Schema.Literals<readonly ['pdf', 'printer']>;
    readonly source: Schema.String;
    readonly name: Schema.String;
    readonly values: typeof _values;
    readonly export: typeof OptionalPath;
}> = Schema.Struct({ kind: Schema.Literals(['pdf', 'printer']), source: Schema.String, name: Schema.String, values: _values, export: OptionalPath }).pipe(
    Schema.check(
        Schema.makeFilter((preset) => Array.every(preset.values, ({ key }) => key !== 'name' && Reflect.get(preset.kind === 'pdf' ? members.PDFExportPreset : members.PrinterPreset, key) === true)),
    ),
    Schema.check(Schema.makeFilter((preset) => preset.kind === 'pdf' || Option.isNone(preset.export) || 'Printer presets are exported as a native preset collection')),
);
const _query: Schema.Struct<{
    readonly name: Schema.String;
    readonly findWhat: Schema.String;
    readonly changeTo: Schema.String;
    readonly languageId: typeof OptionalInt;
    readonly options: typeof _values;
}> = Schema.Struct({
    name: Schema.String,
    findWhat: Schema.String,
    changeTo: Schema.String,
    languageId: OptionalInt,
    options: _values,
}).pipe(Schema.check(Schema.makeFilter(({ options }) => Array.every(options, ({ key }) => Reflect.get(members.FindChangeGrepOption, key) === true))));
const _wordLists: Schema.Literals<readonly ['addedWords', 'removedWords']> = Schema.Literals(['addedWords', 'removedWords']);
const _words: Schema.withDecodingDefaultKey<Schema.$Array<Schema.String>> = _empty(Schema.String.pipe(Schema.check(Schema.isPattern(/^\S+$/u))));
const _dictionary: Schema.Struct<{
    readonly languageId: Schema.Int;
    readonly addedWords: Schema.withDecodingDefaultKey<Schema.$Array<Schema.String>>;
    readonly removedWords: Schema.withDecodingDefaultKey<Schema.$Array<Schema.String>>;
    readonly exports: Schema.withDecodingDefaultKey<Schema.$Array<Schema.Struct<{ readonly list: typeof _wordLists; readonly path: typeof AbsolutePath }>>>;
}> = Schema.Struct({
    languageId: Schema.Int,
    addedWords: _words,
    removedWords: _words,
    exports: _empty(Schema.Struct({ list: _wordLists, path: AbsolutePath })),
}).pipe(
    Schema.check(
        Schema.makeFilter(({ addedWords, removedWords }) => Array.intersection(addedWords, removedWords).length === 0 || 'A word cannot be accepted and excluded in the same dictionary policy'),
    ),
);
const _profileRequest: Schema.Struct<{
    readonly id: Schema.Int;
    readonly name: Schema.String;
    readonly rules: Schema.$Array<typeof _rule>;
    readonly export: typeof OptionalPath;
    readonly embed: Schema.Boolean;
}> = Schema.Struct({
    id: Schema.Int,
    name: Schema.String,
    rules: Schema.Array(_rule).pipe(
        Schema.check(Schema.makeFilter((rules) => Array.dedupe(Array.map(rules, Struct.get('id'))).length === rules.length || 'A preflight rule can be configured once in each profile')),
    ),
    export: OptionalPath,
    embed: Schema.Boolean,
});
const _format: Schema.Literals<Array<keyof typeof enumerations.ExportPresetFormat>> = Schema.Literals(Struct.keys(enumerations.ExportPresetFormat));
const _archive: Schema.TaggedUnion<{
    readonly importProfile: Schema.TaggedStruct<'importProfile', { readonly path: typeof AbsolutePath }>;
    readonly exportProfile: Schema.TaggedStruct<'exportProfile', { readonly id: Schema.Int; readonly path: typeof AbsolutePath }>;
    readonly importPresets: Schema.TaggedStruct<'importPresets', { readonly format: typeof _format; readonly path: typeof AbsolutePath }>;
    readonly exportPresets: Schema.TaggedStruct<'exportPresets', { readonly format: typeof _format; readonly path: typeof AbsolutePath }>;
}> = Schema.TaggedUnion({
    importProfile: { path: AbsolutePath },
    exportProfile: { id: Schema.Int, path: AbsolutePath },
    importPresets: { format: _format, path: AbsolutePath },
    exportPresets: { format: _format, path: AbsolutePath },
});
const PublishingPreset: Schema.Struct<{ readonly kind: typeof _preset.fields.kind; readonly name: Schema.String; readonly file: typeof OptionalPath; readonly values: typeof _readback }> =
    Schema.Struct({
        kind: _preset.fields.kind,
        name: Schema.String,
        file: OptionalPath,
        values: _readback,
    });
const _state: Schema.Struct<{ readonly status: Schema.String; readonly applied: Schema.Boolean; readonly orphan: Schema.Boolean; readonly content: Schema.String }> = Schema.Struct({
    status: Schema.String,
    applied: Schema.Boolean,
    orphan: Schema.Boolean,
    content: Schema.String,
});

const PublishingProfile: Schema.Struct<{
    readonly id: Schema.Int;
    readonly name: Schema.String;
    readonly description: Schema.String;
    readonly rules: Schema.$Array<
        Schema.Struct<{
            readonly id: Schema.String;
            readonly name: Schema.String;
            readonly description: Schema.String;
            readonly flag: Schema.String;
            readonly fullySupported: Schema.Boolean;
            readonly data: Schema.$Array<Schema.Struct<{ readonly id: Schema.String; readonly name: Schema.String; readonly type: Schema.String; readonly value: Schema.Codec<Schema.Json> }>>;
        }>
    >;
}> = Schema.Struct({
    id: Schema.Int,
    name: Schema.String,
    description: Schema.String,
    rules: Schema.Array(
        Schema.Struct({
            id: Schema.String,
            name: Schema.String,
            description: Schema.String,
            flag: Schema.String,
            fullySupported: Schema.Boolean,
            data: Schema.Array(Schema.Struct({ id: Schema.String, name: Schema.String, type: Schema.String, value: Schema.Json })),
        }),
    ),
});

const PublishingComment: Schema.Struct<{
    readonly id: Schema.Int;
    readonly reviewer: Schema.String;
    readonly content: Schema.String;
    readonly date: Schema.String;
    readonly type: Schema.String;
    readonly source: Schema.String;
    readonly geometry: Schema.Result<Schema.Codec<Schema.Json>, typeof HostRejection>;
    readonly status: Schema.String;
    readonly orphan: Schema.Boolean;
    readonly applied: Schema.Boolean;
    readonly replies: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly reviewer: Schema.String; readonly content: Schema.String; readonly date: Schema.String }>>;
}> = Schema.Struct({
    id: Schema.Int,
    reviewer: Schema.String,
    date: Schema.String,
    type: Schema.String,
    source: Schema.String,
    geometry: Schema.Result(Schema.Json, HostRejection),
    ..._state.fields,
    replies: Schema.Array(Schema.Struct({ id: Schema.Int, reviewer: Schema.String, content: Schema.String, date: Schema.String })),
});

const PublishingError: Schema.TaggedUnion<{
    readonly selection: Schema.TaggedStruct<'selection', { readonly resource: Schema.String; readonly candidates: typeof _names }>;
    readonly conflict: Schema.TaggedStruct<'conflict', { readonly id: Schema.Int }>;
    readonly write: Schema.TaggedStruct<'write', { readonly path: typeof _path; readonly reason: typeof WriteRejection }>;
    readonly native: Schema.TaggedStruct<'native', { readonly reason: typeof HostRejection }>;
    readonly file: Schema.TaggedStruct<'file', { readonly path: typeof AbsolutePath; readonly reason: Schema.Defect }>;
}> = Schema.TaggedUnion({
    selection: { resource: Schema.String, candidates: _names },
    conflict: { id: Schema.Int },
    write: { path: _path, reason: WriteRejection },
    native: { reason: HostRejection },
    file: { path: AbsolutePath, reason: Schema.Defect() },
});
const _rejected: Schema.$Array<Schema.Struct<{ readonly target: Schema.String; readonly reason: typeof PublishingError }>> = Schema.Array(
    Schema.Struct({ target: Schema.String, reason: PublishingError }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const PublishingInput: Schema.toTaggedUnion<
    'operation',
    readonly [
        Schema.Struct<{ readonly operation: Schema.Literal<'inspect'>; readonly target: typeof _target }>,
        Schema.Struct<{
            readonly operation: Schema.Literal<'configure'>;
            readonly target: typeof _target;
            readonly preflight: Schema.OptionFromOptionalKey<typeof _profileRequest>;
            readonly policies: Schema.withDecodingDefaultKey<Schema.$Array<typeof _policy>>;
            readonly presets: Schema.withDecodingDefaultKey<Schema.$Array<typeof _preset>>;
            readonly queries: Schema.withDecodingDefaultKey<Schema.$Array<typeof _query>>;
            readonly dictionaries: Schema.withDecodingDefaultKey<Schema.$Array<typeof _dictionary>>;
        }>,
        Schema.Struct<{ readonly operation: Schema.Literal<'transfer'>; readonly target: typeof _target; readonly resources: Schema.NonEmptyArray<typeof _archive> }>,
        Schema.Struct<{ readonly operation: Schema.Literal<'preflight'>; readonly documentId: typeof OptionalInt; readonly profileId: typeof OptionalInt; readonly maximumSeconds: Schema.Number }>,
        Schema.Struct<{
            readonly operation: Schema.Literal<'review'>;
            readonly documentId: typeof OptionalInt;
            readonly source: Schema.OptionFromOptionalKey<typeof AbsolutePath>;
            readonly comments: Schema.withDecodingDefaultKey<Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly expected: typeof _state; readonly status: typeof _status }>>>;
        }>,
    ]
> = Schema.Union([
    Schema.Struct({ operation: Schema.Literal('inspect'), target: _target }),
    Schema.Struct({
        operation: Schema.Literal('configure'),
        target: _target,
        preflight: Schema.OptionFromOptionalKey(_profileRequest),
        policies: _empty(_policy),
        presets: _empty(_preset),
        queries: _empty(_query),
        dictionaries: _empty(_dictionary),
    }).pipe(
        Schema.check(
            Schema.makeFilter(
                ({ preflight, policies, presets, queries, dictionaries }) =>
                    Array.every(
                        [
                            Array.map(policies, Struct.get('section')),
                            Array.map(presets, Struct.pick(['kind', 'name'])),
                            Array.map(queries, Struct.get('name')),
                            Array.map(dictionaries, Struct.get('languageId')),
                            [
                                ...Array.flatMap(Option.toArray(preflight), (profile) => Option.toArray(profile.export)),
                                ...Array.flatMap(presets, (preset) => Option.toArray(preset.export)),
                                ...Array.flatMap(dictionaries, (dictionary) => Array.map(dictionary.exports, Struct.get('path'))),
                            ],
                        ],
                        (identities) => Array.dedupe(identities).length === identities.length,
                    ) || 'Each policy, preset, query, dictionary and export destination must identify one resource',
            ),
        ),
    ),
    Schema.Struct({ operation: Schema.Literal('transfer'), target: _target, resources: Schema.NonEmptyArray(_archive) }),
    Schema.Struct({
        operation: Schema.Literal('preflight'),
        documentId: OptionalInt,
        profileId: OptionalInt,
        maximumSeconds: Schema.Number.pipe(Schema.check(Schema.isFinite(), Schema.isGreaterThan(0))),
    }),
    Schema.Struct({
        operation: Schema.Literal('review'),
        documentId: OptionalInt,
        source: Schema.OptionFromOptionalKey(AbsolutePath),
        comments: _empty(Schema.Struct({ id: Schema.Int, expected: _state, status: _status })),
    }),
]).pipe(Schema.toTaggedUnion('operation'));

const _preflight: Schema.Tuple<
    readonly [
        Schema.String,
        Schema.String,
        Schema.$Array<Schema.Tuple<readonly [Schema.Int, Schema.String, Schema.String, Schema.String, Schema.$Array<Schema.Tuple<readonly [Schema.String, Schema.String]>>]>>,
    ]
> = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]))]);
const _coverage: Schema.Struct<{ readonly scope: Schema.String; readonly layers: Schema.String; readonly nonprinting: Schema.Boolean; readonly pasteboard: Schema.Boolean }> = Schema.Struct({
    scope: Schema.String,
    layers: Schema.String,
    nonprinting: Schema.Boolean,
    pasteboard: Schema.Boolean,
});
const _profiles: Schema.$Array<typeof PublishingProfile> = Schema.Array(PublishingProfile);
const _comments: Schema.$Array<typeof PublishingComment> = Schema.Array(PublishingComment);
const _presets: Schema.$Array<typeof PublishingPreset> = Schema.Array(PublishingPreset);
const _policies: Schema.$Record<Schema.Literals<Array<keyof typeof PublishingSections>>, Schema.optionalKey<typeof _readback>> = Schema.Record(
    Schema.Literals(Struct.keys(PublishingSections)),
    Schema.optionalKey(_readback),
);
const _files: Schema.$Array<typeof AbsolutePath> = Schema.Array(AbsolutePath);
const _dictionaryState: Schema.Struct<{ readonly languageId: Schema.Int; readonly name: Schema.String; readonly addedWords: typeof _names; readonly removedWords: typeof _names }> = Schema.Struct({
    languageId: Schema.Int,
    name: Schema.String,
    addedWords: _names,
    removedWords: _names,
});

const PublishingOutput: Schema.toTaggedUnion<
    'kind',
    readonly [
        Schema.Struct<{
            readonly kind: Schema.Literal<'inspection'>;
            readonly document: Schema.OptionFromNullOr<Schema.Struct<{ readonly id: Schema.Int; readonly name: Schema.String; readonly modified: Schema.Boolean }>>;
            readonly profiles: Schema.$Array<typeof PublishingProfile>;
            readonly presets: typeof _presets;
            readonly policies: typeof _policies;
            readonly rules: Schema.$Array<Schema.Struct<{ readonly id: Schema.String; readonly name: Schema.String; readonly description: Schema.String; readonly fullySupported: Schema.Boolean }>>;
            readonly languages: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly name: Schema.String; readonly locale: Schema.String }>>;
            readonly comments: Schema.$Array<typeof PublishingComment>;
        }>,
        Schema.Struct<{
            readonly kind: Schema.Literal<'configured'>;
            readonly profiles: Schema.$Array<typeof PublishingProfile>;
            readonly presets: typeof _presets;
            readonly policies: typeof _policies;
            readonly queries: Schema.$Array<typeof _query>;
            readonly dictionaries: Schema.$Array<typeof _dictionaryState>;
            readonly files: typeof _files;
            readonly rejected: typeof _rejected;
        }>,
        Schema.Struct<{
            readonly kind: Schema.Literal<'transferred'>;
            readonly profiles: typeof _profiles;
            readonly presets: typeof _presets;
            readonly files: typeof _files;
            readonly rejected: typeof _rejected;
        }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'completed'>; readonly profile: typeof PublishingProfile; readonly coverage: typeof _coverage; readonly results: typeof _preflight }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'incomplete'>; readonly profile: typeof PublishingProfile; readonly coverage: typeof _coverage }>,
        Schema.Struct<{
            readonly kind: Schema.Literal<'review'>;
            readonly imported: Schema.$Array<Schema.Int>;
            readonly comments: Schema.$Array<typeof PublishingComment>;
            readonly changed: Schema.$Array<Schema.Int>;
            readonly rejected: typeof _rejected;
        }>,
    ]
> = Schema.Union([
    Schema.Struct({
        kind: Schema.Literal('inspection'),
        document: Schema.OptionFromNullOr(Schema.Struct({ id: Schema.Int, name: Schema.String, modified: Schema.Boolean })),
        profiles: _profiles,
        presets: _presets,
        policies: _policies,
        rules: Schema.Array(Schema.Struct({ id: Schema.String, name: Schema.String, description: Schema.String, fullySupported: Schema.Boolean })),
        languages: Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, locale: Schema.String })),
        comments: _comments,
    }),
    Schema.Struct({
        kind: Schema.Literal('configured'),
        profiles: _profiles,
        presets: _presets,
        policies: _policies,
        queries: Schema.Array(_query),
        dictionaries: Schema.Array(_dictionaryState),
        files: _files,
        rejected: _rejected,
    }),
    Schema.Struct({ kind: Schema.Literal('transferred'), profiles: _profiles, presets: _presets, files: _files, rejected: _rejected }),
    Schema.Struct({ kind: Schema.Literal('completed'), profile: PublishingProfile, coverage: _coverage, results: _preflight }),
    Schema.Struct({ kind: Schema.Literal('incomplete'), profile: PublishingProfile, coverage: _coverage }),
    Schema.Struct({ kind: Schema.Literal('review'), imported: _ids, comments: _comments, changed: _ids, rejected: _rejected }),
]).pipe(Schema.toTaggedUnion('kind'));

// --- [DURABLE RESOURCES] ---------------------------------------------------------------

const persistPublishing: (input: typeof PublishingInput.Type, output: typeof PublishingOutput.Type) => Effect.Effect<typeof PublishingOutput.Type, never, FileSystem.FileSystem> = Effect.fnUntraced(
    function* (input, output) {
        if (output.kind !== 'configured' && output.kind !== 'transferred') {
            return output;
        }
        const fs = yield* FileSystem.FileSystem;
        const copies =
            input.operation === 'configure' && output.kind === 'configured'
                ? Array.flatMap(input.presets, (request) =>
                      Array.map(Option.toArray(request.export), (path) => ({
                          path,
                          write: Effect.flatMap(
                              Effect.fromOption(
                                  Option.flatMap(
                                      Array.findFirst(output.presets, (preset) => preset.kind === request.kind && preset.name === request.name),
                                      Struct.get('file'),
                                  ),
                                  () => PublishingError.cases.selection.make({ resource: request.name, candidates: Array.map(output.presets, Struct.get('name')) }),
                              ),
                              (source) => fs.copyFile(source, path),
                          ),
                      })),
                  )
                : [];
        const words =
            input.operation === 'configure' && output.kind === 'configured'
                ? Array.flatMap(input.dictionaries, (request) =>
                      Array.map(request.exports, ({ path, list }) => ({
                          path,
                          write: Effect.flatMap(
                              Effect.fromOption(
                                  Array.findFirst(output.dictionaries, (dictionary) => dictionary.languageId === request.languageId),
                                  () => PublishingError.cases.selection.make({ resource: String(request.languageId), candidates: Array.map(output.dictionaries, Struct.get('name')) }),
                              ),
                              (dictionary) => fs.writeFileString(path, Array.join(dictionary[list], '\n')),
                          ),
                      })),
                  )
                : [];
        const [rejected, files] = yield* Effect.partition([...Array.map(output.files, (path) => ({ path, write: Effect.void })), ...copies, ...words], ({ path, write }) =>
            write.pipe(
                Effect.andThen(fs.stat(path)),
                Effect.as(path),
                Effect.mapError((reason) => ({ target: path, reason: Schema.is(PublishingError)(reason) ? reason : PublishingError.cases.file.make({ path, reason }) })),
            ),
        );
        return { ...output, files, rejected: [...output.rejected, ...rejected] };
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { PublishingComment, PublishingError, PublishingInput, PublishingOutput, PublishingPreset, PublishingProfile, PublishingSections, persistPublishing };
