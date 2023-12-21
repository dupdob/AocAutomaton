# Changelog for AoC Automaton

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
- design is asynchronous to speed up execution. For example, user input is retrieved from the web alongside algorithm tests