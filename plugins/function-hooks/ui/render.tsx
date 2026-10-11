import type { Elements, RenderElement, RenderSurface } from 'claude-code';
import { none, type Option, some } from '../composition.ts';
import type { Notice, Service } from '../hooks/state.d.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Row {
    readonly label: 'hooks' | 'services';
    readonly facts: string;
    readonly button: Option<{ readonly hotkey: string; readonly label: string; readonly onPress: () => void }>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const bandRows = (notice: Option<Notice>, down: readonly Service[], restart: () => void): readonly Row[] => [
    ...(notice.kind === 'none' ? [] : [{ label: 'hooks' as const, facts: notice.value.text, button: none }]),
    ...(down.length === 0 ? [] : [{ label: 'services' as const, facts: down.map(({ name }) => `${name} down`).join(' · '), button: some({ hotkey: '1', label: 'restart', onPress: restart }) }]),
];

const band = ({ Box, Button, Text }: Elements[RenderSurface], rows: readonly Row[], below: RenderElement): RenderElement => (
    <Box flexDirection="column">
        {below}
        {rows.map((row) => (
            <Box flexDirection="row" justifyContent="space-between" key={row.label}>
                <Box>
                    <Box flexShrink={0} width={2}>
                        <Text color="suggestion">✦</Text>
                    </Box>
                    <Box flexShrink={0} width={10}>
                        <Text dimColor={true}>{row.label}</Text>
                    </Box>
                    <Text wrap="truncate">{row.facts}</Text>
                </Box>
                {row.button.kind === 'none' ? null : (
                    <Box flexShrink={0} marginLeft={2}>
                        <Button hotkey={row.button.value.hotkey} label={row.button.value.label} onPress={row.button.value.onPress} plain={true} />
                    </Box>
                )}
            </Box>
        ))}
    </Box>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { band, bandRows };
