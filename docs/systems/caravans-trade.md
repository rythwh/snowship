# Caravans & Trade

## Key files
| File | Role |
|---|---|
| `Assets/Code/Caravan/CaravanManager.cs` | Each game minute increments `caravanTimer`; after 7 days there is a growing chance per minute, and by 27 days it is certain. Then `SpawnCaravan(Foot, 4)` picks a walkable edge spawn and a trading post (or a random tile) in that region as the target |
| `Assets/Code/Caravan/Caravan.cs` | Traders, generated `Location`, an inventory stocked from a random `ResourceGroup`, resources to trade, the leave timer |
| `Assets/Code/Caravan/Trader.cs` | A `Human` that wanders near the target; when it has trading posts to visit, it collects reserved trade resources and then creates a `CollectResourcesJob` for the colony |
| `Assets/Code/Caravan/Location.cs` | Random origin (name, wealth, resource richness, city size, biome) |
| `Assets/Code/Resource/Models/Objects/ObjectInstanceTypes/TradingPost.cs` | Container subclass acting as the exchange point |
| `Assets/Code/UI/Simulation/MVPE/TradeMenu/` | Trade UI; `TradeResourceAmount` / `ConfirmedTradeResourceAmount` |
| `Assets/Code/Job/Jobs/TaskJobs/TradeJob.cs` | Job that walks a colonist to a trader |

Debug commands `spawncaravans` and `opentrademenu` exist in `DebugManager`.

## Open issues
CAR-1, INPUT-1 (`CaravanManager.Tick` uses `Input.GetMouseButtonDown`), RES-5, MAT-2. See [issues.md](../issues.md).
