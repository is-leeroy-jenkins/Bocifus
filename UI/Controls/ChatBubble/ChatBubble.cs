// ******************************************************************************************
//     Assembly:                Booger
//     Author:                  Terry D. Eppler
//     Created:                 08-08-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        08-08-2024
// ******************************************************************************************
// <copyright file="ChatBubble.cs" company="Terry D. Eppler">
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
//   ChatBubble.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <inheritdoc />
    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBeInternal" ) ]
    public class ChatBubble : Control
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes the <see cref="T:Bocifus.ChatBubble" /> class.
        /// </summary>
        static ChatBubble( )
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ChatBubble ),
                new FrameworkPropertyMetadata( typeof( ChatBubble ) ) );
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string _username
        {
            get { return (string)GetValue( _usernameProperty ); }
            set { SetValue( _usernameProperty, value ); }
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string _content
        {
            get { return (string)GetValue( _contentProperty ); }
            set { SetValue( _contentProperty, value ); }
        }

        /// <summary>
        /// Gets or sets the header foreground.
        /// </summary>
        /// <value>
        /// The header foreground.
        /// </value>
        public Brush _headerForeground
        {
            get { return (Brush)GetValue( _headerForegroundProperty ); }
            set { SetValue( _headerForegroundProperty, value ); }
        }

        /// <summary>
        /// Gets or sets the header background.
        /// </summary>
        /// <value>
        /// The header background.
        /// </value>
        public Brush _headerBackground
        {
            get { return (Brush)GetValue( _headerBackgroundProperty ); }
            set { SetValue( _headerBackgroundProperty, value ); }
        }

        /// <summary>
        /// Gets or sets the content foreground.
        /// </summary>
        /// <value>
        /// The content foreground.
        /// </value>
        public Brush _contentForeground
        {
            get { return (Brush)GetValue( _contentForegroundProperty ); }
            set { SetValue( _contentForegroundProperty, value ); }
        }

        /// <summary>
        /// Gets or sets the content background.
        /// </summary>
        /// <value>
        /// The content background.
        /// </value>
        public Brush _contentBackground
        {
            get { return (Brush)GetValue( _contentBackgroundProperty ); }
            set { SetValue( _contentBackgroundProperty, value ); }
        }

        /// <summary>
        /// Gets or sets the corner radius.
        /// </summary>
        /// <value>
        /// The corner radius.
        /// </value>
        public CornerRadius _cornerRadius
        {
            get { return (CornerRadius)GetValue( _cornerRadiusProperty ); }
            set { SetValue( _cornerRadiusProperty, value ); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is readonly.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is readonly; otherwise, <c>false</c>.
        /// </value>
        public bool _isReadonly
        {
            get { return (bool)GetValue( _isReadonlyProperty ); }
            set { SetValue( _isReadonlyProperty, value ); }
        }

        // Using a DependencyProperty as the backing store for Username.
        // This enables animation, styling, binding, etc...
        /// <summary>
        /// The username property
        /// </summary>
        public static readonly DependencyProperty _usernameProperty =
            DependencyProperty.Register( nameof( _username ), typeof( string ),
                typeof( ChatBubble ), new PropertyMetadata( string.Empty ) );

        // Using a DependencyProperty as the backing store for Content.
        // This enables animation, styling, binding, etc...
        /// <summary>
        /// The content property
        /// </summary>
        public static readonly DependencyProperty _contentProperty =
            DependencyProperty.Register( nameof( _content ), typeof( string ),
                typeof( ChatBubble ), new PropertyMetadata( string.Empty ) );

        // Using a DependencyProperty as the backing store for HeaderForeground.
        // This enables animation, styling, binding, etc...
        /// <summary>
        /// The header foreground property
        /// </summary>
        public static readonly DependencyProperty _headerForegroundProperty =
            DependencyProperty.Register( nameof( _headerForeground ), typeof( Brush ),
                typeof( ChatBubble ), new PropertyMetadata( Brushes.Gray ) );

        // Using a DependencyProperty as the backing store for HeaderBackground.
        // This enables animation, styling, binding, etc...
        /// <summary>
        /// The header background property
        /// </summary>
        public static readonly DependencyProperty _headerBackgroundProperty =
            DependencyProperty.Register( nameof( _headerBackground ), typeof( Brush ),
                typeof( ChatBubble ), new PropertyMetadata( Brushes.Transparent ) );

        // Using a DependencyProperty as the backing store for ContentForeground.  T
        // his enables animation, styling, binding, etc...
        /// <summary>
        /// The content foreground property
        /// </summary>
        public static readonly DependencyProperty _contentForegroundProperty =
            DependencyProperty.Register( nameof( _contentForeground ), typeof( Brush ),
                typeof( ChatBubble ), new PropertyMetadata( Brushes.Black ) );

        // Using a DependencyProperty as the backing store for ContentBackground.
        // This enables animation, styling, binding, etc...
        /// <summary>
        /// The content background property
        /// </summary>
        public static readonly DependencyProperty _contentBackgroundProperty =
            DependencyProperty.Register( nameof( _contentBackground ), typeof( Brush ),
                typeof( ChatBubble ), new PropertyMetadata( Brushes.White ) );

        // Using a DependencyProperty as the backing store for CornerRadius.
        // This enables animation, styling, binding, etc...
        /// <summary>
        /// The corner radius property
        /// </summary>
        public static readonly DependencyProperty _cornerRadiusProperty =
            DependencyProperty.Register( nameof( _cornerRadius ), typeof( CornerRadius ),
                typeof( ChatBubble ), new PropertyMetadata( new CornerRadius( 0 ) ) );

        // Using a DependencyProperty as the backing store for IsReadonly.
        // This enables animation, styling, binding, etc...
        /// <summary>
        /// The is readonly property
        /// </summary>
        public static readonly DependencyProperty _isReadonlyProperty =
            DependencyProperty.Register( nameof( _isReadonly ), typeof( bool ),
                typeof( ChatBubble ), new PropertyMetadata( true ) );
    }
}