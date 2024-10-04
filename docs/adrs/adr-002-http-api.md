# ADR 002: HTTP API vs gRPC

## Status

`Accepted`

## Context

We need to decide on the communication protocol for the application it's external API.

## Decision

We will implement an HTTP API (REST) for the application.

## Consequences

### Positive

- Broad client support
- Human-readable payloads
- Can leverage existing web infrastructure
- Familiar to most developers
- Easier testing and debugging

### Negative

- Less e