import { Schema } from 'effect';
import { hydrateRoot } from 'react-dom/client';
import { Bootstrap } from '../model/document.ts';
import '../theme.css';
import { Site } from './site.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const root = document.querySelector('#portfolio');
if (root) {
    hydrateRoot(root, <Site {...Schema.decodeUnknownSync(Schema.fromJsonString(Bootstrap))(document.querySelector('#portfolio-data')?.textContent)} />);
}
