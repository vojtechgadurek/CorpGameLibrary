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
Every entity implementing ITrader interface is to interact with other entities via the transaction system. 
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

A example:
```cs
var transaction = new TwoPartyTransaction(buyer, seller)
				.AddTransactionItem(100.Create<Money>(), TransactionDirection.FromBuyerToSeller) //Tries to add transfer of 100 money from buyer to seller
				.AddTransactionItem(100.Create<Oil>(), TransactionDirection.FromSellerToBuyer) //Tries to add transfer of 100 oil from seller to buyer
transaction.TryExecute(); //Tries to execute transaction
```

TwoPartyProportinalTransaction allows to merge multiple resource transfers in to one transaction.
But it allow partial execution, but there may be only two resourceType in transaction.



So only the transaction is executed only to some proportion. So, for example only 50 % resources may be tranfered.
TryExecuteProportional is method used for executing partial transfer.   


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
