import { Array, Effect, FileSystem, HashSet, Match, Option, String } from 'effect';
import type { Word } from 'sh-syntax';
import { covers, type Invocation, patternOf, roots, type Table, type Verdict } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Field = Invocation['deletes'][number];
type Origin = Extract<Field['pieces'][number], { readonly _tag: 'Unknown' }>['origin'];
type Denial = Extract<Verdict, { readonly deny: string }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALPHANUMERIC = /[\p{L}\p{N}]/u;
const WILDCARD = /[*?[]/u;
const REMOVERS = ['rm', 'rimraf', 'trash', 'unlink', 'shred'];
const WALKERS = ['find', 'gfind', 'fd', 'fdfind'];
const RECURSION = ['-r', '-R', '--recursive'];
const nameTests = ['-name', '-iname', '-path', '-ipath', '-regex', '-iregex', '-wholename', '-iwholename'];
const nullSeparators = ['-print0', '--print0', '-0'];
const eachPath = { instead: 'name each path you made, in full' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const render = (field: Field) => field.pieces.map((piece) => Match.valueTags(piece, { Text: ({ text }) => text, Wild: ({ text }) => text, Secret: () => '<secret>', Unknown: ({ origin }) => (origin._tag === 'Variable' ? `$${origin.name}` : '<expansion>') })).join('');
const literal = (word: Word) => Option.map(Option.all(word.parts.map((part) => (part.type === 'Lit' ? Option.some(part.value) : Option.none()))), Array.join(''));
const isMktemp = (origin: Origin): boolean =>
    Match.value(origin).pipe(
        Match.tag('Substitution', ({ programs }) => Array.isReadonlyArrayNonEmpty(programs) && programs.every((program) => program === 'mktemp')),
        Match.tag('Variable', ({ origins }) => HashSet.size(origins) > 0 && HashSet.every(origins, isMktemp)),
        Match.orElse(() => false),
    );
const isUnderMktemp = ({ pieces: [first, ...rest] }: Field): boolean => {
    const below = Option.exists(patternOf(rest), (path) => path === '' || (path.startsWith('/') && path.split('/').every((segment) => segment !== '..' && !(segment.startsWith('.') && WILDCARD.test(segment)))));
    return first?._tag === 'Unknown' && isMktemp(first.origin) && below;
};
const sweeps = (walker: Invocation) => (walker.runs('fd', 'fdfind') ? !Option.exists(walker.operand(), (pattern) => ALPHANUMERIC.test(pattern)) : !walker.has(...nameTests));
const refuse =
    (cwd: string, walker: Option.Option<Invocation>) =>
    (invocation: Invocation): readonly Denial[] => {
        const subject = invocation.label;
        const [recursive, removes, walks] = [invocation.has(...RECURSION) || invocation.runs('rimraf', 'trash'), invocation.runs(...REMOVERS), invocation.runs(...WALKERS)];
        const swept = Option.toArray(Option.filter(walker, sweeps)).flatMap((outer) => roots(outer, cwd).flatMap((root) => (covers(root, cwd) ? [{ subject: `${outer.label} ${root}`, deny: `runs ${subject} on everything under it`, values: { instead: 'add a pattern that selects only what to delete' } }] : [])));
        const args = Option.toArray(invocation.node).flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? cmd.args.slice(1) : []));
        const emptied = Array.getSomes(args.map(literal)).flatMap((text) => {
            const cut = text.lastIndexOf('/', text.search(WILDCARD)) + 1;
            const folder = `${cwd}/${text.slice(0, cut)}`.replaceAll(/\/(?:\.\/)+/gu, '/');
            return recursive && removes && WILDCARD.test(text) && !ALPHANUMERIC.test(text.slice(cut)) && covers(folder, cwd) ? [{ subject: `${subject} ${text}`, deny: `empties the working folder ${cwd}`, values: eachPath }] : [];
        });
        const judge = (field: Field): readonly Denial[] => {
            const unknown = field.pieces.flatMap((piece) => (piece._tag === 'Unknown' ? [piece.origin._tag] : []));
            const folder = Option.getOrElse(field.text, () => field.prefix.slice(0, field.prefix.lastIndexOf('/'))) || '/';
            const unnamed = Option.isSome(field.text) || !ALPHANUMERIC.test(render(field).slice(field.prefix.lastIndexOf('/') + 1));
            return Match.value({ fed: unknown.includes('Input'), temporary: isUnderMktemp(field), late: unknown.length > 0, working: unnamed && covers(folder, cwd) }).pipe(
                Match.when({ fed: true }, () => swept),
                Match.when({ temporary: true }, () => []),
                Match.when({ late: true }, () => (recursive && covers(folder, cwd) ? [{ subject: `${subject} ${render(field)}`, deny: 'deletes a path that expands only when it runs', values: { instead: 'pass the path literally' } }] : [])),
                Match.when({ working: true }, () => [
                    walks ? { subject: `${subject} ${folder}`, deny: 'deletes everything under the working folder', values: { instead: 'add a -name pattern that selects only what to delete' } } : { subject: `${subject} ${folder}`, deny: `${folder === cwd ? 'is' : 'holds'} the working folder`, values: eachPath },
                ]),
                Match.orElse(() => []),
            );
        };
        const own = walks && !sweeps(invocation) ? [] : [...emptied, ...invocation.deletes.flatMap(judge), ...(removes && Array.isReadonlyArrayEmpty(invocation.deletes) ? swept : [])];
        return [...own, ...invocation.inner.flatMap(refuse(cwd, walks ? Option.some(invocation) : walker))];
    };

// --- [RULES] ---------------------------------------------------------------------------

const removal: Table = {
    'removal.wide': { action: 'Shell', decide: (script, { cwd }) => Effect.map(script.invocations, (invocations) => invocations.flatMap(refuse(cwd, Option.none()))) },
    'removal.fed': {
        program: 'xargs',
        decide: Effect.fnUntraced(function* (xargs, { invocations, script, shell }) {
            const fs = yield* FileSystem.FileSystem;
            const remover = Option.filter(Array.head(xargs.inner), (inner) => inner.runs(...REMOVERS));
            const walker = Array.findFirst(invocations, (other) => other.runs(...WALKERS) && other.feeds(xargs) && Option.exists(other.stage, ({ index }) => index === 0) && other.inner.length === 0);
            const [nulls, plain] = [xargs.has('-0', '--null'), xargs.only('-0', '--null', '-r', '--no-run-if-empty')];
            const argv = Option.flatMap(remover, (inner) => inner.argv);
            const list = Option.filter(xargs.stdin._tag === 'File' ? xargs.stdin.path.text : Option.none(), () => plain);
            const listed = yield* list.pipe(
                Effect.fromOption,
                Effect.flatMap((file) => fs.readFileString(file)),
                Effect.option,
            );
            const paths = Option.filter(
                Option.map(listed, (text) => text.split(nulls ? '\0' : '\n').filter(String.isNonEmpty)),
                (items) => Array.isReadonlyArrayNonEmpty(items) && (nulls || items.every((item) => !/[\s'"\\]/u.test(item))),
            );
            const direct = yield* Effect.transposeOption(Option.map(Option.all([argv, paths]), ([words, items]) => shell.script(script.dialect)`${[...words, ...(words.includes('--') ? [] : ['--']), ...items]}`));
            const walk = Option.flatMap(walker, (found) => Option.all(found.words.filter((word) => !word.spells(...nullSeparators)).map(({ text }) => text)));
            const batch = Option.map(argv, (words) => (Option.exists(walker, (found) => found.runs('fd', 'fdfind')) ? Array.make('-X', ...words) : Array.make('-exec', ...words, '{}', '+')));
            const agrees = Option.exists(walker, (found) => found.has(...nullSeparators) === nulls) && Option.exists(xargs.stage, ({ count }) => count === 2);
            const joined = Option.filter(Option.all([walk, batch, Option.flatMap(walker, ({ span }) => span)]), () => plain && agrees);
            return Match.value({ direct: Option.getOrUndefined(Option.all([direct, xargs.node])), joined: Option.getOrUndefined(Option.all([joined, xargs.node])), recursive: Option.getOrUndefined(Option.filter(remover, (inner) => inner.has(...RECURSION))) }).pipe(
                Match.when({ direct: Match.defined }, ({ direct: [command, stmt] }): readonly Verdict[] => [{ edit: (editor) => [editor.command(stmt, command)] }]),
                Match.when({ joined: Match.defined }, ({ joined: [[words, exec, span], stmt] }): readonly Verdict[] => [{ edit: (editor) => [editor.replace(span, Array.appendAll(words, exec)), editor.remove(stmt)] }]),
                Match.when({ recursive: Match.defined }, ({ recursive: { label } }): readonly Verdict[] => [
                    { subject: `xargs ${label} -r`, deny: 'deletes paths it reads from standard input, which the hook cannot judge', values: { instead: 'run find <folder> -name <pattern> -exec rm -r {} +, or name each path to rm' } },
                ]),
                Match.orElse(() => []),
            );
        }),
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { removal };
