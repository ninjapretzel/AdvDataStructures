# AdvDataStructures
Some implementations of advanced data structures.

- [How to run](#How-to-run)  
- [Directories](#Directories)  
	- [Leftist Heap](#lheap)  
	- [Van Emde Boas Tree](#vanEmdeBoasTree)  

## How to run:
All of the folders will be C# projects that can be run with the `dotnet` command line program.  
This can be installed from: https://dotnet.microsoft.com/download  
For windows and mac, install is trivial. Linux might have extra steps to make the installed program usable from the command line, add to path if neccessary.

Once installed and the `dotnet` command is available, to run, simply:
```
cd {dir}
dotnet run
```

## Directories:

### lheap
"Leftist Heap", a very simple tree structure that satisfies the heap property, with a rule that makes unioning very fast, and uses union as the simplest operation.

### vanEmdeBoasTree
A tree with a recursive `sqrt(n)` structure and `log(log(n))` time for operations.
