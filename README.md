# InvestProvider.Backend

Backend services implemented as AWS Lambda functions for the
`API4.InvestProvider` endpoints. The project targets **.NET 8** and can be
built and tested locally using the standard `dotnet` CLI tools.

## Handlers

- `GenerateSignature` – generate an EIP712 signature for investors.
- `AdminWriteAllocation` – write whitelist information into DynamoDB.
- `AdminGetAllocation` – retrieve whitelist data for project phases.
- `AdminCreatePoolzBackId` – register a pool identifier in DynamoDB.
- `MyAllocation` – return allocation details for the current phase.
- `MyUpcomingAllocation` – return upcoming allocations for multiple projects.

## Environment variables
| Name | Description | Default |
| --- | --- | --- |
| `STRAPI_GRAPHQL_URL` | URL of Strapi GraphQL endpoint | - |
| `SECRET_ID_OF_SIGN_ACCOUNT` | Identifier of the signer's secret | - |
| `SECRET_KEY_OF_SIGN_ACCOUNT` | Key of the signer's secret | - |
| `PRIVATE_KEY_OF_LOCAL_SIGN_ACCOUNT` | Private key for development signer | - |
| `MIN_INVEST_AMOUNT` | Minimum allowed invest amount | `1` |
| `MAX_PARALLEL` | Max parallel operations for admin handlers | `10` |
| `BATCH_SIZE` | Batch write size for admin allocations | `25` |

## GenerateSignature handler
This handler generate EIP712 signature for user that will use in [InvestProvider contract](https://github.com/The-Poolz/LockDealNFT.InvestProvider), in [invest(uint256 poolId, uint256 amount, uint256 validUntil, bytes calldata signature)](https://github.com/The-Poolz/LockDealNFT.InvestProvider?tab=readme-ov-file#investing-with-erc20-tokens) function.

#### Request
Handler receive `documentId` of project, user address for which signature will generated, and invest amount in wei.
```json
{
  "ProjectId": "xyz",
  "UserAddress": "0x0000000000000000000000000000000000000000",
  "WeiAmount": "1000000000000000000"
}
```

#### Response
Handler return generated signature, signature valid until (finish time of current phase in project) in unix-timestamp, and poolId related to project.
```json
{
  "Signature": "0x",
  "ValidUntil": 123456789,
  "PoolzBackId": 123
}
```

**Handler workflow:**
- Receive project information from Strapi and throw exception if active phase in project not found.
- Receive from DynamoDb poolId of project and throw if poolId not registered.
- Receive token decimals related for poolId and convert `WeiAmount` into amount taking into account decimals. Throw exception if invested amount less then allowed amount for investing.
- Generate signature depending on phase type
  - If phase is whitelist. Loading whitelist info of user from DB, and generate signature. Throw exception if user not in white list. Throw exception if user tried to invest amount more than him allowed to investing.
  - If phase is FCFS. Just generate signature. Throw exception if user already invested. Throw exception if user tried to invest more then allowed in current phase.

## AdminWriteAllocation handler
This handler using to write whitelist information into DB.

#### Request
Handler receive `documentId` of project, `id` of phase and white list data.
```json
{
  "ProjectId": "xyz",
  "PhaseId": "123",
  "Users": [
    {
      "UserAddress": "0x0000000000000000000000000000000000000000",
      "Amount": 1.0
    },
    ...
  ]
}
```

#### Response
Handler just returns count of saved item into DB.
```json
{
  "Saved": 123
}
```


**Handler workflow:**
- Receive from DynamoDb poolId of project and throw if poolId not registered.
- Receive project information from Strapi. Throw exception if phase in project not found, if phase is not whitelist, or if phase already finished.
- Batch save into DynamoDb whitelist data.

## AdminGetAllocation handler
This handler retrieves the stored white-list information for all whitelist phases of a project.

#### Request
Handler receive `documentId` of project.
```json
{
  "ProjectId": "xyz"
}
```

#### Response
Handler returns an array of phase allocations.
```json
[
  {
    "PhaseId": "123",
    "WhiteList": [
      { "UserAddress": "0x0000000000000000000000000000000000000000", "Amount": 1.0 }
    ]
  }
]
```

**Handler workflow:**
- Load project phases from Strapi.
- Query DynamoDB for whitelist entries of each phase in parallel.
- Return consolidated results.

## AdminCreatePoolzBackId handler
This handler registers mapping between a Strapi project and a `PoolzBackId` in DynamoDB.

#### Request
Handler receive `documentId` of project, pool identifier and chain id.
```json
{
  "ProjectId": "xyz",
  "PoolzBackId": 123,
  "ChainId": 1
}
```

#### Response
Handler returns the saved mapping.
```json
{
  "ProjectId": "xyz",
  "PoolzBackId": 123
}
```

**Handler workflow:**
- Validate that the pool exists on-chain and that the providers match.
- Save mapping information in DynamoDB.
- Return the created project mapping.

## MyAllocation handler
This handler returns allocation details for the caller in the current phase.

#### Request
Handler receive `documentId` of project and user address.
```json
{
  "ProjectId": "xyz",
  "UserAddress": "0x0000000000000000000000000000000000000000"
}
```

#### Response
Handler returns the whitelist amount and related information.
```json
{
  "Amount": 1.0,
  "StartTime": "2024-06-01T00:00:00Z",
  "EndTime": "2024-06-02T00:00:00Z",
  "PoolzBackId": 123
}
```

**Handler workflow:**
- Validate that the current phase is whitelist and the user is listed.
- Return the allocation amount, phase dates and pool identifier.

## MyUpcomingAllocation handler
This handler lists how much a user will be able to invest in upcoming phases across multiple projects.

#### Request
Handler receive list of `projectIDs` and user address.
```json
{
  "ProjectIDs": ["p1", "p2"],
  "UserAddress": "0x0000000000000000000000000000000000000000"
}
```

#### Response
Handler returns upcoming allocation amounts by project.
```json
[
  {
    "ProjectId": "p1",
    "PoolzBackId": 11,
    "Amount": 0.5
  }
]
```

**Handler workflow:**
- Query DynamoDB for all whitelist entries of the user.
- Load project information for the requested IDs.
- Aggregate amounts per project.

## Getting Started

Clone the repository and restore dependencies using the `.NET` CLI:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build InvestProvider.Backend.sln
```

Run the unit tests:

```bash
dotnet test InvestProvider.Backend.sln
```

## License

This project is licensed under the [MIT License](LICENSE).
