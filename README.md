# Documentation
Vojtěch Gadurek - August 2023
## Sumary
The project is a library for unfinished minning game [https://github.com/vojtechgadurek/MinnigGameBlazorServer]
It offers these features
  - safe transactions, that always uphold game rules
  - storages with limited capacity with customisable handling of overflows
  - spotmarket - which matches demand and supply
## Overview
This library offers easy way for implementing some sort of simple trading game. The most important part of it is the transaction/trading system.
Every entity implementing ITrader interface is to interact with other entities via the transaction system, but for the sake of simplicity I would start with the resource type system.

### Resource type system
The library enforces code safety by using static type system for resources.
There are two main kinds of resource types which are Cash and HardResource. They are abstracted by ICash and IHardResource interfaces, and both of them implemets IResource.
ICash is used for representing money and IHardResource is used for representing resources like oil, gold, etc 
(resources that actually requires real space for storing them). There are two predefined types of resources, Money and Oil. Money is ICash and Oil is IHardResource.

#### R<TResource>
R\<TResource\> is a struct, that where TResource (TResource must implement IResource) is the resource type it represents. R\<TResource\> behaves like a double and it is internaly implemented
by double by it. It value is possible to get by using Amount property. 
Nearly all operators are overloaded and works as one would expect from double. 

```cs
R<Money> money = 100.Create<Money>(); //Creates 100 money
R<Oil> oil = 100.Create<Oil>(); //Creates 100 oil

//money = money + oil; //This would not compile, because money and oil are different types of resources
money = money + 100.Create<Money>(); //This would compile, because both operands are of type Money
money = money * 100 //This would compile, because multiplying by scalar is allowed
money = money * money; //This would not compile, because multiplying two money values is not allowed

//one can also create resource by using new 
R<Money> money = new R<Money>(100); //Creates 100 money
```

##### Capacity
There is also a concept of capacity, which is used for representing space for storing resources. It is used to not messup together resources and capacity.
 ```cs
R<Oil> oil = 100.Create<Oil>(); //Creates 100 oil
R<Capacity<oil> capacity1 = 100.Create<Capacity<Oil>>(); //Creates capacity for 100 oil
R<Capacity<oil> capacity2 = 100.Create<Oil>().ToCapacity();
capacity1.WillFit(oil); //True
capacity1.WillFit(oil * 2); //False
```


#### Custom resource types

Reflection is used for getting information about resource types. These is done by Resources class, which finds all classes/struct with ResourceDirective declaring them as resource type.

As just two types are hardcoded, one may need create addtional resource types. One just need to create empty struct/class and directive ResourceDirective.
```cs
//Write directive providing important information about the new resource type
[Resource("Coal" /* Name of the resource */, TypeOfResourceType.HardResource /* kind of the resource*/, typeof(Coal) /* Reference to the class/struct implementing it */)] 

//Create class/struct implementing IHardResource, if the resource is hard resource or ICash if the resource is cash
public struct Coal : IHardResource
{

}
```

### Transaction System
Transaction system gurantees, that transaction will be completed only if all resources and properties are availible 
for the transaction and there is space for these resources and properties
otherwise it will be safely aborted releasing all locked resources and properties. 
#### Transactions types

##### Transfers 


There are two types of transfer for resources and properties (ResourceTransfer, PropertyTransfer).
They allow to transfer resources and properties between two  ITrader entities. 
They gurantee, that there is resource availible for the transfer and space for storing it. 
Thy will also lock the resource/property and the capacity for the lifethime of the transaction.
If the transaction is aborted, via Dispose() the resource/property and capacity will be released.
The locked resource/blocked capacity cannot be used by others. 

Transfer are not executed immediately, but by calling ExecuteTransfer().


Resource transfer allows also a partial transfer, where only some percentage of the transfer is executed and 
the rest is held locked/blocked for futher usage. It may be usefull for example for implementing trade offers,
where one does not require to sell all resources at once.

##### TwoPartyTransaction
TwoPartyTransaction allows to merge multiple transfers in to one transaction.
It gurantees, that all transfers will be executed or none of them.

An example:
```cs
var transaction = new TwoPartyTransaction(buyer, seller)
				.AddTransactionItem(100.Create<Money>(), TransactionDirection.FromBuyerToSeller) //Tries to add transfer of 100 money from buyer to seller
				.AddTransactionItem(100.Create<Oil>(), TransactionDirection.FromSellerToBuyer) //Tries to add transfer of 100 oil from seller to buyer
transaction.TryExecute(); //Tries to execute transaction
```

TwoPartyProportinalTransaction allows to merge multiple resource transfers in to one transaction.
But it allow partial execution, but there may be only two resourceType in transaction.



So only the transaction is executed only to some proportion. So, for example only 50 % resources may be tranfered.
TryExecuteProportional is method used for executing partial transfer. If there is 0 resources availible for transfer,
transaction will be marked as completed and all resources will be released.

An example: 
```cs
var transaction = new TwoPartyProportinalTransaction(100.Create<Money>(),100.Create<Oil>(), buyer, seller)
//Creates transaction, that will try to transfer 100 money from buyer to seller and 100 oil from seller to buyer
transaction.TryExecuteProportional(0.5); //Tries to execute transaction to 50 % of its capacity
transaction.TryExecuteProportional(0.5); //Just 25 money and oil will be left
transaction.TryExecuteProportional(25.Create<Oil>()); //Transfer will be marked completed;
transaction.TryExecuteProportional(25.Create<Oil>()); //False will be returned, because transaction is already completed
```

## Classes
### Game
Game is the main class, that holds all important componets for the game together, also it initializes each gaming session.
### ITrader
ITrader is very important contract. It 
### Player
Player holds infromation about a player important for his indetification, like password and username and also data used by the game l
### Trader
- it represtents a entity able to hold resources and properties
### PriviligedTrader
- has MagicalStock used for entities mocking Trader contract.
### Transactions
- They allow an exchange of properties and resources between players with these rules
    - they are always done in full, so all properties and resources must be availible for a transaction
    - these transfer cannot overflow or underflow storages
- They usually lock underling asset, so there is guarantee, it will be availible for the duration of the transaction
#### ResourceTransfer/PropetyTransfer
- simplest, used for oneway transfer of resources
#### TwoPartyTransaction
- allows to merge mutiple Transfers in to one transaction
#### ProportionalTransaction
- allows only resource transfers and it allows, that just some percentage of the transaction would be completed
### Stock
- colection of Silos for each resource
- used for storing resource
- allows blocking capacity
- locking resources
- addind/removing resource/capacity
- force adding resource -> may lead to spill or underfill -> allows solution dor that
#### MagicalStock
- always returns true
####  CashSilo
- infinite capacity
#### ResourceSilo 
- finite capacity
#### LimitedDouble
- has two limits and values, value must always be between limits
### Resource
- struct for representing resource
#### ResourceType

### Property
- represents entity with possibility to be owned
### Registers
- allow entities with id to be held in one place
### SpotMarket
- posible to add trades, if two have matching price, trade will be made
- forcesell - if one overspill its silo

### OilField
- creates a oilfield, where additonal minig rig may be placed
