#!/usr/bin/env bash
cd ./src/IDP/
dotnet restore
dotnet publish -c release -r osx.10.11-x64
dotnet publish -c release -r linux-x64
dotnet publish -c release -r win8-x64
dotnet publish -c release -r win10-x64