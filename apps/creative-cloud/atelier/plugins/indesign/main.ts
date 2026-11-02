import indesign from 'adobe:indesign';
import { Contract, host } from '@rasm/indesign/host';
import { plugin } from '@rasm/uxp/entrypoints';
import { Link } from '@rasm/uxp/link';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { server, application } = host(indesign);

plugin((link) => Link.dial(Contract, server, { ...link, path: '/com.adobe.InDesign', application }));
