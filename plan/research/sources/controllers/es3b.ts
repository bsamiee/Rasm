const f = (a: number[]): string => { const out: string[] = []; for (const x of a) { out.push(`v${x}`); } const {y, ...rest} = {y: 1, z: 2}; return out.join(",") + rest.z + y; };
export default f;
