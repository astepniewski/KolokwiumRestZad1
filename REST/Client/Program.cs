using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static string FilePath;
        private static string OutputFilePath;
        private static readonly string serverAddress = "http://localhost:2015";

        static void Main(string[] args)
        {
            Console.WriteLine("Podaj ścieżkę do pliku");
            FilePath = Console.ReadLine();

            var wc = new WebClient();
            wc.UploadFile(serverAddress + "/api/Bib2/UploadFile", "POST", FilePath);

            Console.WriteLine("Podaj hasło do zaszyfrowania:");
            var password = Console.ReadLine();

            Console.WriteLine("Podaj gdzie zapisać plik");
            OutputFilePath = Console.ReadLine();

            DownloadFileAsync(OutputFilePath + @"\\encrypted." + Path.GetExtension(FilePath), "encrypt", password).Wait();
            Console.WriteLine("Plik zaszyfrowano");

            Console.WriteLine("Podaj hasło do odszyfrowania:");
            password = Console.ReadLine();

            Console.WriteLine("Podaj gdzie zapisać plik");
            OutputFilePath = Console.ReadLine();

            try
            {
                DownloadFileAsync(OutputFilePath + @"\\decrypted." + Path.GetExtension(FilePath), "decrypt", password).Wait();
                Console.WriteLine("Plik odszyfrowano");
            }
            catch (Exception e)
            {
                Console.WriteLine("Nie można odszyfrować! Złe hasło!");
                Console.ReadKey();
                return;
            }

            var fileFormat = Path.GetExtension(FilePath);
            Console.WriteLine("Format pliku to: " + fileFormat);

            if (".bmp".Equals(fileFormat) || ".jpg".Equals(fileFormat) || ".mp3".Equals(fileFormat))
            {
                System.Diagnostics.Process.Start(FilePath);
            }

            Console.ReadKey();
        }
        static async Task DownloadFileAsync(string path, string option, string password)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(serverAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                HttpResponseMessage response = await client.GetAsync(@"/api/Bib2/GetAES?option=" + option + "&password=" + password);
                if(response.StatusCode.Equals(HttpStatusCode.BadRequest))
                {
                    throw new CryptographicException();
                }
                if (response.IsSuccessStatusCode)
                {
                    using (var fileStream = File.Create(path))
                    {
                        CopyStream(await response.Content.ReadAsStreamAsync(), fileStream);
                    }
                }
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

    }
}
