# [API_CATALOGUE] react-error-boundary

`react-error-boundary` provides a React class error boundary with three rendering strategies — static `fallback`, `FallbackComponent`, or `fallbackRender` — plus a `useErrorBoundary` hook for imperatively triggering or resetting the boundary from within a descendant, and a `withErrorBoundary` HOC. The `ErrorBoundaryContext` exposes boundary state to deep consumers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-error-boundary`
- package: `react-error-boundary`
- module: `react-error-boundary` (types: `dist/react-error-boundary.d.ts`)
- asset: runtime library
- rail: ui

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: error boundary types
- rail: ui

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                                                 |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `ErrorBoundaryProps`              | union type    | `WithFallback \| WithComponent \| WithRender`                                          |
|  [02]   | `ErrorBoundaryPropsWithFallback`  | type alias    | static `ReactNode` fallback strategy                                                   |
|  [03]   | `ErrorBoundaryPropsWithComponent` | type alias    | `FallbackComponent: ComponentType<FallbackProps>` strategy                             |
|  [04]   | `ErrorBoundaryPropsWithRender`    | type alias    | `fallbackRender: (props: FallbackProps) => ReactNode` strategy                         |
|  [05]   | `FallbackProps`                   | type alias    | `{ error: unknown; resetErrorBoundary: (...args) => void }`                            |
|  [06]   | `ErrorBoundaryContextType`        | type alias    | `{ didCatch: boolean; error: unknown \| null; resetErrorBoundary: (...args) => void }` |
|  [07]   | `OnErrorCallback`                 | type alias    | `(error: unknown, info: ErrorInfo) => void`                                            |
|  [08]   | `UseErrorBoundaryApi`             | type alias    | `{ error, resetBoundary, showBoundary }`                                               |

[PUBLIC_TYPE_SCOPE]: shared props base
- rail: ui

`ErrorBoundarySharedProps` (the intersection base for all three strategy types) carries `onError?: OnErrorCallback`, `onReset?: (...args) => void`, `resetKeys?: unknown[]`, and `children?: ReactNode`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ErrorBoundary component
- rail: ui

```ts
// dist/react-error-boundary.d.ts
export declare class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState>

// Three mutually exclusive prop strategies:
type ErrorBoundaryPropsWithFallback = ErrorBoundarySharedProps & {
  fallback: ReactNode;
  FallbackComponent?: never;
  fallbackRender?: never;
};

type ErrorBoundaryPropsWithComponent = ErrorBoundarySharedProps & {
  fallback?: never;
  FallbackComponent: ComponentType<FallbackProps>;
  fallbackRender?: never;
};

type ErrorBoundaryPropsWithRender = ErrorBoundarySharedProps & {
  fallback?: never;
  FallbackComponent?: never;
  fallbackRender: (props: FallbackProps) => ReactNode;
};

type FallbackProps = {
  error: unknown;
  resetErrorBoundary: (...args: unknown[]) => void;
};
```

[ENTRYPOINT_SCOPE]: hooks and HOC
- rail: ui

```ts
export declare function useErrorBoundary(): {
  error: unknown | null;
  resetBoundary: () => void;
  showBoundary: (error: unknown) => void;
}

export declare function withErrorBoundary<
  Type extends ComponentClass<unknown>,
  Props extends object,
>(
  Component: ComponentType<Props>,
  errorBoundaryProps: ErrorBoundaryProps,
): ForwardRefExoticComponent<PropsWithoutRef<Props> & RefAttributes<InstanceType<Type>>>
```

[ENTRYPOINT_SCOPE]: context and utility
- rail: ui

```ts
export declare const ErrorBoundaryContext: Context<ErrorBoundaryContextType | null>

export declare function getErrorMessage(thrown: unknown): string | undefined
```

## [04]-[IMPLEMENTATION_LAW]

[UI_TOPOLOGY]:
- exactly one of `fallback`, `FallbackComponent`, or `fallbackRender` is required; the union uses `never` to enforce mutual exclusion at the type level
- `FallbackComponent` receives `FallbackProps` and renders while an error is caught; `fallbackRender` is the inline function form
- `resetKeys` is a dependency list; when any key changes, the boundary resets automatically
- `onReset` fires after a boundary reset via `resetErrorBoundary` or `resetBoundary`
- `onError` fires synchronously in `componentDidCatch`; use for logging, not recovery

[LOCAL_ADMISSION]:
- `showBoundary(error)` from `useErrorBoundary` triggers the boundary from an async context (e.g. inside an Effect or event handler) where React's error propagation does not reach the boundary.
- `resetBoundary()` from `useErrorBoundary` resets the nearest ancestor `ErrorBoundary` without requiring `resetKeys`.
- `ErrorBoundaryContext` exposes `didCatch` and `error` for deep consumers that need to render differently inside a caught boundary without being the fallback component.

[RAIL_LAW]:
- Package: `react-error-boundary`
- Owns: React error boundary rendering strategies and imperative boundary control
- Accept: `FallbackComponent` or `fallbackRender` for typed fallback props; `showBoundary` for async error capture
- Reject: bare `class extends Component` error boundaries in new code; `fallback` when the fallback needs to read `error` or call `resetErrorBoundary`
