Projeto White Label para Matching de Ordens de operações para Ativos, derivativos e cripto moedas.

A arquitetura foi planejada para casar um número alto de ordens e gerar mensagens de negociação em tempo real.

Temos duas partes. 

A parte da exchange é a parte onde temos:

 - Front end White Label com o Sistema de homebroker - Clientes podem enviar as ordens
 - Front end White Label com o Sistema de Risk manager - Os Contratantes tem acesso ao patrimonio em tempo real dos cliente
 - Backend com Engine de Inconsistencias - mensagens de negócios com erro, erros sistemicos e 
 - Backend com Engine de BFF para usar como Gateway, Orquestração das mensagens de input e output.
 - Backend com Engine de Audit - Infrmações que de logs para auditoria
 - Backend com Engine de Limite - Onde é verificado em tempo real o Limite necessário para o cliente poder efetuar a operação de compra
 - Backend com Engine de Posição - Onde é calculado a posição do cliente em tempo real para enviar para o homebroker
 - Backend com Engine de OrderRouter - Onde é feita a orquestração das  ordens que chegam do matching e que chegam do homebroker.





![exchange_gif](https://github.com/user-attachments/assets/47c2019c-c03c-4856-8daa-20fd69874f92)



CQRS - com Coreografia, Mensageria

Singleton

Flyweight (OU algo parecido)



Command

Mediator

Observer

Ioc - Inversão de controle (Removido)

Injeção de depedencia

Unit of Work (Removido)

Event Sourcing (removido)

Alguns conceitos de S.O.L.I.D. tbm foram usados.

kind create cluster

kubectl apply -f .

![image](https://github.com/bvarandas/ChallengeDigitas/assets/13907905/852bac3a-0493-45b5-87ff-6e1c03d6c84d)

Tela Angular

![image](https://github.com/bvarandas/ChallengeDigitas/assets/13907905/97426a53-90f9-47c0-bfdd-0a3028c03033)



Modelo da arquitetura C4
