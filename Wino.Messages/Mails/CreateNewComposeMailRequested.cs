﻿using Wino.Core.Domain.Models.Reader;

namespace Wino.Messages.Mails
{
    /// <summary>
    /// When a new composing requested.
    /// </summary>
    /// <param name="RenderModel"></param>
    public record CreateNewComposeMailRequested(MailRenderModel RenderModel);
}
