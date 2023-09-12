# Documentation
Vojtěch Gadurek - August 2023
## Sumary
The project is a library for unfinished minning game [https://github.com/vojtechgadurek/MinnigGameBlazorServer]
It offers these features
  - safe transactions, that always uphold game rules
  - storages with limited capacity with customisible handling of overflows
  - spotmarket - which matches demand and supply
## Classes
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
