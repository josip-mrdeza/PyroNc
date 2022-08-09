# Pyro
A Collection of libraries, mostly used for PyroNc, a G-code simulation software I'm building.

# PyroNc

## CommandHelper.cs
  
>### Important methods  
>
>>***List<string[]> IdentifyVariables(this string[] splitCode)***
>>>This function takes a line of G-Code split into pieces (preferably with a empty char separator).
>>>It collects indicies of known functions (Fetched from ValueStorage's dictionaries), 
followed by attempting to string together the known function's parameters from the line.
It returns a List<string[]> signifying that each string array in the list is one 'function', first element is the actual function ID, followed for all it's parameters.
>  
>
>>***List<ICommand> GatherCommands(this List<string[]> arrOfCommands)***
>>>This function takes the last function's output and tries to fetch the required copies of functions from the ValueStorage's dictionaries, 
then it attempts to assign the functions their parameters or, if a comment char is present in one of the string arrays, it will create a comment containing a string of all text
after the comment char. Afterwards it returns a List of commands that have been read.
>
>
>>***async Task<ValueStorage> CreateFromFile(ITool tool)***

