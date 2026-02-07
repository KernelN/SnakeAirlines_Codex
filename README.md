# SnakeAirlines_Codex

Simple 2D Snake prototype for Unity 6.

## Scripts in `Assets/_Game`
- `Board`: Grid size, world conversion, wrapping, and free-cell lookup.
- `SnakeHead`: Handles movement, collisions, scoring triggers, and Move action subscription.
- `SnakeBody`: Owns snake cells and updates the trail renderer based on head commands.
- `FoodManager`: Spawns and tracks food.
- `ScoreManager`: Adds/subtracts score.
- `SnakeFood`: Holds food grid coordinates.

## Scene setup
1. Create an empty `Game` object and add these components:
   - `Board`
   - `FoodManager`
   - `ScoreManager`
2. Create `SnakeHead` GameObject with:
   - `SpriteRenderer`
   - `LineRenderer` (for body trail)
   - `SnakeHead`
   - `SnakeBody`
3. In `SnakeHead`, assign references to `Board`, `FoodManager`, `ScoreManager`, and `SnakeBody` from `Game`.
4. Set `SnakeHead > Move Action Reference` to `InputSystem_Actions` -> `Player/Move` (existing action in `Assets/InputSystem_Actions.inputactions`).
5. Create a `Food` prefab with:
   - `SpriteRenderer`
   - `SnakeFood`
6. Assign the `Food` prefab to `FoodManager`.

## Controls (Unity Input System)
- Movement comes from the existing `Player/Move` Input Action.
- Default bindings in the provided input asset already include WASD and arrow keys.

## Collision rule
If the head reaches a body segment, the snake trims from that collision point to the tail, and score is reduced by the equivalent removed segment amount instead of ending the game.
