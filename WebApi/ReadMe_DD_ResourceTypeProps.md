## **Design Decisions: Metadata-Based Design**

### **1. Problem Statement**
In a system where multiple resource types (e.g., Virtual Machines, Storage Accounts, etc.) need to be managed, each resource type has its own set of specific properties. The challenge is to design a solution that:
- Avoids creating a separate table for each resource type (to prevent schema proliferation).
- Dynamically handles resource-specific properties without hardcoding logic in the backend or frontend.
- Allows for easy addition of new resource types and properties without requiring code or schema changes.

---

### **2. Why Not Use Inheritance Mapping?**
Inheritance mapping (e.g., Table-Per-Type or Table-Per-Hierarchy) is a common approach for handling polymorphic data. However, it has several drawbacks in this context:
1. **Schema Proliferation**:
   - Table-Per-Type (TPT) requires creating a separate table for each resource type, which can lead to a bloated schema as the number of resource types grows.
2. **Rigid Design**:
   - Adding a new resource type or property requires schema changes and potentially code changes, making the system less flexible.
3. **Hardcoding**:
   - The backend and frontend would require hardcoded logic to handle each resource type and its specific properties, reducing maintainability.

---

### **3. Metadata-Based Design Overview**
The metadata-based design uses a **single table for all resource-specific properties** and dynamically determines the properties for each resource type based on metadata. This approach is flexible, scalable, and avoids the drawbacks of inheritance mapping.

#### **Key Components**
1. **Resource Table**:
   - Stores common properties for all resources (e.g., `Name`, `Status`, `ResourceTypeId`).

2. **ResourceProperty Table (Metadata)**:
   - Defines the properties for each resource type (e.g., "OsType", "VmSize").
   - Acts as a lookup table for resource-specific properties.

3. **ResourcePropertyValue Table**:
   - Stores the actual values for resource-specific properties.
   - Links to both the `Resource` table and the `ResourceProperty` table.

---

### **4. Database Schema**

#### **Resource Table**
Stores common properties for all resources.

```csharp
public class Resource
{
    public int ResourceId { get; set; }
    public required string Name { get; set; }
    public int WorkloadEnvironmentRegionId { get; set; }
    public int ResourceTypeId { get; set; }
    public required string Status { get; set; }

    // Navigation Properties
    public WorkloadEnvironmentRegion? WorkloadEnvironmentRegion { get; set; }
    public ResourceType? ResourceType { get; set; }
}
```

#### **ResourceProperty Table**
Defines the metadata for resource-specific properties.

```csharp
public class ResourceProperty
{
    public int PropertyId { get; set; }
    public int ResourceTypeId { get; set; }
    public string Name { get; set; } = string.Empty; // Property name (e.g., "OsType")
    public string DataType { get; set; } = string.Empty; // Data type (e.g., "string", "int")
    public bool IsRequired { get; set; } // Whether the property is required
}
```

#### **ResourcePropertyValue Table**
Stores the actual values for resource-specific properties.

```csharp
public class ResourcePropertyValue
{
    public int PropertyValueId { get; set; }
    public int ResourceId { get; set; } // Links to Resource
    public int PropertyId { get; set; } // Links to ResourceProperty
    public string Value { get; set; } = string.Empty; // Value stored as a string for flexibility

    // Navigation Properties
    public Resource? Resource { get; set; }
    public ResourceProperty? ResourceProperty { get; set; }
}
```

---

### **5. How It Works**

#### **Step 1: Metadata-Driven Property Definition**
- The `ResourceProperty` table defines the properties for each resource type.
- Example:
  - For a Virtual Machine (`ResourceTypeId = 1`):
    - `OsType` (string, required)
    - `VmSize` (string, required)
    - `OsDiskSizeGB` (int, required)
    - `DataDiskSizeGB` (int, optional)

#### **Step 2: Storing Resource-Specific Properties**
- The `ResourcePropertyValue` table stores the actual values for resource-specific properties.
- Example:
  - For a Virtual Machine resource (`ResourceId = 101`):
    - `OsType = "Linux"`
    - `VmSize = "Standard_DS1_v2"`
    - `OsDiskSizeGB = 128`
    - `DataDiskSizeGB = 256`

#### **Step 3: Dynamic UI Rendering**
- The frontend queries the `ResourceProperty` table to fetch the properties for the selected resource type.
- The UI dynamically renders input fields based on the metadata (e.g., text boxes for strings, number inputs for integers).

#### **Step 4: Dynamic Backend Handling**
- The backend processes resource-specific properties dynamically using the `ResourceProperty` table.
- The `ResourcePropertyValue` table is populated with the values provided by the user.

---

### **6. Benefits of Metadata-Based Design**

#### **Flexibility**
- Adding a new resource type or property only requires updating the `ResourceProperty` table—no schema or code changes are needed.

#### **Scalability**
- The design supports an unlimited number of resource types and properties without bloating the database schema.

#### **Dynamic UI**
- The frontend dynamically renders fields based on the metadata, reducing hardcoding and improving maintainability.

#### **Normalized Schema**
- The database schema is clean and normalized, avoiding table proliferation.

#### **Maintainability**
- The backend logic is generic and metadata-driven, reducing duplication and hardcoding.

---

### **7. Example Workflow**

#### **Adding a New Resource Type**
1. Add a new entry in the `ResourceType` table (e.g., "Virtual Machine").
2. Add entries in the `ResourceProperty` table to define the properties for the new resource type.

#### **Adding a New Resource**
1. The user selects a resource type (e.g., "Virtual Machine") in the UI.
2. The frontend queries the `ResourceProperty` table to fetch the required properties.
3. The user fills in the values for the properties.
4. The backend saves the resource in the `Resource` table and the property values in the `ResourcePropertyValue` table.

---

### **8. Why This Design Is Better Than Inheritance Mapping**

| **Aspect**              | **Metadata-Based Design**                          | **Inheritance Mapping**                     |
|--------------------------|---------------------------------------------------|---------------------------------------------|
| **Schema Proliferation** | Single table for all resource-specific properties | Separate table for each resource type       |
| **Flexibility**          | Easy to add new resource types or properties      | Requires schema changes                     |
| **Dynamic UI**           | Fully dynamic based on metadata                   | Requires hardcoding for each resource type  |
| **Maintainability**      | Generic backend logic                             | Hardcoded logic for each resource type      |
| **Scalability**          | Supports unlimited resource types and properties | Limited by schema complexity                |

---
