using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerpiViewer
{
    public class FileDisplay
    {
        string Filename { get; set; }
        string Source { get; set; }
        string Author { get; set; }

        public FileDisplay (string name, string source, string author)
        {
            this.Filename = name;
            this.Source = source;
            this.Author = author;
        }
    }
}
