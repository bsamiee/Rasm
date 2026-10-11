import { RegistryProvider } from '@effect/atom-react';
import { Schema } from 'effect';
import { hydrateRoot } from 'react-dom/client';
import { Bootstrap } from '../model/document.ts';
import '../theme.css';
import { Site } from './site.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const root = document.querySelector('#portfolio');
if (root) {
    const data = document.querySelector('#portfolio-data');
    hydrateRoot(
        root,
        <RegistryProvider>
            <Site {...Schema.decodeUnknownSync(Schema.fromJsonString(Bootstrap))(data?.textContent)} />
        </RegistryProvider>,
    );
    data?.remove();
}
