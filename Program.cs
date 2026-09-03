namespace KayOne;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        if (Environment.GetEnvironmentVariable("KAYONE_ENGINE_TEST") == "1")
        {
            var engine = new EnterpriseEngine(new EnterpriseEngineOptions
            {
                DataFilePath = Environment.GetEnvironmentVariable("KAYONE_DATA_FILE"),
                SeedDemoData = true,
                EnforceAuthorization = true
            });
            var report = engine.RunAcceptanceChecks();
            var path = Environment.GetEnvironmentVariable("KAYONE_TEST_REPORT")
                ?? Path.Combine(Path.GetTempPath(), "kayone-engine-acceptance.json");
            File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
            Environment.ExitCode = report.Success ? 0 : 1;
            return;
        }
        ApplicationConfiguration.Initialize();
        Application.Run(new WebDashboardForm());
    }
}
