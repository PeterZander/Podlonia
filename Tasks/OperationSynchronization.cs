using System;

namespace Podlonia.Tasks
{
    public enum OperationTypes { NoOperation, RSSScan, EnclosureDownload, DeviceSync }
    public class OperationIdentification
    {
        internal long Id;
    }
    public static class OperationSynchronization
    {
        public static event Action<OperationIdentification> OperationStarting;
        public static event Action<OperationIdentification> OperationEnding;
        public static event Action<OperationIdentification,string,decimal> OperationProgressing;

        static OperationTypes CurrentOperation = OperationTypes.NoOperation;
        static object CurrentOperationLock = new object();
        static long LastOperationId = 0;
        public static OperationIdentification StartOperation( OperationTypes ot )
        {
            lock ( CurrentOperationLock )
            {
                if ( CurrentOperation != OperationTypes.NoOperation ) return null;

                CurrentOperation = ot;
                var result = new OperationIdentification { Id = ++LastOperationId };

                try
                {
                    OperationStarting?.Invoke( result );
                }
                catch( Exception ex )
                {
                    Program.Log( ex.Message );
                }

                return result;
            }
        }
        public static bool EndOperation( OperationIdentification operationid )
        {
            lock ( CurrentOperationLock )
            {
                if ( CurrentOperation == OperationTypes.NoOperation ) return false;
                if ( operationid.Id != LastOperationId ) return false;

                CurrentOperation = OperationTypes.NoOperation;

                try
                {
                    OperationEnding?.Invoke( operationid );
                }
                catch( Exception ex )
                {
                    Program.Log( ex.Message );
                }

                return true;
            }
        }
        public static void OperationProgress( OperationIdentification operationid, string message, decimal completedpercent )
        {
            lock ( CurrentOperationLock )
            {
                if ( CurrentOperation == OperationTypes.NoOperation
                    || operationid.Id != LastOperationId )
                {
                    throw new InvalidOperationException( "Not the current operation" );
                }

                try
                {
                    OperationProgressing?.Invoke( operationid, message, completedpercent );
                }
                catch( Exception ex )
                {
                    Program.Log( ex.Message );
                }
           }
        }
    }
}