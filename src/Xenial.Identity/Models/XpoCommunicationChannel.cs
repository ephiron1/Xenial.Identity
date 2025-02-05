﻿using DevExpress.Xpo;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Models;

[Persistent("CommunicationChannel")]
public class XpoCommunicationChannel : XpoIdentityBaseObjectString
{
    public XpoCommunicationChannel(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Id = Guid.NewGuid().ToString();
        ChannelType = CommunicationChannelType.Email;
    }

    private CommunicationChannelType channelType = CommunicationChannelType.Email;
    public CommunicationChannelType ChannelType
    {
        get => channelType;
        set => SetPropertyValue(nameof(ChannelType), ref channelType, value);
    }

    private string channelProviderType = "";
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    public string ChannelProviderType
    {
        get => channelProviderType;
        set => SetPropertyValue(nameof(ChannelProviderType), ref channelProviderType, value);
    }

    private string channelSettings = "";
    [Size(SizeAttribute.Unlimited)]
    public string ChannelSettings
    {
        get => channelSettings;
        set => SetPropertyValue(nameof(ChannelSettings), ref channelSettings, value);
    }
}

public enum CommunicationChannelType
{
    Email,
    Sms
}
