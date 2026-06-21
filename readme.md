# Freeway

Freeway é um jogo desktop desenvolvido em C#/.NET com interface gráfica usando Eto.Forms e backend de comunicação via sockets TCP. O projeto simula uma versão multiplayer do clássico jogo Freeway, onde jogadores atravessam uma pista enquanto carros se movimentam automaticamente.

## Tecnologias utilizadas

- C#
- .NET 10
- Eto.Forms
- Eto.Platform.Gtk
- Sockets TCP
- Threads e Workers
- Programação orientada a interfaces

## Objetivo do projeto

O objetivo foi criar uma estrutura de jogo multiplayer com separação entre interface, lógica de estado, comunicação em rede e processamento assíncrono. O jogo pode ser iniciado como servidor ou como cliente, permitindo que diferentes instâncias se comuniquem por rede.

## Funcionalidades implementadas

- Tela inicial com opção de iniciar servidor ou conectar como cliente.
- Tela de conexão com IP e porta do servidor.
- Servidor TCP para aceitar conexões de clientes.
- Cliente TCP para envio e recebimento de mensagens.
- Sistema de mensagens compactadas em um único byte.
- Controle de jogadores por teclado.
- Estrutura para movimentação automática dos carros.
- Controle de estado do jogo com matriz de elementos.
- Separação entre carros, jogadores, ações, mensagens e pacotes de rede.
- Workers em threads separadas para processar entrada, jogo e carros.

## Como funciona

Ao iniciar a aplicação, o programa registra uma implementação de rede baseada em sockets e abre a janela principal.

A tela inicial possui duas opções:

- **Iniciar Jogo**: cria um servidor local na porta padrão `3333`.
- **Conectar**: abre uma tela para informar IP e porta de um servidor existente.

No modo servidor, o jogo cria os principais componentes internos:

- `GameStateHandler`: controla o estado dos jogadores, carros, pontuação e colisões.
- `InputWorker`: processa comandos recebidos dos jogadores.
- `CarService`: cria workers responsáveis pelo movimento dos carros.
- `GameWorker`: controla o ciclo principal do jogo.
- `SocketNetworkServer`: aceita conexões e distribui mensagens.

No modo cliente, o jogador se conecta ao servidor e envia comandos de movimento pelo teclado.

## Controles

- `W` ou seta para cima: mover para cima.
- `S` ou seta para baixo: mover para baixo.
- `Esc`: enviar comando de parada.

## Modelo de mensagens

As mensagens do jogo são representadas por um único byte usando divisão por bits:

- 2 bits para o tipo da mensagem.
- 3 bits para o ID do jogador.
- 3 bits para a ação executada.

Tipos de mensagem:

- `State`: estado do jogo.
- `Movement`: movimento do jogador ou carro.
- `Control`: comandos de controle, como iniciar, parar, pausar ou resetar.

## Estado do jogo

O estado principal é representado por uma matriz (`GameStateMatrix`) composta por elementos do tipo `GameElement`.

Cada elemento pode ser:

- `None`: espaço vazio.
- `Car`: carro.
- `Player`: jogador.

Também existe um modelo completo (`GameState`) que guarda arrays de jogadores e carros.

## Configurações principais

As configurações ficam em `ConfigurationSingleton`:

- Porta padrão: `3333`
- FPS: `60`
- Tamanho padrão da janela: `1024x768`
- Máximo de jogadores: `8`
- Máximo de carros: `20`
- Nível mínimo de dificuldade: `1`
- Nível máximo de dificuldade: `5`

## Como executar

É necessário ter o SDK do .NET compatível com `net10.0` instalado.

```bash
dotnet restore
dotnet run
```

## Status do projeto

O projeto já possui a base estrutural do jogo, incluindo interface, rede, modelos, workers e lógica de atualização do estado. Algumas partes ainda estão em desenvolvimento, como a atualização visual completa da cena e a serialização completa do estado do jogo para envio aos clientes.

## Autor

Projeto desenvolvido como estudo de jogo desktop multiplayer em C#, com foco em comunicação em rede, threads, arquitetura em camadas e controle de estado concorrente.