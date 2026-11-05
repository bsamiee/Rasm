import { Schema, Struct } from 'effect';
import { Application, placing } from '#/cases/precondition';
import { type Contract, culture, Opened, Placed, Template } from '#/hosts/rhino/contract';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Contract.Survey.Type) => {
    const parts = survey.host.language.culture.split('-').filter((part) => part !== '');
    const language = { language: culture(Schema.Literals(parts.map((_, index) => parts.slice(0, parts.length - index).join('-')))) };
    const placed = { ...Struct.omit(Placed.fields, ['source', 'language']), ...language };
    return Schema.Union([
        placing(Placed, Schema.Struct({ template: Template.name, units: Template.units, tolerance: Template.tolerance, ...placed }), ({ template, units, tolerance, ...rest }) => ({ ...rest, source: { _tag: 'Template' as const, name: template, units, tolerance } })),
        placing(Placed, Schema.Struct({ open: Opened.path, units: Opened.units, tolerance: Opened.tolerance, ...placed }), ({ open, units, tolerance, ...rest }) => ({ ...rest, source: { _tag: 'Open' as const, path: open, ...(units === undefined ? {} : { units }), ...(tolerance === undefined ? {} : { tolerance }) } })),
        placing(Placed, Schema.Struct({ application: Application, ...Struct.pick(Placed.fields, ['canvas', 'steps', 'batch']), ...language }), ({ application: _, ...rest }) => ({ ...rest, source: { _tag: 'Application' as const } })),
    ]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
