# Documentation
Vojtěch Gadurek - August 2023
## Sumary
The project is a library for unfinished minning game [https://github.com/vojtechgadurek/MinnigGameBlazorServer]
It offers these features
  - safe transactions, that always uphold game rules
  - storages with limited capacity with customisable handling of overflows
  - spotmarket - which matches demand and supply
## Overview
This library offers an easy way for implementing some simple trading game. The most important part of it is the transaction/trading system.
Every entity implementing the ITrader interface can interact with other entities via the transaction system, but for simplicity, I would start with the resource type system.

### Resource type system
The library enforces code safety by using a static type system for resources.
There are two main kinds of resource types which are Cash and HardResource. They are abstracted by ICash and IHardResource interfaces, and both of them implemets IResource.
ICash is used for representing money and IHardResource is used for representing resources like oil, gold, etc 
(resources that require real space for storing them). There are two predefined types of resources, Money and Oil. Money is ICash and Oil is IHardResource.

#### R\<TResource\>
R\<TResource\> is a struct, that where TResource (TResource must implement IResource) is the resource type it represents. R\<TResource\> behaves like a double and it is internally implemented as a double. Its value is possible to get by using the Amount property. 
Nearly all operators are overloaded and works as one would expect from double. 

```cs
R<Money> money = 100.Create<Money>(); //Creates 100 money
R<Oil> oil = 100.Create<Oil>(); //Creates 100 oil

//money = money + oil; //This would not compile, because money and oil are different types of resources
money = money + 100.Create<Money>(); //This would compile because both operands are of type Money
money = money * 100 //This would compile because multiplying by scalar is allowed
money = money * money; //This would not compile, because multiplying two money values is not allowed

//one can also create resources by using new 
R<Money> money = new R<Money>(100); //Creates 100 money
```

##### Capacity
There is also a concept of capacity, which is used to represent space for storing resources. It is used to not mess up together resources and capacity.
 ```cs
R<Oil> oil = 100.Create<Oil>(); //Creates 100 oil
R<Capacity<oil> capacity1 = 100.Create<Capacity<Oil>>(); //Creates capacity for 100 oil
R<Capacity<oil> capacity2 = 100.Create<Oil>().ToCapacity();
capacity1.WillFit(oil); //True
capacity1.WillFit(oil * 2); //False
```


#### Custom resource types

Reflection is used for getting information about resource types. This is done by Resources class, which finds all classes/struct with ResourceDirective declaring them as resource type.

As just two types are hardcoded, one may need to create additional resource types. One just needs to create an empty struct/class and directive ResourceDirective.
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
Transaction are threadsafe.
#### Transactions types

##### Transfers 


There are two types of transfer for resources and properties (ResourceTransfer, PropertyTransfer).
They allow to transfer resources and properties between two  ITrader entities. 
They gurantee, that there is resource availible for the transfer and space for storing it. 
Thy will also lock the resource/property and the capacity for the lifethime of the transaction.
If the transaction is aborted, via Dispose() the resource/property and capacity will be released.
The locked resource/blocked capacity cannot be used by others.

An example:


Transfer are not executed immediately, but by calling ExecuteTransfer().

```cs

var transfer = new ResourceTransfer(100.Create<Money>(), buyer, seller); //Creates transfer of 100 money from buyer to seller
//If buyer did not ha 100 money, or seller did not had space for 100 money, transaction would be aborted
transfer.TryExecuteTransfer(); //If transaction was not aborted, it will be executed and than always returns true, otherwise false


var transfer = new PropertyTransfer((Property) property, buyer, seller); //Transfers property from player one to player two
//If player one did not had the property, or player two did not had space for the property, transaction would be aborted
transfer.ExecuteTransfer(); //If transaction was not aborted, it will be executed and than always returns true, otherwise it will throw an InvalidOperationException
```



Resource transfer allows also a partial transfer, where only some percentage of the transfer is executed and 
the rest is held locked/blocked for futher usage. It may be usefull for example for implementing trade offers,
where one does not require to sell all resources at once. If resource held by transfer is equal to 0, trade wil be auto autocompleted.

```cs

var transfer = new ResourceTransfer(100.Create<Money>(), buyer, seller); //Creates transfer of 100 money from buyer to seller
//If buyer did not ha 100 money, or seller did not had space for 100 money, transaction would be aborted
//For the example we will assume transfer was not aborted
transfer.TryExecutePartialTransfer(0.5); //50 will be left to transfer
transfer.TryExecutePartialTransfer(0.5); //25 will be left to transfer
transfer.TryExecutePartialTransfer(25.Create<Money>()); //0 left to transfer, transaction will be completed
```



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

#### Transaction system implemenatation details

When transfer is created, it will tries to lock the resource/property and capacity for the transaction.
If it fails, it will release all resources it managed to get held of, will set itself as disposed. 
Locking of resource (property soon to be) is done by Holded\<TResource\> and its decendants.

##### Blocking/Locking/Holding

###### Holded\<TResource>\
It allows a ability to hold a resource for transaction. A release delagate must be provided, when Release() is called, it is expected to safely release the resource.
###### Blocked\<TResourceHeld, TResourceBlockedFor\>
It allows to hold some resource for later and used with TResourceBlockedFor later. A use deleagte must be provided, when Use(resourceBlocked) is called, it is expected to safely use the resource.
###### Locked\<TResource>
It allows to lock some resource for later. A take deleagte must be provided, when get() is called, it is expected to safely get the resource from some source.
###### BlockedResource and LockedResource
They are derivation of Blocked and Locked for R\<\> types as such they allow partial take and partial use. 



```cs
Silo<Money> silo1 = StockFactory>CreateNoLimitsSilo<Money>(); //Creates a silo with no limits
Silo<Money> silo2 = StockFactory>CreateNoLimitsSilo<Money>(); //Creates a silo with no limits

BlockedResource<Money> blockedCapacityForMoney =  (BlockedResource<Money>) silo1.TryGetBlockOnCapacity(100.Create<Money>().ToCapacity()); //Tries to get 100 money from silo
BlockedResource<Money> lockedMoney =  (LockedResource<Money>) silo2.TryGetLockOnResource(100.Create<Money>()); //Tries to get 100 money from silo

blockedCapacityForMoney.Use(lockedMoney.Get()); //Uses the free space filling by locked resources
```

### Resources storing - Silos and Stocks

#### Silo\<TResource\>

Silo provides capacity to store resource of its type.
It enforce resource is always between upperlimit and lowerlimit.
It allows locking resources and also blocking capacity for later use.
Sometimes one it is needed to increase or decrease resources regardless of resources held. 
Then silo handles overflow and underflow by ISpillHandler and IUnderfillHandler, which must be provided at creation of the silo.
```cs
Silo<Money> silo1 = StockFactory>CreateNoLimitsSilo<Money>(); //Creates a silo with no limits with no reosurce in it
Silo<Money> silo2 = StockFactory>CreateNoLimitsSilo<Money>(); //Creates a silo with no limits with no reosurce in it

BlockedResource<Money> blockedCapacityForMoney =  (BlockedResource<Money>) silo1.TryGetBlockOnCapacity(100.Create<Money>().ToCapacity()); //Tries to get 100 money from silo
BlockedResource<Money> lockedMoney =  (LockedResource<Money>) silo2.TryGetLockOnResource(100.Create<Money>()); //Tries to get 100 money from silo

silo1.ForceIncreaseResource(100.Create<Money>()); //Increases resource by 100 money, it does not matter if there is space for it or not
silo1.GetResource(); //Returns 100 money


```


##### SiloFactory
SiloFactory is a factory class for creating silos. It provides methods for creating silos with different limits and spill/underfill handlers.

##### SiloConfifuration
Allows to configure silo. It is used by SiloFactory to create silos. 

##### LimitedDouble
LimitedDouble is a double, that is always between upperlimit and lowerlimit. It is possible to increase its value or value of this upper and lower limit, but it will allway upphold the rule.

#### Stock\<TResource\>
Stock creates silo for any Resource type and allows to access it via generic methods (that copy Silo public contract). 

```cs
Stock stock = StockFactory.CreatePriviligedTraderStock();

BlockedResource<Money> blockedCapacityForMoney =  (BlockedResource<Money>) stock.TryGetBlockOnCapacity(100.Create<Money>().ToCapacity()); //Tries to get 100 money from silo
BlockedResource<Money> lockedMoney =  (LockedResource<Money>) stock.TryGetLockOnResource(100.Create<Money>()); //Tries to get 100 money from silo

stock.ForceIncreaseResource(100.Create<Money>()); //Increases resource by 100 money, it does not matter if there is space for it or not
stock.GetResource<Money>(); //Returns 100 money

```

#### ISpillHandler and IUnderfillHandler
ISpillHandler and IUnderfillHandler are interfaces. Classes that implment them are used to handle overflow and underflow of resources.

##### SimpleSpillHandler
Dummy spill handler, that does nothing.

##### BankMoneySpillHandler
Handles cash underfill by borrowing money from a bank. It is used by CashSilo.

##### ResourceSpillHandler
Handles resource overflow by selling it at spotmarket.


### Register
Register is a class for storing game objects, it gives them unique id and allows to access them by id. It expect every object it stores to provide a place to store id as reference.
It provides possiblity to do action afeter registering a item.
#### PropertyRegister
Puts all properties in one place.
#### PlayerRegister
Manages players and their identification. 

### Properties
Properties represent a tradable entity thats in nature undividable or its limited in number and perform some action at the end of each round.
#### OilField
OilField is a property that produces oil each round. It does not care, if there is capacity to store it. 

### GameEntities
Game entities that need to interact via transactions system with other entities may be derived from PrivilegedTrader which implements ITrader, but its transaction allways succeed.
#### OilFieldProspector
Allow entities with ITrader interface to buy a oil field.
#### SpotMarket
Allows to sell resources at spot market. One need to provide price for resource traded, resource amount willing to trade, and direction of trade.
If there is any counterparty willing to trade at the price( or lower if buy offer, or higher if sell offer, the trade will be executed.
Trade will be executed at the price each party set a difference being profit for spotmarket.
Spotmarket allows market order which will try to sell trade at any price and minimum price must be provided. - This is used by ResourceSpillHandler.

### Player 
Player is a class that represents a player in the game. It is used by PlayerRegister to store players and give them unique id.

### Game 
Game is a class that represents a game. It is used to store all components/constants together.


