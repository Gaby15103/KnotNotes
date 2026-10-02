# KnotNotes - System Design Specification

## 1. Use Cases (Cas d'Utilisation)

### UC1: Initialiser l'espace de travail (Index Knowledge Base)
* **Actor:** Student (User)
* **Preconditions:** The local folder containing `.md` and `.pdf` files exists and is accessible on disk.
* **Main Scenario:**
   1. The user launches the application and selects the root directory.
   2. The application scans the folder recursively for supported file formats.
   3. Parsers read the content of each file.
   4. The system instantiates `MarkdownNote` or `PdfDocumentNode` objects and registers them into the `KnowledgeGraph`.
   5. The UI displays an overview summary (total notes, extracted tags, and link counts).
* **Postconditions:** The in-memory knowledge graph is fully populated and ready for queries.

### UC2: Explorer les backlinks et relations (Explore Backlinks)
* **Actor:** Student (User)
* **Preconditions:** The knowledge base has been successfully indexed.
* **Main Scenario:**
   1. The user searches for or selects a specific concept (e.g., `Semaphores`).
   2. The user requests to view **Backlinks** (all documents referencing this concept).
   3. The `KnowledgeGraph` filters its nodes to find items whose `OutgoingLinks` contain the target concept.
   4. The UI displays the list of referring documents as clickable items.
* **Postconditions:** The user navigates directly to the linked study notes.

### UC3: Synchronisation en temps réel (Live File Watcher)
* **Actor:** File System / Background Process
* **Preconditions:** The application is running and monitoring the root directory.
* **Main Scenario:**
   1. The user modifies, creates, or deletes a note using an external editor (e.g., Neovim or VS Code).
   2. The `FileSystemWatcher` intercepts the system event (`Changed`, `Created`, `Deleted`).
   3. The application triggers an incremental re-parse of the affected file.
   4. The `KnowledgeGraph` updates or removes the corresponding node and edges.
   5. The Avalonia UI automatically refreshes via MVVM data bindings.
* **Postconditions:** UI and memory state remain synchronized with disk changes without restarting.

---

## 2. Detailed Class Design (Conception Détaillée)

### A. Domain Layer (`KnotNotes.Domain`)
* **`IDocumentNode` (Interface):** Defines core properties (`Title`, `FilePath`, `OutgoingLinks`).
* **`MarkdownNote` (Entity):** Implements `IDocumentNode`, manages raw text content, and extracts `#tags` and `[[Wiki-Links]]` via regular expressions.
* **`PdfDocumentNode` (Entity):** Implements `IDocumentNode`, stores extracted PDF text for indexing.
* **`KnowledgeGraph` (Aggregate Root):** Maintains an in-memory dictionary of all documents for fast O(1) lookups, backlink analysis, and orphan detection.

### B. Presentation Layer (`KnotNotes.Desktop`)
* **`MainViewModel`:** Connects the domain state to the Avalonia UI using the MVVM pattern (`ObservableProperty`, `RelayCommand`, `ObservableCollection`).