﻿using Wino.Core.Domain;
using Wino.Core.Domain.Entities.Shared;

namespace Wino.Mail.ViewModels.Data;

public class AccountContactViewModel : AccountContact
{
    public AccountContactViewModel(AccountContact contact)
    {
        Address = contact.Address;
        Name = contact.Name;
        Base64ContactPicture = contact.Base64ContactPicture;
        IsRootContact = contact.IsRootContact;
    }

    /// <summary>
    /// Gets or sets whether the contact is the current account.
    /// </summary>
    public bool IsMe { get; set; }

    /// <summary>
    /// Provides a short name of the contact.
    /// <see cref="ShortDisplayName"/> or "You"
    /// </summary>
    public string ShortNameOrYou => IsMe ? $"{Translator.AccountContactNameYou};" : ShortDisplayName;

    /// <summary>
    /// Short display name of the contact.
    /// Either Name or Address.
    /// </summary>
    public string ShortDisplayName => Address == Name || string.IsNullOrWhiteSpace(Name) ? $"{Address.ToLowerInvariant()};" : $"{Name};";

    /// <summary>
    /// Display name of the contact in a format: Name <Address>.
    /// </summary>
    public string DisplayName => Address == Name || string.IsNullOrWhiteSpace(Name) ? Address.ToLowerInvariant() : $"{Name} <{Address.ToLowerInvariant()}>";
}
