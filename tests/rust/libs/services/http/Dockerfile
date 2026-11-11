FROM golang:1.26-alpine AS build
RUN go install github.com/summerwind/h2spec/cmd/h2spec@70ac2294010887f48b18e2d64f5cccd48421fad1

FROM alpine:3.23
COPY --from=build /go/bin/h2spec /usr/local/bin/h2spec
ENTRYPOINT ["h2spec"]
