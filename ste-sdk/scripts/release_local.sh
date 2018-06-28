#!/bin/sh

rm -rf ~/.nuget/packages/improbable.stesdk/
msbuild /p:Configuration=Release
nuget pack stesdk/STESDK.csproj -Prop Configuration=Release
nuget add Improbable.STESDK.*.nupkg -source ~/.nuget/packages/