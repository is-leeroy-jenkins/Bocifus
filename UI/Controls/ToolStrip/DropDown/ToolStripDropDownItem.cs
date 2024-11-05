

namespace Bocifus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using Syncfusion.Windows.Tools.Controls;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:Syncfusion.Windows.Tools.Controls.ComboBoxItemAdv" />
    public class ToolStripDropDownItem : ComboBoxItemAdv
    {
        /// <summary>
        /// The theme
        /// </summary>
        private protected readonly DarkMode _theme = new DarkMode();

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used
        /// to store custom information about this element.
        /// </summary>
        public new object Tag { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Bobo.ToolStripDropDownItem" /> class.
        /// </summary>
        public ToolStripDropDownItem( )
            : base( )
        {
            // Control Properties
            SetResourceReference( StyleProperty, typeof( ComboBoxItemAdv ) );
            Background = _theme.ControlInterior;
            BorderBrush = _theme.ControlInterior;
            Foreground = _theme.LightBlueBrush;
            Margin = new Thickness( 1 );
            Height = 24;
            Padding = new Thickness( 10, 3, 1, 1 );
            BorderThickness = new Thickness( 0 );
            HorizontalContentAlignment = HorizontalAlignment.Left;

            // Event Wiring
            MouseEnter += OnItemMouseEnter;
            MouseLeave += OnItemMouseLeave;
        }

        /// <summary>
        /// Called when [item mouse enter].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/>
        /// instance containing the event data.</param>
        private protected void OnItemMouseEnter( object sender, EventArgs e )
        {
            try
            {
                if( sender is ToolStripDropDownItem _item )
                {
                    _item.Foreground = _theme.WhiteForeground;
                    _item.Background = _theme.SteelBlueBrush;
                    _item.BorderBrush = _theme.SteelBlueBrush;
                }
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [item mouse leave].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/>
        /// instance containing the event data.</param>
        private protected void OnItemMouseLeave( object sender, EventArgs e )
        {
            try
            {
                if( sender is ToolStripDropDownItem _item )
                {
                    _item.Foreground = _theme.Foreground;
                    _item.Background = _theme.ControlInterior;
                    _item.BorderBrush = _theme.ControlInterior;
                }
            }
            catch(Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Fails the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private protected void Fail( Exception ex )
        {
            var _error = new ErrorWindow( ex);
            _error?.SetText();
            _error?.ShowDialog();
        }
    }
}
