FROM chef AS toolchain
WORKDIR /src
ENV CARGO_TARGET_DIR=/src/target CARGO_BUILD_BUILD_DIR=/src/target
COPY rust-toolchain.toml ./
RUN rustup toolchain install
FROM toolchain AS planner
COPY --parents Cargo.toml Cargo.lock .cargo/config.toml apps/grpc apps/mcp libs/rust tests/rust tools/dylint ./
RUN cargo chef prepare --recipe-path recipe.json
FROM toolchain AS build
COPY --from=planner /src/recipe.json ./
RUN cargo chef cook --profile dist --package rasm-store --recipe-path recipe.json
COPY --parents Cargo.toml Cargo.lock .cargo/config.toml apps/grpc apps/mcp libs/rust tests/rust tools/dylint ./
RUN cargo build --locked --profile dist --package rasm-store
FROM distroless
COPY --from=build /src/target/dist/rasm-store /
ENTRYPOINT ["/rasm-store"]
