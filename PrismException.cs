using System;

public class PrismException : Exception
{
    public PrismException()
    {
    }

    public PrismException(string JPayload) : base(JPayload)
    {
    }
}

