var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MusicApi>("minimal-api");
builder.AddProject<Projects.ControllerMusicApi>("controller");

builder.Build().Run();
