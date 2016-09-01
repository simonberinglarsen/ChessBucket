FROM microsoft/dotnet:latest

ENV ASPNETCORE_ENVIRONMENT Development
EXPOSE 5000

COPY . /app
WORKDIR /app
RUN ["dotnet", "restore"]
ENTRYPOINT dotnet run


