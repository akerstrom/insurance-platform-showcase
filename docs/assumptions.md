# Project Context

**ThreadPilot** is a new core insurance system. This project is the **integration layer** that connects ThreadPilot to legacy systems via modern REST APIs.

```
ThreadPilot (core system) → Integration Layer (this project) → Legacy Systems
```

## Legacy Systems

| System | Description | Integration |
|--------|-------------|-------------|
| Insurance Mainframe | Customer policies and claims | REST API (via Insurance Service) |
| Vehicle Database | Vehicle info by registration number | REST API (via Vehicle Service) |

## Integration Layer Services

| Service | Port | Role |
|---------|------|------|
| Vehicle Service | 5001 | Anti-corruption layer for legacy vehicle DB |
| Insurance Service | 5002 | Anti-corruption layer for legacy mainframe |
| Customer Service | 5003 | Orchestrator - aggregates and enriches data |
