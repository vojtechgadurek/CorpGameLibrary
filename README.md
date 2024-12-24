
# Documentation  
**Vojtěch Gadurek – August 2023**  

## Summary  
This project is a library for an unfinished mining game: [GitHub Repository](https://github.com/vojtechgadurek/MinnigGameBlazorServer).  

**Features include:**  
- Safe transactions that always follow game rules.  
- Storages with limited capacity and customizable overflow handling.  
- A spot market to match supply and demand.  

## Overview  
This library simplifies the creation of a basic trading game. The main feature is its transaction system. Any entity implementing the `ITrader` interface can interact with others using this system. However, for simplicity, let’s first look at the resource type system.  

### Resource Type System  
The library ensures safety by using a static type system for resources.  
There are two main resource types:  
- **Cash** (e.g., money)  
- **HardResource** (e.g., physical resources like oil or gold)  

These are represented by the `ICash` and `IHardResource` interfaces, both of which implement the `IResource` interface. The library provides two predefined resource types: `Money` (cash) and `Oil` (hard resource).  

#### R\<TResource\>  
`R<TResource>` is a struct that represents a specific resource type. It behaves like a `double` and supports mathematical operations using overloaded operators.  

```csharp
R<Money> money = 100.Create<Money>(); // Creates 100 units of money  
R<Oil> oil = 100.Create<Oil>(); // Creates 100 units of oil  

//money = money + oil; // Error: Money and Oil are different types  
money = money + 100.Create<Money>(); // Valid: Both are Money  
money = money * 100; // Valid: Multiplying by a scalar is allowed  
money = money * money; // Error: Multiplying two Money values is not allowed  

// Alternatively, create resources using 'new':  
R<Money> money = new R<Money>(100); // Creates 100 units of money  
```

##### Capacity  
Capacity represents storage space for resources and keeps resources and storage amounts separate.  

```csharp
R<Oil> oil = 100.Create<Oil>(); // Creates 100 units of oil  
R<Capacity<Oil>> capacity1 = 100.Create<Capacity<Oil>>(); // Creates capacity for 100 units of oil  
capacity1.WillFit(oil); // True  
capacity1.WillFit(oil * 2); // False  
```

#### Custom Resource Types  
You can define custom resources using reflection. The `Resources` class identifies all resource types marked with the `ResourceDirective` attribute.  

```csharp
[Resource("Coal", TypeOfResourceType.HardResource, typeof(Coal))]  
public struct Coal : IHardResource {}  
```

### Transaction System  
The transaction system ensures that transactions only complete if all conditions are met:  
- Resources and storage space are available.  
- Resources are safely locked.  

Transactions are thread-safe.  

#### Transaction Types  

##### Transfers  
Transfers allow resources or properties to be exchanged between two `ITrader` entities. Resources are locked for the transaction duration. If the transaction fails, all locked resources are released.  

```csharp
var transfer = new ResourceTransfer(100.Create<Money>(), buyer, seller);  
transfer.TryExecuteTransfer(); // Executes the transfer if possible; returns true or false  

var propertyTransfer = new PropertyTransfer(property, buyer, seller);  
propertyTransfer.ExecuteTransfer(); // Executes the transfer or throws an exception if invalid  
```

Partial transfers are supported, allowing trades in smaller increments.  

```csharp
var transfer = new ResourceTransfer(100.Create<Money>(), buyer, seller);  
transfer.TryExecutePartialTransfer(0.5); // Transfers 50%  
transfer.TryExecutePartialTransfer(25.Create<Money>()); // Completes the transfer  
```

##### TwoPartyTransaction  
This allows multiple transfers to be grouped into a single transaction, ensuring that either all transfers succeed or none do.  

```csharp
var transaction = new TwoPartyTransaction(buyer, seller)  
    .AddTransactionItem(100.Create<Money>(), TransactionDirection.FromBuyerToSeller)  
    .AddTransactionItem(100.Create<Oil>(), TransactionDirection.FromSellerToBuyer);  
transaction.TryExecute();  
```

Partial execution is possible with proportional transactions (`TwoPartyProportionalTransaction`).  

```csharp
var transaction = new TwoPartyProportionalTransaction(100.Create<Money>(), 100.Create<Oil>(), buyer, seller);  
transaction.TryExecuteProportional(0.5); // Executes 50% of the transaction  
transaction.TryExecuteProportional(25.Create<Oil>()); // Completes the transaction  
```

### Implementation Details  
When a transfer is created, it tries to lock resources and capacity for the transaction. If it fails, all locked resources are released, and the transaction is marked as disposed.  

#### Locking and Blocking  
- `Holded<TResource>`: Holds a resource for a transaction.  
- `Blocked<TResourceHeld, TResourceBlockedFor>`: Blocks a resource for later use.  
- `Locked<TResource>`: Locks a resource for later retrieval.  
- `BlockedResource` and `LockedResource`: Specialized versions for `R<>` types, supporting partial use and take.  

```csharp
Silo<Money> silo1 = StockFactory.CreateNoLimitsSilo<Money>();  
Silo<Money> silo2 = StockFactory.CreateNoLimitsSilo<Money>();  

BlockedResource<Money> blockedCapacityForMoney = silo1.TryGetBlockOnCapacity(100.Create<Money>().ToCapacity());  
LockedResource<Money> lockedMoney = silo2.TryGetLockOnResource(100.Create<Money>());  

blockedCapacityForMoney.Use(lockedMoney.Get());  
```

### Resource Storage: Silos and Stocks  

#### Silo\<TResource\>  
A silo provides storage capacity for a resource type. It ensures resources are within upper and lower limits. Overflows and underflows are handled using `ISpillHandler` and `IUnderfillHandler`.  

```csharp
Silo<Money> silo1 = StockFactory.CreateNoLimitsSilo<Money>();  
silo1.ForceIncreaseResource(100.Create<Money>()); // Adds 100 units of money, regardless of capacity  
silo1.GetResource(); // Returns 100 units of money  
```

##### SiloFactory  
A factory class for creating silos with different configurations and handlers.  

##### SiloConfiguration  
Defines silo settings, used by the `SiloFactory`.  

##### LimitedDouble  
A `double` with enforced upper and lower limits.  

#### Stock\<TResource\>  
A stock creates silos for any resource type and provides generic methods for accessing them.  

```csharp
Stock stock = StockFactory.CreatePrivilegedTraderStock();  
stock.ForceIncreaseResource(100.Create<Money>());  
stock.GetResource<Money>(); // Returns 100 units of money  
```

#### Spill and Underfill Handlers  
- `ISpillHandler` and `IUnderfillHandler`: Interfaces for handling resource overflows and underflows.  
- `SimpleSpillHandler`: Does nothing for overflows.  
- `BankMoneySpillHandler`: Borrows money from a bank during underflows.  
- `ResourceSpillHandler`: Sells excess resources on the spot market.  

### Register  
The `Register` class stores game objects, assigns them unique IDs, and provides access by ID.  
- **PropertyRegister**: Stores all properties.  
- **PlayerRegister**: Manages players and their IDs.  

### Properties  
Properties represent tradeable, indivisible entities.  
- **OilField**: Produces oil each round without checking storage capacity.  

### Game Entities  
Entities interacting through transactions should derive from `PrivilegedTrader`, which ensures all transactions succeed.  
- **OilFieldProspector**: Allows entities to buy oil fields.  
- **SpotMarket**: Executes trades at specified prices and manages market orders.  

### Player  
The `Player` class represents a game player and is managed by `PlayerRegister`.  

### Game  
The `Game` class combines all components and constants into one structure.  
