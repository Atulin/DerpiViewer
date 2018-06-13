using System.IO;
using System.Linq;

namespace DerpiViewer
{
    public class FileDisplay
    {
        public string Filename { get; set; }
        public string Extension { get; set; }
        public string Source { get; set; }
        public string Author { get; set; }
        public string[] Tags { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
        public string Thumb { get; set; }
        public string File { get; set; }
        public string Id { get; set; }
        public string Link { get; set; }

        private readonly string _webmUri = "Imgs/Placeholders/webm.png";
        private readonly string _mp4Uri = "Imgs/Placeholders/mp4.png";


        public FileDisplay (string name, string extension, string source, string author, string tags, string description, int score, string thumb, string file, string id)
        {
            this.Filename = name;
            this.Extension = extension.Split('/')[1].ToUpper();
            this.Source = source;
            this.Author = author;
            this.Tags = tags.Split(',').Select(sValue => sValue.Trim()).ToArray();
            this.Description = description;
            this.Score = score;

            switch (this.Extension)
            {
                case "WEBM":
                    this.Thumb = _webmUri;
                    break;
                case "MP4":
                    this.Thumb = _mp4Uri;
                    break;
                default:
                    this.Thumb = thumb;
                    break;
            }
            
            this.File = file;
            this.Id = id;

            this.Link = "https://derpibooru.org/" + id;
        }
    }
}
