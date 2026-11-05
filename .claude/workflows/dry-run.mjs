#!/usr/bin/env node
import { AsyncLocalStorage } from 'node:async_hooks'
import { createHash } from 'node:crypto'
import { mkdirSync, readdirSync, readFileSync, writeFileSync } from 'node:fs'
import { basename, dirname, join, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'

// --- [CONSTANTS] -----------------------------------------------------------------------

const HERE = dirname(fileURLToPath(import.meta.url))
const LOG = resolve(HERE, '../../.artifacts/logs/dry-run.log')
const SHOWN = 12
const DEAD = null
const IMPOSSIBLE = [
  [/helper agents?|sub-?agents?|agent tool|task tool|spawn (?:an?|fresh|helper)/i, 'asks for agents a workflow agent cannot start, helpers are agent() calls in the script'],
  [/(?<![Nn]o )app\.activate|(?<![Nn]o )computer-use/, 'takes focus from the owner'],
  [/watch-apps watch[^`\n]*\|/, 'filters its watch-apps watch Monitor'],
  [/undefined|\[object Object\]/, 'holds a value that never formed'],
]

// --- [CASES] ---------------------------------------------------------------------------

const BASE = 'abc1234'
const A = 'libs/typescript/x/a.ts'
const B = 'libs/typescript/x/b.ts'
const SPANNING = { files: [A, 'libs/typescript/y/b.ts'], change: 'moves one API' }
const row = (file, line) => ({ files: [file], rows: [{ line, text: `row ${line}` }] })
const pkg = (id, files, needs) => ({ id, title: id, section: id, files, needs })
const finding = (owner, paths = []) => ({ site: 'libs/typescript/a.ts:1', kind: 'improvement', fact: 'fact', owner, paths, joins: '' })
const FILES = {
  'map:libs/typescript/x': { groups: [[A], [B]], base: 'counts' },
  'read:1': { findings: [{ files: [A], change: 'local' }, SPANNING] },
  'read:2': { findings: [{ files: [B], change: 'local' }, SPANNING] },
  'edit:1': { files: [A], spanning: [] },
  'edit:2': { files: [B], spanning: [] },
  'challenge:*': { files: [A], spanning: [] },
  'verdict:*': { files: [A], spanning: [] },
  'read:relations:libs/typescript/x': { findings: [SPANNING, { files: [B, 'libs/typescript/z.ts'], change: 'folds one relation' }] },
  'edit:cross:1': { files: [A], spanning: [{ files: ['libs/typescript/w.ts'], change: 'reaches further' }] },
  'edit:cross:2': { files: [], spanning: [] },
}
const GRAPH = { packages: [pkg('a', [A], []), pkg('b', [B], ['a'])] }
const WRITER = { summary: 'wrote', files: [A] }
const CASES = {
  deepen: [
    { name: 'files', args: { language: 'typescript', path: 'libs/typescript/x', base: BASE }, fixtures: FILES },
    {
      name: 'plan',
      args: { language: 'typescript', mode: 'reduce', path: 'libs/typescript/x', base: BASE, plan: 'plans/PLAN.md', inputs: ['docs/x'], oracle: 'node proof.ts', exclude: ['libs/typescript/x/z'] },
      fixtures: {
        'map:plan': { units: [{ files: [A, B], reads: [], slices: [row(A, 1), row(A, 2), row(B, 3)] }, { files: ['libs/typescript/x/c.ts', A], reads: [], slices: [row('libs/typescript/x/c.ts', 4), row(A, 5)] }], base: 'counts' },
        'edit:*': { files: [A], spanning: [] },
        'challenge:*': { files: [A], spanning: [] },
        'verdict:*': { files: [A], spanning: [] },
      },
    },
    { name: 'build', args: { language: 'typescript', mode: 'build', key: 'x', base: BASE }, fixtures: { architect: GRAPH, 'write:a': WRITER, 'write:b': WRITER } },
    {
      name: 'build-corrected',
      args: { language: 'typescript', mode: 'build', key: 'x', base: BASE },
      fixtures: { architect: { packages: [pkg('a', [A], ['b']), pkg('b', [A], ['a', 'ghost'])] }, 'architect:corrected': GRAPH, 'write:a': WRITER, 'write:b': WRITER },
    },
    {
      name: 'build-refused',
      args: { language: 'typescript', mode: 'build', key: 'x', base: BASE },
      expect: 'package graph',
      fixtures: { architect: { packages: [pkg('a', [A], ['b']), pkg('b', [B], ['a'])] }, 'architect:corrected': { packages: [pkg('a', [A], ['b']), pkg('b', [B], ['a'])] } },
    },
    {
      name: 'finished',
      args: { language: 'typescript', mode: 'build', key: 'x', base: BASE, finished: 'finished.js' },
      fixtures: { 'workflow:finished.js': { 'brief:code': 'wrote', 'brief:members': 'wrote', plan: 'titles', architect: GRAPH }, 'write:a': WRITER, 'write:b': WRITER },
    },
  ],
  tune: [
    {
      name: 'exclude',
      args: { language: 'typescript', base: BASE, exclude: ['tools/ast-grep/rules/python', 'tsconfig.base.json'] },
      fixtures: {
        'map:units': { units: [{ name: 'one', paths: ['libs/typescript/a.ts'] }, { name: 'two', paths: ['libs/typescript/b.ts'] }] },
        'read:one': { findings: [finding('biome', ['biome.json']), finding('typescript-coding'), finding('ast-grep', ['tools/ast-grep/rules/typescript/x.yml']), finding('ast-grep', ['tools/ast-grep/rules/python/held.yml'])] },
        'read:two': { findings: [finding('tsc', ['tsconfig.base.json']), finding('typescript-coding-effect')] },
        'write:check:biome': { files: ['biome.json'], renamed: [], removed: [], changed: [], reroutes: [finding('ast-grep', ['tools/ast-grep/rules/sql/y.yml']), finding('tsc', ['tsconfig.base.json'])], report: 'wrote' },
      },
    },
  ],
  guidance: [
    {
      name: 'hostile-select',
      args: { areas: ['libs', 'apps'] },
      fixtures: { select: { write: ['libs/typescript', 'libs/typescript/data', 'apps/web', '.', '../outside', 'tools/held'], remove: ['CLAUDE.md', 'apps/old/CLAUDE.md', 'libs/typescript/CLAUDE.md', 'apps/x.ts'] } },
    },
  ],
  'harness-depth': [
    {
      name: 'indesign',
      args: { key: 'indesign', name: 'InDesign', apps: ['2026', '2027'], skills: ['watch-apps'], products: ['docs/research/x'], base: BASE },
      fixtures: {
        'mine:model': { needs: ['need one', 'need two'], findings: ['model finding'] },
        'edit:slice': { slices: [{ files: ['tools/harnesses/indesign/host.ts'], findings: ['f'] }, { files: ['tools/harnesses/indesign/host.ts'], findings: ['g'] }, { files: ['tools/harnesses/run.ts'], findings: ['h'] }, { files: ['libs/typescript/outside.ts'], findings: ['outside the kit'] }], refused: [] },
      },
    },
  ],
  'extension-research': [
    { name: 'note', args: { key: 'color' } },
    { name: 'plan', args: { key: 'color', mode: 'plan' } },
    { name: 'plan-no-evidence', args: { key: 'data', mode: 'plan' } },
  ],
  languages: [{ name: 'rows' }],
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SCHEMA]
const kindOf = (value) => (Array.isArray(value) ? 'array' : value === null ? 'null' : typeof value)
const fits = (schema, value, at = '$') => {
  const kind = kindOf(value)
  const type = schema.type === 'integer' && Number.isInteger(value) ? 'number' : schema.type
  if (type && type !== kind) return [`${at}: ${kind}, schema says ${schema.type}`]
  if (schema.enum && !schema.enum.includes(value)) return [`${at}: ${value}, schema allows ${schema.enum.join(', ')}`]
  if (kind === 'array') return value.flatMap((each, index) => (schema.items ? fits(schema.items, each, `${at}[${index}]`) : []))
  if (kind !== 'object') return []
  const absent = (schema.required ?? []).filter((key) => !(key in value)).map((key) => `${at}.${key}: required, absent`)
  const held = schema.properties ?? {}
  return [...absent, ...Object.keys(held).flatMap((key) => (key in value ? fits(held[key], value[key], `${at}.${key}`) : []))]
}
const satisfiable = (schema, at = '$') => [
  ...(schema.enum?.length === 0 ? [`${at}: empty enum`] : []),
  ...(schema.required ?? []).filter((key) => !(key in (schema.properties ?? {}))).map((key) => `${at}.${key}: required, not a property`),
  ...Object.keys(schema.properties ?? {}).flatMap((key) => satisfiable(schema.properties[key], `${at}.${key}`)),
  ...(schema.items ? satisfiable(schema.items, `${at}[]`) : []),
]
const built = (schema, path) => {
  if (schema.enum) return schema.enum[0]
  if (schema.type === 'object') return Object.fromEntries(Object.keys(schema.properties ?? {}).map((key) => [key, built(schema.properties[key], `${path}.${key}`)]))
  if (schema.type === 'array') return schema.items ? [built(schema.items, `${path}[0]`)] : []
  if (schema.type === 'boolean') return false
  if (schema.type === 'number' || schema.type === 'integer') return 0
  return path
}

// --- [SCRIPT]
const parts = (text) => {
  const end = text.match(/^}\n/m)
  if (!text.startsWith('export const meta') || !end) return { defects: ['meta is not the first statement, closed by a } at line start'], phases: [], body: text }
  const meta = new Function(`"use strict"; return (${text.slice(text.indexOf('{'), end.index + 1)})`)()
  const absent = ['name', 'description'].filter((field) => !meta[field])
  return {
    defects: [...absent.map((field) => `meta lacks ${field}`), ...(Buffer.byteLength(text) > 524288 ? [`${Buffer.byteLength(text)} bytes, the loader skips scripts above 524288`] : [])],
    phases: (meta.phases ?? []).map((each) => each.title),
    body: text.slice(end.index + 2),
  }
}
const loaded = (path) => parts(readFileSync(path, 'utf8'))
const located = (name) => {
  const sibling = join(HERE, `${name}.js`)
  return name.includes('/') || name.endsWith('.js') ? resolve(name) : sibling
}
const barred = (name) => () => {
  throw new Error(`${name} throws in a workflow script, resume would break`)
}
class Clock extends Date {
  static now = barred('Date.now()')
  constructor(...given) {
    if (given.length === 0) barred('new Date()')()
    super(...given)
  }
}
const Random = Object.create(Math, { random: { value: barred('Math.random()') } })

// --- [STUBS]
const pass = async (script, args, fixtures, { fate, delay = () => 0 } = {}) => {
  const started = []
  const keys = []
  const logged = []
  const swallowed = []
  const defects = []
  const titles = new Set()
  const answered = new Set()
  const lanes = new AsyncLocalStorage()
  const trunk = { prev: '' }
  const mixed = (...values) => createHash('sha256').update(JSON.stringify(values)).digest('hex').slice(0, 16)
  const fork = (kind) => {
    const lane = lanes.getStore() ?? trunk
    const from = lane.prev
    const slots = []
    lane.prev = mixed(from, kind, 'fork')
    return {
      inSlot: (index, thunk) => {
        slots[index] = { prev: mixed(from, kind, index) }
        return lanes.run(slots[index], thunk)
      },
      join: () => {
        lane.prev = mixed(lane.prev, kind, slots.map((slot) => slot?.prev ?? ''))
      },
    }
  }
  const settled = (kind, results) => results.map((result, index) => {
    if (result.status === 'fulfilled') return result.value
    swallowed.push(`${kind}[${index}] failed: ${result.reason?.message ?? result.reason}`)
    return DEAD
  })
  const agent = async (prompt, opts = {}) => {
    const label = opts.label ?? '(no label)'
    const lane = lanes.getStore() ?? trunk
    lane.prev = mixed(prompt, opts, lane.prev)
    started.push(label)
    keys.push(`${label} ${lane.prev}`)
    if (opts.phase) titles.add(opts.phase)
    IMPOSSIBLE.filter(([pattern]) => pattern.test(prompt)).forEach(([, why]) => defects.push(`${label}: prompt ${why}`))
    if (opts.schema) satisfiable(opts.schema).forEach((why) => defects.push(`${label}: schema ${why}`))
    if (opts.schema?.type !== undefined && opts.schema.type !== 'object') defects.push(`${label}: schema root is ${opts.schema.type}`)
    await new Promise((done) => setTimeout(done, delay(label)))
    if (fate?.label === label && fate.kind === 'null') return DEAD
    if (fate?.label === label && fate.kind === 'throw') throw new Error(`stub ${label} threw`)
    const key = Object.hasOwn(fixtures, label) ? label : Object.keys(fixtures).find((each) => each.endsWith('*') && label.startsWith(each.slice(0, -1)))
    if (key === undefined) return opts.schema ? built(opts.schema, label) : `text from ${label}`
    answered.add(key)
    if (!opts.schema) {
      defects.push(`${label}: a fixture answers it and it has no schema, so the runtime returns text`)
      return `text from ${label}`
    }
    fits(opts.schema, fixtures[key]).forEach((why) => defects.push(`${label}: fixture refused: ${why}`))
    return fixtures[key]
  }
  const parallel = async (thunks) => {
    const slots = fork('parallel')
    await null
    const results = await Promise.allSettled(thunks.map((thunk, index) => slots.inSlot(index, async () => thunk())))
    slots.join()
    return settled('parallel', results)
  }
  const pipeline = async (items, ...stages) => {
    const slots = fork('pipeline')
    await null
    const results = await Promise.allSettled(items.map((item, index) => slots.inSlot(index, () => stages.reduce((prior, stage) => prior.then((value) => (value === DEAD ? value : stage(value, item, index))), Promise.resolve(item)))))
    slots.join()
    return settled('pipeline', results)
  }
  const execute = (path, given, workflow) => {
    const { body } = loaded(path)
    const names = ['args', 'agent', 'parallel', 'pipeline', 'workflow', 'log', 'phase', 'budget', 'Date', 'Math', 'process', 'require', 'Buffer']
    return new Function(...names, `return (async () => {\n${body}\n})()`)(given, agent, parallel, pipeline, workflow, (text) => logged.push(String(text)), (title) => titles.add(title), { total: null, spent: () => 0, remaining: () => Infinity }, Clock, Random)
  }
  const nested = (name) => {
    throw new Error(`workflow(${JSON.stringify(name)}) cannot nest a second level`)
  }
  const workflow = async (ref, given) => {
    const name = ref?.scriptPath ?? ref
    const key = `workflow:${name}`
    if (Object.hasOwn(fixtures, key)) {
      answered.add(key)
      return fixtures[key]
    }
    return execute(located(name), given, nested)
  }
  const unhandled = []
  const onRejection = (reason) => unhandled.push(String(reason?.message ?? reason))
  process.on('unhandledRejection', onRejection)
  const deadlock = new Promise((done) => setTimeout(() => done({ deadlock: true }), 8000).unref())
  const outcome = await Promise.race([execute(script, args, workflow).then((value) => ({ value }), (error) => ({ error })), deadlock])
  await new Promise((done) => setTimeout(done, 0))
  process.off('unhandledRejection', onRejection)
  return { started, keys, logged, swallowed, defects, titles, answered, outcome, unhandled }
}

// --- [REPORT]
const shown = (lines) => [...lines.slice(0, SHOWN).map((line) => `  ${line.slice(0, 240)}`), ...(lines.length > SHOWN ? [`  and ${lines.length - SHOWN} more`] : [])]
const section = (title, lines) => (lines.length > 0 ? [title, ...shown(lines)] : [])
const raw = (error) => /^stub .* threw$/.test(error.message) || error instanceof TypeError || error instanceof RangeError || error instanceof ReferenceError
const hash = (label) => [...label].reduce((sum, char) => sum + char.charCodeAt(0), 0) % 8
const judged = async (script, { name, args, fixtures = {}, expect }) => {
  const { defects, phases } = loaded(script)
  const clean = await pass(script, args, fixtures)
  const header = `${basename(script, '.js')}/${name}: agents ${clean.started.length}, labels ${new Set(clean.started).size}`
  if (expect !== undefined) {
    const error = clean.outcome.error
    const verdict = error?.message?.includes(expect) && !raw(error) ? [`  aborts as it states: ${error.message.slice(0, 200)}`] : [`DEFECT expected an abort naming "${expect}", got ${error?.message ?? 'a finished run'}`]
    return { lines: [header, ...verdict], failed: verdict[0].startsWith('DEFECT') }
  }
  const later = await pass(script, args, fixtures, { delay: hash })
  const earlier = await pass(script, args, fixtures, { delay: (label) => 7 - hash(label) })
  const held = new Set(later.keys)
  const moved = earlier.keys.filter((key) => !held.has(key)).map((key) => key.split(' ')[0])
  const labels = [...new Set(clean.started)]
  const faults = await labels.flatMap((label) => ['null', 'throw'].map((kind) => ({ label, kind }))).reduce((prior, fate) => prior.then(async (held) => [...held, { key: `${fate.label} ${fate.kind}`, label: fate.label, ...(await pass(script, args, fixtures, { fate })) }]), Promise.resolve([]))
  const finished = faults.filter((fault) => fault.outcome.value !== undefined || (!fault.outcome.error && !fault.outcome.deadlock))
  const recorded = (fault) => fault.logged.some((line) => line.includes(fault.label))
  const reported = (fault) => JSON.stringify(fault.outcome.value ?? '').includes(fault.label)
  const crashes = faults.filter((fault) => fault.outcome.error && raw(fault.outcome.error)).map((fault) => `${fault.key}: ${fault.outcome.error.message}`)
  const stated = faults.filter((fault) => fault.outcome.error && !raw(fault.outcome.error)).map((fault) => `${fault.key}: ${fault.outcome.error.message}`)
  const deadlocked = faults.filter((fault) => fault.outcome.deadlock).map((fault) => fault.key)
  const silent = finished.filter((fault) => !recorded(fault) && !reported(fault)).map((fault) => fault.key)
  const unreported = finished.filter((fault) => recorded(fault) && !reported(fault)).map((fault) => fault.key)
  const unhandled = faults.filter((fault) => fault.unhandled.length > 0).map((fault) => `${fault.key}: ${fault.unhandled[0]}`)
  const duplicates = [...Map.groupBy(clean.started, String)].filter(([, held]) => held.length > 1).map(([label]) => label)
  const unused = Object.keys(fixtures).filter((key) => !clean.answered.has(key))
  const untitled = [...clean.titles.difference(new Set(phases))]
  const found = [
    ...defects,
    ...clean.defects,
    ...clean.swallowed.map((line) => `script threw inside a slot: ${line}`),
    ...clean.unhandled.map((line) => `unhandled rejection: ${line}`),
    ...(clean.outcome.deadlock ? [`deadlock after ${clean.started.at(-1)}`] : []),
    ...(clean.outcome.error ? [`clean pass threw: ${clean.outcome.error.message}`] : []),
    ...duplicates.map((label) => `duplicate label ${label}`),
    ...unused.map((key) => `fixture ${key} answers no call`),
    ...untitled.map((title) => `phase ${title} is absent from meta.phases`),
    ...(moved.length > 0 ? [`resume misses ${moved.length}/${earlier.keys.length} under another finish order, first ${moved[0]}`] : []),
  ]
  const lines = [
    header,
    ...found.map((line) => `DEFECT ${line.slice(0, 240)}`),
    `  loss of one agent: ${crashes.length} crash, ${deadlocked.length} deadlock, ${silent.length} silent, ${unreported.length} logged but absent from the result, ${unhandled.length} unhandled, ${stated.length} abort as stated`,
    ...section('CRASH', crashes),
    ...section('DEADLOCK', deadlocked),
    ...section('SILENT', silent),
    ...section('UNREPORTED', unreported),
    ...section('UNHANDLED', unhandled),
    ...section('ABORT AS STATED', stated),
  ]
  return { lines, failed: found.length + crashes.length + deadlocked.length + silent.length + unreported.length + unhandled.length > 0 }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const [, , target, argsPath, fixturesPath] = process.argv
const scripts = target ? [target] : readdirSync(HERE).filter((file) => file.endsWith('.js')).map((file) => basename(file, '.js'))
const adhoc = argsPath ? [{ name: 'given', args: argsPath === '-' ? undefined : (await import(resolve(argsPath), { with: { type: 'json' } })).default, fixtures: fixturesPath ? (await import(resolve(fixturesPath))).default : {} }] : null
process.removeAllListeners('warning')
const runs = scripts.flatMap((name) => (adhoc ?? CASES[basename(located(name), '.js')] ?? [{ name: 'no args' }]).map((each) => [located(name), each]))
const results = await runs.reduce((prior, [script, each]) => prior.then(async (held) => [...held, await judged(script, each)]), Promise.resolve([]))
const report = results.flatMap((result) => result.lines).join('\n')
mkdirSync(dirname(LOG), { recursive: true })
writeFileSync(LOG, `${report}\n`)
console.log(report)
process.exitCode = results.some((result) => result.failed) ? 1 : 0
