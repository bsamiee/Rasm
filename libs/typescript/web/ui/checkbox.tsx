import { Check } from 'lucide-react';
import type { ReactNode } from 'react';
import { CheckboxButton, CheckboxField, type CheckboxFieldProps } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Checkbox = ({ children, ...props }: Omit<CheckboxFieldProps, 'children'> & { children?: ReactNode }) => (
    <CheckboxField {...props}>
        <CheckboxButton>
            <Check />
            {children}
        </CheckboxButton>
    </CheckboxField>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Checkbox };
