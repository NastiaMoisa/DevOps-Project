FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY . .

EXPOSE 5149

ENTRYPOINT ["dotnet", "DevOpsProject.HiveMind.API.dll"]
