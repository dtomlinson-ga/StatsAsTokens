### Stats As Tokens
*by Vertigon*

[NexusMods Page](https://www.nexusmods.com/stardewvalley/mods/9659)

This mod allows Content Patcher pack creators to access all stats tracked by the game through custom CP tokens, allowing for patches to trigger
on various player milestones including number of seeds sown, truffles found, trash cans checked and many more! See below for a complete list.

### Usage
Download this mod, place it in your Mods folder, and list it as a dependency in your `manifest.json` in order to access its custom tokens.

### Dependencies
* [SMAPI](https://smapi.io/)  v3.12.0 or higher is a required dependency.
* [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) v1.23.0 or higher is a required dependency.

## Custom Tokens Provided

### **`Vertigon.StatsAsTokens/Stats`**

This token takes exactly two named arguments (both must be provided in order for it to work):
* **`player`**: Must be one of the following:
  * `host`: The player hosting the lobby, or
  * `local`: The player on the local splitscreen or computer, if not the host
* **`stat`**: The stat to track. See below for a complete list.

The arguments are case-insensitive and space-insensitive.

For example:
`{{Vertigon.StatsAsTokens/Stats:player=host|stat=diamondsFound}}` will be parsed as the number of diamonds found by the host player.

Here is a complete list of stats currently usable as arguments:

* `seedsSown`
* `itemsShipped`
* `itemsCooked`
* `itemsCrafted`
* `chickenEggsLayed`
* `duckEggsLayed`
* `cowMilkProduced`
* `goatMilkProduced`
* `rabbitWoolProduced`
* `sheepWoolProduced`
* `cheeseMade`
* `goatCheeseMade`
* `trufflesFound`
* `stoneGathered`
* `rocksCrushed`
* `dirtHoed`
* `giftsGiven`
* `timesUnconscious`
* `averageBedtime`
* `timesFished`
* `fishCaught`
* `bouldersCracked`
* `stumpsChopped`
* `stepsTaken`
* `monstersKilled`
* `diamondsFound`
* `prismaticShardsFound`
* `otherPreciousGemsFound`
* `caveCarrotsFound`
* `copperFound`
* `ironFound`
* `coalFound`
* `coinsFound`
* `goldFound`
* `iridiumFound`
* `barsSmelted`
* `beveragesMade`
* `preservesMade`
* `piecesOfTrashRecycled`
* `mysticStonesCrushed`
* `daysPlayed`
* `weedsEliminated`
* `sticksChopped`
* `notesFound`
* `questsCompleted`
* `starLevelCropsShipped`
* `cropsShipped`
* `itemsForaged`
* `slimesKilled`
* `geodesCracked`
* `goodFriends`
* `totalMoneyGifted`
* `individualMoneyEarned`
* `timesEnchanted`
* `beachFarmSpawns`
* `hardModeMonstersKilled`
* `childrenTurnedToDoves`
* `boatRidesToIsland`
* `trashCansChecked`

### **`Vertigon.StatsAsTokens/MonstersKilled`**

This token takes exactly two named arguments (both must be provided in order for it to work):
* **`player`**: Must be one of the following:
  * `host`: The player hosting the lobby, or
  * `local`: The player on the local splitscreen or computer, if not the host
* **`monster`**: The monster to check kills for. See below for a complete list.

The arguments are case-insensitive and space-insensitive.

For example:
`{{Vertigon.StatsAsTokens/MonstersKilled:player=local|monster=Green Slime}}` will be parsed as the number of slimes (all slimes except Big Slimes and Tiger Slimes are considered Green Slime for this purpose) killed by the local player.

Here is a complete list of monsters currently usable as arguments:

* `Green Slime`
* `Dust Spirit`
* `Bat`
* `Frost Bat`
* `Lava Bat`
* `Iridium Bat`
* `Stone Golem`
* `Wilderness Golem`
* `Grub`
* `Fly`
* `Frost Jelly`
* `Sludge`
* `Shadow Guy`
* `Ghost`
* `Carbon Ghost`
* `Duggy`
* `Rock Crab`
* `Lava Crab`
* `Iridium Crab`
* `Fireball`
* `Squid Kid`
* `Skeleton Warrior`
* `Crow`*
* `Frog`*
* `Cat`*
* `Shadow Brute`
* `Shadow Shaman`
* `Skeleton`
* `Skeleton Mage`
* `Metal Head`
* `Spiker`
* `Bug`
* `Mummy`
* `Big Slime`
* `Serpent`
* `Pepper Rex`
* `Tiger Slime`
* `Lava Lurk`
* `Hot Head`
* `Magma Sprite`
* `Magma Duggy`
* `Magma Sparker`
* `False Magma Cap`
* `Dwarvish Sentry`
* `Putrid Ghost`
* `Shadow Sniper`
* `Spider`
* `Royal Serpent`
* `Blue Squid`
\* not actually monsters, but stored in Data/Monsters. Probably will never be anything other than 0. 


Theoretically this token supports custom monster types as well, so long as they provide the game with a proper Name variable when instantiated.

### Upcoming Features
 * Track custom stats such as numbers of each type of food eaten
 * Track animals owned by players

If you have any issues:
Make sure SMAPI is up-to-date.
You can reach me on the Stardew Valley discord (Vertigon#1851) or on the Nexus mod page.
Please provide a SMAPI log, as well as your manifest.json, so that I can assist you better.

