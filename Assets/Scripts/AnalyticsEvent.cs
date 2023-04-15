using System;

[Serializable]
public class AnalyticsEvent
{
    public string Type;
    public string Data;

    public AnalyticsEvent(string type, string data)
    {
        Type = type;
        Data = data;
    }
}
