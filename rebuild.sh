#!/bin/sh -e
echo Rebuilding Ixian Seeder...
echo Cleaning previous build
dotnet clean --configuration Release
echo Restoring packages
dotnet restore
echo Building Ixian Seeder
dotnet build --configuration Release
echo Done rebuilding Ixian Seeder