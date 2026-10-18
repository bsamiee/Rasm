import { RegistryProvider } from '@effect/atom-react';
import type { ComponentProps } from 'react';
import { Site } from './site.tsx';

// --- [OPERATIONS] ----------------------------------------------------------------------

const root = (value: ComponentProps<typeof Site>) => (
    <RegistryProvider>
        <Site {...value} />
    </RegistryProvider>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { root };
