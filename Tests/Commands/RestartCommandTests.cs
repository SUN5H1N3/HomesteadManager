using Core.Commands;
using Core.Services;
using NSubstitute;
using Xunit;

namespace Tests.Commands;

public class RestartCommandTests {
    private readonly IRestartService _restart = Substitute.For<IRestartService>();
    private readonly ILogsService _logs = Substitute.For<ILogsService>();

    private RestartCommand BuildCommand() => new(_restart, _logs);

    [Fact]
    public void Execute_WithNoArgs_PassesNullMinutes() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildCommand().Execute([]);

        _restart.Received(1).Restart(null);
    }

    [Fact]
    public void Execute_WithNumericFirstArg_PassesParsedMinutes() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildCommand().Execute(["7"]);

        _restart.Received(1).Restart(7);
    }

    [Fact]
    public void Execute_WithNonNumericFirstArg_PassesNullMinutes() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildCommand().Execute(["soon"]);

        _restart.Received(1).Restart(null);
    }

    [Fact]
    public void Execute_WhenRestarted_FollowsLogs() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildCommand().Execute([]);

        _logs.Received(1).Follow();
    }

    [Fact]
    public void Execute_WhenRestartedWithWithoutServerLogsFlag_DoesNotFollow() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Restarted);

        BuildCommand().Execute(["--without-server-logs"]);

        _logs.DidNotReceive().Follow();
    }

    [Fact]
    public void Execute_WhenRebooting_DoesNotFollowLogs() {
        _restart.Restart(Arg.Any<int?>()).Returns(RestartOutcome.Rebooting);

        BuildCommand().Execute([]);

        _logs.DidNotReceive().Follow();
    }
}
