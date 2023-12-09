using Raylib_CsLo;

namespace PiShockDesktop
{
    public class Timer
    {
        /* VARIABLES */
        public Action TimerDoneAction;
        public double TimerStartTime = 0;   // Start time (seconds)
        public double TimerDuration = 0;    // Lifetime (seconds)
        public double TimeUntilDone = 0;
        public bool TimerStarted = false;

        /* CONSTRUCTORS */
        public Timer(double Duration, Action DoneAction)
        {
            TimerDoneAction = DoneAction;
            TimerDuration = Duration;
            TimeUntilDone = Duration;
        }

        /* FUNCTIONS */
        public void Start()
        {
            TimerStartTime = Raylib.GetTime();
            TimeUntilDone = TimerDuration;
            TimerStarted = true;
        }

        public void Stop()
        {
            TimeUntilDone = 0;
            TimerStarted = false;
        }

        public void Reset()
        {
            TimeUntilDone = TimerDuration;
            Stop();
        }

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

        public bool TimerDone()
        {
            return TimeUntilDone <= 0 && TimerStarted;
        }

        public double GetElapsed()
        {
            return TimerStartTime - TimeUntilDone;
        }
    }
}
