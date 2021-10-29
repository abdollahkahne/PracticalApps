using System.Diagnostics.Tracing;

// I only copy-paste this!!!

namespace NorthwindCookieAuth.EventSources
{
    [EventSource(Name =SourceName)]
    public sealed class LogElapsedUrlEventSource:EventSource
    {
        const string SourceName="LogElapsedUrl";
        const int SourceId=1;
        private readonly EventCounter _counter;

        // since this class has a private constructor, this constructor can only called inside of class
        // In this case we create a static readonly field with the private constructor
        // So this is a singleton (There is another similar approach with Static Constructors which runns at First Instantiation where the instance constructor is )
        public static readonly LogElapsedUrlEventSource Instance=new LogElapsedUrlEventSource();
        private LogElapsedUrlEventSource():base(EventSourceSettings.EtwSelfDescribingEventFormat) {
            _counter=new EventCounter(SourceName,this);
        }

        [Event(SourceId,Message="Elapsed Time for URL {0}:{1}",Level =EventLevel.Informational)]
        public void LogElapsed(string url,float time) {
            WriteEvent(SourceId,url,time);
            _counter.WriteMetric(time);
        }

        
    }
}