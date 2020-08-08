using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileHistoryFixer
{
    public class DirectoryModel
    {
        public DirectoryModel()
        {
            Files = new List<FileModel>();
            Directories = new List<DirectoryModel>();
        }

        public string Path { get; set; }

        public List<FileModel> Files { get; set; }

        public List<DirectoryModel> Directories { get; set; }
    }

    public class FileModel
    {
        public FileModel()
        {
            Versions = new List<VersionModel>();
        }

        public string OriginalName { get; set; }

        public string Extension { get; set; }

        public List<VersionModel> Versions { get; set; }
    }

    public class VersionModel
    {
        public VersionModel(string text)
        {
            Text = text;

            DateTime = new DateTime(
                            Convert.ToInt32(text.Substring(1, 4)),
                            Convert.ToInt32(text.Substring(6, 2)),
                            Convert.ToInt32(text.Substring(9, 2)),
                            Convert.ToInt32(text.Substring(12, 2)),
                            Convert.ToInt32(text.Substring(15, 2)),
                            Convert.ToInt32(text.Substring(18, 2))
                        );
        }

        public DateTime DateTime { get; private set; }
        public string Text { get; private set; }
    }
}
