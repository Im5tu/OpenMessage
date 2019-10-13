using System;
using System.Diagnostics;
using OpenMessage.Extensions;

namespace OpenMessage
{
    /// <summary>
    /// Helper for writing out activities to the ecosystem
    /// </summary>
    public static class Trace
    {
        private static readonly DiagnosticListener _listener = new DiagnosticListener("OpenMessage");

        /// <summary>
        /// Records an activity with the specified operation name
        /// </summary>
        /// <param name="operationName">The name to give to the activity</param>
        /// <param name="parentId">The parent of the activity if not known</param>
        public static ActivityTracer WithActivity(string operationName, string parentId = null)
        {
            operationName.Must().NotBeNull();
            var activity = new Activity(operationName);

            if (parentId != null)
                activity.SetParentId(parentId);

            _listener.StartActivity(activity, AnonymousObject.Empty);
            return new ActivityTracer(_listener, activity);
        }

        /// <summary>
        /// Automatically records the end of an activity on dispose
        /// </summary>
        public struct ActivityTracer : IDisposable
        {
            private readonly DiagnosticListener _listener;
            private readonly Activity _activity;

            internal ActivityTracer(DiagnosticListener listener, Activity activity)
            {
                _listener = listener;
                _activity = activity;
            }

            /// <summary>
            /// Dispose...
            /// </summary>
            public void Dispose()
            {
                _listener?.StopActivity(_activity, AnonymousObject.Empty);
            }
        }
    }
}