# ThreadPilot Architecture
ThreadPilot is the new core insurance platform. In this implementation it is composed for now of three microservices:

Customer Service – the customer-centric API and orchestrator, storing additional customer data and aggregating information from other domains.

Insurance Service – an anti-corruption layer around the legacy insurance mainframe, exposing policies in a modern, normalized format.

Vehicle Service – an anti-corruption layer around the legacy vehicle database, providing clean vehicle data based on registration number.

External channels integrate with the Customer Service, which serves as the public face of ThreadPilot. The Customer Service then calls the Insurance and Vehicle services to retrieve and enrich data, so consumers no longer need to talk to legacy systems directly.


## Legacy Systems

### Insurance Mainframe
- Get insurance policy by ID
  - GET /legacy/insurances/{id}

### Vehicle database

## Services

### Customer Service
- Get customer by ID
  - GET /customers/{id}

### Vehical Service
Endpoints:
- Get vehicle by VIN
  - GET /vehicles/{vin}

### Insurance Service
- Get insurance policy by ID
  - GET /insurances/{id}

### Logging/Tracking Service
- Log events
  - POST /logs
