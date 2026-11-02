import photoshop from 'adobe:photoshop';
import { Contract, host } from '@rasm/photoshop/host';
import { plugin } from '@rasm/uxp/entrypoints';
import { Link } from '@rasm/uxp/link';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { server, application } = host(photoshop);

plugin((link) => Link.dial(Contract, server, { ...link, path: '/com.adobe.Photoshop', application }));
