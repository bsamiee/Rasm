import { RegistryProvider } from '@effect/atom-react';
import type { ReactNode } from 'react';
import { Site } from './site.tsx';

// --- [OPERATIONS] ----------------------------------------------------------------------

const root = (value: Parameters<typeof Site>[0]): ReactNode => (
    <RegistryProvider>
        <Site {...value} />
    </RegistryProvider>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { root };
