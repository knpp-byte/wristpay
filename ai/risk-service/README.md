# Risk / AI service (deferred)

Not started. Per the agreed roadmap, Python + FastAPI is used only once the core payment platform
(`backend/`) is working and there's a real need for fraud scoring, anomaly detection, or voice/NLP
("Send Ravi ₹500").

AI must never authorize a transaction independently — it can only feed a risk signal into the
backend's `PaymentService` risk engine.
