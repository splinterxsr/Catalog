# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Instala a ferramenta do EF Core dentro do ambiente de SDK (Onde o comando funciona)
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ["Catalog.Api/Catalog.Api.csproj", "Catalog.Api/"]
RUN dotnet restore "./Catalog.Api/Catalog.Api.csproj"
COPY . .
WORKDIR "/src/Catalog.Api"
RUN dotnet build "./Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Gera o bundle de migração usando o SDK na pasta de publicação final
# Nota: Garanta que o pacote Microsoft.EntityFrameworkCore.Design esteja instalado no seu Catalog.Api.csproj
WORKDIR /app/publish
RUN dotnet ef migrations bundle --project /src/Catalog.Api/Catalog.Api.csproj --self-contained -r linux-x64 -o /app/publish/migrate
RUN chmod +x migrate

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]