﻿namespace COG.Listener.Event.Impl;

public class PlayerPhysicsEvent : Event
{
    public PlayerPhysics PlayerPhysics { get; }

    public PlayerPhysicsEvent(PlayerPhysics playerPhysics)
    {
        PlayerPhysics = playerPhysics;
    }
}