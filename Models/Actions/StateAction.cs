namespace Freeway.Models.Actions;
internal enum StateAction : byte
{
    Snapshot = 0, // estado completo
    StateChanged = 1, // houve alteração
    Collision = 2, // colisão detectada
    Point = 3, // marcou ponto
    Win = 4, // venceu
    GameOver = 5, // perdeu
    Connected = 6, // jogador entrou
    Disconnected = 7  // jogador saiu
}