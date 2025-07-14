FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./src/ReportService.sln ./
COPY ./src/ReportService.Api/ReportService.Api.csproj ./ReportService.Api/
COPY ./src/ReportService.DataAccess/ReportService.DataAccess.csproj ./ReportService.DataAccess/
COPY ./src/ReportService.Tests/ReportService.Tests.csproj ./ReportService.Tests/
COPY ./src/ReportService.Logic/ReportService.Logic.csproj ./ReportService.Logic/
COPY ./src/Domain/Domain.csproj ./Domain/

RUN dotnet restore ReportService.sln

COPY ./src/ ./

WORKDIR /src/ReportService.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReportService.Api.dll"]
