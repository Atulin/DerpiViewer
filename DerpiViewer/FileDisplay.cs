using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerpiViewer
{
    public class FileDisplay
    {
        public string Filename { get; set; }
        public string Source { get; set; }
        public string Author { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
        public string Thumb { get; set; }
        public string File { get; set; }

        public FileDisplay (string name, string source, string author, string tags, string description, int score, string thumb, string file)
        {
            this.Filename = name;
            this.Source = source;
            this.Author = author;
            this.Tags = tags;
            this.Description = Description;
            this.Score = Score;
            this.Thumb = thumb;
            this.File = file;
        }
    }
}
