using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace DerpiViewer
{
    public class Query
    {
        private const string path = "queries.sav";

        public string Name { get; set; }
        public string Tags { get; set; }
        public string QueryStr { get; set; }

        public Query(string name, string tags, string query)
        {
            Name = name;
            Tags = tags;
            QueryStr = query;
        }

        public Query()
        {
            Name = null;
            Tags = null;
            QueryStr = null;
        }

        // Save current query to json
        public void Save()
        {
            if (!File.Exists(path))
                File.WriteAllText(path, "");
            
            var jsonData = File.ReadAllText(path);

            // De-serialize to object or create new list
            var data = JsonConvert.DeserializeObject<List<Query>>(jsonData)
                               ?? new List<Query>();

            // Add the query to the list
            data.Add(this);

            // Serialize
            string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented);

            //write string to file
            File.WriteAllText(path, json);
        }

        // Get all queries from json
        public static List<Query> GetAll()
        {
            var jsonData = File.ReadAllText(path);

            // De-serialize to object or create new list
            var data = JsonConvert.DeserializeObject<List<Query>>(jsonData)
                       ?? new List<Query>();

            return data;
        }

        // Override ToString() method
        public override string ToString()
        {
            return Name;
        }

    }

}
