namespace Selector
{
    public class DatabaseOptions
    {
        public const string Key = "Database";

        public bool Enabled { get; set; } = false;
        public string ConnectionString { get; set; }
        public bool Migrate { get; set; } = false;
    }
}
