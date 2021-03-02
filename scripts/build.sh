#!/bin/bash

# Stop on any errors
set -e

dotnet restore ../
dotnet build ../PinaColada.sln --configuration Release --nologo --no-restore
dotnet test ../PinaColada.sln --configuration Release --nologo --no-build --no-restore --verbosity normal
