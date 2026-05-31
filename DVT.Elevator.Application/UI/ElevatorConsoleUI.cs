using DVT.Elevator.Application.Helpers;
using DVT.Elevator.Application.Models;
using DVT.Elevator.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Collections.Concurrent;

namespace DVT.Elevator.Application.UI;

public class ElevatorConsoleUI : BackgroundService
{
    private readonly IElevatorApiClient _apiClient;
    private readonly ElevatorSignalRService _signalR;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ElevatorConsoleUI> _logger;
    private int _selectedBuildingId = 1;
    private string _selectedBuildingName = "Loading...";
    private bool _isRunning = true;

    // Live cache updated by SignalR push events
    private readonly ConcurrentDictionary<int, ElevatorStatusModel> _liveStatuses = new();
    private string _signalRStatus = "Connecting...";

    public ElevatorConsoleUI(
        IElevatorApiClient apiClient,
        ElevatorSignalRService signalR,
        IConfiguration configuration,
        ILogger<ElevatorConsoleUI> logger)
    {
        _apiClient = apiClient;
        _signalR = signalR;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await WaitForApiAsync(stoppingToken);
            ShowWelcomeScreen();
            await LoadDefaultBuildingAsync();

            // Start SignalR connection
            await _signalR.StartAsync(stoppingToken);
            await _signalR.SubscribeToBuildingAsync(_selectedBuildingId);

            // Wire up SignalR push events to update the live cache
            _signalR.OnElevatorStatusChanged += status =>
            {
                _liveStatuses[status.Id] = status;
            };

            _signalR.OnElevatorMoved += moved =>
            {
                if (_liveStatuses.TryGetValue(moved.ElevatorId, out var existing))
                {
                    existing.CurrentFloor = moved.CurrentFloor;
                    existing.Direction = moved.Direction;
                }
            };

            _signalR.OnConnectionStateChanged += state =>
            {
                _signalRStatus = state;
            };

            _signalR.OnCapacityWarning += message =>
            {
                _logger.LogWarning("Capacity warning: {Message}", message);
            };

            while (!stoppingToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    await ShowMainMenuAsync(stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"\n[red]Unexpected error: {Markup.Escape(ex.Message)}[/]");
                    await Task.Delay(2000, stoppingToken);
                }
            }

            AnsiConsole.MarkupLine("\n[yellow]Goodbye![/]");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in console UI");
        }
    }

    // ─── Startup ──────────────────────────────────────────────────────────────

    private async Task WaitForApiAsync(CancellationToken cancellationToken)
    {
        var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7083";

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[dim]Connecting to API at {apiUrl}...[/]");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Waiting for API to be ready...", async ctx =>
            {
                var attempts = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (await _apiClient.IsApiHealthyAsync())
                    {
                        ctx.Status("[green]API is ready![/]");
                        await Task.Delay(500, cancellationToken);
                        return;
                    }
                    attempts++;
                    ctx.Status($"[yellow]Waiting for API... (attempt {attempts})[/]");
                    await Task.Delay(2000, cancellationToken);
                }
            });
    }

    private void ShowWelcomeScreen()
    {
        AnsiConsole.Clear();

        var rule = new Rule("[bold blue]DVT Elevator Challenge[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var panel = new Panel(
            Align.Center(
                new Markup(
                    "[bold yellow]Real-Time Elevator Simulation System[/]\n" +
                    "[dim]Interactive Console Frontend[/]\n\n" +
                    "[cyan]Features:[/]\n" +
                    "  [green]►[/] Real-time elevator status monitoring\n" +
                    "  [green]►[/] Interactive elevator requests\n" +
                    "  [green]►[/] Multi-building support\n" +
                    "  [green]►[/] Maintenance mode control\n" +
                    "  [green]►[/] Live request tracking"
                )
            ))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        System.Console.ReadKey(true);
    }

    private async Task LoadDefaultBuildingAsync()
    {
        var buildings = await _apiClient.GetBuildingsAsync();
        if (buildings.Any())
        {
            _selectedBuildingId = buildings.First().Id;
            _selectedBuildingName = buildings.First().Name;
        }
    }

    // ─── Main Menu ────────────────────────────────────────────────────────────

    private async Task ShowMainMenuAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();

        var header = new Rule($"[bold blue]DVT Elevator Control[/] [dim]│[/] [yellow]{_selectedBuildingName}[/]");
        header.RuleStyle("blue dim");
        AnsiConsole.Write(header);
        AnsiConsole.WriteLine();

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]What would you like to do?[/]")
                .PageSize(12)
                .HighlightStyle(Style.Parse("bold cyan"))
                .AddChoices(
                    "📊  View Real-Time Elevator Status",
                    "🛗  Request an Elevator",
                    "📋  View Active Requests",
                    "📜  View Request History",
                    "🏢  View Building Information",
                    "➕  Create a Building",
                    "🔼  Add an Elevator",
                    "🔧  Set Elevator Maintenance Mode",
                    "🔄  Change Building",
                    "❌  Exit"
                ));

        switch (choice)
        {
            case "📊  View Real-Time Elevator Status":
                await ShowRealTimeStatusAsync(cancellationToken);
                break;
            case "🛗  Request an Elevator":
                await RequestElevatorAsync(cancellationToken);
                break;
            case "📋  View Active Requests":
                await ShowActiveRequestsAsync(cancellationToken);
                break;
            case "📜  View Request History":
                await ShowRequestHistoryAsync(cancellationToken);
                break;
            case "🏢  View Building Information":
                await ShowBuildingInfoAsync(cancellationToken);
                break;
            case "➕  Create a Building":
                await CreateBuildingAsync(cancellationToken);
                break;
            case "🔼  Add an Elevator":
                await CreateElevatorAsync(cancellationToken);
                break;
            case "🔧  Set Elevator Maintenance Mode":
                await SetMaintenanceModeAsync(cancellationToken);
                break;
            case "🔄  Change Building":
                await SelectBuildingAsync(cancellationToken);
                break;
            case "❌  Exit":
                _isRunning = false;
                break;
        }
    }

    // ─── Real-Time Status ─────────────────────────────────────────────────────

    private async Task ShowRealTimeStatusAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold blue]📊 Real-Time Elevator Status[/]");

        var signalRActive = _signalR.IsConnected;
        var sourceLabel = signalRActive ? "[green]● SignalR (live push)[/]" : "[yellow]● HTTP polling (SignalR unavailable)[/]";
        AnsiConsole.MarkupLine($"[dim]Data source: {sourceLabel}  │  Press [bold]ESC[/] to return[/]\n");

        // Seed the live cache from HTTP on first load
        var initial = await _apiClient.GetElevatorStatusesAsync(_selectedBuildingId);
        foreach (var s in initial)
            _liveStatuses[s.Id] = s;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var displayTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    List<ElevatorStatusModel> statuses;

                    if (_signalR.IsConnected && _liveStatuses.Any())
                    {
                        // Use SignalR live cache — no HTTP call needed
                        statuses = _liveStatuses.Values
                            .Where(s => s != null)
                            .ToList();
                    }
                    else
                    {
                        // Fallback to HTTP polling when SignalR is unavailable
                        statuses = await _apiClient.GetElevatorStatusesAsync(_selectedBuildingId);
                        foreach (var s in statuses)
                            _liveStatuses[s.Id] = s;
                    }

                    RenderStatusTable(statuses);
                    await Task.Delay(500, cts.Token); // Refresh display every 500ms
                }
                catch (OperationCanceledException) { break; }
                catch { await Task.Delay(1000, cts.Token); }
            }
        }, cts.Token);

        // Wait for ESC
        while (!cancellationToken.IsCancellationRequested)
        {
            if (System.Console.KeyAvailable)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    cts.Cancel();
                    break;
                }
            }
            await Task.Delay(100, cancellationToken);
        }

        try { await displayTask; } catch { }
    }

    private void RenderStatusTable(List<ElevatorStatusModel> statuses)
    {
        // Move cursor to top to redraw in place
        System.Console.SetCursorPosition(0, 3);

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderStyle(Style.Parse("blue dim"));
        table.AddColumn(new TableColumn("[bold]Elevator[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Floor[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Target[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Direction[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Status[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Passengers[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Capacity[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Available[/]").Centered());

        if (!statuses.Any())
        {
            table.AddRow("[dim]No elevators registered for this building[/]", "", "", "", "", "", "", "");
        }
        else
        {
            foreach (var s in statuses.OrderBy(x => x.Name))
            {
                var dirIcon = s.Direction switch
                {
                    "Up" => "[green]▲ Up[/]",
                    "Down" => "[yellow]▼ Down[/]",
                    _ => "[grey]● Idle[/]"
                };

                var statusColor = s.Status switch
                {
                    "Moving" => "blue",
                    "Loading" => "yellow",
                    "Overloaded" => "red",
                    "Maintenance" => "orange1",
                    _ => "grey"
                };

                var capacityPct = s.MaxCapacity > 0 ? (double)s.PassengerCount / s.MaxCapacity : 0;
                var capacityColor = capacityPct >= 1.0 ? "red" : capacityPct >= 0.8 ? "yellow" : "green";
                var capacityBar = BuildCapacityBar(s.PassengerCount, s.MaxCapacity);

                table.AddRow(
                    $"[bold]{Markup.Escape(s.Name)}[/]",
                    $"[cyan bold]{s.CurrentFloor}[/]",
                    s.TargetFloor != s.CurrentFloor ? $"[dim]{s.TargetFloor}[/]" : "[dim]-[/]",
                    dirIcon,
                    $"[{statusColor}]{Markup.Escape(s.Status)}[/]",
                    $"[{capacityColor}]{s.PassengerCount}[/]",
                    $"[dim]{s.MaxCapacity}[/]  {capacityBar}",
                    s.IsAvailable ? "[green]✓[/]" : "[red]✗[/]"
                );
            }
        }

        AnsiConsole.Write(table);
        var signalRIndicator = _signalR.IsConnected ? "[green]● Live[/]" : "[yellow]● Polling[/]";
        AnsiConsole.MarkupLine($"[dim]Updated: {TimeZoneHelper.FormatTime(DateTime.UtcNow)} SAST  │  {signalRIndicator}  │  Building: {Markup.Escape(_selectedBuildingName)}[/]");
    }

    private static string BuildCapacityBar(int current, int max)
    {
        if (max == 0) return "";
        var filled = (int)Math.Round((double)current / max * 5);
        var empty = 5 - filled;
        var color = (double)current / max >= 1.0 ? "red" : (double)current / max >= 0.8 ? "yellow" : "green";
        return $"[{color}]{new string('█', filled)}[/][dim]{new string('░', empty)}[/]";
    }

    // ─── Request Elevator ─────────────────────────────────────────────────────

    private async Task RequestElevatorAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold blue]🛗 Request an Elevator[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var building = await _apiClient.GetBuildingByIdAsync(_selectedBuildingId);
        if (building == null)
        {
            AnsiConsole.MarkupLine("[red]Could not load building information.[/]");
            Pause();
            return;
        }

        var maxFloor = building.TotalFloors - 1;

        AnsiConsole.MarkupLine($"[dim]Building: [bold]{Markup.Escape(building.Name)}[/]  │  Floors: 0 – {maxFloor}[/]\n");

        int sourceFloor, destinationFloor, passengerCount;

        try
        {
            sourceFloor = AnsiConsole.Ask<int>($"[yellow]Source floor[/] [dim](0–{maxFloor}):[/]");
            destinationFloor = AnsiConsole.Ask<int>($"[yellow]Destination floor[/] [dim](0–{maxFloor}):[/]");
            passengerCount = AnsiConsole.Ask<int>("[yellow]Number of passengers[/] [dim](1–50):[/]");
        }
        catch
        {
            AnsiConsole.MarkupLine("[red]Invalid input.[/]");
            Pause();
            return;
        }

        AnsiConsole.WriteLine();

        PassengerRequestResponseModel? result = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync("Dispatching elevator...", async ctx =>
            {
                try
                {
                    result = await _apiClient.RequestElevatorAsync(new CreatePassengerRequestModel
                    {
                        SourceFloor = sourceFloor,
                        DestinationFloor = destinationFloor,
                        PassengerCount = passengerCount,
                        BuildingId = _selectedBuildingId
                    });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"\n[red]✗ {Markup.Escape(ex.Message)}[/]");
                }
            });

        if (result != null)
        {
            AnsiConsole.WriteLine();
            var panel = new Panel(
                $"[green]✓ {Markup.Escape(result.Message)}[/]\n\n" +
                $"[bold]Request ID:[/]       [cyan]{result.RequestId}[/]\n" +
                $"[bold]Assigned Elevator:[/] [cyan]{(result.AssignedElevatorId.HasValue ? $"Elevator #{result.AssignedElevatorId}" : "Queued – waiting for elevator")}[/]\n" +
                $"[bold]Estimated Arrival:[/] [cyan]{result.EstimatedArrivalTime} seconds[/]\n\n" +
                $"[dim]From floor [bold]{sourceFloor}[/] → floor [bold]{destinationFloor}[/]  │  {passengerCount} passenger(s)[/]")
            {
                Header = new PanelHeader("[bold green] Elevator Dispatched [/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(panel);
        }

        Pause();
    }

    // ─── Active Requests ──────────────────────────────────────────────────────

    private async Task ShowActiveRequestsAsync(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var displayTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var requests = await _apiClient.GetActiveRequestsAsync(_selectedBuildingId);

                    // Clear and redraw from the header down on every refresh
                    AnsiConsole.Clear();
                    var rule = new Rule("[bold blue]📋 Active Requests[/]");
                    rule.RuleStyle("blue");
                    AnsiConsole.Write(rule);
                    AnsiConsole.MarkupLine("[dim]Auto-refreshes every 2 seconds. Press [bold]ESC[/] to return to menu.[/]\n");

                    if (!requests.Any())
                    {
                        AnsiConsole.MarkupLine("[yellow]No active requests at this time.[/]");
                        AnsiConsole.MarkupLine($"\n[dim]Updated: {TimeZoneHelper.FormatTime(DateTime.UtcNow)} SAST[/]");
                    }
                    else
                    {
                        var table = new Table();
                        table.Border(TableBorder.Rounded);
                        table.BorderStyle(Style.Parse("blue dim"));
                        table.AddColumn(new TableColumn("[bold]ID[/]").Centered());
                        table.AddColumn(new TableColumn("[bold]From[/]").Centered());
                        table.AddColumn(new TableColumn("[bold]To[/]").Centered());
                        table.AddColumn(new TableColumn("[bold]Direction[/]").Centered());
                        table.AddColumn(new TableColumn("[bold]Passengers[/]").Centered());
                        table.AddColumn(new TableColumn("[bold]Status[/]").Centered());
                        table.AddColumn(new TableColumn("[bold]Elevator[/]").Centered());
                        table.AddColumn(new TableColumn("[bold]Requested[/]").Centered());

                        foreach (var r in requests.OrderBy(x => x.RequestTime))
                        {
                            var statusColor = r.Status switch
                            {
                                "Pending" => "yellow",
                                "Assigned" => "blue",
                                "InProgress" => "cyan",
                                _ => "grey"
                            };

                            var dirIcon = r.Direction switch
                            {
                                "Up" => "[green]▲ Up[/]",
                                "Down" => "[yellow]▼ Down[/]",
                                _ => "[grey]─[/]"
                            };

                            table.AddRow(
                                $"[dim]{r.Id}[/]",
                                $"[cyan]{r.SourceFloor}[/]",
                                $"[cyan]{r.DestinationFloor}[/]",
                                dirIcon,
                                $"{r.PassengerCount}",
                                $"[{statusColor}]{Markup.Escape(r.Status)}[/]",
                                r.AssignedElevatorId.HasValue ? $"[green]#{r.AssignedElevatorId}[/]" : "[dim]Waiting...[/]",
                                $"[dim]{TimeZoneHelper.FormatTime(r.RequestTime)} SAST[/]"
                            );
                        }

                        AnsiConsole.Write(table);
                        AnsiConsole.MarkupLine($"[dim]Total active: [bold]{requests.Count}[/]  │  Updated: {TimeZoneHelper.FormatTime(DateTime.UtcNow)} SAST[/]");
                    }

                    await Task.Delay(2000, cts.Token);
                }
                catch (OperationCanceledException) { break; }
                catch { await Task.Delay(2000, cts.Token); }
            }
        }, cts.Token);

        // Wait for ESC
        while (!cancellationToken.IsCancellationRequested)
        {
            if (System.Console.KeyAvailable)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    cts.Cancel();
                    break;
                }
            }
            await Task.Delay(100, cancellationToken);
        }

        try { await displayTask; } catch { }
    }

    // ─── Request History ──────────────────────────────────────────────────────

    private async Task ShowRequestHistoryAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold blue]📜 Request History[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        List<PassengerRequestModel> history = new();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading history...", async ctx =>
            {
                history = await _apiClient.GetRequestHistoryAsync(_selectedBuildingId);
            });

        if (!history.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No request history found.[/]");
        }
        else
        {
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.BorderStyle(Style.Parse("blue dim"));
            table.AddColumn(new TableColumn("[bold]ID[/]").Centered());
            table.AddColumn(new TableColumn("[bold]From[/]").Centered());
            table.AddColumn(new TableColumn("[bold]To[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Passengers[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Status[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Elevator[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Time[/]").Centered());

            foreach (var r in history.OrderByDescending(x => x.RequestTime).Take(20))
            {
                var statusColor = r.Status switch
                {
                    "Completed" => "green",
                    "Cancelled" => "red",
                    "Pending" => "yellow",
                    "InProgress" => "cyan",
                    _ => "grey"
                };

                table.AddRow(
                    $"[dim]{r.Id}[/]",
                    $"[cyan]{r.SourceFloor}[/]",
                    $"[cyan]{r.DestinationFloor}[/]",
                    $"{r.PassengerCount}",
                    $"[{statusColor}]{Markup.Escape(r.Status)}[/]",
                    r.AssignedElevatorId.HasValue ? $"[dim]#{r.AssignedElevatorId}[/]" : "[dim]-[/]",
                    $"[dim]{TimeZoneHelper.FormatTime(r.RequestTime)} SAST[/]"
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[dim]Showing last 20 of [bold]{history.Count}[/] records[/]");
        }

        Pause();
    }

    // ─── Building Info ────────────────────────────────────────────────────────

    private async Task ShowBuildingInfoAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold blue]🏢 Building Information[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        BuildingModel? building = null;
        List<ElevatorModel> elevators = new();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading...", async ctx =>
            {
                building = await _apiClient.GetBuildingByIdAsync(_selectedBuildingId);
                elevators = await _apiClient.GetElevatorsByBuildingAsync(_selectedBuildingId);
            });

        if (building == null)
        {
            AnsiConsole.MarkupLine("[red]Building not found.[/]");
            Pause();
            return;
        }

        var infoPanel = new Panel(
            $"[bold]Name:[/]         [cyan]{Markup.Escape(building.Name)}[/]\n" +
            $"[bold]Total Floors:[/] [cyan]{building.TotalFloors}[/]\n" +
            $"[bold]Elevators:[/]    [cyan]{elevators.Count}[/]\n" +
            $"[bold]Building ID:[/]  [dim]{building.Id}[/]")
        {
            Header = new PanelHeader($"[bold yellow] {Markup.Escape(building.Name)} [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("yellow"),
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(infoPanel);
        AnsiConsole.WriteLine();

        if (elevators.Any())
        {
            AnsiConsole.MarkupLine("[bold]Elevators:[/]");
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.BorderStyle(Style.Parse("blue dim"));
            table.AddColumn("[bold]Name[/]");
            table.AddColumn("[bold]Type[/]");
            table.AddColumn(new TableColumn("[bold]Capacity[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Speed[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Status[/]").Centered());

            foreach (var e in elevators.OrderBy(x => x.Name))
            {
                var statusColor = e.Status switch
                {
                    "Moving" => "blue",
                    "Maintenance" => "orange1",
                    "Overloaded" => "red",
                    _ => "green"
                };

                table.AddRow(
                    $"[bold]{Markup.Escape(e.Name)}[/]",
                    $"[dim]{Markup.Escape(e.ElevatorTypeName ?? "Standard")}[/]",
                    $"{e.MaxCapacity}",
                    $"{e.Speed} fl/min",
                    $"[{statusColor}]{Markup.Escape(e.Status)}[/]"
                );
            }

            AnsiConsole.Write(table);
        }

        Pause();
    }

    // ─── Maintenance Mode ─────────────────────────────────────────────────────

    private async Task SetMaintenanceModeAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold blue]🔧 Elevator Maintenance Mode[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        List<ElevatorModel> elevators = new();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading elevators...", async ctx =>
            {
                elevators = await _apiClient.GetElevatorsByBuildingAsync(_selectedBuildingId);
            });

        if (!elevators.Any())
        {
            AnsiConsole.MarkupLine("[red]No elevators found.[/]");
            Pause();
            return;
        }

        var choices = elevators
            .Select(e => $"{e.Name}  [dim](ID:{e.Id} │ {e.Status})[/]")
            .ToList();
        choices.Add("[dim]← Cancel[/]");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select elevator:[/]")
                .HighlightStyle(Style.Parse("bold cyan"))
                .AddChoices(choices));

        if (choice.Contains("Cancel")) return;

        var selectedElevator = elevators[choices.IndexOf(choice)];
        var currentlyInMaintenance = selectedElevator.Status == "Maintenance";

        AnsiConsole.MarkupLine($"\n[bold]Elevator:[/] [cyan]{Markup.Escape(selectedElevator.Name)}[/]");
        AnsiConsole.MarkupLine($"[bold]Current Status:[/] [yellow]{Markup.Escape(selectedElevator.Status)}[/]\n");

        var enable = AnsiConsole.Confirm(
            currentlyInMaintenance
                ? "[yellow]This elevator is in maintenance. Remove from maintenance?[/]"
                : "[yellow]Put this elevator into maintenance mode?[/]",
            !currentlyInMaintenance);

        bool success = false;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Updating...", async ctx =>
            {
                success = await _apiClient.SetMaintenanceModeAsync(selectedElevator.Id, enable);
            });

        AnsiConsole.WriteLine();
        if (success)
            AnsiConsole.MarkupLine($"[green]✓ {Markup.Escape(selectedElevator.Name)} maintenance mode {(enable ? "enabled" : "disabled")} successfully.[/]");
        else
            AnsiConsole.MarkupLine("[red]✗ Failed to update maintenance mode. Check API connection.[/]");

        Pause();
    }

    // ─── Change Building ──────────────────────────────────────────────────────

    private async Task SelectBuildingAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold blue]🔄 Select Building[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        List<BuildingModel> buildings = new();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading buildings...", async ctx =>
            {
                buildings = await _apiClient.GetBuildingsAsync();
            });

        if (!buildings.Any())
        {
            AnsiConsole.MarkupLine("[red]No buildings found.[/]");
            Pause();
            return;
        }

        var choices = buildings
            .Select(b => $"{b.Name}  [dim](ID:{b.Id} │ {b.TotalFloors} floors)[/]")
            .ToList();

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select building:[/]")
                .HighlightStyle(Style.Parse("bold cyan"))
                .AddChoices(choices));

        var selected = buildings[choices.IndexOf(choice)];

        // Unsubscribe from old building, subscribe to new one
        await _signalR.UnsubscribeFromBuildingAsync(_selectedBuildingId);
        _liveStatuses.Clear();

        _selectedBuildingId = selected.Id;
        _selectedBuildingName = selected.Name;

        await _signalR.SubscribeToBuildingAsync(_selectedBuildingId);

        AnsiConsole.MarkupLine($"\n[green]✓ Switched to [bold]{Markup.Escape(selected.Name)}[/][/]");
        await Task.Delay(1000, cancellationToken);
    }

    // ─── Create Building ──────────────────────────────────────────────────────

    private async Task CreateBuildingAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold blue]➕ Create a Building[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        string name;
        int totalFloors;

        try
        {
            name = AnsiConsole.Ask<string>("[yellow]Building name:[/]");
            totalFloors = AnsiConsole.Ask<int>("[yellow]Total floors[/] [dim](1–200):[/]");
        }
        catch
        {
            AnsiConsole.MarkupLine("[red]Invalid input.[/]");
            Pause();
            return;
        }

        BuildingModel? result = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync("Creating building...", async ctx =>
            {
                try
                {
                    result = await _apiClient.CreateBuildingAsync(new CreateBuildingModel
                    {
                        Name = name,
                        TotalFloors = totalFloors
                    });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"\n[red]✗ {Markup.Escape(ex.Message)}[/]");
                }
            });

        if (result != null)
        {
            AnsiConsole.WriteLine();
            var panel = new Panel(
                $"[green]✓ Building created successfully![/]\n\n" +
                $"[bold]Name:[/]         [cyan]{Markup.Escape(result.Name)}[/]\n" +
                $"[bold]Total Floors:[/] [cyan]{result.TotalFloors}[/]\n" +
                $"[bold]Building ID:[/]  [cyan]{result.Id}[/]")
            {
                Header = new PanelHeader("[bold green] Building Created [/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(panel);

            // Switch to the new building
            if (AnsiConsole.Confirm("\n[yellow]Switch to this building?[/]"))
            {
                await _signalR.UnsubscribeFromBuildingAsync(_selectedBuildingId);
                _liveStatuses.Clear();

                _selectedBuildingId = result.Id;
                _selectedBuildingName = result.Name;

                await _signalR.SubscribeToBuildingAsync(result.Id);
                AnsiConsole.MarkupLine($"[green]✓ Switched to {Markup.Escape(result.Name)}[/]");
            }
        }

        Pause();
    }

    // ─── Create Elevator ──────────────────────────────────────────────────────

    private async Task CreateElevatorAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold blue]🔼 Add an Elevator[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[dim]Adding elevator to: [bold]{Markup.Escape(_selectedBuildingName)}[/][/]\n");

        // Load elevator types
        List<ElevatorTypeModel> elevatorTypes = new();
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading elevator types...", async ctx =>
            {
                elevatorTypes = await _apiClient.GetElevatorTypesAsync();
            });

        if (!elevatorTypes.Any())
        {
            // Fallback to known types if endpoint not available
            elevatorTypes = new List<ElevatorTypeModel>
            {
                new() { Id = 1, Name = "Passenger" },
                new() { Id = 2, Name = "Freight" },
                new() { Id = 3, Name = "Glass" },
                new() { Id = 4, Name = "High-Speed" }
            };
        }

        string elevatorName;
        int maxCapacity, speed, initialFloor;

        try
        {
            elevatorName = AnsiConsole.Ask<string>("[yellow]Elevator name:[/]");
            initialFloor = AnsiConsole.Ask<int>("[yellow]Initial floor[/] [dim](default 0):[/]", 0);
            maxCapacity = AnsiConsole.Ask<int>("[yellow]Max capacity[/] [dim](1–100):[/]", 10);
            speed = AnsiConsole.Ask<int>("[yellow]Speed[/] [dim](floors per minute, 1–300):[/]", 60);
        }
        catch
        {
            AnsiConsole.MarkupLine("[red]Invalid input.[/]");
            Pause();
            return;
        }

        // Select elevator type
        var typeChoices = elevatorTypes.Select(t => $"{t.Name} (ID:{t.Id})").ToList();
        var typeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select elevator type:[/]")
                .HighlightStyle(Style.Parse("bold cyan"))
                .AddChoices(typeChoices));

        var selectedType = elevatorTypes[typeChoices.IndexOf(typeChoice)];

        ElevatorModel? result = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync("Adding elevator...", async ctx =>
            {
                try
                {
                    result = await _apiClient.CreateElevatorAsync(new CreateElevatorModel
                    {
                        Name = elevatorName,
                        InitialFloor = initialFloor,
                        MaxCapacity = maxCapacity,
                        Speed = speed,
                        ElevatorTypeId = selectedType.Id,
                        BuildingId = _selectedBuildingId
                    });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"\n[red]✗ {Markup.Escape(ex.Message)}[/]");
                }
            });

        if (result != null)
        {
            AnsiConsole.WriteLine();
            var panel = new Panel(
                $"[green]✓ Elevator added successfully![/]\n\n" +
                $"[bold]Name:[/]         [cyan]{Markup.Escape(result.Name)}[/]\n" +
                $"[bold]Type:[/]         [cyan]{Markup.Escape(result.ElevatorTypeName ?? selectedType.Name)}[/]\n" +
                $"[bold]Max Capacity:[/] [cyan]{result.MaxCapacity} passengers[/]\n" +
                $"[bold]Speed:[/]        [cyan]{result.Speed} floors/min[/]\n" +
                $"[bold]Building:[/]     [cyan]{Markup.Escape(_selectedBuildingName)}[/]\n" +
                $"[bold]Elevator ID:[/]  [cyan]{result.Id}[/]")
            {
                Header = new PanelHeader("[bold green] Elevator Added [/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(panel);
        }

        Pause();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to return to menu...[/]");
        System.Console.ReadKey(true);
    }
}
