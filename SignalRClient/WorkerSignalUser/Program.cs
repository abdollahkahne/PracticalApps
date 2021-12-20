using WorkerSignalUser;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>(); // In this worker we use the Hub that added and mapped to endpoint in a webapp 
    })
    .Build();

await host.RunAsync();
