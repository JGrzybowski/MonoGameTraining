using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MonoGameTraining
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
		readonly Game1 _game;

		public GamePage()
        {
            this.InitializeComponent();

			// Create the game.
			var launchArguments = string.Empty;
            _game = MonoGame.Framework.XamlGame<Game1>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
            swapChainPanel.DataContext = _game;
        }

        private void Light1Switch_Click(object sender, RoutedEventArgs e)
        {
            _game.Light1.IsOn = !_game.Light1.IsOn;
        }

        private void Light2Switch_Click(object sender, RoutedEventArgs e)
        {
            _game.Light2.IsOn = !_game.Light2.IsOn;
        }

        private void TerrainSwitch_Click(object sender, RoutedEventArgs e)
        {
            _game.GrassTextureIndex = (_game.GrassTextureIndex == 0) ? 1 : 0;
        }

        private void FogIntensitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_game != null)
                _game.FogIntensity = (float)e.NewValue;
        }

        private void FogStartSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_game != null)
                _game.FogStart = (float)e.NewValue;
        }


        private void FogEndSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_game != null)
                _game.FogEnd= (float)e.NewValue;

        }

        private void TextrueFiltersCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var value = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString();
            if (_game != null)
                _game.TexFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), value);
        }

        private void BiasSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_game != null)
                _game.MipMapLevelBias = (float)e.NewValue;
        }
    }
}
