namespace Notes.Identity.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AuthDbContext authDbContext)
        {
            authDbContext.Database.EnsureCreated();
        }
    }
}
