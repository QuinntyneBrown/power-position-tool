using Topshelf;

namespace PowerPositionTool
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Create a windows service using TopShelf: http://docs.topshelf-project.com/en/latest/
            HostFactory.Run(x =>
            {
                x.UseLog4Net("log4net.config");
                x.Service<Service>(s =>
                {
                    s.ConstructUsing(name => new Service());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.SetServiceName("PowerPositionTool");
                x.SetDisplayName("Power Position Tool");
                x.SetDescription("Calculates the day-ahead power position."); 
            });
        }
    }
}
