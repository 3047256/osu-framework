// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Input.StateChanges;
using osuTK;

namespace osu.Framework.Input.States
{
    public class TabletState
    {
        public readonly ButtonStates<TabletPenButton> PenButtons = new ButtonStates<TabletPenButton>();
        public readonly ButtonStates<TabletAuxiliaryButton> AuxiliaryButtons = new ButtonStates<TabletAuxiliaryButton>();

        public Vector2 Position { set; get; }

        public double Pressure { set; get; }

        public bool IsStateValid { set; get; } = true;

        /// <summary>
        /// The last input source to make a change to the state.
        /// </summary>
        public IInput LastSource { get; set; }

        public bool IsButtonPressed(TabletPenButton button) => PenButtons.IsPressed(button);
        public void SetButtonPressed(TabletPenButton button, bool pressed) => PenButtons.SetPressed(button, pressed);

        public bool IsButtonPressed(TabletAuxiliaryButton button) => AuxiliaryButtons.IsPressed(button);
        public void SetButtonPressed(TabletAuxiliaryButton button, bool pressed) => AuxiliaryButtons.SetPressed(button, pressed);

        public override string ToString()
        {
            string output;
            if (IsStateValid)
            {
                output = $"({Position.X:F0},{Position.Y:F0}) "
                    + $"PenButtons [{PenButtons}] "
                    + $"AuxiliaryButtons [{AuxiliaryButtons}] "
                    + $"Pressure {Pressure * 100}%";
            }
            else
            {
                output = "(Invalid)";
            }
            return $@"{GetType().ReadableName()} {output}";
        }
    }
}
