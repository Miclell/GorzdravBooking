using Core.Events.Common;
using Infrastructure.Services;
using StatefulMenu.Core.Models;

namespace CLI.Helpers;

public static class HeaderFactorySetup
{
    private static HeaderStateService? _stateService;
    private static (string Active, string Status, string NextTime, string LastResults) _cachedState;

    public static void Initialize(IEventBus eventBus)
    {
        _stateService = new HeaderStateService();

        eventBus.SubscribeMultiple(_stateService);

        _stateService.OnStateChanged += () =>
        {
            _cachedState = (
                Active: _stateService.ActiveRequests,
                _stateService.Status,
                NextTime: _stateService.NextRequestTime,
                LastResults: _stateService.LastRequests
            );
        };

        _cachedState = (
            _stateService.ActiveRequests,
            _stateService.Status,
            _stateService.NextRequestTime,
            _stateService.LastRequests
        );
    }

    public static MenuHeaderOptions SetupHeader()
    {
        if (_stateService == null)
            throw new InvalidOperationException("HeaderFactory not initialized");

        return new MenuHeaderOptions
        {
            Separator = " | ",
            Segments = new List<Func<string>>
            {
                () => _cachedState.Active,
                () => _cachedState.Status,
                () => _cachedState.NextTime,
                () => _cachedState.LastResults
            }
        };
    }
}