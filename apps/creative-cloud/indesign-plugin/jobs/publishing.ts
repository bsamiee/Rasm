// --- [IMPORTS] -------------------------------------------------------------------------

import {
    app,
    BuildingBlockTypes,
    ChangeCaseOptions,
    CommentStatusEnum,
    ConditionIndicatorMethod,
    ConditionIndicatorMode,
    EndnoteFrameCreate,
    EndnoteRestarting,
    EndnoteScope,
    ExportPresetFormat,
    FootnoteFirstBaseline,
    FootnoteMarkerPositioning,
    FootnoteNumberingStyle,
    FootnoteRestarting,
    IndexFormat,
    NothingEnum,
    NumberedParagraphsOptions,
    PageNumberPosition,
    PDFExportPreset,
    SearchModes,
    SearchStrategies,
    TaggedPDFStructureOrderOptions,
    UIColors,
    VariableNumberingStyles,
    VariableScopes,
    VariableTypes,
} from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection, WriteRejection } from '@rasm/creative-cloud-server/errors';
import type {
    Article,
    Color,
    Condition,
    CrossReferenceFormat,
    DateVariablePreference,
    Document,
    FileNameVariablePreference,
    MatchParagraphStylePreference,
    PageNumberVariablePreference,
    PreflightProfile,
    PrinterPreset,
    TextVariable,
    TOCStyle,
} from '@rasm/creative-cloud-server/indesign';
import { members } from '@rasm/creative-cloud-server/indesign';
import { TypographyError } from '@rasm/creative-cloud-server/indesign/jobs';
import {
    PublishingComment,
    PublishingError,
    type PublishingInput,
    PublishingOutput,
    type PublishingPreset,
    PublishingProfile,
    PublishingSections,
} from '@rasm/creative-cloud-server/indesign/publishing';
import { AbsolutePath } from '@rasm/creative-cloud-server/values';
import type { PageSize } from '@rasm/typography/page-sizes';
import { Array, Effect, Equal, Exit, Function, flow, identity, Option, Record, Result, Schema, type Scope, Struct } from 'effect';
import { documentFor, fileFor, type Live, live } from '../host.ts';
import type { Typography } from '../typography.ts';
import { readProperties, writeProperties } from './preferences.ts';

// --- [DOCUMENT] ------------------------------------------------------------------------

const configureDocument: (
    document: Document,
    typography: Typography,
    leading: number,
    intent: PageSize['intent'],
    ink: Color,
) => Effect.Effect<
    {
        readonly running: TextVariable;
        readonly lastPage: TextVariable;
        readonly fileName: TextVariable;
        readonly outputDate: TextVariable;
        readonly article: Article;
        readonly contents: TOCStyle;
        readonly reference: CrossReferenceFormat;
    },
    Array.NonEmptyReadonlyArray<(typeof TypographyError.cases.textVariableSelection)['Type']>
> = Effect.fnUntraced(function* (document, typography, leading, intent, ink) {
    const available = document.textVariables.everyItem().getElements();
    const selections = Record.map(
        {
            running: VariableTypes.MATCH_PARAGRAPH_STYLE_TYPE,
            lastPage: VariableTypes.LAST_PAGE_NUMBER_TYPE,
            fileName: VariableTypes.FILE_NAME_TYPE,
            outputDate: VariableTypes.OUTPUT_DATE_TYPE,
        },
        (type) => {
            const matches = Array.filter(available, (variable) => variable.variableType.equals(type));
            return Result.fromOption(
                Option.filter(Array.head(matches), () => matches.length === 1),
                () => TypographyError.cases.textVariableSelection.make({ type: type.toString(), candidates: Array.map(matches, Struct.get('index')) }),
            );
        },
    );
    const [errors] = Array.partition(Record.values(selections), identity);
    if (Array.isArrayNonEmpty(errors)) {
        return yield* Effect.fail(errors);
    }
    const variables = yield* Effect.fromResult(Result.all(selections)).pipe(Effect.mapError(Array.of));
    const running: MatchParagraphStylePreference = variables.running.variableOptions;
    const lastPage: PageNumberVariablePreference = variables.lastPage.variableOptions;
    const fileName: FileNameVariablePreference = variables.fileName.variableOptions;
    const outputDate: DateVariablePreference = variables.outputDate.variableOptions;
    const { conditionalTextPreferences, footnoteOptions, endnoteOptions, indexGenerationOptions, taggedPDFPreferences } = document;
    running.properties = {
        textBefore: '',
        textAfter: '',
        appliedParagraphStyle: typography.paragraphs.h1,
        searchStrategy: SearchStrategies.FIRST_ON_PAGE,
        deleteEndPunctuation: true,
        changeCase: ChangeCaseOptions.NONE,
    };
    lastPage.properties = { scope: VariableScopes.DOCUMENT_SCOPE, format: VariableNumberingStyles.ARABIC, textBefore: '', textAfter: '' };
    fileName.properties = { includeExtension: true, includePath: false, textBefore: '', textAfter: '' };
    outputDate.properties = { format: 'yyyy-MM-dd', textBefore: '', textAfter: '' };
    const reference = document.crossReferenceFormats.add({ name: 'Page Only Bold', appliedCharacterStyle: typography.characters.strong });
    reference.buildingBlocks.add(BuildingBlockTypes.PAGE_NUMBER_BUILDING_BLOCK);
    const contents = document.tocStyles.add({
        name: 'Contents',
        title: 'Contents',
        titleStyle: typography.paragraphs.contentsTitle,
        createBookmarks: true,
        runIn: false,
        includeBookDocuments: true,
        includeHidden: false,
        numberedParagraphs: NumberedParagraphsOptions.EXCLUDE_NUMBERS,
    });
    Array.forEach(
        [
            { source: typography.paragraphs.h1, formatStyle: typography.paragraphs.contents1 },
            { source: typography.paragraphs.h2, formatStyle: typography.paragraphs.contents2 },
            { source: typography.paragraphs.heading, formatStyle: typography.paragraphs.contents3 },
        ],
        ({ source, formatStyle }, index) =>
            contents.tocStyleEntries.add(source.name, {
                formatStyle,
                level: index + 1,
                pageNumberPosition: PageNumberPosition.AFTER_ENTRY,
                separator: '^t',
                pageNumberStyle: typography.characters.figures,
                separatorStyle: document.characterStyles.firstItem(),
            }),
    );
    const output = intent === 'print' ? 'print' : 'digital';
    const conditions = Record.map(
        {
            print: { name: 'Print', indicatorColor: UIColors.GREEN },
            digital: { name: 'Digital', indicatorColor: UIColors.BLUE },
            internal: { name: 'Internal', indicatorColor: UIColors.RED },
        },
        (properties, channel) => document.conditions.add({ ...properties, indicatorMethod: ConditionIndicatorMethod.USE_HIGHLIGHT, visible: channel === output }),
    );
    const conditionSets = Record.map(conditions, (selected) =>
        document.conditionSets.add({
            name: selected.name,
            setConditions: Array.map(Record.values(conditions), (condition): [Condition, boolean] => [condition, condition.id === selected.id]),
        }),
    );
    conditionalTextPreferences.properties = { activeConditionSet: conditionSets[output], showConditionIndicators: ConditionIndicatorMode.HIDE_INDICATORS };
    footnoteOptions.properties = {
        footnoteNumberingStyle: FootnoteNumberingStyle.ARABIC,
        startAt: 1,
        restartNumbering: FootnoteRestarting.DONT_RESTART,
        markerPositioning: FootnoteMarkerPositioning.NORMAL_MARKER,
        footnoteMarkerStyle: typography.characters.note,
        footnoteTextStyle: typography.paragraphs.note,
        separatorText: '\t',
        spacer: leading,
        spaceBetween: 0,
        footnoteFirstBaselineOffset: FootnoteFirstBaseline.LEADING_OFFSET,
        eosPlacement: false,
        noSplitting: false,
        ruleOn: true,
        ruleLineWeight: typography.rule,
        ruleColor: ink,
        ruleWidth:
            (Number(document.documentPreferences.pageWidth) -
                Number(document.marginPreferences.left) -
                Number(document.marginPreferences.right) -
                (document.marginPreferences.columnCount - 1) * Number(document.marginPreferences.columnGutter)) /
            document.marginPreferences.columnCount,
        ruleOffset: leading / 2,
        enableStraddling: false,
    };
    endnoteOptions.properties = {
        endnoteTitle: 'Notes',
        endnoteTitleStyle: typography.paragraphs.h1,
        endnoteNumberingStyle: FootnoteNumberingStyle.ARABIC,
        startEndnoteNumberAt: 1,
        restartEndnoteNumbering: EndnoteRestarting.CONTINUOUS,
        endnoteMarkerPositioning: FootnoteMarkerPositioning.NORMAL_MARKER,
        endnoteMarkerStyle: typography.characters.note,
        endnoteTextStyle: typography.paragraphs.note,
        endnoteSeparatorText: '\t',
        scopeValue: EndnoteScope.ENDNOTE_DOCUMENT_SCOPE,
        frameCreateOption: EndnoteFrameCreate.NEW_PAGE,
    };
    document.indexes.add();
    indexGenerationOptions.properties = {
        title: 'Index',
        titleStyle: typography.paragraphs.contentsTitle,
        replaceExistingIndex: true,
        includeBookDocuments: true,
        includeHiddenEntries: false,
        indexFormat: IndexFormat.NESTED_FORMAT,
        includeSectionHeadings: true,
        includeEmptyIndexSections: false,
        level1Style: typography.paragraphs.small,
        level2Style: typography.paragraphs.note,
        level3Style: typography.paragraphs.note,
        level4Style: typography.paragraphs.note,
        sectionHeadingStyle: typography.paragraphs.heading,
        pageNumberStyle: typography.characters.figures,
        crossReferenceStyle: typography.characters.emphasis,
        crossReferenceTopicStyle: document.characterStyles.firstItem(),
        followingTopicSeparator: ' ',
        betweenEntriesSeparator: '; ',
        betweenPageNumbersSeparator: ', ',
        pageRangeSeparator: '–',
        beforeCrossReferenceSeparator: '. ',
        entryEndSeparator: '',
    };
    taggedPDFPreferences.structureOrder = TaggedPDFStructureOrderOptions.USE_ARTICLES;
    return { ...variables, article: document.articles.add('Main', true), contents, reference };
});

// --- [NATIVE STATE] --------------------------------------------------------------------

const _profile = (profile: PreflightProfile): Effect.Effect<typeof PublishingProfile.Type, HostRejection> =>
    Effect.try({
        try: () => {
            const available = app.preflightRules.everyItem().getElements();
            return PublishingProfile.make({
                id: profile.id,
                name: profile.name,
                description: profile.description,
                rules: Array.map(profile.preflightRuleInstances.everyItem().getElements(), (rule) => ({
                    id: rule.id,
                    name: rule.name,
                    description: rule.description,
                    flag: String(rule.flag),
                    fullySupported: Array.some(available, (candidate) => candidate.id === rule.id && candidate.fullFeature),
                    data: Array.map(rule.ruleDataObjects.everyItem().getElements(), (field) => ({ id: field.id, name: field.name, type: String(field.dataType), value: field.dataValue })),
                })),
            });
        },
        catch: thrown,
    });

const _comments = (document: Document): Effect.Effect<readonly (typeof PublishingComment.Type)[], HostRejection> =>
    Effect.try({
        try: () =>
            Array.map(document.pdfComments.everyItem().getElements(), (comment) =>
                PublishingComment.make({
                    id: comment.id,
                    reviewer: comment.commentReviewer,
                    content: comment.commentContent,
                    date: comment.commentDate.toISOString(),
                    type: String(comment.commentType),
                    source: comment.commentFilePath,
                    geometry: Result.mapError(
                        Result.try(() => Schema.decodeUnknownSync(Schema.Json)(comment.commentPathGeometry)),
                        thrown,
                    ),
                    status: String(comment.commentStatus),
                    orphan: comment.commentIsOrphan,
                    applied: comment.commentIsApplied,
                    replies: Array.map(comment.replies.everyItem().getElements(), (reply) => ({
                        id: reply.id,
                        reviewer: reply.replyReviewer,
                        content: reply.replyContent,
                        date: reply.replyDate.toISOString(),
                    })),
                }),
            ),
        catch: thrown,
    });

const _configure = (
    table: Live,
    target: object,
    path: readonly string[],
    values: ReadonlyArray<{ readonly key: string; readonly value: Schema.Json }>,
): Effect.Effect<void, Array.NonEmptyReadonlyArray<typeof PublishingError.Type>> =>
    writeProperties(
        table,
        Array.map(values, (value) => ({ target, ...value })),
        Option.none(),
    ).pipe(
        Effect.mapError((reason) => [PublishingError.cases.native.make({ reason })] as const),
        Effect.flatMap((outcomes) => {
            const [errors] = Array.partition(Array.zip(values, outcomes), ([{ key }, outcome]) =>
                Result.mapError(outcome, (reason) => PublishingError.cases.write.make({ path: Array.append(path, key), reason })),
            );
            return Array.isArrayNonEmpty(errors) ? Effect.fail(errors) : Effect.void;
        }),
    );

const _preset: (table: Live, preset: PDFExportPreset | PrinterPreset) => Effect.Effect<typeof PublishingPreset.Type> = Effect.fnUntraced(function* (table, preset) {
    const pdf = preset instanceof PDFExportPreset;
    const values = yield* readProperties(table, preset, pdf ? members.PDFExportPreset : members.PrinterPreset);
    return {
        kind: pdf ? 'pdf' : 'printer',
        name: preset.name,
        file: Option.flatMap(Option.flatMap(Record.get<string, Result.Result<Schema.Json, HostRejection>>(values, 'fullName'), Result.getSuccess), Schema.decodeUnknownOption(AbsolutePath)),
        values,
    };
});

const _publish = <Request extends { readonly name: string }, Native extends { readonly duplicate: () => Native; readonly remove: () => void }, Value>(
    requests: Iterable<Request>,
    resolve: (request: Request) => Effect.Effect<Native, Array.NonEmptyReadonlyArray<typeof PublishingError.Type>>,
    configure: (created: Native, request: Request) => Effect.Effect<{ readonly retained: Native; readonly value: Value }, Array.NonEmptyReadonlyArray<typeof PublishingError.Type>, Scope.Scope>,
): Effect.Effect<[Array<{ readonly target: string; readonly reason: typeof PublishingError.Type }>, Value[]]> =>
    Effect.partition(
        requests,
        Effect.fnUntraced(
            function* (request) {
                const source = yield* resolve(request);
                const configured = yield* Effect.acquireUseRelease(
                    Effect.sync(() => source.duplicate()),
                    (created) => Effect.scoped(configure(created, request)),
                    (created, exit) => (Exit.isSuccess(exit) && exit.value.retained === created ? Effect.void : Effect.sync(() => created.remove())),
                );
                return configured.value;
            },
            (effect) => Effect.catchDefect(effect, (reason) => Effect.fail([PublishingError.cases.native.make({ reason: thrown(reason) })] as const)),
            (effect, request) =>
                Effect.mapError(
                    effect,
                    Array.map((reason) => ({ target: request.name, reason })),
                ),
        ),
    ).pipe(Effect.map(([errors, values]) => [Array.flatten(errors), values]));

// --- [PUBLISHING] ----------------------------------------------------------------------

const publishing: (input: typeof PublishingInput.Type) => Effect.Effect<typeof PublishingOutput.Type, HostRejection> = Effect.fnUntraced(
    function* (input) {
        if (input.operation === 'preflight') {
            const document = yield* documentFor(input.documentId);
            const profiles = [...document.preflightProfiles.everyItem().getElements(), ...app.preflightProfiles.everyItem().getElements()];
            const candidates = Option.match(input.profileId, {
                onNone: () => {
                    const working = document.preflightOptions.preflightWorkingProfile;
                    return Array.filter(profiles, (candidate) => (typeof working === 'string' ? candidate.name === working : candidate.id === working.id));
                },
                onSome: (id) => Array.filter(profiles, (candidate) => candidate.id === id),
            });
            const selected = yield* Effect.fromOption(
                Option.filter(Array.head(candidates), () => candidates.length === 1),
                () => HostRejection.cases.profileAbsent.make({ profile: Option.match(input.profileId, { onNone: () => document.name, onSome: String }) }),
            );
            const profile = yield* _profile(selected);
            const coverage = {
                scope: String(document.preflightOptions.preflightScope),
                layers: String(document.preflightOptions.preflightWhichLayers),
                nonprinting: document.preflightOptions.preflightIncludeNonprintingObjects,
                pasteboard: document.preflightOptions.preflightIncludeObjectsOnPasteboard,
            };
            const process = yield* Effect.acquireRelease(Effect.try({ try: () => app.preflightProcesses.add(document, selected, document.preflightOptions), catch: thrown }), (created) =>
                Effect.sync(() => created.remove()),
            );
            const completed = yield* Effect.try({ try: () => process.waitForProcess(input.maximumSeconds), catch: thrown });
            if (!completed) {
                return { kind: 'incomplete' as const, profile, coverage };
            }
            return yield* Effect.try({ try: () => PublishingOutput.cases.completed.make({ kind: 'completed', profile, coverage, results: process.aggregatedResults }), catch: thrown });
        }
        if (input.operation === 'review') {
            const document = yield* documentFor(input.documentId);
            const before = yield* _comments(document);
            yield* Effect.forEach(Option.toArray(input.source), (source) =>
                Effect.flatMap(fileFor(source, false), (file) => Effect.try({ try: () => document.importPdfComments(file), catch: thrown })),
            );
            const state = yield* _comments(document);
            const [rejected, changed] = yield* Effect.partition(input.comments, ({ id, expected, status }) =>
                Effect.try({
                    try: (): Result.Result<number, typeof PublishingError.Type> => {
                        const comment = document.pdfComments.itemByID(id);
                        if (
                            !(
                                comment.isValid &&
                                Equal.equals({ status: String(comment.commentStatus), applied: comment.commentIsApplied, orphan: comment.commentIsOrphan, content: comment.commentContent }, expected)
                            )
                        ) {
                            return Result.fail(PublishingError.cases.conflict.make({ id }));
                        }
                        comment.changeStatus(CommentStatusEnum[status]);
                        return comment.commentStatus.equals(CommentStatusEnum[status])
                            ? Result.succeed(id)
                            : Result.fail(PublishingError.cases.write.make({ path: ['commentStatus'], reason: WriteRejection.cases.unchanged.make({}) }));
                    },
                    catch: (reason) => PublishingError.cases.native.make({ reason: thrown(reason) }),
                }).pipe(
                    Effect.flatMap(Effect.fromResult),
                    Effect.mapError((reason) => ({ target: String(id), reason })),
                ),
            );
            return {
                kind: 'review' as const,
                imported: Array.difference(Array.map(state, Struct.get('id')), Array.map(before, Struct.get('id'))),
                comments: yield* _comments(document),
                changed,
                rejected,
            };
        }
        const document = input.target._tag === 'application' ? Option.none() : Option.some(yield* documentFor(input.target.id));
        const table = yield* live;
        const owner = Option.getOrElse(document, () => app);
        const settings = {
            preflightOptions: owner.preflightOptions,
            dictionaryPreferences: owner.dictionaryPreferences,
            pdfExportPreferences: app.pdfExportPreferences,
            ...Record.getSomes({ printPreferences: Option.map(document, Struct.get('printPreferences')) }),
        };
        const profiles = [...app.preflightProfiles.everyItem().getElements(), ...Array.flatMap(Option.toArray(document), (open) => open.preflightProfiles.everyItem().getElements())];
        if (input.operation === 'inspect') {
            return {
                kind: 'inspection' as const,
                document: Option.map(document, ({ id, name, modified }) => ({ id, name, modified })),
                profiles: yield* Effect.forEach(profiles, _profile),
                presets: yield* Effect.forEach([...app.pdfExportPresets.everyItem().getElements(), ...app.printerPresets.everyItem().getElements()], (preset) => _preset(table, preset)),
                policies: yield* Effect.all(Record.map(settings, (target, section) => readProperties(table, target, members[PublishingSections[section]]))),
                rules: Array.map(app.preflightRules.everyItem().getElements(), ({ id, name, description, fullFeature }) => ({ id, name, description, fullySupported: fullFeature })),
                languages: Array.map(app.languagesWithVendors.everyItem().getElements(), ({ id, name, icuLocaleName }) => ({ id, name, locale: icuLocaleName })),
                comments: Array.flatten(yield* Effect.forEach(Option.toArray(document), _comments)),
            };
        }
        if (input.operation === 'transfer') {
            const [rejected, files] = yield* Effect.partition(
                input.resources,
                Effect.fnUntraced(
                    function* (resource) {
                        switch (resource._tag) {
                            case 'importProfile':
                                app.loadPreflightProfile(yield* fileFor(resource.path, false));
                                break;
                            case 'exportProfile': {
                                const candidates = Array.filter(profiles, (profile) => profile.id === resource.id);
                                const selected = yield* Effect.fromOption(
                                    Option.filter(Array.head(candidates), () => candidates.length === 1),
                                    () => PublishingError.cases.selection.make({ resource: String(resource.id), candidates: Array.map(candidates, Struct.get('name')) }),
                                );
                                selected.save(yield* fileFor(resource.path, true));
                                break;
                            }
                            case 'importPresets':
                                app.importFile(ExportPresetFormat[resource.format], yield* fileFor(resource.path, false));
                                break;
                            case 'exportPresets':
                                app.exportPresets(ExportPresetFormat[resource.format], yield* fileFor(resource.path, true));
                                break;
                        }
                        return resource.path;
                    },
                    (effect) => Effect.catchDefect(effect, (reason) => Effect.fail(PublishingError.cases.native.make({ reason: thrown(reason) }))),
                    (effect, resource) =>
                        Effect.mapError(effect, (reason) => ({ target: resource.path, reason: Schema.is(PublishingError)(reason) ? reason : PublishingError.cases.native.make({ reason }) })),
                ),
            );
            return {
                kind: 'transferred' as const,
                profiles: yield* Effect.forEach(
                    [...app.preflightProfiles.everyItem().getElements(), ...Array.flatMap(Option.toArray(document), (open) => open.preflightProfiles.everyItem().getElements())],
                    _profile,
                ),
                presets: yield* Effect.forEach([...app.pdfExportPresets.everyItem().getElements(), ...app.printerPresets.everyItem().getElements()], (preset) => _preset(table, preset)),
                files,
                rejected,
            };
        }
        const workingProfile = owner.preflightOptions.preflightWorkingProfile;
        const previousProfiles = typeof workingProfile === 'string' ? Array.filter(profiles, (profile) => profile.name === workingProfile) : [workingProfile];
        const preflightPreferences = Option.map(
            Option.filter(Array.head(previousProfiles), () => previousProfiles.length === 1),
            (profile) => ({
                preflightWorkingProfile: profile,
                preflightOff: owner.preflightOptions.preflightOff,
                ...(Option.isNone(document) ? { preflightEmbedWorkingProfile: app.preflightOptions.preflightEmbedWorkingProfile } : {}),
            }),
        );
        const [preflightErrors, configuredProfiles] = yield* _publish(
            Array.map(Option.toArray(input.preflight), (request) => ({
                ...request,
                destination: Option.liftPredicate(
                    Option.isSome(document) && request.embed ? document.value.preflightProfiles.firstItem() : app.preflightProfiles.itemByName(request.name),
                    Struct.get('isValid'),
                ),
            })),
            (request) => {
                if (Option.isNone(preflightPreferences)) {
                    return Effect.fail([
                        PublishingError.cases.selection.make({
                            resource: typeof workingProfile === 'string' ? workingProfile : workingProfile.name,
                            candidates: Array.map(previousProfiles, (profile) => profile.toSpecifier()),
                        }),
                    ] as const);
                }
                const candidates = Array.filter(profiles, (candidate) => candidate.id === request.id);
                return Effect.fromOption(
                    Option.filter(Array.head(candidates), () => candidates.length === 1),
                    () => [PublishingError.cases.selection.make({ resource: request.name, candidates: Array.map(candidates, Struct.get('name')) })] as const,
                );
            },
            Effect.fnUntraced(function* (profile, request) {
                const registered = Record.fromIterableBy(app.preflightRules.everyItem().getElements(), Struct.get('id'));
                const initial = Record.fromIterableBy(profile.preflightRuleInstances.everyItem().getElements(), Struct.get('id'));
                const [additionErrors] = yield* Effect.partition(
                    Array.filter(request.rules, (rule) => !Record.has(initial, rule.id)),
                    (rule) =>
                        Effect.fromOption(Record.get(registered, rule.id), () => PublishingError.cases.selection.make({ resource: rule.id, candidates: Record.keys(registered) })).pipe(
                            Effect.andThen(Effect.try({ try: () => profile.preflightProfileRules.add(rule.id), catch: (reason) => PublishingError.cases.native.make({ reason: thrown(reason) }) })),
                        ),
                );
                const rules = Record.fromIterableBy(profile.preflightRuleInstances.everyItem().getElements(), Struct.get('id'));
                const data = Record.map(rules, (rule) => Record.fromIterableBy(rule.ruleDataObjects.everyItem().getElements(), Struct.get('id')));
                const fields = Array.flatMap(request.rules, (rule) => Array.map(Record.toEntries(rule.data), ([key, value]) => ({ ruleId: rule.id, key, value })));
                const [flagErrors] = yield* Effect.partition(request.rules, (rule) =>
                    Effect.fromOption(Record.get(rules, rule.id), () => [PublishingError.cases.selection.make({ resource: rule.id, candidates: Record.keys(rules) })] as const).pipe(
                        Effect.flatMap((target) => _configure(table, target, [rule.id], [{ key: 'flag', value: rule.flag }])),
                    ),
                );
                const [dataErrors] = yield* Effect.partition(fields, ({ ruleId, key, value }) =>
                    Effect.fromOption(
                        Option.flatMap(Record.get(data, ruleId), Record.get(key)),
                        () => [PublishingError.cases.selection.make({ resource: key, candidates: Record.keys(Option.getOrElse(Record.get(data, ruleId), Record.empty)) })] as const,
                    ).pipe(Effect.flatMap((field) => _configure(table, field, [ruleId, key], [{ key: 'dataValue', value }]))),
                );
                const errors = [...additionErrors, ...Array.flatten(flagErrors), ...Array.flatten(dataErrors)];
                if (Array.isArrayNonEmpty(errors)) {
                    return yield* Effect.fail(errors);
                }
                const embedded = Option.filter(document, Function.constant(request.embed));
                const backup = yield* Effect.acquireRelease(
                    Effect.sync(() => Option.map(request.destination, (existing) => ({ copy: existing.duplicate(), existing }))),
                    (saved, exit) =>
                        Effect.sync(() => {
                            if (Option.isSome(saved) && Exit.isFailure(exit)) {
                                saved.value.existing.update(saved.value.copy);
                            }
                        }).pipe(
                            Effect.ensuring(
                                Effect.sync(() => {
                                    if (Option.isSome(saved)) {
                                        saved.value.copy.remove();
                                    }
                                }),
                            ),
                        ),
                );
                const expected = yield* _profile(profile).pipe(Effect.mapError((reason) => [PublishingError.cases.native.make({ reason })] as const));
                const retained = Option.isSome(request.destination)
                    ? request.destination.value
                    : Option.match(embedded, {
                          onNone: () => app.preflightProfiles.add({ name: request.name }),
                          onSome: (open) => open.embed(profile),
                      });
                yield* Effect.addFinalizer((exit) => (Exit.isFailure(exit) && Option.isNone(backup) ? Effect.sync(() => retained.remove()) : Effect.void));
                retained.update(profile);
                if (retained.name !== request.name) {
                    retained.name = request.name;
                }
                const configured = yield* _profile(retained).pipe(Effect.mapError((reason) => [PublishingError.cases.native.make({ reason })] as const));
                if (!Equal.equals(configured.rules, expected.rules)) {
                    return yield* Effect.fail([PublishingError.cases.write.make({ path: ['preflightRuleInstances'], reason: WriteRejection.cases.unchanged.make({}) })] as const);
                }
                yield* Effect.forEach(
                    Option.toArray(request.export),
                    Effect.fnUntraced(function* (path) {
                        retained.save(yield* fileFor(path, true));
                    }),
                ).pipe(Effect.mapError((reason) => [PublishingError.cases.native.make({ reason })] as const));
                owner.preflightOptions.properties = {
                    preflightWorkingProfile: retained,
                    preflightOff: false,
                    ...(Option.isNone(document) ? { preflightEmbedWorkingProfile: request.embed } : {}),
                };
                const working = owner.preflightOptions.preflightWorkingProfile;
                if (
                    !(
                        (typeof working === 'string' ? working === retained.name : working.id === retained.id) &&
                        !owner.preflightOptions.preflightOff &&
                        (Option.isSome(document) || app.preflightOptions.preflightEmbedWorkingProfile === request.embed)
                    )
                ) {
                    return yield* Effect.fail([PublishingError.cases.write.make({ path: ['preflightWorkingProfile'], reason: WriteRejection.cases.unchanged.make({}) })] as const);
                }
                return { retained, value: { profile: configured, file: request.export } };
            }),
        ).pipe(
            Effect.onExit((exit) =>
                (Exit.isSuccess(exit) && Array.isArrayEmpty(exit.value[0])) || Option.isNone(preflightPreferences)
                    ? Effect.void
                    : Effect.sync(() => {
                          owner.preflightOptions.properties = preflightPreferences.value;
                      }),
            ),
        );
        const [presetErrors, presets] = yield* _publish(
            Array.map(input.presets, (request) => ({
                ...request,
                destination: Option.liftPredicate((request.kind === 'pdf' ? app.pdfExportPresets : app.printerPresets).itemByName(request.name), Struct.get('isValid')),
            })),
            (request) => {
                const collection = request.kind === 'pdf' ? app.pdfExportPresets : app.printerPresets;
                return Effect.fromOption(
                    Option.liftPredicate(collection.itemByName(request.source), Struct.get('isValid')),
                    () => [PublishingError.cases.selection.make({ resource: request.source, candidates: Array.map(collection.everyItem().getElements(), Struct.get('name')) })] as const,
                );
            },
            Effect.fnUntraced(function* (created, request) {
                const stagedName = created.name;
                yield* _configure(table, created, [], request.values);
                yield* Effect.acquireRelease(
                    Effect.sync(() => Option.map(request.destination, (existing) => ({ preset: existing.duplicate(), name: existing.name }))),
                    (saved, exit) =>
                        Effect.sync(() => {
                            if (Option.isNone(saved)) {
                                return;
                            }
                            if (Exit.isSuccess(exit) || Option.exists(request.destination, Struct.get('isValid'))) {
                                saved.value.preset.remove();
                                return;
                            }
                            Reflect.set(created, 'name', stagedName);
                            saved.value.preset.name = saved.value.name;
                        }),
                );
                yield* Effect.forEach(Option.toArray(request.destination), (existing) => Effect.sync(() => existing.remove()));
                yield* _configure(table, created, [], [{ key: 'name', value: request.name }]);
                return { retained: created, value: yield* _preset(table, created) };
            }),
        );
        const [policyErrors] = yield* Effect.partition(input.policies, ({ section, values }) =>
            Effect.fromOption(Record.get<string, object>(settings, section), () => [PublishingError.cases.selection.make({ resource: section, candidates: Struct.keys(settings) })] as const).pipe(
                Effect.flatMap((target) => _configure(table, target, [section], values)),
                Effect.mapError(Array.map((reason) => ({ target: section, reason }))),
            ),
        );
        const languages = app.languagesWithVendors.everyItem().getElements();
        const [queryErrors, queries] = yield* Effect.partition(
            input.queries,
            Effect.fnUntraced(
                function* (request) {
                    const language = Option.flatMap(request.languageId, (id) => Array.findFirst(languages, (candidate) => candidate.id === id));
                    if (Option.isSome(request.languageId) && Option.isNone(language)) {
                        return yield* Effect.fail([
                            PublishingError.cases.selection.make({ resource: String(request.languageId.value), candidates: Array.map(languages, (candidate) => String(candidate.id)) }),
                        ] as const);
                    }
                    yield* Effect.acquireRelease(
                        Effect.sync(() => ({ find: app.findGrepPreferences.properties, change: app.changeGrepPreferences.properties, options: app.findChangeGrepOptions.properties })),
                        (saved) =>
                            Effect.sync(() => {
                                app.findGrepPreferences = NothingEnum.NOTHING;
                                app.changeGrepPreferences = NothingEnum.NOTHING;
                                app.findGrepPreferences.properties = saved.find;
                                app.changeGrepPreferences.properties = saved.change;
                                app.findChangeGrepOptions.properties = saved.options;
                            }),
                    );
                    app.findGrepPreferences = NothingEnum.NOTHING;
                    app.changeGrepPreferences = NothingEnum.NOTHING;
                    app.findGrepPreferences.properties = { findWhat: request.findWhat, appliedLanguage: Option.getOrElse(language, () => NothingEnum.NOTHING) };
                    app.changeGrepPreferences.changeTo = request.changeTo;
                    yield* _configure(table, app.findChangeGrepOptions, [], request.options);
                    const options = yield* readProperties(table, app.findChangeGrepOptions, members.FindChangeGrepOption);
                    app.saveFindChangeQuery(request.name, SearchModes.GREP_SEARCH);
                    app.findGrepPreferences = NothingEnum.NOTHING;
                    app.changeGrepPreferences = NothingEnum.NOTHING;
                    app.loadFindChangeQuery(request.name, SearchModes.GREP_SEARCH);
                    const sameLanguage = Option.match(language, {
                        onNone: () => NothingEnum.NOTHING.equals(app.findGrepPreferences.appliedLanguage),
                        onSome: (selected) => app.findGrepPreferences.appliedLanguage === selected.name,
                    });
                    if (
                        app.findGrepPreferences.findWhat !== request.findWhat ||
                        app.changeGrepPreferences.changeTo !== request.changeTo ||
                        !sameLanguage ||
                        !Equal.equals(options, yield* readProperties(table, app.findChangeGrepOptions, members.FindChangeGrepOption))
                    ) {
                        return yield* Effect.fail([
                            PublishingError.cases.write.make({ path: ['findWhat', 'changeTo', 'appliedLanguage', 'options'], reason: WriteRejection.cases.unchanged.make({}) }),
                        ] as const);
                    }
                    return { ...request, options: Array.map(Record.toEntries(Record.getSuccesses(options)), ([key, value]) => ({ key, value })) };
                },
                Effect.scoped,
                (effect) => Effect.catchDefect(effect, (reason) => Effect.fail([PublishingError.cases.native.make({ reason: thrown(reason) })] as const)),
                (effect, request) =>
                    Effect.mapError(
                        effect,
                        Array.map((reason) => ({ target: request.name, reason })),
                    ),
            ),
        );
        const [dictionaryErrors, dictionaries] = yield* Effect.partition(
            input.dictionaries,
            Effect.fnUntraced(
                function* (request) {
                    const language = yield* Effect.fromOption(
                        Array.findFirst(languages, (candidate) => candidate.id === request.languageId),
                        () => PublishingError.cases.selection.make({ resource: String(request.languageId), candidates: Array.map(languages, (candidate) => String(candidate.id)) }),
                    );
                    const dictionary = yield* Effect.fromOption(Option.liftPredicate(app.userDictionaries.itemByName(language.name), Struct.get('isValid')), () =>
                        PublishingError.cases.selection.make({ resource: language.name, candidates: Array.map(app.userDictionaries.everyItem().getElements(), Struct.get('name')) }),
                    );
                    const requested = Struct.pick(request, ['addedWords', 'removedWords']);
                    yield* Effect.forEach(Record.toEntries(Record.filter(requested, Array.isReadonlyArrayNonEmpty)), ([list, words]) =>
                        Effect.sync(() => dictionary.addWord(Array.dedupe(words), list === 'removedWords')),
                    );
                    const actual = Record.map(requested, (_, key) => dictionary[key]);
                    return {
                        languageId: language.id,
                        name: dictionary.name,
                        ...actual,
                        missing: Array.flatMap(Record.toEntries(requested), ([list, words]) => Array.map(Array.difference(words, actual[list]), (word) => ({ list, word }))),
                    };
                },
                (effect) => Effect.catchDefect(effect, (reason) => Effect.fail(PublishingError.cases.native.make({ reason: thrown(reason) }))),
                (effect, request) => Effect.mapError(effect, (reason) => ({ target: String(request.languageId), reason })),
            ),
        );
        const composed = yield* Effect.result(
            Effect.forEach(Option.toArray(Option.filter(document, () => Array.isArrayNonEmpty(dictionaries))), (open) => Effect.try({ try: () => open.recompose(), catch: thrown })),
        );
        return {
            kind: 'configured' as const,
            profiles: Array.map(configuredProfiles, Struct.get('profile')),
            presets,
            policies: yield* Effect.all(Record.map(settings, (target, section) => readProperties(table, target, members[PublishingSections[section]]))),
            queries,
            dictionaries: Array.map(dictionaries, Struct.omit(['missing'])),
            files: Array.flatMap(configuredProfiles, (configured) => Option.toArray(configured.file)),
            rejected: [
                ...preflightErrors,
                ...presetErrors,
                ...Array.flatten(queryErrors),
                ...Array.flatten(policyErrors),
                ...dictionaryErrors,
                ...Array.flatMap(dictionaries, ({ missing }) =>
                    Array.map(missing, ({ list, word }) => ({ target: word, reason: PublishingError.cases.write.make({ path: [list], reason: WriteRejection.cases.unchanged.make({}) }) })),
                ),
                ...Array.map(Option.toArray(Result.getFailure(composed)), (reason) => ({ target: owner.name, reason: PublishingError.cases.native.make({ reason }) })),
            ],
        };
    },
    Effect.scoped,
    Effect.catchDefect(flow(thrown, Effect.fail)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { configureDocument, publishing };
