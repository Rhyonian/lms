# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# adjust the csproj name if different
COPY apps/api/*.csproj apps/api/
RUN dotnet restore apps/api/*.csproj
COPY apps/api/ apps/api/
WORKDIR /src/apps/api
RUN dotnet publish -c Release -o /out

# production runtime (small)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# adjust DLL name if different
ENTRYPOINT ["dotnet","Lms.Api.dll"]

# development runtime (SDK inside for CLI)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","Lms.Api.dll"]
