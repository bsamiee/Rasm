import { hydratePage } from '@rasm/hosting/hydrate.ts';
import { shell } from '../model/document.ts';
import './theme.css';
import { root } from './root.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

hydratePage(shell, root);
