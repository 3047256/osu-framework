﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osuTK.Graphics;
using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Input;

namespace osu.Framework.Graphics.UserInterface
{
    public abstract partial class DropdownHeader : Container, IKeyBindingHandler<PlatformAction>
    {
        public event Action<DropdownSelectionAction> ChangeSelection;

        protected Container Background;
        protected Container Foreground;

        public bool AlwaysShowSearchBar
        {
            get => SearchBar.AlwaysDisplayOnFocus;
            set => SearchBar.AlwaysDisplayOnFocus = value;
        }

        protected internal DropdownSearchBar SearchBar { get; }

        public Bindable<string> SearchTerm => SearchBar.SearchTerm;

        private Color4 backgroundColour = Color4.DarkGray;

        protected Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                updateState();
            }
        }

        private Color4 disabledColour = Color4.Gray;

        protected Color4 DisabledColour
        {
            get => disabledColour;
            set
            {
                disabledColour = value;
                updateState();
            }
        }

        protected Color4 BackgroundColourHover { get; set; } = Color4.Gray;

        protected override Container<Drawable> Content => Foreground;

        protected internal abstract LocalisableString Label { get; set; }

        public BindableBool Enabled { get; } = new BindableBool(true);

        public Action ToggleMenu;

        protected DropdownHeader()
        {
            Masking = true;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Width = 1;

            InternalChildren = new Drawable[]
            {
                Background = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DarkGray,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    },
                },
                Foreground = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                SearchBar = CreateSearchBar(),
            };
        }

        protected abstract DropdownSearchBar CreateSearchBar();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(_ => updateState(), true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Enabled.Value)
                return false;

            ToggleMenu?.Invoke();
            return false;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        public void UpdateSearchBarFocus(MenuState state)
        {
            if (state == MenuState.Open)
                SearchBar.ObtainFocus();
            else
                SearchBar.ReleaseFocus();
        }

        private void updateState()
        {
            Colour = Enabled.Value ? Color4.White : DisabledColour;
            Background.Colour = IsHovered && Enabled.Value ? BackgroundColourHover : BackgroundColour;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!IsHovered)
                return false;

            if (!Enabled.Value)
                return true;

            switch (e.Key)
            {
                case Key.Up:
                    ChangeSelection?.Invoke(DropdownSelectionAction.Previous);
                    return true;

                case Key.Down:
                    ChangeSelection?.Invoke(DropdownSelectionAction.Next);
                    return true;

                default:
                    return base.OnKeyDown(e);
            }
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (!Enabled.Value)
                return true;

            switch (e.Action)
            {
                case PlatformAction.MoveToListStart:
                    ChangeSelection?.Invoke(DropdownSelectionAction.First);
                    return true;

                case PlatformAction.MoveToListEnd:
                    ChangeSelection?.Invoke(DropdownSelectionAction.Last);
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        public enum DropdownSelectionAction
        {
            Previous,
            Next,
            First,
            Last,
            FirstVisible,
            LastVisible
        }
    }
}
