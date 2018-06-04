FROM ubuntu:18.04

# RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg && mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg && sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-bionic-prod artful main" > /etc/apt/sources.list.d/dotnetdev.list' 
RUN apt-get update && \
    apt-get install -y curl ca-certificates apt-transport-https gnupg2 build-essential && \
    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb https://download.mono-project.com/repo/ubuntu preview-bionic main" | tee /etc/apt/sources.list.d/mono-official-preview.list && \
    apt-get update && \
    apt-get install -y nuget msbuild mono-devel openjdk-8-jdk cmake

# Install jsonnet
RUN  curl -L -o jsonnet.tar.gz https://github.com/google/jsonnet/archive/v0.10.0.tar.gz && ls -lh && \
     tar xf jsonnet.tar.gz && \
     cd jsonnet-0.10.0 && \
     make -j 4 && \
     chmod +x jsonnet && \
     mv jsonnet /usr/bin && \
     cd .. && \
     rm -rf jsonnet-0.10.0

ENV JAVA_HOME /usr/lib/jvm/java-8-openjdk-amd64/
WORKDIR /home/project
