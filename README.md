### Stats As Tokens
*by Vertigon*



This mod allows Content Patcher pack creators to access all stats tracked by the game through custom CP tokens, allowing for patches to trigger
on various player milestones including number of seeds sown, truffles found, trash cans checked and many more! See below for a complete list.

### Usage
Download this mod, place it in your Mods folder, and list it as a dependency in your `manifest.json` in order to access its custom tokens.

### Dependencies
* [SMAPI](https://smapi.io/)  v3.12.0 or higher is a required dependency.
* [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) v1.23.0 or higher is a required dependency.

### Custom Tokens Provided

Only one custom token is provided: **`Vertigon.StatsAsTokens/Stats`**

This token takes exactly two named arguments (both must be provided in order for it to work):
* **`player`**: Must be one of the following:
  * `host`: The player hosting the lobby, or
  * `local`: The player on the local splitscreen or computer, if not the host
* **`stat`**: The stat to track. See below for a complete list.

For example:
`{{Vertigon.StatsAsTokens/Stats:player=host|stat:diamondsFound}}` will be parsed as the number of diamonds found by the host player.

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

### Upcoming Features
 * Track numbers of each type of monster slain
 * Track custom stats such as numbers of each type of food eaten

If you have any issues:
Make sure SMAPI is up-to-date.
You can reach me on the Stardew Valley discord (Vertigon#1851) or on the Nexus mod page.
Please provide a SMAPI log, as well as your manifest.json, so that I can assist you better.
