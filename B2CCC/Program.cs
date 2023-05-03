using IdentityModel.Client;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleClientAD.Native
{
    public class ModelBasic
    {
        public string access_token { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //CCMsal();

            //CChttpClient();

            //ROPC();

            Console.WriteLine("Hello World!");
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/azure/active-directory-b2c/add-ropc-policy?tabs=app-reg-ga&pivots=b2c-user-flow#register-an-application
        /// </summary>
        private static void ROPC()
        {
            var clientToken = new HttpClient();
            var paramsUrl = new Dictionary<string, string>() {

                {"client_id","1674dc0d-08fc-4ce2-a4d8-1f50e57757b3"},
                {"client_secret" , "~yx8Q~pZJs8Gnh4Ncc6IK182eveSznTnp4sP2bPa" },
                {"grant_type" , "password" },
                {"username" , "usuario1" },
                {"password" , "p@$$w0rd" },
                {"scope" , "openid 1674dc0d-08fc-4ce2-a4d8-1f50e57757b3 offline_access" }
            };

            var url = "https://seedazb2c.b2clogin.com/seedazb2c.onmicrosoft.com/B2C_1_RPOC/oauth2/v2.0/token";
            var requestrequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(paramsUrl)
            };
            var res = clientToken.SendAsync(requestrequest).Result;

            var data = res.Content.ReadAsStringAsync().Result;
            var result = System.Text.Json.JsonSerializer.Deserialize<ModelBasic>(data);
        }

        private static void CCMsal()
        {
            //para gerar
            //$cert=New-SelfSignedCertificate -Subject "CN=DaemonConsoleCert" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature

            var certificateFullPath = AppDomain.CurrentDomain.BaseDirectory + "\\cc4.pfx";
            X509Certificate2 myCertificate2 = new X509Certificate2(certificateFullPath, "123456");

            var app = ConfidentialClientApplicationBuilder.Create("fef4f076-e35c-4710-962b-dc44d90d1f4e")
                .WithCertificate(myCertificate2)
                .WithAuthority(new Uri("https://login.microsoftonline.com/seedazb2c.onmicrosoft.com")) // Tenant ID
                .Build();

            app.AddInMemoryTokenCache();

            AuthenticationResult result = null;
            try
            {
                result = app.AcquireTokenForClient(new string[] { "https://seedazb2c.onmicrosoft.com/fef4f076-e35c-4710-962b-dc44d90d1f4e/.default" }).ExecuteAsync().Result;
                Console.WriteLine("Token acquired");
                Console.WriteLine("Token: " + result.AccessToken);
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {

                Console.WriteLine("Scope provided is not supported");
            }
        }

        private static void CChttpClient()
        {
            //var myCertificate = X509Certificate2.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "\\cc4.cer");
            //var clientToken = new HttpClient(new HttpClientHandler
            //{
            //    ClientCertificateOptions = ClientCertificateOption.Manual,
            //    SslProtocols = SslProtocols.Tls12,
            //    ClientCertificates = { new X509Certificate2(myCertificate) }
            //});

            var clientToken = new HttpClient();
            var paramsUrl = new Dictionary<string, string>() {

                {"client_id","fef4f076-e35c-4710-962b-dc44d90d1f4e"},
                {"client_secret" , "UVY8Q~4gZhdjWU_4tMRAJchCEXz-_vszVHcPybBk" },
                {"grant_type" , "client_credentials" },
                {"scope" , "https://seedazb2c.onmicrosoft.com/fef4f076-e35c-4710-962b-dc44d90d1f4e/.default" }
            };

            var url = "https://login.microsoftonline.com/seedazb2c.onmicrosoft.com/oauth2/v2.0/token";
            var requestrequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(paramsUrl)
            };
            var res = clientToken.SendAsync(requestrequest).Result;

            var data = res.Content.ReadAsStringAsync().Result;
            var result = System.Text.Json.JsonSerializer.Deserialize<ModelBasic>(data);

            // Chamada API com token Bearer
            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44351/api/WeatherForecast");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.access_token);
                HttpResponseMessage responseApi = httpClient.SendAsync(request).Result;
                responseApi.EnsureSuccessStatusCode();
                string responseBody = responseApi.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Response:");
                Console.WriteLine(responseBody);
            }
        }
    }
}
