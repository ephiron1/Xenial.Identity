﻿// Copyright 2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Serilog.Events;

using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;

#nullable enable

namespace Xenial.Identity.Infrastructure.Logging.MemoryConsole.Output;

internal class TextTokenRenderer : OutputTemplateTokenRenderer
{
    private readonly ConsoleTheme theme;
    private readonly string text;

    public TextTokenRenderer(ConsoleTheme theme, string text)
    {
        this.theme = theme;
        this.text = text;
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        var _ = 0;
        using (theme.Apply(output, ConsoleThemeStyle.TertiaryText, ref _))
        {
            output.Write(text);
        }
    }
}
