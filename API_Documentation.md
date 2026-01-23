# eSports API Gateway - Client Documentation

## Overview

The eSports API Gateway provides pricing services for eSports betting scenarios. This RESTful API allows authenticated clients to submit pricing requests for various sports fixtures and receive probability calculations and true pricing information.

**Base URL (Production):** `https://pricing.sitechesports.com`

## Authentication

### API Key Authentication

All API requests require a valid API key for authentication. API keys should be included in the request header.

**Header Name:** `X-API-Key`

**Example:**
```
X-API-Key: ak_live_your_api_key_here
```

**Alternative (Less Secure):** API keys can also be passed as a query parameter `apikey`, but header-based authentication is strongly recommended.

### Obtaining API Keys

Contact your API provider to obtain production API keys. Each client receives:
- A unique API key
- Specific allowed origins for CORS
- Optional expiration date
- Usage description and limits

## CORS Policy

The API implements strict CORS policies based on your API key and environment:

### Development Environment
- Allowed origins: `http://localhost:3000`, `http://localhost:8080`, `http://127.0.0.1:3000`

### Production Environment  
- Origins are configured per client based on API key
- Only pre-approved domains can make requests
- Credentials are required for cross-origin requests

## Content Type

All requests and responses use `application/json` content type.

## API Endpoints

### POST / (Root Endpoint)

Calculates pricing and probability for eSports betting scenarios.

**Endpoint:** `POST /`

**Request Headers:**
```
Content-Type: application/json
X-API-Key: ak_live_your_api_key_here
```

**Request Body:**

```json
{
  "sportId": 1,
  "fixtureId": "fixture_123",
  "legs": [
    {
      // BetBuilderLeg object structure
      // Note: Specific properties depend on eSportsSimulator.DTO package
    }
  ]
}
```

**Request Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `sportId` | integer | Yes | Unique identifier for the sport |
| `fixtureId` | string | Yes | Unique identifier for the fixture/match |
| `legs` | array | Yes | Array of BetBuilderLeg objects representing bet components |

**Response:**

```json
{
  "probability": 0.75,
  "truePrice": 1.33
}
```

**Response Schema:**

| Field | Type | Description |
|-------|------|-------------|
| `probability` | number (double) | Calculated probability of the bet outcome (0.0 to 1.0) |
| `truePrice` | number (double) | True price/odds for the betting scenario |

## Data Models

### PriceRequest

The main request object for pricing calculations.

```typescript
interface PriceRequest {
  sportId: number;        // Sport identifier
  fixtureId: string;      // Fixture/match identifier  
  legs: BetBuilderLeg[];  // Array of bet components
}
```

### PriceResponse

The response object containing pricing information.

```typescript
interface PriceResponse {
  probability: number;    // Probability value (0.0 - 1.0)
  truePrice: number;      // Calculated true price/odds
}
```

### BetBuilderLeg

*Note: This object is defined in the external `eSportsSimulator.DTO` package. The specific structure should be obtained from the eSportsSimulator documentation or package source.*

## Example Usage

### cURL Example

```bash
curl -X POST https://pricing.sitechesports.com/ \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ak_live_your_api_key_here" \
  -d '{
    "sportId": 1,
    "fixtureId": "match_001",
    "legs": [
      {
        // BetBuilderLeg properties here
      }
    ]
  }'
```

### JavaScript/TypeScript Example

```typescript
const pricingRequest = {
  sportId: 1,
  fixtureId: "match_001",
  legs: [
    // BetBuilderLeg objects
  ]
};

const response = await fetch('https://pricing.sitechesports.com/', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-API-Key': 'ak_live_your_api_key_here'
  },
  body: JSON.stringify(pricingRequest)
});

if (!response.ok) {
  if (response.status === 401) {
    throw new Error('Invalid API key');
  }
  throw new Error(`HTTP error! status: ${response.status}`);
}

const result = await response.json();
console.log('Probability:', result.probability);
console.log('True Price:', result.truePrice);
```

### Python Example

```python
import requests
import json

url = "https://pricing.sitechesports.com/"
payload = {
    "sportId": 1,
    "fixtureId": "match_001",
    "legs": [
        # BetBuilderLeg objects
    ]
}

headers = {
    "Content-Type": "application/json",
    "X-API-Key": "ak_live_your_api_key_here"
}

response = requests.post(url, data=json.dumps(payload), headers=headers)

if response.status_code == 401:
    raise Exception("Invalid API key")
elif response.status_code != 200:
    raise Exception(f"HTTP error! status: {response.status_code}")

result = response.json()
print(f"Probability: {result['probability']}")
print(f"True Price: {result['truePrice']}")
```

### C# Example

```csharp
using System.Text.Json;
using System.Text;

var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-Key", "ak_live_your_api_key_here");

var request = new
{
    SportId = 1,
    FixtureId = "match_001",
    Legs = new[]
    {
        // BetBuilderLeg objects
    }
};

var json = JsonSerializer.Serialize(request);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("https://pricing.sitechesports.com/", content);

if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    throw new UnauthorizedAccessException("Invalid API key");
}

response.EnsureSuccessStatusCode();
var responseContent = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<PriceResponse>(responseContent);

Console.WriteLine($"Probability: {result.Probability}");
Console.WriteLine($"True Price: {result.TruePrice}");
```

## HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success - Request processed successfully |
| 400 | Bad Request - Invalid request format or missing required fields |
| 401 | Unauthorized - Invalid, missing, expired, or inactive API key |
| 500 | Internal Server Error - Server-side processing error |

## Error Handling

The API follows standard HTTP status codes. In case of errors, the response will include appropriate status codes and error messages.

### Common Error Scenarios

1. **Missing API Key**: Ensure the `X-API-Key` header is included in all requests
2. **Invalid API Key**: Verify your API key is correct and active
3. **Expired API Key**: Contact your provider if your key has expired
4. **CORS Issues**: Ensure your domain is in the allowed origins list for your API key
5. **Missing Required Fields**: Ensure all required fields (`sportId`, `fixtureId`, `legs`) are provided
6. **Invalid Data Types**: Verify that `sportId` is an integer and other fields match expected types
7. **Empty Legs Array**: The `legs` array should contain at least one BetBuilderLeg object

### Authentication Error Response

```json
{
  "error": "Unauthorized",
  "message": "API key is required"
}
```

Possible authentication error messages:
- "API key is required"
- "Invalid API key" 
- "API key is inactive"
- "API key has expired"

## Rate Limiting

Currently, no rate limiting is implemented. However, clients should implement reasonable request throttling to avoid overwhelming the service.

## API Versioning

The current API does not use explicit versioning. Any breaking changes will be communicated in advance.

## Support and Contact

For technical support or questions about the API:
- Check the OpenAPI documentation at: `https://pricing.sitechesports.com/scalar/v1`
- Review the API reference documentation available through the Scalar interface

## Additional Notes

1. **BetBuilderLeg Structure**: The specific structure of `BetBuilderLeg` objects depends on the `eSportsSimulator.DTO` package. Consult the eSportsSimulator documentation for detailed field specifications.

2. **Environment Configuration**: The API supports different environments:
   - Development: Uses Azure-hosted backend services
   - Production: Configured for production workloads

3. **Monitoring**: The API includes OpenTelemetry integration for monitoring and observability.

4. **Docker Support**: The API can be deployed using Docker containers with HTTPS support.

## Changelog

- **v1.0**: Initial API release with pricing calculation endpoint