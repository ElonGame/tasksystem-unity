﻿using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace Tasks
{
	public class TaskManager : IDisposable
	{
		private readonly TaskRunner runner = new TaskRunner();

		public ITaskHandle<T> Schedule<T>(T data, ITask<T> task)
			where T : struct, ITaskData
		{
			SingleTaskHandle<T> handle = new SingleTaskHandle<T>(data, task, runner);
			handle.Schedule();
			return handle;
		}

		public ITaskHandle<T[]> ScheduleBatch<T>(T[] data, ITask<T> task)
			where T : struct, ITaskData
		{
			BatchTaskHandle<T> handle = new BatchTaskHandle<T>(data, task, runner);
			handle.Schedule();
			return handle;
		}

		public void Dispose()
		{
			runner.Dispose();
		}
	}
}