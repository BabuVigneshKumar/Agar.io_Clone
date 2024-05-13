using System;

[Serializable]
public class ServerEvent
{
    public string playerID;
    public double reqTime;
    public Action eventAction;
    public string GetEventID()
    {
        return playerID + reqTime;
    }
}
