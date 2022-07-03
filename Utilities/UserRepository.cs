using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Utilities
{
    public class UserRepository
    {
        public async Task<bool> SendEmailForCodeVerificationAsync(int verificationCode, string toAddress, string username, string uri)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                var body = $"{{\"Email\": \"{toAddress}\",\"Subject\":\"[LG Merchandiser] Email Verification Code\",\"Username\":\"{username}\",\"OTP\":\"{verificationCode}\"}}";
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Content = content;
                var response = await MakeRequestAsync(request, client);
                var authenticationModel = JsonConvert.DeserializeObject<AuthenticationModel>(response);

                if (authenticationModel.status.Equals("success"))
                {
                    return true;
                }
                else
                {
                    
                    return false;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                throw new Exception();
            }
        }
        public async Task<bool> SendEmailForlocationVerificationAsync(string storeName,string messagefromMerchandiser, string username, string location, string uri)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                var body = $"{{\"StoreName\": \"{storeName}\",\"Message\":\"{messagefromMerchandiser}\",\"Username\":\"{username}\",\"Location\":\"{location}\"}}";
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Content = content;
                var response = await MakeRequestAsync(request, client);
                var authenticationModel = JsonConvert.DeserializeObject<AuthenticationModel>(response);

                if (authenticationModel.status.Equals("success"))
                {
                    return true;
                }
                else
                {

                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception();
            }
        }
        
        public async Task<string> MakeRequestAsync(HttpRequestMessage getRequest, HttpClient client)
        {
            var response = await client.SendAsync(getRequest).ConfigureAwait(false);
            var responseString = string.Empty;
            try
            {
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                // empty responseString
            }

            return responseString;
        }
    }

    public class AuthenticationModel
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}
