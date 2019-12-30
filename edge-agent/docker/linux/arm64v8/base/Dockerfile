# ARG base_tag=3.0.0-bionic-arm64v8
ARG num_procs=4

# FROM mcr.microsoft.com/dotnet/core/runtime:${base_tag}

ARG base_tag=3.1.0-alpine3.10-arm64v8
FROM mcr.microsoft.com/dotnet/core/runtime-deps:${base_tag}

# RUN mkdir -p ../publish

# RUN apt-get update && \
#     apt-get install -y libsnappy1v5 && \
#     rm -rf /var/lib/apt/lists/* && \
#     ln -s /lib/aarch64-linux-gnu/libdl.so.2 /usr/lib/aarch64-linux-gnu/libdl.so.2 && \
#     ln -s /lib/aarch64-linux-gnu/libdl.so.2 /lib/aarch64-linux-gnu/libdl.so && \
#     ln -s /lib/aarch64-linux-gnu/libdl.so.2 /usr/lib/aarch64-linux-gnu/libdl.so && \
#     ln -s /lib/aarch64-linux-gnu/libc.so.6 /usr/lib/aarch64-linux-gnu/libc.so.6

RUN apk update && \
    apk add --no-cache --update snappy libcap

COPY  librocksdb.so /usr/lib
