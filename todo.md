
RTS Proto

Stream #1:
- Set up grid
- Camera movement
- Placeable buildings (only 1x1 size)
- Buildings can't be placed on top of existing buildings
- Build preview rendering
- Health
- Turrets shooting each other

Stream #2:
- // ref returns
- // make preview green/red
- // pick a building for placement, basic UI
- // construction progress
- // money
- // mobile units
- // giving move orders to units
- // unit rect selection

Stream #3:
- // Created a public repo
- // Refactor production into a separate class
- // Refactor guns so that units can shoot just like turrets
- // Add different sized buldings
- // Make tanks
- // Unit queues
- // Cookie clicker resource "gathering"

Stream #4:
- // Move camera by the edges of the screen
- // Drawing improvements:
	- // Add NDraw to project for drawing lines
	- // Nicer selection graphics (with NDraw)
- // Force destroy units on delete
- // Selecting buildings
- // Fix bug: Cancel troops in queue when all buildings that can produce it are destroyed
- // Cancel unit build on right click in UI
- // Better bullets graphics
- // Fix: Turrets should stop firing when out of range

Stream #5:
- // Disable edge scrolling in editor
- Factions - BIG REFACTOR
	- // Split input from construction
	- // Units need to belong to Factions (add to Health script? (renamed to Entity!))
	- // Building call Destroy on the faction
	- // Per-faction closest finding (skipped this)
	- // Make faction units fight each other
	- // Make enemy units unselectable
	- // Faction graphics, red and blue
- // Increase radius of selection circle on tanks
- // Formation movement
- // Unit info UI
- // Health graphics
- Building rally points

- SelectAllUnits
- Select all units on screen by type on double click
- Scatter
- Attack orders