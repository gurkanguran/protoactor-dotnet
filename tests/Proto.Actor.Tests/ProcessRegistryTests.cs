﻿using System;
using System.Threading.Tasks;
using Proto.TestFixtures;
using Xunit;

namespace Proto.Tests;

public class ProcessRegistryTests
{
    [Fact]
    public async Task Given_PIDDoesNotExist_TryAddShouldAddLocalPID()
    {
        await using var system = new ActorSystem();
        var id = Guid.NewGuid().ToString();
        var p = new TestProcess(system);
        var reg = new ProcessRegistry(system);

        var (pid, ok) = reg.TryAdd(id, p);

        Assert.True(ok);
        Assert.Equal(system.Address, pid.Address);
    }

    [Fact]
    public async Task Given_PIDExists_TryAddShouldNotAddLocalPID()
    {
        await using var system = new ActorSystem();
        var id = Guid.NewGuid().ToString();
        var p = new TestProcess(system);
        var reg = new ProcessRegistry(system);
        reg.TryAdd(id, p);

        var (_, ok) = reg.TryAdd(id, p);

        Assert.False(ok);
    }

    [Fact]
    public async Task Given_PIDExists_GetShouldReturnIt()
    {
        await using var system = new ActorSystem();
        var id = Guid.NewGuid().ToString();
        var p = new TestProcess(system);
        var reg = new ProcessRegistry(system);
        reg.TryAdd(id, p);
        var (pid, _) = reg.TryAdd(id, p);

        var p2 = reg.Get(pid);

        Assert.Same(p, p2);
    }

    [Fact]
    public async Task Given_PIDWasRemoved_GetShouldReturnDeadLetterProcess()
    {
        await using var system = new ActorSystem();
        var id = Guid.NewGuid().ToString();
        var p = new TestProcess(system);
        var reg = new ProcessRegistry(system);
        var (pid, _) = reg.TryAdd(id, p);
        reg.Remove(pid);

        var p2 = reg.Get(pid);

        Assert.Same(system.DeadLetter, p2);
    }

    [Fact]
    public async Task Given_PIDExistsInHostResolver_GetShouldReturnIt()
    {
        await using var system = new ActorSystem();
        var pid = new PID();
        var p = new TestProcess(system);
        var reg = new ProcessRegistry(system);
        reg.RegisterHostResolver(x => p);

        var p2 = reg.Get(pid);

        Assert.Same(p, p2);
    }
}