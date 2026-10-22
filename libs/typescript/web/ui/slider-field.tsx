import { Label, Slider, SliderOutput, type SliderProps, SliderThumb, SliderTrack } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const SliderField = ({ label, ...props }: Omit<SliderProps<number>, 'children'> & { readonly label: string }) => (
    <Slider {...props}>
        <Label>{label}</Label>
        <SliderOutput />
        <SliderTrack>
            <SliderThumb />
        </SliderTrack>
    </Slider>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { SliderField };
