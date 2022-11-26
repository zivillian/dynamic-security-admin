FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY Dynsec ./src/Dynsec
COPY DynsecAdmin ./src/DynsecAdmin
ARG TARGETARCH
RUN if [ "$TARGETARCH" = "amd64" ]; then \
    RID=linux-musl-x64 ; \
    elif [ "$TARGETARCH" = "arm64" ]; then \
    RID=linux-musl-arm64 ; \
    elif [ "$TARGETARCH" = "arm" ]; then \
    RID=linux-musl-arm ; \
    fi \
    && dotnet publish -c Release -o out -r $RID --sc src/DynsecAdmin/DynsecAdmin.csproj

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine
EXPOSE 80
WORKDIR /app
COPY --from=build-env /app/out .

ENV MQTTSERVER=localhost

ENTRYPOINT ["/app/dynsecadmin"]