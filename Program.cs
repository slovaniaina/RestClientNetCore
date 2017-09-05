using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestClientNetCore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Security.Cryptography;

namespace RestClientNetCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Get repo from API client!");

            //var repositories = ProcessRepositories().Result;
            //if (repositories != null)
            //{
            //    foreach (var repo in repositories)
            //    {
            //        Console.WriteLine(repo.Name);
            //        Console.WriteLine(repo.Description);
            //        Console.WriteLine(repo.GitHubHomeUrl);
            //        Console.WriteLine(repo.Homepage);
            //        Console.WriteLine(repo.Watchers);
            //        Console.WriteLine("Last push ");
            //        Console.WriteLine(repo.LastPush);
            //    }
            //}

            var test = PostValues();
            Console.WriteLine("Response "+test.Result.Success+ " Urn "+test.Result.Urn);

            Console.ReadLine();
        }

        private static async Task<List<Repository>> ProcessRepositories()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var streamTask = client.GetStreamAsync("https://api.github.com/orgs/dotnet/repos");

            var serializer = new DataContractJsonSerializer(typeof(List<Repository>));

            var repositories = serializer.ReadObject(await streamTask) as List<Repository>;
            return repositories;
        }

        private static async Task<GrsJsonResponse> PostValues()
        {
            var pairs = new Dictionary<string, string> {{"grs_trait[3328]", "36197"}};
            var formContent =
                new FormUrlEncodedContent(pairs);
            const string uri = "http://stage.grs-pg.com/rest/app/548/consumer/132111/context/update/3163/flex/162";

            #region header
            formContent.Headers.Remove("Content-Type");
            formContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            #endregion
            var req = new HttpRequestMessage(HttpMethod.Put, uri);

            #region authentication
            var authInfo = string.Format("{0}:{1}", "vortex_gf_de_de", "Vortex2016");
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers.Add("Authorization", "Basic " + authInfo);
            req.Headers.Remove("Expect");
            req.Headers.Add("Expect", "");
            req.Content = formContent;
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            #endregion

            var client = new HttpClient();
            var response = await client.SendAsync(req);

            response.EnsureSuccessStatusCode();

            var test = await response.Content.ReadAsStringAsync();

            var grsResponse = JsonConvert.DeserializeObject<GrsJsonResponse>(test);

            return grsResponse;
        }

        //Encrypting a string
        public static string StringEncrypt(string inText, string key)
        {
            var bytesBuff = Encoding.Unicode.GetBytes(inText);
            using (var aes = Aes.Create())
            {
                var crypto = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = crypto.GetBytes(32);
                aes.IV = crypto.GetBytes(16);
                using (var mStream = new MemoryStream())
                {
                    using (var cStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(bytesBuff, 0, bytesBuff.Length);
                        cStream.Close();
                    }
                    inText = Convert.ToBase64String(mStream.ToArray());
                }
            }
            return inText;
        }

        //Decrypting a string
        public static string StringDecrypt(string cryptTxt, string key)
        {
            cryptTxt = cryptTxt.Replace(" ", "+");
            var bytesBuff = Convert.FromBase64String(cryptTxt);
            using (var aes = Aes.Create())
            {
                var crypto = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = crypto.GetBytes(32);
                aes.IV = crypto.GetBytes(16);
                using (var mStream = new MemoryStream())
                {
                    using (var cStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(bytesBuff, 0, bytesBuff.Length);
                        cStream.Close();
                    }
                    cryptTxt = Encoding.Unicode.GetString(mStream.ToArray());
                }
            }
            return cryptTxt;
        }

        //public System.Drawing.Image Base64ToImage(string base64String)
        //{
        //    var image = System.IO.File.OpenRead("C:\\test\random_image.jpeg");
        //    return File(image, "image/jpeg");
        //}


    }
}
