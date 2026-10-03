using Application.Services;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Knotnotes scan <path>");
            Console.WriteLine("       Knotnotes search <query>");
            return;
        }
        
        string commaned = args[0].ToLower();
        var scanner = new WorkspaceScanner();

        if (commaned == "scan" && args.Length > 1)
        {
            string path = args[1];
            Console.WriteLine($"Scanning workspace at {path}...");
            
            var graph = scanner.ScanDirectory(path);
            int count = graph.GetAllNodes.Count();
            
            Console.WriteLine($"Sucessfully indexed {count} documents.");
            foreach (var node in graph.GetAllNodes)
            {
                Console.WriteLine($" - [{node.Title}] ({node.FilePath})");
            }
        }
    }
}