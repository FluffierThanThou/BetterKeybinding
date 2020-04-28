﻿
using System.Linq;
using UnityEngine;
using Verse;

namespace BetterKeybinding
{
    public class KeyBind: IExposable
    {
        public string label;
        public InputType type;
        public EventModifiers modifiers;
        public KeyCode key;
        public int button;
        public Vector2 scrollDirection;
        private bool editMode;

        public KeyBind()
        {
            // scribe
        }

        public KeyBind( string label, int button, EventModifiers modifiers = EventModifiers.None )
        {
            type = InputType.MouseButton;
            this.button = button;
            this.modifiers = modifiers;
            this.label = label;
        }

        public KeyBind( string label, Vector2 scrollDirection, EventModifiers modifiers = EventModifiers.None )
        {
            type = InputType.ScrollWheel;
            this.scrollDirection = scrollDirection;
            this.modifiers = modifiers;
            this.label = label;
        }

        public KeyBind( string label, KeyCode key, EventModifiers modifiers = EventModifiers.None )
        {
            type = InputType.Key;
            this.key = key;
            this.modifiers = modifiers;
            this.label = label;
        }

        public static implicit operator KeyBind( KeyBindingDef keybinding )
        {
            return new KeyBind( keybinding.label, keybinding.MainKey );
        }

        public void Draw(Rect canvas, int margin = 6)
        {
            var labelRect = new Rect(
                    canvas.min,
                    new Vector2(canvas.width * 2 / 3f, canvas.height))
               .ContractedBy(margin);
            var previewRect = new Rect(
                    labelRect.xMax,
                    canvas.yMin,
                    canvas.width / 3f,
                    canvas.height )
               .ContractedBy( margin );

            Widgets.Label( labelRect, label.CapitalizeFirst() );
            if ( Widgets.ButtonInvisible( previewRect ) ) editMode = true;
            Widgets.DrawOptionBackground(previewRect, editMode);
            Text.Anchor = TextAnchor.MiddleCenter;
            if (editMode)
            {
                Widgets.Label(previewRect, ModifierLabel(Event.current.modifiers));

                if (Event.current.type == EventType.MouseDown)
                {
                    type = InputType.MouseButton;
                    button = Event.current.button;
                    modifiers = Event.current.modifiers;
                    editMode = false;
                }
                else if (Event.current.type == EventType.KeyDown
                      && Event.current.keyCode != KeyCode.LeftControl
                      && Event.current.keyCode != KeyCode.RightControl
                      && Event.current.keyCode != KeyCode.LeftShift
                      && Event.current.keyCode != KeyCode.RightShift
                      && Event.current.keyCode != KeyCode.AltGr
                      && Event.current.keyCode != KeyCode.LeftAlt
                      && Event.current.keyCode != KeyCode.RightAlt )
                {
                    type = InputType.Key;
                    key = Event.current.keyCode;
                    modifiers = Event.current.modifiers;
                    editMode = false;
                }
                else if (Event.current.type == EventType.ScrollWheel)
                {
                    type = InputType.ScrollWheel;
                    scrollDirection = Event.current.delta.normalized;
                    modifiers = Event.current.modifiers;
                    editMode = false;
                }
            }
            else
            {
                Widgets.Label(previewRect, Label);
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }
        public string Label
        {
            get
            {
                var label = modifiers == EventModifiers.None ? "" : ModifierLabel( modifiers ) + " + ";
                switch ( type )
                {
                    case InputType.Key:
                        label += KeyLabel( key );
                        break;
                    case InputType.MouseButton:
                        label += ButtonLabel( button );
                        break;
                    case InputType.ScrollWheel:
                        label += ScrollLabel( scrollDirection );
                        break;
                    default:
                        label += "Unknown";
                        break;
                }

                return label;
            }
        }

        public static string ModifierLabel( EventModifiers modifiers )
        {
            return string.Join(
                " + ",
                modifiers.GetAllSelectedItems<EventModifiers>()
                         .Where( m => m != EventModifiers.None )
                         .Select( e => e.ToString() ) );
        }

        public static string ButtonLabel( int button )
        {
            switch ( button )
            {
                case 0:
                    return "LMB";
                case 1:
                    return "RMB";
                case 2:
                    return "MMB";
                default:
                    return $"Button {button}";
            }
        }

        public static string KeyLabel( KeyCode key )
        {
            return key.ToStringReadable();
        }

        public static string ScrollLabel( Vector2 delta )
        {
            var normalized = delta.normalized;
            if ( normalized == Vector2.up )
                return "ScrollDown";
            if ( normalized == Vector2.down )
                return "ScrollUp";
            if ( normalized == Vector2.left )
                return "ScrollLeft";
            if ( normalized == Vector2.right )
                return "ScrollRight";
            return "ScrollUnknown";
        }

        public bool ModifiersMatch( Event @event )
        {
            return (@event.modifiers & modifiers) == modifiers;
        }

        public bool KeyCodeMatches( Event @event )
        {
            return @event.type == EventType.KeyDown && @event.keyCode == key;
        }

        public bool ButtonMatches( Event @event )
        {
            return @event.type == EventType.MouseDown && @event.button == button;
        }

        public bool ScrollMatches( Event @event )
        {
            return @event.type == EventType.ScrollWheel && @event.delta.normalized == scrollDirection;
        }

        public bool JustPressed
        {
            get
            {
                var @event = Event.current;
                if ( !ModifiersMatch( @event ) )
                    return false;

                switch ( type )
                {
                    case InputType.Key:
                        return KeyCodeMatches( @event );
                    case InputType.MouseButton:
                        return ButtonMatches( @event );
                    case InputType.ScrollWheel:
                        return ScrollMatches( @event );
                    default:
                        Log.Warning( $"unknown keybind type: {type}"  );
                        return false;
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look( ref label, "label" );
            Scribe_Values.Look( ref type, "type" );
            Scribe_Values.Look( ref modifiers, "modifiers" );
            Scribe_Values.Look( ref key, "key" );
            Scribe_Values.Look( ref button, "button" );
            Scribe_Values.Look( ref scrollDirection, "scrollDirection" );
        }
    }
}
