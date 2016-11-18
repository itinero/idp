cd ./src/IDP/
dotnet restore
dotnet build
dotnet publish -c release

cd bin\release\netcoreapp1.1\win10-x64\publish