# Build and publish the MVC web application using the .NET 8 SDK.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY RaceDay.Mvc.csproj ./
RUN dotnet restore ./RaceDay.Mvc.csproj
COPY . ./
RUN dotnet publish ./RaceDay.Mvc.csproj -c Release -o /app/publish /p:UseAppHost=false

# Run using the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
