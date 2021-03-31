# Difficulty of embedding
## Lua 
- Requires 2 LOC for Hello World, but only 1 LOC for initialization
- Is self contained

Code:
```cs
using var lua = new NLua.Lua();
lua.DoString("print('Hello, World!')");
```

## Python
- Requires 4 LOC for Hello World (3 LOC for initialization)
- Is not self-contained => Requires having python installed

```cs
Runtime.Runtime.PythonDLL = "python39.dll";
using var state = Py.GIL();
using var scope = Py.CreateScope();
scope.Exec("print('Hello, World!')");
```