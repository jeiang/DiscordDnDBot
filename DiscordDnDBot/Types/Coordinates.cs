using System.Net;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace DiscordDnDBot.Types
{
    internal class Coordinates
    {
        public static Coordinates NaN
        {
            get
            {
                return new Coordinates()
                {
                    Latitude = double.NaN,
                    Longitude = double.NaN,
                };
            }
        }

        public double Longitude { get; private set; }
        public double Latitude { get; private set; }

        public static async Task<Coordinates> GetCoordinatesAsync(string location)
        {
            location = string.Join("+", location.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            string query = $"https://nominatim.openstreetmap.org/search.php?q={location}&format=jsonv2";

            using HttpClientHandler handler = new()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 OPR/82.0.4227.50");

            var request = new HttpRequestMessage(HttpMethod.Get, query);
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
            {
                return NaN;
            }

            string data = await response.Content.ReadAsStringAsync();
            dynamic[] output = JsonConvert.DeserializeObject<dynamic[]>(data) ?? Array.Empty<dynamic>();
            if (output == null || output.Length == 0)
            {
                return NaN;
            }
            
            try
            {
                return new Coordinates()
                {
                    Latitude = output[0].lat,
                    Longitude = output[0].lon,
                };
            }
            catch
            {
                return NaN;
            }
        }
    }
}
