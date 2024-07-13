using ExecutionScope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope;

[assembly: Parallelize(Workers = 8, Scope = ExecutionScope.ClassLevel)]