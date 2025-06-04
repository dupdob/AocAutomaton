# AocAutomaton
## TLDR;
AoCAutomaton provides skeleton classes and interfaces that
makes [Advent of Code][1] even more enjoyable by 
simplifying mundane tasks, so you can concentrate on solving the puzzles.
It is available as a Nuget package, named **AOC2**.

## Details
AoCAutomaton implements an overall logic as well as some helpers.
The overall workflow is implemented by the `Automaton` class.
It will:
- create an instance of your solving logic
- fetch your specific input data (and cache them)
- tests your logic against sample/test data you may have provided (usually by copy-pasting from the day's puzzle)
- if tests are not ok, it will stop there allowing you to fix your code.
- otherwise it runs your solving logic against your specific input
- submits the answer to the AoC site
- gets the result (and cache it) and provides you with the main message (success or failure kind)
- if this is not the good answer, it will stop. Otherwise
- it does the same for the second part of the question

The `Automaton` caches data to prevent unnecessary hammering of the AoC
site. Also, it **automatically deals with response delay and submit the answer** as soon as possible. It will also
avoid submitting known wrong answers, such as null, zero or negatives values, previously tried answers or answers that
are outside the known range (when you got 'answer is too low' or 'too high' results).
---- 
## Philosophy
**AocAutomaton** is designed to be used within your IDE of choice, focusing on **reducing friction when solving AoC puzzles**. 
Friction appears as the need to copy/paste your input data between the site and
your code (either in-line or as dedicated data files), but also when copy/pasting the answer or inputting it manually, 
which leads to the occasional typo error. AocAutomaton also helps you verify that your solution works against the
provided examples; and it also provides a minimal unit test logic so you can check methods in isolation

**It does not provide** standard **algorithms** and/or helpers to simplify the **solving of the puzzles**.
I think this would spoil the fun. Of course, you are free to use any package you find suitable to help you
find the solution if you are so inclined.

**AocAutomation** provides you an integration class that implements interaction between your solver and the AoC site.
You just need to provide a class that contains the solving logic and implements a simple interface with only three methods.

**AocAutomation** use several **Attributes** for you to provide needed specifications. This is different from alpha
versions that required you to implement the abstract `SetUp` method in order to provide these specifications via method 
calls.

Other abstract classes are provided to offer alternative interaction approaches, so you can pick the
design matching the puzzle characteristics and/or your preferences; and of course, you can even use your own design.
---- 
## How to use it
### Prerequisite
In order to interact with the AoC site, AoCAutomaton requires **your AoC
session id**. It is stored as a hexadecimal value in a `session` cookie;
you must get this value (via your browser of choice).
Then you have two options:
1. Safest: store it in an environment variable named `AOC_SESSION`.
2. Input it when prompted by the automaton. 
This is not recommended as it will be visible in the console and may be stored in your IDE history.
The token will be stored in a json file (plain text). As a safety precaution, 
AoCAutomaton will add a .gitignore entry to prevent this file from being committed to a repo.
 
Bear in mind that these tokens have a lifetime of a month so expect to have to refresh it through the website.
As of now, there is limited error handling, so you should except some exception if the token is invalid or expired.

_Note_: early alpha versions allowed to pass the session id as a setup parameter (in code).
I reverted that design due to the risk of having session ids with public visibility on GitHub. 

## Starting point
Just add AoC2 package to the project you (plan to) use for AdventOfCode.
In the `main` method of your program, create an instance of the `Automaton` class, such as.
`var automaton = Automaton.WebsiteAutomaton(2025);`

Create a class that inherits from `SolverWithParser` class, such as `MySolverDay1`. You will have to override the following methods:
- `Parse(string data)` to parse the input data and store it in fields/properties as you see fit.
- `GetAnswer1()` to compute the answer to the first part of the question.
- `GetAnswer2()` to compute the answer to the second part of the question.

Then call the `RunDay` method of the `Automaton` instance, passing your solver class as a type parameter.
`automaton.RunDay<MySolverDay1>();`

When you run your program, the automaton will take care of everything:
1. Fetch your data input from the AoC website
2. Run your solver against every test data you have provided, if any
3. Run your solver for question 1 (if tests are successful)
4. Push the answer to AoC site
5. Report the response (success or failure)
6. Repeat with question 2  (test and submission) if question 1 was successful.

## Sample code
Here is a simple example of a program that uses the automaton to solve the Day 1 puzzle of the 2019 Advent of Code.
Warning: Minor Spoilers ahead. This is not the actual solution, but a sample code
with a slightly different formula from the actual exercise to illustrate how to use the automaton.

```[csharp]
internal class Program
{
    // this is the entry point of your program
    private void Main()
    {
        // create an automaton that interacts with the AoC website
        var automaton = Automaton.WebsiteAutomaton();
        // run a specific day solver
        automaton.RunDay<TheSolver>();
        // and voila, you're done. The automaton will take care of everything
    }
}
...
// Your need to write a class to write your solving logic
// Specify the day number via the DayAttribute
[Day(1)]
public class TheSolver: SolverWithParser
{
    private int[] _masses;

    protected override void Parse(string data) 
        => _masses = data.SplitLines().Select(int.Parse).ToArray();

    [Example("1969", 654)]
    [Example(1, "100756", 33583)]
    public override object GetAnswer1() 
        => _masses.Aggregate<int, long>(0, (current, mass) => current + ComputeFuelForMass(mass));

    [ReuseExample(1, 50346)]
    public override object GetAnswer2() 
        => _masses.Aggregate<int, long>(0, (current, mass) => current + ComputeFuelForMass(mass, true));

    private static long ComputeFuelForMass(int mass, bool withFuel = false)
    {
        if (mass <= 14)
        {
            return 0;
        }

        var fuel = mass / 7 - 2;
        return withFuel ? fuel + ComputeFuelForMass(fuel, true) : fuel;
    }
}
```

Let's break down the code:
### Solver class definition
```[csharp]
[Day(1)]
public class TheSolver: SolverWithParser...
```
We define a class that solves the puzzle for day 1. 
We provide the day number via the `DayAttribute`. This is required so the automaton 
where to get input data and post your answer.

### Parsing the input data
```[csharp]
protected override void Parse(string data) 
    => _masses = data.SplitLines().Select(int.Parse).ToArray();
```
This method is called once per solver instance, and it is where you parse the input data, provided as a string.
For this puzzle, we split the input data into lines (`SplitLines` extension method), parse each line as an integer and
store the result in a private field `_masses`.

### Solving the first part
Let us disregard the method attributes for now and focus on the method itself:
```[csharp]
public override object GetAnswer1() 
    => _masses.Aggregate<int, long>(0, (current, mass) => current + ComputeFuelForMass(mass));
```
This method computes the answer to the first part of the puzzle. Here, we use the `Aggregate` LINQ method to
sum the fuel requirements for each mass in the `_masses` array. The `ComputeFuelForMass` method is a helper
that calculates the fuel needed for a given mass.

### Providing examples
```[csharp]
[Example("1969", 654)]
[Example(1, "100756", 33583)]
public override object GetAnswer1()... 
```
Now, coming back to the method attributes, 
We can see that they are used to provide examples for the first part of the puzzle. 
Those examples are straight from the AoC site.
The automaton will use these  to test your solver logic before running it against the actual input data.
The `Example` attribute has several signatures. The first one here is the simplest, 
where the first parameter is the input data ("1969") and the second parameter is the expected result (654).
The second example uses the day number (1) as the first parameter, which allows the automaton to reuse the same example
for both parts of the puzzle as we will see later.

### Solving the second part
```[csharp]
[ReuseExample(1, 50346)]
public override object GetAnswer2() 
    => _masses.Aggregate<int, long>(0, (current, mass) => current + ComputeFuelForMass(mass, true));
```
This method computes the answer to the second part of the puzzle, in a similar way to the first part, with a slightly
different logic in the `ComputeFuelForMass` method (it now takes into account the fuel needed for the fuel itself).

The `ReuseExample` attribute is used here to reuse the example from the first part of the puzzle. It takes the example
id (1) and the expected result (50346) as parameters. In this case, this is equivalent to `[Example("100756", 50346)]`.
The 'ResueExample' attribute is useful when an example value is large or complex, and is resued between the two parts of the puzzle.

### Helper method
```[csharp]
private static long ComputeFuelForMass(int mass, bool withFuel = false)...
```
This is a private helper method that is specific to this puzzle, it is here to remind you that you are free to structure
your solver class as you see fit. You can add any number of private or public methods, fields, properties, etc.


**That's it!**, you are now ready to write your own solver classes and use the automaton to solve the puzzles.
The rest of this dodcumentation will provide more details on the available features and how to customize your solvers.

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