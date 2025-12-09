## As simple data ingestion service implementation.
The data flow is something like this: Sensors -> Edge device (on-prem) -> /api/ingest -> WorkerService --> storage.

Main compoenents:
1. HTTP ingress: a minimal api /api/ingest - decouple http threads from message processing + custom validation etc.
2. A background worker (QueueProcessor) - drains the channel (or azure 
3. A processing pipeline (SensorDataPipeline) -  business logic lives here to proceess/massage data before persistence.
