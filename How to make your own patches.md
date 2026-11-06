# How to make your own patches

it's "in-memory" LuaCsForBarotrauma plugin based on BaseBaroPlugin

Copy paste this mod into LocalMods, change code in ClientProject\ClientSource\Patches, hot reload with cl_reloadcs

## How it works? 
It gets compiled after Showperf Extensions and CUI, so you can reference all its stuff  
Check `ClientProject\Showperf public api`

## Nuances

- Showperf stores ticks by Identifier, so capturing by Identifier is faster
- Always stop watch before capturing ticks, because creating ids takes time