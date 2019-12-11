ARG base_tag=2.0-linux-arm64v8
FROM microsoft/azureiotedge-hub-base:${base_tag}

# Add an unprivileged user account for running Edge Hub
ARG EDGEHUBUSER_ID=1000
#RUN useradd -ms /bin/bash -u ${EDGEHUBUSER_ID} edgehubuser
RUN adduser -Ds /bin/sh -u ${EDGEHUBUSER_ID} edgehubuser
ENV EdgeHubUser=edgehubuser

ARG EXE_DIR=.

WORKDIR /app

COPY $EXE_DIR/ ./

# Expose MQTT, AMQP and HTTPS ports
EXPOSE 8883/tcp
EXPOSE 5671/tcp
EXPOSE 443/tcp

RUN setcap 'cap_net_bind_service=+ep' /usr/share/dotnet/dotnet
# && \
#   chmod -R 777 ./Microsoft.Azure.Devices.Edge.Hub.Service
    # chown edgehubuser ./Microsoft.Azure.Devices.Edge.Hub.Service

USER edgehubuser
ENV OptimizeForPerformance false
ENV MqttEventsProcessorThreadCount 1
CMD echo "$(date --utc +"%Y-%m-%d %H:%M:%S %:z") Starting Edge Hub" && \
    # ls -l ./Microsoft.Azure.Devices.Edge.Hub.Service && \
    exec /usr/bin/dotnet Microsoft.Azure.Devices.Edge.Hub.Service.dll
    #exec /bin/sh ./Microsoft.Azure.Devices.Edge.Hub.Service
    # ./Microsoft.Azure.Devices.Edge.Hub.Service
