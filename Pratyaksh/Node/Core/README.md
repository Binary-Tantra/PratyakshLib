# Pratyaksh.Node.Core

[![NuGet Version](https://img.shields.io/nuget/v/Pratyaksh.Node.Core.svg)](https://www.nuget.org/packages/Pratyaksh.Node.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

**Pratyaksh.Node.Core** is a pure, headless, rendering-agnostic node graph data model and extensible type system. It provides the domain logic for node-based visual programming environments, state machines, logic editors, shader builders, and workflow graphs.

Because it contains zero dependencies on graphics libraries, windowing backends, or UI frameworks, `Pratyaksh.Node.Core` can run in CLI tools, backend microservices, unit test harnesses, and game server logic.

---

## 📦 Key Features

- **Pure Domain Model**:
  - `Graph`: Central container managing nodes, ports, connections, and variables.
  - `Node`: Represents an operation, function, or entity with designated input and output ports.
  - `Port`: Typed input or output communication pin (`PortFlowType.Input`, `PortFlowType.Output`).
  - `Connection`: Directed link binding a source output port to a target input port.
  - `Variable`: Graph-scoped reactive variable with type-safe accessors and two-way bindables (`BindableBool`, `BindableInt`, `BindableFloat`, `BindableString`).
- **Extensible Type System**:
  - `TypeManager`: Register custom data types (`DataType`), define compatible type conversions, and validate port-to-port connections.
  - Default types supported: `Execution` (flow control), `Int`, `Float`, `Number`, `String`, `Bool`.
- **Reactive Event System**:
  - Granular lifecycle notifications (`OnNodeAdded`, `OnNodeRemoved`, `OnPortAdded`, `OnConnectionRemoved`, `OnVariableAdded`, etc.) enabling clean decoupling from visual frontends.
- **Headless & Testable**:
  - Run graph evaluations, validation checks, and transformation passes entirely headless.

---

## 🚀 Installation

Install via the .NET CLI:

```bash
dotnet add package Pratyaksh.Node.Core
```

Or via the NuGet Package Manager:

```powershell
Install-Package Pratyaksh.Node.Core
```

---

## 🛠️ Usage Examples

### 1. Constructing a Node Graph Programmatically

```csharp
using Pratyaksh.Node.Core.DataModel;

// 1. Create a graph instance
Graph graph = new();

// 2. Register default types (Int, Float, String, Bool, Execution)
graph.Types.RegisterDefaultTypes();

DataType floatType = graph.Types.GetType("Float")!;
DataType execType = graph.Types.GetType("Execution")!;

// 3. Create a Math Add node (Inputs: Float A, Float B | Output: Float Result)
Node addNode = graph.AddNode(
    templateId: 101,
    inputPortTypes: [floatType, floatType],
    outputPortTypes: [floatType]
);

// 4. Create a Math Multiply node (Inputs: Float A, Float B | Output: Float Result)
Node mulNode = graph.AddNode(
    templateId: 102,
    inputPortTypes: [floatType, floatType],
    outputPortTypes: [floatType]
);

// 5. Connect addNode's output to mulNode's first input
int sourcePortId = addNode.OutputPortIds[0];
int targetPortId = mulNode.InputPortIds[0];

graph.AddConnection(sourcePortId, targetPortId);

Console.WriteLine($"Connected Port {sourcePortId} -> Port {targetPortId}");
Console.WriteLine($"Graph has {graph.Nodes.Count} nodes and {graph.Connections.Count} connection(s).");
```

### 2. Managing Graph Variables

```csharp
using Pratyaksh.Node.Core.DataModel;

Graph graph = new();
graph.Types.RegisterDefaultTypes();

DataType intType = graph.Types.GetType("Int")!;

// Add graph variable
graph.AddVariable("Health", intType, 100);

// Retrieve and mutate variable
Variable? healthVar = graph.Variables.Values.FirstOrDefault(v => v.VarName == "Health");
if (healthVar != null)
{
    Console.WriteLine($"Current Health: {healthVar.VarValue}"); // 100
    healthVar.VarValue = 85;
    Console.WriteLine($"Updated Health: {healthVar.VarValue}"); // 85
}
```

### 3. Registering Custom Data Types

```csharp
using Pratyaksh.Node.Core.DataModel;

TypeManager typeManager = new();

// Register custom Vector3 type
DataType vec3Type = new(
    name: "Vector3",
    csharpType: typeof(System.Numerics.Vector3),
    category: DataCategory.Data
);

typeManager.RegisterType(vec3Type);
```

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
