namespace WispoRoboto.Chatbot.Extensions;

public static class IntExtensions
{
    public static DateTime UnixTimestampToDateTime(this int unixTimestamp)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimestamp).ToLocalTime();
        return dateTime;
    }
}