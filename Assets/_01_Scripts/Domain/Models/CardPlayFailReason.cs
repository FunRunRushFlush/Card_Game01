using System;

[Serializable]
public struct CardPlayFailReason
{
    public CardPlayFailCode Code;
    public string Message;

    public CardPlayFailReason(CardPlayFailCode code, string message)
    {
        Code = code;
        Message = message;
    }
}
