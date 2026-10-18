import type { ReactNode } from 'react';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CustomProperties = ({ properties, children }: { properties: Readonly<Record<`--${string}`, number | string>>; children: (scope: { readonly 'data-properties': string }) => ReactNode }) => {
    const declarations = Object.entries(properties)
        .map((property) => property.join(':'))
        .join(';');
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
