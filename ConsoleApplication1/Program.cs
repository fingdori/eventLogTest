using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics.Eventing.Reader;

class ReadEventValuesExample
{
    static void Main(string[] args)
    {
        String logName = "Application";
        String queryString = "*[System/Level=2]";
        UdpClient _udpClient;
        String host = "192.168.1.28";
        UTF8Encoding _encoding = new UTF8Encoding();

        EventLogQuery eventsQuery = new EventLogQuery(logName,
            PathType.LogName, queryString);

        EventLogReader logReader;
        Console.WriteLine("Querying the Application channel event log for all events...");
        try
        {
            // Query the log
            logReader = new EventLogReader(eventsQuery);
        }
        catch (EventLogNotFoundException e)
        {
            Console.WriteLine("Failed to query the log!");
            Console.WriteLine(e);
            return;
        }

        //////
        // This section creates a list of XPath reference strings to select
        // the properties that we want to display
        // In this example, we will extract the User, TimeCreated, EventID and EventRecordID
        //////
        // Array of strings containing XPath references
        String[] xPathRefs = new String[4];
        xPathRefs[0] = "Event/System/Security/@UserID";
        xPathRefs[1] = "Event/System/TimeCreated/@SystemTime";
        xPathRefs[2] = "Event/System/EventID";
        xPathRefs[3] = "Event/System/EventRecordID";
        // Place those strings in an IEnumberable object
        IEnumerable<String> xPathEnum = xPathRefs;
        // Create the property selection context using the XPath reference
        EventLogPropertySelector logPropertyContext = new EventLogPropertySelector(xPathEnum);

        _udpClient = new UdpClient();
        _udpClient.Connect(host, 5140);

        int numberOfEvents = 0;
        // For each event returned from the query
        for (EventRecord eventInstance = logReader.ReadEvent();
                eventInstance != null;
                eventInstance = logReader.ReadEvent())
        {
            IList<object> logEventProps;
            try
            {
                // Cast the EventRecord into an EventLogRecord to retrieve property values.
                // This will fetch the event properties we requested through the
                // context created by the EventLogPropertySelector
                logEventProps = ((EventLogRecord)eventInstance).GetPropertyValues(logPropertyContext);
                Console.WriteLine("Event {0} :", ++numberOfEvents);
                Console.WriteLine("User: {0}", logEventProps[0]);
                Console.WriteLine("TimeCreated: {0}", logEventProps[1]);
                Console.WriteLine("EventID: {0}", logEventProps[2]);
                Console.WriteLine("EventRecordID : {0}", logEventProps[3]);

                // Event properties can also be retrived through the event instance
                Console.WriteLine("Event Description:" + eventInstance.FormatDescription());
                Console.WriteLine("MachineName: " + eventInstance.MachineName);


                Console.WriteLine("=================== START ====================");
                Console.WriteLine(eventInstance.ToXml());
                Console.WriteLine("==============================================");

                String xmlString = eventInstance.ToXml();
                byte[] data = _encoding.GetBytes(xmlString);
                _udpClient.Send(data, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't render event!");
                Console.WriteLine("Exception: Event {0} may not have an XML representation \n\n", ++numberOfEvents);
                Console.WriteLine(e);
            }
        }
    }
}