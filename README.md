1. Detailed Use Cases (Spécification des Cas d'Utilisation)
   UC1: Index Local Knowledge Base (Initialisation de l'espace de travail)
   Actor: Student (User)

Preconditions: The local folder path containing .md and .pdf files exists and is accessible.

Main Scenario:

The user launches the application and selects the root folder path.

The application triggers the file scanner to recursively search for supported file formats.

For each file found, the appropriate parser (MarkdownParser or PdfParser) reads the content.

The system instantiates MarkdownNote or PdfDocumentNode objects and adds them to the KnowledgeGraph.

The UI displays an overview summary (total notes, extracted tags, and link counts).

Postconditions: The KnowledgeGraph is populated in memory and ready for queries.

UC2: Explore Backlinks & Graph Relationships (Recherche de relations)
Actor: Student (User)

Preconditions: The knowledge base is successfully indexed.

Main Scenario:

The user selects a specific note or searches for a concept (e.g., Semaphores).

The user requests to view Backlinks (notes that point to this concept).

The KnowledgeGraph queries its internal dictionary and filters nodes containing the target in their OutgoingLinks.

The UI displays the list of referring documents, allowing the user to click and navigate directly to them.

UC3: Real-Time File Synchronization (Live Watcher Sync)
Actor: File System / Background Process

Preconditions: The application is running and monitoring the root directory.

Main Scenario:

The user modifies, creates, or deletes a Markdown file using an external text editor (e.g., Neovim or VS Code).

The FileSystemWatcher intercepts the file system event (Changed, Created, or Deleted).

The application triggers a incremental re-parse of only the affected file.

The KnowledgeGraph updates or removes the corresponding node and its connections.

The Avalonia UI automatically refreshes via MVVM data bindings without requiring an application restart.

2. Detailed Class Design (Conception Détaillée des Classes)
   This breaks down the attributes and core methods across your architecture layers.

A. Domain Layer (KnotNotes.Domain)
Pure business models, invariants, and graph logic with zero external dependencies.

Plaintext
+-------------------------------------------------------+
|                       <<interface>>                   |
|                        IDocumentNode                  |
+-------------------------------------------------------+
| + Title: string { get; }                              |
| + FilePath: string { get; }                           |
| + OutgoingLinks: HashSet<string> { get; }             |
+-------------------------------------------------------+
^
| implements
+-----------------+-----------------+
|                                   |
+---------------------------------+ +---------------------------------+
|          MarkdownNote           | |         PdfDocumentNode         |
+---------------------------------+ +---------------------------------+
| - Content: string               | | - ExtractedText: string         |
| - Tags: HashSet<string>         | | - OutgoingLinks: HashSet<string>|
+---------------------------------+ +---------------------------------+
| + UpdateContent(newContent)     | | + PdfDocumentNode(path, text)   |
| - ParseMetadata()               | +---------------------------------+
+---------------------------------+

+-------------------------------------------------------+
|                    KnowledgeGraph                     |
+-------------------------------------------------------+
| - _nodes: Dictionary<string, IDocumentNode>           |
+-------------------------------------------------------+
| + AddOrUpdateNode(node: IDocumentNode)                |
| + GetAllNodes(): IEnumerable<IDocumentNode>           |
| + GetBacklinks(targetTitle: string): IEnumerable      |
| + GetOrphanNodes(): IEnumerable<IDocumentNode>        |
+-------------------------------------------------------+
B. Presentation Layer / MVVM (KnotNotes.Desktop)
Connects the domain state to the Avalonia UI components safely.

Plaintext
+-------------------------------------------------------+
|                     MainViewModel                     |
+-------------------------------------------------------+
| - _graph: KnowledgeGraph                              |
| - searchQuery: string [ObservableProperty]            |
| - selectedNote: IDocumentNode [ObservableProperty]    |
| + FilteredNotes: ObservableCollection<IDocumentNode>  |
+-------------------------------------------------------+
| + SearchNotesCommand() [RelayCommand]                 |
| + LoadDirectoryCommand(path: string) [RelayCommand]   |
+-------------------------------------------------------+
3. Interaction Sequence: File Change to UI Update
   To understand how data flows through your object-oriented design, here is the sequence of execution when a file is modified externally:

FileSystemWatcher (Infrastructure) detects a write/save event on C:/Notes/OperatingSystems.md.

It notifies the GraphManager (Application Service), passing the file path.

The service invokes the MarkdownParser, which reads the raw text from disk and instantiates or updates a MarkdownNote (Domain Entity), automatically executing regex parsing for [[Links]] and #Tags.

The service passes the updated note to the KnowledgeGraph (Domain Aggregate Root), which updates its internal dictionary.

The KnowledgeGraph raises a notification or updates state, which the MainViewModel (Presentation) reads.

The Avalonia UI list box or view bound to FilteredNotes automatically re-renders via data binding.