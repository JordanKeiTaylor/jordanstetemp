#!/bin/sh

spatial worker_package unpack-to worker_sdk csharp worker-sdk/
spatial schema generate --language csharp --output standard-library-generated-schema/