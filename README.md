# AocAutomaton
## TLDR;
AoCAutomaton provides skeleton classes and interfaces that
makes [Advent of Code][1] even more enjoyable by 
simplifying mundane tasks, so you can concentrate on solving the puzzle.
It is available as a Nuget package, named **AOC**.

## Details
AoCAutomaton implements an overall logic as well as some helpers.
The overall workflow is implemented by the `Automaton` class.
It will:
- create an instance of your solving logic
- fetch your specific input data (and cache them)
- tests your logic against sample/test data you may have provided (usually by copy pasting from the day's puzzle)
- if tests are not ok, it will stop. Otherwise
- runs your solving logic against your private input
- submit the answer to the AoC site
- get the result (and cache it) and provides you with the main message
- if this is not the good answer, it will stop. Otherwise
- it does the same for the second part of the question

The `Automaton` caches data to prevent any useless hammering of the AoC
site. Also, it **automatically deals with response delay and submit the answer** as soon as possible.
---- 
## Philosophy
**AocAutomaton** is designed to be used within your IDE of choice, focusing on **reducing friction when solving AoC puzzles**. Friction appears as the need to copy/paste your input data between the site and
your code (either in-line or as dedicated data files), but also when copy/pasting the answer or inputting it manually, which leads to the occasional typo error. 
One can also add dealing with provided examples to use as test/validation input.

**It does not try to provide** you with standard **algorithms** and/or helpers to simplify the **solving of the puzzle**. I consider this could spoil the fun for most users.
That being said, there is nothing wrong with building your own set of helper functions on top
of this library; I may publish one as well at a later date.
The important point is that **this is a separate concern** and will not be part of this library.

AocAutomation provides you an integration class that will implements interaction between your solver and the AoC site.
You just need to provide a class that contains the solving logic and implements a simple interface with only three methods.

Other abstract classes are provided to offer alternative interaction approaches, so you can pick the
design matching the puzzle characteristics and/or your preferences; and of course, you can even use your own design.
---- 
## How to use it
### Prerequisite
In order to interact with the AoC site, AoCAutomaton requires your AoC
session id. It is stored as an hexadecimal value in a `session` cookie;
you must get this value (via your browser of choice) and store it in an environment variable named 
`AOC_SESSION`. Bear in mind that these tokens have a lifetime of roughly a month so expect to have to refresh it through the website. As of now, there is limited error handling, so you will get some exception if the token is invalid

_Note: early versions allowed to pass the session id as a set up parameter. I reverted that design due to the risk of having session ids with public visibility on GitHub (or elsewhere). It is more difficult to leak an environment variable.

## Starting point
Just add AocAutomaton to the project you (plan to) use for AdventOfCode.
In the `main` method of your program, create an `Automaton` instance.
Write an `ISolver` implementation for the puzzle you plan to solve and 
use the `Automaton.RunDay` method that will:
1. Fetch your data input from the AoC website
2. Run your solver against every test data you have provided, if any
3. Run your solver for question 1 (if tests are successful)
4. Push the answer to AoC site
5. Report the response (success or failure)
6. Repeat with question 2 if question 1 was successful.

## Sample code

```[csharp]
internal class Program
{
    private void Main()
    {
        var automaton = new Automaton();
        automaton.RunDay<TheSolver>();
    }
}
...
internal class TheSolver : ISolver
{
  // provides the puzzle data
  public void SetupRun(Automaton automaton)
  {
    // set the day number (mandatory)
    automaton.Day = 12;
    // provides test data (optional)
    automaton.RegisterTestDataAndResult("test data", 12, 1);
  }
  
  // compute the answer to the first part
  public object GetAnswer1(string data)
  {
    // compute the answer
    ...
    return answer;
  }

  // compute the answer to the second part
  public object GetAnswer2(string data)
  {
    // compute the answer
    ...
    return answer;
  }
}
```
## Available command for the automation engine
### Behavioral
The following methods or properties can be used to modify the automaton behavior.
These methods or properties should be used during the setup phase, i.e. within your `ISolver.SetupRun` method implementation. Using them at some other time is not documented
and may lead to surprising results
#### Day
`int Day {get;set}`

Sets the day number. This is mandatory for interaction with the AoC website (if not set, an exception will stop the program).
#### ResetBetweenQuestions
`void ResetBetweenQuestions()`

When called, the automation engine will use separate (solver) instances for question 1 and 2. This can be helpful 
when the solver alter its initialization data.
Note that it implies that data will be (automatically) parsed again.

### Test Related
The following methods can be used to provide test data that the automation will use to check your solver before trying to solve the exercise with your AoC provided input.
AoCAutomaton support as many test examples as you may provide. It also supports providing example specific data for part one and two.

#### RegisterTestDataAndResult
`Automaton RegisterTestDataAndResult(string data, object expected, int question = 1);`

where

`data`: input data to be used for testing purposes
`expected`: the expected answer.
`question` : identifies for which question part the data must be used. `1` for the first part (default), `2` for the second part.

Provides input data for testing the solver as well as the expected answer, either for question 1 or 2.
You can register as much test data as you wish (memory permitting, of course). 
  
When you register test data and expected answer for question 1, you can provide the expected answer for question two via a subsequent call to `RegisterTestResult` (or via `AskVisualConfirmation`).

 
#### RegisterTestData
`Automaton RegisterTestData(string data, int question = 3);`

where

`data`: input data to be used for testing purposes
`question` : identifies for which question part the data must be used. `1` for the first part, `2` for the second part or `3` for both parts (default case).

Provides input data for testing the solver. By default this test data will be used for both part of the question, but you can specify which part this is for.
You can register as much test data as you wish (memory permitting, of course). 

Note that test data and test results must be registered in the same order, 
otherwise there will be mix ups.

#### RegisterTestResult
`public Automaton RegisterTestResult(object expected, int question = 1)`
where

`expected`: the expected answer.
`question` : identifies for which question part the data must be used. `1` for the first part or `2` for the second part.

Provides an expected result for testing the solver. Associated test data must have already been registered, otherwise an exception is raised, with one tolerance: 
you can register one (1) result for part two without providing test data for it. AoCAutomaton will automatically reuse the test data provided for part one. 

Note that test data and test results must be registered in the same order,
otherwise there will be mix ups.

#### RequestVisualConfirmation
`public Automaton AskVisualConfirmation(int question = 1)`
where


`question` : identifies for which question part the data must be used. `1` for the first part or `2` for the second part.

Provides an expected result for testing the solver. Associated test data must have already been registered, otherwise an exception is raised, with one tolerance: 
you can register one (1) result for part two without providing test data for it. AoCAutomaton will automatically reuse the test data provided for part one. 

Note that test data and test results must be registered in the same order,
otherwise there will be mix ups.

## Provided solver designs
You can choose any of the design of each of your solver.

### Original ISolver
This is the default interface, with only three methods. It is recommended when the data parsing
is straightforward, which is usually the case for the initial puzzles of each year.
This is also the interface than must be implemented by any custom design you may provide.

```[csharp]
public interface ISolver
{
  // provides the puzzle data
  void SetupRun(Automaton automaton);
  
  // compute the answer to the first part
  object GetAnswer1(string data);

  // compute the answer to the second part
  object GetAnswer2(string data);
}
```

. `SetupRun(Automaton automaton)` method: implement it to feed puzzle data (day and test values)
to the automaton
. `GetAnswer[1|2](string data)` methods: implement the input parsing and solving logic within 
each of these methods. Note than a solver instance will always receive the same `data` value
for each method.

### Solver with separated parser
This abstract class offers a method to parse data outside of the solving logic.
This design is perfect as soon as you want need to convert the input data to some
specialized structure that will be stored as fields/properties.
Parsing occurs only once per solver.

``` [Csharp]
// this class definition is for documentation purpose
// actual definition is different 
public class SolverWithParser
{
  // provides the puzzle data
  public abstract void SetupRun(Automaton automaton);
  
  // compute the answer to the first part
  public abstract object GetAnswer1();

  // compute the answer to the second part
  public abstract object GetAnswer2();
  
  // parse the puzzle data
  protected abstract void Parse(string data);
}
```

### Solver with line parser
This is a variant from the previous one. Most AoC puzzles use a data set
consisting of one line records. Parsing will be implemented via the `ParseLine` method.
It receives each line to parse, with its index and total number of lines.

```[csharp]
// this class definition is for documentation purpose
// actual definition is different 
public class SolverWithParser
{
  // provides the puzzle data
  public abstract void SetupRun(Automaton automaton);
  
  // compute the answer to the first part
  public abstract object GetAnswer1();

  // compute the answer to the second part
  public abstract object GetAnswer2();
  
  // parse a single line
  protected abstract void ParseLine(string line, int index, int lineCount);
}
```

[1]:	https://adventofcode.com/