import { Record } from 'effect';
import type { ReactNode } from 'react';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CustomProperties = ({ properties, children }: { properties: Record.ReadonlyRecord<`--${string}`, number | string>; children: (scope: { readonly 'data-properties': string }) => ReactNode }) => {
    const declarations = Record.collect(properties, (name, value) => `${name}:${value}`).join(';');
    const key = encodeURIComponent(declarations);
    return (
        <>
            <style href={key} precedence="properties">{`[data-properties="${key}"]{${declarations}}`}</style>
            {children({ 'data-properties': key })}
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CustomProperties };
