namespace WebApiBackgroundJob
{
    public class Anyservices: Interface
    {
        public string printfromBackgroundJOb()
        {
            return "abcdef";
        }
    }

    public class MessageDto
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public string To { get; set; }
    }
}
