# Projetos RabbitMQ com .NET 10

Este projeto tem como objetivo executar processos relacionados ao cadastro de jogos e inclusão de jogos no catálogo do usuário.

## Estrutura do Projeto

- **Catalog.Api**: Aplicação ASP.NET Core WebAPI MVC responsável pelo CRUD de jogos e por iniciar o fluxo de compra. 
- **Catalog.Worker**: Aplicação Console que consome mensagens da fila payments-queue e inclui jogo no catálogo do usuário após aprovação do pagamento.
- **docker-compose.yml**: Configuração do RabbitMQ, Postgres, MySQL, UsersAPI e PaymentsWorker.

## Pré-requisitos

- .NET 10 SDK
- Docker e Docker Compose

## Como executar

### 1. Iniciar o RabbitMQ
```bash
# Docker-compose
docker-compose up -d
```

### 2. Executar o Catalog.Api (Terminal 1)
```bash
cd Catalog.Api
dotnet run
```

### 3. Usar a aplicação
1. Abra o navegador em: http://localhost:5025 (ou a porta indicada)
2. Execute os procedimentos informados no arquivo Catalog.Api.http da solução.

### Acesso ao RabbitMQ Management
- URL: http://localhost:15672
- Usuário: guest
- Senha: guest

### Acesso ao Banco de dados Postgres
- Porta: 5432
- Usuário: catalog_user
- Senha: catalog_pass
- Base: catalog_db

## Tecnologias Utilizadas

- **.NET 10**: Framework principal
- **ASP.NET Core MVC**: WebAPI do Catalogo
- **MassTransit.RabbitMQ 8.5.10**: Biblioteca para comunicação com RabbitMQ
- **Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2**: Persistência de jogos e catálogo
- **RabbitMQ 3.11**: Message broker
