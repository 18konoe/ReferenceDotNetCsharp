using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicLibrary
{
    public class Library
    {
        public event EventHandler<TextUpdatedEventArgs> Updated;
        private string privateString = string.Empty;
        public async Task UpdateAsync(string newString)
        {
            privateString = newString;
            Updated?.Invoke(this, new TextUpdatedEventArgs(newString));
        }

        public void Update(string newString)
        {
            privateString = newString;
            Updated?.Invoke(this, new TextUpdatedEventArgs(newString));
            //Task.Run((() => UpdateAsync(newString)));
        }
    }

    public class TextUpdatedEventArgs : EventArgs
    {
        public string Text { get; }

        public TextUpdatedEventArgs(string text)
        {
            Text = text;
        }
    }

    public class ExampleForm : Form
    {
        public ExampleForm() : base()
        {
            this.Text = "Click me";
        }
    }
}
