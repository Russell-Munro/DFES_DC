using System;

public static class ServiceLocator
{
    public static IServiceProvider Instance { get; set; }
}

// Usage anywhere:
//var dbContext = ServiceLocator.Instance.GetRequiredService<DatabaseContext>();