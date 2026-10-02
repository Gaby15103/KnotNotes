namespace Domain.Entities;

public interface IDocumentNode
{
    string Title { get; }
    string FilePath { get; }
    HashSet<string> OutgoingLinks { get; }
}