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
        private const string Path = "queries.sav";

        public string Name { get; set; }
        public string Tags { get; set; }
        public string QueryStr { get; set; }
        public int? MinScore { get; set; }
        public int? MaxScore { get; set; }
        public bool[] Ratings { get; set; }

        public Query(string name, string tags, string query, int minScore, int maxScore, bool[] ratings)
        {
            Name = name;
            Tags = tags;
            QueryStr = query;
            MinScore = minScore;
            MaxScore = maxScore;
            Ratings = ratings;
        }

        public Query()
        {
            Name = null;
            Tags = null;
            QueryStr = null;
            MinScore = null;
            MaxScore = null;
            Ratings = new[]{true, false, false};
        }


        // Save current query to json
        public void SaveQuery()
        {
            if (!File.Exists(Path))
                File.WriteAllText(Path, "");
            
            var jsonData = File.ReadAllText(Path);

            // De-serialize to object or create new list
            var data = JsonConvert.DeserializeObject<List<Query>>(jsonData)
                               ?? new List<Query>();

            // Add the query to the list
            data.Insert(0, this);

            // Serialize
            string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented);

            //write string to file
            File.WriteAllText(Path, json);
        }


        // Delete current query from json
        public void DeleteQuery()
        {
            if (!File.Exists(Path))
                File.WriteAllText(Path, "");

            var jsonData = File.ReadAllText(Path);

            // De-serialize to object or create new list
            var data = JsonConvert.DeserializeObject<List<Query>>(jsonData)
                       ?? new List<Query>();

            // Remove the query from list
            data.Remove(this);

            // Serialize
            string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented);

            // Write string to file
            File.WriteAllText(Path, json);
        }


        // Get all queries from json
        public static List<Query> GetAll()
        {
            var jsonData = File.ReadAllText(Path);

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


        // Override Equals() method
        protected bool Equals(Query other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Tags, other.Tags) && string.Equals(QueryStr, other.QueryStr);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Query) obj);
        }

        // Override GetHashCode() method
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tags != null ? Tags.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (QueryStr != null ? QueryStr.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
