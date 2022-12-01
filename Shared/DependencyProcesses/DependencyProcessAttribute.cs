namespace ProjectStatistics.DependencyProcesses;

public class DependencyProcessAttribute<T> : Attribute where T : IDependencyProcess
{
    public Type Type { get; } = typeof(T);
}