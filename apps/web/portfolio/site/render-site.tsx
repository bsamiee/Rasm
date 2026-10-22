import { RegistryProvider } from '@effect/atom-react';
import type { ComponentProps } from 'react';
import { Site } from '#/site/site';

// --- [OPERATIONS] ----------------------------------------------------------------------

const renderSite = (value: ComponentProps<typeof Site>) => (
    <RegistryProvider>
        <Site {...value} />
    </RegistryProvider>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { renderSite };
