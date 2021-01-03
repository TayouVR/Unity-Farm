using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SchedulerSystem : MonoBehaviour
{
	[System.Serializable]
	protected class JobParameters
	{
		public string methodName;
		public float startTime;
		public float invokeRate;
		public int invokeCount;

		[Header("Hidden Variables")]
		public float m_StartTime;
		public float m_TargetTime;
		public int m_CurrentInvokeCount;

		public bool IsInfinite { get { return invokeCount <= -1; } }

		public JobParameters () { }

		public JobParameters (string methodName, float startTime, float invokeRate, int invokeCount)
		{
			this.methodName = methodName;
			this.startTime = startTime;
			this.invokeRate = invokeRate;
			this.invokeCount = invokeCount;

			m_StartTime = Time.time;
			m_TargetTime = Time.time + startTime;
		}
	}

	protected struct JobParametersPair
	{
		public Job Job;
		public JobParameters JobParameters;
	}

	static SchedulerSystem m_Instance { get; set; }
	public static SchedulerSystem Instance
	{
		get
		{
			if (!m_Instance)
			{
				m_Instance = new GameObject("SchedulerSystem").AddComponent<SchedulerSystem>();
				DontDestroyOnLoad(m_Instance);
			}
			return m_Instance;
		}
	}

	public delegate void Job();
	List<JobParametersPair> activeJobs = new List<JobParametersPair>();
	[SerializeField] List<JobParameters> activeJobsInspector = new List<JobParameters>();

	void Update ()
	{
		if (activeJobs.Count == 0)
			return;

		List<JobParametersPair> JobsList = activeJobs.FindAll(match => match.JobParameters != null);
    
		for (int i = 0; i < JobsList.Count; i++)
		{
			try
			{
				JobParameters m_JobParams = JobsList[i].JobParameters;

				if (Time.time >= m_JobParams.m_TargetTime)
				{
					Job job = JobsList[i].Job;
					job();

					m_JobParams.m_CurrentInvokeCount++;
					if (m_JobParams.m_CurrentInvokeCount >= m_JobParams.invokeCount && !m_JobParams.IsInfinite)
					{
						RemoveJob(job);
						i--;
					}

					m_JobParams.m_TargetTime = m_JobParams.m_StartTime + m_JobParams.invokeRate * m_JobParams.m_CurrentInvokeCount;
				}
			}
			catch (Exception e)
			{
				Debug.Log(e.Message + e.StackTrace);
				Debug.Log($"[Core:Scheduler] A job could not be run. Please notify the developers and send them the logfile. " + JobsList[i].Job.Method.Name);
			}
		}
	}

	public static void ClearAllJobs ()
	{
		Instance.activeJobs.Clear();
		Instance.activeJobsInspector.Clear();
		Debug.Log("[Core:Scheduler] Removed all jobs and cleared scheduler array.");
	}

	public static void RemoveJob (Job job)
	{
		JobParametersPair p = Instance.activeJobs.Find(match => match.Job == job);
		if (p.JobParameters != null)
		{
			Debug.Log("[Core:Scheduler] Removed job [" + job.Method.Name + "]");
			Instance.activeJobsInspector.Remove(p.JobParameters);
			Instance.activeJobs.Remove(p);
		}
		else
			Debug.Log("[Core:Scheduler] Failed to remove job [" + job.Method.Name + "] because it was not found!");
	}

	public static void AddJob (Job job, float startTime, float invokeRate)
	{
		AddJob(job, startTime, invokeRate, -1);
	}

	public static void AddJob (Job job, float invokeRate)
	{
		AddJob(job, 0, invokeRate, -1);
	}

	public static void AddJob (Job job, float startTime, float invokeRate, int invokeCount)
	{
		if (invokeCount == 0)
		{ 
			Debug.Log("[Core:Scheduler] Invoke count can't be equal to 0! Method Name: " + job.Method.Name);
			return;
		}

		Debug.Log("[Core:Scheduler] Added scheduler task: " + job.Method.Name);
		JobParameters m_Job = new JobParameters(job.Method.Name, startTime, invokeRate, invokeCount);

		JobParametersPair p = new JobParametersPair()
		{
			Job = job,
			JobParameters = m_Job
		};
        
		Instance.activeJobs.Add(p);
		Instance.activeJobsInspector.Add(m_Job);
	}
}