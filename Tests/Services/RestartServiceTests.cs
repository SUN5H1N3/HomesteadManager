using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Tests.Services;

public class RestartServiceTests {
    private readonly IRconService _rcon = Substitute.For<IRconService>();
    private readonly IServerService _server = Substitute.For<IServerService>();
    private readonly IBackupService _backup = Substitute.For<IBackupService>();
    private readonly IShutdownService _shutdown = Substitute.For<IShutdownService>();
    private readonly ISleeper _sleeper = Substitute.For<ISleeper>();
    private readonly ILogger<RestartService> _logger = NullLogger<RestartService>.Instance;

    private RestartService BuildService(bool rebootAfterRestart = false, string defaultWarningMinutes = "10") {
        var config = Substitute.For<IConfiguration>();
        config["Restart:WarningMinutes"].Returns(defaultWarningMinutes);
        config["Restart:RebootAfterRestart"].Returns(rebootAfterRestart.ToString().ToLowerInvariant());
        return new RestartService(_rcon, _server, _backup, config, _logger, _shutdown, _sleeper);
    }

    [Fact]
    public void Restart_WithZeroWarning_SkipsWarningLoop() {
        var service = BuildService();

        service.Restart(0);

        _sleeper.DidNotReceiveWithAnyArgs().Sleep(TimeSpan.Zero);
        _rcon.Received(1).Say("Restarting now!");
    }

    [Fact]
    public void Restart_CallsStopBackupStartInOrder() {
        var service = BuildService();

        service.Restart(0);

        Received.InOrder(() => {
            _server.Stop();
            _backup.CreateBackup();
            _backup.CleanupBackups();
            _server.Start();
        });
    }

    [Fact]
    public void Restart_WhenBackupThrows_ContinuesAndStartsServer() {
        _backup.When(b => b.CreateBackup()).Do(_ => throw new IOException("disk full"));
        var service = BuildService();

        var outcome = service.Restart(0);

        _server.Received(1).Start();
        Assert.Equal(RestartOutcome.Restarted, outcome);
    }

    [Fact]
    public void Restart_WhenRebootAfterRestart_RebootsAndDoesNotStartServer() {
        var service = BuildService(rebootAfterRestart: true);

        var outcome = service.Restart(0);

        _shutdown.Received(1).Reboot();
        _server.DidNotReceive().Start();
        Assert.Equal(RestartOutcome.Rebooting, outcome);
    }

    [Fact]
    public void Restart_WhenNoReboot_ReturnsRestarted() {
        var service = BuildService(rebootAfterRestart: false);

        var outcome = service.Restart(0);

        _shutdown.DidNotReceive().Reboot();
        _server.Received(1).Start();
        Assert.Equal(RestartOutcome.Restarted, outcome);
    }

    [Fact]
    public void Restart_WhenWarningMinutesNull_UsesConfigDefault() {
        var service = BuildService(defaultWarningMinutes: "0");

        service.Restart(null);

        _sleeper.DidNotReceiveWithAnyArgs().Sleep(default);
    }

    [Fact]
    public void Restart_WithFiveMinuteWarning_AnnouncesAtMinuteMarks() {
        var service = BuildService();

        service.Restart(5);
        
        _rcon.Received(1).Say("Server will restart in 5 minutes!");
        _rcon.Received(1).Say("Server will restart in 4 minutes!");
        _rcon.Received(1).Say("Server will restart in 1 minute!");
        _rcon.Received(1).Say("Server will restart in 50 seconds!");
        _rcon.Received(1).Say("Server will restart in 10 seconds!");
        _rcon.Received(1).Say("Restarting now!");
    }

    [Fact]
    public void Restart_WithTwelveMinuteWarning_UsesTenMinuteTierFirst() {
        var service = BuildService();

        service.Restart(12);

        _rcon.Received(1).Say("Server will restart in 12 minutes!");
        _rcon.Received(1).Say("Server will restart in 2 minutes!");
        _sleeper.Received().Sleep(TimeSpan.FromMinutes(10));
    }
}
