#!/bin/sh
spatial worker_package unpack-to worker_sdk csharp build/worker-sdk/
cp build/worker-sdk/Improbable.WorkerSdkCsharp.dll worker-sdk/
spatial schema generate --language csharp --output build/generated-code/csharp/
cp -R build/generated-code/csharp/* main/
sed -i -e 's/public partial struct/public struct/g' main/improbable/*