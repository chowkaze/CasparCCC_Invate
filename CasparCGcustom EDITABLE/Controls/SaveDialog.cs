using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FotbollsVMKlocka.Controls
{
	public partial class SaveDialog : Form
	{
		public SaveDialog()
		{
			InitializeComponent();
		}

		public string StorageName
		{
			get { return this.comboBox.Text; }
		}

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }

	}
}
