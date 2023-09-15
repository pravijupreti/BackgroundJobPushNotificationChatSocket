using Microsoft.AspNetCore.Builder.Extensions;
using Newtonsoft.Json;
using System.Text;

namespace WebApiBackgroundJob
{
    public class FirebaseNotification : IFirebaseNotification
    {
        public async Task<bool> NotifyAsync(string to, string title, string body)
        {
            try
            {
                //Server key from FCM console
                var serverKey = string.Format("key={0}", "b58af07d383edfdc942cbeef6a03f7bf79f84caa");

                //Sender id from FCM console
                var senderId = string.Format("id={0}", "151042347594");

                var data = new
                {
                    to, // Recipient device token
                    notification = new { title, body }
                };

                // Using Newtonsoft.Json
                var jsonBody = JsonConvert.SerializeObject(data);

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send"))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
                    httpRequest.Headers.TryAddWithoutValidation("Sender", senderId);
                    httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        var result = await httpClient.SendAsync(httpRequest);

                        if (result.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            // Use result.StatusCode to handle failure
                            // Your custom error handler here
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return false;
        }
    }
}
