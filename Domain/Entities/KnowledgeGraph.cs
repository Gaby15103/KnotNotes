namespace Domain.Entities;

public class KnowledgeGraph
{
    private readonly Dictionary<string, IDocumentNode> _nodes = new(StringComparer.OrdinalIgnoreCase);

    public void AddOrUpdateNode(IDocumentNode node)
    {
        _nodes.Add(node.FilePath, node);
    }
    
    public IEnumerable<IDocumentNode> GetAllNodes => _nodes.Values;

    public IEnumerable<IDocumentNode> GetBackLinks(string targetTitle)
    {
        return _nodes.Values.Where(n => n.OutgoingLinks.Contains(targetTitle));
    }

    public IEnumerable<IDocumentNode> GEtOrphanNodes()
    {
        return _nodes.Values.Where(n => !n.OutgoingLinks.Any() && !GetBackLinks(n.Title).Any());
    }
}