using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeachersPet.CustomControls {
    //Taken from https://stackoverflow.com/a/37292579
    public class PlaceHolderTextBox : TextBox {
        bool isPlaceHolder = true;
        string _placeHolderText;
        public string PlaceHolderText
        {
            get { return _placeHolderText; }
            set
            {
                _placeHolderText = value;
                SetPlaceholder();
            }
        }

        public new string Text
        {
            get => isPlaceHolder ? string.Empty : base.Text;
            set => base.Text = value;
        }

        //when the control loses focus, the placeholder is shown
        public void SetPlaceholder()
        {
            if (string.IsNullOrEmpty(base.Text))
            {
                base.Text = PlaceHolderText;
                Foreground = Brushes.Gray;
                FontStyle = FontStyles.Italic;
                isPlaceHolder = true;
            }
        }

        //when the control is focused, the placeholder is removed
        private void RemovePlaceHolder()
        {

            if (isPlaceHolder)
            {
                base.Text = "";
                var windowText = System.Drawing.SystemColors.WindowText;
                var color = System.Windows.Media.Color.FromArgb(windowText.A, windowText.R, windowText.G, windowText.B);
                Foreground = new SolidColorBrush(color);
                FontStyle = FontStyles.Normal;
                isPlaceHolder = false;
            }
        }
        public PlaceHolderTextBox()
        {
            GotFocus += RemovePlaceHolder;
            LostFocus += SetPlaceholder;
        }

        private void SetPlaceholder(object sender, EventArgs e)
        {
            SetPlaceholder();
        }

        private void RemovePlaceHolder(object sender, EventArgs e)
        {
            RemovePlaceHolder();
        }
    }

}