// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;

namespace osu.Framework.Tests.Visual.Performance
{
    public partial class TestSceneSpriteTextPerformance : PerformanceTestScene
    {
        private int wordLength;
        private int wordsCount;
        private int paragraphsCount;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddLabel("Sprite Texts");
            AddSliderStep("word length", 1, 10, 5, v =>
            {
                wordLength = v;
                recreate();
            });

            AddSliderStep("words count", 1, 100, 20, v =>
            {
                wordsCount = v;
                recreate();
            });

            AddSliderStep("paragraphs count", 1, 20, 1, v =>
            {
                paragraphsCount = v;
                recreate();
            });
        }

        private void recreate()
        {
            var text = new StringBuilder();

            for (int p = 0; p < paragraphsCount; p++)
            {
                for (int w = 0; w < wordsCount; w++)
                {
                    for (int c = 0; c < wordLength; c++)
                    {
                        char character = (char)RNG.Next('a', 'z');
                        character = c == 0 ? char.ToUpperInvariant(character) : character;
                        text.Append(character);
                    }

                    text.Append(' ');
                }

                text.AppendLine();
                text.AppendLine();
            }

            Child = new TextFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Text = text.ToString(),
            };
        }
    }
}
