// Path operations over a file path or a command word, the segments and the prefix a policy reads

// --- [OPERATIONS] ----------------------------------------------------------------------

// The last path segment, the command word of a leaf and the file name of a path
const basename = (text: string): string => text.split('/').at(-1) ?? '';

// The dot-prefixed last dot segment of the file name, the whole name dot-prefixed when it holds no dot
const extension = (path: string): string => `.${basename(path).split('.').at(-1) ?? ''}`;

// Whether the path sits under the directory, relative or absolute
const under = (path: string, directory: string): boolean => path.includes(`/${directory}/`) || path.startsWith(`${directory}/`);

// The path without the working directory prefix, a path outside it stays as given
const relative = (cwd: string, path: string): string => (path.startsWith(`${cwd}/`) && path.slice(cwd.length + 1)) || path;

// --- [EXPORTS] -------------------------------------------------------------------------

export { basename, extension, relative, under };
