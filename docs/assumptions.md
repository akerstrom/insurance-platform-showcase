## Assumed Context

### Legacy Systems
We have 2 legacy systems:
1. An insurance mainframe system that manages customer policies and claims. Some of its data is exposed via REST APIs.
2. A relative old vehicle data system that provides vehicle information based on VIN numbers. Fetched via file transfer every night from 3rd party providers and stores data in a relational database.

### ThreadPilot 
The new core system **ThreadPilot** will over time replace the legacy insurance system. For now, it integrates with both legacy systems.

New core system **ThreadPilot** functions are:
- Manage customer insurance policies.
- Handle insurance claims.
- In the future integrate with 3rd party services for realtime vehicle data. Replacing Legacy Vehicle database.(2)

**Main vision:**
- Decouple frontends and new services from tightly coupled legacy systems.
- Aggregate and normalize data about customers, vehicles, and insurances into a coherent domain model.
- Act as the central place for applying business rules (pricing, validation, enrichment) instead of spreading them across many old systems.
- Enable a gradual migration away from legacy by hiding them behind stable, versioned APIs.