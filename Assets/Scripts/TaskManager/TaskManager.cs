﻿using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace Tasks
{
	public class TaskManager : IDisposable
	{
		private readonly TaskRunner runner;

		public TaskManager(int executorCount = 7)
		{
			runner = new TaskRunner(executorCount);
		}

		public IDependency ScheduleSingle(ITask task, 
			IDependency dependency = null, ITracker tracker = null)
		{
			SingleTaskHandle handle = new SingleTaskHandle(task, runner);
			Schedule(handle, dependency, tracker);
			return handle;
		}

		public IDependency ScheduleArray<T1>(T1[] data, ITask<T1> task, int batchSize = 10, 
			IDependency dependency = null, ITracker tracker = null)
			where T1 : struct
		{
			ArrayTaskHandle<T1> handle = new ArrayTaskHandle<T1>(data, task, runner);	
			Schedule(handle, dependency, tracker, batchSize);
			return handle;
		}

		public IDependency ScheduleArray<T1, T2>(T1[] data1, T2[] data2, ITask<T1, T2> task, int batchSize = 10, 
			IDependency dependency = null, ITracker tracker = null)
			where T1 : struct
			where T2 : struct
		{
			ArrayTaskHandle<T1, T2> handle = new ArrayTaskHandle<T1, T2>(data1, data2, task, runner);
			Schedule(handle, dependency, tracker, batchSize);
			return handle;
		}

		public void Dispose()
		{
			runner.Dispose();
		}

		private void Schedule(BaseTaskHandle handle, IDependency dependency, ITracker tracker = null, int batchSize = 1)
		{
			//Note: Passing a tracker can be used to 'subscribe' to events before the tasks actually get scheduled one
			//advantage of this is that using this tracker will be thread-safe by default and you don't need to deal
			//with scheduling happening before you subscribe. See the wall of text below :)
			if(tracker != null)
				tracker.Track(handle);

			//Note thread-safety is a bit tricky because if we check the 'IsCompleted' first then subscribe if its not complete yet
			//then the completed event could actually be fired in between those calls. Thats why we first subscribe (even tho it might allready be complete)
			//and then schedule if its allready complete. In the worst case 'Schedule' gets called twice when the 'Completed' is fired in between the subscribing
			//and the if, but the handle will ignore the second schedule call
			if(dependency != null)
				dependency.Completed += () => handle.Schedule(batchSize);
			if(dependency == null || dependency.IsCompleted)
				handle.Schedule(batchSize);
		}
	}
}