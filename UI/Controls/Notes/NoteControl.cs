// ******************************************************************************************
//     Assembly:                Booger
//     Author:                  Terry D. Eppler
//     Created:                 08-08-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        08-08-2024
// ******************************************************************************************
// <copyright file="NoteControl.cs" company="Terry D. Eppler">
//    Booger is a quick & dirty WPF application that interacts with OpenAI GPT-3.5 Turbo API
//    based on NET6 and written in C-Sharp.
// 
//    Copyright ©  2024  Terry D. Eppler
// 
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the “Software”),
//    to deal in the Software without restriction,
//    including without limitation the rights to use,
//    copy, modify, merge, publish, distribute, sublicense,
//    and/or sell copies of the Software,
//    and to permit persons to whom the Software is furnished to do so,
//    subject to the following conditions:
// 
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
// 
//    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT.
//    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//    DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//    ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//    DEALINGS IN THE SOFTWARE.
// 
//    You can contact me at: terryeppler@gmail.com or eppler.terry@epa.gov
// </copyright>
// <summary>
//   NoteControl.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Control" />
    public class NoteControl : Control
    {
        static NoteControl( )
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( NoteControl ),
                new FrameworkPropertyMetadata( typeof( NoteControl ) ) );
        }

        public NoteControl( )
        {
            DependencyPropertyDescriptor
                .FromProperty( _showProperty, typeof( NoteControl ) ).AddValueChanged( this, ( s, e ) =>
                {
                    _contentRenderTransform = _translateTransform;
                    var _duration = new Duration( TimeSpan.FromMilliseconds( 200 ) );
                    var _ease = new CircleEase( )
                    {
                        EasingMode = EasingMode.EaseOut
                    };

                    var _xAnimation = new DoubleAnimation( -15, 0, _duration )
                    {
                        EasingFunction = _ease
                    };

                    var _opacityAnimation = new DoubleAnimation( 0, 1, _duration )
                    {
                        EasingFunction = _ease
                    };

                    if( _show )
                    {
                        Visibility = Visibility.Visible;
                        _xAnimation.From = -15;
                        _xAnimation.To = 0;
                        _opacityAnimation.From = 0;
                        _opacityAnimation.To = 1;
                    }
                    else
                    {
                        _xAnimation.From = 0;
                        _xAnimation.To = -15;
                        _opacityAnimation.From = 1;
                        _opacityAnimation.To = 0;
                        _opacityAnimation.Completed += ( s, e ) =>
                        {
                            Visibility = Visibility.Hidden;
                        };
                    }

                    _translateTransform.BeginAnimation( TranslateTransform.YProperty,
                        _xAnimation );

                    BeginAnimation( OpacityProperty, _opacityAnimation );
                } );
        }

        private readonly TranslateTransform _translateTransform =
            new TranslateTransform( );

        public string _text
        {
            get { return (string)GetValue( _textProperty ); }
            set { SetValue( _textProperty, value ); }
        }

        public bool _show
        {
            get { return (bool)GetValue( _showProperty ); }
            set { SetValue( _showProperty, value ); }
        }

        public Transform _contentRenderTransform
        {
            get { return (Transform)GetValue( _contentRenderTransformProperty ); }
            set { SetValue( _contentRenderTransformProperty, value ); }
        }

        // Using a DependencyProperty as the backing store for Show.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _showProperty =
            DependencyProperty.Register( nameof( _show ), typeof( bool ), typeof( NoteControl ),
                new PropertyMetadata( true ) );

        // Using a DependencyProperty as the backing store for Text.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _textProperty =
            DependencyProperty.Register( nameof( _text ), typeof( string ), typeof( NoteControl ),
                new PropertyMetadata( string.Empty ) );

        // Using a DependencyProperty as the backing store for ContentRenderTransform.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _contentRenderTransformProperty =
            DependencyProperty.Register( nameof( _contentRenderTransform ), typeof( Transform ),
                typeof( NoteControl ), new PropertyMetadata( null ) );
    }
}