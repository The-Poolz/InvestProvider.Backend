# API4.InvestedProvider

## Overview

**API URL**: [https://data.poolz.finance/graphql](https://data.poolz.finance/graphql)

## Authentication

Each endpoint require a user token (generated via `generateTokenFromSignature`) in the Authorization header:
```
Authorization: USER_TOKEN
```

## Queries

### Get User's Upcoming Allocations

```graphql
query MyUpcomingAllocation($projectIDs: [String!]!) {
  myUpcomingAllocation(projectIDs: $projectIDs) {
    ProjectId
    PoolzBackId
    Amount
  }
}
```

**Parameters**:
- `projectIDs` (required) - array of project IDs

### Get Current Allocation for Project

```graphql
query MyAllocation($projectId: String!) {
  myAllocation(projectId: $projectId) {
    Amount
    StartTime
    EndTime
  }
}
```

**Parameters**:
- `projectId` (required) - project ID

### Generate Investment Signature

```graphql
query GenerateMyInvestSignature($projectId: String!, $weiAmount: String!) {
  generateMyInvestSignature(projectId: $projectId, weiAmount: $weiAmount) {
    Signature
    ValidUntil
    PoolzBackId
  }
}
```

**Parameters**:
- `projectId` (required) - project ID
- `weiAmount` (required) - investment amount in wei

### Get Allocations (Admin)

```graphql
query AdminGetAllocation($projectId: String!, $phaseId: String!) {
  adminGetAllocation(projectId: $projectId, phaseId: $phaseId) {
    UserAddress
    Amount
  }
}
```

**Parameters**:
- `projectId` (required) - project ID
- `phaseId` (required) - allocation phase ID

## Mutations

### Create PoolzBack ID (Admin)

```graphql
mutation AdminCreatePoolzBackId($projectId: String!, $poolzBackId: Int!) {
  adminCreatePoolzBackId(projectId: $projectId, poolzBackId: $poolzBackId) {
    ProjectId
    PoolzBackId
  }
}
```

**Parameters**:
- `projectId` (required) - project ID
- `poolzBackId` (required) - numeric PoolzBack ID

### Write Allocations (Admin)

```graphql
mutation AdminWriteAllocation($input: AdminWriteAllocation!) {
  adminWriteAllocation(input: $input)
}
```

**Input Example**:
```graphql
{
  "input": {
    "ProjectId": "proj_123",
    "PhaseId": "phase_1",
    "Users": [
      {
        "UserAddress": "0x123...",
        "Amount": 1000.0
      },
      {
        "UserAddress": "0x456...",
        "Amount": 500.0
      }
    ]
  }
}
```

## Error Codes

Code | Error Type | Description
----|-----------|-----------
401 | UNAUTHORIZED | Authentication required
