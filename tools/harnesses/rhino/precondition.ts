import { Record, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Expression = Schema.NonEmptyString.annotate({ description: 'C# expression over doc, view, made, canvas, editor, and scope' });
const Member = Schema.NonEmptyString.annotate({ description: 'Enumeration member name the engine decodes against RhinoCommon' });
const Point = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite]);
const Tolerance = Schema.Struct({
    absolute: Schema.Finite.annotate({ description: 'ModelAbsoluteTolerance in the case units' }),
    angle: Schema.optionalKey(Schema.Finite.annotate({ description: 'ModelAngleToleranceDegrees' })),
    relative: Schema.optionalKey(Schema.Finite.annotate({ description: 'ModelRelativeTolerance' })),
});
const sources = {
    template: { template: Schema.NonEmptyString.annotate({ description: 'File under FileSettings.TemplateFolder of the running language' }), units: Member.annotate({ description: 'UnitSystem member' }), tolerance: Tolerance },
    open: { open: Schema.NonEmptyString.annotate({ description: '.3dm path under the product folder, its own units and tolerances kept unless stated' }), units: Schema.optionalKey(Member), tolerance: Schema.optionalKey(Tolerance) },
    application: { application: Schema.Literal(true).annotate({ description: 'Application-level case opening no document' }) },
};
const scripted = {
    canvas: Schema.optionalKey(Schema.Struct({ open: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: '.ghz path under the product folder' })) }).annotate({ description: 'Grasshopper 2 document the editor canvas shows, new without open' })),
    steps: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'C# statements run after add and select in one undo record' })),
    batch: Schema.optionalKey(Schema.Boolean.annotate({ description: 'BatchMode on for the case, command line input in place of dialogs' })),
};
const documented = new Set(Object.values(sources).flatMap(Object.keys));

// --- [OPERATIONS] ----------------------------------------------------------------------

const merged = (batch: Record.ReadonlyRecord<string, Schema.Json>, own: Record.ReadonlyRecord<string, Schema.Json>) => ({ ...(Object.keys(own).some((key) => key in sources) ? Record.filter(batch, (_, key) => !documented.has(key)) : batch), ...own });
const precondition = (languages: readonly string[]) => {
    const language = { language: Schema.optionalKey(Schema.Literals(languages).annotate({ description: 'RhCore localization Localization.CurrentLanguageId must report' })) };
    const placed = {
        ...language,
        view: Schema.optionalKey(
            Schema.Struct({
                projection: Schema.optionalKey(Member.annotate({ description: 'DefinedViewportProjection member' })),
                display: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'DisplayModeDescription English name' })),
                location: Schema.optionalKey(Point),
                target: Schema.optionalKey(Point),
                lens: Schema.optionalKey(Schema.Finite.annotate({ description: 'Camera35mmLensLength' })),
            }).annotate({ description: 'Active viewport of the case document' }),
        ),
        layout: Schema.optionalKey(Schema.Struct({ name: Schema.NonEmptyString, width: Schema.Finite, height: Schema.Finite, units: Schema.optionalKey(Member.annotate({ description: 'UnitSystem member of the page units' })) }).annotate({ description: 'Page view added in page units' })),
        layers: Schema.optionalKey(
            Schema.Array(Schema.Struct({ path: Schema.NonEmptyString.annotate({ description: 'Layer full path, parents joined by ::' }), color: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'HTML color name or #rrggbb' })), locked: Schema.optionalKey(Schema.Boolean) })).annotate({
                description: 'Layers made or updated before add, each parent path created first',
            }),
        ),
        add: Schema.optionalKey(
            Schema.Array(Schema.Struct({ as: Schema.NonEmptyString, geometry: Expression.annotate({ description: 'GeometryBase expression' }), layer: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Layer full path' })) })).annotate({
                description: 'Objects added with default attributes, their ids read as made["<as>"]',
            }),
        ),
        select: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'made names selected, a locked, hidden, or absent one failing the case' })),
        ...scripted,
    };
    return Schema.Union([Schema.Struct({ ...sources.template, ...placed }), Schema.Struct({ ...sources.open, ...placed }), Schema.Struct({ ...sources.application, ...language, ...scripted })]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { merged, precondition };
