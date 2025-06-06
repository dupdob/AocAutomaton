
_Note_: this document is currently a dump of the old documentation. It mostly concerns now obsolete methods.


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
