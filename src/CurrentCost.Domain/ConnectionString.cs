namespace CurrentCost.Domain
{
    public class ConnectionString : BaseSetting
    {
        public string? DockerAppConnectionString { get; set; }
        public string? AppConnectionString { get; set; }

        public override string? GetAddress() => InDocker ? DockerAppConnectionString : AppConnectionString;
    }
}
