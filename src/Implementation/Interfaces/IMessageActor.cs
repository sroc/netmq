using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Implementation.Interfaces
{
    public interface IMessageActor
    {
        IMessageActor AddActorName(string name);
        IMessageActor AddHostName(string hostName);
        IMessageActor AddPort(int port);
        IMessageActor AddTopic(string topic);
    }
}
