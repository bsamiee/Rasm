import type { InDesignDOMObject } from 'adobe:indesign';
import { Option, Predicate, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Specifier = Schema.String.annotate({ description: "InDesign's path to an object, as results return it" });
class Reference extends Schema.Class<Reference, { readonly brand: unique symbol }>('Reference')({ type: Schema.String, specifier: Specifier, name: Schema.OptionFromOptionalKey(Schema.String), valid: Schema.Boolean }) {
    static readonly of = (object: InDesignDOMObject<unknown>): Reference => {
        const valid = object.isValid;
        return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: valid && 'name' in object ? Option.liftPredicate(object.name, Predicate.isString) : Option.none(), valid });
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const leaf = (specifier: string): string => specifier.slice(specifier.lastIndexOf('/') + 1);

// --- [EXPORTS] -------------------------------------------------------------------------

export { leaf, Reference, Specifier };
