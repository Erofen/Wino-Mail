﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Wino.Messages.Mails
{
    /// <summary>
    /// When mail save as PDF requested.
    /// </summary>
    public record SaveAsPDFRequested(string FileSavePath);
}
