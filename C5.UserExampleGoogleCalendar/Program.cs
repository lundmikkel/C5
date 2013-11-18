using System;
using System.Collections.Generic;
using System.Net;
using C5.intervals;

namespace C5.UserExampleGoogleCalendar
{
    class Program
    {
        /// <summary>
        /// Creates an interval collection from the intervals found in the calendar at the url. See the calendar at https://www.google.com/calendar/embed?src=bechmellson.com_eoauecnh84i50tbftksd5bfdl4@group.calendar.google.com&ctz=Europe/Copenhagen
        /// </summary>
        public static void Main(string[] args)
        {
            const string url = "http://www.google.com/calendar/ical/bechmellson.com_eoauecnh84i50tbftksd5bfdl4%40group.calendar.google.com/public/basic.ics";
            // Parse url
            var events = parseUrlToCalendarEvents(url);

            // Print events
            foreach (var e in events)
                Console.Out.WriteLine(e);
            Console.Out.WriteLine();

            // Create interval collection
            var coll = new DynamicIntervalTree<CalendarEvent, DateTime>(events);
            Console.Out.WriteLine(coll.MaximumOverlap);
            Console.ReadLine();
        }

        class CalendarEvent : IInterval<DateTime>
        {
            public string Title { get; private set; }
            public DateTime Low { get; private set; }
            public DateTime High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return false; } }
            public TimeSpan Length { get { return High.Subtract(Low); } }

            public CalendarEvent(string title, DateTime startTime, DateTime endTime)
            {
                Title = title;
                Low = startTime;
                High = endTime;
            }

            public override string ToString()
            {
                return IntervalExtensions.ToString(this) + " Length = " + Length;
            }
        }

        private static IEnumerable<CalendarEvent> parseUrlToCalendarEvents(string url)
        {
            // In case of IOException take a look here: http://stackoverflow.com/questions/14432079/wcf-the-specified-registry-key-does-not-exist-in-base-channel-call#14432540
            var contents = new WebClient().DownloadString(url);
            // All the parsed events
            var events = new ArrayList<CalendarEvent>();

            // Example datestring to get the correct length for parsing
            var dsLen = @"19700101T000000Z".Length;
            var index = contents.IndexOf("DTSTART:", StringComparison.Ordinal);

            do
            {
                // TODO: Parse title
                var title = String.Empty;
                var low = stringToDate(contents.Substring(index + "DTSTART:".Length, dsLen));
                contents = contents.Substring(index + dsLen + "DTEND:".Length + "DTSTART:".Length + "\r\n".Length);
                var high = stringToDate(contents.Substring(0, dsLen));

                // Create and add calendar event to list
                events.Add(new CalendarEvent(title, low, high));

                index = contents.IndexOf("DTSTART:", StringComparison.Ordinal);
            } while (index >= 0);

            return events;
        }

        private static DateTime stringToDate(String dateString)
        {
            var correctedDateString = dateString.Substring(0, dateString.Length - 3).Insert(4, "-").Insert(7, "-").Insert(13, ":");
            return DateTime.Parse(correctedDateString).AddHours(1);
        }
    }
}
