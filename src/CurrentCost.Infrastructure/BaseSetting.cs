namespace CurrentCost.Infrastructure;

public abstract class BaseSetting
{
    public bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    public abstract string GetAddress();

}
