# [API_CATALOGUE] clsx

`clsx` joins conditional class-name inputs into one space-delimited `string`. The variadic `clsx(...inputs: ClassValue[])` accepts strings, numbers, bigints, nested arrays, and plain objects whose truthy keys contribute their key name, while `null`, `undefined`, `false`, `0`, and empty strings drop out. `ClassValue` is the recursive input union, `ClassDictionary` is the `Record<string, any>` object form, and `ClassArray` is the nested-array form. The package ships dual module formats: the `.` entry resolves the full recursive joiner, and the `./lite` entry resolves a string-only joiner that skips array and object recursion. The CommonJS build exposes the joiner as both the callable default export and a named `clsx` member; the ESM build exposes `clsx` as a named export and the default export.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `clsx`
- package: `clsx`
- module: `dist/clsx.mjs`
- main: `dist/clsx.js`
- types: `clsx.d.ts`
- entry `.`: full recursive joiner
- entry `./lite`: string-only joiner
- asset: dual CJS/ESM utility

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: class-name input union
- entry: `clsx`

| [INDEX] | [SYMBOL]          | [DEFINITION]                                                                                  | [ROLE]                   |
| :-----: | :---------------- | :-------------------------------------------------------------------------------------------- | :----------------------- |
|   [1]   | `ClassValue`      | `ClassArray \| ClassDictionary \| string \| number \| bigint \| null \| boolean \| undefined` | recursive accepted input |
|   [2]   | `ClassDictionary` | `Record<string, any>`                                                                         | truthy-key object form   |
|   [3]   | `ClassArray`      | `ClassValue[]`                                                                                | nested array form        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: joiner functions
- return: `string`

| [INDEX] | [SURFACE]                               | [ENTRY]     | [CAPABILITY]                                   |
| :-----: | :-------------------------------------- | :---------- | :--------------------------------------------- |
|   [1]   | `clsx(...inputs: ClassValue[]): string` | `clsx`      | recursive join over strings, arrays, objects   |
|   [2]   | `export default clsx`                   | `clsx`      | ESM default joiner binding                     |
|   [3]   | `module.exports.clsx`                   | `clsx`      | CJS named joiner alongside callable export     |
|   [4]   | `clsx(...inputs): string`               | `clsx/lite` | string-only join, no array or object recursion |

## [4]-[IMPLEMENTATION_LAW]

[JOIN_TOPOLOGY]:
- input axes: `string`, `number`, `bigint`, `ClassArray`, `ClassDictionary`, `null`, `boolean`, `undefined`
- string and number inputs append directly; bigint flows through the same numeric arm
- array inputs recurse element-by-element and skip falsy elements
- object inputs append each key whose value is truthy
- falsy inputs (`null`, `undefined`, `false`, `0`, `""`) contribute nothing
- separator: a single space inserted only between non-empty fragments
- output: one flat space-delimited `string`, empty when every input is falsy

[ENTRY_TOPOLOGY]:
- `.` entry: full recursive joiner over the complete `ClassValue` union
- `./lite` entry: accepts only `string` inputs, ignores arrays and objects, smaller footprint
- CJS shape: callable default export plus a `.clsx` named property bound to the same function
- ESM shape: `clsx` named export plus default export bound to the same function

[LOCAL_ADMISSION]:
- Compose conditional class names through `clsx` instead of manual string concatenation or ternary chains.
- Pass object maps for toggle-style classes and arrays for grouped fragments.
- Reserve the `./lite` entry for hot paths that only join string fragments.

[RAIL_LAW]:
- Package: `clsx`
- Owns: conditional class-name string construction
- Accept: `ClassValue` inputs across string, numeric, array, and object axes
- Reject: hand-rolled class-name concatenation or ternary class strings
