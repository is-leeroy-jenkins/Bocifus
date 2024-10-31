// ******************************************************************************************
//     Assembly:                Booger
//     Author:                  Terry D. Eppler
//     Created:                 08-08-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        08-08-2024
// ******************************************************************************************
// <copyright file="ConditionalControl.cs" company="Terry D. Eppler">
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
//   ConditionalControl.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <inheritdoc />
    public class ConditionalControl : Control
    {
        static ConditionalControl( )
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ConditionalControl ),
                new FrameworkPropertyMetadata( typeof( ConditionalControl ) ) );
        }

        public bool _condition
        {
            get { return (bool)GetValue( _conditionProperty ); }
            set { SetValue( _conditionProperty, value ); }
        }

        public FrameworkElement _elementWhileTrue
        {
            get
            {
                return (FrameworkElement)GetValue( _elementWhileTrueProperty );
            }
            set { SetValue( _elementWhileTrueProperty, value ); }
        }

        public FrameworkElement _elementWhileFalse
        {
            get
            {
                return (FrameworkElement)GetValue( _elementWhileFalseProperty );
            }
            set { SetValue( _elementWhileFalseProperty, value ); }
        }

        // Using a DependencyProperty as the backing store for Condition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _conditionProperty =
            DependencyProperty.Register( nameof( _condition ), typeof( bool ),
                typeof( ConditionalControl ), new PropertyMetadata( true ) );

        // Using a DependencyProperty as the backing store for ElementWhileTrue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _elementWhileTrueProperty =
            DependencyProperty.Register( nameof( _elementWhileTrue ),
                typeof( FrameworkElement ), typeof( ConditionalControl ),
                new PropertyMetadata( null ) );

        // Using a DependencyProperty as the backing store for ElementWhileFalse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _elementWhileFalseProperty =
            DependencyProperty.Register( nameof( _elementWhileFalse ),
                typeof( FrameworkElement ), typeof( ConditionalControl ),
                new PropertyMetadata( null ) );
    }
}