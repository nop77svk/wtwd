# Work-Time Watch-Dog

_Warning:_ Windows-only, as it 100% relies on the Windows event log.

Ever needed to track the time you used (i.e., worked on) your PC? Do you have
to feed some corporate time tracking system with your work time info on
a weekly basis, yet you don't like (or forget about) clicking the time tracker
on each task change and when you already do, you for the hell cannot remember
when you were at your PC? Or you are home-officing with your kids at home,
having your work time fragmented as hell, so it's just plain impossible to
track your work time thoroughly and in detail? Do you spend 12 hours "working",
yet you just cannot seem to finish your work and you don't know where the time
got lost? Well, look no further...

Mine your Windows event log for PC startup/shutdown/wakeup/sleep/unlock/lock
events and transform them into a human readable information on the times when
your PC has been used. It does not track your tasks for you, but it at least
gives you clues on whether your seemingly 12-hour work day indeed took 12
hours or was it more like 3 hours of PC work and 9 hours of attending to your
children in perpetual desperate need of your attention.

Just run and see the results.

Tested (as of July 13, 2022) on Windows 10 only.

_Note:_ Lock+Unlock events do not exist on all Windows 10 systems. This will
be worked on in some future releases.
