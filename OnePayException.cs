using System;

public class OnePayException : Exception
{
    public OnePayException()
    {
    }

    public OnePayException(string JPayload) : base(JPayload)
    {
    }
}

