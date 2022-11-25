namespace ProxyVyV
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void ProxyEventNotification(string Direction, ref int StatusCode, string HTTPVerb, string ResourceName, string URL, ref string Headers, ref string Params, ref string Payload, string ContentType, ref bool CanContinue);
}

