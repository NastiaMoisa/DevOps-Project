FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore DevOpsProject.sln
RUN dotnet publish DevOpsProject.HiveMind.API/DevOpsProject.HiveMind.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 5149

ENTRYPOINT ["dotnet", "DevOpsProject.HiveMind.API.dll"]
