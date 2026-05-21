# Mapeamento do Projeto: TelegramBotManager

## Visão Geral
O **TelegramBotManager** é uma aplicação backend construída em **C#** utilizando o modelo de **Azure Functions** (Isolated Worker Model). Seu principal objetivo é atuar como um gerenciador de mensagens e comandos originados do Telegram e outras integrações de compra/banco, com um forte enfoque em **Controle Financeiro** (Financial Control).

A arquitetura do projeto está estruturada seguindo os princípios de **Clean Architecture** (Arquitetura Limpa) e **CQRS** (utilizando a biblioteca MediatR), dividida de forma clara em camadas:
- **Application**: Contém a lógica de negócio, os DTOs, interfaces e os "Features" (Commands e Queries do MediatR).
- **Domain**: Entidades centrais do domínio e regras de negócio puras (ex: sob a pasta `FinancialControl`).
- **Infrastructure**: Configurações de acesso a dados (Supabase) e injeção de dependências externas.
- **Functions**: Contém os pontos de entrada das Azure Functions (Triggers HTTP e de Fila).
- **Common**: Exceções e Helpers compartilhados.
- **Configurations**: Modelos de configuração.
- **Middleware**: Interceptadores como tratamento global de exceções.

## Fluxo Principal e Azure Functions

A aplicação possui como principal fluxo o recebimento de mensagens e seu enfileiramento em **Azure Storage Queues**. O Telegram exige que respostas aos webhooks sejam rápidas, então o uso de filas garante que as mensagens não sejam perdidas ou repetidas por time-out.

As principais Functions identificadas são:

1. **`Message` (HTTP Trigger)**:
   - Recebe dados em formato HTTP, tenta convertê-los em um `PurchaseDto` (indicando que possivelmente processa payloads de uma automação bancária, como leitura de SMS ou notificações de apps).
   - Tenta executar o comando `BankTransactionAutoSaveCommand` para processamento automático.
   - Caso falhe ou não seja esse o cenário, enfileira a requisição no Azure Storage Queue (`FinancialMessageQueueClient`) para processamento normal.

2. **`TelegramMessage` (HTTP Trigger)**:
   - Atua como o Webhook do Telegram. 
   - Valida se a mensagem vem de grupos autorizados de Controle Financeiro (`FinancialControlOptions.AllowedGroup`).
   - Se for permitida, ela é inserida na fila `FinancialQueueClient`.

3. **`FinancialControl` (Queue Trigger)**:
   - Consome as mensagens da fila `financial-control-queue`.
   - Lê as atualizações do Telegram (`TelegramUpdateDto`) e dispara o comando `FinanceControlMessageReceivedCommand` através do MediatR.

4. **`FinancialControlDailyReports` (Timer Trigger)**:
   - Serviço que roda todos os dias às 08:00h da manhã, responsável pela consolidação e envio de relatórios diários das finanças no Telegram.

## Funcionalidades de Negócio (Features - MediatR)

A pasta `Application/Features` indica uma vasta gama de casos de uso suportados na área de Controle Financeiro:
- Salvar transações bancárias automaticamente (`BankTransactionAutoSave`)
- Criar e categorizar transações via bot (`FinanceControlCreateTransaction`, `FinanceControlCreateCategory`, `FinanceControlDefineCategory`)
- Excluir e listar transações para exclusão (`FinanceControlDeleteTransaction`, `FinanceControlListTransactionsToDelete`)
- Editar transações do mês vigente (`FinanceControlEditTransactionsOfMonth`)
- Gerenciar data de fechamento de cartões de crédito (`CreditCardClosingDateManagement`, `CreditCardListClosingDate`)
- Enviar relatórios diários de gastos e finanças (`FinancialControlDailyReports`)
- Orquestrar mensagens recebidas (`FinanceControlMessageReceived`)

## Detalhes de Implementação / FAQ

1. **Automação de Bancos (PurchaseDto)**: Um aplicativo lê as notificações push do smartphone e as envia via HTTP POST para a function `Message`, que então encaminha para auto-processamento das transações bancárias.
2. **Armazenamento / Banco de Dados**: A aplicação utiliza Supabase. O mapeamento relacional (DDL) e das entidades pode ser encontrado em `\TelegramBotManager\Domain\Data\DDL.sql`.
3. **Padrão CQRS / Roteamento de Mensagens**: Quando as mensagens chegam pelo bot e são consumidas pela Azure Function via Fila, o `FinanceControlMessageReceivedCommand` atua como um roteador e estruturador. Ele analisa a mensagem/comando e repassa a execução para os respectivos Handlers de Features.
4. **Segurança**: A segurança principal das Functions HTTP (ex: `Message` e `TelegramMessage`) se dá pela autenticação nativa da plataforma, exigindo a *Api Key* (Function Key) gerada no próprio portal do Azure.

---
*Este arquivo (gemini.md) serve como base de conhecimento atualizada sobre o projeto e os detalhes da sua infraestrutura.*
