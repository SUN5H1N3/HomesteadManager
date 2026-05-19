using Core.Cli;
using Core.Commands;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;
using Xunit;

namespace Tests.Commands;

public class RestartCommandTests {
    private readonly IRestartService _restart = Substitute.For<IRestartService>();
    private readonly ILogsService _logs = Substitute.For<ILogsService>();

    private CommandAppTester BuildApp() {
        var services = new ServiceCollection()
            .AddSingleton(_restart)
            .AddSingleton(_logs);

        var app = new CommandAppTester(new TypeRegistrar(services));
        app.SetDefaultCommand<RestartCommand>();
        return app;
    }

    [Fact]
    public void Execute_WithNoArgs_PassesNullMinutes() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildApp().Run();

        _restart.Received(1).Restart(null);
    }

    [Fact]
    public void Execute_WithNumericFirstArg_PassesParsedMinutes() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildApp().Run("7");

        _restart.Received(1).Restart(7);
    }

    [Fact]
    public void Execute_WhenRestarted_FollowsLogs() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildApp().Run();

        _logs.Received(1).Follow();
    }

    [Fact]
    public void Execute_WhenRestartedWithWithoutServerLogsFlag_DoesNotFollow() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildApp().Run("--without-server-logs");

        _logs.DidNotReceive().Follow();
    }

    [Fact]
    public void Execute_WhenRebooting_DoesNotFollowLogs() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Rebooting);

        BuildApp().Run();

        _logs.DidNotReceive().Follow();
    }
}
