import { RegistryProvider } from '@effect/atom-react';
import { hydratePage } from '@rasm/hosting/hydrate.ts';
import { shell } from '../model/document.ts';
import './theme.css';
import { Site } from './site.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

hydratePage(shell, (bootstrap) => (
    <RegistryProvider>
        <Site {...bootstrap} />
    </RegistryProvider>
));
