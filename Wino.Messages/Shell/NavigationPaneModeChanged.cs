﻿using Wino.Core.Domain.Enums;

namespace Wino.Messages.Shell
{
    /// <summary>
    /// When navigation pane mode is changed.
    /// </summary>
    /// <param name="NewMode">New navigation mode.</param>
    public record NavigationPaneModeChanged(MenuPaneMode NewMode);
}
