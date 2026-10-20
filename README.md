# Projeto Tech Challenge FIAP - CatalogAPI e CatalogWorker

Este projeto tem como objetivo executar processos relacionados ao cadastro de jogos e inclusão de jogos no catálogo do usuário.

## Estrutura do Projeto

- **Catalog.Api**: Aplicação ASP.NET Core WebAPI MVC responsável pelo CRUD de jogos e por iniciar o fluxo de compra. 
- **Catalog.Worker**: Aplicação Console que consome mensagens do tópico `payments-topic` e inclui jogo no catálogo do usuário após aprovação do pagamento.

## Tecnologias Utilizadas

- **.NET 10**: Framework principal
- **ASP.NET Core MVC**: WebAPI do Catalogo
- **Azure.Extensions.AspNetCore.Configuration.Secrets 1.5.2**: Biblioteca para obtenção de segredos do chave de cofres na Azure Key Vault
- **Azure.Identity 1.21.0**: Biblioteca para autenticar nos serviços da Azure usando conta de Serviço (Microsoft Entrada ID)
- **MassTransit.Azure.ServiceBus.Core 8.3.4**: Biblioteca para abstração de comunicação com Azure Bus Service
- **Elastic.Clients.Elasticsearch 8.19.27**: Persistência e consulta de jogos no Elastic search
- **MongoDB.Driver 3.11.0**: Persistência de jogos e catálogo
- **StackExchange.Redis 3.1.31**: Serviço de cache
