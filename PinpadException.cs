using System;

public class PinpadException : Exception
{
    public PinpadException()
    {
    }

    public PinpadException(string JPayload) : base(JPayload)
    {
    }
}

