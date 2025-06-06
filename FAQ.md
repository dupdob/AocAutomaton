# Intro
Welcome to the FAQ section of Aoc Automation. 
Here, you'll find answers to common questions about the project, its features, and how to use it effectively.

#### 1. What is Aoc Automation?
Aoc Automation is a framework designed to automate the process of solving Advent of Code puzzles. 
It provides a structured way to implement solvers, manage puzzle data, and handle input parsing.
It is also able to interact with the Advent of Code website to retrieve puzzle data and submit answers, thus
limiting the need for error-prone copy/pasting.

#### 2. How do I create a new solver?
To create a new solver, you simply add a class inhering from `AocAutomation.SolverWithParser`.
You need to override the Parse method to convert the input data into a format that your solver can work with.
Then, implement the `GetAnswer1` and `GetAnswer2` methods to compute the answers for the two parts of the puzzle.
One last thing: you must decorate it with the `DayAttribute` attribute to specify the day of the puzzle.


#### 3. How do I run my solver?
You can run your solver by using the `AocAutomation.Automaton` class. You must first create
the desired automaton instance, then use the `Run` method to execute your solver.
```[csharp]
internal class Program
{
    // this is the entry point of your program
    private void Main()
    {
        // create an automaton that interacts with the AoC website
        // you must provide the year of the puzzles you want to solve
        var automaton = Automaton.WebsiteAutomaton(2025);
        // run a specific day by providing the solver class
        automaton.RunDay<TheSolver>();
        // and voila, you're done. The automaton will take care of everything
    }
}
```

#### 4. How do I create other solvers?
To create other solvers, you create new classes similar to the first one.
And you run them by adjusting the type parameter of the `RunDay` method.

_Note: Future versions plan to provide auto discovery and selection but for now, 
you must run them explicitly._

#### 5 How do I test my solver against samples provided by AoC?
You must use the 'Example' attribute to specify the sample data for your solver.
This attribute should be applied to the GetAnswer1 or GetAnswer2 methods,
depending on which part of the puzzle you want to test.

##### 5.1 Simple case
The basic usage is `[Example("sample data", result)]` where "sample data" is the input you want to test against
and result is the expected output.

#### 5.2 Reuse
Recent AoC puzzles reuse the same sample data for both parts of the puzzle. If you
do not want to repeat the same sample data for both parts, you can use the `ReuseExample` attribute.
First, you must provide an 'id' to the `Example` attribute, like this 
`[Example(1, "sample data", resultForPart1)]` on GetAnswer1 method.
Then, you can use the `[ReuseExample(id, resultForPart2)]` attribute on GetAnswer2 method
to reuse the same sample data.

#### 6. Why do GetAnswer1 and GetAnswer2 methods return `object`?
AoC puzzles expect answers can be numerical (most of the time) but also sometimes textual.
To accommodate this, the methods return `object` so you can return any type of answer.
AoC Automation expects you to return a string or integer type.

##### 7. Why does AoC Automation refuse to submit my answer?
Aoc Automation will refuse to submit answer that are known to be wrong. There is no
override to this behavior. If you are sure your answer is correct, you can still
submit it manually on the Advent of Code website, then please open an issue.
AoC Automation will reject the following answers:
- `null`: null is considered as failed to compute an answer
- `""`: an empty string is considered as failed to compute an answer
- `0`: 0 is considered as a failed to compute an answer
- _any negative number_: there is no known puzzle that expects a negative answer
- _any already attempted answer_
- _any number that is greater than a previous answer that got a 'too high' response_
- _any number that is lower than a previous answer that got a 'too low' response_

Note that AoC Automation will report why it refused to submit your answer, 
so you can adjust it accordingly.


#### x. How do I handle exceptions in my solver?
Aoc Automation does not handle exceptions in your solver. They will be thrown as usual.
