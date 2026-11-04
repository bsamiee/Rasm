import { Array, Order, Record } from 'effect';
import type { ReactNode } from 'react';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CustomProperties = ({ properties, children }: { readonly properties: Record.ReadonlyRecord<`--${string}`, number | string>; readonly children: (scope: { readonly 'data-properties': string }) => ReactNode }) => {
    const rules = Array.sort(
        Record.collect(properties, (name, value) => `${name}:${String(value).replaceAll(/[\\{};]/gu, '\\$&')}`),
        Order.String,
    ).map((declaration) => ({ declaration, scope: encodeURIComponent(declaration) }));
    return (
        <>
            {rules.map(({ declaration, scope }) => (
                <style href={scope} key={scope} precedence="properties">{`[data-properties~="${scope}"]{${declaration}}`}</style>
            ))}
            {children({ 'data-properties': rules.map(({ scope }) => scope).join(' ') })}
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CustomProperties };
