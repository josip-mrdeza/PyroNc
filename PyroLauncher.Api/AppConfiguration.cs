namespace PyroLauncher.Api
{
    public class AppConfiguration
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullPath { get; set; }
        public string PicturePath { get; set; }

        public AppConfiguration(string name, string description, string fullPath, string picturePath)
        {
            Name = name;
            Description = description;
            FullPath = fullPath;
            PicturePath = picturePath;
        }
    }
}