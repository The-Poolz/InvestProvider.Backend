# InvestProvider.Backend

Lambda handler of API4.InvestProvider endpoints.

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
Handler receive `documentId` of project, `if` of phase and white list data.
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
