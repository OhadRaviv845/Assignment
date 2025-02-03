# API Testing Guide - Credito Score Engine

This guide outlines the testing procedures for the scoring engine API implementation. It covers the main test cases demonstrating core functionality and features.

## Setup

Start the application using Docker:
```bash
docker-compose up --build
```

The API will be available at `http://localhost:5000`

## Test Cases

### 1. Socioeconomic Scoring Service
Test the main scoring service with sample data:

```bash
curl -X POST http://localhost:5000/api/scoring/execute \
-H "Content-Type: application/json" \
-d '{
    "serviceName": "SocioeconomicScoring",
    "payload": {
        "customerId": "123",
        "income": 50000
    }
}'
```

Expected Response:
```json
{
    "executionId": "guid-value",
    "result": {
        "score": 750,
        "risk_level": "low",
        "recommendation": "approved"
    },
    "status": "Success",
    "executionTime": "00:00:00.5xxxxx"
}
```

### 2. Bank Statement Analysis
Test the bank statement analysis service:

```bash
curl -X POST http://localhost:5000/api/scoring/execute \
-H "Content-Type: application/json" \
-d '{
    "serviceName": "BankStatementAnalyzer",
    "payload": {
        "accountId": "ACC123",
        "months": 3
    }
}'
```

### 3. Error Handling
Verify error handling with an invalid service request:

```bash
curl -X POST http://localhost:5000/api/scoring/execute \
-H "Content-Type: application/json" \
-d '{
    "serviceName": "NonExistentService",
    "payload": {
        "test": "data"
    }
}'
```

### 4. Rate Limiting
Test the rate limiting implementation (30 requests per minute):

```bash
# Execute multiple requests to test rate limiting
for i in {1..35}; do
    curl -X POST http://localhost:5000/api/scoring/execute \
    -H "Content-Type: application/json" \
    -d '{
        "serviceName": "SocioeconomicScoring",
        "payload": {
            "customerId": "123",
            "income": 50000
        }
    }'
    echo "\n"
    sleep 0.1
done
```

After exceeding the rate limit, the API returns:
```json
{
    "statusCode": 429,
    "message": "API calls quota exceeded!"
}
```

## Monitoring

### Log Inspection
Monitor application behavior through logs:
```bash
docker-compose logs -f
```

The logs provide information about:
- Request processing
- Retry attempts
- Rate limiting events
- Circuit breaker status
- Error occurrences

### Swagger Documentation
Interactive API documentation is available at:
- URL: http://localhost:5000/swagger
- Provides testing interface
- Shows available endpoints and models

## Troubleshooting

Common issues and resolutions:
1. Connection failures:
   - Verify Docker container status
   - Ensure port 5000 availability

2. Rate limit exceeded:
   - Wait 60 seconds for limit reset
   - Reduce request frequency

3. Request failures:
   - Verify JSON payload format
   - Check Content-Type header

## Health Monitoring

Verify API health status:
```bash
curl http://localhost:5000/health
```

Expected Response:
```json
{
    "status": "Healthy"
}
```

This implementation demonstrates the required features including retry mechanism, circuit breaker pattern, and rate limiting. Each component can be tested using the procedures outlined above.
