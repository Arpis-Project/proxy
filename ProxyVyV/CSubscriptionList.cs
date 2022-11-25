namespace ProxyVyV
{
    using System;
    using System.Collections.Generic;

    public class CSubscriptionList : List<CSubscription>
    {
        public void AddSubscription(string AName, Direction ADirection, bool AGet, bool APut, bool APost, bool ADelete, string AResource, string APattern, bool APayload, bool ARedirected)
        {
            CSubscription subscription1 = new CSubscription();
            subscription1.Name = AName;
            subscription1.Direction = ADirection;
            subscription1.Get = AGet;
            subscription1.Put = APut;
            subscription1.Post = APost;
            subscription1.Delete = ADelete;
            subscription1.Resource = AResource;
            subscription1.Pattern = APattern;
            subscription1.Payload = APayload;
            subscription1.Redirected = ARedirected;
            CSubscription item = subscription1;
            base.Add(item);
        }
    }
}

