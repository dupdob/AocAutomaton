# AocAutomaton
## TLDR;
AoCAutomaton provides skeleton classes and interfaces that
makes [Advent of Code](https://adventofcode.com/) even more enjoyable by 
simplifying mundane tasks, so you can concentrate on solving the puzzle.

## Details
AoCAutomaton implements an overall logic as well as some helpers.
The overall workflow is implemented by the `Automaton` class.
It will:
- create an instance of your solving logic
- fetch your specific input data (and cache them)
- tests your logic against sample/test data you may have provided
  (usually by copy pasting from the day's puzzle)
- if tests are not ok, it will stop. Otherwise
- runs your solving logic against your private input
- submit the answer to the AoC site
- get the result (and cache it) and provides you with the main message
- if this is not the good answer, it will stop. Otherwise
- it does the same for the second part of the question

The `Automaton` caches data to prevent any useless hammering of the AoC
site. Also, it automatically deals response delay and submit the answer
as soon as possible.

## How to use it
## Prerequisite
In order to interact with the AoC site, AoCAutomaton requires your AoC
session id. It is stored as an hexadecimal value in a `session` cookie;
you must get this value (via your browser of choice and store it in an environment variable named 
`AOC_SESSION`. Bear in mind that these tokens have a lifetime of roughly a month so expect
to have to refresh it through the website sometimes. As of now, there is limited error handling, so
you will get some exception if the token is invalid

_Note: early versions allowed to pass the session id as a set up parameter. I reverted that design due to the risk of having 
session ids with public visibility on GitHub (or elsewhere). It is more difficult to leak an environment variable._
