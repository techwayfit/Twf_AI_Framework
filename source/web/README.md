# TWF AI Framework - Workflow Designer

A visual workflow designer for creating and managing AI workflows using the TWF AI Framework.

## Features

### Visual Workflow Designer
- **Drag-and-drop interface** for creating workflows
- **Node-based visual programming** similar to n8n
- **Connection management** with visual bezier curves
- **Zoom and pan** controls for large workflows
- **Property editing** for node configuration

### Node Types Available
- **AI Nodes**: LLM, Prompt Builder, Embedding, Output Parser
- **Control Nodes**: If, Switch, Loop
- **Data Nodes**: Filter, Transform, Merge
- **IO Nodes**: HTTP Request, File Read, File Write

### Persistence Options
The workflow designer supports two storage backends:

#### 1. JSON File Storage (Default)
Workflows are saved as individual JSON files in a directory.

```json
{
  "UseDatabase": false,
  "WorkflowDataDirectory": "workflows"
}
```

**Pros:**
- Simple, no database setup required
- Easy to version control
- Human-readable format
- Easy backup and migration

**Cons:**
- Slower for large numbers of workflows
- No built-in search capabilities
- Concurrent access limitations

#### 2. SQLite Database
Workflows are stored in a SQLite database.

```json
{
  "UseDatabase": true,
  "ConnectionStrings": {
    "WorkflowDb": "Data Source=workflows.db"
  }
}
```

**Pros:**
- Better performance for many workflows
- Built-in indexing and search
- ACID compliance
- Better concurrent access

**Cons:**
- Requires database setup
- Less human-readable
- Slightly more complex deployment

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Modern web browser (Chrome, Firefox, Edge)

### Running the Application

1. **Configure storage** in `appsettings.json`:
   ```json
   {
     "UseDatabase": false,
     "WorkflowDataDirectory": "workflows"
   }
   ```

2. **Run the application**:
   ```bash
   cd source/web
   dotnet run
   ```

3. **Open your browser** to `https://localhost:5001`

### Creating Your First Workflow

1. Click **"Create New Workflow"**
2. Enter a name and description
3. Click **"Create & Open Designer"**
4. Drag nodes from the palette on the left
5. Connect nodes by dragging from output ports to input ports
6. Click **"Save"** to persist your workflow

## Workflow Designer Interface

### Toolbar
- **Zoom In/Out**: Control canvas zoom level
- **Reset**: Reset zoom to 100%
- **Save**: Save workflow to storage
- **Delete**: Delete selected node or connection
- **Back**: Return to workflow list

### Node Palette (Left Panel)
Browse and drag available node types onto the canvas.

### Canvas (Center)
Visual workspace for designing workflows:
- **Drag nodes** to reposition
- **Click nodes** to select and edit properties
- **Drag from ports** to create connections
- **Click connections** to select for deletion

### Properties Panel (Right Panel)
Edit properties of the selected node:
- Node name
- Node type (read-only)
- Category (read-only)
- Custom parameters (node-specific)

## Keyboard Shortcuts

- **Delete**: Delete selected node or connection
- **Escape**: Deselect all
- **Ctrl+S**: Save workflow

## Project Structure

```
source/web/
??? Controllers/
?   ??? WorkflowController.cs    # MVC controller for workflow operations
??? Data/
?   ??? WorkflowDbContext.cs     # EF Core database context
??? Models/
?   ??? WorkflowDefinition.cs    # Workflow data models
??? Repositories/
?   ??? IWorkflowRepository.cs        # Repository interface
?   ??? JsonFileWorkflowRepository.cs    # JSON file implementation
?   ??? SqliteWorkflowRepository.cs      # SQLite implementation
??? Views/
?   ??? Workflow/
?   ??? Index.cshtml         # Workflow list
?       ??? Create.cshtml        # Create workflow
?       ??? Designer.cshtml      # Visual designer
?       ??? Details.cshtml       # Workflow details
?       ??? Delete.cshtml        # Delete confirmation
??? wwwroot/
    ??? js/
        ??? workflow-designer.js # Designer JavaScript logic
```

## Workflow JSON Format

Workflows are saved in the following JSON format:

```json
{
  "id": "guid",
  "name": "My Workflow",
  "description": "Description",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "nodes": [
    {
 "id": "node-guid",
      "name": "LLM Node",
      "type": "LlmNode",
      "category": "AI",
      "parameters": {},
    "position": { "x": 100, "y": 100 },
      "color": "#4A90E2"
    }
  ],
  "connections": [
    {
    "id": "connection-guid",
      "sourceNodeId": "source-node-guid",
      "sourcePort": "output",
      "targetNodeId": "target-node-guid",
      "targetPort": "input"
    }
  ],
  "metadata": {
    "author": "User Name",
 "tags": ["ai", "workflow"],
    "version": 1,
    "isActive": true
  }
}
```

## Switching Between Storage Backends

To switch from JSON files to SQLite:

1. Update `appsettings.json`:
   ```json
   {
     "UseDatabase": true
   }
   ```

2. The database will be created automatically on first run

3. To migrate existing workflows, use the migration utility (TODO)

## API Endpoints

The workflow controller exposes the following endpoints:

- `GET /Workflow` - List all workflows
- `GET /Workflow/Details/{id}` - View workflow details
- `GET /Workflow/Create` - Create workflow form
- `POST /Workflow/Create` - Create new workflow
- `GET /Workflow/Designer/{id}` - Open visual designer
- `GET /Workflow/Edit/{id}` - Edit workflow metadata
- `POST /Workflow/Edit/{id}` - Update workflow metadata
- `GET /Workflow/Delete/{id}` - Delete confirmation
- `POST /Workflow/Delete/{id}` - Delete workflow
- `GET /Workflow/GetWorkflow/{id}` - Get workflow JSON (API)
- `POST /Workflow/SaveWorkflow` - Save workflow JSON (API)
- `GET /Workflow/GetAvailableNodes` - Get available node types (API)

## Future Enhancements

- [ ] Workflow execution engine
- [ ] Real-time collaboration
- [ ] Workflow templates library
- [ ] Export/import workflows
- [ ] Workflow versioning
- [ ] Undo/redo support
- [ ] Node parameter validation
- [ ] Custom node creation UI
- [ ] Workflow testing framework
- [ ] Performance metrics dashboard

## Contributing

Contributions are welcome! Please feel free to submit pull requests.

## License

This project is part of the TWF AI Framework.
