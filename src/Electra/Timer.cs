/* DIRECTVES */
#region DIRECTIVES
using Raylib_CsLo;
#endregion

/* NAMESPACES */
#region NAMESPACES
namespace Electra
{
    public class Timer
    {
        /* VARIABLES */
        #region VARIABLES
        public Action TimerDoneAction;
        public double TimerStartTime = 0;   // Start time (seconds)
        public double TimerDuration = 0;    // Lifetime (seconds)
        public double TimeUntilDone = 0;
        public bool TimerStarted = false;
        public bool Recurring = false;
        #endregion

        /* CONSTRUCTORS */
        #region CONSTRUCTORS
        public Timer(double Duration, Action DoneAction)
        {
            TimerDoneAction = DoneAction;
            TimerDuration = Duration;
            TimeUntilDone = Duration;
        }
        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        /// <summary>
        /// Start the timer.
        /// </summary>
        public void Start()
        {
            TimerStartTime = Raylib.GetTime();
            TimeUntilDone = TimerDuration;
            TimerStarted = true;
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void Stop()
        {
            TimeUntilDone = 0;
            TimerStarted = false;
        }

        /// <summary>
        /// Reset the timer.
        /// </summary>
        public void Reset()
        {
            TimeUntilDone = TimerDuration;
            Stop();

            if (Recurring)
            {
                Start();
            }
        }

        /// <summary>
        /// Update the timer.
        /// </summary>
        public void Update()
        {
            if (TimeUntilDone > 0)
            {
                TimeUntilDone -= Raylib.GetFrameTime();
            }

            if (TimerDone())
            {
                TimerDoneAction.Invoke();
                Reset();
            }
        }

        /// <summary>
        /// Check if the timer is done.
        /// </summary>
        /// <returns>Returns true if finished and false if not finished.</returns>
        public bool TimerDone()
        {
            return TimeUntilDone <= 0 && TimerStarted;
        }

        /// <summary>
        /// Get the elapsed time since the timer was started.
        /// </summary>
        /// <returns>Returns tehe elapsed time since the timer was started.</returns>
        public double GetElapsed()
        {
            return TimerStartTime - TimeUntilDone;
        }
        #endregion
    }
}
#endregion