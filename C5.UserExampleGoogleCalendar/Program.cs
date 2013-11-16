using System;
using System.Collections.Generic;
using System.Net;
using C5.intervals;

namespace C5.UserExampleGoogleCalendar
{
    class Program
    {
        static void Main(string[] args)
        {
            // Interval Calendar - see it at https://www.google.com/calendar/embed?src=bechmellson.com_eoauecnh84i50tbftksd5bfdl4@group.calendar.google.com&ctz=Europe/Copenhagen
            const string url = "http://www.google.com/calendar/ical/bechmellson.com_eoauecnh84i50tbftksd5bfdl4%40group.calendar.google.com/public/basic.ics";
            var events = GetCalendarEvents(url);
            foreach (var e in events)
                Console.Out.WriteLine(e);
            Console.Out.WriteLine();

            var dit = eventsToDITCollenction(events);
            
            // TODO skal overlap returnere 0 eller 1 ved intet overlap?
            Console.Out.WriteLine(dit.MaximumOverlap);
            Console.ReadLine();
        }

        private static IIntervalCollection<IInterval<DateTime>, DateTime> eventsToDITCollenction(List<CalendarEvent> events)
        {
            var dit = new DynamicIntervalTree<IInterval<DateTime>, DateTime>();
            foreach (var e in events)
            {
                dit.Add(new IntervalBase<DateTime>(e.StartTime,e.EndTime, lowIncluded: true, highIncluded: true));
            }
            return dit;
        }

        private static List<CalendarEvent> GetCalendarEvents(string url)
        {
            var contents = new WebClient().DownloadString(url);

            // Example datestring from the feed to get the correct length for parsing
            var dsLen = "20131114T123000Z".Length;
            var dates = new List<DateTime>();
            var index = contents.IndexOf("DTSTART:", StringComparison.Ordinal);
            do
            {
                dates.Add(stringToDate(contents.Substring(index + "DTSTART:".Length, dsLen)));
                contents = contents.Substring(index + dsLen + "DTEND:".Length + "DTSTART:".Length + "\r\n".Length);
                dates.Add(stringToDate(contents.Substring(0, dsLen)));
                index = contents.IndexOf("DTSTART:", StringComparison.Ordinal);
            } while (index >= 0);
            return parseCalendarFeed(dates);
        }

        private static DateTime stringToDate(String dateString)
        {
            var correctedDateString = dateString.Substring(0, dateString.Length - 3).Insert(4, "-").Insert(7, "-").Insert(13, ":");
            return DateTime.Parse(correctedDateString).AddHours(1);
        }

        class CalendarEvent
        {
            public DateTime StartTime { get; private set; }
            public DateTime EndTime { get; private set; }

            public TimeSpan Length()
            {
                return EndTime.Subtract(StartTime);
            }

            public CalendarEvent(DateTime startTime, DateTime endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
            }

            public override string ToString()
            {
                return StartTime + " -> " + EndTime + " Length = " + Length();
            }
        }

        private static List<CalendarEvent> parseCalendarFeed(List<DateTime> dates)
        {
            var events = new List<CalendarEvent>();
            for (var i = 0; i < dates.Count; i += 2)
                events.Add(new CalendarEvent(dates[i], dates[i + 1]));
            return events;
        }
    }
}
