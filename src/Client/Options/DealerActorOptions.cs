namespace Client.Options
{
    public sealed class DealerActorOptions
    {
        public const string DealerActor = "DealerActor";

        public string ActorName { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
