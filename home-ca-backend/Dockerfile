FROM mcr.microsoft.com/dotnet/sdk:8.0 AS buildEnv
WORKDIR /App    

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=buildEnv /App/out .
COPY ./entrypoint.sh .

CMD ["/bin/sh",  "-c", "/App/entrypoint.sh"]