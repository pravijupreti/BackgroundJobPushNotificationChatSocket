namespace WebApiBackgroundJob
{
    public interface IFirebaseNotification
    {
        Task<bool> NotifyAsync(string to, string title, string body);
    }
}
