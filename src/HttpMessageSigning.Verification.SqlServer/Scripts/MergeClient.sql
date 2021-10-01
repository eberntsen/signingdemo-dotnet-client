﻿MERGE INTO {ClientsTableName} AS TARGET
    USING (VALUES (@Id, @Name, @NonceLifetime, @ClockSkew, @SigType, @SigParameter, @SigHashAlgorithm, @IsSigParameterEncrypted, @RequestTargetEscaping, @V)) AS SOURCE ([Id], [Name], [NonceLifetime], [ClockSkew], [SigType], [SigParameter], [SigHashAlgorithm], [IsSigParameterEncrypted], [RequestTargetEscaping], [V])
    ON SOURCE.[Id] = TARGET.[Id]
    WHEN MATCHED THEN UPDATE SET [Name] = SOURCE.[Name], [NonceLifetime] = SOURCE.[NonceLifetime], [ClockSkew] = SOURCE.[ClockSkew], [SigType] = SOURCE.[SigType], [SigParameter] = SOURCE.[SigParameter], [SigHashAlgorithm] = SOURCE.[SigHashAlgorithm], [IsSigParameterEncrypted] = SOURCE.[IsSigParameterEncrypted], [RequestTargetEscaping] = SOURCE.[RequestTargetEscaping], [V] = SOURCE.[V]
    WHEN NOT MATCHED THEN INSERT ([Id], [Name], [NonceLifetime], [ClockSkew], [SigType], [SigParameter], [SigHashAlgorithm], [IsSigParameterEncrypted], [RequestTargetEscaping], [V]) VALUES (SOURCE.[Id], SOURCE.[Name], SOURCE.[NonceLifetime], SOURCE.[ClockSkew], SOURCE.[SigType], SOURCE.[SigParameter], SOURCE.[SigHashAlgorithm], SOURCE.[IsSigParameterEncrypted], SOURCE.[RequestTargetEscaping], SOURCE.[V]);
