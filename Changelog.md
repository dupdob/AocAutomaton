# Changelog for AoC Automaton

# V 1.0 
## Overview
### Documentation
A FAQ as be made available to help you get started with AoC Automaton: [FAQ](FAQ.md).
[Documentation](Documentation.md) file has been cleared as its content wad no longer relevant due to the new architecture.
A new documentation will be provided in the future.
AocAutomation mainly relies on Attributes to configure the solver, provides examples and their associated expected
results, unit tests, workflow options etc...
The FAQ provides details on how to use them.

### Extra parameters
Add support simpler support for puzzles with extra parameters, such as the one for [Day 14-2015](https://adventofcode.com/2015/day/14). 
Just ensure your solver class inherits from `SolverWithParam<T>` (or `SolverWithParams<T,TU>` for two parameters), 
implement the `GetAnswer1(T param)` (or `GetAnswer1(T param1, TU param2)`) providing the default value for the
parameter(s) in the signature. And you need to provide the appropriate value(s) for these parameter(s) for 
examples such as:
```csharp
// we have an extra parameter of type int, with a default value of 1000
// and a specific value of 165 for the example input
[Example("example input", 1000, 165)]
public override object GetAnswer1(int param = 1000)
{
    // your logic here
}
```


### Parsing input data
Add string extension methods to simplify the parsing of input data:
- `SplitLines()` which splits a string into an array of lines, removing any trailing newline characters. Example usage:
```csharp
string input = "line1\nline2\nline3";
string[] lines = input.SplitLines();
// lines will contain ["line1", "line2", "line3"]
```

- `SplitBlocks()` which splits a string into an array of blocks, where each block is separated by two newlines (i.e an 
empty line). Example usage:
```csharp
string input = "block1\nline1\nline2\n\nblock2\nline1\nline2";
List<string[]> blocks = input.SplitBlocks();
// blocks will contain {["block1", "line1", "line2"], ["block2", "line1", "line2"]}
```

### Unit tests
Add unit test capability via the `UnitTest` attribute. This allows you to define unit tests for any solver method.
See the FAQ for more details on how to use it.

### 

# V 0.9
## Overview
New architecture which allows to select the interactive mode. Either http/automatic (existing), or console/manual (new), depending on your
preference.

The change of architecture resulted in the removal of some classes and some methods' signatures may have changed since
earlier versions.
This version is considered to be close to V1.0, hence APIs are considered as stable. As such, Aoc Automaton will
adhere to semantic versioning (semver) for future versions.
The handling of extra parameters have been redesigned. New solver bases classes are also available.

## New attributes
AoC automaton now allows you to use attributes to specify core information about your solver, which is simpler
and more elegant than the previous approach via a dedicated setup. 
You use `[Day(x)]` (where x is the exercise's day you are solving) on your solver class, and you can use 
`[Example(input, expected)]` on the `GetAnswer1()` and `GetAnswer2()` methods to provide test values.
You can also use `[Example(id, input, expected)]` on `GetAnswer1()` and 
`[PreviousExample(id, expected)]` on `GetAnswer2()` to reuse an input value from part 1.


## Behavior changes
The automaton persist its cache, which stores question status (solved or not), past attempts and low/high mark when
available. 
The automaton automatically rejects any answer that:
1) is null (assume no answer provided)
2) is zero or negative
3) has already been unsuccessfully attempted
4) is higher than the high mark or lower than the low mark

### SolverWithParser
This is the base implementation for every solver. Now it removes the trailing newline symbol from the input. As such
single line input data are easier to process.

## New methods, classes
### ConsoleUserInterface
This class implements console based interaction. In short, it asks for console input for your exercise data and simply 
display the computed result, for you to copy-paste into AoC website.

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

### 3. SolverWithDataAsLines
New solver template that exposes input data as a list of lines (`string`s). You need to override `ParseLines(string[] lines)`
and parse the data appropriately. _Note: this is now the base class for all Solver templates._

## Implementation features
- Assume Day is current (only during December month). Note that is still recommended to set the Day explicitly
- Automatically request visual confirmation when no expected result has been provided for part two and tests data are present
- Skip part two for Day 25 as there is no part two.
- When testing part two, skips part one when:
  1. No expected result provided for part one
  2. The automaton must be reset between questions
  3. Visual confirm is not activated (for this data)


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
