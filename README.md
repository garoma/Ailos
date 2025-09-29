🏦 API de Movimentação Bancária — Arquitetura Moderna e Escalável
Como parte de um teste técnico de recrutamento, desenvolvi uma API RESTful para movimentação bancária, aplicando os principais padrões e boas práticas da engenharia de software moderna. O projeto foi construído com foco em escalabilidade, manutenibilidade e testabilidade, utilizando uma arquitetura limpa (Clean Architecture) combinada com DDD (Domain-Driven Design), CQRS e injeção de dependência com MediatR.

🚀 Funcionalidades Implementadas
Consulta de saldo bancário

Movimentação financeira (débito/crédito) com validação de saldo

Controle de idempotência para evitar duplicidade de transações

🧱 Arquitetura e Organização do Projeto
Application Layer: responsável pelos casos de uso, com Handlers organizados por comandos e queries.

Domain Layer: contém entidades, validações e lógica de negócio.

Infrastructure Layer: abstrações de acesso a dados (CommandStore e QueryStore), utilizando SQLite.

Tests: testes unitários com Moq e FluentAssertions, garantindo a confiabilidade dos fluxos principais.

🧰 Tecnologias Utilizadas
.NET 8

MediatR (CQRS e comunicação desacoplada)

FluentValidation (validação das regras de negócio)

SQLite (banco de dados leve para testes rápidos)

Swagger/OpenAPI (documentação automática)

xUnit, Moq, FluentAssertions (testes automatizados)

SOLID + Clean Architecture + DDD (manutenção facilitada e código desacoplado)

🔐 Destaques Técnicos
Autenticação e autorização prontas para expansão com JWT

Separação clara entre camadas de domínio, aplicação e infraestrutura

Estratégia de Idempotência para garantir que a mesma requisição não cause efeitos duplicados

Uso de DTOs para isolar o modelo de domínio da API externa