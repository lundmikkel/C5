using System;
using System.Collections.Generic;
using System.Net;
using C5.intervals;

namespace C5.UserGuideExamples.intervals
{
    public class FeedReader
    {
        /// <summary>
        /// Creates an interval collection from the intervals found in the calendar at the url. See the calendar at https://www.google.com/calendar/embed?src=bechmellson.com_eoauecnh84i50tbftksd5bfdl4@group.calendar.google.com&ctz=Europe/Copenhagen
        /// </summary>
        public static void Main(string[] args)
        {
            string url = "http://www.google.com/calendar/ical/bechmellson.com_eoauecnh84i50tbftksd5bfdl4%40group.calendar.google.com/public/basic.ics";

            // Anders' FaceBook events - see how to get your URL here http://www.askdavetaylor.com/subscribe_to_facebook_calendar_ical/
            //url = "http://www.facebook.com/ical/u.php?uid=100002201102330&key=AQC8FQYGgkuL2Ajy";

            // Parse url
            var events = ParseUrl(url, FeedType.Ics);

            // Print events
            foreach (var e in events)
                Console.Out.WriteLine(e);
            Console.Out.WriteLine();

            // Create interval collection
            var coll = new DynamicIntervalTree<CalendarEvent, DateTime>(events);
            Console.Out.WriteLine(coll.MaximumOverlap);
            Console.ReadLine();
        }

        public class CalendarEvent : IInterval<DateTime>, IComparable<CalendarEvent>
        {
            public string Title { get; private set; }
            public DateTime Low { get; private set; }
            public DateTime High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return false; } }
            public TimeSpan Duration { get { return High.Subtract(Low); } }

            public CalendarEvent(string title, DateTime startTime, DateTime endTime)
            {
                Title = title;
                Low = startTime;
                High = endTime;
            }

            public int CompareTo(CalendarEvent other)
            {
                return IntervalExtensions.CompareTo(this, other);
            }

            public override string ToString()
            {
                return String.Format("{0,-15}: {1} (duration: {2})", Title, IntervalExtensions.ToString(this), Duration);
            }
        }

        public enum FeedType
        {
            Ics
        }

        public static IEnumerable<CalendarEvent> ParseUrl(string url, FeedType type)
        {
            // In case of IOException take a look here: http://stackoverflow.com/questions/14432079/wcf-the-specified-registry-key-does-not-exist-in-base-channel-call#14432540
            return ParseString(new WebClient { Encoding = System.Text.Encoding.UTF8 }.DownloadString(url), type);

        }

        public static IEnumerable<CalendarEvent> ParseString(string feed, FeedType type)
        {
            // All the parsed events
            var events = new ArrayList<CalendarEvent>();

            // Example datestring to get the correct length for parsing
            var dateLength = @"19700101T000000Z".Length;

            int begin, end = 0;

            while ((begin = feed.IndexOf("BEGIN:VEVENT", end)) >= 0)
            {
                // Find end of event
                end = feed.IndexOf("END:VEVENT", begin);

                // Take a substring of the vevent found
                var vevent = feed.Substring(begin, end - begin);

                // Find title
                var index = vevent.IndexOf("SUMMARY");
                index = vevent.IndexOf(":", index) + 1;
                // TODO: Is this the proper way to find the title?
                var title = vevent.Substring(index, vevent.IndexOf("\r", index) - index);

                // Find start time
                index = vevent.IndexOf("DTSTART");
                index = vevent.IndexOf(":", index) + 1;
                var low = stringToDate(vevent.Substring(index, dateLength));

                // Find end time
                index = vevent.IndexOf("DTEND");
                index = vevent.IndexOf(":", index) + 1;
                var high = stringToDate(vevent.Substring(index, dateLength));

                // Create and add calendar event to list
                events.Add(new CalendarEvent(title, low, high));
            }

            // Sort events
            var eventArray = events.ToArray();
            Sorting.IntroSort(eventArray);

            return eventArray;
        }

        private static DateTime stringToDate(String dateString)
        {
            // TODO: Fix to parse the ICS date/date-time format
            var correctedDateString = dateString.Substring(0, dateString.Length - 3).Insert(4, "-").Insert(7, "-").Insert(13, ":");
            return DateTime.Parse(correctedDateString).AddHours(1);
        }
    }
}
