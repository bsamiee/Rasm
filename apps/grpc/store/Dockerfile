FROM rust AS build
WORKDIR /src
COPY rust-toolchain.toml ./
RUN rustup toolchain install
COPY --parents Cargo.toml Cargo.lock .cargo/config.toml apps/grpc apps/mcp libs/rust tests/rust tools/dylint ./
RUN --mount=type=cache,target=/usr/local/cargo/registry --mount=type=cache,target=/usr/local/cargo/git --mount=type=cache,target=.cache/rust --mount=type=cache,target=.artifacts/rust cargo build --locked --profile dist --package rasm-store && cp .artifacts/rust/dist/rasm-store /
FROM distroless
COPY --from=build /rasm-store /
ENTRYPOINT ["/rasm-store"]
