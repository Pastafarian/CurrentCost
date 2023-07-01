namespace CurrentCost.Infrastructure;

public class SeqSettings : BaseSetting
{

    
    /// <summary>
    /// Seq Host Address. By default set to "localhost".
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Seq Host Address. By default set to "seq".
    /// </summary>
    public string DockerHost { get; set; } = "seq";

    /// <summary>
    /// Event Bus Port. By default set to 5341.
    /// </summary>
    public int Port { get; set; } = 5341;

    public override string GetAddress()
    {
        return $"http://{(InDocker ? DockerHost : Host)}:{Port}";
    }
}