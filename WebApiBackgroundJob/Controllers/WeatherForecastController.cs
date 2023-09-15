using Hangfire;
using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin.Messaging;
using Firebase.Database.Offline;
using System.Runtime.CompilerServices;

namespace WebApiBackgroundJob.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
        private readonly Interface _interface;
        private readonly ILogger<WeatherForecastController> _logger;
        public readonly IFirebaseNotification _firebaseNotification;
        public WeatherForecastController(IFirebaseNotification firebaseNotification, Interface @interface, ILogger<WeatherForecastController> logger)
        {
            _interface = @interface;
            _logger = logger;
            _firebaseNotification = firebaseNotification;
            if (FirebaseApp.DefaultInstance == null)
            {
                var credential = GoogleCredential.FromFile("E:\\WebApiBackgroundJob\\WebApiBackgroundJob\\Firebase_Admin_sdk.json");
                var firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
            }
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            BackgroundJob.Enqueue(() => _interface.printfromBackgroundJOb());
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("FCM")]
        public async Task<IActionResult> Firebase(string title, string body, string to)
        {
            return Ok(await _firebaseNotification.NotifyAsync(to, title, body).ConfigureAwait(false));
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
        {
            // Store the message in Firebase Realtime Database
            // You can use the FirebaseAdmin SDK to interact with the database
            // Example code:
            var firebaseClient = new FirebaseClient("https://fir-auth-5f97e-default-rtdb.asia-southeast1.firebasedatabase.app");
            await firebaseClient.Child("messages").PostAsync(messageDto);

            //Notify the client applications about the new message using Firebase Cloud Messaging(optional)
             //Example code:
             var message = new Message
             {
                 Topic = "new-message",
                 Data = new Dictionary<string, string>
                 {
                     { "messageId", messageDto.Id },
                     { "senderId", messageDto.SenderId },
                     { "content", messageDto.Content }
                 }
             };
            await FirebaseMessaging.DefaultInstance.SendAsync(message);

            return Ok();
        }

        [HttpGet("GetMessage")]
        public async Task<IActionResult> GetMessagesAsync()
        {
            // Retrieve chat messages from Firebase Realtime Database
            // Example code:
            var firebaseClient = new FirebaseClient("https://fir-auth-5f97e-default-rtdb.asia-southeast1.firebasedatabase.app");
            var currentUserId = "string";
            var messages = await firebaseClient
            .Child("messages")
            .OrderBy("To")
            .EqualTo(currentUserId)
            .OnceAsync<MessageDto>();

            // You can format the messages in a desired way and return them
            // For simplicity, we'll return the raw messages
            return Ok(messages.Select(m => m.Object));
        }

        [HttpGet("SetSentData")]
        public async Task<IActionResult> GetSentMessagesAsync([FromQuery]List<string> userIds)
        {
            // Retrieve chat messages from Firebase Realtime Database
            // Example code:
            var firebaseClient = new FirebaseClient("https://fir-auth-5f97e-default-rtdb.asia-southeast1.firebasedatabase.app");
            var MessagesList = new List<MessageDto>();
            foreach (var user in userIds)
            {
                var senderMessages = await firebaseClient
                .Child("messages")
                .OrderBy("SenderId")
                .EqualTo(user)
                .OnceAsync<MessageDto>();

                var recipientMessages = await firebaseClient
                    .Child("messages")
                    .OrderBy("To")
                    .EqualTo(user)
                    .OnceAsync<MessageDto>();
                if (senderMessages.Count != default)
                {
                    MessagesList.AddRange(senderMessages.Select(m => m.Object));
                }
                if(recipientMessages.Count != default)
                {
                    MessagesList.AddRange(recipientMessages.Select(m => m.Object));
                }
            }

            var specificUserMessages = MessagesList.Where(x =>
    (userIds.Contains(x.SenderId) && userIds.Contains(x.To)) ||
    (userIds.Contains(x.To) && userIds.Contains(x.SenderId)))
    .ToList().DistinctBy(x=>x.Content);

            return Ok(specificUserMessages);
        }
    }
}