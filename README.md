# Pyro
A Collection of libraries, mostly used for PyroNc, a G-code simulation software I'm building.

# Pyro.IO.Mods
>Intended for adding plugins to .NET software at runtime.

# Pyro.IO
>General library intended to provide utilities for LINQ and General IO.

# Pyro.Math
>Provides various methods for calculating newton's forces (Pyro.Math.Physics), Plotting the points of a circle based on the n number of points, Plotting lines ( segmented into n parts ), and other basic mathemathical functions such as Sin, Cos, Tan, Arc_Sin, Arc_Cos, Arc_Tan...  
>TODO: CUDA Implementation

# Pyro.Nc
>The core of Pyro; This assembly contains code required to manage a tool for CNC Simulation, ranging from parsing valid GCODE strings fetched from Unity's InputField instance,  converting that ***GCODE*** string to ***ICommand*** instances which later get executed by the tool using the ***ICommand.Execute(bool draw)***, the boolean 'draw' signifying the ability for the command to draw it's path in the case of ***GCommands*** which move the tool in order to cut or position. 
>This assembly requires a .txt file located in *C:\Users\X\AppData\Local\PyroNc\commandId.txt*, which contains all the required ID's that Pyro.Nc's GCODE parser uses to identify ***ICommands***.
>>If this file is missing, an error is thrown whilst initializing the class ValueStorage (The class containing dictionaries constisting of all ICommands) and no code can be parsed or executed!  
>>This file is automatically added to the defined path at ***compile time***.



