namespace br.dev.optimus.hermes.app.models
{
    internal class QueueStatus
    {
        public int Total { get; set; } = 0;
        public int Running { get; set; } = 0;
        public int Completed { get; set; } = 0;
        public int Errors { get; set; } = 0;

        public void PlusRunning()
        {
            Running++;
        }

        public void MinusRunning()
        {
            if (Running > 0) Running--;
        }
    }
}
