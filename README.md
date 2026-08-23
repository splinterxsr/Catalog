# Projeto Tech Challenge FIAP - CatalogAPI e CatalogWorker

Este projeto tem como objetivo executar processos relacionados ao cadastro de jogos e inclusão de jogos no catálogo do usuário.

## Estrutura do Projeto

- **Catalog.Api**: Aplicação ASP.NET Core WebAPI MVC responsável pelo CRUD de jogos e por iniciar o fluxo de compra. 
- **Catalog.Worker**: Aplicação Console que consome mensagens da fila payments-queue e inclui jogo no catálogo do usuário após aprovação do pagamento.

### Acesso ao Banco de dados MongoDB
- Porta: 27017
- Usuário: root
- Senha: r00tp@ss
- Base: fcg

## Tecnologias Utilizadas

- **.NET 10**: Framework principal
- **ASP.NET Core MVC**: WebAPI do Catalogo
- **MassTransit.RabbitMQ 8.5.10**: Biblioteca para comunicação com RabbitMQ
- **MongoDB.Driver 3.11.0**: Persistência de jogos e catálogo
- **StackExchange.Redis 3.1.31**: Serviço de cache
