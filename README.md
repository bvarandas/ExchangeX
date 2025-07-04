Projeto White Label para Matching de Ordens de operações para Ativos, derivativos e cripto moedas.

A arquitetura foi projetada para efetuar um número alto de ordens (dependendo do tamanho do Pod) o mais rápido possível, gerando mensagens de cotação/negociação em tempo real para nosos futuros parceiros em tempo real.
A idéia é termos vários parceiros conectados em nosso sistema de matching, consumindo nossas informações de cotação/negociação, e ao mesmo tempo, fornecermos o serviço de homebroker para clientes diretos.

Para isso, temos duas partes: 
 - O Módulo Exchange, onde fornecemos o homebroker e o Risk trader
 - O módulo Matching, onde as ordens serão casadas e as mensagens serão criadas.

A parte da exchange é a parte onde temos:

 - Front end White Label com o Sistema de homebroker - Clientes podem enviar as ordens
 - Front end White Label com o Sistema de Risk manager - Os Contratantes tem acesso ao patrimonio em tempo real dos cliente
 - Backend com Engine de Inconsistencias - mensagens de negócios com erro, erros sistemicos e 
 - Backend com Engine de BFF para usar como Gateway, Identity Provider, Orquestração das mensagens de input e output.
 - Backend com Engine de Audit - Infrmações que de logs para auditoria
 - Backend com Engine de Limite - Onde é verificado em tempo real o Limite necessário para o cliente poder efetuar a operação de compra
 - Backend com Engine de Posição - Onde é calculado a posição do cliente em tempo real para enviar para o homebroker
 - Backend com Engine de OrderRouter - Onde é feita a orquestração das  ordens que chegam do matching e que chegam do homebroker.

![_exchange_gif](https://github.com/user-attachments/assets/ec3d8a2b-8414-402b-b46b-3644c46ff43c)
Modelo da arquitetura C4

A Parte de Matching é parte onde temos:
 - Backend com Engine Order - monitoramento de Ordens Stop e gerenciamento de status de ordens
 - Backend Order Entry Engine - entrada externa de ordens via FIX - Usado para receber ordens de homebrokers e plataformas de negociação externos.
 - Backend MarketData Engine - Saída de sinal FIX Cotação para parceiros externos est]ao nesse serviço
 - Backend DropCopy Engine - Saída de sinal FIX Negocios para parceiros externos estõa nesse serviço
 - Backend Trade Engine - As negociações efetuadas são salvas nesse microserviço intermediário
 - Backend Matching - Serviço onde o casamento de ordens é executado. Depois do matching executado, o serviço cria três Objetos:
   - TradeEngine: trade Executado que vai para o serviço de Trade Engine
   - MarketDdataEngine: Quando um trade é executado o objeto de marketData é criado para enviar para serviço de marketData para o mesmo criar o objeto FIX para envio para parceiros externos
   - OrderEntryEngine: Quando um trande é executado o objeto de OrderEngine é criado para enviar para o serviço OrderEngine para o mesmo alterar o status da ordem.
- Dentro da Matcing Engine, temos ainda um Book  de ordens, que é o  book  de ordens abertas aguardando para serem executadas.
     

![_Matching](https://github.com/user-attachments/assets/a3636932-1095-4d0e-88c3-3dcff0de76d2)
Modelo da arquitetura C4

Arquiteturas e Patterns 
 - Event Driven Architecture/ Message Driven Architecture
 - Outbox para garantia de entrega de mensagens
 - Tratamento Acid de mensagens entre microserviços - Atomicidade, Consistencia, Isolamento e durabilidade.
 - Paralelismo e alta disponibilidade de serviços.
 - CQRS - separando conexão de gravação e seleção...ainda mais pq seleção consulta mais memória do que banco de dados. Pois os dados são atualiados via eventos.
 - Singleton
 - Flyweight (OU algo parecido)
 - Command
 - Mediator
 - Observer
 - Ioc - Inversão de controle (Removido)
 - Injeção de depedencia
 - Unit of Work (Removido)
 - Event Sourcing (removido)
 - Alguns conceitos de S.O.L.I.D. tbm foram usados.










