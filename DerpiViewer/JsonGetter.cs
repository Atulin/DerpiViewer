using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace DerpiViewer
{
    class JsonGetter
    {
        public static string GetJson (string url)
        {
            var json = "";

            using (var webClient = new WebClient())
            {
                try
                {
                    json = webClient.DownloadString(url);
                }
                catch
                {
                    
                }
            }

            return json;
        }

    }
}
