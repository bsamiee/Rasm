# Review-scope exclusions. This file replaces Macroscope's built-in defaults, so the full roster lives here; tests/, docs/, and .claude/ stay reviewed.

# Lockfiles and generated manifests
**/pnpm-lock.yaml
**/uv.lock
**/packages.lock.json

# Recorded evidence and golden fixtures
**/goldens/**
**/*.verified.*
**/*.har

# Vendored native binaries and generated SDK bindings
**/vendor/runtimes/**
**/vendor/sdk/Lib3MF.cs

# Dependency, build, cache, and scratch output roots
**/node_modules/**
**/__pycache__/**
**/*.pyc
**/.venv/**
**/.artifacts/**
**/.cache/**
**/.cacheuv/**
**/bin/**
**/obj/**
**/dist/**
**/*.tsbuildinfo
**/scratch/**
**/.scratch/**
