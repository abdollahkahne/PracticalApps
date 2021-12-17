#nullable disable
namespace ServerSide.Hubs
{
    public class MessageInput
    {
        public string Messsage { get; set; }
    }

    public class CounterInput
    {
        public int Count { get; set; }
        public int Delay { get; set; }
    }

}