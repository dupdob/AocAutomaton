# Intro
Welcome to the FAQ section of Aoc Automation. 
Here, you'll find answers to common questions about the project, its features, and how to use it effectively.

### 1. What is Aoc Automation?
Aoc Automation is a framework designed to automate the process of solving Advent of Code puzzles. 
It provides a structured way to implement solvers, manage puzzle data, and handle input parsing.
It is also able to interact with the Advent of Code website to retrieve puzzle data and submit answers, thus
limiting the need for error-prone copy/pasting.

### 2. How do I create a new solver?
To create a new solver, you simply add a class inhering from `AocAutomation.SolverWithParser`.
You need to override the Parse method to convert the input data into a format that your solver can work with.
Then, implement the `GetAnswer1` and `GetAnswer2` methods to compute the answers for the two parts of the puzzle.
One last thing: you must decorate it with the `DayAttribute` attribute to specify the day of the puzzle.


### 3. How do I run my solver?
You can run your solver by using the `AocAutomation.Automaton` class. You must first create
the desired automaton instance, then use the `Run` method to execute your solver.
```csharp
internal class Program
{
    // this is the entry point of your program
    private void Main()
    {
        // create an automaton that interacts with the AoC website
        // you must provide the year of the  puzzles you want to solve
        var automaton = Automaton.WebsiteAutomaton(2025);
        // run a specific day by providing the solver class
        automaton.RunDay<TheSolver>();
        // and voila, you're done. The automaton will take care of everything
    }
}
```

### 4. How to parse the input data?
To parse the input data, you override the `Parse` method in your solver class, it receives the input data as a string.
Aoc Automation provides two string extension methods to help you parse the data:
- `SplitLines()` which splits a string into an array of lines, removing any trailing newline characters. 
You can then parse each line as needed.
Example usage:
```csharp
string input = "line1\nline2\nline3";
string[] lines = input.SplitLines();
// lines will contain ["line1", "line2", "line3"]
```

- `SplitBlocks()` which splits a string into an array of blocks, where each block is separated by two newlines (i.e an
  empty line). you can then parse each block as you see fit. Example usage:
```csharp
string input = "block1\nline1\nline2\n\nblock2\nline1\nline2";
List<string[]> blocks = input.SplitBlocks();
// blocks will contain {["block1", "line1", "line2"], ["block2", "line1", "line2"]}
```


### 5. How do I create other solvers? (_in the same project_)
To create other solvers, you create new classes similar to the first one.
And you run them by adjusting the type parameter of the `RunDay` method.

_Note: Future versions plan to provide auto discovery and selection but for now, 
you must run them explicitly._

### 6 How do I test my solver against samples provided by AoC?
That is a great question, and that is one of the main features of Aoc Automation.
You must use the 'Example' attribute to specify the sample data for your solver.
This attribute should be applied to the GetAnswer1 or GetAnswer2 methods,
depending on which part of the puzzle you want to test.

#### 6.1 Simplest case
The basic usage is `[Example("sample data", result)]` where "sample data" is the input you want to test against
and result is the expected output.

#### 6.2 Reusing sample data for part 2
Recent AoC puzzles usually reuse the same sample data for both parts of the puzzle. If you
do not wish to repeat the same  data for both parts, you can use the `ReuseExample` attribute.
First, you must provide an 'id' to the `Example` attribute, like this 
`[Example(1, "sample data", resultForPart1)]` on GetAnswer1 method.
Then, you can use the `[ReuseExample(id, resultForPart2)]` attribute on GetAnswer2 method
to reuse the same sample data.

### 7. Why do GetAnswer1 and GetAnswer2 methods return `object`?
AoC puzzles expect answers can be numerical (most of the time) but also sometimes textual.
To accommodate this, the methods return `object` so you can return any type of answer.
AoC Automation expects you to return a string or integer type.

### 8. Why does AoC Automation refuse to submit my answer?
Aoc Automation will refuse to submit answer that are known to be wrong. There is no
override to this behavior. If you are sure your answer is correct, you can still
submit it manually on the Advent of Code website, then please open an issue.
AoC Automation will reject the following answers:
- `null`, `""`, `0`: null, empty string and zero are considered as a failure to compute an answer
- _any negative number_: there is no known puzzle that expects a negative answer
- _any already attempted answer_: AoC automation assumes repeating the same answer will yield the same result, so
it will not submit it and reuse the result that was cached on first attempt.
- _any number that is greater than a previous answer that got a 'too high' response_
- _any number that is lower than a previous answer that got a 'too low' response_

Note that AoC Automation will report why it refused to submit your answer.

### 9. How do I handle puzzles with parameters?
Some AoC puzzles have one (or more) parameters on top of the puzzle input. See 
[AoC 2015 Day 14](https://adventofcode.com/2015/day/14): the sample provides an answer for an elapsed time of 1000
seconds, but your suppose to compute the answer for 2503 seconds.
The easiest way to handle this is to have your solver class inherit from `AocAutomation.SolverWithParam`.
This class provides overloaded `GetAnswer1` and `GetAnswer2` methods that accept an extra parameter.
You must specify the default value for the paramter in the signature of the method, and you can 
provide any customized value with the `Example` attribute(s). 
For Day 2014-14, it would look like this:
```csharp
[Day(14)]
public class ReinderOlympicsSolver : AocAutomation.SolverWithParam<int>
{

    // we provide an example for this part and add the actual paramter value
    [Example(1, @" Comet can fly 14 km/s for 10 seconds, but then must rest for 127 seconds.
    Dancer can fly 16 km/s for 11 seconds, but then must rest for 162 seconds.",
    1120, 
    // the parameter value is 1000 for this example
    1000)]
    public override object GetAnswer1(int elapsedTime = 2503)
    {
    ...
    }
    
    // this example reuses the same input data and parameter value.
    [ReuseExample(1, 689)]
    public override object GetAnswer2(int elapsedTime = 2503)
    {
    ...
    }
    
    public override void Parse(string data)
    {
    ..
    }
}
```
AoC Automaton will ensure the example data will use the provided parameter value
when running the solver (1000 seconds in this case), and the default value (2503 seconds) when running the actual puzzle.

In short, if your puzzle requires one parameter, you can use the `SolverWithParam<T>` class as this:
1) Inherit from `SolverWithParam<T>` where T is the type of the parameter (could be anything, but usually `int` or `string`)
2) Override the `GetAnswer1(T param = default)` and `GetAnswer2(T param = default)` methods, providing a 
default value for the parameter.
3) Use the `Example` attribute to provide sample data and expected results, including the parameter value
(as the last argument).

That's it! AoC Automation will handle the parameter for you, and you can run your solver as usual. 
Note that there is no compile time type check for the parameter value defined in the `Example` attribute. A type mismatch
will result in a runtime exception when the automaton tries to run your solver with the provided example data.

If you need two parameters, you can use the `SolverWithTwoParams<T1, T2>` class with the same approach.
No three parameters support is available at this time, you can use tuple instead.

### 10. How do I handle puzzles without an example?
Some AoC puzzles do not provide an example. Or to be more precise, the provided example covers only part of the problem.
For example, [AoC 2015 Day 21](https://adventofcode.com/2015/day/21) provides an example of combat between the player and
the monster, while the puzzle requires you to find the cheapest equipment to defeat the monster.
This should drive you to implement a 'combat simulation' method that could be tested against the example.

AoC Automation supports this via the `UnitTestAttribute` attribute that you can apply to any method in your solver class.
The syntax is:
```csharp
[UnitTest(result, param1, param2, ...)]
```
Where `result` is the expected result of the test, and `param1`, `param2`, etc. are the parameters to pass to the method.

Here is a signature matching the example above:
```csharp
[UnitTest(true, 8, 5, 5, 12, 7, 2)]
private static bool FightMonster(int hitPoints, int damage, int armor, int monsterHitPoints, int monsterDamage, int monsterArmor)
```
You can declare as many test case as you need on any method in your solver class.The method should be `static`, as all
unit tests will be run against the same instance of your solver class (in the current implementation).

When you run your solver, AoC Automation will automatically run all unit tests and report any failure.

### 11. How do I handle puzzle where one must recognize text?
Some AoC puzzles result in a low resolution image of a word
(see [Aoc 2019 Day 8, second part](https://adventofcode.com/2019/day/8)).
Aoc Automation does not provide any OCR capabilities, but there is an escape allowing a
human to perform the translation.
Mark the `GetAnswer1` or `GetAnswer2` method with the `VisualResultAttribute` attribute.
Such as:
```csharp
[VisualResult]
public override object GetAnswer1()
{
  ...
}
```
Aoc Automation will then display the result and ask you to provide the corresponding answer.
Note that behavior is the same for test and actual puzzle data.


### x. How do I handle exceptions in my solver?
Aoc Automation does not handle exceptions in your solver. They will be thrown as usual.
