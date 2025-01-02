# Changelog for AoC Automaton

# V0.5

## New methods, classes

### 1. Automaton
   - Allow to request visual confirmation for tests via `AskVisualConfirm(...)` method.

### 2. SolverWithBlockParser
   
New solver template (the one you can inherit from to solve some AoC exercise). It exposes input data
as a series of empty line separated block (such as for [Day 15-2024](https://adventofcode.com/2024/day/15)).
You need to override `ParseBlock(List<string> data, int blockIndex)` where _data_ is the block as a list of strings and
_blockIndex_ is the 0 base index for this block (for Dat 15-2024, the map is block **0** and the instruction is a single
as block **1**).


## Implementation features
- Assume Day is current (only during December month). Note that is still recommended to set the Day explicitly
- Automatically request visual confirmation when no expected result has been provided for part two and tests data are present
- Skip part two for Day 25 as there is no part two.
- When testing part two, skips part one when:
    1. No expected result provided for part one
  2. The automaton must be reset between questions
  3. Visual confirm is not activated (for this data)


## Fixes



# The one with a fix(V 0.2.2)
- Question is flagged as solved in the state appropriately

# The one with a state at the right place(V 0.2.1)
- Store the state in the configured cache folder (if any)
# The one with a state (V 0.2)
- Implements a persisted state cache that stores the exercise state, including if it has been solved or not, the correct answer (if found) and past attempted answers
The state logic will be leveraged upon in future releases for extra features (such as using too high/too low responses from AoC, or skipping question 1 altogether when possible).
# First release (V 0.1)
- Implements an automated workflow for AoC exercises, including a test step where the algorithm is tested via provided examples.
- provides several APIs to register test data and expected results
- integrates with AoC website by pushing answers and retrieving results
- cache previous attempts for performance reason
