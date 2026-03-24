# Workflow Designer - Getting Started Guide

## Overview

The TWF AI Framework now includes a visual workflow designer web application that allows you to create, edit, and manage AI workflows through an intuitive drag-and-drop interface.

## Quick Start

### 1. Run the Web Application

```bash
cd source/web
dotnet run
```

The application will start on `https://localhost:5001` (or the port specified in launchSettings.json).

### 2. Create Your First Workflow

1. Navigate to `https://localhost:5001`
2. Click **"Create New Workflow"**
3. Enter:
   - **Name**: "My First AI Workflow"
   - **Description**: "A simple workflow to test the designer"
   - **Tags**: "test, ai"
4. Click **"Create & Open Designer"**

### 3. Design Your Workflow

#### Add Nodes
- Find nodes in the left **Node Palette**
- Drag a **Prompt Builder** node onto the canvas
- Drag an **LLM** node onto the canvas
- Drag an **Output Parser** node onto the canvas

#### Connect Nodes
- Click and drag from the **output port** (right side) of the Prompt Builder
- Drop on the **input port** (left side) of the LLM node
- Connect the LLM output to the Output Parser input

#### Configure Nodes
- Click on any node to select it
- Edit properties in the right **Properties Panel**
- Change the node name if needed

### 4. Save Your Workflow

Click the **"Save"** button in the toolbar. Your workflow is now persisted!

## Storage Options

### Option 1: JSON Files (Default)

Workflows are saved as individual `.json` files in the `workflows/` directory.

**Configuration** (`appsettings.json`):
```json
{
  "UseDatabase": false,
  "WorkflowDataDirectory": "workflows"
}
```

**When to use:**
- Small to medium number of workflows (< 100)
- Want easy version control
- Prefer human-readable storage
- Simple deployment requirements

### Option 2: SQLite Database

Workflows are stored in a SQLite database file.

**Configuration** (`appsettings.json`):
```json
{
  "UseDatabase": true,
  "ConnectionStrings": {
    "WorkflowDb": "Data Source=workflows.db"
  }
}
```

**When to use:**
- Large number of workflows (> 100)
- Need better search performance
- Multiple concurrent users
- Want ACID compliance

## Workflow Designer Features

### Visual Canvas
- **Infinite canvas** with grid background
- **Zoom controls** (50% - 200%)
- **Pan and scroll** for navigation
- **Snap to grid** (coming soon)

### Node Operations
- **Drag to move** nodes around
- **Click to select** and edit properties
- **Delete key** to remove selected node
- **Multi-select** (coming soon)

### Connection Management
- **Bezier curves** for visual appeal
- **Arrow indicators** show data flow direction
- **Click to select** connections
- **Delete key** to remove connections
- **Auto-routing** (coming soon)

### Property Editing
- **Real-time updates** as you type
- **Node-specific parameters** based on type
- **Validation** (coming soon)
- **Templates** (coming soon)

## Available Node Types

### AI Nodes (Blue)
- **LLM Node**: Execute large language model calls
- **Prompt Builder**: Build prompts from templates
- **Embedding Node**: Generate vector embeddings
- **Output Parser**: Parse and structure LLM outputs

### Control Nodes (Orange)
- **If Node**: Conditional branching
- **Switch Node**: Multi-way branching
- **Loop Node**: Iterate over collections

### Data Nodes (Green)
- **Filter Node**: Filter data based on conditions
- **Transform Node**: Transform data structure
- **Merge Node**: Combine multiple data sources

### IO Nodes (Purple)
- **HTTP Request**: Make HTTP API calls
- **File Read**: Read data from files
- **File Write**: Write data to files

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Delete` | Delete selected node or connection |
| `Escape` | Deselect all |
| `Ctrl+S` | Save workflow |
| `Ctrl+Z` | Undo (coming soon) |
| `Ctrl+Y` | Redo (coming soon) |

## Workflow Management

### List View
- View all workflows in a card-based layout
- Search by name, description, or tags
- Sort by creation or update date
- Quick actions: Design, View, Delete

### Details View
- View complete workflow information
- See all nodes and connections
- View workflow statistics
- Export/import (coming soon)

### Designer View
- Full-featured visual editor
- Real-time auto-save (coming soon)
- Collaboration features (coming soon)

## Tips and Best Practices

### Organizing Workflows
1. Use **descriptive names** that explain the workflow's purpose
2. Add **comprehensive descriptions** for documentation
3. Use **tags** to categorize workflows (e.g., "production", "testing", "ai")
4. Set the **author** field for team workflows

### Designing Workflows
1. **Layout nodes left-to-right** following data flow
2. **Group related nodes** vertically
3. **Use colors** to distinguish node types
4. **Name nodes** descriptively (e.g., "Extract User Intent" vs "LLM 1")
5. **Avoid crossing connections** when possible

### Performance
1. For large workflows (>50 nodes), consider using **subflows** (coming soon)
2. **Zoom out** to see the entire workflow structure
3. **Use the minimap** (coming soon) for navigation

## Troubleshooting

### Workflow not saving
- Check browser console for errors
- Verify write permissions on the workflows directory
- Check disk space
- Ensure the web application has necessary permissions

### Nodes not connecting
- Verify you're dragging from output (right) to input (left)
- Ensure both ports are visible
- Try zooming in for better precision
- Refresh the page if connections appear stuck

### Performance issues
- Try switching to SQLite storage for better performance
- Reduce the number of nodes per workflow
- Clear browser cache
- Use a modern browser (Chrome, Firefox, Edge)

## Example Workflows

### 1. Simple Chat Bot
```
[Prompt Builder] ? [LLM Node] ? [Output Parser]
```

### 2. Data Processing Pipeline
```
[File Read] ? [Filter] ? [Transform] ? [File Write]
```

### 3. Conditional Workflow
```
[HTTP Request] ? [If Node] ?? [LLM Node] ? [Output]
         ?? [Error Handler]
```

## Next Steps

- Explore the [API Documentation](./API.md) (coming soon)
- Learn about [Custom Node Development](./CUSTOM_NODES.md) (coming soon)
- Read the [Workflow Execution Guide](./EXECUTION.md) (coming soon)
- Join the [Community Forum](https://github.com/techwayfit/Twf_AI_Framework/discussions)

## Support

- ?? [Documentation](./README.md)
- ?? [Report Issues](https://github.com/techwayfit/Twf_AI_Framework/issues)
- ?? [Discussions](https://github.com/techwayfit/Twf_AI_Framework/discussions)
- ?? Contact: support@techwayfit.com (if applicable)
