# Mapeamento do Projeto: TelegramBotManager

## Visão Geral
O **TelegramBotManager** é uma aplicação backend construída em **C#** utilizando o modelo de **Azure Functions** (Isolated Worker Model). Seu principal objetivo é atuar como um gerenciador de mensagens e comandos originados do Telegram e outras integrações de compra/banco, com um forte enfoque em **Controle Financeiro** (Financial Control).

A arquitetura do projeto evoluiu para aplicar de forma rigorosa os princípios de **Clean Architecture**, **CQRS**, **SOLID** e **DDD (Domain-Driven Design)**.

- **Application**: Contém os *DTOs*, a lógica de orquestração via *Features* (Commands/Queries do MediatR) e o `TelegramMessageRouter`, que faz o roteamento das mensagens de entrada para seus respectivos comandos.
- **Domain**: Entidades centrais (`Transaction`, `Category`), interfaces de repositórios, **Value Objects** (`Money`, `CreditCard`) e **Domain Services** (ex: `TransactionInstallmentService`).
- **Infrastructure**: Configurações de acesso a dados via Supabase, conversores (*Mappers*) e a injeção de dependências (`DependencyInjection.cs`).
- **Functions**: Contém os *Triggers* (HTTP e Queue). O principal *QueueTrigger* (`FinancialControl.cs`) lê as mensagens da fila, aciona o roteador da aplicação e dispara o MediatR.
- **Common**: Exceções e Helpers compartilhados.
- **Configurations**: Modelos de configuração das chaves de acesso.

## Fluxo Principal e Azure Functions

Para evitar time-out no Webhook do Telegram, o projeto adota o uso intensivo de **Azure Storage Queues**:

1. **`Message` (HTTP Trigger)**: Recebe payloads de um app mobile (Push Notifications), converte em `PurchaseDto` e tenta o auto-processamento da transação bancária via MediatR (`BankTransactionAutoSaveCommand`). Se falhar ou for mensagem simples, joga na fila.
2. **`TelegramMessage` (HTTP Trigger)**: Webhook puro do Telegram. Valida o grupo autorizado e joga a mensagem na fila `financial-control-queue`.
3. **`FinancialControl` (Queue Trigger)**: Consome a fila, usa o `ITelegramMessageRouter` para descobrir qual Comando instanciar e finalmente usa o `_mediator.Send()` para disparar a *Feature* correta.
4. **`FinancialControlDailyReports` (Timer Trigger)**: Consolidação e envio de relatórios diários de gastos às 08:00 da manhã.

## Funcionalidades de Negócio (Features)

A aplicação conta com os seguintes casos de uso no padrão Command:
- Salvar notificações bancárias automaticamente (`BankTransactionAutoSave`).
- Criar e categorizar transações via bot manualmente (`FinanceControlCreateTransaction`, `FinanceControlCreateCategory`, `FinanceControlDefineCategory`).
- Excluir e listar exclusões (`FinanceControlDeleteTransaction`, `FinanceControlListTransactionsToDelete`).
- Gerenciar fechamentos (`CreditCardClosingDateManagement`, `CreditCardListClosingDate`).
- Edição de lançamentos do mês e Geração de relatórios diários.

## Detalhes Técnicos e Infraestrutura
1. **Value Objects**: Para proteger o domínio de primitivos nulos ou inválidos, são utilizados objetos como `Money` e `CreditCard`.
2. **Supabase**: Toda a camada de dados e banco PostgreSQL está hospedada no Supabase. O banco usa RLS desativado nas tabelas, com schemas definidos no `DDL.sql`. O projeto usa a lib oficial `supabase-csharp`.
3. **Segurança**: As Functions não contêm chaves expostas no código. Elas são injetadas no ambiente (no dev, via `local.settings.json` não-commitado). A segurança no consumo das APIs dá-se pela *Function Key* nativa do Azure.
