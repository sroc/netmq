namespace Implementation.Interfaces
{
    public interface IActor
    {
        string ActorName {get; }
        string Address { get; }

        IActor AddActorName(string name);
        IActor AddHostName(string hostName);
        IActor AddPort(int port);
        IActor AddTopic(string topic);
    }
}
