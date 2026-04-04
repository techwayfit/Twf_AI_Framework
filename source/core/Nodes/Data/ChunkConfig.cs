namespace TwfAiFramework.Nodes.Data;

public sealed class ChunkConfig
{
    public int ChunkSize { get; init; } = 500;
    public int Overlap { get; init; } = 50;
    public ChunkStrategy Strategy { get; init; } = ChunkStrategy.Character;
}