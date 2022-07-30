#pragma warning disable CA1416
namespace NoP77svk.wtwd.cli.InitLockUnlock;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using global::wtwd.Utilities;
using Microsoft.Win32.TaskScheduler;
using NoP77svk.wtwd.Model;
using NoP77svk.wtwd.Utilities;

public static class InitLockUnlockProgram
{
    public static void Execute(InitLockUnlockCLI cli)
    {
        Execute(InitLockUnlockConfig.FromRawCLI(cli));
    }

    internal static void Execute(InitLockUnlockConfig cli)
    {
        InitializeEventSource();
        InitializeScheduledTasks(cli);
    }

    private static void InitializeEventSource()
    {
        EventSourceCreationData eventSource = new EventSourceCreationData(LockUnlockEventLog.SourceName, LockUnlockEventLog.LogName)
        {
            MachineName = LockUnlockEventLog.LocalMachine
        };

        if (!EventLog.SourceExists(LockUnlockEventLog.SourceName))
        {
            try
            {
                EventLog.CreateEventSource(eventSource);
                Console.WriteLine("Event log source successfully initialized");
            }
            catch (SecurityException e)
            {
                throw new SecurityException("Please, re-run again with elevated privileges", e);
            }
        }
        else
        {
            Console.WriteLine("Event log source already initialized");
        }
    }

    private static void InitializeScheduledTasks(InitLockUnlockConfig cli)
    {
        TaskFolder wtwdFolder = TaskService.Instance.RootFolder.CreateFolderIfNotExists("NoP77svk").CreateFolderIfNotExists("WTWD");
        Console.WriteLine("Scheduler folder /NoP77svk/WTWD created");

        WindowsUser user = WindowsUser.Current();

        (string, string, TaskSessionStateChangeType)[] taskSettings = new (string, string, TaskSessionStateChangeType)[]
        {
            new ("lock", "Lock", TaskSessionStateChangeType.SessionLock),
            new ("unlock", "Unlock", TaskSessionStateChangeType.SessionUnlock)
        };

        foreach (var row in taskSettings)
        {
            TaskDefinition definition = TaskService.Instance.NewTask();
            definition.RegistrationInfo.Author = user.DomainUser;
            definition.RegistrationInfo.Date = DateTime.Now;
            definition.RegistrationInfo.Description = $"WTWD Explicit {row.Item2}";
            definition.Principal.LogonType = TaskLogonType.InteractiveToken;
            definition.Principal.UserId = user.DomainUser;
            definition.Settings.AllowDemandStart = false;
            definition.Settings.AllowHardTerminate = true;
            definition.Settings.DisallowStartIfOnBatteries = false;
            definition.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(2);
            definition.Settings.MultipleInstances = TaskInstancesPolicy.Parallel;
            definition.Settings.RestartCount = 3;
            definition.Settings.StartWhenAvailable = true;

            SessionStateChangeTrigger trigger = new SessionStateChangeTrigger(row.Item3, user.DomainUser);
            trigger.ExecutionTimeLimit = TimeSpan.FromMinutes(2);
            definition.Triggers.Add(trigger);

            ExecAction action = new ExecAction(cli.ExeFilePath, row.Item1);
            definition.Actions.Add(action);

            Task lockTask = wtwdFolder.RegisterTaskDefinition($"Explicit{row.Item2}", definition);
            lockTask.Enabled = true;

            Console.WriteLine($"Explicit {row.Item1} scheduled task created");
        }
    }
}
