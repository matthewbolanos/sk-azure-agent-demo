// Copyright (c) Microsoft. All rights reserved.
namespace Events;

/// <summary>
/// Processes Events emitted by shared steps.<br/>
/// </summary>
public static class ProcessEvents
{
    public static readonly string StartProcess = nameof(StartProcess);
    public static readonly string NeedMoreData = nameof(NeedMoreData);
    public static readonly string NeedsEdit = nameof(NeedsEdit);
    public static readonly string Approved = nameof(Approved);
}