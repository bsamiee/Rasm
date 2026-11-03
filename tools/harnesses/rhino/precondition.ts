import { Schema, Struct } from 'effect';
import { Application, placings } from '#/precondition';
import { Opened, Placed, type Survey, Template } from '#/rhino/contract';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Survey.Type) => {
    const parts = survey.language.culture.split('-').filter((part) => part !== '');
    const ancestry = parts.map((_, index) => parts.slice(0, parts.length - index).join('-'));
    const language = { language: Schema.optionalKey(Schema.Literals(ancestry).annotate({ description: 'Culture the instance runs or one of its parents' })) };
    const placed = { ...Struct.omit(Placed.fields, ['source', 'language']), ...language };
    const scripted = { ...Struct.pick(Placed.fields, ['canvas', 'steps', 'batch']), ...language };
    return placings(
        Placed,
        {
            document: Schema.Struct({ template: Template.name, units: Template.units, tolerance: Template.tolerance, ...placed }),
            open: Schema.Struct({ open: Opened.path, units: Opened.units, tolerance: Opened.tolerance, ...placed }),
            application: Schema.Struct({ application: Application, ...scripted }),
        },
        {
            document: ({ template, units, tolerance, ...rest }) => ({ ...rest, source: { _tag: 'Template' as const, name: template, units, tolerance } }),
            open: ({ open, units, tolerance, ...rest }) => ({ ...rest, source: { _tag: 'Open' as const, path: open, ...(units === undefined ? {} : { units }), ...(tolerance === undefined ? {} : { tolerance }) } }),
            application: ({ application: _, ...rest }) => ({ ...rest, source: { _tag: 'Application' as const } }),
        },
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
